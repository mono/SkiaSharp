# Issue Triage Report — #2719

| Field | Value |
|-------|-------|
| Repository | mono/SkiaSharp |
| Analyzed | 2026-04-30T13:05:00Z |
| Type | type/bug (0.80 (80%)) |
| Area | area/SkiaSharp (0.95 (95%)) |
| Suggested action | needs-investigation (0.78 (78%)) |

**Issue Summary:** After calling ExtractSubset on an SKBitmap, the destination bitmap's RowBytes property returns the original (source) row stride rather than the natural row bytes for the new (smaller) width.

**Analysis:** SKBitmap.RowBytes returns the native sk_bitmap_get_row_bytes value, which after ExtractSubset equals the source bitmap's full row stride. SKBitmap.Info.RowBytes is computed as Width * BytesPerPixel and gives a 'natural' expected value for the subset width. The discrepancy arises because ExtractSubset creates a pixel alias (view) into the original buffer without copying, so the actual memory stride is larger than what Info.RowBytes computes. This is by-design Skia behavior but can be confusing and may have changed between versions.

**Recommendations:** **needs-investigation** — Reporter has a partial repro showing RowBytes/Info.RowBytes diverge after ExtractSubset. The behavior may be by-design but the regression claim and related #2751 warrant investigation. Need to confirm if 2.88.2 copied vs. aliased pixel data.

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

1. Create an SKBitmap of any size
2. Call ExtractSubset with a smaller rectangle into a destination SKBitmap
3. Check destination.RowBytes versus destination.Info.RowBytes
4. Observe RowBytes equals source bitmap row bytes, not subset width * bytesPerPixel

**Environment:** SkiaSharp 2.88.3, Visual Studio, Windows

**Related issues:** #2751, #2681

### Bug Signals

| Field | Value |
|-------|-------|
| Severity | medium |
| Regression claimed | True |
| Error type | wrong-output |
| Error message | bitmap.RowBytes != bitmap.Info.RowBytes after ExtractSubset |
| Repro quality | partial |
| Target frameworks | — |

### Version Analysis

| Field | Value |
|-------|-------|
| Mentioned versions | 2.88.3, 2.88.2 |
| Worked in | 2.88.2 |
| Broke in | 2.88.3 |
| Current relevance | likely |
| Relevance reason | No code changes to SKBitmap.RowBytes or ExtractSubset have been made to address this discrepancy. |

### Regression

| Field | Value |
|-------|-------|
| Is regression | True |
| Confidence | 0.55 (55%) |
| Reason | Reporter states 2.88.2 did not exhibit this; however, Skia's ExtractSubset has always created a view sharing the original pixel buffer and row stride. The regression claim is not fully verified. |
| Worked in version | 2.88.2 |
| Broke in version | 2.88.3 |

## Analysis

### Technical Summary

SKBitmap.RowBytes returns the native sk_bitmap_get_row_bytes value, which after ExtractSubset equals the source bitmap's full row stride. SKBitmap.Info.RowBytes is computed as Width * BytesPerPixel and gives a 'natural' expected value for the subset width. The discrepancy arises because ExtractSubset creates a pixel alias (view) into the original buffer without copying, so the actual memory stride is larger than what Info.RowBytes computes. This is by-design Skia behavior but can be confusing and may have changed between versions.

### Rationale

The discrepancy between RowBytes and Info.RowBytes is architecturally expected after ExtractSubset since the subset shares pixel memory with the source bitmap. However, the reporter's regression claim (worked in 2.88.2) and the related issue #2751 (GetPixelSpan incorrect size for subset bitmaps) suggest there may be a real downstream impact worth investigating. The issue deserves investigation to clarify whether documented behavior changed or if a workaround/documentation improvement is needed.

### Key Signals

