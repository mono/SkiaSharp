# Issue Triage Report — #4150

| Field | Value |
|-------|-------|
| Repository | mono/SkiaSharp |
| Analyzed | 2026-09-25T04:28:00Z |
| Type | type/bug (0.99 (99%)) |
| Area | area/SkiaSharp.Views (0.97 (97%)) |
| Suggested action | ready-to-fix (0.93 (93%)) |

**Issue Summary:** SkiaSharp.Views conversion helpers use packed SKImageInfo sizes instead of the actual pixmap or bitmap stride, producing misaligned rows for subset or otherwise strided pixel buffers; this has been confirmed for the Apple conversions on an iOS simulator.

**Analysis:** The conversion helpers conflate SKImageInfo's packed row size and byte size with a pixel object's actual storage layout. SKPixmap.RowBytes reads the native stride, but Apple ToCGImage and WPF conversion code pass packed values to consumers that must traverse the underlying pixel buffer, so a subset or padded buffer is interpreted with incorrect row boundaries. The Windows Forms direct platform-color path already supplies pixmap.RowBytes, while its non-platform-color fallback delegates to ReadPixels rather than manually using packed layout.

**Recommendations:** **ready-to-fix** — The affected calls and the required distinction between packed and actual stride are established by source inspection and an independent iOS confirmation; implementation should include platform conversion regression tests.

---

## Classification

| Field | Value |
|-------|-------|
| Type | type/bug |
| Area | area/SkiaSharp.Views |
| Platforms | os/iOS, os/Windows-Classic |
| Backends | — |
| Tenets | tenet/reliability |
| Perf | — |
| Partner | — |
| Current labels | type/bug |

## Evidence

### Reproduction

1. Create a non-zero-origin subset of a larger bitmap so the subset retains the parent row stride.
2. Convert the subset through SKPixmap.ToCGImage(), SKBitmap.ToCGImage(), or a destination-pixmap conversion helper.
3. Compare a later row with the source subset; packed-stride handling interprets padding as pixel data or writes at the wrong offset.

**Environment:** Confirmed in SkiaSharp/SkiaSharp.Views 4.151.0 on an iOS 26.5 ARM64 simulator; the report also identifies WPF and Windows Forms conversion paths.

**Related issues:** #4137, #2719, #4111

**Repository links:**
- https://github.com/mono/SkiaSharp/issues/4137 — Related SKPixmap RowBytes bug fixed by mono/SkiaSharp#4148; this issue is explicitly limited to separate SkiaSharp.Views conversion paths.
- https://github.com/mono/SkiaSharp/issues/2719 — Open report that a subset bitmap retains its parent RowBytes while its Info.RowBytes changes, which is the condition relevant to these conversions.
- https://github.com/mono/SkiaSharp/issues/4111 — Earlier fixed incorrect SKPixmap row-offset calculation, showing the same broader stride-sensitive behavior.

### Bug Signals

| Field | Value |
|-------|-------|
| Severity | medium |
| Regression claimed | False |
| Error type | wrong-output |
| Error message | Rows are read or written at packed offsets when the pixmap or bitmap has a larger actual stride. |
| Repro quality | partial |
| Target frameworks | — |

### Version Analysis

| Field | Value |
|-------|-------|
| Mentioned versions | 4.151.0 |
| Worked in | — |
| Broke in | — |
| Current relevance | likely |
| Relevance reason | The current source retains the cited uses of info.BytesSize and info.RowBytes in the affected conversions. |

## Analysis

### Technical Summary

The conversion helpers conflate SKImageInfo's packed row size and byte size with a pixel object's actual storage layout. SKPixmap.RowBytes reads the native stride, but Apple ToCGImage and WPF conversion code pass packed values to consumers that must traverse the underlying pixel buffer, so a subset or padded buffer is interpreted with incorrect row boundaries. The Windows Forms direct platform-color path already supplies pixmap.RowBytes, while its non-platform-color fallback delegates to ReadPixels rather than manually using packed layout.

### Rationale

This is a reproducible wrong-output bug in SkiaSharp.Views: the report identifies exact calls, source inspection confirms them, and an independent reporter confirmed the Apple behavior on iOS. It is not a duplicate of #4137 because #4137 fixed GetPixelSpan in the core binding, whereas the affected conversion calls remain in SkiaSharp.Views. The impact is incorrect conversion output and an undersized provider length for strided pixmaps, but no crash or data loss has been reported, supporting medium severity.

### Key Signals

