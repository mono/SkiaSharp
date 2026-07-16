# Issue Triage Report — #4440

| Field | Value |
|-------|-------|
| Repository | mono/SkiaSharp |
| Analyzed | 2026-07-16T05:12:32Z |
| Type | type/bug (0.95 (95%)) |
| Area | area/libSkiaSharp.native (0.92 (92%)) |
| Suggested action | needs-investigation (0.90 (90%)) |

**Issue Summary:** SkiaSharp.NativeAssets.WebAssembly fails to link on .NET 11 preview 6+ because the pre-built WASM static library was compiled with Emscripten 3.1.56 which uses saveSetjmp/testSetjmp, but .NET 11 ships Emscripten 5.0.6 which replaced those symbols with __wasm_setjmp/__wasm_setjmp_test.

**Analysis:** SkiaSharp ships pre-built WASM static libraries compiled with Emscripten 3.1.56 for net9.0+, but .NET 11 preview 6+ bundles Emscripten 5.0.6, which removed the legacy setjmp symbols (saveSetjmp, testSetjmp) in favour of Wasm-native equivalents (__wasm_setjmp, __wasm_setjmp_test). The linker fails because there is an ABI mismatch between the pre-built .a files and the host emcc version. A new build of libSkiaSharp.a with Emscripten 5.0.6 is required and the SkiaSharp.targets file must be updated to select the correct artifact for net11.0+.

**Recommendations:** **needs-investigation** — Root cause is clear (ABI mismatch between emsdk 3.1.56 and 5.0.6), but the fix requires native CI pipeline work. Needs a maintainer to scope the 5.x build effort and verify .NET 11 final Emscripten version.

---

## Classification

| Field | Value |
|-------|-------|
| Type | type/bug |
| Area | area/libSkiaSharp.native |
| Platforms | os/WASM |
| Backends | — |
| Tenets | tenet/compatibility |
| Perf | — |
| Partner | — |
| Current labels | type/bug |

## Evidence

### Reproduction

1. Create a Blazor WASM or browser-wasm project targeting net11.0
2. Add SkiaSharp 3.116.0 or 3.119.4 NuGet package
3. Run dotnet publish -c Release
4. Observe linker errors: undefined symbol: saveSetjmp, undefined symbol: testSetjmp

**Environment:** macOS, VS Code, .NET 11.0-preview.6, Emscripten 5.0.6 (from Microsoft.NET.Runtime.Emscripten.5.0.6.Sdk.osx-arm64)

### Bug Signals

| Field | Value |
|-------|-------|
| Severity | high |
| Regression claimed | False |
| Error type | build-error |
| Error message | undefined symbol: saveSetjmp / undefined symbol: testSetjmp |
| Repro quality | complete |
| Target frameworks | net11.0-browser-wasm |

### Version Analysis

| Field | Value |
|-------|-------|
| Mentioned versions | 3.116.0, 3.119.4, 4.151.0-preview.2.1 |
| Worked in | — |
| Broke in | — |
| Current relevance | likely |
| Relevance reason | SkiaSharp.targets only references emscripten 3.1.34 (net8.0) and 3.1.56 (net9.0+) native binaries; no net11.0 / emsdk 5.x native assets exist yet. |

## Analysis

### Technical Summary

SkiaSharp ships pre-built WASM static libraries compiled with Emscripten 3.1.56 for net9.0+, but .NET 11 preview 6+ bundles Emscripten 5.0.6, which removed the legacy setjmp symbols (saveSetjmp, testSetjmp) in favour of Wasm-native equivalents (__wasm_setjmp, __wasm_setjmp_test). The linker fails because there is an ABI mismatch between the pre-built .a files and the host emcc version. A new build of libSkiaSharp.a with Emscripten 5.0.6 is required and the SkiaSharp.targets file must be updated to select the correct artifact for net11.0+.

### Rationale

This is clearly a build compatibility bug: .NET 11 preview 6+ ships Emscripten 5.0.6 which is ABI-incompatible with SkiaSharp's pre-built WASM static libraries compiled with Emscripten 3.1.56. The SkiaSharp.targets file lacks a net11.0 entry pointing to a 5.x-compiled artifact. The fix requires building libSkiaSharp.a / libHarfBuzzSharp.a with Emscripten 5.0.6 and updating SkiaSharp.targets to select those artifacts for net11.0+.

### Key Signals

