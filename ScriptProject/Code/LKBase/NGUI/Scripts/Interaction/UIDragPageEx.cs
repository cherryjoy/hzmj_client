using UnityEngine;
using System.Collections;

[AddComponentMenu("NGUI/Interaction/DragPageEx")]
public class UIDragPageEx : MonoBehaviour {
	public float startPos;
    public int pageXLength;
	public int pageYLength;
    public int pageCount;
	public bool mLoop;
	public UIPageControllerEx mPageContro;
	public Vector3 scale = Vector3.one;
	private bool bVertical;
    private Plane mPlane;
    private Vector3 mLastPos;
    private float lastEdge;
    private float nextEdge;
    private float lastLocalPostion;
    private float pressLastTime;
	
	private int mPageIndex = -1;
	
	public delegate void OnDragToPage(int page);
	private OnDragToPage DragToPage;
	
	public delegate void OnUpdateDragPos(float dis,bool bMove);
	private OnUpdateDragPos UpdateDragPos;
	
	void OnDestroy()
	{
		ClearListen();
	}
	
	void ClearListen()
	{
		mPageContro.TurnToPage -= this.OnTurnToPage;
		mPageContro.ImmediatelyTurnToPage -= this.OnImmediatelyTurnToPage;
	}
	
	/// <summary>
	/// this function be Used to the Instantiated page.
	/// </summary>
	public void SetPageInformation(int index,UIPageControllerEx contro,bool bVer,int perPageSize,int totlePage,OnDragToPage handle0,OnUpdateDragPos handle1)
	{
		mPageContro = contro;
		if(bVer) pageYLength = perPageSize;
		else pageXLength = perPageSize;
		pageCount = totlePage;
		SetPageInformation(index,bVer,handle0,handle1);
	}
	
	public void SetPageInformation(int index,bool bVer,OnDragToPage handle0,OnUpdateDragPos handle1)
	{
		if(mPageContro != null && mPageIndex == -1)
		{
			mPageContro.TurnToPage += this.OnTurnToPage;
			mPageContro.ImmediatelyTurnToPage += this.OnImmediatelyTurnToPage;
		}
		mPageIndex = index;
		bVertical = bVer;
		DragToPage = handle0;
		UpdateDragPos = handle1;
		SetEdge();
	}
	
	private void SetEdge()
	{
		if(bVertical)
		{
			lastEdge = pageYLength * (pageCount -1) - startPos;
			nextEdge = startPos;
		}
		else
		{
			lastEdge = startPos - pageXLength * (pageCount -1);
			nextEdge = startPos;
		}
	}
	
	void OnTurnToPage(int page)
    {
        if (page == mPageIndex)
        {
            stopTarget();
            Vector3 localPostion = mPageContro.transform.localPosition;
			if(bVertical) localPostion.y = pageYLength * page - startPos;
			else localPostion.x = startPos - pageXLength * page;
            moveTarget(localPostion);
			gameObject.SetActive(false);
			gameObject.SetActive(true);
			if(DragToPage != null)
				DragToPage(mPageIndex);
        }
    }
	
	void OnImmediatelyTurnToPage(int page)
    {
        if (page == mPageIndex)
        {
            stopTarget();
            Vector3 localPostion = mPageContro.transform.localPosition;
			if(bVertical) localPostion.y = GetDestY(page);
			else localPostion.x = GetDestX(page);
            mPageContro.transform.localPosition = localPostion;
			gameObject.SetActive(false);
			gameObject.SetActive(true);
			if(DragToPage != null)
				DragToPage(mPageIndex);
			mPageContro.LeavePage();
        }
    }
	
	private void stopTarget()
    {
	    SpringPosition sp = mPageContro.GetComponent<SpringPosition>();
	    if (sp != null)
	        sp.enabled = false;
		if(UpdateDragPos != null)
	   		UpdateDragPos(0f,false);
    }
	
	private void moveTarget(Vector3 dest)
    {
        SpringPosition sp = SpringPosition.Begin(mPageContro.gameObject, dest, 5f);
        sp.ignoreTimeScale = true;
        sp.worldSpace = false;
		if(UpdateDragPos != null)
			UpdateDragPos(bVertical ? dest.y : dest.x,true);
    }
	
	private float GetDestX(int page)
    {
        return startPos - pageXLength * page;
    }
	
	private float GetDestY(int page)
	{
		return pageYLength * page - startPos;
	}
	
