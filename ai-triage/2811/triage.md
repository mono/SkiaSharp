# Issue Triage Report — #2811

| Field | Value |
|-------|-------|
| Repository | mono/SkiaSharp |
| Analyzed | 2026-04-30T15:24:14Z |
| Type | type/bug (0.92 (92%)) |
| Area | area/SkiaSharp.Views.Maui (0.90 (90%)) |
| Suggested action | needs-investigation (0.82 (82%)) |

**Issue Summary:** On .NET MAUI (.NET 9.0 Preview 2) with SkiaSharp 3.0 Preview 2.1, a custom control derived from SKCanvasView that overrides MeasureOverride returns the correct size from that method, but OnSizeAllocated is called with a different (often much smaller or zero) height on Android and iOS.

**Analysis:** SKCanvasView in MAUI does not participate correctly in the MAUI measure/layout pass when subclassed with MeasureOverride. The MAUI Controls SKCanvasView (source/SkiaSharp.Views.Maui/SkiaSharp.Views.Maui.Controls/SKCanvasView.cs) inherits from Microsoft.Maui.Controls.View without overriding any measure or layout methods, and the handler (SKCanvasViewHandler) does not implement MeasureNativeView or any layout size negotiation. As a result, the platform handler's native view size may not reflect the virtual view's desired size, causing OnSizeAllocated to receive wrong dimensions.

**Recommendations:** **needs-investigation** — Real regression with screenshots and related issues confirming the root pattern. Workaround exists but root cause in handler layout sizing needs proper investigation.

---

## Classification

| Field | Value |
|-------|-------|
| Type | type/bug |
| Area | area/SkiaSharp.Views.Maui |
| Platforms | os/Android, os/iOS |
| Backends | — |
| Tenets | — |
| Partner | partner/maui |
| Current labels | type/bug |

## Evidence

### Reproduction

1. Create a custom control inheriting from SKCanvasView in a MAUI application
2. Override MeasureOverride to return a computed Size based on content
3. Use the control in a MAUI layout (e.g. StackLayout, Grid)
4. Observe that OnSizeAllocated receives a smaller height than returned from MeasureOverride, sometimes 0 on iOS

**Environment:** SkiaSharp 3.0 Preview 2.1, .NET 9.0 Preview 2, Android 14 (Pixel 6a), iOS 17.3.1 (iPad 11,7), Visual Studio on Windows

**Repository links:**
- https://github.com/hyvanmielenpelit/GnollHack — Reporter's project demonstrating the issue (CustomLabel in GnollHackM vs GnollHackX)
- https://github.com/dotnet/maui/issues/4019#issuecomment-1510654509 — MAUI upstream issue with workaround for custom view sizing
- https://github.com/mono/SkiaSharp/issues/2506 — Related issue: MeasureOverride on derived SKCanvasView does not set DesiredSize.Height
- https://github.com/mono/SkiaSharp/issues/3239 — Related issue: InvalidateMeasure doesn't call MeasureOverride after first call

**Attachments:**
- Screenshot_20240327-234854.png — https://github.com/mono/SkiaSharp/assets/53081197/d8011baa-c326-472d-bdb9-b09b477854b9
- Screenshot_20240327-234739.png — https://github.com/mono/SkiaSharp/assets/53081197/4a612cfb-ba86-40d2-86d7-bde23fb6e43d

### Bug Signals

| Field | Value |
|-------|-------|
| Severity | medium |
| Regression claimed | True |
| Error type | wrong-output |
| Error message | — |
| Repro quality | partial |
| Target frameworks | net9.0-android, net9.0-ios |

### Version Analysis

| Field | Value |
|-------|-------|
| Mentioned versions | 3.0 Preview 2.1, 2.80.x |
| Worked in | 2.80.x |
| Broke in | 3.0 Preview 2.1 |
| Current relevance | likely |
| Relevance reason | SKCanvasView.cs in MAUI Controls does not override MeasureOverride and the handler does not provide any layout size negotiation, so this gap likely persists in current code. |

### Regression

