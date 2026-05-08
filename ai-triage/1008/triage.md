# Issue Triage Report — #1008

| Field | Value |
|-------|-------|
| Repository | mono/SkiaSharp |
| Analyzed | 2026-05-03T00:05:00Z |
| Type | type/enhancement (0.85 (85%)) |
| Area | area/SkiaSharp.Views (0.90 (90%)) |
| Suggested action | keep-open (0.80 (80%)) |

**Issue Summary:** SKCanvasView (UWP/Xamarin.Forms) does not appear in the accessibility tree because it renders via a WriteableBitmap background brush on a Canvas with no accessible children or automation properties.

**Analysis:** SKXamlCanvas (the UWP implementation) extends the XAML Canvas panel and renders its content by setting Background to an ImageBrush backed by a WriteableBitmap. Because no XAML children are added and no OnCreateAutomationPeer() override or AutomationProperties are implemented, accessibility tools see an empty panel with no accessible content. Accessibility support was never built for this control.

**Recommendations:** **keep-open** — Valid enhancement request — UWP accessibility support (automation peer) was never implemented for SKXamlCanvas. A workaround exists using AutomationProperties.

---

## Classification

| Field | Value |
|-------|-------|
| Type | type/enhancement |
| Area | area/SkiaSharp.Views |
| Platforms | os/Windows-Universal-UWP |
| Backends | — |
| Tenets | tenet/compatibility |
| Partner | — |

## Evidence

### Reproduction

1. Create a Xamarin.Forms UWP app using SKCanvasView as base class for a custom control
2. Display SVG content via SkiaSharp rendering
3. Open Accessibility Inspect tool and observe the visual tree
4. Notice the SKCanvasView node does not appear in accessibility data

**Environment:** SkiaSharp 1.68.1-rc.165, Xamarin.Forms, UWP MinVersion 17134 (Win10 1803), VS 2019, desktop computer

**Screenshots:**
- https://user-images.githubusercontent.com/13911774/68713980-e031b580-0553-11ea-91cd-2224f6dd3f98.png — Accessibility Inspect showing surrounding text elements visible but SKCanvasView SVG image node absent

**Attachments:**
- SkiaSharpBug.zip — https://github.com/mono/SkiaSharp/files/3838377/SkiaSharpBug.zip — Sample Xamarin.Forms UWP application demonstrating the accessibility gap

### Version Analysis

| Field | Value |
|-------|-------|
| Mentioned versions | 1.68.1-rc.165 |
| Worked in | — |
| Broke in | — |
| Current relevance | likely |
| Relevance reason | SKXamlCanvas still extends Canvas and renders via WriteableBitmap ImageBrush with no accessibility API support — the same architecture exists in the current codebase. |

## Analysis

### Technical Summary

SKXamlCanvas (the UWP implementation) extends the XAML Canvas panel and renders its content by setting Background to an ImageBrush backed by a WriteableBitmap. Because no XAML children are added and no OnCreateAutomationPeer() override or AutomationProperties are implemented, accessibility tools see an empty panel with no accessible content. Accessibility support was never built for this control.

### Rationale

The reported behavior (no accessibility node) is consistent with the implementation: a Canvas with a background brush and no automation peer override will not expose meaningful accessibility data. This is not a regression or crash — accessibility support was never implemented for SKCanvasView on UWP. The appropriate classification is type/enhancement since this represents missing desired functionality.

### Key Signals

- "the SVG image did not appear [in accessibility data]" — **issue body** (The XAML accessibility tree does not include the SKCanvasView node, confirming no automation peer is registered.)
- "I used SKCanvasView as the base class in a custom control" — **issue body** (Subclassing SKCanvasView does not help without a custom automation peer at the library level.)

### Code Investigation

