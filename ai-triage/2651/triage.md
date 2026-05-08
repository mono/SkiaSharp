# Issue Triage Report — #2651

| Field | Value |
|-------|-------|
| Repository | mono/SkiaSharp |
| Analyzed | 2026-04-26T15:35:21Z |
| Type | type/bug (0.85 (85%)) |
| Area | area/SkiaSharp (0.80 (80%)) |
| Suggested action | close-as-not-a-bug (0.85 (85%)) |

**Issue Summary:** Reporter crashes in Blazor WASM when calling SKSurface.Snapshot() repeatedly on a large (8000x2000) raster surface without disposing previous snapshots, causing a C++ out-of-memory exception after approximately 80 mouse-move events.

**Analysis:** The crash is a C++ out-of-memory exception in WASM caused by not disposing old SKImage snapshots. Each 8000x2000 RGBA snapshot occupies ~61 MB of native raster pixel data allocated via SkData::MakeWithCopy. Because the layerBackGround field is overwritten without first calling Dispose(), each mouse-move leaks ~61 MB. After ~80 calls (~5 GB accumulated), the WASM heap is exhausted and operator new throws.

**Recommendations:** **close-as-not-a-bug** — SKSurface.Snapshot() behavior is correct. The crash is caused by not disposing previous SKImage objects — a common memory management misunderstanding that has a clear one-line fix.

---

## Classification

| Field | Value |
|-------|-------|
| Type | type/bug |
| Area | area/SkiaSharp |
| Platforms | os/WASM |
| Backends | — |
| Tenets | tenet/reliability |
| Partner | — |
| Current labels | type/bug |

## Evidence

### Reproduction

1. Create Blazor WASM app with SKCanvasView at 8000x2000 pixels
2. On first paint, call e.Surface.Snapshot() and store in a field
3. On subsequent mouse-move events, draw the stored snapshot then overwrite the field with e.Surface.Snapshot() without disposing the old snapshot
4. After ~80 mouse-move events, observe Uncaught Error 63124824 in browser console

**Environment:** Windows 11 / Chrome, SkiaSharp 2.88.3 + SkiaSharp.Views.Blazor 2.88.6, .NET 6 and .NET 7 WASM

### Bug Signals

| Field | Value |
|-------|-------|
| Severity | medium |
| Regression claimed | False |
| Error type | crash |
| Error message | Uncaught Error 63124824 at ___cxa_throw / $operator new(unsigned long) — C++ OOM in SkData::MakeWithCopy |
| Repro quality | partial |
| Target frameworks | net7.0, net6.0 |

**Stack trace:**

```text
$SkData::MakeWithCopy -> $MakeRasterCopyPriv -> $SkMakeImageFromRasterBitmapPriv -> $SkSurface_Raster::onNewImageSnapshot -> $SkSurface::makeImageSnapshot -> $sk_surface_new_image_snapshot
```

### Version Analysis

| Field | Value |
|-------|-------|
| Mentioned versions | 2.88.3, 2.88.6 |
| Worked in | — |
| Broke in | — |
| Current relevance | likely |
| Relevance reason | Failure to dispose SKImage snapshots causes the same OOM in any SkiaSharp version under WASM memory constraints. |

## Analysis

### Technical Summary

The crash is a C++ out-of-memory exception in WASM caused by not disposing old SKImage snapshots. Each 8000x2000 RGBA snapshot occupies ~61 MB of native raster pixel data allocated via SkData::MakeWithCopy. Because the layerBackGround field is overwritten without first calling Dispose(), each mouse-move leaks ~61 MB. After ~80 calls (~5 GB accumulated), the WASM heap is exhausted and operator new throws.

### Rationale

SKSurface.Snapshot() correctly creates an independent raster copy of pixel data — the SkiaSharp API is behaving as designed. The reporter overwrites layerBackGround without calling Dispose() on the old SKImage, leaking ~61 MB per call. Per triage guidance: keep type/bug (crash is real, API is easy to misuse for memory) and suggest close-as-not-a-bug with a clear workaround.

### Key Signals

- "___cxa_throw at $operator new(unsigned long) -> SkData::MakeWithCopy" — **stack trace** (C++ OOM exception thrown when allocating raster pixel copy. Classic WASM memory exhaustion pattern.)
- "When the canvas is smaller (300x400) the exception is not occurring" — **issue body** (Confirms memory pressure hypothesis: 300x400x4=480 KB vs 8000x2000x4=61 MB per snapshot. Small-canvas accumulation stays under WASM heap limit; large canvas does not.)
- "layerBackGround = e.Surface.Snapshot(); // Here the exception is raised" — **code snippet** (Old SKImage in layerBackGround field is never disposed before being overwritten, leaking ~61 MB per call.)

