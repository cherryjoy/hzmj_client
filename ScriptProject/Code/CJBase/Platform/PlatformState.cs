using UnityEngine;
using System.Collections;
using System.Runtime.InteropServices;
using System.IO;
/**
 *  该文件包含常用的接口定义实现，对于一些特殊的，不太常见的接口，我们会定义在PlatformStateExtension.cs中
 */

public partial class PlatformState
{
    private static PlatformState instance;
    public static PlatformState Instance
    {
        get
        {
            if (instance == null) instance = new PlatformState();
            return instance;
        }
    }

#if UNITY_ANDROID
    private AndroidJavaObject mAndroidObj;
#endif

#if UNITY_IPHONE
    [DllImport("__Internal")]
    private static extern void cjsdkInit(string objName, string customInfo);

    [DllImport("__Internal")]
    private static extern void cjsdkLogin(string objName, string customInfo);

    [DllImport("__Internal")]
	private static extern void cjsdkShare(string objName, string title, string description, string contextUrl, string imageUrl, string extent, string scene);

    //[DllImport("__Internal")]
    //private static extern void cjsdkPayment(string pid, string amount, string productName, string customInfo, string ext, string scene);
#endif

/**
*  初始化平台
*  ext:
*      json字符串，具体格式为，不能为空
*/
    public void CJSDKInit(string objName, string ext)
    {
        //Debug.Log("objname : " + objName + " ext : " + ext);
#if UNITY_ANDROID
        if (mAndroidObj == null)
        {
            try
            {
                AndroidJavaClass jc = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
                mAndroidObj = jc.GetStatic<AndroidJavaObject>("currentActivity");
            }
            catch (System.Exception ex)
            {
                log("new AndroidJavaObject", ex.ToString());
            }
        }

        if (mAndroidObj != null)
        {
            log("init", "ext is " + ext);
            try
            {
                mAndroidObj.Call("CJSDKInit", objName, ext);
            }
            catch (System.Exception e)
            {
                log("CJSDKInit", e.ToString());
            }
        }
#elif UNITY_IPHONE
        cjsdkInit(objName, ext);
#endif
    }


    /**
     *  启动登录页面
     *  ext:
     *      json格式字符串
     */
    public void CJSDKLogin(string objName, string ext)
    {
#if UNITY_ANDROID
        if (mAndroidObj != null)
        {
            log("login", "ext is " + ext);
            try
            {
                mAndroidObj.Call("CJSDKLogin", objName, ext);
            }
            catch (System.Exception ex)
            {
                log("CJSDKLogin", ex.ToString());
            }
        }
#elif UNITY_IPHONE
        cjsdkLogin(objName, ext);
#endif
    }

    /**
     *  分享
     *  title 标题
     *  description 描述
     *  contextUrl 跳转链接
     *  imageUrl 显示图片，为空则默认显示应用图标
     */
	public void CJSDKShare(string objName, string title, string description, string contextUrl, string imageUrl, string extent, string scene)
    {
		if (string.IsNullOrEmpty(imageUrl))
		{ 
			SDKShareCallBack(objName, title, description, contextUrl, imageUrl, extent, scene);
		}
		else
		{
			//imageUrl = "screenshot.png";
			//Application.CaptureScreenshot(imageUrl, 100);
			PlatformSDKController.mSelf.ShareScreenShot(objName, title, description, contextUrl, imageUrl, extent, scene);
		}
    }

