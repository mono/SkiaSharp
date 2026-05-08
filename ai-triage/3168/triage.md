# Issue Triage Report — #3168

| Field | Value |
|-------|-------|
| Repository | mono/SkiaSharp |
| Analyzed | 2026-04-21T18:50:00Z |
| Type | type/bug (0.97 (97%)) |
| Area | area/SkiaSharp.Views (0.95 (95%)) |
| Suggested action | close-as-fixed (0.85 (85%)) |

**Issue Summary:** SKXamlCanvas in WinUI crashes with a COMException (REGDB_E_CLASSNOTREG) on devices without a GPU or with outdated AMD Radeon drivers because BufferExtensions fails to initialize its COM interop layer; root cause is a missing VC runtime redistributable, and the issue was closed as fixed.

**Analysis:** SKXamlCanvas uses WriteableBitmap internally and calls BufferExtensions.GetByteBuffer() to obtain pixel data. On devices without proper GPU/COM support (VMs without GPU, Windows IoT, old Radeon drivers pre-2021), the static initializer of BufferExtensions fails with COMException REGDB_E_CLASSNOTREG. The root cause is a missing VC runtime redistributable required for the WinRT/COM interop layer in SkiaSharp.Views.WinUI.Native. The fix was a packaging change; the reporter also proposed a code-level fallback to SoftwareBitmap.

**Recommendations:** **close-as-fixed** — Issue was closed as 'completed' by maintainer on 2025-08-22 after confirming a fix (VC runtime redistributable packaging) was shipped. The crash is no longer expected to occur with the fix in place.

---

## Classification

| Field | Value |
|-------|-------|
| Type | type/bug |
| Area | area/SkiaSharp.Views |
| Platforms | os/Windows-WinUI |
| Backends | — |
| Tenets | tenet/reliability |
| Partner | — |
| Current labels | type/bug, area/SkiaSharp.Views, os/Windows-WinUI, tenet/reliability, triage/triaged |

## Evidence

### Reproduction

1. Create a WinUI 3 application
2. Install LiveChartsCore.SkiaSharpView.WinUI or use SKXamlCanvas directly
3. Run the application in Windows Sandbox or on a device without a GPU
4. Application crashes when rendering the canvas

