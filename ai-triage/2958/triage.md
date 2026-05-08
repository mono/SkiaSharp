# Issue Triage Report — #2958

| Field | Value |
|-------|-------|
| Repository | mono/SkiaSharp |
| Analyzed | 2026-04-23T15:52:49Z |
| Type | type/bug (0.95 (95%)) |
| Area | area/SkiaSharp.Views (0.92 (92%)) |
| Suggested action | needs-investigation (0.88 (88%)) |

**Issue Summary:** SKSwapChainPanel crashes in packaged WinUI 3 apps on .NET 8 with 'Error loading EGL entry points: libGLESv2.dll'; the same app works fine unpackaged or on .NET 7.

**Analysis:** In packaged WinUI 3 apps on .NET 8, the ANGLE native libraries (libEGL.dll / libGLESv2.dll) fail to load when AngleSwapChainPanel initialises the EGL display. .NET 8 tightened native DLL resolution inside packaged MSIX apps, so the DLLs shipped with SkiaSharp are not discoverable through the default DllImport search path unless they reside in the package's application folder. The same code works unpackaged because the DLL search falls back to the process working directory.

**Recommendations:** **needs-investigation** — Clear regression with reproduction boundary identified (packaged .NET 8 only). Root cause is .NET 8 native DLL loading change in MSIX packaged apps. Needs deeper investigation into NativeLibrary resolvers and package manifest requirements for ANGLE DLLs.

---

## Classification

| Field | Value |
|-------|-------|
| Type | type/bug |
| Area | area/SkiaSharp.Views |
| Platforms | os/Windows-WinUI |
| Backends | backend/OpenGL |
| Tenets | tenet/compatibility, tenet/reliability |
| Partner | — |
| Current labels | type/bug |

## Evidence

### Reproduction

1. Create a packaged WinUI 3 app targeting .NET 8
2. Add SkiaSharp 3.0.0-Preview4.1 and SkiaSharp.Views.WinUI 3.0.0-Preview4.1
3. Add SKSwapChainPanel to the main window
4. Run the app as a packaged MSIX

**Environment:** Windows 10, WinUI 3, .NET 8, SkiaSharp 3.0.0-Preview4.1, packaged app (MSIX)

**Repository links:**
- https://github.com/mono/SkiaSharp/issues/2968 — Related: SKGLView crashes WinUI 3 MAUI unpackaged app on .NET 9 Preview — same ANGLE/EGL loading root cause, now closed
- https://github.com/dotnet/maui/issues/23737 — External: dotnet/maui issue referenced by community member as similar EGL loading failure in unpackaged apps

**Attachments:**
- WinUI_SKLmnsCanvas_Net8.zip — https://github.com/user-attachments/files/16426606/WinUI_SKLmnsCanvas_Net8.zip

### Bug Signals

| Field | Value |
|-------|-------|
| Severity | high |
| Regression claimed | True |
| Error type | crash |
| Error message | Error loading EGL entry points: libGLESv2.dll |
| Repro quality | partial |
| Target frameworks | net8.0-windows10.0.19041.0 |

### Version Analysis

| Field | Value |
|-------|-------|
| Mentioned versions | 3.0.0-preview4.1, 2.88.2 |
| Worked in | 2.88.2 / .NET 7 |
| Broke in | .NET 8 (packaged) |
| Current relevance | likely |
| Relevance reason | The AngleSwapChainPanel/GlesContext code path has not materially changed since this report; packaged-app native DLL loading is a known regression area. |

### Regression

| Field | Value |
|-------|-------|
| Is regression | True |
| Confidence | 0.85 (85%) |
| Reason | Reporter confirms it works on .NET 7 packaged and .NET 8 unpackaged; only .NET 8 packaged fails. .NET 8 changed native DLL search paths in packaged MSIX apps. |
| Worked in version | .NET 7 (packaged) |
| Broke in version | .NET 8 (packaged) |

## Analysis

### Technical Summary

In packaged WinUI 3 apps on .NET 8, the ANGLE native libraries (libEGL.dll / libGLESv2.dll) fail to load when AngleSwapChainPanel initialises the EGL display. .NET 8 tightened native DLL resolution inside packaged MSIX apps, so the DLLs shipped with SkiaSharp are not discoverable through the default DllImport search path unless they reside in the package's application folder. The same code works unpackaged because the DLL search falls back to the process working directory.

### Rationale

