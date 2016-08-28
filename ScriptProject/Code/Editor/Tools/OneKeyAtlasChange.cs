using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;

[InitializeOnLoad]
public class OneKeyAtlasChange:EditorWindow {

	[MenuItem("LK-TOOL/一键切换图集",
	          false,
	          1)]
	static void OpenSceneOpener(){
        EditorWindow windows = EditorWindow.GetWindow<OneKeyAtlasChange>(true, "一键切换图集");
        windows.position = new Rect(Screen.currentResolution.width/2-300, Screen.currentResolution.height/2-200, 600, 400);
        windows.autoRepaintOnSceneChange = true;
	}


    UIAtlas oldAtlas;
    UIAtlas newAtlas;
	void Awake(){
		//mCurInput = EditorPrefs.GetString(LASTINPUTKEY);
        GameObject atlasGameObject = (GameObject)ResLoader.Load("UI/Atlas/Bit32/BaseUI", typeof(GameObject));
        oldAtlas = atlasGameObject.GetComponent<UIAtlas>();

        atlasGameObject = (GameObject)ResLoader.Load("UI/Atlas/Bit32/base", typeof(GameObject));
        newAtlas = atlasGameObject.GetComponent<UIAtlas>();
	}

    void OnGUI()
    {
        GUILayout.BeginHorizontal();
        {
            EditorGUILayout.LabelField("原图集 ");
            EditorGUILayout.LabelField("新图集 ");
        }
        GUILayout.EndHorizontal();
        GUILayout.BeginHorizontal();
        {
            ComponentSelector.Draw<UIAtlas>(oldAtlas, OnSelectOldAtlas);
            ComponentSelector.Draw<UIAtlas>(newAtlas, OnSelectNewAtlas);
        }
        GUILayout.EndHorizontal();

        if (GUILayout.Button("一键替换"))
        {
            GameObject beChangeObj = Selection.activeGameObject;
            if (beChangeObj == null)
            {
                Debug.Log("没有选中任何东西，不能开始替换。");
            }
            int j = 0;
            UISprite[] sprs = beChangeObj.GetComponentsInChildren<UISprite>(true);
            foreach (UISprite spr in sprs)
            {
                if (spr.atlas == oldAtlas)
                {
                    j++;
                    spr.atlas = newAtlas;
                    Debug.Log(spr.name + "的atals被替换成功了。");
                }

                EditorUtility.SetDirty(spr);
            }

            Debug.Log(j + "个Sprite的atlas被替换了。");

            AssetDatabase.SaveAssets();
        }
    }

    void OnSelectOldAtlas(MonoBehaviour obj)
    {
        oldAtlas = obj as UIAtlas;
    }

    void OnSelectNewAtlas(MonoBehaviour obj)
    {
        newAtlas = obj as UIAtlas;
    }

}
