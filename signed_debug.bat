set DT=%date:~0,4%%date:~5,2%%date:~8,2%%time:~0,2%%time:~3,2%
set DT=%DT: =0%
jarsigner -verbose -keystore cjhzmj.keystore -storepass Iamufo821 -signedjar cjhzmj_signed.apk -digestalg SHA1 -sigalg MD5withRSA cjhzmj_debug.zip cjhzmj.keystore
zipalign -v -f 4 cjhzmj_signed.apk cjhzmj_debug_signed_aligned_%DT%.apk
pause
