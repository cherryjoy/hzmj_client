//----------------------------------------------
//            NGUI: Next-Gen UI kit
// Copyright � 2011-2012 Tasharen Entertainment
//----------------------------------------------

using UnityEngine;
using System;
using System.Collections.Generic;
using UniLua;
using System.Runtime.InteropServices;

/// <summary>
/// Helper class containing generic functions used throughout the UI library.
/// </summary>

static public partial class NGUITools
{
	static AudioListener mListener;
	static AudioSource UIAudioSource = null;

    static public void PlaySoundByPath(string path)
    {
       AudioClip clip = ResLoader.Load(path) as AudioClip;
       if (clip != null)
       {
           PlaySound(clip, 1f);
       }
    }

	/// <summary>
	/// Play the specified audio clip.
	/// </summary>

	static public AudioSource PlaySound (AudioClip clip) { return PlaySound(clip, 1f); }

	/// <summary>
	/// Play the specified audio clip with the specified volume.
	/// </summary>

	static public AudioSource PlaySound (AudioClip clip, float volume)
	{
        // 音量改为使用系统设置中的音量设置
        volume = MusicManager.Instance.AudioVolume;
		if (clip != null)
		{
			if (mListener != null)
			{				
				if(UIAudioSource == null)
				{
					UIAudioSource  = mListener.gameObject.AddComponent<AudioSource>();
				}
				//Debug.Log(volume);
				UIAudioSource.PlayOneShot(clip, volume);
				return UIAudioSource;
			}
			
			if (mListener == null)
			{
				mListener = GameObject.FindObjectOfType(typeof(AudioListener)) as AudioListener;

                if (mListener == null)
                {
                    Camera cam = Camera.main;
                    if (cam == null) cam = GameObject.FindObjectOfType(typeof(Camera)) as Camera;
                    if (cam != null) mListener = cam.gameObject.AddComponent<AudioListener>();
                }
                else
                {
                    if (UIAudioSource == null)
                    {
                        UIAudioSource = mListener.gameObject.AddComponent<AudioSource>();
                    }
                    //Debug.Log(volume);
                    UIAudioSource.PlayOneShot(clip, volume);
                    return UIAudioSource;
                }
			}
		}
		return null;
	}

	/// <summary>
	/// Same as Random.Range, but the returned value is >= min and <= max.
	/// Unity's Random.Range is < max instead, unless min == max.
	/// This means Range(0,1) produces 0 instead of 0 or 1. That's unacceptable.
	/// </summary>

	static public int RandomRange (int min, int max)
	{
		if (min == max) return min;
		return UnityEngine.Random.Range(min, max + 1);
	}

	/// <summary>
	/// Returns the hierarchy of the object in a human-readable format.
	/// </summary>

	static public string GetHierarchy (GameObject obj)
	{
		string path = obj.name;

		while (obj.transform.parent != null)
		{
			obj = obj.transform.parent.gameObject;
			path = obj.name + "/" + path;
		}
		return "\"" + path + "\"";
	}

	static public string GetTypeName(UnityEngine.Object obj)
	{
		if (obj == null) return "Null";
		string s = obj.GetType().ToString();
		if (s.StartsWith("UI")) s = s.Substring(2);
		else if (s.StartsWith("UnityEngine.")) s = s.Substring(12);
		return s;
	}

	/// <summary>
	/// Parse a RrGgBb color encoded in the string.
	/// </summary>

	static public Color ParseColor (string text, int offset)
	{
		int r = (NGUIMath.HexToDecimal(text[offset])	 << 4) | NGUIMath.HexToDecimal(text[offset + 1]);
		int g = (NGUIMath.HexToDecimal(text[offset + 2]) << 4) | NGUIMath.HexToDecimal(text[offset + 3]);
		int b = (NGUIMath.HexToDecimal(text[offset + 4]) << 4) | NGUIMath.HexToDecimal(text[offset + 5]);
		float f = 1f / 255f;
		return new Color(f * r, f * g, f * b);
	}

	/// <summary>
	/// The reverse of ParseColor -- encodes a color in RrGgBb format.
	/// </summary>

	static public string EncodeColor (Color c)
	{
#if UNITY_FLASH
		// int.ToString(format) doesn't seem to be supported on Flash as of 3.5.0 -- it simply silently crashes
		return "FFFFFF";
#else
		int i = 0xFFFFFF & (NGUIMath.ColorToInt(c) >> 8);
		return i.ToString("X6");
#endif
	}

