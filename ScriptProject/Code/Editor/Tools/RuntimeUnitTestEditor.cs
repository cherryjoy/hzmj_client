using UnityEngine;
using System.Collections;
using UnityEditor;
using System.Collections.Generic;
using System;
using System.Reflection;

public class RuntimeUnitTestEditor : EditorWindow
{
    public class TestEntry
    {
        public Component component;
        public string scriptName;
        public string methodName;
    }
    public static RuntimeUnitTestEditor unitTestWindow;

    private Vector2 m_scrollPosition;
    private List<TestEntry> m_testEntryList;
    private List<TestEntry> m_uniqueEntryList;
    private List<bool> m_foldoutStateList;

    [MenuItem("LK-TOOL/CarDebugSuite/RuntimeUnitTestWindow &#u")]
    public static void RuntimeUnitTestWindow()
    {
        List<TestEntry> testEntryList = GetSceneTestEntries();
        RuntimeUnitTestEditor.unitTestWindow = EditorWindow.GetWindow(typeof(RuntimeUnitTestEditor), true) as RuntimeUnitTestEditor;
        RuntimeUnitTestEditor.unitTestWindow.Initialize(testEntryList);
    }

    public void Initialize(List<TestEntry> testEntryList)
    {
        m_testEntryList = testEntryList;
        int foldoutCount = GetUniqueComponentCount(testEntryList);
        m_uniqueEntryList = GetUniqueComponentList(testEntryList);
        m_foldoutStateList = new List<bool>(foldoutCount);
        for (int i = 0; i < foldoutCount; ++i)
        {
            m_foldoutStateList.Add(true);
        }
    }

    void OnGUI()
    {
        if (GUILayout.Button("Refresh"))
        {
            List<TestEntry> testEntryList = GetSceneTestEntries();
            RuntimeUnitTestEditor.unitTestWindow = EditorWindow.GetWindow(typeof(RuntimeUnitTestEditor), true) as RuntimeUnitTestEditor;
            RuntimeUnitTestEditor.unitTestWindow.Initialize(testEntryList);
        }

        m_scrollPosition = EditorGUILayout.BeginScrollView(m_scrollPosition);
        for (int i = 0; i < m_foldoutStateList.Count; ++i)
        {
            m_foldoutStateList[i] = EditorGUILayout.Foldout(m_foldoutStateList[i], m_uniqueEntryList[i].scriptName);
            if (m_foldoutStateList[i])
            {
                for (int j = 0; j < m_testEntryList.Count; ++j)
                {
                    if (m_testEntryList[j].component == m_uniqueEntryList[i].component)
                    {
                        if (GUILayout.Button(m_testEntryList[j].methodName))
                        {
                            Type scriptType = m_testEntryList[j].component.GetType();
                            scriptType.InvokeMember(
                             m_testEntryList[j].methodName,
                             BindingFlags.DeclaredOnly |
                             BindingFlags.Public |
                             BindingFlags.NonPublic |
                             BindingFlags.Instance |
                             BindingFlags.InvokeMethod,
                             null,
                             m_testEntryList[j].component,
                             null);
                        }
                    }
                }
            }
        }
        EditorGUILayout.EndScrollView();
    }

    public static List<TestEntry> GetSceneTestEntries()
    {
        List<TestEntry> testEntryList = new List<TestEntry>();
        GameObject[] hierarchyGameObjects = GameObject.FindObjectsOfType(typeof(GameObject)) as GameObject[];
        for (int i = 0; i < hierarchyGameObjects.Length; ++i)
        {
            MonoBehaviour[] gameObjectScripts = hierarchyGameObjects[i].GetComponents<MonoBehaviour>();
            for (int j = 0; j < gameObjectScripts.Length; ++j)
            {
                if (gameObjectScripts[j] != null)
                {
                    Type scriptType = gameObjectScripts[j].GetType();
                    MethodInfo[] scriptMethodInfos = scriptType.GetMethods(
                        BindingFlags.DeclaredOnly
                        | BindingFlags.Public
                        | BindingFlags.NonPublic
                        | BindingFlags.Instance);

                    foreach (MethodInfo methodInfo in scriptMethodInfos)
                    {
                        Attribute[] customAttributes = Attribute.GetCustomAttributes(methodInfo);
                        foreach (Attribute customAttribute in customAttributes)
                        {
                            if (customAttribute.GetType() == typeof(RuntimeTestAttribute))
                            {
                                TestEntry testEntry = new TestEntry() { component = gameObjectScripts[j], scriptName = string.Concat(gameObjectScripts[j].name, ".", scriptType.Name), methodName = methodInfo.Name };
                                testEntryList.Add(testEntry);
                            }
                        }
                    }
                }
            }
        }

        return testEntryList;
    }

    public static int GetUniqueComponentCount(List<TestEntry> testEntryList)
    {
        int uniqueComponentCount = 0;
        for (int i = 0; i < testEntryList.Count; ++i)
        {
            if (i == 0)
            {
                ++uniqueComponentCount;
            }
            for (int j = 0; j < i; ++j)
            {
                if (testEntryList[j].component == testEntryList[i].component)
                {
                    break;
                }
                if (j == i - 1)
                {
                    ++uniqueComponentCount;
                }
            }
        }

        return uniqueComponentCount;
    }

    public static List<TestEntry> GetUniqueComponentList(List<TestEntry> testEntryList)
    {
        List<TestEntry> uniqueComponentList = new List<TestEntry>();
        for (int i = 0; i < testEntryList.Count; ++i)
        {
            if (i == 0)
            {
                uniqueComponentList.Add(testEntryList[i]);
            }
            for (int j = 0; j < i; ++j)
            {
                if (testEntryList[j].component == testEntryList[i].component)
                {
                    break;
                }
                if (j == i - 1)
                {
                    uniqueComponentList.Add(testEntryList[i]);
                }
            }
        }

        return uniqueComponentList;
    }
}
