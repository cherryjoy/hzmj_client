using UnityEngine;
using System.IO;
using System.Collections.Generic;
using UniLua;

public class CClientSendMessage : Singleton<CClientSendMessage>
{
    private static LuaState Lua { get { return LuaInstance.instance.Get(); } }

    public static void SendRoomLoad()
    {
#if USE_LK_LOG	
		LKLog.Log("net", "SendRoomLoad");
#endif
        Lua.GetGlobal("ClientSendMsg");
        Lua.GetField(-1, "SendRoomLoad");
        Lua.PCall(0, 0, 0);
        Lua.Pop(1);
    }

    public static void SendRoomPlayerDead(long killer_id, long dead_id)
    {
#if USE_LK_LOG	
		LKLog.Log("net", string.Format("SendRoomPlayerDead killer_id = {0}, dead_id = {1}", killer_id, dead_id));
#endif
        Lua.GetGlobal("luaScene");
        Lua.GetField(-1, "RoomPlayerDead");
        Lua.PushLongId(killer_id);
        Lua.PushLongId(dead_id);
        Lua.PCall(2, 0, 0);
        Lua.Pop(1);
    }
    public static void SendRoomPlayerRelive()
    {
#if USE_LK_LOG	
		LKLog.Log("net", "SendRoomPlayerRelive");
#endif

        Lua.GetGlobal("ClientSendMsg");
        Lua.GetField(-1, "SendRoomPlayerRelive");
        Lua.PCall(0, 0, 0);
        Lua.Pop(1);
    }
    public static void SendSceneEnter(int sceneID)
    {
#if USE_LK_LOG	
		LKLog.Log("net", string.Format("SendSceneEnter sceneID = {0}", sceneID));
#endif
        Lua.GetGlobal("ClientSendMsg");
        Lua.GetField(-1, "SendSceneEnter");
        Lua.PushInteger(sceneID);
        Lua.PCall(1, 0, 0);
        Lua.Pop(1);
    }
    public static void SendRefreshAttr()
    {
#if USE_LK_LOG	
		LKLog.Log("net", "SendRefreshAttr");
#endif
        Lua.GetGlobal("ClientSendMsg");
        Lua.GetField(-1, "RoleAttrRefresh");
        Lua.PCall(0, 0, 0);
        Lua.Pop(1);
    }
    public static void SendSceneLoad(int scene_id)
    {
#if USE_LK_LOG	
		LKLog.Log("net", string.Format("SendSceneLoad scene_id = {0}", scene_id));
#endif
        Lua.GetGlobal("luaScene");
        Lua.GetField(-1, "SceneLoaded");
        Lua.PushInteger(scene_id);
        Lua.PCall(1, 0, 0);
        Lua.Pop(1);
    }

    public static bool SendPing(int netPing)
    {
        return LuaNetwork.Net.SendNetMessage((int)ENetMsgID_C2S.C2S_PING_TIME, UsStructTo.StructToBytes(netPing));
    }

#if OPEN_BROAD_PING
	public static void SendBMSG_PingTime1()
	{
		BroadCastMessage(ENetMsgID_S2C.S2C_BROAD_PING_TIME1, UsStructTo.StructToBytes(Time.time));
	}

	public static void SendBMSG_PingTime2(float time)
	{
		BroadCastMessage(ENetMsgID_S2C.S2C_BROAD_PING_TIME2, UsStructTo.StructToBytes(time));
	}
#endif

}