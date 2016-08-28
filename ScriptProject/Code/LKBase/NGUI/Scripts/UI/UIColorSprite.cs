using UnityEngine;
using System.Collections.Generic;

[ExecuteInEditMode]
[AddComponentMenu("NGUI/UI/Sprite (Color)")]
public class UIColorSprite : UISprite
{

	[SerializeField]
	Color mTopColor = Color.white;
	[SerializeField]
	Color mMiddleColor = Color.white;
	[SerializeField]
	Color mBottomColor = Color.white;

	public Color TopColor
	{
		get
		{
			return mTopColor;
		}
		set
		{
			mTopColor = value;
			mChanged = true;
		}
	}

	public Color MiddleColor
	{
		get
		{
			return mMiddleColor;
		}
		set
		{
			mMiddleColor = value;
			mChanged = true;
		}
	}

	public Color BottomColor
	{
		get
		{
			return mBottomColor;
		}
		set
		{
			mBottomColor = value;
			mChanged = true;
		}
	}

	override public void OnFill(BetterList<Vector3> verts, BetterList<Vector2> uvs, BetterList<Color> cols)
	{
        Vector2 uv0 = new Vector2(mOuterUV.xMin, mOuterUV.yMin);
        Vector2 uv1 = new Vector2(mOuterUV.xMax, mOuterUV.yMax);

		Color c0 = color * mTopColor;
		Color c2 = color * mBottomColor;
		Color c1 = color * mMiddleColor;

        Vector4 v = drawingDimensions;

        float mid = v.y + (v.w - v.y) / 2f;
        verts.Add(new Vector3(v.x, v.y));
        verts.Add(new Vector3(v.x, mid));
        verts.Add(new Vector3(v.z, mid));
        verts.Add(new Vector3(v.z, v.y));

        verts.Add(new Vector3(v.x, mid));
        verts.Add(new Vector3(v.x, v.w));
        verts.Add(new Vector3(v.z, v.w));
        verts.Add(new Vector3(v.z, mid));

        Vector2 t = uv1;

        uv1.y = uv1.y + (uv0.y - t.y) / 2f;

        uvs.Add(uv0);
        uvs.Add(new Vector2(uv0.x, uv1.y));
        uvs.Add(uv1);
        uvs.Add(new Vector2(uv1.x, uv0.y));

        uv1 = t;
        uv0.y = uv1.y + (uv0.y - t.y) / 2f;
        uvs.Add(uv0);
        uvs.Add(new Vector2(uv0.x, uv1.y));
        uvs.Add(uv1);
        uvs.Add(new Vector2(uv1.x, uv0.y));

        cols.Add(c2);
        cols.Add(c1);
        cols.Add(c1);
        cols.Add(c2);

        cols.Add(c1);
        cols.Add(c0);
        cols.Add(c0);
        cols.Add(c1);

	}
}
