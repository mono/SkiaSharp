# Issue Triage Report — #730

| Field | Value |
|-------|-------|
| Repository | mono/SkiaSharp |
| Analyzed | 2026-04-27T09:04:33Z |
| Type | type/bug (0.95 (95%)) |
| Area | area/SkiaSharp.Views (0.92 (92%)) |
| Suggested action | needs-investigation (0.80 (80%)) |

**Issue Summary:** SKGLView on UWP crashes with 'Failed to create EGL surface' when ViewExtensions.RotateYTo is called on a parent Grid, triggering CompositionScaleChanged on AngleSwapChainPanel during animation.

**Analysis:** RotateYTo animation on a parent Grid triggers CompositionScaleChanged on AngleSwapChainPanel, causing EnsureRenderSurface to call CreateSurface while the panel is in a transitional animation state. eglCreateWindowSurface returns EGL_NO_SURFACE and the code throws 'Failed to create EGL surface' with no recovery. Related issue #1573 (same stack trace, page-navigation trigger) was closed as completed in March 2021.

**Recommendations:** **needs-investigation** — Related issue #1573 with the identical stack trace was closed as completed, but this issue uses a different trigger (RotateYTo animation) and remains open. Investigation is needed to confirm whether the existing fix covers this scenario.

---

## Classification

| Field | Value |
|-------|-------|
| Type | type/bug |
| Area | area/SkiaSharp.Views |
| Platforms | os/Windows-Universal-UWP |
| Backends | backend/OpenGL |
| Tenets | tenet/reliability |
| Partner | — |
| Current labels | type/bug, os/Windows-Universal-UWP, area/SkiaSharp.Views, area/SkiaSharp.Views.Forms, backend/OpenGL |

## Evidence

### Reproduction

1. Create a Xamarin.Forms UWP app with a Grid containing an SKGLView
2. Call ViewExtensions.RotateYTo(grid, 180, 300) several times, alternating from 180 to 0
3. Observe the unhandled 'Failed to create EGL surface' exception, usually on the first try

**Environment:** Xamarin.Forms UWP, SkiaSharp 1.68.0-preview28, VS2017

**Related issues:** #1573, #705

**Repository links:**
- https://devdiv.visualstudio.com/DevDiv/_workitems/edit/752323 — Internal VS bug #752323 filed by reporter
- https://github.com/mono/SkiaSharp/issues/1573 — Related issue: same stack trace on UWP page navigation, closed as completed March 2021

### Bug Signals

| Field | Value |
|-------|-------|
| Severity | high |
| Regression claimed | False |
| Error type | exception |
| Error message | System.Exception: Failed to create EGL surface at SkiaSharp.Views.GlesInterop.GlesContext.CreateSurface |
| Repro quality | partial |
| Target frameworks | — |

**Stack trace:**

```text
System.Exception: Failed to create EGL surface
   at SkiaSharp.Views.GlesInterop.GlesContext.CreateSurface(SwapChainPanel panel, Nullable`1 renderSurfaceSize, Nullable`1 resolutionScale)
   at SkiaSharp.Views.UWP.AngleSwapChainPanel.EnsureRenderSurface()
   at SkiaSharp.Views.UWP.AngleSwapChainPanel.OnCompositionChanged(SwapChainPanel sender, Object args)
```

### Version Analysis

| Field | Value |
|-------|-------|
| Mentioned versions | 1.68.0-preview28 |
| Worked in | — |
| Broke in | — |
| Current relevance | unknown |
| Relevance reason | Filed against a preview version from 2018. Related issue #1573 with the identical stack trace was closed as 'completed' in March 2021; the fix may also cover the RotateYTo trigger, but this has not been verified. |

### Fix Status

| Field | Value |
|-------|-------|
| Likely fixed | True |
| Confidence | 0.55 (55%) |
| Reason | Issue #1573 reports the exact same exception via the same code path (GlesContext.CreateSurface → EnsureRenderSurface → OnCompositionChanged) and was closed as completed. The underlying fix likely guards this path, but the RotateYTo animation trigger was not explicitly tested. |
| Related PRs | — |
| Related commits | — |
| Fixed in version | — |

