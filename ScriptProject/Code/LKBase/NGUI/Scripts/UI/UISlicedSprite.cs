//----------------------------------------------
//            NGUI: Next-Gen UI kit
// Copyright © 2011-2012 Tasharen Entertainment
//----------------------------------------------

using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// 9-sliced widget component used to draw large widgets using small textures.
/// </summary>

[ExecuteInEditMode]
[AddComponentMenu("NGUI/UI/Sprite (Sliced)")]
public class UISlicedSprite : UISprite
{
	[SerializeField] bool mFillCenter = true;

	protected Rect mInner;
	protected Rect mInnerUV;
	protected Vector3 mScale = Vector3.one;

	/// <summary>
	/// Inner set of UV coordinates.
	/// </summary>

	public Rect innerUV { get { UpdateUVs(); return mInnerUV; } }

	/// <summary>
	/// Whether the center part of the sprite will be filled or not. Turn it off if you want only to borders to show up.
	/// </summary>

    public bool fillCenter { get { return mFillCenter; } set { if (mFillCenter != value) { mFillCenter = value; MarkAsChanged(); } } }

	/// <summary>
	/// Update the texture UVs used by the widget.
	/// </summary>

    override public void UpdateUVs()
    {
        Init();
        Texture tex = mainTexture;

        if (tex != null && sprite != null)
        {
            mOuterUV.Set(mSprite.outer.x, mSprite.outer.y, mSprite.outer.width, mSprite.outer.height);
            mInnerUV.Set(mSprite.outer.x + mSprite.borderLeft, mSprite.outer.y + mSprite.borderTop, mSprite.outer.width - mSprite.borderLeft - mSprite.borderRight, mSprite.outer.height - mSprite.borderBottom - mSprite.borderTop);

            mOuterUV = NGUIMath.ConvertToTexCoords(mOuterUV, tex.width, tex.height);
            mInnerUV = NGUIMath.ConvertToTexCoords(mInnerUV, tex.width, tex.height);

            mChanged = true;
        }
    }

	/// <summary>
	/// Sliced sprite shouldn't inherit the sprite's changes to this function.
	/// </summary>

	override public void MakePixelPerfect ()
	{
        //Vector3 pos = cachedTransform.localPosition;
        //pos.x = Mathf.RoundToInt(pos.x);
        //pos.y = Mathf.RoundToInt(pos.y);
        //pos.z = Mathf.RoundToInt(pos.z);
        //cachedTransform.localPosition = pos;

        //Vector3 scale = cachedTransform.localScale;
        //scale.x = Mathf.RoundToInt(scale.x * 0.5f) << 1;
        //scale.y = Mathf.RoundToInt(scale.y * 0.5f) << 1;
        //scale.z = 1f;
        //cachedTransform.localScale = scale;

        base.MakePixelPerfect();
	}

    override public void MakePixelPerfectWithSizeInAtlas()
    {
        Texture tex = mainTexture;
        int width = Mathf.RoundToInt(Mathf.Abs(mOuterUV.width * tex.width));
        int height = Mathf.RoundToInt(Mathf.Abs(mOuterUV.height * tex.height));
        SetDimesions(width, height);
    }

	override public bool OnUpdate()
	{
		//UpdateUVs();
		return false;
	}

    //we don't need invoke UpdateUVs every frame,we only need call it When it's localScale changed
    //call this function to change SlicedSprite's LocalScale INSTEAD OF Assigning it directly
    public void SetSize(Vector3 newSize){
        transform.localScale = newSize;
        UpdateUVs();
    }

#region Various fill functions

    /// <summary>
    /// Minimum allowed width for this widget.
    /// </summary>

    override public int minWidth
    {
        get
        {
            Vector4 b = border;
            if (atlas != null) b *= atlas.pixelSize;
            int min = Mathf.RoundToInt(b.x + b.z);

            UIAtlas.Sprite sp = sprite;
            if (sp != null) min += (int)(sp.paddingLeft + sp.paddingRight);
            return Mathf.Max(base.minWidth, ((min & 1) == 1) ? min + 1 : min);
        }
    }

    /// <summary>
    /// Minimum allowed height for this widget.
    /// </summary>

    override public int minHeight
    {
        get
        {
            Vector4 b = border;
            if (atlas != null) b *= atlas.pixelSize;
            int min = Mathf.RoundToInt(b.y + b.w);

            UIAtlas.Sprite sp = sprite;
            if (sp != null) min += (int)(sp.paddingTop + sp.paddingBottom);
            return Mathf.Max(base.minHeight, ((min & 1) == 1) ? min + 1 : min);
        }
    }

    // Static variables to reduce garbage collection
    static Vector2[] mTempPos = new Vector2[4];
    static Vector2[] mTempUVs = new Vector2[4];

    /// <summary>
    /// Sliced sprites generally have a border. X = left, Y = bottom, Z = right, W = top.
    /// </summary>

    override public Vector4 border
    {
        get
        {
            UIAtlas.Sprite sp = sprite;
            if (sp == null) return Vector2.zero;
            return new Vector4(sp.borderLeft, sp.borderBottom, sp.borderRight, sp.borderTop);
        }
    }

#endregion

    /// <summary>
	/// Draw the widget.
	/// </summary>

	override public void OnFill (BetterList<Vector3> verts, BetterList<Vector2> uvs, BetterList<Color> cols)
	{
		if (mOuterUV == mInnerUV)
		{
			base.OnFill(verts, uvs, cols);
			return;
		}

        Vector4 dr = drawingDimensions;

        if (dr.x == dr.z || dr.y == dr.w)
            return;

        Vector4 br = border * atlas.pixelSize;

        mTempPos[0].x = dr.x;
        mTempPos[0].y = dr.y;
        mTempPos[3].x = dr.z;
        mTempPos[3].y = dr.w;

        mTempPos[1].x = mTempPos[0].x + br.x;
        mTempPos[1].y = mTempPos[0].y + br.y;
        mTempPos[2].x = mTempPos[3].x - br.z;
        mTempPos[2].y = mTempPos[3].y - br.w;

        mTempUVs[0] = new Vector2(mOuterUV.xMin, mOuterUV.yMin);
        mTempUVs[1] = new Vector2(mInnerUV.xMin, mInnerUV.yMin);
        mTempUVs[2] = new Vector2(mInnerUV.xMax, mInnerUV.yMax);
        mTempUVs[3] = new Vector2(mOuterUV.xMax, mOuterUV.yMax);

        for (int x = 0; x < 3; ++x)
        {
            int x2 = x + 1;

            for (int y = 0; y < 3; ++y)
            {
                if (!mFillCenter && x == 1 && y == 1) continue;

                int y2 = y + 1;

                verts.Add(new Vector3(mTempPos[x].x, mTempPos[y].y));
                verts.Add(new Vector3(mTempPos[x].x, mTempPos[y2].y));
                verts.Add(new Vector3(mTempPos[x2].x, mTempPos[y2].y));
                verts.Add(new Vector3(mTempPos[x2].x, mTempPos[y].y));

                uvs.Add(new Vector2(mTempUVs[x].x, mTempUVs[y].y));
                uvs.Add(new Vector2(mTempUVs[x].x, mTempUVs[y2].y));
                uvs.Add(new Vector2(mTempUVs[x2].x, mTempUVs[y2].y));
                uvs.Add(new Vector2(mTempUVs[x2].x, mTempUVs[y].y));


                cols.Add(color);
                cols.Add(color);
                cols.Add(color);
                cols.Add(color);
            }
        }
	}
}
