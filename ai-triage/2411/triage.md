# Issue Triage Report — #2411

| Field | Value |
|-------|-------|
| Repository | mono/SkiaSharp |
| Analyzed | 2026-04-24T19:30:00Z |
| Type | type/bug (0.88 (88%)) |
| Area | area/SkiaSharp.Views.Blazor (0.85 (85%)) |
| Suggested action | needs-investigation (0.80 (80%)) |

**Issue Summary:** Blazor WebAssembly app using SkiaSharp throws DllNotFoundException for libSkiaSharp when deployed to Azure via GitHub Actions with .NET 7.0, while the same app works with .NET 6.0; root cause is missing wasm-tools workload in the CI environment.

**Analysis:** SkiaSharp.NativeAssets.WebAssembly sets WasmBuildNative=true in its MSBuild targets and registers the static libSkiaSharp.a via NativeFileReference, which requires the .NET wasm-tools workload (Emscripten toolchain) to link the library into dotnet.wasm at build time. GitHub-hosted runners do not have wasm-tools pre-installed for .NET 7.0+. Without it, the NativeFileReference item is either silently skipped or Emscripten is unavailable, resulting in a dotnet.wasm missing the libSkiaSharp symbols — which surfaces as DllNotFoundException at runtime.

**Recommendations:** **needs-investigation** — The immediate workaround is confirmed (install wasm-tools in CI). However, the pattern has recurred across .NET 7, 8, 9, and 10 in at least three separate issues, suggesting SkiaSharp should either emit a build-time diagnostic when wasm-tools is absent or improve its documentation to make the CI requirement more prominent.

---

## Classification

| Field | Value |
|-------|-------|
| Type | type/bug |
| Area | area/SkiaSharp.Views.Blazor |
| Platforms | os/WASM |
| Backends | — |
| Tenets | tenet/reliability, tenet/compatibility |
| Partner | — |

## Evidence

### Reproduction

1. Create a Blazor WebAssembly client-side app targeting .NET 7.0
2. Add SkiaSharp 2.88.3 NuGet package
3. Add a Razor component that calls SkiaSharp APIs (SKImageInfo, SKSurface, SKCanvas)
4. Deploy via GitHub Actions to Azure App Service (Windows) without installing the wasm-tools workload
5. Navigate to the component page — DllNotFoundException: libSkiaSharp is thrown

**Environment:** SkiaSharp 2.88.3, .NET 7.0, Blazor WebAssembly (client-side), Azure App Service (Windows), GitHub Actions CI/CD

**Repository links:**
- https://github.com/mono/SkiaSharp/issues/2745 — Related: same DllNotFoundException in Blazor WASM .NET 8.0
- https://github.com/mono/SkiaSharp/issues/3422 — Related: same TypeInitialization/DllNotFoundException in Blazor WASM .NET 10

**Attachments:**
- Blazor-GitHub-Actions.zip — https://github.com/mono/SkiaSharp/files/10924231/Blazor-GitHub-Actions.zip — Reproduction project: .NET 6 and .NET 7 Blazor apps with GitHub Actions workflow

### Bug Signals

| Field | Value |
|-------|-------|
| Severity | medium |
| Regression claimed | True |
| Error type | exception |
| Error message | System.TypeInitializationException: TypeInitialization_Type, SkiaSharp.SKImageInfo ---> System.DllNotFoundException: libSkiaSharp |
| Repro quality | complete |
| Target frameworks | net7.0 |

### Version Analysis

| Field | Value |
|-------|-------|
| Mentioned versions | 2.88.3 |
| Worked in | 2.88.3 with .NET 6.0 |
| Broke in | 2.88.3 with .NET 7.0 |
| Current relevance | likely |
| Relevance reason | Similar issues (2745, 3422) confirm the pattern persists across .NET 8, 9, and 10 when wasm-tools is absent from the CI environment. |

### Regression

| Field | Value |
|-------|-------|
| Is regression | True |
| Confidence | 0.82 (82%) |
| Reason | Same SkiaSharp version (2.88.3) and same code works with .NET 6.0 but fails with .NET 7.0. The .NET 7.0 SDK requires the wasm-tools workload to be installed separately, whereas .NET 6.0 bundled it. |
| Worked in version | net6.0 |
| Broke in version | net7.0 |

## Analysis

### Technical Summary

