# Provision a software Vulkan ICD (SwiftShader) + the Khronos loader on the
# headless Windows CI agent so the ganesh-vulkan visual/GPU tests EXECUTE and
# golden-compare, mirroring what the Linux leg does with Mesa lavapipe.
#
# The binaries come from first-party, Apache-2.0, signed Silk.NET native NuGet
# packages, fetched by *immutable* api.nuget.org flat-container URL + pinned
# SHA-256. This is a plain HTTPS artifact download (the install-7zip.ps1 / LLVM
# convention), NOT a NuGet <PackageReference>, so the repo's curated
# dotnet-public / dotnet-eng feeds are untouched and there is no NU1101 risk.
#
#   Silk.NET.Vulkan.Loader.Native      -> vulkan-1.dll         (Khronos loader)
#   Silk.NET.Vulkan.SwiftShader.Native -> vk_swiftshader.dll   (software ICD)
#                                         vk_swiftshader_icd.json
#
# The loader is discovered by Silk.NET via the OS search path (LoadLibrary
# "vulkan-1"), so we prepend the extract dir to PATH. The ICD is discovered by
# the loader via VK_ICD_FILENAMES / VK_DRIVER_FILES (the Windows loader uses the
# registry/env, not the app dir), so we point those at the extracted manifest.
# The manifest's library_path is ".\vk_swiftshader.dll" (relative to the json),
# so the json and dll are extracted into the same directory.
#
# SKIP-SAFE BY DESIGN: a missing/broken ICD makes the Vulkan cells skip (see
# tests/VulkanTests/VKTest.cs), never fail. Any error here is swallowed with a
# warning and `exit 0`, so this can only ADD coverage, never turn a leg red.

Param(
    [string] $LoaderVersion = '2025.9.12',
    [string] $SwiftShaderVersion = '2025.9.8',
    [ValidateSet('x64', 'x86')]
    [string] $Arch = 'x64'
)

# Pinned SHA-256 of each immutable .nupkg (nuget.org packages never change).
$loaderSha = '33811c05ab0bcba632ad38abba459b0d72d6e58859342e0782d011863dde07d1'
$swiftSha  = 'c65b7eaf5b4bfc3aa7ff8f0055f763a1f8898df8036bb45cc5793c6327876b17'

try {
    $ErrorActionPreference = 'Stop'

    $rid = "win-$Arch"

    $repoRoot = if ($env:BUILD_SOURCESDIRECTORY) {
        $env:BUILD_SOURCESDIRECTORY
    } else {
        (Resolve-Path (Join-Path $PSScriptRoot '..\..\..\..')).Path
    }
    $dest = Join-Path $repoRoot "output\vulkan-icd\$rid"
    $tempDir = Join-Path $repoRoot "output\vulkan-icd\_download"
    New-Item -ItemType Directory -Force -Path $dest | Out-Null
    New-Item -ItemType Directory -Force -Path $tempDir | Out-Null

    $downloadFile = Join-Path $PSScriptRoot '..\shared\download-file.ps1'

    # id (lower-case) => @{ version; sha256; entries = @{ zipEntryPath = destFileName } }
    $packages = @(
        @{
            Id      = 'silk.net.vulkan.loader.native'
            Version = $LoaderVersion
            Sha256  = $loaderSha
            Entries = @{ "runtimes/$rid/native/vulkan-1.dll" = 'vulkan-1.dll' }
        },
        @{
            Id      = 'silk.net.vulkan.swiftshader.native'
            Version = $SwiftShaderVersion
            Sha256  = $swiftSha
            Entries = @{
                "runtimes/$rid/native/vk_swiftshader.dll"      = 'vk_swiftshader.dll'
                "runtimes/$rid/native/vk_swiftshader_icd.json" = 'vk_swiftshader_icd.json'
            }
        }
    )

    Add-Type -AssemblyName System.IO.Compression.FileSystem

    foreach ($pkg in $packages) {
        $id = $pkg.Id
        $ver = $pkg.Version
        $uri = "https://api.nuget.org/v3-flatcontainer/$id/$ver/$id.$ver.nupkg"
        $nupkg = Join-Path $tempDir "$id.$ver.nupkg"

        Write-Host "Downloading $id $ver ..."
        Write-Host "  $uri"
        & $downloadFile -Uri $uri -OutFile $nupkg

        $actualSha = (Get-FileHash -Algorithm SHA256 -Path $nupkg).Hash.ToLowerInvariant()
        if ($actualSha -ne $pkg.Sha256.ToLowerInvariant()) {
            throw "SHA-256 mismatch for $id $ver`n  expected: $($pkg.Sha256)`n  actual:   $actualSha"
        }
        Write-Host "  SHA-256 OK ($actualSha)"

        $zip = [System.IO.Compression.ZipFile]::OpenRead($nupkg)
        try {
            foreach ($entryPath in $pkg.Entries.Keys) {
                $destName = $pkg.Entries[$entryPath]
                $entry = $zip.Entries | Where-Object { $_.FullName -eq $entryPath } | Select-Object -First 1
                if (-not $entry) {
                    throw "Package $id $ver is missing expected entry '$entryPath'."
                }
                $target = Join-Path $dest $destName
                [System.IO.Compression.ZipFileExtensions]::ExtractToFile($entry, $target, $true)
                $len = (Get-Item $target).Length
                Write-Host "  extracted $destName ($len bytes)"
            }
        } finally {
            $zip.Dispose()
        }
    }

    $icdJson = Join-Path $dest 'vk_swiftshader_icd.json'
    $loaderDll = Join-Path $dest 'vulkan-1.dll'
    if (-not (Test-Path $icdJson))   { throw "ICD manifest not found after extraction: $icdJson" }
    if (-not (Test-Path $loaderDll)) { throw "Vulkan loader not found after extraction: $loaderDll" }

    Write-Host ""
    Write-Host "Provisioned software Vulkan ($rid) in: $dest"
    Get-ChildItem $dest | Select-Object Name, Length | Format-Table -AutoSize | Out-String | Write-Host
    Write-Host "ICD manifest contents:"
    Get-Content $icdJson | Write-Host

    # Loader discovery: Silk.NET loads "vulkan-1" by name off the OS search path.
    Write-Host "##vso[task.prependpath]$dest"
    # ICD discovery: point the Vulkan loader at the SwiftShader manifest. Both the
    # legacy (VK_ICD_FILENAMES) and current (VK_DRIVER_FILES) variables are set so
    # older/newer loaders both resolve it, matching the Linux leg.
    Write-Host "##vso[task.setvariable variable=VK_ICD_FILENAMES]$icdJson"
    Write-Host "##vso[task.setvariable variable=VK_DRIVER_FILES]$icdJson"

    Write-Host "Software Vulkan ICD provisioned; ganesh-vulkan cells should now execute."
    exit 0
} catch {
    # Never fail the leg: no ICD simply means the Vulkan cells skip.
    Write-Warning "Could not provision the software Vulkan ICD; the ganesh-vulkan tests will skip on this leg."
    Write-Warning $_.Exception.Message
    exit 0
}
