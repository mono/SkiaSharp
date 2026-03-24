################################################################################
# Build Environment
################################################################################
FROM mcr.microsoft.com/dotnet/core/sdk:3.1 AS build-env
WORKDIR /app

# set up the contents
RUN mkdir packages
COPY . .

# build and publish
RUN dotnet publish -c Release -o /app/out


################################################################################
# Run Environment
################################################################################
FROM mcr.microsoft.com/windows/servercore:2004 AS runtime
SHELL ["powershell", "-Command", "$ErrorActionPreference = 'Stop'; $ProgressPreference = 'SilentlyContinue';"]

# install and setup ASP.NET Core Runtime
ENV ASPNETCORE_VERSION 3.1.6
RUN Invoke-WebRequest -OutFile aspnetcore.zip https://dotnetcli.blob.core.windows.net/dotnet/aspnetcore/Runtime/$env:ASPNETCORE_VERSION/aspnetcore-runtime-$env:ASPNETCORE_VERSION-win-x64.zip; \
    Expand-Archive aspnetcore.zip -DestinationPath $env:ProgramFiles\dotnet; \
    Remove-Item -Force aspnetcore.zip
RUN setx /M PATH $($env:PATH + ';' + $env:ProgramFiles + '\dotnet')
ENV ASPNETCORE_URLS=http://+:80 \
    DOTNET_RUNNING_IN_CONTAINERS=true

# prepare the container for launch
WORKDIR /app
EXPOSE 80

# copy from the build environment
COPY --from=build-env /app/out .

# run
ENTRYPOINT [ "dotnet", "SkiaSharpSample.dll" ]
