#!/usr/bin/env bash

if [ -z "${1:-}" ]; then
	printf 'Specify a development database file to rebuild.\n' >&2
	exit 2
fi

databasePath="${1//\\//}"
databaseName="${databasePath##*/}"

if [ "${databaseName,,}" = 'jobsdb.db' ] || [ -d "$databasePath" ]; then
	printf 'Specify a development database file; jobsdb.db is reserved for the application.\n' >&2
	exit 2
fi

for suffix in -wal -shm -journal; do
	if [ -e "$databasePath$suffix" ]; then
		printf 'Close database connections and resolve SQLite journal files before rebuilding.\n' >&2
		exit 2
	fi
done

temporaryDatabase="$databasePath.rebuild"

if [ -e "$temporaryDatabase" ] || [ -L "$temporaryDatabase" ]; then
	printf 'Temporary database already exists: %s\n' "$temporaryDatabase" >&2
	exit 2
fi

trap 'rm -f -- "$temporaryDatabase"' EXIT
schemaDirectory="$(dirname -- "${BASH_SOURCE[0]}")/../SourceCode/Database"

sqlite3 -bail "$temporaryDatabase" < "$schemaDirectory/schema.sql" || exit $?

if [ -f "$schemaDirectory/staticData.sql" ]; then
	sqlite3 -bail "$temporaryDatabase" < "$schemaDirectory/staticData.sql" || exit $?
fi

mv -f -- "$temporaryDatabase" "$databasePath" || exit $?
printf 'Database rebuilt: %s\n' "$databasePath"
