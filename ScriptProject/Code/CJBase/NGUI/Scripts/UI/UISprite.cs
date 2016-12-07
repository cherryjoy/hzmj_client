//----------------------------------------------
//            NGUI: Next-Gen UI kit
// Copyright © 2011-2012 Tasharen Entertainment
//----------------------------------------------

using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Very simple UI sprite -- a simple quad of specified size, drawn using a part of the texture atlas.
/// </summary>

[ExecuteInEditMode]
[AddComponentMenu("NGUI/UI/Sprite (Basic)")]
public class UISprite : UIWidget
{
	// Cached and saved values
	[SerializeField] UIAtlas mAtlas;
	[SerializeField] string mSpriteName;

	protected UIAtlas.Sprite mSprite;
	protected Rect mOuter;
	protected Rect mOuterUV;

	// BUG: There is a bug in Unity 3.4.2 and all the way up to 3.5 b7 -- when instantiating from prefabs,
	// for some strange reason classes get initialized with default values. So for example, 'mSprite' above
	// gets initialized as if it was created with 'new UIAtlas.Sprite()' instead of 'null'. Fun, huh?

	bool mSpriteSet = false;
	string mLastName = "";

    //keep sprite drawingDimensions to it's original sprite size
    public bool IsKeepOriSize = false;

	/// <summary>
	/// Outer set of UV coordinates.
	/// </summary>

	public Rect outerUV { get { UpdateUVs(); return mOuterUV; } }

	/// <summary>
	/// Atlas used by this widget.
	/// </summary>
 
	public UIAtlas atlas
	{
		get
		{
			return mAtlas;
		}
		set
		{
			if (mAtlas != value)
			{
				mAtlas = value;

				// Update the material
				material = (mAtlas != null) ? mAtlas.spriteMaterial : null;

				// Automatically choose the first sprite
				if (string.IsNullOrEmpty(mSpriteName))
				{
					if (mAtlas != null && mAtlas.spriteList.Count > 0)
					{
						sprite = mAtlas.spriteList[0];
						mSpriteName = mSprite.name;
					}
				}

				// Re-link the sprite
				if (!string.IsNullOrEmpty(mSpriteName))
				{
					string sprite = mSpriteName;
					mSpriteName = "";
					spriteName = sprite;
					mChanged = true;
					mOuter = new Rect();
					UpdateUVs();
				}
			}
		}
	}

	/// <summary>
	/// Sprite within the atlas used to draw this widget.
	/// </summary>
 
	public string spriteName
	{
		get
		{
			return mSpriteName;
		}
		set
		{
			if (string.IsNullOrEmpty(value))
			{
				// If the sprite name hasn't been set yet, no need to do anything
				if (string.IsNullOrEmpty(mSpriteName)) return;

				// Clear the sprite name and the sprite reference
				mSpriteName = "";
				sprite = null;
				mChanged = true;
			}
			else if (mSpriteName != value)
			{
				// If the sprite name changes, the sprite reference should also be updated
				mSpriteName = value;
				sprite = (mAtlas != null) ? mAtlas.GetSprite(mSpriteName) : null;
				mChanged = true;
				if (mSprite != null) 
                    UpdateUVs();
			}
		}
	}

	/// <summary>
	/// Get the sprite used by the atlas. Work-around for a bug in Unity.
	/// </summary>

	protected UIAtlas.Sprite sprite
	{
		get
		{
			if (!mSpriteSet) mSprite = null;
			return mSprite;
		}
		set
		{
			mSprite = value;
			mSpriteSet = true;
		}
	}

	/// <summary>
	/// Helper function that calculates the relative offset based on the current pivot.
	/// </summary>

