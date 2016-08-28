using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UniLua;

//public enum DragDirection
//{
//    Horizontal,
//    Vertical
//}

public class GridViewController : MonoBehaviour
{
    public GameObject m_CellPrefab;
    public GameObject m_PreBtn;
    public GameObject m_NextBtn;
    public int ShowGridCount = 3;
    public int m_Width = 0;
    public int m_Height = 0;
    public int m_StartPosX = 0;
    public int m_StartPosY = 0;
    public int m_Space = 5;
    public DragDirection m_Direction = DragDirection.Horizontal;
    public LuaBehaviour Controller;

    private int dataCount = 0;
    private List<GameObject> m_GoCacheList = new List<GameObject>();
    private int m_Item_count = 0;
    private int m_CurrtGridIndex = 0;
    private Vector3 m_CachePos = Vector3.zero;
    private List<GameObject> m_BaseList = new List<GameObject>();
    private Vector3 m_LeftRange = Vector3.zero;
    private Vector3 m_RightRange = Vector3.zero;
    private Vector3 m_TopRange = Vector3.zero;
    private Vector3 m_BottomRange = Vector3.zero;
    private float m_GridSize = 0;
    private float m_GridHeightSize = 0;
    private int m_CurrCellCount = 0;
    //private bool pressed = false;

    public void RefreshDataSources(int dataCount)
    {
        this.dataCount = dataCount;
        m_Item_count = dataCount;
        if (m_Item_count <= ShowGridCount)
            m_CurrCellCount = m_Item_count;
        else
            m_CurrCellCount = ShowGridCount + 1;
        CalcPos();
        int index = GetMin();
        for (int i = 0; i < index; i++)
        {
            int location = m_CurrtGridIndex + index + i;
            if (location < dataCount)
                // m_BaseList[i].SetCellData(m_DataSource[location]);
                CallLua(m_BaseList[i], location);
        }
        for (int i = index; i < ShowGridCount + 1; i++)
        {
            int location = m_CurrtGridIndex - index + i;
            if (location < m_BaseList.Count && location >= 0)
                //m_BaseList[i].SetCellData(m_DataSource[location]);
                CallLua(m_BaseList[i], location);
        }
    }

    private void CallLua(GameObject obj, int index)
    {
        LuaBehaviour lb;
        LuaState lua_;
        lb = obj.GetComponentInChildren<LuaBehaviour>();
        lua_ = LuaInstance.instance.Get();
        lua_.GetGlobal(lb.ScriptName);
        lua_.GetField(-1, "SetCellData");
        lua_.RawGetI(LuaAPI.LUA_REGISTRYINDEX, lb.Object_ref);
        lua_.RawGetI(LuaAPI.LUA_REGISTRYINDEX, Controller.Object_ref);
        lua_.PushInteger(index);
        lua_.PCall(3, 0, 0);
        lua_.Pop(1);
    }

    public void SetDataSource(int dataCount)
    {

        m_BaseList.Clear();
        for (int i = 0; i < m_GoCacheList.Count; i++)
        {
            GameObject.Destroy(m_GoCacheList[i]);
        }
        m_GoCacheList.Clear();
        m_CurrtGridIndex = 0;
        Vector3 pos = transform.localPosition;
        transform.localPosition = new Vector3(0, 0, pos.z);

        this.dataCount = dataCount;
        m_Item_count = dataCount;
        m_GridSize = m_Width + m_Space / 2;
        if (m_Direction == DragDirection.Vertical)
        {
            m_GridHeightSize = m_Height + m_Space / 2;
        }

        if (m_Item_count <= ShowGridCount)
            m_CurrCellCount = m_Item_count;
        else
            m_CurrCellCount = ShowGridCount + 1;

        for (int i = 0; i < m_CurrCellCount; i++)
        {
            GameObject go = NGUITools.AddChild(gameObject, m_CellPrefab);
            go.name = "ItemCell";
            m_GoCacheList.Add(go);

            m_BaseList.Add(go);
            if (m_Direction == DragDirection.Horizontal)
                go.transform.localPosition = new Vector3(m_StartPosX + i * m_GridSize, m_StartPosY, 0);
            else
                go.transform.localPosition = new Vector3(m_StartPosX, m_StartPosY - i * m_GridHeightSize, 0);
            if (m_CurrCellCount > ShowGridCount)
            {
                UIDragPage dp = go.GetComponent<UIDragPage>();
                if (dp == null)
                {
                    dp = go.AddComponent<UIDragPage>();
                }

                dp.m_Target = transform;
                dp.DragDirection = m_Direction;
            }
            SpringPosition sp = go.GetComponent<SpringPosition>();
            if (sp != null)
            {
                sp.enabled = false;
            }

            CallLua(m_BaseList[i], i);
        }
        SetBtnState();
        CalcPos();
    }

