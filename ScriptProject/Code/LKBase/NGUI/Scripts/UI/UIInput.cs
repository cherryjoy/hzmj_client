//----------------------------------------------
//            NGUI: Next-Gen UI kit
// Copyright © 2011-2012 Tasharen Entertainment
//----------------------------------------------

using UnityEngine;

/// <summary>
/// Editable text input field.
/// </summary>

[AddComponentMenu("NGUI/UI/Input (Basic)")]
public class UIInput : MonoBehaviour
{
	public UILabel label;
	public int maxChars = 0;
	public string caratChar = "|";
	public Color activeColor = Color.white;
    public GameObject MsgReceiver;

	string mText = "";
	string mDefaultText = "";
	public string DefaultText
	{
		get { return mDefaultText; }
	}
	Color mDefaultColor = Color.white;

    private static readonly string OnInputGotFocus = "OnInputGotFocus";
    private static readonly string OnInputLostFocus = "OnInputLostFocus";

#if UNITY_IPHONE || UNITY_ANDROID
#if UNITY_3_4
	iPhoneKeyboard mKeyboard;
#else
	TouchScreenKeyboard mKeyboard;
#endif
#else
	string mLastIME = "";
#endif

	/// <summary>
	/// Input field's current text value.
	/// </summary>

	public string text
	{
		get
		{
			if (selected) return mText;
			return (label != null) ? label.text : "";
		}
		set
		{
			mText = value;

			if (label != null)
			{
				label.supportEncoding = false;
				label.text = selected ? value + caratChar : value;
				label.showLastPasswordChar = selected;
			}
		}
	}

	/// <summary>
	/// Whether the input is currently selected.
	/// </summary>

	public bool selected
	{
		get
		{
			return UICamera.selectedObject == gameObject;
		}
		set
		{
			if (!value && UICamera.selectedObject == gameObject) UICamera.selectedObject = null;
			else if (value) UICamera.selectedObject = gameObject;
		}
	}

	/// <summary>
	/// Labels used for input shouldn't support color encoding.
	/// </summary>

	protected void Init ()
	{
		if (label == null) label = GetComponentInChildren<UILabel>();
		if (label != null)
		{
			mDefaultText = label.text;
			mDefaultColor = label.color;
			label.supportEncoding = false;
		}
	}

	/// <summary>
	/// Initialize everything on start.
	/// </summary>

	void Start () { Init(); }

	/// <summary>
	/// Selection event, sent by UICamera.
	/// </summary>

	void OnSelect (bool isSelected)
	{
		if (label != null && enabled && gameObject.activeSelf)
		{
			if (isSelected)
			{
				mText = (label.text == mDefaultText) ? "" : label.text;
				label.color = activeColor;

#if UNITY_IPHONE || UNITY_ANDROID
				if (Application.platform == RuntimePlatform.IPhonePlayer ||
					Application.platform == RuntimePlatform.Android)
				{
#if UNITY_3_4
					mKeyboard = iPhoneKeyboard.Open(mText);
#else
					mKeyboard = TouchScreenKeyboard.Open(mText);
#endif
					UpdateLabel();
				}
				else
#endif
				{
					Input.imeCompositionMode = IMECompositionMode.On;
					Transform t = label.cachedTransform;
					Vector3 offset = label.pivotOffset;
					offset.y += label.relativeSize.y;
					offset = t.TransformPoint(offset);
					Input.compositionCursorPos = UICamera.lastCamera.WorldToScreenPoint(offset);
					UpdateLabel();
				}

                NGUITools.Broadcast(UIInput.OnInputGotFocus, label);

				InvokeRepeating("FlashCarat", 0, 0.5f);
			}
#if UNITY_IPHONE || UNITY_ANDROID
			else if (mKeyboard != null)
			{
				mKeyboard.active = false;
				
				if (string.IsNullOrEmpty(mText))
				{
					label.text = mDefaultText;
					label.color = mDefaultColor;
				}
				else label.text = mText;

				label.showLastPasswordChar = false;
				CancelInvoke("FlashCarat");

                NGUITools.Broadcast(UIInput.OnInputLostFocus, label);
			}
#endif
            else
			{
				if (string.IsNullOrEmpty(mText))
				{
					label.text = mDefaultText;
					label.color = mDefaultColor;
				}
				else label.text = mText;

				label.showLastPasswordChar = false;
				Input.imeCompositionMode = IMECompositionMode.Off;
				CancelInvoke("FlashCarat");

                NGUITools.Broadcast(UIInput.OnInputLostFocus, label);
			}
		}
	}

#if UNITY_IPHONE || UNITY_ANDROID
	/// <summary>
	/// Update the text and the label by grabbing it from the iOS/Android keyboard.
	/// </summary>

