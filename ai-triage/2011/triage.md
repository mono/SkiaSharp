# Issue Triage Report — #2011

| Field | Value |
|-------|-------|
| Repository | mono/SkiaSharp |
| Analyzed | 2026-05-02T23:45:57Z |
| Type | type/enhancement (0.85 (85%)) |
| Area | area/SkiaSharp.Views (0.90 (90%)) |
| Suggested action | close-as-fixed (0.65 (65%)) |

**Issue Summary:** Reporter requests that the System.Drawing.Common dependency introduced in SkiaSharp.Views.Desktop.Common 2.80.3 (via new extension methods) be decoupled into a separate optional library to avoid cross-platform compatibility issues.

**Analysis:** The reporter found that SkiaSharp.Views.Desktop.Common 2.80.3 added a dependency on System.Drawing.Common via new extension methods (SKColor/SKPoint/SKRect/Bitmap conversions). System.Drawing.Common is problematic on non-Windows platforms. Investigation shows that in the current codebase, these extensions (Extensions.Desktop.cs) reside in SkiaSharp.Views.WindowsForms, NOT in SkiaSharp.Views.Desktop.Common — the concern appears to have been addressed by moving the extensions to the Windows Forms-specific project.

**Recommendations:** **close-as-fixed** — Code investigation shows the System.Drawing extensions are now in SkiaSharp.Views.WindowsForms (appropriate) and not in Desktop.Common. The original concern appears addressed.

---

## Classification

| Field | Value |
|-------|-------|
| Type | type/enhancement |
| Area | area/SkiaSharp.Views |
| Platforms | os/Linux, os/Windows-Classic |
| Backends | — |
| Tenets | tenet/compatibility |
| Partner | — |

## Evidence

### Reproduction

**Environment:** SkiaSharp.Views.Desktop.Common 2.80.3+

**Repository links:**
- https://github.com/mono/SkiaSharp/blob/main/source/SkiaSharp.Views/SkiaSharp.Views.Shared/Extensions.cs — Reporter-referenced file where the extension methods were originally located

### Version Analysis

| Field | Value |
|-------|-------|
| Mentioned versions | 2.80.2, 2.80.3 |
| Worked in | 2.80.2 |
| Broke in | 2.80.3 |
| Current relevance | unlikely |
| Relevance reason | Current code has System.Drawing extension methods in SkiaSharp.Views.WindowsForms/Extensions.Desktop.cs (expected for WinForms), NOT in SkiaSharp.Views.Desktop.Common — the dependency appears to have been moved to the appropriate platform-specific project. |

## Analysis

### Technical Summary

The reporter found that SkiaSharp.Views.Desktop.Common 2.80.3 added a dependency on System.Drawing.Common via new extension methods (SKColor/SKPoint/SKRect/Bitmap conversions). System.Drawing.Common is problematic on non-Windows platforms. Investigation shows that in the current codebase, these extensions (Extensions.Desktop.cs) reside in SkiaSharp.Views.WindowsForms, NOT in SkiaSharp.Views.Desktop.Common — the concern appears to have been addressed by moving the extensions to the Windows Forms-specific project.

### Rationale

The issue is an enhancement request for package separation. The current Desktop.Common.csproj only includes SkiaSharp.Views.Shared/**/*.cs which contains no System.Drawing references. The System.Drawing extension methods are now in SkiaSharp.Views.WindowsForms where they are expected. This suggests the dependency issue was resolved in a later version.

### Key Signals

- "from SkiaSharp.Views.Desktop.Common 2.80.3+ there is now a dependancy on system.drawing.common" — **issue body** (Reporter observed the dependency added in 2.80.3 via extension methods in the shared library.)
- "it is the addition of the extension methods that are causing the reliance on the new dependancy" — **issue body** (Extension methods bridging System.Drawing types were added to the shared package.)
- "is there any plan to change this in the future, or create a seperate extension library?" — **issue body** (Enhancement request for package separation, not a crash/exception bug.)

### Code Investigation

| File | Lines | Relevance | Finding |
|------|-------|-----------|---------|
| `source/SkiaSharp.Views/SkiaSharp.Views.Desktop.Common/SkiaSharp.Views.Desktop.Common.csproj` | — | direct | Only includes SkiaSharp.Views.Shared/**/*.cs — no direct or transitive System.Drawing.Common reference. No PackageReference for System.Drawing.Common. |
| `source/SkiaSharp.Views/SkiaSharp.Views.WindowsForms/Extensions.Desktop.cs` | 1-185 | direct | Contains all System.Drawing conversion extensions (SKPoint/SKRect/SKSize/Bitmap/SKColor) referencing System.Drawing types. This file is in the WindowsForms project, NOT in Desktop.Common — appropriate since WinForms requires System.Drawing. |
| `source/SkiaSharp.Views/SkiaSharp.Views.Shared/Extensions.cs` | 1-27 | direct | Contains only EnvironmentExtensions (internal class, DllNotFoundException check) — no System.Drawing references whatsoever. |

