# Issue Triage Report — #1235

| Field | Value |
|-------|-------|
| Repository | mono/SkiaSharp |
| Analyzed | 2026-05-06T12:56:03Z |
| Type | type/bug (0.88 (88%)) |
| Area | area/SkiaSharp.Views (0.90 (90%)) |
| Suggested action | needs-investigation (0.78 (78%)) |

**Issue Summary:** SKGLView (OpenGL) renders nothing on some macOS devices (iMac 27 Mid 2011, High Sierra) while SKCanvasView (CPU) works correctly — likely caused by hardware that cannot satisfy the required NSOpenGLPixelFormat attributes including Accelerated and 4x Multisample.

**Analysis:** SKGLView on macOS requests a hardware-accelerated OpenGL pixel format with 4x MSAA, double-buffering, and other attributes. On older GPUs (iMac 27 Mid 2011 with AMD Radeon HD 6770M on High Sierra), this pixel format combination may fail to initialize, causing the NSOpenGLView to be unusable and draw nothing. SKCanvasView uses CPU rasterization and is unaffected. The Mac Mini with Intel HD 4000 on Catalina works, suggesting a hardware capability or OS-level driver difference.

**Recommendations:** **needs-investigation** — Bug is plausible and code investigation confirms a likely root cause (no fallback for hardware that can't satisfy pixel format), but needs reproduction on the affected hardware to confirm. The reporter provided a repro project.

---

## Classification

| Field | Value |
|-------|-------|
| Type | type/bug |
| Area | area/SkiaSharp.Views |
| Platforms | os/macOS |
| Backends | backend/OpenGL |
| Tenets | — |
| Partner | — |

## Evidence

### Reproduction

1. Create a Xamarin.Mac app using SKGLView
2. Run on an iMac 27 Mid 2011 running High Sierra
3. Observe that the GL view renders nothing (left side empty)
4. Compare to SKCanvasView on same machine which renders correctly (right side)

**Environment:** SkiaSharp 1.68.2-preview.60, Visual Studio 2019 16.5.4, macOS High Sierra 10.13, iMac 27 Mid 2011. Works on Mac Mini End 2012, Intel HD 4000, macOS Catalina 10.15.4.

**Repository links:**
- https://github.com/charlenni/SkiaTest — Minimal reproduction project from reporter
- https://github.com/Mapsui/Mapsui/issues/892 — Related cross-repo issue in Mapsui

### Bug Signals

| Field | Value |
|-------|-------|
| Severity | medium |
| Regression claimed | False |
| Error type | missing-output |
| Error message | — |
| Repro quality | partial |
| Target frameworks | xamarin.mac |

### Version Analysis

| Field | Value |
|-------|-------|
| Mentioned versions | 1.68.2-preview.60 |
| Worked in | — |
| Broke in | — |
| Current relevance | likely |
| Relevance reason | The macOS SKGLView pixel format initialization code in source/SkiaSharp.Views/SkiaSharp.Views/Platform/macOS/SKGLView.cs still uses the same NSOpenGLPixelFormatAttribute combination requiring hardware acceleration and 4x MSAA, which would fail silently on older GPUs. |

## Analysis

### Technical Summary

SKGLView on macOS requests a hardware-accelerated OpenGL pixel format with 4x MSAA, double-buffering, and other attributes. On older GPUs (iMac 27 Mid 2011 with AMD Radeon HD 6770M on High Sierra), this pixel format combination may fail to initialize, causing the NSOpenGLView to be unusable and draw nothing. SKCanvasView uses CPU rasterization and is unaffected. The Mac Mini with Intel HD 4000 on Catalina works, suggesting a hardware capability or OS-level driver difference.

### Rationale

The reporter clearly describes blank rendering in SKGLView while SKCanvasView works, with device-specific behavior pointing to a GPU/driver compatibility issue. Code investigation confirms the pixel format attributes in Initialize() include hard requirements (Accelerated, Multisample, 4 samples) that may not be satisfiable on all hardware, and there is no fallback path if PixelFormat creation fails.

### Key Signals

- "On some Mac devices there isn't anything drawn in SKGLView on Mac" — **issue body** (Device-specific OpenGL failure — not a universal regression but hardware-dependent)
- "The left side for GPU is empty, the right side for CPU is working correct" — **issue body (screenshots description)** (CPU path (SKCanvasView) works; only the GPU/OpenGL path (SKGLView) is broken, narrowing the root cause to OpenGL context or surface creation)
- "But worked on a Mac Mini, End 2012, Intel HD 4000 graphics, Catalina (10.15.4)" — **issue body** (The issue is hardware- or OS-specific, not universal. Different GPU and newer OS works.)

### Code Investigation

| File | Lines | Relevance | Finding |
|------|-------|-----------|---------|
| `source/SkiaSharp.Views/SkiaSharp.Views/Platform/macOS/SKGLView.cs` | 62-82 | direct | Initialize() creates an NSOpenGLPixelFormat with hard requirements: Accelerated, DoubleBuffer, Multisample, 4 MSAA samples. If these attributes are not supported by the GPU/driver combination, NSOpenGLPixelFormat returns a null or invalid pixel format, and the NSOpenGLView will fail to draw anything. There is no fallback to a less-demanding pixel format. |
| `source/SkiaSharp.Views/SkiaSharp.Views/Platform/macOS/SKGLView.cs` | 88-94 | direct | PrepareOpenGL() calls GRGlInterface.Create() and GRContext.CreateGl(). If the pixel format was not properly initialized in Initialize(), GRContext creation may silently fail and the context field remains null, causing DrawRect to produce no output. |

### Workarounds

- Use SKCanvasView instead of SKGLView on macOS — CPU rendering path works correctly on the same hardware
- If OpenGL is required, try setting a less demanding pixel format by removing Multisample/Accelerated attributes (requires source modification)

### Next Questions

- Does removing NSOpenGLPixelFormatAttribute.Accelerated or reducing MSAA samples to 0 fix the issue on the affected iMac?
- Is NSOpenGLPixelFormat null after Initialize() on the affected device?
- Does the issue reproduce with current SkiaSharp versions (2.x/3.x) on the same hardware?

### Resolution Proposals

**Hypothesis:** The NSOpenGLPixelFormat created in Initialize() requires hardware acceleration and 4x MSAA which older GPUs on High Sierra cannot provide. If pixel format creation silently fails (returns nil), NSOpenGLView renders nothing. Adding a fallback pixel format without MSAA would fix the issue.

1. **Add fallback pixel format without MSAA** — fix, confidence 0.75 (75%), cost/s, validated=untested
   - If the primary pixel format (with Multisample/4 samples) fails to initialize, fall back to a simpler pixel format without multisampling. Check if PixelFormat is null/invalid after the initial assignment.
2. **Use SKCanvasView as workaround** — workaround, confidence 0.95 (95%), cost/xs, validated=untested
   - For users on older Mac hardware, SKCanvasView provides identical drawing API using CPU rasterization and works correctly on all devices.

**Recommended proposal:** Add fallback pixel format without MSAA

**Why:** Fixes the root cause for users who need GPU rendering. The fallback approach is low-risk and follows the pattern of handling optional GL capabilities.

## Recommendations

### Actionability

| Field | Value |
|-------|-------|
| Suggested action | needs-investigation |
| Confidence | 0.78 (78%) |
| Reason | Bug is plausible and code investigation confirms a likely root cause (no fallback for hardware that can't satisfy pixel format), but needs reproduction on the affected hardware to confirm. The reporter provided a repro project. |
| Suggested repro platform | macos |

### Automatable Actions

| Type | Risk | Confidence | Description | Details |
|------|------|------------|-------------|---------|
| update-labels | low | 0.90 (90%) | Apply bug, views, macOS, OpenGL labels | labels=type/bug, area/SkiaSharp.Views, os/macOS, backend/OpenGL |
| add-comment | medium | 0.78 (78%) | Ask for confirmation on current version and suggest SKCanvasView workaround | — |

**Comment draft for `add-comment`:**

```markdown
Thanks for the detailed report and repro project!

This looks like a hardware compatibility issue with the OpenGL pixel format that `SKGLView` requests on macOS. The `Initialize()` method requires hardware acceleration and 4x multisampling — older GPUs on High Sierra may not satisfy these requirements, causing the OpenGL context to silently fail.

**Workaround:** Use `SKCanvasView` instead of `SKGLView`. The CPU rendering path works correctly on the same hardware.

Could you confirm:
1. Does this still reproduce with the latest SkiaSharp version?
2. On the affected iMac, does anything appear in the console/Xcode output when the GL view fails to draw?
```

<details>
<summary>Raw JSON</summary>

```json
{
  "meta": {
    "schemaVersion": "1.0",
    "number": 1235,
    "repo": "mono/SkiaSharp",
    "analyzedAt": "2026-05-06T12:56:03Z"
  },
  "summary": "SKGLView (OpenGL) renders nothing on some macOS devices (iMac 27 Mid 2011, High Sierra) while SKCanvasView (CPU) works correctly — likely caused by hardware that cannot satisfy the required NSOpenGLPixelFormat attributes including Accelerated and 4x Multisample.",
  "classification": {
    "type": {
      "value": "type/bug",
      "confidence": 0.88
    },
    "area": {
      "value": "area/SkiaSharp.Views",
      "confidence": 0.9
    },
    "platforms": [
      "os/macOS"
    ],
    "backends": [
      "backend/OpenGL"
    ]
  },
  "evidence": {
    "bugSignals": {
      "severity": "medium",
      "regressionClaimed": false,
      "errorType": "missing-output",
      "reproQuality": "partial",
      "targetFrameworks": [
        "xamarin.mac"
      ]
    },
    "reproEvidence": {
      "stepsToReproduce": [
        "Create a Xamarin.Mac app using SKGLView",
        "Run on an iMac 27 Mid 2011 running High Sierra",
        "Observe that the GL view renders nothing (left side empty)",
        "Compare to SKCanvasView on same machine which renders correctly (right side)"
      ],
      "environmentDetails": "SkiaSharp 1.68.2-preview.60, Visual Studio 2019 16.5.4, macOS High Sierra 10.13, iMac 27 Mid 2011. Works on Mac Mini End 2012, Intel HD 4000, macOS Catalina 10.15.4.",
      "repoLinks": [
        {
          "url": "https://github.com/charlenni/SkiaTest",
          "description": "Minimal reproduction project from reporter"
        },
        {
          "url": "https://github.com/Mapsui/Mapsui/issues/892",
          "description": "Related cross-repo issue in Mapsui"
        }
      ]
    },
    "versionAnalysis": {
      "mentionedVersions": [
        "1.68.2-preview.60"
      ],
      "currentRelevance": "likely",
      "relevanceReason": "The macOS SKGLView pixel format initialization code in source/SkiaSharp.Views/SkiaSharp.Views/Platform/macOS/SKGLView.cs still uses the same NSOpenGLPixelFormatAttribute combination requiring hardware acceleration and 4x MSAA, which would fail silently on older GPUs."
    }
  },
  "analysis": {
    "summary": "SKGLView on macOS requests a hardware-accelerated OpenGL pixel format with 4x MSAA, double-buffering, and other attributes. On older GPUs (iMac 27 Mid 2011 with AMD Radeon HD 6770M on High Sierra), this pixel format combination may fail to initialize, causing the NSOpenGLView to be unusable and draw nothing. SKCanvasView uses CPU rasterization and is unaffected. The Mac Mini with Intel HD 4000 on Catalina works, suggesting a hardware capability or OS-level driver difference.",
    "rationale": "The reporter clearly describes blank rendering in SKGLView while SKCanvasView works, with device-specific behavior pointing to a GPU/driver compatibility issue. Code investigation confirms the pixel format attributes in Initialize() include hard requirements (Accelerated, Multisample, 4 samples) that may not be satisfiable on all hardware, and there is no fallback path if PixelFormat creation fails.",
    "keySignals": [
      {
        "text": "On some Mac devices there isn't anything drawn in SKGLView on Mac",
        "source": "issue body",
        "interpretation": "Device-specific OpenGL failure — not a universal regression but hardware-dependent"
      },
      {
        "text": "The left side for GPU is empty, the right side for CPU is working correct",
        "source": "issue body (screenshots description)",
        "interpretation": "CPU path (SKCanvasView) works; only the GPU/OpenGL path (SKGLView) is broken, narrowing the root cause to OpenGL context or surface creation"
      },
      {
        "text": "But worked on a Mac Mini, End 2012, Intel HD 4000 graphics, Catalina (10.15.4)",
        "source": "issue body",
        "interpretation": "The issue is hardware- or OS-specific, not universal. Different GPU and newer OS works."
      }
    ],
    "codeInvestigation": [
      {
        "file": "source/SkiaSharp.Views/SkiaSharp.Views/Platform/macOS/SKGLView.cs",
        "lines": "62-82",
        "finding": "Initialize() creates an NSOpenGLPixelFormat with hard requirements: Accelerated, DoubleBuffer, Multisample, 4 MSAA samples. If these attributes are not supported by the GPU/driver combination, NSOpenGLPixelFormat returns a null or invalid pixel format, and the NSOpenGLView will fail to draw anything. There is no fallback to a less-demanding pixel format.",
        "relevance": "direct"
      },
      {
        "file": "source/SkiaSharp.Views/SkiaSharp.Views/Platform/macOS/SKGLView.cs",
        "lines": "88-94",
        "finding": "PrepareOpenGL() calls GRGlInterface.Create() and GRContext.CreateGl(). If the pixel format was not properly initialized in Initialize(), GRContext creation may silently fail and the context field remains null, causing DrawRect to produce no output.",
        "relevance": "direct"
      }
    ],
    "nextQuestions": [
      "Does removing NSOpenGLPixelFormatAttribute.Accelerated or reducing MSAA samples to 0 fix the issue on the affected iMac?",
      "Is NSOpenGLPixelFormat null after Initialize() on the affected device?",
      "Does the issue reproduce with current SkiaSharp versions (2.x/3.x) on the same hardware?"
    ],
    "workarounds": [
      "Use SKCanvasView instead of SKGLView on macOS — CPU rendering path works correctly on the same hardware",
      "If OpenGL is required, try setting a less demanding pixel format by removing Multisample/Accelerated attributes (requires source modification)"
    ],
    "resolution": {
      "hypothesis": "The NSOpenGLPixelFormat created in Initialize() requires hardware acceleration and 4x MSAA which older GPUs on High Sierra cannot provide. If pixel format creation silently fails (returns nil), NSOpenGLView renders nothing. Adding a fallback pixel format without MSAA would fix the issue.",
      "proposals": [
        {
          "title": "Add fallback pixel format without MSAA",
          "description": "If the primary pixel format (with Multisample/4 samples) fails to initialize, fall back to a simpler pixel format without multisampling. Check if PixelFormat is null/invalid after the initial assignment.",
          "category": "fix",
          "confidence": 0.75,
          "effort": "cost/s",
          "validated": "untested"
        },
        {
          "title": "Use SKCanvasView as workaround",
          "description": "For users on older Mac hardware, SKCanvasView provides identical drawing API using CPU rasterization and works correctly on all devices.",
          "category": "workaround",
          "confidence": 0.95,
          "effort": "cost/xs",
          "validated": "untested"
        }
      ],
      "recommendedProposal": "Add fallback pixel format without MSAA",
      "recommendedReason": "Fixes the root cause for users who need GPU rendering. The fallback approach is low-risk and follows the pattern of handling optional GL capabilities."
    }
  },
  "output": {
    "actionability": {
      "suggestedAction": "needs-investigation",
      "confidence": 0.78,
      "reason": "Bug is plausible and code investigation confirms a likely root cause (no fallback for hardware that can't satisfy pixel format), but needs reproduction on the affected hardware to confirm. The reporter provided a repro project.",
      "suggestedReproPlatform": "macos"
    },
    "actions": [
      {
        "type": "update-labels",
        "description": "Apply bug, views, macOS, OpenGL labels",
        "risk": "low",
        "confidence": 0.9,
        "labels": [
          "type/bug",
          "area/SkiaSharp.Views",
          "os/macOS",
          "backend/OpenGL"
        ]
      },
      {
        "type": "add-comment",
        "description": "Ask for confirmation on current version and suggest SKCanvasView workaround",
        "risk": "medium",
        "confidence": 0.78,
        "comment": "Thanks for the detailed report and repro project!\n\nThis looks like a hardware compatibility issue with the OpenGL pixel format that `SKGLView` requests on macOS. The `Initialize()` method requires hardware acceleration and 4x multisampling — older GPUs on High Sierra may not satisfy these requirements, causing the OpenGL context to silently fail.\n\n**Workaround:** Use `SKCanvasView` instead of `SKGLView`. The CPU rendering path works correctly on the same hardware.\n\nCould you confirm:\n1. Does this still reproduce with the latest SkiaSharp version?\n2. On the affected iMac, does anything appear in the console/Xcode output when the GL view fails to draw?"
      }
    ]
  }
}
```

</details>
