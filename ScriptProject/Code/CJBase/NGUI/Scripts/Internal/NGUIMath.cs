//----------------------------------------------
//            NGUI: Next-Gen UI kit
// Copyright ?2011-2012 Tasharen Entertainment
//----------------------------------------------

using UnityEngine;
using System;
using System.Collections.Generic;

/// <summary>
/// Helper class containing generic functions used throughout the UI library.
/// </summary>

static public class NGUIMath
{
	public static readonly float AngleToRad = 0.0174532924f;
	/// <summary>
	/// Ensure that the angle is within -180 to 180 range.
	/// </summary>

	static public float WrapAngle (float angle)
	{
		while (angle > 180f) angle -= 360f;
		while (angle < -180f) angle += 360f;
		return angle;
	}

	/// <summary>
	/// Convert a hexadecimal character to its decimal value.
	/// </summary>

	static public int HexToDecimal (char ch)
	{
		switch (ch)
		{
			case '0': return 0x0;
			case '1': return 0x1;
			case '2': return 0x2;
			case '3': return 0x3;
			case '4': return 0x4;
			case '5': return 0x5;
			case '6': return 0x6;
			case '7': return 0x7;
			case '8': return 0x8;
			case '9': return 0x9;
			case 'a':
			case 'A': return 0xA;
			case 'b':
			case 'B': return 0xB;
			case 'c':
			case 'C': return 0xC;
			case 'd':
			case 'D': return 0xD;
			case 'e':
			case 'E': return 0xE;
			case 'f':
			case 'F': return 0xF;
		}
		return 0xF;
	}

	static public string DecimalToHex(int num)
	{
		num &= 0xFFFFFF;
#if UNITY_FLASH
		StringBuilder sb = new StringBuilder();
		sb.Append(DecimalToHexChar((num >> 20) & 0xF));
		sb.Append(DecimalToHexChar((num >> 16) & 0xF));
		sb.Append(DecimalToHexChar((num >> 12) & 0xF));
		sb.Append(DecimalToHexChar((num >> 8) & 0xF));
		sb.Append(DecimalToHexChar((num >> 4) & 0xF));
		sb.Append(DecimalToHexChar(num & 0xF));
		return sb.ToString();
#else
		return num.ToString("X6");
#endif
	}

	/// <summary>
	/// Convert the specified color to RGBA32 integer format.
	/// </summary>


	static public int ColorToInt (Color c)
	{
		int retVal = 0;
		retVal |= Mathf.RoundToInt(c.r * 255f) << 24;
		retVal |= Mathf.RoundToInt(c.g * 255f) << 16;
		retVal |= Mathf.RoundToInt(c.b * 255f) << 8;
		retVal |= Mathf.RoundToInt(c.a * 255f);
		return retVal;
	}

	/// <summary>
	/// Convert the specified RGBA32 integer to Color.
	/// </summary>

	static public Color IntToColor (int val)
	{
		float inv = 1f / 255f;
		Color c = Color.black;
		c.r = inv * ((val >> 24) & 0xFF);
		c.g = inv * ((val >> 16) & 0xFF);
		c.b = inv * ((val >> 8) & 0xFF);
		c.a = inv * (val & 0xFF);
		return c;
	}

	/// <summary>
	/// Convert the specified integer to a human-readable string representing the binary value. Useful for debugging bytes.
	/// </summary>

	static public string IntToBinary (int val, int bits)
	{
		string final = "";

		for (int i = bits; i > 0; )
		{
			if (i == 8 || i == 16 || i == 24) final += " ";
			final += ((val & (1 << --i)) != 0) ? '1' : '0';
		}
		return final;
	}

	/// <summary>
	/// Convenience conversion function, allowing hex format (0xRrGgBbAa).
	/// </summary>

	static public Color HexToColor (uint val)
	{
		return IntToColor((int)val);
	}

