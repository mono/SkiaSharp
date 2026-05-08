# Issue Triage Report — #243

| Field | Value |
|-------|-------|
| Repository | mono/SkiaSharp |
| Analyzed | 2026-04-24T12:20:00Z |
| Type | type/feature-request (0.95 (95%)) |
| Area | area/Build (0.85 (85%)) |
| Suggested action | keep-open (0.85 (85%)) |

**Issue Summary:** Feature request to include ANGLE (OpenGL ES over Direct3D) in the Windows Classic (win7-*) native runtime packages so that OpenGL-backed SkiaSharp views work on systems without native OpenGL drivers.

**Analysis:** ANGLE is already built and packaged for WinUI (native/winui-angle/build.cake, CI at chromium/6275), but Windows Classic (WPF/WinForms) native builds do not include ANGLE. The WPF SKGLElement and WinForms SKGLControl both use OpenTK rather than ANGLE, leaving Win7 and driver-less systems unable to use GPU-accelerated rendering. A separate NuGet package approach was discussed to avoid DLL conflicts with AMD's libEGL.dll.

**Recommendations:** **keep-open** — Valid feature request acknowledged by maintainer with clear design direction (separate NuGet package), but not yet implemented. Tracking in Backlog is appropriate.

---

## Classification

| Field | Value |
|-------|-------|
| Type | type/feature-request |
| Area | area/Build |
| Platforms | os/Windows-Classic |
| Backends | backend/OpenGL |
| Tenets | tenet/compatibility |
| Partner | — |
| Current labels | os/Windows-Classic, area/Build, backend/OpenGL, type/feature-request, triage/triaged |

## Evidence

### Reproduction

**Environment:** Windows 7 / win7-* runtimes; systems without native OpenGL drivers fall back to software rendering

**Repository links:**
- https://i.imgur.com/DZWkJdg.png — Screenshot showing ANGLE supports DX9-based rendering including WinXP
- https://www.dropbox.com/s/ayi8ill098jxh1d/angle-d3d9.zip?dl=0 — Community-provided ANGLE D3D9 build targeting chromium/2550 with WinXP compatibility
- https://www.nuget.org/packages/Avalonia.Angle.Windows.Natives — Avalonia's existing ANGLE NuGet package for Windows
- https://chromium.googlesource.com/angle/angle/+/76e90947a8e421308dff9d00c4955717c6667f12/extensions/EGL_ANGLE_platform_angle_device_type_swiftshader.txt — ANGLE SwiftShader extension for software fallback

## Analysis

### Technical Summary

ANGLE is already built and packaged for WinUI (native/winui-angle/build.cake, CI at chromium/6275), but Windows Classic (WPF/WinForms) native builds do not include ANGLE. The WPF SKGLElement and WinForms SKGLControl both use OpenTK rather than ANGLE, leaving Win7 and driver-less systems unable to use GPU-accelerated rendering. A separate NuGet package approach was discussed to avoid DLL conflicts with AMD's libEGL.dll.

### Rationale

This is clearly a feature request — ANGLE already exists in the project for WinUI but has not been extended to Windows Classic runtimes. The maintainer acknowledged the request and is tracking design concerns (DLL conflicts, build complexity) as the reason it remains open. The tenet/compatibility label applies since ANGLE is needed for Windows 7 and systems without native OpenGL drivers.

### Key Signals

- "Angle has support even for DX9-based rendering which is preinstalled on Win7. Why isn't it included?" — **issue body** (Reporter wants ANGLE included in Windows Classic native runtime packages for Win7 GL compatibility.)
- "I am actually a bit concerned about this... if the user installs another library that uses it? There may be clashes." — **comment by mattleibow (2020-04-24)** (Maintainer acknowledges the concern — DLL name conflicts with AMD libEGL.dll is the main blocker.)
- "Also, keep in mind that some graphics drivers (e.g. AMD) ship their own libEGL.dll in system32." — **comment by kekekeks (2020-04-25)** (Bundling libEGL.dll/libGLESv2.dll risks overwriting/conflicting with vendor-supplied DLLs.)
- "Packaging ANGLE as a separate package and referencing it from WPF/Winforms views should work." — **comment by kekekeks (2020-04-25)** (Consensus: a dedicated ANGLE NuGet package (opt-in) is safer than bundling with main libSkiaSharp package.)

### Code Investigation

