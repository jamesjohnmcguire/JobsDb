@ECHO OFF
SETLOCAL

PUSHD "%~dp0..\SourceCode"
IF ERRORLEVEL 1 EXIT /B 1

ECHO Rebuilding solution...
dotnet build --configuration Debug --target:Rebuild
IF ERRORLEVEL 1 GOTO error

ECHO Updating database...
CALL "%~dp0updateDatabase.cmd" --no-build %*
IF ERRORLEVEL 1 GOTO error

ECHO Running all tests...
CALL "%~dp0test.cmd"
IF ERRORLEVEL 1 GOTO error

POPD
EXIT /B 0

:error
SET "resultCode=%ERRORLEVEL%"
ECHO Build workflow failed with exit code %resultCode%.
POPD
EXIT /B %resultCode%
