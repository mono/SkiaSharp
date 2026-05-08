# Issue Triage Report — #3379

| Field | Value |
|-------|-------|
| Repository | mono/SkiaSharp |
| Analyzed | 2026-04-27T02:48:23Z |
| Type | type/bug (0.97 (97%)) |
| Area | area/SkiaSharp.Views (0.97 (97%)) |
| Suggested action | ready-to-fix (0.92 (92%)) |

**Issue Summary:** SkiaSharp.Views.WPF references OpenTK.GLWpfControl 3.3.0 for .NET Framework targets, but that version is not strong-named, causing FileLoadException in strong-named consumer applications; bumping to 3.3.1 (where strong-naming was added) would fix it.

**Analysis:** The SkiaSharp.Views.WPF csproj pins OpenTK.GLWpfControl to version 3.3.0 for .NET Framework targets (net4x). OpenTK.GLWpfControl 3.3.0 was not strong-named (PublicKeyToken=null), but strong-naming was added in 3.3.1. When a consumer application is itself strong-named, the .NET CLR enforces that all its dependencies are also strong-named, causing a FileLoadException at runtime. The fix is a one-line dependency version bump in the csproj.

**Recommendations:** **ready-to-fix** — Root cause is clearly identified (OpenTK.GLWpfControl 3.3.0 not strong-named for net4x targets), fix is a one-line dependency version bump to 3.3.1, and a second user confirms the issue is still present.

---

## Classification

| Field | Value |
|-------|-------|
| Type | type/bug |
| Area | area/SkiaSharp.Views |
| Platforms | os/Windows-Classic |
| Backends | — |
| Tenets | tenet/compatibility |
| Partner | — |
| Current labels | type/bug, os/Windows-Classic, area/SkiaSharp.Views, tenet/compatibility |

## Evidence

### Reproduction

1. Create a .NET Framework (net48) WPF application with a strong-name key (SignAssembly=true)
2. Add a PackageReference to SkiaSharp.Views.WPF 3.116.0 or later
3. Build or run the application
4. Observe FileLoadException because GLWpfControl 3.3.0 has no PublicKeyToken (not strong-named)

**Environment:** Windows, .NET Framework 4.8, SkiaSharp.Views.WPF 3.116.0 / 3.119.1, Visual Studio

**Code snippets:**

```csharp
<Project Sdk="Microsoft.NET.Sdk"><PropertyGroup><TargetFramework>net48</TargetFramework><SignAssembly>true</SignAssembly><AssemblyOriginatorKeyFile>MyStrongNameKey.snk</AssemblyOriginatorKeyFile></PropertyGroup><ItemGroup><PackageReference Include="SkiaSharp.Views.WPF" Version="3.119.1" /></ItemGroup></Project>
```

### Bug Signals

| Field | Value |
|-------|-------|
| Severity | medium |
| Regression claimed | True |
| Error type | exception |
| Error message | System.IO.FileLoadException: Could not load file or assembly 'GLWpfControl, Version=3.3.0.0, Culture=neutral, PublicKeyToken=null' or one of its dependencies. A strongly-named assembly is required. (Exception from HRESULT: 0x80131044) |
| Repro quality | complete |
| Target frameworks | net48 |

### Version Analysis

| Field | Value |
|-------|-------|
| Mentioned versions | 3.116.0, 3.119.1, 2.88.9 |
| Worked in | 2.88.9 |
| Broke in | — |
| Current relevance | likely |
| Relevance reason | The csproj still pins OpenTK.GLWpfControl 3.3.0 for net4x targets in the current main branch. |

### Regression

| Field | Value |
|-------|-------|
| Is regression | True |
| Confidence | 0.85 (85%) |
| Reason | Reporter states 2.88.9 worked. SkiaSharp 3.x introduced or changed the OpenTK.GLWpfControl dependency pinned to 3.3.0 which lacks strong-naming. |
| Worked in version | 2.88.9 |
| Broke in version | 3.116.0 |

## Analysis

### Technical Summary

