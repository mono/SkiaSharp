<#
Inspects both libraries for every supported Emscripten version and feature combination. Static archives must contain
only wasm32 WebAssembly objects and define the expected public entry points. Checking every member catches empty
archives or accidentally bundled host objects without requiring a browser or WebAssembly runtime.
#>

param([Parameter(Mandatory)][string] $NativeRoot)

$ErrorActionPreference = 'Stop'
$PSNativeCommandUseErrorActionPreference = $true
Import-Module "$PSScriptRoot/Helpers.psm1"
Assert-NativeRoot $NativeRoot
foreach ($tool in 'llvm-ar', 'llvm-readobj', 'llvm-nm') {
    Assert-Tool $tool
}

# Inspect every member, not just the archive container or its first object. No target code is executed.
foreach ($artifact in Get-WasmArtifacts $NativeRoot | Where-Object Kind -eq WasmArchive) {
    Assert-Artifact $artifact.Path
    $members = @(& llvm-ar t $artifact.Path)
    if ($members.Count -eq 0) {
        throw "Empty WebAssembly archive: $($artifact.Path)"
    }
    $headers = (& llvm-readobj --file-headers $artifact.Path) -join "`n"
    $formats = @([regex]::Matches($headers, '(?m)^Format:\s+(\S+)'))
    $arches = @([regex]::Matches($headers, '(?m)^Arch:\s+(\S+)'))
    if ($formats.Count -ne $members.Count -or $arches.Count -ne $members.Count -or
        @($formats | Where-Object { $_.Groups[1].Value -cne 'WASM' }).Count -gt 0 -or
        @($arches | Where-Object { $_.Groups[1].Value -cne 'wasm32' }).Count -gt 0) {
        throw "Every archive member must be a wasm32 WebAssembly object: $($artifact.Path)"
    }

    $symbols = (& llvm-nm --defined-only --extern-only --format=just-symbols $artifact.Path) -join "`n"
    $exports = if ($artifact.Name -eq 'libSkiaSharp') {
        'sk_version_get_milestone', 'sk_bitmap_new'
    } else {
        'hb_version', 'hb_buffer_create'
    }
    foreach ($export in $exports) {
        if ($symbols -cnotmatch "(?m)^$export`r?$") {
            throw "Missing public WebAssembly symbol '$export': $($artifact.Path)"
        }
    }

    Write-Host "PASS: $($artifact.Path); $($members.Count) wasm32 objects and the expected public API."
}
