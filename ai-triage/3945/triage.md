# Issue Triage Report — #3945

| Field | Value |
|-------|-------|
| Repository | mono/SkiaSharp |
| Analyzed | 2026-05-13T05:30:00Z |
| Type | type/bug (0.95 (95%)) |
| Area | area/Build (0.92 (92%)) |
| Suggested action | keep-open (0.88 (88%)) |

**Issue Summary:** Uno Gallery sample fails to build on Windows because Uno.Resizetizer 1.13.0-dev.17 loads an old 3.x native libSkiaSharp.dll (version 116.0) at MSBuild task runtime, which is incompatible with the 4.x managed SkiaSharp.dll (expects [88.1, 89.0)), causing TypeInitializationException across all four TFMs; macOS builds fine.

**Analysis:** Uno.Resizetizer's MSBuild task loads SkiaSharp at build time to process SVG assets; on Windows it picks up an old 3.x native libSkiaSharp.dll (milestone 116) from the NuGet cache while the managed SkiaSharp.dll is 4.x (expects native [88.1, 89.0)), triggering a version check failure in SKObject's static constructor. This is a native library resolution difference between Windows and macOS in the Resizetizer tooling. A workaround is active (Uno project excluded from Windows CI .slnx), but the fix requires Resizetizer to be updated to support SkiaSharp 4.x.

**Recommendations:** **keep-open** — Root cause is in upstream Uno.Resizetizer — requires an update from the Uno Platform team. CI is unblocked by the #3835 workaround. Keep open to track until Resizetizer supports SkiaSharp 4.x on Windows.

---

## Classification

| Field | Value |
|-------|-------|
| Type | type/bug |
| Area | area/Build |
| Platforms | os/Windows-Classic |
| Backends | — |
| Tenets | tenet/compatibility, tenet/reliability |
| Partner | partner/unoplatform |
| Current labels | type/bug, area/Build, area/Gallery |

## Evidence

### Reproduction

1. Build the Uno Gallery sample on Windows using Uno.Sdk 6.6.0-dev.208 (with Resizetizer 1.13.0-dev.17)
2. Observe TypeInitializationException from Resizetizer MSBuild task across all four TFMs
3. Same build succeeds on macOS

**Environment:** Windows only; Uno.Resizetizer 1.13.0-dev.17 bundled in Uno.Sdk/6.6.0-dev.208; SkiaSharp 4.x managed, 3.x native (116.0) resolved at MSBuild task runtime

**Repository links:**
- https://dev.azure.com/xamarin/6fd3d886-57a5-4e31-8db7-52a1b47c07a8/_build/results?buildId=157618 — ADO Build #157618 — Samples stage, Windows job showing the Resizetizer crash
- https://github.com/mono/SkiaSharp/issues/3841 — Parent issue: Uno SDK injects SkiaSharp 3.x native assets causing WASM link errors and Resizetizer crash; WASM part fixed by #3867, this Windows Resizetizer crash tracked separately
- https://github.com/mono/SkiaSharp/pull/3867 — PR #3867 (merged): fixed WASM link errors by using SkiaSharpVersion property; did not fix this Resizetizer crash
- https://github.com/mono/SkiaSharp/pull/3835 — PR #3835 (merged): applied workaround — excluded Uno project from Windows .slnx solution file so CI is unblocked

### Bug Signals

| Field | Value |
|-------|-------|
| Severity | high |
| Regression claimed | False |
| Error type | exception |
| Error message | The version of the native libSkiaSharp library (116.0) is incompatible with this version of SkiaSharp. Supported versions of the native libSkiaSharp library are in the range [88.1, 89.0). |
| Repro quality | complete |
| Target frameworks | net10.0-android, net10.0-windows10.0.26100, net10.0-desktop, net10.0-browserwasm |

**Stack trace:**

```text
System.TypeInitializationException: The type initializer for 'SkiaSharp.SKColorSpace' threw an exception. ---> System.TypeInitializationException: The type initializer for 'SkiaSharp.SKObject' threw an exception. ---> System.InvalidOperationException: The version of the native libSkiaSharp library (116.0) is incompatible
```

### Version Analysis

| Field | Value |
|-------|-------|
| Mentioned versions | 116.0, 88.1, 89.0 |
| Worked in | — |
| Broke in | — |
| Current relevance | likely |
| Relevance reason | Workaround is in place (Uno excluded from Windows CI build) but root cause — Resizetizer resolving 3.x native lib on Windows — is not fixed upstream. |

