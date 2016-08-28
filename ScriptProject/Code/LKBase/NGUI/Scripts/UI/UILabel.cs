//----------------------------------------------
//            NGUI: Next-Gen UI kit
// Copyright © 2011-2012 Tasharen Entertainment
//----------------------------------------------

using UnityEngine;
using System.Collections.Generic;

[ExecuteInEditMode]
[AddComponentMenu("NGUI/UI/Label")]
public class UILabel : UIWidget
{
	public enum Effect
	{
		None,
		Shadow,
		Outline,
        Shadow2,
	}
	[SerializeField]
	protected UIFont mNewFont;
	[SerializeField]
	protected string mText = "";
	[SerializeField]
	protected int mMaxLineWidth = 0;
	[SerializeField]
	protected bool mEncoding = true;
	[SerializeField]
	protected bool mMultiline = true;
	[SerializeField]
	protected bool mPassword = false;
	[SerializeField]
	protected bool mShowLastChar = false;
	[SerializeField]
	protected Effect mEffectStyle = Effect.None;
	[SerializeField]
	protected Color mEffectColor = Color.black;
	[SerializeField]
	protected Color mTopColor = Color.white;
	[SerializeField]
	protected Color mBottomColor = Color.white;
	[SerializeField]
	protected int mSpaceingY = 0;
	[SerializeField]
	protected int mSpaceingX = 0;
	[SerializeField]
	protected int mFontSize = 24;
	[SerializeField]
	protected FontStyle mFontStyle = FontStyle.Normal;
	[SerializeField]
	protected Font mTrueTypeFont;
	/// <summary>
	/// Obsolete, do not use. Use 'mMaxLineWidth' instead.
	/// </summary>
	[SerializeField]
	float mLineWidth = 0;


	bool mShouldBeProcessed = true;
	protected string mProcessedText = null;

	// Cached values, used to determine if something has changed and thus must be updated
    //Vector3 mLastScale = Vector3.one;
    int mLastFontWidth = 0;
	string mLastText = "";
	int mLastWidth = 0;
	bool mLastEncoding = true;
	bool mLastMulti = true;
	bool mLastPass = false;
	bool mLastShow = false;
	Effect mLastEffect = Effect.None;
	Color mLastColor = Color.black;

    protected Material mCacheMat;

    protected bool mSelfChanged = true;//if we need reCaculate the verts uvs and cols

    //It was used olny for Chinese.we don't suggest using it.
    [SerializeField]
    protected int fixedWidthForChinese = 0;

    public int FixedWidthForChinese
    {
        get { return fixedWidthForChinese; }
        set
        {
            if (fixedWidthForChinese != value)
            {
                fixedWidthForChinese = value;
                hasChanged = true; mSelfChanged = true;
            }
        }
    }

    //It was used olny for Chinese.we don't suggest using it.
    private int FixedSpacingXForChinese{
        get{
            int fixedSpaceX = 0;
            if (pivot == Pivot.Center && !multiLine && FixedWidthForChinese != 0 && mSpaceingX == 0)
            {
                string tempText = font.WrapText(mProcessedText, 100000f, false, mEncoding, FontSize, FontStyle, 0);
                int proTextLen = tempText.Length;
                if (proTextLen > 1)
                {
                    fixedSpaceX = (FixedWidthForChinese - FontSize * proTextLen) / (proTextLen - 1);
                    if (fixedSpaceX > 0)
                    {
                        return fixedSpaceX;
                    }
                }
            }

            return 0;
        }
    }

    
	/// <summary>
	/// Function used to determine if something has changed (and thus the geometry must be rebuilt)
	/// </summary>

	bool hasChanged
	{
		get
		{
			return mShouldBeProcessed ||
				mLastText != text ||
				mLastWidth != mMaxLineWidth ||
				mLastEncoding != mEncoding ||
				mLastMulti != mMultiline ||
				mLastPass != mPassword ||
				mLastShow != mShowLastChar ||
				mLastEffect != mEffectStyle ||
				mLastColor != mEffectColor;
		}
		set
		{
			if (value)
			{
				mChanged = true;
				mShouldBeProcessed = true;
			}
			else
			{
				mShouldBeProcessed = false;
				mLastText = text;
				mLastWidth = mMaxLineWidth;
				mLastEncoding = mEncoding;
				mLastMulti = mMultiline;
				mLastPass = mPassword;
				mLastShow = mShowLastChar;
				mLastEffect = mEffectStyle;
				mLastColor = mEffectColor;
			}
		}
	}

