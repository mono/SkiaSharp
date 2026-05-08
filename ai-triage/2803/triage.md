# Issue Triage Report — #2803

| Field | Value |
|-------|-------|
| Repository | mono/SkiaSharp |
| Analyzed | 2026-04-21T18:51:36Z |
| Type | type/bug (0.88 (88%)) |
| Area | area/SkiaSharp.Views (0.85 (85%)) |
| Suggested action | needs-investigation (0.78 (78%)) |

**Issue Summary:** AccessViolationException when using SKSwapChainPanel in WinUI3 with SkiaSharp 3.0.0-preview, caused by externally-installed ANGLE conflicting with bundled libraries or by building with AnyCPU instead of x64 platform target.

**Analysis:** AccessViolationException when initializing SKSwapChainPanel in WinUI3. Root causes identified: (1) user separately installed ANGLE via vcpkg which conflicts with ANGLE bundled in the NuGet package; (2) building with AnyCPU target instead of x64, causing native architecture mismatch; (3) SKSwapChainPanel is not supported on Uno Desktop (Windows) in early previews — SKXamlCanvas should be used instead.

**Recommendations:** **needs-investigation** — Multiple root causes identified with working workarounds, but the underlying AnyCPU architecture mismatch may be fixable via MSBuild enforcement. Needs investigation to determine if this is fixed in later 3.x releases.

---

## Classification

| Field | Value |
|-------|-------|
| Type | type/bug |
| Area | area/SkiaSharp.Views |
| Platforms | os/Windows-WinUI |
| Backends | backend/OpenGL |
| Tenets | — |
| Partner | partner/unoplatform |
| Current labels | type/bug, status/needs-attention |

## Evidence

### Reproduction

1. Create a WinUI3 app targeting Windows 10
2. Add SkiaSharp.Views.WinUI 3.0.0-preview NuGet package
3. Add SKSwapChainPanel control to XAML
4. Build with AnyCPU or after separately installing ANGLE via vcpkg
5. Run the app — AccessViolationException is thrown immediately

**Environment:** Windows 10, Visual Studio, SkiaSharp.Views.WinUI 3.0.0-preview 2.1

**Repository links:**
- https://github.com/mono/SkiaSharp/issues/2803#issuecomment-2058798641 — Reporter-provided working WinUI3 sample zip (WinUI package, no external ANGLE)
- https://github.com/mono/SkiaSharp/issues/2803#issuecomment-2855801131 — Community workaround: switch from AnyCPU to x64 configuration

### Bug Signals

| Field | Value |
|-------|-------|
| Severity | medium |
| Regression claimed | True |
| Error type | exception |
| Error message | System.AccessViolationException: Attempted to read or write protected memory |
| Repro quality | partial |
| Target frameworks | net8.0-windows10.0 |

### Version Analysis

| Field | Value |
|-------|-------|
| Mentioned versions | 3.0.0-preview.2.1, 2.88.2 |
| Worked in | 2.88.2 |
| Broke in | 3.0.0-preview.2.1 |
| Current relevance | likely |
| Relevance reason | SKSwapChainPanel is a new v3 feature (not present in 2.88.x); version comparison means the feature itself is new rather than a regression in existing functionality. |

## Analysis

### Technical Summary

AccessViolationException when initializing SKSwapChainPanel in WinUI3. Root causes identified: (1) user separately installed ANGLE via vcpkg which conflicts with ANGLE bundled in the NuGet package; (2) building with AnyCPU target instead of x64, causing native architecture mismatch; (3) SKSwapChainPanel is not supported on Uno Desktop (Windows) in early previews — SKXamlCanvas should be used instead.

### Rationale

This is a real crash bug with multiple root causes: (1) user error — installing ANGLE via vcpkg conflicts with bundled ANGLE; (2) configuration error — building as AnyCPU instead of x64; (3) platform limitation — SKSwapChainPanel is not supported for Uno Desktop. The fix is a combination of documentation/guidance improvements and potentially enforcing platform target validation. The issue is medium severity because clear workarounds exist.

### Key Signals

