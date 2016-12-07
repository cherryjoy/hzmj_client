//----------------------------------------------
//            NGUI: Next-Gen UI kit
// Copyright © 2011-2012 Tasharen Entertainment
//----------------------------------------------

using UnityEngine;
using System.Collections.Generic;
using System.Reflection;
using System.Collections;
/// <summary>
/// Base class for all UI components that should be derived from when creating new widget types.
/// </summary>

public abstract class UIWidget : MonoBehaviour
{

#region For Node
	//int mNodeVisibleFlag = -1;

	private Transform trans;			// Managed transform
	//private UIWidget widget;			// Widget on this transform, if any

	//private bool lastActive = false;	// Last active state
	private Vector3 lastPos;			// Last local position, used to see if it has changed
	private Quaternion lastRot;		// Last local rotation
    //private Vector3 lastScale;		// Last local scale

	public int changeFlag = -1;		// -1 = not checked, 0 = not changed, 1 = changed

	Vector3 mOldV0;
	int mCacheFrame = 0;
	bool mMoved = false;
	public bool UpdateTransform(int frame)
	{	
		if (mChanged)
		{
			return true;
		}
		if (mCacheFrame == frame)
		{
			return mMoved;
		}
		mCacheFrame = frame;
		mMoved = false;
		if (cachedTransform.hasChanged)
		{
			mTrans.hasChanged = false;

			Vector2 offset = pivotOffset;
            //Vector3 scale = cachedTransform.localScale;
            Vector3 scale = new Vector3(width, height);
            //float mWidth = scale.x;
            //float mHeight = scale.y;
			float x0 = -offset.x * scale.x;
			float y0 = -offset.y * scale.y;
			float x1 = x0 + scale.x;
			float y1 = y0 + scale.y;

			Transform wt = cachedTransform;

			Vector3 v0 = wt.TransformPoint(x0, y0, 0f);
			Vector3 v1 = wt.TransformPoint(x1, y1, 0f);
			
			v0 = panel.mWorldToLocal.MultiplyPoint3x4(v0);
			v1 = panel.mWorldToLocal.MultiplyPoint3x4(v1);

			float offset0 = Vector3.SqrMagnitude(mOldV0 - v0);
			if (offset0 > 0.000001f || offset0 > 0.000001f)
			{
				mMoved = true;
				mOldV0 = v0;
			}
		}
		return mMoved;
	}
#endregion


	public enum Pivot
	{
		TopLeft,
		Top,
		TopRight,
		Left,
		Center,
		Right,
		BottomLeft,
		Bottom,
		BottomRight,
	}

	// Cached and saved values
	[SerializeField] Material mMat;
	[SerializeField] Color mColor = Color.white;
	[SerializeField] Pivot mPivot = Pivot.Center;
    [HideInInspector] [SerializeField] protected int mWidth = 100;
    [HideInInspector] [SerializeField] protected int mHeight = 100;
	[SerializeField] int mDepth = 0;
	[SerializeField] bool mIsHidePanel = false;
    /// <summary>
    /// Final calculated alpha.
    /// </summary>

    //[System.NonSerialized] public float finalAlpha = 1f;
    protected Vector4 mDrawRegion = new Vector4(0f, 0f, 1f, 1f);
	
	Transform mTrans;
    UIWidget mWgt;
	Texture mTex;
	UIPanel mPanel;
	protected bool mChanged = true;
	protected bool mPlayMode = true;
	protected bool haveCheckLayer = false;
    /*
	Vector3 mDiffPos;
	Quaternion mDiffRot;
	Vector3 mDiffScale;
     */
	int mVisibleFlag = -1;

	// Widget's generated geometry
	UIGeometry mGeom = new UIGeometry();

	/// <summary>
	/// Color used by the widget.
	/// </summary>

    public Color color { get { return mColor; } set { if (mColor != value) { mColor = value; mChanged = true; MarkAsChanged(); } } }

