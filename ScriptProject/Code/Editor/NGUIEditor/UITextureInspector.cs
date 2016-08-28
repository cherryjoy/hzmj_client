//----------------------------------------------
//            NGUI: Next-Gen UI kit
// Copyright © 2011-2012 Tasharen Entertainment
//----------------------------------------------
using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

/// <summary>
/// Inspector class used to edit UITextures.
/// </summary>

[CustomEditor(typeof(UITexture))]
public class UITextureInspector : UIWidgetInspector
{
    protected UITexture mTexture;
    private static string NONE = "None";
    private string pathSelection = NONE;
    private string textureSelection = NONE;

    private string UITextureParentPath = "/Resources/Textures/UITexture/";
    private string UITextureParentPathWithoutRes = "Textures/UITexture/";
    private string[] FolderList;
    private string[] TextureList;

    override protected  void OnInit()
    {
        mTexture = mWidget as UITexture;
        if (!string.IsNullOrEmpty(mTexture.MaterialPath) && mTexture.MaterialPath.StartsWith("Textures/UITexture/"))
        {
            mTexture.MaterialPath = mTexture.MaterialPath.Replace('\\', '/');
            string[] splits = mTexture.MaterialPath.Split('/');
            if (splits != null && splits.Length == 4)
            {
                pathSelection = splits[2];
                textureSelection = splits[3];

                if (mTexture.material == null)
                {
                    Shader s = Shader.Find("Unlit/Transparent Colored");
                    mTexture.material = new Material(s);
                }
                Resources.UnloadAsset(mTexture.material.mainTexture);
                mTexture.material.mainTexture = mTexture.AysnLoadTex(mTexture.MaterialPath) as Texture;
            }
        }

        FolderList = NGUIEditorTools.GetSonDirectoryNameWithoutPath(UITextureParentPath);
        InsertOneAtHead<string>(ref FolderList, "None");
        TextureList = NGUIEditorTools.GetFilesInDirectoryWithoutPathAndExtension(UITextureParentPath + pathSelection, ".png");
        InsertOneAtHead<string>(ref TextureList, "None");
    }

	override protected bool OnDrawProperties ()
	{
        ///////////////////////////////////////////////////////////////////////////////
        ////////////////////////////// New ///////////////////////////////////////////
        mTexture = mWidget as UITexture;
        mTexture.UVOffset = EditorGUILayout.Vector4Field("UV偏移", mTexture.UVOffset);
        mTexture.MarkAsChanged();
        EditorGUILayout.LabelField("TexturePath", mTexture.MaterialPath);
        string path = NGUIEditorTools.DrawList("Panel", FolderList, pathSelection);
        if (path != pathSelection)
        {
            pathSelection = path;
            TextureList = NGUIEditorTools.GetFilesInDirectoryWithoutPathAndExtension(UITextureParentPath + pathSelection, ".png");
        }
        string texture = NGUIEditorTools.DrawList("Texture", TextureList, textureSelection);
        if (texture != textureSelection)
        {
            if (texture != NONE)
            {
                textureSelection = texture;
                mTexture.MaterialPath = UITextureParentPathWithoutRes + pathSelection + "/" + textureSelection;
                mTexture.MaterialPath.Replace('\\', '/');
                if (mTexture.material == null)
                {
                    Shader s = Shader.Find("Unlit/Transparent Colored");
                    mTexture.material = new Material(s);
                }
                Resources.UnloadAsset(mTexture.material.mainTexture);
                Material tmpMat = new Material(mTexture.material);
                tmpMat.mainTexture = mTexture.AysnLoadTex(mTexture.MaterialPath) as Texture;
                mTexture.material = tmpMat;
                EditorUtility.SetDirty(mWidget);
            }
        }

        //////////////////////////////////////////////////////////////////////////////////////////////
        ////////////////////// Before ////////////////////////////////////////////////////////////////
		if(mWidget.IsHidePanel != EditorGUILayout.Toggle("Hide Panel", mWidget.IsHidePanel))
		{
			mWidget.IsHidePanel = !mWidget.IsHidePanel;
			EditorUtility.SetDirty(mWidget);
			EditorWindow.FocusWindowIfItsOpen<UIPanelTool>();			
		}
		
		Material mat = EditorGUILayout.ObjectField("Material", mWidget.material, typeof(Material), false) as Material;

		if (mWidget.material != mat)
		{
			NGUIEditorTools.RegisterUndo("Material Selection", mWidget);
			mWidget.material = mat;
		}
		return true;
	}

    public static void  InsertOneAtHead<T>(ref T[] array , T element)
    {
        if (array== null)
        {
            array =  new T[]{element};
        }
        T[] result = new T[array.Length + 1];
        result[0] = element;
        for (int i = 1; i < result.Length; ++i )
        {
            result[i] = array[i - 1];
        }
        array = result;
    }

	override protected void OnDrawTexture ()
	{
		Texture2D tex = mWidget.mainTexture as Texture2D;
		if (tex != null)
		{
			// Draw the atlas
			EditorGUILayout.Separator();
			NGUIEditorTools.DrawSprite(tex, new Rect(0f, 0f, 1f, 1f), null);

			// Sprite size label
			Rect rect = GUILayoutUtility.GetRect(Screen.width, 18f);
			EditorGUI.DropShadowLabel(rect, "Texture Size: " + tex.width + "x" + tex.height);
		}
	}
}