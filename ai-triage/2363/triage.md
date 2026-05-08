# Issue Triage Report — #2363

| Field | Value |
|-------|-------|
| Repository | mono/SkiaSharp |
| Analyzed | 2026-05-01T23:32:00Z |
| Type | type/bug (0.82 (82%)) |
| Area | area/SkiaSharp.Views.Uno (0.95 (95%)) |
| Suggested action | needs-info (0.88 (88%)) |

**Issue Summary:** SKSwapChainPanel (hardware/WebGL rendering) does not work in SkiaSharp.Views.Uno on WASM, while software rendering works fine.

**Analysis:** Reporter states that SKSwapChainPanel (hardware WebGL rendering) breaks the WASM app in Uno Platform while removing it (falling back to software rendering) restores functionality. The issue lacks version numbers, error messages, and stack traces, making root-cause analysis impossible without more information.

**Recommendations:** **needs-info** — The issue body contains no version numbers, no error messages, no stack traces, and no reproduction steps. The only information is that removing SKSwapChainPanel fixes the problem. More info is needed before investigation can proceed.

---

## Classification

| Field | Value |
|-------|-------|
| Type | type/bug |
| Area | area/SkiaSharp.Views.Uno |
| Platforms | os/WASM |
| Backends | backend/OpenGL |
| Tenets | — |
| Partner | partner/unoplatform |

## Evidence

### Reproduction

### Bug Signals

| Field | Value |
|-------|-------|
| Severity | medium |
| Regression claimed | False |
| Error type | platform-specific |
| Error message | — |
| Repro quality | partial |
| Target frameworks | — |

## Analysis

### Technical Summary

Reporter states that SKSwapChainPanel (hardware WebGL rendering) breaks the WASM app in Uno Platform while removing it (falling back to software rendering) restores functionality. The issue lacks version numbers, error messages, and stack traces, making root-cause analysis impossible without more information.

### Rationale

The reporter describes a functional regression limited to SKSwapChainPanel on WASM/Uno. Code inspection shows SKSwapChainPanel.Wasm.cs relies on WebGL context creation through JSImport; any failure there would silently break rendering. However, with no version numbers, no error messages, and no stack traces, it is impossible to determine whether this is a SkiaSharp bug, an Uno Platform incompatibility, or a WebGL environment issue. Needs-info is appropriate.

### Key Signals

- "As soon I remove sk:SKSwapChainPanel.. from MainPage.xaml the sample is working. So only Software rendering." — **issue body** (Hardware path (WebGL via SKSwapChainPanel) fails; software fallback works.)

### Code Investigation

| File | Lines | Relevance | Finding |
|------|-------|-----------|---------|
| `source/SkiaSharp.Views.Uno/SkiaSharp.Views.Uno.WinUI.Wasm/SKSwapChainPanel.Wasm.cs` | 99-155 | direct | SKSwapChainPanel on WASM creates a WebGL context via JSImport/JSInterop and uses GRGlInterface.Create() to initialise GPU-backed SkiaSharp rendering. If WebGL context creation fails or jsInfo.IsValid is false, RenderFrame() silently returns without rendering. |
| `source/SkiaSharp.Views.Uno/SkiaSharp.Views.Uno.WinUI.Wasm/SKSwapChainPanel.Wasm.cs` | 62-66 | direct | DoLoaded() calls NativeMethods.CreateContext which uses JSImport to call globalThis.SkiaSharp.Views.Windows.SKSwapChainPanel.createContextStatic. If this JS function is missing or errors out, jsInfo will not be populated correctly and IsValid will remain false. |

### Next Questions

- Which version of SkiaSharp.Views.Uno is being used?
- Which version of Uno Platform is being used?
- What browser/browser version is being tested?
- Are there any console errors in the browser developer tools?
- Does this fail on all browsers or specific ones (Chrome, Firefox, Safari)?
- Does the SkiaSharpTest.Wasm sample from the Uno.Samples repo reproduce the issue out of the box?

### Resolution Proposals

