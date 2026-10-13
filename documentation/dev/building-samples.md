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

`AppiumSampleTests` runs generated Basic and Gallery MAUI apps on Android,
iOS, MacCatalyst and Windows, and native Basic Android, iOS, MacCatalyst,
macOS and WinUI apps. Basic covers CPU rendering, drawing/clear and GPU smoke;
Gallery covers search, filtering, Gradient controls and CPU/GPU rendering.

After the package setup above, restore local tools with `dotnet tool restore`.
Install Appium and the selected driver explicitly; put `appium` on PATH.
Use UiAutomator2 5.0.7 for API 24 and 8.5.0 for API 37 (driver 6+ requires API
26), XCUITest for iOS, Mac2 for MacCatalyst/macOS, or Windows for WinUI/MAUI
Windows. Tests do not install tools, workloads or device images.

Set `ANDROID_HOME` or `ANDROID_SDK_ROOT` for Android. On macOS, set
`DEVELOPER_DIR` to the Xcode required by your installed workload; tests do not
change the global Xcode selection.

The explicit Android profiles are **Pixel_API_24**, Android 7.0/API 24,
`emulator-5558`, 4 KB pages, and **Pixel_API_37**, Android 17/API 37,
`emulator-5556`, 16 KB pages. iOS uses **iPhone 16 Pro** on **18.6** and **26.2**.
Create the exact AVD/runtime/simulator before selecting its cases. For API 37
on an ARM64 host:

```sh
dotnet android avd create --name Pixel_API_37 \
  --sdk 'system-images;android-37.1;google_apis_ps16k;arm64-v8a' \
  --device pixel_9
```

Tests serialize foreground interactions, start only selected stopped devices
and restore their original state. To dedicate an already running device, set
`SKIASHARP_APPIUM_USE_BOOTED_DEVICE=1`; it remains running afterward. Existing
sample installations or driver sessions are conflicts; tests do not overwrite
their data or stop unrelated processes.

Appium port 4727 and the selected driver port must be free: Android 8217,
iOS 8117, Mac2 10117, Windows 4737. Mac2 requires an unlocked GUI session,
Xcode automation/XcodeHelper Accessibility permissions, and
`SKIASHARP_APPIUM_MAC_WDA_ROOT` pointing to the installed driver's
WebDriverAgentMac source. Windows uses port 4737 and requires an interactive
desktop, Developer Mode and `SKIASHARP_APPIUM_WINAPPDRIVER` pointing to the
explicitly installed executable.

After bootstrap, package staging, and exact-suffix generation:

```sh
dotnet test tests/SkiaSharp.Tests.Samples/SkiaSharp.Tests.Samples.csproj \
  -p:TargetFramework=net10.0 -p:TargetFrameworks=net10.0 \
  -- --filter-class '*AppiumInfrastructureTests' --report-trx
dotnet test tests/SkiaSharp.Tests.Samples/SkiaSharp.Tests.Samples.csproj \
  -p:TargetFramework=net10.0 -p:TargetFrameworks=net10.0 \
  -- --filter-trait AndroidApi=37 --report-trx
```

Select Android rows with `AndroidApi=24` or `AndroidApi=37`; select other
platforms with `Platform=iOS`, `Platform=MacCatalyst`, `Platform=macOS` or
`Platform=Windows`. Multiple xUnit include-traits are ORed, not intersected.
Select only platforms supported by your host. Missing prerequisites fail
selected cases.

Local `samples` and IDE discovery remain unfiltered. CI runs Appium platform
legs after acquiring native and NuGet artifacts; host legs exclude
`Category=Device`. Cake forwards `sampleTestTrait` / `sampleExcludeTrait` to
xUnit without interpreting devices.

Static references use the comparison policy above and live under
`Expected/Appium/<family>/<platform>/<exact-profile>/`. Review captures before
adding or replacing references; missing references fail. Appium/device logs,
screenshots, UI hierarchy and binlogs use the shared diagnostics directory.
