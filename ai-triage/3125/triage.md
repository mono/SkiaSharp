# Issue Triage Report — #3125

| Field | Value |
|-------|-------|
| Repository | mono/SkiaSharp |
| Analyzed | 2026-04-27T20:36:42Z |
| Type | type/bug (0.90 (90%)) |
| Area | area/libSkiaSharp.native (0.95 (95%)) |
| Suggested action | close-as-duplicate (0.82 (82%)) |

**Issue Summary:** Reporter gets DllNotFoundException (0x8007007E) when running SkiaSharp 3.116.1 with NativeAssets.Win32 in a Windows Docker container based on mcr.microsoft.com/dotnet/aspnet:7.0, which runs on Windows Nano Server where FONTSUB.dll is unavailable.

**Analysis:** The reporter is using SkiaSharp.NativeAssets.Win32 in a Windows Nano Server Docker container. The Win32 native binary depends on FONTSUB.dll for XPS font subsetting, but Nano Server does not include FONTSUB.dll. The fix is to use SkiaSharp.NativeAssets.NanoServer which was built without the FONTSUB.dll dependency. This is essentially the same scenario as issue #2440.

**Recommendations:** **close-as-duplicate** — Essentially identical to #2440 — same error code 0x8007007E, same Windows container base image, same root cause (Win32 package used instead of NanoServer). The workaround (use NanoServer package) is well-documented.

---

## Classification

| Field | Value |
|-------|-------|
| Type | type/bug |
| Area | area/libSkiaSharp.native |
| Platforms | os/Windows-Nano-Server |
| Backends | — |
| Tenets | tenet/reliability |
| Partner | — |
| Current labels | type/bug, area/libSkiaSharp.native, os/Windows-Nano-Server, tenet/reliability, triage/triaged |

## Evidence

### Reproduction

1. Create an ASP.NET app targeting net7.0
2. Reference SkiaSharp 3.116.1 and SkiaSharp.NativeAssets.Win32 3.116.1
3. Build Docker image using mcr.microsoft.com/dotnet/aspnet:7.0 as runtime base (Windows container mode)
4. Run container - observe DllNotFoundException at startup

**Environment:** Windows Docker container, mcr.microsoft.com/dotnet/aspnet:7.0, SkiaSharp 3.116.1, SkiaSharp.NativeAssets.Win32 3.116.1, Visual Studio 2022, Windows 11 dev machine

**Related issues:** #2440

### Bug Signals

| Field | Value |
|-------|-------|
| Severity | high |
| Regression claimed | False |
| Error type | exception |
| Error message | Unable to load DLL 'libSkiaSharp' or one of its dependencies: The specified module could not be found. (0x8007007E) |
| Repro quality | partial |
| Target frameworks | net7.0 |

### Version Analysis

| Field | Value |
|-------|-------|
| Mentioned versions | 3.116.1 |
| Worked in | — |
| Broke in | — |
| Current relevance | likely |
| Relevance reason | The Nano Server native binary requirement has not changed — using Win32 package in a Nano Server container still fails because FONTSUB.dll is still absent. |

## Analysis

### Technical Summary

The reporter is using SkiaSharp.NativeAssets.Win32 in a Windows Nano Server Docker container. The Win32 native binary depends on FONTSUB.dll for XPS font subsetting, but Nano Server does not include FONTSUB.dll. The fix is to use SkiaSharp.NativeAssets.NanoServer which was built without the FONTSUB.dll dependency. This is essentially the same scenario as issue #2440.

### Rationale

Error code 0x8007007E is ERROR_MOD_NOT_FOUND — a dependency DLL of libSkiaSharp.dll could not be found. The mcr.microsoft.com/dotnet/aspnet Windows container runs on Windows Nano Server which has a minimal Win32 API surface. SkiaSharp provides a dedicated NanoServer native assets package built without the FONTSUB.dll dependency. Issue #2440 is identical in root cause, environment, and error message. The fix is clearly documented in packages.md and demonstrated in official Docker samples.

### Key Signals

- "Unable to load DLL 'libSkiaSharp' or one of its dependencies: The specified module could not be found. (0x8007007E)" — **issue title** (0x8007007E = ERROR_MOD_NOT_FOUND: a DLL that libSkiaSharp.dll depends on is not present in the Nano Server environment (FONTSUB.dll))
- "FROM mcr.microsoft.com/dotnet/aspnet:7.0 AS base" — **issue body** (The ASP.NET runtime Docker image for Windows is based on Windows Nano Server, which has a minimal Win32 API surface and lacks FONTSUB.dll)
- "SkiaSharp.NativeAssets.Win32 3.116.1" — **issue body** (Reporter uses the standard Win32 native assets package, which links against FONTSUB.dll — not available in Nano Server)
- "Works fine in my development environment (Windows 11, running Visual studio 2022)" — **issue body** (Confirms the issue is Nano Server / container-specific, not a general SkiaSharp defect)

