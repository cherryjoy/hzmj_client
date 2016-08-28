using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

[CustomEditor(typeof(UIColorSprite))]
public class UIColorSpriteInspector : UISpriteInspector
{
	override protected bool OnDrawProperties()
	{
		UIColorSprite mColorSprite = mWidget as UIColorSprite;
		if (!base.OnDrawProperties())
		{
			return false;
		}
		GUILayout.BeginHorizontal();
		Color color = EditorGUILayout.ColorField("Top Color", mColorSprite.TopColor);

		if (mColorSprite.TopColor != color)
		{
			NGUIEditorTools.RegisterUndo("Top Change", mColorSprite);
			mColorSprite.TopColor = color;
		}
		GUILayout.EndHorizontal();

		GUILayout.BeginHorizontal();
		color = EditorGUILayout.ColorField("Middle Color", mColorSprite.MiddleColor);

		if (mColorSprite.MiddleColor != color)
		{
			NGUIEditorTools.RegisterUndo("Middle Change", mColorSprite);
			mColorSprite.MiddleColor = color;
		}
		GUILayout.EndHorizontal();

		GUILayout.BeginHorizontal();
		color = EditorGUILayout.ColorField("Bottom Color", mColorSprite.BottomColor);

		if (mColorSprite.BottomColor != color)
		{
			NGUIEditorTools.RegisterUndo("Bottom Color Change", mColorSprite);
			mColorSprite.BottomColor = color;
		}
		GUILayout.EndHorizontal();
		return true;
	}

}
