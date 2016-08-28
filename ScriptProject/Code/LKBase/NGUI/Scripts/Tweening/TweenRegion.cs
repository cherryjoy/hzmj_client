//----------------------------------------------
//            NGUI: Next-Gen UI kit
// Copyright © 2011-2012 Tasharen Entertainment
//----------------------------------------------

using UnityEngine;

/// <summary>
/// Tween the object's local scale.
/// </summary>

[AddComponentMenu("NGUI/Tween/Region")]
public class TweenRegion : UITweener
{
    public float from;
    public float to;
    public enum Direction
    {
        Horizontal,
        Vertical,
    }
    public Direction direction = Direction.Horizontal;
    
    UIWidget mWidget;

    public float regionValue
    {

        get {
            if (mWidget == null)
            {
                mWidget = GetComponentInChildren<UIWidget>();
            }
            if (direction == Direction.Horizontal)
            {
                return mWidget.drawRegion.z;
            }
            else
            {
                return mWidget.drawRegion.w;
            }
            
        }

        set
        {
            if (direction == Direction.Horizontal)
            {
                mWidget.drawRegion = new Vector4(mWidget.drawRegion.x, mWidget.drawRegion.y, value, mWidget.drawRegion.w);
            }
            else
            {
                mWidget.drawRegion = new Vector4(mWidget.drawRegion.x, mWidget.drawRegion.y, mWidget.drawRegion.z, value);
            }
        }
    }

    void Awake()
    {
        mWidget = GetComponentInChildren<UIWidget>();
    }

    override protected void OnUpdate(float factor)
    {
        regionValue = from * (1f - factor) + to * factor;
    }

    /// <summary>
    /// Start the tweening operation.
    /// </summary>

    static public TweenRegion Begin(GameObject go, float duration, float number)
    {
        TweenRegion comp = UITweener.Begin<TweenRegion>(go, duration);
        comp.from = comp.regionValue;
        comp.to = number;
        return comp;
    }
}
