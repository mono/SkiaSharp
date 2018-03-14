
$ErrorActionPreference = 'Stop'

# Prepare the script itself
. "./build-common.ps1"

# build the native bits
. "./build-externals.ps1" -SkipInit $true -SkipBuild $true

# Build a "linux-y" libSkiaSharp
# it will output to:
#   <root>/externals/skia/out/<name>/<arch>/linSkiaSharp.so.<soname-version>
# where:
#   - <root> is the root of the repository
#   - <name> is the -name
#   - <arch> is the -arch
#   - <soname-version> is the result of $(GetVersion "libSkiaSharp" "soname")

Build-Linux-Arch-SkiaSharp  `
    -arch "x64"  `
    -name "custom"  `
    -gpu $true  `
    -cc "c++"  `
    -cxx "cc"  `
    -ar "ar"  `
    -flags_asm @()  `
    -flags_cc @()  `
    -flags_cxx @()  `
    -flags_ld @("-static-libstdc++")
