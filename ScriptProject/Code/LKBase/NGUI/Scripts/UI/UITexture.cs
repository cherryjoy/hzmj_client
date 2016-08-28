//----------------------------------------------
//            NGUI: Next-Gen UI kit
// Copyright © 2011-2012 Tasharen Entertainment
//----------------------------------------------

using UnityEngine;
using System.Collections.Generic;
using System.IO;
using System.Collections;

/// <summary>
/// If you don't have or don't wish to create an atlas, you can simply use this script to draw a texture.
/// Keep in mind though that this will create an extra draw call with each UITexture present, so it's
/// best to use it only for backgrounds or temporary visible widgets.
/// </summary>

[ExecuteInEditMode]
[AddComponentMenu("NGUI/UI/Texture")]
public class UITexture : UIWidget
{
    public Vector4 UVOffset = Vector4.zero;
	/// <summary>
	/// Adjust the scale of the widget to make it pixel-perfect.
	/// </summary>
	public string MaterialPath;

    void Awake()
    {
        if(material!=null)
        {
            base.material = new Material(material);
        }
    }

	public bool IsNoLoaded()
	{	
		if(material.mainTexture == null ) return true;
		else return false;
	}
	
	override public void MakePixelPerfect ()
	{
		Texture tex = mainTexture;

		if (tex != null)
		{
            cachedTransform.localScale = Vector3.one;

            Dimensions = new Vector3(tex.width, tex.height, 1);
		}
	}

    public override Vector4 drawingDimensions
    {
        get
        {
            Vector2 offset = pivotOffset;

            float x0 = (offset.x) * mWidth;
            float y0 = (offset.y - 1) * mHeight;
            float x1 = x0 + mWidth;
            float y1 = offset.y * mHeight;

            return new Vector4(
                mDrawRegion.x == 0f ? x0 : Mathf.Lerp(x0, x1, mDrawRegion.x),
                mDrawRegion.y == 0f ? y0 : Mathf.Lerp(y0, y1, mDrawRegion.y),
                mDrawRegion.z == 1f ? x1 : Mathf.Lerp(x0, x1, mDrawRegion.z),
                mDrawRegion.w == 1f ? y1 : Mathf.Lerp(y0, y1, mDrawRegion.w));
        }
    }	

	/// <summary>
	/// Virtual function called by the UIScreen that fills the buffers.
	/// </summary>
	override public void OnFill (BetterList<Vector3> verts, BetterList<Vector2> uvs, BetterList<Color> cols)
	{
        Vector4 v = drawingDimensions;

        verts.Add(new Vector3(v.x, v.y));
        verts.Add(new Vector3(v.x, v.w));
        verts.Add(new Vector3(v.z, v.w));
        verts.Add(new Vector3(v.z, v.y));

        float x = UVOffset.x / Dimensions.x;
        float y = UVOffset.y / Dimensions.y;
        float z = UVOffset.z / Dimensions.x;
        float w = UVOffset.w / Dimensions.y;
        uvs.Add(new Vector2(0 + x, 0 + y));
        uvs.Add(new Vector2(0f + x, 1f + w));
        uvs.Add(new Vector2(1 + z, 1 + w));
        uvs.Add(new Vector2(1f + z, 0f + y));


        cols.Add(color);
        cols.Add(color);
        cols.Add(color);
        cols.Add(color);
	}
	
	public Texture2D AysnLoadTex(string strPath, string suffix = ".png")
	{
		if (string.IsNullOrEmpty(strPath) == true)
			return null;
		
		Texture2D tex2D = ResLoader.Load(strPath) as Texture2D;
		if(tex2D != null) 
			return tex2D;
	
		return null;
	}

	public void ReleaseSyncLoadMaterial()
	{
		ReleaseMaterialAndTexture(material);
		material = null;
	}

	public static void  ReleaseMaterialAndTexture(Material mat)
	{
		if (mat != null)
		{
			mat.mainTexture = null;
		}
		mat = null;
	}

    public override Material material
    {
        get
        {
            return base.material;
        }
        set
        {
            if(base.material != null)
            {
                Destroy(base.material);
            }
            if(value!=null)
            {
                base.material = new Material(value);
            }else
            {
                base.material = null;
            }
        }
    }

    public Texture MainTexture
    {
        get
        {
            if (material != null)
                return material.mainTexture;
            return null;
        }
        set
        {
            if (base.material == null && value!=null)
            {
                Shader s = ResLoader.LoadAsset("Shader/NGUI/Shaders/Unlit - Transparent Colored") as Shader;
                Material mat = new Material(s);
                mat.mainTexture = value;

                base.material = mat;
            }else if(base.material!=null)
            {
                base.material.mainTexture = value;
                if (panel != null)
                {
                    panel.MarkMaterialAsChanged(base.material, false);
                    panel.ChangeMtl = true;
                }
            }
        }
    }

    public void SetMainTextureByPath(string texturePath)
    {
        if (texturePath == null)
        {
            MainTexture = null;
            return;
        }
        Texture2D tex = ResLoader.Load(texturePath, typeof(Texture2D)) as Texture2D;
        MainTexture = tex;
       
    }

    protected new void OnDestroy ()
    {
        if(base.material!=null)
        {
            base.material.mainTexture = null;

            Destroy(base.material);
            base.material = null;
        }

        base.OnDestroy();
    }
}
