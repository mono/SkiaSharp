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
gh release view {tag} --json body -q '.body' > /tmp/release-body.md
```

### 2. Analyze Each PR

For each PR line (format: `* Description by @author in URL`):

```bash
# Extract PR number from URL and fetch details
gh pr view {number} --json labels,author
```

Determine:
- **Platform** from PR title/labels (required - use 📦 if none)
- **Contributor** — add ❤️ if author is not `mattleibow`
- **Breaking change** — title contains `BREAKING`, removes API
- **New feature** — title contains `Add`, `Support`, `Enable`, `Implement`, or bumps Skia/HarfBuzz

### 3. Build Sections

- **Breaking Changes** — only if there are breaking PRs (list them here AND in What's Changed)
- **New Features** — only if there are feature PRs (list them here AND in What's Changed)
- **What's Changed** — always include, contains ALL PRs

Format all items: `* {platform}{❤️} Description...`

### 4. Update Release

```bash
gh release edit {tag} --notes-file /tmp/release-body.md
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
