# Issue Triage Report — #2936

| Field | Value |
|-------|-------|
| Repository | mono/SkiaSharp |
| Analyzed | 2026-05-06T05:20:44Z |
| Type | type/bug (0.95 (95%)) |
| Area | area/SkiaSharp.Views.Maui (0.90 (90%)) |
| Suggested action | needs-investigation (0.82 (82%)) |

**Issue Summary:** SKGLView on Windows (.NET MAUI) flashes white for ~200ms when made visible in Release configuration with an NVIDIA GPU, but works correctly in Debug mode and with Intel integrated GPU.

**Analysis:** The white flash is likely caused by ANGLE/OpenGL context initialization timing on NVIDIA GPUs in Release mode. In AngleSwapChainPanel.RenderFrame(), a SwapBuffers() call occurs when pendingSizeChange is true (line 224), which swaps the cleared (white) backbuffer to the screen before the first SkiaSharp frame is painted. NVIDIA drivers may flush/present this intermediate cleared state more aggressively than Intel integrated GPU drivers, making the white flash visible. Release-mode JIT optimizations may also affect timing, widening the initialization window.

**Recommendations:** **needs-investigation** — Real visual bug with specific but narrow conditions (NVIDIA GPU + Release mode + Windows). Root cause is identified in the ANGLE swap chain initialization code. Needs investigation to confirm the fix approach without breaking buffer management.

---

## Classification

| Field | Value |
|-------|-------|
| Type | type/bug |
| Area | area/SkiaSharp.Views.Maui |
| Platforms | os/Windows-Classic |
| Backends | backend/OpenGL |
| Tenets | — |
| Partner | partner/maui |
| Current labels | type/bug |

## Evidence

### Reproduction

1. Create a .NET MAUI 9.0 app with SKGLView
2. Run in Release configuration on Windows with NVIDIA GPU
3. Make the SKGLView visible (show from hidden state or navigate to page)
4. Observe white flash for approximately 200ms before content renders

**Environment:** .NET MAUI 9.0 Preview 6, SkiaSharp 3.0 Preview 3.1, Windows 11, NVIDIA GPU (Release mode only)

**Repository links:**
- https://github.com/hyvanmielenpelit/GnollHack — Reporter's application where the issue was observed

### Bug Signals

| Field | Value |
|-------|-------|
| Severity | medium |
| Regression claimed | False |
| Error type | wrong-output |
| Error message | SKGLView displays white frame for ~200ms on first visibility in Release+NVIDIA configuration |
| Repro quality | partial |
| Target frameworks | net9.0-windows10.0 |

### Version Analysis

| Field | Value |
|-------|-------|
| Mentioned versions | 3.x (Alpha), 3.0 Preview 3.1 |
| Worked in | — |
| Broke in | — |
| Current relevance | likely |
| Relevance reason | The AngleSwapChainPanel and SKSwapChainPanel code path on Windows has not seen changes that would address GPU-specific initialization timing. |

## Analysis

### Technical Summary

The white flash is likely caused by ANGLE/OpenGL context initialization timing on NVIDIA GPUs in Release mode. In AngleSwapChainPanel.RenderFrame(), a SwapBuffers() call occurs when pendingSizeChange is true (line 224), which swaps the cleared (white) backbuffer to the screen before the first SkiaSharp frame is painted. NVIDIA drivers may flush/present this intermediate cleared state more aggressively than Intel integrated GPU drivers, making the white flash visible. Release-mode JIT optimizations may also affect timing, widening the initialization window.

### Rationale

The reporter clearly describes wrong visual output (white flash) under specific conditions (NVIDIA GPU, Release mode, Windows). The code investigation reveals a SwapBuffers call in the size-change path that presents a cleared buffer before content is drawn. This matches the GPU-specific behavior since NVIDIA discrete drivers handle buffer swaps differently from Intel integrated. This is a real bug, not a usage error.

### Key Signals

- "With integrated GPU (Intel UHD Graphics), SKGLView does not flash white but works properly." — **issue body** (The bug is GPU/driver-specific, pointing to ANGLE initialization timing differences between NVIDIA and Intel GPU drivers.)
- "Both debug configuration with NVIDIA and integrated GPU in release mode work fine." — **issue body** (Release mode optimization changes timing — likely widens the window between surface creation and first content render.)
- "we already made a workaround by adding a black frame on the top of the flashing SKGLView for the first 250ms" — **issue body** (Reporter has a functional workaround, reducing urgency, but confirms the bug is real and reproducible.)

