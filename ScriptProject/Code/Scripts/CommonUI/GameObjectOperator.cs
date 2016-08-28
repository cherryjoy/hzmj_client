using UnityEngine;
using System.Collections.Generic;
using System;

public delegate void CommonEndCallBack(object obj);

public static class GameObjectOperator
{
	static public GameObject AddChild(GameObject parent)
	{
		GameObject go = new GameObject();

		if (parent != null)
		{
			Transform t = go.transform;
			t.parent = parent.transform;
			go.layer = parent.layer;
			t.localPosition = Vector3.zero;
			t.localRotation = Quaternion.identity;
			t.localScale = Vector3.one;
		}
		return go;
	}

	static public GameObject AddChild(GameObject parent, GameObject prefab)
	{
		GameObject go = GameObject.Instantiate(prefab) as GameObject;

		if (go != null && parent != null)
		{
			SetChildLayer(go.transform, parent.layer);

			Transform t = go.transform;
			t.parent = parent.transform;
			t.localPosition = Vector3.zero;
			t.localRotation = Quaternion.identity;
			t.localScale = Vector3.one;
		}
		return go;
	}

	static void SetChildLayer(Transform t, int layer)
	{
		for (int i = 0; i < t.childCount; ++i)
		{
			Transform child = t.GetChild(i);

			if (child.GetComponent<UIPanel>() == null)
			{
				child.gameObject.layer = layer;
				SetChildLayer(child, layer);
			}
		}
	}

	static public T AddChild<T>(GameObject parent) where T : Component
	{
		GameObject go = AddChild(parent);
		go.name = GetName<T>();
		return go.AddComponent<T>();
	}

	static public string GetHierarchy(GameObject obj)
	{
		string path = obj.name;

		while (obj.transform.parent != null)
		{
			obj = obj.transform.parent.gameObject;
			path = obj.name + "/" + path;
		}
		return "\"" + path + "\"";
	}

	static private string GetName<T>() where T : Component
	{
		string s = typeof(T).ToString();
		if (s.StartsWith("UI")) s = s.Substring(2);
		else if (s.StartsWith("UnityEngine.")) s = s.Substring(12);
		return s;
	}

	/// <summary>
	/// 递归获取GameObject的所有子节点
	/// </summary>
	/// <param name="father"></param>
	/// <returns></returns>
	public static List<GameObject> GetAllChildGameObject(GameObject father)
	{
		List<GameObject> childList = new List<GameObject>();
		Transform fatherTran = father.transform;
		int childCount = fatherTran.childCount;
		for (int i = 0; i < childCount; i++)
		{
			Transform childTran = fatherTran.GetChild(i);
			childList.Add(childTran.gameObject);
			if (childTran.childCount > 0)
			{
				List<GameObject> list = GetAllChildGameObject(childTran.gameObject);
				for (int index = 0; index < list.Count; ++index)
				{
					childList.Add(list[index]);
				}
			}
		}
		return childList;
	}

	public static List<GameObject> GetAllChildGameObjectIncludeParent(GameObject father)
	{
		List<GameObject> childList = GetAllChildGameObject(father);
		childList.Add(father);
		return childList;
	}

	/// <summary>
	/// Add Common Bag Without Close Btn And Background 
	/// </summary>
	/// <param name="parent">bag transform parent</param>
	/// <param name="bagType">Bag Controller Component Often Inherit From EquipBaseBagController(You Need To Overrid Two Action Functions)</param>
	/// <returns></returns>
	public static GameObject AddHeroCommonBag(GameObject parent, System.Type bagType)
	{
		string baseBagePrefabPath = "HeroCommon/CommonBag";
		GameObject BaseBagPrefab = (GameObject)ResLoader.Load(UIPrefabDummy.GetUIPrefabPath(baseBagePrefabPath), typeof(GameObject));
		GameObject bag = NGUITools.AddChildNotLoseScale(parent, BaseBagPrefab);
		bag.AddComponent(bagType);
		return bag;
	}


