#!/usr/bin/env bash

set -euo pipefail

for command in dpkg grep glxinfo vulkaninfo Xvfb; do
    if ! command -v "$command" >/dev/null 2>&1; then
        echo "Required software GPU command is missing: $command" >&2
        exit 1
    fi
done

lavapipe_icd=""
while IFS= read -r installed_path; do
    if [[ "$installed_path" =~ /lvp_icd(\.x86_64)?\.json$ ]]; then
        lavapipe_icd="$installed_path"
        break
    fi
done < <(dpkg -L mesa-vulkan-drivers)
if [[ -z "$lavapipe_icd" || ! -r "$lavapipe_icd" ]]; then
    echo "mesa-vulkan-drivers did not install a readable lavapipe ICD manifest." >&2
    exit 1
fi

publish_variable() {
    local name="$1"
    local value="$2"

    export "$name=$value"

    if [[ -n "${GITHUB_ENV:-}" ]]; then
        printf '%s=%s\n' "$name" "$value" >> "$GITHUB_ENV"
    fi

    if [[ -n "${TF_BUILD:-}" ]]; then
        printf '##vso[task.setvariable variable=%s]%s\n' "$name" "$value"
    fi
}

RUNNER_TRACKING_ID="" nohup Xvfb :99 -screen 0 1280x1024x24 > /tmp/xvfb.log 2>&1 &
sleep 3
echo "Started Xvfb on :99"

# Expose the software-GPU environment to the test step. Forcing
# Mesa (GL) and the lavapipe ICD (Vulkan) keeps output deterministic.
#
# softpipe rather than llvmpipe for GL: llvmpipe's LLVM shader JIT
# segfaults compiling the fragment shader Skia generates for a
# runtime blender and takes the whole test host with it, which is
# what made this leg lose all its results (#4590). softpipe is the
# non-JIT gallium rasterizer and runs the same tests green. It
# caps at GL 3.3, which is well past the 3.0 Ganesh needs. See
# #4604 to move back to llvmpipe.
publish_variable DISPLAY :99
publish_variable LIBGL_ALWAYS_SOFTWARE 1
publish_variable GALLIUM_DRIVER softpipe
publish_variable VK_ICD_FILENAMES "$lavapipe_icd"
publish_variable VK_DRIVER_FILES "$lavapipe_icd"

glxinfo_output="$(mktemp)"
vulkaninfo_output="$(mktemp)"
trap 'rm -f "$glxinfo_output" "$vulkaninfo_output"' EXIT

if ! glxinfo -B >"$glxinfo_output" 2>&1; then
    cat "$glxinfo_output"
    cat /tmp/xvfb.log
    echo "Mesa software OpenGL could not initialize on Xvfb." >&2
    exit 1
fi
cat "$glxinfo_output"

if ! grep -Eiq 'renderer string:.*softpipe' "$glxinfo_output"; then
    cat /tmp/xvfb.log
    echo "Mesa software OpenGL did not initialize with softpipe." >&2
    exit 1
fi

if ! vulkaninfo --summary >"$vulkaninfo_output" 2>&1; then
    cat "$vulkaninfo_output"
    echo "vulkaninfo could not initialize the pinned Mesa lavapipe ICD." >&2
    exit 1
fi
cat "$vulkaninfo_output"

if ! grep -Eiq '(deviceName|driverName).*(llvmpipe|lavapipe)' "$vulkaninfo_output"; then
    echo "The pinned Vulkan ICD did not expose Mesa lavapipe/llvmpipe." >&2
    exit 1
fi

echo "Verified Mesa softpipe OpenGL and lavapipe Vulkan on Xvfb."