- "undefined symbol: saveSetjmp / undefined symbol: testSetjmp" — **issue body build log** (Emscripten 5.x removed the legacy SJLJ symbols. The pre-built .a was compiled with emsdk 3.1.56 which still had them.)
- "emscripten 5.0.6 (from Microsoft.NET.Runtime.Emscripten.5.0.6.Sdk)" — **issue body build log** (.NET 11 preview 6+ upgraded its bundled Emscripten from 3.x to 5.0.6, creating an ABI mismatch.)
- "Same error on 4.151.0-preview.2.1 version" — **comment #1** (Latest SkiaSharp preview also does not have a 5.x build, confirming the issue is not version-specific within SkiaSharp releases.)
- "replace saveSetjmp and testSetjmp on __wasm_setjmp and __wasm_setjmp_test" — **issue body expected behavior** (Reporter correctly identifies the root cause and expected fix.)

### Code Investigation

| File | Lines | Relevance | Finding |
|------|-------|-----------|---------|
| `binding/SkiaSharp.NativeAssets.WebAssembly/buildTransitive/SkiaSharp.targets` | 32-35 | direct | NativeFileReference for net9.0+ always points to emscripten 3.1.56 artifacts. There is no condition for net11.0 / emscripten 5.x, so net11.0 picks up the incompatible 3.1.56 binary. |
| `scripts/azure-templates-jobs-wasm-matrix.yml` | — | direct | CI matrix builds only for emscripten 3.1.34 and 3.1.56. No build target for emscripten 5.0.6 exists yet. |
| `native/wasm/build.cake` | 7-8 | related | EMSCRIPTEN_VERSION is controlled by env/arg. The build infrastructure already supports versioned output directories, so adding a 5.0.6 build is feasible. |

### Next Questions

- Which exact Emscripten version does .NET 11 RTM plan to ship? (5.0.6 or later?)
- Should the 5.x build also cover SIMD and Threading variants?
- Does the -fwasm-exceptions / WASM_LEGACY_EXCEPTIONS=0 flag combination used by .NET 11 require a matching build flag in the native artifact?

### Resolution Proposals

**Hypothesis:** Build libSkiaSharp.a and libHarfBuzzSharp.a with Emscripten 5.0.6 and add a net11.0 condition to SkiaSharp.targets.

1. **Add Emscripten 5.0.6 WASM build variant** — fix, confidence 0.90 (90%), cost/m, validated=untested
   - Extend the CI matrix in azure-templates-jobs-wasm-matrix.yml with emscripten 5.0.6 entries (st, simd, mt, simd+mt). Update SkiaSharp.targets to add a NativeFileReference for net11.0+ pointing to the 5.0.6 artifacts.
2. **Workaround: stay on .NET 10** — workaround, confidence 0.95 (95%), cost/xs, validated=untested
   - Until a 5.x build is shipped, use net10.0-browser-wasm instead of net11.0-browser-wasm for WASM projects using SkiaSharp. .NET 10 bundles Emscripten 3.1.56 which matches the current SkiaSharp artifacts.

**Recommended proposal:** Add Emscripten 5.0.6 WASM build variant

**Why:** Workaround is temporary; .NET 11 will ship and users will need native support. The build infrastructure is already parameterized by emscripten version, so the work is incremental.

## Recommendations

### Actionability

| Field | Value |
|-------|-------|
| Suggested action | needs-investigation |
| Confidence | 0.90 (90%) |
| Reason | Root cause is clear (ABI mismatch between emsdk 3.1.56 and 5.0.6), but the fix requires native CI pipeline work. Needs a maintainer to scope the 5.x build effort and verify .NET 11 final Emscripten version. |
| Suggested repro platform | linux |

### Automatable Actions

| Type | Risk | Confidence | Description | Details |
|------|------|------------|-------------|---------|
| update-labels | low | 0.95 (95%) | Apply area/libSkiaSharp.native, os/WASM, tenet/compatibility | labels=type/bug, area/libSkiaSharp.native, os/WASM, tenet/compatibility |
| add-comment | medium | 0.90 (90%) | Acknowledge root cause and provide workaround | — |

**Comment draft for `add-comment`:**

```markdown
Thanks for the detailed report!

The root cause is an **Emscripten ABI mismatch**: SkiaSharp's pre-built WASM static libraries (`.a` files) are compiled with Emscripten **3.1.56**, which uses `saveSetjmp`/`testSetjmp`. .NET 11 preview 6+ bundles Emscripten **5.0.6**, which replaced those symbols with `__wasm_setjmp`/`__wasm_setjmp_test`. The linker cannot find the old symbols in the new toolchain.

A fix requires building new `libSkiaSharp.a` / `libHarfBuzzSharp.a` artifacts with Emscripten 5.0.6 and updating `SkiaSharp.targets` with a `net11.0` condition.

**Workaround (while you wait):** target `net10.0` for WASM instead of `net11.0`. .NET 10 ships Emscripten 3.1.56 which matches the current SkiaSharp artifacts.

This issue is tracked for the next SkiaSharp release once .NET 11's Emscripten version is finalized.
```

