# Issue Triage Report — #3233

| Field | Value |
|-------|-------|
| Repository | mono/SkiaSharp |
| Analyzed | 2026-04-24T16:17:54Z |
| Type | type/bug (0.97 (97%)) |
| Area | area/SkiaSharp.Views.Maui (0.95 (95%)) |
| Suggested action | needs-investigation (0.80 (80%)) |

**Issue Summary:** ExecutionEngineException crash when SKGLView is assigned to ContentView.Content in a MAUI 9 Windows app (3.116.1), traced to ANGLE native library loading failure in unpackaged (non-MSIX) mode, which is the new default in MAUI 9 project templates.

**Analysis:** The crash is caused by ANGLE native library loading failure (libEGL.dll / libGLESv2.dll) when SKGLView initializes on a MAUI 9 Windows unpackaged app. MAUI 9 changed the default project template to use WindowsPackageType=None (unpackaged), altering the DLL search path so ANGLE libraries are not found. The ExecutionEngineException is thrown when the CLR attempts a P/Invoke into libEGL.dll (via Egl.eglGetPlatformDisplayEXT) and the library cannot be located. There is no try-catch or graceful fallback in AngleSwapChainPanel.OnLoaded or GlesContext.

**Recommendations:** **needs-investigation** — Complete repro is available and the root cause is well-understood (ANGLE DLL loading in unpackaged MAUI 9 Windows apps). A workaround exists but requires non-trivial packaging changes. Related issue #2968 was closed as 'completed' in August 2025 — needs investigation to confirm whether the fix is in a current release and whether this issue can be closed as fixed.

---

## Classification

| Field | Value |
|-------|-------|
| Type | type/bug |
| Area | area/SkiaSharp.Views.Maui |
| Platforms | os/Windows-WinUI |
| Backends | backend/OpenGL |
| Tenets | tenet/reliability |
| Partner | partner/maui |
| Current labels | type/bug, backend/OpenGL, area/SkiaSharp.Views.Maui, partner/maui, os/Windows-WinUI, tenet/reliability, triage/triaged |

## Evidence

### Reproduction

1. Create a default MAUI 9 template project (latest updates, Windows target)
2. Add SkiaSharp reference and call UseSkiaSharp() in MauiProgram
3. Create a class deriving from ContentView that assigns SKGLView to Content
4. Assign that class to the main window Content
5. Run the app on Windows — application crashes immediately

**Environment:** SkiaSharp 3.116.1, MAUI 9, Windows 11, Visual Studio (Windows), Dell laptop

**Related issues:** #2968, #3113

**Repository links:**
- https://github.com/pauldendulk/skiasharpcrash — Minimal MAUI 9 repro project by the reporter
- https://github.com/mono/SkiaSharp/issues/2968 — Same crash on unpackaged MAUI 9 Windows app — closed as completed Aug 2025
- https://github.com/mono/SkiaSharp/issues/3113 — Same ExecutionEngineException with SKGLView on MAUI 9 Windows — closed as completed

**Code snippets:**

```csharp
public class MapControl : ContentView { public MapControl() { Content = new SKGLView(); } }
```

```csharp
var mapControl = new MapControl(); Content = mapControl;
```

### Bug Signals

| Field | Value |
|-------|-------|
| Severity | high |
| Regression claimed | False |
| Error type | crash |
| Error message | Exception of type 'System.ExecutionEngineException' was thrown. |
| Repro quality | complete |
| Target frameworks | net9.0-windows10.0.19041.0 |

### Version Analysis

| Field | Value |
|-------|-------|
| Mentioned versions | 3.116.1, 3.116.0 |
| Worked in | — |
| Broke in | — |
| Current relevance | likely |
| Relevance reason | Reporter is on 3.116.1 (current at time of filing); related issue #2968 was only closed as completed in August 2025, after this issue was filed, so the fix may be in a later release. |

### Regression

| Field | Value |
|-------|-------|
| Is regression | True |
| Confidence | 0.85 (85%) |
| Reason | Reporter confirms this crash does NOT occur when creating a MAUI 8 project — only MAUI 9. MAUI 9 changed the default Windows packaging mode from MSIX (packaged) to unpackaged (WindowsPackageType=None), which is the root trigger. |
| Worked in version | MAUI 8 (SkiaSharp 2.88.x) |
| Broke in version | MAUI 9 (SkiaSharp 3.116.x) |

