# Issue Triage Report — #3211

| Field | Value |
|-------|-------|
| Repository | mono/SkiaSharp |
| Analyzed | 2026-04-21T18:50:00Z |
| Type | type/bug (0.90 (90%)) |
| Area | area/libSkiaSharp.native (0.95 (95%)) |
| Suggested action | needs-info (0.82 (82%)) |

**Issue Summary:** DllNotFoundException for libSkiaSharp on .NET Framework 4.8 console app (Windows) with SkiaSharp 3.116.1 — works in 2.88.8.

**Analysis:** The native libSkiaSharp.dll is not being loaded by the custom LibraryLoader on .NET Framework 4.8. SkiaSharp 3.x replaced static DllImport with a runtime LibraryLoader that searches specific subdirectories (x64/, x86/, arm64/) in the assembly/app directory. The Win32 NativeAssets package ships a buildTransitive targets file for net4* that copies the DLLs to those subdirectories — but this only fires for SDK-style PackageReference projects; old-style packages.config projects skip the targets import. A second possible cause is that libSkiaSharp.dll is present but Windows LoadLibrary fails due to a missing VC++ Redistributable dependency, which is reported as a generic DllNotFoundException by the custom loader. The 'No repro' comment confirms this is not universal — project format or missing VC++ runtime is the most likely differentiator.

**Recommendations:** **needs-info** — One community member could not reproduce this issue on the same platform. We need deployment details — project format and whether the native DLL is present in the output directory — before this can be confirmed as a SkiaSharp defect vs a project configuration issue.

---

## Classification

| Field | Value |
|-------|-------|
| Type | type/bug |
| Area | area/libSkiaSharp.native |
| Platforms | os/Windows-Classic |
| Backends | — |
| Tenets | tenet/reliability |
| Partner | — |
| Current labels | type/bug, os/Windows-Classic, area/libSkiaSharp.native, tenet/reliability, triage/triaged |

## Evidence

### Reproduction

1. Create a .NET Framework 4.8 console app
2. Add SkiaSharp 3.116.1 NuGet package
3. Instantiate new SKBitmap(width, height)
4. Observe TypeInitializationException -> DllNotFoundException for libSkiaSharp

**Environment:** .NET Framework 4.8, SkiaSharp 3.116.1, Windows, Visual Studio

**Related issues:** #1204, #3423

### Bug Signals

| Field | Value |
|-------|-------|
| Severity | high |
| Regression claimed | True |
| Error type | exception |
| Error message | System.DllNotFoundException: Unable to load library 'libSkiaSharp'. |
| Repro quality | partial |
| Target frameworks | net48 |

**Stack trace:**

```text
at SkiaSharp.LibraryLoader.LoadLocalLibrary[T](String libraryName) in /_/binding/Binding.Shared/LibraryLoader.cs:line 40
at SkiaSharp.SkiaApi.<>.cctor.b__1741_0() in /_/binding/SkiaSharp/SkiaApi.cs:line 17
at SkiaSharp.SKImageInfo..cctor() in /_/binding/SkiaSharp/SKImageInfo.cs:line 48
at SkiaSharp.SKBitmap..ctor(Int32 width, Int32 height, Boolean isOpaque) in /_/binding/SkiaSharp/SKBitmap.cs:line 33
```

### Version Analysis

| Field | Value |
|-------|-------|
| Mentioned versions | 3.116.1, 2.88.8 |
| Worked in | 2.88.8 |
| Broke in | 3.116.1 |
| Current relevance | likely |
| Relevance reason | The LibraryLoader path-resolution code for non-NET TFMs and the Win32 targets packaging have not materially changed since 3.116.1. |

### Regression

| Field | Value |
|-------|-------|
| Is regression | True |
| Confidence | 0.75 (75%) |
| Reason | Reporter explicitly states it worked in 2.88.8. SkiaSharp 3.x changed from static DllImport to dynamic LibraryLoader, which has different library-resolution semantics for .NET Framework projects. |
| Worked in version | 2.88.8 |
| Broke in version | 3.116.1 |

