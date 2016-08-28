//----------------------------------------------
//            NGUI: Next-Gen UI kit
// Copyright © 2011-2012 Tasharen Entertainment
//----------------------------------------------

using UnityEditor;
using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// UI Widget Creation Wizard
/// </summary>

public class UICreateWidgetWizard : EditorWindow
{
	public enum WidgetType
	{
		Label,
		Sprite,
		ColorSprite,
		Particle,
		SlicedSprite,
		TiledSprite,
		FilledSprite,
		SimpleTexture,
		Button,
		ImageButton,
		Checkbox,
		ProgressBar,
		Slider,
		Input,
		PopupList,
		PopupMenu,
		RadioButton,
        ComplexRadioButton,
	}

	static WidgetType mType = WidgetType.Button;
	static string mParticle = "";
	static string mSprite = "";
	static string mColorSprite = "";
	static string mSliced = "";
	static string mTiled = "";
	static string mFilled = "";
	static string mButton = "";
	static string mImage0 = "";
	static string mImage1 = "";
	static string mImage2 = "";
    static string mImage3 = "";
	static string mSliderBG = "";
	static string mSliderFG = "";
	static string mSliderTB = "";
	static string mCheckBG = "";
	static string mCheck = "";
	static string mInputBG = "";
	static string mListFG = "";
	static string mListBG = "";
	static string mListHL = "";
	static Color mColor = Color.white;
	static bool mLoaded = false;
	/// <summary>
	/// Save the specified string into player prefs.
	/// </summary>

	static void SaveString (string field, string val)
	{
		if (string.IsNullOrEmpty(val))
		{
			PlayerPrefs.DeleteKey(field);
		}
		else
		{
			PlayerPrefs.SetString(field, val);
		}
	}

	/// <summary>
	/// Load the specified string from player prefs.
	/// </summary>

	static string LoadString (string field) { string s = PlayerPrefs.GetString(field); return (string.IsNullOrEmpty(s)) ? "" : s; }

	/// <summary>
	/// Save all serialized values in player prefs.
	/// This is necessary because static values get wiped out as soon as scripts get recompiled.
	/// </summary>

	static void Save ()
	{
		PlayerPrefs.SetInt("NGUI Widget Type", (int)mType);
		PlayerPrefs.SetInt("NGUI Color", NGUIMath.ColorToInt(mColor));

		SaveString("NGUI Particle", mParticle);
		SaveString("NGUI Sprite", mSprite);
		SaveString("NGUI Sliced", mSliced);
		SaveString("NGUI ColorSprite", mColorSprite);
		SaveString("NGUI Tiled", mTiled);
		SaveString("NGUI Filled", mFilled);
		SaveString("NGUI Button", mButton);
		SaveString("NGUI Image 0", mImage0);
		SaveString("NGUI Image 1", mImage1);
		SaveString("NGUI Image 2", mImage2);
        SaveString("NGUI Image 3", mImage3);
		SaveString("NGUI CheckBG", mCheckBG);
		SaveString("NGUI Check", mCheck);
		SaveString("NGUI SliderBG", mSliderBG);
		SaveString("NGUI SliderFG", mSliderFG);
		SaveString("NGUI SliderTB", mSliderTB);
		SaveString("NGUI InputBG", mInputBG);
		SaveString("NGUI ListFG", mListFG);
		SaveString("NGUI ListBG", mListBG);
		SaveString("NGUI ListHL", mListHL);

		PlayerPrefs.Save();
	}

	/// <summary>
	/// Load all serialized values from Player Prefs.
	/// This is necessary because static values get wiped out as soon as scripts get recompiled.
	/// </summary>

	static void Load ()
	{
		mType = (WidgetType)PlayerPrefs.GetInt("NGUI Widget Type", 0);

		int color = PlayerPrefs.GetInt("NGUI Color", -1);
		if (color != -1) mColor = NGUIMath.IntToColor(color);

		mParticle	= LoadString("NGUI Particle");
		mSprite		= LoadString("NGUI Sprite");
		mSliced		= LoadString("NGUI Sliced");
		mColorSprite = LoadString("NGUI ColorSprite");
		mTiled		= LoadString("NGUI Tiled");
		mFilled		= LoadString("NGUI Filled");
		mButton		= LoadString("NGUI Button");
		mImage0		= LoadString("NGUI Image 0");
		mImage1		= LoadString("NGUI Image 1");
		mImage2		= LoadString("NGUI Image 2");
        mImage3     = LoadString("NGUI Image 3");
		mCheckBG	= LoadString("NGUI CheckBG");
		mCheck		= LoadString("NGUI Check");
		mSliderBG	= LoadString("NGUI SliderBG");
		mSliderFG	= LoadString("NGUI SliderFG");
		mSliderTB	= LoadString("NGUI SliderTB");
		mInputBG	= LoadString("NGUI InputBG");
		mListFG		= LoadString("NGUI ListFG");
		mListBG		= LoadString("NGUI ListBG");
		mListHL		= LoadString("NGUI ListHL");
	}