	void Update()
    {

        if (mLoop && pageCount >= 2)
        {
			if(bVertical)
			{
				if(pageYLength <= 0) return;
			}
			else
			{
				if(pageXLength <= 0) return;
			}
            GameObject[] pages = mPageContro.GetAllPages();
			float pageX = pages[0].transform.localPosition.x;
            float pageY = pages[0].transform.localPosition.y;
            float pageZ = pages[0].transform.localPosition.z;
			
			if(bVertical)
			{
				pages[0].transform.localPosition = new Vector3(pageX, 0, pageZ);
	            pages[pageCount - 1].transform.localPosition = new Vector3(pageX, pageYLength * (pageCount -1), pageZ);
	
	            Vector3 localPostion = mPageContro.transform.localPosition;
	
	            if ((localPostion.y < pageYLength) && (localPostion.y > 0))
	            {
	                pages[pageCount - 1].transform.localPosition = new Vector3(pageX, -pageYLength, pageZ);
	            }
	
	
	            float fPageToalSize = -pageYLength * (pageCount - 1);
	
	            if (localPostion.y <= fPageToalSize)
	            {
	                pages[0].transform.localPosition = new Vector3(pageX, -pageYLength * pageCount, pageZ);
	            }
			}
			else
			{
				pages[0].transform.localPosition = new Vector3(0, pageY, pageZ);
	            pages[pageCount - 1].transform.localPosition = new Vector3(pageXLength * (pageCount - 1), pageY, pageZ);
	
	            Vector3 localPostion = mPageContro.transform.localPosition;
	
	            if ((localPostion.x < pageXLength) && (localPostion.x > 0))
	            {
	                pages[pageCount - 1].transform.localPosition = new Vector3(-pageXLength, pageY, pageZ);
	            }
	
	
	            float fPageToalSize = -pageXLength * (pageCount - 1);
	
	            if (localPostion.x <= fPageToalSize)
	            {
	                pages[0].transform.localPosition = new Vector3(pageXLength * pageCount, pageY, pageZ);
	            }
			}
		}
    }
	