	/// <summary>
	/// Parse an embedded symbol, such as [FFAA00] (set color) or [-] (undo color change)
	/// </summary>

	static public int ParseSymbol (string text, int index, List<Color> colors)
	{
		int length = text.Length;

		if (index + 2 < length)
		{
			if (text[index + 1] == '-')
			{
				if (text[index + 2] == ']')
				{
					if (colors != null && colors.Count > 1) colors.RemoveAt(colors.Count - 1);
					return 3;
				}
			}
			else if (index + 7 < length)
			{
				if (text[index + 7] == ']')
				{
					if (colors != null)
					{
						Color c = ParseColor(text, index + 1);
						c.a = colors[colors.Count - 1].a;
						colors.Add(c);
					}
					return 8;
				}
			}
		}
		return 0;
	}

	/// <summary>
	/// Runs through the specified string and removes all color-encoding symbols.
	/// </summary>

	static public string StripSymbols (string text)
	{
		if (text != null)
		{
			text = text.Replace("\\n", "\n");
			for (int i = 0, imax = text.Length; i < imax; )
			{
				char c = text[i];

				if (c == '[')
				{
					int retVal = ParseSymbol(text, i, null);

					if (retVal > 0)
					{
						text = text.Remove(i, retVal);
						imax = text.Length;
						continue;
					}
				}
				++i;
			}
		}
		return text;
	}

	/// <summary>
	/// Find the camera responsible for drawing the objects on the specified layer.
	/// </summary>

	static public Camera FindCameraForLayer (int layer,bool ignoreLayer = true)
	{
        if (ignoreLayer)
        {
            Camera cam = UIRoot.mSelf.mCamera;
            if (cam != null)
            {
                return cam;
            }
        }
		int layerMask = 1 << layer;
		Camera[] cameras = GameObject.FindObjectsOfType(typeof(Camera)) as Camera[];

		foreach (Camera cam in cameras)
		{
			if ((cam.cullingMask & layerMask) != 0)
			{
				return cam;
			}
		}
		return null;
	}


	static public UIPanel FindPanelForLayer(int layer)
	{
		UIPanel[] panels = GameObject.FindObjectsOfType(typeof(UIPanel)) as UIPanel[];
		for (int i = 0; i < panels.Length; i++ )
		{
			if (panels[i].gameObject.layer == layer)
			{
				return panels[i];
			}
		}
		return null;
	}

	/// <summary>
	/// Add a collider to the game object containing one or more widgets.
	/// </summary>

	static public BoxCollider AddWidgetCollider (GameObject go)
	{
		if (go != null)
		{
			Collider col = go.GetComponent<Collider>();
			BoxCollider box = col as BoxCollider;

			if (box == null)
			{
				if (col != null)
				{
					if (Application.isPlaying) GameObject.Destroy(col);
					else GameObject.DestroyImmediate(col);
				}
				box = go.AddComponent<BoxCollider>();
			}

			int depth = NGUITools.CalculateNextDepth(go);

			Bounds b = NGUIMath.CalculateRelativeWidgetBounds(go.transform);
			box.isTrigger = true;
			box.center = b.center + Vector3.back * (depth * 0.25f);
			box.size = new Vector3(b.size.x, b.size.y, 0f);
			return box;
		}
		return null;
	}
	
/*
	/// <summary>
	/// Want to swap a low-res atlas for a hi-res one? Just use this function.
	/// </summary>

	[Obsolete("Use UIAtlas.replacement instead")]
	static public void ReplaceAtlas (UIAtlas before, UIAtlas after)
	{
		UISprite[] sprites = GameObject.FindObjectsOfType(typeof(UISprite)) as UISprite[];
		
		foreach (UISprite sprite in sprites)
		{
			if (sprite.atlas == before)
			{
				sprite.atlas = after;
			}
		}

		UILabel[] labels = GameObject.FindObjectsOfType(typeof(UILabel)) as UILabel[];

		foreach (UILabel lbl in labels)
		{
			if (lbl.font != null && lbl.font.atlas == before)
			{
				lbl.font.atlas = after;
			}
		}
	}

	/// <summary>
	/// Want to swap a low-res font for a hi-res one? This is the way.
	/// </summary>

	[Obsolete("Use UIFont.replacement instead")]
	static public void ReplaceFont (UIFont before, UIFont after)
	{
		UILabel[] labels = GameObject.FindObjectsOfType(typeof(UILabel)) as UILabel[];

		foreach (UILabel lbl in labels)
		{
			if (lbl.font == before)
			{
				lbl.font = after;
			}
		}
	}
*/
	/// <summary>
	/// Helper function that returns the string name of the type.
	/// </summary>

