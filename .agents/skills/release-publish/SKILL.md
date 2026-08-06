---
name: release-publish
description: >
  Publish SkiaSharp packages and finalize the release.
  
  Use when user says "publish X", "finalize X", "tag X", or "finish release X".
  
  This is the FINAL step - after release-testing passes.
  Publishes to NuGet.org, creates tag and GitHub release, and closes an exact stable
  milestone when one exists.
  
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
│  2. Publish to NuGet.org → Confirm queue, human approves push      │
│  3. Verify Published     → After pipeline success, poll NuGet.org  │
│  4. Tag Release          → Push git tag (ask_user first!)          │
│  5. Refresh Web Notes    → Dispatch once from main; do not wait    │
│  6. Create GitHub Release→ Generate notes, set prerelease flag     │
│  7. Customer Teaser      → Extract key bits from the generated log │
│  8. Milestone Hygiene    → Stable: close exact match if it exists  │
└────────────────────────────────────────────────────────────────────┘
```

**Preview vs Stable differences:**
| Step | Preview | Stable |
|------|---------|--------|
| 1. NuGet version | `X.Y.Z-preview.N.{build}` | `X.Y.Z` (no build number) |
| 2. Publish stage | "Push Preview" | "Push Stable" |
| 4. Tag format | `vX.Y.Z-preview.N.{build}` | `vX.Y.Z` |
| 5. Website notes refresh | Dispatch once; do not wait | Dispatch once; do not wait |
| 6. GitHub Release | `--prerelease` flag | No flag, attach samples |
| 7. Customer teaser | Breaking + What's New + Fixes (usually short) | + Dependency Updates + contributors |
| 8. Milestone | Skip | Close exact milestone if one exists |

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

# Stable: search for internal stable builds (public NuGet.org version remains the base)
dotnet package search SkiaSharp --source "https://aka.ms/skiasharp-eap/index.json" --exact-match --prerelease --format json | jq -r '.searchResult[].packages[].version' | grep "^{base}-stable\."
```

If missing, STOP and ask user to verify testing was completed.

---

## Step 2: Publish to NuGet.org

Trigger the [publish pipeline](https://dev.azure.com/devdiv/DevDiv/_build?definitionId=25298) to push packages to NuGet.org.

### Verifying Source Build Before Publishing

Before triggering the publish pipeline, confirm builds completed using the **release-status** skill:

```bash
python .agents/skills/release-status/scripts/pipeline-status.py release/{version}
```

The `SkiaSharp` pipeline (ID 10789) must show `[OK]` - this is the pipeline that produced the
packages on the internal feed. See [release-status](../release-status/SKILL.md) for details.

### Pipeline Steps

Use the tested CLI/REST path in
[references/azure-publish.md](references/azure-publish.md). Use the numeric managed run ID from
release-status to verify pipeline `10789`, `completed/succeeded`, the exact release branch and
full commit, and the expected stable/preview/RC build-number label.

⚠️ `resources.pipelines.SkiaSharp.version` is the managed pipeline **build number string**, for
example `4.151.1-stable.1+4.151.1`. Never put the numeric managed run/build ID in that field; the
numeric ID is only for querying and validating the source run.

Show the validated source summary and JSON body, then use `ask_user` for explicit confirmation.
Before asking, verify that pipeline `25298` has no active run as documented in
[references/azure-publish.md](references/azure-publish.md). Only after confirmation, invoke
`scripts/queue-publish.py` with the verified build-number string. The script validates the
build-number shape, infers `pushStable`, queues pipeline `25298`, and prints the new run ID and URL.

### Verification During Pipeline Run

Queue confirmation and push approval are separate trust boundaries:

1. **Run name** — Wait for the queued run to rename itself to
   `SkiaSharp {managed-build-number}` and verify it exactly.
2. **Push type** — Verify the timeline stage is **"Push Preview"** or **"Push Stable"**:
   - Preview release → should show "Push Preview"
   - Stable release → should show "Push Stable"
3. **Human approval** — Ask the user to open Azure DevOps and approve the push.

The agent may queue after confirmation, but it must never approve the Azure DevOps push through a
UI, CLI, or approvals API. Poll the publish run until it reaches `completed/succeeded`; stop on any
other terminal result.

---

## Step 3: Verify Packages Published

Do not start this step until publish pipeline `25298` is terminal `completed/succeeded`. In
particular, do not poll NuGet.org while the run is waiting for human approval: an existing public
version can create a false success signal for a rerun.

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
git push origin {tag} || exit 1
```

---

## Step 5: Refresh Website Release Notes & API Diffs

After pushing the tag in Step 4, dispatch **Sync - Release Notes & API Diffs** once from `main`:

```bash
gh workflow run update-release-notes.lock.yml \
  --repo mono/SkiaSharp \
  --ref main || exit 1
echo "Started Sync - Release Notes & API Diffs from main."
```

Once GitHub accepts the dispatch, report that it was started and treat this step as complete.

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

## Step 8: Stable Milestone Hygiene (Optional)

Preview and RC releases skip milestone closure. For stable releases, a milestone is optional and
must match the exact public version title (`X.Y.Z` or `X.Y.Z.F`, no `v` prefix). Never use a
substring/nearest match and never create a milestone during publishing.

```bash
# Slurp all pages before exact matching so similarly named milestones cannot qualify.
STABLE_VERSION="{stable-version}"
gh api "repos/mono/SkiaSharp/milestones?state=all&per_page=100" --paginate --slurp \
  | jq --arg version "$STABLE_VERSION" \
      '[.[][] | select(.title == $version) |
        {number,title,state,open_issues,closed_issues}]'
```

- **No exact match:** report `No milestone exists; nothing to close` and succeed.
- **Already closed:** report it and succeed.
- **More than one exact match:** stop and ask the user which milestone is authoritative.
- **One open exact match with open issues:** list them with
  `gh issue list --repo mono/SkiaSharp --milestone "{stable-version}" --state open`, then ask the
  user before closing or reassigning anything. Leaving the optional milestone open does not fail
  the release.
- **One open exact match with zero open issues:** close that exact milestone number:

  ```bash
  gh api repos/mono/SkiaSharp/milestones/{number} -X PATCH -f state=closed
  ```

---

## Error Recovery

### Pipeline Fails

| Failure Point | Recovery |
|---------------|----------|
| Pipeline won't start | Verify branch name, check Azure DevOps permissions |
| Build fails mid-run | Check logs, fix issue on release branch, re-run pipeline |
| Human approval rejected | Stop; verify the selected build and ask before queueing a replacement |
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
- [references/azure-publish.md](references/azure-publish.md) — Validated REST queue and human approval boundary
- [references/github-release-teaser.md](references/github-release-teaser.md) — Customer teaser playbook: classification rules + template
