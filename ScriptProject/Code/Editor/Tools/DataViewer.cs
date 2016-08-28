using UnityEditor;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;

public class DataViewer : EditorWindow {

	[MenuItem("LK-TOOL/DataViewer")]
	public static void Create()
	{
		DataViewer window = GetWindow(typeof(DataViewer)) as DataViewer;

		window.Show();
	}
	
	private bool displaySearch = false;
	private string searchInput = string.Empty;
	private List<object> searchFiles = new List<object>();
	
	string mSelectFile = "";
	bool mErrorFile = false;
	//AssetBundle mAssetBundle = null;
	string mVersion = "";
	object[] mFiles;
	
	Vector2 mScollView;
	//string path = "";
	//List<int> l1 =  new List<int>();
	//List<int> l2 =  new List<int>();
	//bool check = true;
	void OnGUI()
	{
		GUILayout.Label("Select a pkg file and view");
		
		string file = null;
		GUILayout.BeginHorizontal();
		if (GUILayout.Button("Brower...", GUILayout.Width(80f)) == true)
		{
			mErrorFile = false;
			mFiles = null;
			
			file = EditorUtility.OpenFilePanel("open pkg file", "", "*");
			//path = file.Substring(0, file.LastIndexOf(".")) + @"/";
			if (file.Length != 0)
			{
				WWW www = new WWW("file:///" + file);
				if (www.assetBundle == null)
				{
					mErrorFile = true;
				}
				else
				{
					//mAssetBundle = www.assetBundle;
					//TextAsset ta = www.assetBundle.Load("Data/version") as TextAsset;


						//mVersion = ta.text;
					displaySearch = true;
					mSelectFile = file;
					mFiles = www.assetBundle.LoadAllAssets();
					searchFiles.Clear();
					searchFiles.AddRange(mFiles);
					www.assetBundle.Unload(false);
					//}
				}
			}
		}
		GUILayout.Label(mSelectFile);
		
		if(displaySearch){
			searchInput = GUILayout.TextField(searchInput, GUILayout.Width(150f));
			if(GUILayout.Button("Search", GUILayout.Width(60f))){
				if(!string.IsNullOrEmpty(searchInput)){
					if(mFiles == null){
						return ;
					}
					searchFiles.Clear();
					Regex r = new Regex(string.Format("{0}+", searchInput.ToLower()));
					for(int i = 0,max = mFiles.Length;  i < max; i++){
						string name = string.Empty;
                        if (mFiles[i].GetType() == typeof(TextAsset))
                        {
                            TextAsset ta = mFiles[i] as TextAsset;
                            name = ta.name;
                        }
						if(name.IndexOf(".") > 0){
							name = name.Substring(0, name.Length - 4);
						}
						if(r.IsMatch(name.ToLower())){
							searchFiles.Add(mFiles[i]);
						}
					}
				}else{
					searchFiles.Clear();
					searchFiles.AddRange(mFiles);
				}
			}
		}
		
		GUILayout.EndHorizontal();
		
		if (mErrorFile == true)
		{
			GUILayout.Label("It is not a pkg file!");
			return;
		}

		if (searchFiles != null )
		{
			GUILayout.BeginHorizontal();
			GUILayout.Label("version " + mVersion);
			GUILayout.Label("Total File:" + searchFiles.Count.ToString());
			GUILayout.EndHorizontal();
			
			mScollView = GUILayout.BeginScrollView(mScollView);
			for(int i = 0; i < searchFiles.Count; i++)
			{
				GUILayout.BeginHorizontal();
				GUILayout.Label((i + 1).ToString(), GUILayout.Width(30));
				if (searchFiles[i].GetType() == typeof(TextAsset))
				{
					TextAsset ta = searchFiles[i] as TextAsset;
					if (GUILayout.Button(ta.name) == true)
						Selection.activeObject = ta;
				}
				else
					GUILayout.Label(searchFiles[i].ToString());
				GUILayout.EndHorizontal();
			}
			GUILayout.EndScrollView();
		}
	}
}
