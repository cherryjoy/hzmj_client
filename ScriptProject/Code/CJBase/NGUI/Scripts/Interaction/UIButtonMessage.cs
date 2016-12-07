﻿//----------------------------------------------
//            NGUI: Next-Gen UI kit
// Copyright © 2011-2012 Tasharen Entertainment
//----------------------------------------------

using UnityEngine;

/// <summary>
/// Sends a message to the remote object when something happens.
/// </summary>

[AddComponentMenu("NGUI/Interaction/Button Message")]
public class UIButtonMessage : MonoBehaviour
{
	public enum Trigger
	{
		OnClick,
		OnMouseOver,
		OnMouseOut,
		OnPress,
		OnRelease,
	}

	public GameObject target;
	public string functionName;
	public Trigger trigger = Trigger.OnClick;
	public bool includeChildren = false;

	void OnHover (bool isOver)
	{
		if (((isOver && trigger == Trigger.OnMouseOver) ||
			(!isOver && trigger == Trigger.OnMouseOut))) Send();
	}

	void OnPress (bool isPressed)
	{
		if (((isPressed && trigger == Trigger.OnPress) ||
			(!isPressed && trigger == Trigger.OnRelease))) Send();
	}

	void OnClick ()
	{
		if (trigger == Trigger.OnClick) Send();
	}

	void Send ()
	{
		if (!enabled || !gameObject.activeSelf || string.IsNullOrEmpty(functionName)) return;
        if (target == null) {
            Debug.Log(gameObject.name + " : Target is Null");
            return;
        } 

		if (includeChildren)
		{
			Transform[] transforms = target.GetComponentsInChildren<Transform>();

			foreach (Transform t in transforms)
			{
				t.gameObject.SendMessage(functionName, gameObject, SendMessageOptions.DontRequireReceiver);
			}
		}
		else
		{
			target.SendMessage(functionName, gameObject, SendMessageOptions.DontRequireReceiver);
		}
	}
}