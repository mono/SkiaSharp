#!/usr/bin/env bash
set -euo pipefail
export LC_ALL=C
unset LD_PRELOAD LD_AUDIT
cd -- "$(dirname -- "${BASH_SOURCE[0]}")"

fail() { echo "FAIL: $*" >&2; exit 1; }
run() { printf '+ '; printf '%q ' "$@"; echo; "$@"; }

[[ $(uname -s) == Linux ]] || fail "HarfBuzz symbol binding requires Linux glibc."
for tool in getconf readelf grep mktemp rm; do
    command -v "$tool" >/dev/null || fail "Missing required tool: $tool"
done
libc=$(getconf GNU_LIBC_VERSION) || fail "HarfBuzz symbol binding requires glibc."
[[ $libc == glibc\ * ]] || fail "Unsupported host libc: $libc"
case $(uname -m) in
    x86_64) arch=x64; machine='Advanced Micro Devices X86-64' ;;
    aarch64) arch=arm64; machine=AArch64 ;;
    *) fail "Unsupported host architecture: $(uname -m)" ;;
esac
root=${1:?Pass the absolute output/native directory}
[[ $root == /* && -d $root ]] || fail "Native root must be an existing absolute directory: $root"
compiler=${CC-cc}
[[ $compiler =~ ^[[:alnum:]_.+-]+$ ]] || fail "CC must be a compiler name, not a command or path."
command -v "$compiler" >/dev/null || fail "Missing C compiler: $compiler"

# Check both artifacts before executing any native probe.
for variant in linux linuxnodeps; do
    library=$root/$variant/$arch/libHarfBuzzSharp.so
    echo "Checking $variant ($arch, $libc): $library"
    [[ -f $library && -s $library ]] || fail "Missing or empty artifact: $library"
    header=$(readelf -h "$library")
    grep -Eq 'Class: +ELF64$' <<<"$header" || fail "Expected ELF64: $library"
    grep -Eq "Machine: +$machine\$" <<<"$header" || fail "Artifact architecture does not match $arch: $library"
    grep -Eq 'Type: +DYN ' <<<"$header" || fail "Expected shared library: $library"
    dynamic=$(readelf -d "$library")
    grep -Fq 'Shared library: [libc.so.6]' <<<"$dynamic" || fail "Artifact is not glibc-linked: $library"
    if grep -Fq 'musl' <<<"$dynamic"; then fail "Artifact depends on musl: $library"; fi
done

scratch=$(mktemp -d)
trap 'rm -rf -- "$scratch"' EXIT
trap 'exit 130' INT
trap 'exit 143' TERM
run "$compiler" -std=c11 -O0 -Wall -Wextra -Werror probe.c -ldl -o "$scratch/probe"
run "$compiler" -std=c11 -O0 -Wall -Wextra -Werror -shared -fPIC -DINTERPOSER probe.c -o "$scratch/interposer.so"
run "$compiler" -std=c11 -O0 -Wall -Wextra -Werror -shared -fPIC -fsemantic-interposition -DNEGATIVE_CONTROL probe.c -o "$scratch/control.so"

echo "Control: intentionally preemptible fake HarfBuzz alone"
run "$scratch/probe" "$scratch/control.so"
echo "Control: interposer must terminate fake HarfBuzz with exit 86"
status=0
run "$scratch/probe" "$scratch/control.so" "$scratch/interposer.so" >"$scratch/control.log" 2>&1 || status=$?
while IFS= read -r line; do echo "$line"; done <"$scratch/control.log"
[[ $status == 86 ]] || fail "Control returned $status, expected 86"
grep -Fq 'FAIL: bundled HarfBuzz called the global hb_font_set_funcs' "$scratch/control.log" ||
    fail "Control exited 86 without the interposer diagnostic"
echo "PASS: negative control was interposed (exit 86)"

for variant in linux linuxnodeps; do
    library=$root/$variant/$arch/libHarfBuzzSharp.so
    echo "$variant: bundled library alone"
    run "$scratch/probe" "$library"
    echo "$variant: conflicting functions loaded globally first"
    run "$scratch/probe" "$library" "$scratch/interposer.so"
done
