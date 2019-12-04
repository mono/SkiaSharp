
################################################################################
# runtime
# 
# Downloads the files that are needed for running an ASP.NET Core app.
################################################################################

FROM microsoft/dotnet:2.1-aspnetcore-runtime AS runtime
RUN apt-get update \
    && apt-get install -y --no-install-recommends libfontconfig1 \
    && rm -rf /var/lib/apt/lists/*


################################################################################
# build
# 
# Builds the app.
################################################################################

FROM microsoft/dotnet:2.1-sdk AS build
WORKDIR /src
COPY SkiaSharpSample/SkiaSharpSample.csproj SkiaSharpSample/
COPY packages/ packages/
RUN dotnet restore SkiaSharpSample/SkiaSharpSample.csproj -s "/src/packages" -s "https://api.nuget.org/v3/index.json"

COPY . .
WORKDIR /src/SkiaSharpSample
RUN dotnet build SkiaSharpSample.csproj -c Release -o /app


################################################################################
# publish
# 
# Publishes the app.
################################################################################

FROM build AS publish
RUN dotnet publish SkiaSharpSample.csproj -c Release -o /app


################################################################################
# final
# 
# Copies and runs the app.
################################################################################

FROM runtime AS final
WORKDIR /app
EXPOSE 80
COPY --from=publish /app .
ENTRYPOINT dotnet SkiaSharpSample.dll
