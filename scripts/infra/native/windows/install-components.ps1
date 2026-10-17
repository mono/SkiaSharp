Param(
    [string] $InstallationPath,
    [switch] $BuildTools
)

$ErrorActionPreference = 'Stop'

$config = if ($BuildTools) {
    Join-Path $PSScriptRoot 'build-tools.vsconfig'
} else {
    [IO.Path]::GetFullPath((Join-Path $PSScriptRoot '../../../../source/.vsconfig'))
}
$components = (Get-Content $config -Raw | ConvertFrom-Json).components
$installer = "${env:ProgramFiles(x86)}\Microsoft Visual Studio\Installer\setup.exe"
$vswhere = "${env:ProgramFiles(x86)}\Microsoft Visual Studio\Installer\vswhere.exe"
$selection = @('-all', '-version', '[18.0,19.0)')
if ($BuildTools) { $selection += @('-products', 'Microsoft.VisualStudio.Product.BuildTools') }
if (!$InstallationPath) {
    $InstallationPath = & $vswhere @selection -latest -property installationPath
    if ($LASTEXITCODE -ne 0 -or !$InstallationPath) { throw 'No existing Visual Studio 2026 instance was found.' }
}
$arguments = @('--installPath', "`"$InstallationPath`"", '--config', "`"$config`"", '--quiet', '--norestart')
if (Test-Path $InstallationPath) {
    $existing = & $vswhere @selection -property installationPath
    if ($LASTEXITCODE -ne 0 -or @($existing) -notcontains $InstallationPath) {
        throw "Expected the selected Visual Studio 2026 product at $InstallationPath."
    }
    $arguments = @('modify') + $arguments
} else {
    if (!$BuildTools) { throw "No existing Visual Studio 2026 installation at $InstallationPath." }
    $installer = Join-Path $env:TEMP 'vs_buildtools.exe'
    Invoke-WebRequest 'https://aka.ms/vs/stable/vs_buildtools.exe' -OutFile $installer
    $arguments += '--wait'
}
Write-Host "Installing components from $config into $InstallationPath"
$process = Start-Process -FilePath $installer -ArgumentList $arguments -Wait -PassThru
if ($process.ExitCode -ne 0) {
    throw "Visual Studio component installation exited with code $($process.ExitCode)."
}

$installed = & $vswhere @selection -requires $components -property installationPath
if ($LASTEXITCODE -ne 0 -or @($installed) -notcontains $InstallationPath) {
    throw "Could not verify all configured components in $InstallationPath (vswhere exit code $LASTEXITCODE)."
}
Write-Host 'Verified all configured Visual Studio components.'
$env:VS_INSTALL = $InstallationPath
& (Join-Path $PSScriptRoot 'select-vs.ps1') -InstallationPath $InstallationPath