SkiaSharp.NativeAssets.WebAssembly sets WasmBuildNative=true in its MSBuild targets and registers the static libSkiaSharp.a via NativeFileReference, which requires the .NET wasm-tools workload (Emscripten toolchain) to link the library into dotnet.wasm at build time. GitHub-hosted runners do not have wasm-tools pre-installed for .NET 7.0+. Without it, the NativeFileReference item is either silently skipped or Emscripten is unavailable, resulting in a dotnet.wasm missing the libSkiaSharp symbols — which surfaces as DllNotFoundException at runtime.

### Rationale

The issue is a well-understood CI environment setup problem: .NET 7.0 separated the wasm-tools workload from the base SDK install (unlike .NET 6.0). SkiaSharp.NativeAssets.WebAssembly's targets unconditionally enable WasmBuildNative and add NativeFileReference, both of which require wasm-tools. A community contributor confirmed the fix in March 2026 by adding 'dotnet workload install wasm-tools' to the GitHub Actions workflow.

### Key Signals

- "System.DllNotFoundException: libSkiaSharp at SkiaSharp.SKImageInfo..cctor()" — **issue body** (The static libSkiaSharp.a was not linked into dotnet.wasm at build time — WASM runtime cannot resolve the symbol.)
- "When deploy .NET 6.0 works fine without any issue. .NET 7.0 we are facing libSkiaSharp assembly not found issue." — **issue body** (Regression introduced by .NET 7.0 requiring wasm-tools workload to be installed separately.)
- "This issue can be fixed by installing wasm-tools: dotnet workload install wasm-tools" — **comment by ingweland (2026-03-21)** (Confirmed fix: add wasm-tools workload installation step to GitHub Actions workflow.)

### Code Investigation

| File | Lines | Relevance | Finding |
|------|-------|-----------|---------|
| `binding/SkiaSharp.NativeAssets.WebAssembly/buildTransitive/SkiaSharp.targets` | 22-41 | direct | For Blazor WASM apps (UsingMicrosoftNETSdkBlazorWebAssembly == true), the targets file sets WasmBuildNative=true and adds NativeFileReference for the Emscripten-compiled libSkiaSharp.a — for .NET 7.0 it uses the 3.1.12 Emscripten version. Both require the wasm-tools workload to process NativeFileReference during the WASM native build step. |
| `binding/SkiaSharp.NativeAssets.WebAssembly/buildTransitive/SkiaSharp.props` | 1-10 | direct | Sets SkiaSharpStaticLibraryPath pointing to build-time libSkiaSharp.a files packaged in the NuGet. Confirms SkiaSharp ships static libraries for WASM and relies on the Emscripten toolchain (wasm-tools workload) to perform the final link. |
| `documentation/dev/packages.md` | 95-99 | context | Documentation explicitly states: 'WebAssembly uses static linking at compile time. The .a files are passed to the Emscripten linker which embeds them into the final dotnet.wasm binary.' Confirms that missing wasm-tools means the .a file is never linked. |

**Error fingerprint:** `DllNotFoundException:libSkiaSharp@SKImageInfo..cctor:BlazorWASM`

### Workarounds

- Add 'dotnet workload install wasm-tools' as a step in the GitHub Actions workflow before the build step.
- Use 'SkiaSharp.Views.Blazor' package instead of bare SkiaSharp — it auto-includes SkiaSharp.NativeAssets.WebAssembly and documents the wasm-tools requirement.

### Next Questions

- Should SkiaSharp emit a build-time error/warning when wasm-tools is absent and WasmBuildNative=true?
- Can the documentation for SkiaSharp.NativeAssets.WebAssembly be updated to explicitly list 'wasm-tools workload required' as a prerequisite?

### Resolution Proposals

**Hypothesis:** The wasm-tools .NET workload is not pre-installed on GitHub-hosted runners for .NET 7.0+. SkiaSharp's WASM targets require this workload to link libSkiaSharp.a into dotnet.wasm. Without it, the native library is absent from the WASM bundle, causing DllNotFoundException at runtime.

1. **Install wasm-tools workload in CI** — workaround, confidence 0.95 (95%), cost/xs, validated=yes
   - Add a workflow step to install the wasm-tools workload before building the Blazor WASM app. This enables Emscripten and allows SkiaSharp's NativeFileReference to be linked into dotnet.wasm.

```csharp
- name: Install WASM workload
  run: dotnet workload install wasm-tools
```
2. **Use SkiaSharp.Views.Blazor for better setup guidance** — alternative, confidence 0.80 (80%), cost/xs, validated=untested
   - Reference SkiaSharp.Views.Blazor instead of the bare SkiaSharp package. It auto-includes SkiaSharp.NativeAssets.WebAssembly and its NuGet readme documents the wasm-tools requirement, making the setup requirement more discoverable.

