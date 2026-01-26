---
name: release-publish
description: >
  Publish SkiaSharp packages and finalize the release.
  
  Use when user says "publish X", "finalize X", "tag X", or "finish release X".
  
  This is the FINAL step - after release-testing passes.
  Publishes to NuGet.org, creates tag, GitHub release, and closes milestone.
---

# Release Publish Skill

Publish packages to NuGet.org and finalize releases.

⚠️ **NO UNDO:** This is step 3 of 3. See [releasing.md](../../../documentation/releasing.md) for full workflow.

**Prerequisite:** release-testing must have passed. The versions should already be known from testing.

---

## Step 1: Confirm Versions

The user should provide the versions from release-testing. If not, ask for them:
- SkiaSharp version (e.g., `3.119.2-preview.2.3`)
- HarfBuzzSharp version (e.g., `8.3.1.4-preview.2.3`)

**Quick verification** - confirm packages exist on preview feed:
```bash
dotnet package search SkiaSharp --source "https://aka.ms/skiasharp-eap/index.json" --exact-match --prerelease --format json | jq -r '.searchResult[].packages[].version' | grep "{expected-skia-version}"
```

If the expected version is missing, STOP and ask user to verify testing was completed.

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

Verify the packages are available on NuGet.org:
```bash
dotnet package search SkiaSharp --source https://api.nuget.org/v3/index.json --exact-match --prerelease --format json | jq -r '.searchResult[].packages[].version' | grep "{expected-skia-version}"
```

Or check: `https://www.nuget.org/packages/SkiaSharp/{skia-version}`

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

```bash
# Preview (tag = v3.119.2-preview.2.5)
gh release create {tag} --generate-notes --prerelease --verify-tag

# Stable (tag = v3.119.2)
gh release create {tag} --generate-notes --verify-tag
gh release upload {tag} samples.zip  # if available
```

The `--generate-notes` flag auto-generates release notes from PRs/commits since last release. The `--verify-tag` ensures the tag exists before creating the release.

---

## Step 6: Close Milestone (Stable only)

**Skip for preview releases.**

```bash
gh api repos/:owner/:repo/milestones --jq '.[] | "\(.number): \(.title)"'
gh api repos/:owner/:repo/milestones/{number} -X PATCH -f state=closed
```

---

## Resources

- [releasing.md](../../../documentation/releasing.md) — Version patterns, tag formats, workflow diagrams
