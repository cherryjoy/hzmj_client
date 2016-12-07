using UnityEngine;
using System.Collections;
using UniLua;

public class LuaBehaviourUpdate : LuaBehaviour
{

    // Update is called once per frame
    void Update()
    {

        if (func_update_ref_ == LuaAPI.LUA_REFNIL)
        {
            return;
        }
        LuaState lua_ = LuaInstance.instance.Get();
        // call lua update function
        lua_.RawGetI(LuaAPI.LUA_REGISTRYINDEX, func_update_ref_);
        lua_.RawGetI(LuaAPI.LUA_REGISTRYINDEX, Object_ref);

        lua_.PCall(1, 0, 0);
    }


}
