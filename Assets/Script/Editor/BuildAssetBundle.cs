using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Diagnostics;
using System.Reflection;
using System.Runtime.InteropServices;

public class BuildAssetBundle : EditorWindow
{
    BuildTarget platform_ = EditorUserBuildSettings.activeBuildTarget;
	[MenuItem("CJ-TOOL/BuildAssetBundle")]
    static void OpenWindow()
    {
        BuildAssetBundle window = GetWindow<BuildAssetBundle>();
        window.title = "Bundle Builder";
        window.InitPath();
        window.Show();
    }

    [MenuItem("CJ-TOOL/一键导出资源")]
    static void OneKeyBuildWin()
    {
        CommandBuild.OneKeyBuildWindows();
    }

    bool lua_debug_ = false;

    // export the bundle path
    string export_path_ = string.Empty;

    // dest file path
    string dest_path_ = string.Empty;

    // script file path
    string script_path_ = string.Empty;

    // unity project Resources path
    string resource_path_ = string.Empty;

    // build the assetbundles path
    string assetbundles_path_ = string.Empty;
    
    // project path
    string project_path_ = string.Empty;

    Dictionary<int, string> asset_hash_dict_ = new Dictionary<int, string>();

    const int LOAD_BITSIT = 1;
    //pack with ext into one bundle
    //*_unload：不会直接加载  load:会直接加载
    enum bundle_combine_type:int
    {
        all_in_one_unload = (1 << 1),  //所有的合成一个文件
        all_in_one_load = (1 << 2) | LOAD_BITSIT,

        single_dir_unload = (1 << 3),   //单个目录合成一个
        single_dir_load = (1 << 4) | LOAD_BITSIT,    

        top_dir_unload = (1 << 5),     //子目录（仅一层）所有的合成一个
        top_dir_load = (1 << 6) | LOAD_BITSIT,
        
        single_load = ( 1 << 7) | LOAD_BITSIT ,        //单个文件
        single_unload = ( 1 << 8 ),

        character = ( 1 << 9 ) | LOAD_BITSIT,          //角色（合并prefab与anim)
        scene = ( 1 << 10) | LOAD_BITSIT,              //场景

        atlas = ( 1 << 11) | LOAD_BITSIT,       //图集
    }

    bool[] select_build_combine_ = new bool[0];
    readonly BundleCombineNode[] combine_bunldes_ = new BundleCombineNode[]
    {    
        //UI
        new BundleCombineNode("UI/Prefab", "*.prefab", bundle_combine_type.single_dir_load),
        new BundleCombineNode("UI/Prefab", "*.mat", bundle_combine_type.single_dir_unload),
        new BundleCombineNode("UI/Atlas/Bit32", "*.prefab", bundle_combine_type.atlas),
        new BundleCombineNode("UI/Atlas/Bit16", "*.prefab", bundle_combine_type.atlas),

        new BundleCombineNode("UI/Icon", "*.png;*.jpg", bundle_combine_type.single_dir_load),
        new BundleCombineNode("UI/Icon/ItemIcon", "*.png;*.jpg", bundle_combine_type.single_load),

        new BundleCombineNode("UI/Res", "*.asset", bundle_combine_type.all_in_one_load),
        new BundleCombineNode("UI/Texture", "*.tga;*.jpg;*.png", bundle_combine_type.all_in_one_load),
        new BundleCombineNode("UI/Texture", "*.mat", bundle_combine_type.all_in_one_unload),

        new BundleCombineNode("UI/Fonts", "*.ttf", bundle_combine_type.single_load),

        //Music
        new BundleCombineNode("Music", "*.wav;*.mp3", bundle_combine_type.single_load),

        //Material
        new BundleCombineNode("Material", "*.mat", bundle_combine_type.all_in_one_load),
        new BundleCombineNode("Material", "*.tga;*.png", bundle_combine_type.all_in_one_load),

        //shader
        new BundleCombineNode("Shader", "*.shader;*.cginc", bundle_combine_type.all_in_one_unload, 1, "shader"),
    };

    //需要单独加载的文件
    Dictionary<string, string> single_bundles_ = new Dictionary<string, string>
    {
        //<src_file, dest_file>
        {"UI/Prefab/ToolsUI/MessageBox.prefab|UI/Prefab/ToolsUI/MessageBox.prefab", "UI/Prefab/ToolsUI/MessageBox"},
    };

    string[] scene_path_ = {"Assets/Login.unity", 
                            "Assets/scene.unity", 
                            "Assets/GameHall.unity"};

    string script_bundle_name_ = "all.prefab";
    string script_bundle_list_ = "all.txt";
    string bundle_crc_name_ = "bundle_crc";
    string asset_dependency_name_ = "asset_dependency";
    string asset_hash_name_ = "asset_hash";

    Dictionary<string, AssetBundle> load_bundle_ = new Dictionary<string, AssetBundle>();

    Dictionary<string, string> asset_dependency_ = new Dictionary<string, string>();

    Dictionary<string, string> assets_to_assetbundle_ = new Dictionary<string, string>();

    Dictionary<string, uint> bundle_crc_ = new Dictionary<string, uint>();

    enum DrawGUI
    {
        Build,
        Dependency,
        View,
        Tools
    }

    public void InitPath()
    {
        project_path_ = System.Environment.CurrentDirectory;

        export_path_ = Application.dataPath;
        export_path_ = export_path_.Substring(0, export_path_.Length - "Assets".Length) + "Exports";
        CreateFolder(export_path_);

        resource_path_ = Application.dataPath;
        resource_path_ = Path.Combine(resource_path_, "Resources") + "/";
        resource_path_ = resource_path_.Replace("\\", "/");

        script_path_ = Application.dataPath;
        script_path_ += "/Plugins/";

        // file path init
        dest_path_ = Application.dataPath;
        dest_path_ = dest_path_.Substring(0, dest_path_.Length - "Assets".Length) + "Dest";
        CreateFolder(dest_path_);

        script_bundle_list_ = dest_path_ + "/" + "all.txt";
        bundle_crc_name_ = dest_path_ + "/" + "bundle_crc";

        assetbundles_path_ = project_path_ + "/" + "assetbundles";
        assetbundles_path_ = assetbundles_path_.Replace("\\", "/");
        CreateFolder(assetbundles_path_);
        asset_dependency_name_ = dest_path + "/" + "asset_dependency";

        asset_hash_name_ = dest_path + "/" + "asset_hash";
    }

