# Issue Triage Report — #4421

| Field | Value |
|-------|-------|
| Repository | mono/SkiaSharp |
| Analyzed | 2026-07-15T05:10:00Z |
| Type | type/bug (0.99 (99%)) |
| Area | area/libSkiaSharp.native (0.97 (97%)) |
| Suggested action | ready-to-fix (0.97 (97%)) |

**Issue Summary:** Severe CPU rasterization performance regression (5–20× slowdown, flickering) in SKCanvasView on Windows starting from Skia m146, caused by removal of SK_ENABLE_LEGACY_SHADERCONTEXT default define in upstream Skia commit 356c7267fa; fix PR #4428 is open.

**Analysis:** Upstream Skia m146 removed SK_ENABLE_LEGACY_SHADERCONTEXT as a default define, causing SkShaderBase::makeContext() to always return nullptr. This forces SkBlitter.cpp to use the generic raster-pipeline blitter (CreateSkRPBlitter) instead of the hand-optimized SkARGB32_Shader_Blitter for N32+SrcOver draws with a shader. The fix is to add -DSK_ENABLE_LEGACY_SHADERCONTEXT to extra_cflags in all CPU-raster native/*/build.cake files.

**Recommendations:** **ready-to-fix** — Root cause confirmed by both reporter and maintainer; fix PR #4428 is open and awaiting CI; the define is the sanctioned upstream opt-in mechanism.

---

## Classification

| Field | Value |
|-------|-------|
| Type | type/bug |
| Area | area/libSkiaSharp.native |
| Platforms | os/Windows-Classic, os/Windows-WinUI, os/Android, os/iOS, os/macOS, os/Linux |
| Backends | backend/Raster |
| Tenets | tenet/performance, tenet/reliability |
| Perf | perf/rendering |
| Partner | — |
| Current labels | type/bug |

## Evidence

### Reproduction

1. Create a WinUI or Windows app using SKCanvasView with CPU (software) rasterization
2. In the paint handler, draw repeating tiled bitmaps over a large area using DrawBitmap with src/dst rects
3. Observe high CPU usage, severe frame drops, and visual flickering
4. Compare against SkiaSharp 3.119.4 (m145) to confirm regression

**Environment:** Windows, Visual Studio, SkiaSharp 4.147-preview1.1 and later (m146+)

**Repository links:**
- https://github.com/mono/SkiaSharp/pull/4428 — Fix PR: Restore SK_ENABLE_LEGACY_SHADERCONTEXT to fix CPU raster perf regression

### Bug Signals

| Field | Value |
|-------|-------|
| Severity | high |
| Regression claimed | True |
| Error type | performance |
| Error message | CPU rendering 5–20× slower; pages take 1–2s, display black during render, visible flickering |
| Repro quality | complete |
| Target frameworks | net8.0-windows, net9.0-windows |

### Version Analysis

| Field | Value |
|-------|-------|
| Mentioned versions | 4.147-preview1.1, 4.148.0, 4.150.0, 3.119.4 |
| Worked in | 3.119.4 |
| Broke in | 4.147-preview1.1 |
| Current relevance | likely |
| Relevance reason | Regression still present in m151; fix PR #4428 is open but not yet merged |

### Regression

| Field | Value |
|-------|-------|
| Is regression | True |
| Confidence | 0.99 (99%) |
| Reason | Upstream Skia commit 356c7267fa removed the default definition of SK_ENABLE_LEGACY_SHADERCONTEXT from include/core/SkTypes.h in m146. Reporter and maintainer confirmed this is the exact cause via custom builds. |
| Worked in version | 3.119.4 |
| Broke in version | 4.147-preview1.1 |

### Fix Status

| Field | Value |
|-------|-------|
| Likely fixed | False |
| Confidence | 0.95 (95%) |
| Reason | Fix PR #4428 is open but not yet merged; adding -DSK_ENABLE_LEGACY_SHADERCONTEXT to extra_cflags in native build configs is the sanctioned fix |
| Related PRs | #4428 |
| Related commits | — |
| Fixed in version | — |

## Analysis

### Technical Summary

Upstream Skia m146 removed SK_ENABLE_LEGACY_SHADERCONTEXT as a default define, causing SkShaderBase::makeContext() to always return nullptr. This forces SkBlitter.cpp to use the generic raster-pipeline blitter (CreateSkRPBlitter) instead of the hand-optimized SkARGB32_Shader_Blitter for N32+SrcOver draws with a shader. The fix is to add -DSK_ENABLE_LEGACY_SHADERCONTEXT to extra_cflags in all CPU-raster native/*/build.cake files.

