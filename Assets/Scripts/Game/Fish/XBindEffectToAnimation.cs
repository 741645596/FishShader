using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
class XBindEffectToAnimationNode
{
    public GameObject go;
    public string name;
    public float time;
    [HideInInspector]
    public bool playing;
    [HideInInspector]
    public float elapsed;


}

class XBindEffectToAnimation : MonoBehaviour
{
    public XBindEffectToAnimationNode[] Actions;
    float m_CurrentTime;
    Animator m_Animator;

    public void SetAnimator(Animator animator)
    {
        m_Animator = animator;
    }

    public void UpdateNode(float dt)
    {
        if (m_Animator == null) return;
        var stateInfo = m_Animator.GetCurrentAnimatorStateInfo(0);
        for (int i = 0; i < Actions.Length; i++)
        {
            var node = Actions[i];
            if (stateInfo.IsName(node.name))
            {
                float t = stateInfo.normalizedTime * stateInfo.length;
                if (t < node.elapsed)
                {
                    //LogUtils.V($"{node.elapsed} {t}");
                    node.elapsed = t;
                    node.playing = false;
                    node.go.SetActive(false);
                }
                else
                {
                    node.elapsed = t;
                    if (!node.playing)
                    {
                        if (t >= node.time)
                        {
                            node.playing = true;
                            node.go.SetActive(true);
                            SyncPaticles(node.go, t - node.time);
                        }
                    }
                }
            }
            else if (node.playing)
            {
                node.elapsed = 0;
                node.playing = false;
                node.go.SetActive(false);
            }
        }
    }

    void SyncPaticles(GameObject go, float time)
    {
        //LogUtils.V(time);
        var particles = go.GetComponents<ParticleSystem>();
        for (int i = 0; i < particles.Length; i++)
        {
            particles[i].Simulate(time);
        }
    }
}
