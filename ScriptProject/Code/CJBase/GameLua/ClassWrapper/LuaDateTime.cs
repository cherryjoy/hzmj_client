using System;
using System.Collections.Generic;
using System.Collections;
using UnityEngine;
using UniLua;
class LuaDateTime
{
    static LuaState lua_;
    public static void RegisterToLua(LuaState lua, Type type)
    {
        lua_ = lua;
        string[] funcList = new string[]
        {
            "Get",
            "GetBySeconds",
            "GetServerSeconds",
            "GetWithTimeZone",
            "GetSecondsLeft",
            "GetByServerSeconds",
            "GetClientTimeByServerSeconds",
            "GetClientNowTime",
            "Ticks_197011",
            "Ticks_UtcNow",
            "Ticks_Now",
            "LongAdd",
            "LongSub",
            "LongMultiply",
            "LongDivide",
            "LongLess",
        };

        LuaAPI.lua_CFunction[] funcDeList = new LuaAPI.lua_CFunction[]
        {
             Get,
             GetBySeconds,
             GetServerSeconds,
             GetWithTimeZone,
             GetSecondsLeft,
             GetByServerSeconds,
             GetClientTimeByServerSeconds,
             GetClientNowTime,
             Ticks_197011,
             Ticks_UtcNow,
             Ticks_Now,
             LongAdd,
             LongSub,
             LongMultiply,
             LongDivide,
             LongLess,
             
        };
        LuaWrapper.RegisterToLua(lua, type, funcList, funcDeList);
    }

    [MonoPInvokeCallback(typeof(LuaAPI.lua_CFunction))]
    public static int Get(IntPtr l)
    {
        long ticks = lua_.ReadLongFromUnManaged(1);

        DateTime time = new DateTime(ticks);
        lua_.NewClassUserData(time);
        return 1;
    }

    [MonoPInvokeCallback(typeof(LuaAPI.lua_CFunction))]
    public static int GetBySeconds(IntPtr l)
    {
        int seconds = lua_.ToInteger(1);

        TimeSpan time = TimeSpan.FromSeconds(seconds);
        lua_.NewClassUserData(time);
        return 1;
    }

    [MonoPInvokeCallback(typeof(LuaAPI.lua_CFunction))]
    public static int GetServerSeconds(IntPtr l)
    {
        DateTime serverDateTime = (DateTime)lua_.ToUserDataObject(1);
        DateTime time1970 = new DateTime(1970, 1, 1);
        long tickFrom1970 = (serverDateTime.Ticks-time1970.Ticks);
        int seconds = (int)(tickFrom1970 / 10000000L);

        lua_.PushInteger(seconds);
        return 1;
    }

    [MonoPInvokeCallback(typeof(LuaAPI.lua_CFunction))]
    public static int GetWithTimeZone(IntPtr l)
    {
        long ticks = lua_.ReadLongFromUnManaged(1);
        long timeZoneTicks = lua_.ReadLongFromUnManaged(2);
        ticks += timeZoneTicks;
        DateTime time = new DateTime(ticks);
        lua_.NewClassUserData(time);
        return 1;
    }

     [MonoPInvokeCallback(typeof(LuaAPI.lua_CFunction))]
    public static int GetSecondsLeft(IntPtr l)
    {
        DateTime from = (DateTime)lua_.ToUserDataObject(1);
        DateTime to = (DateTime)lua_.ToUserDataObject(2);

        TimeSpan subTime = to.Subtract(from);
        double positiveTotalSec = Mathf.Abs((float)subTime.TotalSeconds);
        lua_.PushNumber(positiveTotalSec);
        lua_.PushBoolean(subTime.TotalSeconds < 0);
        
        return 2;
    }

