# Issue Triage Report — #2382

| Field | Value |
|-------|-------|
| Repository | mono/SkiaSharp |
| Analyzed | 2026-05-04T11:30:00Z |
| Type | type/enhancement (0.78 (78%)) |
| Area | area/Build (0.85 (85%)) |
| Suggested action | needs-info (0.75 (75%)) |

**Issue Summary:** Reporter requests removal of duplicate native DLL files from the application root folder during ASP.NET MVC publish, referencing the previously closed issue #1413.

**Analysis:** The reporter is asking that duplicate native DLLs (the ones placed at the application root folder alongside the arch-specific copies in x86/x64/arm subdirectories) be removed during ASP.NET MVC publish, as they increase build size and deployment time. This was previously reported in #1413, where the behavior was explained as intentional: SkiaSharp v2.x moved native binaries to per-arch subdirectories to correctly handle Any CPU builds. The root-level copies appear to be a by-product of the NuGet runtimes/ layout and the .NET publish pipeline. The issue lacks critical details (version, specific publish output, repro steps).

**Recommendations:** **needs-info** — The issue lacks version information, TFM, publish mode, and repro steps. The behavior was already explained in #1413, but it may still be valid for newer versions or different configurations. More details are needed before investigating a fix.

---

## Classification

| Field | Value |
|-------|-------|
| Type | type/enhancement |
| Area | area/Build |
| Platforms | os/Windows-Classic |
| Backends | — |
| Tenets | tenet/performance |
| Partner | — |

## Evidence

### Reproduction

**Repository links:**
- https://github.com/mono/SkiaSharp/issues/1413 — Original bug report: Publishing ASP.NET MVC app copies native libraries to wrong folder — closed as completed in 2020; maintainer explained the behavior is by-design to support Any CPU via arch subdirectories.

## Analysis

### Technical Summary

The reporter is asking that duplicate native DLLs (the ones placed at the application root folder alongside the arch-specific copies in x86/x64/arm subdirectories) be removed during ASP.NET MVC publish, as they increase build size and deployment time. This was previously reported in #1413, where the behavior was explained as intentional: SkiaSharp v2.x moved native binaries to per-arch subdirectories to correctly handle Any CPU builds. The root-level copies appear to be a by-product of the NuGet runtimes/ layout and the .NET publish pipeline. The issue lacks critical details (version, specific publish output, repro steps).

### Rationale

Classified as type/enhancement because the behavior is known and intentional per #1413 — maintainer explicitly explained it. Reporter is requesting an optimization to reduce publish output size. Area is Build because the issue is about the NuGet native asset deployment pipeline. Confidence is moderate because the issue is sparse, no repro, and the referenced #1413 was resolved with a deliberate design decision.

### Key Signals

- "It increases the size of the build and the time needed for the pipeline to copy that build each time." — **issue body** (The concern is build size and deployment time, not a correctness bug — pointing to an enhancement request rather than a defect.)
- "We need to remove duplicate Dlls from either root or bin folder." — **issue body** (Explicit change request to the publish output behavior.)

### Code Investigation

| File | Lines | Relevance | Finding |
|------|-------|-----------|---------|
| `source/SkiaSharp.NuGet.targets` | — | context | NuGet targets file handles packaging but does not contain logic for controlling where native assets are placed during application publish. Native asset placement is controlled by the runtimes/{rid}/native/ NuGet layout. |
| `documentation/dev/packages.md` | 147-153 | direct | Documents that SkiaSharp.NativeAssets.Win32 uses runtimes/{rid}/native/ layout. In framework-dependent publish, binaries land in runtimes/{rid}/native/; in self-contained publish, binaries are flattened to the publish root. This explains the 'duplicate' behavior: the runtimes/ folder AND a root-level copy can both appear depending on build mode. |

### Next Questions

- What SkiaSharp version and .NET TFM are being used?
- Is this a framework-dependent or self-contained publish?
- What specific RID (e.g. win-x64) is being targeted?
- Is the duplicate appearing in runtimes/ AND root, or only root?

### Resolution Proposals

**Hypothesis:** The duplicate native DLLs at the root are likely produced by the .NET publish pipeline for non-platform-specific (e.g. net4.7.2) TFMs where the runtimes/ layout is not used and files are flattened. Updating the NuGet package or project targets to suppress root-level copies while preserving arch-specific ones may address this.

1. **Use explicit RID to avoid root-level copies** — workaround, cost/xs, validated=untested
   - Specifying an explicit RuntimeIdentifier (e.g. win-x64) in the publish profile forces self-contained or RID-specific output, which puts native binaries in the expected location without root duplicates.
