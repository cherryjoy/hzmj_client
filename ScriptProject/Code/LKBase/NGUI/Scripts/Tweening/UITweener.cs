//----------------------------------------------
//            NGUI: Next-Gen UI kit
// Copyright © 2011-2012 Tasharen Entertainment
//----------------------------------------------

using UnityEngine;
using System.Collections;

/// <summary>
/// Base class for all tweening operations.
/// </summary>

public abstract class UITweener : IgnoreTimeScale
{
	public enum Method
	{
		Linear,
		EaseIn,
		EaseOut,
		EaseInOut,
		Acceler,
		Decelerate,
	}

	public enum Style
	{
		Once,
		Loop,
		PingPong,
		PingPongLoop,
	}

	/// <summary>
	/// Tweening method used.
	/// </summary>

	public Method method = Method.Linear;

	public bool mDestroyAfterTween = false;

	/// <summary>
	/// Does it play once? Does it loop?
	/// </summary>

	public Style style = Style.Once;

	/// <summary>
	/// How long is the duration of the tween?
	/// </summary>

	public float duration = 1f;

	/// <summary>
	/// Used by buttons and tween sequences. Group of '0' means not in a sequence.
	/// </summary>

	public int tweenGroup = 0;

	/// <summary>
	/// Target used with 'callWhenFinished', or this game object if none was specified.
	/// </summary>

	public GameObject eventReceiver;

	/// <summary>
	/// Name of the function to call when the tween finishes.
	/// </summary>

    public string callWhenFinished = "OnEventWithMessage";
    public bool isSenderToMessage = true;

    public float delayTime = 0f;
	public float waitTime;
    public string callWhenWait = "OnEventWithMessage";

	float mDuration = 0f;
	float mAmountPerDelta = 1f;
	float mFactor = 0f;
	bool  mWait = false;
    public object data;

    public bool IsSendMsg = true;

	protected UIWidget[] chidrenWidgets;

	/// <summary>
	/// Amount advanced per delta time.
	/// </summary>

	float amountPerDelta
	{
		get
		{
			if (mDuration != duration)
			{
				mDuration = duration;
				mAmountPerDelta = Mathf.Abs((duration > 0f) ? 1f / duration : 1000f);
			}
			return mAmountPerDelta;
		}
	}

	/// <summary>
	/// Update on start, so there is no frame in-between.
	/// </summary>

	void Start () {
		if (chidrenWidgets == null || chidrenWidgets.Length == 0)
		{
			UIWidget[] widgets = GetComponentsInChildren<UIWidget>(true);
			UIWidget[] selfWidgets = GetComponents<UIWidget>();
			chidrenWidgets = NGUITools.CombieArray<UIWidget>(widgets, selfWidgets);
		}
		Update();
        gameObject.transform.SetParent();
	}

	/// <summary>
	/// Update the tweening factor and call the virtual update function.
	/// </summary>

	void Update ()
	{
		float delta = UpdateRealTimeDelta();
        //delay play tween animation
        if (delayTime > 0) 
        {
            delayTime -= delta;
            return;
        }
		if(mWait)
		{
			waitTime -= delta;
			if(waitTime <= 0)
			{
				mFactor = Mathf.Clamp01(mFactor);

				if (string.IsNullOrEmpty(callWhenFinished))
				{
					enabled = false;
				}
				else
				{
					// Notify the event listener target
					GameObject go = eventReceiver;
					if (go == null) go = gameObject;
                    LuaMessage msg = new LuaMessage(isSenderToMessage?gameObject:null, this, "TweenEnd");
                    go.SendMessage(callWhenFinished, msg, SendMessageOptions.DontRequireReceiver);

					// Disable this script unless the SendMessage function above changed something
					if (mFactor == 1f && mAmountPerDelta > 0f || mFactor == 0f && mAmountPerDelta < 0f)
					{
						enabled = false;
					}
				}
			}
			return;
		}

		// Advance the sampling factor
		if(method == Method.Acceler)
			mFactor += amountPerDelta * delta * 1.0f/(1.0f-mFactor);
		else if(method == Method.Decelerate)
			mFactor += amountPerDelta * delta * 1.0f / (1.0f + mFactor * 0.5f);
		else 
			mFactor += amountPerDelta * delta;

		// Loop style simply resets the play factor after it exceeds 1.
		if (style == Style.Loop)
		{
			if (mFactor > 1f)
			{
				mFactor -= Mathf.Floor(mFactor);
			}
		}
		else if (style == Style.PingPong)
		{
			// Ping-pong style reverses the direction
			if (mFactor > 1f)
			{
				mFactor = 1f - (mFactor - Mathf.Floor(mFactor));
				mAmountPerDelta = -mAmountPerDelta;
			}
//			Modifyed by dc.......
//			else if (mFactor < 0f)
//			{
//				mFactor = -mFactor;
//				mFactor -= Mathf.Floor(mFactor);
//				mAmountPerDelta = -mAmountPerDelta;
//			}
		}
		else if (style == Style.PingPongLoop)
		{
			// Ping-pong style reverses the direction
			if (mFactor > 1f)
			{
				mFactor = 1f - (mFactor - Mathf.Floor(mFactor));
				mAmountPerDelta = -mAmountPerDelta;
			}
			else if (mFactor < 0f)
			{
				mFactor = -mFactor;
				mFactor -= Mathf.Floor(mFactor);
				mAmountPerDelta = -mAmountPerDelta;
			}
		}

		// Calculate the sampling value
		float val = Mathf.Clamp01(mFactor);

		if (method == Method.EaseIn)
		{
			val = 1f - Mathf.Sin(0.5f * Mathf.PI * (1f - val));
		}
		else if (method == Method.EaseOut)
		{
			val = Mathf.Sin(0.5f * Mathf.PI * val);
		}
		else if (method == Method.EaseInOut)
		{
			const float pi2 = Mathf.PI * 2f;
			val = val - Mathf.Sin(val * pi2) / pi2;
		}

		if (chidrenWidgets != null && chidrenWidgets.Length > 0)
		{
			for (int i = 0; i < chidrenWidgets.Length; i++)
			{
				chidrenWidgets[i].MarkAsChanged();
			}
		}
		// Call the virtual update
		OnUpdate(val);

		// If the factor goes out of range and this is a one-time tweening operation, disable the script
		if ((style == Style.Once || style == Style.PingPong)&& (mFactor > 1f || mFactor < 0f))
		{
			if(waitTime > 0)
			{
				mWait = true;
				
				if (string.IsNullOrEmpty(callWhenWait) == false)
				{
					GameObject go = eventReceiver;
					if (go == null) go = gameObject;
                    LuaMessage msg = new LuaMessage(isSenderToMessage ? gameObject : null, this, "TweenWait");
                    go.SendMessage(callWhenWait, msg, SendMessageOptions.DontRequireReceiver);
				}
			}
			else
			{
				mFactor = Mathf.Clamp01(mFactor);

				if (string.IsNullOrEmpty(callWhenFinished))
				{
					enabled = false;
				}
				else
				{
					// Notify the event listener target
					GameObject go = eventReceiver;
					if (go == null) go = gameObject;
                    LuaMessage msg = new LuaMessage(isSenderToMessage ? gameObject : null, this, "TweenEnd");
                    go.SendMessage(callWhenFinished, msg, SendMessageOptions.DontRequireReceiver);

					// Disable this script unless the SendMessage function above changed something
					if (mFactor == 1f && mAmountPerDelta > 0f || mFactor == 0f && mAmountPerDelta < 0f)
					{
						enabled = false;
					}
				}
				if (mDestroyAfterTween)
				{
					GameObject.Destroy(gameObject);
				}
				
			}
		}
	}

