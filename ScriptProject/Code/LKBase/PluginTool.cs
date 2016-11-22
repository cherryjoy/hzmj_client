using UnityEngine;
using System.Collections;
using System.Runtime.InteropServices;
using System;
public class PluginTool {

    public static readonly string VERSION_KEY = "version";
    public static readonly string PLATFORM_KEY = "platform";
    public static readonly string GAMEDEFINE_KEY = "gamedefine";
    public static readonly string GAMEID_KEY = "gameid";
    public string logUrl = "http://www.cherryjoy.com:8888/index.php";
	public String logStr;
  
    private static PluginTool toolManager = null;
#if UNITY_IPHONE
    [DllImport("__Internal")]
    private static extern string getGameDefineContent();
    [DllImport("__Internal")]
    private static extern string getPlatformConfig(string key,string defaultvalue);
    [DllImport("__Internal")]
    private static extern string getPersistentDataPath();
    [DllImport("__Internal")]
    private static extern string getClientVersion();
    [DllImport("__Internal")]
    private static extern string getClientVersionCode();
    [DllImport("__Internal")]
    private static extern int getClientFreeStorage();
    [DllImport("__Internal")]
    private static extern string  getVersionName();
    [DllImport("__Internal")]
    static extern string GetMacAddressiOS();
   
#endif
    private PluginTool()
    {

    }

    public static PluginTool SharedInstance()
    {
        if (toolManager == null)
        {
            toolManager = new PluginTool();
        }

        return toolManager;
    }

    private string persistentDataPath = "";
    public string PersisitentDataPath
    {
        get
        {
            return persistentDataPath;
        }
    }

#if UNITY_ANDROID
    private AndroidJavaObject androidBrigde = null;
#endif
    public bool Init()
    {
#if UNITY_ANDROID

        AndroidJavaClass androidClass = new AndroidJavaClass("com.cm.plugin.PluginTool");
        if (androidClass != null)
        {
            androidBrigde = androidClass.CallStatic<AndroidJavaObject>("SharedInstance");
        }

        if (androidBrigde == null)
        {
            return false;
        }
        persistentDataPath = androidBrigde.Call<string>("GetPersistentDataPath");
        if (persistentDataPath == null)
        {
            return false;
        }

#elif UNITY_IPHONE
       persistentDataPath = getPersistentDataPath();
        if(persistentDataPath == null)
        {
            return false;
        }
#elif UNITY_EDITOR
        persistentDataPath = Application.persistentDataPath+"/";
#endif
        Debug.Log(persistentDataPath);
        return true;
    }

    public string GetGameDefineContent()
    {
#if UNITY_ANDROID 
        if (androidBrigde != null)
        {
            return androidBrigde.Call<string>("GetGameDefineContent");
        }
#elif UNITY_IPHONE
        return getGameDefineContent();
#endif

        return null;
    }

    public string GetClientVersion()
    {
#if UNITY_ANDROID 
        if (androidBrigde != null)
        {
            return androidBrigde.Call<string>("GetClientVersion");
        }
#elif UNITY_IPHONE
        return getClientVersion();
#endif

        return null;
    }

    public string GetPlatformConfig(string key,string defalutValue)
    {
#if UNITY_ANDROID 
        if (androidBrigde != null)
        {
            return androidBrigde.Call<string>("GetPlatformConfigContent",key,defalutValue);
        }
#elif UNITY_IPHONE
        return getPlatformConfig(key,defalutValue);
#endif
        return defalutValue;
    }

    public int GetClientFreeStorage()
    {
#if UNITY_ANDROID 
         if (androidBrigde != null)
        {
            return androidBrigde.Call<int>("GetClientFreeStorage");
        }
#elif UNITY_IPHONE
        return getClientFreeStorage();
#endif
        return 0;
    }

    public string GetClientVersionCode()
    {
#if UNITY_ANDROID 
         if (androidBrigde != null)
        {
            return androidBrigde.Call<string>("GetClientVersionCode");
        }
#elif UNITY_IPHONE
        return getClientVersionCode();
#endif
        return null;
    }
   
    public bool SDCardExists()
    {
#if UNITY_ANDROID 
         if (androidBrigde != null)
        {
            return androidBrigde.Call<bool>("SDCardExists");
        }
#endif
        return true;
    }

    public void ThreadCopyAssetFromPackageToSdcard(string gameObjectName, string fileInPackage, string destDir)
    {
#if UNITY_ANDROID
       if (androidBrigde != null)
        {
            androidBrigde.Call("ThreadCopyAssetFromPackageToSdcard", gameObjectName, fileInPackage, destDir);
        }
#endif
    }

    public void CopyAssetFromPackageToSdcard(string fileInPackage, string destDir)
    {
#if UNITY_ANDROID
        if (androidBrigde != null)
        {
            androidBrigde.Call("CopyAssetFromPackageToSdcard", fileInPackage, destDir);
        }
#endif
    }

    public string GetVersionName()
    {
#if UNITY_ANDROID
        if (androidBrigde != null)
        {
            return androidBrigde.Call<string>("GetVersionName");
        }
#elif UNITY_IPHONE
        return getVersionName();
#endif
        return null;
    }

    public void InstallApk(string path)
    {
#if UNITY_ANDROID
        if (androidBrigde != null)
        {
            androidBrigde.Call("InstallApk",path);
        }
#endif
    }
    public string GetMacAddress()
    {
        string macaddr = "";
#if UNITY_IPHONE
        macaddr = GetMacAddressiOS();
#endif
        return macaddr;
    }
}
