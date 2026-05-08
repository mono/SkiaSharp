# Issue Triage Report — #3119

| Field | Value |
|-------|-------|
| Repository | mono/SkiaSharp |
| Analyzed | 2026-04-21T18:35:44Z |
| Type | type/bug (0.90 (90%)) |
| Area | area/libSkiaSharp.native (0.88 (88%)) |
| Suggested action | close-as-external (0.88 (88%)) |

**Issue Summary:** EntryPointNotFoundException for 'sk_region_op2' when FreeSpire.XLS 14.2.0 (built against SkiaSharp 2.88.x) is used alongside SkiaSharp.NativeAssets.Linux 3.116.x, where the native symbol was renamed to 'sk_region_op' in the 3.x series.

**Analysis:** FreeSpire.XLS 14.2.0 embeds or depends on SkiaSharp 2.88.x managed bindings that P/Invoke 'sk_region_op2'. SkiaSharp 3.x renamed this C API entry point to 'sk_region_op'. The user is providing the 3.x native library which does not export 'sk_region_op2', causing an EntryPointNotFoundException at startup. This is a version mismatch between a third-party library's SkiaSharp 2.88.x dependency and the user's SkiaSharp 3.x native assets.

**Recommendations:** **close-as-external** — The EntryPointNotFoundException is caused by FreeSpire.XLS (a third-party library) using the old SkiaSharp 2.88.x P/Invoke symbol sk_region_op2 with the 3.x native library. SkiaSharp correctly renamed the symbol in 3.x. The fix must come from FreeSpire.XLS updating its SkiaSharp dependency.

---

## Classification

| Field | Value |
|-------|-------|
| Type | type/bug |
| Area | area/libSkiaSharp.native |
| Platforms | os/Linux |
| Backends | — |
| Tenets | tenet/compatibility |
| Partner | — |
| Current labels | type/bug |

## Evidence

### Reproduction

1. Add FreeSpire.XLS 14.2.0 and SkiaSharp.NativeAssets.Linux 3.116.1 to a .NET project
2. Run on Linux (e.g., Docker mcr.microsoft.com/dotnet/aspnet:8.0)
3. Instantiate 'new Workbook()'

**Environment:** Docker Linux (mcr.microsoft.com/dotnet/aspnet:8.0), FreeSpire.XLS 14.2.0, SkiaSharp.NativeAssets.Linux 3.116.1

**Repository links:**
- https://github.com/WouterDevExperts/SkiaSharpBug — Reporter's minimal repro project

### Bug Signals

| Field | Value |
|-------|-------|
| Severity | medium |
| Regression claimed | True |
| Error type | exception |
| Error message | System.EntryPointNotFoundException: Unable to find an entry point named 'sk_region_op2' in shared library 'libSkiaSharp' |
| Repro quality | complete |
| Target frameworks | net8.0 |

**Stack trace:**

```text
at SkiaSharp.SkiaApi.sk_region_op2(IntPtr r, IntPtr src, SKRegionOperation op)
at SkiaSharp.SKRegion.Op(SKRegion region, SKRegionOperation op)
[... Spire.Xls internal frames ...]
```

### Version Analysis

| Field | Value |
|-------|-------|
| Mentioned versions | 2.88.9, 3.116.0, 3.116.1 |
| Worked in | 2.88.9 |
| Broke in | 3.116.1 |
| Current relevance | likely |
| Relevance reason | The 3.x native library exports sk_region_op (not sk_region_op2); FreeSpire.XLS was compiled against the 2.88.x SkiaSharp managed assembly, which called sk_region_op2. |

### Regression

| Field | Value |
|-------|-------|
| Is regression | True |
| Confidence | 0.92 (92%) |
| Reason | The 3.x SkiaSharp C API renamed sk_region_op2 to sk_region_op. FreeSpire.XLS bundles its own SkiaSharp 2.88.x managed DLL that P/Invokes the old symbol name, so upgrading native assets to 3.x breaks it. |
| Worked in version | 2.88.9 |
| Broke in version | 3.116.1 |

