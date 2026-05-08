# Issue Triage Report — #2446

| Field | Value |
|-------|-------|
| Repository | mono/SkiaSharp |
| Analyzed | 2026-04-22T05:09:28Z |
| Type | type/bug (0.80 (80%)) |
| Area | area/SkiaSharp (0.92 (92%)) |
| Suggested action | close-as-not-a-bug (0.78 (78%)) |

**Issue Summary:** Memory grows unboundedly when creating SKTypeface from a stream in a loop and calling MeasureText, because Skia's internal glyph cache retains data after the typeface is disposed; workarounds via SKGraphics.PurgeFontCache() exist.

**Analysis:** When SKTypeface is created from a stream and used with MeasureText, Skia's internal SkStrikeCache (glyph cache) retains rendered glyph data globally even after the typeface is disposed. This is by-design for performance, but in batch scenarios (e.g., 100+ document conversions each using unique embedded fonts) the cache grows beyond practical limits and can cause OOM. The fix is to call SKGraphics.PurgeFontCache() periodically, or to set cache limits via SKGraphics.SetFontCacheCountLimit/SetFontCacheLimit.

**Recommendations:** **close-as-not-a-bug** — The memory growth is Skia's intentional glyph caching behavior, not a SkiaSharp bug. The public API (SKGraphics.PurgeFontCache, SetFontCacheLimit) provides full control, and the reporter confirmed the workaround resolves the issue.

---

## Classification

| Field | Value |
|-------|-------|
| Type | type/bug |
| Area | area/SkiaSharp |
| Platforms | os/Windows-Classic |
| Backends | — |
| Tenets | tenet/reliability |
| Partner | — |

## Evidence

### Reproduction

1. Run a loop calling SKTypeface.FromStream() with a .ttf file each iteration
2. Create SKPaint with that typeface and call paint.MeasureText()
3. Dispose paint, typeface, and stream; call GC.Collect()
4. Observe memory growing continuously

**Environment:** Windows 10 Pro, Visual Studio 2022 Preview, SkiaSharp 2.88.3

**Repository links:**
- https://github.com/mono/SkiaSharp/files/11273548/SkiaSharpTesting.zip — Attached minimal repro project
- https://github.com/mono/SkiaSharp/issues/996 — Related prior issue: memory leak when creating SKTypeface from file/stream (closed as fixed in 1.68.1, but glyph cache concern remains)

### Bug Signals

| Field | Value |
|-------|-------|
| Severity | medium |
| Regression claimed | False |
| Error type | memory-leak |
| Error message | Memory allocation increasing rapidly without any disposal |
| Repro quality | complete |
| Target frameworks | net6.0-windows |

### Version Analysis

| Field | Value |
|-------|-------|
| Mentioned versions | 2.88.3 |
| Worked in | — |
| Broke in | — |
| Current relevance | likely |
| Relevance reason | The glyph cache behavior is fundamental to Skia and has not changed; the SKGraphics cache control APIs are available but underdocumented for this use case. |

## Analysis

### Technical Summary

When SKTypeface is created from a stream and used with MeasureText, Skia's internal SkStrikeCache (glyph cache) retains rendered glyph data globally even after the typeface is disposed. This is by-design for performance, but in batch scenarios (e.g., 100+ document conversions each using unique embedded fonts) the cache grows beyond practical limits and can cause OOM. The fix is to call SKGraphics.PurgeFontCache() periodically, or to set cache limits via SKGraphics.SetFontCacheCountLimit/SetFontCacheLimit.

### Rationale

The memory growth is due to Skia's global glyph cache (SkStrikeCache), not a raw memory leak in SKTypeface disposal. A contributor confirmed this is by-design caching behavior. The difference between FromFamilyName (no leak) vs FromStream/FromFile (apparent leak) is that Skia deduplicates named system fonts in the cache, whereas stream-loaded fonts each get their own cache entry. Issue #996 described a similar pattern and was closed as fixed in 1.68.1; the current issue is the expected glyph cache behavior with no built-in auto-eviction for stream typefaces. The workaround via SKGraphics.PurgeFontCache() was confirmed by both a community member and the original reporter.

### Key Signals

