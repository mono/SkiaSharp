# Building and Validating Samples

This guide explains how to build SkiaSharp samples using CI-produced NuGet packages. The samples use **package references** (not project references) when built through the `samples` cake target, so they need downloadable NuGet packages to compile.

## Transport Feed

Official builds register wrapper packages as non-shipping assets in the same BAR
as the product packages. The Maestro `SkiaSharp` channel routes them to the
shared **dotnet-libraries-transport** Azure DevOps feed:

```
https://pkgs.dev.azure.com/dnceng/public/_packaging/dotnet-libraries-transport/nuget/v3/index.json
```

These wrapper packages bundle the real NuGet packages inside their `tools/` directory:

| Wrapper package | Contains |
|-----------------|----------|
| `_nativeassets` | Native binaries (per-platform frameworks/dylibs) |
| `_nugets` | The build's single NuGet package family: exact stable or prerelease |

The wrapper packages use `0.0.0-{source}.{build}` versioning to identify their CI source. The actual NuGet packages inside have their real, user-facing version numbers.

## Two-Step Process

Building samples requires two separate sets of arguments because the CI feed version and the NuGet package version are different things:

### Step 1: Acquire packages

For a pull request build, use the supported repository helper and copy its
packages into the sample workflow directory:

```powershell
pwsh scripts/get-skiasharp-pr.ps1 3553 -SuccessfulOnly -Force
New-Item output/nugets -ItemType Directory -Force | Out-Null
Copy-Item ~/.skiasharp/hives/pr-3553/packages/*.nupkg output/nugets/
```

For an exact public build, download its canonical `nuget` pipeline artifact
and extract non-symbol packages to `output/nugets/`. For a promoted branch
build, retrieve and extract the matching branch-versioned `_NuGets` transport
package from the public `dotnet-libraries-transport` feed. Do not use a
retired parent documentation-download Cake target.

### Step 2: Build samples — use the real NuGet version

After downloading, the extracted nupkgs in `output/nugets/` have real version numbers. The `samples` target needs `--previewLabel` and `--buildNumber` matching these real versions:

```powershell
# Detect from downloaded packages
ls output/nugets/SkiaSharp.[0-9]*-*.nupkg
# → SkiaSharp.4.152.0-preview.0.26418.3.nupkg
# So: --previewLabel=preview.0 --buildNumber=26418.3
```

## NuGet Package Version Construction

The Cake build constructs the NuGet suffix in `scripts/infra/shared/shared.cake`:

```csharp
var PREVIEW_LABEL = Argument ("previewLabel", EnvironmentVariable ("PREVIEW_LABEL") ?? "preview").ToLowerInvariant ();
var BUILD_NUMBER = Argument ("buildNumber", EnvironmentVariable ("BUILD_NUMBER") ?? "0");
var DOTNET_FINAL_VERSION_KIND = Argument (
    "dotNetFinalVersionKind",
    EnvironmentVariable ("DOTNET_FINAL_VERSION_KIND") ?? "").ToLowerInvariant ();

var PREVIEW_NUGET_SUFFIX = DOTNET_FINAL_VERSION_KIND == "release" ? "" : PREVIEW_LABEL;
if (DOTNET_FINAL_VERSION_KIND != "release" && !string.IsNullOrEmpty (BUILD_NUMBER))
    PREVIEW_NUGET_SUFFIX += $".{BUILD_NUMBER}";
```

The normal NuGet version is `{base_version}-{PREVIEW_NUGET_SUFFIX}`. In CI,
source-controlled `PREVIEW_LABEL=stable` derives
`DOTNET_FINAL_VERSION_KIND=release`; direct Cake invocations select the same
exact `{base_version}` with `--dotNetFinalVersionKind=release`.

- **base_version**: From `scripts/VERSIONS.txt` (e.g. `3.119.4`)
- **PREVIEW_LABEL**: The preview label (e.g. `preview.0` — first preview, `preview.1` — second, etc.)
- **BUILD_NUMBER**: Arcade's package build identity (`short-date.revision`)

**Example:** `4.152.0-preview.0.26418.3` → `previewLabel=preview.0`, `buildNumber=26418.3`

## Cake Arguments

### For building samples (`samples`)

These arguments control the **NuGet version suffix** used when rewriting package references:

