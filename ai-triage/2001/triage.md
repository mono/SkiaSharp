# Issue Triage Report — #2001

| Field | Value |
|-------|-------|
| Repository | mono/SkiaSharp |
| Analyzed | 2026-04-24T14:13:53Z |
| Type | type/bug (0.92 (92%)) |
| Area | area/SkiaSharp.Views.Maui (0.95 (95%)) |
| Suggested action | needs-investigation (0.80 (80%)) |

**Issue Summary:** InvalidateSurface on SKCanvasView in .NET MAUI does not trigger OnPaintSurface across multiple platforms, particularly when the view is placed inside StackLayout or Grid with alignment options, or when InvalidateSurface is called from a background thread.

**Analysis:** InvalidateSurface dispatches via MAUI CommandMapper to platform-specific calls (SetNeedsDisplay on iOS, Invalidate on Android). Multiple distinct root causes have been identified by commenters: (1) MAUI layout containers like StackLayout fail to give SKCanvasView a measurable size, causing the platform view to not render even when invalidated; (2) calling InvalidateSurface from a non-UI thread works inconsistently across platforms; (3) alignment options (HorizontalOptions/VerticalOptions) interact poorly with SKCanvasView's custom measure logic.

**Recommendations:** **needs-investigation** — Multiple root causes identified. Background thread dispatch is a clear SkiaSharp fix. Layout sizing interaction may need MAUI upstream investigation. Both need proper reproduction and targeted fix.

---

## Classification

| Field | Value |
|-------|-------|
| Type | type/bug |
| Area | area/SkiaSharp.Views.Maui |
| Platforms | os/iOS, os/Android, os/Windows-Classic |
| Backends | — |
| Tenets | tenet/reliability |
| Partner | partner/maui |
| Current labels | type/bug, os/Windows-Classic, os/iOS, os/Android, area/SkiaSharp.Views.Maui, partner/maui, tenet/reliability, triage/triaged |

## Evidence

### Reproduction

1. Create a MAUI app with SKCanvasView placed inside a StackLayout
2. Call InvalidateSurface() from a background thread (e.g., System.Timer callback)
3. Observe that OnPaintSurface is never invoked on iOS or Android
4. Alternatively: set HorizontalOptions or VerticalOptions on SKCanvasView inside a Grid

**Environment:** VS 2022 17.x, .NET MAUI 7/8, SkiaSharp 2.88.x–3.x, iOS device connected directly to Windows

**Related issues:** #6120

**Repository links:**
- https://github.com/dotnet/maui/issues/6120 — MAUI upstream issue referenced by reporter

### Bug Signals

| Field | Value |
|-------|-------|
| Severity | medium |
| Regression claimed | False |
| Error type | missing-output |
| Error message | OnPaintSurface never called after InvalidateSurface |
| Repro quality | partial |
| Target frameworks | net7.0-ios, net8.0-ios, net8.0-android, net7.0-maccatalyst |

### Version Analysis

| Field | Value |
|-------|-------|
| Mentioned versions | 2.88.0, 2.88.1-preview.63, 2.88.3, 2.88.8 |
| Worked in | — |
| Broke in | — |
| Current relevance | likely |
| Relevance reason | The handler dispatch mechanism via Handler?.Invoke and layout-size interaction has not fundamentally changed in the MAUI views handler code. |

## Analysis

### Technical Summary

InvalidateSurface dispatches via MAUI CommandMapper to platform-specific calls (SetNeedsDisplay on iOS, Invalidate on Android). Multiple distinct root causes have been identified by commenters: (1) MAUI layout containers like StackLayout fail to give SKCanvasView a measurable size, causing the platform view to not render even when invalidated; (2) calling InvalidateSurface from a non-UI thread works inconsistently across platforms; (3) alignment options (HorizontalOptions/VerticalOptions) interact poorly with SKCanvasView's custom measure logic.

### Rationale

