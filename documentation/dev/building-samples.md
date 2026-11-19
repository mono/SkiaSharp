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

Use `--appiumMode=build` to build without starting a simulator, then
`--appiumMode=run` to test those apps. Use the same host architecture and
package set for both commands. Run mode fails if the apps are missing.

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

For iOS 26.5, use `IOS_VERSION=26.5` and `IOS_BUILD=23F77`. Installation fails
if that exact runtime build is unavailable. The run command creates a temporary
simulator and deletes it afterward.

CI requires all four scenarios to pass without skips on both runtimes.

## Test groups and diagnostics

| Group | Coverage | CI hosts |
| --- | --- | --- |
| Build | Generated sample compilation and infrastructure checks | Windows, macOS, Linux |
| Host | Console, Web API and browser execution | Windows, macOS, Linux |
| Docker | Container builds and execution | Windows, Linux |
| Ios | Basic MAUI/native iOS and Gallery MAUI Appium execution | Configured macOS simulator pools |

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

Review the raw capture and diff in the test results before changing a reference.
iOS captures show the canvas, not necessarily the whole screen. Comparison
temporarily excludes the top 200 and bottom 322 pixels of full-height iPhone
16 Pro captures to avoid host-dependent system chrome. Canvas-only captures and
saved images are not cropped. Missing references fail and require manual review.

Samples use zero channel tolerance and allow 0.075% differing pixels by
default. Animated GPU pages may use a 6% mean RGBA error limit.

See [golden-image tests](golden-image-tests.md) for lookup, packaging,
diagnostics and reference review.
