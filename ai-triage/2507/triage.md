# Issue Triage Report — #2507

| Field | Value |
|-------|-------|
| Repository | mono/SkiaSharp |
| Analyzed | 2026-05-04T22:12:48Z |
| Type | type/feature-request (0.90 (90%)) |
| Area | area/Build (0.85 (85%)) |
| Suggested action | keep-open (0.80 (80%)) |

**Issue Summary:** Request to add PE file version resources (VERSIONINFO) to libSkiaSharp.dll and libHarfBuzzSharp.dll on Windows for serviceability automation.

**Analysis:** The Windows native DLLs (libSkiaSharp.dll and libHarfBuzzSharp.dll) do not embed PE VERSIONINFO resources, which breaks serviceability tooling that validates binary metadata. No .rc resource files exist in native/windows/ for either DLL, and the vcxproj projects do not reference any ResourceCompile items. Adding VERSIONINFO requires adding .rc files with VS_VERSION_INFO to the native/windows build projects.

**Recommendations:** **keep-open** — Valid serviceability request with a clear implementation path. The native Windows DLLs currently lack VERSIONINFO PE resources. Keeping open for a contributor or maintainer to implement.

---

## Classification

| Field | Value |
|-------|-------|
| Type | type/feature-request |
| Area | area/Build |
| Platforms | os/Windows-Classic |
| Backends | — |
| Tenets | tenet/compatibility |
| Partner | — |

## Evidence

### Reproduction

1. Obtain libSkiaSharp.dll or libHarfBuzzSharp.dll from SkiaSharp NuGet package (Windows)
2. Inspect file version resources using tools like 'filever' or PowerShell Get-Item with VersionInfo property
3. Observe that FileVersion, ProductVersion and other VERSIONINFO fields are empty/absent

**Environment:** Windows DLLs libSkiaSharp.dll and libHarfBuzzSharp.dll shipped via LiveCharts (SkiaSharp dependency); automation tooling verifying FileVersion PE metadata fails.

## Analysis

### Technical Summary

The Windows native DLLs (libSkiaSharp.dll and libHarfBuzzSharp.dll) do not embed PE VERSIONINFO resources, which breaks serviceability tooling that validates binary metadata. No .rc resource files exist in native/windows/ for either DLL, and the vcxproj projects do not reference any ResourceCompile items. Adding VERSIONINFO requires adding .rc files with VS_VERSION_INFO to the native/windows build projects.

### Rationale

This is a feature request — the binaries currently have no version resources and the reporter is asking for them to be added. The native/windows/libHarfBuzzSharp/ directory contains only the .sln and .vcxproj files with no .rc resource files. The Windows build cake script also has no logic for embedding version info. This is a build-time change scoped to Windows DLL generation, hence area/Build. The request is valid and reasonable for enterprise serviceability scenarios.

### Key Signals

- "these two binaries don't have it [file version resources] — libHarfBuzzSharp.dll, libSkiaSharp.dll" — **issue body** (Confirms the VERSIONINFO PE resource is absent from both Windows DLLs.)
- "Is it possible to have file version resources added for all binaries for serviceability purposes?" — **issue body** (Clear feature request for build-time addition of VS_VERSION_INFO to Windows native DLLs.)

### Code Investigation

| File | Lines | Relevance | Finding |
|------|-------|-----------|---------|
| `native/windows/libHarfBuzzSharp/libHarfBuzzSharp.vcxproj` | — | direct | No ResourceCompile item group or .rc file reference. The project builds a DLL with no version resources embedded. |
| `native/windows/build.cake` | — | direct | Builds libSkiaSharp and libHarfBuzzSharp via MSBuild vcxproj. No step to generate or inject version resources during the build process. |

### Resolution Proposals

**Hypothesis:** Add .rc files with VS_VERSION_INFO to native/windows build projects for both libSkiaSharp and libHarfBuzzSharp, and reference them in the respective .vcxproj files.

1. **Add VERSIONINFO .rc files to Windows native projects** — fix, confidence 0.80 (80%), cost/m, validated=untested
   - Create version.rc files containing VS_VERSION_INFO blocks for both libSkiaSharp and libHarfBuzzSharp Windows vcxproj projects. The build.cake can inject version values at build time from the SkiaSharp version string.

**Recommended proposal:** Add VERSIONINFO .rc files to Windows native projects

**Why:** Standard Windows DLL practice; well-understood implementation path using existing MSBuild ResourceCompile infrastructure.

## Recommendations

### Actionability

| Field | Value |
|-------|-------|
| Suggested action | keep-open |
| Confidence | 0.80 (80%) |
| Reason | Valid serviceability request with a clear implementation path. The native Windows DLLs currently lack VERSIONINFO PE resources. Keeping open for a contributor or maintainer to implement. |
| Suggested repro platform | windows |

### Automatable Actions

| Type | Risk | Confidence | Description | Details |
|------|------|------------|-------------|---------|
| update-labels | low | 0.90 (90%) | Apply feature-request, Build, Windows-Classic, and compatibility labels | labels=type/feature-request, area/Build, os/Windows-Classic, tenet/compatibility |
| add-comment | medium | 0.80 (80%) | Acknowledge request and explain implementation path | — |

**Comment draft for `add-comment`:**

```markdown
Thanks for the request! You're right that the Windows native DLLs currently don't include PE VERSIONINFO resources.

Implementing this would involve:
1. Adding `.rc` files with `VS_VERSION_INFO` blocks to the `native/windows/` build projects for both `libSkiaSharp` and `libHarfBuzzSharp`
2. Referencing them as `ResourceCompile` items in the `.vcxproj` files
3. Injecting the SkiaSharp version at build time via the `build.cake` script

We're keeping this open for a contributor to implement. PRs are welcome!
```

