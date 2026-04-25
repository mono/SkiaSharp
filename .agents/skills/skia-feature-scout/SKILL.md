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

The skill uses a **dual-model / synthesize / verify** architecture. Two different AI models each
independently do a complete audit (release notes + hidden APIs + binding checks), then you merge
their findings, resolve conflicts, and verify accuracy. This approach catches items that any single
model would miss — testing shows individual runs miss 15-30% of findings the other model catches,
with almost no overlap in their blind spots.

```
Phase 1: Setup (fetch notes, determine milestone, locate C API)
Phase 2: Launch two parallel full-audit agents (different models)
  ├─ Agent 1 (Opus 4.7): Full audit — notes + hidden APIs + binding check
  └─ Agent 2 (GPT 5.4):  Full audit — notes + hidden APIs + binding check
Phase 3: Synthesize — merge both agents' findings, dedupe, resolve conflicts
Phase 4: Verify — you spot-check high-priority items directly
Phase 5: Generate outputs (JSON → validate → HTML → markdown)
Phase 6: Offer next steps
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
cat > /tmp/skia-release-notes.md << 'NOTES'
<paste all fetched content>
NOTES
```

In **windowed mode**, you can stop once you've read past the target milestone range, but always
read at least a few milestones beyond the current one to catch future items.

**1b. Determine Current Milestone**

```bash
cd externals/skia && git log --oneline -1
cat externals/skia/include/core/SkMilestone.h 2>/dev/null
```

The user may also tell you directly. Everything at or below this milestone is "available now".
Everything above is "coming with bump".

**1c. Locate C API headers**

The skia submodule may not be checked out in this worktree. Find the C API:

```bash
ls externals/skia/include/c/ 2>/dev/null
# Fallback to main repo checkout
ls /path/to/main/SkiaSharp/externals/skia/include/c/ 2>/dev/null
```

Record the path for agents to use.

### Phase 2: Launch Two Full-Audit Agents

Launch **two** background `task` agents simultaneously, using **different models**. Each does the
same complete job — read ALL release notes, scan C++ headers for hidden APIs, check bindings. Their
different perspectives and blind spots complement each other.

Give each agent this prompt (adapted with actual paths and milestone):

```
task agent_type=general-purpose mode=background model=claude-opus-4.7 name=scout-opus:
task agent_type=general-purpose mode=background model=gpt-5.4 name=scout-gpt:

  "You are auditing Skia release notes for SkiaSharp. Work from scratch — be thorough.
   SkiaSharp is on milestone M{current}.

   FIRST: Read .agents/skills/skia-feature-scout/references/extraction-criteria.md
   It contains the full criteria, categories, priorities, binding status classifications,
   and C#→C API→C++ type mapping table you need for this audit.

   THEN DO THREE THINGS:

   1. RELEASE NOTES SCAN
      Read /tmp/skia-release-notes.md (the full file, ALL milestones M78 through latest).
      Extract every notable feature. For each, record:
      - name, category, milestoneIntroduced, milestoneEnhanced, milestoneDeprecated,
        milestoneRemoved, skiaApi, description, userValue, priority
      Include: new APIs/classes, codec/format support, color types, shader features,
      image filter additions, canvas enhancements, performance improvements,
      behavior changes, codec introspection, GPU interop, text/font APIs,
      sampling options, API migrations.
      Exclude: header reorgs, Graphite/Dawn internals, build system, SkSL parser fixes.
      PAY EQUAL ATTENTION TO OLD MILESTONES (M78-M100). They contain overlooked gems.

   2. HIDDEN API SCAN
      For each type SkiaSharp binds, fetch the upstream C++ header from Google's repo
      (https://raw.githubusercontent.com/google/skia/main/include/core/SkImage.h etc.)
      and compare against our C API in {c_api_path}.
      Focus on: SkImage.h, SkCanvas.h, SkImageFilters.h, SkShader.h, SkCodec.h,
      SkColorFilter.h, SkPath.h, SkPathBuilder.h, SkBitmap.h, SkTextBlob.h.
      For each public C++ method with no C API equivalent, output:
      { cppClass, cppHeader, cppMethod, description, priority }

   3. BINDING VERIFICATION
      For each feature found in step 1, check:
      - C API: grep {c_api_path}/*.h and src/c/*.cpp
      - C# wrapper: grep binding/SkiaSharp/*.cs
      - Generated interop: check SkiaApi.generated.cs for hidden struct fields
        (especially encoder options: ICC, XMP, gainmap, HDR metadata fields that
        exist in generated code but not in public option structs)
      - For any method with 'Raw' in the name, verify it calls the correct C API
        (not a regular version — read the implementation, not just the signature)
      Classify each as: full, partial, missing, action_needed, correctly_absent, not_applicable

      Also look for SkiaSharp APIs wrapping things Skia has deprecated — these need [Obsolete].

   Save your complete findings as JSON to {output_path}/{agent_name}-findings.json"
```

### Phase 3: Synthesize

When both agents complete, merge their findings. This is your most important job —
you are the quality gate.

1. **Collect** all feature items from both agents into a combined list
2. **Deduplicate** — if both found the same feature (by skiaApi name or description), merge them:
   - Keep the richer description / userValue
   - If they disagree on bindingStatus, the more cautious one wins (e.g., `missing` beats `full`)
   - If they disagree on priority, keep the higher one and note the disagreement
3. **Union the hidden APIs** — combine both agents' hidden API lists, deduplicating by method name.
   Testing shows the two models find almost completely different hidden APIs (only ~6% overlap), so
   the union is much richer than either alone.
4. **Cross-reference binding statuses**:
   - If one agent says `full` and the other says `missing`, investigate directly (Phase 4)
   - If one found `partial` bindings (C API exists, no C# wrapper), keep those — they're quick wins
   - If either found implementation bugs (wrong C API calls), mark as `action_needed`
   - If either found hidden generated interop fields, add as `partial` quick wins
5. **Track feature lifecycles** — merge entries for features that appear across multiple milestones
   into single items with milestoneIntroduced + milestoneEnhanced + milestoneDeprecated + milestoneRemoved
6. **Extract performance notes** into the `performance` array
7. **Extract deprecations** into the `deprecations` array with concrete `[Obsolete]` messages
8. **Build `nextSteps`** ordered by priority, with `skillToUse` and `effort`

### Phase 4: Verify High-Priority Findings

For every item marked `critical` or `high` priority, and every item marked `full` (to confirm
they're not false positives), do a direct verification yourself:

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

See [references/extraction-criteria.md](references/extraction-criteria.md) for the complete criteria,
categories, priorities, binding status classifications, and C#→C API→C++ type mapping table.

Agents must read that file before starting their audit. It contains everything they need to know
about what to look for, what to skip, and how to classify findings.

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
