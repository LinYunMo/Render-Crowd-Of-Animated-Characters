using System.Collections.Generic;
using JetBrains.Annotations;
using UnityEngine;


public class AnimationController : MonoBehaviour
{
    
    [SerializeField]
    public List<string> animationClip = new List<string>();
    [SerializeField]
    public List<Texture2D> animationMap = new List<Texture2D>();
    [SerializeField] 
    public List<float> animationLength = new List<float>();
    
    [SerializeField]
    public Material animMat = null;
    
    [SerializeField]
    private string _defaultClip = "";
    public string defaultClipName
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
    
    private static readonly int AnimMapProperty = Shader.PropertyToID("_AnimMap");
    private static readonly int AnimLen = Shader.PropertyToID("_AnimLen");
    private static readonly int PlayAnimProperty = Shader.PropertyToID("_PlayAnim");
    
    private float _defaultClipLen = 0.0f;
    
    private string _currentClip = "";
    private float _currentClipLen = 0.0f;
    private float _currentPlayedTime = 0.0f;
    private bool _currentClipLoop = true;

    public void Play([CanBeNull] string name, bool loop = true)
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
    
    private bool IsAnimationExists(string name)
    {
        return animationClip.Contains(name);
    }
    
    void Awake()
    {
        this.animMat = gameObject.GetComponent<MeshRenderer>().material;
    }

    // Start is called before the first frame update
    void Start()
    {
        
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
