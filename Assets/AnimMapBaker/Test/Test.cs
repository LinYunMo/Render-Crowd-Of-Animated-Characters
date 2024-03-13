using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Test : MonoBehaviour
{
   
    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }

    public AnimationController ani = null;

    private string[] animName = new string[] {"Alman_attack", "Alman_died", "Alman_idel", "Alman_run", "Alman_win"};

    private int animNun = 5;
    private int animNow = 0;
    public void switchAni()
    {
        animNow++;
        if (animNow == animNun) animNow = 0;
        ani.Play(animName[animNow], false);
    }
}
