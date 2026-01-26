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

### Categories

| Section | When to Include |
|---------|-----------------|
| **Breaking Changes** | Only if there are breaking changes |
| **New Features** | Only if there are new features |
| **What's Changed** | Always (full list with all PRs) |

### Emojis

**Platform (required on all items):**
| Emoji | Meaning |
|-------|---------|
| 🍎 | Apple (iOS/macOS/tvOS/Mac Catalyst) |
| 🪟 | Windows |
| 🐧 | Linux |
| 🤖 | Android |
| 🌐 | WebAssembly/Blazor |
| 🎨 | Core API |
| 🏗️ | Build system/CI |
| 📦 | General (fallback - always use something!) |

**Contributor:**
| Emoji | Meaning |
|-------|---------|
| ❤️ | Community contribution (not @mattleibow) |

### Release Note Structure

```markdown
## Breaking Changes
* 🎨 Remove deprecated SKFoo API... by @mattleibow

## New Features
* 🍎❤️ Support SKMetalView on tvOS... by @MartinZikmund
* 🐧❤️ Add riscv64 build support... by @kasperk81

## What's Changed
* 🎨 Remove deprecated SKFoo API... by @mattleibow
* 🍎❤️ Support SKMetalView on tvOS... by @MartinZikmund
* 🪟❤️ Enable Control Flow Guard... by @Aguilex
* 📦 Adding the initial set of AI docs... by @mattleibow
* 🏗️ Bump to the next version... by @mattleibow

## New Contributors
(Auto-generated)

**Full Changelog**: (Auto-generated)
```

### Process

1. Get the release body:
   ```bash
   gh release view {tag} --json body -q '.body' > /tmp/release-body.md
   ```

2. For each PR line (format: `* Description by @author in URL`):
   - Fetch PR details: `gh pr view {number} --json labels,author`
   - Determine **platform** from PR title/labels (required - use 📦 if none)
   - Add ❤️ if author is not `mattleibow`
   - Check if **breaking change** (title contains `BREAKING`, removes API)
   - Check if **new feature** (title contains `Add`, `Support`, `Enable`, `Implement`, or bumps Skia/HarfBuzz)

3. Build sections:
   - **Breaking Changes** — only if there are breaking PRs (list them here AND in What's Changed)
   - **New Features** — only if there are feature PRs (list them here AND in What's Changed)
   - **What's Changed** — always include, contains ALL PRs

4. Format all items: `* {platform}{❤️} Description...`

5. Update the release:
   ```bash
   gh release edit {tag} --notes-file /tmp/release-body.md
   ```

### Label-to-Platform Mapping

| Label Pattern | Platform Emoji |
|---------------|----------------|
| `os/Windows*` | 🪟 |
| `os/macOS`, `os/iOS`, `os/tvOS` | 🍎 |
| `os/Linux` | 🐧 |
| `os/Android` | 🤖 |
| `backend/SkiaSharp` | 🎨 |
| `area/Build` | 🏗️ |
| (no platform label) | 📦 |

### Title Keywords-to-Platform Mapping

| Title Contains | Platform Emoji |
|----------------|----------------|
| `iOS`, `macOS`, `tvOS`, `Apple`, `Metal`, `Catalyst` | 🍎 |
| `Windows`, `Win`, `UWP`, `WinUI`, `Direct3D`, `D3D` | 🪟 |
| `Linux`, `Alpine`, `riscv`, `LoongArch` | 🐧 |
| `Android`, `NDK` | 🤖 |
| `WebAssembly`, `Wasm`, `Blazor` | 🌐 |
| `SK*` (API classes) | 🎨 |
| `Build`, `CI`, `Pipeline` | 🏗️ |
| (no platform keywords) | 📦 |

### Example Transformation

**Original (auto-generated):**
```
* Support SKMetalView on tvOS by @MartinZikmund in https://github.com/mono/SkiaSharp/pull/3114
* Fix the incorrect call in SafeRef by @kkwpsv in https://github.com/mono/SkiaSharp/pull/3143
* Adding the initial set of AI docs by @mattleibow in https://github.com/mono/SkiaSharp/pull/3406
```

**After annotation:**
```
* 🍎❤️ Support SKMetalView on tvOS by @MartinZikmund in https://github.com/mono/SkiaSharp/pull/3114
* 🎨❤️ Fix the incorrect call in SafeRef by @kkwpsv in https://github.com/mono/SkiaSharp/pull/3143
* 📦 Adding the initial set of AI docs by @mattleibow in https://github.com/mono/SkiaSharp/pull/3406
```

---

## Step 7: Close Milestone (Stable only)

**Skip for preview releases.**

```bash
gh api repos/:owner/:repo/milestones --jq '.[] | "\(.number): \(.title)"'
gh api repos/:owner/:repo/milestones/{number} -X PATCH -f state=closed
```

---

## Resources

- [releasing.md](../../../documentation/releasing.md) — Version patterns, tag formats, workflow diagrams
