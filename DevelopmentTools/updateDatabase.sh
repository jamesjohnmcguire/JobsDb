#!/usr/bin/env bash

cd -- "$(dirname -- "${BASH_SOURCE[0]}")/../SourceCode" || exit 1

dotnet ef database update --project DigitalZenWorks.JobsDb.Library --configuration Debug "$@"
resultCode=$?

if [ "$resultCode" -ne 0 ]; then
	printf 'Database update failed with exit code %s.\n' "$resultCode" >&2
fi

exit "$resultCode"