## Analysis

### Technical Summary

Uno.Resizetizer's MSBuild task loads SkiaSharp at build time to process SVG assets; on Windows it picks up an old 3.x native libSkiaSharp.dll (milestone 116) from the NuGet cache while the managed SkiaSharp.dll is 4.x (expects native [88.1, 89.0)), triggering a version check failure in SKObject's static constructor. This is a native library resolution difference between Windows and macOS in the Resizetizer tooling. A workaround is active (Uno project excluded from Windows CI .slnx), but the fix requires Resizetizer to be updated to support SkiaSharp 4.x.

### Rationale

The error is a clear version mismatch in SkiaSharpVersion.CheckNativeLibraryCompatible() — native version 116.0 falls outside [88.1, 89.0). The root cause is in Uno.Resizetizer's native library resolution on Windows, not in SkiaSharp itself. The issue is well-documented with CI evidence, the parent issue #3841 was investigated, the WASM portion was fixed by #3867, and a CI workaround was applied in #3835. The upstream Resizetizer tool must be updated to support SkiaSharp 4.x on Windows.

### Key Signals

- "The version of the native libSkiaSharp library (116.0) is incompatible with this version of SkiaSharp. Supported versions are in range [88.1, 89.0)." — **issue body** (Native lib 116.0 is SkiaSharp 3.x (Skia milestone 116); managed SkiaSharp 4.x expects [88.1, 89.0). Pure version mismatch in SKObject static ctor.)
- "Windows-specific — the same Android TFM builds successfully on macOS" — **issue body** (Native library resolution differs between Windows and macOS for the Resizetizer MSBuild task.)
- "Workaround applied in PR #3835: added platform-specific .slnx files. The Windows variant excludes the Uno project." — **comment #2 by mattleibow** (CI is unblocked but root cause is not fixed — Uno project missing from Windows builds.)
- "Affects all four TFMs on Windows: net10.0-android, net10.0-windows10.0.26100, net10.0-desktop, net10.0-browserwasm" — **comment #1 by mattleibow** (Broader than originally stated — all Resizetizer-processed TFMs affected, not just Android.)

### Code Investigation

| File | Lines | Relevance | Finding |
|------|-------|-----------|---------|
| `binding/SkiaSharp/SkiaSharpVersion.cs` | 38-66 | direct | CheckNativeLibraryCompatible() computes maxSupported as (minSupported.Major+1, 0). For SkiaSharp 4.x NativeMinimum=88.1, maxSupported=89.0. Native 116.0 >= 89.0 => throws InvalidOperationException with the exact error message in the issue. |
| `samples/Gallery/SkiaSharpSample.Windows.slnx` | 1-19 | direct | Windows .slnx solution file does NOT include the Uno project (samples/Gallery/Uno/). This is the PR #3835 workaround — Uno is excluded from Windows CI builds to unblock the pipeline. |

### Workarounds

- Use the platform-specific SkiaSharpSample.Windows.slnx which excludes the Uno project — already applied in CI via PR #3835
- On developer machines, build the Uno Gallery on macOS or Linux where Resizetizer resolves the correct 4.x native library

### Next Questions

- Has Uno.Resizetizer been updated to support SkiaSharp 4.x native library resolution on Windows?
- Is there a way to force Resizetizer's MSBuild task to use a specific SkiaSharp native asset path on Windows?
- When will a compatible version of Uno.Sdk/Resizetizer be available that resolves to 4.x natives on Windows?

### Resolution Proposals

**Hypothesis:** Uno.Resizetizer 1.13.0-dev.17 resolves to a cached SkiaSharp 3.x native DLL on Windows due to NuGet cache/PATH differences between the two platforms. The fix must come from Uno.Resizetizer being updated to use SkiaSharp 4.x.

1. **Wait for Resizetizer update from Uno team** — investigation, confidence 0.85 (85%), cost/m, validated=untested
   - File/track an issue with Uno Platform team to update Resizetizer to support SkiaSharp 4.x native library on Windows. The SkiaSharp team should coordinate with @ramezgerges / Uno Platform.
2. **Investigate forcing SkiaSharp 4.x native asset in Resizetizer MSBuild task** — workaround, confidence 0.60 (60%), cost/s, validated=untested
   - Investigate whether a Directory.Build.props or explicit package pin in the Uno sample csproj can force Resizetizer's MSBuild context to resolve the 4.x native lib on Windows.

