@ECHO OFF
SETLOCAL

IF "%~1"=="" (
	ECHO Specify a new database file. 1>&2
	EXIT /B 2
)

IF EXIST "%~f1" (
	ECHO Database destination already exists: "%~f1" 1>&2
	EXIT /B 2
)

sqlite3 -bail "%~f1" < "%~dp0..\SourceCode\Database\schema.sql"
IF NOT "%ERRORLEVEL%"=="0" GOTO error

ECHO Database created: "%~f1"
EXIT /B 0

:error
SET "resultCode=%ERRORLEVEL%"
ECHO Database creation failed with exit code %resultCode%. 1>&2
EXIT /B %resultCode%
