param(
    [Parameter(Mandatory = $true)]
    [string] $ToolsDirectory
)

$ErrorActionPreference = 'Stop'
. "$PSScriptRoot/windows-gpu-process-ownership.ps1"
if (-not $IsWindows) { throw 'Windows GPU native tooling must be provisioned on the owning Windows worker.' }
$root = [IO.Path]::GetFullPath($ToolsDirectory)
Assert-GpuPrivatePath $root (Get-Location).Path
New-Item -ItemType Directory -Path $root -Force | Out-Null
$evidence = [ordered]@{}

function Assert-MicrosoftBinary([string] $Path) {
    Assert-GpuPrivatePath $Path (Get-Location).Path
    $signature = Get-AuthenticodeSignature -LiteralPath $Path
    if ($signature.Status -ne 'Valid' -or $signature.SignerCertificate.Subject -notmatch 'O=Microsoft Corporation(?:,|$)') {
        throw "Refusing non-Microsoft or invalidly signed native debugging tool: $Path"
    }
    return @{
        Path = [IO.Path]::GetFullPath($Path)
        SHA256 = (Get-FileHash -LiteralPath $Path -Algorithm SHA256).Hash
        Publisher = $signature.SignerCertificate.Subject
        Version = (Get-Item -LiteralPath $Path).VersionInfo.FileVersion
    }
}

function Invoke-BoundedTool([string] $Path, [string[]] $Arguments, [string] $Name, [switch] $LicenseHelp) {
    $process = Start-Process -FilePath $Path -ArgumentList $Arguments -PassThru `
        -RedirectStandardOutput (Join-Path $root "$Name.stdout.log") `
        -RedirectStandardError (Join-Path $root "$Name.stderr.log")
    try {
        if (-not $process.WaitForExit(300000)) {
            $process.Kill()
            $process.WaitForExit()
            throw "$Name exceeded its five-minute task-local provisioning bound."
        }
        if ($process.ExitCode -ne 0 -and -not $LicenseHelp) {
            throw "$Name failed with exit code $($process.ExitCode). See task-local logs; no elevation or policy exception was attempted."
        }
    } finally {
        $process.Dispose()
    }
}

$procDump = $env:SKIASHARP_WINDOWS_GPU_PROCDUMP
if (-not $procDump) {
    $procDump = (Get-Command procdump64.exe, procdump.exe -ErrorAction SilentlyContinue |
        Select-Object -First 1).Source
}
if (-not $procDump) {
    $archive = Join-Path $root 'ProcDump-official.zip'
    $supplied = [IO.Path]::GetFullPath('tools/windows-gpu-private/media/ProcDump-official.zip')
    if (Test-Path -LiteralPath $supplied) {
        Copy-Item -LiteralPath $supplied -Destination $archive -Force
    } else {
        Invoke-WebRequest -Uri 'https://download.sysinternals.com/files/Procdump.zip' -OutFile $archive -TimeoutSec 120
    }
    $directory = Join-Path $root 'procdump'
    Expand-Archive -LiteralPath $archive -DestinationPath $directory -Force
    $procDump = Join-Path $directory 'procdump64.exe'
    $evidence.ProcDumpSource = 'https://download.sysinternals.com/files/Procdump.zip'
    $evidence.ProcDumpArchiveSHA256 = (Get-FileHash -LiteralPath $archive -Algorithm SHA256).Hash
}
$evidence.ProcDump = Assert-MicrosoftBinary $procDump
$accepted = Get-ItemPropertyValue -LiteralPath 'HKCU:\Software\Sysinternals\ProcDump' `
    -Name EulaAccepted -ErrorAction SilentlyContinue
if ($accepted -ne 1) {
    # Authorized tool use accepts only its per-user license. Never install a
    # postmortem debugger (-i/-u) or submit dumps to WER (-wer).
    Invoke-BoundedTool $procDump @('-accepteula', '-?') 'procdump-license' -LicenseHelp
    if ((Get-ItemPropertyValue -LiteralPath 'HKCU:\Software\Sysinternals\ProcDump' -Name EulaAccepted) -ne 1) {
        throw 'Official ProcDump did not record the authorized per-user license acceptance.'
    }
    $evidence.ProcDumpLicense = 'Per-user license accepted for this authorized ephemeral task.'
} else {
    $evidence.ProcDumpLicense = 'Existing per-user license reused.'
}

$debugger = $env:SKIASHARP_WINDOWS_GPU_CDB
if (-not $debugger) {
    $debugger = (Get-Command cdb.exe -ErrorAction SilentlyContinue).Source
}
if (-not $debugger) {
    $sdkDebugger = Join-Path ${env:ProgramFiles(x86)} 'Windows Kits/10/Debuggers/x64/cdb.exe'
    if (Test-Path -LiteralPath $sdkDebugger) { $debugger = $sdkDebugger }
}
if (-not $debugger) {
    # This complete debugger-only MSI is from Microsoft's SDK 26100.6584 media.
    # Its size/hash were verified against that official bundle's payload manifest.
    $uri = 'https://download.microsoft.com/download/9d2b0e7e-c04f-402c-8ce6-51f1e56028e8/KIT_BUNDLE_WINDOWSSDK_MEDIACREATION/Installers/X64%20Debuggers%20And%20Tools-x64_en-us.msi'
    $package = Join-Path $root 'X64-Debuggers-And-Tools-26100.msi'
    Invoke-WebRequest -Uri $uri -OutFile $package -TimeoutSec 120
    if ((Get-Item -LiteralPath $package).Length -ne 40779776 -or
        (Get-FileHash -LiteralPath $package -Algorithm SHA1).Hash -ne '8359C23862849E2651626A2EF3DE25D60DF4A6DC') {
        throw 'Official SDK debugger-only media does not match its pinned bundle manifest.'
    }
    $evidence.DebuggerPackage = Assert-MicrosoftBinary $package
    $evidence.DebuggerSource = $uri
    $directory = Join-Path $root 'sdk-debuggers'
    # /a extracts administrative media to the task directory; it does not
    # register/install the SDK, change debugger policy, or request elevation.
    $log = Join-Path $root 'sdk-debuggers-extract.log'
    Invoke-BoundedTool (Join-Path $env:SystemRoot 'System32/msiexec.exe') @(
        '/a', "`"$package`"", '/qn', "TARGETDIR=`"$directory`"", '/l*v', "`"$log`""
    ) 'sdk-debuggers-extract'
    $debugger = (Get-ChildItem -LiteralPath $directory -Filter 'cdb.exe' -Recurse -File |
        Where-Object { $_.DirectoryName -match '(?i)[\\/]x64(?:[\\/]|$)' } |
        Select-Object -First 1).FullName
    if (-not $debugger) { throw 'Official debugger-only media extraction did not produce x64 cdb.exe.' }
}
$evidence.Debugger = Assert-MicrosoftBinary $debugger
$evidence | ConvertTo-Json -Depth 5 | Set-Content -LiteralPath (Join-Path $root 'tools-validation.json')
[pscustomobject]@{ ProcDump = $procDump; Debugger = $debugger }
