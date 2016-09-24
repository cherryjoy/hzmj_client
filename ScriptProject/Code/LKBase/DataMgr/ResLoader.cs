using UnityEngine;
using System.Collections.Generic;
using System.Text;
using System.IO;

public class ResLoader
{
    public class BundleNode
    {
        public AssetBundle bundle_;
        public string name_;
        public int ref_count_ = 0;
        public bool unloadable_ = true;
        public int loadedAsset_count_ = 0;

        private List<BundleNode> dependencies_ = new List<BundleNode>();
       
        public BundleNode(AssetBundle bundle, string name)
        {
            bundle_ = bundle;
            name_ = name;
        }

        public void Retain()
        {
            ref_count_++;
        }

        public void Release(bool unloadAllLoadedObjects)
        {
            ref_count_--;

            if (ref_count_ == 0 && loadedAsset_count_ == 0)
            {
                Unload(unloadAllLoadedObjects);
            }
        }

        public void Unload(bool unloadAllLoadedObjects)
        {
            if (unloadable_)
            {
                if (bundle_ != null)
                    bundle_.Unload(unloadAllLoadedObjects);

                bundles_.Remove(name_);
                bundle_ = null;
     
                for (int i = 0; i < dependencies_.Count; i++)
                {
                    dependencies_[i].Release(unloadAllLoadedObjects);
                }
            }
        }

        public void AddDependency(BundleNode dependency)
        {
            dependency.Retain();
            dependencies_.Add(dependency);
        }

        public void UnloadAsset(string name)
        {
            loadedAsset_count_--;

            if (loadedAsset_count_ == 0 && ref_count_ == 0)
            {
                Unload(false);
            }
        }

        public Object LoadAsset(string name)
        {
            Object asset = null;
            if (string.IsNullOrEmpty(name))
            {
                string[] assetNames = bundle_.GetAllAssetNames();
                if (assetNames.Length > 0)
                {
                    asset = bundle_.LoadAsset(assetNames[0]); //每个bundle中仅有一个asset
                }
            }
            else
            {
                string assetName = asset_path_prefix_ + name;
                asset = bundle_.LoadAsset(assetName);
            }

            return asset; 
        }
    }

    static string data_path_ = string.Empty;
    static string assetbundles_path_ = string.Empty;
    static string asset_path_prefix_ = "assets/resources/";

    class AssetNameNode
    {
        public string name_;
        public string bundle_name_;
    }

    class AssetNode
    {
        public string name_;
        private BundleNode bundle_;
        private System.WeakReference asset_;

        public AssetNode(string name, BundleNode bundle, Object asset)
        {
            name_ = name;
            bundle_ = bundle;
            asset_ = new System.WeakReference(asset);
        }

        public Object Asset
        {
            get
            {
                return (UnityEngine.Object)asset_.Target;
            }

            set
            {
                asset_.Target = value;
            }
        }
        
        public void Dispose()
        {
            if (bundle_ != null)
            {
                bundle_.UnloadAsset(name_);
                bundle_ = null;
            }

            assets_.Remove(name_);
            asset_ = null;
        }
    }

    static AssetBundleManifest dependence_ = null;
    static Dictionary<string, AssetNameNode> asset_dependence_ = new Dictionary<string, AssetNameNode>();
    static Dictionary<string, BundleNode> bundles_ = new Dictionary<string,BundleNode>();
    static Dictionary<string, AssetNode> assets_ = new Dictionary<string, AssetNode>();

    static AssetBundle main_ = null;
    static AssetBundle scripts_ = null;
    static AssetBundle dependence_bundle_ = null;

    //static List<BundleNode> mru_ = new List<BundleNode>();

    static string[] preload_bundles = new string[]
    {
        "ui/fonts/hzgb",
        "ui/fonts/msyh",
    };

    private static AssetBundle LoadBundle(string name)
    {
#if ASSETBUNDLE
        int offset = AutherFile.LKFile_GetDataOffset(name);
        
        if (offset == -1)
        {
            Debug.LogError(name + " not exsit");
            return null;
        }
        else
        {
            AssetBundle ab = AssetBundle.LoadFromFile(data_path_ + "/data", 0, (ulong)offset);
#if DEBUG
            ab.name = name;
#endif
            return ab;
        }
#else
        return null;
#endif

    }

