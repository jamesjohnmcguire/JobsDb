#!/usr/bin/env bash

if [ "$#" -gt 1 ] || [[ "${1:-}" == -* ]]; then
	printf 'Specify a database file path, not EF command options.\n' >&2
	exit 2
fi

cd -- "$(dirname -- "${BASH_SOURCE[0]}")/../SourceCode" || exit 1

databasePath="${1:-${LOCALAPPDATA:-${XDG_DATA_HOME:-$HOME/.local/share}}/JobsDb/jobsdb.db}"
databasePath="${databasePath//\\//}"
schemaDirectory='Database'

if [ -z "${1:-}" ]; then
	mkdir -p -- "$(dirname -- "$databasePath")" || exit $?
fi

if [ ! -f "$schemaDirectory/Upgrades/upgradeOrder.txt" ]; then
	printf 'Missing SQL upgrade order.\n' >&2
	exit 1
fi

schemaCount="$(sqlite3 -bail "$databasePath" "SELECT COUNT(*) FROM sqlite_schema WHERE name NOT GLOB 'sqlite_*';")" || exit $?

if [ "$schemaCount" = '0' ]; then
	sqlite3 -bail -cmd 'PRAGMA foreign_keys=ON;' "$databasePath" < "$schemaDirectory/schema.sql" || exit $?
else
	while read -r version upgradeFile || [ -n "$version" ]; do
		# Git may check out the manifest with CRLF line endings.
		upgradeFile="${upgradeFile%$'\r'}"
		appliedCount="$(sqlite3 -bail "$databasePath" "SELECT COUNT(*) FROM __EFMigrationsHistory WHERE MigrationId='$version';")" || exit $?

		if [ "$appliedCount" = '0' ]; then
			printf 'Applying %s...\n' "$version"
			sqlite3 -bail -cmd 'PRAGMA foreign_keys=ON;' "$databasePath" < "$schemaDirectory/Upgrades/$upgradeFile" || exit $?
		fi
	done < "$schemaDirectory/Upgrades/upgradeOrder.txt"
fi

if [ -f "$schemaDirectory/staticData.sql" ]; then
	sqlite3 -bail -cmd 'PRAGMA foreign_keys=ON;' "$databasePath" < "$schemaDirectory/staticData.sql" || exit $?
fi

integrityResult="$(sqlite3 -bail "$databasePath" 'PRAGMA quick_check;')" || exit $?
foreignKeyErrors="$(sqlite3 -bail "$databasePath" 'SELECT COUNT(*) FROM pragma_foreign_key_check;')" || exit $?

if [ "$integrityResult" != 'ok' ] || [ "$foreignKeyErrors" != '0' ]; then
	printf 'Database integrity or foreign-key check failed.\n' >&2
	exit 1
fi

printf 'Database updated: %s\n' "$databasePath"