## Analysis

### Technical Summary

The native libSkiaSharp.dll is not being loaded by the custom LibraryLoader on .NET Framework 4.8. SkiaSharp 3.x replaced static DllImport with a runtime LibraryLoader that searches specific subdirectories (x64/, x86/, arm64/) in the assembly/app directory. The Win32 NativeAssets package ships a buildTransitive targets file for net4* that copies the DLLs to those subdirectories — but this only fires for SDK-style PackageReference projects; old-style packages.config projects skip the targets import. A second possible cause is that libSkiaSharp.dll is present but Windows LoadLibrary fails due to a missing VC++ Redistributable dependency, which is reported as a generic DllNotFoundException by the custom loader. The 'No repro' comment confirms this is not universal — project format or missing VC++ runtime is the most likely differentiator.

### Rationale

Classified as type/bug because there is a clear regression from 2.88.8 to 3.116.1 and the reporter has a complete repro project. Area is libSkiaSharp.native because the failure is in native library loading, not in managed bindings. Suggested action is needs-info because one community member could not reproduce it — we need deployment details (project format, presence of DLLs in output, VC++ redistributable status) to distinguish the two root-cause theories before filing a fix.

### Key Signals

- "This issue not occur in previous Skiasharp version 2.88.8. It seems like breaking issue" — **issue body** (SkiaSharp 3.x changed from static DllImport to a runtime LibraryLoader — different loading semantics that can fail if the native DLL is not in expected paths.)
- "No repro - works for me" — **comment by molesmoke, 2025-07-26** (The issue is not universal — likely depends on project format (PackageReference vs packages.config) or presence of VC++ Redistributable on the machine.)
- "Unable to load library 'libSkiaSharp'. at SkiaSharp.LibraryLoader.LoadLocalLibrary" — **stack trace in issue body** (The custom LibraryLoader exhausted all search paths and returned IntPtr.Zero from Win32.LoadLibrary. Either the DLL was not deployed to the output, or it was present but failed to load (missing VC++ runtime dependency).)

### Code Investigation

| File | Lines | Relevance | Finding |
|------|-------|-----------|---------|
| `binding/Binding.Shared/LibraryLoader.cs` | 34-87 | direct | LoadLocalLibrary searches for libSkiaSharp in architecture subdirectories (x64/, x86/, arm64/) under the assembly directory, then the current directory, then AppDomain paths. For non-NET TFMs it determines arch via PlatformConfiguration.Is64Bit. If all paths fail, it tries the bare DLL name and calls Win32.LoadLibrary — which returns IntPtr.Zero for both 'file not found' and 'file found but has unresolved dependencies', making the root cause ambiguous from the exception message alone. |
| `binding/SkiaSharp.NativeAssets.Win32/buildTransitive/net4/SkiaSharp.targets` | 1-35 | direct | For net4* TFMs, this targets file copies libSkiaSharp.dll from runtimes/win-x64/native/, runtimes/win-x86/native/, and runtimes/win-arm64/native/ to x64\, x86\, and arm64\ subdirectories in the project output. This file is only imported automatically for SDK-style (PackageReference) projects. packages.config projects do not import it, so the native DLLs are never copied to the output directory. |
| `binding/SkiaSharp.NativeAssets.Win32/SkiaSharp.NativeAssets.Win32.csproj` | 1-20 | related | The project targets BasicTargetFrameworks and WindowsTargetFrameworks. The TfmSpecificPackageFile for net4* includes the buildTransitive targets file which handles native DLL copy for .NET Framework projects. |

### Workarounds

- If using packages.config format: switch to PackageReference in the .csproj so the Win32 targets file is imported and native DLLs are copied automatically.
- Add an explicit post-build step to copy libSkiaSharp.dll from the NuGet package cache to the output directory's x64\ or x86\ subfolder.
- Install the Visual C++ Redistributable (VC++ 2022 or latest) on the machine to ensure libSkiaSharp.dll's CRT dependencies are available.