| File | Lines | Relevance | Finding |
|------|-------|-----------|---------|
| `native/winui-angle/build.cake` | — | direct | ANGLE is already built for WinUI (UWP/WinAppSDK) targeting chromium/6275. Build targets x86/x64/arm64 and outputs libEGL.dll and libGLESv2.dll. No equivalent exists for Windows Classic. |
| `source/SkiaSharp.Views/SkiaSharp.Views.WPF/SKGLElement.cs` | 1-30 | direct | SKGLElement (WPF) uses OpenTK (GLWpfControl, OpenTK.Mathematics) — no ANGLE/EGL interop. ANGLE would need a separate integration path. |
| `source/SkiaSharp.Views/SkiaSharp.Views.WindowsForms/SKGLControl.cs` | 1-30 | direct | SKGLControl (WinForms) uses OpenTK/GLControl — no ANGLE/EGL interop. Same situation as WPF. |
| `source/SkiaSharp.Views/SkiaSharp.Views.WinUI/AngleSwapChainPanel.cs` | 1-30 | related | AngleSwapChainPanel (WinUI/UWP) already uses GlesInterop/EGL — ANGLE integration exists for WinUI but not for WPF/WinForms. |
| `scripts/azure-templates-stages-native-windows.yml` | — | related | CI builds ANGLE only for WinUI (3 jobs: x86/x64/arm64). No CI jobs for Windows Classic ANGLE builds. |

### Next Questions

- Would a standalone SkiaSharp.NativeAssets.Windows.Angle NuGet package be the preferred delivery mechanism?
- Is Windows 7 still a target platform given its end-of-life status?
- Would the ANGLE build for Windows Classic use the same chromium/6275 build or require patching for Win7/DX9 support?
- How should DLL naming conflicts with AMD's system libEGL.dll be handled (rename, LoadLibrary by path)?

### Resolution Proposals

**Hypothesis:** ANGLE should be offered as a separate opt-in NuGet package (e.g., SkiaSharp.NativeAssets.Windows.Angle) that ships libEGL.dll/libGLESv2.dll and integrates with WPF/WinForms views, avoiding conflicts with vendor-supplied DLLs.

1. **Separate ANGLE NuGet package for Windows Classic** — fix, confidence 0.80 (80%), cost/l, validated=untested
   - Create a new native NuGet package that bundles ANGLE for win-x86/win-x64/win-arm64, referenced optionally from SkiaSharp.Views.WPF and SkiaSharp.Views.WindowsForms. Users who need Win7 ANGLE support explicitly add the package.
2. **Use existing Avalonia.Angle.Windows.Natives or community builds** — workaround, confidence 0.65 (65%), cost/s, validated=untested
   - As a workaround, users can reference Avalonia's ANGLE package or build ANGLE themselves and place libEGL.dll/libGLESv2.dll alongside their app — SkiaSharp's GLES context setup will pick them up automatically.

**Recommended proposal:** Separate ANGLE NuGet package for Windows Classic

**Why:** Opt-in package avoids DLL conflict risks (AMD libEGL.dll), keeps the main libSkiaSharp package lean, and aligns with the maintainer's stated preference from the 2020 discussion.

## Recommendations

### Actionability

| Field | Value |
|-------|-------|
| Suggested action | keep-open |
| Confidence | 0.85 (85%) |
| Reason | Valid feature request acknowledged by maintainer with clear design direction (separate NuGet package), but not yet implemented. Tracking in Backlog is appropriate. |
| Suggested repro platform | windows |

### Automatable Actions

| Type | Risk | Confidence | Description | Details |
|------|------|------------|-------------|---------|
| update-labels | low | 0.90 (90%) | Apply/confirm feature-request, Build, Windows-Classic, OpenGL, compatibility labels | labels=type/feature-request, area/Build, os/Windows-Classic, backend/OpenGL, tenet/compatibility |
| add-comment | medium | 0.80 (80%) | Summarize triage findings, confirm the separate NuGet approach, and note the DLL conflict caveat | — |

**Comment draft for `add-comment`:**

```markdown
**Triage update:** This is a valid feature request that remains open. ANGLE is already built and packaged for WinUI (`native/winui-angle/build.cake`) but has not been extended to Windows Classic (WPF/WinForms).

The primary design challenge is avoiding DLL name conflicts — some AMD drivers ship their own `libEGL.dll` in System32. The consensus from the 2020 discussion is that a **separate opt-in NuGet package** (e.g., `SkiaSharp.NativeAssets.Windows.Angle`) is the right approach, giving users explicit control over whether ANGLE is deployed.

**Workaround:** Until then, you can manually place `libEGL.dll` and `libGLESv2.dll` (from any ANGLE build compatible with GLES2, such as [Avalonia.Angle.Windows.Natives](https://www.nuget.org/packages/Avalonia.Angle.Windows.Natives)) in your application output directory. SkiaSharp's OpenGL context will load them automatically.

Tracked in Backlog — contributions welcome!
```

<details>
<summary>Raw JSON</summary>