1. **Request missing version and error information** — investigation, cost/xs, validated=untested
   - Ask the reporter to provide: SkiaSharp.Views.Uno version, Uno Platform version, browser console errors, and whether the issue is browser-specific.

**Recommended proposal:** 0

**Why:** The issue description is too sparse to diagnose without version and error information.

## Recommendations

### Actionability

| Field | Value |
|-------|-------|
| Suggested action | needs-info |
| Confidence | 0.88 (88%) |
| Reason | The issue body contains no version numbers, no error messages, no stack traces, and no reproduction steps. The only information is that removing SKSwapChainPanel fixes the problem. More info is needed before investigation can proceed. |
| Suggested repro platform | linux |

### Missing Info

- SkiaSharp.Views.Uno NuGet package version
- Uno Platform version
- Browser and browser version used for testing
- Browser console errors/exceptions
- Detailed reproduction steps

### Automatable Actions

| Type | Risk | Confidence | Description | Details |
|------|------|------------|-------------|---------|
| update-labels | low | 0.95 (95%) | Apply triage classification labels | labels=type/bug, area/SkiaSharp.Views.Uno, os/WASM, backend/OpenGL, partner/unoplatform |
| add-comment | medium | 0.88 (88%) | Request missing version and error information from the reporter | — |

**Comment draft for `add-comment`:**

```markdown
Thank you for reporting this! To help us investigate, could you please provide the following information?

1. **SkiaSharp.Views.Uno version** — which NuGet package version are you using?
2. **Uno Platform version** — what version of the Uno packages are you referencing?
3. **Browser and version** — which browser(s) have you tested (Chrome, Firefox, Safari, etc.)?
4. **Browser console errors** — please open the browser developer tools (F12) and check the Console tab for any errors or exceptions when using `SKSwapChainPanel`. Please paste those here.
5. **Reproduction steps** — can you share the updated `MainPage.xaml` snippet that demonstrates the issue?

This information will help us determine whether the issue is in SkiaSharp, the Uno Platform integration, or the WebGL environment.
```

<details>
<summary>Raw JSON</summary>

