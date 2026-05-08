# Issue Triage Report — #3276

| Field | Value |
|-------|-------|
| Repository | mono/SkiaSharp |
| Analyzed | 2026-05-06T21:35:00Z |
| Type | type/bug (0.85 (85%)) |
| Area | area/libSkiaSharp.native (0.88 (88%)) |
| Suggested action | close-as-external (0.82 (82%)) |

**Issue Summary:** Reporter encounters a native libSkiaSharp version compatibility exception when deploying a Blazor/XAF app with DevExpress.Drawing.Skia 24.2.6 and SkiaSharp 3.119.0 on Azure App Service (Linux). The app works on Windows and in a sibling Worker project that doesn't use DevExpress.

**Analysis:** DevExpress.Drawing.Skia 24.2.6 was built against SkiaSharp 2.88.x and likely ships its own libSkiaSharp.so native binary for that version. On Linux, the dynamic linker loads whichever libSkiaSharp.so it finds first; if DevExpress's 2.88.x native binary wins the race, SkiaSharp's static constructor calls SkiaSharpVersion.CheckNativeLibraryCompatible() which throws InvalidOperationException because the native version (2.88.x) is outside the supported range for the 3.119.0 managed bindings. On Windows, DLL loading order differs and the SkiaSharp 3.119.0 native DLL wins, so no conflict occurs.

**Recommendations:** **close-as-external** — Root cause is DevExpress.Drawing.Skia bundling an incompatible SkiaSharp 2.88.x native library. SkiaSharp cannot resolve this conflict — DevExpress must update their package to target SkiaSharp 3.x. The reporter can work around by pinning SkiaSharp 2.88.9 or moving NativeAssets to the executable project.

---

## Classification

| Field | Value |
|-------|-------|
| Type | type/bug |
| Area | area/libSkiaSharp.native |
| Platforms | os/Linux |
| Backends | — |
| Tenets | tenet/compatibility, tenet/reliability |
| Partner | — |
| Current labels | type/bug |

## Evidence

### Reproduction

1. Create a Blazor/XAF project with DevExpress.Blazor 24.2.6 and DevExpress.Drawing.Skia 24.2.6
2. Add SkiaSharp 3.119.0 + SkiaSharp.NativeAssets.Linux.NoDependencies 3.119.0 in a shared Core library
3. Deploy to Azure App Service running Linux
4. Observe runtime exception in the browser console

**Environment:** Azure App Service on Linux, SkiaSharp 3.119.0, DevExpress.Drawing.Skia 24.2.6, Visual Studio on Windows (local dev)

**Related issues:** #2653, #1341

**Repository links:**
- https://github.com/mono/SkiaSharp/issues/2653 — Related: Linux native library loading issues
- https://github.com/mono/SkiaSharp/issues/1341 — Historical: Unable to load libSkiaSharp on Azure App Service Linux

### Bug Signals

| Field | Value |
|-------|-------|
| Severity | high |
| Regression claimed | True |
| Error type | exception |
| Error message | native libSkiaSharp library version exception (exact message in screenshot, not provided as text) |
| Repro quality | partial |
| Target frameworks | net8.0 |

### Version Analysis

| Field | Value |
|-------|-------|
| Mentioned versions | 3.119.0, 2.88.9 |
| Worked in | 2.88.9 |
| Broke in | — |
| Current relevance | likely |
| Relevance reason | SkiaSharp 3.x introduced a major ABI break and a native version compatibility check. DevExpress.Drawing.Skia 24.2.6 was likely built against SkiaSharp 2.88.x and ships a 2.88.x native libSkiaSharp.so on Linux, which conflicts with the 3.119.0 managed bindings. |

### Regression

| Field | Value |
|-------|-------|
| Is regression | True |
| Confidence | 0.80 (80%) |
| Reason | Reporter explicitly states it worked with 2.88.9 and broke after upgrading to 3.119.0. The SkiaSharp 3.x series has an incompatible native ABI compared to 2.88.x. |
| Worked in version | 2.88.9 |
| Broke in version | 3.119.0 |

## Analysis

### Technical Summary

