@ECHO OFF
SETLOCAL

PUSHD "%~dp0..\SourceCode"
IF ERRORLEVEL 1 EXIT /B 1

ECHO Updating database...
CALL "%~dp0updateDatabase.cmd" %*
IF NOT "%ERRORLEVEL%"=="0" GOTO error

ECHO Rebuilding solution...
dotnet build --configuration Debug --target:Rebuild
IF NOT "%ERRORLEVEL%"=="0" GOTO error

ECHO Running all tests...
CALL "%~dp0test.cmd"
IF NOT "%ERRORLEVEL%"=="0" GOTO error

POPD
EXIT /B 0

:error
SET "resultCode=%ERRORLEVEL%"
ECHO Build workflow failed with exit code %resultCode%.
POPD
EXIT /B %resultCode%