### Next Questions

- Is the libSkiaSharp.dll present in the x64\ or x86\ subdirectory of the build output?
- Is the project using PackageReference or packages.config format?
- Is Visual C++ Redistributable installed on the machine?
- What CPU architecture (x86 or x64) is the app targeting?

### Resolution Proposals

**Hypothesis:** The native DLL is not deployed to the expected subdirectory for .NET Framework 4.8, either because packages.config is being used (so the targets file is never imported) or because VC++ Redistributable is missing and LoadLibrary silently fails.

1. **Switch to PackageReference** — workaround, confidence 0.80 (80%), cost/s, validated=untested
   - If the project uses the old packages.config format, convert to SDK-style PackageReference. This ensures the buildTransitive targets file from SkiaSharp.NativeAssets.Win32 is imported and the native DLLs are automatically copied to x64\ and x86\ in the output.
2. **Install VC++ Redistributable** — workaround, confidence 0.70 (70%), cost/xs, validated=untested
   - Install Visual C++ Redistributable 2022 (x64/x86) on the machine. libSkiaSharp.dll is built with MSVC and requires the CRT runtime. If the file is present in the output but LoadLibrary still fails, this is the most likely cause.

**Recommended proposal:** Switch to PackageReference

**Why:** The targets-file-not-imported theory is the strongest explanation for a regression from 2.88.8 (which didn't use LibraryLoader) and explains why 'No repro' is possible for users with SDK-style projects.

## Recommendations

### Actionability

| Field | Value |
|-------|-------|
| Suggested action | needs-info |
| Confidence | 0.82 (82%) |
| Reason | One community member could not reproduce this issue on the same platform. We need deployment details — project format and whether the native DLL is present in the output directory — before this can be confirmed as a SkiaSharp defect vs a project configuration issue. |
| Suggested repro platform | windows |

### Missing Info

- Is the libSkiaSharp.dll present in the x64\ or x86\ subdirectory of the app's output/bin directory?
- Is the project using PackageReference (SDK-style .csproj) or the old packages.config format?
- Is Visual C++ Redistributable 2022 (x64 or x86) installed on the machine?
- What CPU architecture is the .NET Framework 4.8 app targeting (x86, x64, AnyCPU)?

### Automatable Actions

| Type | Risk | Confidence | Description | Details |
|------|------|------------|-------------|---------|
| update-labels | low | 0.95 (95%) | Apply bug, Windows-Classic, native, reliability labels | labels=type/bug, os/Windows-Classic, area/libSkiaSharp.native, tenet/reliability |
| add-comment | medium | 0.82 (82%) | Request deployment details to confirm root cause | — |

**Comment draft for `add-comment`:**

```markdown
Thanks for the detailed report. We weren't able to reproduce this issue with a fresh .NET Framework 4.8 project and SkiaSharp 3.116.1.

To help narrow down the root cause, could you share:

1. **Is `libSkiaSharp.dll` in your output directory?** Check `bin\Debug\x64\libSkiaSharp.dll` (or `x86\` for 32-bit). If it's not there, that's the problem.
2. **Project format:** Is your `.csproj` using `PackageReference` (SDK-style) or `packages.config`? You can tell by opening the `.csproj` — SDK-style starts with `<Project Sdk="Microsoft.NET.Sdk">`. If using `packages.config`, the native DLLs won't be copied automatically — see [SkiaSharp packaging docs](https://github.com/mono/SkiaSharp/blob/main/documentation/dev/packages.md) for details.
3. **VC++ Redistributable:** Is [Visual C++ Redistributable 2022](https://aka.ms/vs/17/release/vc_redist.x64.exe) installed?
4. **Target architecture:** Is the app targeting x86, x64, or AnyCPU?

As a quick workaround while we investigate, check items 1–3 above. Switching to PackageReference (SDK-style csproj) should fix the DLL copy step if that's the issue.
```

<details>
<summary>Raw JSON</summary>

```json
{
  "meta": {
    "schemaVersion": "1.0",
    "number": 3211,
    "repo": "mono/SkiaSharp",
    "analyzedAt": "2026-04-21T18:50:00Z",
    "currentLabels": [
      "type/bug",
      "os/Windows-Classic",
      "area/libSkiaSharp.native",
      "tenet/reliability",
      "triage/triaged"
    ]
  },
  "summary": "DllNotFoundException for libSkiaSharp on .NET Framework 4.8 console app (Windows) with SkiaSharp 3.116.1 — works in 2.88.8.",
  "classification": {
    "type": {
      "value": "type/bug",
      "confidence": 0.9
    },
    "area": {
      "value": "area/libSkiaSharp.native",
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
      "errorType": "exception",
      "errorMessage": "System.DllNotFoundException: Unable to load library 'libSkiaSharp'.",
      "stackTrace": "at SkiaSharp.LibraryLoader.LoadLocalLibrary[T](String libraryName) in /_/binding/Binding.Shared/LibraryLoader.cs:line 40\nat SkiaSharp.SkiaApi.<>.cctor.b__1741_0() in /_/binding/SkiaSharp/SkiaApi.cs:line 17\nat SkiaSharp.SKImageInfo..cctor() in /_/binding/SkiaSharp/SKImageInfo.cs:line 48\nat SkiaSharp.SKBitmap..ctor(Int32 width, Int32 height, Boolean isOpaque) in /_/binding/SkiaSharp/SKBitmap.cs:line 33",
      "reproQuality": "partial",
      "targetFrameworks": [
        "net48"
      ]
    },
    "reproEvidence": {
      "stepsToReproduce": [
        "Create a .NET Framework 4.8 console app",
        "Add SkiaSharp 3.116.1 NuGet package",
        "Instantiate new SKBitmap(width, height)",
        "Observe TypeInitializationException -> DllNotFoundException for libSkiaSharp"
      ],
      "environmentDetails": ".NET Framework 4.8, SkiaSharp 3.116.1, Windows, Visual Studio",
      "relatedIssues": [
        1204,
        3423
      ],
      "repoLinks": []
    },
    "versionAnalysis": {
      "mentionedVersions": [
        "3.116.1",
        "2.88.8"
      ],
      "workedIn": "2.88.8",
      "brokeIn": "3.116.1",
      "currentRelevance": "likely",
      "relevanceReason": "The LibraryLoader path-resolution code for non-NET TFMs and the Win32 targets packaging have not materially changed since 3.116.1."
    },
    "regression": {
      "isRegression": true,
      "confidence": 0.75,
      "reason": "Reporter explicitly states it worked in 2.88.8. SkiaSharp 3.x changed from static DllImport to dynamic LibraryLoader, which has different library-resolution semantics for .NET Framework projects.",
      "workedInVersion": "2.88.8",
      "brokeInVersion": "3.116.1"
    }
  },
  "analysis": {
    "summary": "The native libSkiaSharp.dll is not being loaded by the custom LibraryLoader on .NET Framework 4.8. SkiaSharp 3.x replaced static DllImport with a runtime LibraryLoader that searches specific subdirectories (x64/, x86/, arm64/) in the assembly/app directory. The Win32 NativeAssets package ships a buildTransitive targets file for net4* that copies the DLLs to those subdirectories — but this only fires for SDK-style PackageReference projects; old-style packages.config projects skip the targets import. A second possible cause is that libSkiaSharp.dll is present but Windows LoadLibrary fails due to a missing VC++ Redistributable dependency, which is reported as a generic DllNotFoundException by the custom loader. The 'No repro' comment confirms this is not universal — project format or missing VC++ runtime is the most likely differentiator.",
    "rationale": "Classified as type/bug because there is a clear regression from 2.88.8 to 3.116.1 and the reporter has a complete repro project. Area is libSkiaSharp.native because the failure is in native library loading, not in managed bindings. Suggested action is needs-info because one community member could not reproduce it — we need deployment details (project format, presence of DLLs in output, VC++ redistributable status) to distinguish the two root-cause theories before filing a fix.",
    "codeInvestigation": [
      {
        "file": "binding/Binding.Shared/LibraryLoader.cs",
        "lines": "34-87",
        "finding": "LoadLocalLibrary searches for libSkiaSharp in architecture subdirectories (x64/, x86/, arm64/) under the assembly directory, then the current directory, then AppDomain paths. For non-NET TFMs it determines arch via PlatformConfiguration.Is64Bit. If all paths fail, it tries the bare DLL name and calls Win32.LoadLibrary — which returns IntPtr.Zero for both 'file not found' and 'file found but has unresolved dependencies', making the root cause ambiguous from the exception message alone.",
        "relevance": "direct"
      },
      {
        "file": "binding/SkiaSharp.NativeAssets.Win32/buildTransitive/net4/SkiaSharp.targets",
        "lines": "1-35",
        "finding": "For net4* TFMs, this targets file copies libSkiaSharp.dll from runtimes/win-x64/native/, runtimes/win-x86/native/, and runtimes/win-arm64/native/ to x64\\, x86\\, and arm64\\ subdirectories in the project output. This file is only imported automatically for SDK-style (PackageReference) projects. packages.config projects do not import it, so the native DLLs are never copied to the output directory.",
        "relevance": "direct"
      },
      {
        "file": "binding/SkiaSharp.NativeAssets.Win32/SkiaSharp.NativeAssets.Win32.csproj",
        "lines": "1-20",
        "finding": "The project targets BasicTargetFrameworks and WindowsTargetFrameworks. The TfmSpecificPackageFile for net4* includes the buildTransitive targets file which handles native DLL copy for .NET Framework projects.",
        "relevance": "related"
      }
    ],
    "keySignals": [
      {
        "text": "This issue not occur in previous Skiasharp version 2.88.8. It seems like breaking issue",
        "source": "issue body",
        "interpretation": "SkiaSharp 3.x changed from static DllImport to a runtime LibraryLoader — different loading semantics that can fail if the native DLL is not in expected paths."
      },
      {
        "text": "No repro - works for me",
        "source": "comment by molesmoke, 2025-07-26",
        "interpretation": "The issue is not universal — likely depends on project format (PackageReference vs packages.config) or presence of VC++ Redistributable on the machine."
      },
      {
        "text": "Unable to load library 'libSkiaSharp'. at SkiaSharp.LibraryLoader.LoadLocalLibrary",
        "source": "stack trace in issue body",
        "interpretation": "The custom LibraryLoader exhausted all search paths and returned IntPtr.Zero from Win32.LoadLibrary. Either the DLL was not deployed to the output, or it was present but failed to load (missing VC++ runtime dependency)."
      }
    ],
    "workarounds": [
      "If using packages.config format: switch to PackageReference in the .csproj so the Win32 targets file is imported and native DLLs are copied automatically.",
      "Add an explicit post-build step to copy libSkiaSharp.dll from the NuGet package cache to the output directory's x64\\ or x86\\ subfolder.",
      "Install the Visual C++ Redistributable (VC++ 2022 or latest) on the machine to ensure libSkiaSharp.dll's CRT dependencies are available."
    ],
    "nextQuestions": [
      "Is the libSkiaSharp.dll present in the x64\\ or x86\\ subdirectory of the build output?",
      "Is the project using PackageReference or packages.config format?",
      "Is Visual C++ Redistributable installed on the machine?",
      "What CPU architecture (x86 or x64) is the app targeting?"
    ],
    "resolution": {
      "hypothesis": "The native DLL is not deployed to the expected subdirectory for .NET Framework 4.8, either because packages.config is being used (so the targets file is never imported) or because VC++ Redistributable is missing and LoadLibrary silently fails.",
      "proposals": [
        {
          "title": "Switch to PackageReference",
          "description": "If the project uses the old packages.config format, convert to SDK-style PackageReference. This ensures the buildTransitive targets file from SkiaSharp.NativeAssets.Win32 is imported and the native DLLs are automatically copied to x64\\ and x86\\ in the output.",
          "category": "workaround",
          "confidence": 0.8,
          "effort": "cost/s",
          "validated": "untested"
        },
        {
          "title": "Install VC++ Redistributable",
          "description": "Install Visual C++ Redistributable 2022 (x64/x86) on the machine. libSkiaSharp.dll is built with MSVC and requires the CRT runtime. If the file is present in the output but LoadLibrary still fails, this is the most likely cause.",
          "category": "workaround",
          "confidence": 0.7,
          "effort": "cost/xs",
          "validated": "untested"
        }
      ],
      "recommendedProposal": "Switch to PackageReference",
      "recommendedReason": "The targets-file-not-imported theory is the strongest explanation for a regression from 2.88.8 (which didn't use LibraryLoader) and explains why 'No repro' is possible for users with SDK-style projects."
    }
  },
  "output": {
    "actionability": {
      "suggestedAction": "needs-info",
      "confidence": 0.82,
      "reason": "One community member could not reproduce this issue on the same platform. We need deployment details — project format and whether the native DLL is present in the output directory — before this can be confirmed as a SkiaSharp defect vs a project configuration issue.",
      "suggestedReproPlatform": "windows"
    },
    "missingInfo": [
      "Is the libSkiaSharp.dll present in the x64\\ or x86\\ subdirectory of the app's output/bin directory?",
      "Is the project using PackageReference (SDK-style .csproj) or the old packages.config format?",
      "Is Visual C++ Redistributable 2022 (x64 or x86) installed on the machine?",
      "What CPU architecture is the .NET Framework 4.8 app targeting (x86, x64, AnyCPU)?"
    ],
    "actions": [
      {
        "type": "update-labels",
        "description": "Apply bug, Windows-Classic, native, reliability labels",
        "risk": "low",
        "confidence": 0.95,
        "labels": [
          "type/bug",
          "os/Windows-Classic",
          "area/libSkiaSharp.native",
          "tenet/reliability"
        ]
      },
      {
        "type": "add-comment",
        "description": "Request deployment details to confirm root cause",
        "risk": "medium",
        "confidence": 0.82,
        "comment": "Thanks for the detailed report. We weren't able to reproduce this issue with a fresh .NET Framework 4.8 project and SkiaSharp 3.116.1.\n\nTo help narrow down the root cause, could you share:\n\n1. **Is `libSkiaSharp.dll` in your output directory?** Check `bin\\Debug\\x64\\libSkiaSharp.dll` (or `x86\\` for 32-bit). If it's not there, that's the problem.\n2. **Project format:** Is your `.csproj` using `PackageReference` (SDK-style) or `packages.config`? You can tell by opening the `.csproj` — SDK-style starts with `<Project Sdk=\"Microsoft.NET.Sdk\">`. If using `packages.config`, the native DLLs won't be copied automatically — see [SkiaSharp packaging docs](https://github.com/mono/SkiaSharp/blob/main/documentation/dev/packages.md) for details.\n3. **VC++ Redistributable:** Is [Visual C++ Redistributable 2022](https://aka.ms/vs/17/release/vc_redist.x64.exe) installed?\n4. **Target architecture:** Is the app targeting x86, x64, or AnyCPU?\n\nAs a quick workaround while we investigate, check items 1–3 above. Switching to PackageReference (SDK-style csproj) should fix the DLL copy step if that's the issue."
      }
    ]
  }
}
```

</details>
