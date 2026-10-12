param(
    [Parameter(Mandatory = $true)] [string] $Directory,
    [Parameter(Mandatory = $true)] [string] $ProcDumpPath,
    [Parameter(Mandatory = $true)] [string] $DumpDirectory,
    [Parameter(Mandatory = $true)] [string] $Repository,
    [Parameter(Mandatory = $true)] [string] $DebuggerPath,
    [Parameter(Mandatory = $true)] [string] $ExpectedApplicationRoot
)

$ErrorActionPreference = 'Stop'
. "$PSScriptRoot/windows-gpu-process-ownership.ps1"
Assert-GpuPrivatePath $DumpDirectory $Repository
[IO.File]::WriteAllText((Join-Path $Directory "native-recovery-ready-$PID"), "$PID")
# This is an independent OS process, not a child in a terminating PowerShell
# job. Each recovery attempt is bounded; ownership survives unsafe detachment.
while ($true) {
    try {
        if (Invoke-GpuOwnerReconciliation $Directory $ProcDumpPath) {
            [IO.File]::WriteAllText((Join-Path $Directory 'native-writer-stopped'), 'Recovery verified detachment and helper exit.')
            if (Test-Path -LiteralPath $DumpDirectory) {
                $ownership = Get-Content -LiteralPath (Join-Path $Directory 'native-owner.json') -Raw | ConvertFrom-Json
                try {
                    Invoke-GpuPrivateDumpAnalysis $Directory $DumpDirectory $DebuggerPath `
                        $ExpectedApplicationRoot $Repository $ownership.HostPid
                } catch {
                    $_ | Out-String | Set-Content -LiteralPath (Join-Path $Directory 'native-recovery-analysis-error.txt')
                } finally {
                    if (Invoke-GpuOwnerReconciliation $Directory $ProcDumpPath) {
                        Get-ChildItem -LiteralPath $DumpDirectory -Filter '*.dmp' -File |
                            Remove-Item -Force -ErrorAction Stop
                    } else {
                        throw 'Recovery analysis cleanup still owns live helpers.'
                    }
                }
            }
            break
        }
    } catch {
        $_ | Out-String | Set-Content -LiteralPath (Join-Path $Directory 'native-recovery-error.txt')
    }
    [IO.File]::WriteAllText((Join-Path $Directory 'native-recovery-heartbeat'), [datetime]::UtcNow.ToString('o'))
    Start-Sleep -Seconds 5
}