	/// <summary>
	/// Set or get the value that specifies where the widget's pivot point should be.
	/// </summary>

    public Pivot pivot { get { return mPivot; } set { if (mPivot != value) { mPivot = value; mChanged = true; MarkAsChanged(); } } }
	
	/// <summary>
	/// Depth controls the rendering order -- lowest to highest.
	/// </summary>

	public int depth { get { return mDepth; } set { if (mDepth != value) { mDepth = value; if (mPanel != null) mPanel.MarkMaterialAsChanged(mMat, true); } } }

	/// <summary>
	/// Transform gets cached for speed.
	/// </summary>

    public Transform cachedTransform { get { if (mTrans == null) mTrans = transform; return mTrans; } }
    public UIWidget cachedWidght { get { if (mWgt == null) mWgt = this; return mWgt; } }

    public int bufferSize
    {
        get { return mGeom.verts.size; }
    }


	/// <summary>
	/// Returns the material used by this widget.
	/// </summary>
	
	public bool IsHidePanel
	{
		get {return mIsHidePanel;}
		set {mIsHidePanel = value;}
	}

	public virtual Material material
	{
		get
		{
			return mMat;
		}
		set
		{
			if (mMat != value)
			{
				if (mPanel != null)
				{
					mPanel.RemoveWidget(this);
					mPanel = null;
				}
				
				mMat = value;
				if (mMat != null)
				{
					mTex = mMat.mainTexture;
				}
				else
				{
					mTex = null;
				}
				if (mMat != null)
				{
					CreatePanel();
				}
				
			}
		}
	}

	/// <summary>
	/// Returns the texture used to draw this widget.
	/// </summary>

	public Texture mainTexture
	{
		get
		{
			if (mTex == null)
			{
				Material mat = material;
				if (mat != null) mTex = mat.mainTexture;
			}
			return mTex;
		}
	}

	/// <summary>
	/// Returns the UI panel responsible for this widget.
	/// </summary>

	public UIPanel panel { get { CreatePanel(); return mPanel; } set { mPanel = value; } }

	/// <summary>
	/// Flag set by the UIPanel and used in optimization checks.
	/// </summary>

	public int visibleFlag 
	{
		get { return mVisibleFlag; } 
		set { mVisibleFlag = value; } 
	}

	/// <summary>
	/// Static widget comparison function used for Z-sorting.
	/// </summary>

	static public int CompareFunc (UIWidget left, UIWidget right)
	{
		if (left.mDepth > right.mDepth) return 1;
		if (left.mDepth < right.mDepth) return -1;
		//return 0;
		// sort material
		if(left.material == null || right.material == null) return 1;
		
		if (left.material.GetInstanceID() > right.material.GetInstanceID()) return 1;
		if (left.material.GetInstanceID() < right.material.GetInstanceID()) return -1;
		return 0;
	}

	/// <summary>
	/// Tell the panel responsible for the widget that something has changed and the buffers need to be rebuilt.
	/// </summary>

	public virtual void MarkAsChanged ()
	{
		mChanged = true;

		// If we're in the editor, update the panel right away so its geometry gets updated.
		if (mPanel != null && enabled && gameObject.activeSelf && !Application.isPlaying && mMat != null)
		{
			mPanel.AddWidget(this);
			CheckLayer();
#if UNITY_EDITOR
			// Mark the panel as dirty so it gets updated
			UnityEditor.EditorUtility.SetDirty(mPanel.gameObject);
#endif
		}
	}

	/// <summary>
	/// Ensure we have a panel referencing this widget.
	/// </summary>

	void CreatePanel ()
	{
		if (gameObject!= null && mPanel == null && enabled && gameObject.activeSelf && gameObject.activeInHierarchy)
		{
			if (cachedTransform == null)
			{
				Debug.LogWarning("Cache Panel False");
			}
			
			mPanel = UIPanel.Find(cachedTransform);
			if(mPanel != null)
			{
				CheckLayer();
				mPanel.AddWidget(this);
				mChanged = true;
			}
		}
	}