	void OnPress(bool pressed)
	{
        if (enabled && gameObject.activeSelf && mPageContro != null && pageCount >= 2)
        {
			if(bVertical){
				if(pageYLength <= 0) return;
			}
			else{
				if(pageXLength <= 0) return;
			}
			GameObject[] pages = mPageContro.GetAllPages();
			float pageX = pages[0].transform.localPosition.x;
	        float pageY = pages[0].transform.localPosition.y;
	        float pageZ = pages[0].transform.localPosition.z;
	
	        if (mLoop)
	        {
				if(bVertical){
					pages[0].transform.localPosition = new Vector3(pageX, 0, pageZ);
	           	 	pages[pageCount-1].transform.localPosition = new Vector3(pageX, -pageYLength * (pageCount -1), pageZ);
				}
				else{
					pages[0].transform.localPosition = new Vector3(0, pageY, pageZ);
	            	pages[pageCount-1].transform.localPosition = new Vector3(pageXLength * (pageCount - 1), pageY, pageZ);	
				}
	        }
            if (pressed)
            {
                stopTarget();
                // Remember the hit position
                mLastPos = UICamera.lastHit.point;
                pressLastTime = Time.timeSinceLevelLoad;
                lastLocalPostion = bVertical ? mPageContro.transform.localPosition.y : mPageContro.transform.localPosition.x;
               
                // Create the plane to drag along
                Transform trans = UICamera.lastCamera.transform;
                mPlane = new Plane(trans.rotation * Vector3.back, mLastPos);
            }
            else
            {
                Vector3 localPostion =  mPageContro.transform.localPosition;
                int dstPage = 0;
                if ((!bVertical && localPostion.x <= lastEdge) || (bVertical && localPostion.y >= lastEdge))
                {
                    //Turn right or up
					if(bVertical) localPostion.y = lastEdge;
					else localPostion.x = lastEdge;
                    dstPage = pageCount - 1;

                    if (mLoop)
                    {

                        float TransSincePress = bVertical ? (mPageContro.transform.localPosition.y - lastLocalPostion) : (mPageContro.transform.localPosition.x - lastLocalPostion);
                        float timeSincePress = Time.timeSinceLevelLoad - pressLastTime;
                        float Speed = TransSincePress / timeSincePress;

                        if (Mathf.Abs(Speed) > 200)
                        {

                            float offset = bVertical ? (mPageContro.transform.localPosition.y + pageYLength * (pageCount - 1)) : (mPageContro.transform.localPosition.x + pageXLength * (pageCount - 1));
                            dstPage = 0;
							if(bVertical){
								localPostion.y = GetDestY(dstPage);
								float x = mPageContro.transform.localPosition.x;
                            	float z = mPageContro.transform.localPosition.z;
                            	mPageContro.transform.localPosition = new Vector3(x, localPostion.y - pageYLength - offset, z);
                            	pages[pageCount - 1].transform.localPosition = new Vector3(pageX, pageYLength, pageZ);
							}
							else{
							 	localPostion.x = GetDestX(dstPage);
							 	float y = mPageContro.transform.localPosition.y;
                            	float z = mPageContro.transform.localPosition.z;
                            	mPageContro.transform.localPosition = new Vector3(localPostion.x + pageXLength + offset, y, z);
                            	pages[pageCount - 1].transform.localPosition = new Vector3(-pageXLength, pageY, pageZ);
							}
                        }
                    }

                }
                else if ((!bVertical && localPostion.x >= nextEdge) || (bVertical && localPostion.y <= nextEdge))
                {
                    //Turn Left or down
					if(bVertical) localPostion.y = nextEdge;
					else localPostion.x = nextEdge;
                    dstPage = 0;

                    if (mLoop)
                    {

                        float TransSincePress = mPageContro.transform.localPosition.x - lastLocalPostion;
                        float timeSincePress = Time.timeSinceLevelLoad - pressLastTime;
                        float Speed = TransSincePress / timeSincePress;

                        if (Mathf.Abs(Speed) > 200)
                        {
                            float offset = bVertical ? mPageContro.transform.localPosition.y : mPageContro.transform.localPosition.x;
                            dstPage = pageCount - 1;
							if(bVertical){
								localPostion.y = GetDestY(dstPage);
	                            float x = mPageContro.transform.localPosition.x;
	                            float z = mPageContro.transform.localPosition.z;
	                            mPageContro.transform.localPosition = new Vector3(x, localPostion.y + pageYLength - offset, z);
	                            pages[0].transform.localPosition = new Vector3(pageX, -pageCount * pageYLength, pageZ);
							}
							else
							{
								localPostion.x = GetDestX(dstPage);
	                            float y = mPageContro.transform.localPosition.y;
	                            float z = mPageContro.transform.localPosition.z;
	                            mPageContro.transform.localPosition = new Vector3(localPostion.x - pageXLength + offset, y, z);
	                            pages[0].transform.localPosition = new Vector3(pageXLength * pageCount, pageY, pageZ);
							}
                        }
                    }
                }
                else
                {
                    float TransSincePress = bVertical ? localPostion.y - lastLocalPostion : localPostion.x - lastLocalPostion;
                    float timeSincePress = Time.timeSinceLevelLoad - pressLastTime;
                    float Speed = TransSincePress / timeSincePress;

                    float trans = bVertical ? (startPos - localPostion.y) / pageYLength : (localPostion.x - startPos) / pageXLength;
                    if (Mathf.Abs(Speed) > 200)
                    {
                        if (Speed > 0)
                        {
							if(bVertical) trans = Mathf.Floor(trans);
							else trans = Mathf.Ceil(trans);
                        }
                        else
                        {
							if(bVertical) trans = Mathf.Ceil(trans);
                            else trans = Mathf.Floor(trans);
                        }
						int dirPage = -Mathf.RoundToInt(trans);
						if(dirPage >=0 && dirPage < pageCount && !pages[dirPage].activeSelf)
							pages[dirPage].SetActive(true);
                    }
                    int turnPage = Mathf.RoundToInt(trans);
                    dstPage = -turnPage;
					if(bVertical) localPostion.y = GetDestY(dstPage);
					else localPostion.x = GetDestX(dstPage);
                }
				if(DragToPage != null)
					DragToPage(dstPage);
                moveTarget(localPostion);
            }
        }
    }
	
	 void OnDrag(Vector2 delta)
    {
        if (enabled && gameObject.activeSelf && mPageContro != null && pageCount >= 2)
        {
			if(bVertical){
				if(pageYLength <= 0) return;
			}
			else{
				if(pageXLength <= 0) return;
			}
            Ray ray = UICamera.lastCamera.ScreenPointToRay(UICamera.lastTouchPosition);
            float dist = 0f;
            if (mPlane.Raycast(ray, out dist))
            {
                Vector3 currentPos = ray.GetPoint(dist);
                Vector3 offset = currentPos - mLastPos;
                mLastPos = currentPos;
				
                if (offset.x != 0f || offset.y != 0f)
                {
                    offset = mPageContro.transform.InverseTransformDirection(offset);
                    offset.Scale(scale);
                    offset = mPageContro.transform.TransformDirection(offset);
                }
                // Adjust the position
                mPageContro.transform.position += offset;
				SetLeftRightPageBVisible(mPageIndex +1);
				SetLeftRightPageBVisible(mPageIndex -1);
            }
        }
    }
	
	private void SetLeftRightPageBVisible(int page)
	{
		GameObject[] pages = mPageContro.GetAllPages();
		if(page >=0 && page < pageCount) 
			mPageContro.JugePageBeVisible(pages[page],bVertical ? pageYLength:pageXLength);
	}
}
