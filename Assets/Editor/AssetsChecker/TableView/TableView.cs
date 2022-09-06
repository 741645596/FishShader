

using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.IMGUI.Controls;
using UnityEngine;

namespace EditerUtils
{
    /// <summary>
    /// 
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class TableView<T> : TreeView
    {
        // Cell相关回调
        public interface ITableViewCell
        {
            public void TableViewDidShowCell(T t, Rect r, int column, TableCellItem item);
            public void TableViewDidClickCell(T t, TableCellItem item);
            public void TableViewDidDoubleClickCell(T t, TableCellItem item);
            public void TableViewDidRightClickCell(T t, TableCellItem item);
        }

        public class TableCellItem : TreeViewItem
        {
            public T data;
        }

        // 图标宽度
        const float kIconWidth = 18f;

        private List<TreeViewItem> _datas;
        private ITableViewCell _iCellCB;

        public TableView(TreeViewState state,
            MultiColumnHeader multicolumnHeader,
            ITableViewCell cellCallback,
            float cellRowHeight)
            : base(state, multicolumnHeader)
        {
            rowHeight = cellRowHeight;
            columnIndexForTreeFoldouts = 0;
            showAlternatingRowBackgrounds = true;
            showBorder = true;
            //customFoldoutYOffset =
            //    (rowHeight - EditorGUIUtility.singleLineHeight) *
            //    0.5f;
            extraSpaceBeforeIconAndLabel = kIconWidth;

            _iCellCB = cellCallback;
            _datas = new List<TreeViewItem>();
        }

        /// <summary>
        /// T为自己指定的数据结构，用于回调使用
        /// </summary>
        /// <param name="datas"></param>
        public void Reload(List<T> datas)
        {
            _datas.Clear();

            for (int i=0; i<datas.Count; i++)
            {
                var item = new TableCellItem
                {
                    id = i+1,       // id必须唯一
                    depth = 0,      // 显示深度，默认从0开始，如果有子层级+1
                    displayName = "",
                    data = datas[i],
                };
                _datas.Add(item);
            }
            Reload();
        }

        /// <summary>
        /// 自己指定，用来显示多层级(树形)结构
        /// </summary>
        /// <param name="datas"></param>
        public void ReloadData(List<TableCellItem> datas)
        {
            _datas.Clear();
            foreach (TreeViewItem item in datas)
            {
                _datas.Add(item);
            }
            Reload();
        }

        /// <summary>
        /// 设置树形结构三角标起始位置在哪个column，0是第一个
        /// </summary>
        /// <param name="foldoutIndex"></param>
        public void SetFoldoutIndex(int foldoutIndex)
        {
            columnIndexForTreeFoldouts = foldoutIndex;
            //customFoldoutYOffset = (rowHeight - EditorGUIUtility.singleLineHeight) * 0.5f;
        }

        /// <summary>
        /// 获取树形结构三角标结束位置，如果该列有三角标需要加上该偏移量才是正确显示的起始位置
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        public float GetFoldoutOffsetX(TreeViewItem item)
        {
            return GetContentIndent(item);
        }

        protected override TreeViewItem BuildRoot()
        {
            var root = new TreeViewItem { id = 0, depth = -1 };
            SetupParentsAndChildrenFromDepths(root, _datas);
            return root;
        }

        protected override void RowGUI(RowGUIArgs args)
        {
            var item = (TableCellItem)args.item;
            for (int i = 0; i < args.GetNumVisibleColumns(); ++i)
            {
                var rect = args.GetCellRect(i);
                var colomnIndex = args.GetColumn(i);
                _iCellCB.TableViewDidShowCell(item.data, rect, colomnIndex, item);
            }
        }

        // 右键回调
        protected override void ContextClickedItem(int id)
        {
            var item = FindItem(id, rootItem) as TableCellItem;
            _iCellCB.TableViewDidRightClickCell(item.data, item);
        }

        protected override void SingleClickedItem(int id)
        {
            var item = FindItem(id, rootItem) as TableCellItem;
            _iCellCB.TableViewDidClickCell(item.data, item);
        }

        protected override void DoubleClickedItem(int id)
        {
            var item = FindItem(id, rootItem) as TableCellItem;
            _iCellCB.TableViewDidDoubleClickCell(item.data, item);
        }
    }
}


