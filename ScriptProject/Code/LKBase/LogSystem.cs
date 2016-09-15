using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UniLua;

public class LogSystem : MonoBehaviour
{
	public static readonly string LOGSYSTEMKEY = "logsystemkey";
	public static readonly string VERSIONKEY = "versionkey";
    private List<string> m_logList = new List<string>();
    void Awake()
    {
        Application.logMessageReceived += HandleLog;
        InitLogList();
    }

    void OnDestroy()
    {
        Application.logMessageReceived -= HandleLog;
    }

    public void InitLogList()
    {
		string logstr = PlayerPrefs.GetString(LOGSYSTEMKEY, "");
        string[] logs = logstr.Split(';');
		m_logList.Clear();
        for (int i = 0; i < logs.Length; i++)
        {
            m_logList.Add(logs[i]);
        }
    }

	public static void ReInit(string versionNowl)
	{
		string versionPre = PlayerPrefs.GetString(VERSIONKEY);
		if (versionNowl != versionPre)
		{
			PlayerPrefs.SetString(VERSIONKEY, versionNowl);
			PlayerPrefs.SetString(LOGSYSTEMKEY, "");
			PlayerPrefs.Save();
			LogSystem logSys = FindObjectOfType<LogSystem>();
			if (logSys != null)
			{
				logSys.InitLogList();
			}
		}
	}

    void SaveKeys(string key)
    {
        m_logList.Add(key);
        saveKeys();
    }

    void saveKeys()
    {
        int count = m_logList.Count;
        string keys = "";
        for (int i = 0; i < count; i++)
        {
            if (i == 0)
                keys = m_logList[i];
            else
                keys += ';' + m_logList[i];
        }
		PlayerPrefs.SetString(LOGSYSTEMKEY, keys);
    }


    void HandleLog(string logString, string stackTrace, LogType type)
    {
        if (type == LogType.Error || type == LogType.Exception)
        {
            string md5str = HashSecurity.Encrypt(logString + stackTrace);
            if (m_logList.Contains(md5str) == false)
            {
                string platform = "win";
#if UNITY_IPHONE
            platform = "ios";
#elif UNITY_ANDROID
            platform = "android";
#endif
                PluginTool pluginTools = PluginTool.SharedInstance();
                string gameId = pluginTools.GetPlatformConfig(PluginTool.GAMEID_KEY, "0");
                string userIdStr = PlayerPrefsEx.GetString("userId");
                long userId = 0;
                if (!string.IsNullOrEmpty(userIdStr))
                {
                    userId = long.Parse(userIdStr);                    
                }
                long roleId = 0;
                string roleName = "";

                LuaState lua = LuaInstance.instance.Get();
                lua.GetGlobal("Client");
                lua.GetField(-1, "RoleInfo");
                if (!lua.IsNil(-1))
                {
                    lua.GetField(-1, "roleAttr");
                    if (!lua.IsNil(-1))
                    {
                        lua.GetField(-1, "roleId");
                        roleId = lua.ReadLongId(-1);
                        lua.Pop(1);
                        lua.GetField(-1, "name");
                        roleName = lua.ToString(-1);
                        lua.Pop(1);
                    }
                    lua.Pop(1);
                }
                lua.Pop(1);
                lua.GetField(-1,"ClientVersion");
                string version = lua.ToString(-1);
                lua.Pop(1);
                int nowSceneId = -1;
                lua.GetField(-1, "CurrentCityId");
                if (!lua.IsNil(-1))
                {
                    nowSceneId = lua.ToInteger(-1);
                }
                lua.Pop(2);

                string request = string.Format("{0}?GameID={1}&PlayerID={2}&RoleId={3}&logString={4}&StackTrace={5}&LogType={6}&RoleName={7}&platform={8}&servernum={9}&gateway_id={10}&machine={11}&version={12}&GameName={13}&sid={14}", 
                   "http://cjhzmj-log.cherryjoy.com:8066/index.php", gameId, userId, roleId, System.Uri.EscapeDataString(logString), System.Uri.EscapeDataString(stackTrace), System.Uri.EscapeDataString(type.ToString()),
                   System.Uri.EscapeDataString(roleName), platform, PlayerPrefs.GetInt("lastServerNum", 0),
                   PlayerPrefs.GetInt("lastServerID", 0), SystemInfo.deviceModel, version, "arthur2",nowSceneId);
 
                StartCoroutine(SendException(request, md5str));
            }
        }
    }

    IEnumerator SendException(string url, string md5key)
    {
        WWW www = new WWW(url);
        yield return www;
        if (www.error == null)
        {
            SaveKeys(md5key);
            Debug.Log("save the error key! " +  System.Text.Encoding.UTF8.GetString(www.bytes, 0, www.bytes.Length));
        }
        else {
            Debug.Log("Send error end!" + www.error);
        }
    }
}
