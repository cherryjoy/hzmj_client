using System;
using System.Collections.Generic;
using UnityEngine;
using UniLua;
class LuaColor
{
    static LuaState lua_;
    public static void RegisterToLua(LuaState lua, Type type)
    {
        lua_ = lua;
         string[] funcList = new string[]
        {
             "Get",
            "Equal",
            "StringToColor",
        };

        LuaAPI.lua_CFunction[] funcDeList = new LuaAPI.lua_CFunction[]
        {
            Get,
            Equal,
            StringToColor,
        };
        LuaWrapper.RegisterToLua(lua, type,funcList,funcDeList);
    }

    [MonoPInvokeCallback(typeof(LuaAPI.lua_CFunction))]
    public static int Equal(IntPtr l)
    {
        Color c1 = (Color)lua_.ToUserDataObject(1);
        Color c2 = (Color)lua_.ToUserDataObject(2);
        lua_.PushBoolean(c1==c2);
        return 1;
    }


    [MonoPInvokeCallback(typeof(LuaAPI.lua_CFunction))]
    public static int Get(IntPtr l)
    {
        float r = (float)lua_.ToNumber(1);
        float g = (float)lua_.ToNumber(2);
        float b = (float)lua_.ToNumber(3);
        float a = (float)lua_.ToNumber(4);

        Color c = new Color(r, g, b, a);

        lua_.NewUserDataWithGC(c);
        return 1;
    }

    [MonoPInvokeCallback(typeof(LuaAPI.lua_CFunction))]
    public static int StringToColor(IntPtr l)
    {
        string colorStr = lua_.ToString(-1);
        Color color = NGUIMath.StringToColor(colorStr);

        lua_.NewUserDataWithGC(color);
        return 1;
    }

}

