# Issue Triage Report — #1552

| Field | Value |
|-------|-------|
| Repository | mono/SkiaSharp |
| Analyzed | 2026-05-03T10:47:00Z |
| Type | type/bug (0.82 (82%)) |
| Area | area/SkiaSharp (0.88 (88%)) |
| Suggested action | needs-info (0.78 (78%)) |

**Issue Summary:** On iOS, SKColorType.Bgra8888 with SKAlphaType.Opaque does not honor the opaque alpha setting — the alpha channel is always treated as meaningful — while the same configuration works correctly on Android and UWP.

**Analysis:** Reporter expects that Bgra8888 + Opaque alphaType (effectively treating the alpha byte as 'X'/unused — the BGRX8888 pattern) works consistently on iOS as it does on Android and UWP. On iOS the alpha byte appears to be honored during compositing regardless of the Opaque flag. The C# binding's GetAlphaType() correctly allows Opaque for Bgra8888 (it falls in the 'any' case), so this is likely an iOS-platform-level constraint — Metal may not natively support a BGRA opaque format and may default to premultiplied behavior.

**Recommendations:** **needs-info** — No SkiaSharp version provided. Minimal code snippet only — no complete repro. Need version and confirmation this still reproduces on current SkiaSharp.

---

## Classification

| Field | Value |
|-------|-------|
| Type | type/bug |
| Area | area/SkiaSharp |
| Platforms | os/iOS |
| Backends | — |
| Tenets | tenet/compatibility |
| Partner | — |

## Evidence

### Reproduction

1. Create an SKImageInfo with colorType: SKColorType.Bgra8888, alphaType: SKAlphaType.Opaque
2. Use this info to process pixel data on iOS
3. Observe that alpha channel is NOT treated as opaque — it is still composited

**Environment:** Xamarin.Forms, iOS platform, SkiaSharp (version not specified)

### Bug Signals

| Field | Value |
|-------|-------|
| Severity | medium |
| Regression claimed | False |
| Error type | wrong-output |
| Error message | — |
| Repro quality | partial |
| Target frameworks | Xamarin.iOS |

### Version Analysis

| Field | Value |
|-------|-------|
| Mentioned versions | — |
| Worked in | — |
| Broke in | — |
| Current relevance | unknown |
| Relevance reason | No SkiaSharp version was specified. The issue dates from 2020 and the current codebase has no dedicated Bgr888x color type, so the core question is still relevant. |

## Analysis

### Technical Summary

Reporter expects that Bgra8888 + Opaque alphaType (effectively treating the alpha byte as 'X'/unused — the BGRX8888 pattern) works consistently on iOS as it does on Android and UWP. On iOS the alpha byte appears to be honored during compositing regardless of the Opaque flag. The C# binding's GetAlphaType() correctly allows Opaque for Bgra8888 (it falls in the 'any' case), so this is likely an iOS-platform-level constraint — Metal may not natively support a BGRA opaque format and may default to premultiplied behavior.

### Rationale

The bug classification is supported by: (1) the behavior works on Android and UWP but not iOS, indicating a platform-specific inconsistency; (2) the C# binding code does not strip the Opaque alpha type for Bgra8888; (3) the missing Bgr888x color type in the enum means there is no dedicated 'ignore alpha' BGR format available.

### Key Signals

- "The setting of SKAlphaType is taken into account and the color scheme BGRX is possible on iOS." — **issue body — Expected Behavior** (Reporter wants Bgra8888+Opaque to behave as BGRX (alpha byte ignored). On iOS this does not happen.)
- "The same setting works on Android and UWP." — **issue body** (Platform-specific discrepancy — the Opaque alphaType is respected elsewhere, indicating an iOS or Metal backend constraint.)

### Code Investigation

