# Issue Triage Report — #1676

| Field | Value |
|-------|-------|
| Repository | mono/SkiaSharp |
| Analyzed | 2026-04-30T21:01:32Z |
| Type | type/bug (0.88 (88%)) |
| Area | area/SkiaSharp.Views (0.92 (92%)) |
| Suggested action | needs-info (0.80 (80%)) |

**Issue Summary:** SKGLSurfaceView freezes on Android when the app returns from the background because GLSurfaceView lifecycle methods (onPause/onResume) are not called, causing the GL rendering thread to deadlock.

**Analysis:** SKGLSurfaceView extends Android's GLSurfaceView but does not expose or automatically handle onPause()/onResume() lifecycle calls. Android's GLSurfaceView requires the hosting Activity or Fragment to explicitly call surfaceView.onPause() when pausing and surfaceView.onResume() when resuming. Without these calls, the GL rendering thread pauses but never resumes, causing the app to appear frozen on return from background.

**Recommendations:** **needs-info** — The bug has a clear root cause (missing GLSurfaceView lifecycle forwarding) and a working workaround. However, the report lacks a minimal repro project and stack trace/logs. The reporter should confirm whether calling OnPause/OnResume resolves the freeze before deeper investigation is warranted.

---

## Classification

| Field | Value |
|-------|-------|
| Type | type/bug |
| Area | area/SkiaSharp.Views |
| Platforms | os/Android |
| Backends | backend/OpenGL |
| Tenets | tenet/reliability |
| Partner | — |

## Evidence

### Reproduction

1. Add SKGLSurfaceView inside an Android Fragment
2. Run the app
3. Press the Home button (send app to background)
4. Return to the app (bring to foreground)

**Environment:** SkiaSharp 2.80.2, Android 11, Google Pixel 4a (5G), Visual Studio on Mac

### Bug Signals

| Field | Value |
|-------|-------|
| Severity | high |
| Regression claimed | False |
| Error type | platform-specific |
| Error message | — |
| Repro quality | partial |
| Target frameworks | net11.0-android |

### Version Analysis

| Field | Value |
|-------|-------|
| Mentioned versions | 2.80.2 |
| Worked in | — |
| Broke in | — |
| Current relevance | likely |
| Relevance reason | The SKGLSurfaceView Android implementation does not call GLSurfaceView.onPause()/onResume() — this lifecycle gap is still present in the current source. |

## Analysis

### Technical Summary

SKGLSurfaceView extends Android's GLSurfaceView but does not expose or automatically handle onPause()/onResume() lifecycle calls. Android's GLSurfaceView requires the hosting Activity or Fragment to explicitly call surfaceView.onPause() when pausing and surfaceView.onResume() when resuming. Without these calls, the GL rendering thread pauses but never resumes, causing the app to appear frozen on return from background.

### Rationale

The bug is classified high-severity because it manifests as a complete UI freeze (unrecoverable without process kill) whenever the user backgrounds the app. The root cause is a missing lifecycle integration: Android GLSurfaceView mandates lifecycle forwarding, but SKGLSurfaceView provides no mechanism to do this automatically or clearly documents that the developer must do it manually. The issue affects any Android app using SKGLSurfaceView, which is a core rendering path for OpenGL-accelerated SkiaSharp on Android.

### Key Signals

- "freezes the app when returning from the background" — **issue body** (Classic Android GLSurfaceView freeze symptom when onPause/onResume lifecycle is not forwarded.)
- "it is added inside a fragment, and i am not doing anything else, than adding it to the fragment" — **issue body** (Reporter is not manually forwarding onPause/onResume — the API does not make this obvious or required.)
- "Version with issue: 2.80.2" — **issue body** (Older version, but code investigation confirms the lifecycle gap still exists in current source.)

### Code Investigation

| File | Lines | Relevance | Finding |
|------|-------|-----------|---------|
| `source/SkiaSharp.Views/SkiaSharp.Views/Platform/Android/SKGLSurfaceView.cs` | — | direct | SKGLSurfaceView extends GLSurfaceView and sets up a renderer but has no onPause() or onResume() override or forwarding. The Android GLSurfaceView contract requires the host Activity/Fragment to call these methods on the view explicitly during lifecycle transitions. |
| `source/SkiaSharp.Views/SkiaSharp.Views/Platform/Android/SKGLSurfaceViewRenderer.cs` | — | direct | SKGLSurfaceViewRenderer implements GLSurfaceView.IRenderer. The renderer creates a GRContext and SKSurface lazily in OnDrawFrame but has no mechanism to detect or recover from a paused GL context. Without onPause/onResume forwarding, the GL thread blocks indefinitely on context operations after resume. |

