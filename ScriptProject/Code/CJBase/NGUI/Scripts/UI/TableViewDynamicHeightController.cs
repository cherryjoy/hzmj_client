using System;
using UnityEngine;
using System.Collections.Generic;
using UniLua;
using System.Collections;

// 记录每个cell的高度
// 排列显示cell
// 拉动时改变显示的cell
public class TableViewDynamicHeightController : MonoBehaviour
{
    #region Filed
    public GameObject cellPrefab;
    public UIPanel clipPanel; // Panel
    public GameObject dragTarget; // UIDragObj
    public LuaBehaviour controller; // lua脚本
    private Transform dragTargetTrans; // drag坐标
    public int cellSpace = 10; // 行间距
    public int momentumAmount = 25; // drag的参数
    //[NonSerialized]
    public List<GameObject> cellList = new List<GameObject>(); // 显示的cell对象
    private int dataCount; // 数据源的个数
    private Vector4 clipRange; // 裁剪区域
    private Vector3 orgPos; // dragTarget的初始坐标
    private Vector3 cachePos; // dragTarget被拖动后的位置
    public string tableViewName = "";
    public string cellPreName = "";
    private List<float> rowHeights = new List<float>();
    private Range lastRange = new Range();
    public Queue<GameObject> cellPool = new Queue<GameObject>(); // cell缓冲池
    #endregion

    #region MonoBehaviour Functions
    void Awake()
    {
        Init();
    }

    void OnDestroy() {
        ClearLuaRef();
    }

    void ClearLuaRef() {
        LuaState lua_ = LuaInstance.instance.Get();
        if (set_cell_func_ref != LuaAPI.LUA_REFNIL)
            lua_.L_Unref(LuaAPI.LUA_REGISTRYINDEX,ref set_cell_func_ref);
        if (cells_ref != LuaAPI.LUA_REFNIL)
            lua_.L_Unref(LuaAPI.LUA_REGISTRYINDEX, ref cells_ref);
    }

    void Update()
    {
        if (dragTargetTrans.localPosition == cachePos)
        {
            return;
        }

        if (dragTargetTrans.localPosition.y - orgPos.y > 1)
        {
            RefreshData(true);
        }
    }

    #endregion

    #region Public Functions
    public void Init()
    {
        if (dragTarget == null)
        {
            dragTarget = gameObject;
        }

        dragTargetTrans = dragTarget.transform;
        if (clipPanel == null || clipPanel.clipping != UIDrawCall.Clipping.SoftClip)
        {
            return;
        }

        clipRange = clipPanel.clipRange;
        orgPos = dragTarget.transform.localPosition;
        cachePos = dragTarget.transform.localPosition;
    }

    public void ClearAndReInit(bool exceptHight)
    {
        SpringPosition tp = dragTarget.GetComponent<SpringPosition>();
        if (tp != null)
        {
            Destroy(tp);
        }

        for (int i = 0; i < cellList.Count; ++i)
        {
            EnqueueItemInPool(cellList[i]);
        }

        cellList = new List<GameObject>();
        if (!exceptHight)
        {
            rowHeights = new List<float>();
            dataCount = 0;
        }

        dragTarget.transform.localPosition = orgPos;
        cachePos = orgPos;
    }

    /// <summary>
    /// 设置数据源的高度
    /// </summary>
    /// <param name="hight"></param>
    public void SetDataSourceHight(float hight)
    {
        rowHeights.Add(hight);
        ++dataCount;
    }

    /// <summary>
    /// 添加动态高度的cell列表
    /// </summary>
    /// <param name="startIndex">从0开始</param>
    public void SetPreHightDataSources(int startIndex)
    {
        ClearAndReInit(true);
        if (dataCount > 0)
        {
            InitPreHightCells(dataCount, startIndex);
        }
    }

