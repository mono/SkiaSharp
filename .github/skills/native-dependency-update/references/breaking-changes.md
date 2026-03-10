# Breaking Change Analysis

Guidance for analyzing breaking changes when updating native dependencies.

## Contents

- [Risk Levels](#risk-levels)
- [What to Analyze](#what-to-analyze)
- [Risk by Version Type](#risk-by-version-type)
- [Library-Specific Notes](#library-specific-notes)
- [BUILD.gn Updates](#buildgn-updates)
- [Analysis Summary Template](#analysis-summary-template)

## Risk Levels

| Level | Description | Action |
|-------|-------------|--------|
| ✅ Safe | No breaking changes | Direct update |
| ⚠️ Minor | New optional features | May need BUILD.gn update |
| 🔴 Breaking | API/ABI changes | Requires code changes |

## What to Analyze

### 1. Changelog/Release Notes

Most projects document changes in files like CHANGES, CHANGELOG, NEWS, HISTORY, or RELEASE_NOTES. Read through the versions between current and target to understand what changed.

Look for:
- "Breaking change" or "API change" mentions
- Deprecated features being removed
- Function renames or signature changes
- New required dependencies

### 2. Source File Changes

Compare the current and target versions to identify file-level changes.

#### Verification Procedure (MANDATORY)

**Step 1: Get ALL added/deleted files (no path filter)**

```bash
cd externals/skia/third_party/externals/{dep}
git diff {old_version}..{new_version} --diff-filter=AD --name-only
```

> ⚠️ **Do NOT filter by path.** Different libraries have different structures—some use `src/`, some have files in root, some use `lib/`. Get the full picture first.

**Why this matters:** In the libwebp 1.3.2→1.6.0 update, checking only `src/dec`, `src/enc`, `src/dsp` missed the new `src/utils/palette.c` file, causing a link failure.

**Step 2: Cross-reference against BUILD.gn**

Open `externals/skia/third_party/{dep}/BUILD.gn` and examine the `sources` list. For each new `.c`/`.cpp` file:
- Is the file's directory pattern already in BUILD.gn?
- If yes → the new file likely needs to be added
- If no → probably tests/examples/tools that Skia doesn't compile

**Step 3: Check deleted files against BUILD.gn**

For each deleted file, search BUILD.gn. If referenced, it must be removed.

**Flags explained:**
- `--diff-filter=AD` — shows only Added (A) and Deleted (D) files
- `--name-only` — shows just filenames for easy scanning

#### Interpreting Results

**New source files** - May need to be added to BUILD.gn, especially if they're:
- Core library files (not tests/examples)
- Platform-specific optimizations (ARM NEON, Intel SSE, RISC-V RVV)

**Deleted source files** - Will break the build if BUILD.gn still references them.

**Modified source files** - Usually fine, but worth noting the scope of changes.

### 3. Header/API Changes

Examine changes to public header files to understand API evolution:

**Added exports** - New functionality, backwards compatible (safe)

**Removed exports** - Breaking change! Code using these will fail to compile/link.

**Changed signatures** - Breaking change! Callers need to be updated.

Many libraries have a symbols export file (symbols.def, exports.def, etc.) that clearly shows what's added or removed.

### 4. Build System Changes

Check if the library's own build configuration changed in ways that affect Skia's BUILD.gn:
- New required compiler defines
- New source file organization
- Changed include paths
- New or removed dependencies

## Risk by Version Type

**Security-only releases** - Usually patch the minimum code necessary. Very low risk.

**Patch versions (1.2.3 → 1.2.4)** - Bug fixes, typically backwards compatible. Low risk.

**Minor versions (1.2.x → 1.3.x)** - New features, deprecations may be introduced. Medium risk.

**Major versions (1.x → 2.x)** - Expect breaking changes. High risk, detailed analysis required.

## Library-Specific Notes

### libpng
- Very stable API, rarely breaks
- New platform optimizations added over time (ARM, SSE, RISC-V)
- Check `scripts/symbols.def` for export changes
- Security fixes are typically drop-in replacements

### libexpat
- Stable parser API
- Check for new required initialization or cleanup functions
- Memory allocation behavior may change

### zlib
- Extremely stable, core API unchanged for decades
- MiniZip is a separate optional component—check if Skia uses it before worrying about MiniZip CVEs

### libwebp
- Encoder/decoder APIs are stable
- New image format features may be added

### harfbuzz
- Shaping API is stable
- Font function callbacks may evolve
- Check for deprecated functions being removed

### libjpeg-turbo
- Very stable API
- Platform-specific SIMD optimizations may be added

### freetype
- Rendering API is stable
- Font loading functions occasionally evolve

## BUILD.gn Updates

BUILD.gn changes are typically needed when:
1. **New source files** are added to the core library
2. **Platform-specific code** is added (new CPU architecture support)
3. **New compile defines** are required
4. **Dependencies change** (new deps added or removed)

BUILD.gn changes are usually NOT needed for:
- Bug fixes in existing files
- Security patches
- Documentation updates
- Test/example changes
- New platform support SkiaSharp doesn't use (RISC-V, LoongArch, MIPS)

The BUILD.gn lives at `externals/skia/third_party/{dep}/BUILD.gn`. Look at existing patterns—sources are listed explicitly, platform-specific code uses `if (current_cpu == "...")` conditionals.

## Analysis Summary Template

When reporting analysis results:

```
## {dep} {old_version} → {new_version}

**Risk:** ✅ Safe / ⚠️ Minor / 🔴 Breaking

**Changes:**
- New APIs: {description or "None"}
- Removed APIs: {description or "None"}
- New source files: {list or "None"}
- Deleted source files: {list or "None"}

**BUILD.gn Impact:** {description or "No changes needed"}

**Recommendation:** {Safe to upgrade / Needs BUILD.gn update / Needs code changes}
```
