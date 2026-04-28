---
description: "Update website release notes when code changes land on main, release branches, or tags are pushed."
on:
  push:
    branches: [main, "release/**"]
    tags: ["v*"]
    paths-ignore:
      - "documentation/docfx/releases/*.md"
      - ".github/**"
  workflow_dispatch:
  skip-bots: [github-actions, copilot, dependabot]
concurrency:
  group: update-release-notes-${{ github.ref }}
  cancel-in-progress: true
timeout-minutes: 10
permissions:
  contents: read
tools:
  bash: ["python3", "gh", "git", "cat", "grep", "sort", "head", "tail", "sed", "awk"]
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

# Update Release Notes

Automatically update website release notes when code changes land on `main`,
`release/*` branches, or when release tags are pushed.

## Step 1 — Determine branch and fetch raw data

The generate-release-notes.py script handles all the diff logic. Just pass it the
current branch or tag ref.

```bash
echo "Ref: $GITHUB_REF"
```

### Determine the branch name

| `GITHUB_REF` pattern | `--branch` argument |
|----------------------|---------------------|
| `refs/heads/main` | `main` |
| `refs/heads/release/X.Y.Z-preview.N` | `release/X.Y.Z-preview.N` |
| `refs/heads/release/X.Y.Z` | `release/X.Y.Z` |
| `refs/heads/release/X.Y.x` | `release/X.Y.x` |
| `refs/tags/v*` | Find the release branch for this tag (see below) |

For tag pushes, find the release branch the tag points to:

```bash
TAG=${GITHUB_REF#refs/tags/}
# Extract branch-style name: v4.147.0-preview.1.1 → release/4.147.0-preview.1
# Strip the 'v' prefix and trailing build number
BRANCH_VERSION=$(echo "$TAG" | sed 's|^v||; s|\.[0-9]*$||')
BRANCH="release/${BRANCH_VERSION}"
```

### Run the script

```bash
python3 .agents/skills/release-notes/scripts/generate-release-notes.py \
  --branch "$BRANCH" \
  --output /tmp/raw-changes.md
```

The script outputs:
- **To stderr:** Branch name, version, diff range, PR count
- **To file:** Raw markdown list of PRs with metadata

Also ensure the TOC is up to date:

```bash
python3 .agents/skills/release-notes/scripts/generate-release-notes.py --update-toc
```

Read `/tmp/raw-changes.md` and `documentation/docfx/releases/TEMPLATE.md`.

## Step 2 — Determine version and header

Read the version from the script's stderr output (it prints `Version: X.Y.Z`).

### Header format

| Branch type | Header |
|-------------|--------|
| `main` | `> **Upcoming release** · In development · Not yet available on NuGet` |
| `release/X.Y.x` (servicing) | `> **Upcoming release** · In development · Not yet available on NuGet` |
| `release/X.Y.Z-preview.N` | Check if a GitHub release exists for this version. If yes: `> **Preview only** · [NuGet](...) · [GitHub Release](...)`. If no: `> **Upcoming release** · In development` |
| `release/X.Y.Z` (stable) | Check if a GitHub release exists. If yes: `> **Released {date}** · [NuGet](...) · [GitHub Release](...)`. If no: `> **Upcoming release** · In development` |
| Tag push | Always has a release: `> **{theme}** · Released {date} · [NuGet](...) · [GitHub Release](...)` |

## Step 3 — Write polished content

Rewrite the raw PR list into polished release notes following the template.

### Header format by trigger type

| Trigger | Header |
|---------|--------|
| `main` push | `> **Upcoming release** · In development · Not yet available on NuGet` |
| `release/*` (no tag yet) | `> **Upcoming release** · In development · Not yet available on NuGet` |
| `release/*` (tag exists) | `> **Preview only** · [NuGet](https://www.nuget.org/packages/SkiaSharp/{nuget-version}) · [GitHub Release](url)` |
| Tag push (preview) | `> **{theme}** · Released {date} · [NuGet](...) · [GitHub Release](...)` |
| Tag push (stable) | `> **{theme}** · Released {date} · [NuGet](...) · [GitHub Release](...)` |

### Content rules

1. **Highlights** — 1-3 sentences. Lead with the biggest changes. Mention community
   contributors by linked name.

2. **Skia engine** — The version number encodes the Skia milestone (e.g., 4.**147**.0 = m147).
   If a "Bump skia" PR exists, list it first under **Engine**.

3. **Categorize features** — Group by what they affect:
   Engine, GPU & Rendering, API Surface, Text & Fonts, Platform, Security, etc.
   Each item: **bold title** — description. ❤️ [@contributor](https://github.com/contributor) ([#NNN](url))

4. **Community contributors** — Anyone not `@mattleibow`. Mark with ❤️ inline AND
   in a Contributors table. **ALWAYS** link: `[@user](https://github.com/user)`.

5. **Omit noise** — Skip version bumps, CI-only fixes, doc updates, workflow/skill changes.
   If many, mention as: "Plus several CI and documentation improvements."

6. **Breaking changes** — If any, list under `### ⚠️ Breaking Changes` after Highlights.

7. **PR links** — Every item links to its PR.

If there are no user-facing changes, write: `*No user-facing changes yet.*`

## Step 4 — Write the version file

Use the `edit` tool to **replace the entire content** of `documentation/docfx/releases/{VERSION}.md`
with the polished release notes.

The file should follow the template structure exactly — title, blockquote header,
then polished sections (Highlights, Breaking Changes, New Features, etc.).

## Step 5 — Create or update the pull request

Always use `dev/release-notes-{VERSION}` as the branch name when creating the pull request.
This ensures each workflow run updates the **same PR** for a given version instead of
opening duplicates.

The PR targets `main` — release notes always live on main for the docs site.
