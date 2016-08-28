
//游戏端实现OKSDKDelegate接口，OKSDK回调时，会调用OKSDKDelegate

#import "LKSobotPluginManger.h"
#import "oksdk/OKSDK.h"

@interface OKGame : NSObject <OKSDKDelegate>

@end
