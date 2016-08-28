using UnityEngine;
using System.Collections;
using UnityEditor;
using System;
using System.Reflection;
using System.Collections.Generic;

public class CarRelation : EditorWindow
{
    private GameObject m_findTarget;
    private List<GameObject> m_referenceList = new List<GameObject>();
    private Vector2 m_scrollPosition;
    private const int m_maxDetectLevel = 1;
    private int m_currentDetectLevel;

    [MenuItem("LK-TOOL/CarDebugSuite/RelationWindow &#r")]
    public static void RelationWindow()
    {
        EditorWindow.GetWindow(typeof(CarRelation), true);
    }

    public void OnGUI()
    {
        m_findTarget = (GameObject)EditorGUILayout.ObjectField("FindTarget:", m_findTarget, typeof(GameObject), true);
        if (GUILayout.Button("FindReference"))
        {
            m_referenceList.Clear();
            if (m_findTarget != null)
            {
                GameObject[] allGameObjects = (GameObject[])FindObjectsOfType(typeof(GameObject));
                foreach (GameObject singleGameObject in allGameObjects)
                {
                    Component[] components = singleGameObject.GetComponents(typeof(MonoBehaviour));
                    foreach (Component component in components)
                    {
                        if (component != null && m_findTarget != null)
                        {
                            if (DetectRelation(component, m_findTarget))
                            {
                                if (!m_referenceList.Contains(singleGameObject))
                                {
                                    m_referenceList.Add(singleGameObject);
                                }
                            }
                        }
                    }
                }
            }
        }

        EditorGUILayout.Separator();

        m_scrollPosition = EditorGUILayout.BeginScrollView(m_scrollPosition);
        foreach (GameObject referenceGameObject in m_referenceList)
        {
            if (referenceGameObject)
            {
                if (GUILayout.Button(referenceGameObject.name))
                {
                    Selection.activeGameObject = referenceGameObject;
                }
            }
        }
        EditorGUILayout.EndScrollView();
    }

    public bool DetectRelation(object componentObject, GameObject referenceGameObject)
    {
        Type componentType = componentObject.GetType();
        FieldInfo[] fields = componentType.GetFields(BindingFlags.Public | BindingFlags.Instance);

        Component[] referenceComponents = referenceGameObject.GetComponents<Component>();

        foreach (var customField in fields)
        {
            var customFieldValue = customField.GetValue(componentObject);

            if (customFieldValue is GameObject)
            {
                if ((GameObject)customFieldValue == referenceGameObject)
                {
                    return true;
                }
            }
            else if (customFieldValue is GameObject[])
            {
                foreach (GameObject itemGameObject in (GameObject[])customFieldValue)
                {
                    if (itemGameObject == referenceGameObject)
                    {
                        return true;
                    }
                }
            }
            else if (customFieldValue is Component)
            {
                foreach (Component itemComponent in referenceComponents)
                {
                    if ((Component)customFieldValue == itemComponent)
                    {
                        return true;
                    }
                }
            }
            else
            {
                if (customFieldValue is object[])
                {
                    foreach (var itemObject in (object[])customFieldValue)
                    {
                        if (itemObject != null && referenceGameObject != null && DetectRelation(itemObject, referenceGameObject))
                        {
                            return true;
                        }
                    }
                }
                else if (customFieldValue is object)
                {
                    ++m_currentDetectLevel;
                    if (m_currentDetectLevel <= m_maxDetectLevel)
                    {
                        if (customFieldValue != null && referenceGameObject != null && DetectRelation(customFieldValue, referenceGameObject))
                        {
                            return true;
                        }
                    }
                }
            }
        }

        return false;
    }

}
