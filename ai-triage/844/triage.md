# Issue Triage Report — #844

| Field | Value |
|-------|-------|
| Repository | mono/SkiaSharp |
| Analyzed | 2026-05-01T10:31:39Z |
| Type | type/bug (0.97 (97%)) |
| Area | area/SkiaSharp.Views (0.88 (88%)) |
| Suggested action | close-as-fixed (0.82 (82%)) |

**Issue Summary:** AppKitThreadAccessException thrown on macOS when SKGLView's HasRenderLoop is enabled via Xamarin.Forms, because the CVDisplayLink callback fires on a non-UI thread and calls NSView.Display() directly.

**Analysis:** The CVDisplayLink callback in the Xamarin.Forms macOS SKGLViewRenderer calls NSView.Display() directly from the CVDisplayLink thread (which is not the main thread), triggering AppKit's thread-safety check. The reporter identified the root cause and provided a workaround (InvokeOnMainThread). The Xamarin.Forms renderers have since been removed; the MAUI handler on iOS/macCatalyst uses CADisplayLink with BeginInvokeOnMainThread, correctly marshaling display calls to the UI thread.

**Recommendations:** **close-as-fixed** — The Xamarin.Forms macOS renderer containing the bug no longer exists. The MAUI replacement correctly marshals Display() to the main thread. Xamarin.Forms is EOL.

---

## Classification

| Field | Value |
|-------|-------|
| Type | type/bug |
| Area | area/SkiaSharp.Views |
| Platforms | os/macOS |
| Backends | — |
| Tenets | tenet/reliability |
| Partner | — |

## Evidence

### Reproduction

1. Create a Xamarin.Forms app targeting macOS
2. Add an SKGLView and set HasRenderLoop = true
3. Run the app and observe AppKitThreadAccessException from CVDisplayLink callback

**Environment:** macOS Mojave 10.14.4, SkiaSharp 1.68.1, Visual Studio for Mac 2019 v8.0.5

**Repository links:**
- https://github.com/danien/SkiaSharp/tree/dev/issue-844_SKGLView — Forked repro with SKGLView replacing SKCanvasView in Xamarin.Forms sample

### Bug Signals

| Field | Value |
|-------|-------|
| Severity | medium |
| Regression claimed | False |
| Error type | exception |
| Error message | AppKit Consistency error: you are calling a method that can only be invoked from the UI thread. |
| Repro quality | complete |
| Target frameworks | Xamarin.Forms macOS |

### Version Analysis

| Field | Value |
|-------|-------|
| Mentioned versions | 1.68.1 |
| Worked in | — |
| Broke in | — |
| Current relevance | unlikely |
| Relevance reason | The Xamarin.Forms Mac renderer (SKGLViewRenderer.cs) no longer exists in the codebase. The MAUI replacement (SKGLViewHandler.iOS.cs) correctly uses BeginInvokeOnMainThread and CADisplayLink, which eliminates the threading issue. Xamarin.Forms is also EOL. |

### Fix Status

| Field | Value |
|-------|-------|
| Likely fixed | True |
| Confidence | 0.85 (85%) |
| Reason | The Xamarin.Forms Mac renderer that contained the bug (CVDisplayLink callback calling Display() off the main thread) has been removed. The MAUI handler uses CADisplayLink with BeginInvokeOnMainThread, properly marshaling to the UI thread. Xamarin.Forms is EOL and superseded by MAUI. |
| Related PRs | — |
| Related commits | — |
| Fixed in version | — |

## Analysis

### Technical Summary

The CVDisplayLink callback in the Xamarin.Forms macOS SKGLViewRenderer calls NSView.Display() directly from the CVDisplayLink thread (which is not the main thread), triggering AppKit's thread-safety check. The reporter identified the root cause and provided a workaround (InvokeOnMainThread). The Xamarin.Forms renderers have since been removed; the MAUI handler on iOS/macCatalyst uses CADisplayLink with BeginInvokeOnMainThread, correctly marshaling display calls to the UI thread.

### Rationale

Clear threading bug in the Xamarin.Forms Mac renderer: CVDisplayLink fires on a non-UI thread, but NSView.Display() requires the main thread. The reporter pinpointed the exact file and line. The Xamarin.Forms renderer is gone from the current codebase, superseded by MAUI which handles threading correctly. The issue is effectively resolved for users of the current MAUI-based implementation.