| Field | Value |
|-------|-------|
| Is regression | True |
| Confidence | 0.80 (80%) |
| Reason | Reporter explicitly states this works correctly in Xamarin.Forms (2.80.x) and breaks in the MAUI (3.0 Preview 2.1) migration. The MAUI handler architecture is entirely different from Xamarin renderers. |
| Worked in version | 2.80.x |
| Broke in version | 3.0 Preview 2.1 |

## Analysis

### Technical Summary

SKCanvasView in MAUI does not participate correctly in the MAUI measure/layout pass when subclassed with MeasureOverride. The MAUI Controls SKCanvasView (source/SkiaSharp.Views.Maui/SkiaSharp.Views.Maui.Controls/SKCanvasView.cs) inherits from Microsoft.Maui.Controls.View without overriding any measure or layout methods, and the handler (SKCanvasViewHandler) does not implement MeasureNativeView or any layout size negotiation. As a result, the platform handler's native view size may not reflect the virtual view's desired size, causing OnSizeAllocated to receive wrong dimensions.

### Rationale

The issue clearly describes broken layout behavior (wrong dimensions delivered to OnSizeAllocated) that regressed from Xamarin to MAUI. Screenshots confirm visual truncation. Related issues #2506 and #3239 describe the same root class of problem: SKCanvasView derived views not having their MeasureOverride results honored by the MAUI layout system. The reporter links a MAUI upstream issue and workaround, suggesting this is partly a MAUI/SkiaSharp integration gap.

### Key Signals

