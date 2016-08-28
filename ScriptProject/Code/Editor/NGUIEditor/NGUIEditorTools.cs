//----------------------------------------------
//            NGUI: Next-Gen UI kit
// Copyright © 2011-2012 Tasharen Entertainment
//----------------------------------------------

using UnityEditor;
using UnityEngine;
using System.Collections.Generic;
using System.IO;

/// <summary>
/// Tools for the editor
/// </summary>

public class NGUIEditorTools
{
	static Texture2D mWhiteTex;
	static Texture2D mBackdropTex;
	static Texture2D mContrastTex;
	static Texture2D mGradientTex;

    static string mEditedName = null;
    static string mLastSprite = null;
	/// <summary>
	/// Returns a blank usable 1x1 white texture.
	/// </summary>

	static public Texture2D blankTexture
	{
		get
		{
			if (mWhiteTex == null) mWhiteTex = CreateDummyTex();
			return mWhiteTex;
		}
	}

	/// <summary>
	/// Returns a usable texture that looks like a dark checker board.
	/// </summary>

	static public Texture2D backdropTexture
	{
		get
		{
			if (mBackdropTex == null) mBackdropTex = CreateCheckerTex(
				new Color(0.1f, 0.1f, 0.1f, 0.5f),
				new Color(0.2f, 0.2f, 0.2f, 0.5f));
			return mBackdropTex;
		}
	}

	/// <summary>
	/// Returns a usable texture that looks like a high-contrast checker board.
	/// </summary>

	static public Texture2D contrastTexture
	{
		get
		{
			if (mContrastTex == null) mContrastTex = CreateCheckerTex(
				new Color(0f, 0.0f, 0f, 0.5f),
				new Color(1f, 1f, 1f, 0.5f));
			return mContrastTex;
		}
	}

	/// <summary>
	/// Gradient texture is used for title bars / headers.
	/// </summary>

	static public Texture2D gradientTexture
	{
		get
		{
			if (mGradientTex == null) mGradientTex = CreateGradientTex();
			return mGradientTex;
		}
	}

	/// <summary>
	/// Create a white dummy texture.
	/// </summary>

	static Texture2D CreateDummyTex ()
	{
		Texture2D tex = new Texture2D(1, 1);
		tex.name = "[Generated] Dummy Texture";
		tex.SetPixel(0, 0, Color.white);
		tex.Apply();
		tex.filterMode = FilterMode.Point;
		return tex;
	}

	/// <summary>
	/// Create a checker-background texture
	/// </summary>

	static Texture2D CreateCheckerTex (Color c0, Color c1)
	{
		Texture2D tex = new Texture2D(16, 16);
		tex.name = "[Generated] Checker Texture";

		for (int y = 0; y < 8;  ++y) for (int x = 0; x < 8;  ++x) tex.SetPixel(x, y, c1);
		for (int y = 8; y < 16; ++y) for (int x = 0; x < 8;  ++x) tex.SetPixel(x, y, c0);
		for (int y = 0; y < 8;  ++y) for (int x = 8; x < 16; ++x) tex.SetPixel(x, y, c0);
		for (int y = 8; y < 16; ++y) for (int x = 8; x < 16; ++x) tex.SetPixel(x, y, c1);

		tex.Apply();
		tex.filterMode = FilterMode.Point;
		return tex;
	}

	/// <summary>
	/// Create a gradient texture
	/// </summary>

	static Texture2D CreateGradientTex ()
	{
		Texture2D tex = new Texture2D(1, 16);
		tex.name = "[Generated] Gradient Texture";

		Color c0 = new Color(1f, 1f, 1f, 0f);
		Color c1 = new Color(1f, 1f, 1f, 0.4f);

		for (int i = 0; i < 16; ++i)
		{
			float f = Mathf.Abs((i / 15f) * 2f - 1f);
			f *= f;
			tex.SetPixel(0, i, Color.Lerp(c0, c1, f));
		}

		tex.Apply();
		tex.filterMode = FilterMode.Bilinear;
		return tex;
	}

	/// <summary>
	/// Draws the tiled texture. Like GUI.DrawTexture() but tiled instead of stretched.
	/// </summary>

	static public void DrawTiledTexture (Rect rect, Texture tex)
	{
		GUI.BeginGroup(rect);
		{
			int width  = Mathf.RoundToInt(rect.width);
			int height = Mathf.RoundToInt(rect.height);

			for (int y = 0; y < height; y += tex.height)
			{
				for (int x = 0; x < width; x += tex.width)
				{
					GUI.DrawTexture(new Rect(x, y, tex.width, tex.height), tex);
				}
			}
		}
		GUI.EndGroup();
	}

