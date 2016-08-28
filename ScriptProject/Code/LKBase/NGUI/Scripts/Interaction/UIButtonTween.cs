﻿//----------------------------------------------
//            NGUI: Next-Gen UI kit
// Copyright © 2011-2012 Tasharen Entertainment
//----------------------------------------------

using UnityEngine;
using AnimationOrTween;

/// <summary>
/// Attaching this to an object lets you activate tweener components on other objects.
/// </summary>

[AddComponentMenu("NGUI/Interaction/Button Tween")]
public class UIButtonTween : MonoBehaviour
{
	public GameObject tweenTarget;
	public int tweenGroup = 0;
	public Trigger trigger = Trigger.OnClick;
	public Direction playDirection = Direction.Forward;
	public bool resetOnPlay = false;
	public EnableCondition ifDisabledOnPlay = EnableCondition.DoNothing;
	public DisableCondition disableWhenFinished = DisableCondition.DoNotDisable;
	public bool includeChildren = false;

	UITweener[] mTweens;

	void Start () { if (tweenTarget == null) tweenTarget = gameObject; }

	void OnHover (bool isOver)
	{
		if (enabled)
		{
			if (trigger == Trigger.OnHover ||
				(trigger == Trigger.OnHoverTrue && isOver) ||
				(trigger == Trigger.OnHoverFalse && !isOver))
			{
				Play(isOver);
			}
		}
	}

	void OnPress (bool isPressed)
	{
		if (enabled)
		{
			if (trigger == Trigger.OnPress ||
				(trigger == Trigger.OnPressTrue && isPressed) ||
				(trigger == Trigger.OnPressFalse && !isPressed))
			{
				Play(isPressed);
			}
		}
	}

	void OnClick ()
	{
		if (enabled && trigger == Trigger.OnClick)
		{
			Play(true);
		}
	}

	void Update ()
	{
		if (disableWhenFinished != DisableCondition.DoNotDisable && (int)playDirection == (int)disableWhenFinished && mTweens != null)
		{
			bool isFinished = true;

			foreach (UITweener tw in mTweens)
			{
				if (tw.enabled)
				{
					isFinished = false;
					break;
				}
			}

			if (isFinished)
			{
				tweenTarget.SetActive(false);
				mTweens = null;
			}
		}
	}

	/// <summary>
	/// Activate the tweeners.
	/// </summary>

	void Play (bool forward)
	{
		GameObject go = (tweenTarget == null) ? gameObject : tweenTarget;

		if (!go.activeSelf)
		{
			// If the object is disabled, don't do anything
			if (ifDisabledOnPlay != EnableCondition.EnableThenPlay) return;

			// Enable the game object before tweening it
			go.SetActive(true);
		}

		// Gather the tweening components
		mTweens = includeChildren ? go.GetComponentsInChildren<UITweener>() : go.GetComponents<UITweener>();

		if (mTweens.Length == 0)
		{
			// No tweeners found -- should we disable the object?
			if (disableWhenFinished != DisableCondition.DoNotDisable) tweenTarget.SetActive(false);
		}
		else
		{
			bool activated = false;
			if (playDirection == Direction.Reverse) forward = !forward;

			// Run through all located tween components
			foreach (UITweener tw in mTweens)
			{
				// If the tweener's group matches, we can work with it
				if (tw.tweenGroup == tweenGroup)
				{
					// Ensure that the game objects are enabled
					if (!activated && !go.activeSelf)
					{
						activated = true;
						go.SetActive(true);
					}

					// Toggle or activate the tween component
					if (playDirection == Direction.Toggle) tw.Toggle();
					else tw.Play(forward);
					if (resetOnPlay) tw.Reset();
				}
			}
		}
	}
}