    static public Color StringToColor(string colorStr)
    {
        uint colorValue = uint.Parse(FillingColorString(colorStr), System.Globalization.NumberStyles.HexNumber);

        return HexToColor(colorValue);
    }

    static private string FillingColorString(string colorStr)
    {
        if (string.IsNullOrEmpty(colorStr))
            return "FFFFFFFF";

        colorStr = colorStr.Replace("#", string.Empty);
        int len = colorStr.Length;
        if (len == 8)
            return colorStr;
        if (len == 6)
            return colorStr + "FF";
        if (len == 3)
            return string.Format("{0}{1}{2}{3}{4}{5}FF", colorStr[0], colorStr[0], colorStr[1], colorStr[1], colorStr[2], colorStr[2]);
        if (len == 4)
            return string.Format("{0}{1}{2}{3}{4}{5}{6}{7}", colorStr[0], colorStr[0], colorStr[1], colorStr[1], colorStr[2], colorStr[2], colorStr[3], colorStr[3]);

        return "FFFFFFFF";
    }

	/// <summary>
	/// Convert from top-left based pixel coordinates to bottom-left based UV coordinates.
	/// </summary>

	static public Rect ConvertToTexCoords (Rect rect, int width, int height)
	{
		Rect final = rect;

		if (width != 0f && height != 0f)
		{
			final.xMin = rect.xMin / width;
			final.xMax = rect.xMax / width;
			final.yMin = 1f - rect.yMax / height;
			final.yMax = 1f - rect.yMin / height;
		}
		return final;
	}

	/// <summary>
	/// Convert from bottom-left based UV coordinates to top-left based pixel coordinates.
	/// </summary>

	static public Rect ConvertToPixels (Rect rect, int width, int height, bool round)
	{
		Rect final = rect;

		if (round)
		{
			final.xMin = Mathf.RoundToInt(rect.xMin * width);
			final.xMax = Mathf.RoundToInt(rect.xMax * width);
			final.yMin = Mathf.RoundToInt((1f - rect.yMax) * height);
			final.yMax = Mathf.RoundToInt((1f - rect.yMin) * height);
		}
		else
		{
			final.xMin = rect.xMin * width;
			final.xMax = rect.xMax * width;
			final.yMin = (1f - rect.yMax) * height;
			final.yMax = (1f - rect.yMin) * height;
		}
		return final;
	}

	/// <summary>
	/// Round the pixel rectangle's dimensions.
	/// </summary>

	static public Rect MakePixelPerfect (Rect rect)
	{
		rect.xMin = Mathf.RoundToInt(rect.xMin);
		rect.yMin = Mathf.RoundToInt(rect.yMin);
		rect.xMax = Mathf.RoundToInt(rect.xMax);
		rect.yMax = Mathf.RoundToInt(rect.yMax);
		return rect;
	}

	/// <summary>
	/// Round the texture coordinate rectangle's dimensions.
	/// </summary>

	static public Rect MakePixelPerfect (Rect rect, int width, int height)
	{
		rect = ConvertToPixels(rect, width, height, true);
		rect.xMin = Mathf.RoundToInt(rect.xMin);
		rect.yMin = Mathf.RoundToInt(rect.yMin);
		rect.xMax = Mathf.RoundToInt(rect.xMax);
		rect.yMax = Mathf.RoundToInt(rect.yMax);
		return ConvertToTexCoords(rect, width, height);
	}

	/// <summary>
	/// The much-dreaded half-pixel offset of DirectX9:
	/// http://drilian.com/2008/11/25/understanding-half-pixel-and-half-texel-offsets/
	/// </summary>

	static public Vector3 ApplyHalfPixelOffset (Vector3 pos)
	{
		RuntimePlatform platform = Application.platform;

		if (platform == RuntimePlatform.WindowsPlayer ||
			platform == RuntimePlatform.WindowsWebPlayer ||
			platform == RuntimePlatform.WindowsEditor)
		{
			pos.x = pos.x - 0.5f;
			pos.y = pos.y + 0.5f;
		}
		return pos;
	}

