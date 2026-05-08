# Issue Triage Report — #3440

| Field | Value |
|-------|-------|
| Repository | mono/SkiaSharp |
| Analyzed | 2026-04-30T01:08:00Z |
| Type | type/bug (0.95 (95%)) |
| Area | area/SkiaSharp.Views.Maui (0.88 (88%)) |
| Suggested action | needs-info (0.78 (78%)) |

**Issue Summary:** MAUI app using SkiaSharp 3.119.1 crashes on certain Windows systems with TypeInitializationException in SkiaSharp.Views.WinUI.Native.PropertySetExtensions when ANGLE GLES context creates a surface.

**Analysis:** The crash occurs when ANGLE's WinRT interop class PropertySetExtensions fails to initialize its COM activation factory on certain Windows systems. GlesContext.CreateSurface() unconditionally calls PropertySetExtensions.AddSingle() when a resolutionScale is provided (which AngleSwapChainPanel always does via CompositionScaleX). If the Windows system cannot locate the BaseActivationFactory for the WinRT type in SkiaSharp.Views.WinUI.Native, a TypeInitializationException is thrown. A similar issue (#2948) was seen in Microsoft Store sandbox and self-resolved via environment changes. The root cause — why the activation factory is missing on certain user systems — requires investigation into ANGLE deployment and Windows WinRT registration.

**Recommendations:** **needs-info** — Stack trace is clear but 'certain systems' condition is unexplained. Need to know what differentiates failing systems — particularly whether the SkiaSharp.Views.WinUI.Native native component is present in the deployed app. Similar issue #2948 was environment-specific.

---

## Classification

| Field | Value |
|-------|-------|
| Type | type/bug |
| Area | area/SkiaSharp.Views.Maui |
| Platforms | os/Windows-WinUI |
| Backends | — |
| Tenets | tenet/reliability |
| Partner | — |
| Current labels | type/bug, area/SkiaSharp.Views.Maui, os/Windows-WinUI, tenet/reliability |

## Evidence

### Reproduction

1. Create a .NET MAUI app using LiveCharts2 with SkiaSharp 3.119.1
2. Deploy the app to a Windows 11 system (Lenovo 83C4, Windows 11 26100.7462)
3. Navigate to a page containing a LiveCharts CartesianChart
4. Observe crash on page load with TypeInitializationException

**Environment:** SkiaSharp 3.119.1, Windows 11 10.0.26100.7462, Lenovo 83C4, MAUI with LiveCharts2

**Related issues:** #2948

**Repository links:**
- https://github.com/mono/SkiaSharp/issues/2948 — Similar PropertySetExtensions crash in Microsoft Store certification — closed as environment issue (Store adjusted their setup)

### Bug Signals

| Field | Value |
|-------|-------|
| Severity | high |
| Regression claimed | False |
| Error type | exception |
| Error message | TypeInitializationException: The type initializer for 'SkiaSharp.Views.WinUI.Native.PropertySetExtensions' threw an exception. Inner: COMException: Element not found. |
| Repro quality | partial |
| Target frameworks | net9.0-windows10.0.19041 |

**Stack trace:**

```text
at IObjectReference PropertySetExtensions.get__objRef_global__SkiaSharp_Views_WinUI_Native_IPropertySetExtensionsStatics()
  at void PropertySetExtensions.AddSingle(PropertySet propertySet, string key, float value)
  at void GlesContext.CreateSurface(SwapChainPanel panel, Size? renderSurfaceSize, float? resolutionScale)
  at void AngleSwapChainPanel.EnsureRenderSurface()
  at void AngleSwapChainPanel.OnLoaded(object sender, RoutedEventArgs e)
```

### Version Analysis

| Field | Value |
|-------|-------|
| Mentioned versions | 3.119.1 |
| Worked in | — |
| Broke in | — |
| Current relevance | likely |
| Relevance reason | GlesContext.CreateSurface with PropertySetExtensions.AddSingle call is present in current codebase with no mitigation for missing WinRT activation factory. |

## Analysis

### Technical Summary