	public int FontSize
	{
		get { return mFontSize; }
		set
		{
			if (mFontSize != value)
			{
				mFontSize = value;
				hasChanged = true;mSelfChanged = true;
			}
		}
	}

	public FontStyle FontStyle
	{
		get { return mFontStyle; }
		set
		{
			if (mFontStyle != value)
			{
				mFontStyle = value;
				MarkAsChanged();
			}
		}
	}

	public Font TrueTypeFont
	{
		get {
			if (mTrueTypeFont == null)
			{
				return font.TrueTypeFont;
			}
			return mTrueTypeFont;
		}
		set
		{
			if (mTrueTypeFont == null || mTrueTypeFont != value)
			{
				mTrueTypeFont = value;
				MarkAsChanged();
				if (Application.isPlaying == false)
				{
#if UNITY_EDITOR
					UnityEditor.EditorUtility.SetDirty(this);
#endif
				}
			}
		}
	}

	public override Material material
	{
		get
		{
			if (mCacheMat == null)
			{
				mCacheMat = TrueTypeFont.material;
			}
			return mCacheMat;
		}
	}

	/// <summary>
	/// Set the font used by this label.
	/// </summary>

	public UIFont font
	{
		get
		{
			if (mNewFont == null)
			{
				mNewFont = UIFont.CreateInstance<UIFont>();
			}
			return mNewFont;
		}
		set
		{
			if (mNewFont != null && mNewFont != value)
			{
				mNewFont = value;
				mChanged = true;
				hasChanged = true;mSelfChanged = true;
				MarkAsChanged();
			}
		}
	}

	/// <summary>
	/// Text that's being displayed by the label.
	/// </summary>

	public string text
	{
		get
		{
			return mText;
		}
		set
		{
			if (string.IsNullOrEmpty(value))
			{
				if (!string.IsNullOrEmpty(mText)) mText = "";
				hasChanged = true;mSelfChanged = true;
				MarkAsChanged();
			}
			else if (mText != value)
			{
				mText = value;
				hasChanged = true;mSelfChanged = true;
				MarkAsChanged();
				ProcessText();
			}
		}
	}

	/// <summary>
	/// Whether this label will support color encoding in the format of [RRGGBB] and new line in the form of a "\\n" string.
	/// </summary>

	public bool supportEncoding
	{
		get
		{
			return mEncoding;
		}
		set
		{
			if (mEncoding != value)
			{
				mEncoding = value;
				hasChanged = true;mSelfChanged = true;
				if (value) mPassword = false;
			}
		}
	}

	/// <summary>
	/// Maximum width of the label in pixels.
	/// </summary>

	public int lineWidth
	{
		get
		{
			return mMaxLineWidth;
		}
		set
		{
			if (mMaxLineWidth != value)
			{
				mMaxLineWidth = value;
				hasChanged = true;mSelfChanged = true;
			}
		}
	}

	public int lineSpacingY
	{
		get
		{
			return mSpaceingY;
		}
		set
		{
			if (mSpaceingY != value)
			{
				mSpaceingY = value;
				hasChanged = true;mSelfChanged = true;
			}
		}
	}

	public int FontSpacingX
	{
		get
		{
            int fixedSpaceX = FixedSpacingXForChinese;
            if (fixedSpaceX > 0)
            {
                return fixedSpaceX;
            }

			return mSpaceingX;
		}
		set
		{
			if (mSpaceingX != value)
			{
				mSpaceingX = value;
				hasChanged = true;mSelfChanged = true;
			}
		}
	}

	/// <summary>
	/// Whether the label supports multiple lines.
	/// </summary>

	public bool multiLine
	{
		get
		{
			return mMultiline;
		}
		set
		{
			if (mMultiline != value)
			{
				mMultiline = value;
				hasChanged = true;mSelfChanged = true;
				if (value) mPassword = false;
			}
		}
	}

	/// <summary>
	/// Whether the label's contents should be hidden
	/// </summary>

