using UnityEngine;
using System.Collections;
using System;
public class LuaBehaviourGUI : LuaBehaviour 
{
	void OnGUI()
    {
        if (GUILayout.Button("ok") == true)
        {
            LuaInstance.instance.Get().L_DoString("Cube:SetActive(false)");
        }
    }
}
