@ECHO OFF
SETLOCAL

IF "%~1"=="" (
	ECHO Specify a development database file to rebuild. 1>&2
	EXIT /B 2
)

IF /I "%~nx1"=="jobsdb.db" (
	ECHO jobsdb.db is reserved for the application database. 1>&2
	EXIT /B 2
)

IF EXIST "%~f1\" (
	ECHO The database destination must be a file. 1>&2
	EXIT /B 2
)

IF EXIST "%~f1-wal" GOTO inUse
IF EXIST "%~f1-shm" GOTO inUse
IF EXIST "%~f1-journal" GOTO inUse

SET "temporaryDatabase=%~f1.rebuild"
IF EXIST "%temporaryDatabase%" (
	ECHO Temporary database already exists: "%temporaryDatabase%" 1>&2
	EXIT /B 2
)

sqlite3 -bail "%temporaryDatabase%" < "%~dp0..\SourceCode\Database\schema.sql"
IF NOT "%ERRORLEVEL%"=="0" GOTO error

IF NOT EXIST "%~dp0..\SourceCode\Database\staticData.sql" GOTO replace
sqlite3 -bail "%temporaryDatabase%" < "%~dp0..\SourceCode\Database\staticData.sql"
IF NOT "%ERRORLEVEL%"=="0" GOTO error

:replace
MOVE /Y "%temporaryDatabase%" "%~f1" > NUL
IF NOT "%ERRORLEVEL%"=="0" GOTO error

ECHO Database rebuilt: "%~f1"
EXIT /B 0

:inUse
ECHO Close database connections and resolve SQLite journal files before rebuilding. 1>&2
EXIT /B 2

:error
SET "resultCode=%ERRORLEVEL%"
IF EXIST "%temporaryDatabase%" DEL /Q "%temporaryDatabase%"
ECHO Database rebuild failed with exit code %resultCode%. 1>&2
EXIT /B %resultCode%
