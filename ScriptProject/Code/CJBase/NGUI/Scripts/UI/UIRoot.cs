//----------------------------------------------
//            NGUI: Next-Gen UI kit
// Copyright © 2011-2012 Tasharen Entertainment
//----------------------------------------------

using UnityEngine;

/// <summary>
/// This is a script used to keep the game object scaled to 2/(Screen.height).
/// If you use it, be sure to NOT use UIOrthoCamera at the same time.
/// </summary>

[ExecuteInEditMode]
[AddComponentMenu("NGUI/UI/Root")]
public class UIRoot : MonoBehaviour
{
	public bool automatic = true;
	public int manualHeight = 800;
	
	private bool m_bUpdate = false;
	public static UIRoot mSelf = null;
    public Camera mCamera;
    private float size;
    public float ScaleSize {
        get {
            return size;
        }
    }
	public bool IsUpdate
	{
		get {return m_bUpdate;}
	}
	
	Transform mTrans;
	
	void Awake()
	{
		mSelf = this;
        mTrans = transform;
        //TextureMgr.Instance.ReleaseAll();
        size = GetRightSize();
        SetNewSize(size);
	}
	
	void OnDestroy()
	{
		mSelf = null;
	}

	void Start ()
	{
		UIOrthoCamera oc = GetComponentInChildren<UIOrthoCamera>();

        if (oc != null)
        {
            Debug.LogWarning("UIRoot should not be active at the same time as UIOrthoCamera. Disabling UIOrthoCamera.", oc);
            Camera cam = oc.gameObject.GetComponent<Camera>();
            oc.enabled = false;
            if (cam != null) cam.orthographicSize = 1f;
        }
        mCamera = UICamera.mainCamera;
	}

    public void SetCameraManual() {
        mCamera = UICamera.mainCamera;
    }

	void Update ()
	{
/*
		manualHeight = Mathf.Max(2, automatic ? Screen.height : manualHeight);

		float size = 2f / manualHeight;
		Vector3 ls = mTrans.localScale;

		if (!Mathf.Approximately(ls.x, size) ||
			!Mathf.Approximately(ls.y, size) ||
			!Mathf.Approximately(ls.z, size))
		{
			mTrans.localScale = new Vector3(size, size, size);
		}
		/*
		if (!Mathf.Approximately(ls.x, size) ||
			!Mathf.Approximately(ls.y, size))
		{
			mTrans.localScale = new Vector3(size, size, 1.0f);
		}
		*/

        size = GetRightSize();
        SetNewSize(size);
		
		m_bUpdate = true;
	}

    void SetNewSize(float size) {
        Vector3 ls = mTrans.localScale;
        if (!Mathf.Approximately(ls.x, size) ||
            !Mathf.Approximately(ls.y, size) ||
            !Mathf.Approximately(ls.z, size))
        {
            //mTrans.localScale = new Vector3(size, size, size);
            Vector3 scale = mTrans.localScale;
            scale.x = scale.y = scale.z = size;
            mTrans.localScale = scale;
        }
    }

    float GetRightSize() {
        float r = 960.0f / 640.0f;
        float rightSize = 0.0f;
        float tr = (float)Screen.width / (float)Screen.height;
        if (tr >= r)
        {
            rightSize = 2.0f / 640.0f;
        }
        else
        {
            rightSize = 2.0f / 640.0f;
            rightSize = rightSize * (tr / r);
        }

        return rightSize;
    }
}
