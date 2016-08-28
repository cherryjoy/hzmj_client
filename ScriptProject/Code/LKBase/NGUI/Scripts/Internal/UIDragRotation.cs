using System;
using UnityEngine;

public class UIDragRotation : IgnoreTimeScale
{
	public Transform target;
	public float momentumAmount = 35f;

	Plane mPlane;
	Vector3 mLastPos;
	bool mPressed = false;
	float mMomentum = 0;

	void OnPress(bool pressed)
	{
		if (enabled && gameObject.activeSelf && target != null)
		{
			mPressed = pressed;
			if (pressed)
			{
				mMomentum = 0f;
				TweenRotation tr = target.GetComponent<TweenRotation>();
				if (tr != null) tr.enabled = false;

				mLastPos = UICamera.lastHit.point;
				mPlane = new Plane((UICamera.lastCamera.transform.rotation) * Vector3.back, mLastPos);
			} 
			else
			{
				float targetZ = target.localEulerAngles.z + mMomentum * 12f;
				int targetStar = GetIndexByRotationZ(targetZ);
				float targetRoaZ = GetStarRightRotation(targetStar);
				TweenRotation tr = TweenRotation.Begin(target.gameObject, Mathf.Abs(mMomentum) / 30 + 0.2f, Quaternion.AngleAxis(targetRoaZ, Vector3.forward));
				tr.method = UITweener.Method.EaseIn;
			}
		}
	}

	void OnDrag(Vector2 delta)
	{
		if (enabled && gameObject.activeSelf && target != null)
		{
			Ray ray = UICamera.lastCamera.ScreenPointToRay(UICamera.lastTouchPosition);
			float dist = 0f;
			if (mPlane.Raycast(ray, out dist))
			{
				Vector3 currentPos = ray.GetPoint(dist);
				Vector3 lastLocalPos = target.parent.InverseTransformPoint(mLastPos);
				mLastPos = currentPos;
				Vector3 curLocalPos = target.parent.InverseTransformPoint(currentPos);
				float degreeCur = Vector3.Angle(Vector3.up, curLocalPos);
				float degreeLast = Vector3.Angle(Vector3.up, lastLocalPos);
				float degreeOffset = degreeCur -degreeLast ;
				if (curLocalPos.x > 0)
				{
					degreeOffset = -degreeOffset;
				}
				mMomentum = Mathf.Lerp(mMomentum, degreeOffset * (realTimeDelta * momentumAmount), 0.5f);
				target.localEulerAngles+= new Vector3(0, 0, degreeOffset);
			}
		}
	}


	void LateUpdate()
	{
		float delta = UpdateRealTimeDelta();
		if (target == null) return;
		if (mPressed)
		{
			TweenRotation tr = target.GetComponent<TweenRotation>();
			if (tr != null) tr.enabled = false;
		}else
		{
			//if (Mathf.Abs(mMomentum) > 0.00001f)
			//{
			//    Vector3 localAngle = target.localEulerAngles;
			//    localAngle.z += NGUIMath.SpringDampen(ref mMomentum, 5f, delta);
			//    target.localEulerAngles = localAngle;
			//}
		}
	}

    int GetIndexByRotationZ(float zRot)
    {
        if (zRot <= 15 || zRot > 345)
        {
            return 1;
        }
        int begin = 15;
        int offset = 30;
        for (int i = 2; i <= 12; i++)
        {
            if (zRot >= begin && zRot <= begin + offset)
            {
                return i;
            }
            begin += 30;
        }
        return 1;
    }

    float GetStarRightRotation(int star)
    {
        return (star - 1) * 30f;
    }
}
