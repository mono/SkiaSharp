# Issue Triage Report — #1898

| Field | Value |
|-------|-------|
| Repository | mono/SkiaSharp |
| Analyzed | 2026-05-02T18:53:56Z |
| Type | type/bug (0.85 (85%)) |
| Area | area/SkiaSharp.Views.Forms (0.95 (95%)) |
| Suggested action | needs-investigation (0.72 (72%)) |

**Issue Summary:** SkiaSharp.Views.Forms 2.88.0-preview.179 has an empty netcoreapp3.1 directory in the NuGet package, despite netcore 3.1 support reportedly being added in preview.127.

**Analysis:** The SkiaSharp.Views.Forms NuGet package had netcoreapp3.1 support added in preview.127 but the netcoreapp3.1 directory became empty by preview.179, possibly due to a build pipeline change. The package is now obsolete in the current codebase (superseded by SkiaSharp.Views.Maui), and the build script explicitly skips netcoreapp TFMs during package processing. The stable 2.88.x series was released.

**Recommendations:** **needs-investigation** — The bug was reported against a preview version. The package is now obsolete in the current codebase. Need to verify whether stable 2.88.0 fixed the issue before closing.

---

## Classification

| Field | Value |
|-------|-------|
| Type | type/bug |
| Area | area/SkiaSharp.Views.Forms |
| Platforms | — |
| Backends | — |
| Tenets | tenet/compatibility |
| Partner | — |

## Evidence

### Reproduction

### Bug Signals

| Field | Value |
|-------|-------|
| Severity | medium |
| Regression claimed | True |
| Error type | missing-output |
| Error message | — |
| Repro quality | partial |
| Target frameworks | netcoreapp3.1 |

### Version Analysis

| Field | Value |
|-------|-------|
| Mentioned versions | 2.88.0-preview.120, 2.88.0-preview.127, 2.88.0-preview.179 |
| Worked in | 2.88.0-preview.127 |
| Broke in | 2.88.0-preview.179 |
| Current relevance | unlikely |
| Relevance reason | SkiaSharp.Views.Forms is now in the OBSOLETED_NUGETS list in build.cake; the stable 2.88.x series was released and Xamarin.Forms users should migrate to SkiaSharp.Views.Maui |

### Regression

| Field | Value |
|-------|-------|
| Is regression | True |
| Confidence | 0.80 (80%) |
| Reason | Reporter states netcoreapp3.1 was present in preview.127 but is missing/empty in preview.179 |
| Worked in version | 2.88.0-preview.127 |
| Broke in version | 2.88.0-preview.179 |

## Analysis

### Technical Summary

The SkiaSharp.Views.Forms NuGet package had netcoreapp3.1 support added in preview.127 but the netcoreapp3.1 directory became empty by preview.179, possibly due to a build pipeline change. The package is now obsolete in the current codebase (superseded by SkiaSharp.Views.Maui), and the build script explicitly skips netcoreapp TFMs during package processing. The stable 2.88.x series was released.

### Rationale

Classified as type/bug because netcoreapp3.1 DLLs were confirmed present in an earlier preview and are missing in a later one, indicating a regression. Area is SkiaSharp.Views.Forms since the affected package is specifically the Xamarin.Forms view layer. Severity is medium because it affects a specific target framework but the package was in preview status. The package is now obsoleted in the codebase in favor of MAUI.

### Key Signals

- "netcoreapp3.1 directory is empty in Nuget Package Explorer" — **issue body** (The TFM directory exists but has no DLLs — confirming the package was built without netcore3.1 assets)
- "Support for netcore 3.1 was added in 2.88.0-preview.127 version" — **issue body - referencing release notes** (This was a regression from a known working state)

### Code Investigation

| File | Lines | Relevance | Finding |
|------|-------|-----------|---------|
| `build.cake` | — | direct | SkiaSharp.Views.Forms is listed in OBSOLETED_NUGETS alongside SkiaSharp.Views.Maui.Controls.Compatibility, indicating it is no longer actively built or maintained in the current codebase. |
| `scripts/cake/UtilsManaged.cake` | — | direct | netcoreapp TFMs are explicitly skipped in the package processing logic with comment '// skip this one for now', which could explain why the netcoreapp3.1 directory was empty. |

### Resolution Proposals

**Hypothesis:** The netcoreapp3.1 TFM was dropped from the build for SkiaSharp.Views.Forms between preview.127 and preview.179, possibly intentionally or due to a build script change. The stable 2.88.x series was later released.

1. **Verify if stable 2.88.0 release restored netcoreapp3.1 support** — investigation, cost/xs, validated=untested
   - Check the stable SkiaSharp.Views.Forms 2.88.0 NuGet package on nuget.org to confirm whether the netcoreapp3.1 directory was restored in the final release.
