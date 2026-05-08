# Issue Triage Report — #2552

| Field | Value |
|-------|-------|
| Repository | mono/SkiaSharp |
| Analyzed | 2026-04-28T12:53:57Z |
| Type | type/bug (0.82 (82%)) |
| Area | area/SkiaSharp.Views.Maui (0.75 (75%)) |
| Suggested action | needs-info (0.80 (80%)) |

**Issue Summary:** SKLottieView animation is not visible when used inside a custom MAUI component that subclasses SKLottieView, but works when SKLottieView is used directly on a XAML page.

**Analysis:** When a custom MAUI component inherits from SKLottieView (SkiaSharp.Extended.UI), the animation does not render. This is likely caused by MAUI handler registration only targeting SKLottieView specifically, so the user's derived type lacks a handler and the native rendering surface is never created. A confirmed workaround exists: wrap SKLottieView in a ContentView instead of inheriting.

**Recommendations:** **needs-info** — Reporter did not provide SkiaSharp or SkiaSharp.Extended.UI version details. A confirmed workaround exists but the root cause and fix scope require version data to investigate further.

---

## Classification

| Field | Value |
|-------|-------|
| Type | type/bug |
| Area | area/SkiaSharp.Views.Maui |
| Platforms | — |
| Backends | — |
| Tenets | — |
| Partner | partner/maui |

## Evidence

### Reproduction

1. Create a MAUI project with SkiaSharp.Extended.UI
2. Add SKLottieView directly in a XAML page — animation shows correctly
3. Create a custom component class that inherits from SKLottieView
4. Place the custom component on the same XAML page
5. Observe that animation is not visible in the custom component

**Environment:** MAUI app with SkiaSharp.Extended.UI; version details not provided by reporter (template placeholder left unfilled)

**Repository links:**
- https://github.com/mono/SkiaSharp/files/12342183/AnimationMauiTest.zip — Sample MAUI project demonstrating the issue with direct SKLottieView vs custom subclass

### Bug Signals

| Field | Value |
|-------|-------|
| Severity | medium |
| Regression claimed | False |
| Error type | wrong-output |
| Error message | — |
| Repro quality | partial |
| Target frameworks | — |

## Analysis

### Technical Summary

When a custom MAUI component inherits from SKLottieView (SkiaSharp.Extended.UI), the animation does not render. This is likely caused by MAUI handler registration only targeting SKLottieView specifically, so the user's derived type lacks a handler and the native rendering surface is never created. A confirmed workaround exists: wrap SKLottieView in a ContentView instead of inheriting.

### Rationale

Animation works when used directly but silently fails in a subclass — consistent with MAUI's handler architecture where a handler registered for the base type may not activate for a user-defined derived type. The AnimationFailed event not firing confirms no runtime exception, just a silent rendering skip. A community member confirmed the ContentView wrapping workaround. Missing version info prevents full root-cause pinpointing.

### Key Signals

- "if i create an own components which contains the same SKLottieView: the animation is not visible" — **issue body** (Custom component (subclass of SKLottieView) fails to render the animation, suggesting MAUI handler is not activated for the derived type.)
- "The event handler for AnimationFailed is not called" — **issue body** (No error fires — the rendering pipeline is simply bypassed for the custom component, not a runtime exception.)
- "PASTE ANY DETAILED VERSION INFO HERE" — **issue body** (Reporter did not fill in the version template — SkiaSharp and SkiaSharp.Extended.UI versions are unknown.)
- "One workaround that worked for me was, wrapping the SKLottieView inside of a ContentView instead of directly inheriting SKLottieView." — **comment by TimLandskron** (ContentView composition avoids the handler resolution issue entirely, confirming the root cause is in MAUI handler lookup for subclassed view types.)

### Code Investigation

| File | Lines | Relevance | Finding |
|------|-------|-----------|---------|
| `source/SkiaSharp.Views.Maui/SkiaSharp.Views.Maui.Controls/AppHostBuilderExtensions.cs` | 15-16 | direct | Handlers are registered only for SKCanvasView and SKGLView via AddHandler<TView, THandler>(). SKLottieView from SkiaSharp.Extended.UI must register its own handler separately. When the user subclasses SKLottieView, the registered handler targets the base type. If MAUI handler resolution does not walk the full inheritance chain for the user's custom type, the view renders as empty. |
| `source/SkiaSharp.Views.Maui/SkiaSharp.Views.Maui.Controls/SKCanvasView.cs` | 11-66 | related | SKCanvasView is the MAUI Views base class implementing ISKCanvasView. It relies on Handler being set by the MAUI framework to invoke the native rendering surface. If a subclass of SKCanvasView (or SKLottieView) does not get a handler mapped to it, Handler will be null and the rendering surface is never created. |

