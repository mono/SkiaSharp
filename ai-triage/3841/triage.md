# Issue Triage Report — #3841

| Field | Value |
|-------|-------|
| Repository | mono/SkiaSharp |
| Analyzed | 2026-05-05T08:35:00Z |
| Type | type/bug (0.98 (98%)) |
| Area | area/Build (0.95 (95%)) |
| Suggested action | ready-to-fix (0.92 (92%)) |

**Issue Summary:** Uno Gallery sample fails in CI because Uno SDK injects SkiaSharp 3.x native assets (via the SkiaRenderer UnoFeature), conflicting with the 4.x packages and causing WASM link errors and Resizetizer version mismatch crashes.

**Analysis:** The Uno SDK (Uno.Sdk/6.6.0-dev.208) injects a direct PackageReference for SkiaSharp.Skottie 3.x via its Uno.Implicit.Packages.ProjectSystem.Uno.targets when UnoFeatures includes SkiaRenderer. The sample csproj pins SkiaSharp.Skottie with ExcludeAssets=all to prevent asset leakage, but the Uno SDK's injection creates a separate reference not covered by that override, so NuGet resolves SkiaSharp.NativeAssets.WebAssembly 3.119.2 instead of 4.x. This causes undefined symbol errors in the WASM linker (3.x native lib lacks 4.x C API symbols) and a Resizetizer version mismatch crash on Windows.

**Recommendations:** **ready-to-fix** — Root cause is clearly identified, reporter reproduced it locally, CI logs confirm the exact wrong package version, and concrete fix strategies are provided.

---

## Classification

| Field | Value |
|-------|-------|
| Type | type/bug |
| Area | area/Build |
| Platforms | os/WASM, os/Windows-Classic |
| Backends | — |
| Tenets | tenet/compatibility, tenet/reliability |
| Partner | partner/unoplatform |

## Evidence

### Reproduction

1. dotnet build output/samples/Gallery/Uno/SkiaSharpSample.Uno.csproj -f:net10.0-browserwasm

**Repository links:**
- https://dev.azure.com/xamarin/6fd3d886-57a5-4e31-8db7-52a1b47c07a8/_build/results?buildId=157354 — ADO Build #157354 — Samples stage, Linux and Windows jobs showing CI failures.

**Code snippets:**

```csharp
wasm-ld : error : lto.tmp: undefined symbol: sk_pathbuilder_close
wasm-ld : error : lto.tmp: undefined symbol: sk_pathbuilder_conic_to
wasm-ld : error : too many errors emitted
```

### Bug Signals

| Field | Value |
|-------|-------|
| Severity | high |
| Regression claimed | False |
| Error type | build-error |
| Error message | wasm-ld: undefined symbol: sk_pathbuilder_close; The version of the native libSkiaSharp library (116.0) is incompatible with this version of SkiaSharp. Supported versions are in range [88.1, 89.0) |
| Repro quality | complete |
| Target frameworks | net10.0-browserwasm, net10.0-windows10.0.26100 |

### Version Analysis

| Field | Value |
|-------|-------|
| Mentioned versions | 3.119.2, 4.147.0, 6.6.0-dev.208 |
| Worked in | — |
| Broke in | — |
| Current relevance | likely |
| Relevance reason | Issue was filed against the current main branch; CI builds confirm it is actively failing. |

## Analysis

### Technical Summary

The Uno SDK (Uno.Sdk/6.6.0-dev.208) injects a direct PackageReference for SkiaSharp.Skottie 3.x via its Uno.Implicit.Packages.ProjectSystem.Uno.targets when UnoFeatures includes SkiaRenderer. The sample csproj pins SkiaSharp.Skottie with ExcludeAssets=all to prevent asset leakage, but the Uno SDK's injection creates a separate reference not covered by that override, so NuGet resolves SkiaSharp.NativeAssets.WebAssembly 3.119.2 instead of 4.x. This causes undefined symbol errors in the WASM linker (3.x native lib lacks 4.x C API symbols) and a Resizetizer version mismatch crash on Windows.

