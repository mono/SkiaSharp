# Writing Documentation

This guide covers generating and updating SkiaSharp API documentation.

SkiaSharp provides two types of documentation: concept docs and API docs.

## Concept Docs

The conceptual docs live on the `main` branch under `documentation/conceptual/` and are published to:

- https://mono.github.io/SkiaSharp/docs/

See [site.md](site.md) for how to build, preview, and customize the docs site.

## API Docs

The API docs are XML reference documentation generated from SkiaSharp assemblies using [mdoc](https://github.com/mono/api-doc-tools). They are published to https://docs.microsoft.com/dotnet/api/skiasharp.

Because the docs are large and contain examples, images, and other assets, they are hosted in a separate repository:

- https://github.com/mono/SkiaSharp-API-docs

This repository is pulled into the main SkiaSharp repo as a Git submodule at `docs/`. The XML files live under `docs/SkiaSharpAPI/`.

## Automated Pipeline

Two workflows in this repo automate the full docs lifecycle:

### Step 1: Stub generation + AI docs (auto-api-docs-writer)

A single agentic workflow handles the entire pipeline — regenerating XML stubs from CI NuGet packages AND filling "To be added." placeholders with AI-written documentation:

- **Workflow**: [`auto-api-docs-writer.md`](../../.github/workflows/auto-api-docs-writer.md) (agentic, in this repo)
- **Schedule**: Daily at 8 AM UTC
- **What it does**:
  1. **Pre-agent step**: Downloads latest NuGet packages from CI, runs `dotnet cake --target=update-docs` to regenerate XML stubs
  2. **AI agent**: Reads the `api-docs` skill, finds all "To be added." placeholders, reads C# source code, writes proper documentation
  3. **Post-step**: Pushes branch and creates PR in `mono/SkiaSharp-API-docs`
- **Output**: PR `automation/write-api-docs` → `main` in `mono/SkiaSharp-API-docs`
- **Manual trigger**: Go to [Actions](https://github.com/mono/SkiaSharp/actions/workflows/auto-api-docs-writer.lock.yml) → "Auto API Docs Writer" → "Run workflow"
- **Skip regeneration**: Set `skip_regeneration=true` to skip the stub generation step (useful when stubs are already up to date and you only want AI doc writing)

> **Note**: There is also a standalone [`update-docs.yml`](https://github.com/mono/SkiaSharp-API-docs/blob/main/.github/workflows/update-docs.yml) workflow in the docs repo that only regenerates stubs (no AI writing). The agentic workflow above supersedes it for the full pipeline.

### Step 2: Submodule sync (auto-docs-submodule-sync)

After docs changes are merged to `mono/SkiaSharp-API-docs` `main` (whether from the AI pipeline, manual edits, or any other source), the submodule pointer in this repo needs to be updated:

- **Workflow**: [`auto-docs-submodule-sync.yml`](../../.github/workflows/auto-docs-submodule-sync.yml) (in this repo)
- **Schedule**: Daily at 10 AM UTC
- **What it does**: Compares the `docs/` submodule SHA with the latest `mono/SkiaSharp-API-docs` `main` SHA. If behind, creates a PR to bump the submodule.
- **Output**: PR `automation/update-docs-submodule` → `main` in `mono/SkiaSharp`
- **Manual trigger**: Go to [Actions](https://github.com/mono/SkiaSharp/actions/workflows/auto-docs-submodule-sync.yml) → "Auto Docs Submodule Sync" → "Run workflow"

### Pipeline timeline

| Time (UTC) | Workflow | Action |
|------------|----------|--------|
| 8:00 AM | `auto-api-docs-writer` | Regenerates stubs + AI fills placeholders → PR to docs repo |
| 10:00 AM | `auto-docs-submodule-sync` | Bumps `docs/` submodule → PR to SkiaSharp |

Each step produces a PR that requires human review before merging.

### Manual submodule update

You can also update the submodule manually:

```bash
cd docs
git checkout main
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

- [`.claude/skills/api-docs/references/patterns.md`](../../.claude/skills/api-docs/references/patterns.md) — XML syntax and examples
- [`.claude/skills/api-docs/references/checklist.md`](../../.claude/skills/api-docs/references/checklist.md) — Review severity criteria

## Cake Targets Reference

| Target | Description |
|--------|-------------|
| `docs-download-output` | Downloads `_nugets` and `_nugetspreview` packages from the SkiaSharp-CI Azure Artifacts feed |
| `docs-api-diff` | Compares current NuGets against latest published versions, generates markdown diffs and changelogs |
| `docs-api-diff-past` | Generates historical API diffs across all published versions |
| `docs-update-frameworks` | Extracts assemblies, builds `frameworks.xml` with monikers, runs `mdoc update` to generate XML API docs |
| `docs-format-docs` | Cleans XML output, removes duplicates, syncs extension method docs, reports coverage |
| `update-docs` | Runs `docs-api-diff` → `docs-update-frameworks` → `docs-format-docs` in sequence |