2. **Investigate NuGet runtimes layout for net4.x TFMs** — investigation, cost/m, validated=untested
   - Review whether the SkiaSharp.NativeAssets.Win32 package needs a different native asset layout for net4.x targets to avoid root-level copies during publish.

**Recommended proposal:** workaround-1

**Why:** Specifying an explicit RID is a quick workaround that avoids the duplicate while the investigation proceeds.

## Recommendations

### Actionability

| Field | Value |
|-------|-------|
| Suggested action | needs-info |
| Confidence | 0.75 (75%) |
| Reason | The issue lacks version information, TFM, publish mode, and repro steps. The behavior was already explained in #1413, but it may still be valid for newer versions or different configurations. More details are needed before investigating a fix. |
| Suggested repro platform | windows |

### Missing Info

- SkiaSharp version being used
- .NET / .NET Framework target version
- Publish profile / RID (self-contained vs framework-dependent)
- Exact publish output folder structure (screenshot or directory listing)

### Automatable Actions

| Type | Risk | Confidence | Description | Details |
|------|------|------------|-------------|---------|
| update-labels | low | 0.85 (85%) | Apply classification labels | labels=type/enhancement, area/Build, os/Windows-Classic, tenet/performance |
| link-related | low | 0.95 (95%) | Link to prior report #1413 which covered the same area and was closed as completed | linkedIssue=#1413 |
| add-comment | medium | 0.78 (78%) | Ask for missing version and publish details, and reference #1413 for context | — |

**Comment draft for `add-comment`:**

```markdown
Thanks for the report! This area was previously investigated in #1413, where the maintainer explained that in SkiaSharp v2.x native binaries were moved to per-architecture subdirectories (`x86/`, `x64/`, `arm/`) to correctly support Any CPU builds on .NET 4.x.

Could you provide the following details so we can investigate further?

- **SkiaSharp version** you are using
- **Target framework** (e.g. `net472`, `net6.0`, `net8.0`)
- **Publish mode** — framework-dependent or self-contained?
- **RuntimeIdentifier** (e.g. `win-x64`) if set in the publish profile
- A directory listing or screenshot of the duplicate files in your publish output

As a potential workaround: specifying an explicit `RuntimeIdentifier` (e.g. `<RuntimeIdentifier>win-x64</RuntimeIdentifier>`) in your publish profile may eliminate the root-level copies.
```

<details>
<summary>Raw JSON</summary>

