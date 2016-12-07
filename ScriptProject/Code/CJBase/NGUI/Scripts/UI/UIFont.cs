//----------------------------------------------
//            NGUI: Next-Gen UI kit
// Copyright © 2011-2012 Tasharen Entertainment
//----------------------------------------------

using UnityEngine;
using System.Collections.Generic;
using System.Text;

/// <summary>
/// UIFont contains everything needed to be able to print text.
/// </summary>
/// 
public class UIFont
{
	static UIFont mSelf;
	public static UIFont CreateInstance<T>()
	{
		if (mSelf == null)
		{
			mSelf = new UIFont();
		}
		return mSelf;
	}

	public UIFont()
	{
        Font.textureRebuilt += RefreshAllLables;
	}

	static Font _DefaultFont;
	public static Font DefaultFont
	{
		get
		{
			if (_DefaultFont == null)
			{
                Object fontObject = ResLoader.Load("UI/Fonts/msyh");
				if (fontObject != null)
				{
					//Debug.Log("msyh Not Null");
					_DefaultFont = fontObject as Font;
				}
				else
				{
					Object[] fonts = Resources.FindObjectsOfTypeAll(typeof(Font)) as Object[];
#if UNITY_EDITOR
					for (int i = 0; i < fonts.Length; i++ )
					{
						if (fonts[i].name.Contains("Arial"))
						{
							_DefaultFont = fonts[i] as Font;
							break;
						}
					}
					
#else
					if (fonts != null && fonts.Length > 0)
					{
						_DefaultFont = fonts[0] as Font;
					}
#endif
				}
			}
			return _DefaultFont;
		}
	}

	static Font _ArtFont;
	public static Font ArtFont
	{
		get
		{
			if (_ArtFont == null)
			{
				Object fontObject = ResLoader.Load("UI/Fonts/HZGB");
				_ArtFont = fontObject as Font;
			}
			return _ArtFont;
		}
	}

	public enum Alignment
	{
		Left,
		Center,
		Right,
	}

	[SerializeField]
	Rect mUVRect = new Rect(0f, 0f, 1f, 1f);
	[SerializeField]
	Font mTrueTypeFont;

	public Font TrueTypeFont
	{
		get
		{
			if (mTrueTypeFont == null)
			{
				mTrueTypeFont = DefaultFont;
			}
			return mTrueTypeFont;
		}
		set
		{
			mTrueTypeFont = value;
		}
	}


	/// <summary>
	/// Offset and scale applied to all UV coordinates.
	/// </summary>

	public Rect uvRect
	{
		get
		{
			return mUVRect;
		}
		set
		{
			;
		}
	}

	/// <summary>
	/// Sprite used by the font, if any.
	/// </summary>

	public string spriteName
	{
		get
		{
			return "";
		}
		set
		{
		}
	}

	void RefreshAllLables(Font font)
	{
		UIPanel[] panels = UnityEngine.GameObject.FindObjectsOfType<UIPanel>() as UIPanel[];
        for (int i = 0; i < panels.Length; i++)
		{
            foreach (UIWidget widget in panels[i].widgets) { 
                if(widget.GetType() == typeof(UILabel) || widget.GetType() == typeof(UIColorLabel) ){
                    UILabel lbl = widget as UILabel;
                    if (lbl.TrueTypeFont == font)
                    {
                        lbl.MarkAsChanged();
                        lbl.ProcessText();
                        lbl.UpdateGeometry(ref panels[i].mWorldToLocal, (lbl.changeFlag == 1));
                    }
                }
            }
            panels[i].Fill();
		}
	}

	/// <summary>
	/// Helper function that determines whether the font uses the specified one, taking replacements into account.
	/// </summary>

	bool References(UIFont font)
	{
		if (font == null) return false;
		if (font == this) return true;
		return false;
	}

	/// <summary>
	/// Helper function that determines whether the two atlases are related.
	/// </summary>

	static public bool CheckIfRelated(UIFont a, UIFont b)
	{
		if (a == null || b == null) return false;
		return a == b || a.References(b) || b.References(a);
	}

	/// <summary>
	/// Refresh all labels that use this font.
	/// </summary>

	public void MarkAsDirty()
	{
		UILabel[] labels = (UILabel[])Object.FindObjectsOfType(typeof(UILabel));

		//foreach (UILabel lbl in labels)
		for (int i = 0; i < labels.Length; i++)
		{
			UILabel lbl = labels[i];
			if (lbl.enabled && lbl.gameObject.activeSelf && CheckIfRelated(this, lbl.font))
			{
				lbl.font = null;
				lbl.font = this;
			}
		}
	}

