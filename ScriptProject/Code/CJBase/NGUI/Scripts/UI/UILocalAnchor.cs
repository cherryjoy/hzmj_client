using UnityEngine;
using System.Collections;

[ExecuteInEditMode]
public class UILocalAnchor : MonoBehaviour {
    public UIWidget Target;
    public UIWidget.Pivot RelativeWay = UIWidget.Pivot.Center;
    public Vector3 OffsetFromTargeCenter = Vector3.zero;
    public Vector2 RatioOffset = Vector2.zero;
    public bool RePositionNow = false;
	public bool RePositionAlways = false;
    public bool RePosOnStart = true;



    // Use this for initialization
	void Start () {
        if (RePosOnStart)
        {
            RePos();
        }
	}

#if UNITY_EDITOR
    // Update is called once per frame
	void Update () {
        if (RePositionNow || RePositionAlways)
        {
            RePos();
            RePositionNow = false;
        }  
	}
#endif
    void RePos()
    {
        if (Target != null)
        {
            Vector2 offset = Vector2.zero;

            if (Target.pivot == UIWidget.Pivot.Top || Target.pivot == UIWidget.Pivot.Center || Target.pivot == UIWidget.Pivot.Bottom) offset.x = 0;
            else if (Target.pivot == UIWidget.Pivot.TopRight || Target.pivot == UIWidget.Pivot.Right || Target.pivot == UIWidget.Pivot.BottomRight) offset.x = -0.5f;
            else if (Target.pivot == UIWidget.Pivot.TopLeft || Target.pivot == UIWidget.Pivot.Left || Target.pivot == UIWidget.Pivot.BottomLeft) offset.x = 0.5f;

            if (Target.pivot == UIWidget.Pivot.Left || Target.pivot == UIWidget.Pivot.Center || Target.pivot == UIWidget.Pivot.Right) offset.y = 0;
            else if (Target.pivot == UIWidget.Pivot.BottomLeft || Target.pivot == UIWidget.Pivot.Bottom || Target.pivot == UIWidget.Pivot.BottomRight) offset.y = 0.5f;
            else if (Target.pivot == UIWidget.Pivot.Top || Target.pivot == UIWidget.Pivot.TopLeft || Target.pivot == UIWidget.Pivot.TopRight) offset.y = -0.5f;

            if (RelativeWay == UIWidget.Pivot.Top || RelativeWay == UIWidget.Pivot.Center || RelativeWay == UIWidget.Pivot.Bottom) offset.x += 0;
            else if (RelativeWay == UIWidget.Pivot.TopRight || RelativeWay == UIWidget.Pivot.Right || RelativeWay == UIWidget.Pivot.BottomRight) offset.x += 0.5f;
            else if (RelativeWay == UIWidget.Pivot.TopLeft || RelativeWay == UIWidget.Pivot.Left || RelativeWay == UIWidget.Pivot.BottomLeft) offset.x += -0.5f;

            if (RelativeWay == UIWidget.Pivot.Left || RelativeWay == UIWidget.Pivot.Center || RelativeWay == UIWidget.Pivot.Right) offset.y += 0;
            else if (RelativeWay == UIWidget.Pivot.BottomLeft || RelativeWay == UIWidget.Pivot.Bottom || RelativeWay == UIWidget.Pivot.BottomRight) offset.y += -0.5f;
            else if (RelativeWay == UIWidget.Pivot.TopLeft || RelativeWay == UIWidget.Pivot.Top || RelativeWay == UIWidget.Pivot.TopRight) offset.y += 0.5f;

            Vector2 size = new Vector2(Target.transform.localScale.x * Target.Dimensions.x, Target.transform.localScale.y * Target.Dimensions.y);
            Vector3 relativePos = Vector3.zero;
            relativePos.x += offset.x * size.x;
            relativePos.y += offset.y * size.y;

            transform.localPosition = new Vector3(
                relativePos.x + OffsetFromTargeCenter.x + Target.transform.localPosition.x + RatioOffset.x * relativePos.x,
                relativePos.y + OffsetFromTargeCenter.y + Target.transform.localPosition.y + RatioOffset.y * relativePos.y,
                transform.localPosition.z
                );
        }
      
    }

}