### Workarounds

- Wrap SKLottieView inside a ContentView instead of directly subclassing SKLottieView. Define AnimationControl as a ContentView with SKLottieView as a child element in XAML rather than inheriting from SKLottieView.

### Next Questions

- Which version of SkiaSharp and SkiaSharp.Extended.UI is being used?
- Does the same issue occur on all MAUI target platforms (Android, iOS, Windows)?
- Does explicitly registering a handler for the custom subclass type in MauiProgram.cs resolve the issue?

### Resolution Proposals

**Hypothesis:** MAUI's handler resolution for user-defined subclasses of SKLottieView does not trigger the native rendering pipeline because the handler is registered only for SKLottieView itself, not for arbitrary derived types.

1. **Wrap in ContentView (confirmed workaround)** — workaround, confidence 0.95 (95%), cost/xs, validated=yes
   - Instead of inheriting from SKLottieView, create AnimationControl as a ContentView and place SKLottieView inside it in XAML. This composition pattern avoids the handler inheritance issue entirely.

```csharp
<ContentView xmlns:controls="clr-namespace:SkiaSharp.Extended.UI.Controls;assembly=SkiaSharp.Extended.UI"
             xmlns:x="http://schemas.microsoft.com/winfx/2009/xaml"
             x:Class="AnimationMauiTest.AnimationControl">
    <controls:SKLottieView
        Source="Assets/Animations/customloader_dark.json"
        RepeatCount="-1"
        IsVisible="True"
        IsAnimationEnabled="True"
        RepeatMode="Restart"
        HeightRequest="232"
        WidthRequest="232"
        HorizontalOptions="FillAndExpand"
        VerticalOptions="FillAndExpand"/>
</ContentView>
```
2. **Register handler for the custom subclass** — fix, confidence 0.55 (55%), cost/s, validated=untested
   - In MauiProgram.cs, explicitly register a handler mapping for the custom component type. This may require accessing the SKLottieView handler type from SkiaSharp.Extended.UI.

**Recommended proposal:** Wrap in ContentView (confirmed workaround)

**Why:** Confirmed working by a community member, minimal code change, and aligns with MAUI's recommended composition-over-inheritance pattern for handler-based views.

## Recommendations

### Actionability

| Field | Value |
|-------|-------|
| Suggested action | needs-info |
| Confidence | 0.80 (80%) |
| Reason | Reporter did not provide SkiaSharp or SkiaSharp.Extended.UI version details. A confirmed workaround exists but the root cause and fix scope require version data to investigate further. |
| Suggested repro platform | linux |

### Missing Info

- SkiaSharp NuGet package version
- SkiaSharp.Extended.UI NuGet package version
- MAUI target framework version (e.g. net7.0-android, net8.0-ios)
- Which platform(s) exhibit the issue (Android, iOS, Windows)

### Automatable Actions

| Type | Risk | Confidence | Description | Details |
|------|------|------------|-------------|---------|
| update-labels | low | 0.85 (85%) | Apply bug, MAUI views area, and MAUI partner labels | labels=type/bug, area/SkiaSharp.Views.Maui, partner/maui |
| add-comment | medium | 0.90 (90%) | Share confirmed ContentView workaround and request missing version info | — |

**Comment draft for `add-comment`:**

```markdown
Thanks for the detailed report and repro project!

A community member found a workaround: instead of inheriting from `SKLottieView`, wrap it in a `ContentView`:

```xml
<ContentView xmlns:controls="clr-namespace:SkiaSharp.Extended.UI.Controls;assembly=SkiaSharp.Extended.UI"
             xmlns:x="http://schemas.microsoft.com/winfx/2009/xaml"
             x:Class="AnimationMauiTest.AnimationControl">
    <controls:SKLottieView
        Source="Assets/Animations/customloader_dark.json"
        RepeatCount="-1"
        IsVisible="True"
        IsAnimationEnabled="True"
        RepeatMode="Restart"
        HeightRequest="232"
        WidthRequest="232"
        HorizontalOptions="FillAndExpand"
        VerticalOptions="FillAndExpand"/>
</ContentView>
```

This follows MAUI's recommended pattern of composition over inheritance when working with handler-based views.

Could you also provide the following so we can investigate the root cause further?
- SkiaSharp NuGet package version
- SkiaSharp.Extended.UI NuGet package version
- MAUI target framework (e.g. `net7.0-android`, `net8.0-ios`)
- Platform(s) where you see the issue (Android, iOS, Windows)
```

<details>
<summary>Raw JSON</summary>