DevExpress.Drawing.Skia 24.2.6 was built against SkiaSharp 2.88.x and likely ships its own libSkiaSharp.so native binary for that version. On Linux, the dynamic linker loads whichever libSkiaSharp.so it finds first; if DevExpress's 2.88.x native binary wins the race, SkiaSharp's static constructor calls SkiaSharpVersion.CheckNativeLibraryCompatible() which throws InvalidOperationException because the native version (2.88.x) is outside the supported range for the 3.119.0 managed bindings. On Windows, DLL loading order differs and the SkiaSharp 3.119.0 native DLL wins, so no conflict occurs.

### Rationale

The error is clearly a third-party version conflict: DevExpress.Drawing.Skia is a DevExpress product that has its own SkiaSharp dependency. SkiaSharp 3.x introduced a breaking ABI change (milestone version bumped from 88 to 119) and `SkiaSharpVersion.CheckNativeLibraryCompatible()` enforces this via an `InvalidOperationException`. The fix requires DevExpress to update DevExpress.Drawing.Skia to target SkiaSharp 3.x. SkiaSharp itself cannot resolve this conflict. Classified as `close-as-external` because the root cause is outside SkiaSharp.

### Key Signals

- "DevExpress.Blazor 24.2.6 + DevExpress.Drawing.Skia 24.2.6" — **issue body** (DevExpress.Drawing.Skia is a third-party wrapper around SkiaSharp that may bundle its own native binary.)
- "The Worker project is working fine in the same environment" — **issue body** (The Worker project has no DevExpress dependency. This isolates the cause to the DevExpress-related packages.)
- "Last Known Good Version: 2.88.9" — **issue body** (The breakage aligns exactly with the SkiaSharp 3.x major version bump, which introduced ABI incompatibility.)
- "SkiaSharp.NativeAssets.Linux.NoDependencies in Core project (library, not executable)" — **issue body** (NativeAssets should be in the executable project, not a library. This may also contribute to wrong binary deployment.)

### Code Investigation

| File | Lines | Relevance | Finding |
|------|-------|-----------|---------|
| `binding/SkiaSharp/SkiaSharpVersion.cs` | 35-67 | direct | CheckNativeLibraryCompatible() is called from SKObject static constructor. It reads the native library's milestone/increment via sk_version_get_milestone() and compares to VersionConstants (3.119). If DevExpress ships a 2.88.x libSkiaSharp.so and it gets loaded first, this throws InvalidOperationException with message 'The version of the native libSkiaSharp library (X.Y) is incompatible with this version of SkiaSharp.' |
| `binding/SkiaSharp/SKObject.cs` | 39-49 | direct | SKObject static constructor calls SkiaSharpVersion.CheckNativeLibraryCompatible(true) — this is the throw site for the version mismatch exception. |
| `documentation/dev/packages.md` | 138-145 | related | NativeAssets packages must be in the executable project, not a library. Reporter has NativeAssets.Linux.NoDependencies in Core (a library project). This means the correct 3.119.0 native binary may not be deployed, allowing DevExpress's bundled binary to be loaded instead. |

### Workarounds

- Move SkiaSharp.NativeAssets.Linux.NoDependencies 3.119.0 from the Core library project to the Dashboard executable project
- Contact DevExpress support to request a DevExpress.Drawing.Skia version that supports SkiaSharp 3.x
- Pin SkiaSharp to 2.88.9 to match DevExpress.Drawing.Skia's expected version (not recommended long-term)

### Next Questions

- What is the exact exception text from the screenshot?
- Does DevExpress.Drawing.Skia 24.2.6 declare a SkiaSharp version dependency in its nuspec?
- Which NuGet-resolved version of SkiaSharp does the Dashboard project use after DevExpress is included?

### Resolution Proposals

**Hypothesis:** DevExpress.Drawing.Skia ships or depends on a SkiaSharp 2.88.x native binary; on Linux this conflicts with SkiaSharp 3.119.0 managed bindings triggering the native version check. Compounding issue: NativeAssets package is in a library project instead of the executable.

1. **Move NativeAssets to executable project** — workaround, confidence 0.70 (70%), cost/xs, validated=untested
   - Move SkiaSharp.NativeAssets.Linux.NoDependencies from the Core class library to the Dashboard executable project so the 3.119.0 native binary is correctly deployed alongside the app.