**Recommended proposal:** Wait for Resizetizer update from Uno team

**Why:** Root cause is in Uno.Resizetizer's native library resolution on Windows. Only a Resizetizer update that ships SkiaSharp 4.x-compatible native assets will fully resolve this. The CI workaround in #3835 keeps builds unblocked in the interim.

## Recommendations

### Actionability

| Field | Value |
|-------|-------|
| Suggested action | keep-open |
| Confidence | 0.88 (88%) |
| Reason | Root cause is in upstream Uno.Resizetizer — requires an update from the Uno Platform team. CI is unblocked by the #3835 workaround. Keep open to track until Resizetizer supports SkiaSharp 4.x on Windows. |
| Suggested repro platform | windows |

### Automatable Actions

| Type | Risk | Confidence | Description | Details |
|------|------|------------|-------------|---------|
| update-labels | low | 0.92 (92%) | Apply bug, build, Windows, Uno partner, and tenet labels | labels=type/bug, area/Build, os/Windows-Classic, partner/unoplatform, tenet/compatibility, tenet/reliability |
| add-comment | medium | 0.88 (88%) | Post summary noting workaround in place and upstream dependency on Uno team | — |

**Comment draft for `add-comment`:**

```markdown
Thanks for the detailed report and follow-up.

**Status:** A CI workaround was applied in PR #3835 — the Uno project is excluded from the Windows `.slnx` solution, so CI is unblocked. The root cause remains open.

**Root cause:** `Uno.Resizetizer 1.13.0-dev.17` (bundled in `Uno.Sdk/6.6.0-dev.208`) loads SkiaSharp at MSBuild task runtime to process SVG assets. On Windows it resolves to an older `libSkiaSharp.dll` (version 116.0 = SkiaSharp 3.x) while the managed SkiaSharp.dll is 4.x (expects native `[88.1, 89.0)`). The version check in `SKObject`'s static constructor then throws `TypeInitializationException`.

**Fix path:** This needs a Resizetizer update from the Uno Platform team to ship/resolve SkiaSharp 4.x-compatible native assets on Windows. @ramezgerges — could you track this with the Uno team?

**Workaround for developers:** Build the Uno Gallery on macOS or Linux where Resizetizer correctly resolves the 4.x native library.
```

<details>
<summary>Raw JSON</summary>

