# Issue Triage Report — #2681

| Field | Value |
|-------|-------|
| Repository | mono/SkiaSharp |
| Analyzed | 2026-04-25T14:08:53Z |
| Type | type/bug (0.95 (95%)) |
| Area | area/SkiaSharp (0.92 (92%)) |
| Suggested action | needs-investigation (0.82 (82%)) |

**Issue Summary:** Reporter receives System.AccessViolationException when calling SKBitmap.ExtractSubset followed by SKBitmap.Resize in a loop over many images; the crash is non-deterministic and worsens under GC pressure.

**Analysis:** AccessViolationException likely caused by native memory pressure or a use-after-free when calling ExtractSubset on a pre-allocated SKBitmap: the original pixel allocation is replaced but may not be fully freed, and the subset shares pixel memory with the source bitmap without explicit lifetime management. GC finalization race or accumulated memory pressure across loop iterations causes the eventual crash.

**Recommendations:** **needs-investigation** — Real crash with partial repro code. Root cause (memory leak in pre-allocated ExtractSubset vs. finalization race) requires deeper native investigation. Workarounds exist but the underlying cause is unconfirmed.

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
| Current labels | type/bug |

## Evidence

### Reproduction

1. Load a bitmap of decent size into SKBitmap bmp
2. Create a pre-allocated SKBitmap subset with new SKBitmap(rect.Width, rect.Height, ...)
3. Call bmp.ExtractSubset(subset, rect) to make subset share bmp's pixel memory
4. Call subset.Resize(new SKImageInfo(bmp.Width*3, bmp.Height*3, ...), SKFilterQuality.High)
5. Repeat the above in a loop over many images — crash occurs after several iterations

**Environment:** Windows 11, Visual Studio, SkiaSharp 2.88.3

**Related issues:** #2634, #2645, #2456

**Repository links:**
- https://github.com/mono/SkiaSharp/issues/2456 — Same reporter's earlier issue: AccessViolation on SKBitmap.Copy() in multithreaded MAUI context
- https://github.com/mono/SkiaSharp/issues/2634 — Related: SKBitmap.Decode crash in 2.88.6
- https://github.com/mono/SkiaSharp/issues/2645 — Related: AccessViolationException in SKCodec.GetScaledDimensions in 2.88.6

### Bug Signals

| Field | Value |
|-------|-------|
| Severity | high |
| Regression claimed | True |
| Error type | crash |
| Error message | System.AccessViolationException |
| Repro quality | partial |
| Target frameworks | net6.0-windows |

### Version Analysis

| Field | Value |
|-------|-------|
| Mentioned versions | 2.88.3, 2.88.2 |
| Worked in | 2.88.2 |
| Broke in | 2.88.3 |
| Current relevance | unknown |
| Relevance reason | Reporter later stated 'It doesn't seem to matter which version of SkiaSharp I use', contradicting the initial regression claim. The bug may be a pre-existing memory management issue exposed by usage pattern. |

### Regression

| Field | Value |
|-------|-------|
| Is regression | False |
| Confidence | 0.70 (70%) |
| Reason | Reporter retracted the version claim in a follow-up comment, indicating the version attribution was incorrect. The crash is non-deterministic and version-independent. |
| Worked in version | — |
| Broke in version | — |

## Analysis

### Technical Summary

AccessViolationException likely caused by native memory pressure or a use-after-free when calling ExtractSubset on a pre-allocated SKBitmap: the original pixel allocation is replaced but may not be fully freed, and the subset shares pixel memory with the source bitmap without explicit lifetime management. GC finalization race or accumulated memory pressure across loop iterations causes the eventual crash.

### Rationale

Reporter provides repro code showing ExtractSubset called on a pre-allocated SKBitmap (which creates a memory ownership ambiguity), followed by Resize in a loop. The non-deterministic crash behavior, the fact that manual Dispose+GC.Collect delays the crash, and the reporter's link to their own multithreaded issue #2456 all point to memory management rather than a version regression. The code investigation confirms ExtractSubset replaces the destination bitmap's pixels with a shared reference to the source's PixelRef — if the source is GC-finalized while the subset is still in scope, native Skia's ref-counting should protect pixels, but accumulated leaks or threading hazards can still cause crashes over many iterations.

### Key Signals