	/// <summary>
	/// Per-pixel offset taking scale into consideration.
	/// If the scale dimension is an odd number, it won't apply the offset.
	/// This is useful for centered sprites.
	/// </summary>

	static public Vector3 ApplyHalfPixelOffset (Vector3 pos, Vector3 scale)
	{
		RuntimePlatform platform = Application.platform;

		if (platform == RuntimePlatform.WindowsPlayer ||
			platform == RuntimePlatform.WindowsWebPlayer ||
			platform == RuntimePlatform.WindowsEditor)
		{
			if (Mathf.RoundToInt(scale.x) == (Mathf.RoundToInt(scale.x * 0.5f) * 2)) pos.x = pos.x - 0.5f;
			if (Mathf.RoundToInt(scale.y) == (Mathf.RoundToInt(scale.y * 0.5f) * 2)) pos.y = pos.y + 0.5f;
		}
		return pos;
	}

	/// <summary>
	/// Constrain 'rect' to be within 'area' as much as possible, returning the Vector2 offset necessary for this to happen.
	/// This function is useful when trying to restrict one area (window) to always be within another (viewport).
	/// </summary>

	static public Vector2 ConstrainRect (Vector2 minRect, Vector2 maxRect, Vector2 minArea, Vector2 maxArea)
	{
		Vector2 offset = Vector2.zero;

		float contentX = maxRect.x - minRect.x;
		float contentY = maxRect.y - minRect.y;

		float areaX = maxArea.x - minArea.x;
		float areaY = maxArea.y - minArea.y;

		if (contentX > areaX)
		{
			float diff = contentX - areaX;
			minArea.x -= diff;
			maxArea.x += diff;
		}

		if (contentY > areaY)
		{
			float diff = contentY - areaY;
			minArea.y -= diff;
			maxArea.y += diff;
		}

		if (minRect.x < minArea.x) offset.x += minArea.x - minRect.x;
		if (maxRect.x > maxArea.x) offset.x -= maxRect.x - maxArea.x;
		if (minRect.y < minArea.y) offset.y += minArea.y - minRect.y;
		if (maxRect.y > maxArea.y) offset.y -= maxRect.y - maxArea.y;

		return offset;
	}

	/// <summary>
	/// Calculate the combined bounds of all widgets attached to the specified game object or its children (in world space).
	/// </summary>

	static public Bounds CalculateAbsoluteWidgetBounds (Transform trans)
	{
		UIWidget[] widgets = trans.GetComponentsInChildren<UIWidget>() as UIWidget[];

		Bounds b = new Bounds(trans.transform.position, Vector3.zero);
		bool first = true;

		foreach (UIWidget w in widgets)
		{
			if (w is UIParticle)
			{
				continue;
			}
            Vector2 offset = w.pivotOffset;
            Vector2 size = Vector2.zero;
            if (w is UILabel)
            {
                UILabel label = w as UILabel;
                size = w.relativeSize * label.FontSize;
            }
            else
            {
                size = w.Dimensions;
            }

            float x0 = offset.x * size.x;
            float y0 = (offset.y - 1) * size.y;
            float x1 = x0 + size.x;
            float y1 = offset.y * size.y;

            Transform wt = w.cachedTransform;

            if(first)
            {
                first = false;
                b = new Bounds(wt.TransformPoint(new Vector3(x0,y0, 0f)), Vector3.zero);
            }else
            {
                b.Encapsulate(wt.TransformPoint(new Vector3(x0, y0, 0f)));
            }
            b.Encapsulate(wt.TransformPoint(new Vector3(x0, y1, 0f)));
            b.Encapsulate(wt.TransformPoint(new Vector3(x1, y0, 0f)));
            b.Encapsulate(wt.TransformPoint(new Vector3(x1, y1, 0f)));


            /*
            Vector3 backUpScale = w.cachedTransform.localScale;
            Vector2 size = new Vector2(w.relativeSize.x , w.relativeSize.y );
			Vector2 offset = w.pivotOffset;
			float x = (offset.x - 0.5f) * size.x;
			float y = (offset.y - 0.5f) * size.y;
			size *= 0.5f;

			Transform wt = w.cachedTransform;
            wt.localScale = new Vector3(wt.localScale.x * w.Dimensions.x, wt.localScale.y * w.Dimensions.y, wt.localScale.z);
			Vector3 v0 = wt.TransformPoint(new Vector3(x - size.x, y - size.y, 0f));

			// 'Bounds' can never start off with nothing, apparently, and including the origin point is wrong.
			if (first)
			{
				first = false;
				b = new Bounds(v0, Vector3.zero);
			}
			else
			{
				b.Encapsulate(v0);
			}

			b.Encapsulate(wt.TransformPoint(new Vector3(x - size.x, y + size.y, 0f)));
			b.Encapsulate(wt.TransformPoint(new Vector3(x + size.x, y - size.y, 0f)));
			b.Encapsulate(wt.TransformPoint(new Vector3(x + size.x, y + size.y, 0f)));

            wt.localScale = backUpScale;*/
		}
		return b;
	}

