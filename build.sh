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

dotnet build shared.inctrak.com/shared.inctrak.com.csproj
dotnet test shared.inctrak.com.Tests/shared.inctrak.com.Tests.csproj
