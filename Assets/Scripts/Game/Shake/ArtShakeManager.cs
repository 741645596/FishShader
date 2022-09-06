using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Timeline;
using UnityEngine.Playables;

[RequireComponent(typeof(Animator))]
public class ArtShakeManager : MonoBehaviour
{
    public string name;

    Animator animator;

    private void Start()
    {
        animator = GetComponent<Animator>();
        ShakeUtils.AddArtShakeManager(this);
    }

    private void OnDestroy()
    {
        ShakeUtils.RemoveArtShakeManager(this);
    }

    public void PlayShake(int level)
    {
        string trigger = $"level{level}";
        //LogUtils.I($"PlayShake {trigger}");
        if (animator != null)
        {
            animator.SetTrigger(trigger);
        }
    }


}