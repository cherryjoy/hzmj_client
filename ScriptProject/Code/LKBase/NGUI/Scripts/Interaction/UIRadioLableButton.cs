using UnityEngine;
using System.Collections;

[ExecuteInEditMode]
[AddComponentMenu("NGUI/UI/Radio Lable Button")]
public class UIRadioLableButton : MonoBehaviour
{	
	public UILabel lable;
	public Color ActiveColor;
	public Color UnactiveColor;
	public bool StartActive = false;
	public bool Auto = true;
	public bool option = true;
	public object userData = 0;
	public GameObject[] userDataRecv_Order;	
	private bool m_bSetActive = false;
	public string userDataSendMessage = "OnChoosedButton";
	// Use this for initialization	
	void OnEnable()
	{
		if (lable == null)
			lable = GetComponentInChildren<UILabel> ();
	}
	void Start ()
	{		
		if (Auto)
			Set (StartActive);
	}
	
	public void Set (bool active)
	{
		if (lable != null) {
			m_bSetActive = active;
			lable.color = m_bSetActive ? ActiveColor : UnactiveColor;
			if (userDataRecv_Order != null && userDataRecv_Order.Length > 0 && m_bSetActive) {
				NGUITools.SendAll (userDataRecv_Order, userDataSendMessage, userData, SendMessageOptions.DontRequireReceiver);
			}
			if (option && m_bSetActive) {
				UIRadioLableButton[] rbs = transform.parent.GetComponentsInChildren<UIRadioLableButton> ();
				foreach (UIRadioLableButton rb in rbs)
					if (rb != this)
						rb.Set (false);
			}
		}
	}
	
	void OnChooseButton ()
	{
		Set (true);
	}
	
	void OnReset ()
	{
		Set (StartActive);	
	}
	
	void OnClick ()
	{
		if (m_bSetActive == false)
			Set (true);
	}
}
