using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

public class CommandBuild
{
    public static void Build()
    {
        string target_path = getParam("target_path").Replace("\\", "/");
        BuildTarget platform = getBuildTarget(getParam("platform"));
        bool lua_debug = stringToBool(getParam("debug"));
        bool assetbundle = stringToBool(getParam("assetbundle"));
        bool clean = stringToBool(getParam("clean"));
        bool build_lua = stringToBool(getParam("build_lua"));
        bool export_data = stringToBool(getParam("export_data"));
        bool csharp_script = stringToBool(getParam("csharp_script"));
        bool set_bundlename = stringToBool(getParam("set_bundlename"));

        OneKeyBuild(target_path, platform, lua_debug, assetbundle, clean, build_lua, export_data, csharp_script, set_bundlename);    
    }

    public static void OneKeyBuildWindows()
    {
        OneKeyBuild("F:\\MaJiang\\hzmj_client", BuildTarget.StandaloneWindows, false, true, false, true, true, true, true);
    }

    static void OneKeyBuild(string target_path, BuildTarget platform, bool lua_debug, bool assetbundle, 
        bool clean, bool build_lua, bool export_data, bool csharp_script, bool set_bundlename)
    {
        Debug.Log("BuildTarget-" + platform.ToString() + "\n" +
               "target_path-" + target_path);

        BuildAssetBundle window = EditorWindow.GetWindow<BuildAssetBundle>();
        window.InitPath();
        window.SwitchPlatform(platform);

        //create dest path.
        CreateFolder(window.dest_path);

        if (assetbundle)
        {
            if (clean)  //clear assetbundle path
            {
                FileUtil.DeleteFileOrDirectory(window.assetbundles_path);
                CreateFolder(window.assetbundles_path);
            }

            //0.export dependency.
            if (csharp_script)
            {
                window.BuildCSharpScript();
            }

            //1.build Assetbundle.
            window.BuildAssetBundles(set_bundlename);
        }

        //2.compile lua code
        if (build_lua)
        {
            FileUtil.DeleteFileOrDirectory(window.export_path);
            CreateFolder(window.export_path);

            window.BuildLua(lua_debug);
        }

        //3.
        if (export_data)
            window.PakAll();

        //4.copy asset to target_path
        //assetbundles
        CopyToTarget(window, target_path);   
    }

    static void CopyToTarget(BuildAssetBundle window, string target_path)
    {
        List<string> bundles = new List<string>();
        //bundles = GetAssetbundles(window.bundle_crc_name, window.export_path);
        CopyDir(window.assetbundles_path, target_path + "/assetbundles", bundles);
        
        //lua
        string csharp_script_path = Path.Combine(window.dest_path, "csharp_script").Replace("\\", "/");
        bundles.Add(csharp_script_path);
        CopyDir(window.export_path, Path.Combine(target_path, "Assets"), bundles);

        //csharp_script
        //File.Copy(csharp_script_path, Path.Combine(target_path, "csharp_script"), true);

        List<string> empty = new List<string>();
        //Dest
        CopyDir(window.dest_path, Path.Combine(target_path, "Dest"), empty);

        //StreamingAssets/Data
        CopyDir(Path.Combine(Application.streamingAssetsPath, "Data"), Path.Combine(target_path, "Assets\\Data"), empty);

        //StreamingAssets/Textures
        CopyDir(Path.Combine(Application.streamingAssetsPath, "Textures"), Path.Combine(target_path, "Assets\\Textures"), empty);
    }

    static BuildTarget getBuildTarget(string buildTarget)
    {
        if (buildTarget.Equals("android"))
            return BuildTarget.Android;
        else if (buildTarget.Equals("ios"))
            return BuildTarget.iOS;
        else if (buildTarget.Equals("wp"))
            return BuildTarget.WP8Player;
      
        return BuildTarget.StandaloneWindows;
    }

    static bool stringToBool(string value)
    {
        if (value.Equals("1"))
            return true;
        return false;
    }