    // Load asset bundle or Resources.Load
    // auto_unload: load the asset and release the bundles
    public static Object Load(string name)
    {
#if ASSETBUNDLE
        if (dependence_ == null)
        {
#if DEBUG
            Debug.Log("ResLoader is not initialized. Please Call its Init method first.");
#endif
            return null;
        }

        name = name.ToLower();
        if (string.IsNullOrEmpty(name))
        {
            Debug.LogError("name is empty");
            return null;
        }

        Object asset = GetAsset(name);
        if (asset != null)
        {
            return asset;
        }
        else
        {
            string bundle_name = name;
            string asset_name = string.Empty;

            AssetNameNode asset_name_node;
            if (asset_dependence_.TryGetValue(bundle_name, out asset_name_node))
            {
                bundle_name = asset_name_node.bundle_name_;
                asset_name = asset_name_node.name_;
            }

            BundleNode main_node = null;
            Object asset_main = null;

            if (bundles_.TryGetValue(bundle_name, out main_node))
            {
                asset_main = main_node.LoadAsset(asset_name);
            }
            else
            {
                // load bundles
                main_node = new BundleNode(null, bundle_name);

                //Load dependencies.
                GetDependencies(main_node, bundle_name);

                AssetBundle ab_main = LoadBundle(bundle_name);
                if (ab_main != null)
                {
                    main_node.bundle_ = ab_main;
                    //mru_.Add(main_node);
                    bundles_.Add(bundle_name, main_node);

                    asset_main = main_node.LoadAsset(asset_name);
                } 
            }

            if (asset_main != null)
                AddAsset(name, main_node, asset_main);

            return asset_main;
        }
#else

#if DEBUG
        System.Diagnostics.Stopwatch st = new System.Diagnostics.Stopwatch();
		st.Start();
		Object obj = Resources.Load(name);
		st.Stop();

        if (obj == null)
        {
            Debug.LogError(name);
        }

		return obj;
#else
		return Resources.Load(name);
#endif
     
#endif
    }

    private static void GetDependencies(BundleNode main_node, string bundle_name)
    {
        string[] dependency_names = dependence_.GetDirectDependencies(bundle_name);
        for (int i = 0, count = dependency_names.Length; i < count; i++)
        {
            string dependency_name = dependency_names[i];
            if (string.IsNullOrEmpty(dependency_name))
            {
                Debug.LogError(bundle_name + " has empty dependency." + i + " " + count);
                continue;
            }

            BundleNode dependency_node;
            if (bundles_.TryGetValue(dependency_name, out dependency_node) == false)
            {
                dependency_node = new BundleNode(null, dependency_name);
                GetDependencies(dependency_node, dependency_name);

                AssetBundle ab = LoadBundle(dependency_name);
                if (ab != null)
                {
                    dependency_node.bundle_ = ab;
                    //mru_.Add(main_node);
                    bundles_.Add(dependency_name, dependency_node);
                }
            }

            if (dependency_node != null && dependency_node.bundle_ != null)
                main_node.AddDependency(dependency_node);
        }
    }

    private static Object GetAsset(string name)
    {
        AssetNode asset_node;
        if (assets_.TryGetValue(name, out asset_node) == true)
        {
            return asset_node.Asset;
        }

        return null;
    }

    private static void AddAsset(string name, BundleNode bundle, Object asset)
    {
        AssetNode asset_node;
        if (assets_.TryGetValue(name, out asset_node) == true)
        {
            asset_node.Asset = asset;
        }
        else
        {
            asset_node = new AssetNode(name, bundle, asset);
            assets_.Add(name, asset_node);
            bundle.loadedAsset_count_++;
        }
    }

    public static void Init()
    {
#if ASSETBUNDLE
        //InitData();

        if (dependence_ == null)
        { 
            LoadDependence();
            LoadDefault();
        }
#endif
    }

    public static bool CheckBundle(string name)
    {
        if (bundles_.ContainsKey(name))
        {
            BundleNode node = bundles_[name];
            return true;
        }
        return false;
    }

    public static void InitData()
    {
#if ASSETBUNDLE
        InitPath();

        if (AutherFile.LKFile_Init(data_path_) == false)
            throw new System.Exception("data file do not exist");
#endif
    }

    private static void LoadDependence()
    {
#if ASSETBUNDLE
        dependence_bundle_ = LoadBundle("assetbundles");
        dependence_ = dependence_bundle_.LoadAsset<AssetBundleManifest>("assetbundlemanifest");

        byte[] dependency = AutherFile.GetData("asset_dependency");
        MemoryStream se = new MemoryStream(dependency);
        StreamReader sr = new StreamReader(se);

        string[] list;
        string assetBundleName = string.Empty;
        string s = sr.ReadLine();
        while (s != null)
        {
            if (s[0] != '\t')
            {
                assetBundleName = s;
            }
            else
            {
                list = s.Substring(1).Split('|');  //assetName|ext
                if (list.Length == 2)
                {
                    AssetNameNode asset_node = new AssetNameNode();
                    asset_node.name_ = list[0] + list[1];
                    asset_node.bundle_name_ = assetBundleName;
                    asset_dependence_.Add(list[0], asset_node);
                }
            }
            s = sr.ReadLine();
        }
        se.Close();
        sr.Close();
#endif
    }

