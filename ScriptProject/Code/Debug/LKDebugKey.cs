#if UNITY_STANDALONE_WIN
using UnityEngine;
using System.Collections;
using UniLua;

public class LKDebugKey : MonoBehaviour
{
    bool autoFight = false;
    bool lockMove = true;
	void Update () 
	{
		if (Input.GetKeyDown(KeyCode.Minus))
		{
			Time.timeScale -= 0.1f;
			if (Time.timeScale < 0)
				Time.timeScale = 0;
		}

		if (Input.GetKeyDown(KeyCode.Equals))
		{
			Time.timeScale += 0.1f;
			if (Time.timeScale > 4)
				Time.timeScale = 1;
		}

		if (Input.GetKeyDown(KeyCode.F10))
		{
			Debug.Log(Time.timeScale);
			if (Time.timeScale > 0)
				Time.timeScale = 0;
			else
				Time.timeScale = 1;
		}

        if(Input.GetKeyDown(KeyCode.Delete))
        {
            PlayerPrefs.DeleteAll();
            PlayerPrefs.Save();
        }
	}
}
#endif
