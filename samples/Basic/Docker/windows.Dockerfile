# escape=`

################################################################################
# sdk
# 
# Downloads the files that are needed for building an ASP.NET Core app.
################################################################################

FROM microsoft/windowsservercore:1803 AS sdk
SHELL ["powershell", "-Command", "$ErrorActionPreference = 'Stop'; $ProgressPreference = 'SilentlyContinue';"]

# install .NET Core SDK
ENV DOTNET_SDK_VERSION 2.1.403
RUN Invoke-WebRequest -OutFile dotnet.zip https://dotnetcli.blob.core.windows.net/dotnet/Sdk/$Env:DOTNET_SDK_VERSION/dotnet-sdk-$Env:DOTNET_SDK_VERSION-win-x64.zip; `
    $dotnet_sha512 = '52bb1117f170587eaceec1f78cdc41a41d4272154b5535bf61c86bfb75287323cac248434b05eabe4bc7716facabdb0f6475015cbb63f38d91af662618a06720'; `
    if ((Get-FileHash dotnet.zip -Algorithm sha512).Hash -ne $dotnet_sha512) { `
        Write-Host 'CHECKSUM VERIFICATION FAILED!'; `
        exit 1; `
    }; `
    `
    Expand-Archive dotnet.zip -DestinationPath $Env:ProgramFiles\dotnet; `
    Remove-Item -Force dotnet.zip

# set PATH
RUN setx /M PATH $($Env:PATH + ';' + $Env:ProgramFiles + '\dotnet')

# Configure Kestrel web server to bind to port 80 when present
ENV ASPNETCORE_URLS=http://+:80 `
    # Enable detection of running in a container
    DOTNET_RUNNING_IN_CONTAINER=true `
    # Enable correct mode for dotnet watch (only mode supported in a container)
    DOTNET_USE_POLLING_FILE_WATCHER=true `
    # Skip extraction of XML docs - generally not useful within an image/container - helps performance
    NUGET_XMLDOC_MODE=skip

# Trigger first run experience by running arbitrary cmd to populate local package cache
RUN dotnet help


################################################################################
# runtime
# 
# Downloads the files that are needed for running an ASP.NET Core app.
################################################################################

FROM microsoft/windowsservercore:1803 AS runtime
SHELL ["powershell", "-Command", "$ErrorActionPreference = 'Stop'; $ProgressPreference = 'SilentlyContinue';"]

# install ASP.NET Core Runtime
ENV ASPNETCORE_VERSION 2.1.5
RUN Invoke-WebRequest -OutFile aspnetcore.zip https://dotnetcli.blob.core.windows.net/dotnet/aspnetcore/Runtime/$Env:ASPNETCORE_VERSION/aspnetcore-runtime-$Env:ASPNETCORE_VERSION-win-x64.zip; `
    $aspnetcore_sha512 = '98224c8646b7eab234b97f52735905bb0219ea2290490e408ff469459ea82116068854e7b9c5869bccef780b4ceac17477f34f23e06a0a6bedca445a3866d73e'; `
    if ((Get-FileHash aspnetcore.zip -Algorithm sha512).Hash -ne $aspnetcore_sha512) { `
        Write-Host 'CHECKSUM VERIFICATION FAILED!'; `
        exit 1; `
    }; `
    `
    Expand-Archive aspnetcore.zip -DestinationPath $Env:ProgramFiles\dotnet; `
    Remove-Item -Force aspnetcore.zip

RUN setx /M PATH $($Env:PATH + ';' + $Env:ProgramFiles + '\dotnet')

# install Visual C++ Redistributable
RUN Invoke-WebRequest -OutFile vc_redist.x64.exe https://aka.ms/vs/15/release/vc_redist.x64.exe; `
    Start-Process vc_redist.x64.exe -ArgumentList '/install /passive /norestart' -Wait; `
    Remove-Item -Force vc_redist.x64.exe

# Configure web servers to bind to port 80 when present
ENV ASPNETCORE_URLS=http://+:80 `
    # Enable detection of running in a container
    DOTNET_RUNNING_IN_CONTAINERS=true


################################################################################
# build
# 
# Builds the app.
################################################################################

FROM sdk AS build
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
