using UnityEngine;
using System.Collections.Generic;
using UniLua;
using System.Runtime.InteropServices;
using System.Reflection;
using System;
using System.Linq.Expressions;
public class LuaClass
{
    public Type register_type_ = null;

    int register_funtion_ = 0;
    int field_index_ = 0;
    int classIndex = 0;
    LuaState lua_ = null;
    public List<MethodInfo> methods_ = new List<MethodInfo>();
    public List<ParameterInfo[]> paramters_ = new List<ParameterInfo[]>();
    public FieldInfo[] fields_ = null;
    public PropertyInfo[] propertys_ = null;
    public int type_ref_ = 0;

    public bool is_register
    {
        get
        {
            return type_ref_ != 0;
        }
    }

    public LuaClass(LuaState lua, Type type, int index)
    {
        classIndex = index;
        lua_ = lua;
        BeginRegisterClass(type);
    }

    void BeginRegisterClass(Type type)
    {
        if (register_type_ != null)
            return;
        lua_.NewTable();
        lua_.PushValue(-1);
        lua_.SetField(-2, "__index");

        register_type_ = type;

        RegisterField();
    }

    public void RegisterFunction(FuncInfo funcInfo)
    {
        RegisterFuncByMethodInfo(funcInfo.funcName, GetMethodInfo(funcInfo), LuaInstance.Callback);
    }

    public void RegisterStaticFunction(FuncInfo funcInfo)
    {
        RegisterFuncByMethodInfo(funcInfo.funcName, GetMethodInfo(funcInfo), LuaInstance.CallbackStatic);
    }

    private MethodInfo GetMethodInfo(FuncInfo funcInfo)
    {
        MethodInfo methodInfo = null;
        if (funcInfo.funcParamTypes == null)
        {
            methodInfo = register_type_.GetMethod(funcInfo.orginalName, BindingFlags.Instance | BindingFlags.Public | BindingFlags.Static
        | BindingFlags.FlattenHierarchy);
        }
        else
        {
            methodInfo = register_type_.GetMethod(funcInfo.orginalName, BindingFlags.Instance | BindingFlags.Public | BindingFlags.Static
        | BindingFlags.FlattenHierarchy, null, funcInfo.funcParamTypes, null);
        }
        return methodInfo;
    }

    public void RegisterFuncByMethodInfo(String funcName, MethodInfo m, LuaAPI.lua_CFunction cfunc)
    {
        if (m == null)
            return; // FIX add warnning
        else
        {
            methods_.Add(m);
            paramters_.Add(m.GetParameters());

            int a = LuaInstance.MergeInt(register_funtion_, classIndex);
            lua_.PushInteger(a);
            lua_.PushLuaClosure(cfunc, 1);
            lua_.SetField(-2, funcName);
            register_funtion_++;
        }
    }    
    public void EndRegisterClass()
    {
        if (type_ref_ == 0)
        {
            lua_.PushValue(-1);
            type_ref_ = lua_.L_Ref(LuaAPI.LUA_REGISTRYINDEX);
            lua_.PushLuaClosure(LuaState.GC, 0);
            lua_.SetField(-2, "__gc");
            lua_.SetGlobal(register_type_.Name);
        }
    }

    public void RegisterField()
    {
        fields_ = register_type_.GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.Static);

        if (fields_ == null)
            return; // FIX add warnning
        else
        {
            LuaAPI.lua_CFunction cfunc = LuaInstance.SetField;

            for (int i = 0; i < fields_.Length; i++)
            {
                int a = LuaInstance.MergeInt(field_index_, classIndex);
                lua_.PushInteger(a);
                lua_.PushLuaClosure(cfunc, 1);
                lua_.SetField(-2, fields_[i].Name);

                field_index_++;
            }
        }
    }
}