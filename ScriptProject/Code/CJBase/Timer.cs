using System;
using System.Collections.Generic;
using UnityEngine;
using UniLua;

public class TimerManager
{
    private static TimerManager instance;
    public static TimerManager Instance
    {
        get
        {
            if (instance == null)
            {
                instance = new TimerManager();
            }
            return instance;
        }
    }

    int update_func_ref_;
    int gUpdate_func_ref_;
    LuaState lua_;
    float minimumCallTime = 0.1f;
    private float timeGo = 0;
    private float lastTime = 0;

    private float gtimeGo = 0;
    private float glastTime = 0;

    bool isThereAnyEvent = false;
    bool gIsThereAnyEvent = false;

    const string LUATMNAME = "LuaTimerManager";

    public List<CTimerEvent> timeEvents = new List<CTimerEvent>();

    TimerManager()
    {
        Init();
    }

    //DO NOT init twice,unless you start a new lua state
    public void Init()
    {
        lua_ = LuaInstance.instance.Get();
        lua_.GetGlobal(LUATMNAME);
        lua_.GetField(-1, "Update");
        update_func_ref_ = lua_.L_Ref(LuaAPI.LUA_REGISTRYINDEX);
        lua_.GetField(-1, "gUpdate");
        gUpdate_func_ref_ = lua_.L_Ref(LuaAPI.LUA_REGISTRYINDEX);

        lua_.Pop(1);

        lastTime = Time.realtimeSinceStartup;
    }

    public void SetTimerManager(bool isCallLua)
    {
        isThereAnyEvent = isCallLua;
    }

    public void SetGTimerManager(bool isCallLua)
    {
        gIsThereAnyEvent = isCallLua;
    }

    public CTimerEvent NewEvent(float interval, float totalTime, TimerEventHandle h, bool alwaysRepeat = false)
    {
        CTimerEvent timerEvent = new CTimerEvent(interval, totalTime, h, EventDestroy, alwaysRepeat);
        timeEvents.Add(timerEvent);

        return timerEvent;
    }

    public CTimerEvent NewEvent(CTimerEvent cTimerEvent)
    {
        timeEvents.Add(cTimerEvent);

        return cTimerEvent;
    }

    public void EventDestroy(CTimerEvent timeEvent)
    {
        if (timeEvent != null)
        {
            timeEvents.Remove(timeEvent);
            timeEvent = null;
        }
    }

    public void Update()
    {
        timeGo = Time.realtimeSinceStartup;
        while (timeGo > lastTime + minimumCallTime)
        {
            if (isThereAnyEvent)
            {
                lua_.RawGetI(LuaAPI.LUA_REGISTRYINDEX, update_func_ref_);
                lua_.PCall(0, 0, 0);
            }

            lastTime += minimumCallTime;
        }

        gtimeGo += Time.deltaTime;
        while (gtimeGo > minimumCallTime)
        {
            if (gIsThereAnyEvent)
            {
                lua_.RawGetI(LuaAPI.LUA_REGISTRYINDEX, gUpdate_func_ref_);
                lua_.PCall(0, 0, 0);
            }
            gtimeGo -= minimumCallTime;
        }

        for (int i = 0; i < timeEvents.Count; i++)
        {
            timeEvents[i].Update();
        }
    }
}

public delegate void TimerEventHandle();
public delegate void TimerDestroyHanle(CTimerEvent e);
public class CTimerEvent
{
    public float Interval = -1;
    public float TotalTime = 0;
    public bool AlwaysRepeat = false;
    public TimerEventHandle handle = null;

    protected TimerDestroyHanle destroyHandle;
    protected float leftTime = -1;

    public CTimerEvent(float interval, float totalTime, TimerEventHandle h, TimerDestroyHanle dh, bool alwaysRepeat = false)
    {
        Interval = interval;
        TotalTime = totalTime;
        handle = h;
        AlwaysRepeat = alwaysRepeat;
        destroyHandle = dh;

        leftTime = interval;
    }

    public void ReSetThisInter()
    {
        leftTime = Interval;
    }
    public void Update()
    {

        if (AlwaysRepeat)
        {
            CountDown();
        }
        else
        {
            if (TotalTime > 0)
            {
                if (CountDown())
                {
                    TotalTime -= Interval;
                }

            }
            else
            {
                destroyHandle(this);
            }
        }
    }

    protected bool CountDown()
    {
        leftTime -= Time.deltaTime;
        if (leftTime <= 0)
        {
            leftTime = Interval;
            if (handle != null)
            {
                handle();
                return true;
            }
        }
        return false;
    }

    public void Cancle()
    {
        destroyHandle(this);
    }

}
