using System;
using UnityEngine;
using UniLua;
using System.IO;
using System.Collections;
using System.Threading;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using System.Text;

class UpdateInitController : MonoBehaviour
{
    static bool IsInit = false;
    private LuaBehaviour mupdateBehaviour;
    string platformVersion = "0.0.0";
    private LKHttpRequest httRequest;
    private string platformCfg = "";
    private bool existsUpdateLua = false;
    void Awake()
    {

        Screen.sleepTimeout = SleepTimeout.NeverSleep;

#if UNITY_ANDROID
        Application.targetFrameRate = 30;
#endif
#if UNITY_STANDALONE_WIN
        Application.targetFrameRate = 60;
#endif
        GameQualityChecker.GameQualityLevel gameQualityLevel = GameQualityChecker.Instance.EstimateGameQualityLevel();
        if (gameQualityLevel == GameQualityChecker.GameQualityLevel.lowLevel)
        {
            Shader.globalMaximumLOD = 300;
        }
        else if (gameQualityLevel == GameQualityChecker.GameQualityLevel.highLevel)
        {
            Shader.globalMaximumLOD = 600;
        }
    }

    void Start()
    {
#if UNITY_STANDALONE_WIN
        if (!PluginTool.SharedInstance().Init())
        {
            Debug.LogError("PluginTool Init error!");
        }
        StreamReader sr = new StreamReader(Path.Combine(Application.streamingAssetsPath, "platformconfig"));
        platformCfg = sr.ReadToEnd();
        sr.Close();
        StartLua(false);
#else
        if (!PluginTool.SharedInstance().Init())
        {
            Debug.LogError("PluginTool Init error!");
        }
        GetPlatformCfg();
#endif
    }

    void FixedUpdate()
    {
        if (httRequest != null)
        {
            httRequest.Update();
        }
        if (unZipInfo != null)
        {
            if (unZipInfo.unZip.IsError)
            {
                LuaInstance.instance.Get().LuaFuncCall(mupdateBehaviour.Object_ref, unZipInfo.unzipError);
                unZipInfo = null;
            }
            else if (unZipInfo.unZip.UnZipSuccess && !string.IsNullOrEmpty(unZipInfo.unzipSuccess))
            {
                existsUpdateLua = unZipInfo.unZip.ExistsUpdateLua;
                LuaInstance.instance.Get().LuaFuncCall(mupdateBehaviour.Object_ref, unZipInfo.unzipSuccess,existsUpdateLua);
                unZipInfo = null;
            }
            else if (unZipInfo.unZip.UnZipIng && !string.IsNullOrEmpty(unZipInfo.upzipProgress))
            {
                LuaInstance.instance.Get().LuaFuncCall(mupdateBehaviour.Object_ref, unZipInfo.upzipProgress, unZipInfo.unZip.Progress);
            }
        }

        if (copyFile != null)
        {
            if (copyFile.Progress != -1)
            {
                LuaInstance.instance.Get().LuaFuncCall(mupdateBehaviour.Object_ref, copyFile.CopyProgressCallback, copyFile.Progress);
                //Debug.Log("copying" + copyFile.Progress);
            }
            else if (!string.IsNullOrEmpty(copyFile.CopyEndCallback))
            {
                LuaInstance.instance.Get().LuaFuncCall(mupdateBehaviour.Object_ref, copyFile.CopyEndCallback);
                //Debug.Log("copend ");
                copyFile.Progress = -1;
                copyFile = null;
                return;
            }
            else if (!string.IsNullOrEmpty(copyFile.CopyErrorCallback))
            {
                LuaInstance.instance.Get().LuaFuncCall(mupdateBehaviour.Object_ref, copyFile.CopyErrorCallback);
                copyFile.Progress = -1;
                copyFile = null;
                return;
            }
        }
    }

    void GetPlatformCfg()
    {

#if  UNITY_ANDROID
        string path =  "jar:file://" + Application.dataPath + "!/assets/"+"platformconfig";
        StartCoroutine(EasyDownload(path, (www) =>
            {
                platformCfg = www.text;
                www.Dispose();
                CheckFirstEnterGame();
            }));
#else
        StreamReader sr = new StreamReader(Path.Combine(Application.streamingAssetsPath, "platformconfig"));
        platformCfg = sr.ReadToEnd();
        sr.Close();
        CheckFirstEnterGame();
#endif
    }