	/// <summary>
	/// Get the printed size of the specified string. The returned value is in local coordinates. Multiply by transform's scale to get pixels.
	/// </summary>

	public Vector2 CalculatePrintedSize(string text, int SpacingY, bool encoding)
	{
		return CalculatePrintedSize(text, SpacingY, encoding, 24, FontStyle.Normal, 0);
	}

	public Vector2 CalculatePrintedSize(string text, int SpacingY, bool encoding, int fontSize)
	{
		return CalculatePrintedSize(text, SpacingY, encoding, fontSize, FontStyle.Normal, 0);
	}


	public Vector2 CalculatePrintedSize(string text, int SpacingY, bool encoding, int fontSize, FontStyle fontStyle, int SpacingX, Font trueFont = null)
{
		if (trueFont == null)
		{
			trueFont = TrueTypeFont;
		}
		
		Vector2 v = Vector2.zero;
		RefreshText(text, fontSize, fontStyle, trueFont);
		int lineHeight = (fontSize  + SpacingY);
		if (trueFont != null && !string.IsNullOrEmpty(text))
		{
			if (encoding) text = NGUITools.StripSymbols(text);

			int maxX = 0;
			int x = 0;
			int y = 0;
			CharacterInfo tempCharInfo;

			for (int i = 0, imax = text.Length; i < imax; ++i)
			{
				char c = text[i];

				// Start a new line
				if (c == '\n')
				{
					if (x > maxX) maxX = x;
					x = 0;
					y += (lineHeight);
					continue;
				}

				// Skip invalid characters
				if (c < ' ') { continue; }

                if (c == '\\')
                {
                    if (i + 1 < imax)
                    {
                        if (text[i + 1] == '<' )
                        {
                            i++;
                            c = '[';

                        }
                        if (i + 1 < imax)
                        {
                            if (text[i + 1] == '>')
                            {
                                i++;
                                c = ']';
                            }
                        }

                    }
                }

				if (!trueFont.GetCharacterInfo(c, out tempCharInfo, fontSize, fontStyle))
				{
					continue;
				}

				x += SpacingX + (int)tempCharInfo.width;
			}

			// Convert from pixel coordinates to local coordinates
			float scale = (fontSize > 0) ? 1f / fontSize : 1f;

			v.x = scale * ((x > maxX) ? x : maxX);
			v.y = scale * (y + lineHeight);
		}
		return v;
	}

	/// <summary>
	/// Convenience function that ends the line by either appending a new line character or replacing a space with one.
	/// </summary>

	static void EndLine(ref StringBuilder s)
	{
		int i = s.Length - 1;
		if (i > 0 && s[i] == ' ') s[i] = '\n';
		else s.Append('\n');
	}

	/// <summary>
	/// Text wrapping functionality. The 'maxWidth' should be in local coordinates (take pixels and divide them by transform's scale).
	/// </summary>
	
	public string WrapText(string text, float maxWidth, bool multiline, bool encoding)
	{
		return WrapText(text, maxWidth, multiline, encoding, 24, FontStyle.Normal, 0);
	}

	public string WrapText(string text, float maxWidth, bool multiline, bool encoding, int fontSize)
	{
		return WrapText(text, maxWidth, multiline, encoding, fontSize, FontStyle.Normal, 0);
	}

