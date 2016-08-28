//----------------------------------------------
//            NGUI: Next-Gen UI kit
// Copyright © 2011-2012 Tasharen Entertainment
//----------------------------------------------

using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// UI Panel is responsible for collecting, sorting and updating widgets in addition to generating widgets' geometry.
/// </summary>

[ExecuteInEditMode]
[AddComponentMenu("NGUI/UI/Panel")]
unsafe public class UIPanel : MonoBehaviour
{
	public enum DebugInfo
	{
		None,
		Gizmos,
		Geometry,
	}

    public enum PanelDepthFlag
    {
        None,
        Normal,
        Bottom,
        Top,
    }

	/// <summary>
	/// Whether this panel will show up in the panel tool (set this to 'false' for dynamically created temporary panels)
	/// </summary>
	
	public bool showProgress = true;
	public bool showInPanelTool = true;

	/// <summary>
	/// Whether the panel will create an additional pass to write to depth.
	/// Turning this on will double the number of draw calls, but will reduce fillrate.
	/// In order to make the most out of this feature, move your widgets on the Z and minimize the amount of visible transparency.
	/// </summary>

	public bool depthPass = false;
	//public UIOrder order;

	// Whether generated geometry is shown or hidden
	[SerializeField] DebugInfo mDebugInfo = DebugInfo.None;

	// Clipping rectangle
	[SerializeField] UIDrawCall.Clipping mClipping = UIDrawCall.Clipping.None;
	[SerializeField] Vector4 mClipRange = Vector4.zero;
	[SerializeField] Vector2 mClipSoftness = new Vector2(1f, 1f);
    [HideInInspector][SerializeField] public PanelDepthFlag mDepthFlag = PanelDepthFlag.None;

	// List of all widgets managed by this panel
	[System.NonSerialized]
	List<UIWidget> mWidgets = new List<UIWidget>();

	[System.NonSerialized]
	Dictionary<Transform, UIWidget> mWidgetDicts = new Dictionary<Transform, UIWidget>();

	// Widgets using these materials will be rebuilt next frame
	//List<Material> mChanged = new List<Material>();
	bool mtlChanged = false;

	// List of UI Screens created on hidden and invisible game objects
	List<UIDrawCall> mDrawCalls = new List<UIDrawCall>();
	
	// Cached in order to reduce memory allocations
    /*
	BetterList<Vector3> mVerts = new BetterList<Vector3>();
	BetterList<Vector2> mUvs = new BetterList<Vector2>();
	BetterList<Color> mCols = new BetterList<Color>();
    */
    Vector3[] mVerts = new Vector3[0];
    Vector2[] mUvs = new Vector2[0];
    Color[] mCols = new Color[0];

	Transform mTrans;
	Camera mCam;
	int mLayer = -1;
	bool mDepthChanged = false;
	bool mRebuildAll = false;

	float mMatrixTime = 0f;
	public Matrix4x4 mWorldToLocal = Matrix4x4.identity;

	// Values used for visibility checks
	static float[] mTemp = new float[4];
	Vector2 mMin = Vector2.zero;
	Vector2 mMax = Vector2.zero;

	// When traversing through the child dictionary, deleted values are stored here
	List<Transform> mRemoved = new List<Transform>();

    static readonly int topDepth = -600;
    static readonly int bottomDepth = 200;
    static readonly int normalDepth = -200;

    static List<UIPanel> mTopList = new List<UIPanel>();
    static List<UIPanel> mBottomList = new List<UIPanel>();
    static List<UIPanel> mNormalList = new List<UIPanel>();

#if UNITY_EDITOR
	// Screen size, saved for gizmos, since Screen.width and Screen.height returns the Scene view's dimensions in OnDrawGizmos.
	Vector2 mScreenSize = Vector2.one;
#endif

	/// <summary>
	/// Cached for speed.
	/// </summary>

    public Transform cachedTransform { get { if (mTrans == null) mTrans = transform; return mTrans; } }

    static public void AddToTop(UIPanel p)
    {
        RemoveFromBottom(p);
        RemoveFromNormal(p);
        if (mTopList.Contains(p) == false)
        {
            mTopList.Add(p);
        }
        else
        {
           int idx = mTopList.IndexOf(p);
           mTopList.RemoveAt(idx);
           mTopList.Add(p);
        }
        SortTopQueue();
    }

    static private void RemoveFromTop(UIPanel p)
    {
        mTopList.Remove(p);
    }

