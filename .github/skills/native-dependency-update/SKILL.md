---
name: native-dependency-update
description: >
  Update native dependencies (libpng, libexpat, zlib, libwebp, harfbuzz, freetype, libjpeg-turbo, etc.)
  in SkiaSharp's Skia fork. Handles security CVE fixes, bug fixes, and version bumps.
  
  Use when user asks to:
  - Bump/update a native dependency (libpng, zlib, expat, webp, etc.)
  - Fix a CVE or security vulnerability in a native library
  - Update Skia's DEPS file
  - Check what version of a dependency is currently used
  - Analyze breaking changes between dependency versions
  
  Triggers: "bump libpng", "update zlib", "fix CVE in expat", "update native deps",
  "what version of libpng", "check for breaking changes".

  For security audits (finding CVEs, checking PR coverage), use the `security-audit` skill instead.
---

# Native Dependency Update Skill

Update native dependencies in SkiaSharp's Skia fork (mono/skia).

## Key References

- **[documentation/dependencies.md](../../../documentation/dependencies.md)** — Complete dependency list, cgmanifest.json format, CVE database names
- **[references/breaking-changes.md](references/breaking-changes.md)** — Breaking change analysis guidance

## ⚠️ MANDATORY: Follow Every Phase

You MUST complete ALL phases in order. Do not skip phases to save time.

### Pre-Flight Checklist

Before starting, confirm you will:
- [ ] Complete Phase 1-8 in order
- [ ] Update DEPS, `externals/skia` submodule, AND `cgmanifest.json`
- [ ] Build and test locally before any PR
- [ ] Create PRs (never push directly to `skiasharp` or `main`)
- [ ] Use "Fixes #NNNNN" in PR body (never close issues manually)
- [ ] Stop and ask at every 🛑 checkpoint

## Critical Rules

> **🛑 STOP AND ASK** before: Creating PRs, Merging PRs, Force pushing, Any destructive git operations

### ❌ NEVER Do These

| Shortcut | Why It's Wrong |
|----------|----------------|
| Push directly to `skiasharp` or `main` | Bypasses PR review and CI |
| Skip native build phase | CI is too slow; must verify locally first |
| Manually close issues | Breaks audit trail; PR merge auto-closes |
| Skip `cgmanifest.json` update | Security compliance requires it |
| Skip `externals/skia` submodule update | SkiaSharp won't use the new dependency version |
| Revert/undo pushed commits | Fix forward with new commit instead |

---

## Workflow

### Phase 1: Discovery

1. **Check for existing PRs** in mono/SkiaSharp and mono/skia
2. **Check current version** in `externals/skia/DEPS`
3. **Find target version** — get commit hash with `git rev-parse {tag}^{commit}`

### Phase 2: Analysis

**Source File Verification (MANDATORY):**

```bash
cd externals/skia/third_party/externals/{dep}
git diff {old}..{new} --diff-filter=AD --name-only  # Added/Deleted files
```

Cross-reference against `externals/skia/third_party/{dep}/BUILD.gn` — new source files may need to be added.

👉 See [references/breaking-changes.md](references/breaking-changes.md) for risk assessment.

### Phase 3: Local Changes

1. Edit `externals/skia/DEPS` with new commit hash
2. Update BUILD.gn if needed (rare)
3. **Update `cgmanifest.json`** with new version (required for CVE detection)
4. Checkout new version in dependency directory

👉 See [documentation/dependencies.md](../../../documentation/dependencies.md#cgmanifestjson) for the cgmanifest format.

### Phase 4: Build & Test

> 🛑 **MANDATORY: Build locally before creating PRs.**

```bash
dotnet cake --target=externals-macos --arch=arm64
dotnet cake --target=tests-netcore --skipExternals=all
```

> ⚠️ **Never use `dotnet test` directly** — use Cake for proper skip trait handling.

### Phase 5: Create PRs

> **🛑 STOP AND ASK FOR APPROVAL** before creating PRs.

**Both PRs must be created together** — CI requires both.

1. **mono/skia PR**: Branch `dev/update-{dep}` → target `skiasharp`
2. **SkiaSharp PR**: Branch `dev/update-{dep}` → target `main`
   - Update `externals/skia` submodule to point to mono/skia PR
   - Update `cgmanifest.json`
   - Link issues with "Fixes #NNNNN" in body

### Phase 6: Monitor CI

SkiaSharp uses Azure DevOps. mono/skia has no CI — relies on SkiaSharp's.

### Phase 7: Merge

> **🛑 STOP AND ASK FOR APPROVAL** before each merge.

1. Merge mono/skia PR first
2. Update SkiaSharp submodule to merged commit
3. SkiaSharp PR may auto-merge; check state first

### Phase 8: Verify

- Related issues auto-closed
- Both PRs merged
- No failures on main

---

## Common Dependencies

| Dependency | DEPS Key |
|------------|----------|
| libpng | `third_party/externals/libpng` |
| libexpat | `third_party/externals/expat` |
| zlib | `third_party/externals/zlib` |
| libwebp | `third_party/externals/libwebp` |
| harfbuzz | `third_party/externals/harfbuzz` |
| freetype | `third_party/externals/freetype` |
| libjpeg-turbo | `third_party/externals/libjpeg-turbo` |

For cgmanifest names and upstream URLs, see [documentation/dependencies.md](../../../documentation/dependencies.md#name-mapping).
