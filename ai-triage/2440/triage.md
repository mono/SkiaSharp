# Issue Triage Report — #2440

| Field | Value |
|-------|-------|
| Repository | mono/SkiaSharp |
| Analyzed | 2026-04-25T20:25:00Z |
| Type | type/bug (0.90 (90%)) |
| Area | area/libSkiaSharp.native (0.95 (95%)) |
| Suggested action | needs-info (0.80 (80%)) |

**Issue Summary:** DllNotFoundException loading libSkiaSharp in Windows Docker containers (mcr.microsoft.com/dotnet/aspnet:6.0) due to missing FONTSUB.dll dependency in the container image; the SkiaSharp.NativeAssets.NanoServer package resolves this by providing a libSkiaSharp build compiled without the FONTSUB.dll dependency.

**Analysis:** libSkiaSharp.dll (Win32 variant) depends on FONTSUB.dll for font subsetting in XPS document creation, but this DLL is absent from modern Windows container images like mcr.microsoft.com/dotnet/aspnet:6.0. The SkiaSharp.NativeAssets.NanoServer package provides a libSkiaSharp compiled without this dependency and is the fix for Windows containers.

**Recommendations:** **needs-info** — Workaround is well-established (NanoServer package) and confirmed in comments, but the original reporter has never responded to confirm resolution. Asking them to verify the fix before closing.

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

## Evidence

### Reproduction

**Environment:** mcr.microsoft.com/dotnet/aspnet:6.0 Windows container on Azure DevOps, SkiaSharp 2.88.3, Visual Studio

**Related issues:** #591, #676, #1341

**Repository links:**
- https://github.com/mono/SkiaSharp/issues/1341 — Related Linux DllNotFoundException issue
- https://github.com/mono/SkiaSharp/issues/591 — Original Windows Docker DllNotFoundException issue (closed/resolved, locked)
- https://github.com/mono/SkiaSharp/issues/676 — NanoServer support history (closed)

### Bug Signals

| Field | Value |
|-------|-------|
| Severity | medium |
| Regression claimed | — |
| Error type | exception |
| Error message | Unable to load DLL 'libSkiaSharp' or one of its dependencies: The specified module could not be found. (0x8007007E) |
| Repro quality | partial |
| Target frameworks | net6.0 |

### Version Analysis

| Field | Value |
|-------|-------|
| Mentioned versions | 2.88.3 |
| Worked in | — |
| Broke in | — |
| Current relevance | likely |
| Relevance reason | The Win32 NativeAssets still depend on FONTSUB.dll which is absent from modern Windows container base images. The NanoServer package continues to be the recommended solution. |

## Analysis

### Technical Summary

libSkiaSharp.dll (Win32 variant) depends on FONTSUB.dll for font subsetting in XPS document creation, but this DLL is absent from modern Windows container images like mcr.microsoft.com/dotnet/aspnet:6.0. The SkiaSharp.NativeAssets.NanoServer package provides a libSkiaSharp compiled without this dependency and is the fix for Windows containers.

### Rationale

Error 0x8007007E is ERROR_MOD_NOT_FOUND — libSkiaSharp.dll loads but its FONTSUB.dll dependency is absent from the Windows container. The NanoServer package csproj explicitly documents this scenario and is the correct fix. Classified as type/bug because standard SkiaSharp NuGet usage silently fails in a common deployment scenario (Windows containers) with no documentation warning at the point of failure.

### Key Signals

- "Unable to load DLL 'libSkiaSharp' or one of its dependencies: The specified module could not be found. (0x8007007E)" — **issue body** (Windows error 0x8007007E (ERROR_MOD_NOT_FOUND) indicates a dependency of libSkiaSharp.dll is missing from the container, not libSkiaSharp itself.)
- "Using mcr.microsoft.com/dotnet/aspnet:6.0 as runtime Docker image" — **issue body** (Standard Windows container image that does not include FONTSUB.dll, a font subsetting library required by the Win32 libSkiaSharp build.)
- "Switch to SkiaSharp.NativeAssets.NanoServer... does not include dependencies on things not in NanoServer, like font subsetting" — **comment by hraftery** (Community-confirmed workaround: NanoServer package removes FONTSUB.dll dependency and works in all Windows containers lacking that DLL.)
- "fontsub.dll is the last offending dependency, and it relates to a chunk of functionality that may not be missed by many - hence the NanoServer variant was born!" — **comment by hraftery referencing #591** (Historical context: NanoServer package was specifically created to address FONTSUB.dll unavailability in Windows containers.)

### Code Investigation

