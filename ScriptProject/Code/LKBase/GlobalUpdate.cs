using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UniLua;

public class GlobalUpdate : MonoBehaviour
{
    public static GlobalUpdate Instance
    {
        get {
            GameObject obj = GameObject.Find("NeverDestroyObj");
            GlobalUpdate glo = obj.GetComponent<GlobalUpdate>();
            return glo; }
    }

    void Awake()
    {
        DontDestroyOnLoad(gameObject);
#if ASSETBUNDLE
        StartCoroutine(LoadBundleCoroutine());
#endif
    }

    void Update()
    {
        LuaState lua_ = LuaInstance.instance.Get();
        TimerManager.Instance.Update();
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            lua_.RawGetI(LuaAPI.LUA_REGISTRYINDEX, LuaInstance.instance.client_table_ref);
            lua_.GetField(-1, "EscapeBtnOnClick");
            lua_.PCall(0, 0, 0);
            lua_.Pop(1);
        }
    }

    public void OnApplicationPause(bool pauseStatus)
    {
        LuaState lua_ = LuaInstance.instance.Get();
        if (LuaInstance.instance.client_table_ref != LuaAPI.LUA_REFNIL)
        {
            lua_.RawGetI(LuaAPI.LUA_REGISTRYINDEX, LuaInstance.instance.client_table_ref);
            lua_.GetField(-1, "FuncApplicationPause");
            lua_.PushBoolean(pauseStatus);
            lua_.PCall(1, 0, 0);
            lua_.Pop(1);
        }
    }

    void OnApplicationQuit()
    {
        LuaInstance.instance.Get().Close();
        ResLoader.Close();
        LuaNetwork.Net.DisConnect();
    }

    public void GetWWW(LuaBehaviour eventReceiver, string url, string successFuncName, string failFuncName)
    {
        StartCoroutine(StartDownLoad(eventReceiver, url, successFuncName, failFuncName));
    }

    IEnumerator StartDownLoad(LuaBehaviour luaBehav,string url, string successFuncName, string failFuncName, bool isDispose = true)
    {
        LuaState lua_ = LuaInstance.instance.Get();
        WWW www = new WWW(url);
        yield return www;
        if (www.error != null)
        {
            Debug.LogWarning("Download error : " + url);
            Debug.LogWarning(www.error);

            lua_.LuaFuncCall(luaBehav.Object_ref, failFuncName);
            yield break;
        }

        lua_.LuaFuncCall(luaBehav.Object_ref, successFuncName, url, www);

        if (www != null && isDispose)
        {
            www.Dispose();
        }

    }

    public void DoAfterOneFrame(string tabelName, string callBackName)
    {
        StopCoroutine("WaitTodo");
        StartCoroutine(WaitTodo(tabelName, callBackName));
    }

    IEnumerator WaitTodo(string tabelName, string callBackName)
    {
        yield return 1;
        LuaState lua_ = LuaInstance.instance.Get();
        lua_.GetGlobal(tabelName);
        lua_.GetField(-1, callBackName);
        lua_.PCall(0, 0, 0);
        lua_.Pop(1);
    }

    public void HideAllCam()
    {
        Camera[] cams = Camera.allCameras;
        foreach (var cam in cams)
        {
            cam.enabled = false;
        }
    }

        #region async load bundles
#if ASSETBUNDLE
    private Queue<LoadBundleNode> loadBundle_queue_ = new Queue<LoadBundleNode>();
    IEnumerator LoadBundleCoroutine()
    {
        while (true)
        {
            if (loadBundle_queue_.Count > 0)
            {
                LoadBundleNode node = loadBundle_queue_.Dequeue();

                AssetBundle bundle = ResLoader.GetAssetBundle(node.bundle_name_);
                if (bundle == null)
                {
                    AssetBundleCreateRequest bundle_request = ResLoader.LoadBundleAsync(node.bundle_name_);
                    yield return bundle_request;

                    if (bundle_request != null)
                    {
                        bundle = bundle_request.assetBundle;
#if DEBUG
                        bundle.name = "async_" + node.bundle_name_;
#endif
                        ResLoader.AddBundleNode(bundle, node.bundle_name_);
                    }   
                }

                if (bundle != null)
                {
                    AssetBundleRequest asest_request = ResLoader.LoadAssetAsync(bundle);
                    yield return asest_request;

                    if (asest_request != null)
                    {
                        if (node.onload_ != null)
                        {
                            node.onload_(asest_request.asset);
                        }
                    }
                }
            }
            yield return null;
        }
    }

    public void AddLoadBundleNode(LoadBundleNode node)
    {
        loadBundle_queue_.Enqueue(node);
    }
#endif
        #endregion
}
