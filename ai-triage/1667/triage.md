# Issue Triage Report — #1667

| Field | Value |
|-------|-------|
| Repository | mono/SkiaSharp |
| Analyzed | 2026-05-06T15:23:27Z |
| Type | type/question (0.95 (95%)) |
| Area | area/SkiaSharp.Views (0.85 (85%)) |
| Suggested action | needs-info (0.90 (90%)) |

**Issue Summary:** User asking whether SkiaSharp's AngleSwapChainPanel (UWP ANGLE/OpenGL ES rendering surface) works on Xbox One.

**Analysis:** A single-sentence question asking whether AngleSwapChainPanel works on Xbox One. No code, no error messages, no repro steps, no version info. Xbox One supports UWP apps, but the ANGLE (DirectX-based OpenGL ES) implementation may not be fully supported on Xbox's restricted UWP environment.

**Recommendations:** **needs-info** — Single-sentence question with no code, no error, no version info, and no reproduction steps. Cannot determine compatibility or diagnose an issue without more context.

---

## Classification

| Field | Value |
|-------|-------|
| Type | type/question |
| Area | area/SkiaSharp.Views |
| Platforms | os/Windows-Universal-UWP |
| Backends | backend/OpenGL |
| Tenets | — |
| Partner | — |

## Evidence

### Reproduction

**Environment:** Xbox One running UWP app (inferred). No SkiaSharp version specified.

**Repository links:**
- https://github.com/mono/SkiaSharp/issues/803 — Related question: 'Does Skia or SkiaSharp run on Console Platforms, like PS4 or XBOX 360' — closed as completed

## Analysis

### Technical Summary

A single-sentence question asking whether AngleSwapChainPanel works on Xbox One. No code, no error messages, no repro steps, no version info. Xbox One supports UWP apps, but the ANGLE (DirectX-based OpenGL ES) implementation may not be fully supported on Xbox's restricted UWP environment.

### Rationale

Classified as type/question because the reporter is asking about platform compatibility, not reporting broken behavior. The issue has no code, no error message, and no repro steps. The 'needs-info' action is appropriate because we cannot answer the question without knowing what was tried and what failures occurred. AngleSwapChainPanel is a UWP component that uses ANGLE (D3D-backed OpenGL ES); Xbox One's UWP support is constrained and we do not have documented compatibility data to answer definitively.

### Key Signals

- "Does Skia with AngleSwapChainPanel work on xbox one?" — **issue body** (Straightforward compatibility/usage question. No error or code provided.)

### Code Investigation

| File | Lines | Relevance | Finding |
|------|-------|-----------|---------|
| `source/SkiaSharp.Views/SkiaSharp.Views.WinUI/AngleSwapChainPanel.cs` | 1-20 | direct | AngleSwapChainPanel extends UWP SwapChainPanel using Microsoft.UI.Xaml and relies on GlesContext (ANGLE/EGL). It's a WinUI/UWP-only component requiring ANGLE support in the target environment. |
| `source/SkiaSharp.Views/SkiaSharp.Views.WinUI/AngleSwapChainPanel.cs` | 126-135 | related | Context creation (GlesContext) happens in OnLoaded via EGL surface creation backed by the SwapChainPanel. Xbox One UWP has a restricted DirectX subset; whether ANGLE's EGL backend works depends on Xbox's D3D feature support. |

### Workarounds

- Use SKCanvasView (software/CPU raster) if ANGLE/OpenGL ES is unavailable on Xbox One UWP
- Check #803 for prior discussion on console platform support

### Next Questions

- What SkiaSharp version is being used?
- Is the target UWP or WinUI?
- What specific failure is observed on Xbox One (DllNotFoundException, blank rendering, exception)?
- Does ANGLE (EGL/OpenGL ES) initialize successfully on Xbox One?

### Resolution Proposals

**Hypothesis:** The reporter wants to use hardware-accelerated OpenGL ES rendering via ANGLE on Xbox One UWP. Xbox One's restricted UWP environment may not support the required ANGLE feature set.

1. **Request more information** — investigation, confidence 0.90 (90%), cost/xs, validated=untested
   - Ask what version of SkiaSharp is being used, whether they are targeting UWP or WinUI, what specific errors occur, and whether ANGLE initializes at all on Xbox One.
