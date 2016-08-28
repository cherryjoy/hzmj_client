using UnityEngine;
using UnityEditor;

using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using System.Text;

public class NDBLocalizationWindow : EditorWindow
{
    //PlayerPrefs saved key
	private static readonly string mNDBRootSuffix = "_NDBRoot";
	private static readonly string mSceneLabelSuffix = "_SceneRoot";
	private static readonly string mTextLabelSuffix = "_TextsFile";

    //asset path in unity
    private readonly string SavedPathForAssets = "Assets/StreamingAssets/Data";
    private readonly string SceneSavedPathForAssets = "Assets/StreamingAssets/Data/Scenes";

    private string mNDBRootPath = "";
    private string mSceneRootPath = "";
    private string mTextPath = "";

    [MenuItem("NDB/NDBWindow")]
    static void Init()
    {
        NDBLocalizationWindow win = EditorWindow.GetWindow<NDBLocalizationWindow>();
        win.title = "NDB Localization";
        win.maxSize = win.minSize = new Vector2(600, 240);
        win.Show();
    }

    #region cache the path

    private string GetPathFromPlayerPrefs(string pathKey)
    {
		string key = string.Format("Localization_{0}", Application.dataPath + pathKey);

        if (EditorPrefs.HasKey(key))
        {
            return EditorPrefs.GetString(key);
        }

        return "";
    }

    private void SavePathToPlayerPrefs(string pathKey, string path)
    {
		EditorPrefs.SetString(string.Format("Localization_{0}", Application.dataPath + pathKey), path);
    }

    private void DeletePathToPlayerPrefs(string pathKey)
    {
		EditorPrefs.DeleteKey(string.Format("Localization_{0}", Application.dataPath + pathKey));
    }

    #endregion

    #region Draw layout

    private void DrawFolderPathFiled(ref string path, string fieldLabel)
    {
        EditorGUILayout.BeginHorizontal();

        EditorGUILayout.LabelField(fieldLabel + ":", path, GUILayout.Width(460));
        if (GUILayout.Button("Select", GUILayout.MaxWidth(120)))
        {
            string tempPath = EditorUtility.OpenFolderPanel(fieldLabel, path, "");
            if (!string.IsNullOrEmpty(tempPath))
            {
                path = tempPath;

                SavePathToPlayerPrefs(fieldLabel, path);
            }
        }
        if (GUILayout.Button("Delete", GUILayout.MaxWidth(120)))
        {
            DeletePathToPlayerPrefs(fieldLabel);
        }

        EditorGUILayout.EndHorizontal();
    }

    private void DrawFilePathField(ref string path, string fieldLabel, string fieldType)
    {
        EditorGUILayout.BeginHorizontal();

        EditorGUILayout.LabelField(fieldLabel + ":", path, GUILayout.Width(460));
        if (GUILayout.Button("Select", GUILayout.MaxWidth(120)))
        {
            string tempPath = EditorUtility.OpenFilePanel(fieldLabel, path, fieldType);
            if (!string.IsNullOrEmpty(tempPath))
            {
                path = tempPath;

                SavePathToPlayerPrefs(fieldLabel, path);
            }
        }

        if (GUILayout.Button("Delete", GUILayout.MaxWidth(120)))
        {
            DeletePathToPlayerPrefs(fieldLabel);
        }

        EditorGUILayout.EndHorizontal();
    }

    private void DrawOperationForField()
	{
		GUILayout.BeginHorizontal();

		if (GUILayout.Button("Import all files", GUILayout.MaxWidth(120)))
		{
			ImportAllFile();
		}

		GUILayout.EndHorizontal();
	}
    #endregion

    void OnGUI()
    {
        GUI.color = new Color(1.0f, 0.6f, 0.3f, 1.0f);

        EditorGUILayout.BeginVertical();
        NGUIEditorTools.DrawHeader("Data Package Tool");

        EditorGUILayout.Space();

        mNDBRootPath = GetPathFromPlayerPrefs(mNDBRootSuffix);
        mSceneRootPath = GetPathFromPlayerPrefs(mSceneLabelSuffix);
        mTextPath = GetPathFromPlayerPrefs(mTextLabelSuffix);

        DrawFolderPathFiled(ref mNDBRootPath, mNDBRootSuffix);
		DrawFolderPathFiled(ref mSceneRootPath, mSceneLabelSuffix);
		DrawFilePathField(ref mTextPath, mTextLabelSuffix, "ndb");

        EditorGUILayout.Space();
        NGUIEditorTools.DrawSeparator();
        EditorGUILayout.Space();

        DrawOperationForField();

        EditorGUILayout.EndVertical();
    }

    void OnProjectChange()
    {
    }

    #region import