    void CalcPos()
    {
        if (m_Direction == DragDirection.Horizontal)
        {
            m_LeftRange = GetGirdPos(0);
            m_RightRange = GetGirdPos(m_Item_count - ShowGridCount);
        }
        else
        {
            m_TopRange = GetGirdPos(m_Item_count - ShowGridCount);
            m_BottomRange = GetGirdPos(0);
        }
    }

    void LateUpdate()
    {
        if (dataCount == 0 || dataCount < 1 || m_CurrCellCount <= ShowGridCount)
            return;
        //Debug.Log("======");
        float posX = transform.localPosition.x;
        float posY = transform.localPosition.y;
        if (m_Direction == DragDirection.Horizontal)
        {
            if (posX < m_RightRange.x)
            {
                transform.localPosition = m_RightRange;
                posX = m_RightRange.x;
            }
            else if (posX > m_LeftRange.x)
            {
                transform.localPosition = m_LeftRange;
                posX = m_LeftRange.x;
            }
        }
        else
        {
            if (posY < m_BottomRange.y)
            {
                transform.localPosition = m_BottomRange;
                posY = m_BottomRange.y;
            }
            else if (posY > m_TopRange.y)
            {
                transform.localPosition = m_TopRange;
                posY = m_TopRange.y;
            }
        }
        int grid = 0;
        if (m_Direction == DragDirection.Horizontal)
            grid = (int)System.Math.Round(System.Math.Abs(posX) / m_GridSize);
        else
            grid = (int)System.Math.Round(System.Math.Abs(posY) / m_GridHeightSize);
        if (m_CurrtGridIndex == grid || grid > m_Item_count - ShowGridCount || grid < 0)
        {
            return;
        }
        Debug.Log("current grid: " + grid);
        bool ahead = grid > m_CurrtGridIndex;
        if (ahead)
        {
            //move to left

            int index = GetMin();
            ResetItemPos(ahead, index, grid);
            //GameObject go = m_GoCacheList[index];
            //Vector3 pos = go.transform.localPosition;
            //if (m_Direction == DragDirection.Horizontal)
            //    pos += new Vector3(m_CurrCellCount * m_GridSize, pos.y, pos.z);
            //else
            //    pos -= new Vector3(pos.x, m_CurrCellCount * m_GridHeightSize, pos.z);
            //go.transform.localPosition = pos;
            //if (grid + ShowGridCount < m_DataSource.Count)
            //{
            //    m_BaseList[index].SetCellData(m_DataSource[grid + ShowGridCount]);
            //}
        }
        else
        {
            //move to right
            int index = GetMax();
            ResetItemPos(ahead, index, grid);
            //GameObject go = m_GoCacheList[index];
            //Vector3 pos = go.transform.localPosition;
            //if (m_Direction == DragDirection.Horizontal)
            //    pos -= new Vector3(m_CurrCellCount * m_GridSize, pos.y, pos.z);
            //else
            //    pos += new Vector3(pos.x, m_CurrCellCount * m_GridHeightSize, pos.z);
            //go.transform.localPosition = pos;
            //m_BaseList[index].SetCellData(m_DataSource[grid]);
        }
        m_CurrtGridIndex = grid;
        SetBtnState();
    }