    void InitDestPath()
    {
        // init dest path 
    }

    void LoadConfig()
    {
        //LKJson json = new LKJson();
        //LKJsonObj = json.ReadJsObject();
    }

    DrawGUI drawgui_ = DrawGUI.Build;
    void OnGUI()
    {
        Color back = GUI.color;
        GUILayout.BeginHorizontal();
        if (drawgui_ == DrawGUI.Build)
            GUI.color = Color.green;
        else
            GUI.color = back;
        if (GUILayout.Button("Build"))
            drawgui_ = DrawGUI.Build;

        if (drawgui_ == DrawGUI.Dependency)
            GUI.color = Color.green;
        else
            GUI.color = back;

        if (drawgui_ == DrawGUI.Tools)
            GUI.color = Color.green;
        else
            GUI.color = back;

        if (GUILayout.Button("Tools"))
            drawgui_ = DrawGUI.Tools;

        if (drawgui_ == DrawGUI.View)
            GUI.color = Color.green;
        else
            GUI.color = back;

        if (GUILayout.Button("Pkg Viewer"))
            drawgui_ = DrawGUI.View;

        GUILayout.EndHorizontal();

        GUI.color = back;
        GUILayout.Space(10);
        if (drawgui_ == DrawGUI.Build)
            DrawBuild();
        else if (drawgui_ == DrawGUI.Tools)
            DrawTools();
    }

    string check_depends_ = string.Empty;
    void DrawTools()
    {
        check_depends_ = GUILayout.TextField(check_depends_);

        if (GUILayout.Button("Check Dependency"))
        {
            CheckDependency();
        }

        if (GUILayout.Button("Check DuplicateAssets"))
        {
            CheckDuplicateAssets();
        }

        if (GUILayout.Button("Check AssetBundle Size"))
        {
            CheckAssetBundleSize();
        }

        if (GUILayout.Button("Check Asset With None AssetBundleName"))
        {
            CheckAssetWithNoneAssetBundleName();
        }
    }

    bool select_all_ = true;
    bool select_single_bundles_ = true;

    bool write_to_dependency_ = false;
    Vector2 ref_position_ = Vector2.zero;
    Vector2 build_view_pos_ = Vector2.zero;

    AssetBundle ab_;
    string check_asset_dependency = "";
    const string ASSET_RESOURCE_PREFIX = "Assets/Resources/";

    void DrawBuild()
    {
        GUILayout.BeginHorizontal();
        if (GUILayout.Toggle(platform_ == BuildTarget.StandaloneWindows, "Windows"))
            platform_ = BuildTarget.StandaloneWindows;
        if (GUILayout.Toggle(platform_ == BuildTarget.iOS, "iOS"))
            platform_ = BuildTarget.iOS;
        if (GUILayout.Toggle(platform_ == BuildTarget.Android, "Android"))
            platform_ = BuildTarget.Android;
        GUILayout.EndHorizontal();
        GUILayout.Label(export_path_);
        
        GUILayout.BeginHorizontal();
        if (GUILayout.Button("Build AssetBundle", GUILayout.Width(160)))
        {
            if (EditorUtility.DisplayDialogComplex("Build", "Build:" + platform_.ToString(), "OK", "Cancel", "Cancel") == 0)
                BuildBundles();
        }

        if (GUILayout.Button("Save Bundle CRC", GUILayout.Width(160)))
        {
            SaveBundleCRC();
        }

        GUILayout.EndHorizontal();

        GUILayout.BeginHorizontal();
        if (GUILayout.Button("Build lua code", GUILayout.Width(160)))
        {
            BuildLua(export_path_, lua_debug_);
        }

        lua_debug_ = GUILayout.Toggle(lua_debug_, "Debug");
        GUILayout.EndHorizontal();


        GUILayout.BeginHorizontal();

        if (GUILayout.Button("Build lua small code to android", GUILayout.Width(160)))
        {
            buildSmallLua(lua_debug_);
           // BuildLua(export_path_, lua_debug_);
        }

        lua_debug_ = GUILayout.Toggle(lua_debug_, "Debug");
        GUILayout.EndHorizontal();


        GUILayout.Label("Pack all files in the Exports folder");
        if (GUILayout.Button("Pak All"))
        {
            PakAll();
        }

        if (GUILayout.Button("Pak Samll to android"))
        {
            PakSmallData();
        }

        GUILayout.BeginHorizontal();
     
        bool new_select_all = GUILayout.Toggle(select_all_, "select all", GUILayout.Width(120));
        bool is_changed = new_select_all ^ select_all_;
        select_all_ = new_select_all;

        write_to_dependency_ = GUILayout.Toggle(write_to_dependency_, "Write to dependency file");

        if (GUILayout.Button("Gen Bundles Name"))
        {
            GenBundleNames();
        }
        GUILayout.EndHorizontal();

        if (select_build_combine_.Length != combine_bunldes_.Length)
        {
            select_build_combine_ = new bool[combine_bunldes_.Length];
            for(int i = 0; i < combine_bunldes_.Length; i++)
            {
                select_build_combine_[i] = select_all_;
            }
        }

        build_view_pos_ = GUILayout.BeginScrollView(build_view_pos_);
        for(int i = 0; i < combine_bunldes_.Length; i++)
        {
            bool value = is_changed ? select_all_ : select_build_combine_[i];
            select_build_combine_[i] = GUILayout.Toggle(value, combine_bunldes_[i].ToString());
        }
        GUILayout.EndScrollView();

        bool flag = is_changed ? select_all_ : select_single_bundles_;
        select_single_bundles_ = GUILayout.Toggle(flag, "single bundles");

       
        if (GUILayout.Button("Test build script"))
        {
            CollectComponent();
        }

        //if (GUILayout.Button("Build scene"))
        //{
        //    BuildScene();
        //}

        if (GUILayout.Button("Build ScriptBundle"))
        {
            BuildScriptBundle();
        }
    }

    //void BuildScene()
    //{
    //    BuildPipeline.BuildStreamedSceneAssetBundle(scene_path_, export_path_ + "/level.unity3d", platform_);
    //}

    void BuildBundles()
    {
        //csharp_script
        BuildScriptBundle();

        //Assetbundles
        string output_path = assetbundles_path_;
        if (!Directory.Exists(output_path))
            Directory.CreateDirectory(output_path);

        UnityEngine.Debug.Log( "platform:" + platform_ + " output path:" + output_path);
        BuildPipeline.BuildAssetBundles(output_path, BuildAssetBundleOptions.ChunkBasedCompression | BuildAssetBundleOptions.DeterministicAssetBundle, platform_);

        SaveBundleCRC();
    }

