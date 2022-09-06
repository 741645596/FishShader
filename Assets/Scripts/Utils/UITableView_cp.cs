using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public enum ScrollDir
{
    DIR_VERTICAL,
    DIR_HORIZONTAL,
}

public class TableCell
{
    public int index = 0;
    public GameObject cellObj = null;
}

public class TableCellGroup
{
    public int Id;
    public int Total;
    public ScrollDir Dir;
    public int Group;
    public Vector2 LocalPosition = new Vector2(0, 0);
    public Vector2 Size = new Vector2(1,1);
    public Vector2 CellSize;                   // 子控件尺寸
    public Vector2 SpacingSize;                // 间隔（垂直的只有y有用，水平的只有x有用）
    public Action<GameObject, int> CallBack = null;
    public UITableView_cp Parent;

    public List<TableCell> cellList = new List<TableCell>();

    public void CreateCells()
    {
        for(int i = 0; i < Group; i++)
        {
            var cell = Parent.CreateCell();
            cell.index = i;
            cellList.Add(cell);
            Parent.cellList.Add(cell);
        }
        // 计算大小
        Size = CellSize + SpacingSize;
        if (Dir == ScrollDir.DIR_VERTICAL)
            Size.x *= Group;
        else
            Size.y *= Group;
    }

    public void UpdatePosition(Vector2 pos)
    {
        LocalPosition = pos;
        int index = Id * Group;
        for (int i = 0; i < cellList.Count; i++)
        {
            var obj = cellList[i].cellObj;
            cellList[i].index = index;
            if (Dir == ScrollDir.DIR_VERTICAL)
                obj.transform.localPosition = new Vector3(i*(CellSize.x + SpacingSize.x), pos.y, 0);
            else
                obj.transform.localPosition = new Vector3(pos.x, -i * (CellSize.y + SpacingSize.y), 0);

            //LogUtils.I("UpdatePosition:" + index);
            obj.SetActive(index < Total);
            index++;
        }
    }

    public void OnCallBack()
    {
        for (int i = 0; i < cellList.Count; i++)
        {
            if (cellList[i].index < Total)
                CallBack(cellList[i].cellObj, cellList[i].index);
        }
    }

    public void Clear()
    {
        foreach(var cell in cellList)
        {
            cell.cellObj.SetActive(false);
            Parent.PutCell(cell);
        }
        cellList.Clear();
    }

    public bool HadCell(int id)
    {
        return (id >= Id * Group && id < (Id + 1) * Group);
    }

}

public class UITableView_cp : MonoBehaviour
{
    public List<TableCell> cellList = new List<TableCell>();
    private List<TableCell> cellPool = new List<TableCell>();
    private LinkedList<TableCellGroup> cellGroupList = new LinkedList<TableCellGroup>();
    private int Group;                          // 每一组的数量
    private int cellCount;                      // 子控件数量
    private Vector2 cellSize;                   // 子控件尺寸
    private Vector2 spacingSize;                // 间隔（垂直的只有y有用，水平的只有x有用）
    private ScrollDir direct;                   // 滚动方向，跟ScrollView一致(暂时只支持垂直和水平两种)
    private ScrollRect scrollRect = null;
    private GameObject cellPrefab = null;
    private Action<GameObject, int> updateCallBack = null;
    private Vector2 prevPos;
    private Vector2 curDirPos;
    private int curLastId = 0;
    private int groupCount = 0;

    /**
        sr: ScrollView控件
        prefab: 滚动列表中控件的预制体
        spacing: 子控件间隔
        callBack: 刷新子控件数据的回调函数
        groupNum : 一组多少个子控件
    */
    public void Init(ScrollRect sr, GameObject prefab, Vector2 spacing, Action<GameObject, int> updateCall, int groupNum = 1)
    {
        if (sr == null || prefab == null || updateCall == null)
        {
            LogUtils.E("UITableView_cp init error cellPrefab or callBack is null");
            return;
        }
        scrollRect = sr;
        cellPrefab = prefab;
        spacingSize = spacing;
        updateCallBack = updateCall;
        cellSize = cellPrefab.transform.GetComponent<RectTransform>().rect.size;
        direct = scrollRect.vertical?ScrollDir.DIR_VERTICAL:ScrollDir.DIR_HORIZONTAL;
        var content = scrollRect.content;
        // 设置默认锚点(左上)
        content.anchorMin = new Vector2(0, 1);
        content.anchorMax = new Vector2(0, 1);
        content.localPosition = new Vector3(0, 0);
        Group = groupNum;
        // 监听滚动事件
        //scrollRect.onValueChanged.AddListener((v) =>
        //{
        //    curDirPos = content.anchoredPosition - prevPos;
        //    prevPos = content.anchoredPosition;
        //    CheckAndUpdateCellList();
        //});
    }

