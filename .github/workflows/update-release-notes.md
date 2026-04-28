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

## Step 1 — Determine context

Figure out what triggered this run and what we need to update.

```bash
echo "Ref: $GITHUB_REF"
echo "Event: $GITHUB_EVENT_NAME"
```

### Identify the trigger type

| `GITHUB_REF` pattern | Type | Example |
|----------------------|------|---------|
| `refs/heads/main` | Main branch push | Upcoming unreleased version |
| `refs/heads/release/X.Y.Z-preview.N` | Preview release branch | `release/4.147.0-preview.1` |
| `refs/heads/release/X.Y.Z` | Stable release branch | `release/3.119.2` |
| `refs/heads/release/X.Y.x` | Servicing branch | `release/3.119.x` |
| `refs/tags/v*` | Tag push (release published) | `v4.147.0-preview.1.1` |

Determine the trigger type from `GITHUB_REF` and store it for later steps.

## Step 2 — Determine diff range and version

Based on the trigger type, compute the diff range and target version file.

### For `main` branch pushes

```bash
# Get upcoming version
VERSION=$(grep 'SKIASHARP_VERSION:' scripts/azure-templates-variables.yml | awk '{print $2}')

# Find latest release tag reachable from HEAD
LATEST_TAG=$(git describe --tags --abbrev=0 --match "v*" HEAD 2>/dev/null || echo "")

# If no tag, use the initial commit
if [ -z "$LATEST_TAG" ]; then
  DIFF_FROM=$(git rev-list --max-parents=0 HEAD)
else
  DIFF_FROM="$LATEST_TAG"
fi
```

- **Version file:** `documentation/docfx/releases/{VERSION}.md`
- **Diff range:** `{LATEST_TAG}..HEAD`
- **Header:** `> **Upcoming release** · In development · Not yet available on NuGet`

### For `release/X.Y.Z-preview.N` branch pushes

```bash
# Extract base version: release/4.147.0-preview.1 → 4.147.0
BRANCH=${GITHUB_REF#refs/heads/}
VERSION=$(echo "$BRANCH" | sed 's|release/||; s|-preview\..*||')

# Find previous tag for this version, or fall back to any previous tag
LATEST_TAG=$(git tag -l "v${VERSION}*" --sort=-v:refname | head -1)
if [ -z "$LATEST_TAG" ]; then
  LATEST_TAG=$(git describe --tags --abbrev=0 --match "v*" HEAD 2>/dev/null || echo "")
fi
```

- **Version file:** `documentation/docfx/releases/{VERSION}.md`
- **Diff range:** `{LATEST_TAG}..HEAD`
- **Header:** `> **Preview only** · [NuGet](...) · [GitHub Release](...)`
  (if a tag exists for this preview, otherwise `> **In development**`)

### For `release/X.Y.x` servicing branch pushes

```bash
# Extract minor version: release/3.119.x → 3.119
BRANCH=${GITHUB_REF#refs/heads/}
MINOR=$(echo "$BRANCH" | sed 's|release/||; s|\.x$||')

# Find latest tag for this minor
LATEST_TAG=$(git tag -l "v${MINOR}.*" --sort=-v:refname | head -1)

# Determine next patch: v3.119.4-preview.1.1 → 3.119.5
LATEST_PATCH=$(echo "$LATEST_TAG" | sed 's|^v||; s|-.*||' | awk -F. '{print $3}')
NEXT_PATCH=$((LATEST_PATCH + 1))
VERSION="${MINOR}.${NEXT_PATCH}"
```

- **Version file:** `documentation/docfx/releases/{VERSION}.md`
- **Diff range:** `{LATEST_TAG}..HEAD`
- **Header:** `> **Upcoming release** · In development · Not yet available on NuGet`

### For `release/X.Y.Z` stable branch pushes

```bash
# Extract version: release/3.119.2 → 3.119.2
BRANCH=${GITHUB_REF#refs/heads/}
VERSION=$(echo "$BRANCH" | sed 's|release/||')

# Find latest preview tag for this version
LATEST_TAG=$(git tag -l "v${VERSION}-preview.*" --sort=-v:refname | head -1)
if [ -z "$LATEST_TAG" ]; then
  LATEST_TAG=$(git describe --tags --abbrev=0 --match "v*" HEAD 2>/dev/null || echo "")
fi
```

- **Version file:** `documentation/docfx/releases/{VERSION}.md`
- **Diff range:** `{LATEST_TAG}..HEAD`

### For tag pushes (`v*`)

```bash
TAG=${GITHUB_REF#refs/tags/}

# Extract base version: v4.147.0-preview.1.1 → 4.147.0
VERSION=$(echo "$TAG" | sed 's|^v||; s|-preview\..*||; s|-stable\..*||')

# Extract minor version: 4.147.0 → 4.147
MINOR=$(echo "$VERSION" | awk -F. '{print $1"."$2}')

# Find previous tag scoped to same minor version (not globally!)
# This ensures v3.119.5 diffs from v3.119.4*, not from v4.147.0*
PREVIOUS_TAG=$(git tag -l "v${MINOR}.*" --sort=-v:refname | grep -v "^${TAG}$" | head -1)

# Fall back to commit ancestry if no same-minor tag exists (first release of a new major)
if [ -z "$PREVIOUS_TAG" ]; then
  PREVIOUS_TAG=$(git describe --tags --abbrev=0 --match "v*" "${TAG}^" 2>/dev/null || echo "")
fi
```

- **Version file:** `documentation/docfx/releases/{VERSION}.md`
- **Diff range:** `{PREVIOUS_TAG}..{TAG}`
- **Header:** includes Released date, NuGet link, GitHub Release link

## Step 3 — Fetch raw change data

Use the generate script to get raw PR data, or fall back to git log:

```bash
python3 .agents/skills/release-notes/scripts/generate-release-notes.py --unreleased --output /tmp/raw-changes.md
```

For tag pushes (released versions), fetch the GitHub release body instead:

```bash
gh release view "$TAG" --json body,publishedAt,tagName -q '.' > /tmp/release-data.json
```

Also ensure the TOC is up to date:

```bash
python3 .agents/skills/release-notes/scripts/generate-release-notes.py --update-toc
```

Read the raw data and `documentation/docfx/releases/TEMPLATE.md` for formatting reference.

## Step 4 — Write polished content

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

## Step 5 — Write the version file

Use the `edit` tool to **replace the entire content** of `documentation/docfx/releases/{VERSION}.md`
with the polished release notes.

The file should follow the template structure exactly — title, blockquote header,
then polished sections (Highlights, Breaking Changes, New Features, etc.).

## Step 6 — Create or update the pull request

Always use `dev/release-notes-{VERSION}` as the branch name when creating the pull request.
This ensures each workflow run updates the **same PR** for a given version instead of
opening duplicates.

The PR targets `main` — release notes always live on main for the docs site.
