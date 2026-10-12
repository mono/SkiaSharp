param(
    [Parameter(Mandatory = $true)]
    [datetime] $Since,
    [Parameter(Mandatory = $true)]
    [string] $ResultsDirectory
)

$ErrorActionPreference = 'Stop'
$directory = Join-Path $ResultsDirectory 'diagnostics'
New-Item -ItemType Directory -Path $directory -Force | Out-Null
$errors = [System.Collections.Generic.List[string]]::new()

try {
    $events = @(Get-WinEvent -FilterHashtable @{
        LogName = 'Application'
        StartTime = $Since
        Id = 1000, 1001, 1026
    } -ErrorAction Stop | Where-Object {
        $_.Message -match '(?i)SkiaSharp\.Tests\.Devices(?:\.exe)?'
    })
    $events | ForEach-Object { $_.ToXml() } |
        Set-Content -LiteralPath (Join-Path $directory 'application-events.xml.txt')
    $events | Select-Object TimeCreated, ProviderName, Id, Message |
        ConvertTo-Json -Depth 5 |
        Set-Content -LiteralPath (Join-Path $directory 'application-events.json')
    Write-Host "Collected $($events.Count) application events for the owning Windows GPU test host."
} catch {
    $errors.Add("Application event capture: $_")
}

# Read only the existing per-user reports for this executable. Do not enable
# system-wide dumps or change WER, debugger, registration, or installation policy.
foreach ($root in @(
    (Join-Path $env:LOCALAPPDATA 'CrashDumps'),
    (Join-Path $env:LOCALAPPDATA 'Microsoft\Windows\WER\ReportArchive'),
    (Join-Path $env:LOCALAPPDATA 'Microsoft\Windows\WER\ReportQueue')
)) {
    if (-not (Test-Path -LiteralPath $root)) { continue }
    try {
        $matches = @(Get-ChildItem -LiteralPath $root -Force | Where-Object {
            $_.Name -match '(?i)SkiaSharp\.Tests\.Devices' -and $_.LastWriteTime -ge $Since
        })
        foreach ($item in $matches) {
            Copy-Item -LiteralPath $item.FullName -Destination $directory -Recurse -Force
        }
    } catch {
        $errors.Add("Existing report capture from ${root}: $_")
    }
}

$errors | Set-Content -LiteralPath (Join-Path $directory 'capture-errors.txt')
