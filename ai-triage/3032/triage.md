# Issue Triage Report — #3032

| Field | Value |
|-------|-------|
| Repository | mono/SkiaSharp |
| Analyzed | 2026-05-06T01:03:45Z |
| Type | type/enhancement (0.92 (92%)) |
| Area | area/SkiaSharp.HarfBuzz (0.95 (95%)) |
| Suggested action | keep-open (0.85 (85%)) |

**Issue Summary:** Reporter requests that the no-SKShaper overload of DrawShapedText cache the SKShaper (and optionally the shaping results) instead of creating and disposing a new instance on every call, backed by benchmarks in a linked blog post.

**Analysis:** The convenience overload DrawShapedText(string, float, float, SKTextAlign, SKFont, SKPaint) in CanvasExtensions.cs calls `new SKShaper(font.Typeface)` inside the method body and wraps it in `using`, so it is constructed and destroyed on every render call. SKShaper construction is expensive because it opens the font stream, builds a HarfBuzz Face, and allocates a HarfBuzz Buffer. The fix is to cache one SKShaper per typeface (e.g., via a static ConcurrentDictionary), or to encourage callers to pass a pre-created SKShaper. A secondary improvement is caching the shaping Result keyed on (text, font size) to avoid re-shaping identical strings.

**Recommendations:** **keep-open** — Valid performance enhancement with benchmarks. A workaround exists (pass SKShaper explicitly). The caching design needs discussion (thread safety, cache eviction, typeface lifecycle) before implementation.

---

## Classification

| Field | Value |
|-------|-------|
| Type | type/enhancement |
| Area | area/SkiaSharp.HarfBuzz |
| Platforms | — |
| Backends | — |
| Tenets | tenet/performance |
| Partner | — |
| Current labels | type/feature-request |

## Evidence

### Reproduction

1. Call the no-SKShaper overload of DrawShapedText in a tight rendering loop
2. Profile and observe that a new SKShaper is allocated and disposed on every call

**Environment:** Any platform; benchmarks provided at https://www.mrumpler.at/performance-tuning-drawshapedtext/

**Repository links:**
- https://www.mrumpler.at/performance-tuning-drawshapedtext/ — Reporter's blog post with benchmarks showing the performance gap between the no-cache and cached-shaper overloads

## Analysis

### Technical Summary

The convenience overload DrawShapedText(string, float, float, SKTextAlign, SKFont, SKPaint) in CanvasExtensions.cs calls `new SKShaper(font.Typeface)` inside the method body and wraps it in `using`, so it is constructed and destroyed on every render call. SKShaper construction is expensive because it opens the font stream, builds a HarfBuzz Face, and allocates a HarfBuzz Buffer. The fix is to cache one SKShaper per typeface (e.g., via a static ConcurrentDictionary), or to encourage callers to pass a pre-created SKShaper. A secondary improvement is caching the shaping Result keyed on (text, font size) to avoid re-shaping identical strings.

### Rationale

Classified as type/enhancement (not feature-request) because the infrastructure (SKShaper, shaped-text drawing) already exists; the request is to improve the built-in convenience overload by caching. Area is area/SkiaSharp.HarfBuzz because the impacted code lives in CanvasExtensions.cs within the SkiaSharp.HarfBuzz package. The tenet/performance label captures the quality dimension. suggestedAction is keep-open because the improvement is valid but requires a design decision on cache ownership and invalidation strategy (no obvious quick fix without API changes or hidden state).

### Key Signals

- "It creates and disposes a SKShaper at every call which is very slow." — **issue body** (Confirmed by code: CanvasExtensions.cs line 33 wraps the shaper in `using`.)
- "There is an overload of DrawShapedText which takes a SKShaper and does not create/dispose it at every call, but people need to have more insider knowledge of HarfBuzzSharp to use it." — **issue body** (The workaround already exists; the enhancement is to make it automatic/transparent.)