| Argument | Environment variable | Default | Purpose |
|----------|---------------------|---------|---------|
| `--previewLabel` | `PREVIEW_LABEL` | `preview` | Preview suffix label |
| `--buildNumber` | `BUILD_NUMBER` | `0` | Build number for suffix |
| `--dotNetFinalVersionKind` | `DOTNET_FINAL_VERSION_KIND` | `""` | Set to `release` for an exact stable version |

> **Note:** `--previewLabel` and `--buildNumber` only control the package version
> used while sample generation rewrites package references. Acquire packages
> first, then derive both values from the downloaded package filenames.

## Cake Targets

| Target | What it does | Output directory |
|--------|-------------|-----------------|
| `samples-generate` | Copies samples to `output/`, converts ProjectRef → PackageRef | `output/samples/`, `output/samples-preview/` |
| `samples` | Generates samples, then invokes the ordinary sample test project | `output/logs/testlogs/samples/` |

## Building Samples

After acquiring packages as described above, run `dotnet cake --target=samples`
to generate and test the sample projects. The separate `samples-prepare` and
`samples-run` stages are no longer needed; each test owns its preparation and cleanup.

After package acquisition and generation, the same suite runs directly from
`dotnet test` or an IDE without recursively invoking Cake:

```sh
dotnet cake --target=samples-generate --previewLabel=pr.5291 --buildNumber=26505.69
dotnet test tests/SkiaSharp.Tests.Samples.slnx -- --report-trx
```

The suffix in this example is an artifact identity, not a recommended package
version. Use the exact producing build's packages and suffix. `output/nugets`
must contain exactly one non-symbol core package for each of SkiaSharp and
HarfBuzzSharp, plus the native/view packages needed by the selected samples.
The acquired core package identity selects the generated stable or preview
tree automatically. For stable packages use `--dotNetFinalVersionKind=release`.
Generation writes exact package references; the tests never rewrite them.

The runner uses the normal SkiaSharp project reference and native asset import;
it has no SkiaSharp/HarfBuzzSharp package version overrides. CI declares both
`native` and `nuget` prerequisites and uses the existing required-artifact
download step to populate `output/`, exactly like the other repository tests.
Local runs need the normal repository native bootstrap before package acquisition
and sample generation. For C#-only work, `dotnet cake --target=externals-download`
provides it; run this before staging packages because that target resets `output/`.
For native changes, build the natives from source instead. Generation only
generates samples; it neither acquires nor extracts natives.

Use ordinary IDE or `dotnet test` filters to select discoverable theory rows;
there is no separate Cake/MSBuild sample filter. Missing generated inputs or
empty eligible sample discovery fail. The runner remains `net10.0`; it does
not retarget sample projects. No SDK, workload, executable, or path properties
are required: IDE test runs use the same generated inputs and conventional
repository paths as CLI and CI runs.

The test helpers keep these responsibilities separate: `Repo` exposes named
repository paths and privately reads the core package identity to select the
generated sample tree; `ProcessRunner` runs both dotnet and Docker.
`SampleWorkspace` owns the sample copy, private caches, import/SDK fences, and
cleanup. `SampleTestBase.BuildSample` orchestrates preparation, SDK selection,
and the build. `DotNet` supplies reusable `Build` and `GetVersion` commands,
process environment configuration, and NuGet restore configuration. `Build`
always requests a binlog in its supplied diagnostics directory and retains
the text log and available binlog on failure.
Infrastructure tests pass temporary paths directly rather than overriding
process-wide settings.
Helpers use xUnit v3's current test context for output and attachments, without
passing output helpers through tests or fixtures. File attachments use the
`TestContext.Current.AddFileAttachment` extension. Process output outside an
individual test, such as shared-fixture cleanup, uses diagnostic messages.

Sample builds invoke `dotnet` from the test process's inherited `PATH`.
A consumer-local `global.json` searches only that host installation without
copying the repository SDK/workload pins or Arcade mappings. The host controls
the installed SDKs and workloads; no installation or workload selection occurs
inside the suite. An installation containing multiple SDKs uses normal .NET SDK
selection, including prereleases. CI can select an isolated installation on
`PATH` for a specific SDK. IDEs must be launched with the intended environment.
The runner itself remains a normal repository `net10.0` project, built using
the repository `global.json`, and needs a compatible runtime to execute.
Consumer builds default to Release in the test data, independently of the
runner's Debug/Release configuration. Configuration is a theory argument:
additional Debug cases can be added and selected without new MSBuild settings.