2. **Use software raster fallback** — workaround, confidence 0.75 (75%), cost/s, validated=untested
   - If ANGLE does not work on Xbox One, use SKCanvasView (backed by the CPU raster backend) rather than SKGLView/AngleSwapChainPanel. This avoids the ANGLE dependency entirely.

**Recommended proposal:** Request more information

**Why:** The issue is too sparse to answer or diagnose. We need to know what failure the reporter is seeing before advising on compatibility.

## Recommendations

### Actionability

| Field | Value |
|-------|-------|
| Suggested action | needs-info |
| Confidence | 0.90 (90%) |
| Reason | Single-sentence question with no code, no error, no version info, and no reproduction steps. Cannot determine compatibility or diagnose an issue without more context. |
| Suggested repro platform | windows |

### Missing Info

- SkiaSharp version
- UWP or WinUI target framework
- Specific error or failure observed on Xbox One
- Whether ANGLE/EGL initializes successfully

### Automatable Actions

| Type | Risk | Confidence | Description | Details |
|------|------|------------|-------------|---------|
| update-labels | low | 0.95 (95%) | Apply question, views, and UWP labels | labels=type/question, area/SkiaSharp.Views, os/Windows-Universal-UWP |
| add-comment | medium | 0.90 (90%) | Ask for version, target framework, and specific failure | — |

**Comment draft for `add-comment`:**

```markdown
Thanks for the question! To help you better, could you provide a bit more context?

- **SkiaSharp version** you're using
- **Target framework** — UWP (`uap10.0`) or WinUI 3 (`net8.0-windows10.x`)
- **What specific failure are you seeing?** For example: does the app crash on startup, is the screen blank, or do you get an exception?
- **Does ANGLE initialize at all?** (i.e., does `OnRenderFrame` get called?)

Xbox One runs a restricted subset of UWP APIs. `AngleSwapChainPanel` relies on ANGLE (DirectX-backed OpenGL ES via EGL), and we'd need to know whether the D3D feature set Xbox One exposes is sufficient for ANGLE to initialize.

If ANGLE doesn't work, a fallback would be to use `SKCanvasView` (CPU/raster backend) which doesn't depend on OpenGL ES.
```

<details>
<summary>Raw JSON</summary>