Clear regression signal: works on .NET 7 packaged, works on .NET 8 unpackaged, fails on .NET 8 packaged. The error message ('Error loading EGL entry points: libGLESv2.dll') maps directly to the Egl.cs DllImport('libEGL.dll') / Gles.cs DllImport('libGLESv2.dll') P/Invoke calls inside GlesContext.InitializeDisplay(). .NET 8 changed how packaged apps resolve native DLLs, requiring them to be in the package's application folder or added via a package appxmanifest extension.

### Key Signals

- "It fails regardless of Packaged App or without Packaged App" — **comment #2258080098 (later retracted)** (Initial claim; reporter later corrected this.)
- "It works fine for unpacked WinUI 3 application, regardless of .net version (works on both .net 7 & .net 8). However, It crashes for packaged application." — **comment #2267968914** (Definitive reproduction boundary: packaged .NET 8 only.)
- "logs shows "Error loading EGL entry points: libGLESv2.dll" message" — **comment #2302402196** (Confirms the failure point is ANGLE native DLL loading (libGLESv2.dll), not SkiaSharp core.)

### Code Investigation

| File | Lines | Relevance | Finding |
|------|-------|-----------|---------|
| `source/SkiaSharp.Views/SkiaSharp.Views.WinUI/GlesInterop/Egl.cs` | 1-16 | direct | All EGL entry points use [DllImport("libEGL.dll")]. On .NET 8 in packaged apps, this DLL is not resolved via the default search path without an explicit NativeLibrary.SetDllImportResolver or package manifest declaration. |
| `source/SkiaSharp.Views/SkiaSharp.Views.WinUI/GlesInterop/GlesContext.cs` | 155-243 | direct | InitializeDisplay() calls Egl.eglGetPlatformDisplayEXT; if libEGL.dll fails to load the P/Invoke itself throws, producing the observed crash. There is no try/catch or fallback in this code path. |
| `source/SkiaSharp.Views/SkiaSharp.Views.WinUI/AngleSwapChainPanel.cs` | 124-135 | related | OnLoaded calls 'new GlesContext()' directly with no error handling; any exception from EGL loading crashes the application at panel load time. |

**Error fingerprint:** `egl-load-fail:libGLESv2.dll:winui-packaged:net8`

### Workarounds

- Use unpackaged WinUI 3 app mode (-p:WindowsPackageType=None) — confirmed to work on both .NET 7 and .NET 8
- Use SKCanvasView (CPU raster) instead of SKSwapChainPanel (OpenGL) to avoid the ANGLE dependency entirely

### Next Questions

- Does the same crash occur with SKGLView (also uses AngleSwapChainPanel) in a packaged .NET 8 app?
- Does the Windows App SDK version affect this (1.5 vs 1.6)?
- Is the issue fixed or present in later SkiaSharp 3.x preview or stable releases?
- Is there a NativeLibrary resolver registered for libEGL.dll in the packaged app scenario that would make the DLL discoverable?

### Resolution Proposals

**Hypothesis:** .NET 8 changed native DLL resolution in packaged (MSIX) apps so that DLLs in the NuGet runtimes folder are no longer automatically discoverable via default DllImport search. A NativeLibrary.SetDllImportResolver call or a custom DLL search path setup for ANGLE libraries is required.

1. **Use unpackaged mode as workaround** — workaround, confidence 0.95 (95%), cost/xs, validated=yes
   - Build or publish the WinUI 3 app as unpackaged (-p:WindowsPackageType=None). This is confirmed to work on both .NET 7 and .NET 8.
2. **Fall back to SKCanvasView (CPU)** — alternative, confidence 0.90 (90%), cost/s, validated=untested
   - Replace SKSwapChainPanel with SKCanvasView which uses CPU rasterisation (Skia Raster backend) and has no ANGLE/OpenGL dependency. Performance will be lower but the app will not crash.
3. **Fix: Add NativeLibrary resolver for libEGL.dll in packaged app context** — fix, confidence 0.70 (70%), cost/m, validated=untested
   - Register a NativeLibrary.SetDllImportResolver in the SkiaSharp.Views.WinUI package startup code that resolves libEGL.dll and libGLESv2.dll from the correct path inside a packaged app. This mirrors what the Windows App SDK does for its own native libraries.

**Recommended proposal:** Use unpackaged mode as workaround

**Why:** Immediately actionable with no code change. The proper fix requires investigation into .NET 8 packaged app DLL resolution.

## Recommendations

### Actionability

| Field | Value |
|-------|-------|
| Suggested action | needs-investigation |
| Confidence | 0.88 (88%) |
| Reason | Clear regression with reproduction boundary identified (packaged .NET 8 only). Root cause is .NET 8 native DLL loading change in MSIX packaged apps. Needs deeper investigation into NativeLibrary resolvers and package manifest requirements for ANGLE DLLs. |
| Suggested repro platform | windows |