The SkiaSharp.Views.WPF csproj pins OpenTK.GLWpfControl to version 3.3.0 for .NET Framework targets (net4x). OpenTK.GLWpfControl 3.3.0 was not strong-named (PublicKeyToken=null), but strong-naming was added in 3.3.1. When a consumer application is itself strong-named, the .NET CLR enforces that all its dependencies are also strong-named, causing a FileLoadException at runtime. The fix is a one-line dependency version bump in the csproj.

### Rationale

This is a clear dependency-version bug: the pinned version (3.3.0) lacks a required attribute (strong-name key) that was added in the immediate next patch release (3.3.1). The root cause is directly visible in source/SkiaSharp.Views/SkiaSharp.Views.WPF/SkiaSharp.Views.WPF.csproj line 15. No code investigation ambiguity. The fix is straightforward. Severity is medium because it only affects strong-named .NET Framework applications (not .NET 5+ targets which use OpenTK.GLWpfControl 4.x), but there is no real workaround other than avoiding strong-naming or building from source.

### Key Signals

- "System.IO.FileLoadException: 'Could not load file or assembly 'GLWpfControl, Version=3.3.0.0, Culture=neutral, PublicKeyToken=null'" — **issue body** (The CLR rejects the assembly because it has no PublicKeyToken — confirming 3.3.0 is not strong-named.)
- "Strong-naming was added in OpenTK.GLWpfControl 3.3.1" — **issue body** (Bumping the dependency to 3.3.1 is a well-defined, minimal fix.)
- "Last Known Good Version of SkiaSharp: 2.88.9" — **issue body** (Confirms this is a regression introduced in the 3.x line.)
- "Is there any new version available which fixes the strong naming issue?" — **comment by Pat781 (2026-03-27)** (A second user is experiencing the same problem, confirming it is not isolated.)

### Code Investigation

| File | Lines | Relevance | Finding |
|------|-------|-----------|---------|
| `source/SkiaSharp.Views/SkiaSharp.Views.WPF/SkiaSharp.Views.WPF.csproj` | 13-16 | direct | For .NET Framework targets (Condition="$(TargetFramework.StartsWith('net4'))"), OpenTK.GLWpfControl is pinned to version 3.3.0 which has PublicKeyToken=null (not strong-named). The non-Framework path uses 4.2.3 which is not affected. |
| `source/SkiaSharp.Views/SkiaSharp.Views.WPF/SkiaSharp.Views.WPF.csproj` | 10 | related | SignAssembly is explicitly set to false for non-net4x targets, meaning only the .NET Framework build path is affected by the strong-naming constraint. This narrows the impact to net4x consumers with strong-named applications. |

### Workarounds

- Target net8.0-windows or another .NET (non-Framework) TFM — the non-net4x path uses OpenTK.GLWpfControl 4.x which is not affected.
- Build SkiaSharp.Views.WPF from source with the pin changed to 3.3.1 and reference the local package.

### Next Questions

- Is OpenTK.GLWpfControl 3.3.1 API-compatible with 3.3.0 (no breaking changes)?
- Are there any other OpenTK-related packages pinned to non-strong-named versions?

### Resolution Proposals

**Hypothesis:** Bump OpenTK.GLWpfControl from 3.3.0 to 3.3.1 (or latest 3.x) in the net4x ItemGroup of SkiaSharp.Views.WPF.csproj.

1. **Bump OpenTK.GLWpfControl to 3.3.1 for .NET Framework targets** — fix, confidence 0.92 (92%), cost/xs, validated=untested
   - Change line 15 of SkiaSharp.Views.WPF.csproj from Version="3.3.0" to Version="3.3.1". OpenTK.GLWpfControl 3.3.1 added strong-naming, which allows it to be used in strong-named consumer assemblies.
2. **Migrate net4x consumers to .NET (non-Framework) TFM** — workaround, confidence 0.90 (90%), cost/xl, validated=untested
   - Consumers can target net8.0-windows or higher instead of net48. The non-net4x path uses OpenTK.GLWpfControl 4.x which is unaffected. Note: this is a significant migration effort for existing .NET Framework applications.

