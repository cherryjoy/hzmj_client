/usr/bin/security unlock-keychain -p Iamufo821
/usr/bin/xcodebuild -project /Users/smartlean/Documents/majiang/xcode/Unity-iPhone.xcodeproj PROVISIONING_PROFILE=4bcf1f09-4967-4c6c-820c-487245c8ca13 -sdk iphoneos CODE_SIGN_IDENTITY="iPhone Developer: Shu-Ping Chou (8JMEFPBBGM)" GCC_GENERATE_DEBUGGING_SYMBOLS=YES DEBUG_INFORMATION_FORMAT=dwarf-with-dsym DWARF_DSYM_FILE_SHOULD_ACCOMPANY_PRODUCT=YES DEBUGGING_SYMBOLS=YES PRODUCT_NAME=hzmj -configuration Release 
/usr/bin/xcrun -sdk iphoneos PackageApplication -v /Users/smartlean/Documents/majiang/xcode/build/Release-iphoneos/hzmj.app -o /Users/smartlean/Documents/majiang/xcode/build/hzmj.ipa
var=`date +%Y%m%d%H%M`
mv /Users/smartlean/Documents/majiang/xcode/build/hzmj.ipa /Users/smartlean/Documents/majiang/xcode/build/hzmj_dev_$var.ipa
echo hzmj_dev_$var.ipa
