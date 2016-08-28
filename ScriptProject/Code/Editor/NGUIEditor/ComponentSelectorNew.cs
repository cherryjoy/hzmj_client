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

public class ComponentSelectorNew : ScriptableWizard
{
	public delegate void OnSelectionCallback (Object obj);

	System.Type mType;
	OnSelectionCallback mCallback;
	Object[] mObjects;

	/// <summary>
	/// Draw a button + object selection combo filtering specified types.
	/// </summary>

	static public void Draw<T> (string buttonName, T obj, OnSelectionCallback cb, params GUILayoutOption[] options) where T : Object
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

	static public void Draw<T> (T obj, OnSelectionCallback cb, params GUILayoutOption[] options) where T : Object
	{
		Draw<T>(NGUITools.GetName<T>(), obj, cb, options);
	}

	/// <summary>
	/// Show the selection wizard.
	/// </summary>

	static public  void Show<T> (OnSelectionCallback cb) where T : Object
	{
		System.Type type = typeof(T);
		ComponentSelectorNew comp = ScriptableWizard.DisplayWizard<ComponentSelectorNew>("Select " + type.ToString());
		comp.mType = type;
		comp.mCallback = cb;
		comp.mObjects = Resources.FindObjectsOfTypeAll(type) as Object[];
	}

	/// <summary>
	/// Draw the custom wizard.
	/// </summary>

	void OnGUI ()
	{
		NGUIEditorTools.SetLabelWidth(80f);

		if (mObjects.Length == 0)
		{
			GUILayout.Label("No recently used " + mType.ToString() + " components found.\nTry drag & dropping one instead.");
		}
		else
		{
			GUILayout.Label("List of recently used components:");
			NGUIEditorTools.DrawSeparator();

			Object sel = null;

			foreach (Object o in mObjects)
			{
				if (DrawObject(o))
				{
					sel = o;
				}
			}

			if (sel != null)
			{
				mCallback(sel);
				Close();
			}
		}
	}

	/// <summary>
	/// Draw details about the specified monobehavior in column format.
	/// </summary>

	bool DrawObject (Object ob)
	{
		bool retVal = false;
		Component comp = ob as Component;

		GUILayout.BeginHorizontal();
		{
			if (comp != null && EditorUtility.IsPersistent(comp.gameObject))
				GUI.contentColor = new Color(0.6f, 0.8f, 1f);

			GUILayout.Label(NGUITools.GetTypeName(ob), "AS TextArea", GUILayout.Width(80f), GUILayout.Height(20f));

			if (comp != null)
			{
				GUILayout.Label(NGUITools.GetHierarchy(comp.gameObject), "AS TextArea", GUILayout.Height(20f));
			}
			else if (ob is Font)
			{
				Font fnt = ob as Font;
				GUILayout.Label(fnt.name, "AS TextArea", GUILayout.Height(20f));
			}
			GUI.contentColor = Color.white;

			retVal = GUILayout.Button("Select", "ButtonLeft", GUILayout.Width(60f), GUILayout.Height(16f));
		}
		GUILayout.EndHorizontal();
		return retVal;
	}
}
