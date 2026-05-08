# Issue Triage Report — #3286

| Field | Value |
|-------|-------|
| Repository | mono/SkiaSharp |
| Analyzed | 2026-04-28T09:04:33Z |
| Type | type/bug (0.93 (93%)) |
| Area | area/Build (0.88 (88%)) |
| Suggested action | needs-investigation (0.87 (87%)) |

**Issue Summary:** Debug symbols (PDB) for SkiaSharp 3.116.1 and 3.119.0 native Windows DLL are missing from both the Microsoft Symbol Server and NuGet symbol server, preventing debugger symbol resolution.

**Analysis:** The Windows native libSkiaSharp.pdb is generated during build (via /Z7 and /DEBUG:FULL flags, then explicitly copied to the output directory), and the NativeAssets.Build.targets mechanism routes .pdb files into the _TargetPathsToSymbols item group for inclusion in the .snupkg. The 403 from symbols.nuget.org may indicate the snupkg was uploaded but lacks the PDB, or the PDB GUID doesn't match. The 404 from the MS symbol server confirms the PDB is not indexed there. The root cause is either the PDB not being included in the published snupkg, or the snupkg not being pushed to either symbol server during the 3.x release pipeline.

**Recommendations:** **needs-investigation** — The build infrastructure for PDB generation and snupkg packaging appears correct, but symbols are missing from both public servers for all tested 3.x releases. A maintainer needs to check the release publish pipeline to confirm snupkg files are being pushed to NuGet.org.

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

1. Install dotnet-symbol tool
2. Run: dotnet-symbol ~/.nuget/packages/skiasharp.nativeassets.win32/3.119.0/runtimes/win-x64/native/libSkiaSharp.dll
3. Observe: 404 Not Found on msdl.microsoft.com
4. Run with --server-path https://symbols.nuget.org/download/symbols
5. Observe: 403 Forbidden on symbols.nuget.org

**Environment:** SkiaSharp 3.119.0 and 3.116.1 on Windows x64. Symbols for 2.88.0 work on both servers.

### Bug Signals

| Field | Value |
|-------|-------|
| Severity | medium |
| Regression claimed | True |
| Error type | missing-output |
| Error message | ERROR: Not Found / 403 Forbidden for libskiasharp.pdb on symbol servers |
| Repro quality | complete |
| Target frameworks | net9.0 |

### Version Analysis

| Field | Value |
|-------|-------|
| Mentioned versions | 2.88.0, 3.116.1, 3.119.0 |
| Worked in | 2.88.0 |
| Broke in | — |
| Current relevance | likely |
| Relevance reason | Reporter confirms both 3.116.1 and 3.119.0 are missing symbols; 2.88.0 symbols resolve correctly on both servers. |

### Regression

| Field | Value |
|-------|-------|
| Is regression | True |
| Confidence | 0.85 (85%) |
| Reason | Symbols were available for 2.88.0 on both servers but are missing for all 3.x releases tested (3.116.1, 3.119.0). |
| Worked in version | 2.88.0 |
| Broke in version | 3.116.1 |

## Analysis

### Technical Summary

The Windows native libSkiaSharp.pdb is generated during build (via /Z7 and /DEBUG:FULL flags, then explicitly copied to the output directory), and the NativeAssets.Build.targets mechanism routes .pdb files into the _TargetPathsToSymbols item group for inclusion in the .snupkg. The 403 from symbols.nuget.org may indicate the snupkg was uploaded but lacks the PDB, or the PDB GUID doesn't match. The 404 from the MS symbol server confirms the PDB is not indexed there. The root cause is either the PDB not being included in the published snupkg, or the snupkg not being pushed to either symbol server during the 3.x release pipeline.

### Rationale

This is a build/release infrastructure bug: the code properly generates and packages PDB files, but the native debug symbols for 3.x Windows packages are not reaching the public symbol servers. The 403 from NuGet vs 404 from MS indicates the snupkg may be partially published. This is classified as area/Build because the root cause is in the CI/CD release pipeline rather than the library code itself.