    void CheckFirstEnterGame()
    {
        if (CompareVersion() || ExistsBackupIndex())
        {
            FirstUnzip();
        }
        else
        {

            StartLua(false);
        }
    }
    void FirstUnzip()
    {
        string persisitentDataPath = PluginTool.SharedInstance().PersisitentDataPath;
        if (!Directory.Exists(persisitentDataPath))
        {
            if (!Directory.Exists(persisitentDataPath))
            {
                Directory.CreateDirectory(persisitentDataPath);
            }
        }

        
#if  UNITY_ANDROID
#if  ASSETBUNDLE
        //PluginTool.SharedInstance().CopyAssetFromPackageToSdcard("temp/data", persisitentDataPath); // ? 在lua中以及有拷贝了，为啥这儿还有?
        //PluginTool.SharedInstance().CopyAssetFromPackageToSdcard("temp/index", persisitentDataPath);
        //PluginTool.SharedInstance().CopyAssetFromPackageToSdcard("temp/fragment", persisitentDataPath);
         
        StartLua(true);
#else
        string path = "jar:file://" + Application.dataPath + "!/assets/" + "StreamingFile.txt";
        StartCoroutine(EasyDownload(path, (www) =>
        {
            string text = www.text;
            www.Dispose();
            CopyAssetToSdcard(text, persisitentDataPath);
        }));
#endif
#else
        StartLua(true);
#endif
    }

    void CopyAssetToSdcard(string text, string destPath)
    {
        string[] lines = text.Split(new char[] { '\n' });
        for (int i = 0; i < lines.Length; ++i)
        {
            string line = lines[i];
            line = line.Replace("\n", "");
            line = line.Trim();
            if (!string.IsNullOrEmpty(line))
            {
                PluginTool.SharedInstance().CopyAssetFromPackageToSdcard(line, destPath);
            }
        }

        StartLua(true);
    }

    void StartLua(bool onlyRead)
    {
        InstanceGlobalUpdate();
        if (!IsInit)
        {
            if (onlyRead)
            {
#if UNITY_ANDROID
                ResLoader.InitStreamingAssetsPath(PluginTool.SharedInstance().PersisitentDataPath + "temp");
                ResLoader.InitSmallData();
#elif UNITY_IPHONE //|| UNITY_STANDALONE_WIN || UNITY_EDTOR
                UnityEngine.iOS.Device.SetNoBackupFlag(PluginTool.SharedInstance().PersisitentDataPath);
                ResLoader.InitStreamingAssetsPath(Application.streamingAssetsPath);
                ResLoader.Init();
#endif
            }
            else
            {
                IsInit = true;
                ResLoader.InitData();
                ResLoader.Init();
            }


            LuaInstance.instance.Init(true);
            //IsInit = true;
        }

        if (GlobalUpdate.Instance.gameObject.GetComponent<LogSystem>() == null)
        {
            GlobalUpdate.Instance.gameObject.AddComponent<LogSystem>();
            LogSystem.ReInit(PluginTool.SharedInstance().GetClientVersion());
        }

        LuaState lua = LuaInstance.instance.Get();
        lua.L_DoString(platformCfg);
        GameObject updateUI = NGUITools.AddChildByPath("Update/UpdateControl", null);
        mupdateBehaviour = updateUI.GetComponent<LuaBehaviour>();
        lua.RawGetI(LuaAPI.LUA_REGISTRYINDEX, mupdateBehaviour.Object_ref);
        lua.NewClassUserData(this);
        lua.SetField(-2, "UpdateCtr");

    }