<details>
<summary>Raw JSON</summary>

```json
{
  "meta": {
    "schemaVersion": "1.0",
    "number": 4440,
    "repo": "mono/SkiaSharp",
    "analyzedAt": "2026-07-16T05:12:32Z",
    "currentLabels": [
      "type/bug"
    ]
  },
  "summary": "SkiaSharp.NativeAssets.WebAssembly fails to link on .NET 11 preview 6+ because the pre-built WASM static library was compiled with Emscripten 3.1.56 which uses saveSetjmp/testSetjmp, but .NET 11 ships Emscripten 5.0.6 which replaced those symbols with __wasm_setjmp/__wasm_setjmp_test.",
  "classification": {
    "type": {
      "value": "type/bug",
      "confidence": 0.95
    },
    "area": {
      "value": "area/libSkiaSharp.native",
      "confidence": 0.92
    },
    "platforms": [
      "os/WASM"
    ],
    "tenets": [
      "tenet/compatibility"
    ]
  },
  "evidence": {
    "bugSignals": {
      "severity": "high",
      "regressionClaimed": false,
      "errorType": "build-error",
      "errorMessage": "undefined symbol: saveSetjmp / undefined symbol: testSetjmp",
      "reproQuality": "complete",
      "targetFrameworks": [
        "net11.0-browser-wasm"
      ]
    },
    "reproEvidence": {
      "stepsToReproduce": [
        "Create a Blazor WASM or browser-wasm project targeting net11.0",
        "Add SkiaSharp 3.116.0 or 3.119.4 NuGet package",
        "Run dotnet publish -c Release",
        "Observe linker errors: undefined symbol: saveSetjmp, undefined symbol: testSetjmp"
      ],
      "environmentDetails": "macOS, VS Code, .NET 11.0-preview.6, Emscripten 5.0.6 (from Microsoft.NET.Runtime.Emscripten.5.0.6.Sdk.osx-arm64)",
      "repoLinks": []
    },
    "versionAnalysis": {
      "mentionedVersions": [
        "3.116.0",
        "3.119.4",
        "4.151.0-preview.2.1"
      ],
      "currentRelevance": "likely",
      "relevanceReason": "SkiaSharp.targets only references emscripten 3.1.34 (net8.0) and 3.1.56 (net9.0+) native binaries; no net11.0 / emsdk 5.x native assets exist yet."
    }
  },
  "analysis": {
    "summary": "SkiaSharp ships pre-built WASM static libraries compiled with Emscripten 3.1.56 for net9.0+, but .NET 11 preview 6+ bundles Emscripten 5.0.6, which removed the legacy setjmp symbols (saveSetjmp, testSetjmp) in favour of Wasm-native equivalents (__wasm_setjmp, __wasm_setjmp_test). The linker fails because there is an ABI mismatch between the pre-built .a files and the host emcc version. A new build of libSkiaSharp.a with Emscripten 5.0.6 is required and the SkiaSharp.targets file must be updated to select the correct artifact for net11.0+.",
    "codeInvestigation": [
      {
        "file": "binding/SkiaSharp.NativeAssets.WebAssembly/buildTransitive/SkiaSharp.targets",
        "lines": "32-35",
        "finding": "NativeFileReference for net9.0+ always points to emscripten 3.1.56 artifacts. There is no condition for net11.0 / emscripten 5.x, so net11.0 picks up the incompatible 3.1.56 binary.",
        "relevance": "direct"
      },
      {
        "file": "scripts/azure-templates-jobs-wasm-matrix.yml",
        "finding": "CI matrix builds only for emscripten 3.1.34 and 3.1.56. No build target for emscripten 5.0.6 exists yet.",
        "relevance": "direct"
      },
      {
        "file": "native/wasm/build.cake",
        "lines": "7-8",
        "finding": "EMSCRIPTEN_VERSION is controlled by env/arg. The build infrastructure already supports versioned output directories, so adding a 5.0.6 build is feasible.",
        "relevance": "related"
      }
    ],
    "keySignals": [
      {
        "text": "undefined symbol: saveSetjmp / undefined symbol: testSetjmp",
        "source": "issue body build log",
        "interpretation": "Emscripten 5.x removed the legacy SJLJ symbols. The pre-built .a was compiled with emsdk 3.1.56 which still had them."
      },
      {
        "text": "emscripten 5.0.6 (from Microsoft.NET.Runtime.Emscripten.5.0.6.Sdk)",
        "source": "issue body build log",
        "interpretation": ".NET 11 preview 6+ upgraded its bundled Emscripten from 3.x to 5.0.6, creating an ABI mismatch."
      },
      {
        "text": "Same error on 4.151.0-preview.2.1 version",
        "source": "comment #1",
        "interpretation": "Latest SkiaSharp preview also does not have a 5.x build, confirming the issue is not version-specific within SkiaSharp releases."
      },
      {
        "text": "replace saveSetjmp and testSetjmp on __wasm_setjmp and __wasm_setjmp_test",
        "source": "issue body expected behavior",
        "interpretation": "Reporter correctly identifies the root cause and expected fix."
      }
    ],
    "rationale": "This is clearly a build compatibility bug: .NET 11 preview 6+ ships Emscripten 5.0.6 which is ABI-incompatible with SkiaSharp's pre-built WASM static libraries compiled with Emscripten 3.1.56. The SkiaSharp.targets file lacks a net11.0 entry pointing to a 5.x-compiled artifact. The fix requires building libSkiaSharp.a / libHarfBuzzSharp.a with Emscripten 5.0.6 and updating SkiaSharp.targets to select those artifacts for net11.0+.",
    "nextQuestions": [
      "Which exact Emscripten version does .NET 11 RTM plan to ship? (5.0.6 or later?)",
      "Should the 5.x build also cover SIMD and Threading variants?",
      "Does the -fwasm-exceptions / WASM_LEGACY_EXCEPTIONS=0 flag combination used by .NET 11 require a matching build flag in the native artifact?"
    ],
    "resolution": {
      "hypothesis": "Build libSkiaSharp.a and libHarfBuzzSharp.a with Emscripten 5.0.6 and add a net11.0 condition to SkiaSharp.targets.",
      "proposals": [
        {
          "title": "Add Emscripten 5.0.6 WASM build variant",
          "description": "Extend the CI matrix in azure-templates-jobs-wasm-matrix.yml with emscripten 5.0.6 entries (st, simd, mt, simd+mt). Update SkiaSharp.targets to add a NativeFileReference for net11.0+ pointing to the 5.0.6 artifacts.",
          "category": "fix",
          "confidence": 0.9,
          "effort": "cost/m",
          "validated": "untested"
        },
        {
          "title": "Workaround: stay on .NET 10",
          "description": "Until a 5.x build is shipped, use net10.0-browser-wasm instead of net11.0-browser-wasm for WASM projects using SkiaSharp. .NET 10 bundles Emscripten 3.1.56 which matches the current SkiaSharp artifacts.",
          "category": "workaround",
          "confidence": 0.95,
          "effort": "cost/xs",
          "validated": "untested"
        }
      ],
      "recommendedProposal": "Add Emscripten 5.0.6 WASM build variant",
      "recommendedReason": "Workaround is temporary; .NET 11 will ship and users will need native support. The build infrastructure is already parameterized by emscripten version, so the work is incremental."
    }
  },
  "output": {
    "actionability": {
      "suggestedAction": "needs-investigation",
      "confidence": 0.9,
      "reason": "Root cause is clear (ABI mismatch between emsdk 3.1.56 and 5.0.6), but the fix requires native CI pipeline work. Needs a maintainer to scope the 5.x build effort and verify .NET 11 final Emscripten version.",
      "suggestedReproPlatform": "linux"
    },
    "actions": [
      {
        "type": "update-labels",
        "description": "Apply area/libSkiaSharp.native, os/WASM, tenet/compatibility",
        "risk": "low",
        "confidence": 0.95,
        "labels": [
          "type/bug",
          "area/libSkiaSharp.native",
          "os/WASM",
          "tenet/compatibility"
        ]
      },
      {
        "type": "add-comment",
        "description": "Acknowledge root cause and provide workaround",
        "risk": "medium",
        "confidence": 0.9,
        "comment": "Thanks for the detailed report!\n\nThe root cause is an **Emscripten ABI mismatch**: SkiaSharp's pre-built WASM static libraries (`.a` files) are compiled with Emscripten **3.1.56**, which uses `saveSetjmp`/`testSetjmp`. .NET 11 preview 6+ bundles Emscripten **5.0.6**, which replaced those symbols with `__wasm_setjmp`/`__wasm_setjmp_test`. The linker cannot find the old symbols in the new toolchain.\n\nA fix requires building new `libSkiaSharp.a` / `libHarfBuzzSharp.a` artifacts with Emscripten 5.0.6 and updating `SkiaSharp.targets` with a `net11.0` condition.\n\n**Workaround (while you wait):** target `net10.0` for WASM instead of `net11.0`. .NET 10 ships Emscripten 3.1.56 which matches the current SkiaSharp artifacts.\n\nThis issue is tracked for the next SkiaSharp release once .NET 11's Emscripten version is finalized."
      }
    ]
  }
}
```

</details>
