#!/usr/bin/env bash

if [ -z "${1:-}" ]; then
	printf 'Specify a new database file.\n' >&2
	exit 2
fi

if [ -e "$1" ] || [ -L "$1" ]; then
	printf 'Database destination already exists: %s\n' "$1" >&2
	exit 2
fi

sqlite3 -bail "$1" < "$(dirname -- "${BASH_SOURCE[0]}")/../SourceCode/Database/schema.sql"
resultCode=$?

if [ "$resultCode" -ne 0 ]; then
	printf 'Database creation failed with exit code %s.\n' "$resultCode" >&2
	exit "$resultCode"
fi

printf 'Database created: %s\n' "$1"