    private void ImportAllFile()
    {  
        if (!string.IsNullOrEmpty(mSceneRootPath))
        {
            CopyAllSceneFiles(mSceneRootPath);
        }

        if (!string.IsNullOrEmpty(mNDBRootPath))
        {
            CopyAllNDBAndTBLFiles(mNDBRootPath);
        }

        if (!string.IsNullOrEmpty(mTextPath))
        {
            CopyFile(mTextPath, SavedPathForAssets);

			mTextPath = mTextPath.Replace(".ndb", ".tbl");
			CopyFile(mTextPath, SavedPathForAssets);
		}

		AssetDatabase.Refresh(ImportAssetOptions.ForceUpdate);
	}

    private void CopyAllSceneFiles(string dirPath)
    {
        if (!string.IsNullOrEmpty(dirPath) && Directory.Exists(dirPath))
        {
			Directory.CreateDirectory(SceneSavedPathForAssets);

            string[] files = EditorTools.GetAllFilePathForDisk(dirPath, "*.txt", false);

            if (files == null || files.Length == 0)
            {
                return;
            }

            for (int i = 0, c = files.Length; i < c; i++)
            {
                string file = files[i];
                string fileName = Path.GetFileName(file);

                EditorUtility.DisplayProgressBar("Import Scene", "Importing " + fileName, (float)(i + 1) / c);

                File.Copy(file, EditorTools.GetPathForDisk(SceneSavedPathForAssets + "/" + fileName), true);
            }

			AssetDatabase.Refresh(ImportAssetOptions.ForceUpdate);

			EditorUtility.ClearProgressBar();
        }
    }

    private void CopyAllNDBAndTBLFiles(string dirPath)
	{
		if (string.IsNullOrEmpty(dirPath) || !Directory.Exists(dirPath))
		{
			return;
		}

		// .ndb
		{
			string[] ndbFiles = EditorTools.GetAllFilePathForDisk(dirPath, "*.ndb", true);

			if (ndbFiles == null || ndbFiles.Length == 0)
			{
				return;
			}

			for (int i = 0, c = ndbFiles.Length; i < c; i++)
			{
				string file = ndbFiles[i];
				string fileName = Path.GetFileName(file);

				EditorUtility.DisplayProgressBar("Import NDB", "import " + fileName, (float)(i + 1) / c);

				CopyFile(file, SavedPathForAssets);
			}
		}

		// .tbl
		{
			string[] tblFiles = EditorTools.GetAllFilePathForDisk(dirPath, "*.tbl", true);

			if (tblFiles == null || tblFiles.Length == 0)
			{
				return;
			}

			for (int i = 0, c = tblFiles.Length; i < c; i++)
			{
				string file = tblFiles[i];
				string fileName = Path.GetFileName(file);

				EditorUtility.DisplayProgressBar("Import TBL", "import " + fileName, (float)(i + 1) / c);

				CopyFile(file, SavedPathForAssets);
			}
		}

		AssetDatabase.Refresh(ImportAssetOptions.ForceUpdate);

		EditorUtility.ClearProgressBar();
	}

    #endregion

    #region ndb load

    private void CopyFile(string filePath, string savePath)
    {
		if (!savePath.StartsWith("Assets/"))
		{
			savePath = savePath.Substring(savePath.IndexOf("Assets/"));
		}

		if (savePath.Length > 0)
		{
            string WDBAssetExtension = "";
			savePath += "/" + filePath.Substring(filePath.LastIndexOf("/") + 1);
            savePath = savePath.Replace(".", "_");
            savePath += WDBAssetExtension;
			savePath = EditorTools.GetPathForDisk(savePath);
            Debug.Log(savePath);

			//File.Copy(filePath, savePath, true);
            FileStream fs = new FileStream(filePath, FileMode.Open);
            byte[] data = new byte[(int)fs.Length];
			fs.Read(data, 0, data.Length);
            fs.Close();

            int offset = WDBData.GetOffset(data);
            if (offset == 0)
                Debug.LogError(filePath + " error!");
            else
            {
                FileStream fsout = new FileStream(savePath, FileMode.Create);
                if (offset % 4 == 0)
                {
                    fsout.Write(data, 0, data.Length);
                }
                else
                {
                    fsout.Write(data, 0, offset);
                    fsout.WriteByte(0);
                    fsout.WriteByte(0);
                    fsout.Write(data, offset, data.Length - offset);
                }
                fsout.Close();
            }
		}
    }

    #endregion

    private string SelectedImportFile(string extension)
    {
        string filePath = EditorUtility.OpenFilePanel("Selected File", "", extension);
        if (!string.IsNullOrEmpty(filePath))
        {
            return filePath;
        }

        return null;
    }

}