	/// <summary>
	/// Check to ensure that the widget resides on the same layer as its panel.
	/// </summary>

	void CheckLayer ()
	{
		if (haveCheckLayer)
		{
			return;
		}
		haveCheckLayer = true;
		if (mPanel != null && mPanel.gameObject.layer != gameObject.layer)
		{
			gameObject.layer = mPanel.gameObject.layer;
		}
	}

	/// <summary>
	/// Checks to ensure that the widget is still parented to the right panel.
	/// </summary>

	void CheckParent ()
	{
		if (mPanel != null)
		{
			// This code allows drag & dropping of widgets onto different panels in the editor.
			bool valid = true;
			Transform t = cachedTransform.parent;

			// Run through the parents and see if this widget is still parented to the transform
			while (t != null)
			{
				if (t == mPanel.cachedTransform) break;
				if (!mPanel.WatchesTransform(t)) { valid = false; break; }
				t = t.parent;
			}

			// This widget is no longer parented to the same panel. Remove it and re-add it to a new one.
			if (!valid)
			{
				mPanel.RemoveWidget(this);
				mPanel = null;
				CreatePanel();
			}
		}
	}

	/// <summary>
	/// Cache the transform.
	/// </summary
	
	void Awake ()
	{
		if (GetComponents<UIWidget>().Length > 1)
		{
			Debug.LogError("Can't have more than one widget on the same game object.\nDestroying the second one.", this);
			if (Application.isPlaying) DestroyImmediate(this);
			else Destroy(this);
		}
		else
		{
			mPlayMode = Application.isPlaying;
		}
		
		
	}

	/// <summary>
	/// Mark the widget and the panel as having been changed.
	/// </summary>

	protected void OnEnable ()
	{
		mChanged = true;
		if (panel != null && mMat != null) 
		{
			panel.MarkMaterialAsChanged(mMat, false);
		}
	}

	/// <summary>
	/// Set the depth, call the virtual start function, and sure we have a panel to work with.
	/// </summary>

	void Start ()
	{
		trans = transform;
		lastPos = trans.localPosition;
		lastRot = trans.localRotation;
        //lastScale = trans.localScale;
		OnStart();
		CreatePanel();
	}

	/// <summary>
	/// Ensure that we have a panel to work with. The reason the panel isn't added in OnEnable()
	/// is because OnEnable() is called right after Awake(), which is a problem when the widget
	/// is brought in on a prefab object as it happens before it gets parented.
	/// </summary>

	protected void Update ()
	{
		CheckLayer();

		// Ensure we have a panel to work with by now
		if (mPanel == null) CreatePanel();
#if UNITY_EDITOR
		else if (!Application.isPlaying) CheckParent();
#endif
		
		// Automatically reset the Z scaling component back to 1 as it's not used
		Vector3 scale = cachedTransform.localScale;

		if (scale.z != 1f)
		{
			scale.z = 1f;
			mTrans.localScale = scale;
		}
	}

	/// <summary>
	/// Don't store any references to the panel.
	/// </summary>

	protected void OnDisable()
	{
		if (mPanel != null) 
            mPanel.MarkMaterialAsChanged(material, false);
		if (panel != null)
		{
			panel.RemoveWidget(this);
			panel = null;
		}
	}

	/// <summary>
	/// Unregister this widget.
	/// </summary>

	protected void OnDestroy ()
	{
		if (mPanel != null)
		{
			mPanel.RemoveWidget(this);
			mPanel = null;
		}
	}

#if UNITY_EDITOR

	/// <summary>
	/// Draw some selectable gizmos.
	/// </summary>