- "Both SKBitmap.ToCGImage() and SKPixmap.ToCGImage() ignore the actual row stride." — **issue comment #5785732650** (Independent confirmation on an iOS simulator establishes real incorrect output in the Apple conversion path.)
- "This is separate from #4137 (which is about GetPixelSpan) and pre-existing." — **issue body** (The related core-binding fix does not resolve the Views conversion calls.)
- "A non-zero-origin subset of a larger bitmap ... shares the parent buffer." — **issue body** (A subset can have an actual stride larger than its packed image-info row size.)

### Code Investigation

| File | Lines | Relevance | Finding |
|------|-------|-----------|---------|
| `source/SkiaSharp.Views/SkiaSharp.Views/Platform/Apple/AppleExtensions.cs` | 229-267 | direct | SKPixmap.ToCGImage passes info.BytesSize to CGDataProvider and info.RowBytes to CGImage; SKBitmap.ToCGImage passes its native byte count to the provider but still passes info.RowBytes to CGImage. |
| `source/SkiaSharp.Views/SkiaSharp.Views.WPF/WPFExtensions.cs` | 189-205 | direct | BitmapSource.ToSKPixmap calls CopyPixels with info.BytesSize and info.RowBytes instead of the destination pixmap's actual capacity and RowBytes. |
| `source/SkiaSharp.Views/SkiaSharp.Views.WindowsForms/Extensions.Desktop.cs` | 224-249 | related | The platform-color conversion constructs the temporary System.Drawing.Bitmap with pixmap.RowBytes; the fallback delegates to ReadPixels, so it does not reproduce the WPF manual packed-stride call pattern. |
| `binding/SkiaSharp/SKPixmap.cs` | 155-165 | direct | SKPixmap.RowBytes returns sk_pixmap_get_row_bytes from the native pixmap, exposing the actual storage stride rather than a calculated packed row size. |
| `binding/SkiaSharp/SKImageInfo.cs` | 175-190 | direct | SKImageInfo.BytesSize and RowBytes are calculated as Width * Height * BytesPerPixel and Width * BytesPerPixel, respectively, so they are packed-layout values. |

### Workarounds

- Until the helpers are fixed, avoid passing an ExtractSubset result or another manually strided pixmap or bitmap directly to the affected conversion helpers; first copy it into a newly allocated packed bitmap or image.
- For Windows Forms, the direct platform-color path already uses pixmap.RowBytes; retain the original color type where practical to avoid its conversion fallback.

### Next Questions

- Determine the correct byte-count expression for a subset pointer passed to CGDataProvider, including whether it must include the final row's full stride or only reachable pixels.
- Add platform-hosted regression tests for Apple, WPF, and Windows Forms using a non-zero-origin subset or explicitly padded stride.
- Audit other SkiaSharp.Views conversion helpers for manual uses of SKImageInfo.BytesSize or SKImageInfo.RowBytes with an existing pixmap or bitmap buffer.

### Resolution Proposals

**Hypothesis:** Replace packed SKImageInfo layout arguments with the source or destination object's actual RowBytes and corresponding reachable native byte count wherever a conversion API consumes that object's pixel buffer.

1. **Honor actual strides in Views conversions** — fix, confidence 0.94 (94%), cost/m, validated=untested
   - Update the Apple and WPF conversion paths to use SKPixmap.RowBytes or SKBitmap.RowBytes and the correct native buffer extent, then add strided-subset regression coverage for each affected platform host.
2. **Materialize a packed copy before conversion** — workaround, confidence 0.82 (82%), cost/xs, validated=untested
   - As a temporary mitigation, copy a subset or manually strided pixel buffer into a newly allocated packed bitmap or image before calling the affected conversion helper.

**Recommended proposal:** Honor actual strides in Views conversions

**Why:** The faulty arguments are directly visible in the conversion methods and a regression test can distinguish packed from actual row traversal without changing the public API.

## Recommendations

### Actionability

| Field | Value |
|-------|-------|
| Suggested action | ready-to-fix |
| Confidence | 0.93 (93%) |
| Reason | The affected calls and the required distinction between packed and actual stride are established by source inspection and an independent iOS confirmation; implementation should include platform conversion regression tests. |
| Suggested repro platform | macos |

### Automatable Actions

| Type | Risk | Confidence | Description | Details |
|------|------|------------|-------------|---------|
| update-labels | low | 0.97 (97%) | Apply the Views, platform, and reliability labels for the confirmed stride-conversion defect. | labels=type/bug, area/SkiaSharp.Views, os/iOS, os/Windows-Classic, tenet/reliability |
| link-related | low | 0.95 (95%) | Link related core stride issue #4137 to clarify that its merged GetPixelSpan fix does not cover these Views conversions. | linkedIssue=#4137 |