```json
{
  "meta": {
    "schemaVersion": "1.0",
    "number": 1667,
    "repo": "mono/SkiaSharp",
    "analyzedAt": "2026-05-06T15:23:27Z"
  },
  "summary": "User asking whether SkiaSharp's AngleSwapChainPanel (UWP ANGLE/OpenGL ES rendering surface) works on Xbox One.",
  "classification": {
    "type": {
      "value": "type/question",
      "confidence": 0.95
    },
    "area": {
      "value": "area/SkiaSharp.Views",
      "confidence": 0.85
    },
    "platforms": [
      "os/Windows-Universal-UWP"
    ],
    "backends": [
      "backend/OpenGL"
    ]
  },
  "evidence": {
    "reproEvidence": {
      "environmentDetails": "Xbox One running UWP app (inferred). No SkiaSharp version specified.",
      "repoLinks": [
        {
          "url": "https://github.com/mono/SkiaSharp/issues/803",
          "description": "Related question: 'Does Skia or SkiaSharp run on Console Platforms, like PS4 or XBOX 360' — closed as completed"
        }
      ]
    }
  },
  "analysis": {
    "summary": "A single-sentence question asking whether AngleSwapChainPanel works on Xbox One. No code, no error messages, no repro steps, no version info. Xbox One supports UWP apps, but the ANGLE (DirectX-based OpenGL ES) implementation may not be fully supported on Xbox's restricted UWP environment.",
    "codeInvestigation": [
      {
        "file": "source/SkiaSharp.Views/SkiaSharp.Views.WinUI/AngleSwapChainPanel.cs",
        "lines": "1-20",
        "finding": "AngleSwapChainPanel extends UWP SwapChainPanel using Microsoft.UI.Xaml and relies on GlesContext (ANGLE/EGL). It's a WinUI/UWP-only component requiring ANGLE support in the target environment.",
        "relevance": "direct"
      },
      {
        "file": "source/SkiaSharp.Views/SkiaSharp.Views.WinUI/AngleSwapChainPanel.cs",
        "lines": "126-135",
        "finding": "Context creation (GlesContext) happens in OnLoaded via EGL surface creation backed by the SwapChainPanel. Xbox One UWP has a restricted DirectX subset; whether ANGLE's EGL backend works depends on Xbox's D3D feature support.",
        "relevance": "related"
      }
    ],
    "keySignals": [
      {
        "text": "Does Skia with AngleSwapChainPanel work on xbox one?",
        "source": "issue body",
        "interpretation": "Straightforward compatibility/usage question. No error or code provided."
      }
    ],
    "rationale": "Classified as type/question because the reporter is asking about platform compatibility, not reporting broken behavior. The issue has no code, no error message, and no repro steps. The 'needs-info' action is appropriate because we cannot answer the question without knowing what was tried and what failures occurred. AngleSwapChainPanel is a UWP component that uses ANGLE (D3D-backed OpenGL ES); Xbox One's UWP support is constrained and we do not have documented compatibility data to answer definitively.",
    "workarounds": [
      "Use SKCanvasView (software/CPU raster) if ANGLE/OpenGL ES is unavailable on Xbox One UWP",
      "Check #803 for prior discussion on console platform support"
    ],
    "nextQuestions": [
      "What SkiaSharp version is being used?",
      "Is the target UWP or WinUI?",
      "What specific failure is observed on Xbox One (DllNotFoundException, blank rendering, exception)?",
      "Does ANGLE (EGL/OpenGL ES) initialize successfully on Xbox One?"
    ],
    "resolution": {
      "hypothesis": "The reporter wants to use hardware-accelerated OpenGL ES rendering via ANGLE on Xbox One UWP. Xbox One's restricted UWP environment may not support the required ANGLE feature set.",
      "proposals": [
        {
          "title": "Request more information",
          "description": "Ask what version of SkiaSharp is being used, whether they are targeting UWP or WinUI, what specific errors occur, and whether ANGLE initializes at all on Xbox One.",
          "category": "investigation",
          "confidence": 0.9,
          "effort": "cost/xs",
          "validated": "untested"
        },
        {
          "title": "Use software raster fallback",
          "description": "If ANGLE does not work on Xbox One, use SKCanvasView (backed by the CPU raster backend) rather than SKGLView/AngleSwapChainPanel. This avoids the ANGLE dependency entirely.",
          "category": "workaround",
          "confidence": 0.75,
          "effort": "cost/s",
          "validated": "untested"
        }
      ],
      "recommendedProposal": "Request more information",
      "recommendedReason": "The issue is too sparse to answer or diagnose. We need to know what failure the reporter is seeing before advising on compatibility."
    }
  },
  "output": {
    "actionability": {
      "suggestedAction": "needs-info",
      "confidence": 0.9,
      "reason": "Single-sentence question with no code, no error, no version info, and no reproduction steps. Cannot determine compatibility or diagnose an issue without more context.",
      "suggestedReproPlatform": "windows"
    },
    "missingInfo": [
      "SkiaSharp version",
      "UWP or WinUI target framework",
      "Specific error or failure observed on Xbox One",
      "Whether ANGLE/EGL initializes successfully"
    ],
    "actions": [
      {
        "type": "update-labels",
        "description": "Apply question, views, and UWP labels",
        "risk": "low",
        "confidence": 0.95,
        "labels": [
          "type/question",
          "area/SkiaSharp.Views",
          "os/Windows-Universal-UWP"
        ]
      },
      {
        "type": "add-comment",
        "description": "Ask for version, target framework, and specific failure",
        "risk": "medium",
        "confidence": 0.9,
        "comment": "Thanks for the question! To help you better, could you provide a bit more context?\n\n- **SkiaSharp version** you're using\n- **Target framework** — UWP (`uap10.0`) or WinUI 3 (`net8.0-windows10.x`)\n- **What specific failure are you seeing?** For example: does the app crash on startup, is the screen blank, or do you get an exception?\n- **Does ANGLE initialize at all?** (i.e., does `OnRenderFrame` get called?)\n\nXbox One runs a restricted subset of UWP APIs. `AngleSwapChainPanel` relies on ANGLE (DirectX-backed OpenGL ES via EGL), and we'd need to know whether the D3D feature set Xbox One exposes is sufficient for ANGLE to initialize.\n\nIf ANGLE doesn't work, a fallback would be to use `SKCanvasView` (CPU/raster backend) which doesn't depend on OpenGL ES."
      }
    ]
  }
}
```

</details>
