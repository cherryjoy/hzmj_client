using UnityEngine;
using System.Collections;
using System;
using System.Collections.Generic;
using UniLua;
using System.Runtime.InteropServices;

[Serializable]
public class LuaGameObject
{
    public GameObject Obj;//use by Inspector Only
    public UnityEngine.Object regObj;
    public string Name;
    public int TypeIndex;
}

[Serializable]
public class LuaBehav
{
    public LuaBehaviour Behav;
    public string Name;
}

public enum LuaBehaviourType{
    Normal = -1,
    Main = 0,   
}

public class LuaBehaviour : MonoBehaviour
{
    public LuaBehaviourType LuaBehavType = LuaBehaviourType.Normal;
    public string scriptShortPath; // set by LuaBehaviourInspetor.cs when lua_script is set
    public string ScriptName;
    public LuaGameObject[] lua_params;
    public LuaBehav[] LuaBehavArray;
    public bool IsOnClickPassGameObject = false;
    public bool isArray = false;
    public bool isBehavArray = false;

    //function refs
    protected int func_start_ref_ = LuaAPI.LUA_REFNIL;
    protected int func_update_ref_ = LuaAPI.LUA_REFNIL;
    protected int func_destroy_ref_ = LuaAPI.LUA_REFNIL;

    // lua component object ref
    private int object_ref_;
    public int Object_ref
    {
        get
        {
            return object_ref_;
        }

        set
        {
            object_ref_ = value;
        }
    }
    // lua class ref
    [HideInInspector]
    public int Luaclass_ref = LuaAPI.LUA_REFNIL;
    public LuaBehaviour()
    {
        LuaState lua_ = LuaInstance.instance.Get();
        lua_.NewTable();
        Object_ref = lua_.L_Ref(LuaAPI.LUA_REGISTRYINDEX);    
    }

    void Awake()
    {
        LuaState lua_ = LuaInstance.instance.Get();
       // Debug.Log("start:" + gameObject.name + Object_ref);
        // only load file one time
        if (!string.IsNullOrEmpty(scriptShortPath))
            LuaInstance.instance.DoFile(scriptShortPath);

        lua_.RawGetI(LuaAPI.LUA_REGISTRYINDEX, Object_ref);
        if (!string.IsNullOrEmpty(ScriptName)) {
            // save lua script
            lua_.GetGlobal(ScriptName);
            lua_.PushValue(-1);
            Luaclass_ref = lua_.L_Ref(LuaAPI.LUA_REGISTRYINDEX);

            //set metatable to ref table
            lua_.PushValue(-1);
            lua_.SetField(-2, "__index");
            lua_.SetMetaTable(-2);
        }
        //register LuaBehaviour List ref
        if (LuaBehavArray != null && LuaBehavArray.Length > 0)
        {
            for (int i = 0; i < LuaBehavArray.Length; i++)
            {
                if (LuaBehavArray[i].Behav == null)
                    continue;
                lua_.RawGetI(LuaAPI.LUA_REGISTRYINDEX, LuaBehavArray[i].Behav.Object_ref);
                if (!isBehavArray)
                {
                    if (string.IsNullOrEmpty(LuaBehavArray[i].Name))
                    {
                        lua_.SetField(-2, LuaBehavArray[i].Behav.name);
                    }
                    else {
                        lua_.SetField(-2, LuaBehavArray[i].Name);
                    }
                }
                else
                {
                    lua_.RawSetI(-2, i + 1);
                }
            }
        }

        RegisterLuaParams(isArray);

        //register LuaBehaviour GameObject it self
        if (!isArray && !string.IsNullOrEmpty(scriptShortPath))
        {
            LuaInstance.instance.RegisterGameObject(gameObject, "gameObject");
        }

        if (!string.IsNullOrEmpty(scriptShortPath))
        {
            RefBehaviourFuncs();
        }
        //         UnityEngine.Debug.Log(LuaInstance.ConstructString(lua_));
    }

