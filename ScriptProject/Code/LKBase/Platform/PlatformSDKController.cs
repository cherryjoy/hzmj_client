//using UnityEngine;
//using System.Collections;
//using System.Reflection;
//using System.Collections.Generic;
//using UniLua;

//public class PlatformSDKController : MonoBehaviour
//{
//    public static PlatformSDKController mSelf = null;
//    public PlatformState SDKState = null;
//    int luaPlatformHanderRef;

//    void Awake()
//    {
//        if (mSelf != null)
//        {
//            if (mSelf != this)
//            {
//                GameObject.Destroy(this);
//                return;
//            }
//        }
//        mSelf = this;
//        SDKState = new PlatformState();
//        LuaState lua = LuaInstance.instance.Get();
//        lua = LuaInstance.instance.Get();
//        lua.GetGlobal("PlatformHandler");
//        luaPlatformHanderRef = lua.L_Ref(LuaAPI.LUA_REGISTRYINDEX);
//    }

//    void OnDestroy() 
//    {
//        LuaState lua = LuaInstance.instance.Get();
//        lua.L_Unref(LuaAPI.LUA_REGISTRYINDEX, ref luaPlatformHanderRef);
//    }

//    public void Start()
//    {
//        LuaState lua = LuaInstance.instance.Get();
//        lua.LuaFuncCall(luaPlatformHanderRef, "Start");
        
//    }

//    public void InitSdk()
//    {
//        LuaState lua = LuaInstance.instance.Get();
//        lua.LuaFuncCall(luaPlatformHanderRef, "onSdkInit");
//    }

//    //第三方sdk初始化完成的消息
//    void onInitFinish(string msg)
//    {
//        LuaState lua = LuaInstance.instance.Get();
//        lua.LuaStaticFuncCall(luaPlatformHanderRef, "onInitFinish", msg);
//    }

//    void onLoginFinish(string msg)
//    {
//        LuaState lua = LuaInstance.instance.Get();
//        lua.LuaStaticFuncCall(luaPlatformHanderRef, "onLoginFinish", msg);
//    }

//    void onLogoutFinish(string msg)
//    {
//        LuaState lua = LuaInstance.instance.Get();
//        lua.LuaStaticFuncCall(luaPlatformHanderRef, "onLogoutFinish", msg);
//    }
//    void onExitFinsh(string msg)
//    {
//        LuaState lua = LuaInstance.instance.Get();
//        lua.LuaStaticFuncCall(luaPlatformHanderRef, "onSdkExit", msg);
//    }
//    void OnPayFinish(string msg)
//    {
//        LuaState lua = LuaInstance.instance.Get();
//        lua.LuaStaticFuncCall(luaPlatformHanderRef, "OnPayFinish", msg);
//    }

//    void onSwitchAccountFinish(string msg)
//    {
//        LuaState lua = LuaInstance.instance.Get();
//        lua.LuaStaticFuncCall(luaPlatformHanderRef, "onSwitchAccountFinish", msg);
//    }

//}
