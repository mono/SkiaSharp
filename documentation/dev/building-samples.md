# Building and Validating Samples

The Samples suite builds generated applications against produced NuGet packages,
runs Console/Web APIs on the host and in Docker, and checks browser, iOS and Android UIs.

## Run the suite

Install the repository SDK and your host's required workloads. For Docker tests,
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

To run one group, add `--runGroup=Build`, `--runGroup=Host`,
`--runGroup=Docker`, `--runGroup=Ios`, or `--runGroup=Android`. Omit `--runGroup`
to run the full suite; device groups require their platform setup and explicit profiles.
Group names are case-sensitive.

## IDE and direct test runs

After staging packages, generate once before IDE or direct test runs:

```sh
dotnet cake --target=samples-generate --previewLabel=pr.1234 --buildNumber=26509.10
dotnet test tests/SkiaSharp.Tests.Samples.slnx \
  -p:TargetFramework=net10.0 -p:TargetFrameworks=net10.0 -- --report-trx
```

Keep both framework properties to limit the runner's source dependencies to
.NET 10. Sample builds use the SDK on `PATH` and their own declared TFMs.

Sample helpers accept one optional `DotNetOptions` record. For example,
`BuildSample(folder, project, new() { Framework = "net10.0-browserwasm" })`
selects a framework with `--framework` without replacing declared framework lists.
Reuse the same options with `DotNet.Build` and the callback-based `DotNet.Run`;
additional `Properties` are passed to both before run's `--` separator.
Configuration defaults to Release. `WorkingDirectory` defaults to the project
directory (workspace root for raw `DotNet.Execute` commands).
`Timeout` defaults to 30 minutes for builds and one minute for raw commands.
For sample runs it overrides only address/exit waits (normally two/three minutes),
not callback duration or browser-operation waits; cleanup/output reads stay bounded
to one minute. Configuration, framework and helper-owned restore properties cannot
be overridden through `Properties`.

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

## iOS simulator runs

The `Ios` group runs Basic MAUI, native iOS and Gallery MAUI samples on macOS
using Appium and XCUITest. Install Node.js and the SDK, workloads and Xcode from
the [shared CI variables](../../scripts/azure-templates-variables.yml).

After staging packages, select one of the runtimes from the
[sample stage](../../scripts/azure-templates-stages-samples.yml):

```sh
IOS_VERSION=18.6
sudo scripts/infra/native/apple/select-xcode.sh "$XCODE_VERSION"
pwsh scripts/infra/tests/install-appium.ps1 -Platform iOS
pwsh scripts/infra/managed/install-ios-runtime.ps1 -Version "$IOS_VERSION"
dotnet cake --target=samples --runGroup=Ios --iosVersion="$IOS_VERSION" \
  --iosModel="iPhone 16 Pro" \
  --skipExternals=all --previewLabel=pr.1234 --buildNumber=26509.10
```

Use your staged packages' suffix, not the example suffix. Reference lookup uses
the actual runtime version, matching version tiers and compatible older captures.
Missing references and mismatches still fail.
The Appium installer also downloads the XCUITest driver's matching prebuilt simulator WDA.

For IDE or direct runs, use `TEST_IOS_SIMULATOR_VERSION` and
`TEST_IOS_SIMULATOR_MODEL` for selection, and `TEST_IOS_SIMULATOR_UDID` to target
an already-running simulator you manage.

Cake cleans up the simulator it creates. Inspect TRX attachments or CI's
`samples_appium_*_logs` artifacts for logs, UI hierarchy and screenshots.

### Two-phase Appium execution

The same test bodies build/prepare an app and pass a scenario to `appium.Run`.
Select `--appiumMode=build`, `run`, or `all` (default), or set
`TEST_APPIUM_MODE` for direct/IDE runs. Explicit Cake phases require a configured
Appium run group.

```sh
# Stage one producing build's packages first; use their actual suffix.
dotnet cake --target=samples --runGroup=Ios --appiumMode=build \
  --skipExternals=all --previewLabel=pr.1234 --buildNumber=26509.10

# After provisioning Appium and the chosen runtime/image, consume that build.
dotnet cake --target=samples --runGroup=Ios --appiumMode=run \
  --iosVersion=18.6 --iosModel="iPhone 16 Pro" --skipExternals=all

# One shot: complete all selected app builds, then boot and run the device.
dotnet cake --target=samples --runGroup=Ios \
  --iosVersion=18.6 --iosModel="iPhone 16 Pro" \
  --skipExternals=all --previewLabel=pr.1234 --buildNumber=26509.10
```

