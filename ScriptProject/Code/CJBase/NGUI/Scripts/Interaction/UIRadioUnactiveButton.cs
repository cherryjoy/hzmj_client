using UnityEngine;
using System.Collections;

[ExecuteInEditMode]
[AddComponentMenu("NGUI/UI/Radio Unactive Button")]
public class UIRadioUnactiveButton : MonoBehaviour
{
    public bool UseObjTarget = false;
	public UISprite target;
    public GameObject objTarget;
	public bool option = true;
	private bool  bActice;

    private GameObject usingTarget;
	
	void OnEnable ()
    {
        if (UseObjTarget)
        {
            usingTarget = objTarget;
        }

        else
        {
            if (target == null)
                target = GetComponentInChildren<UISprite>();
            usingTarget = target.gameObject;
        }
		
	}
	
	void Set (bool state)
	{
		if (usingTarget != null) {
			bActice = state;	
			if (bActice != usingTarget.activeSelf)
				usingTarget.gameObject.SetActive (bActice);
			if (option && bActice == true) {
				UIRadioUnactiveButton[] rbs = transform.parent.GetComponentsInChildren<UIRadioUnactiveButton> ();
				foreach (UIRadioUnactiveButton rb in rbs)
					if (rb != this)
						rb.Set (false);
			}			
		}
	}
	
	void OnChooseButton ()
	{
		Set (true);		
	}
	
	void OnClick ()
	{
		if (enabled) {
			if (bActice == false) {
				Set (true);
			}
		}
	}
	
	public void InitCheck(bool state)
	{
        if (usingTarget == null)
        {
            OnEnable();
        }

		if (usingTarget != null) {
			bActice = state;	
			if (bActice != usingTarget.activeSelf)
				usingTarget.gameObject.SetActive (bActice);
		}
	}
}
