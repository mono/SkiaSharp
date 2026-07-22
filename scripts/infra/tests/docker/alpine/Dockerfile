# Containerized test environment — Linux Alpine (musl).
#
# This image is the ENVIRONMENT the SkiaSharp console test suite runs inside. It is consumed by the
# bootstrapper's `docker:` feature (scripts/azure-templates-jobs-bootstrapper.yml), which builds it
# with the approved container-image task and then runs the tests inside it via:
#
#   docker run --volume <repo>:/work skiasharp /bin/bash /work/cmd.sh
#   # where cmd.sh = "dotnet tool restore" + "dotnet cake --target=tests-container ..."
#
# So this Dockerfile only provisions the toolchain + fonts; the repository (including
# output/native/alpine/<arch>/libSkiaSharp.so — the musl build — from the `native` artifact) is
# mounted at /work at run time. There is intentionally no COPY here. The build context is this
# directory, not the repo. The tests-container cake target selects the musl build via
# --nativePlatform=alpine.

FROM mcr.microsoft.com/dotnet/sdk:10.0-alpine

# Runtime + fonts for the musl native library and Skia's fontconfig-backed default font manager:
#   - bash: the bootstrapper `docker:` feature runs the suite via /bin/bash /work/cmd.sh
#   - libstdc++ / libgcc: required by the native libSkiaSharp.so
#   - icu-libs: globalization (avoids invariant-mode limitations in the tests)
#   - fontconfig + font-dejavu: "DejaVu Sans" (DefaultFontFamily on Linux tests)
RUN apk add --no-cache \
        bash \
        libstdc++ \
        libgcc \
        icu-libs \
        fontconfig \
        font-dejavu \
    && fc-cache -f

WORKDIR /work
