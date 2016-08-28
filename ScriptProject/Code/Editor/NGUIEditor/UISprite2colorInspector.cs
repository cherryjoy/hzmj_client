using UnityEngine;
using UnityEditor;
using System.Collections;

[CustomEditor(typeof(UISprite2color))]
public class UISprite2colorInspector : UISpriteInspector {
    override protected bool OnDrawProperties()
    {
        UISprite2color mColorSprite = mWidget as UISprite2color;
        if (!base.OnDrawProperties())
        {
            return false;
        }
        GUILayout.BeginHorizontal();
        Color color = EditorGUILayout.ColorField("Top Color", mColorSprite.UpColor);

        if (mColorSprite.UpColor != color)
        {
            NGUIEditorTools.RegisterUndo("Top Change", mColorSprite);
            mColorSprite.UpColor = color;
        }
        GUILayout.EndHorizontal();

        GUILayout.BeginHorizontal();
        color = EditorGUILayout.ColorField("Bottom Color", mColorSprite.DownColor);

        if (mColorSprite.DownColor != color)
        {
            NGUIEditorTools.RegisterUndo("Bottom Color Change", mColorSprite);
            mColorSprite.DownColor = color;
        }
        GUILayout.EndHorizontal();
        return true;
    }
}