- "I have also build and install angle with vcpkg. Without any luck." — **issue body** (User installed ANGLE separately via vcpkg — this conflicts with bundled ANGLE in the NuGet package and is the likely root cause for the original reporter.)
- "You should not install any other ANGLE library - it is included in the package." — **comment by mattleibow** (Maintainer confirmed that ANGLE must NOT be installed externally. The package bundles the correct ANGLE binaries.)
- "I was able to make it running. Dont install Uno package. Install WinUI Package." — **comment by AmitParmar2005** (Original reporter resolved their issue by using SkiaSharp.Views.WinUI instead of the Uno package, suggesting a packaging/dependency conflict.)
- "SKSwapChainPanel is supported on iOS, Android and WebAssembly. Catalyst does not have the right setup yet (Metal should be used) and Desktop target must SKXamlCanvas for now." — **comment by jeromelaban (Uno contributor)** (SKSwapChainPanel is explicitly not supported for Uno Desktop (Windows) in current previews.)
- "switching from the AnyCPU to an x64 configuration for the WinUI build of the Uno App fixed the issue" — **comment by darenm** (AnyCPU configuration causes native architecture mismatch. Native ANGLE and Skia binaries are built for x64 — using AnyCPU can result in process running as x86, causing AccessViolationException on first native call.)

### Code Investigation

| File | Lines | Relevance | Finding |
|------|-------|-----------|---------|
| `source/SkiaSharp.Views/SkiaSharp.Views.WinUI/AngleSwapChainPanel.cs` | 124-135 | direct | OnLoaded creates GlesContext and calls CreateSurface(this, null, CompositionScaleX) — this is where ANGLE native initialization occurs. If a conflicting ANGLE DLL from vcpkg is present in PATH or application folder, the wrong binary is loaded, leading to memory layout mismatch and AccessViolationException. |
| `source/SkiaSharp.Views/SkiaSharp.Views.WinUI/AngleSwapChainPanel.cs` | 214-238 | direct | RenderFrame calls glesContext.MakeCurrent() then native GL calls via ANGLE. Architecture mismatch (AnyCPU resolving to x86 while native ANGLE is x64) would cause access violation at first native interop call. |
| `source/SkiaSharp.Views/SkiaSharp.Views.WinUI/SKSwapChainPanel.cs` | 41-98 | direct | SKSwapChainPanel.OnRenderFrame creates GRGlInterface and GRContext from ANGLE-backed OpenGL context. A null GRGlInterface (returned when ANGLE is not properly initialized) passed to GRContext.CreateGl causes access violation in native Skia code. |
| `source/SkiaSharp.Views.Uno/SkiaSharp.Views.Uno.WinUI/SKSwapChainPanel.Reference.cs` | 16-35 | related | Uno reference API stub throws NotImplementedException for all SKSwapChainPanel members — SKSwapChainPanel is explicitly unsupported on Uno reference platform (Desktop). |

### Workarounds

- Do NOT install ANGLE via vcpkg or any external package manager — ANGLE is bundled in SkiaSharp.Views.WinUI NuGet package.
- Set the WinUI project's platform target to x64 (not AnyCPU) in Visual Studio project properties.
- For Uno Desktop (Windows) apps, use SKXamlCanvas instead of SKSwapChainPanel — SKSwapChainPanel is not supported on Uno Desktop in current versions.
- For Uno apps on other platforms (iOS, Android, WASM), SKSwapChainPanel is supported normally.

### Next Questions

- Is the AnyCPU→x64 issue a WinUI3 packaging requirement or could it be fixed by enforcing x64 in the SkiaSharp.Views.WinUI package MSBuild targets?
- Has the SKSwapChainPanel Uno Desktop support been added in more recent SkiaSharp 3.x releases?

### Resolution Proposals

**Hypothesis:** Two separate issues: (1) external ANGLE installation causes DLL conflict/memory corruption; (2) AnyCPU builds load 64-bit native libs into a potentially 32-bit process. Both can be addressed via documentation and MSBuild enforcement.

1. **Document ANGLE bundling and x64 requirement** — workaround, confidence 0.90 (90%), cost/xs, validated=untested
   - Add clear warning in README and NuGet package description: do not install ANGLE separately; always set platform target to x64 (not AnyCPU) for WinUI3 apps using SKSwapChainPanel.
2. **Add MSBuild target to enforce x64 platform for WinUI** — fix, confidence 0.75 (75%), cost/s, validated=untested
   - Add a build warning or error in SkiaSharp.Views.WinUI.targets when PlatformTarget is AnyCPU on Windows, since native ANGLE binaries require x64 or arm64.
3. **Use SKXamlCanvas instead of SKSwapChainPanel for Uno Desktop** — alternative, confidence 0.95 (95%), cost/xs, validated=untested
   - For Uno Platform Desktop (Windows) targets, replace SKSwapChainPanel with SKXamlCanvas which uses software rendering and is fully supported.

**Recommended proposal:** Use SKXamlCanvas instead of SKSwapChainPanel for Uno Desktop