    class NamePair
    {
        public string name;
        public string path;
        public string ext;
    }

    void ChangeFileName(NamePair names, string ext, string add)
    {
        if (names.ext == ext)
        {
            string new_path = names.path.Substring(0, names.path.Length - ext.Length);
            
            new_path += add + ext;

            string new_file = Path.Combine(resource_path_, new_path);

            if (File.Exists(new_file) == false)
                //File.Move(Path.Combine(resource_path_, names.path), new_file);
            {
                UnityEngine.Debug.LogWarning("change name :" + new_file);
                FileInfo fi = new FileInfo(Path.Combine(resource_path_, names.path));
                fi.MoveTo(new_file);

                // change meta file name
                FileInfo fi_meta = new FileInfo(Path.Combine(resource_path_, names.path + ".meta"));
                fi_meta.MoveTo(new_file + ".meta");
            }
            names.path = new_path;
            names.name += add;
        }
    }

    AssetBundle LoadAssetBundle(string name)
    {
        UnityEngine.Debug.Log(name);

        if (load_bundle_.ContainsKey(name))
            return load_bundle_[name];

        AssetBundle ab = AssetBundle.LoadFromFile(Path.Combine(export_path_, name));
        if (ab != null)
            load_bundle_.Add(name, ab);
        return ab;
    }


    static string AssetPathToBundleName(string name)
    {
        name = name.Substring(ASSET_RESOURCE_PREFIX.Length);
        int point_pos = name.LastIndexOf(".");
        if (point_pos != -1)
            name = name.Substring(0, point_pos);
        return name;
    }


    string AssetNameToFileName(string name)
    {
        return name.Substring(ASSET_RESOURCE_PREFIX.Length);
    }

    static string RemoveExt(string name)
    {
        int point_pos = name.LastIndexOf(".");
        if (point_pos != -1)
            name = name.Substring(0, point_pos);
        return name;
    }

    static string GetName(string filePath)
    {
        int pos = filePath.LastIndexOf("/");
        int point_pos = filePath.LastIndexOf(".");
        int length = point_pos - pos - 1;
        return filePath.Substring(pos + 1, length);
    }

    void BuildScriptBundle()
    {
        // change name to _
        string target_file = Path.Combine(dest_path_, "csharp_script");

        UnityEngine.Object o = AssetDatabase.LoadAssetAtPath("Assets/UnPackResource/all.prefab", typeof(UnityEngine.GameObject));
        BuildPipeline.BuildAssetBundle(o, null, target_file,
            BuildAssetBundleOptions.ChunkBasedCompression | BuildAssetBundleOptions.DeterministicAssetBundle | BuildAssetBundleOptions.CollectDependencies, 
            platform_);

    }

    void PackFolder(string pre_name, FileInfo[] files)
    {
        for (int i = 0; i < files.Length; i++)
        {
            FileInfo f = files[i];
            if (f.Name.EndsWith(".meta"))
                continue;
            EditorUtility.DisplayProgressBar("Pak", f.FullName, (float)i / (float)files.Length);
            PackFile(pre_name, f);
        }

        EditorUtility.ClearProgressBar();
    }

    void PackFile(string pre_name, FileInfo f)
    {
        FileStream file = new FileStream(f.FullName, FileMode.Open);
        string path = f.FullName.Substring(pre_name.Length + 1, f.FullName.Length - pre_name.Length - 1);
        //path = RemoveExt(path);
        byte[] buff = new byte[f.Length];
        file.Read(buff, 0, (int)f.Length);

        string assetName = path.Replace("\\", "/");
        int hashStr = AutherFile.GetStringHash(assetName);
        AutherFile.PutData(hashStr, buff);

        string oldAssetName = string.Empty;
        if (asset_hash_dict_.TryGetValue(hashStr, out oldAssetName) == false)
        {
            asset_hash_dict_.Add(hashStr, assetName);
        }
        else
        {
            UnityEngine.Debug.LogError(string.Format("{0} 与 {1} hash值{2}发生碰撞，尝试修改文件名解决这个问题。", assetName, oldAssetName, hashStr));
        }

        file.Close();
    }

    void SaveDependency(string file_name, Dictionary<string, List<string>> data)
    {
        // save to export path
        StreamWriter sw = new StreamWriter(file_name);
        foreach (var k in data.Keys)
        {
            List<string> files = data[k];
            if (files != null && files.Count > 0)
            {
                sw.WriteLine(k);
                foreach (var f in files)
                {
                    sw.Write('\t');
                    sw.WriteLine(f);
                }
            }
        }

        sw.Close();
    }

    void SaveAssetDependency(string file_name, Dictionary<string, string> data)
    {
        UnityEngine.Debug.Log("SaveAssetDependency." + file_name);
        FileInfo f = new FileInfo(file_name);
        if (f.Exists)
            f.Delete();

        StreamWriter sw = new StreamWriter(file_name);
        int startIndex = ASSET_RESOURCE_PREFIX.Length;

        Dictionary<string, List<string>> map = new Dictionary<string, List<string>>();
        List<string> list;
        foreach (var k in data.Keys)
        {
            string assetName = k.Substring(startIndex).ToLower();
            string assetBundleName = data[k];

            if (map.TryGetValue(assetBundleName, out list) == false)
            {
                list = new List<string>();
                map.Add(assetBundleName, list);
            }
            map[assetBundleName].Add(assetName);
        }

        foreach (var k in map)
        {
            //assetbundleName
            sw.WriteLine(k.Key);
            for (int i = 0; i < k.Value.Count; i++)
            {
                sw.Write('\t');
                sw.WriteLine(RemoveExt(k.Value[i]) + "|." + GetExt(k.Value[i]));
            }
        }
        sw.Close();
    }