### Code Investigation

| File | Lines | Relevance | Finding |
|------|-------|-----------|---------|
| `source/SkiaSharp.HarfBuzz/SkiaSharp.HarfBuzz/CanvasExtensions.cs` | 28-35 | direct | The public DrawShapedText convenience overload that does not accept an SKShaper creates `new SKShaper(font.Typeface)` inside a `using` block on every invocation, then delegates to the shaper-accepting overload. |
| `source/SkiaSharp.HarfBuzz/SkiaSharp.HarfBuzz/SKShaper.cs` | 15-33 | direct | SKShaper constructor opens the typeface stream, constructs a HarfBuzz Face and Font with SetFunctionsOpenType(), and allocates a HarfBuzz Buffer — all work that only needs to be done once per typeface. |

### Workarounds

- Create and reuse an SKShaper instance per typeface at the call site, then call the DrawShapedText overload that accepts an SKShaper parameter.

### Resolution Proposals

**Hypothesis:** Cache one SKShaper per SKTypeface inside a static or instance-level dictionary keyed on the typeface handle, disposing entries when the typeface is disposed. Optionally cache shaping results keyed on (text, font-size) with an LRU eviction policy.

1. **Static SKShaper cache in CanvasExtensions** — fix, cost/m, validated=untested
   - Add a static ConcurrentDictionary<int, SKShaper> inside CanvasExtensions keyed on SKTypeface.RuntimeId. Retrieve or create the shaper on first use; do not dispose it per-call.
2. **Use existing shaper overload as the current workaround** — workaround, cost/xs, validated=untested
   - Create one SKShaper per typeface at the rendering site and pass it to DrawShapedText(SKShaper, ...). This avoids the allocation cost immediately with no library change.

**Recommended proposal:** Use existing shaper overload as the current workaround

**Why:** Zero risk, available today; the cache-based fix needs API/lifetime design discussion before implementation.

## Recommendations

### Actionability

| Field | Value |
|-------|-------|
| Suggested action | keep-open |
| Confidence | 0.85 (85%) |
| Reason | Valid performance enhancement with benchmarks. A workaround exists (pass SKShaper explicitly). The caching design needs discussion (thread safety, cache eviction, typeface lifecycle) before implementation. |
| Suggested repro platform | linux |

### Automatable Actions

| Type | Risk | Confidence | Description | Details |
|------|------|------------|-------------|---------|
| update-labels | low | 0.92 (92%) | Apply type/enhancement, area/SkiaSharp.HarfBuzz, tenet/performance labels | labels=type/enhancement, area/SkiaSharp.HarfBuzz, tenet/performance |
| add-comment | medium | 0.85 (85%) | Acknowledge the valid performance concern, confirm the code path, point to the existing workaround, and explain the cache-design question that must be resolved before implementing | — |

**Comment draft for `add-comment`:**

```markdown
Thank you for the detailed report and benchmarks!

I confirmed the behaviour in the source: the convenience overload `DrawShapedText(string, float, float, SKTextAlign, SKFont, SKPaint)` in [`CanvasExtensions.cs`](https://github.com/mono/SkiaSharp/blob/main/source/SkiaSharp.HarfBuzz/SkiaSharp.HarfBuzz/CanvasExtensions.cs#L28-L35) constructs a new `SKShaper` on every call and disposes it immediately. Because `SKShaper` construction opens the font stream and initialises HarfBuzz internals, this is indeed expensive for tight rendering loops.

**Workaround available today:** Use the overload that accepts a pre-created `SKShaper` and keep it alive for the lifetime of your rendering loop:

```csharp
// Create once (e.g., in your page/view constructor)
private readonly SKShaper _shaper = new SKShaper(myTypeface);

// In your paint handler
canvas.DrawShapedText(_shaper, text, x, y, textAlign, font, paint);

