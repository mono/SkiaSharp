$ErrorActionPreference = 'Stop'
if ($env:SKIASHARP_GPU_CAPTURE_FIXTURE_ANALYZER) {
    'Fixture fault: 0xc0000005'
    'Fixture module: test-renderer.dll'
    'Fixture stack: test-renderer!Present'
    if ($env:SKIASHARP_GPU_CAPTURE_FIXTURE_ANALYZER -eq 'error') { exit 23 }
    exit 0
}
$repository = [IO.Path]::GetFullPath((Join-Path $PSScriptRoot '../../../..'))
$testRepository = $repository
. "$repository/scripts/infra/tests/windows-gpu-process-ownership.ps1"
$root = Join-Path $repository 'output/gpu-view-validation/native-capture-tests'
$privateRoot = Join-Path $repository "tools/windows-gpu-private/fixtures-$([guid]::NewGuid().ToString('N'))"
New-Item -ItemType Directory -Path $root -Force | Out-Null
New-Item -ItemType Directory -Path $privateRoot -Force | Out-Null
$parseErrors = $null
$ast = [Management.Automation.Language.Parser]::ParseFile(
    (Join-Path $repository 'scripts/infra/tests/monitor-windows-gpu-dump.ps1'),
    [ref] $null, [ref] $parseErrors)
if ($parseErrors) { throw ($parseErrors | Out-String) }
$main = @($ast.EndBlock.Statements | Where-Object { $_ -is [Management.Automation.Language.TryStatementAst] })[0]
$cleanupText = $main.Finally.Extent.Text
$cleanup = [scriptblock]::Create($cleanupText.Substring(1, $cleanupText.Length - 2))

function Start-Process {
    param($FilePath, $ArgumentList, [switch] $PassThru, $RedirectStandardOutput, $RedirectStandardError)
    if ($env:SKIASHARP_GPU_CAPTURE_FIXTURE_ANALYZER -and $ArgumentList -contains '-z') {
        return (Microsoft.PowerShell.Management\Start-Process -FilePath $FilePath `
            -ArgumentList @('-NoLogo', '-NoProfile', '-File', "`"$testRepository/scripts/infra/tests/tests/Test-WindowsGpuCapture.ps1`"") `
            -PassThru -RedirectStandardOutput $RedirectStandardOutput -RedirectStandardError $RedirectStandardError)
    }
    throw 'Deliberate private dump analysis failure.'
}