    static public void AddToBottom(UIPanel p)
    {
        RemoveFromNormal(p);
        RemoveFromTop(p);
        if (mBottomList.Contains(p) == false)
        {
            mBottomList.Add(p);
        }
        else
        {
            int idx = mBottomList.IndexOf(p);
            mBottomList.RemoveAt(idx);
            mBottomList.Add(p);
        }
        SortBottomQueue();
    }

    static private void RemoveFromBottom(UIPanel p)
    {
        mBottomList.Remove(p);
    }

    static public void AddToNormal(UIPanel p)
    {
        RemoveFromBottom(p);
        RemoveFromTop(p);
        if (mNormalList.Contains(p) == false)
        {
            mNormalList.Add(p);
        }
        else
        {
            int idx = mNormalList.IndexOf(p);
            mNormalList.RemoveAt(idx);
            mNormalList.Add(p);
        }
        SortNormalQueue();
    }

    static private void RemoveFromNormal(UIPanel p)
    {
        mNormalList.Remove(p);
    }

    static public void SortTopQueue()
    {
        int count = mTopList.Count;
        UIPanel p = mTopList[count - 1];
        Vector3 pos = p.transform.localPosition;
        pos.z = calculateBaseDepthInParents(p.gameObject, topDepth);        
        p.transform.localPosition = pos;

        for (int i = count - 2, j = 1; i >= 0; i--, j++)
        {
            p = mTopList[i];
            pos = p.transform.localPosition;          
            pos.z = calculateBaseDepthInParents(p.gameObject, topDepth + j);            
            p.transform.localPosition = pos;
        }
    }

    static public void SortBottomQueue()
    {
        int count = mBottomList.Count;
        UIPanel p = mBottomList[count - 1];
        Vector3 pos = p.transform.localPosition;
        pos.z = calculateBaseDepthInParents(p.gameObject, bottomDepth);
        p.transform.localPosition = pos;
        for (int i = count - 2, j = 1; i >= 0; i--, j++)
        {
            p = mBottomList[i];
            pos = p.transform.localPosition;
            pos.z = calculateBaseDepthInParents(p.gameObject, bottomDepth + j);
            p.transform.localPosition = pos;
        }
    }

    static public void SortNormalQueue()
    {
        int count = mNormalList.Count;
        UIPanel p = mNormalList[count - 1];
        Vector3 pos = p.transform.localPosition;
        pos.z = calculateBaseDepthInParents(p.gameObject, normalDepth);
        p.transform.localPosition = pos;
        for (int i = count - 2, j = 1; i >= 0; i--, j++)
        {
            p = mNormalList[i];
            pos = p.transform.localPosition;
            pos.z = calculateBaseDepthInParents(p.gameObject, topDepth + j);
            p.transform.localPosition = pos;
        }
    }

	/// <summary>
	/// Whether the panel's generated geometry will be hidden or not.
	/// </summary>

	public DebugInfo debugInfo
	{
		get
		{
			return mDebugInfo;
		}
		set
		{
			if (mDebugInfo != value)
			{
				mDebugInfo = value;
				List<UIDrawCall> list = drawCalls;
				HideFlags flags = (mDebugInfo == DebugInfo.Geometry) ? HideFlags.DontSave | HideFlags.NotEditable : HideFlags.HideAndDontSave;

				foreach (UIDrawCall dc in list)
				{
					GameObject go = dc.gameObject;
					go.SetActive(false);
					go.hideFlags = flags;
					go.SetActive(true);
				}
			}
		}
	}

    static public UIPanel findParentPanelWidthFlag(GameObject go, PanelDepthFlag flag)
    {
        if (go == null) return null;
        Transform t = go.transform.parent;
        if (t == null) return null;
        UIPanel tp = t.GetComponent<UIPanel>();
        if (tp != null)
        {
            if (tp.DepthFlag == flag) return tp;
        }
        while (t != null)
        {
            t = t.parent;
            if (t != null)
            {
                tp = t.GetComponent<UIPanel>();
                if (tp != null && tp.DepthFlag == flag) return tp;
            }
        }
        return null;
    }

    static public UIPanel findParentPanelWithoutFlag(GameObject go, PanelDepthFlag flag)
    {
        if (go == null) return null;
        Transform t = go.transform.parent;
        if (t == null) return null;
        UIPanel tp = t.GetComponent<UIPanel>();
        if (tp != null)
        {
            if (tp.DepthFlag != flag) return tp;
        }
        while (t != null)
        {
            t = t.parent;
            if (t != null)
            {
                tp = t.GetComponent<UIPanel>();
                if (tp != null && tp.DepthFlag != flag) return tp;
            }
        }

        return null;
    }

