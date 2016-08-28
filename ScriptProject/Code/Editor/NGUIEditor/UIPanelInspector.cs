//----------------------------------------------
//            NGUI: Next-Gen UI kit
// Copyright © 2011-2012 Tasharen Entertainment
//----------------------------------------------

using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

[CustomEditor(typeof(UIPanel))]
public class UIPanelInspector : Editor
{
	/// <summary>
	/// Draw the inspector widget.
	/// </summary>

	public override void OnInspectorGUI ()
	{
		UIPanel panel = target as UIPanel;
		List<UIDrawCall> drawcalls = panel.drawCalls;
		NGUIEditorTools.SetLabelWidth(80f);

		NGUIEditorTools.DrawSeparator();
		
		if(panel.showProgress != EditorGUILayout.Toggle("Show Progress", panel.showProgress))
		{
			panel.showProgress = !panel.showProgress;
			EditorUtility.SetDirty(panel);
			EditorWindow.FocusWindowIfItsOpen<UIPanelTool>();			
		}
		
		if (panel.showInPanelTool != EditorGUILayout.Toggle("Panel Tool", panel.showInPanelTool))
		{
			panel.showInPanelTool = !panel.showInPanelTool;
			EditorUtility.SetDirty(panel);
			EditorWindow.FocusWindowIfItsOpen<UIPanelTool>();
		}

		bool depth = EditorGUILayout.Toggle("Depth Pass", panel.depthPass);

		if (panel.depthPass != depth)
		{
			panel.depthPass = depth;
			panel.UpdateDrawcalls();
			EditorUtility.SetDirty(panel);
		}

		EditorGUILayout.LabelField("Widgets", panel.widgets.Count.ToString());
		EditorGUILayout.LabelField("Draw Calls", drawcalls.Count.ToString());

		UIPanel.DebugInfo di = (UIPanel.DebugInfo)EditorGUILayout.EnumPopup("Debug Info", panel.debugInfo);

		if (panel.debugInfo != di)
		{
			panel.debugInfo = di;
			EditorUtility.SetDirty(panel);
		}

		UIDrawCall.Clipping clipping = (UIDrawCall.Clipping)EditorGUILayout.EnumPopup("Clipping", panel.clipping);

		if (panel.clipping != clipping)
		{
			panel.clipping = clipping;
			EditorUtility.SetDirty(panel);
		}

		if (panel.clipping != UIDrawCall.Clipping.None)
		{
			Vector4 range = panel.clipRange;

			GUILayout.BeginHorizontal();
			GUILayout.Space(80f);
			Vector2 pos = EditorGUILayout.Vector2Field("Center", new Vector2(range.x, range.y));
			GUILayout.EndHorizontal();

			GUILayout.BeginHorizontal();
			GUILayout.Space(80f);
			Vector2 size = EditorGUILayout.Vector2Field("Size", new Vector2(range.z, range.w));
			GUILayout.EndHorizontal();

			if (size.x < 0f) size.x = 0f;
			if (size.y < 0f) size.y = 0f;

			range.x = pos.x;
			range.y = pos.y;
			range.z = size.x;
			range.w = size.y;

			if (panel.clipRange != range)
			{
				NGUIEditorTools.RegisterUndo("Clipping Change", panel);
				panel.clipRange = range;
				EditorUtility.SetDirty(panel);
			}
			
			if (panel.clipping != UIDrawCall.Clipping.SoftClip)   
			{
				panel.clipping = UIDrawCall.Clipping.None;
			}
			
			if (panel.clipping == UIDrawCall.Clipping.SoftClip)
			{
				GUILayout.BeginHorizontal();
				GUILayout.Space(80f);
				Vector2 soft = EditorGUILayout.Vector2Field("Softness", panel.clipSoftness);
				GUILayout.EndHorizontal();

				if (soft.x < 1f) soft.x = 1f;
				if (soft.y < 1f) soft.y = 1f;

				if (panel.clipSoftness != soft)
				{
					NGUIEditorTools.RegisterUndo("Clipping Change", panel);
					panel.clipSoftness = soft;
					EditorUtility.SetDirty(panel);
				}
			}
		}

        UIPanel.PanelDepthFlag pf = (UIPanel.PanelDepthFlag)EditorGUILayout.EnumPopup("Depth Flag", panel.DepthFlag);
        if (pf != panel.DepthFlag)
        {
            panel.DepthFlag = pf;
            EditorUtility.SetDirty(panel);
        }

		//panel.order = (UIOrder)EditorGUILayout.ObjectField("UIOrder", panel.order,typeof(UIOrder));

		foreach (UIDrawCall dc in drawcalls)
		{
			NGUIEditorTools.DrawSeparator();
			EditorGUILayout.ObjectField("Material", dc.material, typeof(Material), false);
			EditorGUILayout.LabelField("Triangles", dc.triangles.ToString());
		}

        //DrawFinalProperties(panel);
	}

    /// <summary>
    /// Add the "Show draw calls" button at the very end.
    /// </summary>

    //void DrawFinalProperties(UIPanel panel)
    //{
    //    if (GUILayout.Button("Show Draw Calls"))
    //    {
    //        NGUISettings.showAllDCs = false;

    //        if (UIDrawCallViewer.instance != null)
    //        {
    //            UIDrawCallViewer.instance.Focus();
    //            UIDrawCallViewer.instance.Repaint();
    //        }
    //        else
    //        {
    //           EditorWindow.GetWindow<UIDrawCallViewer>(false, "Draw Call Tool", true);
    //        }
    //        UIDrawCallViewer.instance.SetPanelDrawcalls(panel);
    //    }
    //}
}
