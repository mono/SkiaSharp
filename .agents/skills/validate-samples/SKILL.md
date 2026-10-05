---
name: validate-samples
description: >-
  Build and validate SkiaSharp sample projects using CI-produced NuGet packages.
  Downloads the latest CI artifacts, detects the preview version, and runs the
  samples cake target to verify all samples compile correctly.
  Triggers: "validate samples", "build samples", "test samples", "check samples build",
  "run samples", "do the samples build", "samples CI", "verify sample builds".
  Also use when asked to check if samples work after a code change, or when
  investigating sample build failures. Use this skill proactively whenever the
  user mentions building, testing, or validating any SkiaSharp sample project.
---

# Validate Samples

Automates the workflow for building SkiaSharp samples against CI-produced NuGet packages.
The samples use package references (not project references) when built through the cake
target, so they need downloadable NuGet packages.

## When to Use

- After making changes to samples and wanting to verify they build
- When CI reports sample build failures and you need to reproduce locally
- When validating that a new SkiaSharp release doesn't break samples
- After merging changes that affect sample project files or dependencies

## Workflow

### Step 1: Verify prerequisites

Install the stable and preview SDKs pinned in `scripts/azure-templates-variables.yml`.
Keep root `global.json` unchanged. The `SkiaSharp.Tests.Samples` runner targets
net10 and uses the repository SDK. Each invocation tests one selected consumer SDK;
YAML owns the six OS/SDK runs. Samples keep their declared TFMs. An invocation
without an SDK override inherits the full repository `global.json`.
Never clear user/global NuGet caches.

Full sample coverage requires the host workloads for both SDK feature bands,
including SDK 11's backward-targeting packs and `wasm-tools-net10`.
Gallery is build-only. Docker is classified by Dockerfiles and has separate
`DockerBuild` and `SampleRun` theories, included in both profiles. The host console
also runs and saves its PNG. Existing Docker images are independent of the host SDK.
The former Integration platform probes/goldens are in this project; `ManualPlatform`
is local opt-in only and skips on CI.

### Step 2: Download CI packages

Choose the source that actually owns the packages.

PR builds are pipeline artifacts and are not published to the transport feed.
Download them with the repository helper, then copy the packages into the
sample workflow's expected directory:

```powershell
pwsh scripts/get-skiasharp-pr.ps1 3553 -Force
New-Item output/nugets -ItemType Directory -Force | Out-Null
Copy-Item ~/.skiasharp/hives/pr-3553/packages/*.nupkg output/nugets/
```

For an exact non-PR commit, resolve its public definition-345 Build ID, download
that run's canonical `nuget` artifact, and extract non-symbol `.nupkg` files into
`output/nugets/`. For a promoted branch build, retrieve and extract its
branch-versioned `_NuGets` transport package from the public
`dotnet-libraries-transport` feed. Do not query that feed by SHA or use a
retired parent documentation-download Cake target.

### Step 3: Detect the preview version

Run the detection script — it prints the preview label and build number
extracted from the downloaded nupkg filenames:

```powershell
pwsh .agents/skills/validate-samples/scripts/detect-preview-version.ps1
```

Output:
```
Found: SkiaSharp.3.119.4-preview.0.76.nupkg
Preview label: preview.0
Build number:  76
Full suffix:   preview.0.76
```

Parse `Preview label` and `Build number` from the output for the next step.

### Step 4: Test samples

```powershell
dotnet cake --target=samples --previewLabel=<PREVIEW_LABEL> --buildNumber=<BUILD_NUMBER>
```

That uses repository SDK defaults and a net10 synthetic consumer profile
(net9/net10 coverage). To test another explicit SDK/profile:

```powershell
dotnet cake --target=samples --previewLabel=<PREVIEW_LABEL> --buildNumber=<BUILD_NUMBER> `
  --sampleSdkVersion=<DOTNET_VERSION_PREVIEW> `
  --sampleWorkloadVersion=<DOTNET_WORKLOAD_VERSION_PREVIEW> `
  --consumerTargetFramework=net11.0
```

To build a single sample, add `--sample=<name>`:

```powershell
dotnet cake --target=samples --previewLabel=<PREVIEW_LABEL> --buildNumber=<BUILD_NUMBER> --sample=Blazor
```

C# theories enumerate eligible sample solutions and own builds and Docker staging.
One invocation uses one SDK; YAML owns SDK iteration.
Selection is the theory data, not a separate JSON plan.
Cake keeps package-reference generation/ZIPs and uses shared `RunDotNetTest`.
TRX and binlogs appear under `output/logs/testlogs/samples/`; commands and build
output appear in normal test output.
The same project includes 44 single-target `PackageOutput` cases and four
`PackageMultiTarget` cases using the current/previous consumer TFMs; no separate
MSBuild lane or preparation/run target exists. Select groups with
`--sampleTestCategories=SampleBuild,PackageOutput,PackageMultiTarget,DockerBuild,SampleRun,RuntimeSmoke,Infrastructure`.
These are the default host categories. All sample solutions are `.slnx`; no
legacy solution/filter parsers or Docker PowerShell runners are used.

For package-output-only diagnostics without workloads or sample generation:

```powershell
dotnet test tests/SkiaSharp.Tests.Samples/SkiaSharp.Tests.Samples.csproj `
  -p:PackageDirectory=/absolute/path/to/nugets `
  -p:SampleSdkVersion=<DOTNET_VERSION_PREVIEW> `
  -p:ConsumerTargetFramework=net11.0 `
  -- --filter-trait "Category=PackageOutput" "Category=PackageMultiTarget" --report-trx
```

## Troubleshooting

### Incorrect package versions
Check the exact artifact identity and generated PackageReference versions.
Each run has fresh private caches and artifact-only SkiaSharp/HarfBuzzSharp
source mapping; do not change global caches or permit public fallback.

### Platform-specific samples not building
Some platforms are disabled by default:
```powershell
# Pass these MSBuild properties to enable optional platforms
-p:IsNetTVOSSupported=true
-p:IsNetTizenSupported=true
-p:IsNetMacOSSupported=true
```

### WinUI XAML compiler crash on .NET 10
May need a newer `Microsoft.WindowsAppSDK` version.

### "The local source 'packages' doesn't exist" (Docker samples)
The Samples test runner stages input nupkgs per Docker case and invokes Docker
directly from C#. `sample.http` contains the Web API's plain GET requests;
the first checks readiness and image responses are decoded/saved.
Samples without that file use the console `output.png` convention.

## Further Reading

See [Building Samples](../../../documentation/dev/building-samples.md) for version construction
details, download resolution, cake arguments reference, and how `samples-generate` works.
