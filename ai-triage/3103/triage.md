# Issue Triage Report — #3103

| Field | Value |
|-------|-------|
| Repository | mono/SkiaSharp |
| Analyzed | 2026-04-26T19:45:49Z |
| Type | type/bug (0.90 (90%)) |
| Area | area/libSkiaSharp.native (0.92 (92%)) |
| Suggested action | needs-info (0.80 (80%)) |

**Issue Summary:** SkiaSharp 3.116.0 throws TypeInitializationException / DllNotFoundException when used in an Azure Linux Function App targeting .NET 9.0 (isolated) because the Linux native binary is not deployed — SkiaSharp.NativeAssets.Linux.NoDependencies must be added manually to the application project.

**Analysis:** The Azure Linux Function App fails with DllNotFoundException because libSkiaSharp.so is not deployed. On Linux, SkiaSharp.NativeAssets.Linux or SkiaSharp.NativeAssets.Linux.NoDependencies must be explicitly added as a PackageReference in the executable project — it is not auto-included by the core SkiaSharp package. The error path /home/site/wwwroot/libSkiaSharp.so: No such file or directory confirms the binary is absent from the deployment output.

**Recommendations:** **needs-info** — The root cause is clear (missing Linux native assets package) but we need to confirm whether the reporter has already tried adding SkiaSharp.NativeAssets.Linux.NoDependencies, and whether SkiaSharp is referenced in the app project or only in a class library.

---

## Classification

| Field | Value |
|-------|-------|
| Type | type/bug |
| Area | area/libSkiaSharp.native |
| Platforms | os/Linux |
| Backends | — |
| Tenets | tenet/reliability |
| Partner | — |
| Current labels | type/bug |

## Evidence

### Reproduction

1. Create an Azure Functions app targeting net9.0 (isolated) on Linux
2. Add SkiaSharp 3.116.0 package reference
3. Call SKDocument.CreatePdf() (or any SkiaSharp API that triggers native loading)
4. Deploy to Azure Linux Function App
5. Observe DllNotFoundException — works locally on Windows and in Docker Desktop

**Environment:** Azure Linux Function App, net9.0-isolated, SkiaSharp 3.116.0, Visual Studio on Windows for development

**Repository links:**
- https://github.com/HmichaelG/SkiaTest — Reporter's minimal reproduction project (Azure Functions app using SkiaSharp for PDF generation)

### Bug Signals

| Field | Value |
|-------|-------|
| Severity | high |
| Regression claimed | True |
| Error type | exception |
| Error message | System.DllNotFoundException: Unable to load shared library 'libSkiaSharp' or one of its dependencies. |
| Repro quality | complete |
| Target frameworks | net9.0 |

**Stack trace:**

```text
System.TypeInitializationException -> System.DllNotFoundException at SkiaSharp.SKAbstractManagedWStream..cctor() -> SkiaSharp.SKDocument.CreatePdf(Stream, SKDocumentPdfMetadata)
```

### Version Analysis

| Field | Value |
|-------|-------|
| Mentioned versions | 3.116.0, 2.88.7 |
| Worked in | 2.88.7 |
| Broke in | 3.116.0 |
| Current relevance | likely |
| Relevance reason | Linux native assets have never been auto-included; the behavior difference between 2.88.7 (.NET 8.0) and 3.116.0 (.NET 9.0) may also be a deployment environment change (Windows vs Linux Azure Functions). |

### Regression

| Field | Value |
|-------|-------|
| Is regression | False |
| Confidence | 0.75 (75%) |
| Reason | Linux NativeAssets have never been auto-included by design. The reporter is comparing SkiaSharp 2.88.7 on .NET 8.0 with 3.116.0 on .NET 9.0 on Azure Linux — both the SkiaSharp version AND the target platform may have changed (Windows to Linux). The root cause is missing NativeAssets package reference. |
| Worked in version | — |
| Broke in version | — |

## Analysis

### Technical Summary

