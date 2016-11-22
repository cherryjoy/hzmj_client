#import "UnityAppController.h"
#import "UnityAppController+ViewHandling.h"
#import "UnityAppController+Rendering.h"
#import "iPhone_Sensors.h"

#import <CoreGraphics/CoreGraphics.h>
#import <QuartzCore/QuartzCore.h>
#import <QuartzCore/CADisplayLink.h>
#import <Availability.h>

#import <OpenGLES/EAGL.h>
#import <OpenGLES/EAGLDrawable.h>
#import <OpenGLES/ES2/gl.h>
#import <OpenGLES/ES2/glext.h>

#include <mach/mach_time.h>

// MSAA_DEFAULT_SAMPLE_COUNT was moved to iPhone_GlesSupport.h
// ENABLE_INTERNAL_PROFILER and related defines were moved to iPhone_Profiler.h
// kFPS define for removed: you can use Application.targetFrameRate (30 fps by default)
// DisplayLink is the only run loop mode now - all others were removed

#include "CrashReporter.h"

#include "UI/OrientationSupport.h"
#include "UI/UnityView.h"
#include "UI/Keyboard.h"
#include "UI/SplashScreen.h"
#include "Unity/InternalProfiler.h"
#include "Unity/DisplayManager.h"
#include "Unity/EAGLContextHelper.h"
#include "Unity/GlesHelper.h"
#include "PluginBase/AppDelegateListener.h"

#pragma mark - WXApiDelegate

#define WeiXinID @"wx71ed68a2c3f97352"
#define WeiXinSecret @"0586d4bede4d2044251bc10893f6f140"
#define WeiXinName @"樱桃红中麻将"
#define ksendAuthRequestNotification @"ksendAuthRequestNotification"
#define InitMethod "onInitFinish"
//#define InitCode "{\"code\":1}"
#define LoginMethod "onLoginFinish"
#define ShareMethod "onShareFinish"
#define EnterMethod "onEnterFinish"
#define BUFFER_SIZE 1024 * 100
static NSString* myGameObject;
static NSString* myAppId;
static NSString* myAppSecret;
static NSString* myExtent = @"{\"code\":1}";

bool	_ios42orNewer			= false;
bool	_ios43orNewer			= false;
bool	_ios50orNewer			= false;
bool	_ios60orNewer			= false;
bool	_ios70orNewer			= false;
bool	_ios80orNewer			= false;
bool	_ios81orNewer			= false;
bool	_ios82orNewer			= false;
bool 	_ios90orNewer			= false;
bool 	_ios91orNewer			= false;

// was unity rendering already inited: we should not touch rendering while this is false
bool	_renderingInited		= false;
// was unity inited: we should not touch unity api while this is false
bool	_unityAppReady			= false;
// see if there's a need to do internal player pause/resume handling
//
// Typically the trampoline code should manage this internally, but
// there are use cases, videoplayer, plugin code, etc where the player
// is paused before the internal handling comes relevant. Avoid
// overriding externally managed player pause/resume handling by
// caching the state
bool	_wasPausedExternal		= false;
// should we skip present on next draw: used in corner cases (like rotation) to fill both draw-buffers with some content
bool	_skipPresent			= false;
// was app "resigned active": some operations do not make sense while app is in background
bool	_didResignActive		= false;

// was startUnity scheduled: used to make startup robust in case of locking device
static bool	_startUnityScheduled	= false;

bool	_supportsMSAA			= false;


@implementation UnityAppController

@synthesize unityView				= _unityView;
@synthesize unityDisplayLink		= _unityDisplayLink;

@synthesize rootView				= _rootView;
@synthesize rootViewController		= _rootController;
@synthesize mainDisplay				= _mainDisplay;
@synthesize renderDelegate			= _renderDelegate;
@synthesize quitHandler				= _quitHandler;

#if !UNITY_TVOS
@synthesize interfaceOrientation	= _curOrientation;
#endif

