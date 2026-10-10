Param(
    [string] $InstallationPath,
    [switch] $BuildTools,
    [ValidateSet('2022', '2026')]
    [string] $VisualStudioVersion = '2022'
)

$ErrorActionPreference = 'Stop'

if ($BuildTools -and $VisualStudioVersion -eq '2026') {
    throw 'The Build Tools configuration is currently validated only for Visual Studio 2022.'
}
$config = if ($BuildTools) {
    Join-Path $PSScriptRoot 'build-tools.vsconfig'
} elseif ($VisualStudioVersion -eq '2026') {
    Join-Path $PSScriptRoot 'vs2026.vsconfig'
} else {
    [IO.Path]::GetFullPath((Join-Path $PSScriptRoot '../../../../source/.vsconfig'))
}
$components = (Get-Content $config -Raw | ConvertFrom-Json).components
$installer = "${env:ProgramFiles(x86)}\Microsoft Visual Studio\Installer\setup.exe"
$vswhere = "${env:ProgramFiles(x86)}\Microsoft Visual Studio\Installer\vswhere.exe"
$versionRange = if ($VisualStudioVersion -eq '2026') { '[18.0,19.0)' } else { '[17.0,18.0)' }
$selection = @('-all', '-version', $versionRange)
if ($BuildTools) { $selection += @('-products', 'Microsoft.VisualStudio.Product.BuildTools') }
if (!$InstallationPath) {
    $InstallationPath = & $vswhere @selection -latest -property installationPath
    if ($LASTEXITCODE -ne 0 -or !$InstallationPath) { throw "No existing Visual Studio $VisualStudioVersion instance was found." }
}
$arguments = @('--installPath', "`"$InstallationPath`"", '--config', "`"$config`"", '--quiet', '--norestart')
if (Test-Path $InstallationPath) {
    $existing = & $vswhere @selection -property installationPath
    if ($LASTEXITCODE -ne 0 -or @($existing) -notcontains $InstallationPath) {
        throw "Expected the selected Visual Studio $VisualStudioVersion product at $InstallationPath."
    }
    $arguments = @('modify') + $arguments
} else {
    if (!$BuildTools) { throw "No existing Visual Studio $VisualStudioVersion installation at $InstallationPath." }
    $installer = Join-Path $env:TEMP 'vs_buildtools.exe'
    Invoke-WebRequest 'https://aka.ms/vs/17/release/vs_buildtools.exe' -OutFile $installer
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