```json
{
  "meta": {
    "schemaVersion": "1.0",
    "number": 3945,
    "repo": "mono/SkiaSharp",
    "analyzedAt": "2026-05-13T05:30:00Z",
    "currentLabels": [
      "type/bug",
      "area/Build",
      "area/Gallery"
    ]
  },
  "summary": "Uno Gallery sample fails to build on Windows because Uno.Resizetizer 1.13.0-dev.17 loads an old 3.x native libSkiaSharp.dll (version 116.0) at MSBuild task runtime, which is incompatible with the 4.x managed SkiaSharp.dll (expects [88.1, 89.0)), causing TypeInitializationException across all four TFMs; macOS builds fine.",
  "classification": {
    "type": {
      "value": "type/bug",
      "confidence": 0.95
    },
    "area": {
      "value": "area/Build",
      "confidence": 0.92
    },
    "platforms": [
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
      "errorType": "exception",
      "errorMessage": "The version of the native libSkiaSharp library (116.0) is incompatible with this version of SkiaSharp. Supported versions of the native libSkiaSharp library are in the range [88.1, 89.0).",
      "stackTrace": "System.TypeInitializationException: The type initializer for 'SkiaSharp.SKColorSpace' threw an exception. ---> System.TypeInitializationException: The type initializer for 'SkiaSharp.SKObject' threw an exception. ---> System.InvalidOperationException: The version of the native libSkiaSharp library (116.0) is incompatible",
      "reproQuality": "complete",
      "targetFrameworks": [
        "net10.0-android",
        "net10.0-windows10.0.26100",
        "net10.0-desktop",
        "net10.0-browserwasm"
      ]
    },
    "reproEvidence": {
      "stepsToReproduce": [
        "Build the Uno Gallery sample on Windows using Uno.Sdk 6.6.0-dev.208 (with Resizetizer 1.13.0-dev.17)",
        "Observe TypeInitializationException from Resizetizer MSBuild task across all four TFMs",
        "Same build succeeds on macOS"
      ],
      "environmentDetails": "Windows only; Uno.Resizetizer 1.13.0-dev.17 bundled in Uno.Sdk/6.6.0-dev.208; SkiaSharp 4.x managed, 3.x native (116.0) resolved at MSBuild task runtime",
      "repoLinks": [
        {
          "url": "https://dev.azure.com/xamarin/6fd3d886-57a5-4e31-8db7-52a1b47c07a8/_build/results?buildId=157618",
          "description": "ADO Build #157618 — Samples stage, Windows job showing the Resizetizer crash"
        },
        {
          "url": "https://github.com/mono/SkiaSharp/issues/3841",
          "description": "Parent issue: Uno SDK injects SkiaSharp 3.x native assets causing WASM link errors and Resizetizer crash; WASM part fixed by #3867, this Windows Resizetizer crash tracked separately"
        },
        {
          "url": "https://github.com/mono/SkiaSharp/pull/3867",
          "description": "PR #3867 (merged): fixed WASM link errors by using SkiaSharpVersion property; did not fix this Resizetizer crash"
        },
        {
          "url": "https://github.com/mono/SkiaSharp/pull/3835",
          "description": "PR #3835 (merged): applied workaround — excluded Uno project from Windows .slnx solution file so CI is unblocked"
        }
      ]
    },
    "versionAnalysis": {
      "mentionedVersions": [
        "116.0",
        "88.1",
        "89.0"
      ],
      "currentRelevance": "likely",
      "relevanceReason": "Workaround is in place (Uno excluded from Windows CI build) but root cause — Resizetizer resolving 3.x native lib on Windows — is not fixed upstream."
    }
  },
  "analysis": {
    "summary": "Uno.Resizetizer's MSBuild task loads SkiaSharp at build time to process SVG assets; on Windows it picks up an old 3.x native libSkiaSharp.dll (milestone 116) from the NuGet cache while the managed SkiaSharp.dll is 4.x (expects native [88.1, 89.0)), triggering a version check failure in SKObject's static constructor. This is a native library resolution difference between Windows and macOS in the Resizetizer tooling. A workaround is active (Uno project excluded from Windows CI .slnx), but the fix requires Resizetizer to be updated to support SkiaSharp 4.x.",
    "rationale": "The error is a clear version mismatch in SkiaSharpVersion.CheckNativeLibraryCompatible() — native version 116.0 falls outside [88.1, 89.0). The root cause is in Uno.Resizetizer's native library resolution on Windows, not in SkiaSharp itself. The issue is well-documented with CI evidence, the parent issue #3841 was investigated, the WASM portion was fixed by #3867, and a CI workaround was applied in #3835. The upstream Resizetizer tool must be updated to support SkiaSharp 4.x on Windows.",
    "keySignals": [
      {
        "text": "The version of the native libSkiaSharp library (116.0) is incompatible with this version of SkiaSharp. Supported versions are in range [88.1, 89.0).",
        "source": "issue body",
        "interpretation": "Native lib 116.0 is SkiaSharp 3.x (Skia milestone 116); managed SkiaSharp 4.x expects [88.1, 89.0). Pure version mismatch in SKObject static ctor."
      },
      {
        "text": "Windows-specific — the same Android TFM builds successfully on macOS",
        "source": "issue body",
        "interpretation": "Native library resolution differs between Windows and macOS for the Resizetizer MSBuild task."
      },
      {
        "text": "Workaround applied in PR #3835: added platform-specific .slnx files. The Windows variant excludes the Uno project.",
        "source": "comment #2 by mattleibow",
        "interpretation": "CI is unblocked but root cause is not fixed — Uno project missing from Windows builds."
      },
      {
        "text": "Affects all four TFMs on Windows: net10.0-android, net10.0-windows10.0.26100, net10.0-desktop, net10.0-browserwasm",
        "source": "comment #1 by mattleibow",
        "interpretation": "Broader than originally stated — all Resizetizer-processed TFMs affected, not just Android."
      }
    ],
    "codeInvestigation": [
      {
        "file": "binding/SkiaSharp/SkiaSharpVersion.cs",
        "lines": "38-66",
        "finding": "CheckNativeLibraryCompatible() computes maxSupported as (minSupported.Major+1, 0). For SkiaSharp 4.x NativeMinimum=88.1, maxSupported=89.0. Native 116.0 >= 89.0 => throws InvalidOperationException with the exact error message in the issue.",
        "relevance": "direct"
      },
      {
        "file": "samples/Gallery/SkiaSharpSample.Windows.slnx",
        "lines": "1-19",
        "finding": "Windows .slnx solution file does NOT include the Uno project (samples/Gallery/Uno/). This is the PR #3835 workaround — Uno is excluded from Windows CI builds to unblock the pipeline.",
        "relevance": "direct"
      }
    ],
    "workarounds": [
      "Use the platform-specific SkiaSharpSample.Windows.slnx which excludes the Uno project — already applied in CI via PR #3835",
      "On developer machines, build the Uno Gallery on macOS or Linux where Resizetizer resolves the correct 4.x native library"
    ],
    "nextQuestions": [
      "Has Uno.Resizetizer been updated to support SkiaSharp 4.x native library resolution on Windows?",
      "Is there a way to force Resizetizer's MSBuild task to use a specific SkiaSharp native asset path on Windows?",
      "When will a compatible version of Uno.Sdk/Resizetizer be available that resolves to 4.x natives on Windows?"
    ],
    "resolution": {
      "hypothesis": "Uno.Resizetizer 1.13.0-dev.17 resolves to a cached SkiaSharp 3.x native DLL on Windows due to NuGet cache/PATH differences between the two platforms. The fix must come from Uno.Resizetizer being updated to use SkiaSharp 4.x.",
      "proposals": [
        {
          "title": "Wait for Resizetizer update from Uno team",
          "description": "File/track an issue with Uno Platform team to update Resizetizer to support SkiaSharp 4.x native library on Windows. The SkiaSharp team should coordinate with @ramezgerges / Uno Platform.",
          "category": "investigation",
          "confidence": 0.85,
          "effort": "cost/m",
          "validated": "untested"
        },
        {
          "title": "Investigate forcing SkiaSharp 4.x native asset in Resizetizer MSBuild task",
          "description": "Investigate whether a Directory.Build.props or explicit package pin in the Uno sample csproj can force Resizetizer's MSBuild context to resolve the 4.x native lib on Windows.",
          "category": "workaround",
          "confidence": 0.6,
          "effort": "cost/s",
          "validated": "untested"
        }
      ],
      "recommendedProposal": "Wait for Resizetizer update from Uno team",
      "recommendedReason": "Root cause is in Uno.Resizetizer's native library resolution on Windows. Only a Resizetizer update that ships SkiaSharp 4.x-compatible native assets will fully resolve this. The CI workaround in #3835 keeps builds unblocked in the interim."
    }
  },
  "output": {
    "actionability": {
      "suggestedAction": "keep-open",
      "confidence": 0.88,
      "reason": "Root cause is in upstream Uno.Resizetizer — requires an update from the Uno Platform team. CI is unblocked by the #3835 workaround. Keep open to track until Resizetizer supports SkiaSharp 4.x on Windows.",
      "suggestedReproPlatform": "windows"
    },
    "actions": [
      {
        "type": "update-labels",
        "description": "Apply bug, build, Windows, Uno partner, and tenet labels",
        "risk": "low",
        "confidence": 0.92,
        "labels": [
          "type/bug",
          "area/Build",
          "os/Windows-Classic",
          "partner/unoplatform",
          "tenet/compatibility",
          "tenet/reliability"
        ]
      },
      {
        "type": "add-comment",
        "description": "Post summary noting workaround in place and upstream dependency on Uno team",
        "risk": "medium",
        "confidence": 0.88,
        "comment": "Thanks for the detailed report and follow-up.\n\n**Status:** A CI workaround was applied in PR #3835 — the Uno project is excluded from the Windows `.slnx` solution, so CI is unblocked. The root cause remains open.\n\n**Root cause:** `Uno.Resizetizer 1.13.0-dev.17` (bundled in `Uno.Sdk/6.6.0-dev.208`) loads SkiaSharp at MSBuild task runtime to process SVG assets. On Windows it resolves to an older `libSkiaSharp.dll` (version 116.0 = SkiaSharp 3.x) while the managed SkiaSharp.dll is 4.x (expects native `[88.1, 89.0)`). The version check in `SKObject`'s static constructor then throws `TypeInitializationException`.\n\n**Fix path:** This needs a Resizetizer update from the Uno Platform team to ship/resolve SkiaSharp 4.x-compatible native assets on Windows. @ramezgerges — could you track this with the Uno team?\n\n**Workaround for developers:** Build the Uno Gallery on macOS or Linux where Resizetizer correctly resolves the 4.x native library."
      }
    ]
  }
}
```

</details>
