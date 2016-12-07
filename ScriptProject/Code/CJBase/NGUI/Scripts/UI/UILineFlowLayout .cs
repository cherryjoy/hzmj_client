using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[System.Serializable]
public class UILineFlowLayoutChild
{
    public Transform tran;
    public bool isCenter;
    public int offset;
    public UILineFlowLayoutChild(Transform _tran, bool _center = false)
    {
        tran = _tran;
        isCenter = _center;
    }

    public UILineFlowLayoutChild()
    {

    }
}
[ExecuteInEditMode]
public class UILineFlowLayout : MonoBehaviour
{
    public enum LayoutUpdateMode
    {
        Update = 0,
        LateUpdate = 1,
    }

    private bool mIsNeedRerefresh = true;
    private Transform mTransform = null;

    [HideInInspector]
    public static int RefreshCount = 0;

    [HideInInspector]
    public int mRefreshCount = 0;

    public int mStyle = 0; //0:Horizontal 1:Vertical
    public int mAnchor = 0;//0:Center -1:left 1:right 2:Top -2:Bottom
    public LayoutUpdateMode updateMode = LayoutUpdateMode.LateUpdate;

    [SerializeField]
    private List<UILineFlowLayoutChild> m_childrens = new List<UILineFlowLayoutChild>();

    public List<UILineFlowLayoutChild> childrens
    {
        set { m_childrens = value; IsNeedRefresh = true; }
        get
        {
            return m_childrens;
        }
    }

    public float space = 5.0f;

    public int Style
    {
        get
        {
            return mStyle;
        }
        set
        {
            if (value != mStyle)
            {
                mStyle = value;
                mIsNeedRerefresh = true;

                if (mStyle == 1)
                {
                    if (mAnchor != -1 && mAnchor != 0 && mAnchor != 1)
                    {
                        mAnchor = 0;
                    }
                }
                else if (mStyle == 0)
                {
                    if (mAnchor != 2 && mAnchor != -2 && mAnchor !=0 )
                    {
                        mAnchor = 0;
                    }
                }
#if UNITY_EDITOR
                UnityEditor.EditorUtility.SetDirty(gameObject);

                LateUpdate();
#endif
            }
        }
    }

    public int Anchor
    {
        get
        {
            return mAnchor;
        }
        set
        {
            if (mStyle ==0)
            {
                if (value != 0 && value != 2 && value != -2)
                {
                    return;
                }
            }
            else if (mStyle == 1)
            {
                if (value != -1 && value != 1 && value != 0)
                {
                    return;
                }
            }
            else
            {
                return;
            }

            if (value != mAnchor)
            {
                mAnchor = value;
                mIsNeedRerefresh = true;
#if UNITY_EDITOR
                UnityEditor.EditorUtility.SetDirty(gameObject);
                LateUpdate();
#endif

            }
        }
    }

    internal class LineFlowLayoutData
    {
        internal UILineFlowLayoutChild child;
        internal Bounds originBounds;
        internal Bounds targetBounds;

        public LineFlowLayoutData(UILineFlowLayoutChild c, Bounds oriBounds)
        {
            child = c;
            originBounds = oriBounds;
            targetBounds = oriBounds;
        }
    }