The Azure Linux Function App fails with DllNotFoundException because libSkiaSharp.so is not deployed. On Linux, SkiaSharp.NativeAssets.Linux or SkiaSharp.NativeAssets.Linux.NoDependencies must be explicitly added as a PackageReference in the executable project — it is not auto-included by the core SkiaSharp package. The error path /home/site/wwwroot/libSkiaSharp.so: No such file or directory confirms the binary is absent from the deployment output.

### Rationale

Error pattern 'libSkiaSharp.so: cannot open shared object file: No such file or directory' maps directly to the documented troubleshooting case: native binary not found. The SkiaSharp.csproj only auto-includes Win32 and macOS native assets for non-platform TFMs (net9.0). Linux is intentionally excluded and must be added manually. Azure Functions container environments should use SkiaSharp.NativeAssets.Linux.NoDependencies which has no fontconfig or other system library dependencies.

### Key Signals

- "/home/site/wwwroot/libSkiaSharp.so: cannot open shared object file: No such file or directory" — **issue body — stack trace** (The .so file is completely absent from the deployment output — classic missing NativeAssets package reference.)
- "SkiaSharp 2.88.7 works fine on .NET 8.0; SkiaSharp 2.88.7 + fails on .NET 9.0" — **issue body** (Regression claim compares different versions AND different platforms — likely the switch to Azure Linux Functions (not just the .NET version) is the relevant change.)
- "Same issue on Azure App Services, Linux, .NET 9. Is there any workaround?" — **comment by ingweland** (Confirms the pattern is reproducible across multiple Azure Linux hosting environments (Functions + App Services).)

### Code Investigation

| File | Lines | Relevance | Finding |
|------|-------|-----------|---------|
| `binding/SkiaSharp/SkiaSharp.csproj` | — | direct | ProjectReference for NativeAssets.Win32 is included for !$(TargetFramework.Contains('-')) (generic net9.0). ProjectReference for NativeAssets.Linux is absent — Linux native assets are never auto-included by the main SkiaSharp package. |
| `binding/SkiaSharp.NativeAssets.Linux.NoDependencies/SkiaSharp.NativeAssets.Linux.NoDependencies.csproj` | — | direct | Separate package containing linux-x64, linux-arm64, linux-musl-x64, etc. native .so files. No external dependencies (only libc/libm/libpthread/libdl). Must be referenced explicitly in the application/executable project. |
| `documentation/dev/packages.md` | 183-213 | direct | Troubleshooting section confirms: error pattern 'libSkiaSharp.so: cannot open shared object file: No such file or directory' = binary not found. Fix: add SkiaSharp.NativeAssets.Linux.NoDependencies as a direct PackageReference in the executable project. |

### Workarounds

- Add <PackageReference Include="SkiaSharp.NativeAssets.Linux.NoDependencies" Version="3.116.0" /> to the Azure Functions .csproj (application project, not a class library).
- Alternatively use SkiaSharp.NativeAssets.Linux if fontconfig system font enumeration is needed, but NoDependencies is recommended for Azure Functions containers.

### Next Questions

- Has the reporter already tried adding SkiaSharp.NativeAssets.Linux or SkiaSharp.NativeAssets.Linux.NoDependencies to their project?
- Is SkiaSharp referenced in the Function app project directly, or only in a referenced class library?

### Resolution Proposals

**Hypothesis:** libSkiaSharp.so is not deployed to Azure Linux because SkiaSharp.NativeAssets.Linux.NoDependencies was not added to the Azure Functions application project.

1. **Add SkiaSharp.NativeAssets.Linux.NoDependencies** — workaround, confidence 0.95 (95%), cost/xs, validated=yes
   - Add the Linux native assets package directly to the Azure Functions .csproj. NoDependencies is correct for Azure Functions/containers as it has no external OS library requirements.

```csharp
<PackageReference Include="SkiaSharp.NativeAssets.Linux.NoDependencies" Version="3.116.0" />
```

**Recommended proposal:** Add SkiaSharp.NativeAssets.Linux.NoDependencies

**Why:** Directly resolves missing .so by ensuring it is copied to the deployment output. NoDependencies variant is appropriate for Azure Functions which run in minimal Linux containers.

## Recommendations

### Actionability

