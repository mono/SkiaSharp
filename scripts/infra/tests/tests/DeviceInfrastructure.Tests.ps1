$ErrorActionPreference = 'Stop'
Set-StrictMode -Version Latest

$repo = (Resolve-Path (Join-Path $PSScriptRoot '../../../..')).Path
$tests = Join-Path $repo 'scripts/infra/tests'
$root = Join-Path $repo "output/device-routing-fixture-$([Guid]::NewGuid().ToString('N'))"
$keys = @('ANDROID_TEST_RESULTS', 'ANDROID_TEST_DEVICE', 'ANDROID_TEST_DEVICE_VERSION',
    'ANDROID_TEST_SKIN', 'FIXTURE_ARCH', 'FIXTURE_MODE')
$oldEnvironment = @{}
foreach ($key in $keys) {
    $oldEnvironment[$key] = [Environment]::GetEnvironmentVariable($key)
    [Environment]::SetEnvironmentVariable($key, $null)
}

try {
    New-Item $root -ItemType Directory -Force | Out-Null
    $sample = Get-Content (Join-Path $tests 'tests-samples.cake') -Raw
    $cakes = @((Get-Item (Join-Path $repo 'build.cake'))) +
        @(Get-ChildItem $tests -Filter '*.cake')
    foreach ($cake in $cakes) {
        $content = Get-Content $cake.FullName -Raw
        if ($content -match 'test-shared\.cake|tests-apple\.cake|infra/samples/samples\.cake') {
            throw "Stale runner reference: $($cake.Name)"
        }
        foreach ($load in [regex]::Matches($content, '(?m)^#load "([^"]+)"')) {
            if (-not (Test-Path (Join-Path $cake.DirectoryName $load.Groups[1].Value))) {
                throw "Broken Cake load in $($cake.Name): $($load.Value)"
            }
        }
    }
    $build = Get-Content (Join-Path $repo 'build.cake') -Raw
    foreach ($route in @(
        './scripts/infra/tests/tests-ios.cake", "tests-ios',
        './scripts/infra/tests/tests-maccatalyst.cake", "tests-maccatalyst',
        './scripts/infra/tests/tests-samples.cake", "Default',
        './scripts/infra/tests/tests-samples.cake", "samples-generate')) {
        if (-not $build.Contains($route)) { throw "Missing public target route: $route" }
    }
    if ($sample -match 'Setup\(|Teardown\(|tests-shared-(apple|android)|Appium') {
        throw 'The sample runner must not own any device or Appium lifecycle.'
    }
    $cache = Get-Content (Join-Path $repo 'scripts/infra/caching/repo-deps.json') -Raw | ConvertFrom-Json
    $sampleInputs = $cache.jobs.managed.children.package.children.samples.include
    foreach ($include in @('scripts/infra/tests/tests-samples.cake', 'scripts/infra/tests/tests-shared*.cake')) {
        if ($include -notin $sampleInputs) { throw "Missing sample cache input: $include" }
    }
    $stages = Get-Content (Join-Path $repo 'scripts/azure-templates-stages-tests.yml') -Raw
    if (-not $stages.Contains('--iosSimulatorModel="$(TEST_IOS_SIMULATOR_MODEL)" --iosVersion=$(TEST_IOS_SIMULATOR_VERSION)')) {
        throw 'CI must explicitly forward iOS model and version.'
    }

    # Run the actual platform runner statements, replacing only their external boundaries.
    # No build, test host, screenshot, device CLI, or SDK executable can run from these harnesses.
    $stubs = @'
using System.Runtime.InteropServices;
var TARGET = Argument("target", "Default");
var SKIP_BUILD = Argument("skipbuild", false);
var DATE_TIME_STR = "fixture";
var FIXTURE_ARCH = EnvironmentVariable("FIXTURE_ARCH") == "arm64"
    ? System.Runtime.InteropServices.Architecture.Arm64 : System.Runtime.InteropServices.Architecture.X64;
bool FIXTURE_BUILT = false;
void Record(string value) =>
    System.IO.File.AppendAllText(System.IO.Path.Combine(ROOT_PATH.FullPath, "result"), value + "\n");
void FixtureCleanDir(DirectoryPath path) { }
void FixtureTakeSnapshot(DirectoryPath path, string name) => Record("snapshot:" + name);
void FixtureDotNetTool(string arguments)
{
    if (arguments != "android device list")
        throw new Exception("Unexpected real tool boundary: " + arguments);
    Record("physical");
}
void FixtureRunDotNetBuild(FilePath project, string configuration, Dictionary<string, string> properties)
{
    if (configuration != "Release" || properties["TargetFramework"] != "net10.0-android")
        throw new Exception("Android Release pre-build changed.");
    FIXTURE_BUILT = true;
    Record("build");
    if (EnvironmentVariable("FIXTURE_MODE") == "build-failure")
        throw new Exception("Fixture build failure.");
}
void FixtureSetupAndroidEmulator(ref string avd, ref string serial, string image, string model)
{
    if (!SKIP_BUILD && !FIXTURE_BUILT)
        throw new Exception("Emulator started before Release pre-build.");
    avd = "owned-avd";
    serial = "emulator-5554";
    Record("setup:" + image + ":" + model);
}
void FixtureTeardownAndroidEmulator(ref string avd, ref string serial, Exception error)
{
    if (avd != "owned-avd" || serial != "emulator-5554")
        throw new Exception("Cleanup targeted a foreign Android device.");
    Record("cleanup");
    avd = null;
    serial = null;
}
void FixtureSetupIosSimulator(ref string simulator)
{
    simulator = "12345678-1234-1234-1234-123456789abc";
    Record("ios-setup");
}
void FixtureTeardownIosSimulator(ref string simulator, Exception error)
{
    if (simulator == null)
        return;
    if (simulator != "12345678-1234-1234-1234-123456789abc")
        throw new Exception("Cleanup targeted a foreign Apple device.");
    Record("ios-cleanup");
    simulator = null;
}
void FixtureRunDeviceRunnersTest(FilePath project, DirectoryPath output, string configuration,
    string framework, bool noBuild, Dictionary<string, string> properties = null)
{
    var isAndroid = framework == "net10.0-android";
    if (configuration != (isAndroid ? "Release" : "Debug") || noBuild != (isAndroid || SKIP_BUILD))
        throw new Exception("DeviceRunners configuration changed.");
    var device = properties != null && properties.TryGetValue("DeviceRunnersDevice", out var value)
        ? value : "physical-or-host";
    Record("test:" + framework + ":" + device);
    if (EnvironmentVariable("FIXTURE_MODE") == "test-failure")
        throw new Exception("Fixture test failure.");
}
'@
    foreach ($platform in @('android', 'ios', 'maccatalyst')) {
        $source = Get-Content (Join-Path $tests "tests-$platform.cake") -Raw
        $source = [regex]::Replace($source, '(?m)^#load .*\r?\n', '')
        $source = $source.Replace('Directory("../../..")', "Directory(`"$($root.Replace('\', '/'))`")")
        foreach ($boundary in @('CleanDir', 'TakeSnapshot', 'DotNetTool', 'RunDotNetBuild',
                'SetupAndroidEmulator', 'TeardownAndroidEmulator', 'SetupIosSimulator',
                'TeardownIosSimulator', 'RunDeviceRunnersTest')) {
            $source = $source.Replace("$boundary(", "Fixture$boundary(")
        }
        $source = $source.Replace('RuntimeInformation.ProcessArchitecture', 'FIXTURE_ARCH')
        $source = $source.Replace('RunTarget(TARGET);', 'Task("unrelated").Does(() => Record("unrelated")); RunTarget(TARGET);')
        Set-Content (Join-Path $root "$platform.cake") "$stubs`n$source"
    }

    $cases = @(
        @{ Args = @(); Arch = 'x64'; Image = '36;google_apis;x86_64' }
        @{ Args = @(); Arch = 'arm64'; Image = '36;google_apis;arm64-v8a' }
        @{ Args = @('--device=android-emulator-32', '--deviceVersion=latest'); Arch = 'x64'; Image = '36;google_apis;x86' }
        @{ Args = @('--device=android-simulator-64', '--deviceVersion=35', '--skin=pixel_9'); Arch = 'x64'; Image = '35;google_apis;x86_64'; Model = 'pixel_9' }
        @{ Args = @('--device=android-emulator-64_35', '--deviceVersion=latest', '--skipbuild=true'); Arch = 'x64'; Image = '35;google_apis;x86_64'; SkipBuild = $true }
        @{ Args = @('--device=android-device-32'); Arch = 'x64'; Physical = $true }
        @{ Args = @('--device=android-device-64'); Arch = 'arm64'; Physical = $true }
        @{ Args = @(); Arch = 'x64'; Image = '35;google_apis;x86'; Env = $true }
        @{ Args = @(); Arch = 'x64'; Mode = 'test-failure'; Image = '36;google_apis;x86_64' }
        @{ Args = @(); Arch = 'x64'; Mode = 'build-failure' }
    )
    foreach ($case in $cases) {
        $env:FIXTURE_ARCH = $case.Arch
        $env:FIXTURE_MODE = $case['Mode']
        $env:ANDROID_TEST_DEVICE = if ($case['Env']) { 'android-emulator-32' } else { $null }
        $env:ANDROID_TEST_DEVICE_VERSION = if ($case['Env']) { '35' } else { $null }
        $env:ANDROID_TEST_SKIN = if ($case['Env']) { 'Environment model' } else { $null }
        & dotnet cake (Join-Path $root 'android.cake') @($case.Args) --verbosity=quiet
        if (($LASTEXITCODE -ne 0) -ne [bool]$case['Mode']) { throw "Unexpected runner result: $($case.Args)." }
        $result = @(Get-Content (Join-Path $root 'result'))
        if ($result[0] -ne $(if ($case['SkipBuild']) { "setup:system-images;android-$($case.Image):Nexus 5X" } else { 'build' })) {
            throw 'Release pre-build no longer precedes emulator startup.'
        }
        if ($case['Mode'] -eq 'build-failure') {
            if ($result.Count -ne 1) { throw 'Build failure must not start or delete a device.' }
        } elseif ($case['Physical']) {
            if ($result.Count -ne 3 -or $result[1] -ne 'physical' -or
                $result[2] -ne 'test:net10.0-android:physical-or-host') {
                throw 'Physical selection must not create, select an owned serial, screenshot, or delete an emulator.'
            }
        } else {
            $model = if ($case['Model']) { $case['Model'] } elseif ($case['Env']) { 'Environment model' } else { 'Nexus 5X' }
            if ("setup:system-images;android-$($case.Image):$model" -notin $result -or
                'test:net10.0-android:emulator-5554' -notin $result -or $result[-1] -ne 'cleanup') {
                throw "Image/model/serial selection or cleanup changed: $result"
            }
        }
        Remove-Item (Join-Path $root 'result')
    }
    $env:FIXTURE_MODE = $null
    foreach ($platform in @('ios', 'maccatalyst')) {
        $targets = if ($platform -eq 'ios') { @('Default', 'tests-ios', 'unrelated') } else { @('Default', 'tests-maccatalyst') }
        foreach ($target in $targets) {
            & dotnet cake (Join-Path $root "$platform.cake") "--target=$target" --verbosity=quiet
            if ($LASTEXITCODE -ne 0) { throw "Apple routing fixture failed: $platform/$target." }
            $result = @(Get-Content (Join-Path $root 'result'))
            $expected = if ($target -eq 'unrelated') { @('unrelated') } elseif ($platform -eq 'ios') {
                @('ios-setup', 'test:net10.0-ios:12345678-1234-1234-1234-123456789abc', 'ios-cleanup')
            } else { @('test:net10.0-maccatalyst:physical-or-host') }
            if (($result -join '|') -ne ($expected -join '|')) { throw "Apple target routing changed: $result" }
            Remove-Item (Join-Path $root 'result')
        }
    }
    foreach ($cake in @($cakes | Where-Object { $_.Name -notlike 'tests-shared*.cake' })) {
        & dotnet cake $cake.FullName --showdescription --verbosity=quiet
        if ($LASTEXITCODE -ne 0) { throw "Cake description/compilation failed: $($cake.Name)." }
    }
    Write-Host 'Device infrastructure references, selection/routing fixtures, and Cake descriptions passed.'
} finally {
    foreach ($entry in $oldEnvironment.GetEnumerator()) {
        [Environment]::SetEnvironmentVariable($entry.Key, $entry.Value)
    }
    if (Test-Path $root) { Remove-Item -LiteralPath $root -Recurse -Force }
}
