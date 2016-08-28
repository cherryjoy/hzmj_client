using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UniLua;
using System.IO;

[ExecuteInEditMode]
[RequireComponent(typeof(UILabel))]
public class LocalizationLabelText : MonoBehaviour
{
    public string mTextKeyID;
    public string TextKeyID
    {
        get
        {
            return mTextKeyID;
        }
        set
        {
            if (value != mTextKeyID)
            {
                mTextKeyID = value;
                ChangedCurrentText();
            }
        }
    }
    public UILabel textLabel = null;
    private LuaState lua;
    void Start()
    {
        lua = LuaInstance.instance.Get();

        if (textLabel == null)
        {
            textLabel = GetComponent<UILabel>();
            if (textLabel == null)
            {
                Debug.LogError("Need UILabel");
            }
        }

        ChangedCurrentText();
    }


    private void ChangedCurrentText()
    {
        if (textLabel != null)
        {
            string cacheLblText = textLabel.text;

            string text = null;
            if (!string.IsNullOrEmpty(mTextKeyID)) {
                lua.RawGetI(LuaAPI.LUA_REGISTRYINDEX,LuaInstance.instance.text_table_ref);
                #if UNITY_EDITOR
                if (lua.IsNil(-1)) {
                    Debug.Log("Run game first once.");
                    return;
                }
                #endif
                lua.GetField(-1, mTextKeyID);
#if UNITY_EDITOR
                if (lua.IsNil(-1))
                {
                    Debug.LogError("Text not Found!");
                    return;
                }
#endif
                text = lua.ToString(-1);
                lua.Pop(2);
            }

            if (!string.IsNullOrEmpty(text))
            {
                textLabel.text = text;
            }
            else
            {
                textLabel.text = cacheLblText;
            }
        }
    }


#if UNITY_EDITOR
    // Update is called once per frame
    void Update()
    {
        textLabel.text = GetTextFromLocalFile(mTextKeyID);
    }

    string GetTextFromLocalFile(string key) {
        string text = "Loading...";

        if (!string.IsNullOrEmpty(key)) {
            LuaState lua = LuaInstance.instance.Get();
            LuaInstance.instance.DoFile("Utils/Text.txt");
            lua.GetGlobal("Text");
            lua.GetField(-1, key);
            if (!lua.IsNil(-1))
            {
                text = lua.ToString(-1);
            }
            else
            {
                text = "Text not Found!";
            }

            //string s = LuaInstance.ConstructString(lua);
            lua.Pop(2);
        }
       
        return text;
    }


#endif
}