    public static void InitStreamingAssetsPath(string path)
    {
#if ASSETBUNDLE
         data_path_ = path;
         AutherFile.LKFile_InitOnlyRead(data_path_);
#endif
    }

    private static void InitPath()
    {
#if UNITY_EDITOR
        data_path_ = Application.dataPath;
        assetbundles_path_ = data_path_.Substring(0, data_path_.Length - "Assets".Length) + "AssetBundles/";
        data_path_ = data_path_.Substring(0, data_path_.Length - "Assets".Length) + "Dest/";
#elif UNITY_STANDALONE_WIN
        data_path_ = Application.dataPath;
        data_path_ = data_path_.Substring(0, data_path_.LastIndexOf("/") + 1);
        assetbundles_path_ = data_path_ + "AssetBundles/";
#elif UNITY_ANDROID || UNITY_IPHONE
        data_path_ = PluginTool.SharedInstance().PersisitentDataPath;
        assetbundles_path_ = data_path_ + "AssetBundles/";
#endif
        Debug.Log("data_path:" + data_path_);
        Debug.Log("assetbundles_path:" + assetbundles_path_);
    }

    private static void LoadDefault()
    {
        // preload bundles
        main_ = LoadBundle("shader");
        scripts_ = LoadBundle("csharp_script");

        AddBundleNode(main_, "shader", false);

        for (int i = 0; i < preload_bundles.Length; i++)
        {
            AssetBundle ab = LoadBundle(preload_bundles[i]);
            AddBundleNode(ab, preload_bundles[i], false);
        }       
    }

    public static void InitSmallData()
    {

        if (dependence_ == null)
        {
            LoadDependence();
        }
        main_ = LoadBundle("shader");
        scripts_ = LoadBundle("csharp_script");
        AddBundleNode(main_, "shader", false);
        AssetBundle ab = LoadBundle(preload_bundles[1]);
        AddBundleNode(ab, preload_bundles[1], false);

    }

    public static BundleNode AddBundleNode(AssetBundle ab, string name, bool unloadable = true)
    {
        BundleNode node = new BundleNode(ab, name);
        node.unloadable_ = unloadable;
        bundles_.Add(name, node);

        return node;
    }

    public static AssetBundle GetAssetBundle(string name)
    {
        AssetBundle ab = null;

        BundleNode node;
        if (bundles_.TryGetValue(name, out node))
        {
            ab = node.bundle_;
        }

        return ab;
    }

    public static Object Load(string path, System.Type systemTypeInstance)
    {
#if ASSETBUNDLE
        return Load(path);
#else
        return Resources.Load(path, systemTypeInstance);
#endif
        
    }

    public static Object LoadBundleFile(string path)
    {
        return Load(path);
    }

    // load raw file
    public static byte[] LoadRaw(string relativePath)
    {
#if ASSETBUNDLE
        if (dependence_ == null)
        {
            LoadDependence();
            // preload bundles
            LoadDefault();
        }
        return AutherFile.GetData(relativePath);
#else
        string path = UIPathManager.GetStreamPath() + relativePath;
        if (File.Exists(path))
        {
            FileStream fs = File.OpenRead(path);
            int length = (int)fs.Length;
            byte[] data = new byte[length];
            fs.Read(data, 0, length);
            fs.Close();
            return data;
        }
        else {
            return null;
        }
        
#endif
    }

    // Unload bundles without assets
    public static void UnloadBundles(bool unloadAll = false)
    {
#if ASSETBUNDLE
        if (unloadAll)
        {
            foreach (var node in bundles_)
            {
                node.Value.bundle_.Unload(true);
                node.Value.bundle_ = null;
            }

            bundles_.Clear();
            //mru_.Clear();
            assets_.Clear();
        }        
#endif
        Resources.UnloadUnusedAssets();
    }

