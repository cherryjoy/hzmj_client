using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;

public class CopyAssetPath
{
	[MenuItem("CJ-TOOL/Copy Asset Path")]
	static void CopyPath()
	{
		if (Selection.activeObject != null)
		{
			string res = "";
			for (int i = 0; i < Selection.objects.Length; i++)
			{
				string assetpath = AssetDatabase.GetAssetPath(Selection.objects[i]);
				// remove Assets/Resources/
				assetpath = assetpath.Substring("Assets/Resources/".Length);
				// remove back fix
				int n = assetpath.LastIndexOf(".");
				if (n != -1)
				{
					assetpath =  assetpath.Substring(0, n);
				}
				res += assetpath;
			}
			
			ClipboardHelper.clipBoard = res;
		}
	}
	
    [MenuItem("CJ-TOOL/Show Selections")]
    static void ShowObject()
    {
        Debug.Log("Select GameOBject: " + Selection.gameObjects.Length);

        int mesh_render = 0;
        HashSet<Material> mats = new HashSet<Material>();
        
        for (int i = 0; i < Selection.gameObjects.Length; i++)
        {
            GameObject g = Selection.gameObjects[i];
            MeshRenderer mr = g.GetComponent<MeshRenderer>();
            if (mr != null)
            {
                mesh_render++;

                for (int j = 0; j < mr.sharedMaterials.Length; j++)
                {
                    if (mr.sharedMaterials[j] != null)
                        mats.Add(mr.sharedMaterials[j]);
                }
            }
        }

        Debug.Log("Mesh Number: " + mesh_render);
        Debug.Log("Material Number: " + mats.Count);
    }
}
