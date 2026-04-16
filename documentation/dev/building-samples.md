# Building and Validating Samples

This guide explains how to build SkiaSharp samples using CI-produced NuGet packages. The samples use **package references** (not project references) when built through the `samples` cake target, so they need downloadable NuGet packages to compile.

## CI Artifacts Feed

All CI builds publish wrapper packages to the **SkiaSharp-CI** Azure DevOps feed:

```
https://pkgs.dev.azure.com/xamarin/public/_packaging/SkiaSharp-CI/nuget/v3/index.json
```

These wrapper packages bundle the real NuGet packages inside their `tools/` directory:

| Wrapper package | Contains |
|-----------------|----------|
| `_nativeassets` | Native binaries (per-platform frameworks/dylibs) |
| `_nugets` | Stable NuGet packages (e.g. `SkiaSharp.3.119.4.nupkg`) |
| `_nugetspreview` | Preview NuGet packages (e.g. `SkiaSharp.3.119.4-preview.0.76.nupkg`) |

The wrapper packages use `0.0.0-{source}.{build}` versioning to identify their CI source. The actual NuGet packages inside have their real, user-facing version numbers.

## Two-Step Process

Building samples requires two separate sets of arguments because the CI feed version and the NuGet package version are different things:

### Step 1: Download — select which CI build to fetch

The `docs-download-output` target resolves the CI wrapper package version using these args (checked in priority order):

| Argument | Resolves to | Use case |
|----------|------------|----------|
| `--previewLabel=pr.3553` | `0.0.0-pr.3553.*` | PR build |
| `--gitSha=abc123` | `0.0.0-commit.abc123.*` | Specific commit |
| `--gitBranch=release/3.119.4` | `0.0.0-branch.release.3.119.4.*` | Release branch |
| `--gitBranch=main` | `0.0.0-branch.main.*` | Main branch (nightly) |
| *(no args)* | `0.0.0-branch.main.*` | Default: latest from main |

The `.*` wildcard selects the **latest** matching build from the feed.

### Step 2: Build samples — use the real NuGet version

After downloading, the extracted nupkgs in `output/nugets/` have real version numbers. The `samples` target needs `--previewLabel` and `--buildNumber` matching these real versions:

```powershell
# Detect from downloaded packages
ls output/nugets/SkiaSharp.3*-*.nupkg
# → SkiaSharp.3.119.4-preview.0.76.nupkg
# So: --previewLabel=preview.0 --buildNumber=76
```

## NuGet Package Version Construction

The cake build constructs the NuGet preview suffix in `build.cake` (lines 56-72):

```csharp
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
```

The final NuGet version is `{base_version}-{PREVIEW_NUGET_SUFFIX}`:

- **base_version**: From `scripts/VERSIONS.txt` (e.g. `3.119.4`)
- **PREVIEW_LABEL**: The preview label (e.g. `preview.0` — first preview, `preview.1` — second, etc.)
- **BUILD_NUMBER**: The CI build counter

**Example:** `3.119.4-preview.0.76` → `previewLabel=preview.0`, `buildNumber=76`

## Cake Arguments

### For downloading (`docs-download-output`)

These arguments control **which CI build** to fetch from the feed:

| Argument | Environment variable | Default | Purpose |
|----------|---------------------|---------|---------|
| `--previewLabel` | `PREVIEW_LABEL` | `preview` | When starts with `pr.`, fetches PR build |
| `--gitSha` | `GIT_SHA` | `""` | Fetch by commit SHA |
| `--gitBranch` | `GIT_BRANCH_NAME` | `""` | Fetch by branch name |
| `--previewFeed` | — | SkiaSharp-CI URL | Override the NuGet feed |

### For building samples (`samples`)

These arguments control the **NuGet version suffix** used when rewriting package references:

| Argument | Environment variable | Default | Purpose |
|----------|---------------------|---------|---------|
| `--previewLabel` | `PREVIEW_LABEL` | `preview` | Preview suffix label |
| `--buildNumber` | `BUILD_NUMBER` | `0` | Build number for suffix |
| `--sample` | — | `""` | Filter to build a specific sample |

> **Note:** `--previewLabel` serves double duty: it selects the CI artifact during download AND forms the NuGet suffix during sample generation. For nightly builds from main, you typically run download with default args, then set `--previewLabel` and `--buildNumber` to match the extracted packages.

## Cake Targets

| Target | What it does | Output directory |
|--------|-------------|-----------------|
| `docs-download-output` | Downloads stable + preview NuGet packages from CI feed | `output/nugets/` |
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

See [`.claude/skills/validate-samples/SKILL.md`](../../.claude/skills/validate-samples/SKILL.md)
for the full step-by-step workflow if you need to run it manually.

## How `samples-generate` Works

The `CreateSamplesDirectory()` function in `scripts/cake/samples.cake`:

1. **`<ProjectReference>`** → converted to `<PackageReference>` using the project's `<PackagingGroup>` as the package ID and version from `VERSIONS.txt`
2. **Existing `<PackageReference>`** → version updated from `VERSIONS.txt`
3. For SkiaSharp/HarfBuzzSharp packages, the preview suffix is appended
4. Two output trees: `output/samples/` (stable) and `output/samples-preview/` (preview)

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
The SkiaSharp-CI feed is public — no authentication required.