```json
{
  "meta": {
    "schemaVersion": "1.0",
    "number": 243,
    "repo": "mono/SkiaSharp",
    "analyzedAt": "2026-04-24T12:20:00Z",
    "currentLabels": [
      "os/Windows-Classic",
      "area/Build",
      "backend/OpenGL",
      "type/feature-request",
      "triage/triaged"
    ]
  },
  "summary": "Feature request to include ANGLE (OpenGL ES over Direct3D) in the Windows Classic (win7-*) native runtime packages so that OpenGL-backed SkiaSharp views work on systems without native OpenGL drivers.",
  "classification": {
    "type": {
      "value": "type/feature-request",
      "confidence": 0.95
    },
    "area": {
      "value": "area/Build",
      "confidence": 0.85
    },
    "platforms": [
      "os/Windows-Classic"
    ],
    "backends": [
      "backend/OpenGL"
    ],
    "tenets": [
      "tenet/compatibility"
    ]
  },
  "evidence": {
    "reproEvidence": {
      "environmentDetails": "Windows 7 / win7-* runtimes; systems without native OpenGL drivers fall back to software rendering",
      "repoLinks": [
        {
          "url": "https://i.imgur.com/DZWkJdg.png",
          "description": "Screenshot showing ANGLE supports DX9-based rendering including WinXP"
        },
        {
          "url": "https://www.dropbox.com/s/ayi8ill098jxh1d/angle-d3d9.zip?dl=0",
          "description": "Community-provided ANGLE D3D9 build targeting chromium/2550 with WinXP compatibility"
        },
        {
          "url": "https://www.nuget.org/packages/Avalonia.Angle.Windows.Natives",
          "description": "Avalonia's existing ANGLE NuGet package for Windows"
        },
        {
          "url": "https://chromium.googlesource.com/angle/angle/+/76e90947a8e421308dff9d00c4955717c6667f12/extensions/EGL_ANGLE_platform_angle_device_type_swiftshader.txt",
          "description": "ANGLE SwiftShader extension for software fallback"
        }
      ]
    }
  },
  "analysis": {
    "summary": "ANGLE is already built and packaged for WinUI (native/winui-angle/build.cake, CI at chromium/6275), but Windows Classic (WPF/WinForms) native builds do not include ANGLE. The WPF SKGLElement and WinForms SKGLControl both use OpenTK rather than ANGLE, leaving Win7 and driver-less systems unable to use GPU-accelerated rendering. A separate NuGet package approach was discussed to avoid DLL conflicts with AMD's libEGL.dll.",
    "rationale": "This is clearly a feature request — ANGLE already exists in the project for WinUI but has not been extended to Windows Classic runtimes. The maintainer acknowledged the request and is tracking design concerns (DLL conflicts, build complexity) as the reason it remains open. The tenet/compatibility label applies since ANGLE is needed for Windows 7 and systems without native OpenGL drivers.",
    "keySignals": [
      {
        "text": "Angle has support even for DX9-based rendering which is preinstalled on Win7. Why isn't it included?",
        "source": "issue body",
        "interpretation": "Reporter wants ANGLE included in Windows Classic native runtime packages for Win7 GL compatibility."
      },
      {
        "text": "I am actually a bit concerned about this... if the user installs another library that uses it? There may be clashes.",
        "source": "comment by mattleibow (2020-04-24)",
        "interpretation": "Maintainer acknowledges the concern — DLL name conflicts with AMD libEGL.dll is the main blocker."
      },
      {
        "text": "Also, keep in mind that some graphics drivers (e.g. AMD) ship their own libEGL.dll in system32.",
        "source": "comment by kekekeks (2020-04-25)",
        "interpretation": "Bundling libEGL.dll/libGLESv2.dll risks overwriting/conflicting with vendor-supplied DLLs."
      },
      {
        "text": "Packaging ANGLE as a separate package and referencing it from WPF/Winforms views should work.",
        "source": "comment by kekekeks (2020-04-25)",
        "interpretation": "Consensus: a dedicated ANGLE NuGet package (opt-in) is safer than bundling with main libSkiaSharp package."
      }
    ],
    "codeInvestigation": [
      {
        "file": "native/winui-angle/build.cake",
        "finding": "ANGLE is already built for WinUI (UWP/WinAppSDK) targeting chromium/6275. Build targets x86/x64/arm64 and outputs libEGL.dll and libGLESv2.dll. No equivalent exists for Windows Classic.",
        "relevance": "direct"
      },
      {
        "file": "source/SkiaSharp.Views/SkiaSharp.Views.WPF/SKGLElement.cs",
        "lines": "1-30",
        "finding": "SKGLElement (WPF) uses OpenTK (GLWpfControl, OpenTK.Mathematics) — no ANGLE/EGL interop. ANGLE would need a separate integration path.",
        "relevance": "direct"
      },
      {
        "file": "source/SkiaSharp.Views/SkiaSharp.Views.WindowsForms/SKGLControl.cs",
        "lines": "1-30",
        "finding": "SKGLControl (WinForms) uses OpenTK/GLControl — no ANGLE/EGL interop. Same situation as WPF.",
        "relevance": "direct"
      },
      {
        "file": "source/SkiaSharp.Views/SkiaSharp.Views.WinUI/AngleSwapChainPanel.cs",
        "lines": "1-30",
        "finding": "AngleSwapChainPanel (WinUI/UWP) already uses GlesInterop/EGL — ANGLE integration exists for WinUI but not for WPF/WinForms.",
        "relevance": "related"
      },
      {
        "file": "scripts/azure-templates-stages-native-windows.yml",
        "finding": "CI builds ANGLE only for WinUI (3 jobs: x86/x64/arm64). No CI jobs for Windows Classic ANGLE builds.",
        "relevance": "related"
      }
    ],
    "nextQuestions": [
      "Would a standalone SkiaSharp.NativeAssets.Windows.Angle NuGet package be the preferred delivery mechanism?",
      "Is Windows 7 still a target platform given its end-of-life status?",
      "Would the ANGLE build for Windows Classic use the same chromium/6275 build or require patching for Win7/DX9 support?",
      "How should DLL naming conflicts with AMD's system libEGL.dll be handled (rename, LoadLibrary by path)?"
    ],
    "resolution": {
      "hypothesis": "ANGLE should be offered as a separate opt-in NuGet package (e.g., SkiaSharp.NativeAssets.Windows.Angle) that ships libEGL.dll/libGLESv2.dll and integrates with WPF/WinForms views, avoiding conflicts with vendor-supplied DLLs.",
      "proposals": [
        {
          "title": "Separate ANGLE NuGet package for Windows Classic",
          "description": "Create a new native NuGet package that bundles ANGLE for win-x86/win-x64/win-arm64, referenced optionally from SkiaSharp.Views.WPF and SkiaSharp.Views.WindowsForms. Users who need Win7 ANGLE support explicitly add the package.",
          "category": "fix",
          "confidence": 0.8,
          "effort": "cost/l",
          "validated": "untested"
        },
        {
          "title": "Use existing Avalonia.Angle.Windows.Natives or community builds",
          "description": "As a workaround, users can reference Avalonia's ANGLE package or build ANGLE themselves and place libEGL.dll/libGLESv2.dll alongside their app — SkiaSharp's GLES context setup will pick them up automatically.",
          "category": "workaround",
          "confidence": 0.65,
          "effort": "cost/s",
          "validated": "untested"
        }
      ],
      "recommendedProposal": "Separate ANGLE NuGet package for Windows Classic",
      "recommendedReason": "Opt-in package avoids DLL conflict risks (AMD libEGL.dll), keeps the main libSkiaSharp package lean, and aligns with the maintainer's stated preference from the 2020 discussion."
    }
  },
  "output": {
    "actionability": {
      "suggestedAction": "keep-open",
      "confidence": 0.85,
      "reason": "Valid feature request acknowledged by maintainer with clear design direction (separate NuGet package), but not yet implemented. Tracking in Backlog is appropriate.",
      "suggestedReproPlatform": "windows"
    },
    "actions": [
      {
        "type": "update-labels",
        "description": "Apply/confirm feature-request, Build, Windows-Classic, OpenGL, compatibility labels",
        "risk": "low",
        "confidence": 0.9,
        "labels": [
          "type/feature-request",
          "area/Build",
          "os/Windows-Classic",
          "backend/OpenGL",
          "tenet/compatibility"
        ]
      },
      {
        "type": "add-comment",
        "description": "Summarize triage findings, confirm the separate NuGet approach, and note the DLL conflict caveat",
        "risk": "medium",
        "confidence": 0.8,
        "comment": "**Triage update:** This is a valid feature request that remains open. ANGLE is already built and packaged for WinUI (`native/winui-angle/build.cake`) but has not been extended to Windows Classic (WPF/WinForms).\n\nThe primary design challenge is avoiding DLL name conflicts — some AMD drivers ship their own `libEGL.dll` in System32. The consensus from the 2020 discussion is that a **separate opt-in NuGet package** (e.g., `SkiaSharp.NativeAssets.Windows.Angle`) is the right approach, giving users explicit control over whether ANGLE is deployed.\n\n**Workaround:** Until then, you can manually place `libEGL.dll` and `libGLESv2.dll` (from any ANGLE build compatible with GLES2, such as [Avalonia.Angle.Windows.Natives](https://www.nuget.org/packages/Avalonia.Angle.Windows.Natives)) in your application output directory. SkiaSharp's OpenGL context will load them automatically.\n\nTracked in Backlog — contributions welcome!"
      }
    ]
  }
}
```

</details>