	/// <summary>
	/// Atlas selection function.
	/// </summary>

	void OnSelectAtlas (MonoBehaviour obj)
	{
		UISettings.atlas = obj as UIAtlas;
		Repaint();
	}

	/// <summary>
	/// Font selection function.
	/// </summary>

	void OnSelectFont (Object obj)
	{
		Font trueFont = obj as Font;
		UIFont font = UIFont.CreateInstance<UIFont>();
		font.TrueTypeFont = trueFont;
		UISettings.font = font;
		Repaint();
	}



	/// <summary>
	/// Convenience function -- creates the "Add To" button and the parent object field to the right of it.
	/// </summary>

	bool ShouldCreate (GameObject go, bool isValid)
	{
		GUI.color = isValid ? Color.green : Color.grey;

		GUILayout.BeginHorizontal();
		bool retVal = GUILayout.Button("Add To", GUILayout.Width(76f));
		GUI.color = Color.white;
		GameObject sel = EditorGUILayout.ObjectField(go, typeof(GameObject), true, GUILayout.Width(140f)) as GameObject;
		GUILayout.Label("Select the parent in the Hierarchy View", GUILayout.MinWidth(10000f));
		GUILayout.EndHorizontal();

		if (sel != go) Selection.activeGameObject = sel;

		if (retVal && isValid)
		{
			NGUIEditorTools.RegisterUndo("Add a Widget");
			return true;
		}
		return false;
	}

	/// <summary>
	/// Label creation function.
	/// </summary>

	void CreateLabel (GameObject go)
	{
		GUILayout.BeginHorizontal();
        mColor = new Color(212f / 255f, 228f / 255f, 238f / 255f);
		Color c = EditorGUILayout.ColorField("Color", mColor, GUILayout.Width(220f));
		GUILayout.Label("Color tint the label will start with");
		GUILayout.EndHorizontal();

		if (mColor != c)
		{
			mColor = c;
			Save();
		}

		if (ShouldCreate(go, UISettings.font != null))
		{
			UILabel lbl = NGUITools.AddWidget<UILabel>(go);
            lbl.Dimensions = new Vector2(24f, 24f);
			lbl.font = UISettings.font;
			lbl.text = "Loading...";
			lbl.color = mColor;
			lbl.MakePixelPerfect();
			Selection.activeGameObject = lbl.gameObject;
		}
	}

	/// <summary>
	/// Sprite creation function.
	/// </summary>

	bool CreateSprite<T> (GameObject go, ref string field) where T : UISprite
	{
		if (UISettings.atlas != null)
		{
			GUILayout.BeginHorizontal();
			string sp = UISpriteInspector.SpriteField(UISettings.atlas, "Sprite", field, GUILayout.Width(200f));

			if (!string.IsNullOrEmpty(sp))
			{
				GUILayout.Space(20f);
				GUILayout.Label("Sprite that will be created");
			}
			GUILayout.EndHorizontal();

			if (sp != field)
			{
				field = sp;
				Save();
			}
		}
		if (ShouldCreate(go, UISettings.atlas != null))
		{
			T sprite = NGUITools.AddWidget<T>(go);
			sprite.name = sprite.name + " (" + field + ")";
			sprite.atlas = UISettings.atlas;
			sprite.spriteName = field;
			sprite.MakePixelPerfect();
			Selection.activeGameObject = sprite.gameObject;
			
			return true;
		}
		else
			return false;
	}

	/// <summary>
	/// Sprite creation function.
	/// </summary>

	void CreateSprite<T>(GameObject go, string field, SpriteSelector.Callback callback) where T : UISprite
	{
		if (UISettings.atlas != null)
		{
			UISpriteInspector.SpriteField("Sprite", "Sprite that will be created", UISettings.atlas, field, callback);

			if (!string.IsNullOrEmpty(field))
			{
				GUILayout.BeginHorizontal();
				UISettings.pivot = (UIWidget.Pivot)EditorGUILayout.EnumPopup("Pivot", UISettings.pivot, GUILayout.Width(200f));
				GUILayout.Space(20f);
				GUILayout.Label("Initial pivot point used by the sprite");
				GUILayout.EndHorizontal();
			}
		}

		if (ShouldCreate(go, UISettings.atlas != null))
		{
			T sprite = NGUITools.AddWidget<T>(go);
			sprite.name = sprite.name + " (" + field + ")";
			sprite.atlas = UISettings.atlas;
			sprite.spriteName = field;
			sprite.pivot = UISettings.pivot;
			sprite.MakePixelPerfect();
			Selection.activeGameObject = sprite.gameObject;
		}
	}