```json
{
  "meta": {
    "schemaVersion": "1.0",
    "number": 2552,
    "repo": "mono/SkiaSharp",
    "analyzedAt": "2026-04-28T12:53:57Z"
  },
  "summary": "SKLottieView animation is not visible when used inside a custom MAUI component that subclasses SKLottieView, but works when SKLottieView is used directly on a XAML page.",
  "classification": {
    "type": {
      "value": "type/bug",
      "confidence": 0.82
    },
    "area": {
      "value": "area/SkiaSharp.Views.Maui",
      "confidence": 0.75
    },
    "partner": "partner/maui"
  },
  "evidence": {
    "bugSignals": {
      "severity": "medium",
      "regressionClaimed": false,
      "errorType": "wrong-output",
      "reproQuality": "partial"
    },
    "reproEvidence": {
      "stepsToReproduce": [
        "Create a MAUI project with SkiaSharp.Extended.UI",
        "Add SKLottieView directly in a XAML page — animation shows correctly",
        "Create a custom component class that inherits from SKLottieView",
        "Place the custom component on the same XAML page",
        "Observe that animation is not visible in the custom component"
      ],
      "environmentDetails": "MAUI app with SkiaSharp.Extended.UI; version details not provided by reporter (template placeholder left unfilled)",
      "repoLinks": [
        {
          "url": "https://github.com/mono/SkiaSharp/files/12342183/AnimationMauiTest.zip",
          "description": "Sample MAUI project demonstrating the issue with direct SKLottieView vs custom subclass"
        }
      ]
    }
  },
  "analysis": {
    "summary": "When a custom MAUI component inherits from SKLottieView (SkiaSharp.Extended.UI), the animation does not render. This is likely caused by MAUI handler registration only targeting SKLottieView specifically, so the user's derived type lacks a handler and the native rendering surface is never created. A confirmed workaround exists: wrap SKLottieView in a ContentView instead of inheriting.",
    "rationale": "Animation works when used directly but silently fails in a subclass — consistent with MAUI's handler architecture where a handler registered for the base type may not activate for a user-defined derived type. The AnimationFailed event not firing confirms no runtime exception, just a silent rendering skip. A community member confirmed the ContentView wrapping workaround. Missing version info prevents full root-cause pinpointing.",
    "keySignals": [
      {
        "text": "if i create an own components which contains the same SKLottieView: the animation is not visible",
        "source": "issue body",
        "interpretation": "Custom component (subclass of SKLottieView) fails to render the animation, suggesting MAUI handler is not activated for the derived type."
      },
      {
        "text": "The event handler for AnimationFailed is not called",
        "source": "issue body",
        "interpretation": "No error fires — the rendering pipeline is simply bypassed for the custom component, not a runtime exception."
      },
      {
        "text": "PASTE ANY DETAILED VERSION INFO HERE",
        "source": "issue body",
        "interpretation": "Reporter did not fill in the version template — SkiaSharp and SkiaSharp.Extended.UI versions are unknown."
      },
      {
        "text": "One workaround that worked for me was, wrapping the SKLottieView inside of a ContentView instead of directly inheriting SKLottieView.",
        "source": "comment by TimLandskron",
        "interpretation": "ContentView composition avoids the handler resolution issue entirely, confirming the root cause is in MAUI handler lookup for subclassed view types."
      }
    ],
    "codeInvestigation": [
      {
        "file": "source/SkiaSharp.Views.Maui/SkiaSharp.Views.Maui.Controls/AppHostBuilderExtensions.cs",
        "lines": "15-16",
        "finding": "Handlers are registered only for SKCanvasView and SKGLView via AddHandler<TView, THandler>(). SKLottieView from SkiaSharp.Extended.UI must register its own handler separately. When the user subclasses SKLottieView, the registered handler targets the base type. If MAUI handler resolution does not walk the full inheritance chain for the user's custom type, the view renders as empty.",
        "relevance": "direct"
      },
      {
        "file": "source/SkiaSharp.Views.Maui/SkiaSharp.Views.Maui.Controls/SKCanvasView.cs",
        "lines": "11-66",
        "finding": "SKCanvasView is the MAUI Views base class implementing ISKCanvasView. It relies on Handler being set by the MAUI framework to invoke the native rendering surface. If a subclass of SKCanvasView (or SKLottieView) does not get a handler mapped to it, Handler will be null and the rendering surface is never created.",
        "relevance": "related"
      }
    ],
    "workarounds": [
      "Wrap SKLottieView inside a ContentView instead of directly subclassing SKLottieView. Define AnimationControl as a ContentView with SKLottieView as a child element in XAML rather than inheriting from SKLottieView."
    ],
    "nextQuestions": [
      "Which version of SkiaSharp and SkiaSharp.Extended.UI is being used?",
      "Does the same issue occur on all MAUI target platforms (Android, iOS, Windows)?",
      "Does explicitly registering a handler for the custom subclass type in MauiProgram.cs resolve the issue?"
    ],
    "resolution": {
      "hypothesis": "MAUI's handler resolution for user-defined subclasses of SKLottieView does not trigger the native rendering pipeline because the handler is registered only for SKLottieView itself, not for arbitrary derived types.",
      "proposals": [
        {
          "title": "Wrap in ContentView (confirmed workaround)",
          "description": "Instead of inheriting from SKLottieView, create AnimationControl as a ContentView and place SKLottieView inside it in XAML. This composition pattern avoids the handler inheritance issue entirely.",
          "category": "workaround",
          "codeSnippet": "<ContentView xmlns:controls=\"clr-namespace:SkiaSharp.Extended.UI.Controls;assembly=SkiaSharp.Extended.UI\"\n             xmlns:x=\"http://schemas.microsoft.com/winfx/2009/xaml\"\n             x:Class=\"AnimationMauiTest.AnimationControl\">\n    <controls:SKLottieView\n        Source=\"Assets/Animations/customloader_dark.json\"\n        RepeatCount=\"-1\"\n        IsVisible=\"True\"\n        IsAnimationEnabled=\"True\"\n        RepeatMode=\"Restart\"\n        HeightRequest=\"232\"\n        WidthRequest=\"232\"\n        HorizontalOptions=\"FillAndExpand\"\n        VerticalOptions=\"FillAndExpand\"/>\n</ContentView>",
          "confidence": 0.95,
          "effort": "cost/xs",
          "validated": "yes"
        },
        {
          "title": "Register handler for the custom subclass",
          "description": "In MauiProgram.cs, explicitly register a handler mapping for the custom component type. This may require accessing the SKLottieView handler type from SkiaSharp.Extended.UI.",
          "category": "fix",
          "confidence": 0.55,
          "effort": "cost/s",
          "validated": "untested"
        }
      ],
      "recommendedProposal": "Wrap in ContentView (confirmed workaround)",
      "recommendedReason": "Confirmed working by a community member, minimal code change, and aligns with MAUI's recommended composition-over-inheritance pattern for handler-based views."
    }
  },
  "output": {
    "actionability": {
      "suggestedAction": "needs-info",
      "confidence": 0.8,
      "reason": "Reporter did not provide SkiaSharp or SkiaSharp.Extended.UI version details. A confirmed workaround exists but the root cause and fix scope require version data to investigate further.",
      "suggestedReproPlatform": "linux"
    },
    "missingInfo": [
      "SkiaSharp NuGet package version",
      "SkiaSharp.Extended.UI NuGet package version",
      "MAUI target framework version (e.g. net7.0-android, net8.0-ios)",
      "Which platform(s) exhibit the issue (Android, iOS, Windows)"
    ],
    "actions": [
      {
        "type": "update-labels",
        "description": "Apply bug, MAUI views area, and MAUI partner labels",
        "risk": "low",
        "confidence": 0.85,
        "labels": [
          "type/bug",
          "area/SkiaSharp.Views.Maui",
          "partner/maui"
        ]
      },
      {
        "type": "add-comment",
        "description": "Share confirmed ContentView workaround and request missing version info",
        "risk": "medium",
        "confidence": 0.9,
        "comment": "Thanks for the detailed report and repro project!\n\nA community member found a workaround: instead of inheriting from `SKLottieView`, wrap it in a `ContentView`:\n\n```xml\n<ContentView xmlns:controls=\"clr-namespace:SkiaSharp.Extended.UI.Controls;assembly=SkiaSharp.Extended.UI\"\n             xmlns:x=\"http://schemas.microsoft.com/winfx/2009/xaml\"\n             x:Class=\"AnimationMauiTest.AnimationControl\">\n    <controls:SKLottieView\n        Source=\"Assets/Animations/customloader_dark.json\"\n        RepeatCount=\"-1\"\n        IsVisible=\"True\"\n        IsAnimationEnabled=\"True\"\n        RepeatMode=\"Restart\"\n        HeightRequest=\"232\"\n        WidthRequest=\"232\"\n        HorizontalOptions=\"FillAndExpand\"\n        VerticalOptions=\"FillAndExpand\"/>\n</ContentView>\n```\n\nThis follows MAUI's recommended pattern of composition over inheritance when working with handler-based views.\n\nCould you also provide the following so we can investigate the root cause further?\n- SkiaSharp NuGet package version\n- SkiaSharp.Extended.UI NuGet package version\n- MAUI target framework (e.g. `net7.0-android`, `net8.0-ios`)\n- Platform(s) where you see the issue (Android, iOS, Windows)"
      }
    ]
  }
}
```

</details>
