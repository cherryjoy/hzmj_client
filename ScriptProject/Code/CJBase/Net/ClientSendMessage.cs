using UnityEngine;
using System.IO;
using System.Collections.Generic;
using UniLua;

public class CClientSendMessage : Singleton<CClientSendMessage>
{
    private static LuaState Lua { get { return LuaInstance.instance.Get(); } }

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

    public static bool SendPing(int netPing)
    {
        return LuaNetwork.Net.SendNetMessage((int)ENetMsgID_C2S.C2S_PING_TIME, UsStructTo.StructToBytes(netPing));
    }
}