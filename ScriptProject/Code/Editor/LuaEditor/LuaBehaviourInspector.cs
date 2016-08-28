using System;
using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

[CustomEditor(typeof(LuaBehaviour))]
class LuaBehaviourInspector : Editor
{
    LuaBehaviour mLuaBehav;
    int parDelIndex = -1;
    int behavDelIndex = -1;

    public override void OnInspectorGUI()
    {
        NGUIEditorTools.SetLabelWidth(80f);
        mLuaBehav = target as LuaBehaviour;

        if (mLuaBehav.lua_params == null)
            mLuaBehav.lua_params = new LuaGameObject[0];
        if (mLuaBehav.LuaBehavArray == null)
            mLuaBehav.LuaBehavArray = new LuaBehav[0];
        UnityEngine.Object luaScript = null;
        if (mLuaBehav.scriptShortPath != null) {
            luaScript = AssetDatabase.LoadAssetAtPath("Assets/StreamingAssets/Script/" + mLuaBehav.scriptShortPath, typeof(UnityEngine.Object));
            if(luaScript!=null)
            mLuaBehav.ScriptName = luaScript.name;
        }
        UnityEngine.Object ta = EditorGUILayout.ObjectField("Lua Script", luaScript, typeof(UnityEngine.Object), false);
        
        if (ta != luaScript)
        {
            string path = AssetDatabase.GetAssetPath(ta);
            string[] strs = path.Split(new string[]{"StreamingAssets/Script/"},StringSplitOptions.None);
            mLuaBehav.scriptShortPath = strs[strs.Length-1];

            luaScript = AssetDatabase.LoadAssetAtPath("Assets/StreamingAssets/Script/" + mLuaBehav.scriptShortPath, typeof(UnityEngine.Object));
            if (luaScript != null)
            {
                mLuaBehav.ScriptName = luaScript.name;
            }
            else
            {
                mLuaBehav.ScriptName = "";
            }
            
        }
        if (ta != null) {
            EditorGUILayout.LabelField("Script Path: " + mLuaBehav.scriptShortPath);
        }

        mLuaBehav.LuaBehavType = (LuaBehaviourType)EditorGUILayout.EnumPopup("BehavType", mLuaBehav.LuaBehavType);

        GUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("IsOnClickPassGameObject");
        mLuaBehav.IsOnClickPassGameObject = EditorGUILayout.Toggle("", mLuaBehav.IsOnClickPassGameObject);
        GUILayout.EndHorizontal();
        if (GUILayout.Button("Copy Names"))
        {
            string allNames = "";
            foreach (LuaGameObject luaG in mLuaBehav.lua_params)
            {
                if (luaG.Obj.Equals(null))
                    continue;
                if (string.IsNullOrEmpty(luaG.Name))
                {
                    allNames += "self."+luaG.Obj.name;
                }
                else
                {
                    allNames += "self."+luaG.Name;
                }
                if (luaG.regObj.GetType() == typeof(UILabel))
                {
                    allNames += ":set_text()"+Environment.NewLine;;
                }
                else {
                    allNames += Environment.NewLine;
                }
            }

            foreach (LuaBehav luaB in mLuaBehav.LuaBehavArray)
            {
                if (luaB == null)
                    continue;
                if (string.IsNullOrEmpty(luaB.Name))
                {
                    allNames += "self."+luaB.Behav.name + Environment.NewLine;
                }
                else
                {
                    allNames += "self."+luaB.Name + Environment.NewLine;
                }
            }

            EditorGUIUtility.systemCopyBuffer = allNames;
            Debug.Log("All object's name in LuaBehaviour " + mLuaBehav.name + " is Copy to clipboard now.");
        }

        if (parDelIndex >= 0) {
            mLuaBehav.lua_params = ArrayRemoveByIndex(mLuaBehav.lua_params, parDelIndex);
            parDelIndex = -1;
        }
        if (behavDelIndex >= 0)
        {
            mLuaBehav.LuaBehavArray = ArrayRemoveByIndex(mLuaBehav.LuaBehavArray, behavDelIndex);
            behavDelIndex = -1;
        }

        NGUIEditorTools.DrawSeparator();

        int luaParNum = 0;
        int luaBehNum = 0;
        if (mLuaBehav.lua_params != null)
            luaParNum = mLuaBehav.lua_params.Length;
        if (mLuaBehav.LuaBehavArray != null)
            luaBehNum = mLuaBehav.LuaBehavArray.Length;

        EditorGUILayout.LabelField("Components:"+luaParNum);
        GUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("IsArray");
        mLuaBehav.isArray = EditorGUILayout.Toggle("", mLuaBehav.isArray);
        GUILayout.EndHorizontal();
        mLuaBehav.lua_params = DrawLuaGameObjList(mLuaBehav.lua_params);

        NGUIEditorTools.DrawSeparator();

        EditorGUILayout.LabelField("LuaBehaviors:"+luaBehNum);
        mLuaBehav.LuaBehavArray = DrawLuaBehavList(mLuaBehav.LuaBehavArray);


        if (GUI.changed) {
            EditorUtility.SetDirty(target);

        }
    }

