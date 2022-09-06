
using System.Collections.Generic;
using EditerUtils;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;

/// <summary>
/// 预制粒子专有属性
/// </summary>
public class PrefabParticleAssetInfo : AssetInfoBase
{
    // 是否打开投射阴影
    public bool isOpenCastShadows;

    // 是否打开接收阴影
    public bool isOpenReceiveShadows;

    // 是否打开光照探针
    public bool isOpenLightProbes;

    // 是否打开反射探针
    public bool isOpenReflectionProbes;

    // 是否修复默认1000个最大粒子数
    public bool isDefaultMaxParticles;

    // 是否需要将材质球关闭
    public bool isNeedSetMatNull;

    // 是否关闭了Prewarm
    public bool isOpenPrewarm;

    // 是否关闭了Collision
    public bool isOpenCollision;

    // 是否关闭了Trigger
    public bool isOpenTrigger;

    // 是否超过30个最大粒子数
    public bool isOver30MaxParticles;

    // 如果粒子类型是Mesh，是否超过5个粒子发射数
    public bool isOverMeshBurstsCount;

    // 是否需要开启读写
    public bool isNeedRW;

    // 是否超过500面数
    public bool isOverTrianglesCount;

    // 是否所有预制中所有第一个材质球的主贴图超过
    public bool isOverMainTextureSize;

    // 是否冗余Mesh引用
    public bool isRedundancyMesh;

    // 所有预制中所有第一个材质球的主贴图宽高
    public Vector2 mainTextureSize;

    public PrefabParticleAssetInfo()
    {
        isOpenCastShadows = false;
        isOpenReceiveShadows = false; 
        isOpenLightProbes = false; 
        isOpenReflectionProbes = false; 
        isDefaultMaxParticles = false; 
        isNeedSetMatNull = false; 
        isOpenPrewarm = false; 
        isOpenCollision = false; 
        isOpenTrigger = false; 
        isOver30MaxParticles = false;
        isOverMeshBurstsCount = false;
        isNeedRW = false; 
        isOverTrianglesCount = false;
        isOverMainTextureSize = false;
    }

    public override bool CanFix()
    {
        // 碰撞器
        if (isOpenCollision || isOpenTrigger) return true;

        // 最大粒子数
        if (isDefaultMaxParticles) return true;

        // R&W
        if (isNeedRW) return true;

        // 开启Prewarnm
        if (isOpenPrewarm) return true;

        // 需要关闭Renderer
        if (isNeedSetMatNull) return true;

        // 冗余Mesh引用
        if (isRedundancyMesh) return true;

        // 需要关闭阴影/探针
        if (isOpenCastShadows ||
            isOpenReceiveShadows ||
            isOpenLightProbes ||
            isOpenReflectionProbes)
        {
            return true;
        }
        return false;
    }

    public override void Fix()
    {
        FixNotRrefresh();
        AssetDatabase.Refresh();
    }

    public void FixNotRrefresh()
    {
        var obj = AssetDatabase.LoadAssetAtPath<GameObject>(assetPath);
        var psArr = obj.GetComponentsInChildren<ParticleSystem>(true);

        // 碰撞器
        if (isOpenCollision || isOpenTrigger)
        {
            _FixCollisionAndTrigger(psArr);
            isOpenCollision = false;
            isOpenTrigger = false;
        }

        // 最大粒子数
        if (isDefaultMaxParticles)
        {
            _FixDefaultMaxParticles(psArr);
            isDefaultMaxParticles = false;
        }

        // R&W
        if (isNeedRW)
        {
            _FixRW(psArr);
            isNeedRW = false;
        }

        // 开启Prewarnm
        if (isOpenPrewarm)
        {
            _FixPrewarn(psArr);
            isOpenPrewarm = false;
        }

        // 需要关闭Renderer
        if (isNeedSetMatNull)
        {
            _FixNeedSetMatNull(psArr);
            isNeedSetMatNull = false;
        }

        // 需要关闭阴影/探针
        if (isOpenCastShadows ||
            isOpenReceiveShadows ||
            isOpenLightProbes ||
            isOpenReflectionProbes)
        {
            _FixShadowsAndProbes(psArr);
            isOpenCastShadows = false;
            isOpenReceiveShadows = false;
            isOpenLightProbes = false;
            isOpenReflectionProbes = false;
        }

        if (isRedundancyMesh)
        {
            _FixRedundancyMesh(psArr);
            isRedundancyMesh = false;
        }

        EditorUtility.SetDirty(obj);
        PrefabUtility.SavePrefabAsset(obj.gameObject);
    }

