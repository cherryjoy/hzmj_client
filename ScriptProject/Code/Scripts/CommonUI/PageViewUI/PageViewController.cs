using System;
using System.Collections.Generic;
using UnityEngine;
using UniLua;

public class PageViewController : MonoBehaviour
{
    private LuaState lua_;

    public GameObject m_pageCellPrefab;
    public GameObject m_prePageGo;
    public GameObject m_nextPageGo;

    public int m_pageHorizontalSize = 100;

    public PageViewBarController m_pageViewBar = null;
    public LuaBehaviour controller = null;
    public int moveSpeed = 8;
    private int m_currentPage = -1;
    private int m_pageNumber = 0;

    private Transform cachedTranform = null;

    private Vector3 m_leftRange = Vector3.zero;
    private Vector3 m_rightRange = Vector3.zero;
    private Vector3 m_CachePos = Vector3.zero;

    private Dictionary<int, GameObject> m_cachedCellsDic = new Dictionary<int, GameObject>();
    private bool m_isNeedToUpdate = false;

    void Awake()
    {
        cachedTranform = transform;
        preFramePos = cachedTranform.localPosition;
        if (m_prePageGo != null)
            m_prePageGo.SetActive(false);
        if (m_nextPageGo != null) 
            m_nextPageGo.SetActive(false);
    }

    void OnEnable()
    {
        lua_ = LuaInstance.instance.Get();
    }

    private Vector3 preFramePos = new Vector3();
    void Update()
    {
        if (!m_isNeedToUpdate) return;

        //if (m_pageNumber <= 1 || !m_isNeedToUpdate) return;
        
        Vector3 localPos = cachedTranform.localPosition;
        if(Math.Abs(Vector3.Distance(localPos,preFramePos))< 0.1)
        {
            return;
        }
        preFramePos = localPos;
        if (localPos.x > m_leftRange.x)
        {
            localPos.x = m_leftRange.x;
            cachedTranform.localPosition = localPos;

            m_currentPage = 0;

            return;
        }
        else if (localPos.x < m_rightRange.x)
        {
            localPos.x = m_rightRange.x;
            cachedTranform.localPosition = localPos;

            m_currentPage = m_pageNumber - 1;

            return;
        }
        else
        {
            m_currentPage = (int)Math.Round(Math.Abs(localPos.x) + m_pageHorizontalSize * 0.5f) / m_pageHorizontalSize;
            float curPagePosX = m_currentPage * m_pageHorizontalSize;
            if (Mathf.Abs(Math.Abs(localPos.x) - Math.Abs(curPagePosX)) <= 5)
            {
                last_cur = last_need = m_currentPage;
                //SetCachedCellState();
            }
            else
            {
                int lpage = (int)((Math.Abs(localPos.x) - m_pageHorizontalSize * 0.5f) / (float)m_pageHorizontalSize + 0.5);
                int rpage = (int)((Math.Abs(localPos.x) + m_pageHorizontalSize * 0.5f) / (float)m_pageHorizontalSize + 0.5);

                if (lpage != last_cur || last_need != rpage)
                {
                    if (lpage < last_cur)
                    {
                        FillCellDataByIndex(ShowCellAtPage(lpage), lpage);
                    }
                    if (last_need < rpage)
                    {
                        FillCellDataByIndex(ShowCellAtPage(rpage), rpage);
                    }
                }

                last_cur = lpage;
                last_need = rpage;
            }
        }
        SetPageUpDownState();
    }

    private int last_cur = 0;
    private int last_need = 0;

    public int GetCurrectPageNum()
    {
        return m_currentPage;
    }

    public void SetPageNumberToInitPage(int num,bool isInit = false)
    {
        m_pageNumber = num;
        m_currentPage = 0;
        if (isInit)
        {
            InitPage();
        }
    }

    public void InitPageWithPageIndex(int pageNum, int pageIndex)
    {
        m_currentPage = pageIndex;
        m_pageNumber = pageNum;

        InitPage();

        transform.localPosition = GetTargetPagePos(pageIndex);

    }
    private bool isInitPageCell = false;
    public void InitPage() 
    {
        if (m_pageNumber < 1)
            return;

        if (!isInitPageCell)
        {
            for (int i = 0; i < 2; i++)
            {
                GameObject cell = NGUITools.AddChildNotLoseAnything(gameObject, m_pageCellPrefab);
                cell.SetActive(false);

                m_cachedCellsDic.Add(i, cell);
            }

            isInitPageCell = true;
        }

        FillCellDataByIndex(ShowCellAtPage(m_currentPage), m_currentPage);

        if (m_pageViewBar != null)
        {
            m_pageViewBar.OnBarSelectedAtIndex(m_currentPage);
        }

        SetPageUpDownState();

        m_leftRange = GetTargetPagePos(0);
        m_rightRange = GetTargetPagePos(m_pageNumber - 1);
    }

