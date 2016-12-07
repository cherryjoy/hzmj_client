using UnityEngine;
using System.Collections;

[AddComponentMenu("NGUI/UI/AutoScreenScale")]
public class AutoScreenScale : MonoBehaviour
{
    public enum Direction
    {
        Vertical,
        Horizontal,
        Both,
    }
    public enum ScaleThingsType {
        UIWidget,
        Collider,
        UIPanel,
    }
    public UIRoot uiRoot = null;
    public Camera uiCamera = null;
    public Direction direcion = Direction.Both;
    public Vector2 scaleRate = new Vector2(1f,1f);
    public Vector2 offset = new Vector2(0f, 0f);
    //private Vector2 cacheOffset = new Vector2(0f, 0f);
    public ScaleThingsType WhatToScale = ScaleThingsType.UIWidget;
    public bool keepRate = false;
    private int updateCount = 1;
    private readonly Vector3 STANDARDSCREENSIZE = new Vector3(960.0f, 640.0f, 0.0f);

    void Awake()
    {
        if (uiCamera == null) uiCamera = NGUITools.FindCameraForLayer(gameObject.layer);
        if (uiRoot == null) uiRoot = FindUIRootForLayer(gameObject.layer);
        Reset();
    }

    void Reset()
	{
        Vector3 vSize = new Vector3(960.0f, 640.0f, 0.0f);

        vSize.x *= uiRoot.transform.localScale.x;
        vSize.y *= uiRoot.transform.localScale.y;
        vSize = uiCamera.WorldToScreenPoint(vSize);

        if (WhatToScale == ScaleThingsType.UIWidget)
        {
            UIWidget widget = transform.GetComponent<UIWidget>();
            if (widget == null)
                return;
            vSize = ScaleByType(widget.Dimensions, vSize,direcion);

            widget.Dimensions = new Vector2(vSize.x, vSize.y);
            BoxCollider boxCollider = transform.GetComponent<BoxCollider>();
            if (boxCollider != null)
            {
                boxCollider.size = new Vector3(vSize.x, vSize.y, boxCollider.size.z);
            }
        }
        else if(WhatToScale == ScaleThingsType.Collider)
        {
            BoxCollider boxCollider = transform.GetComponent<BoxCollider>();
            if (boxCollider == null)
                return;
            vSize = ScaleByType(new Vector2(boxCollider.size.x, boxCollider.size.y),vSize, direcion);

            boxCollider.size = new Vector3(vSize.x, vSize.y, boxCollider.size.z);
        }
        else
        {
            UIPanel panel = transform.GetComponent<UIPanel>();
            if (panel == null)
                return;
            Vector2 oriPanelSize = new Vector2(panel.clipRange.z,panel.clipRange.w);
            vSize = ScaleByType(oriPanelSize,vSize, direcion);
            panel.clipRange = new Vector4(panel.clipRange.x, panel.clipRange.y, vSize.x, vSize.y);
        }
	}

    Vector3 ScaleByType(Vector2 oriSize,Vector3 screenSize, Direction dir) {
        Vector3 scaleTo = screenSize;
        Vector2 curScreenSize = new Vector2(Screen.width, Screen.height);
        //Debug.Log(Screen.width + " " +Screen.height);
        float oriRate = oriSize.x / oriSize.y;
        if (dir == Direction.Horizontal)
        {
            scaleTo.x = STANDARDSCREENSIZE.x * curScreenSize.x / (scaleTo.x - curScreenSize.x * 0.5f);
            scaleTo.x *= scaleRate.x;
            scaleTo.x += offset.x;
            scaleTo.y = oriSize.y;
            if (keepRate)
            {
                scaleTo.y = scaleTo.x / oriRate;
            }

        }
        if (dir == Direction.Vertical)
        {
            scaleTo.y = STANDARDSCREENSIZE.y * curScreenSize.y / (scaleTo.y - curScreenSize.y * 0.5f);
            scaleTo.y *= scaleRate.y;
            scaleTo.y += offset.y;
            scaleTo.x = oriSize.x;
            if (keepRate)
            {
                scaleTo.x = scaleTo.y * oriRate;
            }
        }
        if (dir == Direction.Both)
        {
            scaleTo.x = STANDARDSCREENSIZE.x * curScreenSize.x / (scaleTo.x - curScreenSize.x * 0.5f);
            scaleTo.y = STANDARDSCREENSIZE.y * curScreenSize.y / (scaleTo.y - curScreenSize.y * 0.5f);
            scaleTo.x *= scaleRate.x;
            scaleTo.y *= scaleRate.y;
            scaleTo.x += offset.x;
            scaleTo.y += offset.y;
        }
        return scaleTo;
    }

    void LateUpdate()
    {
        if (updateCount > 0)
        {
            Reset();
            updateCount--;
        }
    }

    UIRoot FindUIRootForLayer(int layer, bool ignoreLayer = true)
    {
        if (ignoreLayer)
        {
            if (UIRoot.mSelf != null)
            {
                return UIRoot.mSelf;
            }
        }
        UIRoot[] roots = GameObject.FindObjectsOfType(typeof(UIRoot)) as UIRoot[];
        for (int i = 0; i < roots.Length; i++)
        {
            if (roots[i].gameObject.layer == layer)
            {
                return roots[i];
            }
        }
        return null;
    }
}
