#!/usr/bin/env bash
set -euo pipefail
export LC_ALL=C

root=${1:?Pass the absolute output/native directory}
[[ $root == /* && -d $root ]] || { echo "Expected absolute native artifact directory: $root" >&2; exit 1; }
for tool in readelf awk sort tail; do
    command -v "$tool" >/dev/null || { echo "Missing required tool: $tool" >&2; exit 1; }
done
policy=$root/../../scripts/azure-templates-stages-native-linux.yml
[[ -s $policy ]] || { echo "Missing native build policy: $policy" >&2; exit 1; }

# Read the ceilings from the build matrix rather than maintain a second version policy.
limits=$(awk '
    /^[[:space:]]*- arch: / { arch = $3 }
    /additionalArgs: --verifyGlibcMax=/ {
        split($0, value, "--verifyGlibcMax=")
        print arch, value[2]
        count++
    }
    END { if (count == 0) exit 1 }
' "$policy")
for arch in x86 x64 arm arm64 riscv64 loongarch64; do
    count=$(awk -v arch="$arch" '$1 == arch { count++ } END { print count+0 }' <<<"$limits")
    [[ $count == 1 ]] || { echo "Expected one GLIBC ceiling for $arch in $policy" >&2; exit 1; }
done

while read -r arch maximum; do
    [[ $maximum =~ ^[0-9]+(\.[0-9]+)+$ ]] ||
        { echo "Invalid GLIBC ceiling for $arch: $maximum" >&2; exit 1; }
    for variant in linux linuxnodeps; do
        for name in libSkiaSharp libHarfBuzzSharp; do
            library=$root/$variant/$arch/$name.so
            [[ -s $library ]] || { echo "Missing or empty artifact: $library" >&2; exit 1; }
            output=$(readelf --wide --version-info "$library")
            versions=$(awk '{
                while (match($0, /GLIBC_[[:alnum:]_.]+/)) {
                    print substr($0, RSTART+6, RLENGTH-6)
                    $0 = substr($0, RSTART+RLENGTH)
                }
            }' <<<"$output" | sort -u)
            echo "$variant/$arch/$name: GLIBC ceiling $maximum; requirements: ${versions//$'\n'/ }"
            while read -r version; do
                [[ -n $version ]] || continue
                [[ $version =~ ^[0-9]+(\.[0-9]+)+$ ]] ||
                    { echo "Unrecognized GLIBC requirement: $version in $library" >&2; exit 1; }
                greatest=$(printf '%s\n%s\n' "$version" "$maximum" | sort -V | tail -n 1)
                [[ $greatest == "$maximum" ]] ||
                    { echo "$library requires GLIBC $version above $maximum" >&2; exit 1; }
            done <<<"$versions"
        done
    done
done <<<"$limits"
