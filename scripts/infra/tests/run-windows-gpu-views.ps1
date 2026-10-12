param(
    [switch] $CaptureNativeDumps,
    [string] $ResultsDirectory = 'output/logs/testlogs/gpu-views-windows',
    [string] $ToolsDirectory
)

$ErrorActionPreference = 'Stop'
. "$PSScriptRoot/windows-gpu-process-ownership.ps1"
$project = 'tests/SkiaSharp.Tests.Devices/SkiaSharp.Tests.Devices.csproj'
$framework = (& dotnet msbuild binding/SkiaSharp/SkiaSharp.csproj -nologo `
    -p:TargetFramework=netstandard2.0 -p:TargetFrameworks=netstandard2.0 `
    -getProperty:WindowsTargetFrameworksCurrent).Trim()
if ($LASTEXITCODE -ne 0 -or $framework -notmatch '^net\d+\.\d+-windows[\d.]+$') {
    throw "Could not resolve the current Windows test framework: $framework"
}
$resultsRoot = [IO.Path]::GetFullPath($ResultsDirectory)
$invocation = [guid]::NewGuid().ToString('N')
$results = Join-Path $resultsRoot "invocation-$invocation"
$repository = (Get-Location).Path
$privateRoot = [IO.Path]::GetFullPath("tools/windows-gpu-private/$invocation")
New-Item -ItemType Directory -Path $results -Force | Out-Null
@{ Invocation = $invocation; StartedUtc = [datetime]::UtcNow } |
    ConvertTo-Json | Set-Content -LiteralPath (Join-Path $results 'invocation.json')
$applicationRoot = [IO.Path]::GetFullPath('tests/SkiaSharp.Tests.Devices/bin')
$previousDiagnostics = $env:SKIASHARP_WINDOWS_GPU_DIAGNOSTICS
$previousNativeDump = $env:SKIASHARP_WINDOWS_GPU_NATIVE_DUMP
$tools = $null
if ($CaptureNativeDumps) {
    if (-not $ToolsDirectory) { $ToolsDirectory = Join-Path $privateRoot 'debug-tools' }
    Assert-GpuPrivatePath $ToolsDirectory $repository
    $tools = & "$PSScriptRoot/get-windows-gpu-debug-tools.ps1" -ToolsDirectory $ToolsDirectory
    Copy-Item -LiteralPath (Join-Path $ToolsDirectory 'tools-validation.json') `
        -Destination (Join-Path $results 'tools-validation.json')
    & "$PSScriptRoot/monitor-windows-gpu-dump.ps1" -ProcDumpPath $tools.ProcDump `
        -DebuggerPath $tools.Debugger -VerifyTool
}

