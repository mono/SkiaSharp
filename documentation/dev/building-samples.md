# Building and Validating Samples

The Samples suite builds generated applications against produced NuGet packages
and runs the Console/Web samples on the host and in Docker.

## Run the suite

Install the repository SDK and the workloads required by your host's samples.
Docker build/run tests run on any host with usable Docker, including macOS.
Put `docker` on `PATH` and start a daemon using Windows containers on Windows
or Linux containers on macOS/Linux. CI does not provision Docker on macOS.
Unavailable Docker produces visible skips; wrong container mode and sample
failures fail the tests.

**Bootstrap before staging packages:** bootstrap resets `output/`.
For managed-only work:

```sh
dotnet cake --target=externals-download
```

For native or Skia changes, build natives from source instead; see
[AGENTS.md](../../AGENTS.md). Do not download prebuilt natives after native changes.

Stage one producing build's non-symbol packages in `output/nugets/`, including
SkiaSharp, HarfBuzzSharp, and the required native/view packages. Its base versions
must match `scripts/VERSIONS.txt`. Download the public `nuget` build artifact,
or use the PR helper:

```powershell
$pr = 1234 # Producing PR number.
pwsh scripts/get-skiasharp-pr.ps1 $pr -SuccessfulOnly -Force
New-Item output/nugets -ItemType Directory -Force | Out-Null
Copy-Item "$HOME/.skiasharp/hives/pr-$pr/packages/*.nupkg" output/nugets/
```

Pass the producing package suffix explicitly. For example, packages ending in
`-pr.1234.26509.10` require:

```sh
dotnet cake --target=samples --previewLabel=pr.1234 --buildNumber=26509.10
```

This generates the samples and runs the suite. Do not mix producing builds:
generation uses the supplied arguments, not package-filename version detection.
Cake streams active-test progress and test output to the console while retaining
TRX results and diagnostic artifacts.

To run one group, add `--runGroup=Build`, `--runGroup=Host`, or
`--runGroup=Docker`. Omit `--runGroup` to run the full suite.
Group names are case-sensitive.

## IDE and direct test runs

After staging packages, generate once before opening the test project in an IDE
or running it directly:

```sh
dotnet cake --target=samples-generate --previewLabel=pr.1234 --buildNumber=26509.10
dotnet test tests/SkiaSharp.Tests.Samples.slnx \
  -p:TargetFramework=net10.0 -p:TargetFrameworks=net10.0 -- --report-trx
```

The solution includes the runner's source dependencies; the properties above
limit their build to the runner's .NET 10 target.

Direct tests use the generated inputs without invoking Cake or downloading
packages. The runner uses the repository SDK; sample builds use `dotnet` from
the inherited `PATH`, without changing project TFMs. Set the intended SDK and
platform environment before launching the terminal or IDE.

## Coverage and diagnostics

The suite builds host-eligible Basic and Gallery solutions. Gallery, WASM, and
Blazor are build-only; Console/Web runtime tests check exit codes, HTTP responses,
and PNGs on the host and in Docker. Generated-project tests check exact package
versions and Uno's SkiaSharp override. Integration/MSBuild suites remain separate.

| Group | Tests | CI hosts |
| --- | --- | --- |
| Build | Generated sample compilation and infrastructure checks | Windows, macOS, Linux |
| Host | Console and Web sample execution | Windows, macOS, Linux |
| Docker | Container image builds and sample execution | Windows, Linux |

When adding tests, choose a `RunGroup` by execution target, not sample family.
Each test requires exactly one typed attribute, such as
`[RunGroup(SampleRunGroup.Build)]`. Apply it to a class only when every test uses
the same group; otherwise annotate individual methods.

Use IDE filters or select one test:

```sh
dotnet test tests/SkiaSharp.Tests.Samples/SkiaSharp.Tests.Samples.csproj \
  -- --filter-method '*BasicSampleTests.WebSampleReturnsImage'
```

