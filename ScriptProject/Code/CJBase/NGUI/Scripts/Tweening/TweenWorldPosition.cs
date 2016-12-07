using UnityEngine;

/// <summary>
/// Tween the object's position.
/// </summary>

[AddComponentMenu("NGUI/Tween/WorldPosition")]
public class TweenWorldPosition : UITweener
{
	public Vector3 from;
	public Vector3 to;

	Transform mTrans;

	public Vector3 position 
    { 
        get 
        { 
            if(mTrans == null) 
                mTrans = transform;
            return mTrans.position; 
        } 
        set { mTrans.position = value; } }

	void Awake() { mTrans = transform; }

	override protected void OnUpdate(float factor) {
		float z = mTrans.position.z;
		Vector3 tmpPos = from * (1f - factor) + to * factor; 
		tmpPos.z = z ;
		mTrans.position = tmpPos;

	}

	/// <summary>
	/// Start the tweening operation.
	/// </summary>

	static public TweenWorldPosition Begin(GameObject go, float duration, Vector3 pos, bool destroyAfterTween = false)
	{
		TweenWorldPosition comp = UITweener.Begin<TweenWorldPosition>(go, duration, destroyAfterTween);
		comp.from = comp.position;
		comp.to = pos;
		return comp;
	}
}