    static public float calculateBaseDepthInParents(GameObject go, float maxDepth)
    {
        float depth = maxDepth;
        go = go.transform.parent.gameObject;
        UIPanel panel = NGUITools.FindInParents<UIPanel>(go);
        while (panel != null)
        {
            depth -= panel.transform.localPosition.z;
            panel = NGUITools.FindInParents<UIPanel>(panel.transform.parent.gameObject);
        }
        if (depth != maxDepth) depth -= 1;
        return depth;
    }

    public PanelDepthFlag DepthFlag
    {
        get
        {
            return mDepthFlag;
        }
        set
        {
            mDepthFlag = value;
            if (mDepthFlag == PanelDepthFlag.Normal)
            {
                AddToNormal(this);
            }
            else if (mDepthFlag == PanelDepthFlag.Bottom)
            {
                AddToBottom(this);
            }
            else if (mDepthFlag == PanelDepthFlag.Top)
            {
                AddToTop(this);
                UIPanel p = findParentPanelWidthFlag(gameObject, PanelDepthFlag.Top);
#if UNITY_EDITOR
                if (p != null)
                {
                    Debug.LogWarning("the parent property is top, are you ensure set the top property for this panel? ");
                }
#endif
                if (p != null)
                {
                    float parentDepth = p.transform.localPosition.z;

                }

            }
            else
            {
                RemoveFromTop(this);
                RemoveFromBottom(this);
                RemoveFromNormal(this);
                Vector3 pos = transform.localPosition;
                pos.z = 0f;
                transform.localPosition = pos;
            }
        }
    }

    public float Depth
    {
        get { return transform.localPosition.z; }
        set
        {
            Vector3 pos = transform.localPosition;
            if (Mathf.Abs(pos.z - value) > 0.01f)
            {
                pos.z = value;
                transform.localPosition = pos;
            }
        }
    }



	/// <summary>
	/// Clipping method used by all draw calls.
	/// </summary>

	public UIDrawCall.Clipping clipping
	{
		get
		{
			return mClipping;
		}
		set
		{
			if (mClipping != value)
			{
				mClipping = value;
				UpdateDrawcalls();
			}
		}
	}

	/// <summary>
	/// Rectangle used for clipping (used with a valid shader)
	/// </summary>

	public Vector4 clipRange
	{
		get
		{
			return mClipRange;
		}
		set
		{
			if (mClipRange != value)
			{
				mClipRange = value;
				UpdateDrawcalls();
			}
		}
	}

	/// <summary>
	/// Clipping softness is used if the clipped style is set to "Soft".
	/// </summary>

	public Vector2 clipSoftness { get { return mClipSoftness; } set { if (mClipSoftness != value) { mClipSoftness = value; UpdateDrawcalls(); } } }

	/// <summary>
	/// Widgets managed by this panel.
	/// </summary>

	public List<UIWidget> widgets { get { return mWidgets; } }

	/// <summary>
	/// Retrieve the list of all active draw calls, removing inactive ones in the process.
	/// </summary>

	public List<UIDrawCall> drawCalls
	{
		get
		{
			for (int i = mDrawCalls.Count; i > 0; )
			{
				UIDrawCall dc = mDrawCalls[--i];
				if (dc == null) mDrawCalls.RemoveAt(i);
			}
			return mDrawCalls;
		}
	}


	/// <summary>
	/// Returns whether the specified rectangle is visible by the panel. The coordinates must be in world space.
	/// </summary>

	bool IsVisible (Vector3 a, Vector3 b, Vector3 c, Vector3 d)
	{
		UpdateTransformMatrix();

		// Transform the specified points from world space to local space
		a = mWorldToLocal.MultiplyPoint3x4(a);
		b = mWorldToLocal.MultiplyPoint3x4(b);
		c = mWorldToLocal.MultiplyPoint3x4(c);
		d = mWorldToLocal.MultiplyPoint3x4(d);

		mTemp[0] = a.x;
		mTemp[1] = b.x;
		mTemp[2] = c.x;
		mTemp[3] = d.x;

		float minX = Mathf.Min(mTemp);
		float maxX = Mathf.Max(mTemp);

		mTemp[0] = a.y;
		mTemp[1] = b.y;
		mTemp[2] = c.y;
		mTemp[3] = d.y;

		float minY = Mathf.Min(mTemp);
		float maxY = Mathf.Max(mTemp);

		if (maxX < mMin.x) return false;
		if (maxY < mMin.y) return false;
		if (minX > mMax.x) return false;
		if (minY > mMax.y) return false;
		return true;
	}

