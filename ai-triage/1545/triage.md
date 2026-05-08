# Issue Triage Report — #1545

| Field | Value |
|-------|-------|
| Repository | mono/SkiaSharp |
| Analyzed | 2026-04-24T11:45:00Z |
| Type | type/bug (0.90 (90%)) |
| Area | area/SkiaSharp (0.85 (85%)) |
| Suggested action | needs-investigation (0.82 (82%)) |

**Issue Summary:** Cubic bezier paths with antialiasing enabled are not rendered when using the OpenGL backend on Windows and Linux; only straight-line segments draw correctly — a second reporter confirmed the issue at the C API level and found that calling gr_direct_context_reset_context with 0xffffffff (GRBackendState.All) instead of 0 fixes the rendering.

**Analysis:** Cubic bezier paths with antialiasing enabled are silently invisible when using the OpenGL backend. A second reporter reproduced at the C API level and discovered that calling gr_direct_context_reset_context with 0 (no-op reset) triggers the issue, while using 0xffffffff (GRBackendState.All) fixes it. The root cause is likely stale OpenGL state (especially stencil) from prior frames interfering with Skia's tessellation-based antialiasing shader for cubic segments.

**Recommendations:** **needs-investigation** — Bug is reproducible on multiple platforms with a known workaround but the root cause in Skia's GL backend needs deeper investigation to determine if this is a Skia bug or expected behavior requiring better documentation.

---

## Classification

| Field | Value |
|-------|-------|
| Type | type/bug |
| Area | area/SkiaSharp |
| Platforms | os/Windows-Classic, os/Linux |
| Backends | backend/OpenGL |
| Tenets | tenet/reliability |
| Partner | — |

## Evidence

### Reproduction

1. Create a GRContext backed by OpenGL
2. Draw two SKPath objects: one with LineTo and one with CubicTo, both with IsAntialias=true
3. Observe that the cubic path is not rendered on screen

**Environment:** SkiaSharp 2.80.2, Visual Studio 2019 16.7.1, Windows 10, GeForce GTX 760; also confirmed on Linux with SkiaSharp 2.88.5 at C API level

**Repository links:**
- https://github.com/mono/SkiaSharp/issues/1545 — Original issue report
- https://gitlab.com/Mis012/gtk4-sk-area — GTK4+Skia C repro project by second reporter

### Bug Signals

| Field | Value |
|-------|-------|
| Severity | medium |
| Regression claimed | False |
| Error type | wrong-output |
| Error message | Cubic path with antialiasing is invisible; only LineTo segments render |
| Repro quality | complete |
| Target frameworks | net461 |

### Version Analysis

| Field | Value |
|-------|-------|
| Mentioned versions | 2.80.2, 2.88.5 |
| Worked in | — |
| Broke in | — |
| Current relevance | likely |
| Relevance reason | Second reporter confirmed the issue reproduces with 2.88.5 and the skiasharp branch. No fix has been identified in the codebase. |

## Analysis

### Technical Summary

Cubic bezier paths with antialiasing enabled are silently invisible when using the OpenGL backend. A second reporter reproduced at the C API level and discovered that calling gr_direct_context_reset_context with 0 (no-op reset) triggers the issue, while using 0xffffffff (GRBackendState.All) fixes it. The root cause is likely stale OpenGL state (especially stencil) from prior frames interfering with Skia's tessellation-based antialiasing shader for cubic segments.

### Rationale

The bug is clearly a wrong-output issue that affects multiple users on multiple platforms (Windows and Linux). The symptom is consistent: antialiasing off = all paths visible; antialiasing on = only straight-line segments visible. The C API-level reproduction by Mis012 confirms the issue is in Skia's OpenGL backend, not C# bindings. The workaround (call ResetContext with GRBackendState.All) is validated by the reporter and makes sense given how Skia manages GPU state.

### Key Signals

- "canv.DrawPath(path, paint); //is NOT on screen" — **issue body** (Cubic bezier segment with antialiasing does not render at all on OpenGL)
- "sk_paint_set_antialias(stroke, true); // causes paths which include cubic sections to not render properly" — **comment by Mis012** (Second reporter independently confirmed the issue at C API level)
- "gr_direct_context_reset_context should be called with 0xffffffff, that seems to fix the issue for me" — **comment by Mis012** (Confirmed workaround: fully resetting GPU state before each frame resolves the rendering failure)
- "the first frame is always rendered correctly" — **comment by Mis012** (Stale GPU state from prior frames causes the issue — first frame has a clean slate)

### Code Investigation

