using UnityEngine;
using System.Collections;
using System;
using System.Collections.Generic;
using UniLua;
using System.IO;

public class ClientInitController : MonoBehaviour
{
    public GameObject UIParent;
    static bool IsInit = false;
    LuaBehaviour mLoginLuaBehav;
   // GameObject PlatformSDKObj;
    public static Vector2 ScreenSize;
    int getScreenCount = 2;
    
    //Init engine setting


    //Init Lua
    void Start() 
    {
        if (GlobalUpdate.Instance.gameObject.GetComponent<LogSystem>() == null)
        {
            GlobalUpdate.Instance.gameObject.AddComponent<LogSystem>();
            LogSystem.ReInit(PluginTool.SharedInstance().GetClientVersion());
        }
        GeneratSDKObj();
        GameObject loginUI = NGUITools.AddChildByPath("Login/LoginUIRoot", UIParent);
       mLoginLuaBehav = loginUI.GetComponent<LuaBehaviour>();
       LuaState lua = LuaInstance.instance.Get();
       lua.RawGetI(LuaAPI.LUA_REGISTRYINDEX, mLoginLuaBehav.Object_ref);
       lua.NewClassUserData(this);
       lua.SetField(-2, "ClientInitCtr");
        GameObject GameCoreUpdatePrefab = (GameObject)ResLoader.Load("UI/Prefab/NewSceneMustHave/GameCoreUpdate", typeof(GameObject));
        Instantiate(GameCoreUpdatePrefab);

        //init game logic after lua finish init and show the UI
        InitGameLogic();
    }

    void Update() {
        if (getScreenCount >0)
        {
            ScreenSize = new Vector2(Screen.width, Screen.height);
            getScreenCount--;
        }
    }

    void GeneratSDKObj() 
    {
        LuaState lua = LuaInstance.instance.Get();
        lua.GetGlobal("GameDefine_IsNeedAddPlatformSDKObj");
        bool IsNeedAddPlatformSDKObj = lua.ToBoolean(-1);
        lua.Pop(1);
        if (IsNeedAddPlatformSDKObj)
        {
            GameObject PlatformSDKObj = GameObject.Find("PlatformSDKObj");
            if (PlatformSDKObj == null)
            {
                PlatformSDKObj = new GameObject("PlatformSDKObj");
                PlatformSDKObj.AddComponent<PlatformSDKController>();
                DontDestroyOnLoad(PlatformSDKObj);
            }
            else if (PlatformSDKObj.GetComponent<PlatformSDKController>() == null)
            {
                PlatformSDKObj.AddComponent<PlatformSDKController>();
            }
            PlatformSDKController controller = PlatformSDKObj.GetComponent<PlatformSDKController>();
            controller.InitSdk();
        }
    }

    void InitGameLogic() {
       
    }

    public void StartDownload(string url, string successFuncName, string failFuncName)
    {
        StartCoroutine(EasyDownload(url, successFuncName, failFuncName));
    }

    public void GetDownLoadTexture(string url, string successFuncName, string failFuncName)
    {
        StartCoroutine(StartDownLoadTexture(url, successFuncName, failFuncName));
    }

    IEnumerator StartDownLoadTexture(string url, string successFuncName, string failFuncName, bool isDispose = true)
    {
        WWW www = new WWW(url);
        yield return www;
        LuaState lua = LuaInstance.instance.Get();
        if (www.error != null)
        {
            Debug.LogWarning("Download error : " + url);
            Debug.LogWarning(www.error);

            lua.LuaFuncCall(mLoginLuaBehav.Object_ref, failFuncName);
            yield break;
        }

        lua.LuaFuncCall(mLoginLuaBehav.Object_ref, successFuncName,url, www);

        if (www != null && isDispose)
        {
            www.Dispose();
        }

    }

    IEnumerator EasyDownload(string url, string successFuncName, string failFuncName, bool isDispose = true)
    {
        WWW www = new WWW(url);
        yield return www;
        LuaState lua = LuaInstance.instance.Get();
        if (www.error != null)
        {
            Debug.LogWarning("Download error : " + url);
            Debug.LogWarning(www.error);

            lua.LuaFuncCall(mLoginLuaBehav.Object_ref, failFuncName);
            yield break;
        }
        string xmlString = System.Text.Encoding.UTF8.GetString(www.bytes, 0, www.bytes.Length);
        lua.LuaFuncCall(mLoginLuaBehav.Object_ref, successFuncName, xmlString);
        if (www != null && isDispose)
        {
            www.Dispose();
        }

    }

}

