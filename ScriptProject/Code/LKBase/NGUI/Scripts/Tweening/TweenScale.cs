﻿//----------------------------------------------
//            NGUI: Next-Gen UI kit
// Copyright © 2011-2012 Tasharen Entertainment
//----------------------------------------------

using UnityEngine;

/// <summary>
/// Tween the object's local scale.
/// </summary>

[AddComponentMenu("NGUI/Tween/Scale")]
public class TweenScale : UITweener
{
	public Vector3 from;
	public Vector3 to;
	public bool updateTable = false;

	Transform mTrans;
	UITable mTable;
	
	public Vector3 scale { 
        get {
            if (mTrans == null)
            {
                mTrans = transform;
            }
            return mTrans.localScale; 
        }

        set
        {
			mTrans.localScale = value; 
        } 
    }

	void Awake ()
	{
		mTrans = transform;
		if (updateTable) mTable = NGUITools.FindInParents<UITable>(gameObject);
	}

	override protected void OnUpdate (float factor)
	{
		mTrans.localScale = from * (1f - factor) + to * factor;
		if (mTable != null) mTable.repositionNow = true;
	}

	/// <summary>
	/// Start the tweening operation.
	/// </summary>

	static public TweenScale Begin (GameObject go, float duration, Vector3 scale)
	{
		TweenScale comp = UITweener.Begin<TweenScale>(go, duration);
		comp.from = comp.scale;
		comp.to = scale;
		return comp;
	}
}
