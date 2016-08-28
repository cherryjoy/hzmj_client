using UnityEngine;

public class SpringRotation : IgnoreTimeScale
{
	public Vector3 target = Vector3.zero;
	public float strength = 10f;
	public bool ignoreTimeScale = false;
	public GameObject EventReceiver;
	private string SpringEndCall = "OnSpringEnd";

	Transform mTrans;
	float mThreshold = 0f;

	void Start() { mTrans = transform; }


	void Update()
	{
		float delta = ignoreTimeScale ? UpdateRealTimeDelta() : Time.deltaTime;

		if (mThreshold == 0f) mThreshold = (target.z - mTrans.localEulerAngles.z) * 0.005f;
		mTrans.localEulerAngles = NGUIMath.SpringLerp(mTrans.localEulerAngles, target, strength, delta);
		if (mThreshold >= target.z - mTrans.localEulerAngles.z)
		{
			enabled = false;
			if (EventReceiver != null)
			{
				EventReceiver.SendMessage(SpringEndCall, SendMessageOptions.DontRequireReceiver);
			}
		}
	}



	static public SpringRotation Begin(GameObject go, Vector3 rot, float strength)
	{
		SpringRotation sp = go.GetComponent<SpringRotation>();
		if (sp == null) sp = go.AddComponent<SpringRotation>();
		sp.target = rot;
		sp.strength = strength;

		if (!sp.enabled)
		{
			sp.mThreshold = 0f;
			sp.enabled = true;
		}
		return sp;
	}
}