2. **Migrate to SkiaSharp.Views.Maui for .NET 6+ / MAUI projects** — workaround, cost/m, validated=untested
   - SkiaSharp.Views.Forms is obsolete. Users should migrate to SkiaSharp.Views.Maui.Controls for MAUI apps. For Xamarin.Forms projects still on netcoreapp3.1/WPF, use SkiaSharp.Views desktop packages directly.

**Recommended proposal:** investigate-stable

**Why:** Before directing users to migrate, confirm whether the stable release fixed this issue. If fixed, close as fixed; if still broken for stable, the workaround is migration to MAUI.

## Recommendations

### Actionability

| Field | Value |
|-------|-------|
| Suggested action | needs-investigation |
| Confidence | 0.72 (72%) |
| Reason | The bug was reported against a preview version. The package is now obsolete in the current codebase. Need to verify whether stable 2.88.0 fixed the issue before closing. |
| Suggested repro platform | linux |

### Missing Info

- Does the issue reproduce with the stable SkiaSharp.Views.Forms 2.88.0 release (not preview)?
- Is the reporter still using Xamarin.Forms or have they migrated to MAUI?

### Automatable Actions

| Type | Risk | Confidence | Description | Details |
|------|------|------------|-------------|---------|
| update-labels | low | 0.90 (90%) | Apply type/bug, area/SkiaSharp.Views.Forms, tenet/compatibility | labels=type/bug, area/SkiaSharp.Views.Forms, tenet/compatibility |
| add-comment | medium | 0.72 (72%) | Inform reporter that the package is now obsolete and suggest migration to MAUI, while asking if the stable release fixed the issue. | — |

**Comment draft for `add-comment`:**

```markdown
Thank you for reporting this!

A few updates since this was filed:

- **`SkiaSharp.Views.Forms` is now deprecated** in favor of `SkiaSharp.Views.Maui.Controls` for .NET MAUI projects.
- The stable `SkiaSharp.Views.Forms 2.88.0` (and later 2.88.x) was released after this preview.

Could you check if this issue persists with **stable 2.88.x**? If you are still on Xamarin.Forms with a netcoreapp3.1 desktop target, please let us know your exact scenario.

If you have migrated to .NET MAUI, please use `SkiaSharp.Views.Maui.Controls` instead.
```

<details>
<summary>Raw JSON</summary>

