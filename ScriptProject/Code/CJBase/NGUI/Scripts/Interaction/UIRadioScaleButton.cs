using UnityEngine;
using System.Collections;

[ExecuteInEditMode]
[AddComponentMenu("NGUI/UI/Radio Scale Button")]
public class UIRadioScaleButton : MonoBehaviour
{
	public UISprite target;
	public Vector3 activeScale;
	public Vector3 unactiveScale;
	public bool option = true;
	public bool Startstate = false;
	private bool  bActice;
	public object user_data;
	public GameObject[] user_data_recvs;
	public bool Auto = true;
	public bool ClickForbidOnDataEmpty = false;
	public string user_data_send_name = "OnChoosedButton";
	
	void OnEnable()
	{
		if (target == null)
			target = GetComponentInChildren<UISprite> ();
	}
	
	void Start ()
	{		
		if (Auto)
			Set (Startstate);
	}
	
	void Set (bool state)
	{
		if (target != null) {
			bActice = state;
			if (bActice && user_data_recvs != null && user_data_recvs.Length > 0) {
				NGUITools.SendAll (user_data_recvs, user_data_send_name, user_data, SendMessageOptions.DontRequireReceiver);
			}
			TweenScale.Begin(target.gameObject,0.1f,bActice?activeScale:unactiveScale).method = UITweener.Method.EaseInOut;
			if (option && bActice == true) {
				UIRadioScaleButton[] rbs = transform.parent.GetComponentsInChildren<UIRadioScaleButton> ();
				foreach (UIRadioScaleButton rb in rbs)
					if (rb != this)
						rb.Set (false);
			}			
		}
	}	
	void OnReset ()
	{
		Set (Startstate);
	}
	
	void OnChooseButton ()
	{
		Set (true);		
	}
	
	void OnClick ()
	{
		if (bActice == false) {
			if (ClickForbidOnDataEmpty == false || user_data != null)
				Set (true);
		}
	}
	
	
}
