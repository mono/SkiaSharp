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

## Quick Reference

```bash
# Full workflow — clear cache, download, detect version, build all samples
rm -rf externals/package_cache/skiasharp* externals/package_cache/harfbuzzsharp*
dotnet cake --target=docs-download-output
# Detect version from downloaded nupkgs, then:
dotnet cake --target=samples --previewLabel=preview.0 --buildNumber=76
```

## When to Use

- After making changes to samples and wanting to verify they build
- When CI reports sample build failures and you need to reproduce locally
- When validating that a new SkiaSharp release doesn't break samples
- After merging changes that affect sample project files or dependencies

## Step-by-Step Workflow

### Step 1: Clear Cached Packages

Old cached packages cause stale version resolution. Always clear before validating:

```bash
rm -rf externals/package_cache/skiasharp*
rm -rf externals/package_cache/harfbuzzsharp*
```

If you suspect deeper caching issues, also clear the global NuGet cache:

```bash
dotnet nuget locals all --clear
```

### Step 2: Download CI Packages

Download the latest stable and preview NuGet packages from the CI feed:

```bash
dotnet cake --target=docs-download-output
```

This downloads from the **SkiaSharp-CI** Azure DevOps feed:
`https://pkgs.dev.azure.com/xamarin/public/_packaging/SkiaSharp-CI/nuget/v3/index.json`

Two package sets are fetched:
- `_nugets` — stable packages (e.g. `SkiaSharp.3.119.4.nupkg`)
- `_nugetspreview` — preview packages (e.g. `SkiaSharp.3.119.4-preview.0.76.nupkg`)

Output goes to `output/nugets/`.

> **Note:** This target clears `./output/` first.

### Step 3: Detect the Preview Version

The preview label and build number must be passed to the samples target. Run the
detection script to extract them from the downloaded nupkg filenames:

```bash
bash .github/skills/validate-samples/scripts/detect-preview-version.sh
```

The script prints the values to stdout:
```
Found: SkiaSharp.3.119.4-preview.0.76.nupkg
Preview label: preview.0
Build number:  76
Full suffix:   preview.0.76
```

Parse the `Preview label` and `Build number` values from the output and use them
in the next step.

### Step 4: Build Samples

Run the full samples pipeline with the detected version:

```bash
dotnet cake --target=samples --previewLabel=$PREVIEW_LABEL --buildNumber=$BUILD_NUMBER
```

This runs three sub-targets in sequence:
1. **samples-generate** — Copies `samples/` to `output/samples/` and `output/samples-preview/`,
   converting `<ProjectReference>` to `<PackageReference>` using versions from `scripts/VERSIONS.txt`
2. **samples-prepare** — Clears cached SkiaSharp/HarfBuzz packages from `externals/package_cache`,
   copies nupkgs next to Dockerfiles for Docker sample builds
3. **samples-run** — Builds every sample solution in the output directory

#### Building a Single Sample

Use the `--sample` filter to build just one:

```bash
dotnet cake --target=samples --previewLabel=$PREVIEW_LABEL --buildNumber=$BUILD_NUMBER --sample=tvOS
dotnet cake --target=samples --previewLabel=$PREVIEW_LABEL --buildNumber=$BUILD_NUMBER --sample=Blazor
dotnet cake --target=samples --previewLabel=$PREVIEW_LABEL --buildNumber=$BUILD_NUMBER --sample=Android
```

## Version Construction Deep Dive

### Two-step version system

The CI feed and the NuGet packages use **different versioning**:

1. **CI feed wrapper version** (e.g. `0.0.0-branch.main.2580`) — identifies the source build
2. **Real NuGet version** (e.g. `3.119.4-preview.0.76`) — the user-facing package version inside

When downloading, cake uses `--gitBranch`/`--gitSha`/`--previewLabel` to resolve the CI wrapper version.
After extraction, the real nupkg versions are in `output/nugets/`.

### How preview versions are built

The cake build constructs the NuGet preview suffix from two arguments:

