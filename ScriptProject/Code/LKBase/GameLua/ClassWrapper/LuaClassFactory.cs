using System;
using System.Collections.Generic;
using System.Reflection;
using UniLua;
using UnityEngine;


class LuaClassFactory
{
    static LuaState lua_;
    public static void RegisterToLua(LuaState lua, Type type)
    {
        lua_ = lua;
        string[] funcList = new string[]
        {
             "Get",
             "GetByParams",
             "GetByComplexParams",
             "IsSameType",
        };

        LuaAPI.lua_CFunction[] funcDeList = new LuaAPI.lua_CFunction[]
        {
             Get,
             GetByParams,
             GetByComplexParams,
             IsSameType,
        };
        LuaWrapper.RegisterToLua(lua, type, funcList, funcDeList);
    }

    [MonoPInvokeCallback(typeof(LuaAPI.lua_CFunction))]
    public static int Get(IntPtr l) 
    {
        string className = lua_.ToString(-1);

        Type classType = Register.GetGameClassType(className);
        object classInstace = Activator.CreateInstance(classType);

        lua_.NewClassUserData(classInstace);
        return 1;
    }

    [MonoPInvokeCallback(typeof(LuaAPI.lua_CFunction))]
    public static int GetByParams(IntPtr l)
    {
        string className = lua_.ToString(1);
        int num = lua_.ToInteger(2);
        object[] parameters = new object[num];
        for (int i = 0; i < num; i++) {
            parameters[i] = lua_.ToUserDataObject(3 + i);
        }

        Type classType = Register.GetGameClassType(className);
        
        object classInstace = Activator.CreateInstance(classType,parameters);

        lua_.NewClassUserData(classInstace);
        return 1;
    }

    [MonoPInvokeCallback(typeof(LuaAPI.lua_CFunction))]
    public static int GetByComplexParams(IntPtr l)
    {
        string className = lua_.ToString(1);
        int num = lua_.ToInteger(2);
        object[] parameters = new object[num];
        int stackIndex = 3;
        for (int i = 0; i < num; i++) {
            LuaAPI.VarType type = (LuaAPI.VarType) lua_.ToInteger(stackIndex);
            stackIndex++;
            parameters[i] = lua_.GetObjByType(type,stackIndex);
            stackIndex++;
        }

        Type classType = Register.GetGameClassType(className);
        
        object classInstace = Activator.CreateInstance(classType,parameters);

        lua_.NewClassUserData(classInstace);
        return 1;
    }

    [MonoPInvokeCallback(typeof(LuaAPI.lua_CFunction))]
    public static int IsSameType(IntPtr l)
    {
        object par1 = lua_.ToUserDataObject(-2);
        object par2 = lua_.ToUserDataObject(-1);

        bool isSame = par1.GetType().Equals(par2.GetType());
        lua_.PushBoolean(isSame);
        return 1;
    }

}