    void SaveBundleCRC()
    {
        bundle_crc_.Clear();

        int startIndex = assetbundles_path.Length + 1;

        FileInfo[] files = GetAllAssetBundles();
        string assetBundleName = string.Empty;
        string fullName = string.Empty;

        for (int i = 0, length = files.Length; i < length; i++)
        {
            fullName = files[i].FullName;
            uint crc;
            BuildPipeline.GetCRCForAssetBundle(fullName, out crc);

            assetBundleName = fullName.Substring(startIndex).Replace("\\", "/");
            EditorUtility.DisplayProgressBar("SaveBundleCRC", assetBundleName, (float)i / length);

            bundle_crc_[assetBundleName] = crc;
        }

        EditorUtility.ClearProgressBar();

        StreamWriter sw = new StreamWriter(bundle_crc_name_);
        foreach (var v in bundle_crc_.Keys)
        {
            sw.WriteLine(v + ":" + bundle_crc_[v].ToString());
        }
        sw.Close();
    }

    void CollectComponent()
    {
        List<string> prefabs = GetAllAssetsString(resource_path_, true, "*.prefab");
        HashSet<System.Type> collects = new HashSet<System.Type>();
        HashSet<System.Type> failed_com = new HashSet<System.Type>();

        // get all component

        int i = 0;
        foreach (var p in prefabs)
        {
            EditorUtility.DisplayProgressBar("Collect", p, (float)i / (float)prefabs.Count);
            string path = ToAssetPath(p);
            GameObject o = AssetDatabase.LoadAssetAtPath(path, typeof(GameObject)) as GameObject;
            if (o == null)
            {
                UnityEngine.Debug.Log("LoadAsset Error." + path);
                continue;
            }
            MonoBehaviour[] comps = o.GetComponents<MonoBehaviour>();

            foreach (var c in comps)
            {
                if (c == null)
                    UnityEngine.Debug.Log(o.name);
                else
                    collects.Add(c.GetType());
            }

            comps = o.GetComponentsInChildren<MonoBehaviour>(true);
            foreach (var c in comps)
            {
                if (c == null)
                    UnityEngine.Debug.Log(o.name);
                else
                    collects.Add(c.GetType());
            }

            i++;
        }
        EditorUtility.ClearProgressBar();

        GameObject all = new GameObject("test");
        all.SetActive(false);
        foreach (var com in collects)
        {
            //UnityEngine.Debug.Log(com);
            Component t = all.AddComponent(com);
            if (t == null)
            {
                UnityEngine.Debug.Log("Add component " + com.Name + " failed.");
                failed_com.Add(com);
            }
        }

        //add again
        foreach (var com in failed_com)
        {
            Component t = all.AddComponent(com);
            if (t == null)
            {
                UnityEngine.Debug.LogError("Add component " + com.Name + " failed.");
            }
            else
            {
                UnityEngine.Debug.Log("Add component " + com.Name + " successed.");
            }
        }
 
        PrefabUtility.CreatePrefab("Assets/UnPackResource/" + script_bundle_name_, all);
        DestroyImmediate(all);
   
        List<System.Type> sort_com = new List<System.Type>(collects);
        sort_com.Sort((v0, v1) =>
        {
            return v0.Name.CompareTo(v1.Name);
        });

        StreamWriter fs = new StreamWriter(script_bundle_list_);
        foreach (var com in sort_com)
        {
            fs.WriteLine(com);
        }
        fs.Close();
    }
    
    public void BuildLua(string target_path, bool use_debug)
    {
        if (Directory.Exists(target_path))
            Directory.Delete(target_path, true);

        string tool_path = System.IO.Path.GetFullPath(Application.dataPath);

        if (platform_ == BuildTarget.iOS)
            tool_path += "/UnPackResource/Tool/luaCompiler";
        else
            tool_path += "/UnPackResource/Tool/luaCompiler.exe";

        List<string> files = GetAllAssetsString("StreamingAssets/Script", "*.txt", true);
        for (int i = 0; i < files.Count; i++)
        {
            string s = files[i];
            string target_file = target_path + ToLuaPath(s);
            if (File.Exists(target_file))
            {
                File.Delete(target_file);
               // UnityEngine.Debug.LogError(target_file + " Exsit");
               //continue;
            }

            EditorUtility.DisplayProgressBar("Compile", target_file, (float)i / (float)files.Count);
            CreateFolderByName(target_file);

            s = s.Substring(project_path_.Length + 1);
            string param = @"-o " + target_file + " " + s;
            if (use_debug == false)
                param = "-v -s " + param;
            ProcessStartInfo info = new ProcessStartInfo(tool_path, param);
            info.UseShellExecute = false;
            info.CreateNoWindow = true;
            Process p = Process.Start(info);
            p.Start();
            p.WaitForExit();
        }
        
        EditorUtility.ClearProgressBar();
    }

    void buildSmallLua(bool debug)
    {
        if (Directory.Exists(export_path_))
            Directory.Delete(export_path_, true);

        string tool_path = System.IO.Path.GetFullPath(Application.dataPath);

        if (platform_ == BuildTarget.iOS)
            tool_path += "/UnPackResource/Tool/luaCompiler";
        else
            tool_path += "/UnPackResource/Tool/luaCompiler.exe";

        string[] luaFiles ={"Assets/StreamingAssets/Script/UpdateUI/UpdateRegisterClass.txt",
                              "Assets/StreamingAssets/Script/Utils/ColorFilter.txt",
                              "Assets/StreamingAssets/Script/Utils/GlobalEnum.txt",
                              "Assets/StreamingAssets/Script/Utils/JSON.txt",
                           "Assets/StreamingAssets/Script/Utils/LuaLayoutCtr.txt",
                           "Assets/StreamingAssets/Script/Utils/LuaMessageBox.txt",
                           "Assets/StreamingAssets/Script/Utils/LuaUIViewCtr.txt",
                           "Assets/StreamingAssets/Script/Utils/slaxdom.txt",
                           "Assets/StreamingAssets/Script/Utils/slaxml.txt",
                           "Assets/StreamingAssets/Script/Utils/Text.txt",
                           "Assets/StreamingAssets/Script/Utils/TimerManager.txt",
                          "Assets/StreamingAssets/Script/Utils/UnityLevelMgr.txt",
                           "Assets/StreamingAssets/Script/Utils/UpdateConfig.txt",
                           "Assets/StreamingAssets/Script/Utils/UpdateController.txt",
                           "Assets/StreamingAssets/Script/Utils/Util.txt",
                           "Assets/StreamingAssets/Script/Utils/RegisterClass.txt",};

        List<string> files = new List<string>(); //GetAllAssetsString("StreamingAssets/Script", "*.txt", true);
        foreach (string file in luaFiles)
        {
            FileInfo fi = new FileInfo(file);
            if (fi != null)
            {
                files.Add(fi.FullName);
            }
        }
        for (int i = 0; i < files.Count; i++)
        {
            string s = files[i];
            string target_file = export_path_ + ToLuaPath(s);
            if (File.Exists(target_file))
            {
                File.Delete(target_file);
                // UnityEngine.Debug.LogError(target_file + " Exsit");
                //continue;
            }

            EditorUtility.DisplayProgressBar("Compile", target_file, (float)i / (float)files.Count);
            CreateFolderByName(target_file);

            s = s.Substring(project_path_.Length + 1);
            string param = @"-o " + target_file + " " + s;
            if (debug == false)
                param = "-v -s " + param;
            ProcessStartInfo info = new ProcessStartInfo(tool_path, param);
            info.UseShellExecute = false;
            info.CreateNoWindow = true;
            Process p = Process.Start(info);
            p.Start();
            p.WaitForExit();
        }

        EditorUtility.ClearProgressBar();
    }

