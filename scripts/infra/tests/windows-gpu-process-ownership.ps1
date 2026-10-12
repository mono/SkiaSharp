function Get-GpuOwnedProcessIdentity {
    param(
        [Parameter(Mandatory = $true)] [Diagnostics.Process] $Process,
        [Parameter(Mandatory = $true)] [string] $Executable,
        [Parameter(Mandatory = $true)] [ValidateSet('collector', 'cancel', 'analysis', 'monitor', 'recovery')] [string] $Role
    )
    $started = 0L
    $Process.Refresh()
    if (-not $Process.HasExited) { $started = $Process.StartTime.ToUniversalTime().Ticks }
    [pscustomobject]@{
        Id = $Process.Id
        StartTimeUtcTicks = $started
        Executable = [IO.Path]::GetFullPath($Executable)
        Role = $Role
    }

}

function Assert-GpuPrivatePath {
    param(
        [Parameter(Mandatory = $true)] [string] $Path,
        [Parameter(Mandatory = $true)] [string] $Repository
    )
    $private = [IO.Path]::GetFullPath($Path).TrimEnd([IO.Path]::DirectorySeparatorChar) + [IO.Path]::DirectorySeparatorChar
    $published = [IO.Path]::GetFullPath((Join-Path $Repository 'output')).TrimEnd([IO.Path]::DirectorySeparatorChar) + [IO.Path]::DirectorySeparatorChar
    if ($private.StartsWith($published, [StringComparison]::OrdinalIgnoreCase) -or
        $published.StartsWith($private, [StringComparison]::OrdinalIgnoreCase)) {
        throw 'GPU dump/tool storage must be disjoint from the whole-output failure publication root.'
    }
    $ancestor = [IO.Path]::GetFullPath($Path)
    while ($ancestor) {
        if ((Test-Path -LiteralPath $ancestor) -and
            ((Get-Item -LiteralPath $ancestor -Force).Attributes -band [IO.FileAttributes]::ReparsePoint)) {
            throw 'GPU private storage must not traverse a symbolic link or junction.'
        }
        $ancestor = Split-Path -Parent $ancestor
    }
}

function Save-GpuHelperIdentity {
    param([string] $Directory, $Identity)
    $Identity | ConvertTo-Json | Set-Content -LiteralPath (Join-Path $Directory "native-helper-$($Identity.Id).json")
}

function Invoke-GpuOwnerReconciliation {
    param(
        [Parameter(Mandatory = $true)] [string] $Directory,
        [Parameter(Mandatory = $true)] [string] $ProcDumpPath,
        [int] $GraceMilliseconds = 10000,
        [scriptblock] $VerifyDetached
    )
    $record = Join-Path $Directory 'native-owner.json'
    if (-not (Test-Path -LiteralPath $record)) { return $true }
    $identity = Get-Content -LiteralPath $record -Raw | ConvertFrom-Json
    if ($identity.Collector.Role -ne 'collector' -or
        -not [string]::Equals($identity.Collector.Executable, [IO.Path]::GetFullPath($ProcDumpPath), [StringComparison]::OrdinalIgnoreCase)) {
        throw 'Collector record does not match this invocation tool; refusing recovery.'
    }
    $collector = Get-Process -Id $identity.Collector.Id -ErrorAction SilentlyContinue
    if ($collector) { Assert-GpuOwnedProcessIdentity $collector $identity.Collector $identity.HostPid }
    foreach ($record in @(Get-ChildItem -LiteralPath $Directory -Filter 'native-helper-*.json' -File)) {
        $helperIdentity = Get-Content -LiteralPath $record.FullName -Raw | ConvertFrom-Json
        if ($helperIdentity.Role -notin 'cancel', 'analysis') { throw 'Invalid owned helper role.' }
        $helper = Get-Process -Id $helperIdentity.Id -ErrorAction SilentlyContinue
        if ($helper) {
            try {
                Assert-GpuOwnedProcessIdentity $helper $helperIdentity $identity.HostPid
                if ($helperIdentity.Role -ne 'analysis' -or -not $helper.WaitForExit(120000)) {
                    Stop-GpuOwnedProcess $helper $helperIdentity $identity.HostPid | Out-Null
                }
            }
            finally { if ($helper.HasExited) { $helper.Dispose() } }
        }
    }
    $hostIdentity = $identity
    $detachmentCallback = $VerifyDetached
    $detached = {
        if ($detachmentCallback) { return (& $detachmentCallback $hostIdentity) }
        return (Test-GpuHostDetached $hostIdentity)
    }
    if (-not $collector) { return (& $detached) }
    try {
        Assert-GpuOwnedProcessIdentity $collector $identity.Collector $identity.HostPid
        try { Signal-GpuProcDumpCancellation $identity.HostPid }
        catch { Write-Warning "PID-scoped cancellation event unavailable: $_" }
        if (-not $collector.WaitForExit($GraceMilliseconds)) {
            $cancel = $null
            $cancelFailure = $null
            try {
                $cancel = Start-Process -FilePath $ProcDumpPath -ArgumentList @('-cancel', $identity.HostPid) `
                    -PassThru -RedirectStandardOutput (Join-Path $Directory 'owner-cancel.stdout.log') `
                    -RedirectStandardError (Join-Path $Directory 'owner-cancel.stderr.log')
                $cancelIdentity = Get-GpuOwnedProcessIdentity $cancel $ProcDumpPath 'cancel'
                Save-GpuHelperIdentity $Directory $cancelIdentity
                if (-not $cancel.WaitForExit($GraceMilliseconds)) {
                    Stop-GpuOwnedProcess $cancel $cancelIdentity $identity.HostPid | Out-Null
                }
            } catch {
                Write-Warning "PID-scoped cancellation helper failed: $_"
                if ($cancel -and -not $cancel.HasExited) { $cancelFailure = $_ }
            } finally {
                if ($cancel -and $cancel.HasExited) { $cancel.Dispose() }
            }
            if ($cancelFailure) { throw $cancelFailure }
            if (-not $collector.WaitForExit($GraceMilliseconds) -and
                -not (Stop-GpuOwnedProcess $collector $identity.Collector $identity.HostPid $detached)) {
                return $false
            }
        }
        return (& $detached)
    } finally {
        if ($collector.HasExited) { $collector.Dispose() }
    }
}

