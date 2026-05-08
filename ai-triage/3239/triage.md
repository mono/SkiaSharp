# Issue Triage Report — #3239

| Field | Value |
|-------|-------|
| Repository | mono/SkiaSharp |
| Analyzed | 2026-04-23T03:18:30Z |
| Type | type/bug (0.95 (95%)) |
| Area | area/SkiaSharp.Views.Maui (0.92 (92%)) |
| Suggested action | needs-investigation (0.85 (85%)) |

**Issue Summary:** In SkiaSharp.Views.Maui.Controls 3.x, calling InvalidateMeasure() on a custom SKCanvasView-derived control fails to trigger MeasureOverride after the first layout pass, preventing dynamic resizing based on content; workaround is to explicitly set HeightRequest/WidthRequest.

**Analysis:** SKCanvasViewHandler does not override GetDesiredSize to delegate to the MAUI virtual view's MeasureOverride. After the first measure pass, MAUI's layout engine caches the platform view's intrinsic size. Subsequent InvalidateMeasure() calls trigger the MAUI IView.InvalidateMeasure() chain (which calls the base ViewHandler's MapInvalidateMeasure) but since GetDesiredSize returns the native view's size rather than invoking MeasureOverride, the layout does not re-measure the virtual view. Setting HeightRequest/WidthRequest bypasses MAUI's measurement caching, which is why the workaround works.

**Recommendations:** **needs-investigation** — Confirmed regression with a minimal repro app, clear call stack, and consistent failure mode. Root cause is in SKCanvasViewHandler's measurement delegation to MAUI. Needs deeper investigation into MAUI layout engine interaction and a fix in the handler layer.

---

## Classification

| Field | Value |
|-------|-------|
| Type | type/bug |
| Area | area/SkiaSharp.Views.Maui |
| Platforms | os/Windows-Classic |
| Backends | — |
| Tenets | tenet/reliability, tenet/compatibility |
| Partner | partner/maui |
| Current labels | type/bug |

## Evidence

### Reproduction

1. Create a custom control inheriting from SKCanvasView that overrides MeasureOverride to compute size based on content
2. Place the control in a nested layout (HorizontalStackLayout or Grid)
3. Change a property that should change the desired size and call InvalidateMeasure()
4. Observe that MeasureOverride is NOT called again and the view does not resize

**Environment:** SkiaSharp.Views.Maui.Controls 3.116.1, Visual Studio on Windows 11

**Repository links:**
- https://github.com/DanTravison/GlyphViewer/blob/Preferences/Controls/SkLabel.cs — Reporter's custom SKLabel control demonstrating the issue with workaround comments
- https://github.com/DanTravison/GlyphViewer/blob/Preferences/Views/GlyphView.cs — Reporter's GlyphView control with simpler repro case
- https://github.com/DanTravison/SKInvalidateMeasure — Minimal sample app demonstrating the issue — three SkLabel instances in different layouts
- https://github.com/mono/SkiaSharp/issues/2811 — Related issue #2811: MeasureOverride result not respected in MAUI layout, same class of problem
- https://github.com/dotnet/maui/issues/4019#issuecomment-1510654509 — MAUI upstream issue with workaround referenced by related issue #2811

### Bug Signals

| Field | Value |
|-------|-------|
| Severity | medium |
| Regression claimed | True |
| Error type | wrong-output |
| Error message | After the first MeasureOverride occurs, calling InvalidateMeasure no longer causes MeasureOverride to occur |
| Repro quality | complete |
| Target frameworks | net9.0-windows10.0.19041 |

### Version Analysis

| Field | Value |
|-------|-------|
| Mentioned versions | 3.116.0, 3.116.1, 2.88.9 |
| Worked in | 2.88.9 |
| Broke in | 3.116.0 |
| Current relevance | likely |
| Relevance reason | No fix or related PR found for this issue; SKCanvasViewHandler command mapper still does not override InvalidateMeasure handling |

### Regression

