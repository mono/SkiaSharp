# Building and Validating Samples

This guide explains how to test SkiaSharp samples using CI-produced NuGet packages.
The `samples` Cake target generates **package-referenced** samples and calls
the shared `RunDotNetTest` helper for `tests/SkiaSharp.Tests.Samples`.
C# tests build every eligible non-Docker solution once per invocation, keeping
the projects' declared target frameworks. YAML runs the suite separately for
each OS/SDK profile: Windows, macOS and Linux with SDK 10 and SDK 11. The net10
runner uses the repository's `global.json`; the sample build SDK can be overridden
without changing that file. Gallery is built, not launched or screenshotted.
The host console and Docker samples also run and save their rendered PNGs.
The runner uses the same artifact-matched SkiaSharp packages to decode those images.

Install the selected SDK from `scripts/azure-templates-variables.yml` and its
required host workloads, alongside the runner's SDK 10. SDK 11's mobile manifests include net10 packs;
WebAssembly also needs `wasm-tools-net10`. The SDK 11 Apple workloads require
Xcode 26.5, so only the Samples macOS job overrides the repository's Xcode pin.
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
| `--sampleTestCategories` | — | Host groups | Comma-separated categories; empty selects all, including local-only groups |

> **Note:** `--previewLabel` and `--buildNumber` only control the package version
> used while sample generation rewrites package references. Acquire packages
> first, then derive both values from the downloaded package filenames.

## Cake Targets

| Target | What it does | Output directory |
|--------|-------------|-----------------|
| `samples-generate` | Copies samples to `output/`, converts ProjectRef → PackageRef | `output/samples/`, `output/samples-preview/` |
| `samples` | Generates samples, then runs the combined sample/package-output test project | `output/logs/testlogs/samples/` |

There are no separate preparation/run targets or MSBuild test lane. The runner
owns package staging, private caches, SDK selection, build diagnostics and cleanup.
It never clears user NuGet caches or prunes unrelated Docker resources.

## Building Samples

The easiest way to build and validate samples is with the **`validate-samples`** Copilot skill.
Ask Copilot to run it — it handles downloading packages, detecting versions, and building automatically.

Example prompts:
- "validate samples"
- "build the samples against the latest CI packages"
- "check if the Blazor sample builds"
- "validate samples from PR 3553"
- "do the samples build after my changes?"

The skill follows the workflow described above: acquire one exact CI package
set, detect its version, then test with `dotnet cake --target=samples`.

See [`.agents/skills/validate-samples/SKILL.md`](../../.agents/skills/validate-samples/SKILL.md)
for the full step-by-step workflow if you need to run it manually.

## How `samples-generate` Works

The `CreateSamplesDirectory()` function in `scripts/infra/samples/samples.cake`:

1. **`<ProjectReference>`** → converted to `<PackageReference>` using the project's `<PackagingGroup>` as the package ID and version from `VERSIONS.txt`
2. **Existing `<PackageReference>`** → version updated from `VERSIONS.txt`
3. For SkiaSharp/HarfBuzzSharp packages, the preview suffix is appended
4. Two output trees: `output/samples/` (stable) and `output/samples-preview/` (preview)

`samples` selects the stable tree only for an exact release identity. Any
non-empty `PREVIEW_NUGET_SUFFIX` selects the preview tree so its references
match the single package family emitted by that build.
All sample solutions use `.slnx`, including the `Windows`, `Mac` and `Linux`
variants. Generation removes binding/source projects outside `samples/`
and keeps the sample projects and their solution configuration.

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

Omit the SDK/workload properties to inherit the full repository `global.json`,
including its roll-forward policy. `ConsumerTargetFramework` defaults to `net10.0`;
it does not retarget samples or the test runner. Other optional properties are
`SampleFilter` and `SampleTestArtifactsDirectory`.
Select a subset with the runner's `--filter-trait` followed by one or more
`Category=PackageOutput`, `Category=PackageMultiTarget`, `Category=SampleBuild`,
`Category=DockerBuild`, `Category=SampleRun`, `Category=RuntimeSmoke` or
`Category=Infrastructure` arguments.
The 44 single-target and four multi-target package-output cases preserve the
build/publish/RID assertions and do not require sample workloads. A net10 consumer
profile covers net9/net10; a net11 profile covers net10/net11, using its selected SDK.
`SampleFilter` selects solutions and Dockerfiles, including the console run,
but does not filter synthetic package-output cases.
An unselected build group is skipped; a filter with no eligible samples fails.

Each invocation has a generated workspace and private restore cache.
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

Docker samples are identified by `Dockerfile`, `linux.Dockerfile` or
`windows.Dockerfile`, not folder names or scripts. They are excluded from the
ordinary build theory. `DockerBuild` builds each image; `SampleRun` executes
the console and Docker samples, building an image if needed. Both categories
can run independently. Tests remove only their own images and containers.

The Docker Web API's `sample.http` supplies plain GET requests separated by `###`.
The first request checks readiness; successful `image/png` responses are decoded
and saved. Tests allocate a localhost port and replace the fixture's
`http://localhost:8080` authority. Headers, variables and script syntax are
deliberately unsupported. A Docker sample without `sample.http` uses the console
convention: it exits successfully and writes an 800x600 `output.png`.
These files are rendering artifacts, not page screenshots or new approved goldens.

The existing Docker SDK/runtime images and project TFMs remain unchanged in
both profiles. Docker unavailability produces explicit skipped results;
failures after a successful Docker probe fail the tests.

## Local Platform Checks

The former Integration project's browser, MAUI and golden-image helpers now
live in this project under `PlatformTests/`, with the existing goldens in `Assets/`.
`RuntimeSmoke` checks native loading and PNG encoding on CI.
`ManualPlatform` retains the generated browser/device/desktop probes for local
use; replacing them with real multi-page sample navigation is a later step.
These tests are skipped on CI before starting Appium or browsers.

With the host's prerequisites installed, opt into a local platform group:

```sh
SKIASHARP_RUN_MANUAL_PLATFORM_TESTS=1 \
dotnet test tests/SkiaSharp.Tests.Samples/SkiaSharp.Tests.Samples.csproj \
  -- --filter-class SkiaSharp.Tests.Samples.PlatformTests.MauiMacCatalystTests
```

Mac Catalyst execution needs Appium's mac2 driver, a logged-in graphical
session and UI Automation/Accessibility permissions. Android/iOS checks still
need their devices or simulators. The default Cake/CI categories do not include
`ManualPlatform`; an unfiltered local invocation skips it unless opted in.

The `sampleProfiles` YAML parameter owns the OS/SDK matrix, including exact SDK
and workload pins, the consumer TFM and optional per-profile `testCategories`.
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