	/// <summary>
	/// Calculate the combined bounds of all widgets attached to the specified game object or its children (in relative-to-object space).
	/// </summary>

	static public Bounds CalculateRelativeWidgetBounds (Transform root, Transform child,bool includeInActive = false,bool includeTexture = true)
	{
        UIWidget[] widgets = child.GetComponentsInChildren<UIWidget>(includeInActive) as UIWidget[];

		Matrix4x4 toLocal = root.worldToLocalMatrix;
		Bounds b = new Bounds(Vector3.zero, Vector3.zero);
		bool first = true;

		foreach (UIWidget w in widgets)
		{
			if (w is UIParticle)
			{
				continue;
			}

            if (!includeTexture && w is UITexture)
            {
                continue;
            }
            Vector3 backUpScale = w.cachedTransform.localScale;
            Vector2 size = new Vector2(w.relativeSize.x, w.relativeSize.y); //w.relativeSize;
			Vector2 offset = w.pivotOffset;
			float x = (offset.x + 0.5f) * size.x;
			float y = (offset.y - 0.5f) * size.y;
			size *= 0.5f;

			// Transform the coordinates from relative-to-widget to world space, then make them relative to game object
			Transform toWorld = w.cachedTransform;
            toWorld.localScale = new Vector3(toWorld.localScale.x * w.Dimensions.x, toWorld.localScale.y * w.Dimensions.y, toWorld.localScale.z);
			Vector3 v0 = toLocal.MultiplyPoint3x4(toWorld.TransformPoint(new Vector3(x - size.x, y - size.y, 0f)));

			if (first)
			{
				first = false;
				b = new Bounds(v0, Vector3.zero);
			}
			else
			{
				b.Encapsulate(v0);
			}

			b.Encapsulate(toLocal.MultiplyPoint3x4(toWorld.TransformPoint(new Vector3(x - size.x, y + size.y, 0f))));
			b.Encapsulate(toLocal.MultiplyPoint3x4(toWorld.TransformPoint(new Vector3(x + size.x, y - size.y, 0f))));
			b.Encapsulate(toLocal.MultiplyPoint3x4(toWorld.TransformPoint(new Vector3(x + size.x, y + size.y, 0f))));

            toWorld.localScale = backUpScale;
		}
		return b;
	}

