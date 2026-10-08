# Building and Validating Samples

This guide explains how to test SkiaSharp samples using CI-produced NuGet packages.
The `samples` Cake target generates **package-referenced** samples and calls
the shared `RunDotNetTest` helper for `tests/SkiaSharp.Tests.Samples`.
C# tests build every eligible non-Docker solution, keeping
the projects' declared target frameworks. YAML runs the suite separately for
each OS/SDK profile: Windows, macOS and Linux with SDK 10 and SDK 11. The net10
runner uses the repository's `global.json`; the sample build SDK can be overridden
without changing that file. The runner SDK, consumer SDK and project TFMs are
independent: SDK 11 builds the unchanged net10 projects, and later SDK profiles
can build whichever TFMs the samples declare. Gallery is built, not launched or screenshotted.
The host console and Docker samples also run in their own processes and save
their rendered PNGs.
The runner uses the same artifact-matched SkiaSharp packages to decode those images.

Install the selected SDK from `scripts/azure-templates-variables.yml` and its
required host workloads, alongside the runner's SDK 10. The SDK 11 profile pins
RC1 and its workload set; it uses the existing approved feeds, not a preview-feed
fallback. SDK 11's mobile manifests include net10 packs;
WebAssembly also needs `wasm-tools-net10`. The SDK 11 profile still builds net10
Apple samples using its backward-targeting packs. The Samples macOS jobs use
the hosted `macos-26` image; SDK 10 retains the repository Xcode pin and SDK 11
selects Xcode 26.6, required by its newer net10 Apple packs. Native builds and
source/unit-test agents are unchanged.
Workload installation uses an exact SDK context and the repository's approved
NuGet configuration. Missing mirrored packages fail explicitly; provisioning
does not add NuGet.org or select a different SDK/workload set.
Samsung manifests are registered beside the resolved, PATH-selected `dotnet`
executable, not an inherited `DOTNET_ROOT` that may refer to another installation.
Missing required workloads are failures, not reasons to retarget or omit samples.

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

For routine local validation against the latest promoted main build:

```powershell
dotnet cake --target=nuget-download --gitBranch=main
```

The target uses the transport-feed downloader and replaces only `output/nugets/`.
It does not remove native binaries or previous test evidence. Detect the real
package version after acquisition; a currently running PR build is not required.

For a pull request build, use the supported repository helper and copy its
packages into the sample workflow directory:

```powershell
pwsh scripts/get-skiasharp-pr.ps1 3553 -SuccessfulOnly -Force
New-Item output/nugets -ItemType Directory -Force | Out-Null
Copy-Item ~/.skiasharp/hives/pr-3553/packages/*.nupkg output/nugets/
```

For an exact public build, download its canonical `nuget` pipeline artifact
and extract non-symbol packages to `output/nugets/`. For a promoted branch
build, use `nuget-download --gitBranch=<branch>` to extract the matching
branch-versioned `_NuGets` transport package. Do not use a
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

These arguments control package-reference generation and the single test invocation:

| Argument | Environment variable | Default | Purpose |
|----------|---------------------|---------|---------|
| `--previewLabel` | `PREVIEW_LABEL` | `preview` | Preview suffix label |
| `--buildNumber` | `BUILD_NUMBER` | `0` | Build number for suffix |
| `--dotNetFinalVersionKind` | `DOTNET_FINAL_VERSION_KIND` | `""` | Set to `release` for an exact stable version |
| `--sample` | — | `""` | Substring filter for generated solutions and Dockerfiles |
| `--sampleSdkVersion` | — | `""` | Exact consumer SDK override; empty inherits repository `global.json` |
| `--sampleWorkloadVersion` | — | `""` | Optional `sdk.workloadVersion` pin with an explicit SDK override |
| `--consumerTargetFramework` | — | `net10.0` | Current synthetic-consumer TFM; tests also cover its previous major |
| `--sampleTestExcludeCategories` | — | `""` | Comma-separated categories to exclude; empty runs every test, including new/uncategorized and local platform checks |

> **Note:** `--previewLabel` and `--buildNumber` only control the package version
> used while sample generation rewrites package references. Acquire packages
> first, then derive both values from the downloaded package filenames.

## Cake Targets

| Target | What it does | Output directory |
|--------|-------------|-----------------|
| `samples-generate` | Copies samples to `output/`, converts ProjectRef → PackageRef | `output/samples/`, `output/samples-preview/` |
| `samples` | Generates samples, then runs the combined sample/package-output test project | `output/logs/testlogs/samples/` |
| `nuget-download` | Downloads the latest promoted package family for `--gitBranch` | `output/nugets/` |

There are no separate preparation/run targets or MSBuild test lane. The runner
owns package staging, private caches, SDK selection, build diagnostics and cleanup.
It never clears user NuGet caches or prunes unrelated Docker resources.

## Building Samples

The Samples CI stage builds and validates the generated package consumers.
For local diagnostics, acquire one exact package set and derive its version
as described above, then run `dotnet cake --target=samples`. The direct
`dotnet test` entry point is documented below; no separate skill or release
validation wrapper is required.

