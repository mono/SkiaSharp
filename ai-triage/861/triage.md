# Issue Triage Report — #861

| Field | Value |
|-------|-------|
| Repository | mono/SkiaSharp |
| Analyzed | 2026-05-01T08:31:07Z |
| Type | type/bug (0.90 (90%)) |
| Area | area/SkiaSharp.Views.Forms (0.85 (85%)) |
| Suggested action | needs-info (0.85 (85%)) |

**Issue Summary:** Fatal SIGSEGV crash (signal 11, fault addr 0x20) occurs when disposing an SKCanvasView on a physical Android device (Redmi Note 6 Pro) running SkiaSharp 1.68 with Xamarin.Forms 3.6.

**Analysis:** The crash is a SIGSEGV (fault addr 0x20, a near-null dereference) occurring during disposal of SKCanvasView on a physical Android device. The Dispose path in SKCanvasView calls SurfaceFactory.Dispose() which calls FreeBitmap(). A possible race condition exists if OnDraw is executing concurrently with Dispose — specifically, if bitmap is freed while LockPixels is still active or the SKSurface created from locked bitmap pixels is being used. The bitmap field is not protected by any lock. No repro code was supplied.

**Recommendations:** **needs-info** — SIGSEGV crash reported on Android with no repro code. Maintainer already asked for code snippet; reporter never replied. Issue needs repro steps or code before investigation can proceed.

---

## Classification

| Field | Value |
|-------|-------|
| Type | type/bug |
| Area | area/SkiaSharp.Views.Forms |
| Platforms | os/Android |
| Backends | — |
| Tenets | tenet/reliability |
| Partner | — |

## Evidence

### Reproduction

**Repository links:**
- https://github.com/mono/SkiaSharp/issues/1733 — Related SIGSEGV native crash on Android with libSkiaSharp.so

### Bug Signals

| Field | Value |
|-------|-------|
| Severity | high |
| Regression claimed | False |
| Error type | crash |
| Error message | Fatal signal 11 (SIGSEGV), code 1, fault addr 0x20 in tid 19915 |
| Repro quality | partial |
| Target frameworks | Xamarin.Android |

**Stack trace:**

```text
libc Fatal signal 11 (SIGSEGV), code 1, fault addr 0x20 in tid 19915 (begroup.MyGlobe)
```

### Version Analysis

| Field | Value |
|-------|-------|
| Mentioned versions | 1.68 |
| Worked in | — |
| Broke in | — |
| Current relevance | unknown |
| Relevance reason | Issue was filed against v1.68; no repro code was provided and no maintainer confirmed a fix. |

## Analysis

### Technical Summary

The crash is a SIGSEGV (fault addr 0x20, a near-null dereference) occurring during disposal of SKCanvasView on a physical Android device. The Dispose path in SKCanvasView calls SurfaceFactory.Dispose() which calls FreeBitmap(). A possible race condition exists if OnDraw is executing concurrently with Dispose — specifically, if bitmap is freed while LockPixels is still active or the SKSurface created from locked bitmap pixels is being used. The bitmap field is not protected by any lock. No repro code was supplied.

### Rationale

Classified as type/bug with high severity because the crash is a hard SIGSEGV with no workaround and occurs on real devices. Area is SkiaSharp.Views.Forms because the stack trace shows SKCanvasViewRenderer (Xamarin.Forms renderer). The code investigation reveals a plausible race condition in SurfaceFactory between bitmap disposal and active draw calls, which aligns with the fault address 0x20 (offset into a freed struct). suggestedAction is needs-info because no minimal repro code was provided and the maintainer already asked for one.

### Key Signals

- "Fatal signal 11 (SIGSEGV), code 1, fault addr 0x20 in tid 19915" — **issue body - logcat output** (Near-null pointer dereference in the main app thread during view disposal.)
- "It works well on simulator." — **issue body** (Timing-dependent or hardware-specific; emulator does not reproduce, suggesting a race condition that surfaces under real device scheduling.)
- "Do you have any more information? A code snippet?" — **maintainer comment #504809459** (Maintainer requested more context; reporter never followed up.)

### Code Investigation