    bool CompareVersion()
    {
        string clientVersion = GetClientVersion();
        platformVersion = AnalysisPlatformCfg(platformCfg);
        if (clientVersion == null)
        {
            return true;
        }
        string[] clientVersions = clientVersion.Split(new char[] { '.' });
        string[] platformVersions = platformVersion.Split(new char[] { '.' });
        if (int.Parse(clientVersions[0]) < int.Parse(platformVersions[0]))
        {
            return true;
        }
        else if (int.Parse(clientVersions[0]) == int.Parse(platformVersions[0]))
        {
            if (int.Parse(clientVersions[1]) < int.Parse(platformVersions[1]))
            {
                return true;
            }
            else if (int.Parse(clientVersions[1]) == int.Parse(platformVersions[1]))
            {
                if (int.Parse(clientVersions[2]) < int.Parse(platformVersions[2]))
                {

                    return true;
                }
            }
        }
        return false;
    }

    string GetClientVersion()
    {
        string path = Path.Combine(PluginTool.SharedInstance().PersisitentDataPath, "ClientVersion.txt");
        if (!File.Exists(path))
        {
            return null;
        }
        StreamReader sr = new StreamReader(path);
        return sr.ReadToEnd();
    }



    IEnumerator EasyDownload(string path, Action<WWW> callback)
    {
        WWW www = new WWW(path);
        yield return www;
        if (www.error != null)
        {
            Debug.Log("www error " + www.url);
        }
        else
        {
            callback(www);
        }
    }

    string AnalysisPlatformCfg(string text)
    {
        string[] lines = text.Split(new char[] { '\n' });
        foreach (string line in lines)
        {
            if (line.Contains("GameDefine_version"))
            {
                return line.Split(new char[] { '=' })[1].Replace("\"", "").Replace(" ", "");
            }

        }
        return null;
    }


    class LKUnZipInfo
    {
        public LKUnZip unZip = new LKUnZip();
        public string unzipSuccess;
        public string upzipProgress;
        public string unzipError;
    }
    LKUnZipInfo unZipInfo;

    public void Unzip(string path, string unzipSuccess, string upzipProgress, string unzipError)
    {
        string persisitentDataPath = PluginTool.SharedInstance().PersisitentDataPath;
        LuaState lua = LuaInstance.instance.Get();
        if (!File.Exists(path))
        {
            lua.LuaFuncCall(mupdateBehaviour.Object_ref, unzipError);
            return;
        }
        unZipInfo = new LKUnZipInfo();
        unZipInfo.unZip.ExcuteUnzip(path, persisitentDataPath);
        unZipInfo.unzipSuccess = unzipSuccess;
        unZipInfo.unzipError = unzipError;
        unZipInfo.upzipProgress = upzipProgress;
    }


    void OnDestroy()
    {
        GameObject PlatformSDKObj = GameObject.Find("PlatformSDKObj");
        if (PlatformSDKObj != null)
        {
            Destroy(PlatformSDKObj);

        }
        /*GameObject HeadPortraitController = GameObject.Find("HeadPortraitController");
        if (HeadPortraitController != null)
        {
            Destroy(HeadPortraitController);
        }*/

        ResLoader.Close();
        CDataMgr.Instance.Clear();
        ResLoader.InitData();
        ResLoader.Init();
        LuaInstance.instance.Reset();
        LuaState lua = LuaInstance.instance.Get();
        lua.L_DoString(platformCfg);
        Debug.Log("IsInit: " + IsInit + ", existsUpdateLua: " + existsUpdateLua);
        if (IsInit && !existsUpdateLua)
        {
            LuaInstance.instance.DoFile("Utils/Util.txt");
            lua.GetGlobal("Util");
            lua.GetField(-1, "LoadLoginScene");
            lua.PCall(0, 0, 0);
            lua.Pop(1);
        }
        else
        {
            IsInit = true;
        }
    }

