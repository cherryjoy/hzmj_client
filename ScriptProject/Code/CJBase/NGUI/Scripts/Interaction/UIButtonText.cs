using UnityEngine;
using System.Collections;

[ExecuteInEditMode]
[AddComponentMenu("NGUI/UI/Button Text Easy Motify")]
public class UIButtonText : MonoBehaviour
{
	public UILabel lable;
	
	void OnEnable()
	{
		if(lable == null){
			lable = GetComponentInChildren<UILabel>();
		}
	}
	
	public void SetText(string text)
	{
		lable.text = text;
	}	
}
