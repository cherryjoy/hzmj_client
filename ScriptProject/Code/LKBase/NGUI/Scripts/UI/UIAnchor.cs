//----------------------------------------------
//            NGUI: Next-Gen UI kit
// Copyright © 2011-2012 Tasharen Entertainment
//----------------------------------------------

using UnityEngine;

/// <summary>
/// This script can be used to anchor an object to the side of the screen,
/// or scale an object to always match the dimensions of the screen.
/// </summary>

[ExecuteInEditMode]
[AddComponentMenu("NGUI/UI/Anchor")]
public class UIAnchor : MonoBehaviour
{
	public enum Side
	{
		BottomLeft,
		Left,
		TopLeft,
		Top,
		TopRight,
		Right,
		BottomRight,
		Bottom,
		Center,
	}

    public enum AnchorDirection
    {
        X,
        Y,
        Both,
    }

	public Camera uiCamera = null;
	public Side side = Side.Center;
	public bool halfPixelOffset = true;
	public bool stretchToFill = false;
	public float depthOffset = 0f;
	public Vector3 ExtraOffset = Vector3.zero;
    public Vector3 ViewportOffset = Vector3.zero;

	public bool updateAllTheTime = false;

	Transform mTrans;
	bool mIsWindows = false;
	int updateCount = 3;
    Vector3 mZeroPointOfViewport = Vector3.zero;

    public AnchorDirection direction = AnchorDirection.Both;

	/// <summary>
	/// Change the associated widget to be top-left aligned.
	/// </summary>

	void ChangeWidgetPivot ()
	{
		UIWidget widget = GetComponent<UIWidget>();
		if (widget != null) widget.pivot = UIWidget.Pivot.TopLeft;
	}

	/// <summary>
	/// Automatically make the widget top-left aligned if we're stretching to fill.
	/// </summary>

	void Start () { 
		if (stretchToFill) ChangeWidgetPivot();  
	
		///
		///此脚本在编辑状态执行，由于编辑器屏幕的不规则会导致UI计算出现不差（see 170line），所以必须在游戏运行时（屏幕正确时）重新调整
		///
		LanGang_AdjustUIPosition();
	}

	/// <summary>
	/// Automatically find the camera responsible for drawing the widgets under this object.
	/// </summary>

	void OnEnable ()
	{
		mTrans = transform;

		mIsWindows = (Application.platform == RuntimePlatform.WindowsPlayer ||
			Application.platform == RuntimePlatform.WindowsWebPlayer ||
			Application.platform == RuntimePlatform.WindowsEditor);

		if (uiCamera == null) uiCamera = NGUITools.FindCameraForLayer(gameObject.layer);
        mZeroPointOfViewport = uiCamera.ViewportToWorldPoint(Vector3.zero);
    }

	/// <summary>
	/// Anchor the object to the appropriate point.
	/// </summary>

	public void Update ()
	{
		//langang 2012-7-30

		if (updateAllTheTime == false)
		{
			updateCount--;
			if (updateCount > 0)
			{
				LanGang_AdjustUIPosition();
			}
		}
		else
		{
			LanGang_AdjustUIPosition();
		}
		
		/*
		if (uiCamera != null)
		{
			if (stretchToFill)
			{
				side = Side.TopLeft;
				if (!Application.isPlaying) ChangeWidgetPivot();
			}
			
			Vector3 v = new Vector3(Screen.width, Screen.height, 0f);

			if (side == Side.Center)
			{
				v.x *= uiCamera.rect.width * 0.5f;
				v.y *= uiCamera.rect.height * 0.5f;
			}
			else
			{
				if (side == Side.Right || side == Side.TopRight || side == Side.BottomRight)
				{
					v.x *= uiCamera.rect.xMax;
				}
				else if (side == Side.Top || side == Side.Center || side == Side.Bottom)
				{
					v.x *= (uiCamera.rect.xMax - uiCamera.rect.xMin) * 0.5f;
				}
				else
				{
					v.x *= uiCamera.rect.xMin;
				}

				if (side == Side.Top || side == Side.TopRight || side == Side.TopLeft)
				{
					v.y *= uiCamera.rect.yMax;
				}
				else if (side == Side.Left || side == Side.Center || side == Side.Right)
				{
					v.y *= (uiCamera.rect.yMax - uiCamera.rect.yMin) * 0.5f;
				}
				else
				{
					v.y *= uiCamera.rect.yMin;
				}
			}

			//v.z = (mTrans.TransformPoint(Vector3.forward * depthOffset) -
			//	mTrans.TransformPoint(Vector3.zero)).magnitude * Mathf.Sign(depthOffset);

			if (uiCamera.orthographic)
			{
				//v.z += (uiCamera.nearClipPlane + uiCamera.farClipPlane) * 0.5f;

				if (halfPixelOffset && mIsWindows)
				{
					v.x -= 0.5f;
					v.y += 0.5f;
				}
			}

			Vector3 newPos = uiCamera.ScreenToWorldPoint(v);
			Vector3 currPos = mTrans.position;

			// Wrapped in an 'if' so the scene doesn't get marked as 'edited' every frame
			newPos.z = currPos.z;
			if (newPos != currPos) mTrans.position = newPos;

			if (stretchToFill)
			{
				Vector3 localPos = mTrans.localPosition;
				Vector3 localScale = new Vector3(Mathf.Abs(localPos.x) * 2f, Mathf.Abs(localPos.y) * 2f, 1f);
				if (mTrans.localScale != localScale) mTrans.localScale = localScale;
			}
		}
		*/
	}
	
