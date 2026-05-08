# Issue Triage Report — #3372

| Field | Value |
|-------|-------|
| Repository | mono/SkiaSharp |
| Analyzed | 2026-04-21T21:30:52Z |
| Type | type/feature-request (0.92 (92%)) |
| Area | area/SkiaSharp.Views (0.90 (90%)) |
| Suggested action | keep-open (0.88 (88%)) |

**Issue Summary:** Request to replace the Microsoft.WindowsAppSDK metapackage dependency in SkiaSharp.Views.WinUI with the more targeted Microsoft.WindowsAppSDK.WinUI sub-package (available from WASDK 1.8+) to reduce forced transitive dependencies on downstream consumers.

**Analysis:** SkiaSharp.Views.WinUI currently depends on the Microsoft.WindowsAppSDK metapackage (v1.4) rather than the targeted Microsoft.WindowsAppSDK.WinUI sub-package. This creates a viral transitive dependency that forces all downstream consumers (MAUI, Windows Community Toolkit, etc.) to reference the full metapackage. The Microsoft.WindowsAppSDK.WinUI sub-package only exists from WASDK 1.8+. The maintainer (mattleibow) has acknowledged the request and investigated it but determined the upgrade must wait: current MAUI workloads ship with WASDK 1.7, mixing WASDK sub-package versions is unsupported until 2.1+, and a bump to 1.8 today would break users on the released VS MAUI workload. The request is legitimate and tracked; the blocker is an ecosystem timing issue.

**Recommendations:** **keep-open** — The maintainer has acknowledged and investigated the request. The upgrade is technically valid but blocked by MAUI VS workload still shipping WASDK 1.7 and WASDK version-mixing restrictions in 1.8/2.0. The issue should remain open until the MAUI workload ships WASDK 1.8 in a released VS version.

---

## Classification

| Field | Value |
|-------|-------|
| Type | type/feature-request |
| Area | area/SkiaSharp.Views |
| Platforms | os/Windows-WinUI |
| Backends | — |
| Tenets | tenet/compatibility |
| Partner | partner/maui |
| Current labels | area/SkiaSharp.Views, type/feature-request, os/Windows-WinUI |

## Evidence

### Reproduction

**Environment:** SkiaSharp.Views.WinUI references Microsoft.WindowsAppSDK 1.4.230913002 metapackage. Microsoft.WindowsAppSDK.WinUI sub-package only exists from 1.8 onwards.

**Related issues:** #2999

**Repository links:**
- https://github.com/mono/SkiaSharp/issues/3372#issuecomment-4175921120 — Maintainer (mattleibow) investigation comment: WASDK 1.8 sub-package required but current MAUI workloads still on 1.7
- https://github.com/mono/SkiaSharp/issues/3372#issuecomment-4188926904 — Maintainer notes WASDK 1.8/2.0 version mixing is not supported; 2.1+ targets public stable ABI enabling independent sub-package versioning

### Version Analysis

| Field | Value |
|-------|-------|
| Mentioned versions | 1.4.230913002, 1.6, 1.7, 1.8, 2.0, 2.1 |
| Worked in | — |
| Broke in | — |
| Current relevance | likely |
| Relevance reason | The SkiaSharp.Views.WinUI.csproj still references Microsoft.WindowsAppSDK 1.4.230913002 metapackage. The Microsoft.WindowsAppSDK.WinUI sub-package only exists from 1.8 onwards, and MAUI shipped workloads are still on 1.7 as of the latest investigation. |

## Analysis

### Technical Summary

SkiaSharp.Views.WinUI currently depends on the Microsoft.WindowsAppSDK metapackage (v1.4) rather than the targeted Microsoft.WindowsAppSDK.WinUI sub-package. This creates a viral transitive dependency that forces all downstream consumers (MAUI, Windows Community Toolkit, etc.) to reference the full metapackage. The Microsoft.WindowsAppSDK.WinUI sub-package only exists from WASDK 1.8+. The maintainer (mattleibow) has acknowledged the request and investigated it but determined the upgrade must wait: current MAUI workloads ship with WASDK 1.7, mixing WASDK sub-package versions is unsupported until 2.1+, and a bump to 1.8 today would break users on the released VS MAUI workload. The request is legitimate and tracked; the blocker is an ecosystem timing issue.

