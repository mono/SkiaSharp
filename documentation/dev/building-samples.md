# Building and Validating Samples

The Samples suite builds generated applications against produced NuGet packages,
runs Console/Web APIs on the host and in Docker, and checks actual browser UIs.

## Run the suite

Install .NET SDK **10.0.402** and workload set **10.0.401** with your host's
required workloads. .NET 10 managed Apple builds require macOS Tahoe and
**Xcode 26.6**; select it with `DEVELOPER_DIR`.
Native Apple sample apps use the same Xcode version.
The WinUI C++ projection uses SDK **10.0.113** alongside the main SDK because
VS MSBuild 17.x cannot load the newer SDK feature bands.

The pinned Apple workload is **26.5.10318**. Basic and Gallery pin MAUI
**10.0.110**. Basic native and Basic/Gallery MAUI samples support iOS 15.0 and
MacCatalyst 17.0 (macOS 14); native AppKit and tvOS samples require macOS 14.0 and
tvOS 15.0. Their Apple heads use the scene lifecycle for local
Xcode 27 compatibility as well as the CI Apple 26 pairing. MAUI uses
`MauiUISceneDelegate`; native UIKit retains its `Main` storyboard and default-tab
selection in the owning scene. Check the built app's scene manifest and runtime
launch behavior; source plist checks alone do not establish compatibility.

The macOS Build group also needs a tvOS simulator runtime to compile storyboards.
CI uses the selected Xcode's standard platform download, without deriving a
runtime version from the SDK version:

```sh
xcrun simctl list runtimes
xcodebuild -downloadPlatform tvOS
```

For Docker tests,
put `docker` on `PATH` and start a daemon using Windows containers on Windows
or Linux containers on macOS/Linux. Unavailable Docker skips those tests;
wrong container mode and sample failures fail.

**Bootstrap before staging packages:** bootstrap resets `output/`.
For managed-only work:

```sh
dotnet cake --target=externals-download
```

For native or Skia changes, build natives from source instead; see
[AGENTS.md](../../AGENTS.md). Do not download prebuilt natives after native changes.

Stage one producing build's non-symbol packages in `output/nugets/`, including
the required native/view packages. Base versions must match `scripts/VERSIONS.txt`.
Download its `nuget` artifact or use the PR helper:

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

This generates the samples and runs the suite. Keep all packages from the same
producing build and pass its suffix explicitly.

To run one group, add `--runGroup=Build`, `--runGroup=Host`, or
`--runGroup=Docker`. Appium groups are `Android`, `Ios`, `Mac` and `Windows`.
Omit `--runGroup` to run the full suite.
Group names are case-sensitive.

## IDE and direct test runs

After staging packages, generate once before IDE or direct test runs:

```sh
dotnet cake --target=samples-generate --previewLabel=pr.1234 --buildNumber=26509.10
dotnet test tests/SkiaSharp.Tests.Samples.slnx \
  -p:TargetFramework=net10.0 -p:TargetFrameworks=net10.0 -- --report-trx
```

Keep both framework properties to limit the runner's source dependencies to
.NET 10. Sample builds use their own declared TFMs.

Direct tests use the generated inputs without invoking Cake or downloading
packages. The runner uses the repository SDK; sample builds use `dotnet` from
the inherited `PATH`, without changing project TFMs. Set the intended SDK and
platform environment before launching the terminal or IDE. Owned consumers
default to the SDK in the repository's `global.json`; set
`SAMPLE_TEST_SDK_VERSION` to select another exact installed SDK. This selection
uses `rollForward: disable` and does not change the runner SDK or any sample's
declared frameworks. Console keeps its declared `net10.0` target, including when
built with the preview SDK.

Sample helpers accept one optional `DotNetOptions` record. For example,
`BuildSample(folder, project, new() { Framework = "net10.0-android" })`
validates the requested framework against the project declarations and passes
`--framework` to `dotnet`. Project files and framework lists are not rewritten.
Restore keeps the declared framework graph, so its workloads must be available.
Reuse the same options with `DotNet.Build` and the callback-based `DotNet.Run`;
additional `Properties` are passed to both before run's `--` separator.
Configuration defaults to Release. `WorkingDirectory` defaults to the project
directory (workspace root for raw `DotNet.Execute` commands).
`Timeout` defaults to 30 minutes for builds and one minute for raw commands.
For sample runs it overrides only address/exit waits (normally two/three minutes),
not callback duration or browser-operation waits; cleanup/output reads stay bounded
to one minute. Configuration, framework and helper-owned restore properties cannot
be overridden through `Properties`.
HTTP content requests allow 30 seconds for cold rendering; readiness probes retain
their four-second per-attempt limit.