try {
    foreach ($unsafe in 'output', 'output/windows-gpu-private-dumps', 'output/windows-gpu-debug-tools', '.') {
        $rejected = $false
        try { Assert-GpuPrivatePath (Join-Path $repository $unsafe) $repository }
        catch { $rejected = $true }
        if (-not $rejected) { throw "Whole-output publication guard accepted $unsafe." }
    }
    Assert-GpuPrivatePath $privateRoot $repository
    $bootstrapper = Get-Content -LiteralPath "$repository/scripts/azure-templates-jobs-bootstrapper.yml" -Raw
    if ($bootstrapper -notmatch "(?s)condition: failed\(\).*?targetPath: 'output'") {
        throw 'Fixture must cover the unconditional whole-output failure publication.'
    }
    'Whole-output failure publication guard rejects private tools/dumps and ancestors inside publication.'
    foreach ($case in 'analysis-failure', 'earlier-attachment-failure') {
        $directory = Join-Path $root "$case/public"
        $privateDumps = Join-Path $privateRoot "$case/private"
        New-Item -ItemType Directory -Path $directory, $privateDumps -Force | Out-Null
        'private stack data' | Set-Content -LiteralPath (Join-Path $privateDumps 'first.dmp')
        'private stack data' | Set-Content -LiteralPath (Join-Path $privateDumps 'second.dmp')
        $monitor = $null
        $target = $null
        $targetId = 42
        $debugger = 'not-invoked'
        $ExpectedApplicationRoot = Join-Path $root 'tests/SkiaSharp.Tests.Devices/bin'
        $applicationRoot = $ExpectedApplicationRoot
        $failure = $null
        if ($case -ne 'analysis-failure') {
            try { throw 'Original attachment error.' } catch { $failure = $_ }
        }
        . $cleanup
        if (@(Get-ChildItem -LiteralPath $privateDumps -Filter '*.dmp' -File).Count -ne 0) {
            throw "Raw dumps survived $case."
        }
        $expected = if ($case -eq 'analysis-failure') { 'Deliberate private dump analysis failure.' } else { 'Original attachment error.' }
        if ($failure.Exception.Message -ne $expected) {
            throw "Primary exception was replaced for ${case}: $($failure.Exception.Message)"
        }
    }
    'Private dump deletion and primary exception preservation checks passed.'
    $repository = $testRepository

    $bindingAst = [Management.Automation.Language.Parser]::ParseFile(
        "$testRepository/scripts/infra/tests/run-windows-gpu-views.ps1", [ref] $null, [ref] $parseErrors)
    if ($parseErrors) { throw ($parseErrors | Out-String) }
    $launch = $bindingAst.FindAll({
        param($node)
        $node -is [Management.Automation.Language.CommandAst] -and $node.GetCommandName() -eq 'Start-Job'
    }, $true)[0].Extent.Text.Replace('$PSScriptRoot', "$testRepository/scripts/infra/tests")
    $tools = [pscustomobject]@{ ProcDump = 'missing-fixture-procdump.exe'; Debugger = 'missing-fixture-cdb.exe' }
    $runResults = $root
    $applicationRoot = $testRepository
    $privateDumps = $privateRoot
    $since = Get-Date
    $bindingJob = & ([scriptblock]::Create($launch))
    try {
        if (-not (Wait-Job $bindingJob -Timeout 20)) { throw 'Monitor binding fixture did not reach its platform/tool guard.' }
        $bindingError = $null
        try { Receive-Job $bindingJob -ErrorAction Stop | Out-Null } catch { $bindingError = $_ }
        if (-not $bindingError -or $bindingError.Exception.Message -match 'positional parameter|parameter set|mandatory parameters') {
            throw "Actual runner monitor launch failed parameter binding: $bindingError"
        }
        $expected = if ($IsWindows) { 'Cannot find path|does not exist' } else { 'requires a Windows worker' }
        if ($bindingError.Exception.Message -notmatch $expected) { throw "Monitor did not reach its expected platform/tool guard: $bindingError" }
    } finally { Remove-Job $bindingJob -Force }
    'Actual runner monitor launch preserves named arguments/script root and reaches platform/tool validation.'

    $executable = (Get-Process -Id $PID).MainModule.FileName
    foreach ($role in 'cancel', 'collector') {
        $writerDirectory = Join-Path $privateRoot "surviving-$role"
        New-Item -ItemType Directory -Path $writerDirectory -Force | Out-Null
        $dump = Join-Path $writerDirectory 'late.dmp'
        $escapedDump = $dump.Replace("'", "''")
        $start = [Diagnostics.ProcessStartInfo]::new($executable)
        $start.UseShellExecute = $false
        foreach ($argument in @('-NoLogo', '-NoProfile', '-Command',
            "while (`$true) { [IO.File]::AppendAllText('$escapedDump', 'private'); Start-Sleep -Milliseconds 100 }")) {
            $start.ArgumentList.Add($argument)
        }
        $process = [Diagnostics.Process]::Start($start)
        $identity = Get-GpuOwnedProcessIdentity $process $executable $role
        try {
            $deadline = [datetime]::UtcNow.AddSeconds(10)
            while (-not (Test-Path -LiteralPath $dump) -and [datetime]::UtcNow -lt $deadline) {
                Start-Sleep -Milliseconds 25
            }
            if (-not (Test-Path -LiteralPath $dump)) { throw 'Surviving-process fixture did not become responsive.' }
            if ($process.WaitForExit(100)) { throw 'Timeout fixture unexpectedly exited.' }
            $wrong = [pscustomobject]@{
                Id = $identity.Id; StartTimeUtcTicks = $identity.StartTimeUtcTicks
                Executable = $identity.Executable + '.wrong'; Role = $role
            }
            $rejected = $false
            try { Stop-GpuOwnedProcess $process $wrong $PID { $true } | Out-Null }
            catch { $rejected = $true }
            if (-not $rejected -or $process.HasExited) { throw 'Recovery accepted a changed process identity.' }
            if ($role -eq 'collector') {
                if (Stop-GpuOwnedProcess $process $identity $PID { $false }) {
                    throw 'An attached collector was force-stopped.'
                }
                if ($process.HasExited -or -not (Test-Path -LiteralPath $dump)) {
                    throw 'Attached collector ownership/private dump was not retained.'
                }
            }
            if (-not (Stop-GpuOwnedProcess $process $identity $PID { $true })) {
                throw 'Verified detached/helper timeout recovery failed.'
            }
            if (-not $process.HasExited) { throw 'Dump deletion preceded writer exit.' }
            Remove-Item -LiteralPath $dump -Force
            Start-Sleep -Milliseconds 250
            if (Test-Path -LiteralPath $dump) { throw 'A surviving writer recreated a dump after cleanup.' }
        } finally {
            if (-not $process.HasExited) {
                Stop-GpuOwnedProcess $process $identity $PID { $true } | Out-Null
            }
            $process.Dispose()
        }
    }
    'Real surviving helper/collector timeout recovery, identity checks and late-writer cleanup passed.'

    $runnerAst = [Management.Automation.Language.Parser]::ParseFile(
        "$repository/scripts/infra/tests/run-windows-gpu-views.ps1", [ref] $null, [ref] $parseErrors)
    if ($parseErrors) { throw ($parseErrors | Out-String) }
    $runnerFinally = $runnerAst.FindAll({
        param($node)
        $node -is [Management.Automation.Language.TryStatementAst] -and
        $node.Finally -and $node.Finally.Extent.Text -match 'collect-windows-gpu-crash'
    }, $true)[0].Finally.Extent.Text
    $runnerFinally = $runnerFinally.Replace('$PSScriptRoot', "$testRepository/scripts/infra/tests")
    $runnerCleanup = [scriptblock]::Create($runnerFinally.Substring(1, $runnerFinally.Length - 2))
    function Test-GpuHostDetached { return $true }
    foreach ($state in 'Completed', 'Failed') {
        $runResults = Join-Path $root "monitor-$state"
        $directory = Join-Path $runResults 'diagnostics'
        $privateDumps = Join-Path $privateRoot "monitor-$state"
        New-Item -ItemType Directory -Path $directory, $privateDumps -Force | Out-Null
        $dump = Join-Path $privateDumps 'late.dmp'
        $start = [Diagnostics.ProcessStartInfo]::new($executable)
        $start.UseShellExecute = $false
        $escapedDump = $dump.Replace("'", "''")
        foreach ($argument in @('-NoLogo', '-NoProfile', '-Command',
            "while (`$true) { [IO.File]::AppendAllText('$escapedDump', 'private'); Start-Sleep -Milliseconds 50 }")) {
            $start.ArgumentList.Add($argument)
        }
        $writer = [Diagnostics.Process]::Start($start)
        $writerIdentity = Get-GpuOwnedProcessIdentity $writer $executable 'collector'
        $helperDump = Join-Path $privateDumps 'helper.dmp'
        $helperStart = [Diagnostics.ProcessStartInfo]::new($executable)
        $helperStart.UseShellExecute = $false
        foreach ($argument in @('-NoLogo', '-NoProfile', '-Command',
            "while (`$true) { [IO.File]::AppendAllText('$($helperDump.Replace("'", "''"))', 'private'); Start-Sleep -Milliseconds 50 }")) {
            $helperStart.ArgumentList.Add($argument)
        }
        $helper = [Diagnostics.Process]::Start($helperStart)
        $helperIdentity = Get-GpuOwnedProcessIdentity $helper $executable 'cancel'
        Save-GpuHelperIdentity $directory $helperIdentity
        $owner = @{
            HostPid = $PID; HostPath = $executable
            HostStartTimeUtcTicks = (Get-Process -Id $PID).StartTime.ToUniversalTime().Ticks
            Collector = $writerIdentity
        }
        $monitor = Microsoft.PowerShell.Core\Start-Job -ScriptBlock {
            param($Directory, $Owner, $State)
            $Owner | ConvertTo-Json -Depth 4 | Set-Content -LiteralPath (Join-Path $Directory 'native-owner.json')
            if ($State -eq 'Failed') { throw 'Deliberate completed monitor failure.' }
        } -ArgumentList $directory, $owner, $state
        $CaptureNativeDumps = $true
        $tools = [pscustomobject]@{ ProcDump = $executable }
        $env:SKIASHARP_WINDOWS_GPU_DIAGNOSTICS = $directory
        $since = Get-Date
        $captureFailure = $null
        try { throw 'Windows GPU view run 1 failed with exit code 7.' } catch { $testFailure = $_ }
        try {
            Wait-Job $monitor -Timeout 20 | Out-Null
            if ($monitor.State -ne $state) { throw "Monitor did not reach $state." }
            if ($writer.HasExited) { throw 'Monitor fixture did not retain a real surviving writer.' }
            $deadline = [datetime]::UtcNow.AddSeconds(10)
            while (-not (Test-Path -LiteralPath $dump) -and [datetime]::UtcNow -lt $deadline) {
                Start-Sleep -Milliseconds 25
            }
            if (-not (Test-Path -LiteralPath $dump)) { throw 'Reconciliation writer never became responsive.' }
            $wrongOwner = $owner.Clone()
            $wrongOwner.Collector = [pscustomobject]@{
                Id = $writerIdentity.Id; StartTimeUtcTicks = $writerIdentity.StartTimeUtcTicks + 1
                Executable = $writerIdentity.Executable; Role = 'collector'
            }
            $wrongOwner | ConvertTo-Json -Depth 4 | Set-Content -LiteralPath (Join-Path $directory 'native-owner.json')
            $rejected = $false
            try { Invoke-GpuOwnerReconciliation $directory $executable 10 | Out-Null }
            catch { $rejected = $true }
            if (-not $rejected -or $writer.HasExited) { throw 'Fresh reconciliation accepted stale collector start-time identity.' }
            $owner | ConvertTo-Json -Depth 4 | Set-Content -LiteralPath (Join-Path $directory 'native-owner.json')
            . $runnerCleanup
            if (-not $writer.HasExited -or -not $helper.HasExited -or
                (Test-Path -LiteralPath $dump) -or (Test-Path -LiteralPath $helperDump)) {
                throw "$state monitor bypassed surviving-writer reconciliation."
            }
            if ($state -eq 'Failed' -and $captureFailure.Exception.Message -notmatch 'Deliberate completed monitor failure') {
                throw 'Failed-monitor error was not retained during reconciliation.'
            }
            Start-Sleep -Milliseconds 200
            if (Test-Path -LiteralPath $dump) { throw 'Completed/failed monitor writer recreated a deleted dump.' }
            if ($testFailure.Exception.Message -ne 'Windows GPU view run 1 failed with exit code 7.') {
                throw 'Native monitor reconciliation replaced the original test command failure.'
            }
        } finally {
            if (-not $writer.HasExited) { Stop-GpuOwnedProcess $writer $writerIdentity $PID { $true } | Out-Null }
            $writer.Dispose()
            if (-not $helper.HasExited) { Stop-GpuOwnedProcess $helper $helperIdentity $PID | Out-Null }
            $helper.Dispose()
            if (Get-Job -Id $monitor.Id -ErrorAction SilentlyContinue) { Remove-Job $monitor -Force }
        }
    }
    'Actual Completed/Failed monitor jobs reconcile surviving writers through the runner finally path.'

    $runResults = Join-Path $root 'late-analysis'
    $directory = Join-Path $runResults 'diagnostics'
    $privateDumps = Join-Path $privateRoot 'late-analysis'
    New-Item -ItemType Directory -Path $directory, $privateDumps -Force | Out-Null
    $dump = Join-Path $privateDumps 'late.dmp'
    'private stack data' | Set-Content -LiteralPath $dump
    $stopped = Join-Path $directory 'native-writer-stopped'
    $finished = Join-Path $directory 'late-analysis-finished'
    $start = [Diagnostics.ProcessStartInfo]::new($executable)
    $start.UseShellExecute = $false
    foreach ($argument in @('-NoLogo', '-NoProfile', '-Command',
        "while (-not (Test-Path -LiteralPath '$($stopped.Replace("'", "''"))')) { Start-Sleep -Milliseconds 25 }; " +
        "`$stream = [IO.File]::OpenRead('$($dump.Replace("'", "''"))'); try { Start-Sleep -Seconds 35 } finally { `$stream.Dispose() }; " +
        "[IO.File]::WriteAllText('$($finished.Replace("'", "''"))', 'Fixture fault/module/stack analysis completed')")) {
        $start.ArgumentList.Add($argument)
    }
    $helper = [Diagnostics.Process]::Start($start)
    $helperIdentity = Get-GpuOwnedProcessIdentity $helper $executable 'analysis'
    @{
        HostPid = 2147483646; HostPath = $executable; HostStartTimeUtcTicks = 1
        Collector = @{ Id = 2147483645; Executable = $executable; StartTimeUtcTicks = 1; Role = 'collector' }
    } | ConvertTo-Json -Depth 4 | Set-Content -LiteralPath (Join-Path $directory 'native-owner.json')
    $monitor = Microsoft.PowerShell.Core\Start-Job -ScriptBlock {
        param($Directory, $Helper)
        while (-not (Test-Path -LiteralPath (Join-Path $Directory 'native-writer-stopped'))) { Start-Sleep -Milliseconds 25 }
        $Helper | ConvertTo-Json | Set-Content -LiteralPath (Join-Path $Directory "native-helper-$($Helper.Id).json")
        Start-Sleep -Seconds 31
    } -ArgumentList $directory, $helperIdentity
    $global:GpuLateAnalysisWait = @{ Id = $monitor.Id; First = $true }
    function Wait-Job {
        param($Job, $Timeout)
        if ($Job.Id -eq $global:GpuLateAnalysisWait.Id -and $global:GpuLateAnalysisWait.First) {
            $global:GpuLateAnalysisWait.First = $false
            return
        }
        Microsoft.PowerShell.Core\Wait-Job -Job $Job -Timeout $Timeout
    }
    $CaptureNativeDumps = $true
    $tools = [pscustomobject]@{ ProcDump = $executable }
    $env:SKIASHARP_WINDOWS_GPU_DIAGNOSTICS = $directory
    $since = Get-Date
    $captureFailure = $null
    try { throw 'Windows GPU view run 1 failed with exit code 7.' } catch { $testFailure = $_ }
    try {
        $elapsed = [Diagnostics.Stopwatch]::StartNew()
        . $runnerCleanup
        if ($elapsed.Elapsed.TotalSeconds -lt 30 -or -not $helper.HasExited -or
            -not (Test-Path -LiteralPath $finished) -or (Test-Path -LiteralPath $dump) -or
            $monitor.State -in 'Running', 'NotStarted') {
            throw 'Runner returned before late analysis/final-transition reconciliation and raw-dump cleanup.'
        }
        if ($testFailure.Exception.Message -ne 'Windows GPU view run 1 failed with exit code 7.') {
            throw 'Late analysis replaced the original test command failure.'
        }
    } finally {
        Remove-Item Function:\Wait-Job
        Remove-Variable GpuLateAnalysisWait -Scope Global
        if (-not $helper.HasExited) { Stop-GpuOwnedProcess $helper $helperIdentity $PID | Out-Null }
        $helper.Dispose()
        if (Get-Job -Id $monitor.Id -ErrorAction SilentlyContinue) { Remove-Job $monitor -Force }
    }
    'Timed-out monitor analysis held past 30 seconds; runner verified final job/helper exit before dump cleanup.'

    # A recovery OS process must survive the failed job and the caller's throw.
    $directory = Join-Path $root 'surviving-recovery'
    $privateDumps = Join-Path $privateRoot 'surviving-recovery'
    New-Item -ItemType Directory -Path $directory, $privateDumps -Force | Out-Null
    $dump = Join-Path $privateDumps 'late.dmp'
    $escapedDump = $dump.Replace("'", "''")
    $start = [Diagnostics.ProcessStartInfo]::new($executable)
    $start.UseShellExecute = $false
    foreach ($argument in @('-NoLogo', '-NoProfile', '-Command',
        "while (`$true) { [IO.File]::AppendAllText('$escapedDump', 'private'); Start-Sleep -Milliseconds 50 }")) {
        $start.ArgumentList.Add($argument)
    }
    $writer = [Diagnostics.Process]::Start($start)
    $writerIdentity = Get-GpuOwnedProcessIdentity $writer $executable 'collector'
    @{
        HostPid = 2147483646; HostPath = $executable; HostStartTimeUtcTicks = 1
        Collector = $writerIdentity
        DebuggerPath = $executable
        ExpectedApplicationRoot = Join-Path $testRepository 'tests/SkiaSharp.Tests.Devices/bin'
    } | ConvertTo-Json -Depth 4 | Set-Content -LiteralPath (Join-Path $directory 'native-owner.json')
    $handoffJob = Microsoft.PowerShell.Core\Start-Job -ScriptBlock {
        param($Helpers, $Directory, $Executable, $PrivateDumps, $Repository)
        . $Helpers
        Complete-GpuNativeOwnership $Directory $Executable $PrivateDumps $Repository `
            -GraceMilliseconds 50 -VerifyDetached { $false } -WaitForParentRecovery
    } -ArgumentList "$testRepository/scripts/infra/tests/windows-gpu-process-ownership.ps1", `
        $directory, $executable, $privateDumps, $repository
    try {
        $deadline = [datetime]::UtcNow.AddSeconds(20)
        while (-not (Test-Path -LiteralPath (Join-Path $directory 'native-handoff-pending')) -and
            [datetime]::UtcNow -lt $deadline) { Start-Sleep -Milliseconds 50 }
        if ($handoffJob.State -ne 'Running' -or $writer.HasExited -or
            -not (Test-Path -LiteralPath (Join-Path $directory 'native-handoff-pending'))) {
            throw 'A job lost attached-writer ownership before parent recovery takeover.'
        }
        $caught = $null
        try {
            Complete-GpuNativeOwnership $directory $executable $privateDumps $repository `
                -GraceMilliseconds 50 -VerifyDetached { $false } | Out-Null
        } catch { $caught = $_ }
        if ($caught.Exception.Message -notmatch 'surviving recovery PID') { throw "Takeover did not retain a real owner: $caught" }
        $recoveryIdentity = Get-Content -LiteralPath (Join-Path $directory 'native-recovery-owner.json') -Raw | ConvertFrom-Json
        $recovery = Get-Process -Id $recoveryIdentity.Id -ErrorAction Stop
        Assert-GpuOwnedProcessIdentity $recovery $recoveryIdentity $PID
        Wait-Job $handoffJob -Timeout 15 | Out-Null
        if ($handoffJob.State -ne 'Failed') { throw 'The job did not acknowledge the surviving parent-owned recovery process.' }
        Remove-Job $handoffJob
        if ($recovery.HasExited) { throw 'Recovery owner did not survive terminating-job disposal.' }
        if (-not $recovery.WaitForExit(60000)) { throw 'Independent recovery did not stop the verified detached fixture writer.' }
        if (-not $writer.HasExited -or (Test-Path -LiteralPath $dump)) { throw 'Surviving recovery did not verify exit before cleanup.' }
        if (-not (Test-Path -LiteralPath (Join-Path $directory 'native-writer-stopped'))) { throw 'Recovery exit lacked its safe-cleanup acknowledgement.' }
        if ((Get-Content -LiteralPath (Join-Path $directory 'native-recovery-analysis-error.txt') -Raw) -notmatch 'Private native dump analysis failed') {
            throw 'Real recovery skipped its debugger attempt/error evidence before dump deletion.'
        }
        $recovery.Dispose()
    } finally {
        if (-not $writer.HasExited) { Stop-GpuOwnedProcess $writer $writerIdentity $PID { $true } | Out-Null }
        $writer.Dispose()
        if (Get-Job -Id $handoffJob.Id -ErrorAction SilentlyContinue) { Remove-Job $handoffJob -Force }
    }
    'Independent recovery owner survives caller failure and verifies collector exit before deleting private memory.'

    $recoveryAst = [Management.Automation.Language.Parser]::ParseFile(
        "$testRepository/scripts/infra/tests/recover-windows-gpu-dump.ps1", [ref] $null, [ref] $parseErrors)
    if ($parseErrors) { throw ($parseErrors | Out-String) }
    $recoveryText = $recoveryAst.EndBlock.Extent.Text.Replace('$PSScriptRoot', "$testRepository/scripts/infra/tests")
    $recoveryBody = [scriptblock]::Create($recoveryText)
    $previousAnalyzer = $env:SKIASHARP_GPU_CAPTURE_FIXTURE_ANALYZER
    try {
        foreach ($mode in 'success', 'error') {
            $directory = Join-Path $root "recovery-analysis-$mode"
            $DumpDirectory = Join-Path $privateRoot "recovery-analysis-$mode"
            New-Item -ItemType Directory -Path $directory, $DumpDirectory -Force | Out-Null
            $dump = Join-Path $DumpDirectory 'fault.dmp'
            'private stack data' | Set-Content -LiteralPath $dump
            $ProcDumpPath = $executable
            $DebuggerPath = $executable
            $ExpectedApplicationRoot = Join-Path $testRepository 'tests/SkiaSharp.Tests.Devices/bin'
            @{
                HostPid = 2147483646; HostPath = $executable; HostStartTimeUtcTicks = 1
                Collector = @{ Id = 2147483645; Executable = $executable; StartTimeUtcTicks = 1; Role = 'collector' }
            } | ConvertTo-Json -Depth 4 | Set-Content -LiteralPath (Join-Path $directory 'native-owner.json')
            'Original host/cancellation failure.' | Set-Content -LiteralPath (Join-Path $directory 'native-recovery-error.txt')
            $env:SKIASHARP_GPU_CAPTURE_FIXTURE_ANALYZER = $mode
            . $recoveryBody -Directory $directory -ProcDumpPath $ProcDumpPath `
                -DumpDirectory $DumpDirectory -Repository $repository `
                -DebuggerPath $DebuggerPath -ExpectedApplicationRoot $ExpectedApplicationRoot
            $metadata = Get-Content -LiteralPath (Join-Path $directory 'native-stack-2147483646.txt') -Raw
            if ($metadata -notmatch 'Fixture fault:' -or $metadata -notmatch 'Fixture module:' -or $metadata -notmatch 'Fixture stack:') {
                throw "Recovery $mode did not attempt shared private fault/module/stack analysis."
            }
            if (Test-Path -LiteralPath $dump) { throw "Recovery $mode left raw memory after its analysis attempt." }
            if ((Get-Content -LiteralPath (Join-Path $directory 'native-recovery-error.txt') -Raw).Trim() -ne 'Original host/cancellation failure.') {
                throw 'Recovery analysis replaced the original failure.'
            }
            if ($mode -eq 'error' -and
                (Get-Content -LiteralPath (Join-Path $directory 'native-recovery-analysis-error.txt') -Raw) -notmatch 'exit code 23') {
                throw 'Recovery analysis-error cleanup lost the secondary failure.'
            }
        }
    } finally { $env:SKIASHARP_GPU_CAPTURE_FIXTURE_ANALYZER = $previousAnalyzer }
    'Shared recovery analysis produced fault/module/stack metadata; analysis-error cleanup preserved the original failure.'

    $global:WindowsGpuCaptureFixture = [pscustomobject]@{ Runs = 0; Fail = $false }
    function dotnet {
        if ($args[0] -eq 'msbuild') {
            $global:LASTEXITCODE = 0
            'net10.0-windows10.0.19041.0'
            return
        }
        $global:WindowsGpuCaptureFixture.Runs++
        $global:LASTEXITCODE = if ($global:WindowsGpuCaptureFixture.Fail) { 7 } else { 0 }
        $index = [array]::IndexOf($args, '--results-directory')
        $result = $args[$index + 1]
        New-Item -ItemType Directory -Path $result -Force | Out-Null
        '<TestRun><ResultSummary><Counters total="8" passed="8" failed="0" /></ResultSummary></TestRun>' |
            Set-Content -LiteralPath (Join-Path $result 'test-results.trx')
    }
    function Get-WinEvent { throw 'Deliberate diagnostic capture failure.' }
    $previousProfile = $env:LOCALAPPDATA
    $previousDump = $env:SKIASHARP_WINDOWS_GPU_NATIVE_DUMP
    $env:LOCALAPPDATA = Join-Path $root 'profile'
    $env:SKIASHARP_WINDOWS_GPU_NATIVE_DUMP = '1'
    try {
        & "$testRepository/scripts/infra/tests/run-windows-gpu-views.ps1" -ResultsDirectory (Join-Path $root 'normal')
        if ($global:WindowsGpuCaptureFixture.Runs -ne 5) { throw "The non-debugger gate did not run five independent hosts: $($global:WindowsGpuCaptureFixture.Runs)" }
        $first = Get-ChildItem -LiteralPath (Join-Path $root 'normal') -Directory | Select-Object -First 1
        $stale = Join-Path $first.FullName 'run-1/diagnostics'
        New-Item -ItemType Directory -Path $stale -Force | Out-Null
        New-Item -ItemType File -Path (Join-Path $stale 'stop-native-monitor') -Force | Out-Null
        'prior diagnostic evidence' | Set-Content -LiteralPath (Join-Path $stale 'native-owner.json')
        & "$testRepository/scripts/infra/tests/run-windows-gpu-views.ps1" -ResultsDirectory (Join-Path $root 'normal')
        $invocations = @(Get-ChildItem -LiteralPath (Join-Path $root 'normal') -Directory)
        if ($invocations.Count -ne 2 -or $global:WindowsGpuCaptureFixture.Runs -ne 10) {
            throw 'Repeat invocation did not create fresh independent control paths.'
        }
        if ((Get-Content -LiteralPath (Join-Path $stale 'native-owner.json') -Raw).Trim() -ne 'prior diagnostic evidence') {
            throw 'Repeat invocation deleted previous diagnostics.'
        }
        $second = $invocations | Where-Object { $_.FullName -ne $first.FullName }
        if (Test-Path -LiteralPath (Join-Path $second.FullName 'run-1/diagnostics/stop-native-monitor')) {
            throw 'Fresh invocation inherited a stale cancellation marker.'
        }
        'Repeat invocation uses fresh control/ownership paths and preserves prior diagnostic evidence.'
        if ($env:SKIASHARP_WINDOWS_GPU_NATIVE_DUMP -ne '1') { throw 'The prior native dump opt-in value was not restored.' }
        $global:WindowsGpuCaptureFixture.Fail = $true
        $caught = $null
        try {
            & "$testRepository/scripts/infra/tests/run-windows-gpu-views.ps1" -ResultsDirectory (Join-Path $root 'failed')
        } catch { $caught = $_ }
        if ($caught.Exception.Message -ne 'Windows GPU view run 1 failed with exit code 7.') {
            throw "The original test failure was replaced: $($caught.Exception.Message)"
        }
        'Normal five-host gate requires no debugger tools; failed test exit is preserved.'
    } finally {
        Remove-Variable -Name WindowsGpuCaptureFixture -Scope Global
        $env:LOCALAPPDATA = $previousProfile
        $env:SKIASHARP_WINDOWS_GPU_NATIVE_DUMP = $previousDump
    }
} finally {
    Remove-Item -LiteralPath $root -Recurse -Force
    Remove-Item -LiteralPath $privateRoot -Recurse -Force
}
