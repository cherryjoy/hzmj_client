using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEditor;
using UnityEditor.ProjectWindowCallback;
using System.Text.RegularExpressions;
using UnityEngine;

public class LuaScriptTemplateCreator:EndNameEditAction
{
   
    

    [MenuItem("Assets/Create/Lua Script",false,0)]
    public static void CreateNewLuaScript(){
        string templatePath = Application.dataPath + "/Editor Default Resources/LuaScriptTemplate.txt";
        ProjectWindowUtil.StartNameEditingIfProjectWindowExists(
            0,
            ScriptableObject.CreateInstance<LuaScriptTemplateCreator>(),
            RememberLastSelection.LastSelectPath + "/NewLuaScript.txt",
            null,
            templatePath
        );
    }

    public override void Action(int instanceId, string pathName, string resourceFile)
    {
        string fullPath = Path.GetFullPath(pathName);
        StreamReader reader = new StreamReader(resourceFile);
        string text = reader.ReadToEnd();
        reader.Close();
        string nameWithoutExtention = Path.GetFileNameWithoutExtension(pathName);
        text = Regex.Replace(text, "#NAME#", nameWithoutExtention);

        UTF8Encoding utf8 = new UTF8Encoding(false, false);
        StreamWriter writer = new StreamWriter(fullPath, false, utf8);
        writer.Write(text);
        writer.Close();
        AssetDatabase.ImportAsset(pathName);

        UnityEngine.Object obj = AssetDatabase.LoadAssetAtPath(pathName, typeof(UnityEngine.Object));
        ProjectWindowUtil.ShowCreatedAsset(obj);
    }

}

[InitializeOnLoad]
public static class RememberLastSelection { 
    static double timeOfLastUpdate;
    static public string LastSelectPath = "Assets";
    static RememberLastSelection(){
        EditorApplication.update += () =>
        {
            if (EditorApplication.timeSinceStartup - timeOfLastUpdate > 0.1) {
                timeOfLastUpdate = EditorApplication.timeSinceStartup;
                foreach (UnityEngine.Object obj in Selection.GetFiltered(typeof(UnityEngine.Object), SelectionMode.Assets))
                {
                    string path = AssetDatabase.GetAssetPath(obj);
                    if (!string.IsNullOrEmpty(path) )
                    {
                        if (path.Contains("."))
                        {
                            LastSelectPath = Path.GetDirectoryName(path);
                        }
                        else {
                            LastSelectPath = path;
                        }
                        
                        break;
                    }
                }
            }
        };
    }
}