	/// <summary>
	/// Draw a single-pixel outline around the specified rectangle.
	/// </summary>

	static public void DrawOutline (Rect rect)
	{
		if (Event.current.type == EventType.Repaint)
		{
			Texture2D tex = contrastTexture;
			GUI.color = Color.white;
			DrawTiledTexture(new Rect(rect.xMin, rect.yMax, 1f, -rect.height), tex);
			DrawTiledTexture(new Rect(rect.xMax, rect.yMax, 1f, -rect.height), tex);
			DrawTiledTexture(new Rect(rect.xMin, rect.yMin, rect.width, 1f), tex);
			DrawTiledTexture(new Rect(rect.xMin, rect.yMax, rect.width, 1f), tex);
		}
	}

	/// <summary>
	/// Draw a single-pixel outline around the specified rectangle.
	/// </summary>

	static public void DrawOutline (Rect rect, Color color)
	{
		if (Event.current.type == EventType.Repaint)
		{
			Texture2D tex = blankTexture;
			GUI.color = color;
			GUI.DrawTexture(new Rect(rect.xMin, rect.yMin, 1f, rect.height), tex);
			GUI.DrawTexture(new Rect(rect.xMax, rect.yMin, 1f, rect.height), tex);
			GUI.DrawTexture(new Rect(rect.xMin, rect.yMin, rect.width, 1f), tex);
			GUI.DrawTexture(new Rect(rect.xMin, rect.yMax, rect.width, 1f), tex);
			GUI.color = Color.white;
		}
	}

	/// <summary>
	/// Draw a selection outline around the specified rectangle.
	/// </summary>

	static public void DrawOutline (Rect rect, Rect relative, Color color)
	{
		if (Event.current.type == EventType.Repaint)
		{
			// Calculate where the outer rectangle would be
			float x = rect.xMin + rect.width * relative.xMin;
			float y = rect.yMax - rect.height * relative.yMin;
			float width = rect.width * relative.width;
			float height = -rect.height * relative.height;
			relative = new Rect(x, y, width, height);

			// Draw the selection
			DrawOutline(relative, color);
		}
	}

	/// <summary>
	/// Draw a selection outline around the specified rectangle.
	/// </summary>

	static public void DrawOutline (Rect rect, Rect relative)
	{
		if (Event.current.type == EventType.Repaint)
		{
			// Calculate where the outer rectangle would be
			float x = rect.xMin + rect.width * relative.xMin;
			float y = rect.yMax - rect.height * relative.yMin;
			float width = rect.width * relative.width;
			float height = -rect.height * relative.height;
			relative = new Rect(x, y, width, height);

			// Draw the selection
			DrawOutline(relative);
		}
	}

	/// <summary>
	/// Draw a 9-sliced outline.
	/// </summary>

	static public void DrawOutline (Rect rect, Rect outer, Rect inner)
	{
		if (Event.current.type == EventType.Repaint)
		{
			Color green = new Color(0.4f, 1f, 0f, 1f);

			DrawOutline(rect, new Rect(outer.x, inner.y, outer.width, inner.height));
			DrawOutline(rect, new Rect(inner.x, outer.y, inner.width, outer.height));
			DrawOutline(rect, outer, green);
		}
	}

	/// <summary>
	/// Draw a checkered background for the specified texture.
	/// </summary>

	static Rect DrawBackground (Texture2D tex, float ratio)
	{
		Rect rect = GUILayoutUtility.GetRect(0f, 0f);
		rect.width = Screen.width - rect.xMin;
		rect.height = rect.width * ratio;
		GUILayout.Space(rect.height);

		if (Event.current.type == EventType.Repaint)
		{
			Texture2D blank = blankTexture;
			Texture2D check = backdropTexture;

			// Lines above and below the texture rectangle
			GUI.color = new Color(0f, 0f, 0f, 0.2f);
			GUI.DrawTexture(new Rect(rect.xMin, rect.yMin - 1, rect.width, 1f), blank);
			GUI.DrawTexture(new Rect(rect.xMin, rect.yMax, rect.width, 1f), blank);
			GUI.color = Color.white;

			// Checker background
			DrawTiledTexture(rect, check);
		}
		return rect;
	}

	/// <summary>
	/// Draw a texture atlas, complete with a background texture and an outline.
	/// </summary>

	static public Rect DrawAtlas (Texture2D tex, Material mat)
	{
		Rect rect = DrawBackground(tex, (float)tex.height / tex.width);

		if (Event.current.type == EventType.Repaint)
		{
			if (mat == null)
			{
				GUI.DrawTexture(rect, tex);
			}
			else
			{
				UnityEditor.EditorGUI.DrawPreviewTexture(rect, tex, mat);
			}
		}
		return rect;
	}

