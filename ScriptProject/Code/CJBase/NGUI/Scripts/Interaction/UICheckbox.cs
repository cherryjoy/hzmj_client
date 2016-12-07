﻿//----------------------------------------------
//            NGUI: Next-Gen UI kit
// Copyright © 2011-2012 Tasharen Entertainment
//----------------------------------------------

using UnityEngine;
using AnimationOrTween;

/// <summary>
/// Simple checkbox functionality. If 'option' is enabled, checking this checkbox will uncheck all other checkboxes with the same parent.
/// </summary>

[AddComponentMenu("NGUI/Interaction/Checkbox")]
public class UICheckbox : MonoBehaviour
{
	//static public UICheckbox current;

	public GameObject checkObj;
	public UISprite checkSprite;
	public Animation checkAnimation;
	public GameObject eventReceiver;
	public string functionName = "OnActivate";
	public bool startsChecked = true;
	public bool option = false;
	public bool optionCanBeNone = false;
	public bool needAlphaChangeDuration = true;

	bool mChecked = true;
	Transform mTrans;

	/// <summary>
	/// Whether the checkbox is checked.
	/// </summary>

	public bool isChecked { get { return mChecked; } set { if (!option || value || optionCanBeNone) Set(value); } }

	/// <summary>
	/// Activate the initial state.
	/// </summary>

	void Awake ()
	{
		mTrans = transform;
		if (eventReceiver == null) eventReceiver = gameObject;
		mChecked = !startsChecked;
		Set(startsChecked);
	}

	/// <summary>
	/// Check or uncheck on click.
	/// </summary>

	void OnClick () { if (enabled) isChecked = !isChecked; }

	/// <summary>
	/// Fade out or fade in the checkmark and notify the target of OnChecked event.
	/// </summary>

	void Set (bool state)
	{
		if (mChecked != state)
		{
			// Uncheck all other checkboxes
			if (option && state)
			{
				UICheckbox[] cbs = mTrans.parent.GetComponentsInChildren<UICheckbox>();
				foreach (UICheckbox cb in cbs) if (cb != this) cb.Set(false);
			}

			// Remember the state
			mChecked = state;

			// Tween the color of the checkmark
			if (checkSprite != null)
			{
				Color c = checkSprite.color;
				c.a = mChecked ? 1f : 0f;

				if (needAlphaChangeDuration)
					TweenColor.Begin(checkSprite.gameObject, 0.2f, c);
				else
					checkSprite.color = c;
			}

			if (checkObj != null)
			{
				if (mChecked)
					checkObj.gameObject.SetActive(true);
				else
					checkObj.gameObject.SetActive(false);
			}
			

			// Send out the event notification
			if (eventReceiver != null && !string.IsNullOrEmpty(functionName))
			{
				//current = this;
                if (eventReceiver.GetComponent<LuaMessageReceiver>() != null)
                {
                    eventReceiver.SendMessage(functionName, new LuaMessage(gameObject, mChecked, "OnChecked"), SendMessageOptions.DontRequireReceiver);
                }
                else
                {
                    eventReceiver.SendMessage(functionName, mChecked, SendMessageOptions.DontRequireReceiver);
                }
			}

			// Play the checkmark animation
			if (checkAnimation != null)
			{
				ActiveAnimation.Play(checkAnimation, state ? Direction.Forward : Direction.Reverse);
			}
		}
	}
}
