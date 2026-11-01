FROM mcr.microsoft.com/dotnet/sdk:10.0.403-noble AS build
WORKDIR /app
COPY source .
RUN dotnet publish -c Release -o /app/out -r linux-arm --self-contained false

FROM --platform=linux/arm/v7 mcr.microsoft.com/dotnet/runtime:10.0-noble
RUN apt-get update \
    && apt-get install -y --no-install-recommends libfontconfig1 \
    && rm -rf /var/lib/apt/lists/*
WORKDIR /app
COPY --from=build /app/out .
ENTRYPOINT [ "./NativeLibraryMiniTest" ]
