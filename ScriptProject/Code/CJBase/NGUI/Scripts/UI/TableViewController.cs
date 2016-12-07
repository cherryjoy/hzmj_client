using System;
using UnityEngine;
using System.Collections.Generic;
using UniLua;

public class TableViewController : MonoBehaviour
{
    public enum Direction
    {
        Vertical,
        Horizental,
    }

    public enum MoveType{
        Free,
        CellStep,
    }

    public Direction TableDirection = Direction.Vertical;
    public MoveType CellMoveType = MoveType.Free;
    public GameObject CellPrefab;
    public UIPanel ClipPanel;
    public GameObject DragTarget;
    public LuaBehaviour Controller;
    private Transform mDragTargetTrans;
    public GameObject SrollBtn;
    public int m_CellScpace = 5;
    public int mMomentumAmount = 25;
    [NonSerialized]
    public UILineFlowLayout LineFlowLayout;
    private List<GameObject> mCellList = new List<GameObject>();
    private int mDataCount;
    private Vector4 mClipRange;
    private int mVisualCellCount;
    private int mMaxVisualCellCount = -1;
    private Vector3 mOrgPos;
    private Vector3 mCachePos;
    private bool mBtnVisible = true;
    [HideInInspector]
    public float mCellHeight = -1;
    [HideInInspector]
    public int moveBottomCount;
    public bool isNeedInitDefault = true;
    public string TableViewName = "";
    public string CellPreName = "";
    public bool AlwaysCanDrag = false;

    public GameObject PreBtn;
    public GameObject NextBtn;

    public int CellCount
    {
        get
        {
            return mVisualCellCount;
        }
    }

    public int MaxVisualCellCount {
        get {
            if(mMaxVisualCellCount<0){
                Bounds bound = NGUIMath.CalculateRelativeWidgetBounds(CellPrefab.transform, CellPrefab.transform,true);
                float boundHeight = TableDirection == Direction.Vertical ? bound.size.y : bound.size.x;
                mCellHeight = boundHeight + LineFlowLayout.space;
                float clipRangeHeight = TableDirection == Direction.Vertical ? mClipRange.w : mClipRange.z;
                mMaxVisualCellCount = (int)Math.Ceiling(clipRangeHeight / mCellHeight) + 1;
            }
            
            return mMaxVisualCellCount;
        }
    }