	override public Vector2 pivotOffset
	{
		get
		{
			if (sprite == null && mAtlas != null && !string.IsNullOrEmpty(mSpriteName))
			{
				sprite = mAtlas.GetSprite(mSpriteName);
			}

			Vector2 v = Vector2.zero;

			if (mSprite != null)
			{
				Pivot pv = pivot;
                if (pv == Pivot.Top || pv == Pivot.Center || pv == Pivot.Bottom) v.x = (-1f - mSprite.paddingRight + mSprite.paddingLeft) * 0.5f;
                else if (pv == Pivot.TopRight || pv == Pivot.Right || pv == Pivot.BottomRight) v.x = -1f - mSprite.paddingRight;
                else v.x = mSprite.paddingLeft;

                if (pv == Pivot.Left || pv == Pivot.Center || pv == Pivot.Right) v.y = (1f + mSprite.paddingBottom - mSprite.paddingTop) * 0.5f;
                else if (pv == Pivot.BottomLeft || pv == Pivot.Bottom || pv == Pivot.BottomRight) v.y = 1f + mSprite.paddingBottom;
                else v.y = -mSprite.paddingTop;
			}
			return v;
		}
	}

	/// <summary>
	/// Update the texture UVs used by the widget.
	/// </summary>

	virtual public void UpdateUVs()
	{
		Init();

		if (sprite != null && mOuter != mSprite.outer)
		{
			Texture tex = mainTexture;

			if (tex != null)
			{
				mOuter = mSprite.outer;
				mOuterUV = mOuter;

				if (mAtlas.coordinates == UIAtlas.Coordinates.Pixels)
				{
					mOuterUV = NGUIMath.ConvertToTexCoords(mOuterUV, tex.width, tex.height);
				}
				mChanged = true;
			}
		}
	}


    virtual public void MakePixelPerfectWithSizeInAtlas()
    {
        Texture tex = mainTexture;

        int width = Mathf.RoundToInt(Mathf.Abs(outerUV.width * tex.width));
        int height = Mathf.RoundToInt(Mathf.Abs(outerUV.height * tex.height));
        SetDimesions(width, height);
    }

	/// <summary>
	/// Adjust the scale of the widget to make it pixel-perfect.
	/// </summary>

	override public void MakePixelPerfect ()
	{
//		Texture tex = mainTexture;
//
//		if (tex != null)
//		{
//			Rect rect = NGUIMath.ConvertToPixels(outerUV, tex.width, tex.height, true);
//			Vector3 scale = cachedTransform.localScale;
//			scale.x = rect.width;
//			scale.y = rect.height;
//			scale.z = 1f;
//			cachedTransform.localScale = scale;
//		}
		base.MakePixelPerfect();
	}

	/// <summary>
	/// Ensure that the sprite has been initialized properly.
	/// This is necessary because the order of execution is unreliable.
	/// Sometimes the sprite's functions may be called prior to Start().
	/// </summary>

	protected void Init ()
	{
		if (mAtlas != null)
		{
			if (material == null) material = mAtlas.spriteMaterial;
			if (sprite == null) sprite = string.IsNullOrEmpty(mSpriteName) ? null : mAtlas.GetSprite(mSpriteName);
		}
	}

	/// <summary>
	/// Set the atlas and the sprite.
	/// </summary>

	override protected void OnStart ()
	{
        if (IsKeepOriSize)
        {
            Texture2D tex = mainTexture as Texture2D;
            Dimensions = new Vector2(Mathf.RoundToInt(outerUV.width * tex.width), Mathf.RoundToInt(outerUV.height * tex.height));
        }
       		
        if (mAtlas != null)
		{			
			UpdateUVs();	
		}
	}

	/// <summary>
	/// Update the UV coordinates.
	/// </summary>

	override public bool OnUpdate ()
    {
		if (mLastName != mSpriteName)
		{
			mSprite = null;
			mChanged = true;
			mLastName = mSpriteName;
			UpdateUVs();
			return true;
		}
		//UpdateUVs();
		return false;
	}

    /// <summary>
    /// Sprite's dimensions used for drawing. X = left, Y = bottom, Z = right, W = top.
    /// This function automatically adds 1 pixel on the edge if the sprite's dimensions are not even.
    /// It's used to achieve pixel-perfect sprites even when an odd dimension sprite happens to be centered.
    /// </summary>

