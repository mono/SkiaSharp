# Issue Triage Report — #3525

| Field | Value |
|-------|-------|
| Repository | mono/SkiaSharp |
| Analyzed | 2026-05-01T14:41:38Z |
| Type | type/bug (0.97 (97%)) |
| Area | area/SkiaSharp.Views (0.95 (95%)) |
| Suggested action | ready-to-fix (0.95 (95%)) |

**Issue Summary:** SKGLView on macOS reads stencil bits via glGetIntegerv(GL_STENCIL_BITS) which returns 0 on macOS default framebuffers, causing Skia to use the slow DefaultPathRenderer instead of TessellationPathRenderer, resulting in ~5 fps vs expected 60+ fps.

**Analysis:** The root cause is that macOS OpenGL driver lies about stencil bits: glGetIntegerv(GL_STENCIL_BITS) returns 0 for the default framebuffer even when 8 stencil bits are allocated. SKGLView.cs (line 140) and SKGLLayer.cs (line 62) both use this call. Skia then creates a GRBackendRenderTarget with 0 stencil bits, disabling TessellationPathRenderer and forcing the slow DefaultPathRenderer. Fix is confirmed in PR #3546 with 15.6× speedup.

**Recommendations:** **ready-to-fix** — Root cause is fully identified (macOS GL_STENCIL_BITS lie), fix is implemented in PR #3546 with verified 15.6× speedup. Issue can be closed once PR #3546 is merged.

---

## Classification

| Field | Value |
|-------|-------|
| Type | type/bug |
| Area | area/SkiaSharp.Views |
| Platforms | os/macOS |
| Backends | backend/OpenGL |
| Tenets | tenet/performance, tenet/reliability |
| Partner | — |
| Current labels | type/bug |

## Evidence

### Reproduction

1. Create a macOS app using SKGLView
2. Render 40,000+ paths in PaintSurface
3. Observe frame rate of ~5 fps instead of expected 60+ fps

**Environment:** macOS, SkiaSharp 3.116.0, Visual Studio Code

**Repository links:**
- https://github.com/wieslawsoltes/FastSkiaSharp — Benchmark repo with repro
- https://github.com/mattleibow/FastSkiaSharp/tree/main_119 — Fork with matching Skia version
- https://github.com/mono/SkiaSharp/pull/3418 — Initial investigation PR
- https://github.com/mono/SkiaSharp/pull/3546 — Fix PR — one-line stencil bits correction

### Bug Signals

| Field | Value |
|-------|-------|
| Severity | high |
| Regression claimed | False |
| Error type | performance |
| Error message | SKGLView GL rendering: 5.3 fps vs native Skia C++ 61 fps (reported as 120 fps with VSync) |
| Repro quality | complete |
| Target frameworks | net9.0-macos |

### Version Analysis

| Field | Value |
|-------|-------|
| Mentioned versions | 3.116.0 |
| Worked in | — |
| Broke in | — |
| Current relevance | likely |
| Relevance reason | The GL_STENCIL_BITS query bug is present in current source code for both SKGLView.cs and SKGLLayer.cs on macOS. |

### Fix Status

| Field | Value |
|-------|-------|
| Likely fixed | True |
| Confidence | 0.95 (95%) |
| Reason | Maintainer comment on 2026-03-05 identifies exact root cause (macOS glGetIntegerv(GL_STENCIL_BITS) returns 0) and references fix in PR #3546. Fix achieves 77–93 fps, exceeding native C++ GL at 61 fps. |
| Related PRs | #3418, #3546 |
| Related commits | — |
| Fixed in version | — |

## Analysis

### Technical Summary

The root cause is that macOS OpenGL driver lies about stencil bits: glGetIntegerv(GL_STENCIL_BITS) returns 0 for the default framebuffer even when 8 stencil bits are allocated. SKGLView.cs (line 140) and SKGLLayer.cs (line 62) both use this call. Skia then creates a GRBackendRenderTarget with 0 stencil bits, disabling TessellationPathRenderer and forcing the slow DefaultPathRenderer. Fix is confirmed in PR #3546 with 15.6× speedup.

### Rationale