	/// <summary>
	/// Returns whether the specified widget is visible by the panel.
	/// </summary>

	public bool IsVisible (UIWidget w)
	{
		if (!w.enabled || !w.gameObject.activeSelf || w.mainTexture == null || w.color.a < 0.001f) return false;
		
		// fix, all widget out clip range will build too!
		return true;
	}

	/// <summary>
	/// Helper function that marks the specified material as having changed so its mesh is rebuilt next frame.
	/// </summary>

	public void MarkMaterialAsChanged (Material mat, bool sort)
	{
		if (mat != null)
		{
			if (sort) mDepthChanged = true;
			//if (!mChanged.Contains(mat)) mChanged.Add(mat);
			mtlChanged = true;
		}
	}

	/// <summary>
	/// Whether the specified transform is being watched by the panel.
	/// </summary>

	public bool WatchesTransform (Transform t)
	{
		return t == cachedTransform || mWidgetDicts.ContainsKey(t);
	}



	/// <summary>
	/// Add the specified widget to the managed list.
	/// </summary>

	public void AddWidget (UIWidget w)
	{
		if (w != null)
		{
			if (!mWidgetDicts.ContainsKey(w.transform))
			{
				mWidgets.Add(w);
				mWidgetDicts.Add(w.transform, w);
				//if (!mChanged.Contains(w.material)) mChanged.Add(w.material);
				mtlChanged = true;
				mDepthChanged = true;
			}
			w.OnAddWidget();
		}
	}

	/// <summary>
	/// Remove the specified widget from the managed list.
	/// </summary>

	public void RemoveWidget (UIWidget w)
	{
		if (w != null)
		{
			mWidgets.Remove(w);
			mWidgetDicts.Remove(w.transform);
		}
	}

	/// <summary>
	/// Get or create a UIScreen responsible for drawing the widgets using the specified material.
	/// </summary>

	UIDrawCall GetDrawCall (Material mat, bool createIfMissing)
	{
		foreach (UIDrawCall dc in drawCalls) if (dc.material == mat) return dc;

		UIDrawCall sc = null;

		if (createIfMissing)
		{
#if UNITY_EDITOR
			// If we're in the editor, create the game object with hide flags set right away
			GameObject go = UnityEditor.EditorUtility.CreateGameObjectWithHideFlags("_UIDrawCall [" + mat.name + "]",
				/*(mDebugInfo == DebugInfo.Geometry) ? HideFlags.DontSave | HideFlags.NotEditable : */HideFlags.HideAndDontSave);
				//HideFlags.DontSave);
#else
			GameObject go = new GameObject("_UIDrawCall [" + mat.name + "]");
			go.hideFlags = HideFlags.HideAndDontSave;
#endif


			go.layer = gameObject.layer;
			sc = go.AddComponent<UIDrawCall>();
			sc.material = mat;
			mDrawCalls.Add(sc);

			/*
			if(order != null)
			{
				for(int i = 0; i < order.UIMaterialInfos.Length; i++)
				{
					if(order.UIMaterialInfos[i].m_Name == mat.name)
					{
						sc.mtlIndex = i;
						break;
					}
				}
			}
			*/
		}
		return sc;
	}
	
	UIDrawCall GetDrawCall()
	{
		if (mDrawCalls.Count == 0)
		{
#if UNITY_EDITOR
			// If we're in the editor, create the game object with hide flags set right away
			GameObject go = UnityEditor.EditorUtility.CreateGameObjectWithHideFlags("_UIDrawCall [" + name + "]",
				/*(mDebugInfo == DebugInfo.Geometry) ? HideFlags.DontSave | HideFlags.NotEditable : */HideFlags.HideAndDontSave);
				//HideFlags.DontSave);
#else
			GameObject go = new GameObject("_UIDrawCall [" + name + "]");
			go.hideFlags = HideFlags.HideAndDontSave;
#endif

			go.layer = gameObject.layer;
			UIDrawCall sc = go.AddComponent<UIDrawCall>();
			mDrawCalls.Add(sc);
		}
		return mDrawCalls[0];
	}

	/// <summary>
	/// Layer is used to ensure that if it changes, widgets get moved as well.
	/// </summary>

