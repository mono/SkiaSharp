# Issue Triage Report — #3533

| Field | Value |
|-------|-------|
| Repository | mono/SkiaSharp |
| Analyzed | 2026-05-05T20:21:57Z |
| Type | type/enhancement (0.92 (92%)) |
| Area | area/SkiaSharp.Views.Maui (0.88 (88%)) |
| Suggested action | needs-investigation (0.88 (88%)) |

**Issue Summary:** Enhancement request to standardize SKTouchEventArgs.WheelDelta to the v120 convention (120 units = 1 discrete mouse wheel notch) across all platforms; currently only Windows MAUI passes meaningful values while all other platforms hardcode 0 or lack wheel event handling entirely.

**Analysis:** The WheelDelta property on SKTouchEventArgs exists but has no documented unit contract and is only populated on Windows MAUI (via MouseWheelDelta passthrough). The enhancement proposes adopting the v120 convention (120 units per discrete notch, matching Windows and Linux libinput) and implementing platform-specific normalization for Android, macOS, iOS, Tizen, WASM/Blazor, WPF/WinForms, and GTK. Seven sub-issues (#3534–#3540) are already filed for each platform. This is a well-specified enhancement with clear scope and priority ordering.

**Recommendations:** **needs-investigation** — Well-specified enhancement with clear scope, sub-issues filed, and normalization formulas defined. Ready for implementation work to begin on each platform.

---

## Classification

| Field | Value |
|-------|-------|
| Type | type/enhancement |
| Area | area/SkiaSharp.Views.Maui |
| Platforms | os/Android, os/iOS, os/macOS, os/Windows-Classic, os/Windows-WinUI, os/WASM, os/Tizen, os/Linux |
| Backends | — |
| Tenets | tenet/compatibility, tenet/reliability |
| Partner | — |
| Current labels | type/enhancement, area/SkiaSharp |

## Evidence

### Reproduction

**Environment:** SkiaSharp.Views.Maui.Core - multiple platforms; Windows MAUI is the only platform that currently passes WheelDelta values

**Repository links:**
- https://github.com/mono/SkiaSharp/issues/3534 — Sub-issue: Blazor WASM wheel support
- https://github.com/mono/SkiaSharp/issues/3535 — Sub-issue: Android MAUI wheel support
- https://github.com/mono/SkiaSharp/issues/3536 — Sub-issue: macOS MAUI wheel support
- https://github.com/mono/SkiaSharp/issues/3537 — Sub-issue: iOS/iPadOS + Mac Catalyst wheel support
- https://github.com/mono/SkiaSharp/issues/3538 — Sub-issue: Tizen MAUI wheel support
- https://github.com/mono/SkiaSharp/issues/3539 — Sub-issue: WPF/WinForms wheel support
- https://github.com/mono/SkiaSharp/issues/3540 — Sub-issue: GTK3/4 (Linux) wheel support

## Analysis

### Technical Summary

The WheelDelta property on SKTouchEventArgs exists but has no documented unit contract and is only populated on Windows MAUI (via MouseWheelDelta passthrough). The enhancement proposes adopting the v120 convention (120 units per discrete notch, matching Windows and Linux libinput) and implementing platform-specific normalization for Android, macOS, iOS, Tizen, WASM/Blazor, WPF/WinForms, and GTK. Seven sub-issues (#3534–#3540) are already filed for each platform. This is a well-specified enhancement with clear scope and priority ordering.

### Rationale

The WheelDelta property already exists in the public API (SKTouchEventArgs.WheelDelta) and is populated in the Windows MAUI handler (SKTouchHandler.cs Platform/Windows). All other platform handlers (Android, Apple, Tizen) return 0 because they lack wheel event handling. This is an enhancement to add missing functionality and standardize behavior, not a regression. The v120 unit standard is well-established and the normalization formulas for each platform are clearly specified in the issue.

### Key Signals

- "Only Windows MAUI passes meaningful values (raw ±120 per notch). All other platforms either hardcode 0 or don't handle wheel events at all." — **issue body** (Confirms gap — API surface exists but cross-platform behavior is incomplete.)
- "We need to standardize WheelDelta across all platforms to use the v120 convention: 120 units = 1 discrete mouse wheel notch" — **issue body** (Clear, specific enhancement request with a defined standard.)
- "Seven sub-issues #3534–#3540 already filed per platform" — **issue body** (Work is well-decomposed; this is a tracking/umbrella issue.)

### Code Investigation

| File | Lines | Relevance | Finding |
|------|-------|-----------|---------|
| `source/SkiaSharp.Views.Maui/SkiaSharp.Views.Maui.Core/SKTouchEventArgs.cs` | 50 | direct | WheelDelta is a public int property on SKTouchEventArgs with no documentation or unit contract. Default value is 0 when not specified. |
| `source/SkiaSharp.Views.Maui/SkiaSharp.Views.Maui.Core/Platform/Windows/SKTouchHandler.cs` | 101-124 | direct | OnPointerWheelChanged handler reads MouseWheelDelta from PointerPoint.Properties and passes it directly as wheelDelta. Windows already complies with v120 by passthrough since MouseWheelDelta uses WHEEL_DELTA=120 convention natively. |
| `source/SkiaSharp.Views.Maui/SkiaSharp.Views.Maui.Core/Platform/Android/SKTouchHandler.cs` | — | direct | No WheelChanged handling found — no references to WheelDelta, scroll, or wheel events. SKTouchAction.WheelChanged is never raised on Android. |
| `source/SkiaSharp.Views.Maui/SkiaSharp.Views.Maui.Core/Platform/Apple/SKTouchHandler.cs` | — | direct | No WheelChanged handling found — no scroll event handling for macOS/iOS. SKTouchAction.WheelChanged is never raised on Apple platforms. |
| `source/SkiaSharp.Views.Maui/SkiaSharp.Views.Maui.Core/Platform/Tizen/SKTouchHandler.cs` | — | direct | No WheelChanged handling found — no wheel/scroll event handling on Tizen. |

### Next Questions

- Should the v120 standard be formally documented in XML docs on SKTouchEventArgs.WheelDelta?
- Does the Blazor WASM views package (SkiaSharp.Views.Blazor) have its own touch handler separate from the MAUI handler?
- Is SKTouchAction.WheelChanged currently documented or expected to be raised on non-Windows platforms?

### Resolution Proposals

**Hypothesis:** Each platform handler needs to be updated to normalize its native scroll/wheel values to v120 units. The normalization formulas are already specified in the issue body.

1. **Implement per-platform wheel normalization** — fix, confidence 0.90 (90%), cost/l, validated=untested
   - Update each platform's SKTouchHandler to handle scroll events and normalize to v120. Priority order: Blazor WASM (#3534), Android (#3535), macOS (#3536), iOS (#3537), Tizen (#3538), WPF/WinForms (#3539), GTK (#3540). Add XML documentation to SKTouchEventArgs.WheelDelta documenting the v120 unit contract.

**Recommended proposal:** Implement per-platform wheel normalization

**Why:** The sub-issues are already filed and the normalization formulas are specified. Implementation can proceed incrementally per platform.

## Recommendations

### Actionability

| Field | Value |
|-------|-------|
| Suggested action | needs-investigation |
| Confidence | 0.88 (88%) |
| Reason | Well-specified enhancement with clear scope, sub-issues filed, and normalization formulas defined. Ready for implementation work to begin on each platform. |
| Suggested repro platform | linux |

### Automatable Actions

| Type | Risk | Confidence | Description | Details |
|------|------|------------|-------------|---------|
| update-labels | low | 0.90 (90%) | Apply enhancement, views-maui, and platform labels | labels=type/enhancement, area/SkiaSharp.Views.Maui, os/Android, os/iOS, os/macOS, os/Windows-WinUI, os/Windows-Classic, os/WASM, os/Tizen, os/Linux, tenet/compatibility, tenet/reliability |
| link-related | low | 0.95 (95%) | Link to sub-issue Blazor WASM #3534 | linkedIssue=#3534 |
| link-related | low | 0.95 (95%) | Link to sub-issue Android #3535 | linkedIssue=#3535 |

<details>
<summary>Raw JSON</summary>

```json
{
  "meta": {
    "schemaVersion": "1.0",
    "number": 3533,
    "repo": "mono/SkiaSharp",
    "analyzedAt": "2026-05-05T20:21:57Z",
    "currentLabels": [
      "type/enhancement",
      "area/SkiaSharp"
    ]
  },
  "summary": "Enhancement request to standardize SKTouchEventArgs.WheelDelta to the v120 convention (120 units = 1 discrete mouse wheel notch) across all platforms; currently only Windows MAUI passes meaningful values while all other platforms hardcode 0 or lack wheel event handling entirely.",
  "classification": {
    "type": {
      "value": "type/enhancement",
      "confidence": 0.92
    },
    "area": {
      "value": "area/SkiaSharp.Views.Maui",
      "confidence": 0.88
    },
    "platforms": [
      "os/Android",
      "os/iOS",
      "os/macOS",
      "os/Windows-Classic",
      "os/Windows-WinUI",
      "os/WASM",
      "os/Tizen",
      "os/Linux"
    ],
    "tenets": [
      "tenet/compatibility",
      "tenet/reliability"
    ]
  },
  "evidence": {
    "reproEvidence": {
      "environmentDetails": "SkiaSharp.Views.Maui.Core - multiple platforms; Windows MAUI is the only platform that currently passes WheelDelta values",
      "repoLinks": [
        {
          "url": "https://github.com/mono/SkiaSharp/issues/3534",
          "description": "Sub-issue: Blazor WASM wheel support"
        },
        {
          "url": "https://github.com/mono/SkiaSharp/issues/3535",
          "description": "Sub-issue: Android MAUI wheel support"
        },
        {
          "url": "https://github.com/mono/SkiaSharp/issues/3536",
          "description": "Sub-issue: macOS MAUI wheel support"
        },
        {
          "url": "https://github.com/mono/SkiaSharp/issues/3537",
          "description": "Sub-issue: iOS/iPadOS + Mac Catalyst wheel support"
        },
        {
          "url": "https://github.com/mono/SkiaSharp/issues/3538",
          "description": "Sub-issue: Tizen MAUI wheel support"
        },
        {
          "url": "https://github.com/mono/SkiaSharp/issues/3539",
          "description": "Sub-issue: WPF/WinForms wheel support"
        },
        {
          "url": "https://github.com/mono/SkiaSharp/issues/3540",
          "description": "Sub-issue: GTK3/4 (Linux) wheel support"
        }
      ]
    }
  },
  "analysis": {
    "summary": "The WheelDelta property on SKTouchEventArgs exists but has no documented unit contract and is only populated on Windows MAUI (via MouseWheelDelta passthrough). The enhancement proposes adopting the v120 convention (120 units per discrete notch, matching Windows and Linux libinput) and implementing platform-specific normalization for Android, macOS, iOS, Tizen, WASM/Blazor, WPF/WinForms, and GTK. Seven sub-issues (#3534–#3540) are already filed for each platform. This is a well-specified enhancement with clear scope and priority ordering.",
    "rationale": "The WheelDelta property already exists in the public API (SKTouchEventArgs.WheelDelta) and is populated in the Windows MAUI handler (SKTouchHandler.cs Platform/Windows). All other platform handlers (Android, Apple, Tizen) return 0 because they lack wheel event handling. This is an enhancement to add missing functionality and standardize behavior, not a regression. The v120 unit standard is well-established and the normalization formulas for each platform are clearly specified in the issue.",
    "keySignals": [
      {
        "text": "Only Windows MAUI passes meaningful values (raw ±120 per notch). All other platforms either hardcode 0 or don't handle wheel events at all.",
        "source": "issue body",
        "interpretation": "Confirms gap — API surface exists but cross-platform behavior is incomplete."
      },
      {
        "text": "We need to standardize WheelDelta across all platforms to use the v120 convention: 120 units = 1 discrete mouse wheel notch",
        "source": "issue body",
        "interpretation": "Clear, specific enhancement request with a defined standard."
      },
      {
        "text": "Seven sub-issues #3534–#3540 already filed per platform",
        "source": "issue body",
        "interpretation": "Work is well-decomposed; this is a tracking/umbrella issue."
      }
    ],
    "codeInvestigation": [
      {
        "file": "source/SkiaSharp.Views.Maui/SkiaSharp.Views.Maui.Core/SKTouchEventArgs.cs",
        "lines": "50",
        "finding": "WheelDelta is a public int property on SKTouchEventArgs with no documentation or unit contract. Default value is 0 when not specified.",
        "relevance": "direct"
      },
      {
        "file": "source/SkiaSharp.Views.Maui/SkiaSharp.Views.Maui.Core/Platform/Windows/SKTouchHandler.cs",
        "lines": "101-124",
        "finding": "OnPointerWheelChanged handler reads MouseWheelDelta from PointerPoint.Properties and passes it directly as wheelDelta. Windows already complies with v120 by passthrough since MouseWheelDelta uses WHEEL_DELTA=120 convention natively.",
        "relevance": "direct"
      },
      {
        "file": "source/SkiaSharp.Views.Maui/SkiaSharp.Views.Maui.Core/Platform/Android/SKTouchHandler.cs",
        "finding": "No WheelChanged handling found — no references to WheelDelta, scroll, or wheel events. SKTouchAction.WheelChanged is never raised on Android.",
        "relevance": "direct"
      },
      {
        "file": "source/SkiaSharp.Views.Maui/SkiaSharp.Views.Maui.Core/Platform/Apple/SKTouchHandler.cs",
        "finding": "No WheelChanged handling found — no scroll event handling for macOS/iOS. SKTouchAction.WheelChanged is never raised on Apple platforms.",
        "relevance": "direct"
      },
      {
        "file": "source/SkiaSharp.Views.Maui/SkiaSharp.Views.Maui.Core/Platform/Tizen/SKTouchHandler.cs",
        "finding": "No WheelChanged handling found — no wheel/scroll event handling on Tizen.",
        "relevance": "direct"
      }
    ],
    "nextQuestions": [
      "Should the v120 standard be formally documented in XML docs on SKTouchEventArgs.WheelDelta?",
      "Does the Blazor WASM views package (SkiaSharp.Views.Blazor) have its own touch handler separate from the MAUI handler?",
      "Is SKTouchAction.WheelChanged currently documented or expected to be raised on non-Windows platforms?"
    ],
    "resolution": {
      "hypothesis": "Each platform handler needs to be updated to normalize its native scroll/wheel values to v120 units. The normalization formulas are already specified in the issue body.",
      "proposals": [
        {
          "title": "Implement per-platform wheel normalization",
          "description": "Update each platform's SKTouchHandler to handle scroll events and normalize to v120. Priority order: Blazor WASM (#3534), Android (#3535), macOS (#3536), iOS (#3537), Tizen (#3538), WPF/WinForms (#3539), GTK (#3540). Add XML documentation to SKTouchEventArgs.WheelDelta documenting the v120 unit contract.",
          "category": "fix",
          "confidence": 0.9,
          "effort": "cost/l",
          "validated": "untested"
        }
      ],
      "recommendedProposal": "Implement per-platform wheel normalization",
      "recommendedReason": "The sub-issues are already filed and the normalization formulas are specified. Implementation can proceed incrementally per platform."
    }
  },
  "output": {
    "actionability": {
      "suggestedAction": "needs-investigation",
      "confidence": 0.88,
      "reason": "Well-specified enhancement with clear scope, sub-issues filed, and normalization formulas defined. Ready for implementation work to begin on each platform.",
      "suggestedReproPlatform": "linux"
    },
    "actions": [
      {
        "type": "update-labels",
        "description": "Apply enhancement, views-maui, and platform labels",
        "risk": "low",
        "confidence": 0.9,
        "labels": [
          "type/enhancement",
          "area/SkiaSharp.Views.Maui",
          "os/Android",
          "os/iOS",
          "os/macOS",
          "os/Windows-WinUI",
          "os/Windows-Classic",
          "os/WASM",
          "os/Tizen",
          "os/Linux",
          "tenet/compatibility",
          "tenet/reliability"
        ]
      },
      {
        "type": "link-related",
        "description": "Link to sub-issue Blazor WASM #3534",
        "risk": "low",
        "confidence": 0.95,
        "linkedIssue": 3534
      },
      {
        "type": "link-related",
        "description": "Link to sub-issue Android #3535",
        "risk": "low",
        "confidence": 0.95,
        "linkedIssue": 3535
      }
    ]
  }
}
```

</details>