### Resolution Proposals

**Hypothesis:** The System.Drawing extension methods (introduced in 2.80.3 in Desktop.Common) have since been moved to SkiaSharp.Views.WindowsForms where they belong — since WinForms already requires System.Drawing. Desktop.Common no longer has this dependency.

1. **Close as fixed** — fix, confidence 0.65 (65%), cost/xs, validated=untested
   - Verify current NuGet package for SkiaSharp.Views.Desktop.Common no longer depends on System.Drawing.Common, then close the issue with a note that the refactoring was completed.
2. **Keep open for separate extension package** — alternative, confidence 0.50 (50%), cost/m, validated=untested
   - If users still need System.Drawing interop outside of WinForms, consider creating a SkiaSharp.Views.Desktop.Drawing extension NuGet package.

**Recommended proposal:** Close as fixed

**Why:** Current code shows Extensions.Desktop.cs with all System.Drawing types is only in SkiaSharp.Views.WindowsForms, not Desktop.Common. The architectural concern appears resolved.

## Recommendations

### Actionability

| Field | Value |
|-------|-------|
| Suggested action | close-as-fixed |
| Confidence | 0.65 (65%) |
| Reason | Code investigation shows the System.Drawing extensions are now in SkiaSharp.Views.WindowsForms (appropriate) and not in Desktop.Common. The original concern appears addressed. |
| Suggested repro platform | linux |

### Automatable Actions

| Type | Risk | Confidence | Description | Details |
|------|------|------------|-------------|---------|
| update-labels | low | 0.90 (90%) | Apply enhancement, views, compatibility labels | labels=type/enhancement, area/SkiaSharp.Views, os/Linux, os/Windows-Classic, tenet/compatibility |
| add-comment | high | 0.65 (65%) | Explain that the System.Drawing extensions appear to have been moved to WinForms project and confirm if fixed | — |

**Comment draft for `add-comment`:**

```markdown
Thanks for reporting this! After investigating the current codebase, the `System.Drawing` conversion extension methods (for `SKColor`, `SKPoint`, `SKRect`, `Bitmap`, etc.) now reside in `SkiaSharp.Views.WindowsForms` rather than `SkiaSharp.Views.Desktop.Common`. This means `Desktop.Common` no longer has a direct dependency on `System.Drawing.Common`.

Could you confirm whether this issue still occurs in a recent version of the package? If the latest NuGet release of `SkiaSharp.Views.Desktop.Common` no longer pulls in `System.Drawing.Common`, we can close this as resolved.
```

<details>
<summary>Raw JSON</summary>

