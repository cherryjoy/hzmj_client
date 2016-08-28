using UnityEngine;
using System.Collections;

[ExecuteInEditMode]
[AddComponentMenu("NGUI/UI/Label (Color)")]
public class UIColorLabel : UILabel
{
    [SerializeField]
    Color mMiddleColor = Color.white;

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

    public override void OnFill(BetterList<Vector3> verts, BetterList<Vector2> uvs, BetterList<Color> cols)
    {
        if (font == null) return;
        //MakePositionPerfect();
        Pivot p = pivot;
        int offset = verts.size;

        if (p == Pivot.Left || p == Pivot.TopLeft || p == Pivot.BottomLeft)
        {
            font.Print(processedText, color, mTopColor, mMiddleColor, mBottomColor, verts, uvs, cols, mEncoding, UIFont.Alignment.Left, 0, mSpaceingY, FontSize, mFontStyle, FontSpacingX, TrueTypeFont, width, height);
        }
        else if (p == Pivot.Right || p == Pivot.TopRight || p == Pivot.BottomRight)
        {
            font.Print(processedText, color, mTopColor, mMiddleColor, mBottomColor, verts, uvs, cols, mEncoding, UIFont.Alignment.Right,
                Mathf.RoundToInt(relativeSize.x * FontSize), mSpaceingY, FontSize, mFontStyle, FontSpacingX, TrueTypeFont, width, height);
        }
        else
        {
            font.Print(processedText, color, mTopColor, mMiddleColor, mBottomColor, verts, uvs, cols, mEncoding, UIFont.Alignment.Center,
                Mathf.RoundToInt(relativeSize.x * FontSize), mSpaceingY, FontSize, mFontStyle, FontSpacingX, TrueTypeFont, width, height);
        }

        //apply pivot offset to verts
        Vector3 pOffset = new Vector3(pivotOffset.x * (width - 1) * relativeSize.x, pivotOffset.y * (height - 1) * relativeSize.y, 0);
        for (int i = 0; i < verts.size; i++)
        {
            verts.buffer[i] += pOffset;
        }
        
        // Apply an effect if one was requested
        if (effectStyle != Effect.None)
        {
            Vector3 scale = cachedTransform.localScale;
            if (scale.x == 0f || scale.y == 0f) return;

            int end = verts.size;
            float pixel = 1f / FontSize;

            ApplyShadow(verts, uvs, cols, offset, end, pixel, -pixel);

            if (effectStyle == Effect.Outline)
            {
                offset = end;
                end = verts.size;

                ApplyShadow(verts, uvs, cols, offset, end, -pixel, pixel);

                offset = end;
                end = verts.size;

                ApplyShadow(verts, uvs, cols, offset, end, pixel, pixel);

                offset = end;
                end = verts.size;

                ApplyShadow(verts, uvs, cols, offset, end, -pixel, -pixel);
            }
        }
    }

}