### Rationale

Clear performance regression with exact upstream commit identified (356c7267fa), reporter and maintainer both confirmed root cause via custom builds, fix path is unambiguous (add define to build flags), fix PR #4428 already opened by maintainer. Classification as libSkiaSharp.native because the fix is purely in native build configuration, not C# code.

### Key Signals

- "regression was traced back to upstream Google Skia commit 356c7267fa ('Remove SK_DISABLE_LEGACY_SHADERCONTEXT'), which removed the default definition of the SK_ENABLE_LEGACY_SHADERCONTEXT macro" — **issue body** (Pinpoints exact upstream commit as root cause; reporter did thorough bisection analysis)
- "pages with background images can sometimes take 1-2s to render with displaying black in the meanwhile" — **issue body** (Severity is high — visible to end users, makes the app appear frozen)
- "I've opened #4428 which turns SK_ENABLE_LEGACY_SHADERCONTEXT back on across the native builds" — **maintainer comment** (Fix PR is already open; this issue is ready-to-fix pending CI green)
- "extra_cflags=[ '-DSKIA_C_DLL', '-DSK_AVOID_SLOW_RASTER_PIPELINE_BLURS', '/MT{d}', ...] (no SK_ENABLE_LEGACY_SHADERCONTEXT)" — **native/windows/build.cake line 65** (Confirms the define is currently absent from the Windows native build flags)

### Code Investigation

| File | Lines | Relevance | Finding |
|------|-------|-----------|---------|
| `native/windows/build.cake` | 65 | direct | extra_cflags contains '-DSKIA_C_DLL' and '-DSK_AVOID_SLOW_RASTER_PIPELINE_BLURS' but NOT '-DSK_ENABLE_LEGACY_SHADERCONTEXT' — confirms the regression flag is absent from Windows native build |
| `native/winui/build.cake` | 143 | context | WinUI-specific extra_cflags only has '/guard:cf' and '/GS' — no raster perf flags; this is the WinRT projection helper, not the main raster build path |

### Next Questions

- Has the reporter confirmed fix on m150/m151 binaries from PR #4428 CI artifacts?
- Is this a temporary measure until Skia removes the legacy path entirely — should an upstream issue/discussion be filed on skia-discuss?

### Resolution Proposals

