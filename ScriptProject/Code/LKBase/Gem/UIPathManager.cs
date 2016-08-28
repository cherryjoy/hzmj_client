using UnityEngine;
using System.Collections;
using System.IO;

public class UIPathManager  {
    public static string GetStreamPath() {
        string filePath = null;
#if  UNITY_ANDROID
		filePath = PluginTool.SharedInstance().PersisitentDataPath;
#else
        filePath = Application.streamingAssetsPath +"/";
#endif
        return filePath;
    }
}
