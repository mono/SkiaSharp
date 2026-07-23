# Containerized test environment — Linux Alpine (musl).
#
# This image is the ENVIRONMENT the SkiaSharp console test suite runs inside. It is consumed by the
# bootstrapper's `docker:` feature (scripts/azure-templates-jobs-bootstrapper.yml), which builds it
# with the approved container-image task and then runs the tests inside it via:
#
#   docker run --volume <repo>:/work skiasharp /bin/sh /work/cmd.sh
#   # where cmd.sh = "dotnet tool restore" + "dotnet cake --target=tests-container ..."
#
# So this Dockerfile only provisions fonts; the repository (including output/native/alpine/<arch>/
# libSkiaSharp.so — the musl build — from the `native` artifact) is mounted at /work at run time.
# There is intentionally no COPY here. The build context is this directory, not the repo. The
# tests-container cake target selects the musl build via --nativePlatform=alpine.

FROM mcr.microsoft.com/dotnet/sdk:10.0-alpine

# Fonts for Skia's fontconfig-backed default font manager (the base .NET SDK image already provides
# the C++/ICU runtime the native library links against, so only fonts are added):
#   - fontconfig: font discovery used by Skia at runtime
#   - font-dejavu: "DejaVu Sans" (DefaultFontFamily on Linux tests)
#   - font-noto-emoji: "Noto Color Emoji" (UnicodeFontFamilies on musl — covers emoji like U+1F680)
RUN apk add --no-cache \
        fontconfig \
        font-dejavu \
        font-noto-emoji \
    && fc-cache -f

WORKDIR /work
