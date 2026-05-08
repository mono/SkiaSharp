# Issue Triage Report — #769

| Field | Value |
|-------|-------|
| Repository | mono/SkiaSharp |
| Analyzed | 2026-04-24T04:08:15Z |
| Type | type/bug (0.97 (97%)) |
| Area | area/SkiaSharp (0.92 (92%)) |
| Suggested action | close-as-fixed (0.90 (90%)) |

**Issue Summary:** SKBitmap.SetPixel premultiplies RGB channel values when the bitmap is configured with SKAlphaType.Unpremul, causing incorrect pixel bytes on read-back; confirmed fixed in SkiaSharp 3.x.

**Analysis:** SKBitmap.SetPixel was incorrectly premultiplying pixel RGB channels when the bitmap alpha type was SKAlphaType.Unpremul; this caused stored bytes to reflect premultiplied values (e.g., 255*100/255≈100) rather than the raw channel values. The bug was present through SkiaSharp 2.88.8 and is confirmed fixed in SkiaSharp 3.x, likely due to a rewrite of SetPixel to use SKCanvas.DrawPoint which correctly handles the alpha type.

**Recommendations:** **close-as-fixed** — Maintainer confirmed fix in SkiaSharp 3.x in May 2024; community reporter confirmed fix in June 2024. Current code in main uses canvas-based SetPixel that correctly handles alpha types.

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
| Current labels | type/bug, os/Windows-Classic, area/SkiaSharp.Views, status/needs-attention |

## Evidence

### Reproduction

1. Create SKBitmap(16,16, SKColorType.Rgba8888, SKAlphaType.Unpremul)
2. Call SetPixel(0, 0, new SKColor(red:255, green:255, blue:255, alpha:100))
3. Read Bytes[] — RGB channels are 100 instead of 255

**Environment:** SkiaSharp 1.60.3, Visual Studio, Windows 10, Full .NET Framework

**Repository links:**
- https://devdiv.visualstudio.com/DevDiv/_workitems/edit/782039 — VS internal bug #782039 cross-reference

### Bug Signals

| Field | Value |
|-------|-------|
| Severity | medium |
| Regression claimed | False |
| Error type | wrong-output |
| Error message | bytes[0] == 100 instead of 255 when alpha=100 and RGB=255 on SKAlphaType.Unpremul bitmap |
| Repro quality | complete |
| Target frameworks | .NETFramework, net-windows |

### Version Analysis

| Field | Value |
|-------|-------|
| Mentioned versions | 1.60.3, 2.88.8 |
| Worked in | — |
| Broke in | 1.60.3 |
| Current relevance | unlikely |
| Relevance reason | Maintainer confirmed fixed in SkiaSharp 3.x; community member verified the fix in June 2024. |

### Fix Status

| Field | Value |
|-------|-------|
| Likely fixed | True |
| Confidence | 0.92 (92%) |
| Reason | Maintainer mattleibow commented 'This appears to be fixed in SkiaSharp 3.x releases' in May 2024; user neptuwunium confirmed 'It does appear to be fixed' in June 2024. Current SetPixel uses SKCanvas.DrawPoint which properly respects bitmap alpha type. |
| Related PRs | — |
| Related commits | — |
| Fixed in version | 3.x |

## Analysis

### Technical Summary

SKBitmap.SetPixel was incorrectly premultiplying pixel RGB channels when the bitmap alpha type was SKAlphaType.Unpremul; this caused stored bytes to reflect premultiplied values (e.g., 255*100/255≈100) rather than the raw channel values. The bug was present through SkiaSharp 2.88.8 and is confirmed fixed in SkiaSharp 3.x, likely due to a rewrite of SetPixel to use SKCanvas.DrawPoint which correctly handles the alpha type.

### Rationale

Bug classification is clear: reproducible code, specific wrong-output symptom, confirmed versions. Area is area/SkiaSharp (SKBitmap is core, not views; the existing area/SkiaSharp.Views label is incorrect). Action is close-as-fixed because both the maintainer and a community reporter have confirmed resolution in SkiaSharp 3.x.

### Key Signals