| Field | Value |
|-------|-------|
| Suggested action | needs-info |
| Confidence | 0.80 (80%) |
| Reason | The root cause is clear (missing Linux native assets package) but we need to confirm whether the reporter has already tried adding SkiaSharp.NativeAssets.Linux.NoDependencies, and whether SkiaSharp is referenced in the app project or only in a class library. |
| Suggested repro platform | linux |

### Missing Info

- Has SkiaSharp.NativeAssets.Linux or SkiaSharp.NativeAssets.Linux.NoDependencies been added to the project?
- Is SkiaSharp referenced directly in the Azure Functions app project (.csproj) or only in a referenced class library?

### Automatable Actions

| Type | Risk | Confidence | Description | Details |
|------|------|------------|-------------|---------|
| update-labels | low | 0.92 (92%) | Apply type/bug, area/libSkiaSharp.native, os/Linux labels | labels=type/bug, area/libSkiaSharp.native, os/Linux, tenet/reliability |
| add-comment | medium | 0.92 (92%) | Explain the missing Linux NativeAssets package and provide fix | — |

**Comment draft for `add-comment`:**

```markdown
Thanks for the detailed report! This is a common deployment issue on Linux. The `SkiaSharp` NuGet package does **not** auto-include the Linux native binary (`libSkiaSharp.so`) — it must be added manually.

Please add the following to your **Azure Functions application project** (the `.csproj` that produces the executable, not a class library):

```xml
<PackageReference Include="SkiaSharp.NativeAssets.Linux.NoDependencies" Version="3.116.0" />
```

The `NoDependencies` variant is the right choice for Azure Functions/containers — it has no external OS library requirements (no fontconfig, etc.).

> **Important:** The package must be in the application project. If SkiaSharp is only referenced in a class library project, the native binary may not be copied to the final output.

Could you let us know:
1. Have you already tried adding this package?
2. Is `SkiaSharp` referenced directly in the Function App project, or only in a class library?

For more details, see the [Linux deployment guide in our packages documentation](https://github.com/mono/SkiaSharp/blob/main/documentation/dev/packages.md#linux-package-selection-guide).
```

<details>
<summary>Raw JSON</summary>