### Automatable Actions

| Type | Risk | Confidence | Description | Details |
|------|------|------------|-------------|---------|
| update-labels | low | 0.95 (95%) | Apply bug, WinUI views, Windows-WinUI, OpenGL, compatibility and reliability labels | labels=type/bug, area/SkiaSharp.Views, os/Windows-WinUI, backend/OpenGL, tenet/compatibility, tenet/reliability |
| add-comment | medium | 0.88 (88%) | Acknowledge regression, confirm reproduction boundary, provide unpackaged workaround, ask for additional version info | — |
| link-related | low | 0.90 (90%) | Cross-reference with related unpackaged crash issue #2968 | linkedIssue=#2968 |

**Comment draft for `add-comment`:**

```markdown
Thanks for the detailed report and sample project!

Based on the discussion, the reproduction boundary is:
- ✅ .NET 7 packaged WinUI 3 — works
- ✅ .NET 8 **unpackaged** WinUI 3 — works
- ❌ .NET 8 **packaged** WinUI 3 — crashes with `Error loading EGL entry points: libGLESv2.dll`

This points to a .NET 8 change in how packaged (MSIX) apps resolve native DLLs — the ANGLE libraries (`libEGL.dll` / `libGLESv2.dll`) that SkiaSharp uses for OpenGL via `SKSwapChainPanel` are no longer automatically discoverable in the packaged app context.

**Immediate workaround:** Use unpackaged mode by adding `-p:WindowsPackageType=None` to your build command, or set it in your `.csproj`. This is confirmed to work on both .NET 7 and .NET 8.

**Alternative:** Switch to `SKCanvasView` (CPU rasterisation, no ANGLE dependency) if GPU rendering is not required.

Could you also confirm:
1. Which Windows App SDK version are you targeting (1.4, 1.5, or 1.6)?
2. Does the issue also occur with a later SkiaSharp 3.x preview or 3.x stable release?
```

<details>
<summary>Raw JSON</summary>

