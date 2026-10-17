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
using Appium and XCUITest. Install the SDK, workloads and Xcode versions specified
in the [shared CI variables](../../scripts/azure-templates-variables.yml), along
with Node.js and the Appium prerequisites in the
[sample stage configuration](../../scripts/azure-templates-stages-samples.yml).
Multi-target projects need their restore workloads even when building only iOS.

CI runs a pinned runtime and a moving `latest` golden profile; see the
[sample stage configuration](../../scripts/azure-templates-stages-samples.yml).
`latest` means the newest compatible **installed** runtime selected by
`dotnet apple simulator create` under the configured Xcode, not the newest
runtime available to download. CI provisions the pinned runtime explicitly; the latest leg
requires a compatible runtime already installed and does not run a version
installer.

Set `XCODE_VERSION` to the configured Xcode version and choose `latest` or a
pinned runtime for `IOS_VERSION`. Use the repository's Xcode selection script
before provisioning a pinned runtime:

```sh
IOS_VERSION=latest # Or 18.6 for the pinned profile.
sudo scripts/infra/native/apple/select-xcode.sh "$XCODE_VERSION"
pwsh scripts/infra/tests/install-appium.ps1
if [ "$IOS_VERSION" != latest ]; then
  pwsh scripts/infra/managed/install-ios-runtime.ps1 -Version "$IOS_VERSION"
fi
dotnet cake --target=samples --runGroup=Ios --iosVersion="$IOS_VERSION" \
  --iosSimulatorModel="iPhone 16 Pro" \
  --sampleRunId=ios --skipExternals=all --previewLabel=pr.1234 --buildNumber=26509.10
```

Use your staged packages' suffix, not the example suffix. For samples, omitting
`--iosVersion` (with no version environment variable) also selects `latest`.
The pinned installer provisions and verifies the runtime version; tests require
reviewed image references.
Cake creates and boots one simulator using `dotnet apple`, passes its exact UUID
and actual runtime version from the creation JSON to the serialized iOS tests,
and removes only that simulator at teardown, including after failures.
The golden profile stays `latest` even when the selected runtime changes.
The same lifecycle helpers serve the DeviceRunners iOS target, whose explicitly
configured CI runtime and model remain unchanged.
`samples-generate` does not start a simulator. Each test builds its generated sample
and owns a fresh Appium server/session that reinstalls and launches the app without
erasing or rebooting the shared simulator. Appium manages WebDriverAgent.

Device and sample runs use `--iosVersion` and `--iosSimulatorModel`, backed by
`TEST_IOS_SIMULATOR_VERSION` and `TEST_IOS_SIMULATOR_MODEL`. The model is a device
type such as `iPhone 16 Pro`, not the unique name of a created simulator.
For direct test or IDE runs, set those environment variables to select the
runtime and device type. Set `TEST_IOS_SIMULATOR_UDID` to use an already-running simulator
you manage yourself. If unset, the `appium:udid` capability is omitted and Appium
selects or creates a simulator using the model and version. With no version or
`latest`, direct runs omit `appium:platformVersion` and use `latest` references.
When selecting a concrete runtime directly but comparing against the moving
profile, also set `TEST_IOS_SIMULATOR_REFERENCE_VERSION=latest`. Cake sets that
reference variable before exporting the resolved actual version; pinned runs use
their requested numeric reference folder. C# tests do not manage simulator
lifetimes or install runtimes. Unsupported runtimes and missing reviewed references
fail without fallback.

Coverage includes page navigation, CPU/GPU rendering, drawing and Clear, and
Gallery search, filtering and controls. For failures, inspect the TRX attachments
and `samples_appium_*_logs` CI artifacts for server/device logs, UI hierarchy and
screenshots. Missing or mismatched references fail; tests never approve them.
Cake's creation JSON records the actual selected runtime; the actual
`platformVersion` returned by the Appium session and its golden profile are
written to test output.

### Updating latest references

References live under `Expected/Appium/<Maui|Native>/iOS/<model>/<version>/` in
the sample test project. `latest/` moves with the selected runtime; numeric
folders remain pinned. Both use the same strict comparisons.

When a runtime update changes rendering, inspect the actual/diff captures and
runtime reported in the test output. Fix regressions before accepting new images.
For intentional changes, optionally copy the previous reviewed `latest/` folders
to their source runtime version without overwriting existing pinned references.
Then replace only reviewed scene PNGs in `latest/` with actual canvas captures
from the TRX attachments, removing their sequence prefixes. Do not use diff,
full-screen diagnostic or animated GPU captures as references.

Re-run latest and the pinned profile against the same packages before committing.
Record the actual runtime, package build and reason for the image changes in the
PR. Tests never approve references automatically or fall back to another folder.

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
