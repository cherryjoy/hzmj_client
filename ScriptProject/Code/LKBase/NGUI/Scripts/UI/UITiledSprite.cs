//----------------------------------------------
//            NGUI: Next-Gen UI kit
// Copyright © 2011-2012 Tasharen Entertainment
//----------------------------------------------

using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Widget that tiles the sprite repeatedly, fully filling the area.
/// Used best with repeating tileable backgrounds.
/// </summary>

[ExecuteInEditMode]
[AddComponentMenu("NGUI/UI/Sprite (Tiled)")]
public class UITiledSprite : UISlicedSprite
{
	/// <summary>
	/// Tiled sprite shouldn't inherit the sprite's changes to this function.
	/// </summary>

	override public void MakePixelPerfect ()
	{
        //Vector3 pos = cachedTransform.localPosition;
        //pos.x = Mathf.RoundToInt(pos.x);
        //pos.y = Mathf.RoundToInt(pos.y);
        //pos.z = Mathf.RoundToInt(pos.z);
        //cachedTransform.localPosition = pos;

        //Vector3 scale = cachedTransform.localScale;
        //scale.x = Mathf.RoundToInt(scale.x);
        //scale.y = Mathf.RoundToInt(scale.y);
        //scale.z = 1f;
        //cachedTransform.localScale = scale;
        base.MakePixelPerfect();
	}


  

	/// <summary>
	/// Fill the draw buffers.
	/// </summary>

	public override void OnFill (BetterList<Vector3> verts, BetterList<Vector2> uvs, BetterList<Color> cols)
	{
        Texture tex = material.mainTexture;
        if (tex == null) return;

        Vector4 dr = drawingDimensions;
        if (dr.x == dr.z || dr.y == dr.w )
            return;

        Vector2 size = new Vector2(mInnerUV.width * tex.width, mInnerUV.height * tex.height);
        size *= atlas.pixelSize;
        if (size.x < 10 || size.y < 10)
            return;

        float x0 = dr.x;
        float y0 = dr.y;

        float u0 = mInnerUV.xMin;
        float v0 = mInnerUV.yMin;

        while (y0 < dr.w)
        {
            x0 = dr.x;
            float y1 = y0 + size.y;
            float v1 = mInnerUV.yMax;

            if (y1 > dr.w)
            {
                v1 = Mathf.Lerp(mInnerUV.yMin, mInnerUV.yMax, (dr.w - y0) / size.y);
                y1 = dr.w;
            }

            while (x0 < dr.z)
            {
                float x1 = x0 + size.x;
                float u1 = mInnerUV.xMax;

                if (x1 > dr.z)
                {
                    u1 = Mathf.Lerp(mInnerUV.xMin, mInnerUV.xMax, (dr.z - x0) / size.x);
                    x1 = dr.z;
                }

                verts.Add(new Vector3(x0, y0));
                verts.Add(new Vector3(x0, y1));
                verts.Add(new Vector3(x1, y1));
                verts.Add(new Vector3(x1, y0));

                uvs.Add(new Vector2(u0, v0));
                uvs.Add(new Vector2(u0, v1));
                uvs.Add(new Vector2(u1, v1));
                uvs.Add(new Vector2(u1, v0));

                cols.Add(color);
                cols.Add(color);
                cols.Add(color);
                cols.Add(color);

                x0 += size.x;
            }
            y0 += size.y;
        }
	}
}