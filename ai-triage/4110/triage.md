# Issue Triage Report — #4110

| Field | Value |
|-------|-------|
| Repository | mono/SkiaSharp |
| Analyzed | 2026-06-09T05:45:00Z |
| Type | type/bug (0.98 (98%)) |
| Area | area/SkiaSharp.Views.Uno (0.90 (90%)) |
| Suggested action | needs-investigation (0.90 (90%)) |

**Issue Summary:** SKSwapChainPanel rendering produces corrupted text and images in Uno WASM apps when Emscripten MAXIMUM_MEMORY is set to 4GB, caused by 32-bit pointer overflow at the >2GB heap boundary in WebGL texture upload paths.

**Analysis:** When WASM heap exceeds 2GB, pixel data pointers are unsigned 32-bit values >0x80000000 that get sign-extended to negative numbers when marshalled to JavaScript, causing the Emscripten GL wrapper to construct an invalid ArrayBufferView for texSubImage2D.

**Recommendations:** **needs-investigation** — Real regression with complete repro project and clear WebGL error. Root cause is WASM32 large-heap pointer overflow. PR #4122 needs review before the fix can be verified.

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
| Current labels | type/bug |

## Evidence

### Reproduction

1. Create an Uno Platform WASM app using SKSwapChainPanel
2. Add <WasmShellExtraEmccFlags Include="-s MAXIMUM_MEMORY=4GB" /> to the csproj
3. Build and run in Chrome
4. Observe corrupted/missing text and image rendering

**Environment:** Windows 11, Chrome, WebAssembly, SkiaSharp 3.116.0

**Repository links:**
- https://github.com/Jani-z/SKSwapChainBugOnWASM64 — Minimal Uno WASM repro project
- https://github.com/mono/SkiaSharp/pull/4122 — Community fix PR targeting this issue

### Bug Signals

| Field | Value |
|-------|-------|
| Severity | high |
| Regression claimed | True |
| Error type | wrong-output |
| Error message | WebGL: INVALID_OPERATION: texSubImage2D: ArrayBufferView not big enough for request |
| Repro quality | complete |
| Target frameworks | — |

### Version Analysis

| Field | Value |
|-------|-------|
| Mentioned versions | 3.116.0, 2.88.9 |
| Worked in | 2.88.9 |
| Broke in | 3.116.0 |
| Current relevance | likely |
| Relevance reason | The WASM JS interop and pointer handling code has not changed to fix this in 3.x. |

### Regression

| Field | Value |
|-------|-------|
| Is regression | True |
| Confidence | 0.90 (90%) |
| Reason | Reporter explicitly states the same app worked in 2.88.9 and fails in 3.116.0 with MAXIMUM_MEMORY=4GB. |
| Worked in version | 2.88.9 |
| Broke in version | 3.116.0 |

## Analysis

### Technical Summary

When WASM heap exceeds 2GB, pixel data pointers are unsigned 32-bit values >0x80000000 that get sign-extended to negative numbers when marshalled to JavaScript, causing the Emscripten GL wrapper to construct an invalid ArrayBufferView for texSubImage2D.

### Rationale