function Start-GpuRecoveryOwner {
    param(
        [string] $Directory, [string] $ProcDumpPath, [string] $DumpDirectory,
        [string] $Repository, [string] $RecoveryScript = "$PSScriptRoot/recover-windows-gpu-dump.ps1"
    )
    Assert-GpuPrivatePath $DumpDirectory $Repository
    $existing = Join-Path $Directory 'native-recovery-owner.json'
    if (Test-Path -LiteralPath $existing) {
        $identity = Get-Content -LiteralPath $existing -Raw | ConvertFrom-Json
        if ($identity.Role -ne 'recovery') { throw 'Invalid recovery-owner role; refusing takeover.' }
        $process = Get-Process -Id $identity.Id -ErrorAction SilentlyContinue
        if ($process) {
            Assert-GpuOwnedProcessIdentity $process $identity 0
            $ready = Join-Path $Directory "native-recovery-ready-$($identity.Id)"
            if (-not (Test-Path -LiteralPath $ready) -or
                (Get-Content -LiteralPath $ready -Raw).Trim() -ne "$($identity.Id)") {
                throw 'Existing recovery owner has not acknowledged this invocation.'
            }
            return $process
        }
    }
    $executable = (Get-Process -Id $PID).MainModule.FileName
    $start = [Diagnostics.ProcessStartInfo]::new($executable)
    $start.UseShellExecute = $false
    $ownership = Get-Content -LiteralPath (Join-Path $Directory 'native-owner.json') -Raw | ConvertFrom-Json
    if (-not $ownership.DebuggerPath -or -not $ownership.ExpectedApplicationRoot) {
        throw 'Recovery handoff lacks the owning debugger/analysis inputs.'
    }
    foreach ($argument in @('-NoLogo', '-NoProfile', '-File', $RecoveryScript,
        '-Directory', $Directory, '-ProcDumpPath', $ProcDumpPath,
        '-DumpDirectory', $DumpDirectory, '-Repository', $Repository,
        '-DebuggerPath', $ownership.DebuggerPath, '-ExpectedApplicationRoot', $ownership.ExpectedApplicationRoot)) {
        $start.ArgumentList.Add($argument)
    }

    $process = [Diagnostics.Process]::Start($start)
    $identity = Get-GpuOwnedProcessIdentity $process $executable 'recovery'
    $ready = Join-Path $Directory "native-recovery-ready-$($process.Id)"
    $deadline = [datetime]::UtcNow.AddSeconds(15)
    while (-not (Test-Path -LiteralPath $ready) -and [datetime]::UtcNow -lt $deadline -and -not $process.HasExited) {
        Start-Sleep -Milliseconds 50
    }
    if (-not (Test-Path -LiteralPath $ready) -or (Get-Content -LiteralPath $ready -Raw).Trim() -ne "$($process.Id)") {
        throw "Recovery owner PID $($process.Id) did not acknowledge takeover."
    }
    if (-not $process.HasExited) { Assert-GpuOwnedProcessIdentity $process $identity 0 }
    $identity | ConvertTo-Json | Set-Content -LiteralPath $existing
    return $process
}

