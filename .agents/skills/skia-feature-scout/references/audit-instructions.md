# Skia Feature Scout — Agent Audit Instructions

Read this entire file before starting your audit. It contains everything you need: what to look
for, how to check bindings, how to scan C++ headers, and accuracy tips learned from prior runs.

---

## Part 1: Feature Extraction Criteria

### Include (high signal)

- Brand new classes or APIs (SkMesh, skhdr::Metadata, SkAnimatedImage, etc.)
- New codec/format support (AVIF, JPEG XL, animated WebP encoding)
- New color types or color space features (SkColorInfo, reinterpretColorSpace, etc.)
- Shader capabilities (CoordClamp, gradient interpolation spaces)
- Image filter additions (RuntimeShader, Crop with TileMode)
- Canvas/Surface enhancements that affect rendering quality
- Utility methods that make common tasks easier
- Significant API migrations (SkPath → SkPathBuilder)
- **Performance improvements** — rendering speedups, memory reductions, decode optimizations
- **Behavior changes** — existing APIs that silently changed semantics
- **Codec introspection** — SelectionPolicy, getICCProfile, isAnimated, hasHighBitDepthEncodedData
- **GPU interop** — async rescale/readback, MSAA resolve, anisotropic filtering
- **Text APIs** — SkTextBlob::Iter, palette overrides, font argument extensions
- **Sampling options extensions** — anisotropic max level, new filter modes

### Exclude (noise)

- Internal refactoring (moving headers between directories)
- GPU backend-specific changes (Graphite, Dawn, Vulkan internals) unless they affect Ganesh/GL/Metal
- Build system changes (GN flags, defines)
- SkSL parser/compiler fixes (unless they unlock new capabilities)
- Thread safety or memory management internals
- Removals of already-deprecated APIs (unless SkiaSharp still uses them)

### Output Fields per Feature

| Field | Description |
|-------|-------------|
| **name** | Short descriptive name |
| **category** | See categories table below |
| **milestone** | When it first appeared |
| **milestones** | Later milestones that improved/extended it (comma-separated) |
| **milestoneDeprecated** | When it was deprecated (if applicable) |
| **milestoneRemoved** | When it was removed (if applicable) |
| **skiaApi** | The upstream Skia API name(s) |
| **description** | 2-3 sentences: what it does AND why it matters to SkiaSharp users |
| **userValue** | One sentence: why an app developer would want this |
| **priority** | `critical`, `high`, `medium`, or `low` |

### Categories

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

### Priority Classification

| Priority | Criteria |
|----------|----------|
| `critical` | Will cause compile/link/runtime failures on next Skia bump. Migration required. |
| `high` | Major new capability, popular format, or highly requested feature |
| `medium` | Useful addition, quality improvement, or niche but valuable |
| `low` | Minor utility, internal concern, or auto-available |

---

## Part 2: Binding Verification

For each feature you extract from the release notes, check whether SkiaSharp already has it.

### How to Check

1. **C API layer** — Search `externals/skia/include/c/*.h` and `externals/skia/src/c/*.cpp`:
   ```bash
   grep -ril "keyword" externals/skia/include/c/ externals/skia/src/c/
   ```

2. **C# binding layer** — Search `binding/SkiaSharp/*.cs`:
   ```bash
   grep -ril "keyword" binding/SkiaSharp/
   ```

3. **Generated bindings** — Check `binding/SkiaSharp/SkiaApi.generated.cs` for P/Invoke entries
   and internal struct fields that the public C# option types may not expose (especially encoder
   options: ICC, XMP, gainmap, HDR metadata fields).

If the submodule isn't checked out in this worktree, try the main repo checkout or use GitHub API:
```bash
# Main repo fallback
ls /path/to/main/SkiaSharp/externals/skia/include/c/
# Or GitHub API
github-mcp-server-get_file_contents owner=mono repo=skia path=include/c/sk_image.h
```

### Binding Status Classification

| Status | Badge | Description |
|--------|-------|-------------|
| `full` | ✅ | C API + C# wrapper both exist and cover the feature |
| `partial` | 🟡 | C API exists but C# wrapper is missing (**quick win!**) |
| `missing` | ❌ | Neither C API nor C# wrapper exist |
| `correctly_absent` | 🚫 | Skia removed this and SkiaSharp correctly never wrapped it |
| `action_needed` | ⚠️ | SkiaSharp wraps something Skia deprecated/removed without `[Obsolete]` |
| `not_applicable` | ⚪ | Doesn't need a binding (internal, auto-available, Graphite-only) |

### Deprecation Checking

Also look for SkiaSharp APIs that wrap things Skia has deprecated or removed. For each, record:
- The exact SkiaSharp API name and file
- The Skia milestone when deprecated
- The replacement API
- A suggested `[Obsolete("...")]` message

---

## Part 3: Hidden API Discovery (C++ Header Scan)

For types that SkiaSharp already binds, check the upstream C++ headers for new methods that were
added without a release notes mention.

### How it works

1. For each type in the mapping table below, fetch the upstream header from Google's Skia repo:
   ```
   https://raw.githubusercontent.com/google/skia/main/include/core/SkImage.h
   ```
   Also check `include/effects/` for filter/shader types.

2. Extract public method signatures from the C++ header.

3. Compare against the C API functions in `externals/skia/include/c/sk_image.h` (our fork).

