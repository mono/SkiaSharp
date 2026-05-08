# Issue Triage Report — #3148

| Field | Value |
|-------|-------|
| Repository | mono/SkiaSharp |
| Analyzed | 2026-05-02T02:17:00Z |
| Type | type/bug (0.92 (92%)) |
| Area | area/SkiaSharp.Views.Uno (0.90 (90%)) |
| Suggested action | needs-investigation (0.82 (82%)) |

**Issue Summary:** ManipulationDelta.Scale is always 1 when using SKSwapChainPanel on iOS with Uno Platform; SKXamlCanvas works correctly on all platforms.

**Analysis:** SKSwapChainPanel on iOS adds an SKGLView as a native UIKit subview via AddSubview(glView). This native UIView subview captures touch events before the Uno WinUI manipulation layer can process them, causing ManipulationDelta.Scale to always report 1. SKXamlCanvas avoids this by drawing directly on the parent UIView without adding a native subview, allowing touch events to reach the Uno gesture recognizer stack unobstructed.

**Recommendations:** **needs-investigation** — Real platform-specific bug in Uno iOS SKSwapChainPanel touch event handling. Root cause hypothesis is clear (native subview intercepts touches) but fix needs iOS device testing to confirm glView.UserInteractionEnabled = false is sufficient.

---

## Classification

| Field | Value |
|-------|-------|
| Type | type/bug |
| Area | area/SkiaSharp.Views.Uno |
| Platforms | os/iOS |
| Backends | backend/OpenGL |
| Tenets | — |
| Partner | partner/unoplatform |
| Current labels | type/bug |

## Evidence

### Reproduction

1. Create a Uno Platform WinUI app with SKSwapChainPanel on iOS
2. Set ManipulationMode to include Scale on the parent page
3. Perform a pinch gesture on the device
4. Observe that e.Delta.Scale is always 1, never changes

**Environment:** SkiaSharp 2.88.9, iOS 18.0-18.3, Visual Studio (Windows), Uno Platform

**Repository links:**
- https://github.com/user-attachments/files/18601322/SKSwapChainPanelIOS.zip — Repro project zip based on SkiaSharp Uno platform sample

### Bug Signals

| Field | Value |
|-------|-------|
| Severity | medium |
| Regression claimed | False |
| Error type | wrong-output |
| Error message | e.Delta.Scale is always 1 when ManipulationDelta fires on a page that contains SKSwapChainPanel |
| Repro quality | partial |
| Target frameworks | net8.0-ios |

### Version Analysis

| Field | Value |
|-------|-------|
| Mentioned versions | 2.88.9 |
| Worked in | — |
| Broke in | — |
| Current relevance | likely |
| Relevance reason | The iOS SKSwapChainPanel implementation using AddSubview(glView) pattern has not changed; the native subview touch event capture issue persists. |

## Analysis

### Technical Summary

SKSwapChainPanel on iOS adds an SKGLView as a native UIKit subview via AddSubview(glView). This native UIView subview captures touch events before the Uno WinUI manipulation layer can process them, causing ManipulationDelta.Scale to always report 1. SKXamlCanvas avoids this by drawing directly on the parent UIView without adding a native subview, allowing touch events to reach the Uno gesture recognizer stack unobstructed.

### Rationale

Clear wrong-output bug. The reporter correctly identifies that SKXamlCanvas works and SKSwapChainPanel does not. Code investigation confirms that SKSwapChainPanel.iOS.cs inserts a native SKGLView subview that intercepts iOS touch events, breaking Uno's manipulation event pipeline. No user error — the API is correct but the implementation causes gesture event loss.

### Key Signals