**Why:** Immediate, actionable workaround for users today. The Uno Desktop SKSwapChainPanel limitation is acknowledged by the Uno team contributor.

## Recommendations

### Actionability

| Field | Value |
|-------|-------|
| Suggested action | needs-investigation |
| Confidence | 0.78 (78%) |
| Reason | Multiple root causes identified with working workarounds, but the underlying AnyCPU architecture mismatch may be fixable via MSBuild enforcement. Needs investigation to determine if this is fixed in later 3.x releases. |
| Suggested repro platform | windows |

### Automatable Actions

| Type | Risk | Confidence | Description | Details |
|------|------|------------|-------------|---------|
| update-labels | low | 0.90 (90%) | Apply bug, WinUI views, Windows WinUI platform, OpenGL backend labels | labels=type/bug, area/SkiaSharp.Views, os/Windows-WinUI, backend/OpenGL |
| add-comment | medium | 0.85 (85%) | Post analysis with known workarounds | — |

**Comment draft for `add-comment`:**

```markdown
Thanks for reporting this! Based on the discussion and community findings, there are a few known causes for this `AccessViolationException`:

**Root causes identified:**
1. **External ANGLE installation** — ANGLE is bundled inside the `SkiaSharp.Views.WinUI` NuGet package. Installing ANGLE separately via vcpkg or another tool causes DLL conflicts. Remove any externally-installed ANGLE.
2. **AnyCPU platform target** — The native ANGLE and Skia libraries are built for x64/arm64. If your WinUI project targets **AnyCPU**, it may run as a 32-bit process and fail to load the 64-bit native binaries. **Set your project's Platform Target to x64** in Visual Studio (Project Properties → Build → Platform target).
3. **Uno Desktop limitation** — `SKSwapChainPanel` is not supported on Uno Desktop (Windows) in current releases. For Uno Desktop apps, use **`SKXamlCanvas`** instead.

**Quick fix for Uno users:** Switch from AnyCPU to x64 in your WinUI build configuration (credit: @darenm).

Let us know if any of these resolve your issue!
```

<details>
<summary>Raw JSON</summary>

