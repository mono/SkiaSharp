# Issue Triage Report — #2530

| Field | Value |
|-------|-------|
| Repository | mono/SkiaSharp |
| Analyzed | 2026-05-02T10:09:43Z |
| Type | type/bug (0.92 (92%)) |
| Area | area/SkiaSharp (0.90 (90%)) |
| Suggested action | close-as-duplicate (0.88 (88%)) |

**Issue Summary:** Custom SKSL shader via SKRuntimeEffect crashes with SEHException when drawing on a bitmap (raster) canvas; same root cause as #1822 — SKRuntimeEffect requires GPU surface in SkiaSharp v2.

**Analysis:** SKRuntimeEffect crashes when applied on a raster/bitmap canvas because SkiaSharp v2 ships Skia milestone 88, which requires SK_ENABLE_SKSL_INTERPRETER or SkVM (added in milestone 90+) for CPU-side shader evaluation. The issue is resolved in SkiaSharp v3 which ships an updated Skia with full CPU shader support.

**Recommendations:** **close-as-duplicate** — This is a duplicate of #1822 (SKRuntimeEffect doesn't work on Bitmap Canvas), which tracks the same crash and is closed as completed pointing to the v3 fix.

---

## Classification

| Field | Value |
|-------|-------|
| Type | type/bug |
| Area | area/SkiaSharp |
| Platforms | — |
| Backends | — |
| Tenets | tenet/reliability |
| Partner | — |

## Evidence

### Reproduction

1. Create an SKBitmap or SKSurface (raster)
2. Create a canvas from it
3. Create a custom SKSL shader via SKRuntimeEffect.Create()
4. Set the shader on an SKPaint
5. Call canvas.DrawRect() or similar — app crashes

**Environment:** Reporter does not specify version; references issues dated 2021–2022 suggesting v2.88.x

**Related issues:** #1822, #2230, #2496

**Repository links:**
- https://github.com/mono/SkiaSharp/issues/1822 — Primary tracking issue: SKRuntimeEffect doesn't work on Bitmap Canvas
- https://github.com/mono/SkiaSharp/issues/2230 — Feature request: update Skia to milestone 90+ to enable SkVM/CPU shader support
- https://github.com/mono/SkiaSharp/issues/2496 — Duplicate: Custom shader in SKCanvasView crashes .NET MAUI App

### Bug Signals

| Field | Value |
|-------|-------|
| Severity | medium |
| Regression claimed | False |
| Error type | crash |
| Error message | SEHException: External component has thrown an exception (on Windows); silent crash on iOS/macOS |
| Repro quality | partial |
| Target frameworks | — |

### Version Analysis

| Field | Value |
|-------|-------|
| Mentioned versions | — |
| Worked in | — |
| Broke in | — |
| Current relevance | unlikely |
| Relevance reason | The maintainer confirmed this is fixed in the v3 Skia update. The current codebase (v3) includes SKRuntimeEffectTest.Raster which exercises raster-surface shader draws, indicating CPU-side SkSL support is now present. |

### Fix Status

