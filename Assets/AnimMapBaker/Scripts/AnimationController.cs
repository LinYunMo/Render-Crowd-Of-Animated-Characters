using System.Collections.Generic;
using UnityEngine;


public class AnimationController : MonoBehaviour
{
    
    [SerializeField, SetProperty("defaultClip")]
    private string _defaultClip = "";
    public string defaultClip
    {
        get
        {
            return _defaultClip;
        }
        set
        {
            if (!IsAnimationExists(value))
            {
                Debug.LogError("DefaultClip not exist! Please check it!");
                return;
            }
            _defaultClip = value;
        }
    }
    
    [SerializeField, SetProperty("playAnim")]
    private bool _playingAnim = false;
    public bool playAnim
    {
        get {
            return _playingAnim;
        }
        
        set
        {
            _playingAnim = value;
            float uniform = (float)(value ? 1.0 : 0.0);
            if (animMat == null)
            {
                Debug.LogError("Please set material first!");
            }
            animMat.SetFloat(PlayAnimProperty,  uniform);
        }
    }
    
    public List<string> animationClip = new List<string>();
    public List<Texture2D> animationMap = new List<Texture2D>();
    public List<float> animationLength = new List<float>();
    public Material animMat = null;
    
    private static readonly int AnimMapProperty = Shader.PropertyToID("_AnimMap");
    private static readonly int AnimLen = Shader.PropertyToID("_AnimLen");
    private static readonly int PlayAnimProperty = Shader.PropertyToID("_PlayAnim");
    
    private string _currentClip = "";
    private float _currentClipLen = 0.0f;
    private float _currentPlayedTime = 0.0f;
    private bool _currentClipLoop = true;

    public void Play(string name, bool loop = true)
    {
        if (!IsAnimationExists(name))
        {
            if (_defaultClip == null) return;
            name = _defaultClip;
        }
        
        int index = animationClip.IndexOf(name);
        Texture2D clip = (Texture2D)animationMap[index];
        animMat.SetTexture(AnimMapProperty, clip);
        animMat.SetFloat(AnimLen, animationLength[index]);

        playAnim = true;
        
        _currentClipLoop = loop;
        _currentClip = name;
        _currentClipLen = animationLength[index];
        _currentPlayedTime = 0.0f;
    }
    
    public float GetLengthByName(string name)
    {
        if (!IsAnimationExists(name))
        {
            Debug.LogError("Animation not exist! Please check it!");
            return 0.0f;
        }
        int index = animationClip.IndexOf(name);
        return animationLength[index];
    }
    
    private bool IsAnimationExists(string name)
    {
        return animationClip.Contains(name);
    }
    
    void Awake()
    {
        this.animMat = gameObject.GetComponent<MeshRenderer>().material;
    }

    // Update is called once per frame
    void Update()
    {
        if(!_currentClipLoop)
        {
            _currentPlayedTime += Time.deltaTime;
            if (_currentPlayedTime > _currentClipLen)
            {
                if (_defaultClip != null)
                {
                    Play(_defaultClip);
                }
            }
        }
    }
    
}
