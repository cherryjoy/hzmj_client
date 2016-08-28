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

[CustomEditor(typeof(UIImageButton))]
public class UIImageButtonInspector : Editor
{
	UIImageButton mButton;

	/// <summary>
	/// Atlas selection callback.
	/// </summary>

	void OnSelectAtlas (MonoBehaviour obj)
	{
		if (mButton.targets.Length != 0)
		{
			NGUIEditorTools.RegisterUndo("Atlas Selection", mButton.targets);
            foreach (UISprite tar in mButton.targets)
            {
                if (tar != null)
                {
                    tar.atlas = obj as UIAtlas;
                    tar.MakePixelPerfect();
                }
            }
		}
	}

	public override void OnInspectorGUI ()
	{
		NGUIEditorTools.SetLabelWidth(80f);
		mButton = target as UIImageButton;

        if (mButton.targets == null)
            mButton.targets = new UISprite[0];

// 
// 		UISprite sprite = EditorGUILayout.ObjectField("Sprite", mButton.targets, typeof(UISprite), true) as UISprite;
// 
// 		if (mButton.targets != sprite)
// 		{
// 			NGUIEditorTools.RegisterUndo("Image Button Change", mButton);
// 			mButton.targets = sprite;
// 			if (sprite != null) sprite.spriteName = mButton.normalSprite;
// 		}
//         if (parDelIndex >= 0)
//         {
//             mButton.targets = ArrayInspector.ArrayRemoveByIndex(mButton.targets, parDelIndex);
//             parDelIndex = -1;
//         }

      //  mButton.targets = DrawLuaGameObjList(mButton.targets);
        serializedObject.Update();
        EditorGUIUtility.LookLikeInspector();
        SerializedProperty tps = serializedObject.FindProperty("targets");
        EditorGUI.BeginChangeCheck();
        EditorGUILayout.PropertyField(tps, true);
        if (EditorGUI.EndChangeCheck()) {
            serializedObject.ApplyModifiedProperties();
        }
        EditorGUIUtility.LookLikeControls();


        if (mButton.targets.Length != 0)
		{
            UISprite sprite = mButton.targets[0];
            if (sprite != null) {
                ComponentSelector.Draw<UIAtlas>(sprite.atlas, OnSelectAtlas);

                if (sprite.atlas != null)
                {
                    string normal = mButton.normalSprite;
                    string hover = mButton.hoverSprite;
                    string press = mButton.pressedSprite;
                    string disable = mButton.disableSprite;
                    NGUIEditorTools.DrawSpriteField("Normal", "Normal state sprite", sprite.atlas, normal, (name) =>
                    {
                        if (name != normal) { normal = name; Callback(sprite, normal, hover, press, disable); }
                    }, GUILayout.Width(120f));
                    NGUIEditorTools.DrawSpriteField("Hover", "Hover state sprite", sprite.atlas, hover, (name) =>
                    {
                        if (hover != name) { hover = name; Callback(sprite, normal, hover, press, disable); }
                    }, GUILayout.Width(120f));
                    NGUIEditorTools.DrawSpriteField("Pressed", "Pressed state sprite", sprite.atlas, press, (name) =>
                    {
                        if (press != name) { press = name; Callback(sprite, normal, hover, press, disable); }
                    }, GUILayout.Width(120f));
                    NGUIEditorTools.DrawSpriteField("Disabled", "Disabled state sprite", sprite.atlas, disable, (name) =>
                    {
                        if (disable != name) { disable = name; Callback(sprite, normal, hover, press, disable); }
                    }, GUILayout.Width(120f));
                }
            }
            
		}
	}
//     int parDelIndex = -1;
//     UISprite[] DrawLuaGameObjList(UISprite[] objList)
//     {
//         GUILayout.BeginHorizontal();
//         if (GUILayout.Button("Add Obj"))
//         {
//             objList = ArrayInspector.AddOne2Array(objList);
//         }
//         if (GUILayout.Button("Delete Obj"))
//         {
//             if (objList.Length > 0)
//             {
//                 objList = ArrayInspector.RemoveOneFromArray(objList);
//             }
//         }
//         GUILayout.EndHorizontal();
// 
//         for (int i = 0; i < objList.Length; i++)
//         {
// 
//             GUILayout.BeginHorizontal();
//             GUI.backgroundColor = Color.blue;
//             bool IsObjAssignFirst = false;
//             UISprite objSelect = EditorGUILayout.ObjectField("Obj", objList[i], typeof(UISprite), true) as UISprite;
// 
//             GUI.backgroundColor = Color.red;
//             if (GUILayout.Button("X", GUILayout.Width(22f)))
//             {
//                 parDelIndex = i;
//             }
//             GUI.backgroundColor = Color.white;
//             GUILayout.EndHorizontal();
// 
// 
//         }
// 
//         return objList;
//     }

    private void Callback(UISprite sprite, string normal, string hover, string press, string disable)
    {
        if (mButton.normalSprite != normal ||
                           mButton.hoverSprite != hover ||
                           mButton.pressedSprite != press ||
                           mButton.disableSprite != disable)
        {
            NGUIEditorTools.RegisterUndo("Image Button Change", mButton, mButton.gameObject, sprite);
            mButton.normalSprite = normal;
            mButton.hoverSprite = hover;
            mButton.pressedSprite = press;
            mButton.disableSprite = disable;
            sprite.spriteName = normal;
            sprite.MakePixelPerfect();
        }
    }
}
