# Building and Validating Samples

The Samples suite builds generated applications against produced NuGet packages,
runs Console/Web APIs on the host and in Docker, and checks actual browser UIs.

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

The `Ios` group builds the generated, package-consuming Basic MAUI, native iOS,
and Gallery MAUI projects with their declared `net10.0-ios` target and a
host-architecture simulator RID. It does not retarget samples or use a test app.
Use SDK **10.0.402**, workload set **10.0.401**, Xcode **26.6** on macOS Tahoe,
Node.js 24, Appium 3 and XCUITest. The unchanged multi-target MAUI projects
still need their normal restore workload closure.

Select Xcode with process-local `DEVELOPER_DIR` and `MD_APPLE_SDK_ROOT`, not
`xcode-select`. Provision the selected runtime before running tests:

```sh
export DEVELOPER_DIR=/Applications/Xcode_26.6.app/Contents/Developer
export MD_APPLE_SDK_ROOT=/Applications/Xcode_26.6.app
pwsh scripts/infra/tests/install-appium.ps1
pwsh scripts/infra/managed/install-ios-runtime.ps1 -Version 18.6 -BuildVersion 22G86
SAMPLE_TEST_IOS_VERSION=18.6 dotnet cake --target=samples --runGroup=Ios \
  --sampleRunId=ios18_6 --skipExternals=all --previewLabel=pr.1234 --buildNumber=26509.10
```

Use your staged cohort's suffix, not the example suffix. The only reviewed
device profiles are **iPhone 16 Pro / iOS 18.6 (22G86)** and
**iPhone 16 Pro / iOS 26.5 (23F77)**; use the corresponding version/build for the
second run. The runner checks the exact available runtime and never installs it.
Each test creates a fresh simulator UUID and owns its Appium server, session,
WDA build directory, shutdown and deletion. It neither reuses another daemon nor
shuts down unrelated simulators. Startup, diagnostic and cleanup failures are
aggregated rather than masking the original failure.

Each profile runs four scenarios: Basic MAUI CPU/GPU/Drawing navigation; native
iOS CPU/GL/Metal/Drawing navigation; MAUI drawing strokes and Clear; and Gallery
search, category filter, Gradient controls, back navigation and CPU/GPU rendering.
Static canvases use the reviewed PNGs under `Expected/Appium/` with at most two
seconds of settling and the unchanged strict comparer. Missing references fail.
Drawing also checks each stroke region independently before and after Clear.
Animated Basic GPU canvases require populated FPS labels and attach screenshots;
they are not frozen or compared with a relaxed tolerance.

CI adds two strict jobs to the existing Samples stage, using the same native
and NuGet producing cohort as the other sample jobs. Only these jobs select the
newer SDK/workload set and Tahoe host; existing build, host and Docker jobs keep
their pools and SDK settings. These jobs publish `samples_appium_ios*_logs`
artifacts and TRX results. No reference capture or automatic approval is supported.

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
| Ios | Basic MAUI/native iOS and Gallery MAUI Appium execution | macOS Tahoe, iOS 18.6 and 26.5 |

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
