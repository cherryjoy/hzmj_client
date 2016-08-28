﻿//----------------------------------------------
//            NGUI: Next-Gen UI kit
// Copyright © 2011-2012 Tasharen Entertainment
//----------------------------------------------
using UnityEngine;

/// <summary>
/// Simple example script of how a button can be scaled visibly when the mouse hovers over it or it gets pressed.
/// </summary>

[AddComponentMenu("NGUI/Interaction/Button Scale")]
public class UIButtonScale : MonoBehaviour
{
	public Transform tweenTarget;
	public Vector3 hover = new Vector3 (1.1f, 1.1f, 1.1f);
	public Vector3 pressed = new Vector3 (1.05f, 1.05f, 1.05f);
	public float duration = 0.2f;
	Vector3 mScale = new Vector3(1,1,1);
	bool mInitDone = false;

	void Init ()
	{
		mInitDone = true;
		if (tweenTarget == null)
			tweenTarget = transform;
		mScale = tweenTarget.localScale;
	}
	
	void OnDisable ()
	{
		if (tweenTarget != null) {
			tweenTarget.localScale = mScale;
			Destroy( gameObject.GetComponent<TweenScale>() );
		}
	}

	void OnPress (bool isPressed)
	{
		if (enabled) {
			if (!mInitDone)
				Init ();
			TweenScale.Begin (tweenTarget.gameObject, duration, isPressed ? Vector3.Scale (mScale, pressed) : mScale).method = UITweener.Method.EaseInOut;
		}
	}

	void OnHover (bool isOver)
	{
		if (enabled) {
			if (!mInitDone)
				Init ();
			TweenScale.Begin (tweenTarget.gameObject, duration, isOver ? Vector3.Scale (mScale, hover) : mScale).method = UITweener.Method.EaseInOut;
		}
	}
}
