//----------------------------------------------
//            NGUI: Next-Gen UI kit
// Copyright ?2011-2012 Tasharen Entertainment
//----------------------------------------------

using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

/// <summary>
/// Inspector class used to edit UISprites.
/// </summary>

[CustomEditor(typeof(UISprite))]
public class UISpriteInspector : UIWidgetInspector
{
	protected UISprite mSprite;

	/// <summary>
	/// Atlas selection callback.
	/// </summary>

	void OnSelectAtlas (MonoBehaviour obj)
	{
		if (mSprite != null)
		{
			NGUIEditorTools.RegisterUndo("Atlas Selection", mSprite);
			bool resize = (mSprite.atlas == null);
			mSprite.atlas = obj as UIAtlas;
            NGUISettings.atlas = mSprite.atlas;
			mSprite.MarkAsChanged();
			if (resize) mSprite.MakePixelPerfect();
			EditorUtility.SetDirty(mSprite.gameObject);
		}
	}

	/// <summary>
	/// Convenience function that displays a list of sprites and returns the selected value.
	/// </summary>

	static public string SpriteField (UIAtlas atlas, string field, string name, params GUILayoutOption[] options)
	{
		List<string> sprites = atlas.GetListOfSprites();
		return (sprites != null && sprites.Count > 0) ? NGUIEditorTools.DrawList(field, sprites.ToArray(), name, options) : null;
	}

	/// <summary>
	/// Draw a sprite selection field.
	/// </summary>

	static public void SpriteField(string fieldName, string caption, UIAtlas atlas, string spriteName, SpriteSelector.Callback callback)
	{
        NGUIEditorTools.DrawAdvancedSpriteField(atlas, spriteName, callback, false);
	}


	/// <summary>
	/// Convenience function that displays a list of sprites and returns the selected value.
	/// </summary>

	static public string SpriteField (UIAtlas atlas, string name)
	{
		return SpriteField(atlas, "Sprite", name);
	}

	/// <summary>
	/// Draw the atlas and sprite selection fields.
	/// </summary>

	override protected bool OnDrawProperties ()
	{
		mSprite = mWidget as UISprite;
        mSprite.IsKeepOriSize = EditorGUILayout.Toggle("保持原图大小", mSprite.IsKeepOriSize, GUILayout.Width(100f));
		ComponentSelector.Draw<UIAtlas>(mSprite.atlas, OnSelectAtlas);
		if (mSprite.atlas == null) return false;

        SerializedProperty sp = serializedObject.FindProperty("mSpriteName");
		NGUISettings.atlas = mSprite.atlas;
        //NGUIEditorTools.DrawAdvancedSpriteField(atlas.objectReferenceValue as UIAtlas, sp.stringValue, SelectSprite, false);
        NGUIEditorTools.DrawAdvancedSpriteField(NGUISettings.atlas, sp.stringValue, SelectSprite, false);
		return true;
	}

    /// <summary>
    /// Sprite selection callback function.
    /// </summary>

    void SelectSprite(string spriteName)
    {
        serializedObject.Update();
        SerializedProperty sp = serializedObject.FindProperty("mSpriteName");
        sp.stringValue = spriteName;
        serializedObject.ApplyModifiedProperties();
        NGUISettings.selectedSprite = spriteName;
		if (mSprite != null)
		{
			mSprite.UpdateUVs();
			mSprite.MarkAsChanged();
		}
    }

	/// <summary>
	/// Draw the sprite texture.
	/// </summary>

	override protected void OnDrawTexture ()
	{
		Texture2D tex = mSprite.mainTexture as Texture2D;

		if (tex != null)
		{
			// Draw the atlas
			EditorGUILayout.Separator();
			Rect rect = NGUIEditorTools.DrawSprite(tex, mSprite.outerUV, mUseShader ? mSprite.atlas.spriteMaterial : null);

			// Draw the selection
			NGUIEditorTools.DrawOutline(rect, mSprite.outerUV, new Color(0.4f, 1f, 0f, 1f));

			// Sprite size label
			string text = "Sprite Size: ";
			text += Mathf.RoundToInt(Mathf.Abs(mSprite.outerUV.width * tex.width));
			text += "x";
			text += Mathf.RoundToInt(Mathf.Abs(mSprite.outerUV.height * tex.height));

			rect = GUILayoutUtility.GetRect(Screen.width, 18f);
			EditorGUI.DropShadowLabel(rect, text);
		}
	}
}
