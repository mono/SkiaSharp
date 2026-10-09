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
build-only. Separate browser facts execute Web, WASM, and Blazor; device
execution is not part of this suite. Other runtime
tests run host Console/Web and Docker Console/Web, checking exit codes, output,
HTTP responses, and rendered PNGs. Docker retains the samples' original .NET 10
images; its SDK is independent of the host SDK.

Use ordinary IDE filters or select a test directly:

```sh
dotnet test tests/SkiaSharp.Tests.Samples/SkiaSharp.Tests.Samples.csproj \
  -- --filter-method '*BasicSampleTests.WebSampleReturnsImage'
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
host/container platform. Comparisons allow at most **0.075%** of decoded pixels
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

Four named facts check the actual Web page's three 512 x 512 images,
BrowserWASM's 800 x 600 PNG, and Blazor's CPU and GPU canvases. A fixed
1280 x 900 viewport and scale 1 give the canvases 1040 x 836 pixels.
The BrowserWASM app must include the matching artifact's
`SkiaSharp.NativeAssets.WebAssembly` package; the source-built runner cannot
supply the sample's dependencies.

Browser references live under `Expected/Browser/<host-platform>/`; the Web
`SkiaSharp` image reuses `Expected/Host/<host-platform>/web.png`. Review fresh
captures for the remaining stable outputs. The animated GPU canvas retains a
diagnostic screenshot, not an arbitrary-frame golden. Browser filtering does
not replace the unfiltered sample suite; stacked PR branch filters may prevent
producing CI captures until the target branch is eligible.

For restore failures, compare the generated references with the staged package
cohort and inspect the retained binlog. Each consumer already has fresh caches;
clearing shared caches is unnecessary. The existing Integration and MSBuild
test suites remain separate.

Generation regressions can be checked independently with
`pwsh scripts/infra/samples/tests/SampleGeneration.Tests.ps1`; its fixture outputs
are private and the ordinary Samples test suite never invokes Cake.