	public bool password
	{
		get
		{
			return mPassword;
		}
		set
		{
			if (mPassword != value)
			{
				mPassword = value;
				mMultiline = false;
				mEncoding = false;
				hasChanged = true;mSelfChanged = true;
			}
		}
	}

	/// <summary>
	/// Whether the last character of a password field will be shown
	/// </summary>

	public bool showLastPasswordChar
	{
		get
		{
			return mShowLastChar;
		}
		set
		{
			if (mShowLastChar != value)
			{
				mShowLastChar = value;
				hasChanged = true;mSelfChanged = true;
			}
		}
	}

	/// <summary>
	/// What effect is used by the label.
	/// </summary>

	public Effect effectStyle
	{
		get
		{
			return mEffectStyle;
		}
		set
		{
			if (mEffectStyle != value)
			{
				mEffectStyle = value;
				hasChanged = true;mSelfChanged = true;
			}
		}
	}

    public int effectStyleInt
    {
        get
        {
            return (int)mEffectStyle;
        }
        set
        {
            effectStyle = (Effect)value;
        }
    }

	public Color TopColor
	{
		get
		{
			return mTopColor;
		}
		set
		{
			mTopColor = value;
			hasChanged = true;mSelfChanged = true;
		}
	}

	public Color BottomColor
	{
		get
		{
			return mBottomColor;
		}
		set
		{
			mBottomColor = value;
			hasChanged = true;mSelfChanged = true;
		}
	}

	/// <summary>
	/// Color used by the effect, if it's enabled.
	/// </summary>

	public Color effectColor
	{
		get
		{
			return mEffectColor;
		}
		set
		{
			if (mEffectColor != value)
			{
				mEffectColor = value;
				if (mEffectStyle != Effect.None) hasChanged = true;mSelfChanged = true;
			}
		}
	}

	/// <summary>
	/// Returns the processed version of 'text', with new line characters, line wrapping, etc.
	/// </summary>

	public string processedText
	{
        get
        {
            if (mLastFontWidth != cachedWidght.width)
            {
                mLastFontWidth = cachedWidght.width;
                mShouldBeProcessed = true;
            }
            //if (mLastScale != cachedTransform.localScale)
            //{
            //    mLastScale = cachedTransform.localScale;
            //    mShouldBeProcessed = true;
            //}

			// Process the text if necessary
			if (hasChanged)
			{
				ProcessText();
			}
			return mProcessedText;
		}
	}

	public void ProcessText()
	{
		mChanged = true;
		hasChanged = false;
		mLastText = mText;
		mProcessedText = mText.Replace("\\n", "\n");

		if (mPassword)
		{
			mProcessedText = font.WrapText(mProcessedText, 100000f, false, false, FontSize, FontStyle, FontSpacingX);

			string hidden = "";

			if (mShowLastChar)
			{
				for (int i = 1, imax = mProcessedText.Length; i < imax; ++i) hidden += "*";
				if (mProcessedText.Length > 0) hidden += mProcessedText[mProcessedText.Length - 1];
			}
			else
			{
				for (int i = 0, imax = mProcessedText.Length; i < imax; ++i) hidden += "*";
			}
			mProcessedText = hidden;
		}
		else if (mMaxLineWidth > 0)
		{
            //mProcessedText = font.WrapText(mProcessedText, mMaxLineWidth / cachedTransform.localScale.x, mMultiline, mEncoding, FontSize, FontStyle, FontSpacingX);
            mProcessedText = font.WrapText(mProcessedText, Mathf.Abs(mMaxLineWidth / width) > 0 ? mMaxLineWidth / width : 1, mMultiline, mEncoding, FontSize, FontStyle, FontSpacingX);
		}
		else if (!mMultiline)
		{
			mProcessedText = font.WrapText(mProcessedText, 100000f, false, mEncoding, FontSize, FontStyle, FontSpacingX);
		}
	}

