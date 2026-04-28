---
name: release-notes
description: >
  Generate or regenerate polished website release notes for SkiaSharp versions.
  Fetches raw release data from GitHub, reads the template, and writes formatted
  markdown pages to documentation/docfx/releases/.

  Use this skill whenever the user asks to:
  - Generate release notes for a version ("write release notes for 3.119.2")
  - Regenerate or refresh release notes ("regenerate 3.119.x release notes")
  - Format raw release data into the website template
  - Update the release notes after publishing a new release

  Triggers: "release notes for X", "regenerate release notes", "format release notes",
  "update website release notes", "write release notes", "refresh release notes".

  This skill is also called by the release-publish skill (Step 7) after annotating
  a GitHub release. You don't need to be asked explicitly — if release-publish is
  running and reaches Step 7, invoke this skill.
---

# Release Notes Skill

Generate polished website release notes for one or more SkiaSharp versions.

## Process

### Step 1 — Determine versions

Ask the user which version(s) to generate, or infer from context:
- A specific version: `3.119.2`
- Multiple versions: `3.119.0, 3.119.1, 3.119.2`
- A range by minor: "all 3.119.x" — list files matching `documentation/docfx/releases/3.119.*.md`
  and also check GitHub for any releases not yet on disk
- If called from release-publish, the version is already known

### Step 2 — Fetch raw data

For each version, fetch the raw GitHub release data:

```bash
python3 .agents/skills/release-notes/scripts/generate-release-notes.py --version {X.Y.Z}
```

This outputs raw markdown to a temp directory. Multiple `--version` flags can be
combined in one call. Read the output file(s) from the temp directory.

### Step 3 — Read the template

Read `documentation/docfx/releases/TEMPLATE.md`. This is a real example of a polished
release notes page. Match its structure, tone, and formatting exactly.

Determine each version's status from its raw data:
- **Stable release**: has a "## Stable Release" section → header uses `Released {date}` + NuGet link + GitHub Release link
- **Preview only**: only preview sections → header uses `Preview only` + NuGet preview link + GitHub Release link

### Step 4 — Write polished pages

For each version, write `documentation/docfx/releases/{X.Y.Z}.md` with polished content.
If the file exists, replace its entire content — this is a full regeneration.

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

8. **Rollup at top** — Aggregate ALL changes across all previews into the main sections.

9. **Previews are minimal** — One sentence + changelog link each, at the bottom.

10. **Links section** — Full Changelog, NuGet Package, API Diff.

### Step 5 — Update TOC and index

After writing all version files:

```bash
python3 .agents/skills/release-notes/scripts/generate-release-notes.py --update-toc
```

## Parallelization

When regenerating multiple versions, process them in parallel — each version is independent.
Fetch all raw data in one script call, then launch one agent per version to write the polished page.
