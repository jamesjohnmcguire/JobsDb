#!/usr/bin/env bash

cd -- "$(dirname -- "${BASH_SOURCE[0]}")/../SourceCode" || exit 1

printf 'Rebuilding solution...\n'
dotnet build --configuration Debug --target:Rebuild
resultCode=$?

if [ "$resultCode" -eq 0 ]; then
	printf 'Updating database...\n'
	bash ../DevelopmentTools/updateDatabase.sh --no-build "$@"
	resultCode=$?
fi

if [ "$resultCode" -eq 0 ]; then
	printf 'Running all tests...\n'
	bash ../DevelopmentTools/test.sh
	resultCode=$?
fi

if [ "$resultCode" -ne 0 ]; then
	printf 'Build workflow failed with exit code %s.\n' "$resultCode" >&2
fi

exit "$resultCode"