4. Any public C++ method that has no corresponding C API function is a **hidden gap**. Evaluate
   whether it's worth binding based on user value.

5. Skip internal/friend/protected methods and anything Graphite-specific.

### Focus on these high-value headers first

(Most likely to have hidden gems)
- `SkImage.h` — new factory methods, transformations
- `SkCanvas.h` — new draw methods
- `SkImageFilters.h` — new filter factories
- `SkShader.h` / `SkShaders` — new shader factories
- `SkCodec.h` — new decode capabilities
- `SkColorFilter.h` — new filter types
- `SkPath.h` + `SkPathBuilder.h` — path construction changes
- `SkBitmap.h` — pixel manipulation
- `SkTextBlob.h` — text blob iteration and construction
- `SkBlendMode.h` — blend mode queries (e.g., AsCoeff)

### C# → C API → C++ Type Mapping

| C# | C API prefix | C++ header |
|----|-------------|------------|
| `SKCanvas` | `sk_canvas_*` | `include/core/SkCanvas.h` |
| `SKImage` | `sk_image_*` | `include/core/SkImage.h` |
| `SKPaint` | `sk_paint_*` | `include/core/SkPaint.h` |
| `SKPath` | `sk_path_*` | `include/core/SkPath.h` |
| `SKShader` | `sk_shader_*` | `include/core/SkShader.h` |
| `SKColorFilter` | `sk_colorfilter_*` | `include/core/SkColorFilter.h` |
| `SKImageFilter` | `sk_imagefilter_*` | `include/effects/SkImageFilters.h` |
| `SKCodec` | `sk_codec_*` | `include/codec/SkCodec.h` |
| `SKBitmap` | `sk_bitmap_*` | `include/core/SkBitmap.h` |
| `SKPixmap` | `sk_pixmap_*` | `include/core/SkPixmap.h` |
| `SKSurface` | `sk_surface_*` | `include/core/SkSurface.h` |
| `SKFont` | `sk_font_*` | `include/core/SkFont.h` |
| `SKTypeface` | `sk_typeface_*` | `include/core/SkTypeface.h` |
| `SKData` | `sk_data_*` | `include/core/SkData.h` |
| `SKColorSpace` | `sk_colorspace_*` | `include/core/SkColorSpace.h` |
| `SKTextBlob` | `sk_textblob_*` | `include/core/SkTextBlob.h` |
| *(not bound)* | — | `include/core/SkPathBuilder.h` |
| *(not bound)* | — | `include/core/SkBlendMode.h` |

### Hidden API Output Format

For each hidden gap, output:
```json
{ "cppClass": "SkImage", "cppHeader": "include/core/SkImage.h",
  "cppMethod": "reinterpretColorSpace(sk_sp<SkColorSpace>)",
  "description": "Reinterpret image as different color space without conversion",
  "cApiStatus": "missing", "csharpStatus": "missing",
  "priority": "medium", "notes": "Useful for metadata-driven image workflows" }
```

---

## Part 4: Tips for Accurate Assessment

These tips come from lessons learned in prior audit runs. Ignoring them leads to false positives
and missed findings.

- **Don't confuse enum values with full support.** Having `AVIF` in an encoded format enum doesn't
  mean AVIF decoding is fully wired up in C#.
- **Check for the actual C# method, not just the class.** A class may exist but be missing specific
  overloads (e.g., DropShadow exists but only with SKColor, not SKColor4f).
- **Verify C# wrappers actually call the right C API.** A wrapper may exist with the right name but
  forward to the wrong native function. For example, check that `ToRawShader` actually calls a raw
  shader C API and not the regular `makeShader`. **Read the implementation, not just the signature.**
- **Check SkiaApi.generated.cs for hidden plumbing.** The generated interop file may contain fields
  (e.g., gainmap, ICC profile, XMP metadata) in internal structs that the public C# option types
  don't expose. These are quick wins — the native plumbing exists, just needs a public wrapper.
- **Runtime effects children vs image filter children are different.** SKRuntimeEffect supporting
  children doesn't mean SkImageFilters::RuntimeShader is bound.
- **Path features need special attention.** SkPath immutability is a massive migration that affects
  the entire SkiaSharp path API surface.
- **Gradient interpolation is a high-value gap.** CSS Color Level 4 gradient interpolation produces
  dramatically better gradients. This is a visible quality improvement users will notice.
- **Track API churn across milestones.** Some APIs are added then removed (e.g., ICC profile fields
  in encoder options were added in M108 then removed in M142). Flag these lifecycle issues.
- **Performance notes matter.** A Perlin noise speedup or decode optimization benefits users without
  any binding changes needed — but they should know about it.
- **Behavior changes can cause subtle bugs.** If Skia changed how `kRec709` transfer function works,
  apps may see color shifts. Flag these even if no binding change is required.
- **The mono/skia fork may retain deprecated APIs** that upstream removed. This isn't a bug — it's
  intentional for backward compatibility. Flag it but don't classify as broken.
- **C++ headers are the source of truth.** Release notes are curated highlights. The headers contain
  everything. When in doubt, check the header.
- **Don't skip very old milestones.** Features from M78-M90 like SkTextBlob::Iter, SkBlendMode_AsCoeff,
  SkColorInfo, and SkImage::reinterpretColorSpace are easily overlooked but still valuable.
