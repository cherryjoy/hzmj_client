using System;
using System.Collections.Generic;
using UnityEngine;
using UniLua;
class LuaQuaternion
{
    static LuaState lua_;
    public static void RegisterToLua(LuaState lua, Type type)
    {
        lua_ = lua;
         string[] funcList = new string[]
        {
            "Create",
            "Euler",
            "Identity",        
            "x",
            "y",
            "z",
            "w",
            "Equal",
        };

        LuaAPI.lua_CFunction[] funcDeList = new LuaAPI.lua_CFunction[]
        {            
            Create,
            Euler,
            Identity,
            Equal,
            x,
            y,
            z,
            w,
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
        var w = (float)lua_.ToNumber(4);

        var c = new Quaternion(x, y, z,w);
        lua_.NewUserDataWithGC(c);
        return 1;
    }

    [MonoPInvokeCallback(typeof(LuaAPI.lua_CFunction))]
    public static int Euler(IntPtr l)
    {
        var x = (float)lua_.ToNumber(1);
        var y = (float)lua_.ToNumber(2);
        var z = (float)lua_.ToNumber(3);

        var c = Quaternion.Euler(new Vector3(x,y,z));
        lua_.NewUserDataWithGC(c);
        return 1;
    }

    [MonoPInvokeCallback(typeof(LuaAPI.lua_CFunction))]
    public static int Identity(IntPtr l)
    {
        var c = Quaternion.identity;
        lua_.NewUserDataWithGC(c);
        return 1;
    }
    [MonoPInvokeCallback(typeof(LuaAPI.lua_CFunction))]
    public static int x(IntPtr l)
    {
        var v = (Quaternion)lua_.ToUserDataObject(1);
        lua_.PushNumber(v.x);
        return 1;
    }

    [MonoPInvokeCallback(typeof(LuaAPI.lua_CFunction))]
    public static int y(IntPtr l)
    {
        var v = (Quaternion)lua_.ToUserDataObject(1);
        lua_.PushNumber(v.y);
        return 1;
    }

    [MonoPInvokeCallback(typeof(LuaAPI.lua_CFunction))]
    public static int z(IntPtr l)
    {
        var v = (Quaternion)lua_.ToUserDataObject(1);
        lua_.PushNumber(v.z);
        return 1;
    }

    [MonoPInvokeCallback(typeof(LuaAPI.lua_CFunction))]
    public static int w(IntPtr l)
    {
        var v = (Quaternion)lua_.ToUserDataObject(1);
        lua_.PushNumber(v.w);
        return 1;
    }

    [MonoPInvokeCallback(typeof(LuaAPI.lua_CFunction))]
    public static int Equal(IntPtr l)
    {
        var v1 = (Quaternion)lua_.ToUserDataObject(1);
        var v2 = (Quaternion)lua_.ToUserDataObject(2);
        lua_.PushBoolean(v1 == v2);
        return 1;
    }
}