- "RowBytes in Bitmap remains unchanged at the initial bitmap size, The RowBytes in Bitmap.Info.RowBytes does get updated" — **issue body** (Confirms the two properties diverge after ExtractSubset — RowBytes = source stride, Info.RowBytes = natural subset width stride)
- "Seems like this is also affecting the byte array for the bitmap" — **comment** (Reporter observes downstream impact on byte array sizes, suggesting real usage breakage beyond just a property value mismatch)
- "Last Known Good Version: 2.88.2" — **issue body** (Regression claim; however, the root behavior has not changed — possible the reporter's usage pattern changed or a subtle memory layout change in 2.88.3 surfaced this)

### Code Investigation

| File | Lines | Relevance | Finding |
|------|-------|-----------|---------|
| `binding/SkiaSharp/SKBitmap.cs` | 287-289 | direct | RowBytes property calls sk_bitmap_get_row_bytes directly — returns native row stride of the underlying pixel buffer, which is the source bitmap's full stride after ExtractSubset |
| `binding/SkiaSharp/SKImageInfo.cs` | 116 | direct | SKImageInfo.RowBytes is computed as Width * BytesPerPixel — returns expected natural row bytes for the Info dimensions, not the actual buffer stride |
| `binding/SkiaSharp/SKBitmap.cs` | 216-222 | direct | ExtractSubset delegates directly to sk_bitmap_extract_subset without post-processing — destination bitmap shares pixel data with source, so RowBytes inherits source stride |

### Workarounds

- After ExtractSubset, copy the destination bitmap to a new SKBitmap to get compact (natural) row bytes: var copy = destination.Copy()
- Use Info.RowBytes instead of RowBytes when computing byte buffer sizes for subset bitmaps
- Use bitmap.PeekPixels().RowBytes to get the actual memory stride when needed for pixel operations

### Next Questions

- Was ExtractSubset actually producing a copy (not a view) in 2.88.2, which would explain why RowBytes matched Info.RowBytes?
- Is there a code change between 2.88.2 and 2.88.3 that altered SKBitmap.ExtractSubset behavior?
- Does related issue #2751 (GetPixelSpan incorrect size) stem from the same root cause?

### Resolution Proposals

**Hypothesis:** ExtractSubset creates a pixel-aliased view with the source row stride; RowBytes correctly reports this stride while Info.RowBytes computes a natural value. The fix could be to document this behavior clearly, or to make RowBytes on a subset bitmap return Info.RowBytes (which may break raw memory operations).

1. **Document subset behavior** — workaround, confidence 0.80 (80%), cost/xs, validated=untested
   - Add XML documentation to ExtractSubset and RowBytes explaining that after a subset operation, RowBytes equals the source stride and Info.RowBytes gives the natural subset width stride. Document the Copy() workaround.
2. **Investigate 2.88.2 vs 2.88.3 change** — investigation, confidence 0.70 (70%), cost/s, validated=untested
   - Diff the SKBitmap.ExtractSubset implementation between 2.88.2 and 2.88.3 to determine if behavior changed — particularly whether pixel data was previously copied instead of aliased.

**Recommended proposal:** Investigate 2.88.2 vs 2.88.3 change

**Why:** Understanding whether this is a genuine regression is necessary before deciding between a documentation fix and a code fix.

## Recommendations

### Actionability

| Field | Value |
|-------|-------|
| Suggested action | needs-investigation |
| Confidence | 0.78 (78%) |
| Reason | Reporter has a partial repro showing RowBytes/Info.RowBytes diverge after ExtractSubset. The behavior may be by-design but the regression claim and related #2751 warrant investigation. Need to confirm if 2.88.2 copied vs. aliased pixel data. |
| Suggested repro platform | linux |

### Automatable Actions

| Type | Risk | Confidence | Description | Details |
|------|------|------------|-------------|---------|
| update-labels | low | 0.95 (95%) | Apply bug and SkiaSharp area labels | labels=type/bug, area/SkiaSharp, tenet/reliability |
| add-comment | medium | 0.78 (78%) | Explain ExtractSubset view semantics and provide workaround | — |
| link-related | low | 0.90 (90%) | Link related subset/RowBytes issue #2751 | linkedIssue=#2751 |

**Comment draft for `add-comment`:**

```markdown
Thanks for the report! After `ExtractSubset`, the destination bitmap is a **pixel alias** (view) into the source bitmap's memory — it does not copy the pixels. Because of this, `RowBytes` returns the source bitmap's full row stride (needed to correctly step through the shared memory), while `Info.RowBytes` returns `Width * BytesPerPixel` for the subset's dimensions.

If you need a standalone bitmap with compact (natural) row bytes, you can copy it:
```csharp
bool ok = bitmap.ExtractSubset(output.Bitmap, srcRect);
var compactCopy = output.Bitmap.Copy(); // new SKBitmap with natural RowBytes
```

We are investigating whether this behavior changed between 2.88.2 and 2.88.3, and whether the related issue #2751 (SKPixmap.GetPixelSpan size) has the same root cause.
```

<details>
<summary>Raw JSON</summary>

```json
{
  "meta": {
    "schemaVersion": "1.0",
    "number": 2719,
    "repo": "mono/SkiaSharp",
    "analyzedAt": "2026-04-30T13:05:00Z",
    "currentLabels": [
      "type/bug"
    ]
  },
  "summary": "After calling ExtractSubset on an SKBitmap, the destination bitmap's RowBytes property returns the original (source) row stride rather than the natural row bytes for the new (smaller) width.",
  "classification": {
    "type": {
      "value": "type/bug",
      "confidence": 0.8
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
      "regressionClaimed": true,
      "errorType": "wrong-output",
      "errorMessage": "bitmap.RowBytes != bitmap.Info.RowBytes after ExtractSubset",
      "reproQuality": "partial",
      "targetFrameworks": []
    },
    "reproEvidence": {
      "stepsToReproduce": [
        "Create an SKBitmap of any size",
        "Call ExtractSubset with a smaller rectangle into a destination SKBitmap",
        "Check destination.RowBytes versus destination.Info.RowBytes",
        "Observe RowBytes equals source bitmap row bytes, not subset width * bytesPerPixel"
      ],
      "environmentDetails": "SkiaSharp 2.88.3, Visual Studio, Windows",
      "relatedIssues": [
        2751,
        2681
      ]
    },
    "versionAnalysis": {
      "mentionedVersions": [
        "2.88.3",
        "2.88.2"
      ],
      "workedIn": "2.88.2",
      "brokeIn": "2.88.3",
      "currentRelevance": "likely",
      "relevanceReason": "No code changes to SKBitmap.RowBytes or ExtractSubset have been made to address this discrepancy."
    },
    "regression": {
      "isRegression": true,
      "confidence": 0.55,
      "reason": "Reporter states 2.88.2 did not exhibit this; however, Skia's ExtractSubset has always created a view sharing the original pixel buffer and row stride. The regression claim is not fully verified.",
      "workedInVersion": "2.88.2",
      "brokeInVersion": "2.88.3"
    }
  },
  "analysis": {
    "summary": "SKBitmap.RowBytes returns the native sk_bitmap_get_row_bytes value, which after ExtractSubset equals the source bitmap's full row stride. SKBitmap.Info.RowBytes is computed as Width * BytesPerPixel and gives a 'natural' expected value for the subset width. The discrepancy arises because ExtractSubset creates a pixel alias (view) into the original buffer without copying, so the actual memory stride is larger than what Info.RowBytes computes. This is by-design Skia behavior but can be confusing and may have changed between versions.",
    "rationale": "The discrepancy between RowBytes and Info.RowBytes is architecturally expected after ExtractSubset since the subset shares pixel memory with the source bitmap. However, the reporter's regression claim (worked in 2.88.2) and the related issue #2751 (GetPixelSpan incorrect size for subset bitmaps) suggest there may be a real downstream impact worth investigating. The issue deserves investigation to clarify whether documented behavior changed or if a workaround/documentation improvement is needed.",
    "codeInvestigation": [
      {
        "file": "binding/SkiaSharp/SKBitmap.cs",
        "lines": "287-289",
        "finding": "RowBytes property calls sk_bitmap_get_row_bytes directly — returns native row stride of the underlying pixel buffer, which is the source bitmap's full stride after ExtractSubset",
        "relevance": "direct"
      },
      {
        "file": "binding/SkiaSharp/SKImageInfo.cs",
        "lines": "116",
        "finding": "SKImageInfo.RowBytes is computed as Width * BytesPerPixel — returns expected natural row bytes for the Info dimensions, not the actual buffer stride",
        "relevance": "direct"
      },
      {
        "file": "binding/SkiaSharp/SKBitmap.cs",
        "lines": "216-222",
        "finding": "ExtractSubset delegates directly to sk_bitmap_extract_subset without post-processing — destination bitmap shares pixel data with source, so RowBytes inherits source stride",
        "relevance": "direct"
      }
    ],
    "keySignals": [
      {
        "text": "RowBytes in Bitmap remains unchanged at the initial bitmap size, The RowBytes in Bitmap.Info.RowBytes does get updated",
        "source": "issue body",
        "interpretation": "Confirms the two properties diverge after ExtractSubset — RowBytes = source stride, Info.RowBytes = natural subset width stride"
      },
      {
        "text": "Seems like this is also affecting the byte array for the bitmap",
        "source": "comment",
        "interpretation": "Reporter observes downstream impact on byte array sizes, suggesting real usage breakage beyond just a property value mismatch"
      },
      {
        "text": "Last Known Good Version: 2.88.2",
        "source": "issue body",
        "interpretation": "Regression claim; however, the root behavior has not changed — possible the reporter's usage pattern changed or a subtle memory layout change in 2.88.3 surfaced this"
      }
    ],
    "nextQuestions": [
      "Was ExtractSubset actually producing a copy (not a view) in 2.88.2, which would explain why RowBytes matched Info.RowBytes?",
      "Is there a code change between 2.88.2 and 2.88.3 that altered SKBitmap.ExtractSubset behavior?",
      "Does related issue #2751 (GetPixelSpan incorrect size) stem from the same root cause?"
    ],
    "workarounds": [
      "After ExtractSubset, copy the destination bitmap to a new SKBitmap to get compact (natural) row bytes: var copy = destination.Copy()",
      "Use Info.RowBytes instead of RowBytes when computing byte buffer sizes for subset bitmaps",
      "Use bitmap.PeekPixels().RowBytes to get the actual memory stride when needed for pixel operations"
    ],
    "resolution": {
      "hypothesis": "ExtractSubset creates a pixel-aliased view with the source row stride; RowBytes correctly reports this stride while Info.RowBytes computes a natural value. The fix could be to document this behavior clearly, or to make RowBytes on a subset bitmap return Info.RowBytes (which may break raw memory operations).",
      "proposals": [
        {
          "title": "Document subset behavior",
          "description": "Add XML documentation to ExtractSubset and RowBytes explaining that after a subset operation, RowBytes equals the source stride and Info.RowBytes gives the natural subset width stride. Document the Copy() workaround.",
          "category": "workaround",
          "confidence": 0.8,
          "effort": "cost/xs",
          "validated": "untested"
        },
        {
          "title": "Investigate 2.88.2 vs 2.88.3 change",
          "description": "Diff the SKBitmap.ExtractSubset implementation between 2.88.2 and 2.88.3 to determine if behavior changed — particularly whether pixel data was previously copied instead of aliased.",
          "category": "investigation",
          "confidence": 0.7,
          "effort": "cost/s",
          "validated": "untested"
        }
      ],
      "recommendedProposal": "Investigate 2.88.2 vs 2.88.3 change",
      "recommendedReason": "Understanding whether this is a genuine regression is necessary before deciding between a documentation fix and a code fix."
    }
  },
  "output": {
    "actionability": {
      "suggestedAction": "needs-investigation",
      "confidence": 0.78,
      "reason": "Reporter has a partial repro showing RowBytes/Info.RowBytes diverge after ExtractSubset. The behavior may be by-design but the regression claim and related #2751 warrant investigation. Need to confirm if 2.88.2 copied vs. aliased pixel data.",
      "suggestedReproPlatform": "linux"
    },
    "actions": [
      {
        "type": "update-labels",
        "description": "Apply bug and SkiaSharp area labels",
        "risk": "low",
        "confidence": 0.95,
        "labels": [
          "type/bug",
          "area/SkiaSharp",
          "tenet/reliability"
        ]
      },
      {
        "type": "add-comment",
        "description": "Explain ExtractSubset view semantics and provide workaround",
        "risk": "medium",
        "confidence": 0.78,
        "comment": "Thanks for the report! After `ExtractSubset`, the destination bitmap is a **pixel alias** (view) into the source bitmap's memory — it does not copy the pixels. Because of this, `RowBytes` returns the source bitmap's full row stride (needed to correctly step through the shared memory), while `Info.RowBytes` returns `Width * BytesPerPixel` for the subset's dimensions.\n\nIf you need a standalone bitmap with compact (natural) row bytes, you can copy it:\n```csharp\nbool ok = bitmap.ExtractSubset(output.Bitmap, srcRect);\nvar compactCopy = output.Bitmap.Copy(); // new SKBitmap with natural RowBytes\n```\n\nWe are investigating whether this behavior changed between 2.88.2 and 2.88.3, and whether the related issue #2751 (SKPixmap.GetPixelSpan size) has the same root cause."
      },
      {
        "type": "link-related",
        "description": "Link related subset/RowBytes issue #2751",
        "risk": "low",
        "confidence": 0.9,
        "linkedIssue": 2751
      }
    ]
  }
}
```

</details>