```json
{
  "meta": {
    "schemaVersion": "1.0",
    "number": 2958,
    "repo": "mono/SkiaSharp",
    "analyzedAt": "2026-04-23T15:52:49Z",
    "currentLabels": [
      "type/bug"
    ]
  },
  "summary": "SKSwapChainPanel crashes in packaged WinUI 3 apps on .NET 8 with 'Error loading EGL entry points: libGLESv2.dll'; the same app works fine unpackaged or on .NET 7.",
  "classification": {
    "type": {
      "value": "type/bug",
      "confidence": 0.95
    },
    "area": {
      "value": "area/SkiaSharp.Views",
      "confidence": 0.92
    },
    "platforms": [
      "os/Windows-WinUI"
    ],
    "backends": [
      "backend/OpenGL"
    ],
    "tenets": [
      "tenet/compatibility",
      "tenet/reliability"
    ]
  },
  "evidence": {
    "bugSignals": {
      "severity": "high",
      "regressionClaimed": true,
      "errorType": "crash",
      "errorMessage": "Error loading EGL entry points: libGLESv2.dll",
      "reproQuality": "partial",
      "targetFrameworks": [
        "net8.0-windows10.0.19041.0"
      ]
    },
    "reproEvidence": {
      "stepsToReproduce": [
        "Create a packaged WinUI 3 app targeting .NET 8",
        "Add SkiaSharp 3.0.0-Preview4.1 and SkiaSharp.Views.WinUI 3.0.0-Preview4.1",
        "Add SKSwapChainPanel to the main window",
        "Run the app as a packaged MSIX"
      ],
      "environmentDetails": "Windows 10, WinUI 3, .NET 8, SkiaSharp 3.0.0-Preview4.1, packaged app (MSIX)",
      "attachments": [
        {
          "url": "https://github.com/user-attachments/files/16426606/WinUI_SKLmnsCanvas_Net8.zip",
          "filename": "WinUI_SKLmnsCanvas_Net8.zip"
        }
      ],
      "repoLinks": [
        {
          "url": "https://github.com/mono/SkiaSharp/issues/2968",
          "description": "Related: SKGLView crashes WinUI 3 MAUI unpackaged app on .NET 9 Preview — same ANGLE/EGL loading root cause, now closed"
        },
        {
          "url": "https://github.com/dotnet/maui/issues/23737",
          "description": "External: dotnet/maui issue referenced by community member as similar EGL loading failure in unpackaged apps"
        }
      ]
    },
    "versionAnalysis": {
      "mentionedVersions": [
        "3.0.0-preview4.1",
        "2.88.2"
      ],
      "workedIn": "2.88.2 / .NET 7",
      "brokeIn": ".NET 8 (packaged)",
      "currentRelevance": "likely",
      "relevanceReason": "The AngleSwapChainPanel/GlesContext code path has not materially changed since this report; packaged-app native DLL loading is a known regression area."
    },
    "regression": {
      "isRegression": true,
      "confidence": 0.85,
      "reason": "Reporter confirms it works on .NET 7 packaged and .NET 8 unpackaged; only .NET 8 packaged fails. .NET 8 changed native DLL search paths in packaged MSIX apps.",
      "workedInVersion": ".NET 7 (packaged)",
      "brokeInVersion": ".NET 8 (packaged)"
    }
  },
  "analysis": {
    "summary": "In packaged WinUI 3 apps on .NET 8, the ANGLE native libraries (libEGL.dll / libGLESv2.dll) fail to load when AngleSwapChainPanel initialises the EGL display. .NET 8 tightened native DLL resolution inside packaged MSIX apps, so the DLLs shipped with SkiaSharp are not discoverable through the default DllImport search path unless they reside in the package's application folder. The same code works unpackaged because the DLL search falls back to the process working directory.",
    "rationale": "Clear regression signal: works on .NET 7 packaged, works on .NET 8 unpackaged, fails on .NET 8 packaged. The error message ('Error loading EGL entry points: libGLESv2.dll') maps directly to the Egl.cs DllImport('libEGL.dll') / Gles.cs DllImport('libGLESv2.dll') P/Invoke calls inside GlesContext.InitializeDisplay(). .NET 8 changed how packaged apps resolve native DLLs, requiring them to be in the package's application folder or added via a package appxmanifest extension.",
    "codeInvestigation": [
      {
        "file": "source/SkiaSharp.Views/SkiaSharp.Views.WinUI/GlesInterop/Egl.cs",
        "lines": "1-16",
        "finding": "All EGL entry points use [DllImport(\"libEGL.dll\")]. On .NET 8 in packaged apps, this DLL is not resolved via the default search path without an explicit NativeLibrary.SetDllImportResolver or package manifest declaration.",
        "relevance": "direct"
      },
      {
        "file": "source/SkiaSharp.Views/SkiaSharp.Views.WinUI/GlesInterop/GlesContext.cs",
        "lines": "155-243",
        "finding": "InitializeDisplay() calls Egl.eglGetPlatformDisplayEXT; if libEGL.dll fails to load the P/Invoke itself throws, producing the observed crash. There is no try/catch or fallback in this code path.",
        "relevance": "direct"
      },
      {
        "file": "source/SkiaSharp.Views/SkiaSharp.Views.WinUI/AngleSwapChainPanel.cs",
        "lines": "124-135",
        "finding": "OnLoaded calls 'new GlesContext()' directly with no error handling; any exception from EGL loading crashes the application at panel load time.",
        "relevance": "related"
      }
    ],
    "keySignals": [
      {
        "text": "It fails regardless of Packaged App or without Packaged App",
        "source": "comment #2258080098 (later retracted)",
        "interpretation": "Initial claim; reporter later corrected this."
      },
      {
        "text": "It works fine for unpacked WinUI 3 application, regardless of .net version (works on both .net 7 & .net 8). However, It crashes for packaged application.",
        "source": "comment #2267968914",
        "interpretation": "Definitive reproduction boundary: packaged .NET 8 only."
      },
      {
        "text": "logs shows \"Error loading EGL entry points: libGLESv2.dll\" message",
        "source": "comment #2302402196",
        "interpretation": "Confirms the failure point is ANGLE native DLL loading (libGLESv2.dll), not SkiaSharp core."
      }
    ],
    "workarounds": [
      "Use unpackaged WinUI 3 app mode (-p:WindowsPackageType=None) — confirmed to work on both .NET 7 and .NET 8",
      "Use SKCanvasView (CPU raster) instead of SKSwapChainPanel (OpenGL) to avoid the ANGLE dependency entirely"
    ],
    "nextQuestions": [
      "Does the same crash occur with SKGLView (also uses AngleSwapChainPanel) in a packaged .NET 8 app?",
      "Does the Windows App SDK version affect this (1.5 vs 1.6)?",
      "Is the issue fixed or present in later SkiaSharp 3.x preview or stable releases?",
      "Is there a NativeLibrary resolver registered for libEGL.dll in the packaged app scenario that would make the DLL discoverable?"
    ],
    "errorFingerprint": "egl-load-fail:libGLESv2.dll:winui-packaged:net8",
    "resolution": {
      "hypothesis": ".NET 8 changed native DLL resolution in packaged (MSIX) apps so that DLLs in the NuGet runtimes folder are no longer automatically discoverable via default DllImport search. A NativeLibrary.SetDllImportResolver call or a custom DLL search path setup for ANGLE libraries is required.",
      "proposals": [
        {
          "title": "Use unpackaged mode as workaround",
          "description": "Build or publish the WinUI 3 app as unpackaged (-p:WindowsPackageType=None). This is confirmed to work on both .NET 7 and .NET 8.",
          "category": "workaround",
          "confidence": 0.95,
          "effort": "cost/xs",
          "validated": "yes"
        },
        {
          "title": "Fall back to SKCanvasView (CPU)",
          "description": "Replace SKSwapChainPanel with SKCanvasView which uses CPU rasterisation (Skia Raster backend) and has no ANGLE/OpenGL dependency. Performance will be lower but the app will not crash.",
          "category": "alternative",
          "confidence": 0.9,
          "effort": "cost/s",
          "validated": "untested"
        },
        {
          "title": "Fix: Add NativeLibrary resolver for libEGL.dll in packaged app context",
          "description": "Register a NativeLibrary.SetDllImportResolver in the SkiaSharp.Views.WinUI package startup code that resolves libEGL.dll and libGLESv2.dll from the correct path inside a packaged app. This mirrors what the Windows App SDK does for its own native libraries.",
          "category": "fix",
          "confidence": 0.7,
          "effort": "cost/m",
          "validated": "untested"
        }
      ],
      "recommendedProposal": "Use unpackaged mode as workaround",
      "recommendedReason": "Immediately actionable with no code change. The proper fix requires investigation into .NET 8 packaged app DLL resolution."
    }
  },
  "output": {
    "actionability": {
      "suggestedAction": "needs-investigation",
      "confidence": 0.88,
      "reason": "Clear regression with reproduction boundary identified (packaged .NET 8 only). Root cause is .NET 8 native DLL loading change in MSIX packaged apps. Needs deeper investigation into NativeLibrary resolvers and package manifest requirements for ANGLE DLLs.",
      "suggestedReproPlatform": "windows"
    },
    "actions": [
      {
        "type": "update-labels",
        "description": "Apply bug, WinUI views, Windows-WinUI, OpenGL, compatibility and reliability labels",
        "risk": "low",
        "confidence": 0.95,
        "labels": [
          "type/bug",
          "area/SkiaSharp.Views",
          "os/Windows-WinUI",
          "backend/OpenGL",
          "tenet/compatibility",
          "tenet/reliability"
        ]
      },
      {
        "type": "add-comment",
        "description": "Acknowledge regression, confirm reproduction boundary, provide unpackaged workaround, ask for additional version info",
        "risk": "medium",
        "confidence": 0.88,
        "comment": "Thanks for the detailed report and sample project!\n\nBased on the discussion, the reproduction boundary is:\n- ✅ .NET 7 packaged WinUI 3 — works\n- ✅ .NET 8 **unpackaged** WinUI 3 — works\n- ❌ .NET 8 **packaged** WinUI 3 — crashes with `Error loading EGL entry points: libGLESv2.dll`\n\nThis points to a .NET 8 change in how packaged (MSIX) apps resolve native DLLs — the ANGLE libraries (`libEGL.dll` / `libGLESv2.dll`) that SkiaSharp uses for OpenGL via `SKSwapChainPanel` are no longer automatically discoverable in the packaged app context.\n\n**Immediate workaround:** Use unpackaged mode by adding `-p:WindowsPackageType=None` to your build command, or set it in your `.csproj`. This is confirmed to work on both .NET 7 and .NET 8.\n\n**Alternative:** Switch to `SKCanvasView` (CPU rasterisation, no ANGLE dependency) if GPU rendering is not required.\n\nCould you also confirm:\n1. Which Windows App SDK version are you targeting (1.4, 1.5, or 1.6)?\n2. Does the issue also occur with a later SkiaSharp 3.x preview or 3.x stable release?"
      },
      {
        "type": "link-related",
        "description": "Cross-reference with related unpackaged crash issue #2968",
        "risk": "low",
        "confidence": 0.9,
        "linkedIssue": 2968
      }
    ]
  }
}
```

</details>
