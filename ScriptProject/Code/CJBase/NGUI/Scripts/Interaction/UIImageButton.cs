//----------------------------------------------
//            NGUI: Next-Gen UI kit
// Copyright ?2011-2012 Tasharen Entertainment
//----------------------------------------------

using UnityEngine;

/// <summary>
/// Sample script showing how easy it is to implement a standard button that swaps sprites.
/// </summary>

[ExecuteInEditMode]
[AddComponentMenu("NGUI/UI/Image Button")]
public class UIImageButton : MonoBehaviour
{
    public UISprite[] targets = new UISprite[0];
    public string normalSprite;
    public string hoverSprite;
    public string pressedSprite;
    public string disableSprite;

    private bool mEnable = true;

    void Start()
    {
       // if (targets.Length != 0) targets = GetComponentsInChildren<UISlicedSprite>();
    }

    void OnHover(bool isOver)
    {
        if (targets.Length != 0 && mEnable == true)
        {
            foreach (UISprite tar in targets)
            {
                if (tar != null)
                {
                    tar.spriteName = isOver ? hoverSprite : normalSprite;
                    tar.MakePixelPerfect();
                    tar.MarkAsChanged();
                }
            }

        }
    }

    void OnPress(bool pressed)
    {
        if (targets.Length != 0 && mEnable == true)
        {
            foreach (UISprite tar in targets)
            {
                if (tar != null)
                {
                    tar.spriteName = pressed ? pressedSprite : normalSprite;
                    tar.MakePixelPerfect();
                }
            }

        }
    }

    public bool enable
    {
        set
        {
            BoxCollider box = GetComponent<Collider>() as BoxCollider;
            box.enabled = value;
            mEnable = value;

            foreach (UISprite tar in targets)
            {
                if (tar != null)
                {
                    if (mEnable == true)
                    {
                        tar.spriteName = normalSprite;
                    }
                    else
                    {
                        tar.spriteName = disableSprite;
                    }
                    tar.MakePixelPerfect();
                }
            }

        }

        get { return mEnable; }
    }
    public void JustRefresh()
    {
        foreach (UISprite tar in targets)
        {
            if (tar != null)
            {
                tar.spriteName = normalSprite;
                tar.MakePixelPerfect();
                tar.MarkAsChanged();
            }
        }
    }

}
