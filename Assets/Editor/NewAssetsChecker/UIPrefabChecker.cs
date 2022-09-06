using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEditor.IMGUI.Controls;
using UnityEngine.UI;


namespace UIPrefabChecker
{

    public class UIPrefabCheckerWindow : EditorWindow
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

        private SpriteAtlasAssetTreeView m_AssetTreeView;

        [SerializeField] private TreeViewState m_TreeViewState;


        //打开窗口
        [MenuItem("资源检查/UI预制体")]
        static void OpenWindow()
        {
            UIPrefabCheckerWindow window = GetWindow<UIPrefabCheckerWindow>();
            window.wantsMouseMove = false;
            window.titleContent = new GUIContent("UI预制体资源检查");
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

        void SelectAssets(bool bSystemFontText)
        {
            m_isDepend = true;
            m_selectedAssetGuidList.Clear();
            var data = m_data;
            data.CollectDependenciesInfo();

            foreach (var key in data.m_assetDict)
            {
                string path = key.Value.path;
                int idx = path.IndexOf(AssetsCheckerUtils.CheckPath);
                if (idx < 0)
                {
                    continue;
                }
                if (!path.EndsWith(".prefab"))
                {
                    continue;
                }
                
                var gameObject = AssetDatabase.LoadAssetAtPath<GameObject>(path);
                if (gameObject == null)
                {
                    continue;
                }
                if (gameObject.GetComponent<RectTransform>() != null)
                {
                    ///Debug.Log(path);
                    if (bSystemFontText)
                    {
                        bool exist = false;
                        var array = gameObject.GetComponentsInChildren<Text>(true);
                        for (int i = 0; i < array.Length; i++)
                        {
                            var text = array[i];
                            var fontName = text.font.name;
                            //Debug.Log(fontName);
                            if (fontName.Contains("SourceHanSansCN"))
                            {
                                var uniqueKey = AssetsCheckUILogic.GetTipsUniqueKey(text.gameObject);
                                key.Value.uniqueKey.Add(uniqueKey);
                                exist = true;
                            }
                        }
                        if (!exist)
                        {
                            continue;
                        }
                        
                    }
                    System.Diagnostics.Stopwatch stopwatch = new System.Diagnostics.Stopwatch();
                    stopwatch.Start();
                    GameObject go = GameObject.Instantiate(gameObject);
                    stopwatch.Stop();

                    //  获取当前实例测量得出的总时间
                    TimeSpan timespan = stopwatch.Elapsed;
                    double milliseconds = timespan.TotalMilliseconds;
                    GameObject.DestroyImmediate(go);

                    key.Value.desc = $"{milliseconds.ToString()}ms";
                    m_selectedAssetGuidList.Add(key.Key);
                }
            }
            m_selectedAssetGuidList = ReferenceFinderData.sortByName(m_selectedAssetGuidList);
            Debug.Log(m_selectedAssetGuidList.Count);
            needUpdateAssetTree = true;
        }

        void ReplaceFont()
        {
            m_isDepend = true;
            m_selectedAssetGuidList.Clear();
            var data = m_data;
            data.CollectDependenciesInfo();
            var font = AssetDatabase.LoadAssetAtPath<Font>("Assets/GameData/AppRes/Common/CommonFont/SourceHanSansCN.ttf");

            foreach (var key in data.m_assetDict)
            {
                string path = key.Value.path;
                int idx = path.IndexOf(AssetsCheckerUtils.CheckPath);
                if (idx < 0)
                {
                    continue;
                }
                if (!path.EndsWith(".prefab"))
                {
                    continue;
                }
                ModifyPrefab(path, font);
            }
            m_selectedAssetGuidList = ReferenceFinderData.sortByName(m_selectedAssetGuidList);
            Debug.Log(m_selectedAssetGuidList.Count);
            needUpdateAssetTree = true;
        }

        void CheckEditBox()
        {
            m_isDepend = true;
            m_selectedAssetGuidList.Clear();
            var data = m_data;
            data.CollectDependenciesInfo();
            foreach (var key in data.m_assetDict)
            {
                string path = key.Value.path;
                int idx = path.IndexOf(AssetsCheckerUtils.CheckPath);
                if (idx < 0)
                {
                    continue;
                }
                if (!path.EndsWith(".prefab"))
                {
                    continue;
                }
                var go = AssetDatabase.LoadAssetAtPath<GameObject>(path);
                if (go == null)
                {
                    continue;
                }
                var array = go.GetComponentsInChildren<InputField>();
                if (array.Length == 0)
                {
                    continue;
                }
                bool selected = false;
                for (int i = 0; i < array.Length; i++)
                {
                    var inputField = array[i];
                    if (!inputField.textComponent.raycastTarget || !inputField.placeholder.raycastTarget)
                    {
                        Debug.Log(path);
                        //selected = true;
                        var uniqueKey = AssetsCheckUILogic.GetTipsUniqueKey(inputField.gameObject);
                        key.Value.uniqueKey.Add(uniqueKey);
                        inputField.textComponent.raycastTarget = true;
                        inputField.placeholder.raycastTarget = true;
                        Debug.Log($"未开启 raycastTarget{path} {uniqueKey}");
                    }
                }
                if (selected)
                {
                    m_selectedAssetGuidList.Add(key.Key);
                    Debug.Log($"替换字体后，保存预制体{path}");
                    EditorUtility.SetDirty(go);
                    PrefabUtility.SavePrefabAsset(go);
                }
            }
            m_selectedAssetGuidList = ReferenceFinderData.sortByName(m_selectedAssetGuidList);
            Debug.Log(m_selectedAssetGuidList.Count);
            needUpdateAssetTree = true;
        }

        void ModifyPrefab(string path, Font font)
        {
            var go = AssetDatabase.LoadAssetAtPath<GameObject>(path);
            if (go == null)
            {
                return;
            }
            bool savePrefab = false;
            var array = go.GetComponentsInChildren<Text>(true);
            if (array.Length == 0)
            {
                return;
            }
            for (int i = 0; i < array.Length; i++)
            {
                var text = array[i];
                bool replace = false;
                if (text.font == null)
                {
                    replace = true;
                }
                else if (text.font.name == "SourceHanSansCN Blod" || text.font.name == "Arial")
                {
                    replace = true;
                }
                //Debug.Log($"{path} {text.gameObject.name} {text.font.name}");
                //if (replace)
                //{
                //    replace = false;
                //    Debug.Log($"ModifyPrefab {path}");
                //}
                if (replace)
                {
                    text.font = font;
                    savePrefab = true;
                }
                if (text.fontStyle != FontStyle.Normal)
                {
                    text.fontStyle = FontStyle.Normal;
                    savePrefab = true;
                }
            }
            if (savePrefab)
            {
                Debug.Log($"替换字体后，保存预制体{path}");
                EditorUtility.SetDirty(go);
                PrefabUtility.SavePrefabAsset(go);
            }
        }

        void DumpAllUI()
        {
            bool bSystemFontText = true;
            var data = m_data;
            data.CollectDependenciesInfo();
            Dictionary<string, int> dic = new Dictionary<string, int>();

            foreach (var key in data.m_assetDict)
            {
                string path = key.Value.path;
                int idx = path.IndexOf(AssetsCheckerUtils.CheckPath);
                if (idx < 0)
                {
                    continue;
                }
                if (!path.EndsWith(".prefab"))
                {
                    continue;
                }

                var gameObject = AssetDatabase.LoadAssetAtPath<GameObject>(path);
                if (gameObject.GetComponent<RectTransform>() != null)
                {
                    int t = 0;
                    ///Debug.Log(path);
                    if (bSystemFontText)
                    {
                        var array = gameObject.GetComponentsInChildren<Text>(true);
                        for (int i = 0; i < array.Length; i++)
                        {
                            var text = array[i];
                            var fontName = text.font.name;
                            //Debug.Log(fontName);
                            if (fontName.Contains("SourceHanSansCN"))
                            {
                                t = 1;
                                break;
                            }
                        }
                    }
                    string k = path.Replace("Assets/GameData/", "");
                    dic[k] = t;
                }
            }

            var str = LitJson.JsonMapper.ToJson(dic);
            File.WriteAllText(Application.dataPath + "/GameData/AppRes/DataBin/ui.json", str);
            Debug.Log("保存成功");
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
                var headerState = SpriteAtlasAssetTreeView.CreateDefaultMultiColumnHeaderState(position.width);
                var multiColumnHeader = new MultiColumnHeader(headerState);
                m_AssetTreeView = new SpriteAtlasAssetTreeView(m_TreeViewState, multiColumnHeader);
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

            if (GUILayout.Button("所有UI", toolbarButtonGUIStyle))
            {
                SelectAssets(false);
            }

            if (GUILayout.Button("包含系统字的UI", toolbarButtonGUIStyle))
            {
                SelectAssets(true);
            }

            if (GUILayout.Button("保存所有UI列表", toolbarButtonGUIStyle))
            {
                DumpAllUI();
            }

            if (GUILayout.Button("替换所有字体", toolbarButtonGUIStyle))
            {
                ReplaceFont();
            }

            if (GUILayout.Button("检查编辑框", toolbarButtonGUIStyle))
            {
                CheckEditBox();
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

    public enum TVItemType
    {
        NONE,
        Asset,
        HASH
    }

    //带数据的TreeViewItem
    public class AssetViewItem : TreeViewItem
    {
        public TVItemType type = TVItemType.NONE;
        public string hash;
        public ReferenceFinderData.AssetDescription data;

        public AssetViewItem()
        {

        }
    }

    //资源引用树
    public class SpriteAtlasAssetTreeView : TreeView
    {
        //图标宽度
        const float kIconWidth = 18f;

        //列表高度
        const float kRowHeights = 25f;
        public AssetViewItem assetRoot;

        private GUIStyle stateGUIStyle = new GUIStyle { richText = true, alignment = TextAnchor.MiddleCenter };
        public Dictionary<string, List<string>> hashTextures;


        enum MyColumns
        {
            //列信息
            Name,
            Path,
            Description,

        }

        public SpriteAtlasAssetTreeView(TreeViewState state, MultiColumnHeader multicolumnHeader) : base(state, multicolumnHeader)
        {
            rowHeight = kRowHeights;
            columnIndexForTreeFoldouts = 0;
            showAlternatingRowBackgrounds = true;
            showBorder = false;
            customFoldoutYOffset =
                (kRowHeights - EditorGUIUtility.singleLineHeight) *
                0.5f; // center foldout in the row since we also center content. See RowGUI
            extraSpaceBeforeIconAndLabel = kIconWidth;
        }

        //响应右击事件
        protected override void ContextClickedItem(int id)
        {
            var item = (AssetViewItem)FindItem(id, rootItem);
            if (item == null)
                return;

            if (item.data == null)
                return;

            GenericMenu menu = new GenericMenu();

            menu.AddItem(new GUIContent("使用 astc 4x4"), false, x =>
            {
                //var selection = this.GetSelection();
                //for (int i = 0; i < selection.Count; i++)
                //{
                //    var selId = selection[i];
                //    var findItem = this.FindItem(selId, item.parent) as AssetViewItem;
                //    var asset = AssetDatabase.LoadAssetAtPath<SpriteAtlas>(findItem.data.path);
                //    if (asset == null)
                //    {
                //        continue;
                //    }
                //    Debug.Log($"{findItem.data.path} Success");
                //    UIPrefabCheckerWindow.SetAstcFormat(asset, TextureImporterFormat.ASTC_4x4);
                //}
            }, item);


            menu.AddItem(new GUIContent("Delete Asset"), false, x =>
            {
                var selection = this.GetSelection();
                List<string> rslt = new List<string>();
                for (int i = 0; i < selection.Count; i++)
                {
                    var selId = selection[i];
                    var findItem = this.FindItem(selId, item.parent) as AssetViewItem;
                    AssetDatabase.DeleteAsset(findItem.data.path);
                    rslt.Add(findItem.data.path);
                }
            }, item);
            if (menu.GetItemCount() > 0)
            {
                menu.ShowAsContext();
            }
            else
            {
                SetExpanded(id, !IsExpanded(id));
            }
        }

        //响应双击事件
        protected override void DoubleClickedItem(int id)
        {
            var item = (AssetViewItem)FindItem(id, rootItem);
            //在ProjectWindow中高亮双击资源
            if (item == null)
                return;
            if (item.data == null)
                return;

            var info = item.data;
            var list = new HashSet<string>();
            if (info.uniqueKey.Count != 0)
            {
                
                for (int i = 0; i < info.uniqueKey.Count; i++)
                {
                    list.Add(info.uniqueKey[i]);
                }
                AssetsCheckUILogic.GoToAndSelectTips(info.path, list);
            }
            else
            {
                AssetsCheckUILogic.GoToAndSelectTips(info.path, list);
            }
        }

        //生成ColumnHeader
        public static MultiColumnHeaderState CreateDefaultMultiColumnHeaderState(float treeViewWidth)
        {
            var columns = new[] {
            //图标+名称
            new MultiColumnHeaderState.Column {
                headerContent = new GUIContent("Name"),
                headerTextAlignment = TextAlignment.Center,
                sortedAscending = false,
                width = 200,
                minWidth = 60,
                autoResize = false,
                allowToggleVisibility = false,
                canSort = false
            },
            //路径
            new MultiColumnHeaderState.Column {
                headerContent = new GUIContent("Path"),
                headerTextAlignment = TextAlignment.Center,
                sortedAscending = false,
                width = 560,
                minWidth = 60,
                autoResize = false,
                allowToggleVisibility = false,
                canSort = false
            },
             //描述
            new MultiColumnHeaderState.Column {
                headerContent = new GUIContent("Description"),
                headerTextAlignment = TextAlignment.Center,
                sortedAscending = false,
                width = 230,
                minWidth = 100,
                autoResize = false,
                allowToggleVisibility = true,
                canSort = false
            }

        };
            var state = new MultiColumnHeaderState(columns);
            return state;
        }

        protected override TreeViewItem BuildRoot()
        {
            return assetRoot;
        }

        protected override void RowGUI(RowGUIArgs args)
        {
            var item = (AssetViewItem)args.item;
            for (int i = 0; i < args.GetNumVisibleColumns(); ++i)
            {
                CellGUI(args.GetCellRect(i), item, (MyColumns)args.GetColumn(i), ref args);
            }
        }

        void CellGUI(Rect cellRect, AssetViewItem item, MyColumns column, ref RowGUIArgs args)
        {
            //绘制列表中的每项内容
            CenterRectUsingSingleLineHeight(ref cellRect);
            var assetDescription = item.data;
            switch (column)
            {
                case MyColumns.Name:
                    {
                        var iconRect = cellRect;
                        iconRect.x += GetContentIndent(item);
                        iconRect.width = kIconWidth;
                        if (iconRect.x < cellRect.xMax)
                        {
                            var icon = assetDescription == null ? null : GetIcon(assetDescription.path);
                            if (icon != null)
                                GUI.DrawTexture(iconRect, icon, ScaleMode.ScaleToFit);
                        }

                        args.rowRect = cellRect;
                        base.RowGUI(args);
                    }
                    break;
                case MyColumns.Path:
                    {
                        GUI.Label(cellRect, item.type == TVItemType.HASH ? item.hash : assetDescription.path);
                    }
                    break;

                case MyColumns.Description:
                    {
                        if (assetDescription != null)
                        {
                            var str = assetDescription.desc;
                            if (str.Contains("ASTC_6x6"))
                            {
                                GUI.Label(cellRect, $"<color=#F0672AFF>{assetDescription.desc}</color>", stateGUIStyle);
                            }
                            else if (str.Contains("ASTC_5x5"))
                            {
                                GUI.Label(cellRect, $"<color=#F067FAFF>{assetDescription.desc}</color>", stateGUIStyle);
                            }
                            else if (str.Contains("ASTC_4x4"))
                            {
                                GUI.Label(cellRect, $"<color=#FFFF2AFF>{assetDescription.desc}</color>", stateGUIStyle);
                            }
                            else
                            {
                                GUI.Label(cellRect, $"<color=#FF0000FF>{assetDescription.desc}</color>", stateGUIStyle);
                            }
                        }
                    }
                    break;
            }
        }

        //根据资源信息获取资源图标
        private Texture2D GetIcon(string path)
        {
            UnityEngine.Object obj = AssetDatabase.LoadAssetAtPath(path, typeof(UnityEngine.Object));
            if (obj != null)
            {
                Texture2D icon = AssetPreview.GetMiniThumbnail(obj);
                if (icon == null)
                    icon = AssetPreview.GetMiniTypeThumbnail(obj.GetType());
                return icon;
            }

            return null;
        }
    }
}
