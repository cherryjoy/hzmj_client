using UnityEngine;
using System.Collections;

public class SceneNameReader : MonoBehaviour {
    void Awake()
    {
        SceneControl control = this.gameObject.GetComponent<SceneControl>();
        control.sceneName = ResourceLoader.SceneName;
    }
	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
	
	}
    void OnGUI()
    {
        if (GUILayout.Button("Return", GUILayout.Width(100), GUILayout.Height(50)))
        {
            Application.LoadLevel("downResource");
        }
    }
}