	static public string GetName<T> () where T : UnityEngine.Object
	{
		string s = typeof(T).ToString();
		if (s.StartsWith("UI")) s = s.Substring(2);
		else if (s.StartsWith("UnityEngine.")) s = s.Substring(12);
		return s;
	}

	/// <summary>
	/// Add a new child game object.
	/// </summary>

	static public GameObject AddChild (GameObject parent)
	{
		GameObject go = new GameObject();

		if (parent != null)
		{
			Transform t = go.transform;
			t.parent = parent.transform;
			t.localPosition = Vector3.zero;
			t.localRotation = Quaternion.identity;
			t.localScale = Vector3.one;
			go.layer = parent.layer;
		}
		return go;
	}

    static public GameObject AddChild(GameObject parent, GameObject prefab, bool checkPanel, string name = "")
    {
        GameObject go = GameObject.Instantiate(prefab) as GameObject;
        if (go != null && parent != null)
        {
            Transform t = go.transform;
            if (string.IsNullOrEmpty(name) == false)
                t.name = name;
            t.parent = parent.transform;
            t.localRotation = Quaternion.identity;
            t.localScale = Vector3.one;
            t.localPosition = Vector3.zero;
            if (checkPanel)
            {
                UIPanel panel = go.GetComponent<UIPanel>();
                if (panel != null)
                {
                    if (panel.DepthFlag == UIPanel.PanelDepthFlag.Top)
                    {
                        UIPanel.AddToTop(panel);
                    }
                    else if (panel.DepthFlag == UIPanel.PanelDepthFlag.Bottom)
                    {
                        UIPanel.AddToBottom(panel);
                    }
                    else if (panel.DepthFlag == UIPanel.PanelDepthFlag.Normal)
                    {
                        UIPanel.AddToNormal(panel);
                    }
                }
#if UNITY_EDITOR || UNITY_STANDALONE_WIN
                Transform t_p = parent.transform;
                
                UIPanel t_panel = null;
                while (t_p != null)
                {
                    t_panel = t_p.GetComponent<UIPanel>();
                    if (t_panel != null && t_panel.DepthFlag != UIPanel.PanelDepthFlag.None)
                    {
                        break;
                    }
                    t_p = t_p.parent;                    
                }

                if (t_panel != null)
                {
                    UIPanel[] panels = go.GetComponentsInChildren<UIPanel>(true);
                    for (int i = 0; i < panels.Length; i++)
                    {
                        if (panels[i].DepthFlag != UIPanel.PanelDepthFlag.None)
                        {
                            Debug.LogError("child panel couldn't contain non none property.");
                            break;
                        }
                    }
                }
#endif
            }
        }
        return go;
    }

	static public GameObject AddChildWithName(GameObject parent,string name)
	{
		GameObject go = AddChild(parent);
		go.name = name;
		return go;
	}
	/// <summary>
	/// Instantiate an object and add it to the specified parent.
	/// </summary>

	static public GameObject AddChild (GameObject parent, GameObject prefab)
	{
		GameObject go = GameObject.Instantiate(prefab) as GameObject;

		if (go != null && parent != null)
		{
			Transform t = go.transform;
			t.parent = parent.transform;
			t.localPosition = Vector3.zero;
			t.localRotation = Quaternion.identity;
			t.localScale = Vector3.one;
			go.layer = parent.layer;
		}
		return go;
	}

	static public GameObject AddParticleByName(string name, GameObject parent)
	{
		string path = "LiziEffects/Lizi/" + name;
		GameObject prefab = (GameObject)ResLoader.Load(path, typeof(GameObject));
		return NGUITools.AddChildNotLoseScale(parent, prefab);
	}

	static public GameObject AddChildByResourcesPath(string path, GameObject parent)
	{
		GameObject prefab = (GameObject)ResLoader.Load(path, typeof(GameObject));
		return NGUITools.AddChildNotLoseScale(parent, prefab);
	}