### Rationale

Classified as type/bug area/Build because the Uno Gallery CI build is broken due to a dependency resolution conflict introduced by the Uno SDK's automatic SkiaSharp injection. The issue is build-side (NuGet resolution), not a SkiaSharp runtime defect. partner/unoplatform applies because the root cause originates in Uno SDK behavior. Severity is high: this blocks CI. reproQuality is complete because the reporter reproduced it locally with full linker output. suggestedAction is ready-to-fix because root cause and concrete fix strategies are clearly described.

### Key Signals

- "skiasharp.nativeassets.webassembly/3.119.2 — used by Uno Gallery sample" — **issue body** (NuGet resolves the 3.x native asset package for WASM instead of 4.x due to the Uno SDK injection.)
- "wasm-ld : error : lto.tmp: undefined symbol: sk_pathbuilder_close" — **issue body** (3.x native lib is missing C API symbols added in SkiaSharp 4.x, confirming the wrong version is being linked.)
- "The version of the native libSkiaSharp library (116.0) is incompatible with this version of SkiaSharp. Supported versions are in range [88.1, 89.0)" — **issue body** (Resizetizer picks up the 3.x native (milestone 116) but SkiaSharp.dll is 4.x — the version guard fires.)

### Code Investigation

| File | Lines | Relevance | Finding |
|------|-------|-----------|---------|
| `samples/Gallery/Uno/SkiaSharpSample.Uno.csproj` | 44-65 | direct | The csproj pins SkiaSharp.Skottie with ExcludeAssets=all (lines 58-59) to suppress the 3.x floor set by the Uno SDK injected reference, but the Uno SDK still injects a separate direct reference via Uno.Implicit.Packages.ProjectSystem.Uno.targets that this override does not suppress. The IncludeNativeAssets.SkiaSharp.targets import at line 61 provides the correct 4.x native library for in-tree builds. |
| `binding/IncludeNativeAssets.SkiaSharp.targets` | — | direct | The targets file injects local 4.x native library outputs for all TFMs including WASM (browserwasm) and Windows. This is correctly imported in the Uno sample csproj, but if NuGet also resolves a 3.x native asset package from the Uno SDK injection, the 3.x libSkiaSharp.a can win the WASM link step. |

### Next Questions

- Does adding a PackageReference for SkiaSharp.NativeAssets.WebAssembly 4.x with ExcludeAssets=all (same pattern as Skottie) resolve the WASM linker errors?
- Does the Windows Resizetizer crash also disappear once the native assets resolve to 4.x?
- Which exact packages does Uno.Implicit.Packages.ProjectSystem.Uno.targets inject beyond SkiaSharp.Skottie?

### Resolution Proposals

**Hypothesis:** The Uno SDK's UnoFeatures: SkiaRenderer injects SkiaSharp 3.x native assets as a direct PackageReference without ExcludeAssets, overriding the sample's workaround. Adding explicit 4.x PackageReference overrides for all Uno-injected SkiaSharp native asset packages should fix both CI failures.

1. **Pin Uno-injected native asset packages to 4.x** — fix, cost/s, validated=untested
   - Add explicit PackageReference entries for SkiaSharp.NativeAssets.WebAssembly and other Uno-injected SkiaSharp packages pinned to 4.x with ExcludeAssets=all in the Uno sample csproj, so NuGet's conflict resolution always picks the 4.x version.
2. **Use Directory.Packages.props to centrally pin SkiaSharp versions** — workaround, cost/s, validated=untested
   - Add a PackageVersion override in a Directory.Packages.props to clamp all SkiaSharp packages to 4.x across the whole samples tree, preventing Uno SDK injection from downgrading them.
3. **File upstream Uno SDK issue for version override respect** — investigation, cost/xs, validated=untested
   - File an issue with the Uno Platform SDK team requesting that Uno.Implicit.Packages.ProjectSystem.Uno.targets respect existing direct PackageReference overrides or allow the SkiaSharp version to be controlled via a property.