| Field | Value |
|-------|-------|
| Is regression | True |
| Confidence | 0.90 (90%) |
| Reason | Reporter explicitly states the behavior worked in SkiaSharp 2.88.9 (Xamarin.Forms era) and broke in 3.x MAUI. The MAUI view handler architecture differs fundamentally from Xamarin.Forms renderers. |
| Worked in version | 2.88.9 |
| Broke in version | 3.116.0 |

## Analysis

### Technical Summary

SKCanvasViewHandler does not override GetDesiredSize to delegate to the MAUI virtual view's MeasureOverride. After the first measure pass, MAUI's layout engine caches the platform view's intrinsic size. Subsequent InvalidateMeasure() calls trigger the MAUI IView.InvalidateMeasure() chain (which calls the base ViewHandler's MapInvalidateMeasure) but since GetDesiredSize returns the native view's size rather than invoking MeasureOverride, the layout does not re-measure the virtual view. Setting HeightRequest/WidthRequest bypasses MAUI's measurement caching, which is why the workaround works.

### Rationale

The issue is clearly a regression introduced by the MAUI migration. The Xamarin.Forms renderer used OnMeasure which was called repeatedly; the MAUI handler-based architecture uses GetDesiredSize which is only delegated if the handler overrides it. SKCanvasViewHandler lacks this override, causing silent measurement caching. This is a SkiaSharp responsibility to fix in the handler layer, not purely a MAUI bug.

### Key Signals

- "After the first MeasureOverride occurs, calling InvalidateMeasure no longer causes MeasureOverride to occur." — **issue body** (Confirms measurement caching: MAUI caches size after first measure and does not re-invoke MeasureOverride unless the handler provides a fresh desired size.)
- "explicitly setting HeightRequest in these two methods is required to workaround the issue" — **issue body** (Setting HeightRequest bypasses MAUI measurement caching by providing an explicit size constraint, confirming the root cause is in the measurement pipeline rather than painting.)
- "The two nested SkLabel instances don't resize unless the 'Try Workaround' checkbox is checked." — **comment by DanTravison** (Root layout SkLabel works (MAUI may handle root layout measure differently) but nested layouts fail — consistent with measurement caching only affecting constraint-propagation in nested layouts.)
- "overriding SKCanvasViewHandler.GetDesiredSize does not fix the issue for me. I must still set HeightRequest" — **comment by DanTravison (2025-06-18)** (Overriding GetDesiredSize alone is insufficient — the fix must also ensure MAUI re-runs layout after property changes, suggesting the IView.InvalidateMeasure contract needs additional implementation in the handler or the virtual view.)

### Code Investigation

| File | Lines | Relevance | Finding |
|------|-------|-----------|---------|
| `source/SkiaSharp.Views.Maui/SkiaSharp.Views.Maui.Core/Handlers/SKCanvasView/SKCanvasViewHandler.cs` | 1-31 | direct | SKCanvasViewCommandMapper only maps InvalidateSurface — there is no GetDesiredSize override and no special handling for the InvalidateMeasure command beyond what the base ViewHandler.ViewCommandMapper provides |
| `source/SkiaSharp.Views.Maui/SkiaSharp.Views.Maui.Controls/SKCanvasView.cs` | 1-67 | direct | SKCanvasView (MAUI Controls) does not override MeasureOverride itself — that is left to subclasses. No mechanism exists to notify the handler that the desired size has changed. |
| `source/SkiaSharp.Views.Maui/SkiaSharp.Views.Maui.Core/Handlers/SKCanvasView/SKCanvasViewHandler.Windows.cs` | 1-89 | direct | Windows handler creates an SKXamlCanvas but does not override GetDesiredSize. When MAUI asks for the view's desired size it falls through to the base handler which measures the native XAML control's intrinsic size, ignoring the virtual view's MeasureOverride result. |
| `source/SkiaSharp.Views.Maui/SkiaSharp.Views.Maui.Core/Handlers/SKCanvasView/SKCanvasViewHandler.Android.cs` | 1-79 | related | Android handler also does not override GetDesiredSize — same root cause applies cross-platform even though reporter only tested on Windows. |

### Workarounds

