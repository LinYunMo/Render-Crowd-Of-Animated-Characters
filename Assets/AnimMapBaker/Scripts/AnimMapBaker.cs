/*
 * Created by Arthur Wang
 * 用来烘焙动作贴图。烘焙对象使用Animation组件，并且在导入时设置Rig为Legacy
 */

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.Linq;
using System.IO;

/// <summary>
/// 保存需要烘焙的动画的相关数据
/// </summary>
public struct AnimData
{
    #region FIELDS

    private const int MaxTextureSize = 2048;
    
    private int _vertexCount;
    private int _mapWidth;
    private readonly List<AnimationState> _animClips;
    private string _name;

    private  Animation _animation;
    private SkinnedMeshRenderer _skin;
    
    private int _maxRow;

    public List<AnimationState> AnimationClips => _animClips;
    public int MapWidth => _mapWidth;
    public string Name => _name;
    public string DefaultClip => _animation.clip.name;
    public int MaxRow => _maxRow;

    #endregion

    public AnimData(Animation anim, SkinnedMeshRenderer smr, string goName)
    {
        _vertexCount = smr.sharedMesh.vertexCount;
        _mapWidth = Mathf.NextPowerOfTwo(_vertexCount);
        _maxRow = 1;
        if (_mapWidth > MaxTextureSize) // 限制最大宽度
        {
            _maxRow = Mathf.CeilToInt((_mapWidth / MaxTextureSize)); // 宽度系数，决定几行才能放得下一帧
            _mapWidth = MaxTextureSize;
        }
        _animClips = new List<AnimationState>(anim.Cast<AnimationState>());
        _animation = anim;
        _skin = smr;
        _name = goName;
    }

    #region METHODS

    public void AnimationPlay(string animName)
    {
        _animation.Play(animName);
    }

    public void SampleAnimAndBakeMesh(ref Mesh m)
    {
        SampleAnim();
        BakeMesh(ref m);
    }

    private void SampleAnim()
    {
        if (_animation == null)
        {
            Debug.LogError("animation is null!!");
            return;
        }

        _animation.Sample();
    }

    private void BakeMesh(ref Mesh m)
    {
        if (_skin == null)
        {
            Debug.LogError("skin is null!!");
            return;
        }

        _skin.BakeMesh(m);
    }


    #endregion

}

/// <summary>
/// 烘焙后的数据
/// </summary>
public struct BakedData
{
    #region FIELDS

    private readonly string _name;
    private readonly float _animLen;
    private readonly byte[] _rawAnimMap;
    private readonly int _animMapWidth;
    private readonly int _animMapHeight;
    private readonly string _clipName;
    private readonly string _defaultClip;
    private readonly int _maxRow;

    #endregion

    public BakedData(string name, float animLen, Texture2D animMap, string clipName, int maxRow, string defaultClip = "")
    {
        _name = name;
        _animLen = animLen;
        _animMapHeight = animMap.height;
        _animMapWidth = animMap.width;
        _rawAnimMap = animMap.GetRawTextureData();
        _clipName = clipName;
        _maxRow = maxRow;
        _defaultClip = defaultClip;
    }

    public int AnimMapWidth => _animMapWidth;

    public string Name => _name;

    public float AnimLen => _animLen;

    public byte[] RawAnimMap => _rawAnimMap;

    public int AnimMapHeight => _animMapHeight;
    
    public string ClipName => _clipName;
    
    public string DefaultClip => _defaultClip;
    
    public int MaxRow => _maxRow;
}

/// <summary>
/// 烘焙器
/// </summary>
public class AnimMapBaker{

    #region FIELDS

    private AnimData? _animData = null;
    private Mesh _bakedMesh;
    private readonly List<Vector3> _vertices = new List<Vector3>();
    private readonly List<BakedData> _bakedDataList = new List<BakedData>();

    #endregion

    #region METHODS

    public void SetAnimData(GameObject go)
    {
        if(go == null)
        {
            Debug.LogError("go is null!!");
            return;
        }

        var anim = go.GetComponent<Animation>();
        var smr = go.GetComponentInChildren<SkinnedMeshRenderer>();

        if(anim == null || smr == null)
        {
            Debug.LogError("anim or smr is null!!");
            return;
        }
        _bakedMesh = new Mesh();
        _animData = new AnimData(anim, smr, go.name);
    }

    public List<BakedData> Bake()
    {
        _bakedDataList.Clear();
        if(_animData == null)
        {
            Debug.LogError("bake data is null!!");
            return _bakedDataList;
        }

        //每一个动作都生成一个动作图
        foreach (var t in _animData.Value.AnimationClips)
        {
            if(!t.clip.legacy)
            {
                Debug.LogError(string.Format($"{t.clip.name} is not legacy!!"));
                continue;
            }
            BakePerAnimClip(t);
        }

        return _bakedDataList;
    }

    private void BakePerAnimClip(AnimationState curAnim)
    {
        var curClipFrame = 0;
        float sampleTime = 0;
        float perFrameTime = 0;

        // 动作帧率 * 秒数 = 总帧数  取了一个最接近的2的幂的值（对齐）
        curClipFrame = Mathf.ClosestPowerOfTwo((int)(curAnim.clip.frameRate * curAnim.length));
        perFrameTime = curAnim.length / curClipFrame; // 每一帧的时间

        // 图像大小及 Layout，CPU端与GPU端的数据格式要一致
        // TODO
        int maxRow = _animData.Value.MaxRow;
        int maxVertex = _animData.Value.MapWidth;
        var animMap = new Texture2D(_animData.Value.MapWidth, curClipFrame * maxRow, TextureFormat.RGBAHalf, true);
        animMap.name = string.Format($"{_animData.Value.Name}_{curAnim.name}.animMap");
        _animData.Value.AnimationPlay(curAnim.name);
        
        // 采样并烘焙
        // 调整算法，需要考虑到顶点数目过多的情况
        // 根据 maxWidthRatio，决定一帧需要几行
        for (var i = 0; i < curClipFrame; i++)
        {
            curAnim.time = sampleTime;

            _animData.Value.SampleAnimAndBakeMesh(ref _bakedMesh);
            
            // 顶点数据写入纹理
            for(var j = 0; j < _bakedMesh.vertexCount; j++)
            {
                var vertex = _bakedMesh.vertices[j];
                int row = j;
                int col = i * maxRow;
                if (j >= maxVertex)
                {
                    row = j % maxVertex;
                    col = i * maxRow + 1;
                }
                animMap.SetPixel(row, col, new Color(vertex.x, vertex.y, vertex.z));
            }

            sampleTime += perFrameTime;
        }
        animMap.Apply();
        
        _bakedDataList.Add(new BakedData(animMap.name, curAnim.clip.length, animMap, curAnim.clip.name, maxRow, _animData.Value.DefaultClip));
    }

    #endregion

}