    static void CreateFolderByName(string file)
    {
        string path = Path.GetDirectoryName(file);
        if (Directory.Exists(path) == false)
            Directory.CreateDirectory(path);
    }

    static void CreateFolder(string path)
    {
        if (Directory.Exists(path) == false)
            Directory.CreateDirectory(path);
    }

    static public List<string> GetAllAssetsString(string path, string search_pattern, bool bSearchChild)
    {
        return GetAllAssetsString("Assets/" + path, bSearchChild, search_pattern);
    }

    static public List<string> GetAllAssetsString(string path, bool bSearchChild, string searchPattern)
    {
        List<string> assets = new List<string>();
        DirectoryInfo di = new DirectoryInfo(path);
        FileInfo[] files = di.GetFiles(searchPattern, bSearchChild ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly);
        foreach (FileInfo fi in files)
        {
            if (fi.FullName.EndsWith(".meta"))
                continue;
            assets.Add(fi.FullName);
        }

        return assets;
    }

    public List<string> GetAllResourceString(string path, string search_pattern)
    {
        List<string> assets = new List<string>();

        string[] paths = path.Split(';');
        string[] split = search_pattern.Split(';');

        for (int j = 0; j < paths.Length; j++)
        {
            DirectoryInfo di = new DirectoryInfo(Path.Combine(resource_path_, paths[j])); 
            
            for (int i = 0; i < split.Length; i++)
            {
                FileInfo[] files = di.GetFiles(split[i], SearchOption.AllDirectories);
                foreach (FileInfo fi in files)
                {
                    if (fi.FullName.EndsWith(".meta"))
                        continue;

                    assets.Add(fi.FullName.Substring(resource_path_.Length).Replace("\\", "/"));

                }
            }
        }

        return assets;
    }

    static public string ToAssetPath(string filePath)
    {
        string assetRootPath = System.IO.Path.GetFullPath(Application.dataPath);
        return "Assets" + filePath.Substring(assetRootPath.Length).Replace("\\", "/");
    }

    static string ToStreamingAssetPath(string file_path)
    {
        string root = Path.GetFullPath(Application.streamingAssetsPath);
        return file_path.Substring(root.Length).Replace("\\", "/");

    }

    // return path name without "Script" and ext name
    static string ToLuaPath(string file_path)
    {
        string stream_path = ToStreamingAssetPath(file_path);
        return stream_path.Substring("/Script".Length);
    }

    public void SwitchPlatform(BuildTarget platform)
    {
        platform_ = platform;
    }

    public string export_path
    {
        get { return export_path_; }
    }

    public string dest_path
    {
        get { return dest_path_; }
    }

    public string assetbundles_path
    {
        get { return assetbundles_path_; }
    }

    public void BuildCSharpScript()
    {
        CollectComponent();
    }

    public void BuildAssetBundles(bool setBundleName = true)
    {
        if (select_build_combine_.Length != combine_bunldes_.Length)
        {
            select_build_combine_ = new bool[combine_bunldes_.Length];
            for (int i = 0; i < combine_bunldes_.Length; i++)
            {
                select_build_combine_[i] = true;
            }
        }

        write_to_dependency_ = true;

        //Gen AssetBundle Name
        if (setBundleName)
            GenBundleNames();

        BuildBundles();
    }

    public void BuildLua(bool debug)
    {
        BuildLua(export_path_, debug);
    }

    public void PakAll()
    {
        asset_hash_dict_.Clear();

        // clear 3 file
        AutherFile.LKFile_DeleteFile(dest_path_);

        // open data file and write
        AutherFile.LKFile_Init(dest_path_);

        // pak compiled scripts
        DirectoryInfo di = new DirectoryInfo(export_path_);
        FileInfo[] files = di.GetFiles("*", SearchOption.AllDirectories);
        PackFolder(export_path_, files);

        //pak bundle
        FileInfo[] assetbundles = GetAllAssetBundles();
        PackFolder(assetbundles_path, assetbundles);

        //csharp_script
        FileInfo csharp_script = new FileInfo(Path.Combine(dest_path, "csharp_script"));
        if (csharp_script.Exists)
            PackFile(dest_path, csharp_script);

        //asset_dependency
        FileInfo asset_dependency_f = new FileInfo(asset_dependency_name_);
        if (asset_dependency_f.Exists)
            PackFile(dest_path, asset_dependency_f);

        di = new DirectoryInfo(Path.Combine(Application.dataPath, "StreamingAssets/Data"));
        if (di.Exists)
            PackFolder(Path.Combine(Application.dataPath, "StreamingAssets"), di.GetFiles("*", SearchOption.AllDirectories));

        di = new DirectoryInfo(Path.Combine(Application.dataPath, "StreamingAssets/Textures"));
        if (di.Exists)
            PackFolder(Path.Combine(Application.dataPath, "StreamingAssets"), di.GetFiles("*", SearchOption.AllDirectories));

        SaveAssetHashFile(asset_hash_name_, asset_hash_dict_);

        AutherFile.LKFile_UnInit();

        UnityEngine.Debug.Log("Pak All Finish");
    }
    
    void SaveAssetHashFile(string fileName, Dictionary<int, string> hashValue)
    {
        StreamWriter write = new StreamWriter(fileName);
        foreach (var kv in hashValue)
        {
            write.WriteLine(kv.Value + ":" + kv.Key);
        }
        write.Close();
    }

