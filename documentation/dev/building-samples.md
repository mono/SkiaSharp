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

This generates the samples and runs the suite with live test output. Use the
suffix of the staged packages; do not mix producing builds.

## IDE and direct test runs

After staging packages, generate once before opening the test project in an IDE
or running it directly:

```sh
dotnet cake --target=samples-generate --previewLabel=pr.1234 --buildNumber=26509.10
dotnet test tests/SkiaSharp.Tests.Samples.slnx \
  -p:TargetFramework=net10.0 -p:TargetFrameworks=net10.0 -- --report-trx
```

Direct tests do not generate samples or download packages. Keep the `net10.0`
properties above to avoid building the runner's dependencies for every platform.

To build samples with a specific SDK, choose a version from `dotnet --list-sdks`
and set `SAMPLE_TEST_SDK_VERSION` before starting the tests or IDE. For example,
in PowerShell:

```powershell
$env:SAMPLE_TEST_SDK_VERSION = '10.0.401' # Must be installed.
dotnet test tests/SkiaSharp.Tests.Samples.slnx `
  -p:TargetFramework=net10.0 -p:TargetFrameworks=net10.0 -- --report-trx
Remove-Item Env:SAMPLE_TEST_SDK_VERSION
```

This selects the exact sample-build SDK, not the runner SDK or sample TFM.
Without it, normal host SDK selection applies. A missing selected SDK fails.

CI tests SDK10 and SDK11 on Windows, macOS, and Linux. Baseline TFMs remain
unchanged; SDK11 also runs an owned Console copy targeting `net11.0`.
Docker images still use .NET 10 regardless of the host SDK.

## Coverage and diagnostics

The suite builds host-eligible Basic and Gallery solutions. Gallery, WASM, and
Blazor are build-only; Console/Web runtime tests check exit codes, HTTP responses,
and PNGs on the host and in Docker. Generated-project tests check package versions.

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
