using System;
using System.Collections.Generic;
using System.Collections;
using UnityEngine;
using UniLua;
public class LuaNetwork
{
    static LuaState lua_;
    public static CNetWorkGlobal Net;
    public static void RegisterToLua(LuaState lua, Type type)
    {
        lua_ = lua;
        Net = new CNetWorkGlobal();
        string[] funcList = new string[]
        {
            "IsConnected",
            "Connect",
            "DisConnect",
            "SendNetMessage",
            "Update",
			"SetSocketSendNoDeley",
        };

        LuaAPI.lua_CFunction[] funcDeList = new LuaAPI.lua_CFunction[]
        {
             IsConnected,
             Connect,
             DisConnect,
             SendNetMessage,
             Update,
			 SetSocketSendNoDeley,
        };
        LuaWrapper.RegisterToLua(lua, type, funcList, funcDeList);
    }

    [MonoPInvokeCallback(typeof(LuaAPI.lua_CFunction))]
    public static int IsConnected(IntPtr l)
    {
        lua_.PushBoolean(Net.IsConnected());
        return 1;
    }
    
    [MonoPInvokeCallback(typeof(LuaAPI.lua_CFunction))]
    public static int Connect(IntPtr l)
    {
        string ip = lua_.ToString(-2);
        int port = lua_.ToInteger(-1);
        bool isConnect = Net.Connect(ip, (ushort)port);
        lua_.PushBoolean(isConnect);
        return 1;
    }
    
    [MonoPInvokeCallback(typeof(LuaAPI.lua_CFunction))]
    public static int DisConnect(IntPtr l)
    {
        Net.DisConnect();
        return 0;
    }
    
    [MonoPInvokeCallback(typeof(LuaAPI.lua_CFunction))]
    public static int SendNetMessage(IntPtr l)
    {
        int msgId = lua_.ToInteger(-3);
        int length = lua_.ToInteger(-1);
        byte[] data = lua_.CopyBytesFromUnManaged(-2, length);

        lua_.PushBoolean(Net.SendNetMessage(msgId, data));
        return 1;
    }
    
    [MonoPInvokeCallback(typeof(LuaAPI.lua_CFunction))]
    public static int Update(IntPtr l)
    {
        Net.Update();
        return 0;
    }

	[MonoPInvokeCallback(typeof(LuaAPI.lua_CFunction))]
	public static int SetSocketSendNoDeley(IntPtr l)
	{
		bool noDelay = lua_.ToBoolean(-1);
		Net.SetSocketSendNoDeley(noDelay);
		return 1;
	}
}
