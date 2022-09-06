using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
class AnimatorEffectNode
{
    public GameObject go;
    public string name;
    public float time;
    [HideInInspector]
    public bool playing;
    [HideInInspector]
    public float elapsed;


}

[RequireComponent(typeof(Animator))]
class AnimatorEffect : MonoBehaviour
{
    public AnimatorEffectNode[] Actions;
    float m_CurrentTime;
    Animator m_Animator;

    void Start()
    {
        m_Animator = GetComponent<Animator>();
        //Debug.Log($"Animator {m_Animator == null}");
    }

    void Update()
    {
        if (m_Animator == null) return;
        float dt = Time.deltaTime;
        var stateInfo = m_Animator.GetCurrentAnimatorStateInfo(0);
        for (int i = 0; i < Actions.Length; i++)
        {
            var node = Actions[i];
            if (node.go == null) continue;
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
