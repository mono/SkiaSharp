---
name: changelog
description: >
  Generate or regenerate SkiaSharp API changelogs (per-assembly API diffs).
  Wraps the Cake targets that use Mono.ApiTools.NuGetDiff to compare published
  NuGet packages and write markdown diffs to changelogs/{PackageId}/{version}/.

  Use this skill whenever the user asks to:
  - Generate or regenerate API changelogs ("regenerate the API changelogs",
    "update changelogs for 4.148")
  - Diff two SkiaSharp versions' public API surface
  - Preview what the API changelog for an unreleased branch/build would look like
  - Investigate why a changelog comparison picked a particular baseline

  Triggers: "API changelog", "api diff", "regenerate changelogs", "what API
  changed between X and Y", "preview API changes for <branch>".

  NOTE: Changelogs are normally regenerated automatically by the `api-diff`
  workflow on push to main / release branches / tags. This skill is for manual
  regeneration, previews, or corrections.
---

# Changelog (API Diff) Skill

Generate per-assembly API changelogs for SkiaSharp by diffing published NuGet
packages with `Mono.ApiTools.NuGetDiff`.

This skill is used both by the `api-diff` workflow (automatically on push to
`main`, `release/*` branches, and `v*` tags) and manually when regenerating,
previewing, or correcting changelogs.

## How it works

```
scripts/versions.json (shared override config)
        │
        ▼
generate-changelogs.sh  ──►  dotnet cake docs-api-diff[-past]
        │                          │
        │                          ▼
        │                    Mono.ApiTools.NuGetDiff
        ▼                          │
   past | preview                  ▼
                          changelogs/{PackageId}/{version}/{Assembly}.md
```

- **Past mode** regenerates every historical changelog from packages already on
  NuGet.org. Each version is diffed against its predecessor.
- **Preview mode** diffs a not-yet-published build (CI packages for a branch/tag/
  SHA) against its NuGet.org baseline, to preview an unreleased version's diff.

### Baseline selection (the important part)

For each version, the baseline it is compared against is chosen as follows:

1. If `scripts/versions.json` has a `compare_to` for the version, use it.
2. Otherwise walk backwards through the sorted version list and pick the first
   predecessor that is **not** superseded.
3. The very first version has no predecessor → it gets a full API listing.

**Superseded versions are still generated.** A superseded version (e.g.
`4.147.0`, which was previewed then abandoned in favour of `4.148.0`) keeps its
own changelog. The supersede marker only removes it from the pool of *baselines*,
so when `4.148.0` looks back it skips all `4.147.*` and lands on `3.119.4`.

This is the same `scripts/versions.json` the release-notes skill reads, so the
two systems stay consistent. See that file's `$comment` fields for the meaning
of `status`, `superseded_by`, `supersedes`, and `compare_to`.

## Process

### Step 1 — Choose a mode

| Goal | Mode |
| --- | --- |
| Refresh all changelogs from NuGet.org | `past` (default) |
| See an unreleased branch/build's diff | `preview <ref>` |

### Step 2 — Run the script

All logic lives in the script so it runs identically locally and in CI. From the
repository root:

```bash
# Regenerate ALL historical changelogs from NuGet.org (includes prereleases):
.agents/skills/changelog/scripts/generate-changelogs.sh past

# Exclude prereleases:
.agents/skills/changelog/scripts/generate-changelogs.sh past --prerelease false

# Preview the API diff for an unreleased branch / tag / SHA:
.agents/skills/changelog/scripts/generate-changelogs.sh preview main
.agents/skills/changelog/scripts/generate-changelogs.sh preview release/4.150.0-preview.1
```

The script runs `dotnet tool restore` for you. You only need a .NET SDK matching
`global.json` and network access to NuGet.org (plus the AzDO CI feed for preview).

You can also call the Cake targets directly if you prefer:

```bash
dotnet cake --target=docs-api-diff-past --nugetDiffPrerelease=true
```

### Step 3 — Review the output

- **Past mode** writes/updates `changelogs/{PackageId}/{version}/{Assembly}.md`.
  Review with `git diff changelogs/`.
- **Preview mode** writes to `output/api-diff/` (and `changelogs/`) for
  inspection — it is not meant to be committed.

## Configuration: `scripts/versions.json`

Override-only config shared with the release-notes skill. You normally only edit
it a few times a year, when a version is superseded or needs a non-default
baseline. Entry shape:

```json
{
  "version": "4.148.0",
  "compare_to": "3.119.4",
  "supersedes": ["4.147.0"]
}
```

- `status: "superseded"` + `superseded_by` — the version still gets a changelog
  but is never used as a baseline for other versions.
- `compare_to` — force a specific baseline (matched on `major.minor.patch`
  prefix, so `4.148.0` resolves to e.g. `4.148.0-rc.1.2`). Takes priority over
  the automatic walk-back.

## Relationship to the workflow

`.github/workflows/api-diff.yml` is intentionally thin: it checks out the repo,
calls this script, then commits the result to the single `bot/api-diff` PR
branch (force-pushed each run; no PR if nothing changed). Keep generation logic
in the script, not the workflow, so local and CI runs stay identical.