	public static T GetComponentInChildren<T>(GameObject parent) where T : Component
	{
		Transform fatherTran = parent.transform;
		int childCount = fatherTran.childCount;
		for (int i = 0; i < childCount; i++)
		{
			Transform childTran = fatherTran.GetChild(i);
			if (childTran.GetComponent(typeof(T)) != null)
			{
				return childTran.GetComponent(typeof(T)) as T;
			}
			else
			{
				Component t = GameObjectOperator.GetComponentInChildren<T>(childTran.gameObject);
				if (t != null)
				{
					return t as T;
				}
			}
		}
		return null;
	}

	public static T[] GetComponentsInChildren<T>(GameObject parent, bool includeDisactive = false) where T : Component
	{
		List<T> cops = new List<T>();
		List<GameObject> childList = GetAllChildGameObject(parent);
		for (int i = 0; i < childList.Count; i++)
		{
			if (!childList[i].activeInHierarchy && !includeDisactive)
				continue;

			T cop = childList[i].GetComponent(typeof(T)) as T;
			if (cop != null)
			{
				cops.Add(cop);
			}
		}
		T[] result = cops.ToArray();
		return result;
	}

	public static T GetOnlyCompnentInChildren<T>(GameObject go) where T : Component
	{
		T comp = null;

		T[] compArray = go.GetComponentsInChildren<T>(true);
		if (compArray.Length == 1)
			comp = compArray[0];
		else if (compArray.Length > 1)
			Debug.LogError("This gameobject does not contain only one component! Check it if you really want this result.");

		return comp;
	}

	public static T GetOrAddComponent<T>(GameObject go) where T : Component
	{
		Component comp = go.GetComponent(typeof(T));
		if (comp == null)
		{
			comp = go.AddComponent(typeof(T));
		}
		return comp as T;
	}

	public static void SetGameObjectActive(GameObject go)
	{
		if (go == null)
		{
			return;
		}
		if (!(go.activeSelf && go.activeInHierarchy))
		{
			go.SetActive(true);
		}
	}

	public static bool CheckIfOutOfScreen(GameObject go)
	{
		float width = Screen.width;
		Camera uiCamer = NGUITools.FindCameraForLayer(go.layer);
		Vector3 left = uiCamer.ScreenToWorldPoint(Vector3.zero);
		Vector3 right = uiCamer.ScreenToWorldPoint(new Vector3(width, 0, 0));
		BoxCollider collider = NGUITools.AddWidgetCollider(go);
		Bounds bound = collider.bounds;
		GameObject.Destroy(collider);
		if (bound.center.x - bound.extents.x < left.x || bound.center.x + bound.extents.x > right.x)
		{
			return true;
		}
		return false;
	}

	public static void SetGameObjectActive(Component cop)
	{
		if (cop == null)
		{
			return;
		}

		SetGameObjectActive(cop.gameObject);
	}

	public static void SetGameObjectUnActive(GameObject go)
	{
		if (go == null)
		{
			return;
		}
		if ((go.activeSelf && go.activeInHierarchy))
		{
			go.SetActive(false);
		}
	}

	public static void SetGameObjectUnActive(Component cop)
	{
		if (cop == null)
		{
			return;
		}

		SetGameObjectUnActive(cop.gameObject);
	}

	public static Vector3 GetWordScale(Component cop)
	{
		if (cop == null || cop.gameObject == null)
		{
			return Vector3.one;
		}

		Vector3 worldScale = cop.transform.localScale;
		Transform parent = cop.transform.parent;
		while (parent != null && parent.parent != null)
		{
			worldScale = Vector3.Scale(worldScale, parent.localScale);
			parent = parent.parent;
		}
		return worldScale;
	}

	public static void PlayGemCombineEffect(GameObject parent, Vector2 scale)
	{
		GameObject gPrefab = (GameObject)ResLoader.Load("Prefab/UI/GemMergeUIAnim", typeof(GameObject));
		GameObject gObject = NGUITools.AddChildNotLoseAnything(parent, gPrefab);
		gObject.transform.localPosition = Vector3.zero;
		gObject.transform.localScale = Vector3.one;
		UISpriteAnimation animation = gObject.GetComponentInChildren<UISpriteAnimation>();
		animation.transform.localScale = scale;
		animation.Scale = scale;
	}

