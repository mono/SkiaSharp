# Issue Triage Report — #4165

| Field | Value |
|-------|-------|
| Repository | mono/SkiaSharp |
| Analyzed | 2026-06-17T05:55:00Z |
| Type | type/bug (0.98 (98%)) |
| Area | area/Build (0.95 (95%)) |
| Suggested action | ready-to-fix (0.90 (90%)) |

**Issue Summary:** Since v3.119.2, SkiaSharp.NativeAssets.Win32 inadvertently includes three copies of libSkiaSharp.pdb (~80 MB each) in the main NuGet package, increasing the package size from 12.74 MB to 71 MB+ and causing app crashes when the .pdb files are removed because they are listed as required entries in the deps.json manifest.

**Analysis:** The wildcard pattern `libSkiaSharp*` in binding/SkiaSharp.NativeAssets.Win32/SkiaSharp.NativeAssets.Win32.csproj matches both libSkiaSharp.dll and libSkiaSharp.pdb. The addition of `.pdb` to `AllowedOutputExtensionsInPackageBuildOutputFolder` in source/SkiaSharp.NuGet.targets (commit 6a7a452) enables the pack task to include these native debug symbols in the main NuGet package under runtimes/{rid}/native/. Because they are registered in the package as runtime assets, NuGet records them in deps.json as required files, causing app crashes if the .pdb files are removed during deployment.

**Recommendations:** **ready-to-fix** — Root cause is clearly identified (wildcard + AllowedOutputExtensions interaction), the regression boundary is known (3.119.1 good, 3.119.2-preview.1 broken), the files to fix are identified, and a minimal fix is described. This is in an RC milestone (4.150.0-rc.1) so it should be fixed before release.

---

## Classification

| Field | Value |
|-------|-------|
| Type | type/bug |
| Area | area/Build |
| Platforms | os/Windows-Classic |
| Backends | — |
| Tenets | tenet/reliability |
| Partner | — |
| Current labels | type/bug |

## Evidence

### Reproduction

1. Reference SkiaSharp.NativeAssets.Win32 3.119.2-preview.1 or later
2. Inspect the package contents on NuGet.org or locally
3. Observe libSkiaSharp.pdb under runtimes/win-x64/native, runtimes/win-x86/native, runtimes/win-arm64/native
4. Attempt to deploy the app without the .pdb files
5. Observe crash due to deps.json listing libSkiaSharp.pdb as a required asset

**Environment:** SkiaSharp.NativeAssets.Win32 3.119.2-preview.1+; Windows x64/x86/arm64; Visual Studio on Windows

**Repository links:**
- https://www.nuget.org/packages/SkiaSharp.NativeAssets.Win32/3.119.1 — 3.119.1 package (12.74 MB, correct)
- https://www.nuget.org/packages/SkiaSharp.NativeAssets.Win32/3.119.2-preview.1 — 3.119.2-preview.1 package (71+ MB, includes .pdb)
- https://github.com/mono/SkiaSharp/commit/6a7a452af8142fe2daa2f6160472790be7697af0 — Suspected root-cause commit: adds .pdb to AllowedOutputExtensionsInPackageBuildOutputFolder

### Bug Signals

| Field | Value |
|-------|-------|
| Severity | high |
| Regression claimed | True |
| Error type | wrong-output |
| Error message | Could not find 'aspnetcorev2_inprocess.dll'. Exception message: Error: An assembly specified in the application dependencies manifest (Rest.deps.json) was not found: package: 'SkiaSharp.NativeAssets.Win32', version: '4.147.0-preview.3.1' path: 'runtimes/win-x86/native/libSkiaSharp.pdb' |
| Repro quality | complete |
| Target frameworks | net8.0-windows |

### Version Analysis

| Field | Value |
|-------|-------|
| Mentioned versions | 3.119.1, 3.119.2-preview.1, 4.147.0-preview.3.1 |
| Worked in | 3.119.1 |
| Broke in | 3.119.2-preview.1 |
| Current relevance | likely |
| Relevance reason | The packaging targets containing the suspected root cause are still present unchanged in the current tree. |

### Regression

