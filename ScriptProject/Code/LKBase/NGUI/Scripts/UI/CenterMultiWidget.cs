using UnityEngine;
using System.Collections;

[ExecuteInEditMode]
public class CenterMultiWidget : MonoBehaviour {
    public bool CenterNow = false;
    public bool IsCenterGameobjcet = false;
    Vector3 OriPos = Vector3.one;

    void Start()
    {
        RePos();
    }

    void Update()
    {
        if (CenterNow)
        {
            RePos();
            CenterNow = false;
        }
	}

    public void RePos()
    {
        if (transform.parent == null)
        {
            //Debug.Log("center target's parent is null.");
            return;
        }
        Bounds currentBounds = NGUIMath.CalculateRelativeWidgetBounds(transform.parent, transform);

        float boundsOffset = transform.localPosition.x - currentBounds.center.x;
        if (IsCenterGameobjcet)
        {
            for (int i = 0; i <transform.childCount; i++)
            {
                transform.GetChild(i).localPosition += new Vector3(boundsOffset, 0, 0);
            }
        }
        else
        {
            UIWidget[] widgets = transform.GetComponentsInChildren<UIWidget>(true);
            for (int i = 0; i < widgets.Length; i++)
            {
                widgets[i].transform.localPosition += new Vector3(boundsOffset, 0, 0);
            }
        }
        
    }
}