    void PakSmallData()
    {
        asset_hash_dict_.Clear();

        // clear 3 file
        AutherFile.LKFile_DeleteFile(dest_path_);

        // open data file and write
        AutherFile.LKFile_Init(dest_path_);

        // pak compiled scripts
        DirectoryInfo di = new DirectoryInfo(export_path_);
        FileInfo[] files = di.GetFiles("*", SearchOption.AllDirectories);
        PackFolder(export_path_, files);

        //pak bundle
        FileInfo[] assetbundles = GetSmallAssetBundles();
        PackFolder(assetbundles_path, assetbundles);

        //csharp_script
        FileInfo csharp_script = new FileInfo(Path.Combine(dest_path, "csharp_script"));
        if (csharp_script.Exists)
            PackFile(dest_path, csharp_script);

        //asset_dependency
        FileInfo asset_dependency_f = new FileInfo(asset_dependency_name_);
        if (asset_dependency_f.Exists)
            PackFile(dest_path, asset_dependency_f);

//         di = new DirectoryInfo(Path.Combine(Application.dataPath, "StreamingAssets/Data"));
//         if (di.Exists)
//             PackFolder(Path.Combine(Application.dataPath, "StreamingAssets"), di.GetFiles("*", SearchOption.AllDirectories));

//         di = new DirectoryInfo(Path.Combine(Application.dataPath, "StreamingAssets/Textures"));
//         if (di.Exists)
//             PackFolder(Path.Combine(Application.dataPath, "StreamingAssets"), di.GetFiles("*", SearchOption.AllDirectories));
        string updateBGpath = Path.Combine(Application.dataPath, "StreamingAssets/Textures/Update/UpdateBG.jpg");
        FileInfo updateBGinfo = new FileInfo(updateBGpath);
        PackFile(Path.Combine(Application.dataPath, "StreamingAssets"), updateBGinfo);

        AutherFile.LKFile_UnInit();
    }
    FileInfo[] GetSmallAssetBundles()
    {
        string[] assetbundlesBuffer ={"/music/ui/buttonclick_13",
                                     "/ui/atlas/bit16/Emotion",
                                     "/ui/atlas/bit32/baseui",
                                     "/ui/atlas/bit32/login",
                                     "/ui/fonts/msyh",
                                     "/ui/prefab/toolsui/messagebox",
                                     "/assetbundles","/shader"};

        List<FileInfo> assetbundles = new List<FileInfo>();
        if (Directory.Exists(assetbundles_path_))
        {
            DirectoryInfo di = new DirectoryInfo(assetbundles_path_);
            foreach (string file in assetbundlesBuffer)
            {
                FileInfo fi = new FileInfo(assetbundles_path_ + file);
                if (fi != null)
                {
                    assetbundles.Add(fi);
                }
            }
        }
        return assetbundles.ToArray();
    }
    void GenBundleNames()
    {
        AssetDatabase.RemoveUnusedAssetBundleNames();
        bundle_names.Clear();

        if (select_all_)
        {
            List<string> files = GetFiles(new string[] { "" }, new string[] { "*.*" }, true);
            UnityEngine.Debug.Log("files.count:" + files.Count);

            for (int i = 0; i < files.Count; i++)
            {
                string assetPath = ToAssetPath(files[i]);
                assets_to_assetbundle_[assetPath] = "";
            }
        }

        for(int i = 0; i < combine_bunldes_.Length; i++)
        {
            if (!select_build_combine_[i])
                continue;

            SetConbineBundleNames(combine_bunldes_[i]);
        }

        if (select_single_bundles_)
            SetSingleBundleNames();

        if (write_to_dependency_)
            SaveAssetDependency(asset_dependency_name_, asset_dependency_);

        SetEmptyBundleNameForAssets();

        asset_dependency_.Clear();
        assets_to_assetbundle_.Clear();

        UnityEngine.Debug.Log("AssetBudnle Total Count." + bundle_names.Count);

        AssetDatabase.Refresh();
    }

    void SetConbineBundleNames(BundleCombineNode combineNode)
    {
        string dir = combineNode.path_;
        bundle_combine_type combine_type = combineNode.combine_type_;

        if (combine_type == bundle_combine_type.single_dir_load || combine_type == bundle_combine_type.single_dir_unload
            || combine_type == bundle_combine_type.top_dir_load || combine_type == bundle_combine_type.top_dir_unload)
        {
            SetBundleNameWithExt(dir, combineNode, false);

            SearchOption options = SearchOption.AllDirectories;
            bool searchChild = false;
            if (combine_type == bundle_combine_type.top_dir_load || combine_type == bundle_combine_type.top_dir_unload)
            {
                options = SearchOption.TopDirectoryOnly;
                searchChild = true;
            }

            string rootPath = resource_path_ + dir;
            DirectoryInfo info = new DirectoryInfo(rootPath);
            foreach (DirectoryInfo directoryInfo in info.GetDirectories("*", options))
            {
                string relativePath = directoryInfo.FullName.Replace("\\", "/");
                relativePath = relativePath.Substring(rootPath.Length);
                SetBundleNameWithExt(dir + relativePath, combineNode, searchChild);
            }
        }
        else if (combine_type == bundle_combine_type.character)
        {
            //CombineCharacterBundles(combineNode);
        }
        else if (combine_type == bundle_combine_type.atlas)
        {
            CombineAtlas(combineNode);
        }
        else
        {
            SetBundleNameWithExt(dir, combineNode, true);
        }
    }

    Dictionary<string, BundleNode> clip_bundles_ = new Dictionary<string, BundleNode>();
 
    BundleNode BuildBundleNode(string assetPath, List<string> clipPathFilenameArray)
    {
        BundleNode bundle_node = new BundleNode();
        string assetBundleName = AssetPathToBundleName(assetPath);

        bundle_node.name_ = assetBundleName;
        bundle_node.asset_list_.Add(assetPath);

        string clipFileName;
        int count = clipPathFilenameArray.Count;
        for (int i = 0; i < count; i++)
        {
            clipFileName = clipPathFilenameArray[i];
            if (clipFileName.Length == 0)
                continue;

            string clipPath = ASSET_RESOURCE_PREFIX + clipFileName + ".anim";
            BundleNode node;
            if (clip_bundles_.TryGetValue(clipFileName, out node))  //anim被多个prefab使用,合并prefab
            {
                if (!node.name_.Equals(assetBundleName))
                {
                    assetBundleName = node.name_;
                    bundle_node.name_ = node.name_;
                }
                bundle_node.asset_list_.Add(clipPath);
            }
            else
            {
                bundle_node.asset_list_.Add(clipPath);
                clip_bundles_.Add(clipFileName, bundle_node);
            }
        }

        return bundle_node;
    }
  
