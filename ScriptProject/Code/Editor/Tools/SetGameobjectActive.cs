using UnityEngine;
using System.Collections;
using UnityEditor;
public class SetGameobjectActive : EditorWindow{

	[MenuItem("CJ-TOOL/GameObjManager")]
	static void Init()
	{
		EditorWindow win = EditorWindow.GetWindow(typeof(SetGameobjectActive), false, "GameObject Controller");
		win.minSize = new Vector2(300f, 200f);
	}

	bool isIncludeChild = false;
	bool useSameTool = false;
	GameObject go1;
	GameObject[] go2;
	bool startSame = false;
	//bool worldSame = false;
	bool checkCollider = false;
	void OnGUI()
	{
        
        if (GUILayout.Button("Refresh Prefab"))
        {
            string[] guids = AssetDatabase.FindAssets("t:Prefab", new string[] { "Assets/Resources/UI/Prefab" });
            foreach (string guid in guids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                GameObject obj = AssetDatabase.LoadAssetAtPath(path, typeof(GameObject)) as GameObject;
                //rewrite code down here
                LuaBehaviour[] luaBehavs = obj.GetComponentsInChildren<LuaBehaviour>(true);
                foreach (LuaBehaviour hav in luaBehavs)
                {
                    Debug.Log(hav.name);

                    if (hav.scriptShortPath.Length <= 0)
                        continue;
                    Debug.Log("refresh");
                   
                    hav.scriptShortPath = hav.scriptShortPath.Substring(1, hav.scriptShortPath.Length - 1);
                    TextAsset luaScript = AssetDatabase.LoadAssetAtPath("Assets/StreamingAssets/Script/" + hav.scriptShortPath, typeof(TextAsset)) as TextAsset;
                    if (luaScript != null)
                        hav.ScriptName = luaScript.name;
                    //remember to set dirty
                    EditorUtility.SetDirty(hav);
                }
                //rewrite code here end
            }
            AssetDatabase.SaveAssets();
            Debug.Log("Refresh End.");
        }
		GUILayout.Space(10);
		GUILayout.BeginHorizontal();
		GUILayout.FlexibleSpace();
		useSameTool = GUILayout.Toggle(useSameTool, "节点控制");
		GUILayout.FlexibleSpace();
		GUILayout.EndHorizontal();
		GUILayout.Space(10);
		if (useSameTool)
		{
			TOOL1();
		}
		else
		{
			TOOL2();
		}
	}

	void TOOL2()
	{
		GUILayout.BeginVertical();
		GUILayout.BeginHorizontal();
		GUILayout.BeginVertical();
		if (!startSame && GUILayout.Button("准备校对", GUILayout.Width(100)))
		{
			startSame = true;
		}
		if (startSame && GUILayout.Button("重置", GUILayout.Width(100)))
		{
			startSame = false;
			go1 = null;
		}
		GUILayout.EndVertical();

		
		if (!startSame)
		{
			go1 = Selection.activeGameObject;
		}
		else
		{
			go2 = Selection.gameObjects;
		}
		if (go1 != null)
		{
			GUILayout.BeginVertical();
			GUILayout.TextField("LocalPosition: " + go1.transform.localPosition.ToString());
			GUILayout.TextField("LocalScale	  : " + go1.transform.localScale.ToString());
			GUILayout.EndVertical();
		}
		else
		{
			startSame = false;
		}
		GUILayout.EndHorizontal();
		GUILayout.Space(10);
		if(startSame && go1 != null)
			SubLayout();

        RefreshLight();
		GUILayout.EndVertical();
	}

    void RefreshLight() {
        if (GUILayout.Button("光源强度校准", GUILayout.Width(100)))
        {
            GameObject lightParent = GameObject.Find("light");
            Light[] linghts = lightParent.GetComponentsInChildren<Light>();
            foreach (var light in linghts)
            {
                light.intensity = light.intensity / 2;
                Debug.Log("Light:" + light.name + ",its intensity is change from " + light.intensity * 2 + " to " + light.intensity);
            }

        }
    }

