//----------------------------------------------
//            NGUI: Next-Gen UI kit
// Copyright � 2011-2012 Tasharen Entertainment
//----------------------------------------------

using UnityEngine;

/// <summary>
/// Simple slider functionality.
/// </summary>

[ExecuteInEditMode]
[AddComponentMenu("NGUI/Interaction/Slider")]
public class UISlider : IgnoreTimeScale
{
	static public UISlider current;

	public enum Direction
	{
		Horizontal,
		Vertical,
	}

	public UIWidget foreground;
	public Transform thumb;

	public Direction direction = Direction.Horizontal;
	public float rawValue = 1f;
	public GameObject eventReceiver;
	public string functionName = "OnSliderChange";
	public int numberOfSteps = 0;
	public int thumbOffset = 0;
	float mStepValue = 1f;
    //float mStepValueCache = 1f;
	BoxCollider mCol;
	Transform mTrans;

	/// <summary>
	/// Value of the slider. Will match 'rawValue' unless the slider has steps.
	/// </summary>

	public float sliderValue { get { return mStepValue; } set { Set(value); } }

	/// <summary>
	/// Ensure that we have a background and a foreground object to work with.
	/// </summary>

	void Awake ()
	{
		mTrans = transform;
		mCol = this.GetComponent<Collider>() as BoxCollider;
	}

	/// <summary>
	/// We want to receive drag events from the thumb.
	/// </summary>
	/// 
	/// 

	void Start ()
	{
		if (Application.isPlaying && thumb != null && thumb.GetComponent<Collider>() != null)
		{
			UIEventListener listener = UIEventListener.Add(thumb.gameObject);
			listener.onPress += OnPressThumb;
			listener.onDrag += OnDragThumb;
		}	
	}
	
	void OnEnable()
	{
		if (foreground == null)
		{
			Debug.LogWarning("UISlider expected to find a foreground object or a box collider to work with", this);
		}
		
		//SetSlider(rawValue);
		Set(rawValue);
	}

	/// <summary>
	/// Update the slider's position on press.
	/// </summary>

	//void OnPress (bool pressed) { if (pressed) UpdateDrag(); }

	/// <summary>
	/// When dragged, figure out where the mouse is and calculate the updated value of the slider.
	/// </summary>

	void OnDrag (Vector2 delta) { UpdateDrag(); }

	/// <summary>
	/// Callback from the thumb.
	/// </summary>

	void OnPressThumb (GameObject go, bool pressed) { if (pressed) UpdateDrag(); }

	/// <summary>
	/// Callback from the thumb.
	/// </summary>

	void OnDragThumb (GameObject go, Vector2 delta) { UpdateDrag(); }

	/// <summary>
	/// Watch for key events and adjust the value accordingly.
	/// </summary>

	void OnKey (KeyCode key)
	{
		float step = (numberOfSteps > 1f) ? 1f / (numberOfSteps - 1) : 0.125f;

		if (direction == Direction.Horizontal)
		{
			if		(key == KeyCode.LeftArrow)	Set(rawValue - step);
			else if (key == KeyCode.RightArrow) Set(rawValue + step);
		}
		else
		{
			if		(key == KeyCode.DownArrow)	Set(rawValue - step);
			else if (key == KeyCode.UpArrow)	Set(rawValue + step);
		}
	}

	/// <summary>
	/// Update the slider's position based on the mouse.
	/// </summary>

	void UpdateDrag ()
	{
		// Create a plane for the slider
		if (mCol == null || UICamera.lastCamera == null) return;

		// Create a ray and a plane
		Ray ray = UICamera.lastCamera.ScreenPointToRay(UICamera.lastTouchPosition);
		Plane plane = new Plane(mTrans.rotation * Vector3.back, mTrans.position);

		// If the ray doesn't hit the plane, do nothing
		float dist;
		if (!plane.Raycast(ray, out dist)) return;

		// Collider's bottom-left corner in local space
		Vector3 localOrigin = mTrans.localPosition + mCol.center - mCol.size;
		Vector3 localOffset = mTrans.localPosition - localOrigin;
		
		// Direction to the point on the plane in scaled local space
		Vector3 localCursor = mTrans.InverseTransformPoint(ray.GetPoint(dist));
		Vector3 dir = localCursor + localOffset;
		// Update the slider
		Set( (direction == Direction.Horizontal) ? dir.x / mCol.size.x : dir.y / mCol.size.y );
	}

#if UNITY_EDITOR
	void Update () { Set(rawValue); }
#endif

	/// <summary>
	/// Update the visible slider.
	/// </summary>
	
	
	void SetSlider(float val)
	{
		mStepValue = val;

        if (direction == Direction.Horizontal) foreground.drawRegion = new Vector4(0, 0, mStepValue,1);
        else foreground.drawRegion = new Vector4(0, 0, 1, mStepValue);
			
		if (thumb != null)
		{
            Vector3 pos = thumb.localPosition;
            if (direction == Direction.Horizontal)
            {
                thumb.localPosition = new Vector3(mStepValue*foreground.Dimensions.x + thumbOffset,pos.y,pos.z);
            }
            else
            {
                thumb.localPosition = new Vector3(pos.x,mStepValue * foreground.Dimensions.y + thumbOffset,pos.z);
            }

		}
		if (eventReceiver != null && !string.IsNullOrEmpty(functionName))
		{
			//current = this;
            if (eventReceiver.GetComponent<LuaMessageReceiver>() != null)
            {
                eventReceiver.SendMessage(functionName, new LuaMessage(gameObject, mStepValue, "OnChangeValue"), SendMessageOptions.DontRequireReceiver);
            }
            else
            {
                eventReceiver.SendMessage(functionName, mStepValue, SendMessageOptions.DontRequireReceiver);
            }
			//current = null;
		}

       
	}
	
	void Set (float input)
	{
		// Clamp the input
		float val = Mathf.Clamp01(input);

		// Save the raw value
		rawValue = val;

		// Take steps into consideration
		if (numberOfSteps > 1) val = Mathf.Round(val * (numberOfSteps - 1)) / (numberOfSteps - 1);

		// If the stepped value doesn't match the last one, it's time to update
        if (mStepValue != val)
        { 
            SetSlider(val);
        }
	}

	
	void OnClickThumb ()
	{
			if (eventReceiver != null)
			{
				eventReceiver.SendMessage("OnSaveVolume", mStepValue, SendMessageOptions.DontRequireReceiver);
			}
	}
	
	public void HideBar(){
		foreground.gameObject.SetActive(false);
	}

    public void ShowBar()
    {
        foreground.gameObject.SetActive(true);
    }
}
