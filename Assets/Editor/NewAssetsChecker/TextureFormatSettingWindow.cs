using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEditor.IMGUI.Controls;
using UnityEngine.U2D;
using UnityEditor.U2D;
using System.Data;
using System.Text.RegularExpressions;


namespace TextureFormatSettingChecker
{

    public class TextureDescription
    {
        public const string ATALS_EXT = ".spriteatlas";

        public ReferenceFinderData.AssetDescription assetDescription;
        public string path;
        public string guid;
        public bool mipmap;
        public TextureImporterFormat textureImporterFormat = TextureImporterFormat.Automatic;

        public string GetKeyWord()
        {
            string ret = path;
            if (textureImporterFormat != TextureImporterFormat.ASTC_4x4 &&
                textureImporterFormat != TextureImporterFormat.ASTC_5x5 &&
                textureImporterFormat != TextureImporterFormat.ASTC_6x6 && 
                textureImporterFormat != TextureImporterFormat.ASTC_8x8)
            {
                ret += "_未设置纹理压缩";
            }
            if (mipmap)
            {
                ret += "_开启mipmap";
            }
            //Debug.Log($"{ret} {textureImporterFormat.ToString()}");
            return ret;
        }

        public string GetDesc()
        {
            var str = textureImporterFormat.ToString();
            string ret = "";
            if (str.Contains("ASTC_6x6"))
            {
                ret = $"<color=#00FF00FF>{str}</color>";
            }
            else if (str.Contains("ASTC_5x5"))
            {
                ret = $"<color=#F067FAFF>{str}</color>";
            }
            else if (str.Contains("ASTC_4x4"))
            {
                ret = $"<color=#FFFFFFFF>{str}</color>";
            }
            else if (str.Contains("ASTC_8x8"))
            {
                ret = $"<color=#FF002AFF>{str}</color>";
            }
            else
            {
                ret = $"<color=#FF0000FF>未设置纹理压缩</color>";
            }

            if (mipmap)
            {
                ret += $"+<color=#FF0000FF>mipmap</color>";
            }

            return ret;
        }

        public bool IsSpriteAtals()
        {
            return assetDescription.path.EndsWith(ATALS_EXT);
        }


        public void Init()
        {
            if (IsSpriteAtals())
            {
                var asset = AssetDatabase.LoadAssetAtPath<SpriteAtlas>(assetDescription.path);
                if (asset != null)
                {
                    TextureImporterPlatformSettings platformSetting = asset.GetPlatformSettings("Android");
                    if (platformSetting != null)
                    {
                        textureImporterFormat = platformSetting.format;
                    }
                }
            }
            else
            {
                var importer = AssetImporter.GetAtPath(assetDescription.path) as TextureImporter;
                if (importer == null)
                {
                    return;
                }
                mipmap = importer.mipmapEnabled;
                TextureImporterPlatformSettings platformSetting = importer.GetPlatformTextureSettings("Android");
                if (platformSetting == null)
                {
                    return;
                }
                textureImporterFormat = platformSetting.format;
            }
        }

        public void SetTextureImporterFormat(TextureImporterFormat f)
        {
            if (f == textureImporterFormat)
            {
                return;
            }
            textureImporterFormat = f;
            if (IsSpriteAtals())
            {
                SetSpriteAtlasAstcFormat(f);
            }
            else
            {
                SetTextureAstcFormat(f);
            }
        }

        void SetTextureAstcFormat(TextureImporterFormat format)
        {
            var textureImporter = AssetImporter.GetAtPath(assetDescription.path) as TextureImporter;
            if (textureImporter == null)
            {
                return;
            }
            SetAndroidTextureSetting(textureImporter, format);
            SetIosTextureSetting(textureImporter, format);

            AssetDatabase.WriteImportSettingsIfDirty(textureImporter.assetPath);
        }