    void ResetItemPos(bool ahead, int index, int grid)
    {
        if (m_Direction == DragDirection.Horizontal)
        {
            int count = m_GoCacheList.Count;

            GameObject go = m_GoCacheList[index];
            Vector3 vp = go.transform.localPosition;

            float offset = 0;
            if (ahead)
                offset = grid == 0 ? 0 : (grid + count - 1) * m_GridSize;
            else
                offset = grid * m_GridSize;
            go.transform.localPosition = new Vector3(m_StartPosX + offset, vp.y);
            if (ahead)
            {
                for (int i = 0; i < count; i++)
                {
                    if (i == index)
                    {
                        if (grid + ShowGridCount < dataCount)
                        {
                            //m_BaseList[index].SetCellData(m_DataSource[grid + ShowGridCount]);
                            CallLua(m_BaseList[index], grid + ShowGridCount);
                        }
                        continue;
                    }
                    go = m_GoCacheList[i];
                    if (i < index)
                    {
                        go.transform.localPosition = new Vector3(m_StartPosX + offset - (index - i) * m_GridSize, vp.y);
                        int datapos = grid + ShowGridCount - (index - i);
                        if (datapos >= 0 && datapos < dataCount)
                           // m_BaseList[i].SetCellData(m_DataSource[datapos]);
                            CallLua(m_BaseList[i], datapos);
                    }
                    else
                    {
                        go.transform.localPosition = new Vector3(m_StartPosX + offset - (count - (i - index)) * m_GridSize, vp.y);
                        int datapos = grid + ShowGridCount - (count - (i - index));
                        if (datapos >= 0 && datapos < dataCount)
                           // m_BaseList[i].SetCellData(m_DataSource[datapos]);
                            CallLua(m_BaseList[i], datapos);
                    }
                }
            }
            else
            {
                for (int i = 0; i < count; i++)
                {
                    if (i == index)
                    {
                        if (grid < dataCount)
                        {
                           // m_BaseList[index].SetCellData(m_DataSource[grid]);
                            CallLua(m_BaseList[index], grid);
                        }
                        continue;
                    }
                    go = m_GoCacheList[i];
                    if (i > index)
                    {
                        go.transform.localPosition = new Vector3(m_StartPosX + offset + (i - index) * m_GridSize, vp.y);
                        int datapos = grid + (i - index);
                        if (datapos >= 0 && datapos < dataCount)
                           // m_BaseList[i].SetCellData(m_DataSource[datapos]);
                            CallLua(m_BaseList[i], datapos);
                    }
                    else
                    {
                        go.transform.localPosition = new Vector3(m_StartPosX + offset + (count - index + i) * m_GridSize, vp.y);
                        int datapos = grid + (count - index + i);
                        if (datapos >= 0 && datapos < dataCount)
                           // m_BaseList[i].SetCellData(m_DataSource[datapos]);
                            CallLua(m_BaseList[i], datapos);
                    }
                }
            }
        }
        else
        {

        }
    }

    int GetMin()
    {
        int index = 0;
        Vector3 pos = m_GoCacheList[0].transform.localPosition;
        if (m_Direction == DragDirection.Horizontal)
        {
            for (int i = 1; i < m_GoCacheList.Count; i++)
            {
                Vector3 pos1 = m_GoCacheList[i].transform.localPosition;
                if (pos1.x < pos.x)
                {
                    pos = pos1;
                    index = i;
                }
            }
        }
        else
        {
            for (int i = 1; i < m_GoCacheList.Count; i++)
            {
                Vector3 pos1 = m_GoCacheList[i].transform.localPosition;
                if (pos1.y > pos.y)
                {
                    pos = pos1;
                    index = i;
                }
            }
        }

        return index;
    }

