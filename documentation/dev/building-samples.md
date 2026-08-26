# Building and Validating Samples

This guide explains how to build SkiaSharp samples using CI-produced NuGet packages. The samples use **package references** (not project references) when built through the `samples` cake target, so they need downloadable NuGet packages to compile.

## Transport Feed

Official builds register wrapper packages as non-shipping assets in the same BAR
as the product packages. Maestro `.NET Libraries` routes them by asset class to
the **dotnet-libraries-transport** Azure DevOps feed; transport is not a separate
channel:

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

### Step 1: Download packages

Promoted branch builds are available from the transport feed:

| Argument | Resolves to | Use case |
|----------|------------|----------|
| `--gitBranch=release/3.119.4` | `0.0.0-branch.release.3.119.4.*` | Release branch |
| `--gitBranch=main` | `0.0.0-branch.main.*` | Main branch (nightly) |
| *(no args)* | `0.0.0-branch.main.*` | Default: latest from main |

The `.*` wildcard selects the **latest** matching build from the feed.

PR builds are not published to that feed. Use `scripts/get-skiasharp-pr.ps1`
or `.sh`, then copy packages from
`~/.skiasharp/hives/pr-{number}/packages/` to `output/nugets/`.

For another exact public build, download its canonical `nuget` pipeline artifact
and extract non-symbol packages to `output/nugets/`.

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

### For downloading (`docs-download-output`)

These arguments control **which CI build** to fetch from the feed:

| Argument | Environment variable | Default | Purpose |
|----------|---------------------|---------|---------|
| `--gitBranch` | `GIT_BRANCH_NAME` | `""` | Fetch by branch name |
| `--previewFeed` | — | SkiaSharp Transport URL | Override the NuGet feed |

### For building samples (`samples`)

These arguments control the **NuGet version suffix** used when rewriting package references:

| Argument | Environment variable | Default | Purpose |
|----------|---------------------|---------|---------|
| `--previewLabel` | `PREVIEW_LABEL` | `preview` | Preview suffix label |
| `--buildNumber` | `BUILD_NUMBER` | `0` | Build number for suffix |
| `--dotNetFinalVersionKind` | `DOTNET_FINAL_VERSION_KIND` | `""` | Set to `release` for an exact stable version |
| `--sample` | — | `""` | Filter to build a specific sample |

> **Note:** `--previewLabel` serves double duty: it selects the CI artifact during download AND forms the NuGet suffix during sample generation. For nightly builds from main, you typically run download with default args, then set `--previewLabel` and `--buildNumber` to match the extracted packages.

## Cake Targets

| Target | What it does | Output directory |
|--------|-------------|-----------------|
| `docs-download-output` | Downloads the build's NuGet package family from the CI feed | `output/nugets/` |
| `samples-generate` | Copies samples to `output/`, converts ProjectRef → PackageRef | `output/samples/`, `output/samples-preview/` |
| `samples-prepare` | Clears cached SkiaSharp/HarfBuzz packages, copies nupkgs for Docker | — |
| `samples-run` | Builds all generated samples from `output/` | — |
| `samples` | Runs generate → prepare → run in sequence | — |

## Building Samples

The easiest way to build and validate samples is with the **`validate-samples`** Copilot skill.
Ask Copilot to run it — it handles downloading packages, detecting versions, and building automatically.

Example prompts:
- "validate samples"
- "build the samples against the latest CI packages"
- "check if the Blazor sample builds"
- "validate samples from PR 3553"
- "do the samples build after my changes?"

The skill follows the workflow described in the reference sections above: clear cache → download
CI packages → detect preview version → build with `dotnet cake --target=samples`.

See [`.agents/skills/validate-samples/SKILL.md`](../../.agents/skills/validate-samples/SKILL.md)
for the full step-by-step workflow if you need to run it manually.

## How `samples-generate` Works

The `CreateSamplesDirectory()` function in `scripts/infra/samples/samples.cake`:

1. **`<ProjectReference>`** → converted to `<PackageReference>` using the project's `<PackagingGroup>` as the package ID and version from `VERSIONS.txt`
2. **Existing `<PackageReference>`** → version updated from `VERSIONS.txt`
3. For SkiaSharp/HarfBuzzSharp packages, the preview suffix is appended
4. Two output trees: `output/samples/` (stable) and `output/samples-preview/` (preview)

`samples-run` selects the stable tree only for an exact release identity. Any
non-empty `PREVIEW_NUGET_SUFFIX` selects the preview tree so its references
match the single package family emitted by that build.

## Troubleshooting

### Stale cached packages
```powershell
rm -r -fo externals/package_cache/skiasharp*, externals/package_cache/harfbuzzsharp*
dotnet nuget locals all --clear
```

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
The SkiaSharp Transport feed is public — no authentication required.
