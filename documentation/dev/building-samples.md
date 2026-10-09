# Building and Validating Samples

The Samples test suite builds the actual sample applications against produced
NuGet packages, rather than repository project references. It runs through Cake,
`dotnet test`, or an IDE.

## Get started

Install the repository's SDK and the workloads required by your host's samples.
Runtime tests also require Docker: Windows containers on Windows, Linux
containers on macOS/Linux.

**Bootstrap before staging packages.** For managed-only work, run:

```sh
dotnet cake --target=externals-download
```

This supplies native binaries for the test runner and resets `output/`. For
native or Skia submodule changes, build natives from source instead; see
[the repository instructions](../../AGENTS.md). Sample generation does not
download packages or bootstrap natives.

Stage one matching, non-symbol package cohort in `output/nugets/`: SkiaSharp,
HarfBuzzSharp, and their native/view packages needed by the samples. Choose a
producing build matching the current source versions in `scripts/VERSIONS.txt`.
For PR packages, the repository helper downloads a successful build:

```powershell
$pr = 1234 # Replace with the producing PR number.
pwsh scripts/get-skiasharp-pr.ps1 $pr -SuccessfulOnly -Force
New-Item output/nugets -ItemType Directory -Force | Out-Null
Copy-Item "$HOME/.skiasharp/hives/pr-$pr/packages/*.nupkg" output/nugets/
```

Alternatively, extract the producing build's public `nuget` pipeline artifact.
Promoted builds also publish a `_NuGets` wrapper to the public
[transport feed](https://pkgs.dev.azure.com/dnceng/public/_packaging/dotnet-libraries-transport/nuget/v3/index.json);
use the real package versions inside it, not the wrapper's version.

Generation uses the exact staged SkiaSharp/HarfBuzzSharp package versions,
independently of the job's date or build counter:

```sh
dotnet cake --target=samples
```

Do not mix packages from different builds. Without staged packages,
`samples-generate` retains its publication behavior using source versions and
the requested `--previewLabel`/`--buildNumber`.

To use an IDE or invoke the suite directly, generate first:

```sh
dotnet cake --target=samples-generate
dotnet test tests/SkiaSharp.Tests.Samples.slnx -- --report-trx
```

The generated inputs remain available for subsequent IDE/test runs. Direct
tests do not invoke Cake or acquire packages.

## How it works

`samples-generate` copies samples into `output/samples/` and
`output/samples-preview/`, converts repository project references to exact
package references, and removes external projects from sample solutions.
The acquired package identity selects the appropriate generated tree.

Each test copies only its selected sample and required ancestor build/NuGet
configuration into an owned workspace. Gallery also needs its shared sibling
projects. Host-specific `.Windows`, `.Mac`, and `.Linux` solutions select the
appropriate projects without changing their TFMs.

**The repository host and sample consumer are separate.** The test runner is a
normal `net10.0` repository project using the repository SDK pin and native
imports. Consumer builds use `dotnet` from the inherited `PATH`, a host-only
`global.json`, private NuGet/CLI caches, and import fences that exclude repository
build targets and SDK pins. SkiaSharp/HarfBuzzSharp packages resolve only from
the staged artifact directory.

A consumer installation can contain multiple SDKs; normal SDK selection chooses
among them, including previews. To test a specific SDK, select its host
installation explicitly. A newer SDK does not itself retarget the sample or
replace the runtime needed to run it. The suite installs no SDKs/workloads and
does not modify source samples or user caches. IDEs must inherit the intended
environment too.

Current CI has one Samples job per Windows/macOS/Linux host using the .NET 10
SDK. It does **not** yet run separate .NET 10 and .NET 11 lanes. CI supplies the
matching `native` and `nuget` artifacts before generation.
The macOS job provisions an owned Colima/QEMU Docker daemon without requiring
nested virtualization or changing the default Docker context, then deletes it.

## Coverage and diagnostics

Build theories cover eligible Basic and Gallery solutions; Gallery remains
build-only. Named runtime facts check host and Docker Console exit codes,
output and PNGs. Docker Web API checks use HTTP health and image responses,
without a browser; browser facts cover host Web, WASM and Blazor UIs. Device
execution is not part of this suite. Docker retains the samples' original
.NET 10 images; its SDK is independent of the host SDK.

Use ordinary IDE filters or select a test directly:

```sh
dotnet test tests/SkiaSharp.Tests.Samples/SkiaSharp.Tests.Samples.csproj \
  -- --filter-method '*BasicSampleTests.WebSampleRuns'
```

`-- --filter-trait Category=Infrastructure` selects helper tests only; it does
not validate actual samples. Missing inputs, empty discovery, and unavailable
Docker fail rather than silently removing coverage.

Logs, binlogs, test results, and images remain under
`output/logs/testlogs/samples/basic-<sample>-<guid>/`, outside disposable
workspaces. Tests retain separate stdout/stderr and attach diagnostic files;
CI always publishes the platform's `sample_logs_*` artifact. Commands have
bounded timeouts and cleanup removes only owned processes, workspaces, images,
and containers, never user caches or unrelated Docker resources.

PNG references live in `tests/SkiaSharp.Tests.Samples/Expected/`, qualified by
host/container platform. Headless comparisons allow at most **0.075%** of decoded pixels
to differ, with zero per-channel tolerance: any RGBA channel change counts as a
differing pixel. Fractional pixel budgets round down (360 pixels at 800x600;
196 at 512x512). This small image-wide budget accommodates host text
antialiasing differences without allowing color shifts across the whole image.
Image dimensions must still match exactly.
Missing references retain the actual image and fail; mismatches also retain a
diff. Capture and review references on their actual platform: tests never
generate or accept them automatically.

Host/Linux references were reviewed from `sample_logs_linux` in producing
[build 1629334](https://dev.azure.com/dnceng-public/public/_build/results?buildId=1629334)
at source `d1ef43a8`; all four attempts produced identical Console/Web PNGs.
Host/Windows and Docker/Windows references were reviewed from
`sample_logs_windows` in [build 1629446](https://dev.azure.com/dnceng-public/public/_build/results?buildId=1629446)
at source `9a1e2422`; all four attempts produced identical captures for each case.

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

For restore failures, compare the generated references with the staged package
cohort and inspect the retained binlog. Each consumer already has fresh caches;
clearing shared caches is unnecessary. The existing Integration and MSBuild
test suites remain separate.

Generation regressions can be checked independently with
`pwsh scripts/infra/samples/tests/SampleGeneration.Tests.ps1`; its fixture outputs
are private and the ordinary Samples test suite never invokes Cake.
