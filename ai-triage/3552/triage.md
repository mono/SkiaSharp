# Issue Triage Report — #3552

| Field | Value |
|-------|-------|
| Repository | mono/SkiaSharp |
| Analyzed | 2026-07-05T05:30:00Z |
| Type | type/bug (0.98 (98%)) |
| Area | area/SkiaSharp.Views (0.95 (95%)) |
| Suggested action | close-as-duplicate (0.98 (98%)) |

**Issue Summary:** Duplicate of #3525 — identical report of SkiaSharp GL rendering running much slower than native Skia C++ on macOS, caused by macOS GL driver returning 0 stencil bits from glGetIntegerv, disabling TessellationPathRenderer. Root cause identified and fix is in PR #3546.

**Analysis:** Duplicate of #3525. Root cause: macOS GL driver returns 0 from glGetIntegerv(GL_STENCIL_BITS) for default framebuffers (a known driver quirk). SKGLView and SKGLLayer both use this call to construct GRBackendRenderTarget with stencil=0, which disables Skia's TessellationPathRenderer, forcing fallback to DefaultPathRenderer that CPU-tessellates every path with ~40k individual GL draw calls. Native Skia C++ works around this by reading stencil from NSOpenGLPixelFormat. Fix in PR #3546 achieves 15.6x speedup (5.3 fps → 77-93 fps). Metal backend was unaffected.

**Recommendations:** **close-as-duplicate** — Identical report as #3525 by the same author, filed 9 days later. Root cause found (macOS glGetIntegerv(GL_STENCIL_BITS) returns 0) and fix in open PR #3546 achieves 15.6x speedup.

---

## Classification

| Field | Value |
|-------|-------|
| Type | type/bug |
| Area | area/SkiaSharp.Views |
| Platforms | os/macOS |
| Backends | backend/OpenGL |
| Tenets | tenet/performance |
| Partner | — |

## Evidence

### Reproduction

1. Run SkiaSharp GL rendering on macOS using SKGLView
2. Observe fps dramatically lower (~5 fps) compared to native Skia C++ (~120 fps)
3. See benchmark repo: https://github.com/wieslawsoltes/FastSkiaSharp

**Environment:** macOS, SkiaSharp 3.116.0, Visual Studio Code

**Related issues:** #3525

**Repository links:**
- https://github.com/wieslawsoltes/FastSkiaSharp — Benchmark repository demonstrating the performance gap
- https://github.com/mattleibow/FastSkiaSharp/tree/main_119 — Fork with downgraded Skia matching SkiaSharp version
- https://github.com/mono/SkiaSharp/pull/3546 — Fix PR: read stencil bits from NSOpenGLPixelFormat instead of glGetIntegerv

### Bug Signals

| Field | Value |
|-------|-------|
| Severity | high |
| Regression claimed | — |
| Error type | performance |
| Error message | — |
| Repro quality | complete |
| Target frameworks | — |

### Version Analysis

| Field | Value |
|-------|-------|
| Mentioned versions | 3.116.0 |
| Worked in | — |
| Broke in | — |
| Current relevance | unlikely |
| Relevance reason | Root cause identified in issue #3525 and fix is in open PR #3546. After the fix, GL performance reaches 77-93 fps, exceeding native C++ GL at 61 fps. |

### Fix Status

| Field | Value |
|-------|-------|
| Likely fixed | True |
| Confidence | 0.95 (95%) |
| Reason | PR #3546 fixes the exact root cause: reads stencil bits from NSOpenGLPixelFormat.GetValue() instead of glGetIntegerv(GL_STENCIL_BITS) which returns 0 on macOS default framebuffers, causing Skia to disable TessellationPathRenderer. |
| Related PRs | #3546 |
| Related commits | — |
| Fixed in version | — |

## Analysis

### Technical Summary

Duplicate of #3525. Root cause: macOS GL driver returns 0 from glGetIntegerv(GL_STENCIL_BITS) for default framebuffers (a known driver quirk). SKGLView and SKGLLayer both use this call to construct GRBackendRenderTarget with stencil=0, which disables Skia's TessellationPathRenderer, forcing fallback to DefaultPathRenderer that CPU-tessellates every path with ~40k individual GL draw calls. Native Skia C++ works around this by reading stencil from NSOpenGLPixelFormat. Fix in PR #3546 achieves 15.6x speedup (5.3 fps → 77-93 fps). Metal backend was unaffected.

### Rationale

Issue #3552 is a duplicate of #3525, filed 9 days later by the same author (mattleibow) with nearly identical body content. A detailed investigation comment was posted on #3525 on the same day this issue was filed, identifying the root cause as a macOS-specific GL driver quirk and pointing to PR #3546 as the fix. The problematic code path (glGetIntegerv(GL_STENCIL_BITS)) is confirmed in both SKGLView.cs:140 and SKGLLayer.cs:62.

### Key Signals

