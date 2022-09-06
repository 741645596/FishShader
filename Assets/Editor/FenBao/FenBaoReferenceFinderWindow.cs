using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using UnityEditor;
using UnityEngine;
using UnityEditor.IMGUI.Controls;
using UnityEngine.U2D;
using YamlDotNet.RepresentationModel;
using YamlDotNet.Serialization;

public class FenBaoReferenceFinderWindow : EditorWindow
{
    //依赖模式的key
    const string isDependPrefKey = "ReferenceFinderData_IsDepend";

    //是否需要更新信息状态的key
    const string needUpdateStatePrefKey = "ReferenceFinderData_needUpdateState";

    ReferenceFinderData m_data = new ReferenceFinderData();
    private static bool initializedData = false;

    private bool m_isDepend = false;
    private bool m_needUpdateState = true;

    private bool needUpdateAssetTree = false;

    private bool initializedGUIStyle = false;

    //工具栏按钮样式
    private GUIStyle toolbarButtonGUIStyle;

    //工具栏样式
    private GUIStyle toolbarGUIStyle;

    //选中资源列表
    private List<string> m_selectedAssetGuidList = new List<string>();

    private AssetTreeView m_AssetTreeView;

    [SerializeField] private TreeViewState m_TreeViewState;

    

    class tagMeshInfo
    {
        public string path;
        public int count;
    }

    //打开窗口
    [MenuItem("Tools/分包/资源引用检查")]
    static void OpenWindow()
    {
        FenBaoReferenceFinderWindow window = GetWindow<FenBaoReferenceFinderWindow>();
        window.wantsMouseMove = false;
        window.titleContent = new GUIContent("分包资源检查");
        window.Show();
        window.Focus();
    }

    //初始化GUIStyle
    void InitGUIStyleIfNeeded()
    {
        if (!initializedGUIStyle)
        {
            toolbarButtonGUIStyle = new GUIStyle("ToolbarButton");
            toolbarGUIStyle = new GUIStyle("Toolbar");
            initializedGUIStyle = true;
        }
    }

    void SelectInvalidDependcy()
    {
        m_isDepend = true;
        m_selectedAssetGuidList.Clear();
        m_selectedAssetGuidList.AddRange(DependInfo.GetInvalidDependInfo(m_data));
        m_selectedAssetGuidList = AssetsCheckerUtils.SortByName(m_selectedAssetGuidList);
        needUpdateAssetTree = true;
    }

    void SelectInvalidFishDependcy()
    {
        m_selectedAssetGuidList.Clear();
        m_selectedAssetGuidList.AddRange(DependInfo.GetInvalidFishDependInfo(m_data));
        m_selectedAssetGuidList = AssetsCheckerUtils.SortByName(m_selectedAssetGuidList);
        m_isDepend = true;
        needUpdateAssetTree = true;
    }


    private AssetViewItem mapToTvRoot(Dictionary<string, List<string>> selectedAssetGuid)
    {
        updatedAssetSet.Clear();
        int elementCount = 0;
        var root = new AssetViewItem { id = elementCount, depth = -1, displayName = "Root", data = null };
        int depth = 0;
        var stack = new Stack<string>();

        foreach (var itm in selectedAssetGuid.OrderByDescending(x => x.Value.Count()))
        {
            string hash = itm.Key;
            List<string> textureGUIDList = itm.Value;
            if (textureGUIDList.Count == 1)
                continue;

            var hashRefCount = 0;
            for (int i = 0; i < textureGUIDList.Count; i++)
            {
                var childGuid = textureGUIDList[i];
                if (m_data.m_assetDict[childGuid].references.Count == 0)
                    continue;
                hashRefCount++;
            }

            if (hashRefCount == 0)
                continue;

            elementCount++;

            var hashRoot = new AssetViewItem
            {
                type = TVItemType.HASH,
                hash = hash,
                id = elementCount,
                depth = depth,
                displayName = textureGUIDList.Count + "",
                data = null
            };
            root.AddChild(hashRoot);

            for (int i = 0; i < textureGUIDList.Count; i++)
            {
                var childGuid = textureGUIDList[i];
                var child = CreateTree(childGuid, ref elementCount, depth + 1, stack);
                if (child != null)
                    hashRoot.AddChild(child);
            }
        }

        updatedAssetSet.Clear();
        return root;
    }

    //更新选中资源列表
    private void UpdateSelectedAssets()
    {
        m_selectedAssetGuidList.Clear();
        foreach (var obj in Selection.objects)
        {
            string path = AssetDatabase.GetAssetPath(obj);

            if (Directory.Exists(path))
            {
                //如果是文件夹
                string[] folder = new string[] { path };
                //将文件夹下所有资源作为选择资源
                string[] guids = AssetDatabase.FindAssets(null, folder);
                foreach (var guid in guids)
                {
                    if (!m_selectedAssetGuidList.Contains(guid) &&
                        !Directory.Exists(AssetDatabase.GUIDToAssetPath(guid)))
                    {
                        m_selectedAssetGuidList.Add(guid);
                    }
                }
            }
            else
            {
                //如果是文件资源
                string guid = AssetDatabase.AssetPathToGUID(path);
                m_selectedAssetGuidList.Add(guid);
            }
        }

        needUpdateAssetTree = true;
    }