### Key Signals

- "nativeView?.Display() is called inside the displayLink callback because the callback happens from another thread" — **issue body** (Root cause explicitly identified by reporter: CVDisplayLink fires on non-main thread, Display() requires main thread.)
- "Wrapping the callback's code with InvokeOnMainThread() fixes the issue" — **comment by danien** (Reporter self-confirmed the workaround, indicating a clear threading marshaling omission in the original renderer.)
- "SKGLViewRenderer.cs (Xamarin.Forms Mac)" — **issue body** (The affected file is the Xamarin.Forms macOS renderer, which no longer exists in the current codebase.)

### Code Investigation

| File | Lines | Relevance | Finding |
|------|-------|-----------|---------|
| `source/SkiaSharp.Views/SkiaSharp.Views/Platform/macOS/SKGLView.cs` | — | context | Current macOS SKGLView is a plain NSOpenGLView subclass with no CVDisplayLink or render loop — the render loop logic was in the now-removed Xamarin.Forms renderer. |
| `source/SkiaSharp.Views.Maui/SkiaSharp.Views.Maui.Core/Handlers/SKGLView/SKGLViewHandler.iOS.cs` | 136-148 | direct | Current MAUI renderer uses CADisplayLink (not CVDisplayLink) and RequestDisplay() wraps Display() with BeginInvokeOnMainThread — correctly avoiding the threading bug reported in issue #844. |

### Workarounds

- Wrap the CVDisplayLink callback body with InvokeOnMainThread(() => { ... }) to marshal Display() to the main thread (as suggested by reporter).
- Migrate from Xamarin.Forms to .NET MAUI with SkiaSharp.Views.Maui — the MAUI handler is not affected by this bug.

### Resolution Proposals

**Hypothesis:** The Xamarin.Forms Mac renderer omitted the main-thread marshal when calling NSView.Display() from the CVDisplayLink callback. This has been fixed by architecture (renderer removed, MAUI uses BeginInvokeOnMainThread).

1. **Close as fixed — Xamarin.Forms renderer removed** — fix, confidence 0.85 (85%), cost/xs, validated=untested
   - The affected SKGLViewRenderer.cs (Xamarin.Forms Mac) no longer exists. Users on MAUI are not affected. Suggest migrating to MAUI if still on Xamarin.Forms.

**Recommended proposal:** Close as fixed — Xamarin.Forms renderer removed

**Why:** The buggy renderer was deleted as part of the Xamarin.Forms → MAUI migration. The MAUI handler correctly uses BeginInvokeOnMainThread.

## Recommendations

### Actionability

| Field | Value |
|-------|-------|
| Suggested action | close-as-fixed |
| Confidence | 0.82 (82%) |
| Reason | The Xamarin.Forms macOS renderer containing the bug no longer exists. The MAUI replacement correctly marshals Display() to the main thread. Xamarin.Forms is EOL. |
| Suggested repro platform | macos |

### Automatable Actions

| Type | Risk | Confidence | Description | Details |
|------|------|------------|-------------|---------|
| update-labels | low | 0.95 (95%) | Apply bug, views, macOS labels | labels=type/bug, area/SkiaSharp.Views, os/macOS, tenet/reliability |
| add-comment | medium | 0.82 (82%) | Inform reporter that the Xamarin.Forms renderer was removed and MAUI replacement is correct | — |
| close-issue | medium | 0.82 (82%) | Close as fixed — Xamarin.Forms renderer removed, MAUI implementation is correct | stateReason=completed |

**Comment draft for `add-comment`:**

```markdown
Thank you for the detailed report and for identifying the root cause!

The affected `SKGLViewRenderer.cs` (Xamarin.Forms macOS) is no longer part of the codebase — it was removed as part of the Xamarin.Forms → .NET MAUI migration. The current `SKGLViewHandler` for MAUI correctly wraps `Display()` calls with `BeginInvokeOnMainThread`, which avoids the threading issue you described.

If you are still on Xamarin.Forms, the workaround you provided (wrapping the CVDisplayLink callback with `InvokeOnMainThread`) is the correct fix. If possible, migrating to .NET MAUI + `SkiaSharp.Views.Maui` is the recommended long-term path since Xamarin.Forms is now end-of-life.

Closing this as fixed via the MAUI rewrite. Please reopen if you encounter the same behavior in a MAUI project.
```