- Set HeightRequest (and/or WidthRequest) to the computed desired height in MeasureOverride and in the property-changed handler that calls InvalidateMeasure — this forces MAUI to respect the explicit size and re-layout.
- Override SKCanvasViewHandler.GetDesiredSize in a custom handler and register it via MauiAppBuilder.ConfigureMauiHandlers — note that reporter found this alone did not fully resolve the issue without also setting HeightRequest.

### Next Questions

- Does the bug reproduce on Android/iOS, or is it Windows-specific?
- Which MAUI version is in use? The layout caching behavior may differ between MAUI versions.
- Is SKGLView affected by the same issue?

### Resolution Proposals

**Hypothesis:** SKCanvasViewHandler needs to override GetDesiredSize to call the virtual view's MeasureOverride (via VirtualView.Measure / VirtualView.DesiredSize) and ensure that the MAUI IView.InvalidateMeasure command properly propagates to the platform native view's layout invalidation so subsequent measures are triggered.

1. **Override GetDesiredSize in SKCanvasViewHandler to delegate to MeasureOverride** — fix, confidence 0.72 (72%), cost/s, validated=untested
   - Add a GetDesiredSize override in each platform-specific SKCanvasViewHandler that calls VirtualView.Measure(widthConstraint, heightConstraint) and returns the resulting DesiredSize. This ensures MAUI calls MeasureOverride on the virtual view rather than using the native view's intrinsic size.
2. **Set HeightRequest/WidthRequest in the property-changed handler** — workaround, confidence 0.90 (90%), cost/xs, validated=untested
   - In the custom SKCanvasView subclass, after computing the desired size, explicitly set HeightRequest and WidthRequest to the computed values. This bypasses MAUI measurement caching entirely.

**Recommended proposal:** Set HeightRequest/WidthRequest in the property-changed handler

**Why:** Confirmed by reporter to work reliably, no SkiaSharp source changes needed, immediately applicable.

## Recommendations

### Actionability

| Field | Value |
|-------|-------|
| Suggested action | needs-investigation |
| Confidence | 0.85 (85%) |
| Reason | Confirmed regression with a minimal repro app, clear call stack, and consistent failure mode. Root cause is in SKCanvasViewHandler's measurement delegation to MAUI. Needs deeper investigation into MAUI layout engine interaction and a fix in the handler layer. |
| Suggested repro platform | windows |

### Automatable Actions

| Type | Risk | Confidence | Description | Details |
|------|------|------------|-------------|---------|
| update-labels | low | 0.92 (92%) | Apply bug, MAUI views area, Windows platform, reliability/compatibility tenets, and MAUI partner labels | labels=type/bug, area/SkiaSharp.Views.Maui, os/Windows-Classic, tenet/reliability, tenet/compatibility, partner/maui |
| link-related | low | 0.90 (90%) | Cross-reference related issue #2811 which describes the same MeasureOverride/OnSizeAllocated mismatch in MAUI | linkedIssue=#2811 |
| add-comment | medium | 0.85 (85%) | Post analysis with workaround and investigation notes | — |

**Comment draft for `add-comment`:**

```markdown
Thanks for the detailed repro — the minimal sample app makes this much easier to investigate.

The root cause appears to be that `SKCanvasViewHandler` does not override `GetDesiredSize`, so MAUI's layout engine uses the native platform view's intrinsic size rather than calling your `MeasureOverride`. After the first measure pass, MAUI caches this size, and subsequent `InvalidateMeasure()` calls don't trigger another `MeasureOverride` invocation because the handler never returns a fresh desired size.

**Workaround (confirmed by reporter):** In your property-changed handler (e.g. `InvalidateTextMetrics`), explicitly set `HeightRequest` and `WidthRequest` to the values you compute for the new size. This bypasses MAUI's measurement cache and forces re-layout.

This is related to #2811 which describes a similar sizing mismatch with `SKCanvasView` in MAUI. The fix will require overriding `GetDesiredSize` in `SKCanvasViewHandler` to properly delegate to the virtual view's `MeasureOverride`.

A few questions to help investigation:
- Does this also reproduce on Android or iOS, or only on Windows?
- Which .NET MAUI version are you targeting?
```

<details>
<summary>Raw JSON</summary>