- (id)init
{
	if( (self = [super init]) )
	{
		// due to clang issues with generating warning for overriding deprecated methods
		// we will simply assert if deprecated methods are present
		// NB: methods table is initied at load (before this call), so it is ok to check for override
		NSAssert(![self respondsToSelector:@selector(createUnityViewImpl)],
			@"createUnityViewImpl is deprecated and will not be called. Override createUnityView"
		);
		NSAssert(![self respondsToSelector:@selector(createViewHierarchyImpl)],
			@"createViewHierarchyImpl is deprecated and will not be called. Override willStartWithViewController"
		);
		NSAssert(![self respondsToSelector:@selector(createViewHierarchy)],
			@"createViewHierarchy is deprecated and will not be implemented. Use createUI"
		);
	}
	return self;
}


- (void)setWindow:(id)object		{}
- (UIWindow*)window					{ return _window; }


- (void)shouldAttachRenderDelegate	{}
- (void)preStartUnity				{}


- (void)startUnity:(UIApplication*)application
{
	NSAssert(_unityAppReady == NO, @"[UnityAppController startUnity:] called after Unity has been initialized");

	UnityInitApplicationGraphics();

	// we make sure that first level gets correct display list and orientation
	[[DisplayManager Instance] updateDisplayListInUnity];

	UnityLoadApplication();
	Profiler_InitProfiler();

	[self showGameUI];
	[self createDisplayLink];

	UnitySetPlayerFocus(1);
}

extern "C" void UnityRequestQuit()
{
	_didResignActive = true;
	if (GetAppController().quitHandler)
		GetAppController().quitHandler();
	else
		exit(0);
}

#if !UNITY_TVOS
- (NSUInteger)application:(UIApplication*)application supportedInterfaceOrientationsForWindow:(UIWindow*)window
{
	// UIInterfaceOrientationMaskAll
	// it is the safest way of doing it:
	// - GameCenter and some other services might have portrait-only variant
	//     and will throw exception if portrait is not supported here
	// - When you change allowed orientations if you end up forbidding current one
	//     exception will be thrown
	// Anyway this is intersected with values provided from UIViewController, so we are good
	return   (1 << UIInterfaceOrientationPortrait) | (1 << UIInterfaceOrientationPortraitUpsideDown)
		   | (1 << UIInterfaceOrientationLandscapeRight) | (1 << UIInterfaceOrientationLandscapeLeft);
}
#endif

#if !UNITY_TVOS
- (void)application:(UIApplication*)application didReceiveLocalNotification:(UILocalNotification*)notification
{
	AppController_SendNotificationWithArg(kUnityDidReceiveLocalNotification, notification);
	UnitySendLocalNotification(notification);
}
#endif

- (void)application:(UIApplication*)application didReceiveRemoteNotification:(NSDictionary*)userInfo
{
	AppController_SendNotificationWithArg(kUnityDidReceiveRemoteNotification, userInfo);
	UnitySendRemoteNotification(userInfo);
}

- (void)application:(UIApplication*)application didRegisterForRemoteNotificationsWithDeviceToken:(NSData*)deviceToken
{
	AppController_SendNotificationWithArg(kUnityDidRegisterForRemoteNotificationsWithDeviceToken, deviceToken);
	UnitySendDeviceToken(deviceToken);
}

#if !UNITY_TVOS
- (void)application:(UIApplication *)application didReceiveRemoteNotification:(NSDictionary *)userInfo fetchCompletionHandler:(void (^)(UIBackgroundFetchResult result))handler
{
	AppController_SendNotificationWithArg(kUnityDidReceiveRemoteNotification, userInfo);
	UnitySendRemoteNotification(userInfo);
	if (handler)
	{
		handler(UIBackgroundFetchResultNoData);
	}
}
#endif

- (void)application:(UIApplication*)application didFailToRegisterForRemoteNotificationsWithError:(NSError*)error
{
	AppController_SendNotificationWithArg(kUnityDidFailToRegisterForRemoteNotificationsWithError, error);
	UnitySendRemoteNotificationError(error);
}

- (BOOL)application:(UIApplication *)application handleOpenURL:(NSURL *)url
{
    return [WXApi handleOpenURL:url delegate:self];
}

