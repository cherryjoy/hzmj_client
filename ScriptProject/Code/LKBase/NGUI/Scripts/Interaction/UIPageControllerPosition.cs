using UnityEngine;
using System.Collections;

public class UIPageControllerPosition : MonoBehaviour {
	public UIPageController PageController;
	public int PageSize;
	
	void OnEnable()
	{
		float iLocalPositonX = Mathf.Abs(gameObject.transform.localPosition.x);
		
		for( int k = 0 ; k < PageController.pages.Length; k++)
		{
			PageController.pages[k].SetActive(false);
			
			int index = (int)(iLocalPositonX / PageSize);
			
			if(index == k){
				PageController.pages[k].SetActive(true);
			}
		}
	}
}
