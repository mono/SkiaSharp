Param(
    [Parameter(Mandatory)]
    [ValidatePattern('^android-[0-9]+(?:\.[0-9]+)?;[a-zA-Z0-9_]+$')]
    [string] $Image
)

$ErrorActionPreference = 'Stop'

$abi = switch ([Runtime.InteropServices.RuntimeInformation]::OSArchitecture.ToString()) {
    'Arm64' { 'arm64-v8a' }
    'X64' { 'x86_64' }
    default { throw 'Android system images require an Arm64 or X64 host. Run on a supported host; do not select a different ABI manually.' }
}

& (Join-Path $PSScriptRoot 'install-android-package.ps1') -Package "system-images;$Image;$abi"
exit $LASTEXITCODE
