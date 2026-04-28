---
description: "Update the upcoming version's release notes when PRs merge to main."
on:
  push:
    branches: [main]
    paths-ignore:
      - "documentation/docfx/releases/*.md"
      - ".github/**"
  workflow_dispatch:
  skip-bots: [github-actions, copilot, dependabot]
concurrency:
  group: update-release-notes
  cancel-in-progress: true
timeout-minutes: 10
permissions:
  contents: read
tools:
  bash: ["python3", "gh", "git", "cat", "grep"]
  edit:
network:
  allowed:
    - defaults
safe-outputs:
  create-pull-request:
    title-prefix: "[docs] "
    labels: [documentation]
    draft: false
---

# Update Upcoming Release Notes

When code merges to main, update the upcoming version's release notes page with a
polished summary of all changes since the last release.

## Step 1 — Set up

Determine the upcoming version and ensure the file exists:

```bash
grep 'SKIASHARP_VERSION:' scripts/azure-templates-variables.yml
```

Extract the version number (e.g., `4.133.0`). Then ensure the version file and TOC exist:

```bash
python3 .agents/skills/release-notes/scripts/generate-release-notes.py --update-toc
```

This creates `documentation/docfx/releases/{version}.md` if missing and regenerates
`TOC.yml` and `index.md`. Read the version file to see its current content.

## Step 2 — Get raw change data

Fetch the list of changes since the last release:

```bash
python3 .agents/skills/release-notes/scripts/generate-release-notes.py --unreleased --output /tmp/unreleased-raw.md
```

This uses git commit ancestry (not dates) to find all PRs on main that are not in the last
release tag. Read `/tmp/unreleased-raw.md` to capture the raw content.

## Step 3 — Read the template

Read `documentation/docfx/releases/TEMPLATE.md`. This is a real example of a polished release
notes page. Use it as the style reference — match its structure, tone, and formatting.

The upcoming version adapts the template for unreleased status:
- Use `> **Upcoming release** · In development · Not yet available on NuGet` as the header
- Omit the Links section (no NuGet, no changelog, no API diff yet)
- Omit Preview sections (no tagged previews yet)
- Keep everything else: Highlights, Breaking Changes, New Features, Security, Bug Fixes,
  Platform Support, Community Contributors

## Step 4 — Write polished content

Rewrite the raw PR list into polished release notes following the template:

1. **Highlights** — 1-3 sentences. What's the story of this version? Lead with the biggest
   changes. Mention community contributors by linked name.

2. **Skia engine** — The version number encodes the Skia milestone (e.g., 4.**133**.0 = Skia m133).
   Search for the merged bump PR: `gh pr list --repo mono/SkiaSharp --state merged --search "bump skia milestone {N}" --json number,title --limit 1`
   If found, list it first under an **Engine** category. If the raw data already contains
   a "Bump skia" PR, use that directly.

3. **Categorize features** — Group by what they affect. Use sub-headers like:
   Engine, GPU & Rendering, API Surface, Text & Fonts, Platform, Security, etc.
   Each item: **bold title** — description. ❤️ [@contributor](https://github.com/contributor) ([#NNN](url))

4. **Community contributors** — Anyone not `@mattleibow`. Mark with ❤️ inline AND list
   in a Contributors table. **ALWAYS** link usernames: `[@user](https://github.com/user)`.
   Never write bare `@user` anywhere.

5. **Omit noise** — Skip version bumps, CI-only fixes, doc updates, workflow changes,
   skill file edits. If many, mention as: "Plus several CI and documentation improvements."

6. **Breaking changes** — If any PR has a `breaking` label or "BREAKING" in the title,
   list under `### ⚠️ Breaking Changes` right after Highlights.

7. **PR links** — Every item links to its PR: `([#NNN](url))`.

If there are no user-facing changes, write: `*No user-facing changes yet.*`

## Step 5 — Write the version file

Use the `edit` tool to **replace the entire content** of `documentation/docfx/releases/{version}.md`
with the polished release notes from Step 4.

The file should follow the template structure exactly — title, blockquote header
(`> **Upcoming release** · In development · Not yet available on NuGet`),
then the polished sections (Highlights, Breaking Changes, New Features, etc.).

No fence markers or placeholders needed — the workflow overwrites the whole file each run.

## Step 6 — Create or update the pull request

Always use `dev/upcoming-release-notes` as the branch name when creating the pull request.
This ensures each workflow run updates the **same PR** instead of opening a new one.