| File | Lines | Relevance | Finding |
|------|-------|-----------|---------|
| `source/SkiaSharp.Views/SkiaSharp.Views/Platform/Android/SKCanvasView.cs` | 134-154 | direct | Dispose(bool) calls surfaceFactory.Dispose() directly. OnDetachedFromWindow also calls surfaceFactory.Dispose(). Both paths can race with an in-progress OnDraw call. No synchronization is present. |
| `source/SkiaSharp.Views/SkiaSharp.Views/Platform/Android/SurfaceFactory.cs` | 43-86 | direct | FreeBitmap() recycles and disposes the Android Bitmap. CreateSurface() calls bitmap.LockPixels() and returns an SKSurface wrapping the locked memory. If FreeBitmap() is called while the bitmap is locked and SKSurface is still in use, the next access to the recycled/freed bitmap memory causes a SIGSEGV. |

### Next Questions

- What code triggers the disposal of SKCanvasView — is it inside a navigation event or fragment lifecycle?
- Is the crash 100% reproducible or intermittent?
- Does upgrading to a newer SkiaSharp version (2.x) resolve the issue?

### Resolution Proposals

1. **Request minimal repro code from reporter** — investigation, cost/xs, validated=untested
   - Ask reporter to share code showing how they add and remove the SKCanvasView from the UI and where Dispose is called.

**Recommended proposal:** p1

**Why:** No repro code was provided; without it the bug cannot be confirmed or investigated further.

## Recommendations

### Actionability

| Field | Value |
|-------|-------|
| Suggested action | needs-info |
| Confidence | 0.85 (85%) |
| Reason | SIGSEGV crash reported on Android with no repro code. Maintainer already asked for code snippet; reporter never replied. Issue needs repro steps or code before investigation can proceed. |
| Suggested repro platform | linux |

### Missing Info

- Minimal reproducible code showing how SKCanvasView is added and disposed
- Full stack trace or tombstone file from the crash
- Confirmation whether the crash is reproducible with a newer SkiaSharp version

### Automatable Actions

| Type | Risk | Confidence | Description | Details |
|------|------|------------|-------------|---------|
| update-labels | low | 0.90 (90%) | Apply type/bug, area/SkiaSharp.Views.Forms, os/Android labels | labels=type/bug, area/SkiaSharp.Views.Forms, os/Android |
| add-comment | medium | 0.85 (85%) | Ask reporter for minimal repro code and confirm if still reproducible on newer SkiaSharp | — |

**Comment draft for `add-comment`:**

```markdown
Thank you for reporting this crash. To investigate further, could you please provide:

1. A minimal code snippet showing how you add and dispose `SKCanvasView` (e.g., within a page lifecycle or navigation event).
2. A tombstone or full native stack trace if available from the device.
3. Does the same crash occur with a more recent version of SkiaSharp (2.x)?

Without a reproducible case, we're unable to diagnose the root cause of the SIGSEGV.
```

<details>
<summary>Raw JSON</summary>

