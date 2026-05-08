# Issue Triage Report — #1288

| Field | Value |
|-------|-------|
| Repository | mono/SkiaSharp |
| Analyzed | 2026-04-26T21:02:43Z |
| Type | type/enhancement (0.92 (92%)) |
| Area | area/SkiaSharp (0.95 (95%)) |
| Suggested action | needs-investigation (0.80 (80%)) |

**Issue Summary:** Feature request to call GC.AddMemoryPressure and GC.RemoveMemoryPressure on large native SkiaSharp objects (bitmaps, images, surfaces, data) so the .NET GC schedules collections more aggressively when native memory is under pressure.

**Analysis:** The .NET GC is unaware of large native memory held by SkiaSharp objects such as SKBitmap, SKImage, SKData, and SKSurface. Adding GC.AddMemoryPressure when allocating and GC.RemoveMemoryPressure when disposing would help the GC schedule collections more aggressively when native memory is constrained. The main complexity is correctly tracking shared pixel buffers (SKPixelRef) used by SKBitmap to avoid double-counting pressure when subsets are extracted.

**Recommendations:** **needs-investigation** — The enhancement is valid and acknowledged by the maintainer, but the shared pixel buffer complexity requires design investigation before implementation can proceed.

---

## Classification

| Field | Value |
|-------|-------|
| Type | type/enhancement |
| Area | area/SkiaSharp |
| Platforms | — |
| Backends | — |
| Tenets | tenet/reliability, tenet/performance |
| Partner | — |

## Evidence

### Reproduction

**Environment:** Cross-platform; affects all platforms using SkiaSharp native wrappers

### Version Analysis

| Field | Value |
|-------|-------|
| Mentioned versions | — |
| Worked in | — |
| Broke in | — |
| Current relevance | likely |
| Relevance reason | No GC.AddMemoryPressure calls were found anywhere in the binding codebase as of the current HEAD. The v2.88.x Planning milestone (which included this issue) is now closed without this feature being implemented. |

## Analysis

### Technical Summary

The .NET GC is unaware of large native memory held by SkiaSharp objects such as SKBitmap, SKImage, SKData, and SKSurface. Adding GC.AddMemoryPressure when allocating and GC.RemoveMemoryPressure when disposing would help the GC schedule collections more aggressively when native memory is constrained. The main complexity is correctly tracking shared pixel buffers (SKPixelRef) used by SKBitmap to avoid double-counting pressure when subsets are extracted.

### Rationale

This is a genuine enhancement request to improve memory management behaviour. The .NET GC only knows about managed heap allocations; native memory held by SkiaSharp objects is invisible to it. Adding GC memory pressure hints is a well-known pattern for P/Invoke wrappers managing large native allocations. The maintainer explicitly acknowledged the request as valid. The feature has not been implemented — no GC.AddMemoryPressure calls exist in the binding. The reporter's complexity concern about shared pixel buffers (SKPixelRef) is valid and requires design consideration.

### Key Signals

