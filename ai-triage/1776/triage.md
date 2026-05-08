# Issue Triage Report — #1776

| Field | Value |
|-------|-------|
| Repository | mono/SkiaSharp |
| Analyzed | 2026-04-30T11:55:35Z |
| Type | type/bug (0.90 (90%)) |
| Area | area/SkiaSharp.Views.Forms (0.85 (85%)) |
| Suggested action | needs-info (0.75 (75%)) |

**Issue Summary:** ExecutionEngineException crash on Android 6.0.1 (Samsung Galaxy J2 Prime) when GLTextureView+GLThread.Start() attempts to create a new thread during SKGLTextureView initialization via Xamarin.Forms renderer.

**Analysis:** The crash occurs when Android's OS-level thread creation fails (Error 0x0) while SKGLTextureView constructor calls SetRenderer which starts a new Thread for the GL rendering loop. This is an OS-level thread resource exhaustion or device-specific limitation on the Samsung Galaxy J2 Prime running Android 6.0.1, not a bug in SkiaSharp's logic per se, but SkiaSharp does not guard against Thread.Start() failing with an ExecutionEngineException.

**Recommendations:** **needs-info** — The crash is not reproducible, reported on SkiaSharp 2.80.2 (old version), and appears intermittent/device-specific. More info is needed: can it be reproduced on a current version? Is it consistent or intermittent? No minimal repro provided.

---

## Classification

| Field | Value |
|-------|-------|
| Type | type/bug |
| Area | area/SkiaSharp.Views.Forms |
| Platforms | os/Android |
| Backends | backend/OpenGL |
| Tenets | tenet/reliability |
| Partner | — |

## Evidence

### Reproduction

**Environment:** Samsung Galaxy J2 Prime, Android 6.0.1, SkiaSharp 2.80.2, Xamarin.Forms

### Bug Signals

| Field | Value |
|-------|-------|
| Severity | high |
| Regression claimed | False |
| Error type | crash |
| Error message | System.ExecutionEngineException: Couldn't create thread. Error 0x0 |
| Repro quality | partial |
| Target frameworks | monoandroid6.0 |

**Stack trace:**

```text
System.ExecutionEngineException: Couldn't create thread. Error 0x0
  at System.Threading.Thread.Thread_internal(...)
  at SkiaSharp.Views.Android.GLTextureView+GLThread.Start()
  at SkiaSharp.Views.Android.GLTextureView.SetRenderer(...)
  at SkiaSharp.Views.Android.SKGLTextureView.Initialize()
  at SkiaSharp.Views.Forms.SKGLViewRenderer.CreateNativeControl()
```

### Version Analysis

| Field | Value |
|-------|-------|
| Mentioned versions | 2.80.2 |
| Worked in | — |
| Broke in | — |
| Current relevance | unknown |
| Relevance reason | Issue is from 2021 on SkiaSharp 2.80.2; current SkiaSharp version is much newer. No reproduction steps provided; crash not reproducible by reporter. |

## Analysis

### Technical Summary

The crash occurs when Android's OS-level thread creation fails (Error 0x0) while SKGLTextureView constructor calls SetRenderer which starts a new Thread for the GL rendering loop. This is an OS-level thread resource exhaustion or device-specific limitation on the Samsung Galaxy J2 Prime running Android 6.0.1, not a bug in SkiaSharp's logic per se, but SkiaSharp does not guard against Thread.Start() failing with an ExecutionEngineException.

### Rationale

Classified as type/bug in area/SkiaSharp.Views.Forms because the crash occurs at GLTextureView+GLThread.Start() called from Xamarin.Forms SKGLViewRenderer.CreateNativeControl(). The root cause is that the OS cannot create a new thread, which is an OS/device resource issue, but SkiaSharp's GLThread.Start() does not handle this gracefully. The issue is Android-specific and OpenGL backend-specific. Severity is high because it's a hard crash (ExecutionEngineException) with no workaround visible to users, though it appears intermittent and device-specific.

### Key Signals

- "System.ExecutionEngineException: Couldn't create thread. Error 0x0" — **issue body - stack trace** (The Android kernel rejected Thread creation, likely due to per-process thread limit or memory pressure on the low-end device)
- "Issue was reported by our AppCenter production crash monitoring. Not reproducible as of now." — **issue body** (Intermittent crash from production, not locally reproducible; likely a resource-exhaustion race condition)
- "Samsung Galaxy J2 Prime / Android 6.0.1" — **issue body** (Low-end device with limited resources and older Android; more prone to thread limit exhaustion)

