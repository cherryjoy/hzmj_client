using UnityEngine;
using System.Collections;

public class AutoDestroy : MonoBehaviour {
    public bool isParticle = false;
    public float waitTime = -1;//wait how many time to destroy obj, -1 mean forever

    void Start()
    {
        if (isParticle)
        {
            ParticleSystem particleSystem = GetComponent<ParticleSystem>();
            WaitToDestroyThis(particleSystem.duration + particleSystem.startLifetime);
        }
        else
        {
            if (waitTime > 0)
            {
                WaitToDestroyThis(waitTime);
            }
        }
    }

	public void WaitToDestroyThis(float second)
	{
		StartCoroutine(WaitToDestroy(second));
	}

	IEnumerator WaitToDestroy(float second)
	{
		yield return new WaitForSeconds(second);
		Destroy(gameObject);
	}

	void OnDisable()
	{
        if (!isParticle){
            GameObject.Destroy(gameObject);
        }
        else  
        {
            for(int i =0 ;i<transform.childCount;i++){
                transform.GetChild(i).gameObject.SetActive(false);
            } 
        }
            
        StopCoroutine("WaitToDestroy");
	}
}
