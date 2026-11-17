#!/usr/bin/env bash

cd -- "$(dirname -- "${BASH_SOURCE[0]}")/../SourceCode" || exit 1

printf 'Building library for database migrations...\n'
dotnet build DigitalZenWorks.JobsDb.Library --configuration Debug
resultCode=$?

if [ "$resultCode" -eq 0 ]; then
	printf 'Applying database migrations...\n'
	dotnet ef database update --context JobsDbContext --project DigitalZenWorks.JobsDb.Library --configuration Debug --no-build "$@"
	resultCode=$?
fi

if [ "$resultCode" -ne 0 ]; then
	printf 'Database update failed with exit code %s.\n' "$resultCode" >&2
fi

exit "$resultCode"
