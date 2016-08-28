using UnityEngine;
using System.Collections;

[ExecuteInEditMode]
[RequireComponent(typeof(Camera))]
[AddComponentMenu("Image Effects/Simple Bloom")]
public class SimpleBloom : MonoBehaviour
{
    protected bool isSupported = true;

    public int erodeIterations = 1;
    public float bloomThreshhold = 0.7f;
    public int gaussBlurIterations = 1;
    public float gaussBlurDistance = 4.0f;
    public float bloomIntensity = 2.0f;
    public float sourceIntensity = 1.0f;
    private Shader myBloomShader;
    private Material MyBloomMaterial;

    private Material GammaCorrectMaterial;
    public bool viewGamma = false;
    public float gammaLevel0 = 60.0f;
    public float gammaLevel1 = -100.0f;
    public float gammaLevel2 = 1.0f;
    void Start()
    {
        CheckResources();
        if (!isSupported)
        {
            CreateGammaMat();
            if (GammaCorrectMaterial != null)
            {
                GammaCorrectMaterial.SetFloat("_gLevel0", gammaLevel0);
                GammaCorrectMaterial.SetFloat("_gLevel1", gammaLevel1);
                GammaCorrectMaterial.SetFloat("_gLevel2", gammaLevel2);
            }
        }
    }
    private void CreateGammaMat()
    {
        Shader gammaShader = ResLoader.LoadAsset("Shader/Gamma") as Shader; 
        if (gammaShader == null)
            enabled = false;
        GammaCorrectMaterial = new Material(gammaShader);
        GammaCorrectMaterial.hideFlags = HideFlags.DontSave;
    }
    public void OnRenderImage(RenderTexture source, RenderTexture destination)
    {
        if ((!isSupported || viewGamma))
        {
            if (GammaCorrectMaterial == null)
                CreateGammaMat();
            GammaCorrectMaterial.SetFloat("_gLevel0", gammaLevel0);
            GammaCorrectMaterial.SetFloat("_gLevel1", gammaLevel1);
            GammaCorrectMaterial.SetFloat("_gLevel2", gammaLevel2);
            GammaCorrectMaterial.SetTexture("_MainTex", source);
            Graphics.Blit(source, destination, GammaCorrectMaterial);
            return;
        }

        int rtW2 = source.width / 2;
        int rtH2 = source.height / 2;
        int rtW4 = source.width / 2;
        int rtH4 = source.height / 2;
        RenderTextureFormat rtFormat = RenderTextureFormat.Default;
        RenderTexture quarterRezColor = RenderTexture.GetTemporary(rtW4, rtH4, 0, rtFormat);
        RenderTexture halfRezColorDown = RenderTexture.GetTemporary(rtW2, rtH2, 0, rtFormat);
        RenderTexture secondQuarterRezColor = RenderTexture.GetTemporary(rtW4, rtH4, 0, rtFormat);

        //simple down sample
        //Graphics.Blit(source, halfRezColorDown, MyBloomMaterial, 1);//max down sample
        //Graphics.Blit(halfRezColorDown, quarterRezColor, MyBloomMaterial, 0);//blur down sample
        //down sample
        RenderTexture rtDown4 = RenderTexture.GetTemporary(rtW4, rtH4, 0, rtFormat);
        Graphics.Blit(source, halfRezColorDown, MyBloomMaterial, 1);//max down sample
        Graphics.Blit(halfRezColorDown, rtDown4, MyBloomMaterial, 1);//max down sample
        Graphics.Blit(rtDown4, quarterRezColor, MyBloomMaterial, 0);//blur down sample
        RenderTexture.ReleaseTemporary(rtDown4);

        //erode to aug bright range
        for (int ik = 0; ik < erodeIterations; ik++)
        {
            RenderTexture rt4 = RenderTexture.GetTemporary(rtW4, rtH4, 0, rtFormat);
            Graphics.Blit(quarterRezColor, rt4, MyBloomMaterial, 5);
            RenderTexture.ReleaseTemporary(quarterRezColor);
            quarterRezColor = rt4;
        }

        //filter bright
        MyBloomMaterial.SetVector("_Threshhold", new Vector4(bloomThreshhold, bloomThreshhold, bloomThreshhold, bloomThreshhold));
        Graphics.Blit(quarterRezColor, secondQuarterRezColor, MyBloomMaterial, 2);

        //Gauss Blur
        float oneOverBaseSize = 1.0f / 512.0f;
        for (int i = 0; i < gaussBlurIterations; ++i)
        {
            float spreadForPass = (1.0f + i * 0.25f) * gaussBlurDistance;
            RenderTexture blur4 = RenderTexture.GetTemporary(rtW4, rtH4, 0, rtFormat);
            MyBloomMaterial.SetVector("_Offsets", new Vector4(0.0f, spreadForPass * oneOverBaseSize, 0.0f, 0.0f));
            Graphics.Blit(secondQuarterRezColor, blur4, MyBloomMaterial, 3);
            RenderTexture.ReleaseTemporary(secondQuarterRezColor);
            secondQuarterRezColor = blur4;
        }

        //Add Bloom To source
        MyBloomMaterial.SetFloat("_Intensity", bloomIntensity);
        MyBloomMaterial.SetFloat("_SourceIntensity", sourceIntensity);
        MyBloomMaterial.SetTexture("_ColorBuffer", source);
        Graphics.Blit(secondQuarterRezColor, destination, MyBloomMaterial, 4);
        //destination = _colorBuffer + _Intensity * secondQuarterRezColor

        RenderTexture.ReleaseTemporary(halfRezColorDown);
        RenderTexture.ReleaseTemporary(quarterRezColor);
        RenderTexture.ReleaseTemporary(secondQuarterRezColor);
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
        if (!myBloomShader)
            myBloomShader = ResLoader.LoadAsset("Shader/SimpleBloom") as Shader; 

        if (!myBloomShader || !myBloomShader.isSupported)
        {
            Debug.Log("The Shader \"" + myBloomShader.ToString() + "\" is Missing or not supported.");
            isSupported = false;
        }
        else if (!MyBloomMaterial || MyBloomMaterial.shader != myBloomShader)
        {
            MyBloomMaterial = new Material(myBloomShader);
            MyBloomMaterial.hideFlags = HideFlags.DontSave;

            MyBloomMaterial.SetVector("_Threshhold", new Vector4(bloomThreshhold, bloomThreshhold, bloomThreshhold, bloomThreshhold));
            MyBloomMaterial.SetFloat("_Intensity", bloomIntensity);
            MyBloomMaterial.SetFloat("_SourceIntensity", sourceIntensity);
        }

        return isSupported;
    }
}
