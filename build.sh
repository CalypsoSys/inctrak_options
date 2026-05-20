#!/usr/bin/env bash

set -euo pipefail

pushd frontend >/dev/null
pnpm build
pnpm test
popd >/dev/null

pushd frontend-vesting >/dev/null
pnpm build
pnpm test
popd >/dev/null

pushd frontend-signup >/dev/null
pnpm build
pnpm test
popd >/dev/null

dotnet build IncTrak.Api/IncTrak.Api.csproj
dotnet test IncTrak.Api.Tests/IncTrak.Api.Tests.csproj
