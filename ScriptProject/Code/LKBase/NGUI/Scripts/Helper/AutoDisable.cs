using UnityEngine;
using System.Collections;

public class AutoDisable : MonoBehaviour
{
    public bool isParticle = false;
    public float waitTime = -1;//wait how many time to destroy obj, -1 mean forever
 
    void OnEnable()
    {
        if (isParticle)
        {
            ParticleSystem particleSystem = GetComponent<ParticleSystem>();
            WaitToDisableThis(particleSystem.duration + particleSystem.startLifetime);
        }
        else
        {
            if (waitTime > 0)
            {
                WaitToDisableThis(waitTime);
            }
        }
    }

    public void WaitToDisableThis(float second)
    {
        StartCoroutine(WaitToDisable(second));
    }

    IEnumerator WaitToDisable(float second)
    {
        yield return new WaitForSeconds(second);
        gameObject.SetActive(false);
    }

    void OnDisable()
    {
        StopCoroutine("WaitToDisable");
    }
}