    void RegisterLuaParams(bool isArray)
    {
        LuaState lua_ = LuaInstance.instance.Get();
        // register used CSharp class to table
        for (int i = 0; i < lua_params.Length; i++)
        {
            if (lua_params[i].Obj == null)
                continue;
            string regName;
            if (string.IsNullOrEmpty(lua_params[i].Name))
            {
                regName = lua_params[i].regObj.name;
            }else{
                regName = lua_params[i].Name;
            }
            RegComp(lua_params[i].regObj, lua_params[i].regObj.GetType(), regName, isArray);
        }

    }

    public void RegComp(UnityEngine.Object regObj, Type type ,string name,bool isArray = false) {
        LuaState lua_ = LuaInstance.instance.Get();
        Register.AddFuncInfo(type);
        LuaClass lua_class = LuaInstance.instance.RegisterLuaClass(type);
        FuncInfo[] funcInfos;
        if (lua_class.is_register == false)
        {
            if (LuaClassList.classes.TryGetValue(type, out funcInfos))
            {
                for (int j = 0; j < funcInfos.Length; j++)
                {
                    if (funcInfos[j].IsStatic)
                    {
                        lua_class.RegisterStaticFunction(funcInfos[j]);
                    }
                    else
                    {
                        lua_class.RegisterFunction(funcInfos[j]);
                    }
                }
            }

            lua_class.EndRegisterClass();
        }
        // register other CSharp object to lua object field
        if (isArray)
        {
            LuaInstance.instance.RegisterGameObject(regObj, Object_ref);
        }
        else {
            lua_.RawGetI(LuaAPI.LUA_REGISTRYINDEX, Object_ref);
            LuaInstance.instance.RegisterGameObject(regObj, name);
        }
    }

    void RefBehaviourFuncs()
    {
        LuaState lua_ = LuaInstance.instance.Get();
        if (string.IsNullOrEmpty(ScriptName)) {
            Debug.Log(scriptShortPath+"'s ScriptName is Null.");
        }

        lua_.RawGetI(LuaAPI.LUA_REGISTRYINDEX, Luaclass_ref);
        // save func ref
        lua_.GetField(-1, "Start");
        func_start_ref_ = lua_.L_Ref(LuaAPI.LUA_REGISTRYINDEX);

        lua_.GetField(-1, "Update");
        func_update_ref_ = lua_.L_Ref(LuaAPI.LUA_REGISTRYINDEX);

        lua_.GetField(-1, "OnDestroy");
        func_destroy_ref_ = lua_.L_Ref(LuaAPI.LUA_REGISTRYINDEX);
        lua_.Pop(1);
    }

    void Start()
    {
        LuaState lua_ = LuaInstance.instance.Get();
        if (func_start_ref_ == LuaAPI.LUA_REFNIL)
        {
            return;
        }
        lua_.RawGetI(LuaAPI.LUA_REGISTRYINDEX, func_start_ref_);
        lua_.RawGetI(LuaAPI.LUA_REGISTRYINDEX, Object_ref);

        lua_.PCall(1, 0, 0);

        //call Started Func
        CallClientFunc("FuncUIOpen", LuaBehavType);
    }

    void OnDestroy()
    {
        LuaState lua_ = LuaInstance.instance.Get();
        if (func_destroy_ref_ != LuaAPI.LUA_REFNIL)
        {
            // call lua destory function
            lua_.RawGetI(LuaAPI.LUA_REGISTRYINDEX, func_destroy_ref_);
            lua_.RawGetI(LuaAPI.LUA_REGISTRYINDEX, Object_ref);

            lua_.PCall(1, 0, 0);
        }

        //release Timer if it have
      //  Debug.Log("end:"+gameObject.name + Object_ref);
        lua_.RawGetI(LuaAPI.LUA_REGISTRYINDEX, Object_ref);
        lua_.GetField(-1, "Timer");
        bool isNull = lua_.IsNil(-1);
        if (!isNull)
        {
            lua_.GetField(-1, "Stop");
            lua_.PushValue(-2);
            lua_.PCall(1, 0, 0);
        }

        lua_.Pop(2);

        //call OnDestroy Func
        CallClientFunc("FuncUIDestroy", LuaBehavType);

        // release them all
        lua_.L_Unref(LuaAPI.LUA_REGISTRYINDEX, ref func_start_ref_);
        lua_.L_Unref(LuaAPI.LUA_REGISTRYINDEX, ref func_update_ref_);
        lua_.L_Unref(LuaAPI.LUA_REGISTRYINDEX, ref func_destroy_ref_);
        lua_.L_Unref(LuaAPI.LUA_REGISTRYINDEX, ref object_ref_);
        lua_.L_Unref(LuaAPI.LUA_REGISTRYINDEX, ref Luaclass_ref);
    }

