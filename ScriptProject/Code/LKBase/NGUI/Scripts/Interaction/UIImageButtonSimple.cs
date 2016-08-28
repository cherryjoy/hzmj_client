using UnityEngine;

[ExecuteInEditMode]
[AddComponentMenu("NGUI/UI/Image Button Simple")]

public class UIImageButtonSimple : MonoBehaviour
{
    public UISprite target;
    void Start()
    {
        if (target == null) 
        {
            target = gameObject.GetComponent<UISprite>();
        }
    }
    void OnPress(bool pressed)
    {
        if (target == null) 
        {
            return;
        }

        if (pressed)
        {
            target.enabled = true;
        }
        else 
        {
            target.enabled = false;
        }
    }
}

