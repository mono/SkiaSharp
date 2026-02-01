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

⚠️ **NO UNDO:** This is step 3 of 3. See [releasing.md](../../../documentation/releasing.md) for full workflow.

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
│  5. Create GitHub Release→ Generate notes, set prerelease flag     │
│  6. Annotate Notes       → Add platform/contributor emojis         │
│  7. Close Milestone      → Stable releases only                    │
└────────────────────────────────────────────────────────────────────┘
```

**Preview vs Stable differences:**
| Step | Preview | Stable |
|------|---------|--------|
| 2. Pipeline checkbox | "Push Preview" | "Push Stable" |
| 4. Tag format | `vX.Y.Z-preview.N.{build}` | `vX.Y.Z` |
| 5. GitHub Release | `--prerelease` flag | No flag, attach samples |
| 7. Milestone | Skip | Close milestone |

---

## Step 1: Confirm Versions

**Prerequisite:** release-testing must have passed. Versions should be known from testing.

The user should provide:
- SkiaSharp version (e.g., `3.119.2-preview.2.3`)
- HarfBuzzSharp version (e.g., `8.3.1.4-preview.2.3`)

If not provided, ask for them using `ask_user`.

**Quick verification** — confirm packages exist on preview feed:
```bash
dotnet package search SkiaSharp --source "https://aka.ms/skiasharp-eap/index.json" --exact-match --prerelease --format json | jq -r '.searchResult[].packages[].version' | grep "{expected-skia-version}"
```

If missing, STOP and ask user to verify testing was completed.

---

## Step 2: Publish to NuGet.org

Trigger the [publish pipeline](https://dev.azure.com/devdiv/DevDiv/_build?definitionId=25298) to push packages to NuGet.org.

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

## Step 5: Create GitHub Release

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

---

## Step 6: Annotate Release Notes with Emojis

After creating the release, annotate each PR line with **platform** and **community** emojis.

👉 **See [references/release-notes.md](references/release-notes.md)** for:
- Complete emoji reference (platform + contributor)
- Label-to-platform mapping
- Title keyword detection
- Full annotation process and examples

**Quick summary:**
1. Get release body: `gh release view {tag} --json body -q '.body' > /tmp/release-body.md`
2. For each PR: determine platform emoji, add ❤️ for non-mattleibow contributors
3. Build sections: Breaking Changes (if any), New Features (if any), What's Changed (all)
4. Update release: `gh release edit {tag} --notes-file /tmp/release-body.md`

---

## Step 7: Close Milestone (Stable only)

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

- [releasing.md](../../../documentation/releasing.md) — Version patterns, tag formats, workflow diagrams
- [references/release-notes.md](references/release-notes.md) — Emoji annotation details
