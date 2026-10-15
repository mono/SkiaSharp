#Requires -Version 7.4

$ErrorActionPreference = 'Stop'
$PSNativeCommandUseErrorActionPreference = $true

# The bootstrapper supplies PowerShell and .NET; provision only the native inspection tools here.
if (!$IsWindows -or [Runtime.InteropServices.RuntimeInformation]::ProcessArchitecture -ne 'X64') {
    throw 'Native Windows tests require x64 Windows PowerShell Core 7.4 or newer.'
}

Get-Command dotnet -CommandType Application -ErrorAction Stop | Out-Null
& dotnet --version

$llvmVersion = '19.1.1'
$requiredTools = @(
    'llvm-readelf.exe'
    'llvm-readobj.exe'
    'llvm-nm.exe'
    'llvm-ar.exe'
    'llvm-pdbutil.exe'
    'llvm-objdump.exe'
)

function Test-LlvmToolset {
    param([string] $Directory)

    foreach ($tool in $requiredTools) {
        if (!(Test-Path -LiteralPath (Join-Path $Directory $tool) -PathType Leaf)) {
            return $false
        }
    }

    foreach ($tool in $requiredTools) {
        $version = (& (Join-Path $Directory $tool) --version) -join "`n"
        if ($version -notmatch "\b$([regex]::Escape($llvmVersion))\b") {
            return $false
        }
    }

    return $true
}

# Keep the complete toolset and its source archive in a test-owned cache, not the native build installation.
$cacheBase = if ($env:AGENT_TOOLSDIRECTORY) {
    $env:AGENT_TOOLSDIRECTORY
} else {
    $env:USERPROFILE
}
if (!$cacheBase) {
    throw 'AGENT_TOOLSDIRECTORY or USERPROFILE must be set for the native-test tool cache.'
}

$cacheRoot = Join-Path $cacheBase "SkiaSharp/native-tests/llvm-$llvmVersion-x64"
$archiveName = "clang+llvm-$llvmVersion-x86_64-pc-windows-msvc"
$cachedBin = Join-Path $cacheRoot "$archiveName/bin"
$candidates = @(
    $cachedBin
    if ($env:LLVM_HOME) {
        Join-Path $env:LLVM_HOME 'bin'
    }
    if ($env:ProgramFiles) {
        Join-Path $env:ProgramFiles 'LLVM/bin'
    }
    Get-Command llvm-readelf.exe -CommandType Application -All -ErrorAction SilentlyContinue |
        ForEach-Object { Split-Path $_.Source }
)

$llvmBin = $null
foreach ($candidate in $candidates | Select-Object -Unique) {
    if (Test-LlvmToolset $candidate) {
        $llvmBin = $candidate
        break
    }
}

if (!$llvmBin) {
    # Reuse the repository's pinned 7-Zip bootstrapper if extraction tools are not installed.
    $sevenZip = (Get-Command 7z.exe -CommandType Application -ErrorAction SilentlyContinue |
        Select-Object -First 1).Source
    $installedSevenZip = Join-Path $env:ProgramFiles '7-Zip/7z.exe'
    if (!$sevenZip -and (Test-Path -LiteralPath $installedSevenZip -PathType Leaf)) {
        $sevenZip = $installedSevenZip
    }

    if (!$sevenZip) {
        Push-Location (Join-Path $PSScriptRoot '../../..')
        try {
            & "$PSScriptRoot/../native/windows/install-7zip.ps1"
            if ($LASTEXITCODE -ne 0) {
                throw "7-Zip provisioning failed with exit code $LASTEXITCODE."
            }
        } finally {
            Pop-Location
        }

        $sevenZip = $installedSevenZip
    }

    if (!(Test-Path -LiteralPath $sevenZip -PathType Leaf)) {
        throw "7-Zip was not found after provisioning: $sevenZip"
    }

    # The NSIS toolchain omits llvm-readelf. The official full archive contains all six tools.
    # See llvm/utils/release/build_llvm_release.bat at llvmorg-19.1.1 (LLVM_INSTALL_TOOLCHAIN_ONLY=OFF).
    $archive = Join-Path $cacheRoot "$archiveName.tar.xz"
    $tarArchive = Join-Path $cacheRoot "$archiveName.tar"
    $uri = "https://github.com/llvm/llvm-project/releases/download/llvmorg-$llvmVersion/$archiveName.tar.xz"
    New-Item -ItemType Directory -Force -Path $cacheRoot | Out-Null
    if (!(Test-Path -LiteralPath $archive -PathType Leaf)) {
        Write-Host "Downloading the complete LLVM $llvmVersion Windows x64 toolset: $uri"
        Invoke-WebRequest -Uri $uri -OutFile "$archive.download"
        Move-Item -LiteralPath "$archive.download" -Destination $archive -Force
    }

    try {
        & $sevenZip x $archive "-o$cacheRoot" -y
        & $sevenZip x $tarArchive "-o$cacheRoot" -y
    } finally {
        if (Test-Path -LiteralPath $tarArchive -PathType Leaf) {
            Remove-Item -LiteralPath $tarArchive -Force
        }
    }

    $llvmBin = $cachedBin
    if (!(Test-LlvmToolset $llvmBin)) {
        throw "LLVM $llvmVersion provisioning did not produce all required tools in $llvmBin`: $($requiredTools -join ', ')"
    }
}

# Make the verified toolset available both now and in subsequent Azure Pipelines steps.
$env:LLVM_HOME = Split-Path $llvmBin
$env:PATH = "$llvmBin;$env:PATH"
Write-Host "##vso[task.prependpath]$llvmBin"
Write-Host "##vso[task.setvariable variable=LLVM_HOME;]$env:LLVM_HOME"

foreach ($tool in $requiredTools) {
    Write-Host "Verified: $(Join-Path $llvmBin $tool)"
    & (Join-Path $llvmBin $tool) --version
}
