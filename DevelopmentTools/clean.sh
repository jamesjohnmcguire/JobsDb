#!/usr/bin/env bash

cd -- "$(dirname -- "${BASH_SOURCE[0]}")/../SourceCode" || exit 1

dotnet clean --configuration Debug
resultCode=$?

if [ "$resultCode" -ne 0 ]; then
	printf 'Clean failed with exit code %s.\n' "$resultCode" >&2
fi

exit "$resultCode"
