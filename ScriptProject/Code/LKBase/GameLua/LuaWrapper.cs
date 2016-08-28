using System;
using UniLua;
using System.Runtime.InteropServices;
public class LuaWrapper
{
    public static void RegisterToLua(LuaState lua, Type type,string[] funcList,LuaAPI.lua_CFunction[] funcDeList)
    {
        Type classType = type;

        lua.NewTable();

        for (int i = 0; i < funcList.Length; i++)
        {
            lua.PushLuaClosure(funcDeList[i], 0);
            lua.SetField(-2, funcList[i]);
        }

        lua.SetGlobal(type.Name);
    }

}