- "MeasureOverride calculates the right value, but OnSizeAllocated gets called with the wrong height (and that's how the label in fact looks in practice)." — **issue body** (MAUI layout pipeline is not propagating the desired size from MeasureOverride to the allocation phase.)
- "On iOS, the height is even sometimes 0." — **issue body** (iOS appears to have a more severe manifestation, possibly because UIKit native sizing collapses to intrinsic zero if no constraint is provided.)
- "In Xamarin, everything works exactly as intended." — **issue body** (Confirms regression: Xamarin renderer pattern handled custom sizing differently from MAUI handler pattern.)
- "There's a workaround to this in the following Maui issue: https://github.com/dotnet/maui/issues/4019#issuecomment-1510654509" — **comment by janne-hmp** (A MAUI-level workaround exists, suggesting the fix involves the interaction between SKCanvasView and MAUI's measure/layout system.)

### Code Investigation

| File | Lines | Relevance | Finding |
|------|-------|-----------|---------|
| `source/SkiaSharp.Views.Maui/SkiaSharp.Views.Maui.Controls/SKCanvasView.cs` | — | direct | SKCanvasView inherits from Microsoft.Maui.Controls.View and does not override MeasureOverride, ArrangeOverride, or any measure/layout related methods. No size negotiation is done at the virtual view level. |
| `source/SkiaSharp.Views.Maui/SkiaSharp.Views.Maui.Core/Handlers/SKCanvasView/SKCanvasViewHandler.Android.cs` | — | direct | Android handler does not override GetDesiredSize, MeasureNativeView, or any method that would relay MeasureOverride results to the native Android view sizing system. |
| `source/SkiaSharp.Views.Maui/SkiaSharp.Views.Maui.Core/Handlers/SKCanvasView/SKCanvasViewHandler.Apple.cs` | — | direct | iOS/Apple handler similarly does not override any measure/layout methods. The platform view is a bare SKCanvasView (UIView subclass) with no intrinsic content size, which would default to zero on iOS unless constrained by Auto Layout. |
| `source/SkiaSharp.Views.Maui/SkiaSharp.Views.Maui.Core/Handlers/SKCanvasView/SKCanvasViewHandler.cs` | — | related | Handler mapper only maps EnableTouchEvents and IgnorePixelScaling. No size-related mappings or commands exist. |

### Workarounds

- Set HeightRequest (and WidthRequest if needed) explicitly in MeasureOverride after computing the desired size, as demonstrated by the reporter in the linked MAUI issue comment (https://github.com/dotnet/maui/issues/4019#issuecomment-1510654509) and used in related issue #3239.
- Override OnSizeAllocated to force an InvalidateSurface call when size is correct to trigger a repaint at the right dimensions.

### Next Questions

- Does this reproduce on current stable SkiaSharp (3.116.x) or is it specific to 3.0 Preview builds?
- Does SKGLView have the same MeasureOverride sizing issue?
- Is this reproducible on Windows Classic (WinUI) with the same derived-view pattern?

### Resolution Proposals

**Hypothesis:** The MAUI handler for SKCanvasView does not bridge the virtual view's MeasureOverride result back to the native platform sizing system. On iOS, the native SKCanvasView (UIView) has no intrinsic content size, so AutoLayout collapses it to zero. On Android, the MAUI measure pass doesn't call the virtual view's MeasureOverride when sizing the native Android view.

1. **Set HeightRequest/WidthRequest in MeasureOverride** — workaround, confidence 0.80 (80%), cost/xs, validated=untested
   - As a workaround, in the derived class's MeasureOverride, explicitly set this.HeightRequest and this.WidthRequest to the computed dimensions in addition to returning the Size. This forces MAUI to use those values during layout.
2. **Investigate MAUI handler MeasureNativeView override** — investigation, confidence 0.70 (70%), cost/m, validated=untested
   - Investigate whether SKCanvasViewHandler should override GetDesiredSize (MAUI handler method) to call the virtual view's MeasureOverride and return the result, so the MAUI layout system gets correct sizing information.

**Recommended proposal:** Set HeightRequest/WidthRequest in MeasureOverride

**Why:** Immediately actionable workaround while the root cause in the handler is investigated.

## Recommendations

### Actionability

| Field | Value |
|-------|-------|
| Suggested action | needs-investigation |
| Confidence | 0.82 (82%) |
| Reason | Real regression with screenshots and related issues confirming the root pattern. Workaround exists but root cause in handler layout sizing needs proper investigation. |
| Suggested repro platform | macos |

### Automatable Actions

| Type | Risk | Confidence | Description | Details |
|------|------|------------|-------------|---------|
| update-labels | low | 0.92 (92%) | Apply bug, MAUI views, Android, iOS, partner/maui labels | labels=type/bug, area/SkiaSharp.Views.Maui, os/Android, os/iOS, partner/maui |
| link-related | low | 0.90 (90%) | Cross-reference with related MeasureOverride sizing issue #2506 | linkedIssue=#2506 |
| link-related | low | 0.85 (85%) | Cross-reference with related InvalidateMeasure issue #3239 | linkedIssue=#3239 |
| add-comment | medium | 0.82 (82%) | Post response with workaround and analysis | — |

**Comment draft for `add-comment`:**

```markdown
Thanks for the detailed report and screenshots!

This appears to be a known gap in how `SKCanvasView` integrates with MAUI's measure/layout pipeline. When subclassing `SKCanvasView` and overriding `MeasureOverride`, the size you return isn't automatically propagated back to the MAUI layout system's allocation step — this differs from Xamarin.Forms behavior where the renderer pattern handled custom sizing differently.

**Workaround (while we investigate a proper fix):**

In your `MeasureOverride`, in addition to returning the computed `Size`, explicitly set `HeightRequest` (and `WidthRequest` if needed):

```csharp
protected override Size MeasureOverride(double widthConstraint, double heightConstraint)
{
    var size = ComputeMyDesiredSize(widthConstraint, heightConstraint);
    HeightRequest = size.Height;  // <-- add this
    WidthRequest = size.Width;    // <-- add this if needed
    return size;
}
```

This approach is also documented in [dotnet/maui#4019](https://github.com/dotnet/maui/issues/4019#issuecomment-1510654509) and used as a workaround in related issue #3239.

Related issues: #2506, #3239.
```

<details>
<summary>Raw JSON</summary>

```json
{
  "meta": {
    "schemaVersion": "1.0",
    "number": 2811,
    "repo": "mono/SkiaSharp",
    "analyzedAt": "2026-04-30T15:24:14Z",
    "currentLabels": [
      "type/bug"
    ]
  },
  "summary": "On .NET MAUI (.NET 9.0 Preview 2) with SkiaSharp 3.0 Preview 2.1, a custom control derived from SKCanvasView that overrides MeasureOverride returns the correct size from that method, but OnSizeAllocated is called with a different (often much smaller or zero) height on Android and iOS.",
  "classification": {
    "type": {
      "value": "type/bug",
      "confidence": 0.92
    },
    "area": {
      "value": "area/SkiaSharp.Views.Maui",
      "confidence": 0.9
    },
    "platforms": [
      "os/Android",
      "os/iOS"
    ],
    "partner": "partner/maui"
  },
  "evidence": {
    "bugSignals": {
      "severity": "medium",
      "regressionClaimed": true,
      "errorType": "wrong-output",
      "reproQuality": "partial",
      "targetFrameworks": [
        "net9.0-android",
        "net9.0-ios"
      ]
    },
    "reproEvidence": {
      "stepsToReproduce": [
        "Create a custom control inheriting from SKCanvasView in a MAUI application",
        "Override MeasureOverride to return a computed Size based on content",
        "Use the control in a MAUI layout (e.g. StackLayout, Grid)",
        "Observe that OnSizeAllocated receives a smaller height than returned from MeasureOverride, sometimes 0 on iOS"
      ],
      "environmentDetails": "SkiaSharp 3.0 Preview 2.1, .NET 9.0 Preview 2, Android 14 (Pixel 6a), iOS 17.3.1 (iPad 11,7), Visual Studio on Windows",
      "attachments": [
        {
          "url": "https://github.com/mono/SkiaSharp/assets/53081197/d8011baa-c326-472d-bdb9-b09b477854b9",
          "filename": "Screenshot_20240327-234854.png"
        },
        {
          "url": "https://github.com/mono/SkiaSharp/assets/53081197/4a612cfb-ba86-40d2-86d7-bde23fb6e43d",
          "filename": "Screenshot_20240327-234739.png"
        }
      ],
      "repoLinks": [
        {
          "url": "https://github.com/hyvanmielenpelit/GnollHack",
          "description": "Reporter's project demonstrating the issue (CustomLabel in GnollHackM vs GnollHackX)"
        },
        {
          "url": "https://github.com/dotnet/maui/issues/4019#issuecomment-1510654509",
          "description": "MAUI upstream issue with workaround for custom view sizing"
        },
        {
          "url": "https://github.com/mono/SkiaSharp/issues/2506",
          "description": "Related issue: MeasureOverride on derived SKCanvasView does not set DesiredSize.Height"
        },
        {
          "url": "https://github.com/mono/SkiaSharp/issues/3239",
          "description": "Related issue: InvalidateMeasure doesn't call MeasureOverride after first call"
        }
      ]
    },
    "versionAnalysis": {
      "mentionedVersions": [
        "3.0 Preview 2.1",
        "2.80.x"
      ],
      "workedIn": "2.80.x",
      "brokeIn": "3.0 Preview 2.1",
      "currentRelevance": "likely",
      "relevanceReason": "SKCanvasView.cs in MAUI Controls does not override MeasureOverride and the handler does not provide any layout size negotiation, so this gap likely persists in current code."
    },
    "regression": {
      "isRegression": true,
      "confidence": 0.8,
      "reason": "Reporter explicitly states this works correctly in Xamarin.Forms (2.80.x) and breaks in the MAUI (3.0 Preview 2.1) migration. The MAUI handler architecture is entirely different from Xamarin renderers.",
      "workedInVersion": "2.80.x",
      "brokeInVersion": "3.0 Preview 2.1"
    }
  },
  "analysis": {
    "summary": "SKCanvasView in MAUI does not participate correctly in the MAUI measure/layout pass when subclassed with MeasureOverride. The MAUI Controls SKCanvasView (source/SkiaSharp.Views.Maui/SkiaSharp.Views.Maui.Controls/SKCanvasView.cs) inherits from Microsoft.Maui.Controls.View without overriding any measure or layout methods, and the handler (SKCanvasViewHandler) does not implement MeasureNativeView or any layout size negotiation. As a result, the platform handler's native view size may not reflect the virtual view's desired size, causing OnSizeAllocated to receive wrong dimensions.",
    "rationale": "The issue clearly describes broken layout behavior (wrong dimensions delivered to OnSizeAllocated) that regressed from Xamarin to MAUI. Screenshots confirm visual truncation. Related issues #2506 and #3239 describe the same root class of problem: SKCanvasView derived views not having their MeasureOverride results honored by the MAUI layout system. The reporter links a MAUI upstream issue and workaround, suggesting this is partly a MAUI/SkiaSharp integration gap.",
    "keySignals": [
      {
        "text": "MeasureOverride calculates the right value, but OnSizeAllocated gets called with the wrong height (and that's how the label in fact looks in practice).",
        "source": "issue body",
        "interpretation": "MAUI layout pipeline is not propagating the desired size from MeasureOverride to the allocation phase."
      },
      {
        "text": "On iOS, the height is even sometimes 0.",
        "source": "issue body",
        "interpretation": "iOS appears to have a more severe manifestation, possibly because UIKit native sizing collapses to intrinsic zero if no constraint is provided."
      },
      {
        "text": "In Xamarin, everything works exactly as intended.",
        "source": "issue body",
        "interpretation": "Confirms regression: Xamarin renderer pattern handled custom sizing differently from MAUI handler pattern."
      },
      {
        "text": "There's a workaround to this in the following Maui issue: https://github.com/dotnet/maui/issues/4019#issuecomment-1510654509",
        "source": "comment by janne-hmp",
        "interpretation": "A MAUI-level workaround exists, suggesting the fix involves the interaction between SKCanvasView and MAUI's measure/layout system."
      }
    ],
    "codeInvestigation": [
      {
        "file": "source/SkiaSharp.Views.Maui/SkiaSharp.Views.Maui.Controls/SKCanvasView.cs",
        "finding": "SKCanvasView inherits from Microsoft.Maui.Controls.View and does not override MeasureOverride, ArrangeOverride, or any measure/layout related methods. No size negotiation is done at the virtual view level.",
        "relevance": "direct"
      },
      {
        "file": "source/SkiaSharp.Views.Maui/SkiaSharp.Views.Maui.Core/Handlers/SKCanvasView/SKCanvasViewHandler.Android.cs",
        "finding": "Android handler does not override GetDesiredSize, MeasureNativeView, or any method that would relay MeasureOverride results to the native Android view sizing system.",
        "relevance": "direct"
      },
      {
        "file": "source/SkiaSharp.Views.Maui/SkiaSharp.Views.Maui.Core/Handlers/SKCanvasView/SKCanvasViewHandler.Apple.cs",
        "finding": "iOS/Apple handler similarly does not override any measure/layout methods. The platform view is a bare SKCanvasView (UIView subclass) with no intrinsic content size, which would default to zero on iOS unless constrained by Auto Layout.",
        "relevance": "direct"
      },
      {
        "file": "source/SkiaSharp.Views.Maui/SkiaSharp.Views.Maui.Core/Handlers/SKCanvasView/SKCanvasViewHandler.cs",
        "finding": "Handler mapper only maps EnableTouchEvents and IgnorePixelScaling. No size-related mappings or commands exist.",
        "relevance": "related"
      }
    ],
    "workarounds": [
      "Set HeightRequest (and WidthRequest if needed) explicitly in MeasureOverride after computing the desired size, as demonstrated by the reporter in the linked MAUI issue comment (https://github.com/dotnet/maui/issues/4019#issuecomment-1510654509) and used in related issue #3239.",
      "Override OnSizeAllocated to force an InvalidateSurface call when size is correct to trigger a repaint at the right dimensions."
    ],
    "nextQuestions": [
      "Does this reproduce on current stable SkiaSharp (3.116.x) or is it specific to 3.0 Preview builds?",
      "Does SKGLView have the same MeasureOverride sizing issue?",
      "Is this reproducible on Windows Classic (WinUI) with the same derived-view pattern?"
    ],
    "resolution": {
      "hypothesis": "The MAUI handler for SKCanvasView does not bridge the virtual view's MeasureOverride result back to the native platform sizing system. On iOS, the native SKCanvasView (UIView) has no intrinsic content size, so AutoLayout collapses it to zero. On Android, the MAUI measure pass doesn't call the virtual view's MeasureOverride when sizing the native Android view.",
      "proposals": [
        {
          "title": "Set HeightRequest/WidthRequest in MeasureOverride",
          "description": "As a workaround, in the derived class's MeasureOverride, explicitly set this.HeightRequest and this.WidthRequest to the computed dimensions in addition to returning the Size. This forces MAUI to use those values during layout.",
          "category": "workaround",
          "confidence": 0.8,
          "effort": "cost/xs",
          "validated": "untested"
        },
        {
          "title": "Investigate MAUI handler MeasureNativeView override",
          "description": "Investigate whether SKCanvasViewHandler should override GetDesiredSize (MAUI handler method) to call the virtual view's MeasureOverride and return the result, so the MAUI layout system gets correct sizing information.",
          "category": "investigation",
          "confidence": 0.7,
          "effort": "cost/m",
          "validated": "untested"
        }
      ],
      "recommendedProposal": "Set HeightRequest/WidthRequest in MeasureOverride",
      "recommendedReason": "Immediately actionable workaround while the root cause in the handler is investigated."
    }
  },
  "output": {
    "actionability": {
      "suggestedAction": "needs-investigation",
      "confidence": 0.82,
      "reason": "Real regression with screenshots and related issues confirming the root pattern. Workaround exists but root cause in handler layout sizing needs proper investigation.",
      "suggestedReproPlatform": "macos"
    },
    "actions": [
      {
        "type": "update-labels",
        "description": "Apply bug, MAUI views, Android, iOS, partner/maui labels",
        "risk": "low",
        "confidence": 0.92,
        "labels": [
          "type/bug",
          "area/SkiaSharp.Views.Maui",
          "os/Android",
          "os/iOS",
          "partner/maui"
        ]
      },
      {
        "type": "link-related",
        "description": "Cross-reference with related MeasureOverride sizing issue #2506",
        "risk": "low",
        "confidence": 0.9,
        "linkedIssue": 2506
      },
      {
        "type": "link-related",
        "description": "Cross-reference with related InvalidateMeasure issue #3239",
        "risk": "low",
        "confidence": 0.85,
        "linkedIssue": 3239
      },
      {
        "type": "add-comment",
        "description": "Post response with workaround and analysis",
        "risk": "medium",
        "confidence": 0.82,
        "comment": "Thanks for the detailed report and screenshots!\n\nThis appears to be a known gap in how `SKCanvasView` integrates with MAUI's measure/layout pipeline. When subclassing `SKCanvasView` and overriding `MeasureOverride`, the size you return isn't automatically propagated back to the MAUI layout system's allocation step — this differs from Xamarin.Forms behavior where the renderer pattern handled custom sizing differently.\n\n**Workaround (while we investigate a proper fix):**\n\nIn your `MeasureOverride`, in addition to returning the computed `Size`, explicitly set `HeightRequest` (and `WidthRequest` if needed):\n\n```csharp\nprotected override Size MeasureOverride(double widthConstraint, double heightConstraint)\n{\n    var size = ComputeMyDesiredSize(widthConstraint, heightConstraint);\n    HeightRequest = size.Height;  // <-- add this\n    WidthRequest = size.Width;    // <-- add this if needed\n    return size;\n}\n```\n\nThis approach is also documented in [dotnet/maui#4019](https://github.com/dotnet/maui/issues/4019#issuecomment-1510654509) and used as a workaround in related issue #3239.\n\nRelated issues: #2506, #3239."
      }
    ]
  }
}
```

</details>
