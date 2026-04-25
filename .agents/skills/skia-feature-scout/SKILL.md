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
improvements, performance wins, and powerful APIs — and determine which ones SkiaSharp is missing.

You also go beyond the release notes: for types SkiaSharp already binds, you inspect the upstream
C++ headers to find new methods or overloads that Google added quietly without mentioning in
release notes.

## Why This Matters

SkiaSharp wraps Skia via a C API shim layer. When Google adds a major feature to Skia, it doesn't
automatically appear in SkiaSharp — someone needs to:
1. Notice the feature exists
2. Decide it's worth exposing
3. Create a C API wrapper in `externals/skia/include/c/` and `externals/skia/src/c/`
4. Generate P/Invoke bindings
5. Write the C# wrapper in `binding/SkiaSharp/`

This skill handles step 1 and 2 so the team can prioritize steps 3-5.

## Modes of Operation

The skill supports two modes. The user may specify one or you can suggest the right one:

- **Full scan** (default): Reads ALL milestones from the release notes and the full history of
  C++ headers. Best for initial discovery or periodic strategic audits.
- **Windowed scan**: Reads only a milestone range (e.g., m119→m133). Best for auditing a specific
  Skia bump. Ask the user for the previous and current milestones, or detect them.

## Workflow

### Phase 1: Fetch Release Notes

Fetch the full Skia release notes from upstream:

```
https://raw.githubusercontent.com/google/skia/main/RELEASE_NOTES.md
```

The file is large (100KB+). Fetch it in 20KB chunks using `web_fetch` with increasing `start_index`
until you've read the entire file. Continue fetching until you see milestone numbers in the 80s or
the content ends.

In **windowed mode**, you can stop once you've read past the target milestone range, but always
read at least a few milestones beyond the current one to catch future items.

### Phase 2: Determine Current Milestone

Check what Skia milestone SkiaSharp is currently on:

```bash
cd externals/skia && git log --oneline -1
```

Also check `externals/skia/include/core/SkMilestone.h` if available. The user may also tell you
directly. Everything at or below this milestone is "available now". Everything above is "coming
with bump".

### Phase 3: Extract Notable Features

Read each milestone section and extract items that match these criteria.

**Include (high signal):**
- Brand new classes or APIs (SkMesh, skhdr::Metadata, etc.)
- New codec/format support (AVIF, JPEG XL, animated WebP encoding)
- New color types or color space features
- Shader capabilities (CoordClamp, gradient interpolation spaces)
- Image filter additions (RuntimeShader, Crop with TileMode)
- Canvas/Surface enhancements that affect rendering quality
- Utility methods that make common tasks easier
- Significant API migrations (SkPath → SkPathBuilder)
- **Performance improvements** — rendering speedups, memory reductions, decode optimizations
- **Behavior changes** — existing APIs that silently changed semantics

**Exclude (noise):**
- Internal refactoring (moving headers between directories)
- GPU backend-specific changes (Graphite, Dawn, Vulkan internals) unless they affect Ganesh/GL/Metal
- Build system changes (GN flags, defines)
- SkSL parser/compiler fixes (unless they unlock new capabilities)
- Thread safety or memory management internals
- Removals of already-deprecated APIs (unless SkiaSharp still uses them)

For each notable item, record:

| Field | Description |
|-------|-------------|
| **name** | Short descriptive name |
| **category** | See categories below |
| **milestone_introduced** | When it first appeared |
| **milestone_enhanced** | Later milestones that improved/extended it (comma-separated) |
| **milestone_deprecated** | When it was deprecated (if applicable) |
| **milestone_removed** | When it was removed (if applicable) |
| **description** | 2-3 sentences: what it does AND why it matters to SkiaSharp users |
| **priority** | `critical`, `high`, `medium`, or `low` |

#### Categories