    void CombineAtlas(BundleCombineNode combineNode)
    {
        int bitsset = (int)combineNode.combine_type_ & LOAD_BITSIT;
        bool writeToDependency = bitsset == 1;

        List<string> files = GetAllAssetsString("Resources/" + combineNode.path_, combineNode.ext_, true);
        for(int i = 0, count = files.Count; i < count; i++)
        {
            EditorUtility.DisplayProgressBar("Combine Atlas", combineNode.path_, (float)i / (float)count);
            string filename = files[i];
            
            string assetPath = ToAssetPath(filename);
            string bundleName = AssetPathToBundleName(assetPath);
            SetBundleName(assetPath, bundleName, writeToDependency);

            string[] dependencies = AssetDatabase.GetDependencies(assetPath);
            for (int j = 0, length = dependencies.Length; j < length; j++)
            {
                string dependecy = dependencies[j];
                if (dependecy.EndsWith(".mat") || dependecy.EndsWith(".png"))
                {
                    if (dependecy.StartsWith("Assets/Resources/" + combineNode.path_) == false)
                        continue;
                    SetBundleName(dependecy, bundleName, false);
                }
            }
        }

        EditorUtility.ClearProgressBar();
    }

    void SetSingleBundleNames()
    {
        foreach (var kv in single_bundles_)
        {
            string[] list = kv.Key.Split('|');
            string src_file = resource_path_ + list[0];
            if (!File.Exists(src_file))
                continue;

            string dest_file = resource_path_ + list[1];
            string assetPath = ToAssetPath(dest_file.Replace("/", "\\"));

            if (!src_file.Equals(dest_file))
            {
                string file_name = src_file.Substring(src_file.LastIndexOf("/") + 1);
                if (!File.Exists(dest_file))
                {
                    GameObject obj = new GameObject(file_name);
                    obj.SetActive(true);
                    PrefabUtility.CreatePrefab(assetPath, obj);
                    DestroyImmediate(obj);
                }

                File.Copy(src_file, dest_file, true);
                AssetImporter assetImporter = AssetImporter.GetAtPath(assetPath);
                assetImporter.SaveAndReimport();
            }
            
            string assetBundleName = kv.Value;
            SetBundleName(assetPath, assetBundleName, true);
        }
    }

    string GetExt(string name)
    {
        string ext = "";

        int index = name.LastIndexOf(".");
        if (index != -1)
            ext = name.Substring(index+1);

        return ext;
    }

    string GetPlatformFolder(BuildTarget buildTarget)
    {
        if (buildTarget == BuildTarget.Android)
            return "android";
        else if (buildTarget == BuildTarget.iOS)
            return "ios";
        else if (buildTarget == BuildTarget.WP8Player)
            return "wp";
        //...

        return "windows";
    }

    void SetBundleNameWithExt(string dir, BundleCombineNode combineNode, bool searchChild = true)
    {
        string[] dirs = dir.Split(';');
        string[] exts = combineNode.ext_.Split(';');

        string assetBundleName = combineNode.name_;
        if (string.IsNullOrEmpty(assetBundleName))
        {
            string name_prefex = dirs[0];
            string name_ext = exts[0];
            assetBundleName = name_prefex + "_" + GetExt(name_ext);
        }
      
        int bitsset = (int)combineNode.combine_type_ & LOAD_BITSIT;
        bool writeToDependency = bitsset == 1;
        //UnityEngine.Debug.Log("bitset:" + bitsset + " " + writeToDependency);

        string bundleName = assetBundleName;
        List<string> files = GetFiles(dirs, exts, searchChild);
        for (int i = 0, count = files.Count; i < count; i++)
        {
            EditorUtility.DisplayProgressBar("SetBundleNameWithExt-[" + dir + "]", files[i], (float)i / (float)count);
            
            string assetPath = ToAssetPath(files[i]);
            if (combineNode.combine_type_ == bundle_combine_type.single_load
                || combineNode.combine_type_ == bundle_combine_type.single_unload)
            {
                bundleName = AssetPathToBundleName(assetPath);
                if (writeToDependency == false)
                {
                    bundleName = bundleName + "_" + GetExt(assetPath);
                }

                SetBundleName(assetPath, bundleName, false);
            }
            else
            {
                if (combineNode.number_ > 1)  //need to package into mulit files.
                {
                    string name = GetName(assetPath);
                    int intValue = StringToInt(name);
                    int index = intValue % combineNode.number_;
                    bundleName = assetBundleName + "_" + index;
                }

                SetBundleName(assetPath, bundleName, writeToDependency);
            }
        }

        EditorUtility.ClearProgressBar();
    }

    int StringToInt(string value)
    {
        int ret = 0;
        char[] charArray = value.ToCharArray();
        for (int i = 0; i < charArray.Length; i++)
        {
            ret += (int)charArray[i];
        }

        return ret;
    }

    List<string> GetFiles(string[] dirs, string[] exts, bool searchChild)
    {
        List<string> files = new List<string>();
        List<string> exclude_files = new List<string>();

        for(int i = 0; i < dirs.Length; i++)
        {
            string temp_dir = "Resources/" + dirs[i];
            for(int j = 0; j < exts.Length; j++)
            {
                string temp_ext = exts[j];
                if (temp_ext.StartsWith("!"))
                    exclude_files.AddRange(GetAllAssetsString(temp_dir, temp_ext.Substring(1), searchChild));
                else
                    files.AddRange(GetAllAssetsString(temp_dir, temp_ext, searchChild));
            }
        }

        files.RemoveAll(item =>
        {
            return exclude_files.Contains(item);
        });

        return files;
    }

    HashSet<string> bundle_names = new HashSet<string>();
    void SetBundleName(string assetPath, string assetBundleName, bool writeToDependency = false)
    {
        AssetImporter assetImporter = AssetImporter.GetAtPath(assetPath);
        if (assetImporter == null)
        {
            UnityEngine.Debug.LogError("AssetImport is null. " + assetPath);
            return;
        }

        assetBundleName = assetBundleName.ToLower();
        if (assetImporter.assetBundleName != assetBundleName)
            assetImporter.assetBundleName = assetBundleName;

        if (assetBundleName.Length > 0)
        {
            if (writeToDependency)
                asset_dependency_[assetPath] = assetBundleName;
            else
            {
                if (asset_dependency_.ContainsKey(assetPath))
                    asset_dependency_.Remove(assetPath);
            }

            if (assets_to_assetbundle_.ContainsKey(assetPath))
                assets_to_assetbundle_.Remove(assetPath);

            bundle_names.Add(assetBundleName);
        }
    }

