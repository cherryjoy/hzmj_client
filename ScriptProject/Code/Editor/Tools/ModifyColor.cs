using UnityEngine;
using System.Collections;
using UnityEditor;
using System.IO;
using System.Collections.Generic;
using System;

class ColorInfo{
    public Color color;
    public string colorBtnTxt;

    public ColorInfo(Color color,string btnTxt)
    {
        this.color = color;
        colorBtnTxt = btnTxt;
    }
}

public class ModifyColor : EditorWindow {
    //static public ModifyColor instance;

	[MenuItem("LK-TOOL/Modify Color")]
	static void Init()
	{
		EditorWindow win = EditorWindow.GetWindow(typeof(ModifyColor), false, "Modify Color");
		win.minSize = new Vector2(500f, Screen.height);
	}

    void OnEnable()
    {
        //instance = this;
    }

    void OnDisable()
    {
        //instance = null;
    }

	public enum ComponentType
	{
		ENUM_NON_DIFINE,
		ENUM_SPRITE,
		ENUM_LABEL
	}

    Color ColorCopyNow = Color.white;

    ArrayList colorListInfo = new ArrayList();
    string[] colorBtnsTxt;
    Color[] colors;
    bool[] isBtnClick;

	ComponentType curType = ComponentType.ENUM_NON_DIFINE;
    const int MAX_LAST_COLOR_NUM = 4;

    static Queue lastUseColors = new Queue(MAX_LAST_COLOR_NUM);

    int colorCount = 0;

    void InitColorList()
    {
        colorListInfo.Clear();
        colorCount = 0;
        string line;

        string dataPath = Application.dataPath.Replace('/', '\\');
        StreamReader file = new StreamReader(dataPath + @"\Editor Default Resources\UIColorList.txt");

        while ((line = file.ReadLine()) != null)
        {
            if (line != "" && !line.StartsWith("//"))
            {
                colorListInfo.Add(line);
                colorCount++;
            }
        }
        file.Close();
        file = null;

        colorBtnsTxt = new string[colorCount / 2];
        colors = new Color[colorBtnsTxt.Length];

        for (int i = 0; i < colorCount; i++)
        {
            if ((i & 1) == 0)
            {
                int index = i / 2;
                colorBtnsTxt[index] = colorListInfo[i].ToString();
            }
            else
            {
                int index = (i - 1) / 2;
                string[] colorInfo = colorListInfo[i].ToString().Split(' ');
                float alpha = int.Parse(colorInfo[1]) / 100f;
                colors[index] = ColorOperation.GetColorFromStr(colorInfo[0], alpha);
            }
        }	
    }

    void OnFocus()
    {
        InitColorList();
    }

	string[] labelColorFunc = new string[]{
		"Color Tint",
		"TopColor",	
		"BottomColor",
		"Set OutLine",
		"Set None",
		"Set Shadow"
	};

	
	Transform[] tran;
	int witchColor = 0;
    UIWidget curWidget;
    void OnGUI()
    {
        try
        {
            tran = Selection.GetTransforms(SelectionMode.TopLevel);
            if (tran != null)
            {
                for (int i = 0; i < tran.Length; i++)
                {
                    if (tran[i] == null)
                        continue;
                    System.Type type = tran[i].GetComponent<MonoBehaviour>().GetType();
                    if (type.ToString().Contains("UILabel"))
                    {
                        curType = ComponentType.ENUM_LABEL;
                        curWidget = tran[i].GetComponent<UIWidget>();
                    }
                    else if (type.ToString().Contains("UISprite") ||
                        type.ToString().Contains("UISlicedSprite") ||
                        type.ToString().Contains("UITiledSprite"))
                    {
                        curType = ComponentType.ENUM_SPRITE;
                        curWidget = tran[i].GetComponent<UIWidget>();
                    }
                    else
                        curType = ComponentType.ENUM_NON_DIFINE;
                }
            }

            GUILayout.BeginHorizontal();
            showColorType(curType);
            GUILayout.Space(20);
            ShowBtnUI();
            GUILayout.Space(30);
            GUI.color = ColorCopyNow;
            if (GUILayout.Button("点击应用此颜色\n" + ColorCopyNow.ToString(), GUILayout.Width(200), GUILayout.Height(100)))
            {
                SetColor(ColorCopyNow);
            }
            GUI.color = Color.white;
            GUILayout.EndHorizontal();
        }
        catch (System.Exception ex)
        {
        }
    }

	void showColorType(ComponentType type)
	{
		switch(type)
		{
			case ComponentType.ENUM_LABEL:
				LabelUI();
				break;
			case ComponentType.ENUM_SPRITE:
				SpriteUI();
				break;
			case ComponentType.ENUM_NON_DIFINE:
				break;
				default:
				break;
		}
	}

