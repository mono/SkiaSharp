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

Pass the package suffix to Cake. For example, packages named
`SkiaSharp.4.156.0-pr.5291.26505.69.nupkg` require:

```sh
dotnet cake --target=samples --previewLabel=pr.5291 --buildNumber=26505.69
```

That is an example artifact identity, not a recommended package version.
For exact stable packages, use `--dotNetFinalVersionKind=release` instead.
Do not mix packages from different builds.

To use an IDE or invoke the suite directly, generate first:

```sh
dotnet cake --target=samples-generate --previewLabel=pr.5291 --buildNumber=26505.69
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

A consumer installation can contain multiple SDKs. Set
`SAMPLE_TEST_SDK_VERSION` to select an exact installed consumer SDK without
changing the runner's SDK or TFM (PowerShell on any host):

```powershell
$env:SAMPLE_TEST_SDK_VERSION = '10.0.401'
dotnet test tests/SkiaSharp.Tests.Samples.slnx -- --report-trx
$env:SAMPLE_TEST_SDK_VERSION = '11.0.100-rc.1.26425.128'
dotnet test tests/SkiaSharp.Tests.Samples.slnx -- --report-trx
Remove-Item Env:SAMPLE_TEST_SDK_VERSION
```

The pin disables SDK roll-forward; an unavailable SDK fails. Without this
variable, ordinary host SDK selection applies, including previews. IDEs must
inherit the intended environment too. The suite installs no SDKs/workloads and
does not modify source samples or user caches.

CI runs six parallel jobs: SDK10 and SDK11 on Windows, macOS, and Linux. Both
legs build the same baseline samples with their declared TFMs unchanged. Both
install SDK10 and its workloads/runtime; SDK11 legs additionally install the
pinned preview SDK/workloads. The net10 runner remains independent. Consumer
SDK versions and Console runtime TFMs are recorded in test output.

SDK11 legs also retarget an owned copy of the real Console project to `net11.0`
and repeat its run/PNG checks. The baseline Console still targets `net10.0` and
requires runtime10 even when built by SDK11; the extra case requires runtime11.
No source sample or shared generated input is retargeted. CI supplies matching
`native` and `nuget` artifacts before generation.

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
not validate actual samples. Missing inputs, empty discovery, and unavailable
Docker fail rather than silently removing coverage.

Logs, binlogs, test results, and images remain under
`output/logs/testlogs/samples/basic-<sample>-<guid>/`, outside disposable
workspaces. Tests retain separate stdout/stderr and attach diagnostic files;
CI always publishes the platform's `sample_logs_*` artifact. Commands have
bounded timeouts and cleanup removes only owned processes, workspaces, images,
and containers, never user caches or unrelated Docker resources.

PNG references live in `tests/SkiaSharp.Tests.Samples/Expected/`, qualified by
host/container platform. Comparisons use decoded pixels with zero tolerance.
Missing references retain the actual image and fail; mismatches also retain a
diff. Capture and review references on their actual platform: tests never
generate or accept them automatically.

For restore failures, compare the generated references with the staged package
cohort and inspect the retained binlog. Each consumer already has fresh caches;
clearing shared caches is unnecessary. The existing Integration and MSBuild
test suites remain separate.