```json
{
  "meta": {
    "schemaVersion": "1.0",
    "number": 2011,
    "repo": "mono/SkiaSharp",
    "analyzedAt": "2026-05-02T23:45:57Z"
  },
  "summary": "Reporter requests that the System.Drawing.Common dependency introduced in SkiaSharp.Views.Desktop.Common 2.80.3 (via new extension methods) be decoupled into a separate optional library to avoid cross-platform compatibility issues.",
  "classification": {
    "type": {
      "value": "type/enhancement",
      "confidence": 0.85
    },
    "area": {
      "value": "area/SkiaSharp.Views",
      "confidence": 0.9
    },
    "platforms": [
      "os/Linux",
      "os/Windows-Classic"
    ],
    "tenets": [
      "tenet/compatibility"
    ]
  },
  "evidence": {
    "reproEvidence": {
      "environmentDetails": "SkiaSharp.Views.Desktop.Common 2.80.3+",
      "repoLinks": [
        {
          "url": "https://github.com/mono/SkiaSharp/blob/main/source/SkiaSharp.Views/SkiaSharp.Views.Shared/Extensions.cs",
          "description": "Reporter-referenced file where the extension methods were originally located"
        }
      ]
    },
    "versionAnalysis": {
      "mentionedVersions": [
        "2.80.2",
        "2.80.3"
      ],
      "workedIn": "2.80.2",
      "brokeIn": "2.80.3",
      "currentRelevance": "unlikely",
      "relevanceReason": "Current code has System.Drawing extension methods in SkiaSharp.Views.WindowsForms/Extensions.Desktop.cs (expected for WinForms), NOT in SkiaSharp.Views.Desktop.Common — the dependency appears to have been moved to the appropriate platform-specific project."
    }
  },
  "analysis": {
    "summary": "The reporter found that SkiaSharp.Views.Desktop.Common 2.80.3 added a dependency on System.Drawing.Common via new extension methods (SKColor/SKPoint/SKRect/Bitmap conversions). System.Drawing.Common is problematic on non-Windows platforms. Investigation shows that in the current codebase, these extensions (Extensions.Desktop.cs) reside in SkiaSharp.Views.WindowsForms, NOT in SkiaSharp.Views.Desktop.Common — the concern appears to have been addressed by moving the extensions to the Windows Forms-specific project.",
    "rationale": "The issue is an enhancement request for package separation. The current Desktop.Common.csproj only includes SkiaSharp.Views.Shared/**/*.cs which contains no System.Drawing references. The System.Drawing extension methods are now in SkiaSharp.Views.WindowsForms where they are expected. This suggests the dependency issue was resolved in a later version.",
    "codeInvestigation": [
      {
        "file": "source/SkiaSharp.Views/SkiaSharp.Views.Desktop.Common/SkiaSharp.Views.Desktop.Common.csproj",
        "finding": "Only includes SkiaSharp.Views.Shared/**/*.cs — no direct or transitive System.Drawing.Common reference. No PackageReference for System.Drawing.Common.",
        "relevance": "direct"
      },
      {
        "file": "source/SkiaSharp.Views/SkiaSharp.Views.WindowsForms/Extensions.Desktop.cs",
        "lines": "1-185",
        "finding": "Contains all System.Drawing conversion extensions (SKPoint/SKRect/SKSize/Bitmap/SKColor) referencing System.Drawing types. This file is in the WindowsForms project, NOT in Desktop.Common — appropriate since WinForms requires System.Drawing.",
        "relevance": "direct"
      },
      {
        "file": "source/SkiaSharp.Views/SkiaSharp.Views.Shared/Extensions.cs",
        "lines": "1-27",
        "finding": "Contains only EnvironmentExtensions (internal class, DllNotFoundException check) — no System.Drawing references whatsoever.",
        "relevance": "direct"
      }
    ],
    "keySignals": [
      {
        "text": "from SkiaSharp.Views.Desktop.Common 2.80.3+ there is now a dependancy on system.drawing.common",
        "source": "issue body",
        "interpretation": "Reporter observed the dependency added in 2.80.3 via extension methods in the shared library."
      },
      {
        "text": "it is the addition of the extension methods that are causing the reliance on the new dependancy",
        "source": "issue body",
        "interpretation": "Extension methods bridging System.Drawing types were added to the shared package."
      },
      {
        "text": "is there any plan to change this in the future, or create a seperate extension library?",
        "source": "issue body",
        "interpretation": "Enhancement request for package separation, not a crash/exception bug."
      }
    ],
    "resolution": {
      "hypothesis": "The System.Drawing extension methods (introduced in 2.80.3 in Desktop.Common) have since been moved to SkiaSharp.Views.WindowsForms where they belong — since WinForms already requires System.Drawing. Desktop.Common no longer has this dependency.",
      "proposals": [
        {
          "title": "Close as fixed",
          "description": "Verify current NuGet package for SkiaSharp.Views.Desktop.Common no longer depends on System.Drawing.Common, then close the issue with a note that the refactoring was completed.",
          "category": "fix",
          "confidence": 0.65,
          "effort": "cost/xs",
          "validated": "untested"
        },
        {
          "title": "Keep open for separate extension package",
          "description": "If users still need System.Drawing interop outside of WinForms, consider creating a SkiaSharp.Views.Desktop.Drawing extension NuGet package.",
          "category": "alternative",
          "confidence": 0.5,
          "effort": "cost/m",
          "validated": "untested"
        }
      ],
      "recommendedProposal": "Close as fixed",
      "recommendedReason": "Current code shows Extensions.Desktop.cs with all System.Drawing types is only in SkiaSharp.Views.WindowsForms, not Desktop.Common. The architectural concern appears resolved."
    }
  },
  "output": {
    "actionability": {
      "suggestedAction": "close-as-fixed",
      "confidence": 0.65,
      "reason": "Code investigation shows the System.Drawing extensions are now in SkiaSharp.Views.WindowsForms (appropriate) and not in Desktop.Common. The original concern appears addressed.",
      "suggestedReproPlatform": "linux"
    },
    "actions": [
      {
        "type": "update-labels",
        "description": "Apply enhancement, views, compatibility labels",
        "risk": "low",
        "confidence": 0.9,
        "labels": [
          "type/enhancement",
          "area/SkiaSharp.Views",
          "os/Linux",
          "os/Windows-Classic",
          "tenet/compatibility"
        ]
      },
      {
        "type": "add-comment",
        "description": "Explain that the System.Drawing extensions appear to have been moved to WinForms project and confirm if fixed",
        "risk": "high",
        "confidence": 0.65,
        "comment": "Thanks for reporting this! After investigating the current codebase, the `System.Drawing` conversion extension methods (for `SKColor`, `SKPoint`, `SKRect`, `Bitmap`, etc.) now reside in `SkiaSharp.Views.WindowsForms` rather than `SkiaSharp.Views.Desktop.Common`. This means `Desktop.Common` no longer has a direct dependency on `System.Drawing.Common`.\n\nCould you confirm whether this issue still occurs in a recent version of the package? If the latest NuGet release of `SkiaSharp.Views.Desktop.Common` no longer pulls in `System.Drawing.Common`, we can close this as resolved."
      }
    ]
  }
}
```

</details>
