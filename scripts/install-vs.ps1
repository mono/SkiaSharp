Param(
    [string] $InstallPath = "C:\Program Files (x86)\Microsoft Visual Studio\2019\Enterprise",
    [string] $Config = "scripts\components.vsconfig"
)

$ErrorActionPreference = 'Stop'

$Config = Resolve-Path $Config

$vs_installer = "C:\Program Files (x86)\Microsoft Visual Studio\Installer\vs_installer.exe"

Start-Process -FilePath $vs_installer -ArgumentList "modify --installPath ""$InstallPath"" --config ""$Config"" --quiet --norestart" -Wait -PassThru
