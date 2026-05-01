$ErrorActionPreference = 'Stop'

Get-ChildItem . -Directory -Recurse -Depth 0 |
    Sort-Object FullName |
    Select-Object `
        FullName,
        @{ Name="Size (MB)"; Expression={ [Math]::Round((Get-ChildItem $_ -Recurse | Measure-Object -Property Length -Sum -EA 0).Sum / 1MB, 2) } } |
    Sort-Object "Size (MB)" -Descending |
    Format-Table -AutoSize
