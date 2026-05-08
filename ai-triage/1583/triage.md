# Issue Triage Report — #1583

| Field | Value |
|-------|-------|
| Repository | mono/SkiaSharp |
| Analyzed | 2026-05-03T07:30:00Z |
| Type | type/bug (0.90 (90%)) |
| Area | area/SkiaSharp.Views (0.95 (95%)) |
| Suggested action | needs-investigation (0.80 (80%)) |

**Issue Summary:** Xamarin.Mac 'full' (classic Mono) projects fail to build with SkiaSharp.Views due to 'ld: framework not found OpenGLES', because the NuGet fallback resolves to a target that links the iOS-only OpenGLES framework instead of the macOS OpenGL framework.

**Analysis:** SkiaSharp.Views does not include a dedicated lib/ folder entry for the Xamarin.Mac classic (full) TFM. NuGet falls back to a target that was compiled with iOS-specific symbols, causing the linker to try to resolve OpenGLES.framework which does not exist on macOS. The Gles.cs interop file correctly uses '#if __MACOS__' to reference OpenGL.framework vs OpenGLES.framework, but this conditional is only effective when the build defines '__MACOS__'; the fallback TFM does not set that symbol, so the iOS path is compiled in instead.

**Recommendations:** **needs-investigation** — The root cause is reasonably clear (missing Xamarin.Mac classic TFM in NuGet package), but confirming whether Xamarin.Mac full is still an intended target and what fix is needed requires maintainer input.

---

## Classification

| Field | Value |
|-------|-------|
| Type | type/bug |
| Area | area/SkiaSharp.Views |
| Platforms | os/macOS |
| Backends | backend/OpenGL |
| Tenets | tenet/compatibility |
| Partner | — |

## Evidence

### Reproduction

### Bug Signals

| Field | Value |
|-------|-------|
| Severity | medium |
| Regression claimed | False |
| Error type | build-error |
| Error message | ld: framework not found OpenGLES |
| Repro quality | complete |
| Target frameworks | Xamarin.Mac,Version=v4.7.2 |

### Version Analysis

| Field | Value |
|-------|-------|
| Mentioned versions | — |
| Worked in | — |
| Broke in | — |
| Current relevance | likely |
| Relevance reason | The SkiaSharp.Views package still ships; if it lacks a dedicated Xamarin.Mac classic TFM the NuGet fallback issue persists. |

## Analysis

### Technical Summary

SkiaSharp.Views does not include a dedicated lib/ folder entry for the Xamarin.Mac classic (full) TFM. NuGet falls back to a target that was compiled with iOS-specific symbols, causing the linker to try to resolve OpenGLES.framework which does not exist on macOS. The Gles.cs interop file correctly uses '#if __MACOS__' to reference OpenGL.framework vs OpenGLES.framework, but this conditional is only effective when the build defines '__MACOS__'; the fallback TFM does not set that symbol, so the iOS path is compiled in instead.

### Rationale

This is a genuine build-time bug: SkiaSharp.Views does not ship a dedicated Xamarin.Mac classic TFM, causing NuGet to fall back to a lib entry that is compiled without __MACOS__ defined, so the OpenGLES iOS path is linked instead of OpenGL on macOS. Classified as medium severity because it is a complete build blocker for Xamarin.Mac full projects but the workaround is to use the modern net-macos TFM. Confidence is 0.90 because the root cause is clear from code inspection but no classic NuGet layout file was found to confirm the exact fallback.

### Key Signals

- "ld: framework not found OpenGLES" — **issue body - build log** (The linker cannot find OpenGLES.framework on macOS, which is an iOS-only framework. Indicates wrong platform target compiled into the package.)
- "change target framework to 'Xamarin.Mac full, .Net Framework 4.7.2'" — **issue body - reproduction steps** (Reporter is using the classic Xamarin.Mac profile which uses a different TFM than the modern net8.0-macos TFM that SkiaSharp.Views targets.)

### Code Investigation