### Coverage and isolation

Theory rows build actual generated `.slnx` solutions. Host `.Windows`, `.Mac`,
and `.Linux` variants replace the common solution; ordinary dotted solution
names remain eligible. Gallery is build-only, including its shared sibling
projects. WASM and Blazor samples receive eligible build coverage, not browser
navigation or reference-scene comparison.

`BasicSampleTests` covers ordinary samples; `GallerySampleTests` separately
covers Gallery using the same workspace/build pipeline. Both belong to the
`SampleBuild` category, with Gallery also tagged `GalleryBuild`.
Separate infrastructure tests validate the actual Docker, ordinary-sample,
and host Gallery case sets without requiring Docker or building the samples.

Each build copies only its sample directory and required ancestor build/NuGet
configuration into a unique owned workspace under `output/samples-test-workspaces/`.
The Gallery workspace includes its sibling projects. Consumer import fences,
an unpinned host-only `global.json`, and private NuGet/CLI caches prevent repository build
targets, Arcade SDK selection, and user caches from leaking into sample builds.
Sample-local SDK pins are rejected. Source projects and installed workloads
are not modified.

The two Docker samples retain their original .NET 10 Dockerfiles. Tests build
private image tags, run the console and check its exit code and decoded
800 x 600 PNG, and run the web API using GET requests from `sample.http`,
checking HTTP success and the decoded PNG. Docker must be installed, responsive,
and in the host's expected container mode (Windows on Windows, Linux otherwise).
Unavailable Docker fails rather than silently omitting coverage.

Consumer commands have bounded timeouts. Workspaces, staged Docker contexts,
and only owned containers/images are removed; no user-cache clearing or global
Docker prune is performed. Binlogs, build logs, test results, and PNGs remain
outside disposable workspaces in `output/logs/testlogs/samples/`.
PNG, log and binlog files up to 8 MiB are attached to their individual test
results. Larger files remain in the published pipeline diagnostics artifact
with a path in the test output; they are not copied into memory as attachments.
Attachment presentation in IDEs depends on their MTP/VSTest integration.
Infrastructure tests can be run separately with `-- --filter-trait Category=Infrastructure`;
this does not substitute for actual sample and Docker coverage.

The existing `SkiaSharp.Tests.Integration` and `SkiaSharp.Tests.MSBuild` projects
and their entry points remain separate and unchanged. This suite does not
migrate package-output matrices, generated view/device probes, or golden tests.
Related generated dependency prerequisites are tracked in #5297. This suite
includes only the declaration fixes required for its actual generated sample
builds; missing packages cannot be hidden by feeds, TFM overrides, or omitted rows.

## How `samples-generate` Works

The `CreateSamplesDirectory()` function in `scripts/infra/samples/samples.cake`:

Sample solutions are `.slnx` files. Generation keeps the sample projects in each
solution and removes references to projects outside `samples/`; host-specific
variants are selected by their `.Mac`, `.Windows`, or `.Linux` suffix.

1. **`<ProjectReference>`** → converted to `<PackageReference>` using the project's `<PackagingGroup>` as the package ID and version from `VERSIONS.txt`
2. **Existing `<PackageReference>`** → version updated from `VERSIONS.txt`
3. For SkiaSharp/HarfBuzzSharp packages, the preview suffix is appended
4. Two output trees: `output/samples/` (stable) and `output/samples-preview/` (preview)

`samples` selects the stable tree only for an exact release identity. Any
non-empty `PREVIEW_NUGET_SUFFIX` selects the preview tree so its references
match the single package family emitted by that build.

## Troubleshooting

### Package restore failures

Check the retained binlog and compare generated references with the exact
package cohort. SkiaSharp/HarfBuzzSharp consumer references are mapped only
to the artifact directory; they cannot fall back to public feeds. Each test
already has fresh private caches, so clearing shared or user caches is unnecessary.

### tvOS/macOS/Tizen not building
Some platforms are disabled by default:
```powershell
# Pass these MSBuild properties to enable optional platforms
-p:IsNetTVOSSupported=true
-p:IsNetTizenSupported=true
-p:IsNetMacOSSupported=true
```

### WinUI XAML compiler failures on .NET 10
May need a newer `Microsoft.WindowsAppSDK` version.

### NuGet feed authentication
The dotnet-libraries-transport feed is public — no authentication required.
