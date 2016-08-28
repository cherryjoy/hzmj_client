//----------------------------------------------
//            NGUI: Next-Gen UI kit
// Copyright ?2011-2012 Tasharen Entertainment
//----------------------------------------------

using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Similar to a regular UISprite, but lets you only display a part of it. Great for progress bars, sliders, and alike.
/// Originally contributed by David Whatley.
/// </summary>

[ExecuteInEditMode]
[AddComponentMenu("NGUI/UI/Sprite (Filled)")]
public class UIFilledSprite : UISprite
{
	public enum FillDirection
	{
        Horizontal,
        Vertical,
        Radial90,
        Radial180,
        Radial360,
	}

    [SerializeField]FillDirection mFillDirection = FillDirection.Horizontal;
	[SerializeField] float mFillAmount = 1.0f;
    [HideInInspector][SerializeField] bool mInvert = false;
    // Static variables to reduce garbage collection
    static Vector2[] mTempPos = new Vector2[4];
    static Vector2[] mTempUVs = new Vector2[4];

	/// <summary>
	/// Direction of the cut procedure.
	/// </summary>

	public FillDirection fillDirection
	{
		get
		{
			return mFillDirection;
		}
		set
		{
			if (mFillDirection != value)
			{
				mFillDirection = value;
				mChanged = true;
			}
		}
	}

	/// <summary>
	/// Amount of the sprite shown. 0-1 range with 0 being nothing shown, and 1 being the full sprite.
	/// </summary>

	public float fillAmount
	{
		get
		{
			return mFillAmount;
		}
		set
		{
			float val = Mathf.Clamp01(value);

			if (mFillAmount != val)
			{
				mFillAmount = val;
				mChanged = true;
			}
		}
	}

    /// <summary>
    /// Whether the sprite should be filled in the opposite direction.
    /// </summary>

    public bool invert
    {
        get
        {
            return mInvert;
        }
        set
        {
            if (mInvert != value)
            {
                mInvert = value;
                mChanged = true;
            }
        }
    }

	/// <summary>
	/// Virtual function called by the UIScreen that fills the buffers.
	/// </summary>

