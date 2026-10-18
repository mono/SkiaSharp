$ErrorActionPreference = 'Stop'
Set-StrictMode -Version Latest

if (-not ($IsMacOS -or $IsLinux)) {
    throw 'Apple lifecycle fixtures require a Unix host; they never start a simulator.'
}

$repo = (Resolve-Path (Join-Path $PSScriptRoot '../../../..')).Path
$root = Join-Path $repo "output/apple-lifecycle-fixture-$([Guid]::NewGuid().ToString('N'))"
$keys = @('FIXTURE_APPLE_ROOT', 'FIXTURE_APPLE_MODE', 'TEST_IOS_SIMULATOR_MODEL',
    'TEST_IOS_SIMULATOR_VERSION', 'IOS_SIMULATOR_NAME')
$oldEnvironment = @{}
foreach ($key in $keys) { $oldEnvironment[$key] = [Environment]::GetEnvironmentVariable($key) }

try {
    New-Item $root -ItemType Directory -Force | Out-Null
    $fake = Join-Path $root 'apple-fixture'
    Set-Content $fake @'
#!/bin/sh
[ "$1" = "apple" ] && [ "$2" = "simulator" ] || exit 90
printf '%s\n' "$*" >> "$FIXTURE_APPLE_ROOT/commands"
case "$3" in
    create)
        [ "$NO_COLOR" = 1 ] && [ "$SPECTRE_ANSI" = 0 ] &&
            [ "$DOTNET_SYSTEM_CONSOLE_ALLOW_ANSI_COLOR_REDIRECTION" = 0 ] || exit 91
        case "$FIXTURE_APPLE_MODE" in
            create-failure) exit 2 ;;
            non-json) echo 'Creation failed'; exit 0 ;;
            invalid-json) echo '{'; exit 0 ;;
            invalid-uuid) echo '{"udid":"foreign"}'; exit 0 ;;
            missing-metadata) echo '{"udid":"12345678-1234-1234-1234-123456789abc"}'; exit 0 ;;
            invalid-version) version=invalid ;;
            *) version=26.3.1 ;;
        esac
        printf '{"udid":"12345678-1234-1234-1234-123456789abc","runtime":{"version":"%s"}}\n' "$version"
        ;;
    boot)
        case "$FIXTURE_APPLE_MODE" in boot-failure|boot-delete-failure) exit 3 ;; esac
        ;;
    shutdown)
        [ "$FIXTURE_APPLE_MODE" != shutdown-failure ] || exit 4
        ;;
    delete)
        case "$FIXTURE_APPLE_MODE" in delete-failure|boot-delete-failure) exit 5 ;; esac
        ;;
    *) exit 92 ;;
esac
exit 0
'@
    & chmod +x $fake
    if ($LASTEXITCODE -ne 0) { throw 'Could not make Apple fixture executable.' }

    # Replace only external boundaries in a fixture copy, never production files.
    $helper = Join-Path $root 'apple-helper.cake'
    $source = Get-Content (Join-Path $PSScriptRoot '../tests-shared-apple.cake') -Raw
    $source = $source.Replace('StartProcess(', 'FixtureStartProcess(').
        Replace('DotNetTool(', 'FixtureDotNetTool(').
        Replace('IsRunningOnMacOs()', 'FixtureIsRunningOnMacOs()')
    Set-Content $helper $source
    $harness = Join-Path $root 'lifecycle.cake'
    Set-Content $harness @"