    public void Reset(int count)
    {
        SetCellCout(count);
        ClearAllCell();
        UpdateContexSize();
    }

    public void SetCellCout(int count)
    {
        if (count < 0)
            count = 0;
        cellCount = count;
        groupCount = cellCount / Group;
        if(cellCount % Group > 0)
            groupCount += 1;

        foreach(var item in cellGroupList)
            item.Total = count;
        //LogUtils.I("SetCellCout:" + count + ", groupCount:" + groupCount);
    }

    public void UpdateContexSize()
    {
        int poolNum = 0;
        //Rect viewRect = scrollRect.viewport.rect;
        Rect viewRect = scrollRect.GetComponent<RectTransform>().rect;
        if (direct == ScrollDir.DIR_VERTICAL)
        {
            float height = cellSize.y + spacingSize.y;
            scrollRect.content.sizeDelta = new Vector2(scrollRect.content.sizeDelta.x, groupCount * height);
            poolNum = Convert.ToInt32(Math.Ceiling(viewRect.height / height)) + 1;
            if (viewRect.height % height == 0)
                poolNum += 1;
        }
        else
        {
            float width = cellSize.x + spacingSize.x;
            scrollRect.content.sizeDelta = new Vector2(groupCount * width, scrollRect.content.sizeDelta.y);
            poolNum = Convert.ToInt32(Math.Ceiling(viewRect.width / width)) + 1;
            if (viewRect.width % width == 0)
                poolNum += 1;
        }
        //LogUtils.I("Cell temp num:" + poolNum);
        for (int i = 0; i < poolNum; i++)
        {
            var cellGroup = new TableCellGroup()
            {
                Id = i,
                Total = cellCount,
                Dir = direct,
                Group = Group,
                CellSize = cellSize,
                SpacingSize = spacingSize,
                CallBack = updateCallBack,
                Parent = this,
            };
            cellGroup.CreateCells();
            Vector2 pos = new Vector2();
            if (direct == ScrollDir.DIR_VERTICAL)
                pos.y = -i * (cellSize.y + spacingSize.y);
            else
                pos.x = i * (cellSize.x + spacingSize.x);
            cellGroup.UpdatePosition(pos);
            cellGroup.OnCallBack();
            cellGroupList.AddLast(cellGroup);
            curLastId = i;
        }
        prevPos = scrollRect.content.anchoredPosition;
    }
    // 这个接口只能在列表的最后添加、删除多个元素
    public void AddCount(int count)
    {
        var tempCount = groupCount;
        SetCellCout(cellCount + count);
        if (groupCount < cellGroupList.Count || tempCount < cellGroupList.Count)
        {
            scrollRect.content.localPosition = new Vector3(0, 0);
            ClearAllCell();
            UpdateContexSize();
        }
        else
        {
            foreach(var cell in cellList)
                cell.cellObj.SetActive(cell.index < cellCount);
            ResetContent();
        }
    }
    // 删除某个元素
    public void Remove(int id)
    {
        if (id >= cellCount)
            return;

        SetCellCout(cellCount - 1);
        ResetContent();

        bool bFind = false;
        foreach (var group in cellGroupList)
        {
            if (group.HadCell(id))
            {
                bFind = true;
                break;
            }
        }
        if (bFind)
        {
            foreach (var group in cellGroupList)
            {
                if((group.Id + 1)*Group >= id)
                {
                    group.UpdatePosition(group.LocalPosition);
                    group.OnCallBack();
                }
            }
        }
    }

