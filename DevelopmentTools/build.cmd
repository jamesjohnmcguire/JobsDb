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

ECHO Publishing jobsdb...
dotnet publish JobsDb.CommandLine/JobsDb.CommandLine.csproj --configuration Release --runtime win-x64 --self-contained -p:PublishReadyToRun=true -p:PublishSingleFile=true -p:IncludeNativeLibrariesForSelfExtract=true -p:DebugType=embedded --output JobsDb.CommandLine/Release/win-x64
IF NOT "%ERRORLEVEL%"=="0" GOTO error

MOVE /Y "JobsDb.CommandLine\Release\win-x64\jobsdb.exe" "%~dp0jobsdb.exe"
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
