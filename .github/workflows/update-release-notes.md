---
description: "Update website release notes when code changes land on main, release branches, or tags are pushed."
on:
  push:
    branches: [main, "release/**"]
    tags: ["v*"]
    paths-ignore:
      - "documentation/docfx/releases/**"
  workflow_dispatch:
  skip-bots: [github-actions, copilot, dependabot]
concurrency:
  group: update-release-notes-${{ github.ref }}
  cancel-in-progress: true
timeout-minutes: 10
permissions:
  contents: read
tools:
  bash: ["python3", "git", "cat", "grep", "sort", "head", "tail", "sed", "awk"]
  edit:
network: {}
safe-outputs:
  create-pull-request:
    title-prefix: "[docs] "
    labels: [documentation]
    draft: false
    allowed-base-branches: [main]
    preserve-branch-name: true
    recreate-ref: true
---

# Update Release Notes

Automatically update website release notes when code changes land on `main`,
`release/*` branches, or when release tags are pushed.

## Step 1 — Determine the branch

```bash
echo "Ref: $GITHUB_REF"
```

Determine the branch name based on the ref type:

```bash
if echo "$GITHUB_REF" | grep -q "^refs/tags/"; then
  # Tag push — derive release branch from tag
  TAG=${GITHUB_REF#refs/tags/}
  TAG_NO_V=${TAG#v}
  if echo "$TAG_NO_V" | grep -qE "\-preview\.[0-9]+\.[0-9]+$"; then
    BRANCH="release/$(echo "$TAG_NO_V" | sed 's|\.[0-9]*$||')"
  else
    BRANCH="release/${TAG_NO_V}"
  fi
else
  # Branch push — extract branch name directly
  BRANCH="${GITHUB_REF#refs/heads/}"
fi
```

## Step 2 — Generate release notes using the skill

**Important:** The script uses `origin/` remote refs for all git history queries.
Always run from a `main` checkout to avoid leaking release-branch-specific files
(like `PREVIEW_LABEL`) into the PR targeting main.

Ensure the working tree is on `main` and all remote refs are fetched:

```bash
git checkout main
git fetch origin --quiet
```

Use the **release-notes** skill (`.agents/skills/release-notes/SKILL.md`) to generate
polished release notes for the branch determined above. Pass the branch name to the skill.

The skill handles everything: running the script, reading the template, writing the
polished file, and regenerating the TOC.

## Step 3 — Create or update the pull request

Use a branch name that includes both the version and the release status to avoid collisions
between release-branch triggers and main-branch triggers:

- For versioned release branches (e.g. `release/4.147.0-preview.3`): use `dev/release-notes-{VERSION}-released`
- For `main` (unreleased content): use `dev/release-notes-{VERSION}-unreleased`

This ensures each workflow run updates the **same PR** for a given version and status
instead of opening duplicates, and prevents a main push from force-pushing over a
release-branch PR (or vice versa).

The PR targets `main` — release notes always live on main for the docs site.