```json
{
  "meta": {
    "schemaVersion": "1.0",
    "number": 2382,
    "repo": "mono/SkiaSharp",
    "analyzedAt": "2026-05-04T11:30:00Z"
  },
  "summary": "Reporter requests removal of duplicate native DLL files from the application root folder during ASP.NET MVC publish, referencing the previously closed issue #1413.",
  "classification": {
    "type": {
      "value": "type/enhancement",
      "confidence": 0.78
    },
    "area": {
      "value": "area/Build",
      "confidence": 0.85
    },
    "platforms": [
      "os/Windows-Classic"
    ],
    "tenets": [
      "tenet/performance"
    ]
  },
  "evidence": {
    "reproEvidence": {
      "repoLinks": [
        {
          "url": "https://github.com/mono/SkiaSharp/issues/1413",
          "description": "Original bug report: Publishing ASP.NET MVC app copies native libraries to wrong folder — closed as completed in 2020; maintainer explained the behavior is by-design to support Any CPU via arch subdirectories."
        }
      ]
    }
  },
  "analysis": {
    "summary": "The reporter is asking that duplicate native DLLs (the ones placed at the application root folder alongside the arch-specific copies in x86/x64/arm subdirectories) be removed during ASP.NET MVC publish, as they increase build size and deployment time. This was previously reported in #1413, where the behavior was explained as intentional: SkiaSharp v2.x moved native binaries to per-arch subdirectories to correctly handle Any CPU builds. The root-level copies appear to be a by-product of the NuGet runtimes/ layout and the .NET publish pipeline. The issue lacks critical details (version, specific publish output, repro steps).",
    "codeInvestigation": [
      {
        "file": "source/SkiaSharp.NuGet.targets",
        "finding": "NuGet targets file handles packaging but does not contain logic for controlling where native assets are placed during application publish. Native asset placement is controlled by the runtimes/{rid}/native/ NuGet layout.",
        "relevance": "context"
      },
      {
        "file": "documentation/dev/packages.md",
        "finding": "Documents that SkiaSharp.NativeAssets.Win32 uses runtimes/{rid}/native/ layout. In framework-dependent publish, binaries land in runtimes/{rid}/native/; in self-contained publish, binaries are flattened to the publish root. This explains the 'duplicate' behavior: the runtimes/ folder AND a root-level copy can both appear depending on build mode.",
        "relevance": "direct",
        "lines": "147-153"
      }
    ],
    "keySignals": [
      {
        "text": "It increases the size of the build and the time needed for the pipeline to copy that build each time.",
        "source": "issue body",
        "interpretation": "The concern is build size and deployment time, not a correctness bug — pointing to an enhancement request rather than a defect."
      },
      {
        "text": "We need to remove duplicate Dlls from either root or bin folder.",
        "source": "issue body",
        "interpretation": "Explicit change request to the publish output behavior."
      }
    ],
    "rationale": "Classified as type/enhancement because the behavior is known and intentional per #1413 — maintainer explicitly explained it. Reporter is requesting an optimization to reduce publish output size. Area is Build because the issue is about the NuGet native asset deployment pipeline. Confidence is moderate because the issue is sparse, no repro, and the referenced #1413 was resolved with a deliberate design decision.",
    "nextQuestions": [
      "What SkiaSharp version and .NET TFM are being used?",
      "Is this a framework-dependent or self-contained publish?",
      "What specific RID (e.g. win-x64) is being targeted?",
      "Is the duplicate appearing in runtimes/ AND root, or only root?"
    ],
    "resolution": {
      "hypothesis": "The duplicate native DLLs at the root are likely produced by the .NET publish pipeline for non-platform-specific (e.g. net4.7.2) TFMs where the runtimes/ layout is not used and files are flattened. Updating the NuGet package or project targets to suppress root-level copies while preserving arch-specific ones may address this.",
      "proposals": [
        {
          "title": "Use explicit RID to avoid root-level copies",
          "category": "workaround",
          "description": "Specifying an explicit RuntimeIdentifier (e.g. win-x64) in the publish profile forces self-contained or RID-specific output, which puts native binaries in the expected location without root duplicates.",
          "validated": "untested",
          "effort": "cost/xs"
        },
        {
          "title": "Investigate NuGet runtimes layout for net4.x TFMs",
          "category": "investigation",
          "description": "Review whether the SkiaSharp.NativeAssets.Win32 package needs a different native asset layout for net4.x targets to avoid root-level copies during publish.",
          "validated": "untested",
          "effort": "cost/m"
        }
      ],
      "recommendedProposal": "workaround-1",
      "recommendedReason": "Specifying an explicit RID is a quick workaround that avoids the duplicate while the investigation proceeds."
    }
  },
  "output": {
    "actionability": {
      "suggestedAction": "needs-info",
      "confidence": 0.75,
      "reason": "The issue lacks version information, TFM, publish mode, and repro steps. The behavior was already explained in #1413, but it may still be valid for newer versions or different configurations. More details are needed before investigating a fix.",
      "suggestedReproPlatform": "windows"
    },
    "missingInfo": [
      "SkiaSharp version being used",
      ".NET / .NET Framework target version",
      "Publish profile / RID (self-contained vs framework-dependent)",
      "Exact publish output folder structure (screenshot or directory listing)"
    ],
    "actions": [
      {
        "type": "update-labels",
        "description": "Apply classification labels",
        "risk": "low",
        "confidence": 0.85,
        "labels": [
          "type/enhancement",
          "area/Build",
          "os/Windows-Classic",
          "tenet/performance"
        ]
      },
      {
        "type": "link-related",
        "description": "Link to prior report #1413 which covered the same area and was closed as completed",
        "risk": "low",
        "confidence": 0.95,
        "linkedIssue": 1413
      },
      {
        "type": "add-comment",
        "description": "Ask for missing version and publish details, and reference #1413 for context",
        "risk": "medium",
        "confidence": 0.78,
        "comment": "Thanks for the report! This area was previously investigated in #1413, where the maintainer explained that in SkiaSharp v2.x native binaries were moved to per-architecture subdirectories (`x86/`, `x64/`, `arm/`) to correctly support Any CPU builds on .NET 4.x.\n\nCould you provide the following details so we can investigate further?\n\n- **SkiaSharp version** you are using\n- **Target framework** (e.g. `net472`, `net6.0`, `net8.0`)\n- **Publish mode** — framework-dependent or self-contained?\n- **RuntimeIdentifier** (e.g. `win-x64`) if set in the publish profile\n- A directory listing or screenshot of the duplicate files in your publish output\n\nAs a potential workaround: specifying an explicit `RuntimeIdentifier` (e.g. `<RuntimeIdentifier>win-x64</RuntimeIdentifier>`) in your publish profile may eliminate the root-level copies."
      }
    ]
  }
}
```

</details>
