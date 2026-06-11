---
name: release-notes
description: >
  Generate or regenerate polished website release notes for SkiaSharp versions.
  Collects raw PR data from git history (no API calls needed), reads the template,
  and writes formatted markdown pages to documentation/docfx/releases/.

  Use this skill whenever the user asks to:
  - Generate release notes for a version ("write release notes for 3.119.2")
  - Regenerate or refresh release notes ("regenerate 3.119.x release notes")
  - Format raw release data into the website template
  - Manually fix or update release notes that the automated workflow got wrong

  Triggers: "release notes for X", "regenerate release notes", "format release notes",
  "update website release notes", "write release notes", "refresh release notes".

  NOTE: Website release notes are normally updated automatically by the
  `update-release-notes` agentic workflow when code lands on main, release branches,
  or tags are pushed. This skill is for manual regeneration or corrections only.
---

# Release Notes Skill

Generate polished website release notes for SkiaSharp versions.

This skill is used both by the `update-release-notes` agentic workflow (automatically
on push to `main`, `release/*` branches, and tags) and manually when regenerating,
correcting, or bulk-processing release notes.

## Process

### Step 1 — Determine versions

Ask the user which version(s) to generate, or infer from context:
- A specific version: `3.119.2`
- A branch: `release/4.147.0-preview.1` or `main`
- Multiple versions: `3.119.0, 3.119.1, 3.119.2`
- A range by minor: "all 3.119.x"

### Step 2 — Run the script

Run the script to collect raw PR data and write it to the version file:

```bash
python3 .agents/skills/release-notes/scripts/generate-release-notes.py --branch release/4.147.0-preview.1
python3 .agents/skills/release-notes/scripts/generate-release-notes.py --branch main
```

This writes raw PR data to `documentation/docfx/releases/{version}.md` or
`documentation/docfx/releases/{version}-unreleased.md` depending on the branch type,
and regenerates TOC/index. All data comes from git history — no API calls or tokens needed.

When TOC/index are regenerated, the script also **prunes stale unreleased pages**: any
`{version}-unreleased.md` whose stable `{version}.md` already exists is deleted, because
that version has shipped and the line has moved on to the next patch (e.g. once
`3.119.4.md` exists, `3.119.4-unreleased.md` is removed in favour of `3.119.5-unreleased.md`).
An unreleased page is still listed in its minor group even when no stable page of that exact
version exists yet (e.g. `3.119.5-unreleased.md` before `3.119.5` ships).

The file starts with an HTML comment block containing both metadata (version, status, branch,
diff range, PR count) AND the raw PR list. Below the comment is a skeleton heading with a
placeholder for polished content. The raw data comment must be preserved in the final file.

**IMPORTANT:** The script prints a summary at the end listing ALL files to polish:

```
========================================
Files to polish:
  - documentation/docfx/releases/4.147.0.md
  - documentation/docfx/releases/3.119.5-unreleased.md
  - documentation/docfx/releases/4.148.0-unreleased.md
========================================
```

You MUST polish **every file** in the "Files to polish" list — not just the first one.
Read each file to get its raw data and metadata, then rewrite it with polished content.

### Step 3 — Read the template

Read `documentation/docfx/releases/TEMPLATE.md`. This is a real example of a polished
release notes page. Match its structure, tone, and formatting exactly.

Determine the version's status from the HTML comment block in the file (`status: unreleased`, `status: preview`, or `status: stable`):
- **Stable**: header uses `Released {date}` + NuGet link + GitHub Release link
- **Preview**: header uses `Preview only` + preview NuGet link + GitHub Release link
- **Unreleased**: header uses `> **Upcoming release** · In development · Not yet available on NuGet`
- **Superseded** (a `superseded:` line is present in the comment block): the version was a
  preview that will never ship as stable. Keep the script-generated
  `> **Preview only** · Superseded by [X.Y.Z](...) · Never released as stable …` header and
  add a short note in Highlights that the work rolled up into the superseding version.
- **Successor** (a `supersedes:` line is present in the comment block): this version rolls
  up one or more skipped preview-only versions. Keep the script-generated
  `> **Supersedes [X.Y.Z](...)** · Rolls up preview-only work …` note and mention in
  Highlights that the skipped preview work is rolled up cumulatively. This is the back-link
  that makes the supersede relationship **two-way** (the superseded page points forward, the
  successor points back).

### Skipped / superseded minors (preview-only versions)

Occasionally a minor ships previews but is **skipped** before going stable — e.g. `4.147`
was previewed but abandoned in favour of `4.148`. This is handled **automatically** — no
configuration is normally required:

1. **Automatic cumulative base.** When choosing the diff base for a new minor (and for
   `main`), the script picks the most recent prior version that actually shipped a **stable
   git tag**, skipping any minor that only had previews. So `4.148`'s notes roll up *all*
   the skipped `4.147` work instead of showing a tiny delta. The same skip applies to
   **point releases**: a stable patch bases on the most recent previous patch that shipped
   stable, so e.g. `3.119.4` rolls up the preview-only `3.119.3` (the `3.119.3` page still
   keeps its own notes — duplication across the two pages is expected and fine).

