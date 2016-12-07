#if UNITY_STANDALONE_WIN
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UniLua;
using System;


public class GMWindow : MonoBehaviour
{
    private bool m_bShowInput = false;
    private string result = "";
    private string inputText = "";
    private List<string> commandParams= new List<string>();
    private string command;
    protected LuaState lua_;
    private string lua_script = "ClientSendMsg";

    void CallLua()
    {
        lua_ = LuaInstance.instance.Get();
        lua_.GetGlobal(lua_script);
        lua_.GetField(-1, "SendGM");
        lua_.PushString(command);
        lua_.PushStringList(commandParams);
        lua_.PCall(2, 0, 0);
        lua_.Pop(1);
    }
    void Update()
    {
        if ((Input.GetKeyUp("`") || Input.GetKeyUp(KeyCode.F6)))
            m_bShowInput = !m_bShowInput;
    }

#if !DISONGUI
      void OnGUI()
    {
        if (m_bShowInput)
        {
            GUILayout.BeginArea(new Rect(10, 200, 200, 300), GUI.skin.box);
            ShowGM();
            GUILayout.EndArea();
        }
    }
#endif
    void ShowGM()
    {
        string[] commands;
        GUILayout.Label(result);
        GUILayout.Label("请输入命令参数");
        inputText = GUILayout.TextField(inputText);
        if (GUILayout.Button("确定"))
        {
            if (inputText == "")
            {
                result = "命令为空！";
            }
            else
            {
                commands = DoSplit(inputText);
                command = commands[0];
                if (commands.Length > 1)
                {
                    commandParams = new List<string>(commands);
                    commandParams.RemoveAt(0);
                }
                CallLua();
            }
        }
        if (GUILayout.Button("清除"))
        {
            inputText = "";
            result = "清除！";
        }
        if (GUILayout.Button("打印Lua当前使用内存"))
        {
            lua_ = LuaInstance.instance.Get();
            lua_.GetGlobal("gcinfo");
            lua_.PCall(0, 1, 0);
            double size = lua_.ToNumber(-1);

            Debug.Log("Lua Use Size " + size + " KB now.");
        }
    }
    string[] DoSplit(string str)
    {
        char c = ' ';
        if (!str.Contains(c.ToString()))
        {
            str = str + c.ToString();
        }
        string[] result = str.Split(c);
        return result;
    }
}
#endif