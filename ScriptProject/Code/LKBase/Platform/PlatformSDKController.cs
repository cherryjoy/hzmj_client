using UnityEngine;
using System.Collections;
using System.Reflection;
using System.Collections.Generic;
using UniLua;

public class PlatformSDKController : MonoBehaviour
{
    public static PlatformSDKController mSelf = null;
    public PlatformState SDKState = null;
    public int luaPlatformHanderRef;

    void Awake()
    {
        if (mSelf != null)
        {
            if (mSelf != this)
            {
                GameObject.Destroy(this);
                return;
            }
        }
        mSelf = this;
        SDKState = new PlatformState();
        LuaState lua = LuaInstance.instance.Get();
        lua = LuaInstance.instance.Get();
        lua.GetGlobal("PlatformHandler");
        luaPlatformHanderRef = lua.L_Ref(LuaAPI.LUA_REGISTRYINDEX);
    }

    void OnDestroy()
    {
        LuaState lua = LuaInstance.instance.Get();
        lua.L_Unref(LuaAPI.LUA_REGISTRYINDEX, ref luaPlatformHanderRef);
    }

    public void Start()
    {
        LuaState lua = LuaInstance.instance.Get();
        lua.LuaFuncCall(luaPlatformHanderRef, "Start");

    }

	public void ShareScreenShot(string objName, string title, string description, string contextUrl, string picName)
	{
		StartCoroutine(DoScreenShot(objName, title, description, contextUrl, picName));
	}

	IEnumerator DoScreenShot(string objName, string title, string description, string contextUrl, string picName)
	{
		yield return new WaitForEndOfFrame();
		Rect rect = new Rect(Screen.width*0f, Screen.height*0f, Screen.width*1f, Screen.height*1f);
		Texture2D tex = new Texture2D((int)rect.width, (int)rect.height, TextureFormat.RGB24, false);
		tex.ReadPixels(rect, 0, 0);
		tex.Apply();

		int width = 1334;
		if (rect.width <= width)
		{
			byte[] bytes = tex.EncodeToPNG();
			string fileName = Application.persistentDataPath + "/" + picName;
			System.IO.File.WriteAllBytes(fileName, bytes);
		}
		else
		{
			int height = (int)(width * (rect.height / rect.width));
			Texture2D result = new Texture2D(width, height, tex.format, false);
			for (int i = 0; i < result.height; ++i)
			{
				for (int j = 0; j < result.width; ++j)
				{
					Color newColor = tex.GetPixelBilinear((float)j / result.width, (float)i / result.height);
					result.SetPixel(j, i, newColor);
				}
			}

			result.Apply();
			byte[] bytes = result.EncodeToPNG();
			string fileName = Application.persistentDataPath + "/" + picName;
			System.IO.File.WriteAllBytes(fileName, bytes);
		}

		PlatformState.Instance.SDKShareCallBack(objName, title, description, contextUrl, picName);
	}

	/// <summary>
	/// SDK返回
	/// </summary>

    public void InitSdk()
    {
        LuaState lua = LuaInstance.instance.Get();
        lua.LuaFuncCall(luaPlatformHanderRef, "onSdkInit");
    }

    //第三方sdk初始化完成的消息
    void onInitFinish(string msg)
    {
        LuaState lua = LuaInstance.instance.Get();
        lua.LuaStaticFuncCall(luaPlatformHanderRef, "onInitFinish", msg);
    }

    void onLoginFinish(string msg)
    {
        LuaState lua = LuaInstance.instance.Get();
        lua.LuaStaticFuncCall(luaPlatformHanderRef, "onLoginFinish", msg);
    }

    void onShareFinish(string msg)
    {
        LuaState lua = LuaInstance.instance.Get();
        lua.LuaStaticFuncCall(luaPlatformHanderRef, "onShareFinish", msg);
    }

    //void onLogoutFinish(string msg)
    //{
    //    LuaState lua = LuaInstance.instance.Get();
    //    lua.LuaStaticFuncCall(luaPlatformHanderRef, "onLogoutFinish", msg);
    //}
    //void onExitFinsh(string msg)
    //{
    //    LuaState lua = LuaInstance.instance.Get();
    //    lua.LuaStaticFuncCall(luaPlatformHanderRef, "onSdkExit", msg);
    //}
    //void OnPayFinish(string msg)
    //{
    //    LuaState lua = LuaInstance.instance.Get();
    //    lua.LuaStaticFuncCall(luaPlatformHanderRef, "OnPayFinish", msg);
    //}

    //void onSwitchAccountFinish(string msg)
    //{
    //    LuaState lua = LuaInstance.instance.Get();
    //    lua.LuaStaticFuncCall(luaPlatformHanderRef, "onSwitchAccountFinish", msg);
    //}

}