// Dispose when done
_shaper.Dispose();
```

For a library-level fix, we need to agree on a caching strategy (thread safety, cache scope, eviction when typefaces are disposed) before implementing. Keeping this open to track that design work.
```

<details>
<summary>Raw JSON</summary>

```json
{
  "meta": {
    "schemaVersion": "1.0",
    "number": 3032,
    "repo": "mono/SkiaSharp",
    "analyzedAt": "2026-05-06T01:03:45Z",
    "currentLabels": [
      "type/feature-request"
    ]
  },
  "summary": "Reporter requests that the no-SKShaper overload of DrawShapedText cache the SKShaper (and optionally the shaping results) instead of creating and disposing a new instance on every call, backed by benchmarks in a linked blog post.",
  "classification": {
    "type": {
      "value": "type/enhancement",
      "confidence": 0.92
    },
    "area": {
      "value": "area/SkiaSharp.HarfBuzz",
      "confidence": 0.95
    },
    "tenets": [
      "tenet/performance"
    ]
  },
  "evidence": {
    "reproEvidence": {
      "stepsToReproduce": [
        "Call the no-SKShaper overload of DrawShapedText in a tight rendering loop",
        "Profile and observe that a new SKShaper is allocated and disposed on every call"
      ],
      "environmentDetails": "Any platform; benchmarks provided at https://www.mrumpler.at/performance-tuning-drawshapedtext/",
      "repoLinks": [
        {
          "url": "https://www.mrumpler.at/performance-tuning-drawshapedtext/",
          "description": "Reporter's blog post with benchmarks showing the performance gap between the no-cache and cached-shaper overloads"
        }
      ]
    }
  },
  "analysis": {
    "summary": "The convenience overload DrawShapedText(string, float, float, SKTextAlign, SKFont, SKPaint) in CanvasExtensions.cs calls `new SKShaper(font.Typeface)` inside the method body and wraps it in `using`, so it is constructed and destroyed on every render call. SKShaper construction is expensive because it opens the font stream, builds a HarfBuzz Face, and allocates a HarfBuzz Buffer. The fix is to cache one SKShaper per typeface (e.g., via a static ConcurrentDictionary), or to encourage callers to pass a pre-created SKShaper. A secondary improvement is caching the shaping Result keyed on (text, font size) to avoid re-shaping identical strings.",
    "codeInvestigation": [
      {
        "file": "source/SkiaSharp.HarfBuzz/SkiaSharp.HarfBuzz/CanvasExtensions.cs",
        "lines": "28-35",
        "finding": "The public DrawShapedText convenience overload that does not accept an SKShaper creates `new SKShaper(font.Typeface)` inside a `using` block on every invocation, then delegates to the shaper-accepting overload.",
        "relevance": "direct"
      },
      {
        "file": "source/SkiaSharp.HarfBuzz/SkiaSharp.HarfBuzz/SKShaper.cs",
        "lines": "15-33",
        "finding": "SKShaper constructor opens the typeface stream, constructs a HarfBuzz Face and Font with SetFunctionsOpenType(), and allocates a HarfBuzz Buffer — all work that only needs to be done once per typeface.",
        "relevance": "direct"
      }
    ],
    "keySignals": [
      {
        "text": "It creates and disposes a SKShaper at every call which is very slow.",
        "source": "issue body",
        "interpretation": "Confirmed by code: CanvasExtensions.cs line 33 wraps the shaper in `using`."
      },
      {
        "text": "There is an overload of DrawShapedText which takes a SKShaper and does not create/dispose it at every call, but people need to have more insider knowledge of HarfBuzzSharp to use it.",
        "source": "issue body",
        "interpretation": "The workaround already exists; the enhancement is to make it automatic/transparent."
      }
    ],
    "rationale": "Classified as type/enhancement (not feature-request) because the infrastructure (SKShaper, shaped-text drawing) already exists; the request is to improve the built-in convenience overload by caching. Area is area/SkiaSharp.HarfBuzz because the impacted code lives in CanvasExtensions.cs within the SkiaSharp.HarfBuzz package. The tenet/performance label captures the quality dimension. suggestedAction is keep-open because the improvement is valid but requires a design decision on cache ownership and invalidation strategy (no obvious quick fix without API changes or hidden state).",
    "workarounds": [
      "Create and reuse an SKShaper instance per typeface at the call site, then call the DrawShapedText overload that accepts an SKShaper parameter."
    ],
    "resolution": {
      "hypothesis": "Cache one SKShaper per SKTypeface inside a static or instance-level dictionary keyed on the typeface handle, disposing entries when the typeface is disposed. Optionally cache shaping results keyed on (text, font-size) with an LRU eviction policy.",
      "proposals": [
        {
          "title": "Static SKShaper cache in CanvasExtensions",
          "description": "Add a static ConcurrentDictionary<int, SKShaper> inside CanvasExtensions keyed on SKTypeface.RuntimeId. Retrieve or create the shaper on first use; do not dispose it per-call.",
          "category": "fix",
          "effort": "cost/m",
          "validated": "untested"
        },
        {
          "title": "Use existing shaper overload as the current workaround",
          "description": "Create one SKShaper per typeface at the rendering site and pass it to DrawShapedText(SKShaper, ...). This avoids the allocation cost immediately with no library change.",
          "category": "workaround",
          "effort": "cost/xs",
          "validated": "untested"
        }
      ],
      "recommendedProposal": "Use existing shaper overload as the current workaround",
      "recommendedReason": "Zero risk, available today; the cache-based fix needs API/lifetime design discussion before implementation."
    }
  },
  "output": {
    "actionability": {
      "suggestedAction": "keep-open",
      "confidence": 0.85,
      "reason": "Valid performance enhancement with benchmarks. A workaround exists (pass SKShaper explicitly). The caching design needs discussion (thread safety, cache eviction, typeface lifecycle) before implementation.",
      "suggestedReproPlatform": "linux"
    },
    "actions": [
      {
        "type": "update-labels",
        "description": "Apply type/enhancement, area/SkiaSharp.HarfBuzz, tenet/performance labels",
        "risk": "low",
        "confidence": 0.92,
        "labels": [
          "type/enhancement",
          "area/SkiaSharp.HarfBuzz",
          "tenet/performance"
        ]
      },
      {
        "type": "add-comment",
        "description": "Acknowledge the valid performance concern, confirm the code path, point to the existing workaround, and explain the cache-design question that must be resolved before implementing",
        "risk": "medium",
        "confidence": 0.85,
        "comment": "Thank you for the detailed report and benchmarks!\n\nI confirmed the behaviour in the source: the convenience overload `DrawShapedText(string, float, float, SKTextAlign, SKFont, SKPaint)` in [`CanvasExtensions.cs`](https://github.com/mono/SkiaSharp/blob/main/source/SkiaSharp.HarfBuzz/SkiaSharp.HarfBuzz/CanvasExtensions.cs#L28-L35) constructs a new `SKShaper` on every call and disposes it immediately. Because `SKShaper` construction opens the font stream and initialises HarfBuzz internals, this is indeed expensive for tight rendering loops.\n\n**Workaround available today:** Use the overload that accepts a pre-created `SKShaper` and keep it alive for the lifetime of your rendering loop:\n\n```csharp\n// Create once (e.g., in your page/view constructor)\nprivate readonly SKShaper _shaper = new SKShaper(myTypeface);\n\n// In your paint handler\ncanvas.DrawShapedText(_shaper, text, x, y, textAlign, font, paint);\n\n// Dispose when done\n_shaper.Dispose();\n```\n\nFor a library-level fix, we need to agree on a caching strategy (thread safety, cache scope, eviction when typefaces are disposed) before implementing. Keeping this open to track that design work."
      }
    ]
  }
}
```

</details>
