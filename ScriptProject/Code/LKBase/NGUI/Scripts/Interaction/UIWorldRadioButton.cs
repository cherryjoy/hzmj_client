using UnityEngine;

[ExecuteInEditMode]
[AddComponentMenu("NGUI/UI/World Radio Button")]
public class UIWorldRadioButton : MonoBehaviour
{
	public UISprite target;
	public string ActiveSprite;
	public float ActiveScaleX;
	public float ActiveScaleY;
    public float ActiveAlpha = 1;
	public string UnactiveSprite;
	public float UnactiveScaleX;
	public float UnactiveScaleY;
    public float UnactiveAlpha = 1;
	public bool option = true;
	public bool Startstate = false;
	private bool bActice;
	public object user_data;
	public GameObject[] user_data_recvs;
	public bool Auto = true;
	public bool ClickForbidOnDataEmpty = false;
	public string user_data_send_name = "OnChoosedButton";
	public GameObject AttachShow;

	public bool check
	{
		set { Set(value); }
	}

	void OnEnable()
	{
		if (target == null)
			target = GetComponentInChildren<UISprite>();
	}

	void Start()
	{
		if (Auto)
			Set(Startstate);
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
			target.spriteName = bActice ? ActiveSprite : UnactiveSprite;
            float trueAlpha = bActice ? ActiveAlpha : UnactiveAlpha;
            Color nowColor = new Color(target.color.r, target.color.g, target.color.b, trueAlpha);
            target.color = nowColor;
			target.MakePixelPerfect();
			if (ActiveScaleX > 0 && ActiveScaleY > 0 && UnactiveScaleX > 0 && UnactiveScaleY > 0)
			{
				Vector3 scale = target.transform.localScale;
				scale.x = bActice ? ActiveScaleX : UnactiveScaleX;
				scale.y = bActice ? ActiveScaleY : UnactiveScaleY;
				target.transform.localScale = scale;
			}

			if (option && bActice == true)
			{
				UIWorldRadioButton[] rbs = FindObjectsOfType<UIWorldRadioButton>();
				foreach (UIWorldRadioButton rb in rbs)
					if (rb != this)
						rb.Set(false);
			}

			if (AttachShow != null)
				AttachShow.SetActive(state);

		}
	}

	void OnReset()
	{
		Set(Startstate);
	}

	void OnChooseButton()
	{
		Set(true);
	}

	void OnClick()
	{
		if (bActice == false)
		{
			if (ClickForbidOnDataEmpty == false || user_data != null)
				Set(true);
		}
	}
}
