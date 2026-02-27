# Writing Documentation

This guide covers generating and updating SkiaSharp API documentation.

SkiaSharp provides two types of documentation: concept docs and API docs.

## Concept Docs

The concept docs are on the `docs` branch of this repository and published to:

- https://mono.github.io/SkiaSharp/

## API Docs

The API docs are XML reference documentation generated from SkiaSharp assemblies using [mdoc](https://github.com/mono/api-doc-tools). They are published to https://docs.microsoft.com/dotnet/api/skiasharp.

Because the docs are large and contain examples, images, and other assets, they are hosted in a separate repository:

- https://github.com/mono/SkiaSharp-API-docs

This repository is pulled into the main SkiaSharp repo as a Git submodule at `docs/`. The XML files live under `docs/SkiaSharpAPI/`.

## Automated Pipeline

A GitHub Actions workflow in [`mono/SkiaSharp-API-docs`](https://github.com/mono/SkiaSharp-API-docs) regenerates the API docs daily from the latest CI build artifacts. It checks out this repo (for the Cake build scripts) and the docs repo, then runs the doc generation targets:

- **Workflow**: [`update-docs.yml`](https://github.com/mono/SkiaSharp-API-docs/blob/main/.github/workflows/update-docs.yml) (in the docs repo)
- **Schedule**: Runs daily at 6 AM UTC
- **Manual trigger**: Go to the [Actions tab](https://github.com/mono/SkiaSharp-API-docs/actions) on `mono/SkiaSharp-API-docs` → "Update API Docs" → "Run workflow"
- **Branch parameter**: Optionally specify a SkiaSharp branch to generate docs from (defaults to `main`)

### What the pipeline does

1. Downloads the latest NuGet packages from the [SkiaSharp-CI](https://pkgs.dev.azure.com/xamarin/public/_packaging/SkiaSharp-CI/nuget/v3/index.json) Azure Artifacts feed (public, no authentication required)
2. Runs `dotnet cake --target=update-docs` which:
   - Generates API diffs and changelogs (`docs-api-diff`)
   - Extracts assemblies from NuGets, builds framework monikers, and runs `mdoc update` to regenerate XML docs (`docs-update-frameworks`)
   - Cleans up, formats, and validates the generated XML (`docs-format-docs`)
3. If there are changes, creates a PR on [`mono/SkiaSharp-API-docs`](https://github.com/mono/SkiaSharp-API-docs)

### After the PR is merged

Once the docs PR is merged into `mono/SkiaSharp-API-docs`, update the submodule pointer in the main SkiaSharp repo:

```bash
cd docs
git checkout master
git pull
cd ..
git add docs
git commit -m "Update docs submodule"
```

## Generating Docs Locally

### Prerequisites

```bash
dotnet tool restore
```

### Step 1: Create a branch in the docs submodule

```bash
cd docs
git checkout -b my-docs-update
cd ..
```

### Step 2: Download the latest NuGet packages

```bash
dotnet cake --target=docs-download-output
```

This downloads NuGet packages from the SkiaSharp-CI feed. By default it fetches from the `main` branch. To use a different branch:

```bash
dotnet cake --target=docs-download-output --gitBranch=my-feature-branch
```

### Step 3: Regenerate the docs

```bash
dotnet cake --target=update-docs
```

This generates both the changelogs and the XML API docs in the `docs/` directory.

## Editing API Docs

After generating, you can open the XML files in any text editor and add or update documentation. Any members that do not have content will have the placeholder text "To be added."

You can also use the `api-docs` Copilot skill to write and review XML documentation. Ask Copilot to "fill in missing docs", "document SKCanvas", or "review documentation quality" and it will follow the .NET API documentation guidelines automatically.

As you edit, you can apply formatting and see what docs are still missing by running:

```bash
dotnet cake --target=docs-format-docs
```

This will report a summary of documentation coverage (types and members with missing docs).

Once you are happy with your changes, push them to your fork of [`mono/SkiaSharp-API-docs`](https://github.com/mono/SkiaSharp-API-docs) and open a PR.

For detailed XML documentation patterns and review criteria, see:

- [`.github/skills/api-docs/references/patterns.md`](../.github/skills/api-docs/references/patterns.md) — XML syntax and examples
- [`.github/skills/api-docs/references/checklist.md`](../.github/skills/api-docs/references/checklist.md) — Review severity criteria

## Cake Targets Reference

| Target | Description |
|--------|-------------|
| `docs-download-output` | Downloads `_nugets` and `_nugetspreview` packages from the SkiaSharp-CI Azure Artifacts feed |
| `docs-api-diff` | Compares current NuGets against latest published versions, generates markdown diffs and changelogs |
| `docs-api-diff-past` | Generates historical API diffs across all published versions |
| `docs-update-frameworks` | Extracts assemblies, builds `frameworks.xml` with monikers, runs `mdoc update` to generate XML API docs |
| `docs-format-docs` | Cleans XML output, removes duplicates, syncs extension method docs, reports coverage |
| `update-docs` | Runs `docs-api-diff` → `docs-update-frameworks` → `docs-format-docs` in sequence |

