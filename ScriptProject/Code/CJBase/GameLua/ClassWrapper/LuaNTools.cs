using System;
using System.Collections.Generic;
using UnityEngine;
using UniLua;

class LuaNTools
{
    static LuaState lua_;
    public static void RegisterToLua(LuaState lua, Type type)
    {
        lua_ = lua;
        string[] funcList = new string[]
        {
            "AddChild",
            "AddChildWithPosition",
            "AddChildWithLocalPosition",
            "AddChildNotLose",
            "AddChildByResourcesPath",
            "AddChildNoBehaviour",
            "AddChildByResourcesPathNoBehaviour",
            "AddEffect",
            "AddEffectLocal",
        };

        LuaAPI.lua_CFunction[] funcDeList = new LuaAPI.lua_CFunction[]
        {
            AddChild,
            AddChildWithPosition,
            AddChildWithLocalPosition,
            AddChildNotLose,
            AddChildByResourcesPath,
            AddChildNoBehaviour,
            AddChildByResourcesPathNoBehaviour,
            AddEffect,
            AddEffectLocal,
        };
        LuaWrapper.RegisterToLua(lua, type, funcList, funcDeList);
    }

    [MonoPInvokeCallback(typeof(LuaAPI.lua_CFunction))]
    public static int AddChild(IntPtr l)
    {
        GameObject parent = lua_.ToUserDataObject(1) as GameObject;
        string path = lua_.ToString(2);
        GameObject prefab = (GameObject)ResLoader.Load(UIPrefabDummy.GetUIPrefabPath(path), typeof(GameObject));
        GameObject obj = NGUITools.AddChildNotLoseScale(parent, prefab);
        LuaBehaviour luaB = obj.GetComponent<LuaBehaviour>();
        if (luaB == null)
        {
            Debug.Log("There is No LuaBehaviour on new GameObj." + prefab.name);
            return 0;
        }
        lua_.RawGetI(LuaAPI.LUA_REGISTRYINDEX, luaB.Object_ref);
        return 1;
    }

    [MonoPInvokeCallback(typeof(LuaAPI.lua_CFunction))]
    public static int AddChildWithPosition(IntPtr l)
    {
        GameObject parent = lua_.ToUserDataObject(1) as GameObject;
        string path = lua_.ToString(2);
        Vector3 positon = (Vector3)lua_.ToUserDataObject(3);


        GameObject prefab = (GameObject)ResLoader.Load(path, typeof(GameObject));
        GameObject obj = NGUITools.AddChildWithPosition(parent, prefab, positon);
        LuaBehaviour luaB = obj.GetComponent<LuaBehaviour>();
        if (luaB == null)
        {
            Debug.Log("There is No LuaBehaviour on new GameObj." + prefab.name);
            return 0;
        }
        lua_.RawGetI(LuaAPI.LUA_REGISTRYINDEX, luaB.Object_ref);
        return 1;
    }

    [MonoPInvokeCallback(typeof(LuaAPI.lua_CFunction))]
    public static int AddChildWithLocalPosition(IntPtr l)
    {
        GameObject parent = lua_.ToUserDataObject(1) as GameObject;
        string path = lua_.ToString(2);
        Vector3 localPositon = (Vector3)lua_.ToUserDataObject(3);

        GameObject prefab = (GameObject)ResLoader.Load(path, typeof(GameObject));
        prefab.transform.position = localPositon;
        GameObject obj = NGUITools.AddChildNotLoseAnything(parent, prefab);
        LuaBehaviour luaB = obj.GetComponent<LuaBehaviour>();
        if (luaB == null)
        {
            Debug.Log("There is No LuaBehaviour on new GameObj." + prefab.name);
            return 0;
        }
        lua_.RawGetI(LuaAPI.LUA_REGISTRYINDEX, luaB.Object_ref);
        return 1;
    }