### Code Investigation

| File | Lines | Relevance | Finding |
|------|-------|-----------|---------|
| `binding/SkiaSharp/SKSurface.cs` | 274-275 | direct | Snapshot() calls sk_surface_new_image_snapshot directly with no caching. Each call allocates a full pixel copy of the surface via SkData::MakeWithCopy in native Skia — confirmed by the stack trace. |
| `binding/SkiaSharp/SKSurface.cs` | 277-278 | related | Snapshot(SKRectI bounds) overload exists for cropped snapshots — could reduce per-call allocation if only a sub-region is needed. |

### Workarounds

- Dispose old snapshot before overwriting: var old = layerBackGround; layerBackGround = e.Surface.Snapshot(); old?.Dispose();
- Use SKSurface.Snapshot(SKRectI bounds) with a smaller crop region to reduce per-snapshot memory usage
- Use an SKBitmap as a reusable backing buffer with surface.ReadPixels(bitmap, 0, 0) instead of repeated Snapshot() calls

### Next Questions

- Does the reporter also need to dispose the initial firstRun snapshot when the Blazor component is destroyed (IDisposable)?
- Would the reporter be open to using Surface.Canvas.DrawImage from the surface directly instead of making full copies?

### Resolution Proposals

**Hypothesis:** Old SKImage snapshots are not disposed, causing ~61 MB/call memory leak. WASM heap exhausts after ~80 calls.

1. **Dispose old snapshot before overwriting** — workaround, confidence 0.95 (95%), cost/xs, validated=yes
   - Capture the old reference, take the new snapshot, then dispose the old one. This prevents the memory leak with a one-line change.

```csharp
var oldLayer = layerBackGround;
layerBackGround = e.Surface.Snapshot();
oldLayer?.Dispose();
```
2. **Use SKBitmap as a reusable backing buffer** — alternative, confidence 0.80 (80%), cost/s, validated=untested
   - Instead of creating a new SKImage snapshot on every frame, allocate one SKBitmap at the component's initial size and use surface.ReadPixels(bitmap, 0, 0) to capture state. Restore using canvas.DrawBitmap(bitmap, ...). SKBitmap is reused without per-frame allocation.

**Recommended proposal:** Dispose old snapshot before overwriting

**Why:** One-line fix with immediate effect. Directly addresses the leak without changing the overall snapshot pattern the reporter already understands.

## Recommendations

### Actionability

| Field | Value |
|-------|-------|
| Suggested action | close-as-not-a-bug |
| Confidence | 0.85 (85%) |
| Reason | SKSurface.Snapshot() behavior is correct. The crash is caused by not disposing previous SKImage objects — a common memory management misunderstanding that has a clear one-line fix. |
| Suggested repro platform | linux |

### Automatable Actions

| Type | Risk | Confidence | Description | Details |
|------|------|------------|-------------|---------|
| update-labels | low | 0.90 (90%) | Apply bug, SkiaSharp core, WASM, and reliability tenet labels | labels=type/bug, area/SkiaSharp, os/WASM, tenet/reliability |
| add-comment | medium | 0.90 (90%) | Explain disposal requirement and provide workaround with code example | — |
| close-issue | medium | 0.80 (80%) | Close as not a bug — correct disposal pattern documented in comment | stateReason=not_planned |

**Comment draft for `add-comment`:**

```markdown
Thanks for the detailed stack trace!

The crash is a C++ out-of-memory exception in the WASM runtime. Each call to `e.Surface.Snapshot()` on an 8000x2000 surface allocates roughly **61 MB** of native raster pixel data (8000 × 2000 × 4 bytes). Because `layerBackGround` is overwritten without first calling `Dispose()` on the old snapshot, each mouse-move leaks ~61 MB. After ~80 moves that is ~5 GB accumulated — exhausting the WASM heap.

**Fix:** dispose the previous snapshot before creating a new one:

```csharp
var oldLayer = layerBackGround;
layerBackGround = e.Surface.Snapshot();
oldLayer?.Dispose();
```

Also ensure `layerBackGround` is disposed when the component is destroyed (implement `IDisposable` on the component and call `layerBackGround?.Dispose()` there).

If you want to reduce allocations further, consider:
- `SKSurface.Snapshot(SKRectI bounds)` to snapshot only the changed region
- An `SKBitmap` backing buffer with `surface.ReadPixels(bitmap, 0, 0)` / `canvas.DrawBitmap(bitmap, ...)` — the bitmap is reused each frame without a new allocation
```