	void OnParticle(string val) { mParticle = val; Save(); Repaint(); }
	void OnSprite(string val) { mSprite = val; Save(); Repaint(); }
	void OnSliced(string val) { mSliced = val; Save(); Repaint(); }
	void OnColorSprite(string val) { mColorSprite = val; Save(); Repaint(); }
	void OnTiled(string val) { mTiled = val; Save(); Repaint(); }
	void OnFilled(string val) { mFilled = val; Save(); Repaint(); }
    void OnImage0(string val) { mImage0 = val; Save(); Repaint(); }
    void OnImage1(string val) { mImage1 = val; Save(); Repaint(); }
    void OnImage2(string val) { mImage2 = val; Save(); Repaint(); }
    void OnImage3(string val) { mImage3 = val; Save(); Repaint(); }
    void OnButton(string val) { mButton = val; Save(); Repaint(); }

	/// <summary>
	/// UI Texture doesn't do anything other than creating the widget.
	/// </summary>

	void CreateSimpleTexture (GameObject go)
	{
		if (ShouldCreate(go, true))
		{
			UITexture tex = NGUITools.AddWidget<UITexture>(go);
			Selection.activeGameObject = tex.gameObject;
		}
	}

	/// <summary>
	/// Button creation function.
	/// </summary>

	void CreateButton (GameObject go)
	{
		if (UISettings.atlas != null)
		{
            //GUILayout.BeginHorizontal();
            //string bg = UISpriteInspector.SpriteField(UISettings.atlas, "Background", mButton, GUILayout.Width(200f));
            //GUILayout.Space(20f);
            //GUILayout.Label("Sliced Sprite for the background");
            //GUILayout.EndHorizontal();
            //if (mButton != bg) { mButton = bg; Save(); }
            NGUIEditorTools.DrawSpriteField("Background", "Sliced Sprite for the background", NGUISettings.atlas, mButton, OnButton, GUILayout.Width(120f));
		}

		if (ShouldCreate(go, UISettings.atlas != null))
		{
			int depth = NGUITools.CalculateNextDepth(go);
			go = NGUITools.AddChild(go);
			go.name = "Button";

			UISlicedSprite bg = NGUITools.AddWidget<UISlicedSprite>(go);
			bg.name = "Background";
			bg.depth = depth;
			bg.atlas = UISettings.atlas;
			bg.spriteName = mButton;
            //Texture2D tex = bg.mainTexture as Texture2D;
            //bg.transform.localScale = new Vector3(Mathf.RoundToInt(Mathf.Abs(bg.outerUV.width * tex.width)), 
            //    Mathf.RoundToInt(Mathf.Abs(bg.outerUV.height * tex.height)),
            //    1f);
            bg.Dimensions = new Vector2(144f, 54f);
			bg.MakePixelPerfect();

			if (UISettings.font != null)
			{
				UILabel lbl = NGUITools.AddWidget<UILabel>(go);
				lbl.font = UISettings.font;
				lbl.text = go.name;
                lbl.Dimensions = new Vector2(24f, 24f);
				lbl.MakePixelPerfect();
			}

			// Add a collider
			NGUITools.AddWidgetCollider(go);

			// Add the scripts
			go.AddComponent<UIButtonColor>().tweenTarget = bg.gameObject;
			go.AddComponent<UIButtonScale>();
			go.AddComponent<UIButtonOffset>();
			go.AddComponent<UIButtonSound>();

			Selection.activeGameObject = go;
		}
	}

	/// <summary>
	/// Button creation function.
	/// </summary>

