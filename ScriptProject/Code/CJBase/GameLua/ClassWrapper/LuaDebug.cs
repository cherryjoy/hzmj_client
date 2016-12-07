using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UniLua;
using UnityEngine;

class LuaDebug
{
    static LuaState lua_;
    public static void RegisterToLua(LuaState lua, Type type)
    {
        lua_ = lua;
        string[] funcList = new string[]
        {
             "Log",
             "LogError",
             "Log3Error",
             "LogLong",
        };

        LuaAPI.lua_CFunction[] funcDeList = new LuaAPI.lua_CFunction[]
        {
             Log,
             LogError,
             Log3Error,
             LogLong,
        };
        LuaWrapper.RegisterToLua(lua, type, funcList, funcDeList);
    }

    [MonoPInvokeCallback(typeof(LuaAPI.lua_CFunction))]
    public static int Log(IntPtr l)
    {
        string res = GetLogInfo();
        Debug.Log(res);
        return 0;
    }

    [MonoPInvokeCallback(typeof(LuaAPI.lua_CFunction))]
    public static int LogError(IntPtr l)
    {
        string res = GetLogInfo();
        Debug.LogError(res);
        return 0;
    }

    [MonoPInvokeCallback(typeof(LuaAPI.lua_CFunction))]
    public static int Log3Error(IntPtr l)
    {
        string res = GetLogInfo(4);
        Debug.LogError(res);
        return 0;
    }

    [MonoPInvokeCallback(typeof(LuaAPI.lua_CFunction))]
    public static int LogLong(IntPtr l)
    {
        long value = 0;
        if (lua_.Type(-1) == LuaType.LUA_TID) {
            value = lua_.ReadLongId(-1);
        }
        else if (lua_.Type(-1) == LuaType.LUA_TUSERDATA) {
            value = lua_.ReadLongFromUnManaged(-1);
        }

        Debug.Log(value.ToString());
        return 0;
    }

    public static string GetLogInfo(int stackNum = 2) {
        int idx = -1;
        LuaType paraType = lua_.Type(idx);

        object p = null;
        if (paraType == LuaType.LUA_TNUMBER)
        {
            p = (float)lua_.ToNumber(idx);

        }
        else if (paraType == LuaType.LUA_TSTRING)
        {
            p = lua_.ToString(idx);
        }
        else if (paraType == LuaType.LUA_TBOOLEAN)
        {
            p = lua_.ToBoolean(idx);
        }
        else if (paraType == LuaType.LUA_TUSERDATA)
        {
            p = lua_.ToUserDataObject(idx);
        }
        else if (paraType == LuaType.LUA_TNIL)
        {
            p = "Nil";
        }
        else if (paraType == LuaType.LUA_TTABLE)
        {
            p = "Table Can't be Log.";
        }
        lua_.GetGlobal("debug");
        lua_.GetField(-1, "getinfo");
        lua_.PushNumber(stackNum);
        lua_.PCall(1, 1, 0);
        string stackInfo = string.Empty;
        if (!lua_.IsNil(-1))
        {
            lua_.GetField(-1, "source");
            lua_.GetField(-2, "currentline");

            stackInfo = lua_.ToString(-2);
            stackInfo += lua_.ToString(-1);
            lua_.Pop(3);
        }
        else
        {
            lua_.Pop(2);
        }
       

        lua_.GetGlobal("lua_get_debug_info");
        lua_.PCall(0, 1, 0);
        string fullStackInfo = lua_.ToString(-1);
        lua_.Pop(1);

        string res = string.Empty;

        if (p != null)
        {
            res = p.ToString() + System.Environment.NewLine + stackInfo + System.Environment.NewLine +"StackInfo:\n"+ fullStackInfo;
        }
        else
        {
            Debug.LogError("call LuaDebugError: " + paraType + " stack:" + fullStackInfo + System.Environment.NewLine + LuaInstance.ConstructString(lua_));
        }
           
        return res;
    }
}

