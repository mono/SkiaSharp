FROM mcr.microsoft.com/dotnet/sdk:10.0-nanoserver-ltsc2025 AS build
WORKDIR /app

COPY SkiaSharpSample/ .
RUN dotnet publish -c Release -o /app/out

FROM mcr.microsoft.com/dotnet/runtime:10.0-nanoserver-ltsc2025
WORKDIR /app

COPY --from=build /app/out .

ENTRYPOINT ["dotnet", "SkiaSharpSample.dll"]
