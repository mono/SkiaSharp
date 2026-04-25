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

The skill uses a **parallel fan-out / synthesize / verify** architecture. Different agents
specialize in different aspects of the audit and run concurrently, then you merge their findings,
validate for accuracy, and produce the final report. This approach catches items that any single
pass would miss — testing shows individual model runs miss 15-30% of findings that other runs
catch.

```
Phase 1: Setup (fetch notes, determine milestone)
Phase 2: Fan-Out — launch 4 parallel agents
  ├─ Agent A: Recent milestones (current → M110)
  ├─ Agent B: Older milestones (M109 → M78)
  ├─ Agent C: C++ header scan for hidden APIs
  └─ Agent D: Binding inventory (what we already have)
Phase 3: Synthesize — merge all agent findings, dedupe, resolve conflicts
Phase 4: Verify — spot-check high-priority items for hallucinations
Phase 5: Generate outputs (JSON → validate → HTML → markdown)
```

### Phase 1: Setup

**1a. Fetch Release Notes**

Fetch the full Skia release notes from upstream:

```
https://raw.githubusercontent.com/google/skia/main/RELEASE_NOTES.md
```

The file is large (100KB+). Fetch it in 20KB chunks using `web_fetch` with increasing `start_index`
until you've read the entire file. Save to a temp file so agents can read it:

```bash
# Save fetched content to a file agents can access
cat > /tmp/skia-release-notes.md << 'NOTES'
<paste all fetched content>
NOTES
```

In **windowed mode**, you can stop once you've read past the target milestone range, but always
read at least a few milestones beyond the current one to catch future items.

**1b. Determine Current Milestone**

Check what Skia milestone SkiaSharp is currently on:

```bash
cd externals/skia && git log --oneline -1
# Also check:
cat externals/skia/include/core/SkMilestone.h 2>/dev/null
```

The user may also tell you directly. Everything at or below this milestone is "available now".
Everything above is "coming with bump".

**1c. Locate C API headers**

The skia submodule may not be checked out in this worktree. Find the C API:

```bash
# Try worktree first
ls externals/skia/include/c/ 2>/dev/null
# Fallback to main repo checkout
ls /path/to/main/SkiaSharp/externals/skia/include/c/ 2>/dev/null
```

Record the path for agents to use.

### Phase 2: Fan-Out — Launch Parallel Agents

Launch **four** background `task` agents simultaneously. Each has a focused job and produces
a JSON fragment. Give each agent the release notes file path, C API path, and current milestone.

**Agent A: Recent Release Notes (current milestone → M110)**

```
task agent_type=explore mode=background name=scout-recent:
  "Read the Skia release notes at /tmp/skia-release-notes.md, focusing on
   milestones M110 through M{current}+15 (to catch future items).
   
   For each milestone, extract notable features matching these criteria:
   [paste the Include/Exclude criteria and category/priority tables from this skill]
   
   For each item, also check if SkiaSharp has it:
   - Search binding/SkiaSharp/*.cs for C# wrappers
   - Search {c_api_path} for C API functions
   
   Output a JSON array of feature objects. Each must have:
   id, name, category, milestoneIntroduced, milestoneEnhanced, milestoneDeprecated,
   milestoneRemoved, skiaApi, description, userValue, cApiStatus, cApiFunction,
   cApiFile, csharpStatus, csharpMethod, csharpFile, bindingStatus, priority, notes.
   
   Be thorough. Check EVERY milestone in your range. Don't skip any."
```

**Agent B: Older Release Notes (M109 → M78)**

Same prompt as Agent A but for the older milestone range. This agent exists because testing
showed that models lose rigor when processing very long documents — splitting the range ensures
old milestones (where gems like SkTextBlob::Iter M79, SkBlendMode_AsCoeff M80, SkColorInfo M79,
SkImage::reinterpretColorSpace M78 hide) get equal attention.

**Agent C: C++ Header Scan**