The issue is a genuine bug — InvalidateSurface should reliably call OnPaintSurface when the view is visible. Multiple users across 2 years confirm it across iOS, Android, and Windows. There are multiple possible root causes (layout sizing vs thread dispatch), pointing to an underlying fragility in how MAUI's handler architecture integrates with SkiaSharp's view sizing. Workarounds exist (ContentView wrapper, MainThread dispatch), confirming the bug is not a fundamental incompatibility.

### Key Signals

- "InvalidateSurface does not trigger OnPaintSurface, when the app is deployed to an iOS device, directly connected to the Windows dev computer" — **issue body** (Original report: iOS device remote debugging from Windows, suggesting possible threading issue in the hot path)
- "It looks like this issue may be caused by placing the SKCanvasView inside a StackLayout" — **comment by yoshiask** (Layout container sizing is a key contributing factor — StackLayout may not propagate a concrete size to SKCanvasView)
- "I called InvalidateSurface intermittently with System.Timer and on IOS it doesn't call OnPaintSurface. MainThread.BeginInvokeOnMainThread(InvalidateSurface) solves this problem." — **comment by vladtrabl** (Background thread dispatch is a confirmed secondary cause — InvalidateSurface does not marshal to UI thread)
- "I have placed under a ContentView... Can confirm that this workaround worked for me on Android & iOS with .NET8 MAUI 8.0.100" — **comments by Niv2023 and mackayn** (ContentView wrapper fixes layout sizing issue, confirming that is the primary root cause for most users)
- "Without it (HorizontalOptions=center) it works. If I try to wrap it into contentview or another grid to center it by centering a wrapper it also doesn't work." — **comment by VNGames** (Alignment options affect size propagation, adding another dimension to the layout root cause)

### Code Investigation

| File | Lines | Relevance | Finding |
|------|-------|-----------|---------|
| `source/SkiaSharp.Views.Maui/SkiaSharp.Views.Maui.Controls/SKCanvasView.cs` | 43-46 | direct | InvalidateSurface calls Handler?.Invoke(nameof(ISKCanvasView.InvalidateSurface)) — this does NOT marshal to the main thread. If called from a background thread, behavior is undefined across platforms. |
| `source/SkiaSharp.Views.Maui/SkiaSharp.Views.Maui.Core/Handlers/SKCanvasView/SKCanvasViewHandler.Apple.cs` | 37-43 | direct | OnInvalidateSurface calls handler.PlatformView.SetNeedsDisplay(). Has a null-check. SetNeedsDisplay is a UIKit API that only works on the main thread — calling from background thread silently fails on iOS. |
| `source/SkiaSharp.Views.Maui/SkiaSharp.Views.Maui.Core/Handlers/SKCanvasView/SKCanvasViewHandler.Android.cs` | 35-38 | direct | OnInvalidateSurface calls handler.PlatformView.Invalidate() directly with NO null-check on PlatformView and NO thread safety. Android can tolerate off-thread Invalidate but may produce inconsistent results. |
| `source/SkiaSharp.Views.Maui/SkiaSharp.Views.Maui.Core/Handlers/SKCanvasView/SKCanvasViewHandler.cs` | 15-20 | related | SKCanvasViewCommandMapper registers InvalidateSurface command handler. No thread dispatch is performed at the command dispatcher level — dispatch is left entirely to platform implementations. |

### Workarounds

- Wrap SKCanvasView in a ContentView and set size/alignment on the ContentView instead — confirmed working on Android and iOS with MAUI 8
- Call MainThread.BeginInvokeOnMainThread(() => skCanvasView.InvalidateSurface()) instead of calling InvalidateSurface() directly from background threads
- Remove HorizontalOptions and VerticalOptions from the SKCanvasView element in XAML
- Place SKCanvasView directly inside a ContentPage or ContentView rather than StackLayout/Grid

### Next Questions