## Analysis

### Technical Summary

FreeSpire.XLS 14.2.0 embeds or depends on SkiaSharp 2.88.x managed bindings that P/Invoke 'sk_region_op2'. SkiaSharp 3.x renamed this C API entry point to 'sk_region_op'. The user is providing the 3.x native library which does not export 'sk_region_op2', causing an EntryPointNotFoundException at startup. This is a version mismatch between a third-party library's SkiaSharp 2.88.x dependency and the user's SkiaSharp 3.x native assets.

### Rationale

The error is a clear EntryPointNotFoundException for a known-renamed symbol (sk_region_op2 → sk_region_op in 3.x). Code investigation confirms SkiaSharp 3.x bindings only export sk_region_op. The crash originates from FreeSpire.XLS's internal usage of the old SkiaSharp 2.88.x P/Invoke binding. A maintainer comment confirmed FreeSpire.XLS uses an older Skia version. The fix must come from FreeSpire.XLS updating to SkiaSharp 3.x.

### Key Signals

- "Unable to find an entry point named 'sk_region_op2' in shared library 'libSkiaSharp'" — **issue body / stack trace** (The 3.x native library does not export sk_region_op2; FreeSpire.XLS calls the 2.88.x symbol name.)
- "It works fine in the previous version: SkiaSharp.NativeAssets.Linux.NoDependencies version 2.88.9" — **issue body** (Confirms FreeSpire.XLS was built against SkiaSharp 2.88.x which still exported sk_region_op2.)
- "Spire.Xls looks like it uses a much older version of Skia - I'd probably not expect the sample to work." — **comment by molesmoke (community)** (Third-party library compatibility gap — FreeSpire.XLS has not updated to SkiaSharp 3.x.)

### Code Investigation

| File | Lines | Relevance | Finding |
|------|-------|-----------|---------|
| `binding/SkiaSharp/SkiaApi.generated.cs` | 12481-12500 | direct | SkiaSharp 3.x generated bindings define sk_region_op (not sk_region_op2). There is no sk_region_op2 symbol exported in the 3.x native library. The rename happened between 2.88.x and 3.x as part of the C API cleanup. |
| `binding/SkiaSharp/SKRegion.cs` | 213-214 | direct | SKRegion.Op(SKRegion, SKRegionOperation) calls SkiaApi.sk_region_op — confirming the current 3.x managed binding uses sk_region_op, not sk_region_op2. |

### Workarounds

- Pin SkiaSharp.NativeAssets.Linux to version 2.88.9 to match what FreeSpire.XLS expects, until FreeSpire.XLS publishes a version that supports SkiaSharp 3.x.
- Contact E-iceblue (FreeSpire vendor) and request an update to FreeSpire.XLS that targets SkiaSharp 3.x.

### Next Questions

- Has FreeSpire.XLS published a newer version (>14.2.0) that targets SkiaSharp 3.x?
- Is it feasible to add a SkiaSharp 2.88.x compatibility shim that re-exports sk_region_op2 as an alias?

### Resolution Proposals

**Hypothesis:** FreeSpire.XLS 14.2.0 was compiled against SkiaSharp 2.88.x and P/Invokes sk_region_op2 which no longer exists in the SkiaSharp 3.x native library.

1. **Pin native assets to 2.88.9** — workaround, confidence 0.95 (95%), cost/xs, validated=untested
   - Downgrade SkiaSharp.NativeAssets.Linux to 2.88.9 to match FreeSpire.XLS's SkiaSharp 2.88.x dependency. This is a stopgap until FreeSpire.XLS upgrades.
2. **Update FreeSpire.XLS** — fix, confidence 0.80 (80%), cost/s, validated=untested
   - Check if a newer version of FreeSpire.XLS supports SkiaSharp 3.x, or request the vendor update the dependency.

**Recommended proposal:** Pin native assets to 2.88.9

**Why:** Immediate workaround that resolves the issue; the root fix must come from FreeSpire.XLS vendor.

## Recommendations

### Actionability