    void ResetContent()
    {
        if (direct == ScrollDir.DIR_VERTICAL)
        {
            float height = cellSize.y + spacingSize.y;
            scrollRect.content.sizeDelta = new Vector2(scrollRect.content.sizeDelta.x, groupCount * height);
        }
        else
        {
            float width = cellSize.x + spacingSize.x;
            scrollRect.content.sizeDelta = new Vector2(groupCount * width, scrollRect.content.sizeDelta.y);
        }
    }
    void ClearAllCell()
    {
        foreach(var group in cellGroupList)
            group.Clear();
        cellGroupList.Clear();
        cellList.Clear();
    }

    public void ReloadData()
    {
        CheckAndUpdateCellList();
    }

    Rect offsetRect = new Rect();
    Vector2 localPos = new Vector2();
    void CheckAndUpdateCellList()
    {
        if (curDirPos.y > 0 || curDirPos.x < 0)
        {
            if (curLastId < cellCount - 1)
            {
                Rect viewRect = scrollRect.viewport.rect;
                TableCellGroup firstCell = cellGroupList.First.Value;
                localPos = firstCell.LocalPosition;
                var pos = (scrollRect.content.anchoredPosition + localPos);
                if (direct == ScrollDir.DIR_VERTICAL)
                    offsetRect.position = viewRect.position - pos;
                else
                    offsetRect.position = viewRect.position + pos;
                offsetRect.size = firstCell.Size;
                if (!viewRect.Overlaps(offsetRect))
                {
                    //Debug.Log("llastId : " + curLastId);
                    var lastPos = cellGroupList.Last.Value.LocalPosition;
                    curLastId++;
                    firstCell.Id = curLastId;
                    cellGroupList.RemoveFirst();
                    if (direct == ScrollDir.DIR_VERTICAL)
                        lastPos.y -= cellSize.y + spacingSize.y;
                    else
                        lastPos.x += cellSize.x + spacingSize.x;
                    firstCell.UpdatePosition(lastPos);
                    firstCell.OnCallBack();
                    cellGroupList.AddLast(firstCell);
                }
            }
        }
        else if (curDirPos.y < 0 || curDirPos.x > 0)
        {
            var curFirstId = curLastId - cellGroupList.Count;
            if (curFirstId + 1 > 0)
            {
                Rect viewRect = scrollRect.viewport.rect;
                TableCellGroup lastCell = cellGroupList.Last.Value;
                localPos = lastCell.LocalPosition;
                var pos = (scrollRect.content.anchoredPosition + localPos);
                if (direct == ScrollDir.DIR_VERTICAL)
                    offsetRect.position = viewRect.position - pos;
                else
                    offsetRect.position = viewRect.position + pos;
                offsetRect.size = lastCell.Size;
                if (!viewRect.Overlaps(offsetRect))
                {
                    //Debug.Log("lastId : " + curFirstId);
                    var firstPos = cellGroupList.First.Value.LocalPosition;
                    curLastId--;
                    lastCell.Id = curFirstId;
                    cellGroupList.RemoveLast();
                    if (direct == ScrollDir.DIR_VERTICAL)
                        firstPos.y += cellSize.y + spacingSize.y;
                    else
                        firstPos.x -= cellSize.x + spacingSize.x;
                    lastCell.UpdatePosition(firstPos);
                    lastCell.OnCallBack();
                    cellGroupList.AddFirst(lastCell);
                }
            }
        }
    }

    public TableCell CreateCell()
    {
        TableCell cell = null;
        if (cellPool.Count > 0)
        {
            cell = cellPool[0];
            cellPool.RemoveAt(0);
            return cell;
        }
        cell = new TableCell();
        cell.cellObj = GameObject.Instantiate(cellPrefab, scrollRect.content);
        return cell;
    }

    public void PutCell(TableCell cell)
    {
        cellPool.Add(cell);
    }

    void Update()
    {
        if (scrollRect != null)
        {
            Vector2 dirPos = scrollRect.content.anchoredPosition - prevPos;
            if (dirPos.x != 0 || dirPos.y != 0)
            {
                curDirPos = dirPos;
                prevPos = scrollRect.content.anchoredPosition;
                CheckAndUpdateCellList();
            }
        }
    }
}