	void Update()
	{
		if (mKeyboard != null)
		{
			string text = mKeyboard.text;

			if (mText != text)
			{
				mText = text;
				UpdateLabel();
			}

			if (mKeyboard.done)
			{
				mKeyboard = null;
				gameObject.SendMessage("OnSubmit", SendMessageOptions.DontRequireReceiver);
                if (MsgReceiver != null)
                {
                    MsgReceiver.SendMessage("OnEventWithMessage", new LuaMessage(gameObject, this, "OnSubmit"), SendMessageOptions.DontRequireReceiver);
    
                }
				selected = false;
			}

            if(mKeyboard != null && !mKeyboard.active)
            {
                selected = false;
            }
		}
	}
#else
    void Update ()
	{
		if (mLastIME != Input.compositionString)
		{
			mLastIME = Input.compositionString;
			UpdateLabel();
		}
        if (selected == true && (Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl)))
        {
            if (Input.GetKeyDown(KeyCode.V))
            {
                mText = ClipboardHelper.clipBoard;
                UpdateLabel();
            }
            if (Input.GetKeyDown(KeyCode.C))
            {
                ClipboardHelper.clipBoard = mText;
            }
        }
	}
#endif

	/// <summary>
	/// Input event, sent by UICamera.
	/// </summary>

	void OnInput (string input)
	{
		if (selected && enabled && gameObject.activeSelf)
		{
			// Mobile devices handle input in Update()
			if (Application.platform == RuntimePlatform.Android) return;
			if (Application.platform == RuntimePlatform.IPhonePlayer) return;

			foreach (char c in input)
			{
				if (c == '\b')
				{
					// Backspace
					if (mText.Length > 0) mText = mText.Substring(0, mText.Length - 1);
				}
				else if (c == '\r' || c == '\n')
				{
					// Enter
					gameObject.SendMessage("OnSubmit", SendMessageOptions.DontRequireReceiver);
                    if (MsgReceiver != null)
                    {
                        MsgReceiver.SendMessage("OnEventWithMessage", new LuaMessage(gameObject, this, "OnSubmit"), SendMessageOptions.DontRequireReceiver);
    
                    }
					selected = false;
					return;
				}
				else
				{
					// All other characters get appended to the text
					mText += c;
				}
			}

			// Ensure that we don't exceed the maximum length
			UpdateLabel();
		}
	}
	
	bool mShowCarat = false;
	
	void FlashCarat()
	{
		mShowCarat = !mShowCarat;	
		UpdateLabel();
	}

	/// <summary>
	/// Update the visual text label, capping it at maxChars correctly.
	/// </summary>

	void UpdateLabel ()
	{
		if (maxChars > 0 && mText.Length > maxChars) mText = mText.Substring(0, maxChars);

		if (label.font != null)
		{
			// Start with the text and append the IME composition and carat chars
			string processed = (selected && mShowCarat) ? (mText + Input.compositionString + caratChar) : mText + " ";

			// Now wrap this text using the specified line width
			processed = label.font.WrapText(processed, label.lineWidth / label.cachedTransform.localScale.x, true, false, label.FontSize);

			if (!label.multiLine)
			{
				// Split it up into lines
				string[] lines = processed.Split(new char[] { '\n' });

				// Only the last line should be visible
				processed = (lines.Length > 0) ? lines[lines.Length - 1] : "";
			}
			// Update the label's visible text
			label.text = processed;
			label.showLastPasswordChar = selected;
		}
	}
}
