using UnityEngine;
using System.Collections;

public class TweenWait : MonoBehaviour
{
	public delegate void OnTweenWaitDelegate();

	static public void Begin(GameObject go, OnTweenWaitDelegate func, float duration)
	{
		TweenWait comp = go.GetComponent<TweenWait>();
#if UNITY_FLASH
		if ((object)comp == null) comp = (TweenWait)go.AddComponent<TweenWait>();
#else
		if (comp == null) comp = go.AddComponent<TweenWait>();
#endif
		comp.BeginStartCoroutine(func, duration);
	}

	public void BeginStartCoroutine(OnTweenWaitDelegate func, float duration)
	{
		StartCoroutine(DoWait(func, duration));
	}

	IEnumerator DoWait(OnTweenWaitDelegate waitBack, float dur)
	{
		yield return new WaitForSeconds(dur);
		if (waitBack != null)
			waitBack();
        Destroy(this);
	}
}