	//langang 2012-7-30
	public void LanGang_AdjustUIPosition(){
		if (uiCamera != null)
		{
			if (stretchToFill)
			{
				side = Side.TopLeft;
				if (!Application.isPlaying) ChangeWidgetPivot();
			}
			
			Vector3 v = new Vector3(Screen.width, Screen.height, 0f);

			if (side == Side.Center)
			{
				v.x *= uiCamera.rect.width * 0.5f;
				v.y *= uiCamera.rect.height * 0.5f;
			}
			else
			{
				if (side == Side.Right || side == Side.TopRight || side == Side.BottomRight)
				{
					v.x *= uiCamera.rect.xMax;
				}
				else if (side == Side.Top || side == Side.Center || side == Side.Bottom)
				{
					v.x *= (uiCamera.rect.xMax - uiCamera.rect.xMin) * 0.5f;
				}
				else
				{
					v.x *= uiCamera.rect.xMin;
				}

				if (side == Side.Top || side == Side.TopRight || side == Side.TopLeft)
				{
					v.y *= uiCamera.rect.yMax;
				}
				else if (side == Side.Left || side == Side.Center || side == Side.Right)
				{
					v.y *= (uiCamera.rect.yMax - uiCamera.rect.yMin) * 0.5f;
				}
				else
				{
					v.y *= uiCamera.rect.yMin;
				}
			}

			//v.z = (mTrans.TransformPoint(Vector3.forward * depthOffset) -
			//	mTrans.TransformPoint(Vector3.zero)).magnitude * Mathf.Sign(depthOffset);

			if (uiCamera.orthographic)
			{
				//v.z += (uiCamera.nearClipPlane + uiCamera.farClipPlane) * 0.5f;

				if (halfPixelOffset && mIsWindows)
				{
					v.x -= 0.5f;
					v.y += 0.5f;
				}
			}

			Vector3 newPos = uiCamera.ScreenToWorldPoint(v);
			Vector3 currPos = mTrans.position;

			// Wrapped in an 'if' so the scene doesn't get marked as 'edited' every frame
			newPos.z = currPos.z;
            if (direction == AnchorDirection.X)
            {
                newPos.y = currPos.y;
            }
            else if (direction == AnchorDirection.Y)
            {
                newPos.x = currPos.x;
            }
            if (newPos != currPos)
            {
                mTrans.position = newPos;
            }
            if (stretchToFill)
            {
                Vector3 localPos = mTrans.localPosition;
                Vector3 localScale = new Vector3(Mathf.Abs(localPos.x) * 2f, Mathf.Abs(localPos.y) * 2f, 1f);
                if (mTrans.localScale != localScale) mTrans.localScale = localScale;
            }
            if ( ViewportOffset != Vector3.zero)
            {
                Vector3 worldOffset = uiCamera.ViewportToWorldPoint(ViewportOffset);
                Vector3 worldOriPos = mTrans.position;
                mTrans.position = worldOriPos + worldOffset - mZeroPointOfViewport;
            }
            Vector3 tmpPos = mTrans.localPosition;
            tmpPos += ExtraOffset;
            mTrans.localPosition = tmpPos;
        }
	}

    public static Vector3 GetAnchorScreenPosition(Side side)
    {
        float standardWidth = 960.0f;
        float standardHeight = 640.0f;

        float standardAspect = 960.0f / 640.0f;
        float aspect = (float)Screen.width / Screen.height;

        Vector3 screenPosition = Vector3.zero;

        if (aspect < standardAspect)
        {
            float size = standardWidth * 0.5f;

            switch (side)
            {
                case Side.BottomLeft:
                    screenPosition = new Vector3(-size, -size / aspect, 0.0f);
                    break;
                case Side.Left:
                    screenPosition = new Vector3(-size, 0.0f, 0.0f);
                    break;
                case Side.TopLeft:
                    screenPosition = new Vector3(-size, size / aspect, 0.0f);
                    break;
                case Side.Top:
                    screenPosition = new Vector3(0.0f, size / aspect, 0.0f);
                    break;
                case Side.TopRight:
                    screenPosition = new Vector3(size, size / aspect, 0.0f);
                    break;
                case Side.Right:
                    screenPosition = new Vector3(size, 0.0f, 0.0f);
                    break;
                case Side.BottomRight:
                    screenPosition = new Vector3(size, -size / aspect, 0.0f);
                    break;
                case Side.Bottom:
                    screenPosition = new Vector3(0.0f, -size / aspect, 0.0f);
                    break;
                case Side.Center:
                    screenPosition = new Vector3(0.0f, 0.0f, 0.0f);
                    break;
            }
        }
        else
        {
            float size = standardHeight * 0.5f;
            switch (side)
            {
                case Side.BottomLeft:
                    screenPosition = new Vector3(-size * aspect, -size, 0.0f);
                    break;
                case Side.Left:
                    screenPosition = new Vector3(-size * aspect, 0.0f, 0.0f);
                    break;
                case Side.TopLeft:
                    screenPosition = new Vector3(-size * aspect, size, 0.0f);
                    break;
                case Side.Top:
                    screenPosition = new Vector3(0.0f, size, 0.0f);
                    break;
                case Side.TopRight:
                    screenPosition = new Vector3(size * aspect, size, 0.0f);
                    break;
                case Side.Right:
                    screenPosition = new Vector3(size * aspect, 0.0f, 0.0f);
                    break;
                case Side.BottomRight:
                    screenPosition = new Vector3(size * aspect, -size, 0.0f);
                    break;
                case Side.Bottom:
                    screenPosition = new Vector3(0.0f, -size, 0.0f);
                    break;
                case Side.Center:
                    screenPosition = new Vector3(0.0f, 0.0f, 0.0f);
                    break;
            }
        }

        return screenPosition;
    }
}
