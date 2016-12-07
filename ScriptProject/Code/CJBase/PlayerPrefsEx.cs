using System;
using UnityEngine;

// for chinese language
public class PlayerPrefsEx {
	public static void SetString(string key, string str)
	{
		PlayerPrefs.SetString(key, System.Uri.EscapeUriString(str));
	}
	public static string GetString(string key)
	{
		return Uri.UnescapeDataString(PlayerPrefs.GetString(key));
	}

    public static void Save()
    {
        PlayerPrefs.Save();
    }

    public static void DeleteAll()
    {
        PlayerPrefs.DeleteAll();
    }

    public static void DeleteKey(string key)
    {
        PlayerPrefs.DeleteKey(key);
    }

    public static float GetFloat(string key)
    {
        return PlayerPrefs.GetFloat(key);
    }

    public static int GetInt(string key)
    {
        return PlayerPrefs.GetInt(key);
    }

    public static bool HasKey(string key)
    {
        return PlayerPrefs.HasKey(key);
    }

    public static void SetFloat(string key, float value)
    {
        PlayerPrefs.SetFloat(key, value);
    }

    public static void SetInt(string key, int value)
    {
        PlayerPrefs.SetInt(key, value);
    }
}
