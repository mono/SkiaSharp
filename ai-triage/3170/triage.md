# Issue Triage Report — #3170

| Field | Value |
|-------|-------|
| Repository | mono/SkiaSharp |
| Analyzed | 2026-04-28T12:20:00Z |
| Type | type/bug (0.95 (95%)) |
| Area | area/SkiaSharp.Views (0.92 (92%)) |
| Suggested action | needs-investigation (0.87 (87%)) |

**Issue Summary:** MAUI app using SKCanvasView crashes with TypeInitializationException for SkiaSharp.Views.WinUI.Native.BufferExtensions on clean Windows 11 when running as MSIX package, but works locally with VS debugger attached.

**Analysis:** The crash is caused by the native C++/WinRT component SkiaSharp.Views.WinUI.Native.dll failing to initialize on clean Windows machines because the MSVC runtime is not statically linked. The DLL fails COM class activation with REGDB_E_CLASSNOTREG when the VC++ redistributable is absent — identical root cause confirmed in related issue #3136.

**Recommendations:** **needs-investigation** — Root cause is confirmed (dynamic MSVC runtime linking in native WinUI DLL) via related issue #3136, but no fix has been released. Active maintainer investigation ongoing. Needs tracking alongside #3136.

---

## Classification

| Field | Value |
|-------|-------|
| Type | type/bug |
| Area | area/SkiaSharp.Views |
| Platforms | os/Windows-WinUI |
| Backends | — |
| Tenets | tenet/reliability |
| Partner | partner/maui |
| Current labels | type/bug |

## Evidence

### Reproduction

1. Create a new MAUI app in Visual Studio
2. Add .UseSkiaSharp() in builder and reference SkiaSharp.Views.Maui.Controls
3. Add SKCanvasView and register PaintSurface handler
4. Build and create MSIX package
5. Run on a clean Windows 11 machine without VC++ redistributable pre-installed

**Environment:** Windows 11 clean machine; SkiaSharp 3.116.0; tested on .NET 8 and .NET 9; fails during Microsoft Store certification on clean VM

**Related issues:** #3136

**Repository links:**
- https://github.com/mono/SkiaSharp/issues/3136 — Related issue: same TypeInitializationException for BufferExtensions on Windows IoT and old GPU systems — confirmed same root cause, maintainer actively investigating

### Bug Signals

| Field | Value |
|-------|-------|
| Severity | high |
| Regression claimed | True |
| Error type | exception |
| Error message | The type initializer for 'SkiaSharp.Views.WinUI.Native.BufferExtensions' threw an exception. |
| Repro quality | partial |
| Target frameworks | net8.0-windows10.0, net9.0-windows10.0 |

### Version Analysis

| Field | Value |
|-------|-------|
| Mentioned versions | 3.116.0, 2.88.9 |
| Worked in | 2.88.9 |
| Broke in | 3.116.0 |
| Current relevance | likely |
| Relevance reason | No fix has been released as of triage date. Maintainer confirmed the root cause (dynamic MSVC runtime linking) in #3136 and is working on a fix. |

### Regression

| Field | Value |
|-------|-------|
| Is regression | True |
| Confidence | 0.92 (92%) |
| Reason | Reporter confirms 2.88.9 worked. In 3.x, SkiaSharp.Views.WinUI.Native was introduced as a new C++/WinRT component that depends on the MSVC runtime being installed, which is absent on clean machines. |
| Worked in version | 2.88.9 |
| Broke in version | 3.116.0 |

## Analysis

### Technical Summary

The crash is caused by the native C++/WinRT component SkiaSharp.Views.WinUI.Native.dll failing to initialize on clean Windows machines because the MSVC runtime is not statically linked. The DLL fails COM class activation with REGDB_E_CLASSNOTREG when the VC++ redistributable is absent — identical root cause confirmed in related issue #3136.

### Rationale

The exception type, component name, and clean-machine-only context all match the confirmed root cause in #3136. Both issues exhibit TypeInitializationException on BufferExtensions with the same SkiaSharp version (3.116.0), same introduced C++/WinRT component, and the same workaround (install VC++ redistributable). The contextual difference (MSIX/Store certification vs Windows IoT) does not change the underlying defect. Maintainer @mattleibow stated in #3136: 'I think that I was building the interop and not statically linking the runtime.'

### Key Signals

- "WinRT originate error - 0x80131534 : 'The type initializer for 'SkiaSharp.Views.WinUI.Native.BufferExtensions' threw an exception.'" — **issue body** (The native C++/WinRT component fails to initialize — COM activation fails at type initialization due to missing runtime dependency.)
- "works without issue locally...fails certification on a clean windows 11 machine" — **issue body** (VC++ redistributable is present on dev machine (installed by Visual Studio) but absent on the clean certification VM — confirms missing runtime is the trigger.)
- "Last Known Good Version: 2.88.9" — **issue form** (Regression introduced in 3.x when SkiaSharp.Views.WinUI.Native C++/WinRT component was added; 2.88.x did not have this component.)

