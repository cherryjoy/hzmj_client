using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using System.IO;
using System;

class LuaDoFileImmediately:AssetPostprocessor
{
    //static string bassPath = Application.dataPath.Substring(0, Application.dataPath.Length - "assets".Length);
    public static void OnPostprocessAllAssets(
        string[] importedAssets,
        string[] deletedAssets,
        string[] movedAssets,
        string[] movedFromAssetPaths)
    {
        foreach (string s in importedAssets)
        {
            try
            {
                if (Application.isPlaying) {
                    if (s.EndsWith(".txt") && s.Contains("StreamingAssets"))
                    {
                        //TextAsset luaFile = (TextAsset)AssetDatabase.LoadAssetAtPath(s,typeof(TextAsset));
                        Debug.Log("DoFile: "+s);
                        LuaInstance.instance.DoFile(s.Substring("Assets/StreamingAssets/Script/".Length));
                    }
                
                }
                
            }
            catch(Exception e)
            {
                Debug.LogError(e.Message);
            }
        }
      
   }
}

[InitializeOnLoad]
public class WhenEditorStartup
{
	const string S2_UNITY_VERSION_NEW = "5.3.4f1";
    const string S2_UNITY_VERSION_NEW_Pack = "5.3.4p5";
    const string S2_UNITY_VERSION_NEW_Pack2 = "5.3.5p2";
    const float Unity_Version_Lowest = 5.3f;
	static WhenEditorStartup()
	{
		//check unity version
        string sversion = Application.unityVersion.Substring(0, 3);
        float nVersion = float.Parse(sversion);

        if (nVersion < Unity_Version_Lowest)
		{
			bool buttonClicked = EditorUtility.DisplayDialog(@"骚年,你的Unity太Low了", 
                string.Format(@"别人的是{0}, 你的是{1}", S2_UNITY_VERSION_NEW, Application.unityVersion), 
                "去Ftp更新", "打死也不更新");
			if( buttonClicked )
			{
				EditorApplication.Exit(0);
			}
		}
	}

}