---
description: "Regenerate the 'What's Coming Next' release notes section when PRs merge to main."
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

When code merges to main, regenerate the upcoming version's release notes page with a polished summary of all merged PRs since the last release.

## Step 1 — Determine the upcoming version and ensure the file exists

Read the `SKIASHARP_VERSION` from `scripts/azure-templates-variables.yml`:

```bash
grep 'SKIASHARP_VERSION:' scripts/azure-templates-variables.yml
```

This gives you the upcoming version number (e.g., `4.133.0`). The version file lives at `documentation/docfx/releases/{version}.md`.

If the file does **not** exist, create it and update the TOC:

```bash
python3 scripts/generate-release-notes.py --update-toc
```

This creates the version file with fence markers and adds it to `TOC.yml` and `index.md`.

Then read the version file to confirm it has `<!-- UNRELEASED_BEGIN -->` and `<!-- UNRELEASED_END -->` markers.

## Step 2 — Fetch raw PR data

Run the unreleased notes script to fetch merged PRs since the last release tag and save to a temp file:

```bash
python3 scripts/generate-unreleased-notes.py --output /tmp/unreleased-raw.md
```

Then read the output file `/tmp/unreleased-raw.md` to capture the raw content.

## Step 3 — Read the format template

Read `documentation/docfx/releases/TEMPLATE.md` for the release notes style guidelines. The upcoming version page should follow the same conventions but adapted for an in-development release (no stable release date, no NuGet link yet).

## Step 4 — Polish the content with AI

Using the raw PR list from Step 2 and the template from Step 3, rewrite the unreleased section in polished form:

1. **Highlights paragraph** — 1–3 sentences summarizing the theme of what's coming. What should users be excited about?
2. **Skia engine version** — The version number encodes the Skia milestone (e.g., **4.133.0** means Skia **m133**). Always mention the Skia engine version in the Highlights and as the first feature under an **Engine** category. If a "Bump skia" PR is in the raw list, link to it. Otherwise, search for a merged "Bump skia to milestone {N}" PR using `gh pr list --state merged --search "bump skia milestone {N}"` and link to that.
3. **Categorized features** — Group changes by what they affect: Engine, API Surface, Platform, Security, Build, etc. Use the template's emoji prefixes (🎨 Core API, 🍎 Apple, 🪟 Windows, 🐧 Linux, 🤖 Android, 🌐 WebAssembly, 🏗️ Build/CI, 📦 General).
4. **Community contributors** — Mark contributions from anyone other than @mattleibow with ❤️ inline. **Always link usernames:** `[@user](https://github.com/user)` — never use bare `@user` anywhere in the file (prose, bullet points, or tables).
5. **Omit noise** — Do NOT itemize version bumps, CI-only fixes, doc-only updates, workflow changes, or skill file edits. If there are many of these, mention them as a group: "Plus several CI and documentation improvements."
6. **PR links** — Every item should link to its PR: `([#NNN](url))`.
7. **Breaking changes** — If any PR is labeled `breaking` or has "BREAKING" in the title, list it under a `### ⚠️ Breaking Changes` sub-header at the top.

If there are no merged PRs since the last release, just write: `*No changes yet.*`

## Step 5 — Write the polished section into the version file

Use the `edit` tool to replace the content between the fence markers in `documentation/docfx/releases/{version}.md`.

The updated section MUST maintain this exact structure so future runs can find and replace it:

```
<!-- UNRELEASED_BEGIN -->

{polished content from Step 4}

<!-- UNRELEASED_END -->
```

**CRITICAL:** Both the `<!-- UNRELEASED_BEGIN -->` and `<!-- UNRELEASED_END -->` HTML comments MUST remain in the file. They are sentinel markers used by this workflow to locate and replace the content on every run.

Replace everything between (and including) these two lines with the new block containing both markers and your polished content between them.
