using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System;
using System.Reflection;

public class GameObjectRefFinder : EditorWindow
{
	public static string title = "对象引用查找器";
	public GameObject targetObj;
	private Vector2 mScrollVec = Vector2.zero;
	private const string Description = "选中场景节点，单击Start Find，开始寻找所有引用该节点（包括未激活节点）的组件。寻找范围包括，本场景内所有对象的所有组件的所有public成员(包括内的集合元素)。";
	private bool desFadeOut = false;

	//DATA
	public class FindInfo
	{
		public GameObject target = null;
		public Dictionary<string, List<string>> Component_Filds = new Dictionary<string, List<string>>();//Component - Filds
		public bool isFadeOut = true;
	}
	private List<FindInfo> mFindInfos = new List<FindInfo>();


	private const int MaxRecursionCount = 200;
	private static int mCurRecursionCount = 0;

	[MenuItem("LK-TOOL/对象引用查找器")]
	static void Init()
	{
		EditorWindow win = EditorWindow.GetWindow(typeof(GameObjectRefFinder), false, title);
		win.Show();
		//win.minSize = new Vector2(500f, Screen.height);
	}

	void OnGUI()
	{
		GUILayout.BeginVertical();

		//desFadeOut = EditorGUILayout.Foldout(desFadeOut, "描述");
		//if (desFadeOut)
		//    GUILayout.TextArea(Description);
		//GUILayout.Space(10);

		GUILayout.BeginHorizontal();
		GUILayout.Label("Target to find: ");
		Transform[] selectObj = Selection.GetTransforms(SelectionMode.TopLevel);
		if (selectObj.Length > 0)
			targetObj = selectObj[0].gameObject;
		EditorGUILayout.ObjectField(targetObj, typeof(MonoBehaviour), true);

		GUILayout.Space(10);
		bool startfind = GUILayout.Button("Start Find");

		//查找结果
		GUILayout.EndHorizontal();
		if (startfind)
		{
			mFindInfos.Clear();

			if (targetObj == null)
			{
				GUILayout.Label("Please Choose One GameObject!!!  Thank you");
			}
			else
			{
				Find();
			}
		}

		ShowFindResult();
		GUILayout.EndVertical();
	}

    //find target by script
    public List<FindInfo> FindTarget(GameObject obj) {
        targetObj = obj;
        Find();
        return mFindInfos;
    }

    private void Find()
	{
		IteraorAllGameObjects();
	}

	private void IteraorAllGameObjects()
	{
		//GameObject[] gos = GameObject.FindObjectsOfType<GameObject>();
		GameObject[] gos = Resources.FindObjectsOfTypeAll<GameObject>();
		foreach (var go in gos)
		{
			IteratorAllCompnentsOfGo(go);
		}
	}


	private void IteratorAllCompnentsOfGo(GameObject go)
	{
		Component[] coms = go.GetComponents<Component>();
		foreach (var com in coms)
		{
			if (com == null)
			{
				Debug.LogWarning(go.name + " Miss Component!!!");
				continue;
			}
			IteratorAllPublicPropertysOfComponent(com);
		}
	}

	private void IteratorAllPublicPropertysOfComponent(Component srcComponent)
	{
		if (srcComponent == null)
			return;

		Type comType = srcComponent.GetType();
		FieldInfo[] fields = comType.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);