| File | Lines | Relevance | Finding |
|------|-------|-----------|---------|
| `source/SkiaSharp.Views/SkiaSharp.Views.Shared/GlesInterop/Gles.cs` | 14-20 | direct | libGLESv2 constant is '#if __MACOS__' -> '/System/Library/Frameworks/OpenGL.framework/OpenGL' else for iOS/tvOS -> '/System/Library/Frameworks/OpenGLES.framework/OpenGLES'. If __MACOS__ is not defined, the iOS OpenGLES path is used. |
| `source/SkiaSharp.Views/SkiaSharp.Views/SkiaSharp.Views.csproj` | — | direct | TargetFrameworks uses PlatformTargetFrameworks / FullFrameworkTargetFrameworks / WindowsDesktopTargetFrameworks variables. The macOS TFM entry is '-macos' (modern .NET), not the Xamarin.Mac classic TFM (Xamarin.Mac,Version=v4.7.2). No explicit Xamarin.Mac classic TFM is listed. |
| `source/SkiaSharp.Views/SkiaSharp.Views/Platform/macOS/SKGLView.cs` | — | related | SKGLView on macOS extends NSOpenGLView (AppKit); imports 'SkiaSharp.Views.GlesInterop'. Compiled only when '-macos' TFM is active. Under Xamarin.Mac full, this file is not compiled and the wrong GlesInterop symbols apply. |

### Workarounds

- Migrate the Xamarin.Mac project from 'Xamarin.Mac full' / classic profile to the modern net8.0-macos TFM, which is fully supported by SkiaSharp.Views.

### Next Questions

- Which version of SkiaSharp.Views was used when the issue was reported?
- Is the Xamarin.Mac classic profile (Xamarin.Mac,Version=v4.7.2) still a supported target for SkiaSharp.Views in the current release?

### Resolution Proposals

**Hypothesis:** SkiaSharp.Views lacks a dedicated Xamarin.Mac classic TFM entry. Fix would add a XamarinMac20 target to the NuGet package with __MACOS__ defined, or update the shared GlesInterop fallback to not reference OpenGLES on platforms that are not iOS/tvOS.

1. **Add dedicated Xamarin.Mac classic TFM to SkiaSharp.Views NuGet** — fix, cost/m, validated=untested
   - Add 'XamarinMac20' to TargetFrameworks in SkiaSharp.Views.csproj and ensure __MACOS__ is defined, so the correct OpenGL (not OpenGLES) path is used.
2. **Migrate to modern .NET macOS TFM** — workaround, cost/m, validated=untested
   - As a workaround for users: Migrate the project from Xamarin.Mac full / classic to net8.0-macos. The modern macOS TFM is already supported by SkiaSharp.Views.

**Recommended proposal:** p1

**Why:** Fixing the NuGet package to include a Xamarin.Mac classic TFM is the proper solution. Migration to modern .NET is a valid long-term path but is a large change for existing projects.

## Recommendations

### Actionability

| Field | Value |
|-------|-------|
| Suggested action | needs-investigation |
| Confidence | 0.80 (80%) |
| Reason | The root cause is reasonably clear (missing Xamarin.Mac classic TFM in NuGet package), but confirming whether Xamarin.Mac full is still an intended target and what fix is needed requires maintainer input. |
| Suggested repro platform | macos |

### Automatable Actions

| Type | Risk | Confidence | Description | Details |
|------|------|------------|-------------|---------|
| update-labels | low | 0.95 (95%) | Apply type/bug, area/SkiaSharp.Views, os/macOS, backend/OpenGL, tenet/compatibility | labels=type/bug, area/SkiaSharp.Views, os/macOS, backend/OpenGL, tenet/compatibility |
| add-comment | medium | 0.80 (80%) | Acknowledge the build error, explain the root cause (missing Xamarin.Mac classic TFM), and suggest migration to net8.0-macos as a workaround. | — |

**Comment draft for `add-comment`:**

```markdown
Thank you for the detailed reproduction steps!

The error `ld: framework not found OpenGLES` occurs because **SkiaSharp.Views** does not include a dedicated NuGet target for the Xamarin.Mac **classic/full** profile (`Xamarin.Mac,Version=v4.7.2`). When NuGet resolves the package it falls back to a compiled variant that references the iOS-only `OpenGLES.framework`, which is not available on macOS.

**Workaround:** Migrate your project to the modern `net8.0-macos` target framework. SkiaSharp.Views fully supports `net8.0-macos` and correctly links against `OpenGL.framework`.

We'll look into adding an explicit Xamarin.Mac classic TFM to the NuGet package.
```