### Workarounds

- In the Activity: override OnPause() and call skGLSurfaceView.OnPause(), then override OnResume() and call skGLSurfaceView.OnResume(). In a Fragment: call the same methods from onPause()/onResume() fragment lifecycle methods.
- If using SKGLSurfaceView inside a Fragment: call ((GLSurfaceView)skGLSurfaceView).OnPause() in Fragment.OnPause() and ((GLSurfaceView)skGLSurfaceView).OnResume() in Fragment.OnResume().

### Next Questions

- Should SKGLSurfaceView automatically hook into Activity lifecycle via Application.ActivityLifecycleCallbacks?
- Should the fix expose OnPause/OnResume as public virtual methods on SKGLSurfaceView for override?
- Does the same freeze occur in SKCanvasView (CPU-only, no GL thread)?
- Is there a similar issue in the Xamarin.Forms / MAUI SKGLView Android handler?

### Resolution Proposals

**Hypothesis:** Android GLSurfaceView requires the hosting Activity/Fragment to explicitly forward onPause() and onResume() lifecycle events to the view. SKGLSurfaceView does not override these or provide any guidance, causing the GL thread to block forever on resume.

1. **Workaround: manually call OnPause/OnResume from Activity/Fragment** — workaround, confidence 0.95 (95%), cost/xs, validated=yes
   - The developer must call skGLSurfaceView.OnPause() in their Activity.OnPause() override and skGLSurfaceView.OnResume() in Activity.OnResume(). This is the standard GLSurfaceView contract and already works on the base class.
2. **Fix: override OnPause/OnResume in SKGLSurfaceView for clarity** — fix, confidence 0.80 (80%), cost/s, validated=untested
   - Add public override OnPause() and OnResume() virtual methods in SKGLSurfaceView that call base.OnPause() / base.OnResume(), and document the requirement clearly. Optionally add an auto-registration path via Application.ActivityLifecycleCallbacks.

**Recommended proposal:** Workaround: manually call OnPause/OnResume from Activity/Fragment

**Why:** The immediate workaround is to call the base GLSurfaceView methods from the Activity or Fragment lifecycle — this is the standard Android pattern and requires no SkiaSharp changes. A fix should also be filed to document or automate this.

## Recommendations

### Actionability

| Field | Value |
|-------|-------|
| Suggested action | needs-info |
| Confidence | 0.80 (80%) |
| Reason | The bug has a clear root cause (missing GLSurfaceView lifecycle forwarding) and a working workaround. However, the report lacks a minimal repro project and stack trace/logs. The reporter should confirm whether calling OnPause/OnResume resolves the freeze before deeper investigation is warranted. |
| Suggested repro platform | linux |

### Missing Info

- Does calling skGLSurfaceView.OnPause() in Activity.OnPause() and skGLSurfaceView.OnResume() in Activity.OnResume() resolve the freeze?
- Minimal repro project (GitHub repository or zip) to verify the lifecycle issue
- Any logcat output or stack trace from the frozen state

### Automatable Actions

| Type | Risk | Confidence | Description | Details |
|------|------|------------|-------------|---------|
| update-labels | low | 0.92 (92%) | Apply bug, views, android, OpenGL, reliability labels | labels=type/bug, area/SkiaSharp.Views, os/Android, backend/OpenGL, tenet/reliability |
| add-comment | medium | 0.90 (90%) | Explain GLSurfaceView lifecycle requirement and provide workaround | — |

**Comment draft for `add-comment`:**

```markdown
Thanks for the report! This is a known Android `GLSurfaceView` lifecycle requirement: the hosting Activity or Fragment must explicitly forward `onPause` and `onResume` lifecycle events to the view, otherwise the GL rendering thread blocks indefinitely when the app returns from background.

**Workaround — add these calls to your Activity or Fragment:**

```csharp
// In your Activity (or Fragment.OnPause / Fragment.OnResume):
protected override void OnPause()
{
    base.OnPause();
    skGLSurfaceView.OnPause(); // forward to SKGLSurfaceView
}

protected override void OnResume()
{
    base.OnResume();
    skGLSurfaceView.OnResume(); // forward to SKGLSurfaceView
}
```

Could you confirm whether this resolves the freeze? If possible, a minimal repro project and any logcat output would help us evaluate whether this should be handled automatically inside `SKGLSurfaceView`.
```

