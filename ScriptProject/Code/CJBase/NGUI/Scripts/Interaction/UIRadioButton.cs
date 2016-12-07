using UnityEngine;

//[ExecuteInEditMode]
[AddComponentMenu("NGUI/UI/Radio Button")]
public class UIRadioButton : MonoBehaviour
{
	public UISprite target;
    public UILabel LblTarget;
    public UISprite TopLight;
    public Color ActiveLblColor = new Color(0.227f,0.172f,0.020f);
    public Color UnactiveLblColor = new Color(0.733f,0.757f,0.749f);
    public int ActiveTopLightAlpha = 223;
    public int UnActiveTopLightAlpha = 70;
	public bool UseSingleActiveSprite = false;
    public bool UseColorInSingleSprite = false;
	public string ActiveSprite;
	public float ActiveScaleX;
	public float ActiveScaleY;
	public string UnactiveSprite;
	public float UnactiveScaleX;
	public float UnactiveScaleY;
	public bool option = true;
	public bool Startstate = false;
	private bool  bActice;
	public object user_data;
	public GameObject[] user_data_recvs;
	public bool Auto = true;
	public bool ClickForbidOnDataEmpty = false;
	public string user_data_send_name = "OnChoosedButton";
	public GameObject AttachShow;
    public bool userDataIsGo;
	public bool check
	{
		set { Set(value); }
	}
	
	void OnEnable()
	{
		if (target == null)
			target = GetComponentInChildren<UISprite> ();
        if (LblTarget == null)
            LblTarget = GetComponentInChildren<UILabel>();
		SetLblColor();
        if(TopLight == null)
            GetTopLight();
        SetTopLight();
	}
	
	void Start ()
	{		
		if (Auto)
			Set (Startstate);
        
	}

	public void SetBackSelect(bool state = true)
	{
		bActice = state;
		SwitchRadioButton();
		target.MakePixelPerfect();
	}
	
	void Set (bool state)
	{
		if (target != null) {
			bActice = state;
			if (bActice && user_data_recvs != null && user_data_recvs.Length > 0) {
                if (userDataIsGo)
                {
                    user_data = gameObject;
                }      
				NGUITools.SendAll (user_data_recvs, user_data_send_name, user_data, SendMessageOptions.DontRequireReceiver);	 						
			}
			SwitchRadioButton();
			target.MakePixelPerfect ();
			if (ActiveScaleX > 0 && ActiveScaleY > 0 && UnactiveScaleX > 0 && UnactiveScaleY > 0) {
				Vector3 scale = target.transform.localScale;
				scale.x = bActice ? ActiveScaleX : UnactiveScaleX;
				scale.y = bActice ? ActiveScaleY : UnactiveScaleY;
				target.transform.localScale = scale;
			}
			
			if (option && bActice == true){ 
				UIRadioButton[] rbs = transform.parent.GetComponentsInChildren<UIRadioButton> ();
				foreach (UIRadioButton rb in rbs)
					if (rb != this)
						rb.Set (false);
			}
			
			if (AttachShow != null)
				AttachShow.SetActive(state);
			
		}
	}
	
	void OnReset ()
	{
		Set (Startstate);
	}
	
	void OnChooseButton ()
	{
		Set (true);		
	}
	
	void OnClick ()
	{
		if (bActice == false) {
			if (ClickForbidOnDataEmpty == false || user_data != null)
				Set (true);
		}
	}

	private void SwitchRadioButton()
	{
		if (UseSingleActiveSprite)
		{
			if (ActiveSprite != null)
			{
				target.gameObject.SetActive(bActice ? true : false);
                if (UseColorInSingleSprite)
                {
                    SetLblColor(bActice);
                }
			}
		}
		else
        {
            target.spriteName = bActice ? ActiveSprite : UnactiveSprite;
			SetLblColor();
            SetTopLight();
		}
	}

	void SetLblColor()
	{
		if (LblTarget != null)
		{
			bool active = (target.spriteName == ActiveSprite);
			LblTarget.color = active ? ActiveLblColor : UnactiveLblColor;
		}
	}

    void SetLblColor(bool active)
    {
        if (LblTarget != null)
        {
            LblTarget.color = active ? ActiveLblColor : UnactiveLblColor;
        }
    }

    void SetTopLight()
    {
        if (TopLight != null)
        {
            bool active = (target.spriteName == ActiveSprite);
            TopLight.color = active ? new Color(TopLight.color.r, TopLight.color.g, TopLight.color.b, ActiveTopLightAlpha/255f) : new Color(TopLight.color.r, TopLight.color.g, TopLight.color.b, UnActiveTopLightAlpha/255f);
        }
    }

    #region Code Fit  need
    void GetTopLight()
    {
        int count = transform.childCount;
        for (int i = 0; i < count; i++)
        {
            Transform trans = transform.GetChild(i);
            UISprite sprite = trans.GetComponent<UISprite>();
            if (sprite != null && sprite.spriteName.Contains("light"))
            {
                TopLight = sprite;
//                sprite.transform.localPosition += new Vector3(0, -1.5f, 0);
//                sprite.transform.localScale += new Vector3(-3, 0, 0);
            }
        }
    }
    #endregion
}
