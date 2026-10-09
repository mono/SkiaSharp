#!/usr/bin/env bash
set -euo pipefail

if [[ "$(uname -s)" != Darwin ]]; then
    echo "This Docker prerequisite is for macOS sample agents." >&2
    exit 1
fi

root="${AGENT_TEMPDIRECTORY:?AGENT_TEMPDIRECTORY must identify the owned job directory}/samples-docker"
export COLIMA_HOME="$root/colima"
export COLIMA_CACHE_HOME="$root/cache"
export LIMA_HOME="$root/lima"
export DOCKER_CONFIG="$root/docker"
export DOCKER_HOST="unix://$COLIMA_HOME/samples/docker.sock"

if [[ "${1:-start}" == cleanup ]]; then
    if [[ -f "$COLIMA_HOME/samples/colima.yaml" ]]; then
        colima delete samples --force
    fi
    exit 0
fi
if [[ "${1:-start}" != start ]]; then
    echo "Expected start or cleanup." >&2
    exit 1
fi

# Colima honors COLIMA_HOME only when the directory already exists.
mkdir -p "$COLIMA_HOME" "$COLIMA_CACHE_HOME" "$LIMA_HOME" "$DOCKER_CONFIG"
brew install colima qemu lima-additional-guestagents
if ! command -v docker >/dev/null; then
    brew install docker
fi
# A foreign-architecture QEMU guest does not require nested virtualization.
case "$(uname -m)" in
    arm64) guest_arch=x86_64 ;;
    x86_64) guest_arch=aarch64 ;;
    *) echo "Unsupported macOS agent architecture." >&2; exit 1 ;;
esac
colima start samples --runtime docker --vm-type qemu --arch "$guest_arch" \
    --cpu-type max --cpus 2 --memory 4 --disk 20 --mount none \
    --activate=false --ssh-config=false
docker info

echo "##vso[task.setvariable variable=DOCKER_CONFIG]$DOCKER_CONFIG"
echo "##vso[task.setvariable variable=DOCKER_HOST]$DOCKER_HOST"
