# SkiaSharp documentation — how the pieces fit together

This is the **map** of SkiaSharp's documentation system: what gets generated, by
which engine, driven by which skill, on which workflow, and in which repository.
Start here for the big picture, then jump to a deep dive.

## Start here — what are you trying to do?

| If you want to… | Go to |
|---|---|
| **Understand the whole system** at a glance | This page (the map) — read on |
| **Generate or edit docs by hand** — prerequisites, local commands, editing API docs, the Cake target reference | [writing-docs.md](writing-docs.md) |
| **Change how release notes or API diffs behave** — the versioning model, `versions.json`, file layout, per-engine rules | [release-notes-and-api-diffs.md](release-notes-and-api-diffs.md) (the behavior **spec**; read before changing either generator) |
| **Work on the docs website** — build, preview, theming | [site.md](site.md) |

## Contents

- [The four kinds of documentation](#the-four-kinds-of-documentation)
- [Two repositories](#two-repositories)
- [The engines (`scripts/infra/docs/`)](#the-engines-scriptsinfradocs)
- [The skills](#the-skills)
- [CI automation (cross-repo)](#ci-automation-cross-repo)
- [Local generation & Docker](#local-generation--docker)
- [Where to go next](#where-to-go-next)

---

## The four kinds of documentation

SkiaSharp ships four distinct documentation artifacts. They are produced by
different tools and live in different places, which is the main reason the system can
feel sprawling — this table is the whole story on one screen:

| # | Artifact | What it is | Source of truth | Engine |
|---|----------|------------|-----------------|--------|
| 1 | **Conceptual docs** | Hand-written guides & tutorials on the docs site | `documentation/conceptual/` (this repo) | docfx ([site.md](site.md)) |
| 2 | **API reference docs** | Per-type/per-member XML reference (→ learn.microsoft.com) | `docs/SkiaSharpAPI/` (the **docs submodule**) | `docs.cake` (mdoc) |
| 3 | **Release notes** | Human "what's new" pages, AI-polished | `documentation/docfx/releases/` (this repo) | `generate-release-notes.py` |
| 4 | **API diffs** | Machine-generated public-API diffs, no AI | `documentation/docfx/releases/` (this repo) | `api-diff.cake` |

Artifacts **3** and **4** share one versioning model and one config file
(`versions.json`); that shared model is the subject of the
[behavior spec](release-notes-and-api-diffs.md). Artifact **2** is a separate
concern (mdoc), but it shares the same NuGet-diff plumbing
(`api-diff-tools.cake`) and lives next to the others.

---

## Two repositories

```mermaid
flowchart LR
  subgraph A["mono/SkiaSharp (this repo)"]
    conceptual["documentation/conceptual/ — concept docs"]
    releases["documentation/docfx/releases/ — release notes + API diffs"]
    engines["scripts/infra/docs/ — the engines"]
    submodule["docs/ (git submodule)"]
  end
  subgraph B["mono/SkiaSharp-API-docs (the docs repo)"]
    xml["SkiaSharpAPI/ — mdoc XML reference docs"]
  end
  submodule -. "points at" .-> B
  engines --> releases
  engines -. "mdoc writes into" .-> xml
```

- **`mono/SkiaSharp`** (this repo) holds the conceptual docs, the release
  notes/api diffs tree, and **all the generation engines**
  (`scripts/infra/docs/`).
- **`mono/SkiaSharp-API-docs`** (the *docs repo*) holds the large mdoc XML
  reference docs and their images/examples. It is pulled into this repo as a git
  **submodule** at `docs/`, so the XML lives at `docs/SkiaSharpAPI/`. The engines
  in this repo run mdoc *into* that submodule.

Because the engines live here but the XML output lives there, the API-reference
path is inherently **cross-repo** — see the automation section below.

---

## The engines (`scripts/infra/docs/`)

All generation code lives in one directory so local runs, CI, and the local Docker
image all share exactly one copy and nothing can drift:

| File | Role |
|------|------|
| `docs.cake` | mdoc-based XML reference-doc generator (artifact **2**) |
| `api-diff.cake` | API-diff engine (artifact **4**) |
| `generate-release-notes.py` | release-notes engine (artifact **3**) |
| `api-diff-tools.cake` | shared NuGet-diff comparer + layout helpers, `#load`ed by both `api-diff.cake` and `docs.cake` |
| `versions.json` | the **only** override surface — supersession + comparison baselines, honored identically by the Cake and Python engines |
| `pr-authors.json` | PR-author cache for the release-notes engine |
| `generate-api-diffs.sh` | **Path 1** runner → `cake docs-api-diff-past` |
| `generate-release-notes.sh` | **Path 2** runner → `python generate-release-notes.py` |
| `generate-api-docs.sh` | **Path 3** runner → `cake update-docs` (mdoc under mono) |
| `docker/` | the local reproducibility image + `run.sh` wrapper |

The three `generate-*.sh` scripts are the **single source of truth** for each path:
a developer, the CI workflows, and the Docker wrapper all invoke the same script, so
a command can never drift between local and CI. The shared, general-purpose Cake
machinery (`shared.cake`, `download.cake`) stays under `scripts/infra/shared/`.

### The three generation paths

| Path | Produces | Entry script | Needs |
|------|----------|--------------|-------|
| **1 — API diffs** | artifact 4 | `generate-api-diffs.sh` | dotnet (+ NuGet feed) |
| **2 — Release notes** | artifact 3 | `generate-release-notes.sh` | dotnet, python3, **git history**, **gh** (PR authors) |
| **3 — API reference (mdoc)** | artifact 2 | `generate-api-docs.sh` | dotnet, **mono** (to run `mdoc.exe`) |

`mdoc.exe` is a .NET Framework executable. `docs.cake` invokes it under **mono** when
not on Windows, so Path 3 runs on any host with `mono-complete` installed — a Linux CI
runner or the local Docker image, not only Windows.

---

## The skills

Three Copilot skills drive or assist these paths (`.agents/skills/`):

| Skill | Owns | Phase it performs |
|-------|------|-------------------|
| **`release-notes`** | artifacts 3 + 4 | Runs **Prepare** (the deterministic engines, via `scripts/generate.sh`) then **Polish** (AI rewrites only the prose of the listed release-notes pages). See spec §2.2/§4.4. |
| **`api-docs`** | artifact 2 | Fills the `"To be added."` placeholders mdoc leaves in the XML with real prose, following the .NET doc guidelines. |
| **`skia-analyst`** | analysis | Diffs versions / surfaces what changed and what's missing; used for release announcements and gap analysis, not for writing the committed artifacts. |

The `release-notes` skill stays deliberately **thin**: its `scripts/generate.sh` owns
no commands of its own — it just calls `generate-api-diffs.sh` then
`generate-release-notes.sh` in order. All real logic lives in the engines under
`scripts/infra/docs/`.

---

## CI automation (cross-repo)

```mermaid
flowchart TB
  subgraph this["mono/SkiaSharp"]
    urn["update-release-notes\n(ubuntu-latest)"]
    sync["auto-docs-submodule-sync\n(ubuntu-latest)"]
  end
  subgraph docsrepo["mono/SkiaSharp-API-docs"]
    writer["auto-api-docs-writer\n(Linux + mono)"]
  end
  urn -->|"Prepare engines 3+4,\nthen AI Polish → one PR"| prRel["PR: release notes + api diffs\n(this repo)"]
  writer -->|"mdoc stubs +\nAI placeholder fill → PR"| prDocs["PR: XML docs\n(docs repo)"]
  prDocs -->|"merged"| sync
  sync -->|"bump docs/ submodule → PR"| prBump["PR: update submodule\n(this repo)"]
```

| Workflow | Repo | Runner | Produces | Paths |
|----------|------|--------|----------|-------|
| [`update-release-notes`](../../.github/workflows/update-release-notes.md) | this repo | `ubuntu-latest` (native dotnet + python) | one PR with release notes **and** API diffs | 1 + 2 |
| `auto-api-docs-writer` | [docs repo](https://github.com/mono/SkiaSharp-API-docs/blob/main/.github/workflows/auto-api-docs-writer.md) | Linux + mono | PR with regenerated + AI-filled XML docs | 3 |
| [`auto-docs-submodule-sync`](../../.github/workflows/auto-docs-submodule-sync.yml) | this repo | `ubuntu-latest` | PR bumping the `docs/` submodule pointer | — |

Key points:

| Point | What it means |
|---|---|
| **Paths 1 + 2 = one pipeline, one PR** | `update-release-notes` runs the deterministic Prepare phase (Cake API diffs + Python notes) in its own job, hands off to the AI Polish phase via an artifact, and opens a **single** rolling PR with both. Triggers: pushes to `main` and `release/*`, `v*` tags, and weekly. If nothing changed, **no PR is opened** (spec §2.3). |
| **Path 3 is cross-repo** | The engines live in *this* repo but the XML lives in the *docs* repo, so `auto-api-docs-writer` lives in the docs repo, checks SkiaSharp out to borrow the engines, regenerates the stubs, AI-fills placeholders, and opens its PR there. |
| **`auto-docs-submodule-sync` closes the loop** | Once the docs-repo PR merges, it bumps this repo's `docs/` submodule pointer so the new XML is picked up here. |

---

## Local generation & Docker

Everything CI does can be reproduced locally by calling the **same**
`generate-*.sh` scripts. The only host requirement beyond the .NET SDK is **mono**
for Path 3, and **gh** for Path 2's PR-author lookup.

| Mode | What it is | When to use |
|---|---|---|
| **Native** | Install the deps yourself and call the `generate-*.sh` scripts (or the underlying Cake targets in [writing-docs.md](writing-docs.md)). | When you've installed the deps yourself. |
| **Docker** (`scripts/infra/docs/docker/run.sh`) | A **local-only** convenience image that pre-installs dotnet + mono + python + gh and runs the same scripts against a bind-mounted checkout, so a local run reproduces what CI produces. | A one-command reproducible run without installing the deps yourself. |

> **Docker is for local reproducibility only — CI does not use it.** The CI runners
> install the same dependencies natively on Linux and call the same scripts. The image
> simply mirrors that dependency surface so the two stay identical.

---

## Where to go next

- Generating or editing docs by hand → **[writing-docs.md](writing-docs.md)**
- Changing how release notes or API diffs behave →
  **[release-notes-and-api-diffs.md](release-notes-and-api-diffs.md)** (change the
  spec first, then the code)
- The docs **website** (concept docs, theming, preview) → **[site.md](site.md)**