The crash occurs when ANGLE's WinRT interop class PropertySetExtensions fails to initialize its COM activation factory on certain Windows systems. GlesContext.CreateSurface() unconditionally calls PropertySetExtensions.AddSingle() when a resolutionScale is provided (which AngleSwapChainPanel always does via CompositionScaleX). If the Windows system cannot locate the BaseActivationFactory for the WinRT type in SkiaSharp.Views.WinUI.Native, a TypeInitializationException is thrown. A similar issue (#2948) was seen in Microsoft Store sandbox and self-resolved via environment changes. The root cause — why the activation factory is missing on certain user systems — requires investigation into ANGLE deployment and Windows WinRT registration.

### Rationale

Clear exception stack trace from a real deployment. The failure is in ANGLE's WinRT EGL extension (PropertySetExtensions) used to pass surface creation properties. Related issue #2948 confirms this is a known failure mode. Since it only affects 'certain systems', the trigger condition is environment-specific but the code has no fallback path.

### Key Signals

- "COMException: Element not found at new BaseActivationFactory(string typeNamespace, string typeFullName)" — **issue body** (The WinRT activation factory for SkiaSharp.Views.WinUI.Native.PropertySetExtensions cannot be resolved — likely a missing or unregistered WinRT component on the target system.)
- "crashes on certain systems" — **issue body** (Not universal — system-dependent issue. Likely related to graphics driver, Windows feature availability, or deployment type.)
- "We have tried both with HA enabled and disabled but the result is the same" — **issue body** (Hardware acceleration toggle has no effect — the failure is earlier, at the ANGLE EGL initialization level.)

### Code Investigation

| File | Lines | Relevance | Finding |
|------|-------|-----------|---------|
| `source/SkiaSharp.Views/SkiaSharp.Views.WinUI/GlesInterop/GlesContext.cs` | 101-105 | direct | GlesContext.CreateSurface() calls PropertySetExtensions.AddSingle() when resolutionScale.HasValue is true. There is no try/catch or fallback if the WinRT activation factory fails to load. This means any system that cannot initialize SkiaSharp.Views.WinUI.Native.PropertySetExtensions will crash here. |
| `source/SkiaSharp.Views/SkiaSharp.Views.WinUI/AngleSwapChainPanel.cs` | 191-206 | direct | EnsureRenderSurface() always passes CompositionScaleX (a non-null float) as the resolutionScale parameter to GlesContext.CreateSurface(). This guarantees the PropertySetExtensions.AddSingle() path is always taken for WinUI, making the crash deterministic when the activation factory is unavailable. |

### Next Questions

- What distinguishes 'certain systems' from those that work? (graphics driver version, Windows edition, deployment type — sideloaded vs Store)
- Is the SkiaSharp.Views.WinUI.Native.winmd/dll properly deployed alongside the app on failing systems?
- Does the crash occur on a clean Windows install with no special restrictions?
- Is there a way to catch the TypeInitializationException and fall back to a non-GLES rendering path?

### Resolution Proposals

**Hypothesis:** The ANGLE WinRT activation factory for PropertySetExtensions is unavailable on certain Windows configurations, and there is no error handling around this initialization path.

1. **Add try/catch around PropertySetExtensions calls in GlesContext** — workaround, confidence 0.65 (65%), cost/s, validated=untested
   - Wrap the PropertySetExtensions.AddSize and AddSingle calls in try/catch. If the activation factory fails, skip the optional properties and attempt surface creation without them. This degrades gracefully on systems where the WinRT component is unavailable.
2. **Investigate and document deployment requirements for ANGLE WinRT** — investigation, confidence 0.80 (80%), cost/m, validated=untested
   - Determine what Windows feature or driver is required for the PropertySetExtensions WinRT component. Document as known limitation and add diagnostic message when activation fails.

**Recommended proposal:** Investigate and document deployment requirements for ANGLE WinRT

**Why:** The root cause (why the activation factory is missing) must be understood before applying a code fix, to avoid hiding a deployment problem.

## Recommendations

### Actionability

