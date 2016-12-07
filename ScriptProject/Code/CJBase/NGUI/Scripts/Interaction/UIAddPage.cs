using UnityEngine;
using System;
using System.Collections;

public class UIAddPage
{	
	private static GameObject[] addGameObject (GameObject[] src, GameObject target)
	{
		GameObject[] newArray = null;
		if (src != null) {
			newArray = new GameObject[src.Length + 1];
			src.CopyTo (newArray, 0);
		} else {
			newArray = new GameObject[1];
		}
		newArray [newArray.Length - 1] = target;
		return newArray;
	}
	/// <summary>
	/// 首先添加一个页的prefeb到某个页根节点下
	/// </summary>
	private static GameObject AddPageStep1 (UIPageController pageControl, GameObject pagePrefeb, int pageSize)
	{		
		int currentPageSize = pageControl.pages != null ? pageControl.pages.Length : 0;
		GameObject page = (GameObject)GameObject.Instantiate (pagePrefeb);
		page.transform.parent = pageControl.gameObject.transform;
		Vector3 scale = page.transform.localScale;
		scale.Set (1.0f, 1.0f, 1.0f);
		page.transform.localScale = scale;
		page.transform.localPosition = new Vector3 (pageSize * currentPageSize, 0.0f, 0.0f);		
		pageControl.pages = addGameObject (pageControl.pages, page);
		if (pageControl.pageCell0 == null) {
			pageControl.pageCell0 = page;
		}
		return page;
	}
	
	/// <summary>
	/// 添加一个翻页的显示按钮，本步骤为可选的
	/// </summary>
	private static GameObject AddPageStep2_Optional (UIPageController pageControl, Transform turnRoot, GameObject turnButtonPrefeb)
	{
		GameObject turnButton = (GameObject)GameObject.Instantiate (turnButtonPrefeb);
		turnButton.transform.parent = turnRoot;
		Vector3 scale = turnButton.transform.localScale;
		scale.Set (1.0f, 1.0f, 1.0f);
		turnButton.transform.localScale = scale;				
		UITurnButtonController turnContoller = turnButton.GetComponent<UITurnButtonController> ();
		turnContoller.pageBtnID = pageControl.pageBtns != null ? pageControl.pageBtns.Length : 0;
		turnContoller.ctrl = pageControl;
		pageControl.pageBtns = addGameObject (pageControl.pageBtns, turnButton);
		turnButton.SendMessage ("OnReset", SendMessageOptions.DontRequireReceiver);
		return turnButton;
	}
	
	private static void LastFix1 (UIPageController pageControl, int pageSize)
	{
		UIDragXPage[] childDrags = pageControl.gameObject.GetComponentsInChildren<UIDragXPage> ();
		foreach (UIDragXPage childDrag in childDrags) {
			childDrag.SetPageInformation (pageControl.transform, pageSize, pageControl.pages.Length);
		}
	}
	
	private static void LastFix2 (UIPageController pageControl, float turnButtonSpace)
	{
		float startX = - (pageControl.pageBtns.Length - 1) / 2.0f * turnButtonSpace;
		foreach (GameObject turn in pageControl.pageBtns) {
			turn.transform.localPosition = new Vector3 (startX, 0.0f, 0.0f);
			startX += turnButtonSpace;
		}
	}
	
	private static void LastFix2_Left (UIPageController pageControl, float turnButtonSpace)
	{
		for (int i = 0; i < pageControl.pageBtns.Length; i++) {
			pageControl.pageBtns [i].transform.localPosition = new Vector3 (turnButtonSpace * i, 0.0f, 0.0f);
		}
	}
	
	public static GameObject AddOnePage (UIPageController pageControl, GameObject pagePrefeb, int pageSize)
	{
		pageControl.OpenAllPage ();
		GameObject page = AddPageStep1 (pageControl, pagePrefeb, pageSize);
		LastFix1 (pageControl, pageSize);
		pageControl.LeavePage ();
		return page;
	}
	
	public static GameObject AddOnePage (UIPageController pageControl, GameObject pagePrefeb, GameObject turnPrefeb, Transform turnRoot, int pageSize, float turnButtonSpace)
	{
		pageControl.OpenAllPage ();
		GameObject page = AddPageStep1 (pageControl, pagePrefeb, pageSize);
		AddPageStep2_Optional (pageControl, turnRoot, turnPrefeb);
		LastFix1 (pageControl, pageSize);
		LastFix2 (pageControl, turnButtonSpace);
		pageControl.LeavePage ();
		return page;
	}
	
	public static void AddOnePage2 (UIPageController pageControl, GameObject pagePrefeb, GameObject turnPrefeb, Transform turnRoot, int pageSize, float turnButtonSpace, out GameObject madepage, out GameObject madeTurn)
	{
		pageControl.OpenAllPage ();
		madepage = AddPageStep1 (pageControl, pagePrefeb, pageSize);
		madeTurn = AddPageStep2_Optional (pageControl, turnRoot, turnPrefeb);
		LastFix1 (pageControl, pageSize);
		LastFix2_Left (pageControl, turnButtonSpace);
		pageControl.LeavePage ();
	}
	
	public static GameObject AddOnePageEx (UIPageControllerEx pageControl, GameObject pagePrefeb)
	{
		GameObject page = AddPageExStep1(pageControl, pagePrefeb);
		pageControl.SetPageInformation(pageControl.bVertical ? (int)pageControl.ClipPanel.clipRange.w : (int)pageControl.ClipPanel.clipRange.z);
		return page;
	}
	
	public static GameObject AddOnePageEx (UIPageControllerEx pageControl, GameObject pagePrefeb,int pageSize)
	{
		GameObject page = AddPageExStep1(pageControl, pagePrefeb,pageSize);
		pageControl.SetPageInformation(pageSize);
		return page;
	}
	
	private static GameObject AddPageExStep1(UIPageControllerEx pageControl, GameObject pagePrefeb)
	{		
		int currentPageSize = pageControl.pages != null ? pageControl.pages.Length : 0;
		GameObject page = (GameObject)GameObject.Instantiate (pagePrefeb);
		page.transform.parent = pageControl.gameObject.transform;
		Vector3 scale = page.transform.localScale;
		scale.Set (1.0f, 1.0f, 1.0f);
		page.transform.localScale = scale;
		int pageSize = pageControl.bVertical ? (int)pageControl.ClipPanel.clipRange.w : (int)pageControl.ClipPanel.clipRange.z;
		page.transform.localPosition = pageControl.bVertical ? new Vector3 (0.0f,-pageSize * currentPageSize, 0.0f) : new Vector3 (pageSize * currentPageSize,0.0f, 0.0f);		
		pageControl.pages = addGameObject (pageControl.pages, page);
		
		return page;
	}
	
	private static GameObject AddPageExStep1(UIPageControllerEx pageControl, GameObject pagePrefeb,int pageSize)
	{		
		int currentPageSize = pageControl.pages != null ? pageControl.pages.Length : 0;
		GameObject page = (GameObject)GameObject.Instantiate (pagePrefeb);
		page.transform.parent = pageControl.gameObject.transform;
		Vector3 scale = page.transform.localScale;
		scale.Set (1.0f, 1.0f, 1.0f);
		page.transform.localScale = scale;
		page.transform.localPosition = pageControl.bVertical ? new Vector3 (0.0f,-pageSize * currentPageSize, 0.0f) : new Vector3 (pageSize * currentPageSize,0.0f, 0.0f);		
		pageControl.pages = addGameObject (pageControl.pages, page);
		
		return page;
	}
	
}
