#!/usr/bin/env bash

cd -- "$(dirname -- "${BASH_SOURCE[0]}")/../SourceCode" || exit 1

dotnet test --configuration Debug
resultCode=$?

if [ "$resultCode" -ne 0 ]; then
	printf 'Tests failed with exit code %s.\n' "$resultCode" >&2
fi

exit "$resultCode"