### Code Investigation

| File | Lines | Relevance | Finding |
|------|-------|-----------|---------|
| `source/SkiaSharp.Views/SkiaSharp.Views.WinUI/AngleSwapChainPanel.cs` | 218-237 | direct | RenderFrame() calls glesContext.SwapBuffers() when pendingSizeChange is true (lines 224-225), before OnRenderFrame() paints any content. This swaps the cleared (white) backbuffer to the screen, causing the white flash visible on first render. |
| `source/SkiaSharp.Views/SkiaSharp.Views.WinUI/SKSwapChainPanel.cs` | 43-44 | direct | OnRenderFrame() calls glClear() with GL_COLOR_BUFFER_BIT as the very first operation, clearing the backbuffer to the default clear color (transparent/white) before context or surface are guaranteed to exist. |
| `source/SkiaSharp.Views.Maui/SkiaSharp.Views.Maui.Core/Handlers/SKGLView/SKGLViewHandler.Windows.cs` | 13-135 | related | Windows handler uses SKSwapChainPanel (via MauiSKSwapChainPanel). No special handling for initialization delay or first-frame suppression — content is expected immediately after Loaded. |

### Workarounds

- Cover the SKGLView with a solid-color overlay for the first 250ms after showing (reporter's approach using a black frame)
- Pre-initialize the SKGLView before making it visible (load the page/view ahead of time to allow GL context initialization to complete before visibility)

### Next Questions

- Is the SwapBuffers call during pendingSizeChange intentional for correct ANGLE buffer management, or is it a bug?
- Does the issue also occur with other NVIDIA GPU configurations (desktop vs laptop, different driver versions)?
- Is this reproducible with MAUI 9.0 GA / SkiaSharp 3.0 stable release?

### Resolution Proposals

**Hypothesis:** The premature SwapBuffers() call during size-change initialization presents a cleared (white) framebuffer to the screen before the first content frame is drawn, and NVIDIA GPU drivers expose this intermediate state more visibly than Intel integrated drivers.

1. **Skip SwapBuffers on first frame** — fix, confidence 0.65 (65%), cost/s, validated=untested
   - Track whether the first content frame has been painted and skip the pendingSizeChange SwapBuffers call until after the first OnRenderFrame call completes successfully. This prevents presenting the cleared buffer.
2. **Set background in XAML/WinUI** — workaround, confidence 0.55 (55%), cost/xs, validated=untested
   - Set the SwapChainPanel background color to match the intended background before ANGLE renders, so the cleared buffer shows a matching color instead of white.

**Recommended proposal:** Skip SwapBuffers on first frame

**Why:** Addresses the root cause by preventing the empty cleared buffer from being presented. Less likely to introduce visual artifacts than color matching.

## Recommendations

### Actionability

| Field | Value |
|-------|-------|
| Suggested action | needs-investigation |
| Confidence | 0.82 (82%) |
| Reason | Real visual bug with specific but narrow conditions (NVIDIA GPU + Release mode + Windows). Root cause is identified in the ANGLE swap chain initialization code. Needs investigation to confirm the fix approach without breaking buffer management. |
| Suggested repro platform | windows |

### Automatable Actions

| Type | Risk | Confidence | Description | Details |
|------|------|------------|-------------|---------|
| update-labels | low | 0.90 (90%) | Apply MAUI views area, Windows platform, OpenGL backend, and MAUI partner labels | labels=type/bug, area/SkiaSharp.Views.Maui, os/Windows-Classic, backend/OpenGL, partner/maui |
| add-comment | medium | 0.82 (82%) | Ask reporter to confirm reproducibility on stable SkiaSharp 3.x and provide GPU driver version | — |

**Comment draft for `add-comment`:**

```markdown
Thanks for the detailed report! The white flash appears to be caused by the ANGLE swap chain presenting a cleared (white) framebuffer before SkiaSharp has had a chance to paint the first frame. This is visible on NVIDIA discrete GPUs because their drivers flush the buffer swap more aggressively than Intel integrated graphics.

A workaround you're already using (overlay for 250ms) is the most reliable option until a fix is available. You could also try pre-loading the page/view before making it visible to give the GL context time to initialize.

Could you confirm:
1. Does this still reproduce with SkiaSharp 3.x stable (non-preview) and .NET MAUI 9.0 GA?
2. What NVIDIA driver version are you using?
3. Does setting `HasRenderLoop = true` on the SKGLView change the behavior?
```

<details>
<summary>Raw JSON</summary>

```json
{
  "meta": {
    "schemaVersion": "1.0",
    "number": 2936,
    "repo": "mono/SkiaSharp",
    "analyzedAt": "2026-05-06T05:20:44Z",
    "currentLabels": [
      "type/bug"
    ]
  },
  "summary": "SKGLView on Windows (.NET MAUI) flashes white for ~200ms when made visible in Release configuration with an NVIDIA GPU, but works correctly in Debug mode and with Intel integrated GPU.",
  "classification": {
    "type": {
      "value": "type/bug",
      "confidence": 0.95
    },
    "area": {
      "value": "area/SkiaSharp.Views.Maui",
      "confidence": 0.9
    },
    "platforms": [
      "os/Windows-Classic"
    ],
    "backends": [
      "backend/OpenGL"
    ],
    "partner": "partner/maui"
  },
  "evidence": {
    "bugSignals": {
      "severity": "medium",
      "regressionClaimed": false,
      "errorType": "wrong-output",
      "errorMessage": "SKGLView displays white frame for ~200ms on first visibility in Release+NVIDIA configuration",
      "reproQuality": "partial",
      "targetFrameworks": [
        "net9.0-windows10.0"
      ]
    },
    "reproEvidence": {
      "stepsToReproduce": [
        "Create a .NET MAUI 9.0 app with SKGLView",
        "Run in Release configuration on Windows with NVIDIA GPU",
        "Make the SKGLView visible (show from hidden state or navigate to page)",
        "Observe white flash for approximately 200ms before content renders"
      ],
      "environmentDetails": ".NET MAUI 9.0 Preview 6, SkiaSharp 3.0 Preview 3.1, Windows 11, NVIDIA GPU (Release mode only)",
      "repoLinks": [
        {
          "url": "https://github.com/hyvanmielenpelit/GnollHack",
          "description": "Reporter's application where the issue was observed"
        }
      ]
    },
    "versionAnalysis": {
      "mentionedVersions": [
        "3.x (Alpha)",
        "3.0 Preview 3.1"
      ],
      "currentRelevance": "likely",
      "relevanceReason": "The AngleSwapChainPanel and SKSwapChainPanel code path on Windows has not seen changes that would address GPU-specific initialization timing."
    }
  },
  "analysis": {
    "summary": "The white flash is likely caused by ANGLE/OpenGL context initialization timing on NVIDIA GPUs in Release mode. In AngleSwapChainPanel.RenderFrame(), a SwapBuffers() call occurs when pendingSizeChange is true (line 224), which swaps the cleared (white) backbuffer to the screen before the first SkiaSharp frame is painted. NVIDIA drivers may flush/present this intermediate cleared state more aggressively than Intel integrated GPU drivers, making the white flash visible. Release-mode JIT optimizations may also affect timing, widening the initialization window.",
    "rationale": "The reporter clearly describes wrong visual output (white flash) under specific conditions (NVIDIA GPU, Release mode, Windows). The code investigation reveals a SwapBuffers call in the size-change path that presents a cleared buffer before content is drawn. This matches the GPU-specific behavior since NVIDIA discrete drivers handle buffer swaps differently from Intel integrated. This is a real bug, not a usage error.",
    "keySignals": [
      {
        "text": "With integrated GPU (Intel UHD Graphics), SKGLView does not flash white but works properly.",
        "source": "issue body",
        "interpretation": "The bug is GPU/driver-specific, pointing to ANGLE initialization timing differences between NVIDIA and Intel GPU drivers."
      },
      {
        "text": "Both debug configuration with NVIDIA and integrated GPU in release mode work fine.",
        "source": "issue body",
        "interpretation": "Release mode optimization changes timing — likely widens the window between surface creation and first content render."
      },
      {
        "text": "we already made a workaround by adding a black frame on the top of the flashing SKGLView for the first 250ms",
        "source": "issue body",
        "interpretation": "Reporter has a functional workaround, reducing urgency, but confirms the bug is real and reproducible."
      }
    ],
    "codeInvestigation": [
      {
        "file": "source/SkiaSharp.Views/SkiaSharp.Views.WinUI/AngleSwapChainPanel.cs",
        "lines": "218-237",
        "finding": "RenderFrame() calls glesContext.SwapBuffers() when pendingSizeChange is true (lines 224-225), before OnRenderFrame() paints any content. This swaps the cleared (white) backbuffer to the screen, causing the white flash visible on first render.",
        "relevance": "direct"
      },
      {
        "file": "source/SkiaSharp.Views/SkiaSharp.Views.WinUI/SKSwapChainPanel.cs",
        "lines": "43-44",
        "finding": "OnRenderFrame() calls glClear() with GL_COLOR_BUFFER_BIT as the very first operation, clearing the backbuffer to the default clear color (transparent/white) before context or surface are guaranteed to exist.",
        "relevance": "direct"
      },
      {
        "file": "source/SkiaSharp.Views.Maui/SkiaSharp.Views.Maui.Core/Handlers/SKGLView/SKGLViewHandler.Windows.cs",
        "lines": "13-135",
        "finding": "Windows handler uses SKSwapChainPanel (via MauiSKSwapChainPanel). No special handling for initialization delay or first-frame suppression — content is expected immediately after Loaded.",
        "relevance": "related"
      }
    ],
    "workarounds": [
      "Cover the SKGLView with a solid-color overlay for the first 250ms after showing (reporter's approach using a black frame)",
      "Pre-initialize the SKGLView before making it visible (load the page/view ahead of time to allow GL context initialization to complete before visibility)"
    ],
    "nextQuestions": [
      "Is the SwapBuffers call during pendingSizeChange intentional for correct ANGLE buffer management, or is it a bug?",
      "Does the issue also occur with other NVIDIA GPU configurations (desktop vs laptop, different driver versions)?",
      "Is this reproducible with MAUI 9.0 GA / SkiaSharp 3.0 stable release?"
    ],
    "resolution": {
      "hypothesis": "The premature SwapBuffers() call during size-change initialization presents a cleared (white) framebuffer to the screen before the first content frame is drawn, and NVIDIA GPU drivers expose this intermediate state more visibly than Intel integrated drivers.",
      "proposals": [
        {
          "title": "Skip SwapBuffers on first frame",
          "description": "Track whether the first content frame has been painted and skip the pendingSizeChange SwapBuffers call until after the first OnRenderFrame call completes successfully. This prevents presenting the cleared buffer.",
          "category": "fix",
          "confidence": 0.65,
          "effort": "cost/s",
          "validated": "untested"
        },
        {
          "title": "Set background in XAML/WinUI",
          "description": "Set the SwapChainPanel background color to match the intended background before ANGLE renders, so the cleared buffer shows a matching color instead of white.",
          "category": "workaround",
          "confidence": 0.55,
          "effort": "cost/xs",
          "validated": "untested"
        }
      ],
      "recommendedProposal": "Skip SwapBuffers on first frame",
      "recommendedReason": "Addresses the root cause by preventing the empty cleared buffer from being presented. Less likely to introduce visual artifacts than color matching."
    }
  },
  "output": {
    "actionability": {
      "suggestedAction": "needs-investigation",
      "confidence": 0.82,
      "reason": "Real visual bug with specific but narrow conditions (NVIDIA GPU + Release mode + Windows). Root cause is identified in the ANGLE swap chain initialization code. Needs investigation to confirm the fix approach without breaking buffer management.",
      "suggestedReproPlatform": "windows"
    },
    "actions": [
      {
        "type": "update-labels",
        "description": "Apply MAUI views area, Windows platform, OpenGL backend, and MAUI partner labels",
        "risk": "low",
        "confidence": 0.9,
        "labels": [
          "type/bug",
          "area/SkiaSharp.Views.Maui",
          "os/Windows-Classic",
          "backend/OpenGL",
          "partner/maui"
        ]
      },
      {
        "type": "add-comment",
        "description": "Ask reporter to confirm reproducibility on stable SkiaSharp 3.x and provide GPU driver version",
        "risk": "medium",
        "confidence": 0.82,
        "comment": "Thanks for the detailed report! The white flash appears to be caused by the ANGLE swap chain presenting a cleared (white) framebuffer before SkiaSharp has had a chance to paint the first frame. This is visible on NVIDIA discrete GPUs because their drivers flush the buffer swap more aggressively than Intel integrated graphics.\n\nA workaround you're already using (overlay for 250ms) is the most reliable option until a fix is available. You could also try pre-loading the page/view before making it visible to give the GL context time to initialize.\n\nCould you confirm:\n1. Does this still reproduce with SkiaSharp 3.x stable (non-preview) and .NET MAUI 9.0 GA?\n2. What NVIDIA driver version are you using?\n3. Does setting `HasRenderLoop = true` on the SKGLView change the behavior?"
      }
    ]
  }
}
```

</details>