- "C++ version achieves around 120 fps while the managed SkiaSharp code achieves less than 10 fps" — **issue body** (Large ~12x+ performance gap on macOS with GL rendering path)
- "up to 40 fps with new OpenGL flags" — **issue body** (GL config changes partially help, confirming the issue is in GL rendering setup, not in the Skia drawing commands themselves)
- "Same title, same body, same author as #3525 filed 9 days earlier" — **GitHub issue comparison** (Duplicate issue — same reporter filed the same bug twice)

### Code Investigation

| File | Lines | Relevance | Finding |
|------|-------|-----------|---------|
| `source/SkiaSharp.Views/SkiaSharp.Views/Platform/macOS/SKGLView.cs` | 139-154 | direct | DrawRect reads stencil bits via Gles.glGetIntegerv(Gles.GL_STENCIL_BITS, out var stencil) which returns 0 on macOS default framebuffers. GRBackendRenderTarget is then created with stencil=0, disabling TessellationPathRenderer in Skia and forcing ~40k CPU-tessellated GL draw calls per frame. |
| `source/SkiaSharp.Views/SkiaSharp.Views/Platform/macOS/SKGLLayer.cs` | 61-76 | direct | SKGLLayer.DrawInCGLContext has the same pattern: Gles.glGetIntegerv(Gles.GL_STENCIL_BITS, out var stencil) followed by new GRBackendRenderTarget(..., stencil, ...). Both macOS GL views are affected by the same macOS driver quirk. |
| `source/SkiaSharp.Views/SkiaSharp.Views/Platform/Apple/SKMetalView.cs` | 139-178 | context | SKMetalView uses Metal backend and creates render targets without any stencil query — Metal path is unaffected by the macOS GL driver quirk and performance already matches native C++. |

### Next Questions

- Does PR #3546 also fix the same stencil reading issue in SKGLLayer.cs or only SKGLView.cs?

## Recommendations

### Actionability

| Field | Value |
|-------|-------|
| Suggested action | close-as-duplicate |
| Confidence | 0.98 (98%) |
| Reason | Identical report as #3525 by the same author, filed 9 days later. Root cause found (macOS glGetIntegerv(GL_STENCIL_BITS) returns 0) and fix in open PR #3546 achieves 15.6x speedup. |
| Suggested repro platform | macos |

### Automatable Actions

| Type | Risk | Confidence | Description | Details |
|------|------|------------|-------------|---------|
| update-labels | low | 0.98 (98%) | Apply bug, views area, macOS platform, OpenGL backend, and performance tenet labels | labels=type/bug, area/SkiaSharp.Views, os/macOS, backend/OpenGL, tenet/performance |
| link-duplicate | medium | 0.98 (98%) | Mark as duplicate of #3525 | linkedIssue=#3525 |
| close-issue | medium | 0.95 (95%) | Close as duplicate of #3525 where the root cause and fix are tracked | stateReason=not_planned |

<details>
<summary>Raw JSON</summary>