	void CreateImageButton (GameObject go)
	{
		if (UISettings.atlas != null)
		{
            NGUIEditorTools.DrawSpriteField("Normal", "Normal state sprite", NGUISettings.atlas, mImage0, OnImage0, GUILayout.Width(120f));
            NGUIEditorTools.DrawSpriteField("Hover", "Hover state sprite", NGUISettings.atlas, mImage1, OnImage1, GUILayout.Width(120f));
            NGUIEditorTools.DrawSpriteField("Pressed", "Pressed state sprite", NGUISettings.atlas, mImage2, OnImage2, GUILayout.Width(120f));
            NGUIEditorTools.DrawSpriteField("Disabled", "Disabled state sprite", NGUISettings.atlas, mImage3, OnImage3, GUILayout.Width(120f));
		}

		if (ShouldCreate(go, UISettings.atlas != null))
		{
			int depth = NGUITools.CalculateNextDepth(go);
			go = NGUITools.AddChild(go);
			go.name = "Image Button";

			UIAtlas.Sprite sp = UISettings.atlas.GetSprite(mImage0);
			UISprite sprite = (sp.inner == sp.outer) ? NGUITools.AddWidget<UISprite>(go) :
				(UISprite)NGUITools.AddWidget<UISlicedSprite>(go);

			sprite.name = "Background";
			sprite.depth = depth;
			sprite.atlas = UISettings.atlas;
			sprite.spriteName = mImage0;
            sprite.Dimensions = new Vector2(144f, 54f);
            //sprite.transform.localScale = new Vector3(150f, 40f, 1f);
			sprite.MakePixelPerfect();

			if (UISettings.font != null)
			{
				UILabel lbl = NGUITools.AddWidget<UILabel>(go);
				lbl.font = UISettings.font;
                lbl.depth = depth + 1;
				lbl.text = go.name;
                lbl.Dimensions = new Vector2(24f, 24f);
                Vector3 pos = lbl.transform.localPosition;
                pos.y = 4f;
                lbl.transform.localPosition = pos;
				lbl.MakePixelPerfect();
			}

			// Add a collider
			NGUITools.AddWidgetCollider(go);

			// Add the scripts
			UIImageButton ib = go.AddComponent<UIImageButton>();
            ib.targets = new UISprite[1];
			ib.targets[0]		 = sprite;
			ib.normalSprite  = mImage0;
			ib.hoverSprite	 = mImage1;
			ib.pressedSprite = mImage2;
            ib.disableSprite = mImage3;
			go.AddComponent<UIButtonSound>();

			Selection.activeGameObject = go;
		}
	}
	
    void CreateComplexRadioButton(GameObject go)
    {
        if (UISettings.atlas != null)
        {
            NGUIEditorTools.DrawSpriteField("Normal", "Active state sprite", UISettings.atlas, mImage0, (name) =>
            {
                if (mImage0 != name) { mImage0 = name; Save(); Repaint(); }
            }, GUILayout.Width(120f));

            NGUIEditorTools.DrawSpriteField("Pressed", "Unactive state sprite", UISettings.atlas, mImage1, (name) =>
            {
                if (mImage1 != name) { mImage1 = name; Save(); Repaint(); }
            }, GUILayout.Width(120f));

            NGUIEditorTools.DrawSpriteField("Foreground", "foreground sprite", UISettings.atlas, mImage2, (name) =>
            {
                if (mImage2 != name) { mImage2 = name; Save(); Repaint(); }
            }, GUILayout.Width(120f));
        }

        if (ShouldCreate(go, UISettings.atlas != null))
        {
            int depth = NGUITools.CalculateNextDepth(go);
            go = NGUITools.AddChild(go);
            go.name = "Complex Radio Button";

            GameObject btn = NGUITools.AddChild(go);
            btn.name = "Complex Button";

            UIAtlas.Sprite sp1 = UISettings.atlas.GetSprite(mImage0);
            UISprite sprite = (sp1.inner == sp1.outer) ? NGUITools.AddWidget<UISprite>(btn) : (UISprite)NGUITools.AddWidget<UISlicedSprite>(btn);
            sprite.name = "Background";
            sprite.depth = depth;
            sprite.atlas = UISettings.atlas;
            sprite.spriteName = mImage0;
            //sprite.transform.localScale = new Vector3(144f, 54f, 1f);
            sprite.Dimensions = new Vector2(144f, 54f);
            sprite.MakePixelPerfect();

            UIAtlas.Sprite sp2 = UISettings.atlas.GetSprite(mImage2);
            UISprite sprite1 = (sp2.inner == sp2.outer) ? NGUITools.AddWidget<UISprite>(go) : (UISprite)NGUITools.AddWidget<UISlicedSprite>(go);
            sprite1.name = "Foreground";
            sprite1.depth = depth + 1;
            sprite1.atlas = UISettings.atlas;
            sprite1.spriteName = mImage2;
            //sprite1.transform.localScale = new Vector3(174f, 58f,1f);
            sprite1.Dimensions = new Vector2(174f, 58f);
            sprite1.MakePixelPerfect();

            NGUITools.AddWidgetCollider(go);
            UIRadioComplexButton ib = go.AddComponent<UIRadioComplexButton>();
            ib.target = sprite;
            ib.foreground = sprite1;
            ib.ActiveSprite = mImage1;
            ib.UnactiveSprite = mImage0;
            go.AddComponent<UIButtonSound>();

			UILabel label = NGUITools.AddWidget<UILabel>(btn);
			label.name = "Label";
			label.depth = depth + 1;
			label.FontSize = 24;
			label.text = "New Label";
            //label.transform.localScale = new Vector3(24f, 24f, 1f);
            label.Dimensions = new Vector2(24f, 24f);
			ib.mLabel = label;

            //Selection.activeGameObject = go;
        }
    }