```csharp
// build.cake:56-72
var PREVIEW_LABEL = Argument("previewLabel", EnvironmentVariable("PREVIEW_LABEL") ?? "preview");
var FEATURE_NAME = EnvironmentVariable("FEATURE_NAME") ?? "";
var BUILD_NUMBER = Argument("buildNumber", EnvironmentVariable("BUILD_NUMBER") ?? "0");

var PREVIEW_NUGET_SUFFIX = "";
if (!string.IsNullOrEmpty(FEATURE_NAME))
    PREVIEW_NUGET_SUFFIX = $"featurepreview-{FEATURE_NAME}";
else
    PREVIEW_NUGET_SUFFIX = $"{PREVIEW_LABEL}";
if (!string.IsNullOrEmpty(BUILD_NUMBER))
    PREVIEW_NUGET_SUFFIX += $".{BUILD_NUMBER}";
// Result: "preview.0.76" when PREVIEW_LABEL="preview.0" and BUILD_NUMBER="76"
```

The final NuGet version: `{base_version}-{PREVIEW_NUGET_SUFFIX}`
- Base version comes from `scripts/VERSIONS.txt` (e.g. `3.119.4`)
- The `.0` in `preview.0.76` is the **preview number** (first preview = 0, second = 1, etc.)

### How download resolution works

The `DownloadPackageAsync` function resolves the CI artifact version (checked in priority order):

```csharp
// scripts/cake/UtilsManaged.cake:162-173
if (PREVIEW_LABEL.StartsWith("pr."))     → "0.0.0-{PREVIEW_LABEL}.*"   (e.g. "0.0.0-pr.3553.*")
else if (!string.IsNullOrEmpty(GIT_SHA)) → "0.0.0-commit.{sha}.*"
else if (!string.IsNullOrEmpty(GIT_BRANCH_NAME)) → "0.0.0-branch.{name}.*"
else                                     → "0.0.0-branch.main.*"
```

The `CreateSamplesDirectory()` function in `scripts/cake/samples.cake`:

1. **`<ProjectReference>`** → converted to `<PackageReference>` using the project's
   `<PackagingGroup>` as the package ID and version from `VERSIONS.txt`
2. **Existing `<PackageReference>`** → version updated from `VERSIONS.txt`
3. For SkiaSharp/HarfBuzzSharp packages, the preview suffix is appended

Two output trees are created:
- `output/samples/` — stable versions
- `output/samples-preview/` — preview versions (with suffix)

## Cake Arguments Reference

| Argument | Env var | Default | Purpose |
|----------|---------|---------|---------|
| `--previewLabel` | `PREVIEW_LABEL` | `preview` | Preview suffix label |
| `--buildNumber` | `BUILD_NUMBER` | `0` | Build number for suffix |
| `--buildCounter` | `BUILD_COUNTER` | Same as buildNumber | Build counter for CI version |
| `--previewFeed` | — | SkiaSharp-CI URL | Override the NuGet feed |
| `--gitBranch` | `GIT_BRANCH_NAME` | `""` | Branch name for CI version |
| `--sample` | — | `""` | Filter to specific sample |

## Troubleshooting

### "The local source 'packages' doesn't exist" (Docker samples)
Docker samples (`Docker/Console`, `Docker/WebApi`) have their own `nuget.config` with a
local `packages` folder. This is by design — they're built via `run.ps1` inside Docker,
not `dotnet build`. The `samples-prepare` target copies nupkgs there automatically.

### Platform-specific samples not building
Some platforms are disabled by default for local builds:
```bash
# These MSBuild properties enable optional platforms
-p:IsNetTVOSSupported=true
-p:IsNetTizenSupported=true
-p:IsNetMacOSSupported=true
```

### WinUI XAML compiler crash on .NET 10
The WinUI sample may need a newer `Microsoft.WindowsAppSDK` version. This is a known
compatibility issue between older WinUI SDK versions and .NET 10.

### Stale packages after repeated runs
If versions don't update between runs:
```bash
rm -rf externals/package_cache/skiasharp* externals/package_cache/harfbuzzsharp*
dotnet nuget locals all --clear
```

## Further Reading

- [Building Samples](../../documentation/dev/building-samples.md) — Full developer documentation
- `build.cake` lines 56-83 — Version construction
- `scripts/cake/samples.cake` — Sample targets and ProjectRef→PackageRef conversion logic
- `scripts/VERSIONS.txt` — Package version source of truth
