//----------------------------------------------
//            NGUI: Next-Gen UI kit
// Copyright © 2011-2012 Tasharen Entertainment
//----------------------------------------------

using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// UI Atlas contains a collection of sprites inside one large texture atlas.
/// </summary>

[AddComponentMenu("NGUI/UI/Atlas")]
public class UIAtlas : MonoBehaviour
{
	[SerializeField]
	public string MaterialPath;

	[System.Serializable]
	public class Sprite
	{
		public string name = "Unity Bug";
		public Rect outer = new Rect(0f, 0f, 1f, 1f);
        public Rect outerOffset = new Rect(0f, 0f, 0f, 0f);
        public Rect oriOuter = new Rect(0f, 0f, 1f, 1f);
		public Rect inner = new Rect(0f, 0f, 1f, 1f);

		// Padding is needed for trimmed sprites and is relative to sprite width and height
		public float paddingLeft = 0f;
		public float paddingRight = 0f;
		public float paddingTop = 0f;
		public float paddingBottom = 0f;

        public float borderLeft { get { return inner.xMin - outer.xMin; } }
        public float borderRight { get { return outer.xMax - inner.xMax; } }
        public float borderTop { get { return inner.yMin - outer.yMin; } }
        public float borderBottom { get { return outer.yMax - inner.yMax; } }

		public bool hasPadding { get { return paddingLeft != 0f || paddingRight != 0f || paddingTop != 0f || paddingBottom != 0f; } }
	}

	/// <summary>
	/// Pixels coordinates are values within the texture specified in pixels. They are more intuitive,
	/// but will likely change if the texture gets resized. TexCoord coordinates range from 0 to 1,
	/// and won't change if the texture is resized. You can switch freely from one to the other prior
	/// to modifying the texture used by the atlas.
	/// </summary>

	public enum Coordinates
	{
		Pixels,
		TexCoords,
	}

	// Material used by this atlas. Name is kept only for backwards compatibility, it used to be public.
	[SerializeField]
	Material material;

	// List of all sprites inside the atlas. Name is kept only for backwards compatibility, it used to be public.
	[SerializeField]
	List<Sprite> sprites = new List<Sprite>();

	// Function For Search
	Dictionary<string, Sprite> spriteLinkedName = new Dictionary<string, Sprite>();

	// Currently active set of coordinates
	[SerializeField]
	Coordinates mCoordinates = Coordinates.Pixels;

	// Replacement atlas can be used to completely bypass this atlas, pulling the data from another one instead.
	[SerializeField] UIAtlas mReplacement;

    // Size in pixels for the sake of MakePixelPerfect functions.
    [HideInInspector] [SerializeField] float mPixelSize = 1f;

	/// <summary>
	/// Material used by the atlas.
	/// </summary>

	public Material spriteMaterial
	{
		get
		{

			Material mat = (mReplacement != null) ? mReplacement.spriteMaterial : material;
			//if(mat == null) mat = ResLoader.Load("Material/DefaultUI") as Material;
			return mat;
		}
		set
		{
			if (mReplacement != null)
			{
				mReplacement.spriteMaterial = value;
			}
			else
			{
				if (material == null)
				{
					material = value;
				}
				else
				{
					MarkAsDirty();
					material = value;
					MarkAsDirty();
				}
			}
		}
	}

	public bool IsNoLoaded()
	{
		if (spriteMaterial == null) return true;
		else if (spriteMaterial.name == "DefaultUI") return true;
		else return false;
	}

	/// <summary>
	/// List of sprites within the atlas.
	/// </summary>

	public List<Sprite> spriteList
	{
		get
		{
			return (mReplacement != null) ? mReplacement.spriteList : sprites;
		}
		set
		{
			if (mReplacement != null) mReplacement.spriteList = value;
			else sprites = value;
		}
	}

	/// <summary>
	/// Texture used by the atlas.
	/// </summary>

	public Texture texture { get { return (mReplacement != null) ? mReplacement.texture : (material != null ? material.mainTexture as Texture : null); } }

	/// <summary>
	/// Allows switching of the coordinate system from pixel coordinates to texture coordinates.
	/// </summary>

