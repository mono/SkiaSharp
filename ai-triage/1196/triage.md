# Issue Triage Report — #1196

| Field | Value |
|-------|-------|
| Repository | mono/SkiaSharp |
| Analyzed | 2026-04-24T17:30:00Z |
| Type | type/bug (0.85 (85%)) |
| Area | area/SkiaSharp.Views (0.82 (82%)) |
| Suggested action | needs-info (0.88 (88%)) |

**Issue Summary:** Reporter describes a significant rendering performance drop on Android when upgrading from SkiaSharp 1.68.1 to 1.68.2-preview45 in the Mapsui SKGL renderer, particularly when rendering 15 labels simultaneously on low-to-mid-range Xiaomi devices.

**Analysis:** Performance regression on Android SKGL renderer when rendering labels, suspected to be caused by MSAA (multi-sample anti-aliasing) being enabled via the NDK r15→r19 upgrade. No minimal repro was ever provided.

**Recommendations:** **needs-info** — Minimal reproduction was explicitly requested by the maintainer but never provided. The issue concerns a pre-release build from 2020; current relevance is unknown without a repro against modern SkiaSharp.

---

## Classification

| Field | Value |
|-------|-------|
| Type | type/bug |
| Area | area/SkiaSharp.Views |
| Platforms | os/Android |
| Backends | backend/OpenGL |
| Tenets | tenet/performance |
| Partner | — |

## Evidence

### Reproduction

1. Use Mapsui SKGL renderer on Android 9.0 with SkiaSharp 1.68.2-preview45
2. Render a map with 15 labels simultaneously
3. Observe that rendering is extremely choppy vs smooth in 1.68.1

**Environment:** Android 9.0, Xiaomi Redmi 5 Plus and Xiaomi MI A2, SkiaSharp 1.68.2-preview45, Mapsui SKGL renderer

### Bug Signals

| Field | Value |
|-------|-------|
| Severity | medium |
| Regression claimed | True |
| Error type | performance |
| Error message | — |
| Repro quality | none |
| Target frameworks | net-android9.0 |

### Version Analysis

| Field | Value |
|-------|-------|
| Mentioned versions | 1.68.1, 1.68.2-preview45, 1.68.1.1 |
| Worked in | 1.68.1 |
| Broke in | 1.68.2-preview45 |
| Current relevance | unknown |
| Relevance reason | Reported against very old 1.68.2-preview45; unclear if performance gap persists in current 3.x releases. NDK was upgraded from r15 to r19 between those versions, which may have changed EGL/MSAA behavior. |

### Regression

| Field | Value |
|-------|-------|
| Is regression | True |
| Confidence | 0.75 (75%) |
| Reason | Reporter explicitly states the issue started after upgrading from 1.68.1 to 1.68.2-preview45. Maintainer confirmed NDK was upgraded from r15 to r19 between those versions. |
| Worked in version | 1.68.1 |
| Broke in version | 1.68.2-preview45 |

## Analysis

### Technical Summary

Performance regression on Android SKGL renderer when rendering labels, suspected to be caused by MSAA (multi-sample anti-aliasing) being enabled via the NDK r15→r19 upgrade. No minimal repro was ever provided.

### Rationale

The reporter describes a genuine performance regression between two specific SkiaSharp versions, pointing to anti-aliasing as the likely cause. Code investigation shows that SKGLSurfaceViewRenderer and SKGLTextureViewRenderer both query GLES20.GlSamples from the framebuffer and use that sample count when creating the GRBackendRenderTarget. An NDK upgrade from r15 to r19 could result in a different EGL configuration being selected by the driver, potentially enabling MSAA where it was not enabled before. No minimal reproduction was provided despite the maintainer explicitly requesting one; the issue has been stale since 2020.

### Key Signals

- "extremely choppy on preview45...works just fine on 1.68.1" — **issue body** (Clear performance regression correlated with SkiaSharp version upgrade.)
- "Maybe it's because there was no antialiasing in 1.68.1 and it's back in 1.68.2?" — **comment #1 (reporter)** (Reporter suspects anti-aliasing (MSAA) was disabled in 1.68.1 and re-enabled in 1.68.2, causing the performance hit.)
- "The one thing that did change was to upgrade the NDK from r15 to r19" — **comment #2 (maintainer)** (NDK upgrade is the primary architectural change; can affect EGL config selection and MSAA availability.)
- "Send in the repro as soon as you can" — **comment #4 (maintainer)** (Maintainer explicitly requested a minimal repro; none was ever provided.)

### Code Investigation