	/// <summary>
	/// Visible size of the widget in local coordinates.
	/// </summary>
    float realRelativeSizeX = 0;
	public override Vector2 relativeSize
	{
		get
		{
			Vector3 size = (font != null && !string.IsNullOrEmpty(processedText)) ?
				font.CalculatePrintedSize(mProcessedText, mSpaceingY, mEncoding, FontSize, mFontStyle,FontSpacingX) : Vector2.one;
			float scale = width;
            realRelativeSizeX = Mathf.Max(size.x - FontSpacingX / scale, 0.5f);
            size.x = Mathf.Max(size.x, (font != null && scale > 1f) ? lineWidth / scale : 1f);
			size.y = Mathf.Max(size.y, 1f);
			return size;
		}
	}


	/// <summary>
	/// Legacy functionality support.
	/// </summary>

	protected override void OnStart()
	{
		if (font == null)
		{
			font = UIFont.CreateInstance<UIFont>();
		}

		if (mLineWidth > 0f)
		{
			mMaxLineWidth = Mathf.RoundToInt(mLineWidth);
			mLineWidth = 0f;
		}

		font.RefreshText(mText, FontSize, mFontStyle,TrueTypeFont);
	}

	void OnApplicationPause()
	{
		if (mChanged == false)
		{
			mChanged = true;
			MarkAsChanged();

		}
	}

	/// <summary>
	/// UILabel needs additional processing when something changes.
	/// </summary>

	public override void MarkAsChanged()
	{
		hasChanged = true;mSelfChanged = true;
		base.MarkAsChanged();
//         if (gameObject.activeSelf) {
//             gameObject.SetActive(false);
//             gameObject.SetActive(true);
//         }
	}

	/// <summary>
	/// Same as MakePixelPerfect(), but only adjusts the position, not the scale.
	/// </summary>

	public void MakePositionPerfect()
	{
		Vector3 scale = cachedTransform.localScale;

		if (FontSize == Mathf.RoundToInt(scale.x) && FontSize == Mathf.RoundToInt(scale.y) &&
			cachedTransform.localRotation == Quaternion.identity)
		{
			Vector2 actualSize = relativeSize * scale.x;

			int x = Mathf.RoundToInt(actualSize.x);
			int y = Mathf.RoundToInt(actualSize.y);

			Vector3 pos = cachedTransform.localPosition;
			pos.x = Mathf.FloorToInt(pos.x);
			pos.y = Mathf.CeilToInt(pos.y);
			pos.z = Mathf.RoundToInt(pos.z);

			if ((x % 2 == 1) && (pivot == Pivot.Top || pivot == Pivot.Center || pivot == Pivot.Bottom)) pos.x += 0.5f;
			if ((y % 2 == 1) && (pivot == Pivot.Left || pivot == Pivot.Center || pivot == Pivot.Right)) pos.y -= 0.5f;

			if (cachedTransform.localPosition != pos) cachedTransform.localPosition = pos;
		}
	}

	/// <summary>
	/// Text is pixel-perfect when its scale matches the size.
	/// </summary>

	public override void MakePixelPerfect()
	{
		if (font != null)
		{
            //Vector3 scale = cachedTransform.localScale;
            Vector3 scale = new Vector3(width, height);
            scale.x = FontSize;
            scale.y = scale.x;
            scale.z = 1f;

			Vector2 actualSize = relativeSize * scale.x;

			int x = Mathf.RoundToInt(actualSize.x);
			int y = Mathf.RoundToInt(actualSize.y);

			Vector3 pos = cachedTransform.localPosition;
			pos.x = Mathf.FloorToInt(pos.x);
			pos.y = Mathf.CeilToInt(pos.y);
			pos.z = Mathf.RoundToInt(pos.z);

			if (cachedTransform.localRotation == Quaternion.identity)
			{
				if ((x % 2 == 1) && (pivot == Pivot.Top || pivot == Pivot.Center || pivot == Pivot.Bottom)) pos.x += 0.5f;
				if ((y % 2 == 1) && (pivot == Pivot.Left || pivot == Pivot.Center || pivot == Pivot.Right)) pos.y -= 0.5f;
			}
			cachedTransform.localPosition = pos;
            //cachedTransform.localScale = scale;
		}
		else base.MakePixelPerfect();
	}
    
	/// <summary>
	/// Apply a shadow effect to the buffer.
	/// </summary>