    public void CallFunction(GameObject obj, string function)
    {
        LuaState lua_ = LuaInstance.instance.Get();
        lua_.RawGetI(LuaAPI.LUA_REGISTRYINDEX, Luaclass_ref);
   
        lua_.GetField(-1, obj.name + function); // get function name obj_function
        if (lua_.IsNil(-1)) { 
            Debug.LogError("Can't find function: "+obj.name + function);
            return;
        }
        lua_.RawGetI(LuaAPI.LUA_REGISTRYINDEX, Object_ref);

        if (!IsOnClickPassGameObject)
        {
            lua_.PCall(1, 0, 0);
        }
        else
        {
            lua_.NewClassUserData(obj);
            lua_.PCall(2, 0, 0);
        }

        lua_.Pop(1);
    }

    public void CallFunction(Vector2 delta, string function)
    {
        LuaState lua_ = LuaInstance.instance.Get();
        lua_.RawGetI(LuaAPI.LUA_REGISTRYINDEX, Luaclass_ref);
        lua_.GetField(-1,function);
        lua_.RawGetI(LuaAPI.LUA_REGISTRYINDEX, Object_ref);
        lua_.PushNumber(delta.x);
        lua_.PushNumber(delta.y);
        lua_.PCall(3, 0, 0);
        lua_.Pop(1);
    }

    public void CallFunction(GameObject sender, string function, System.Object para)
    {
        LuaState lua_ = LuaInstance.instance.Get();
        lua_.RawGetI(LuaAPI.LUA_REGISTRYINDEX, Luaclass_ref);
        if (sender == null)
        {
            lua_.GetField(-1, function);
        }
        else
        {
            lua_.GetField(-1, sender.name + function);
        }
        
         if (lua_.IsNil(-1)) {
             Debug.LogError("Can't find function: " + sender.name + function);
            return;
        }
        lua_.RawGetI(LuaAPI.LUA_REGISTRYINDEX, Object_ref);

        if (!LuaInstance.PushBaseTypeObj(lua_, para.GetType(), para))
        {
            lua_.NewClassUserData(para);
        }

        if (!IsOnClickPassGameObject || sender == null)
        {
            lua_.PCall(2, 0, 0);
        }
        else
        {
            lua_.NewClassUserData(sender);
            lua_.PCall(3, 0, 0);
        }
        lua_.Pop(1);
    }

     public void CallFunctionWithoutSenderName(GameObject obj, string function)
    {
        LuaState lua_ = LuaInstance.instance.Get();
        lua_.RawGetI(LuaAPI.LUA_REGISTRYINDEX, Luaclass_ref);
        lua_.GetField(-1, function);

        if (lua_.IsNil(-1)) { 
            Debug.LogError("Can't find function: " + function);
            return;
        }
        lua_.RawGetI(LuaAPI.LUA_REGISTRYINDEX, Object_ref);

        if (!IsOnClickPassGameObject)
        {
            lua_.PCall(1, 0, 0);
        }
        else
        {
            lua_.NewClassUserData(obj);
            lua_.PCall(2, 0, 0);
        }

        lua_.Pop(1);
    }

     void CallClientFunc(string funName,LuaBehaviourType behavType) {
        LuaState lua_ = LuaInstance.instance.Get();
         lua_.RawGetI(LuaAPI.LUA_REGISTRYINDEX, LuaInstance.instance.client_table_ref);
         if (lua_.IsNil(-1))
         {
             lua_.Pop(1);
             return;
         }
         
         lua_.GetField(-1, funName);
         lua_.RawGetI(LuaAPI.LUA_REGISTRYINDEX, Object_ref);
         lua_.PushInteger((int)behavType); 
         lua_.PCall(2, 0, 0);
         lua_.Pop(1);
     }

}