- Does adding main-thread dispatch inside InvalidateSurface() fix all reported cases?
- Is the layout sizing issue a SkiaSharp problem (MeasureOverride not returning correct size) or a MAUI layout container issue?
- Does the same issue occur with SKGLView?
- Has this been fixed in recent MAUI releases (8.0.100+) or does it persist?

### Resolution Proposals

**Hypothesis:** Two distinct root causes: (1) InvalidateSurface does not dispatch to UI thread, causing silent failure on iOS when called from background threads; (2) MAUI layout containers fail to propagate a concrete size to SKCanvasView when alignment options are set, causing the platform view to render at zero size.

1. **Add main-thread dispatch in InvalidateSurface** — fix, confidence 0.75 (75%), cost/s, validated=untested
   - Add MainThread.BeginInvokeOnMainThread dispatch in SKCanvasView.InvalidateSurface() or in each platform handler's OnInvalidateSurface to ensure it always runs on the UI thread.
2. **Wrap SKCanvasView in ContentView (user workaround)** — workaround, confidence 0.90 (90%), cost/xs, validated=yes
   - Wrap the SKCanvasView inside a ContentView in XAML and set HeightRequest/WidthRequest on the ContentView wrapper instead of the SKCanvasView directly. This provides a concrete size to the layout system.
3. **Use MainThread.BeginInvokeOnMainThread for background timer calls** — workaround, confidence 0.85 (85%), cost/xs, validated=yes
   - When calling InvalidateSurface from a timer or background thread, wrap with MainThread.BeginInvokeOnMainThread(() => myView.InvalidateSurface()).

**Recommended proposal:** Add main-thread dispatch in InvalidateSurface

**Why:** Addresses the iOS/background-thread root cause with minimal risk. The layout sizing issue may be a MAUI upstream concern.

## Recommendations

### Actionability

| Field | Value |
|-------|-------|
| Suggested action | needs-investigation |
| Confidence | 0.80 (80%) |
| Reason | Multiple root causes identified. Background thread dispatch is a clear SkiaSharp fix. Layout sizing interaction may need MAUI upstream investigation. Both need proper reproduction and targeted fix. |
| Suggested repro platform | linux |

### Automatable Actions

| Type | Risk | Confidence | Description | Details |
|------|------|------------|-------------|---------|
| update-labels | low | 0.95 (95%) | Apply bug, MAUI views, platform, partner, and tenet labels | labels=type/bug, area/SkiaSharp.Views.Maui, os/iOS, os/Android, os/Windows-Classic, partner/maui, tenet/reliability |
| add-comment | medium | 0.80 (80%) | Post analysis with identified root causes and confirmed workarounds | — |

**Comment draft for `add-comment`:**

```markdown
Thanks for the report. After investigating the code and comments, two root causes have been identified:

**1. Background thread dispatch (iOS-specific):** `InvalidateSurface()` does not marshal to the main thread. On iOS, `SetNeedsDisplay()` silently fails when called from a background thread. **Workaround:** Use `MainThread.BeginInvokeOnMainThread(() => myView.InvalidateSurface())`.

**2. MAUI layout sizing interaction:** When `SKCanvasView` is placed inside `StackLayout`, `Grid` with alignment options, or with `HorizontalOptions`/`VerticalOptions` set, MAUI may not propagate a concrete size to the view, causing the platform view to have zero size and not render. **Workaround:** Wrap `SKCanvasView` in a `ContentView` and set size/alignment on the wrapper:

```xml
<ContentView HeightRequest="200" HorizontalOptions="Center">
    <skia:SKCanvasView PaintSurface="OnPaintSurface" />
</ContentView>
```

These workarounds have been confirmed by multiple users. We'll continue investigating a proper fix for both root causes.
```

<details>
<summary>Raw JSON</summary>