- "Skia is caching glyphs data and is only clearing the cache if it reaches some defined memory usage level." — **comment by contributor @Gillibald** (Confirms by-design caching behavior, not a raw memory leak.)
- "SKGraphics.PurgeFontCache(); — You can also set your own limits in the cache via SKGraphics.SetFontCacheCountLimit / SetFontCacheLimit" — **comment by @themcoo** (Confirmed workaround using existing public API.)
- "Huge thanks for your solution. It's solved the problem in attached simple app." — **reporter reply to @themcoo** (PurgeFontCache workaround confirmed working by the original reporter.)
- "At certain stage, app crashed with out of memory problem." — **reporter comment explaining Word-to-PDF batch conversion use case** (Real-world severity: OOM crash in batch document processing with unique embedded fonts per document.)

### Code Investigation

| File | Lines | Relevance | Finding |
|------|-------|-----------|---------|
| `binding/SkiaSharp/SKGraphics.cs` | 14-41 | direct | SKGraphics.PurgeFontCache(), SetFontCacheCountLimit(int), and SetFontCacheLimit(long) are all publicly available APIs that let callers control and purge Skia's global glyph cache. These are the intended escape valves for this scenario. |
| `binding/SkiaSharp/SKTypeface.cs` | 92-113 | direct | SKTypeface.FromStream converts the managed Stream to an SKMemoryStream, then calls sk_typeface_create_from_stream. The SKTypeface object is ref-counted (ISKReferenceCounted). Disposing the C# wrapper decrements the ref count, but does NOT purge glyph data already placed into Skia's global SkStrikeCache during MeasureText. |
| `binding/SkiaSharp/SKTypeface.cs` | 55-71 | related | SKTypeface.FromFamilyName calls PreventPublicDisposal() and returns a cached handle — system fonts are deduplicated, explaining why callers do not see memory growth with named fonts. |

### Workarounds

- Call SKGraphics.PurgeFontCache() after each document/batch to force-release cached glyph data.
- Call SKGraphics.SetFontCacheCountLimit(n) to cap the number of typeface entries in the cache.
- Call SKGraphics.SetFontCacheLimit(bytes) to cap total cache memory (e.g., SKGraphics.SetFontCacheLimit(1024 * 1024) for 1 MB).
- Cache SKTypeface instances in a static dictionary keyed by font identity (file path or stream hash) to avoid creating duplicate entries.

### Next Questions

- Does SKGraphics.PurgeFontCache() also resolve the issue for @afruzan who reported it did not help with FromFile?
- Should SKTypeface.Dispose() automatically call PurgeFontCache for stream-loaded typefaces to match user expectations?

### Resolution Proposals

**Hypothesis:** The memory growth is caused by Skia's global glyph cache accumulating entries for each unique stream-loaded typeface. Disposing the SKTypeface only frees the typeface object, not its glyph cache entries. The cache is evicted automatically only when it exceeds a size threshold.

1. **Purge font cache after batch** — workaround, confidence 0.90 (90%), cost/xs, validated=yes
   - Call SKGraphics.PurgeFontCache() at the end of each document conversion to release all cached glyph data.

```csharp
// After each document conversion:
SKGraphics.PurgeFontCache();

// Or set a tight limit at app startup:
SKGraphics.SetFontCacheCountLimit(2);
SKGraphics.SetFontCacheLimit(4 * 1024 * 1024); // 4 MB
```
2. **Cache and reuse SKTypeface instances** — alternative, confidence 0.80 (80%), cost/s, validated=yes
   - Keep a static dictionary of SKTypeface instances keyed by font identity. Reuse existing typefaces instead of creating new ones from stream each iteration. Only applicable when fonts repeat across documents.

```csharp
private static readonly ConcurrentDictionary<string, SKTypeface> _typefaceCache = new();

public static SKTypeface GetTypeface(string fontKey, Func<SKTypeface> factory)
    => _typefaceCache.GetOrAdd(fontKey, _ => factory());
```

**Recommended proposal:** Purge font cache after batch

**Why:** Directly addresses the batch document scenario with minimal code change. Confirmed working by the original reporter.

## Recommendations

### Actionability