	Vector2 vec = Vector2.zero;
	void SubLayout()
	{
		if (go2 != null)
		{
			//GUILayout.FlexibleSpace();
			SetWidgetAttr(go2);
			SetLabel(go2);
			checkCollider = GUILayout.Toggle(checkCollider, "Scale Collider");
			//GUILayout.FlexibleSpace();
			GUILayout.BeginHorizontal();
			PositionLayout();
			GUILayout.Space(20);
			ScaleLayout();
			GUILayout.EndHorizontal();
			GUILayout.Space(10);
			vec = GUILayout.BeginScrollView(vec);
			for (int i = 0; i < go2.Length; i++)
			{
				GUILayout.BeginHorizontal();
				EditorGUILayout.TextField(NGUITools.GetHierarchy(go2[i]));
				EditorGUILayout.ObjectField(go2[i], typeof(GameObject), true, GUILayout.Width(150));
				GUILayout.EndHorizontal();
				GUILayout.Space(10);
				GUILayout.BeginHorizontal();
				EditorGUILayout.TextField("LocalPosition: " + go2[i].transform.localPosition.ToString());
				EditorGUILayout.TextField("LocalScale   : " + go2[i].transform.localScale.ToString());
				GUILayout.EndHorizontal();
			}
			GUILayout.EndScrollView();
		}
	}

	void SetWidgetAttr(GameObject[] go)
	{
		GUILayout.BeginVertical();

		GUILayout.BeginHorizontal();
		EditorGUILayout.LabelField("设置Povit属性");
		GUILayout.EndHorizontal();
		GUILayout.Space(10);
		GUILayout.BeginHorizontal();
		bool isLeft = GUILayout.Button("Left");
		bool isRight = GUILayout.Button("Right");
		bool isCenter = GUILayout.Button("Center");
		bool isTop = GUILayout.Button("Top");
		bool isBottom = GUILayout.Button("Bottom");
		{
			for (int i = 0; i < go.Length; i++ )
			{
				UIWidget widget = go[i].GetComponent<UIWidget>();
				if (widget != null)
				{
					if (isLeft)
					{
						widget.pivot = UIWidget.Pivot.Left;
					}
					else if (isRight)
					{
						widget.pivot = UIWidget.Pivot.Right;
					}
					else if (isCenter)
					{
						widget.pivot = UIWidget.Pivot.Center;
					}
					else if (isTop)
					{
						widget.pivot = UIWidget.Pivot.Top;
					}
					else if (isBottom)
					{
						widget.pivot = UIWidget.Pivot.Bottom;
					}
				}
			}
		}
		GUILayout.EndHorizontal();

		GUILayout.EndVertical();
		GUILayout.Space(10);
	}

	void SetLabel(GameObject[] go)
	{
		if (go1 != null)
		{
			UILabel src = go1.GetComponent<UILabel>();
			if (src != null)
			{
				GUILayout.BeginVertical();

				GUILayout.BeginHorizontal();
				EditorGUILayout.LabelField("设置Font属性");
				GUILayout.EndHorizontal();
				GUILayout.Space(10);
				GUILayout.BeginHorizontal();

				bool isFontSize = GUILayout.Button("Font Size");
				bool isFontStyle = GUILayout.Button("Font Style");
				bool isFont = GUILayout.Button("Same Font");
				GUILayout.EndHorizontal();

				GUILayout.BeginHorizontal();
				bool isTopCol = GUILayout.Button("Top Color");
				bool isBottomCol = GUILayout.Button("Bottom Color");
				bool isTint = GUILayout.Button("Tint Color");
				GUILayout.EndHorizontal();
				{
					for (int i = 0; i < go.Length; i++)
					{
						UILabel des = go[i].GetComponent<UILabel>();
						if (des != null)
						{
							if (isFontSize)
							{
								des.FontSize = src.FontSize;
							}
							else if (isFontStyle)
							{
								des.FontStyle = src.FontStyle;
							}
							else if (isTopCol)
							{
								des.TopColor = src.TopColor;
							}
							else if (isBottomCol)
							{
								des.BottomColor = src.BottomColor;
							}
							else if (isTint)
							{
								des.color = src.color;
							}
							else if (isFont){
								des.TrueTypeFont = src.TrueTypeFont;
							}
						}
					}
				}
				

				GUILayout.EndVertical();
				GUILayout.Space(10);
			}
		}
	}