    /// <summary>
    /// 添加动态高度的cell，主要用于需要自动向上滚动的功能，例如聊天
    /// </summary>
    /// <param name="count"></param>
    public void AddDataSource()
    {
        if (dataCount == 0)
        {
            SetLuaSetCellFuncRef();
            SetLuaCellsTableRef();
        }

        SpringPosition tp = dragTarget.GetComponent<SpringPosition>();
        if (tp != null)
        {
            tp.enabled = false;
        }

        //SpringPosition
        // 先校正dragTarget的pos
        ReviseDragTargetPos(rowHeights.Count);
        if (IsRoll())
        {
            RefreshData(false);
        }

        GameObject cell = SetUpCell(dataCount, false);
        if (cell == null)
        {
            return;
        }

        Bounds bound = NGUIMath.CalculateRelativeWidgetBounds(cell.transform);
        float cellHeight = bound.size.y + cellSpace;
        // Debug.Log("cellHeight: " + cellHeight);
        rowHeights.Add(cellHeight);

        // 如果超过裁剪区域，这儿也要计算往上移动的高度。
        Range range = CalculateVisibleRowRange(dragTargetTrans.localPosition.y - orgPos.y + cellHeight, false);
        if (IsRoll())
        {
            ReviseDragTargetPos(rowHeights.Count);
            //Debug.Log("lastRang from: " + lastRange.from + ", lastRang count: " + lastRange.count + "\nrang from: " + range.from + ", rang count: " + range.count);
            RemoveInvisibleCell(lastRange, range);
        }

        lastRange = range;
        ++dataCount;
    }

    /// <summary>
    /// 移除一个cell，重新设置该行以下已有cell，并且计算它们的高度，多退少补。
    /// 适用于向下滚动的列表
    /// </summary>
    /// <param name="row">行数，从0开始</param>
    public void RemoveData(int row)
    {
        if (rowHeights.Count == 0)
        {
            return;
        }

        if (row < lastRange.from || row >= lastRange.from + lastRange.count)
        {
            return;
        }

        --dataCount;
        for (int i = row; i < rowHeights.Count - 1; ++i)
        {
            rowHeights[i] = rowHeights[i + 1];
        }

        rowHeights.RemoveAt(rowHeights.Count - 1);

        Range range = CalculateVisibleRowRange(dragTargetTrans.localPosition.y - orgPos.y, false);
        float needMovehight = 0;
        for (int i = range.from; i < row; ++i)
        {
            needMovehight += rowHeights[i];
        }

        needMovehight = clipRange.w - needMovehight;
        // 在后面补上相应的数据，得判断需要补多少个
        float moveHight = 0;
        int num = row;
        for (; num < range.from + range.count; ++num)
        {
            if (moveHight > needMovehight)
            {
                break;
            }

            if (num >= dataCount)
            {
                break;
            }

            int index = num - range.from;
            CallLua(index, num);
            moveHight += rowHeights[num];
            // 修正坐标位置
            if (index > 0 && num > 0)
            {
                cellList[index].transform.localPosition = new Vector3(0, -TotalHeight(num), 0);
            }
        }

        RemoveInvisibleCell(lastRange, range);

        while (moveHight < needMovehight)
        {
            if (num >= dataCount)
            {
                break;
            }

            //Debug.Log("RemoveData SetUpCell: " + num + ", dataCount: " + dataCount);
            GameObject cell = SetUpCell(num, false);
            moveHight += rowHeights[num];
            ++num;
        }

        lastRange = range;
    }

    #endregion

    #region Private Functions
    // 计算要显示的元素
    private void RefreshData(bool isOverBottom)
    {
        cachePos = dragTargetTrans.localPosition;
        float offsetFromOrg = cachePos.y - orgPos.y;
        Range range = CalculateVisibleRowRange(offsetFromOrg, isOverBottom);
        if (range.from == lastRange.from && range.count == lastRange.count)
        {
            return;
        }

        //Debug.Log("RefreshData **lastRang from: " + lastRange.from + ", lastRang count: " + lastRange.count + "\nrang from: " + range.from + ", rang count: " + range.count);
        RemoveInvisibleCell(lastRange, range);
        CreateVisiblieCell(lastRange, range);
        
        lastRange = range;
    }

    private void RemoveInvisibleCell(Range lastRange, Range range)
    {
        if (range.from > lastRange.from)  // 回收
        {
            for (int i = lastRange.from, j = 0; i < range.from; ++i, ++j)
            {
                if (j < lastRange.count)
                {
                    RemoveCell(0);
                }
            }
        }
        else
        {
            for (int i = lastRange.from + lastRange.count - 1, j = 0; i > range.from + range.count - 1; --i, ++j)
            {
                if (j < lastRange.count)
                {
                    RemoveCell(cellList.Count - 1);
                }
            }
        }
    }

    private void CreateVisiblieCell(Range lastRange, Range range)
    {
        if (range.from + range.count > lastRange.from + lastRange.count) // 生成
        {
            int i = 0;
            if (lastRange.from + lastRange.count < range.from)
            {
                i = range.from;
            }
            else
            {
                i = lastRange.from + lastRange.count;
            }

            for (int j = 0; i < range.from + range.count; ++i, ++j)
            {
                if (j < range.count)
                {
                    SetUpCell(i, false);
                }
            }
        }
        else
        {
            int i = 0;
            if (range.from + range.count < lastRange.from)
            {
                i = range.from + range.count - 1;
            }
            else
            {
                i = lastRange.from - 1;
            }

            for (int j = 0; i > range.from - 1; --i, ++j)
            {
                if (j < range.count)
                {
                    SetUpCell(i, true);
                }
            }
        }
    }