    public override Vector4 drawingDimensions
    {
        get
        {
            Vector2 offset = pivotOffset;

            float x0 = offset.x * mWidth;
            float y0 = (offset.y - 1) * mHeight;
            float x1 = x0 + mWidth;
            float y1 = offset.y * mHeight;

            if (sprite != null)
            {
                int padLeft = (int)sprite.paddingLeft;
                int padBottom = (int)sprite.paddingBottom;
                int padRight = (int)sprite.paddingRight;
                int padTop = (int)sprite.paddingTop;

                int w = (int)sprite.outer.width + padLeft + padRight;
                int h = (int)sprite.outer.height + padBottom + padTop;


//                 if ((w & 1) != 0) ++padRight;
//                 if ((h & 1) != 0) ++padTop;

                float px = (1f / w) * mWidth;
                float py = (1f / h) * mHeight;

                y0 += padBottom * py;
                y1 -= padTop * py;
            }

            if (atlas == null)
                Debug.Log(gameObject.name + "lose the atlas!");

            float atlaPixelSize = atlas == null ? 0 : atlas.pixelSize;
            Vector4 br = border * atlaPixelSize;

            float fw = br.x + br.z;
            float fh = br.y + br.w;

            float vx = Mathf.Lerp(x0, x1 - fw, mDrawRegion.x);
            float vy = Mathf.Lerp(y0, y1 - fh, mDrawRegion.y);
            float vz = mDrawRegion.z==0 ? x0:Mathf.Lerp(x0 + fw, x1, mDrawRegion.z);
            float vw = Mathf.Lerp(y0 + fh, y1, mDrawRegion.w);

            return new Vector4(vx, vy, vz, vw);
        }
    }

	/// <summary>
	/// Virtual function called by the UIScreen that fills the buffers.
	/// </summary>

	override public void OnFill (BetterList<Vector3> verts, BetterList<Vector2> uvs, BetterList<Color> cols)
	{
		Vector2 uv0 = new Vector2(mOuterUV.xMin, mOuterUV.yMin);
		Vector2 uv1 = new Vector2(mOuterUV.xMax, mOuterUV.yMax);

        Vector4 v = drawingDimensions;

        if (v.x == v.z || v.y == v.w)
            return;

        verts.Add(new Vector3(v.x, v.y));
        verts.Add(new Vector3(v.x, v.w));
        verts.Add(new Vector3(v.z, v.w));
        verts.Add(new Vector3(v.z, v.y));

        uvs.Add(uv0);
        uvs.Add(new Vector2(uv0.x, uv1.y));
        uvs.Add(uv1);
        uvs.Add(new Vector2(uv1.x, uv0.y));

		cols.Add(color);
		cols.Add(color);
		cols.Add(color);
		cols.Add(color);
	}

	public Vector3 GetSpriteSize()
	{
		Texture2D tex = this.mainTexture as Texture2D;
		int x = Mathf.RoundToInt(Mathf.Abs(this.outerUV.width * tex.width));
		int y = Mathf.RoundToInt(Mathf.Abs(this.outerUV.height * tex.height));
		return new Vector3(x, y, 1);
	}

    public Vector2 GetSpriteSizeByName(string spriteName)
    {
        if (mAtlas == null)
        {
            Debug.Log("atlas is null!!");
            return Vector2.zero;
        }
        UIAtlas.Sprite sprite = mAtlas.GetSprite(spriteName);
        if (sprite == null)
        {
            Debug.Log("GetSpriteSizeByName Wrong!Wrong spriteName.");
            return Vector2.zero;
        }
        Texture tex = mainTexture;
        Rect wantedOuterUV = sprite.outer;
        wantedOuterUV = NGUIMath.ConvertToTexCoords(wantedOuterUV, tex.width, tex.height);
        int x = Mathf.RoundToInt(Mathf.Abs(wantedOuterUV.width * tex.width));
        int y = Mathf.RoundToInt(Mathf.Abs(wantedOuterUV.height * tex.height));
        return new Vector3(x, y);

    }
}