	void Start ()
	{
		mLayer = gameObject.layer;
		mCam = NGUITools.FindCameraForLayer(mLayer);
		//OriScale = cachedTransform.localScale;
		
		//cachedTransform.localScale = new Vector3(0,cachedTransform.localScale.y,cachedTransform.localScale.z);
	}

	/// <summary>
	/// Mark all widgets as having been changed so the draw calls get re-created.
	/// </summary>

	void OnEnable ()
	{
		//foreach (UIWidget w in mWidgets) AddWidget(w);
        foreach (var w in mWidgetDicts) AddWidget(w.Value);
		mRebuildAll = true;
	}

	/// <summary>
	/// Destroy all draw calls we've created when this script gets disabled.
	/// </summary>

	void OnDisable ()
	{
		for (int i = mDrawCalls.Count; i > 0; )
		{
			UIDrawCall dc = mDrawCalls[--i];
			if (dc != null) DestroyImmediate(dc.gameObject);
		}
		mWidgetDicts.Clear();
		mWidgets.Clear();
		mDrawCalls.Clear();
		//mChanged.Clear();
		mtlChanged = false;
	}

    void OnDestroy()
    {
        if (DepthFlag == PanelDepthFlag.Normal)
        {
            RemoveFromNormal(this);
        }
        else if (DepthFlag == PanelDepthFlag.Bottom)
        {
            RemoveFromBottom(this);
        }
        else if (DepthFlag == PanelDepthFlag.Top)
        {
            RemoveFromTop(this);
        }
    }

	/// <summary>
	/// Update the world-to-local transform matrix as well as clipping bounds.
	/// </summary>

	void UpdateTransformMatrix ()
	{
		float time = Time.realtimeSinceStartup;

		if (time == 0f || mMatrixTime != time)
		{
			mMatrixTime = time;
			mWorldToLocal = cachedTransform.worldToLocalMatrix;

			if (mClipping != UIDrawCall.Clipping.None)
			{
				Vector2 size = new Vector2(mClipRange.z, mClipRange.w);

				if (size.x == 0f) size.x = (mCam == null) ? Screen.width  : mCam.pixelWidth;
				if (size.y == 0f) size.y = (mCam == null) ? Screen.height : mCam.pixelHeight;

				size *= 0.5f;

				mMin.x = mClipRange.x - size.x;
				mMin.y = mClipRange.y - size.y;
				mMax.x = mClipRange.x + size.x;
				mMax.y = mClipRange.y + size.y;
			}
		}
	}

	/// <summary>
	/// Run through all managed transforms and see if they've changed.
	/// </summary>

	public void UpdateTransforms ()
	{
		bool transformsChanged = false;

		// Check to see if something has changed
		//foreach (KeyValuePair<Transform, UINode> child in mChildren)
        var child = mWidgetDicts.Keys.GetEnumerator();
        while(child.MoveNext() == true)
		{
			Transform trans = child.Current;
			UIWidget widget = mWidgetDicts[trans];
			if (trans == null || widget == null)
			{
				mRemoved.Add(trans);
				continue;
			}

			if (widget.UpdateTransform(Time.frameCount))
			{
				widget.changeFlag = 1;
				transformsChanged = true;
			}
			else widget.changeFlag = -1;
		}

		// Clean up deleted transforms
		//foreach (Transform rem in mRemoved) 
		for (int i = 0; i < mRemoved.Count; i++)
			mWidgetDicts.Remove(mRemoved[i]);
		mRemoved.Clear();

		// If something has changed, propagate the changes *down* the tree hierarchy (to children).
		// An alternative (but slower) approach would be to do a pc.trans.GetComponentsInChildren<UIWidget>()
		// in the loop above, and mark each one as dirty.

		if (transformsChanged || mRebuildAll)
		{
			var widgetChild = mWidgetDicts.Values.GetEnumerator();
			//foreach (KeyValuePair<Transform, UINode> child in mChildren)
			while (widgetChild.MoveNext() == true)
			{
				UIWidget widget = widgetChild.Current;

				if (widget != null)
				{
					// If the change flag has not yet been determined...
					if (widget.changeFlag == -1)
					{
						widget.changeFlag = widget.UpdateTransform(Time.frameCount) ? 1 : -1;
					}
					if (widget.changeFlag == 1)
					{
						// Is the widget visible?
						int visibleFlag = IsVisible(widget) ? 1 : 0;

						// If the widget is visible (or the flag hasn't been set yet)
						if (visibleFlag == 1 || widget.visibleFlag != 0)
						{
							// Update the visibility flag
							widget.visibleFlag = visibleFlag;
							//Material mat = pc.widget.material;

							// Add this material to the list of changed materials
							//if (!mChanged.Contains(mat)) mChanged.Add(mat);
							mtlChanged = true;
						}
					}
				}
			}
		}
	}