    void SetEmptyBundleNameForAssets()
    {
        //把不需要导出的资源assetbundle设置成空
        int count = assets_to_assetbundle_.Count;
        int i = 0;
        foreach (var kv in assets_to_assetbundle_)
        {
            EditorUtility.DisplayProgressBar("SetEmptyBundleNameForAssets-" + count, kv.Key, (float)i / count);
            if (string.IsNullOrEmpty(kv.Value))
            {
                SetBundleName(kv.Key, "", false);
            }
            i++;
        }

        EditorUtility.ClearProgressBar();
    }

    #region tools
    void CheckDependency()
    {
        string resource_path = ASSET_RESOURCE_PREFIX;
        List<string> files = GetAllAssetsString("Resources", "*.*", true);
        Dictionary<string, List<string>> dependency_error = new Dictionary<string, List<string>>();

        for(int i = 0, count = files.Count; i < count; i++)
        {
            string assetPath = ToAssetPath(files[i]);
            string[] dependencies = AssetDatabase.GetDependencies(new string[]{ assetPath });

            EditorUtility.DisplayProgressBar("CheckDependency", assetPath, i / count);

            foreach (string dependency in dependencies)
            {
                if (dependency.EndsWith(".dll"))
                    continue;

                if (!dependency.StartsWith(resource_path))
                {
                    List<string> list;
                    if (dependency_error.TryGetValue(assetPath, out list) == false)
                    {
                        list = new List<string>();
                        dependency_error.Add(assetPath, list);
                    }

                    list.Add(dependency);
                    UnityEngine.Debug.Log(assetPath + " -> " + dependency);
                }
            }
        }

        EditorUtility.ClearProgressBar();

        SaveDependency(dest_path_ + "/dependency_error", dependency_error);
    }

    void CheckAssetWithNoneAssetBundleName()
    {
        List<string> files = GetAllAssetsString("Resources", "*.*", true);

        for (int i = 0, count = files.Count; i < count; i++)
        {
            string filepath = files[i];
            if (filepath.EndsWith(".meta"))
                continue;

            string assetPath = ToAssetPath(filepath);
            EditorUtility.DisplayProgressBar("CheckAssetWithNoneAssetBundleName", assetPath, (float)i / count);

            AssetImporter assetImporter = AssetImporter.GetAtPath(assetPath);

            string assetBundleName = assetImporter.assetBundleName;
            if (string.IsNullOrEmpty(assetBundleName))
            {
                UnityEngine.Debug.Log(assetPath);
            }
        }

        EditorUtility.ClearProgressBar();
    }

    Dictionary<string, string> asset_bundle_map_ = new Dictionary<string, string>();
    private void CheckDuplicateAssets()
    {
        string[] files = Directory.GetFiles(resource_path_, "*.*", SearchOption.AllDirectories);
    
        List<string> duplicate_assets = new List<string>();
        for (int i = 0, count = files.Length; i < count; i++)
        {
            string filepath = files[i];
            if (filepath.EndsWith(".meta"))
                continue;

            EditorUtility.DisplayProgressBar("CheckDuplicateAssets", filepath, (float)i / count);

            string ext = GetExt(filepath);
            string filepath_no_ext = RemoveExt(filepath);
            
            string tmp_file_o = filepath_no_ext + "_o." + ext;
            string tmp_file_t = filepath_no_ext + "_t." + ext;
            string tmp_file_m = filepath_no_ext + "_m." + ext;

            if (File.Exists(tmp_file_o) || File.Exists(tmp_file_t) || File.Exists(tmp_file_m))
            {
                duplicate_assets.Add(filepath);
            }  
        }

        UnityEngine.Debug.Log("duplicate_assets.count:" + duplicate_assets.Count);
        
        StreamWriter sw = new StreamWriter(dest_path + "/" + "duplicate_assets");
        for (int i = 0;  i < duplicate_assets.Count; i++)
        {
            sw.WriteLine(ToAssetPath(duplicate_assets[i]));
        }
        sw.Close();

        EditorUtility.ClearProgressBar();
    }

    long MAX_SIZE_FOR_ASSETBUNDLE = 1 << 20; 
    private void CheckAssetBundleSize()
    {
        FileInfo[] files = GetAllAssetBundles();
        UnityEngine.Debug.Log("CheckAssetBundleSize start." + files.Length);

        for (int i = 0; i < files.Length; i++)
        {
            string filepath = files[i].FullName;
            EditorUtility.DisplayProgressBar("CheckAssetBundleSize", filepath, i / files.Length);

            FileInfo file = new FileInfo(filepath);
            long file_size = file.Length;
            if (file_size > MAX_SIZE_FOR_ASSETBUNDLE)
            {
                UnityEngine.Debug.Log("assetbundlle:" + filepath + " size." + file_size);
            }
            
        }
        UnityEngine.Debug.Log("CheckAssetBundleSize finish." + asset_bundle_map_.Count);
        EditorUtility.ClearProgressBar();
    }

    FileInfo[] GetAllAssetBundles()
    {
        List<FileInfo> assetbundles = new List<FileInfo>();
        if (Directory.Exists(assetbundles_path_))
        {
            DirectoryInfo di = new DirectoryInfo(assetbundles_path_);
            FileInfo[] files = di.GetFiles("*.*", SearchOption.AllDirectories);


            for (int i = 0; i < files.Length; i++)
            {
                if (files[i].Name.EndsWith(".manifest"))
                    continue;

                assetbundles.Add(files[i]);
            }
        }
        return assetbundles.ToArray();

    }
    #endregion

    #region class
    class BundleNode
    {
        public string name_;
        public List<string> asset_list_ = new List<string>();
    }

    class BundleCombineNode
    {
        public BundleCombineNode(string path, string ext, bundle_combine_type combine_type, int number = 1, string name = null)
        {
            path_ = path;
            ext_ = ext;
            combine_type_ = combine_type;
            name_ = name;
            number_ = number;
        }
        public string path_;
        public string ext_;
        public bundle_combine_type combine_type_;
        public string name_;
        public int number_;

        public override string ToString()
        {
            return path_ + "|" + ext_;
        }
    }
    #endregion
}
