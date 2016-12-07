using System;
using System.Collections.Generic;
using UnityEngine;
using UniLua;
class LuaVector3
{
    static LuaState lua_;
    public static void RegisterToLua(LuaState lua, Type type)
    {
        lua_ = lua;
         string[] funcList = new string[]
        {
            "Create",
            "Vector3",
            "GetMagnitude",
            "Normalize",            
            "x",
            "y",
            "z",
            "Equal",
        };

        LuaAPI.lua_CFunction[] funcDeList = new LuaAPI.lua_CFunction[]
        {            
            Create,
            Vector3,
            GetMagnitude,
            Normalize,
            x,
            y,
            z,
            Equal,
        };
        LuaWrapper.RegisterToLua(lua, type,funcList,funcDeList);
    }
   
    [MonoPInvokeCallback(typeof(LuaAPI.lua_CFunction))]
    public static int Create(IntPtr l)
    {
        var x = (float)lua_.ToNumber(1);
        var y = (float)lua_.ToNumber(2);
        var z = (float)lua_.ToNumber(3);

        var c = new Vector3(x, y, z);
        lua_.NewClassUserData(c);
        return 1;
    }

    [MonoPInvokeCallback(typeof(LuaAPI.lua_CFunction))]
    public static int Vector3(IntPtr l)
    {
        Vector3 v = (Vector3)lua_.ToUserDataObject(1);
        lua_.NewTable();
        lua_.PushNumber(v.x);
        lua_.SetField(-2,"x");
        lua_.PushNumber(v.y);
        lua_.SetField(-2, "y");
        lua_.PushNumber(v.z);
        lua_.SetField(-2, "z");
        return 1;
    }

    [MonoPInvokeCallback(typeof(LuaAPI.lua_CFunction))]
    public static int GetMagnitude(IntPtr l) 
    {
        var v = (Vector3)lua_.ToUserDataObject(1);
        lua_.PushNumber(v.magnitude);
        return 1;
    }

    [MonoPInvokeCallback(typeof(LuaAPI.lua_CFunction))]
    public static int Normalize(IntPtr l)
    {
        var v = (Vector3)lua_.ToUserDataObject(1);
        v.Normalize();
        lua_.Pop(1);
        lua_.NewUserDataWithGC(v);
        return 1;
    }

    [MonoPInvokeCallback(typeof(LuaAPI.lua_CFunction))]
    public static int x(IntPtr l)
    {
        var v = (Vector3)lua_.ToUserDataObject(1);
        lua_.PushNumber(v.x);
        return 1;
    }

    [MonoPInvokeCallback(typeof(LuaAPI.lua_CFunction))]
    public static int y(IntPtr l)
    {
        var v = (Vector3)lua_.ToUserDataObject(1);
        lua_.PushNumber(v.y);
        return 1;
    }

    [MonoPInvokeCallback(typeof(LuaAPI.lua_CFunction))]
    public static int z(IntPtr l)
    {
        var v = (Vector3)lua_.ToUserDataObject(1);
        lua_.PushNumber(v.z);
        return 1;
    }

    [MonoPInvokeCallback(typeof(LuaAPI.lua_CFunction))]
    public static int Equal(IntPtr l)
    {
        var v1 = (Vector3)lua_.ToUserDataObject(1);
        var v2 = (Vector3)lua_.ToUserDataObject(2);
        lua_.PushBoolean(v1 == v2);
        return 1;
    }
}

