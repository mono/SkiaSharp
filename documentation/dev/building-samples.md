# Building and Validating Samples

The Samples suite builds generated applications against produced NuGet packages,
runs Console/Web APIs on the host and in Docker, and checks browser and iOS UIs.

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
`--runGroup=Docker`, or `--runGroup=Ios`. Omit `--runGroup` to run the full suite,
including iOS simulator cases on macOS.
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
pwsh scripts/infra/tests/install-appium.ps1
pwsh scripts/infra/managed/install-ios-runtime.ps1 -Version "$IOS_VERSION"
dotnet cake --target=samples --runGroup=Ios --iosVersion="$IOS_VERSION" \
  --iosModel="iPhone 16 Pro" \
  --skipExternals=all --previewLabel=pr.1234 --buildNumber=26509.10
```

Use your staged packages' suffix, not the example suffix. Each runtime uses its
matching numeric reference folder. Missing or mismatched references fail without fallback.
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

References are in `tests/SkiaSharp.Tests.Samples/Expected/Appium/<Maui|Native>/iOS/<model>/`.
Review actual/diff captures before replacing an intentional change. Use the
canvas PNG from the TRX attachment, not full-screen diagnostics or animated GPU
captures, then rerun every configured profile against the same package build.

## Coverage and diagnostics

Basic/Gallery samples have build coverage; Console/Web APIs and Web/WASM/Blazor
UIs have runtime checks, including Blazor Drawing pointer strokes, color/brush
selection and clear. Basic Uno runs its declared `net10.0-browserwasm` project
target, checking CPU rendering and Drawing navigation, pointer strokes and
clear rather than launching a desktop or host solution variant.
Gallery also has iOS device execution through the separate `Ios` group.
Browser errors and unavailable GPU rendering fail.

| Group | Tests | CI hosts |
| --- | --- | --- |
| Build | Generated sample compilation and infrastructure checks | Windows, macOS, Linux |
| Host | Console, Web API and browser sample execution | Windows, macOS, Linux |
| Docker | Container image builds and sample execution | Windows, Linux |
| Ios | Basic MAUI/native iOS and Gallery MAUI Appium execution | macOS with the configured simulator profiles |

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

Review platform references under `tests/SkiaSharp.Tests.Samples/Expected/`.
Dimensions must match; the default limit is **0.075% differing pixels** with
zero channel tolerance. Existing browser animated GPU pages use a loose
**6% mean RGBA error** smoke check that can miss uniform dark output; iOS
animated Basic GPU pages instead require FPS and retain a capture. Mismatches
retain actual/diff images; references are never accepted automatically.

`SampleImage.Validate` takes `actual`, `expected` (or a relative golden path),
and an optional `SampleImageOptions` record. Its byte-array overload also takes
the actual output path, saves the bytes, then calls the file overload.
Set `ExpectedSize = new(800, 600)` for
fixed-size PNGs; only animated GPU checks set `MaxAverageColorErrorFraction`.
The options own the default `MaxErrorPixelFraction` of `0.00075m`.
Omitting options retains the strict pixel comparison and golden dimensions.