```
task agent_type=explore mode=background name=scout-headers:
  "Scan upstream Skia C++ headers for public methods not exposed in SkiaSharp's C API.
   
   For each of these types, fetch the upstream header from Google's Skia repo and
   compare against the C API in {c_api_path}:
   
   Priority 1 (most likely to have gems):
   - SkImage (include/core/SkImage.h) vs sk_image.h
   - SkCanvas (include/core/SkCanvas.h) vs sk_canvas.h
   - SkImageFilters (include/effects/SkImageFilters.h) vs sk_imagefilter.h
   - SkCodec (include/codec/SkCodec.h) vs sk_codec.h
   - SkShader/SkShaders (include/core/SkShader.h) vs sk_shader.h
   - SkColorFilter (include/core/SkColorFilter.h) vs sk_colorfilter.h
   - SkPath + SkPathBuilder (include/core/SkPath.h, SkPathBuilder.h) vs sk_path.h
   
   Priority 2:
   - SkBitmap, SkPixmap, SkSurface, SkFont, SkTypeface, SkData, SkColorSpace
   - SkTextBlob (include/core/SkTextBlob.h) vs any sk_textblob in C API
   - SkBlendMode (include/core/SkBlendMode.h)
   
   For each public C++ method that has NO corresponding C API function, output:
   { cppClass, cppHeader, cppMethod, description, cApiStatus, csharpStatus, priority, notes }
   
   Focus on methods that would add user value. Skip internal/friend/protected methods."
```

**Agent D: Binding Inventory**

```
task agent_type=explore mode=background name=scout-bindings:
  "Inventory the current SkiaSharp bindings to establish what we already have.
   
   1. List all C API functions from {c_api_path}/*.h (grep for 'SK_C_API')
   2. List all public classes and key methods from binding/SkiaSharp/*.cs
   3. Check SkiaApi.generated.cs for internal struct fields that may have hidden
      plumbing not exposed in public option types (especially encoder options like
      SKJpegEncoderOptions, SKPngEncoderOptions, SKWebpEncoderOptions — look for
      ICC, XMP, gainmap, HDR metadata fields in the generated code vs public structs)
   4. For any method marked 'Raw' or 'raw' in the name, verify it actually calls
      the correct underlying C API (not a regular version of the same method)
   
   Output:
   - A list of C API function names (one per line)
   - A list of {csharpClass, csharpMethod, cApiFunction} for key bindings
   - Any discrepancies found (wrong C API calls, hidden generated fields, etc.)"
```

### Phase 3: Synthesize

When all four agents complete, merge their findings:

1. **Collect** all feature items from Agents A and B into a single list
2. **Deduplicate** — if both agents found the same feature (by skiaApi name), merge them,
   keeping the richer description and noting both milestone ranges
3. **Add hidden APIs** from Agent C as a separate `hiddenApis` array
4. **Cross-reference with Agent D's inventory** to verify binding statuses:
   - If Agent D says a C API function exists but Agents A/B said `missing`, fix to `partial`
   - If Agent D found implementation bugs (wrong C API calls), mark as `action_needed`
   - If Agent D found hidden generated fields, create items for those as quick wins
5. **Track feature lifecycles** — for items that appear in multiple milestones (introduced,
   enhanced, deprecated, removed), merge into a single item with all milestone fields populated
6. **Extract performance notes** into the `performance` array
7. **Extract deprecations** into the `deprecations` array with concrete `[Obsolete]` messages
8. **Build the `nextSteps`** array, ordered by priority, with `skillToUse` and `effort`

### Phase 4: Verify High-Priority Findings

