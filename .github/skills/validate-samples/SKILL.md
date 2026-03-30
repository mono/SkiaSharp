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

### Step 1: Clear cached packages

```powershell
rm -r -fo externals/package_cache/skiasharp*, externals/package_cache/harfbuzzsharp*
```

If you suspect deeper caching issues, also clear the global NuGet cache:

```powershell
dotnet nuget locals all --clear
```

### Step 2: Download CI packages

Downloads the latest NuGet packages from the CI feed into `output/nugets/`.
This target clears `./output/` first.

```powershell
dotnet cake --target=docs-download-output
```

To download from a specific source instead of the latest main build:

```powershell
# From a PR
dotnet cake --target=docs-download-output --previewLabel=pr.3553

# From a specific branch
dotnet cake --target=docs-download-output --gitBranch=release/3.119.4

# From a specific commit
dotnet cake --target=docs-download-output --gitSha=abc123def456
```

### Step 3: Detect the preview version

Run the detection script — it prints the preview label and build number
extracted from the downloaded nupkg filenames:

```powershell
pwsh .github/skills/validate-samples/scripts/detect-preview-version.ps1
```

Output:
```
Found: SkiaSharp.3.119.4-preview.0.76.nupkg
Preview label: preview.0
Build number:  76
Full suffix:   preview.0.76
```

Parse `Preview label` and `Build number` from the output for the next step.

### Step 4: Build samples

```powershell
dotnet cake --target=samples --previewLabel=<PREVIEW_LABEL> --buildNumber=<BUILD_NUMBER>
```

To build a single sample, add `--sample=<name>`:

```powershell
dotnet cake --target=samples --previewLabel=<PREVIEW_LABEL> --buildNumber=<BUILD_NUMBER> --sample=Blazor
```

## Troubleshooting

### Stale packages after repeated runs
```powershell
rm -r -fo externals/package_cache/skiasharp*, externals/package_cache/harfbuzzsharp*
dotnet nuget locals all --clear
```

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
Docker samples are built via `run.ps1` inside Docker, not `dotnet build`.
The `samples-prepare` target copies nupkgs there automatically.

## Further Reading

See [Building Samples](../../documentation/dev/building-samples.md) for version construction
details, download resolution, cake arguments reference, and how `samples-generate` works.