| File | Lines | Relevance | Finding |
|------|-------|-----------|---------|
| `binding/SkiaSharp/Definitions.cs` | 182-194 | direct | SKColorType.Bgra8888 falls under the 'any' case in GetAlphaType() — the C# binding accepts Opaque, Premul, and Unpremul without override. The binding does not suppress the Opaque setting. |
| `binding/SkiaSharp/Definitions.cs` | 36-66 | direct | SKColorType enum does not have a dedicated Bgr888x entry (analogous to Rgb888x = 5). Rgb888x exists for RGB with alpha=X, but the BGR equivalent is absent. Bgr101010x is present but not Bgr888x. |
| `binding/SkiaSharp/SKImageInfo.cs` | 14-16 | related | SKImageInfoNative conversion passes alphaType directly to native without transformation — no layer intercepting the Opaque value. |

### Workarounds

- Use SKColorType.Rgba8888 with SKAlphaType.Opaque — Android, iOS and UWP all support this combination and the alpha byte will be ignored during compositing.
- Manually set every pixel's alpha byte to 0xFF before passing the buffer to SkiaSharp on iOS.

### Next Questions

- Which SkiaSharp version does the reporter use?
- Is the issue observable in a pixel readback scenario or only during rendering/compositing?
- Does the same issue occur on current SkiaSharp versions (2.88.x / 3.x)?
- Is the backend Metal or OpenGL on the affected iOS device?

### Resolution Proposals

**Hypothesis:** iOS (Metal backend) does not support treating BGRA8888 as opaque/X — the alpha channel is always composited. The fix would require either adding a native Bgr888x color type mapping or ensuring the Metal surface creation respects the Opaque flag for BGRA8888.

1. **Use Rgba8888 + Opaque as a workaround** — workaround, confidence 0.80 (80%), cost/xs, validated=untested
   - Switch to SKColorType.Rgba8888 with SKAlphaType.Opaque. This combination is supported on all platforms and the alpha byte is treated as unused (opaque) consistently.
2. **Investigate adding Bgr888x color type** — investigation, confidence 0.60 (60%), cost/m, validated=untested
   - Skia upstream may support kBGR_888x_SkColorType. If so, add Bgr888x to the SKColorType enum and map it in EnumMappings.cs. This would give a dedicated opaque BGR format that matches Rgb888x.

**Recommended proposal:** Use Rgba8888 + Opaque as a workaround

**Why:** Fastest path for the reporter. A dedicated Bgr888x fix requires deeper investigation into iOS/Metal format support.

## Recommendations

### Actionability

| Field | Value |
|-------|-------|
| Suggested action | needs-info |
| Confidence | 0.78 (78%) |
| Reason | No SkiaSharp version provided. Minimal code snippet only — no complete repro. Need version and confirmation this still reproduces on current SkiaSharp. |
| Suggested repro platform | macos |

### Missing Info

- SkiaSharp version number
- Complete minimal reproduction project or code
- Whether the issue reproduces with the current SkiaSharp version
- Specific iOS device or simulator and iOS version

### Automatable Actions

| Type | Risk | Confidence | Description | Details |
|------|------|------------|-------------|---------|
| update-labels | low | 0.88 (88%) | Apply bug, area, platform, and tenet labels | labels=type/bug, area/SkiaSharp, os/iOS, tenet/compatibility |
| add-comment | medium | 0.78 (78%) | Request version info and offer workaround | — |

**Comment draft for `add-comment`:**

```markdown
Thanks for the report! To help investigate this, could you provide:

1. **SkiaSharp version** you are using
2. **iOS version** and device/simulator
3. A **minimal reproduction** showing the unexpected behavior (e.g., pixel values before and after processing)

In the meantime, a possible workaround is to use `SKColorType.Rgba8888` with `SKAlphaType.Opaque` instead — this combination treats the alpha byte as unused/opaque consistently across all platforms:

```csharp
var info = new SKImageInfo(w, h, SKColorType.Rgba8888, SKAlphaType.Opaque);
```

Note that a dedicated `Bgr888x` color type (analogous to `Rgb888x`) does not currently exist in SkiaSharp, so `Bgra8888 + Opaque` is the closest equivalent — whether the iOS Metal backend honors the Opaque flag for this format needs further investigation.
```