```json
{
  "meta": {
    "schemaVersion": "1.0",
    "number": 861,
    "repo": "mono/SkiaSharp",
    "analyzedAt": "2026-05-01T08:31:07Z"
  },
  "summary": "Fatal SIGSEGV crash (signal 11, fault addr 0x20) occurs when disposing an SKCanvasView on a physical Android device (Redmi Note 6 Pro) running SkiaSharp 1.68 with Xamarin.Forms 3.6.",
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
    "tenets": [
      "tenet/reliability"
    ]
  },
  "evidence": {
    "bugSignals": {
      "severity": "high",
      "regressionClaimed": false,
      "errorType": "crash",
      "errorMessage": "Fatal signal 11 (SIGSEGV), code 1, fault addr 0x20 in tid 19915",
      "stackTrace": "libc Fatal signal 11 (SIGSEGV), code 1, fault addr 0x20 in tid 19915 (begroup.MyGlobe)",
      "reproQuality": "partial",
      "targetFrameworks": [
        "Xamarin.Android"
      ]
    },
    "reproEvidence": {
      "codeSnippets": [],
      "repoLinks": [
        {
          "url": "https://github.com/mono/SkiaSharp/issues/1733",
          "description": "Related SIGSEGV native crash on Android with libSkiaSharp.so"
        }
      ]
    },
    "versionAnalysis": {
      "mentionedVersions": [
        "1.68"
      ],
      "currentRelevance": "unknown",
      "relevanceReason": "Issue was filed against v1.68; no repro code was provided and no maintainer confirmed a fix."
    }
  },
  "analysis": {
    "summary": "The crash is a SIGSEGV (fault addr 0x20, a near-null dereference) occurring during disposal of SKCanvasView on a physical Android device. The Dispose path in SKCanvasView calls SurfaceFactory.Dispose() which calls FreeBitmap(). A possible race condition exists if OnDraw is executing concurrently with Dispose — specifically, if bitmap is freed while LockPixels is still active or the SKSurface created from locked bitmap pixels is being used. The bitmap field is not protected by any lock. No repro code was supplied.",
    "codeInvestigation": [
      {
        "file": "source/SkiaSharp.Views/SkiaSharp.Views/Platform/Android/SKCanvasView.cs",
        "finding": "Dispose(bool) calls surfaceFactory.Dispose() directly. OnDetachedFromWindow also calls surfaceFactory.Dispose(). Both paths can race with an in-progress OnDraw call. No synchronization is present.",
        "relevance": "direct",
        "lines": "134-154"
      },
      {
        "file": "source/SkiaSharp.Views/SkiaSharp.Views/Platform/Android/SurfaceFactory.cs",
        "finding": "FreeBitmap() recycles and disposes the Android Bitmap. CreateSurface() calls bitmap.LockPixels() and returns an SKSurface wrapping the locked memory. If FreeBitmap() is called while the bitmap is locked and SKSurface is still in use, the next access to the recycled/freed bitmap memory causes a SIGSEGV.",
        "relevance": "direct",
        "lines": "43-86"
      }
    ],
    "keySignals": [
      {
        "text": "Fatal signal 11 (SIGSEGV), code 1, fault addr 0x20 in tid 19915",
        "source": "issue body - logcat output",
        "interpretation": "Near-null pointer dereference in the main app thread during view disposal."
      },
      {
        "text": "It works well on simulator.",
        "source": "issue body",
        "interpretation": "Timing-dependent or hardware-specific; emulator does not reproduce, suggesting a race condition that surfaces under real device scheduling."
      },
      {
        "text": "Do you have any more information? A code snippet?",
        "source": "maintainer comment #504809459",
        "interpretation": "Maintainer requested more context; reporter never followed up."
      }
    ],
    "rationale": "Classified as type/bug with high severity because the crash is a hard SIGSEGV with no workaround and occurs on real devices. Area is SkiaSharp.Views.Forms because the stack trace shows SKCanvasViewRenderer (Xamarin.Forms renderer). The code investigation reveals a plausible race condition in SurfaceFactory between bitmap disposal and active draw calls, which aligns with the fault address 0x20 (offset into a freed struct). suggestedAction is needs-info because no minimal repro code was provided and the maintainer already asked for one.",
    "resolution": {
      "proposals": [
        {
          "title": "Request minimal repro code from reporter",
          "description": "Ask reporter to share code showing how they add and remove the SKCanvasView from the UI and where Dispose is called.",
          "category": "investigation",
          "effort": "cost/xs",
          "validated": "untested"
        }
      ],
      "recommendedProposal": "p1",
      "recommendedReason": "No repro code was provided; without it the bug cannot be confirmed or investigated further."
    },
    "nextQuestions": [
      "What code triggers the disposal of SKCanvasView — is it inside a navigation event or fragment lifecycle?",
      "Is the crash 100% reproducible or intermittent?",
      "Does upgrading to a newer SkiaSharp version (2.x) resolve the issue?"
    ]
  },
  "output": {
    "actionability": {
      "suggestedAction": "needs-info",
      "confidence": 0.85,
      "reason": "SIGSEGV crash reported on Android with no repro code. Maintainer already asked for code snippet; reporter never replied. Issue needs repro steps or code before investigation can proceed.",
      "suggestedReproPlatform": "linux"
    },
    "missingInfo": [
      "Minimal reproducible code showing how SKCanvasView is added and disposed",
      "Full stack trace or tombstone file from the crash",
      "Confirmation whether the crash is reproducible with a newer SkiaSharp version"
    ],
    "actions": [
      {
        "type": "update-labels",
        "description": "Apply type/bug, area/SkiaSharp.Views.Forms, os/Android labels",
        "risk": "low",
        "confidence": 0.9,
        "labels": [
          "type/bug",
          "area/SkiaSharp.Views.Forms",
          "os/Android"
        ]
      },
      {
        "type": "add-comment",
        "description": "Ask reporter for minimal repro code and confirm if still reproducible on newer SkiaSharp",
        "risk": "medium",
        "confidence": 0.85,
        "comment": "Thank you for reporting this crash. To investigate further, could you please provide:\n\n1. A minimal code snippet showing how you add and dispose `SKCanvasView` (e.g., within a page lifecycle or navigation event).\n2. A tombstone or full native stack trace if available from the device.\n3. Does the same crash occur with a more recent version of SkiaSharp (2.x)?\n\nWithout a reproducible case, we're unable to diagnose the root cause of the SIGSEGV."
      }
    ]
  }
}
```

</details>