### Code Investigation

| File | Lines | Relevance | Finding |
|------|-------|-----------|---------|
| `native/winui/SkiaSharp.Views.WinUI.Native/SkiaSharp.Views.WinUI.Native/BufferExtensions.cpp` | — | direct | BufferExtensions is a C++/WinRT implementation that exposes a WinRT activation factory for GetByteBuffer(IBuffer). If the MSVC runtime DLLs are not statically linked (i.e., /MD instead of /MT build flag), the component will fail to load on machines without VC++ redistributable installed, causing REGDB_E_CLASSNOTREG at COM activation. |
| `source/SkiaSharp.Views/SkiaSharp.Views.WinUI/UWPExtensions.cs` | 176-180 | direct | GetPixels(WriteableBitmap) calls buffer.GetByteBuffer() which calls into BufferExtensions.GetByteBuffer(buffer) — the C++/WinRT P/Invoke boundary. This is triggered during SKXamlCanvas.CreateBitmap() on first render, explaining why the crash occurs when the canvas first paints. |

**Error fingerprint:** `TypeInitializationException:BufferExtensions:WinRT:REGDB_E_CLASSNOTREG`

### Workarounds

- Install the MSVC runtime redistributable on affected machines: `winget install Microsoft.VCRedist.2015+.x64`
- Include the VC++ redistributable as a prerequisite in the MSIX installer/package manifest

### Next Questions

- Does including the VC++ redistributable as a package dependency in the MSIX manifest resolve the Store certification failure?
- Is the root cause precisely the same as #3136 or does the MSIX packaging add another layer?

### Resolution Proposals

**Hypothesis:** SkiaSharp.Views.WinUI.Native.dll is built with dynamically-linked MSVC runtime (/MD). On clean machines without the VC++ redistributable, the WinRT factory activation fails with REGDB_E_CLASSNOTREG at type initialization.

1. **Install VC++ Redistributable as immediate workaround** — workaround, confidence 0.85 (85%), cost/xs, validated=yes
   - Install Microsoft.VCRedist.2015+.x64 on affected machines. Confirmed workaround from #3136 comments by community member.

```csharp
winget install Microsoft.VCRedist.2015+.x64
```
2. **Statically link MSVC runtime in native WinUI DLL build** — fix, confidence 0.87 (87%), cost/s, validated=untested
   - Rebuild SkiaSharp.Views.WinUI.Native.dll with /MT (static runtime linking) so it carries no external MSVC runtime dependency. Maintainer confirmed this is the intended fix in #3136.

**Recommended proposal:** Statically link MSVC runtime in native WinUI DLL build

**Why:** Eliminates the root cause without requiring any action from end users, app distributors, or MSIX package manifests.

## Recommendations

### Actionability

| Field | Value |
|-------|-------|
| Suggested action | needs-investigation |
| Confidence | 0.87 (87%) |
| Reason | Root cause is confirmed (dynamic MSVC runtime linking in native WinUI DLL) via related issue #3136, but no fix has been released. Active maintainer investigation ongoing. Needs tracking alongside #3136. |
| Suggested repro platform | windows |

### Automatable Actions

| Type | Risk | Confidence | Description | Details |
|------|------|------------|-------------|---------|
| update-labels | low | 0.95 (95%) | Apply bug, SkiaSharp.Views, Windows-WinUI, MAUI partner, reliability tenet labels | labels=type/bug, area/SkiaSharp.Views, os/Windows-WinUI, partner/maui, tenet/reliability |
| link-related | low | 0.90 (90%) | Link to #3136 which has the same root cause and active maintainer investigation | linkedIssue=#3136 |
| add-comment | medium | 0.87 (87%) | Post analysis with workaround and cross-reference to #3136 | — |

**Comment draft for `add-comment`:**

```markdown
Thanks for the detailed report. This crash is related to the same issue tracked in #3136 — the `SkiaSharp.Views.WinUI.Native.dll` C++/WinRT component was built without statically linking the MSVC runtime, so it fails to initialize on clean machines that don't have the Visual C++ Redistributable installed.

**Immediate workaround:**
Install the VC++ redistributable on affected machines:
```
winget install Microsoft.VCRedist.2015+.x64
```
Or include it as a prerequisite in your MSIX package manifest.

A proper fix (statically linking the MSVC runtime in the native DLL build) is being investigated in #3136.
```