        private void SetAndroidTextureSetting(TextureImporter textureImporter, TextureImporterFormat format)
        {
            TextureImporterPlatformSettings androidSetting = new TextureImporterPlatformSettings();
            androidSetting.overridden = true;
            androidSetting.name = "Android";
            androidSetting.maxTextureSize = textureImporter.maxTextureSize;
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
            iOSSettings.maxTextureSize = textureImporter.maxTextureSize;
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

        void SetSpriteAtlasAstcFormat(TextureImporterFormat format)
        {
            var asset = AssetDatabase.LoadAssetAtPath<SpriteAtlas>(assetDescription.path);
            if (asset == null)
            {
                return;
            }
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

        public void DisableMipMap()
        {
            mipmap = false;
            var textureImporter = AssetImporter.GetAtPath(assetDescription.path) as TextureImporter;
            if (textureImporter == null)
            {
                return;
            }
            textureImporter.mipmapEnabled = false;
            AssetDatabase.WriteImportSettingsIfDirty(textureImporter.assetPath);
        }
    }

    // 纹理压缩设置界面
    class TextureFormatSettingWindow : EditorWindow
    {
        ReferenceFinderData mReferenceFinderData;
        bool useRegex;
        private int m_SelectCount;
        private string m_SelectFolder;
        private bool m_isDepend = false;
        private bool m_needUpdateState = true;

        private bool needUpdateAssetTree = false;


        //工具栏按钮样式
        private GUIStyle toolbarButtonGUIStyle;

        //工具栏样式
        private GUIStyle toolbarGUIStyle;

        //选中资源列表
        private List<TextureDescription> m_selectedAssetGuidList = new List<TextureDescription>();
        Dictionary<string, TextureDescription> m_DataDic = new Dictionary<string, TextureDescription>();

        private SpriteAtlasAssetTreeView m_AssetTreeView;

        [SerializeField] private TreeViewState m_TreeViewState;


        //打开窗口
        [MenuItem("资源检查/纹理压缩设置")]
        static void OpenWindow()
        {
            TextureFormatSettingWindow window = GetWindow<TextureFormatSettingWindow>();
            window.wantsMouseMove = false;
            window.titleContent = new GUIContent("纹理压缩设置");
            window.InitStyle();
            window.Show();
            window.Focus();
        }

        void InitStyle()
        {
            if (toolbarButtonGUIStyle != null)
            {
                return;
            }
            toolbarButtonGUIStyle = new GUIStyle("ToolbarButton");
            toolbarGUIStyle = new GUIStyle("Toolbar");
        }

        void SelectAssets()
        {
            m_DataDic = new Dictionary<string, TextureDescription>();
            var exts = ".png;.jpg;.tga;.spriteatlas";
            var extlist = exts.Split(';');

            m_isDepend = false;
            m_selectedAssetGuidList.Clear();
            mReferenceFinderData = new ReferenceFinderData();
            mReferenceFinderData.CollectDependenciesInfo();

            foreach (var key in mReferenceFinderData.m_assetDict)
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
                if (ext != ".spriteatlas")
                {
                    bool inAtlas = false;
                    for (int i = 0; i < info.references.Count; i++)
                    {
                        var dp = mReferenceFinderData.m_assetDict[info.references[i]].path;
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
                }
                key.Value.desc = "";// GetAstcFormat(key.Value.path);
                var unit = new TextureDescription();
                unit.path = key.Value.path;
                unit.guid = key.Key;
                unit.assetDescription = key.Value;
                unit.Init();
                m_selectedAssetGuidList.Add(unit);
            }
            m_selectedAssetGuidList.Sort((a, b) =>
            {
                return a.path.CompareTo(b.path);
            });
            //m_selectedAssetGuidList.OrderBy(x =>
            //{
            //    return x.assetDescription.path;
            //});
            needUpdateAssetTree = true;
        }

        //通过选中资源列表更新TreeView
        private void UpdateAssetTree()
        {
            if (needUpdateAssetTree)
            {
                needUpdateAssetTree = false;
                var root = SelectedAssetGuidToRootItem(m_selectedAssetGuidList);
                if (m_SelectCount > 0)
                {
                    UpdateTreeView(root);
                }
            }
        }

        private void UpdateTreeView(AssetViewItem root)
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
            InitStyle();
            DrawOptionBar();
            UpdateAssetTree();
            if (m_AssetTreeView != null && m_SelectCount > 0)
            {
                float height = toolbarGUIStyle.fixedHeight * 2;
                //绘制Treeview
                m_AssetTreeView.OnGUI(new Rect(0, height, position.width,
                    position.height - height));
            }
        }

        //绘制上条
        public void DrawOptionBar()
        {
            EditorGUILayout.BeginHorizontal();
            var folder = EditorGUILayout.TextField("目录筛选：", m_SelectFolder);
            if (m_SelectFolder != folder)
            {
                needUpdateAssetTree = true;
                m_SelectFolder = folder;
            }
            useRegex = GUILayout.Toggle(useRegex, "使用正则");
            if (GUILayout.Button("清除"))
            {
                if (m_SelectFolder != "")
                {
                    needUpdateAssetTree = true;
                    m_SelectFolder = "";
                }
            }
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal(toolbarGUIStyle);

            if (GUILayout.Button("刷新", toolbarButtonGUIStyle))
            {
                SelectAssets();
            }

            if (GUILayout.Button("所有合图资源", toolbarButtonGUIStyle))
            {
                SelectAssets();
                m_SelectFolder = ".spriteatlas";
            }

            if (GUILayout.Button("所有背景图", toolbarButtonGUIStyle))
            {
                SelectAssets();
                m_SelectFolder = "/Map/";
            }

            if (GUILayout.Button("未设置纹理压缩", toolbarButtonGUIStyle))
            {
                SelectAssets();
                m_SelectFolder = "未设置纹理压缩";
            }
            if (GUILayout.Button("开启mipmap的纹理", toolbarButtonGUIStyle))
            {
                SelectAssets();
                m_SelectFolder = "开启mipmap";
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
        private AssetViewItem SelectedAssetGuidToRootItem(List<TextureDescription> list)
        {
            updatedAssetSet.Clear();
            int elementCount = 0;
            var root = new AssetViewItem { id = elementCount, depth = -1, displayName = "Root", data = null };
            int depth = 0;
            var stack = new Stack<string>();
            m_SelectCount = 0;
            
            foreach (var itemData in list)
            {
                bool add = false;
                if (string.IsNullOrEmpty(m_SelectFolder))
                {
                    add = true;
                }
                else if (useRegex && Regex.IsMatch(itemData.GetKeyWord(), @m_SelectFolder))
                {
                    add = true;
                }
                else if (itemData.GetKeyWord().Contains(m_SelectFolder))
                {
                    add = true;
                }
                if (add)
                {
                    m_SelectCount++;
                    var child = CreateTree(itemData, ref elementCount, depth, stack);
                    if (child != null)
                    {
                        root.AddChild(child);
                    }
                }
            }

            updatedAssetSet.Clear();
            return root;
        }

        //通过每个节点的数据生成子节点
        private AssetViewItem CreateTree(TextureDescription itemData, ref int elementCount, int _depth, Stack<string> stack)
        {
            AssetViewItem root = null;
            ++elementCount;
            root = new AssetViewItem
            {
                id = elementCount,
                displayName = itemData.assetDescription.name,
                data = itemData,
                depth = _depth
            };
            return root;
        }
    }

    public enum TVItemType
    {
        NONE,
        Asset,
        HASH
    }

    #region Item
    //带数据的TreeViewItem
    public class AssetViewItem : TreeViewItem
    {
        public TVItemType type = TVItemType.NONE;
        public string hash;
        public TextureDescription data;

        public AssetViewItem()
        {

        }
    }
    #endregion

    #region Window
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
            Formation,
            Size,

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
                var selection = this.GetSelection();
                for (int i = 0; i < selection.Count; i++)
                {
                    var selId = selection[i];
                    var findItem = this.FindItem(selId, item.parent) as AssetViewItem;
                    findItem.data.SetTextureImporterFormat(TextureImporterFormat.ASTC_4x4);
                }
            }, item);