- "bytes[0] == 255 //Blue. Expected 255, result 100" — **issue body** (RGB channels premultiplied by alpha (255 * 100/255 ≈ 100), indicating Unpremul setting is not respected by SetPixel.)
- "This issue still happens five years later on SkiaSharp 2.88.8" — **comment by neptuwunium, 2024-05-31** (Bug persisted across all 2.x releases, not just the original report version.)
- "This appears to be fixed in SkiaSharp 3.x releases." — **comment by mattleibow, 2024-05-31** (Maintainer confirms fix in SkiaSharp 3.x.)
- "It does appear to be fixed." — **comment by neptuwunium, 2024-06-02** (Community confirmation that SkiaSharp 3.x resolves the issue.)

### Code Investigation

| File | Lines | Relevance | Finding |
|------|-------|-----------|---------|
| `binding/SkiaSharp/SKBitmap.cs` | 131-141 | direct | SetPixel currently creates an SKCanvas and calls canvas.DrawPoint(x, y, color). Canvas drawing in Skia respects the bitmap's alpha type, so this implementation should correctly store unpremultiplied values on an Unpremul bitmap. This is likely the fixed implementation in SkiaSharp 3.x. |
| `binding/SkiaSharp/SKBitmap.cs` | 320-326 | direct | Bytes property uses GetPixelSpan().ToArray() which reads raw pixel memory directly. If SetPixel correctly writes unpremultiplied values, Bytes will return the correct raw bytes. The fix in SetPixel would make Bytes return correct data. |
| `binding/SkiaSharp/SKBitmap.cs` | 439-442 | context | Decode() overrides Unpremul to Premul when decoding from codec — intentional design decision for codec decode path, unrelated to SetPixel. |

### Workarounds

- Upgrade to SkiaSharp 3.x where the issue is fixed.
- On SkiaSharp 2.x: write pixel bytes directly via GetPixelSpan() or use Marshal/unsafe to write to the raw pixel buffer instead of calling SetPixel.

### Next Questions

- Was SetPixel previously calling a native sk_bitmap_set_pixel that did not respect alpha type?

### Resolution Proposals

**Hypothesis:** In SkiaSharp 2.x, SetPixel likely called a native function that internally converted color through premultiplied paths, ignoring the bitmap's declared alpha type. The rewrite to canvas.DrawPoint in SkiaSharp 3.x correctly routes through Skia's alpha-aware drawing pipeline.

1. **Close as fixed in SkiaSharp 3.x** — fix, confidence 0.92 (92%), cost/xs, validated=untested
   - The issue is confirmed fixed in SkiaSharp 3.x by both the maintainer and a community reporter. Close with a note directing users to upgrade.

**Recommended proposal:** Close as fixed in SkiaSharp 3.x

**Why:** Both maintainer and user have confirmed the fix is present in 3.x. The issue should be closed to reduce backlog noise.

## Recommendations

### Actionability

| Field | Value |
|-------|-------|
| Suggested action | close-as-fixed |
| Confidence | 0.90 (90%) |
| Reason | Maintainer confirmed fix in SkiaSharp 3.x in May 2024; community reporter confirmed fix in June 2024. Current code in main uses canvas-based SetPixel that correctly handles alpha types. |
| Suggested repro platform | linux |

### Automatable Actions

| Type | Risk | Confidence | Description | Details |
|------|------|------------|-------------|---------|
| update-labels | low | 0.92 (92%) | Correct area label from area/SkiaSharp.Views to area/SkiaSharp; add tenet/reliability | labels=type/bug, area/SkiaSharp, os/Windows-Classic, tenet/reliability |
| add-comment | medium | 0.90 (90%) | Inform users the fix is in SkiaSharp 3.x and close the issue | — |
| close-issue | medium | 0.90 (90%) | Close as fixed — confirmed resolved in SkiaSharp 3.x | stateReason=completed |

**Comment draft for `add-comment`:**

```markdown
This issue was fixed in SkiaSharp 3.x. `SKBitmap.SetPixel` now correctly stores unpremultiplied pixel values when the bitmap is created with `SKAlphaType.Unpremul`.

If you are still on SkiaSharp 2.x, you can work around this by writing pixel data directly via `GetPixelSpan()` rather than using `SetPixel`.

Please upgrade to SkiaSharp 3.x to get the fix. Closing this issue.
```