- (BOOL)application:(UIApplication*)application openURL:(NSURL*)url sourceApplication:(NSString*)sourceApplication annotation:(id)annotation
{
	NSMutableArray* keys	= [NSMutableArray arrayWithCapacity:3];
	NSMutableArray* values	= [NSMutableArray arrayWithCapacity:3];

	#define ADD_ITEM(item)	do{ if(item) {[keys addObject:@#item]; [values addObject:item];} }while(0)

	ADD_ITEM(url);
	ADD_ITEM(sourceApplication);
	ADD_ITEM(annotation);

	#undef ADD_ITEM

	NSDictionary* notifData = [NSDictionary dictionaryWithObjects:values forKeys:keys];
	AppController_SendNotificationWithArg(kUnityOnOpenURL, notifData);
	return [WXApi handleOpenURL:url delegate:self];
}

-(BOOL)application:(UIApplication*)application willFinishLaunchingWithOptions:(NSDictionary*)launchOptions
{
	return YES;
}

- (BOOL)application:(UIApplication*)application didFinishLaunchingWithOptions:(NSDictionary*)launchOptions
{
	::printf("-> applicationDidFinishLaunching()\n");
    [[NSNotificationCenter defaultCenter] addObserver:self selector:@selector(sendAuthRequest) name:ksendAuthRequestNotification object:nil]; // 微信
    
    //向微信注册
    [WXApi registerApp:WeiXinID withDescription:WeiXinName];

	// send notfications
#if !UNITY_TVOS
	if(UILocalNotification* notification = [launchOptions objectForKey:UIApplicationLaunchOptionsLocalNotificationKey])
		UnitySendLocalNotification(notification);

	if(NSDictionary* notification = [launchOptions objectForKey:UIApplicationLaunchOptionsRemoteNotificationKey])
		UnitySendRemoteNotification(notification);

	if ([UIDevice currentDevice].generatesDeviceOrientationNotifications == NO)
		[[UIDevice currentDevice] beginGeneratingDeviceOrientationNotifications];
#endif

	UnityInitApplicationNoGraphics([[[NSBundle mainBundle] bundlePath] UTF8String]);

	[self selectRenderingAPI];
	[UnityRenderingView InitializeForAPI:self.renderingAPI];

	_window			= [[UIWindow alloc] initWithFrame:[UIScreen mainScreen].bounds];
	_unityView		= [self createUnityView];

	[DisplayManager Initialize];
	_mainDisplay	= [DisplayManager Instance].mainDisplay;
	[_mainDisplay createWithWindow:_window andView:_unityView];

	[self createUI];
	[self preStartUnity];

	// if you wont use keyboard you may comment it out at save some memory
	[KeyboardDelegate Initialize];

	return YES;
}

- (void)applicationDidEnterBackground:(UIApplication*)application
{
	::printf("-> applicationDidEnterBackground()\n");
}

- (void)applicationWillEnterForeground:(UIApplication*)application
{
	::printf("-> applicationWillEnterForeground()\n");

	// applicationWillEnterForeground: might sometimes arrive *before* actually initing unity (e.g. locking on startup)
	if(_unityAppReady)
	{
		// if we were showing video before going to background - the view size may be changed while we are in background
		[GetAppController().unityView recreateGLESSurfaceIfNeeded];
	}
}

- (void)applicationDidBecomeActive:(UIApplication*)application
{
	::printf("-> applicationDidBecomeActive()\n");

	[self removeSnapshotView];

	if(_unityAppReady)
	{
		if(UnityIsPaused() && _wasPausedExternal == false)
		{
			UnityWillResume();
			UnityPause(0);
		}
		UnitySetPlayerFocus(1);
	}
	else if(!_startUnityScheduled)
	{
		_startUnityScheduled = true;
		[self performSelector:@selector(startUnity:) withObject:application afterDelay:0];
	}

	_didResignActive = false;
}

- (void)removeSnapshotView
{
	[_snapshotView removeFromSuperview];
	_snapshotView = nil;
}