    LuaGameObject[] DrawLuaGameObjList(LuaGameObject[] objList)
    {
        GUILayout.BeginHorizontal();
        if (GUILayout.Button("Add Obj"))
        {
            objList = AddOne2Array(objList);
        }
        if (GUILayout.Button("Delete Obj"))
        {
            if (objList.Length > 0)
            {
                objList = RemoveOneFromArray(objList);
            }
        }
        GUILayout.EndHorizontal();


        List<string> names = new List<string>();
        for (int i = 0; i < objList.Length; i++)
        {

            GUILayout.BeginHorizontal();
            GUI.backgroundColor = Color.blue;
            bool IsObjAssignFirst = false;
            GameObject objSelect = EditorGUILayout.ObjectField("Obj", objList[i].Obj, typeof(GameObject), true) as GameObject;
            if (objSelect != null) {
                if (objList[i].Obj == null) {
                    IsObjAssignFirst = true;
                }
                objList[i].Obj = objSelect;
            }
            GUI.backgroundColor = Color.red;
            if (GUILayout.Button("X", GUILayout.Width(22f)))
            {
                parDelIndex = i;
            }
            GUI.backgroundColor = Color.white;
            GUILayout.EndHorizontal();

            if (objList[i].Obj != null)
            {
                if (!mLuaBehav.isArray)
                {
                    objList[i].Name = EditorGUILayout.TextField("Name", objList[i].Name);
                }

                Component[] comps = objList[i].Obj.GetComponents<Component>();
                names.Clear();

                for (int j = 0; j < comps.Length; j++)
                {
                    names.Add(comps[j].GetType().Name);
                }
                names.Add("GameObject");

                if (IsObjAssignFirst)
                {
                    objList[i].TypeIndex = names.Count - 1;
                }
                  
                if(objList[i].TypeIndex != names.Count - 1 && objList[i].regObj.GetType() == typeof(GameObject)){
                    objList[i].TypeIndex = names.Count - 1;
                }

                objList[i].TypeIndex = EditorGUILayout.Popup("Type", objList[i].TypeIndex, names.ToArray());

                if (objList[i].TypeIndex < names.Count - 1)
                {
                    objList[i].regObj = comps[objList[i].TypeIndex];
                }
                else
                {
                    objList[i].regObj = objList[i].Obj;
                }
            }
        }

        return objList;
    }

    LuaBehav[] DrawLuaBehavList(LuaBehav[] objList)
    {
        GUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("IsBehavArray");
        mLuaBehav.isBehavArray = EditorGUILayout.Toggle("", mLuaBehav.isBehavArray);
        GUILayout.EndHorizontal();

        GUILayout.BeginHorizontal();
        if (GUILayout.Button("Add Obj"))
        {
            objList = AddOne2Array(objList);
        }
        if (GUILayout.Button("Delete Obj"))
        {
            if (objList.Length > 0)
            {
                objList = RemoveOneFromArray(objList);
            }
        }
        GUILayout.EndHorizontal();


        List<string> names = new List<string>();
        for (int i = 0; i < objList.Length; i++)
        {

            GUILayout.BeginHorizontal();
            GUI.backgroundColor = Color.cyan;
            objList[i].Behav = EditorGUILayout.ObjectField("Obj", objList[i].Behav, typeof(LuaBehaviour), true) as LuaBehaviour;
            GUI.backgroundColor = Color.red;
            if (GUILayout.Button("X", GUILayout.Width(22f)))
            {
                behavDelIndex = i;
            }
            GUI.backgroundColor = Color.white;
            GUILayout.EndHorizontal();

            if (objList[i].Behav != null && !mLuaBehav.isBehavArray)
            {
                objList[i].Name = EditorGUILayout.TextField("Name", objList[i].Name);
            }
        }

        return objList;
    }

    LuaGameObject[] AddOne2Array(LuaGameObject[] array)
    {
        LuaGameObject[] temp = array;
        array = new LuaGameObject[temp.Length+1];
        for (int i = 0; i < temp.Length; i++) {
            array[i] = temp[i];
        }
        array[array.Length-1] = new LuaGameObject();
        return array;
    }

    LuaGameObject[] RemoveOneFromArray(LuaGameObject[] array)
    {
        LuaGameObject[] temp = array;
        array = new LuaGameObject[temp.Length - 1];
        for (int i = 0; i < temp.Length - 1; i++)
        {
            array[i] = temp[i];
        }

        return array;
    }

    LuaGameObject[] ArrayRemoveByIndex(LuaGameObject[] array, int index) {
        LuaGameObject[] temp = new LuaGameObject[array.Length - 1];
        int j = 0;
        for (int i = 0; i < array.Length; i++) {
            if (i != index) {
                temp[j] = array[i];
                j++;
            }
        }

        return temp;
    }

    LuaBehav[] AddOne2Array(LuaBehav[] array)
    {
        LuaBehav[] temp = array;
        array = new LuaBehav[temp.Length + 1];
        for (int i = 0; i < temp.Length; i++)
        {
            array[i] = temp[i];
        }
        array[array.Length - 1] = new LuaBehav();
        return array;
    }

    LuaBehav[] RemoveOneFromArray(LuaBehav[] array)
    {
        LuaBehav[] temp = array;
        array = new LuaBehav[temp.Length - 1];
        for (int i = 0; i < temp.Length - 1; i++)
        {
            array[i] = temp[i];
        }

        return array;
    }

    LuaBehav[] ArrayRemoveByIndex(LuaBehav[] array, int index)
    {
        LuaBehav[] temp = new LuaBehav[array.Length - 1];
        int j = 0;
        for (int i = 0; i < array.Length; i++)
        {
            if (i != index)
            {
                temp[j] = array[i];
                j++;
            }
        }

        return temp;
    }
}
