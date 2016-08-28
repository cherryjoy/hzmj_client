//----------------------------------------------
//            NGUI: Next-Gen UI kit
// Copyright © 2011-2012 Tasharen Entertainment
//----------------------------------------------

using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

/// <summary>
/// Unity doesn't keep the values of static variables after scripts change get recompiled. One way around this
/// is to store the references in PlayerPrefs -- retrieve them at start, and save them whenever something changes.
/// </summary>

public class UISettings
{
	static UIWidget.Pivot mPivot = UIWidget.Pivot.Center;
	static bool mLoaded = false;
	static UIFont mFont;
	static int mFontSize = 24;
	static UIAtlas mAtlas;
	static TextAsset mFontData;
	static Texture2D mFontTexture;
	static string mFontName = "New Font";
	static string mAtlasName = "New Atlas";
	static bool mPreview = true;
	static bool mUnityPacking = true;
	static int mAtlasPadding = 1;
    static public bool mAtlasTrimming = true;
	static string mPartial = "";
	

	static Object GetObject (string name)
	{
		int assetID = PlayerPrefs.GetInt(name, -1);
		return (assetID != -1) ? EditorUtility.InstanceIDToObject(assetID) : null;
	}

	static void Load ()
	{
		mLoaded			= true;
		mFontName		= PlayerPrefs.GetString("NGUI Font Name");
		mAtlasName		= PlayerPrefs.GetString("NGUI Atlas Name");
		mFontData		= GetObject("NGUI Font Asset") as TextAsset;
		mFontTexture	= GetObject("NGUI Font Texture") as Texture2D;
		mAtlas			= GetObject("NGUI Atlas") as UIAtlas;
		mPreview		= PlayerPrefs.GetInt("NGUI Preview") == 0;
		mUnityPacking = EditorPrefs.GetBool("NGUI Unity Packing", false);
		mAtlasPadding = EditorPrefs.GetInt("NGUI Atlas Padding", 1);
        mAtlasTrimming = EditorPrefs.GetBool("NGUI Atlas Trimming", true);
		mPartial = EditorPrefs.GetString("NGUI Partial");
		mPivot = (UIWidget.Pivot)EditorPrefs.GetInt("NGUI Pivot", (int)mPivot);
	}

	static void Save ()
	{
		PlayerPrefs.SetString("NGUI Font Name", mFontName);
		PlayerPrefs.SetString("NGUI Atlas Name", mAtlasName);
		PlayerPrefs.SetInt("NGUI Font Asset", (mFontData != null) ? mFontData.GetInstanceID() : -1);
		PlayerPrefs.SetInt("NGUI Font Texture", (mFontTexture != null) ? mFontTexture.GetInstanceID() : -1);
		PlayerPrefs.SetInt("NGUI Atlas", (mAtlas != null) ? mAtlas.GetInstanceID() : -1);
		PlayerPrefs.SetInt("NGUI Preview", mPreview ? 0 : 1);
		EditorPrefs.SetBool("NGUI Unity Packing", mUnityPacking);
		EditorPrefs.SetInt("NGUI Atlas Padding", mAtlasPadding);
        EditorPrefs.SetBool("NGUI Atlas Trimming", mAtlasTrimming);
		EditorPrefs.SetString("NGUI Partial", mPartial);
		EditorPrefs.SetInt("NGUI Pivot", (int)mPivot);
	}

	/// <summary>
	/// Default pivot point used by sprites.
	/// </summary>

	static public UIWidget.Pivot pivot
	{
		get
		{
			if (!mLoaded) Load();
			return mPivot;
		}
		set
		{
			if (mPivot != value)
			{
				mPivot = value;
				Save();
			}
		}
	}

	/// <summary>
	/// Default font used by NGUI.
	/// </summary>

	static public UIFont font
	{
		get
		{
			if (mFont == null)
			{
				mFont = UIFont.CreateInstance<UIFont>();
			}
			return mFont;
		}
		set
		{
			if (mFont != value)
			{
				mFont = value;
				Save();
			}
		}
	}

	static public int FontSize
	{
		get { return mFontSize; }
	}
	/// <summary>
	/// Default atlas used by NGUI.
	/// </summary>

	static public UIAtlas atlas
	{
		get
		{
			if (!mLoaded) Load();
			return mAtlas;
		}
		set
		{
			if (mAtlas != value)
			{
				mAtlas = value;
				mAtlasName = (mAtlas != null) ? mAtlas.name : "New Atlas";
				Save();
			}
		}
	}


	/// <summary>
	/// Name of the partial sprite name, used to filter sprites.
	/// </summary>

	static public string partialSprite
	{
		get
		{
			if (!mLoaded) Load();
			return mPartial;
		}
		set
		{
			if (mPartial != value)
			{
				mPartial = value;
				EditorPrefs.SetString("NGUI Partial", mPartial);
			}
		}
	}

	/// <summary>
	/// Name of the font, used by the Font Maker.
	/// </summary>

	static public string fontName { get { if (!mLoaded) Load(); return mFontName; } set { if (mFontName != value) { mFontName = value; Save(); } } }

	/// <summary>
	/// Data used to create the font, used by the Font Maker.
	/// </summary>

	static public TextAsset fontData { get { if (!mLoaded) Load(); return mFontData; } set { if (mFontData != value) { mFontData = value; Save(); } } }

	/// <summary>
	/// Texture used to create the font, used by the Font Maker.
	/// </summary>

	static public Texture2D fontTexture { get { if (!mLoaded) Load(); return mFontTexture; } set { if (mFontTexture != value) { mFontTexture = value; Save(); } } }

	/// <summary>
	/// Name of the atlas, used by the Atlas maker.
	/// </summary>

	static public string atlasName { get { if (!mLoaded) Load(); return mAtlasName; } set { if (mAtlasName != value) { mAtlasName = value; Save(); } } }

	/// <summary>
	/// Whether the texture preview will be shown.
	/// </summary>

	static public bool texturePreview { get { if (!mLoaded) Load(); return mPreview; } set { if (mPreview != value) { mPreview = value; Save(); } } }

	/// <summary>
	/// Whether Unity's method or MaxRectBinPack will be used when creating an atlas
	/// </summary>

	static public bool unityPacking { get { if (!mLoaded) Load(); return mUnityPacking; } set { if (mUnityPacking != value) { mUnityPacking = value; Save(); } } }

	/// <summary>
	/// Added padding in-between of sprites when creating an atlas.
	/// </summary>

	static public int atlasPadding { get { if (!mLoaded) Load(); return mAtlasPadding; } set { if (mAtlasPadding != value) { mAtlasPadding = value; Save(); } } }

    /// <summary>
    /// Whether the transparent pixels will be trimmed away when creating an atlas.
    /// </summary>

    static public bool atlasTrimming { get { if (!mLoaded) Load(); return mAtlasTrimming; } set { if (mAtlasTrimming != value) { mAtlasTrimming = value; Save(); } } }
}
