# Linux image for building and validating the fuzzing drop locally.
#
# The fuzzing targets run on Ubuntu and Azure Linux, and the native builds use Azure Linux, so this does too:
#   - the official .NET SDK image on Azure Linux 3.0 (needed for Cake and to publish the harness)
#   - fontconfig, which the Linux native SkiaSharp library needs for the harness smoke run
#
# Usage (see build-local.sh, which also runs it):
#   docker build .

FROM mcr.microsoft.com/dotnet/sdk:10.0.401-azurelinux3.0

ENV DOTNET_CLI_TELEMETRY_OPTOUT=1 \
    DOTNET_NOLOGO=1 \
    PATH=$PATH:/root/.dotnet/tools

RUN tdnf install -y fontconfig git \
    && tdnf clean all

WORKDIR /work