    // 如果有拖动，计算可显示的个数，如果它不等于数据源的个数，需要在底部插入
    // 需要处理拖动到底部的情况
    private Range CalculateVisibleRowRange(float moveHight, bool isOverBottom)
    {
        float tmpHight = 0;
        int startIndex = 0;
        int endIndex = 0;
        if (rowHeights.Count == 0)
        {
            return new Range(0, 0);
        }

        for (int i = 0; i < rowHeights.Count; ++i)
        {
            tmpHight += rowHeights[i];
            if (tmpHight > moveHight + clipRange.w)
            {
                endIndex = i;
                break;
            }

            if (i == rowHeights.Count - 1)
            {
                if (isOverBottom)
                {
                    return lastRange;
                }
                else
                {
                    endIndex = rowHeights.Count - 1;
                }
            }
        }

        tmpHight = 0;

        for (int i = 0; i < rowHeights.Count; ++i)
        {
            tmpHight += rowHeights[i];
            if (tmpHight > moveHight)
            {
                startIndex = i;
                break;
            }
        }

        return new Range(startIndex, endIndex - startIndex + 1);
    }

    private bool IsRoll()
    {
        float height = 0;
        for (int i = 0; i < rowHeights.Count; ++i)
        { 
            height += rowHeights[i];
            if (height > clipRange.w)
            {
                return true;
            }
        }  
        
        return false;
    }

    private float TotalHeight(int end)
    {
        float height = 0;
        for (int i = 0; i < end && i < rowHeights.Count; ++i)
        {
            height += rowHeights[i];
        }

        return height;
    }

    private void ReviseDragTargetPos(int num)
    {
        float tmpHeight = TotalHeight(rowHeights.Count) - clipRange.w;
        float reviseHeight = TotalHeight(num);
        reviseHeight = reviseHeight <= tmpHeight ? reviseHeight : tmpHeight;
        if (reviseHeight > 0)
        {
            dragTargetTrans.localPosition = orgPos + new Vector3(0, reviseHeight, 0);
        }
    }

    int set_cell_func_ref = LuaAPI.LUA_REFNIL;
    int cells_ref = LuaAPI.LUA_REFNIL;

    private void InitPreHightCells(int count, int startIndex)
    {
        SetLuaSetCellFuncRef();
        SetLuaCellsTableRef();

        // 计算需要移动的高度
        ReviseDragTargetPos(startIndex);
        // 计算可以显示的cell数量
        Range range = CalculateVisibleRowRange(dragTargetTrans.localPosition.y - orgPos.y, false);
        for (int i = range.from; i < range.from + range.count; ++i)
        {
            SetUpCell(i, false);
        }

        lastRange = range;
    }

    private void SetLuaSetCellFuncRef()
    {
        LuaState lua = LuaInstance.instance.Get();
        lua.GetGlobal(controller.ScriptName);
        lua.GetField(-1, "SetCellData" + tableViewName);
        if (set_cell_func_ref != LuaAPI.LUA_REFNIL)
        {
            lua.L_Unref(LuaAPI.LUA_REGISTRYINDEX, ref set_cell_func_ref);
        }

        set_cell_func_ref = lua.L_Ref(LuaAPI.LUA_REGISTRYINDEX);
        lua.Pop(1);
    }

    private void SetLuaCellsTableRef()
    {
        LuaState lua = LuaInstance.instance.Get();
        lua.RawGetI(LuaAPI.LUA_REGISTRYINDEX, controller.Object_ref);
        lua.NewTable();
        lua.PushValue(-1);
        if (cells_ref != LuaAPI.LUA_REFNIL)
        {
            lua.L_Unref(LuaAPI.LUA_REGISTRYINDEX, ref cells_ref);
        }

        cells_ref = lua.L_Ref(LuaAPI.LUA_REGISTRYINDEX);
        lua.SetField(-2, "Cells" + tableViewName);
        lua.Pop(1);
    }