## Analysis

### Technical Summary

RotateYTo animation on a parent Grid triggers CompositionScaleChanged on AngleSwapChainPanel, causing EnsureRenderSurface to call CreateSurface while the panel is in a transitional animation state. eglCreateWindowSurface returns EGL_NO_SURFACE and the code throws 'Failed to create EGL surface' with no recovery. Related issue #1573 (same stack trace, page-navigation trigger) was closed as completed in March 2021.

### Rationale

The stack trace unambiguously identifies the failure site: eglCreateWindowSurface returns EGL_NO_SURFACE during a composition-scale-changed event fired while a RotateY animation is in progress. The issue is UWP/ANGLE-specific and a timing/state-dependent crash. This is a real bug. Related issue #1573 shows the same crash path was already addressed; this issue needs verification against the current codebase.

### Key Signals

- "System.Exception: Failed to create EGL surface at GlesContext.CreateSurface at AngleSwapChainPanel.EnsureRenderSurface at AngleSwapChainPanel.OnCompositionChanged" — **issue body** (eglCreateWindowSurface returns EGL_NO_SURFACE — ANGLE could not create the surface during the animation, and the code throws immediately with no retry or recovery.)
- "It will crash, usually on the first try. Sometimes it succeeds but always crashes within a few tries." — **issue body** (Non-deterministic failure indicates a timing or race condition: the CompositionScaleChanged event fires at varying points in the animation where the panel may or may not be in a valid state for surface creation.)
- "I opened #1573 in case it was a distinct root cause, and to provide more details." — **comment by gmurray81, 2021-01-04** (A second reporter confirmed the same exception with the same stack trace under a different trigger (page navigation), pointing to a shared underlying cause in AngleSwapChainPanel.)

### Code Investigation

| File | Lines | Relevance | Finding |
|------|-------|-----------|---------|
| `source/SkiaSharp.Views/SkiaSharp.Views.WinUI/AngleSwapChainPanel.cs` | 163-180 | direct | OnCompositionChanged checks if scale actually changed, then calls DestroyRenderSurface() followed by EnsureRenderSurface(). During RotateYTo animation, CompositionScaleChanged may fire while ANGLE's internal state is inconsistent, causing the subsequent CreateSurface call to fail. |
| `source/SkiaSharp.Views/SkiaSharp.Views.WinUI/GlesInterop/GlesContext.cs` | 77-113 | direct | CreateSurface calls eglCreateWindowSurface and immediately throws 'Failed to create EGL surface' on EGL_NO_SURFACE with no retry or graceful degradation path. Any transient failure is a fatal unhandled exception. |
| `source/SkiaSharp.Views/SkiaSharp.Views.WinUI/AngleSwapChainPanel.cs` | 191-205 | related | EnsureRenderSurface guards with isLoaded, HasSurface, ActualWidth > 0, ActualHeight > 0. These guards do not protect against ANGLE refusing to create a surface during a 3D transform animation, where ActualWidth is still > 0 but the visual state is invalid for EGL. |

### Next Questions

- Does the fix applied for #1573 also prevent this crash when RotateYTo is used as the trigger?
- Does RotateYTo on a parent Grid actually change CompositionScaleX/Y, or is it triggering CompositionScaleChanged via an indirect layout mechanism?
- What EGL error code is returned when eglCreateWindowSurface fails — does it give more diagnostic information?

### Resolution Proposals

**Hypothesis:** The RotateYTo animation on the parent Grid causes the UWP composition system to fire CompositionScaleChanged on the SwapChainPanel. When EnsureRenderSurface is called in response, ANGLE's eglCreateWindowSurface fails because the panel is in a transitional 3D transform state. The fix for #1573 likely added guards that also prevent this, but needs verification.

1. **Verify the #1573 fix covers the RotateYTo scenario** — investigation, confidence 0.80 (80%), cost/xs, validated=untested
   - Reproduce the RotateYTo crash against the current SkiaSharp release. If it no longer crashes, close #730 as fixed. If it still crashes, the EGL surface creation path needs additional error handling.