	override public void OnFill (BetterList<Vector3> verts, BetterList<Vector2> uvs, BetterList<Color> cols)
	{
        if (mFillAmount < 0.001f) return;
        Vector4 v = drawingDimensions;

        if (v.x == v.z || v.y == v.w)
            return;

        float tx0 = mOuterUV.xMin;
        float ty0 = mOuterUV.yMin;
        float tx1 = mOuterUV.xMax;
        float ty1 = mOuterUV.yMax;

        // Horizontal and vertical filled sprites are simple -- just end the sprite prematurely
        if (mFillDirection == FillDirection.Horizontal || mFillDirection == FillDirection.Vertical)
        {
            if (mFillDirection == FillDirection.Horizontal)
            {
                float fill = (tx1 - tx0) * mFillAmount;

                if (mInvert)
                {
                    v.x = v.z - (v.z - v.x) * mFillAmount;
                    tx0 = tx1 - fill;
                }
                else
                {
                    v.z = v.x + (v.z - v.x) * mFillAmount;
                    tx1 = tx0 + fill;
                }
            }
            else if (mFillDirection == FillDirection.Vertical)
            {
                float fill = (ty1 - ty0) * mFillAmount;

                if (mInvert)
                {
                    v.y = v.w - (v.w - v.y) * mFillAmount;
                    ty0 = ty1 - fill;
                }
                else
                {
                    v.w = v.y + (v.w - v.y) * mFillAmount;
                    ty1 = ty0 + fill;
                }
            }
        }

        mTempPos[0] = new Vector2(v.x, v.y);
        mTempPos[1] = new Vector2(v.x, v.w);
        mTempPos[2] = new Vector2(v.z, v.w);
        mTempPos[3] = new Vector2(v.z, v.y);

        mTempUVs[0] = new Vector2(tx0, ty0);
        mTempUVs[1] = new Vector2(tx0, ty1);
        mTempUVs[2] = new Vector2(tx1, ty1);
        mTempUVs[3] = new Vector2(tx1, ty0);

        if (mFillAmount < 1f)
        {
            if (mFillDirection == FillDirection.Radial90)
            {
                if (RadialCut(mTempPos, mTempUVs, mFillAmount, mInvert, 0))
                {
                    for (int i = 0; i < 4; ++i)
                    {
                        verts.Add(mTempPos[i]);
                        uvs.Add(mTempUVs[i]);
                        cols.Add(color);
                    }
                }
                return;
            }

            if (mFillDirection == FillDirection.Radial180)
            {
                for (int side = 0; side < 2; ++side)
                {
                    float fx0, fx1, fy0, fy1;

                    fy0 = 0f;
                    fy1 = 1f;

                    if (side == 0) { fx0 = 0f; fx1 = 0.5f; }
                    else { fx0 = 0.5f; fx1 = 1f; }

                    mTempPos[0].x = Mathf.Lerp(v.x, v.z, fx0);
                    mTempPos[1].x = mTempPos[0].x;
                    mTempPos[2].x = Mathf.Lerp(v.x, v.z, fx1);
                    mTempPos[3].x = mTempPos[2].x;

                    mTempPos[0].y = Mathf.Lerp(v.y, v.w, fy0);
                    mTempPos[1].y = Mathf.Lerp(v.y, v.w, fy1);
                    mTempPos[2].y = mTempPos[1].y;
                    mTempPos[3].y = mTempPos[0].y;

                    mTempUVs[0].x = Mathf.Lerp(tx0, tx1, fx0);
                    mTempUVs[1].x = mTempUVs[0].x;
                    mTempUVs[2].x = Mathf.Lerp(tx0, tx1, fx1);
                    mTempUVs[3].x = mTempUVs[2].x;

                    mTempUVs[0].y = Mathf.Lerp(ty0, ty1, fy0);
                    mTempUVs[1].y = Mathf.Lerp(ty0, ty1, fy1);
                    mTempUVs[2].y = mTempUVs[1].y;
                    mTempUVs[3].y = mTempUVs[0].y;

                    float val = !mInvert ? fillAmount * 2f - side : mFillAmount * 2f - (1 - side);

                    if (RadialCut(mTempPos, mTempUVs, Mathf.Clamp01(val), !mInvert, NGUIMath.RepeatIndex(side + 3, 4)))
                    {
                        for (int i = 0; i < 4; ++i)
                        {
                            verts.Add(mTempPos[i]);
                            uvs.Add(mTempUVs[i]);
                            cols.Add(color);
                        }
                    }
                }
                return;
            }

            if (mFillDirection == FillDirection.Radial360)
            {
                for (int corner = 0; corner < 4; ++corner)
                {
                    float fx0, fx1, fy0, fy1;

                    if (corner < 2) { fx0 = 0f; fx1 = 0.5f; }
                    else { fx0 = 0.5f; fx1 = 1f; }

                    if (corner == 0 || corner == 3) { fy0 = 0f; fy1 = 0.5f; }
                    else { fy0 = 0.5f; fy1 = 1f; }

                    mTempPos[0].x = Mathf.Lerp(v.x, v.z, fx0);
                    mTempPos[1].x = mTempPos[0].x;
                    mTempPos[2].x = Mathf.Lerp(v.x, v.z, fx1);
                    mTempPos[3].x = mTempPos[2].x;

                    mTempPos[0].y = Mathf.Lerp(v.y, v.w, fy0);
                    mTempPos[1].y = Mathf.Lerp(v.y, v.w, fy1);
                    mTempPos[2].y = mTempPos[1].y;
                    mTempPos[3].y = mTempPos[0].y;

                    mTempUVs[0].x = Mathf.Lerp(tx0, tx1, fx0);
                    mTempUVs[1].x = mTempUVs[0].x;
                    mTempUVs[2].x = Mathf.Lerp(tx0, tx1, fx1);
                    mTempUVs[3].x = mTempUVs[2].x;

                    mTempUVs[0].y = Mathf.Lerp(ty0, ty1, fy0);
                    mTempUVs[1].y = Mathf.Lerp(ty0, ty1, fy1);
                    mTempUVs[2].y = mTempUVs[1].y;
                    mTempUVs[3].y = mTempUVs[0].y;

                    float val = mInvert ?
                        mFillAmount * 4f - NGUIMath.RepeatIndex(corner + 2, 4) :
                        mFillAmount * 4f - (3 - NGUIMath.RepeatIndex(corner + 2, 4));

                    if (RadialCut(mTempPos, mTempUVs, Mathf.Clamp01(val), mInvert, NGUIMath.RepeatIndex(corner + 2, 4)))
                    {
                        for (int i = 0; i < 4; ++i)
                        {
                            verts.Add(mTempPos[i]);
                            uvs.Add(mTempUVs[i]);
                            cols.Add(color);
                        }
                    }
                }
                return;
            }
        }

        // Fill the buffer with the quad for the sprite
        for (int i = 0; i < 4; ++i)
        {
            verts.Add(mTempPos[i]);
            uvs.Add(mTempUVs[i]);
            cols.Add(color);
        }
	}

