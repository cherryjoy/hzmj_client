/usr/bin/security unlock-keychain -p Iamufo821
/usr/bin/xcodebuild clean -project /Users/smartlean/Documents/majiang/xcode/Unity-iPhone.xcodeproj -sdk iphoneos -configuration Release
/usr/bin/xcodebuild -project /Users/smartlean/Documents/majiang/xcode/Unity-iPhone.xcodeproj PROVISIONING_PROFILE=7c26c9b8-32e9-4444-8095-55cb20fc9111 -sdk iphoneos CODE_SIGN_IDENTITY="iPhone Distribution: Tianjin 8864 Network Technology Co., Ltd." GCC_GENERATE_DEBUGGING_SYMBOLS=YES DEBUG_INFORMATION_FORMAT=dwarf-with-dsym DWARF_DSYM_FILE_SHOULD_ACCOMPANY_PRODUCT=YES DEBUGGING_SYMBOLS=YES PRODUCT_NAME=hzmj -configuration Release 
/usr/bin/xcrun -sdk iphoneos PackageApplication -v /Users/smartlean/Documents/majiang/xcode/build/Release-iphoneos/hzmj.app -o /Users/smartlean/Documents/majiang/xcode/build/hzmj.ipa
mv /Users/smartlean/Documents/majiang/xcode/build/hzmj.ipa /Users/smartlean/Documents/majiang/xcode/build/hzmj_com.ipa
