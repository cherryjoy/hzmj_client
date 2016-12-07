using UnityEngine;
using System.Collections;

[System.Serializable]
public class UIMaterialInfo
{
	public string m_Name;
	public string m_Path;
}

public class UIOrder : MonoBehaviour {
	public UIMaterialInfo[] UIMaterialInfos;
	// Use this for initialization
	void Awake () {
		enabled = false;
	}
}
