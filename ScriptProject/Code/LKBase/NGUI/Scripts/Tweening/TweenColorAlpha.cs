using UnityEngine;

public class TweenColorAlpha : UITweener
{
    public float from = 0.0f;
    public float to = 1.0f;
    public float cur = 0.0f;

    public bool isIncludeChild = false;

    UIWidget mWidget;
    Material mMat;
    
    public float Alpha
    {
        get
        {
            if(!isIncludeChild)
            { 
                if (mWidget != null) return mWidget.color.a;
                if (mMat != null) return mMat.color.a;
            }
            return cur;
        }
        set
        {
            if(!isIncludeChild)
            {
                if (mWidget != null) mWidget.color = new Color(mWidget.color.r, mWidget.color.g, mWidget.color.b, value);
            }else
            {
                if (chidrenWidgets!=null && chidrenWidgets.Length>0)
                {
                    for(int i = 0;i<chidrenWidgets.Length;i++)
                    {
                        chidrenWidgets[i].color = new Color(chidrenWidgets[i].color.r, chidrenWidgets[i].color.g, chidrenWidgets[i].color.b, value);
                    }
                }
            }

            if (mMat != null) mMat.color = new Color(mMat.color.r, mMat.color.g, mMat.color.b, value);
            cur = value;
        }
    }

    void Awake()
    {
        if(!isIncludeChild)
        {
            mWidget = GetComponentInChildren<UIWidget>();
        }
        Renderer ren = this.GetComponent<Renderer>();
        if (ren != null) mMat = ren.material;
    }

    protected override void OnUpdate(float factor)
    {
        Alpha = from * (1f - factor) + to * factor;
    }

    static public TweenColorAlpha Begin(GameObject go,float duration,float from,float to,bool isIncludeChild)
    {
        TweenColorAlpha comp = UITweener.Begin<TweenColorAlpha>(go, duration);
        comp.to = to;
        comp.from = from;
        comp.isIncludeChild = isIncludeChild;

        return comp;
    }

}