| Field | Value |
|-------|-------|
| Suggested action | close-as-external |
| Confidence | 0.88 (88%) |
| Reason | The EntryPointNotFoundException is caused by FreeSpire.XLS (a third-party library) using the old SkiaSharp 2.88.x P/Invoke symbol sk_region_op2 with the 3.x native library. SkiaSharp correctly renamed the symbol in 3.x. The fix must come from FreeSpire.XLS updating its SkiaSharp dependency. |
| Suggested repro platform | linux |

### Automatable Actions

| Type | Risk | Confidence | Description | Details |
|------|------|------------|-------------|---------|
| update-labels | low | 0.95 (95%) | Apply bug, native, linux, and compatibility labels | labels=type/bug, area/libSkiaSharp.native, os/Linux, tenet/compatibility |
| add-comment | high | 0.88 (88%) | Explain the version mismatch and provide the workaround | — |
| close-issue | medium | 0.85 (85%) | Close as external — fix must come from FreeSpire.XLS vendor | stateReason=not_planned |

**Comment draft for `add-comment`:**

```markdown
Thanks for the detailed report and repro project!

This error occurs because **FreeSpire.XLS 14.2.0 was built against SkiaSharp 2.88.x**, which exported a native function called `sk_region_op2`. In SkiaSharp 3.x, this function was renamed to `sk_region_op`. When the 3.x native library is loaded, `sk_region_op2` no longer exists, causing the `EntryPointNotFoundException`.

**Workaround:** Pin your `SkiaSharp.NativeAssets.Linux` package to version **2.88.9** to match what FreeSpire.XLS expects:
```xml
<PackageReference Include="SkiaSharp.NativeAssets.Linux.NoDependencies" Version="2.88.9" />
```

The permanent fix needs to come from **E-iceblue** (the FreeSpire.XLS vendor) updating their library to target SkiaSharp 3.x. We recommend contacting them or checking their changelog for a newer version.

Because the root cause is in a third-party library, we're closing this as external. If SkiaSharp needs to re-export `sk_region_op2` as a compatibility alias, please reopen with that specific request.
```

<details>
<summary>Raw JSON</summary>

