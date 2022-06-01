$ErrorActionPreference = 'Stop'

if ($IsLinux -or $IsMacOS) {
    Write-Host "Not supported yet."
} else {
    Get-Volume
}