		foreach (FieldInfo pi in fields)
		{
			bool isRefed = false;

			System.Object obj = pi.GetValue(srcComponent);

			mCurRecursionCount = 0;
			isRefed = isObjRefTargetObjEx(obj);

			if (isRefed)
			{
				AddFindResult(srcComponent.gameObject, comType.ToString(), pi.Name);
			}
		}
	}

	private bool isObjRefTargetObjEx(System.Object obj)
	{
		//限制递归次数
		mCurRecursionCount++;
		if (mCurRecursionCount > MaxRecursionCount)
			return false;

		bool isRef = false;
		//Component或GameObject引用
		if (obj is Component || obj is GameObject)
		{
			if ((obj as Component) != null && (obj as Component).gameObject == targetObj)
				isRef = true;
			else if (obj == targetObj)
				isRef = true;
		}
		//集合内的元素引用
		else if ((obj as IEnumerable<System.Object>) != null)
		{
			IEnumerable<System.Object> ie = obj as IEnumerable<System.Object>;
			IEnumerator<System.Object> enumerator = ie.GetEnumerator();
			while (enumerator.MoveNext())
			{
				isRef = isObjRefTargetObjEx(enumerator.Current);
				if (isRef)
					break;
			}
		}
		else if ((obj as IEnumerable<UnityEngine.GameObject>) != null)
		{
			IEnumerable<UnityEngine.GameObject> ie = obj as IEnumerable<UnityEngine.GameObject>;
			IEnumerator<UnityEngine.GameObject> enumerator = ie.GetEnumerator();
			while (enumerator.MoveNext())
			{
				isRef = isObjRefTargetObjEx(enumerator.Current);
				if (isRef)
					break;
			}
		}
		//自定义类或结构体(非Component或GameObject)中的成员引用
		else if (InstanceIsCumstomClass(obj) || InstanceIsCumstomStruct(obj))
		{
			isRef = IsClassOrStructMemebersRefTargetObj(obj);
		}
		else
		{
			isRef = false;
		}

		return isRef;
	}


	private bool IsClassOrStructMemebersRefTargetObj(System.Object srcObj)
	{
		bool isRef = false;

		Type objType = srcObj.GetType();
		FieldInfo[] fields = objType.GetFields();

		foreach (FieldInfo pi in fields)
		{
			System.Object obj = pi.GetValue(srcObj);

			isRef = isObjRefTargetObjEx(obj);
			if (isRef)
				break;
		}
		return isRef;
	}

	private void AddFindResult(GameObject refGo, string componentName, string refMemeberName)
	{
		FindInfo findInfo = GetFindInfoOf(refGo);
		if (findInfo == null)
		{
			findInfo = new FindInfo();
			findInfo.target = refGo;
			findInfo.Component_Filds.Add(componentName, new List<string>(){refMemeberName});
			mFindInfos.Add(findInfo);
		}
		else
		{
			if (findInfo.Component_Filds.ContainsKey(componentName))
			{
				if (!findInfo.Component_Filds[componentName].Contains(refMemeberName))
				{
					findInfo.Component_Filds[componentName].Add(refMemeberName);
				}
			}
			else
			{
				findInfo.Component_Filds.Add(componentName, new List<string>() { refMemeberName });
			}
		}
	}

	private FindInfo GetFindInfoOf(GameObject refGo)
	{
		FindInfo findInfo = null;
		foreach (FindInfo fi in mFindInfos)
		{
			if (fi.target == refGo)
			{
				findInfo = fi;
			}
		}
		return findInfo;
	}

	private static bool InstanceIsCumstomStruct(System.Object obj)
	{
		if (obj == null)
			return false;

		Type objType = obj.GetType();

		if (!objType.IsPrimitive && !objType.IsEnum && objType.IsValueType)
		{
			return true;
		}
		return false;
	}

	private static bool InstanceIsCumstomClass(System.Object obj)
	{
		if (obj == null)
			return false;

		Type objType = obj.GetType();

		if (objType.IsClass && objType != typeof(string))
		{
			return true;
		}
		return false;
	}

	//=========================================================================
	//ShowResult
	//=========================================================================

	private void ShowFindResult()
	{
		if (mFindInfos.Count <= 0)
		{
			GUILayout.Label("未能找到有引用该对象的组件");
			return;
		}


		mScrollVec = EditorGUILayout.BeginScrollView(mScrollVec);

		for (int i = 0; i < mFindInfos.Count; i++)
		{
			FindInfo fi = mFindInfos[i];

			fi.isFadeOut = EditorGUILayout.Foldout(fi.isFadeOut, "引用" + (i + 1));
			if (fi.isFadeOut)
			{
				//EditorGUILayout.TextField("引用对象层级："+NGUITools.GetHierarchy(fi.target));
				GUILayout.BeginHorizontal();
				GUILayout.Label("引用结点：");
				EditorGUILayout.ObjectField(fi.target, typeof(MonoBehaviour), true, GUILayout.Width(250));
				GUILayout.EndHorizontal();

				foreach (KeyValuePair<string, List<string>> kv in fi.Component_Filds)
				{
					foreach (string v in kv.Value)
					{
						GUILayout.BeginHorizontal();
						GUILayout.Label("引用字段：");
						GUILayout.Label(kv.Key + " / " + v, GUILayout.Width(250));
						GUILayout.EndHorizontal();
					}
				}
			}

			GUILayout.Space(10);
		}

		EditorGUILayout.EndScrollView();
	}
}
