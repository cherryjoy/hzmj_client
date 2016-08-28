using UnityEngine;
using System.Collections;

[AddComponentMenu("NGUI/Interaction/UIPageControllerEx")]
public class UIPageControllerEx : MonoBehaviour {
	public UIPanel ClipPanel;
	public GameObject[] pageBtns;
	public GameObject[] pages;
	public GameObject EventReceiver;
	public bool bVertical;
	private float lastPostion;
	private bool  moveStart = false;
	private int mCurPage = 0;
	public int CurPage
	{
		get{return mCurPage;}
	}

    public delegate void DelegeteTurnToPage(int page);
    public DelegeteTurnToPage TurnToPageInvoker;
    public event DelegeteTurnToPage TurnToPage
    {
        add { TurnToPageInvoker += value; }
        remove { TurnToPageInvoker -= value; }
    }

    public delegate void DelegeteImmediatelyTurnToPage(int page);
    public DelegeteImmediatelyTurnToPage ImmediatelyTurnToPageInvoker;
    public event DelegeteImmediatelyTurnToPage ImmediatelyTurnToPage
    {
        add { ImmediatelyTurnToPageInvoker += value; }
        remove { ImmediatelyTurnToPageInvoker -= value; }
    }
	
	void OnDestroy()
	{
		pageBtns = null;
		pages = null;
		Clear();
	}
	
	void Clear()
	{
		moveStart = false;
		mCurPage = 0;
		lastPostion = 0f;
	}
	
	public GameObject[] GetAllPages()
	{
		return pages;
	}
	
	public int GetPagesCount()
	{
		return pages.Length;
	}
	
	void OnEnable()
	{
		Clear();
		SetPageInformation();
		SimulatorTurnPageImmediately(mCurPage);
	}
	
	void SetPageInformation()
	{
		if(pages != null && pages.Length > 0)
		{
			for(int i=0;i < pages.Length;i++)
			{
				UIDragPageEx drag = pages[i].GetComponent<UIDragPageEx>();
				if(drag != null)
				{
					drag.SetPageInformation(i,bVertical,OnTurnPage,OnDragLastMove);
				}
			}
		}
	}
	
	public void SetPageInformation(int perSize)
	{
		if(pages != null && pages.Length > 0)
		{
			for(int i=0;i < pages.Length;i++)
			{
				UIDragPageEx drag = pages[i].GetComponent<UIDragPageEx>();
				if(drag != null)
				{
					drag.SetPageInformation(i,this,bVertical,perSize,pages.Length,OnTurnPage,OnDragLastMove);
				}
			}
		}
	}
	
	void OnTurnPage(int page)
	{
		if (pageBtns != null && page >= 0 && page < pageBtns.Length) {
			pageBtns [page].SendMessage ("OnChooseButton", SendMessageOptions.DontRequireReceiver);
			mCurPage = page;
		}else if( page >= 0 && page < pages.Length){
			mCurPage = page;
		}
	}
	
	void OnDragLastMove(float dis,bool bMove)
	{
		lastPostion = dis;
		moveStart = bMove;
	}
	
	public void SimulatorOnClickTurnPage (int page)
	{
		if(TurnToPageInvoker != null)
		{
            TurnToPageInvoker(page);
		}
		if (pages != null && pageBtns != null && page >= 0 && page < pageBtns.Length) {
			pageBtns [page].SendMessage ("OnClick", SendMessageOptions.DontRequireReceiver);
		}
	}
	
	public void SimulatorTurnPageImmediately(int page)
	{
        if (ImmediatelyTurnToPageInvoker != null)
            ImmediatelyTurnToPageInvoker(page);
	}
	
	void LateUpdate ()
	{
		if (moveStart) {
			SpringPosition sp = GetComponent<SpringPosition> ();
			if (sp != null && sp.enabled == false) {
				Vector3 localPostion = transform.localPosition;
				if(bVertical) localPostion.y = lastPostion;
				else localPostion.x = lastPostion;
				transform.localPosition = localPostion;
				moveStart = false;
				LeavePage();
			}
		}
	}
	
	public void LeavePage()
	{
		if(pages != null)
		{
			for(int i =0;i < pages.Length;i++)
			{
				if(pages[i].activeSelf && i != mCurPage){
					pages[i].SetActive(false);	
				}
			}
			if(pages[mCurPage].activeSelf && EventReceiver != null)
				EventReceiver.SendMessage("OnTurnPage",mCurPage,SendMessageOptions.DontRequireReceiver);
		}
	}
	
	public void JugePageBeVisible(GameObject go,int pageSize)
	{
		if(ClipPanel != null)
		{
			float distance = 0f;
			if(bVertical) distance = Mathf.Abs(ClipPanel.transform.InverseTransformPoint(go.transform.position).y) - pageSize/2 - ClipPanel.clipRange.w/2;
			else distance = Mathf.Abs(ClipPanel.transform.InverseTransformPoint(go.transform.position).x) - pageSize/2 - ClipPanel.clipRange.z/2;
			if( distance <= 0f ){
				if(!go.activeSelf) go.SetActive(true);
			}
			else{
				if(go.activeSelf) go.SetActive(false);
			}
		}
	}
}
