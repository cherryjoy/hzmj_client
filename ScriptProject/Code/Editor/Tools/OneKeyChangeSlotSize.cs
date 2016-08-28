using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;

[InitializeOnLoad]

public class OneKeyChangeSlotSize:EditorWindow
{
    [MenuItem("LK-TOOL/一键更换Slot尺寸",
	          false,
	          1)]

    static void OpenSceneOpener()
    {
        EditorWindow windows = EditorWindow.GetWindow<OneKeyChangeSlotSize>(true, "一键更换Slot尺寸");
        windows.position = new Rect(Screen.currentResolution.width / 2 - 300, Screen.currentResolution.height / 2 - 200, 600, 400);
        windows.autoRepaintOnSceneChange = true;
    }
    string slotSpriteName = "zhuangbeikuang";
    string qualitySpriteName = "pinzhikuang_baise";
    int slotWidth = 102;
    int qualitySpriteWidth = 93;
    void Awake()
    {

    }
    void OnGUI()
    {
        GUILayout.BeginHorizontal();
        {
            EditorGUILayout.LabelField("Slot槽SpriteName");
            EditorGUILayout.LabelField("品质SpriteName");
        }
        GUILayout.EndHorizontal();
        GUILayout.BeginHorizontal();
        {
            slotSpriteName = EditorGUILayout.TextField(slotSpriteName);
            qualitySpriteName = EditorGUILayout.TextField(qualitySpriteName);
        }
        GUILayout.EndHorizontal();
        GUILayout.BeginHorizontal();
        {
            EditorGUILayout.LabelField("Slot槽宽度");
            EditorGUILayout.LabelField("品质槽宽度");
        }
        GUILayout.EndHorizontal();
        GUILayout.BeginHorizontal();
        {
            string slotWidthStr = EditorGUILayout.TextField(slotWidth.ToString());
            int tmpValue = 0;
            if (int.TryParse(slotWidthStr, out tmpValue))
            {
                slotWidth = tmpValue;
            }

            string qualityWidthStr = EditorGUILayout.TextField(qualitySpriteWidth.ToString());
            int tempValue1 = 0;
            if (int.TryParse(qualityWidthStr, out tempValue1))
            {
                qualitySpriteWidth = tempValue1;
            }
        }
        GUILayout.EndHorizontal();

        if (GUILayout.Button("一键更换"))
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
                j++;
                if (spr.spriteName == slotSpriteName)
                {
                    spr.SetDimesions(slotWidth, slotWidth);
                }
                else if (spr.spriteName == qualitySpriteName)
                {
                    spr.SetDimesions(qualitySpriteWidth, qualitySpriteWidth);
                }

                EditorUtility.SetDirty(spr);
            }

            Debug.Log(j + "个Sprite的Size被替换了。");

            AssetDatabase.SaveAssets();
        }
    }
}
