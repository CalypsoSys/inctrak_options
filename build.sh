#!/usr/bin/env bash

set -euo pipefail

dotnet build shared.inctrak.com/shared.inctrak.com.csproj
dotnet build shared.inctrak.com.Tests/shared.inctrak.com.Tests.csproj