```json
{
  "meta": {
    "schemaVersion": "1.0",
    "number": 2363,
    "repo": "mono/SkiaSharp",
    "analyzedAt": "2026-05-01T23:32:00Z"
  },
  "summary": "SKSwapChainPanel (hardware/WebGL rendering) does not work in SkiaSharp.Views.Uno on WASM, while software rendering works fine.",
  "classification": {
    "type": {
      "value": "type/bug",
      "confidence": 0.82
    },
    "area": {
      "value": "area/SkiaSharp.Views.Uno",
      "confidence": 0.95
    },
    "platforms": [
      "os/WASM"
    ],
    "backends": [
      "backend/OpenGL"
    ],
    "partner": "partner/unoplatform"
  },
  "evidence": {
    "bugSignals": {
      "severity": "medium",
      "regressionClaimed": false,
      "errorType": "platform-specific",
      "reproQuality": "partial"
    },
    "reproEvidence": {
      "codeSnippets": [],
      "attachments": [],
      "repoLinks": []
    }
  },
  "analysis": {
    "summary": "Reporter states that SKSwapChainPanel (hardware WebGL rendering) breaks the WASM app in Uno Platform while removing it (falling back to software rendering) restores functionality. The issue lacks version numbers, error messages, and stack traces, making root-cause analysis impossible without more information.",
    "codeInvestigation": [
      {
        "file": "source/SkiaSharp.Views.Uno/SkiaSharp.Views.Uno.WinUI.Wasm/SKSwapChainPanel.Wasm.cs",
        "finding": "SKSwapChainPanel on WASM creates a WebGL context via JSImport/JSInterop and uses GRGlInterface.Create() to initialise GPU-backed SkiaSharp rendering. If WebGL context creation fails or jsInfo.IsValid is false, RenderFrame() silently returns without rendering.",
        "relevance": "direct",
        "lines": "99-155"
      },
      {
        "file": "source/SkiaSharp.Views.Uno/SkiaSharp.Views.Uno.WinUI.Wasm/SKSwapChainPanel.Wasm.cs",
        "finding": "DoLoaded() calls NativeMethods.CreateContext which uses JSImport to call globalThis.SkiaSharp.Views.Windows.SKSwapChainPanel.createContextStatic. If this JS function is missing or errors out, jsInfo will not be populated correctly and IsValid will remain false.",
        "relevance": "direct",
        "lines": "62-66"
      }
    ],
    "keySignals": [
      {
        "text": "As soon I remove sk:SKSwapChainPanel.. from MainPage.xaml the sample is working. So only Software rendering.",
        "source": "issue body",
        "interpretation": "Hardware path (WebGL via SKSwapChainPanel) fails; software fallback works."
      }
    ],
    "rationale": "The reporter describes a functional regression limited to SKSwapChainPanel on WASM/Uno. Code inspection shows SKSwapChainPanel.Wasm.cs relies on WebGL context creation through JSImport; any failure there would silently break rendering. However, with no version numbers, no error messages, and no stack traces, it is impossible to determine whether this is a SkiaSharp bug, an Uno Platform incompatibility, or a WebGL environment issue. Needs-info is appropriate.",
    "nextQuestions": [
      "Which version of SkiaSharp.Views.Uno is being used?",
      "Which version of Uno Platform is being used?",
      "What browser/browser version is being tested?",
      "Are there any console errors in the browser developer tools?",
      "Does this fail on all browsers or specific ones (Chrome, Firefox, Safari)?",
      "Does the SkiaSharpTest.Wasm sample from the Uno.Samples repo reproduce the issue out of the box?"
    ],
    "resolution": {
      "proposals": [
        {
          "title": "Request missing version and error information",
          "category": "investigation",
          "description": "Ask the reporter to provide: SkiaSharp.Views.Uno version, Uno Platform version, browser console errors, and whether the issue is browser-specific.",
          "effort": "cost/xs",
          "validated": "untested"
        }
      ],
      "recommendedProposal": "0",
      "recommendedReason": "The issue description is too sparse to diagnose without version and error information."
    }
  },
  "output": {
    "actionability": {
      "suggestedAction": "needs-info",
      "confidence": 0.88,
      "reason": "The issue body contains no version numbers, no error messages, no stack traces, and no reproduction steps. The only information is that removing SKSwapChainPanel fixes the problem. More info is needed before investigation can proceed.",
      "suggestedReproPlatform": "linux"
    },
    "missingInfo": [
      "SkiaSharp.Views.Uno NuGet package version",
      "Uno Platform version",
      "Browser and browser version used for testing",
      "Browser console errors/exceptions",
      "Detailed reproduction steps"
    ],
    "actions": [
      {
        "type": "update-labels",
        "description": "Apply triage classification labels",
        "risk": "low",
        "confidence": 0.95,
        "labels": [
          "type/bug",
          "area/SkiaSharp.Views.Uno",
          "os/WASM",
          "backend/OpenGL",
          "partner/unoplatform"
        ]
      },
      {
        "type": "add-comment",
        "description": "Request missing version and error information from the reporter",
        "risk": "medium",
        "confidence": 0.88,
        "comment": "Thank you for reporting this! To help us investigate, could you please provide the following information?\n\n1. **SkiaSharp.Views.Uno version** — which NuGet package version are you using?\n2. **Uno Platform version** — what version of the Uno packages are you referencing?\n3. **Browser and version** — which browser(s) have you tested (Chrome, Firefox, Safari, etc.)?\n4. **Browser console errors** — please open the browser developer tools (F12) and check the Console tab for any errors or exceptions when using `SKSwapChainPanel`. Please paste those here.\n5. **Reproduction steps** — can you share the updated `MainPage.xaml` snippet that demonstrates the issue?\n\nThis information will help us determine whether the issue is in SkiaSharp, the Uno Platform integration, or the WebGL environment."
      }
    ]
  }
}
```

</details>
