using UnityEngine;
using System.Collections;

public class ClipOccur : MonoBehaviour {
	
	public UIPanel parent;
	
	private UIPanel mSelf;
	private Vector4 mBackClipRange;
	
	// Use this for initialization
	void Start () {
		mSelf = GetComponent<UIPanel>();
		mBackClipRange = mSelf.clipRange;
	}

    public void SetUIPanel(UIPanel obj)
    {
        parent=obj;
    }
	
	// Update is called once per frame
	void LateUpdate () {
		Vector3 vMinSelf = Vector3.zero, vMaxSelf = Vector3.zero;
		RangeToPoints(mBackClipRange, ref vMinSelf, ref vMaxSelf);
		
		Vector3 vMinParent = Vector3.zero, vMaxParent = Vector3.zero;
		RangeToPoints(parent.clipRange, ref vMinParent, ref vMaxParent);
		
		// to world
		vMinSelf = mSelf.gameObject.transform.localToWorldMatrix.MultiplyPoint(vMinSelf);
		vMaxSelf = mSelf.gameObject.transform.localToWorldMatrix.MultiplyPoint(vMaxSelf);
		
		vMinParent = parent.gameObject.transform.localToWorldMatrix.MultiplyPoint(vMinParent);
		vMaxParent = parent.gameObject.transform.localToWorldMatrix.MultiplyPoint(vMaxParent);
		
		Vector3[] clip = NGUIMath.ClipRectangle(vMinSelf, vMaxSelf, vMinParent, vMaxParent);
		
		// to self local
		if (clip != null)
		{
			clip[0] = mSelf.gameObject.transform.worldToLocalMatrix.MultiplyPoint(clip[0]);
			clip[1] = mSelf.gameObject.transform.worldToLocalMatrix.MultiplyPoint(clip[1]);
			float w = clip[1].x - clip[0].x;
			float h = clip[1].y - clip[0].y;
		
			Vector4 cliped = mSelf.clipRange;
			cliped.x = clip[0].x + w / 2;
			cliped.y = clip[0].y + h / 2;
			cliped.z = w;
			cliped.w = h;
			mSelf.clipRange = cliped;
		}
		else
		{
			Vector4 cliped = mSelf.clipRange;
			cliped.x = cliped.y = cliped.w = cliped.z = 1;
			mSelf.clipRange = cliped;
		}
	}
	
	void RangeToPoints(Vector4 range, ref Vector3 v0, ref Vector3 v1)
	{
		v0.z = v1.z = 0;
		v0.x = range.x - range.z / 2;
		v0.y = range.y - range.w / 2;
		v1.x = range.x + range.z / 2;
		v1.y = range.y + range.w / 2;
	}
}
