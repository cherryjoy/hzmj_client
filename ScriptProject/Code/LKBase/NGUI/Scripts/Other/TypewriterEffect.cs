using UnityEngine;

/// <summary>
/// Trivial script that fills the label's contents gradually, as if someone was typing.
/// </summary>

[RequireComponent(typeof(UILabel))]
[AddComponentMenu("NGUI/Examples/Typewriter Effect")]
public class TypewriterEffect : MonoBehaviour
{
	public int charsPerSecond = 40;
	public GameObject EventReceiver;
    private string CallWhenFinished = "OnEventWithMessage";

	UILabel mLabel;
	string mText;
	int mOffset = 0;
	float mNextChar = 0f;

	void LateUpdate ()
	{
		if (mLabel == null)
		{
			mLabel = GetComponent<UILabel>();
			mText = mLabel.font.WrapText(mLabel.text, mLabel.lineWidth / mLabel.cachedTransform.localScale.x, true, true, mLabel.FontSize);
		}

		if (mOffset < mText.Length)
		{
			if (mNextChar <= Time.time)
			{
				charsPerSecond = Mathf.Max(1, charsPerSecond);

				// Periods and end-of-line characters should pause for a longer time.
				float delay = 1f / charsPerSecond;
				char c = mText[mOffset];
				if (c == '.' || c == '\n' || c == '!' || c == '?') delay *= 4f;

				mNextChar = Time.time + delay;
				mLabel.text = mText.Substring(0, ++mOffset);
			}
		}
		else
		{
			if ((EventReceiver != null) && (!string.IsNullOrEmpty(CallWhenFinished)))
            {
                GameObject go = EventReceiver;
                if (go == null) go = gameObject;
                LuaMessage msg = new LuaMessage(gameObject, this);
                go.SendMessage(CallWhenFinished, msg, SendMessageOptions.DontRequireReceiver);
            }

			Destroy(this);
		}
	}
    public void Done()
    {
        if (mLabel != null)
        {
            mLabel.text = mText;

            if ((EventReceiver != null) && (!string.IsNullOrEmpty(CallWhenFinished)))
            {
                GameObject go = EventReceiver;
                if (go == null) go = gameObject;
                LuaMessage msg = new LuaMessage(gameObject, this);
                go.SendMessage(CallWhenFinished, msg, SendMessageOptions.DontRequireReceiver);
            }

            Destroy(this);
        }
    }
}