<details>
<summary>Raw JSON</summary>

```json
{
  "meta": {
    "schemaVersion": "1.0",
    "number": 2507,
    "repo": "mono/SkiaSharp",
    "analyzedAt": "2026-05-04T22:12:48Z"
  },
  "summary": "Request to add PE file version resources (VERSIONINFO) to libSkiaSharp.dll and libHarfBuzzSharp.dll on Windows for serviceability automation.",
  "classification": {
    "type": {
      "value": "type/feature-request",
      "confidence": 0.9
    },
    "area": {
      "value": "area/Build",
      "confidence": 0.85
    },
    "platforms": [
      "os/Windows-Classic"
    ],
    "tenets": [
      "tenet/compatibility"
    ]
  },
  "evidence": {
    "reproEvidence": {
      "environmentDetails": "Windows DLLs libSkiaSharp.dll and libHarfBuzzSharp.dll shipped via LiveCharts (SkiaSharp dependency); automation tooling verifying FileVersion PE metadata fails.",
      "stepsToReproduce": [
        "Obtain libSkiaSharp.dll or libHarfBuzzSharp.dll from SkiaSharp NuGet package (Windows)",
        "Inspect file version resources using tools like 'filever' or PowerShell Get-Item with VersionInfo property",
        "Observe that FileVersion, ProductVersion and other VERSIONINFO fields are empty/absent"
      ]
    }
  },
  "analysis": {
    "summary": "The Windows native DLLs (libSkiaSharp.dll and libHarfBuzzSharp.dll) do not embed PE VERSIONINFO resources, which breaks serviceability tooling that validates binary metadata. No .rc resource files exist in native/windows/ for either DLL, and the vcxproj projects do not reference any ResourceCompile items. Adding VERSIONINFO requires adding .rc files with VS_VERSION_INFO to the native/windows build projects.",
    "rationale": "This is a feature request — the binaries currently have no version resources and the reporter is asking for them to be added. The native/windows/libHarfBuzzSharp/ directory contains only the .sln and .vcxproj files with no .rc resource files. The Windows build cake script also has no logic for embedding version info. This is a build-time change scoped to Windows DLL generation, hence area/Build. The request is valid and reasonable for enterprise serviceability scenarios.",
    "codeInvestigation": [
      {
        "file": "native/windows/libHarfBuzzSharp/libHarfBuzzSharp.vcxproj",
        "finding": "No ResourceCompile item group or .rc file reference. The project builds a DLL with no version resources embedded.",
        "relevance": "direct"
      },
      {
        "file": "native/windows/build.cake",
        "finding": "Builds libSkiaSharp and libHarfBuzzSharp via MSBuild vcxproj. No step to generate or inject version resources during the build process.",
        "relevance": "direct"
      }
    ],
    "keySignals": [
      {
        "text": "these two binaries don't have it [file version resources] — libHarfBuzzSharp.dll, libSkiaSharp.dll",
        "source": "issue body",
        "interpretation": "Confirms the VERSIONINFO PE resource is absent from both Windows DLLs."
      },
      {
        "text": "Is it possible to have file version resources added for all binaries for serviceability purposes?",
        "source": "issue body",
        "interpretation": "Clear feature request for build-time addition of VS_VERSION_INFO to Windows native DLLs."
      }
    ],
    "resolution": {
      "hypothesis": "Add .rc files with VS_VERSION_INFO to native/windows build projects for both libSkiaSharp and libHarfBuzzSharp, and reference them in the respective .vcxproj files.",
      "proposals": [
        {
          "title": "Add VERSIONINFO .rc files to Windows native projects",
          "description": "Create version.rc files containing VS_VERSION_INFO blocks for both libSkiaSharp and libHarfBuzzSharp Windows vcxproj projects. The build.cake can inject version values at build time from the SkiaSharp version string.",
          "category": "fix",
          "confidence": 0.8,
          "effort": "cost/m",
          "validated": "untested"
        }
      ],
      "recommendedProposal": "Add VERSIONINFO .rc files to Windows native projects",
      "recommendedReason": "Standard Windows DLL practice; well-understood implementation path using existing MSBuild ResourceCompile infrastructure."
    }
  },
  "output": {
    "actionability": {
      "suggestedAction": "keep-open",
      "confidence": 0.8,
      "reason": "Valid serviceability request with a clear implementation path. The native Windows DLLs currently lack VERSIONINFO PE resources. Keeping open for a contributor or maintainer to implement.",
      "suggestedReproPlatform": "windows"
    },
    "actions": [
      {
        "type": "update-labels",
        "description": "Apply feature-request, Build, Windows-Classic, and compatibility labels",
        "risk": "low",
        "confidence": 0.9,
        "labels": [
          "type/feature-request",
          "area/Build",
          "os/Windows-Classic",
          "tenet/compatibility"
        ]
      },
      {
        "type": "add-comment",
        "description": "Acknowledge request and explain implementation path",
        "risk": "medium",
        "confidence": 0.8,
        "comment": "Thanks for the request! You're right that the Windows native DLLs currently don't include PE VERSIONINFO resources.\n\nImplementing this would involve:\n1. Adding `.rc` files with `VS_VERSION_INFO` blocks to the `native/windows/` build projects for both `libSkiaSharp` and `libHarfBuzzSharp`\n2. Referencing them as `ResourceCompile` items in the `.vcxproj` files\n3. Injecting the SkiaSharp version at build time via the `build.cake` script\n\nWe're keeping this open for a contributor to implement. PRs are welcome!"
      }
    ]
  }
}
```

</details>
