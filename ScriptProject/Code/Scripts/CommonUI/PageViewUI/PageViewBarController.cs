using System.Collections.Generic;
using UnityEngine;
using UniLua;

public class PageViewBarController : MonoBehaviour
{
    private static readonly string OnPageViewBarSelected = "OnPageViewBarSelected";

    public List<GameObject> barList = new List<GameObject>();

    public bool isAutoLayout = true;
    public int space = 0;
    private UILineFlowLayout lineFlowLayout = null;
    private int selectedIndex = -1;

    private LuaState luaState = null;

    public void OnBarSelectedAtIndex(int i)
    {
        if (barList != null && i>=0 && i<=barList.Count -1)
        {
            if (selectedIndex >= 0)
            {
                CallLua(barList[selectedIndex], false);
            }

            selectedIndex = i;

            CallLua(barList[selectedIndex], true);
        }
    }

    void Awake()
    {
        luaState = LuaInstance.instance.Get();

        if (barList != null && isAutoLayout)
        {
            lineFlowLayout = gameObject.GetComponent<UILineFlowLayout>();
            if (lineFlowLayout == null)
            {
                lineFlowLayout = gameObject.AddComponent<UILineFlowLayout>();
            }
            lineFlowLayout.Style = 0;
            lineFlowLayout.Anchor = 0;
            lineFlowLayout.space = space;

            for (int i = 0, c = barList.Count; i < c; i++)
            {
                lineFlowLayout.AddChild(barList[i], false);
            }
            lineFlowLayout.mRefreshCount += 2;
        }
    }

    public void AddBarAtIndex(GameObject barGo,int index)
    {
        if(index <0) index = 0;
        else if(index>barList.Count) index = barList.Count;

        barList.Insert(index, barGo);

        if(isAutoLayout)
        {
            lineFlowLayout.childrens.Insert(index, new UILineFlowLayoutChild(barGo.transform, false));
            lineFlowLayout.mRefreshCount += 2;
        }
    }

    public void AddBar(GameObject barGo)
    {
        AddBarAtIndex(barGo, barList.Count);
    }

    public void ClearBar()
    {
        selectedIndex = -1;
        barList = new List<GameObject>();
        if(isAutoLayout)
        {
            lineFlowLayout.RemoveAll();
        }
    }

    private void CallLua(GameObject go, bool isSelected)
    {
        LuaBehaviour luaBe = go.GetComponent<LuaBehaviour>();
        if (luaBe == null || string.IsNullOrEmpty(luaBe.scriptShortPath))
            return;

        luaState.GetGlobal(luaBe.ScriptName);
        luaState.GetField(-1, OnPageViewBarSelected);
        luaState.RawGetI(LuaAPI.LUA_REGISTRYINDEX, luaBe.Object_ref);
        luaState.PushBoolean(isSelected);
        luaState.PCall(2, 0, 0);
        luaState.Pop(1);
    }
}