| Category | What belongs here |
|----------|-------------------|
| `new_feature` | Brand new class, API surface, or capability |
| `codec` | Image format encode/decode support |
| `image` | SkImage methods, factories, transformations |
| `image_filter` | SkImageFilter factories and enhancements |
| `shader` | Shader factories, gradient features, noise |
| `color` | Color types, color spaces, color filters |
| `canvas` | SkCanvas/SkSurface methods and flags |
| `path` | SkPath, SkPathBuilder, path effects |
| `font` | Font/typeface features |
| `utility` | Small helpers, data types, convenience APIs |
| `performance` | Speed improvements, memory optimizations, decode/encode perf |
| `behavior_change` | Existing API changed semantics silently |
| `deprecation` | API deprecated or removed |

#### Priority Classification

| Priority | Criteria |
|----------|----------|
| `critical` | Will cause compile/link/runtime failures on next Skia bump. Migration required. |
| `high` | Major new capability, popular format, or highly requested feature |
| `medium` | Useful addition, quality improvement, or niche but valuable |
| `low` | Minor utility, internal concern, or auto-available |

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

If the submodule isn't checked out in this worktree, try the main repo checkout or use GitHub API:
```bash
# Main repo fallback
ls /path/to/main/SkiaSharp/externals/skia/include/c/
# Or GitHub API
github-mcp-server-get_file_contents owner=mono repo=skia path=include/c/sk_image.h
```

#### Binding Status Classification

| Status | Badge | Description |
|--------|-------|-------------|
| `full` | ✅ | C API + C# wrapper both exist and cover the feature |
| `partial` | 🟡 | C API exists but C# wrapper is missing (**quick win!**) |
| `missing` | ❌ | Neither C API nor C# wrapper exist |
| `correctly_absent` | 🚫 | Skia removed this and SkiaSharp correctly never wrapped it |
| `action_needed` | ⚠️ | SkiaSharp wraps something Skia deprecated/removed without `[Obsolete]` |
| `not_applicable` | ⚪ | Doesn't need a binding (internal, auto-available, Graphite-only) |

### Phase 5: Hidden API Discovery (C++ Header Scan)

This is the secret weapon phase. For types that SkiaSharp already binds, check the upstream C++
headers for new methods that were added without a release notes mention.

**How it works:**

1. Identify the core Skia types SkiaSharp wraps. These map from C# → C API → C++ like:
   - `SKCanvas` → `sk_canvas_*` → `SkCanvas` in `include/core/SkCanvas.h`
   - `SKImage` → `sk_image_*` → `SkImage` in `include/core/SkImage.h`
   - `SKPaint` → `sk_paint_*` → `SkPaint` in `include/core/SkPaint.h`
   - `SKPath` → `sk_path_*` → `SkPath` in `include/core/SkPath.h`
   - `SKShader` → `sk_shader_*` → `SkShader` / `SkShaders` in `include/core/SkShader.h`
   - `SKColorFilter` → `sk_colorfilter_*` → `SkColorFilter` in `include/core/SkColorFilter.h`
   - `SKImageFilter` → `sk_imagefilter_*` → `SkImageFilters` in `include/effects/SkImageFilters.h`
   - `SKCodec` → `sk_codec_*` → `SkCodec` in `include/codec/SkCodec.h`
   - `SKBitmap` → `sk_bitmap_*` → `SkBitmap` in `include/core/SkBitmap.h`
   - `SKPixmap` → `sk_pixmap_*` → `SkPixmap` in `include/core/SkPixmap.h`
   - `SKSurface` → `sk_surface_*` → `SkSurface` / `SkSurfaces` in `include/core/SkSurface.h`
   - `SKFont` → `sk_font_*` → `SkFont` in `include/core/SkFont.h`
   - `SKTypeface` → `sk_typeface_*` → `SkTypeface` in `include/core/SkTypeface.h`
   - `SKData` → `sk_data_*` → `SkData` in `include/core/SkData.h`
   - `SKColorSpace` → `sk_colorspace_*` → `SkColorSpace` in `include/core/SkColorSpace.h`

2. For each type, fetch the upstream C++ header from Google's Skia repo:
   ```
   https://raw.githubusercontent.com/google/skia/main/include/core/SkImage.h
   ```
   Also check `include/effects/` for filter/shader types.

3. Extract public method signatures from the C++ header.

4. Compare against the C API functions in `externals/skia/include/c/sk_image.h` (our fork).

