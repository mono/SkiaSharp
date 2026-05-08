# Issue Triage Report — #2751

| Field | Value |
|-------|-------|
| Repository | mono/SkiaSharp |
| Analyzed | 2026-05-01T21:38:53Z |
| Type | type/bug (0.97 (97%)) |
| Area | area/SkiaSharp (0.95 (95%)) |
| Suggested action | ready-to-fix (0.90 (90%)) |

**Issue Summary:** SKPixmap.GetPixelSpan() returns an incorrectly sized span when the pixmap comes from ExtractSubset (or any pixmap where RowBytes > Width * BytesPerPixel), because span length is calculated using tight Info.BytesSize instead of the native row stride.

**Analysis:** In SKPixmap.GetPixelSpan<T>() (binding/SkiaSharp/SKPixmap.cs line 146), the span length is set to info.BytesSize which equals Width*Height*BytesPerPixel (tight packing). When a pixmap is derived from ExtractSubset, the native RowBytes is wider than Width*BytesPerPixel because it shares the parent bitmap's row stride. The correct size per Skia's own SkPixmap::computeByteSize() is RowBytes*(Height-1) + Width*BytesPerPixel, which is larger and accounts for the last row not needing the full row stride.

**Recommendations:** **ready-to-fix** — Root cause is clearly identified in source code (wrong BytesSize vs RowBytes), minimal fix path is known, and the reporter provides a complete reproduction case.

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
| Current labels | type/bug |

## Evidence

### Reproduction

1. Decode a PNG bitmap (111x33 pixels)
2. Call ExtractSubset to get a 25x25 sub-bitmap starting at (bitmap.Width-25, bitmap.Height-25)
3. Call PeekPixels() to get a SKPixmap from the subset bitmap
4. Call GetPixelSpan() on the pixmap
5. Compare the span length with RowBytes*(Height-1) + RowBytes-for-last-row

**Environment:** Windows 10.0.19045 x64, .NET 8.0.1 x64, SkiaSharp 2.88.7

### Bug Signals

| Field | Value |
|-------|-------|
| Severity | medium |
| Regression claimed | False |
| Error type | wrong-output |
| Error message | GetPixelSpan() returns a span shorter than the actual pixel data when RowBytes > Width * BytesPerPixel |
| Repro quality | complete |
| Target frameworks | net8.0 |

### Version Analysis

| Field | Value |
|-------|-------|
| Mentioned versions | 2.88.7 |
| Worked in | — |
| Broke in | — |
| Current relevance | likely |
| Relevance reason | The SKPixmap.GetPixelSpan implementation uses info.BytesSize which is still calculated as Width*Height*BytesPerPixel; no fix has been merged. |

## Analysis

### Technical Summary

In SKPixmap.GetPixelSpan<T>() (binding/SkiaSharp/SKPixmap.cs line 146), the span length is set to info.BytesSize which equals Width*Height*BytesPerPixel (tight packing). When a pixmap is derived from ExtractSubset, the native RowBytes is wider than Width*BytesPerPixel because it shares the parent bitmap's row stride. The correct size per Skia's own SkPixmap::computeByteSize() is RowBytes*(Height-1) + Width*BytesPerPixel, which is larger and accounts for the last row not needing the full row stride.

### Rationale

The bug is clearly in GetPixelSpan<T>: it uses SKImageInfo.BytesSize (tight) instead of the pixmap's actual native RowBytes to compute the span length. This is wrong for any pixmap where RowBytes != Width*BytesPerPixel, which is the normal case for subsets.

### Key Signals

- "SKPixmap.GetPixelSpan() should return a span with a length of 11200 bytes (in this particular example)" — **issue body** (The expected byte count is RowBytes*(Height-1)+Width*BytesPerPixel; the current code returns Width*Height*BytesPerPixel which is smaller.)
- "The span's length should match the native Skia API: SkPixmap::computeByteSize() which in turn calls SkImageInfo::computeByteSize(pixmap->rowBytes())" — **issue body** (Reporter has correctly identified the root cause and the correct native formula.)
- "In theory, bug should occur on all devices" — **issue body** (Cross-platform bug; only triggered when RowBytes > Width*BytesPerPixel.)

### Code Investigation