| Field | Value |
|-------|-------|
| Is regression | True |
| Confidence | 0.98 (98%) |
| Reason | Package was 12.74 MB in 3.119.1; 71 MB+ in 3.119.2-preview.1. Reporter identifies specific commit that added .pdb to AllowedOutputExtensionsInPackageBuildOutputFolder as the cause. |
| Worked in version | 3.119.1 |
| Broke in version | 3.119.2-preview.1 |

## Analysis

### Technical Summary

The wildcard pattern `libSkiaSharp*` in binding/SkiaSharp.NativeAssets.Win32/SkiaSharp.NativeAssets.Win32.csproj matches both libSkiaSharp.dll and libSkiaSharp.pdb. The addition of `.pdb` to `AllowedOutputExtensionsInPackageBuildOutputFolder` in source/SkiaSharp.NuGet.targets (commit 6a7a452) enables the pack task to include these native debug symbols in the main NuGet package under runtimes/{rid}/native/. Because they are registered in the package as runtime assets, NuGet records them in deps.json as required files, causing app crashes if the .pdb files are removed during deployment.

### Rationale

This is a clear packaging regression: a specific version boundary (3.119.1 good, 3.119.2-preview.1 broken), a identified likely-cause commit, NuGet package evidence, and a confirmed secondary crash symptom from deps.json. The code investigation confirms the wildcard + AllowedOutputExtensions interaction. Fix path is well-defined.

### Key Signals

- "SkiaSharp.NativeAssets.Win32 v3.119.1 is 12.74 MB. v3.119.2-preview.1 and later are greater than 71MB." — **issue body** (Clear size regression — native .pdb files (~80 MB each × 3 RIDs) are being included in the main package.)
- "deleting .pdb files after build/publish does not help. The application crashes with the following error: Error: An assembly specified in the application dependencies manifest was not found: path: 'runtimes/win-x86/native/libSkiaSharp.pdb'" — **comment #1 (yaroslavlsd)** (The .pdb files are registered as required runtime assets in deps.json — not optional. Removing them post-publish breaks the application at startup.)
- "I suspect the issue might have been caused by this change in source/SkiaSharp.NuGet.targets" — **issue body** (Reporter correctly identifies AllowedOutputExtensionsInPackageBuildOutputFolder adding .pdb as the likely cause.)

### Code Investigation

| File | Lines | Relevance | Finding |
|------|-------|-----------|---------|
| `source/SkiaSharp.NuGet.targets` | 5 | direct | AllowedOutputExtensionsInPackageBuildOutputFolder is extended with .pdb and .so. Adding .pdb here allows native .pdb files (C++ debug symbols) picked up by the wildcard in NativeAssets.Win32 to pass through into the main package. |
| `binding/SkiaSharp.NativeAssets.Win32/SkiaSharp.NativeAssets.Win32.csproj` | 9-11 | direct | NativeAssetPackageFile items use the glob pattern `libSkiaSharp*` for each of win-x64, win-x86, win-arm64. This wildcard matches both libSkiaSharp.dll (the desired binary) and libSkiaSharp.pdb (the native C++ debug symbols, ~80 MB each). |
| `binding/NativeAssets.Build.targets` | 42-54 | direct | The _IncludeRuntimesPackageFiles target correctly separates files: .pdb/.dbg go into _TargetPathsToSymbols (for symbols packages) and are excluded from _BuildOutputInPackage. However, if the AllowedOutputExtensionsInPackageBuildOutputFolder interaction causes an additional code path to include .pdb (or if IncludeSymbols=true causes _TargetPathsToSymbols to merge into the main package), the .pdb files still end up in the main package. |
| `binding/NativeAssets.Build.targets` | 28-32 | related | IsNativeAssetsProject sets IncludeBuildOutput=true and IncludeSymbols=true, with BuildOutputTargetFolder=runtimes. IncludeSymbols=true may cause _TargetPathsToSymbols items (which include .pdb) to be merged into the main package when no separate .symbols.nupkg is generated. |

### Workarounds

- Pin to SkiaSharp.NativeAssets.Win32 3.119.1 until a fix is released.
- Do not delete libSkiaSharp.pdb files from the deployment output — include them alongside the .dll (not ideal due to size but prevents the deps.json crash).

