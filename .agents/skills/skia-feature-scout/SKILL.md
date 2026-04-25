---
name: skia-feature-scout
description: >
  Scout Google's Skia release notes for new features, APIs, and capabilities that SkiaSharp should
  expose. Fetches upstream RELEASE_NOTES.md, extracts notable items per milestone, and cross-references
  against SkiaSharp's C API shims and C# bindings to produce a gap analysis with prioritized
  recommendations. Use this skill whenever the user asks about "what's new in Skia", "what are we
  missing", "feature gap analysis", "new Skia APIs", "what should we bind next", "release notes
  scout", "upstream feature audit", "what cool stuff did Google add", or "check for new Skia
  features". Also use proactively when the user mentions a Skia milestone bump or asks what work
  a bump will unlock. Do NOT use the release-notes-audit skill for this — this skill provides an
  independent, complementary perspective focused on developer-facing value rather than API coverage
  counts.
---

# Skia Feature Scout

You are an analyst who reads Google's Skia release notes and identifies features that would add
value to SkiaSharp users. Your goal is to find the gems — new capabilities, formats, visual quality
improvements, and powerful APIs — and determine which ones SkiaSharp is missing.

## Why This Matters

SkiaSharp wraps Skia via a C API shim layer. When Google adds a major feature to Skia, it doesn't
automatically appear in SkiaSharp — someone needs to:
1. Notice the feature exists
2. Decide it's worth exposing
3. Create a C API wrapper in `externals/skia/include/c/` and `externals/skia/src/c/`
4. Generate P/Invoke bindings
5. Write the C# wrapper in `binding/SkiaSharp/`

This skill handles step 1 and 2 so the team can prioritize steps 3-5.

## Workflow

### Phase 1: Fetch Release Notes

Fetch the full Skia release notes from GitHub:

```
https://raw.githubusercontent.com/google/skia/main/RELEASE_NOTES.md
```

The file is large (100KB+). Fetch it in 20KB chunks using `web_fetch` with increasing `start_index`
until you've read the entire file. Continue fetching until you see milestone numbers in the 80s or
the content ends.

### Phase 2: Determine Current Milestone

Check what Skia milestone SkiaSharp is currently on:

```bash
cd externals/skia && git log --oneline -1
```

Also check the skia submodule's DEPS or version info. The user may also tell you directly.
Everything at or below this milestone is "available now". Everything above is "coming with bump".

### Phase 3: Extract Notable Features

Read each milestone section and extract items that match these criteria:

**Include (high signal):**
- Brand new classes or APIs (SkMesh, skhdr::Metadata, etc.)
- New codec/format support (AVIF, JPEG XL, animated WebP encoding)
- New color types or color space features
- Shader capabilities (CoordClamp, gradient interpolation spaces)
- Image filter additions (RuntimeShader, Crop with TileMode)
- Canvas/Surface enhancements that affect rendering quality
- Utility methods that make common tasks easier
- Significant API migrations (SkPath → SkPathBuilder)

**Exclude (noise):**
- Internal refactoring (moving headers between directories)
- GPU backend-specific changes (Graphite, Dawn, Vulkan internals)
- Build system changes (GN flags, defines)
- SkSL parser/compiler fixes (unless they unlock new capabilities)
- Thread safety or memory management internals
- Removals of already-deprecated APIs (unless SkiaSharp still uses them)

For each notable item, record:
- **Name**: Short descriptive name
- **Category**: new-feature | codec | image | shader | color | canvas | font | utility | deprecation
- **Milestone introduced**: When it first appeared
- **Milestone enhanced**: Later milestones that improved/extended it (comma-separated)
- **Milestone deprecated**: When it was deprecated (if applicable)
- **Milestone removed**: When it was removed (if applicable)
- **Description**: 2-3 sentence explanation of what it does and why it matters
- **Priority**: high | medium | low
  - **high**: Major new capability, popular format, or migration requirement
  - **medium**: Useful addition, quality improvement, or niche but valuable
  - **low**: Minor utility, internal concern, or auto-available

### Phase 4: Check SkiaSharp Bindings

For each extracted feature, check whether SkiaSharp already has it:

1. **C API layer** — Search `externals/skia/include/c/*.h` and `externals/skia/src/c/*.cpp`:
   ```bash
   grep -ril "keyword" externals/skia/include/c/ externals/skia/src/c/
   ```

2. **C# binding layer** — Search `binding/SkiaSharp/*.cs`:
   ```bash
   grep -ril "keyword" binding/SkiaSharp/
   ```

3. **Generated bindings** — Check `binding/SkiaSharp/SkiaApi.generated.cs` for P/Invoke entries.

Mark each feature as:
- ✅ **Bound** — Both C API and C# wrapper exist
- 🟡 **Partial** — C API exists but no C# wrapper (quick win!)
- ❌ **Missing** — Neither C API nor C# wrapper exist
- ⚪ **N/A** — Auto-available (e.g., SkSL intrinsics) or internal-only

Note: If the submodule isn't checked out, try the main repo at the repo root path.

### Phase 5: Generate Report

Produce a structured markdown report with these sections:

```markdown
# Skia Feature Scout Report
**Date:** YYYY-MM-DD
**Current SkiaSharp Milestone:** NNN
**Skia HEAD Milestone:** NNN

## Executive Summary
- Count of high/medium/low items found
- Top 3-5 most impactful missing features
- Any critical deprecation warnings

## 🔴 HIGH PRIORITY — Must Investigate
(Table with: Name, Milestone, Category, Bound?, Impact description)

## 🟡 MEDIUM PRIORITY — Should Investigate
(Table format)

## 🟢 ALREADY BOUND
(Brief table confirming what we have)

## ⚪ LOW PRIORITY
(Brief table)

## Quick Wins (C API exists, needs C# wrapper only)
(List any 🟡 Partial items — these are fastest to add)

## Timeline: Before vs After Current Milestone
- Features available now (≤ current milestone)
- Features coming with milestone bump (> current milestone)

## Deprecation Watch
- APIs being deprecated/removed that SkiaSharp may depend on
- Migration requirements

## Recommended Action Plan
- Ordered list of what to tackle first, considering effort and value
```

Save the report to the artifacts directory as `skia-feature-scout-report.md`.

### Phase 6: Offer Next Steps

After presenting the report, offer:
1. "Want me to investigate any of these features in more detail?"
2. "Should I create issues or todos for the high-priority items?"
3. "Want me to check if the C API needs to be created for any specific feature?"
4. "Should I set up a periodic workflow to re-run this audit?"

## Tips for Accurate Assessment

- **Don't confuse enum values with full support.** Having `AVIF` in an encoded format enum doesn't
  mean AVIF decoding is fully wired up in C#.
- **Check for the actual C# method, not just the class.** A class may exist but be missing specific
  overloads (e.g., DropShadow exists but only with SKColor, not SKColor4f).
- **Runtime effects children vs image filter children are different.** SKRuntimeEffect supporting
  children doesn't mean SkImageFilters::RuntimeShader is bound.
- **Path features need special attention.** SkPath immutability is a massive migration that affects
  the entire SkiaSharp path API surface.
- **Gradient interpolation is a high-value gap.** CSS Color Level 4 gradient interpolation produces
  dramatically better gradients. This is a visible quality improvement users will notice.
