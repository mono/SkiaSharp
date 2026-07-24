# Containerized test environment — Linux Azure Linux (glibc).
#
# This image is the ENVIRONMENT the SkiaSharp console test suite runs inside. It is consumed by the
# bootstrapper's `docker:` feature (scripts/azure-templates-jobs-bootstrapper.yml), which builds it
# with the approved container-image task and then runs the tests inside it via:
#
#   docker run --volume <repo>:/work skiasharp /bin/sh /work/cmd.sh
#   # where cmd.sh = "dotnet tool restore" + "dotnet cake --target=tests-container ..."
#
# So this Dockerfile only provisions fonts; the repository (including output/native/linux/<arch>/
# libSkiaSharp.so from the `native` artifact) is mounted at /work at run time. There is intentionally
# no COPY here. The build context is this directory, not the repo. The tests-container cake target
# selects the glibc build via --nativePlatform=linux.
#
# Azure Linux is glibc, so it shares the desktop Linux leg's fontconfig behavior and font config
# (DefaultFontFamily "DejaVu Sans"). It exercises a non-Ubuntu glibc distro (the desktop leg already
# covers Ubuntu).

FROM mcr.microsoft.com/dotnet/sdk:10.0-azurelinux3.0

# Fonts for Skia's fontconfig-backed default font manager:
#   - fontconfig: font discovery used by Skia at runtime
#   - dejavu-sans-fonts: "DejaVu Sans" (DefaultFontFamily on Linux tests)
# Azure Linux has no emoji/symbol font in any repo, so download Noto Emoji (SIL OFL, monochrome)
# for the unicode tests (covers e.g. U+1F680) and verify it by SHA-256. Pinned to a fixed
# googlefonts/noto-emoji commit so the file is reproducible.
ARG NOTO_EMOJI_URL=https://raw.githubusercontent.com/googlefonts/noto-emoji/9a5261d871451f9b5183c93483cbd68ed916b1e9/fonts/NotoEmoji-Regular.ttf
ARG NOTO_EMOJI_SHA256=415dc6290378574135b64c808dc640c1df7531973290c4970c51fdeb849cb0c5
RUN tdnf install -y fontconfig dejavu-sans-fonts \
    && tdnf clean all \
    && mkdir -p /usr/share/fonts/noto \
    && curl -fSL "$NOTO_EMOJI_URL" -o /usr/share/fonts/noto/NotoEmoji-Regular.ttf \
    && echo "$NOTO_EMOJI_SHA256  /usr/share/fonts/noto/NotoEmoji-Regular.ttf" | sha256sum -c - \
    && fc-cache -f

WORKDIR /work