### Next Questions

- Does the same issue affect HarfBuzzSharp.NativeAssets.Win32 (which also uses a wildcard pattern)?
- Does the issue affect other native asset packages (macOS, Linux) that also produce .pdb/.dbg files?
- Is IncludeSymbols=true in NativeAssets.Build.targets intentional — should a separate .symbols.nupkg be generated instead?

### Resolution Proposals

**Hypothesis:** The wildcard `libSkiaSharp*` in NativeAssets.Win32 picks up libSkiaSharp.pdb, and the AllowedOutputExtensionsInPackageBuildOutputFolder change causes these native debug symbols to be included in the main package (and registered in deps.json as required assets).

1. **Restrict wildcard to exclude .pdb in NativeAssets.Win32.csproj** — fix, confidence 0.85 (85%), cost/xs, validated=untested
   - Change the NativeAssetPackageFile glob from `libSkiaSharp*` to `libSkiaSharp.dll` (or use `Exclude='*.pdb'`) so only the DLL is included as a native runtime asset.
2. **Remove .pdb from AllowedOutputExtensionsInPackageBuildOutputFolder** — fix, confidence 0.80 (80%), cost/xs, validated=untested
   - Remove `;.pdb` from AllowedOutputExtensionsInPackageBuildOutputFolder in source/SkiaSharp.NuGet.targets to prevent native .pdb files from being auto-included. May require a different mechanism to include managed .pdb files if that was the intent.
3. **Explicitly exclude .pdb from _TargetPathsToSymbols for native assets packages** — fix, confidence 0.75 (75%), cost/s, validated=untested
   - Modify binding/NativeAssets.Build.targets to ensure native .pdb files never appear in the main package — clear _TargetPathsToSymbols or set IncludeSymbols=false for IsNativeAssetsProject to prevent symbols from merging into the main package.

**Recommended proposal:** Restrict wildcard to exclude .pdb in NativeAssets.Win32.csproj

**Why:** Smallest scope change, directly prevents the wrong files from being registered as native assets. The other proposals may have unintended side effects on managed .pdb or symbol package generation.

## Recommendations

### Actionability

| Field | Value |
|-------|-------|
| Suggested action | ready-to-fix |
| Confidence | 0.90 (90%) |
| Reason | Root cause is clearly identified (wildcard + AllowedOutputExtensions interaction), the regression boundary is known (3.119.1 good, 3.119.2-preview.1 broken), the files to fix are identified, and a minimal fix is described. This is in an RC milestone (4.150.0-rc.1) so it should be fixed before release. |
| Suggested repro platform | windows |

### Automatable Actions

| Type | Risk | Confidence | Description | Details |
|------|------|------------|-------------|---------|
| update-labels | low | 0.95 (95%) | Add area/Build and os/Windows-Classic, tenet/reliability labels (type/bug already present) | labels=type/bug, area/Build, os/Windows-Classic, tenet/reliability |
| add-comment | medium | 0.88 (88%) | Confirm root cause analysis and direct toward fix location | — |

**Comment draft for `add-comment`:**

```markdown
Thanks for the detailed report and for identifying the commit!

Confirmed: the regression was introduced by commit `6a7a452` which added `.pdb` to `AllowedOutputExtensionsInPackageBuildOutputFolder` in `source/SkiaSharp.NuGet.targets`. This extension, combined with the `libSkiaSharp*` wildcard in `binding/SkiaSharp.NativeAssets.Win32/SkiaSharp.NativeAssets.Win32.csproj`, causes the native C++ debug symbols (`libSkiaSharp.pdb`, ~80 MB each) to be included in the main package for all three Windows RIDs (win-x64, win-x86, win-arm64).

As the second commenter noted, the .pdb files are also registered in `deps.json` as required runtime assets, so removing them post-publish causes the app to fail at startup.

**Workaround until a fix ships:** Pin to `SkiaSharp.NativeAssets.Win32 3.119.1` if the package size is blocking you.

The fix is to restrict the wildcard or exclude `.pdb` from the native asset glob so only the `.dll` is packaged as a runtime asset.
```