Type/bug because there is a concrete wrong-output (performance) caused by a code defect in the macOS GL stencil detection. Area is SkiaSharp.Views because the bug is in SKGLView/SKGLLayer. Platform is macOS because the glGetIntegerv(GL_STENCIL_BITS) lie is a macOS-specific GL driver quirk. Metal backend is unaffected (matched native C++ exactly). Fix is identified and in PR #3546.

### Key Signals

- "glGetIntegerv(GL_STENCIL_BITS) returns 0 on macOS default framebuffers — a known driver quirk" — **issue comment by mattleibow, 2026-03-05** (The stencil bit count is misreported, causing Skia path renderer selection to degrade to the slow fallback.)
- "GL (40k paths) Before: 5.3 fps, After: 77–93 fps, Native C++: 61 fps" — **issue comment by mattleibow, 2026-03-05** (One-line fix produces 15.6× speedup and surpasses native C++ GL performance.)
- "Metal had zero overhead — SKMetalView matched native C++ Metal frame-for-frame" — **issue comment by mattleibow, 2026-03-05** (The performance gap is purely in the GL path; Metal and .NET overhead are not factors.)

### Code Investigation

| File | Lines | Relevance | Finding |
|------|-------|-----------|---------|
| `source/SkiaSharp.Views/SkiaSharp.Views/Platform/macOS/SKGLView.cs` | 139-154 | direct | Gles.glGetIntegerv(Gles.GL_STENCIL_BITS, out var stencil) at line 140 returns 0 on macOS default framebuffers. This 0 is passed to GRBackendRenderTarget constructor, so Skia creates a render target with no stencil, disabling TessellationPathRenderer. |
| `source/SkiaSharp.Views/SkiaSharp.Views/Platform/macOS/SKGLLayer.cs` | 61-76 | direct | Same GL_STENCIL_BITS query pattern at line 62 in SKGLLayer — this layer variant has the identical bug. |
| `source/SkiaSharp.Views/SkiaSharp.Views/Platform/macOS/SKGLView.cs` | 66-81 | related | NSOpenGLPixelFormat is configured with StencilSize=8 at line 76, confirming 8 stencil bits ARE requested and allocated — the glGetIntegerv query just returns wrong value at runtime on macOS. |

### Next Questions

- Is the fix in PR #3546 merged? If so, this can be closed as fixed.
- Does the same GL_STENCIL_BITS lie affect iOS or other Apple platforms?

### Resolution Proposals

**Hypothesis:** macOS OpenGL driver returns 0 from glGetIntegerv(GL_STENCIL_BITS) for the default framebuffer as a known driver quirk. The fix is to read stencil bits from NSOpenGLPixelFormat (as native Skia does in GLWindowContext_mac.mm:127) instead of via glGetIntegerv.

1. **Read stencil bits from NSOpenGLPixelFormat** — fix, confidence 0.95 (95%), cost/xs, validated=untested
   - Instead of glGetIntegerv(GL_STENCIL_BITS), query the stencil attribute from the NSOpenGLPixelFormat — the same approach used by native Skia in GLWindowContext_mac.mm. This is in PR #3546.

**Recommended proposal:** Read stencil bits from NSOpenGLPixelFormat

**Why:** Already implemented in PR #3546, confirmed 15.6× speedup to 77–93 fps, surpassing native C++ GL baseline of 61 fps.

## Recommendations

### Actionability

| Field | Value |
|-------|-------|
| Suggested action | ready-to-fix |
| Confidence | 0.95 (95%) |
| Reason | Root cause is fully identified (macOS GL_STENCIL_BITS lie), fix is implemented in PR #3546 with verified 15.6× speedup. Issue can be closed once PR #3546 is merged. |
| Suggested repro platform | macos |

### Automatable Actions

| Type | Risk | Confidence | Description | Details |
|------|------|------------|-------------|---------|
| update-labels | low | 0.97 (97%) | Apply bug, views, macOS, OpenGL, performance labels | labels=type/bug, area/SkiaSharp.Views, os/macOS, backend/OpenGL, tenet/performance, tenet/reliability |
| link-related | low | 0.90 (90%) | Cross-reference investigation PR #3418 | linkedIssue=#3418 |
| link-related | low | 0.95 (95%) | Cross-reference fix PR #3546 | linkedIssue=#3546 |

<details>
<summary>Raw JSON</summary>