```json
{
  "meta": {
    "schemaVersion": "1.0",
    "number": 1898,
    "repo": "mono/SkiaSharp",
    "analyzedAt": "2026-05-02T18:53:56Z"
  },
  "summary": "SkiaSharp.Views.Forms 2.88.0-preview.179 has an empty netcoreapp3.1 directory in the NuGet package, despite netcore 3.1 support reportedly being added in preview.127.",
  "classification": {
    "type": {
      "value": "type/bug",
      "confidence": 0.85
    },
    "area": {
      "value": "area/SkiaSharp.Views.Forms",
      "confidence": 0.95
    },
    "tenets": [
      "tenet/compatibility"
    ]
  },
  "evidence": {
    "bugSignals": {
      "severity": "medium",
      "regressionClaimed": true,
      "errorType": "missing-output",
      "reproQuality": "partial",
      "targetFrameworks": [
        "netcoreapp3.1"
      ]
    },
    "reproEvidence": {
      "repoLinks": []
    },
    "versionAnalysis": {
      "mentionedVersions": [
        "2.88.0-preview.120",
        "2.88.0-preview.127",
        "2.88.0-preview.179"
      ],
      "workedIn": "2.88.0-preview.127",
      "brokeIn": "2.88.0-preview.179",
      "currentRelevance": "unlikely",
      "relevanceReason": "SkiaSharp.Views.Forms is now in the OBSOLETED_NUGETS list in build.cake; the stable 2.88.x series was released and Xamarin.Forms users should migrate to SkiaSharp.Views.Maui"
    },
    "regression": {
      "isRegression": true,
      "confidence": 0.8,
      "reason": "Reporter states netcoreapp3.1 was present in preview.127 but is missing/empty in preview.179",
      "workedInVersion": "2.88.0-preview.127",
      "brokeInVersion": "2.88.0-preview.179"
    }
  },
  "analysis": {
    "summary": "The SkiaSharp.Views.Forms NuGet package had netcoreapp3.1 support added in preview.127 but the netcoreapp3.1 directory became empty by preview.179, possibly due to a build pipeline change. The package is now obsolete in the current codebase (superseded by SkiaSharp.Views.Maui), and the build script explicitly skips netcoreapp TFMs during package processing. The stable 2.88.x series was released.",
    "codeInvestigation": [
      {
        "file": "build.cake",
        "finding": "SkiaSharp.Views.Forms is listed in OBSOLETED_NUGETS alongside SkiaSharp.Views.Maui.Controls.Compatibility, indicating it is no longer actively built or maintained in the current codebase.",
        "relevance": "direct"
      },
      {
        "file": "scripts/cake/UtilsManaged.cake",
        "finding": "netcoreapp TFMs are explicitly skipped in the package processing logic with comment '// skip this one for now', which could explain why the netcoreapp3.1 directory was empty.",
        "relevance": "direct"
      }
    ],
    "keySignals": [
      {
        "text": "netcoreapp3.1 directory is empty in Nuget Package Explorer",
        "source": "issue body",
        "interpretation": "The TFM directory exists but has no DLLs — confirming the package was built without netcore3.1 assets"
      },
      {
        "text": "Support for netcore 3.1 was added in 2.88.0-preview.127 version",
        "source": "issue body - referencing release notes",
        "interpretation": "This was a regression from a known working state"
      }
    ],
    "rationale": "Classified as type/bug because netcoreapp3.1 DLLs were confirmed present in an earlier preview and are missing in a later one, indicating a regression. Area is SkiaSharp.Views.Forms since the affected package is specifically the Xamarin.Forms view layer. Severity is medium because it affects a specific target framework but the package was in preview status. The package is now obsoleted in the codebase in favor of MAUI.",
    "resolution": {
      "hypothesis": "The netcoreapp3.1 TFM was dropped from the build for SkiaSharp.Views.Forms between preview.127 and preview.179, possibly intentionally or due to a build script change. The stable 2.88.x series was later released.",
      "proposals": [
        {
          "title": "Verify if stable 2.88.0 release restored netcoreapp3.1 support",
          "description": "Check the stable SkiaSharp.Views.Forms 2.88.0 NuGet package on nuget.org to confirm whether the netcoreapp3.1 directory was restored in the final release.",
          "category": "investigation",
          "effort": "cost/xs",
          "validated": "untested"
        },
        {
          "title": "Migrate to SkiaSharp.Views.Maui for .NET 6+ / MAUI projects",
          "description": "SkiaSharp.Views.Forms is obsolete. Users should migrate to SkiaSharp.Views.Maui.Controls for MAUI apps. For Xamarin.Forms projects still on netcoreapp3.1/WPF, use SkiaSharp.Views desktop packages directly.",
          "category": "workaround",
          "effort": "cost/m",
          "validated": "untested"
        }
      ],
      "recommendedProposal": "investigate-stable",
      "recommendedReason": "Before directing users to migrate, confirm whether the stable release fixed this issue. If fixed, close as fixed; if still broken for stable, the workaround is migration to MAUI."
    }
  },
  "output": {
    "actionability": {
      "suggestedAction": "needs-investigation",
      "confidence": 0.72,
      "reason": "The bug was reported against a preview version. The package is now obsolete in the current codebase. Need to verify whether stable 2.88.0 fixed the issue before closing.",
      "suggestedReproPlatform": "linux"
    },
    "missingInfo": [
      "Does the issue reproduce with the stable SkiaSharp.Views.Forms 2.88.0 release (not preview)?",
      "Is the reporter still using Xamarin.Forms or have they migrated to MAUI?"
    ],
    "actions": [
      {
        "type": "update-labels",
        "description": "Apply type/bug, area/SkiaSharp.Views.Forms, tenet/compatibility",
        "risk": "low",
        "confidence": 0.9,
        "labels": [
          "type/bug",
          "area/SkiaSharp.Views.Forms",
          "tenet/compatibility"
        ]
      },
      {
        "type": "add-comment",
        "description": "Inform reporter that the package is now obsolete and suggest migration to MAUI, while asking if the stable release fixed the issue.",
        "risk": "medium",
        "confidence": 0.72,
        "comment": "Thank you for reporting this!\n\nA few updates since this was filed:\n\n- **`SkiaSharp.Views.Forms` is now deprecated** in favor of `SkiaSharp.Views.Maui.Controls` for .NET MAUI projects.\n- The stable `SkiaSharp.Views.Forms 2.88.0` (and later 2.88.x) was released after this preview.\n\nCould you check if this issue persists with **stable 2.88.x**? If you are still on Xamarin.Forms with a netcoreapp3.1 desktop target, please let us know your exact scenario.\n\nIf you have migrated to .NET MAUI, please use `SkiaSharp.Views.Maui.Controls` instead."
      }
    ]
  }
}
```

</details>