	static public GameObject AddChildByPath(string path, GameObject parent)
	{
		GameObject prefab = (GameObject)ResLoader.Load(UIPrefabDummy.GetUIPrefabPath(path), typeof(GameObject));
		return NGUITools.AddChildNotLoseScale(parent, prefab);
	}

	/// <summary>
	/// Instantiate an object and add it to the specified parent with it's .original transform
	/// </summary>
	static public GameObject AddChildNotLoseScale(GameObject parent, GameObject prefab, string name = null)
	{
        Vector3 finalWorldPos = Vector3.zero;
        if ( parent != null)
        {
            finalWorldPos = parent.transform.localToWorldMatrix.MultiplyPoint(Vector3.zero);
        }
       
        GameObject go = GameObject.Instantiate(prefab, finalWorldPos, prefab.transform.rotation) as GameObject;
		if (go != null && parent != null)
		{
			Transform t = go.transform;
			Vector3 myLocalScale = t.localScale;
			t.parent = parent.transform;
			t.localScale = myLocalScale;
			t.localRotation = Quaternion.identity;
			go.layer = parent.layer;
		}
		if (!string.IsNullOrEmpty(name))
		{
			go.name = name;
		}
		return go;
	}

	static public GameObject AddChildNotLoseAnything(GameObject parent, GameObject prefab, string name = null)
	{
		if (prefab == null)
		{
			return null;
		}
		Transform initTrans = prefab.transform;
        Vector3 finalWorldPos = Vector3.zero;
        if (parent != null)
        {
            finalWorldPos = parent.transform.localToWorldMatrix.MultiplyPoint(prefab.transform.position);
        }

        GameObject go = GameObject.Instantiate(prefab, finalWorldPos, initTrans.rotation) as GameObject;

		if (go != null && parent != null)
		{
			Transform t = go.transform;
			Vector3 myLocalScale = t.localScale;
			t.parent = parent.transform;
			t.localScale = myLocalScale;
			go.layer = parent.layer;
		}
		if (!string.IsNullOrEmpty(name))
		{
			go.name = name;
		}
		return go;
	}

    static public GameObject AddChildWithPosition(GameObject parent, GameObject prefab, Vector3 position)
    {
        GameObject go = GameObject.Instantiate(prefab, position, prefab.transform.rotation) as GameObject;

        if (go != null && parent != null)
        {
            Transform t = go.transform;
            Vector3 myLocalScale = t.localScale;
            t.parent = parent.transform;
            t.localScale = myLocalScale;

            go.layer = parent.layer;
        }

        return go;
    }

	/// <summary>
	/// Gathers all widgets and calculates the depth for the next widget.
	/// </summary>

	static public int CalculateNextDepth (GameObject go)
	{
		int depth = -1;
        //UIWidget[] widgets = go.GetComponentsInChildren<UIWidget>();
        //foreach (UIWidget w in widgets) depth = Mathf.Max(depth, w.depth);
        UIWidget widget = go.GetComponent<UIWidget>();
        if (widget == null)
        {
            widget = NGUITools.FindInParents<UIWidget>(go);
        }
        if (widget != null)
        {
            depth = Mathf.Max(depth, widget.depth);
        }
		return depth + 1;
	}

	/// <summary>
	/// Add a child object to the specified parent and attaches the specified script to it.
	/// </summary>

	static public T AddChild<T> (GameObject parent) where T : Component
	{
		GameObject go = AddChild(parent);
		go.name = GetName<T>();
		return go.AddComponent<T>();
	}

	/// <summary>
	/// Add a new widget of specified type.
	/// </summary>

	static public T AddWidget<T> (GameObject go) where T : UIWidget
	{
		int depth = CalculateNextDepth(go);

		// Create the widget and place it above other widgets
		T widget = AddChild<T>(go);
		widget.depth = depth;

		// Clear the local transform
        Transform t = widget.transform;
        t.localPosition = Vector3.zero;
        //t.localRotation = Quaternion.identity;
        //t.localScale = new Vector3(100f, 100f, 1f);
        widget.pivot = UIWidget.Pivot.Center;
        widget.width = 100;
        widget.height = 100;
        widget.gameObject.layer = go.layer;
		return widget;
	}