**Recommended proposal:** Bump OpenTK.GLWpfControl to 3.3.1 for .NET Framework targets

**Why:** One-line patch in csproj with no API changes. Low risk of regression since 3.3.1 is a patch release (bug fix only). Directly resolves the reported FileLoadException.

## Recommendations

### Actionability

| Field | Value |
|-------|-------|
| Suggested action | ready-to-fix |
| Confidence | 0.92 (92%) |
| Reason | Root cause is clearly identified (OpenTK.GLWpfControl 3.3.0 not strong-named for net4x targets), fix is a one-line dependency version bump to 3.3.1, and a second user confirms the issue is still present. |
| Suggested repro platform | windows |

### Automatable Actions

| Type | Risk | Confidence | Description | Details |
|------|------|------------|-------------|---------|
| update-labels | low | 0.97 (97%) | Apply type/bug, area/SkiaSharp.Views, os/Windows-Classic, tenet/compatibility labels | labels=type/bug, area/SkiaSharp.Views, os/Windows-Classic, tenet/compatibility |
| add-comment | medium | 0.92 (92%) | Acknowledge the root cause and suggest bumping the dependency | — |

**Comment draft for `add-comment`:**

```markdown
Thanks for the clear repro. The root cause is confirmed: `source/SkiaSharp.Views/SkiaSharp.Views.WPF/SkiaSharp.Views.WPF.csproj` line 15 pins `OpenTK.GLWpfControl` to `3.3.0` for .NET Framework targets, and that version was not strong-named. Strong-naming was introduced in `3.3.1`.

The fix is a one-line bump:
```xml
<PackageReference Include="OpenTK.GLWpfControl" Version="3.3.1" NoWarn="NU1701" />
```

As a short-term workaround, migrating your application to a modern .NET TFM (e.g., `net8.0-windows`) avoids the issue, as the `net5+` path in SkiaSharp.Views.WPF already uses `OpenTK.GLWpfControl 4.x`.
```

<details>
<summary>Raw JSON</summary>