### Code Investigation

| File | Lines | Relevance | Finding |
|------|-------|-----------|---------|
| `source/SkiaSharp.Views/SkiaSharp.Views/Platform/Android/GLTextureView.cs` | — | direct | GLThread.Start() calls thread.Start() directly with no exception handling around Thread.Start(). If the OS cannot allocate a new thread (ENOMEM or thread limit exceeded), Thread.Start() will throw ExecutionEngineException on Mono/Android. |
| `source/SkiaSharp.Views/SkiaSharp.Views/Platform/Android/SKGLTextureView.cs` | — | direct | SKGLTextureView constructor calls Initialize() which immediately calls SetRenderer(renderer). On Android 6.0.1 on low-memory devices, this thread creation at construction time may fail if the system is under thread pressure. |

### Next Questions

- Does the crash still occur with current SkiaSharp (2.88+)?
- Can the reporter confirm whether the crash happens consistently or only under memory pressure?
- What is the total thread count in the app when this crash occurs?
- Does switching to SKCanvasView (CPU rasterizer) resolve the issue on that device?

### Resolution Proposals

**Hypothesis:** The crash is triggered by OS-level thread exhaustion on the low-end Samsung Galaxy J2 Prime (Android 6.0.1) when SKGLTextureView attempts to start a GL rendering thread. SkiaSharp should handle thread creation failure more gracefully, or callers should be aware to use SKCanvasView (software rasterizer) on resource-constrained devices.

1. **Use SKCanvasView instead of SKGLTextureView on low-end Android devices** — workaround, cost/s, validated=untested
   - On resource-constrained devices, use the CPU rasterizer (SKCanvasView) rather than the OpenGL-accelerated SKGLView/SKGLTextureView. This avoids spinning up a dedicated GL thread entirely.
2. **Investigate adding exception handling in GLThread.Start()** — investigation, cost/m, validated=untested
   - GLTextureView.GLThread.Start() could wrap Thread.Start() in a try/catch to handle ExecutionEngineException and propagate a meaningful error rather than crashing.

**Recommended proposal:** workaround-1

**Why:** The root cause is OS thread exhaustion on a low-end device; the most practical immediate workaround is to use the software rasterizer on such devices.

## Recommendations

### Actionability

| Field | Value |
|-------|-------|
| Suggested action | needs-info |
| Confidence | 0.75 (75%) |
| Reason | The crash is not reproducible, reported on SkiaSharp 2.80.2 (old version), and appears intermittent/device-specific. More info is needed: can it be reproduced on a current version? Is it consistent or intermittent? No minimal repro provided. |
| Suggested repro platform | linux |

### Missing Info

- Can this be reproduced on the latest SkiaSharp version (2.88+)?
- Steps or conditions under which the crash reliably occurs (or frequency from AppCenter)
- Confirmation whether using SKCanvasView avoids the crash on the same device

### Automatable Actions

| Type | Risk | Confidence | Description | Details |
|------|------|------------|-------------|---------|
| update-labels | low | 0.90 (90%) | Apply type/bug, area/SkiaSharp.Views.Forms, os/Android, backend/OpenGL, tenet/reliability labels | labels=type/bug, area/SkiaSharp.Views.Forms, os/Android, backend/OpenGL, tenet/reliability |
| add-comment | medium | 0.75 (75%) | Ask reporter for more information: current version, reproducibility conditions, and whether SKCanvasView works as a workaround | — |

**Comment draft for `add-comment`:**

```markdown
Thank you for reporting this crash!

This appears to be a thread creation failure at the OS level on your Samsung Galaxy J2 Prime (Android 6.0.1), where the OS returned an error when SkiaSharp tried to create a GL rendering thread. This can happen on low-memory or resource-constrained devices.

Could you help us with the following?

1. **Current version**: Does this still occur with the latest SkiaSharp (2.88+)?
2. **Reproducibility**: Is this crash consistent, or does it only happen occasionally (e.g., under memory pressure)? The AppCenter report shows intermittent occurrence.
3. **Workaround**: Does switching from `SKGLView` to `SKCanvasView` (CPU rasterizer) resolve the crash on the same device?

As a potential **workaround** in the meantime: if your app can detect low-end devices, consider using `SKCanvasView` instead of `SKGLView`/`SKGLTextureView` — this avoids spinning up a dedicated OpenGL thread entirely.

Thank you!
```