### Code Investigation

| File | Lines | Relevance | Finding |
|------|-------|-----------|---------|
| `binding/SkiaSharp.NativeAssets.NanoServer/SkiaSharp.NativeAssets.NanoServer.csproj` | — | direct | Package notes explicitly state: 'does not make use of the typeface subsetting when creating XPS documents (CreateFontPackage from FONTSUB.dll)'. This is the correct package for Windows Nano Server containers — it was built to exclude the FONTSUB.dll dependency. |
| `documentation/dev/packages.md` | — | direct | NanoServer package documented as 'Windows Nano Server containers (x64 only). SkiaSharp only. Must add manually.' The manual addition requirement explains why reporters using Win32 fail — the correct package is not automatically selected. |
| `samples/Basic/DockerWebApi/SkiaSharpSample/SkiaSharpSample.csproj` | — | related | Official Docker sample references SkiaSharp.NativeAssets.Linux.NoDependencies AND SkiaSharp.NativeAssets.NanoServer together — confirms NanoServer is the required package for Windows container scenarios. |

### Workarounds

- Replace SkiaSharp.NativeAssets.Win32 with SkiaSharp.NativeAssets.NanoServer in the application project's .csproj

### Resolution Proposals

**Hypothesis:** The mcr.microsoft.com/dotnet/aspnet:7.0 Windows container runs on Nano Server. The Win32 NativeAssets package depends on FONTSUB.dll which is absent in Nano Server. Using SkiaSharp.NativeAssets.NanoServer (built without FONTSUB.dll) resolves the exception.

1. **Switch to NanoServer package** — workaround, confidence 0.92 (92%), cost/xs, validated=yes
   - Replace SkiaSharp.NativeAssets.Win32 with SkiaSharp.NativeAssets.NanoServer in the application project. The NanoServer package builds libSkiaSharp.dll without the FONTSUB.dll dependency and is designed specifically for this environment.

```csharp
<PackageReference Include="SkiaSharp.NativeAssets.NanoServer" Version="3.116.1" />
```

**Recommended proposal:** Switch to NanoServer package

**Why:** Directly addresses the root cause. The NanoServer package exists specifically for this scenario and is used in official Docker samples.

## Recommendations

### Actionability

| Field | Value |
|-------|-------|
| Suggested action | close-as-duplicate |
| Confidence | 0.82 (82%) |
| Reason | Essentially identical to #2440 — same error code 0x8007007E, same Windows container base image, same root cause (Win32 package used instead of NanoServer). The workaround (use NanoServer package) is well-documented. |
| Suggested repro platform | windows |

### Automatable Actions

| Type | Risk | Confidence | Description | Details |
|------|------|------------|-------------|---------|
| update-labels | low | 0.95 (95%) | Apply type/bug, area/libSkiaSharp.native, os/Windows-Nano-Server, tenet/reliability labels | labels=type/bug, area/libSkiaSharp.native, os/Windows-Nano-Server, tenet/reliability |
| link-duplicate | medium | 0.82 (82%) | Mark as duplicate of #2440 — same Windows Nano Server DllNotFoundException with identical error code | linkedIssue=#2440 |
| add-comment | medium | 0.90 (90%) | Explain the NanoServer package workaround to the reporter before closing | — |
| close-issue | medium | 0.82 (82%) | Close as duplicate of #2440 | stateReason=not_planned |

**Comment draft for `add-comment`:**

```markdown
Thanks for the report! This is a known issue with Windows Nano Server containers (see #2440 for the same scenario).

The `mcr.microsoft.com/dotnet/aspnet:7.0` Windows container base image runs on **Windows Nano Server**, which has a minimal Win32 API surface and does not include `FONTSUB.dll` — a dependency of the standard `libSkiaSharp.dll` from `SkiaSharp.NativeAssets.Win32`.

**Fix:** Replace `SkiaSharp.NativeAssets.Win32` with `SkiaSharp.NativeAssets.NanoServer` in your application project:

```xml
<PackageReference Include="SkiaSharp.NativeAssets.NanoServer" Version="3.116.1" />
```

The `NanoServer` package builds `libSkiaSharp.dll` without the `FONTSUB.dll` dependency. This is the package used in the official SkiaSharp Docker samples (`samples/Basic/DockerWebApi` and `samples/Basic/DockerConsole`).

Closing as a duplicate of #2440.
```