- "It doesn't happen the first time, but if run it in a loop for several images, it will occur." — **issue body** (Non-deterministic crash under GC/memory pressure — consistent with resource exhaustion or a GC finalization race rather than a deterministic code path bug.)
- "It doesn't seem to matter which version of SkiaSharp I use." — **comment by gktval** (Contradicts the initial regression claim (2.88.2 → 2.88.3). The bug is likely a pre-existing memory management hazard.)
- "if I manually dispose and garbage collect after resizing the image it will mostly fix the issue" — **comment by gktval** (Manual disposal reduces native memory pressure and delays the crash — confirms memory leak or accumulated unreleased resources as a contributing factor.)
- "the error will not occur now after a few dozen iterations. Instead, I can process about a thousand images before I get an AccessViolation error" — **comment by gktval** (Workaround shifts the failure point but doesn't eliminate it — another memory leak exists elsewhere in the caller's code.)

### Code Investigation

| File | Lines | Relevance | Finding |
|------|-------|-----------|---------|
| `binding/SkiaSharp/SKBitmap.cs` | 214-222 | direct | ExtractSubset delegates directly to sk_bitmap_extract_subset(Handle, destination.Handle, &subset). When destination is a pre-allocated SKBitmap (as in the reporter's code), the native call replaces destination's PixelRef with a shared reference to the source's PixelRef. The original pixel allocation made during 'new SKBitmap(width, height)' is discarded at the native C++ level, but no explicit C# tracking of this shared ownership is performed. |
| `binding/SkiaSharp/SKBitmap.cs` | 643-651 | related | PeekPixels sets pixmap.pixelSource = this to keep the bitmap alive during pixel access. However, no analogous mechanism exists between the source bmp and the extracted subset after ExtractSubset — if bmp is GC-eligible, its finalizer can run concurrently with subset operations, even though the underlying C++ PixelRef refcount should prevent the memory from being freed. |
| `binding/SkiaSharp/SKBitmap.cs` | 664-676 | related | Resize(SKImageInfo, SKSamplingOptions) creates a new SKBitmap and calls ScalePixels → PeekPixels on the subset. The pixelSource reference keeps subset alive for the duration of PeekPixels, but does not extend the lifetime of the source bmp whose PixelRef the subset shares. |

### Workarounds

- Explicitly dispose both bmp and subset after each iteration: subset.Dispose(); bmp.Dispose();
- Use SKImage instead of SKBitmap for the source: var image = SKImage.FromBitmap(bmp); var croppedImage = image.Subset(rect); then scale via SKImage or canvas instead of SKBitmap.Resize
- Copy the subset pixels to an independent bitmap before disposing the source: use SKBitmap.Copy() on the subset to produce a fully independent bitmap with its own pixel allocation before calling Resize

### Next Questions

- Is the bitmap processing multithreaded? The reporter's earlier issue #2456 involved multithreaded MAUI bitmap operations.
- Does the crash have a stack trace? None was provided — a stack trace would confirm whether it originates in ExtractSubset, Resize, or GC finalization.
- Does replacing ExtractSubset with a Copy() approach (creating an independent bitmap) eliminate the crash?
- Does the crash reproduce on Linux or macOS, or is it Windows-specific?

### Resolution Proposals

**Hypothesis:** The reporter pre-allocates a SKBitmap with its own pixels, then calls ExtractSubset which replaces those pixels with a shared reference to the source bitmap's PixelRef. The original pixel allocation is abandoned (potential memory leak per iteration). Under high iteration counts, accumulated native memory pressure or a finalization race causes the AccessViolationException.

1. **Explicit disposal of both bitmaps per iteration** — workaround, confidence 0.75 (75%), cost/xs, validated=untested
   - Dispose both the source bitmap and the subset bitmap after each iteration, before the GC has a chance to finalize them non-deterministically.

```csharp
using (SKBitmap bmp = LoadBitmap())
using (SKBitmap subset = new SKBitmap())
{
    bmp.ExtractSubset(subset, rect);
    using var resized = subset.Resize(
        new SKImageInfo(bmp.Width * 3, bmp.Height * 3,
            SKImageInfo.PlatformColorType, SKAlphaType.Unpremul),
        SKSamplingOptions.Default);
    // use resized...
}
```
2. **Copy subset to independent bitmap before resizing** — workaround, confidence 0.80 (80%), cost/s, validated=untested
   - After ExtractSubset, call Copy() on the subset to produce a fully independent bitmap with its own pixel allocation. This breaks the shared-memory dependency on the source bitmap, eliminating the lifetime hazard.

```csharp
using (SKBitmap bmp = LoadBitmap())
{
    var rect = new SKRectI(20, 20, 240, 240);
    SKBitmap shared = new SKBitmap();
    bmp.ExtractSubset(shared, rect);
    using SKBitmap subset = shared.Copy();  // independent copy
    shared.Dispose();
    using var resized = subset.Resize(
        new SKImageInfo(subset.Width * 3, subset.Height * 3,
            SKImageInfo.PlatformColorType, SKAlphaType.Unpremul),
        SKSamplingOptions.Default);
    // use resized...
}
```
3. **Investigate native memory leak in pre-allocated ExtractSubset** — investigation, confidence 0.65 (65%), cost/m, validated=untested
   - Audit the C++ sk_bitmap_extract_subset implementation to confirm whether replacing a pre-allocated bitmap's PixelRef properly releases the old allocation. If there is a native memory leak per call, fix it in the C API layer.

**Recommended proposal:** Copy subset to independent bitmap before resizing

**Why:** Creating an independent copy of the subset pixels eliminates the shared-memory lifetime dependency on the source bitmap entirely, which is the safest approach. The explicit disposal pattern also helps but does not remove the shared-memory hazard during the resize operation itself.

## Recommendations

### Actionability

| Field | Value |
|-------|-------|
| Suggested action | needs-investigation |
| Confidence | 0.82 (82%) |
| Reason | Real crash with partial repro code. Root cause (memory leak in pre-allocated ExtractSubset vs. finalization race) requires deeper native investigation. Workarounds exist but the underlying cause is unconfirmed. |
| Suggested repro platform | linux |

### Missing Info

- Stack trace from the AccessViolationException
- Whether the code is running in a multithreaded context
- Whether the crash reproduces with explicit using blocks (ensuring deterministic disposal)

### Automatable Actions

| Type | Risk | Confidence | Description | Details |
|------|------|------------|-------------|---------|
| update-labels | low | 0.92 (92%) | Apply bug, core area, Windows platform, and reliability tenet labels | labels=type/bug, area/SkiaSharp, os/Windows-Classic, tenet/reliability |
| add-comment | medium | 0.82 (82%) | Post analysis with workarounds and ask for stack trace and threading context | — |
| link-related | low | 0.85 (85%) | Cross-reference reporter's own related issue #2456 (same symptom, multithreaded context) | linkedIssue=#2456 |

**Comment draft for `add-comment`:**

```markdown
Thank you for the detailed report and the workaround you found!

The `AccessViolationException` is most likely caused by a shared-memory lifetime hazard in how `ExtractSubset` is used. When you call `bmp.ExtractSubset(subset, rect)`, the `subset` bitmap shares the underlying pixel memory of `bmp` without making a copy. Over many iterations, either accumulated memory pressure (from the abandoned original pixel allocation in `subset`) or a GC finalization race can corrupt the shared pixel buffer.

**Workaround — break the shared-memory dependency:**

```csharp
using (SKBitmap bmp = LoadBitmap())
{
    var rect = new SKRectI(20, 20, 240, 240);
    // Extract a shared subset, then immediately copy to an independent bitmap
    SKBitmap sharedSubset = new SKBitmap();
    bmp.ExtractSubset(sharedSubset, rect);
    using SKBitmap subset = sharedSubset.Copy();  // independent pixel buffer
    sharedSubset.Dispose();

    using var resized = subset.Resize(
        new SKImageInfo(subset.Width * 3, subset.Height * 3,
            SKImageInfo.PlatformColorType, SKAlphaType.Unpremul),
        SKSamplingOptions.Default);
    // use resized...
}
```

Alternatively, as you found, explicit `Dispose()` after each iteration helps. Using `using` blocks ensures this happens deterministically rather than relying on the GC.

To help investigate the root cause, could you share:
1. The full stack trace from the `AccessViolationException`?
2. Is the bitmap processing running on multiple threads simultaneously?
3. Does the crash go away when you use the `Copy()` approach above?
```

<details>
<summary>Raw JSON</summary>

```json
{
  "meta": {
    "schemaVersion": "1.0",
    "number": 2681,
    "repo": "mono/SkiaSharp",
    "analyzedAt": "2026-04-25T14:08:53Z",
    "currentLabels": [
      "type/bug"
    ]
  },
  "summary": "Reporter receives System.AccessViolationException when calling SKBitmap.ExtractSubset followed by SKBitmap.Resize in a loop over many images; the crash is non-deterministic and worsens under GC pressure.",
  "classification": {
    "type": {
      "value": "type/bug",
      "confidence": 0.95
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
      "severity": "high",
      "regressionClaimed": true,
      "errorType": "crash",
      "errorMessage": "System.AccessViolationException",
      "reproQuality": "partial",
      "targetFrameworks": [
        "net6.0-windows"
      ]
    },
    "reproEvidence": {
      "stepsToReproduce": [
        "Load a bitmap of decent size into SKBitmap bmp",
        "Create a pre-allocated SKBitmap subset with new SKBitmap(rect.Width, rect.Height, ...)",
        "Call bmp.ExtractSubset(subset, rect) to make subset share bmp's pixel memory",
        "Call subset.Resize(new SKImageInfo(bmp.Width*3, bmp.Height*3, ...), SKFilterQuality.High)",
        "Repeat the above in a loop over many images — crash occurs after several iterations"
      ],
      "environmentDetails": "Windows 11, Visual Studio, SkiaSharp 2.88.3",
      "relatedIssues": [
        2634,
        2645,
        2456
      ],
      "repoLinks": [
        {
          "url": "https://github.com/mono/SkiaSharp/issues/2456",
          "description": "Same reporter's earlier issue: AccessViolation on SKBitmap.Copy() in multithreaded MAUI context"
        },
        {
          "url": "https://github.com/mono/SkiaSharp/issues/2634",
          "description": "Related: SKBitmap.Decode crash in 2.88.6"
        },
        {
          "url": "https://github.com/mono/SkiaSharp/issues/2645",
          "description": "Related: AccessViolationException in SKCodec.GetScaledDimensions in 2.88.6"
        }
      ]
    },
    "versionAnalysis": {
      "mentionedVersions": [
        "2.88.3",
        "2.88.2"
      ],
      "workedIn": "2.88.2",
      "brokeIn": "2.88.3",
      "currentRelevance": "unknown",
      "relevanceReason": "Reporter later stated 'It doesn't seem to matter which version of SkiaSharp I use', contradicting the initial regression claim. The bug may be a pre-existing memory management issue exposed by usage pattern."
    },
    "regression": {
      "isRegression": false,
      "confidence": 0.7,
      "reason": "Reporter retracted the version claim in a follow-up comment, indicating the version attribution was incorrect. The crash is non-deterministic and version-independent."
    }
  },
  "analysis": {
    "summary": "AccessViolationException likely caused by native memory pressure or a use-after-free when calling ExtractSubset on a pre-allocated SKBitmap: the original pixel allocation is replaced but may not be fully freed, and the subset shares pixel memory with the source bitmap without explicit lifetime management. GC finalization race or accumulated memory pressure across loop iterations causes the eventual crash.",
    "rationale": "Reporter provides repro code showing ExtractSubset called on a pre-allocated SKBitmap (which creates a memory ownership ambiguity), followed by Resize in a loop. The non-deterministic crash behavior, the fact that manual Dispose+GC.Collect delays the crash, and the reporter's link to their own multithreaded issue #2456 all point to memory management rather than a version regression. The code investigation confirms ExtractSubset replaces the destination bitmap's pixels with a shared reference to the source's PixelRef — if the source is GC-finalized while the subset is still in scope, native Skia's ref-counting should protect pixels, but accumulated leaks or threading hazards can still cause crashes over many iterations.",
    "keySignals": [
      {
        "text": "It doesn't happen the first time, but if run it in a loop for several images, it will occur.",
        "source": "issue body",
        "interpretation": "Non-deterministic crash under GC/memory pressure — consistent with resource exhaustion or a GC finalization race rather than a deterministic code path bug."
      },
      {
        "text": "It doesn't seem to matter which version of SkiaSharp I use.",
        "source": "comment by gktval",
        "interpretation": "Contradicts the initial regression claim (2.88.2 → 2.88.3). The bug is likely a pre-existing memory management hazard."
      },
      {
        "text": "if I manually dispose and garbage collect after resizing the image it will mostly fix the issue",
        "source": "comment by gktval",
        "interpretation": "Manual disposal reduces native memory pressure and delays the crash — confirms memory leak or accumulated unreleased resources as a contributing factor."
      },
      {
        "text": "the error will not occur now after a few dozen iterations. Instead, I can process about a thousand images before I get an AccessViolation error",
        "source": "comment by gktval",
        "interpretation": "Workaround shifts the failure point but doesn't eliminate it — another memory leak exists elsewhere in the caller's code."
      }
    ],
    "codeInvestigation": [
      {
        "file": "binding/SkiaSharp/SKBitmap.cs",
        "lines": "214-222",
        "finding": "ExtractSubset delegates directly to sk_bitmap_extract_subset(Handle, destination.Handle, &subset). When destination is a pre-allocated SKBitmap (as in the reporter's code), the native call replaces destination's PixelRef with a shared reference to the source's PixelRef. The original pixel allocation made during 'new SKBitmap(width, height)' is discarded at the native C++ level, but no explicit C# tracking of this shared ownership is performed.",
        "relevance": "direct"
      },
      {
        "file": "binding/SkiaSharp/SKBitmap.cs",
        "lines": "643-651",
        "finding": "PeekPixels sets pixmap.pixelSource = this to keep the bitmap alive during pixel access. However, no analogous mechanism exists between the source bmp and the extracted subset after ExtractSubset — if bmp is GC-eligible, its finalizer can run concurrently with subset operations, even though the underlying C++ PixelRef refcount should prevent the memory from being freed.",
        "relevance": "related"
      },
      {
        "file": "binding/SkiaSharp/SKBitmap.cs",
        "lines": "664-676",
        "finding": "Resize(SKImageInfo, SKSamplingOptions) creates a new SKBitmap and calls ScalePixels → PeekPixels on the subset. The pixelSource reference keeps subset alive for the duration of PeekPixels, but does not extend the lifetime of the source bmp whose PixelRef the subset shares.",
        "relevance": "related"
      }
    ],
    "workarounds": [
      "Explicitly dispose both bmp and subset after each iteration: subset.Dispose(); bmp.Dispose();",
      "Use SKImage instead of SKBitmap for the source: var image = SKImage.FromBitmap(bmp); var croppedImage = image.Subset(rect); then scale via SKImage or canvas instead of SKBitmap.Resize",
      "Copy the subset pixels to an independent bitmap before disposing the source: use SKBitmap.Copy() on the subset to produce a fully independent bitmap with its own pixel allocation before calling Resize"
    ],
    "nextQuestions": [
      "Is the bitmap processing multithreaded? The reporter's earlier issue #2456 involved multithreaded MAUI bitmap operations.",
      "Does the crash have a stack trace? None was provided — a stack trace would confirm whether it originates in ExtractSubset, Resize, or GC finalization.",
      "Does replacing ExtractSubset with a Copy() approach (creating an independent bitmap) eliminate the crash?",
      "Does the crash reproduce on Linux or macOS, or is it Windows-specific?"
    ],
    "resolution": {
      "hypothesis": "The reporter pre-allocates a SKBitmap with its own pixels, then calls ExtractSubset which replaces those pixels with a shared reference to the source bitmap's PixelRef. The original pixel allocation is abandoned (potential memory leak per iteration). Under high iteration counts, accumulated native memory pressure or a finalization race causes the AccessViolationException.",
      "proposals": [
        {
          "title": "Explicit disposal of both bitmaps per iteration",
          "description": "Dispose both the source bitmap and the subset bitmap after each iteration, before the GC has a chance to finalize them non-deterministically.",
          "category": "workaround",
          "codeSnippet": "using (SKBitmap bmp = LoadBitmap())\nusing (SKBitmap subset = new SKBitmap())\n{\n    bmp.ExtractSubset(subset, rect);\n    using var resized = subset.Resize(\n        new SKImageInfo(bmp.Width * 3, bmp.Height * 3,\n            SKImageInfo.PlatformColorType, SKAlphaType.Unpremul),\n        SKSamplingOptions.Default);\n    // use resized...\n}",
          "validated": "untested",
          "confidence": 0.75,
          "effort": "cost/xs"
        },
        {
          "title": "Copy subset to independent bitmap before resizing",
          "description": "After ExtractSubset, call Copy() on the subset to produce a fully independent bitmap with its own pixel allocation. This breaks the shared-memory dependency on the source bitmap, eliminating the lifetime hazard.",
          "category": "workaround",
          "codeSnippet": "using (SKBitmap bmp = LoadBitmap())\n{\n    var rect = new SKRectI(20, 20, 240, 240);\n    SKBitmap shared = new SKBitmap();\n    bmp.ExtractSubset(shared, rect);\n    using SKBitmap subset = shared.Copy();  // independent copy\n    shared.Dispose();\n    using var resized = subset.Resize(\n        new SKImageInfo(subset.Width * 3, subset.Height * 3,\n            SKImageInfo.PlatformColorType, SKAlphaType.Unpremul),\n        SKSamplingOptions.Default);\n    // use resized...\n}",
          "validated": "untested",
          "confidence": 0.8,
          "effort": "cost/s"
        },
        {
          "title": "Investigate native memory leak in pre-allocated ExtractSubset",
          "description": "Audit the C++ sk_bitmap_extract_subset implementation to confirm whether replacing a pre-allocated bitmap's PixelRef properly releases the old allocation. If there is a native memory leak per call, fix it in the C API layer.",
          "category": "investigation",
          "validated": "untested",
          "confidence": 0.65,
          "effort": "cost/m"
        }
      ],
      "recommendedProposal": "Copy subset to independent bitmap before resizing",
      "recommendedReason": "Creating an independent copy of the subset pixels eliminates the shared-memory lifetime dependency on the source bitmap entirely, which is the safest approach. The explicit disposal pattern also helps but does not remove the shared-memory hazard during the resize operation itself."
    }
  },
  "output": {
    "actionability": {
      "suggestedAction": "needs-investigation",
      "confidence": 0.82,
      "reason": "Real crash with partial repro code. Root cause (memory leak in pre-allocated ExtractSubset vs. finalization race) requires deeper native investigation. Workarounds exist but the underlying cause is unconfirmed.",
      "suggestedReproPlatform": "linux"
    },
    "missingInfo": [
      "Stack trace from the AccessViolationException",
      "Whether the code is running in a multithreaded context",
      "Whether the crash reproduces with explicit using blocks (ensuring deterministic disposal)"
    ],
    "actions": [
      {
        "type": "update-labels",
        "description": "Apply bug, core area, Windows platform, and reliability tenet labels",
        "risk": "low",
        "confidence": 0.92,
        "labels": [
          "type/bug",
          "area/SkiaSharp",
          "os/Windows-Classic",
          "tenet/reliability"
        ]
      },
      {
        "type": "add-comment",
        "description": "Post analysis with workarounds and ask for stack trace and threading context",
        "risk": "medium",
        "confidence": 0.82,
        "comment": "Thank you for the detailed report and the workaround you found!\n\nThe `AccessViolationException` is most likely caused by a shared-memory lifetime hazard in how `ExtractSubset` is used. When you call `bmp.ExtractSubset(subset, rect)`, the `subset` bitmap shares the underlying pixel memory of `bmp` without making a copy. Over many iterations, either accumulated memory pressure (from the abandoned original pixel allocation in `subset`) or a GC finalization race can corrupt the shared pixel buffer.\n\n**Workaround — break the shared-memory dependency:**\n\n```csharp\nusing (SKBitmap bmp = LoadBitmap())\n{\n    var rect = new SKRectI(20, 20, 240, 240);\n    // Extract a shared subset, then immediately copy to an independent bitmap\n    SKBitmap sharedSubset = new SKBitmap();\n    bmp.ExtractSubset(sharedSubset, rect);\n    using SKBitmap subset = sharedSubset.Copy();  // independent pixel buffer\n    sharedSubset.Dispose();\n\n    using var resized = subset.Resize(\n        new SKImageInfo(subset.Width * 3, subset.Height * 3,\n            SKImageInfo.PlatformColorType, SKAlphaType.Unpremul),\n        SKSamplingOptions.Default);\n    // use resized...\n}\n```\n\nAlternatively, as you found, explicit `Dispose()` after each iteration helps. Using `using` blocks ensures this happens deterministically rather than relying on the GC.\n\nTo help investigate the root cause, could you share:\n1. The full stack trace from the `AccessViolationException`?\n2. Is the bitmap processing running on multiple threads simultaneously?\n3. Does the crash go away when you use the `Copy()` approach above?"
      },
      {
        "type": "link-related",
        "description": "Cross-reference reporter's own related issue #2456 (same symptom, multithreaded context)",
        "risk": "low",
        "confidence": 0.85,
        "linkedIssue": 2456
      }
    ]
  }
}
```

</details>
