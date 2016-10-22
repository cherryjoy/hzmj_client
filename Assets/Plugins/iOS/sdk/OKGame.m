

#import "OKGame.h"
#include <sys/socket.h>
#include <netdb.h>
#include <arpa/inet.h>
#include <err.h>
#include <sys/sysctl.h>
#include <net/if.h>
#include <net/if_dl.h>
#include <string.h>
#import <AdSupport/AdSupport.h>
static NSString* gameObject;
#define MakeStringCopy( _x_ ) ( _x_ != NULL && [_x_ isKindOfClass:[NSString class]] ) ? strdup( [_x_ UTF8String] ) : NULL
NSString* CreateString(const char* str);

#if defined(__cplusplus)
extern "C"{
#endif
    
void UnitySendMessage(const char*, const char*, const char*);
void SendMessage2Unity(NSString* func, NSString* result);

#if defined(__cplusplus)
    extern "C"}
#endif

@implementation OKGame

    -(void)initSuccessWithResult:(NSString *)result
    {
        NSLog(@"initSuccessWithResult:%@",result);
        SendMessage2Unity(@"onInitFinish", result);
    }

    -(void)initFalied
    {
        NSLog(@"initFalied");
    }

    -(void)loginSuccessWithResult:(NSString *)result
    {
        NSLog(@"loginSuccessWithResult:%@",result);
        SendMessage2Unity(@"onLoginFinish", result);
    }

    -(void)loginFailedWithResult:(NSString *)result
    {
        NSLog(@"loginFailedWithResult:%@",result);
        SendMessage2Unity(@"onLoginFinish", result);
    }

    -(void)logoutSuccessWithResult:(NSString *)result
    {
        NSLog(@"logoutSuccessWithResult:%@",result);
        SendMessage2Unity(@"onLogoutFinish", result);
    }

    -(void)paySuccessWithResult:(NSString *)result
    {
        NSLog(@"paySuccessWithResult:%@",result);
        SendMessage2Unity(@"OnPayFinish", result);
    }

    -(void)payFailedWithResult:(NSString *)result
    {
        NSLog(@"payFailedWithResult:%@",result);
    }

@end