    int GetMax()
    {
        int index = 0;
        Vector3 pos = m_GoCacheList[0].transform.localPosition;
        if (m_Direction == DragDirection.Horizontal)
        {
            for (int i = 1; i < m_GoCacheList.Count; i++)
            {
                Vector3 pos1 = m_GoCacheList[i].transform.localPosition;
                if (pos1.x > pos.x)
                {
                    pos = pos1;
                    index = i;
                }
            }
        }
        else
        {
            for (int i = 1; i < m_GoCacheList.Count; i++)
            {
                Vector3 pos1 = m_GoCacheList[i].transform.localPosition;
                if (pos1.y < pos.y)
                {
                    pos = pos1;
                    index = i;
                }
            }
        }

        return index;
    }

    Vector3 GetGirdPos(int grid)
    {
        if (grid < 0)
            grid = 0;
        else if (grid > m_Item_count - ShowGridCount)
            grid = m_Item_count - ShowGridCount;
        Vector3 pos = transform.localPosition;
        if (m_Direction == DragDirection.Horizontal)
            pos.x = -grid * m_GridSize;
        else
            pos.y = grid * m_GridHeightSize;
        return pos;
    }

    void OnPressCell()
    {
        //pressed = true;
        if (m_CurrtGridIndex == 0)
        {
            //GameObjectOperator.SetGameObjectUnActive(m_PreBtn);
            m_PreBtn.SetActive(false);
        }
        else
        {
            //GameObjectOperator.SetGameObjectActive(m_PreBtn);
            m_PreBtn.SetActive(true);
        }
    }

    void OnUnPressCell(float speed)
    {
        if (m_CurrCellCount <= ShowGridCount)
            return;
        //pressed = false;
        if (Mathf.Abs(speed) > 200 && speed > 0 && Mathf.Abs(speed) < 1000)
        {
            MoveToGrid(m_CurrtGridIndex + 1);
        }
        else
        {
            MoveToGrid(m_CurrtGridIndex);
        }
    }

    void MoveToGrid(int gridIndex)
    {
        m_CachePos = GetGirdPos(gridIndex);
        SpringPosition sp = SpringPosition.Begin(gameObject, m_CachePos, 7f);
        sp.ignoreTimeScale = true;
        sp.worldSpace = false;
        sp.EventReceiver = gameObject;
    }

    void SetBtnState()
    {
        if (dataCount == 0 || m_Item_count < 1)
        {
            //GameObjectOperator.SetGameObjectUnActive(m_PreBtn);
            //GameObjectOperator.SetGameObjectUnActive(m_NextBtn);
            m_PreBtn.SetActive(false);
            m_NextBtn.SetActive(false);
            return;
        }
        if (m_CurrtGridIndex == 0)
        {
            //GameObjectOperator.SetGameObjectUnActive(m_PreBtn);
            m_PreBtn.SetActive(false);
        }
        else
        {
           // GameObjectOperator.SetGameObjectActive(m_PreBtn);
            m_PreBtn.SetActive(true);
        }
        if (m_CurrtGridIndex == m_Item_count - ShowGridCount)
        {
            //GameObjectOperator.SetGameObjectUnActive(m_NextBtn);
            m_NextBtn.SetActive(false);
        }
        else
        {
           // GameObjectOperator.SetGameObjectActive(m_NextBtn);
            m_NextBtn.SetActive(true);
        }
    }

    void OnNextBtn()
    {
        if (dataCount == 0 || m_Item_count < 1 || m_CurrtGridIndex == m_Item_count - ShowGridCount)
        {
            return;
        }
        MoveToGrid(m_CurrtGridIndex + 1);
    }

    void OnPreBtn()
    {
        if (dataCount == 0 || m_Item_count < 1 || m_CurrtGridIndex == 0)
        {
            return;
        }
        MoveToGrid(m_CurrtGridIndex - 1);
    }
}
