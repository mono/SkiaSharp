---
description: "Update website release notes when code changes land on main, release branches, or tags are pushed."
on:
  push:
    branches: [main, "release/**"]
    tags: ["v*"]
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
# Preview tags: v4.147.0-preview.1.1 → release/4.147.0-preview.1 (strip build number)
# Stable tags:  v3.119.2 → release/3.119.2 (keep as-is, just strip 'v')
TAG_NO_V=${TAG#v}
if echo "$TAG_NO_V" | grep -q "\-preview\."; then
  # Preview: strip trailing .BUILD_NUMBER
  BRANCH_VERSION=$(echo "$TAG_NO_V" | sed 's|\.[0-9]*$||')
else
  # Stable: use version directly (no build number to strip)
  BRANCH_VERSION="$TAG_NO_V"
fi
BRANCH="release/${BRANCH_VERSION}"
```

### Run the script

```bash
python3 .agents/skills/release-notes/scripts/generate-release-notes.py \
  --branch "$BRANCH" \
  --output /tmp/raw-changes.md
```

The script writes a single file with a YAML front-matter header and PR list:

```markdown
---
branch: release/4.147.0-preview.1
version: 4.147.0
status: unreleased
diff: release/3.119.4-preview.1..release/4.147.0-preview.1
pr_count: 45
---

- PR title by @author in https://...
```

The script also regenerates `TOC.yml` and `index.md` automatically.

Read `/tmp/raw-changes.md` and `documentation/docfx/releases/TEMPLATE.md`.

## Step 2 — Determine version and header

Read the YAML front-matter from `/tmp/raw-changes.md` to get `version` and `status`.

### Header format

| `status` | Header |
|----------|--------|
| `unreleased` | `> **Upcoming release** · In development · Not yet available on NuGet` |
| `released` | `> **{theme}** · Released {date} · [NuGet](...) · [GitHub Release](...)` — fetch date from `gh release view` |
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