	/// <summary>
	/// Manually activate the tweening process, reversing it if necessary.
	/// </summary>

	public void Play (bool forward)
	{
		mAmountPerDelta = Mathf.Abs(amountPerDelta);
		if (!forward) mAmountPerDelta = -mAmountPerDelta;
		enabled = true;
	}

	[System.Obsolete("Use Tweener.Play instead")]
	public void Animate (bool forward) { Play(forward); }

	/// <summary>
	/// Manually reset the tweener's state to the beginning.
	/// </summary>

	public void Reset() { mFactor = (mAmountPerDelta < 0f) ? 1f : 0f; }
    public void ResetFactor(bool forward)
    {
        if (forward)
        {
            mFactor = 1;
        }
        else
        {
            mFactor = 0;
        }
    }
	/// <summary>
	/// Manually start the tweening process, reversing its direction.
	/// </summary>

	public void Toggle ()
	{
		if (mFactor > 0f)
		{
			mAmountPerDelta = -amountPerDelta;
		}
		else
		{
			mAmountPerDelta = Mathf.Abs(amountPerDelta);
		}
		enabled = true;
	}

	/// <summary>
	/// Actual tweening logic should go here.
	/// </summary>

	abstract protected void OnUpdate (float factor);

	/// <summary>
	/// Starts the tweening operation.
	/// </summary>

	static public T Begin<T> (GameObject go, float duration) where T : UITweener
	{
		T comp = go.GetComponent<T>();
#if UNITY_FLASH
		if ((object)comp == null) comp = (T)go.AddComponent<T>();
#else
        if (comp == null)
        {
            comp = go.AddComponent<T>();
            comp.style = Style.Once;
        }
#endif
		comp.duration = duration;
		comp.mFactor = 0f;
		comp.enabled = true;
		UIWidget[] widgets = comp.GetComponentsInChildren<UIWidget>(true);
		UIWidget[] selfWidgets = comp.GetComponents<UIWidget>();
		comp.chidrenWidgets = NGUITools.CombieArray<UIWidget>(widgets, selfWidgets);
		return comp;
	}

	static public T Begin<T>(GameObject go, float duration, bool destroyAferTween) where T : UITweener
	{
		T comp = go.GetComponent<T>();
#if UNITY_FLASH
		if ((object)comp == null) comp = (T)go.AddComponent<T>();
#else
		if (comp == null) comp = go.AddComponent<T>();
#endif
		comp.duration = duration;
		comp.mFactor = 0f;
		comp.style = Style.Once;
		comp.enabled = true;
		comp.mDestroyAfterTween = destroyAferTween;
		UIWidget[] widgets = comp.GetComponentsInChildren<UIWidget>(true);
		UIWidget[] selfWidgets =  comp.GetComponents<UIWidget>();
		comp.chidrenWidgets = NGUITools.CombieArray<UIWidget>(widgets, selfWidgets);
		return comp;
	}
}
