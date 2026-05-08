# Issue Triage Report — #648

| Field | Value |
|-------|-------|
| Repository | mono/SkiaSharp |
| Analyzed | 2026-04-22T14:37:02Z |
| Type | type/bug (0.92 (92%)) |
| Area | area/SkiaSharp (0.80 (80%)) |
| Suggested action | needs-investigation (0.80 (80%)) |

**Issue Summary:** Very large images (e.g., 14034×9921 px) silently produce a blank white canvas when drawn via SKSurface, and SKBitmap pixel allocation failure throws a non-intuitive System.Exception instead of OutOfMemoryException.

**Analysis:** Large image rendering produces blank output due to a Skia-level size/allocation limit, while the C# wrapper throws System.Exception instead of OutOfMemoryException on bitmap allocation failure; additionally, no GC.AddMemoryPressure is registered for native bitmap memory.

**Recommendations:** **needs-investigation** — The blank-canvas root cause requires verifying behavior in the current Skia version; meanwhile the exception-type fix in SKBitmap is clearly actionable.

---

## Classification

| Field | Value |
|-------|-------|
| Type | type/bug |
| Area | area/SkiaSharp |
| Platforms | — |
| Backends | — |
| Tenets | tenet/reliability |
| Partner | — |
| Current labels | type/bug, status/skia-update-required, area/libSkiaSharp.native |

## Evidence

### Reproduction

1. Load an image >6 MB (e.g., 14034x9921 px) into SKMemoryStream
2. Create SKImage.FromEncodedData from the stream
3. Create SKSurface.Create(info) with target dimensions
4. Draw the image with DrawImage, call Flush, then Snapshot
5. Observe blank white output with no exception — or catch 'Unable to allocate pixels for the bitmap' when using SKBitmap constructor

**Environment:** Multiple platforms including WPF .NET 4.5 on Windows 10 (SkiaSharp 1.68.2.1) and unspecified.

**Related issues:** #1249

**Repository links:**
- https://github.com/mono/SkiaSharp/issues/648#issuecomment-427031919 — Maintainer identifies root cause as upstream Skia: https://groups.google.com/forum/#!topic/skia-discuss/886WuVcQUGo
- https://stackoverflow.com/questions/52206463/large-image-resizing-shows-blank — Stack Overflow report of the same blank-output symptom

### Bug Signals

| Field | Value |
|-------|-------|
| Severity | medium |
| Regression claimed | False |
| Error type | wrong-output |
| Error message | Unable to allocate pixels for the bitmap. |
| Repro quality | partial |
| Target frameworks | — |

### Version Analysis

| Field | Value |
|-------|-------|
| Mentioned versions | 1.68.2.1 |
| Worked in | — |
| Broke in | — |
| Current relevance | unknown |
| Relevance reason | The Skia native limit was identified in 2018 and tagged status/skia-update-required; whether the current bundled Skia version has removed the restriction is unverified. |

## Analysis

### Technical Summary

Large image rendering produces blank output due to a Skia-level size/allocation limit, while the C# wrapper throws System.Exception instead of OutOfMemoryException on bitmap allocation failure; additionally, no GC.AddMemoryPressure is registered for native bitmap memory.

### Rationale

The blank-canvas behavior was attributed to an upstream Skia internal limit by the maintainer who tagged this status/skia-update-required; verifying whether the current Skia build has resolved this is the primary open question. The secondary problem — throwing System.Exception('Unable to allocate pixels for the bitmap') instead of OutOfMemoryException — is a confirmed C#-wrapper defect visible in SKBitmap.cs lines 52-66. A third concern raised in comments is absent GC.AddMemoryPressure, meaning the GC is unaware of large native memory allocations and may defer collection.

### Key Signals