```json
{
  "meta": {
    "schemaVersion": "1.0",
    "number": 3119,
    "repo": "mono/SkiaSharp",
    "analyzedAt": "2026-04-21T18:35:44Z",
    "currentLabels": [
      "type/bug"
    ]
  },
  "summary": "EntryPointNotFoundException for 'sk_region_op2' when FreeSpire.XLS 14.2.0 (built against SkiaSharp 2.88.x) is used alongside SkiaSharp.NativeAssets.Linux 3.116.x, where the native symbol was renamed to 'sk_region_op' in the 3.x series.",
  "classification": {
    "type": {
      "value": "type/bug",
      "confidence": 0.9
    },
    "area": {
      "value": "area/libSkiaSharp.native",
      "confidence": 0.88
    },
    "platforms": [
      "os/Linux"
    ],
    "tenets": [
      "tenet/compatibility"
    ]
  },
  "evidence": {
    "bugSignals": {
      "severity": "medium",
      "regressionClaimed": true,
      "errorType": "exception",
      "errorMessage": "System.EntryPointNotFoundException: Unable to find an entry point named 'sk_region_op2' in shared library 'libSkiaSharp'",
      "stackTrace": "at SkiaSharp.SkiaApi.sk_region_op2(IntPtr r, IntPtr src, SKRegionOperation op)\nat SkiaSharp.SKRegion.Op(SKRegion region, SKRegionOperation op)\n[... Spire.Xls internal frames ...]",
      "reproQuality": "complete",
      "targetFrameworks": [
        "net8.0"
      ]
    },
    "reproEvidence": {
      "stepsToReproduce": [
        "Add FreeSpire.XLS 14.2.0 and SkiaSharp.NativeAssets.Linux 3.116.1 to a .NET project",
        "Run on Linux (e.g., Docker mcr.microsoft.com/dotnet/aspnet:8.0)",
        "Instantiate 'new Workbook()'"
      ],
      "environmentDetails": "Docker Linux (mcr.microsoft.com/dotnet/aspnet:8.0), FreeSpire.XLS 14.2.0, SkiaSharp.NativeAssets.Linux 3.116.1",
      "repoLinks": [
        {
          "url": "https://github.com/WouterDevExperts/SkiaSharpBug",
          "description": "Reporter's minimal repro project"
        }
      ]
    },
    "versionAnalysis": {
      "mentionedVersions": [
        "2.88.9",
        "3.116.0",
        "3.116.1"
      ],
      "workedIn": "2.88.9",
      "brokeIn": "3.116.1",
      "currentRelevance": "likely",
      "relevanceReason": "The 3.x native library exports sk_region_op (not sk_region_op2); FreeSpire.XLS was compiled against the 2.88.x SkiaSharp managed assembly, which called sk_region_op2."
    },
    "regression": {
      "isRegression": true,
      "confidence": 0.92,
      "reason": "The 3.x SkiaSharp C API renamed sk_region_op2 to sk_region_op. FreeSpire.XLS bundles its own SkiaSharp 2.88.x managed DLL that P/Invokes the old symbol name, so upgrading native assets to 3.x breaks it.",
      "workedInVersion": "2.88.9",
      "brokeInVersion": "3.116.1"
    }
  },
  "analysis": {
    "summary": "FreeSpire.XLS 14.2.0 embeds or depends on SkiaSharp 2.88.x managed bindings that P/Invoke 'sk_region_op2'. SkiaSharp 3.x renamed this C API entry point to 'sk_region_op'. The user is providing the 3.x native library which does not export 'sk_region_op2', causing an EntryPointNotFoundException at startup. This is a version mismatch between a third-party library's SkiaSharp 2.88.x dependency and the user's SkiaSharp 3.x native assets.",
    "rationale": "The error is a clear EntryPointNotFoundException for a known-renamed symbol (sk_region_op2 → sk_region_op in 3.x). Code investigation confirms SkiaSharp 3.x bindings only export sk_region_op. The crash originates from FreeSpire.XLS's internal usage of the old SkiaSharp 2.88.x P/Invoke binding. A maintainer comment confirmed FreeSpire.XLS uses an older Skia version. The fix must come from FreeSpire.XLS updating to SkiaSharp 3.x.",
    "codeInvestigation": [
      {
        "file": "binding/SkiaSharp/SkiaApi.generated.cs",
        "lines": "12481-12500",
        "finding": "SkiaSharp 3.x generated bindings define sk_region_op (not sk_region_op2). There is no sk_region_op2 symbol exported in the 3.x native library. The rename happened between 2.88.x and 3.x as part of the C API cleanup.",
        "relevance": "direct"
      },
      {
        "file": "binding/SkiaSharp/SKRegion.cs",
        "lines": "213-214",
        "finding": "SKRegion.Op(SKRegion, SKRegionOperation) calls SkiaApi.sk_region_op — confirming the current 3.x managed binding uses sk_region_op, not sk_region_op2.",
        "relevance": "direct"
      }
    ],
    "keySignals": [
      {
        "text": "Unable to find an entry point named 'sk_region_op2' in shared library 'libSkiaSharp'",
        "source": "issue body / stack trace",
        "interpretation": "The 3.x native library does not export sk_region_op2; FreeSpire.XLS calls the 2.88.x symbol name."
      },
      {
        "text": "It works fine in the previous version: SkiaSharp.NativeAssets.Linux.NoDependencies version 2.88.9",
        "source": "issue body",
        "interpretation": "Confirms FreeSpire.XLS was built against SkiaSharp 2.88.x which still exported sk_region_op2."
      },
      {
        "text": "Spire.Xls looks like it uses a much older version of Skia - I'd probably not expect the sample to work.",
        "source": "comment by molesmoke (community)",
        "interpretation": "Third-party library compatibility gap — FreeSpire.XLS has not updated to SkiaSharp 3.x."
      }
    ],
    "workarounds": [
      "Pin SkiaSharp.NativeAssets.Linux to version 2.88.9 to match what FreeSpire.XLS expects, until FreeSpire.XLS publishes a version that supports SkiaSharp 3.x.",
      "Contact E-iceblue (FreeSpire vendor) and request an update to FreeSpire.XLS that targets SkiaSharp 3.x."
    ],
    "nextQuestions": [
      "Has FreeSpire.XLS published a newer version (>14.2.0) that targets SkiaSharp 3.x?",
      "Is it feasible to add a SkiaSharp 2.88.x compatibility shim that re-exports sk_region_op2 as an alias?"
    ],
    "resolution": {
      "hypothesis": "FreeSpire.XLS 14.2.0 was compiled against SkiaSharp 2.88.x and P/Invokes sk_region_op2 which no longer exists in the SkiaSharp 3.x native library.",
      "proposals": [
        {
          "title": "Pin native assets to 2.88.9",
          "description": "Downgrade SkiaSharp.NativeAssets.Linux to 2.88.9 to match FreeSpire.XLS's SkiaSharp 2.88.x dependency. This is a stopgap until FreeSpire.XLS upgrades.",
          "category": "workaround",
          "confidence": 0.95,
          "effort": "cost/xs",
          "validated": "untested"
        },
        {
          "title": "Update FreeSpire.XLS",
          "description": "Check if a newer version of FreeSpire.XLS supports SkiaSharp 3.x, or request the vendor update the dependency.",
          "category": "fix",
          "confidence": 0.8,
          "effort": "cost/s",
          "validated": "untested"
        }
      ],
      "recommendedProposal": "Pin native assets to 2.88.9",
      "recommendedReason": "Immediate workaround that resolves the issue; the root fix must come from FreeSpire.XLS vendor."
    }
  },
  "output": {
    "actionability": {
      "suggestedAction": "close-as-external",
      "confidence": 0.88,
      "reason": "The EntryPointNotFoundException is caused by FreeSpire.XLS (a third-party library) using the old SkiaSharp 2.88.x P/Invoke symbol sk_region_op2 with the 3.x native library. SkiaSharp correctly renamed the symbol in 3.x. The fix must come from FreeSpire.XLS updating its SkiaSharp dependency.",
      "suggestedReproPlatform": "linux"
    },
    "actions": [
      {
        "type": "update-labels",
        "description": "Apply bug, native, linux, and compatibility labels",
        "risk": "low",
        "confidence": 0.95,
        "labels": [
          "type/bug",
          "area/libSkiaSharp.native",
          "os/Linux",
          "tenet/compatibility"
        ]
      },
      {
        "type": "add-comment",
        "description": "Explain the version mismatch and provide the workaround",
        "risk": "high",
        "confidence": 0.88,
        "comment": "Thanks for the detailed report and repro project!\n\nThis error occurs because **FreeSpire.XLS 14.2.0 was built against SkiaSharp 2.88.x**, which exported a native function called `sk_region_op2`. In SkiaSharp 3.x, this function was renamed to `sk_region_op`. When the 3.x native library is loaded, `sk_region_op2` no longer exists, causing the `EntryPointNotFoundException`.\n\n**Workaround:** Pin your `SkiaSharp.NativeAssets.Linux` package to version **2.88.9** to match what FreeSpire.XLS expects:\n```xml\n<PackageReference Include=\"SkiaSharp.NativeAssets.Linux.NoDependencies\" Version=\"2.88.9\" />\n```\n\nThe permanent fix needs to come from **E-iceblue** (the FreeSpire.XLS vendor) updating their library to target SkiaSharp 3.x. We recommend contacting them or checking their changelog for a newer version.\n\nBecause the root cause is in a third-party library, we're closing this as external. If SkiaSharp needs to re-export `sk_region_op2` as a compatibility alias, please reopen with that specific request."
      },
      {
        "type": "close-issue",
        "description": "Close as external — fix must come from FreeSpire.XLS vendor",
        "risk": "medium",
        "confidence": 0.85,
        "stateReason": "not_planned"
      }
    ]
  }
}
```

</details>
