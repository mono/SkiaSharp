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

Generation uses the base versions in `scripts/VERSIONS.txt` and the explicit
producing build's `previewLabel`/`buildNumber`. Supply the identity matching
the acquired packages; generation does not detect versions from package files.
For example, packages ending in `-pr.5308.26508.28` require:

```sh
dotnet cake --target=samples --previewLabel=pr.5308 --buildNumber=26508.28
```

Do not mix packages from different builds. CI must carry the producing build's
canonical identity into every consuming job, including jobs starting after midnight.

To use an IDE or invoke the suite directly, generate first:

```sh
dotnet cake --target=samples-generate --previewLabel=pr.5308 --buildNumber=26508.28
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
Every host discovers the Docker tests. The macOS CI initialization checks common
Docker installation paths and adds an installed CLI to `PATH` when needed;
tests simply invoke `docker` from `PATH`. A bounded `docker info` probe checks
availability. For now, unavailable Docker reports visible skips
with the probe diagnostic, rather than filtering tests out of the run.
No custom macOS daemon is installed. A reachable daemon must use the expected
container OS, and build/runtime/cleanup failures remain failures.

## Coverage and diagnostics

Build theories cover eligible Basic and Gallery solutions; Gallery, WASM, and
Blazor receive build coverage, not browser/device execution. Separate runtime
tests run host Console/Web and Docker Console/Web, checking exit codes, output,
HTTP responses, and rendered PNGs. Docker retains the samples' original .NET 10
images; its SDK is independent of the host SDK.

Use ordinary IDE filters or select a test directly:

```sh
dotnet test tests/SkiaSharp.Tests.Samples/SkiaSharp.Tests.Samples.csproj \
  -- --filter-method '*BasicSampleTests.WebSampleReturnsImage'
```

`-- --filter-trait Category=Infrastructure` selects helper tests only; it does
not validate actual samples. Missing inputs and empty discovery fail;
unavailable Docker is reported as skipped, never silently removed.

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

For restore failures, compare the generated references with the staged package
cohort and inspect the retained binlog. Each consumer already has fresh caches;
clearing shared caches is unnecessary. The existing Integration and MSBuild
test suites remain separate.

`ArtifactContractTests` validates the actual generated project folders alongside
sample builds: every declared SkiaSharp/HarfBuzzSharp dependency must exist at
its exact version in the acquired cohort, and every Uno project must override
its SDK's SkiaSharp version unconditionally. These tests never invoke Cake.
