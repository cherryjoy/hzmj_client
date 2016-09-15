using UnityEngine;
using System.Collections;
using UnityEditor;

public class CopySprite : EditorWindow
{
	[MenuItem("CJ-TOOL/Copy Sprite")]
	static void Init()
	{
		EditorWindow win = EditorWindow.GetWindow(typeof(CopySprite), false, "Copy Sprite");
		win.minSize = new Vector2(500f, Screen.height);
	}

	bool startCopy = false;

	void OnGUI()
	{
		RefreshUI();
		DoCopyLogic();
	}
	GameObject go1;
	GameObject[] go2;
	UIAtlas mAtlas;
	string spriteName;
	int depth = 0;
    Vector2 dimension = Vector2.zero;
	bool canCopy = false;
	int type = 0;		//0: UISprite 1:UILabel
	void RefreshUI()
	{
		GUILayout.Space(30);
		if (!startCopy && GUILayout.Button("准备拷贝"))
		{
			startCopy = true;
		}
		else if (startCopy && GUILayout.Button("重置"))
		{
			startCopy = false;
			canCopy = false;
		}

		if (!startCopy)
		{
			go1 = Selection.activeGameObject;
		}
		else
		{
			go2 = Selection.gameObjects;
		}
	}

	void DoCopyLogic()
	{
		if (go1 != null)
		{
			UISprite spr = go1.GetComponent<UISprite>();
			if (spr != null)
			{
				type = 0;
				mAtlas = spr.atlas;
				spriteName = spr.spriteName;
				canCopy = true;
				depth = spr.depth;
                dimension = spr.Dimensions;
				EditorGUILayout.TextField("   Atlas		: " + mAtlas.name);
				EditorGUILayout.TextField("SpriteName	:" + spriteName);
				EditorGUILayout.TextField("   Depth		:" + depth);
                EditorGUILayout.TextField("Dimensions	:" + dimension);
			}else
			{
				UILabel label = go1.GetComponent<UILabel>();
				if (label != null)
				{
					type = 1;
					depth = label.depth;
                    dimension = label.Dimensions;
					canCopy = true;
					EditorGUILayout.TextField("   Depth		:" + depth);
                    EditorGUILayout.TextField("Dimensions	:" + dimension);
				}
				else
				{
                    UIWidget weigt = go1.GetComponent<UIWidget>();
                    if (weigt != null)
                    {
                        depth = weigt.depth;
                        type = 2;
                        dimension = weigt.Dimensions;
                        canCopy = true;
                        EditorGUILayout.TextField("Depth : " + depth);
                        EditorGUILayout.TextField("Dimensions	:" + dimension);
                    }
                    else
                    {
                        mAtlas = null;
                        spriteName = "";
                        depth = 0;
                        canCopy = false;
                    }
				}

			}
		}
		if (type == 0)
		{
			if (canCopy && startCopy)
			{
				if (go2.Length > 0)
				{
					GUILayout.BeginHorizontal();
					if (canCopy && GUILayout.Button("拷贝"))
					{
						for (int i = 0; i < go2.Length; i++)
						{
							UISprite spr = go2[i].GetComponent<UISprite>();
							spr.atlas = mAtlas;
							spr.spriteName = spriteName;
                            spr.Dimensions = dimension;
							spr.gameObject.SetActive(false);
							spr.gameObject.SetActive(true);
						}

					}
					GUILayout.Space(15);
					if (canCopy && GUILayout.Button("设置相同的Depth"))
					{
						for (int i = 0; i < go2.Length; i++)
						{
							UISprite spr = go2[i].GetComponent<UISprite>();
							if (spr != null)
							{
								spr.depth = depth;
							}
						}

					}
					GUILayout.EndHorizontal();
				}
			}
		}
		else if (type == 1)
		{
            if (canCopy && GUILayout.Button("设置相同的Depth"))
            {
                for (int i = 0; i < go2.Length; i++)
                {
                    UILabel label = go2[i].GetComponent<UILabel>();
                    if (label != null)
                    {
                        label.depth = depth;
                    }

                }
            }
            if (canCopy && GUILayout.Button("设置相同的Dimension"))
            {
                for (int i = 0; i < go2.Length; i++)
                {
                    UILabel label = go2[i].GetComponent<UILabel>();
                    if (label != null)
                    {
                        label.Dimensions = dimension;
                    }

                }
            }
		}
        else if (type == 2)
        {
            if (canCopy && GUILayout.Button("设置相同的Depth"))
            {
                for (int i = 0; i < go2.Length; i++)
                {
                    UIWidget widget = go2[i].GetComponent<UIWidget>();
                    if (widget != null)
                    {
                        widget.depth = depth;
                    }
                }
            }
            if (canCopy && GUILayout.Button("设置相同的Dimension"))
            {
                for (int i = 0; i < go2.Length; i++)
                {
                    UIWidget widget = go2[i].GetComponent<UIWidget>();
                    if (widget != null)
                    {
                        widget.Dimensions = dimension;
                    }
                }
            }
        }
		
	}
}
