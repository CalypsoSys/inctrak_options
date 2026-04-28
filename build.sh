#!/usr/bin/env bash

set -euo pipefail

pushd frontend >/dev/null
npm run build
npm run test
popd >/dev/null

pushd frontend-vesting >/dev/null
npm run build
npm run test
popd >/dev/null

dotnet build shared.inctrak.com/shared.inctrak.com.csproj
dotnet test shared.inctrak.com.Tests/shared.inctrak.com.Tests.csproj