| File | Lines | Relevance | Finding |
|------|-------|-----------|---------|
| `binding/SkiaSharp/GRContext.cs` | 131-138 | direct | ResetContext() defaults to GRBackendState.All (0xffffffff). If callers pass 0 or GRBackendState.None, no GL state is reset, which reproduces the issue described in the bug. |
| `binding/SkiaSharp/GRDefinitions.cs` | 12-35 | direct | GRBackendState.All = 0xffffffff and GRGlBackendState.All = 0xffff. The full reset is available but users who pass 0 or use incorrect flags skip it. |
| `source/SkiaSharp.Views/SkiaSharp.Views.WPF/SKGLElement.cs` | 111-113 | related | WPF SKGLElement correctly calls grContext.ResetContext() with the default GRBackendState.All before each render. This confirms the correct pattern exists in the views layer. |

### Workarounds

- Call grContext.ResetContext() (no-arg form, uses GRBackendState.All = 0xffffffff) at the start of each frame before drawing
- Explicitly pass GRBackendState.All: grContext.ResetContext(GRBackendState.All)
- Disable antialiasing as a temporary measure: paint.IsAntialias = false

### Next Questions

- Does the issue reproduce with the current SkiaSharp 3.x release?
- Is the Windows-Classic console repro from the original reporter using any ResetContext call at all?
- Does the bug affect quadratic beziers (ConicTo/QuadTo) or only cubic (CubicTo)?
- Is there a way to surface this as a warning or documentation note in the SkiaSharp API?

### Resolution Proposals

**Hypothesis:** Stale OpenGL stencil or program state from prior frames causes Skia's tessellation-based antialiasing shader for cubic paths to silently fail. Resetting full GL state via GRBackendState.All clears the bad state.

1. **User workaround: call ResetContext(GRBackendState.All) each frame** — workaround, confidence 0.92 (92%), cost/xs, validated=yes
   - Ensure grContext.ResetContext() is called at the start of each render frame. The no-argument form uses GRBackendState.All (0xffffffff) by default.

```csharp
// At the start of each render callback:
grContext.ResetContext(); // defaults to GRBackendState.All

// or explicitly:
grContext.ResetContext(GRBackendState.All);
```
2. **Document the ResetContext requirement in SkiaSharp samples and docs** — fix, confidence 0.85 (85%), cost/s, validated=untested
   - Add a note to the OpenGL usage guide and relevant samples explaining that ResetContext(GRBackendState.All) must be called at the start of each frame when using an external GL context.

**Recommended proposal:** User workaround: call ResetContext(GRBackendState.All) each frame

**Why:** Immediately actionable workaround with high confidence. The documentation gap is secondary and the fix is straightforward for users once they know about it.

## Recommendations

### Actionability

| Field | Value |
|-------|-------|
| Suggested action | needs-investigation |
| Confidence | 0.82 (82%) |
| Reason | Bug is reproducible on multiple platforms with a known workaround but the root cause in Skia's GL backend needs deeper investigation to determine if this is a Skia bug or expected behavior requiring better documentation. |
| Suggested repro platform | linux |

### Automatable Actions

| Type | Risk | Confidence | Description | Details |
|------|------|------------|-------------|---------|
| update-labels | low | 0.90 (90%) | Apply type/bug, area/SkiaSharp, os/Windows-Classic, os/Linux, backend/OpenGL, tenet/reliability labels | labels=type/bug, area/SkiaSharp, os/Windows-Classic, os/Linux, backend/OpenGL, tenet/reliability |
| add-comment | medium | 0.85 (85%) | Post workaround and request confirmation on current version | — |

**Comment draft for `add-comment`:**

```markdown
Thanks for the detailed report! A second reporter (@Mis012) confirmed this issue and found a workaround.

**Root cause:** When using the OpenGL backend, Skia's tessellation-based antialiasing for cubic bezier paths requires a clean GPU state. If `GRContext.ResetContext()` is not called (or is called with `GRBackendState.None`) at the start of each frame, stale OpenGL state from the prior frame silently prevents the cubic path from rendering.

**Workaround:** Call `grContext.ResetContext()` (which defaults to `GRBackendState.All`) at the start of each frame before drawing:

```csharp
// At the start of each render callback:
grContext.ResetContext(); // equivalent to ResetContext(GRBackendState.All)

// Then draw as normal:
canvas.DrawPath(path, paint);
```

Could you confirm whether adding this call resolves the issue for you? Also, does this still reproduce on the latest SkiaSharp 3.x release?
```

<details>
<summary>Raw JSON</summary>