```json
{
  "meta": {
    "schemaVersion": "1.0",
    "number": 3552,
    "repo": "mono/SkiaSharp",
    "analyzedAt": "2026-07-05T05:30:00Z"
  },
  "summary": "Duplicate of #3525 — identical report of SkiaSharp GL rendering running much slower than native Skia C++ on macOS, caused by macOS GL driver returning 0 stencil bits from glGetIntegerv, disabling TessellationPathRenderer. Root cause identified and fix is in PR #3546.",
  "classification": {
    "type": {
      "value": "type/bug",
      "confidence": 0.98
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
      "tenet/performance"
    ]
  },
  "evidence": {
    "bugSignals": {
      "severity": "high",
      "errorType": "performance",
      "reproQuality": "complete"
    },
    "reproEvidence": {
      "stepsToReproduce": [
        "Run SkiaSharp GL rendering on macOS using SKGLView",
        "Observe fps dramatically lower (~5 fps) compared to native Skia C++ (~120 fps)",
        "See benchmark repo: https://github.com/wieslawsoltes/FastSkiaSharp"
      ],
      "environmentDetails": "macOS, SkiaSharp 3.116.0, Visual Studio Code",
      "relatedIssues": [
        3525
      ],
      "repoLinks": [
        {
          "url": "https://github.com/wieslawsoltes/FastSkiaSharp",
          "description": "Benchmark repository demonstrating the performance gap"
        },
        {
          "url": "https://github.com/mattleibow/FastSkiaSharp/tree/main_119",
          "description": "Fork with downgraded Skia matching SkiaSharp version"
        },
        {
          "url": "https://github.com/mono/SkiaSharp/pull/3546",
          "description": "Fix PR: read stencil bits from NSOpenGLPixelFormat instead of glGetIntegerv"
        }
      ]
    },
    "versionAnalysis": {
      "mentionedVersions": [
        "3.116.0"
      ],
      "currentRelevance": "unlikely",
      "relevanceReason": "Root cause identified in issue #3525 and fix is in open PR #3546. After the fix, GL performance reaches 77-93 fps, exceeding native C++ GL at 61 fps."
    },
    "fixStatus": {
      "likelyFixed": true,
      "confidence": 0.95,
      "reason": "PR #3546 fixes the exact root cause: reads stencil bits from NSOpenGLPixelFormat.GetValue() instead of glGetIntegerv(GL_STENCIL_BITS) which returns 0 on macOS default framebuffers, causing Skia to disable TessellationPathRenderer.",
      "relatedPRs": [
        3546
      ]
    }
  },
  "analysis": {
    "summary": "Duplicate of #3525. Root cause: macOS GL driver returns 0 from glGetIntegerv(GL_STENCIL_BITS) for default framebuffers (a known driver quirk). SKGLView and SKGLLayer both use this call to construct GRBackendRenderTarget with stencil=0, which disables Skia's TessellationPathRenderer, forcing fallback to DefaultPathRenderer that CPU-tessellates every path with ~40k individual GL draw calls. Native Skia C++ works around this by reading stencil from NSOpenGLPixelFormat. Fix in PR #3546 achieves 15.6x speedup (5.3 fps → 77-93 fps). Metal backend was unaffected.",
    "rationale": "Issue #3552 is a duplicate of #3525, filed 9 days later by the same author (mattleibow) with nearly identical body content. A detailed investigation comment was posted on #3525 on the same day this issue was filed, identifying the root cause as a macOS-specific GL driver quirk and pointing to PR #3546 as the fix. The problematic code path (glGetIntegerv(GL_STENCIL_BITS)) is confirmed in both SKGLView.cs:140 and SKGLLayer.cs:62.",
    "keySignals": [
      {
        "text": "C++ version achieves around 120 fps while the managed SkiaSharp code achieves less than 10 fps",
        "source": "issue body",
        "interpretation": "Large ~12x+ performance gap on macOS with GL rendering path"
      },
      {
        "text": "up to 40 fps with new OpenGL flags",
        "source": "issue body",
        "interpretation": "GL config changes partially help, confirming the issue is in GL rendering setup, not in the Skia drawing commands themselves"
      },
      {
        "text": "Same title, same body, same author as #3525 filed 9 days earlier",
        "source": "GitHub issue comparison",
        "interpretation": "Duplicate issue — same reporter filed the same bug twice"
      }
    ],
    "codeInvestigation": [
      {
        "file": "source/SkiaSharp.Views/SkiaSharp.Views/Platform/macOS/SKGLView.cs",
        "lines": "139-154",
        "finding": "DrawRect reads stencil bits via Gles.glGetIntegerv(Gles.GL_STENCIL_BITS, out var stencil) which returns 0 on macOS default framebuffers. GRBackendRenderTarget is then created with stencil=0, disabling TessellationPathRenderer in Skia and forcing ~40k CPU-tessellated GL draw calls per frame.",
        "relevance": "direct"
      },
      {
        "file": "source/SkiaSharp.Views/SkiaSharp.Views/Platform/macOS/SKGLLayer.cs",
        "lines": "61-76",
        "finding": "SKGLLayer.DrawInCGLContext has the same pattern: Gles.glGetIntegerv(Gles.GL_STENCIL_BITS, out var stencil) followed by new GRBackendRenderTarget(..., stencil, ...). Both macOS GL views are affected by the same macOS driver quirk.",
        "relevance": "direct"
      },
      {
        "file": "source/SkiaSharp.Views/SkiaSharp.Views/Platform/Apple/SKMetalView.cs",
        "lines": "139-178",
        "finding": "SKMetalView uses Metal backend and creates render targets without any stencil query — Metal path is unaffected by the macOS GL driver quirk and performance already matches native C++.",
        "relevance": "context"
      }
    ],
    "nextQuestions": [
      "Does PR #3546 also fix the same stencil reading issue in SKGLLayer.cs or only SKGLView.cs?"
    ]
  },
  "output": {
    "actionability": {
      "suggestedAction": "close-as-duplicate",
      "confidence": 0.98,
      "reason": "Identical report as #3525 by the same author, filed 9 days later. Root cause found (macOS glGetIntegerv(GL_STENCIL_BITS) returns 0) and fix in open PR #3546 achieves 15.6x speedup.",
      "suggestedReproPlatform": "macos"
    },
    "actions": [
      {
        "type": "update-labels",
        "description": "Apply bug, views area, macOS platform, OpenGL backend, and performance tenet labels",
        "risk": "low",
        "confidence": 0.98,
        "labels": [
          "type/bug",
          "area/SkiaSharp.Views",
          "os/macOS",
          "backend/OpenGL",
          "tenet/performance"
        ]
      },
      {
        "type": "link-duplicate",
        "description": "Mark as duplicate of #3525",
        "risk": "medium",
        "confidence": 0.98,
        "linkedIssue": 3525
      },
      {
        "type": "close-issue",
        "description": "Close as duplicate of #3525 where the root cause and fix are tracked",
        "risk": "medium",
        "confidence": 0.95,
        "stateReason": "not_planned"
      }
    ]
  }
}
```

</details>