**Recommended proposal:** Pin Uno-injected native asset packages to 4.x

**Why:** Adding explicit 4.x PackageReference overrides is the smallest targeted change, keeps the SkiaRenderer feature, and aligns with how the existing Skottie pin was intended to work.

## Recommendations

### Actionability

| Field | Value |
|-------|-------|
| Suggested action | ready-to-fix |
| Confidence | 0.92 (92%) |
| Reason | Root cause is clearly identified, reporter reproduced it locally, CI logs confirm the exact wrong package version, and concrete fix strategies are provided. |
| Suggested repro platform | linux |

### Automatable Actions

| Type | Risk | Confidence | Description | Details |
|------|------|------------|-------------|---------|
| update-labels | low | 0.97 (97%) | Apply classification labels | labels=type/bug, area/Build, os/WASM, os/Windows-Classic, tenet/compatibility, tenet/reliability, partner/unoplatform |
| add-comment | medium | 0.90 (90%) | Acknowledge the report and suggest the fix approach. | — |

**Comment draft for `add-comment`:**

```markdown
Thanks for the detailed report and local reproduction steps!

Root cause confirmed: the Uno SDK's `SkiaRenderer` UnoFeature injects `SkiaSharp.Skottie` (and possibly other SkiaSharp native asset packages) as a direct `PackageReference` via `Uno.Implicit.Packages.ProjectSystem.Uno.targets`, and this injected reference doesn't have `ExcludeAssets=all`, so NuGet resolves `skiasharp.nativeassets.webassembly/3.119.2` for the WASM target instead of the 4.x version.

**Suggested fix:** Add explicit `PackageReference` overrides in `samples/Gallery/Uno/SkiaSharpSample.Uno.csproj` for any Uno-injected SkiaSharp native asset packages (similar to the existing `SkiaSharp.Skottie` pin, but also for `SkiaSharp.NativeAssets.WebAssembly`), setting them to 4.x with `ExcludeAssets=all`. Alternatively, moving to Central Package Management (`Directory.Packages.props`) would let one version declaration cover all injected packages.
```

<details>
<summary>Raw JSON</summary>