    private void SetPageUpDownState()
    {
        if (m_prePageGo == null && m_nextPageGo != null)
            m_nextPageGo.SetActive(false);
        else if (m_prePageGo != null && m_nextPageGo == null)
            m_prePageGo.SetActive(false);
        else if (m_prePageGo != null && m_nextPageGo != null)
        {
            if (m_pageNumber <= 1)
            {
                m_prePageGo.SetActive(false);
                m_nextPageGo.SetActive(false);
            }
            else if (m_currentPage == 0)
            {
                m_prePageGo.SetActive(false);
                m_nextPageGo.SetActive(true);
            }
            else if (m_currentPage == m_pageNumber - 1)
            {
                m_prePageGo.SetActive(true);
                m_nextPageGo.SetActive(false);
            }
            else
            {
                m_prePageGo.SetActive(true);
                m_nextPageGo.SetActive(true);
            }
        }
    }

    public void OnShowNextPage()
    {
        if (m_pageNumber <= 1 || m_currentPage == m_pageNumber - 1)
        {
            return;
        }

        MoveToPage(m_currentPage + 1);

        SetPageUpDownState();
    }

    public void OnShowPrePage()
    {
        if (m_pageNumber <= 1 || m_currentPage <= 0)
        {
            return;
        }

        MoveToPage(m_currentPage - 1);

        SetPageUpDownState();
    }

    public void MoveToPage(int page)
    {
        Vector3 pos = GetTargetPagePos(page);

        m_CachePos = pos;
        SpringPosition sp = SpringPosition.Begin(gameObject, m_CachePos, 5f);
        sp.ignoreTimeScale = true;
        sp.worldSpace = false;
        sp.strength = moveSpeed;
        sp.EventReceiver = gameObject;

        m_isNeedToUpdate = true;
    }

    void OnSpringEnd() 
    {
        transform.localPosition = m_CachePos;

        if (m_pageViewBar != null)
        {
            m_pageViewBar.OnBarSelectedAtIndex(m_currentPage);
        }

        SetCachedCellState(true);
        m_isNeedToUpdate = false;
    }

    private int GetTargetPage(int page)
    {
        if (page < 0)
            page = 0;
        else if (page >= m_pageNumber)
            page = m_pageNumber - 1;

        return page;
    }

    private Vector3 GetTargetPagePos(int page)
    {
        Vector3 pos = cachedTranform.localPosition;
        pos.x = -m_pageHorizontalSize * GetTargetPage(page) ;
        return pos;
    }

    void OnPressCell() {
        m_isNeedToUpdate = true;
    }

    void OnUnPressCell(float speed)
    {
        if (m_pageNumber < 1) { return; }

        if (Math.Abs(speed) > 100 && speed > 0 && m_currentPage < m_pageNumber - 1)
            MoveToPage(m_currentPage + 1);
        else if (Math.Abs(speed) > 100 && speed < 0 && m_currentPage > 0)
            MoveToPage(m_currentPage - 1);
        else
            MoveToPage(m_currentPage);
    }
    
    private void FillCellDataByIndex(GameObject cell,int i)
    {
        LuaBehaviour cellLuaBehaviour = cell.GetComponent<LuaBehaviour>();
        if (cell != null)
        {
            lua_.GetGlobal(cellLuaBehaviour.ScriptName);
            lua_.GetField(-1, "SetPageData");
            lua_.RawGetI(LuaAPI.LUA_REGISTRYINDEX, cellLuaBehaviour.Object_ref);
            lua_.RawGetI(LuaAPI.LUA_REGISTRYINDEX, controller.Object_ref);
            lua_.PushInteger(i);
            lua_.PCall(3, 0, 0);
            lua_.Pop(1);
        }
    }

    private GameObject GetCachedCell(int page)
    {
        if (m_cachedCellsDic.ContainsKey(page))
        {
            return m_cachedCellsDic[page];
        }
        else
        {
            foreach (KeyValuePair<int, GameObject> kvp in m_cachedCellsDic)
            {
                if (kvp.Key != m_currentPage)
                {
                    GameObject cell = m_cachedCellsDic[kvp.Key];

                    m_cachedCellsDic.Remove(kvp.Key);

                    m_cachedCellsDic.Add(page, cell);
                    return cell;
                }
            }
        }

        return null;
    }

    private void SetCachedCellState(bool isSpringEnd= false)
    {
        foreach (KeyValuePair<int, GameObject> kvp in m_cachedCellsDic)
        {
            if (kvp.Key != m_currentPage)
            {
                GameObject cell = m_cachedCellsDic[kvp.Key];
                if(cell.activeSelf)
                {
                    cell.SetActive(false);

                    if(isSpringEnd)
                    {
                        LuaBehaviour cellLuaBehaviour = cell.GetComponent<LuaBehaviour>();
                        if (cell != null)
                        {
                            lua_.GetGlobal(cellLuaBehaviour.ScriptName);
                            lua_.GetField(-1, "OnPageDisable");
                            lua_.RawGetI(LuaAPI.LUA_REGISTRYINDEX, cellLuaBehaviour.Object_ref);
                            lua_.RawGetI(LuaAPI.LUA_REGISTRYINDEX, controller.Object_ref);
                            lua_.PushNumber(kvp.Key);
                            lua_.PCall(3, 0, 0);
                            lua_.Pop(1);
                        }
                    }
                }
            }
        }
    }

    private GameObject ShowCellAtPage(int page)
    {
        GameObject cell = GetCachedCell(page);
        cell.SetActive(true);
        Vector3 localPos = cell.transform.localPosition;
        cell.transform.localPosition = new Vector3(page * m_pageHorizontalSize, localPos.y, localPos.z);

        return cell;
    }
}