    public bool IsNeedRefresh
    {
        set
        {
            mIsNeedRerefresh = value;

#if UNITY_EDITOR
			LateUpdate();		
        }
		get
		{
			return mIsNeedRerefresh;	
		}
#else
        }
#endif

    }

    void Awake()
    {
        mTransform = this.transform;
    }

    void Update()
    {
        if (updateMode == LayoutUpdateMode.Update)
        {
            CheckUpdate();
        }
    }

    void LateUpdate()
    {
        if (updateMode == LayoutUpdateMode.LateUpdate)
        {
            CheckUpdate();
        }
    }

    void CheckUpdate()
    {
        if (mRefreshCount > 0)
        {
            mIsNeedRerefresh = true;
            mRefreshCount--;
        }

        if (RefreshCount > 0)
        {
            mIsNeedRerefresh = true;
            RefreshCount--;
        }

        if (mIsNeedRerefresh)
        {
            mIsNeedRerefresh = false;
            RefreshNow();
        }
    }

    public void RefreshNow()
    {
       
        if (!IsChildrenWithSameParent())
        {
            return;
        }
        if (childrens.Count > 0)
        {
            UILineFlowLayoutChild centerChild = childrens[0];
            int centerChildIndex = 0;

            List<LineFlowLayoutData> boundsData = new List<LineFlowLayoutData>(childrens.Count);
            for (int i = 0, c = childrens.Count; i < c; i++)
            {
                UILineFlowLayoutChild child = childrens[i];

                if (!(child.tran.gameObject.activeSelf && child.tran.gameObject.activeInHierarchy))
                {
                    continue;
                }

                if (child.isCenter && (!centerChild.isCenter || !(centerChild.tran.gameObject.activeSelf && centerChild.tran.gameObject.activeInHierarchy)))
                {
                    centerChild = child;
                    centerChildIndex = boundsData.Count;

                }

                Bounds currentBounds = NGUIMath.CalculateRelativeWidgetBounds(mTransform, child.tran);
                boundsData.Add(new LineFlowLayoutData(child, currentBounds));
            }
           // Debug.Log(centerChildIndex);
            for (int i = centerChildIndex - 1; i >= 0; i--)
            {
                LineFlowLayoutData preData = boundsData[i + 1];
                LineFlowLayoutData curData = boundsData[i];

                curData.targetBounds = CalculateTargetBounds(preData, curData, false);
            }
            for (int i = centerChildIndex + 1, c = boundsData.Count; i < c; i++)
            {
                LineFlowLayoutData preData = boundsData[i - 1];
                LineFlowLayoutData curData = boundsData[i];

                curData.targetBounds = CalculateTargetBounds(preData, curData, true);
            }

            for (int i = 0, c = boundsData.Count; i < c; i++)
            {
                LineFlowLayoutData layoutData = boundsData[i];
                UILineFlowLayoutChild layoutChild = layoutData.child;

                Vector3 offset = layoutData.targetBounds.center - layoutData.originBounds.center;

                layoutChild.tran.localPosition += new Vector3(offset.x, offset.y + layoutData.child.offset, 0);

            }
        }
    }

    public void InsertChild(int index, Transform tran)
    {
        if (!NGUITools.IsChild(mTransform, tran))
        {
            Debug.LogWarning("Not Same Parent");
            return;
        }
        if (IsContainTran(tran))
        {
            Debug.LogWarning("Has Contained the tranform");
            return;
        }

        if (index > childrens.Count || index < 0)
        {
            index = childrens.Count;
        }
        UILineFlowLayoutChild child = new UILineFlowLayoutChild();
        child.tran = tran;
        child.isCenter = false;

        childrens.Insert(index, child);
        mIsNeedRerefresh = true;
    }

    public void AddChild(GameObject go, bool isCenter = false)
    {
        childrens.Add(new UILineFlowLayoutChild(go.transform, isCenter));

        mRefreshCount++;
    }

    public void AddChild(Component cop, bool isCenter = false)
    {
        if (cop == null)
        {
            return;
        }
        childrens.Add(new UILineFlowLayoutChild(cop.transform, isCenter));

    }

    public void DeleteChildAtIndex(int index)
    {
        if (index < 0 || index >= childrens.Count)
        {
            return;
        }

        childrens.RemoveAt(index);
        mIsNeedRerefresh = true;
    }

    public void DeleteChild(Transform tran)
    {
        for (int i = childrens.Count - 1; i >= 0; i--)
        {
            UILineFlowLayoutChild child = childrens[i];
            if (child.tran == tran)
            {
                childrens.Remove(child);

                mIsNeedRerefresh = true;
                return;
            }
        }
    }

    public void RemoveAll()
    {
        childrens.Clear();
    }

    private Bounds CalculateTargetBounds(LineFlowLayoutData preData, LineFlowLayoutData curData, bool isNext)
    {
        Vector3 offset = Vector3.zero;
        int anchorOffset = 0;
        if (mAnchor == 2 || mAnchor == 1)
        {
            anchorOffset = -1;
        }
        else if (mAnchor == -2 || mAnchor == -1)
        {
            anchorOffset = 1;
        }

        if (mStyle == 0)
        {
            offset = new Vector3(((preData.originBounds.size.x + curData.originBounds.size.x) * 0.5f + space) * (isNext ? 1 : -1), (curData.originBounds.size.y - preData.originBounds.size.y) * 0.5f * anchorOffset, 0f);
        }
        else
        {
            offset = new Vector3((curData.originBounds.size.x - preData.originBounds.size.x) * 0.5f * anchorOffset, ((preData.originBounds.size.y + curData.originBounds.size.y) * 0.5f + space) * (isNext ? -1 : 1), 0f);
        }

        return new Bounds(preData.targetBounds.center + offset, curData.originBounds.size);
    }

#if UNITY_EDITOR
	public bool IsChildrenWithSameParent()
#else
    private bool IsChildrenWithSameParent()
#endif
    {
        if (childrens.Count > 0)
        {
            for (int i = 0, c = childrens.Count; i < c; i++)
            {
                Transform tran = childrens[i].tran;
                if (tran == null || (tran.gameObject.activeSelf && tran.parent != mTransform))
                {
                    return false;
                }
            }
        }

        return true;
    }

    private bool IsContainTran(Transform tran)
    {
        for (int i = 0, c = childrens.Count; i < c; i++)
        {
            UILineFlowLayoutChild child = childrens[i];
            if (child.tran == tran)
            {
                return true;
            }
        }

        return false;
    }
}
