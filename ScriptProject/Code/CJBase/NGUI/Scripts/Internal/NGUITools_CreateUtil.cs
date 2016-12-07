using UnityEngine;
using System;
using System.Collections.Generic;

static public partial class NGUITools
{
	public static  UISprite CreateSprite(GameObject parent,UIAtlas atlas,string spriteName)
	{
		int depth = NGUITools.CalculateNextDepth(parent);
		UISprite bg = NGUITools.AddWidget<UISprite>(parent);
		bg.name = "Background";
		bg.depth = depth;
		bg.atlas = atlas;
		bg.spriteName = spriteName;
		Texture2D tex = bg.mainTexture as Texture2D;
		bg.transform.localScale = new Vector3(Mathf.RoundToInt(Mathf.Abs(bg.outerUV.width * tex.width)),
			Mathf.RoundToInt(Mathf.Abs(bg.outerUV.height * tex.height)),
			1f);
		bg.MakePixelPerfect();
		return bg;
	}

	public static UISprite CreateSprite(GameObject parent, UIAtlas atlas, string spriteName, Vector3 scale)
	{
		UISprite sprite = CreateSprite(parent, atlas, spriteName);
		sprite.transform.localScale = scale;
		return sprite;
	}

	public static UISlicedSprite CreateSlicedSprite(GameObject parent, UIAtlas atlas, string spriteName)
	{
		int depth = NGUITools.CalculateNextDepth(parent);
		UISlicedSprite bg = NGUITools.AddWidget<UISlicedSprite>(parent);
		bg.name = "Background";
		bg.depth = depth;
		bg.atlas = atlas;
		bg.spriteName = spriteName;
		Texture2D tex = bg.mainTexture as Texture2D;
		bg.transform.localScale = new Vector3(Mathf.RoundToInt(Mathf.Abs(bg.outerUV.width * tex.width)),
			Mathf.RoundToInt(Mathf.Abs(bg.outerUV.height * tex.height)),
			1f);
		bg.MakePixelPerfect();
		return bg;
	}

	public static UISlicedSprite CreateSlicedSprite(GameObject parent, UIAtlas atlas, string spriteName, Vector3 scale)
	{
		UISlicedSprite sprite = CreateSlicedSprite(parent, atlas, spriteName);
		sprite.transform.localScale = scale;
		return sprite;
	}

	public static UILabel CreateLabel(GameObject parent,string text, int fontSize, bool useArtFont)
	{
		int depth = NGUITools.CalculateNextDepth(parent);
		UILabel lbl = NGUITools.AddWidget<UILabel>(parent);
		lbl.depth = depth + 1;
		lbl.text = text;
		if (useArtFont)
		{
			lbl.TrueTypeFont = UIFont.ArtFont;
		}
		lbl.MakePixelPerfect();
		lbl.transform.localScale = new UnityEngine.Vector3(fontSize, fontSize, 1);
		return lbl;
	}

	public static UILabel CreateLabel(GameObject parent, string text)
	{
		return CreateLabel(parent, text, 22, false);
	}
}
