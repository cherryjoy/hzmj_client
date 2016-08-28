using System.IO;
using System.Collections.Generic;
using System.Linq;

using UnityEngine;

public static class EditorTools
{
    public static string GetPathForAsset(string fullPath)
    {
        int index = fullPath.IndexOf("Assets/");
        if (index < 0)
        {
            Debug.LogError("Can't parse the path into unity's asset:path ="+fullPath);
            return fullPath;
        }

        return fullPath.Substring(index);
    }

    public static string GetPathForDisk(string assetPath)
    {
        if (assetPath.StartsWith("Assets/"))
        {
            return Application.dataPath + assetPath.Substring("Assets".Length);
        }

        return Application.dataPath + "/" + assetPath;
    }
	
	public static string[] GetAllFilePathForDisk(string dirPath, string searchPattern = "*.*", bool includeChild = true)
	{
		if (!Directory.Exists(dirPath))
        {
            return null;
        }

		List<string> outRes = GetFiles(dirPath, null, searchPattern, includeChild);
		return outRes.ToArray();
	}

	private static List<string> GetFiles(string dirPath, List<string> listInput, string searchPattern = "*.*", bool includeChild = true)
	{
		DirectoryInfo di = new DirectoryInfo(dirPath);
		if (di.Attributes == FileAttributes.Hidden)
			return null;
		
		List<string> outRes = null;
		if (listInput == null)
			outRes = new List<string>();
		else
			outRes = listInput;

        string MetaExtension = ".meta";
		FileInfo[] fileInfo = di.GetFiles(searchPattern, SearchOption.TopDirectoryOnly);
		for (int id = 0; id < fileInfo.Length; ++id)
		{
			if (((fileInfo[id].Attributes & FileAttributes.Hidden) != FileAttributes.Hidden)
				&& (Path.GetExtension(fileInfo[id].Name).ToLower() != MetaExtension))
			{
				outRes.Add(fileInfo[id].FullName.Replace("\\","/"));
			}
		}
		
		if (includeChild == true)
		{
			DirectoryInfo[] dinfo = di.GetDirectories(searchPattern, SearchOption.TopDirectoryOnly);
			for (int i = 0; i < dinfo.Length; i++)
			{
				if ((dinfo[i].Attributes & FileAttributes.Hidden) != FileAttributes.Hidden)
				{
					GetFiles(dinfo[i].FullName, outRes, searchPattern, includeChild);
				}
			}
		}
		return outRes;
	}

    public static bool TriggerButton(string buttonText, float width = 200f)
    {
        GUILayout.BeginHorizontal();
        GUILayout.FlexibleSpace();
        GUI.backgroundColor = Color.red;
        bool isTrigger = GUILayout.Button(buttonText, GUILayout.Width(width));
        GUI.backgroundColor = Color.white;
        GUILayout.FlexibleSpace();
        GUILayout.EndHorizontal();

        return isTrigger;
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

    static public string[] GetTextureFiles(string dir)
    {
        var fileArray = Directory.GetFiles(dir, "*.*", SearchOption.AllDirectories).
            Where(f => f.ToLower().EndsWith("png") || f.ToLower().EndsWith("tga") || f.ToLower().EndsWith("jpg") || f.ToLower().EndsWith("bmp"));

        return fileArray.ToArray<string>();
    }

    static public string[] GetDotUnityFiles(string dir)
    {
        var fileArray = Directory.GetFiles(dir, "*.unity", SearchOption.AllDirectories);

        return fileArray.ToArray<string>();
    }

    static public string[] GetPrefabFiles(string dir)
    {
        var fileArray = Directory.GetFiles(dir, "*.prefab", SearchOption.AllDirectories);

        return fileArray.ToArray<string>();
    }
}
