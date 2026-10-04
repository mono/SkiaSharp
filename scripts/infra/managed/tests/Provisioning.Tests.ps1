param(
    [string] $Case = '',
    [string] $JavaHome = '',
    [string] $WorkloadInstaller = (Join-Path $PSScriptRoot '..\install-dotnet-workloads.ps1')
)

$ErrorActionPreference = 'Stop'
$jdkInstaller = Join-Path $PSScriptRoot '..\install-openjdk.ps1'

if ($Case) {
    # Mocks live only in this child process; no SDK, workload, or JDK is installed.
    function global:dotnet {
        Write-Output "MOCK_DOTNET:$((ConvertTo-Json -InputObject @($args) -Compress))"
        $global:LASTEXITCODE = if ($Case -eq 'install-failure') { 23 } else { 0 }
    }
    function global:java {
        Write-Output "MOCK_JAVA:$env:JAVA_HOME"
        $global:LASTEXITCODE = if ($Case -eq 'jdk-failure') { 29 } else { 0 }
    }

    if ($Case.StartsWith('jdk-')) {
        $env:JAVA_HOME_21_X64 = $JavaHome
        $env:JAVA_HOME_17_X64 = $JavaHome
        if ($Case -eq 'jdk-override') {
            $env:JAVA_HOME_21_X64 = ''
            & $jdkInstaller -Version '17.0.8.1' -FolderVersion '17.0.8.1+1'
        } else {
            $env:JAVA_HOME_17_X64 = ''
            & $jdkInstaller
        }
    } else {
        Set-Variable IsLinux ($Case -eq 'default-linux') -Force
        Set-Variable IsMacOS ($Case -eq 'default-macos') -Force
        $workloads = switch ($Case) {
            'single' { 'wasm-tools' }
            'multiple' { ' android, , wasm-tools, maui-android ' }
            default { '' }
        }
        & $WorkloadInstaller -WorkloadSetVersion '11.0.100-rc.1.26458.5' -Workloads $workloads -Tizen ''
    }
    exit $LASTEXITCODE
}

function Assert-Equal {
    param($Actual, $Expected, [string] $Description)
    if ($Actual -cne $Expected) {
        throw "$Description expected '$Expected' but got '$Actual'."
    }
}

function Invoke-Case {
    param([string] $Name, [int] $ExpectedExitCode = 0)

    $output = & (Get-Command pwsh).Source -NoLogo -NoProfile -File $PSCommandPath `
        -Case $Name -JavaHome $JavaHome -WorkloadInstaller $WorkloadInstaller 2>&1
    Assert-Equal $LASTEXITCODE $ExpectedExitCode "$Name exit code"
    return @($output | ForEach-Object { "$_" })
}

$repoRoot = [IO.Path]::GetFullPath((Join-Path $PSScriptRoot '..\..\..\..'))
$testRoot = Join-Path $repoRoot "output\tmp\provisioning-$([Guid]::NewGuid())"
$JavaHome = Join-Path $testRoot 'jdk home'
try {
    New-Item -ItemType Directory (Join-Path $JavaHome 'bin') -Force | Out-Null

    foreach ($path in @($WorkloadInstaller, $jdkInstaller, $PSCommandPath)) {
        $parseErrors = $null
        [Management.Automation.Language.Parser]::ParseFile($path, [ref]$null, [ref]$parseErrors) | Out-Null
        if ($parseErrors) {
            throw "PowerShell syntax errors in ${path}: $parseErrors"
        }
    }

    $pipelineVariables = Get-Content (Join-Path $repoRoot 'scripts\azure-templates-variables.yml')
    $platformPin = @($pipelineVariables | Where-Object { $_ -match '^\s+ANDROID_PLATFORM_VERSIONS:' })
    Assert-Equal $platformPin.Count 1 'Android platform pin count'
    Assert-Equal $platformPin[0].Trim() 'ANDROID_PLATFORM_VERSIONS: 21,35,36,37.0' 'Published Android platform package suffixes'
    Write-Host 'PASS: Android platform package pins'

    $defaults = @{
        'default-windows' = @('android', 'wasm-tools', 'macos', 'ios', 'tvos', 'maccatalyst', 'maui')
        'default-macos' = @('android', 'wasm-tools', 'macos', 'ios', 'tvos', 'maccatalyst', 'maui')
        'default-linux' = @('android', 'wasm-tools', 'maui-android')
        'single' = @('wasm-tools')
        'multiple' = @('android', 'wasm-tools', 'maui-android')
    }
    foreach ($name in @('default-windows', 'default-macos', 'default-linux', 'single', 'multiple', 'install-failure')) {
        $exitCode = if ($name -eq 'install-failure') { 23 } else { 0 }
        $output = Invoke-Case $name $exitCode
        $calls = @($output | Where-Object { $_.StartsWith('MOCK_DOTNET:') } |
            ForEach-Object { ConvertFrom-Json $_.Substring('MOCK_DOTNET:'.Length) -NoEnumerate })
        $expectedWorkloads = if ($name -eq 'install-failure') { $defaults['default-windows'] } else { $defaults[$name] }
        $expectedArgs = @('workload', 'install') + $expectedWorkloads +
            @('--skip-sign-check', '--version', '11.0.100-rc.1.26458.5')
        Assert-Equal ($calls[0] -join '|') ($expectedArgs -join '|') "$name install arguments"
        if ($exitCode -eq 0) {
            Assert-Equal $calls.Count 2 "$name call count"
            Assert-Equal ($calls[1] -join '|') 'workload|list' "$name list arguments"
        } else {
            Assert-Equal $calls.Count 1 "$name stops after install failure"
        }
        Write-Host "PASS: $name"
    }

    $jdkAst = [Management.Automation.Language.Parser]::ParseFile($jdkInstaller, [ref]$null, [ref]$null)
    Assert-Equal $jdkAst.ParamBlock.Parameters[0].DefaultValue.SafeGetValue() '21.0.10' 'JDK version'
    Assert-Equal $jdkAst.ParamBlock.Parameters[1].DefaultValue.SafeGetValue() '21.0.10+7' 'JDK folder version'
    foreach ($name in @('jdk-default', 'jdk-override', 'jdk-failure')) {
        $exitCode = if ($name -eq 'jdk-failure') { 29 } else { 0 }
        $output = Invoke-Case $name $exitCode
        Assert-Equal @($output | Where-Object { $_ -eq "MOCK_JAVA:$JavaHome" }).Count 1 "$name reused home"
        Assert-Equal @($output | Where-Object { $_ -eq "##vso[task.setvariable variable=JAVA_HOME;]$JavaHome" }).Count 1 "$name JAVA_HOME variable"
        Assert-Equal @($output | Where-Object { $_.StartsWith('Downloading OpenJDK') }).Count 0 "$name no download"
        Write-Host "PASS: $name"
    }
    Write-Host 'All provisioning tests passed (9 cases and PowerShell syntax).'
} finally {
    if (Test-Path $testRoot) {
        Remove-Item -LiteralPath $testRoot -Recurse -Force
    }
}