| File | Lines | Relevance | Finding |
|------|-------|-----------|---------|
| `binding/SkiaSharp/SKPixmap.cs` | 140-166 | direct | GetPixelSpan<T>(x,y): when T==byte, spanLength = info.BytesSize = Width*Height*BytesPerPixel. However, info is SKImageInfo which always uses tight RowBytes (Width*BytesPerPixel). The pixmap's actual native row stride (which can be wider for subsets) is available via this.RowBytes (calls sk_pixmap_get_row_bytes). The span is created over the native pixel pointer with this incorrectly tight length, making it shorter than the real allocated data. |
| `binding/SkiaSharp/SKImageInfo.cs` | 112-116 | direct | SKImageInfo.BytesSize = Width*Height*BytesPerPixel and SKImageInfo.RowBytes = Width*BytesPerPixel — both assume tight (no-padding) layout. SKPixmap.RowBytes on the other hand calls the native sk_pixmap_get_row_bytes which can return a larger value for subset pixmaps. |
| `binding/SkiaSharp/SKPixmap.cs` | 103 | related | SKPixmap.RowBytes delegates to SkiaApi.sk_pixmap_get_row_bytes(Handle) — this is the correct native row stride that should be used in span length calculation. |

### Workarounds

- Manually calculate the span byte count as: (pixmap.RowBytes * (pixmapInfo.Height - 1)) + pixmapInfo.RowBytes, then create a span from GetPixels() with that length using unsafe code.
- Copy the bitmap to a new SKBitmap (with tight packing) before calling GetPixelSpan().

### Next Questions

- Does the same issue affect GetPixelSpan<T>(x,y) for the offset calculation at line 162 (y * info.Height + x looks like a bug too)?
- Should SKPixmap.BytesSize/BytesSize64 properties also be fixed to use native RowBytes rather than delegating to info.BytesSize?

### Resolution Proposals

**Hypothesis:** Fix GetPixelSpan<T>() to compute span length using the native RowBytes from sk_pixmap_get_row_bytes rather than the tight-packed SKImageInfo.BytesSize.

1. **Fix span length computation to use native RowBytes** — fix, confidence 0.93 (93%), cost/xs, validated=untested
   - In GetPixelSpan<T>(x,y), replace `spanLength = info.BytesSize` with `spanLength = info.Height > 0 ? (RowBytes * (info.Height - 1) + info.RowBytes) : 0`. This matches Skia's SkPixmap::computeByteSize() formula exactly.
2. **Expose SKPixmap.ComputeByteSize() as a public helper** — fix, confidence 0.85 (85%), cost/s, validated=untested
   - Add a public ComputeByteSize() method to SKPixmap that computes the correct byte size using the native row bytes, mirroring SkPixmap::computeByteSize(). Use it internally in GetPixelSpan<T>().

**Recommended proposal:** Fix span length computation to use native RowBytes

**Why:** Minimal change, directly addresses root cause, matches Skia's own formula. Adding a public API is a separate concern that can follow independently.

## Recommendations

### Actionability

| Field | Value |
|-------|-------|
| Suggested action | ready-to-fix |
| Confidence | 0.90 (90%) |
| Reason | Root cause is clearly identified in source code (wrong BytesSize vs RowBytes), minimal fix path is known, and the reporter provides a complete reproduction case. |
| Suggested repro platform | linux |

### Automatable Actions

| Type | Risk | Confidence | Description | Details |
|------|------|------------|-------------|---------|
| update-labels | low | 0.97 (97%) | Apply bug and area labels | labels=type/bug, area/SkiaSharp, tenet/reliability |
| add-comment | medium | 0.90 (90%) | Acknowledge issue and describe root cause and workaround | — |

**Comment draft for `add-comment`:**

```markdown
Thanks for the detailed repro! The root cause is in `SKPixmap.GetPixelSpan<T>()` — it computes the span length using `info.BytesSize` (`Width × Height × BytesPerPixel`, tight packing) but when the pixmap comes from `ExtractSubset`, the native row stride (`RowBytes`) is wider than `Width × BytesPerPixel` because it shares the parent bitmap's row layout.

Skia's own `SkPixmap::computeByteSize()` formula is: `RowBytes × (Height − 1) + Width × BytesPerPixel`, which accounts for the last row not needing the full stride.

**Workaround (until fixed):** Compute the length manually and build the span yourself:
```csharp
var rowBytes = pixmap.RowBytes;
var info = pixmap.Info;
var byteCount = info.Height > 0 ? rowBytes * (info.Height - 1) + info.RowBytes : 0;
unsafe {
    var span = new Span<byte>((void*)pixmap.GetPixels(), byteCount);
}
```

A fix is straightforward — changing the span length calculation in `SKPixmap.GetPixelSpan<T>()` to use `RowBytes` instead of `Info.BytesSize`.
```