2. **Swallow transient EGL surface creation failures with retry** — fix, confidence 0.65 (65%), cost/s, validated=untested
   - Wrap eglCreateWindowSurface in EnsureRenderSurface with error handling: log and schedule a retry rather than throwing. This handles transient unavailability during animations and navigation without crashing.

**Recommended proposal:** Verify the #1573 fix covers the RotateYTo scenario

**Why:** Least invasive — confirm whether a fix is already in place before adding new code.

## Recommendations

### Actionability

| Field | Value |
|-------|-------|
| Suggested action | needs-investigation |
| Confidence | 0.80 (80%) |
| Reason | Related issue #1573 with the identical stack trace was closed as completed, but this issue uses a different trigger (RotateYTo animation) and remains open. Investigation is needed to confirm whether the existing fix covers this scenario. |
| Suggested repro platform | windows |

### Automatable Actions

| Type | Risk | Confidence | Description | Details |
|------|------|------------|-------------|---------|
| update-labels | low | 0.92 (92%) | Apply type/bug, area/SkiaSharp.Views, os/Windows-Universal-UWP, backend/OpenGL, tenet/reliability labels | labels=type/bug, area/SkiaSharp.Views, os/Windows-Universal-UWP, backend/OpenGL, tenet/reliability |
| link-related | low | 0.92 (92%) | Cross-reference #1573 (same stack trace, different trigger, closed as completed) | linkedIssue=#1573 |
| add-comment | medium | 0.80 (80%) | Note the relationship to #1573 and ask reporter to verify with current release | — |

**Comment draft for `add-comment`:**

```markdown
Thanks for the detailed report. This crash shares the same stack trace as #1573, which was fixed and closed in March 2021. Could you test with the latest stable SkiaSharp release to confirm whether the RotateYTo scenario is also resolved? If the crash still occurs, please share the exact SkiaSharp version and a minimal repro project.
```

<details>
<summary>Raw JSON</summary>