            menu.AddItem(new GUIContent("使用 astc 5x5"), false, x =>
            {
                var selection = this.GetSelection();
                for (int i = 0; i < selection.Count; i++)
                {
                    var selId = selection[i];
                    var findItem = this.FindItem(selId, item.parent) as AssetViewItem;
                    findItem.data.SetTextureImporterFormat(TextureImporterFormat.ASTC_5x5);
                }
            }, item);

            menu.AddItem(new GUIContent("使用 astc 6x6"), false, x =>
            {
                var selection = this.GetSelection();
                for (int i = 0; i < selection.Count; i++)
                {
                    var selId = selection[i];
                    var findItem = this.FindItem(selId, item.parent) as AssetViewItem;
                    findItem.data.SetTextureImporterFormat(TextureImporterFormat.ASTC_6x6);
                }
            }, item);

            menu.AddItem(new GUIContent("使用 astc 8x8"), false, x =>
            {
                var selection = this.GetSelection();
                for (int i = 0; i < selection.Count; i++)
                {
                    var selId = selection[i];
                    var findItem = this.FindItem(selId, item.parent) as AssetViewItem;
                    findItem.data.SetTextureImporterFormat(TextureImporterFormat.ASTC_8x8);
                }
            }, item);

            menu.AddItem(new GUIContent("关闭mipmap"), false, x =>
            {
                var selection = this.GetSelection();
                for (int i = 0; i < selection.Count; i++)
                {
                    var selId = selection[i];
                    var findItem = this.FindItem(selId, item.parent) as AssetViewItem;
                    findItem.data.DisableMipMap();
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
                        GUI.Label(cellRect, item.type == TVItemType.HASH ? item.hash : assetDescription.path.Replace(AssetsCheckerUtils.CheckPath, ""));
                    }
                    break;

                case MyColumns.Formation:
                    {
                        if (assetDescription != null)
                        {
                            GUI.Label(cellRect, assetDescription.GetDesc(), stateGUIStyle);
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
    #endregion
}
