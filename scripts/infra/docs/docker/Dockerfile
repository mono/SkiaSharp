# Local-testing image for ALL of SkiaSharp's documentation-generation paths. It exists
# so a developer can reproduce on any machine exactly what CI produces, without
# installing mono/python/etc. on the host. CI does NOT use this image: the workflows
# install the same dependencies natively on their Linux runners and call the very same
# per-path scripts (scripts/infra/docs/generate-*.sh), so Docker and CI run identical
# code. This image just mirrors that dependency surface for the three paths:
#
#   1. API changelogs   — dotnet cake --target=docs-api-diff-past / docs-api-diff
#   2. Release notes      — python3 scripts/infra/docs/generate-release-notes.py
#   3. mdoc XML reference docs — dotnet cake --target=update-docs
#
# mdoc ships as a .NET Framework executable (scripts/infra/docs/tools/mdoc.*/mdoc.exe).
# The upstream api-docs writer workflow runs the docs step on windows-latest today;
# because docs.cake now invokes mdoc under mono, this image — and equally any plain
# Linux CI runner with mono installed — can run that same step on Linux.
FROM mcr.microsoft.com/dotnet/sdk:10.0

ENV DEBIAN_FRONTEND=noninteractive

# Toolchain:
#   mono-complete — runtime for mdoc.exe (Path 3)
#   python3       — release-notes generator (Path 2)
#   git/curl/unzip/ca-certificates/gnupg — checkout, downloads, package extraction, gh repo key
#
# NOTE on GTK#: the managed GTK#/GIR reference assemblies mdoc needs for the
# SkiaSharp.Views.Gtk* APIs are supplied to mdoc as `--lib` paths by the cake comparer
# (CreateNuGetDiffAsync adds GtkSharp/GdkSharp/GLibSharp/AtkSharp/Mono.GtkSharp and the
# GirCore stack from NuGet). That makes resolution host-independent and means we do NOT
# install a system gtk-sharp2 package (which is unavailable on this Ubuntu base anyway);
# this is the Linux equivalent of the GTK# MSI the Windows workflow installs.
RUN apt-get update \
    && apt-get install -y --no-install-recommends \
        mono-complete \
        python3 \
        git \
        curl \
        unzip \
        ca-certificates \
        gnupg \
    && rm -rf /var/lib/apt/lists/*

# GitHub CLI — REQUIRED by the release-notes generator for PR author resolution. The
# pipeline must NOT silently degrade without it; a GITHUB_TOKEN/GH_TOKEN is passed in
# at run time (see run.sh).
RUN curl -fsSL https://cli.github.com/packages/githubcli-archive-keyring.gpg \
        -o /usr/share/keyrings/githubcli-archive-keyring.gpg \
    && chmod go+r /usr/share/keyrings/githubcli-archive-keyring.gpg \
    && echo "deb [arch=$(dpkg --print-architecture) signed-by=/usr/share/keyrings/githubcli-archive-keyring.gpg] https://cli.github.com/packages stable main" \
        > /etc/apt/sources.list.d/github-cli.list \
    && apt-get update \
    && apt-get install -y --no-install-recommends gh \
    && rm -rf /var/lib/apt/lists/*

# Fail the build early if any required tool is missing.
RUN dotnet --version && mono --version | head -1 && python3 --version && gh --version | head -1

WORKDIR /work

# HOME is overridable; default keeps NuGet/dotnet caches in a writable place when the
# container runs as an arbitrary host UID (run.sh uses -u $(id -u):$(id -g)).
ENV HOME=/tmp