| File | Lines | Relevance | Finding |
|------|-------|-----------|---------|
| `source/SkiaSharp.Views/SkiaSharp.Views/Platform/Android/SKGLSurfaceViewRenderer.cs` | 50-67 | direct | OnDrawFrame queries GLES20.GlGetIntegerv(GLES20.GlSamples) to read the current framebuffer sample count, then passes it directly to GRBackendRenderTarget. If the NDK/EGL driver reports non-zero samples (MSAA enabled), Skia renders with multisampling, which is significantly slower on low-end GPU hardware. |
| `source/SkiaSharp.Views/SkiaSharp.Views/Platform/Android/SKGLTextureViewRenderer.cs` | 62-78 | direct | Identical sample-count forwarding pattern as SKGLSurfaceViewRenderer — same MSAA exposure risk. Both renderers honor whatever the GL driver reports without capping to 1 unless the device max is lower. |

### Workarounds

- Explicitly set EGLConfigChooser to disable MSAA (0 samples) when constructing the GLSurfaceView to ensure consistent behavior: SetEGLConfigChooser(8, 8, 8, 8, 0, 0).
- Disable anti-aliasing on individual SKPaint objects (paint.IsAntialias = false) to reduce fill cost, though this addresses a symptom not the root cause.

### Next Questions

- Does the same performance drop occur in current SkiaSharp 3.x on Android?
- Can a minimal Mapsui-based or standalone repro reproduce the issue?
- What GL sample count does the EGL context report on the affected Xiaomi devices with NDK r19?
- Is Mapsui doing anything custom with EGL configuration that could affect sample count?

### Resolution Proposals

**Hypothesis:** NDK r19 causes the EGL driver to select a MSAA config where r15 did not, resulting in a multi-sampled framebuffer. The SKGL renderer then reads that sample count and creates a multi-sampled GRBackendRenderTarget, significantly increasing GPU fill cost on low-end Android hardware.

1. **Disable MSAA in EGL config chooser** — workaround, confidence 0.70 (70%), cost/xs, validated=untested
   - When creating GLSurfaceView in Mapsui or in a custom subclass, call SetEGLConfigChooser(8, 8, 8, 8, 0, 0) to request 0 stencil samples (last param), preventing the driver from selecting an MSAA config. This is a workaround the reporter can apply immediately.
2. **Investigation: Reproduce with current SkiaSharp 3.x** — investigation, confidence 0.80 (80%), cost/s, validated=untested
   - Verify whether the performance regression persists in the current SkiaSharp 3.x release to determine if it was fixed as a side-effect of other changes.

**Recommended proposal:** Investigation: Reproduce with current SkiaSharp 3.x

**Why:** The issue was filed against a 2020 preview version; before investing in a fix, confirming the regression still exists in current releases is critical.

## Recommendations

### Actionability

| Field | Value |
|-------|-------|
| Suggested action | needs-info |
| Confidence | 0.88 (88%) |
| Reason | Minimal reproduction was explicitly requested by the maintainer but never provided. The issue concerns a pre-release build from 2020; current relevance is unknown without a repro against modern SkiaSharp. |
| Suggested repro platform | linux |

### Missing Info

- Minimal self-contained repro project (standalone or Mapsui-based) that demonstrates the performance regression
- Confirmation of whether the same performance issue occurs with current SkiaSharp 3.x on the same devices

### Automatable Actions

| Type | Risk | Confidence | Description | Details |
|------|------|------------|-------------|---------|
| update-labels | low | 0.90 (90%) | Apply type/bug, area/SkiaSharp.Views, os/Android, backend/OpenGL, tenet/performance labels | labels=type/bug, area/SkiaSharp.Views, os/Android, backend/OpenGL, tenet/performance |
| add-comment | medium | 0.85 (85%) | Request minimal repro and confirmation that the issue reproduces on current SkiaSharp versions | — |

**Comment draft for `add-comment`:**

```markdown
Thanks for the report! We've been investigating this and the most likely cause is that the NDK upgrade (r15→r19) in 1.68.2 changed the EGL configuration, potentially enabling MSAA (multisampling) on your devices — which is significantly slower on mid-range GPU hardware.

As a potential workaround, if you're subclassing `SKGLSurfaceView`, try setting `SetEGLConfigChooser(8, 8, 8, 8, 0, 0)` in your `Initialize()` override to explicitly disable multisampling.

To help us confirm the root cause and check whether this is still relevant in modern SkiaSharp, could you:
1. Provide a minimal standalone repro project (or a small Mapsui-based test) that demonstrates the slowdown?
2. Confirm whether the same issue occurs with the latest SkiaSharp 3.x release?

Without a repro we're unable to profile or verify a fix. Thanks!
```

<details>
<summary>Raw JSON</summary>

