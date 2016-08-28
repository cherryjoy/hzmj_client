using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(PageDataInspector))]
class PageDataInspector : Editor
{
    PageData m_pageData;

    //public override void OnInspectorGUI() 
    //{
    //    TextAsset ta = EditorGUILayout.ObjectField("lua script", m_pageData.lua_script, typeof(TextAsset),false) as TextAsset;

    //    if (ta != m_pageData.lua_script)
    //    {
    //        m_pageData.lua_script = ta;
    //        string path = AssetDatabase.GetAssetPath(ta);
    //        string[] strs = path.Split(new string[] { "StreamingAssets" }, StringSplitOptions.None);
    //        m_pageData.scriptShortPath = strs[strs.Length - 1];
    //    }
    //    if (ta != null)
    //    {
    //        EditorGUILayout.LabelField("Script Path: " + m_pageData.scriptShortPath);
    //    }
    //}
}

