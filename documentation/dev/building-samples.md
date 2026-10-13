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

## Appium application runtime tests

`AppiumSampleTests` builds actual generated Basic MAUI applications on Android,
iOS, MacCatalyst and Windows, plus native Basic Android, iOS, MacCatalyst,
macOS and WinUI applications. Gallery MAUI covers all four MAUI platforms.
Every application retains its declared framework and consumes the exact
staged package cohort. The ordinary source-backed runner does not consume
those product packages. Integration, MSBuild and existing core/device suites
remain unchanged.

`AppiumFixture` owns the server; `AppiumRunningApp` owns the driver, shared
interactions, native element screenshots, diagnostics and cleanup. Android,
iOS, Mac and Windows running apps supply their platform-specific lifecycle.
Mac2 serves both AppKit and MacCatalyst. No HTTP-server abstraction, synthetic
application, guessed desktop scale or sample-family runtime hierarchy is used.

Install Appium and its UiAutomator2/XCUITest drivers explicitly before running
tests; discovery never downloads a server, driver, workload, or system image.
Restore the repository's pinned local tools with `dotnet tool restore`.
Device discovery, boot readiness, installation, launch, and removal reuse `dotnet
android` and `dotnet apple`, as the existing device suite does. Android startup
uses `--wait --wait-exit` so the fixture retains its owned process tree;
no forced AVD creation/deletion or latest/first-device selection is inherited.
`Android.cs` and `Apple.cs` provide typed tool commands through `DotNet.cs`.
The narrow Android AVD identity, page-size, activity and runtime-log queries
use explicitly selected `adb` commands.
Put `appium` on PATH. Set `ANDROID_HOME` or `ANDROID_SDK_ROOT` for Android.
On macOS, set `DEVELOPER_DIR` for both build and Appium to the Xcode required by
the installed Apple workload (for example,
`/Applications/Xcode-26.6.0.app/Contents/Developer`). Tests never change
the global Xcode selection.

The explicit Android profiles are **Pixel_API_24**, Android 7.0/API 24,
`emulator-5558`, 4 KB pages, and **Pixel_API_37**, Android 17/API 37,
`emulator-5556`, 16 KB pages. iOS uses **iPhone 16 Pro** on **18.6** and **26.2**.
These runtime profiles do not retarget the application's compile API.
The assigned Android samples declare minima of API 21 (Basic) or 23 (Gallery);
the assigned iOS samples declare minima of 12.2 (Basic) or 15 (Gallery).
Profile declarations are not evidence of successful driver/runtime execution.
Create the exact AVD/runtime/simulator before selecting its cases. For API 37:

```sh
dotnet android avd create --name Pixel_API_37 \
  --sdk 'system-images;android-37.1;google_apis_ps16k;arm64-v8a' \
  --device pixel_9
```

Tests start only the selected stopped device, then stop their owned process or
restore the simulator to shutdown. A running selected device requires an
explicit `SKIASHARP_APPIUM_USE_BOOTED_DEVICE=1` dedication; it is not stopped after
testing. Other devices, including `emulator-5554`, are not touched. An already
installed sample app is a conflict, not permission to overwrite user data.
Ports 4727 and 8217 (Android) or 8117 (iOS) must be free: tests never reuse or kill
foreign Appium/driver servers. Only the newly installed sample app is removed.
Desktop Mac2 uses port 10117 and requires an unlocked GUI session, Xcode
automation/XcodeHelper Accessibility permissions, and
`SKIASHARP_APPIUM_MAC_WDA_ROOT` pointing to the installed driver's
WebDriverAgentMac source. Windows uses port 4737 and requires an interactive
desktop, Developer Mode and `SKIASHARP_APPIUM_WINAPPDRIVER` pointing to the
explicitly installed executable. Packaged WinUI output is registered from its
actual generated manifest/package, not silently treated as an unpackaged EXE.
Existing installations, state and running app/driver conflicts fail explicitly.

After bootstrap, package staging, and exact-suffix generation:

```sh
dotnet test tests/SkiaSharp.Tests.Samples/SkiaSharp.Tests.Samples.csproj \
  -p:TargetFramework=net10.0 -p:TargetFrameworks=net10.0 \
  -- --filter-class '*AppiumInfrastructureTests' --report-trx
dotnet test tests/SkiaSharp.Tests.Samples/SkiaSharp.Tests.Samples.csproj \
  -p:TargetFramework=net10.0 -p:TargetFrameworks=net10.0 \
  -- --filter-class '*AppiumSampleTests' --report-trx
```

Device tests serialize foreground interactions. Appium's automatic app launch
is disabled: the repository device tools launch the acquired-package app, and
the tests require its actual UI. On iOS, Appium's idle wait is limited to one
second so the real animated GPU page remains interactive; explicit element and
static-frame waits still gate assertions. No rendering loop is disabled.
Basic checks CPU rendering,
flyout navigation, an actual touch stroke/clear, and animated GPU rendering
(smoke only, never a frozen-frame golden). Gallery searches and filters for
Gradient, changes its angle control, returns to the gallery, and renders the
stable Gradient through both CPU and GPU. Static canvas references live in
`Expected/Appium/<family>/<platform>/<exact-profile>/`; their comparison retains
the default 0.075% differing-pixel budget and zero channel tolerance.
Missing references fail with retained captures; they are never auto-approved.
Appium logs, device preflight output, screenshots, UI hierarchy, and build
binlogs are retained as current-test attachments under the shared diagnostics
directory.

Local `samples` and IDE discovery remain unfiltered. CI uses the standard
`Category=Device` and method/row `Platform`/`AndroidApi` traits to route every
assigned case after the producing native+NuGet artifacts are available.
Cake only forwards generic `sampleTestTrait` / `sampleExcludeTrait` values to
xUnit; it does not choose devices or interpret Android API levels. Android
profiles are xUnit theory data, selected with `--filter-trait AndroidApi=24`
or `--filter-trait AndroidApi=37`. CI's Android API parameter selects explicit
driver/image prerequisites and supplies that ordinary row filter.
Host jobs exclude only device rows; additive Appium legs use the existing
Windows, Mac and Android agents/bootstrapper. API 24 uses UiAutomator2 5.0.7;
API 37 uses 8.5.0 because UiAutomator2 6+ requires API 26. Installation is an
explicit CI prerequisite, never test discovery. Missing runtime, driver,
interactive-session or AX support fails the selected leg rather than skipping.
CI provisioning does not change TCC databases, configure autologon, or alter
the user's local workstation. CI success and new screenshot references still
require actual reviewed execution.

Explicit builds use a workspace-only `Directory.Build.targets` import guarded
by the normalized exact selected root project. It checks declared framework
membership before targets run and narrows only that root's restore graph.
Referenced libraries retain their declared frameworks; no global
`TargetFrameworks` override is passed. Existing sample targets are preserved.
Typed `DotNetBuildOptions`, `DotNetRunOptions` and `DotNetCommandOptions` retain
configuration, private-cache, diagnostics and callback defaults. Published
legacy signatures remain compatibility entry points until their callers migrate.