	public Coordinates coordinates
	{
		get
		{
			return (mReplacement == null) ? mCoordinates : mReplacement.mCoordinates;
		}
		set
		{
			if (mReplacement != null)
			{
				mReplacement.coordinates = value;
			}
			else if (mCoordinates != value)
			{
				if (material == null || material.mainTexture == null)
				{
					Debug.LogError("Can't switch coordinates until the atlas material has a valid texture");
					return;
				}

				mCoordinates = value;
				Texture tex = material.mainTexture;

				foreach (Sprite s in sprites)
				{
					if (mCoordinates == Coordinates.TexCoords)
					{
						s.outer = NGUIMath.ConvertToTexCoords(s.outer, tex.width, tex.height);
						s.inner = NGUIMath.ConvertToTexCoords(s.inner, tex.width, tex.height);
					}
					else
					{
						s.outer = NGUIMath.ConvertToPixels(s.outer, tex.width, tex.height, true);
						s.inner = NGUIMath.ConvertToPixels(s.inner, tex.width, tex.height, true);
					}
				}
			}
		}
	}

	/// <summary>
	/// Setting a replacement atlas value will cause everything using this atlas to use the replacement atlas instead.
	/// Suggested use: set up all your widgets to use a dummy atlas that points to the real atlas. Switching that atlas
	/// to another one (for example an HD atlas) is then a simple matter of setting this field on your dummy atlas.
	/// </summary>

	public UIAtlas replacement
	{
		get
		{
			return mReplacement;
		}
		set
		{
			if (mReplacement != value)
			{
				if (mReplacement != null) MarkAsDirty();
				mReplacement = value;
				MarkAsDirty();
			}
		}
	}


	public bool TryGetSprite(string name, out Sprite sprite)
	{
		sprite = GetSprite(name);
		if (sprite == null)
		{
			return false;
		}
		return true;
	}

	public bool ContainSprite(string name)
	{
		Sprite sprite = GetSprite(name);
		if (sprite == null)
		{
			return false;
		}
		return true;

	}

	/// <summary>
	/// Convenience function that retrieves a sprite by name.
	/// </summary>

	public Sprite GetSprite(string name)
	{
		if (mReplacement != null)
		{
			return mReplacement.GetSprite(name);
		}
		else if (!string.IsNullOrEmpty(name))
		{
			if (Application.isPlaying)
			{
				if (spriteLinkedName.Count <= 0)
				{
					for( int i = 0 ; i < sprites.Count ; i++ )
						spriteLinkedName.Add(sprites[i].name, sprites[i]);
				}
				
				Sprite outSprite;
				if ( !spriteLinkedName.TryGetValue(name , out outSprite) )
					return null;
				return outSprite;
			}
			else
			{
				for (int i = 0 ; i < sprites.Count ; i++ )
				{
					// string.Equals doesn't seem to work with Flash export
					if (!string.IsNullOrEmpty(sprites[i].name) && name == sprites[i].name)
					{
						return sprites[i];
					}
				}
			}
		}
		else
		{
			Debug.LogWarning("Expected a valid name, found nothing");
		}
		return null;
	}
	/// <summary>
	/// Function used for sorting in the GetListOfSprites() function below.
	/// </summary>

	static int CompareString(string a, string b) { return a.CompareTo(b); }

	///// <summary>
	///// Convenience function that retrieves a list of all sprite names.
	///// </summary>

	//public List<string> GetListOfSprites ()
	//{
	//    if (mReplacement != null) return mReplacement.GetListOfSprites();
	//    List<string> list = new List<string>();
	//    foreach (Sprite s in sprites) if (s != null && !string.IsNullOrEmpty(s.name)) list.Add(s.name);
	//    list.Sort();
	//    return list;
	//}

	public List<string> GetListOfSprites()
	{
		if (mReplacement != null) return mReplacement.GetListOfSprites();
		List<string> list = new List<string>();

		for (int i = 0, imax = sprites.Count; i < imax; ++i)
		{
			Sprite s = sprites[i];
			if (s != null && !string.IsNullOrEmpty(s.name)) list.Add(s.name);
		}
		//list.Sort(CompareString);
		list.Sort();
		return list;
	}

	/// <summary>
	/// Convenience function that retrieves a list of all sprite names that contain the specified phrase
	/// </summary>