The WebGL error 'ArrayBufferView not big enough for request' directly indicates a wrong or zero-length buffer passed to texSubImage2D. When MAXIMUM_MEMORY=4GB, allocations can occur above 0x80000000; these addresses exceed INT32_MAX and become negative when treated as signed 32-bit integers in JavaScript. This is a classic WASM32 large-heap pointer issue. The bug was not present in 2.88.x, suggesting a change in the WASM interop or Emscripten version in 3.x. A community fix PR (#4122) already exists targeting this path.

### Key Signals

- "WebGL: INVALID_OPERATION: texSubImage2D: ArrayBufferView not big enough for request" — **issue body** (The ArrayBufferView passed to WebGL texSubImage2D is invalid — either wrong offset or zero length due to pointer sign extension.)
- "The issue does not occur with the same app when memory is kept at or below 2GB." — **issue body** (Confirms the 2GB heap boundary as the trigger: addresses above 0x80000000 overflow signed 32-bit range.)
- "Last Known Good Version: 2.88.9" — **issue body** (Regression introduced in the 3.x series, suggesting a change in WASM interop or Emscripten version.)
- "WasmShellExtraEmccFlags Include="-s MAXIMUM_MEMORY=4GB"" — **issue body** (Uno Platform WebAssembly shell flag enabling 4GB memory; confirms Uno WASM environment.)

### Code Investigation

| File | Lines | Relevance | Finding |
|------|-------|-----------|---------|
| `source/SkiaSharp.Views.Uno/SkiaSharp.Views.Uno.WinUI.Wasm/WasmScripts/SkiaSharp.Views.Uno.Wasm.js` | 27-45 | direct | SKXamlCanvas.invalidateCanvas secure-context path uses Module.HEAPU8.buffer.slice(pData, pData+byteLength); if pData arrives as a negative number (sign-extended from i32 >2GB), the slice produces an empty buffer. Non-secure path has a separate existing bug using byteLength as offset instead of pData. |
| `source/SkiaSharp.Views.Uno/SkiaSharp.Views.Uno.WinUI.Wasm/SKXamlCanvas.Wasm.cs` | 70-111 | direct | InvalidateCanvas passes pixelsHandle.AddrOfPinnedObject() (IntPtr) to JavaScript. With [JSImport] (.NET 7+), IntPtr is marshalled as int32, which sign-extends addresses >2GB (>0x80000000) to negative values in JavaScript. |
| `source/SkiaSharp.Views.Uno/SkiaSharp.Views.Uno.WinUI.Wasm/SKSwapChainPanel.Wasm.cs` | 99-155 | direct | SKSwapChainPanel.RenderFrame renders via native Skia GL (GRContext/GRBackendRenderTarget). Skia calls WebGL texSubImage2D internally via Emscripten GL wrappers. When pixel data is above 2GB in the WASM heap, Emscripten pointer-to-ArrayBufferView conversion fails with the reported WebGL error. |

**Error fingerprint:** `wasm-pointer-overflow-webgl-texsubimage2d-4gb`

### Workarounds

- Keep MAXIMUM_MEMORY at or below 2GB (remove -s MAXIMUM_MEMORY=4GB flag). This prevents allocations above the 2GB boundary.
- Use the raster (CPU) rendering path (SKXamlCanvas instead of SKSwapChainPanel) if WebGL rendering is not required.

### Next Questions

- Does the same issue occur with SKXamlCanvas (raster path) at >2GB memory, or only SKSwapChainPanel (GL path)?
- Does the Blazor SKCanvasView (SKHtmlCanvas.ts putImageData) also exhibit this at >2GB memory?
- What Emscripten version changed between 2.88.9 and 3.116.0 that altered pointer marshalling?
- Is community fix PR #4122 technically correct and ready for review?

### Resolution Proposals

**Hypothesis:** WASM32 pointers >2GB are passed to JavaScript as signed i32 (negative values) via .NET WASM JSImport marshalling or Emscripten GL wrappers, causing invalid ArrayBufferView construction in texSubImage2D.

1. **Workaround: cap MAXIMUM_MEMORY at 2GB or below** — workaround, confidence 0.95 (95%), cost/xs, validated=untested
   - Remove the -s MAXIMUM_MEMORY=4GB Emscripten flag or set it to a value <=2GB. This prevents allocations above the 0x80000000 boundary that trigger the pointer overflow.
2. **Fix: unsigned pointer conversion in JS interop** — fix, confidence 0.80 (80%), cost/s, validated=untested
   - Apply unsigned 32-bit conversion (>>> 0) to pointer values in SKXamlCanvas.invalidateCanvas JavaScript before using them as heap offsets, and investigate the [JSImport] IntPtr marshalling to use nuint or otherwise preserve the unsigned value.
3. **Review and merge community fix PR #4122** — investigation, confidence 0.85 (85%), cost/s, validated=untested
   - Community contributor Soumilgit submitted PR #4122 with a large-heap texture upload fallback for SKSwapChainPanel/Blazor GL contexts and a fix for the SKXamlCanvas heap-offset bug. Review and validate this PR.

**Recommended proposal:** Fix: unsigned pointer conversion in JS interop

**Why:** The unsigned conversion approach fixes the root cause for both SKSwapChainPanel and SKXamlCanvas. PR #4122 should be reviewed in parallel as it may implement this correctly.

## Recommendations

### Actionability

| Field | Value |
|-------|-------|
| Suggested action | needs-investigation |
| Confidence | 0.90 (90%) |
| Reason | Real regression with complete repro project and clear WebGL error. Root cause is WASM32 large-heap pointer overflow. PR #4122 needs review before the fix can be verified. |
| Suggested repro platform | linux |

### Automatable Actions

| Type | Risk | Confidence | Description | Details |
|------|------|------------|-------------|---------|
| update-labels | low | 0.95 (95%) | Apply bug, Uno views, WASM, OpenGL, and Uno partner labels | labels=type/bug, area/SkiaSharp.Views.Uno, os/WASM, backend/OpenGL, partner/unoplatform |
| add-comment | medium | 0.85 (85%) | Acknowledge regression, explain root cause, provide workaround, link PR #4122 | — |
| link-related | low | 0.95 (95%) | Link to community fix PR #4122 | linkedIssue=#4122 |

**Comment draft for `add-comment`:**

```markdown
Thanks for the detailed report and the minimal repro project!

This looks like a **32-bit pointer overflow issue** in the WASM large-heap path. When `MAXIMUM_MEMORY` exceeds 2GB, memory addresses can be allocated above `0x80000000`. These addresses are valid as unsigned 32-bit integers but overflow the signed `int32` range — when the .NET WASM marshaller or Emscripten's GL wrappers pass them to JavaScript as signed integers, they become negative. Constructing an `ArrayBufferView` with a negative offset causes the `texSubImage2D: ArrayBufferView not big enough for request` WebGL error.

**Immediate workaround:** Remove the `-s MAXIMUM_MEMORY=4GB` flag or keep `MAXIMUM_MEMORY ≤ 2GB`. This prevents allocations above the 2GB boundary.

A community fix PR has been submitted at https://github.com/mono/SkiaSharp/pull/4122 — the team will review that PR to evaluate the proposed fix.
```

<details>
<summary>Raw JSON</summary>

```json
{
  "meta": {
    "schemaVersion": "1.0",
    "number": 4110,
    "repo": "mono/SkiaSharp",
    "analyzedAt": "2026-06-09T05:45:00Z",
    "currentLabels": [
      "type/bug"
    ]
  },
  "summary": "SKSwapChainPanel rendering produces corrupted text and images in Uno WASM apps when Emscripten MAXIMUM_MEMORY is set to 4GB, caused by 32-bit pointer overflow at the >2GB heap boundary in WebGL texture upload paths.",
  "classification": {
    "type": {
      "value": "type/bug",
      "confidence": 0.98
    },
    "area": {
      "value": "area/SkiaSharp.Views.Uno",
      "confidence": 0.9
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
      "severity": "high",
      "regressionClaimed": true,
      "errorType": "wrong-output",
      "errorMessage": "WebGL: INVALID_OPERATION: texSubImage2D: ArrayBufferView not big enough for request",
      "reproQuality": "complete"
    },
    "reproEvidence": {
      "stepsToReproduce": [
        "Create an Uno Platform WASM app using SKSwapChainPanel",
        "Add <WasmShellExtraEmccFlags Include=\"-s MAXIMUM_MEMORY=4GB\" /> to the csproj",
        "Build and run in Chrome",
        "Observe corrupted/missing text and image rendering"
      ],
      "environmentDetails": "Windows 11, Chrome, WebAssembly, SkiaSharp 3.116.0",
      "repoLinks": [
        {
          "url": "https://github.com/Jani-z/SKSwapChainBugOnWASM64",
          "description": "Minimal Uno WASM repro project"
        },
        {
          "url": "https://github.com/mono/SkiaSharp/pull/4122",
          "description": "Community fix PR targeting this issue"
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
      "relevanceReason": "The WASM JS interop and pointer handling code has not changed to fix this in 3.x."
    },
    "regression": {
      "isRegression": true,
      "confidence": 0.9,
      "reason": "Reporter explicitly states the same app worked in 2.88.9 and fails in 3.116.0 with MAXIMUM_MEMORY=4GB.",
      "workedInVersion": "2.88.9",
      "brokeInVersion": "3.116.0"
    }
  },
  "analysis": {
    "summary": "When WASM heap exceeds 2GB, pixel data pointers are unsigned 32-bit values >0x80000000 that get sign-extended to negative numbers when marshalled to JavaScript, causing the Emscripten GL wrapper to construct an invalid ArrayBufferView for texSubImage2D.",
    "rationale": "The WebGL error 'ArrayBufferView not big enough for request' directly indicates a wrong or zero-length buffer passed to texSubImage2D. When MAXIMUM_MEMORY=4GB, allocations can occur above 0x80000000; these addresses exceed INT32_MAX and become negative when treated as signed 32-bit integers in JavaScript. This is a classic WASM32 large-heap pointer issue. The bug was not present in 2.88.x, suggesting a change in the WASM interop or Emscripten version in 3.x. A community fix PR (#4122) already exists targeting this path.",
    "keySignals": [
      {
        "text": "WebGL: INVALID_OPERATION: texSubImage2D: ArrayBufferView not big enough for request",
        "source": "issue body",
        "interpretation": "The ArrayBufferView passed to WebGL texSubImage2D is invalid — either wrong offset or zero length due to pointer sign extension."
      },
      {
        "text": "The issue does not occur with the same app when memory is kept at or below 2GB.",
        "source": "issue body",
        "interpretation": "Confirms the 2GB heap boundary as the trigger: addresses above 0x80000000 overflow signed 32-bit range."
      },
      {
        "text": "Last Known Good Version: 2.88.9",
        "source": "issue body",
        "interpretation": "Regression introduced in the 3.x series, suggesting a change in WASM interop or Emscripten version."
      },
      {
        "text": "WasmShellExtraEmccFlags Include=\"-s MAXIMUM_MEMORY=4GB\"",
        "source": "issue body",
        "interpretation": "Uno Platform WebAssembly shell flag enabling 4GB memory; confirms Uno WASM environment."
      }
    ],
    "codeInvestigation": [
      {
        "file": "source/SkiaSharp.Views.Uno/SkiaSharp.Views.Uno.WinUI.Wasm/WasmScripts/SkiaSharp.Views.Uno.Wasm.js",
        "lines": "27-45",
        "finding": "SKXamlCanvas.invalidateCanvas secure-context path uses Module.HEAPU8.buffer.slice(pData, pData+byteLength); if pData arrives as a negative number (sign-extended from i32 >2GB), the slice produces an empty buffer. Non-secure path has a separate existing bug using byteLength as offset instead of pData.",
        "relevance": "direct"
      },
      {
        "file": "source/SkiaSharp.Views.Uno/SkiaSharp.Views.Uno.WinUI.Wasm/SKXamlCanvas.Wasm.cs",
        "lines": "70-111",
        "finding": "InvalidateCanvas passes pixelsHandle.AddrOfPinnedObject() (IntPtr) to JavaScript. With [JSImport] (.NET 7+), IntPtr is marshalled as int32, which sign-extends addresses >2GB (>0x80000000) to negative values in JavaScript.",
        "relevance": "direct"
      },
      {
        "file": "source/SkiaSharp.Views.Uno/SkiaSharp.Views.Uno.WinUI.Wasm/SKSwapChainPanel.Wasm.cs",
        "lines": "99-155",
        "finding": "SKSwapChainPanel.RenderFrame renders via native Skia GL (GRContext/GRBackendRenderTarget). Skia calls WebGL texSubImage2D internally via Emscripten GL wrappers. When pixel data is above 2GB in the WASM heap, Emscripten pointer-to-ArrayBufferView conversion fails with the reported WebGL error.",
        "relevance": "direct"
      }
    ],
    "workarounds": [
      "Keep MAXIMUM_MEMORY at or below 2GB (remove -s MAXIMUM_MEMORY=4GB flag). This prevents allocations above the 2GB boundary.",
      "Use the raster (CPU) rendering path (SKXamlCanvas instead of SKSwapChainPanel) if WebGL rendering is not required."
    ],
    "nextQuestions": [
      "Does the same issue occur with SKXamlCanvas (raster path) at >2GB memory, or only SKSwapChainPanel (GL path)?",
      "Does the Blazor SKCanvasView (SKHtmlCanvas.ts putImageData) also exhibit this at >2GB memory?",
      "What Emscripten version changed between 2.88.9 and 3.116.0 that altered pointer marshalling?",
      "Is community fix PR #4122 technically correct and ready for review?"
    ],
    "errorFingerprint": "wasm-pointer-overflow-webgl-texsubimage2d-4gb",
    "resolution": {
      "hypothesis": "WASM32 pointers >2GB are passed to JavaScript as signed i32 (negative values) via .NET WASM JSImport marshalling or Emscripten GL wrappers, causing invalid ArrayBufferView construction in texSubImage2D.",
      "proposals": [
        {
          "title": "Workaround: cap MAXIMUM_MEMORY at 2GB or below",
          "description": "Remove the -s MAXIMUM_MEMORY=4GB Emscripten flag or set it to a value <=2GB. This prevents allocations above the 0x80000000 boundary that trigger the pointer overflow.",
          "category": "workaround",
          "confidence": 0.95,
          "effort": "cost/xs",
          "validated": "untested"
        },
        {
          "title": "Fix: unsigned pointer conversion in JS interop",
          "description": "Apply unsigned 32-bit conversion (>>> 0) to pointer values in SKXamlCanvas.invalidateCanvas JavaScript before using them as heap offsets, and investigate the [JSImport] IntPtr marshalling to use nuint or otherwise preserve the unsigned value.",
          "category": "fix",
          "confidence": 0.8,
          "effort": "cost/s",
          "validated": "untested"
        },
        {
          "title": "Review and merge community fix PR #4122",
          "description": "Community contributor Soumilgit submitted PR #4122 with a large-heap texture upload fallback for SKSwapChainPanel/Blazor GL contexts and a fix for the SKXamlCanvas heap-offset bug. Review and validate this PR.",
          "category": "investigation",
          "confidence": 0.85,
          "effort": "cost/s",
          "validated": "untested"
        }
      ],
      "recommendedProposal": "Fix: unsigned pointer conversion in JS interop",
      "recommendedReason": "The unsigned conversion approach fixes the root cause for both SKSwapChainPanel and SKXamlCanvas. PR #4122 should be reviewed in parallel as it may implement this correctly."
    }
  },
  "output": {
    "actionability": {
      "suggestedAction": "needs-investigation",
      "confidence": 0.9,
      "reason": "Real regression with complete repro project and clear WebGL error. Root cause is WASM32 large-heap pointer overflow. PR #4122 needs review before the fix can be verified.",
      "suggestedReproPlatform": "linux"
    },
    "actions": [
      {
        "type": "update-labels",
        "description": "Apply bug, Uno views, WASM, OpenGL, and Uno partner labels",
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
        "description": "Acknowledge regression, explain root cause, provide workaround, link PR #4122",
        "risk": "medium",
        "confidence": 0.85,
        "comment": "Thanks for the detailed report and the minimal repro project!\n\nThis looks like a **32-bit pointer overflow issue** in the WASM large-heap path. When `MAXIMUM_MEMORY` exceeds 2GB, memory addresses can be allocated above `0x80000000`. These addresses are valid as unsigned 32-bit integers but overflow the signed `int32` range — when the .NET WASM marshaller or Emscripten's GL wrappers pass them to JavaScript as signed integers, they become negative. Constructing an `ArrayBufferView` with a negative offset causes the `texSubImage2D: ArrayBufferView not big enough for request` WebGL error.\n\n**Immediate workaround:** Remove the `-s MAXIMUM_MEMORY=4GB` flag or keep `MAXIMUM_MEMORY ≤ 2GB`. This prevents allocations above the 2GB boundary.\n\nA community fix PR has been submitted at https://github.com/mono/SkiaSharp/pull/4122 — the team will review that PR to evaluate the proposed fix."
      },
      {
        "type": "link-related",
        "description": "Link to community fix PR #4122",
        "risk": "low",
        "confidence": 0.95,
        "linkedIssue": 4122
      }
    ]
  }
}
```

</details>
