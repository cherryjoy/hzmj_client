//----------------------------------------------
//            NGUI: Next-Gen UI kit
// Copyright © 2011-2014 Tasharen Entertainment
//----------------------------------------------

using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

/// <summary>
/// Unity doesn't keep the values of static variables after scripts change get recompiled. One way around this
/// is to store the references in EditorPrefs -- retrieve them at start, and save them whenever something changes.
/// </summary>

public class NGUISettings
{
	public enum ColorMode
	{
		Orange,
		Green,
		Blue,
	}

#region Generic Get and Set methods
	/// <summary>
	/// Save the specified boolean value in settings.
	/// </summary>

	static public void SetBool (string name, bool val) { EditorPrefs.SetBool(name, val); }

	/// <summary>
	/// Save the specified integer value in settings.
	/// </summary>

	static public void SetInt (string name, int val) { EditorPrefs.SetInt(name, val); }

	/// <summary>
	/// Save the specified float value in settings.
	/// </summary>

	static public void SetFloat (string name, float val) { EditorPrefs.SetFloat(name, val); }

	/// <summary>
	/// Save the specified string value in settings.
	/// </summary>

	static public void SetString (string name, string val) { EditorPrefs.SetString(name, val); }

	/// <summary>
	/// Save the specified color value in settings.
	/// </summary>

	static public void SetColor (string name, Color c) { SetString(name, c.r + " " + c.g + " " + c.b + " " + c.a); }

	/// <summary>
	/// Save the specified enum value to settings.
	/// </summary>

	static public void SetEnum (string name, System.Enum val) { SetString(name, val.ToString()); }

	/// <summary>
	/// Save the specified object in settings.
	/// </summary>

	static public void Set (string name, Object obj)
	{
		if (obj == null)
		{
			EditorPrefs.DeleteKey(name);
		}
		else
		{
			if (obj != null)
			{
				string path = AssetDatabase.GetAssetPath(obj);

				if (!string.IsNullOrEmpty(path))
				{
					EditorPrefs.SetString(name, path);
				}
				else
				{
					EditorPrefs.SetString(name, obj.GetInstanceID().ToString());
				}
			}
			else EditorPrefs.DeleteKey(name);
		}
	}

	/// <summary>
	/// Get the previously saved boolean value.
	/// </summary>

	static public bool GetBool (string name, bool defaultValue) { return EditorPrefs.GetBool(name, defaultValue); }

	/// <summary>
	/// Get the previously saved integer value.
	/// </summary>

	static public int GetInt (string name, int defaultValue) { return EditorPrefs.GetInt(name, defaultValue); }

	/// <summary>
	/// Get the previously saved float value.
	/// </summary>

	static public float GetFloat (string name, float defaultValue) { return EditorPrefs.GetFloat(name, defaultValue); }

	/// <summary>
	/// Get the previously saved string value.
	/// </summary>

	static public string GetString (string name, string defaultValue) { return EditorPrefs.GetString(name, defaultValue); }
	
	/// <summary>
	/// Get a previously saved color value.
	/// </summary>

	static public Color GetColor (string name, Color c)
	{
		string strVal = GetString(name, c.r + " " + c.g + " " + c.b + " " + c.a);
		string[] parts = strVal.Split(' ');

		if (parts.Length == 4)
		{
			float.TryParse(parts[0], out c.r);
			float.TryParse(parts[1], out c.g);
			float.TryParse(parts[2], out c.b);
			float.TryParse(parts[3], out c.a);
		}
		return c;
	}

	/// <summary>
	/// Get a previously saved enum from settings.
	/// </summary>

	static public T GetEnum<T> (string name, T defaultValue)
	{
		string val = GetString(name, defaultValue.ToString());
		string[] names = System.Enum.GetNames(typeof(T));
		System.Array values = System.Enum.GetValues(typeof(T));
		
		for (int i = 0; i < names.Length; ++i)
		{
			if (names[i] == val)
				return (T)values.GetValue(i);
		}
		return defaultValue;
	}

	/// <summary>
	/// Get a previously saved object from settings.
	/// </summary>

