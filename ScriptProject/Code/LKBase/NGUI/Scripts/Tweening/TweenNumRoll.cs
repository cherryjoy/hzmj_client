using UnityEngine;
using System.Text;

[AddComponentMenu("NGUI/Tween/TweenNumRoll")]
[RequireComponent(typeof(UILabel))]
public class TweenNumRoll : UITweener
{
	public int PrefixStrId;
	public int SuffixStrId;
	public long RequireNum;

	private UILabel mLabel;
	private StringBuilder mStrBuilder = new StringBuilder(10);
	private string mPrefixStr = string.Empty;
	private string mSuffixStr = string.Empty;

	void Awake() 
	{ 
		mLabel = this.GetComponent<UILabel>();
	}

	override protected void OnUpdate(float factor)
	{
		mStrBuilder.Remove(0, mStrBuilder.Length);

		mLabel.text = mStrBuilder.Append(mPrefixStr).Append((int)(RequireNum * factor)).Append(mSuffixStr).ToString();
	}

	static public TweenNumRoll Begin(GameObject go, float duration, long requireNum, int prefixId, int suffixId)
	{
		TweenNumRoll comp = UITweener.Begin<TweenNumRoll>(go, duration);
		comp.RequireNum = requireNum;
		comp.PrefixStrId = prefixId;
		comp.SuffixStrId = suffixId;

		return comp;
	}
}