	public string WrapText(string text, float maxWidth, bool multiline, bool encoding, int fontSize, FontStyle fontStyle, int SpaceingX, Font trueFont = null)
	{
		if (trueFont == null)
		{
			trueFont = TrueTypeFont;
		}
		
		string result;
		trueFont.RequestCharactersInTexture(text, fontSize, fontStyle);
		if (string.IsNullOrEmpty(text))
		{
			NGUIText.WrapText(text, trueFont, fontSize, fontStyle, (int)maxWidth, 460, 3, encoding, out result);
			return result;
		}

		// Width of the line in pixels
		int lineWidth = Mathf.RoundToInt(maxWidth * fontSize);
		if (lineWidth < 1) return text;

		StringBuilder sb = new StringBuilder();
		int textLength = text.Length;
		int remainingWidth = lineWidth;
		int start = 0;
		int offset = 0;
		bool lineIsEmpty = true;


		CharacterInfo tempCharInfo;
		// Run through all characters
		for (; offset < textLength; ++offset)
		{
			char ch = text[offset];

			// New line character -- start a new line
			if (ch == '\n')
			{
				if (!multiline) break;
				remainingWidth = lineWidth;

				// Add the previous word to the final string
				if (start < offset) sb.Append(text.Substring(start, offset - start + 1));
				else sb.Append(ch);

				lineIsEmpty = true;
				start = offset + 1;
				continue;
			}

			// When encoded symbols such as [RrGgBb] or [-] are encountered, skip past them
			if (encoding && ch == '[')
			{
				if (offset + 2 < textLength)
				{
					if (text[offset + 1] == '-' && text[offset + 2] == ']')
					{
						offset += 2;
						continue;
					}
					else if (offset + 7 < textLength && text[offset + 7] == ']')
					{
						offset += 7;
						continue;
					}
				}
			}

            if (encoding && ch == '\\')
            {
                if (offset + 1 < textLength)
                {

                    if (text[offset + 1] == '<')
                    {
                       // sb.Append( '\\');
                       // sb.Append('<');
                        ch = '[';
                        offset += 1;
                    }
                    else if (text[offset + 1] == '>')
                    {
                       // sb.Append('\\');
                       // sb.Append('>');
                        ch = ']';
                        offset += 1;
                    }
                }
            }

			if (!trueFont.GetCharacterInfo(ch, out tempCharInfo, fontSize, fontStyle))
			{
				continue;
			}


			int charSize = SpaceingX + (int)tempCharInfo.width;

			remainingWidth -= charSize;

			// Doesn't fit?
			if (remainingWidth < 0)
			{
				if (lineIsEmpty || !multiline)
				{
					// This is the first word on the line -- add it up to the character that fits
					sb.Append(text.Substring(start, Mathf.Max(0, offset - start)));

					if (!multiline)
					{
						start = offset;
						break;
					}
					EndLine(ref sb);

					// Start a brand-new line
					lineIsEmpty = true;

					if (ch == ' ')
					{
						start = offset + 1;
						remainingWidth = lineWidth;
					}
					else
					{
						start = offset;
						remainingWidth = lineWidth - charSize;
					}
				}
				else
				{
					// Skip all spaces before the word
					while (start < textLength && text[start] == ' ') ++start;

					// Revert the position to the beginning of the word and reset the line
					lineIsEmpty = true;
					remainingWidth = lineWidth;
					offset = start - 1;
					if (!multiline) break;
					EndLine(ref sb);
					continue;
				}
			}
		}

		if (start < offset) sb.Append(text.Substring(start, offset - start));
		return sb.ToString();
	}


	public TextAlignment GetTextAlign(Alignment align)
	{
		if (align == Alignment.Center)
		{
			return TextAlignment.Center;
		}
		else if (align == Alignment.Left)
		{
			return TextAlignment.Left;
		}
		return TextAlignment.Right;
	}

	/// <summary>
	/// Print the specified text into the buffers.
	/// Note: 'lineWidth' parameter should be in pixels.
	/// </summary>

	public void Print(string text, Color color, Color topColor, Color bottomColor, BetterList<Vector3> verts, BetterList<Vector2> uvs, BetterList<Color> cols,
		bool encoding, Alignment alignment, int lineWidth, int SpacingY, int fontSize, FontStyle fontStyle, int SpacingX, Font trueFont, int width, int height)
	{
		Vector2 scale = fontSize > 0 ? new Vector2(1f / fontSize, 1f / fontSize) : Vector2.one;
        scale.x *= width;
        scale.y *= height;
		if (!string.IsNullOrEmpty(text))
		{
			NGUIText.Print(text, trueFont, fontSize, fontStyle, color, encoding, GetTextAlign(alignment), lineWidth, false, verts, uvs, cols, scale, topColor, bottomColor, SpacingY, SpacingX, width, height);
			return;
		}
	}

    public void Print(string text, Color color, Color topColor, Color middleColor, Color bottomColor, BetterList<Vector3> verts, BetterList<Vector2> uvs, BetterList<Color> cols,
        bool encoding, Alignment alignment, int lineWidth, int SpacingY, int fontSize, FontStyle fontStyle, int SpacingX, Font trueFont, int width, int height)
    {
        Vector2 scale = fontSize > 0 ? new Vector2(1f / fontSize, 1f / fontSize) : Vector2.one;
        scale.x *= width;
        scale.y *= width;
        if (!string.IsNullOrEmpty(text))
        {
            NGUIText.Print(text, trueFont, fontSize, fontStyle, color, encoding, GetTextAlign(alignment), lineWidth, false, verts, uvs, cols, scale, topColor,middleColor, bottomColor, SpacingY, SpacingX, width, height);
            return;
        }
    }

	public void RefreshText(string txt, int fontSize, FontStyle fontStyle, Font trueFont = null)
	{
		if (trueFont != null)
		{
			trueFont.RequestCharactersInTexture(txt, fontSize, fontStyle);
		}
		else
		{
			TrueTypeFont.RequestCharactersInTexture(txt, fontSize, fontStyle);
		}
	
	}
}