| Field | Value |
|-------|-------|
| Suggested action | needs-info |
| Confidence | 0.78 (78%) |
| Reason | Stack trace is clear but 'certain systems' condition is unexplained. Need to know what differentiates failing systems — particularly whether the SkiaSharp.Views.WinUI.Native native component is present in the deployed app. Similar issue #2948 was environment-specific. |
| Suggested repro platform | windows |

### Missing Info

- What differentiates the failing systems from working ones? (hardware, drivers, Windows edition, deployment type)
- Is the app sideloaded, deployed via MSIX, or running from IDE?
- Does the crash occur on a fresh Windows install without any special restrictions?
- Can you check if SkiaSharp.Views.WinUI.Native.dll is present in the deployed app folder on the failing system?

### Automatable Actions

| Type | Risk | Confidence | Description | Details |
|------|------|------------|-------------|---------|
| update-labels | low | 0.95 (95%) | Apply type/bug, area/SkiaSharp.Views.Maui, os/Windows-WinUI, tenet/reliability labels | labels=type/bug, area/SkiaSharp.Views.Maui, os/Windows-WinUI, tenet/reliability |
| add-comment | medium | 0.78 (78%) | Ask for more system details to identify what causes the activation factory failure on certain systems | — |

**Comment draft for `add-comment`:**

```markdown
Thanks for the detailed stack trace. The crash is in ANGLE's WinRT interop initialization (`PropertySetExtensions`) which is used to configure the OpenGL ES surface resolution scale on Windows.

This failure (`COMException: Element not found`) means the WinRT activation factory for the native ANGLE component cannot be found on those specific systems. A similar issue was reported in #2948 in a Microsoft Store sandbox environment and resolved by environment changes.

To help investigate:
1. Does this happen on **all** systems or specific ones? What's different about the failing machines (OS edition, drivers, deployment method)?
2. How is the app deployed on the failing systems — MSIX, sideloaded, or running directly from the IDE?
3. Does `SkiaSharp.Views.WinUI.Native.dll` exist in the deployed app's folder on the failing machine?
4. Can you try wrapping your chart initialization in a try/catch to see if a graceful fallback is possible?

In the meantime, you can try setting the `EnableRenderLoop` or checking if disabling ANGLE rendering in LiveCharts2's SkiaSharp provider helps.
```

<details>
<summary>Raw JSON</summary>

