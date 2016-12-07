﻿//----------------------------------------------
//            NGUI: Next-Gen UI kit
// Copyright © 2011-2012 Tasharen Entertainment
//----------------------------------------------

using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Localization manager is able to parse localization information from text assets.
/// Although a singleton, you will generally not access this class as such. Instead
/// you should implement "void Localize (Localization loc)" functions in your classes.
/// Take a look at UILocalize to see how it's used.
/// </summary>

[AddComponentMenu("NGUI/Internal/Localization")]
public class Localization : MonoBehaviour
{
	static Localization mInst;
	static public Localization instance { get { return mInst; } }

	/// <summary>
	/// Language the localization manager will start with.
	/// </summary>

	public string startingLanguage;

	/// <summary>
	/// Available list of languages.
	/// </summary>

	public TextAsset[] languages;

	Dictionary<string, string> mDictionary = new Dictionary<string, string>();
	string mLanguage;

	/// <summary>
	/// Name of the currently active language.
	/// </summary>

	public string currentLanguage
	{
		get
		{
			if (string.IsNullOrEmpty(mLanguage))
			{
				currentLanguage = PlayerPrefs.GetString("Language");

				if (string.IsNullOrEmpty(mLanguage))
				{
					currentLanguage = startingLanguage;

					if (string.IsNullOrEmpty(mLanguage) && languages.Length > 0)
					{
						currentLanguage = languages[0].name;
					}
				}
			}
			return mLanguage;
		}
		set
		{
			if (languages != null && mLanguage != value)
			{
				if (string.IsNullOrEmpty(value))
				{
					mDictionary.Clear();
				}
				else
				{
					foreach (TextAsset asset in languages)
					{
						if (asset != null && asset.name == value)
						{
							Load(asset);
							return;
						}
					}
				}
				PlayerPrefs.DeleteKey("Language");
			}
		}
	}

	/// <summary>
	/// Determine the starting language.
	/// </summary>

	void Awake () { if (mInst == null) mInst = this; }

	/// <summary>
	/// Remove the instance reference.
	/// </summary>

	void OnDestroy () { if (mInst == this) mInst = null; }

	/// <summary>
	/// Load the specified asset and activate the localization.
	/// </summary>

	void Load (TextAsset asset)
	{
		mLanguage = asset.name;
		PlayerPrefs.SetString("Language", mLanguage);
		ByteReader reader = new ByteReader(asset);
		mDictionary = reader.ReadDictionary();
		NGUITools.Broadcast("OnLocalize", this);
	}

	/// <summary>
	/// Localize the specified value.
	/// </summary>

	public string Get (string key)
	{
		string val;
		return (mDictionary.TryGetValue(key, out val)) ? val : key;
	}
}