```json
{
  "meta": {
    "schemaVersion": "1.0",
    "number": 1196,
    "repo": "mono/SkiaSharp",
    "analyzedAt": "2026-04-24T17:30:00Z"
  },
  "summary": "Reporter describes a significant rendering performance drop on Android when upgrading from SkiaSharp 1.68.1 to 1.68.2-preview45 in the Mapsui SKGL renderer, particularly when rendering 15 labels simultaneously on low-to-mid-range Xiaomi devices.",
  "classification": {
    "type": {
      "value": "type/bug",
      "confidence": 0.85
    },
    "area": {
      "value": "area/SkiaSharp.Views",
      "confidence": 0.82
    },
    "platforms": [
      "os/Android"
    ],
    "backends": [
      "backend/OpenGL"
    ],
    "tenets": [
      "tenet/performance"
    ]
  },
  "evidence": {
    "bugSignals": {
      "severity": "medium",
      "regressionClaimed": true,
      "errorType": "performance",
      "reproQuality": "none",
      "targetFrameworks": [
        "net-android9.0"
      ]
    },
    "reproEvidence": {
      "stepsToReproduce": [
        "Use Mapsui SKGL renderer on Android 9.0 with SkiaSharp 1.68.2-preview45",
        "Render a map with 15 labels simultaneously",
        "Observe that rendering is extremely choppy vs smooth in 1.68.1"
      ],
      "environmentDetails": "Android 9.0, Xiaomi Redmi 5 Plus and Xiaomi MI A2, SkiaSharp 1.68.2-preview45, Mapsui SKGL renderer",
      "relatedIssues": []
    },
    "versionAnalysis": {
      "mentionedVersions": [
        "1.68.1",
        "1.68.2-preview45",
        "1.68.1.1"
      ],
      "workedIn": "1.68.1",
      "brokeIn": "1.68.2-preview45",
      "currentRelevance": "unknown",
      "relevanceReason": "Reported against very old 1.68.2-preview45; unclear if performance gap persists in current 3.x releases. NDK was upgraded from r15 to r19 between those versions, which may have changed EGL/MSAA behavior."
    },
    "regression": {
      "isRegression": true,
      "confidence": 0.75,
      "reason": "Reporter explicitly states the issue started after upgrading from 1.68.1 to 1.68.2-preview45. Maintainer confirmed NDK was upgraded from r15 to r19 between those versions.",
      "workedInVersion": "1.68.1",
      "brokeInVersion": "1.68.2-preview45"
    }
  },
  "analysis": {
    "summary": "Performance regression on Android SKGL renderer when rendering labels, suspected to be caused by MSAA (multi-sample anti-aliasing) being enabled via the NDK r15→r19 upgrade. No minimal repro was ever provided.",
    "rationale": "The reporter describes a genuine performance regression between two specific SkiaSharp versions, pointing to anti-aliasing as the likely cause. Code investigation shows that SKGLSurfaceViewRenderer and SKGLTextureViewRenderer both query GLES20.GlSamples from the framebuffer and use that sample count when creating the GRBackendRenderTarget. An NDK upgrade from r15 to r19 could result in a different EGL configuration being selected by the driver, potentially enabling MSAA where it was not enabled before. No minimal reproduction was provided despite the maintainer explicitly requesting one; the issue has been stale since 2020.",
    "keySignals": [
      {
        "text": "extremely choppy on preview45...works just fine on 1.68.1",
        "source": "issue body",
        "interpretation": "Clear performance regression correlated with SkiaSharp version upgrade."
      },
      {
        "text": "Maybe it's because there was no antialiasing in 1.68.1 and it's back in 1.68.2?",
        "source": "comment #1 (reporter)",
        "interpretation": "Reporter suspects anti-aliasing (MSAA) was disabled in 1.68.1 and re-enabled in 1.68.2, causing the performance hit."
      },
      {
        "text": "The one thing that did change was to upgrade the NDK from r15 to r19",
        "source": "comment #2 (maintainer)",
        "interpretation": "NDK upgrade is the primary architectural change; can affect EGL config selection and MSAA availability."
      },
      {
        "text": "Send in the repro as soon as you can",
        "source": "comment #4 (maintainer)",
        "interpretation": "Maintainer explicitly requested a minimal repro; none was ever provided."
      }
    ],
    "codeInvestigation": [
      {
        "file": "source/SkiaSharp.Views/SkiaSharp.Views/Platform/Android/SKGLSurfaceViewRenderer.cs",
        "lines": "50-67",
        "finding": "OnDrawFrame queries GLES20.GlGetIntegerv(GLES20.GlSamples) to read the current framebuffer sample count, then passes it directly to GRBackendRenderTarget. If the NDK/EGL driver reports non-zero samples (MSAA enabled), Skia renders with multisampling, which is significantly slower on low-end GPU hardware.",
        "relevance": "direct"
      },
      {
        "file": "source/SkiaSharp.Views/SkiaSharp.Views/Platform/Android/SKGLTextureViewRenderer.cs",
        "lines": "62-78",
        "finding": "Identical sample-count forwarding pattern as SKGLSurfaceViewRenderer — same MSAA exposure risk. Both renderers honor whatever the GL driver reports without capping to 1 unless the device max is lower.",
        "relevance": "direct"
      }
    ],
    "nextQuestions": [
      "Does the same performance drop occur in current SkiaSharp 3.x on Android?",
      "Can a minimal Mapsui-based or standalone repro reproduce the issue?",
      "What GL sample count does the EGL context report on the affected Xiaomi devices with NDK r19?",
      "Is Mapsui doing anything custom with EGL configuration that could affect sample count?"
    ],
    "workarounds": [
      "Explicitly set EGLConfigChooser to disable MSAA (0 samples) when constructing the GLSurfaceView to ensure consistent behavior: SetEGLConfigChooser(8, 8, 8, 8, 0, 0).",
      "Disable anti-aliasing on individual SKPaint objects (paint.IsAntialias = false) to reduce fill cost, though this addresses a symptom not the root cause."
    ],
    "resolution": {
      "hypothesis": "NDK r19 causes the EGL driver to select a MSAA config where r15 did not, resulting in a multi-sampled framebuffer. The SKGL renderer then reads that sample count and creates a multi-sampled GRBackendRenderTarget, significantly increasing GPU fill cost on low-end Android hardware.",
      "proposals": [
        {
          "title": "Disable MSAA in EGL config chooser",
          "description": "When creating GLSurfaceView in Mapsui or in a custom subclass, call SetEGLConfigChooser(8, 8, 8, 8, 0, 0) to request 0 stencil samples (last param), preventing the driver from selecting an MSAA config. This is a workaround the reporter can apply immediately.",
          "category": "workaround",
          "confidence": 0.7,
          "effort": "cost/xs",
          "validated": "untested"
        },
        {
          "title": "Investigation: Reproduce with current SkiaSharp 3.x",
          "description": "Verify whether the performance regression persists in the current SkiaSharp 3.x release to determine if it was fixed as a side-effect of other changes.",
          "category": "investigation",
          "confidence": 0.8,
          "effort": "cost/s",
          "validated": "untested"
        }
      ],
      "recommendedProposal": "Investigation: Reproduce with current SkiaSharp 3.x",
      "recommendedReason": "The issue was filed against a 2020 preview version; before investing in a fix, confirming the regression still exists in current releases is critical."
    }
  },
  "output": {
    "actionability": {
      "suggestedAction": "needs-info",
      "confidence": 0.88,
      "reason": "Minimal reproduction was explicitly requested by the maintainer but never provided. The issue concerns a pre-release build from 2020; current relevance is unknown without a repro against modern SkiaSharp.",
      "suggestedReproPlatform": "linux"
    },
    "missingInfo": [
      "Minimal self-contained repro project (standalone or Mapsui-based) that demonstrates the performance regression",
      "Confirmation of whether the same performance issue occurs with current SkiaSharp 3.x on the same devices"
    ],
    "actions": [
      {
        "type": "update-labels",
        "description": "Apply type/bug, area/SkiaSharp.Views, os/Android, backend/OpenGL, tenet/performance labels",
        "risk": "low",
        "confidence": 0.9,
        "labels": [
          "type/bug",
          "area/SkiaSharp.Views",
          "os/Android",
          "backend/OpenGL",
          "tenet/performance"
        ]
      },
      {
        "type": "add-comment",
        "description": "Request minimal repro and confirmation that the issue reproduces on current SkiaSharp versions",
        "risk": "medium",
        "confidence": 0.85,
        "comment": "Thanks for the report! We've been investigating this and the most likely cause is that the NDK upgrade (r15→r19) in 1.68.2 changed the EGL configuration, potentially enabling MSAA (multisampling) on your devices — which is significantly slower on mid-range GPU hardware.\n\nAs a potential workaround, if you're subclassing `SKGLSurfaceView`, try setting `SetEGLConfigChooser(8, 8, 8, 8, 0, 0)` in your `Initialize()` override to explicitly disable multisampling.\n\nTo help us confirm the root cause and check whether this is still relevant in modern SkiaSharp, could you:\n1. Provide a minimal standalone repro project (or a small Mapsui-based test) that demonstrates the slowdown?\n2. Confirm whether the same issue occurs with the latest SkiaSharp 3.x release?\n\nWithout a repro we're unable to profile or verify a fix. Thanks!"
      }
    ]
  }
}
```

</details>
