# Building and validating samples

The sample suite builds generated applications against produced NuGet packages
and runs supported Console, Web API, browser and Docker samples.

## Prepare packages

Install the repository SDK and required workloads. Docker tests require a
running daemon in Windows-container mode on Windows or Linux-container mode on
macOS/Linux.

Bootstrap before staging packages because bootstrap resets `output/`:

```sh
# Managed-only changes
dotnet cake --target=externals-download
```

Native or Skia changes must build natives from source instead; see
[AGENTS.md](../../AGENTS.md).

Stage all non-symbol packages from one producing build in `output/nugets/`.
Package base versions must match `scripts/VERSIONS.txt`. To download a PR build:

```powershell
$pr = 1234
pwsh scripts/get-skiasharp-pr.ps1 $pr -SuccessfulOnly -Force
New-Item output/nugets -ItemType Directory -Force | Out-Null
Copy-Item "$HOME/.skiasharp/hives/pr-$pr/packages/*.nupkg" output/nugets/
```

Run the suite with the package suffix:

```sh
dotnet cake --target=samples --previewLabel=pr.1234 --buildNumber=26509.10
```

Use `--runGroup=Build`, `--runGroup=Host` or `--runGroup=Docker` for one
execution group. Omit it for the full suite.

## IDE and direct runs

Generate the sample projects once after staging packages:

```sh
dotnet cake --target=samples-generate --previewLabel=pr.1234 --buildNumber=26509.10
dotnet test tests/SkiaSharp.Tests.Samples.slnx \
  -p:TargetFramework=net10.0 -p:TargetFrameworks=net10.0 -- --report-trx
```

Keep both framework properties so the runner uses only .NET 10 dependencies.
Sample projects still build their declared target frameworks.

`BuildSample`, `DotNet.Build` and `DotNet.Run` accept an optional
`DotNetOptions` record for framework, configuration, additional properties,
working directory and timeouts.

## Browser runs

Build the runner and install its matching Chromium:

```sh
dotnet build tests/SkiaSharp.Tests.Samples/SkiaSharp.Tests.Samples.csproj
pwsh tests/SkiaSharp.Tests.Samples/bin/Debug/net10.0/playwright.ps1 \
  install --with-deps chromium
dotnet test tests/SkiaSharp.Tests.Samples/SkiaSharp.Tests.Samples.csproj \
  -- --filter-trait Category=Browser --report-trx
```

Use the Release script path for Release builds. CI installs Chromium for Host
and unfiltered runs. Filtered runs do not replace the full suite.

## Mobile Appium runs

The `Ios` group runs the Basic MAUI/native and Gallery MAUI applications
against staged packages.

- `--appiumMode=build` cleans and builds apps without a device, then retains
  their output.
- `--appiumMode=run` requires the existing apps, executes without rebuilding
  them, and cleans the selected app workspaces after all scenarios.
- `--appiumMode=all` (default) cleans, builds, runs, and cleans afterward.

App workspaces use predictable paths under
`output/sample-apps/{platform}/{architecture}/{sample-folder}/`; their normal
`samples/.../bin/Debug/` directories contain the built apps. Both phases use
`dotnet test` to build and execute the host-side tests; only build/all mode
invokes sample-app builds.
Use a matching host platform and architecture and one producing package set.
Run mode never falls back to a sample-app build.

Build and run separately on the same machine by passing `--appiumMode=build`
and then `--appiumMode=run` to the same Cake command below. No archive or
artifact transfer is needed locally. Apps shared by multiple scenarios remain
available until the Appium test collection finishes. Diagnostics are retained
separately under `output/logs/testlogs/samples/`, including after failures.

### iOS simulators

Select Xcode, install the selected runtime and the pinned Appium 3 XCUITest
driver with its prebuilt WDA, then run one declared profile:

```sh
IOS_VERSION=18.6
dotnet tool restore
sudo scripts/infra/native/apple/select-xcode.sh 26.3
pwsh scripts/infra/tests/install-appium.ps1 -Platform iOS
pwsh scripts/infra/managed/install-ios-runtime.ps1 -Version "$IOS_VERSION"
dotnet cake --target=samples --runGroup=Ios --iosVersion="$IOS_VERSION" \
  --iosModel="iPhone 16 Pro" --skipExternals=all \
  --previewLabel=pr.1234 --buildNumber=26509.10
```