	void CreateRadioButton (GameObject go)
	{
		if (UISettings.atlas != null)
		{
            NGUIEditorTools.DrawSpriteField("Normal", "Active state sprite", UISettings.atlas, mImage0, (name) =>
            {
                if (mImage0 != name) { mImage0 = name; Save(); Repaint(); }
            }, GUILayout.Width(120f));

            NGUIEditorTools.DrawSpriteField("Pressed", "Unactive state sprite", UISettings.atlas, mImage1, (name) =>
            {
                if (mImage1 != name) { mImage1 = name; Save(); Repaint(); }
            }, GUILayout.Width(120f));
		}

		if (ShouldCreate(go, UISettings.atlas != null))
		{
			int depth = NGUITools.CalculateNextDepth(go);
			go = NGUITools.AddChild(go);
			go.name = "Radio Button";

			UIAtlas.Sprite sp = UISettings.atlas.GetSprite(mImage0);
			UISprite sprite = (sp.inner == sp.outer) ? NGUITools.AddWidget<UISprite>(go) :
				(UISprite)NGUITools.AddWidget<UISlicedSprite>(go);

			sprite.name = "Background";
			sprite.depth = depth;
			sprite.atlas = UISettings.atlas;
			sprite.spriteName = mImage0;
            //sprite.transform.localScale = new Vector3(150f, 40f, 1f);
            sprite.Dimensions = new Vector2(150f, 40f);
			sprite.MakePixelPerfect();

			// Add a collider
			NGUITools.AddWidgetCollider(go);

			// Add the scripts
			UIRadioButton ib = go.AddComponent<UIRadioButton>();
			ib.target		 = sprite;
			ib.ActiveSprite  = mImage0;
			ib.UnactiveSprite = mImage1;
			go.AddComponent<UIButtonSound>();

			Selection.activeGameObject = go;
		}
	}

	/// <summary>
	/// Checkbox creation function.
	/// </summary>

	void CreateCheckbox (GameObject go)
	{
		if (UISettings.atlas != null)
		{
            NGUIEditorTools.DrawSpriteField("Background", "Sliced Sprite for the background", UISettings.atlas, mCheckBG, (name) =>
            {
                if (name != mCheckBG) { mCheckBG = name; Save(); Repaint(); }
            }, GUILayout.Width(120f));

            NGUIEditorTools.DrawSpriteField("Checkmark", "Sprite visible when the state is 'checked'", UISettings.atlas, mCheck, (name) =>
            {
                if (name != mCheck) { mCheck = name; Save(); Repaint(); }
            }, GUILayout.Width(120f));
		}

		if (ShouldCreate(go, UISettings.atlas != null))
		{
			int depth = NGUITools.CalculateNextDepth(go);
			go = NGUITools.AddChild(go);
			go.name = "Checkbox";

			UISlicedSprite bg = NGUITools.AddWidget<UISlicedSprite>(go);
			bg.name = "Background";
			bg.depth = depth;
			bg.atlas = UISettings.atlas;
			bg.spriteName = mCheckBG;
            //bg.transform.localScale = new Vector3(26f, 26f, 1f);
            bg.Dimensions = new Vector2(26f, 26f);
			bg.MakePixelPerfect();

			UISprite fg = NGUITools.AddWidget<UISprite>(go);
			fg.name = "Checkmark";
			fg.atlas = UISettings.atlas;
			fg.spriteName = mCheck;
            fg.Dimensions = new Vector2(32f, 32f);
			fg.MakePixelPerfect();

			if (UISettings.font != null)
			{
				UILabel lbl = NGUITools.AddWidget<UILabel>(go);
				lbl.font = UISettings.font;
				lbl.text = go.name;
				lbl.pivot = UIWidget.Pivot.Left;
				lbl.transform.localPosition = new Vector3(16f, 0f, 0f);
                lbl.Dimensions = new Vector2(24f, 24f);
				lbl.MakePixelPerfect();
			}

			// Add a collider
			NGUITools.AddWidgetCollider(go);

			// Add the scripts
			go.AddComponent<UICheckbox>().checkSprite = fg;
			go.AddComponent<UIButtonColor>().tweenTarget = bg.gameObject;
			go.AddComponent<UIButtonScale>().tweenTarget = bg.transform;
			go.AddComponent<UIButtonSound>();

			Selection.activeGameObject = go;
		}
	}