<details>
<summary>Raw JSON</summary>

```json
{
  "meta": {
    "schemaVersion": "1.0",
    "number": 3125,
    "repo": "mono/SkiaSharp",
    "analyzedAt": "2026-04-27T20:36:42Z",
    "currentLabels": [
      "type/bug",
      "area/libSkiaSharp.native",
      "os/Windows-Nano-Server",
      "tenet/reliability",
      "triage/triaged"
    ]
  },
  "summary": "Reporter gets DllNotFoundException (0x8007007E) when running SkiaSharp 3.116.1 with NativeAssets.Win32 in a Windows Docker container based on mcr.microsoft.com/dotnet/aspnet:7.0, which runs on Windows Nano Server where FONTSUB.dll is unavailable.",
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
      "os/Windows-Nano-Server"
    ],
    "tenets": [
      "tenet/reliability"
    ]
  },
  "evidence": {
    "bugSignals": {
      "severity": "high",
      "regressionClaimed": false,
      "errorType": "exception",
      "errorMessage": "Unable to load DLL 'libSkiaSharp' or one of its dependencies: The specified module could not be found. (0x8007007E)",
      "reproQuality": "partial",
      "targetFrameworks": [
        "net7.0"
      ]
    },
    "reproEvidence": {
      "stepsToReproduce": [
        "Create an ASP.NET app targeting net7.0",
        "Reference SkiaSharp 3.116.1 and SkiaSharp.NativeAssets.Win32 3.116.1",
        "Build Docker image using mcr.microsoft.com/dotnet/aspnet:7.0 as runtime base (Windows container mode)",
        "Run container - observe DllNotFoundException at startup"
      ],
      "environmentDetails": "Windows Docker container, mcr.microsoft.com/dotnet/aspnet:7.0, SkiaSharp 3.116.1, SkiaSharp.NativeAssets.Win32 3.116.1, Visual Studio 2022, Windows 11 dev machine",
      "relatedIssues": [
        2440
      ]
    },
    "versionAnalysis": {
      "mentionedVersions": [
        "3.116.1"
      ],
      "currentRelevance": "likely",
      "relevanceReason": "The Nano Server native binary requirement has not changed — using Win32 package in a Nano Server container still fails because FONTSUB.dll is still absent."
    }
  },
  "analysis": {
    "summary": "The reporter is using SkiaSharp.NativeAssets.Win32 in a Windows Nano Server Docker container. The Win32 native binary depends on FONTSUB.dll for XPS font subsetting, but Nano Server does not include FONTSUB.dll. The fix is to use SkiaSharp.NativeAssets.NanoServer which was built without the FONTSUB.dll dependency. This is essentially the same scenario as issue #2440.",
    "rationale": "Error code 0x8007007E is ERROR_MOD_NOT_FOUND — a dependency DLL of libSkiaSharp.dll could not be found. The mcr.microsoft.com/dotnet/aspnet Windows container runs on Windows Nano Server which has a minimal Win32 API surface. SkiaSharp provides a dedicated NanoServer native assets package built without the FONTSUB.dll dependency. Issue #2440 is identical in root cause, environment, and error message. The fix is clearly documented in packages.md and demonstrated in official Docker samples.",
    "keySignals": [
      {
        "text": "Unable to load DLL 'libSkiaSharp' or one of its dependencies: The specified module could not be found. (0x8007007E)",
        "source": "issue title",
        "interpretation": "0x8007007E = ERROR_MOD_NOT_FOUND: a DLL that libSkiaSharp.dll depends on is not present in the Nano Server environment (FONTSUB.dll)"
      },
      {
        "text": "FROM mcr.microsoft.com/dotnet/aspnet:7.0 AS base",
        "source": "issue body",
        "interpretation": "The ASP.NET runtime Docker image for Windows is based on Windows Nano Server, which has a minimal Win32 API surface and lacks FONTSUB.dll"
      },
      {
        "text": "SkiaSharp.NativeAssets.Win32 3.116.1",
        "source": "issue body",
        "interpretation": "Reporter uses the standard Win32 native assets package, which links against FONTSUB.dll — not available in Nano Server"
      },
      {
        "text": "Works fine in my development environment (Windows 11, running Visual studio 2022)",
        "source": "issue body",
        "interpretation": "Confirms the issue is Nano Server / container-specific, not a general SkiaSharp defect"
      }
    ],
    "codeInvestigation": [
      {
        "file": "binding/SkiaSharp.NativeAssets.NanoServer/SkiaSharp.NativeAssets.NanoServer.csproj",
        "finding": "Package notes explicitly state: 'does not make use of the typeface subsetting when creating XPS documents (CreateFontPackage from FONTSUB.dll)'. This is the correct package for Windows Nano Server containers — it was built to exclude the FONTSUB.dll dependency.",
        "relevance": "direct"
      },
      {
        "file": "documentation/dev/packages.md",
        "finding": "NanoServer package documented as 'Windows Nano Server containers (x64 only). SkiaSharp only. Must add manually.' The manual addition requirement explains why reporters using Win32 fail — the correct package is not automatically selected.",
        "relevance": "direct"
      },
      {
        "file": "samples/Basic/DockerWebApi/SkiaSharpSample/SkiaSharpSample.csproj",
        "finding": "Official Docker sample references SkiaSharp.NativeAssets.Linux.NoDependencies AND SkiaSharp.NativeAssets.NanoServer together — confirms NanoServer is the required package for Windows container scenarios.",
        "relevance": "related"
      }
    ],
    "workarounds": [
      "Replace SkiaSharp.NativeAssets.Win32 with SkiaSharp.NativeAssets.NanoServer in the application project's .csproj"
    ],
    "resolution": {
      "hypothesis": "The mcr.microsoft.com/dotnet/aspnet:7.0 Windows container runs on Nano Server. The Win32 NativeAssets package depends on FONTSUB.dll which is absent in Nano Server. Using SkiaSharp.NativeAssets.NanoServer (built without FONTSUB.dll) resolves the exception.",
      "proposals": [
        {
          "title": "Switch to NanoServer package",
          "description": "Replace SkiaSharp.NativeAssets.Win32 with SkiaSharp.NativeAssets.NanoServer in the application project. The NanoServer package builds libSkiaSharp.dll without the FONTSUB.dll dependency and is designed specifically for this environment.",
          "category": "workaround",
          "codeSnippet": "<PackageReference Include=\"SkiaSharp.NativeAssets.NanoServer\" Version=\"3.116.1\" />",
          "confidence": 0.92,
          "effort": "cost/xs",
          "validated": "yes"
        }
      ],
      "recommendedProposal": "Switch to NanoServer package",
      "recommendedReason": "Directly addresses the root cause. The NanoServer package exists specifically for this scenario and is used in official Docker samples."
    }
  },
  "output": {
    "actionability": {
      "suggestedAction": "close-as-duplicate",
      "confidence": 0.82,
      "reason": "Essentially identical to #2440 — same error code 0x8007007E, same Windows container base image, same root cause (Win32 package used instead of NanoServer). The workaround (use NanoServer package) is well-documented.",
      "suggestedReproPlatform": "windows"
    },
    "actions": [
      {
        "type": "update-labels",
        "description": "Apply type/bug, area/libSkiaSharp.native, os/Windows-Nano-Server, tenet/reliability labels",
        "risk": "low",
        "confidence": 0.95,
        "labels": [
          "type/bug",
          "area/libSkiaSharp.native",
          "os/Windows-Nano-Server",
          "tenet/reliability"
        ]
      },
      {
        "type": "link-duplicate",
        "description": "Mark as duplicate of #2440 — same Windows Nano Server DllNotFoundException with identical error code",
        "risk": "medium",
        "confidence": 0.82,
        "linkedIssue": 2440
      },
      {
        "type": "add-comment",
        "description": "Explain the NanoServer package workaround to the reporter before closing",
        "risk": "medium",
        "confidence": 0.9,
        "comment": "Thanks for the report! This is a known issue with Windows Nano Server containers (see #2440 for the same scenario).\n\nThe `mcr.microsoft.com/dotnet/aspnet:7.0` Windows container base image runs on **Windows Nano Server**, which has a minimal Win32 API surface and does not include `FONTSUB.dll` — a dependency of the standard `libSkiaSharp.dll` from `SkiaSharp.NativeAssets.Win32`.\n\n**Fix:** Replace `SkiaSharp.NativeAssets.Win32` with `SkiaSharp.NativeAssets.NanoServer` in your application project:\n\n```xml\n<PackageReference Include=\"SkiaSharp.NativeAssets.NanoServer\" Version=\"3.116.1\" />\n```\n\nThe `NanoServer` package builds `libSkiaSharp.dll` without the `FONTSUB.dll` dependency. This is the package used in the official SkiaSharp Docker samples (`samples/Basic/DockerWebApi` and `samples/Basic/DockerConsole`).\n\nClosing as a duplicate of #2440."
      },
      {
        "type": "close-issue",
        "description": "Close as duplicate of #2440",
        "risk": "medium",
        "confidence": 0.82,
        "stateReason": "not_planned"
      }
    ]
  }
}
```

</details>