<details>
<summary>Raw JSON</summary>

```json
{
  "meta": {
    "schemaVersion": "1.0",
    "number": 3170,
    "repo": "mono/SkiaSharp",
    "analyzedAt": "2026-04-28T12:20:00Z",
    "currentLabels": [
      "type/bug"
    ]
  },
  "summary": "MAUI app using SKCanvasView crashes with TypeInitializationException for SkiaSharp.Views.WinUI.Native.BufferExtensions on clean Windows 11 when running as MSIX package, but works locally with VS debugger attached.",
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
    "tenets": [
      "tenet/reliability"
    ],
    "partner": "partner/maui"
  },
  "evidence": {
    "bugSignals": {
      "severity": "high",
      "regressionClaimed": true,
      "errorType": "exception",
      "errorMessage": "The type initializer for 'SkiaSharp.Views.WinUI.Native.BufferExtensions' threw an exception.",
      "reproQuality": "partial",
      "targetFrameworks": [
        "net8.0-windows10.0",
        "net9.0-windows10.0"
      ]
    },
    "reproEvidence": {
      "stepsToReproduce": [
        "Create a new MAUI app in Visual Studio",
        "Add .UseSkiaSharp() in builder and reference SkiaSharp.Views.Maui.Controls",
        "Add SKCanvasView and register PaintSurface handler",
        "Build and create MSIX package",
        "Run on a clean Windows 11 machine without VC++ redistributable pre-installed"
      ],
      "environmentDetails": "Windows 11 clean machine; SkiaSharp 3.116.0; tested on .NET 8 and .NET 9; fails during Microsoft Store certification on clean VM",
      "relatedIssues": [
        3136
      ],
      "repoLinks": [
        {
          "url": "https://github.com/mono/SkiaSharp/issues/3136",
          "description": "Related issue: same TypeInitializationException for BufferExtensions on Windows IoT and old GPU systems — confirmed same root cause, maintainer actively investigating"
        }
      ]
    },
    "versionAnalysis": {
      "mentionedVersions": [
        "3.116.0",
        "2.88.9"
      ],
      "workedIn": "2.88.9",
      "brokeIn": "3.116.0",
      "currentRelevance": "likely",
      "relevanceReason": "No fix has been released as of triage date. Maintainer confirmed the root cause (dynamic MSVC runtime linking) in #3136 and is working on a fix."
    },
    "regression": {
      "isRegression": true,
      "confidence": 0.92,
      "reason": "Reporter confirms 2.88.9 worked. In 3.x, SkiaSharp.Views.WinUI.Native was introduced as a new C++/WinRT component that depends on the MSVC runtime being installed, which is absent on clean machines.",
      "workedInVersion": "2.88.9",
      "brokeInVersion": "3.116.0"
    }
  },
  "analysis": {
    "summary": "The crash is caused by the native C++/WinRT component SkiaSharp.Views.WinUI.Native.dll failing to initialize on clean Windows machines because the MSVC runtime is not statically linked. The DLL fails COM class activation with REGDB_E_CLASSNOTREG when the VC++ redistributable is absent — identical root cause confirmed in related issue #3136.",
    "rationale": "The exception type, component name, and clean-machine-only context all match the confirmed root cause in #3136. Both issues exhibit TypeInitializationException on BufferExtensions with the same SkiaSharp version (3.116.0), same introduced C++/WinRT component, and the same workaround (install VC++ redistributable). The contextual difference (MSIX/Store certification vs Windows IoT) does not change the underlying defect. Maintainer @mattleibow stated in #3136: 'I think that I was building the interop and not statically linking the runtime.'",
    "keySignals": [
      {
        "text": "WinRT originate error - 0x80131534 : 'The type initializer for 'SkiaSharp.Views.WinUI.Native.BufferExtensions' threw an exception.'",
        "source": "issue body",
        "interpretation": "The native C++/WinRT component fails to initialize — COM activation fails at type initialization due to missing runtime dependency."
      },
      {
        "text": "works without issue locally...fails certification on a clean windows 11 machine",
        "source": "issue body",
        "interpretation": "VC++ redistributable is present on dev machine (installed by Visual Studio) but absent on the clean certification VM — confirms missing runtime is the trigger."
      },
      {
        "text": "Last Known Good Version: 2.88.9",
        "source": "issue form",
        "interpretation": "Regression introduced in 3.x when SkiaSharp.Views.WinUI.Native C++/WinRT component was added; 2.88.x did not have this component."
      }
    ],
    "codeInvestigation": [
      {
        "file": "native/winui/SkiaSharp.Views.WinUI.Native/SkiaSharp.Views.WinUI.Native/BufferExtensions.cpp",
        "finding": "BufferExtensions is a C++/WinRT implementation that exposes a WinRT activation factory for GetByteBuffer(IBuffer). If the MSVC runtime DLLs are not statically linked (i.e., /MD instead of /MT build flag), the component will fail to load on machines without VC++ redistributable installed, causing REGDB_E_CLASSNOTREG at COM activation.",
        "relevance": "direct"
      },
      {
        "file": "source/SkiaSharp.Views/SkiaSharp.Views.WinUI/UWPExtensions.cs",
        "lines": "176-180",
        "finding": "GetPixels(WriteableBitmap) calls buffer.GetByteBuffer() which calls into BufferExtensions.GetByteBuffer(buffer) — the C++/WinRT P/Invoke boundary. This is triggered during SKXamlCanvas.CreateBitmap() on first render, explaining why the crash occurs when the canvas first paints.",
        "relevance": "direct"
      }
    ],
    "workarounds": [
      "Install the MSVC runtime redistributable on affected machines: `winget install Microsoft.VCRedist.2015+.x64`",
      "Include the VC++ redistributable as a prerequisite in the MSIX installer/package manifest"
    ],
    "nextQuestions": [
      "Does including the VC++ redistributable as a package dependency in the MSIX manifest resolve the Store certification failure?",
      "Is the root cause precisely the same as #3136 or does the MSIX packaging add another layer?"
    ],
    "errorFingerprint": "TypeInitializationException:BufferExtensions:WinRT:REGDB_E_CLASSNOTREG",
    "resolution": {
      "hypothesis": "SkiaSharp.Views.WinUI.Native.dll is built with dynamically-linked MSVC runtime (/MD). On clean machines without the VC++ redistributable, the WinRT factory activation fails with REGDB_E_CLASSNOTREG at type initialization.",
      "proposals": [
        {
          "title": "Install VC++ Redistributable as immediate workaround",
          "description": "Install Microsoft.VCRedist.2015+.x64 on affected machines. Confirmed workaround from #3136 comments by community member.",
          "category": "workaround",
          "codeSnippet": "winget install Microsoft.VCRedist.2015+.x64",
          "confidence": 0.85,
          "effort": "cost/xs",
          "validated": "yes"
        },
        {
          "title": "Statically link MSVC runtime in native WinUI DLL build",
          "description": "Rebuild SkiaSharp.Views.WinUI.Native.dll with /MT (static runtime linking) so it carries no external MSVC runtime dependency. Maintainer confirmed this is the intended fix in #3136.",
          "category": "fix",
          "confidence": 0.87,
          "effort": "cost/s",
          "validated": "untested"
        }
      ],
      "recommendedProposal": "Statically link MSVC runtime in native WinUI DLL build",
      "recommendedReason": "Eliminates the root cause without requiring any action from end users, app distributors, or MSIX package manifests."
    }
  },
  "output": {
    "actionability": {
      "suggestedAction": "needs-investigation",
      "confidence": 0.87,
      "reason": "Root cause is confirmed (dynamic MSVC runtime linking in native WinUI DLL) via related issue #3136, but no fix has been released. Active maintainer investigation ongoing. Needs tracking alongside #3136.",
      "suggestedReproPlatform": "windows"
    },
    "actions": [
      {
        "type": "update-labels",
        "description": "Apply bug, SkiaSharp.Views, Windows-WinUI, MAUI partner, reliability tenet labels",
        "risk": "low",
        "confidence": 0.95,
        "labels": [
          "type/bug",
          "area/SkiaSharp.Views",
          "os/Windows-WinUI",
          "partner/maui",
          "tenet/reliability"
        ]
      },
      {
        "type": "link-related",
        "description": "Link to #3136 which has the same root cause and active maintainer investigation",
        "risk": "low",
        "confidence": 0.9,
        "linkedIssue": 3136
      },
      {
        "type": "add-comment",
        "description": "Post analysis with workaround and cross-reference to #3136",
        "risk": "medium",
        "confidence": 0.87,
        "comment": "Thanks for the detailed report. This crash is related to the same issue tracked in #3136 — the `SkiaSharp.Views.WinUI.Native.dll` C++/WinRT component was built without statically linking the MSVC runtime, so it fails to initialize on clean machines that don't have the Visual C++ Redistributable installed.\n\n**Immediate workaround:**\nInstall the VC++ redistributable on affected machines:\n```\nwinget install Microsoft.VCRedist.2015+.x64\n```\nOr include it as a prerequisite in your MSIX package manifest.\n\nA proper fix (statically linking the MSVC runtime in the native DLL build) is being investigated in #3136."
      }
    ]
  }
}
```

</details>
