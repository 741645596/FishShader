using System.Collections.Generic;
using System.IO;
using EditerUtils;
using UnityEditor;
using UnityEngine;


public class UnreferencedScriptCheckEditorWindow : EditorWindow
{
    private Vector2 _scrollPos;
    private List<UnreferencedResourceInfo> _assetsInfos;

    public void InitAssetInfos(List<UnreferencedResourceInfo> infos)
    {
        _assetsInfos = infos;
        _IsInCS();
    }

    private void OnEnable()
    {
        titleContent = new GUIContent("查找代码内未被引用资源");
    }

    private void _IsInCS()
    {
        var csFiles = DirectoryHelper.GetAllFiles(PathHelper.Game_Assets_Unity_Path, ".cs");
        foreach (var info in _assetsInfos)
        {
            var content = Path.GetFileName(info.assetPath);
            var res = EditerUtils.FileHelper.IsContentInCSFile(content, csFiles);
            info.isScriptReferenced = res;
        }
    }

    //private void _ShowBottomInfo()
    //{
    //    var size = UnreferencedResourceChecker.GetFileSize(_assetsInfos);
    //    AssetsCheckUILogic.ShowBottomInfo(_assetsInfos.Count, size);
    //}

    private void _ShowContentCell(UnreferencedResourceInfo info)
    {
        EditorGUILayout.BeginHorizontal();

        GUI.color = info.isScriptReferenced ? Color.white :  Color.yellow;

        // 显示文件路径
        EditorGUILayout.LabelField(info.assetPath);

        // 显示问题描述
        var des = info.isScriptReferenced ? "" : "代码内查找不到";
        EditorGUILayout.LabelField(des);

        // 检视按钮
        AssetsCheckUILogic.ShowGoToBt(info.assetPath);

        GUI.color = Color.white;

        EditorGUILayout.EndHorizontal();
    }

    private void _ShowContent()
    {
        // 必须赋值才能滚动
        _scrollPos = GUILayout.BeginScrollView(_scrollPos, false, true);

        foreach (var info in _assetsInfos)
        {
            _ShowContentCell(info);
        }

        GUILayout.EndScrollView();
    }

    private void OnGUI()
    {
        if (_assetsInfos == null)
        {
            _assetsInfos = UnreferencedResourceChecker.CollectAssetInfo();
            _IsInCS();
        }

        // 显示内容标题
        AssetsCheckUILogic.ShowContentTitle();

        // 显示内容
        _ShowContent();

        // 显示底部文件信息
        //_ShowBottomInfo();
    }
}
