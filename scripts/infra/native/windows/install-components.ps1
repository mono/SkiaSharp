Param(
    [string] $InstallationPath = $env:VS_INSTALL,
    [string] $ConfigPath = $env:VS_CONFIG
)

$ErrorActionPreference = 'Stop'

if (!$InstallationPath -or !$ConfigPath) {
    throw 'Select an existing Visual Studio installation before installing components.'
}

$ConfigPath = (Resolve-Path $ConfigPath).Path
$installer = "${env:ProgramFiles(x86)}\Microsoft Visual Studio\Installer\setup.exe"
$arguments = @(
    'modify', '--installPath', "`"$InstallationPath`"",
    '--config', "`"$ConfigPath`"", '--quiet', '--norestart'
)
Write-Host "Installing components from $ConfigPath into $InstallationPath"
$process = Start-Process -FilePath $installer -ArgumentList $arguments -Wait -PassThru
if ($process.ExitCode -ne 0) {
    throw "Visual Studio component installation exited with code $($process.ExitCode)."
}
