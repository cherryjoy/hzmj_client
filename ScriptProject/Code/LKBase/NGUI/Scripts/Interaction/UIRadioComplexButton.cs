using UnityEngine;
using System.Collections;

public class UIRadioComplexButton : MonoBehaviour {

    public UISprite target;
    public UISprite foreground;
    public string ActiveSprite;
    public string UnactiveSprite;
	public GameObject m_ActiveObject;
    public object user_data;
    public GameObject[] user_data_recvs;
    public string user_data_send_name = "OnChoosedComplexButton";
    private bool bActice;
    public GameObject childGameobject;
	public bool userNormalBtn = false;
	[HideInInspector]
	public UILabel mLabel;
	public bool makeAction = false;
	[HideInInspector]
	public Vector3 localPos;
	public Vector3 mVecMove;
	private bool initd = false;

	public string Text
	{
		get
		{
			return mLabel != null ? mLabel.text : "";
		}
		set
		{
			if (mLabel != null)
				mLabel.text = value;
		}
	}

    public bool check
    {
        set {
			bActice = value;
			if (bActice)
			{
				Set(value);
			}
			else
				Reset();
			}
    }

    void OnEnable()
    {
        if (target == null && childGameobject != null)
            target = childGameobject.GetComponentInChildren<UISprite>();
		if (mLabel == null)
		{
			UILabel lb = GetComponentInChildren<UILabel>();
			if (lb != null)
				mLabel = lb;
		}
    }

	void Start()
	{
		localPos = transform.localPosition;
		initd = true;
		if (makeAction && bActice)
		{
			transform.localPosition = new Vector3(localPos.x + mVecMove.x, localPos.y + mVecMove.y, localPos.z + mVecMove.z);
		}
	}
  
    void OnChooseButton()
    {
        Set(true);
    }

    void OnClick()
    {
        if (bActice == false)
        {
			if (user_data != null)
			{
				Set(true);
			}
			else if (userNormalBtn)
			{
				SetNormal(true);
			}
        }
    }

	void SetNormal(bool state)
	{
		if (target != null)
		{
			bActice = state;
			if (bActice && user_data_recvs != null && user_data_recvs.Length > 0)
			{
				SendAll(user_data_recvs, user_data_send_name, SendMessageOptions.DontRequireReceiver);
			}
			if (string.IsNullOrEmpty(ActiveSprite) == false)
			{
				target.spriteName = bActice ? ActiveSprite : UnactiveSprite;
				target.MakePixelPerfect();
			}
			if (m_ActiveObject != null)
			{
				m_ActiveObject.SetActive(bActice);
			}
			if (bActice == true)
			{
				UIRadioComplexButton[] rbs = transform.parent.GetComponentsInChildren<UIRadioComplexButton>();
				foreach (UIRadioComplexButton rb in rbs)
					if (rb != this)
						rb.Set(false);
			}
		}
	}

    void Set(bool state)
    {
        if (target != null)
        {
            bActice = state;
            if (bActice && user_data_recvs != null && user_data_recvs.Length > 0)
            {
                NGUITools.SendAll(user_data_recvs, user_data_send_name, user_data, SendMessageOptions.DontRequireReceiver);
            }
			if (string.IsNullOrEmpty(ActiveSprite) == false)
			{
				target.spriteName = bActice ? ActiveSprite : UnactiveSprite;
				target.MakePixelPerfect();
			}
			if (m_ActiveObject != null)
			{
				m_ActiveObject.SetActive(bActice);
			}
			if (bActice == true)
			{
				UIRadioComplexButton[] rbs = transform.parent.GetComponentsInChildren<UIRadioComplexButton>();
				foreach (UIRadioComplexButton rb in rbs)
				{
					if (rb != this)
					{
						rb.Set(false);
					}
				}
				if (makeAction && initd)
				{
					transform.localPosition = new Vector3(localPos.x + mVecMove.x, localPos.y + mVecMove.y, localPos.z + mVecMove.z);
				}
			}
			else
			{
				if (makeAction && initd)
				{
					transform.localPosition = localPos;
				}
			}
        }
    }

	void Reset()
	{
		UIRadioComplexButton[] rbs = transform.parent.GetComponentsInChildren<UIRadioComplexButton>();
		foreach (UIRadioComplexButton rb in rbs)
		{
			if (string.IsNullOrEmpty(ActiveSprite) == false)
			{
				if (target != null)
					target.spriteName = bActice ? ActiveSprite : UnactiveSprite;
			}
			if (m_ActiveObject != null)
			{
				m_ActiveObject.SetActive(bActice);
			}
			rb.transform.localPosition = rb.localPos;
		}
	}

	void SendAll(GameObject[] objects, string methodName, SendMessageOptions options)
	{
		if (objects != null)
		{
			foreach (GameObject obj in objects)
			{
				//obj.SendMessage(methodName, null, options);
				obj.SendMessage(methodName, options);
			}
		}
	}
}