	/// <summary>
	/// Update all widgets and rebuild their geometry if necessary.
	/// </summary>

    public void UpdateWidgets ()
    {
        var child = mWidgetDicts.Values.GetEnumerator();
        while(child.MoveNext() == true)
        {
			UIWidget w = child.Current;

	        // If the widget is visible, update it
			if (w.visibleFlag == 1 && w != null && w.UpdateGeometry(ref mWorldToLocal, (w.changeFlag == 1)))
	        {
		        mtlChanged = true;
	        }
        }
	}

	/// <summary>
	/// Update the clipping rect in the shaders and draw calls' positions.
	/// </summary>

	public void UpdateDrawcalls ()
	{
		Vector4 range = Vector4.zero;

		if (mClipping != UIDrawCall.Clipping.None)
		{
			range = new Vector4(mClipRange.x, mClipRange.y, mClipRange.z * 0.5f, mClipRange.w * 0.5f);
		}
		if (range.z == 0f) range.z = Screen.width * 0.5f;
		if (range.w == 0f) range.w = Screen.height * 0.5f;
		
		/*
		RuntimePlatform platform = Application.platform;

	
		if (platform == RuntimePlatform.WindowsPlayer ||
			platform == RuntimePlatform.WindowsWebPlayer ||
			platform == RuntimePlatform.WindowsEditor)
		{
			range.x -= 0.5f;
			range.y += 0.5f;
		}
		*/
		
#if UNITY_EDITOR || UNITY_STANDALONE_WIN 
		range.x -= 0.5f;
		range.y += 0.5f;
#endif
		
		Transform t = cachedTransform;

		//foreach (UIDrawCall dc in mDrawCalls)
		for (int i = 0; i < mDrawCalls.Count; i++)
		{
			UIDrawCall dc = mDrawCalls[i];
			dc.clipping = mClipping;
			dc.clipRange = range;
			dc.clipSoftness = mClipSoftness;
			dc.depthPass = depthPass;

			// Set the draw call's transform to match the panel's.
			// Note that parenting directly to the panel causes unity to crash as soon as you hit Play.
			Transform dt = dc.transform;
			Vector3 pos = t.position;
            pos.z -= dc.mtlIndex * 0.1f * t.lossyScale.z;
			dt.position = pos;
			dt.rotation = t.rotation;
			dt.localScale = t.lossyScale;
		}
	}

	/// <summary>
	/// Set the draw call's geometry responsible for the specified material.
	/// </summary>
    /// 
    void ResetBuffer(int length)
    {
        if (mVerts.Length < length)
        {
            mVerts = new Vector3[length];
            mUvs = new Vector2[length];
            mCols = new Color[length];
        }
    }

    unsafe void CleanVertex(int offset)
    {
        /*
        for (int i = offset; i < mVerts.Length; i++)
        {
            mVerts[i] = Vector3.zero;
        }
         */
        for (int i = offset; i < mVerts.Length; i++)
        {
            fixed (Vector3* p = &mVerts[i])
            {
                p->x = p->y = p->z = 0;
            }
        }
    }

    public bool ChangeMtl = false;

    List<Material> mMtls = new List<Material>();
    List<int> mTriangleIdx = new List<int>();