```json
{
  "meta": {
    "schemaVersion": "1.0",
    "number": 3440,
    "repo": "mono/SkiaSharp",
    "analyzedAt": "2026-04-30T01:08:00Z",
    "currentLabels": [
      "type/bug",
      "area/SkiaSharp.Views.Maui",
      "os/Windows-WinUI",
      "tenet/reliability"
    ]
  },
  "summary": "MAUI app using SkiaSharp 3.119.1 crashes on certain Windows systems with TypeInitializationException in SkiaSharp.Views.WinUI.Native.PropertySetExtensions when ANGLE GLES context creates a surface.",
  "classification": {
    "type": {
      "value": "type/bug",
      "confidence": 0.95
    },
    "area": {
      "value": "area/SkiaSharp.Views.Maui",
      "confidence": 0.88
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
      "errorMessage": "TypeInitializationException: The type initializer for 'SkiaSharp.Views.WinUI.Native.PropertySetExtensions' threw an exception. Inner: COMException: Element not found.",
      "stackTrace": "at IObjectReference PropertySetExtensions.get__objRef_global__SkiaSharp_Views_WinUI_Native_IPropertySetExtensionsStatics()\n  at void PropertySetExtensions.AddSingle(PropertySet propertySet, string key, float value)\n  at void GlesContext.CreateSurface(SwapChainPanel panel, Size? renderSurfaceSize, float? resolutionScale)\n  at void AngleSwapChainPanel.EnsureRenderSurface()\n  at void AngleSwapChainPanel.OnLoaded(object sender, RoutedEventArgs e)",
      "reproQuality": "partial",
      "targetFrameworks": [
        "net9.0-windows10.0.19041"
      ]
    },
    "reproEvidence": {
      "stepsToReproduce": [
        "Create a .NET MAUI app using LiveCharts2 with SkiaSharp 3.119.1",
        "Deploy the app to a Windows 11 system (Lenovo 83C4, Windows 11 26100.7462)",
        "Navigate to a page containing a LiveCharts CartesianChart",
        "Observe crash on page load with TypeInitializationException"
      ],
      "environmentDetails": "SkiaSharp 3.119.1, Windows 11 10.0.26100.7462, Lenovo 83C4, MAUI with LiveCharts2",
      "relatedIssues": [
        2948
      ],
      "repoLinks": [
        {
          "url": "https://github.com/mono/SkiaSharp/issues/2948",
          "description": "Similar PropertySetExtensions crash in Microsoft Store certification — closed as environment issue (Store adjusted their setup)"
        }
      ]
    },
    "versionAnalysis": {
      "mentionedVersions": [
        "3.119.1"
      ],
      "currentRelevance": "likely",
      "relevanceReason": "GlesContext.CreateSurface with PropertySetExtensions.AddSingle call is present in current codebase with no mitigation for missing WinRT activation factory."
    }
  },
  "analysis": {
    "summary": "The crash occurs when ANGLE's WinRT interop class PropertySetExtensions fails to initialize its COM activation factory on certain Windows systems. GlesContext.CreateSurface() unconditionally calls PropertySetExtensions.AddSingle() when a resolutionScale is provided (which AngleSwapChainPanel always does via CompositionScaleX). If the Windows system cannot locate the BaseActivationFactory for the WinRT type in SkiaSharp.Views.WinUI.Native, a TypeInitializationException is thrown. A similar issue (#2948) was seen in Microsoft Store sandbox and self-resolved via environment changes. The root cause — why the activation factory is missing on certain user systems — requires investigation into ANGLE deployment and Windows WinRT registration.",
    "rationale": "Clear exception stack trace from a real deployment. The failure is in ANGLE's WinRT EGL extension (PropertySetExtensions) used to pass surface creation properties. Related issue #2948 confirms this is a known failure mode. Since it only affects 'certain systems', the trigger condition is environment-specific but the code has no fallback path.",
    "keySignals": [
      {
        "text": "COMException: Element not found at new BaseActivationFactory(string typeNamespace, string typeFullName)",
        "source": "issue body",
        "interpretation": "The WinRT activation factory for SkiaSharp.Views.WinUI.Native.PropertySetExtensions cannot be resolved — likely a missing or unregistered WinRT component on the target system."
      },
      {
        "text": "crashes on certain systems",
        "source": "issue body",
        "interpretation": "Not universal — system-dependent issue. Likely related to graphics driver, Windows feature availability, or deployment type."
      },
      {
        "text": "We have tried both with HA enabled and disabled but the result is the same",
        "source": "issue body",
        "interpretation": "Hardware acceleration toggle has no effect — the failure is earlier, at the ANGLE EGL initialization level."
      }
    ],
    "codeInvestigation": [
      {
        "file": "source/SkiaSharp.Views/SkiaSharp.Views.WinUI/GlesInterop/GlesContext.cs",
        "lines": "101-105",
        "finding": "GlesContext.CreateSurface() calls PropertySetExtensions.AddSingle() when resolutionScale.HasValue is true. There is no try/catch or fallback if the WinRT activation factory fails to load. This means any system that cannot initialize SkiaSharp.Views.WinUI.Native.PropertySetExtensions will crash here.",
        "relevance": "direct"
      },
      {
        "file": "source/SkiaSharp.Views/SkiaSharp.Views.WinUI/AngleSwapChainPanel.cs",
        "lines": "191-206",
        "finding": "EnsureRenderSurface() always passes CompositionScaleX (a non-null float) as the resolutionScale parameter to GlesContext.CreateSurface(). This guarantees the PropertySetExtensions.AddSingle() path is always taken for WinUI, making the crash deterministic when the activation factory is unavailable.",
        "relevance": "direct"
      }
    ],
    "nextQuestions": [
      "What distinguishes 'certain systems' from those that work? (graphics driver version, Windows edition, deployment type — sideloaded vs Store)",
      "Is the SkiaSharp.Views.WinUI.Native.winmd/dll properly deployed alongside the app on failing systems?",
      "Does the crash occur on a clean Windows install with no special restrictions?",
      "Is there a way to catch the TypeInitializationException and fall back to a non-GLES rendering path?"
    ],
    "resolution": {
      "hypothesis": "The ANGLE WinRT activation factory for PropertySetExtensions is unavailable on certain Windows configurations, and there is no error handling around this initialization path.",
      "proposals": [
        {
          "title": "Add try/catch around PropertySetExtensions calls in GlesContext",
          "description": "Wrap the PropertySetExtensions.AddSize and AddSingle calls in try/catch. If the activation factory fails, skip the optional properties and attempt surface creation without them. This degrades gracefully on systems where the WinRT component is unavailable.",
          "category": "workaround",
          "confidence": 0.65,
          "effort": "cost/s",
          "validated": "untested"
        },
        {
          "title": "Investigate and document deployment requirements for ANGLE WinRT",
          "description": "Determine what Windows feature or driver is required for the PropertySetExtensions WinRT component. Document as known limitation and add diagnostic message when activation fails.",
          "category": "investigation",
          "confidence": 0.8,
          "effort": "cost/m",
          "validated": "untested"
        }
      ],
      "recommendedProposal": "Investigate and document deployment requirements for ANGLE WinRT",
      "recommendedReason": "The root cause (why the activation factory is missing) must be understood before applying a code fix, to avoid hiding a deployment problem."
    }
  },
  "output": {
    "actionability": {
      "suggestedAction": "needs-info",
      "confidence": 0.78,
      "reason": "Stack trace is clear but 'certain systems' condition is unexplained. Need to know what differentiates failing systems — particularly whether the SkiaSharp.Views.WinUI.Native native component is present in the deployed app. Similar issue #2948 was environment-specific.",
      "suggestedReproPlatform": "windows"
    },
    "missingInfo": [
      "What differentiates the failing systems from working ones? (hardware, drivers, Windows edition, deployment type)",
      "Is the app sideloaded, deployed via MSIX, or running from IDE?",
      "Does the crash occur on a fresh Windows install without any special restrictions?",
      "Can you check if SkiaSharp.Views.WinUI.Native.dll is present in the deployed app folder on the failing system?"
    ],
    "actions": [
      {
        "type": "update-labels",
        "description": "Apply type/bug, area/SkiaSharp.Views.Maui, os/Windows-WinUI, tenet/reliability labels",
        "risk": "low",
        "confidence": 0.95,
        "labels": [
          "type/bug",
          "area/SkiaSharp.Views.Maui",
          "os/Windows-WinUI",
          "tenet/reliability"
        ]
      },
      {
        "type": "add-comment",
        "description": "Ask for more system details to identify what causes the activation factory failure on certain systems",
        "risk": "medium",
        "confidence": 0.78,
        "comment": "Thanks for the detailed stack trace. The crash is in ANGLE's WinRT interop initialization (`PropertySetExtensions`) which is used to configure the OpenGL ES surface resolution scale on Windows.\n\nThis failure (`COMException: Element not found`) means the WinRT activation factory for the native ANGLE component cannot be found on those specific systems. A similar issue was reported in #2948 in a Microsoft Store sandbox environment and resolved by environment changes.\n\nTo help investigate:\n1. Does this happen on **all** systems or specific ones? What's different about the failing machines (OS edition, drivers, deployment method)?\n2. How is the app deployed on the failing systems — MSIX, sideloaded, or running directly from the IDE?\n3. Does `SkiaSharp.Views.WinUI.Native.dll` exist in the deployed app's folder on the failing machine?\n4. Can you try wrapping your chart initialization in a try/catch to see if a graceful fallback is possible?\n\nIn the meantime, you can try setting the `EnableRenderLoop` or checking if disabling ANGLE rendering in LiveCharts2's SkiaSharp provider helps."
      }
    ]
  }
}
```

</details>
