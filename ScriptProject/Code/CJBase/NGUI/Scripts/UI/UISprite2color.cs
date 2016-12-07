using UnityEngine;
using System.Collections;

[ExecuteInEditMode]
[AddComponentMenu("NGUI/UI/Sprite (2Color)")]
public class UISprite2color : UISprite {
    public Color UpColor = Color.white;
    public Color DownColor = Color.white;

    override public void OnFill(BetterList<Vector3> verts, BetterList<Vector2> uvs, BetterList<Color> cols)
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

        uvs.Add(uv1);
        uvs.Add(new Vector2(uv1.x, uv0.y));
        uvs.Add(uv0);
        uvs.Add(new Vector2(uv0.x, uv1.y));

        cols.Add(DownColor);
        cols.Add(UpColor);
        cols.Add(UpColor);
        cols.Add(DownColor);
    }
}