	static public Vector4 OccurClipRange(Vector4 v0, Vector4 v1)
	{
		float minx0 = v0.x - v0.w / 2;
		float maxx0 = v0.x + v0.w / 2;
		float miny0 = v0.y - v0.z / 2;
		float maxy0 = v0.y + v0.z / 2;

		float minx1 = v1.x - v1.w / 2;
		float maxx1 = v1.x + v1.w / 2;
		float miny1 = v1.y - v1.z / 2;
		float maxy1 = v1.y + v1.z / 2;

		if (minx0 > maxx1 ||
			minx1 > maxx0 ||
			miny0 > maxy1 ||
			miny1 > maxy0)
			return Vector4.zero;

		float[] points = new float[4];
		points[0] = minx0;
		points[1] = maxx0;
		points[2] = minx1;
		points[3] = maxx1;
		Array.Sort(points);

		float outx0 = points[1];
		float outx1 = points[2];

		points[0] = miny0;
		points[1] = maxy0;
		points[2] = miny1;
		points[3] = maxy1;
		Array.Sort(points);

		float outy0 = points[1];
		float outy1 = points[2];

		Vector4 vecout;
		vecout.w = outx1 - outx0;
		vecout.z = outy1 - outy0;

		vecout.x = outx0 + vecout.w / 2;
		vecout.y = outy0 + vecout.z / 2;

		return vecout;
	}

	static public Vector3[] ClipRectangle(Vector3 vmin0, Vector3 vmax0, Vector3 vmin1, Vector3 vmax1)
	{
		float minx0 = vmin0.x;
		float maxx0 = vmax0.x;
		float miny0 = vmin0.y;
		float maxy0 = vmax0.y;

		float minx1 = vmin1.x;
		float maxx1 = vmax1.x;
		float miny1 = vmin1.y;
		float maxy1 = vmax1.y;

		if (minx0 > maxx1 ||
			minx1 > maxx0 ||
			miny0 > maxy1 ||
			miny1 > maxy0)
			return null;

		Vector3[] vecout = new Vector3[2];

		float[] points = new float[4];
		points[0] = minx0;
		points[1] = maxx0;
		points[2] = minx1;
		points[3] = maxx1;
		Array.Sort(points);

		vecout[0].x = points[1];
		vecout[1].x = points[2];

		points[0] = miny0;
		points[1] = maxy0;
		points[2] = miny1;
		points[3] = maxy1;
		Array.Sort(points);

		vecout[0].y = points[1];
		vecout[1].y = points[2];

		return vecout;
	}

	/// <summary>
	/// Calculate the combined bounds of all widgets attached to the specified game object or its children (in relative-to-object space).
	/// </summary>

	static public Bounds CalculateRelativeWidgetBounds (Transform trans)
	{
		return CalculateRelativeWidgetBounds(trans, trans);
	}

    static public Bounds GetBoundsIngnoreTexture(Transform trans, bool includeActive = false)
    {
        return CalculateRelativeWidgetBounds(trans, trans, includeActive, false);
    }

    static public Bounds GetBoundsIngnoreTexture(Transform rootTrans,Transform trans,bool includeActive = false)
    {
        return CalculateRelativeWidgetBounds(rootTrans, trans, includeActive, false);
    }

	/// <summary>
	/// This code is not framerate-independent:
	///
	/// target.position += velocity;
	/// velocity = Vector3.Lerp(velocity, Vector3.zero, Time.deltaTime * 9f);
	///
	/// But this code is:
	///
	/// target.position += NGUIMath.SpringDampen(ref velocity, 9f, Time.deltaTime);
	/// </summary>

	static public Vector3 SpringDampen (ref Vector3 velocity, float strength, float deltaTime)
	{
		// Dampening factor applied each millisecond
		float dampeningFactor = 1f - strength * 0.001f;
		int ms = Mathf.RoundToInt(deltaTime * 1000f);
		Vector3 offset = Vector3.zero;

		// Apply the offset for each millisecond
		for (int i = 0; i < ms; ++i)
		{
			// Mimic 60 FPS the editor runs at
			offset += velocity * 0.06f;
			velocity *= dampeningFactor;
		}
		return offset;
	}

