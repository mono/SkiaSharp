---
name: release-branch
description: >
  Create a release branch for SkiaSharp.
  Use when user says "release X", "start release X", "create release branch for X",
  "I want to release", or "release now".
  This is the FIRST step of releasing - creates branch and pushes to trigger CI.
  Can auto-detect next preview version from main branch.
---

# Release Branch Skill

Create release branches for SkiaSharp versions.

⚠️ **NO UNDO:** This is **Step 1 of 4** in the release pipeline. See [releasing.md](../../../documentation/dev/releasing.md) for full workflow.

**Pipeline:** **Step 1 (this skill)** → [Step 2: release-status](../release-status/SKILL.md) → [Step 3: release-testing](../release-testing/SKILL.md) → [Step 4: release-publish](../release-publish/SKILL.md)

## ⚠️ Branch Protection (COMPLIANCE REQUIRED)

> **🛑 NEVER commit directly to `main` or `skiasharp` branches. This is a policy violation.**

| Repository | Protected Branches | Required Action |
|------------|-------------------|-----------------|
| SkiaSharp (parent) | `main` | Create `release/X.Y.Z` branch, never commit to main |
| externals/skia (submodule) | `main`, `skiasharp` | Must use feature branch if submodule changes needed |

**Release branches are cut FROM an integration branch (`main` or `release/X.Y.x`), but never modify those branches directly — always go through a branch + PR.**

---

## Concept: Integration Branches

There is **not one "main"** — there is an **integration branch per release line**.
Each one always sits at the *next unreleased version* for its line, with
`PREVIEW_LABEL: preview.0`:

| Integration branch | Line it serves | Example state |
|--------------------|----------------|---------------|
| `main` | Newest in-development line (not yet forked) | `4.150.0` / `preview.0` |
| `release/X.Y.x` | An established / maintenance line | `release/3.119.x` @ `3.119.5`; `release/4.148.x` @ `4.148.0` |

- **Every** release (preview, rc, stable, patch) is cut FROM the line's
  integration branch — `release/{version}` is branched off it.
- **As soon as a stable is cut**, the integration branch is **bumped** to the
  next version (see Step 5) — immediately, *not* after the release publishes.
  After branching, PRs keep merging into the integration branch, and they must
  land on the *next* version, not the one that was just cut. Fixes for the cut
  release go on its own `release/{version}` branch.
- A new minor's `release/X.Y.x` is forked from `main` when stabilization begins;
  `main` is then bumped to the next minor. From that point on, all `X.Y.*`
  releases come from `release/X.Y.x`, not `main`.

---

## Step 1: Determine Version

### Auto-detect (user says "release now")

1. Fetch the integration branch (`main` for the newest line, or `release/X.Y.x` for a maintenance line) and read `SKIASHARP_VERSION` from `scripts/azure-templates-variables.yml`
2. List existing branches: `git branch -r | grep "release/{version}-preview"`
3. Next preview = highest + 1 (or 1 if none)
4. **⚠️ Semver check:** Also verify no bare `release/{version}` branch exists — if it does, the stable release is already cut and you should NOT create another preview. Ask the user to confirm.
5. Confirm with user: "Next release will be `X.Y.Z-preview.N`. Proceed?"

### User provides version

Use the provided version directly.

---

## Step 2: Determine Release Type

⚠️ **Semver ordering:** A bare version `X.Y.Z` is ALWAYS newer than `X.Y.Z-preview.N`. When listing
branches to find the latest, remember that `release/3.119.2` > `release/3.119.2-preview.3`.
Do NOT use alphabetical sorting — it gives wrong results for semver.

| Version Format | Type | Base (integration branch) | PREVIEW_LABEL |
|----------------|------|---------------------------|---------------|
| `X.Y.Z-preview.N` / `X.Y.Z-rc.N` | Preview / RC | `release/X.Y.x` (or `main` if the line isn't forked yet) | `preview.N` / `rc.N` |
| `X.Y.Z` | Stable | `release/X.Y.x` | `stable` |
| `X.Y.Z.F-preview.N` | Hotfix Preview | tag `vX.Y.Z` | `preview.N` |
| `X.Y.Z.F` | Hotfix Stable | `release/X.Y.Z.F-preview.{latest}` | `stable` |

> **Base = the line's integration branch**, NOT a previous preview/rc branch.
> A stable `X.Y.Z` is cut from `release/X.Y.x` (which already produced its
> previews/rcs), not from `release/X.Y.Z-preview.{latest}`.
>
> Find the integration branch:
> ```bash
> git branch -r | grep -E "release/X\.Y\.x$"   # established line (substitute real X.Y)
> # newest line not yet forked → use main
> ```

**NuGet version format by release type:**
- **Preview:** `{base}-{PREVIEW_LABEL}.{build}` (e.g., `3.119.2-preview.2.3`) — build number is part of the prerelease tag
- **Stable:** `{base}` only (e.g., `3.119.2`) — the build number is NEVER appended to stable versions. On the internal feed, stable builds appear as `{base}-stable.{build}` but the published version is just `{base}`.

---

## Step 3: Create Branch and Update PREVIEW_LABEL

1. Checkout the base — the line's **integration branch** (`release/X.Y.x`, or
   `main` / a tag for the special cases above)
