@ECHO OFF
SETLOCAL

PUSHD "%~dp0..\SourceCode"
IF ERRORLEVEL 1 EXIT /B 1

ECHO Building library for database migrations...
dotnet build DigitalZenWorks.JobsDb.Library --configuration Debug
IF NOT "%ERRORLEVEL%"=="0" GOTO error

ECHO Applying database migrations...
dotnet ef database update --context JobsDbContext --project DigitalZenWorks.JobsDb.Library --configuration Debug --no-build %*
IF NOT "%ERRORLEVEL%"=="0" GOTO error

POPD
EXIT /B 0

:error
SET "resultCode=%ERRORLEVEL%"
ECHO Database update failed with exit code %resultCode%.
POPD
EXIT /B %resultCode%
