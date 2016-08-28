using System;
using UnityEngine;

public static partial class ColorOperation
{
	public static string MakeGreenColor(object obj)
	{
		return string.Format("[6fff21]{0:s}[-]", obj.ToString());
	}

	public static string MakeRedColor(object obj)
	{
		return string.Format("[ff0000]{0:s}[-]", obj.ToString());
	}

	public static string MakeRoleNameYellowColor(object obj)
	{
		return string.Format("[ffbe3d]{0:s}[-]", obj.ToString());
	}

	public static string MakeDarkPurpleColor(object obj)
	{
		return string.Format("[b9b0da]{0:s}[-]", obj.ToString());
	}

	public static string MakeGrayColor(object obj)
	{
		return string.Format("[686868]{0:s}[-]", obj.ToString());
	}

	public static string MakeWhiteColor(object obj)
	{
		return string.Format("[fff8e7]{0:s}[-]", obj.ToString());
	}

	public static string MakeBlueColor(object obj)
	{
		return string.Format("[0060ff]{0:s}[-]", obj.ToString());
	}

	public static string MakeYellowColor(object obj)
	{
		return string.Format("[ffd200]{0:s}[-]", obj.ToString());
	}

	public static string MakeGoldColor(object obj)
	{
		return string.Format("[ffff00]{0:s}[-]", obj.ToString());
	}

    public static string MakeOrangeColor(object obj)
    {
        return string.Format("[ff9b00]{0:s}[-]", obj.ToString());
    }

	public static string MakePurpleColor(object obj)
	{
			 return string.Format("[8531ba]{0:s}[-]", obj.ToString());
    }
		

	public static string MakeColorStr(string colorStr, object obj)
	{
		return string.Format("[{0:s}]{1:s}[-]", colorStr, obj.ToString());
	}

	public static string MakePinkColor(object obj)
	{
		return string.Format("[ff00d8]{0:s}[-]", obj.ToString());
	}


	/// <summary>
	/// Make Color From String
	/// </summary>
	/// <param name="colorStr">Formate : FF0000 -> Red</param>
	/// <returns></returns>
	public static Color GetColorFromStr(string colorStr, float alpha = 1)
	{
		if (string.IsNullOrEmpty(colorStr) || colorStr.Length != 6)
		{
			return Color.white;
		}
		int r = Int32.Parse(colorStr.Substring(0, 2), System.Globalization.NumberStyles.HexNumber);
		int g = Int32.Parse(colorStr.Substring(2, 2), System.Globalization.NumberStyles.HexNumber);
		int b = Int32.Parse(colorStr.Substring(4), System.Globalization.NumberStyles.HexNumber);
		Color color = new Color((float)r/255f, (float)g/255f, (float)b/255f, alpha);
		return color;
	}

    public static Color GetColorFromRGBAStr(string colorStr)
    {
        if (string.IsNullOrEmpty(colorStr) || colorStr.Length != 8)
        {
            return Color.white;
        }
        int r = Int32.Parse(colorStr.Substring(0, 2), System.Globalization.NumberStyles.HexNumber);
        int g = Int32.Parse(colorStr.Substring(2, 2), System.Globalization.NumberStyles.HexNumber);
        int b = Int32.Parse(colorStr.Substring(4,2), System.Globalization.NumberStyles.HexNumber);
        int a = Int32.Parse(colorStr.Substring(6, 2), System.Globalization.NumberStyles.HexNumber);
        Color color = new Color((float)r / 255f, (float)g / 255f, (float)b / 255f, (float)a / 255f);
        return color;
    }
}
