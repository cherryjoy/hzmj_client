using UnityEngine;
using System.Collections;

[AddComponentMenu("NGUI/Interaction/Button Out")]
public class UIButtonOutScript : MonoBehaviour
{
	
	public GameObject sprite;
    bool mLastPress = false;
	
	void OnEnable ()
	{
		OnPress (false);
        sprite.SetActive(false);
	}
	
	void OnPress (bool pressed)
	{
        if (mLastPress != pressed)
        {
            mLastPress = pressed;
            //sprite.SetActive(pressed);
            if (pressed)
            {
                GameObject particle = NGUITools.AddParticleByName("dianji01", transform.parent.gameObject);
                particle.transform.localPosition = transform.localPosition;
                //particle.AddComponent("DestroyWhenMove");
                //particle.AddComponent("AutoDestroy");
                //particle.AddComponent<DestroyWhenMove>();
                particle.AddComponent<AutoDestroy>();
                particle.GetComponent<AutoDestroy>().isParticle = true;
            }
        }
	}

}