	void LabelUI()
	{
		GUILayout.BeginVertical();

        CommonUIWidget();

		if (GUILayout.Button(labelColorFunc[1], GUILayout.Width(120)))
		{
			witchColor = 1;
		}
		GUILayout.Space(10);

		if (GUILayout.Button(labelColorFunc[2], GUILayout.Width(120)))
		{
			witchColor = 2;
		}
		GUILayout.Space(50);
		if (GUILayout.Button(labelColorFunc[3], GUILayout.Width(120)))
		{
			for (int i = 0; i < tran.Length; i++)
			{
				UILabel lb = tran[i].GetComponent<UILabel>();
				lb.effectStyle = UILabel.Effect.Outline;
			}
            witchColor = 3;
		}
		GUILayout.Space(10);

		if (GUILayout.Button(labelColorFunc[4], GUILayout.Width(120)))
		{
			for (int i = 0; i < tran.Length; i++)
			{
				UILabel lb = tran[i].GetComponent<UILabel>();
				lb.effectStyle = UILabel.Effect.None;
			}
		}
		GUILayout.Space(10);

		if (GUILayout.Button(labelColorFunc[5], GUILayout.Width(120)))
		{
			for (int i = 0; i < tran.Length; i++)
			{
				UILabel lb = tran[i].GetComponent<UILabel>();
				lb.effectStyle = UILabel.Effect.Shadow;
			}
            witchColor = 3;
		}
		GUILayout.EndVertical();

	}

	void SpriteUI()
    {
        GUILayout.BeginVertical();
        CommonUIWidget();

        GUILayout.EndVertical();
	}

    void CommonUIWidget()
    {
        if (GUILayout.Button("Copy Color", GUILayout.Width(120)))
        {
            if (curWidget != null)
            {
                ColorCopyNow = curWidget.color;
            }
        }
        GUILayout.Space(10);

        GUILayout.Label("正在修改属性：", GUILayout.Width(120));
        GUIStyle fontStyle = new GUIStyle();
        fontStyle.fontSize = 15;
        fontStyle.normal.textColor = Color.green;
        GUILayout.Label(" " + labelColorFunc[witchColor], fontStyle);
        GUILayout.Space(10);

        if (GUILayout.Button(labelColorFunc[0], GUILayout.Width(120)))
        {
            witchColor = 0;
        }
        GUILayout.Space(10);
    }

	void ShowBtnUI()
	{
		GUILayout.BeginVertical();
        GUILayout.Label("最近使用颜色：", GUILayout.Width(120));
        ShowLastUseColor();
        GUILayout.Space(20);

        for (int i = 0; i < colorBtnsTxt.Length; i++)
		{
            int MaxNumPerLine = 2;
			GUILayout.BeginHorizontal();
            for (int j = 0; j < MaxNumPerLine;j++ )
            {
                i += j;
                if (i < colorBtnsTxt.Length)
                {
                    DrawBtn(colors[i], colorBtnsTxt[i]);
                }
            }

			GUILayout.EndHorizontal();
		}
		GUILayout.EndVertical();
	}

    void ShowLastUseColor()
    {
        GUILayout.BeginVertical();
        int i = 0;
        foreach (object color in lastUseColors)
        {
            if(i==0||(i&1)==0)
                GUILayout.BeginHorizontal();

            ColorInfo info = color as ColorInfo;
            DrawBtn(info.color, info.colorBtnTxt);

            if(i==lastUseColors.Count-1||(i&1)!=0)
                GUILayout.EndHorizontal();

            i++;
        }
        GUILayout.EndVertical();
       
    }

    void DrawBtn(Color color,string btnString)
    {
        if (color.a <= 0)
        {
            GUI.color = Color.white;
        }
        else
        {
            GUI.color = color;
        }

        if (GUILayout.Button(btnString, GUILayout.Width(150)))
        {
            SetColor(color);
            SaveLastUseColor(new ColorInfo(color,btnString));
        }
        GUI.color = Color.white;
        GUILayout.Space(10);
    }

	void SetColor(Color color)
	{
		if (curType == ComponentType.ENUM_LABEL)
		{
			for (int i = 0; i < tran.Length; i++)
			{
				UILabel lb = tran[i].GetComponent<UILabel>();
				if (lb != null)
				{
					switch (witchColor)
					{
                        case 0:
                            lb.color = color;
                            lb.MarkAsChanged();
                            break;
						case 1:
                            lb.TopColor = color;
                            lb.MarkAsChanged();
							break;
						case 2:
							lb.BottomColor = color;
                            lb.MarkAsChanged();
							break;
                        case 3:
                            lb.effectColor = color;
                            break;
						default:
							break;
					}
				}
			}
			
		}
		else if (curType == ComponentType.ENUM_SPRITE)
		{
			for (int i = 0; i < tran.Length; i++)
			{
				UISprite sp = tran[i].GetComponent<UISprite>();
				if (sp != null)
				{
					if (witchColor == 0)
					{
						sp.color = color;
                        sp.MarkAsChanged();
					}
				}
			}
		}
		
	}

    void SaveLastUseColor(ColorInfo info)
    {
        bool isContain = false;
        foreach (object color in lastUseColors)
        {
            ColorInfo colorInfo = color as ColorInfo;
            if (colorInfo.color == info.color)
            {
                isContain = true;
                break;
            }
        }
        if (!isContain)
        {
            if(lastUseColors.Count>=MAX_LAST_COLOR_NUM)
                lastUseColors.Dequeue();
            lastUseColors.Enqueue(info);
        } 
    }
}