<details>
<summary>Raw JSON</summary>

```json
{
  "meta": {
    "schemaVersion": "1.0",
    "number": 1776,
    "repo": "mono/SkiaSharp",
    "analyzedAt": "2026-04-30T11:55:35Z"
  },
  "summary": "ExecutionEngineException crash on Android 6.0.1 (Samsung Galaxy J2 Prime) when GLTextureView+GLThread.Start() attempts to create a new thread during SKGLTextureView initialization via Xamarin.Forms renderer.",
  "classification": {
    "type": {
      "value": "type/bug",
      "confidence": 0.9
    },
    "area": {
      "value": "area/SkiaSharp.Views.Forms",
      "confidence": 0.85
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
      "errorType": "crash",
      "errorMessage": "System.ExecutionEngineException: Couldn't create thread. Error 0x0",
      "stackTrace": "System.ExecutionEngineException: Couldn't create thread. Error 0x0\n  at System.Threading.Thread.Thread_internal(...)\n  at SkiaSharp.Views.Android.GLTextureView+GLThread.Start()\n  at SkiaSharp.Views.Android.GLTextureView.SetRenderer(...)\n  at SkiaSharp.Views.Android.SKGLTextureView.Initialize()\n  at SkiaSharp.Views.Forms.SKGLViewRenderer.CreateNativeControl()",
      "reproQuality": "partial",
      "targetFrameworks": [
        "monoandroid6.0"
      ]
    },
    "reproEvidence": {
      "environmentDetails": "Samsung Galaxy J2 Prime, Android 6.0.1, SkiaSharp 2.80.2, Xamarin.Forms"
    },
    "versionAnalysis": {
      "mentionedVersions": [
        "2.80.2"
      ],
      "currentRelevance": "unknown",
      "relevanceReason": "Issue is from 2021 on SkiaSharp 2.80.2; current SkiaSharp version is much newer. No reproduction steps provided; crash not reproducible by reporter."
    }
  },
  "analysis": {
    "summary": "The crash occurs when Android's OS-level thread creation fails (Error 0x0) while SKGLTextureView constructor calls SetRenderer which starts a new Thread for the GL rendering loop. This is an OS-level thread resource exhaustion or device-specific limitation on the Samsung Galaxy J2 Prime running Android 6.0.1, not a bug in SkiaSharp's logic per se, but SkiaSharp does not guard against Thread.Start() failing with an ExecutionEngineException.",
    "codeInvestigation": [
      {
        "file": "source/SkiaSharp.Views/SkiaSharp.Views/Platform/Android/GLTextureView.cs",
        "finding": "GLThread.Start() calls thread.Start() directly with no exception handling around Thread.Start(). If the OS cannot allocate a new thread (ENOMEM or thread limit exceeded), Thread.Start() will throw ExecutionEngineException on Mono/Android.",
        "relevance": "direct"
      },
      {
        "file": "source/SkiaSharp.Views/SkiaSharp.Views/Platform/Android/SKGLTextureView.cs",
        "finding": "SKGLTextureView constructor calls Initialize() which immediately calls SetRenderer(renderer). On Android 6.0.1 on low-memory devices, this thread creation at construction time may fail if the system is under thread pressure.",
        "relevance": "direct"
      }
    ],
    "keySignals": [
      {
        "text": "System.ExecutionEngineException: Couldn't create thread. Error 0x0",
        "source": "issue body - stack trace",
        "interpretation": "The Android kernel rejected Thread creation, likely due to per-process thread limit or memory pressure on the low-end device"
      },
      {
        "text": "Issue was reported by our AppCenter production crash monitoring. Not reproducible as of now.",
        "source": "issue body",
        "interpretation": "Intermittent crash from production, not locally reproducible; likely a resource-exhaustion race condition"
      },
      {
        "text": "Samsung Galaxy J2 Prime / Android 6.0.1",
        "source": "issue body",
        "interpretation": "Low-end device with limited resources and older Android; more prone to thread limit exhaustion"
      }
    ],
    "rationale": "Classified as type/bug in area/SkiaSharp.Views.Forms because the crash occurs at GLTextureView+GLThread.Start() called from Xamarin.Forms SKGLViewRenderer.CreateNativeControl(). The root cause is that the OS cannot create a new thread, which is an OS/device resource issue, but SkiaSharp's GLThread.Start() does not handle this gracefully. The issue is Android-specific and OpenGL backend-specific. Severity is high because it's a hard crash (ExecutionEngineException) with no workaround visible to users, though it appears intermittent and device-specific.",
    "resolution": {
      "hypothesis": "The crash is triggered by OS-level thread exhaustion on the low-end Samsung Galaxy J2 Prime (Android 6.0.1) when SKGLTextureView attempts to start a GL rendering thread. SkiaSharp should handle thread creation failure more gracefully, or callers should be aware to use SKCanvasView (software rasterizer) on resource-constrained devices.",
      "proposals": [
        {
          "title": "Use SKCanvasView instead of SKGLTextureView on low-end Android devices",
          "category": "workaround",
          "effort": "cost/s",
          "validated": "untested",
          "description": "On resource-constrained devices, use the CPU rasterizer (SKCanvasView) rather than the OpenGL-accelerated SKGLView/SKGLTextureView. This avoids spinning up a dedicated GL thread entirely."
        },
        {
          "title": "Investigate adding exception handling in GLThread.Start()",
          "category": "investigation",
          "effort": "cost/m",
          "validated": "untested",
          "description": "GLTextureView.GLThread.Start() could wrap Thread.Start() in a try/catch to handle ExecutionEngineException and propagate a meaningful error rather than crashing."
        }
      ],
      "recommendedProposal": "workaround-1",
      "recommendedReason": "The root cause is OS thread exhaustion on a low-end device; the most practical immediate workaround is to use the software rasterizer on such devices."
    },
    "nextQuestions": [
      "Does the crash still occur with current SkiaSharp (2.88+)?",
      "Can the reporter confirm whether the crash happens consistently or only under memory pressure?",
      "What is the total thread count in the app when this crash occurs?",
      "Does switching to SKCanvasView (CPU rasterizer) resolve the issue on that device?"
    ]
  },
  "output": {
    "actionability": {
      "suggestedAction": "needs-info",
      "confidence": 0.75,
      "reason": "The crash is not reproducible, reported on SkiaSharp 2.80.2 (old version), and appears intermittent/device-specific. More info is needed: can it be reproduced on a current version? Is it consistent or intermittent? No minimal repro provided.",
      "suggestedReproPlatform": "linux"
    },
    "missingInfo": [
      "Can this be reproduced on the latest SkiaSharp version (2.88+)?",
      "Steps or conditions under which the crash reliably occurs (or frequency from AppCenter)",
      "Confirmation whether using SKCanvasView avoids the crash on the same device"
    ],
    "actions": [
      {
        "type": "update-labels",
        "description": "Apply type/bug, area/SkiaSharp.Views.Forms, os/Android, backend/OpenGL, tenet/reliability labels",
        "risk": "low",
        "confidence": 0.9,
        "labels": [
          "type/bug",
          "area/SkiaSharp.Views.Forms",
          "os/Android",
          "backend/OpenGL",
          "tenet/reliability"
        ]
      },
      {
        "type": "add-comment",
        "description": "Ask reporter for more information: current version, reproducibility conditions, and whether SKCanvasView works as a workaround",
        "risk": "medium",
        "confidence": 0.75,
        "comment": "Thank you for reporting this crash!\n\nThis appears to be a thread creation failure at the OS level on your Samsung Galaxy J2 Prime (Android 6.0.1), where the OS returned an error when SkiaSharp tried to create a GL rendering thread. This can happen on low-memory or resource-constrained devices.\n\nCould you help us with the following?\n\n1. **Current version**: Does this still occur with the latest SkiaSharp (2.88+)?\n2. **Reproducibility**: Is this crash consistent, or does it only happen occasionally (e.g., under memory pressure)? The AppCenter report shows intermittent occurrence.\n3. **Workaround**: Does switching from `SKGLView` to `SKCanvasView` (CPU rasterizer) resolve the crash on the same device?\n\nAs a potential **workaround** in the meantime: if your app can detect low-end devices, consider using `SKCanvasView` instead of `SKGLView`/`SKGLTextureView` — this avoids spinning up a dedicated OpenGL thread entirely.\n\nThank you!"
      }
    ]
  }
}
```

</details>