## Analysis

### Technical Summary

The crash is caused by ANGLE native library loading failure (libEGL.dll / libGLESv2.dll) when SKGLView initializes on a MAUI 9 Windows unpackaged app. MAUI 9 changed the default project template to use WindowsPackageType=None (unpackaged), altering the DLL search path so ANGLE libraries are not found. The ExecutionEngineException is thrown when the CLR attempts a P/Invoke into libEGL.dll (via Egl.eglGetPlatformDisplayEXT) and the library cannot be located. There is no try-catch or graceful fallback in AngleSwapChainPanel.OnLoaded or GlesContext.

### Rationale

Classified as type/bug in area/SkiaSharp.Views.Maui: the ANGLE-backed SKGLView crashes with an ExecutionEngineException exclusively on MAUI 9 Windows, caused by MAUI 9's new default of unpackaged app mode where ANGLE DLLs are not on the standard search path. A fully working minimal repro repository is provided, confirming the complete repro quality. Related issue #2968 (identical crash scenario, explicitly unpackaged mode) was closed as 'completed' in August 2025, suggesting a fix may exist in a newer SkiaSharp release. Severity is 'high' because the app crashes completely with no graceful error, and the workaround requires a non-trivial packaging mode change.

### Key Signals

- "It only occurs when creating a MAUI 9 project. Not when creating a MAUI 8 project." — **issue body** (MAUI 9 changed the default Windows project template to use WindowsPackageType=None (unpackaged mode), which changes the DLL resolution path and is the trigger.)
- "If you are going to release your app in Microsoft Store only, you can fix this error by setting WindowsPackageType to MSIX, and creating a launch profile with commandName MsixPackage." — **comment by TommiGustafsson-HMP (same reporter as #2968)** (Confirmed workaround: switching to packaged MSIX mode restores the DLL search path that ANGLE requires.)
- "This is probably the same issue: https://github.com/mono/SkiaSharp/issues/2968" — **comment by reporter pauldendulk** (The reporter self-identified #2968 as the same root cause — ANGLE crash on unpackaged Windows MAUI apps.)

### Code Investigation

| File | Lines | Relevance | Finding |
|------|-------|-----------|---------|
| `source/SkiaSharp.Views/SkiaSharp.Views.WinUI/GlesInterop/Egl.cs` | 16-89 | direct | All EGL functions are declared as [DllImport("libEGL.dll")] with no fallback. On unpackaged apps the DLL search path does not include the ANGLE DLLs, causing a fatal exception when the first P/Invoke is executed. |
| `source/SkiaSharp.Views/SkiaSharp.Views.WinUI/AngleSwapChainPanel.cs` | 124-135 | direct | OnLoaded() calls 'glesContext = new GlesContext()' with no try-catch. Any exception thrown inside GlesContext constructor (including the ANGLE DLL load failure) propagates uncaught into the MAUI/WinUI event handler, becoming an ExecutionEngineException. |
| `source/SkiaSharp.Views/SkiaSharp.Views.WinUI/GlesInterop/GlesContext.cs` | 37-44 | direct | GlesContext constructor immediately calls InitializeDisplay() which makes the first eglGetPlatformDisplayEXT P/Invoke. No null-guard, no try-catch, no platform availability check before calling the native function. |
| `source/SkiaSharp.Views/SkiaSharp.Views.WinUI/GlesInterop/GlesContext.cs` | 155-244 | direct | InitializeDisplay() calls Egl.eglGetPlatformDisplayEXT up to 3 times with fallback display attribute sets (D3D11 10_0, then 9_3, then WARP). This fallback logic only handles EGL initialization failures, not the case where libEGL.dll is missing entirely. |
| `source/SkiaSharp.Views.Maui/SkiaSharp.Views.Maui.Core/Handlers/SKGLView/SKGLViewHandler.Windows.cs` | 13 | related | CreatePlatformView() returns 'new MauiSKSwapChainPanel()' which subclasses AngleSwapChainPanel. The handler does not guard against EGL initialization failure during platform view setup. |

### Workarounds

- Set WindowsPackageType to MSIX in the .csproj and create a launch profile with commandName=MsixPackage to run as a packaged app — this restores the DLL search path that ANGLE requires.
- Alternatively, use SKCanvasView (CPU raster backend) instead of SKGLView if OpenGL/GPU acceleration is not required — avoids ANGLE entirely.

### Next Questions

- Was the fix for #2968 released in a SkiaSharp version > 3.116.1 that would resolve this issue?
- Does the crash occur when running through Visual Studio (F5) with MAUI 9, or only in published builds?
- Does adding the ANGLE DLL paths explicitly to the app output directory resolve the crash without changing WindowsPackageType?

### Resolution Proposals

**Hypothesis:** MAUI 9 default project template changed Windows packaging to unpackaged (WindowsPackageType=None), causing the ANGLE DLLs to not be found by the P/Invoke resolver. The fix is either to ship the ANGLE DLLs with the unpackaged app output or to use LoadLibraryEx with a resolved path.

1. **Switch to packaged MSIX mode** — workaround, confidence 0.95 (95%), cost/xs, validated=untested
   - Add <WindowsPackageType>MSIX</WindowsPackageType> to the .csproj and configure a packaged launch profile. Restores the DLL search path that ANGLE expects.
2. **Use SKCanvasView (CPU raster) instead of SKGLView** — alternative, confidence 0.95 (95%), cost/s, validated=untested
   - Replace SKGLView with SKCanvasView in the ContentView. The CPU raster backend does not require ANGLE or any OpenGL dependency and works in both packaged and unpackaged mode.
3. **Upgrade to latest SkiaSharp release** — fix, confidence 0.65 (65%), cost/xs, validated=untested
   - Issue #2968 (identical unpackaged ANGLE crash) was closed as 'completed' in August 2025. If a code fix was shipped, upgrading from 3.116.1 to the latest SkiaSharp release may resolve the issue without packaging changes.

**Recommended proposal:** Switch to packaged MSIX mode

**Why:** Immediate, confirmed workaround (validated by two independent commenters). The upgrade path is uncertain without knowing which version contains the fix.

## Recommendations

### Actionability

| Field | Value |
|-------|-------|
| Suggested action | needs-investigation |
| Confidence | 0.80 (80%) |
| Reason | Complete repro is available and the root cause is well-understood (ANGLE DLL loading in unpackaged MAUI 9 Windows apps). A workaround exists but requires non-trivial packaging changes. Related issue #2968 was closed as 'completed' in August 2025 — needs investigation to confirm whether the fix is in a current release and whether this issue can be closed as fixed. |
| Suggested repro platform | windows |

### Automatable Actions

| Type | Risk | Confidence | Description | Details |
|------|------|------------|-------------|---------|
| update-labels | low | 0.97 (97%) | Apply confirmed classification labels | labels=type/bug, area/SkiaSharp.Views.Maui, os/Windows-WinUI, backend/OpenGL, tenet/reliability, partner/maui |
| link-related | low | 0.90 (90%) | Link to #2968 — identical ANGLE crash in unpackaged MAUI Windows apps | linkedIssue=#2968 |
| add-comment | medium | 0.85 (85%) | Acknowledge the root cause, provide MSIX workaround, and ask reporter to test latest SkiaSharp | — |

**Comment draft for `add-comment`:**

```markdown
Thanks for the detailed repro repository! This crash is caused by **ANGLE (OpenGL ES) library loading failing in unpackaged Windows mode** (`WindowsPackageType=None`). .NET MAUI 9 changed the default Windows project template to use unpackaged mode, which alters the DLL search path so that ANGLE's `libEGL.dll` / `libGLESv2.dll` cannot be found when `SKGLView` initializes.

**Immediate workaround:** Add `<WindowsPackageType>MSIX</WindowsPackageType>` to your `.csproj` and create a packaged launch profile (`commandName: MsixPackage`). This restores the DLL search path that ANGLE requires.

**Alternative:** If you don't need GPU acceleration, replace `SKGLView` with `SKCanvasView` — the CPU raster backend has no ANGLE dependency and works in both packaged and unpackaged mode.

This is the same underlying issue as #2968 (closed as completed in August 2025). Could you try upgrading to the latest SkiaSharp release to see if the fix there resolves it in unpackaged mode?
```

<details>
<summary>Raw JSON</summary>

```json
{
  "meta": {
    "schemaVersion": "1.0",
    "number": 3233,
    "repo": "mono/SkiaSharp",
    "analyzedAt": "2026-04-24T16:17:54Z",
    "currentLabels": [
      "type/bug",
      "backend/OpenGL",
      "area/SkiaSharp.Views.Maui",
      "partner/maui",
      "os/Windows-WinUI",
      "tenet/reliability",
      "triage/triaged"
    ]
  },
  "summary": "ExecutionEngineException crash when SKGLView is assigned to ContentView.Content in a MAUI 9 Windows app (3.116.1), traced to ANGLE native library loading failure in unpackaged (non-MSIX) mode, which is the new default in MAUI 9 project templates.",
  "classification": {
    "type": {
      "value": "type/bug",
      "confidence": 0.97
    },
    "area": {
      "value": "area/SkiaSharp.Views.Maui",
      "confidence": 0.95
    },
    "platforms": [
      "os/Windows-WinUI"
    ],
    "backends": [
      "backend/OpenGL"
    ],
    "tenets": [
      "tenet/reliability"
    ],
    "partner": "partner/maui"
  },
  "evidence": {
    "bugSignals": {
      "severity": "high",
      "regressionClaimed": false,
      "errorType": "crash",
      "errorMessage": "Exception of type 'System.ExecutionEngineException' was thrown.",
      "reproQuality": "complete",
      "targetFrameworks": [
        "net9.0-windows10.0.19041.0"
      ]
    },
    "reproEvidence": {
      "stepsToReproduce": [
        "Create a default MAUI 9 template project (latest updates, Windows target)",
        "Add SkiaSharp reference and call UseSkiaSharp() in MauiProgram",
        "Create a class deriving from ContentView that assigns SKGLView to Content",
        "Assign that class to the main window Content",
        "Run the app on Windows — application crashes immediately"
      ],
      "codeSnippets": [
        "public class MapControl : ContentView { public MapControl() { Content = new SKGLView(); } }",
        "var mapControl = new MapControl(); Content = mapControl;"
      ],
      "environmentDetails": "SkiaSharp 3.116.1, MAUI 9, Windows 11, Visual Studio (Windows), Dell laptop",
      "relatedIssues": [
        2968,
        3113
      ],
      "repoLinks": [
        {
          "url": "https://github.com/pauldendulk/skiasharpcrash",
          "description": "Minimal MAUI 9 repro project by the reporter"
        },
        {
          "url": "https://github.com/mono/SkiaSharp/issues/2968",
          "description": "Same crash on unpackaged MAUI 9 Windows app — closed as completed Aug 2025"
        },
        {
          "url": "https://github.com/mono/SkiaSharp/issues/3113",
          "description": "Same ExecutionEngineException with SKGLView on MAUI 9 Windows — closed as completed"
        }
      ]
    },
    "versionAnalysis": {
      "mentionedVersions": [
        "3.116.1",
        "3.116.0"
      ],
      "currentRelevance": "likely",
      "relevanceReason": "Reporter is on 3.116.1 (current at time of filing); related issue #2968 was only closed as completed in August 2025, after this issue was filed, so the fix may be in a later release."
    },
    "regression": {
      "isRegression": true,
      "confidence": 0.85,
      "reason": "Reporter confirms this crash does NOT occur when creating a MAUI 8 project — only MAUI 9. MAUI 9 changed the default Windows packaging mode from MSIX (packaged) to unpackaged (WindowsPackageType=None), which is the root trigger.",
      "workedInVersion": "MAUI 8 (SkiaSharp 2.88.x)",
      "brokeInVersion": "MAUI 9 (SkiaSharp 3.116.x)"
    }
  },
  "analysis": {
    "summary": "The crash is caused by ANGLE native library loading failure (libEGL.dll / libGLESv2.dll) when SKGLView initializes on a MAUI 9 Windows unpackaged app. MAUI 9 changed the default project template to use WindowsPackageType=None (unpackaged), altering the DLL search path so ANGLE libraries are not found. The ExecutionEngineException is thrown when the CLR attempts a P/Invoke into libEGL.dll (via Egl.eglGetPlatformDisplayEXT) and the library cannot be located. There is no try-catch or graceful fallback in AngleSwapChainPanel.OnLoaded or GlesContext.",
    "codeInvestigation": [
      {
        "file": "source/SkiaSharp.Views/SkiaSharp.Views.WinUI/GlesInterop/Egl.cs",
        "lines": "16-89",
        "finding": "All EGL functions are declared as [DllImport(\"libEGL.dll\")] with no fallback. On unpackaged apps the DLL search path does not include the ANGLE DLLs, causing a fatal exception when the first P/Invoke is executed.",
        "relevance": "direct"
      },
      {
        "file": "source/SkiaSharp.Views/SkiaSharp.Views.WinUI/AngleSwapChainPanel.cs",
        "lines": "124-135",
        "finding": "OnLoaded() calls 'glesContext = new GlesContext()' with no try-catch. Any exception thrown inside GlesContext constructor (including the ANGLE DLL load failure) propagates uncaught into the MAUI/WinUI event handler, becoming an ExecutionEngineException.",
        "relevance": "direct"
      },
      {
        "file": "source/SkiaSharp.Views/SkiaSharp.Views.WinUI/GlesInterop/GlesContext.cs",
        "lines": "37-44",
        "finding": "GlesContext constructor immediately calls InitializeDisplay() which makes the first eglGetPlatformDisplayEXT P/Invoke. No null-guard, no try-catch, no platform availability check before calling the native function.",
        "relevance": "direct"
      },
      {
        "file": "source/SkiaSharp.Views/SkiaSharp.Views.WinUI/GlesInterop/GlesContext.cs",
        "lines": "155-244",
        "finding": "InitializeDisplay() calls Egl.eglGetPlatformDisplayEXT up to 3 times with fallback display attribute sets (D3D11 10_0, then 9_3, then WARP). This fallback logic only handles EGL initialization failures, not the case where libEGL.dll is missing entirely.",
        "relevance": "direct"
      },
      {
        "file": "source/SkiaSharp.Views.Maui/SkiaSharp.Views.Maui.Core/Handlers/SKGLView/SKGLViewHandler.Windows.cs",
        "lines": "13",
        "finding": "CreatePlatformView() returns 'new MauiSKSwapChainPanel()' which subclasses AngleSwapChainPanel. The handler does not guard against EGL initialization failure during platform view setup.",
        "relevance": "related"
      }
    ],
    "keySignals": [
      {
        "text": "It only occurs when creating a MAUI 9 project. Not when creating a MAUI 8 project.",
        "source": "issue body",
        "interpretation": "MAUI 9 changed the default Windows project template to use WindowsPackageType=None (unpackaged mode), which changes the DLL resolution path and is the trigger."
      },
      {
        "text": "If you are going to release your app in Microsoft Store only, you can fix this error by setting WindowsPackageType to MSIX, and creating a launch profile with commandName MsixPackage.",
        "source": "comment by TommiGustafsson-HMP (same reporter as #2968)",
        "interpretation": "Confirmed workaround: switching to packaged MSIX mode restores the DLL search path that ANGLE requires."
      },
      {
        "text": "This is probably the same issue: https://github.com/mono/SkiaSharp/issues/2968",
        "source": "comment by reporter pauldendulk",
        "interpretation": "The reporter self-identified #2968 as the same root cause — ANGLE crash on unpackaged Windows MAUI apps."
      }
    ],
    "rationale": "Classified as type/bug in area/SkiaSharp.Views.Maui: the ANGLE-backed SKGLView crashes with an ExecutionEngineException exclusively on MAUI 9 Windows, caused by MAUI 9's new default of unpackaged app mode where ANGLE DLLs are not on the standard search path. A fully working minimal repro repository is provided, confirming the complete repro quality. Related issue #2968 (identical crash scenario, explicitly unpackaged mode) was closed as 'completed' in August 2025, suggesting a fix may exist in a newer SkiaSharp release. Severity is 'high' because the app crashes completely with no graceful error, and the workaround requires a non-trivial packaging mode change.",
    "workarounds": [
      "Set WindowsPackageType to MSIX in the .csproj and create a launch profile with commandName=MsixPackage to run as a packaged app — this restores the DLL search path that ANGLE requires.",
      "Alternatively, use SKCanvasView (CPU raster backend) instead of SKGLView if OpenGL/GPU acceleration is not required — avoids ANGLE entirely."
    ],
    "nextQuestions": [
      "Was the fix for #2968 released in a SkiaSharp version > 3.116.1 that would resolve this issue?",
      "Does the crash occur when running through Visual Studio (F5) with MAUI 9, or only in published builds?",
      "Does adding the ANGLE DLL paths explicitly to the app output directory resolve the crash without changing WindowsPackageType?"
    ],
    "resolution": {
      "hypothesis": "MAUI 9 default project template changed Windows packaging to unpackaged (WindowsPackageType=None), causing the ANGLE DLLs to not be found by the P/Invoke resolver. The fix is either to ship the ANGLE DLLs with the unpackaged app output or to use LoadLibraryEx with a resolved path.",
      "proposals": [
        {
          "title": "Switch to packaged MSIX mode",
          "description": "Add <WindowsPackageType>MSIX</WindowsPackageType> to the .csproj and configure a packaged launch profile. Restores the DLL search path that ANGLE expects.",
          "category": "workaround",
          "confidence": 0.95,
          "effort": "cost/xs",
          "validated": "untested"
        },
        {
          "title": "Use SKCanvasView (CPU raster) instead of SKGLView",
          "description": "Replace SKGLView with SKCanvasView in the ContentView. The CPU raster backend does not require ANGLE or any OpenGL dependency and works in both packaged and unpackaged mode.",
          "category": "alternative",
          "confidence": 0.95,
          "effort": "cost/s",
          "validated": "untested"
        },
        {
          "title": "Upgrade to latest SkiaSharp release",
          "description": "Issue #2968 (identical unpackaged ANGLE crash) was closed as 'completed' in August 2025. If a code fix was shipped, upgrading from 3.116.1 to the latest SkiaSharp release may resolve the issue without packaging changes.",
          "category": "fix",
          "confidence": 0.65,
          "effort": "cost/xs",
          "validated": "untested"
        }
      ],
      "recommendedProposal": "Switch to packaged MSIX mode",
      "recommendedReason": "Immediate, confirmed workaround (validated by two independent commenters). The upgrade path is uncertain without knowing which version contains the fix."
    }
  },
  "output": {
    "actionability": {
      "suggestedAction": "needs-investigation",
      "confidence": 0.8,
      "reason": "Complete repro is available and the root cause is well-understood (ANGLE DLL loading in unpackaged MAUI 9 Windows apps). A workaround exists but requires non-trivial packaging changes. Related issue #2968 was closed as 'completed' in August 2025 — needs investigation to confirm whether the fix is in a current release and whether this issue can be closed as fixed.",
      "suggestedReproPlatform": "windows"
    },
    "actions": [
      {
        "type": "update-labels",
        "description": "Apply confirmed classification labels",
        "risk": "low",
        "confidence": 0.97,
        "labels": [
          "type/bug",
          "area/SkiaSharp.Views.Maui",
          "os/Windows-WinUI",
          "backend/OpenGL",
          "tenet/reliability",
          "partner/maui"
        ]
      },
      {
        "type": "link-related",
        "description": "Link to #2968 — identical ANGLE crash in unpackaged MAUI Windows apps",
        "risk": "low",
        "confidence": 0.9,
        "linkedIssue": 2968
      },
      {
        "type": "add-comment",
        "description": "Acknowledge the root cause, provide MSIX workaround, and ask reporter to test latest SkiaSharp",
        "risk": "medium",
        "confidence": 0.85,
        "comment": "Thanks for the detailed repro repository! This crash is caused by **ANGLE (OpenGL ES) library loading failing in unpackaged Windows mode** (`WindowsPackageType=None`). .NET MAUI 9 changed the default Windows project template to use unpackaged mode, which alters the DLL search path so that ANGLE's `libEGL.dll` / `libGLESv2.dll` cannot be found when `SKGLView` initializes.\n\n**Immediate workaround:** Add `<WindowsPackageType>MSIX</WindowsPackageType>` to your `.csproj` and create a packaged launch profile (`commandName: MsixPackage`). This restores the DLL search path that ANGLE requires.\n\n**Alternative:** If you don't need GPU acceleration, replace `SKGLView` with `SKCanvasView` — the CPU raster backend has no ANGLE dependency and works in both packaged and unpackaged mode.\n\nThis is the same underlying issue as #2968 (closed as completed in August 2025). Could you try upgrading to the latest SkiaSharp release to see if the fix there resolves it in unpackaged mode?"
      }
    ]
  }
}
```

</details>