    static string getParam(string paramName)
    {
        string ret = "";
        char[] separator = new char[] { '-' };
        foreach (string arg in System.Environment.GetCommandLineArgs())
        {
            if (arg.StartsWith(paramName))
            {
                ret = arg.Split(separator)[1];
                break;
            }
        }
        return ret;
    }

    static void CreateFolder(string path)
    {
        if (Directory.Exists(path) == false)
            Directory.CreateDirectory(path);
    }

    static void CopyDir(string src, string dest, List<string> exclude_files)
    {
        UnityEngine.Debug.Log("src: " + src + ", dest: " + dest);
        if (Directory.Exists(dest))
            Directory.Delete(dest, true);

        DirectoryInfo di = new DirectoryInfo(src);
        if (di.Exists == true)
        {
            Directory.CreateDirectory(dest);

            foreach (FileSystemInfo fsi in di.GetFileSystemInfos())
            {
                string destname = Path.Combine(dest, fsi.Name);

                if (fsi is FileInfo)
                {
                    if (fsi.Name.EndsWith(".meta") || fsi.Attributes == FileAttributes.Hidden)
                        continue;

                    string temp_full_name = fsi.FullName.Replace("\\", "/");
                    if (exclude_files != null && exclude_files.Contains(temp_full_name))
                    {
                        //Console.WriteLine("exclude_files." + fsi.FullName);
                        continue;
                    }

                    File.Copy(fsi.FullName, destname, true);
                }
                else
                {
                    Directory.CreateDirectory(destname);
                    CopyDir(fsi.FullName, destname, exclude_files);
                }
            }
        }
    }

    static List<string> GetAssetbundles(string file_name, string root_path)
    {
        List<string> bundles = new List<string>();

        if (File.Exists(file_name))
        {
            string[] list;

            StreamReader sr = new StreamReader(file_name);
            string s = sr.ReadLine();
            while (s != null)
            {
                list = s.Split(':');
                if (list.Length == 2)
                {
                    string fullname = Path.Combine(root_path, list[0]).Replace("\\", "/");
                    bundles.Add(fullname);
                }
                s = sr.ReadLine();
            }
        }

        return bundles;
    }

    static void CopyFiles(List<string> filenames, string dest)
    {
        if (Directory.Exists(dest))
            Directory.Delete(dest, true);

        Directory.CreateDirectory(dest);
        foreach (string fullname in filenames)
        {
            if (File.Exists(fullname))
            {
                string name = fullname.Substring(fullname.LastIndexOf("/") + 1);
                string destname = Path.Combine(dest, name);
                //Console.WriteLine("copy file." + fullname + " to " + destname);
                File.Copy(fullname, destname);
            }
            else
            {
                Debug.Log(fullname + " do not exist! \n");
            }
        }
    }

    [MenuItem("CJ-TOOL/Show All Components")]
    public static void ShowAllComponentsInGameObject()
    {
        HashSet<string> collects = new HashSet<string>();

        GameObject o = Selection.activeObject as GameObject;
        MonoBehaviour[] comps = o.GetComponents<MonoBehaviour>();

        foreach (var c in comps)
        {
            if (c == null)
                UnityEngine.Debug.Log(o.name);
            else
                collects.Add(c.GetType().Name);
        }

        comps = o.GetComponentsInChildren<MonoBehaviour>(true);
        foreach (var c in comps)
        {
            if (c == null)
                UnityEngine.Debug.Log(o.name);
            else
                collects.Add(c.GetType().Name);
        }

        List<string> sort_com = new List<string>(collects);
        sort_com.Sort();
        Debug.Log("collects.count" + sort_com.Count);

        string root_path = Path.Combine(Directory.GetCurrentDirectory(), "Test");
        if (!Directory.Exists(root_path))
            Directory.CreateDirectory(root_path);

        string file_name = Path.Combine(root_path, "all.txt");
        if (File.Exists(file_name))
            File.Delete(file_name);

        StreamWriter fs = new StreamWriter(file_name);
        foreach (var com in sort_com)
        {
            fs.WriteLine(com);
        }
        fs.Close();
    }
}

