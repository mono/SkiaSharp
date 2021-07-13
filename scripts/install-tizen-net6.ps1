Param(
    [string] $Version = "6.5.100-preview.5.57"
)

$ErrorActionPreference = 'Stop'

$p = "$env:BUILD_SOURCESDIRECTORY\output\install-logs"
New-Item -ItemType Directory -Force -Path $p | Out-Null

$uri = "https://workload-bin.s3.ap-northeast-2.amazonaws.com/windows/Samsung.NET.Workload.Tizen.$Version.msi"

.\scripts\download-file.ps1 -Uri $uri -OutFile tizen.msi

msiexec /i tizen.msi /norestart /quiet /l* $p\tizen-install.log

exit $LASTEXITCODE