<details>
<summary>Raw JSON</summary>

```json
{
  "meta": {
    "schemaVersion": "1.0",
    "number": 769,
    "repo": "mono/SkiaSharp",
    "analyzedAt": "2026-04-24T04:08:15Z",
    "currentLabels": [
      "type/bug",
      "os/Windows-Classic",
      "area/SkiaSharp.Views",
      "status/needs-attention"
    ]
  },
  "summary": "SKBitmap.SetPixel premultiplies RGB channel values when the bitmap is configured with SKAlphaType.Unpremul, causing incorrect pixel bytes on read-back; confirmed fixed in SkiaSharp 3.x.",
  "classification": {
    "type": {
      "value": "type/bug",
      "confidence": 0.97
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
      "errorType": "wrong-output",
      "errorMessage": "bytes[0] == 100 instead of 255 when alpha=100 and RGB=255 on SKAlphaType.Unpremul bitmap",
      "reproQuality": "complete",
      "targetFrameworks": [
        ".NETFramework",
        "net-windows"
      ]
    },
    "reproEvidence": {
      "stepsToReproduce": [
        "Create SKBitmap(16,16, SKColorType.Rgba8888, SKAlphaType.Unpremul)",
        "Call SetPixel(0, 0, new SKColor(red:255, green:255, blue:255, alpha:100))",
        "Read Bytes[] — RGB channels are 100 instead of 255"
      ],
      "environmentDetails": "SkiaSharp 1.60.3, Visual Studio, Windows 10, Full .NET Framework",
      "repoLinks": [
        {
          "url": "https://devdiv.visualstudio.com/DevDiv/_workitems/edit/782039",
          "description": "VS internal bug #782039 cross-reference"
        }
      ]
    },
    "versionAnalysis": {
      "mentionedVersions": [
        "1.60.3",
        "2.88.8"
      ],
      "brokeIn": "1.60.3",
      "currentRelevance": "unlikely",
      "relevanceReason": "Maintainer confirmed fixed in SkiaSharp 3.x; community member verified the fix in June 2024."
    },
    "fixStatus": {
      "likelyFixed": true,
      "confidence": 0.92,
      "reason": "Maintainer mattleibow commented 'This appears to be fixed in SkiaSharp 3.x releases' in May 2024; user neptuwunium confirmed 'It does appear to be fixed' in June 2024. Current SetPixel uses SKCanvas.DrawPoint which properly respects bitmap alpha type.",
      "fixedInVersion": "3.x"
    }
  },
  "analysis": {
    "summary": "SKBitmap.SetPixel was incorrectly premultiplying pixel RGB channels when the bitmap alpha type was SKAlphaType.Unpremul; this caused stored bytes to reflect premultiplied values (e.g., 255*100/255≈100) rather than the raw channel values. The bug was present through SkiaSharp 2.88.8 and is confirmed fixed in SkiaSharp 3.x, likely due to a rewrite of SetPixel to use SKCanvas.DrawPoint which correctly handles the alpha type.",
    "rationale": "Bug classification is clear: reproducible code, specific wrong-output symptom, confirmed versions. Area is area/SkiaSharp (SKBitmap is core, not views; the existing area/SkiaSharp.Views label is incorrect). Action is close-as-fixed because both the maintainer and a community reporter have confirmed resolution in SkiaSharp 3.x.",
    "keySignals": [
      {
        "text": "bytes[0] == 255 //Blue. Expected 255, result 100",
        "source": "issue body",
        "interpretation": "RGB channels premultiplied by alpha (255 * 100/255 ≈ 100), indicating Unpremul setting is not respected by SetPixel."
      },
      {
        "text": "This issue still happens five years later on SkiaSharp 2.88.8",
        "source": "comment by neptuwunium, 2024-05-31",
        "interpretation": "Bug persisted across all 2.x releases, not just the original report version."
      },
      {
        "text": "This appears to be fixed in SkiaSharp 3.x releases.",
        "source": "comment by mattleibow, 2024-05-31",
        "interpretation": "Maintainer confirms fix in SkiaSharp 3.x."
      },
      {
        "text": "It does appear to be fixed.",
        "source": "comment by neptuwunium, 2024-06-02",
        "interpretation": "Community confirmation that SkiaSharp 3.x resolves the issue."
      }
    ],
    "codeInvestigation": [
      {
        "file": "binding/SkiaSharp/SKBitmap.cs",
        "lines": "131-141",
        "finding": "SetPixel currently creates an SKCanvas and calls canvas.DrawPoint(x, y, color). Canvas drawing in Skia respects the bitmap's alpha type, so this implementation should correctly store unpremultiplied values on an Unpremul bitmap. This is likely the fixed implementation in SkiaSharp 3.x.",
        "relevance": "direct"
      },
      {
        "file": "binding/SkiaSharp/SKBitmap.cs",
        "lines": "320-326",
        "finding": "Bytes property uses GetPixelSpan().ToArray() which reads raw pixel memory directly. If SetPixel correctly writes unpremultiplied values, Bytes will return the correct raw bytes. The fix in SetPixel would make Bytes return correct data.",
        "relevance": "direct"
      },
      {
        "file": "binding/SkiaSharp/SKBitmap.cs",
        "lines": "439-442",
        "finding": "Decode() overrides Unpremul to Premul when decoding from codec — intentional design decision for codec decode path, unrelated to SetPixel.",
        "relevance": "context"
      }
    ],
    "workarounds": [
      "Upgrade to SkiaSharp 3.x where the issue is fixed.",
      "On SkiaSharp 2.x: write pixel bytes directly via GetPixelSpan() or use Marshal/unsafe to write to the raw pixel buffer instead of calling SetPixel."
    ],
    "nextQuestions": [
      "Was SetPixel previously calling a native sk_bitmap_set_pixel that did not respect alpha type?"
    ],
    "resolution": {
      "hypothesis": "In SkiaSharp 2.x, SetPixel likely called a native function that internally converted color through premultiplied paths, ignoring the bitmap's declared alpha type. The rewrite to canvas.DrawPoint in SkiaSharp 3.x correctly routes through Skia's alpha-aware drawing pipeline.",
      "proposals": [
        {
          "title": "Close as fixed in SkiaSharp 3.x",
          "description": "The issue is confirmed fixed in SkiaSharp 3.x by both the maintainer and a community reporter. Close with a note directing users to upgrade.",
          "category": "fix",
          "confidence": 0.92,
          "effort": "cost/xs",
          "validated": "untested"
        }
      ],
      "recommendedProposal": "Close as fixed in SkiaSharp 3.x",
      "recommendedReason": "Both maintainer and user have confirmed the fix is present in 3.x. The issue should be closed to reduce backlog noise."
    }
  },
  "output": {
    "actionability": {
      "suggestedAction": "close-as-fixed",
      "confidence": 0.9,
      "reason": "Maintainer confirmed fix in SkiaSharp 3.x in May 2024; community reporter confirmed fix in June 2024. Current code in main uses canvas-based SetPixel that correctly handles alpha types.",
      "suggestedReproPlatform": "linux"
    },
    "actions": [
      {
        "type": "update-labels",
        "description": "Correct area label from area/SkiaSharp.Views to area/SkiaSharp; add tenet/reliability",
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
        "description": "Inform users the fix is in SkiaSharp 3.x and close the issue",
        "risk": "medium",
        "confidence": 0.9,
        "comment": "This issue was fixed in SkiaSharp 3.x. `SKBitmap.SetPixel` now correctly stores unpremultiplied pixel values when the bitmap is created with `SKAlphaType.Unpremul`.\n\nIf you are still on SkiaSharp 2.x, you can work around this by writing pixel data directly via `GetPixelSpan()` rather than using `SetPixel`.\n\nPlease upgrade to SkiaSharp 3.x to get the fix. Closing this issue."
      },
      {
        "type": "close-issue",
        "description": "Close as fixed — confirmed resolved in SkiaSharp 3.x",
        "risk": "medium",
        "confidence": 0.9,
        "stateReason": "completed"
      }
    ]
  }
}
```

</details>
