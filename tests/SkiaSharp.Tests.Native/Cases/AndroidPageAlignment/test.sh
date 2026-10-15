#!/usr/bin/env bash
set -euo pipefail
export LC_ALL=C

fail() { echo "FAIL: $*" >&2; exit 1; }
command -v readelf >/dev/null || fail "Missing required tool: readelf"
version=$(readelf --version)
[[ $version == *"GNU readelf"* ]] || fail "GNU readelf is required."
root=${1:?Pass the absolute output/native directory}
[[ $root == /* && -d $root ]] || fail "Native root must be an existing absolute directory: $root"

for abi in x86 x86_64 armeabi-v7a arm64-v8a; do
    for name in libSkiaSharp.so libHarfBuzzSharp.so; do
        library=$root/android/$abi/$name
        [[ -s $library && -f $library ]] || fail "Missing or empty artifact: $library"
        headers=$(readelf --program-headers --wide "$library")
        loads=0
        while read -r -a fields; do
            [[ ${fields[0]-} == LOAD ]] || continue
            [[ ${#fields[@]} -ge 8 ]] || fail "Malformed LOAD header: $library"
            offset=${fields[1]}
            address=${fields[2]}
            alignment=${fields[${#fields[@]}-1]}
            for value in "$offset" "$address" "$alignment"; do
                [[ $value =~ ^0x[[:xdigit:]]+$ ]] || fail "Malformed LOAD value '$value': $library"
            done
            # Larger ELF alignments (for example 64 KB) also support 16 KB pages.
            (( alignment >= 0x4000 && (alignment & (alignment - 1)) == 0 )) ||
                fail "LOAD alignment $alignment is not 16 KB compatible: $library"
            (( offset % alignment == address % alignment )) ||
                fail "LOAD offset $offset and address $address are not congruent: $library"
            loads=$((loads + 1))
        done <<<"$headers"
        (( loads > 0 )) || fail "No LOAD program headers found: $library"
        echo "PASS: $library ($loads LOAD headers, 16 KB compatible)"
    done
done
