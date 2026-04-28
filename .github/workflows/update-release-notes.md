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

For branch pushes, extract the branch name directly:

```bash
BRANCH="${GITHUB_REF#refs/heads/}"
```

For tag pushes, derive the release branch from the tag:

```bash
TAG=${GITHUB_REF#refs/tags/}
# Preview tags: v4.147.0-preview.1.1 → release/4.147.0-preview.1 (strip build number)
# Stable tags:  v3.119.2 → release/3.119.2 (keep as-is, just strip 'v')
TAG_NO_V=${TAG#v}
if echo "$TAG_NO_V" | grep -qE "\-preview\.[0-9]+\.[0-9]+$"; then
  # Preview with build number (e.g., 4.147.0-preview.1.1): strip trailing .BUILD
  BRANCH="release/$(echo "$TAG_NO_V" | sed 's|\.[0-9]*$||')"
elif echo "$TAG_NO_V" | grep -q "\-preview\."; then
  # Preview without build number (e.g., 4.147.0-preview.1): use as-is
  BRANCH="release/${TAG_NO_V}"
else
  # Stable: use version directly
  BRANCH="release/${TAG_NO_V}"
fi
```

### Run the script

```bash
python3 .agents/skills/release-notes/scripts/generate-release-notes.py --branch "$BRANCH"
```

The script writes directly to `documentation/docfx/releases/{version}.md` with a YAML
front-matter header and raw PR list, and regenerates TOC/index:

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

Read the version file and `documentation/docfx/releases/TEMPLATE.md`.

## Step 2 — Polish the release notes

The version file now contains raw PR data with a comment that says `REPLACE THIS ENTIRE FILE`.

Use the **release-notes** skill (`.agents/skills/release-notes/SKILL.md`) to rewrite
the file with polished content. The skill has all the formatting rules, template reference,
and content guidelines. Read it and follow Steps 3-4.

The `status` field in the YAML header tells you the release state:
- `unreleased` → use the "Upcoming release / In development" header
- `preview` → use the "Preview only" header with preview NuGet link and GitHub Release link
- `stable` → use the "Released {date}" header with stable NuGet link and GitHub Release link

## Step 3 — Create or update the pull request

Always use `dev/release-notes-{VERSION}` as the branch name when creating the pull request.
This ensures each workflow run updates the **same PR** for a given version instead of
opening duplicates.

The PR targets `main` — release notes always live on main for the docs site.