	public static void DragObjectConnectToTarget(UIDragObject dragObject, GameObject dragTarget, float fScale = 1f, bool bVertical = true, bool bBackToTop = true, bool bRestricInPanel = true)
	{
		dragObject.target = dragTarget.transform;
		if (bVertical)
		{
			dragObject.scale = new Vector3(0, fScale, 0);
		}
		else
		{
			dragObject.scale = new Vector3(fScale, 0, 0);
		}
		dragObject.backTop = bBackToTop;
		dragObject.restrictWithinPanel = bRestricInPanel;
	}

	public static void DragObjectConnectToTarget(GameObject dragTarget, GameObject dragObject)
	{
		UIDragObject drag = GetOrAddComponent<UIDragObject>(dragObject);
		DragObjectConnectToTarget(drag, dragTarget);
	}

	public static Vector3 GetScreenSize()
	{
		Camera cam = UIRoot.mSelf.mCamera;
		Transform root = UIRoot.mSelf.transform;

		Vector3 vSize = new Vector3(960.0f, 640.0f, 0.0f);
		Vector3 vCache = vSize;

		vSize.x *= root.localScale.x;
		vSize.y *= root.localScale.y;
		vSize = cam.WorldToScreenPoint(vSize);
		vSize.x = vCache.x * vCache.x / 960 * Screen.width / (vSize.x - Screen.width * 0.5f);
		vSize.y = vCache.y * vCache.y / 640 * Screen.height / (vSize.y - Screen.height * 0.5f);
		return vSize;
	}

	/// <summary>
	/// Add Common Bag With Close Btn And Background 
	/// </summary>
	/// <param name="parent">bag transform parent</param>
	/// <param name="bagType">Bag Controller Component Often Inherit From EquipBaseBagController(You Need To Overrid Two Action Functions)</param>
	/// <returns></returns>
	public static GameObject MakeCommonBackBag(GameObject parent, Type bagType)
	{
		string BaseBagePrefabPath = "HeroCommon/CommonBackCloseBag";
		GameObject BaseBagPrefab = (GameObject)ResLoader.Load(UIPrefabDummy.GetUIPrefabPath(BaseBagePrefabPath), typeof(GameObject));
		GameObject bag = NGUITools.AddChildNotLoseScale(parent.transform.parent.gameObject, BaseBagPrefab);
		//BaseBagLoader.BagType = bagType;
		bag.transform.localPosition += new UnityEngine.Vector3(0, 0, -30);
		//bag.AddComponent<BaseBagLoader>();
		//RealCloseIfNotTouchThis rc = bag.GetComponent<RealCloseIfNotTouchThis>();
		//if (rc == null)
		//{
		//    bag.AddComponent<RealCloseIfNotTouchThis>();
		//}
		return bag;
	}

	public static float GetScreenScale()
	{
		Vector3 screenSize = GetScreenSize();
		float rateNow = screenSize.x / screenSize.y;
		float rateOrg = 960f / 640f;
		return rateOrg / rateNow;
	}

	public static T[] CombieArray<T>(T[] first, T[] second)
	{
		int count = 0;
		if (first != null)
		{
			count += first.Length;
		}
		if (second != null)
		{
			count += second.Length;
		}
		T[] result = new T[count];
		if (first != null)
		{
			for (int i = 0; i < first.Length; i++)
			{
				result[i] = first[i];
			}
		}
		if (second != null)
		{
			int length = second.Length;
			int startIndex = count - length;
			for (int i = 0; i < length; i++)
			{
				result[startIndex + i] = second[i];
			}
		}
		return result;
	}

	public static T Instantiate<T>(string path) where T: UnityEngine.Object
	{
        T obj = null;

        if (!string.IsNullOrEmpty(path))
        {
            UnityEngine.Object res = ResLoader.Load(path);
            if (res == null)
            {
                LKDebug.LogError("Can't find :" + path);
                return null;
            }
            obj = GameObject.Instantiate(res) as T;
        }

		return obj;
	}
}
