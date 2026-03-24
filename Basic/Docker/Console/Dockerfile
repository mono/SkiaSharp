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
FROM mcr.microsoft.com/dotnet/core/runtime:3.1
WORKDIR /app

# copy from the build environment
COPY --from=build-env /app/out .

# run
ENTRYPOINT [ "dotnet", "SkiaSharpSample.dll" ]