#if defined(__cplusplus)
 extern "C"{
#endif

    void SendMessage2Unity(NSString* func, NSString* result){
        UnitySendMessage([gameObject UTF8String],[func UTF8String],[result UTF8String]);
    }
	char* makeStringCopy(const char* string)  
    {  
        if (NULL == string) {  
            return NULL;  
        }  
        char* res = (char*)malloc(strlen(string)+1);  
        strcpy(res, string);  
        return res;  
    }
	
    NSString* CreateString(const char* str){
     if (str){
         return [NSString stringWithUTF8String:str];
     }
     return [NSString stringWithUTF8String:""];
    }

    //===================================================================================

    void cjsdkInit(const char* objName, const char *params){
        gameObject = [CreateString(objName) mutableCopy];
        NSLog(@"game object name is : %@", gameObject);
        
       //OKGame *okGame = [[OKGame alloc]init];
	   NSString* str =  CreateString(params);
	   str = [str stringByReplacingOccurrencesOfString:@"gameid" withString:@"gameId"];
	   str = [str stringByReplacingOccurrencesOfString:@"appkey" withString:@"appKey"];
	   str = [str stringByReplacingOccurrencesOfString:@"serverid" withString:@"serverId"];
	   NSLog(@"--------------%@",str);
        //[[OKSDK defaultSDK] OKSDKInitWithParams:str withDelegate:okGame];
    }

    void cjsdkLogin(const char* objName, const char* ext){
        gameObject = [CreateString(objName) mutableCopy];
        NSLog(@"game object name is : %@", gameObject);
        
		NSLog(@"oksdklogin %s",ext);
        //[[OKSDK defaultSDK] OKSDKLoginWithExt:CreateString(ext)];
    }

    void oksdkPayment(const char *productuid, const char *amount,const char *productname, const char *custominfo,const char *ext)
	{
		NSLog(@"pid  %@", CreateString(productuid) );
		NSLog(@"amount  %@", CreateString(amount) );
		NSLog(@"productname  %@", CreateString(productname) );
		NSLog(@"custominfo  %@", CreateString(custominfo) );
		NSLog(@"ext  %@", CreateString(ext) );
		//[[OKSDK defaultSDK] OKSDKPayWithAmount:CreateString(amount) AndCustomInfo:CreateString(custominfo) AndProductName:CreateString(productname) AndProductId:CreateString(productuid) AndExt:CreateString(ext)];
    }

	   const char * getGameDefineContent()
    {
		NSLog(@"getGameDefineContent");
		const char* gamedefineContent= [[[NSUserDefaults standardUserDefaults] objectForKey:@"gamedefineContent" ] UTF8String];
		NSLog(@"getGameDefineContent %s",gamedefineContent);
        char* str =  "GameDefine_version = \"1.0.1\" \n GameDefine_gameID = 78 \n GameDefine_platformCode = 5 \n GameDefine_outside_serverlist_url = \"http://192.168.252.12/serverlist/lk_auther2.xml\"";
		return makeStringCopy(str);
	}
    const char* getPlatformConfig(const char* key,const char* defaultvalue)
    {
		/*NSLog(@"getPlatformConfig  %s  %s",key,defaultvalue);
        NSString* keyStr=[NSString stringWithCString:key encoding:NSUTF8StringEncoding];
        const char* value =[[[[NSUserDefaults standardUserDefaults] objectForKey:@"platformDict" ] objectForKey:keyStr] UTF8String];
        if(value==NULL)
        {
            return makeStringCopy(defaultvalue);
        }*/
        return makeStringCopy("true");
    }
    const char * getPersistentDataPath()
    {
		NSLog(@"getPersistentDataPath");
		NSString * path = [[NSSearchPathForDirectoriesInDomains(NSDocumentDirectory, NSUserDomainMask, YES)[0] stringByAppendingString:@"/Assets/"]mutableCopy];
        return makeStringCopy([path UTF8String]);
    }
    const char* getClientVersion()
    {
		NSLog(@"getClientVersion");
		NSString * doc = [NSSearchPathForDirectoriesInDomains(NSDocumentDirectory, NSUserDomainMask, YES)[0] stringByAppendingString:@"/Assets/"];
        NSString* clientVersion = [NSString stringWithContentsOfFile:[doc stringByAppendingString:@"ClientVersion.txt"] encoding:NSUTF8StringEncoding error:nil];
        return  makeStringCopy([clientVersion UTF8String]);
    }
	int getClientFreeStorage()
	{
		 NSDictionary *fattributes = [[NSFileManager defaultManager] attributesOfFileSystemForPath:NSHomeDirectory() error:nil];
		return  [[fattributes objectForKey:NSFileSystemFreeSize] unsignedLongLongValue ]/1024/1024;
	}
	
	const char*  getClientVersionCode()
	{
		NSString *version = [[[NSBundle mainBundle] infoDictionary] objectForKey:(NSString *)kCFBundleVersionKey];
		NSLog(version);
		return makeStringCopy([version UTF8String]);
	}
	
	const char* getVersionName()
	{
		NSString *version = [[[NSBundle mainBundle] infoDictionary] objectForKey:(NSString *)kCFBundleVersionKey];
		NSLog(version);
		return makeStringCopy([version UTF8String]);
	}
	const char* getIPv6(const char *mHost,const char *mPort)
{
	if( nil == mHost )
		return NULL;
	const char *newChar = "No";
	const char *cause = NULL;
	struct addrinfo* res0;
	struct addrinfo hints;
	struct addrinfo* res;
	int n, s;
	
	memset(&hints, 0, sizeof(hints));
	
	hints.ai_flags = AI_DEFAULT;
	hints.ai_family = PF_UNSPEC;
	hints.ai_socktype = SOCK_STREAM;
	
	if((n=getaddrinfo(mHost, "http", &hints, &res0))!=0)
	{
		printf("getaddrinfo error: %s\n",gai_strerror(n));
		return NULL;
	}
	
	struct sockaddr_in6* addr6;
	struct sockaddr_in* addr;
	NSString * NewStr = NULL;
	char ipbuf[32];
	s = -1;
	for(res = res0; res; res = res->ai_next)
	{
		if (res->ai_family == AF_INET6)
		{
			addr6 =( struct sockaddr_in6*)res->ai_addr;
			newChar = inet_ntop(AF_INET6, &addr6->sin6_addr, ipbuf, sizeof(ipbuf));
			NSString * TempA = [[NSString alloc] initWithCString:(const char*)newChar 
encoding:NSASCIIStringEncoding];
			NSString * TempB = [NSString stringWithUTF8String:"=ipv6"];
			
			NewStr = [TempA stringByAppendingString: TempB];
			printf("%s\n", newChar);
		}
		else
		{
			addr =( struct sockaddr_in*)res->ai_addr;
			newChar = inet_ntop(AF_INET, &addr->sin_addr, ipbuf, sizeof(ipbuf));
			NSString * TempA = [[NSString alloc] initWithCString:(const char*)newChar 
encoding:NSASCIIStringEncoding];
			NSString * TempB = [NSString stringWithUTF8String:"=ipv4"];
			
			NewStr = [TempA stringByAppendingString: TempB];			
			printf("%s\n", newChar);
		}
		break;
	}
	
	
	freeaddrinfo(res0);
	
	printf("getaddrinfo OK");
	
	NSString * mIPaddr = NewStr;
	return MakeStringCopy(mIPaddr);
}
const char* GetMacAddressiOS()
{
	int mib[6];
        size_t len;
        char *buf;
        unsigned char macAddress[6];
        struct if_msghdr *ifm;
        struct sockaddr_dl *sdl;
        NSString* errorFlag = NULL;
        
        mib[0] = CTL_NET;
        mib[1] = AF_ROUTE;
        mib[2] = 0;
        mib[3] = AF_LINK;
        mib[4] = NET_RT_IFLIST;
        
        if ((mib[5] = if_nametoindex("en0")) == 0){
            errorFlag = @"if_nametoindex failure";
        }
        else {
            if (sysctl(mib, 6, NULL, &len, NULL, 0) < 0){
                errorFlag = @"sysctl mib failure";
            }else{
                if ((buf = (char*)malloc(len)) == NULL){
                    errorFlag = @"buf allocation failure";
                }
                else {
                    if (sysctl(mib, 6, buf, &len, NULL, 0) < 0){
                        errorFlag = @"sysctl buf failure";
                    }
                }
            }
            
        }
        
        
           if (errorFlag != NULL){
                NSLog(@"Error : %@", errorFlag);
                return NULL;
            }
        
        ifm = (struct if_msghdr*) buf;
        sdl = (struct sockaddr_dl*)(ifm + 1);
        memcpy(&macAddress, sdl->sdl_data + sdl->sdl_nlen, 6);
        NSString* outString = [NSString stringWithFormat:@"%02x%02x%02x%02x%02x%02x", macAddress[0], macAddress[1], macAddress[2], macAddress[3], macAddress[4], macAddress[5]];
        
        free(buf);
        if ([outString compare:@"020000000000"] == NSOrderedSame)
        {
            NSUUID* uuid = [[ASIdentifierManager sharedManager] advertisingIdentifier];
            NSString * result = [uuid UUIDString];
            outString = result;
        }
        return MakeStringCopy(outString);//[outString UTF8String];
}
	
/*	NSString* QCreateString(const char* str){
        if (str)
            return [NSString stringWithUTF8String:str];
        else
            return [NSString stringWithUTF8String:""];
    }
    
    void QCloudInit(const char * objName)
    {
        //[[PhotoHelper shareInstance] QCloudInit:QCreateString(objName)];
    }//
    
    void StartUploadHeadImage(bool bParam, const char* fileId)
    {
        if (bParam) {
           // // 使用相机
            //[[PhotoHelper shareInstance] useCamera:QCreateString(fileId)];
        }
        else
        {
            // 使用相册
           // [[PhotoHelper shareInstance] usePhoto:QCreateString(fileId)];
        }
    }
    
    void DeleteHeadImage(const char* fileId)
    {
        //[[PhotoHelper shareInstance]  deleteImage:QCreateString(fileId)];
    }
	
	*/
	void IsLowDeviceByFlag(){}
	void Log(){}
#if defined(__cplusplus)
 extern "C"}
#endif