<details>
<summary>Raw JSON</summary>

```json
{
  "meta": {
    "schemaVersion": "1.0",
    "number": 4165,
    "repo": "mono/SkiaSharp",
    "analyzedAt": "2026-06-17T05:55:00Z",
    "currentLabels": [
      "type/bug"
    ]
  },
  "summary": "Since v3.119.2, SkiaSharp.NativeAssets.Win32 inadvertently includes three copies of libSkiaSharp.pdb (~80 MB each) in the main NuGet package, increasing the package size from 12.74 MB to 71 MB+ and causing app crashes when the .pdb files are removed because they are listed as required entries in the deps.json manifest.",
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
      "os/Windows-Classic"
    ],
    "tenets": [
      "tenet/reliability"
    ]
  },
  "evidence": {
    "bugSignals": {
      "severity": "high",
      "regressionClaimed": true,
      "errorType": "wrong-output",
      "errorMessage": "Could not find 'aspnetcorev2_inprocess.dll'. Exception message: Error: An assembly specified in the application dependencies manifest (Rest.deps.json) was not found: package: 'SkiaSharp.NativeAssets.Win32', version: '4.147.0-preview.3.1' path: 'runtimes/win-x86/native/libSkiaSharp.pdb'",
      "reproQuality": "complete",
      "targetFrameworks": [
        "net8.0-windows"
      ]
    },
    "reproEvidence": {
      "stepsToReproduce": [
        "Reference SkiaSharp.NativeAssets.Win32 3.119.2-preview.1 or later",
        "Inspect the package contents on NuGet.org or locally",
        "Observe libSkiaSharp.pdb under runtimes/win-x64/native, runtimes/win-x86/native, runtimes/win-arm64/native",
        "Attempt to deploy the app without the .pdb files",
        "Observe crash due to deps.json listing libSkiaSharp.pdb as a required asset"
      ],
      "environmentDetails": "SkiaSharp.NativeAssets.Win32 3.119.2-preview.1+; Windows x64/x86/arm64; Visual Studio on Windows",
      "repoLinks": [
        {
          "url": "https://www.nuget.org/packages/SkiaSharp.NativeAssets.Win32/3.119.1",
          "description": "3.119.1 package (12.74 MB, correct)"
        },
        {
          "url": "https://www.nuget.org/packages/SkiaSharp.NativeAssets.Win32/3.119.2-preview.1",
          "description": "3.119.2-preview.1 package (71+ MB, includes .pdb)"
        },
        {
          "url": "https://github.com/mono/SkiaSharp/commit/6a7a452af8142fe2daa2f6160472790be7697af0",
          "description": "Suspected root-cause commit: adds .pdb to AllowedOutputExtensionsInPackageBuildOutputFolder"
        }
      ]
    },
    "versionAnalysis": {
      "mentionedVersions": [
        "3.119.1",
        "3.119.2-preview.1",
        "4.147.0-preview.3.1"
      ],
      "workedIn": "3.119.1",
      "brokeIn": "3.119.2-preview.1",
      "currentRelevance": "likely",
      "relevanceReason": "The packaging targets containing the suspected root cause are still present unchanged in the current tree."
    },
    "regression": {
      "isRegression": true,
      "confidence": 0.98,
      "reason": "Package was 12.74 MB in 3.119.1; 71 MB+ in 3.119.2-preview.1. Reporter identifies specific commit that added .pdb to AllowedOutputExtensionsInPackageBuildOutputFolder as the cause.",
      "workedInVersion": "3.119.1",
      "brokeInVersion": "3.119.2-preview.1"
    }
  },
  "analysis": {
    "summary": "The wildcard pattern `libSkiaSharp*` in binding/SkiaSharp.NativeAssets.Win32/SkiaSharp.NativeAssets.Win32.csproj matches both libSkiaSharp.dll and libSkiaSharp.pdb. The addition of `.pdb` to `AllowedOutputExtensionsInPackageBuildOutputFolder` in source/SkiaSharp.NuGet.targets (commit 6a7a452) enables the pack task to include these native debug symbols in the main NuGet package under runtimes/{rid}/native/. Because they are registered in the package as runtime assets, NuGet records them in deps.json as required files, causing app crashes if the .pdb files are removed during deployment.",
    "rationale": "This is a clear packaging regression: a specific version boundary (3.119.1 good, 3.119.2-preview.1 broken), a identified likely-cause commit, NuGet package evidence, and a confirmed secondary crash symptom from deps.json. The code investigation confirms the wildcard + AllowedOutputExtensions interaction. Fix path is well-defined.",
    "keySignals": [
      {
        "text": "SkiaSharp.NativeAssets.Win32 v3.119.1 is 12.74 MB. v3.119.2-preview.1 and later are greater than 71MB.",
        "source": "issue body",
        "interpretation": "Clear size regression — native .pdb files (~80 MB each × 3 RIDs) are being included in the main package."
      },
      {
        "text": "deleting .pdb files after build/publish does not help. The application crashes with the following error: Error: An assembly specified in the application dependencies manifest was not found: path: 'runtimes/win-x86/native/libSkiaSharp.pdb'",
        "source": "comment #1 (yaroslavlsd)",
        "interpretation": "The .pdb files are registered as required runtime assets in deps.json — not optional. Removing them post-publish breaks the application at startup."
      },
      {
        "text": "I suspect the issue might have been caused by this change in source/SkiaSharp.NuGet.targets",
        "source": "issue body",
        "interpretation": "Reporter correctly identifies AllowedOutputExtensionsInPackageBuildOutputFolder adding .pdb as the likely cause."
      }
    ],
    "codeInvestigation": [
      {
        "file": "source/SkiaSharp.NuGet.targets",
        "lines": "5",
        "finding": "AllowedOutputExtensionsInPackageBuildOutputFolder is extended with .pdb and .so. Adding .pdb here allows native .pdb files (C++ debug symbols) picked up by the wildcard in NativeAssets.Win32 to pass through into the main package.",
        "relevance": "direct"
      },
      {
        "file": "binding/SkiaSharp.NativeAssets.Win32/SkiaSharp.NativeAssets.Win32.csproj",
        "lines": "9-11",
        "finding": "NativeAssetPackageFile items use the glob pattern `libSkiaSharp*` for each of win-x64, win-x86, win-arm64. This wildcard matches both libSkiaSharp.dll (the desired binary) and libSkiaSharp.pdb (the native C++ debug symbols, ~80 MB each).",
        "relevance": "direct"
      },
      {
        "file": "binding/NativeAssets.Build.targets",
        "lines": "42-54",
        "finding": "The _IncludeRuntimesPackageFiles target correctly separates files: .pdb/.dbg go into _TargetPathsToSymbols (for symbols packages) and are excluded from _BuildOutputInPackage. However, if the AllowedOutputExtensionsInPackageBuildOutputFolder interaction causes an additional code path to include .pdb (or if IncludeSymbols=true causes _TargetPathsToSymbols to merge into the main package), the .pdb files still end up in the main package.",
        "relevance": "direct"
      },
      {
        "file": "binding/NativeAssets.Build.targets",
        "lines": "28-32",
        "finding": "IsNativeAssetsProject sets IncludeBuildOutput=true and IncludeSymbols=true, with BuildOutputTargetFolder=runtimes. IncludeSymbols=true may cause _TargetPathsToSymbols items (which include .pdb) to be merged into the main package when no separate .symbols.nupkg is generated.",
        "relevance": "related"
      }
    ],
    "workarounds": [
      "Pin to SkiaSharp.NativeAssets.Win32 3.119.1 until a fix is released.",
      "Do not delete libSkiaSharp.pdb files from the deployment output — include them alongside the .dll (not ideal due to size but prevents the deps.json crash)."
    ],
    "nextQuestions": [
      "Does the same issue affect HarfBuzzSharp.NativeAssets.Win32 (which also uses a wildcard pattern)?",
      "Does the issue affect other native asset packages (macOS, Linux) that also produce .pdb/.dbg files?",
      "Is IncludeSymbols=true in NativeAssets.Build.targets intentional — should a separate .symbols.nupkg be generated instead?"
    ],
    "resolution": {
      "hypothesis": "The wildcard `libSkiaSharp*` in NativeAssets.Win32 picks up libSkiaSharp.pdb, and the AllowedOutputExtensionsInPackageBuildOutputFolder change causes these native debug symbols to be included in the main package (and registered in deps.json as required assets).",
      "proposals": [
        {
          "title": "Restrict wildcard to exclude .pdb in NativeAssets.Win32.csproj",
          "description": "Change the NativeAssetPackageFile glob from `libSkiaSharp*` to `libSkiaSharp.dll` (or use `Exclude='*.pdb'`) so only the DLL is included as a native runtime asset.",
          "category": "fix",
          "confidence": 0.85,
          "effort": "cost/xs",
          "validated": "untested"
        },
        {
          "title": "Remove .pdb from AllowedOutputExtensionsInPackageBuildOutputFolder",
          "description": "Remove `;.pdb` from AllowedOutputExtensionsInPackageBuildOutputFolder in source/SkiaSharp.NuGet.targets to prevent native .pdb files from being auto-included. May require a different mechanism to include managed .pdb files if that was the intent.",
          "category": "fix",
          "confidence": 0.8,
          "effort": "cost/xs",
          "validated": "untested"
        },
        {
          "title": "Explicitly exclude .pdb from _TargetPathsToSymbols for native assets packages",
          "description": "Modify binding/NativeAssets.Build.targets to ensure native .pdb files never appear in the main package — clear _TargetPathsToSymbols or set IncludeSymbols=false for IsNativeAssetsProject to prevent symbols from merging into the main package.",
          "category": "fix",
          "confidence": 0.75,
          "effort": "cost/s",
          "validated": "untested"
        }
      ],
      "recommendedProposal": "Restrict wildcard to exclude .pdb in NativeAssets.Win32.csproj",
      "recommendedReason": "Smallest scope change, directly prevents the wrong files from being registered as native assets. The other proposals may have unintended side effects on managed .pdb or symbol package generation."
    }
  },
  "output": {
    "actionability": {
      "suggestedAction": "ready-to-fix",
      "confidence": 0.9,
      "reason": "Root cause is clearly identified (wildcard + AllowedOutputExtensions interaction), the regression boundary is known (3.119.1 good, 3.119.2-preview.1 broken), the files to fix are identified, and a minimal fix is described. This is in an RC milestone (4.150.0-rc.1) so it should be fixed before release.",
      "suggestedReproPlatform": "windows"
    },
    "actions": [
      {
        "type": "update-labels",
        "description": "Add area/Build and os/Windows-Classic, tenet/reliability labels (type/bug already present)",
        "risk": "low",
        "confidence": 0.95,
        "labels": [
          "type/bug",
          "area/Build",
          "os/Windows-Classic",
          "tenet/reliability"
        ]
      },
      {
        "type": "add-comment",
        "description": "Confirm root cause analysis and direct toward fix location",
        "risk": "medium",
        "confidence": 0.88,
        "comment": "Thanks for the detailed report and for identifying the commit!\n\nConfirmed: the regression was introduced by commit `6a7a452` which added `.pdb` to `AllowedOutputExtensionsInPackageBuildOutputFolder` in `source/SkiaSharp.NuGet.targets`. This extension, combined with the `libSkiaSharp*` wildcard in `binding/SkiaSharp.NativeAssets.Win32/SkiaSharp.NativeAssets.Win32.csproj`, causes the native C++ debug symbols (`libSkiaSharp.pdb`, ~80 MB each) to be included in the main package for all three Windows RIDs (win-x64, win-x86, win-arm64).\n\nAs the second commenter noted, the .pdb files are also registered in `deps.json` as required runtime assets, so removing them post-publish causes the app to fail at startup.\n\n**Workaround until a fix ships:** Pin to `SkiaSharp.NativeAssets.Win32 3.119.1` if the package size is blocking you.\n\nThe fix is to restrict the wildcard or exclude `.pdb` from the native asset glob so only the `.dll` is packaged as a runtime asset."
      }
    ]
  }
}
```

</details>
