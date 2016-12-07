using UnityEngine;
using UnityEngine.SceneManagement;

[AddComponentMenu("NGUI/Examples/Load Level On Click")]
public class LoadLevelOnClick : MonoBehaviour
{
	public string levelName;

	void OnClick ()
	{
		if (!string.IsNullOrEmpty(levelName))
		{
            SceneManager.LoadScene(levelName);
			//Application.LoadLevel(levelName);
		}
	}
}