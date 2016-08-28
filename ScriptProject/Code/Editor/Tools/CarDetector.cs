using UnityEngine;
using System.Collections;
using UnityEditor;
using System;
using System.Reflection;
using System.Collections.Generic;

public class CarDetector : EditorWindow
{
    public enum FindTarget
    {
        ScriptContainer,
        GameObjectContainer
    }

    private string m_componentName="";
    private List<GameObject> m_specifiedGameObject = new List<GameObject>();
    private Vector2 m_scrollPosition;

    [MenuItem("LK-TOOL/CarDebugSuite/DetectorWindow &#d")]
    public static void DetectorWindow()
    {
        EditorWindow.GetWindow(typeof(CarDetector), true);
    }

    private void OnGUI()
    {
        m_componentName = EditorGUILayout.TextField("ComponentName:", m_componentName);
        if (GUILayout.Button("Find"))
        {
            m_specifiedGameObject.Clear();
            Assembly[] assemblies = AppDomain.CurrentDomain.GetAssemblies();
            foreach (var item in assemblies)
            {
                Type anyType = item.GetType(m_componentName,false,true);
                if (anyType != null)
                {
                    GameObject[] allGameObjects = (GameObject[])FindObjectsOfType(typeof(GameObject));
                    foreach (GameObject singleGameObject in allGameObjects)
                    {
                        Component[] components = singleGameObject.GetComponents(anyType);
                        foreach (Component component in components)
                        {
                            m_specifiedGameObject.Add(component.gameObject);
                        }
                    }
                    break;
                }
            }
        }

        EditorGUILayout.Separator();

        m_scrollPosition = EditorGUILayout.BeginScrollView(m_scrollPosition);
        foreach (GameObject item in m_specifiedGameObject)
        {
            if (item!=null)
            {
                if (GUILayout.Button(item.name))
                {
                    Selection.activeGameObject = item;
                }
            }
        }
        EditorGUILayout.EndScrollView();

    }

}