### Key Signals

- "ERROR: Not Found: libskiasharp.pdb - 'https://msdl.microsoft.com/download/symbols/libskiasharp.pdb/ac13267c46d9bc874c4c44205044422e1/libskiasharp.pdb'" — **issue body** (PDB with this specific GUID is not indexed on the MS symbol server — either not published or published without this PDB.)
- "ERROR: 403 Forbidden: libskiasharp.pdb - 'https://symbols.nuget.org/download/symbols/...'" — **issue body** (403 from NuGet symbols may mean the snupkg was uploaded but does not contain the PDB file, or was not published at all. 403 vs 404 distinction is important: NuGet returns 403 for packages that exist but the specific symbol is not found/forbidden.)
- "Symbols for 2.88.0 can be downloaded from both servers" — **issue body** (Confirmed regression — the publishing mechanism worked for the 2.x line but is broken for 3.x.)
- "Same for 3.116.1" — **comment by kekekeks** (Affects multiple 3.x releases, not just 3.119.0 — pattern of consistent omission in the 3.x release pipeline.)

### Code Investigation

| File | Lines | Relevance | Finding |
|------|-------|-----------|---------|
| `native/windows/build.cake` | 79-86 | direct | Windows native build uses /Z7 (embedded debug info) and /DEBUG:FULL linker flag, and explicitly copies libSkiaSharp.pdb to the output directory — PDB generation IS correctly configured. |
| `binding/NativeAssets.Build.targets` | 28-52 | direct | IsNativeAssetsProject sets IncludeSymbols=true and routes .pdb/.dbg files into _TargetPathsToSymbols (snupkg symbols path) while non-PDB files go into _BuildOutputInPackage. Mechanism to include PDB in snupkg is in place. |
| `binding/SkiaSharp.NativeAssets.Win32/SkiaSharp.NativeAssets.Win32.csproj` | 9-11 | direct | Uses glob pattern libSkiaSharp* which captures both .dll and .pdb files from output/native/windows/{arch}/. PDB should be picked up automatically. |
| `build.cake` | 495-499 | related | .snupkg files are moved to output/nugets-symbols/ directory after packing. The separation from signed packages is intentional. |
| `scripts/azure-templates-stages-package.yml` | 70-83 | related | nugets-symbols artifact is collected and published in CI. Whether snupkg files in this artifact are pushed to NuGet.org/symbols.nuget.org as part of the release publish step is unclear from available pipeline files. |

### Next Questions

- Are the .snupkg files for SkiaSharp.NativeAssets.Win32 actually being pushed to NuGet.org during the 3.x release? Check the release-publish pipeline step.
- Does the snupkg artifact contain the libSkiaSharp.pdb file? Can be verified by extracting the snupkg from a 3.119.0 release artifact.
- Was there a change in the release pipeline between 2.88.x and 3.x that stopped pushing snupkg to symbol servers?
- Are symbols missing for other platforms (Linux .dbg, macOS .dSYM) or only Windows?

### Resolution Proposals

**Hypothesis:** The .snupkg for SkiaSharp.NativeAssets.Win32 is either not being pushed to NuGet.org during the 3.x release process, or the PDB files are not being included in the snupkg due to a packaging configuration gap introduced in the 3.x build system.

1. **Verify and fix snupkg publishing in release pipeline** — investigation, confidence 0.80 (80%), cost/s, validated=untested
   - Check the release-publish step to ensure .snupkg files from nugets-symbols artifact are included in the NuGet push command. For NuGet.org, snupkg must be pushed separately or with --include-symbols. Also verify the snupkg actually contains the libSkiaSharp.pdb by extracting a recent artifact.
2. **Inspect built snupkg content** — investigation, confidence 0.85 (85%), cost/xs, validated=untested
   - Extract the SkiaSharp.NativeAssets.Win32.*.snupkg from the CI artifact of a 3.119.0 build and verify it contains runtimes/win-x64/native/libSkiaSharp.pdb. If absent, the issue is in NativeAssets.Build.targets or the pack step.