```json
{
  "meta": {
    "schemaVersion": "1.0",
    "number": 3525,
    "repo": "mono/SkiaSharp",
    "analyzedAt": "2026-05-01T14:41:38Z",
    "currentLabels": [
      "type/bug"
    ]
  },
  "summary": "SKGLView on macOS reads stencil bits via glGetIntegerv(GL_STENCIL_BITS) which returns 0 on macOS default framebuffers, causing Skia to use the slow DefaultPathRenderer instead of TessellationPathRenderer, resulting in ~5 fps vs expected 60+ fps.",
  "classification": {
    "type": {
      "value": "type/bug",
      "confidence": 0.97
    },
    "area": {
      "value": "area/SkiaSharp.Views",
      "confidence": 0.95
    },
    "platforms": [
      "os/macOS"
    ],
    "backends": [
      "backend/OpenGL"
    ],
    "tenets": [
      "tenet/performance",
      "tenet/reliability"
    ]
  },
  "evidence": {
    "bugSignals": {
      "severity": "high",
      "regressionClaimed": false,
      "errorType": "performance",
      "errorMessage": "SKGLView GL rendering: 5.3 fps vs native Skia C++ 61 fps (reported as 120 fps with VSync)",
      "reproQuality": "complete",
      "targetFrameworks": [
        "net9.0-macos"
      ]
    },
    "reproEvidence": {
      "stepsToReproduce": [
        "Create a macOS app using SKGLView",
        "Render 40,000+ paths in PaintSurface",
        "Observe frame rate of ~5 fps instead of expected 60+ fps"
      ],
      "environmentDetails": "macOS, SkiaSharp 3.116.0, Visual Studio Code",
      "repoLinks": [
        {
          "url": "https://github.com/wieslawsoltes/FastSkiaSharp",
          "description": "Benchmark repo with repro"
        },
        {
          "url": "https://github.com/mattleibow/FastSkiaSharp/tree/main_119",
          "description": "Fork with matching Skia version"
        },
        {
          "url": "https://github.com/mono/SkiaSharp/pull/3418",
          "description": "Initial investigation PR"
        },
        {
          "url": "https://github.com/mono/SkiaSharp/pull/3546",
          "description": "Fix PR — one-line stencil bits correction"
        }
      ]
    },
    "versionAnalysis": {
      "mentionedVersions": [
        "3.116.0"
      ],
      "currentRelevance": "likely",
      "relevanceReason": "The GL_STENCIL_BITS query bug is present in current source code for both SKGLView.cs and SKGLLayer.cs on macOS."
    },
    "fixStatus": {
      "likelyFixed": true,
      "confidence": 0.95,
      "reason": "Maintainer comment on 2026-03-05 identifies exact root cause (macOS glGetIntegerv(GL_STENCIL_BITS) returns 0) and references fix in PR #3546. Fix achieves 77–93 fps, exceeding native C++ GL at 61 fps.",
      "relatedPRs": [
        3418,
        3546
      ]
    }
  },
  "analysis": {
    "summary": "The root cause is that macOS OpenGL driver lies about stencil bits: glGetIntegerv(GL_STENCIL_BITS) returns 0 for the default framebuffer even when 8 stencil bits are allocated. SKGLView.cs (line 140) and SKGLLayer.cs (line 62) both use this call. Skia then creates a GRBackendRenderTarget with 0 stencil bits, disabling TessellationPathRenderer and forcing the slow DefaultPathRenderer. Fix is confirmed in PR #3546 with 15.6× speedup.",
    "rationale": "Type/bug because there is a concrete wrong-output (performance) caused by a code defect in the macOS GL stencil detection. Area is SkiaSharp.Views because the bug is in SKGLView/SKGLLayer. Platform is macOS because the glGetIntegerv(GL_STENCIL_BITS) lie is a macOS-specific GL driver quirk. Metal backend is unaffected (matched native C++ exactly). Fix is identified and in PR #3546.",
    "keySignals": [
      {
        "text": "glGetIntegerv(GL_STENCIL_BITS) returns 0 on macOS default framebuffers — a known driver quirk",
        "source": "issue comment by mattleibow, 2026-03-05",
        "interpretation": "The stencil bit count is misreported, causing Skia path renderer selection to degrade to the slow fallback."
      },
      {
        "text": "GL (40k paths) Before: 5.3 fps, After: 77–93 fps, Native C++: 61 fps",
        "source": "issue comment by mattleibow, 2026-03-05",
        "interpretation": "One-line fix produces 15.6× speedup and surpasses native C++ GL performance."
      },
      {
        "text": "Metal had zero overhead — SKMetalView matched native C++ Metal frame-for-frame",
        "source": "issue comment by mattleibow, 2026-03-05",
        "interpretation": "The performance gap is purely in the GL path; Metal and .NET overhead are not factors."
      }
    ],
    "codeInvestigation": [
      {
        "file": "source/SkiaSharp.Views/SkiaSharp.Views/Platform/macOS/SKGLView.cs",
        "lines": "139-154",
        "finding": "Gles.glGetIntegerv(Gles.GL_STENCIL_BITS, out var stencil) at line 140 returns 0 on macOS default framebuffers. This 0 is passed to GRBackendRenderTarget constructor, so Skia creates a render target with no stencil, disabling TessellationPathRenderer.",
        "relevance": "direct"
      },
      {
        "file": "source/SkiaSharp.Views/SkiaSharp.Views/Platform/macOS/SKGLLayer.cs",
        "lines": "61-76",
        "finding": "Same GL_STENCIL_BITS query pattern at line 62 in SKGLLayer — this layer variant has the identical bug.",
        "relevance": "direct"
      },
      {
        "file": "source/SkiaSharp.Views/SkiaSharp.Views/Platform/macOS/SKGLView.cs",
        "lines": "66-81",
        "finding": "NSOpenGLPixelFormat is configured with StencilSize=8 at line 76, confirming 8 stencil bits ARE requested and allocated — the glGetIntegerv query just returns wrong value at runtime on macOS.",
        "relevance": "related"
      }
    ],
    "nextQuestions": [
      "Is the fix in PR #3546 merged? If so, this can be closed as fixed.",
      "Does the same GL_STENCIL_BITS lie affect iOS or other Apple platforms?"
    ],
    "resolution": {
      "hypothesis": "macOS OpenGL driver returns 0 from glGetIntegerv(GL_STENCIL_BITS) for the default framebuffer as a known driver quirk. The fix is to read stencil bits from NSOpenGLPixelFormat (as native Skia does in GLWindowContext_mac.mm:127) instead of via glGetIntegerv.",
      "proposals": [
        {
          "title": "Read stencil bits from NSOpenGLPixelFormat",
          "description": "Instead of glGetIntegerv(GL_STENCIL_BITS), query the stencil attribute from the NSOpenGLPixelFormat — the same approach used by native Skia in GLWindowContext_mac.mm. This is in PR #3546.",
          "category": "fix",
          "confidence": 0.95,
          "effort": "cost/xs",
          "validated": "untested"
        }
      ],
      "recommendedProposal": "Read stencil bits from NSOpenGLPixelFormat",
      "recommendedReason": "Already implemented in PR #3546, confirmed 15.6× speedup to 77–93 fps, surpassing native C++ GL baseline of 61 fps."
    }
  },
  "output": {
    "actionability": {
      "suggestedAction": "ready-to-fix",
      "confidence": 0.95,
      "reason": "Root cause is fully identified (macOS GL_STENCIL_BITS lie), fix is implemented in PR #3546 with verified 15.6× speedup. Issue can be closed once PR #3546 is merged.",
      "suggestedReproPlatform": "macos"
    },
    "actions": [
      {
        "type": "update-labels",
        "description": "Apply bug, views, macOS, OpenGL, performance labels",
        "risk": "low",
        "confidence": 0.97,
        "labels": [
          "type/bug",
          "area/SkiaSharp.Views",
          "os/macOS",
          "backend/OpenGL",
          "tenet/performance",
          "tenet/reliability"
        ]
      },
      {
        "type": "link-related",
        "description": "Cross-reference investigation PR #3418",
        "risk": "low",
        "confidence": 0.9,
        "linkedIssue": 3418
      },
      {
        "type": "link-related",
        "description": "Cross-reference fix PR #3546",
        "risk": "low",
        "confidence": 0.95,
        "linkedIssue": 3546
      }
    ]
  }
}
```

</details>