CI runs both the declared iOS 18.6 profile and `latest` (the selected Xcode's
installed runtime). Cake creates its simulator in `Setup` for run/all mode and
deletes only that simulator in `Teardown`, including after a failure.
Build-only, sample-generation, and listing/dry-run commands do not start a
device. CI builds apps in a separate job; local all-mode runs may keep the
simulator booted while apps build.

CI archives the app workspaces into `.tar.gz` files, preserving executable
permissions and symlinks. Build-only `cache` and `obj` directories are excluded
from the app archive. Runtime jobs
download the apps from that build job, extract the folders to the same layout,
and use `dotnet test` to build the host-side test project and run the scenarios.
Runtime jobs need the native/package inputs for the host tests, but no mobile
workloads or sample-app builds. CI acquires apps from the build job in the same
pipeline run.
Reviewed references cover 18.6.0, 26.2.0, 26.3.1, 26.5.0 and versionless Gallery
cases. `latest` may select a newer, unreviewed runtime; missing references still
fail strictly. Capture and review them rather than accepting an unmatched image.

## Test groups and diagnostics

| Group | Coverage | CI hosts |
| --- | --- | --- |
| Build | Generated sample compilation and infrastructure checks | Windows, macOS, Linux |
| Host | Console, Web API and browser execution | Windows, macOS, Linux |
| Docker | Container builds and execution | Windows, Linux |
| Ios | Basic MAUI/native iOS and Gallery MAUI Appium execution | Configured macOS simulator pools |

Choose a `RunGroup` by execution target. Each test requires exactly one typed
attribute such as `[RunGroup(SampleRunGroup.Build)]`; place it on the class only
when every test uses the same group.

Keep build, run and infrastructure checks in the existing purpose-based test
classes. Browser and Docker workloads use their existing shared collections.

Find output, binlogs, results and images under
`output/logs/testlogs/samples/` or CI's `sample_logs_*` artifacts. For restore
failures, confirm the generated versions match the staged package set.

## Sample golden images

References live under `tests/Goldens/Samples/{profile}/`. Host profiles are
`mac`, `linux` and `windows`; Docker uses `docker-linux` and `docker-windows`.
Cases use paths such as `Console/output`, `Web/page` and
`Blazor/drawing-drawn`.

iOS profiles use optional max-version directories:

- `iphone-16-pro/18.6.0/maui-basic-cpu.png`

Architecture-specific files override the unsuffixed reference. Appium captures
the selected canvas element, not necessarily the full device screen; image
dimensions therefore follow the element screenshot returned by the driver.
Those PNG bytes are saved unchanged. The shared Golden verifier transforms
decoded copies of actual and reference images only during comparison.
The iOS 39-pixel bottom inset applies only to the declared 1206 x 2622 profile;
shorter element captures are not cropped again. Diagnostics and reference
proposals retain the original captured images; only diff images use the
comparison dimensions.
Initial, drawn and cleared states use separate case names where needed.

Use the sample helpers to select the profile:

```csharp
SampleImage.Host("Console/output").Verify(
    GoldenInput.Path(actualPath),
    new(expectedSize: new(800, 600)));

SampleImage.Host("Web/output").Verify(
    GoldenInput.Bytes(png, "output.png"),
    new(expectedSize: new(512, 512)));

await SampleImage.Browser("Blazor/cpu").VerifyAsync(
    GoldenInput.Capture(app.Screenshot),
    cancellationToken: TestContext.Current.CancellationToken);
```

Samples use zero channel tolerance and allow 0.075% differing pixels by
default. Animated GPU pages may use a 6% mean RGBA error limit. Browser
references use the operating-system architecture because Playwright selects its
bundled browser for the host hardware independently of the managed process
architecture.

See [golden-image tests](golden-image-tests.md) for lookup, packaging,
diagnostics and reference review.
