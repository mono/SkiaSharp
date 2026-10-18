$ErrorActionPreference = 'Stop'
Set-StrictMode -Version Latest

if (-not ($IsMacOS -or $IsLinux)) {
    throw 'Android emulator lifecycle fixtures require a Unix host; they never start a device.'
}

$repo = (Resolve-Path (Join-Path $PSScriptRoot '../../../..')).Path
$root = Join-Path $repo "output/android-lifecycle-fixture-$([Guid]::NewGuid().ToString('N'))"
$oldEnvironment = @{}
$keys = @('ANDROID_HOME', 'ANDROID_SDK_MANAGER_PATH', 'FIXTURE_ANDROID_ROOT',
        'FIXTURE_ANDROID_MODE', 'FIXTURE_ARCH', 'FIXTURE_CONFIG_EXPECTED', 'FIXTURE_CONFIG_ERROR', 'FIXTURE_PROFILE_EXPECTED',
        'TEST_ANDROID_VERSION', 'TEST_ANDROID_IMAGE', 'TEST_ANDROID_MODEL', 'TEST_ANDROID_UDID',
        'ANDROID_TEST_DEVICE', 'ANDROID_TEST_DEVICE_VERSION', 'ANDROID_TEST_SKIN', 'ANDROID_TEST_RESULTS') +
    @(Get-ChildItem Env: | Where-Object Name -Match '^(TEST_ANDROID_|ANDROID_TEST_)' | Select-Object -ExpandProperty Name)
foreach ($key in @($keys | Sort-Object -Unique)) {
    $oldEnvironment[$key] = [Environment]::GetEnvironmentVariable($key)
    Remove-Item -LiteralPath "Env:$key" -ErrorAction SilentlyContinue
}