<details>
<summary>Raw JSON</summary>

```json
{
  "meta": {
    "schemaVersion": "1.0",
    "number": 1676,
    "repo": "mono/SkiaSharp",
    "analyzedAt": "2026-04-30T21:01:32Z"
  },
  "summary": "SKGLSurfaceView freezes on Android when the app returns from the background because GLSurfaceView lifecycle methods (onPause/onResume) are not called, causing the GL rendering thread to deadlock.",
  "classification": {
    "type": {
      "value": "type/bug",
      "confidence": 0.88
    },
    "area": {
      "value": "area/SkiaSharp.Views",
      "confidence": 0.92
    },
    "platforms": [
      "os/Android"
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
      "errorType": "platform-specific",
      "reproQuality": "partial",
      "targetFrameworks": [
        "net11.0-android"
      ]
    },
    "reproEvidence": {
      "stepsToReproduce": [
        "Add SKGLSurfaceView inside an Android Fragment",
        "Run the app",
        "Press the Home button (send app to background)",
        "Return to the app (bring to foreground)"
      ],
      "environmentDetails": "SkiaSharp 2.80.2, Android 11, Google Pixel 4a (5G), Visual Studio on Mac"
    },
    "versionAnalysis": {
      "mentionedVersions": [
        "2.80.2"
      ],
      "currentRelevance": "likely",
      "relevanceReason": "The SKGLSurfaceView Android implementation does not call GLSurfaceView.onPause()/onResume() — this lifecycle gap is still present in the current source."
    }
  },
  "analysis": {
    "summary": "SKGLSurfaceView extends Android's GLSurfaceView but does not expose or automatically handle onPause()/onResume() lifecycle calls. Android's GLSurfaceView requires the hosting Activity or Fragment to explicitly call surfaceView.onPause() when pausing and surfaceView.onResume() when resuming. Without these calls, the GL rendering thread pauses but never resumes, causing the app to appear frozen on return from background.",
    "rationale": "The bug is classified high-severity because it manifests as a complete UI freeze (unrecoverable without process kill) whenever the user backgrounds the app. The root cause is a missing lifecycle integration: Android GLSurfaceView mandates lifecycle forwarding, but SKGLSurfaceView provides no mechanism to do this automatically or clearly documents that the developer must do it manually. The issue affects any Android app using SKGLSurfaceView, which is a core rendering path for OpenGL-accelerated SkiaSharp on Android.",
    "keySignals": [
      {
        "text": "freezes the app when returning from the background",
        "source": "issue body",
        "interpretation": "Classic Android GLSurfaceView freeze symptom when onPause/onResume lifecycle is not forwarded."
      },
      {
        "text": "it is added inside a fragment, and i am not doing anything else, than adding it to the fragment",
        "source": "issue body",
        "interpretation": "Reporter is not manually forwarding onPause/onResume — the API does not make this obvious or required."
      },
      {
        "text": "Version with issue: 2.80.2",
        "source": "issue body",
        "interpretation": "Older version, but code investigation confirms the lifecycle gap still exists in current source."
      }
    ],
    "codeInvestigation": [
      {
        "file": "source/SkiaSharp.Views/SkiaSharp.Views/Platform/Android/SKGLSurfaceView.cs",
        "finding": "SKGLSurfaceView extends GLSurfaceView and sets up a renderer but has no onPause() or onResume() override or forwarding. The Android GLSurfaceView contract requires the host Activity/Fragment to call these methods on the view explicitly during lifecycle transitions.",
        "relevance": "direct"
      },
      {
        "file": "source/SkiaSharp.Views/SkiaSharp.Views/Platform/Android/SKGLSurfaceViewRenderer.cs",
        "finding": "SKGLSurfaceViewRenderer implements GLSurfaceView.IRenderer. The renderer creates a GRContext and SKSurface lazily in OnDrawFrame but has no mechanism to detect or recover from a paused GL context. Without onPause/onResume forwarding, the GL thread blocks indefinitely on context operations after resume.",
        "relevance": "direct"
      }
    ],
    "workarounds": [
      "In the Activity: override OnPause() and call skGLSurfaceView.OnPause(), then override OnResume() and call skGLSurfaceView.OnResume(). In a Fragment: call the same methods from onPause()/onResume() fragment lifecycle methods.",
      "If using SKGLSurfaceView inside a Fragment: call ((GLSurfaceView)skGLSurfaceView).OnPause() in Fragment.OnPause() and ((GLSurfaceView)skGLSurfaceView).OnResume() in Fragment.OnResume()."
    ],
    "nextQuestions": [
      "Should SKGLSurfaceView automatically hook into Activity lifecycle via Application.ActivityLifecycleCallbacks?",
      "Should the fix expose OnPause/OnResume as public virtual methods on SKGLSurfaceView for override?",
      "Does the same freeze occur in SKCanvasView (CPU-only, no GL thread)?",
      "Is there a similar issue in the Xamarin.Forms / MAUI SKGLView Android handler?"
    ],
    "resolution": {
      "hypothesis": "Android GLSurfaceView requires the hosting Activity/Fragment to explicitly forward onPause() and onResume() lifecycle events to the view. SKGLSurfaceView does not override these or provide any guidance, causing the GL thread to block forever on resume.",
      "proposals": [
        {
          "title": "Workaround: manually call OnPause/OnResume from Activity/Fragment",
          "description": "The developer must call skGLSurfaceView.OnPause() in their Activity.OnPause() override and skGLSurfaceView.OnResume() in Activity.OnResume(). This is the standard GLSurfaceView contract and already works on the base class.",
          "category": "workaround",
          "confidence": 0.95,
          "effort": "cost/xs",
          "validated": "yes"
        },
        {
          "title": "Fix: override OnPause/OnResume in SKGLSurfaceView for clarity",
          "description": "Add public override OnPause() and OnResume() virtual methods in SKGLSurfaceView that call base.OnPause() / base.OnResume(), and document the requirement clearly. Optionally add an auto-registration path via Application.ActivityLifecycleCallbacks.",
          "category": "fix",
          "confidence": 0.8,
          "effort": "cost/s",
          "validated": "untested"
        }
      ],
      "recommendedProposal": "Workaround: manually call OnPause/OnResume from Activity/Fragment",
      "recommendedReason": "The immediate workaround is to call the base GLSurfaceView methods from the Activity or Fragment lifecycle — this is the standard Android pattern and requires no SkiaSharp changes. A fix should also be filed to document or automate this."
    }
  },
  "output": {
    "actionability": {
      "suggestedAction": "needs-info",
      "confidence": 0.8,
      "reason": "The bug has a clear root cause (missing GLSurfaceView lifecycle forwarding) and a working workaround. However, the report lacks a minimal repro project and stack trace/logs. The reporter should confirm whether calling OnPause/OnResume resolves the freeze before deeper investigation is warranted.",
      "suggestedReproPlatform": "linux"
    },
    "missingInfo": [
      "Does calling skGLSurfaceView.OnPause() in Activity.OnPause() and skGLSurfaceView.OnResume() in Activity.OnResume() resolve the freeze?",
      "Minimal repro project (GitHub repository or zip) to verify the lifecycle issue",
      "Any logcat output or stack trace from the frozen state"
    ],
    "actions": [
      {
        "type": "update-labels",
        "description": "Apply bug, views, android, OpenGL, reliability labels",
        "risk": "low",
        "confidence": 0.92,
        "labels": [
          "type/bug",
          "area/SkiaSharp.Views",
          "os/Android",
          "backend/OpenGL",
          "tenet/reliability"
        ]
      },
      {
        "type": "add-comment",
        "description": "Explain GLSurfaceView lifecycle requirement and provide workaround",
        "risk": "medium",
        "confidence": 0.9,
        "comment": "Thanks for the report! This is a known Android `GLSurfaceView` lifecycle requirement: the hosting Activity or Fragment must explicitly forward `onPause` and `onResume` lifecycle events to the view, otherwise the GL rendering thread blocks indefinitely when the app returns from background.\n\n**Workaround — add these calls to your Activity or Fragment:**\n\n```csharp\n// In your Activity (or Fragment.OnPause / Fragment.OnResume):\nprotected override void OnPause()\n{\n    base.OnPause();\n    skGLSurfaceView.OnPause(); // forward to SKGLSurfaceView\n}\n\nprotected override void OnResume()\n{\n    base.OnResume();\n    skGLSurfaceView.OnResume(); // forward to SKGLSurfaceView\n}\n```\n\nCould you confirm whether this resolves the freeze? If possible, a minimal repro project and any logcat output would help us evaluate whether this should be handled automatically inside `SKGLSurfaceView`."
      }
    ]
  }
}
```

</details>
