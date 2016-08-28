using UnityEngine;
using UnityEditor;
using System.Collections;

[CustomEditor(typeof(UIColorLabel))]
public class UIColorLabelInspector : UILabelInspector
{

    override protected bool OnDrawProperties()
    {
        UIColorLabel mColorLabel = mWidget as UIColorLabel;
        if (!base.OnDrawProperties())
        {
            return false;
        }

        GUILayout.BeginHorizontal();
        Color color = EditorGUILayout.ColorField("Middle Color", mColorLabel.MiddleColor);

        if (mColorLabel.MiddleColor != color)
        {
            NGUIEditorTools.RegisterUndo("Color Change", mColorLabel);
            mColorLabel.MiddleColor = color;
        }
        GUILayout.EndHorizontal();

        return true;
    }


}
