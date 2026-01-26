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

| Type | Action |
|------|--------|
| Preview | Optional - trigger [publish pipeline](https://dev.azure.com/devdiv/DevDiv/_build?definitionId=25298) |
| Stable | Required - trigger [publish pipeline](https://dev.azure.com/devdiv/DevDiv/_build?definitionId=25298) |

Ask user to trigger pipeline and wait for completion.

---

## Step 3: Verify Packages Published

**If published to NuGet.org:**
```bash
dotnet package search SkiaSharp --source https://api.nuget.org/v3/index.json --exact-match --prerelease --format json | jq -r '.searchResult[].packages[].version' | grep "{expected-skia-version}"
```

Or check: `https://www.nuget.org/packages/SkiaSharp/{skia-version}`

**Preview skipping NuGet.org:** Verify packages still exist on preview feed before tagging.

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
