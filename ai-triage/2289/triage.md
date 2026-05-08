# Issue Triage Report — #2289

| Field | Value |
|-------|-------|
| Repository | mono/SkiaSharp |
| Analyzed | 2026-05-02T16:05:00Z |
| Type | type/question (0.88 (88%)) |
| Area | area/Build (0.85 (85%)) |
| Suggested action | close-as-external (0.82 (82%)) |

**Issue Summary:** User requests someone build a custom SkiaSharp 1.68 package with Xamarin.Forms 2.5.1.527436 dependency for Windows 10 Mobile.

**Analysis:** The reporter wants a pre-built custom SkiaSharp 1.68 NuGet package with the Xamarin.Forms dependency pinned to version 2.5.1.527436, specifically to target Windows 10 Mobile (a UWP platform). Windows 10 Mobile reached end-of-life in 2020, and SkiaSharp 1.68 is several major versions behind current releases. This is not a bug report but a build/packaging assistance request for a platform and version combination that is no longer supported.

**Recommendations:** **close-as-external** — Windows 10 Mobile is end-of-life and SkiaSharp 1.68 is an unsupported legacy release. The request is outside the project's active support scope. The maintainers cannot be expected to produce custom legacy builds for EOL platforms.

---

## Classification

| Field | Value |
|-------|-------|
| Type | type/question |
| Area | area/Build |
| Platforms | os/Windows-Universal-UWP |
| Backends | — |
| Tenets | tenet/compatibility |
| Partner | — |

## Evidence

### Reproduction

**Screenshots:**
- https://user-images.githubusercontent.com/12032735/197130488-fca4d553-e241-4be9-b644-aaf65cfe5c97.png — DepotTools error during custom build attempt

### Version Analysis

| Field | Value |
|-------|-------|
| Mentioned versions | 1.68 |
| Worked in | — |
| Broke in | — |
| Current relevance | unlikely |
| Relevance reason | SkiaSharp 1.68 is a very old release; current versions are in the 2.x and 3.x series. Windows 10 Mobile reached end-of-life in January 2020. |

## Analysis

### Technical Summary

The reporter wants a pre-built custom SkiaSharp 1.68 NuGet package with the Xamarin.Forms dependency pinned to version 2.5.1.527436, specifically to target Windows 10 Mobile (a UWP platform). Windows 10 Mobile reached end-of-life in 2020, and SkiaSharp 1.68 is several major versions behind current releases. This is not a bug report but a build/packaging assistance request for a platform and version combination that is no longer supported.

### Rationale

Classified as type/question because the reporter is asking for help/assistance rather than reporting a defect or requesting a new feature in SkiaSharp itself. The area is Build because the request is about building a custom package. The suggested action is close-as-external because the platform (Windows 10 Mobile / UWP) is end-of-life and the SkiaSharp version requested (1.68) is unsupported; this is outside the project's current scope.

### Key Signals

- "Someone, please build a custom SkiaSharp 1.68 package. Before assembling, change the Xamarin Forms dependency to version 2.5.1.527436" — **issue body** (User is asking the community or maintainers to do the build work on their behalf, not reporting a defect.)
- "I need this to build the simplest FlappyBird game for Windows 10 Mobile" — **issue body** (Windows 10 Mobile is end-of-life (EOL January 2020); the platform is no longer supported.)

### Code Investigation

| File | Lines | Relevance | Finding |
|------|-------|-----------|---------|
| `binding/SkiaSharp.Views.Forms/SkiaSharp.Views.Forms.csproj` | — | related | File does not exist; no XF 2.5 dependency references found in codebase. SkiaSharp.Views.Forms was replaced by MAUI-era packages. |
| `build.cake` | — | context | Build targets focus on current SkiaSharp versions; no mechanism for building legacy 1.68 with custom XF dependencies is present. |

### Resolution Proposals

1. **Self-serve build from legacy tag** — alternative, cost/l, validated=untested
   - Build the legacy package from the old SkiaSharp 1.68 source tag by cloning the repo, checking out the v1.68.x tag, and modifying the Xamarin.Forms package reference before running the build.

