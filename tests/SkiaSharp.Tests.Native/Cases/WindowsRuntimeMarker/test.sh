#!/usr/bin/env bash
set -euo pipefail
export LC_ALL=C

fail() { echo "FAIL: $*" >&2; exit 1; }
command -v grep >/dev/null || fail "Missing required tool: grep"
root=${1:?Pass the absolute output/native directory}
[[ $root == /* && -d $root ]] || fail "Native root must be an existing absolute directory: $root"

# CheckWindowsDependencies in scripts/infra/native/shared/native-shared.cake.
# This is a runtime build marker check, not proof of Spectre mitigation.
# native/winui/build.cake explicitly excludes the managed projection PDB.
symbols=(
    windows/x86/libSkiaSharp.pdb
    windows/x86/libHarfBuzzSharp.pdb
    windows/x64/libSkiaSharp.pdb
    windows/x64/libHarfBuzzSharp.pdb
    windows/ARM64/libSkiaSharp.pdb
    windows/arm64/libHarfBuzzSharp.pdb
    nanoserver/x64/libSkiaSharp.pdb
    nanoserver/x64/libHarfBuzzSharp.pdb
)
for arch in x86 x64 arm64; do
    for name in SkiaSharp.Views.WinUI.Native libEGL libGLESv2; do
        symbols+=("winui/$arch/$name.pdb")
    done
done

for relative in "${symbols[@]}"; do
    pdb=$root/$relative
    echo "Checking Spectre runtime build marker: $pdb"
    [[ -f $pdb && -s $pdb ]] || fail "Missing or empty artifact: $pdb"
    grep -aEi 'objr_spectre|objd_spectre' "$pdb" >/dev/null ||
        fail "Missing Spectre runtime build marker (objr_spectre/objd_spectre): $pdb"
done
echo "PASS: Windows native PDBs contain the Spectre runtime build marker."