```json
{
  "meta": {
    "schemaVersion": "1.0",
    "number": 3103,
    "repo": "mono/SkiaSharp",
    "analyzedAt": "2026-04-26T19:45:49Z",
    "currentLabels": [
      "type/bug"
    ]
  },
  "summary": "SkiaSharp 3.116.0 throws TypeInitializationException / DllNotFoundException when used in an Azure Linux Function App targeting .NET 9.0 (isolated) because the Linux native binary is not deployed — SkiaSharp.NativeAssets.Linux.NoDependencies must be added manually to the application project.",
  "classification": {
    "type": {
      "value": "type/bug",
      "confidence": 0.9
    },
    "area": {
      "value": "area/libSkiaSharp.native",
      "confidence": 0.92
    },
    "platforms": [
      "os/Linux"
    ],
    "tenets": [
      "tenet/reliability"
    ]
  },
  "evidence": {
    "bugSignals": {
      "severity": "high",
      "regressionClaimed": true,
      "errorType": "exception",
      "errorMessage": "System.DllNotFoundException: Unable to load shared library 'libSkiaSharp' or one of its dependencies.",
      "stackTrace": "System.TypeInitializationException -> System.DllNotFoundException at SkiaSharp.SKAbstractManagedWStream..cctor() -> SkiaSharp.SKDocument.CreatePdf(Stream, SKDocumentPdfMetadata)",
      "reproQuality": "complete",
      "targetFrameworks": [
        "net9.0"
      ]
    },
    "reproEvidence": {
      "stepsToReproduce": [
        "Create an Azure Functions app targeting net9.0 (isolated) on Linux",
        "Add SkiaSharp 3.116.0 package reference",
        "Call SKDocument.CreatePdf() (or any SkiaSharp API that triggers native loading)",
        "Deploy to Azure Linux Function App",
        "Observe DllNotFoundException — works locally on Windows and in Docker Desktop"
      ],
      "environmentDetails": "Azure Linux Function App, net9.0-isolated, SkiaSharp 3.116.0, Visual Studio on Windows for development",
      "repoLinks": [
        {
          "url": "https://github.com/HmichaelG/SkiaTest",
          "description": "Reporter's minimal reproduction project (Azure Functions app using SkiaSharp for PDF generation)"
        }
      ]
    },
    "versionAnalysis": {
      "mentionedVersions": [
        "3.116.0",
        "2.88.7"
      ],
      "workedIn": "2.88.7",
      "brokeIn": "3.116.0",
      "currentRelevance": "likely",
      "relevanceReason": "Linux native assets have never been auto-included; the behavior difference between 2.88.7 (.NET 8.0) and 3.116.0 (.NET 9.0) may also be a deployment environment change (Windows vs Linux Azure Functions)."
    },
    "regression": {
      "isRegression": false,
      "confidence": 0.75,
      "reason": "Linux NativeAssets have never been auto-included by design. The reporter is comparing SkiaSharp 2.88.7 on .NET 8.0 with 3.116.0 on .NET 9.0 on Azure Linux — both the SkiaSharp version AND the target platform may have changed (Windows to Linux). The root cause is missing NativeAssets package reference."
    }
  },
  "analysis": {
    "summary": "The Azure Linux Function App fails with DllNotFoundException because libSkiaSharp.so is not deployed. On Linux, SkiaSharp.NativeAssets.Linux or SkiaSharp.NativeAssets.Linux.NoDependencies must be explicitly added as a PackageReference in the executable project — it is not auto-included by the core SkiaSharp package. The error path /home/site/wwwroot/libSkiaSharp.so: No such file or directory confirms the binary is absent from the deployment output.",
    "rationale": "Error pattern 'libSkiaSharp.so: cannot open shared object file: No such file or directory' maps directly to the documented troubleshooting case: native binary not found. The SkiaSharp.csproj only auto-includes Win32 and macOS native assets for non-platform TFMs (net9.0). Linux is intentionally excluded and must be added manually. Azure Functions container environments should use SkiaSharp.NativeAssets.Linux.NoDependencies which has no fontconfig or other system library dependencies.",
    "keySignals": [
      {
        "text": "/home/site/wwwroot/libSkiaSharp.so: cannot open shared object file: No such file or directory",
        "source": "issue body — stack trace",
        "interpretation": "The .so file is completely absent from the deployment output — classic missing NativeAssets package reference."
      },
      {
        "text": "SkiaSharp 2.88.7 works fine on .NET 8.0; SkiaSharp 2.88.7 + fails on .NET 9.0",
        "source": "issue body",
        "interpretation": "Regression claim compares different versions AND different platforms — likely the switch to Azure Linux Functions (not just the .NET version) is the relevant change."
      },
      {
        "text": "Same issue on Azure App Services, Linux, .NET 9. Is there any workaround?",
        "source": "comment by ingweland",
        "interpretation": "Confirms the pattern is reproducible across multiple Azure Linux hosting environments (Functions + App Services)."
      }
    ],
    "codeInvestigation": [
      {
        "file": "binding/SkiaSharp/SkiaSharp.csproj",
        "finding": "ProjectReference for NativeAssets.Win32 is included for !$(TargetFramework.Contains('-')) (generic net9.0). ProjectReference for NativeAssets.Linux is absent — Linux native assets are never auto-included by the main SkiaSharp package.",
        "relevance": "direct"
      },
      {
        "file": "binding/SkiaSharp.NativeAssets.Linux.NoDependencies/SkiaSharp.NativeAssets.Linux.NoDependencies.csproj",
        "finding": "Separate package containing linux-x64, linux-arm64, linux-musl-x64, etc. native .so files. No external dependencies (only libc/libm/libpthread/libdl). Must be referenced explicitly in the application/executable project.",
        "relevance": "direct"
      },
      {
        "file": "documentation/dev/packages.md",
        "lines": "183-213",
        "finding": "Troubleshooting section confirms: error pattern 'libSkiaSharp.so: cannot open shared object file: No such file or directory' = binary not found. Fix: add SkiaSharp.NativeAssets.Linux.NoDependencies as a direct PackageReference in the executable project.",
        "relevance": "direct"
      }
    ],
    "workarounds": [
      "Add <PackageReference Include=\"SkiaSharp.NativeAssets.Linux.NoDependencies\" Version=\"3.116.0\" /> to the Azure Functions .csproj (application project, not a class library).",
      "Alternatively use SkiaSharp.NativeAssets.Linux if fontconfig system font enumeration is needed, but NoDependencies is recommended for Azure Functions containers."
    ],
    "nextQuestions": [
      "Has the reporter already tried adding SkiaSharp.NativeAssets.Linux or SkiaSharp.NativeAssets.Linux.NoDependencies to their project?",
      "Is SkiaSharp referenced in the Function app project directly, or only in a referenced class library?"
    ],
    "resolution": {
      "hypothesis": "libSkiaSharp.so is not deployed to Azure Linux because SkiaSharp.NativeAssets.Linux.NoDependencies was not added to the Azure Functions application project.",
      "proposals": [
        {
          "title": "Add SkiaSharp.NativeAssets.Linux.NoDependencies",
          "category": "workaround",
          "description": "Add the Linux native assets package directly to the Azure Functions .csproj. NoDependencies is correct for Azure Functions/containers as it has no external OS library requirements.",
          "codeSnippet": "<PackageReference Include=\"SkiaSharp.NativeAssets.Linux.NoDependencies\" Version=\"3.116.0\" />",
          "confidence": 0.95,
          "effort": "cost/xs",
          "validated": "yes"
        }
      ],
      "recommendedProposal": "Add SkiaSharp.NativeAssets.Linux.NoDependencies",
      "recommendedReason": "Directly resolves missing .so by ensuring it is copied to the deployment output. NoDependencies variant is appropriate for Azure Functions which run in minimal Linux containers."
    }
  },
  "output": {
    "actionability": {
      "suggestedAction": "needs-info",
      "confidence": 0.8,
      "reason": "The root cause is clear (missing Linux native assets package) but we need to confirm whether the reporter has already tried adding SkiaSharp.NativeAssets.Linux.NoDependencies, and whether SkiaSharp is referenced in the app project or only in a class library.",
      "suggestedReproPlatform": "linux"
    },
    "missingInfo": [
      "Has SkiaSharp.NativeAssets.Linux or SkiaSharp.NativeAssets.Linux.NoDependencies been added to the project?",
      "Is SkiaSharp referenced directly in the Azure Functions app project (.csproj) or only in a referenced class library?"
    ],
    "actions": [
      {
        "type": "update-labels",
        "description": "Apply type/bug, area/libSkiaSharp.native, os/Linux labels",
        "risk": "low",
        "confidence": 0.92,
        "labels": [
          "type/bug",
          "area/libSkiaSharp.native",
          "os/Linux",
          "tenet/reliability"
        ]
      },
      {
        "type": "add-comment",
        "description": "Explain the missing Linux NativeAssets package and provide fix",
        "risk": "medium",
        "confidence": 0.92,
        "comment": "Thanks for the detailed report! This is a common deployment issue on Linux. The `SkiaSharp` NuGet package does **not** auto-include the Linux native binary (`libSkiaSharp.so`) — it must be added manually.\n\nPlease add the following to your **Azure Functions application project** (the `.csproj` that produces the executable, not a class library):\n\n```xml\n<PackageReference Include=\"SkiaSharp.NativeAssets.Linux.NoDependencies\" Version=\"3.116.0\" />\n```\n\nThe `NoDependencies` variant is the right choice for Azure Functions/containers — it has no external OS library requirements (no fontconfig, etc.).\n\n> **Important:** The package must be in the application project. If SkiaSharp is only referenced in a class library project, the native binary may not be copied to the final output.\n\nCould you let us know:\n1. Have you already tried adding this package?\n2. Is `SkiaSharp` referenced directly in the Function App project, or only in a class library?\n\nFor more details, see the [Linux deployment guide in our packages documentation](https://github.com/mono/SkiaSharp/blob/main/documentation/dev/packages.md#linux-package-selection-guide)."
      }
    ]
  }
}
```

</details>