2. **Contact DevExpress to update their SkiaSharp dependency** — alternative, confidence 0.90 (90%), cost/m, validated=untested
   - The permanent fix requires DevExpress to release a version of DevExpress.Drawing.Skia that targets SkiaSharp 3.x. File a support ticket with DevExpress referencing the SkiaSharp 3.x migration.
3. **Pin SkiaSharp to 2.88.9** — workaround, confidence 0.80 (80%), cost/xs, validated=untested
   - Downgrade SkiaSharp and NativeAssets to 2.88.9 to match DevExpress.Drawing.Skia's expected version. Use SkiaSharp.NativeAssets.Linux.NoDependencies 2.88.9 in the executable project.

**Recommended proposal:** Contact DevExpress to update their SkiaSharp dependency

**Why:** The root cause is in DevExpress.Drawing.Skia. The workarounds are temporary; only DevExpress updating their package provides a clean long-term solution.

## Recommendations

### Actionability

| Field | Value |
|-------|-------|
| Suggested action | close-as-external |
| Confidence | 0.82 (82%) |
| Reason | Root cause is DevExpress.Drawing.Skia bundling an incompatible SkiaSharp 2.88.x native library. SkiaSharp cannot resolve this conflict — DevExpress must update their package to target SkiaSharp 3.x. The reporter can work around by pinning SkiaSharp 2.88.9 or moving NativeAssets to the executable project. |
| Suggested repro platform | linux |

### Missing Info

- Exact error message text (only provided as screenshot)
- SkiaSharp version that DevExpress.Drawing.Skia 24.2.6 depends on (from nuspec)

### Automatable Actions

| Type | Risk | Confidence | Description | Details |
|------|------|------------|-------------|---------|
| update-labels | low | 0.88 (88%) | Apply bug, native, linux, compatibility labels | labels=type/bug, area/libSkiaSharp.native, os/Linux, tenet/compatibility, tenet/reliability |
| add-comment | medium | 0.82 (82%) | Explain the version conflict, suggest workarounds, and direct reporter to DevExpress | — |
| close-issue | high | 0.75 (75%) | Close as external — root cause is in DevExpress.Drawing.Skia | stateReason=not_planned |

**Comment draft for `add-comment`:**

```markdown
Thanks for the detailed report.

The root cause is a version conflict between `DevExpress.Drawing.Skia 24.2.6` and `SkiaSharp 3.119.0`. DevExpress.Drawing.Skia was built against SkiaSharp 2.88.x and likely ships (or expects) a 2.88.x native `libSkiaSharp.so`. On Linux, if that 2.88.x native binary gets loaded, SkiaSharp 3.119.0's managed code immediately throws because the version check in `SkiaSharpVersion.CheckNativeLibraryCompatible()` detects a native/managed version mismatch.

A few things you can try while waiting for DevExpress to release a SkiaSharp 3.x compatible build:

**Workaround 1 — Move NativeAssets to the executable project:**

The `SkiaSharp.NativeAssets.Linux.NoDependencies` reference should be in the **Dashboard** executable project, not in the `Core` library. When it's in a library, the native binary may not get deployed correctly:

```xml
<!-- Dashboard.csproj (executable) -->
<PackageReference Include="SkiaSharp.NativeAssets.Linux.NoDependencies" Version="3.119.0" />

<!-- Remove from Core.csproj -->
```

**Workaround 2 — Pin SkiaSharp to 2.88.9 to match DevExpress:**

```xml
<PackageReference Include="SkiaSharp" Version="2.88.9" />
<PackageReference Include="SkiaSharp.NativeAssets.Linux.NoDependencies" Version="2.88.9" />
```

The permanent fix requires DevExpress to update `DevExpress.Drawing.Skia` to target SkiaSharp 3.x. We recommend filing a support ticket with DevExpress referencing the [SkiaSharp 3.x migration](https://github.com/mono/SkiaSharp/releases).

Could you confirm whether moving the NativeAssets package to the Dashboard executable project changes anything? That will help confirm which binary is being loaded.
```

<details>
<summary>Raw JSON</summary>