Build mode needs compile SDKs/workloads and packages, but no
emulator/simulator, Appium server, WDA, or device serial. It builds, reads the
built application identity, exports the app, and deliberately does not invoke
the scenario. Repeated cases sharing an app reuse its successful export only
within that test invocation. A fresh build/all invocation rebuilds it.

Apps are staged at
`output/appium-apps/{android|ios}/{x64|arm64}/{Basic/Maui|Basic/Android|Basic/iOS|Gallery}/`.
Each directory contains `manifest.json` and `app.apk` (signed Android APK)
or `app.tar` (iOS bundle, preserving executable permissions and symbolic links).
Manifests record project, framework, runtime architecture, app identity,
artifact SHA-256, and the actual acquired SkiaSharp/HarfBuzzSharp package IDs,
versions and SHA-256 hashes. Missing, corrupt, traversal, mismatched-target or
different-package-cohort artifacts fail; run mode never falls back to building.
Device OS/API is not part of the app key: the same build fans out to runtime profiles.

Build mode also exports the ordinary source-built test runner and host native
assets as `output/appium-runner/{android|ios}/{x64|arm64}/runner.tar` plus
`runner.sha256` and `runner.host`. Cake run mode checks its hash/host and
executes that runner directly with `dotnet exec`:
no sample generation, project evaluation, restore, or compilation. Keep both
staging trees and the exact producing `output/nugets` cohort when transferring
between machines; the platform, host OS and architecture must match the build
host. `TEST_SAMPLE_REPOSITORY` points relocated runners at the consumer checkout
(Cake sets it automatically). Direct test runs default to `all` and prepare
each app before starting its server; use Cake to finish every selected build
before booting its owned device.

CI has one device-free build job per platform/host architecture and dependent
runtime-profile jobs. They download apps and runners from the **current** run,
and packages from the same exact producing build. Only build jobs install sample
workloads/compile platforms; run jobs provision the selected devices and Appium.
Both phases publish separate test results, so build-as-test success is distinct
from scenario execution.

### Updating references

