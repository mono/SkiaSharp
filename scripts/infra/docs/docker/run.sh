#!/usr/bin/env bash
#
# run.sh — run any SkiaSharp documentation-generation step inside the docs Docker image
# (scripts/infra/docs/docker/Dockerfile) for LOCAL testing. The image mirrors the
# dependency surface CI installs natively and runs the very same per-path scripts, so a
# local run reproduces what CI produces. (CI itself does NOT use Docker — its Linux
# runners install dotnet/mono/python directly and call the same scripts.) The repo is
# bind-mounted at /work, so externals/package_cache and output/ persist between runs
# (a "warm" cache). Set COLD=1 to force a fresh, empty package cache for a
# from-scratch download.
#
# The container runs as the host user so every file it writes (regenerated
# API diffs, docs XML, output/) is owned by you, not root.
#
# GitHub auth: GITHUB_TOKEN and GH_TOKEN are forwarded from the host environment.
# The release-notes generator REQUIRES gh for PR author resolution — it must never
# silently degrade — so this script errors out for the release-notes path when no
# token is present (override with ALLOW_NO_TOKEN=1 for the API-only / docs paths).
#
# Usage:
#   run.sh build                       Build (or rebuild) the docker image.
#   run.sh shell                       Open an interactive shell in the image.
#
#   The three documentation-generation paths, each a single canonical script under
#   scripts/infra/docs/ that local runs, CI, and this wrapper all share:
#   run.sh api-diffs [args...]         Path 1: API diffs (generate-api-diffs.sh).
#   run.sh notes [args...]             Path 2: release notes (needs a token).
#   run.sh api-docs [args...]          Path 3: mdoc XML docs (generate-api-docs.sh).
#   run.sh all                         All three paths, in order (needs a token).
#
#   run.sh cake <target> [args...]     Escape hatch: run an arbitrary cake target.
#   run.sh exec <command...>           Run an arbitrary command in the container.
#
# Environment:
#   COLD=1            Mount a fresh empty package cache (cold run).
#   IMAGE=<name>      Override the image tag (default: skiasharp-docs).
#   ALLOW_NO_TOKEN=1  Permit running without a GitHub token (non-notes paths only).
#
set -euo pipefail

IMAGE="${IMAGE:-skiasharp-docs}"
SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
REPO_ROOT="$(cd "$SCRIPT_DIR/../../../.." && pwd)"

die() { echo "run.sh: $*" >&2; exit 1; }

build_image() {
    echo "==> Building $IMAGE from $SCRIPT_DIR/Dockerfile"
    docker build -t "$IMAGE" "$SCRIPT_DIR"
}

# Ensure the image exists; build it on first use.
ensure_image() {
    if ! docker image inspect "$IMAGE" >/dev/null 2>&1; then
        echo "==> Image '$IMAGE' not found; building it first."
        build_image
    fi
}

# Assemble the common `docker run` arguments (mounts, user, env, cold-cache overlay)
# into the global RUN_ARGS array. A global is used instead of a bash 4.3 nameref so the
# script also runs on the bash 3.2 that ships with macOS.
RUN_ARGS=()
docker_run_args() {
    RUN_ARGS=(
        --rm
        -u "$(id -u):$(id -g)"
        -v "$REPO_ROOT:/work"
        -w /work
        -e HOME=/tmp
    )
    # Make git history reachable inside the container for WORKTREE checkouts. In a
    # normal clone, .git is a real directory under /work (already mounted). In a git
    # worktree, /work/.git is a pointer FILE to a gitdir under the parent repo's .git,
    # which lives OUTSIDE the mount — so git would be unusable and the release-notes
    # path (which reads tags/branches/history for PR attribution) would fail. Mount the
    # git common dir at its own host-absolute path so both the /work/.git pointer and
    # the worktree's commondir back-link resolve identically inside the container. This
    # is a no-op for a normal clone (skipped) and for CI (which uses normal clones).
    if [ -f "$REPO_ROOT/.git" ]; then
        local common
        common="$(cd "$REPO_ROOT" && git rev-parse --git-common-dir 2>/dev/null || true)"
        if [ -n "$common" ] && [ -d "$common" ]; then
            common="$(cd "$common" && pwd)"
            RUN_ARGS+=(-v "$common:$common")
        fi
    fi
    # Forward GitHub auth when present (never printed).
    [ -n "${GITHUB_TOKEN:-}" ] && RUN_ARGS+=(-e "GITHUB_TOKEN=${GITHUB_TOKEN}")
    [ -n "${GH_TOKEN:-}" ]     && RUN_ARGS+=(-e "GH_TOKEN=${GH_TOKEN}")
    # Cold cache: bind-mount a fresh, host-owned empty directory over the package
    # cache so the run downloads everything from scratch while the real warm cache
    # under externals/package_cache is left untouched. A host-owned bind mount (not a
    # Docker anonymous volume) is used deliberately: anonymous volumes are created
    # root-owned, but the container runs as the host user, so it could not write into
    # them ("Access to the path ... is denied"). The directory lives under the
    # gitignored output/ tree so it never pollutes the repo.
    if [ "${COLD:-0}" = "1" ]; then
        echo "==> COLD run: using a fresh, empty package cache." >&2
        COLD_CACHE_DIR="$REPO_ROOT/output/cold-cache"
        rm -rf "$COLD_CACHE_DIR"
        mkdir -p "$COLD_CACHE_DIR"
        RUN_ARGS+=(-v "$COLD_CACHE_DIR:/work/externals/package_cache")
    fi
}

