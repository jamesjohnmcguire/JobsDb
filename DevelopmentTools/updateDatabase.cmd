@ECHO OFF
SETLOCAL

PUSHD "%~dp0..\SourceCode"
IF ERRORLEVEL 1 EXIT /B 1

dotnet ef database update --project DigitalZenWorks.JobsDb.Library --configuration Debug %*
IF ERRORLEVEL 1 GOTO error

POPD
EXIT /B 0

:error
SET "resultCode=%ERRORLEVEL%"
ECHO Database update failed with exit code %resultCode%.
POPD
EXIT /B %resultCode%
