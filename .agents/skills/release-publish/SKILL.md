---
name: release-publish
description: >
  Publish SkiaSharp packages and finalize the release.
  
  Use when user says "publish X", "finalize X", "tag X", or "finish release X".
  
  This is the FINAL step - after release-testing passes.
  Publishes to NuGet.org, creates tag, GitHub release, and closes milestone.
  
  Triggers: "publish the release", "push to nuget", "create github release",
  "tag the release", "close the milestone", "annotate release notes",
  "testing passed what's next", "finalize 3.119.2", "release is ready".
---

# Release Publish Skill

Publish packages to NuGet.org and finalize releases.

⚠️ **NO UNDO:** This is **Step 4 of 4** in the release pipeline (final step). See [releasing.md](../../../documentation/dev/releasing.md) for full workflow.

**Pipeline:** [Step 1: release-branch](../release-branch/SKILL.md) → [Step 2: release-status](../release-status/SKILL.md) → [Step 3: release-testing](../release-testing/SKILL.md) → **Step 4 (this skill)**

## ⚠️ Branch Protection (COMPLIANCE REQUIRED)

> **🛑 NEVER commit directly to `main` or `skiasharp` branches. This is a policy violation.**

| Repository | Protected Branches | Required Action |
|------------|-------------------|-----------------|
| SkiaSharp (parent) | `main` | Tags/releases created from release branches, never modify main directly |
| externals/skia (submodule) | `main`, `skiasharp` | Never modify directly |

**Publishing creates tags on existing release branches — it does NOT modify protected branches.**

---

## Workflow Overview

```
┌────────────────────────────────────────────────────────────────────┐
│  1. Confirm Versions     → Verify packages exist on preview feed   │
│  2. Publish to NuGet.org → Trigger Azure pipeline (manual)         │
│  3. Verify Published     → Poll NuGet.org until indexed            │
│  4. Tag Release          → Push git tag (ask_user first!)          │
│  5. Refresh Web Notes    → Dispatch docs workflow (tag→stable flip)│
│  6. Create GitHub Release→ Generate notes, set prerelease flag     │
│  7. Customer Teaser      → Extract key bits from the generated log │
│  8. Close Milestone      → Stable releases only                    │
└────────────────────────────────────────────────────────────────────┘
```

**Preview vs Stable differences:**
| Step | Preview | Stable |
|------|---------|--------|
| 1. NuGet version | `X.Y.Z-preview.N.{build}` | `X.Y.Z` (no build number) |
| 2. Pipeline checkbox | "Push Preview" | "Push Stable" |
| 4. Tag format | `vX.Y.Z-preview.N.{build}` | `vX.Y.Z` |
| 5. Website notes refresh | Dispatch (usually a no-op) | Dispatch — flips page to **stable** |
| 6. GitHub Release | `--prerelease` flag | No flag, attach samples |
| 7. Customer teaser | Breaking + What's New + Fixes (usually short) | + Dependency Updates + contributors |
| 8. Milestone | Skip | Close milestone |

---

## Step 1: Confirm Versions

### ⚠️ Semver Version Ordering

When identifying which version to publish, use **semver ordering**, not alphabetical:
- `3.119.2` (bare) is NEWER than `3.119.2-preview.3` — it's the stable/final release
- Always verify you are publishing from the correct branch
- If both `release/3.119.2` and `release/3.119.2-preview.3` exist, the bare version is the latest

**Prerequisite:** release-testing must have passed. Versions should be known from testing.

The user should provide:
- **Preview:** SkiaSharp version with build number (e.g., `3.119.2-preview.2.3`)
- **Stable:** SkiaSharp base version only (e.g., `3.119.2`) — no build number

⚠️ **Stable versions never include a build number.** The build number only appears in the prerelease component (e.g., `3.119.2-preview.2.3`) or in the internal stable tag (e.g., `3.119.2-stable.3`). It is never appended to the base version directly.

If not provided, ask for them using `ask_user`.

**Quick verification** — confirm packages exist on preview feed:
```bash
# Preview: search for the exact NuGet version
dotnet package search SkiaSharp --source "https://aka.ms/skiasharp-eap/index.json" --exact-match --prerelease --format json | jq -r '.searchResult[].packages[].version' | grep "{expected-version}"

# Stable: search for internal stable builds (NuGet version is just the base, e.g., 3.119.2)
dotnet package search SkiaSharp --source "https://aka.ms/skiasharp-eap/index.json" --exact-match --prerelease --format json | jq -r '.searchResult[].packages[].version' | grep "^{base}-stable\."
```