	public List<string> GetListOfSprites(string match)
	{
		if (mReplacement != null) return mReplacement.GetListOfSprites(match);
		if (string.IsNullOrEmpty(match)) return GetListOfSprites();
		List<string> list = new List<string>();

		// First try to find an exact match
		for (int i = 0, imax = sprites.Count; i < imax; ++i)
		{
			Sprite s = sprites[i];

			if (s != null && !string.IsNullOrEmpty(s.name) && string.Equals(match, s.name, StringComparison.OrdinalIgnoreCase))
			{
				list.Add(s.name);
				return list;
			}
		}

		// No exact match found? Split up the search into space-separated components.
		string[] keywords = match.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
		for (int i = 0; i < keywords.Length; ++i) keywords[i] = keywords[i].ToLower();

		// Try to find all sprites where all keywords are present
		for (int i = 0, imax = sprites.Count; i < imax; ++i)
		{
			Sprite s = sprites[i];

			if (s != null && !string.IsNullOrEmpty(s.name))
			{
				string tl = s.name.ToLower();
				int matches = 0;

				for (int b = 0; b < keywords.Length; ++b)
				{
					if (tl.Contains(keywords[b])) ++matches;
				}
				if (matches == keywords.Length) list.Add(s.name);
			}
		}
		//list.Sort(CompareString);
		return list;
	}


	/// <summary>
	/// Helper function that determines whether the atlas uses the specified one, taking replacements into account.
	/// </summary>

	bool References(UIAtlas atlas)
	{
		if (atlas == null) return false;
		if (atlas == this) return true;
		return (mReplacement != null) ? mReplacement.References(atlas) : false;
	}

	/// <summary>
	/// Helper function that determines whether the two atlases are related.
	/// </summary>

	static public bool CheckIfRelated(UIAtlas a, UIAtlas b)
	{
		if (a == null || b == null) return false;
		return a == b || a.References(b) || b.References(a);
	}

	/// <summary>
	/// Mark all widgets associated with this atlas as having changed.
	/// </summary>

	public void MarkAsDirty()
	{
		UISprite[] list = (UISprite[])GameObject.FindObjectsOfType(typeof(UISprite));

		foreach (UISprite sp in list)
		{
			if (CheckIfRelated(this, sp.atlas))
			{
				sp.atlas = null;
				sp.atlas = this;
#if UNITY_EDITOR
				UnityEditor.EditorUtility.SetDirty(sp);
#endif
			}
		}

		/*
				UIFont[] fonts = (UIFont[])GameObject.FindObjectsOfType(typeof(UIFont));

				foreach (UIFont font in fonts)
				{
					if (CheckIfRelated(this, font.atlas))
					{
						font.atlas = null;
						font.atlas = this;
		#if UNITY_EDITOR
						UnityEditor.EditorUtility.SetDirty(font);
		#endif
					}
				}
		*/

		UILabel[] labels = (UILabel[])GameObject.FindObjectsOfType(typeof(UILabel));

		foreach (UILabel lbl in labels)
		{
			if (lbl.font != null/* && CheckIfRelated(this, lbl.font.atlas)*/)
			{
				UIFont font = lbl.font;
				lbl.font = null;
				lbl.font = font;
#if UNITY_EDITOR
				UnityEditor.EditorUtility.SetDirty(lbl);
#endif

			}
		}

	}

    /// <summary>
    /// Pixel size is a multiplier applied to widgets dimensions when performing MakePixelPerfect() pixel correction.
    /// Most obvious use would be on retina screen displays. The resolution doubles, but with UIRoot staying the same
    /// for layout purposes, you can still get extra sharpness by switching to an HD atlas that has pixel size set to 0.5.
    /// </summary>

    public float pixelSize
    {
        get
        {
            return (mReplacement != null) ? mReplacement.pixelSize : mPixelSize;
        }
        set
        {
            if (mReplacement != null)
            {
                mReplacement.pixelSize = value;
            }
            else
            {
                float val = Mathf.Clamp(value, 0.25f, 4f);

                if (mPixelSize != val)
                {
                    mPixelSize = val;
                    MarkAsDirty();
                }
            }
        }
    }
}
