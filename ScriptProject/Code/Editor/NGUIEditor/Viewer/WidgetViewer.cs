using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;

class ItemWidget
{
	public GameObject mObj;
	public Rect mRect;
	
	public ItemWidget(GameObject o, Rect r)
	{
		mObj = o;
		mRect = r;
	}
}

public class WidgetViewer : EditorWindow
{
	[MenuItem("NGUI/Widget Viewer")]
	public static void Create()
	{
		WidgetViewer window = GetWindow(typeof(WidgetViewer)) as WidgetViewer;
		window.Show();
	}
	
	Vector2 mScrollView;
	List<ItemWidget> mWidgets = new List<ItemWidget>();
	
	GameObject mSelect;
	
	void OnGUI()
	{
		ShowToolBar();
		if (mLock == false)
			 mSelect = Selection.activeGameObject;
		
		if (mSelect != null)
		{
			UIPanel panel = mSelect.GetComponent<UIPanel>();
			if (panel != null)
			{
				if (Event.current.type == EventType.Repaint)
					mWidgets.Clear();
				
				mScrollView = GUILayout.BeginScrollView(mScrollView);
				ShowWidgets(mSelect, 0);
				ShowCalls();
				GUILayout.EndScrollView();
			}
		}
	}
	
	bool mLock = false;
	
	void ShowToolBar()
	{
		EditorGUILayout.BeginHorizontal();
		mLock = GUILayout.Toggle(mLock, "Lock", GUI.skin.button);
		EditorGUILayout.EndHorizontal();
	}
	
	Vector3[] RectPoints(Rect r)
	{
		Vector3[] points = new Vector3[5];
		points[0].x = r.xMin;
		points[0].y = r.yMin;
		points[1].x = r.xMax;
		points[1].y = r.yMin;
		points[2].x = r.xMax;
		points[2].y = r.yMax;
		points[3].x = r.xMin;
		points[3].y = r.yMax;
		points[4].x = points[0].x;
		points[4].y = points[0].y;
		return points;
	}
	
	void ShowWidgets(GameObject g, int depth)
	{
		GUILayout.BeginHorizontal();
		GUILayout.Space(20 * depth);
		
		if (g.GetComponent<UIButtonMessage>() != null ||
			g.GetComponent<UIImageButton>() != null)
		{
			GUILayout.Label(" ", GUI.skin.button, GUILayout.Width(14), GUILayout.Height(12));
		}
		
		bool bClick = GUILayout.Button(g.name, GUI.skin.label, GUILayout.Width(GUI.skin.label.CalcSize(new GUIContent(g.name)).x));
		if (bClick == true && mLock == true)
			Selection.activeObject = g;
		
		if (Event.current.type == EventType.Repaint)
		{
			Rect rect = GUILayoutUtility.GetLastRect();
			mWidgets.Add(new ItemWidget(g, rect));

			if (g.GetComponent<BoxCollider>() != null)
			{
				Vector3[] points = RectPoints(rect);
				Handles.color = Color.green;
				Handles.DrawPolyLine(points);
			}
		}
		
		string Objz = string.Format("Z:{0:f}", g.transform.localPosition.z);
		GUILayout.Label(Objz, GUILayout.Width(GUI.skin.label.CalcSize(new GUIContent(Objz)).x));
		
		BoxCollider Colz = g.GetComponent<BoxCollider>();
		if (Colz != null)
		{
			Objz = string.Format("Z:{0:f}", Colz.center.z);
			GUILayout.Label(Objz, GUILayout.Width(GUI.skin.label.CalcSize(new GUIContent(Objz)).x));
		}
		
		GUILayout.EndHorizontal();
		
		for (int i = 0; i < g.transform.childCount; i++)
		{
			GameObject c = g.transform.GetChild(i).gameObject;
			ShowWidgets(c, depth + 1);
		}
	}
	
	void ShowCalls()
	{
		if (Event.current.type == EventType.Repaint)
		{
			foreach(ItemWidget widget in mWidgets)
			{
				UIButtonMessage um = widget.mObj.GetComponent<UIButtonMessage>();
				if (um != null)
				{
					System.Type t = um.GetType();
		
					System.Reflection.FieldInfo[] fis = t.GetFields();
					foreach (System.Reflection.FieldInfo f in fis)
					{
						if (f.Name == "target")
						{
							GameObject o = f.GetValue(um) as GameObject;
							
							if (o != null)
							{
								foreach(ItemWidget i in mWidgets)
								{
									if (i.mObj == o)
									{
										Handles.color = Color.red;
										Vector3[] start = RectPoints(widget.mRect);
										Vector3[] end = RectPoints(i.mRect);
										Handles.DrawLine(start[1], end[1]);
									}
								}
							}
						}
					}
				}
			}
		}
	}
	
	void OnSelectionChange()
	{
		Repaint();
	}
}