```json
{
  "meta": {
    "schemaVersion": "1.0",
    "number": 2001,
    "repo": "mono/SkiaSharp",
    "analyzedAt": "2026-04-24T14:13:53Z",
    "currentLabels": [
      "type/bug",
      "os/Windows-Classic",
      "os/iOS",
      "os/Android",
      "area/SkiaSharp.Views.Maui",
      "partner/maui",
      "tenet/reliability",
      "triage/triaged"
    ]
  },
  "summary": "InvalidateSurface on SKCanvasView in .NET MAUI does not trigger OnPaintSurface across multiple platforms, particularly when the view is placed inside StackLayout or Grid with alignment options, or when InvalidateSurface is called from a background thread.",
  "classification": {
    "type": {
      "value": "type/bug",
      "confidence": 0.92
    },
    "area": {
      "value": "area/SkiaSharp.Views.Maui",
      "confidence": 0.95
    },
    "platforms": [
      "os/iOS",
      "os/Android",
      "os/Windows-Classic"
    ],
    "tenets": [
      "tenet/reliability"
    ],
    "partner": "partner/maui"
  },
  "evidence": {
    "bugSignals": {
      "severity": "medium",
      "regressionClaimed": false,
      "errorType": "missing-output",
      "errorMessage": "OnPaintSurface never called after InvalidateSurface",
      "reproQuality": "partial",
      "targetFrameworks": [
        "net7.0-ios",
        "net8.0-ios",
        "net8.0-android",
        "net7.0-maccatalyst"
      ]
    },
    "reproEvidence": {
      "stepsToReproduce": [
        "Create a MAUI app with SKCanvasView placed inside a StackLayout",
        "Call InvalidateSurface() from a background thread (e.g., System.Timer callback)",
        "Observe that OnPaintSurface is never invoked on iOS or Android",
        "Alternatively: set HorizontalOptions or VerticalOptions on SKCanvasView inside a Grid"
      ],
      "environmentDetails": "VS 2022 17.x, .NET MAUI 7/8, SkiaSharp 2.88.x–3.x, iOS device connected directly to Windows",
      "relatedIssues": [
        6120
      ],
      "repoLinks": [
        {
          "url": "https://github.com/dotnet/maui/issues/6120",
          "description": "MAUI upstream issue referenced by reporter"
        }
      ]
    },
    "versionAnalysis": {
      "mentionedVersions": [
        "2.88.0",
        "2.88.1-preview.63",
        "2.88.3",
        "2.88.8"
      ],
      "currentRelevance": "likely",
      "relevanceReason": "The handler dispatch mechanism via Handler?.Invoke and layout-size interaction has not fundamentally changed in the MAUI views handler code."
    }
  },
  "analysis": {
    "summary": "InvalidateSurface dispatches via MAUI CommandMapper to platform-specific calls (SetNeedsDisplay on iOS, Invalidate on Android). Multiple distinct root causes have been identified by commenters: (1) MAUI layout containers like StackLayout fail to give SKCanvasView a measurable size, causing the platform view to not render even when invalidated; (2) calling InvalidateSurface from a non-UI thread works inconsistently across platforms; (3) alignment options (HorizontalOptions/VerticalOptions) interact poorly with SKCanvasView's custom measure logic.",
    "rationale": "The issue is a genuine bug — InvalidateSurface should reliably call OnPaintSurface when the view is visible. Multiple users across 2 years confirm it across iOS, Android, and Windows. There are multiple possible root causes (layout sizing vs thread dispatch), pointing to an underlying fragility in how MAUI's handler architecture integrates with SkiaSharp's view sizing. Workarounds exist (ContentView wrapper, MainThread dispatch), confirming the bug is not a fundamental incompatibility.",
    "keySignals": [
      {
        "text": "InvalidateSurface does not trigger OnPaintSurface, when the app is deployed to an iOS device, directly connected to the Windows dev computer",
        "source": "issue body",
        "interpretation": "Original report: iOS device remote debugging from Windows, suggesting possible threading issue in the hot path"
      },
      {
        "text": "It looks like this issue may be caused by placing the SKCanvasView inside a StackLayout",
        "source": "comment by yoshiask",
        "interpretation": "Layout container sizing is a key contributing factor — StackLayout may not propagate a concrete size to SKCanvasView"
      },
      {
        "text": "I called InvalidateSurface intermittently with System.Timer and on IOS it doesn't call OnPaintSurface. MainThread.BeginInvokeOnMainThread(InvalidateSurface) solves this problem.",
        "source": "comment by vladtrabl",
        "interpretation": "Background thread dispatch is a confirmed secondary cause — InvalidateSurface does not marshal to UI thread"
      },
      {
        "text": "I have placed under a ContentView... Can confirm that this workaround worked for me on Android & iOS with .NET8 MAUI 8.0.100",
        "source": "comments by Niv2023 and mackayn",
        "interpretation": "ContentView wrapper fixes layout sizing issue, confirming that is the primary root cause for most users"
      },
      {
        "text": "Without it (HorizontalOptions=center) it works. If I try to wrap it into contentview or another grid to center it by centering a wrapper it also doesn't work.",
        "source": "comment by VNGames",
        "interpretation": "Alignment options affect size propagation, adding another dimension to the layout root cause"
      }
    ],
    "codeInvestigation": [
      {
        "file": "source/SkiaSharp.Views.Maui/SkiaSharp.Views.Maui.Controls/SKCanvasView.cs",
        "lines": "43-46",
        "finding": "InvalidateSurface calls Handler?.Invoke(nameof(ISKCanvasView.InvalidateSurface)) — this does NOT marshal to the main thread. If called from a background thread, behavior is undefined across platforms.",
        "relevance": "direct"
      },
      {
        "file": "source/SkiaSharp.Views.Maui/SkiaSharp.Views.Maui.Core/Handlers/SKCanvasView/SKCanvasViewHandler.Apple.cs",
        "lines": "37-43",
        "finding": "OnInvalidateSurface calls handler.PlatformView.SetNeedsDisplay(). Has a null-check. SetNeedsDisplay is a UIKit API that only works on the main thread — calling from background thread silently fails on iOS.",
        "relevance": "direct"
      },
      {
        "file": "source/SkiaSharp.Views.Maui/SkiaSharp.Views.Maui.Core/Handlers/SKCanvasView/SKCanvasViewHandler.Android.cs",
        "lines": "35-38",
        "finding": "OnInvalidateSurface calls handler.PlatformView.Invalidate() directly with NO null-check on PlatformView and NO thread safety. Android can tolerate off-thread Invalidate but may produce inconsistent results.",
        "relevance": "direct"
      },
      {
        "file": "source/SkiaSharp.Views.Maui/SkiaSharp.Views.Maui.Core/Handlers/SKCanvasView/SKCanvasViewHandler.cs",
        "lines": "15-20",
        "finding": "SKCanvasViewCommandMapper registers InvalidateSurface command handler. No thread dispatch is performed at the command dispatcher level — dispatch is left entirely to platform implementations.",
        "relevance": "related"
      }
    ],
    "workarounds": [
      "Wrap SKCanvasView in a ContentView and set size/alignment on the ContentView instead — confirmed working on Android and iOS with MAUI 8",
      "Call MainThread.BeginInvokeOnMainThread(() => skCanvasView.InvalidateSurface()) instead of calling InvalidateSurface() directly from background threads",
      "Remove HorizontalOptions and VerticalOptions from the SKCanvasView element in XAML",
      "Place SKCanvasView directly inside a ContentPage or ContentView rather than StackLayout/Grid"
    ],
    "nextQuestions": [
      "Does adding main-thread dispatch inside InvalidateSurface() fix all reported cases?",
      "Is the layout sizing issue a SkiaSharp problem (MeasureOverride not returning correct size) or a MAUI layout container issue?",
      "Does the same issue occur with SKGLView?",
      "Has this been fixed in recent MAUI releases (8.0.100+) or does it persist?"
    ],
    "resolution": {
      "hypothesis": "Two distinct root causes: (1) InvalidateSurface does not dispatch to UI thread, causing silent failure on iOS when called from background threads; (2) MAUI layout containers fail to propagate a concrete size to SKCanvasView when alignment options are set, causing the platform view to render at zero size.",
      "proposals": [
        {
          "title": "Add main-thread dispatch in InvalidateSurface",
          "description": "Add MainThread.BeginInvokeOnMainThread dispatch in SKCanvasView.InvalidateSurface() or in each platform handler's OnInvalidateSurface to ensure it always runs on the UI thread.",
          "category": "fix",
          "confidence": 0.75,
          "effort": "cost/s",
          "validated": "untested"
        },
        {
          "title": "Wrap SKCanvasView in ContentView (user workaround)",
          "description": "Wrap the SKCanvasView inside a ContentView in XAML and set HeightRequest/WidthRequest on the ContentView wrapper instead of the SKCanvasView directly. This provides a concrete size to the layout system.",
          "category": "workaround",
          "confidence": 0.9,
          "effort": "cost/xs",
          "validated": "yes"
        },
        {
          "title": "Use MainThread.BeginInvokeOnMainThread for background timer calls",
          "description": "When calling InvalidateSurface from a timer or background thread, wrap with MainThread.BeginInvokeOnMainThread(() => myView.InvalidateSurface()).",
          "category": "workaround",
          "confidence": 0.85,
          "effort": "cost/xs",
          "validated": "yes"
        }
      ],
      "recommendedProposal": "Add main-thread dispatch in InvalidateSurface",
      "recommendedReason": "Addresses the iOS/background-thread root cause with minimal risk. The layout sizing issue may be a MAUI upstream concern."
    }
  },
  "output": {
    "actionability": {
      "suggestedAction": "needs-investigation",
      "confidence": 0.8,
      "reason": "Multiple root causes identified. Background thread dispatch is a clear SkiaSharp fix. Layout sizing interaction may need MAUI upstream investigation. Both need proper reproduction and targeted fix.",
      "suggestedReproPlatform": "linux"
    },
    "actions": [
      {
        "type": "update-labels",
        "description": "Apply bug, MAUI views, platform, partner, and tenet labels",
        "risk": "low",
        "confidence": 0.95,
        "labels": [
          "type/bug",
          "area/SkiaSharp.Views.Maui",
          "os/iOS",
          "os/Android",
          "os/Windows-Classic",
          "partner/maui",
          "tenet/reliability"
        ]
      },
      {
        "type": "add-comment",
        "description": "Post analysis with identified root causes and confirmed workarounds",
        "risk": "medium",
        "confidence": 0.8,
        "comment": "Thanks for the report. After investigating the code and comments, two root causes have been identified:\n\n**1. Background thread dispatch (iOS-specific):** `InvalidateSurface()` does not marshal to the main thread. On iOS, `SetNeedsDisplay()` silently fails when called from a background thread. **Workaround:** Use `MainThread.BeginInvokeOnMainThread(() => myView.InvalidateSurface())`.\n\n**2. MAUI layout sizing interaction:** When `SKCanvasView` is placed inside `StackLayout`, `Grid` with alignment options, or with `HorizontalOptions`/`VerticalOptions` set, MAUI may not propagate a concrete size to the view, causing the platform view to have zero size and not render. **Workaround:** Wrap `SKCanvasView` in a `ContentView` and set size/alignment on the wrapper:\n\n```xml\n<ContentView HeightRequest=\"200\" HorizontalOptions=\"Center\">\n    <skia:SKCanvasView PaintSurface=\"OnPaintSurface\" />\n</ContentView>\n```\n\nThese workarounds have been confirmed by multiple users. We'll continue investigating a proper fix for both root causes."
      }
    ]
  }
}
```

</details>