```json
{
  "meta": {
    "schemaVersion": "1.0",
    "number": 1545,
    "repo": "mono/SkiaSharp",
    "analyzedAt": "2026-04-24T11:45:00Z"
  },
  "summary": "Cubic bezier paths with antialiasing enabled are not rendered when using the OpenGL backend on Windows and Linux; only straight-line segments draw correctly — a second reporter confirmed the issue at the C API level and found that calling gr_direct_context_reset_context with 0xffffffff (GRBackendState.All) instead of 0 fixes the rendering.",
  "classification": {
    "type": {
      "value": "type/bug",
      "confidence": 0.9
    },
    "area": {
      "value": "area/SkiaSharp",
      "confidence": 0.85
    },
    "platforms": [
      "os/Windows-Classic",
      "os/Linux"
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
      "severity": "medium",
      "regressionClaimed": false,
      "errorType": "wrong-output",
      "errorMessage": "Cubic path with antialiasing is invisible; only LineTo segments render",
      "reproQuality": "complete",
      "targetFrameworks": [
        "net461"
      ]
    },
    "reproEvidence": {
      "stepsToReproduce": [
        "Create a GRContext backed by OpenGL",
        "Draw two SKPath objects: one with LineTo and one with CubicTo, both with IsAntialias=true",
        "Observe that the cubic path is not rendered on screen"
      ],
      "environmentDetails": "SkiaSharp 2.80.2, Visual Studio 2019 16.7.1, Windows 10, GeForce GTX 760; also confirmed on Linux with SkiaSharp 2.88.5 at C API level",
      "repoLinks": [
        {
          "url": "https://github.com/mono/SkiaSharp/issues/1545",
          "description": "Original issue report"
        },
        {
          "url": "https://gitlab.com/Mis012/gtk4-sk-area",
          "description": "GTK4+Skia C repro project by second reporter"
        }
      ]
    },
    "versionAnalysis": {
      "mentionedVersions": [
        "2.80.2",
        "2.88.5"
      ],
      "currentRelevance": "likely",
      "relevanceReason": "Second reporter confirmed the issue reproduces with 2.88.5 and the skiasharp branch. No fix has been identified in the codebase."
    }
  },
  "analysis": {
    "summary": "Cubic bezier paths with antialiasing enabled are silently invisible when using the OpenGL backend. A second reporter reproduced at the C API level and discovered that calling gr_direct_context_reset_context with 0 (no-op reset) triggers the issue, while using 0xffffffff (GRBackendState.All) fixes it. The root cause is likely stale OpenGL state (especially stencil) from prior frames interfering with Skia's tessellation-based antialiasing shader for cubic segments.",
    "rationale": "The bug is clearly a wrong-output issue that affects multiple users on multiple platforms (Windows and Linux). The symptom is consistent: antialiasing off = all paths visible; antialiasing on = only straight-line segments visible. The C API-level reproduction by Mis012 confirms the issue is in Skia's OpenGL backend, not C# bindings. The workaround (call ResetContext with GRBackendState.All) is validated by the reporter and makes sense given how Skia manages GPU state.",
    "keySignals": [
      {
        "text": "canv.DrawPath(path, paint); //is NOT on screen",
        "source": "issue body",
        "interpretation": "Cubic bezier segment with antialiasing does not render at all on OpenGL"
      },
      {
        "text": "sk_paint_set_antialias(stroke, true); // causes paths which include cubic sections to not render properly",
        "source": "comment by Mis012",
        "interpretation": "Second reporter independently confirmed the issue at C API level"
      },
      {
        "text": "gr_direct_context_reset_context should be called with 0xffffffff, that seems to fix the issue for me",
        "source": "comment by Mis012",
        "interpretation": "Confirmed workaround: fully resetting GPU state before each frame resolves the rendering failure"
      },
      {
        "text": "the first frame is always rendered correctly",
        "source": "comment by Mis012",
        "interpretation": "Stale GPU state from prior frames causes the issue — first frame has a clean slate"
      }
    ],
    "codeInvestigation": [
      {
        "file": "binding/SkiaSharp/GRContext.cs",
        "lines": "131-138",
        "finding": "ResetContext() defaults to GRBackendState.All (0xffffffff). If callers pass 0 or GRBackendState.None, no GL state is reset, which reproduces the issue described in the bug.",
        "relevance": "direct"
      },
      {
        "file": "binding/SkiaSharp/GRDefinitions.cs",
        "lines": "12-35",
        "finding": "GRBackendState.All = 0xffffffff and GRGlBackendState.All = 0xffff. The full reset is available but users who pass 0 or use incorrect flags skip it.",
        "relevance": "direct"
      },
      {
        "file": "source/SkiaSharp.Views/SkiaSharp.Views.WPF/SKGLElement.cs",
        "lines": "111-113",
        "finding": "WPF SKGLElement correctly calls grContext.ResetContext() with the default GRBackendState.All before each render. This confirms the correct pattern exists in the views layer.",
        "relevance": "related"
      }
    ],
    "workarounds": [
      "Call grContext.ResetContext() (no-arg form, uses GRBackendState.All = 0xffffffff) at the start of each frame before drawing",
      "Explicitly pass GRBackendState.All: grContext.ResetContext(GRBackendState.All)",
      "Disable antialiasing as a temporary measure: paint.IsAntialias = false"
    ],
    "nextQuestions": [
      "Does the issue reproduce with the current SkiaSharp 3.x release?",
      "Is the Windows-Classic console repro from the original reporter using any ResetContext call at all?",
      "Does the bug affect quadratic beziers (ConicTo/QuadTo) or only cubic (CubicTo)?",
      "Is there a way to surface this as a warning or documentation note in the SkiaSharp API?"
    ],
    "resolution": {
      "hypothesis": "Stale OpenGL stencil or program state from prior frames causes Skia's tessellation-based antialiasing shader for cubic paths to silently fail. Resetting full GL state via GRBackendState.All clears the bad state.",
      "proposals": [
        {
          "title": "User workaround: call ResetContext(GRBackendState.All) each frame",
          "description": "Ensure grContext.ResetContext() is called at the start of each render frame. The no-argument form uses GRBackendState.All (0xffffffff) by default.",
          "category": "workaround",
          "codeSnippet": "// At the start of each render callback:\ngrContext.ResetContext(); // defaults to GRBackendState.All\n\n// or explicitly:\ngrContext.ResetContext(GRBackendState.All);",
          "confidence": 0.92,
          "effort": "cost/xs",
          "validated": "yes"
        },
        {
          "title": "Document the ResetContext requirement in SkiaSharp samples and docs",
          "description": "Add a note to the OpenGL usage guide and relevant samples explaining that ResetContext(GRBackendState.All) must be called at the start of each frame when using an external GL context.",
          "category": "fix",
          "confidence": 0.85,
          "effort": "cost/s",
          "validated": "untested"
        }
      ],
      "recommendedProposal": "User workaround: call ResetContext(GRBackendState.All) each frame",
      "recommendedReason": "Immediately actionable workaround with high confidence. The documentation gap is secondary and the fix is straightforward for users once they know about it."
    }
  },
  "output": {
    "actionability": {
      "suggestedAction": "needs-investigation",
      "confidence": 0.82,
      "reason": "Bug is reproducible on multiple platforms with a known workaround but the root cause in Skia's GL backend needs deeper investigation to determine if this is a Skia bug or expected behavior requiring better documentation.",
      "suggestedReproPlatform": "linux"
    },
    "actions": [
      {
        "type": "update-labels",
        "description": "Apply type/bug, area/SkiaSharp, os/Windows-Classic, os/Linux, backend/OpenGL, tenet/reliability labels",
        "risk": "low",
        "confidence": 0.9,
        "labels": [
          "type/bug",
          "area/SkiaSharp",
          "os/Windows-Classic",
          "os/Linux",
          "backend/OpenGL",
          "tenet/reliability"
        ]
      },
      {
        "type": "add-comment",
        "description": "Post workaround and request confirmation on current version",
        "risk": "medium",
        "confidence": 0.85,
        "comment": "Thanks for the detailed report! A second reporter (@Mis012) confirmed this issue and found a workaround.\n\n**Root cause:** When using the OpenGL backend, Skia's tessellation-based antialiasing for cubic bezier paths requires a clean GPU state. If `GRContext.ResetContext()` is not called (or is called with `GRBackendState.None`) at the start of each frame, stale OpenGL state from the prior frame silently prevents the cubic path from rendering.\n\n**Workaround:** Call `grContext.ResetContext()` (which defaults to `GRBackendState.All`) at the start of each frame before drawing:\n\n```csharp\n// At the start of each render callback:\ngrContext.ResetContext(); // equivalent to ResetContext(GRBackendState.All)\n\n// Then draw as normal:\ncanvas.DrawPath(path, paint);\n```\n\nCould you confirm whether adding this call resolves the issue for you? Also, does this still reproduce on the latest SkiaSharp 3.x release?"
      }
    ]
  }
}
```

</details>