	/// <summary>
	/// Draw an enlarged sprite within the specified texture atlas.
	/// </summary>

	static public Rect DrawSprite (Texture2D tex, Rect sprite, Material mat)
	{
		float paddingX = 4f / tex.width;
		float paddingY = 4f / tex.height;
		float ratio = (sprite.height + paddingY) / (sprite.width + paddingX);

		ratio *= (float)tex.height / tex.width;

		// Draw the checkered background
		Rect rect = DrawBackground(tex, ratio);

		// We only want to draw into this rectangle
		GUI.BeginGroup(rect);
		{
			if (Event.current.type == EventType.Repaint)
			{
				// We need to calculate where to begin and how to stretch the texture
				// for it to appear properly scaled in the rectangle
				float scaleX = rect.width / (sprite.width + paddingX);
				float scaleY = rect.height / (sprite.height + paddingY);
				float ox = scaleX * (sprite.x - paddingX * 0.5f);
				float oy = scaleY * (1f - (sprite.yMax + paddingY * 0.5f));

				Rect drawRect = new Rect(-ox, -oy, scaleX, scaleY);

				if (mat == null)
				{
					GUI.DrawTexture(drawRect, tex);
				}
				else
				{
					// NOTE: DrawPreviewTexture doesn't seem to support BeginGroup-based clipping
					// when a custom material is specified. It seems to be a bug in Unity.
					// Passing 'null' for the material or omitting the parameter clips as expected.
					UnityEditor.EditorGUI.DrawPreviewTexture(drawRect, tex, mat);
					//UnityEditor.EditorGUI.DrawPreviewTexture(drawRect, tex);
					//GUI.DrawTexture(drawRect, tex);
				}
				rect = new Rect(drawRect.x + rect.x, drawRect.y + rect.y, drawRect.width, drawRect.height);
			}
		}
		GUI.EndGroup();
		return rect;
	}

	/// <summary>
	/// Draw a visible separator in addition to adding some padding.
	/// </summary>

	static public void DrawSeparator ()
	{
		GUILayout.Space(12f);

		if (Event.current.type == EventType.Repaint)
		{
			Texture2D tex = blankTexture;
			Rect rect = GUILayoutUtility.GetLastRect();
			GUI.color = new Color(0f, 0f, 0f, 0.25f);
			GUI.DrawTexture(new Rect(0f, rect.yMin + 6f, Screen.width, 4f), tex);
			GUI.DrawTexture(new Rect(0f, rect.yMin + 6f, Screen.width, 1f), tex);
			GUI.DrawTexture(new Rect(0f, rect.yMin + 9f, Screen.width, 1f), tex);
			GUI.color = Color.white;
		}
	}

	/// <summary>
	/// Draw a distinctly different looking header label
	/// </summary>

	static public Rect DrawHeader (string text)
	{
		GUILayout.Space(28f);
		Rect rect = GUILayoutUtility.GetLastRect();
		rect.yMin += 5f;
		rect.yMax -= 4f;
		rect.width = Screen.width;

		if (Event.current.type == EventType.Repaint)
		{
			GUI.color = Color.black;
			GUI.DrawTexture(new Rect(0f, rect.yMin, Screen.width, rect.yMax - rect.yMin), gradientTexture);
			GUI.color = new Color(0f, 0f, 0f, 0.25f);
			GUI.DrawTexture(new Rect(0f, rect.yMin, Screen.width, 1f), blankTexture);
			GUI.DrawTexture(new Rect(0f, rect.yMax - 1, Screen.width, 1f), blankTexture);
			GUI.color = Color.white;
			GUI.Label(new Rect(rect.x + 4f, rect.y, rect.width - 4, rect.height), text, EditorStyles.boldLabel);
		}
		return rect;
	}

    /// <summary>
    /// Draw a distinctly different looking header label
    /// </summary>

    static public bool DrawHeader(string text, string key, bool forceOn)
    {
        bool state = EditorPrefs.GetBool(key, true);

        GUILayout.Space(3f);
        if (!forceOn && !state) GUI.backgroundColor = new Color(0.8f, 0.8f, 0.8f);
        GUILayout.BeginHorizontal();
        GUILayout.Space(3f);

        GUI.changed = false;
#if UNITY_3_5
		if (!GUILayout.Toggle(true, text, "dragtab", GUILayout.MinWidth(20f))) state = !state;
#else
        if (!GUILayout.Toggle(true, "<b><size=11>" + text + "</size></b>", "dragtab", GUILayout.MinWidth(20f))) state = !state;
#endif
        if (GUI.changed) EditorPrefs.SetBool(key, state);

        GUILayout.Space(2f);
        GUILayout.EndHorizontal();
        GUI.backgroundColor = Color.white;
        if (!forceOn && !state) GUILayout.Space(3f);
        return state;
    }