## How `samples-generate` Works

The `CreateSamplesDirectory()` function in `scripts/infra/samples/samples.cake`:

Sample solutions are `.slnx` files. Generation keeps the sample projects in each
solution and removes references to projects outside `samples/`; host-specific
variants are selected by their `.Mac`, `.Windows`, or `.Linux` suffix.

1. **`<ProjectReference>`** → converted to `<PackageReference>` using the project's `<PackagingGroup>` and version from `VERSIONS.txt`; native-asset projects use their own package ID while sharing the family version
2. **Existing `<PackageReference>`** → version updated from `VERSIONS.txt`
3. For SkiaSharp/HarfBuzzSharp packages, the preview suffix is appended
4. Two output trees: `output/samples/` (stable) and `output/samples-preview/` (preview)

`samples` selects the stable tree only for an exact release identity. Any
non-empty `PREVIEW_NUGET_SUFFIX` selects the preview tree so its references
match the single package family emitted by that build.
All sample solutions use `.slnx`, including the `Windows`, `Mac` and `Linux`
variants. Generation removes binding/source projects outside `samples/`
and keeps the sample projects and their solution configuration.
All three Uno consumers (Basic UnoPlatform, Gallery Uno and SkiaFiddle) declare
their runtime dependencies after the source-only `_UnoPlatformSamples.targets`
import. Their out-of-tree project references become
artifact-versioned package references during generation; their `SkiaSharpVersion`
properties pin Uno.Sdk's implicit core reference to the same artifact family.

## Direct Test Invocation

Generate first with `dotnet cake --target=samples-generate` and the matching
package suffix. Then run the test project without recursively invoking Cake:

```sh
dotnet test tests/SkiaSharp.Tests.Samples/SkiaSharp.Tests.Samples.csproj \
  -p:PackageDirectory=/absolute/path/to/nugets \
  -p:SamplesDirectory=/absolute/path/to/generated/samples-preview \
  -p:SampleSdkVersion=<exact-sdk-version> \
  -p:SampleWorkloadVersion=<workload-set-version> \
  -p:ConsumerTargetFramework=net11.0 \
  -- --report-trx --results-directory /absolute/path/to/test-results
```

Omit the SDK/workload properties to copy the repository's SDK selection,
including its roll-forward policy, into the isolated workspace.
Repository tool and MSBuild SDK configuration are not copied.
`ConsumerTargetFramework` defaults to `net10.0`;
it selects the synthetic package-output matrix and does not retarget samples,
the test runner or the preserved net10 Integration probe apps. Other optional properties are
`SampleFilter` and `SampleTestArtifactsDirectory`.
Select a subset with the runner's `--filter-trait` followed by one or more
`Category=PackageOutput`, `Category=PackageMultiTarget`, `Category=SampleBuild`,
`Category=DockerBuild`, `Category=SampleRun`, `Category=RuntimeSmoke` or
`Category=Infrastructure` arguments. An unfiltered run includes new tests without
any category. Use `--filter-not-trait Category=Docker` to exclude both Docker
build and run cases without excluding the host console run.
The 44 single-target and four multi-target package-output cases preserve the
build/publish/RID assertions and do not require sample workloads. A net10 consumer
profile covers net9/net10; a net11 profile covers net10/net11, using its selected SDK.
`SampleFilter` selects solutions and Dockerfiles, including the console run,
but does not filter synthetic package-output cases.
An unselected build group is skipped; a filter with no eligible samples fails.

Each test case copies only its selected sample folder and any generated ancestor
build/NuGet configuration into an owned workspace under
`output/samples-test-workspaces/`, with its own `global.json` and private restore
cache. Selecting `Gallery` copies its sibling projects together; a Console case
does not copy unrelated samples. `SampleTestBase` owns preparation and disposal.
Empty `Directory.Build.props` and `Directory.Build.targets` at the
workspace root stop repository build imports from reaching the consumers.
Build/publish commands run in that copied tree; sample TFMs are not rewritten.
SkiaSharp/HarfBuzzSharp source mapping permits only the supplied artifacts.
One shared lookup classifies generated samples as `Sample`, `Gallery` or `Docker`
and filters platform variants for the current host. The build theory selects
Sample/Gallery entries; the Docker theory selects Docker entries.
Discovery is bounded to two directory levels: `Gallery/` and `Basic/<sample>/`,
without walking nested projects, assets or build outputs.
Test results identify each solution; the CI run title identifies its SDK profile.
Build output appears in the test output and MSBuild binlogs are written under
`output/logs/testlogs/samples`. No separate sample plan is generated.
Missing generated inputs or a filter with no eligible matches fail explicitly.
Consumer CLI commands have explicit timeouts and terminate their owned process
trees on timeout. Sample runs do not enable MTP's HangDump activity monitor:
version 1.9.1 falsely timed out a progressing macOS consumer run and then crashed
while enumerating dump files after all tests passed. Ordinary core test runners
retain their existing HangDump settings; CI also retains its sample-job time limit.

