
using System.Collections.Generic;
using EditerUtils;
using UnityEditor;
using UnityEditor.IMGUI.Controls;
using UnityEngine;

/// <summary>
/// 图集检测
/// </summary>
public class AtlasCheckEditorWindow : AssetCheckEditorWindowBase<AtlasAssetInfo>
{
    public const string Title = "合图资源";

    private void _ShowRuleDes()
    {
        const string s_Des = "检查规则：\n" +
            "规范：文件夹以_atlas结尾的表示为合图\n" +
            "1、_atlas文件夹下是否存在图集\n" +
            "2、原图的纹理资源Texture Type必须是Sprite(2D and UI)\n" +
            "3、因为合图会对原图二次压缩，所以原图必须是最高纹理格式ASTC 4x4\n" +
            "4、SpriteAtlas纹理压缩格式是astc 4x4 || 5x5 || 6x6\n" +
            "5、Allow Rotation需要打开，否则动态创建可能会倒转\n" +
            "备注：所有的全部修复和一键设置按钮只修改当前面板展示内容";
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
            AtlasChecker.FixAll(_showInfos, (isCancel)=>
            {
                Reload();
            });
        }
        GUI.color = Color.white;

        if (GUILayout.Button("一键设置ASTC", GUILayout.Width(100)))
        {
            AssetsCheckUILogic.ShowASTCPopMenu(format =>
            {
                AtlasChecker.SetAllAstcFormat(_showInfos, format, (isCancel)=>
                {
                    Reload();
                });
            });
        }

        EditorGUILayout.EndHorizontal();
    }

    protected override string OnGetTitle()
    {
        return Title;
    }

    protected override void OnStartCollectAssetInfo(System.Action<List<AtlasAssetInfo>> finishCB)
    {
        AtlasChecker.CollectAssetInfo(finishCB);
    }

    protected override void OnShowCellButton(AtlasAssetInfo info, Rect rect, bool isError)
    {
        // 检视按钮
        GUILogicHelper.ShowFourCheckBt(rect, info.assetPath, () =>
        {
            var path = info.isSpriteAtlasExist ? info.spriteAtlasAssetPath : info.assetPath;
            Selection.activeObject = AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(path);
        });

        // 单独设置ASTC
        GUILogicHelper.ShowFourCustiomBt("设置ASTC", rect, () =>
        {
            AssetsCheckUILogic.ShowASTCPopMenu(format =>
            {
                info.SetAstcFormat(format);
            });
        });

        // 修复按钮
        if (isError)
        {
            GUILogicHelper.ShowFourFixBt(rect, 2, () =>
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
        return 164;
    }

    protected override List<AtlasAssetInfo> OnGetShowInfos()
    {
        return _isFilter ? AtlasChecker.GetErrorAssetInfos(_assetsInfos) : _assetsInfos;
    }
}
