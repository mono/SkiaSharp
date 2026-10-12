[CmdletBinding(DefaultParameterSetName = 'Monitor')]
param(
    [Parameter(Mandatory = $true)]
    [string] $ProcDumpPath,
    [Parameter(Mandatory = $true)]
    [string] $DebuggerPath,
    [Parameter(ParameterSetName = 'Monitor', Mandatory = $true)]
    [string] $ResultsDirectory,
    [Parameter(ParameterSetName = 'Monitor', Mandatory = $true)]
    [string] $ExpectedApplicationRoot,
    [Parameter(ParameterSetName = 'Monitor', Mandatory = $true)]
    [string] $DumpDirectory,
    [Parameter(ParameterSetName = 'Monitor', Mandatory = $true)]
    [datetime] $Since,
    [Parameter(ParameterSetName = 'Verify', Mandatory = $true)]
    [switch] $VerifyTool
)

$ErrorActionPreference = 'Stop'
. "$PSScriptRoot/windows-gpu-process-ownership.ps1"
if (-not $IsWindows) { throw 'Native GPU dump collection requires a Windows worker.' }
$tool = (Resolve-Path -LiteralPath $ProcDumpPath).Path
$signature = Get-AuthenticodeSignature -LiteralPath $tool
if ($signature.Status -ne 'Valid' -or $signature.SignerCertificate.Subject -notmatch 'O=Microsoft Corporation(?:,|$)') {
    throw 'Native GPU dump collection requires an approved Microsoft-signed ProcDump executable.'
}
$debugger = (Resolve-Path -LiteralPath $DebuggerPath).Path
$debuggerSignature = Get-AuthenticodeSignature -LiteralPath $debugger
if ($debuggerSignature.Status -ne 'Valid' -or $debuggerSignature.SignerCertificate.Subject -notmatch 'O=Microsoft Corporation(?:,|$)') {
    throw 'Private native dump analysis requires an approved Microsoft-signed Windows SDK cdb executable.'
}
$accepted = Get-ItemPropertyValue -LiteralPath 'HKCU:\Software\Sysinternals\ProcDump' `
    -Name EulaAccepted -ErrorAction SilentlyContinue
if ($accepted -ne 1) {
    throw 'ProcDump licensing must be completed by get-windows-gpu-debug-tools.ps1 before attachment.'
}
if ($VerifyTool) { return }

$directory = Join-Path $ResultsDirectory 'diagnostics'
New-Item -ItemType Directory -Path $directory -Force | Out-Null
$privateDumps = [IO.Path]::GetFullPath($DumpDirectory)
$repository = Split-Path -Parent (Split-Path -Parent (Split-Path -Parent $ExpectedApplicationRoot))
Assert-GpuPrivatePath $privateDumps $repository
New-Item -ItemType Directory -Path $privateDumps -Force | Out-Null
$stopFile = Join-Path $directory 'stop-native-monitor'
$applicationRoot = [IO.Path]::GetFullPath($ExpectedApplicationRoot).TrimEnd('\') + '\'
$clock = [Diagnostics.Stopwatch]::StartNew()
$monitor = $null
$target = $null
$targetId = 0
$failure = $null
$monitorIdentity = $null
$writerStopped = $true

try {
    while ($clock.Elapsed.TotalSeconds -lt 900 -and -not (Test-Path -LiteralPath $stopFile)) {
        $startup = Get-ChildItem -LiteralPath $directory -Filter 'host-*.log' -File |
            Where-Object { $_.LastWriteTimeUtc -ge $Since.ToUniversalTime() } |
            Sort-Object CreationTimeUtc | Select-Object -First 1
        if ($startup) {
            if ($startup.BaseName -notmatch '^host-(\d+)$') { throw 'Invalid owning-host PID record.' }
            $targetId = [int] $Matches[1]
            $target = Get-Process -Id $targetId -ErrorAction Stop
            $path = $target.MainModule.FileName
            if ($target.StartTime.ToUniversalTime() -lt $Since.ToUniversalTime() -or
                -not $path.StartsWith($applicationRoot, [StringComparison]::OrdinalIgnoreCase) -or
                [IO.Path]::GetFileName($path) -ne 'SkiaSharp.Tests.Devices.exe') {
                throw "Refusing native dump attachment to out-of-scope PID $targetId."
            }
            break
        }
        Start-Sleep -Milliseconds 50
    }
    if (-not $target) {
        if (Test-Path -LiteralPath $stopFile) { throw 'Native collection stopped before the owning test host started.' }
        throw 'The owning Windows GPU host did not publish its PID within the bounded collection window.'
    }

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
    $attached = $false
    if (-not [SkiaGpuNativeDebugger]::CheckRemoteDebuggerPresent($target.Handle, [ref] $attached)) {
        throw "Cannot inspect debugger state for owning PID ${targetId}: $([Runtime.InteropServices.Marshal]::GetLastWin32Error())."
    }
    if ($attached) { throw "Owning PID $targetId already has a debugger; refusing to replace it." }

    # Direct PID monitoring only: never -i, -u, -wer, -w, or a process-name match.
    # -g selects native exception monitoring; -e captures an unhandled fault.
    $monitor = Start-Process -FilePath $tool -ArgumentList @(
        '-g', '-e', '-mm', '-n', '1', '-at', '30', $targetId, "`"$privateDumps`""
    ) -PassThru -RedirectStandardOutput (Join-Path $directory "procdump-$targetId.stdout.log") `
        -RedirectStandardError (Join-Path $directory "procdump-$targetId.stderr.log")
    $monitorIdentity = Get-GpuOwnedProcessIdentity $monitor $tool 'collector'
    @{
        HostPid = $targetId
        HostPath = $path
        HostStartTimeUtcTicks = $target.StartTime.ToUniversalTime().Ticks
        Collector = $monitorIdentity
        DebuggerPath = $debugger
        ExpectedApplicationRoot = $ExpectedApplicationRoot
    } | ConvertTo-Json -Depth 4 | Set-Content -LiteralPath (Join-Path $directory 'native-owner.json')
    $writerStopped = $false

    $attachment = [Diagnostics.Stopwatch]::StartNew()
    while ($attachment.Elapsed.TotalSeconds -lt 15) {
        if ($monitor.HasExited) { throw "ProcDump exited before attachment with code $($monitor.ExitCode)." }
        if ($target.HasExited) { throw 'The owning host exited before native dump attachment.' }
        if ([SkiaGpuNativeDebugger]::CheckRemoteDebuggerPresent($target.Handle, [ref] $attached) -and $attached) {
            break
        }
        Start-Sleep -Milliseconds 25
    }
    if (-not $attached) { throw 'Native dump attachment did not complete within 15 seconds; debugger permission is required.' }
    @{
        HostPid = $targetId
        HostPath = $path
        CollectorPid = $monitor.Id
        CollectorPath = $tool
        Publisher = $signature.SignerCertificate.Subject
        AttachedUtc = [datetime]::UtcNow
    } | ConvertTo-Json | Set-Content -LiteralPath (Join-Path $directory "native-attachment-$targetId.json")
    [IO.File]::WriteAllText((Join-Path $directory "native-ready-$targetId"), "$targetId")

    while (-not $monitor.HasExited -and $clock.Elapsed.TotalSeconds -lt 900 -and
        -not (Test-Path -LiteralPath $stopFile)) {
        Start-Sleep -Milliseconds 100
    }
} catch {
    $failure = $_
} finally {
    try {
        if ($monitor) {
            $writerStopped = $false
            $writerStopped = Complete-GpuNativeOwnership $directory $tool $privateDumps $repository -WaitForParentRecovery
        }
    } catch {
        if (-not $failure) { $failure = $_ }
        else { Write-Warning "Secondary native collector cancellation failure: $_" }
    } finally {
        if (-not $monitor) { $writerStopped = $true }
        if ($writerStopped) {
            if ($monitor) { $monitor.Dispose() }
            if ($target) { $target.Dispose() }
            [IO.File]::WriteAllText((Join-Path $directory 'native-writer-stopped'), "$targetId")
        }
    }

    try {
        # Only fault metadata, module names and native stacks enter public test logs.
        # Analyze even after attachment/cancellation failure; cleanup never depends
        # on reaching a success-only tail of this script.
        if ($writerStopped) {
            Invoke-GpuPrivateDumpAnalysis $directory $privateDumps $debugger $ExpectedApplicationRoot $repository $targetId
        }
    } catch {
        if (-not $failure) { $failure = $_ }
        else { Write-Warning "Secondary native dump analysis failure: $_" }
    } finally {
        try {
            if ($writerStopped) {
                if (Test-Path -LiteralPath (Join-Path $directory 'native-owner.json')) {
                    $writerStopped = $false
                    $writerStopped = Complete-GpuNativeOwnership $directory $tool $privateDumps $repository -WaitForParentRecovery
                }
                Get-ChildItem -LiteralPath $privateDumps -Filter '*.dmp' -File |
                    Remove-Item -Force -ErrorAction Stop
            }
        } catch {
            if (-not $failure) { $failure = $_ }
            else { Write-Warning "Private native dump deletion failure: $_" }
        }
    }
}
if ($failure) {
    $failure | Out-String | Set-Content -LiteralPath (Join-Path $directory 'native-monitor-error.txt')
    throw $failure
}
