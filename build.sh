#!/usr/bin/env bash

set -euo pipefail

dotnet build goals.inctrak.com/goals.inctrak.com.csproj
dotnet build shared.inctrak.com/shared.inctrak.com.csproj
dotnet build ModelDriver/ModelDriver.csproj
