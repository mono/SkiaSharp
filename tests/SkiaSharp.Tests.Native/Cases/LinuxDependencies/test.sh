#!/usr/bin/env bash
set -euo pipefail
export LC_ALL=C

root=${1:?Pass the absolute output/native directory}
[[ $root == /* && -d $root ]] || { echo "Expected absolute native artifact directory: $root" >&2; exit 1; }
for tool in readelf awk grep; do
    command -v "$tool" >/dev/null || { echo "Missing required tool: $tool" >&2; exit 1; }
done

# These are the glibc, musl and Bionic variants built by the native Linux matrix.
for variant in linux linuxnodeps alpine alpinenodeps bionic; do
    case "$variant" in
        linux*) arches="x86 x64 arm arm64 riscv64 loongarch64" ;;
        alpine*) arches="x64 arm arm64 riscv64 loongarch64" ;;
        bionic) arches="x64 arm64" ;;
    esac
    for arch in $arches; do
        for name in libSkiaSharp libHarfBuzzSharp; do
            library=$root/$variant/$arch/$name.so
            [[ -s $library ]] || { echo "Missing or empty artifact: $library" >&2; exit 1; }
            dynamic=$(readelf --wide --dynamic "$library")
            needed=$(awk '/\(NEEDED\)/ { print }' <<<"$dynamic")
            echo "$variant/$arch/$name dependencies:"
            echo "$needed"
            case "$variant/$name" in
                linux/libSkiaSharp|alpine/libSkiaSharp)
                    grep -iq fontconfig <<<"$needed" ||
                        { echo "Missing required fontconfig dependency: $library" >&2; exit 1; } ;;
                *nodeps/*|bionic/*)
                    if grep -iq fontconfig <<<"$needed"; then
                        echo "Unexpected fontconfig dependency: $library" >&2
                        exit 1
                    fi ;;
            esac
        done
    done
done