```json
{
  "meta": {
    "schemaVersion": "1.0",
    "number": 3841,
    "repo": "mono/SkiaSharp",
    "analyzedAt": "2026-05-05T08:35:00Z"
  },
  "summary": "Uno Gallery sample fails in CI because Uno SDK injects SkiaSharp 3.x native assets (via the SkiaRenderer UnoFeature), conflicting with the 4.x packages and causing WASM link errors and Resizetizer version mismatch crashes.",
  "classification": {
    "type": {
      "value": "type/bug",
      "confidence": 0.98
    },
    "area": {
      "value": "area/Build",
      "confidence": 0.95
    },
    "platforms": [
      "os/WASM",
      "os/Windows-Classic"
    ],
    "tenets": [
      "tenet/compatibility",
      "tenet/reliability"
    ],
    "partner": "partner/unoplatform"
  },
  "evidence": {
    "bugSignals": {
      "severity": "high",
      "regressionClaimed": false,
      "errorType": "build-error",
      "errorMessage": "wasm-ld: undefined symbol: sk_pathbuilder_close; The version of the native libSkiaSharp library (116.0) is incompatible with this version of SkiaSharp. Supported versions are in range [88.1, 89.0)",
      "reproQuality": "complete",
      "targetFrameworks": [
        "net10.0-browserwasm",
        "net10.0-windows10.0.26100"
      ]
    },
    "reproEvidence": {
      "stepsToReproduce": [
        "dotnet build output/samples/Gallery/Uno/SkiaSharpSample.Uno.csproj -f:net10.0-browserwasm"
      ],
      "codeSnippets": [
        "wasm-ld : error : lto.tmp: undefined symbol: sk_pathbuilder_close\nwasm-ld : error : lto.tmp: undefined symbol: sk_pathbuilder_conic_to\nwasm-ld : error : too many errors emitted"
      ],
      "repoLinks": [
        {
          "url": "https://dev.azure.com/xamarin/6fd3d886-57a5-4e31-8db7-52a1b47c07a8/_build/results?buildId=157354",
          "description": "ADO Build #157354 — Samples stage, Linux and Windows jobs showing CI failures."
        }
      ]
    },
    "versionAnalysis": {
      "mentionedVersions": [
        "3.119.2",
        "4.147.0",
        "6.6.0-dev.208"
      ],
      "currentRelevance": "likely",
      "relevanceReason": "Issue was filed against the current main branch; CI builds confirm it is actively failing."
    }
  },
  "analysis": {
    "summary": "The Uno SDK (Uno.Sdk/6.6.0-dev.208) injects a direct PackageReference for SkiaSharp.Skottie 3.x via its Uno.Implicit.Packages.ProjectSystem.Uno.targets when UnoFeatures includes SkiaRenderer. The sample csproj pins SkiaSharp.Skottie with ExcludeAssets=all to prevent asset leakage, but the Uno SDK's injection creates a separate reference not covered by that override, so NuGet resolves SkiaSharp.NativeAssets.WebAssembly 3.119.2 instead of 4.x. This causes undefined symbol errors in the WASM linker (3.x native lib lacks 4.x C API symbols) and a Resizetizer version mismatch crash on Windows.",
    "codeInvestigation": [
      {
        "file": "samples/Gallery/Uno/SkiaSharpSample.Uno.csproj",
        "finding": "The csproj pins SkiaSharp.Skottie with ExcludeAssets=all (lines 58-59) to suppress the 3.x floor set by the Uno SDK injected reference, but the Uno SDK still injects a separate direct reference via Uno.Implicit.Packages.ProjectSystem.Uno.targets that this override does not suppress. The IncludeNativeAssets.SkiaSharp.targets import at line 61 provides the correct 4.x native library for in-tree builds.",
        "relevance": "direct",
        "lines": "44-65"
      },
      {
        "file": "binding/IncludeNativeAssets.SkiaSharp.targets",
        "finding": "The targets file injects local 4.x native library outputs for all TFMs including WASM (browserwasm) and Windows. This is correctly imported in the Uno sample csproj, but if NuGet also resolves a 3.x native asset package from the Uno SDK injection, the 3.x libSkiaSharp.a can win the WASM link step.",
        "relevance": "direct"
      }
    ],
    "keySignals": [
      {
        "text": "skiasharp.nativeassets.webassembly/3.119.2 — used by Uno Gallery sample",
        "source": "issue body",
        "interpretation": "NuGet resolves the 3.x native asset package for WASM instead of 4.x due to the Uno SDK injection."
      },
      {
        "text": "wasm-ld : error : lto.tmp: undefined symbol: sk_pathbuilder_close",
        "source": "issue body",
        "interpretation": "3.x native lib is missing C API symbols added in SkiaSharp 4.x, confirming the wrong version is being linked."
      },
      {
        "text": "The version of the native libSkiaSharp library (116.0) is incompatible with this version of SkiaSharp. Supported versions are in range [88.1, 89.0)",
        "source": "issue body",
        "interpretation": "Resizetizer picks up the 3.x native (milestone 116) but SkiaSharp.dll is 4.x — the version guard fires."
      }
    ],
    "rationale": "Classified as type/bug area/Build because the Uno Gallery CI build is broken due to a dependency resolution conflict introduced by the Uno SDK's automatic SkiaSharp injection. The issue is build-side (NuGet resolution), not a SkiaSharp runtime defect. partner/unoplatform applies because the root cause originates in Uno SDK behavior. Severity is high: this blocks CI. reproQuality is complete because the reporter reproduced it locally with full linker output. suggestedAction is ready-to-fix because root cause and concrete fix strategies are clearly described.",
    "resolution": {
      "hypothesis": "The Uno SDK's UnoFeatures: SkiaRenderer injects SkiaSharp 3.x native assets as a direct PackageReference without ExcludeAssets, overriding the sample's workaround. Adding explicit 4.x PackageReference overrides for all Uno-injected SkiaSharp native asset packages should fix both CI failures.",
      "proposals": [
        {
          "title": "Pin Uno-injected native asset packages to 4.x",
          "description": "Add explicit PackageReference entries for SkiaSharp.NativeAssets.WebAssembly and other Uno-injected SkiaSharp packages pinned to 4.x with ExcludeAssets=all in the Uno sample csproj, so NuGet's conflict resolution always picks the 4.x version.",
          "category": "fix",
          "effort": "cost/s",
          "validated": "untested"
        },
        {
          "title": "Use Directory.Packages.props to centrally pin SkiaSharp versions",
          "description": "Add a PackageVersion override in a Directory.Packages.props to clamp all SkiaSharp packages to 4.x across the whole samples tree, preventing Uno SDK injection from downgrading them.",
          "category": "workaround",
          "effort": "cost/s",
          "validated": "untested"
        },
        {
          "title": "File upstream Uno SDK issue for version override respect",
          "description": "File an issue with the Uno Platform SDK team requesting that Uno.Implicit.Packages.ProjectSystem.Uno.targets respect existing direct PackageReference overrides or allow the SkiaSharp version to be controlled via a property.",
          "category": "investigation",
          "effort": "cost/xs",
          "validated": "untested"
        }
      ],
      "recommendedProposal": "Pin Uno-injected native asset packages to 4.x",
      "recommendedReason": "Adding explicit 4.x PackageReference overrides is the smallest targeted change, keeps the SkiaRenderer feature, and aligns with how the existing Skottie pin was intended to work."
    },
    "nextQuestions": [
      "Does adding a PackageReference for SkiaSharp.NativeAssets.WebAssembly 4.x with ExcludeAssets=all (same pattern as Skottie) resolve the WASM linker errors?",
      "Does the Windows Resizetizer crash also disappear once the native assets resolve to 4.x?",
      "Which exact packages does Uno.Implicit.Packages.ProjectSystem.Uno.targets inject beyond SkiaSharp.Skottie?"
    ]
  },
  "output": {
    "actionability": {
      "suggestedAction": "ready-to-fix",
      "confidence": 0.92,
      "reason": "Root cause is clearly identified, reporter reproduced it locally, CI logs confirm the exact wrong package version, and concrete fix strategies are provided.",
      "suggestedReproPlatform": "linux"
    },
    "actions": [
      {
        "type": "update-labels",
        "description": "Apply classification labels",
        "risk": "low",
        "confidence": 0.97,
        "labels": [
          "type/bug",
          "area/Build",
          "os/WASM",
          "os/Windows-Classic",
          "tenet/compatibility",
          "tenet/reliability",
          "partner/unoplatform"
        ]
      },
      {
        "type": "add-comment",
        "description": "Acknowledge the report and suggest the fix approach.",
        "risk": "medium",
        "confidence": 0.9,
        "comment": "Thanks for the detailed report and local reproduction steps!\n\nRoot cause confirmed: the Uno SDK's `SkiaRenderer` UnoFeature injects `SkiaSharp.Skottie` (and possibly other SkiaSharp native asset packages) as a direct `PackageReference` via `Uno.Implicit.Packages.ProjectSystem.Uno.targets`, and this injected reference doesn't have `ExcludeAssets=all`, so NuGet resolves `skiasharp.nativeassets.webassembly/3.119.2` for the WASM target instead of the 4.x version.\n\n**Suggested fix:** Add explicit `PackageReference` overrides in `samples/Gallery/Uno/SkiaSharpSample.Uno.csproj` for any Uno-injected SkiaSharp native asset packages (similar to the existing `SkiaSharp.Skottie` pin, but also for `SkiaSharp.NativeAssets.WebAssembly`), setting them to 4.x with `ExcludeAssets=all`. Alternatively, moving to Central Package Management (`Directory.Packages.props`) would let one version declaration cover all injected packages."
      }
    ]
  }
}
```

</details>