### Rationale

Classified as type/feature-request because the change involves switching a package dependency reference rather than fixing broken behaviour. Area is area/SkiaSharp.Views because the affected project is SkiaSharp.Views.WinUI. Platform is os/Windows-WinUI. The tenet/compatibility label applies because the core motivation is reducing forced transitive dependency exposure downstream. partner/maui is appropriate because MAUI workload compatibility is the main blocker and the change directly impacts MAUI's upgrade timeline. The suggested action is keep-open: the request is valid, technically sound, and the maintainer is actively tracking it — the sole blocker is ecosystem timing (MAUI WASDK workload version).

### Key Signals

- "Please upgrade the WinUI package to reference Microsoft.WindowsAppSDK.WinUI 1.8, so that it no longer relies on the Microsoft.WindowsAppSDK metapackage" — **issue body** (Clear request to narrow the dependency from the full metapackage to the WinUI-only sub-package starting with WASDK 1.8.)
- "The Microsoft.WindowsAppSDK.WinUI subpackage (which would let me depend on just the WinUI piece instead of the full metapackage) only exists starting at 1.8 — there are no 1.6 or 1.7 versions of it." — **comment by mattleibow** (Confirmed the technical blocker: the targeted sub-package is new in 1.8 only.)
- "I need to wait for MAUI to update VS and the workloads to 1.8 first." — **comment by mattleibow** (Upgrade is blocked by the MAUI VS workload still shipping WASDK 1.7; bumping now would break users on the released MAUI workload.)
- "For 1.8 and 2.0 it is not supported to mix and match versions." — **comment by mattleibow (from Windows team conversation)** (WASDK sub-package version mixing is unsupported until 2.1+, making a partial migration risky.)
- "None of the people using the Toolkit could update their apps to 1.8 either because we weren't there yet" — **comment by michael-hawker (Windows Community Toolkit)** (Confirms that ecosystem-wide impact: library-level fixes are required before end-user apps can upgrade.)

### Code Investigation

| File | Lines | Relevance | Finding |
|------|-------|-----------|---------|
| `source/SkiaSharp.Views/SkiaSharp.Views.WinUI/SkiaSharp.Views.WinUI.csproj` | 17 | direct | Line 17: PackageReference to Microsoft.WindowsAppSDK version 1.4.230913002 (metapackage). This is the dependency the reporter wants replaced with the Microsoft.WindowsAppSDK.WinUI sub-package. |
| `binding/SkiaSharp.NativeAssets.WinUI/SkiaSharp.NativeAssets.WinUI.csproj` | — | context | Native assets project for WinUI does not reference WindowsAppSDK directly; only the Views project carries the metapackage dependency. |

### Next Questions

- Has the MAUI VS workload been updated to WASDK 1.8 in a released VS version?
- Has mattleibow opened the promised PR for multi-target net9/net10 support with WASDK 1.8?
- Is WASDK 2.1 with stable sub-package ABI released and available for targeting?

## Recommendations

### Actionability

| Field | Value |
|-------|-------|
| Suggested action | keep-open |
| Confidence | 0.88 (88%) |
| Reason | The maintainer has acknowledged and investigated the request. The upgrade is technically valid but blocked by MAUI VS workload still shipping WASDK 1.7 and WASDK version-mixing restrictions in 1.8/2.0. The issue should remain open until the MAUI workload ships WASDK 1.8 in a released VS version. |
| Suggested repro platform | windows |

### Automatable Actions

