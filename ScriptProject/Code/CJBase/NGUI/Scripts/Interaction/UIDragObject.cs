//----------------------------------------------
//            NGUI: Next-Gen UI kit
// Copyright ?2011-2012 Tasharen Entertainment
//----------------------------------------------

using UnityEngine;
using System.Collections;

/// <summary>
/// Allows dragging of the specified target object by mouse or touch, optionally limiting it to be within the UIPanel's clipped rectangle.
/// </summary>

[AddComponentMenu("NGUI/Interaction/Drag Object")]
public class UIDragObject : IgnoreTimeScale
{
	public enum DragEffect
	{
		None,
		Momentum,
		MomentumAndSpring,
	}

	/// <summary>
	/// Target object that will be dragged.
	/// </summary>

	public Transform target;

	/// <summary>
	/// Scale value applied to the drag delta. Set X or Y to 0 to disallow dragging in that direction.
	/// </summary>

	public Vector3 scale = Vector3.one;

	/// <summary>
	/// Effect the scroll wheel will have on the momentum.
	/// </summary>

	public float scrollWheelFactor = 0f;

	/// <summary>
	/// Whether the dragging will be restricted to be within the parent panel's bounds.
	/// </summary>

	public bool restrictWithinPanel = false;

	/// <summary>
	/// Effect to apply when dragging.
	/// </summary>

	public DragEffect dragEffect = DragEffect.MomentumAndSpring;

	/// <summary>
	/// How much momentum gets applied when the press is released after dragging.
	/// </summary>

	public float momentumAmount = 25f;
	
	public bool backTop = false;//add by DC

    public bool IgnoreTexture = false;

	Plane mPlane;
	Vector3 mLastPos;
	UIPanel mPanel;
	bool mPressed = false;
	Vector3 mMomentum = Vector3.zero;
	float mScroll = 0f;
	Bounds mBounds;

	/// <summary>
	/// Find the panel responsible for this object.
	/// </summary>

	void FindPanel ()
	{
		mPanel = (target != null) ? UIPanel.Find(target.transform, false) : null;
		if (mPanel == null) restrictWithinPanel = false;
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
				if (restrictWithinPanel && mPanel == null) FindPanel();

				// Calculate the bounds
                if (restrictWithinPanel) mBounds = IgnoreTexture ? NGUIMath.GetBoundsIngnoreTexture(mPanel.cachedTransform, target) : NGUIMath.CalculateRelativeWidgetBounds(mPanel.cachedTransform, target);

				// Remove all momentum on press
				mMomentum = Vector3.zero;
				mScroll = 0f;

				// Disable the spring movement
				SpringPosition sp = target.GetComponent<SpringPosition>();
				if (sp != null) sp.enabled = false;

				// Remember the hit position
				mLastPos = UICamera.lastHit.point;

				// Create the plane to drag along
				Transform trans = UICamera.lastCamera.transform;
				mPlane = new Plane((mPanel != null ? mPanel.cachedTransform.rotation : trans.rotation) * Vector3.back, mLastPos);
			}
			else if (restrictWithinPanel && mPanel != null && mPanel.clipping != UIDrawCall.Clipping.None && dragEffect == DragEffect.MomentumAndSpring)
			{
				mPanel.ConstrainTargetToBounds(target, ref mBounds, false);
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

                offset = new Vector3(offset.x * scale.x, offset.y * scale.y, offset.z);

				if (offset.x != 0f || offset.y != 0f)
				{
					offset = target.InverseTransformDirection(offset);
					offset.Scale(scale);
					offset = target.TransformDirection(offset);
				}

				// Adjust the momentum
				mMomentum = Vector3.Lerp(mMomentum, offset * (realTimeDelta * momentumAmount), 0.5f);

				// We want to constrain the UI to be within bounds
				if (restrictWithinPanel)
				{
					// Adjust the position and bounds
					Vector3 localPos = target.localPosition;
					target.position += offset;
					mBounds.center = mBounds.center + (target.localPosition - localPos);
                    Vector3 nLocalPos = target.localPosition;
                    if(scale.x == 0)
                    {
                        target.localPosition = new Vector3(localPos.x, nLocalPos.y, nLocalPos.z); 
                    }else if(scale.y == 0)
                    {
                        target.localPosition = new Vector3(nLocalPos.x, localPos.y, nLocalPos.z); 
                    }
                    
					// Constrain the UI to the bounds, and if done so, eliminate the momentum
					if (dragEffect != DragEffect.MomentumAndSpring && mPanel.clipping != UIDrawCall.Clipping.None &&
						mPanel.ConstrainTargetToBounds(target, ref mBounds, true))
					{
						mMomentum = Vector3.zero;
						mScroll = 0f;
					}
				}
				else
				{
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
		float delta = UpdateRealTimeDelta();
		if (target == null) return;

		if (mPressed)
		{
			// Disable the spring movement
			SpringPosition sp = target.GetComponent<SpringPosition>();
			if (sp != null) sp.enabled = false;
			mScroll = 0f;
		}
		else
		{
			mMomentum += scale * (-mScroll * 0.05f);
			mScroll = NGUIMath.SpringLerp(mScroll, 0f, 20f, delta);

			if (mMomentum.magnitude > 0.0001f)
			{
				// Apply the momentum
				if (mPanel == null) FindPanel();

				if (mPanel != null)
				{
                    Vector3 oLocalPos = target.localPosition;
					target.position += NGUIMath.SpringDampen(ref mMomentum, 9f, delta);
                    Vector3 nLocalPos = target.localPosition;
                    if (scale.x == 0)
                    {
                        target.localPosition = new Vector3(oLocalPos.x, nLocalPos.y, nLocalPos.z);
                    }
                    else if (scale.y == 0)
                    {
                        target.localPosition = new Vector3(nLocalPos.x, oLocalPos.y, nLocalPos.z);
                    }

					if (restrictWithinPanel && mPanel.clipping != UIDrawCall.Clipping.None)
					{
                        mBounds = IgnoreTexture ? NGUIMath.GetBoundsIngnoreTexture(mPanel.cachedTransform, target) : NGUIMath.CalculateRelativeWidgetBounds(mPanel.cachedTransform, target);
						
						if(backTop && (mBounds.size.y < mPanel.clipRange.w) && (mBounds.max.y  < mPanel.clipRange.y + mPanel.clipRange.w*0.5f))
						{
							if (!mPanel.ConstrainTargetToBounds1Top(target, ref mBounds, false))
							{
								SpringPosition sp = target.GetComponent<SpringPosition>();
								if (sp != null) sp.enabled = false;
							}
						}
						else
						{
							if (!mPanel.ConstrainTargetToBounds(target, ref mBounds, dragEffect == DragEffect.None))
							{
								SpringPosition sp = target.GetComponent<SpringPosition>();
								if (sp != null) sp.enabled = false;
							}
						}
					}
				}
			}
			else mScroll = 0f;
		}
	}

    void OnDrawGizmos()
    {
//         mBounds = NGUIMath.CalculateRelativeWidgetBounds(mPanel.cachedTransform, target);
//         Gizmos.color = Color.blue;
//         Gizmos.DrawWireCube(mBounds.center, mBounds.size);
    }

	/// <summary>
	/// If the object should support the scroll wheel, do it.
	/// </summary>

	void OnScroll (float delta)
	{
		if (enabled && gameObject.activeSelf)
		{
			if (Mathf.Sign(mScroll) != Mathf.Sign(delta)) mScroll = 0f;
			mScroll += delta * scrollWheelFactor;
		}
	}
}