<details>
<summary>Raw JSON</summary>

```json
{
  "meta": {
    "schemaVersion": "1.0",
    "number": 4150,
    "repo": "mono/SkiaSharp",
    "analyzedAt": "2026-09-25T04:28:00Z",
    "currentLabels": [
      "type/bug"
    ]
  },
  "summary": "SkiaSharp.Views conversion helpers use packed SKImageInfo sizes instead of the actual pixmap or bitmap stride, producing misaligned rows for subset or otherwise strided pixel buffers; this has been confirmed for the Apple conversions on an iOS simulator.",
  "classification": {
    "type": {
      "value": "type/bug",
      "confidence": 0.99
    },
    "area": {
      "value": "area/SkiaSharp.Views",
      "confidence": 0.97
    },
    "platforms": [
      "os/iOS",
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
      "errorMessage": "Rows are read or written at packed offsets when the pixmap or bitmap has a larger actual stride.",
      "reproQuality": "partial"
    },
    "reproEvidence": {
      "stepsToReproduce": [
        "Create a non-zero-origin subset of a larger bitmap so the subset retains the parent row stride.",
        "Convert the subset through SKPixmap.ToCGImage(), SKBitmap.ToCGImage(), or a destination-pixmap conversion helper.",
        "Compare a later row with the source subset; packed-stride handling interprets padding as pixel data or writes at the wrong offset."
      ],
      "environmentDetails": "Confirmed in SkiaSharp/SkiaSharp.Views 4.151.0 on an iOS 26.5 ARM64 simulator; the report also identifies WPF and Windows Forms conversion paths.",
      "relatedIssues": [
        4137,
        2719,
        4111
      ],
      "repoLinks": [
        {
          "url": "https://github.com/mono/SkiaSharp/issues/4137",
          "description": "Related SKPixmap RowBytes bug fixed by mono/SkiaSharp#4148; this issue is explicitly limited to separate SkiaSharp.Views conversion paths."
        },
        {
          "url": "https://github.com/mono/SkiaSharp/issues/2719",
          "description": "Open report that a subset bitmap retains its parent RowBytes while its Info.RowBytes changes, which is the condition relevant to these conversions."
        },
        {
          "url": "https://github.com/mono/SkiaSharp/issues/4111",
          "description": "Earlier fixed incorrect SKPixmap row-offset calculation, showing the same broader stride-sensitive behavior."
        }
      ]
    },
    "versionAnalysis": {
      "mentionedVersions": [
        "4.151.0"
      ],
      "currentRelevance": "likely",
      "relevanceReason": "The current source retains the cited uses of info.BytesSize and info.RowBytes in the affected conversions."
    }
  },
  "analysis": {
    "summary": "The conversion helpers conflate SKImageInfo's packed row size and byte size with a pixel object's actual storage layout. SKPixmap.RowBytes reads the native stride, but Apple ToCGImage and WPF conversion code pass packed values to consumers that must traverse the underlying pixel buffer, so a subset or padded buffer is interpreted with incorrect row boundaries. The Windows Forms direct platform-color path already supplies pixmap.RowBytes, while its non-platform-color fallback delegates to ReadPixels rather than manually using packed layout.",
    "rationale": "This is a reproducible wrong-output bug in SkiaSharp.Views: the report identifies exact calls, source inspection confirms them, and an independent reporter confirmed the Apple behavior on iOS. It is not a duplicate of #4137 because #4137 fixed GetPixelSpan in the core binding, whereas the affected conversion calls remain in SkiaSharp.Views. The impact is incorrect conversion output and an undersized provider length for strided pixmaps, but no crash or data loss has been reported, supporting medium severity.",
    "keySignals": [
      {
        "text": "Both SKBitmap.ToCGImage() and SKPixmap.ToCGImage() ignore the actual row stride.",
        "source": "issue comment #5785732650",
        "interpretation": "Independent confirmation on an iOS simulator establishes real incorrect output in the Apple conversion path."
      },
      {
        "text": "This is separate from #4137 (which is about GetPixelSpan) and pre-existing.",
        "source": "issue body",
        "interpretation": "The related core-binding fix does not resolve the Views conversion calls."
      },
      {
        "text": "A non-zero-origin subset of a larger bitmap ... shares the parent buffer.",
        "source": "issue body",
        "interpretation": "A subset can have an actual stride larger than its packed image-info row size."
      }
    ],
    "codeInvestigation": [
      {
        "file": "source/SkiaSharp.Views/SkiaSharp.Views/Platform/Apple/AppleExtensions.cs",
        "lines": "229-267",
        "finding": "SKPixmap.ToCGImage passes info.BytesSize to CGDataProvider and info.RowBytes to CGImage; SKBitmap.ToCGImage passes its native byte count to the provider but still passes info.RowBytes to CGImage.",
        "relevance": "direct"
      },
      {
        "file": "source/SkiaSharp.Views/SkiaSharp.Views.WPF/WPFExtensions.cs",
        "lines": "189-205",
        "finding": "BitmapSource.ToSKPixmap calls CopyPixels with info.BytesSize and info.RowBytes instead of the destination pixmap's actual capacity and RowBytes.",
        "relevance": "direct"
      },
      {
        "file": "source/SkiaSharp.Views/SkiaSharp.Views.WindowsForms/Extensions.Desktop.cs",
        "lines": "224-249",
        "finding": "The platform-color conversion constructs the temporary System.Drawing.Bitmap with pixmap.RowBytes; the fallback delegates to ReadPixels, so it does not reproduce the WPF manual packed-stride call pattern.",
        "relevance": "related"
      },
      {
        "file": "binding/SkiaSharp/SKPixmap.cs",
        "lines": "155-165",
        "finding": "SKPixmap.RowBytes returns sk_pixmap_get_row_bytes from the native pixmap, exposing the actual storage stride rather than a calculated packed row size.",
        "relevance": "direct"
      },
      {
        "file": "binding/SkiaSharp/SKImageInfo.cs",
        "lines": "175-190",
        "finding": "SKImageInfo.BytesSize and RowBytes are calculated as Width * Height * BytesPerPixel and Width * BytesPerPixel, respectively, so they are packed-layout values.",
        "relevance": "direct"
      }
    ],
    "workarounds": [
      "Until the helpers are fixed, avoid passing an ExtractSubset result or another manually strided pixmap or bitmap directly to the affected conversion helpers; first copy it into a newly allocated packed bitmap or image.",
      "For Windows Forms, the direct platform-color path already uses pixmap.RowBytes; retain the original color type where practical to avoid its conversion fallback."
    ],
    "nextQuestions": [
      "Determine the correct byte-count expression for a subset pointer passed to CGDataProvider, including whether it must include the final row's full stride or only reachable pixels.",
      "Add platform-hosted regression tests for Apple, WPF, and Windows Forms using a non-zero-origin subset or explicitly padded stride.",
      "Audit other SkiaSharp.Views conversion helpers for manual uses of SKImageInfo.BytesSize or SKImageInfo.RowBytes with an existing pixmap or bitmap buffer."
    ],
    "resolution": {
      "hypothesis": "Replace packed SKImageInfo layout arguments with the source or destination object's actual RowBytes and corresponding reachable native byte count wherever a conversion API consumes that object's pixel buffer.",
      "proposals": [
        {
          "title": "Honor actual strides in Views conversions",
          "description": "Update the Apple and WPF conversion paths to use SKPixmap.RowBytes or SKBitmap.RowBytes and the correct native buffer extent, then add strided-subset regression coverage for each affected platform host.",
          "category": "fix",
          "validated": "untested",
          "confidence": 0.94,
          "effort": "cost/m"
        },
        {
          "title": "Materialize a packed copy before conversion",
          "description": "As a temporary mitigation, copy a subset or manually strided pixel buffer into a newly allocated packed bitmap or image before calling the affected conversion helper.",
          "category": "workaround",
          "validated": "untested",
          "confidence": 0.82,
          "effort": "cost/xs"
        }
      ],
      "recommendedProposal": "Honor actual strides in Views conversions",
      "recommendedReason": "The faulty arguments are directly visible in the conversion methods and a regression test can distinguish packed from actual row traversal without changing the public API."
    }
  },
  "output": {
    "actionability": {
      "suggestedAction": "ready-to-fix",
      "confidence": 0.93,
      "reason": "The affected calls and the required distinction between packed and actual stride are established by source inspection and an independent iOS confirmation; implementation should include platform conversion regression tests.",
      "suggestedReproPlatform": "macos"
    },
    "actions": [
      {
        "type": "update-labels",
        "description": "Apply the Views, platform, and reliability labels for the confirmed stride-conversion defect.",
        "risk": "low",
        "confidence": 0.97,
        "labels": [
          "type/bug",
          "area/SkiaSharp.Views",
          "os/iOS",
          "os/Windows-Classic",
          "tenet/reliability"
        ]
      },
      {
        "type": "link-related",
        "description": "Link related core stride issue #4137 to clarify that its merged GetPixelSpan fix does not cover these Views conversions.",
        "risk": "low",
        "confidence": 0.95,
        "linkedIssue": 4137
      }
    ]
  }
}
```

</details>
