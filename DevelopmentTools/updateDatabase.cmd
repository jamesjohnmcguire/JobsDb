@ECHO OFF
SETLOCAL

SET "databasePath=%~1"
IF NOT "%~2"=="" GOTO arguments
IF "%databasePath:~0,1%"=="-" GOTO arguments

PUSHD "%~dp0..\SourceCode"
IF ERRORLEVEL 1 EXIT /B 1

IF "%databasePath%"=="" SET "databasePath=%LOCALAPPDATA%\JobsDb\jobsdb.db"
FOR %%P IN ("%databasePath%") DO SET "databasePath=%%~fP"
IF "%~1"=="" IF NOT EXIST "%LOCALAPPDATA%\JobsDb" MKDIR "%LOCALAPPDATA%\JobsDb"
SET "schemaDirectory=%~dp0..\SourceCode\Database"
IF NOT EXIST "%schemaDirectory%\Upgrades\upgradeOrder.txt" GOTO error

SET "schemaCount="
FOR /F %%C IN ('sqlite3 -bail "%databasePath%" "SELECT COUNT(*) FROM sqlite_schema WHERE name NOT GLOB 'sqlite_*';"') DO SET "schemaCount=%%C"
IF "%schemaCount%"=="" GOTO error
IF "%schemaCount%"=="0" GOTO initialize

FOR /F "usebackq tokens=1,2" %%V IN ("%schemaDirectory%\Upgrades\upgradeOrder.txt") DO (
	CALL :applyUpgrade %%V %%W
	IF ERRORLEVEL 1 GOTO error
)
GOTO staticData

:initialize
sqlite3 -bail -cmd "PRAGMA foreign_keys=ON;" "%databasePath%" < "%schemaDirectory%\schema.sql"
IF NOT "%ERRORLEVEL%"=="0" GOTO error

:staticData
IF NOT EXIST "%schemaDirectory%\staticData.sql" GOTO validate
sqlite3 -bail -cmd "PRAGMA foreign_keys=ON;" "%databasePath%" < "%schemaDirectory%\staticData.sql"
IF NOT "%ERRORLEVEL%"=="0" GOTO error

:validate
SET "integrityResult="
FOR /F "delims=" %%R IN ('sqlite3 -bail "%databasePath%" "PRAGMA quick_check;"') DO SET "integrityResult=%%R"
IF NOT "%integrityResult%"=="ok" GOTO error
SET "foreignKeyErrors="
FOR /F %%C IN ('sqlite3 -bail "%databasePath%" "SELECT COUNT(*) FROM pragma_foreign_key_check;"') DO SET "foreignKeyErrors=%%C"
IF NOT "%foreignKeyErrors%"=="0" GOTO error

ECHO Database updated: "%databasePath%"
POPD
EXIT /B 0

:applyUpgrade
SET "historyTable="
FOR /F %%T IN ('sqlite3 -bail "%databasePath%" "SELECT CASE WHEN COUNT(*) = 1 THEN MAX(name) END FROM sqlite_schema WHERE type = 'table' AND name IN ('MigrationsHistory', '__EFMigrationsHistory');"') DO SET "historyTable=%%T"
IF "%historyTable%"=="" (
	ECHO Expected exactly one migration history table. 1>&2
	EXIT /B 1
)
SET "appliedCount="
FOR /F %%C IN ('sqlite3 -bail "%databasePath%" "SELECT COUNT(*) FROM %historyTable% WHERE MigrationId='%~1';"') DO SET "appliedCount=%%C"
IF "%appliedCount%"=="" EXIT /B 1
IF "%appliedCount%"=="1" EXIT /B 0
ECHO Applying %~1...
sqlite3 -bail -cmd "PRAGMA foreign_keys=ON;" "%databasePath%" < "%schemaDirectory%\Upgrades\%~2"
IF NOT "%ERRORLEVEL%"=="0" EXIT /B 1
EXIT /B 0

:arguments
ECHO Specify a database file path, not EF command options. 1>&2
EXIT /B 2

:error
ECHO Database update failed. 1>&2
POPD
EXIT /B 1
