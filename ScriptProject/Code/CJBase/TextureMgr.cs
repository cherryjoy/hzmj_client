using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.IO;

public class TextureMgr : Singleton<TextureMgr>
{
    public enum TextureResizeType
    {
        NotResize,
        ResizeBasedOnOrigin,
        ResizeBasedOnWidth,
        ResizeBasedOnHeight
    }

    Dictionary<string, Material> mTextures = new Dictionary<string, Material>();

    public bool CheckIfInMgr(Material mat)
    {
        if (mat == null)
        {
            return false;
        }
        foreach (Material data in mTextures.Values)
        {
            if (data != null && data.Equals(mat))
            {
                return true;
            }
        }
        return false;
    }

    public Material LoadNetTexture(string url, WWW www)
    {
        Shader s = ResLoader.LoadAsset("Shader/NGUI/Shaders/Unlit - Transparent Colored") as Shader;
        Material mat = new Material(s);
        mat.mainTexture = www.texture;

        Material oldMat = null;
        if (mTextures.TryGetValue(url,out oldMat))
        {
            if (oldMat.mainTexture!= null)
            {
                GameObject.Destroy(oldMat.mainTexture);
                GameObject.Destroy(oldMat);
            }
            mTextures.Remove(url);
        }

        mTextures.Add(url, mat);
        return mat;
    }

    public Material LoadNetTexturePlus(string url, string shaderName, WWW www)
    {
        if (mTextures.ContainsKey(url))
        {
            Material outmtl;
            mTextures.TryGetValue(url, out outmtl);

            return outmtl;
        }

        Shader s = ResLoader.LoadAsset(shaderName) as Shader;
        Material mat = new Material(s);
        mat.mainTexture = www.texture;
        mTextures.Add(url, mat);
        return mat;
    }

    public Material LoadTexture(string path, bool transparent)
    {
        Shader s = ResLoader.LoadAsset("Shader/NGUI/Shaders/Unlit - Transparent Colored") as Shader;
        return BaseLoadTexture(s, path, transparent);
    }

    public Material LoadTextureWithShader(string shaderName, string path, bool transparent)
    {
        Shader s = ResLoader.LoadAsset(shaderName) as Shader;
        return BaseLoadTexture(s, path, transparent);
    }

    public Material LoadResizeTexture(UITexture texture, string shaderName, string path, bool transparent, TextureResizeType type)
    {
        Shader s = ResLoader.LoadAsset(shaderName) as Shader;
        Material mt =  BaseLoadTexture(s, path, transparent);

        texture.material = mt;

        ResizeUITexture(mt, texture, type);

        return mt;
    }

    private void ResizeUITexture(Material newMat, UITexture texture, TextureResizeType resizeType)
    {
        Vector3 dim = texture.Dimensions;

        if (newMat.mainTexture != null)
        {
            switch (resizeType)
            {
                case TextureResizeType.ResizeBasedOnOrigin:
                    {
                        dim.x = newMat.mainTexture.width;
                        dim.y = newMat.mainTexture.height;
                    }
                    break;

                case TextureResizeType.ResizeBasedOnWidth:
                    {
                        dim.y = dim.x / newMat.mainTexture.width * newMat.mainTexture.height;
                    }
                    break;

                case TextureResizeType.ResizeBasedOnHeight:
                    {
                        dim.x = dim.y / newMat.mainTexture.height * newMat.mainTexture.width;
                    }
                    break;
            }
        }

        texture.Dimensions = dim;
    }

    Material BaseLoadTexture(Shader s,string path,bool transparent) {
        if (mTextures.ContainsKey(path) == true)
        {
            Material outmtl;
            mTextures.TryGetValue(path, out outmtl);

            return outmtl;
        }
        else {
            Material mtl = new Material(s);
            mTextures.Add(path, mtl);
            Texture2D tex;
            if (transparent)
            {
                tex = new Texture2D(1, 1, TextureFormat.RGBA32, false);
                path += ".png";
            }
            else
            {
                tex = new Texture2D(1, 1, TextureFormat.RGB24, false);
                path += ".jpg";
            }
            tex.name = path;
            tex.wrapMode = TextureWrapMode.Clamp;
            mtl.mainTexture = tex;

            string cachePath = "Textures/" + path;

            byte[] texData = ResLoader.LoadRaw(cachePath);
            if (texData != null)
            {
                tex.LoadImage(texData);
            }
            return mtl;
        }
    }

    public void ReleaseMaterial(string textname)
    {
        Material mtl;
        if (mTextures.TryGetValue(textname, out mtl) == true)
        {
            mTextures.Remove(textname);
            Object.Destroy(mtl.mainTexture);
            Object.Destroy(mtl);
        }
    }

    public void ReleaseAll(bool isDestroy)
    {
        if (isDestroy)
        {
            foreach (Material mtl in mTextures.Values)
            {
                if (mtl != null && mtl.mainTexture != null)
                {
                    Object.Destroy(mtl.mainTexture);
                    Object.Destroy(mtl);
                }
            }   
        }
        mTextures.Clear();
    }

}