| File | Lines | Relevance | Finding |
|------|-------|-----------|---------|
| `source/SkiaSharp.Views/SkiaSharp.Views.WinUI/SKXamlCanvas.cs` | 26-68 | direct | SKXamlCanvas extends Canvas. No OnCreateAutomationPeer() override is present. No AutomationProperties are set. The entire drawing output is rendered by setting Background to an ImageBrush wrapping a WriteableBitmap — making the control content invisible to accessibility tools. |
| `source/SkiaSharp.Views/SkiaSharp.Views.WinUI/SKXamlCanvas.cs` | 237-254 | direct | CreateBitmap() creates a WriteableBitmap and assigns it as Background via an ImageBrush. There are no XAML child elements added to the Canvas, so accessibility tools see an empty panel with no accessible role or name. |

### Workarounds

- Set AutomationProperties.Name on the custom control subclass: AutomationProperties.SetName(this, "Logo image");
- Set AutomationProperties.IsInAccessibleTree to true explicitly on the subclass.
- Override OnCreateAutomationPeer() in the subclass to return a custom FrameworkElementAutomationPeer with an accessible name.

### Next Questions

- Is accessibility support for UWP/WinUI views on the roadmap?
- Should SkiaSharp expose an AccessibleName property on SKXamlCanvas and SKSwapChainPanel?

### Resolution Proposals

**Hypothesis:** No automation peer is implemented for SKXamlCanvas, so the control is invisible to accessibility tools. Adding AutomationProperties support or a custom peer would fix this.

1. **Set AutomationProperties on the custom subclass (user workaround)** — workaround, confidence 0.80 (80%), cost/xs, validated=untested
   - Users can subclass SKCanvasView and set AutomationProperties.Name and AutomationProperties.IsInAccessibleTree = true on their control to make it visible in the accessibility tree.
2. **Override OnCreateAutomationPeer in SKXamlCanvas** — fix, confidence 0.75 (75%), cost/m, validated=untested
   - Add an OnCreateAutomationPeer() override in SKXamlCanvas that returns a FrameworkElementAutomationPeer, allowing accessibility tools to discover the control. A Name property could be exposed for accessible label support.

**Recommended proposal:** Set AutomationProperties on the custom subclass (user workaround)

**Why:** Shortest path to unblocking the reporter. Library-level fix is also valid but requires UWP/WinUI automation peer expertise and design discussion.

## Recommendations

### Actionability

| Field | Value |
|-------|-------|
| Suggested action | keep-open |
| Confidence | 0.80 (80%) |
| Reason | Valid enhancement request — UWP accessibility support (automation peer) was never implemented for SKXamlCanvas. A workaround exists using AutomationProperties. |
| Suggested repro platform | windows |

### Automatable Actions

| Type | Risk | Confidence | Description | Details |
|------|------|------------|-------------|---------|
| update-labels | low | 0.90 (90%) | Apply enhancement, views, and UWP platform labels | labels=type/enhancement, area/SkiaSharp.Views, os/Windows-Universal-UWP, tenet/compatibility |
| add-comment | medium | 0.80 (80%) | Explain the root cause and provide a workaround using AutomationProperties | — |

**Comment draft for `add-comment`:**

```markdown
Thanks for the report and sample project!

The root cause is that `SKXamlCanvas` (the UWP backing control) renders its content by setting a `WriteableBitmap` as the `Background` brush on a XAML `Canvas` panel. No XAML children are added and no custom automation peer is implemented, so accessibility tools like Inspect see an empty, unnamed panel.

As a workaround until this is addressed at the library level, you can make your custom control visible to accessibility tools by setting `AutomationProperties` on it:

```csharp
// In your custom control's constructor:
AutomationProperties.SetName(this, "Your accessible name here");
AutomationProperties.SetIsInAccessibleTree(this, true);
```

Or in XAML:
```xml
<local:SVGImage AutomationProperties.Name="Logo image" />
```

For a more complete solution, you could override `OnCreateAutomationPeer()` in your subclass to return a `FrameworkElementAutomationPeer`. We're tracking library-level accessibility support for SKCanvasView on UWP as an enhancement.
```

<details>
<summary>Raw JSON</summary>