Docker samples are identified by `Dockerfile`, `linux.Dockerfile` or
`windows.Dockerfile`, not folder names or scripts. They are excluded from the
ordinary build theory. `DockerBuild` builds each image; `SampleRun` executes
the console and Docker samples, building an image if needed. Both categories
can run independently, while the Docker class's separate `Category=Docker`
marker covers both methods. CI runs both Docker methods in every SDK profile;
the class-level marker remains available for an explicit opt-out.
Tests remove only their own images and containers.

The Docker Web API's `sample.http` supplies plain GET requests separated by `###`.
The first request checks readiness; successful `image/png` responses are decoded
and saved. Tests allocate a localhost port and replace the fixture's
`http://localhost:8080` authority. Headers, variables and script syntax are
deliberately unsupported. A Docker sample without `sample.http` uses the console
convention: it exits successfully and writes an 800x600 `output.png`.
These files are rendering artifacts, not page screenshots or new approved goldens.

The host and Docker console tests require successful execution and validate
the rendered 800x600 PNG.
The Docker SDK/runtime images and project TFMs remain as declared in both
profiles; the sample-test runner remains on the repository SDK/net10 while
consumer SDK selection is independent. Docker unavailability produces explicit
skipped results;
failures after a successful Docker probe fail the tests.

## IDE and Runtime Checks

The runner is an ordinary repository test project and imports the normal test
build defaults. After acquiring packages into `output/nugets/` and generating
matching sample inputs, open `tests/SkiaSharp.Tests.Samples.slnx` in Visual Studio
or run it with `dotnet test`. No device-selection MSBuild properties are needed.
The runner uses the repository SDK; only its child workspaces are isolated.
Test classes live at the project root and shared execution helpers in `Utils/`.
The existing reference images remain in `Assets/`.
`RuntimeSmoke` checks native loading and PNG encoding on CI.
`Browser` starts the actual Basic Web, BrowserWebAssembly and BlazorWebAssembly
samples and drives them with headless Playwright Chromium on CI and locally.
The Web sample's Razor page and PNG endpoints are checked; BrowserWebAssembly
executes its .NET WASM renderer; BlazorWebAssembly navigates CPU and GPU pages.
The preserved generated Blazor probes additionally exercise canvas and GL views.
Browser runtime tests validate rendered dimensions and scene pixels and save
screenshots. Separate `Golden` theory cases compare the existing browser reference;
there is no runtime CI-environment branch selecting which assertions to run.
CI explicitly installs matching Chromium
and its system dependencies. Gallery and SkiaFiddle are not navigated by these
tests; Gallery is built through its host solution, while SkiaFiddle has no solution
entry and is only generated and checked for package-reference integrity.
`Device` and `Desktop` retain generated Appium view probes for local use;
replacing them with real multi-page sample navigation is a later step.
`Golden` renders `TestImage` on the host and in the preserved standalone
Linux container consumer, comparing both against the unchanged base reference
image using the shared 95% screenshot comparator. The container probe reuses
the owned Docker lifecycle and artifact staging; it does not change or combine
the two actual Docker samples. CI explicitly
excludes `Device`, `Desktop` and `Golden` through the test runner's category filter.
There is no additional runtime policy or manual opt-in. Browser runtime cases
remain included.

With the host's prerequisites installed, select a local platform group:

```sh
dotnet test tests/SkiaSharp.Tests.Samples/SkiaSharp.Tests.Samples.csproj \
  -- --filter-class SkiaSharp.Tests.Samples.MauiMacCatalystTests
```

Mac Catalyst execution needs Appium's mac2 driver, a logged-in graphical
session and UI Automation/Accessibility permissions. Android/iOS cases declare
their exact prerequisites in theory data: `Pixel_API_36` at `emulator-5554`,
Android 16/API 36, and an available `iPhone 16 Pro` simulator on iOS 26.2.
Appium uses port 4723. Missing or ambiguous exact prerequisites fail with installation
guidance; tests never choose a newer runtime, another emulator or another booted
simulator. Install/create the declared prerequisites and rerun the same IDE test.
There is no local opt-in:
an unfiltered local run attempts supported probes and the host golden.
For a local run without GUI prerequisites, pass
`--sampleTestExcludeCategories=Device,Desktop,Golden` to Cake.

The `sampleProfiles` YAML parameter owns the OS/SDK matrix. Each profile's `sdk`
object declares `sdkVersion`, `workloadSetVersion` and optional Tizen manifest
pins; the job translates that object into `dotnetSdks`. A preview sample profile
also declares its host workload lists, including `wasm-tools-net10`, rather than
the installer guessing backward-targeting requirements from the SDK major.
The consumer TFM and per-profile `testExcludeCategories` remain separate.
Omitting `xcodeVersion` inherits the global pin; net11 explicitly selects 26.6.
Adding another profile requires no C# SDK-matrix logic.

## Troubleshooting

### Missing or incorrect packages
Each run uses fresh private caches. Check the input artifact build identity and
the generated PackageReference versions; do not clear global caches or allow
missing SkiaSharp/HarfBuzzSharp packages to restore from public feeds.

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