```json
{
  "meta": {
    "schemaVersion": "1.0",
    "number": 3276,
    "repo": "mono/SkiaSharp",
    "analyzedAt": "2026-05-06T21:35:00Z",
    "currentLabels": [
      "type/bug"
    ]
  },
  "summary": "Reporter encounters a native libSkiaSharp version compatibility exception when deploying a Blazor/XAF app with DevExpress.Drawing.Skia 24.2.6 and SkiaSharp 3.119.0 on Azure App Service (Linux). The app works on Windows and in a sibling Worker project that doesn't use DevExpress.",
  "classification": {
    "type": {
      "value": "type/bug",
      "confidence": 0.85
    },
    "area": {
      "value": "area/libSkiaSharp.native",
      "confidence": 0.88
    },
    "platforms": [
      "os/Linux"
    ],
    "tenets": [
      "tenet/compatibility",
      "tenet/reliability"
    ]
  },
  "evidence": {
    "bugSignals": {
      "severity": "high",
      "regressionClaimed": true,
      "errorType": "exception",
      "errorMessage": "native libSkiaSharp library version exception (exact message in screenshot, not provided as text)",
      "reproQuality": "partial",
      "targetFrameworks": [
        "net8.0"
      ]
    },
    "reproEvidence": {
      "stepsToReproduce": [
        "Create a Blazor/XAF project with DevExpress.Blazor 24.2.6 and DevExpress.Drawing.Skia 24.2.6",
        "Add SkiaSharp 3.119.0 + SkiaSharp.NativeAssets.Linux.NoDependencies 3.119.0 in a shared Core library",
        "Deploy to Azure App Service running Linux",
        "Observe runtime exception in the browser console"
      ],
      "environmentDetails": "Azure App Service on Linux, SkiaSharp 3.119.0, DevExpress.Drawing.Skia 24.2.6, Visual Studio on Windows (local dev)",
      "relatedIssues": [
        2653,
        1341
      ],
      "repoLinks": [
        {
          "url": "https://github.com/mono/SkiaSharp/issues/2653",
          "description": "Related: Linux native library loading issues"
        },
        {
          "url": "https://github.com/mono/SkiaSharp/issues/1341",
          "description": "Historical: Unable to load libSkiaSharp on Azure App Service Linux"
        }
      ]
    },
    "versionAnalysis": {
      "mentionedVersions": [
        "3.119.0",
        "2.88.9"
      ],
      "workedIn": "2.88.9",
      "currentRelevance": "likely",
      "relevanceReason": "SkiaSharp 3.x introduced a major ABI break and a native version compatibility check. DevExpress.Drawing.Skia 24.2.6 was likely built against SkiaSharp 2.88.x and ships a 2.88.x native libSkiaSharp.so on Linux, which conflicts with the 3.119.0 managed bindings."
    },
    "regression": {
      "isRegression": true,
      "confidence": 0.8,
      "reason": "Reporter explicitly states it worked with 2.88.9 and broke after upgrading to 3.119.0. The SkiaSharp 3.x series has an incompatible native ABI compared to 2.88.x.",
      "workedInVersion": "2.88.9",
      "brokeInVersion": "3.119.0"
    }
  },
  "analysis": {
    "summary": "DevExpress.Drawing.Skia 24.2.6 was built against SkiaSharp 2.88.x and likely ships its own libSkiaSharp.so native binary for that version. On Linux, the dynamic linker loads whichever libSkiaSharp.so it finds first; if DevExpress's 2.88.x native binary wins the race, SkiaSharp's static constructor calls SkiaSharpVersion.CheckNativeLibraryCompatible() which throws InvalidOperationException because the native version (2.88.x) is outside the supported range for the 3.119.0 managed bindings. On Windows, DLL loading order differs and the SkiaSharp 3.119.0 native DLL wins, so no conflict occurs.",
    "rationale": "The error is clearly a third-party version conflict: DevExpress.Drawing.Skia is a DevExpress product that has its own SkiaSharp dependency. SkiaSharp 3.x introduced a breaking ABI change (milestone version bumped from 88 to 119) and `SkiaSharpVersion.CheckNativeLibraryCompatible()` enforces this via an `InvalidOperationException`. The fix requires DevExpress to update DevExpress.Drawing.Skia to target SkiaSharp 3.x. SkiaSharp itself cannot resolve this conflict. Classified as `close-as-external` because the root cause is outside SkiaSharp.",
    "keySignals": [
      {
        "text": "DevExpress.Blazor 24.2.6 + DevExpress.Drawing.Skia 24.2.6",
        "source": "issue body",
        "interpretation": "DevExpress.Drawing.Skia is a third-party wrapper around SkiaSharp that may bundle its own native binary."
      },
      {
        "text": "The Worker project is working fine in the same environment",
        "source": "issue body",
        "interpretation": "The Worker project has no DevExpress dependency. This isolates the cause to the DevExpress-related packages."
      },
      {
        "text": "Last Known Good Version: 2.88.9",
        "source": "issue body",
        "interpretation": "The breakage aligns exactly with the SkiaSharp 3.x major version bump, which introduced ABI incompatibility."
      },
      {
        "text": "SkiaSharp.NativeAssets.Linux.NoDependencies in Core project (library, not executable)",
        "source": "issue body",
        "interpretation": "NativeAssets should be in the executable project, not a library. This may also contribute to wrong binary deployment."
      }
    ],
    "codeInvestigation": [
      {
        "file": "binding/SkiaSharp/SkiaSharpVersion.cs",
        "lines": "35-67",
        "finding": "CheckNativeLibraryCompatible() is called from SKObject static constructor. It reads the native library's milestone/increment via sk_version_get_milestone() and compares to VersionConstants (3.119). If DevExpress ships a 2.88.x libSkiaSharp.so and it gets loaded first, this throws InvalidOperationException with message 'The version of the native libSkiaSharp library (X.Y) is incompatible with this version of SkiaSharp.'",
        "relevance": "direct"
      },
      {
        "file": "binding/SkiaSharp/SKObject.cs",
        "lines": "39-49",
        "finding": "SKObject static constructor calls SkiaSharpVersion.CheckNativeLibraryCompatible(true) — this is the throw site for the version mismatch exception.",
        "relevance": "direct"
      },
      {
        "file": "documentation/dev/packages.md",
        "lines": "138-145",
        "finding": "NativeAssets packages must be in the executable project, not a library. Reporter has NativeAssets.Linux.NoDependencies in Core (a library project). This means the correct 3.119.0 native binary may not be deployed, allowing DevExpress's bundled binary to be loaded instead.",
        "relevance": "related"
      }
    ],
    "workarounds": [
      "Move SkiaSharp.NativeAssets.Linux.NoDependencies 3.119.0 from the Core library project to the Dashboard executable project",
      "Contact DevExpress support to request a DevExpress.Drawing.Skia version that supports SkiaSharp 3.x",
      "Pin SkiaSharp to 2.88.9 to match DevExpress.Drawing.Skia's expected version (not recommended long-term)"
    ],
    "nextQuestions": [
      "What is the exact exception text from the screenshot?",
      "Does DevExpress.Drawing.Skia 24.2.6 declare a SkiaSharp version dependency in its nuspec?",
      "Which NuGet-resolved version of SkiaSharp does the Dashboard project use after DevExpress is included?"
    ],
    "resolution": {
      "hypothesis": "DevExpress.Drawing.Skia ships or depends on a SkiaSharp 2.88.x native binary; on Linux this conflicts with SkiaSharp 3.119.0 managed bindings triggering the native version check. Compounding issue: NativeAssets package is in a library project instead of the executable.",
      "proposals": [
        {
          "title": "Move NativeAssets to executable project",
          "description": "Move SkiaSharp.NativeAssets.Linux.NoDependencies from the Core class library to the Dashboard executable project so the 3.119.0 native binary is correctly deployed alongside the app.",
          "category": "workaround",
          "confidence": 0.7,
          "effort": "cost/xs",
          "validated": "untested"
        },
        {
          "title": "Contact DevExpress to update their SkiaSharp dependency",
          "description": "The permanent fix requires DevExpress to release a version of DevExpress.Drawing.Skia that targets SkiaSharp 3.x. File a support ticket with DevExpress referencing the SkiaSharp 3.x migration.",
          "category": "alternative",
          "confidence": 0.9,
          "effort": "cost/m",
          "validated": "untested"
        },
        {
          "title": "Pin SkiaSharp to 2.88.9",
          "description": "Downgrade SkiaSharp and NativeAssets to 2.88.9 to match DevExpress.Drawing.Skia's expected version. Use SkiaSharp.NativeAssets.Linux.NoDependencies 2.88.9 in the executable project.",
          "category": "workaround",
          "confidence": 0.8,
          "effort": "cost/xs",
          "validated": "untested"
        }
      ],
      "recommendedProposal": "Contact DevExpress to update their SkiaSharp dependency",
      "recommendedReason": "The root cause is in DevExpress.Drawing.Skia. The workarounds are temporary; only DevExpress updating their package provides a clean long-term solution."
    }
  },
  "output": {
    "actionability": {
      "suggestedAction": "close-as-external",
      "confidence": 0.82,
      "reason": "Root cause is DevExpress.Drawing.Skia bundling an incompatible SkiaSharp 2.88.x native library. SkiaSharp cannot resolve this conflict — DevExpress must update their package to target SkiaSharp 3.x. The reporter can work around by pinning SkiaSharp 2.88.9 or moving NativeAssets to the executable project.",
      "suggestedReproPlatform": "linux"
    },
    "missingInfo": [
      "Exact error message text (only provided as screenshot)",
      "SkiaSharp version that DevExpress.Drawing.Skia 24.2.6 depends on (from nuspec)"
    ],
    "actions": [
      {
        "type": "update-labels",
        "description": "Apply bug, native, linux, compatibility labels",
        "risk": "low",
        "confidence": 0.88,
        "labels": [
          "type/bug",
          "area/libSkiaSharp.native",
          "os/Linux",
          "tenet/compatibility",
          "tenet/reliability"
        ]
      },
      {
        "type": "add-comment",
        "description": "Explain the version conflict, suggest workarounds, and direct reporter to DevExpress",
        "risk": "medium",
        "confidence": 0.82,
        "comment": "Thanks for the detailed report.\n\nThe root cause is a version conflict between `DevExpress.Drawing.Skia 24.2.6` and `SkiaSharp 3.119.0`. DevExpress.Drawing.Skia was built against SkiaSharp 2.88.x and likely ships (or expects) a 2.88.x native `libSkiaSharp.so`. On Linux, if that 2.88.x native binary gets loaded, SkiaSharp 3.119.0's managed code immediately throws because the version check in `SkiaSharpVersion.CheckNativeLibraryCompatible()` detects a native/managed version mismatch.\n\nA few things you can try while waiting for DevExpress to release a SkiaSharp 3.x compatible build:\n\n**Workaround 1 — Move NativeAssets to the executable project:**\n\nThe `SkiaSharp.NativeAssets.Linux.NoDependencies` reference should be in the **Dashboard** executable project, not in the `Core` library. When it's in a library, the native binary may not get deployed correctly:\n\n```xml\n<!-- Dashboard.csproj (executable) -->\n<PackageReference Include=\"SkiaSharp.NativeAssets.Linux.NoDependencies\" Version=\"3.119.0\" />\n\n<!-- Remove from Core.csproj -->\n```\n\n**Workaround 2 — Pin SkiaSharp to 2.88.9 to match DevExpress:**\n\n```xml\n<PackageReference Include=\"SkiaSharp\" Version=\"2.88.9\" />\n<PackageReference Include=\"SkiaSharp.NativeAssets.Linux.NoDependencies\" Version=\"2.88.9\" />\n```\n\nThe permanent fix requires DevExpress to update `DevExpress.Drawing.Skia` to target SkiaSharp 3.x. We recommend filing a support ticket with DevExpress referencing the [SkiaSharp 3.x migration](https://github.com/mono/SkiaSharp/releases).\n\nCould you confirm whether moving the NativeAssets package to the Dashboard executable project changes anything? That will help confirm which binary is being loaded."
      },
      {
        "type": "close-issue",
        "description": "Close as external — root cause is in DevExpress.Drawing.Skia",
        "risk": "high",
        "confidence": 0.75,
        "stateReason": "not_planned"
      }
    ]
  }
}
```

</details>
