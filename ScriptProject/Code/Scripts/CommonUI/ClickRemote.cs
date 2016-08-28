using UnityEngine;
using System.Collections;

public class ClickRemote: MonoBehaviour
{
    public LuaBehaviour EndEventReciver;
    public Transform ClickViewer;//sth visible poiont to remoting thing
    public GameObject Target {
        get
        {
            return m_Target;
        }

        set
        {
            StopAllCoroutines();
            m_Target = value;

            //set collider
            BoxCollider targetCollider = m_Target.GetComponent<BoxCollider>();
            if (targetCollider != null)
            {
                m_Collider.size = Vector3.Scale(targetCollider.size, m_Target.transform.localScale);
                Vector3 rightCenter = targetCollider.center;
                rightCenter.z = m_Collider.center.z;
                m_Collider.center = rightCenter;
            }
            Update();
        }
    }
	private GameObject m_Target;
	private BoxCollider m_Collider;
	private Transform m_Trans;
    public Vector2 viewerOffset = Vector2.zero;

	void Awake()
	{
		m_Collider = GetComponent<BoxCollider>();
		m_Trans = transform;
	}

    public void SetViewrOffset(float x, float y) {
        viewerOffset = new Vector2(x, y);
    }

    void Update() {
            //set position
        if (m_Target != null) {
            Vector3 v = m_Target.transform.position;
            m_Trans.position = v;
            v = m_Trans.localPosition;
            v.x += viewerOffset.x;
            v.y += viewerOffset.y;
            v.z = 0.0f;
            m_Trans.localPosition = v;
            ClickViewer.position = m_Trans.position;

            v.x -= viewerOffset.x;
            v.y -= viewerOffset.y;
            m_Trans.localPosition = v;
        }
    }

	void OnClick()
	{
		if (m_Target == null)
			return;
		m_Target.SendMessage("OnClick", m_Target, SendMessageOptions.DontRequireReceiver);
        StartCoroutine(WaitForOneFrameToEndClick());
	}

    IEnumerator WaitForOneFrameToEndClick()
    {
        yield return 0;
        EndEventReciver.CallFunction(gameObject, "_ClickEnd");
    }

	void OnPress(object obj)
	{
		if (m_Target != null)
		{
			m_Target.SendMessage("OnPress", obj, SendMessageOptions.DontRequireReceiver);
		}
	}

	void OnSelect(object obj)
	{
		if (m_Target != null)
		{
			m_Target.SendMessage("OnSelect", obj, SendMessageOptions.DontRequireReceiver);
		}
	}

	void OnHover(object obj)
	{
		if (m_Target != null)
		{
			m_Target.SendMessage("OnHover", obj, SendMessageOptions.DontRequireReceiver);
		}
	}
}