try {
    $bin = Join-Path $root 'sdk/cmdline-tools/selected/bin'
    New-Item $bin, (Join-Path $root 'sdk/platform-tools'), (Join-Path $root 'sdk/emulator') `
        -ItemType Directory -Force | Out-Null
    foreach ($imagePath in @('android-36/google_apis/x86', 'android-36/google_apis/x86_64',
            'android-36/google_apis/arm64-v8a', 'android-26/google_apis/x86_64',
            'android-37.1/google_apis_ps16k/arm64-v8a')) {
        $image = Join-Path $root "sdk/system-images/$imagePath"
        New-Item $image -ItemType Directory -Force | Out-Null
        Set-Content (Join-Path $image 'system.img') 'fixture'
    }
    Set-Content (Join-Path $bin 'sdkmanager') 'fixture; never executed'
    Set-Content (Join-Path $bin 'avdmanager') @'
#!/bin/sh
command="$1"
if [ "$command" = "list" ]; then
    printf 'list\n' >> "$FIXTURE_ANDROID_ROOT/commands"
    [ "$FIXTURE_ANDROID_MODE" != create-list-failure ] || exit 10
    echo 'Available Android Virtual Devices:'
    printf '    Name: %s-unrelated\n' "$(cat "$FIXTURE_ANDROID_ROOT/name")"
    if [ -d "$FIXTURE_ANDROID_ROOT/$(cat "$FIXTURE_ANDROID_ROOT/name")" ]; then
        echo 'The following Android Virtual Devices could not be loaded:'
        printf '    Name: %s\n' "$(cat "$FIXTURE_ANDROID_ROOT/name")"
    fi
    exit 0
fi
printf '%s\n' "$@" >> "$FIXTURE_ANDROID_ROOT/avd-arguments"
while [ "$#" -gt 0 ]; do
    if [ "$1" = "--name" ]; then shift; name="$1"; fi
    shift
done
printf '%s %s\n' "$command" "$name" >> "$FIXTURE_ANDROID_ROOT/commands"
if [ "$command" = "create" ]; then
    read answer
    [ "$answer" = "no" ] || exit 3
    printf '%s' "$name" > "$FIXTURE_ANDROID_ROOT/name"
    [ "$FIXTURE_ANDROID_MODE" != create-no-avd ] || exit 6
    mkdir "$FIXTURE_ANDROID_ROOT/$name" || exit 4
    case "$FIXTURE_ANDROID_MODE" in create-failure|create-list-failure) exit 6 ;; esac
else
    if [ -f "$FIXTURE_ANDROID_ROOT/pid" ]; then
        # The helper must reap its owned process before deleting this AVD.
        ! kill -0 "$(cat "$FIXTURE_ANDROID_ROOT/pid")" 2>/dev/null || exit 7
    fi
    [ "$FIXTURE_ANDROID_MODE" != delete-failure ] || exit 8
    rmdir "$FIXTURE_ANDROID_ROOT/$name" || exit 5
fi
'@
    Set-Content (Join-Path $root 'sdk/emulator/emulator') @'
#!/bin/sh
printf '%s' "$$" > "$FIXTURE_ANDROID_ROOT/pid"
printf '%s\n' "$@" > "$FIXTURE_ANDROID_ROOT/emulator-arguments"
echo 'owned emulator boot output'
echo 'owned emulator boot error' >&2
[ "$FIXTURE_ANDROID_MODE" != boot-failure ] || exit 9
while :; do sleep 1; done
'@
    Set-Content (Join-Path $root 'sdk/platform-tools/adb') @'
#!/bin/sh
printf '%s\n' "$*" >> "$FIXTURE_ANDROID_ROOT/adb-arguments"
while [ ! -f "$FIXTURE_ANDROID_ROOT/pid" ]; do sleep 0.01; done
if [ "$3" = "shell" ] && [ "$4" = "getprop" ]; then
    [ "$FIXTURE_ANDROID_MODE" != boot-failure ] || exit 1
    echo 1
elif [ "$3" = "emu" ]; then
    if [ "$FIXTURE_ANDROID_MODE" = identity-failure ]; then echo foreign-avd; else cat "$FIXTURE_ANDROID_ROOT/name"; fi
elif [ "$4" = "settings" ]; then
    [ "$FIXTURE_ANDROID_MODE" != animation-failure ] || exit 2
fi
'@
    & chmod +x (Join-Path $bin 'avdmanager') (Join-Path $root 'sdk/emulator/emulator') (Join-Path $root 'sdk/platform-tools/adb')
    if ($LASTEXITCODE -ne 0) { throw 'Could not make lifecycle fixtures executable.' }

    $helper = (Join-Path $root 'android-helper.cake').Replace('\', '/')
    $source = Get-Content (Join-Path $PSScriptRoot '../tests-shared-android.cake') -Raw
    $source = $source.Replace('System.Runtime.InteropServices.RuntimeInformation.ProcessArchitecture',
        'System.Enum.Parse<System.Runtime.InteropServices.Architecture>(EnvironmentVariable("FIXTURE_ARCH"))')
    Set-Content $helper $source
    $harness = Join-Path $root 'lifecycle.cake'
    Set-Content $harness @"
#load "$helper"
DirectoryPath ROOT_PATH = MakeAbsolute(Directory("$($root.Replace('\', '/'))"));
Task("Default").Does(() => {
    var windowsCommand = AndroidAvdWindowsArguments(
        @"C:\Program Files (x86)\Android\cmdline-tools\bin\avdmanager.bat",
        "create", "avd", "--device", "Nexus 5X",
        "--package", "system-images;android-36;google_apis;x86_64");
    var expectedCommand = "/d /v:off /s /c \"\"C:\\Program Files (x86)\\Android\\cmdline-tools\\bin\\avdmanager.bat\" " +
        "\"create\" \"avd\" \"--device\" \"Nexus 5X\" \"--package\" \"system-images;android-36;google_apis;x86_64\"\"";
    if (windowsCommand != expectedCommand)
        throw new Exception("Windows batch invocation lost its outer command quotes: " + windowsCommand);
    foreach (var invalid in new[] { "bad\"model", "%EXPANDED%", "bad\nmodel" }) {
        try {
            AndroidAvdWindowsArguments("avdmanager.bat", invalid);
            throw new Exception("Windows batch invocation accepted a non-literal argument.");
        } catch (ArgumentException) { }
    }
    string avd = null, serial = null;
    Exception failure = null;
    var mode = EnvironmentVariable("FIXTURE_ANDROID_MODE");
    try {
        SetupAndroidEmulator(ref avd, ref serial);
        if (mode == "success") {
            if (!avd.StartsWith("SkiaSharp-Tests-") || !serial.StartsWith("emulator-"))
                throw new Exception("Missing exact owned identity.");
            try {
                SetupAndroidEmulator(ref avd, ref serial);
                throw new Exception("Duplicate setup unexpectedly succeeded.");
            } catch (InvalidOperationException) { }
        }
    } catch (Exception error) {
        failure = error;
    } finally {
        try {
            TeardownAndroidEmulator(ref avd, ref serial, failure);
        } catch (Exception error) {
            failure = error;
        }
    }
    if ((failure != null) != (mode != "success"))
        throw new Exception("Unexpected lifecycle result: " + mode, failure);
    if (mode == "create-no-avd" &&
        (failure is AggregateException || !failure.Message.Contains("exit code 6")))
        throw new Exception("Rejected creation was obscured by cleanup of a nonexistent AVD.", failure);
    if (mode == "create-list-failure" &&
        (!(failure is AggregateException) || !failure.ToString().Contains("exit code 6") ||
            !failure.ToString().Contains("exit code 10")))
        throw new Exception("AVD discovery failure lost the original creation error.", failure);
    if (mode == "delete-failure" || mode == "create-list-failure") {
        if (avd == null)
            throw new Exception("Failed deletion lost AVD ownership.");
        System.Environment.SetEnvironmentVariable("FIXTURE_ANDROID_MODE", "success");
        TeardownAndroidEmulator(ref avd, ref serial, null);
    }
    if (avd != null || serial != null || ANDROID_EMULATOR_PROCESS != null)
        throw new Exception("Owned resources remain after cleanup.");
});
Task("Unused").Does(() => { });
Task("Configuration").Does(() => {
    var expectError = EnvironmentVariable("FIXTURE_CONFIG_ERROR") == "true";
    try {
        var before = (TEST_ANDROID_VERSION, TEST_ANDROID_IMAGE, TEST_ANDROID_MODEL);
        var config = GetAndroidEmulatorConfiguration();
        if (expectError)
            throw new Exception("Invalid Android configuration unexpectedly succeeded.");
        var actual = string.Join("|", config.UsingEmulator, config.SystemImage, config.Model, config.Architecture);
        if (actual != EnvironmentVariable("FIXTURE_CONFIG_EXPECTED"))
            throw new Exception("Unexpected selected configuration: " + actual);
        var expectedProfile = EnvironmentVariable("FIXTURE_PROFILE_EXPECTED");
        if (expectedProfile != null &&
            string.Join("|", TEST_ANDROID_VERSION, TEST_ANDROID_IMAGE, TEST_ANDROID_MODEL) != expectedProfile)
            throw new Exception("OS version/image/model inputs lost their values or precedence.");
        if (config != GetAndroidEmulatorConfiguration() ||
            before != (TEST_ANDROID_VERSION, TEST_ANDROID_IMAGE, TEST_ANDROID_MODEL))
            throw new Exception("Selection mutated profile inputs or changed on a second call.");
        if (!config.UsingEmulator) {
            string avd = null, serial = null;
            SetupAndroidEmulator(ref avd, ref serial);
            if (avd != null || serial != null || ANDROID_AVD_MANAGER != null || ANDROID_ADB != null ||
                ANDROID_EMULATOR_PROCESS != null)
                throw new Exception("Physical selection acquired emulator resources or tools.");
        }
    } catch (ArgumentException) {
        if (!expectError)
            throw;
    }
});
Task("PureConfiguration").Does(() => {
    var x64 = System.Runtime.InteropServices.Architecture.X64;
    var arm64 = System.Runtime.InteropServices.Architecture.Arm64;
    var x86 = System.Runtime.InteropServices.Architecture.X86;
    var cases = new[] {
        ("android-emulator-32", "36", x64, true, "x86"),
        ("android-emulator-32", "36", arm64, true, "x86"),
        ("android-emulator-64", "36", x64, true, "x86_64"),
        ("android-simulator-64", "36", arm64, true, "arm64-v8a"),
        ("android-device-32", "36", x64, false, "armeabi-v7a"),
        ("android-device-32", "36", arm64, false, "armeabi-v7a"),
        ("android-device-64", "36", x64, false, "arm64-v8a"),
        ("android-device-64", "36", arm64, false, "arm64-v8a"),
    };
    foreach (var (device, version, host, emulator, abi) in cases) {
        var config = SelectAndroidEmulatorConfiguration(device, version, "Nexus 5X", "", "", "", host);
        if (config.UsingEmulator != emulator || config.Architecture != abi ||
            config.SystemImage != (emulator ? "system-images;android-36;google_apis;" + abi : null) ||
            config.Model != (emulator ? "Nexus 5X" : null))
            throw new Exception("Incorrect legacy mapping: " + device + "/" + host);
    }
    foreach (var version in new[] { "", "latest" }) {
        foreach (var device in new[] { "android-emulator-64", "android-emulator-64_35" }) {
            var config = SelectAndroidEmulatorConfiguration(device, version, "Nexus 5X", "", "", "", x64);
            if (!config.SystemImage.Contains(device.EndsWith("_35") ? "android-35;" : "android-36;"))
                throw new Exception("Empty/latest or embedded API compatibility changed.");
        }
    }
    foreach (var device in new[] { "", "android", "ios-emulator-64", "android-unknown-64",
            "android-emulator-16", "android-emulator-64-extra", "android-emulator-64_bad",
            "android-emulator-64_35_36", "android-emulator-64_0", "android-emulator-64_999999999999" }) {
        try {
            SelectAndroidEmulatorConfiguration(device, "latest", "Nexus 5X", "", "", "", x64);
            throw new Exception("Invalid device accepted: " + device);
        } catch (ArgumentException) { }
    }
    foreach (var version in new[] { "0", "-1", "8.0", "garbage", "999999999999" }) {
        try {
            SelectAndroidEmulatorConfiguration("android-emulator-64", version, "Nexus 5X", "", "", "", x64);
            throw new Exception("Invalid legacy API accepted: " + version);
        } catch (ArgumentException) { }
    }
    foreach (var skin in new[] { "", " ", "bad\nmodel" }) {
        try {
            SelectAndroidEmulatorConfiguration("android-emulator-64", "36", skin, "", "", "", x64);
            throw new Exception("Invalid legacy model accepted.");
        } catch (ArgumentException) { }
    }
    foreach (var profile in new[] {
        ("8.0", "", ""), ("", "android-26;google_apis", ""), ("", "", "pixel_9"),
        ("8.0", "android-26;google_apis", ""), ("8.0", "", "pixel_9"),
        ("", "android-26;google_apis", "pixel_9"), ("latest", "android-26;google_apis", "pixel_9"),
        ("17", "system-images;android-37.1;google_apis_ps16k", "pixel_9"),
        ("17", "android-37.1;google_apis_ps16k;arm64-v8a", "pixel_9"),
        ("17", "android-37.1;bad-vendor", "pixel_9"), ("17", "android-37.1;google_apis_ps16k", "Nexus 5X"),
        ("17", "android-37.1;google_apis_ps16k", "pixel/9"), (" ", "android-26;google_apis", "pixel_9"),
        ("8.0\n", "android-26;google_apis", "pixel_9"), ("8.0", "android-26;google_apis\n", "pixel_9"),
        ("8.0", "android-26;google_apis", "pixel_9\n"),
    }) {
        try {
            SelectAndroidEmulatorConfiguration("android-emulator-64", "36", "Nexus 5X",
                profile.Item1, profile.Item2, profile.Item3, x64);
            throw new Exception("Partial/invalid explicit profile accepted: " + profile);
        } catch (ArgumentException) { }
    }
    foreach (var selection in new[] { ("android-emulator-64", x86), ("android-device-32", x64),
            ("android-device-64", arm64) }) {
        try {
            SelectAndroidEmulatorConfiguration(selection.Item1, "36", "Nexus 5X",
                "8.0", "android-26;google_apis", "pixel_9", selection.Item2);
            throw new Exception("Unsupported host or physical explicit profile accepted.");
        } catch (ArgumentException) { }
    }
});
RunTarget(Argument("target", "Default"));
"@
    $env:ANDROID_HOME = Join-Path $root 'sdk'
    $env:ANDROID_SDK_MANAGER_PATH = Join-Path $bin 'sdkmanager'
    $env:FIXTURE_ANDROID_ROOT = $root
    $env:FIXTURE_ARCH = 'X64'
    & dotnet cake $harness --target=PureConfiguration --verbosity=quiet
    if ($LASTEXITCODE -ne 0) { throw 'Pure Android selection fixtures failed.' }

    $configCases = @(
        @{ Args = @(); Expected = 'True|system-images;android-36;google_apis;x86_64|Nexus 5X|x86_64' }
        @{ Args = @(); Env = @{ ANDROID_TEST_DEVICE_VERSION = '' }; Expected = 'True|system-images;android-36;google_apis;x86_64|Nexus 5X|x86_64' }
        @{ Args = @('--device=android-emulator-64_35'); Env = @{ ANDROID_TEST_DEVICE_VERSION = '' }; Expected = 'True|system-images;android-35;google_apis;x86_64|Nexus 5X|x86_64' }
        @{ Args = @(); Env = @{ ANDROID_TEST_DEVICE = 'android-emulator-32'; ANDROID_TEST_DEVICE_VERSION = '35'; ANDROID_TEST_SKIN = 'Environment model' }
            Expected = 'True|system-images;android-35;google_apis;x86|Environment model|x86' }
        @{ Args = @('--device=android-simulator-64', '--deviceVersion=36', '--skin=Nexus 5X')
            Env = @{ ANDROID_TEST_DEVICE = 'android-device-32'; ANDROID_TEST_DEVICE_VERSION = '35'; ANDROID_TEST_SKIN = 'Environment model' }
            Expected = 'True|system-images;android-36;google_apis;x86_64|Nexus 5X|x86_64' }
        @{ Args = @('--device=android-device-32'); Expected = 'False|||armeabi-v7a' }
        @{ Args = @('--device=android-device-64'); Expected = 'False|||arm64-v8a' }
        @{ Args = @('--androidVersion=8.0', '--androidImage=android-26;google_apis', '--androidModel=pixel_9')
            Expected = 'True|system-images;android-26;google_apis;x86_64|pixel_9|x86_64'; Profile = '8.0|android-26;google_apis|pixel_9' }
        @{ Args = @(); Arch = 'Arm64'
            Env = @{ TEST_ANDROID_VERSION = '17'; TEST_ANDROID_IMAGE = 'android-37.1;google_apis_ps16k'; TEST_ANDROID_MODEL = 'pixel_9' }
            Expected = 'True|system-images;android-37.1;google_apis_ps16k;arm64-v8a|pixel_9|arm64-v8a'; Profile = '17|android-37.1;google_apis_ps16k|pixel_9' }
        @{ Args = @('--androidVersion=8.0', '--androidImage=android-26;google_apis', '--androidModel=pixel_9')
            Env = @{ TEST_ANDROID_VERSION = '17'; TEST_ANDROID_IMAGE = 'android-37.1;google_apis_ps16k'; TEST_ANDROID_MODEL = 'other_model' }
            Expected = 'True|system-images;android-26;google_apis;x86_64|pixel_9|x86_64'; Profile = '8.0|android-26;google_apis|pixel_9' }
        @{ Args = @('--androidVersion=8.0'); Env = @{ TEST_ANDROID_VERSION = '17'; TEST_ANDROID_IMAGE = 'android-26;google_apis'; TEST_ANDROID_MODEL = 'pixel_9' }
            Expected = 'True|system-images;android-26;google_apis;x86_64|pixel_9|x86_64'; Profile = '8.0|android-26;google_apis|pixel_9' }
        @{ Args = @('--androidModel=Nexus 5X'); Env = @{ TEST_ANDROID_VERSION = '17'; TEST_ANDROID_IMAGE = 'android-37.1;google_apis_ps16k'; TEST_ANDROID_MODEL = 'pixel_9' }; Error = $true }
        @{ Args = @('--androidVersion=8.0'); Error = $true }
        @{ Args = @('--device=android-device-64', '--androidVersion=8.0', '--androidImage=android-26;google_apis', '--androidModel=pixel_9'); Error = $true }
        @{ Args = @('--androidVersion=8.0'); Target = 'Unused' }
    )
    $selectionKeys = @('TEST_ANDROID_VERSION', 'TEST_ANDROID_IMAGE', 'TEST_ANDROID_MODEL',
        'ANDROID_TEST_DEVICE', 'ANDROID_TEST_DEVICE_VERSION', 'ANDROID_TEST_SKIN')
    # No valid SDK exists during configuration/physical checks, so accidental lifecycle work fails.
    $env:ANDROID_HOME = Join-Path $root 'no-sdk'
    $env:ANDROID_SDK_MANAGER_PATH = Join-Path $root 'no-sdk/sdkmanager'
    foreach ($case in $configCases) {
        foreach ($key in $selectionKeys) { Remove-Item -LiteralPath "Env:$key" -ErrorAction SilentlyContinue }
        if ($case['Env']) {
            foreach ($entry in $case.Env.GetEnumerator()) { [Environment]::SetEnvironmentVariable($entry.Key, $entry.Value) }
        }
        $env:FIXTURE_ARCH = if ($case['Arch']) { $case.Arch } else { 'X64' }
        $env:FIXTURE_CONFIG_EXPECTED = $case['Expected']
        $env:FIXTURE_PROFILE_EXPECTED = $case['Profile']
        $env:FIXTURE_CONFIG_ERROR = if ($case['Error']) { 'true' } else { 'false' }
        $target = if ($case['Target']) { $case.Target } else { 'Configuration' }
        & dotnet cake $harness "--target=$target" @($case.Args) --verbosity=quiet
        if ($LASTEXITCODE -ne 0) { throw "Android input selection failed: $($case.Args)." }
        if (Test-Path (Join-Path $root 'commands')) { throw 'Configuration/physical selection ran SDK tools.' }
    }
    foreach ($key in $selectionKeys) { Remove-Item -LiteralPath "Env:$key" -ErrorAction SilentlyContinue }
    $env:ANDROID_HOME = Join-Path $root 'sdk'

    $cases = @(
        @{ Mode = 'success'; Abi = 'x86'; Device = 'android-emulator-32' }
        @{ Mode = 'success'; Abi = 'x86_64'; Arch = 'X64' }
        @{ Mode = 'success'; Abi = 'arm64-v8a' }
        @{ Mode = 'success'; Abi = 'x86_64'; Arch = 'X64'; Version = '8.0'; Image = 'android-26;google_apis'; Model = 'pixel_9' }
        @{ Mode = 'success'; Abi = 'arm64-v8a'; Version = '17'; Image = 'android-37.1;google_apis_ps16k'; Model = 'pixel_9' }
        @{ Mode = 'identity-failure'; Abi = 'arm64-v8a' }
        @{ Mode = 'create-no-avd'; Abi = 'arm64-v8a' }
        @{ Mode = 'create-failure'; Abi = 'arm64-v8a' }
        @{ Mode = 'create-list-failure'; Abi = 'arm64-v8a' }
        @{ Mode = 'boot-failure'; Abi = 'arm64-v8a' }
        @{ Mode = 'animation-failure'; Abi = 'arm64-v8a' }
        @{ Mode = 'delete-failure'; Abi = 'arm64-v8a' }
        @{ Mode = 'missing-image'; Abi = 'arm64-v8a' }
        @{ Mode = 'missing-tools'; Abi = 'arm64-v8a' }
    )
    foreach ($case in $cases) {
        $env:FIXTURE_ANDROID_MODE = $case.Mode
        $env:FIXTURE_ARCH = if ($case['Arch']) { $case.Arch } else { 'Arm64' }
        $env:ANDROID_TEST_DEVICE = if ($case['Device']) { $case.Device } else { 'android-emulator-64' }
        $env:ANDROID_TEST_DEVICE_VERSION = if ($case.Mode -eq 'missing-image') { '99' } else { '36' }
        $env:TEST_ANDROID_VERSION = $case['Version']
        $env:TEST_ANDROID_IMAGE = $case['Image']
        $env:TEST_ANDROID_MODEL = $case['Model']
        $expectedImage = if ($case['Image']) { "system-images;$($case.Image);$($case.Abi)" } else {
            "system-images;android-$($env:ANDROID_TEST_DEVICE_VERSION);google_apis;$($case.Abi)"
        }
        $expectedModel = if ($case['Model']) { $case.Model } else { 'Nexus 5X' }
        $env:ANDROID_SDK_MANAGER_PATH = if ($case.Mode -eq 'missing-tools') {
            Join-Path $root 'sdk/cmdline-tools/missing/bin/sdkmanager'
        } else { Join-Path $bin 'sdkmanager' }
        & dotnet cake $harness --verbosity=quiet
        if ($LASTEXITCODE -ne 0) { throw "Owned emulator lifecycle fixture failed: $($case.Mode)/$($case.Abi)." }
        if ($case.Mode -in @('missing-image', 'missing-tools')) {
            if (Test-Path (Join-Path $root 'commands')) { throw 'Invalid SDK/image must fail before creation.' }
            continue
        }
        $arguments = @(Get-Content (Join-Path $root 'avd-arguments'))
        if ($arguments[5] -ne $expectedImage -or $arguments[7] -ne $expectedModel) {
            throw 'Full system image ABI or legacy model was not passed as a single argument.'
        }
        $name = Get-Content (Join-Path $root 'name') -Raw
        if (Test-Path (Join-Path $root $name)) { throw "Owned AVD was not deleted: $name." }
        $commands = @(Get-Content (Join-Path $root 'commands'))
        if ($commands[0] -ne "create $name" -or @($commands | Where-Object {
                $_ -ne "create $name" -and $_ -ne "delete $name" -and $_ -ne 'list' }).Count) {
            throw 'Lifecycle targeted a non-owned AVD.'
        }
        if ($case.Mode -eq 'create-no-avd' -and ($commands -join '|') -ne "create $name|list") {
            throw 'Rejected creation must list once, never delete a nonexistent or unrelated AVD, and clear ownership.'
        }
        if ($case.Mode -notin @('create-no-avd', 'create-failure', 'create-list-failure')) {
            $emulatorPid = [int](Get-Content (Join-Path $root 'pid') -Raw)
            try {
                $process = [Diagnostics.Process]::GetProcessById($emulatorPid)
            } catch [ArgumentException] {
                $process = $null
            }
            if ($null -ne $process) {
                try {
                    if (-not $process.HasExited) { throw 'Owned emulator process is still running.' }
                } finally { $process.Dispose() }
            }
            $logs = Join-Path $root "output/logs/android/$name"
            if ((Get-Content (Join-Path $logs 'emulator.stdout.log') -Raw) -notmatch 'owned emulator boot output' -or
                (Get-Content (Join-Path $logs 'emulator.stderr.log') -Raw) -notmatch 'owned emulator boot error') {
                throw 'Owned pre-adb boot output was not retained.'
            }
            $emulatorArguments = @(Get-Content (Join-Path $root 'emulator-arguments'))
            if ($emulatorArguments[1] -ne $name -or '-no-window' -notin $emulatorArguments -or
                '-no-snapshot' -notin $emulatorArguments) { throw 'Headless owned emulator arguments changed.' }
            $port = $emulatorArguments[3]
            foreach ($command in @(Get-Content (Join-Path $root 'adb-arguments'))) {
                if (-not $command.StartsWith("-s emulator-$port ")) { throw "ADB did not target the exact owned serial: $command" }
                if ($case.Mode -eq 'identity-failure' -and $command.Contains('settings put')) {
                    throw 'Identity failure must not mutate a foreign emulator.'
                }
            }
            if ($case.Mode -eq 'success') {
                $adb = Get-Content (Join-Path $root 'adb-arguments') -Raw
                foreach ($setting in @('window_animation_scale', 'transition_animation_scale', 'animator_duration_scale')) {
                    if (-not $adb.Contains("settings put global $setting 0")) { throw "Animation setting was not disabled: $setting" }
                }
            }
        }
        foreach ($file in @('pid', 'name', 'commands', 'avd-arguments', 'adb-arguments', 'emulator-arguments')) {
            $path = Join-Path $root $file
            if (Test-Path $path) { Remove-Item -LiteralPath $path }
        }
    }
    Write-Host 'Android configuration and lifecycle fixtures passed (fake SDK only; emulator ABIs, physical mappings, legacy and explicit profiles).'
} finally {
    # On a fixture assertion failure, stop only a process whose command contains this exact fixture path.
    $pidFile = Join-Path $root 'pid'
    if (Test-Path $pidFile) {
        $ownedPid = [int](Get-Content $pidFile -Raw)
        $command = & ps -p $ownedPid -o command=
        if ($command -and $command.Contains((Join-Path $root 'sdk/emulator/emulator'))) {
            $process = [Diagnostics.Process]::GetProcessById($ownedPid)
            try {
                $process.Kill($true)
                if (-not $process.WaitForExit(30000)) { throw 'Fixture emulator did not exit.' }
            } finally { $process.Dispose() }
        }
    }
    foreach ($entry in $oldEnvironment.GetEnumerator()) {
        if ($null -eq $entry.Value) {
            Remove-Item -LiteralPath "Env:$($entry.Key)" -ErrorAction SilentlyContinue
        } else {
            [Environment]::SetEnvironmentVariable($entry.Key, $entry.Value)
        }
    }
    if (Test-Path $root) { Remove-Item -LiteralPath $root -Recurse -Force }
}