- "We probably should do this soon. We have image, bitmap, surface, data and memory streams." — **comment by maintainer mattleibow (issue #1288)** (Maintainer has acknowledged the request and considers it valid and actionable.)
- "This might be more tricky than it sounds. One really needs to track the memory usage of the underlying shared pixel buffer" — **comment by reporter ziriax (issue #1288)** (The reporter identified key complexity: SKBitmap subset operations share the same SKPixelRef memory, so naively adding pressure per-bitmap would double-count.)

### Code Investigation

| File | Lines | Relevance | Finding |
|------|-------|-----------|---------|
| `binding/SkiaSharp/SKObject.cs` | 212-291 | direct | SKNativeObject is the base class for all native wrappers. Its Dispose method calls DisposeNative() to free native memory. No GC.AddMemoryPressure or GC.RemoveMemoryPressure calls exist anywhere in this class hierarchy. |
| `binding/SkiaSharp/SKBitmap.cs` | 14-79 | direct | SKBitmap constructors allocate native pixel memory (SkiaApi.sk_bitmap_new, TryAllocPixels) but make no GC.AddMemoryPressure call. DisposeNative calls sk_bitmap_destructor. The public ByteCount property (int) reports the native pixel buffer size. SKBitmap can share pixel buffers via SKPixelRef when subsets are extracted. |
| `binding/SkiaSharp/SKImage.cs` | 16-35 | related | SKImage implements ISKReferenceCounted. The Create() factory allocates via Marshal.AllocCoTaskMem but does not report memory pressure to the GC. No AddMemoryPressure calls found. |
| `binding/SkiaSharp/SKData.cs` | 11-80 | related | SKData implements ISKNonVirtualReferenceCounted. The public Size property (long) reports the native data size. No memory pressure tracking. Can hold large blobs of encoded image data. |

### Workarounds

- Manually call GC.AddMemoryPressure(bitmap.ByteCount) after creating an SKBitmap and GC.RemoveMemoryPressure(byteCount) after disposing it as a user-side interim workaround.

### Next Questions

- Should memory pressure be tracked at the SKPixelRef level to handle shared pixel buffers, or per-object with deduplication?
- What is the correct byte size to report for SKImage and SKSurface whose native memory size isn't trivially computable from a single property?

### Resolution Proposals

**Hypothesis:** Add GC.AddMemoryPressure in constructors/factories of large-memory SkiaSharp objects and GC.RemoveMemoryPressure in DisposeNative, with shared pixel buffer tracking for SKBitmap to avoid double-counting.

1. **Library-level memory pressure in SKBitmap, SKImage, SKData, SKSurface** — fix, confidence 0.75 (75%), cost/m, validated=untested
   - In each large-object constructor/factory, call GC.AddMemoryPressure(byteSize) after successful native allocation. In DisposeNative(), call GC.RemoveMemoryPressure(byteSize). For SKBitmap, use ByteCount. For SKData, use Size. For SKImage and SKSurface, estimate from width*height*bytesPerPixel. Handle shared pixel buffers in SKBitmap carefully.
2. **User-side manual memory pressure workaround** — workaround, confidence 0.90 (90%), cost/xs, validated=yes
   - Application code can manually call GC.AddMemoryPressure after creating a bitmap and GC.RemoveMemoryPressure after disposing it as an interim measure until the library implements this natively.

```csharp
var bitmap = new SKBitmap(width, height);
GC.AddMemoryPressure(bitmap.ByteCount);
try
{
    // use bitmap
}
finally
{
    long byteCount = bitmap.ByteCount;
    bitmap.Dispose();
    GC.RemoveMemoryPressure(byteCount);
}
```

**Recommended proposal:** Library-level memory pressure in SKBitmap, SKImage, SKData, SKSurface

**Why:** The enhancement is well-understood and the maintainer already agreed to it. Implementing at the library level means all callers benefit automatically without any application-level changes.

## Recommendations

### Actionability

| Field | Value |
|-------|-------|
| Suggested action | needs-investigation |
| Confidence | 0.80 (80%) |
| Reason | The enhancement is valid and acknowledged by the maintainer, but the shared pixel buffer complexity requires design investigation before implementation can proceed. |
| Suggested repro platform | linux |

### Automatable Actions

| Type | Risk | Confidence | Description | Details |
|------|------|------------|-------------|---------|
| update-labels | low | 0.92 (92%) | Apply enhancement, area, and tenet labels | labels=type/enhancement, area/SkiaSharp, tenet/reliability, tenet/performance |
| add-comment | medium | 0.80 (80%) | Acknowledge the enhancement and summarize design considerations with workaround | — |

**Comment draft for `add-comment`:**

```markdown
Thanks for the suggestion! This is a valid enhancement — calling `GC.AddMemoryPressure` / `GC.RemoveMemoryPressure` on large native objects like `SKBitmap`, `SKImage`, `SKData`, and `SKSurface` would help the .NET GC schedule collections more appropriately.

The primary design challenge is handling shared pixel buffers: when `SKBitmap` is used with subset operations, multiple bitmaps share the same `SKPixelRef` memory block. Naïve per-object tracking would double-count. One approach is to track memory pressure at the `SKPixelRef` level rather than per bitmap instance.

In the meantime, you can apply memory pressure manually as a workaround:

```csharp
var bitmap = new SKBitmap(width, height);
GC.AddMemoryPressure(bitmap.ByteCount);
try
{
    // use bitmap
}
finally
{
    long byteCount = bitmap.ByteCount;
    bitmap.Dispose();
    GC.RemoveMemoryPressure(byteCount);
}
```
```

<details>
<summary>Raw JSON</summary>

```json
{
  "meta": {
    "schemaVersion": "1.0",
    "number": 1288,
    "repo": "mono/SkiaSharp",
    "analyzedAt": "2026-04-26T21:02:43Z"
  },
  "summary": "Feature request to call GC.AddMemoryPressure and GC.RemoveMemoryPressure on large native SkiaSharp objects (bitmaps, images, surfaces, data) so the .NET GC schedules collections more aggressively when native memory is under pressure.",
  "classification": {
    "type": {
      "value": "type/enhancement",
      "confidence": 0.92
    },
    "area": {
      "value": "area/SkiaSharp",
      "confidence": 0.95
    },
    "tenets": [
      "tenet/reliability",
      "tenet/performance"
    ]
  },
  "evidence": {
    "reproEvidence": {
      "environmentDetails": "Cross-platform; affects all platforms using SkiaSharp native wrappers"
    },
    "versionAnalysis": {
      "mentionedVersions": [],
      "currentRelevance": "likely",
      "relevanceReason": "No GC.AddMemoryPressure calls were found anywhere in the binding codebase as of the current HEAD. The v2.88.x Planning milestone (which included this issue) is now closed without this feature being implemented."
    }
  },
  "analysis": {
    "summary": "The .NET GC is unaware of large native memory held by SkiaSharp objects such as SKBitmap, SKImage, SKData, and SKSurface. Adding GC.AddMemoryPressure when allocating and GC.RemoveMemoryPressure when disposing would help the GC schedule collections more aggressively when native memory is constrained. The main complexity is correctly tracking shared pixel buffers (SKPixelRef) used by SKBitmap to avoid double-counting pressure when subsets are extracted.",
    "keySignals": [
      {
        "text": "We probably should do this soon. We have image, bitmap, surface, data and memory streams.",
        "source": "comment by maintainer mattleibow (issue #1288)",
        "interpretation": "Maintainer has acknowledged the request and considers it valid and actionable."
      },
      {
        "text": "This might be more tricky than it sounds. One really needs to track the memory usage of the underlying shared pixel buffer",
        "source": "comment by reporter ziriax (issue #1288)",
        "interpretation": "The reporter identified key complexity: SKBitmap subset operations share the same SKPixelRef memory, so naively adding pressure per-bitmap would double-count."
      }
    ],
    "codeInvestigation": [
      {
        "file": "binding/SkiaSharp/SKObject.cs",
        "lines": "212-291",
        "finding": "SKNativeObject is the base class for all native wrappers. Its Dispose method calls DisposeNative() to free native memory. No GC.AddMemoryPressure or GC.RemoveMemoryPressure calls exist anywhere in this class hierarchy.",
        "relevance": "direct"
      },
      {
        "file": "binding/SkiaSharp/SKBitmap.cs",
        "lines": "14-79",
        "finding": "SKBitmap constructors allocate native pixel memory (SkiaApi.sk_bitmap_new, TryAllocPixels) but make no GC.AddMemoryPressure call. DisposeNative calls sk_bitmap_destructor. The public ByteCount property (int) reports the native pixel buffer size. SKBitmap can share pixel buffers via SKPixelRef when subsets are extracted.",
        "relevance": "direct"
      },
      {
        "file": "binding/SkiaSharp/SKImage.cs",
        "lines": "16-35",
        "finding": "SKImage implements ISKReferenceCounted. The Create() factory allocates via Marshal.AllocCoTaskMem but does not report memory pressure to the GC. No AddMemoryPressure calls found.",
        "relevance": "related"
      },
      {
        "file": "binding/SkiaSharp/SKData.cs",
        "lines": "11-80",
        "finding": "SKData implements ISKNonVirtualReferenceCounted. The public Size property (long) reports the native data size. No memory pressure tracking. Can hold large blobs of encoded image data.",
        "relevance": "related"
      }
    ],
    "rationale": "This is a genuine enhancement request to improve memory management behaviour. The .NET GC only knows about managed heap allocations; native memory held by SkiaSharp objects is invisible to it. Adding GC memory pressure hints is a well-known pattern for P/Invoke wrappers managing large native allocations. The maintainer explicitly acknowledged the request as valid. The feature has not been implemented — no GC.AddMemoryPressure calls exist in the binding. The reporter's complexity concern about shared pixel buffers (SKPixelRef) is valid and requires design consideration.",
    "nextQuestions": [
      "Should memory pressure be tracked at the SKPixelRef level to handle shared pixel buffers, or per-object with deduplication?",
      "What is the correct byte size to report for SKImage and SKSurface whose native memory size isn't trivially computable from a single property?"
    ],
    "workarounds": [
      "Manually call GC.AddMemoryPressure(bitmap.ByteCount) after creating an SKBitmap and GC.RemoveMemoryPressure(byteCount) after disposing it as a user-side interim workaround."
    ],
    "resolution": {
      "hypothesis": "Add GC.AddMemoryPressure in constructors/factories of large-memory SkiaSharp objects and GC.RemoveMemoryPressure in DisposeNative, with shared pixel buffer tracking for SKBitmap to avoid double-counting.",
      "proposals": [
        {
          "title": "Library-level memory pressure in SKBitmap, SKImage, SKData, SKSurface",
          "description": "In each large-object constructor/factory, call GC.AddMemoryPressure(byteSize) after successful native allocation. In DisposeNative(), call GC.RemoveMemoryPressure(byteSize). For SKBitmap, use ByteCount. For SKData, use Size. For SKImage and SKSurface, estimate from width*height*bytesPerPixel. Handle shared pixel buffers in SKBitmap carefully.",
          "category": "fix",
          "confidence": 0.75,
          "effort": "cost/m",
          "validated": "untested"
        },
        {
          "title": "User-side manual memory pressure workaround",
          "description": "Application code can manually call GC.AddMemoryPressure after creating a bitmap and GC.RemoveMemoryPressure after disposing it as an interim measure until the library implements this natively.",
          "codeSnippet": "var bitmap = new SKBitmap(width, height);\nGC.AddMemoryPressure(bitmap.ByteCount);\ntry\n{\n    // use bitmap\n}\nfinally\n{\n    long byteCount = bitmap.ByteCount;\n    bitmap.Dispose();\n    GC.RemoveMemoryPressure(byteCount);\n}",
          "category": "workaround",
          "confidence": 0.9,
          "effort": "cost/xs",
          "validated": "yes"
        }
      ],
      "recommendedProposal": "Library-level memory pressure in SKBitmap, SKImage, SKData, SKSurface",
      "recommendedReason": "The enhancement is well-understood and the maintainer already agreed to it. Implementing at the library level means all callers benefit automatically without any application-level changes."
    }
  },
  "output": {
    "actionability": {
      "suggestedAction": "needs-investigation",
      "confidence": 0.8,
      "reason": "The enhancement is valid and acknowledged by the maintainer, but the shared pixel buffer complexity requires design investigation before implementation can proceed.",
      "suggestedReproPlatform": "linux"
    },
    "actions": [
      {
        "type": "update-labels",
        "description": "Apply enhancement, area, and tenet labels",
        "risk": "low",
        "confidence": 0.92,
        "labels": [
          "type/enhancement",
          "area/SkiaSharp",
          "tenet/reliability",
          "tenet/performance"
        ]
      },
      {
        "type": "add-comment",
        "description": "Acknowledge the enhancement and summarize design considerations with workaround",
        "risk": "medium",
        "confidence": 0.8,
        "comment": "Thanks for the suggestion! This is a valid enhancement — calling `GC.AddMemoryPressure` / `GC.RemoveMemoryPressure` on large native objects like `SKBitmap`, `SKImage`, `SKData`, and `SKSurface` would help the .NET GC schedule collections more appropriately.\n\nThe primary design challenge is handling shared pixel buffers: when `SKBitmap` is used with subset operations, multiple bitmaps share the same `SKPixelRef` memory block. Naïve per-object tracking would double-count. One approach is to track memory pressure at the `SKPixelRef` level rather than per bitmap instance.\n\nIn the meantime, you can apply memory pressure manually as a workaround:\n\n```csharp\nvar bitmap = new SKBitmap(width, height);\nGC.AddMemoryPressure(bitmap.ByteCount);\ntry\n{\n    // use bitmap\n}\nfinally\n{\n    long byteCount = bitmap.ByteCount;\n    bitmap.Dispose();\n    GC.RemoveMemoryPressure(byteCount);\n}\n```"
      }
    ]
  }
}
```

</details>
