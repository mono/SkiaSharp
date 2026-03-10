$ErrorActionPreference = 'Stop'

if ($IsLinux -or $IsMacOS) {
    df -h
} else {
    Get-Volume
}
