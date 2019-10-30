Param(
    [string] $InstallerUri = "https://aka.ms/vs/16/release/vs_enterprise.exe",
    [string] $VSConfig = (Join-Path $pwd ".vsconfig")
)

$ErrorActionPreference = 'Stop'

$path = & "C:\Program Files (x86)\Microsoft Visual Studio\Installer\vswhere.exe" -latest -property installationPath
Write-Host "Found Visual Studio at: $path"

$installer = Join-Path $env:TEMP vs_enterprise.exe
Invoke-WebRequest -Uri $InstallerUri -OutFile $installer

Write-Host "Updating the installer..."
cmd /c "`"$installer`" --update --wait --quiet"

Write-Host "Installing components ($VSConfig)..."
cmd /c "`"$installer`" modify --installPath `"$path`" --config $VSConfig --quiet --norestart --includeRecommended --wait"

Write-Host "Installation complete."
