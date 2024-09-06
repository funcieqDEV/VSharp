@echo off
setlocal
set "batch_dir=%~dp0"
set "new_path=%batch_dir%files"
setx PATH "%PATH%;%new_path%"
echo Installed..
pause
