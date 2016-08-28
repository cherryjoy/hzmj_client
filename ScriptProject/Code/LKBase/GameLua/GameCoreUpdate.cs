using UnityEngine;
using System.Collections;
using UniLua;

public class GameCoreUpdate : MonoBehaviour
{
    void Awake() {
        LuaState lua_ = LuaInstance.instance.Get();
#if !UNITY_ANDROID && !UNITY_IPHONE
        Instantiate(ResLoader.Load("UI/Prefab/UITools/FastTools"));
#endif
    }

    void OnDestroy() {
        Resources.UnloadUnusedAssets();
    }

    void Update()
    {
#if UNITY_EDITOR || LuaDebugger
         LuaState lua_ = LuaInstance.instance.Get();
        LuaAPI.lua_IdleDB(lua_.GetLuaPtr());
#endif

        PingManager.Instance.UpdatePingServer();

        LuaNetwork.Net.Update();
    }

    void LateUpdate() {
        LuaNetwork.Net.SendByteThisFrame();
    }
}