**Recommended proposal:** Verify and fix snupkg publishing in release pipeline

**Why:** The pipeline files show snupkg files are moved to nugets-symbols but it's unclear if they're pushed to NuGet.org. Verifying the publish step first addresses the most likely gap.

## Recommendations

### Actionability

| Field | Value |
|-------|-------|
| Suggested action | needs-investigation |
| Confidence | 0.87 (87%) |
| Reason | The build infrastructure for PDB generation and snupkg packaging appears correct, but symbols are missing from both public servers for all tested 3.x releases. A maintainer needs to check the release publish pipeline to confirm snupkg files are being pushed to NuGet.org. |
| Suggested repro platform | windows |

### Automatable Actions

| Type | Risk | Confidence | Description | Details |
|------|------|------------|-------------|---------|
| update-labels | low | 0.90 (90%) | Apply bug, build, and Windows labels | labels=type/bug, area/Build, os/Windows-Classic, tenet/reliability |
| add-comment | medium | 0.87 (87%) | Acknowledge the issue and describe investigation steps | — |

**Comment draft for `add-comment`:**

```markdown
Thanks for the detailed reproduction steps. The 403 from `symbols.nuget.org` and 404 from `msdl.microsoft.com` both confirm the PDB for `libSkiaSharp.dll` was not indexed on public symbol servers for 3.x releases.

The build pipeline is configured to generate and package PDB files (`/DEBUG:FULL` + `/Z7` flags, `NativeAssets.Build.targets` routes `.pdb` into the `.snupkg`), but it appears the symbol packages may not be reaching the public servers during the 3.x release workflow.

We'll investigate whether the `.snupkg` for `SkiaSharp.NativeAssets.Win32` is being pushed to NuGet.org as part of the release process.
```

<details>
<summary>Raw JSON</summary>

