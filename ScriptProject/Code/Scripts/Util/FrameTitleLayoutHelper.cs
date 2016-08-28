using UnityEngine;

[ExecuteInEditMode]
public class FrameTitleLayoutHelper : MonoBehaviour
{
    public UISlicedSprite bgSprite;

    public GameObject shadowGO;
    public GameObject lineGO;

#if UNITY_EDITOR
    private FrameLayoutHelper parentHelper;
    void Start()
    {
        parentHelper = NGUITools.FindInParents<FrameLayoutHelper>(gameObject);
        if (parentHelper!=null)
        {
            if (parentHelper.FrameSize.x != 0)
            {
                LayoutFrame(parentHelper.FrameSize);
            }
        }
    }

    //void Update()
    //{
    //    if (parentHelper != null)
    //    {
    //        if (parentHelper.FrameSize.x != 0)
    //        {
    //            LayoutFrame(parentHelper.FrameSize);
    //        }
    //    }
    //}

    public void LayoutFrame(Vector2 fSize)
    {
        if (parentHelper == null)
            parentHelper = NGUITools.FindInParents<FrameLayoutHelper>(gameObject);

        transform.localPosition = Vector3.zero;

        if (parentHelper == null)
        {
            return;
        }

        if(bgSprite != null)
        {
            bgSprite.Dimensions = new Vector2(fSize.x, bgSprite.Dimensions.y);
        }
        if(shadowGO!=null)
        {
            shadowGO.transform.localPosition = new Vector3(fSize.x * 0.5f, -36f, 0f);
            UISprite[] childs = shadowGO.GetComponentsInChildren<UISprite>();
            foreach(UISprite child in childs)
            {
                child.Dimensions = new Vector2(fSize.x * 0.5f-2, child.Dimensions.y);
            }
        }
        if(lineGO!=null)
        {
            lineGO.transform.localPosition = new Vector3(fSize.x * 0.5f, 0, 0);
            UISprite[] childs = lineGO.GetComponentsInChildren<UISprite>();
            foreach (UISprite child in childs)
            {
                child.Dimensions = new Vector2(fSize.x * 0.5f*0.7f, child.Dimensions.y);
            }
        }
    }
#endif

}