```json
{
  "meta": {
    "schemaVersion": "1.0",
    "number": 1008,
    "repo": "mono/SkiaSharp",
    "analyzedAt": "2026-05-03T00:05:00Z"
  },
  "summary": "SKCanvasView (UWP/Xamarin.Forms) does not appear in the accessibility tree because it renders via a WriteableBitmap background brush on a Canvas with no accessible children or automation properties.",
  "classification": {
    "type": {
      "value": "type/enhancement",
      "confidence": 0.85
    },
    "area": {
      "value": "area/SkiaSharp.Views",
      "confidence": 0.9
    },
    "platforms": [
      "os/Windows-Universal-UWP"
    ],
    "tenets": [
      "tenet/compatibility"
    ]
  },
  "evidence": {
    "reproEvidence": {
      "stepsToReproduce": [
        "Create a Xamarin.Forms UWP app using SKCanvasView as base class for a custom control",
        "Display SVG content via SkiaSharp rendering",
        "Open Accessibility Inspect tool and observe the visual tree",
        "Notice the SKCanvasView node does not appear in accessibility data"
      ],
      "screenshots": [
        {
          "url": "https://user-images.githubusercontent.com/13911774/68713980-e031b580-0553-11ea-91cd-2224f6dd3f98.png",
          "description": "Accessibility Inspect showing surrounding text elements visible but SKCanvasView SVG image node absent"
        }
      ],
      "attachments": [
        {
          "url": "https://github.com/mono/SkiaSharp/files/3838377/SkiaSharpBug.zip",
          "filename": "SkiaSharpBug.zip",
          "description": "Sample Xamarin.Forms UWP application demonstrating the accessibility gap"
        }
      ],
      "environmentDetails": "SkiaSharp 1.68.1-rc.165, Xamarin.Forms, UWP MinVersion 17134 (Win10 1803), VS 2019, desktop computer"
    },
    "versionAnalysis": {
      "mentionedVersions": [
        "1.68.1-rc.165"
      ],
      "currentRelevance": "likely",
      "relevanceReason": "SKXamlCanvas still extends Canvas and renders via WriteableBitmap ImageBrush with no accessibility API support — the same architecture exists in the current codebase."
    }
  },
  "analysis": {
    "summary": "SKXamlCanvas (the UWP implementation) extends the XAML Canvas panel and renders its content by setting Background to an ImageBrush backed by a WriteableBitmap. Because no XAML children are added and no OnCreateAutomationPeer() override or AutomationProperties are implemented, accessibility tools see an empty panel with no accessible content. Accessibility support was never built for this control.",
    "rationale": "The reported behavior (no accessibility node) is consistent with the implementation: a Canvas with a background brush and no automation peer override will not expose meaningful accessibility data. This is not a regression or crash — accessibility support was never implemented for SKCanvasView on UWP. The appropriate classification is type/enhancement since this represents missing desired functionality.",
    "keySignals": [
      {
        "text": "the SVG image did not appear [in accessibility data]",
        "source": "issue body",
        "interpretation": "The XAML accessibility tree does not include the SKCanvasView node, confirming no automation peer is registered."
      },
      {
        "text": "I used SKCanvasView as the base class in a custom control",
        "source": "issue body",
        "interpretation": "Subclassing SKCanvasView does not help without a custom automation peer at the library level."
      }
    ],
    "codeInvestigation": [
      {
        "file": "source/SkiaSharp.Views/SkiaSharp.Views.WinUI/SKXamlCanvas.cs",
        "lines": "26-68",
        "finding": "SKXamlCanvas extends Canvas. No OnCreateAutomationPeer() override is present. No AutomationProperties are set. The entire drawing output is rendered by setting Background to an ImageBrush wrapping a WriteableBitmap — making the control content invisible to accessibility tools.",
        "relevance": "direct"
      },
      {
        "file": "source/SkiaSharp.Views/SkiaSharp.Views.WinUI/SKXamlCanvas.cs",
        "lines": "237-254",
        "finding": "CreateBitmap() creates a WriteableBitmap and assigns it as Background via an ImageBrush. There are no XAML child elements added to the Canvas, so accessibility tools see an empty panel with no accessible role or name.",
        "relevance": "direct"
      }
    ],
    "workarounds": [
      "Set AutomationProperties.Name on the custom control subclass: AutomationProperties.SetName(this, \"Logo image\");",
      "Set AutomationProperties.IsInAccessibleTree to true explicitly on the subclass.",
      "Override OnCreateAutomationPeer() in the subclass to return a custom FrameworkElementAutomationPeer with an accessible name."
    ],
    "nextQuestions": [
      "Is accessibility support for UWP/WinUI views on the roadmap?",
      "Should SkiaSharp expose an AccessibleName property on SKXamlCanvas and SKSwapChainPanel?"
    ],
    "resolution": {
      "hypothesis": "No automation peer is implemented for SKXamlCanvas, so the control is invisible to accessibility tools. Adding AutomationProperties support or a custom peer would fix this.",
      "proposals": [
        {
          "title": "Set AutomationProperties on the custom subclass (user workaround)",
          "description": "Users can subclass SKCanvasView and set AutomationProperties.Name and AutomationProperties.IsInAccessibleTree = true on their control to make it visible in the accessibility tree.",
          "category": "workaround",
          "confidence": 0.8,
          "effort": "cost/xs",
          "validated": "untested"
        },
        {
          "title": "Override OnCreateAutomationPeer in SKXamlCanvas",
          "description": "Add an OnCreateAutomationPeer() override in SKXamlCanvas that returns a FrameworkElementAutomationPeer, allowing accessibility tools to discover the control. A Name property could be exposed for accessible label support.",
          "category": "fix",
          "confidence": 0.75,
          "effort": "cost/m",
          "validated": "untested"
        }
      ],
      "recommendedProposal": "Set AutomationProperties on the custom subclass (user workaround)",
      "recommendedReason": "Shortest path to unblocking the reporter. Library-level fix is also valid but requires UWP/WinUI automation peer expertise and design discussion."
    }
  },
  "output": {
    "actionability": {
      "suggestedAction": "keep-open",
      "confidence": 0.8,
      "reason": "Valid enhancement request — UWP accessibility support (automation peer) was never implemented for SKXamlCanvas. A workaround exists using AutomationProperties.",
      "suggestedReproPlatform": "windows"
    },
    "actions": [
      {
        "type": "update-labels",
        "description": "Apply enhancement, views, and UWP platform labels",
        "risk": "low",
        "confidence": 0.9,
        "labels": [
          "type/enhancement",
          "area/SkiaSharp.Views",
          "os/Windows-Universal-UWP",
          "tenet/compatibility"
        ]
      },
      {
        "type": "add-comment",
        "description": "Explain the root cause and provide a workaround using AutomationProperties",
        "risk": "medium",
        "confidence": 0.8,
        "comment": "Thanks for the report and sample project!\n\nThe root cause is that `SKXamlCanvas` (the UWP backing control) renders its content by setting a `WriteableBitmap` as the `Background` brush on a XAML `Canvas` panel. No XAML children are added and no custom automation peer is implemented, so accessibility tools like Inspect see an empty, unnamed panel.\n\nAs a workaround until this is addressed at the library level, you can make your custom control visible to accessibility tools by setting `AutomationProperties` on it:\n\n```csharp\n// In your custom control's constructor:\nAutomationProperties.SetName(this, \"Your accessible name here\");\nAutomationProperties.SetIsInAccessibleTree(this, true);\n```\n\nOr in XAML:\n```xml\n<local:SVGImage AutomationProperties.Name=\"Logo image\" />\n```\n\nFor a more complete solution, you could override `OnCreateAutomationPeer()` in your subclass to return a `FrameworkElementAutomationPeer`. We're tracking library-level accessibility support for SKCanvasView on UWP as an enhancement."
      }
    ]
  }
}
```

</details>