- (void)applicationWillResignActive:(UIApplication*)application
{
	::printf("-> applicationWillResignActive()\n");

	if(_unityAppReady)
	{
		UnitySetPlayerFocus(0);

		_wasPausedExternal = UnityIsPaused();
		if (_wasPausedExternal == false)
		{
			// do pause unity only if we dont need special background processing
			// otherwise batched player loop can be called to run user scripts
			int bgBehavior = UnityGetAppBackgroundBehavior();
			if(bgBehavior == appbgSuspend || bgBehavior == appbgExit)
			{
				// Force player to do one more frame, so scripts get a chance to render custom screen for minimized app in task manager.
				// NB: UnityWillPause will schedule OnApplicationPause message, which will be sent normally inside repaint (unity player loop)
				// NB: We will actually pause after the loop (when calling UnityPause).
				UnityWillPause();
				[self repaint];
				UnityPause(1);

				// this is done on the next frame so that
				// in the case where unity is paused while going 
				// into the background and an input is deactivated
				// we don't mess with the view hierarchy while taking
				// a view snapshot (case 760747).
				dispatch_async(dispatch_get_main_queue(), ^{
					if (_didResignActive)
					{
						[self removeSnapshotView];

						_snapshotView = [self createSnapshotView];
						if(_snapshotView)
						[_rootView addSubview:_snapshotView];
					}
				});
			}
		}
	}

	_didResignActive = true;
}

- (void)applicationDidReceiveMemoryWarning:(UIApplication*)application
{
	::printf("WARNING -> applicationDidReceiveMemoryWarning()\n");
}

- (void)applicationWillTerminate:(UIApplication*)application
{
	::printf("-> applicationWillTerminate()\n");

	Profiler_UninitProfiler();
	UnityCleanup();

	extern void SensorsCleanup();
	SensorsCleanup();
}