    public override string GetErrorDes()
    {
        var desArr = new List<string>();
        if (isOpenCollision || isOpenTrigger) desArr.Add("Collision和Trigger开启");
        if (isOpenCastShadows || isOpenReceiveShadows) desArr.Add("阴影开启");
        if (isOpenLightProbes || isOpenReflectionProbes) desArr.Add("光照探针开启");
        if (isNeedRW) desArr.Add("粒子Mesh关闭RW");
        if (isDefaultMaxParticles) desArr.Add("发射粒子数与Max不一致");

        if (isOpenPrewarm) desArr.Add("Prewarm开启");
        if (isNeedSetMatNull) desArr.Add("Renderer关闭Material未置空");
        if (isOverMainTextureSize) desArr.Add("总尺寸超过1024x1024");
        if (isOver30MaxParticles) desArr.Add("粒子数超过30");
        if (isOverMeshBurstsCount) desArr.Add("网格发射数超过5个");
        if (isOverTrianglesCount) desArr.Add("网格面数超过500");
        if (isRedundancyMesh) desArr.Add("Mesh引用冗余");

        return string.Join("；", desArr);
    }

    public override bool IsError()
    {
        if (isDefaultMaxParticles) return true;
        if (isOver30MaxParticles) return true;
        if (isOpenPrewarm) return true;
        if (isOpenCollision) return true;
        if (isOpenTrigger) return true;
        if (isNeedSetMatNull) return true;
        if (isOpenCastShadows) return true;
        if (isOpenReceiveShadows) return true;

        if (isOpenLightProbes) return true;
        if (isOpenReflectionProbes) return true;
        if (isNeedRW) return true;
        if (isOverMainTextureSize) return true;
        if (isOverTrianglesCount) return true;
        if (isOverMeshBurstsCount) return true;
        if (isRedundancyMesh) return true;

        return false;
    }

    private static void _FixCollisionAndTrigger(ParticleSystem[] psArr)
    {
        foreach (var child in psArr)
        {
            ParticleSystem.CollisionModule collisionModule = child.collision;
            collisionModule.enabled = false;

            ParticleSystem.TriggerModule triggerModule = child.trigger;
            triggerModule.enabled = false;

            EditorUtility.SetDirty(child);
        }
    }

    private static void _FixDefaultMaxParticles(ParticleSystem[] psArr)
    {
        foreach (var child in psArr)
        {
            if (child.main.maxParticles != PrefabParticleChecker.Max_Particles_Default_Count)
            {
                continue;
            }

            // 是否开启发射器
            ParticleSystem.MainModule mainModule = child.main;
            if (child.emission.enabled)
            {
                // 如果没设置burst数量就给最大粒子1个
                if (child.emission.burstCount == 0)
                {
                    mainModule.maxParticles = PrefabParticleChecker.Max_Particles_Count;
                }
                else
                {
                    var count = PrefabParticleChecker.GetBurstCount(child);
                    count = count == 0 ? PrefabParticleChecker.Max_Particles_Count : count;  // 给个最大默认值防止修复错了
                    mainModule.maxParticles = count; // 就给最大粒子=burst数量
                }
            }
            else
            {
                mainModule.maxParticles = 0; // 没开发射器，最大粒子数为0
            }

            EditorUtility.SetDirty(child); //【保存】覆盖预制状态
        }
    }

    private static void _FixRW(ParticleSystem[] psArr)
    {
        foreach (var child in psArr)
        {
            ParticleSystemRenderer renderComp = child.GetComponent<ParticleSystemRenderer>();
            var modelImporter = MeshHelper.GetModelImporter(renderComp);
            if (modelImporter != null)
            {
                modelImporter.isReadable = true;
                EditorUtility.SetDirty(child);
            }
        }
    }

    private static void _FixPrewarn(ParticleSystem[] psArr)
    {
        // 获得需要被关闭Prewarm的粒子组件
        List<ParticleSystem> particleSystems = PrefabParticleLogic.GetOpenPrewarmParticleSystems(psArr);
        foreach (var child in particleSystems)
        {
            ParticleSystem.MainModule mainModule = child.main;
            mainModule.prewarm = false;

            EditorUtility.SetDirty(child);
        }
    }

    private static void _FixNeedSetMatNull(ParticleSystem[] psArr)
    {
        List<ParticleSystemRenderer> needFixComps = PrefabParticleLogic.GetNeedCloseRenderers(psArr);
        foreach (var renderComp in needFixComps)
        {
            renderComp.renderMode = ParticleSystemRenderMode.None;
            renderComp.sharedMaterial = null;

            EditorUtility.SetDirty(renderComp);
        }
    }

    private static void _FixShadowsAndProbes(ParticleSystem[] psArr)
    {
        foreach (var child in psArr)
        {
            Renderer renderComp = child.GetComponent<Renderer>();
            renderComp.shadowCastingMode = ShadowCastingMode.Off;    // 关闭投射阴影
            renderComp.receiveShadows = false;                    // 关闭接收阴影打开
            renderComp.lightProbeUsage = LightProbeUsage.Off;      // 关闭光照探针
            renderComp.reflectionProbeUsage = ReflectionProbeUsage.Off; // 关闭反射探针
            EditorUtility.SetDirty(child); //【保存】覆盖预制状态
        }
    }

    private static void _FixRedundancyMesh(ParticleSystem[] psArr)
    {
        var needFixComps = PrefabParticleLogic.GetRedundancyMeshParticleSystems(psArr);
        foreach (var renderComp in needFixComps)
        {
            renderComp.mesh = null;
            EditorUtility.SetDirty(renderComp);
        }
    }
}