**Hypothesis:** Adding -DSK_ENABLE_LEGACY_SHADERCONTEXT to extra_cflags in all CPU-raster native/*/build.cake files restores the fast legacy blitter path and eliminates the performance regression.

1. **Add -DSK_ENABLE_LEGACY_SHADERCONTEXT to all CPU-raster native builds** — fix, confidence 0.97 (97%), cost/xs, validated=yes
   - Add '-DSK_ENABLE_LEGACY_SHADERCONTEXT' next to the existing '-DSK_AVOID_SLOW_RASTER_PIPELINE_BLURS' flag in extra_cflags in native/windows/build.cake (and all other CPU-raster platform build.cake files). PR #4428 already implements this across windows, macos, ios, tvos, linux, android, tizen, and wasm.

**Recommended proposal:** Add -DSK_ENABLE_LEGACY_SHADERCONTEXT to all CPU-raster native builds

**Why:** Root cause is confirmed, fix is minimal and low-risk (restores pre-m146 default behavior), PR #4428 is ready for CI verification.

## Recommendations

### Actionability

| Field | Value |
|-------|-------|
| Suggested action | ready-to-fix |
| Confidence | 0.97 (97%) |
| Reason | Root cause confirmed by both reporter and maintainer; fix PR #4428 is open and awaiting CI; the define is the sanctioned upstream opt-in mechanism. |
| Suggested repro platform | windows |

### Automatable Actions

| Type | Risk | Confidence | Description | Details |
|------|------|------------|-------------|---------|
| update-labels | low | 0.97 (97%) | Apply area, platform, backend, and performance tenet labels | labels=type/bug, area/libSkiaSharp.native, os/Windows-Classic, os/Windows-WinUI, backend/Raster, tenet/performance, perf/rendering |
| link-related | low | 0.99 (99%) | Cross-reference fix PR #4428 | linkedIssue=#4428 |
| set-milestone | low | 0.90 (90%) | Set milestone to 4.151.0-rc.1 matching the current open milestone | milestone=4.151.0-rc.1 |

<details>
<summary>Raw JSON</summary>

```json
{
  "meta": {
    "schemaVersion": "1.0",
    "number": 4421,
    "repo": "mono/SkiaSharp",
    "analyzedAt": "2026-07-15T05:10:00Z",
    "currentLabels": [
      "type/bug"
    ]
  },
  "summary": "Severe CPU rasterization performance regression (5–20× slowdown, flickering) in SKCanvasView on Windows starting from Skia m146, caused by removal of SK_ENABLE_LEGACY_SHADERCONTEXT default define in upstream Skia commit 356c7267fa; fix PR #4428 is open.",
  "classification": {
    "type": {
      "value": "type/bug",
      "confidence": 0.99
    },
    "area": {
      "value": "area/libSkiaSharp.native",
      "confidence": 0.97
    },
    "platforms": [
      "os/Windows-Classic",
      "os/Windows-WinUI",
      "os/Android",
      "os/iOS",
      "os/macOS",
      "os/Linux"
    ],
    "backends": [
      "backend/Raster"
    ],
    "tenets": [
      "tenet/performance",
      "tenet/reliability"
    ],
    "perf": [
      "perf/rendering"
    ]
  },
  "evidence": {
    "bugSignals": {
      "severity": "high",
      "regressionClaimed": true,
      "errorType": "performance",
      "errorMessage": "CPU rendering 5–20× slower; pages take 1–2s, display black during render, visible flickering",
      "reproQuality": "complete",
      "targetFrameworks": [
        "net8.0-windows",
        "net9.0-windows"
      ]
    },
    "reproEvidence": {
      "stepsToReproduce": [
        "Create a WinUI or Windows app using SKCanvasView with CPU (software) rasterization",
        "In the paint handler, draw repeating tiled bitmaps over a large area using DrawBitmap with src/dst rects",
        "Observe high CPU usage, severe frame drops, and visual flickering",
        "Compare against SkiaSharp 3.119.4 (m145) to confirm regression"
      ],
      "environmentDetails": "Windows, Visual Studio, SkiaSharp 4.147-preview1.1 and later (m146+)",
      "repoLinks": [
        {
          "url": "https://github.com/mono/SkiaSharp/pull/4428",
          "description": "Fix PR: Restore SK_ENABLE_LEGACY_SHADERCONTEXT to fix CPU raster perf regression"
        }
      ]
    },
    "versionAnalysis": {
      "mentionedVersions": [
        "4.147-preview1.1",
        "4.148.0",
        "4.150.0",
        "3.119.4"
      ],
      "workedIn": "3.119.4",
      "brokeIn": "4.147-preview1.1",
      "currentRelevance": "likely",
      "relevanceReason": "Regression still present in m151; fix PR #4428 is open but not yet merged"
    },
    "regression": {
      "isRegression": true,
      "confidence": 0.99,
      "reason": "Upstream Skia commit 356c7267fa removed the default definition of SK_ENABLE_LEGACY_SHADERCONTEXT from include/core/SkTypes.h in m146. Reporter and maintainer confirmed this is the exact cause via custom builds.",
      "workedInVersion": "3.119.4",
      "brokeInVersion": "4.147-preview1.1"
    },
    "fixStatus": {
      "likelyFixed": false,
      "confidence": 0.95,
      "reason": "Fix PR #4428 is open but not yet merged; adding -DSK_ENABLE_LEGACY_SHADERCONTEXT to extra_cflags in native build configs is the sanctioned fix",
      "relatedPRs": [
        4428
      ]
    }
  },
  "analysis": {
    "summary": "Upstream Skia m146 removed SK_ENABLE_LEGACY_SHADERCONTEXT as a default define, causing SkShaderBase::makeContext() to always return nullptr. This forces SkBlitter.cpp to use the generic raster-pipeline blitter (CreateSkRPBlitter) instead of the hand-optimized SkARGB32_Shader_Blitter for N32+SrcOver draws with a shader. The fix is to add -DSK_ENABLE_LEGACY_SHADERCONTEXT to extra_cflags in all CPU-raster native/*/build.cake files.",
    "rationale": "Clear performance regression with exact upstream commit identified (356c7267fa), reporter and maintainer both confirmed root cause via custom builds, fix path is unambiguous (add define to build flags), fix PR #4428 already opened by maintainer. Classification as libSkiaSharp.native because the fix is purely in native build configuration, not C# code.",
    "keySignals": [
      {
        "text": "regression was traced back to upstream Google Skia commit 356c7267fa ('Remove SK_DISABLE_LEGACY_SHADERCONTEXT'), which removed the default definition of the SK_ENABLE_LEGACY_SHADERCONTEXT macro",
        "source": "issue body",
        "interpretation": "Pinpoints exact upstream commit as root cause; reporter did thorough bisection analysis"
      },
      {
        "text": "pages with background images can sometimes take 1-2s to render with displaying black in the meanwhile",
        "source": "issue body",
        "interpretation": "Severity is high — visible to end users, makes the app appear frozen"
      },
      {
        "text": "I've opened #4428 which turns SK_ENABLE_LEGACY_SHADERCONTEXT back on across the native builds",
        "source": "maintainer comment",
        "interpretation": "Fix PR is already open; this issue is ready-to-fix pending CI green"
      },
      {
        "text": "extra_cflags=[ '-DSKIA_C_DLL', '-DSK_AVOID_SLOW_RASTER_PIPELINE_BLURS', '/MT{d}', ...] (no SK_ENABLE_LEGACY_SHADERCONTEXT)",
        "source": "native/windows/build.cake line 65",
        "interpretation": "Confirms the define is currently absent from the Windows native build flags"
      }
    ],
    "codeInvestigation": [
      {
        "file": "native/windows/build.cake",
        "lines": "65",
        "finding": "extra_cflags contains '-DSKIA_C_DLL' and '-DSK_AVOID_SLOW_RASTER_PIPELINE_BLURS' but NOT '-DSK_ENABLE_LEGACY_SHADERCONTEXT' — confirms the regression flag is absent from Windows native build",
        "relevance": "direct"
      },
      {
        "file": "native/winui/build.cake",
        "lines": "143",
        "finding": "WinUI-specific extra_cflags only has '/guard:cf' and '/GS' — no raster perf flags; this is the WinRT projection helper, not the main raster build path",
        "relevance": "context"
      }
    ],
    "nextQuestions": [
      "Has the reporter confirmed fix on m150/m151 binaries from PR #4428 CI artifacts?",
      "Is this a temporary measure until Skia removes the legacy path entirely — should an upstream issue/discussion be filed on skia-discuss?"
    ],
    "resolution": {
      "hypothesis": "Adding -DSK_ENABLE_LEGACY_SHADERCONTEXT to extra_cflags in all CPU-raster native/*/build.cake files restores the fast legacy blitter path and eliminates the performance regression.",
      "proposals": [
        {
          "title": "Add -DSK_ENABLE_LEGACY_SHADERCONTEXT to all CPU-raster native builds",
          "description": "Add '-DSK_ENABLE_LEGACY_SHADERCONTEXT' next to the existing '-DSK_AVOID_SLOW_RASTER_PIPELINE_BLURS' flag in extra_cflags in native/windows/build.cake (and all other CPU-raster platform build.cake files). PR #4428 already implements this across windows, macos, ios, tvos, linux, android, tizen, and wasm.",
          "category": "fix",
          "confidence": 0.97,
          "effort": "cost/xs",
          "validated": "yes"
        }
      ],
      "recommendedProposal": "Add -DSK_ENABLE_LEGACY_SHADERCONTEXT to all CPU-raster native builds",
      "recommendedReason": "Root cause is confirmed, fix is minimal and low-risk (restores pre-m146 default behavior), PR #4428 is ready for CI verification."
    }
  },
  "output": {
    "actionability": {
      "suggestedAction": "ready-to-fix",
      "confidence": 0.97,
      "reason": "Root cause confirmed by both reporter and maintainer; fix PR #4428 is open and awaiting CI; the define is the sanctioned upstream opt-in mechanism.",
      "suggestedReproPlatform": "windows"
    },
    "actions": [
      {
        "type": "update-labels",
        "description": "Apply area, platform, backend, and performance tenet labels",
        "risk": "low",
        "confidence": 0.97,
        "labels": [
          "type/bug",
          "area/libSkiaSharp.native",
          "os/Windows-Classic",
          "os/Windows-WinUI",
          "backend/Raster",
          "tenet/performance",
          "perf/rendering"
        ]
      },
      {
        "type": "link-related",
        "description": "Cross-reference fix PR #4428",
        "risk": "low",
        "confidence": 0.99,
        "linkedIssue": 4428
      },
      {
        "type": "set-milestone",
        "description": "Set milestone to 4.151.0-rc.1 matching the current open milestone",
        "risk": "low",
        "confidence": 0.9,
        "milestone": "4.151.0-rc.1"
      }
    ]
  }
}
```

</details>
