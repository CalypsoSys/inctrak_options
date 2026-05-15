FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build-env
WORKDIR /app

# Copy csproj and restore as distinct layers
COPY *.csproj ./
RUN dotnet restore

# Copy everything else and build
COPY . ./
RUN dotnet publish -c Release -o out

# Build runtime image
FROM mcr.microsoft.com/dotnet/aspnet:10.0
WORKDIR /app
RUN apt-get update \
    && apt-get install -y --no-install-recommends libgomp1 libstdc++6 \
    && rm -rf /var/lib/apt/lists/*
COPY --from=build-env /app/out .
RUN set -eux; \
    find /app/runtimes -path '*/native/*' -type f -name 'libggml-base.so' -execdir ln -sf libggml-base.so libggml-base.so.0 \; ; \
    find /app/runtimes -path '*/native/*' -type f -name 'libggml-cpu.so' -execdir ln -sf libggml-cpu.so libggml-cpu.so.0 \; ; \
    find /app/runtimes -path '*/native/*' -type f -name 'libggml.so' -execdir ln -sf libggml.so libggml.so.0 \; ; \
    find /app/runtimes -path '*/native/*' -type f -name 'libllama.so' -execdir ln -sf libllama.so libllama.so.0 \; ; \
    find /app/runtimes -path '*/native/*' -type f -name 'libmtmd.so' -execdir ln -sf libmtmd.so libmtmd.so.0 \;
ENTRYPOINT ["dotnet", "IncTrak.dll"]