| Field | Value |
|-------|-------|
| Likely fixed | True |
| Confidence | 0.85 (85%) |
| Reason | Maintainer (mattleibow) commented on #1822: 'GPU and CPU surfaces are all fast!' after the new Skia milestone update. A Raster test class exists in SKRuntimeEffectTest.cs in the current repo exercising CPU-surface shader draws. |
| Related PRs | — |
| Related commits | — |
| Fixed in version | v3 (preview feed: https://aka.ms/skiasharp-eap/index.json) |

## Analysis

### Technical Summary

SKRuntimeEffect crashes when applied on a raster/bitmap canvas because SkiaSharp v2 ships Skia milestone 88, which requires SK_ENABLE_SKSL_INTERPRETER or SkVM (added in milestone 90+) for CPU-side shader evaluation. The issue is resolved in SkiaSharp v3 which ships an updated Skia with full CPU shader support.

### Rationale

Reporter clearly describes a native crash using SKRuntimeEffect on a bitmap canvas and links three directly related upstream issues (#1822, #2230, #2496). All three were closed as 'completed' pointing to the v3 Skia milestone upgrade as the fix. The current codebase contains a Raster test class in SKRuntimeEffectTest.cs that draws with a custom shader on a CPU surface, confirming the fix is in the repo. This is effectively a duplicate of #1822.

### Key Signals

- "This is very annoying for just 1 missing line of C++ code." — **issue body** (Reporter is aware of the SK_ENABLE_SKSL_INTERPRETER workaround discussed in #1822 comments.)
- "References #2496, #1822, #2230" — **issue body** (All three referenced issues are closed as completed, pointing to the v3 fix.)
- "Just been testing this in the new Skia update and it is looking so good. GPU and CPU surfaces are all fast!" — **#1822 comment by mattleibow (2023-08-05)** (Maintainer confirmed CPU shader support is working in the new Skia update (v3).)

### Code Investigation

| File | Lines | Relevance | Finding |
|------|-------|-----------|---------|
| `tests/Tests/SkiaSharp/SKRuntimeEffectTest.cs` | 1060-1079 | direct | A 'Raster' test class creates an SKSurface via SKSurface.Create(Info) (raster) and inherits TestEffectTests, which runs shader draw tests. This class exists and is not skipped, confirming raster surface shader support in the current codebase. |
| `binding/SkiaSharp/SKRuntimeEffect.cs` | — | related | ToShader() calls sk_runtimeeffect_make_shader via P/Invoke. No platform check or CPU/GPU branching in C# — the fix is entirely in the underlying native Skia library (milestone upgrade in v3). |

### Workarounds

- Use a GPU surface (SKGLSurface or SKMetalSurface) instead of a bitmap canvas — SKRuntimeEffect works on GPU in v2.
- Upgrade to SkiaSharp v3 (available on preview feed https://aka.ms/skiasharp-eap/index.json) where both CPU and GPU surfaces support SKRuntimeEffect.

### Next Questions

- What SkiaSharp version is the reporter using? The issue likely does not affect v3.

### Resolution Proposals

**Hypothesis:** The crash is caused by missing SkVM/SKSL CPU interpreter in Skia milestone 88 (shipped in SkiaSharp v2). The fix shipped in SkiaSharp v3 with an updated Skia milestone.

1. **Upgrade to SkiaSharp v3** — workaround, confidence 0.90 (90%), cost/xs, validated=untested
   - SkiaSharp v3 includes an updated Skia library with CPU shader support (SkVM). The reporter should upgrade to the v3 preview from https://aka.ms/skiasharp-eap/index.json or wait for the stable v3 release.
2. **Use GPU canvas in v2** — workaround, confidence 0.90 (90%), cost/s, validated=untested
   - In SkiaSharp v2, SKRuntimeEffect only works with a GPU canvas. Use SKGLSurface or SKMetalSurface instead of an SKBitmap canvas.

**Recommended proposal:** Upgrade to SkiaSharp v3

**Why:** Directly addresses the root cause; CPU + GPU shader support confirmed working in v3.

## Recommendations

### Actionability

| Field | Value |
|-------|-------|
| Suggested action | close-as-duplicate |
| Confidence | 0.88 (88%) |
| Reason | This is a duplicate of #1822 (SKRuntimeEffect doesn't work on Bitmap Canvas), which tracks the same crash and is closed as completed pointing to the v3 fix. |
| Suggested repro platform | linux |

### Automatable Actions

| Type | Risk | Confidence | Description | Details |
|------|------|------------|-------------|---------|
| update-labels | low | 0.92 (92%) | Apply bug, area/SkiaSharp, reliability labels | labels=type/bug, area/SkiaSharp, tenet/reliability |
| link-duplicate | medium | 0.88 (88%) | Mark as duplicate of #1822 (primary tracking issue) | linkedIssue=#1822 |
| add-comment | medium | 0.88 (88%) | Explain the issue is a known limitation in v2 and fixed in v3 | — |
| close-issue | medium | 0.85 (85%) | Close as duplicate of #1822 | stateReason=not_planned |

**Comment draft for `add-comment`:**

```markdown
Thanks for the report! This is a known limitation in SkiaSharp v2 — `SKRuntimeEffect` (custom SKSL shaders) requires a GPU surface and does not work on raster/bitmap canvases in v2. This is because SkiaSharp v2 ships Skia milestone 88, which does not include SkVM CPU-side shader evaluation.

This is tracked in #1822 and has been fixed in **SkiaSharp v3**, which ships a newer Skia milestone with full CPU + GPU shader support.

**Workarounds:**
1. **Upgrade to SkiaSharp v3** (preview available at `https://aka.ms/skiasharp-eap/index.json`)
2. **Use a GPU canvas** (`SKGLSurface` or `SKMetalSurface`) in v2 — custom shaders work there

Closing as duplicate of #1822.
```

<details>
<summary>Raw JSON</summary>

```json
{
  "meta": {
    "schemaVersion": "1.0",
    "number": 2530,
    "repo": "mono/SkiaSharp",
    "analyzedAt": "2026-05-02T10:09:43Z"
  },
  "summary": "Custom SKSL shader via SKRuntimeEffect crashes with SEHException when drawing on a bitmap (raster) canvas; same root cause as #1822 — SKRuntimeEffect requires GPU surface in SkiaSharp v2.",
  "classification": {
    "type": {
      "value": "type/bug",
      "confidence": 0.92
    },
    "area": {
      "value": "area/SkiaSharp",
      "confidence": 0.9
    },
    "tenets": [
      "tenet/reliability"
    ]
  },
  "evidence": {
    "bugSignals": {
      "severity": "medium",
      "regressionClaimed": false,
      "errorType": "crash",
      "errorMessage": "SEHException: External component has thrown an exception (on Windows); silent crash on iOS/macOS",
      "reproQuality": "partial",
      "targetFrameworks": []
    },
    "reproEvidence": {
      "stepsToReproduce": [
        "Create an SKBitmap or SKSurface (raster)",
        "Create a canvas from it",
        "Create a custom SKSL shader via SKRuntimeEffect.Create()",
        "Set the shader on an SKPaint",
        "Call canvas.DrawRect() or similar — app crashes"
      ],
      "environmentDetails": "Reporter does not specify version; references issues dated 2021–2022 suggesting v2.88.x",
      "relatedIssues": [
        1822,
        2230,
        2496
      ],
      "repoLinks": [
        {
          "url": "https://github.com/mono/SkiaSharp/issues/1822",
          "description": "Primary tracking issue: SKRuntimeEffect doesn't work on Bitmap Canvas"
        },
        {
          "url": "https://github.com/mono/SkiaSharp/issues/2230",
          "description": "Feature request: update Skia to milestone 90+ to enable SkVM/CPU shader support"
        },
        {
          "url": "https://github.com/mono/SkiaSharp/issues/2496",
          "description": "Duplicate: Custom shader in SKCanvasView crashes .NET MAUI App"
        }
      ]
    },
    "versionAnalysis": {
      "mentionedVersions": [],
      "currentRelevance": "unlikely",
      "relevanceReason": "The maintainer confirmed this is fixed in the v3 Skia update. The current codebase (v3) includes SKRuntimeEffectTest.Raster which exercises raster-surface shader draws, indicating CPU-side SkSL support is now present."
    },
    "fixStatus": {
      "likelyFixed": true,
      "confidence": 0.85,
      "reason": "Maintainer (mattleibow) commented on #1822: 'GPU and CPU surfaces are all fast!' after the new Skia milestone update. A Raster test class exists in SKRuntimeEffectTest.cs in the current repo exercising CPU-surface shader draws.",
      "relatedPRs": [],
      "fixedInVersion": "v3 (preview feed: https://aka.ms/skiasharp-eap/index.json)"
    }
  },
  "analysis": {
    "summary": "SKRuntimeEffect crashes when applied on a raster/bitmap canvas because SkiaSharp v2 ships Skia milestone 88, which requires SK_ENABLE_SKSL_INTERPRETER or SkVM (added in milestone 90+) for CPU-side shader evaluation. The issue is resolved in SkiaSharp v3 which ships an updated Skia with full CPU shader support.",
    "rationale": "Reporter clearly describes a native crash using SKRuntimeEffect on a bitmap canvas and links three directly related upstream issues (#1822, #2230, #2496). All three were closed as 'completed' pointing to the v3 Skia milestone upgrade as the fix. The current codebase contains a Raster test class in SKRuntimeEffectTest.cs that draws with a custom shader on a CPU surface, confirming the fix is in the repo. This is effectively a duplicate of #1822.",
    "keySignals": [
      {
        "text": "This is very annoying for just 1 missing line of C++ code.",
        "source": "issue body",
        "interpretation": "Reporter is aware of the SK_ENABLE_SKSL_INTERPRETER workaround discussed in #1822 comments."
      },
      {
        "text": "References #2496, #1822, #2230",
        "source": "issue body",
        "interpretation": "All three referenced issues are closed as completed, pointing to the v3 fix."
      },
      {
        "text": "Just been testing this in the new Skia update and it is looking so good. GPU and CPU surfaces are all fast!",
        "source": "#1822 comment by mattleibow (2023-08-05)",
        "interpretation": "Maintainer confirmed CPU shader support is working in the new Skia update (v3)."
      }
    ],
    "codeInvestigation": [
      {
        "file": "tests/Tests/SkiaSharp/SKRuntimeEffectTest.cs",
        "lines": "1060-1079",
        "finding": "A 'Raster' test class creates an SKSurface via SKSurface.Create(Info) (raster) and inherits TestEffectTests, which runs shader draw tests. This class exists and is not skipped, confirming raster surface shader support in the current codebase.",
        "relevance": "direct"
      },
      {
        "file": "binding/SkiaSharp/SKRuntimeEffect.cs",
        "finding": "ToShader() calls sk_runtimeeffect_make_shader via P/Invoke. No platform check or CPU/GPU branching in C# — the fix is entirely in the underlying native Skia library (milestone upgrade in v3).",
        "relevance": "related"
      }
    ],
    "workarounds": [
      "Use a GPU surface (SKGLSurface or SKMetalSurface) instead of a bitmap canvas — SKRuntimeEffect works on GPU in v2.",
      "Upgrade to SkiaSharp v3 (available on preview feed https://aka.ms/skiasharp-eap/index.json) where both CPU and GPU surfaces support SKRuntimeEffect."
    ],
    "nextQuestions": [
      "What SkiaSharp version is the reporter using? The issue likely does not affect v3."
    ],
    "resolution": {
      "hypothesis": "The crash is caused by missing SkVM/SKSL CPU interpreter in Skia milestone 88 (shipped in SkiaSharp v2). The fix shipped in SkiaSharp v3 with an updated Skia milestone.",
      "proposals": [
        {
          "title": "Upgrade to SkiaSharp v3",
          "description": "SkiaSharp v3 includes an updated Skia library with CPU shader support (SkVM). The reporter should upgrade to the v3 preview from https://aka.ms/skiasharp-eap/index.json or wait for the stable v3 release.",
          "category": "workaround",
          "confidence": 0.9,
          "effort": "cost/xs",
          "validated": "untested"
        },
        {
          "title": "Use GPU canvas in v2",
          "description": "In SkiaSharp v2, SKRuntimeEffect only works with a GPU canvas. Use SKGLSurface or SKMetalSurface instead of an SKBitmap canvas.",
          "category": "workaround",
          "confidence": 0.9,
          "effort": "cost/s",
          "validated": "untested"
        }
      ],
      "recommendedProposal": "Upgrade to SkiaSharp v3",
      "recommendedReason": "Directly addresses the root cause; CPU + GPU shader support confirmed working in v3."
    }
  },
  "output": {
    "actionability": {
      "suggestedAction": "close-as-duplicate",
      "confidence": 0.88,
      "reason": "This is a duplicate of #1822 (SKRuntimeEffect doesn't work on Bitmap Canvas), which tracks the same crash and is closed as completed pointing to the v3 fix.",
      "suggestedReproPlatform": "linux"
    },
    "actions": [
      {
        "type": "update-labels",
        "description": "Apply bug, area/SkiaSharp, reliability labels",
        "risk": "low",
        "confidence": 0.92,
        "labels": [
          "type/bug",
          "area/SkiaSharp",
          "tenet/reliability"
        ]
      },
      {
        "type": "link-duplicate",
        "description": "Mark as duplicate of #1822 (primary tracking issue)",
        "risk": "medium",
        "confidence": 0.88,
        "linkedIssue": 1822
      },
      {
        "type": "add-comment",
        "description": "Explain the issue is a known limitation in v2 and fixed in v3",
        "risk": "medium",
        "confidence": 0.88,
        "comment": "Thanks for the report! This is a known limitation in SkiaSharp v2 — `SKRuntimeEffect` (custom SKSL shaders) requires a GPU surface and does not work on raster/bitmap canvases in v2. This is because SkiaSharp v2 ships Skia milestone 88, which does not include SkVM CPU-side shader evaluation.\n\nThis is tracked in #1822 and has been fixed in **SkiaSharp v3**, which ships a newer Skia milestone with full CPU + GPU shader support.\n\n**Workarounds:**\n1. **Upgrade to SkiaSharp v3** (preview available at `https://aka.ms/skiasharp-eap/index.json`)\n2. **Use a GPU canvas** (`SKGLSurface` or `SKMetalSurface`) in v2 — custom shaders work there\n\nClosing as duplicate of #1822."
      },
      {
        "type": "close-issue",
        "description": "Close as duplicate of #1822",
        "risk": "medium",
        "confidence": 0.85,
        "stateReason": "not_planned"
      }
    ]
  }
}
```

</details>