extern "C"
{
    NSString* myCreateString(const char* str){
        if (str){
            return [NSString stringWithUTF8String:str];
        }
        return [NSString stringWithUTF8String:""];
    }
    
    bool isWXAppInstalled()
    {
        return [WXApi isWXAppInstalled];
    }
    bool isWXAppSupportApi()
    {
        return [WXApi isWXAppSupportApi];
    }
    
    void cjsdkInit(const char* objName, const char *ext)
    {
        myGameObject = [myCreateString(objName) mutableCopy];
        //NSLog(@"game object name is : %@", myGameObject);
        //NSLog(@"cjsdkInit params is : %s", ext);
        
        NSString* jsonString =  myCreateString(ext);
        NSData *data = [jsonString dataUsingEncoding:NSUTF8StringEncoding];
        NSError *jsonParseError;
        NSDictionary *responseData = [NSJSONSerialization JSONObjectWithData:data options:NSJSONReadingMutableContainers error:&jsonParseError];
        myAppId = [responseData valueForKey:@"appid"];
        myAppSecret = [responseData valueForKey:@"appsecret"];
        NSLog(@"cjsdkInit %@:%@",myAppId, myAppSecret);
        
        UnitySendMessage([myGameObject UTF8String], InitMethod, [myExtent cStringUsingEncoding:NSUTF8StringEncoding]);
    }
    
    // 给Unity3d调用的方法
    void WXLogin()
    {
        // 登录
        [[NSNotificationCenter defaultCenter] postNotificationName:ksendAuthRequestNotification object:nil];
    }
    
    // type 0 只刷新 1 刷新并返回消息
    void refreshAccessToken(NSString *code, int type)
    {
        NSString *path = code;
        NSLog(@"refreshAccessTokenUrl: %@", path);
        NSURLRequest *request = [[NSURLRequest alloc] initWithURL:[NSURL URLWithString:path] cachePolicy:NSURLRequestUseProtocolCachePolicy timeoutInterval:10.0];
        NSOperationQueue *queue = [[NSOperationQueue alloc] init];
        [NSURLConnection sendAsynchronousRequest:request queue:queue completionHandler:
         ^(NSURLResponse *response,NSData *data,NSError *connectionError)
         {
             if (connectionError != NULL)
             {
             }
             else
             {
                 if (data != NULL)
                 {
                     NSError *jsonParseError;
                     NSDictionary *responseData = [NSJSONSerialization JSONObjectWithData:data options:NSJSONReadingMutableContainers error:&jsonParseError];
                     
                     NSLog(@"#####refreshAccessToken = %@",responseData);
                     
                     int errCode = [[responseData valueForKey:@"errcode"] intValue];
                     //NSLog(@"errCode: %d", errCode);
                     
                     if (type == 1)
                     {
                         if (errCode == 0)
                         {
                             NSString *jsonData = [[NSString alloc] initWithData:data encoding:NSUTF8StringEncoding];
                             UnitySendMessage([myGameObject UTF8String], LoginMethod, [jsonData cStringUsingEncoding:NSUTF8StringEncoding]);
                         }
                         else
                         {
                             WXLogin();
                         }
                     }
                 }
             }
         }];
    }
    
    void cjsdkLogin(const char* objName, const char* ext){
        myGameObject = [myCreateString(objName) mutableCopy];
        //NSLog(@"game object name is : %@", myGameObject);
        NSLog(@"cjsdkLogin params is : %s", ext);
        
        NSString* jsonString =  myCreateString(ext);
        if (nil == jsonString || 0 == jsonString.length)
        {
            WXLogin();
        }
        else
        {
            NSData *data = [jsonString dataUsingEncoding:NSUTF8StringEncoding];
            NSError *jsonParseError;
            NSDictionary *responseData = [NSJSONSerialization JSONObjectWithData:data options:NSJSONReadingMutableContainers error:&jsonParseError];
            NSString *openId = [responseData valueForKey:@"openid"];
            NSString *accessToken = [responseData valueForKey:@"accessToken"];
            NSString *refresh_token = [responseData valueForKey:@"refresh_token"];
            NSString *refreshTokenUrl = [NSString stringWithFormat:@"https://api.weixin.qq.com/sns/oauth2/refresh_token?appid=%@&grant_type=refresh_token&refresh_token=%@",myAppId, refresh_token];
            
            
            NSString *authTokenUrl = [NSString stringWithFormat:@"https://api.weixin.qq.com/sns/auth?access_token=%@&openid=%@", accessToken, openId];
            NSLog(@"authTokenUrl: %@", authTokenUrl);
            NSURLRequest *request = [[NSURLRequest alloc] initWithURL:[NSURL URLWithString:authTokenUrl] cachePolicy:NSURLRequestUseProtocolCachePolicy timeoutInterval:10.0];
            NSOperationQueue *queue = [[NSOperationQueue alloc] init];
            [NSURLConnection sendAsynchronousRequest:request queue:queue completionHandler:
             ^(NSURLResponse *response,NSData *data,NSError *connectionError)
             {
                 if (connectionError != NULL)
                 {
                 }
                 else
                 {
                     if (data != NULL)
                     {
                         NSError *jsonParseError;
                         NSDictionary *responseData = [NSJSONSerialization JSONObjectWithData:data options:NSJSONReadingMutableContainers error:&jsonParseError];
                         
                         NSLog(@"#####authTokenResult = %@",responseData);
                         if (jsonParseError != NULL)
                         {
                             //NSLog(@"#####responseData = %@",jsonParseError);
                         }
                         
                         int errCode = [[responseData valueForKey:@"errcode"] intValue];
                         //NSLog(@"errCode: %d", errCode);
                         if (errCode == 0)
                         {
                             NSString *jsonData = [[NSString alloc] initWithData:data encoding:NSUTF8StringEncoding];
                             //NSLog(@"#####JsonData = %@",jsonData);
                             UnitySendMessage([myGameObject UTF8String], LoginMethod, [jsonData cStringUsingEncoding:NSUTF8StringEncoding]);
                             
                             // refresh token
                             refreshAccessToken(refreshTokenUrl, 0);
                         }
                         else
                         {
                             refreshAccessToken(refreshTokenUrl, 1);
                         }
                     }
                 }
             }];
        }
    }
    
    void cjsdkShare(const char* objName, const char* title,const char*desc,const char*contextUrl, const char* imageUrl, const char* extent, const char* scene)
    {
        myGameObject = [myCreateString(objName) mutableCopy];
        //NSLog(@"game object name is : %@", myGameObject);
        NSString *titleStr=[NSString stringWithUTF8String:title];
        NSString *descStr=[NSString stringWithUTF8String:desc];
        NSString *urlStr=[NSString stringWithUTF8String:contextUrl];
        NSString *imageStr=[NSString stringWithUTF8String:imageUrl];
        NSString *extStr=[NSString stringWithUTF8String:extent];
        NSString *sceneStr=[NSString stringWithUTF8String:scene];
        //imageStr = @"screenshot.png";
        //NSLog(@"cjsdkShare titleStr:%@",titleStr);
        //NSLog(@"cjsdkShare descStr:%@",descStr);
        //NSLog(@"cjsdkShare urlStr:%@",urlStr);
        NSLog(@"cjsdkShare imageUrl:%@",imageStr);
        NSLog(@"cjsdkShare sceneStr:%@",sceneStr);
        int sceneType = WXSceneSession;
        if ([sceneStr  isEqual: @"0"])
        {
            sceneType = WXSceneSession;
        }
        else if ([sceneStr  isEqual: @"1"])
        {
            sceneType = WXSceneTimeline;
        }
        else
        {
            sceneType = WXSceneFavorite;
        }
        
        // 分享
        if (nil == imageStr || 0 == imageStr.length)
        {
            if (sceneType == WXSceneSession)
            {
                Byte* pBuffer = (Byte *)malloc(BUFFER_SIZE);
                memset(pBuffer, 0, BUFFER_SIZE);
                NSData* data = [NSData dataWithBytes:pBuffer length:BUFFER_SIZE];
                free(pBuffer);
                
                UIImage *thumbImage=[UIImage imageNamed:@"AppIcon60x60@2x"];
                WXAppExtendObject *ext = [WXAppExtendObject object];
                ext.extInfo = extStr;
                ext.url = urlStr;
                ext.fileData = data;
                WXMediaMessage *message = [WXMediaMessage message];
                message.title = titleStr;
                message.description = descStr;
                message.mediaObject = ext;
                message.messageExt = @"test";
                message.messageAction = @"<action>dotaliTest</action>";
                message.mediaTagName = @"WECHAT_TAG_SHARE";
                [message setThumbImage:thumbImage];
            
                SendMessageToWXReq* req = [[SendMessageToWXReq alloc] init];
                req.bText = NO;
                req.message = message;
                req.scene = WXSceneSession;
                [WXApi sendReq:req];
            }
            else
            {
                WXMediaMessage *message = [WXMediaMessage message];
                message.title = titleStr;
                message.description = descStr;
                UIImage *img=[UIImage imageNamed:@"AppIcon60x60@2x"];
                [message setThumbImage:img];
                WXWebpageObject *ext = [WXWebpageObject object];
                ext.webpageUrl = urlStr;
                message.mediaObject = ext;
                message.mediaTagName = @"WECHAT_TAG_SHARE";
                SendMessageToWXReq* req = [[SendMessageToWXReq alloc] init];
                req.bText = NO;
                req.message = message;
                req.scene = sceneType;
                [WXApi sendReq:req];
            }
        }
        else
        {
            WXMediaMessage *message = [WXMediaMessage message];
            NSString *path=[NSString stringWithFormat:@"%@/Documents/%@",NSHomeDirectory(),imageStr];
            //NSLog(@"image path: %@", path);
            WXImageObject *imageObject = [WXImageObject object];
            imageObject.imageData = [NSData dataWithContentsOfFile:path];
            message.mediaObject = imageObject;
            UIImage *img=[[UIImage alloc]initWithContentsOfFile:path];
            CGSize size = img.size;
            CGFloat width = size.width;
            CGFloat height = size.height;
            CGFloat scaledWidth = 400;//width*0.05;
            CGFloat scaledHeight = 400*(height/width);
            UIGraphicsBeginImageContext(CGSizeMake(scaledWidth,scaledHeight));//thiswillcrop
            [img drawInRect:CGRectMake(0,0,scaledWidth,scaledHeight)];
            UIImage* newImage=UIGraphicsGetImageFromCurrentImageContext();
            UIGraphicsEndImageContext();
            NSData *compImg = UIImageJPEGRepresentation(newImage, 0.5);
            [message setThumbImage:[UIImage imageWithData:compImg]];
            
            //UIImage *imgJPeg = [UIImage imageWithData:compImg];//建立UIIMage为jpeg格式
            //UIImageWriteToSavedPhotosAlbum(imgJPeg,nil,nil,nil);//保存到相册
            
            SendMessageToWXReq* req = [[SendMessageToWXReq alloc] init];
            req.bText = NO;
            req.message = message;
            req.scene = sceneType;
            [WXApi sendReq:req];
        }
    }
}