	/// <summary>
	/// Progress bar creation function.
	/// </summary>

	void CreateSlider (GameObject go, bool slider)
	{
		if (UISettings.atlas != null)
		{
            NGUIEditorTools.DrawSpriteField("Background", "Sprite for the background (empty bar)", UISettings.atlas, mSliderBG, (name) =>
            {
                if (name != mSliderBG) { mSliderBG = name; Save(); Repaint(); }
            }, GUILayout.Width(120f));

            NGUIEditorTools.DrawSpriteField("Foreground", "Sprite for the foreground (full bar)", UISettings.atlas, mSliderFG, (name) =>
            {
                if (name != mSliderFG) { mSliderFG = name; Save(); Repaint(); }
            }, GUILayout.Width(120f));

            if (slider)
            {
                string tb = mSliderTB;
                NGUIEditorTools.DrawSpriteField("Thumb", "Sprite for the thumb indicator", UISettings.atlas, mSliderTB, (name) =>
                {
                    if (name != mSliderTB) { mSliderTB = name; Save(); Repaint(); }
                }, GUILayout.Width(120f));
            }
		}

		if (ShouldCreate(go, UISettings.atlas != null))
		{
			int depth = NGUITools.CalculateNextDepth(go);
			go = NGUITools.AddChild(go);
			go.name = slider ? "Slider" : "Progress Bar";

			// Background sprite
			UIAtlas.Sprite bgs = UISettings.atlas.GetSprite(mSliderBG);
			UISprite back = (bgs.inner == bgs.outer) ?
				(UISprite)NGUITools.AddWidget<UISprite>(go) :
				(UISprite)NGUITools.AddWidget<UISlicedSprite>(go);

			back.name = "Background";
			back.depth = depth;
			back.pivot = UIWidget.Pivot.Left;
			back.atlas = UISettings.atlas;
			back.spriteName = mSliderBG;
            //back.transform.localScale = new Vector3(200f, 30f, 1f);
            back.Dimensions = new Vector2(200f, 30f);
			back.MakePixelPerfect();

			// Fireground sprite
			UIAtlas.Sprite fgs = UISettings.atlas.GetSprite(mSliderFG);
			UISprite front = (fgs.inner == fgs.outer) ?
				(UISprite)NGUITools.AddWidget<UIFilledSprite>(go) :
				(UISprite)NGUITools.AddWidget<UISlicedSprite>(go);

			front.name = "Foreground";
			front.pivot = UIWidget.Pivot.Left;
			front.atlas = UISettings.atlas;
			front.spriteName = mSliderFG;
            //front.transform.localScale = new Vector3(200f, 30f, 1f);
            front.Dimensions = new Vector2(196f, 28f);
			front.MakePixelPerfect();

			// Add a collider
			if (slider) NGUITools.AddWidgetCollider(go);

			// Add the slider script
			UISlider uiSlider = go.AddComponent<UISlider>();
			uiSlider.foreground = front.GetComponent<UIWidget>();
            //uiSlider.fullSize = front.transform.localScale;

			// Thumb sprite
			if (slider)
			{
				UIAtlas.Sprite tbs = UISettings.atlas.GetSprite(mSliderTB);
				UISprite thb = (tbs.inner == tbs.outer) ?
					(UISprite)NGUITools.AddWidget<UISprite>(go) :
					(UISprite)NGUITools.AddWidget<UISlicedSprite>(go);

				thb.name = "Thumb";
				thb.atlas = UISettings.atlas;
				thb.spriteName = mSliderTB;
				thb.transform.localPosition = new Vector3(200f, 0f, 0f);
                //thb.transform.localScale = new Vector3(20f, 40f, 1f);
                thb.Dimensions = new Vector2(20f, 20f);
				thb.MakePixelPerfect();

				NGUITools.AddWidgetCollider(thb.gameObject);
				thb.gameObject.AddComponent<UIButtonColor>();
				thb.gameObject.AddComponent<UIButtonScale>();

				uiSlider.thumb = thb.transform;
			}
			uiSlider.rawValue = 0.75f;

			// Select the slider
			Selection.activeGameObject = go;
		}
	}

	/// <summary>
	/// Input field creation function.
	/// </summary>
    void OnInputBG(string val) { mInputBG = val; Save(); Repaint(); }


