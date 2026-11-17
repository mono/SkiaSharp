# Building and validating samples

The sample suite builds generated applications against produced NuGet packages
and runs Console, Web API, browser, Docker and iOS samples.

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

Apps are retained under
`output/sample-apps/{platform}/{architecture}/{sample-folder}/`.
Use the same host architecture and package set for both phases. Run mode never
rebuilds missing apps. Logs and captures remain under
`output/logs/testlogs/samples/` after workspace cleanup.

### iOS simulators

Build with Xcode 26.3 for the pinned .NET iOS workload, then select Xcode 26.6
for simulator execution. For iOS 18.6:

```sh
IOS_VERSION=18.6
IOS_BUILD=22G86
dotnet tool restore
sudo scripts/infra/native/apple/select-xcode.sh 26.3
dotnet cake --target=samples --runGroup=Ios --appiumMode=build \
  --skipExternals=all --previewLabel=pr.1234 --buildNumber=26509.10
sudo scripts/infra/native/apple/select-xcode.sh 26.6
pwsh scripts/infra/tests/install-appium.ps1 -Platform iOS
pwsh scripts/infra/managed/install-ios-runtime.ps1 \
  -Version "$IOS_VERSION" -BuildVersion "$IOS_BUILD"
dotnet cake --target=samples --runGroup=Ios --appiumMode=run \
  --iosVersion="$IOS_VERSION" --iosModel="iPhone 16 Pro" --skipExternals=all \
  --previewLabel=pr.1234 --buildNumber=26509.10
```

For iOS 26.5, use `IOS_VERSION=26.5` and `IOS_BUILD=23F77`. Runtime installation
fails if the requested build is unavailable or mismatched. Cake creates and
deletes only its own simulator, including after failure; build-only and dry-run
commands do not start one.

CI uses the AcesShared Tahoe image for the dedicated iOS jobs. One build job
archives the apps; two runtime jobs consume that archive from the same pipeline
run, preserving permissions and symlinks. Runtime jobs compile the host tests
but do not build apps or install mobile workloads. Each exact runtime must pass
all four scenarios with zero skips.

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

iOS references include a runtime directory, for example
`iphone-16-pro/18.6.0/maui-basic-cpu.png`; shared Gallery cases may omit it.

Architecture-specific files override the unsuffixed reference. Appium saves
the canvas element's PNG unchanged, so captures need not have full-screen
dimensions. Comparison excludes the 39-pixel bottom band only for the declared
1206 x 2622 iPhone profile; shorter captures are not cropped again. Captures and
reference proposals remain raw; only diff images use comparison dimensions.
Missing references fail and require review, never automatic approval.

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