    [MonoPInvokeCallback(typeof(LuaAPI.lua_CFunction))]
    public static int GetByServerSeconds(IntPtr l)
    {
        int seconds = lua_.ToInteger(1);
        long timeZoneTicks = lua_.ReadLongFromUnManaged(2);
        long ticks = (long)seconds * 10000000L ;     
        DateTime time1970 = new DateTime(1970,1,1);
        ticks += time1970.Ticks;
        ticks += timeZoneTicks;
        DateTime time = new DateTime(ticks,DateTimeKind.Utc);
        lua_.NewClassUserData(time);
        return 1;
    }


    [MonoPInvokeCallback(typeof(LuaAPI.lua_CFunction))]
    public static int GetClientTimeByServerSeconds(IntPtr l)
    {
        int seconds = lua_.ToInteger(1);
        long ticks = (long)seconds * 10000000L;
        DateTime time1970 = new DateTime(1970, 1, 1);
        ticks += time1970.Ticks;
        DateTime time = new DateTime(ticks, DateTimeKind.Utc);
        time = time.ToLocalTime();
        lua_.NewClassUserData(time);
        return 1;
    }

    [MonoPInvokeCallback(typeof(LuaAPI.lua_CFunction))]
    public static int GetClientNowTime(IntPtr l)
    {
        DateTime time = new DateTime(DateTime.UtcNow.Ticks, DateTimeKind.Utc);
        time = time.ToLocalTime();
        lua_.NewClassUserData(time);
        return 1;
    }

    [MonoPInvokeCallback(typeof(LuaAPI.lua_CFunction))]
    public static int Ticks_197011(IntPtr l)
    {
        DateTime time = new DateTime(1970, 1, 1);
        lua_.PushLongInterger(time.Ticks);
        return 1;
    }

    [MonoPInvokeCallback(typeof(LuaAPI.lua_CFunction))]
    public static int Ticks_UtcNow(IntPtr l)
    {        
        long ticks = DateTime.UtcNow.Ticks;
        lua_.PushLongInterger(ticks);
        return 1;
    }

    [MonoPInvokeCallback(typeof(LuaAPI.lua_CFunction))]
    public static int Ticks_Now(IntPtr l)
    {
        long ticks = DateTime.Now.Ticks;
        lua_.PushLongInterger(ticks);
        return 1;
    }


    [MonoPInvokeCallback(typeof(LuaAPI.lua_CFunction))]
    public static int LongAdd(IntPtr l)
    {
        long lParamleft = lua_.ReadLongFromUnManaged(1);
        long lParamright = lua_.ReadLongFromUnManaged(2);
        long ticks = lParamleft + lParamright;
        lua_.PushLongInterger(ticks);
        return 1;
    }

    [MonoPInvokeCallback(typeof(LuaAPI.lua_CFunction))]
    public static int LongSub(IntPtr l)
    {
        long lParamleft = lua_.ReadLongFromUnManaged(1);
        long lParamright = lua_.ReadLongFromUnManaged(2);
        long ticks = lParamleft - lParamright;

        lua_.PushLongInterger(ticks);
        return 1;
    }

    [MonoPInvokeCallback(typeof(LuaAPI.lua_CFunction))]
    public static int LongMultiply(IntPtr l)
    {
        long lParamleft = lua_.ReadLongFromUnManaged(1);
        float lParamright = (float)lua_.ToNumber(2);
        long ticks = (long)(lParamleft * lParamright);
        lua_.PushLongInterger(ticks);
        return 1;
    }

    [MonoPInvokeCallback(typeof(LuaAPI.lua_CFunction))]
    public static int LongDivide(IntPtr l)
    {
        long lParamleft = lua_.ReadLongFromUnManaged(1);
        int lParamright = (int)lua_.ToNumber(2);
        long ticks = (long)(lParamleft / lParamright);
        lua_.PushLongInterger(ticks);
        return 1;
    }

    [MonoPInvokeCallback(typeof(LuaAPI.lua_CFunction))]
    public static int LongLess(IntPtr l)
    {
        long lParamleft = lua_.ReadLongFromUnManaged(1);
        long lParamright = lua_.ReadLongFromUnManaged(2);
        lua_.PushBoolean(lParamleft < lParamright);
        return 1;
    }

}

