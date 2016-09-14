//using UnityEngine;
//using System.Collections;
//using System.Runtime.InteropServices;
///**
// *  该文件包含常用的接口定义实现，对于一些特殊的，不太常见的接口，我们会定义在PlatformStateExtension.cs中
// */

//public partial class PlatformState 
//{
//    private static PlatformState instance;
//    public static PlatformState Instance
//    {
//        get 
//        {
//            if (instance == null) instance = new PlatformState();
//            return instance;
//        }
//    }

//#if UNITY_ANDROID
//    private AndroidJavaObject mAndroidObj;
//#endif

//#if UNITY_IPHONE
//    [DllImport("__Internal")]
//    private static extern void oksdkSetMessageObjName(string objName);
	
//    [DllImport("__Internal")]
//    private static extern  void oksdkInit(string customInfo);
	
//    [DllImport("__Internal")]
//    private static extern void oksdkLogin(string customInfo);

//    [DllImport("__Internal")]
//    private static extern void oksdkCreateRole(string customInfo);
	
//    [DllImport("__Internal")]
//    private static extern void oksdkEnterGame(string customInfo);
	
//    [DllImport("__Internal")]
//    private static extern void oksdkUserCenter(string customInfo);
    
//    [DllImport("__Internal")]
//    private static extern void oksdkEnterBBS(string customInfo);
	
//    [DllImport("__Internal")]
//    private static extern void oksdkLogout(string customInfo);
	
//    [DllImport("__Internal")]
//    private static extern void oksdkLeavePlatform();
	
//    [DllImport("__Internal")]
//    private static extern void oksdkPayment(string pid,string amount,string productName,string customInfo,string ext);

//    [DllImport("__Internal")]
//    private static extern void okLog(string str);
//#endif

//                                        /**
//     *  初始化平台
//     *  ext:
//     *      json字符串，具体格式为，不能为空
//     */
//    public void OKSDKInit(string objName, string ext)
//    {
//        Debug.Log("objname : " + objName + " ext : " + ext);
//#if UNITY_ANDROID
//        if (mAndroidObj == null)
//        {

//            try
//            {
//                mAndroidObj = new AndroidJavaObject("com.oksdk.helper.UnityPluginInterface");
//            } 
//            catch (System.Exception ex) 
//            {
//                log("new AndroidJavaObject", ex.ToString());
           
//            }
//        }

//        if (mAndroidObj != null)
//        {
//            log("init", "ext is " + ext);
//           try
//           {
//               mAndroidObj.Call("OKSDKInit", objName, ext);
//           }
//           catch(System.Exception e)
//           {
//               log("OKSDKInit", e.ToString());
//           }
//        }
//#elif UNITY_IPHONE
//        oksdkSetMessageObjName(objName);
//        oksdkInit(ext);
//#endif
//    }      


//    /**
//     *  启动登录页面
//     *  ext:
//     *      json格式字符串
//     */
//    public void OKSDKLogin(string objName, string ext){
//#if UNITY_ANDROID
//        if (mAndroidObj != null)
//        {
//            log("login", "ext is " + ext);
//            try
//            {
//                mAndroidObj.Call("OKSDKLogin", objName, ext);
//            }
//            catch(System.Exception ex)
//            {
//                log("OKSDKLogin", ex.ToString());
//            }
//        }
//#elif UNITY_IPHONE
//        oksdkLogin(ext);
//#endif
//    }

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
//    public void log(string tag, string msg)
//    {
//        Debug.Log(tag + "  " + msg);
//    }
//    public void OKLog(string msg)
//    {
//#if UNITY_ANDROID
//        UnityEngine.Debug.Log(msg);
//#elif UNITY_IPHONE
//        okLog(msg);
//#endif
//    }


//}
