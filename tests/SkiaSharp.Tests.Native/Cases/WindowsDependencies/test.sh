#!/usr/bin/env bash
set -euo pipefail
export LC_ALL=C

fail() { echo "FAIL: $*" >&2; exit 1; }
for tool in llvm-readobj sed grep; do
    command -v "$tool" >/dev/null || fail "Missing required tool: $tool"
done
root=${1:?Pass the absolute output/native directory}
[[ $root == /* && -d $root ]] || fail "Native root must be an existing absolute directory: $root"

# Merged native artifact paths from native/{windows,nanoserver,winui,winui-angle}/build.cake.
libraries=(
    windows/x86/libSkiaSharp.dll
    windows/x86/libHarfBuzzSharp.dll
    windows/x64/libSkiaSharp.dll
    windows/x64/libHarfBuzzSharp.dll
    windows/ARM64/libSkiaSharp.dll
    windows/arm64/libHarfBuzzSharp.dll
    nanoserver/x64/libSkiaSharp.dll
    nanoserver/x64/libHarfBuzzSharp.dll
    winui/any/SkiaSharp.Views.WinUI.Native.Projection.dll
)
for arch in x86 x64 arm64; do
    for name in SkiaSharp.Views.WinUI.Native libEGL libGLESv2; do
        libraries+=("winui/$arch/$name.dll")
    done
done

for relative in "${libraries[@]}"; do
    library=$root/$relative
    echo "Checking hard imports: $library"
    [[ -f $library && -s $library ]] || fail "Missing or empty artifact: $library"
    inspection=$(llvm-readobj --coff-imports "$library") ||
        fail "llvm-readobj could not inspect PE imports: $library"
    grep -Eq '^Format: COFF-' <<<"$inspection" || fail "Expected a Windows PE artifact: $library"
    imports=$(sed -n '/^Import {/,/^}/s/^[[:space:]]*Name:[[:space:]]*//p' <<<"$inspection")
    # VERIFY_EXCLUDED in scripts/infra/native/windows/windows-shared.cake.
    # Delay-only imports remain allowed, matching CheckWindowsDependencies.
    if grep -Ei 'VCRUNTIME|MSVCP' <<<"$imports"; then
        fail "Forbidden VCRUNTIME/MSVCP hard import: $library"
    fi
done
echo "PASS: Windows hard imports exclude VCRUNTIME/MSVCP."