```json
{
  "meta": {
    "schemaVersion": "1.0",
    "number": 3239,
    "repo": "mono/SkiaSharp",
    "analyzedAt": "2026-04-23T03:18:30Z",
    "currentLabels": [
      "type/bug"
    ]
  },
  "summary": "In SkiaSharp.Views.Maui.Controls 3.x, calling InvalidateMeasure() on a custom SKCanvasView-derived control fails to trigger MeasureOverride after the first layout pass, preventing dynamic resizing based on content; workaround is to explicitly set HeightRequest/WidthRequest.",
  "classification": {
    "type": {
      "value": "type/bug",
      "confidence": 0.95
    },
    "area": {
      "value": "area/SkiaSharp.Views.Maui",
      "confidence": 0.92
    },
    "platforms": [
      "os/Windows-Classic"
    ],
    "tenets": [
      "tenet/reliability",
      "tenet/compatibility"
    ],
    "partner": "partner/maui"
  },
  "evidence": {
    "bugSignals": {
      "severity": "medium",
      "regressionClaimed": true,
      "errorType": "wrong-output",
      "errorMessage": "After the first MeasureOverride occurs, calling InvalidateMeasure no longer causes MeasureOverride to occur",
      "reproQuality": "complete",
      "targetFrameworks": [
        "net9.0-windows10.0.19041"
      ]
    },
    "reproEvidence": {
      "stepsToReproduce": [
        "Create a custom control inheriting from SKCanvasView that overrides MeasureOverride to compute size based on content",
        "Place the control in a nested layout (HorizontalStackLayout or Grid)",
        "Change a property that should change the desired size and call InvalidateMeasure()",
        "Observe that MeasureOverride is NOT called again and the view does not resize"
      ],
      "environmentDetails": "SkiaSharp.Views.Maui.Controls 3.116.1, Visual Studio on Windows 11",
      "repoLinks": [
        {
          "url": "https://github.com/DanTravison/GlyphViewer/blob/Preferences/Controls/SkLabel.cs",
          "description": "Reporter's custom SKLabel control demonstrating the issue with workaround comments"
        },
        {
          "url": "https://github.com/DanTravison/GlyphViewer/blob/Preferences/Views/GlyphView.cs",
          "description": "Reporter's GlyphView control with simpler repro case"
        },
        {
          "url": "https://github.com/DanTravison/SKInvalidateMeasure",
          "description": "Minimal sample app demonstrating the issue — three SkLabel instances in different layouts"
        },
        {
          "url": "https://github.com/mono/SkiaSharp/issues/2811",
          "description": "Related issue #2811: MeasureOverride result not respected in MAUI layout, same class of problem"
        },
        {
          "url": "https://github.com/dotnet/maui/issues/4019#issuecomment-1510654509",
          "description": "MAUI upstream issue with workaround referenced by related issue #2811"
        }
      ]
    },
    "versionAnalysis": {
      "mentionedVersions": [
        "3.116.0",
        "3.116.1",
        "2.88.9"
      ],
      "workedIn": "2.88.9",
      "brokeIn": "3.116.0",
      "currentRelevance": "likely",
      "relevanceReason": "No fix or related PR found for this issue; SKCanvasViewHandler command mapper still does not override InvalidateMeasure handling"
    },
    "regression": {
      "isRegression": true,
      "confidence": 0.9,
      "reason": "Reporter explicitly states the behavior worked in SkiaSharp 2.88.9 (Xamarin.Forms era) and broke in 3.x MAUI. The MAUI view handler architecture differs fundamentally from Xamarin.Forms renderers.",
      "workedInVersion": "2.88.9",
      "brokeInVersion": "3.116.0"
    }
  },
  "analysis": {
    "summary": "SKCanvasViewHandler does not override GetDesiredSize to delegate to the MAUI virtual view's MeasureOverride. After the first measure pass, MAUI's layout engine caches the platform view's intrinsic size. Subsequent InvalidateMeasure() calls trigger the MAUI IView.InvalidateMeasure() chain (which calls the base ViewHandler's MapInvalidateMeasure) but since GetDesiredSize returns the native view's size rather than invoking MeasureOverride, the layout does not re-measure the virtual view. Setting HeightRequest/WidthRequest bypasses MAUI's measurement caching, which is why the workaround works.",
    "rationale": "The issue is clearly a regression introduced by the MAUI migration. The Xamarin.Forms renderer used OnMeasure which was called repeatedly; the MAUI handler-based architecture uses GetDesiredSize which is only delegated if the handler overrides it. SKCanvasViewHandler lacks this override, causing silent measurement caching. This is a SkiaSharp responsibility to fix in the handler layer, not purely a MAUI bug.",
    "codeInvestigation": [
      {
        "file": "source/SkiaSharp.Views.Maui/SkiaSharp.Views.Maui.Core/Handlers/SKCanvasView/SKCanvasViewHandler.cs",
        "lines": "1-31",
        "finding": "SKCanvasViewCommandMapper only maps InvalidateSurface — there is no GetDesiredSize override and no special handling for the InvalidateMeasure command beyond what the base ViewHandler.ViewCommandMapper provides",
        "relevance": "direct"
      },
      {
        "file": "source/SkiaSharp.Views.Maui/SkiaSharp.Views.Maui.Controls/SKCanvasView.cs",
        "lines": "1-67",
        "finding": "SKCanvasView (MAUI Controls) does not override MeasureOverride itself — that is left to subclasses. No mechanism exists to notify the handler that the desired size has changed.",
        "relevance": "direct"
      },
      {
        "file": "source/SkiaSharp.Views.Maui/SkiaSharp.Views.Maui.Core/Handlers/SKCanvasView/SKCanvasViewHandler.Windows.cs",
        "lines": "1-89",
        "finding": "Windows handler creates an SKXamlCanvas but does not override GetDesiredSize. When MAUI asks for the view's desired size it falls through to the base handler which measures the native XAML control's intrinsic size, ignoring the virtual view's MeasureOverride result.",
        "relevance": "direct"
      },
      {
        "file": "source/SkiaSharp.Views.Maui/SkiaSharp.Views.Maui.Core/Handlers/SKCanvasView/SKCanvasViewHandler.Android.cs",
        "lines": "1-79",
        "finding": "Android handler also does not override GetDesiredSize — same root cause applies cross-platform even though reporter only tested on Windows.",
        "relevance": "related"
      }
    ],
    "keySignals": [
      {
        "text": "After the first MeasureOverride occurs, calling InvalidateMeasure no longer causes MeasureOverride to occur.",
        "source": "issue body",
        "interpretation": "Confirms measurement caching: MAUI caches size after first measure and does not re-invoke MeasureOverride unless the handler provides a fresh desired size."
      },
      {
        "text": "explicitly setting HeightRequest in these two methods is required to workaround the issue",
        "source": "issue body",
        "interpretation": "Setting HeightRequest bypasses MAUI measurement caching by providing an explicit size constraint, confirming the root cause is in the measurement pipeline rather than painting."
      },
      {
        "text": "The two nested SkLabel instances don't resize unless the 'Try Workaround' checkbox is checked.",
        "source": "comment by DanTravison",
        "interpretation": "Root layout SkLabel works (MAUI may handle root layout measure differently) but nested layouts fail — consistent with measurement caching only affecting constraint-propagation in nested layouts."
      },
      {
        "text": "overriding SKCanvasViewHandler.GetDesiredSize does not fix the issue for me. I must still set HeightRequest",
        "source": "comment by DanTravison (2025-06-18)",
        "interpretation": "Overriding GetDesiredSize alone is insufficient — the fix must also ensure MAUI re-runs layout after property changes, suggesting the IView.InvalidateMeasure contract needs additional implementation in the handler or the virtual view."
      }
    ],
    "nextQuestions": [
      "Does the bug reproduce on Android/iOS, or is it Windows-specific?",
      "Which MAUI version is in use? The layout caching behavior may differ between MAUI versions.",
      "Is SKGLView affected by the same issue?"
    ],
    "workarounds": [
      "Set HeightRequest (and/or WidthRequest) to the computed desired height in MeasureOverride and in the property-changed handler that calls InvalidateMeasure — this forces MAUI to respect the explicit size and re-layout.",
      "Override SKCanvasViewHandler.GetDesiredSize in a custom handler and register it via MauiAppBuilder.ConfigureMauiHandlers — note that reporter found this alone did not fully resolve the issue without also setting HeightRequest."
    ],
    "resolution": {
      "hypothesis": "SKCanvasViewHandler needs to override GetDesiredSize to call the virtual view's MeasureOverride (via VirtualView.Measure / VirtualView.DesiredSize) and ensure that the MAUI IView.InvalidateMeasure command properly propagates to the platform native view's layout invalidation so subsequent measures are triggered.",
      "proposals": [
        {
          "title": "Override GetDesiredSize in SKCanvasViewHandler to delegate to MeasureOverride",
          "description": "Add a GetDesiredSize override in each platform-specific SKCanvasViewHandler that calls VirtualView.Measure(widthConstraint, heightConstraint) and returns the resulting DesiredSize. This ensures MAUI calls MeasureOverride on the virtual view rather than using the native view's intrinsic size.",
          "category": "fix",
          "confidence": 0.72,
          "effort": "cost/s",
          "validated": "untested"
        },
        {
          "title": "Set HeightRequest/WidthRequest in the property-changed handler",
          "description": "In the custom SKCanvasView subclass, after computing the desired size, explicitly set HeightRequest and WidthRequest to the computed values. This bypasses MAUI measurement caching entirely.",
          "category": "workaround",
          "confidence": 0.9,
          "effort": "cost/xs",
          "validated": "untested"
        }
      ],
      "recommendedProposal": "Set HeightRequest/WidthRequest in the property-changed handler",
      "recommendedReason": "Confirmed by reporter to work reliably, no SkiaSharp source changes needed, immediately applicable."
    }
  },
  "output": {
    "actionability": {
      "suggestedAction": "needs-investigation",
      "confidence": 0.85,
      "reason": "Confirmed regression with a minimal repro app, clear call stack, and consistent failure mode. Root cause is in SKCanvasViewHandler's measurement delegation to MAUI. Needs deeper investigation into MAUI layout engine interaction and a fix in the handler layer.",
      "suggestedReproPlatform": "windows"
    },
    "actions": [
      {
        "type": "update-labels",
        "description": "Apply bug, MAUI views area, Windows platform, reliability/compatibility tenets, and MAUI partner labels",
        "risk": "low",
        "confidence": 0.92,
        "labels": [
          "type/bug",
          "area/SkiaSharp.Views.Maui",
          "os/Windows-Classic",
          "tenet/reliability",
          "tenet/compatibility",
          "partner/maui"
        ]
      },
      {
        "type": "link-related",
        "description": "Cross-reference related issue #2811 which describes the same MeasureOverride/OnSizeAllocated mismatch in MAUI",
        "risk": "low",
        "confidence": 0.9,
        "linkedIssue": 2811
      },
      {
        "type": "add-comment",
        "description": "Post analysis with workaround and investigation notes",
        "risk": "medium",
        "confidence": 0.85,
        "comment": "Thanks for the detailed repro — the minimal sample app makes this much easier to investigate.\n\nThe root cause appears to be that `SKCanvasViewHandler` does not override `GetDesiredSize`, so MAUI's layout engine uses the native platform view's intrinsic size rather than calling your `MeasureOverride`. After the first measure pass, MAUI caches this size, and subsequent `InvalidateMeasure()` calls don't trigger another `MeasureOverride` invocation because the handler never returns a fresh desired size.\n\n**Workaround (confirmed by reporter):** In your property-changed handler (e.g. `InvalidateTextMetrics`), explicitly set `HeightRequest` and `WidthRequest` to the values you compute for the new size. This bypasses MAUI's measurement cache and forces re-layout.\n\nThis is related to #2811 which describes a similar sizing mismatch with `SKCanvasView` in MAUI. The fix will require overriding `GetDesiredSize` in `SKCanvasViewHandler` to properly delegate to the virtual view's `MeasureOverride`.\n\nA few questions to help investigation:\n- Does this also reproduce on Android or iOS, or only on Windows?\n- Which .NET MAUI version are you targeting?"
      }
    ]
  }
}
```

</details>