	public void SDKShareCallBack(string objName, string title, string description, string contextUrl, string imageUrl, string extent, string scene)
	{
		#if UNITY_ANDROID
        if (mAndroidObj == null)
        {
            //Debug.Log("mAndroidObj == null");
            try
            {
                AndroidJavaClass jc = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
                mAndroidObj = jc.GetStatic<AndroidJavaObject>("currentActivity");
            }
            catch (System.Exception ex)
            {
                log("new AndroidJavaObject", ex.ToString());
            }
        }

        if (mAndroidObj != null)
        {
            log("share", "title: " + title + ", description: " + description + ", contextUrl: " + contextUrl + ", imageUrl: " + imageUrl + ", extent: " + extent + ", scene: " + scene);
            try
            {
                mAndroidObj.Call("CJSDKShare", objName, title, description, contextUrl, imageUrl, extent, scene);
            }
            catch (System.Exception ex)
            {
                log("CJSDKShare", ex.ToString());
            }
        }
#elif UNITY_IPHONE
		cjsdkShare(objName, title, description, contextUrl, imageUrl, extent, scene);
#endif
	}

//    /**
//     *  创建角色
//     *  ext:
//     *      json格式字符串
//     */
//    public void OKSDKCreateRole(string json)
//    {
//#if UNITY_ANDROID
//        if (mAndroidObj != null)
//        {
//            log("createRole", "json is " + json);
//            try
//            {
//                mAndroidObj.Call("OKSDKCreateRole", json);
//            }
//            catch(System.Exception e)
//            {
//                 log("OKSDKCreateRole", e.ToString());
//            }
//        }
//#elif UNITY_IPHONE
//        oksdkCreateRole(json);
//#endif
//    }                               

//    /**
//     *  进入游戏
//     *  ext:
//     *      json格式字符串
//     */
//    public void OKSDKEnterGame(string json)
//    {
//#if UNITY_ANDROID
//        if (mAndroidObj != null)
//        {
//            log("enterGame", "json is " + json);
//            try
//            {
//                mAndroidObj.Call("OKSDKEnterGame", json);
//            }
//            catch (System.Exception ex)
//            {
//                log("OKSDKEnterGame", ex.ToString());
//            }
            
//        }
//#elif UNITY_IPHONE
//       oksdkEnterGame(json);
//#endif
//    }


//    /**
//     *  登出当前账号
//     *  ext:
//     *      json格式字符串
//     */
//    public void OKSDKLogout(string objName, string ext){
//#if UNITY_ANDROID
//        if (mAndroidObj != null){
//            log("logout", "ext is " + ext);
//            try
//            {
//                mAndroidObj.Call("OKSDKLogout", objName, ext);
//            }
//            catch (System.Exception ex)
//            {
//                log("OKSDKLogout", ex.ToString());
//            }
            
//        }
//#elif UNITY_IPHONE
//        oksdkLogout(ext);
//#endif
//    }

//    /**
//     *  打开平台的用户中心界面，有的平台可能没有提供用户中心的界面
//     *  所以在某些平台，调用该接口不会有任何作用
//     *
//     *  ext
//     *      json格式字符串，该参数在android暂时无用
//     */
//    public void OKSDKUserCenter(string ext) {
//#if UNITY_ANDROID
//        if (mAndroidObj != null){
//            log("userCenter", "ext is " + ext);
//            try
//            {
//                mAndroidObj.Call("OKSDKUserCenter", ext);
//            }
//            catch (System.Exception ex)
//            {
//                log("OKSDKUserCenter", ex.ToString());
//            }
           
//        }
//#elif UNITY_IPHONE
//        oksdkUserCenter(ext);
//#endif
//                                            }

//    public void OKSDKPay(string objName, string amount, string customInfo, string productName, string productId, string ext) {
//#if UNITY_ANDROID
//        if (mAndroidObj != null)
//        {
//            try
//            {
//                mAndroidObj.Call("OKSDKPay", objName, amount, customInfo, productName, productId, ext);
//            }
//            catch (System.Exception ex)
//            {
//                log("OKSDKPay", ex.ToString());
//            }
            
//        }
//#elif UNITY_IPHONE
//        oksdkPayment(productId, amount, productName,customInfo, ext);
//#endif
//                                                                                                                             }

//    public void OKSDKEnterBBS(string ext) {
//#if UNITY_ANDROID
//        if (mAndroidObj != null)
//        {
//            log("enterBBS", "ext is " + ext);
//           try
//           {
//                mAndroidObj.Call("OKSDKEnterBBS", ext);
//           }
//           catch (System.Exception ex)
//           {
//                log("OKSDKEnterBBS", ex.ToString());
//           }
            
//        }
//#elif UNITY_IPHONE
//        oksdkEnterBBS(ext);
//#endif
//                                          }

//    public void OKSDKLevelUp(string level)
//    {
//#if UNITY_ANDROID
//        if (mAndroidObj != null){
//         try
//         {
//            mAndroidObj.Call("OKSDKLevelUp", level);
//         }
//         catch (System.Exception ex)
//         {
//            log("OKSDKLevelUp", ex.ToString());
//         }
           
//        }
//#endif
//    }

//    /************************下面的方法只有android才会使用*************************/
//    /**
//     *  当退出游戏时使用，释放sdk占用资源，android使用接口
//     *  ext:
//     *      扩展参数，目前没有用，json格式
//     */
//    public void OKSDKExit(string objName) 
//    {
//#if UNITY_ANDROID
//        if (mAndroidObj != null)
//        {
//            log("exit", "");
//            try
//            {
//                mAndroidObj.Call("OKSDKExit", objName);
//            }
//            catch (System.Exception ex)
//            {
//                log("OKSDKExit", ex.ToString());
//            }
           
//        }
//#elif UNITY_IPHONE
//       oksdkLeavePlatform();
//#endif

//    }
    public void log(string tag, string msg)
    {
        Debug.Log(tag + "  " + msg);
    }
}