    /// <summary>
    /// Adjust the specified quad, making it be radially filled instead.
    /// </summary>

    static bool RadialCut(Vector2[] xy, Vector2[] uv, float fill, bool invert, int corner)
    {
        // Nothing to fill
        if (fill < 0.001f) return false;

        // Even corners invert the fill direction
        if ((corner & 1) == 1) invert = !invert;

        // Nothing to adjust
        if (!invert && fill > 0.999f) return true;

        // Convert 0-1 value into 0 to 90 degrees angle in radians
        float angle = Mathf.Clamp01(fill);
        if (invert) angle = 1f - angle;
        angle *= 90f * Mathf.Deg2Rad;

        // Calculate the effective X and Y factors
        float cos = Mathf.Cos(angle);
        float sin = Mathf.Sin(angle);

        RadialCut(xy, cos, sin, invert, corner);
        RadialCut(uv, cos, sin, invert, corner);
        return true;
    }

    /// <summary>
    /// Adjust the specified quad, making it be radially filled instead.
    /// </summary>

    static void RadialCut(Vector2[] xy, float cos, float sin, bool invert, int corner)
    {
        int i0 = corner;
        int i1 = NGUIMath.RepeatIndex(corner + 1, 4);
        int i2 = NGUIMath.RepeatIndex(corner + 2, 4);
        int i3 = NGUIMath.RepeatIndex(corner + 3, 4);

        if ((corner & 1) == 1)
        {
            if (sin > cos)
            {
                cos /= sin;
                sin = 1f;

                if (invert)
                {
                    xy[i1].x = Mathf.Lerp(xy[i0].x, xy[i2].x, cos);
                    xy[i2].x = xy[i1].x;
                }
            }
            else if (cos > sin)
            {
                sin /= cos;
                cos = 1f;

                if (!invert)
                {
                    xy[i2].y = Mathf.Lerp(xy[i0].y, xy[i2].y, sin);
                    xy[i3].y = xy[i2].y;
                }
            }
            else
            {
                cos = 1f;
                sin = 1f;
            }

            if (!invert) xy[i3].x = Mathf.Lerp(xy[i0].x, xy[i2].x, cos);
            else xy[i1].y = Mathf.Lerp(xy[i0].y, xy[i2].y, sin);
        }
        else
        {
            if (cos > sin)
            {
                sin /= cos;
                cos = 1f;

                if (!invert)
                {
                    xy[i1].y = Mathf.Lerp(xy[i0].y, xy[i2].y, sin);
                    xy[i2].y = xy[i1].y;
                }
            }
            else if (sin > cos)
            {
                cos /= sin;
                sin = 1f;

                if (invert)
                {
                    xy[i2].x = Mathf.Lerp(xy[i0].x, xy[i2].x, cos);
                    xy[i3].x = xy[i2].x;
                }
            }
            else
            {
                cos = 1f;
                sin = 1f;
            }

            if (invert) xy[i3].y = Mathf.Lerp(xy[i0].y, xy[i2].y, sin);
            else xy[i1].x = Mathf.Lerp(xy[i0].x, xy[i2].x, cos);
        }
    }
}
