#!/usr/bin/env bash
set -euo pipefail
export LC_ALL=C

fail() { echo "FAIL: $*" >&2; exit 1; }
for tool in llvm-readobj sed grep; do
    command -v "$tool" >/dev/null || fail "Missing required tool: $tool"
done
root=${1:?Pass the absolute output/native directory}
[[ $root == /* && -d $root ]] || fail "Native root must be an existing absolute directory: $root"

# VERIFY_DELAY_LOADED in native/windows/build.cake. NanoServer disables Direct3D.
for arch in x86 x64 ARM64; do
    library=$root/windows/$arch/libSkiaSharp.dll
    echo "Checking Direct3D delay imports: $library"
    [[ -f $library && -s $library ]] || fail "Missing or empty artifact: $library"
    inspection=$(llvm-readobj --coff-imports "$library") ||
        fail "llvm-readobj could not inspect PE imports: $library"
    grep -Eq '^Format: COFF-' <<<"$inspection" || fail "Expected a Windows PE artifact: $library"
    imports=$(sed -n '/^Import {/,/^}/s/^[[:space:]]*Name:[[:space:]]*//p' <<<"$inspection")
    delayed=$(sed -n '/^DelayImport {/,/^}/s/^[[:space:]]*Name:[[:space:]]*//p' <<<"$inspection")
    for dependency in d3d12 D3DCOMPILER; do
        if grep -Fi "$dependency" <<<"$imports"; then
            fail "Hard import must be delay-loaded ($dependency): $library"
        fi
        grep -Fiq "$dependency" <<<"$delayed" ||
            fail "Missing required delay import ($dependency): $library"
    done
done
echo "PASS: Windows Skia Direct3D imports are delay-loaded."