try {
    foreach ($iteration in 1..5) {
        $runResults = Join-Path $results "run-$iteration"
        $since = Get-Date
        $env:SKIASHARP_WINDOWS_GPU_DIAGNOSTICS = Join-Path $runResults 'diagnostics'
        $env:SKIASHARP_WINDOWS_GPU_NATIVE_DUMP = if ($CaptureNativeDumps) { '1' } else { $null }
        $privateDumps = Join-Path $privateRoot "run-$iteration"
        $monitor = $null
        $testFailure = $null
        $captureFailure = $null
        try {
            if ($CaptureNativeDumps) {
                $monitor = Start-Job -FilePath "$PSScriptRoot/monitor-windows-gpu-dump.ps1" `
                    -ArgumentList $tools.ProcDump, $tools.Debugger, $runResults, $applicationRoot, $privateDumps, $since
            }
            dotnet test $project -p:TargetFramework=$framework -p:TargetFrameworks=$framework `
                -p:WindowsPackageType=None -p:Platform=x64 -p:RuntimeIdentifier=win-x64 `
                --filter 'FullyQualifiedName~GpuViewTests|FullyQualifiedName~GaneshViewHandlerDoesNotLeak' `
                --logger trx --results-directory $runResults
            $testExitCode = $LASTEXITCODE
            if ($testExitCode -ne 0) {
                throw "Windows GPU view run $iteration failed with exit code $testExitCode."
            }
            [xml] $report = Get-Content -LiteralPath (Join-Path $runResults 'test-results.trx') -Raw
            $counters = $report.TestRun.ResultSummary.Counters
            if ($counters.total -ne 8 -or $counters.passed -ne 8 -or $counters.failed -ne 0) {
                throw "Windows GPU view run $iteration did not complete all eight cases."
            }
        } catch {
            $testFailure = $_
        } finally {
            try {
                if ($monitor) {
                    New-Item -ItemType Directory -Path $env:SKIASHARP_WINDOWS_GPU_DIAGNOSTICS -Force | Out-Null
                    New-Item -ItemType File -Path (Join-Path $env:SKIASHARP_WINDOWS_GPU_DIAGNOSTICS 'stop-native-monitor') -Force | Out-Null
                    $completed = Wait-Job -Job $monitor -Timeout 180
                    if ($completed) { Receive-Job -Job $monitor -ErrorAction Stop }
                    else { throw 'Native monitor exceeded its bounded completion window.' }
                }
            } catch {
                $captureFailure = $_
            } finally {
                $writerStopped = $false
                if ($CaptureNativeDumps) {
                    try {
                        # Receive-Job failures and completed jobs must take the
                        # same fresh ownership-reconciliation path as timeouts.
                        $writerStopped = Complete-GpuNativeOwnership $env:SKIASHARP_WINDOWS_GPU_DIAGNOSTICS `
                            $tools.ProcDump $privateDumps $repository
                    } catch {
                        if (-not $captureFailure) { $captureFailure = $_ }
                        else { Write-Warning "Native owner reconciliation also failed: $_" }
                    }
                    if ($monitor -and $monitor.State -in 'Running', 'NotStarted') {
                        # Cancellation may only now let the monitor enter its
                        # bounded CDB phase. Keep ownership until its final exit.
                        while (-not (Wait-Job -Job $monitor -Timeout 180)) {
                            Write-Warning 'Native monitor still owns analysis/cleanup; waiting for its final transition.'
                        }
                        try { Receive-Job -Job $monitor -ErrorAction Stop }
                        catch {
                            if (-not $captureFailure) { $captureFailure = $_ }
                            else { Write-Warning "Final native monitor failure: $_" }
                        }
                    }
                    try {
                        # Re-read identities AFTER the final job transition,
                        # including analyzers created after initial cancellation.
                        $writerStopped = Complete-GpuNativeOwnership $env:SKIASHARP_WINDOWS_GPU_DIAGNOSTICS `
                            $tools.ProcDump $privateDumps $repository
                    } catch {
                        $writerStopped = $false
                        if (-not $captureFailure) { $captureFailure = $_ }
                        else { Write-Warning "Final native owner reconciliation failed: $_" }
                    }
                    # Do not finish this task (and trigger worker orphan cleanup)
                    # while an acknowledged recovery owner is still attached.
                    $recoveryRecord = Join-Path $env:SKIASHARP_WINDOWS_GPU_DIAGNOSTICS 'native-recovery-owner.json'
                    if (Test-Path -LiteralPath $recoveryRecord) {
                        try {
                            $identity = Get-Content -LiteralPath $recoveryRecord -Raw | ConvertFrom-Json
                            $recovery = Get-Process -Id $identity.Id -ErrorAction SilentlyContinue
                            if ($recovery) {
                                Assert-GpuOwnedProcessIdentity $recovery $identity 0
                                while (-not $recovery.WaitForExit(60000)) {
                                    Write-Warning "Recovery PID $($identity.Id) retains ownership; waiting for verified detachment rather than killing the test host."
                                }
                                $recovery.Dispose()
                            }
                            $writerStopped = Complete-GpuNativeOwnership $env:SKIASHARP_WINDOWS_GPU_DIAGNOSTICS `
                                $tools.ProcDump $privateDumps $repository
                        } catch {
                            if (-not $captureFailure) { $captureFailure = $_ }
                            else { Write-Warning "Surviving owner verification also failed: $_" }
                        }
                    }
                }
                if ($monitor -and $writerStopped -and $monitor.State -notin 'Running', 'NotStarted') {
                    Remove-Job -Job $monitor -ErrorAction Continue
                }
                if ($CaptureNativeDumps -and $writerStopped -and
                    (-not $monitor -or $monitor.State -notin 'Running', 'NotStarted') -and
                    (Test-Path -LiteralPath $privateDumps)) {
                    try {
                        Get-ChildItem -LiteralPath $privateDumps -Filter '*.dmp' -File |
                            Remove-Item -Force -ErrorAction Stop
                    } catch {
                        if (-not $captureFailure) { $captureFailure = $_ }
                        else { Write-Warning "Private dump cleanup failed: $_" }
                    }
                }
            }
            try {
                & "$PSScriptRoot/collect-windows-gpu-crash.ps1" -Since $since -ResultsDirectory $runResults
            } catch {
                Write-Warning "Windows GPU host diagnostic capture failed: $_"
            }
        }
        # Diagnostic failures never replace the original failed test command.
        if ($testFailure) {
            if ($captureFailure) { Write-Warning "Native capture also failed: $captureFailure" }
            throw $testFailure
        }
        if ($captureFailure) { throw $captureFailure }
    }
} finally {
    $env:SKIASHARP_WINDOWS_GPU_DIAGNOSTICS = $previousDiagnostics
    $env:SKIASHARP_WINDOWS_GPU_NATIVE_DUMP = $previousNativeDump
}
