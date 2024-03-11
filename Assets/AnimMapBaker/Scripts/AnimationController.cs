using System.Collections.Generic;
using JetBrains.Annotations;
using UnityEngine;

public class AnimationController : MonoBehaviour
{

    [SerializeField]
    public string defaultClip = "";// 何处赋值？
    [SerializeField]
    public List<string> animationClip = new List<string>();
    [SerializeField]
    public List<Texture2D> animationMap = new List<Texture2D>();
    [SerializeField] 
    public List<float> animationLength = new List<float>();
    
    [SerializeField]
    private Material animMat = null;
    
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

    public void Play([CanBeNull] string name)
    {
        if (!IsAnimationExists(name))
        {
            if (defaultClip == null) return;
            name = defaultClip;
        }

        int index = animationClip.IndexOf(name);
        Texture2D clip = (Texture2D)animationMap[index];
        animMat.SetTexture(AnimMapProperty, clip);
        animMat.SetFloat(AnimLen, animationLength[index]);

        playAnim = true;
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
        
    }
    
}