- "every ManipulationDelta of the Scale is 1" — **issue body** (Pinch gesture data is not reaching the Uno WinUI event pipeline — the underlying touches are consumed by the native subview.)
- "If SKXamlCanvas is used everything works just fine on every platform" — **issue body** (The difference in iOS implementation between SKSwapChainPanel and SKXamlCanvas is the root cause — SKXamlCanvas does not add a native subview.)
- "Even though the event is not registered directly on SKSwapChainPanel the e.Delta.Scale is 1" — **issue body** (The event is on the parent Page, yet the SKSwapChainPanel's inner native view still intercepts touches.)

### Code Investigation

| File | Lines | Relevance | Finding |
|------|-------|-----------|---------|
| `source/SkiaSharp.Views.Uno/SkiaSharp.Views.Uno.WinUI/SKSwapChainPanel.iOS.cs` | 36-41 | direct | DoLoaded() creates an SKGLView and calls AddSubview(glView), placing a native UIKit view as a child. This native view intercepts raw touch events on iOS, preventing them from reaching the Uno WinUI manipulation gesture recognizer on the parent FrameworkElement. |
| `source/SkiaSharp.Views.Uno/SkiaSharp.Views.Uno.WinUI/SKXamlCanvas.AppleiOS.cs` | 1-88 | related | SKXamlCanvas on iOS inherits UIView directly and draws via Draw(CGRect), without adding any native subview. Touch events therefore propagate normally through the Uno hit-test tree, explaining why ManipulationDelta works correctly with SKXamlCanvas. |

### Workarounds

- Use SKXamlCanvas instead of SKSwapChainPanel when gesture/manipulation support is needed on iOS (software rendering fallback).
- Register gesture recognizers at the UIWindow or UIViewController level instead of on the Uno Page element.

### Next Questions

- Does setting glView.UserInteractionEnabled = false fix the touch pass-through on iOS?
- Is the same issue present on macOS Catalyst (SKSwapChainPanel.MacCatalyst.cs uses a similar pattern)?
- Which version of Uno Platform is the reporter using?

### Resolution Proposals

**Hypothesis:** The SKGLView added by SKSwapChainPanel.iOS.cs intercepts touch events. Setting glView.UserInteractionEnabled = false should allow touches to pass through to the Uno gesture layer while still rendering correctly.

1. **Disable user interaction on inner SKGLView** — fix, confidence 0.75 (75%), cost/xs, validated=untested
   - After AddSubview(glView), set glView.UserInteractionEnabled = false so touches pass through to the Uno hit-test and manipulation recognizer stack.
2. **Use SKXamlCanvas as workaround** — workaround, confidence 0.95 (95%), cost/xs, validated=untested
   - Replace SKSwapChainPanel with SKXamlCanvas for any iOS scenario requiring ManipulationDelta gesture support. SKXamlCanvas uses software rendering but does not block touch events.

**Recommended proposal:** Disable user interaction on inner SKGLView

**Why:** Single-line fix that preserves hardware acceleration while restoring gesture event delivery.

## Recommendations

### Actionability

| Field | Value |
|-------|-------|
| Suggested action | needs-investigation |
| Confidence | 0.82 (82%) |
| Reason | Real platform-specific bug in Uno iOS SKSwapChainPanel touch event handling. Root cause hypothesis is clear (native subview intercepts touches) but fix needs iOS device testing to confirm glView.UserInteractionEnabled = false is sufficient. |
| Suggested repro platform | macos |

### Automatable Actions

| Type | Risk | Confidence | Description | Details |
|------|------|------------|-------------|---------|
| update-labels | low | 0.92 (92%) | Apply bug, Uno views, iOS, OpenGL, and Uno partner labels | labels=type/bug, area/SkiaSharp.Views.Uno, os/iOS, backend/OpenGL, partner/unoplatform |
| add-comment | medium | 0.82 (82%) | Explain root cause and suggest workaround/fix direction | — |

**Comment draft for `add-comment`:**

```markdown
Thanks for the detailed repro! This looks like a touch event interception issue specific to the iOS implementation of `SKSwapChainPanel`.

On iOS, `SKSwapChainPanel` adds a native `SKGLView` (a `UIView` subclass) as a subview. This native view captures raw touch events before Uno's manipulation gesture recognizer can process them, causing `ManipulationDelta.Scale` to always be `1`. `SKXamlCanvas` avoids this by rendering directly on the parent `UIView` without inserting a native child view.

**Workaround:** Use `SKXamlCanvas` instead of `SKSwapChainPanel` when you need `ManipulationDelta` gesture support on iOS. This uses software rendering but will correctly pass through touch events.

**Potential fix:** Setting `glView.UserInteractionEnabled = false` on the inner `SKGLView` in the iOS implementation may allow touches to pass through to the Uno gesture layer. We'll investigate this path.

Could you also confirm which version of Uno Platform you are using?
```

<details>
<summary>Raw JSON</summary>

```json
{
  "meta": {
    "schemaVersion": "1.0",
    "number": 3148,
    "repo": "mono/SkiaSharp",
    "analyzedAt": "2026-05-02T02:17:00Z",
    "currentLabels": [
      "type/bug"
    ]
  },
  "summary": "ManipulationDelta.Scale is always 1 when using SKSwapChainPanel on iOS with Uno Platform; SKXamlCanvas works correctly on all platforms.",
  "classification": {
    "type": {
      "value": "type/bug",
      "confidence": 0.92
    },
    "area": {
      "value": "area/SkiaSharp.Views.Uno",
      "confidence": 0.9
    },
    "platforms": [
      "os/iOS"
    ],
    "backends": [
      "backend/OpenGL"
    ],
    "partner": "partner/unoplatform"
  },
  "evidence": {
    "bugSignals": {
      "severity": "medium",
      "regressionClaimed": false,
      "errorType": "wrong-output",
      "errorMessage": "e.Delta.Scale is always 1 when ManipulationDelta fires on a page that contains SKSwapChainPanel",
      "reproQuality": "partial",
      "targetFrameworks": [
        "net8.0-ios"
      ]
    },
    "reproEvidence": {
      "stepsToReproduce": [
        "Create a Uno Platform WinUI app with SKSwapChainPanel on iOS",
        "Set ManipulationMode to include Scale on the parent page",
        "Perform a pinch gesture on the device",
        "Observe that e.Delta.Scale is always 1, never changes"
      ],
      "environmentDetails": "SkiaSharp 2.88.9, iOS 18.0-18.3, Visual Studio (Windows), Uno Platform",
      "repoLinks": [
        {
          "url": "https://github.com/user-attachments/files/18601322/SKSwapChainPanelIOS.zip",
          "description": "Repro project zip based on SkiaSharp Uno platform sample"
        }
      ]
    },
    "versionAnalysis": {
      "mentionedVersions": [
        "2.88.9"
      ],
      "currentRelevance": "likely",
      "relevanceReason": "The iOS SKSwapChainPanel implementation using AddSubview(glView) pattern has not changed; the native subview touch event capture issue persists."
    }
  },
  "analysis": {
    "summary": "SKSwapChainPanel on iOS adds an SKGLView as a native UIKit subview via AddSubview(glView). This native UIView subview captures touch events before the Uno WinUI manipulation layer can process them, causing ManipulationDelta.Scale to always report 1. SKXamlCanvas avoids this by drawing directly on the parent UIView without adding a native subview, allowing touch events to reach the Uno gesture recognizer stack unobstructed.",
    "rationale": "Clear wrong-output bug. The reporter correctly identifies that SKXamlCanvas works and SKSwapChainPanel does not. Code investigation confirms that SKSwapChainPanel.iOS.cs inserts a native SKGLView subview that intercepts iOS touch events, breaking Uno's manipulation event pipeline. No user error — the API is correct but the implementation causes gesture event loss.",
    "keySignals": [
      {
        "text": "every ManipulationDelta of the Scale is 1",
        "source": "issue body",
        "interpretation": "Pinch gesture data is not reaching the Uno WinUI event pipeline — the underlying touches are consumed by the native subview."
      },
      {
        "text": "If SKXamlCanvas is used everything works just fine on every platform",
        "source": "issue body",
        "interpretation": "The difference in iOS implementation between SKSwapChainPanel and SKXamlCanvas is the root cause — SKXamlCanvas does not add a native subview."
      },
      {
        "text": "Even though the event is not registered directly on SKSwapChainPanel the e.Delta.Scale is 1",
        "source": "issue body",
        "interpretation": "The event is on the parent Page, yet the SKSwapChainPanel's inner native view still intercepts touches."
      }
    ],
    "codeInvestigation": [
      {
        "file": "source/SkiaSharp.Views.Uno/SkiaSharp.Views.Uno.WinUI/SKSwapChainPanel.iOS.cs",
        "lines": "36-41",
        "finding": "DoLoaded() creates an SKGLView and calls AddSubview(glView), placing a native UIKit view as a child. This native view intercepts raw touch events on iOS, preventing them from reaching the Uno WinUI manipulation gesture recognizer on the parent FrameworkElement.",
        "relevance": "direct"
      },
      {
        "file": "source/SkiaSharp.Views.Uno/SkiaSharp.Views.Uno.WinUI/SKXamlCanvas.AppleiOS.cs",
        "lines": "1-88",
        "finding": "SKXamlCanvas on iOS inherits UIView directly and draws via Draw(CGRect), without adding any native subview. Touch events therefore propagate normally through the Uno hit-test tree, explaining why ManipulationDelta works correctly with SKXamlCanvas.",
        "relevance": "related"
      }
    ],
    "nextQuestions": [
      "Does setting glView.UserInteractionEnabled = false fix the touch pass-through on iOS?",
      "Is the same issue present on macOS Catalyst (SKSwapChainPanel.MacCatalyst.cs uses a similar pattern)?",
      "Which version of Uno Platform is the reporter using?"
    ],
    "workarounds": [
      "Use SKXamlCanvas instead of SKSwapChainPanel when gesture/manipulation support is needed on iOS (software rendering fallback).",
      "Register gesture recognizers at the UIWindow or UIViewController level instead of on the Uno Page element."
    ],
    "resolution": {
      "hypothesis": "The SKGLView added by SKSwapChainPanel.iOS.cs intercepts touch events. Setting glView.UserInteractionEnabled = false should allow touches to pass through to the Uno gesture layer while still rendering correctly.",
      "proposals": [
        {
          "title": "Disable user interaction on inner SKGLView",
          "description": "After AddSubview(glView), set glView.UserInteractionEnabled = false so touches pass through to the Uno hit-test and manipulation recognizer stack.",
          "category": "fix",
          "confidence": 0.75,
          "effort": "cost/xs",
          "validated": "untested"
        },
        {
          "title": "Use SKXamlCanvas as workaround",
          "description": "Replace SKSwapChainPanel with SKXamlCanvas for any iOS scenario requiring ManipulationDelta gesture support. SKXamlCanvas uses software rendering but does not block touch events.",
          "category": "workaround",
          "confidence": 0.95,
          "effort": "cost/xs",
          "validated": "untested"
        }
      ],
      "recommendedProposal": "Disable user interaction on inner SKGLView",
      "recommendedReason": "Single-line fix that preserves hardware acceleration while restoring gesture event delivery."
    }
  },
  "output": {
    "actionability": {
      "suggestedAction": "needs-investigation",
      "confidence": 0.82,
      "reason": "Real platform-specific bug in Uno iOS SKSwapChainPanel touch event handling. Root cause hypothesis is clear (native subview intercepts touches) but fix needs iOS device testing to confirm glView.UserInteractionEnabled = false is sufficient.",
      "suggestedReproPlatform": "macos"
    },
    "actions": [
      {
        "type": "update-labels",
        "description": "Apply bug, Uno views, iOS, OpenGL, and Uno partner labels",
        "risk": "low",
        "confidence": 0.92,
        "labels": [
          "type/bug",
          "area/SkiaSharp.Views.Uno",
          "os/iOS",
          "backend/OpenGL",
          "partner/unoplatform"
        ]
      },
      {
        "type": "add-comment",
        "description": "Explain root cause and suggest workaround/fix direction",
        "risk": "medium",
        "confidence": 0.82,
        "comment": "Thanks for the detailed repro! This looks like a touch event interception issue specific to the iOS implementation of `SKSwapChainPanel`.\n\nOn iOS, `SKSwapChainPanel` adds a native `SKGLView` (a `UIView` subclass) as a subview. This native view captures raw touch events before Uno's manipulation gesture recognizer can process them, causing `ManipulationDelta.Scale` to always be `1`. `SKXamlCanvas` avoids this by rendering directly on the parent `UIView` without inserting a native child view.\n\n**Workaround:** Use `SKXamlCanvas` instead of `SKSwapChainPanel` when you need `ManipulationDelta` gesture support on iOS. This uses software rendering but will correctly pass through touch events.\n\n**Potential fix:** Setting `glView.UserInteractionEnabled = false` on the inner `SKGLView` in the iOS implementation may allow touches to pass through to the Uno gesture layer. We'll investigate this path.\n\nCould you also confirm which version of Uno Platform you are using?"
      }
    ]
  }
}
```

</details>
