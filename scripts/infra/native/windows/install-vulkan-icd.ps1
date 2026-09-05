# Provisions a software Vulkan ICD (SwiftShader) + the Khronos loader so the
# ganesh-vulkan tests execute and golden-compare, mirroring what the Linux leg
# does with Mesa lavapipe.
#
# Downloads two first-party, Apache-2.0 Silk.NET native NuGet packages from the
# dnceng "dotnet-public" mirror by immutable .nupkg URL (a plain HTTPS artifact
# download, NOT a <PackageReference>, so the curated feeds are untouched):
#   Silk.NET.Vulkan.Loader.Native      -> vulkan-1.dll        (Khronos loader)
#   Silk.NET.Vulkan.SwiftShader.Native -> vk_swiftshader.dll + vk_swiftshader_icd.json
#
# Both x64 and x86 are provisioned: the .NET Framework leg runs the suite once
# per bitness, and a 32-bit process cannot load a 64-bit vulkan-1.dll.
#
# Bitness resolution is left to Windows rather than PATH ordering, as the
# official Vulkan Runtime installer does it:
#
#   * the loader goes in System32 (x64) and SysWOW64 (x86). WOW64 redirection
#     points a 32-bit process at SysWOW64, so LoadLibrary("vulkan-1") finds the
#     matching bitness. PATH could not disambiguate two files of one name.
#   * the ICD manifests stay in externals\vulkan-icd\<arch>\ beside their
#     vk_swiftshader.dll, because the manifest's "library_path" is relative.
#   * each manifest is registered in the registry view its loader reads: the
#     native Khronos\Vulkan\Drivers key for x64, WOW6432Node for x86.
#
# Any failure here is fatal, so a broken ICD surfaces loudly instead of silently
# dropping Vulkan coverage. An agent that genuinely cannot run Vulkan must be
# declared with SKIASHARP_TEST_SKIP_GPU (see
# documentation/dev/gpu-test-policy.md), never inferred from a load failure.

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
# Staged alongside the other downloaded native deps (externals/angle, externals/winappsdk),
# not output/ which is reserved for artifacts our own native build produces.
$dest = Join-Path $repoRoot 'externals\vulkan-icd'
$tempDir = Join-Path $dest '_download'
New-Item -ItemType Directory -Force -Path $dest, $tempDir | Out-Null

$downloadFile = Join-Path $PSScriptRoot '..\shared\download-file.ps1'
Add-Type -AssemblyName System.IO.Compression.FileSystem

$packages = @(
    @{ Id = 'silk.net.vulkan.loader.native';     Version = $loaderVersion; Sha256 = $loaderSha },
    @{ Id = 'silk.net.vulkan.swiftshader.native'; Version = $swiftVersion;  Sha256 = $swiftSha  }
)

foreach ($pkg in $packages) {
    $uri = "https://pkgs.dev.azure.com/dnceng/public/_packaging/dotnet-public/nuget/v3/flat2/$($pkg.Id)/$($pkg.Version)/$($pkg.Id).$($pkg.Version).nupkg"
    $nupkg = Join-Path $tempDir "$($pkg.Id).$($pkg.Version).nupkg"

    Write-Host "Downloading $($pkg.Id) $($pkg.Version) ..."
    & $downloadFile -Uri $uri -OutFile $nupkg

    $actualSha = (Get-FileHash -Algorithm SHA256 -Path $nupkg).Hash.ToLowerInvariant()
    if ($actualSha -ne $pkg.Sha256) {
        throw "SHA-256 mismatch for $($pkg.Id) $($pkg.Version): expected $($pkg.Sha256), got $actualSha"
    }

    # Unzip the whole .nupkg, then split the native binaries per architecture.
    $extractDir = Join-Path $tempDir $pkg.Id
    if (Test-Path $extractDir) { Remove-Item $extractDir -Recurse -Force }
    [System.IO.Compression.ZipFile]::ExtractToDirectory($nupkg, $extractDir)

    foreach ($arch in @('x64', 'x86')) {
        $src = Join-Path $extractDir "runtimes\win-$arch\native"
        if (-not (Test-Path $src)) {
            throw "$($pkg.Id) $($pkg.Version) has no runtimes\win-$arch\native payload."
        }
        $archDest = Join-Path $dest $arch
        New-Item -ItemType Directory -Force -Path $archDest | Out-Null
        Copy-Item -Path (Join-Path $src '*') -Destination $archDest -Force
        Write-Host "  staged $arch native binaries from $($pkg.Id)"
    }
}

# Install the loader into the system directories so LoadLibrary("vulkan-1")
# resolves to the right bitness automatically (see the header note). System32 is
# the 64-bit store; SysWOW64 is the 32-bit one that WOW64 redirects to.
$loaderTargets = @(
    @{ Arch = 'x64'; Dir = (Join-Path $env:SystemRoot 'System32') },
    @{ Arch = 'x86'; Dir = (Join-Path $env:SystemRoot 'SysWOW64') }
)
foreach ($t in $loaderTargets) {
    $src = Join-Path $dest "$($t.Arch)\vulkan-1.dll"
    if (-not (Test-Path $src)) { throw "Missing staged loader: $src" }
    if (-not (Test-Path $t.Dir)) { throw "Missing system directory: $($t.Dir)" }
    Copy-Item -Path $src -Destination (Join-Path $t.Dir 'vulkan-1.dll') -Force
    Write-Host "Installed the $($t.Arch) Vulkan loader into $($t.Dir)"
}

# The Khronos loader ignores VK_ICD_FILENAMES / VK_DRIVER_FILES for elevated
# processes, and CI agents run elevated, so the registry is the discovery path
# that applies: a REG_DWORD named with the manifest's absolute path, data 0.
# WOW6432Node is the view a 32-bit loader sees as plain SOFTWARE\Khronos.
$icdTargets = @(
    @{ Arch = 'x64'; Key = 'HKLM:\SOFTWARE\Khronos\Vulkan\Drivers' },
    @{ Arch = 'x86'; Key = 'HKLM:\SOFTWARE\WOW6432Node\Khronos\Vulkan\Drivers' }
)
foreach ($t in $icdTargets) {
    $icdJson = Join-Path $dest "$($t.Arch)\vk_swiftshader_icd.json"
    if (-not (Test-Path $icdJson)) { throw "Missing staged ICD manifest: $icdJson" }
    if (-not (Test-Path $t.Key)) { New-Item -Path $t.Key -Force | Out-Null }
    New-ItemProperty -Path $t.Key -Name $icdJson -PropertyType DWord -Value 0 -Force | Out-Null
    Write-Host "Registered the $($t.Arch) ICD: `"$icdJson`" = dword:0 under $($t.Key)"
}

Write-Host "Software Vulkan ICD provisioned for x64 and x86; ganesh-vulkan cells will execute."
