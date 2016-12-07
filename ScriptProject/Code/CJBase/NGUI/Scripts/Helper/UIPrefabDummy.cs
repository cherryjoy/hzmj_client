using UnityEngine;
using System.Collections;

public class UIPrefabDummy : MonoBehaviour {
	
	public string WidgetName;
	private GameObject mWidget;
	public bool DestroyWhenDisable = true;
	public bool SendSelfToChild = false;
	public bool UseFullPath;
    public bool IsAddUnhideParentAuto = true;
	public Vector3 Position = Vector3.zero;
	
	public delegate void OnLoad<T>(T t);

	public bool AllowParentTrigger = true;//If allow parent node's visibility change to trigger UIPrefabDummy
	private bool mParentHide = false;

	void OnEnable()
	{
		if (!AllowParentTrigger)
		{
			if (mParentHide)
			{
				mParentHide = false;
				return;
			}
		}

		InitPrefab();
	}
	
	
	public void Open<Tt>(OnLoad<Tt> onload) where Tt : UnityEngine.Component
	{
		gameObject.SetActive(true);
		
		InitPrefab();
		
		if (onload != null)
		{
			Tt t = mWidget.GetComponent<Tt>();
			onload(t);
		}
	}

    public void OpenNPC<Tt>(OnLoad<Tt> onload) where Tt : UnityEngine.Component
    {
        gameObject.SetActive(true);

        InitNPCPrefab();

        if (onload != null)
        {
            Tt t = mWidget.GetComponent<Tt>();
            onload(t);
        }
    }

    void InitNPCPrefab()
    {
        if (mWidget == null)
        {
            if (string.IsNullOrEmpty(WidgetName) == false)
            {
				string path = !UseFullPath ? GetUIPrefabPath(WidgetName) : WidgetName;

				GameObject prefab = (GameObject)ResLoader.Load(path, typeof(GameObject));
                mWidget = NGUITools.AddChild(gameObject, prefab);
                if (SendSelfToChild)
                {
                    mWidget.SendMessage("OnMessageFromParent", gameObject, SendMessageOptions.DontRequireReceiver);
                }
            }
            else
            {
                Debug.Log("WidgetName is null");
            }
        }
    }
	
	void InitPrefab()
	{
		if (mWidget == null)
		{
            if (string.IsNullOrEmpty(WidgetName) == false)
            {
				string path = !UseFullPath ? GetUIPrefabPath(WidgetName) : WidgetName;

				GameObject prefab = (GameObject)ResLoader.Load(path, typeof(GameObject));
                mWidget = NGUITools.AddChild(gameObject, prefab);
                mWidget.transform.localPosition = Position;
                if (IsAddUnhideParentAuto)
                {
                    UIHideParent hideParent = mWidget.GetComponent<UIHideParent>();
                    if (hideParent == null)
                    {
                        mWidget.AddComponent<UIHideParent>();
                    }
                }

                if (SendSelfToChild)
                {
                    mWidget.SendMessage("OnMessageFromParent", gameObject, SendMessageOptions.DontRequireReceiver);
                }
            }
            else
            {
                Debug.Log("WidgetName is null");
            }
		}
	}

	void OnHideParentClose()
	{
		if (mWidget != null)
		{
			if (DestroyWhenDisable == true)
				GameObject.Destroy(mWidget);
		}

		if (gameObject.activeSelf)
			StartCoroutine(DisableSelf());
	}

	IEnumerator DisableSelf()
	{
		yield return 1;
		gameObject.SetActive(false);
	}


	public static string GetUIPrefabPath(string subPath)
	{
        return "UI/Prefab/" + subPath;
	}

	void OnDisable()
	{
		if (gameObject.activeSelf == true && gameObject.activeInHierarchy == false)
			mParentHide = true;

		if (mWidget != null)
		{
			if (DestroyWhenDisable == true)
				GameObject.Destroy(mWidget);
		}
	}
	
	void OnDestroy()
	{
		mWidget = null;		
	}
}
