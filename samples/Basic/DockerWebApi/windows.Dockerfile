FROM mcr.microsoft.com/dotnet/sdk:10.0-nanoserver-ltsc2025 AS build
WORKDIR /app

COPY SkiaSharpSample/ .
RUN dotnet publish -c Release -r win-x64 -o /app/out

FROM mcr.microsoft.com/dotnet/aspnet:10.0-nanoserver-ltsc2025
WORKDIR /app

COPY --from=build /app/out .

EXPOSE 8080
ENTRYPOINT ["dotnet", "SkiaSharpSample.dll"]
