# Building and Validating Samples

This guide explains how to build SkiaSharp samples using CI-produced NuGet packages. The samples use **package references** (not project references) when built through the `samples` cake target, so they need downloadable NuGet packages to compile.

## CI Artifacts Feed

All CI builds publish packages to the **SkiaSharp-CI** Azure DevOps feed:

```
https://pkgs.dev.azure.com/xamarin/public/_packaging/SkiaSharp-CI/nuget/v3/index.json
```

Three package families are published per build:

| Package | Contains | Used by |
|---------|----------|---------|
| `_nativeassets` | Native binaries (per-platform frameworks/dylibs) | `externals-download` target |
| `_nugets` | Stable NuGet packages | `docs-download-output` target |
| `_nugetspreview` | Preview NuGet packages | `docs-download-output` target |

## Version Patterns on the CI Feed

Packages on the CI feed use `0.0.0` as the base version with a prerelease label encoding the source:

| Source | Version format | Example |
|--------|---------------|---------|
| **main branch (nightly)** | `0.0.0-branch.main.{build}` | `0.0.0-branch.main.3` |
| **develop branch** | `0.0.0-branch.develop.{build}` | `0.0.0-branch.develop.35` |
| **Custom branch** | `0.0.0-branch.{name}.{build}` | `0.0.0-branch.v2.80.4.9` |
| **PR build** | `0.0.0-pr.{number}.{build}` | `0.0.0-pr.1696.10` |
| **Commit** | `0.0.0-commit.{sha}.{build}` | `0.0.0-commit.013a831...2464` |
| **Release (stable)** | `0.0.0-branch.release.{ver}.{build}` | `0.0.0-branch.release.3.119.4.76` |
| **Release (preview)** | `0.0.0-branch.release.{ver}-preview.{n}.{build}` | `0.0.0-branch.release.2.88.9-preview.2.1646` |

The `{build}` number is the Azure DevOps build counter.

## NuGet Package Version Construction

When CI packs NuGet packages, it constructs preview versions like this:

```
{base_version}-{PREVIEW_LABEL}.{BUILD_NUMBER}
```

- **base_version**: From `scripts/VERSIONS.txt` (e.g. `3.119.4`)
- **PREVIEW_LABEL**: Set by CI, typically `preview.{increment}` where increment comes from `VERSIONS.txt`
- **BUILD_NUMBER**: The CI build counter

**Example:** `3.119.4-preview.0.76`
- Base: `3.119.4`
- Preview label: `preview.0` (where `0` = the `libSkiaSharp increment` value from `VERSIONS.txt`)
- Build number: `76`

## Cake Targets

| Target | What it does | Output directory |
|--------|-------------|-----------------|
| `externals-download` | Downloads native binaries from CI feed | `output/native/{platform}/` |
| `docs-download-output` | Downloads stable + preview NuGet packages from CI feed | `output/nugets/` |
| `samples-generate` | Copies samples to `output/`, converts ProjectRef → PackageRef | `output/samples/`, `output/samples-preview/` |
| `samples-prepare` | Clears cached SkiaSharp/HarfBuzz packages, copies nupkgs for Docker | — |
| `samples-run` | Builds all generated samples from `output/` | — |
| `samples` | Runs generate → prepare → run in sequence | — |

## Cake Arguments

| Argument | Environment variable | Default | Purpose |
|----------|---------------------|---------|---------|
| `--previewLabel` | `PREVIEW_LABEL` | `preview` | Preview suffix label |
| `--buildNumber` | `BUILD_NUMBER` | `0` | Build number appended to suffix |
| `--buildCounter` | `BUILD_COUNTER` | Same as `BUILD_NUMBER` | Build counter for CI version |
| `--artifactsFeed` | — | SkiaSharp-CI feed URL | Override the NuGet feed |
| `--gitBranch` | `GIT_BRANCH_NAME` | `""` | Branch name for CI package version |
| `--sample` | — | `""` | Filter to build a specific sample |

## How `samples-generate` Works

The `samples-generate` target copies the `samples/` directory into `output/samples/` and rewrites project files:

