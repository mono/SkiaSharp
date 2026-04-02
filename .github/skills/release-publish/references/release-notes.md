# Release Notes Annotation

Annotate GitHub release notes with platform and community emojis.

## Process Overview

1. Get the release body
2. For each PR, determine platform and contributor status
3. Build categorized sections
4. Update the release

---

## Categories

| Section | When to Include |
|---------|-----------------|
| **Breaking Changes** | Only if there are breaking changes |
| **New Features** | Only if there are new features |
| **What's Changed** | Always (full list with all PRs) |

---

## Emojis

### Platform (required on all items)

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

### Contributor

| Emoji | Meaning |
|-------|---------|
| ❤️ | Community contribution (not @mattleibow) |

---

## Label-to-Platform Mapping

| Label Pattern | Platform Emoji |
|---------------|----------------|
| `os/Windows*` | 🪟 |
| `os/macOS`, `os/iOS`, `os/tvOS` | 🍎 |
| `os/Linux` | 🐧 |
| `os/Android` | 🤖 |
| `backend/SkiaSharp` | 🎨 |
| `area/Build` | 🏗️ |
| (no platform label) | 📦 |

## Title Keywords-to-Platform Mapping

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

---

## Commands

### 1. Get Release Body

```bash
gh release view {tag} --json body -q '.body' > /tmp/skiasharp/release/release-body.md
```

### 2. Analyze Each PR

For each PR line (format: `* Description by @author in URL`):

```bash
# Extract PR number from URL and fetch details
gh pr view {number} --json labels,author,title,body
```

Determine:
- **Platform** from PR title/labels (required - use 📦 if none)
- **Contributor** — add ❤️ if author is not `mattleibow`
- **Breaking change** — title contains `BREAKING`, removes API
- **New feature** — title contains `Add`, `Support`, `Enable`, `Implement`, or bumps Skia/HarfBuzz
- **Backport** — title starts with `[release/{version}]` prefix (see below)

### 2a. Handle Backport PRs

Backport PRs have titles prefixed with `[release/{version}]` and are created by `@github-actions`. These should be traced back to the original PR for proper attribution.

**Identifying backports:**
- Title starts with `[release/...]`
- Author is `github-actions[bot]`
- Has `backport` label

**Tracing to original:**
```bash
# Get backport PR body - contains reference to original
gh pr view {backport-number} --json body -q '.body'
# Output: "Backport of {commit} from #{original-pr-number}."

# Get original PR details
gh pr view {original-pr-number} --json author,title,labels
```

**Annotation format for backports:**
```
* {emoji}{❤️} {clean title} (originally by @{original-author} in #{original-pr}) by @github-actions in {backport-url}
```

- Remove the `[release/{version}]` prefix from the title
- Add `(originally by @{author} in #{number})` before `by @github-actions`
- Use the **original author** to determine ❤️ (not github-actions)
- Use the **original PR** title/labels for platform emoji

### 3. Build Sections

- **Breaking Changes** — only if there are breaking PRs (list them here AND in What's Changed)
- **New Features** — only if there are feature PRs (list them here AND in What's Changed)
- **What's Changed** — always include, contains ALL PRs

Format all items: `* {platform}{❤️} Description...`

### 4. Update Release

```bash
gh release edit {tag} --notes-file /tmp/skiasharp/release/release-body.md
```

---

## Release Note Structure

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

---

## Example Transformation

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

## Backport Example

**Original (auto-generated backport):**
```
* [release/4.119.2-preview.2] Add Spectre mitigation flag for libSkiaSharp.dll. by @github-actions in https://github.com/mono/SkiaSharp/pull/3497
```

**After tracing and annotation:**
```
* 🪟❤️ Add Spectre mitigation flag for libSkiaSharp.dll. (originally by @sshumakov in #3496) by @github-actions in https://github.com/mono/SkiaSharp/pull/3497
```

Key changes:
1. Removed `[release/4.119.2-preview.2]` prefix
2. Added `(originally by @sshumakov in #3496)` attribution
3. Added 🪟 (Windows) based on original PR content
4. Added ❤️ because @sshumakov is a community contributor
