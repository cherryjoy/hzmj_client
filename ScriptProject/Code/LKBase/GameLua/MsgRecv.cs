using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UniLua;

public class MsgRecvOnSelfClick : MonoBehaviour
{
    LuaBehaviour lua_behaviour_ = null;

    void Start()
    {
        lua_behaviour_ = gameObject.GetComponent<LuaBehaviour>();
        if (lua_behaviour_ == null)
            enabled = false;
    }

    void OnClickSelf(GameObject obj)
    {
        LuaState lua_ = LuaInstance.instance.Get();
        Debug.Log("OnClickSelf");
        lua_.GetGlobal(lua_behaviour_.ScriptName);
        lua_.GetField(-1, "OnClickSelf");
        lua_.RawGetI(LuaAPI.LUA_REGISTRYINDEX, lua_behaviour_.Object_ref);
        lua_.PCall(1, 0, 0);
        lua_.Pop(1);
    }
}