- (void)onReq:(BaseReq *)req // 微信向第三方程序发起请求,要求第三方程序响应
{
    if ([req isKindOfClass:[ShowMessageFromWXReq class]]) {
        ShowMessageFromWXReq *showMessageReq = (ShowMessageFromWXReq *)req;
        WXMediaMessage *msg = showMessageReq.message;
        
        //显示微信传过来的内容
        WXAppExtendObject *obj = msg.mediaObject;
        
        //NSString *strTitle = [NSString stringWithFormat:@"微信请求App显示内容"];
        NSString *strMsg = [NSString stringWithFormat:@"openID: %@, 标题：%@ \n内容：%@ \n附带信息：%@ \n缩略图:%lu bytes\n附加消息:%@\n", req.openID, msg.title, msg.description, obj.extInfo, (unsigned long)msg.thumbData.length, msg.messageExt];
        
        NSLog(@"%@", strMsg);
        myExtent = obj.extInfo;
        if (nil != myAppSecret)
        {
            NSLog(@"UnitySendMessage %@", myExtent);
            UnitySendMessage([myGameObject UTF8String], EnterMethod, [myExtent cStringUsingEncoding:NSUTF8StringEncoding]);
        }
    } else if ([req isKindOfClass:[LaunchFromWXReq class]]) {
        LaunchFromWXReq *launchReq = (LaunchFromWXReq *)req;
        WXMediaMessage *msg = launchReq.message;
        //从微信启动App
        //NSString *strTitle = [NSString stringWithFormat:@"从微信启动"];
        NSString *strMsg = [NSString stringWithFormat:@"openID: %@, messageExt:%@", req.openID, msg.messageExt];
        NSLog(@"%@", strMsg);
    }
}