    void Awake()
    {

        if (isNeedInitDefault) Init();
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

    SpringPosition sprPos = null;
    SpringPosition GetMySpringPosition()
    {
        if (sprPos == null)
        {
            sprPos = gameObject.GetComponent<SpringPosition>();
        }
        return sprPos;
    }

    void SetArrowBtnState(int offsetFromOrg)
    {
        if (PreBtn == null || NextBtn == null)
            return;
        if (mDataCount != mVisualCellCount)
        {
            if (moveBottomCount == 0)
            {
                NGUITools.SetActiveSelf(PreBtn, false);
            }
            else if (mDataCount == moveBottomCount + mVisualCellCount)
            {
                NGUITools.SetActiveSelf(NextBtn, false);
            }
            else
            {
                NGUITools.SetActiveSelf(PreBtn, true);
                NGUITools.SetActiveSelf(NextBtn, true);
            }
        }
        else
        {
            if (offsetFromOrg == 0)
            {
                NGUITools.SetActiveSelf(PreBtn, false);
                NGUITools.SetActiveSelf(NextBtn, true);
            }
            else
            {
                NGUITools.SetActiveSelf(PreBtn, true);
                NGUITools.SetActiveSelf(NextBtn, false);
            }
        }
    }

    public void Init()
    {
        if (DragTarget == null)
        {
            DragTarget = gameObject;
        }
        mDragTargetTrans = DragTarget.transform;  
        if (ClipPanel == null || ClipPanel.clipping != UIDrawCall.Clipping.SoftClip)
        {
            return;
        }
        mClipRange = ClipPanel.clipRange;
        mOrgPos = DragTarget.transform.localPosition;
        mCachePos = DragTarget.transform.localPosition;
    }

    public void MoveDragObjToEnd() {
        DragTarget.transform.localPosition = GetEndLocalPos(); 
    }

    Vector3 GetEndLocalPos() {
        float distanceToEnd = mDataCount * mCellHeight - mClipRangeHeight;
        Vector3 moveVec = TableDirection == Direction.Vertical ? new Vector3(0, distanceToEnd, 0) : new Vector3(-distanceToEnd, 0, 0);

        return mOrgPos + moveVec; 
    }

    public void SetDataSources(int count,bool isStatic = false)
    {
        ClearAndReInit();
        mDataCount=count;
        if ( mDataCount > 0)
        {
            InitCells(isStatic);
        }
        else
        {
            mVisualCellCount = 0;
        }
    }

    void ClearAndReInit()
    {
        SpringPosition tp = DragTarget.GetComponent<SpringPosition>();
        if (tp != null)
        {
            Destroy(tp);
        }
        moveBottomCount = 0;
        for (int i = 0; i < mCellList.Count; i++)
        {
            GameObject.Destroy(mCellList[i].gameObject);
        }
        LineFlowLayout = gameObject.GetComponent<UILineFlowLayout>();
        LineFlowLayout.Style = TableDirection == Direction.Vertical ? 1 : 0;
        LineFlowLayout.childrens = new List<UILineFlowLayoutChild>();
        LineFlowLayout.space = m_CellScpace;
        mCellList = new List<GameObject>();
        DragTarget.transform.localPosition = mOrgPos;
        mCachePos = mOrgPos;
        mDataCount = 0;
    }

    #region refresh cells data
    public void ChangeDataSources(int count) {
        int LastDataCount = mDataCount;
        mDataCount = count;
        if (count >= mVisualCellCount)
        {
            if (count >= LastDataCount)
            {
                if (LastDataCount <= MaxVisualCellCount)
                {
                    SetDataSources(mDataCount);
                }
                else {
                    for (int i = moveBottomCount; i < moveBottomCount + mVisualCellCount; i++)
                    {
                        CallLua(i);
                    }
                }
            }
            else {
                if (LastDataCount != moveBottomCount + mVisualCellCount)
                {
                    for (int i = moveBottomCount; i < moveBottomCount + mVisualCellCount; i++)
                    {
                        CallLua(i);
                    }
                }
                else
                {
                    //get the bottom of data
                    if (LastDataCount >= mVisualCellCount)
                    {
                        int minuCount = LastDataCount - count;

                        int lastIndex = LineFlowLayout.childrens.Count - 1;
                        int cacheCount = moveBottomCount;
                        Vector3 cellHeight = new Vector3(0, mCellHeight, 0);

                        for (int i = 0; i < minuCount; i++)
                        {
                            Vector3 pos = LineFlowLayout.childrens[0].tran.localPosition;
                            LineFlowLayout.childrens.RemoveAt(lastIndex);
                            int removeIndex = (cacheCount - 1) % mVisualCellCount;
                            mCellList[removeIndex].transform.localPosition = pos + cellHeight;
                            LineFlowLayout.childrens.Insert(0, new UILineFlowLayoutChild(mCellList[removeIndex].transform, false));
                            moveBottomCount--;
                            cacheCount--;
                        }

                        for (int i = moveBottomCount; i < moveBottomCount + mVisualCellCount; i++)
                        {
                            CallLua(i);
                        }
                        
                        Vector3 dragPos = DragTarget.transform.localPosition;
                        DragTarget.transform.localPosition = new Vector3(dragPos.x, dragPos.y - mCellHeight * minuCount, dragPos.z);
                        SpringPosition sprPos = GetMySpringPosition();
                        if(sprPos != null)
                            sprPos.target = DragTarget.transform.localPosition;
                    }
                    else
                    {
                        SetDataSources(mDataCount);
                    }

                }
            }
        }
        else {
            SetDataSources(count);
        }
    
    }

    public void RemoveData(int index) {
        if (index < 0 || index > mDataCount)
        {
            Debug.LogError("Remove Data index " + index + " out of range[0," + mDataCount + "]!");
            return;
        }
        if (mDataCount != moveBottomCount + mVisualCellCount)
        {
            mDataCount--;
            if (index >= moveBottomCount && index < moveBottomCount + mVisualCellCount)
            {
                for (int i = index; i < moveBottomCount + mVisualCellCount; i++)
                {
                    CallLua(i);
                }
            }

        }
        else {
            //get the bottom of data
            mDataCount--;
            if (mDataCount >= mVisualCellCount)
            {
                int lastIndex = LineFlowLayout.childrens.Count - 1;
                int cacheCount = moveBottomCount;
                Vector3 cellHeight = new Vector3(0, mCellHeight, 0);

                Vector3 pos = LineFlowLayout.childrens[0].tran.localPosition;
                LineFlowLayout.childrens.RemoveAt(lastIndex);
                int removeIndex = (moveBottomCount - 1) % mVisualCellCount;
                mCellList[removeIndex].transform.localPosition = pos + cellHeight;
                LineFlowLayout.childrens.Insert(0, new UILineFlowLayoutChild(mCellList[removeIndex].transform, false));
                CallLua(moveBottomCount - 1);
                moveBottomCount--;

                if (index >= moveBottomCount && index < moveBottomCount + mVisualCellCount - 1)
                {
                    for (int i = index; i < moveBottomCount + mVisualCellCount - 1; i++)
                    {
                        CallLua(i);
                    }
                }

                Vector3 dragPos = DragTarget.transform.localPosition;

                TweenPosition posTw = TweenPosition.Begin(DragTarget, 0.2f, new Vector3(dragPos.x, dragPos.y - mCellHeight, dragPos.z));
                posTw.callWhenFinished = "MoveEnd";
            }
            else {
                SetDataSources(mDataCount);
            }
            
        }
    }

    void MoveEnd(LuaMessage msg)
    {
        Debug.Log("Move End");
    }

    public void AddData(int index) {
        if (index > mDataCount+1) {
            Debug.LogError("Add Data index " + index + " out of range[0,"+mDataCount+"+ 1]!");
            return;
        }
        mDataCount++;
        if (mDataCount <= MaxVisualCellCount)
        {
            SetDataSources(mDataCount);
            if (mDataCount == MaxVisualCellCount || mDataCount == MaxVisualCellCount-1)
                TweenMoveToEnd();
        }
        else {
            if (index >= moveBottomCount && index < moveBottomCount + mVisualCellCount)
            {
                for (int i = index; i < moveBottomCount + mVisualCellCount; i++)
                {
                    CallLua(i);
                }
            }

            if (moveBottomCount + mVisualCellCount >= mDataCount - 1)
            {
                TweenMoveToEnd();
            }
        }
    }

    void TweenMoveToEnd()
    {
        TweenPosition posTw = TweenPosition.Begin(DragTarget, 0.2f, GetEndLocalPos());
        posTw.callWhenFinished = "MoveEnd";
    }

    #endregion

    int cells_ref = LuaAPI.LUA_REFNIL;
    int set_cell_func_ref = LuaAPI.LUA_REFNIL;
    float mClipRangeHeight;
    private void InitCells(bool isStatic)
    {
        SetLuaSetCellFuncRef();
        SetLuaCellsCountRef(2);
        SetLuaCellsTableRef();

        GameObject cell = SetUpCell(0);
        if (cell == null)
            return;

        Bounds bound = NGUIMath.CalculateRelativeWidgetBounds(cell.transform);
        if (mCellHeight <= 0)
        {
            float boundHeight = TableDirection == Direction.Vertical ? bound.size.y : bound.size.x;
            mCellHeight = boundHeight + LineFlowLayout.space;
        }
        mClipRangeHeight = TableDirection == Direction.Vertical ? mClipRange.w : mClipRange.z;
        SetVisualCellCount(mClipRangeHeight, isStatic);
        SetLuaCellsCountRef(mVisualCellCount);
        
        for (int i = 1; i < mVisualCellCount; i++)
        {
            SetUpCell(i);
        }

        if (AlwaysCanDrag || mBtnVisible)
        {
            InitDragCells();
            SetArrowBtnState(0);
        }
        else
        {
            if (PreBtn!=null&&NextBtn!=null)
            {
                NGUITools.SetActiveSelf(PreBtn, false);
                NGUITools.SetActiveSelf(NextBtn, false);
            }
        }

        LineFlowLayout.RefreshNow();
           
    }

    private void CallLua(int index)
    {
        LuaState lua_ = LuaInstance.instance.Get();
        lua_ = LuaInstance.instance.Get();
        lua_.RawGetI(LuaAPI.LUA_REGISTRYINDEX, set_cell_func_ref);
        lua_.RawGetI(LuaAPI.LUA_REGISTRYINDEX, Controller.Object_ref);
        lua_.PushInteger(index);
        lua_.PCall(2, 0, 0);
    }

    private GameObject SetUpCell(int index) {
        GameObject cell = GetCell(index);
        if (cell == null)
        {
            Debug.LogWarning("CellPrefab is Null!!");
            GameObject.Destroy(cell);
        }
        CallLua(index);
        mCellList.Add(cell);
        LineFlowLayout.AddChild(cell);
        return cell;
    }

    private GameObject GetCell(int cellNum){
        float space = cellNum == 0? 0:LineFlowLayout.space;
        Vector3 finalLocalPos = new Vector3(0, cellNum *( space - mCellHeight), 0);
        CellPrefab.transform.position = finalLocalPos;
        GameObject cell = NGUITools.AddChildNotLoseAnything(gameObject, CellPrefab);
     
        cell.name = CellPreName + "Cell" + cellNum;
        cell.SetActive(true);
        AddLuaCellToCtr(cell);

        return cell;
    }

    private void SetLuaCellsTableRef() {
        LuaState lua_ = LuaInstance.instance.Get();
        lua_.RawGetI(LuaAPI.LUA_REGISTRYINDEX, Controller.Object_ref);
        lua_.NewTable();
        lua_.PushValue(-1);
        if (cells_ref != LuaAPI.LUA_REFNIL)
            lua_.L_Unref(LuaAPI.LUA_REGISTRYINDEX,ref cells_ref);
        cells_ref = lua_.L_Ref(LuaAPI.LUA_REGISTRYINDEX);
        lua_.SetField(-2, "Cells" + TableViewName);
        lua_.Pop(1);
    }

    private void SetLuaCellsCountRef(int cellCount) {
        LuaState lua_ = LuaInstance.instance.Get();
        lua_.RawGetI(LuaAPI.LUA_REGISTRYINDEX, Controller.Object_ref);
        lua_.PushInteger(cellCount);
        lua_.SetField(-2, "CellsCount" + TableViewName);
        lua_.Pop(1);
        
    }

    private void SetLuaSetCellFuncRef() {
        LuaState lua_ = LuaInstance.instance.Get();
        lua_.GetGlobal(Controller.ScriptName);
        lua_.GetField(-1, "SetCellData" + TableViewName);
        if (set_cell_func_ref != LuaAPI.LUA_REFNIL)
            lua_.L_Unref(LuaAPI.LUA_REGISTRYINDEX,ref set_cell_func_ref);
        set_cell_func_ref = lua_.L_Ref(LuaAPI.LUA_REGISTRYINDEX);
        lua_.Pop(1);
    }

    private void SetVisualCellCount(float clipRangeHeight,bool isStatic = false) {
        mVisualCellCount = (int)Math.Ceiling(clipRangeHeight / mCellHeight) + 1;

        mBtnVisible = true;
        if (mDataCount < mVisualCellCount || isStatic)
        {
            if (mDataCount < mVisualCellCount && clipRangeHeight >= (mCellHeight * mDataCount))
            {
                mBtnVisible = false;
            }

            mVisualCellCount = mDataCount;
        }

    }


    private void InitDragCells()
    {
        for (int i = 0; i < mCellList.Count; i++)
        {
            InitDragCell(mCellList[i]);
        }
    }

    UIDragObject scrollDragObjet;
    private void InitDragCell(GameObject cell)
    {
        UIDragObject dragObject = cell.GetComponent<UIDragObject>();
        if (dragObject == null)
        {
            dragObject = cell.gameObject.AddComponent<UIDragObject>();
            dragObject.backTop = true;
            dragObject.restrictWithinPanel = true;
            dragObject.momentumAmount = mMomentumAmount;
        }
        dragObject.target = DragTarget.transform;
        dragObject.scale = TableDirection == Direction.Vertical ? new Vector3(0, 1, 0) : new Vector3(1, 0, 0);

        if (SrollBtn != null)
        {
            if (scrollDragObjet == null)
            {
                scrollDragObjet = cell.gameObject.AddComponent<UIDragObject>();
            }
            scrollDragObjet.target = SrollBtn.transform;
            scrollDragObjet.scale = new Vector3(0, -1, 0);
            scrollDragObjet.restrictWithinPanel = true;
          
            scrollDragObjet = null;
        }

    }

    void Update()
    {
        if (mDragTargetTrans.localPosition == mCachePos)
        {
            return;
        }

        RefreshData();
    }

    public void RefreshData()
    {
        mCachePos = mDragTargetTrans.localPosition;
        float offsetFromOgr = TableDirection == Direction.Vertical ? mCachePos.y - mOrgPos.y : mOrgPos.x - mCachePos.x;
        int offsetFromOrgCount = (int)(Math.Abs(offsetFromOgr) / mCellHeight);
        if (offsetFromOgr > 0 && mVisualCellCount != mDataCount)
        {
            InsertBottom(offsetFromOrgCount);
        }
        SetArrowBtnState(offsetFromOrgCount);
    }

    void InsertBottom(int offsetFromOgr)
    {
    //    Debug.Log(moveBottomCount);
        int lastIndex = LineFlowLayout.childrens.Count - 1;
        int cacheCount = moveBottomCount;
        Vector3 cellHeight = TableDirection == Direction.Vertical ? new Vector3(0, mCellHeight, 0) : new Vector3(mCellHeight, 0, 0);
        for (int i = cacheCount; i > offsetFromOgr; i--)
        {
            Vector3 pos = LineFlowLayout.childrens[0].tran.localPosition;
            LineFlowLayout.childrens.RemoveAt(lastIndex);
            int removeIndex = (i - 1) % mVisualCellCount;
            mCellList[removeIndex].transform.localPosition = TableDirection == Direction.Vertical ? pos + cellHeight : pos - cellHeight;
            LineFlowLayout.childrens.Insert(0, new UILineFlowLayoutChild(mCellList[removeIndex].transform, false));
            CallLua(i-1);
            moveBottomCount--;
        }

        for (int i = 0; i < offsetFromOgr; i++)
        {
            if (i < moveBottomCount || mVisualCellCount + i >= mDataCount)
            {
                continue;
            }

            LineFlowLayout.childrens.RemoveAt(0);
            LineFlowLayout.AddChild(mCellList[i % mVisualCellCount]);
            if(i >=LineFlowLayout.childrens.Count-2)
                LineFlowLayout.RefreshNow();
            CallLua(mVisualCellCount + i);
            moveBottomCount++;
        }
        
        LineFlowLayout.mRefreshCount += 1;
    }

    void AddLuaCellToCtr(GameObject obj) {
        LuaState lua_ = LuaInstance.instance.Get();
        lua_.RawGetI(LuaAPI.LUA_REGISTRYINDEX, LuaInstance.instance.table_table_ref);
        lua_.GetField(-1, "insert");
        lua_.RawGetI(LuaAPI.LUA_REGISTRYINDEX, cells_ref);
        LuaBehaviour cellLB = obj.GetComponent<LuaBehaviour>();
        lua_.RawGetI(LuaAPI.LUA_REGISTRYINDEX, cellLB.Object_ref);
        lua_.PCall(2, 0, 0);
    }
}
