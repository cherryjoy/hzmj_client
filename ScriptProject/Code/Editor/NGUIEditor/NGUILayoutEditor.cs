using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;

public class NGUILayoutEditor : EditorWindow {
	
	[MenuItem("NGUI/Layout Editor")]
	static void Init ()
	{
		EditorWindow.GetWindow (typeof(NGUILayoutEditor), true, "Layout Editor");
		
	}
	
	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
	
	}
	
	void OnGUI() {
		
		int OP = 0;
	
		if (GUI.Button(new Rect(0, 0, 80, 30), "align X") == true)
		{
			OP = 1;	
		}
		if (GUI.Button(new Rect(80, 0, 80, 30), "align Y") == true)
		{
			OP = 2;	
		}

		
		if (OP > 0)
		{
			List<GameObject> selects = new List<GameObject>();
			foreach (GameObject o in Selection.gameObjects)
			{
				if (o.GetComponents<UIWidget>().Length >= 0)
				{
					selects.Add(o);
				}
			}
			
			if (selects.Count > 0)
			{
				if (OP == 1)
				{
					for (int i = 1; i < selects.Count; i++)
					{
						Vector3 position = selects[i].transform.position;
						position.x = selects[0].transform.position.x;
						Undo.RegisterCreatedObjectUndo(null, "move");
						selects[i].transform.position = position;
						
					}
				}
				else if (OP == 2)
				{
					for (int i = 1; i < selects.Count; i++)
					{
						Vector3 position = selects[i].transform.position;
						position.y = selects[0].transform.position.y;
                        Undo.RegisterCreatedObjectUndo(null, "move");
						selects[i].transform.position = position;
					}
				}
			}
		}
	}
	
	void ShowInfo()
	{
		/*
		Debug.Log("localposition" + o.transform.localPosition);
		Debug.Log("postion" + o.transform.position);
		UIWidget widget = o.GetComponent<UIWidget>();
		Debug.Log("relativeSize" + widget.relativeSize);
		Debug.Log("pivotOffset" + widget.pivotOffset);
		
		Vector3 pos = Vector3.zero;
		Vector2 size = widget.relativeSize;
		Vector2 offset = widget.pivotOffset;
		pos.x += (offset.x + 0.5f) * size.x;
		pos.y += (offset.y - 0.5f) * size.y;
		
		Debug.Log("translate" + o.transform.localToWorldMatrix.MultiplyPoint3x4(pos));
		*/
	}
}
