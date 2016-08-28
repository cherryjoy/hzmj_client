//----------------------------------------------
//            NGUI: Next-Gen UI kit
// Copyright © 2011-2012 Tasharen Entertainment
//----------------------------------------------

using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

/// <summary>
/// Inspector class used to edit UIPopupLists.
/// </summary>

[CustomEditor(typeof(UIPopupList))]
public class UIPopupListInspector : Editor
{
	UIPopupList mList;

	void RegisterUndo ()
	{
		NGUIEditorTools.RegisterUndo("Popup List Change", mList);
	}

	void OnSelectAtlas (MonoBehaviour obj)
	{
		RegisterUndo();
		mList.atlas = obj as UIAtlas;
	}
	
	void OnSelectFont (Object obj)
	{
		RegisterUndo();
		mList.font = UIFont.CreateInstance<UIFont>();
		mList.font.TrueTypeFont = obj as Font;
	}

	public override void OnInspectorGUI ()
	{
		NGUIEditorTools.SetLabelWidth(80f);
		mList = target as UIPopupList;

		ComponentSelector.Draw<UIAtlas>(mList.atlas, OnSelectAtlas);
		ComponentSelectorNew.Draw<Font>(mList.font.TrueTypeFont, OnSelectFont);

		UILabel lbl = EditorGUILayout.ObjectField("Text Label", mList.textLabel, typeof(UILabel), true) as UILabel;

		if (mList.textLabel != lbl)
		{
			RegisterUndo();
			mList.textLabel = lbl;
			if (lbl != null) lbl.text = mList.selection;
		}

		if (mList.atlas != null)
		{
            NGUIEditorTools.DrawSpriteField("Background", "Foreground sprite (shown on the button)", mList.atlas, mList.backgroundSprite, (name) =>
            {
                if (name != mList.backgroundSprite) { RegisterUndo(); mList.backgroundSprite = name; }
            }, GUILayout.Width(120f));

            NGUIEditorTools.DrawSpriteField("Highlight", "Sprite used to highlight the selected option", mList.atlas, mList.highlightSprite, (name) =>
            {
                if (name != mList.highlightSprite) { RegisterUndo(); mList.highlightSprite = name; }
            }, GUILayout.Width(120f));

			GUILayout.BeginHorizontal();
			GUILayout.Space(6f);
			GUILayout.Label("Options");
			GUILayout.EndHorizontal();

			string text = "";
			foreach (string s in mList.items) text += s + "\n";

			GUILayout.Space(-22f);
			GUILayout.BeginHorizontal();
			GUILayout.Space(84f);
			string modified = EditorGUILayout.TextArea(text, GUILayout.Height(100f));
			GUILayout.EndHorizontal();

			if (modified != text)
			{
				RegisterUndo();
				string[] split = modified.Split(new char[] { '\n' }, System.StringSplitOptions.RemoveEmptyEntries);
				mList.items.Clear();
				foreach (string s in split) mList.items.Add(s);

				if (string.IsNullOrEmpty(mList.selection) || !mList.items.Contains(mList.selection))
				{
					mList.selection = mList.items.Count > 0 ? mList.items[0] : "";
				}
			}

			string sel = NGUIEditorTools.DrawList("Selection", mList.items.ToArray(), mList.selection);

			if (mList.selection != sel)
			{
				RegisterUndo();
				mList.selection = sel;
			}

			float ts = EditorGUILayout.FloatField("Text Scale", mList.textScale);
			Color tc = EditorGUILayout.ColorField("Text Color", mList.textColor);
			Color bc = EditorGUILayout.ColorField("Background", mList.backgroundColor);
			Color hc = EditorGUILayout.ColorField("Highlight", mList.highlightColor);

			GUILayout.BeginHorizontal();
			bool isLocalized = EditorGUILayout.Toggle("Localized", mList.isLocalized, GUILayout.Width(100f));
			bool isAnimated = EditorGUILayout.Toggle("Animated", mList.isAnimated);
			GUILayout.EndHorizontal();

			if (mList.textScale != ts ||
				mList.textColor != tc ||
				mList.highlightColor != hc ||
				mList.backgroundColor != bc ||
				mList.isLocalized != isLocalized ||
				mList.isAnimated != isAnimated)
			{
				RegisterUndo();
				mList.textScale = ts;
				mList.textColor = tc;
				mList.backgroundColor = bc;
				mList.highlightColor = hc;
				mList.isLocalized = isLocalized;
				mList.isAnimated = isAnimated;
			}

			NGUIEditorTools.DrawSeparator();

			GUILayout.BeginHorizontal();
			GUILayout.Space(6f);
			GUILayout.Label("Padding", GUILayout.Width(76f));
			GUILayout.BeginVertical();
			GUILayout.Space(-12f);
			Vector2 padding = EditorGUILayout.Vector2Field("", mList.padding);
			GUILayout.EndVertical();
			GUILayout.EndHorizontal();
			
			if (mList.padding != padding)
			{
				RegisterUndo();
				mList.padding = padding;
			}

			NGUIEditorTools.SetLabelWidth(100f);

			GameObject go = EditorGUILayout.ObjectField("Event Receiver", mList.eventReceiver,
				typeof(GameObject), true) as GameObject;

			string fn = EditorGUILayout.TextField("Function Name", mList.functionName);

			if (mList.eventReceiver != go || mList.functionName != fn)
			{
				RegisterUndo();
				mList.eventReceiver = go;
				mList.functionName = fn;
			}
		}
	}
}
