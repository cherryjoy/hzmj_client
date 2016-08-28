using System.IO;
using UnityEditor;
using UnityEngine;

public class SimpleStateMachineDataFilter : EditorWindow
{
    [MenuItem("LK-TOOL/DataFilter/MachineDataFilter")]
    public static void DetectorWindow()
    {
        EditorWindow.GetWindow(typeof(SimpleStateMachineDataFilter), true);
    }

    private void OnGUI()
    {
        GUILayout.Label("Please Select target data files");

        if (GUILayout.Button("TransferObjectFieldResourcePathName"))
        {
            System.Action<SimpleStateMachineData.SimpleAnimationEventData> handler = TransferObjectFieldResourcePathName;

            Object[] simpleStateMachineObjects = GetSelectedObjects(typeof(SimpleStateMachineData));
            Object[] overrideStateMachineObjects = GetSelectedObjects(typeof(OverrideSimpleStateMachineData));

            if (simpleStateMachineObjects != null)
            {
                for (int i = 0; i < simpleStateMachineObjects.Length; ++i)
                {
                    SimpleStateMachineData simpleStateMachineData = simpleStateMachineObjects[i] as SimpleStateMachineData;
                    FilterSimpleStateMachineData(simpleStateMachineData, handler);
                }
            }

            if (overrideStateMachineObjects != null)
            {
                for (int i = 0; i < overrideStateMachineObjects.Length; ++i)
                {
                    OverrideSimpleStateMachineData overrideSimpleStateMachineData = overrideStateMachineObjects[i] as OverrideSimpleStateMachineData;
                    FilterOverrideSimpleStateMachineData(overrideSimpleStateMachineData, handler);
                }    
            }

            Debug.Log("TransferObjectFieldResourcePathName");
        }

        if (GUILayout.Button("ClearObjectField"))
        {
            System.Action<SimpleStateMachineData.SimpleAnimationEventData> handler = ClearObjectField;

            Object[] simpleStateMachineObjects = GetSelectedObjects(typeof(SimpleStateMachineData));
            Object[] overrideStateMachineObjects = GetSelectedObjects(typeof(OverrideSimpleStateMachineData));

            if (simpleStateMachineObjects != null)
            {
                for (int i = 0; i < simpleStateMachineObjects.Length; ++i)
                {
                    SimpleStateMachineData simpleStateMachineData = simpleStateMachineObjects[i] as SimpleStateMachineData;
                    FilterSimpleStateMachineData(simpleStateMachineData, handler);
                }
            }

            if (overrideStateMachineObjects != null)
            {
                for (int i = 0; i < overrideStateMachineObjects.Length; ++i)
                {
                    OverrideSimpleStateMachineData overrideSimpleStateMachineData = overrideStateMachineObjects[i] as OverrideSimpleStateMachineData;
                    FilterOverrideSimpleStateMachineData(overrideSimpleStateMachineData, handler);
                }
            }

            Debug.Log("TransferObjectFieldResourcePathName");
        }

        if (GUILayout.Button("ReplacePlayAnimEffectFunctionName"))
        {
            System.Action<SimpleStateMachineData.SimpleAnimationEventData> handler = ReplacePlayAnimEffectFunctionName;

            Object[] simpleStateMachineObjects = GetSelectedObjects(typeof(SimpleStateMachineData));
            Object[] overrideStateMachineObjects = GetSelectedObjects(typeof(OverrideSimpleStateMachineData));

            if (simpleStateMachineObjects != null)
            {
                for (int i = 0; i < simpleStateMachineObjects.Length; ++i)
                {
                    SimpleStateMachineData simpleStateMachineData = simpleStateMachineObjects[i] as SimpleStateMachineData;
                    FilterSimpleStateMachineData(simpleStateMachineData, handler);
                }
            }

            if (overrideStateMachineObjects != null)
            {
                for (int i = 0; i < overrideStateMachineObjects.Length; ++i)
                {
                    OverrideSimpleStateMachineData overrideSimpleStateMachineData = overrideStateMachineObjects[i] as OverrideSimpleStateMachineData;
                    FilterOverrideSimpleStateMachineData(overrideSimpleStateMachineData, handler);
                }
            }

            Debug.Log("TransferObjectFieldResourcePathName");
        }

        if (GUILayout.Button("TransferInt2Field"))
        {
            System.Action<SimpleStateMachineData.SimpleAnimationEventData> handler = TransferInt2Field;

            Object[] simpleStateMachineObjects = GetSelectedObjects(typeof(SimpleStateMachineData));
            Object[] overrideStateMachineObjects = GetSelectedObjects(typeof(OverrideSimpleStateMachineData));

            if (simpleStateMachineObjects != null)
            {
                for (int i = 0; i < simpleStateMachineObjects.Length; ++i)
                {
                    SimpleStateMachineData simpleStateMachineData = simpleStateMachineObjects[i] as SimpleStateMachineData;
                    FilterSimpleStateMachineData(simpleStateMachineData, handler);
                }
            }

            if (overrideStateMachineObjects != null)
            {
                for (int i = 0; i < overrideStateMachineObjects.Length; ++i)
                {
                    OverrideSimpleStateMachineData overrideSimpleStateMachineData = overrideStateMachineObjects[i] as OverrideSimpleStateMachineData;
                    FilterOverrideSimpleStateMachineData(overrideSimpleStateMachineData, handler);
                }
            }

            Debug.Log("TransferInt2Field");
        }

        if (GUILayout.Button("SynchronizeEffectPropertyToPrefab"))
        {
            SynchronizeEffectPropertyToPrefab();

            Debug.Log("Synchronized");
        }
    }

