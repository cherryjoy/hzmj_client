using UnityEditor;
using UnityEngine;

public class UIEqualRatioScale : MonoBehaviour
{
    public Vector4 offset = Vector4.zero;
    public UIEqualRatioScale.Direction direction = Direction.Height;
    public bool isResetInScreen = true;

    public enum Direction
    {
        Auto = 0,
        Width = 1,
        Height = 2,
    }

    public enum RaioScaleMode
    {
        Start = 0,
        LateUpdate = 1,
    }

    private Camera uiCamera = null;
    private Transform parent;

    public RaioScaleMode scaleMode = RaioScaleMode.LateUpdate;
    void Start()
    {
        uiCamera = NGUITools.FindCameraForLayer(gameObject.layer);
        parent = transform.parent;
        if(scaleMode == RaioScaleMode.Start)
            LateUpdate();
    }

    public void RatioScaleByHand() {
        LateUpdate();
    }

    void LateUpdate()
    {
#if !UNITY_EDITOR
        RatioScale();
        Destroy(this);
#else

        RatioScale();
        this.enabled = false;
#endif
    }

    public void RatioScale()
    {
        if (uiCamera == null || parent == null)
        {
#if !UNITY_EDITOR
            Destroy(this);
#endif
            return;
        }

        Bounds selfBounds = NGUIMath.CalculateRelativeWidgetBounds(parent, transform);
        if (selfBounds.size.x == 0 || selfBounds.size.y == 0)
            return;

        Vector3 screenLeftBottom = new Vector3(offset.x, offset.y, 0.0f);
        Vector3 screenTopRight = new Vector3(Screen.width - offset.z, Screen.height - offset.w);
        Vector3 screenLeftBottomInGlobal = uiCamera.ScreenToWorldPoint(screenLeftBottom);
        Vector3 screenTopRightInGlobal = uiCamera.ScreenToWorldPoint(screenTopRight);

        float screenWidthInGlobal = Mathf.Abs(screenTopRightInGlobal.x - screenLeftBottomInGlobal.x);
        float screenHeightInGlobal = Mathf.Abs(screenTopRightInGlobal.y - screenLeftBottomInGlobal.y);
        
        Vector3 globalLeftBottom = parent.TransformPoint(new Vector3(selfBounds.center.x - selfBounds.extents.x, selfBounds.center.y - selfBounds.extents.y));
        Vector3 globalTopRight = parent.TransformPoint(new Vector3(selfBounds.center.x + selfBounds.extents.x, selfBounds.center.y + selfBounds.extents.y));

        float widthInGlobal = Mathf.Abs(globalTopRight.x - globalLeftBottom.x);
        float heightInGlobal = Mathf.Abs(globalTopRight.y - globalLeftBottom.y);

        float scaleOffset = 1;
        Direction posDir = direction;
        if (direction == Direction.Height)
        {
            scaleOffset = screenHeightInGlobal / Mathf.Abs(heightInGlobal);
        }
        else if(direction == Direction.Width)
        {
            scaleOffset = screenWidthInGlobal / Mathf.Abs(widthInGlobal);
        }
        else
        {
            float scaleH = screenHeightInGlobal / Mathf.Abs(heightInGlobal);
            float scaleW = screenWidthInGlobal / Mathf.Abs(widthInGlobal);

            if(scaleH > scaleW)
            {
                scaleOffset = scaleH;
                posDir = Direction.Height;
            }else
            {
                scaleOffset = scaleW;
                posDir = Direction.Width;
            }
        }
        transform.localScale *= scaleOffset;
        selfBounds.size *= scaleOffset;

        if(isResetInScreen)
        {
            globalLeftBottom = parent.TransformPoint(new Vector3(selfBounds.center.x - selfBounds.extents.x, selfBounds.center.y - selfBounds.extents.y));
            globalTopRight = parent.TransformPoint(new Vector3(selfBounds.center.x + selfBounds.extents.x, selfBounds.center.y + selfBounds.extents.y));

            Vector3 pos = transform.position;
            if (posDir == Direction.Height)
            {
                pos.y += screenLeftBottomInGlobal.y - globalLeftBottom.y;
            }
            else
            {
                pos.x += screenLeftBottomInGlobal.x - globalLeftBottom.x;
            }

            transform.position = pos;
        }
        
    }

}

