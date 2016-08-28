using UnityEngine;
using System.Collections;

[ExecuteInEditMode]
[RequireComponent(typeof(Camera))]
[AddComponentMenu("Image Effects/Simple Blur")]
public class SimpleBlur : MonoBehaviour
{
    protected bool isSupported = true;
    private Shader myBlurShader;
    private Material blurMaterial;
    public float blurSize = 3.0f;
    void Start()
    {
        CheckResources();
    }
    public bool CheckResources()
    {
        isSupported = true;
        if (!SystemInfo.supportsImageEffects || !SystemInfo.supportsRenderTextures)
        {
            Debug.Log("System not support ImageEffects or RenderTextures.");
            isSupported = false;
            return false;
        }
        if (!myBlurShader)
            myBlurShader = ResLoader.LoadAsset("Shader/MobileBlur") as Shader; 

        if (!myBlurShader || !myBlurShader.isSupported)
        {
            Debug.Log("The Shader \"" + myBlurShader.ToString() + "\" is Missing or not supported.");
            isSupported = false;
        }
        else if (!blurMaterial || blurMaterial.shader != myBlurShader)
        {
            blurMaterial = new Material(myBlurShader);
            blurMaterial.hideFlags = HideFlags.DontSave;
            /*
            MyBlurMaterial.SetVector("_Threshhold", new Vector4(bloomThreshhold, bloomThreshhold, bloomThreshhold, bloomThreshhold));
            MyBlurMaterial.SetFloat("_Intensity", bloomIntensity);
            MyBlurMaterial.SetFloat("_SourceIntensity", sourceIntensity);*/
        }

        return isSupported;
    }
    public void OnRenderImage(RenderTexture source, RenderTexture destination)
    {
        if(CheckResources() == false) {
			Graphics.Blit (source, destination);
			return;
		}
		float widthMod  = 1.0f;

		blurMaterial.SetVector ("_Parameter", new Vector4 (blurSize * widthMod, -blurSize * widthMod, 0.0f, 0.0f));
		source.filterMode = FilterMode.Bilinear;

		int rtW  = source.width;
		int rtH  = source.height;

		// downsample
		RenderTexture rt = RenderTexture.GetTemporary (rtW, rtH, 0, source.format);

		rt.filterMode = FilterMode.Bilinear;
		Graphics.Blit (source, rt, blurMaterial, 0);

		blurMaterial.SetVector ("_Parameter", new Vector4 (blurSize * widthMod, -blurSize * widthMod , 0.0f, 0.0f));

		// vertical blur
		RenderTexture rt2   = RenderTexture.GetTemporary (rtW, rtH, 0, source.format);
		rt2.filterMode = FilterMode.Bilinear;
		Graphics.Blit (rt, rt2, blurMaterial, 1);
		RenderTexture.ReleaseTemporary (rt);
		rt = rt2;

		// horizontal blur
		rt2 = RenderTexture.GetTemporary (rtW, rtH, 0, source.format);
		rt2.filterMode = FilterMode.Bilinear;
		Graphics.Blit (rt, rt2, blurMaterial, 2);
		RenderTexture.ReleaseTemporary (rt);
		rt = rt2;
		
		Graphics.Blit (rt, destination);

		RenderTexture.ReleaseTemporary (rt);
    }
}