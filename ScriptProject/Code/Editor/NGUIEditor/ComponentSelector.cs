//----------------------------------------------
//            NGUI: Next-Gen UI kit
// Copyright © 2011-2012 Tasharen Entertainment
//----------------------------------------------

using UnityEditor;
using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// EditorGUILayout.ObjectField doesn't support custom components, so a custom wizard saves the day.
/// Unfortunately this tool only shows components that are being used by the scene, so it's a "recently used" selection tool.
/// </summary>

public class ComponentSelector : ScriptableWizard
{
	public delegate void OnSelectionCallback (MonoBehaviour obj);

	System.Type mType;
    string mTitle;
	OnSelectionCallback mCallback;
	Object[] mObjects;
    bool mSearched = false;
    Vector2 mScroll = Vector2.zero;

    static string GetName(System.Type t)
    {
        string s = t.ToString();
        s = s.Replace("UnityEngine.", "");
        if (s.StartsWith("UI")) s = s.Substring(2);
        return s;
    }

	/// <summary>
	/// Draw a button + object selection combo filtering specified types.
	/// </summary>

	static public void Draw<T> (string buttonName, T obj, OnSelectionCallback cb, params GUILayoutOption[] options) where T : MonoBehaviour
	{
		GUILayout.BeginHorizontal();
		bool show = GUILayout.Button(buttonName, GUILayout.Width(76f));
		T o = EditorGUILayout.ObjectField(obj, typeof(T), false, options) as T;
		GUILayout.EndHorizontal();
		if (show) Show<T>(cb);
		else if (o != obj) cb(o);
	}

	/// <summary>
	/// Draw a button + object selection combo filtering specified types.
	/// </summary>

	static public void Draw<T> (T obj, OnSelectionCallback cb, params GUILayoutOption[] options) where T : MonoBehaviour
	{
		Draw<T>(NGUITools.GetName<T>(), obj, cb, options);
	}

	/// <summary>
	/// Show the selection wizard.
	/// </summary>

	static void Show<T> (OnSelectionCallback cb) where T : MonoBehaviour
	{
		System.Type type = typeof(T);
        string title = (type == typeof(UIAtlas) ? "Select an " : "Select a ") + GetName(type);
		ComponentSelector comp = ScriptableWizard.DisplayWizard<ComponentSelector>("Select " + type.ToString());
		comp.mType = type;
        comp.mTitle = title;
		comp.mCallback = cb;
		comp.mObjects = Resources.FindObjectsOfTypeAll(type) as MonoBehaviour[];

        if (comp.mObjects == null || comp.mObjects.Length == 0)
        {
            comp.Search();
        }
        else
        {
            System.Array.Sort(comp.mObjects,
                delegate(Object a, Object b) { return a.name.CompareTo(b.name); });
        }
	}

    /// <summary>
    /// Search the entire project for required assets.
    /// </summary>

    void Search()
    {
        mSearched = true;
        string[] paths = AssetDatabase.GetAllAssetPaths();
        bool isComponent = mType.IsSubclassOf(typeof(Component));
        BetterList<Object> list = new BetterList<Object>();

        for (int i = 0; i < paths.Length; ++i)
        {
            string path = paths[i];

            if (path.EndsWith(".prefab", System.StringComparison.OrdinalIgnoreCase))
            {
                EditorUtility.DisplayProgressBar("Loading", "Searching assets, please wait...", (float)i / paths.Length);
                Object obj = AssetDatabase.LoadMainAssetAtPath(path);
                if (obj == null) continue;

                if (!isComponent)
                {
                    System.Type t = obj.GetType();
                    if (t == mType || t.IsSubclassOf(mType))
                        list.Add(obj);
                }
                else if (PrefabUtility.GetPrefabType(obj) == PrefabType.Prefab)
                {
                    Object t = (obj as GameObject).GetComponent(mType);
                    if (t != null) list.Add(t);
                }
            }
        }
        list.Sort(delegate(Object a, Object b) { return a.name.CompareTo(b.name); });
        mObjects = list.ToArray();
        EditorUtility.ClearProgressBar();
    }

	/// <summary>
	/// Draw the custom wizard.
	/// </summary>

	void OnGUI ()
	{
        if (mObjects == null) return;
        NGUIEditorTools.SetLabelWidth(80f);
        GUILayout.Label(mTitle, "LODLevelNotifyText");
        GUILayout.Space(6f);

        if (mObjects.Length == 0)
        {
            GUILayout.Label("No recently used " + mType.ToString() + " components found.\nTry drag & dropping one instead.");
        }
        else
        {
            Object sel = null;
            mScroll = GUILayout.BeginScrollView(mScroll);

            foreach (Object o in mObjects)
                if (DrawObject(o))
                    sel = o;

            GUILayout.EndScrollView();

            if (sel != null)
            {
                mCallback(sel as MonoBehaviour);
                Close();
            }
        }

        if (!mSearched)
        {
            GUILayout.Space(6f);
            GUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            bool search = GUILayout.Button("Show All", "LargeButton", GUILayout.Width(120f));
            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();
            if (search) Search();
        }
	}

	/// <summary>
	/// Draw details about the specified monobehavior in column format.
	/// </summary>

	bool DrawObject (MonoBehaviour mb)
	{
		bool retVal = false;

		GUILayout.BeginHorizontal();
		{
			if (EditorUtility.IsPersistent(mb.gameObject))
			{
				GUILayout.Label("Prefab", GUILayout.Width(80f));
			}
			else
			{
				GUI.color = Color.grey;
				GUILayout.Label("Object", GUILayout.Width(80f));
			}

			GUILayout.Label(NGUITools.GetHierarchy(mb.gameObject));
			GUI.color = Color.white;
			retVal = GUILayout.Button("Select", GUILayout.Width(60f));
		}
		GUILayout.EndHorizontal();
		return retVal;
	}

    /// <summary>
    /// Draw details about the specified object in column format.
    /// </summary>

    bool DrawObject(Object obj)
    {
        bool retVal = false;
        Component comp = obj as Component;

        GUILayout.BeginHorizontal();
        {
            if (comp != null && EditorUtility.IsPersistent(comp.gameObject))
                GUI.contentColor = new Color(0.6f, 0.8f, 1f);

            retVal |= GUILayout.Button(obj.name, "AS TextArea", GUILayout.Width(120f), GUILayout.Height(20f));
            retVal |= GUILayout.Button(AssetDatabase.GetAssetPath(obj).Replace("Assets/", ""), "AS TextArea", GUILayout.Height(20f));
            GUI.contentColor = Color.white;

            retVal |= GUILayout.Button("Select", "ButtonLeft", GUILayout.Width(60f), GUILayout.Height(16f));
        }
        GUILayout.EndHorizontal();
        return retVal;
    }
}