	public void Fill()
	{
        int buffer_size = 0;
		// Cleanup deleted widgets
		for (int i = mWidgets.Count; i > 0; ) 
		{
			UIWidget widget = mWidgets[--i];
            if (widget == null)
            {
                mWidgets.RemoveAt(i);
            }
            else if (widget.visibleFlag == 1 && widget.gameObject.activeSelf)
            {
                buffer_size += widget.bufferSize;
            }
		}

        ResetBuffer(buffer_size);

        mMtls.Clear();
        mTriangleIdx.Clear();
		mTriangleIdx.Add(0);
		Material currMtl = null;
		
		// Fill the buffers for the specified material
		//foreach (UIWidget w in mWidgets)
        int filled = 0;
		for (int i = 0; i < mWidgets.Count; i++)
		{
			UIWidget w = mWidgets[i];
			if (w.visibleFlag == 1 && w.gameObject.activeSelf)
			{
				if (currMtl == null || currMtl == w.material)
				{
					currMtl = w.material;
				
					w.WriteToBuffers(mVerts, mUvs, mCols, filled);
                    filled += w.bufferSize;
				}
				else if (currMtl != w.material)
				{
					mMtls.Add(currMtl);
                    mTriangleIdx.Add(filled);
							
					currMtl = w.material;

					w.WriteToBuffers(mVerts, mUvs, mCols, filled);
                    filled += w.bufferSize;
				}
			}
		}

        CleanVertex(filled);

        if (filled > 0)
		{
			mMtls.Add(currMtl);
			mTriangleIdx.Add(filled);
			
			//UIDrawCall dc = GetDrawCall(currMtl, true);
			//dc.depthPass = depthPass;
			
			UIDrawCall dc = GetDrawCall();
			if (dc == null)
			{
				mDrawCalls.RemoveAt(0);
				return;
			}
				
			Mesh mesh = dc.FillMesh(mMtls.ToArray(), ChangeMtl);
			ChangeMtl = false;
			
			mesh.vertices = mVerts;
			mesh.uv = mUvs;
			mesh.colors = mCols;
			mesh.subMeshCount = mMtls.Count;


			int lastidx = 0;
			int[] mIndices;
			for (int i = 0; i < mesh.subMeshCount; i++)
			{
				int count = mTriangleIdx[i + 1] - mTriangleIdx[i];
				int indexCount = (count >> 1) * 3;
				mIndices = PanelVertexBuffer.GetBuffer(indexCount);

				int index = 0;

				for (int j = 0; j < count; j += 4)
				{
					mIndices[index++] = j + lastidx;
					mIndices[index++] = j + lastidx + 1;
					mIndices[index++] = j + lastidx + 2;

					mIndices[index++] = j + lastidx + 2;
					mIndices[index++] = j + lastidx + 3;
					mIndices[index++] = j + lastidx;
				}

				for (int j = index; j < mIndices.Length ;  j++)
                    mIndices[j] = 0;

				lastidx += count;
				mesh.SetTriangles(mIndices, i);
			}
				
			//mesh.RecalculateBounds();
		}
		else
		{
			//Debug.Log("noting");
			if (mDrawCalls.Count > 0 && mDrawCalls[0] != null)
			{
				DestroyImmediate(mDrawCalls[0].gameObject);
				mDrawCalls.RemoveAt(0);
			}
		}
	}

	
	/// <summary>
	/// Main update function
	/// </summary>

	void LateUpdate ()
	{
		UpdateTransformMatrix();
		UpdateTransforms();

		// Always move widgets to the panel's layer
		if (mLayer != gameObject.layer)
		{
			mLayer = gameObject.layer;
			mCam = NGUITools.FindCameraForLayer(mLayer);
			SetChildLayer(cachedTransform, mLayer);
			//foreach (UIDrawCall dc in drawCalls) dc.gameObject.layer = mLayer;
            for (int i = 0; i < drawCalls.Count; i++)
                drawCalls[i].gameObject.layer = mLayer;
		}

		UpdateWidgets();

		// If the depth has changed, we need to re-sort the widgets
		if (mDepthChanged)
		{
			mDepthChanged = false;
			mWidgets.Sort(UIWidget.CompareFunc);
			
		}

		// Fill the draw calls for all of the changed materials
		//foreach (Material mat in mChanged) Fill(mat);
		
		if (mtlChanged == true)
		{
			Fill();

			mtlChanged = false;
			mRebuildAll = false;
		}
		
		// Update the clipping rects
		UpdateDrawcalls();
		
#if UNITY_EDITOR
		mScreenSize = new Vector2(Screen.width, Screen.height);
#endif
	}

#if UNITY_EDITOR

	/// <summary>
	/// Draw a visible pink outline for the clipped area.
	/// </summary>

	void OnDrawGizmos ()
	{
		if (mDebugInfo == DebugInfo.Gizmos && mClipping != UIDrawCall.Clipping.None)
		{
			Vector2 size = new Vector2(mClipRange.z, mClipRange.w);

			if (size.x == 0f) size.x = mScreenSize.x;
			if (size.y == 0f) size.y = mScreenSize.y;

			Gizmos.matrix = transform.localToWorldMatrix;
			Gizmos.color = Color.magenta;
			Gizmos.DrawWireCube(new Vector2(mClipRange.x, mClipRange.y), size);
		}
	}
#endif

	/// <summary>
	/// Calculate the offset needed to be constrained within the panel's bounds.
	/// </summary>