    public void LoadLoginScene()
    {
        LuaState lua = LuaInstance.instance.Get();
        LuaInstance.instance.DoFile("Utils/Util.txt");
        lua.GetGlobal("Util");
        lua.GetField(-1, "LoadLoginScene");
        lua.PCall(0, 0, 0);
        lua.Pop(1);

    }
    public void HttpDownLoadXml(string url, string downloadEnd, string downloadError)
    {
        httRequest = null;
        httRequest = new LKHttpRequest2String((str) =>
            {
                //CallLuafunction(downloadEnd, str);
                LuaInstance.instance.Get().LuaStaticFuncCall(mupdateBehaviour.Luaclass_ref, downloadEnd, str);
            }, (error) =>
                {
                    LuaInstance.instance.Get().LuaStaticFuncCall(mupdateBehaviour.Luaclass_ref, downloadError, error);
                });
        httRequest.Download(url, "", "");
    }
    public void HttpDownLoadFile(string url, string path, string writeSizeFilePath, string downloadbegin, string downloading, string downloadEnd, string downloadError)
    {
        httRequest = null;
        httRequest = new LKHttpRequest2File((offset,downloadLength)=>
            {
                LuaInstance.instance.Get().LuaStaticFuncCall(mupdateBehaviour.Luaclass_ref, downloadbegin, offset, downloadLength);
            },
             (progress)=>
                 {
                     LuaInstance.instance.Get().LuaStaticFuncCall(mupdateBehaviour.Luaclass_ref, downloading, progress);
                 },
                 (endStr)=>
                     {
                         LuaInstance.instance.Get().LuaStaticFuncCall(mupdateBehaviour.Luaclass_ref, downloadEnd, endStr);
                     },
                     (error)=>
                     {
                         Debug.Log(error);
                         LuaInstance.instance.Get().LuaStaticFuncCall(mupdateBehaviour.Luaclass_ref, downloadError, error);
                     });
        httRequest.Download(url, path, writeSizeFilePath);
    }
    public void Quit()
    {
        Application.Quit();
    }

    public void StartCopyFullData(string fileInPackage, string destDir)
    {
        PluginTool.SharedInstance().ThreadCopyAssetFromPackageToSdcard(gameObject.name, fileInPackage, destDir);
    }

    public void AndroidReadFileError(string msg)
    {
        Debug.Log(msg);
        LuaInstance.instance.Get().LuaFuncCall(mupdateBehaviour.Object_ref, "PackageError");
    }
    public void AndroidReadFileProgress(string progress)
    {
        float p = float.Parse(progress);

        LuaInstance.instance.Get().LuaFuncCall(mupdateBehaviour.Object_ref, "UnzipProgress", p);
    }

    public void AndroidReadFileSuccess(string ext)
    {
        LuaInstance.instance.Get().LuaFuncCall(mupdateBehaviour.Object_ref, "AndroidCopayEnd");
    }

    public void ConfirmDownload()
    {
        if (httRequest != null)
        {
            httRequest.SetBegianDownload();
        }
    }

    LKThreadCopyFile copyFile = null;

    public void ThreadCopyFile(string file, string destPath, string copyEndCallback, string copyProgressCallback, string copyErrorCallback)
    {
        copyFile = new LKThreadCopyFile();
        copyFile.ThreadCopyFile(file, destPath, copyProgressCallback, copyEndCallback, copyErrorCallback);
    }

    public void CopyFile(string file, string destPath)
    {
        FileInfo fi = new FileInfo(file);
        fi.CopyTo(destPath, true);
    }

    void InstanceGlobalUpdate()
    {
        GameObject neverDestroyObj = GameObject.Find("NeverDestroyObj");
        if (neverDestroyObj == null)
        {
            neverDestroyObj = new GameObject("NeverDestroyObj");
        }
        if (neverDestroyObj.GetComponent<GlobalUpdate>() == null)
        {
            neverDestroyObj.AddComponent<GlobalUpdate>();
        }
        neverDestroyObj.GetComponent<GlobalUpdate>();
    }
    public bool ExistsBackupIndex()
    {
        string path = Path.Combine(PluginTool.SharedInstance().PersisitentDataPath, "index1");
        string clientVersionPath = Path.Combine(PluginTool.SharedInstance().PersisitentDataPath, "ClientVersion.txt");
        if (File.Exists(path))
        {
            File.Delete(clientVersionPath);
            File.Delete(path);
            return true;
        }
        return false;
    }

    public void DeleteFile(string path)
    {
        File.Delete(path);
    }

}
