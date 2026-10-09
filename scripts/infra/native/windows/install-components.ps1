Param(
    [Parameter(Mandatory = $true)]
    [string] $InstallationPath
)

$ErrorActionPreference = 'Stop'

$config = [IO.Path]::GetFullPath((Join-Path $PSScriptRoot '../../../../source/.vsconfig'))
$components = (Get-Content $config -Raw | ConvertFrom-Json).components
$installer = "${env:ProgramFiles(x86)}\Microsoft Visual Studio\Installer\setup.exe"
$arguments = @('--installPath', "`"$InstallationPath`"", '--config', "`"$config`"", '--quiet', '--norestart')
if (Test-Path $InstallationPath) {
    $arguments = @('modify') + $arguments
} else {
    $installer = Join-Path $env:TEMP 'vs_community.exe'
    Invoke-WebRequest 'https://aka.ms/vs/18/stable/vs_community.exe' -OutFile $installer
    $arguments += '--wait'
}
Write-Host "Installing components from $config into $InstallationPath"
$process = Start-Process -FilePath $installer -ArgumentList $arguments -Wait -PassThru
if ($process.ExitCode -ne 0) {
    throw "Visual Studio component installation exited with code $($process.ExitCode)."
}

$vswhere = "${env:ProgramFiles(x86)}\Microsoft Visual Studio\Installer\vswhere.exe"
$installed = & $vswhere -all -prerelease -products '*' -requires $components -property installationPath
if ($LASTEXITCODE -ne 0 -or @($installed) -notcontains $InstallationPath) {
    throw "Could not verify all configured components in $InstallationPath (vswhere exit code $LASTEXITCODE)."
}
Write-Host 'Verified all configured Visual Studio components.'