**Recommended proposal:** Self-serve build from legacy tag

**Why:** Maintainers should point the reporter to the git tag and self-serve build instructions, then close as out-of-scope for current support.

## Recommendations

### Actionability

| Field | Value |
|-------|-------|
| Suggested action | close-as-external |
| Confidence | 0.82 (82%) |
| Reason | Windows 10 Mobile is end-of-life and SkiaSharp 1.68 is an unsupported legacy release. The request is outside the project's active support scope. The maintainers cannot be expected to produce custom legacy builds for EOL platforms. |
| Suggested repro platform | windows |

### Missing Info

- Specific error encountered when attempting to build (beyond DepotTools screenshot)

### Automatable Actions

| Type | Risk | Confidence | Description | Details |
|------|------|------------|-------------|---------|
| update-labels | low | 0.88 (88%) | Apply type/question, area/Build, os/Windows-Universal-UWP, tenet/compatibility labels | labels=type/question, area/Build, os/Windows-Universal-UWP, tenet/compatibility |
| add-comment | medium | 0.82 (82%) | Explain that Windows 10 Mobile is EOL, SkiaSharp 1.68 is unsupported, and point to the git tag for self-serve builds | — |
| close-issue | medium | 0.82 (82%) | Close as not planned — EOL platform, unsupported legacy version, out of scope | stateReason=not_planned |

**Comment draft for `add-comment`:**

```markdown
Thank you for reaching out! Unfortunately this request is outside the scope of what the SkiaSharp maintainers can support:

1. **Windows 10 Mobile** reached end-of-life in January 2020 and is no longer a supported target.
2. **SkiaSharp 1.68** is a legacy release many major versions behind the current supported releases (2.x/3.x).

If you need to build SkiaSharp 1.68 with a custom Xamarin.Forms dependency, you can:
- Clone the repository and check out the `v1.68.x` tag
- Modify the Xamarin.Forms package reference in the relevant `.csproj` files
- Run the build using the instructions in `CONTRIBUTING.md`

We're closing this issue as it falls outside our current support scope, but we encourage you to self-serve from the tagged source.
```

<details>
<summary>Raw JSON</summary>

