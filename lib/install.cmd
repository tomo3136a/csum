@echo off
pushd %~dp0
set INST=c:\opt\bin
set APP=csum

echo installing...
if exist ..\bin\%APP%.exe (
  if not exist %INST% mkdir %INST% 
  copy ..\bin\%APP%.exe %INST%
)
set LIB=%INST%\..\lib
if exist ..\lib\install_task.cmd (
  if not exist %LIB% mkdir %LIB% 
  copy ..\lib\install_task.cmd %LIB%\install_task_%APP%.cmd
  copy ..\lib\install_task.ps1 %LIB%\install_task_%APP%.ps1
)
popd
pause
