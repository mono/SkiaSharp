#!/usr/bin/env bash
set -ex

# Replays saved fuzz inputs, such as the crash inputs the fuzzing service kept, through a fuzz target in a Linux
# container. Use it to reproduce a crash.
#
# Parameters:
# $1  - the fuzz target: managed-streams | picture-deserialize | harfbuzz-managed-callbacks | skottie-resource-providers
# $2+ - files or directories of inputs, relative to the repository root
#
# Example:
#   ./scripts/infra/security/fuzzing/docker/run-local.sh managed-streams output/crashes
#
# It uses the native libraries that are already in ./output/native, so get them first if you have not:
#   dotnet cake --target=externals-download
#
# The container runs on Docker's default platform, so it matches your machine (arm64 on Apple silicon).
# The repository is mounted into the container, so the build writes its bin/obj folders next to your sources.
# If a native crash kills the harness, the last "Replaying" line names the input that did it.

DIR="$( cd "$( dirname "${BASH_SOURCE[0]}" )" >/dev/null 2>&1 && pwd )"

if [ ! -d "$DIR/../../../../../output/native" ]; then
    echo "No native libraries found in ./output/native. Run 'dotnet cake --target=externals-download' first."
    exit 1
fi

(cd "$DIR" &&
  docker build --tag skiasharp-fuzzing .)

cd "$DIR/../../../../.."

# In a git worktree, .git is a file that points at a directory outside the repository, so mount that
# directory at the same path to keep git working in the container.
GIT_MOUNT=""
if [ -f .git ]; then
    GIT_COMMON="$(cd "$(git rev-parse --git-common-dir)" && pwd)"
    GIT_MOUNT="--volume $GIT_COMMON:$GIT_COMMON"
fi

docker run --rm --name skiasharp-fuzzing \
    --ulimit core=0 \
    --volume $(pwd):/work $GIT_MOUNT \
    --env GIT_CONFIG_COUNT=1 --env GIT_CONFIG_KEY_0=safe.directory --env GIT_CONFIG_VALUE_0='*' \
    skiasharp-fuzzing \
    dotnet run --project tests/SkiaSharp.Fuzzing/SkiaSharp.Fuzzing.csproj \
        --configuration Release --framework net10.0 -p:AllTargetFrameworks=net10.0 \
        -- --replay "$@"