#load "$helper"
// Intercept every subprocess entry point; no PATH lookup or real Apple tool is possible.
int FixtureStartProcess(string executable, ProcessSettings settings, out IEnumerable<string> stdout)
{
    if (executable != "dotnet")
        throw new Exception("Unexpected fixture executable.");
    return StartProcess("$fake", settings, out stdout);
}
void FixtureDotNetTool(string arguments)
{
    var exit = StartProcess("$fake",
        new ProcessSettings { Arguments = arguments });
    if (exit != 0)
        throw new Exception(arguments + " failed: " + exit);
}
bool FixtureIsRunningOnMacOs() => EnvironmentVariable("FIXTURE_APPLE_MODE") != "non-mac";
Task("Default").Does(() => {
    var initialVersion = TEST_IOS_SIMULATOR_VERSION;
    var cases = new[] {
        "success", "create-failure", "non-json", "invalid-json", "invalid-uuid",
        "missing-metadata", "invalid-version", "boot-failure", "shutdown-failure",
        "delete-failure", "boot-delete-failure", "non-mac",
    };
    foreach (var mode in cases) {
        System.Environment.SetEnvironmentVariable("FIXTURE_APPLE_MODE", mode);
        TEST_IOS_SIMULATOR_VERSION = initialVersion;
        string simulator = null;
        Exception failure = null;
        try {
            SetupIosSimulator(ref simulator);
            TeardownIosSimulator(ref simulator, null);
        } catch (Exception error) {
            failure = error;
        }
        var expectFailure = mode != "success" && mode != "non-mac";
        if ((failure != null) != expectFailure)
            throw new Exception("Unexpected fixture outcome: " + mode, failure);
        if (mode.Contains("delete-failure")) {
            if (simulator == null)
                throw new Exception("Failed deletion lost ownership.");
            if (mode == "boot-delete-failure" &&
                !failure.ToString().Contains("simulator boot"))
                throw new Exception("Cleanup lost the original boot failure.");
            System.Environment.SetEnvironmentVariable("FIXTURE_APPLE_MODE", "success");
            TeardownIosSimulator(ref simulator, null);
        }
        if (simulator != null)
            throw new Exception("Owned simulator remains: " + mode);
    }
});
RunTarget("Default");
"@
    $env:FIXTURE_APPLE_ROOT = $root
    $env:TEST_IOS_SIMULATOR_VERSION = '26.3.1'

    $cases = @(
        @{ Args = @(); Model = 'iPhone 16'; EnvModel = $null; Legacy = $null }
        @{ Args = @('--iosSimulator=Legacy CLI'); Model = 'Legacy CLI'; EnvModel = $null; Legacy = 'Legacy environment' }
        @{ Args = @(); Model = 'Legacy environment'; EnvModel = $null; Legacy = 'Legacy environment' }
        @{ Args = @('--iosSimulator=Legacy CLI'); Model = 'New environment'; EnvModel = 'New environment'; Legacy = $null }
        @{ Args = @('--iosSimulatorModel=New CLI'); Model = 'New CLI'; EnvModel = 'New environment'; Legacy = $null }
    )
    foreach ($case in $cases) {
        $env:TEST_IOS_SIMULATOR_MODEL = $case.EnvModel
        $env:IOS_SIMULATOR_NAME = $case.Legacy
        & dotnet cake $harness @($case.Args) --verbosity=quiet
        if ($LASTEXITCODE -ne 0) { throw "Apple lifecycle fixture failed: $($case.Model)." }
        $commands = @(Get-Content (Join-Path $root 'commands'))
        $creates = @($commands | Where-Object { $_ -match '^apple simulator create ' })
        if ($creates.Count -ne 11) { throw 'Unexpected creation count (including non-mac host guard).' }
        foreach ($create in $creates) {
            if (-not $create.Contains("--device-type $($case.Model) --runtime com.apple.CoreSimulator.SimRuntime.iOS-26-3 --format json")) {
                throw "Model/runtime selection changed: $create"
            }
        }
        $shutdowns = @($commands | Where-Object { $_ -match '^apple simulator shutdown ' })
        $deletes = @($commands | Where-Object { $_ -match '^apple simulator delete ' })
        if ($shutdowns.Count -ne 9 -or $deletes.Count -ne 9) {
            throw 'Metadata/boot/shutdown/deletion failure cleanup did not run.'
        }
        foreach ($command in @($shutdowns) + @($deletes)) {
            if ($command -notmatch '^apple simulator (shutdown|delete) "?(12345678-1234-1234-1234-123456789abc)"?$') {
                throw "Cleanup did not target the exact owned UUID: $command"
            }
        }
        Remove-Item (Join-Path $root 'commands')
    }

    # Latest and omitted versions must not request a runtime; CLI version overrides environment.
    foreach ($version in @('', 'latest', '26.2')) {
        $env:TEST_IOS_SIMULATOR_VERSION = $version
        $argsForVersion = @()
        if ($version -eq '26.2') { $argsForVersion = @('--iosVersion=latest') }
        & dotnet cake $harness @argsForVersion --verbosity=quiet
        if ($LASTEXITCODE -ne 0) { throw "Apple latest-runtime fixture failed: $version." }
        if ((Get-Content (Join-Path $root 'commands') -Raw).Contains('--runtime')) {
            throw 'Latest/omitted version must select the latest installed runtime.'
        }
        Remove-Item (Join-Path $root 'commands')
    }
    Write-Host 'Apple lifecycle fixtures passed (fake tools only).'
} finally {
    foreach ($entry in $oldEnvironment.GetEnumerator()) {
        [Environment]::SetEnvironmentVariable($entry.Key, $entry.Value)
    }
    if (Test-Path $root) { Remove-Item -LiteralPath $root -Recurse -Force }
}