require_token_for_notes() {
    if [ -z "${GITHUB_TOKEN:-}" ] && [ -z "${GH_TOKEN:-}" ]; then
        die "release-notes generation needs a GitHub token (set GITHUB_TOKEN or GH_TOKEN). It must not run without gh auth."
    fi
}

cmd="${1:-}"; shift || true
case "$cmd" in
    build)
        build_image
        ;;
    shell)
        ensure_image
        docker_run_args
        exec docker run -it "${RUN_ARGS[@]}" "$IMAGE" bash
        ;;
    cake)
        [ $# -ge 1 ] || die "usage: run.sh cake <target> [args...]"
        ensure_image
        docker_run_args
        target="$1"; shift
        exec docker run "${RUN_ARGS[@]}" "$IMAGE" \
            bash -lc 'dotnet tool restore && dotnet cake --target="$0" "$@"' "$target" "$@"
        ;;
    api-diffs)
        ensure_image
        docker_run_args
        exec docker run "${RUN_ARGS[@]}" "$IMAGE" \
            scripts/infra/docs/generate-api-diffs.sh "$@"
        ;;
    api-docs)
        ensure_image
        docker_run_args
        exec docker run "${RUN_ARGS[@]}" "$IMAGE" \
            scripts/infra/docs/generate-api-docs.sh "$@"
        ;;
    notes)
        [ "${ALLOW_NO_TOKEN:-0}" = "1" ] || require_token_for_notes
        ensure_image
        docker_run_args
        exec docker run "${RUN_ARGS[@]}" "$IMAGE" \
            scripts/infra/docs/generate-release-notes.sh "$@"
        ;;
    all)
        # Full local run: all three paths in one container, in dependency order. Path 1
        # (API diffs) must precede Path 2 (notes) because the release-notes engine
        # consumes the API diff trees + co-release sidecar Path 1 writes; Path 3
        # (api-docs) is independent. Sharing one container (and one COLD cache, when set)
        # means later paths reuse the packages the first path downloaded. Because it runs
        # the notes path, 'all' requires a token (override with ALLOW_NO_TOKEN=1).
        [ "${ALLOW_NO_TOKEN:-0}" = "1" ] || require_token_for_notes
        ensure_image
        docker_run_args
        exec docker run "${RUN_ARGS[@]}" "$IMAGE" bash -euo pipefail -c '
            scripts/infra/docs/generate-api-diffs.sh
            scripts/infra/docs/generate-release-notes.sh
            scripts/infra/docs/generate-api-docs.sh
        '
        ;;
    exec)
        [ $# -ge 1 ] || die "usage: run.sh exec <command...>"
        ensure_image
        docker_run_args
        exec docker run "${RUN_ARGS[@]}" "$IMAGE" "$@"
        ;;
    ""|-h|--help|help)
        sed -n '2,34p' "${BASH_SOURCE[0]}" | sed 's/^# \{0,1\}//'
        ;;
    *)
        die "unknown command '$cmd' (try: build | shell | api-diffs | notes | api-docs | all | cake | exec | help)"
        ;;
esac