	static public float SpringDampen(ref float velocity, float strength, float deltaTime)
	{
		// Dampening factor applied each millisecond
		float dampeningFactor = 1f - strength * 0.001f;
		int ms = Mathf.RoundToInt(deltaTime * 1000f);
		float offset = 0f;

		// Apply the offset for each millisecond
		for (int i = 0; i < ms; ++i)
		{
			// Mimic 60 FPS the editor runs at
			offset += velocity * 0.06f;
			velocity *= dampeningFactor;
		}
		return offset;
	}

	/// <summary>
	/// Calculate how much to interpolate by.
	/// </summary>

	static public float SpringLerp (float strength, float deltaTime)
	{
		int ms = Mathf.RoundToInt(deltaTime * 1000f);
		deltaTime = 0.001f * strength;
		float cumulative = 0f;
		for (int i = 0; i < ms; ++i) cumulative = Mathf.Lerp(cumulative, 1f, deltaTime);
		return cumulative;
	}

	/// <summary>
	/// Mathf.Lerp(from, to, Time.deltaTime * strength) is not framerate-independent. This function is.
	/// </summary>

	static public float SpringLerp (float from, float to, float strength, float deltaTime)
	{
		int ms = Mathf.RoundToInt(deltaTime * 1000f);
		deltaTime = 0.001f * strength;
		for (int i = 0; i < ms; ++i) from = Mathf.Lerp(from, to, deltaTime);
		return from;
	}

	/// <summary>
	/// Vector2.Lerp(from, to, Time.deltaTime * strength) is not framerate-independent. This function is.
	/// </summary>

	static public Vector2 SpringLerp (Vector2 from, Vector2 to, float strength, float deltaTime)
	{
		return Vector2.Lerp(from, to, SpringLerp(strength, deltaTime));
	}

	/// <summary>
	/// Vector3.Lerp(from, to, Time.deltaTime * strength) is not framerate-independent. This function is.
	/// </summary>

	static public Vector3 SpringLerp (Vector3 from, Vector3 to, float strength, float deltaTime)
	{
		return Vector3.Lerp(from, to, SpringLerp(strength, deltaTime));
	}

	/// <summary>
	/// Quaternion.Slerp(from, to, Time.deltaTime * strength) is not framerate-independent. This function is.
	/// </summary>

	static public Quaternion SpringLerp (Quaternion from, Quaternion to, float strength, float deltaTime)
	{
		return Quaternion.Slerp(from, to, SpringLerp(strength, deltaTime));
	}

	/// <summary>
	/// Since there is no Mathf.RotateTowards...
	/// </summary>

	static public float RotateTowards (float from, float to, float maxAngle)
	{
		float diff = WrapAngle(to - from);
		if (Mathf.Abs(diff) > maxAngle) diff = maxAngle * Mathf.Sign(diff);
		return from + diff;
	}

    /// <summary>
    /// Wrap the index using repeating logic, so that for example +1 past the end means index of '1'.
    /// </summary>

    [System.Diagnostics.DebuggerHidden]
    [System.Diagnostics.DebuggerStepThrough]
    static public int RepeatIndex(int val, int max)
    {
        if (max < 1) return 0;
        while (val < 0) val += max;
        while (val >= max) val -= max;
        return val;
    }

    static public Rect RectMinus(Rect leftRect, Rect rightRect) {
        float left = leftRect.xMin - rightRect.xMin;
        float top = leftRect.yMin - rightRect.yMin;
        float width = leftRect.width - rightRect.width;
        float height = leftRect.height - rightRect.height;

        return new Rect(left,top,width,height);
    }

    static public Rect RectAdd(Rect leftRect, Rect rightRect)
    {
        float left = leftRect.xMin + rightRect.xMin;
        float top = leftRect.yMin + rightRect.yMin;
        float width = leftRect.width + rightRect.width;
        float height = leftRect.height + rightRect.height;

        return new Rect(left, top, width, height);
    }
}
