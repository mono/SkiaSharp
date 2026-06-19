# Writing Documentation

> **New here?** Read **[docs-overview.md](docs-overview.md)** first — it maps the whole
> documentation system (the four artifacts, the engines, the skills, and the cross-repo
> automation). This page is the hands-on **operator** guide for generating and editing
> API docs and changelogs.

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

Two workflows automate the full docs lifecycle. They live in **different repos** —
the stub-generation/AI-writing workflow is in the docs repo (it writes into that
repo), and the submodule-sync workflow is here:

### Step 1: Stub generation + AI docs (auto-api-docs-writer)

A single agentic workflow handles the entire pipeline — regenerating XML stubs from CI NuGet packages AND filling "To be added." placeholders with AI-written documentation:

- **Workflow**: [`auto-api-docs-writer.md`](https://github.com/mono/SkiaSharp-API-docs/blob/main/.github/workflows/auto-api-docs-writer.md) (agentic, in the **docs repo** `mono/SkiaSharp-API-docs` — it checks SkiaSharp out to borrow the engines, then writes the XML into the docs repo)
- **Schedule**: Daily at 8 AM UTC
- **What it does**:
  1. **Pre-agent step**: Downloads latest NuGet packages from CI, runs `dotnet cake --target=update-docs` to regenerate XML stubs
  2. **AI agent**: Reads the `api-docs` skill, finds all "To be added." placeholders, reads C# source code, writes proper documentation
  3. **Post-step**: Pushes branch and creates PR in `mono/SkiaSharp-API-docs`
- **Output**: PR `automation/write-api-docs` → `main` in `mono/SkiaSharp-API-docs`
- **Manual trigger**: Go to [Actions](https://github.com/mono/SkiaSharp-API-docs/actions/workflows/auto-api-docs-writer.lock.yml) → "Auto API Docs Writer" → "Run workflow"

> **Runner note**: `mdoc.exe` is a .NET Framework executable, but `docs.cake` runs it
> under **mono**, so the stub-regeneration job runs on a Linux runner with
> `mono-complete` installed (see [docs-overview.md](docs-overview.md) → Path 3).

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

This generates the XML API docs in the `docs/` directory. (The API changelogs
are produced separately by the `docs-api-diff*` targets — see below.)

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

- [`.agents/skills/api-docs/references/patterns.md`](../../.agents/skills/api-docs/references/patterns.md) — XML syntax and examples
- [`.agents/skills/api-docs/references/checklist.md`](../../.agents/skills/api-docs/references/checklist.md) — Review severity criteria

## Cake Targets Reference

| Target | Description |
|--------|-------------|
| `docs-download-output` | Downloads `_nugets` and `_nugetspreview` packages from the SkiaSharp-CI Azure Artifacts feed |
| `docs-api-diff` | Compares current (unpublished CI) NuGets against latest published versions; writes transient markdown diffs to `output/api-diff/` only (a CI gate — never the committed tree) |
| `docs-api-diff-past` | Regenerates the committed historical API changelogs under `documentation/docfx/releases/` across all published versions |
| `docs-update-frameworks` | Extracts assemblies, builds `frameworks.xml` with monikers, runs `mdoc update` to generate XML API docs |
| `docs-format-docs` | Cleans XML output, removes duplicates, syncs extension method docs, reports coverage |
| `update-docs` | Runs `docs-update-frameworks` → `docs-format-docs` in sequence (the current API diff is a standalone CI gate and is no longer bundled here) |

> The two `docs-api-diff*` targets live alongside the other doc engines at
> [`scripts/infra/docs/api-diff.cake`](../../scripts/infra/docs/api-diff.cake);
> the wrappers in `build.cake` just forward to it. The behavior is specified in
> [`release-notes-and-changelogs.md`](release-notes-and-changelogs.md) — read it
> before changing either engine.

## API changelogs

The per-assembly API changelogs are written into the in-site releases tree
(line-first, package-namespaced): `documentation/docfx/releases/<line>/{PackageId}/{Assembly}.md`
for SkiaSharp-versioned packages, and the parallel
`documentation/docfx/releases/harfbuzzsharp/<hb-line>/{PackageId}/{Assembly}.md`
tree for HarfBuzzSharp-versioned packages (see
[`release-notes-and-changelogs.md`](release-notes-and-changelogs.md) §3). They are
generated by `Mono.ApiTools.NuGetDiff` (the `docs-api-diff-past` Cake target) and
kept up to date automatically as part of the **Prepare** phase of the unified
[`update-release-notes`](../../.github/workflows/update-release-notes.md) pipeline
(on push to `main` and `release/*`, on `v*` tags, and weekly). That one workflow's
Prepare phase regenerates both the API changelogs and the release-notes raw data,
then the AI **Polish** phase rewrites prose, and it opens a single rolling PR with
both the changelogs and the notes — there is no separate api-diff workflow.

To regenerate the full committed set locally exactly the way CI does:

```bash
dotnet tool restore
dotnet cake --target=docs-api-diff-past --nugetDiffPrerelease=true
git diff documentation/docfx/releases/
```

`docs-api-diff-past` diffs **published** NuGet.org versions: every version is
compared against its predecessor, with baselines and superseded-version skips
driven by [`scripts/infra/docs/versions.json`](../../scripts/infra/docs/versions.json) (same config the
release-notes script uses). Superseded versions still get their own changelog;
they are only removed from the pool of *baselines*, so e.g. `4.148.0` walks past
the abandoned `4.147.*` previews and lands on `3.119.4`.

### Previewing an unreleased branch

Previewing the API diff of a branch/tag that is **not yet on NuGet.org** is a
manual, multi-step operation (it is intentionally not part of the automated
workflow). It diffs CI packages for the ref against their NuGet.org baseline:

1. Overlay the ref's nuget versions onto `scripts/VERSIONS.txt` so the download
   and diff targets resolve the right package versions (skip this if previewing
   `main`, which is already the working tree):

   ```bash
   git fetch --depth=1 origin release/4.150.0-preview.1
   git show FETCH_HEAD:scripts/VERSIONS.txt | grep -E '[[:space:]]nuget[[:space:]]'
   # replace the "# nuget versions" block in scripts/VERSIONS.txt with those lines
   ```

2. Download the CI packages for the ref and run the diff against the feed:

   ```bash
   dotnet cake --target=docs-download-output --gitBranch=release/4.150.0-preview.1
   dotnet cake --target=docs-api-diff --gitBranch=release/4.150.0-preview.1 --nugetDiffPrerelease=true
   ```

   (Use `--gitSha=<40-char-sha>` instead of `--gitBranch` for a tag or commit.)

The preview output lands in `output/api-diff/` for inspection — it is **not**
meant to be committed.

### Relationship to release notes

The API changelogs (Cake) and the website release notes
([`generate-release-notes.py`](../../scripts/infra/docs/generate-release-notes.py))
are **separate systems** that deliberately share only one thing:
[`scripts/infra/docs/versions.json`](../../scripts/infra/docs/versions.json). That file is the single
source of truth for two decisions, and both systems honour it identically:

- **Supersession** — only a version with an explicit `status: superseded` entry
  is treated as superseded (Cake's `IsVersionSuperseded`, Python's
  `resolve_superseded_by`). Neither side auto-detects it; to skip a version
  everywhere, add it to `versions.json`.
- **`compare_to` baselines** — when present, both sides diff against the same
  baseline version (e.g. `4.148.0` → `3.119.4`).

For any version *not* carrying a `compare_to` override, each system picks the
default baseline (the previous version) on its own, and the two can differ
slightly: the Python release-notes generator additionally skips any candidate
baseline that has no stable git tag, whereas Cake walks purely by version order.
This only matters for unlisted, preview-only versions; add a `compare_to` entry
to `versions.json` to pin both systems to the same baseline.

Where they intentionally differ is **granularity**, and this is by design — do
not "fix" them into agreement:

- **API changelogs** produce one diff *per published NuGet package*, including
  preview-to-preview deltas (e.g. each `4.147.0-preview.*` against the one
  before it). The audience is "what changed in this exact package".
- **Release notes** produce one cumulative page *per release* (the highest
  branch for each version), diffed against the previous stable/baseline. The
  audience is "what's new since the last release".

So a given version can show a finer-grained baseline under
`documentation/docfx/releases/<line>/` than on its release-notes page, even
though both agree on supersession and any explicit `compare_to` override.