- (void)onResp:(BaseResp *)resp // 第三方程序向微信发送了sendReq的请求,那么onResp会被回调
{
    if([resp isKindOfClass:[SendAuthResp class]]) // 登录授权
    {
        SendAuthResp *temp = (SendAuthResp*)resp;
        NSLog(@"SendAuthResp:%@",temp.code);
        //if(temp.code!=nil)UnitySendMessage([myGameObject UTF8String], LoginMethod, [temp.code cStringUsingEncoding:NSUTF8StringEncoding]);
        
        [self getAccessToken:temp.code];
    }
    else if([resp isKindOfClass:[SendMessageToWXResp class]])
    {
        // 分享
        if(resp.errCode==0)
        {
            NSString *code = [NSString stringWithFormat:@"%d",resp.errCode]; // 0是成功 -2是取消
            NSLog(@"SendMessageToWXResp:%@",code);
            UnitySendMessage([myGameObject UTF8String], ShareMethod, [code cStringUsingEncoding:NSUTF8StringEncoding]);
        }
    }
}

#pragma mark - Private

- (void)sendAuthRequest
{
    SendAuthReq* req = [[SendAuthReq alloc] init];
    req.scope = @"snsapi_userinfo";
    req.state = @"only123";
    [WXApi sendAuthReq:req viewController:_rootController delegate:self];
}

- (void)getAccessToken:(NSString *)code
{
    NSString *path = [NSString stringWithFormat:@"https://api.weixin.qq.com/sns/oauth2/access_token?appid=%@&secret=%@&code=%@&grant_type=authorization_code",myAppId,myAppSecret,code];
    NSLog(@"getAccessTokenUrl: %@", path);
    NSURLRequest *request = [[NSURLRequest alloc] initWithURL:[NSURL URLWithString:path] cachePolicy:NSURLRequestUseProtocolCachePolicy timeoutInterval:10.0];
    NSOperationQueue *queue = [[NSOperationQueue alloc] init];
    [NSURLConnection sendAsynchronousRequest:request queue:queue completionHandler:
     ^(NSURLResponse *response,NSData *data,NSError *connectionError)
     {
         if (connectionError != NULL)
         {
         }
         else
         {
             if (data != NULL)
             {
                 NSError *jsonParseError;
                 NSDictionary *responseData = [NSJSONSerialization JSONObjectWithData:data options:NSJSONReadingMutableContainers error:&jsonParseError];
                 
                 NSLog(@"#####responseData = %@",responseData);
                 NSString *jsonData = [[NSString alloc] initWithData:data encoding:NSUTF8StringEncoding];
                 NSLog(@"#####JsonData = %@",jsonData);
                 UnitySendMessage([myGameObject UTF8String], LoginMethod, [jsonData cStringUsingEncoding:NSUTF8StringEncoding]);
                 if (jsonParseError != NULL)
                 {
                     //NSLog(@"#####responseData = %@",jsonParseError);
                 }
             }
         }
     }];
}