| File | Lines | Relevance | Finding |
|------|-------|-----------|---------|
| `binding/SkiaSharp.NativeAssets.NanoServer/SkiaSharp.NativeAssets.NanoServer.csproj` | — | direct | PackageNotes explicitly states: 'This variation of the Windows native assets includes the build of libSkiaSharp.dll that does not make use of the typeface subsetting when creating XPS documents (CreateFontPackage from FONTSUB.dll).' Confirms this package was designed for environments without FONTSUB.dll, including all Windows containers. |
| `binding/SkiaSharp.NativeAssets.Win32/SkiaSharp.NativeAssets.Win32.csproj` | — | related | Win32 package ships x64, x86, and arm64 binaries with no documented dependency requirements. Absence of FONTSUB.dll documentation here is a contributing factor to user confusion when deploying to Windows containers. |

### Workarounds

- Add SkiaSharp.NativeAssets.NanoServer as a PackageReference in the application project instead of relying on auto-included Win32 assets. This build omits FONTSUB.dll dependency (XPS font subsetting not available).
- Switch to an older Windows container image such as mcr.microsoft.com/dotnet/runtime:5.0-windowsservercore-ltsc2019 which includes FONTSUB.dll. Disadvantage: large image (5.41 GB), .NET 5.0 only, outdated OS.

### Next Questions

- Has the original reporter confirmed whether SkiaSharp.NativeAssets.NanoServer resolves their issue on mcr.microsoft.com/dotnet/aspnet:6.0?
- Should documentation/dev/packages.md be updated to clarify that the NanoServer package is required for all Windows containers (not just Nano Server), since FONTSUB.dll is absent from standard aspnet/runtime base images?

### Resolution Proposals

**Hypothesis:** The Win32 libSkiaSharp.dll links against FONTSUB.dll for XPS font subsetting. Windows container images derived from dotnet/aspnet or dotnet/runtime do not include FONTSUB.dll, causing the DLL load failure. The NanoServer package ships a libSkiaSharp compiled without this dependency.

1. **Use SkiaSharp.NativeAssets.NanoServer** — fix, confidence 0.95 (95%), cost/xs, validated=yes
   - Add SkiaSharp.NativeAssets.NanoServer as a PackageReference in the application (executable) project. This provides a libSkiaSharp build that omits the FONTSUB.dll dependency. XPS font subsetting will not be available, but all other SkiaSharp functionality works normally.

```csharp
<PackageReference Include="SkiaSharp.NativeAssets.NanoServer" Version="2.88.3" />
```
2. **Switch to windowsservercore container image** — alternative, confidence 0.70 (70%), cost/s, validated=yes
   - Use mcr.microsoft.com/dotnet/runtime:5.0-windowsservercore-ltsc2019 as the runtime base image. This image includes FONTSUB.dll, allowing the standard Win32 libSkiaSharp to load. Downside: 5.41 GB image, .NET 5.0 only, and an outdated OS version.

**Recommended proposal:** Use SkiaSharp.NativeAssets.NanoServer

**Why:** One-line NuGet change, confirmed by community comment, and is the intended solution per the package's documented purpose. The alternative requires an older base image and pinned runtime version.

## Recommendations

### Actionability

| Field | Value |
|-------|-------|
| Suggested action | needs-info |
| Confidence | 0.80 (80%) |
| Reason | Workaround is well-established (NanoServer package) and confirmed in comments, but the original reporter has never responded to confirm resolution. Asking them to verify the fix before closing. |
| Suggested repro platform | windows |

### Missing Info

- Has the reporter tried adding SkiaSharp.NativeAssets.NanoServer to their application project?
- Confirmation that the NanoServer package resolves the issue on mcr.microsoft.com/dotnet/aspnet:6.0 Windows containers

### Automatable Actions

| Type | Risk | Confidence | Description | Details |
|------|------|------------|-------------|---------|
| update-labels | low | 0.92 (92%) | Apply bug, native assets, Windows Nano Server, reliability labels | labels=type/bug, area/libSkiaSharp.native, os/Windows-Nano-Server, tenet/reliability |
| add-comment | medium | 0.90 (90%) | Post workaround with NanoServer package guidance and ask reporter to confirm | — |

**Comment draft for `add-comment`:**

```markdown
Thanks for the report! The `mcr.microsoft.com/dotnet/aspnet:6.0` Windows container image does not include `FONTSUB.dll`, which is a dependency of the standard `libSkiaSharp.dll` (Win32 build) used for font subsetting in XPS documents.

The fix is to add `SkiaSharp.NativeAssets.NanoServer` to your **application** project (not a library project):

```xml
<PackageReference Include="SkiaSharp.NativeAssets.NanoServer" Version="2.88.3" />
```

This package ships a `libSkiaSharp` build that omits the `FONTSUB.dll` dependency. Note that XPS font subsetting will not be available, but all standard drawing operations work normally.

Could you confirm whether this resolves the issue for you?
```

<details>
<summary>Raw JSON</summary>