<details>
<summary>Raw JSON</summary>

```json
{
  "meta": {
    "schemaVersion": "1.0",
    "number": 2651,
    "repo": "mono/SkiaSharp",
    "analyzedAt": "2026-04-26T15:35:21Z",
    "currentLabels": [
      "type/bug"
    ]
  },
  "summary": "Reporter crashes in Blazor WASM when calling SKSurface.Snapshot() repeatedly on a large (8000x2000) raster surface without disposing previous snapshots, causing a C++ out-of-memory exception after approximately 80 mouse-move events.",
  "classification": {
    "type": {
      "value": "type/bug",
      "confidence": 0.85
    },
    "area": {
      "value": "area/SkiaSharp",
      "confidence": 0.8
    },
    "platforms": [
      "os/WASM"
    ],
    "tenets": [
      "tenet/reliability"
    ]
  },
  "evidence": {
    "bugSignals": {
      "severity": "medium",
      "regressionClaimed": false,
      "errorType": "crash",
      "errorMessage": "Uncaught Error 63124824 at ___cxa_throw / $operator new(unsigned long) — C++ OOM in SkData::MakeWithCopy",
      "stackTrace": "$SkData::MakeWithCopy -> $MakeRasterCopyPriv -> $SkMakeImageFromRasterBitmapPriv -> $SkSurface_Raster::onNewImageSnapshot -> $SkSurface::makeImageSnapshot -> $sk_surface_new_image_snapshot",
      "reproQuality": "partial",
      "targetFrameworks": [
        "net7.0",
        "net6.0"
      ]
    },
    "reproEvidence": {
      "stepsToReproduce": [
        "Create Blazor WASM app with SKCanvasView at 8000x2000 pixels",
        "On first paint, call e.Surface.Snapshot() and store in a field",
        "On subsequent mouse-move events, draw the stored snapshot then overwrite the field with e.Surface.Snapshot() without disposing the old snapshot",
        "After ~80 mouse-move events, observe Uncaught Error 63124824 in browser console"
      ],
      "environmentDetails": "Windows 11 / Chrome, SkiaSharp 2.88.3 + SkiaSharp.Views.Blazor 2.88.6, .NET 6 and .NET 7 WASM"
    },
    "versionAnalysis": {
      "mentionedVersions": [
        "2.88.3",
        "2.88.6"
      ],
      "currentRelevance": "likely",
      "relevanceReason": "Failure to dispose SKImage snapshots causes the same OOM in any SkiaSharp version under WASM memory constraints."
    }
  },
  "analysis": {
    "summary": "The crash is a C++ out-of-memory exception in WASM caused by not disposing old SKImage snapshots. Each 8000x2000 RGBA snapshot occupies ~61 MB of native raster pixel data allocated via SkData::MakeWithCopy. Because the layerBackGround field is overwritten without first calling Dispose(), each mouse-move leaks ~61 MB. After ~80 calls (~5 GB accumulated), the WASM heap is exhausted and operator new throws.",
    "rationale": "SKSurface.Snapshot() correctly creates an independent raster copy of pixel data — the SkiaSharp API is behaving as designed. The reporter overwrites layerBackGround without calling Dispose() on the old SKImage, leaking ~61 MB per call. Per triage guidance: keep type/bug (crash is real, API is easy to misuse for memory) and suggest close-as-not-a-bug with a clear workaround.",
    "codeInvestigation": [
      {
        "file": "binding/SkiaSharp/SKSurface.cs",
        "lines": "274-275",
        "finding": "Snapshot() calls sk_surface_new_image_snapshot directly with no caching. Each call allocates a full pixel copy of the surface via SkData::MakeWithCopy in native Skia — confirmed by the stack trace.",
        "relevance": "direct"
      },
      {
        "file": "binding/SkiaSharp/SKSurface.cs",
        "lines": "277-278",
        "finding": "Snapshot(SKRectI bounds) overload exists for cropped snapshots — could reduce per-call allocation if only a sub-region is needed.",
        "relevance": "related"
      }
    ],
    "keySignals": [
      {
        "text": "___cxa_throw at $operator new(unsigned long) -> SkData::MakeWithCopy",
        "source": "stack trace",
        "interpretation": "C++ OOM exception thrown when allocating raster pixel copy. Classic WASM memory exhaustion pattern."
      },
      {
        "text": "When the canvas is smaller (300x400) the exception is not occurring",
        "source": "issue body",
        "interpretation": "Confirms memory pressure hypothesis: 300x400x4=480 KB vs 8000x2000x4=61 MB per snapshot. Small-canvas accumulation stays under WASM heap limit; large canvas does not."
      },
      {
        "text": "layerBackGround = e.Surface.Snapshot(); // Here the exception is raised",
        "source": "code snippet",
        "interpretation": "Old SKImage in layerBackGround field is never disposed before being overwritten, leaking ~61 MB per call."
      }
    ],
    "workarounds": [
      "Dispose old snapshot before overwriting: var old = layerBackGround; layerBackGround = e.Surface.Snapshot(); old?.Dispose();",
      "Use SKSurface.Snapshot(SKRectI bounds) with a smaller crop region to reduce per-snapshot memory usage",
      "Use an SKBitmap as a reusable backing buffer with surface.ReadPixels(bitmap, 0, 0) instead of repeated Snapshot() calls"
    ],
    "nextQuestions": [
      "Does the reporter also need to dispose the initial firstRun snapshot when the Blazor component is destroyed (IDisposable)?",
      "Would the reporter be open to using Surface.Canvas.DrawImage from the surface directly instead of making full copies?"
    ],
    "resolution": {
      "hypothesis": "Old SKImage snapshots are not disposed, causing ~61 MB/call memory leak. WASM heap exhausts after ~80 calls.",
      "proposals": [
        {
          "title": "Dispose old snapshot before overwriting",
          "description": "Capture the old reference, take the new snapshot, then dispose the old one. This prevents the memory leak with a one-line change.",
          "category": "workaround",
          "codeSnippet": "var oldLayer = layerBackGround;\nlayerBackGround = e.Surface.Snapshot();\noldLayer?.Dispose();",
          "confidence": 0.95,
          "effort": "cost/xs",
          "validated": "yes"
        },
        {
          "title": "Use SKBitmap as a reusable backing buffer",
          "description": "Instead of creating a new SKImage snapshot on every frame, allocate one SKBitmap at the component's initial size and use surface.ReadPixels(bitmap, 0, 0) to capture state. Restore using canvas.DrawBitmap(bitmap, ...). SKBitmap is reused without per-frame allocation.",
          "category": "alternative",
          "confidence": 0.8,
          "effort": "cost/s",
          "validated": "untested"
        }
      ],
      "recommendedProposal": "Dispose old snapshot before overwriting",
      "recommendedReason": "One-line fix with immediate effect. Directly addresses the leak without changing the overall snapshot pattern the reporter already understands."
    }
  },
  "output": {
    "actionability": {
      "suggestedAction": "close-as-not-a-bug",
      "confidence": 0.85,
      "reason": "SKSurface.Snapshot() behavior is correct. The crash is caused by not disposing previous SKImage objects — a common memory management misunderstanding that has a clear one-line fix.",
      "suggestedReproPlatform": "linux"
    },
    "actions": [
      {
        "type": "update-labels",
        "description": "Apply bug, SkiaSharp core, WASM, and reliability tenet labels",
        "risk": "low",
        "confidence": 0.9,
        "labels": [
          "type/bug",
          "area/SkiaSharp",
          "os/WASM",
          "tenet/reliability"
        ]
      },
      {
        "type": "add-comment",
        "description": "Explain disposal requirement and provide workaround with code example",
        "risk": "medium",
        "confidence": 0.9,
        "comment": "Thanks for the detailed stack trace!\n\nThe crash is a C++ out-of-memory exception in the WASM runtime. Each call to `e.Surface.Snapshot()` on an 8000x2000 surface allocates roughly **61 MB** of native raster pixel data (8000 × 2000 × 4 bytes). Because `layerBackGround` is overwritten without first calling `Dispose()` on the old snapshot, each mouse-move leaks ~61 MB. After ~80 moves that is ~5 GB accumulated — exhausting the WASM heap.\n\n**Fix:** dispose the previous snapshot before creating a new one:\n\n```csharp\nvar oldLayer = layerBackGround;\nlayerBackGround = e.Surface.Snapshot();\noldLayer?.Dispose();\n```\n\nAlso ensure `layerBackGround` is disposed when the component is destroyed (implement `IDisposable` on the component and call `layerBackGround?.Dispose()` there).\n\nIf you want to reduce allocations further, consider:\n- `SKSurface.Snapshot(SKRectI bounds)` to snapshot only the changed region\n- An `SKBitmap` backing buffer with `surface.ReadPixels(bitmap, 0, 0)` / `canvas.DrawBitmap(bitmap, ...)` — the bitmap is reused each frame without a new allocation"
      },
      {
        "type": "close-issue",
        "description": "Close as not a bug — correct disposal pattern documented in comment",
        "risk": "medium",
        "confidence": 0.8,
        "stateReason": "not_planned"
      }
    ]
  }
}
```

</details>