```json
{
  "meta": {
    "schemaVersion": "1.0",
    "number": 3286,
    "repo": "mono/SkiaSharp",
    "analyzedAt": "2026-04-28T09:04:33Z",
    "currentLabels": [
      "type/bug"
    ]
  },
  "summary": "Debug symbols (PDB) for SkiaSharp 3.116.1 and 3.119.0 native Windows DLL are missing from both the Microsoft Symbol Server and NuGet symbol server, preventing debugger symbol resolution.",
  "classification": {
    "type": {
      "value": "type/bug",
      "confidence": 0.93
    },
    "area": {
      "value": "area/Build",
      "confidence": 0.88
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
      "severity": "medium",
      "regressionClaimed": true,
      "errorType": "missing-output",
      "errorMessage": "ERROR: Not Found / 403 Forbidden for libskiasharp.pdb on symbol servers",
      "reproQuality": "complete",
      "targetFrameworks": [
        "net9.0"
      ]
    },
    "reproEvidence": {
      "stepsToReproduce": [
        "Install dotnet-symbol tool",
        "Run: dotnet-symbol ~/.nuget/packages/skiasharp.nativeassets.win32/3.119.0/runtimes/win-x64/native/libSkiaSharp.dll",
        "Observe: 404 Not Found on msdl.microsoft.com",
        "Run with --server-path https://symbols.nuget.org/download/symbols",
        "Observe: 403 Forbidden on symbols.nuget.org"
      ],
      "environmentDetails": "SkiaSharp 3.119.0 and 3.116.1 on Windows x64. Symbols for 2.88.0 work on both servers.",
      "repoLinks": []
    },
    "versionAnalysis": {
      "mentionedVersions": [
        "2.88.0",
        "3.116.1",
        "3.119.0"
      ],
      "workedIn": "2.88.0",
      "currentRelevance": "likely",
      "relevanceReason": "Reporter confirms both 3.116.1 and 3.119.0 are missing symbols; 2.88.0 symbols resolve correctly on both servers."
    },
    "regression": {
      "isRegression": true,
      "confidence": 0.85,
      "reason": "Symbols were available for 2.88.0 on both servers but are missing for all 3.x releases tested (3.116.1, 3.119.0).",
      "workedInVersion": "2.88.0",
      "brokeInVersion": "3.116.1"
    }
  },
  "analysis": {
    "summary": "The Windows native libSkiaSharp.pdb is generated during build (via /Z7 and /DEBUG:FULL flags, then explicitly copied to the output directory), and the NativeAssets.Build.targets mechanism routes .pdb files into the _TargetPathsToSymbols item group for inclusion in the .snupkg. The 403 from symbols.nuget.org may indicate the snupkg was uploaded but lacks the PDB, or the PDB GUID doesn't match. The 404 from the MS symbol server confirms the PDB is not indexed there. The root cause is either the PDB not being included in the published snupkg, or the snupkg not being pushed to either symbol server during the 3.x release pipeline.",
    "codeInvestigation": [
      {
        "file": "native/windows/build.cake",
        "lines": "79-86",
        "finding": "Windows native build uses /Z7 (embedded debug info) and /DEBUG:FULL linker flag, and explicitly copies libSkiaSharp.pdb to the output directory — PDB generation IS correctly configured.",
        "relevance": "direct"
      },
      {
        "file": "binding/NativeAssets.Build.targets",
        "lines": "28-52",
        "finding": "IsNativeAssetsProject sets IncludeSymbols=true and routes .pdb/.dbg files into _TargetPathsToSymbols (snupkg symbols path) while non-PDB files go into _BuildOutputInPackage. Mechanism to include PDB in snupkg is in place.",
        "relevance": "direct"
      },
      {
        "file": "binding/SkiaSharp.NativeAssets.Win32/SkiaSharp.NativeAssets.Win32.csproj",
        "lines": "9-11",
        "finding": "Uses glob pattern libSkiaSharp* which captures both .dll and .pdb files from output/native/windows/{arch}/. PDB should be picked up automatically.",
        "relevance": "direct"
      },
      {
        "file": "build.cake",
        "lines": "495-499",
        "finding": ".snupkg files are moved to output/nugets-symbols/ directory after packing. The separation from signed packages is intentional.",
        "relevance": "related"
      },
      {
        "file": "scripts/azure-templates-stages-package.yml",
        "lines": "70-83",
        "finding": "nugets-symbols artifact is collected and published in CI. Whether snupkg files in this artifact are pushed to NuGet.org/symbols.nuget.org as part of the release publish step is unclear from available pipeline files.",
        "relevance": "related"
      }
    ],
    "keySignals": [
      {
        "text": "ERROR: Not Found: libskiasharp.pdb - 'https://msdl.microsoft.com/download/symbols/libskiasharp.pdb/ac13267c46d9bc874c4c44205044422e1/libskiasharp.pdb'",
        "source": "issue body",
        "interpretation": "PDB with this specific GUID is not indexed on the MS symbol server — either not published or published without this PDB."
      },
      {
        "text": "ERROR: 403 Forbidden: libskiasharp.pdb - 'https://symbols.nuget.org/download/symbols/...'",
        "source": "issue body",
        "interpretation": "403 from NuGet symbols may mean the snupkg was uploaded but does not contain the PDB file, or was not published at all. 403 vs 404 distinction is important: NuGet returns 403 for packages that exist but the specific symbol is not found/forbidden."
      },
      {
        "text": "Symbols for 2.88.0 can be downloaded from both servers",
        "source": "issue body",
        "interpretation": "Confirmed regression — the publishing mechanism worked for the 2.x line but is broken for 3.x."
      },
      {
        "text": "Same for 3.116.1",
        "source": "comment by kekekeks",
        "interpretation": "Affects multiple 3.x releases, not just 3.119.0 — pattern of consistent omission in the 3.x release pipeline."
      }
    ],
    "rationale": "This is a build/release infrastructure bug: the code properly generates and packages PDB files, but the native debug symbols for 3.x Windows packages are not reaching the public symbol servers. The 403 from NuGet vs 404 from MS indicates the snupkg may be partially published. This is classified as area/Build because the root cause is in the CI/CD release pipeline rather than the library code itself.",
    "nextQuestions": [
      "Are the .snupkg files for SkiaSharp.NativeAssets.Win32 actually being pushed to NuGet.org during the 3.x release? Check the release-publish pipeline step.",
      "Does the snupkg artifact contain the libSkiaSharp.pdb file? Can be verified by extracting the snupkg from a 3.119.0 release artifact.",
      "Was there a change in the release pipeline between 2.88.x and 3.x that stopped pushing snupkg to symbol servers?",
      "Are symbols missing for other platforms (Linux .dbg, macOS .dSYM) or only Windows?"
    ],
    "resolution": {
      "hypothesis": "The .snupkg for SkiaSharp.NativeAssets.Win32 is either not being pushed to NuGet.org during the 3.x release process, or the PDB files are not being included in the snupkg due to a packaging configuration gap introduced in the 3.x build system.",
      "proposals": [
        {
          "title": "Verify and fix snupkg publishing in release pipeline",
          "description": "Check the release-publish step to ensure .snupkg files from nugets-symbols artifact are included in the NuGet push command. For NuGet.org, snupkg must be pushed separately or with --include-symbols. Also verify the snupkg actually contains the libSkiaSharp.pdb by extracting a recent artifact.",
          "category": "investigation",
          "confidence": 0.8,
          "effort": "cost/s",
          "validated": "untested"
        },
        {
          "title": "Inspect built snupkg content",
          "description": "Extract the SkiaSharp.NativeAssets.Win32.*.snupkg from the CI artifact of a 3.119.0 build and verify it contains runtimes/win-x64/native/libSkiaSharp.pdb. If absent, the issue is in NativeAssets.Build.targets or the pack step.",
          "category": "investigation",
          "confidence": 0.85,
          "effort": "cost/xs",
          "validated": "untested"
        }
      ],
      "recommendedProposal": "Verify and fix snupkg publishing in release pipeline",
      "recommendedReason": "The pipeline files show snupkg files are moved to nugets-symbols but it's unclear if they're pushed to NuGet.org. Verifying the publish step first addresses the most likely gap."
    }
  },
  "output": {
    "actionability": {
      "suggestedAction": "needs-investigation",
      "confidence": 0.87,
      "reason": "The build infrastructure for PDB generation and snupkg packaging appears correct, but symbols are missing from both public servers for all tested 3.x releases. A maintainer needs to check the release publish pipeline to confirm snupkg files are being pushed to NuGet.org.",
      "suggestedReproPlatform": "windows"
    },
    "actions": [
      {
        "type": "update-labels",
        "description": "Apply bug, build, and Windows labels",
        "risk": "low",
        "confidence": 0.9,
        "labels": [
          "type/bug",
          "area/Build",
          "os/Windows-Classic",
          "tenet/reliability"
        ]
      },
      {
        "type": "add-comment",
        "description": "Acknowledge the issue and describe investigation steps",
        "risk": "medium",
        "confidence": 0.87,
        "comment": "Thanks for the detailed reproduction steps. The 403 from `symbols.nuget.org` and 404 from `msdl.microsoft.com` both confirm the PDB for `libSkiaSharp.dll` was not indexed on public symbol servers for 3.x releases.\n\nThe build pipeline is configured to generate and package PDB files (`/DEBUG:FULL` + `/Z7` flags, `NativeAssets.Build.targets` routes `.pdb` into the `.snupkg`), but it appears the symbol packages may not be reaching the public servers during the 3.x release workflow.\n\nWe'll investigate whether the `.snupkg` for `SkiaSharp.NativeAssets.Win32` is being pushed to NuGet.org as part of the release process."
      }
    ]
  }
}
```

</details>
