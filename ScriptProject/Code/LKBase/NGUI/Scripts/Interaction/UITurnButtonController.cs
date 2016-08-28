using UnityEngine;
using System.Collections;

[AddComponentMenu("NGUI/Interaction/UITurnButtonController")]
public class UITurnButtonController : MonoBehaviour
{	
	public int pageBtnID = 0;
	public UIPageController ctrl = null;
		
	void OnClick ()
	{
		if (enabled && ctrl) {
			ctrl.SendMessage("OnTurnToPage",pageBtnID,SendMessageOptions.DontRequireReceiver);
		}
	}
}