2. **Automatic supersede label (two-way).** A version that has only preview tags is flagged as
   *superseded* once a **newer version is known**. A newer version is "known" from a later
   `release/*` branch or `v*` tag, **or from `main` itself**: if `main`'s in-development
   `SKIASHARP_VERSION` is newer and the preview-only version's minor line was never branched
   for servicing (no `release/X.Y.x`), `main`'s version supersedes it. So with `main` on
   `4.148.0` and only `release/4.147.0-preview.*` branches (no `release/4.147.x`), `4.147.0`
   is flagged *superseded by 4.148.0* immediately — no `4.148` branch or tag required. The
   page is forced to `preview` status and rendered with a *"Preview only · Superseded by
   4.148.0 · Never released as stable"* header. The label links to the successor's published
   page (`{ver}.md`) when it exists, otherwise to its in-development page
   (`{ver}-unreleased.md`). Until a newer version is known, the version is just a normal preview.

   The relationship is rendered **both ways**: the successor page (computed by the inverse
   `detect_supersedes`) carries a `supersedes:` data-block line and a
   *"Supersedes [X.Y.Z] · Rolls up preview-only work …"* note, so e.g. `4.148.0` points back
   to `4.147.0`, `3.119.4` to `3.119.3`, and `3.119.0` to `3.118.0` — automatically, without
   the AI having to infer it from the diff base.

> Detection is purely tag/branch/`main`-version based, so no configuration file is needed. (If a
> manual override is ever required it can be reintroduced later; for now the automatic
> behaviour is the only path.)

When polishing a superseded page, keep the script-generated *"Preview only · Superseded by …"*
header and add a one-line note in Highlights that the work rolled up into the successor.
When polishing a **successor** page, keep the script-generated *"Supersedes …"* note and add a
one-line note in Highlights that the skipped preview work is rolled up cumulatively.

### Step 4 — Write polished pages

For **every file** listed in the script's "Files to polish" output, write polished release
notes **below the raw data HTML comment block**. 

**CRITICAL: Preserve the raw data comment.** The `<!-- RAW PR DATA ... -->` comment block at
the top of the file must remain intact. Replace everything AFTER it (the skeleton heading and
placeholder) with polished content. When you write the polished content, start with the
`<!-- Generated: ... -->` timestamp line, then the `# Version X.Y.Z` heading, then the rest.

The final file structure should be:

```markdown
<!-- RAW PR DATA — Do not remove this comment block. ... -->

<!-- Generated: YYYY-MM-DDTHH:MM:SSZ by {model-name} -->

# Version X.Y.Z

> **theme** · status · links

## Highlights
...
```

Each file has its own HTML comment block with version, status, branch, and diff range.

**CRITICAL: Each file is independent.** Only use the raw PR data that is IN THAT FILE.
Do NOT combine data from other files. For example, if `3.119.4-unreleased.md` has 4 PRs
and `3.119.4.md` has 101 PRs, the unreleased file should only cover those 4 PRs — it is
NOT a rollup of the version file.

Follow these rules:

1. **Highlights** — 1-3 sentences. What's the story? Lead with the biggest changes.
   Mention community contributors by linked name.

2. **Skia engine** — If a "Bump skia" or "milestone" PR appears in the raw data,
   list it first under an **Engine** category.

3. **Categorize features** — Group by what they affect. Use sub-headers:
   Engine, GPU & Rendering, API Surface, Text & Fonts, Platform, Security, etc.
   Each item: **bold title** — description. ❤️ [@contributor](https://github.com/contributor) ([#NNN](url))

4. **Community contributors** — Anyone not @mattleibow. Mark with ❤️ inline AND
   in a Contributors table. **ALWAYS** link: `[@user](https://github.com/user)`.
   Never write bare `@user` anywhere in the file.

5. **Omit noise** — Skip version bumps, CI-only fixes, doc updates, workflow/skill changes.
   If many, mention as: "Plus several CI and documentation improvements."

6. **Breaking changes** — If any, list under `### ⚠️ Breaking Changes` after Highlights.

7. **PR links** — Every item links to its PR.

8. **Generation timestamp** — Always include `<!-- Generated: YYYY-MM-DDTHH:MM:SSZ by {model-name} -->`
   as the first line AFTER the raw data comment block (before the `# Version` heading).
   Use the current UTC time and your model name.

9. **Rollup at top** — Aggregate ALL changes across all previews into the main sections.

10. **Previews are minimal** — One sentence + changelog link each, at the bottom.

11. **Links section** — Full Changelog, NuGet Package, API Diff.

## Parallelization

When regenerating multiple versions, process them in parallel — each version is independent.
Fetch all raw data in one script call, then launch one agent per version to write the polished page.
