# Provision a software Vulkan ICD (SwiftShader) + the Khronos loader on the
# Windows CI agent so the ganesh-vulkan tests execute and golden-compare,
# mirroring what the Linux leg does with Mesa lavapipe.
#
# Downloads two first-party, Apache-2.0 Silk.NET native NuGet packages by their
# immutable api.nuget.org URL (a plain HTTPS artifact download, NOT a NuGet
# <PackageReference>, so the curated feeds are untouched):
#   Silk.NET.Vulkan.Loader.Native      -> vulkan-1.dll        (Khronos loader)
#   Silk.NET.Vulkan.SwiftShader.Native -> vk_swiftshader.dll  (software ICD)
#                                         vk_swiftshader_icd.json
#
# This is a deterministic, required provisioning step: any failure is fatal and
# fails the build, so a broken ICD surfaces loudly instead of silently dropping
# Vulkan coverage. (Missing GPU support is still handled at the test layer in
# tests/VulkanTests/VKTest.cs, but on CI we require the software ICD to be here.)

$ErrorActionPreference = 'Stop'

# Pinned package versions + the SHA-256 of each immutable .nupkg. Update the
# version and its SHA together (this is CI config, bumped manually).
$loaderVersion = '2025.9.12'
$loaderSha     = '33811c05ab0bcba632ad38abba459b0d72d6e58859342e0782d011863dde07d1'
$swiftVersion  = '2025.9.8'
$swiftSha      = 'c65b7eaf5b4bfc3aa7ff8f0055f763a1f8898df8036bb45cc5793c6327876b17'

$repoRoot = if ($env:BUILD_SOURCESDIRECTORY) {
    $env:BUILD_SOURCESDIRECTORY
} else {
    (Resolve-Path (Join-Path $PSScriptRoot '..\..\..\..')).Path
}
$dest = Join-Path $repoRoot 'output\vulkan-icd\win-x64'
$tempDir = Join-Path $repoRoot 'output\vulkan-icd\_download'
New-Item -ItemType Directory -Force -Path $dest, $tempDir | Out-Null

$downloadFile = Join-Path $PSScriptRoot '..\shared\download-file.ps1'
Add-Type -AssemblyName System.IO.Compression.FileSystem

$packages = @(
    @{
        Id      = 'silk.net.vulkan.loader.native'
        Version = $loaderVersion
        Sha256  = $loaderSha
        Entries = @{ 'runtimes/win-x64/native/vulkan-1.dll' = 'vulkan-1.dll' }
    },
    @{
        Id      = 'silk.net.vulkan.swiftshader.native'
        Version = $swiftVersion
        Sha256  = $swiftSha
        Entries = @{
            'runtimes/win-x64/native/vk_swiftshader.dll'      = 'vk_swiftshader.dll'
            'runtimes/win-x64/native/vk_swiftshader_icd.json' = 'vk_swiftshader_icd.json'
        }
    }
)

foreach ($pkg in $packages) {
    $uri = "https://api.nuget.org/v3-flatcontainer/$($pkg.Id)/$($pkg.Version)/$($pkg.Id).$($pkg.Version).nupkg"
    $nupkg = Join-Path $tempDir "$($pkg.Id).$($pkg.Version).nupkg"

    Write-Host "Downloading $($pkg.Id) $($pkg.Version) ..."
    & $downloadFile -Uri $uri -OutFile $nupkg

    $actualSha = (Get-FileHash -Algorithm SHA256 -Path $nupkg).Hash.ToLowerInvariant()
    if ($actualSha -ne $pkg.Sha256) {
        throw "SHA-256 mismatch for $($pkg.Id) $($pkg.Version): expected $($pkg.Sha256), got $actualSha"
    }

    $zip = [System.IO.Compression.ZipFile]::OpenRead($nupkg)
    try {
        foreach ($entryPath in $pkg.Entries.Keys) {
            $entry = $zip.GetEntry($entryPath)
            if (-not $entry) { throw "Package $($pkg.Id) $($pkg.Version) is missing entry '$entryPath'." }
            $target = Join-Path $dest $pkg.Entries[$entryPath]
            [System.IO.Compression.ZipFileExtensions]::ExtractToFile($entry, $target, $true)
            Write-Host "  extracted $($pkg.Entries[$entryPath])"
        }
    } finally {
        $zip.Dispose()
    }
}

$icdJson = Join-Path $dest 'vk_swiftshader_icd.json'

# Register the ICD in the Windows registry. CI agents run ELEVATED, and the
# Khronos loader deliberately ignores VK_ICD_FILENAMES / VK_DRIVER_FILES for
# elevated processes, so the registry (a REG_DWORD named with the manifest's
# absolute path, data 0 = enabled) is the discovery path that actually applies.
$driversKey = 'HKLM:\SOFTWARE\Khronos\Vulkan\Drivers'
if (-not (Test-Path $driversKey)) { New-Item -Path $driversKey -Force | Out-Null }
New-ItemProperty -Path $driversKey -Name $icdJson -PropertyType DWord -Value 0 -Force | Out-Null
Write-Host "Registered ICD: `"$icdJson`" = dword:0 under $driversKey"

# Make vulkan-1.dll discoverable by Silk.NET's LoadLibrary("vulkan-1").
Write-Host "##vso[task.prependpath]$dest"

Write-Host "Software Vulkan ICD provisioned; ganesh-vulkan cells will execute."