| Type | Risk | Confidence | Description | Details |
|------|------|------------|-------------|---------|
| update-labels | low | 0.90 (90%) | Confirm or add labels: type/feature-request, area/SkiaSharp.Views, os/Windows-WinUI, tenet/compatibility, partner/maui | labels=type/feature-request, area/SkiaSharp.Views, os/Windows-WinUI, tenet/compatibility, partner/maui |

<details>
<summary>Raw JSON</summary>

```json
{
  "meta": {
    "schemaVersion": "1.0",
    "number": 3372,
    "repo": "mono/SkiaSharp",
    "analyzedAt": "2026-04-21T21:30:52Z",
    "currentLabels": [
      "area/SkiaSharp.Views",
      "type/feature-request",
      "os/Windows-WinUI"
    ]
  },
  "summary": "Request to replace the Microsoft.WindowsAppSDK metapackage dependency in SkiaSharp.Views.WinUI with the more targeted Microsoft.WindowsAppSDK.WinUI sub-package (available from WASDK 1.8+) to reduce forced transitive dependencies on downstream consumers.",
  "classification": {
    "type": {
      "value": "type/feature-request",
      "confidence": 0.92
    },
    "area": {
      "value": "area/SkiaSharp.Views",
      "confidence": 0.9
    },
    "platforms": [
      "os/Windows-WinUI"
    ],
    "tenets": [
      "tenet/compatibility"
    ],
    "partner": "partner/maui"
  },
  "evidence": {
    "reproEvidence": {
      "environmentDetails": "SkiaSharp.Views.WinUI references Microsoft.WindowsAppSDK 1.4.230913002 metapackage. Microsoft.WindowsAppSDK.WinUI sub-package only exists from 1.8 onwards.",
      "relatedIssues": [
        2999
      ],
      "repoLinks": [
        {
          "url": "https://github.com/mono/SkiaSharp/issues/3372#issuecomment-4175921120",
          "description": "Maintainer (mattleibow) investigation comment: WASDK 1.8 sub-package required but current MAUI workloads still on 1.7"
        },
        {
          "url": "https://github.com/mono/SkiaSharp/issues/3372#issuecomment-4188926904",
          "description": "Maintainer notes WASDK 1.8/2.0 version mixing is not supported; 2.1+ targets public stable ABI enabling independent sub-package versioning"
        }
      ]
    },
    "versionAnalysis": {
      "mentionedVersions": [
        "1.4.230913002",
        "1.6",
        "1.7",
        "1.8",
        "2.0",
        "2.1"
      ],
      "currentRelevance": "likely",
      "relevanceReason": "The SkiaSharp.Views.WinUI.csproj still references Microsoft.WindowsAppSDK 1.4.230913002 metapackage. The Microsoft.WindowsAppSDK.WinUI sub-package only exists from 1.8 onwards, and MAUI shipped workloads are still on 1.7 as of the latest investigation."
    }
  },
  "analysis": {
    "summary": "SkiaSharp.Views.WinUI currently depends on the Microsoft.WindowsAppSDK metapackage (v1.4) rather than the targeted Microsoft.WindowsAppSDK.WinUI sub-package. This creates a viral transitive dependency that forces all downstream consumers (MAUI, Windows Community Toolkit, etc.) to reference the full metapackage. The Microsoft.WindowsAppSDK.WinUI sub-package only exists from WASDK 1.8+. The maintainer (mattleibow) has acknowledged the request and investigated it but determined the upgrade must wait: current MAUI workloads ship with WASDK 1.7, mixing WASDK sub-package versions is unsupported until 2.1+, and a bump to 1.8 today would break users on the released VS MAUI workload. The request is legitimate and tracked; the blocker is an ecosystem timing issue.",
    "codeInvestigation": [
      {
        "file": "source/SkiaSharp.Views/SkiaSharp.Views.WinUI/SkiaSharp.Views.WinUI.csproj",
        "finding": "Line 17: PackageReference to Microsoft.WindowsAppSDK version 1.4.230913002 (metapackage). This is the dependency the reporter wants replaced with the Microsoft.WindowsAppSDK.WinUI sub-package.",
        "relevance": "direct",
        "lines": "17"
      },
      {
        "file": "binding/SkiaSharp.NativeAssets.WinUI/SkiaSharp.NativeAssets.WinUI.csproj",
        "finding": "Native assets project for WinUI does not reference WindowsAppSDK directly; only the Views project carries the metapackage dependency.",
        "relevance": "context"
      }
    ],
    "keySignals": [
      {
        "text": "Please upgrade the WinUI package to reference Microsoft.WindowsAppSDK.WinUI 1.8, so that it no longer relies on the Microsoft.WindowsAppSDK metapackage",
        "source": "issue body",
        "interpretation": "Clear request to narrow the dependency from the full metapackage to the WinUI-only sub-package starting with WASDK 1.8."
      },
      {
        "text": "The Microsoft.WindowsAppSDK.WinUI subpackage (which would let me depend on just the WinUI piece instead of the full metapackage) only exists starting at 1.8 — there are no 1.6 or 1.7 versions of it.",
        "source": "comment by mattleibow",
        "interpretation": "Confirmed the technical blocker: the targeted sub-package is new in 1.8 only."
      },
      {
        "text": "I need to wait for MAUI to update VS and the workloads to 1.8 first.",
        "source": "comment by mattleibow",
        "interpretation": "Upgrade is blocked by the MAUI VS workload still shipping WASDK 1.7; bumping now would break users on the released MAUI workload."
      },
      {
        "text": "For 1.8 and 2.0 it is not supported to mix and match versions.",
        "source": "comment by mattleibow (from Windows team conversation)",
        "interpretation": "WASDK sub-package version mixing is unsupported until 2.1+, making a partial migration risky."
      },
      {
        "text": "None of the people using the Toolkit could update their apps to 1.8 either because we weren't there yet",
        "source": "comment by michael-hawker (Windows Community Toolkit)",
        "interpretation": "Confirms that ecosystem-wide impact: library-level fixes are required before end-user apps can upgrade."
      }
    ],
    "rationale": "Classified as type/feature-request because the change involves switching a package dependency reference rather than fixing broken behaviour. Area is area/SkiaSharp.Views because the affected project is SkiaSharp.Views.WinUI. Platform is os/Windows-WinUI. The tenet/compatibility label applies because the core motivation is reducing forced transitive dependency exposure downstream. partner/maui is appropriate because MAUI workload compatibility is the main blocker and the change directly impacts MAUI's upgrade timeline. The suggested action is keep-open: the request is valid, technically sound, and the maintainer is actively tracking it — the sole blocker is ecosystem timing (MAUI WASDK workload version).",
    "nextQuestions": [
      "Has the MAUI VS workload been updated to WASDK 1.8 in a released VS version?",
      "Has mattleibow opened the promised PR for multi-target net9/net10 support with WASDK 1.8?",
      "Is WASDK 2.1 with stable sub-package ABI released and available for targeting?"
    ]
  },
  "output": {
    "actionability": {
      "suggestedAction": "keep-open",
      "confidence": 0.88,
      "reason": "The maintainer has acknowledged and investigated the request. The upgrade is technically valid but blocked by MAUI VS workload still shipping WASDK 1.7 and WASDK version-mixing restrictions in 1.8/2.0. The issue should remain open until the MAUI workload ships WASDK 1.8 in a released VS version.",
      "suggestedReproPlatform": "windows"
    },
    "actions": [
      {
        "type": "update-labels",
        "description": "Confirm or add labels: type/feature-request, area/SkiaSharp.Views, os/Windows-WinUI, tenet/compatibility, partner/maui",
        "risk": "low",
        "confidence": 0.9,
        "labels": [
          "type/feature-request",
          "area/SkiaSharp.Views",
          "os/Windows-WinUI",
          "tenet/compatibility",
          "partner/maui"
        ]
      }
    ]
  }
}
```

</details>
