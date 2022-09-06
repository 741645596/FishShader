using System.Collections.Generic;
using UnityEngine;


public class PrefabParticleLogic
{
    /// <summary>
    /// 获取需要被置空材质球的Renderer组件集合：
    /// 如果粒子中的Renderer组件被关闭，并且Renderer组件中的renderMode不为空，并且材质球不为空
    /// 则需要设置材质球为空
    /// 我们将这些组件收集返回
    /// </summary>
    /// <param name="tra"> 从某个父节点开始遍历所有节点 </param>
    /// <returns></returns>
    public static List<ParticleSystemRenderer> GetNeedCloseRenderers(Transform tra)
    {
        var psArr = tra.GetComponentsInChildren<ParticleSystem>(true);
        return GetNeedCloseRenderers(psArr);
    }

    public static List<GameObject> GetNeedCloseRenderersObjs(ParticleSystem[] psArr)
    {
        var list = new List<GameObject>();
        var renders = GetNeedCloseRenderers(psArr);
        foreach (var render in renders)
        {
            list.Add(render.gameObject);
        }
        return list;
    }

    public static List<ParticleSystemRenderer> GetNeedCloseRenderers(ParticleSystem[] psArr)
    {
        List<ParticleSystemRenderer> particleSystemRenderers = new List<ParticleSystemRenderer>();
        foreach (var child in psArr)
        {
            ParticleSystemRenderer renderComp = child.GetComponent<ParticleSystemRenderer>();
            if (renderComp.enabled)
            {
                continue;
            }

            if (renderComp.renderMode != ParticleSystemRenderMode.None ||
                 renderComp.sharedMaterial != null)
            {
                particleSystemRenderers.Add(renderComp);
            }
        }
        return particleSystemRenderers;
    }

    /// <summary>
    /// 获取冗余Mesh的粒子组件
    /// </summary>
    /// <param name="psArr"></param>
    /// <returns></returns>
    public static List<ParticleSystemRenderer> GetRedundancyMeshParticleSystems(ParticleSystem[] psArr)
    {
        var particleSystemRenderers = new List<ParticleSystemRenderer>();
        foreach (var child in psArr)
        {
            // 非Mesh模式，但是Mesh又有值，则冗余
            var renderComp = child.GetComponent<ParticleSystemRenderer>();
            if (renderComp.renderMode != ParticleSystemRenderMode.Mesh &&
                renderComp.mesh != null)
            {
                particleSystemRenderers.Add(renderComp);
            }
        }
        return particleSystemRenderers;
    }

    public static List<ParticleSystemRenderer> GetRedundancyMeshRenderers(Transform tra)
    {
        var psArr = tra.GetComponentsInChildren<ParticleSystem>(true);
        return GetRedundancyMeshParticleSystems(psArr);
    }

    public static List<GameObject> GetRedundancyMeshRenderersObjs(ParticleSystem[] psArr)
    {
        var list = new List<GameObject>();
        var renders = GetRedundancyMeshParticleSystems(psArr);
        foreach (var render in renders)
        {
            list.Add(render.gameObject);
        }
        return list;
    }

    /// <summary>
    /// 获取一些打开了Prewarm的粒子组件集合：
    /// </summary>
    /// <param name="tra"> 从某个父节点开始遍历所有节点 </param>
    /// <returns></returns>
    public static List<ParticleSystem> GetOpenPrewarmParticleSystems(Transform tra)
    {
        var psArr = tra.GetComponentsInChildren<ParticleSystem>(true);
        return GetOpenPrewarmParticleSystems(psArr);
    }

    public static List<GameObject> GetOpenPrewarmParticleSystemsObjs(ParticleSystem[] psArr)
    {
        var list = new List<GameObject>();
        var renders = GetOpenPrewarmParticleSystems(psArr);
        foreach (var render in renders)
        {
            list.Add(render.gameObject);
        }
        return list;
    }

    public static List<ParticleSystem> GetOpenPrewarmParticleSystems(ParticleSystem[] psArr)
    {
        List<ParticleSystem> particleSystems = new List<ParticleSystem>();
        foreach (var child in psArr)
        {
            ParticleSystem particleSystem = child.GetComponent<ParticleSystem>();
            bool isFix = child.main.prewarm; // 如果Prewarm是打开的,就记录一下，待修复状态
            if (isFix)
            {
                particleSystems.Add(particleSystem);
            }
        }

        return particleSystems;
    }

}
