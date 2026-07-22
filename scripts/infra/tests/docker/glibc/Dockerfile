# Containerized test environment — Linux (glibc).
#
# This image is the ENVIRONMENT the SkiaSharp console test suite runs inside. It is consumed by the
# bootstrapper's `docker:` feature (scripts/azure-templates-jobs-bootstrapper.yml), which builds it
# with the approved container-image task and then runs the tests inside it via:
#
#   docker run --volume <repo>:/work skiasharp /bin/bash /work/cmd.sh
#   # where cmd.sh = "dotnet tool restore && dotnet cake --target=tests-container ..."
#
# So this Dockerfile only provisions the toolchain + fonts; the repository (including
# output/native/linux/x64/libSkiaSharp.so from the `native` artifact) is mounted at /work at run
# time — there is intentionally no COPY here. The build context is this directory, not the repo.
#
# The console suite is built with -p:TargetFrameworks=net10.0 (see tests-container.cake), which
# collapses the multi-targeted binding projects to a single TFM, so the Android/iOS workloads are
# NOT needed and are deliberately not installed (keeping the image lean).

FROM mcr.microsoft.com/dotnet/sdk:10.0

# Fonts matching the CI Linux test agents so Skia's fontconfig-backed default font manager can
# resolve DefaultFontFamily ("DejaVu Sans") and the Unicode family ("Symbola"):
#   - fontconfig / libfontconfig1: font discovery used by Skia at runtime
#   - fonts-dejavu-core: "DejaVu Sans"
#   - ttf-ancient-fonts: "Symbola"
RUN apt-get update -qq \
    && DEBIAN_FRONTEND=noninteractive apt-get install -y --no-install-recommends \
        fontconfig \
        libfontconfig1 \
        fonts-dejavu-core \
        ttf-ancient-fonts \
    && fc-cache -f \
    && rm -rf /var/lib/apt/lists/*

WORKDIR /work