**Recommended proposal:** Install wasm-tools workload in CI

**Why:** Confirmed fix by community contributor. Direct solution with zero SkiaSharp code changes required.

## Recommendations

### Actionability

| Field | Value |
|-------|-------|
| Suggested action | needs-investigation |
| Confidence | 0.80 (80%) |
| Reason | The immediate workaround is confirmed (install wasm-tools in CI). However, the pattern has recurred across .NET 7, 8, 9, and 10 in at least three separate issues, suggesting SkiaSharp should either emit a build-time diagnostic when wasm-tools is absent or improve its documentation to make the CI requirement more prominent. |
| Suggested repro platform | linux |

### Automatable Actions

| Type | Risk | Confidence | Description | Details |
|------|------|------------|-------------|---------|
| update-labels | low | 0.90 (90%) | Apply type/bug, area/SkiaSharp.Views.Blazor, os/WASM, tenet/reliability, tenet/compatibility labels | labels=type/bug, area/SkiaSharp.Views.Blazor, os/WASM, tenet/reliability, tenet/compatibility |
| add-comment | medium | 0.92 (92%) | Post confirmed workaround with CI snippet | — |
| link-related | low | 0.90 (90%) | Cross-reference related issue #2745 (same error in .NET 8.0 Blazor WASM) | linkedIssue=#2745 |
| link-related | low | 0.90 (90%) | Cross-reference related issue #3422 (same error in .NET 10 Blazor WASM) | linkedIssue=#3422 |

**Comment draft for `add-comment`:**

```markdown
Thanks for the detailed report and the reproduction project.

Here's the fix for your GitHub Actions workflow: add a step to install the `wasm-tools` .NET workload before building.

```yaml
- name: Install WASM workload
  run: dotnet workload install wasm-tools
```

This workload provides the Emscripten toolchain that SkiaSharp needs to link its native `libSkiaSharp.a` into your `dotnet.wasm` binary at build time. Starting with .NET 7.0, this workload must be installed explicitly — it was bundled with the .NET 6.0 SDK, which is why that version worked without it.

This fix was confirmed by a community contributor in a recent comment above. If adding that step resolves the issue, please let us know — we're tracking whether to add a build-time diagnostic to catch this missing workload earlier.
```

<details>
<summary>Raw JSON</summary>

