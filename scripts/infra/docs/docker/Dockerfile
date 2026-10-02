# Local-testing image for the parent repository's release-notes and API-diff paths.
# It exists so a developer can reproduce on any machine without installing
# python/etc. on the host. CI does NOT use this image: its Linux runners install
# the same dependencies natively and call the same entrypoints. This image mirrors:
#
#   1. API diffs   — dotnet cake --target=docs-api-diff (run by the release-notes Prepare)
#   2. Release notes      — .agents/skills/release-notes/scripts/prepare.sh (+ render.sh)
# API reference generation belongs to mono/SkiaSharp-API-docs and is deliberately
# outside this image.
FROM mcr.microsoft.com/dotnet/sdk:10.0.401

ENV DEBIAN_FRONTEND=noninteractive

# Toolchain:
#   python3       — release-notes generator (Path 2)
#   git/curl/unzip/ca-certificates/gnupg — checkout, downloads, package extraction, gh repo key
#
RUN apt-get update \
    && apt-get install -y --no-install-recommends \
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
RUN dotnet --version && python3 --version && gh --version | head -1

WORKDIR /work

# HOME is overridable; default keeps NuGet/dotnet caches in a writable place when the
# container runs as an arbitrary host UID (run.sh uses -u $(id -u):$(id -g)).
ENV HOME=/tmp
