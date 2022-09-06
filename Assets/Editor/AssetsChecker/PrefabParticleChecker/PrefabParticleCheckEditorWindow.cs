
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

/// <summary>
/// 预制粒子检测,EditorUI界面
/// </summary>
public class PrefabParticleCheckEditorWindow : AssetCheckEditorWindowBase<PrefabParticleAssetInfo>
{
    public const string Title = "预制粒子";

    private void _ShowRuleDes()
    {
        const string s_Des = "检查规则：\n" +
            "1、建议关闭Collision和Trigger\n" +
            "2、建议关闭阴影和光照探针\n" +
            "3、粒子类型是Mesh，则Mesh的R&W必须开启\n" +
            "4、最大粒子建议与Bursts发射粒子数一致\n" +
            "5、Prewarm打开会模拟一次粒子的整个生命周期有可能会造成卡顿，一般建议关闭\n" +
            "6、Renderer关闭时需要把Material置为空\n" +
            "7、设置过Mesh后，又改为Billboard模式，则之前设置的Mesh会冗余\n" +
            "8、建议材质总显示尺寸不要超过1024x1024，计算方式：发射粒子数 * 材质球第一个纹理尺寸 < 1024x1024\n" +
            "9、仅建议：粒子数量小于30\r\n" +
               "仅建议：粒子类型是Mesh，则发射数小于5个\r\n" +
               "仅建议：粒子类型是Mesh，则网格面片小于500";
        AssetsCheckUILogic.ShowRuleDes(s_Des);
    }

    private void _ShowFixAll()
    {
        EditorGUILayout.Space();
        EditorGUILayout.BeginHorizontal();
        GUILayout.FlexibleSpace();

        ShowProblemToggle();

        GUI.color = Color.green;
        if (GUILayout.Button("全部修复", GUILayout.Width(100)))
        {
            PrefabParticleChecker.FixAll(_showInfos, (isCancel)=>
            {
                Reload();
            });
        }
        GUI.color = Color.white;

        AssetsCheckUILogic.ShowCancelTipsBt();

        EditorGUILayout.EndHorizontal();
    }

    protected override string OnGetTitle()
    {
        return Title;
    }

    protected override void OnStartCollectAssetInfo(System.Action<List<PrefabParticleAssetInfo>> finishCB)
    {
        PrefabParticleChecker.CollectAssetInfo(finishCB);
    }

    protected override void OnShowCellButton(PrefabParticleAssetInfo info, Rect rect, bool isError)
    {
        // 检视按钮
        GUILogicHelper.ShowFourCheckBt(rect, info.assetPath, ()=>
        {
            var keys = PrefabParticleChecker.GetErrorObjUniqueKeys(info);
            AssetsCheckUILogic.GoToAndSelectTips(info.assetPath, keys);
        });

        // 修复按钮
        if (info.CanFix())
        {
            GUILogicHelper.ShowFourFixBt(rect, () =>
            {
                info.Fix();

                Reload();
            });
        }
    }

    protected override void OnShowTopInfo()
    {
        // 显示规则信息
        _ShowRuleDes();

        // 显示复选框和全部修复按钮
        _ShowFixAll();
    }

    protected override float OnGetTableViewPosY()
    {
        return 230;
    }

    protected override List<PrefabParticleAssetInfo> OnGetShowInfos()
    {
        return _isFilter ? PrefabParticleChecker.GetErrorAssetInfos(_assetsInfos) : _assetsInfos;
    }
}
