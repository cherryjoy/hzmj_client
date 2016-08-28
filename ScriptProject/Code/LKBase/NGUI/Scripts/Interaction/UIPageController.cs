using UnityEngine;
using System.Collections;

[AddComponentMenu("NGUI/Interaction/UIPageController")]
public class UIPageController : MonoBehaviour
{	
	public GameObject[] pageBtns;
	public GameObject[] pages;
	public GameObject pageCell0;
	private float lastPostionX;
	private bool  moveStart = false;
	int page = 0;
	private int lastPage
	{
		get
		{
			return page;
		}
		set
		{
			page = value;
		}
	}
	
	public GameObject EventReceiver;
	private Vector3 mInitPos = Vector3.zero;

	void clear ()
	{
		lastPostionX = 0f;
		moveStart = false;
		lastPage = 0;	
	}
	
	public GameObject[] GetAllPages ()
	{
		return pages;
	}
	
	public void OnReset ()
	{
		pageBtns = null;
		pages = null;
		pageCell0 = null;
		clear ();
	}
	
	public int GetLastPage ()
	{
		return lastPage;
	}
	// be set page
	void OnTurnPage (int page)
	{
		if (pageBtns != null && page >= 0 && page < pageBtns.Length) {
			pageBtns [page].SendMessage ("OnChooseButton", SendMessageOptions.DontRequireReceiver);
			lastPage = page;
		}else if( page >= 0 && page < pages.Length){
			lastPage = page;
		}
	}
	
	void OnEnable ()
	{
		clear ();
		if (pageCell0 != null)
			pageCell0.SendMessage ("OnImmediatelyTurnToPage", lastPage, SendMessageOptions.DontRequireReceiver);		
		if (pageBtns != null && lastPage >= 0 && lastPage < pageBtns.Length) {
			pageBtns [lastPage].SetActive (true);
			if(EventReceiver != null) EventReceiver.SendMessage("OnTurnPage",lastPage,SendMessageOptions.DontRequireReceiver);
			pageBtns [lastPage].SendMessage ("OnChooseButton", SendMessageOptions.DontRequireReceiver);		
		}
		LeavePage ();
	}

	void OnDisable()
	{
		transform.localPosition = mInitPos;
	}
	
	// press btn
	void OnTurnToPage (int page)
	{
		if (pages != null && page >= 0 && page < pages.Length) {
			if( pages.Length < 5 )
				OpenAllPage();
			else
				OpenNextPage(page);
			//OpenAllPage ();
			if (pageCell0 != null)
				pageCell0.SendMessage ("OnTurnToPage", page, SendMessageOptions.DontRequireReceiver);
			lastPage = page;		
		}
	}
	
	// page Start LastMoveX
	void OnDragLastMoveX (DragXPostion xData)
	{
		moveStart = xData.moved;
		lastPostionX = xData.x;		
	}
	
	void LateUpdate ()
	{
		if (moveStart) {
			SpringPosition sp = GetComponent<SpringPosition> ();
			if (sp != null && sp.enabled == false) {
				Vector3 localPostion = transform.localPosition;
				localPostion.x = lastPostionX;
				transform.localPosition = localPostion;
				moveStart = false;
				LeavePage ();
				gameObject.SendMessage("OnTurnStop", lastPage, SendMessageOptions.DontRequireReceiver);
			}
		}
	}
	
	public void OpenAllPage ()
	{
		if (pages != null) {
			for (int i = 0; i < pages.Length; i++) {
				if (pages [i].activeSelf == false)
					pages [i].SetActive (true);
					
				if(EventReceiver != null) EventReceiver.SendMessage("OnTurnPage",lastPage,SendMessageOptions.DontRequireReceiver);
			}
		}
	}
	
	public void OpenNextPage(int page)    
	{
		if( pages != null )
		{
			if( page == 0 )
			{
				if( pages[0] != null && pages[0].activeSelf == false )
					pages[0].SetActive(true);
				if( pages[pages.Length-1] != null && pages[pages.Length-1].activeSelf == false )
					pages[pages.Length-1].SetActive(true);
			}
			if( pageCell0.activeSelf == false )
				pageCell0.SetActive(true);
			if( page-1 >= 0 )
				if( pages[page-1] != null && pages[page-1].activeSelf == false )
					pages[page-1].SetActive(true);
			if( page+1 <= pages.Length-1 )
				if( pages[page+1] != null && pages[page+1].activeSelf == false )
					pages[page+1].SetActive(true);
			if( pages[page] != null && pages[page].activeSelf == false )
				pages[page].SetActive(true);
			
			if(EventReceiver != null) EventReceiver.SendMessage("OnTurnPage",lastPage,SendMessageOptions.DontRequireReceiver);
		}
	}
	
	public void LeavePage ()
	{
		if (pages != null) {
			for (int i = 0; i < pages.Length; i++) {
				if (pages [i].activeSelf && i != lastPage) {
					pages [i].SetActive (false);
				} 
			}
			if (lastPage >= 0 && lastPage < pages.Length && pages [lastPage].activeSelf == false)
				pages [lastPage].SetActive (true);
				if(EventReceiver != null) EventReceiver.SendMessage("OnTurnPage",lastPage,SendMessageOptions.DontRequireReceiver);
		}
	}
	
	public void SimulatorOnClickTurnPage (int page)
	{
		if (pages != null && pageBtns != null && page >= 0 && page < pages.Length) {
			pageBtns [page].SendMessage ("OnClick", SendMessageOptions.DontRequireReceiver);
		}
	}
	
	public void SimulatorOnClickTurnPageWithoutButton(int page)
	{
		OnTurnToPage(page);
	}
	
	public void SimulatorOnClickTurnPageImmediately (int page)
	{
		if (pages != null && pageBtns != null && page >= 0 && page < pages.Length) {
			//OpenAllPage();
			if( pages.Length < 5)
				OpenAllPage();
			else
				OpenNextPage(page);
			if (pageCell0 != null)
				pageCell0.SendMessage ("OnImmediatelyTurnToPage", page, SendMessageOptions.DontRequireReceiver);
			if(page < pageBtns.Length)
				pageBtns [page].SendMessage ("OnChooseButton", SendMessageOptions.DontRequireReceiver);
			lastPage = page;
			LeavePage();
			gameObject.SendMessage("OnTurnStop", lastPage, SendMessageOptions.DontRequireReceiver);
		}		
	}
}
