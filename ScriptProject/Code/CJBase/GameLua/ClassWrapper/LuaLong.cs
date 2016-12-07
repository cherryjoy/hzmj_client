using System;
using UniLua;
using UnityEngine;

class LuaLong
{
    static LuaState lua_;
    public static void RegisterToLua(LuaState lua, Type type)
    {
        lua_ = lua;
        string[] funcList = new string[]
        {
            "ShowLong",
            "ShowLableWithLong",
            "ToString",
            "GetDataLong",
        };

        LuaAPI.lua_CFunction[] funcDeList = new LuaAPI.lua_CFunction[]
        {
             ShowLong,
             ShowLableWithLong,
             ToString,
             GetDataLong,
        };
        LuaWrapper.RegisterToLua(lua, type, funcList, funcDeList);
    }

    [MonoPInvokeCallback(typeof(LuaAPI.lua_CFunction))]
    public static int ShowLong(IntPtr l)
    {
        UILabel label = (UILabel)lua_.ToUserDataObject(-2);
        long value = lua_.ReadLongFromUnManaged(-1);
        label.text = value.ToString();
        return 0;
    }

    [MonoPInvokeCallback(typeof(LuaAPI.lua_CFunction))]
    public static int ShowLableWithLong(IntPtr l) //4个参数（lable,value,text前面,text后面）
    {
        UILabel label = (UILabel)lua_.ToUserDataObject(-4);
        long value = lua_.ReadLongFromUnManaged(-3);
        string text1 = lua_.ToString(-2);
        string text2 = lua_.ToString(-1);
        string finalText = string.Format("{0}{1}{2}", text1, value.ToString(), text2);
        label.text = finalText;
        return 0;
    }

    [MonoPInvokeCallback(typeof(LuaAPI.lua_CFunction))]
    public static int ToString(IntPtr l)
    {
        long value = lua_.ReadLongFromUnManaged(-1);
        lua_.PushString(value.ToString());
        return 1;
    }

    [MonoPInvokeCallback(typeof(LuaAPI.lua_CFunction))]
    public static int GetDataLong(IntPtr l) //3个参数（WDBData,index,wdbIndex）
    {
        WDBData db = (WDBData)lua_.ToUserDataObject(-3);
        int index = lua_.ToInteger(-2);
        int wdbIndex = lua_.ToInteger(-1);

        long longValue = db.GetData<long>(index, wdbIndex);
        lua_.PushLongInterger(longValue);
        return 1;
    }

}