<details>
<summary>Raw JSON</summary>

```json
{
  "meta": {
    "schemaVersion": "1.0",
    "number": 2751,
    "repo": "mono/SkiaSharp",
    "analyzedAt": "2026-05-01T21:38:53Z",
    "currentLabels": [
      "type/bug"
    ]
  },
  "summary": "SKPixmap.GetPixelSpan() returns an incorrectly sized span when the pixmap comes from ExtractSubset (or any pixmap where RowBytes > Width * BytesPerPixel), because span length is calculated using tight Info.BytesSize instead of the native row stride.",
  "classification": {
    "type": {
      "value": "type/bug",
      "confidence": 0.97
    },
    "area": {
      "value": "area/SkiaSharp",
      "confidence": 0.95
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
      "errorMessage": "GetPixelSpan() returns a span shorter than the actual pixel data when RowBytes > Width * BytesPerPixel",
      "reproQuality": "complete",
      "targetFrameworks": [
        "net8.0"
      ]
    },
    "reproEvidence": {
      "stepsToReproduce": [
        "Decode a PNG bitmap (111x33 pixels)",
        "Call ExtractSubset to get a 25x25 sub-bitmap starting at (bitmap.Width-25, bitmap.Height-25)",
        "Call PeekPixels() to get a SKPixmap from the subset bitmap",
        "Call GetPixelSpan() on the pixmap",
        "Compare the span length with RowBytes*(Height-1) + RowBytes-for-last-row"
      ],
      "environmentDetails": "Windows 10.0.19045 x64, .NET 8.0.1 x64, SkiaSharp 2.88.7"
    },
    "versionAnalysis": {
      "mentionedVersions": [
        "2.88.7"
      ],
      "currentRelevance": "likely",
      "relevanceReason": "The SKPixmap.GetPixelSpan implementation uses info.BytesSize which is still calculated as Width*Height*BytesPerPixel; no fix has been merged."
    }
  },
  "analysis": {
    "summary": "In SKPixmap.GetPixelSpan<T>() (binding/SkiaSharp/SKPixmap.cs line 146), the span length is set to info.BytesSize which equals Width*Height*BytesPerPixel (tight packing). When a pixmap is derived from ExtractSubset, the native RowBytes is wider than Width*BytesPerPixel because it shares the parent bitmap's row stride. The correct size per Skia's own SkPixmap::computeByteSize() is RowBytes*(Height-1) + Width*BytesPerPixel, which is larger and accounts for the last row not needing the full row stride.",
    "rationale": "The bug is clearly in GetPixelSpan<T>: it uses SKImageInfo.BytesSize (tight) instead of the pixmap's actual native RowBytes to compute the span length. This is wrong for any pixmap where RowBytes != Width*BytesPerPixel, which is the normal case for subsets.",
    "keySignals": [
      {
        "text": "SKPixmap.GetPixelSpan() should return a span with a length of 11200 bytes (in this particular example)",
        "source": "issue body",
        "interpretation": "The expected byte count is RowBytes*(Height-1)+Width*BytesPerPixel; the current code returns Width*Height*BytesPerPixel which is smaller."
      },
      {
        "text": "The span's length should match the native Skia API: SkPixmap::computeByteSize() which in turn calls SkImageInfo::computeByteSize(pixmap->rowBytes())",
        "source": "issue body",
        "interpretation": "Reporter has correctly identified the root cause and the correct native formula."
      },
      {
        "text": "In theory, bug should occur on all devices",
        "source": "issue body",
        "interpretation": "Cross-platform bug; only triggered when RowBytes > Width*BytesPerPixel."
      }
    ],
    "codeInvestigation": [
      {
        "file": "binding/SkiaSharp/SKPixmap.cs",
        "lines": "140-166",
        "finding": "GetPixelSpan<T>(x,y): when T==byte, spanLength = info.BytesSize = Width*Height*BytesPerPixel. However, info is SKImageInfo which always uses tight RowBytes (Width*BytesPerPixel). The pixmap's actual native row stride (which can be wider for subsets) is available via this.RowBytes (calls sk_pixmap_get_row_bytes). The span is created over the native pixel pointer with this incorrectly tight length, making it shorter than the real allocated data.",
        "relevance": "direct"
      },
      {
        "file": "binding/SkiaSharp/SKImageInfo.cs",
        "lines": "112-116",
        "finding": "SKImageInfo.BytesSize = Width*Height*BytesPerPixel and SKImageInfo.RowBytes = Width*BytesPerPixel — both assume tight (no-padding) layout. SKPixmap.RowBytes on the other hand calls the native sk_pixmap_get_row_bytes which can return a larger value for subset pixmaps.",
        "relevance": "direct"
      },
      {
        "file": "binding/SkiaSharp/SKPixmap.cs",
        "lines": "103",
        "finding": "SKPixmap.RowBytes delegates to SkiaApi.sk_pixmap_get_row_bytes(Handle) — this is the correct native row stride that should be used in span length calculation.",
        "relevance": "related"
      }
    ],
    "nextQuestions": [
      "Does the same issue affect GetPixelSpan<T>(x,y) for the offset calculation at line 162 (y * info.Height + x looks like a bug too)?",
      "Should SKPixmap.BytesSize/BytesSize64 properties also be fixed to use native RowBytes rather than delegating to info.BytesSize?"
    ],
    "workarounds": [
      "Manually calculate the span byte count as: (pixmap.RowBytes * (pixmapInfo.Height - 1)) + pixmapInfo.RowBytes, then create a span from GetPixels() with that length using unsafe code.",
      "Copy the bitmap to a new SKBitmap (with tight packing) before calling GetPixelSpan()."
    ],
    "resolution": {
      "hypothesis": "Fix GetPixelSpan<T>() to compute span length using the native RowBytes from sk_pixmap_get_row_bytes rather than the tight-packed SKImageInfo.BytesSize.",
      "proposals": [
        {
          "title": "Fix span length computation to use native RowBytes",
          "description": "In GetPixelSpan<T>(x,y), replace `spanLength = info.BytesSize` with `spanLength = info.Height > 0 ? (RowBytes * (info.Height - 1) + info.RowBytes) : 0`. This matches Skia's SkPixmap::computeByteSize() formula exactly.",
          "category": "fix",
          "confidence": 0.93,
          "effort": "cost/xs",
          "validated": "untested"
        },
        {
          "title": "Expose SKPixmap.ComputeByteSize() as a public helper",
          "description": "Add a public ComputeByteSize() method to SKPixmap that computes the correct byte size using the native row bytes, mirroring SkPixmap::computeByteSize(). Use it internally in GetPixelSpan<T>().",
          "category": "fix",
          "confidence": 0.85,
          "effort": "cost/s",
          "validated": "untested"
        }
      ],
      "recommendedProposal": "Fix span length computation to use native RowBytes",
      "recommendedReason": "Minimal change, directly addresses root cause, matches Skia's own formula. Adding a public API is a separate concern that can follow independently."
    }
  },
  "output": {
    "actionability": {
      "suggestedAction": "ready-to-fix",
      "confidence": 0.9,
      "reason": "Root cause is clearly identified in source code (wrong BytesSize vs RowBytes), minimal fix path is known, and the reporter provides a complete reproduction case.",
      "suggestedReproPlatform": "linux"
    },
    "actions": [
      {
        "type": "update-labels",
        "description": "Apply bug and area labels",
        "risk": "low",
        "confidence": 0.97,
        "labels": [
          "type/bug",
          "area/SkiaSharp",
          "tenet/reliability"
        ]
      },
      {
        "type": "add-comment",
        "description": "Acknowledge issue and describe root cause and workaround",
        "risk": "medium",
        "confidence": 0.9,
        "comment": "Thanks for the detailed repro! The root cause is in `SKPixmap.GetPixelSpan<T>()` — it computes the span length using `info.BytesSize` (`Width × Height × BytesPerPixel`, tight packing) but when the pixmap comes from `ExtractSubset`, the native row stride (`RowBytes`) is wider than `Width × BytesPerPixel` because it shares the parent bitmap's row layout.\n\nSkia's own `SkPixmap::computeByteSize()` formula is: `RowBytes × (Height − 1) + Width × BytesPerPixel`, which accounts for the last row not needing the full stride.\n\n**Workaround (until fixed):** Compute the length manually and build the span yourself:\n```csharp\nvar rowBytes = pixmap.RowBytes;\nvar info = pixmap.Info;\nvar byteCount = info.Height > 0 ? rowBytes * (info.Height - 1) + info.RowBytes : 0;\nunsafe {\n    var span = new Span<byte>((void*)pixmap.GetPixels(), byteCount);\n}\n```\n\nA fix is straightforward — changing the span length calculation in `SKPixmap.GetPixelSpan<T>()` to use `RowBytes` instead of `Info.BytesSize`."
      }
    ]
  }
}
```

</details>