For every item marked `critical` or `high` priority, and every item marked `full` (to confirm
they're not false positives), do a direct verification:

```bash
# For each high-priority "missing" item — confirm it's truly missing
grep -ril "function_name_or_keyword" binding/SkiaSharp/ externals/skia/include/c/ externals/skia/src/c/

# For each "full" item — confirm the C# wrapper exists AND calls the right native function
grep -n "MethodName" binding/SkiaSharp/SKRelevantFile.cs
```

This catches hallucinations where an agent claims something exists (or doesn't) incorrectly.
Pay special attention to the **implementation**, not just the signature — a method may exist by
name but forward to the wrong native function (the "ToRawShader bug" pattern).

### Phase 5: Generate Outputs

The synthesis from Phase 3 should have produced all the data. Now generate the three output
formats.

**5a. Generate Structured JSON Report**

Produce a JSON report following the schema in [references/schema-cheatsheet.md](references/schema-cheatsheet.md).
The formal JSON Schema is at [references/feature-scout-schema.json](references/feature-scout-schema.json).
Save to the artifacts directory as `skia-feature-scout-YYYY-MM-DD.json`.

The JSON must include:
- `meta` — audit metadata (date, milestones, source)
- `summary` — counts by status and priority
- `items` — every cataloged feature with full binding details
- `deprecations` — APIs needing `[Obsolete]` markers, with exact SkiaSharp API name/file,
  the Skia milestone when deprecated, the replacement API, and a suggested `[Obsolete("...")]` msg
- `hiddenApis` — features discovered via C++ header scan (not in release notes)
- `performance` — performance-related changes worth noting
- `nextSteps` — prioritized action items, each with `skillToUse` and `effort`

**5b. Validate JSON Report**

> 🛑 **MANDATORY:** Always validate before rendering.

```bash
pip3 install -r .agents/skills/skia-feature-scout/scripts/requirements.txt --quiet
python3 .agents/skills/skia-feature-scout/scripts/validate-feature-scout.py <path-to-json>
```

Exit codes: 0=valid, 1=fixable (regenerate), 2=fatal. Fix and re-validate if it fails.

**5c. Render HTML Report**

> 🛑 **MANDATORY:** Always generate the HTML report.

```bash
python3 .agents/skills/skia-feature-scout/scripts/render-feature-scout.py <path-to-json>
```

**5d. Generate Markdown Summary**

Present a concise markdown summary in the conversation, grouped by urgency:

1. 🔴 **Critical** — Will break on next Skia bump
2. ⚠️ **Action Needed** — Deprecated APIs missing `[Obsolete]` markers
3. ❌ **Missing (High)** — Major features with no binding
4. 🔶 **Missing (Medium)** — Useful features to plan for
5. 🟢 **Full** — Features already bound
6. 🟡 **Quick Wins** — C API exists, just needs C# wrapper
7. 🆕 **Hidden APIs** — Discovered via C++ scan, not in release notes
8. ⚡ **Performance** — Speed/memory improvements
9. 🔄 **Behavior Changes** — Silent semantic changes

Include: Before/After milestone split, Deprecation Watch, Recommended Action Plan with skill routing.

### Phase 6: Offer Next Steps

After presenting the report, offer:
1. "Want me to investigate any of these features in more detail?"
2. "Should I create issues or todos for the high-priority items?"
3. "Want me to use the `add-api` skill to start binding a specific feature?"
4. "Should I set up a periodic workflow to re-run this audit?"

---

## Feature Extraction Criteria

These criteria are used by the fan-out agents (Phase 2) and by you during synthesis (Phase 3).

**Include (high signal):**
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

## Tips for Accurate Assessment

- **Don't confuse enum values with full support.** Having `AVIF` in an encoded format enum doesn't
  mean AVIF decoding is fully wired up in C#.
- **Check for the actual C# method, not just the class.** A class may exist but be missing specific
  overloads (e.g., DropShadow exists but only with SKColor, not SKColor4f).
- **Verify C# wrappers actually call the right C API.** A wrapper may exist with the right name but
  forward to the wrong native function. For example, check that `ToRawShader` actually calls a raw
  shader C API and not the regular `makeShader`. Read the implementation, not just the signature.
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
