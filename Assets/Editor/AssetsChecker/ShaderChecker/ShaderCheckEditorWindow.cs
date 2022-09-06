
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

public class ShaderCheckEditorWindow : AssetCheckEditorWindowBase<ShaderAssetInfo>
{
    public const string Title = "Shader资源";

    private int _sortIndex = 0;

    private void _ShowRuleDes()
    {
        const string s_Des = "检查规则：\n" +
            "1、支持SRP Batcher\n" +
            "2、建议纹理采样数量小于5张\n" +
            "3、建议变体数量小于33个";
        AssetsCheckUILogic.ShowRuleDes(s_Des);
    }

    private void _SortAction()
    {
        _sortIndex++;
        _sortIndex = _sortIndex > 2 ? 0 : _sortIndex;
        if (_sortIndex == 0)
            _assetsInfos.Sort((a, b) => { return b.textureCount - a.textureCount; });
        else if (_sortIndex == 1)
            _assetsInfos.Sort((a, b) => { return b.variantCount - a.variantCount; });
        else
            _assetsInfos = _assetsInfos.OrderBy((info) => { return info.assetPath; }).ToList();
    }

    private string _GetSortName()
    {
        if (_sortIndex == 0) return "排序：纹理数量";
        else if (_sortIndex == 1) return "排序：变体数量";
        return "排序：文件名";
    }

    private void _ShowToggle()
    {
        EditorGUILayout.Space();
        EditorGUILayout.BeginHorizontal();
        GUILayout.FlexibleSpace();

        // 排序按钮
        var btName = _GetSortName();
        if (GUILayout.Button(btName, GUILayout.Width(100)))
        {
            _SortAction();

            Reload();
        }

        GUILayout.Space(30);

        ShowProblemToggle();

        EditorGUILayout.EndHorizontal();
    }

    protected override string OnGetTitle()
    {
        return Title;
    }

    protected override void OnStartCollectAssetInfo(System.Action<List<ShaderAssetInfo>> finishCB)
    {
        ShaderChecker.CollectAssetInfo(finishCB);
    }

    protected override void OnShowTopInfo()
    {
        // 显示规则信息
        _ShowRuleDes();

        // 只显示问题开关
        _ShowToggle();
    }

    protected override float OnGetTableViewPosY()
    {
        return 110;
    }

    protected override List<ShaderAssetInfo> OnGetShowInfos()
    {
        return _isFilter ? ShaderChecker.GetErrorAssetInfos(_assetsInfos) : _assetsInfos;
    }
}
