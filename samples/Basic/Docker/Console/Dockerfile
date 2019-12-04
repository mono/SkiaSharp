FROM mcr.microsoft.com/dotnet/core/sdk:3.0 AS build-env
WORKDIR /app
RUN mkdir packages
RUN dir
COPY . ./
RUN dotnet restore -s "packages" -s "https://api.nuget.org/v3/index.json"
RUN dotnet publish -c Release -o out

FROM mcr.microsoft.com/dotnet/core/runtime:3.0
WORKDIR /app
COPY --from=build-env /app/out .
ENTRYPOINT [ "dotnet", "SkiaSharpSample.dll" ]