```json
{
  "meta": {
    "schemaVersion": "1.0",
    "number": 730,
    "repo": "mono/SkiaSharp",
    "analyzedAt": "2026-04-27T09:04:33Z",
    "currentLabels": [
      "type/bug",
      "os/Windows-Universal-UWP",
      "area/SkiaSharp.Views",
      "area/SkiaSharp.Views.Forms",
      "backend/OpenGL"
    ]
  },
  "summary": "SKGLView on UWP crashes with 'Failed to create EGL surface' when ViewExtensions.RotateYTo is called on a parent Grid, triggering CompositionScaleChanged on AngleSwapChainPanel during animation.",
  "classification": {
    "type": {
      "value": "type/bug",
      "confidence": 0.95
    },
    "area": {
      "value": "area/SkiaSharp.Views",
      "confidence": 0.92
    },
    "platforms": [
      "os/Windows-Universal-UWP"
    ],
    "backends": [
      "backend/OpenGL"
    ],
    "tenets": [
      "tenet/reliability"
    ]
  },
  "evidence": {
    "bugSignals": {
      "severity": "high",
      "regressionClaimed": false,
      "errorType": "exception",
      "errorMessage": "System.Exception: Failed to create EGL surface at SkiaSharp.Views.GlesInterop.GlesContext.CreateSurface",
      "stackTrace": "System.Exception: Failed to create EGL surface\n   at SkiaSharp.Views.GlesInterop.GlesContext.CreateSurface(SwapChainPanel panel, Nullable`1 renderSurfaceSize, Nullable`1 resolutionScale)\n   at SkiaSharp.Views.UWP.AngleSwapChainPanel.EnsureRenderSurface()\n   at SkiaSharp.Views.UWP.AngleSwapChainPanel.OnCompositionChanged(SwapChainPanel sender, Object args)",
      "reproQuality": "partial"
    },
    "reproEvidence": {
      "stepsToReproduce": [
        "Create a Xamarin.Forms UWP app with a Grid containing an SKGLView",
        "Call ViewExtensions.RotateYTo(grid, 180, 300) several times, alternating from 180 to 0",
        "Observe the unhandled 'Failed to create EGL surface' exception, usually on the first try"
      ],
      "environmentDetails": "Xamarin.Forms UWP, SkiaSharp 1.68.0-preview28, VS2017",
      "relatedIssues": [
        1573,
        705
      ],
      "repoLinks": [
        {
          "url": "https://devdiv.visualstudio.com/DevDiv/_workitems/edit/752323",
          "description": "Internal VS bug #752323 filed by reporter"
        },
        {
          "url": "https://github.com/mono/SkiaSharp/issues/1573",
          "description": "Related issue: same stack trace on UWP page navigation, closed as completed March 2021"
        }
      ]
    },
    "versionAnalysis": {
      "mentionedVersions": [
        "1.68.0-preview28"
      ],
      "currentRelevance": "unknown",
      "relevanceReason": "Filed against a preview version from 2018. Related issue #1573 with the identical stack trace was closed as 'completed' in March 2021; the fix may also cover the RotateYTo trigger, but this has not been verified."
    },
    "fixStatus": {
      "likelyFixed": true,
      "confidence": 0.55,
      "reason": "Issue #1573 reports the exact same exception via the same code path (GlesContext.CreateSurface → EnsureRenderSurface → OnCompositionChanged) and was closed as completed. The underlying fix likely guards this path, but the RotateYTo animation trigger was not explicitly tested.",
      "relatedPRs": []
    }
  },
  "analysis": {
    "summary": "RotateYTo animation on a parent Grid triggers CompositionScaleChanged on AngleSwapChainPanel, causing EnsureRenderSurface to call CreateSurface while the panel is in a transitional animation state. eglCreateWindowSurface returns EGL_NO_SURFACE and the code throws 'Failed to create EGL surface' with no recovery. Related issue #1573 (same stack trace, page-navigation trigger) was closed as completed in March 2021.",
    "rationale": "The stack trace unambiguously identifies the failure site: eglCreateWindowSurface returns EGL_NO_SURFACE during a composition-scale-changed event fired while a RotateY animation is in progress. The issue is UWP/ANGLE-specific and a timing/state-dependent crash. This is a real bug. Related issue #1573 shows the same crash path was already addressed; this issue needs verification against the current codebase.",
    "keySignals": [
      {
        "text": "System.Exception: Failed to create EGL surface at GlesContext.CreateSurface at AngleSwapChainPanel.EnsureRenderSurface at AngleSwapChainPanel.OnCompositionChanged",
        "source": "issue body",
        "interpretation": "eglCreateWindowSurface returns EGL_NO_SURFACE — ANGLE could not create the surface during the animation, and the code throws immediately with no retry or recovery."
      },
      {
        "text": "It will crash, usually on the first try. Sometimes it succeeds but always crashes within a few tries.",
        "source": "issue body",
        "interpretation": "Non-deterministic failure indicates a timing or race condition: the CompositionScaleChanged event fires at varying points in the animation where the panel may or may not be in a valid state for surface creation."
      },
      {
        "text": "I opened #1573 in case it was a distinct root cause, and to provide more details.",
        "source": "comment by gmurray81, 2021-01-04",
        "interpretation": "A second reporter confirmed the same exception with the same stack trace under a different trigger (page navigation), pointing to a shared underlying cause in AngleSwapChainPanel."
      }
    ],
    "codeInvestigation": [
      {
        "file": "source/SkiaSharp.Views/SkiaSharp.Views.WinUI/AngleSwapChainPanel.cs",
        "lines": "163-180",
        "finding": "OnCompositionChanged checks if scale actually changed, then calls DestroyRenderSurface() followed by EnsureRenderSurface(). During RotateYTo animation, CompositionScaleChanged may fire while ANGLE's internal state is inconsistent, causing the subsequent CreateSurface call to fail.",
        "relevance": "direct"
      },
      {
        "file": "source/SkiaSharp.Views/SkiaSharp.Views.WinUI/GlesInterop/GlesContext.cs",
        "lines": "77-113",
        "finding": "CreateSurface calls eglCreateWindowSurface and immediately throws 'Failed to create EGL surface' on EGL_NO_SURFACE with no retry or graceful degradation path. Any transient failure is a fatal unhandled exception.",
        "relevance": "direct"
      },
      {
        "file": "source/SkiaSharp.Views/SkiaSharp.Views.WinUI/AngleSwapChainPanel.cs",
        "lines": "191-205",
        "finding": "EnsureRenderSurface guards with isLoaded, HasSurface, ActualWidth > 0, ActualHeight > 0. These guards do not protect against ANGLE refusing to create a surface during a 3D transform animation, where ActualWidth is still > 0 but the visual state is invalid for EGL.",
        "relevance": "related"
      }
    ],
    "nextQuestions": [
      "Does the fix applied for #1573 also prevent this crash when RotateYTo is used as the trigger?",
      "Does RotateYTo on a parent Grid actually change CompositionScaleX/Y, or is it triggering CompositionScaleChanged via an indirect layout mechanism?",
      "What EGL error code is returned when eglCreateWindowSurface fails — does it give more diagnostic information?"
    ],
    "resolution": {
      "hypothesis": "The RotateYTo animation on the parent Grid causes the UWP composition system to fire CompositionScaleChanged on the SwapChainPanel. When EnsureRenderSurface is called in response, ANGLE's eglCreateWindowSurface fails because the panel is in a transitional 3D transform state. The fix for #1573 likely added guards that also prevent this, but needs verification.",
      "proposals": [
        {
          "title": "Verify the #1573 fix covers the RotateYTo scenario",
          "description": "Reproduce the RotateYTo crash against the current SkiaSharp release. If it no longer crashes, close #730 as fixed. If it still crashes, the EGL surface creation path needs additional error handling.",
          "category": "investigation",
          "confidence": 0.8,
          "effort": "cost/xs",
          "validated": "untested"
        },
        {
          "title": "Swallow transient EGL surface creation failures with retry",
          "description": "Wrap eglCreateWindowSurface in EnsureRenderSurface with error handling: log and schedule a retry rather than throwing. This handles transient unavailability during animations and navigation without crashing.",
          "category": "fix",
          "confidence": 0.65,
          "effort": "cost/s",
          "validated": "untested"
        }
      ],
      "recommendedProposal": "Verify the #1573 fix covers the RotateYTo scenario",
      "recommendedReason": "Least invasive — confirm whether a fix is already in place before adding new code."
    }
  },
  "output": {
    "actionability": {
      "suggestedAction": "needs-investigation",
      "confidence": 0.8,
      "reason": "Related issue #1573 with the identical stack trace was closed as completed, but this issue uses a different trigger (RotateYTo animation) and remains open. Investigation is needed to confirm whether the existing fix covers this scenario.",
      "suggestedReproPlatform": "windows"
    },
    "actions": [
      {
        "type": "update-labels",
        "description": "Apply type/bug, area/SkiaSharp.Views, os/Windows-Universal-UWP, backend/OpenGL, tenet/reliability labels",
        "risk": "low",
        "confidence": 0.92,
        "labels": [
          "type/bug",
          "area/SkiaSharp.Views",
          "os/Windows-Universal-UWP",
          "backend/OpenGL",
          "tenet/reliability"
        ]
      },
      {
        "type": "link-related",
        "description": "Cross-reference #1573 (same stack trace, different trigger, closed as completed)",
        "risk": "low",
        "confidence": 0.92,
        "linkedIssue": 1573
      },
      {
        "type": "add-comment",
        "description": "Note the relationship to #1573 and ask reporter to verify with current release",
        "risk": "medium",
        "confidence": 0.8,
        "comment": "Thanks for the detailed report. This crash shares the same stack trace as #1573, which was fixed and closed in March 2021. Could you test with the latest stable SkiaSharp release to confirm whether the RotateYTo scenario is also resolved? If the crash still occurs, please share the exact SkiaSharp version and a minimal repro project."
      }
    ]
  }
}
```

</details>