	/// <summary>
	/// Draw a simple box outline for the entire line.
	/// </summary>

	static public void HighlightLine (Color c)
	{
		Rect rect = GUILayoutUtility.GetRect(Screen.width - 16f, 22f);
		GUILayout.Space(-23f);
		c.a *= 0.3f;
		GUI.color = c;
		GUI.DrawTexture(rect, gradientTexture);
		c.r *= 0.5f;
		c.g *= 0.5f;
		c.b *= 0.5f;
		GUI.color = c;
		GUI.DrawTexture(new Rect(rect.x, rect.y + 1f, rect.width, 1f), blankTexture);
		GUI.DrawTexture(new Rect(rect.x, rect.y + rect.height - 1f, rect.width, 1f), blankTexture);
		GUI.color = Color.white;
	}

	/// <summary>
	/// Convenience function that displays a list of sprites and returns the selected value.
	/// </summary>

	static public string DrawList (string field, string[] list, string selection, params GUILayoutOption[] options)
	{
		if (list != null && list.Length > 0)
		{
			int index = 0;
			if (string.IsNullOrEmpty(selection)) selection = list[0];

			// We need to find the sprite in order to have it selected
			if (!string.IsNullOrEmpty(selection))
			{
				for (int i = 0; i < list.Length; ++i)
				{
					if (selection.Equals(list[i], System.StringComparison.OrdinalIgnoreCase))
					{
						index = i;
						break;
					}
				}
			}

			// Draw the sprite selection popup
			index = EditorGUILayout.Popup(field, index, list, options);
			return list[index];
		}
		return null;
	}

	/// <summary>
	/// Helper function that returns the selected root object.
	/// </summary>

	static public GameObject SelectedRoot ()
	{
		GameObject go = Selection.activeGameObject;

		// Only use active objects
		if (go != null && !go.activeSelf) go = null;

		// No selection? Try to find the root automatically
		if (go == null)
		{
			UIPanel[] panels = GameObject.FindObjectsOfType(typeof(UIPanel)) as UIPanel[];

			foreach (UIPanel p in panels)
			{
				if (!p.gameObject.activeSelf) continue;
				go = p.gameObject;
				break;
			}
		}

		// Now find the first uniformly scaled object
		if (go != null)
		{
			Transform t = go.transform;

			// Find the first uniformly scaled object
			while (!Mathf.Approximately(t.localScale.x, t.localScale.y) ||
				   !Mathf.Approximately(t.localScale.x, t.localScale.z))
			{
				t = t.parent;

				if (t == null)
				{
					Debug.LogWarning("You must select a uniformly scaled object first.");
					return null;
				}
				else go = t.gameObject;
			}
		}
		return go;
	}

	/// <summary>
	/// Helper function that checks to see if this action would break the prefab connection.
	/// </summary>

	static public bool WillLosePrefab (GameObject root)
	{
		if (root == null) return false;

		if (root.transform != null)
		{
			// Check if the selected object is a prefab instance and display a warning
#if UNITY_3_4
			PrefabType type = EditorUtility.GetPrefabType(root);
#else
			PrefabType type = PrefabUtility.GetPrefabType(root);
#endif
			if (type == PrefabType.PrefabInstance)
			{
				return EditorUtility.DisplayDialog("Losing prefab",
					"This action will lose the prefab connection. Are you sure you wish to continue?",
					"Continue", "Cancel");
			}
		}
		return true;
	}

	/// <summary>
	/// Change the import settings of the specified texture asset, making it readable.
	/// </summary>

