# Feature Extraction Criteria

These criteria are used by the audit agents (Phase 2) and by the orchestrator during
synthesis (Phase 3). Agents: read this file before starting your audit.

## Include (high signal)

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

## Exclude (noise)

- Internal refactoring (moving headers between directories)
- GPU backend-specific changes (Graphite, Dawn, Vulkan internals) unless they affect Ganesh/GL/Metal
- Build system changes (GN flags, defines)
- SkSL parser/compiler fixes (unless they unlock new capabilities)
- Thread safety or memory management internals
- Removals of already-deprecated APIs (unless SkiaSharp still uses them)

## Output Fields

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
| **userValue** | One sentence: why an app developer would want this |
| **priority** | `critical`, `high`, `medium`, or `low` |

## Categories

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

## Priority Classification

| Priority | Criteria |
|----------|----------|
| `critical` | Will cause compile/link/runtime failures on next Skia bump. Migration required. |
| `high` | Major new capability, popular format, or highly requested feature |
| `medium` | Useful addition, quality improvement, or niche but valuable |
| `low` | Minor utility, internal concern, or auto-available |

## Binding Status Classification

When checking SkiaSharp bindings, classify each feature as:

| Status | Badge | Description |
|--------|-------|-------------|
| `full` | ✅ | C API + C# wrapper both exist and cover the feature |
| `partial` | 🟡 | C API exists but C# wrapper is missing (**quick win!**) |
| `missing` | ❌ | Neither C API nor C# wrapper exist |
| `correctly_absent` | 🚫 | Skia removed this and SkiaSharp correctly never wrapped it |
| `action_needed` | ⚠️ | SkiaSharp wraps something Skia deprecated/removed without `[Obsolete]` |
| `not_applicable` | ⚪ | Doesn't need a binding (internal, auto-available, Graphite-only) |

## C# → C API → C++ Type Mapping

These are the core types to check for hidden APIs:

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