	protected void ApplyShadow(BetterList<Vector3> verts, BetterList<Vector2> uvs, BetterList<Color> cols, int start, int end, float x, float y)
	{
		Color c = mEffectColor;
		c.a *= color.a;

		for (int i = start; i < end; ++i)
		{
			verts.Add(verts.buffer[i]);
			uvs.Add(uvs.buffer[i]);
			cols.Add(cols.buffer[i]);

			verts.buffer[i].x += x * width;
			verts.buffer[i].y += y * height;
			cols.buffer[i] = c;
		}
	}

	/// <summary>
	/// Draw the label.
	/// </summary>
	public override void OnFill(BetterList<Vector3> verts, BetterList<Vector2> uvs, BetterList<Color> cols)
	{
		if (font == null) return;
		//MakePositionPerfect();
		Pivot p = pivot;
		int offset = verts.size;

        float leftTextLineOffset = 0;
        if (p == Pivot.Left || p == Pivot.TopLeft || p == Pivot.BottomLeft)
        {
            font.Print(processedText, color, mTopColor, mBottomColor, verts, uvs, cols, mEncoding, UIFont.Alignment.Left, 0, mSpaceingY, FontSize, mFontStyle, FontSpacingX, TrueTypeFont, width, height);
            leftTextLineOffset = pivotOffset.x * (width) * realRelativeSizeX;
        }
        else if (p == Pivot.Right || p == Pivot.TopRight || p == Pivot.BottomRight)
        {
            font.Print(processedText, color, mTopColor, mBottomColor, verts, uvs, cols, mEncoding, UIFont.Alignment.Right,
                Mathf.RoundToInt(relativeSize.x * width), mSpaceingY, FontSize, mFontStyle, FontSpacingX, TrueTypeFont, width, height);
            if (lineWidth > 0)
                leftTextLineOffset = -lineWidth + FontSpacingX;
            else
                leftTextLineOffset = pivotOffset.x * (width) * realRelativeSizeX;
           // leftTextLineOffset -= lineWidth * 0.5f - Mathf.Abs(leftTextLineOffset);
        }
        else
        {
            font.Print(processedText, color, mTopColor, mBottomColor, verts, uvs, cols, mEncoding, UIFont.Alignment.Center,
                Mathf.RoundToInt(relativeSize.x * width), mSpaceingY, FontSize, mFontStyle, FontSpacingX, TrueTypeFont, width, height);
            leftTextLineOffset = pivotOffset.x * (width) * realRelativeSizeX;
            if(lineWidth>0)
                leftTextLineOffset -= lineWidth * 0.5f - Mathf.Abs(leftTextLineOffset) - FontSpacingX * 0.5f;
        }

        //apply pivot offset to verts
        
        Vector3 pOffset = new Vector3(leftTextLineOffset, pivotOffset.y * (height + 1) * relativeSize.y, 0);
        for (int i = 0; i < verts.size; i++)
        {
            verts.buffer[i] += pOffset;
        }

		// Apply an effect if one was requested
		if (effectStyle != Effect.None)
		{
			Vector3 scale = cachedTransform.localScale;
			if (scale.x == 0f || scale.y == 0f) return;

			int end = verts.size;
			float pixel = 1f / FontSize;

			ApplyShadow(verts, uvs, cols, offset, end, pixel, -pixel);

			if (effectStyle == Effect.Outline)
			{
				offset = end;
				end = verts.size;

				ApplyShadow(verts, uvs, cols, offset, end, -pixel, pixel);

				offset = end;
				end = verts.size;

				ApplyShadow(verts, uvs, cols, offset, end, pixel, pixel);

				offset = end;
				end = verts.size;

				ApplyShadow(verts, uvs, cols, offset, end, -pixel, -pixel);
			}
            else if (effectStyle == Effect.Shadow2)
            {
                offset = end;
                end = verts.size;
                ApplyShadow(verts, uvs, cols, offset, end, pixel * 2, -pixel);

                offset = end;
                end = verts.size;
                ApplyShadow(verts, uvs, cols, offset, end, pixel * 2, -pixel);

                offset = end;
                end = verts.size;
                ApplyShadow(verts, uvs, cols, offset, end, pixel * 2, -pixel * 2);

                offset = end;
                end = verts.size;
                ApplyShadow(verts, uvs, cols, offset, end, pixel * 2, -pixel * 2);

            }
		}
	}
}
