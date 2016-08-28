﻿//----------------------------------------------
//            NGUI: Next-Gen UI kit
// Copyright © 2011-2012 Tasharen Entertainment
//----------------------------------------------

using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Very simple sprite animation. Attach to a sprite and specify a bunch of sprite names and it will cycle through them.
/// </summary>

[ExecuteInEditMode]
[RequireComponent(typeof(UISprite))]
[AddComponentMenu("NGUI/UI/Sprite Animation")]
public class UISpriteAnimation : MonoBehaviour
{
	[SerializeField] int mFPS = 30;
	[SerializeField] string mPrefix = "";
	public bool mLoop = false;
	public Vector2 Scale = Vector2.one;

	UISprite mSprite;
	float mDelta = 0f;
	int mIndex = 0;
	List<string> mSpriteNames = new List<string>();

	/// <summary>
	/// Animation framerate.
	/// </summary>

	public int framesPerSecond { get { return mFPS; } set { mFPS = value; } }

	/// <summary>
	/// Set the name prefix used to filter sprites from the atlas.
	/// </summary>

	public string namePrefix { get { return mPrefix; } set { if (mPrefix != value) { mPrefix = value; RebuildSpriteList(); } } }

	/// <summary>
	/// Rebuild the sprite list first thing.
	/// </summary>

	void Start () { RebuildSpriteList(); }

	/// <summary>
	/// Advance the sprite animation process.
	/// </summary>

	void Update ()
	{
		if (mSpriteNames.Count > 1 && Application.isPlaying)
		{
			mDelta += Time.deltaTime;
			float rate = mFPS > 0f ? 1f / mFPS : 0f;

			if (rate < mDelta)
			{
				mDelta = (rate > 0f) ? mDelta - rate : 0f;
				if (++mIndex >= mSpriteNames.Count) 
				{
					mIndex = 0;
					
					if(mLoop == false)
						Destroy(gameObject);
				}
				mSprite.spriteName = mSpriteNames[mIndex];
				Texture2D tex = mSprite.mainTexture as Texture2D;
				
				if(tex != null)
				{
					transform.localScale = new Vector3(Mathf.RoundToInt(Mathf.Abs(mSprite.outerUV.width * tex.width)) * Scale.x,
					Mathf.RoundToInt(Mathf.Abs(mSprite.outerUV.height * tex.height)) * Scale.y,
					1f);
				}
				mSprite.MakePixelPerfect();
			}
		}
	}

	/// <summary>
	/// Rebuild the sprite list after changing the sprite name.
	/// </summary>

	void RebuildSpriteList ()
	{
		if (mSprite == null) mSprite = GetComponent<UISprite>();
		mSpriteNames.Clear();

		if (mSprite != null && mSprite.atlas != null)
		{
			List<UIAtlas.Sprite> sprites = mSprite.atlas.spriteList;
			
			foreach (UIAtlas.Sprite sprite in sprites)
			{
				if (string.IsNullOrEmpty(mPrefix) || sprite.name.StartsWith(mPrefix))
				{
					mSpriteNames.Add(sprite.name);
				}
			}
			mSpriteNames.Sort();
		}
	}
}
