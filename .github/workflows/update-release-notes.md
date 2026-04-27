---
description: "Regenerate the 'What's Coming Next' release notes section when PRs merge to main."
on:
  push:
    branches: [main]
    paths-ignore:
      - "documentation/docfx/releases/index.md"
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
  bash: ["python3", "gh", "git", "cat"]
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

# Update Unreleased Release Notes

When code merges to main, regenerate the "What's Coming Next" section in the release notes index page with a polished summary of all merged PRs since the last release.

## Step 1 — Capture raw PR data

Run the unreleased notes generator script to fetch merged PRs since the last release tag:

```bash
python3 scripts/generate-unreleased-notes.py
```

This modifies `documentation/docfx/releases/index.md` in place, replacing the placeholder block with a raw list of merged PRs grouped by type. Read the modified file to capture the raw unreleased content — everything between `## What's Coming Next` and `## All Versions`.

Then restore the original file so you have a clean base for writing:

```bash
git checkout -- documentation/docfx/releases/index.md
```

## Step 2 — Read the format template

Read `documentation/docfx/releases/TEMPLATE.md` for the release notes style guidelines. The unreleased section should follow the same conventions but adapted for an "upcoming changes" context (no version number, no NuGet link, no preview sections).

## Step 3 — Polish the content with AI

Using the raw PR list from Step 1 and the template from Step 2, rewrite the unreleased section in polished form:

1. **Highlights paragraph** — 1–3 sentences summarizing the theme of what's coming. What should users be excited about?
2. **Categorized features** — Group changes by what they affect: Engine, API Surface, Platform, Security, Build, etc. Use the template's emoji prefixes (🎨 Core API, 🍎 Apple, 🪟 Windows, 🐧 Linux, 🤖 Android, 🌐 WebAssembly, 🏗️ Build/CI, 📦 General).
3. **Community contributors** — Mark contributions from anyone other than @mattleibow with ❤️ inline.
4. **Omit noise** — Do NOT itemize version bumps, CI-only fixes, doc-only updates, workflow changes, or skill file edits. If there are many of these, mention them as a group: "Plus several CI and documentation improvements."
5. **PR links** — Every item should link to its PR: `([#NNN](url))`.
6. **Breaking changes** — If any PR is labeled `breaking` or has "BREAKING" in the title, list it under a `### ⚠️ Breaking Changes` sub-header at the top.

If there are no merged PRs since the last release, just write: `*No unreleased changes yet.*`

## Step 4 — Write the polished section back into index.md

Use the `edit` tool to replace the content between the placeholder markers in `documentation/docfx/releases/index.md`.

The updated section MUST maintain this exact structure so future runs can find and replace it:

```
<!-- UNRELEASED_PLACEHOLDER -->

{polished content from Step 3}

*Build the site with CI to see merged PRs since the last release.*
```

**CRITICAL:** Both the `<!-- UNRELEASED_PLACEHOLDER -->` HTML comment and the italic `*Build the site with CI to see merged PRs since the last release.*` line MUST remain in the file. They are sentinel markers used by this workflow to locate and replace the unreleased section on every run.

Replace everything between (and including) these two lines:

```
<!-- UNRELEASED_PLACEHOLDER -->
```

through:

```
*Build the site with CI to see merged PRs since the last release.*
```

with the new block containing both markers and your polished content between them.