function Invoke-GpuPrivateDumpAnalysis {
    param(
        [string] $Directory, [string] $DumpDirectory, [string] $DebuggerPath,
        [string] $ExpectedApplicationRoot, [string] $Repository, [int] $HostId
    )
    Assert-GpuPrivatePath $DumpDirectory $Repository
    foreach ($dump in @(Get-ChildItem -LiteralPath $DumpDirectory -Filter '*.dmp' -File)) {
        $analysis = $null
        try {
            $native = Join-Path $Repository 'output/native'
            $symbols = @($ExpectedApplicationRoot)
            if (Test-Path -LiteralPath $native) {
                $symbols += @(Get-ChildItem -LiteralPath $native -Filter '*.pdb' -Recurse -File |
                    Select-Object -ExpandProperty DirectoryName -Unique)
            }
            $symbols += "srv*$(Join-Path $DumpDirectory 'symbols')*https://msdl.microsoft.com/download/symbols"
            $analysis = Start-Process -FilePath $DebuggerPath -ArgumentList @(
                '-z', "`"$($dump.FullName)`"", '-y', "`"$($symbols -join ';')`"",
                '-c', '".exr -1; .ecxr; kb 40; lm; q"'
            ) -PassThru -RedirectStandardOutput (Join-Path $Directory "native-stack-$HostId.txt") `
                -RedirectStandardError (Join-Path $Directory "native-stack-$HostId.error.txt")
            $analysisIdentity = Get-GpuOwnedProcessIdentity $analysis $DebuggerPath 'analysis'
            Save-GpuHelperIdentity $Directory $analysisIdentity
            if (-not $analysis.WaitForExit(120000)) {
                Stop-GpuOwnedProcess $analysis $analysisIdentity $HostId | Out-Null
                throw 'Private native dump analysis exceeded its two-minute bound.'
            }
            if ($analysis.ExitCode -ne 0) { throw "Private native dump analysis failed with exit code $($analysis.ExitCode)." }
        } finally {
            if ($analysis -and -not $analysis.HasExited) {
                Stop-GpuOwnedProcess $analysis $analysisIdentity $HostId | Out-Null
            }
            if ($analysis -and $analysis.HasExited) { $analysis.Dispose() }
        }
    }
}

function Complete-GpuNativeOwnership {
    param(
        [string] $Directory, [string] $ProcDumpPath, [string] $DumpDirectory,
        [string] $Repository, [int] $GraceMilliseconds = 10000,
        [scriptblock] $VerifyDetached,
        [string] $RecoveryScript = "$PSScriptRoot/recover-windows-gpu-dump.ps1",
        [switch] $WaitForParentRecovery
    )
    Assert-GpuPrivatePath $DumpDirectory $Repository
    $stopped = Join-Path $Directory 'native-writer-stopped'
    Remove-Item -LiteralPath $stopped -Force -ErrorAction SilentlyContinue
    # Reconcile even when a marker exists: an abandoned analysis/cancel helper
    # may outlive the writer. Only fresh verified identities permit cleanup.
    $safe = $false
    $errorRecord = $null
    try { $safe = Invoke-GpuOwnerReconciliation $Directory $ProcDumpPath $GraceMilliseconds $VerifyDetached }
    catch { $errorRecord = $_ }
    if (-not $safe -and $WaitForParentRecovery) {
        # A job child cannot guarantee that its own descendants survive job
        # disposal. Keep it alive until the outer runner acknowledges takeover.
        while (-not $safe) {
            $record = Join-Path $Directory 'native-recovery-owner.json'
            if (Test-Path -LiteralPath $record) {
                $identity = Get-Content -LiteralPath $record -Raw | ConvertFrom-Json
                if ($identity.Role -ne 'recovery') { throw 'Invalid parent recovery-owner role.' }
                $owner = Get-Process -Id $identity.Id -ErrorAction SilentlyContinue
                if ($owner) {
                    try {
                        Assert-GpuOwnedProcessIdentity $owner $identity 0
                        $ready = Join-Path $Directory "native-recovery-ready-$($identity.Id)"
                        if ((Test-Path -LiteralPath $ready) -and
                            (Get-Content -LiteralPath $ready -Raw).Trim() -eq "$($identity.Id)") {
                            throw "Native recovery handed to independent parent-owned PID $($identity.Id)."
                        }
                    } finally { $owner.Dispose() }
                }
            }
            [IO.File]::WriteAllText((Join-Path $Directory 'native-handoff-pending'), [datetime]::UtcNow.ToString('o'))
            Start-Sleep -Seconds 5
            try { $safe = Invoke-GpuOwnerReconciliation $Directory $ProcDumpPath $GraceMilliseconds $VerifyDetached }
            catch { $errorRecord = $_ }
        }
    }
    if (-not $safe) {
        $recovery = Start-GpuRecoveryOwner $Directory $ProcDumpPath $DumpDirectory $Repository $RecoveryScript
        if ($errorRecord) { throw $errorRecord }
        throw "Native ownership handed to surviving recovery PID $($recovery.Id); dumps remain private until verified detachment and helper exit."
    }
    [IO.File]::WriteAllText($stopped, 'Verified collector/helper exit and host detachment.')
    return $true
}

function Assert-GpuOwnedProcessIdentity {
    param(
        [Parameter(Mandatory = $true)] [Diagnostics.Process] $Process,
        [Parameter(Mandatory = $true)] $Identity,
        [int] $ProtectedHostId
    )
    if ($Process.Id -eq $ProtectedHostId) { throw 'Refusing recovery against the test host.' }
    $Process.Refresh()
    if ($Process.HasExited) { return }
    $current = Get-Process -Id $Identity.Id -ErrorAction SilentlyContinue
    if (-not $current) {
        $Process.Refresh()
        if ($Process.HasExited) { return }
        throw "Cannot inspect owned GPU helper PID $($Identity.Id); refusing recovery."
    }
    try {
        if ($Process.Id -ne $Identity.Id -or
            $current.StartTime.ToUniversalTime().Ticks -ne $Identity.StartTimeUtcTicks -or
            -not [string]::Equals($current.MainModule.FileName, $Identity.Executable, [StringComparison]::OrdinalIgnoreCase)) {
            throw "Owned GPU helper identity changed for PID $($Identity.Id); refusing recovery."
        }
    } finally {
        $current.Dispose()
    }
}

function Stop-GpuOwnedProcess {
    param(
        [Parameter(Mandatory = $true)] [Diagnostics.Process] $Process,
        [Parameter(Mandatory = $true)] $Identity,
        [int] $ProtectedHostId,
        [scriptblock] $VerifyDetached,
        [int] $ExitTimeoutMilliseconds = 10000
    )
    Assert-GpuOwnedProcessIdentity $Process $Identity $ProtectedHostId
    if ($Process.HasExited) { return $true }
    # A debugger must not be force-stopped while it can terminate its debuggee.
    if ($Identity.Role -eq 'collector' -and (-not $VerifyDetached -or -not (& $VerifyDetached))) {
        return $false
    }
    $Process.Kill()
    if (-not $Process.WaitForExit($ExitTimeoutMilliseconds)) {
        throw "Owned GPU $($Identity.Role) PID $($Identity.Id) survived bounded recovery."
    }
    return $true
}

function Signal-GpuProcDumpCancellation {
    param([Parameter(Mandatory = $true)] [int] $HostId)
    # ProcDump documents this PID-scoped event as its graceful Ctrl+C equivalent.
    $event = [Threading.EventWaitHandle]::OpenExisting("ProcDump-$HostId")
    try { $event.Set() | Out-Null }
    finally { $event.Dispose() }
}

function Test-GpuHostDetached {
    param([Parameter(Mandatory = $true)] $Identity)
    $hostProcess = Get-Process -Id $Identity.HostPid -ErrorAction SilentlyContinue
    if (-not $hostProcess) { return $true }
    try {
        if ($hostProcess.StartTime.ToUniversalTime().Ticks -ne $Identity.HostStartTimeUtcTicks -or
            -not [string]::Equals($hostProcess.MainModule.FileName, $Identity.HostPath, [StringComparison]::OrdinalIgnoreCase)) {
            throw 'Owning host identity changed; refusing debugger-state recovery.'
        }
        if (-not ('SkiaGpuNativeDebugger' -as [type])) {
            Add-Type -TypeDefinition @'
using System;
using System.Runtime.InteropServices;
public static class SkiaGpuNativeDebugger
{
    [DllImport("kernel32.dll", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    public static extern bool CheckRemoteDebuggerPresent(IntPtr process, [MarshalAs(UnmanagedType.Bool)] out bool present);
}
'@
        }
        $present = $false
        if (-not [SkiaGpuNativeDebugger]::CheckRemoteDebuggerPresent($hostProcess.Handle, [ref] $present)) {
            throw "Cannot verify debugger detachment from owning PID $($Identity.HostPid)."
        }
        return -not $present
    } finally { $hostProcess.Dispose() }
}