	static bool MakeTextureReadable (string path, bool force)
	{
		if (string.IsNullOrEmpty(path)) return false;
		TextureImporter ti = AssetImporter.GetAtPath(path) as TextureImporter;
		if (ti == null) return false;

		TextureImporterSettings settings = new TextureImporterSettings();
		ti.ReadTextureSettings(settings);

		if (force ||
			settings.mipmapEnabled ||
			!settings.readable ||
			settings.maxTextureSize < 4096 ||
			settings.filterMode != FilterMode.Point ||
			settings.wrapMode != TextureWrapMode.Clamp ||
			settings.npotScale != TextureImporterNPOTScale.None)
		{
			settings.mipmapEnabled = false;
			settings.readable = true;
			settings.maxTextureSize = 4096;
			settings.textureFormat = TextureImporterFormat.RGBA32;
			settings.filterMode = FilterMode.Point;
			settings.wrapMode = TextureWrapMode.Clamp;
			settings.npotScale = TextureImporterNPOTScale.None;
			
			ti.SetTextureSettings(settings);
			AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceUpdate);
		}
		return true;
	}

	/// <summary>
	/// Change the import settings of the specified texture asset, making it suitable to be used as a texture atlas.
	/// </summary>

	static bool MakeTextureAnAtlas (string path, bool force)
	{
		if (string.IsNullOrEmpty(path)) return false;
		TextureImporter ti = AssetImporter.GetAtPath(path) as TextureImporter;
		if (ti == null) return false;

		TextureImporterSettings settings = new TextureImporterSettings();
		ti.ReadTextureSettings(settings);

		if (force ||
			settings.readable ||
			settings.maxTextureSize < 4096 ||
			settings.wrapMode != TextureWrapMode.Clamp ||
			settings.npotScale != TextureImporterNPOTScale.ToNearest)
		{
			settings.mipmapEnabled = false;
			settings.readable = false;
			settings.maxTextureSize = 4096;
			settings.textureFormat = TextureImporterFormat.RGBA32;
			settings.filterMode = FilterMode.Trilinear;
			settings.aniso = 4;
			settings.wrapMode = TextureWrapMode.Clamp;
			settings.npotScale = TextureImporterNPOTScale.ToNearest;
			
			ti.SetTextureSettings(settings);
			AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceUpdate);
		}
		return true;
	}

	/// <summary>
	/// Fix the import settings for the specified texture, re-importing it if necessary.
	/// </summary>

	static public Texture2D ImportTexture (string path, bool forInput, bool force)
	{
		if (!string.IsNullOrEmpty(path))
		{
			if (forInput) { if (!MakeTextureReadable(path, force)) return null; }
			else if (!MakeTextureAnAtlas(path, force)) return null;
			return AssetDatabase.LoadAssetAtPath(path, typeof(Texture2D)) as Texture2D;
		}
		return null;
	}

	/// <summary>
	/// Fix the import settings for the specified texture, re-importing it if necessary.
	/// </summary>

	static public Texture2D ImportTexture (Texture tex, bool forInput, bool force)
	{
		if (tex != null)
		{
			string path = AssetDatabase.GetAssetPath(tex.GetInstanceID());
			return ImportTexture(path, forInput, force);
		}
		return null;
	}

	/// <summary>
	/// Figures out the saveable filename for the texture of the specified atlas.
	/// </summary>

	static public string GetSaveableTexturePath (UIAtlas atlas)
	{
		// Path where the texture atlas will be saved
		string path = "";

		// If the atlas already has a texture, overwrite its texture
		if (atlas.texture != null)
		{
			path = AssetDatabase.GetAssetPath(atlas.texture.GetInstanceID());

			if (!string.IsNullOrEmpty(path))
			{
				int dot = path.LastIndexOf('.');
				return path.Substring(0, dot) + ".png";
			}
		}

		// No texture to use -- figure out a name using the atlas
		path = AssetDatabase.GetAssetPath(atlas.GetInstanceID());
		path = string.IsNullOrEmpty(path) ? "Assets/" + atlas.name + ".png" : path.Replace(".prefab", ".png");
		return path;
	}

	/// <summary>
	/// Helper function that returns the folder where the current selection resides.
	/// </summary>

	static public string GetSelectionFolder ()
	{
		if (Selection.activeObject != null)
		{
			string path = AssetDatabase.GetAssetPath(Selection.activeObject.GetInstanceID());

			if (!string.IsNullOrEmpty(path))
			{
				int dot = path.LastIndexOf('.');
				int slash = Mathf.Max(path.LastIndexOf('/'), path.LastIndexOf('\\'));
				if (slash > 0) return (dot > slash) ? path.Substring(0, slash + 1) : path + "/";
			}
		}
		return "Assets/";
	}

	/// <summary>
	/// Struct type for the integer vector field below.
	/// </summary>

	public struct IntVector
	{
		public int x;
		public int y;
	}

	/// <summary>
	/// Integer vector field.
	/// </summary>

	static public IntVector IntPair (string prefix, string leftCaption, string rightCaption, int x, int y)
	{
		GUILayout.BeginHorizontal();

		if (string.IsNullOrEmpty(prefix))
		{
			GUILayout.Space(82f);
		}
		else
		{
			GUILayout.Label(prefix, GUILayout.Width(74f));
		}

        //EditorGUIUtility.LookLikeControls(48f);
        SetLabelWidth(48f);

		IntVector retVal;
		retVal.x = EditorGUILayout.IntField(leftCaption, x, GUILayout.MinWidth(30f));
		retVal.y = EditorGUILayout.IntField(rightCaption, y, GUILayout.MinWidth(30f));

        //EditorGUIUtility.LookLikeControls(80f);
        SetLabelWidth(80f);

		GUILayout.EndHorizontal();
		return retVal;
	}

	/// <summary>
	/// Integer rectangle field.
	/// </summary>

	static public Rect IntRect (string prefix, Rect rect)
	{
		int left	= Mathf.RoundToInt(rect.xMin);
		int top		= Mathf.RoundToInt(rect.yMin);
		int width	= Mathf.RoundToInt(rect.width);
		int height	= Mathf.RoundToInt(rect.height);

		NGUIEditorTools.IntVector a = NGUIEditorTools.IntPair(prefix, "Left", "Top", left, top);
		NGUIEditorTools.IntVector b = NGUIEditorTools.IntPair(null, "Width", "Height", width, height);

		return new Rect(a.x, a.y, b.x, b.y);
	}

	/// <summary>
	/// Integer vector field.
	/// </summary>

	static public Vector4 IntPadding (string prefix, Vector4 v)
	{
		int left	= Mathf.RoundToInt(v.x);
		int top		= Mathf.RoundToInt(v.y);
		int right	= Mathf.RoundToInt(v.z);
		int bottom	= Mathf.RoundToInt(v.w);

		NGUIEditorTools.IntVector a = NGUIEditorTools.IntPair(prefix, "Left", "Top", left, top);
		NGUIEditorTools.IntVector b = NGUIEditorTools.IntPair(null, "Right", "Bottom", right, bottom);

		return new Vector4(a.x, a.y, b.x, b.y);
	}

	/// <summary>
	/// Create an undo point for the specified objects.
	/// This action also marks the object as dirty so prefabs work correctly in 3.5.0 (work-around for a bug in Unity).
	/// </summary>

	static public void RegisterUndo (string name, params Object[] objects)
	{
		if (objects != null && objects.Length > 0)
		{
			foreach (Object obj in objects)
			{
				if (obj == null) continue;
                //Undo.RegisterUndo(obj, name);
                Undo.RecordObject(obj, name);
				EditorUtility.SetDirty(obj);
			}
		}
		else
		{
            //Undo.RegisterSceneUndo(name);
            Undo.RegisterCreatedObjectUndo(null, name);
		}
	}

    static public bool DrawPrefixButton(string text)
    {
        return GUILayout.Button(text, "DropDownButton", GUILayout.Width(76f));
    }

    /// <summary>
    /// Convenience function that displays a list of sprites and returns the selected value.
    /// </summary>

    static public void DrawAdvancedSpriteField(UIAtlas atlas, string spriteName, SpriteSelector.Callback callback, bool editable,
        params GUILayoutOption[] options)
    {
        if (atlas == null) return;

        // Give the user a warning if there are no sprites in the atlas
        if (atlas.spriteList.Count == 0)
        {
            EditorGUILayout.HelpBox("No sprites found", MessageType.Warning);
            return;
        }

        // Sprite selection drop-down list
        GUILayout.BeginHorizontal();
        {
            if (NGUIEditorTools.DrawPrefixButton("Sprite"))
            {
                NGUISettings.atlas = atlas;
                NGUISettings.selectedSprite = spriteName;
                SpriteSelector.Show(callback);
            }

            if (editable)
            {
                if (!string.Equals(spriteName, mLastSprite))
                {
                    mLastSprite = spriteName;
                    mEditedName = null;
                }

                string newName = GUILayout.TextField(string.IsNullOrEmpty(mEditedName) ? spriteName : mEditedName);

                if (newName != spriteName)
                {
                    mEditedName = newName;
                }
            }
            else
            {
                GUILayout.BeginHorizontal();
                GUILayout.Label(spriteName, "HelpBox", GUILayout.Height(18f));
                GUILayout.Space(18f);
                GUILayout.EndHorizontal();

                if (GUILayout.Button("Edit", GUILayout.Width(40f)))
                {
                    NGUISettings.atlas = atlas;
                    NGUISettings.selectedSprite = spriteName;
                    Select(atlas.gameObject);
                }
            }
        }
        GUILayout.EndHorizontal();
    }

    /// <summary>
    /// Draw a sprite selection field.
    /// </summary>

    static public void DrawSpriteField(string label, string caption, UIAtlas atlas, string spriteName, SpriteSelector.Callback callback, params GUILayoutOption[] options)
    {
        GUILayout.BeginHorizontal();
        GUILayout.Label(label, GUILayout.Width(76f));

        if (atlas.GetSprite(spriteName) == null)
            spriteName = "";

        if (GUILayout.Button(spriteName, "MiniPullDown", options))
        {
            NGUISettings.atlas = atlas;
            NGUISettings.selectedSprite = spriteName;
            SpriteSelector.Show(callback);
        }

        if (!string.IsNullOrEmpty(caption))
        {
            GUILayout.Space(20f);
            GUILayout.Label(caption);
        }
        GUILayout.EndHorizontal();
    }

    /// <summary>
    /// Select the specified game object and remember what was selected before.
    /// </summary>

    static public void Select(GameObject go)
    {
        Selection.activeGameObject = go;
    }

    /// <summary>
    /// Load the asset at the specified path.
    /// </summary>

    static public Object LoadAsset(string path)
    {
        if (string.IsNullOrEmpty(path)) return null;
        return AssetDatabase.LoadMainAssetAtPath(path);
    }

    /// <summary>
    /// Convenience function to load an asset of specified type, given the full path to it.
    /// </summary>

    static public T LoadAsset<T>(string path) where T : Object
    {
        Object obj = LoadAsset(path);
        if (obj == null) return null;

        T val = obj as T;
        if (val != null) return val;

        if (typeof(T).IsSubclassOf(typeof(Component)))
        {
            if (obj.GetType() == typeof(GameObject))
            {
                GameObject go = obj as GameObject;
                return go.GetComponent(typeof(T)) as T;
            }
        }
        return null;
    }

    /// <summary>
    /// Unity 4.3 changed the way LookLikeControls works.
    /// </summary>

    static public void SetLabelWidth(float width)
    {
#if UNITY_3_5 || UNITY_4_0 || UNITY_4_1 || UNITY_4_2
		EditorGUIUtility.LookLikeControls(width);
#else
        EditorGUIUtility.labelWidth = width;
#endif
    }

    /// <summary>
    /// Repaints all inspector windows related to sprite drawing.
    /// </summary>

    static public void RepaintSprites()
    {
        if (SpriteSelector.instance != null)
            SpriteSelector.instance.Repaint();
    }

    /// <summary>
    /// Select the specified sprite within the currently selected atlas.
    /// </summary>

    static public void SelectSprite(string spriteName)
    {
        if (NGUISettings.atlas != null)
        {
            NGUISettings.selectedSprite = spriteName;
            NGUIEditorTools.Select(NGUISettings.atlas.gameObject);
            RepaintSprites();
        }
    }

    public static Object[] GetAllObjectForAsset(string assetDir, bool includeChild = true, bool ingoreMeta = true)
    {
        string dirPathForDisk = GetPathForDisk(assetDir);

        string[] filePaths = GetAllFilePathForDisk(dirPathForDisk);

        if (filePaths == null) return null;

        List<Object> objs = new List<Object>();
        foreach (string fp in filePaths)
        {
            string assetPath = GetPathForAsset(fp);

            if ((File.GetAttributes(fp) & FileAttributes.Hidden) == FileAttributes.Hidden)
            {
                continue;
            }
            if (fp.ToLower().EndsWith(".meta"))
            {
                if (ingoreMeta)
                {
                    continue;
                }
                else
                {
                    objs.Add(AssetDatabase.LoadAssetAtPath(assetPath, typeof(Object)));
                }
            }

            objs.Add(AssetDatabase.LoadAssetAtPath(assetPath, typeof(Object)));
        }

        return objs.ToArray();

    }

    public static string[] GetAllFilePathForDisk(
        string dirPath,
        string searchPattern = "*.*",
        bool includeChild = true, bool
        ingoreHiden = true)
    {
        if (File.Exists(dirPath))
        {
            return new string[1] { dirPath };
        }

        if (!Directory.Exists(dirPath))
        {
            return null;
        }

        List<string> filePaths = new List<string>();
        string[] curFilePaths = Directory.GetFiles(dirPath, searchPattern, SearchOption.TopDirectoryOnly);

        if (ingoreHiden)
        {
            foreach (string file in curFilePaths)
            {
                if ((File.GetAttributes(file) & FileAttributes.Hidden) == 0)
                {
                    filePaths.Add(file);
                }
            }
        }
        else
        {
            filePaths.AddRange(curFilePaths);
        }

        if (includeChild)
        {
            string[] dirPaths = Directory.GetDirectories(dirPath);
            if (dirPaths != null && dirPath.Length != 0)
            {
                foreach (string dir in dirPaths)
                {
                    if (ingoreHiden)
                    {
                        if ((File.GetAttributes(dir) & FileAttributes.Hidden) != 0)
                        {
                            continue;
                        }
                    }

                    string[] files = GetAllFilePathForDisk(dir, searchPattern, includeChild, ingoreHiden);
                    if (files != null && files.Length != 0)
                        filePaths.AddRange(files);
                }
            }
        }

        for (int i = 0, count = filePaths.Count; i < count; i++)
        {
            filePaths[i] = filePaths[i].Replace("\\", "/");
        }

        return filePaths.ToArray();
    }

    public static string GetPathForAsset(string diskPath)
    {
        int index = diskPath.IndexOf("Assets/");
        if (index < 0)
        {
            Debug.LogWarning("Dont need to parse");

            return diskPath;
        }

        return diskPath.Substring(index);
    }

    public static string GetPathForDisk(string assetPath)
    {
        if (assetPath.StartsWith("Assets"))
        {
            return Application.dataPath + assetPath.Substring("Assets".Length);
        }

        return Application.dataPath + "/" + assetPath;
    }

    public static string[] GetSonDirectoryNameWithoutPath(string fatherPath)
    {
        string path = GetPathForDisk(fatherPath);
        string[] sonPaths = null;
        try
        {
            sonPaths = Directory.GetDirectories(path);
        }
        catch
        {
            return null;
        }
        if (sonPaths == null)
        {
            return null;
        }
        for (int i = 0; i < sonPaths.Length; ++i)
        {
            sonPaths[i] = GetNameWithoutPath(sonPaths[i]);
        }
        return sonPaths;
    }

    public static string[] GetFilesInDirectoryWithoutPathAndExtension(string fatherPath, string extension = "")
    {
        string path = GetPathForDisk(fatherPath);
        string[] sonFiles = null;
        try
        {
            sonFiles = Directory.GetFiles(path);
        }
        catch
        {
            return null;
        }

        if (sonFiles == null)
        {
            return null;
        }
        List<string> fiels = new List<string>();
        string tmpStr;
        for (int i = 0; i < sonFiles.Length; ++i)
        {
            tmpStr = GetNameWithoutPath(sonFiles[i]);
            tmpStr = tmpStr.Substring(0, tmpStr.LastIndexOf('.'));
            if (!string.IsNullOrEmpty(extension))
            {
                if (sonFiles[i].EndsWith(extension))
                {
                    fiels.Add(tmpStr);
                }
            }
            else
            {
                fiels.Add(tmpStr);
            }
        }
        return fiels.ToArray();
    }

    /// <summary>
    /// Get String Name Without Path
    /// </summary>
    /// <param name="path">The Full Path for Directory Or File</param>
    private static string GetNameWithoutPath(string path)
    {
        path = path.Replace('\\', '/');
        return path.Substring(path.LastIndexOf('/') + 1);
    }

    /// <summary>
    /// Begin drawing the content area.
    /// </summary>

    static public void BeginContents()
    {
        GUILayout.BeginHorizontal();
        GUILayout.Space(4f);
        EditorGUILayout.BeginHorizontal("AS TextArea", GUILayout.MinHeight(10f));
        GUILayout.BeginVertical();
        GUILayout.Space(2f);
    }

    /// <summary>
    /// End drawing the content area.
    /// </summary>

    static public void EndContents()
    {
        GUILayout.Space(3f);
        GUILayout.EndVertical();
        EditorGUILayout.EndHorizontal();
        GUILayout.Space(3f);
        GUILayout.EndHorizontal();
        GUILayout.Space(3f);
    }

    /// <summary>
    /// Helper function that draws a serialized property.
    /// </summary>

    static public SerializedProperty DrawProperty(string label, SerializedObject serializedObject, string property, params GUILayoutOption[] options)
    {
        return DrawProperty(label, serializedObject, property, false, options);
    }

    /// <summary>
    /// Helper function that draws a serialized property.
    /// </summary>

    static public SerializedProperty DrawProperty(string label, SerializedObject serializedObject, string property, bool padding, params GUILayoutOption[] options)
    {
        SerializedProperty sp = serializedObject.FindProperty(property);

        if (sp != null)
        {
            if (padding) EditorGUILayout.BeginHorizontal();

            if (label != null) EditorGUILayout.PropertyField(sp, new GUIContent(label), options);
            else EditorGUILayout.PropertyField(sp, options);

            if (padding)
            {
                GUILayout.Space(18f);
                EditorGUILayout.EndHorizontal();
            }
        }
        return sp;
    }

    /// <summary>
    /// Helper function that draws a serialized property.
    /// </summary>

    static public void DrawProperty(string label, SerializedProperty sp, params GUILayoutOption[] options)
    {
        DrawProperty(label, sp, true, options);
    }

    /// <summary>
    /// Helper function that draws a serialized property.
    /// </summary>

    static public void DrawProperty(string label, SerializedProperty sp, bool padding, params GUILayoutOption[] options)
    {
        if (sp != null)
        {
            if (padding) EditorGUILayout.BeginHorizontal();

            if (label != null) EditorGUILayout.PropertyField(sp, new GUIContent(label), options);
            else EditorGUILayout.PropertyField(sp, options);

            if (padding)
            {
                GUILayout.Space(18f);
                EditorGUILayout.EndHorizontal();
            }
        }
    }
}