References live in `tests/Goldens/Samples/<profile>/<version-or-base>/`.
For example, `iphone-16-pro/26.3.1/maui-basic-cpu.png` and
`mac/base/blazor-cpu.x64.png`. Cases distinguish MAUI/native and drawing states;
the test driver does not affect the key. The runner reads its packaged
`Goldens/` subtree, so preserve that content when relocating the runner;
reference lookup does not require the source checkout. See
[reference lookup and promotion](golden-image-tests.md#goldens) for ordering,
diagnostic export and review rules. For transformed captures, proposals retain
the raw PNG; do not replace a raw golden with its cropped comparison artifact.
Rerun all configured profiles against the same package cohort before promotion.

## Android emulator runs

The `Android` group runs Basic MAUI, native Android and Gallery MAUI with
Appium/UiAutomator2. The same `--appiumMode=build|run|all` phases apply; use
`--runGroup=Android` with the explicit image/model options below. Install Node.js
and the SDK, JDK, workloads, compile platforms and emulator tools specified by the
[Android CI legs](../../scripts/azure-templates-stages-samples.yml).
The Appium installer pins UiAutomator2; owned-emulator setup preinstalls that
driver's exact server APKs and prepared sample APKs once. Runtime sessions clear
and relaunch the installed applications instead of uninstalling and reinstalling
them for every scenario.
Use one complete image/model profile from that configuration.
Headless Linux requires `libpulse0` and KVM access.

After staging packages, restore the repository's pinned .NET tools and set the
selected SDK's absolute `ANDROID_HOME` in your shell:

```sh
# Set these to one complete profile from the sample stage configuration.
export TEST_ANDROID_IMAGE="<image>" TEST_ANDROID_MODEL="<model>"
dotnet tool restore
pwsh scripts/infra/managed/install-android-sdk.ps1
pwsh scripts/infra/managed/install-android-package.ps1 -Package 'cmdline-tools;latest'
pwsh scripts/infra/managed/install-android-package.ps1 -Package platform-tools
pwsh scripts/infra/managed/install-android-package.ps1 -Package emulator
pwsh scripts/infra/managed/install-android-image.ps1 -Image "$TEST_ANDROID_IMAGE"
pwsh scripts/infra/tests/install-appium.ps1 -Platform Android
dotnet cake --target=samples --runGroup=Android \
  --skipExternals=all --previewLabel=pr.1234 --buildNumber=26509.10
```

Use your packages' suffix, not the example suffix. Provisioning scripts publish
SDK paths for CI but cannot export them back to a calling shell.
Update command-line tools as well as emulator packages: older SDKs may not
include the configured Pixel 9 device profile.

Provisioning and emulator lifecycle use the pinned `dotnet android` CLI.
Cake creates a unique AVD, starts it with `avd start --wait --no-animations`
and the CLI's CPU/response settling, then requires its exact serial in
`device list`. The released CLI owns boot readiness, including its built-in
launcher/ANR handling; Cake does not add a second boot/readiness loop.
Only teardown uses direct ADB to stop that exact serial before CLI AVD deletion,
because the released tool has no standalone stop command. Diagnostics include
CLI logcat, device properties and the Android tool's process log; arbitrary
shell/last-ANR dumps are not available through this CLI.
Cake creates and cleans up only its unique AVD/process; each test owns its
Appium server/session and installed APK. `samples-generate` starts neither.

For direct/IDE runs, explicitly set the same two `TEST_ANDROID_*` profile
variables and `TEST_ANDROID_UDID` to the exact serial of a device you manage.
Missing serials fail before device interaction; no first-device selection is used.

Android references use `pixel-9/<API>/<case>[.arm64|.x64].png`.
API 26's three Gallery gradients and API 36's Gallery CPU/post-interaction drawing
captures have reviewed x64 overrides; the other verified pairs share unsuffixed
files. Initial drawing is a separate case: captured control antialiasing on
API 36 x64 changes after interaction. These cases retain channel tolerance
zero and the same outlier budget rather than selecting a reference by its pixels.
API levels are not Android
release numbers: API 36 never searches API 26. The registered Pixel 9 profile
requires `google_apis`; another image flavor needs its own reviewed profile.
The actual device API and ABI are checked before comparing. Animated Basic GPU
pages require FPS and retain captures; Gallery's static GPU gradient stays strict.

## Coverage and diagnostics

Basic/Gallery samples have build coverage; Console/Web APIs and Web/WASM/Blazor
UIs have runtime checks, including Blazor Drawing pointer strokes, color/brush
selection and clear. Basic Uno runs its declared `net10.0-browserwasm` project
target, checking CPU rendering and Drawing navigation, pointer strokes and
clear rather than launching a desktop or host solution variant.
Gallery also has device execution through the separate `Ios` and `Android` groups.
Browser errors and unavailable GPU rendering fail.

| Group | Tests | CI hosts |
| --- | --- | --- |
| Build | Generated sample compilation and infrastructure checks | Windows, macOS, Linux |
| Host | Console, Web API and browser sample execution | Windows, macOS, Linux |
| Docker | Container image builds and sample execution | Windows, Linux |
| Ios | Basic MAUI/native iOS and Gallery MAUI Appium execution | macOS with the configured simulator profiles |
| Android | Basic MAUI/native Android and Gallery MAUI Appium execution | Configured Android test pool with the two selected emulator profiles |

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
Tests and theory rows within each class/collection remain serial.
Each test requires exactly one typed attribute, such as
`[RunGroup(SampleRunGroup.Build)]`. Apply it to a class only when every test uses
the same group; otherwise annotate individual methods.

Find output, binlogs, results and images under `output/logs/testlogs/samples/`
or CI's `sample_logs_*` artifacts. For restore failures, check the generated
versions against staged packages. Do not clear shared caches.

Review references under `tests/Goldens/Samples/`.
Dimensions must match; the default limit is **0.075% differing pixels** with
zero channel tolerance. Existing browser animated GPU pages use a loose
**6% mean RGBA error** smoke check that can miss uniform dark output; iOS/Android
animated Basic GPU pages instead require FPS and retain a capture. Mismatches
retain actual/diff images; references are never accepted automatically.

`SampleImage.Validate` takes `actual`, a typed golden request (or an absolute
reference path for infrastructure fixtures), and optional `SampleImageOptions`.
It resolves once before comparison/retries and reports the selected key and
ordered lookup chain. A differing selected image cannot trigger looser lookup.
Its byte-array overload also takes
the actual output path, saves the bytes, then calls the file overload.
Set `ExpectedSize = new(800, 600)` for
fixed-size PNGs; only animated GPU checks set `MaxAverageColorErrorFraction`.
The options own the default `MaxErrorPixelFraction` of `0.00075m`.
Omitting options retains the strict pixel comparison and golden dimensions.