- "the image itself doesn't show upon save, only the white background canvas" — **issue body** (Silent failure: large image draws as blank without any exception.)
- "I believe this issue is actually a skia one" — **comment by mattleibow (2018-10-04)** (Maintainer attributes the blank-canvas root cause to an upstream Skia native library limit.)
- "Unable to allocate pixels for the bitmap" — **issue body and comment by perumaal (2020-05-11)** (SKBitmap constructor throws System.Exception; OutOfMemoryException would be more appropriate per .NET conventions.)
- "Yes that really should throw an OutOfMemory exception. Also, I am not sure if SkiaSharp adds native memory pressure" — **comment by ziriax (2020-05-12)** (Two actionable C#-wrapper improvements: correct exception type and GC.AddMemoryPressure.)

### Code Investigation

| File | Lines | Relevance | Finding |
|------|-------|-----------|---------|
| `binding/SkiaSharp/SKBitmap.cs` | 52-66 | direct | SKBitmap(SKImageInfo, int rowBytes) and SKBitmap(SKImageInfo, SKBitmapAllocFlags) both throw 'throw new Exception(UnableToAllocatePixelsMessage)' on allocation failure; the message constant at line 17 reads 'Unable to allocate pixels for the bitmap.' — should be OutOfMemoryException. |
| `binding/SkiaSharp/SKBitmap.cs` | 76-91 | related | TryAllocPixels delegates to native sk_bitmap_try_alloc_pixels / sk_bitmap_try_alloc_pixels_with_flags with no GC.AddMemoryPressure call; large allocations are invisible to the .NET GC. |

### Workarounds

- Use SKImage.FromEncodedData() + DrawImage on an SKSurface instead of SKBitmap.Decode() — slower but avoids the bitmap allocation path (confirmed by @nor0x)
- Call TryAllocPixels(info) and inspect the bool return value to detect failure gracefully instead of catching Exception
- Pre-scale image dimensions before allocation to keep the bitmap size within available memory

### Next Questions

- Has the upstream Skia image-size limit been raised or removed in the Skia milestone currently bundled with SkiaSharp 3.x?
- Should SKBitmap constructors throw OutOfMemoryException (with the existing message) instead of plain Exception?
- Should successful pixel allocation register native memory via GC.AddMemoryPressure to prompt timely GC collection?

### Resolution Proposals

**Hypothesis:** The blank-canvas issue may have been resolved in a newer Skia build; the most immediately actionable SkiaSharp-level fix is changing Exception to OutOfMemoryException in SKBitmap constructors.

1. **Throw OutOfMemoryException on allocation failure** — fix, confidence 0.90 (90%), cost/xs, validated=untested
   - In SKBitmap constructors (lines 55-56 and 63-64 of SKBitmap.cs), replace 'throw new Exception(UnableToAllocatePixelsMessage)' with 'throw new OutOfMemoryException(UnableToAllocatePixelsMessage)' to follow .NET conventions.
2. **Verify large-image behavior in current Skia build** — investigation, confidence 0.75 (75%), cost/s, validated=untested
   - Test whether the current bundled Skia version still silently fails to draw very large decoded images, confirming whether status/skia-update-required can be removed.
3. **Add GC.AddMemoryPressure for native bitmap allocations** — fix, confidence 0.70 (70%), cost/m, validated=untested
   - After successful pixel allocation in TryAllocPixels, call GC.AddMemoryPressure(byteSize) and pair with GC.RemoveMemoryPressure in DisposeNative so the GC schedules collection sooner when large native bitmaps accumulate.

**Recommended proposal:** Throw OutOfMemoryException on allocation failure

**Why:** Simplest actionable fix with high confidence; improves developer experience immediately and is independent of the upstream Skia native fix.

## Recommendations

### Actionability

| Field | Value |
|-------|-------|
| Suggested action | needs-investigation |
| Confidence | 0.80 (80%) |
| Reason | The blank-canvas root cause requires verifying behavior in the current Skia version; meanwhile the exception-type fix in SKBitmap is clearly actionable. |
| Suggested repro platform | linux |

### Automatable Actions

| Type | Risk | Confidence | Description | Details |
|------|------|------------|-------------|---------|
| update-labels | low | 0.90 (90%) | Apply correct type, area, and tenet labels | labels=type/bug, area/SkiaSharp, tenet/reliability |
| add-comment | medium | 0.80 (80%) | Inform of root cause split and actionable C#-level fixes with workarounds | — |

**Comment draft for `add-comment`:**

```markdown
Thanks for the detailed report and code sample.

This issue has two distinct components:

**1. Blank canvas for very large images** — The maintainer identified this as an upstream Skia limitation (see the [Skia discussion](https://groups.google.com/forum/#!topic/skia-discuss/886WuVcQUGo)). Verification is needed to confirm whether this limit still exists in the current Skia build.

**2. Non-intuitive exception type** — When `SKBitmap` pixel allocation fails, the C# wrapper throws `System.Exception("Unable to allocate pixels for the bitmap.")` instead of `OutOfMemoryException`. This is an actionable fix within SkiaSharp.

**Workarounds available now:**
- Use `SKImage.FromEncodedData()` + `DrawImage()` on an `SKSurface` instead of `SKBitmap.Decode()` — slower but avoids the allocation path (confirmed working by @nor0x)
- Use `TryAllocPixels(info)` and inspect the `bool` return value to detect failure gracefully without catching a generic exception
- Pre-scale image dimensions before allocation to keep the pixel buffer within available memory
```

<details>
<summary>Raw JSON</summary>

```json
{
  "meta": {
    "schemaVersion": "1.0",
    "number": 648,
    "repo": "mono/SkiaSharp",
    "analyzedAt": "2026-04-22T14:37:02Z",
    "currentLabels": [
      "type/bug",
      "status/skia-update-required",
      "area/libSkiaSharp.native"
    ]
  },
  "summary": "Very large images (e.g., 14034×9921 px) silently produce a blank white canvas when drawn via SKSurface, and SKBitmap pixel allocation failure throws a non-intuitive System.Exception instead of OutOfMemoryException.",
  "classification": {
    "type": {
      "value": "type/bug",
      "confidence": 0.92
    },
    "area": {
      "value": "area/SkiaSharp",
      "confidence": 0.8
    },
    "tenets": [
      "tenet/reliability"
    ]
  },
  "evidence": {
    "bugSignals": {
      "severity": "medium",
      "regressionClaimed": false,
      "errorType": "wrong-output",
      "errorMessage": "Unable to allocate pixels for the bitmap.",
      "reproQuality": "partial"
    },
    "reproEvidence": {
      "stepsToReproduce": [
        "Load an image >6 MB (e.g., 14034x9921 px) into SKMemoryStream",
        "Create SKImage.FromEncodedData from the stream",
        "Create SKSurface.Create(info) with target dimensions",
        "Draw the image with DrawImage, call Flush, then Snapshot",
        "Observe blank white output with no exception — or catch 'Unable to allocate pixels for the bitmap' when using SKBitmap constructor"
      ],
      "environmentDetails": "Multiple platforms including WPF .NET 4.5 on Windows 10 (SkiaSharp 1.68.2.1) and unspecified.",
      "relatedIssues": [
        1249
      ],
      "repoLinks": [
        {
          "url": "https://github.com/mono/SkiaSharp/issues/648#issuecomment-427031919",
          "description": "Maintainer identifies root cause as upstream Skia: https://groups.google.com/forum/#!topic/skia-discuss/886WuVcQUGo"
        },
        {
          "url": "https://stackoverflow.com/questions/52206463/large-image-resizing-shows-blank",
          "description": "Stack Overflow report of the same blank-output symptom"
        }
      ]
    },
    "versionAnalysis": {
      "mentionedVersions": [
        "1.68.2.1"
      ],
      "currentRelevance": "unknown",
      "relevanceReason": "The Skia native limit was identified in 2018 and tagged status/skia-update-required; whether the current bundled Skia version has removed the restriction is unverified."
    }
  },
  "analysis": {
    "summary": "Large image rendering produces blank output due to a Skia-level size/allocation limit, while the C# wrapper throws System.Exception instead of OutOfMemoryException on bitmap allocation failure; additionally, no GC.AddMemoryPressure is registered for native bitmap memory.",
    "rationale": "The blank-canvas behavior was attributed to an upstream Skia internal limit by the maintainer who tagged this status/skia-update-required; verifying whether the current Skia build has resolved this is the primary open question. The secondary problem — throwing System.Exception('Unable to allocate pixels for the bitmap') instead of OutOfMemoryException — is a confirmed C#-wrapper defect visible in SKBitmap.cs lines 52-66. A third concern raised in comments is absent GC.AddMemoryPressure, meaning the GC is unaware of large native memory allocations and may defer collection.",
    "keySignals": [
      {
        "text": "the image itself doesn't show upon save, only the white background canvas",
        "source": "issue body",
        "interpretation": "Silent failure: large image draws as blank without any exception."
      },
      {
        "text": "I believe this issue is actually a skia one",
        "source": "comment by mattleibow (2018-10-04)",
        "interpretation": "Maintainer attributes the blank-canvas root cause to an upstream Skia native library limit."
      },
      {
        "text": "Unable to allocate pixels for the bitmap",
        "source": "issue body and comment by perumaal (2020-05-11)",
        "interpretation": "SKBitmap constructor throws System.Exception; OutOfMemoryException would be more appropriate per .NET conventions."
      },
      {
        "text": "Yes that really should throw an OutOfMemory exception. Also, I am not sure if SkiaSharp adds native memory pressure",
        "source": "comment by ziriax (2020-05-12)",
        "interpretation": "Two actionable C#-wrapper improvements: correct exception type and GC.AddMemoryPressure."
      }
    ],
    "codeInvestigation": [
      {
        "file": "binding/SkiaSharp/SKBitmap.cs",
        "lines": "52-66",
        "finding": "SKBitmap(SKImageInfo, int rowBytes) and SKBitmap(SKImageInfo, SKBitmapAllocFlags) both throw 'throw new Exception(UnableToAllocatePixelsMessage)' on allocation failure; the message constant at line 17 reads 'Unable to allocate pixels for the bitmap.' — should be OutOfMemoryException.",
        "relevance": "direct"
      },
      {
        "file": "binding/SkiaSharp/SKBitmap.cs",
        "lines": "76-91",
        "finding": "TryAllocPixels delegates to native sk_bitmap_try_alloc_pixels / sk_bitmap_try_alloc_pixels_with_flags with no GC.AddMemoryPressure call; large allocations are invisible to the .NET GC.",
        "relevance": "related"
      }
    ],
    "workarounds": [
      "Use SKImage.FromEncodedData() + DrawImage on an SKSurface instead of SKBitmap.Decode() — slower but avoids the bitmap allocation path (confirmed by @nor0x)",
      "Call TryAllocPixels(info) and inspect the bool return value to detect failure gracefully instead of catching Exception",
      "Pre-scale image dimensions before allocation to keep the bitmap size within available memory"
    ],
    "nextQuestions": [
      "Has the upstream Skia image-size limit been raised or removed in the Skia milestone currently bundled with SkiaSharp 3.x?",
      "Should SKBitmap constructors throw OutOfMemoryException (with the existing message) instead of plain Exception?",
      "Should successful pixel allocation register native memory via GC.AddMemoryPressure to prompt timely GC collection?"
    ],
    "resolution": {
      "hypothesis": "The blank-canvas issue may have been resolved in a newer Skia build; the most immediately actionable SkiaSharp-level fix is changing Exception to OutOfMemoryException in SKBitmap constructors.",
      "proposals": [
        {
          "title": "Throw OutOfMemoryException on allocation failure",
          "description": "In SKBitmap constructors (lines 55-56 and 63-64 of SKBitmap.cs), replace 'throw new Exception(UnableToAllocatePixelsMessage)' with 'throw new OutOfMemoryException(UnableToAllocatePixelsMessage)' to follow .NET conventions.",
          "category": "fix",
          "confidence": 0.9,
          "effort": "cost/xs",
          "validated": "untested"
        },
        {
          "title": "Verify large-image behavior in current Skia build",
          "description": "Test whether the current bundled Skia version still silently fails to draw very large decoded images, confirming whether status/skia-update-required can be removed.",
          "category": "investigation",
          "confidence": 0.75,
          "effort": "cost/s",
          "validated": "untested"
        },
        {
          "title": "Add GC.AddMemoryPressure for native bitmap allocations",
          "description": "After successful pixel allocation in TryAllocPixels, call GC.AddMemoryPressure(byteSize) and pair with GC.RemoveMemoryPressure in DisposeNative so the GC schedules collection sooner when large native bitmaps accumulate.",
          "category": "fix",
          "confidence": 0.7,
          "effort": "cost/m",
          "validated": "untested"
        }
      ],
      "recommendedProposal": "Throw OutOfMemoryException on allocation failure",
      "recommendedReason": "Simplest actionable fix with high confidence; improves developer experience immediately and is independent of the upstream Skia native fix."
    }
  },
  "output": {
    "actionability": {
      "suggestedAction": "needs-investigation",
      "confidence": 0.8,
      "reason": "The blank-canvas root cause requires verifying behavior in the current Skia version; meanwhile the exception-type fix in SKBitmap is clearly actionable.",
      "suggestedReproPlatform": "linux"
    },
    "actions": [
      {
        "type": "update-labels",
        "description": "Apply correct type, area, and tenet labels",
        "risk": "low",
        "confidence": 0.9,
        "labels": [
          "type/bug",
          "area/SkiaSharp",
          "tenet/reliability"
        ]
      },
      {
        "type": "add-comment",
        "description": "Inform of root cause split and actionable C#-level fixes with workarounds",
        "risk": "medium",
        "confidence": 0.8,
        "comment": "Thanks for the detailed report and code sample.\n\nThis issue has two distinct components:\n\n**1. Blank canvas for very large images** — The maintainer identified this as an upstream Skia limitation (see the [Skia discussion](https://groups.google.com/forum/#!topic/skia-discuss/886WuVcQUGo)). Verification is needed to confirm whether this limit still exists in the current Skia build.\n\n**2. Non-intuitive exception type** — When `SKBitmap` pixel allocation fails, the C# wrapper throws `System.Exception(\"Unable to allocate pixels for the bitmap.\")` instead of `OutOfMemoryException`. This is an actionable fix within SkiaSharp.\n\n**Workarounds available now:**\n- Use `SKImage.FromEncodedData()` + `DrawImage()` on an `SKSurface` instead of `SKBitmap.Decode()` — slower but avoids the allocation path (confirmed working by @nor0x)\n- Use `TryAllocPixels(info)` and inspect the `bool` return value to detect failure gracefully without catching a generic exception\n- Pre-scale image dimensions before allocation to keep the pixel buffer within available memory"
      }
    ]
  }
}
```

</details>