	void CreateInput (GameObject go)
	{
		if (UISettings.atlas != null)
		{
            //GUILayout.BeginHorizontal();
            //string bg = UISpriteInspector.SpriteField(UISettings.atlas, "Background", mInputBG, GUILayout.Width(200f));
            //GUILayout.Space(20f);
            //GUILayout.Label("Sliced Sprite for the background");
            //GUILayout.EndHorizontal();
            NGUIEditorTools.DrawSpriteField("Background", "Normal state sprite", NGUISettings.atlas, mInputBG, OnInputBG, GUILayout.Width(120f));
		}

		if (ShouldCreate(go, UISettings.atlas != null && UISettings.font != null))
		{
			int depth = NGUITools.CalculateNextDepth(go);
			go = NGUITools.AddChild(go);
			go.name = "Input";

			float padding = 3f;

			UISlicedSprite bg = NGUITools.AddWidget<UISlicedSprite>(go);
			bg.name = "Background";
			bg.depth = depth;
			bg.atlas = UISettings.atlas;
			bg.spriteName = mInputBG;
			bg.pivot = UIWidget.Pivot.Left;
            //bg.transform.localScale = new Vector3(400f, UISettings.FontSize + padding * 2f, 1f);
            //bg.height = 52;
            bg.Dimensions = new Vector2(400f, 52f);
			bg.MakePixelPerfect();

			UILabel lbl = NGUITools.AddWidget<UILabel>(go);
			lbl.font = UISettings.font;
			lbl.pivot = UIWidget.Pivot.Left;
			lbl.transform.localPosition = new Vector3(padding, 0f, 0f);
			lbl.multiLine = false;
			lbl.supportEncoding = false;
			lbl.lineWidth = Mathf.RoundToInt(400f - padding * 2f);
			lbl.text = "";
            lbl.Dimensions = new Vector2(24f, 24f);
			lbl.MakePixelPerfect();

			// Add a collider to the background
			NGUITools.AddWidgetCollider(bg.gameObject);

			// Add an input script to the background and have it point to the label
			UIInput input = bg.gameObject.AddComponent<UIInput>();
			input.label = lbl;

			// Update the selection
			Selection.activeGameObject = go;
		}
	}

	/// <summary>
	/// Create a popup list or a menu.
	/// </summary>

	void CreatePopup (GameObject go, bool isDropDown)
	{
		if (UISettings.atlas != null)
		{
            NGUIEditorTools.DrawSpriteField("Foreground", "Foreground sprite (shown on the button)", UISettings.atlas, mListFG, (name) =>
            {
                if (name != mListFG) { mListFG = name; Save(); Repaint(); }
            }, GUILayout.Width(120f));

            NGUIEditorTools.DrawSpriteField("Background", "Foreground sprite (shown on the button)", UISettings.atlas, mListBG, (name) =>
            {
                if (name != mListBG) { mListBG = name; Save(); Repaint(); }
            }, GUILayout.Width(120f));

            NGUIEditorTools.DrawSpriteField("Highlight", "Sprite used to highlight the selected option", UISettings.atlas, mListHL, (name) =>
            {
                if (name != mListHL) { mListHL = name; Save(); Repaint(); }
            }, GUILayout.Width(120f));
		}

		if (ShouldCreate(go, UISettings.atlas != null && UISettings.font != null))
		{
			int depth = NGUITools.CalculateNextDepth(go);
			go = NGUITools.AddChild(go);
			go.name = isDropDown ? "Popup List" : "Popup Menu";

			// Background sprite
			UISprite sprite = NGUITools.AddSprite(go, UISettings.atlas, mListFG);
			sprite.depth = depth;
			sprite.atlas = UISettings.atlas;
            //sprite.transform.localScale = new Vector3(150f, 34f, 1f);
            sprite.Dimensions = new Vector2(150f, 34f);
			sprite.pivot = UIWidget.Pivot.Left;
			sprite.MakePixelPerfect();

			UIAtlas.Sprite sp = UISettings.atlas.GetSprite(mListFG);
			float padding = Mathf.Max(4f, sp.inner.xMin - sp.outer.xMin);

			// Text label
			UILabel lbl = NGUITools.AddWidget<UILabel>(go);
			lbl.font = UISettings.font;
			lbl.text = go.name;
			lbl.pivot = UIWidget.Pivot.Left;
			lbl.cachedTransform.localPosition = new Vector3(padding, 0f, 0f);
            lbl.Dimensions = new Vector2(24f, 24f);
			lbl.MakePixelPerfect();

			// Add a collider
			NGUITools.AddWidgetCollider(go);

			// Add the popup list
			UIPopupList list = go.AddComponent<UIPopupList>();
			list.atlas = UISettings.atlas;
			list.font = UISettings.font;
			list.backgroundSprite = mListBG;
			list.highlightSprite = mListHL;
			list.padding = new Vector2(padding, Mathf.RoundToInt(padding * 0.5f));
			if (isDropDown) list.textLabel = lbl;
			for (int i = 0; i < 5; ++i) list.items.Add(isDropDown ? ("List Option " + i) : ("Menu Option " + i));

			// Add the scripts
			go.AddComponent<UIButtonColor>().tweenTarget = sprite.gameObject;
			go.AddComponent<UIButtonSound>();

			Selection.activeGameObject = go;
		}
	}