| Field | Value |
|-------|-------|
| Suggested action | close-as-not-a-bug |
| Confidence | 0.78 (78%) |
| Reason | The memory growth is Skia's intentional glyph caching behavior, not a SkiaSharp bug. The public API (SKGraphics.PurgeFontCache, SetFontCacheLimit) provides full control, and the reporter confirmed the workaround resolves the issue. |
| Suggested repro platform | linux |

### Automatable Actions

| Type | Risk | Confidence | Description | Details |
|------|------|------------|-------------|---------|
| update-labels | low | 0.90 (90%) | Apply type/bug, area/SkiaSharp, os/Windows-Classic, tenet/reliability | labels=type/bug, area/SkiaSharp, os/Windows-Classic, tenet/reliability |
| add-comment | medium | 0.85 (85%) | Explain glyph cache behavior and provide PurgeFontCache workaround | — |
| close-issue | medium | 0.78 (78%) | Close as not a bug — by-design Skia glyph caching with documented workaround | stateReason=not_planned |

**Comment draft for `add-comment`:**

```markdown
Thanks for the detailed report and repro!

The memory growth you're seeing is caused by Skia's **internal glyph cache** (`SkStrikeCache`). When `MeasureText` is called, Skia renders and caches the glyph metrics for that typeface globally — this cache is not released when you dispose the `SKTypeface`. It's a performance optimization (avoid re-rasterizing the same glyphs), but in a batch scenario with many unique stream-loaded fonts it can grow significantly.

SkiaSharp exposes the cache control APIs you need via `SKGraphics`:

```csharp
// Option 1: Purge cache after each document conversion
SKGraphics.PurgeFontCache();

// Option 2: Cap cache size at app startup (e.g., 4 MB)
SKGraphics.SetFontCacheLimit(4 * 1024 * 1024);

// Option 3: Cap number of cached typefaces
SKGraphics.SetFontCacheCountLimit(10);
```

For your Word-to-PDF scenario, calling `SKGraphics.PurgeFontCache()` after each document conversion should keep memory bounded.

Since this behavior is by design in Skia and the workaround is available via the public API, I'll close this issue. Please reopen if `PurgeFontCache` doesn't fully resolve your use case.
```

<details>
<summary>Raw JSON</summary>

