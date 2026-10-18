$ErrorActionPreference = 'Stop'
Set-StrictMode -Version Latest

if (-not ($IsMacOS -or $IsLinux)) {
    throw 'Android emulator lifecycle fixtures require a Unix host; they never start a device.'
}

$repo = (Resolve-Path (Join-Path $PSScriptRoot '../../../..')).Path
$root = Join-Path $repo "output/android-lifecycle-fixture-$([Guid]::NewGuid().ToString('N'))"
$oldEnvironment = @{}
foreach ($key in @('ANDROID_HOME', 'ANDROID_SDK_MANAGER_PATH', 'FIXTURE_ANDROID_ROOT',
        'FIXTURE_ANDROID_MODE', 'FIXTURE_ANDROID_IMAGE', 'FIXTURE_ANDROID_MODEL')) {
    $oldEnvironment[$key] = [Environment]::GetEnvironmentVariable($key)
}

try {
    $bin = Join-Path $root 'sdk/cmdline-tools/selected/bin'
    New-Item $bin, (Join-Path $root 'sdk/platform-tools'), (Join-Path $root 'sdk/emulator') `
        -ItemType Directory -Force | Out-Null
    foreach ($abi in @('x86', 'x86_64', 'armeabi-v7a', 'arm64-v8a')) {
        $image = Join-Path $root "sdk/system-images/android-36/google_apis/$abi"
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

    $helper = (Join-Path $PSScriptRoot '../tests-shared-android.cake').Replace('\', '/')
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
        SetupAndroidEmulator(ref avd, ref serial,
            EnvironmentVariable("FIXTURE_ANDROID_IMAGE"), EnvironmentVariable("FIXTURE_ANDROID_MODEL"));
        if (mode == "success") {
            if (!avd.StartsWith("SkiaSharp-Tests-") || !serial.StartsWith("emulator-"))
                throw new Exception("Missing exact owned identity.");
            try {
                SetupAndroidEmulator(ref avd, ref serial,
                    EnvironmentVariable("FIXTURE_ANDROID_IMAGE"), EnvironmentVariable("FIXTURE_ANDROID_MODEL"));
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
RunTarget("Default");
"@
    $env:ANDROID_HOME = Join-Path $root 'sdk'
    $env:ANDROID_SDK_MANAGER_PATH = Join-Path $bin 'sdkmanager'
    $env:FIXTURE_ANDROID_ROOT = $root
    $env:FIXTURE_ANDROID_MODEL = 'Nexus 5X'

    $cases = @(
        @{ Mode = 'success'; Abi = 'x86' }
        @{ Mode = 'success'; Abi = 'x86_64' }
        @{ Mode = 'success'; Abi = 'armeabi-v7a' }
        @{ Mode = 'success'; Abi = 'arm64-v8a' }
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
        $env:FIXTURE_ANDROID_IMAGE = "system-images;android-36;google_apis;$($case.Abi)"
        if ($case.Mode -eq 'missing-image') {
            $env:FIXTURE_ANDROID_IMAGE = 'system-images;android-99;google_apis;arm64-v8a'
        }
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
        if ($arguments[5] -ne $env:FIXTURE_ANDROID_IMAGE -or $arguments[7] -ne 'Nexus 5X') {
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
    Write-Host 'Android lifecycle fixtures passed (fake SDK only; all four ABIs and legacy model).'
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
        [Environment]::SetEnvironmentVariable($entry.Key, $entry.Value)
    }
    if (Test-Path $root) { Remove-Item -LiteralPath $root -Recurse -Force }
}