    //通过选中资源列表更新TreeView
    private void UpdateAssetTree()
    {
        if (needUpdateAssetTree && m_selectedAssetGuidList.Count != 0)
        {
            var root = SelectedAssetGuidToRootItem(m_selectedAssetGuidList);
            updateTreeView(root);
            needUpdateAssetTree = false;
        }
    }

    private void updateTreeView(AssetViewItem root)
    {
        if (m_AssetTreeView == null)
        {
            //初始化TreeView
            if (m_TreeViewState == null)
                m_TreeViewState = new TreeViewState();
            var headerState = AssetTreeView.CreateDefaultMultiColumnHeaderState(position.width);
            var multiColumnHeader = new MultiColumnHeader(headerState);
            m_AssetTreeView = new AssetTreeView(m_TreeViewState, multiColumnHeader);
        }

        m_AssetTreeView.assetRoot = root;
        m_AssetTreeView.CollapseAll();
        m_AssetTreeView.Reload();
    }

    private void OnGUI()
    {
        InitGUIStyleIfNeeded();
        DrawOptionBar();
        UpdateAssetTree();
        if (m_AssetTreeView != null)
        {
            //绘制Treeview
            m_AssetTreeView.OnGUI(new Rect(0, toolbarGUIStyle.fixedHeight, position.width,
                position.height - toolbarGUIStyle.fixedHeight));
        }
    }

    //绘制上条
    public void DrawOptionBar()
    {
        EditorGUILayout.BeginHorizontal(toolbarGUIStyle);

        if (GUILayout.Button("分包-非法引用的资源", toolbarButtonGUIStyle))
        {
            SelectInvalidDependcy();
        }

        if (GUILayout.Button("分包-非法的鱼的资源引用", toolbarButtonGUIStyle))
        {
            SelectInvalidFishDependcy();
        }

        GUILayout.FlexibleSpace();

        //扩展
        if (GUILayout.Button("Expand", toolbarButtonGUIStyle))
        {
            if (m_AssetTreeView != null) m_AssetTreeView.ExpandAll();
        }

        //折叠
        if (GUILayout.Button("Collapse", toolbarButtonGUIStyle))
        {
            if (m_AssetTreeView != null) m_AssetTreeView.CollapseAll();
        }

        EditorGUILayout.EndHorizontal();
    }

    private void OnModelSelect()
    {
        needUpdateAssetTree = true;
    }


    //生成root相关
    private HashSet<string> updatedAssetSet = new HashSet<string>();

    //通过选择资源列表生成TreeView的根节点
    private AssetViewItem SelectedAssetGuidToRootItem(List<string> selectedAssetGuid)
    {
        updatedAssetSet.Clear();
        int elementCount = 0;
        var root = new AssetViewItem { id = elementCount, depth = -1, displayName = "Root", data = null };
        int depth = 0;
        var stack = new Stack<string>();
        foreach (var childGuid in selectedAssetGuid)
        {
            var child = CreateTree(childGuid, ref elementCount, depth, stack);
            if (child != null)
                root.AddChild(child);
        }

        updatedAssetSet.Clear();
        return root;
    }

    //通过每个节点的数据生成子节点
    private AssetViewItem CreateTree(string guid, ref int elementCount, int _depth, Stack<string> stack)
    {
        if (stack.Contains(guid))
            return null;

        AssetViewItem root = null;

        stack.Push(guid);
        if (m_needUpdateState && !updatedAssetSet.Contains(guid))
        {
            m_data.UpdateAssetState(guid);
            updatedAssetSet.Add(guid);
        }

        ++elementCount;
        if (m_data.m_assetDict.ContainsKey(guid))
        {
            var referenceData = m_data.m_assetDict[guid];
            root = new AssetViewItem
            { id = elementCount, displayName = referenceData.name, data = referenceData, depth = _depth };
            var childGuids = m_isDepend ? referenceData.dependencies : referenceData.references;
            foreach (var childGuid in childGuids)
            {
                var child = CreateTree(childGuid, ref elementCount, _depth + 1, stack);
                if (child != null)
                    root.AddChild(child);
            }
        }
        else
        {
            var guidToAssetPath = AssetDatabase.GUIDToAssetPath(guid);
            Debug.Log($"GUID not in assetDict{guidToAssetPath}");
        }

        stack.Pop();
        return root;
    }
}
