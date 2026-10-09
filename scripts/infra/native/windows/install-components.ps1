Param(
    [Parameter(Mandatory = $true)]
    [string] $InstallationPath
)

$ErrorActionPreference = 'Stop'

$config = Join-Path $PSScriptRoot 'build-tools.vsconfig'
$components = (Get-Content $config -Raw | ConvertFrom-Json).components
$installer = "${env:ProgramFiles(x86)}\Microsoft Visual Studio\Installer\setup.exe"
$vswhere = "${env:ProgramFiles(x86)}\Microsoft Visual Studio\Installer\vswhere.exe"
$selection = @('-all', '-products', 'Microsoft.VisualStudio.Product.BuildTools', '-version', '[17.0,18.0)')
$arguments = @('--installPath', "`"$InstallationPath`"", '--config', "`"$config`"", '--quiet', '--norestart')
if (Test-Path $InstallationPath) {
    $existing = & $vswhere @selection -property installationPath
    if ($LASTEXITCODE -ne 0 -or @($existing) -notcontains $InstallationPath) {
        throw "Expected Visual Studio 2022 Build Tools at $InstallationPath."
    }
    $arguments = @('modify') + $arguments
} else {
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