    private void FilterSimpleStateMachineData(SimpleStateMachineData simpleStateMachineData, System.Action<SimpleStateMachineData.SimpleAnimationEventData> handler)
    {
        if (simpleStateMachineData != null)
        {
            int length = simpleStateMachineData.statesData.Length;
            for (int i = 0; i < length; ++i)
            {
                FilterSimpleStateData(simpleStateMachineData.statesData[i], handler);
            }    
        }

        EditorUtility.SetDirty(simpleStateMachineData);
    }

    private void FilterOverrideSimpleStateMachineData(OverrideSimpleStateMachineData overrideStateMachineData, System.Action<SimpleStateMachineData.SimpleAnimationEventData> handler)
    {
        if (overrideStateMachineData != null)
        {
            int length = overrideStateMachineData.override_events_data.Length;
            for (int i = 0; i < length; ++i)
            {
                handler(overrideStateMachineData.override_events_data[i]);
            }
        }

        EditorUtility.SetDirty(overrideStateMachineData);
    }
    
    private void FilterSimpleStateData(SimpleStateMachineData.SimpleStateData simpleStateData, System.Action<SimpleStateMachineData.SimpleAnimationEventData> handler)
    {
        if (simpleStateData != null)
        {
            int length = simpleStateData.eventDataList.Length;
            for (int i = 0; i < length; ++i)
            {
                handler(simpleStateData.eventDataList[i]);
            }
        }
    }

    private void TransferObjectFieldResourcePathName(SimpleStateMachineData.SimpleAnimationEventData simpleAnimationEventData)
    {
        if (simpleAnimationEventData != null)
        {
            ValidateObjectAssetPathNames(simpleAnimationEventData);

            int length = simpleAnimationEventData.objVals.Length;
            for (int i = 0; i < length; ++i)
            {
                if (simpleAnimationEventData.objVals[i] != null)
                {
                    string assetPathName = AssetDatabase.GetAssetPath(simpleAnimationEventData.objVals[i]);
                    string resourcePathName = GetResourePath(assetPathName);

                    simpleAnimationEventData.objectAssetPathNames[i] = resourcePathName;
                    simpleAnimationEventData.objVals[i] = null;
                }
            }
        }
    }

    private void ClearObjectField(SimpleStateMachineData.SimpleAnimationEventData simpleAnimationEventData)
    {
        if (simpleAnimationEventData != null)
        {
            int length = simpleAnimationEventData.objVals.Length;
            for (int i = 0; i < length; ++i)
            {
                if (simpleAnimationEventData.objVals[i] != null)
                {
                    simpleAnimationEventData.objVals[i] = null;
                }
            }
        }
    }