5. Any public C++ method that has no corresponding C API function is a **hidden gap**. Evaluate
   whether it's worth binding based on user value.

**Focus on these high-value headers first** (most likely to have hidden gems):
- `SkImage.h` — new factory methods, transformations
- `SkCanvas.h` — new draw methods
- `SkImageFilters.h` — new filter factories
- `SkShader.h` / `SkShaders` — new shader factories
- `SkCodec.h` — new decode capabilities
- `SkColorFilter.h` — new filter types
- `SkPath.h` + `SkPathBuilder.h` — path construction changes

Use background `task` agents to parallelize this — launch one per header file.

### Phase 6: Cross-Validation

For accuracy, use a `task` agent with a different model to independently verify the top findings:

```
task agent_type=general-purpose:
  "Verify these SkiaSharp binding findings. For each item, confirm:
   1. The C API status (search externals/skia/include/c/ and src/c/)
   2. The C# binding status (search binding/SkiaSharp/)
   Report any disagreements."
```

At minimum, validate:
- All items marked `full` (confirm they actually exist)
- All items marked `action_needed` (confirm the deprecation)
- All items marked `missing` that are `high` or `critical` priority

### Phase 7: Generate Structured JSON Report

Produce a JSON report following the schema in [references/report-schema.md](references/report-schema.md).
Save to the artifacts directory as `skia-feature-scout-YYYY-MM-DD.json`.

The JSON must include:
- `meta` — audit metadata (date, milestones, source)
- `summary` — counts by status and priority
- `items` — every cataloged feature with full binding details
- `deprecations` — APIs needing `[Obsolete]` markers, with:
  - The exact SkiaSharp API name and file
  - The Skia milestone when deprecated
  - The replacement API
  - A suggested `[Obsolete("...")]` message
- `hiddenApis` — features discovered via C++ header scan (not in release notes)
- `performance` — performance-related changes worth noting
- `nextSteps` — prioritized action items, each with:
  - `skillToUse` — which SkiaSharp skill to invoke (`add-api`, `issue-fix`, `update-skia`, etc.)
  - `effort` — estimated effort (`trivial`, `small`, `medium`, `large`)

### Phase 8: Render HTML Report

Run the render script to produce a self-contained HTML dashboard:

```bash
python3 .agents/skills/skia-feature-scout/scripts/render-feature-scout.py \
  <path-to-json>
```

This produces a filterable, interactive HTML report. Present the output path to the user.

### Phase 9: Generate Markdown Summary

Present a concise markdown summary in the conversation. Group items by urgency:

1. 🔴 **Critical** — Will break on next Skia bump
2. ⚠️ **Action Needed** — Deprecated APIs missing `[Obsolete]` markers
3. ❌ **Missing (High)** — Major features with no binding
4. 🔶 **Missing (Medium)** — Useful features to plan for
5. 🟢 **Full** — Features already bound
6. 🟡 **Quick Wins** — C API exists, just needs C# wrapper
7. 🆕 **Hidden APIs** — Discovered via C++ scan, not in release notes
8. ⚡ **Performance** — Speed/memory improvements
9. 🔄 **Behavior Changes** — Silent semantic changes

Include sections for:
- **Before/After Current Milestone** — what's workable today vs needs a bump
- **Deprecation Watch** — with concrete `[Obsolete]` messages
- **Recommended Action Plan** — ordered by priority, with skill routing

### Phase 10: Offer Next Steps

After presenting the report, offer:
1. "Want me to investigate any of these features in more detail?"
2. "Should I create issues or todos for the high-priority items?"
3. "Want me to use the `add-api` skill to start binding a specific feature?"
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
- **Performance notes matter.** A Perlin noise speedup or decode optimization benefits users without
  any binding changes needed — but they should know about it.
- **Behavior changes can cause subtle bugs.** If Skia changed how `kRec709` transfer function works,
  apps may see color shifts. Flag these even if no binding change is required.
- **The mono/skia fork may retain deprecated APIs** that upstream removed. This isn't a bug — it's
  intentional for backward compatibility. Flag it but don't classify as broken.
- **C++ headers are the source of truth.** Release notes are curated highlights. The headers contain
  everything. When in doubt, check the header.