```json
{
  "meta": {
    "schemaVersion": "1.0",
    "number": 2289,
    "repo": "mono/SkiaSharp",
    "analyzedAt": "2026-05-02T16:05:00Z"
  },
  "summary": "User requests someone build a custom SkiaSharp 1.68 package with Xamarin.Forms 2.5.1.527436 dependency for Windows 10 Mobile.",
  "classification": {
    "type": {
      "value": "type/question",
      "confidence": 0.88
    },
    "area": {
      "value": "area/Build",
      "confidence": 0.85
    },
    "platforms": [
      "os/Windows-Universal-UWP"
    ],
    "tenets": [
      "tenet/compatibility"
    ]
  },
  "evidence": {
    "reproEvidence": {
      "screenshots": [
        {
          "url": "https://user-images.githubusercontent.com/12032735/197130488-fca4d553-e241-4be9-b644-aaf65cfe5c97.png",
          "description": "DepotTools error during custom build attempt"
        }
      ],
      "repoLinks": []
    },
    "versionAnalysis": {
      "mentionedVersions": [
        "1.68"
      ],
      "currentRelevance": "unlikely",
      "relevanceReason": "SkiaSharp 1.68 is a very old release; current versions are in the 2.x and 3.x series. Windows 10 Mobile reached end-of-life in January 2020."
    }
  },
  "analysis": {
    "summary": "The reporter wants a pre-built custom SkiaSharp 1.68 NuGet package with the Xamarin.Forms dependency pinned to version 2.5.1.527436, specifically to target Windows 10 Mobile (a UWP platform). Windows 10 Mobile reached end-of-life in 2020, and SkiaSharp 1.68 is several major versions behind current releases. This is not a bug report but a build/packaging assistance request for a platform and version combination that is no longer supported.",
    "codeInvestigation": [
      {
        "file": "binding/SkiaSharp.Views.Forms/SkiaSharp.Views.Forms.csproj",
        "finding": "File does not exist; no XF 2.5 dependency references found in codebase. SkiaSharp.Views.Forms was replaced by MAUI-era packages.",
        "relevance": "related"
      },
      {
        "file": "build.cake",
        "finding": "Build targets focus on current SkiaSharp versions; no mechanism for building legacy 1.68 with custom XF dependencies is present.",
        "relevance": "context"
      }
    ],
    "keySignals": [
      {
        "text": "Someone, please build a custom SkiaSharp 1.68 package. Before assembling, change the Xamarin Forms dependency to version 2.5.1.527436",
        "source": "issue body",
        "interpretation": "User is asking the community or maintainers to do the build work on their behalf, not reporting a defect."
      },
      {
        "text": "I need this to build the simplest FlappyBird game for Windows 10 Mobile",
        "source": "issue body",
        "interpretation": "Windows 10 Mobile is end-of-life (EOL January 2020); the platform is no longer supported."
      }
    ],
    "rationale": "Classified as type/question because the reporter is asking for help/assistance rather than reporting a defect or requesting a new feature in SkiaSharp itself. The area is Build because the request is about building a custom package. The suggested action is close-as-external because the platform (Windows 10 Mobile / UWP) is end-of-life and the SkiaSharp version requested (1.68) is unsupported; this is outside the project's current scope.",
    "resolution": {
      "proposals": [
        {
          "title": "Self-serve build from legacy tag",
          "category": "alternative",
          "description": "Build the legacy package from the old SkiaSharp 1.68 source tag by cloning the repo, checking out the v1.68.x tag, and modifying the Xamarin.Forms package reference before running the build.",
          "validated": "untested",
          "effort": "cost/l"
        }
      ],
      "recommendedProposal": "Self-serve build from legacy tag",
      "recommendedReason": "Maintainers should point the reporter to the git tag and self-serve build instructions, then close as out-of-scope for current support."
    }
  },
  "output": {
    "actionability": {
      "suggestedAction": "close-as-external",
      "confidence": 0.82,
      "reason": "Windows 10 Mobile is end-of-life and SkiaSharp 1.68 is an unsupported legacy release. The request is outside the project's active support scope. The maintainers cannot be expected to produce custom legacy builds for EOL platforms.",
      "suggestedReproPlatform": "windows"
    },
    "missingInfo": [
      "Specific error encountered when attempting to build (beyond DepotTools screenshot)"
    ],
    "actions": [
      {
        "type": "update-labels",
        "description": "Apply type/question, area/Build, os/Windows-Universal-UWP, tenet/compatibility labels",
        "risk": "low",
        "confidence": 0.88,
        "labels": [
          "type/question",
          "area/Build",
          "os/Windows-Universal-UWP",
          "tenet/compatibility"
        ]
      },
      {
        "type": "add-comment",
        "description": "Explain that Windows 10 Mobile is EOL, SkiaSharp 1.68 is unsupported, and point to the git tag for self-serve builds",
        "risk": "medium",
        "confidence": 0.82,
        "comment": "Thank you for reaching out! Unfortunately this request is outside the scope of what the SkiaSharp maintainers can support:\n\n1. **Windows 10 Mobile** reached end-of-life in January 2020 and is no longer a supported target.\n2. **SkiaSharp 1.68** is a legacy release many major versions behind the current supported releases (2.x/3.x).\n\nIf you need to build SkiaSharp 1.68 with a custom Xamarin.Forms dependency, you can:\n- Clone the repository and check out the `v1.68.x` tag\n- Modify the Xamarin.Forms package reference in the relevant `.csproj` files\n- Run the build using the instructions in `CONTRIBUTING.md`\n\nWe're closing this issue as it falls outside our current support scope, but we encourage you to self-serve from the tagged source."
      },
      {
        "type": "close-issue",
        "description": "Close as not planned — EOL platform, unsupported legacy version, out of scope",
        "risk": "medium",
        "confidence": 0.82,
        "stateReason": "not_planned"
      }
    ]
  }
}
```

</details>
