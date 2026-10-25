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

- `--appiumMode=build` prepares apps and the compiled runner without a device.
- `--appiumMode=run` runs those prepared artifacts without building.
- `--appiumMode=all` (default) builds before creating the simulator.

Prepared apps and runners are under `output/appium-apps/` and
`output/appium-runner/`. Keep them with the exact producing `output/nugets`
packages and use a matching host platform and architecture.

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
installed runtime). Cake creates and deletes only its own simulator. Build
jobs publish prepared apps and a checksummed runner; runtime jobs download
those artifacts and execute without rebuilding the test project.
`latest` is not guaranteed to pass: references exist for 18.6.0, 26.2.0,
26.3.1 and versionless Gallery cases, but not 26.5. Missing references fail
strictly; capture and review them rather than accepting an unmatched image.

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

Architecture-specific files override the unsuffixed reference. iOS references
retain raw captures; comparison removes only the declared system-owned inset.
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