```json
{
  "meta": {
    "schemaVersion": "1.0",
    "number": 2446,
    "repo": "mono/SkiaSharp",
    "analyzedAt": "2026-04-22T05:09:28Z"
  },
  "summary": "Memory grows unboundedly when creating SKTypeface from a stream in a loop and calling MeasureText, because Skia's internal glyph cache retains data after the typeface is disposed; workarounds via SKGraphics.PurgeFontCache() exist.",
  "classification": {
    "type": {
      "value": "type/bug",
      "confidence": 0.8
    },
    "area": {
      "value": "area/SkiaSharp",
      "confidence": 0.92
    },
    "platforms": [
      "os/Windows-Classic"
    ],
    "tenets": [
      "tenet/reliability"
    ]
  },
  "evidence": {
    "bugSignals": {
      "severity": "medium",
      "regressionClaimed": false,
      "errorType": "memory-leak",
      "errorMessage": "Memory allocation increasing rapidly without any disposal",
      "reproQuality": "complete",
      "targetFrameworks": [
        "net6.0-windows"
      ]
    },
    "reproEvidence": {
      "stepsToReproduce": [
        "Run a loop calling SKTypeface.FromStream() with a .ttf file each iteration",
        "Create SKPaint with that typeface and call paint.MeasureText()",
        "Dispose paint, typeface, and stream; call GC.Collect()",
        "Observe memory growing continuously"
      ],
      "environmentDetails": "Windows 10 Pro, Visual Studio 2022 Preview, SkiaSharp 2.88.3",
      "repoLinks": [
        {
          "url": "https://github.com/mono/SkiaSharp/files/11273548/SkiaSharpTesting.zip",
          "description": "Attached minimal repro project"
        },
        {
          "url": "https://github.com/mono/SkiaSharp/issues/996",
          "description": "Related prior issue: memory leak when creating SKTypeface from file/stream (closed as fixed in 1.68.1, but glyph cache concern remains)"
        }
      ]
    },
    "versionAnalysis": {
      "mentionedVersions": [
        "2.88.3"
      ],
      "currentRelevance": "likely",
      "relevanceReason": "The glyph cache behavior is fundamental to Skia and has not changed; the SKGraphics cache control APIs are available but underdocumented for this use case."
    }
  },
  "analysis": {
    "summary": "When SKTypeface is created from a stream and used with MeasureText, Skia's internal SkStrikeCache (glyph cache) retains rendered glyph data globally even after the typeface is disposed. This is by-design for performance, but in batch scenarios (e.g., 100+ document conversions each using unique embedded fonts) the cache grows beyond practical limits and can cause OOM. The fix is to call SKGraphics.PurgeFontCache() periodically, or to set cache limits via SKGraphics.SetFontCacheCountLimit/SetFontCacheLimit.",
    "rationale": "The memory growth is due to Skia's global glyph cache (SkStrikeCache), not a raw memory leak in SKTypeface disposal. A contributor confirmed this is by-design caching behavior. The difference between FromFamilyName (no leak) vs FromStream/FromFile (apparent leak) is that Skia deduplicates named system fonts in the cache, whereas stream-loaded fonts each get their own cache entry. Issue #996 described a similar pattern and was closed as fixed in 1.68.1; the current issue is the expected glyph cache behavior with no built-in auto-eviction for stream typefaces. The workaround via SKGraphics.PurgeFontCache() was confirmed by both a community member and the original reporter.",
    "codeInvestigation": [
      {
        "file": "binding/SkiaSharp/SKGraphics.cs",
        "lines": "14-41",
        "finding": "SKGraphics.PurgeFontCache(), SetFontCacheCountLimit(int), and SetFontCacheLimit(long) are all publicly available APIs that let callers control and purge Skia's global glyph cache. These are the intended escape valves for this scenario.",
        "relevance": "direct"
      },
      {
        "file": "binding/SkiaSharp/SKTypeface.cs",
        "lines": "92-113",
        "finding": "SKTypeface.FromStream converts the managed Stream to an SKMemoryStream, then calls sk_typeface_create_from_stream. The SKTypeface object is ref-counted (ISKReferenceCounted). Disposing the C# wrapper decrements the ref count, but does NOT purge glyph data already placed into Skia's global SkStrikeCache during MeasureText.",
        "relevance": "direct"
      },
      {
        "file": "binding/SkiaSharp/SKTypeface.cs",
        "lines": "55-71",
        "finding": "SKTypeface.FromFamilyName calls PreventPublicDisposal() and returns a cached handle — system fonts are deduplicated, explaining why callers do not see memory growth with named fonts.",
        "relevance": "related"
      }
    ],
    "keySignals": [
      {
        "text": "Skia is caching glyphs data and is only clearing the cache if it reaches some defined memory usage level.",
        "source": "comment by contributor @Gillibald",
        "interpretation": "Confirms by-design caching behavior, not a raw memory leak."
      },
      {
        "text": "SKGraphics.PurgeFontCache(); — You can also set your own limits in the cache via SKGraphics.SetFontCacheCountLimit / SetFontCacheLimit",
        "source": "comment by @themcoo",
        "interpretation": "Confirmed workaround using existing public API."
      },
      {
        "text": "Huge thanks for your solution. It's solved the problem in attached simple app.",
        "source": "reporter reply to @themcoo",
        "interpretation": "PurgeFontCache workaround confirmed working by the original reporter."
      },
      {
        "text": "At certain stage, app crashed with out of memory problem.",
        "source": "reporter comment explaining Word-to-PDF batch conversion use case",
        "interpretation": "Real-world severity: OOM crash in batch document processing with unique embedded fonts per document."
      }
    ],
    "workarounds": [
      "Call SKGraphics.PurgeFontCache() after each document/batch to force-release cached glyph data.",
      "Call SKGraphics.SetFontCacheCountLimit(n) to cap the number of typeface entries in the cache.",
      "Call SKGraphics.SetFontCacheLimit(bytes) to cap total cache memory (e.g., SKGraphics.SetFontCacheLimit(1024 * 1024) for 1 MB).",
      "Cache SKTypeface instances in a static dictionary keyed by font identity (file path or stream hash) to avoid creating duplicate entries."
    ],
    "nextQuestions": [
      "Does SKGraphics.PurgeFontCache() also resolve the issue for @afruzan who reported it did not help with FromFile?",
      "Should SKTypeface.Dispose() automatically call PurgeFontCache for stream-loaded typefaces to match user expectations?"
    ],
    "resolution": {
      "hypothesis": "The memory growth is caused by Skia's global glyph cache accumulating entries for each unique stream-loaded typeface. Disposing the SKTypeface only frees the typeface object, not its glyph cache entries. The cache is evicted automatically only when it exceeds a size threshold.",
      "proposals": [
        {
          "title": "Purge font cache after batch",
          "description": "Call SKGraphics.PurgeFontCache() at the end of each document conversion to release all cached glyph data.",
          "category": "workaround",
          "codeSnippet": "// After each document conversion:\nSKGraphics.PurgeFontCache();\n\n// Or set a tight limit at app startup:\nSKGraphics.SetFontCacheCountLimit(2);\nSKGraphics.SetFontCacheLimit(4 * 1024 * 1024); // 4 MB",
          "confidence": 0.9,
          "effort": "cost/xs",
          "validated": "yes"
        },
        {
          "title": "Cache and reuse SKTypeface instances",
          "description": "Keep a static dictionary of SKTypeface instances keyed by font identity. Reuse existing typefaces instead of creating new ones from stream each iteration. Only applicable when fonts repeat across documents.",
          "category": "alternative",
          "codeSnippet": "private static readonly ConcurrentDictionary<string, SKTypeface> _typefaceCache = new();\n\npublic static SKTypeface GetTypeface(string fontKey, Func<SKTypeface> factory)\n    => _typefaceCache.GetOrAdd(fontKey, _ => factory());",
          "confidence": 0.8,
          "effort": "cost/s",
          "validated": "yes"
        }
      ],
      "recommendedProposal": "Purge font cache after batch",
      "recommendedReason": "Directly addresses the batch document scenario with minimal code change. Confirmed working by the original reporter."
    }
  },
  "output": {
    "actionability": {
      "suggestedAction": "close-as-not-a-bug",
      "confidence": 0.78,
      "reason": "The memory growth is Skia's intentional glyph caching behavior, not a SkiaSharp bug. The public API (SKGraphics.PurgeFontCache, SetFontCacheLimit) provides full control, and the reporter confirmed the workaround resolves the issue.",
      "suggestedReproPlatform": "linux"
    },
    "actions": [
      {
        "type": "update-labels",
        "description": "Apply type/bug, area/SkiaSharp, os/Windows-Classic, tenet/reliability",
        "risk": "low",
        "confidence": 0.9,
        "labels": [
          "type/bug",
          "area/SkiaSharp",
          "os/Windows-Classic",
          "tenet/reliability"
        ]
      },
      {
        "type": "add-comment",
        "description": "Explain glyph cache behavior and provide PurgeFontCache workaround",
        "risk": "medium",
        "confidence": 0.85,
        "comment": "Thanks for the detailed report and repro!\n\nThe memory growth you're seeing is caused by Skia's **internal glyph cache** (`SkStrikeCache`). When `MeasureText` is called, Skia renders and caches the glyph metrics for that typeface globally — this cache is not released when you dispose the `SKTypeface`. It's a performance optimization (avoid re-rasterizing the same glyphs), but in a batch scenario with many unique stream-loaded fonts it can grow significantly.\n\nSkiaSharp exposes the cache control APIs you need via `SKGraphics`:\n\n```csharp\n// Option 1: Purge cache after each document conversion\nSKGraphics.PurgeFontCache();\n\n// Option 2: Cap cache size at app startup (e.g., 4 MB)\nSKGraphics.SetFontCacheLimit(4 * 1024 * 1024);\n\n// Option 3: Cap number of cached typefaces\nSKGraphics.SetFontCacheCountLimit(10);\n```\n\nFor your Word-to-PDF scenario, calling `SKGraphics.PurgeFontCache()` after each document conversion should keep memory bounded.\n\nSince this behavior is by design in Skia and the workaround is available via the public API, I'll close this issue. Please reopen if `PurgeFontCache` doesn't fully resolve your use case."
      },
      {
        "type": "close-issue",
        "description": "Close as not a bug — by-design Skia glyph caching with documented workaround",
        "risk": "medium",
        "confidence": 0.78,
        "stateReason": "not_planned"
      }
    ]
  }
}
```

</details>