	static public T AddWidget<T>(GameObject go, Vector3 scale) where T : UIWidget
	{
		int depth = CalculateNextDepth(go);

		// Create the widget and place it above other widgets
		T widget = AddChild<T>(go);
		widget.depth = depth;

		// Clear the local transform
		Transform t = widget.transform;
		t.localPosition = Vector3.zero;
		t.localRotation = Quaternion.identity;
		t.localScale = scale;
		widget.gameObject.layer = go.layer;
		return widget;
	}

	/// <summary>
	/// Add a sprite appropriate for the specified atlas sprite.
	/// It will be a UISlicedSprite if the sprite has an inner rect, and a regular sprite otherwise.
	/// </summary>

	static public UISprite AddSprite (GameObject go, UIAtlas atlas, string spriteName)
	{
		UIAtlas.Sprite sp = (atlas != null) ? atlas.GetSprite(spriteName) : null;
		UISprite sprite = (sp == null || sp.inner == sp.outer) ? AddWidget<UISprite>(go) : (UISprite)AddWidget<UISlicedSprite>(go);
		sprite.atlas = atlas;
		sprite.spriteName = spriteName;
		return sprite;
	}

	/// <summary>
	/// Finds the specified component on the game object or one of its parents.
	/// </summary>

	static public T FindInParents<T> (GameObject go) where T : Component
	{
		if (go == null) return null;
		object comp = go.GetComponent<T>();

		if (comp == null)
		{
			Transform t = go.transform.parent;

			while (t != null && comp == null)
			{
				comp = t.gameObject.GetComponent<T>();
				t = t.parent;
			}
		}
		return (T)comp;
	}

	/// <summary>
	/// Destroy the specified object, immediately if in edit mode.
	/// </summary>

	static public void Destroy (UnityEngine.Object obj)
	{
		if (obj != null)
		{
			if (Application.isPlaying) UnityEngine.Object.Destroy(obj);
			else UnityEngine.Object.DestroyImmediate(obj);
		}
	}

	/// <summary>
	/// Call the specified function on all objects in the scene.
	/// </summary>

	static public void Broadcast (string funcName)
	{
		GameObject[] gos = GameObject.FindObjectsOfType(typeof(GameObject)) as GameObject[];
		foreach (GameObject go in gos) go.SendMessage(funcName, SendMessageOptions.DontRequireReceiver);
	}

	/// <summary>
	/// Call the specified function on all objects in the scene.
	/// </summary>

	static public void Broadcast (string funcName, object param)
	{
		GameObject[] gos = GameObject.FindObjectsOfType(typeof(GameObject)) as GameObject[];
		foreach (GameObject go in gos) go.SendMessage(funcName, param, SendMessageOptions.DontRequireReceiver);
	}

	/// <summary>
	/// Determines whether the 'parent' contains a 'child' in its hierarchy.
	/// </summary>

	static public bool IsChild (Transform parent, Transform child)
	{
		if (parent == null || child == null) return false;

		while (child != null)
		{
			if (child == parent) return true;
			child = child.parent;
		}
		return false;
	}
	
	public static void RemoveChild (Transform parent)
	{
		List<GameObject> children = new List<GameObject> ();
		for (int i = 0; i < parent.childCount; i++)
			children.Add (parent.GetChild (i).gameObject);
		
		foreach (GameObject obj in children)
			GameObject.Destroy (obj);
		children.Clear ();
	}
	
	public static void RemoveChildImmediate (Transform parent)
	{
		List<GameObject> children = new List<GameObject> ();
		for (int i = 0; i < parent.childCount; i++)
			children.Add (parent.GetChild (i).gameObject);
		
		foreach (GameObject obj in children)
			GameObject.DestroyImmediate (obj);
		children.Clear ();
	}
	
	public static void SendAll (GameObject[] objects, string methodName, object data, SendMessageOptions options)
	{
		if (objects != null) {
			foreach (GameObject obj in objects) {
				if (obj == null)
				{
					continue;
				}
				obj.SendMessage (methodName, data, options);
			}
		}
	}

	public static void SetActive(Transform trans,bool active)
	{
		if(trans != null){
			trans.gameObject.SetActive(active);
			if(active){
				for(int i = 0;i < trans.childCount;++i){
					trans.GetChild(i).gameObject.SetActive(active);
				}
			}
		}
	}

