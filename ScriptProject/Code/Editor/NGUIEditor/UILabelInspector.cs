//----------------------------------------------
//            NGUI: Next-Gen UI kit
// Copyright © 2011-2012 Tasharen Entertainment
//----------------------------------------------

using UnityEngine;
using UnityEditor;
using System;

/// <summary>
/// Inspector class used to edit UILabels.
/// </summary>

[CustomEditor(typeof(UILabel)),CanEditMultipleObjects]
public class UILabelInspector : UIWidgetInspector
{
	UILabel mLabel;

	/// <summary>
	/// Register an Undo command with the Unity editor.
	/// </summary>

	void RegisterUndo() { NGUIEditorTools.RegisterUndo("Label Change", mLabel); }

	/// <summary>
	/// Font selection callback.
	/// </summary>

	void OnSelectFont(UnityEngine.Object obj)
	{
		if (mLabel != null)
		{
			NGUIEditorTools.RegisterUndo("Font Selection", mLabel);
			bool resize = (mLabel.font == null);
			mLabel.font = mLabel.font == null ? UIFont.CreateInstance<UIFont>() : mLabel.font;
			mLabel.TrueTypeFont = obj as Font;
			if (resize) mLabel.MakePixelPerfect();
		}
	}

	override protected void OnInit() { mAllowPreview = false; }

	override protected bool OnDrawProperties()
	{
		mLabel = mWidget as UILabel;
		if (GUILayout.Button("Font", GUILayout.Width(76f)))
		{
			ComponentSelectorNew.Show<Font>(OnSelectFont);
		}

		int fontSize = EditorGUILayout.IntField("FontSize:", mLabel.FontSize);
		if (fontSize != mLabel.FontSize) { RegisterUndo(); mLabel.FontSize = fontSize; }

		Font font = EditorGUILayout.ObjectField("", mLabel.TrueTypeFont, typeof(Font), false) as Font;
		if (font != mLabel.TrueTypeFont)
		{
			mLabel.TrueTypeFont = font;
		}

		EditorGUILayout.ObjectField("Material", mLabel.material, typeof(Material), false);

		if (mLabel.font == null) return false;

		string text = EditorGUILayout.TextArea(mLabel.text, GUILayout.Height(100f));
		if (!text.Equals(mLabel.text)) { RegisterUndo(); mLabel.text = text; }

		GUILayout.BeginHorizontal();
		{
			int len = EditorGUILayout.IntField("Line Width", mLabel.lineWidth, GUILayout.Width(120f));
			if (len != mLabel.lineWidth) { RegisterUndo(); mLabel.lineWidth = len; }

			bool multi = EditorGUILayout.Toggle("Multi-line", mLabel.multiLine, GUILayout.Width(100f));
			if (multi != mLabel.multiLine) { RegisterUndo(); mLabel.multiLine = multi; }
		}
		GUILayout.EndHorizontal();

        GUILayout.BeginHorizontal();
        {
            int len = EditorGUILayout.IntField("Fixed Chinese", mLabel.FixedWidthForChinese, GUILayout.Width(120f));
            if (len != mLabel.FixedWidthForChinese) { RegisterUndo(); mLabel.FixedWidthForChinese = len; }
        }
        GUILayout.EndHorizontal();

		int spaceingY = EditorGUILayout.IntField("LineSpacingY", mLabel.lineSpacingY);
		if (spaceingY != mLabel.lineSpacingY) { RegisterUndo(); mLabel.lineSpacingY = spaceingY; }

		int spaceingX = EditorGUILayout.IntField("FontSpacingX", mLabel.FontSpacingX);
		if (spaceingX != mLabel.FontSpacingX) { RegisterUndo(); mLabel.FontSpacingX = spaceingX; }

		GUILayout.BeginHorizontal();
		{
			bool password = EditorGUILayout.Toggle("Password", mLabel.password, GUILayout.Width(120f));
			if (password != mLabel.password) { RegisterUndo(); mLabel.password = password; }

			bool encoding = EditorGUILayout.Toggle("Encoding", mLabel.supportEncoding, GUILayout.Width(100f));
			if (encoding != mLabel.supportEncoding) { RegisterUndo(); mLabel.supportEncoding = encoding; }
		}
		GUILayout.EndHorizontal();

		GUILayout.BeginHorizontal();
		{
			UILabel.Effect effect = (UILabel.Effect)EditorGUILayout.EnumPopup("Effect", mLabel.effectStyle, GUILayout.Width(170f));
			if (effect != mLabel.effectStyle) { RegisterUndo(); mLabel.effectStyle = effect; }

			if (effect != UILabel.Effect.None)
			{
				Color c = EditorGUILayout.ColorField(mLabel.effectColor);
				if (mLabel.effectColor != c) { RegisterUndo(); mLabel.effectColor = c; }
			}
		}
		GUILayout.EndHorizontal();
		FontStyle style = (FontStyle)EditorGUILayout.EnumPopup("FontStyle:", mLabel.FontStyle, GUILayout.Width(200f));
		if (style != mLabel.FontStyle)
		{
			RegisterUndo();
			mLabel.FontStyle = style;
		}

		GUILayout.BeginHorizontal();
		Color color = EditorGUILayout.ColorField("Top Color", mLabel.TopColor);

        uint colorIntValue = (uint)NGUIMath.ColorToInt(color);
        colorIntValue &=0xFFFFFFFF;
        string colorRGBA = colorIntValue.ToString("X8");
        string ncolorRGBA = EditorGUILayout.TextField("RGBA:", colorRGBA);
        if (ncolorRGBA != colorRGBA)
        {
            color = ColorOperation.GetColorFromRGBAStr(ncolorRGBA);
        }

		if (mLabel.TopColor != color)
		{
			NGUIEditorTools.RegisterUndo("Color Change", mLabel);
			mLabel.TopColor = color;
		}
		GUILayout.EndHorizontal();

		GUILayout.BeginHorizontal();
		color = EditorGUILayout.ColorField("Bottom Color", mLabel.BottomColor);

        colorIntValue = (uint)NGUIMath.ColorToInt(color);
        colorIntValue &= 0xFFFFFFFF;
        colorRGBA = colorIntValue.ToString("X8");
        ncolorRGBA = EditorGUILayout.TextField("RGBA:", colorRGBA);
        if (ncolorRGBA != colorRGBA)
        {
            color = ColorOperation.GetColorFromRGBAStr(ncolorRGBA);
        }

		if (mLabel.BottomColor != color)
		{
			NGUIEditorTools.RegisterUndo("Color Change", mLabel);
			mLabel.BottomColor = color;
		}
		GUILayout.EndHorizontal();

		return true;
	}

	override protected void OnDrawTexture()
	{
		Texture2D tex = mLabel.mainTexture as Texture2D;

		if (tex != null)
		{
			// Draw the atlas
			EditorGUILayout.Separator();
			NGUIEditorTools.DrawSprite(tex, mLabel.font.uvRect, mUseShader ? mLabel.material : null);

			// Sprite size label
			Rect rect = GUILayoutUtility.GetRect(Screen.width, 18f);
			EditorGUI.DropShadowLabel(rect, "Font Size: " + mLabel.FontSize);
		}
	}
}