	static public T Get<T> (string name, T defaultValue) where T : Object
	{
		string path = EditorPrefs.GetString(name);
		if (string.IsNullOrEmpty(path)) return null;
		
		T retVal = NGUIEditorTools.LoadAsset<T>(path);
		
		if (retVal == null)
		{
			int id;
			if (int.TryParse(path, out id))
				return EditorUtility.InstanceIDToObject(id) as T;
		}
		return retVal;
	}
#endregion

#region Convenience accessor properties

	static public Color color
	{
		get { return GetColor("NGUI Color", Color.white); }
		set { SetColor("NGUI Color", value); }
	}

	static public ColorMode colorMode
	{
		get { return GetEnum("NGUI Color Mode", ColorMode.Blue); }
		set { SetEnum("NGUI Color Mode", value); }
	}

	static public UIAtlas atlas
	{
		get { return Get<UIAtlas>("NGUI Atlas", null); }
		set { Set("NGUI Atlas", value); }
	}

	static public Texture texture
	{
		get { return Get<Texture>("NGUI Texture", null); }
		set { Set("NGUI Texture", value); }
	}

#if !UNITY_3_5 && !UNITY_4_0 && !UNITY_4_1 && !UNITY_4_2
	static public Sprite sprite2D
	{
		get { return Get<Sprite>("NGUI Sprite2D", null); }
		set { Set("NGUI Sprite2D", value); }
	}
#endif

	static public string selectedSprite
	{
		get { return GetString("NGUI Sprite", null); }
		set { SetString("NGUI Sprite", value); }
	}

	static public UIWidget.Pivot pivot
	{
		get { return GetEnum("NGUI Pivot", UIWidget.Pivot.Center); }
		set { SetEnum("NGUI Pivot", value); }
	}

	static public int layer
	{
		get
		{
			int layer = GetInt("NGUI Layer", -1);
			if (layer == -1) layer = LayerMask.NameToLayer("UI");
			if (layer == -1) layer = LayerMask.NameToLayer("2D UI");
			return (layer == -1) ? 9 : layer;
		}
		set
		{
			SetInt("NGUI Layer", value);
		}
	}

	static public TextAsset fontData
	{
		get { return Get<TextAsset>("NGUI Font Data", null); }
		set { Set("NGUI Font Data", value); }
	}

	static public Texture2D fontTexture
	{
		get { return Get<Texture2D>("NGUI Font Texture", null); }
		set { Set("NGUI Font Texture", value); }
	}

	static public int fontSize
	{
		get { return GetInt("NGUI Font Size", 16); }
		set { SetInt("NGUI Font Size", value); }
	}

	static public FontStyle fontStyle
	{
		get { return GetEnum("NGUI Font Style", FontStyle.Normal); }
		set { SetEnum("NGUI Font Style", value); }
	}

	static public string partialSprite
	{
		get { return GetString("NGUI Partial", null); }
		set { SetString("NGUI Partial", value); }
	}

	static public int atlasPadding
	{
		get { return GetInt("NGUI Padding", 1); }
		set { SetInt("NGUI Padding", value); }
	}

	static public bool atlasTrimming
	{
		get { return GetBool("NGUI Trim", true); }
		set { SetBool("NGUI Trim", value); }
	}

	static public bool atlasPMA
	{
		get { return GetBool("NGUI PMA", false); }
		set { SetBool("NGUI PMA", value); }
	}

	static public bool unityPacking
	{
		get { return GetBool("NGUI Packing", true); }
		set { SetBool("NGUI Packing", value); }
	}

	static public bool forceSquareAtlas
	{
		get { return GetBool("NGUI Square", false); }
		set { SetBool("NGUI Square", value); }
	}

	static public bool allow4096
	{
		get { return GetBool("NGUI 4096", true); }
		set { SetBool("NGUI 4096", value); }
	}

	static public bool showAllDCs
	{
		get { return GetBool("NGUI DCs", true); }
		set { SetBool("NGUI DCs", value); }
	}

	static public bool drawGuides
	{
		get { return GetBool("NGUI Guides", false); }
		set { SetBool("NGUI Guides", value); }
	}
#endregion
}
