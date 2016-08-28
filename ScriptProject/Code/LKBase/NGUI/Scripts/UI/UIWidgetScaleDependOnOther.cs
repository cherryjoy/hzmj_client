
using UnityEngine;
using UnityEditor;

public class UIWidgetScaleDependOnOther : MonoBehaviour
{
    public UIWidget scaledWidget = null;
    public GameObject dependOnGameObject = null;

    public Vector2 sizeOffset = Vector2.zero;
    private bool isNeedUpdate = true;

    private Transform widgetTran;
    void Awake()
    {
        if (dependOnGameObject == null)
            enabled = false;
        if (scaledWidget == null)
            scaledWidget = gameObject.GetComponent<UIWidget>();
        if (scaledWidget == null)
            enabled = false;
        else
            widgetTran = scaledWidget.transform;
    }

    void Start()
    {
        if (isNeedUpdate && enabled)
        {
            StartScale();
            isNeedUpdate = false;
        }
    }

    public bool IsNeedUpdate
    {
        set
        {
            isNeedUpdate = true;
        }
    }

    void Update()
    {
        if (isNeedUpdate && enabled)
        {
            StartScale();
            isNeedUpdate = false;
        }
    }

    public void StartScaleImmediate()
    {
        StartScale();
        isNeedUpdate = false;
    }


    void StartScale()
    {
        Bounds dependBounds = NGUIMath.CalculateAbsoluteWidgetBounds(dependOnGameObject.transform);
        Vector3 bottomLeft = new Vector3(dependBounds.center.x - dependBounds.extents.x, dependBounds.center.y - dependBounds.extents.y, dependBounds.center.z);
        Vector3 topRight = new Vector3(dependBounds.center.x + dependBounds.extents.x, dependBounds.center.y + dependBounds.extents.y, dependBounds.center.z);

        Vector3 localBottomLeft = widgetTran.InverseTransformPoint(bottomLeft);
        Vector3 localTopRight = widgetTran.InverseTransformPoint(topRight);

        float width = localTopRight.x - localBottomLeft.x + sizeOffset.x;
        float height = localTopRight.y - localBottomLeft.y + sizeOffset.y;

        scaledWidget.Dimensions = new Vector2(width, height);
    }

    
}