<details>
<summary>Raw JSON</summary>

```json
{
  "meta": {
    "schemaVersion": "1.0",
    "number": 1583,
    "repo": "mono/SkiaSharp",
    "analyzedAt": "2026-05-03T07:30:00Z"
  },
  "summary": "Xamarin.Mac 'full' (classic Mono) projects fail to build with SkiaSharp.Views due to 'ld: framework not found OpenGLES', because the NuGet fallback resolves to a target that links the iOS-only OpenGLES framework instead of the macOS OpenGL framework.",
  "classification": {
    "type": {
      "value": "type/bug",
      "confidence": 0.9
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
      "tenet/compatibility"
    ]
  },
  "evidence": {
    "bugSignals": {
      "severity": "medium",
      "regressionClaimed": false,
      "errorType": "build-error",
      "errorMessage": "ld: framework not found OpenGLES",
      "reproQuality": "complete",
      "targetFrameworks": [
        "Xamarin.Mac,Version=v4.7.2"
      ]
    },
    "reproEvidence": {
      "repoLinks": []
    },
    "versionAnalysis": {
      "mentionedVersions": [],
      "currentRelevance": "likely",
      "relevanceReason": "The SkiaSharp.Views package still ships; if it lacks a dedicated Xamarin.Mac classic TFM the NuGet fallback issue persists."
    }
  },
  "analysis": {
    "summary": "SkiaSharp.Views does not include a dedicated lib/ folder entry for the Xamarin.Mac classic (full) TFM. NuGet falls back to a target that was compiled with iOS-specific symbols, causing the linker to try to resolve OpenGLES.framework which does not exist on macOS. The Gles.cs interop file correctly uses '#if __MACOS__' to reference OpenGL.framework vs OpenGLES.framework, but this conditional is only effective when the build defines '__MACOS__'; the fallback TFM does not set that symbol, so the iOS path is compiled in instead.",
    "codeInvestigation": [
      {
        "file": "source/SkiaSharp.Views/SkiaSharp.Views.Shared/GlesInterop/Gles.cs",
        "finding": "libGLESv2 constant is '#if __MACOS__' -> '/System/Library/Frameworks/OpenGL.framework/OpenGL' else for iOS/tvOS -> '/System/Library/Frameworks/OpenGLES.framework/OpenGLES'. If __MACOS__ is not defined, the iOS OpenGLES path is used.",
        "relevance": "direct",
        "lines": "14-20"
      },
      {
        "file": "source/SkiaSharp.Views/SkiaSharp.Views/SkiaSharp.Views.csproj",
        "finding": "TargetFrameworks uses PlatformTargetFrameworks / FullFrameworkTargetFrameworks / WindowsDesktopTargetFrameworks variables. The macOS TFM entry is '-macos' (modern .NET), not the Xamarin.Mac classic TFM (Xamarin.Mac,Version=v4.7.2). No explicit Xamarin.Mac classic TFM is listed.",
        "relevance": "direct"
      },
      {
        "file": "source/SkiaSharp.Views/SkiaSharp.Views/Platform/macOS/SKGLView.cs",
        "finding": "SKGLView on macOS extends NSOpenGLView (AppKit); imports 'SkiaSharp.Views.GlesInterop'. Compiled only when '-macos' TFM is active. Under Xamarin.Mac full, this file is not compiled and the wrong GlesInterop symbols apply.",
        "relevance": "related"
      }
    ],
    "keySignals": [
      {
        "text": "ld: framework not found OpenGLES",
        "source": "issue body - build log",
        "interpretation": "The linker cannot find OpenGLES.framework on macOS, which is an iOS-only framework. Indicates wrong platform target compiled into the package."
      },
      {
        "text": "change target framework to 'Xamarin.Mac full, .Net Framework 4.7.2'",
        "source": "issue body - reproduction steps",
        "interpretation": "Reporter is using the classic Xamarin.Mac profile which uses a different TFM than the modern net8.0-macos TFM that SkiaSharp.Views targets."
      }
    ],
    "rationale": "This is a genuine build-time bug: SkiaSharp.Views does not ship a dedicated Xamarin.Mac classic TFM, causing NuGet to fall back to a lib entry that is compiled without __MACOS__ defined, so the OpenGLES iOS path is linked instead of OpenGL on macOS. Classified as medium severity because it is a complete build blocker for Xamarin.Mac full projects but the workaround is to use the modern net-macos TFM. Confidence is 0.90 because the root cause is clear from code inspection but no classic NuGet layout file was found to confirm the exact fallback.",
    "resolution": {
      "hypothesis": "SkiaSharp.Views lacks a dedicated Xamarin.Mac classic TFM entry. Fix would add a XamarinMac20 target to the NuGet package with __MACOS__ defined, or update the shared GlesInterop fallback to not reference OpenGLES on platforms that are not iOS/tvOS.",
      "proposals": [
        {
          "title": "Add dedicated Xamarin.Mac classic TFM to SkiaSharp.Views NuGet",
          "description": "Add 'XamarinMac20' to TargetFrameworks in SkiaSharp.Views.csproj and ensure __MACOS__ is defined, so the correct OpenGL (not OpenGLES) path is used.",
          "category": "fix",
          "effort": "cost/m",
          "validated": "untested"
        },
        {
          "title": "Migrate to modern .NET macOS TFM",
          "description": "As a workaround for users: Migrate the project from Xamarin.Mac full / classic to net8.0-macos. The modern macOS TFM is already supported by SkiaSharp.Views.",
          "category": "workaround",
          "effort": "cost/m",
          "validated": "untested"
        }
      ],
      "recommendedProposal": "p1",
      "recommendedReason": "Fixing the NuGet package to include a Xamarin.Mac classic TFM is the proper solution. Migration to modern .NET is a valid long-term path but is a large change for existing projects."
    },
    "workarounds": [
      "Migrate the Xamarin.Mac project from 'Xamarin.Mac full' / classic profile to the modern net8.0-macos TFM, which is fully supported by SkiaSharp.Views."
    ],
    "nextQuestions": [
      "Which version of SkiaSharp.Views was used when the issue was reported?",
      "Is the Xamarin.Mac classic profile (Xamarin.Mac,Version=v4.7.2) still a supported target for SkiaSharp.Views in the current release?"
    ]
  },
  "output": {
    "actionability": {
      "suggestedAction": "needs-investigation",
      "confidence": 0.8,
      "reason": "The root cause is reasonably clear (missing Xamarin.Mac classic TFM in NuGet package), but confirming whether Xamarin.Mac full is still an intended target and what fix is needed requires maintainer input.",
      "suggestedReproPlatform": "macos"
    },
    "actions": [
      {
        "type": "update-labels",
        "description": "Apply type/bug, area/SkiaSharp.Views, os/macOS, backend/OpenGL, tenet/compatibility",
        "labels": [
          "type/bug",
          "area/SkiaSharp.Views",
          "os/macOS",
          "backend/OpenGL",
          "tenet/compatibility"
        ],
        "risk": "low",
        "confidence": 0.95
      },
      {
        "type": "add-comment",
        "description": "Acknowledge the build error, explain the root cause (missing Xamarin.Mac classic TFM), and suggest migration to net8.0-macos as a workaround.",
        "comment": "Thank you for the detailed reproduction steps!\n\nThe error `ld: framework not found OpenGLES` occurs because **SkiaSharp.Views** does not include a dedicated NuGet target for the Xamarin.Mac **classic/full** profile (`Xamarin.Mac,Version=v4.7.2`). When NuGet resolves the package it falls back to a compiled variant that references the iOS-only `OpenGLES.framework`, which is not available on macOS.\n\n**Workaround:** Migrate your project to the modern `net8.0-macos` target framework. SkiaSharp.Views fully supports `net8.0-macos` and correctly links against `OpenGL.framework`.\n\nWe'll look into adding an explicit Xamarin.Mac classic TFM to the NuGet package.",
        "risk": "medium",
        "confidence": 0.8
      }
    ]
  }
}
```

</details>