	void OnDrawGizmos ()
	{
		if (visibleFlag != 0 && mPanel != null && mPanel.debugInfo == UIPanel.DebugInfo.Gizmos)
		{
			Color outline = new Color(1f, 1f, 1f, 0.2f);

			// Position should be offset by depth so that the selection works properly
			Vector3 pos = Vector3.zero;
			pos.z -= mDepth * 0.25f;

			// Widget's local size
			Vector2 size = relativeSize;
            Vector2 offset = pivotOffsetOnGizmos;

			pos.x += (offset.x + 0.5f * width) * size.x;
			pos.y += (offset.y - 0.5f * height) * size.y;
			// Draw the gizmo
			Gizmos.matrix = cachedTransform.localToWorldMatrix;
			Gizmos.color = (UnityEditor.Selection.activeGameObject == gameObject) ? new Color(0f, 0.75f, 1f) : outline;
            size.x *= width;
            size.y *= height;

			Gizmos.DrawWireCube(pos, size);
			Gizmos.color = Color.clear;
			Gizmos.DrawCube(pos, size);
		}

//         Bounds bounds = NGUIMath.CalculateRelativeWidgetBounds(mPanel.cachedTransform, transform);
//         Gizmos.matrix = cachedTransform.localToWorldMatrix;
//         Gizmos.color = Color.yellow;
//         Gizmos.DrawWireCube(bounds.center, bounds.size);
	}

    public Vector2 pivotOffsetOnGizmos
    {
        get
        {
            Vector2 v = Vector2.zero;

            if (pivot == Pivot.Top || pivot == Pivot.Center || pivot == Pivot.Bottom) v.x = -0.5f;
            else if (pivot == Pivot.TopRight || pivot == Pivot.Right || pivot == Pivot.BottomRight) v.x = -1f;

            if (pivot == Pivot.Left || pivot == Pivot.Center || pivot == Pivot.Right) v.y = 0.5f;
            else if (pivot == Pivot.BottomLeft || pivot == Pivot.Bottom || pivot == Pivot.BottomRight) v.y = 1f;

            v.x *= width;
            v.y *= height;
            return v;
        }
    }

#endif

	/// <summary>
	/// Update the widget and fill its geometry if necessary. Returns whether something was changed.
	/// </summary>

	public bool UpdateGeometry (ref Matrix4x4 worldToPanel, bool parentMoved)
	{
		if (material == null) return false;

		if (OnUpdate() || mChanged || changeFlag == 1)
		{
			mChanged = false;
			mGeom.Clear();
			changeFlag = -1;

			OnFill(mGeom.verts, mGeom.uvs, mGeom.cols);

			if (mGeom.hasVertices)
			{
               // cachedTransform.rotation = new Quaternion(0, 0, cachedTransform.rotation.z, 0);
                cachedTransform.eulerAngles = new Vector3(0, 0, cachedTransform.eulerAngles.z);
				mGeom.ApplyTransform(worldToPanel * cachedTransform.localToWorldMatrix);
			}
			return true;
		}
		else if (mGeom.hasVertices && parentMoved)
		{
            //cachedTransform.rotation = new Quaternion(0, 0, cachedTransform.rotation.z,0);
            cachedTransform.eulerAngles = new Vector3(0, 0, cachedTransform.eulerAngles.z);
			mGeom.ApplyTransform(worldToPanel * cachedTransform.localToWorldMatrix);
		}
		return false;
	}

	/// <summary>
	/// Append the local geometry buffers to the specified ones.
	/// </summary>

	public void WriteToBuffers (Vector3[] v, Vector2[] u, Color[] c, int offset)
	{
        mGeom.WriteToBuffers(v, u, c, offset);
	}

	/// <summary>
	/// Make the widget pixel-perfect.
	/// </summary>

