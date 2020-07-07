FROM mcr.microsoft.com/dotnet/core/sdk:3.1 AS build
WORKDIR /app
COPY source .
RUN dotnet publish -c Release -o /app/out -r linux-arm

FROM mcr.microsoft.com/dotnet/core/runtime:3.1-buster-slim-arm32v7
RUN apt-get update \
    && apt-get install -y --no-install-recommends libfontconfig1 \
    && rm -rf /var/lib/apt/lists/*
WORKDIR /app
COPY --from=build /app/out .
ENTRYPOINT [ "./NativeLibraryMiniTest" ]
