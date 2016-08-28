using System;
using System.Collections.Generic;
using UnityEngine;
using UniLua;

public class MsgRecvWithId : MonoBehaviour
{
    public TextAsset lua_script;


    void OnClickWithId(GameObject obj)
    {
        LuaState lua_ = LuaInstance.instance.Get();
        lua_.GetGlobal(lua_script.name);
        lua_.GetField(-1, "OnClickWithName");
        string name = obj.name;
        lua_.PushString(name);
        lua_.PCall(1, 0, 0);
        lua_.Pop(1);
    }
}
