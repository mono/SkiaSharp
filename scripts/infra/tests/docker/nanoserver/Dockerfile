# escape=`
# Containerized test environment — Windows Nano Server.
#
# This image is the ENVIRONMENT the SkiaSharp console test suite runs inside. It is consumed by the
# bootstrapper's `docker:` feature (scripts/azure-templates-jobs-bootstrapper.yml), which builds it
# with the approved container-image task and then runs the tests inside it via:
#
#   docker run --volume <repo>:C:\work -w C:\work skiasharp cmd /c C:\work\cmd.cmd
#   # where cmd.cmd = "dotnet tool restore" + "dotnet cake --target=tests-container ..."
#
# So this Dockerfile only provisions the toolchain; the repository (including
# output/native/nanoserver/x64/libSkiaSharp.dll from the `native` artifact) is mounted at C:\work at
# run time — there is intentionally no COPY here. The build context is this directory, not the repo.
#
# It uses the Nano Server .NET *SDK* image so the suite can be built AND run in one place. Because
# it is Nano Server, it has ZERO system fonts — which is exactly the environment under test. The
# tests-container cake target overlays output/native/nanoserver/x64/libSkiaSharp.dll (via
# --nativePlatform=nanoserver), so the suite runs against the nanoserver native build, not the
# regular windows one.
#
# REQUIRES a Windows container host (Windows agent with container support). It cannot be built or
# run on Linux/macOS Docker.

FROM mcr.microsoft.com/dotnet/sdk:10.0-nanoserver-ltsc2022

WORKDIR C:\work