	void PositionLayout()
	{
		GUILayout.BeginVertical();
		if (GUILayout.Button("Position All"))
		{
			for (int i = 0; i < go2.Length; i++)
			{
				if (checkCollider)
				{
					BoxCollider collider1 = go1.GetComponent<BoxCollider>();
					if (collider1 != null)
					{
						BoxCollider collider = go2[i].GetComponent<BoxCollider>();
						collider.center = collider1.center;
					}
				}
				else
				{
					go2[i].transform.localPosition = go1.transform.localPosition;
				}
			}
			
		}

		if (GUILayout.Button("Position X"))
		{
			for (int i = 0; i < go2.Length; i++)
			{
				if (checkCollider)
				{
					BoxCollider collider1 = go1.GetComponent<BoxCollider>();
					if (collider1 != null)
					{
						BoxCollider collider = go2[i].GetComponent<BoxCollider>();
						collider.center = new Vector3(collider1.center.x, collider.center.y, collider.center.z);
					}
				}
				else
				{
					Vector3 selfPos = go2[i].transform.localPosition;
					go2[i].transform.localPosition = new Vector3(go1.transform.localPosition.x, selfPos.y, selfPos.z);
				}
			}
		}

		if (GUILayout.Button("Position Y"))
		{
			for (int i = 0; i < go2.Length; i++)
			{
				if (checkCollider)
				{
					BoxCollider collider1 = go1.GetComponent<BoxCollider>();
					if (collider1 != null)
					{
						BoxCollider collider = go2[i].GetComponent<BoxCollider>();
						collider.center = new Vector3(collider.center.x, collider1.center.y, collider.center.z);
					}
				}
				else
				{
					Vector3 selfPos = go2[i].transform.localPosition;
					go2[i].transform.localPosition = new Vector3(selfPos.x, go1.transform.localPosition.y, selfPos.z);
				}
			}
		}

		if (GUILayout.Button("Position Z"))
		{
			for (int i = 0; i < go2.Length; i++)
			{
				if (checkCollider)
				{
					BoxCollider collider1 = go1.GetComponent<BoxCollider>();
					if (collider1 != null)
					{
						BoxCollider collider = go2[i].GetComponent<BoxCollider>();
						collider.center = new Vector3(collider.center.x, collider.center.y, collider1.center.z);
					}
				}
				else
				{
					Vector3 selfPos = go2[i].transform.localPosition;
					go2[i].transform.localPosition = new Vector3(selfPos.x, selfPos.y, go1.transform.localPosition.z);
				}
			}
		}
		GUILayout.EndVertical();
	}

	void ScaleLayout()
	{
		GUILayout.BeginVertical();
		if (GUILayout.Button("Scale All"))
		{
			for (int i = 0; i < go2.Length; i++)
			{
				if (checkCollider)
				{
					BoxCollider collider1 = go1.GetComponent<BoxCollider>();
					if (collider1 != null)
					{
						BoxCollider collider = go2[i].GetComponent<BoxCollider>();
						collider.size = collider1.size;
					}
				} 
				else
				{
					go2[i].transform.localScale = go1.transform.localScale;
				}
			}
		}

		if (GUILayout.Button("Scale X"))
		{
			for (int i = 0; i < go2.Length; i++)
			{
				if (checkCollider)
				{
					BoxCollider collider1 = go1.GetComponent<BoxCollider>();
					if (collider1 != null)
					{
						BoxCollider collider = go2[i].GetComponent<BoxCollider>();
						collider.size = new Vector3(collider1.size.x, collider.size.y, collider.size.z);
					}
				}
				else
				{
					Vector3 selfScale = go2[i].transform.localScale;
					go2[i].transform.localScale = new Vector3(go1.transform.localScale.x, selfScale.y, selfScale.z);
				}
			}
		}

		if (GUILayout.Button("Scale Y"))
		{
			for (int i = 0; i < go2.Length; i++)
			{
				if (checkCollider)
				{
					BoxCollider collider1 = go1.GetComponent<BoxCollider>();
					if (collider1 != null)
					{
						BoxCollider collider = go2[i].GetComponent<BoxCollider>();
						collider.size = new Vector3(collider.size.x, collider1.size.y, collider.size.z);
					}
				}
				else
				{
					Vector3 selfScale = go2[i].transform.localScale;
					go2[i].transform.localScale = new Vector3(selfScale.x, go1.transform.localScale.y, selfScale.z);
				}
			}
		}

		if (GUILayout.Button("Scale Z"))
		{
			for (int i = 0; i < go2.Length; i++)
			{
				if (checkCollider)
				{
					BoxCollider collider1 = go1.GetComponent<BoxCollider>();
					if (collider1 != null)
					{
						BoxCollider collider = go2[i].GetComponent<BoxCollider>();
						collider.size = new Vector3(collider.size.x, collider.size.y, collider1.size.z);
					}
				}
				else
				{
					Vector3 selfScale = go2[i].transform.localScale;
					go2[i].transform.localScale = new Vector3(selfScale.x, selfScale.y, go1.transform.localScale.z);
				}
			}
		}
		GUILayout.EndVertical();
	}

