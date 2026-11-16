@ECHO OFF
SETLOCAL

PUSHD "%~dp0..\SourceCode"
IF ERRORLEVEL 1 EXIT /B 1

dotnet build --configuration Debug --target:Rebuild
IF ERRORLEVEL 1 GOTO error

POPD
EXIT /B 0

:error
SET "resultCode=%ERRORLEVEL%"
ECHO Build failed with exit code %resultCode%.
POPD
EXIT /B %resultCode%
