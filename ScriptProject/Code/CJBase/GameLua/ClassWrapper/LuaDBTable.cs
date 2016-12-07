using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UniLua;
using UnityEngine;


class LuaDBTable
{
    static LuaState lua_;
    public static void RegisterToLua(LuaState lua, Type type)
    {
        lua_ = lua;
        string[] funcList = new string[]
        {
             "SetLuaTableField",
        };

        LuaAPI.lua_CFunction[] funcDeList = new LuaAPI.lua_CFunction[]
        {
             SetLuaTableField,
        };
        LuaWrapper.RegisterToLua(lua, type, funcList, funcDeList);
    }

    [MonoPInvokeCallback(typeof(LuaAPI.lua_CFunction))]
    public static int SetLuaTableField(IntPtr l) 
    {
        string tableName = lua_.ToString(1);
        WDBData dbData = CDataMgr.Instance.GetOrCreateDB(tableName);

        lua_.GetGlobal("WDB_"+tableName);
        foreach (KeyValuePair<string, int> kv in dbData.mFieldName)
        {
            lua_.PushInteger(dbData.GetFieldByName(kv.Key));
            lua_.SetField(-2, kv.Key);
        }
        lua_.Pop(2);

        return 0;
    }

}

