using UnityEngine;
public class FrameLayoutHelper : MonoBehaviour
{
    public UISlicedSprite bgSprite;
    public UISprite bgEffectSprite;
    public UISlicedSprite bgEffectSprite2;
    public UISprite lineLeftSprite;
    public UISprite lineRightSprite;
    public UISprite lineEffectLeftSprite;
    public UISprite lineEffectRightSprite;
    public UISlicedSprite otherSprite;

#if UNITY_EDITOR 

    public Vector2 FrameSize
    {
        get
        {
            return this.bgSprite.Dimensions;
        }
    }

    public void PivotFrame(UIWidget.Pivot piovt, Vector2 fSize)
    {
        Vector2 offset = Vector2.one;
        if (piovt == UIWidget.Pivot.TopLeft || piovt == UIWidget.Pivot.Left || piovt == UIWidget.Pivot.BottomLeft)
        {
            offset.x = 0;
        }
        else if (piovt == UIWidget.Pivot.Center || piovt == UIWidget.Pivot.Top || piovt == UIWidget.Pivot.Bottom)
        {
            offset.x = -0.5f;
        }
        else
        {
            offset.x = -1;
        }

        if (piovt == UIWidget.Pivot.Top || piovt == UIWidget.Pivot.TopLeft || piovt == UIWidget.Pivot.TopRight)
        {
            offset.y = 0;
        }
        else if (piovt == UIWidget.Pivot.Left || piovt == UIWidget.Pivot.Center || piovt == UIWidget.Pivot.Right)
        {
            offset.y = 0.5f;
        }
        else
        {
            offset.y = 1.0f;
        }

        transform.localPosition += new Vector3(fSize.x * offset.x, fSize.y * offset.y, 0);
    }

    public void LayoutFrame(Vector2 fSize)
    {
        if (this.bgSprite != null)
        {
            this.bgSprite.Dimensions = fSize;
        }
        if (this.bgEffectSprite != null)
        {
            this.bgEffectSprite.Dimensions = fSize;
        }
        if(this.bgEffectSprite2 !=null)
        {
            Vector3 lp = bgSprite.transform.localPosition;
            lp += new Vector3(2, -2, 0);
            bgEffectSprite2.transform.localPosition = lp;
            this.bgEffectSprite2.Dimensions = new Vector2(fSize.x - 4, fSize.y - 4);
        }
        Vector3 pos;
        if (this.lineEffectLeftSprite != null)
        {
            pos = this.lineEffectLeftSprite.transform.localPosition;
            this.lineEffectLeftSprite.transform.localPosition = new Vector3(fSize.x * 0.5f, pos.y, pos.z);
            this.lineEffectLeftSprite.Dimensions = new Vector2(fSize.x * 0.5f, this.lineEffectLeftSprite.Dimensions.y);
        }
        if (this.lineEffectRightSprite != null)
        {
            pos = this.lineEffectRightSprite.transform.localPosition;
            this.lineEffectRightSprite.transform.localPosition = new Vector3(fSize.x * 0.5f, pos.y, pos.z);
            this.lineEffectRightSprite.Dimensions = new Vector2(fSize.x * 0.5f, this.lineEffectRightSprite.Dimensions.y);
        }
        if (this.lineLeftSprite != null)
        {
            pos = this.lineLeftSprite.transform.localPosition;
            this.lineLeftSprite.transform.localPosition = new Vector3(fSize.x * 0.5f, pos.y, pos.z);
            this.lineLeftSprite.Dimensions = new Vector2(fSize.x * 0.5f, this.lineLeftSprite.Dimensions.y);
        }
        if (this.lineLeftSprite != null)
        {
            pos = this.lineRightSprite.transform.localPosition;
            this.lineRightSprite.transform.localPosition = new Vector3(fSize.x * 0.5f, pos.y, pos.z);
            this.lineRightSprite.Dimensions = new Vector2(fSize.x * 0.5f, this.lineRightSprite.Dimensions.y);
        }

        if(otherSprite!=null)
        {
            pos = this.otherSprite.transform.localPosition;
            this.otherSprite.transform.localPosition = new Vector3(5, pos.y, pos.z);
            this.otherSprite.Dimensions = new Vector2(fSize.x - 10, this.otherSprite.Dimensions.y);
        }
    }

#endif
}

