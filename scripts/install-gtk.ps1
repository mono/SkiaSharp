Param(
    [string] $Version = "2.12.45"
)

$ErrorActionPreference = 'Stop'

$p = "$env:BUILD_SOURCESDIRECTORY\output\logs\install-logs"
New-Item -ItemType Directory -Force -Path $p | Out-Null

$uri = "https://xamarin.azureedge.net/GTKforWindows/Windows/gtk-sharp-$Version.msi"

.\scripts\download-file.ps1 -Uri $uri -OutFile gtk-sharp.msi

msiexec /i gtk-sharp.msi /norestart /quiet /l* $p\gtk-sharp-install.log

exit $LASTEXITCODE