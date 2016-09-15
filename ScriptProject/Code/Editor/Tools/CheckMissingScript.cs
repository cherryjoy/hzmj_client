using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.IO;

public class CheckMissingScript : EditorWindow
{
	[MenuItem("CJ-TOOL/CheckMissingScript")]
	static void Init()
	{
		EditorWindow.GetWindow(typeof(CheckMissingScript));
	}

	private Vector2 mScrollPosition;
	private HashSet<string> mNeedCheckGoSet = new HashSet<string>();

	void OnGUI()
	{
		if (EditorTools.TriggerButton("BeginCheck"))
		{
			Check();
		}

		mScrollPosition = GUILayout.BeginScrollView(mScrollPosition);
		foreach (string file in mNeedCheckGoSet)
		{
			EditorGUILayout.TextField(file);
		}
		GUILayout.EndScrollView();
	}

	void OnDestroy()
	{
		mNeedCheckGoSet.Clear();
	}

	private bool Check(bool needSuccessNotify = true)
	{
		OnDestroy();

		bool isSuccessful = true;

		string dir = EditorUtility.OpenFolderPanel("Choose need checked folder", EditorTools.GetPathForDisk("Resources"), string.Empty);
		{
			string[] dotUnityFiles = EditorTools.GetDotUnityFiles(dir);
			foreach (string file in dotUnityFiles)
			{
				string scenePath = EditorTools.GetPathForAsset(file);
				EditorApplication.OpenScene(scenePath);

				GameObject[] goArray = GameObject.FindObjectsOfType(typeof(GameObject)) as GameObject[];
				isSuccessful = Check_Impl(goArray, Path.GetFileName(scenePath) + ": ");
			}

			EditorApplication.NewScene();
			{
				List<GameObject> goList = new List<GameObject>();

				string[] prefabFiles = EditorTools.GetPrefabFiles(dir);
				foreach (string file in prefabFiles)
				{
					string prefabPath = EditorTools.GetPathForAsset(file);

					GameObject go = AssetDatabase.LoadAssetAtPath(prefabPath, typeof(GameObject)) as GameObject;
					if (go != null)
					{
						Transform[] transArray = go.GetComponentsInChildren<Transform>(true);
						foreach (Transform trans in transArray)
						{
							goList.Add(trans.gameObject);
						}						
					}
				}

				isSuccessful = Check_Impl(goList.ToArray(), "Prefab: ");
			}

			Resources.UnloadUnusedAssets();
		}

		if (isSuccessful && needSuccessNotify)
		{
			EditorUtility.DisplayDialog("Missing script check Success!", "Great!", "ok");
			return true;
		}
		else if (!isSuccessful)
		{
			EditorUtility.DisplayDialog("Missing script check Failed!", "You need to Fix below as shown!", "ok");
			return true;
		}

		return false;
	}

	private bool Check_Impl(GameObject[] goArray, string prefixName)
	{
		bool isSuccessful = true;

		foreach (GameObject go in goArray)
		{
			MonoBehaviour[] compArray = go.GetComponents<MonoBehaviour>();
			foreach (MonoBehaviour comp in compArray)
			{
				if (comp == null)
				{
					string goPath = prefixName + EditorTools.GetHierarchy(go);
					mNeedCheckGoSet.Add(goPath);

					isSuccessful = false;
				}
			}
		}

		return isSuccessful;
	}
}