	void TOOL1()
	{
		GUILayout.BeginVertical();
		//GUILayout.FlexibleSpace();
		GUILayout.BeginHorizontal();
		if (GUILayout.Button("Create GameObject"))
		{
			Transform trans = Selection.activeTransform;
			if (trans != null)
			{
				GameObject go = new GameObject("GameObject");
				go.transform.parent = trans;
				go.transform.localScale = new Vector3(1.0f, 1.0f, 1.0f);
				go.transform.localPosition = new Vector3(0f, 0f, 0f);
				go.layer = trans.gameObject.layer;
				Selection.activeGameObject = go;
			}
		}

		if (GUILayout.Button("Create Label"))
		{
			GameObject go = Selection.activeGameObject;
			if (go != null)
			{
				UILabel lbl = NGUITools.AddWidget<UILabel>(go);
				lbl.font = UISettings.font;
				lbl.text = "Loading...";
                lbl.color = new Color(212f / 255f, 228f / 255f, 238f / 255f); ;
                lbl.Dimensions = new Vector2(24, 24);
				lbl.MakePixelPerfect();
			}
		}
		GUILayout.EndHorizontal();
		GUILayout.Space(10);
		GUILayout.BeginHorizontal();
		if (GUILayout.Button("Add UILineFlowLayout"))
		{
			Selection.activeGameObject.AddComponent<UILineFlowLayout>();
		}
		if (GUILayout.Button("Add DragObject"))
		{
			GameObject[] objs = Selection.gameObjects;
			if (objs.Length > 0)
			{
				Transform parent = objs[0].transform.parent;
				for (int i = 0; i < objs.Length; i++)
				{
					UIDragObject drag = objs[i].GetComponent<UIDragObject>();
					if (drag == null)
					{
						drag = objs[i].AddComponent<UIDragObject>();
					}
					drag.target = parent;
				}
				//UILineFlowLayout lineFlow = objs[0].transform.parent.GetComponent<UILineFlowLayout>();
				//if (lineFlow != null)
				//{
				//    for (int i = 0; i < objs.Length; i++)
				//    {
				//        UILineFlowLayoutChild child = new UILineFlowLayoutChild(objs[i].transform);
				//        lineFlow.childrens.Add(child);
				//    }
				//}
			}
		}
		GUILayout.EndHorizontal();

// 		if (GUILayout.Button("Remove LocalizationLabelText"))
// 		{
// 			GameObject go = Selection.activeGameObject;
// 			LocalizationLabelText localization = go.GetComponent<LocalizationLabelText>();
// 			if (go != null)
// 			{
// 				DestroyImmediate(localization);
// 				go.GetComponent<UILabel>().text = "";
// 				go.SetActive(false);
// 				go.SetActive(true);
// 			}
// 		}
		GUILayout.Space(10);
		isIncludeChild = GUILayout.Toggle(isIncludeChild, "包含所有孩子节点");
		GUILayout.Space(10);
		bool makeActive = GUILayout.Button("激活");
		if (makeActive)
		{
			Transform[] tran = Selection.transforms;
			if (isIncludeChild)
			{
				for (int i = 0; i < tran.Length; i++)
					activeChild(tran[i], true);
			}
			else
			{
				for (int i = 0; i < tran.Length; i++)
					tran[i].gameObject.SetActive(true);
			}
		}
		GUILayout.Space(10);
		bool makeUnActive = GUILayout.Button("取消激活");
		if (makeUnActive)
		{
			Transform[] tran = Selection.transforms;
			if (isIncludeChild)
			{
				for (int i = 0; i < tran.Length; i++)
					activeChild(tran[i], false);
			}
			else
			{
				for (int i = 0; i < tran.Length; i++)
					tran[i].gameObject.SetActive(false);
			}
		}
		GUILayout.Space(15);
		GUI.color = Color.red;
		bool quikRemove = GUILayout.Button("一键移除");
		GUI.color = Color.white;
		if (quikRemove)
		{
			Transform[] tran = Selection.transforms;
			for (int i = 0; i < tran.Length; i++)
			{
				GameObject.DestroyImmediate(tran[i].gameObject);
			}
		}
		//GUILayout.FlexibleSpace();
		GUILayout.EndVertical();
	}

	void activeChild(Transform tran, bool active)
	{
		tran.gameObject.SetActive(active);
		int childCount = tran.childCount;
		if(childCount > 0)
		{
			for (int i = 0; i < childCount; i++)
			{
				activeChild(tran.GetChild(i), active);
			}
			
		}
	}
}
