using UnityEngine;
using System.Collections;


[AddComponentMenu("NGUI/Interaction/Drag RefreshObject")]
public class UIDragRefresh : MonoBehaviour
{
	public Transform target;

	public Vector3 scale = Vector3.one;

	public GameObject receiver;
	public string strPressDown = "OnPressDown";
	public string strPressUp = "OnPressUp";
	
	public float cellHeight;
	
	Plane mPlane;
	Vector3 mLastPos;
	UIPanel mPanel;
	bool mPressed = false;

	/// <summary>
	/// Find the panel responsible for this object.
	/// </summary>

	UIPanel GetPanel ()
	{
		if (mPanel == null)
			mPanel = (target != null) ? UIPanel.Find(target.transform, false) : null;
		return mPanel;
	}

	/// <summary>
	/// Create a plane on which we will be performing the dragging.
	/// </summary>

	void OnPress (bool pressed)
	{
		if (enabled && gameObject.activeSelf && target != null)
		{
			mPressed = pressed;

			if (pressed)
			{
				if(receiver != null)
					receiver.SendMessage(strPressDown, SendMessageOptions.DontRequireReceiver);
			}
			else 
			{
				if(receiver != null)
					receiver.SendMessage(strPressUp, SendMessageOptions.DontRequireReceiver);
				if(GetPanel().clipRange.w > target.childCount * cellHeight)
				{
					Vector3 offset = scale * (GetPanel().cachedTransform.position.y + GetPanel().clipRange.w * 0.5f -  target.localPosition.y);
					if(offset.magnitude > 0)
					{
						SpringPosition sp = SpringPosition.Begin(target.gameObject, target.localPosition + offset, 13f);
						sp.ignoreTimeScale = true;
						sp.worldSpace = false;
					}
				}
				else
				{
					Vector3 offset = Vector3.zero;
					
					float minRectY = GetPanel().cachedTransform.position.y - GetPanel().clipRange.w * 0.5f;
					float maxRectY = GetPanel().cachedTransform.position.y + GetPanel().clipRange.w * 0.5f;
					float minAreaY = target.localPosition.y - target.childCount * cellHeight;
					float maxAreaY = target.localPosition.y;
					
					float contentY = GetPanel().clipRange.w;
					float areaY = target.childCount * cellHeight;

					if (contentY > areaY)
					{
						float diff = contentY - areaY;
						minAreaY -= diff;
						maxAreaY += diff;
					}
					
					if (minRectY < minAreaY) offset.y += minAreaY - minRectY;
					if (maxRectY > maxAreaY) offset.y -= maxRectY - maxAreaY;
					
					if(offset.magnitude > 0)
					{
						SpringPosition sp = SpringPosition.Begin(target.gameObject, target.localPosition + offset, 13f);
						sp.ignoreTimeScale = true;
						sp.worldSpace = false;
					}
				}
			}
		}
	}

	/// <summary>
	/// Drag the object along the plane.
	/// </summary>

	void OnDrag (Vector2 delta)
	{
		if (enabled && gameObject.activeSelf && target != null)
		{
			Ray ray = UICamera.lastCamera.ScreenPointToRay(UICamera.lastTouchPosition);
			float dist = 0f;

			if (mPlane.Raycast(ray, out dist))
			{
				Vector3 currentPos = ray.GetPoint(dist);
				Vector3 offset = currentPos - mLastPos;
				mLastPos = currentPos;

				if (offset.x != 0f || offset.y != 0f)
				{
					offset = target.InverseTransformDirection(offset);
					offset.Scale(scale);
					offset = target.TransformDirection(offset);

					// Adjust the position
					target.position += offset;
				}
			}
		}
	}

	/// <summary>
	/// Apply the dragging momentum.
	/// </summary>

	void LateUpdate ()
	{
		if (target == null) return;

		if (mPressed)
		{
			// Disable the spring movement
			SpringPosition sp = target.GetComponent<SpringPosition>();
			if (sp != null) sp.enabled = false;
		}
//		else
//		{
//			mMomentum += scale * (-mScroll * 0.05f);
//			mScroll = NGUIMath.SpringLerp(mScroll, 0f, 20f, delta);
//
//			if (mMomentum.magnitude > 0.0001f)
//			{
//				// Apply the momentum
//				if (mPanel == null) FindPanel();
//
//				if (mPanel != null)
//				{
//					target.position += NGUIMath.SpringDampen(ref mMomentum, 9f, delta);
//
//					if (restrictWithinPanel && mPanel.clipping != UIDrawCall.Clipping.None)
//					{
//						mBounds = NGUIMath.CalculateRelativeWidgetBounds(mPanel.cachedTransform, target);
//						
//						if(backTop && (mBounds.max.y - mBounds.min.y < mPanel.clipRange.w) && (mBounds.max.y  < mPanel.clipRange.y + mPanel.clipRange.w*0.5f))
//						{
//							if (!mPanel.ConstrainTargetToBounds1Top(target, ref mBounds, false))
//							{
//								SpringPosition sp = target.GetComponent<SpringPosition>();
//								if (sp != null) sp.enabled = false;
//							}
//						}
//						else
//						{
//							if (!mPanel.ConstrainTargetToBounds(target, ref mBounds, dragEffect == DragEffect.None))
//							{
//								SpringPosition sp = target.GetComponent<SpringPosition>();
//								if (sp != null) sp.enabled = false;
//							}
//						}
//					}
//				}
//			}
//		}
	}
}