- (void)getUserInfo:(NSString *)accessToken withOpenID: (NSString *)openid
{
    NSString *path = [NSString stringWithFormat:@"https://api.weixin.qq.com/sns/userinfo?access_token=%@&openid=%@",accessToken,openid];
    NSLog(@"getUserInfoUrl: %@", path);
    NSURLRequest *request = [[NSURLRequest alloc] initWithURL:[NSURL URLWithString:path] cachePolicy:NSURLRequestUseProtocolCachePolicy timeoutInterval:10.0];
    NSOperationQueue *queue = [[NSOperationQueue alloc] init];
    [NSURLConnection sendAsynchronousRequest:request queue:queue completionHandler:
     ^(NSURLResponse *response,NSData *data,NSError *connectionError) {
         if (connectionError != NULL)
         {
         }
         else
         {
             if (data != NULL) {
                 NSError *jsonError;
                 NSString *responseData = [NSJSONSerialization JSONObjectWithData:data options:NSJSONReadingMutableContainers error:&jsonError];
                 NSLog(@"#####responseData = %@",responseData);
                 //NSString *jsonData = [NSString stringWithFormat:@"%@",responseData];
                 //UnitySendMessage([myGameObject UTF8String], LoginMethod, [jsonData cStringUsingEncoding:NSUTF8StringEncoding]);
                 if (jsonError != NULL) {
                     //NSLog(@"#####responseData = %@",jsonError);
                 }
             }
         }
     }];
}

#pragma mark -

@end


void AppController_SendNotification(NSString* name)
{
	[[NSNotificationCenter defaultCenter] postNotificationName:name object:GetAppController()];
}
void AppController_SendNotificationWithArg(NSString* name, id arg)
{
	[[NSNotificationCenter defaultCenter] postNotificationName:name object:GetAppController() userInfo:arg];
}
void AppController_SendUnityViewControllerNotification(NSString* name)
{
	[[NSNotificationCenter defaultCenter] postNotificationName:name object:UnityGetGLViewController()];
}

extern "C" UIWindow*			UnityGetMainWindow()		{ return GetAppController().mainDisplay.window; }
extern "C" UIViewController*	UnityGetGLViewController()	{ return GetAppController().rootViewController; }
extern "C" UIView*				UnityGetGLView()			{ return GetAppController().unityView; }
extern "C" ScreenOrientation	UnityCurrentOrientation()	{ return GetAppController().unityView.contentOrientation; }



bool LogToNSLogHandler(LogType logType, const char* log, va_list list)
{
	NSLogv([NSString stringWithUTF8String:log], list);
	return true;
}

void UnityInitTrampoline()
{
#if ENABLE_CRASH_REPORT_SUBMISSION
	SubmitCrashReportsAsync();
#endif
	InitCrashHandling();

	NSString* version = [[UIDevice currentDevice] systemVersion];

	// keep native plugin developers happy and keep old bools around
	_ios42orNewer = true;
	_ios43orNewer = true;
	_ios50orNewer = true;
	_ios60orNewer = true;
	_ios70orNewer = [version compare: @"7.0" options: NSNumericSearch] != NSOrderedAscending;
	_ios80orNewer = [version compare: @"8.0" options: NSNumericSearch] != NSOrderedAscending;
	_ios81orNewer = [version compare: @"8.1" options: NSNumericSearch] != NSOrderedAscending;
	_ios82orNewer = [version compare: @"8.2" options: NSNumericSearch] != NSOrderedAscending;
	_ios90orNewer = [version compare: @"9.0" options: NSNumericSearch] != NSOrderedAscending;
	_ios91orNewer = [version compare: @"9.1" options: NSNumericSearch] != NSOrderedAscending;

	// Try writing to console and if it fails switch to NSLog logging
	::fprintf(stdout, "\n");
	if(::ftell(stdout) < 0)
		UnitySetLogEntryHandler(LogToNSLogHandler);
}

