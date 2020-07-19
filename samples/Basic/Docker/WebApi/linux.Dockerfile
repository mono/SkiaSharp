################################################################################
# Build Environment
################################################################################
FROM mcr.microsoft.com/dotnet/core/sdk:3.1 AS build-env
WORKDIR /app

# set up the contents
RUN mkdir packages
COPY . ./

# build and publish
RUN dotnet publish -c Release -o /app/out


################################################################################
# Run Environment
################################################################################
FROM mcr.microsoft.com/dotnet/core/aspnet:3.1 AS runtime

# install Fontconfig
RUN apt-get update \
    && apt-get install -y --no-install-recommends libfontconfig1 \
    && rm -rf /var/lib/apt/lists/*

# prepare the container for launch
WORKDIR /app
EXPOSE 80

# copy from the build environment
COPY --from=build-env /app/out .

# run
ENTRYPOINT [ "dotnet", "SkiaSharpSample.dll" ]
