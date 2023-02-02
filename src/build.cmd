@echo off
pushd %~dp0
set OPT=-Sta -NonInteractive -NoProfile -NoLogo
powershell %OPT% -ExecutionPolicy RemoteSigned %~dpn0.ps1 %*
popd
%pause