<details>
<summary>Raw JSON</summary>

```json
{
  "meta": {
    "schemaVersion": "1.0",
    "number": 1552,
    "repo": "mono/SkiaSharp",
    "analyzedAt": "2026-05-03T10:47:00Z"
  },
  "summary": "On iOS, SKColorType.Bgra8888 with SKAlphaType.Opaque does not honor the opaque alpha setting — the alpha channel is always treated as meaningful — while the same configuration works correctly on Android and UWP.",
  "classification": {
    "type": {
      "value": "type/bug",
      "confidence": 0.82
    },
    "area": {
      "value": "area/SkiaSharp",
      "confidence": 0.88
    },
    "platforms": [
      "os/iOS"
    ],
    "tenets": [
      "tenet/compatibility"
    ]
  },
  "evidence": {
    "bugSignals": {
      "severity": "medium",
      "regressionClaimed": false,
      "errorType": "wrong-output",
      "reproQuality": "partial",
      "targetFrameworks": [
        "Xamarin.iOS"
      ]
    },
    "reproEvidence": {
      "stepsToReproduce": [
        "Create an SKImageInfo with colorType: SKColorType.Bgra8888, alphaType: SKAlphaType.Opaque",
        "Use this info to process pixel data on iOS",
        "Observe that alpha channel is NOT treated as opaque — it is still composited"
      ],
      "environmentDetails": "Xamarin.Forms, iOS platform, SkiaSharp (version not specified)"
    },
    "versionAnalysis": {
      "mentionedVersions": [],
      "currentRelevance": "unknown",
      "relevanceReason": "No SkiaSharp version was specified. The issue dates from 2020 and the current codebase has no dedicated Bgr888x color type, so the core question is still relevant."
    }
  },
  "analysis": {
    "summary": "Reporter expects that Bgra8888 + Opaque alphaType (effectively treating the alpha byte as 'X'/unused — the BGRX8888 pattern) works consistently on iOS as it does on Android and UWP. On iOS the alpha byte appears to be honored during compositing regardless of the Opaque flag. The C# binding's GetAlphaType() correctly allows Opaque for Bgra8888 (it falls in the 'any' case), so this is likely an iOS-platform-level constraint — Metal may not natively support a BGRA opaque format and may default to premultiplied behavior.",
    "rationale": "The bug classification is supported by: (1) the behavior works on Android and UWP but not iOS, indicating a platform-specific inconsistency; (2) the C# binding code does not strip the Opaque alpha type for Bgra8888; (3) the missing Bgr888x color type in the enum means there is no dedicated 'ignore alpha' BGR format available.",
    "keySignals": [
      {
        "text": "The setting of SKAlphaType is taken into account and the color scheme BGRX is possible on iOS.",
        "source": "issue body — Expected Behavior",
        "interpretation": "Reporter wants Bgra8888+Opaque to behave as BGRX (alpha byte ignored). On iOS this does not happen."
      },
      {
        "text": "The same setting works on Android and UWP.",
        "source": "issue body",
        "interpretation": "Platform-specific discrepancy — the Opaque alphaType is respected elsewhere, indicating an iOS or Metal backend constraint."
      }
    ],
    "codeInvestigation": [
      {
        "file": "binding/SkiaSharp/Definitions.cs",
        "lines": "182-194",
        "finding": "SKColorType.Bgra8888 falls under the 'any' case in GetAlphaType() — the C# binding accepts Opaque, Premul, and Unpremul without override. The binding does not suppress the Opaque setting.",
        "relevance": "direct"
      },
      {
        "file": "binding/SkiaSharp/Definitions.cs",
        "lines": "36-66",
        "finding": "SKColorType enum does not have a dedicated Bgr888x entry (analogous to Rgb888x = 5). Rgb888x exists for RGB with alpha=X, but the BGR equivalent is absent. Bgr101010x is present but not Bgr888x.",
        "relevance": "direct"
      },
      {
        "file": "binding/SkiaSharp/SKImageInfo.cs",
        "lines": "14-16",
        "finding": "SKImageInfoNative conversion passes alphaType directly to native without transformation — no layer intercepting the Opaque value.",
        "relevance": "related"
      }
    ],
    "nextQuestions": [
      "Which SkiaSharp version does the reporter use?",
      "Is the issue observable in a pixel readback scenario or only during rendering/compositing?",
      "Does the same issue occur on current SkiaSharp versions (2.88.x / 3.x)?",
      "Is the backend Metal or OpenGL on the affected iOS device?"
    ],
    "workarounds": [
      "Use SKColorType.Rgba8888 with SKAlphaType.Opaque — Android, iOS and UWP all support this combination and the alpha byte will be ignored during compositing.",
      "Manually set every pixel's alpha byte to 0xFF before passing the buffer to SkiaSharp on iOS."
    ],
    "resolution": {
      "hypothesis": "iOS (Metal backend) does not support treating BGRA8888 as opaque/X — the alpha channel is always composited. The fix would require either adding a native Bgr888x color type mapping or ensuring the Metal surface creation respects the Opaque flag for BGRA8888.",
      "proposals": [
        {
          "title": "Use Rgba8888 + Opaque as a workaround",
          "description": "Switch to SKColorType.Rgba8888 with SKAlphaType.Opaque. This combination is supported on all platforms and the alpha byte is treated as unused (opaque) consistently.",
          "category": "workaround",
          "confidence": 0.8,
          "effort": "cost/xs",
          "validated": "untested"
        },
        {
          "title": "Investigate adding Bgr888x color type",
          "description": "Skia upstream may support kBGR_888x_SkColorType. If so, add Bgr888x to the SKColorType enum and map it in EnumMappings.cs. This would give a dedicated opaque BGR format that matches Rgb888x.",
          "category": "investigation",
          "confidence": 0.6,
          "effort": "cost/m",
          "validated": "untested"
        }
      ],
      "recommendedProposal": "Use Rgba8888 + Opaque as a workaround",
      "recommendedReason": "Fastest path for the reporter. A dedicated Bgr888x fix requires deeper investigation into iOS/Metal format support."
    }
  },
  "output": {
    "actionability": {
      "suggestedAction": "needs-info",
      "confidence": 0.78,
      "reason": "No SkiaSharp version provided. Minimal code snippet only — no complete repro. Need version and confirmation this still reproduces on current SkiaSharp.",
      "suggestedReproPlatform": "macos"
    },
    "missingInfo": [
      "SkiaSharp version number",
      "Complete minimal reproduction project or code",
      "Whether the issue reproduces with the current SkiaSharp version",
      "Specific iOS device or simulator and iOS version"
    ],
    "actions": [
      {
        "type": "update-labels",
        "description": "Apply bug, area, platform, and tenet labels",
        "risk": "low",
        "confidence": 0.88,
        "labels": [
          "type/bug",
          "area/SkiaSharp",
          "os/iOS",
          "tenet/compatibility"
        ]
      },
      {
        "type": "add-comment",
        "description": "Request version info and offer workaround",
        "risk": "medium",
        "confidence": 0.78,
        "comment": "Thanks for the report! To help investigate this, could you provide:\n\n1. **SkiaSharp version** you are using\n2. **iOS version** and device/simulator\n3. A **minimal reproduction** showing the unexpected behavior (e.g., pixel values before and after processing)\n\nIn the meantime, a possible workaround is to use `SKColorType.Rgba8888` with `SKAlphaType.Opaque` instead — this combination treats the alpha byte as unused/opaque consistently across all platforms:\n\n```csharp\nvar info = new SKImageInfo(w, h, SKColorType.Rgba8888, SKAlphaType.Opaque);\n```\n\nNote that a dedicated `Bgr888x` color type (analogous to `Rgb888x`) does not currently exist in SkiaSharp, so `Bgra8888 + Opaque` is the closest equivalent — whether the iOS Metal backend honors the Opaque flag for this format needs further investigation."
      }
    ]
  }
}
```

</details>
