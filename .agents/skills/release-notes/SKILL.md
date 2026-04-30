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

The file starts with an HTML comment header containing metadata (version, status, branch,
diff range, PR count), followed by the raw PR list. Read the header to understand context.

**IMPORTANT:** The script prints a summary at the end listing ALL files to polish:

```
========================================
Files to polish:
  - documentation/docfx/releases/4.147.0.md
  - documentation/docfx/releases/4.147.0-unreleased.md
  - documentation/docfx/releases/3.119.4-unreleased.md
========================================
```

You MUST polish **every file** in the "Files to polish" list — not just the first one.
Read each file to get its raw data and metadata, then rewrite it with polished content.

### Step 3 — Read the template

Read `documentation/docfx/releases/TEMPLATE.md`. This is a real example of a polished
release notes page. Match its structure, tone, and formatting exactly.

Determine the version's status from the HTML comment header in the file (`status: unreleased`, `status: preview`, or `status: stable`):
- **Stable**: header uses `Released {date}` + NuGet link + GitHub Release link
- **Preview**: header uses `Preview only` + preview NuGet link + GitHub Release link
- **Unreleased**: header uses `> **Upcoming release** · In development · Not yet available on NuGet`

### Step 4 — Write polished pages

For **every file** listed in the script's "Files to polish" output, replace the raw content
with polished release notes. If the file exists, replace its entire content — this is a full
regeneration. Each file has its own HTML comment header with version, status, branch, and diff range.

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
   as the very first line of the file. Use the current UTC time and your model name.

9. **Rollup at top** — Aggregate ALL changes across all previews into the main sections.

10. **Previews are minimal** — One sentence + changelog link each, at the bottom.

11. **Links section** — Full Changelog, NuGet Package, API Diff.

## Parallelization

When regenerating multiple versions, process them in parallel — each version is independent.
Fetch all raw data in one script call, then launch one agent per version to write the polished page.
