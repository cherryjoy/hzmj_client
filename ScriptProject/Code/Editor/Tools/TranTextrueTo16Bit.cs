
using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Diagnostics;

public class TranTextrueTo16Bit : EditorWindow
{
    static List<string> AllDir = new List<string>();
    bool IsTransTexFinish = false;
    static string tranPath;
    [MenuItem("WonderTools/texturePacker &#z",
              false,
              1)]


    static void Init()
    {
        string dataPath = Application.dataPath.Replace('/', '\\');
        tranPath = dataPath + @"\Resources\UI\Atlas\Bit16";

        EditorWindow windows = EditorWindow.GetWindow<TranTextrueTo16Bit>(true, "TranTextrueTo16Bit");

    }

    void OnGUI() {
        tranPath = EditorGUILayout.TextField("Â·¾¶:", tranPath);
        if (GUILayout.Button("¿ªÊ¼Ñ¹Ëõ")) {
            DoStart();
        }
    }

    static void DoStart()
    {
        UnityEngine.Debug.Log("TranTestureTo16Bit tool is running!");

        DirectoryInfo Folder = new DirectoryInfo(tranPath);
        GetAllDirList(Folder);

        CheckAndRemove16BitTex();
        ChangeAllTexImportSetting();
        UnityEngine.Debug.Log("Change Textures ImportSetting Finish.");

        Thread newThread = new Thread(new ThreadStart(RunCmd));
        newThread.Start();
    }

    //travel all file in the directory
    static void GetAllDirList(DirectoryInfo directory)
    {
        foreach (FileInfo file in directory.GetFiles())
        {
            if (file.Name.EndsWith(".png"))
                AllDir.Add(file.FullName);
        }

        foreach (DirectoryInfo subDirectory in directory.GetDirectories())
        {
            GetAllDirList(subDirectory);
        }
    }

    static void RunCmd()
    {
        string commandLine = "";
        UnityEngine.Debug.Log("cmd running !");
        foreach (string fileFullName in AllDir)
        {
            UnityEngine.Debug.Log(fileFullName);
            string nameForTexPacker = fileFullName.Replace('\\', '/');
            commandLine = "TexturePacker --opt RGBA4444 --dither-fs-alpha --border-padding 0 --shape-padding 0 --allow-free-size --disable-rotation --no-trim --sheet " + nameForTexPacker;
            commandLine += " " + nameForTexPacker + "\n";
            //commandLine += "xcopy *.png " + fileFullName + " /r /y \n";
            UnityEngine.Debug.Log(commandLine);

            Process TranTexTo16Bit = new Process();
            TranTexTo16Bit.StartInfo.FileName = "cmd.exe";
            TranTexTo16Bit.StartInfo.Arguments = "/c " + commandLine;
            TranTexTo16Bit.StartInfo.UseShellExecute = false;
            TranTexTo16Bit.StartInfo.RedirectStandardInput = true;
            TranTexTo16Bit.StartInfo.RedirectStandardOutput = true;
            TranTexTo16Bit.StartInfo.RedirectStandardError = true;
            TranTexTo16Bit.StartInfo.CreateNoWindow = true;
            TranTexTo16Bit.Start();

            UnityEngine.Debug.Log(TranTexTo16Bit.StandardOutput.ReadToEnd());
        }

        UnityEngine.Debug.Log("------------------TranTextrueTo16Bit Process End!!!--------------------");

    }

    static void ChangeAllTexImportSetting()
    {
        foreach (string fileFullName in AllDir)
        {
            string nameForTexPacker = fileFullName.Replace('\\', '/');

            // 6 is the length of string "Assets"
            string AssetPath = nameForTexPacker.Substring(Application.dataPath.Length - 6);

            if (string.IsNullOrEmpty(AssetPath)) return;
            TextureImporter ti = AssetImporter.GetAtPath(AssetPath) as TextureImporter;
            if (ti == null) return;

            TextureImporterSettings settings = new TextureImporterSettings();
            ti.ReadTextureSettings(settings);

            settings.textureFormat = TextureImporterFormat.Automatic16bit;

            ti.SetTextureSettings(settings);
            AssetDatabase.ImportAsset(AssetPath, ImportAssetOptions.ForceUpdate);
        }
    }

    static void CheckAndRemove16BitTex()
    {
        for (int i = AllDir.Count - 1; i >= 0; i--)
        {
            string dir = AllDir[i] as string;
            string nameForTexPacker = dir.Replace('\\', '/');

            // 6 is the length of string "Assets"
            string AssetPath = nameForTexPacker.Substring(Application.dataPath.Length - 6);
            //UnityEngine.Debug.Log(Application.dataPath);
            if (string.IsNullOrEmpty(AssetPath)) return;
            TextureImporter ti = AssetImporter.GetAtPath(AssetPath) as TextureImporter;
            if (ti == null) continue;

            TextureImporterSettings settings = new TextureImporterSettings();
            ti.ReadTextureSettings(settings);
            UnityEngine.Debug.Log(AssetPath + " format: " + settings.textureFormat);
            if (settings.textureFormat == TextureImporterFormat.Automatic16bit)
            {
                AllDir.RemoveAt(i);
            }
        }
    }
}
