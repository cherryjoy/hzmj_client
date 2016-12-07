using UnityEngine;
using System.Collections;

public class AutoFullScreenScale : MonoBehaviour {
	void Reset()
	{
		Camera cam = UIRoot.mSelf.mCamera;
		Transform root = UIRoot.mSelf.transform;
		
		Vector3 vSize = new Vector3(960.0f, 640.0f, 0.0f);
		Vector3 vCache = vSize;
		
		vSize.x *= root.localScale.x;
		vSize.y *= root.localScale.y;
		vSize = cam.WorldToScreenPoint(vSize);
		vSize.x = vCache.x * vCache.x / 960 * Screen.width / (vSize.x - Screen.width * 0.5f);
		float rate = vSize.x / transform.localScale.x;
		Vector3 scale = transform.localScale;
		{
			scale.x = vSize.x;
			scale.y = scale.y * rate;
			transform.localScale = scale;
		}
	}

	void LateUpdate()
	{
		Reset();
	}
}