	/// <summary>
	/// Repaint the window on selection.
	/// </summary>

	void OnSelectionChange () { Repaint(); }

	/// <summary>
	/// Draw the custom wizard.
	/// </summary>

	void OnGUI ()
	{
		// Load the saved preferences
		if (!mLoaded) { mLoaded = true; Load(); }

		NGUIEditorTools.SetLabelWidth(80f);
		GameObject go = NGUIEditorTools.SelectedRoot();

		if (go == null)
		{
			GUILayout.Label("You must create a UI first.");
			
			if (GUILayout.Button("Open the New UI Wizard"))
			{
				EditorWindow.GetWindow<UICreateNewUIWizard>(false, "New UI", true);
			}
		}
		else
		{
			GUILayout.Space(4f);

            GUILayout.BeginHorizontal();
            ComponentSelector.Draw<UIAtlas>(UISettings.atlas, OnSelectAtlas, GUILayout.Width(140f));
            GUILayout.Label("Texture atlas used by widgets", GUILayout.MinWidth(10000f));
            GUILayout.EndHorizontal();

			//GUILayout.BeginHorizontal();
			//ComponentSelectorNew.Draw<Font>(UISettings.font.TrueTypeFont, OnSelectFont, GUILayout.Width(140f));
			//GUILayout.Label("Font used by labels", GUILayout.MinWidth(10000f));
			//GUILayout.EndHorizontal();

			GUILayout.Space(-2f);
			NGUIEditorTools.DrawSeparator();

			GUILayout.BeginHorizontal();
			WidgetType wt = (WidgetType)EditorGUILayout.EnumPopup("Template", mType, GUILayout.Width(200f));
			GUILayout.Space(20f);
			GUILayout.Label("Select a widget template to use");
			GUILayout.EndHorizontal();

			if (mType != wt) { mType = wt; Save(); }

			switch (mType)
			{
				case WidgetType.Label:			CreateLabel(go); break;
				//case WidgetType.Sprite:			CreateSprite<UISprite>(go, ref mSprite); break;
				////case WidgetType.Particle:		CreateParticleWidget(go, ref mSprite); break;
				//case WidgetType.SlicedSprite:	CreateSprite<UISlicedSprite>(go, ref mSliced); break;
				//case WidgetType.TiledSprite:	CreateSprite<UITiledSprite>(go, ref mTiled); break;
				//case WidgetType.FilledSprite:	CreateSprite<UIFilledSprite>(go, ref mFilled); break;
				case WidgetType.Particle:			CreateSprite<UIParticle>(go, mParticle, OnParticle); break;
				case WidgetType.Sprite:			CreateSprite<UISprite>(go, mSprite, OnSprite); break;
				case WidgetType.ColorSprite: CreateSprite<UIColorSprite>(go, mSprite, OnColorSprite); break;
				case WidgetType.SlicedSprite:	CreateSprite<UISlicedSprite>(go, mSliced, OnSliced); break;
				case WidgetType.TiledSprite:	CreateSprite<UITiledSprite>(go, mTiled, OnTiled); break;
				case WidgetType.FilledSprite: CreateSprite<UIFilledSprite>(go, mFilled, OnFilled); break;
				case WidgetType.SimpleTexture:	CreateSimpleTexture(go); break;
				case WidgetType.Button:			CreateButton(go); break;
				case WidgetType.ImageButton:	CreateImageButton(go); break;
				case WidgetType.Checkbox:		CreateCheckbox(go); break;
				case WidgetType.ProgressBar:	CreateSlider(go, false); break;
				case WidgetType.Slider:			CreateSlider(go, true); break;
				case WidgetType.Input:			CreateInput(go); break;
				case WidgetType.PopupList:		CreatePopup(go, true); break;
				case WidgetType.PopupMenu:		CreatePopup(go, false); break;
				case WidgetType.RadioButton:    CreateRadioButton(go);break;
                case WidgetType.ComplexRadioButton: CreateComplexRadioButton(go); break;
			}
		}
	}
}