Find stdout/stderr, binlogs, results, and images under
`output/logs/testlogs/samples/`, or in CI's `sample_logs_*` artifact. For restore
failures, compare generated versions with the staged packages and inspect the
binlog. Tests use private workspaces/caches; do not clear shared caches.

PNG references are platform-specific under `tests/SkiaSharp.Tests.Samples/Expected/`.
Dimensions must match; at most **0.075%** of pixels may differ, with zero channel
tolerance. Failures retain the actual image; mismatches also retain a diff.
Review replacement references on the affected platform; tests never accept them
automatically.

## MAUI mobile runtime tests

`MauiSampleTests` builds **generated Basic/Maui and Gallery/Maui**, using the
declared `net10.0-android` or `net10.0-ios` framework and the staged package
cohort. The ordinary source-backed runner does not consume those product
packages. Integration and MSBuild suites remain unchanged.

Install Appium and its UiAutomator2/XCUITest drivers explicitly before running
tests; discovery never downloads a server, driver, workload, or system image.
Restore the repository's pinned local tools with `dotnet tool restore`.
Device discovery, boot readiness, installation, and removal reuse `dotnet
android` and `dotnet apple`, as the existing device suite does. Android startup
uses `--wait --wait-exit` so the fixture retains its owned process tree;
no forced AVD creation/deletion or latest/first-device selection is inherited.
Only the narrow Android AVD identity, page-size, activity and PID-log queries
use explicitly selected `adb` commands.
Put `appium` on PATH. Set `ANDROID_HOME` or `ANDROID_SDK_ROOT` for Android.
On macOS, set `DEVELOPER_DIR` for both build and Appium to the Xcode required by
the installed Apple workload (for example,
`/Applications/Xcode-26.6.0.app/Contents/Developer`). Tests never change
the global Xcode selection.

The explicit device cases are **Pixel_API_37**, Android 17/API 37,
`emulator-5556`, and **iPhone 16 Pro**, iOS 26.2. API 37 is the selected runtime,
not a retargeting of the sample's declared compile framework. Create the exact
AVD/runtime/simulator before testing. For the reviewed Android profile:

```sh
avdmanager create avd --name Pixel_API_37 \
  --package 'system-images;android-37.1;google_apis_ps16k;arm64-v8a' \
  --device pixel_9
```

Tests start only the selected stopped device, then stop their owned process or
restore the simulator to shutdown. A running selected device requires an
explicit `SKIASHARP_MAUI_USE_BOOTED_DEVICE=1` dedication; it is not stopped after
testing. Other devices, including `emulator-5554`, are not touched. An already
installed sample app is a conflict, not permission to overwrite user data.
Ports 4727 and 8217 (Android) or 8117 (iOS) must be free: tests never reuse or kill
foreign Appium/driver servers. Only the newly installed sample app is removed.

After bootstrap, package staging, and exact-suffix generation:

```sh
dotnet test tests/SkiaSharp.Tests.Samples/SkiaSharp.Tests.Samples.csproj \
  -p:TargetFramework=net10.0 -p:TargetFrameworks=net10.0 \
  -- --filter-class '*MauiInfrastructureTests' --report-trx
dotnet test tests/SkiaSharp.Tests.Samples/SkiaSharp.Tests.Samples.csproj \
  -p:TargetFramework=net10.0 -p:TargetFrameworks=net10.0 \
  -- --filter-class '*MauiSampleTests' --report-trx
```

Device tests serialize foreground interactions. Basic checks CPU rendering,
flyout navigation, an actual touch stroke/clear, and animated GPU rendering
(smoke only, never a frozen-frame golden). Gallery searches and filters for
Gradient, changes its angle control, returns to the gallery, and renders the
stable Gradient through both CPU and GPU. Static canvas references live in
`Expected/Maui/<platform>/<exact-device>/<version>/`; their comparison retains
the default 0.075% differing-pixel budget and zero channel tolerance.
Missing references fail with retained captures; they are never auto-approved.
Appium logs, device preflight output, screenshots, UI hierarchy, and build
binlogs are retained as current-test attachments under the shared diagnostics
directory.