1. **`<ProjectReference>`** items are converted to **`<PackageReference>`** items using:
   - The referenced project's `<PackagingGroup>` as the package ID
   - The version from `scripts/VERSIONS.txt`
   - Preview suffix appended for SkiaSharp/HarfBuzzSharp packages

2. **Existing `<PackageReference>`** items have their versions updated from `VERSIONS.txt`

3. Two output trees are created:
   - `output/samples/` — uses stable package versions
   - `output/samples-preview/` — uses preview package versions (with suffix)

## Step-by-Step: Building Samples Locally

### 1. Clear cached packages

```bash
# Remove cached SkiaSharp/HarfBuzz packages so fresh ones are restored
rm -rf externals/package_cache/skiasharp*
rm -rf externals/package_cache/harfbuzzsharp*
```

### 2. Download CI packages

```bash
# Download both stable and preview NuGet packages from CI
dotnet cake --target=docs-download-output
```

This populates `output/nugets/` with `.nupkg` files.

### 3. Detect the preview version

Look at the downloaded preview packages to find the label and build number:

```bash
# Find a preview SkiaSharp package and extract the suffix
ls output/nugets/SkiaSharp.3*-*.nupkg
# Example output: SkiaSharp.3.119.4-preview.0.76.nupkg
# This means: previewLabel=preview.0, buildNumber=76
```

Parse it: everything between the base version and the last `.` before `.nupkg` is the label+build. The last numeric component is the build number; the rest is the preview label.

### 4. Build samples

```bash
# Build all samples with the detected preview label and build number
dotnet cake --target=samples --previewLabel=preview.0 --buildNumber=76
```

To build a **single sample** (e.g. tvOS only):

```bash
dotnet cake --target=samples --previewLabel=preview.0 --buildNumber=76 --sample=tvOS
```

### Complete one-liner

```bash
rm -rf externals/package_cache/skiasharp* externals/package_cache/harfbuzzsharp* && \
dotnet cake --target=docs-download-output && \
SUFFIX=$(ls output/nugets/SkiaSharp.3*-*.nupkg | head -1 | sed 's/.*SkiaSharp\.[0-9.]*-//' | sed 's/\.nupkg//') && \
LABEL=$(echo $SUFFIX | sed 's/\.[0-9]*$//') && \
BUILD=$(echo $SUFFIX | grep -o '[0-9]*$') && \
echo "Detected: previewLabel=$LABEL buildNumber=$BUILD" && \
dotnet cake --target=samples --previewLabel=$LABEL --buildNumber=$BUILD
```

## Building with Native Binaries (for local development)

If you need native binaries (e.g. for running samples that use project references directly):

```bash
# Download pre-built native binaries
dotnet cake --target=externals-download
```

This populates `output/native/` with platform-specific directories (`ios/`, `iossimulator/`, `tvos/`, `tvossimulator/`, `android/`, etc.).

> **Note:** `externals-download` and `docs-download-output` both clear `./output/` first, so run them in sequence if you need both native binaries and NuGet packages. Run `externals-download` first, then `docs-download-output` (which only clears `output/nugets/`-related content).

## Troubleshooting

### Stale cached packages
If samples build against old package versions, clear the package cache:
```bash
rm -rf externals/package_cache/skiasharp* externals/package_cache/harfbuzzsharp*
dotnet nuget locals all --clear
```

### tvOS/macOS/Tizen not building
Some platforms are disabled by default for local builds. Enable them:
```bash
dotnet build samples/Basic/tvOS/... -p:IsNetTVOSSupported=true
dotnet build samples/Basic/Tizen/... -p:IsNetTizenSupported=true
dotnet build samples/Basic/macOS/... -p:IsNetMacOSSupported=true
```

### WinUI XAML compiler failures on .NET 10
The WinUI sample requires a compatible `Microsoft.WindowsAppSDK` version. If the XAML compiler crashes, the SDK version may need updating.

### NuGet feed authentication
The SkiaSharp-CI feed is public — no authentication required.