```json
{
  "meta": {
    "schemaVersion": "1.0",
    "number": 2803,
    "repo": "mono/SkiaSharp",
    "analyzedAt": "2026-04-21T18:51:36Z",
    "currentLabels": [
      "type/bug",
      "status/needs-attention"
    ]
  },
  "summary": "AccessViolationException when using SKSwapChainPanel in WinUI3 with SkiaSharp 3.0.0-preview, caused by externally-installed ANGLE conflicting with bundled libraries or by building with AnyCPU instead of x64 platform target.",
  "classification": {
    "type": {
      "value": "type/bug",
      "confidence": 0.88
    },
    "area": {
      "value": "area/SkiaSharp.Views",
      "confidence": 0.85
    },
    "platforms": [
      "os/Windows-WinUI"
    ],
    "backends": [
      "backend/OpenGL"
    ],
    "partner": "partner/unoplatform"
  },
  "evidence": {
    "bugSignals": {
      "severity": "medium",
      "regressionClaimed": true,
      "errorType": "exception",
      "errorMessage": "System.AccessViolationException: Attempted to read or write protected memory",
      "reproQuality": "partial",
      "targetFrameworks": [
        "net8.0-windows10.0"
      ]
    },
    "reproEvidence": {
      "stepsToReproduce": [
        "Create a WinUI3 app targeting Windows 10",
        "Add SkiaSharp.Views.WinUI 3.0.0-preview NuGet package",
        "Add SKSwapChainPanel control to XAML",
        "Build with AnyCPU or after separately installing ANGLE via vcpkg",
        "Run the app — AccessViolationException is thrown immediately"
      ],
      "environmentDetails": "Windows 10, Visual Studio, SkiaSharp.Views.WinUI 3.0.0-preview 2.1",
      "repoLinks": [
        {
          "url": "https://github.com/mono/SkiaSharp/issues/2803#issuecomment-2058798641",
          "description": "Reporter-provided working WinUI3 sample zip (WinUI package, no external ANGLE)"
        },
        {
          "url": "https://github.com/mono/SkiaSharp/issues/2803#issuecomment-2855801131",
          "description": "Community workaround: switch from AnyCPU to x64 configuration"
        }
      ]
    },
    "versionAnalysis": {
      "mentionedVersions": [
        "3.0.0-preview.2.1",
        "2.88.2"
      ],
      "workedIn": "2.88.2",
      "brokeIn": "3.0.0-preview.2.1",
      "currentRelevance": "likely",
      "relevanceReason": "SKSwapChainPanel is a new v3 feature (not present in 2.88.x); version comparison means the feature itself is new rather than a regression in existing functionality."
    }
  },
  "analysis": {
    "summary": "AccessViolationException when initializing SKSwapChainPanel in WinUI3. Root causes identified: (1) user separately installed ANGLE via vcpkg which conflicts with ANGLE bundled in the NuGet package; (2) building with AnyCPU target instead of x64, causing native architecture mismatch; (3) SKSwapChainPanel is not supported on Uno Desktop (Windows) in early previews — SKXamlCanvas should be used instead.",
    "codeInvestigation": [
      {
        "file": "source/SkiaSharp.Views/SkiaSharp.Views.WinUI/AngleSwapChainPanel.cs",
        "lines": "124-135",
        "finding": "OnLoaded creates GlesContext and calls CreateSurface(this, null, CompositionScaleX) — this is where ANGLE native initialization occurs. If a conflicting ANGLE DLL from vcpkg is present in PATH or application folder, the wrong binary is loaded, leading to memory layout mismatch and AccessViolationException.",
        "relevance": "direct"
      },
      {
        "file": "source/SkiaSharp.Views/SkiaSharp.Views.WinUI/AngleSwapChainPanel.cs",
        "lines": "214-238",
        "finding": "RenderFrame calls glesContext.MakeCurrent() then native GL calls via ANGLE. Architecture mismatch (AnyCPU resolving to x86 while native ANGLE is x64) would cause access violation at first native interop call.",
        "relevance": "direct"
      },
      {
        "file": "source/SkiaSharp.Views/SkiaSharp.Views.WinUI/SKSwapChainPanel.cs",
        "lines": "41-98",
        "finding": "SKSwapChainPanel.OnRenderFrame creates GRGlInterface and GRContext from ANGLE-backed OpenGL context. A null GRGlInterface (returned when ANGLE is not properly initialized) passed to GRContext.CreateGl causes access violation in native Skia code.",
        "relevance": "direct"
      },
      {
        "file": "source/SkiaSharp.Views.Uno/SkiaSharp.Views.Uno.WinUI/SKSwapChainPanel.Reference.cs",
        "lines": "16-35",
        "finding": "Uno reference API stub throws NotImplementedException for all SKSwapChainPanel members — SKSwapChainPanel is explicitly unsupported on Uno reference platform (Desktop).",
        "relevance": "related"
      }
    ],
    "keySignals": [
      {
        "text": "I have also build and install angle with vcpkg. Without any luck.",
        "source": "issue body",
        "interpretation": "User installed ANGLE separately via vcpkg — this conflicts with bundled ANGLE in the NuGet package and is the likely root cause for the original reporter."
      },
      {
        "text": "You should not install any other ANGLE library - it is included in the package.",
        "source": "comment by mattleibow",
        "interpretation": "Maintainer confirmed that ANGLE must NOT be installed externally. The package bundles the correct ANGLE binaries."
      },
      {
        "text": "I was able to make it running. Dont install Uno package. Install WinUI Package.",
        "source": "comment by AmitParmar2005",
        "interpretation": "Original reporter resolved their issue by using SkiaSharp.Views.WinUI instead of the Uno package, suggesting a packaging/dependency conflict."
      },
      {
        "text": "SKSwapChainPanel is supported on iOS, Android and WebAssembly. Catalyst does not have the right setup yet (Metal should be used) and Desktop target must SKXamlCanvas for now.",
        "source": "comment by jeromelaban (Uno contributor)",
        "interpretation": "SKSwapChainPanel is explicitly not supported for Uno Desktop (Windows) in current previews."
      },
      {
        "text": "switching from the AnyCPU to an x64 configuration for the WinUI build of the Uno App fixed the issue",
        "source": "comment by darenm",
        "interpretation": "AnyCPU configuration causes native architecture mismatch. Native ANGLE and Skia binaries are built for x64 — using AnyCPU can result in process running as x86, causing AccessViolationException on first native call."
      }
    ],
    "rationale": "This is a real crash bug with multiple root causes: (1) user error — installing ANGLE via vcpkg conflicts with bundled ANGLE; (2) configuration error — building as AnyCPU instead of x64; (3) platform limitation — SKSwapChainPanel is not supported for Uno Desktop. The fix is a combination of documentation/guidance improvements and potentially enforcing platform target validation. The issue is medium severity because clear workarounds exist.",
    "workarounds": [
      "Do NOT install ANGLE via vcpkg or any external package manager — ANGLE is bundled in SkiaSharp.Views.WinUI NuGet package.",
      "Set the WinUI project's platform target to x64 (not AnyCPU) in Visual Studio project properties.",
      "For Uno Desktop (Windows) apps, use SKXamlCanvas instead of SKSwapChainPanel — SKSwapChainPanel is not supported on Uno Desktop in current versions.",
      "For Uno apps on other platforms (iOS, Android, WASM), SKSwapChainPanel is supported normally."
    ],
    "nextQuestions": [
      "Is the AnyCPU→x64 issue a WinUI3 packaging requirement or could it be fixed by enforcing x64 in the SkiaSharp.Views.WinUI package MSBuild targets?",
      "Has the SKSwapChainPanel Uno Desktop support been added in more recent SkiaSharp 3.x releases?"
    ],
    "resolution": {
      "hypothesis": "Two separate issues: (1) external ANGLE installation causes DLL conflict/memory corruption; (2) AnyCPU builds load 64-bit native libs into a potentially 32-bit process. Both can be addressed via documentation and MSBuild enforcement.",
      "proposals": [
        {
          "title": "Document ANGLE bundling and x64 requirement",
          "description": "Add clear warning in README and NuGet package description: do not install ANGLE separately; always set platform target to x64 (not AnyCPU) for WinUI3 apps using SKSwapChainPanel.",
          "category": "workaround",
          "confidence": 0.9,
          "effort": "cost/xs",
          "validated": "untested"
        },
        {
          "title": "Add MSBuild target to enforce x64 platform for WinUI",
          "description": "Add a build warning or error in SkiaSharp.Views.WinUI.targets when PlatformTarget is AnyCPU on Windows, since native ANGLE binaries require x64 or arm64.",
          "category": "fix",
          "confidence": 0.75,
          "effort": "cost/s",
          "validated": "untested"
        },
        {
          "title": "Use SKXamlCanvas instead of SKSwapChainPanel for Uno Desktop",
          "description": "For Uno Platform Desktop (Windows) targets, replace SKSwapChainPanel with SKXamlCanvas which uses software rendering and is fully supported.",
          "category": "alternative",
          "confidence": 0.95,
          "effort": "cost/xs",
          "validated": "untested"
        }
      ],
      "recommendedProposal": "Use SKXamlCanvas instead of SKSwapChainPanel for Uno Desktop",
      "recommendedReason": "Immediate, actionable workaround for users today. The Uno Desktop SKSwapChainPanel limitation is acknowledged by the Uno team contributor."
    }
  },
  "output": {
    "actionability": {
      "suggestedAction": "needs-investigation",
      "confidence": 0.78,
      "reason": "Multiple root causes identified with working workarounds, but the underlying AnyCPU architecture mismatch may be fixable via MSBuild enforcement. Needs investigation to determine if this is fixed in later 3.x releases.",
      "suggestedReproPlatform": "windows"
    },
    "actions": [
      {
        "type": "update-labels",
        "description": "Apply bug, WinUI views, Windows WinUI platform, OpenGL backend labels",
        "risk": "low",
        "confidence": 0.9,
        "labels": [
          "type/bug",
          "area/SkiaSharp.Views",
          "os/Windows-WinUI",
          "backend/OpenGL"
        ]
      },
      {
        "type": "add-comment",
        "description": "Post analysis with known workarounds",
        "risk": "medium",
        "confidence": 0.85,
        "comment": "Thanks for reporting this! Based on the discussion and community findings, there are a few known causes for this `AccessViolationException`:\n\n**Root causes identified:**\n1. **External ANGLE installation** — ANGLE is bundled inside the `SkiaSharp.Views.WinUI` NuGet package. Installing ANGLE separately via vcpkg or another tool causes DLL conflicts. Remove any externally-installed ANGLE.\n2. **AnyCPU platform target** — The native ANGLE and Skia libraries are built for x64/arm64. If your WinUI project targets **AnyCPU**, it may run as a 32-bit process and fail to load the 64-bit native binaries. **Set your project's Platform Target to x64** in Visual Studio (Project Properties → Build → Platform target).\n3. **Uno Desktop limitation** — `SKSwapChainPanel` is not supported on Uno Desktop (Windows) in current releases. For Uno Desktop apps, use **`SKXamlCanvas`** instead.\n\n**Quick fix for Uno users:** Switch from AnyCPU to x64 in your WinUI build configuration (credit: @darenm).\n\nLet us know if any of these resolve your issue!"
      }
    ]
  }
}
```

</details>
