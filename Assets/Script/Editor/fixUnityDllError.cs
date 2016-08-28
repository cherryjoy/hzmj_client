using UnityEditor;
using UnityEngine;
using System.Text.RegularExpressions;
using System.IO;
using System.Text;

public class ReimportUnityEngineUI : EditorWindow
{
    [MenuItem("LK-TOOL/Reimport UI Assemblies")]
    public static void ReimportUI()
    {
        string engineDll = string.Format( EditorApplication.applicationContentsPath + "/UnityExtensions/Unity/GUISystem/{1}", string.Empty, "UnityEngine.UI.dll");
        ReimportDll( engineDll );

        engineDll = string.Format( EditorApplication.applicationContentsPath + "/UnityExtensions/Unity/GUISystem/Editor/{1}", string.Empty, "UnityEditor.UI.dll");
        ReimportDll( engineDll );

        engineDll = string.Format( EditorApplication.applicationContentsPath + "/UnityExtensions/Unity/Networking/{1}", string.Empty, "UnityEngine.Networking.dll");
        ReimportDll( engineDll );

        engineDll = string.Format( EditorApplication.applicationContentsPath + "/UnityExtensions/Unity/Networking/Editor/{1}", string.Empty, "UnityEditor.Networking.dll");
        ReimportDll( engineDll );

        engineDll = string.Format( EditorApplication.applicationContentsPath + "/UnityExtensions/Unity/Advertisements/{1}", string.Empty, "UnityEngine.Advertisements.dll");
        ReimportDll( engineDll );

        engineDll = string.Format( EditorApplication.applicationContentsPath + "/UnityExtensions/Unity/Advertisements/Editor/{1}", string.Empty, "UnityEditor.Advertisements.dll");
        ReimportDll( engineDll );

        engineDll = string.Format( EditorApplication.applicationContentsPath + "/UnityExtensions/Unity/EditorTestsRunner/Editor/{1}", string.Empty, "UnityEditor.EditorTestsRunner.dll");
        ReimportDll( engineDll );

        engineDll = string.Format( EditorApplication.applicationContentsPath + "/UnityExtensions/Unity/EditorTestsRunner/Editor/{1}", string.Empty, "nunit.framework.dll");
        ReimportDll( engineDll );

        engineDll = string.Format( EditorApplication.applicationContentsPath + "/UnityExtensions/Unity/UnityAnalytics/{1}", string.Empty, "UnityEngine.Analytics.dll");
        ReimportDll( engineDll );

        engineDll = string.Format( EditorApplication.applicationContentsPath + "/UnityExtensions/Unity/UnityPurchasing/{1}", string.Empty, "UnityEngine.Purchasing.dll");
        ReimportDll( engineDll );

        engineDll = string.Format( EditorApplication.applicationContentsPath + "/UnityExtensions/Unity/TreeEditor/Editor/{1}", string.Empty, "UnityEditor.TreeEditor.dll");
        ReimportDll( engineDll );
    }

    static void ReimportDll(string path)
    {
        if ( File.Exists( path ) )
            AssetDatabase.ImportAsset( path, ImportAssetOptions.ForceUpdate| ImportAssetOptions.DontDownloadFromCacheServer );
        else
            Debug.LogError( string.Format( "DLL not found {0}", path ) );
    }
}
