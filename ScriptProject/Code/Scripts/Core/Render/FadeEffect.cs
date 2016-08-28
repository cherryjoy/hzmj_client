using UnityEngine;
using System.Collections;

public class FadeEffect : MonoBehaviour
{
    public enum FadeType
    {
        time,
        curve
    }

    public Shader shader;
    public Color FadeColor
    {
        get { return fadeColor; }
        set { 
            fadeColor = value;
            if(mMaterial != null)
                mMaterial.SetColor("_Color", fadeColor);
        }
    }
    private Color fadeColor;
    private Material mMaterial;

    void Awake()
    {
        if (!shader)
            shader = ResLoader.LoadAsset("Shader/FadeEffect") as Shader; 

        mMaterial = new Material(shader);
        mMaterial.SetColor("_Color", fadeColor);
    }

    void OnRenderImage(RenderTexture src, RenderTexture dest)
    {
        if (!CheckSupport(shader))
        {
            Graphics.Blit(src, dest);
            return;
        }

        mMaterial.SetTexture("_ColorBuffer", src);
        Graphics.Blit(src, dest, mMaterial);
    }

    private bool CheckSupport(Shader shader)
    {
        if (shader.isSupported)
        {
            return true;
        }
        else
        {
            return false;
        }
    }
    public void SetPercent(float percent)
    {
        mMaterial.SetFloat("_Percent", percent);
    }

    public void OnDestroy()
    {
        Destroy(mMaterial);
    }
}