# Skia Analyst — Analysis Instructions

Read this before starting any analysis. It covers both lenses — changelog (what shipped) and gap analysis (what's missing) — because every finding needs both.

## Input Flexibility

The user may specify:
- **Nothing** → full scan (all milestones, everything)
- **One milestone** → "what's new since m133" (windowed from that milestone to current)
- **Two milestones** → "between m133 and m147" (windowed range)
- **One git ref** → "what changed since v3.119.4" (diff from ref to HEAD)
- **Two git refs** → "diff v3.119.4..origin/main" (explicit range)
- **Vague** → "scan for new stuff" (full scan)

Infer the scan mode from context. If unclear, ask.

## Data Sources

### 1. Skia Release Notes
Fetch from: `https://raw.githubusercontent.com/google/skia/main/RELEASE_NOTES.md`
Extract features per milestone. For windowed/diff mode, focus on the relevant range but still scan a few milestones beyond.

### 2. C++ Header Scan (Hidden API Discovery)
For types SkiaSharp already binds, fetch upstream C++ headers from `google/skia` and compare
against what SkiaSharp exposes:
- **Upstream C++ headers**: fetch from GitHub (`owner=google, repo=skia, path=include/core/SkImage.h`)
- **SkiaSharp C API**: grep `binding/SkiaSharp/SkiaApi.generated.cs` for `sk_*` / `gr_*` P/Invoke externs.
  This reflects the full C API surface even if `externals/skia/include/c/` isn't checked out.
- **SkiaSharp C# wrappers**: `binding/SkiaSharp/*.cs`

High-value headers to scan:
| C# | C API prefix | C++ header |
|----|-------------|------------|
| `SKCanvas` | `sk_canvas_*` | `include/core/SkCanvas.h` |
| `SKImage` | `sk_image_*` | `include/core/SkImage.h` |
| `SKPath` | `sk_path_*` | `include/core/SkPath.h` |
| `SKPathBuilder` | `sk_pathbuilder_*` | `include/core/SkPathBuilder.h` |
| `SKShader` | `sk_shader_*` | `include/core/SkShader.h` |
| `SKImageFilter` | `sk_imagefilter_*` | `include/effects/SkImageFilters.h` |
| `SKColorFilter` | `sk_colorfilter_*` | `include/core/SkColorFilter.h` |
| `SKCodec` | `sk_codec_*` | `include/codec/SkCodec.h` |
| `SKFont` | `sk_font_*` | `include/core/SkFont.h` |
| `SKTypeface` | `sk_typeface_*` | `include/core/SkTypeface.h` |
| `SKSurface` | `sk_surface_*` | `include/core/SkSurface.h` |
| `SKColorSpace` | `sk_colorspace_*` | `include/core/SkColorSpace.h` |

### 3. Git Diff (when refs provided)
- C API diff: `git diff REF1..REF2 -- externals/skia/include/c/ externals/skia/src/c/`
- C# binding diff: `git diff REF1..REF2 -- binding/ ':!*.generated.cs'`
- Git log: `git log REF1..REF2 --oneline`
- Build/deps: `git diff REF1..REF2 -- native/ externals/skia/DEPS cgmanifest.json`

## Classification: Both Lenses

Every finding needs BOTH a changelog classification AND a gap classification.

### Changelog lens (what happened)

**changeType:**
| Value | Criteria |
|-------|----------|
| `added` | New API, class, method, capability |
| `changed` | Modified behavior or signature |
| `fixed` | Bug fix |
| `deprecated` | New `[Obsolete]` marker |
| `removed` | Removed API or feature |
| `dependency` | Dependency version bump |
| `platform` | New platform, build flag change |
| `upstream` | Benefit from Skia engine upgrade (automatic, no API change) |

**importance:**
| Value | Criteria |
|-------|----------|
| `breaking` | Existing code may fail to compile or behave differently |
| `major` | New user-facing capability, significant feature |
| `minor` | Enhancement, quality improvement |
| `patch` | Bug fix, internal improvement |

### Gap lens (what to do about it)

**bindingStatus:**
| Value | Description |
|-------|-------------|
| `full` | C API + C# wrapper both exist |
| `partial` | C API exists but C# wrapper missing (quick win) |
| `missing` | Neither C API nor C# wrapper |
| `action_needed` | SkiaSharp wraps deprecated/removed Skia API |
| `correctly_absent` | Skia removed; SkiaSharp never wrapped |
| `not_applicable` | Doesn't need a binding (internal, auto-available) |

**impact:**
| Value | Decision guide |
|-------|---------------|
| `transformative` | "Does this unlock a new *category* of app?" |
| `significant` | "Would a user *see* the difference or gain a major capability?" |
| `moderate` | "Is this useful but not exciting?" |
| `minor` | Default for small helpers |

**priority:**
| Value | Criteria |
|-------|----------|
| `critical` | Will break on next Skia bump |
| `high` | Major new capability or highly requested |
| `medium` | Useful addition, quality improvement |
| `low` | Minor utility, internal concern |

## What to Include vs Exclude

**Include:**
- New classes, APIs, capabilities
- Codec/format support (AVIF, JPEG XL, HDR)
- Shader capabilities, image filter additions
- Color types, color spaces
- Performance improvements
- Behavior changes
- Deprecations and removals
- Security hardening, dependency updates
- Platform expansion

**Exclude:**
- Internal refactoring (header moves)
- Graphite-only changes (SkiaSharp uses Ganesh)
- Dawn backend changes
- Build system internals (GN flags)
- Version bump commits
- CI/docs/skill-only changes

## Upstream Benefits (changeType: "upstream")

When a Skia bump is in the diff, extract "invisible benefits" — things users get automatically:
- Rendering quality (mipmap sharpening, noise rotation, backdrop tiling)
- Performance (noise raster speedup, canvas preallocation)
- Color accuracy (kRec709 BT.1886, kHLG/kPQ corrections)
- Codec improvements (Exif orientation, high bit-depth detection)
- Reliability (Vulkan device-lost, SkSL validation)

For upstream findings: `bindingStatus` = `not_applicable`, `impact` and `priority` based on user value.

## Optional Enrichment Fields

For findings with `importance` of `major` or higher, include a `slideBullet` — a one-line marketing
bullet for downstream tooling (e.g., blog post drafts). Not rendered in the Markdown report but
captured in the JSON for other tools to use.

For `importance: "breaking"` findings, include a `migrationGuide` with before/after code.

For `changeType: "dependency"` findings, include `dependencyName`, `dependencyFrom`, `dependencyTo`.

## Accuracy Tips

- Don't confuse enum values with full support
- Check the actual C# method, not just the class
- Verify C# wrappers call the right C API
- Check SkiaApi.generated.cs for hidden plumbing
- milestone fields must be integers (133 not "m133")