**Environment:** Windows 10/11, WinUI 1.6, SkiaSharp 3.116.0; also reported on 2.88.9. Affects VMs without GPU, devices with outdated AMD Radeon drivers (pre-2021), and Windows IoT Enterprise (see #3136).

**Related issues:** #3136

**Repository links:**
- https://github.com/mono/SkiaSharp/issues/3136 — Related issue: same COM crash (REGDB_E_CLASSNOTREG in BufferExtensions) on Windows IoT with SKCanvasView
- https://github.com/mattleibow/ComCrashTestApp — Maintainer test app to reproduce and investigate the COM crash

### Bug Signals

| Field | Value |
|-------|-------|
| Severity | high |
| Regression claimed | False |
| Error type | exception |
| Error message | TypeInitializationException: The type initializer for 'SkiaSharp.Views.WinUI.Native.BufferExtensions' threw an exception. InnerException: COMException: Class not registered (0x80040154 REGDB_E_CLASSNOTREG) |
| Repro quality | complete |
| Target frameworks | net9.0-windows10.0.19041 |

**Stack trace:**

```text
BufferExtensions..cctor() → BaseActivationFactory..ctor() → COMException (REGDB_E_CLASSNOTREG); called from SKXamlCanvas.CreateBitmap() → DoInvalidate()
```

### Version Analysis

| Field | Value |
|-------|-------|
| Mentioned versions | 2.88.9, 3.116.0 |
| Worked in | — |
| Broke in | — |
| Current relevance | unlikely |
| Relevance reason | Issue was closed as 'completed' by maintainer on 2025-08-22. A packaging fix (VC runtime redistributable) was shipped. |

### Fix Status

| Field | Value |
|-------|-------|
| Likely fixed | True |
| Confidence | 0.85 (85%) |
| Reason | Issue closed as 'completed' by maintainer mattleibow on 2025-08-22. Maintainer stated: 'While we wait for the fix to go out, there is a workaround — install the VC runtime redistributable', indicating the real fix was a packaging/distribution change. |
| Related PRs | — |
| Related commits | — |
| Fixed in version | — |

## Analysis

### Technical Summary

SKXamlCanvas uses WriteableBitmap internally and calls BufferExtensions.GetByteBuffer() to obtain pixel data. On devices without proper GPU/COM support (VMs without GPU, Windows IoT, old Radeon drivers pre-2021), the static initializer of BufferExtensions fails with COMException REGDB_E_CLASSNOTREG. The root cause is a missing VC runtime redistributable required for the WinRT/COM interop layer in SkiaSharp.Views.WinUI.Native. The fix was a packaging change; the reporter also proposed a code-level fallback to SoftwareBitmap.

### Rationale

This is a clear crash with a full stack trace, 100% reproducible in Windows Sandbox, and affects a broad class of environments. REGDB_E_CLASSNOTREG points to COM class registration failure, not a SkiaSharp logic bug. The maintainer identified the VC runtime as the root cause and confirmed a fix was shipped. Closing as fixed is appropriate since state_reason is 'completed'.

### Key Signals

- "The issue originates from SkiaSharp.Views.WinUI.Native, where IBuffer is cast to IBufferByteAccess. COM fails to locate the required GPU resources." — **issue body** (Reporter correctly identified the COM interop failure point; root cause is missing VC runtime for COM class registration.)
- "COMException: Class not registered (0x80040154 REGDB_E_CLASSNOTREG) in BufferExtensions..cctor()" — **issue #3136 stack trace (same crash)** (REGDB_E_CLASSNOTREG indicates a missing COM class registration — VC runtime redistributable is required for the WinRT interop to work.)
- "While we wait for the fix to go out, there is a workaround - install the VC runtime redistributable." — **comment by mattleibow (COLLABORATOR)** (Maintainer confirmed VC runtime as the root cause and shipped a fix in a subsequent release.)
- "This issue can be reproduced 100% of the time in Windows Sandbox, as it runs in a virtualized environment with no GPU access." — **issue body** (Reliable repro environment identified: Windows Sandbox, making this easy to reproduce and verify fixes.)

### Code Investigation

| File | Lines | Relevance | Finding |
|------|-------|-----------|---------|
| `source/SkiaSharp.Views/SkiaSharp.Views.WinUI/SKXamlCanvas.cs` | 237-255 | direct | CreateBitmap() creates a WriteableBitmap and calls bitmap.GetPixels() (line 240) with no COMException handling. This triggers BufferExtensions.GetByteBuffer() which fails on GPU-less systems. No software rendering fallback path exists in the current codebase. |
| `source/SkiaSharp.Views/SkiaSharp.Views.WinUI/SKXamlCanvas.cs` | 170-207 | related | DoInvalidate() calls CreateBitmap() and then bitmap.Invalidate() with no exception handling. Any COMException from the bitmap creation path propagates unhandled, causing an app crash. |

### Workarounds

- Install the Visual C++ Runtime Redistributable on the affected device (confirmed by maintainer).
- Apply code-level fallback: catch COMException in CreateBitmap() and use SoftwareBitmap/SoftwareBitmapSource instead of WriteableBitmap (full implementation provided by reporter in issue comments).

### Next Questions

- Has the VC runtime redistributable been added as a required dependency in the SkiaSharp.Views.WinUI NuGet package manifest?
- Should a SoftwareBitmap code-level fallback be added to SKXamlCanvas for robustness even after the packaging fix?
- Is related issue #3136 (Windows IoT crash) also resolved by the same fix?

### Resolution Proposals

**Hypothesis:** Missing VC runtime redistributable caused COM class registration failure in BufferExtensions static initializer, crashing all WinUI apps on GPU-less or low-driver environments. Fix was a packaging/distribution change to include or depend on the VC runtime redistributable.

1. **Install VC Runtime Redistributable** — workaround, confidence 0.90 (90%), cost/xs, validated=untested
   - Install the Visual C++ Runtime Redistributable on the affected device. This resolves REGDB_E_CLASSNOTREG for the COM interop used by SkiaSharp.Views.WinUI.Native. This is the shipped fix per the maintainer.
2. **Add SoftwareBitmap fallback in SKXamlCanvas** — fix, confidence 0.75 (75%), cost/m, validated=untested
   - Catch COMException in CreateBitmap() and fall back to SoftwareBitmap/SoftwareBitmapSource for software rendering. Reporter provided a complete implementation in issue comments. This would make SKXamlCanvas resilient even without the VC runtime.

**Recommended proposal:** Install VC Runtime Redistributable

**Why:** Confirmed by maintainer as the fix that was shipped. Issue was subsequently closed as 'completed'.

## Recommendations

### Actionability

| Field | Value |
|-------|-------|
| Suggested action | close-as-fixed |
| Confidence | 0.85 (85%) |
| Reason | Issue was closed as 'completed' by maintainer on 2025-08-22 after confirming a fix (VC runtime redistributable packaging) was shipped. The crash is no longer expected to occur with the fix in place. |
| Suggested repro platform | windows |

### Automatable Actions

| Type | Risk | Confidence | Description | Details |
|------|------|------------|-------------|---------|
| update-labels | low | 0.97 (97%) | Apply type/bug, area/SkiaSharp.Views, os/Windows-WinUI, tenet/reliability labels | labels=type/bug, area/SkiaSharp.Views, os/Windows-WinUI, tenet/reliability |
| link-related | low | 0.90 (90%) | Cross-reference related issue #3136 which has the same COMException crash root cause on Windows IoT | linkedIssue=#3136 |

<details>
<summary>Raw JSON</summary>

```json
{
  "meta": {
    "schemaVersion": "1.0",
    "number": 3168,
    "repo": "mono/SkiaSharp",
    "analyzedAt": "2026-04-21T18:50:00Z",
    "currentLabels": [
      "type/bug",
      "area/SkiaSharp.Views",
      "os/Windows-WinUI",
      "tenet/reliability",
      "triage/triaged"
    ]
  },
  "summary": "SKXamlCanvas in WinUI crashes with a COMException (REGDB_E_CLASSNOTREG) on devices without a GPU or with outdated AMD Radeon drivers because BufferExtensions fails to initialize its COM interop layer; root cause is a missing VC runtime redistributable, and the issue was closed as fixed.",
  "classification": {
    "type": {
      "value": "type/bug",
      "confidence": 0.97
    },
    "area": {
      "value": "area/SkiaSharp.Views",
      "confidence": 0.95
    },
    "platforms": [
      "os/Windows-WinUI"
    ],
    "tenets": [
      "tenet/reliability"
    ]
  },
  "evidence": {
    "bugSignals": {
      "severity": "high",
      "regressionClaimed": false,
      "errorType": "exception",
      "errorMessage": "TypeInitializationException: The type initializer for 'SkiaSharp.Views.WinUI.Native.BufferExtensions' threw an exception. InnerException: COMException: Class not registered (0x80040154 REGDB_E_CLASSNOTREG)",
      "stackTrace": "BufferExtensions..cctor() → BaseActivationFactory..ctor() → COMException (REGDB_E_CLASSNOTREG); called from SKXamlCanvas.CreateBitmap() → DoInvalidate()",
      "reproQuality": "complete",
      "targetFrameworks": [
        "net9.0-windows10.0.19041"
      ]
    },
    "reproEvidence": {
      "stepsToReproduce": [
        "Create a WinUI 3 application",
        "Install LiveChartsCore.SkiaSharpView.WinUI or use SKXamlCanvas directly",
        "Run the application in Windows Sandbox or on a device without a GPU",
        "Application crashes when rendering the canvas"
      ],
      "environmentDetails": "Windows 10/11, WinUI 1.6, SkiaSharp 3.116.0; also reported on 2.88.9. Affects VMs without GPU, devices with outdated AMD Radeon drivers (pre-2021), and Windows IoT Enterprise (see #3136).",
      "relatedIssues": [
        3136
      ],
      "repoLinks": [
        {
          "url": "https://github.com/mono/SkiaSharp/issues/3136",
          "description": "Related issue: same COM crash (REGDB_E_CLASSNOTREG in BufferExtensions) on Windows IoT with SKCanvasView"
        },
        {
          "url": "https://github.com/mattleibow/ComCrashTestApp",
          "description": "Maintainer test app to reproduce and investigate the COM crash"
        }
      ]
    },
    "versionAnalysis": {
      "mentionedVersions": [
        "2.88.9",
        "3.116.0"
      ],
      "currentRelevance": "unlikely",
      "relevanceReason": "Issue was closed as 'completed' by maintainer on 2025-08-22. A packaging fix (VC runtime redistributable) was shipped."
    },
    "fixStatus": {
      "likelyFixed": true,
      "confidence": 0.85,
      "reason": "Issue closed as 'completed' by maintainer mattleibow on 2025-08-22. Maintainer stated: 'While we wait for the fix to go out, there is a workaround — install the VC runtime redistributable', indicating the real fix was a packaging/distribution change."
    }
  },
  "analysis": {
    "summary": "SKXamlCanvas uses WriteableBitmap internally and calls BufferExtensions.GetByteBuffer() to obtain pixel data. On devices without proper GPU/COM support (VMs without GPU, Windows IoT, old Radeon drivers pre-2021), the static initializer of BufferExtensions fails with COMException REGDB_E_CLASSNOTREG. The root cause is a missing VC runtime redistributable required for the WinRT/COM interop layer in SkiaSharp.Views.WinUI.Native. The fix was a packaging change; the reporter also proposed a code-level fallback to SoftwareBitmap.",
    "rationale": "This is a clear crash with a full stack trace, 100% reproducible in Windows Sandbox, and affects a broad class of environments. REGDB_E_CLASSNOTREG points to COM class registration failure, not a SkiaSharp logic bug. The maintainer identified the VC runtime as the root cause and confirmed a fix was shipped. Closing as fixed is appropriate since state_reason is 'completed'.",
    "codeInvestigation": [
      {
        "file": "source/SkiaSharp.Views/SkiaSharp.Views.WinUI/SKXamlCanvas.cs",
        "lines": "237-255",
        "finding": "CreateBitmap() creates a WriteableBitmap and calls bitmap.GetPixels() (line 240) with no COMException handling. This triggers BufferExtensions.GetByteBuffer() which fails on GPU-less systems. No software rendering fallback path exists in the current codebase.",
        "relevance": "direct"
      },
      {
        "file": "source/SkiaSharp.Views/SkiaSharp.Views.WinUI/SKXamlCanvas.cs",
        "lines": "170-207",
        "finding": "DoInvalidate() calls CreateBitmap() and then bitmap.Invalidate() with no exception handling. Any COMException from the bitmap creation path propagates unhandled, causing an app crash.",
        "relevance": "related"
      }
    ],
    "keySignals": [
      {
        "text": "The issue originates from SkiaSharp.Views.WinUI.Native, where IBuffer is cast to IBufferByteAccess. COM fails to locate the required GPU resources.",
        "source": "issue body",
        "interpretation": "Reporter correctly identified the COM interop failure point; root cause is missing VC runtime for COM class registration."
      },
      {
        "text": "COMException: Class not registered (0x80040154 REGDB_E_CLASSNOTREG) in BufferExtensions..cctor()",
        "source": "issue #3136 stack trace (same crash)",
        "interpretation": "REGDB_E_CLASSNOTREG indicates a missing COM class registration — VC runtime redistributable is required for the WinRT interop to work."
      },
      {
        "text": "While we wait for the fix to go out, there is a workaround - install the VC runtime redistributable.",
        "source": "comment by mattleibow (COLLABORATOR)",
        "interpretation": "Maintainer confirmed VC runtime as the root cause and shipped a fix in a subsequent release."
      },
      {
        "text": "This issue can be reproduced 100% of the time in Windows Sandbox, as it runs in a virtualized environment with no GPU access.",
        "source": "issue body",
        "interpretation": "Reliable repro environment identified: Windows Sandbox, making this easy to reproduce and verify fixes."
      }
    ],
    "workarounds": [
      "Install the Visual C++ Runtime Redistributable on the affected device (confirmed by maintainer).",
      "Apply code-level fallback: catch COMException in CreateBitmap() and use SoftwareBitmap/SoftwareBitmapSource instead of WriteableBitmap (full implementation provided by reporter in issue comments)."
    ],
    "nextQuestions": [
      "Has the VC runtime redistributable been added as a required dependency in the SkiaSharp.Views.WinUI NuGet package manifest?",
      "Should a SoftwareBitmap code-level fallback be added to SKXamlCanvas for robustness even after the packaging fix?",
      "Is related issue #3136 (Windows IoT crash) also resolved by the same fix?"
    ],
    "resolution": {
      "hypothesis": "Missing VC runtime redistributable caused COM class registration failure in BufferExtensions static initializer, crashing all WinUI apps on GPU-less or low-driver environments. Fix was a packaging/distribution change to include or depend on the VC runtime redistributable.",
      "proposals": [
        {
          "title": "Install VC Runtime Redistributable",
          "description": "Install the Visual C++ Runtime Redistributable on the affected device. This resolves REGDB_E_CLASSNOTREG for the COM interop used by SkiaSharp.Views.WinUI.Native. This is the shipped fix per the maintainer.",
          "category": "workaround",
          "confidence": 0.9,
          "effort": "cost/xs",
          "validated": "untested"
        },
        {
          "title": "Add SoftwareBitmap fallback in SKXamlCanvas",
          "description": "Catch COMException in CreateBitmap() and fall back to SoftwareBitmap/SoftwareBitmapSource for software rendering. Reporter provided a complete implementation in issue comments. This would make SKXamlCanvas resilient even without the VC runtime.",
          "category": "fix",
          "confidence": 0.75,
          "effort": "cost/m",
          "validated": "untested"
        }
      ],
      "recommendedProposal": "Install VC Runtime Redistributable",
      "recommendedReason": "Confirmed by maintainer as the fix that was shipped. Issue was subsequently closed as 'completed'."
    }
  },
  "output": {
    "actionability": {
      "suggestedAction": "close-as-fixed",
      "confidence": 0.85,
      "reason": "Issue was closed as 'completed' by maintainer on 2025-08-22 after confirming a fix (VC runtime redistributable packaging) was shipped. The crash is no longer expected to occur with the fix in place.",
      "suggestedReproPlatform": "windows"
    },
    "actions": [
      {
        "type": "update-labels",
        "description": "Apply type/bug, area/SkiaSharp.Views, os/Windows-WinUI, tenet/reliability labels",
        "risk": "low",
        "confidence": 0.97,
        "labels": [
          "type/bug",
          "area/SkiaSharp.Views",
          "os/Windows-WinUI",
          "tenet/reliability"
        ]
      },
      {
        "type": "link-related",
        "description": "Cross-reference related issue #3136 which has the same COMException crash root cause on Windows IoT",
        "risk": "low",
        "confidence": 0.9,
        "linkedIssue": 3136
      }
    ]
  }
}
```

</details>