If missing, STOP and ask user to verify testing was completed.

---

## Step 2: Publish to NuGet.org

Trigger the [publish pipeline](https://dev.azure.com/devdiv/DevDiv/_build?definitionId=25298) to push packages to NuGet.org.

### Verifying Source Build Before Publishing

Before triggering the publish pipeline, confirm builds completed using the **release-status** skill:

```bash
python3 .agents/skills/release-status/scripts/pipeline-status.py release/{version}
```

The `SkiaSharp` pipeline (ID 10789) must show ✅ — this is the pipeline that produced the
packages on the internal feed. See [release-status](../release-status/SKILL.md) for details.

### Pipeline Steps

1. Open the [NuGet.org publish pipeline](https://dev.azure.com/devdiv/DevDiv/_build?definitionId=25298)
2. Click **"Run pipeline"**
3. Select **"SkiaSharp"** from the radio buttons
4. Check **"Confirm push to NuGet.org"** checkbox
5. **For stable releases ONLY:** Check **"Push stable packages"** checkbox
   - ⚠️ Do NOT check this for preview releases
6. Click **"Next: Resources"**
7. In **"Pipeline artifacts"**, click the **SkiaSharp** artifact selector
8. From the **branch dropdown**, select `release/{version}` (the release branch)
9. From the **pipeline runs list**, select the correct build by checking the build number
10. Click **"Use selected run"**
11. Click **"Run"**

### Verification During Pipeline Run

⚠️ **Before approving the push step, verify BOTH:**

1. **Run name** — The pipeline run will rename itself to the version being released. Confirm this matches your expected version.
2. **Push type** — The publish step will indicate **"Push Preview"** or **"Push Stable"**. Verify this matches your release type:
   - Preview release → should show "Push Preview"
   - Stable release → should show "Push Stable"

**Only approve the push step when both are correct.** Wait for pipeline completion (typically 5-10 minutes after approval).

Ask user to follow these steps and wait for completion.

---

## Step 3: Verify Packages Published

**Use curl to verify** (more reliable than `dotnet package search` which has version limits):

```bash
# Check if packages exist - HTTP 200 = success
curl -s -o /dev/null -w "%{http_code}" "https://api.nuget.org/v3-flatcontainer/skiasharp/{version}/skiasharp.nuspec"
curl -s -o /dev/null -w "%{http_code}" "https://api.nuget.org/v3-flatcontainer/harfbuzzsharp/{version}/harfbuzzsharp.nuspec"
```

**If packages not yet indexed**, poll until available (NuGet.org can take 5-15 minutes):

```bash
# Poll every 30 seconds, max 10 minutes
for i in 1 2 3 4 5 6 7 8 9 10 11 12 13 14 15 16 17 18 19 20; do
  skia=$(curl -s -o /dev/null -w "%{http_code}" "https://api.nuget.org/v3-flatcontainer/skiasharp/{version}/skiasharp.nuspec")
  hb=$(curl -s -o /dev/null -w "%{http_code}" "https://api.nuget.org/v3-flatcontainer/harfbuzzsharp/{version}/harfbuzzsharp.nuspec")
  echo "$(date +%H:%M:%S) - SkiaSharp: $skia, HarfBuzzSharp: $hb"
  if [ "$skia" = "200" ] && [ "$hb" = "200" ]; then
    echo "✅ Both packages available on NuGet.org!"
    break
  fi
  sleep 30
done
```

> **Note:** Use explicit list `1 2 3...` instead of `{1..20}` brace expansion for better compatibility with async shell execution.

Or manually check: `https://www.nuget.org/packages/SkiaSharp/{version}`

---

## Step 4: Tag Release

Tag formats:
- **Preview:** `vX.Y.Z-preview.N.{build}` (e.g., `v3.119.2-preview.2.5`)
- **Stable:** `vX.Y.Z` (e.g., `v3.119.2`)

```bash
git fetch origin
git checkout release/{branch-version}
git pull
git tag {tag}
```

**Confirm with `ask_user`** before pushing tag (cannot be undone):
```bash
git push origin {tag}
```

---

## Step 5: Refresh Website Release Notes & API Diffs

The website release-notes and API-diff pages (`documentation/docfx/releases/`) are
produced by the **Update Release Notes & API Diffs** workflow. That workflow runs
**daily and on pushes to `main`** — it deliberately **no longer triggers on `v*`
tags** — so after pushing the tag in Step 4, **dispatch it manually** to refresh the
site immediately instead of waiting up to ~24h for the next daily run.

This matters most for **stable** releases: a clean `vX.Y.Z` tag is what flips that
version's page from "preview / unreleased" to **stable**. The generator detects this
via `_version_has_stable_tag`, which reads `git tag` — so the page cannot flip until
the workflow runs again with the tag visible. For previews it is usually a no-op (the
release-branch push already refreshed the pages), and the workflow's no-op gate opens
no PR when nothing changed, so this step is always safe to run.

```bash
# Always dispatch from main: it owns the unified generation (latest scripts +
# versions.json) and walks every release/* ref and all v* tags itself — including
# the tag you just pushed. Do NOT dispatch from the release branch.
gh workflow run "Update Release Notes & API Diffs" --repo mono/SkiaSharp --ref main

# Optional: follow the run to completion.
gh run watch "$(gh run list --workflow 'Update Release Notes & API Diffs' --repo mono/SkiaSharp --branch main --limit 1 --json databaseId --jq '.[0].databaseId')" --repo mono/SkiaSharp
```

If anything changed, the workflow opens (or updates) the rolling `[docs]`
**`bot/release-notes`** PR with the refreshed pages — review and merge it like any
docs PR. If nothing changed, no PR is opened.

> ⚠️ These **website** release notes are separate from the **GitHub Release** notes
> created in Step 6. This step updates the docfx site; Step 6 publishes the GitHub
> Release. Do both.

---

## Step 6: Create GitHub Release

### Title Format

| Release Type | Title Format | Example |
|--------------|--------------|---------|
| Preview | `Version X.Y.Z (Preview N)` | `Version 3.119.2 (Preview 2)` |
| Stable | `Version X.Y.Z` | `Version 3.119.2` |
| Hotfix Preview | `Version X.Y.Z.F (Preview N)` | `Version 3.119.2.1 (Preview 1)` |
| Hotfix Stable | `Version X.Y.Z.F` | `Version 3.119.2.1` |

### Finding the Previous Release Tag

**Always use `--notes-start-tag` to explicitly specify the previous release.** The auto-selection may pick the wrong tag.

```bash
# List recent tags to find the previous release
git tag -l "v3.119*" --sort=-v:refname | head -10
```

| Current Release | Previous Tag (--notes-start-tag) |
|-----------------|----------------------------------|
| `v3.119.2-preview.2.3` | `v3.119.2-preview.1.2` (previous preview) |
| `v3.119.2-preview.1.1` | `v3.119.1` (last stable) |
| `v3.119.2` (stable) | `v3.119.2-preview.N.X` (last preview of this version) |
| `v3.119.2.1-preview.1.1` (hotfix) | `v3.119.2` (stable being hotfixed) |

### Commands

```bash
# Preview (e.g., v3.119.2-preview.2.3)
gh release create {tag} \
  --title "Version {X.Y.Z} (Preview {N})" \
  --generate-notes \
  --notes-start-tag {previous-tag} \
  --prerelease \
  --verify-tag

# Stable (e.g., v3.119.2)
gh release create {tag} \
  --title "Version {X.Y.Z}" \
  --generate-notes \
  --notes-start-tag {previous-tag} \
  --verify-tag

# Upload samples for stable releases (if available)
gh release upload {tag} samples.zip
```

- `--title` sets the release title (use format above)
- `--generate-notes` auto-generates release notes from PRs/commits
- `--notes-start-tag` specifies the previous release to diff from (required)
- `--prerelease` marks as prerelease (preview only)
- `--verify-tag` ensures the tag exists before creating the release

> The generated notes are the **raw input** for Step 7, which extracts a short,
> customer-facing teaser from them and keeps this full list folded below it.

---

## Step 7: Add a Customer-Facing Teaser

The auto-generated notes from Step 6 are a flat wall of **every** merged PR (CI,
version bumps, dependency refreshes, backports — 100+ lines). Maintainers told us this
is "too heavy and hard to find things." So we keep that full list (it carries the PR
numbers + author handles for free) but **fold it into a `<details>` block** and add a
short **customer teaser** on top with only the bits a package consumer cares about.

The teaser is generated **only from the release log we just created** — no website
release-notes, no `documentation/docfx/` files, no git operations, no waiting.

👉 **See [references/github-release-teaser.md](references/github-release-teaser.md)** — the
canonical playbook with the full classification rules, teaser template, and a worked
example. Process:

1. **Capture** the generated log:
   ```bash
   gh release view {tag} --json body -q '.body' > /tmp/skiasharp/release/generated-log.md
   ```
2. **Build the teaser** from `generated-log.md` following the doc's *Classifying the PRs*
   section. In short: drop the plumbing (CI/build/test, build-tooling and version bumps,
   docs/notes automation, backport, internal refactors), then classify the rest into these
   sections **in this order** and omit any that are empty:
   - **⚠️ Breaking Changes** — removed/renamed/retyped public APIs, newly `[Obsolete]`/
     deprecated APIs (incl. promoted to warning/error), changed defaults, min-version or TFM
     drops.
   - **✨ What's New** — new features/APIs, perf wins, new platform support, and the **Skia
     engine milestone bump** (a headline, not a dependency).
   - **🐛 Fixes** — consumer-visible bug fixes on public types/scenarios (fold CI/docs/sample
     fixes and vague `[skia-sync]` engine-sync fixes).
   - **📦 Dependency Updates** — bundled **native** library bumps (libpng, freetype, …) as
     `Updated <dep> to <version>`. **Never** write "security" or name a CVE.

   End each bullet with `by @author (#NNNN)`, then add a `Thanks to our contributors:` line
   of the unique community handles. Open with one neutral subtitle line.
3. **Assemble** the final body — teaser on top, then the captured log folded below —
   per the template, and write it to `/tmp/skiasharp/release/release-body.md`.
4. **Update** the release:
   ```bash
   gh release edit {tag} --notes-file /tmp/skiasharp/release/release-body.md
   ```

> This is the **only** content step for the GitHub Release. The richer, categorized
> website release notes are produced separately by the workflow dispatched in Step 5;
> the teaser links out to them but never reads or waits on them.

---

## Step 8: Close Milestone (Stable only)

**Skip for preview releases.**

```bash
gh api repos/:owner/:repo/milestones --jq '.[] | "\(.number): \(.title)"'
gh api repos/:owner/:repo/milestones/{number} -X PATCH -f state=closed
```

---

## Error Recovery

### Pipeline Fails

| Failure Point | Recovery |
|---------------|----------|
| Pipeline won't start | Verify branch name, check Azure DevOps permissions |
| Build fails mid-run | Check logs, fix issue on release branch, re-run pipeline |
| Approval rejected | Re-trigger pipeline with correct settings |
| Push step fails | Check NuGet.org status, retry pipeline |

### NuGet.org Issues

| Issue | Recovery |
|-------|----------|
| Indexing takes >15 min | Normal for large packages. Keep polling. |
| Package shows 404 after publish | Wait up to 30 min. NuGet CDN propagation delay. |
| Wrong version published | **Cannot unpublish.** Release new corrected version. |

### Git/GitHub Issues

| Issue | Recovery |
|-------|----------|
| Tag push rejected | Check if tag exists: `git ls-remote --tags origin \| grep {tag}` |
| Tag already exists | **Cannot delete.** Must use different tag or release new version. |
| GitHub release fails | Re-run `gh release create` with `--verify-tag` |
| Release notes wrong | Edit with `gh release edit {tag} --notes-file ...` |

### General Recovery

If you've partially completed and need to resume:
1. Check what's done: `gh release view {tag}` (release exists?), `git ls-remote --tags origin` (tag exists?)
2. Skip completed steps
3. Continue from where you left off

---

## Resources

- [releasing.md](../../../documentation/dev/releasing.md) — Version patterns, tag formats, workflow diagrams
- [references/github-release-teaser.md](references/github-release-teaser.md) — Customer teaser playbook: classification rules + template
