# Writing Documentation

> **New here?** Read **[docs-overview.md](docs-overview.md)** first — it maps the whole
> documentation system (the four artifacts, the engines, the skills, and the cross-repo
> automation). This page is the hands-on **operator** guide for generating and editing
> API docs and api diffs.

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
- **Manual trigger**: Go to [Actions](https://github.com/mono/SkiaSharp/actions/workflows/auto-docs-submodule-sync.yml) → "Sync - Docs Submodule" → "Run workflow"

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

This generates the XML API docs in the `docs/` directory. (The release-site API
diffs are produced separately by the single `docs-api-diff` target — see below.)

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
| `docs-api-diff` | Generates the committed release-site API diffs under `documentation/docfx/releases/` from published NuGet.org packages; incremental and scoped with `--force`, `--minVersion`, `--maxVersion` |
| `docs-update-frameworks` | Extracts assemblies, builds `frameworks.xml` with monikers, runs `mdoc update` to generate XML API docs |
| `docs-format-docs` | Cleans XML output, removes duplicates, syncs extension method docs, reports coverage |
| `update-docs` | Runs `docs-update-frameworks` → `docs-format-docs` in sequence |

> The single `docs-api-diff` target lives alongside the other doc engines at
> [`scripts/infra/docs/api-diff.cake`](../../scripts/infra/docs/api-diff.cake);
> `build.cake` exposes the target and uses `RunCake` to isolate the heavy
> API-diff addins there. The behavior is specified in
> [`release-notes-and-api-diffs.md`](release-notes-and-api-diffs.md) — read it
> before changing the engine.

## API diffs

The per-assembly API diffs are written into the in-site releases tree
(line-first, package-namespaced): `documentation/docfx/releases/<line>/{PackageId}/{Assembly}.md`
for SkiaSharp-versioned packages, and the parallel
`documentation/docfx/releases/harfbuzzsharp/<hb-line>/{PackageId}/{Assembly}.md`
tree for HarfBuzzSharp-versioned packages (see
[`release-notes-and-api-diffs.md`](release-notes-and-api-diffs.md) §3). They are
generated by `Mono.ApiTools.NuGetDiff` (the single `docs-api-diff` Cake target) and
kept up to date automatically as part of the **Prepare** phase of the unified
[`update-release-notes`](../../.github/workflows/update-release-notes.md) pipeline
(on push to `main` and on a daily cron — `main` walks every `release/*` ref and
reads `v*` tags itself, so release branches and tags need no trigger of their own).
That one workflow's Prepare phase regenerates both the API diffs and the
release-notes raw data, then the AI **Polish** phase rewrites prose, and it opens a
single rolling PR with both the api diffs and the notes — there is no separate
api-diff workflow.

To run the same full Prepare phase locally (API diffs + release-notes data + index):

```bash
dotnet tool restore
.agents/skills/release-notes/scripts/prepare.sh --force
git diff documentation/docfx/releases/
```

To regenerate only the API-diff tree while working on the Cake engine, call the
Cake target directly and pass Cake-style flags:

```bash
dotnet tool restore
dotnet cake --target=docs-api-diff --nugetDiffPrerelease=true --force=true
# optional scope:
# dotnet cake --target=docs-api-diff --nugetDiffPrerelease=true --minVersion=4.148.0 --maxVersion=4.150.0
```

Without `--force`, `docs-api-diff` skips any line whose committed API-diff folder
already has an `index.md`; a shipped version's public API diff is immutable, so the
folder is a cache. `--force` with no version scope rebuilds the whole back-catalogue
(the mode to use after changing the diff tools). `--minVersion` / `--maxVersion`
restrict the line cores rebuilt, while out-of-range lines remain available as
baselines for selected lines.

`docs-api-diff` diffs **published** NuGet.org versions: every emitted line is compared
against its predecessor, with baselines and superseded-version skips driven by
[`scripts/infra/docs/versions.json`](../../scripts/infra/docs/versions.json) (same
config the release-notes script uses). Superseded versions still get their own api
diff; they are only removed from the pool of *baselines*, so e.g. `4.148.0` walks
past the abandoned `4.147.*` previews and lands on `3.119.4`.

### Unpublished package diffs

The automated unpublished-package diff path was removed. `docs-api-diff` is the
committed, feed-based release-site generator; it does not compare freshly built local
NuGets from a branch against the feed, and the build/test pipeline no longer runs a
separate API-diff validation stage. For release-notes validation before merging a
pipeline change, dispatch `update-release-notes` with `source_branch` pointing at the
feature branch; it still uses the feed-based committed-diff engine described above.

### Relationship to release notes

The API diffs (Cake) and the website release notes
([`build-data.py`](../../.agents/skills/release-notes/scripts/build-data.py) + [`render-notes.py`](../../.agents/skills/release-notes/scripts/render-notes.py))
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

- **API diffs** produce one committed folder *per emitted line*, with
  package-namespaced, per-assembly files generated from that line's representative
  published package. The audience is "what changed in this line's public surface".
- **Release notes** produce one cumulative page *per release* (the highest
  branch for each version), diffed against the previous stable/baseline. The
  audience is "what's new since the last release".

So a given version can show a finer-grained baseline under
`documentation/docfx/releases/<line>/` than on its release-notes page, even
though both agree on supersession and any explicit `compare_to` override.