	public Vector3 CalculateConstrainOffset (Vector2 min, Vector2 max)
	{
		float offsetX = clipRange.z * 0.5f;
		float offsetY = clipRange.w * 0.5f;

		Vector2 minRect = new Vector2(min.x, min.y);
		Vector2 maxRect = new Vector2(max.x, max.y);
		Vector2 minArea = new Vector2(clipRange.x - offsetX, clipRange.y - offsetY);
		Vector2 maxArea = new Vector2(clipRange.x + offsetX, clipRange.y + offsetY);

		minArea.x += clipSoftness.x;
		minArea.y += clipSoftness.y;
		maxArea.x -= clipSoftness.x;
		maxArea.y -= clipSoftness.y;

		return NGUIMath.ConstrainRect(minRect, maxRect, minArea, maxArea);
	}

	/// <summary>
	/// Constrain the current target position to be within panel bounds.
	/// </summary>

	public bool ConstrainTargetToBounds (Transform target, ref Bounds targetBounds, bool immediate)
	{
		Vector3 offset = CalculateConstrainOffset(targetBounds.min, targetBounds.max);

		if (offset.magnitude > 0f)
		{
			if (immediate)
			{
				target.localPosition += offset;
				targetBounds.center += offset;
				SpringPosition sp = target.GetComponent<SpringPosition>();
				if (sp != null) sp.enabled = false;
			}
			else
			{
				SpringPosition sp = SpringPosition.Begin(target.gameObject, target.localPosition + offset, 13f);
				sp.ignoreTimeScale = true;
				sp.worldSpace = false;
			}
			return true;
		}
		return false;
	}
	
	public Vector3 CalculateConstrainTopOffset (Vector2 min, Vector2 max)
	{
		float offsetY = clipRange.w * 0.5f;
		
		float maxAreaY =  clipRange.y + offsetY - clipSoftness.y;
		
		Vector3 vOffset = Vector3.zero;
		if(max.y < maxAreaY)
			vOffset.y = maxAreaY - max.y;
		return vOffset;
	}

	/// <summary>
	/// Constrain the current target position to be within panel bounds.
	/// </summary>

	public bool ConstrainTargetToBounds1Top (Transform target, ref Bounds targetBounds, bool immediate)
	{
		Vector3 offset = CalculateConstrainTopOffset(targetBounds.min, targetBounds.max);

		if (offset.magnitude > 0f)
		{
			if (immediate)
			{
				target.localPosition += offset;
				targetBounds.center += offset;
				SpringPosition sp = target.GetComponent<SpringPosition>();
				if (sp != null) sp.enabled = false;
			}
			else
			{
				SpringPosition sp = SpringPosition.Begin(target.gameObject, target.localPosition + offset, 13f);
				sp.ignoreTimeScale = true;
				sp.worldSpace = false;
			}
			return true;
		}
		return false;
	}

	/// <summary>
	/// Constrain the specified target to be within the panel's bounds.
	/// </summary>

	public bool ConstrainTargetToBounds (Transform target, bool immediate)
	{
		Bounds bounds = NGUIMath.CalculateRelativeWidgetBounds(cachedTransform, target);
		return ConstrainTargetToBounds(target, ref bounds, immediate);
	}

	/// <summary>
	/// Helper function that recursively sets all childrens' game objects layers to the specified value, stopping when it hits another UIPanel.
	/// </summary>

	static void SetChildLayer (Transform t, int layer)
	{
		for (int i = 0; i < t.childCount; ++i)
		{
			Transform child = t.GetChild(i);

			if (child.GetComponent<UIPanel>() == null)
			{
				child.gameObject.layer = layer;
				SetChildLayer(child, layer);
			}
		}
	}

	/// <summary>
	/// Find the UIPanel responsible for handling the specified transform.
	/// </summary>

	static public UIPanel Find (Transform trans, bool createIfMissing)
	{
		UIPanel panel = null;

		while (panel == null && trans != null)
		{
			panel = trans.GetComponent<UIPanel>();
			if (panel != null) break;
			if (trans.parent == null) break;
			trans = trans.parent;
		}

//		if (createIfMissing && panel == null)
//		{
//			panel = trans.gameObject.AddComponent<UIPanel>();
//			SetChildLayer(panel.cachedTransform, panel.gameObject.layer);
//		}
		return panel;
	}

	/// <summary>
	/// Find the UIPanel responsible for handling the specified transform, creating a new one if necessary.
	/// </summary>

	static public UIPanel Find (Transform trans) { return Find(trans, true); }
}
