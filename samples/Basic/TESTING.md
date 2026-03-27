# Testing Samples with Arbitrary SkiaSharp NuGet Versions

This guide explains how to test the SkiaSharp samples using a specific version of the SkiaSharp NuGet packages.

## Prerequisites

- [.NET 10 SDK](https://dotnet.microsoft.com/download)
- [Docker](https://docs.docker.com/get-docker/) (for Docker samples)
- [PowerShell](https://github.com/PowerShell/PowerShell) (for `run.ps1` scripts)

## Quick Start

```bash
# 1. Generate the samples (replaces project refs with NuGet package refs using VERSIONS.txt)
dotnet cake --target=samples-generate

# 2. Build and test all samples
dotnet cake --target=samples --previewLabel ""

# 3. Or test a single sample
dotnet cake --target=samples --sample "Basic/DockerConsole" --previewLabel ""
```

## How It Works

### Step 1: Version Resolution

The `samples-generate` target copies `samples/` to `output/samples/` and transforms:
- `<ProjectReference>` → `<PackageReference>` (using the `PackagingGroup` from the referenced project)
- `Version="1.0.0"` → real version from `scripts/VERSIONS.txt`
- Removes `<Import>` of internal build targets

### Step 2: NuGet Packages

For Docker samples, the `samples` target copies NuGet packages from `output/nugets/` into a `packages/` directory next to each Dockerfile. The `nuget.config` in each Docker sample lists `packages/` as a local source with `nuget.org` as fallback.

### Step 3: Build and Run

- **Regular samples** are built with `dotnet build` against the solution files
- **Docker samples** are built and run via their `run.ps1` scripts

## The `--sample` Filter

Use `--sample` to run only a specific sample:

```bash
# Only DockerConsole
dotnet cake --target=samples --sample "Basic/DockerConsole"

# Only DockerWebApi
dotnet cake --target=samples --sample "Basic/DockerWebApi"

# Only the Web sample
dotnet cake --target=samples --sample "Basic/Web"
```

The filter matches against the full path, so `Basic/Docker` would match both `DockerConsole` and `DockerWebApi`.

## The `--previewLabel` Flag

By default, `samples-generate` creates two outputs:
- `output/samples/` — stable versions (e.g., `3.119.4`)
- `output/samples-preview/` — preview versions (e.g., `3.119.4-preview.0`)

The `samples` target uses `output/samples-preview/` if `PREVIEW_ONLY_NUGETS` is non-empty. Pass `--previewLabel ""` to force using stable versions.

## Docker Samples

Docker samples use NuGet `PackageReference` with `Version="1.0.0"` as a placeholder. The `samples-generate` step replaces this with the real version. The Dockerfiles expect:

- `nuget.config` — with `packages/` local source
- `packages/` — directory containing `.nupkg` files (copied by the `samples` cake target)

When running publicly (after download from the samples zip), the `packages/` dir is empty and packages restore from `nuget.org` with whatever version is in the `.csproj`.
