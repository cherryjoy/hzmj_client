using UnityEngine;
using System.Collections;

[ExecuteInEditMode]
[AddComponentMenu("NGUI/UI/Image Button")]
public class UISelectButton : MonoBehaviour {

	public GameObject target;
	public GameObject select;
	public GameObject disable;
	private string m_BackParticlePath = "LiziEffects/Lizi/tubiao02";
	private GameObject m_BackParticle;
	
	private bool mEnable = true;

	
	void OnEnable()
	{
		target.gameObject.SetActive(true);
		select.gameObject.SetActive(false);
		disable.gameObject.SetActive(false);
		StartCoroutine(WaitToSetBack());
	}

	IEnumerator WaitToSetBack()
	{
		yield return 0;
		if (this != null && this.gameObject != null)
		{
			SetBackParticle(mEnable);
		}
	}
	
	void OnPress (bool pressed)
	{
		if(pressed) select.gameObject.SetActive(true);
		else select.gameObject.SetActive(false);
	}	
	
	public bool enable 
	{
		set 
		{
            BoxCollider box = GetComponent<Collider>() as BoxCollider;
			box.enabled = value;
			mEnable = value;	
			
			target.gameObject.SetActive(false);
		    disable.gameObject.SetActive(false);	
			
			if (mEnable == true) target.gameObject.SetActive(true);
			else disable.gameObject.SetActive(true);

			SetBackParticle(mEnable);
		}
		
		get { return mEnable; }
	}	

	void SetBackParticle(bool bEnable)
	{
		if (bEnable)
		{
			if (m_BackParticle == null)
			{
				m_BackParticle = NGUITools.AddChildByResourcesPath(m_BackParticlePath, gameObject);
			}
			NGUITools.SetActiveSelf(m_BackParticle, true);
			m_BackParticle.transform.localScale = new UnityEngine.Vector3(110f, 110f, 1);
			Vector3 tmpPos = m_BackParticle.transform.localPosition;
			tmpPos.y = 8;
			m_BackParticle.transform.localPosition = tmpPos;
			m_BackParticle.GetComponent<UIWidget>().depth = -3;
		}
		else
		{
            NGUITools.SetActiveSelf(m_BackParticle, false);
		}	
	}
	
	void OnDestroy()
	{
		if(target != null ) target = null;
		if(select != null) select = null;
		if(disable != null) disable = null;
	}
}