    #region async
    public static void LoadAsync(string name, OnLoad onload)
    {
#if ASSETBUNDLE
        name = name.ToLower();
        Object asset = GetAsset(name);
        if (asset != null)
        {
            if (onload != null)
                onload(asset);
        }
        else
        {
            string bundle_name = name;
            BundleNode bundle_node;
            if (bundles_.TryGetValue(bundle_name, out bundle_node))
            {
                asset = bundle_node.LoadAsset(string.Empty);

                if (onload != null)
                    onload(asset);
            }
            else
            {
                LoadBundleNode load_node = new LoadBundleNode(bundle_name, (obj) =>
                {
                    bundle_node = bundles_[bundle_name];
                    AddAsset(name, bundle_node, obj);

                    if (onload != null)
                        onload(obj);
                });

                GlobalUpdate.Instance.AddLoadBundleNode(load_node);
            }
        }
#else
        Object obj = Resources.Load(name);

        if (onload != null)
            onload(obj);
#endif
    }

    public static AssetBundleCreateRequest LoadBundleAsync(string name)
    {
#if ASSETBUNDLE
        int offset = AutherFile.LKFile_GetDataOffset(name);

        if (offset == -1)
        {
            Debug.LogError(name + " not exsit");
            return null;
        }
        else
        {
            AssetBundleCreateRequest request = AssetBundle.LoadFromFileAsync(data_path_ + "/data", 0, (ulong)offset);
            return request;
        }
#else
        return null;
#endif
    }

    public static AssetBundleRequest LoadAssetAsync(AssetBundle bundle)
    {
        string[] assetNames = bundle.GetAllAssetNames();
        if (assetNames.Length > 0)
        {
            string assetName = assetNames[0];      //每个bundle中仅有一个asset
            AssetBundleRequest request = bundle.LoadAssetAsync(assetName);
            return request;
        }
        return null;
    }
    #endregion

    #region LoadFromMemory
    public static AssetBundle LoadFromMemory(byte[] binary)
    {
#if ASSETBUNDLE
        AssetBundle ab = AssetBundle.LoadFromMemory(binary);
        return ab;
#else
        return null;
#endif
    }
    #endregion LoadFromMemory
	
     public static void UnloadUnusedAssets()
    {
        Resources.UnloadUnusedAssets();
#if ASSETBUNDLE
#if DEBUG
        int bundles_count = bundles_.Count;
        int assets_count = assets_.Count;
#endif
        List<AssetNode> delete_list = new List<AssetNode>();
        foreach (var node in assets_)
        {
            if (node.Value.Asset == null)
            {
                delete_list.Add(node.Value);
            }
        }

        for (int i = 0; i < delete_list.Count; i++)
        {
            delete_list[i].Dispose();
        }
        delete_list.Clear();
#if DEBUG
        Debug.Log("UnloadUnusedAssets->assets_.count:" + assets_count + "->" + assets_.Count + " bundles_.Count:" + bundles_count + "->" + bundles_.Count);
#endif
#endif
    }

    // Unload bundles and close Auther file
    public static void Close()
    {
#if ASSETBUNDLE
        UnloadBundles(true);
        main_ = null;

        if (scripts_ != null)
        {
            scripts_.Unload(true);
            scripts_ = null;
        }
        
        dependence_ = null;
        
        if (dependence_bundle_ != null)
        {
            dependence_bundle_.Unload(true);
            dependence_bundle_ = null;
        }

        if (asset_dependence_ != null)
        {
            asset_dependence_.Clear();
        }
        AutherFile.LKFile_UnInit();
#endif
    }

    // only shaders use this function
    public static Object LoadAsset(string path)
    {
#if ASSETBUNDLE
        path = path.ToLower();
        path = asset_path_prefix_ + path + ".shader";
        if (main_)
        {
            return main_.LoadAsset(path);
        }
        else
        {
            return null;
        }
#else
        return Resources.Load(path);
#endif
    }

    public static void UnloadScene(string scene_path, string env_path)
    {
#if ASSETBUNDLE
        scene_path = scene_path.ToLower();
        BundleNode scene_node;
        if (bundles_.TryGetValue(scene_path, out scene_node))
        {
            scene_node.Unload(false);
            //mru_.Remove(scene_node);
        }

        //env_path = env_path.ToLower();
        //BundleNode env_node;
        //if (bundles_.TryGetValue(env_path, out env_node))
        //{
        //    env_node.Unload(false);
        //    //mru_.Remove(env_node);
        //}
#endif
    }
}

public delegate void OnLoad(Object o);
public class LoadBundleNode
{
    public string bundle_name_;
    public OnLoad onload_;

    public LoadBundleNode(string bundle_name, OnLoad onload)
    {
        bundle_name_ = bundle_name;
        onload_ = onload;
    }
}
