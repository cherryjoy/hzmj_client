using UnityEngine;
using System.Collections;
using System.IO;
using System.Threading;
using System;

public class DownLoadScript : MonoBehaviour {
    private string ftpPath = "192.168.49.124/DB";
    private string userName = "kingnet";
    private string passworld = "king123";
    private string sceneName = "scene001";
	// Use this for initialization
	void Start () {
        readConfig();
	}
	
	// Update is called once per frame
	void Update () {
	}

    void OnGUI()
    {
        GUI.skin.label.fontSize = 25;
        GUI.skin.textField.fontSize = 25;
        GUI.skin.button.fontSize = 25;
        if (ResourceLoader.downLoadState == 2)
        {
            if (ResourceLoader.downloadError == "")
            {

                GUILayout.BeginArea(new Rect(Screen.width * 0.5f - 250, Screen.height * 0.5f - 60, 500, 120));
                if (GUILayout.Button("下载完成，点我退出", GUILayout.Height(50), GUILayout.Width(500))) 
                {
                    Application.Quit();
                }
                GUILayout.EndArea();
                return;
            }
        }
        if (ResourceLoader.downLoadState == 1)
        {
            GUILayout.BeginArea(new Rect(Screen.width * 0.5f - 100, Screen.height * 0.5f - 30, 200, 60));
            GUI.color = Color.green;
            GUILayout.Label("下载中...");
            GUI.color = Color.white;
            GUILayout.EndArea();
            return;
        }
        GUILayout.BeginArea(new Rect(Screen.width * 0.5f - 300, Screen.height*0.5f - 240, 600,480));

        GUILayout.BeginVertical();
        {
            GUI.color = Color.red;
            GUILayout.Label(ResourceLoader.downloadError);
            GUI.color = Color.white;
            GUILayout.BeginHorizontal();
            GUILayout.Space(30);
            GUILayout.Label("FtpPath:");
            ftpPath = GUILayout.TextField(ftpPath, 200,GUILayout.Width(460));
            GUILayout.EndHorizontal();

            GUILayout.Space(50);

            GUILayout.BeginHorizontal();
            GUILayout.Label("UserName:");
            userName = GUILayout.TextField(userName, 100, GUILayout.Width(140));

            GUILayout.Space(40);
            GUILayout.Label("PassWord:");
            passworld = GUILayout.PasswordField(passworld,'*', 100, GUILayout.Width(140));
            GUILayout.EndHorizontal();

            GUILayout.Space(20);
            GUILayout.BeginHorizontal();
            GUILayout.Space(400);
            if (GUILayout.Button("DownLoad", GUILayout.Width(200), GUILayout.Height(60)))
            {
                writeConfig();
                ResourceLoader.ftpServerIP = ftpPath;
                ResourceLoader.ftpUserID = userName;
                ResourceLoader.ftpPassword = passworld;
               // ResourceLoader.DownLoadData();
                Thread downLoadThread = new Thread(ResourceLoader.DownLoadData);
                downLoadThread.Start();
            }
            GUILayout.EndHorizontal();
            GUILayout.Space(50);

            GUILayout.BeginHorizontal();
            GUILayout.Space(50);
            GUILayout.Label("Scene:");
            sceneName = GUILayout.TextField(sceneName, 200, GUILayout.Width(460));
            GUILayout.EndHorizontal();

            GUILayout.Space(10);
            GUILayout.BeginHorizontal();
            GUILayout.Space(400);
            if (GUILayout.Button("Show", GUILayout.Width(200), GUILayout.Height(60)))
            {
                ResourceLoader.SceneName = sceneName;
                writeConfig();
                Application.LoadLevel("sceneViewMobile");
            }
            GUILayout.EndHorizontal();
        } GUILayout.EndVertical();

        GUILayout.EndArea();
    }
    private void readConfig()
    {
        string filePath =  "/mnt/sdcard/lk/DB" + "/Reconfig";
        Debug.Log("yyss:read config:" + filePath);
        try
        {
            FileInfo fileInfo = new FileInfo(filePath);
            FileStream fs = fileInfo.Open(FileMode.OpenOrCreate, FileAccess.Read, FileShare.ReadWrite);

            int buffLength = 128;
            byte[] buff = new byte[buffLength];
            fs.Read(buff, 0, 128);

            string str = System.Text.Encoding.Default.GetString(buff);
            Debug.Log("yyss:read:" + str);
            string[] strList = str.Split('#');
            if (strList.Length == 5)
            {
                ftpPath = strList[0];
                userName = strList[1];
                passworld = strList[2];
                sceneName = strList[3];
            }
            fs.Close();
        }
        catch (Exception ex)
        {
            Debug.Log("yyss:"+ ex.Data);
        }
        Debug.Log("yyss:read config down");
    }
    private void writeConfig()
    {
        string filePath =  "/mnt/sdcard/lk/DB" + "/Reconfig";
        Debug.Log("yyss:write config:" + filePath);

        try
        {
            FileInfo fileInfo = new FileInfo(filePath);

            FileStream fs = fileInfo.Open(FileMode.Create, FileAccess.Write, FileShare.ReadWrite);
            string str = ftpPath + "#" + userName + "#" + passworld + "#" + sceneName + "#";
            Debug.Log("yyss:write:" + str);
            byte[] buff = System.Text.Encoding.Default.GetBytes(str);
            fs.Write(buff, 0, buff.Length);
            fs.Close();
        }
        catch (Exception ex)
        {
            Debug.Log("yyss:" + ex.Data);
        }
        Debug.Log("yyss:write config down");
    }
}