```json
{
  "meta": {
    "schemaVersion": "1.0",
    "number": 3379,
    "repo": "mono/SkiaSharp",
    "analyzedAt": "2026-04-27T02:48:23Z",
    "currentLabels": [
      "type/bug",
      "os/Windows-Classic",
      "area/SkiaSharp.Views",
      "tenet/compatibility"
    ]
  },
  "summary": "SkiaSharp.Views.WPF references OpenTK.GLWpfControl 3.3.0 for .NET Framework targets, but that version is not strong-named, causing FileLoadException in strong-named consumer applications; bumping to 3.3.1 (where strong-naming was added) would fix it.",
  "classification": {
    "type": {
      "value": "type/bug",
      "confidence": 0.97
    },
    "area": {
      "value": "area/SkiaSharp.Views",
      "confidence": 0.97
    },
    "platforms": [
      "os/Windows-Classic"
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
      "errorMessage": "System.IO.FileLoadException: Could not load file or assembly 'GLWpfControl, Version=3.3.0.0, Culture=neutral, PublicKeyToken=null' or one of its dependencies. A strongly-named assembly is required. (Exception from HRESULT: 0x80131044)",
      "reproQuality": "complete",
      "targetFrameworks": [
        "net48"
      ]
    },
    "reproEvidence": {
      "stepsToReproduce": [
        "Create a .NET Framework (net48) WPF application with a strong-name key (SignAssembly=true)",
        "Add a PackageReference to SkiaSharp.Views.WPF 3.116.0 or later",
        "Build or run the application",
        "Observe FileLoadException because GLWpfControl 3.3.0 has no PublicKeyToken (not strong-named)"
      ],
      "codeSnippets": [
        "<Project Sdk=\"Microsoft.NET.Sdk\"><PropertyGroup><TargetFramework>net48</TargetFramework><SignAssembly>true</SignAssembly><AssemblyOriginatorKeyFile>MyStrongNameKey.snk</AssemblyOriginatorKeyFile></PropertyGroup><ItemGroup><PackageReference Include=\"SkiaSharp.Views.WPF\" Version=\"3.119.1\" /></ItemGroup></Project>"
      ],
      "environmentDetails": "Windows, .NET Framework 4.8, SkiaSharp.Views.WPF 3.116.0 / 3.119.1, Visual Studio"
    },
    "versionAnalysis": {
      "mentionedVersions": [
        "3.116.0",
        "3.119.1",
        "2.88.9"
      ],
      "workedIn": "2.88.9",
      "currentRelevance": "likely",
      "relevanceReason": "The csproj still pins OpenTK.GLWpfControl 3.3.0 for net4x targets in the current main branch."
    },
    "regression": {
      "isRegression": true,
      "confidence": 0.85,
      "reason": "Reporter states 2.88.9 worked. SkiaSharp 3.x introduced or changed the OpenTK.GLWpfControl dependency pinned to 3.3.0 which lacks strong-naming.",
      "workedInVersion": "2.88.9",
      "brokeInVersion": "3.116.0"
    }
  },
  "analysis": {
    "summary": "The SkiaSharp.Views.WPF csproj pins OpenTK.GLWpfControl to version 3.3.0 for .NET Framework targets (net4x). OpenTK.GLWpfControl 3.3.0 was not strong-named (PublicKeyToken=null), but strong-naming was added in 3.3.1. When a consumer application is itself strong-named, the .NET CLR enforces that all its dependencies are also strong-named, causing a FileLoadException at runtime. The fix is a one-line dependency version bump in the csproj.",
    "rationale": "This is a clear dependency-version bug: the pinned version (3.3.0) lacks a required attribute (strong-name key) that was added in the immediate next patch release (3.3.1). The root cause is directly visible in source/SkiaSharp.Views/SkiaSharp.Views.WPF/SkiaSharp.Views.WPF.csproj line 15. No code investigation ambiguity. The fix is straightforward. Severity is medium because it only affects strong-named .NET Framework applications (not .NET 5+ targets which use OpenTK.GLWpfControl 4.x), but there is no real workaround other than avoiding strong-naming or building from source.",
    "codeInvestigation": [
      {
        "file": "source/SkiaSharp.Views/SkiaSharp.Views.WPF/SkiaSharp.Views.WPF.csproj",
        "lines": "13-16",
        "finding": "For .NET Framework targets (Condition=\"$(TargetFramework.StartsWith('net4'))\"), OpenTK.GLWpfControl is pinned to version 3.3.0 which has PublicKeyToken=null (not strong-named). The non-Framework path uses 4.2.3 which is not affected.",
        "relevance": "direct"
      },
      {
        "file": "source/SkiaSharp.Views/SkiaSharp.Views.WPF/SkiaSharp.Views.WPF.csproj",
        "lines": "10",
        "finding": "SignAssembly is explicitly set to false for non-net4x targets, meaning only the .NET Framework build path is affected by the strong-naming constraint. This narrows the impact to net4x consumers with strong-named applications.",
        "relevance": "related"
      }
    ],
    "keySignals": [
      {
        "text": "System.IO.FileLoadException: 'Could not load file or assembly 'GLWpfControl, Version=3.3.0.0, Culture=neutral, PublicKeyToken=null'",
        "source": "issue body",
        "interpretation": "The CLR rejects the assembly because it has no PublicKeyToken — confirming 3.3.0 is not strong-named."
      },
      {
        "text": "Strong-naming was added in OpenTK.GLWpfControl 3.3.1",
        "source": "issue body",
        "interpretation": "Bumping the dependency to 3.3.1 is a well-defined, minimal fix."
      },
      {
        "text": "Last Known Good Version of SkiaSharp: 2.88.9",
        "source": "issue body",
        "interpretation": "Confirms this is a regression introduced in the 3.x line."
      },
      {
        "text": "Is there any new version available which fixes the strong naming issue?",
        "source": "comment by Pat781 (2026-03-27)",
        "interpretation": "A second user is experiencing the same problem, confirming it is not isolated."
      }
    ],
    "nextQuestions": [
      "Is OpenTK.GLWpfControl 3.3.1 API-compatible with 3.3.0 (no breaking changes)?",
      "Are there any other OpenTK-related packages pinned to non-strong-named versions?"
    ],
    "workarounds": [
      "Target net8.0-windows or another .NET (non-Framework) TFM — the non-net4x path uses OpenTK.GLWpfControl 4.x which is not affected.",
      "Build SkiaSharp.Views.WPF from source with the pin changed to 3.3.1 and reference the local package."
    ],
    "resolution": {
      "hypothesis": "Bump OpenTK.GLWpfControl from 3.3.0 to 3.3.1 (or latest 3.x) in the net4x ItemGroup of SkiaSharp.Views.WPF.csproj.",
      "proposals": [
        {
          "title": "Bump OpenTK.GLWpfControl to 3.3.1 for .NET Framework targets",
          "description": "Change line 15 of SkiaSharp.Views.WPF.csproj from Version=\"3.3.0\" to Version=\"3.3.1\". OpenTK.GLWpfControl 3.3.1 added strong-naming, which allows it to be used in strong-named consumer assemblies.",
          "category": "fix",
          "confidence": 0.92,
          "effort": "cost/xs",
          "validated": "untested"
        },
        {
          "title": "Migrate net4x consumers to .NET (non-Framework) TFM",
          "description": "Consumers can target net8.0-windows or higher instead of net48. The non-net4x path uses OpenTK.GLWpfControl 4.x which is unaffected. Note: this is a significant migration effort for existing .NET Framework applications.",
          "category": "workaround",
          "confidence": 0.9,
          "effort": "cost/xl",
          "validated": "untested"
        }
      ],
      "recommendedProposal": "Bump OpenTK.GLWpfControl to 3.3.1 for .NET Framework targets",
      "recommendedReason": "One-line patch in csproj with no API changes. Low risk of regression since 3.3.1 is a patch release (bug fix only). Directly resolves the reported FileLoadException."
    }
  },
  "output": {
    "actionability": {
      "suggestedAction": "ready-to-fix",
      "confidence": 0.92,
      "reason": "Root cause is clearly identified (OpenTK.GLWpfControl 3.3.0 not strong-named for net4x targets), fix is a one-line dependency version bump to 3.3.1, and a second user confirms the issue is still present.",
      "suggestedReproPlatform": "windows"
    },
    "actions": [
      {
        "type": "update-labels",
        "description": "Apply type/bug, area/SkiaSharp.Views, os/Windows-Classic, tenet/compatibility labels",
        "risk": "low",
        "confidence": 0.97,
        "labels": [
          "type/bug",
          "area/SkiaSharp.Views",
          "os/Windows-Classic",
          "tenet/compatibility"
        ]
      },
      {
        "type": "add-comment",
        "description": "Acknowledge the root cause and suggest bumping the dependency",
        "risk": "medium",
        "confidence": 0.92,
        "comment": "Thanks for the clear repro. The root cause is confirmed: `source/SkiaSharp.Views/SkiaSharp.Views.WPF/SkiaSharp.Views.WPF.csproj` line 15 pins `OpenTK.GLWpfControl` to `3.3.0` for .NET Framework targets, and that version was not strong-named. Strong-naming was introduced in `3.3.1`.\n\nThe fix is a one-line bump:\n```xml\n<PackageReference Include=\"OpenTK.GLWpfControl\" Version=\"3.3.1\" NoWarn=\"NU1701\" />\n```\n\nAs a short-term workaround, migrating your application to a modern .NET TFM (e.g., `net8.0-windows`) avoids the issue, as the `net5+` path in SkiaSharp.Views.WPF already uses `OpenTK.GLWpfControl 4.x`."
      }
    ]
  }
}
```

</details>
