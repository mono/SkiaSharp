# Building and Validating Samples

The Samples suite builds generated applications against produced NuGet packages,
runs Console/Web APIs on the host and in Docker, and checks actual browser UIs.

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

This generates the samples and runs the suite. Do not mix producing builds:
generation uses the supplied arguments, not package-filename version detection.

## IDE and direct test runs

After staging packages, generate once before opening the test project in an IDE
or running it directly:

```sh
dotnet cake --target=samples-generate --previewLabel=pr.1234 --buildNumber=26509.10
dotnet test tests/SkiaSharp.Tests.Samples.slnx \
  -p:TargetFramework=net10.0 -p:TargetFrameworks=net10.0 -- --report-trx
```

The solution includes the runner's source dependencies; the properties above
limit their build to the runner's .NET 10 target.

Direct tests use the generated inputs without invoking Cake or downloading
packages. The runner uses the repository SDK; sample builds use `dotnet` from
the inherited `PATH`, without changing project TFMs. Set the intended SDK and
platform environment before launching the terminal or IDE.

## Coverage and diagnostics

Build theories cover eligible Basic and Gallery solutions; Gallery remains
build-only. Named runtime facts check host and Docker Console exit codes,
output and PNGs. Basic Web and Docker Web API retain their HTTP/image tests
without a browser; Basic Web also has a separate page screenshot test.
Browser facts cover host Web, WASM and Blazor UIs. Device execution is not part
of this suite. Docker retains the samples' original .NET 10 images; its SDK is
independent of the host SDK.
Generated-project tests check exact package versions and Uno's SkiaSharp override.
Integration/MSBuild suites remain separate.

Use IDE filters or select one test:

```sh
dotnet test tests/SkiaSharp.Tests.Samples/SkiaSharp.Tests.Samples.csproj \
  -- --filter-method '*BasicSampleTests.WebSample*'
```

Find stdout/stderr, binlogs, results, and images under
`output/logs/testlogs/samples/`, or in CI's `sample_logs_*` artifact. For restore
failures, compare generated versions with the staged packages and inspect the
binlog. Tests use private workspaces/caches; do not clear shared caches.

PNG references are platform-specific under `tests/SkiaSharp.Tests.Samples/Expected/`.
Dimensions must match; at most **0.075%** of pixels may differ, with zero channel
tolerance, except for the animated GPU page described below.
Failures retain the actual image; mismatches also retain a diff.
Review replacement references on the affected platform; tests never accept them
automatically.

## Browser runs

After bootstrap, package acquisition and generation, explicitly install the
Chromium revision matching the runner's Playwright 1.55.0 dependency:

```sh
dotnet build tests/SkiaSharp.Tests.Samples/SkiaSharp.Tests.Samples.csproj
pwsh tests/SkiaSharp.Tests.Samples/bin/Debug/net10.0/playwright.ps1 install --with-deps chromium
dotnet test tests/SkiaSharp.Tests.Samples/SkiaSharp.Tests.Samples.csproj \
  -- --filter-trait Category=Browser --report-trx
```

Use the Release script path for a Release runner build. CI installs Chromium
explicitly too; tests never download browsers. On Linux, `--with-deps` may
require elevated privileges. Missing browsers, browser errors and unavailable
GPU rendering fail rather than silently skipping coverage.

Named facts in the existing Basic test class check browser console errors and
compare full-page screenshots using `SampleImage`. They cover Web,
BrowserWASM and Blazor CPU/GPU. DockerWebApi has no web UI and retains its
HTTP/PNG test rather than capturing Chromium's built-in image viewer.
Captures use a fixed 1280 x 900 viewport and device scale 1.
The BrowserWASM app must include the matching artifact's
`SkiaSharp.NativeAssets.WebAssembly` package; the source-built runner cannot
supply the sample's dependencies.

Page references use the existing `Expected/Host/<host-platform>/` directory.
Page screenshots are distinct from the existing headless PNG references and
need their own capture/review.

The four initial page references were reviewed locally on macOS 27.0.1
(26A434), using Playwright 1.55.0 and the exact package cohort from producing
[build 1628893](https://dev.azure.com/dnceng-public/public/_build/results?buildId=1628893):
SkiaSharp `4.156.0-pr.5291.26508.18` and HarfBuzzSharp
`14.4.0.100-pr.5291.26508.18`. These are local-first references, not producing
browser CI proof. Later matching CI captures need review before any reference
update; tests never replace references automatically.

Only the animated GPU page allows **6% average RGBA color error**: the sum of
absolute channel differences divided by `pixel count * 4 * 255`. No channel
differences are discarded. This is not the percentage of changed pixels and
does not change the shared 0.075% differing-pixel policy for other pages and
headless samples. Dimensions must still match.

Against the committed local GPU reference, 24 actual frames across two page
loads measured 0.869-5.641% average color error. White and pure-black canvas
controls measured 50.273% and 6.331%, so 6% accepts the observed animation and
rejects those controls; exactly 5% rejected healthy frames. Uniform dark output
measured 4.067% and can still pass. This is deliberately a crash/white-output
smoke check, not exact shader regression coverage. CI-to-local calibration and
stronger animated comparison remain follow-up work; animation, clock and FPS
are unchanged.

Browser filtering does not replace the unfiltered sample suite; stacked PR
branch filters may prevent producing CI captures until the target is eligible.
