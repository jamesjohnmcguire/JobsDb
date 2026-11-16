#!/usr/bin/env bash

cd -- "$(dirname -- "${BASH_SOURCE[0]}")/../SourceCode" || exit 1

dotnet build --configuration Debug --target:Rebuild
resultCode=$?

if [ "$resultCode" -ne 0 ]; then
	printf 'Build failed with exit code %s.\n' "$resultCode" >&2
fi

exit "$resultCode"