	virtual public void MakePixelPerfect ()
	{
        Vector3 pos = cachedTransform.localPosition;
        pos.z = Mathf.Round(pos.z);
        pos.x = Mathf.Round(pos.x);
        pos.y = Mathf.Round(pos.y);
        cachedTransform.localPosition = pos;

        Vector3 ls = cachedTransform.localScale;
        cachedTransform.localScale = new Vector3(Mathf.Sign(ls.x), Mathf.Sign(ls.y), 1f);
        //Vector3 scale = cachedTransform.localScale;
        //Vector3 scale = new Vector3(mWidth, mHeight);
        //int width  = Mathf.RoundToInt(scale.x);
        //int height = Mathf.RoundToInt(scale.y);

        //scale.x = width;
        //scale.y = height;
        //scale.z = 1f;

        //Vector3 pos = cachedTransform.localPosition;
        //pos.z = Mathf.RoundToInt(pos.z);

        //if (width % 2 == 1 && (pivot == Pivot.Top || pivot == Pivot.Center || pivot == Pivot.Bottom))
        //{
        //    pos.x = Mathf.Floor(pos.x) + 0.5f;
        //}
        //else
        //{
        //    pos.x = Mathf.Round(pos.x);
        //}

        //if (height % 2 == 1 && (pivot == Pivot.Left || pivot == Pivot.Center || pivot == Pivot.Right))
        //{
        //    pos.y = Mathf.Ceil(pos.y) - 0.5f;
        //}
        //else
        //{
        //    pos.y = Mathf.Round(pos.y);
        //}

        //cachedTransform.localPosition = pos;
        //cachedTransform.localScale = Vector3.one;
	}

    public void FixOldVersion()
    {
        Vector3 scale = transform.localScale;
        if (scale != Vector3.one)
        {
            Dimensions = new Vector2(scale.x, scale.y);
            transform.localScale = Vector3.one;
        }
    }

    public Vector4 drawRegion {
        get {
            return mDrawRegion;
        }
        set {
            if (mDrawRegion != value) {
                mDrawRegion = value;
                MarkAsChanged();
            }
        }
    }


    /// <summary>
    /// Helper function that calculates the relative offset based on the current pivot.
    /// </summary>

    virtual public Vector2 pivotOffset
    {
        get
        {
            Vector2 v = Vector2.zero;

            if (mPivot == Pivot.Top || mPivot == Pivot.Center || mPivot == Pivot.Bottom) v.x = -0.5f;
            else if (mPivot == Pivot.TopRight || mPivot == Pivot.Right || mPivot == Pivot.BottomRight) v.x = -1f;

            if (mPivot == Pivot.Left || mPivot == Pivot.Center || mPivot == Pivot.Right) v.y = 0.5f;
            else if (mPivot == Pivot.BottomLeft || mPivot == Pivot.Bottom || mPivot == Pivot.BottomRight) v.y = 1f;

            return v;
        }
    }

    /// <summary>
    /// Helper funcation set or get the Dimensions value
    /// </summary>

    virtual public Vector2 Dimensions
    {
        get { return new Vector2(width, height); }
        set
        {
            if (cachedWidght.Dimensions != value)
            {
                Vector2 val = value;
                width = Mathf.RoundToInt(val.x);
                height = Mathf.RoundToInt(val.y);

                MarkAsChanged();
            }
        }
    }

    virtual public void SetDimesions(int width, int height)
    {
        Dimensions = new Vector2(width, height);
    }


    ///// <summary>
    ///// Helper function that calculates the relative offset based on the current pivot.
    ///// </summary>

    //virtual public Vector2 pivotOffset
    //{
    //    get
    //    {
    //        Vector2 v = Vector2.zero;

    //        if (mPivot == Pivot.Top || mPivot == Pivot.Center || mPivot == Pivot.Bottom) v.x = 0.5f;
    //        else if (mPivot == Pivot.TopRight || mPivot == Pivot.Right || mPivot == Pivot.BottomRight) v.x = 1f;

    //        if (mPivot == Pivot.Left || mPivot == Pivot.Center || mPivot == Pivot.Right) v.y = 0.5f;
    //        else if (mPivot == Pivot.BottomLeft || mPivot == Pivot.Bottom || mPivot == Pivot.BottomRight) v.y = 1f;

    //        return v;
    //    }
    //}

	/// <summary>
	/// Deprecated property.
	/// </summary>

	[System.Obsolete("Use 'relativeSize' instead")]
	public Vector2 visibleSize { get { return relativeSize; } }

