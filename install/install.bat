@echo off
setlocal
set "batch_dir=%~dp0"
set "new_path=%batch_dir%files"
setx PATH "%PATH%;%new_path%"
echo V# succesfull installed..
echo downloading .net 7.0
start https://download.visualstudio.microsoft.com/download/pr/6f7abf5c-3f6d-43cc-8f3c-700c27d4976b/b7a3b806505c95c7095ca1e8c057e987/dotnet-sdk-7.0.410-win-x64.exe
echo PATH "%PATH%;%new_path%"
pause
