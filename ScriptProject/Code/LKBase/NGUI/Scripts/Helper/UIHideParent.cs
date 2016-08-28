using UnityEngine;
using System.Collections;

public class UIHideParent : MonoBehaviour {

	void OnDisable()
	{
		if (transform.parent != null)
		{
			transform.parent.gameObject.SendMessage("OnHideParentClose", SendMessageOptions.DontRequireReceiver);
		}
	}


    void OnClose()
    {
        transform.parent.gameObject.SetActive(false);
    }

}
