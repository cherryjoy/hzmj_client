using UnityEngine;
using System.Collections;

[ExecuteInEditMode]
public class UIMatchSize : MonoBehaviour
{
    public UIWidget Target;
    public enum Direction
    {
        Vertical,
        Horizontal,
        Both,
    }

    public Direction direcion = Direction.Vertical;
    public Vector3 offset = Vector3.zero;
    public bool transformDirection = false;
    public bool useForCollider = false;
    public bool updateAllTheTime = false;
    private int updateCount = 1;
   


    void DoMatchSize()
    {
        Vector3 vSize = offset;
        if (Target != null)
        {
            vSize.x += Target.Dimensions.x;
            vSize.y += Target.Dimensions.y;
           // vSize.x *= rate.x;
            //vSize.y *= rate.y;
            if (!useForCollider) 
            {
                UIWidget uiwidget = transform.GetComponent<UIWidget>();
                if (uiwidget != null)
                {
                    if (direcion == Direction.Horizontal)
                    {
                        uiwidget.Dimensions = transformDirection ? new Vector2(uiwidget.Dimensions.x, vSize.x) : new Vector2(vSize.x, uiwidget.Dimensions.y);
                    }
                    if (direcion == Direction.Vertical)
                    {
                        uiwidget.Dimensions = transformDirection ? new Vector2(vSize.y, uiwidget.Dimensions.y) : new Vector2(uiwidget.Dimensions.x, vSize.y);
                    }
                    if (direcion == Direction.Both)
                    {
                        uiwidget.Dimensions = transformDirection ? new Vector2(vSize.y, vSize.x) : new Vector2(vSize.x, vSize.y);
                    }
                }
            }
            else
            {
                BoxCollider boxCollider = transform.GetComponent<BoxCollider>();
                if (boxCollider != null) 
                {
                    if (direcion == Direction.Horizontal)
                    {

                        boxCollider.size = transformDirection ? new Vector3(boxCollider.size.x, vSize.x, boxCollider.size.z) : new Vector3(vSize.x, boxCollider.size.y, boxCollider.size.z);
                    }
                    if (direcion == Direction.Vertical)
                    {
                        boxCollider.size = transformDirection ? new Vector3(vSize.y, boxCollider.size.y, boxCollider.size.z) : new Vector3(boxCollider.size.x, vSize.y, boxCollider.size.z);
                    }
                    if (direcion == Direction.Both)
                    {
                        boxCollider.size = transformDirection ? new Vector3(vSize.y, vSize.x, boxCollider.size.z) : new Vector3(vSize.x, vSize.y, boxCollider.size.z);
                    }
                }

            }
            
        
        }
    }

#if !UNITY_EDITOR
    void Start()
    {
		DoMatchSize();
		Destroy(this);
    }
#endif

#if UNITY_EDITOR
    void LateUpdate()
    {
        if (updateAllTheTime == false)
        {
            if (updateCount > 0)
            {
                DoMatchSize();
                updateCount--;
            }
        }
        else
        {
            DoMatchSize();
        }
    }
#endif
}