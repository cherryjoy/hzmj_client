#if UNITY_STANDALONE_WIN
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UniLua;
using UnityEngine.SceneManagement;

//public class AccountManager : EditorWindow
public class AccountManager : MonoBehaviour
{
    //[MenuItem("LK-TOOL/AccountManager")]
    //static void Init()
    //{
    //    EditorWindow win = EditorWindow.GetWindow(typeof(AccountManager), false, "AccountManager");
    //    win.minSize = new Vector2(700f, Screen.height);
    //    ((AccountManager)win).LoadAccount();
    //}
    void Start()
    {
        LoadAccount();
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.F9) == true)
        {
            isShowWindow = !isShowWindow;
        }
    }

    Dictionary<string, string> m_Dict = new Dictionary<string, string>();
    private Vector2 mScrollVec = Vector2.zero;
    private bool isShowPwd = false;
    private bool isShowWindow = false;
    void LoadAccount()
    {
        string localAccount = PlayerPrefs.GetString("LocalAccount", "");
        if (string.IsNullOrEmpty(localAccount) == false)
        {
            string[] val = localAccount.Split(';');
            for (int i = 0; i < val.Length; i++)
            {
                string[] val1 = val[i].Split(',');
                if (m_Dict.ContainsKey(val1[0]) == false)
                {
                    m_Dict.Add(val1[0], val1[1]);
                }
            }
        }
    }

    void SaveAccount(string username = null, string pwd = null)
    {
        if (string.IsNullOrEmpty(username) == false && m_Dict.ContainsKey(username) == false)
        {
            m_Dict.Add(username, pwd);
        }
        string localAccount = null;
        foreach (string key in m_Dict.Keys)
        {
            if (string.IsNullOrEmpty(localAccount))
            {
                localAccount = key + "," + m_Dict[key];
            }
            else
            {
                localAccount += ";" + key + "," + m_Dict[key];
            }
        }
        PlayerPrefs.SetString("LocalAccount", localAccount);
    }

    #if !DISONGUI
    void OnGUI()
    {
        if (isShowWindow == false)
            return;
        GUILayout.BeginVertical(GUI.skin.box);
        GUILayout.Space(10);

        GUILayout.BeginHorizontal();
        isShowPwd = GUILayout.Toggle(isShowPwd, "Show Password");
        if (GUILayout.Button("Back To Login Scene"))
        {
           // Client.Instance.QuitGame();
            //Application.LoadLevel("Login");
            SceneManager.LoadScene("Login");
        }
        GUILayout.EndHorizontal();

        GUILayout.BeginHorizontal();
        GUILayout.Label("Current Account: ", GUILayout.Width(100));
        GUILayout.TextField(PlayerPrefs.GetString("userName", ""));
        //if (Client.ClientRole != null)
            //GUILayout.TextField(Client.ClientRole.name);
        GUILayout.EndHorizontal();
        GUILayout.BeginHorizontal();
        if (GUILayout.Button("Load Local Users"))
        {
            LoadAccount();
        }
        GUILayout.Space(5);
        if (GUILayout.Button("Save Current User"))
        {
            SaveAccount(PlayerPrefs.GetString("userName", ""), PlayerPrefs.GetString("password", ""));
            LoadAccount();
        }
        GUILayout.EndHorizontal();
        GUILayout.Space(10);
        mScrollVec = GUILayout.BeginScrollView(mScrollVec);
        foreach (string key in m_Dict.Keys)
        {

            GUILayout.BeginHorizontal();
            GUILayout.TextField(key, GUILayout.Width(120));
            GUILayout.Space(5);
            if (isShowPwd)
                GUILayout.Label(m_Dict[key]);
            else
                GUILayout.Label("******");
            if (GUILayout.Button("Switch Account"))
            {
                PlayerPrefs.SetString("userName", key);
                PlayerPrefs.SetString("password", m_Dict[key]);
               // Client.Instance.QuitGame();
                //Application.LoadLevel("Login");
                SceneManager.LoadScene("Update");
            }
            if (GUILayout.Button("Delete Account"))
            {
                m_Dict.Remove(key);
                SaveAccount();
                break;
            }
            GUILayout.EndHorizontal();
            GUILayout.Space(2);
        }
        GUILayout.EndScrollView();
        GUILayout.EndVertical();
    }
#endif
}
#endif