    public static T[] CombieArray<T>(T[] first, T[] second)
    {
        int count = 0;
        if (first != null)
        {
            count += first.Length;
        }
        if (second != null)
        {
            count += second.Length;
        }
        T[] result = new T[count];
        if (first != null)
        {
            for (int i = 0; i < first.Length; i++)
            {
                result[i] = first[i];
            }
        }
        if (second != null)
        {
            int length = second.Length;
            int startIndex = count - length;
            for (int i = 0; i < length; i++)
            {
                result[startIndex + i] = second[i];
            }
        }
        return result;
    }

    //  transplant from new version
    /// <summary>
    /// Helper function that returns whether the specified MonoBehaviour is active.
    /// </summary>

    static public bool GetActive(Behaviour mb)
    {
#if UNITY_3_5
		return mb != null && mb.enabled && mb.gameObject.active;
#else
        return mb != null && mb.enabled && mb.gameObject.activeInHierarchy;
#endif
    }

    /// <summary>
    /// Unity4 has changed GameObject.active to GameObject.activeself.
    /// </summary>

    static public bool GetActive(GameObject go)
    {
#if UNITY_3_5
		return go && go.active;
#else
        return go && go.activeInHierarchy;
#endif
    }

    /// <summary>
    /// Unity4 has changed GameObject.active to GameObject.SetActive.
    /// </summary>

    static public void SetActiveSelf(GameObject go, bool state)
    {
#if UNITY_3_5
		go.active = state;
#else
        if(state!= go.activeSelf)
            go.SetActive(state);
#endif
    }

    /// <summary>
    /// Adjust the widget's collider based on the depth of the widgets, as well as the widget's dimensions.
    /// </summary>

    static public void UpdateWidgetCollider(GameObject go)
    {
        UpdateWidgetCollider(go, false);
    }

    /// <summary>
    /// Adjust the widget's collider based on the depth of the widgets, as well as the widget's dimensions.
    /// </summary>

    static public void UpdateWidgetCollider(GameObject go, bool considerInactive)
    {
        if (go != null)
        {
            UpdateWidgetCollider(go.GetComponent<BoxCollider>(), considerInactive);
        }
    }

    /// <summary>
    /// Adjust the widget's collider based on the depth of the widgets, as well as the widget's dimensions.
    /// </summary>

    static public void UpdateWidgetCollider(BoxCollider bc)
    {
        UpdateWidgetCollider(bc, false);
    }

    /// <summary>
    /// Adjust the widget's collider based on the depth of the widgets, as well as the widget's dimensions.
    /// </summary>

    static public void UpdateWidgetCollider(BoxCollider box, bool considerInactive)
    {
        if (box != null)
        {
            GameObject go = box.gameObject;
            UIWidget w = go.GetComponent<UIWidget>();

            if (w != null)
            {
                Vector4 region = w.drawingDimensions;
                box.center = new Vector3((region.x + region.z) * 0.5f, (region.y + region.w) * 0.5f);
                box.size = new Vector3(region.z - region.x, region.w - region.y);
            }
            else
            {
                Bounds b = NGUIMath.CalculateRelativeWidgetBounds(go.transform, go.transform);
                box.center = b.center;
                box.size = new Vector3(b.size.x, b.size.y, 0f);
            }
#if UNITY_EDITOR
            UnityEditor.EditorUtility.SetDirty(box);
#endif
        }
    }

    public static void ChangedWidgetColorIncludeChild(GameObject go, Color color, bool considerInactive)
    {
        if (go == null) return;
        UIWidget[] allWidget = go.GetComponentsInChildren<UIWidget>(considerInactive);
        if (allWidget == null || allWidget.Length == 0) return;

        for (int i = 0; i < allWidget.Length; i++)
        {
            allWidget[i].color = color;
        }
    }

    public static void ChangedWidgetAlphaColorIncludeChild(GameObject go,float alpha,bool considerInactive)
    {
        if (go == null) return;
        UIWidget[] allWidget = go.GetComponentsInChildren<UIWidget>(considerInactive);
        if (allWidget == null || allWidget.Length == 0) return;

        for (int i = 0; i < allWidget.Length; i++)
        {
            Color oldColor = allWidget[i].color;
            
            allWidget[i].color = new Color(oldColor.r,oldColor.g,oldColor.b,alpha);
        }
    }

    // end of transplant from new version
}
