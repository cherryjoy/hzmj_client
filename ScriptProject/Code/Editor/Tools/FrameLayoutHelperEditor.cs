using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(FrameLayoutHelper),true)]
public class FrameLayoutHelperEditor : Editor
{
    private FrameLayoutHelper helper = null;

    private Vector2 frameSize = Vector2.zero;
    private UIWidget.Pivot pivot = UIWidget.Pivot.TopLeft;
    public override void OnInspectorGUI()
    {
        helper = (FrameLayoutHelper)target;
        if(helper.bgSprite!=null)
        {
            frameSize = helper.bgSprite.Dimensions;
        }else
        {
            base.OnInspectorGUI();
            return;
        }

        EditorGUILayout.BeginVertical();
        {
            base.OnInspectorGUI();
            if (frameSize.x == 0 || frameSize.y == 0)
            {
                frameSize = helper.bgSprite.Dimensions;
            }

            Vector2 fSize = EditorGUILayout.Vector2Field("FrameSize:", frameSize);
            UIWidget.Pivot p = (UIWidget.Pivot)EditorGUILayout.EnumPopup("Frame Pivot:", pivot);
            if(fSize.x != frameSize.x || frameSize.y!=fSize.y)
            {
                if(fSize.x > 1 && fSize.y>1)
                {
                    helper.LayoutFrame(fSize);
                    frameSize = fSize;

                    FrameTitleLayoutHelper ftlHelper = helper.GetComponentInChildren<FrameTitleLayoutHelper>();
                    if(ftlHelper!=null)
                    {
                        ftlHelper.LayoutFrame(fSize);
                    }
                }
            }

            if (p != pivot)
            {
                pivot = p;
                helper.PivotFrame(pivot, frameSize);
            }
        }
        EditorGUILayout.EndVertical();
    }
}