```json
{
  "meta": {
    "schemaVersion": "1.0",
    "number": 2440,
    "repo": "mono/SkiaSharp",
    "analyzedAt": "2026-04-25T20:25:00Z"
  },
  "summary": "DllNotFoundException loading libSkiaSharp in Windows Docker containers (mcr.microsoft.com/dotnet/aspnet:6.0) due to missing FONTSUB.dll dependency in the container image; the SkiaSharp.NativeAssets.NanoServer package resolves this by providing a libSkiaSharp build compiled without the FONTSUB.dll dependency.",
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
      "severity": "medium",
      "errorType": "exception",
      "errorMessage": "Unable to load DLL 'libSkiaSharp' or one of its dependencies: The specified module could not be found. (0x8007007E)",
      "reproQuality": "partial",
      "targetFrameworks": [
        "net6.0"
      ]
    },
    "reproEvidence": {
      "environmentDetails": "mcr.microsoft.com/dotnet/aspnet:6.0 Windows container on Azure DevOps, SkiaSharp 2.88.3, Visual Studio",
      "relatedIssues": [
        591,
        676,
        1341
      ],
      "repoLinks": [
        {
          "url": "https://github.com/mono/SkiaSharp/issues/1341",
          "description": "Related Linux DllNotFoundException issue"
        },
        {
          "url": "https://github.com/mono/SkiaSharp/issues/591",
          "description": "Original Windows Docker DllNotFoundException issue (closed/resolved, locked)"
        },
        {
          "url": "https://github.com/mono/SkiaSharp/issues/676",
          "description": "NanoServer support history (closed)"
        }
      ]
    },
    "versionAnalysis": {
      "mentionedVersions": [
        "2.88.3"
      ],
      "currentRelevance": "likely",
      "relevanceReason": "The Win32 NativeAssets still depend on FONTSUB.dll which is absent from modern Windows container base images. The NanoServer package continues to be the recommended solution."
    }
  },
  "analysis": {
    "summary": "libSkiaSharp.dll (Win32 variant) depends on FONTSUB.dll for font subsetting in XPS document creation, but this DLL is absent from modern Windows container images like mcr.microsoft.com/dotnet/aspnet:6.0. The SkiaSharp.NativeAssets.NanoServer package provides a libSkiaSharp compiled without this dependency and is the fix for Windows containers.",
    "keySignals": [
      {
        "text": "Unable to load DLL 'libSkiaSharp' or one of its dependencies: The specified module could not be found. (0x8007007E)",
        "source": "issue body",
        "interpretation": "Windows error 0x8007007E (ERROR_MOD_NOT_FOUND) indicates a dependency of libSkiaSharp.dll is missing from the container, not libSkiaSharp itself."
      },
      {
        "text": "Using mcr.microsoft.com/dotnet/aspnet:6.0 as runtime Docker image",
        "source": "issue body",
        "interpretation": "Standard Windows container image that does not include FONTSUB.dll, a font subsetting library required by the Win32 libSkiaSharp build."
      },
      {
        "text": "Switch to SkiaSharp.NativeAssets.NanoServer... does not include dependencies on things not in NanoServer, like font subsetting",
        "source": "comment by hraftery",
        "interpretation": "Community-confirmed workaround: NanoServer package removes FONTSUB.dll dependency and works in all Windows containers lacking that DLL."
      },
      {
        "text": "fontsub.dll is the last offending dependency, and it relates to a chunk of functionality that may not be missed by many - hence the NanoServer variant was born!",
        "source": "comment by hraftery referencing #591",
        "interpretation": "Historical context: NanoServer package was specifically created to address FONTSUB.dll unavailability in Windows containers."
      }
    ],
    "codeInvestigation": [
      {
        "file": "binding/SkiaSharp.NativeAssets.NanoServer/SkiaSharp.NativeAssets.NanoServer.csproj",
        "finding": "PackageNotes explicitly states: 'This variation of the Windows native assets includes the build of libSkiaSharp.dll that does not make use of the typeface subsetting when creating XPS documents (CreateFontPackage from FONTSUB.dll).' Confirms this package was designed for environments without FONTSUB.dll, including all Windows containers.",
        "relevance": "direct"
      },
      {
        "file": "binding/SkiaSharp.NativeAssets.Win32/SkiaSharp.NativeAssets.Win32.csproj",
        "finding": "Win32 package ships x64, x86, and arm64 binaries with no documented dependency requirements. Absence of FONTSUB.dll documentation here is a contributing factor to user confusion when deploying to Windows containers.",
        "relevance": "related"
      }
    ],
    "workarounds": [
      "Add SkiaSharp.NativeAssets.NanoServer as a PackageReference in the application project instead of relying on auto-included Win32 assets. This build omits FONTSUB.dll dependency (XPS font subsetting not available).",
      "Switch to an older Windows container image such as mcr.microsoft.com/dotnet/runtime:5.0-windowsservercore-ltsc2019 which includes FONTSUB.dll. Disadvantage: large image (5.41 GB), .NET 5.0 only, outdated OS."
    ],
    "nextQuestions": [
      "Has the original reporter confirmed whether SkiaSharp.NativeAssets.NanoServer resolves their issue on mcr.microsoft.com/dotnet/aspnet:6.0?",
      "Should documentation/dev/packages.md be updated to clarify that the NanoServer package is required for all Windows containers (not just Nano Server), since FONTSUB.dll is absent from standard aspnet/runtime base images?"
    ],
    "rationale": "Error 0x8007007E is ERROR_MOD_NOT_FOUND — libSkiaSharp.dll loads but its FONTSUB.dll dependency is absent from the Windows container. The NanoServer package csproj explicitly documents this scenario and is the correct fix. Classified as type/bug because standard SkiaSharp NuGet usage silently fails in a common deployment scenario (Windows containers) with no documentation warning at the point of failure.",
    "resolution": {
      "hypothesis": "The Win32 libSkiaSharp.dll links against FONTSUB.dll for XPS font subsetting. Windows container images derived from dotnet/aspnet or dotnet/runtime do not include FONTSUB.dll, causing the DLL load failure. The NanoServer package ships a libSkiaSharp compiled without this dependency.",
      "proposals": [
        {
          "title": "Use SkiaSharp.NativeAssets.NanoServer",
          "description": "Add SkiaSharp.NativeAssets.NanoServer as a PackageReference in the application (executable) project. This provides a libSkiaSharp build that omits the FONTSUB.dll dependency. XPS font subsetting will not be available, but all other SkiaSharp functionality works normally.",
          "category": "fix",
          "codeSnippet": "<PackageReference Include=\"SkiaSharp.NativeAssets.NanoServer\" Version=\"2.88.3\" />",
          "confidence": 0.95,
          "effort": "cost/xs",
          "validated": "yes"
        },
        {
          "title": "Switch to windowsservercore container image",
          "description": "Use mcr.microsoft.com/dotnet/runtime:5.0-windowsservercore-ltsc2019 as the runtime base image. This image includes FONTSUB.dll, allowing the standard Win32 libSkiaSharp to load. Downside: 5.41 GB image, .NET 5.0 only, and an outdated OS version.",
          "category": "alternative",
          "confidence": 0.7,
          "effort": "cost/s",
          "validated": "yes"
        }
      ],
      "recommendedProposal": "Use SkiaSharp.NativeAssets.NanoServer",
      "recommendedReason": "One-line NuGet change, confirmed by community comment, and is the intended solution per the package's documented purpose. The alternative requires an older base image and pinned runtime version."
    }
  },
  "output": {
    "actionability": {
      "suggestedAction": "needs-info",
      "confidence": 0.8,
      "reason": "Workaround is well-established (NanoServer package) and confirmed in comments, but the original reporter has never responded to confirm resolution. Asking them to verify the fix before closing.",
      "suggestedReproPlatform": "windows"
    },
    "missingInfo": [
      "Has the reporter tried adding SkiaSharp.NativeAssets.NanoServer to their application project?",
      "Confirmation that the NanoServer package resolves the issue on mcr.microsoft.com/dotnet/aspnet:6.0 Windows containers"
    ],
    "actions": [
      {
        "type": "update-labels",
        "description": "Apply bug, native assets, Windows Nano Server, reliability labels",
        "risk": "low",
        "confidence": 0.92,
        "labels": [
          "type/bug",
          "area/libSkiaSharp.native",
          "os/Windows-Nano-Server",
          "tenet/reliability"
        ]
      },
      {
        "type": "add-comment",
        "description": "Post workaround with NanoServer package guidance and ask reporter to confirm",
        "risk": "medium",
        "confidence": 0.9,
        "comment": "Thanks for the report! The `mcr.microsoft.com/dotnet/aspnet:6.0` Windows container image does not include `FONTSUB.dll`, which is a dependency of the standard `libSkiaSharp.dll` (Win32 build) used for font subsetting in XPS documents.\n\nThe fix is to add `SkiaSharp.NativeAssets.NanoServer` to your **application** project (not a library project):\n\n```xml\n<PackageReference Include=\"SkiaSharp.NativeAssets.NanoServer\" Version=\"2.88.3\" />\n```\n\nThis package ships a `libSkiaSharp` build that omits the `FONTSUB.dll` dependency. Note that XPS font subsetting will not be available, but all standard drawing operations work normally.\n\nCould you confirm whether this resolves the issue for you?"
      }
    ]
  }
}
```

</details>