2. Create branch `release/{version}`
3. Set `PREVIEW_LABEL` with the helper script (edits + verifies
   `scripts/azure-templates-variables.yml`):
   ```bash
   pwsh .agents/skills/release-branch/scripts/bump-version.ps1 -PreviewLabel {label}
   # {label} = stable | preview.N | rc.N   (add -DryRun to preview)
   ```
4. Commit: `git commit -m "Bump the version to {version}"`
5. Show diff summary to user and **confirm with the user** before pushing

---

## Step 4: Push Branch

```bash
git push -u origin release/{version}
```

This triggers the CI pipeline chain (2-4 hours total):

```
SkiaSharp-Native → SkiaSharp → SkiaSharp-Tests
   (~60-90 min)    (~30-60 min)   (~15-30 min)
```

Use the **release-status** skill to track build progress. Quick check:

```bash
python3 .agents/skills/release-status/scripts/pipeline-status.py release/{version}
```

---

## Step 5: Bump the Integration Branch (Immediately After Cutting a Stable)

**Do this right after Step 4** — as soon as the stable release branch is cut and
pushed, advance that line's **integration branch** to the next version. Do **not**
wait for the release to publish.

Why immediately: once `release/{version}` is branched off, PRs keep merging into
the integration branch (`release/X.Y.x`, and `main` for its line). Those changes
must land on the **next** version, not the one that was just cut. Any fixes for
the cut release go onto its own `release/{version}` branch instead. If the
integration branch isn't bumped at cut time, post-branch merges collide with the
released version.

Previews/RCs do **not** trigger this — they're iterations toward the same
`X.Y.Z`. Cutting the **stable** does, because that finalizes `X.Y.Z`. (e.g. when
`3.119.4` was cut, `release/3.119.x` was bumped to `3.119.5` — "Bump to the next
version after release".)

**How each line advances:**

| Released line | Integration branch | How it advances |
|---------------|--------------------|-----------------|
| Maintenance line | `release/X.Y.x` | Next **patch** (`X.Y.Z` → `X.Y.(Z+1)`) — use the helper below. `assembly` stays pinned at `X.Y.0.0`; only `file` / `nuget` / `SKIASHARP_VERSION` move. |
| Newest line | `main` | Next **minor** (`X.Y` → next milestone). This is a **Skia milestone update** (e.g. `[skia-sync] Update Skia to milestone mNNN`) that also rewrites the milestone/soname/increment lines, `assembly`, `cgmanifest.json` and native sources — a **separate process, not this helper.** |

> The helper below covers the **maintenance-line patch bump only**. It refuses a
> `major.minor` change (that's a milestone update — out of scope).

1. Create branch `bump-version-{next}` from the maintenance branch
   (e.g. `git checkout -b bump-version-{next} origin/release/X.Y.x`)

2. Apply the patch bump with the helper script. It edits + verifies
   `SKIASHARP_VERSION` in `azure-templates-variables.yml` and all SkiaSharp /
   HarfBuzzSharp `file`+`nuget` lines in `VERSIONS.txt` (SkiaSharp `assembly`
   stays `X.Y.0.0`, HarfBuzzSharp `assembly` stays `1.0.0.0`):
   ```bash
   pwsh .agents/skills/release-branch/scripts/bump-version.ps1 \
     -SkiaSharpVersion {next} \
     -HarfBuzzSharpVersion {hb-next} \
     -PreviewLabel preview.0
   # add -DryRun first to preview
   ```
   - `{hb-next}`: the next HarfBuzzSharp version — normally increment the last
     digit (`8.3.1.4` → `8.3.1.5`); on a native HarfBuzz upgrade, reset to the
     3-digit native version (e.g. `14.2.0`).

3. Commit: `git commit -m "Bump to the next version ({next}) after release"`

4. Show diff to user, then:
   ```bash
   git push -u origin bump-version-{next}
   gh pr create --title "Bump to the next version ({next}) after release" --body ""
   gh pr merge --merge --delete-branch
   ```

---

## Resources

- [scripts/bump-version.ps1](scripts/bump-version.ps1) — Sets `PREVIEW_LABEL` and/or bumps SkiaSharp + HarfBuzzSharp versions in `VERSIONS.txt` and `azure-templates-variables.yml`, with a verification gate (`-DryRun` to preview)
- [releasing.md](../../../documentation/dev/releasing.md) — Version patterns, HarfBuzz versioning, workflow diagrams