<details>
<summary>Raw JSON</summary>

```json
{
  "meta": {
    "schemaVersion": "1.0",
    "number": 844,
    "repo": "mono/SkiaSharp",
    "analyzedAt": "2026-05-01T10:31:39Z"
  },
  "summary": "AppKitThreadAccessException thrown on macOS when SKGLView's HasRenderLoop is enabled via Xamarin.Forms, because the CVDisplayLink callback fires on a non-UI thread and calls NSView.Display() directly.",
  "classification": {
    "type": {
      "value": "type/bug",
      "confidence": 0.97
    },
    "area": {
      "value": "area/SkiaSharp.Views",
      "confidence": 0.88
    },
    "platforms": [
      "os/macOS"
    ],
    "tenets": [
      "tenet/reliability"
    ]
  },
  "evidence": {
    "bugSignals": {
      "severity": "medium",
      "regressionClaimed": false,
      "errorType": "exception",
      "errorMessage": "AppKit Consistency error: you are calling a method that can only be invoked from the UI thread.",
      "reproQuality": "complete",
      "targetFrameworks": [
        "Xamarin.Forms macOS"
      ]
    },
    "reproEvidence": {
      "stepsToReproduce": [
        "Create a Xamarin.Forms app targeting macOS",
        "Add an SKGLView and set HasRenderLoop = true",
        "Run the app and observe AppKitThreadAccessException from CVDisplayLink callback"
      ],
      "environmentDetails": "macOS Mojave 10.14.4, SkiaSharp 1.68.1, Visual Studio for Mac 2019 v8.0.5",
      "repoLinks": [
        {
          "url": "https://github.com/danien/SkiaSharp/tree/dev/issue-844_SKGLView",
          "description": "Forked repro with SKGLView replacing SKCanvasView in Xamarin.Forms sample"
        }
      ]
    },
    "versionAnalysis": {
      "mentionedVersions": [
        "1.68.1"
      ],
      "currentRelevance": "unlikely",
      "relevanceReason": "The Xamarin.Forms Mac renderer (SKGLViewRenderer.cs) no longer exists in the codebase. The MAUI replacement (SKGLViewHandler.iOS.cs) correctly uses BeginInvokeOnMainThread and CADisplayLink, which eliminates the threading issue. Xamarin.Forms is also EOL."
    },
    "fixStatus": {
      "likelyFixed": true,
      "confidence": 0.85,
      "reason": "The Xamarin.Forms Mac renderer that contained the bug (CVDisplayLink callback calling Display() off the main thread) has been removed. The MAUI handler uses CADisplayLink with BeginInvokeOnMainThread, properly marshaling to the UI thread. Xamarin.Forms is EOL and superseded by MAUI."
    }
  },
  "analysis": {
    "summary": "The CVDisplayLink callback in the Xamarin.Forms macOS SKGLViewRenderer calls NSView.Display() directly from the CVDisplayLink thread (which is not the main thread), triggering AppKit's thread-safety check. The reporter identified the root cause and provided a workaround (InvokeOnMainThread). The Xamarin.Forms renderers have since been removed; the MAUI handler on iOS/macCatalyst uses CADisplayLink with BeginInvokeOnMainThread, correctly marshaling display calls to the UI thread.",
    "rationale": "Clear threading bug in the Xamarin.Forms Mac renderer: CVDisplayLink fires on a non-UI thread, but NSView.Display() requires the main thread. The reporter pinpointed the exact file and line. The Xamarin.Forms renderer is gone from the current codebase, superseded by MAUI which handles threading correctly. The issue is effectively resolved for users of the current MAUI-based implementation.",
    "keySignals": [
      {
        "text": "nativeView?.Display() is called inside the displayLink callback because the callback happens from another thread",
        "source": "issue body",
        "interpretation": "Root cause explicitly identified by reporter: CVDisplayLink fires on non-main thread, Display() requires main thread."
      },
      {
        "text": "Wrapping the callback's code with InvokeOnMainThread() fixes the issue",
        "source": "comment by danien",
        "interpretation": "Reporter self-confirmed the workaround, indicating a clear threading marshaling omission in the original renderer."
      },
      {
        "text": "SKGLViewRenderer.cs (Xamarin.Forms Mac)",
        "source": "issue body",
        "interpretation": "The affected file is the Xamarin.Forms macOS renderer, which no longer exists in the current codebase."
      }
    ],
    "codeInvestigation": [
      {
        "file": "source/SkiaSharp.Views/SkiaSharp.Views/Platform/macOS/SKGLView.cs",
        "finding": "Current macOS SKGLView is a plain NSOpenGLView subclass with no CVDisplayLink or render loop — the render loop logic was in the now-removed Xamarin.Forms renderer.",
        "relevance": "context"
      },
      {
        "file": "source/SkiaSharp.Views.Maui/SkiaSharp.Views.Maui.Core/Handlers/SKGLView/SKGLViewHandler.iOS.cs",
        "lines": "136-148",
        "finding": "Current MAUI renderer uses CADisplayLink (not CVDisplayLink) and RequestDisplay() wraps Display() with BeginInvokeOnMainThread — correctly avoiding the threading bug reported in issue #844.",
        "relevance": "direct"
      }
    ],
    "workarounds": [
      "Wrap the CVDisplayLink callback body with InvokeOnMainThread(() => { ... }) to marshal Display() to the main thread (as suggested by reporter).",
      "Migrate from Xamarin.Forms to .NET MAUI with SkiaSharp.Views.Maui — the MAUI handler is not affected by this bug."
    ],
    "resolution": {
      "hypothesis": "The Xamarin.Forms Mac renderer omitted the main-thread marshal when calling NSView.Display() from the CVDisplayLink callback. This has been fixed by architecture (renderer removed, MAUI uses BeginInvokeOnMainThread).",
      "proposals": [
        {
          "title": "Close as fixed — Xamarin.Forms renderer removed",
          "description": "The affected SKGLViewRenderer.cs (Xamarin.Forms Mac) no longer exists. Users on MAUI are not affected. Suggest migrating to MAUI if still on Xamarin.Forms.",
          "category": "fix",
          "confidence": 0.85,
          "effort": "cost/xs",
          "validated": "untested"
        }
      ],
      "recommendedProposal": "Close as fixed — Xamarin.Forms renderer removed",
      "recommendedReason": "The buggy renderer was deleted as part of the Xamarin.Forms → MAUI migration. The MAUI handler correctly uses BeginInvokeOnMainThread."
    }
  },
  "output": {
    "actionability": {
      "suggestedAction": "close-as-fixed",
      "confidence": 0.82,
      "reason": "The Xamarin.Forms macOS renderer containing the bug no longer exists. The MAUI replacement correctly marshals Display() to the main thread. Xamarin.Forms is EOL.",
      "suggestedReproPlatform": "macos"
    },
    "actions": [
      {
        "type": "update-labels",
        "description": "Apply bug, views, macOS labels",
        "risk": "low",
        "confidence": 0.95,
        "labels": [
          "type/bug",
          "area/SkiaSharp.Views",
          "os/macOS",
          "tenet/reliability"
        ]
      },
      {
        "type": "add-comment",
        "description": "Inform reporter that the Xamarin.Forms renderer was removed and MAUI replacement is correct",
        "risk": "medium",
        "confidence": 0.82,
        "comment": "Thank you for the detailed report and for identifying the root cause!\n\nThe affected `SKGLViewRenderer.cs` (Xamarin.Forms macOS) is no longer part of the codebase — it was removed as part of the Xamarin.Forms → .NET MAUI migration. The current `SKGLViewHandler` for MAUI correctly wraps `Display()` calls with `BeginInvokeOnMainThread`, which avoids the threading issue you described.\n\nIf you are still on Xamarin.Forms, the workaround you provided (wrapping the CVDisplayLink callback with `InvokeOnMainThread`) is the correct fix. If possible, migrating to .NET MAUI + `SkiaSharp.Views.Maui` is the recommended long-term path since Xamarin.Forms is now end-of-life.\n\nClosing this as fixed via the MAUI rewrite. Please reopen if you encounter the same behavior in a MAUI project."
      },
      {
        "type": "close-issue",
        "description": "Close as fixed — Xamarin.Forms renderer removed, MAUI implementation is correct",
        "risk": "medium",
        "confidence": 0.82,
        "stateReason": "completed"
      }
    ]
  }
}
```

</details>