    private void TransferInt2Field(SimpleStateMachineData.SimpleAnimationEventData simpleAnimationEventData)
    {
        if (simpleAnimationEventData != null)
        {
            ValidateInt2AssetPathNames(simpleAnimationEventData);

            int length = simpleAnimationEventData.intVals2.Length;
            for (int i = 0; i < length; ++i)
            {
                if (simpleAnimationEventData.funcNames[i] == "PlayEffect" && simpleAnimationEventData.intVals2[i] != 0)
                {
                    ModelEffects modelEffects = new ModelEffects();
                    if (modelEffects.LoadConfig(simpleAnimationEventData.intVals2[i]))
                    {
                        simpleAnimationEventData.objectAssetPathNames[i] = modelEffects.name_;
                    }
                    else
                    {
                        Debug.Log(simpleAnimationEventData.intVals2[i] + "doesn't exist in table");
                    }
                }
            }
        }
    }
    
    private void ReplacePlayAnimEffectFunctionName(SimpleStateMachineData.SimpleAnimationEventData simpleAnimationEventData)
    {
        if (simpleAnimationEventData != null)
        {
            int length = simpleAnimationEventData.funcNames.Length;

            for (int i = 0; i < length; ++i)
            {
                if (!string.IsNullOrEmpty(simpleAnimationEventData.funcNames[i]))
                {
                    if (simpleAnimationEventData.funcNames[i].Trim() == "PlayAnimEffect")
                    {
                        simpleAnimationEventData.funcNames[i] = "PlayEffect";
                    }
                }
            }
        }
    }

    private void ValidateObjectAssetPathNames(SimpleStateMachineData.SimpleAnimationEventData simpleAnimationEventData)
    {
        if (simpleAnimationEventData != null)
        {
            int length = simpleAnimationEventData.objVals.Length;

            if (simpleAnimationEventData.objectAssetPathNames != null)
            {
                if (simpleAnimationEventData.objectAssetPathNames.Length < length)
                {
                    simpleAnimationEventData.objectAssetPathNames = new string[length];
                }
            }
        }
    }

    private void ValidateInt2AssetPathNames(SimpleStateMachineData.SimpleAnimationEventData simpleAnimationEventData)
    {
        if (simpleAnimationEventData != null)
        {
            int length = simpleAnimationEventData.intVals2.Length;

            if (simpleAnimationEventData.objectAssetPathNames != null)
            {
                if (simpleAnimationEventData.objectAssetPathNames.Length < length)
                {
                    simpleAnimationEventData.objectAssetPathNames = new string[length];
                }
            }
        }
    }

    private void SynchronizeEffectPropertyToPrefab()
    {
        string effectPathFormat = "Assets/Resources/{0}.prefab";
        int recordCount = CDataMgr.Effects.GetRecordCount();
        for (int i = 0; i < recordCount; ++i)
        {
            int id = CDataMgr.Effects.GetDataByNumber<int>(i, 0);

            ModelEffects modelEffects = new ModelEffects();
            modelEffects.LoadConfig(id);

            string effectPathName = string.Format(effectPathFormat, modelEffects.name_);
            GameObject effectGameObject = AssetDatabase.LoadAssetAtPath(effectPathName, typeof(GameObject)) as GameObject;
            if (effectGameObject != null)
            {
                EffectController effectController = effectGameObject.GetComponent<EffectController>();
                if (effectController == null)
                {
                    effectController = effectGameObject.AddComponent<EffectController>();
                }

                effectController.bindPosition = modelEffects.bind_pos_;
                effectController.canBreakOff = modelEffects.can_break_off_;
                effectController.canHitPause = modelEffects.can_hit_pause_;
                effectController.effectType = (int)modelEffects.effect_type_;
            }
        }
    }

    private string GetResourePath(string assetPathName)
    {
        string resourcePathName = string.Empty;

        if (!string.IsNullOrEmpty(assetPathName))
        {
            int resourceIndex = assetPathName.IndexOf("Resources");
            if (resourceIndex > 0)
            {
                resourceIndex += "Resources".Length + 1;
                resourcePathName = assetPathName.Substring(resourceIndex);

                string resourcePath = Path.GetDirectoryName(resourcePathName);
                string resourceName = Path.GetFileNameWithoutExtension(resourcePathName);
                resourcePathName = string.Concat(resourcePath, "/", resourceName);
            }
        }

        return resourcePathName;
    }

    private Object[] GetSelectedObjects(System.Type type)
    {
        Object[] selectedObjects = Selection.GetFiltered(type, SelectionMode.DeepAssets);

        int length = selectedObjects.Length;
        for (int i = 0; i < length; ++i)
        {
            Debug.Log(selectedObjects[i].name);
        }

        return selectedObjects;
    }
}
