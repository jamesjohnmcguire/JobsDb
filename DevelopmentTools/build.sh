#!/usr/bin/env bash

cd -- "$(dirname -- "${BASH_SOURCE[0]}")/../SourceCode" || exit 1

printf 'Updating database...\n'
bash ../DevelopmentTools/updateDatabase.sh "$@"
resultCode=$?

if [ "$resultCode" -eq 0 ]; then
	printf 'Rebuilding solution...\n'
	dotnet build --configuration Debug --target:Rebuild
	resultCode=$?
fi

if [ "$resultCode" -eq 0 ]; then
	printf 'Publishing jobsdb...\n'
	dotnet publish JobsDb.CommandLine/JobsDb.CommandLine.csproj --configuration Release --runtime win-x64 --self-contained -p:PublishReadyToRun=true -p:PublishSingleFile=true -p:IncludeNativeLibrariesForSelfExtract=true -p:DebugType=embedded --output JobsDb.CommandLine/Release/win-x64
	resultCode=$?
fi

if [ "$resultCode" -eq 0 ]; then
	mv -f -- 'JobsDb.CommandLine/Release/win-x64/jobsdb.exe' '../DevelopmentTools/jobsdb.exe'
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
