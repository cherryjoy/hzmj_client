﻿using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UniLua;


/// 头像列表下载器;
public class WWWPortraitLoader : MonoBehaviour
{
    public static string generateLocalPath(string wwwUrl)
    {
        return Application.persistentDataPath + "/ImageCache/" + wwwUrl.GetHashCode();
    }

    public static void LoadOnLinePhoto(string strURL, string successFuncName, string failFuncName, object tex, string shaderName, string defaultIconPath)
    {
        GameObject go = new GameObject("WWWPortraitLoader");
        WWWPortraitLoader loader = go.AddComponent<WWWPortraitLoader>();
        //判断是否是第一次加载这张图片;
        string imagePath = generateLocalPath(strURL);
        bool isLocal = false;
        if (!File.Exists(imagePath))
        {
            isLocal = false;
        }
        else
        {
            isLocal = true;
            strURL = "file:///" + generateLocalPath(strURL);
        }

        loader.StartCoroutine(loader.DownloadImage(strURL, successFuncName, failFuncName, tex, shaderName, defaultIconPath, isLocal));
    }

    private IEnumerator DownloadImage(string url, string successFuncName, string failFuncName, object texture, string shaderName, string defaultIconPath, bool isLocal)
    {
        if (string.IsNullOrEmpty(url))
        {
            yield break;
        }
        LuaState lua = LuaInstance.instance.Get();
        WWW www = new WWW(url);
        yield return www;
        if (www.isDone)
        {
            if (www.error == null)
            {
                Material mat = TextureMgr.Instance.LoadNetTexturePlus(url, shaderName, www);
                lua.LuaFuncCall(
                    PlatformSDKController.mSelf.luaPlatformHanderRef, successFuncName, texture, mat);

                //将图片保存至缓存路径;
                if (!isLocal)
                {
                    SaveImageToLocal(url, mat.mainTexture as Texture2D);
                }
            }
            else
            {
                Debug.LogWarning("DownloadImage:data.imgname load error:" + www.error);
                lua.LuaFuncCall(
                    PlatformSDKController.mSelf.luaPlatformHanderRef, failFuncName, texture, defaultIconPath);
            }
        }

        www.Dispose();
        Destroy(gameObject);
    }

    public static bool SaveImageToLocal(string url, Texture2D image)
    {
        if (string.IsNullOrEmpty(url) || image == null)
        {
            return false;
        }

        string path = Application.persistentDataPath + "/ImageCache/";
        if (!Directory.Exists(path))
        {
            Directory.CreateDirectory(path);
        }

        //将图片保存至缓存路径;
        byte[] pngData = image.EncodeToPNG();
        string imagePath = generateLocalPath(url);
        File.WriteAllBytes(imagePath, pngData);

        return true;
    }

    public static void DeleteImageFromLocal(string wwwUrl)
    {
        string imagePath = generateLocalPath(wwwUrl);
        if (File.Exists(imagePath))
        {
            File.Delete(imagePath);
        }
    }
}