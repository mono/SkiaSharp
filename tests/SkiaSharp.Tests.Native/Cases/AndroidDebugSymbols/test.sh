#!/usr/bin/env bash
set -euo pipefail
export LC_ALL=C
shopt -s nocasematch

fail() { echo "FAIL: $*" >&2; exit 1; }
command -v readelf >/dev/null || fail "Missing required tool: readelf"
version=$(readelf --version)
[[ $version == *"GNU readelf"* ]] || fail "GNU readelf is required."
root=${1:?Pass the absolute output/native directory}
[[ $root == /* && -d $root ]] || fail "Native root must be an existing absolute directory: $root"

build_id() {
    local notes
    notes=$(readelf --notes "$1") || return
    [[ $notes =~ Build[[:space:]]ID:[[:space:]]*([[:xdigit:]]+) ]] ||
        fail "Missing GNU build ID: $1"
    echo "${BASH_REMATCH[1]}"
}

for abi in x86 x86_64 armeabi-v7a arm64-v8a; do
    for name in libSkiaSharp.so libHarfBuzzSharp.so; do
        library=$root/android/$abi/$name
        symbols=$library.dbg
        for file in "$library" "$symbols"; do
            [[ -f $file && -s $file ]] || fail "Missing or empty artifact: $file"
        done
        module_id=$(build_id "$library")
        symbol_id=$(build_id "$symbols")
        [[ $module_id == "$symbol_id" ]] ||
            fail "GNU build IDs differ: $library ($module_id) and $symbols ($symbol_id)"
        debuglink=$(readelf --string-dump=.gnu_debuglink "$library")
        linked_name=
        while IFS= read -r line; do
            if [[ $line =~ ^[[:space:]]*\[[[:space:]]*0\][[:space:]]+(.*)$ ]]; then
                linked_name=${BASH_REMATCH[1]}
                break
            fi
        done <<<"$debuglink"
        # File names are case-sensitive even though build ID hex digits are not.
        shopt -u nocasematch
        [[ $linked_name == "$name.dbg" ]] ||
            fail "Expected .gnu_debuglink '$name.dbg', found '$linked_name': $library"
        shopt -s nocasematch
        echo "PASS: $library links $name.dbg (GNU build ID $module_id)"
    done
done