    private void AddLuaCellToCtr(GameObject obj, int index = 0)
    {
        LuaState lua = LuaInstance.instance.Get();
        lua.RawGetI(LuaAPI.LUA_REGISTRYINDEX, LuaInstance.instance.table_table_ref);
        lua.GetField(-1, "insert");
        lua.RawGetI(LuaAPI.LUA_REGISTRYINDEX, cells_ref);
        if (index > 0)
        {
            lua.PushInteger(index);
            LuaBehaviour luaBehav = obj.GetComponent<LuaBehaviour>();
            lua.RawGetI(LuaAPI.LUA_REGISTRYINDEX, luaBehav.Object_ref);
            lua.PCall(3, 0, 0); // table.insert(luaScript.Cells, index, obj)
        }
        else
        {
            LuaBehaviour luaBehav = obj.GetComponent<LuaBehaviour>();
            lua.RawGetI(LuaAPI.LUA_REGISTRYINDEX, luaBehav.Object_ref);
            lua.PCall(2, 0, 0); // table.insert(luaScript.Cells, obj)
        }
    }

    private void RemoveLuaCellToCtr(int index)
    {
        LuaState lua = LuaInstance.instance.Get();
        lua.RawGetI(LuaAPI.LUA_REGISTRYINDEX, LuaInstance.instance.table_table_ref);
        lua.GetField(-1, "remove");
        lua.RawGetI(LuaAPI.LUA_REGISTRYINDEX, cells_ref);
        lua.PushInteger(index);
        lua.PCall(2, 0, 0); // table.remove(luaScript.Cells, index)
    }

    // 由于在此函数中会调用lua的SetCellData，可能会改变GameObject的大小，所以需要在调用结束后计算一下高度
    private GameObject SetUpCell(int row, bool insertFirst)
    {
        GameObject cell = GetCell(row);
        if (cell == null)
        {
            Debug.LogWarning("cellPrefab is Null!!");
            GameObject.Destroy(cell);
        }

        //Debug.Log("SetUpCell: " + cell.name);
        if (insertFirst)
        {
            AddLuaCellToCtr(cell, 1); // lua 从1开始索引
            cellList.Insert(0, cell);
            //Debug.Log("index: " + index);
            CallLua(0, row);
        }
        else
        {
            AddLuaCellToCtr(cell);
            cellList.Add(cell);
            //Debug.Log("index: " + index);
            CallLua(cellList.Count - 1, row);
        }

        return cell;
    }

    private GameObject GetCell(int cellNum)
    {
        // 计算cell的坐标
        float height = 0;
        for (int i = 0; i < cellNum; ++i)
        {
            height += rowHeights[i];
        }

        cellPrefab.transform.position = new Vector3(0, -height, 0);

        GameObject cell = DequeueItemInPool();
        cell.transform.localPosition = cellPrefab.transform.position;
        cell.name = cellPreName + "Cell" + cellNum;
        return cell;
    }

    private void RemoveCell(int index)
    {
        if (cellList.Count == 0) return;

        //Debug.Log("RemoveCell: " + cellList[index].name);
        EnqueueItemInPool(cellList[index]);
        cellList.RemoveAt(index);
        RemoveLuaCellToCtr(index + 1);
    }

    /// <summary>
    /// 设置cell
    /// </summary>
    /// <param name="index">对应到lua中的cells列表的索引，从0开始</param>
    /// <param name="row">对应整个列表的行数，从0开始</param>
    private void CallLua(int index, int row)
    {
        LuaState lua = LuaInstance.instance.Get();
        //Debug.Log("CallLua index: " + index + ", row :" + row);
        lua = LuaInstance.instance.Get();
        lua.RawGetI(LuaAPI.LUA_REGISTRYINDEX, set_cell_func_ref);
        lua.RawGetI(LuaAPI.LUA_REGISTRYINDEX, controller.Object_ref);
        lua.PushInteger(index);
        lua.PushInteger(row);
        lua.PCall(3, 0, 0); // luaScript.SetCellData(index, row)
    }

    private void EnqueueItemInPool(GameObject obj)
    {
        obj.transform.localPosition = new Vector3(0, 0, 0);
        obj.SetActive(false);
        cellPool.Enqueue(obj);
    }

    private GameObject DequeueItemInPool()
    {
        GameObject obj = null;
        if (cellPool.Count > 0)
        {
            obj = cellPool.Dequeue();
        }
        else
        {
            obj = NGUITools.AddChildNotLoseAnything(gameObject, cellPrefab);
        }

        obj.SetActive(true);
        return obj;
    }

    #endregion
}

public struct Range
{
    public int from;
    public int count;

    public Range(int fromValue, int valueCount)
    {
        from = fromValue;
        count = valueCount;
    }
}