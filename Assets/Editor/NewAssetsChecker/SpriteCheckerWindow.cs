using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEditor.IMGUI.Controls;
using UnityEngine.U2D;
using UnityEditor.U2D;


namespace SpriteAtalsChecker
{

    public class SpriteCheckerWindow : EditorWindow
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

        private SpriteAssetTreeView m_AssetTreeView;

        [SerializeField] private TreeViewState m_TreeViewState;


        //打开窗口
        [MenuItem("资源检查/精灵资源")]
        static void OpenWindow()
        {
            SpriteCheckerWindow window = GetWindow<SpriteCheckerWindow>();
            window.wantsMouseMove = false;
            window.titleContent = new GUIContent("精灵资源检查");
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

        void SelectSpriteAssets()
        {
            var exts = ".png;.jpg;.tga";
            var extlist = exts.Split(';');

            m_isDepend = true;
            m_selectedAssetGuidList.Clear();
            var data = m_data;
            data.CollectDependenciesInfo();

            foreach (var key in data.m_assetDict)
            {
                var info = key.Value;
                string path = key.Value.path;
                int idx = path.IndexOf(AssetsCheckerUtils.CheckPath);
                if (idx < 0)
                {
                    continue;
                }
                string ext = Path.GetExtension(path);
                if (!extlist.Contains(ext))
                {
                    continue;
                }
                var importer = AssetImporter.GetAtPath(info.path) as TextureImporter;

                if (importer == null)
                {
                    continue;
                }
                if (importer.textureType != TextureImporterType.Sprite)
                {
                    continue;
                }
                bool inAtlas = false;
                for (int i = 0; i < info.references.Count; i++)
                {
                    var dp = data.m_assetDict[info.references[i]].path;
                    if (dp.EndsWith(".spriteatlas"))
                    {
                        inAtlas = true;
                        break;
                    }
                }
                if (inAtlas)
                {
                    continue;
                }
                key.Value.desc = GetAstcFormat(key.Value.path);
                m_selectedAssetGuidList.Add(key.Key);
            }
            m_selectedAssetGuidList = ReferenceFinderData.sortByName(m_selectedAssetGuidList);
            needUpdateAssetTree = true;
        }

        void AllToAstc4x4()
        {
            for (int i = 0; i < m_selectedAssetGuidList.Count; i++)
            {
                var info = m_data.m_assetDict[m_selectedAssetGuidList[i]];
                var importer = AssetImporter.GetAtPath(info.path) as TextureImporter;
                SetAstcFormat(importer, TextureImporterFormat.ASTC_4x4);
            }
        }

        void AllToAstc6x6()
        {
            for (int i = 0; i < m_selectedAssetGuidList.Count; i++)
            {
                var info = m_data.m_assetDict[m_selectedAssetGuidList[i]];
                var importer = AssetImporter.GetAtPath(info.path) as TextureImporter;
                SetAstcFormat(importer, TextureImporterFormat.ASTC_6x6);
            }
        }

        void AllToAstc5x5()
        {
            for (int i = 0; i < m_selectedAssetGuidList.Count; i++)
            {
                var info = m_data.m_assetDict[m_selectedAssetGuidList[i]];
                var importer = AssetImporter.GetAtPath(info.path) as TextureImporter;
                SetAstcFormat(importer, TextureImporterFormat.ASTC_5x5);
            }
        }

        public static void SetAstcFormat(SpriteAtlas asset, TextureImporterFormat format)
        {
            TextureImporterPlatformSettings platformSetting = asset.GetPlatformSettings("Android");
            platformSetting.overridden = true;
            platformSetting.maxTextureSize = 2048;
            platformSetting.format = format;
            asset.SetPlatformSettings(platformSetting);

            platformSetting = asset.GetPlatformSettings("iPhone");
            platformSetting.overridden = true;
            platformSetting.maxTextureSize = 2048;
            platformSetting.format = format;
            asset.SetPlatformSettings(platformSetting);

            AssetDatabase.SaveAssets();
        }

        public static string GetAstcFormat(string path)
        {
            var importer = AssetImporter.GetAtPath(path) as TextureImporter;

            if (importer == null)
            {
                return "";
            }
            TextureImporterPlatformSettings platformSetting = importer.GetPlatformTextureSettings("Android");
            if (platformSetting == null)
            {
                return "";
            }
            TextureImporterFormat format = platformSetting.format;
            return format.ToString();
        }

        

        private void SetAstcFormat(TextureImporter textureImporter, TextureImporterFormat format)
        {
            SetAndroidTextureSetting(textureImporter, format);
            SetIosTextureSetting(textureImporter, format);

            AssetDatabase.WriteImportSettingsIfDirty(textureImporter.assetPath);
        }

        private void SetAndroidTextureSetting(TextureImporter textureImporter, TextureImporterFormat format)
        {
            TextureImporterPlatformSettings androidSetting = new TextureImporterPlatformSettings();
            androidSetting.overridden = true;
            androidSetting.name = "Android";
            androidSetting.maxTextureSize = 2048;
            androidSetting.format = format;

            try
            {
                textureImporter.SetPlatformTextureSettings(androidSetting);
            }
            catch (Exception e)
            {
                Debug.Log(textureImporter.assetPath);
                Console.WriteLine(e);
                throw;
            }
        }

        private void SetIosTextureSetting(TextureImporter textureImporter, TextureImporterFormat format)
        {
            TextureImporterPlatformSettings iOSSettings = new TextureImporterPlatformSettings();
            iOSSettings.overridden = true;
            iOSSettings.name = "iOS";
            iOSSettings.textureCompression = TextureImporterCompression.Uncompressed;
            iOSSettings.maxTextureSize = 2048;
            iOSSettings.format = format;
            try
            {
                textureImporter.SetPlatformTextureSettings(iOSSettings);
            }
            catch (Exception e)
            {
                Debug.Log(textureImporter.assetPath);
                Console.WriteLine(e);
                throw;
            }
        }


        private SpriteAssetViewItem mapToTvRoot(Dictionary<string, List<string>> selectedAssetGuid)
        {
            updatedAssetSet.Clear();
            int elementCount = 0;
            var root = new SpriteAssetViewItem { id = elementCount, depth = -1, displayName = "Root", data = null };
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

                var hashRoot = new SpriteAssetViewItem
                {
                    type = SpriteItemType.HASH,
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

        private void updateTreeView(SpriteAssetViewItem root)
        {
            if (m_AssetTreeView == null)
            {
                //初始化TreeView
                if (m_TreeViewState == null)
                    m_TreeViewState = new TreeViewState();
                var headerState = SpriteAssetTreeView.CreateDefaultMultiColumnHeaderState(position.width);
                var multiColumnHeader = new MultiColumnHeader(headerState);
                m_AssetTreeView = new SpriteAssetTreeView(m_TreeViewState, multiColumnHeader);
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

            if (GUILayout.Button("刷新", toolbarButtonGUIStyle))
            {
                SelectSpriteAssets();
            }

            if (GUILayout.Button("都转成asc6x6", toolbarButtonGUIStyle))
            {
                AllToAstc6x6();
            }

            if (GUILayout.Button("都转成asc5x5", toolbarButtonGUIStyle))
            {
                AllToAstc5x5();
            }

            if (GUILayout.Button("都转成asc4x4", toolbarButtonGUIStyle))
            {
                AllToAstc4x4();
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
        private SpriteAssetViewItem SelectedAssetGuidToRootItem(List<string> selectedAssetGuid)
        {
            updatedAssetSet.Clear();
            int elementCount = 0;
            var root = new SpriteAssetViewItem { id = elementCount, depth = -1, displayName = "Root", data = null };
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
        private SpriteAssetViewItem CreateTree(string guid, ref int elementCount, int _depth, Stack<string> stack)
        {
            if (stack.Contains(guid))
                return null;

            SpriteAssetViewItem root = null;

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
                root = new SpriteAssetViewItem
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

    public enum SpriteItemType
    {
        NONE,
        Asset,
        HASH
    }

    //带数据的TreeViewItem
    public class SpriteAssetViewItem : TreeViewItem
    {
        public SpriteItemType type = SpriteItemType.NONE;
        public string hash;
        public ReferenceFinderData.AssetDescription data;

        public SpriteAssetViewItem()
        {

        }
    }

    //资源引用树
    public class SpriteAssetTreeView : TreeView
    {
        //图标宽度
        const float kIconWidth = 18f;

        //列表高度
        const float kRowHeights = 25f;
        public SpriteAssetViewItem assetRoot;

        private GUIStyle stateGUIStyle = new GUIStyle { richText = true, alignment = TextAnchor.MiddleCenter };
        public Dictionary<string, List<string>> hashTextures;


        enum MyColumns
        {
            //列信息
            Name,
            Path,
            Description,

        }

        public SpriteAssetTreeView(TreeViewState state, MultiColumnHeader multicolumnHeader) : base(state, multicolumnHeader)
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
            var item = (SpriteAssetViewItem)FindItem(id, rootItem);
            if (item == null)
                return;

            if (item.data == null)
                return;

            GenericMenu menu = new GenericMenu();

            menu.AddItem(new GUIContent("使用 astc 4x4"), false, x =>
            {
                var selection = this.GetSelection();
                for (int i = 0; i < selection.Count; i++)
                {
                    var selId = selection[i];
                    var findItem = this.FindItem(selId, item.parent) as SpriteAssetViewItem;
                    var asset = AssetDatabase.LoadAssetAtPath<SpriteAtlas>(findItem.data.path);
                    if (asset == null)
                    {
                        continue;
                    }
                    Debug.Log($"{findItem.data.path} Success");
                    SpriteCheckerWindow.SetAstcFormat(asset, TextureImporterFormat.ASTC_4x4);
                }
            }, item);

            menu.AddItem(new GUIContent("使用 astc 5x5"), false, x =>
            {
                var selection = this.GetSelection();
                for (int i = 0; i < selection.Count; i++)
                {
                    var selId = selection[i];
                    var findItem = this.FindItem(selId, item.parent) as SpriteAssetViewItem;
                    var asset = AssetDatabase.LoadAssetAtPath<SpriteAtlas>(findItem.data.path);
                    if (asset == null)
                    {
                        continue;
                    }
                    SpriteCheckerWindow.SetAstcFormat(asset, TextureImporterFormat.ASTC_5x5);
                }
            }, item);

            menu.AddItem(new GUIContent("使用 astc 6x6"), false, x =>
            {
                var selection = this.GetSelection();
                for (int i = 0; i < selection.Count; i++)
                {
                    var selId = selection[i];
                    var findItem = this.FindItem(selId, item.parent) as SpriteAssetViewItem;
                    var asset = AssetDatabase.LoadAssetAtPath<SpriteAtlas>(findItem.data.path);
                    if (asset == null)
                    {
                        continue;
                    }
                    SpriteCheckerWindow.SetAstcFormat(asset, TextureImporterFormat.ASTC_6x6);
                }
            }, item);

            menu.AddItem(new GUIContent("Delete Asset"), false, x =>
            {
                var selection = this.GetSelection();
                List<string> rslt = new List<string>();
                for (int i = 0; i < selection.Count; i++)
                {
                    var selId = selection[i];
                    var findItem = this.FindItem(selId, item.parent) as SpriteAssetViewItem;
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
            var item = (SpriteAssetViewItem)FindItem(id, rootItem);
            //在ProjectWindow中高亮双击资源
            if (item == null)
                return;
            if (item.data == null)
                return;

            var assetObject = AssetDatabase.LoadAssetAtPath(item.data.path, typeof(UnityEngine.Object));
            EditorUtility.FocusProjectWindow();
            Selection.activeObject = assetObject;
            EditorGUIUtility.PingObject(assetObject);
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
            var item = (SpriteAssetViewItem)args.item;
            for (int i = 0; i < args.GetNumVisibleColumns(); ++i)
            {
                CellGUI(args.GetCellRect(i), item, (MyColumns)args.GetColumn(i), ref args);
            }
        }

        void CellGUI(Rect cellRect, SpriteAssetViewItem item, MyColumns column, ref RowGUIArgs args)
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
                        GUI.Label(cellRect, item.type == SpriteItemType.HASH ? item.hash : assetDescription.path);
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