    [MonoPInvokeCallback(typeof(LuaAPI.lua_CFunction))]
    public static int AddChildNotLose(IntPtr l)
    {
        GameObject parent = lua_.ToUserDataObject(1) as GameObject;
        string path = lua_.ToString(2);
        GameObject prefab = (GameObject)ResLoader.Load(UIPrefabDummy.GetUIPrefabPath(path), typeof(GameObject));
        GameObject obj = NGUITools.AddChildNotLoseAnything(parent, prefab);
        LuaBehaviour luaB = obj.GetComponent<LuaBehaviour>();
        if (luaB == null)
        {
            Debug.Log("There is No LuaBehaviour on new GameObj." + prefab.name);
            return 0;
        }
        lua_.RawGetI(LuaAPI.LUA_REGISTRYINDEX, luaB.Object_ref);
        return 1;
    }

    [MonoPInvokeCallback(typeof(LuaAPI.lua_CFunction))]
    public static int AddChildNoBehaviour(IntPtr l)
    {
        GameObject parent = lua_.ToUserDataObject(1) as GameObject;
        string path = lua_.ToString(2);
        GameObject prefab = (GameObject)ResLoader.Load(UIPrefabDummy.GetUIPrefabPath(path), typeof(GameObject));
        GameObject obj = NGUITools.AddChildNotLoseAnything(parent, prefab);
        lua_.NewClassUserData(obj);
        return 1;
    }

    [MonoPInvokeCallback(typeof(LuaAPI.lua_CFunction))]
    public static int AddChildByResourcesPath(IntPtr l)
    {
        GameObject parent = lua_.ToUserDataObject(1) as GameObject;
        string path = lua_.ToString(2);

        GameObject prefab = (GameObject)ResLoader.Load(path, typeof(GameObject));
        GameObject obj = NGUITools.AddChildNotLoseAnything(parent,prefab );
        LuaBehaviour luaB = obj.GetComponent<LuaBehaviour>();
        if (luaB == null)
        {
            Debug.Log("There is No LuaBehaviour on new GameObj." + obj.name);
            //lua_.RawGetI(LuaAPI.LUA_REGISTRYINDEX, obj.Object_ref);
            return 0;
        }
        lua_.RawGetI(LuaAPI.LUA_REGISTRYINDEX, luaB.Object_ref);
        return 1;
    }

    [MonoPInvokeCallback(typeof(LuaAPI.lua_CFunction))]
    public static int AddChildByResourcesPathNoBehaviour(IntPtr l)
    {
        GameObject parent = lua_.ToUserDataObject(1) as GameObject;
        string path = lua_.ToString(2);
        GameObject prefab = (GameObject)ResLoader.Load(path, typeof(GameObject));
        GameObject obj = NGUITools.AddChildNotLoseAnything(parent, prefab);
        lua_.NewClassUserData(obj);
        return 1;
    }

    [MonoPInvokeCallback(typeof(LuaAPI.lua_CFunction))]
    public static int AddEffect(IntPtr l)
    {
        GameObject parent = lua_.ToUserDataObject(1) as GameObject;
        string path = lua_.ToString(2);
        Vector3 position = (Vector3)lua_.ToUserDataObject(3);
        GameObject prefab = (GameObject)ResLoader.Load(path, typeof(GameObject));
        GameObject obj = NGUITools.AddChildWithPosition(parent, prefab, position);
        lua_.NewClassUserData(obj);
        return 1;
    }

    
    [MonoPInvokeCallback(typeof(LuaAPI.lua_CFunction))]
    public static int AddEffectLocal(IntPtr l)
    {
        GameObject parent = lua_.ToUserDataObject(1) as GameObject;
        string path = lua_.ToString(2);
        Vector3 uiPositon = (Vector3)lua_.ToUserDataObject(3);
        Vector3 position = parent.transform.localToWorldMatrix.MultiplyPoint(uiPositon);   
        GameObject prefab = (GameObject)ResLoader.Load(path, typeof(GameObject));
        GameObject obj = NGUITools.AddChildWithPosition(parent, prefab, position);
        lua_.NewClassUserData(obj);
        return 1;
    }

}