	/// <summary>
	/// Visible size of the widget in relative coordinates. In most cases this can remain at (1, 1).
	/// If you want to figure out the widget's size in pixels, scale this value by cachedTransform.localScale.
	/// </summary>

	virtual public Vector2 relativeSize { get { return Vector2.one; } }

	/// <summary>
	/// Virtual Start() functionality for widgets.
	/// </summary>

	virtual protected void OnStart () { }

	/// <summary>
	/// Virtual version of the Update function. Should return 'true' if the widget has changed visually.
	/// </summary>

	virtual public bool OnUpdate () { return false; }

	/// <summary>
	/// Virtual function called by the UIPanel that fills the buffers.
	/// </summary>

	virtual public void OnFill (BetterList<Vector3> verts, BetterList<Vector2> uvs, BetterList<Color> cols) { }

    void OnValidate()
    {
        width = mWidth;
        height = mHeight;
        MarkAsChanged();
    }
	
	void HidePanel()
	{
		if (Application.isEditor == true && Application.isPlaying == false) return;
		
		if(panel != null && panel.showProgress)
		{
			float x = panel.transform.localPosition.x;
			float y = panel.transform.localPosition.y;
			float z = -2000;
			
			panel.transform.localPosition = new Vector3(x,y,z);
		}		
	}

    /// <summary>
    /// Local space region where the actual drawing will take place.
    /// X = left, Y = bottom, Z = right, W = top.
    /// </summary>

    public virtual Vector4 drawingDimensions
    {
        get
        {
            Vector2 offset = pivotOffset;

            float x0 = -offset.x * mWidth;
            float y0 = -offset.y * mHeight;
            float x1 = x0 + mWidth;
            float y1 = y0 + mHeight;

            return new Vector4(
                mDrawRegion.x == 0f ? x0 : Mathf.Lerp(x0, x1, mDrawRegion.x),
                mDrawRegion.y == 0f ? y0 : Mathf.Lerp(y0, y1, mDrawRegion.y),
                mDrawRegion.z == 1f ? x1 : Mathf.Lerp(x0, x1, mDrawRegion.z),
                mDrawRegion.w == 1f ? y1 : Mathf.Lerp(y0, y1, mDrawRegion.w));
        }
    }	
	
	public void OnAddWidget()
	{
	
		if (Application.isEditor == true && Application.isPlaying == false) return;
		
		if(GetType() == typeof(UISprite) || GetType().IsSubclassOf(typeof(UISprite)))
		{
			UISprite spr = this as UISprite;
			if(spr.atlas == null) return;	
			
			string strSpriteName = spr.spriteName;
			spr.spriteName = "";
			spr.spriteName = strSpriteName;
			
			spr.material = spr.atlas.spriteMaterial;
		}
	}

    /// <summary>
    /// Dimensions of the sprite's border, if any.
    /// </summary>

    virtual public Vector4 border { get { return Vector4.zero; } }

    /// <summary>
    /// Minimum allowed width for this widget.
    /// </summary>

    virtual public int minWidth { get { return 1; } }

    /// <summary>
    /// Minimum allowed height for this widget.
    /// </summary>

    virtual public int minHeight { get { return 1; } }

    public int width
    {
        get
        {
            return mWidth;
        }

        set
        {
            int min = minWidth;
            if (value < min) value = min;

            if (mWidth != value)
            {
                mWidth = value;
            }
        }
    }

    public int height
    {
        get
        {
            return mHeight;
        }

        set
        {
            int min = minHeight;
            if (value < min) value = min;

            if (mHeight != value)
            {
                mHeight = value;
            }
        }
    }


    //for lua to invoke
    public void SetColor(float r, float g, float b, float a) {
        Color c = new Color(r, g, b, a);
        color = c;
    }
// 
// //     public void GetColor() { 
// //         
// //     }
// 
//     public void SetPosition(float x, float y, float z) { 
//         
//     
//     }

}
