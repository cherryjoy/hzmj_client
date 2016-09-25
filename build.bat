set WORKSPACE=F:\MaJiang\hzmj_client
set DT=%date:~0,4%%date:~5,2%%date:~8,2%%time:~0,2%%time:~3,2%
set DT=%DT: =0%
set LOGFILE=F:\MaJiang\hzmj_client\Logs\Log_%DT%.txt
if not exist LOGFILE type nul>%LOGFILE%

"D:\Program Files\Unity5.3.5p8\Editor\Unity.exe" ^
  -projectPath "%WORKSPACE%"  -logFile "%LOGFILE%" ^
  -executeMethod CommandBuild.OneKeyBuildWin ^
  -batchmode -nographics ^
  -quit