using UnityEngine;

[AddComponentMenu("NGUI/Examples/Spin With Mouse")]
public class SpinWithMouse : MonoBehaviour
{
	public Transform target;

	Transform mTrans;

	void Start ()
	{
		mTrans = transform;
	}

	void OnDrag (Vector2 delta)
	{
		if (target != null)
		{
			target.localRotation = Quaternion.Euler(0f, -0.5f * delta.x, 0f) * target.localRotation;
		}
		else
		{
			mTrans.localRotation = Quaternion.Euler(0f, -0.5f * delta.x, 0f) * mTrans.localRotation;
		}
	}
}