## Browser runs

For local browser tests, build the runner and install its matching Chromium:

```sh
dotnet build tests/SkiaSharp.Tests.Samples/SkiaSharp.Tests.Samples.csproj
pwsh tests/SkiaSharp.Tests.Samples/bin/Debug/net10.0/playwright.ps1 install --with-deps chromium
dotnet test tests/SkiaSharp.Tests.Samples/SkiaSharp.Tests.Samples.csproj \
  -- --filter-trait Category=Browser --report-trx
```

Use the Release script path for a Release build. CI installs Chromium before
Host-group or unfiltered tests automatically; other groups and local tests do
not. Linux `--with-deps` may require `sudo`.
See [Playwright browser installation](https://playwright.dev/dotnet/docs/browsers).
Filtered runs do not replace the full suite.

## Coverage and diagnostics

Basic/Gallery samples have build coverage; Console/Web APIs and Web/WASM/Blazor
UIs have runtime checks, including Blazor Drawing pointer strokes, color/brush
selection and clear. Basic Uno runs its declared `net10.0-browserwasm` project
target, checking CPU rendering and Drawing navigation, pointer strokes and
clear rather than launching a desktop or host solution variant.
Gallery MAUI also has Appium device-runtime coverage.
Browser errors and unavailable GPU rendering fail.

| Group | Tests | CI hosts |
| --- | --- | --- |
| Build | Generated sample compilation and infrastructure checks | Windows, macOS, Linux |
| Host | Console, Web API and browser sample execution | Windows, macOS, Linux |
| Docker | Container image builds and sample execution | Windows, Linux |
| Android | MAUI and native Android app execution | Linux (Ubuntu 24.04, KVM) |
| Ios | MAUI and native iOS simulator execution | macOS |
| Mac | MAUI/native MacCatalyst and native AppKit execution | macOS |
| Windows | MAUI and native WinUI app execution | Windows |

When adding tests, choose a `RunGroup` by execution target, not sample family.
Name execution classes by purpose: `SampleBuildTests`, `BasicSampleRunTests`,
`BrowserSampleRunTests`, and `UnoBrowserSampleRunTests`. `DockerSampleBuildTests`
and `DockerSampleRunTests` share one collection fixture and both use the Docker
group. Infrastructure, discovery, and artifact-contract checks stay separate.
`DotNetSampleTestBase` prepares isolated .NET workspaces; `DockerSampleTestBase`
prepares flat Docker build contexts. The Docker fixture owns cached images,
container execution, and Docker resource cleanup. Both bases live beside the
test classes; CLI wrappers (`DotNet`, `Docker`) and fixtures live in `Utils`.
Independent test collections run in parallel. Browser classes share a collection
to serialize browser workloads; Docker classes share another for their image
cache and owned-resource registry. Neither collection blocks unrelated tests.
Appium platform classes share a third collection to serialize focus-sensitive
device interactions. Tests and theory rows within each class/collection remain serial.
Each test requires exactly one typed attribute, such as
`[RunGroup(SampleRunGroup.Build)]`. Apply it to a class only when every test uses
the same group; otherwise annotate individual methods.

Find output, binlogs, results and images under `output/logs/testlogs/samples/`
or CI's `sample_logs_*` artifacts. For restore failures, check the generated
versions against staged packages. Do not clear shared caches.

Review platform references under `tests/SkiaSharp.Tests.Samples/Expected/`.
Dimensions must match; the default limit is **0.075% differing pixels** with
zero channel tolerance. Animated browser GPU pages use a loose **6% mean RGBA error**
smoke check that can miss uniform dark output. Mismatches retain actual/diff
images; references are never accepted automatically.

`SampleImage.Validate` takes `actual`, `expected` (or a relative golden path),
and an optional `SampleImageOptions` record. Its byte-array overload also takes
the actual output path, saves the bytes, then calls the file overload.
Set `ExpectedSize = new(800, 600)` for fixed-size PNGs.
Omitting options retains the strict pixel comparison and golden dimensions.
Pass a screenshot callback to retry that same comparison for up to two seconds.
Only the final mismatch produces a diff; missing references fail immediately.
For an explicitly requested reference-generation run, build the runner with
`-p:CaptureSampleReferences=true`. Screenshot comparisons then retain PNGs and
`.reference.json` destination records instead of asserting golden matches.
Inspect the captures before copying them into `Expected/`, then rebuild without
that property and run normal validation. Capture mode never updates references
automatically and is not evidence that golden comparisons pass.

## Appium application runtime tests

`AppiumAndroidSampleRunTests`, `AppiumIosSampleRunTests`, `AppiumMacSampleRunTests`
and `AppiumWindowsSampleRunTests` run generated Basic and Gallery MAUI apps on Android,
iOS, MacCatalyst and Windows, and native Basic Android, iOS, MacCatalyst,
macOS and WinUI apps. Basic visits every page and checks CPU/GPU rendering;
separate MAUI touch tests draw two strokes and clear the drawing.
Gallery covers search, filtering, Gradient controls and CPU/GPU rendering.

After the package setup above, restore local tools with `dotnet tool restore`.
Install Appium and the selected driver explicitly; put `appium` on PATH.
Use UiAutomator2 for API 26 and 37 (driver 6+ requires API 26), XCUITest for iOS,
Mac2 for MacCatalyst/macOS, or Windows for WinUI/MAUI
Windows. Tests do not install tools, workloads or device images.
CI runs `scripts/infra/tests/install-appium.ps1` to install Appium and the selected
driver in the standard npm global and `~/.appium` locations. Windows jobs use
WinAppDriver supplied by the selected UI-test image.
Appium jobs use the repository's platform-default .NET workloads and pinned Tizen setup.
Android jobs run on Linux with KVM. Linux emulator runs are headless and require
`libpulse0`. Basic and Gallery MAUI declare Android only
on Linux, Android/iOS/MacCatalyst on macOS, and Android/Windows on Windows.
Native Basic Android remains Android-only. Framework selection does not rewrite
these declarations; restore uses the host's declared graph.
CI provisions each explicitly selected emulator image or
simulator runtime; local tests still require those prerequisites to be installed.
The shared bootstrapper supplies Node.js 22 on managed hosts and selects the SDK
descriptor's exact installed Xcode for the job through `DEVELOPER_DIR` and
`MD_APPLE_SDK_ROOT`, without changing the machine's global Xcode selection.
Windows samples build with `WindowsPackageType=None` and launch their executable
directly; tests do not register or uninstall MSIX packages.
MAUI Windows canvas captures use the existing page/layout hosts, whose automation
peers expose their bounds; the underlying WinUI canvas controls have no peers.

Set `ANDROID_HOME` or `ANDROID_SDK_ROOT` for Android. On macOS, set
`DEVELOPER_DIR` to the Xcode required by your installed workload; tests do not
change the global Xcode selection.
Android requires the installed `cmdline-tools/latest/bin/avdmanager`, or the
sibling of `ANDROID_SDK_MANAGER_PATH` when an SDK manager is selected explicitly.
Debug APKs embed their managed assemblies because Appium installs them without
the separate `dotnet` fast-deployment step.

Local runs select Android **17**, using model `pixel_9`. iOS defaults to **iPhone 16 Pro**
on **26.5**. Install `system-images;android-37.1;google_apis_ps16k;<host-ABI>`
and the selected iOS runtime/device type before running tests. Missing prerequisites
fail local tests; the test runner never installs device images or simulator runtimes.

Install the selected prerequisites explicitly before a local run:

```powershell
pwsh scripts/infra/managed/install-android-package.ps1 -Package emulator
pwsh scripts/infra/managed/install-android-image.ps1 -Image 'android-37.1;google_apis_ps16k'
# On macOS, with DEVELOPER_DIR already selecting the required installed Xcode:
pwsh scripts/infra/managed/install-ios-runtime.ps1 -Version 26.5 -BuildVersion 23F77
```

The Android image helper selects `arm64-v8a` or `x86_64` from the host architecture;
pass only the short image name, without `system-images;` or an ABI.
The iOS helper requires the exact released OS version and build, warms CoreSimulator,
and verifies availability again after downloading. Its transcript is
`output/logs/install-ios-runtime.log`. These helpers do not create or boot devices.

CI device jobs supply the desired version/image or simulator model. To use a
different preinstalled runtime locally:

```sh
export SAMPLE_TEST_ANDROID_VERSION=8.0
export SAMPLE_TEST_ANDROID_MODEL=pixel_9
export SAMPLE_TEST_ANDROID_IMAGE='android-26;google_apis'
export SAMPLE_TEST_IOS_VERSION=18.6
export SAMPLE_TEST_IOS_MODEL='iPhone 16 Pro'
```

Android defaults to `pixel_9` when the model variable is unset. Model IDs come
from `dotnet android avd devices`.
The Android ABI follows the host architecture; the image and iOS runtime/model
must already be installed. Unset these variables to return to local defaults.

Appium receives the device profile and built app.
Android reads its package ID from the APK with the repository Android tool;
iOS and Mac read the built bundle's `Info.plist` with `plutil` (XML or binary).
Builders do not read app IDs from source projects.
XCUITest's
`enforceFreshSimulatorCreation` creates, boots and deletes a fresh iOS simulator;
its startup normalizes the status bar to `10:10`, fixed signal indicators and
100% battery through Appium. This does not change the system clock or crop images.
UiAutomator2's `avd` capability boots the uniquely named Android AVD.
The small Android helper creates/deletes a unique `SkiaSharpSamples_<version>_<id>`
AVD from the preinstalled image, because UiAutomator2 does not provide AVD
creation/deletion. Appium selects the emulator serial; existing devices/data
remain untouched.
Appium owns app installation, launch, logs and session cleanup. Foreground
interactions are serialized.
Android guest diagnostics match the exact owned AVD name before collecting boot
properties, logcat and available kernel output, including failed session creation.

The native AppKit scenario attaches `opengl-pixel-formats.log`, comparing the
view's CGL requirements with bounded controls on the same host. This does not
select a fallback or replace execution of the actual app.
Mac drawing-page diagnostics also record the Clear button's enabled, hittable
and keyboard-focus state in `drawing-clear-state.log`.

The fixture uses a free loopback port for each server and driver connection;
UiAutomator2 selects its own available driver port. Mac2 requires an unlocked GUI session,
Xcode automation/XcodeHelper Accessibility permissions, and
an installed Mac2 driver. Appium discovers and builds its WebDriverAgentMac.
Windows requires an interactive desktop, Developer Mode and installed WinAppDriver.
Windows Appium CI jobs select `1es-windows-2022-open`, matching MAUI's public
Windows UI-test image; ordinary Windows build jobs keep their existing image.
The installer does not enable Developer Mode or change desktop settings.
Appium finds WinAppDriver in standard Program Files locations or its MSI registration;
`APPIUM_WAD_PATH` optionally overrides its executable path.
Mac2 launches the exact built bundle through `appPath`; no custom launch or
LaunchServices scripts are needed. Existing running apps are not reused.
Mac2 relaunches each test app; normal macOS preferences and sandbox containers
are left intact, so repeated runs do not delete user data or change app identities.
Mac2 and Windows drivers own app launch/termination and their WDA/WAD
server lifecycle. Windows launches the exact built executable through its full
path; existing processes are not reused or terminated.
Windows retains the session window's verified process identity and handle, then
terminates only that owned process if it survives driver/server cleanup.

After bootstrap, package staging, and exact-suffix generation:

```sh
dotnet test tests/SkiaSharp.Tests.Samples/SkiaSharp.Tests.Samples.csproj \
  -p:TargetFramework=net10.0 -p:TargetFrameworks=net10.0 \
  -- --filter-class '*AppiumInfrastructureTests' --report-trx
dotnet test tests/SkiaSharp.Tests.Samples/SkiaSharp.Tests.Samples.csproj \
  -p:TargetFramework=net10.0 -p:TargetFrameworks=net10.0 \
  -- --filter-trait RunGroup=Android --report-trx
```

Select a device group with `RunGroup=Android`, `RunGroup=Ios`,
`RunGroup=Mac` or `RunGroup=Windows`. Each platform class has separate MAUI and
native facts; mobile facts use one selected device, not a theory matrix.
Select only platforms supported by your host. Missing prerequisites fail selected cases.

Local `samples` and IDE discovery remain unfiltered. CI runs Appium device
legs after acquiring native and NuGet artifacts; existing `Build`, `Host` and
`Docker` jobs retain their execution groups. CI explicitly installs the selected
Appium driver and image/runtime; tests only check these prerequisites.
The `sampleSdks` objects in `scripts/azure-templates-stages-test.yml` declare SDK
installs, Console frameworks, Xcode, agent image and device profiles. Build,
Host and Docker jobs use both SDK descriptors. `runDevices` selects which
descriptor supplies the device-runtime jobs; it does not multiply devices by SDKs.
Each Android/iOS profile gets its own job. Cake selects only the execution group:

```sh
dotnet cake --target=samples --runGroup=Android \
  --previewLabel=pr.1234 --buildNumber=26509.10
```

Static references use **0.075% differing pixels/zero channel tolerance** and live under
`Expected/Appium/<family>/<platform>/<exact-profile>/`. Review captures before
adding or replacing references; missing references fail. Appium/device logs,
screenshots, UI hierarchy and binlogs use the shared diagnostics directory.