```json
{
  "meta": {
    "schemaVersion": "1.0",
    "number": 2411,
    "repo": "mono/SkiaSharp",
    "analyzedAt": "2026-04-24T19:30:00Z"
  },
  "summary": "Blazor WebAssembly app using SkiaSharp throws DllNotFoundException for libSkiaSharp when deployed to Azure via GitHub Actions with .NET 7.0, while the same app works with .NET 6.0; root cause is missing wasm-tools workload in the CI environment.",
  "classification": {
    "type": {
      "value": "type/bug",
      "confidence": 0.88
    },
    "area": {
      "value": "area/SkiaSharp.Views.Blazor",
      "confidence": 0.85
    },
    "platforms": [
      "os/WASM"
    ],
    "tenets": [
      "tenet/reliability",
      "tenet/compatibility"
    ]
  },
  "evidence": {
    "bugSignals": {
      "severity": "medium",
      "regressionClaimed": true,
      "errorType": "exception",
      "errorMessage": "System.TypeInitializationException: TypeInitialization_Type, SkiaSharp.SKImageInfo ---> System.DllNotFoundException: libSkiaSharp",
      "reproQuality": "complete",
      "targetFrameworks": [
        "net7.0"
      ]
    },
    "reproEvidence": {
      "stepsToReproduce": [
        "Create a Blazor WebAssembly client-side app targeting .NET 7.0",
        "Add SkiaSharp 2.88.3 NuGet package",
        "Add a Razor component that calls SkiaSharp APIs (SKImageInfo, SKSurface, SKCanvas)",
        "Deploy via GitHub Actions to Azure App Service (Windows) without installing the wasm-tools workload",
        "Navigate to the component page — DllNotFoundException: libSkiaSharp is thrown"
      ],
      "attachments": [
        {
          "url": "https://github.com/mono/SkiaSharp/files/10924231/Blazor-GitHub-Actions.zip",
          "description": "Reproduction project: .NET 6 and .NET 7 Blazor apps with GitHub Actions workflow",
          "filename": "Blazor-GitHub-Actions.zip"
        }
      ],
      "environmentDetails": "SkiaSharp 2.88.3, .NET 7.0, Blazor WebAssembly (client-side), Azure App Service (Windows), GitHub Actions CI/CD",
      "repoLinks": [
        {
          "url": "https://github.com/mono/SkiaSharp/issues/2745",
          "description": "Related: same DllNotFoundException in Blazor WASM .NET 8.0"
        },
        {
          "url": "https://github.com/mono/SkiaSharp/issues/3422",
          "description": "Related: same TypeInitialization/DllNotFoundException in Blazor WASM .NET 10"
        }
      ]
    },
    "versionAnalysis": {
      "mentionedVersions": [
        "2.88.3"
      ],
      "workedIn": "2.88.3 with .NET 6.0",
      "brokeIn": "2.88.3 with .NET 7.0",
      "currentRelevance": "likely",
      "relevanceReason": "Similar issues (2745, 3422) confirm the pattern persists across .NET 8, 9, and 10 when wasm-tools is absent from the CI environment."
    },
    "regression": {
      "isRegression": true,
      "confidence": 0.82,
      "reason": "Same SkiaSharp version (2.88.3) and same code works with .NET 6.0 but fails with .NET 7.0. The .NET 7.0 SDK requires the wasm-tools workload to be installed separately, whereas .NET 6.0 bundled it.",
      "workedInVersion": "net6.0",
      "brokeInVersion": "net7.0"
    }
  },
  "analysis": {
    "summary": "SkiaSharp.NativeAssets.WebAssembly sets WasmBuildNative=true in its MSBuild targets and registers the static libSkiaSharp.a via NativeFileReference, which requires the .NET wasm-tools workload (Emscripten toolchain) to link the library into dotnet.wasm at build time. GitHub-hosted runners do not have wasm-tools pre-installed for .NET 7.0+. Without it, the NativeFileReference item is either silently skipped or Emscripten is unavailable, resulting in a dotnet.wasm missing the libSkiaSharp symbols — which surfaces as DllNotFoundException at runtime.",
    "rationale": "The issue is a well-understood CI environment setup problem: .NET 7.0 separated the wasm-tools workload from the base SDK install (unlike .NET 6.0). SkiaSharp.NativeAssets.WebAssembly's targets unconditionally enable WasmBuildNative and add NativeFileReference, both of which require wasm-tools. A community contributor confirmed the fix in March 2026 by adding 'dotnet workload install wasm-tools' to the GitHub Actions workflow.",
    "keySignals": [
      {
        "text": "System.DllNotFoundException: libSkiaSharp at SkiaSharp.SKImageInfo..cctor()",
        "source": "issue body",
        "interpretation": "The static libSkiaSharp.a was not linked into dotnet.wasm at build time — WASM runtime cannot resolve the symbol."
      },
      {
        "text": "When deploy .NET 6.0 works fine without any issue. .NET 7.0 we are facing libSkiaSharp assembly not found issue.",
        "source": "issue body",
        "interpretation": "Regression introduced by .NET 7.0 requiring wasm-tools workload to be installed separately."
      },
      {
        "text": "This issue can be fixed by installing wasm-tools: dotnet workload install wasm-tools",
        "source": "comment by ingweland (2026-03-21)",
        "interpretation": "Confirmed fix: add wasm-tools workload installation step to GitHub Actions workflow."
      }
    ],
    "codeInvestigation": [
      {
        "file": "binding/SkiaSharp.NativeAssets.WebAssembly/buildTransitive/SkiaSharp.targets",
        "lines": "22-41",
        "finding": "For Blazor WASM apps (UsingMicrosoftNETSdkBlazorWebAssembly == true), the targets file sets WasmBuildNative=true and adds NativeFileReference for the Emscripten-compiled libSkiaSharp.a — for .NET 7.0 it uses the 3.1.12 Emscripten version. Both require the wasm-tools workload to process NativeFileReference during the WASM native build step.",
        "relevance": "direct"
      },
      {
        "file": "binding/SkiaSharp.NativeAssets.WebAssembly/buildTransitive/SkiaSharp.props",
        "lines": "1-10",
        "finding": "Sets SkiaSharpStaticLibraryPath pointing to build-time libSkiaSharp.a files packaged in the NuGet. Confirms SkiaSharp ships static libraries for WASM and relies on the Emscripten toolchain (wasm-tools workload) to perform the final link.",
        "relevance": "direct"
      },
      {
        "file": "documentation/dev/packages.md",
        "lines": "95-99",
        "finding": "Documentation explicitly states: 'WebAssembly uses static linking at compile time. The .a files are passed to the Emscripten linker which embeds them into the final dotnet.wasm binary.' Confirms that missing wasm-tools means the .a file is never linked.",
        "relevance": "context"
      }
    ],
    "workarounds": [
      "Add 'dotnet workload install wasm-tools' as a step in the GitHub Actions workflow before the build step.",
      "Use 'SkiaSharp.Views.Blazor' package instead of bare SkiaSharp — it auto-includes SkiaSharp.NativeAssets.WebAssembly and documents the wasm-tools requirement."
    ],
    "nextQuestions": [
      "Should SkiaSharp emit a build-time error/warning when wasm-tools is absent and WasmBuildNative=true?",
      "Can the documentation for SkiaSharp.NativeAssets.WebAssembly be updated to explicitly list 'wasm-tools workload required' as a prerequisite?"
    ],
    "errorFingerprint": "DllNotFoundException:libSkiaSharp@SKImageInfo..cctor:BlazorWASM",
    "resolution": {
      "hypothesis": "The wasm-tools .NET workload is not pre-installed on GitHub-hosted runners for .NET 7.0+. SkiaSharp's WASM targets require this workload to link libSkiaSharp.a into dotnet.wasm. Without it, the native library is absent from the WASM bundle, causing DllNotFoundException at runtime.",
      "proposals": [
        {
          "title": "Install wasm-tools workload in CI",
          "description": "Add a workflow step to install the wasm-tools workload before building the Blazor WASM app. This enables Emscripten and allows SkiaSharp's NativeFileReference to be linked into dotnet.wasm.",
          "category": "workaround",
          "codeSnippet": "- name: Install WASM workload\n  run: dotnet workload install wasm-tools",
          "confidence": 0.95,
          "effort": "cost/xs",
          "validated": "yes"
        },
        {
          "title": "Use SkiaSharp.Views.Blazor for better setup guidance",
          "description": "Reference SkiaSharp.Views.Blazor instead of the bare SkiaSharp package. It auto-includes SkiaSharp.NativeAssets.WebAssembly and its NuGet readme documents the wasm-tools requirement, making the setup requirement more discoverable.",
          "category": "alternative",
          "confidence": 0.8,
          "effort": "cost/xs",
          "validated": "untested"
        }
      ],
      "recommendedProposal": "Install wasm-tools workload in CI",
      "recommendedReason": "Confirmed fix by community contributor. Direct solution with zero SkiaSharp code changes required."
    }
  },
  "output": {
    "actionability": {
      "suggestedAction": "needs-investigation",
      "confidence": 0.8,
      "reason": "The immediate workaround is confirmed (install wasm-tools in CI). However, the pattern has recurred across .NET 7, 8, 9, and 10 in at least three separate issues, suggesting SkiaSharp should either emit a build-time diagnostic when wasm-tools is absent or improve its documentation to make the CI requirement more prominent.",
      "suggestedReproPlatform": "linux"
    },
    "actions": [
      {
        "type": "update-labels",
        "description": "Apply type/bug, area/SkiaSharp.Views.Blazor, os/WASM, tenet/reliability, tenet/compatibility labels",
        "risk": "low",
        "confidence": 0.9,
        "labels": [
          "type/bug",
          "area/SkiaSharp.Views.Blazor",
          "os/WASM",
          "tenet/reliability",
          "tenet/compatibility"
        ]
      },
      {
        "type": "add-comment",
        "description": "Post confirmed workaround with CI snippet",
        "risk": "medium",
        "confidence": 0.92,
        "comment": "Thanks for the detailed report and the reproduction project.\n\nHere's the fix for your GitHub Actions workflow: add a step to install the `wasm-tools` .NET workload before building.\n\n```yaml\n- name: Install WASM workload\n  run: dotnet workload install wasm-tools\n```\n\nThis workload provides the Emscripten toolchain that SkiaSharp needs to link its native `libSkiaSharp.a` into your `dotnet.wasm` binary at build time. Starting with .NET 7.0, this workload must be installed explicitly — it was bundled with the .NET 6.0 SDK, which is why that version worked without it.\n\nThis fix was confirmed by a community contributor in a recent comment above. If adding that step resolves the issue, please let us know — we're tracking whether to add a build-time diagnostic to catch this missing workload earlier."
      },
      {
        "type": "link-related",
        "description": "Cross-reference related issue #2745 (same error in .NET 8.0 Blazor WASM)",
        "risk": "low",
        "confidence": 0.9,
        "linkedIssue": 2745
      },
      {
        "type": "link-related",
        "description": "Cross-reference related issue #3422 (same error in .NET 10 Blazor WASM)",
        "risk": "low",
        "confidence": 0.9,
        "linkedIssue": 3422
      }
    ]
  }
}
```

</details>
