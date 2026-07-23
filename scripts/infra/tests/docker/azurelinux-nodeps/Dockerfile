# Containerized test environment — Linux Azure Linux, No Dependencies (glibc).
#
# This image proves the NoDependencies native build (skia_use_fontconfig=false) runs on a bare
# glibc image with NOTHING installed beyond the base .NET SDK — no fontconfig, no fonts, no extra
# runtime libraries. That is the whole point of the leg: it fails if libSkiaSharp ever grows an
# undeclared dependency on an optional system package. The font-dependent tests self-skip at
# runtime (there is no system font manager), so the suite still passes.
#
# Consumed by the bootstrapper's `docker:` feature via:
#
#   docker run --volume <repo>:/work skiasharp /bin/sh /work/cmd.sh
#
# The repository (including output/native/linuxnodeps/<arch>/libSkiaSharp.so from the `native`
# artifact) is mounted at /work at run time; tests-container selects it via --nativePlatform=linuxnodeps.
# The base image already provides everything the native library links against (libstdc++, libgcc,
# libc, ICU), so there is deliberately nothing to install here.

FROM mcr.microsoft.com/dotnet/sdk:10.0-azurelinux3.0

WORKDIR /work
