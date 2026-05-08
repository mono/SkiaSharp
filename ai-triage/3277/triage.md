# Issue Triage Report — #3277

| Field | Value |
|-------|-------|
| Repository | mono/SkiaSharp |
| Analyzed | 2026-04-24T15:05:09Z |
| Type | type/question (0.92 (92%)) |
| Area | area/libSkiaSharp.native (0.90 (90%)) |
| Suggested action | close-as-not-a-bug (0.95 (95%)) |

**Issue Summary:** Reporter questioned whether SkiaSharp.NativeAssets.iOS supports iOS 18 when it appeared unavailable for net8.0-ios18.0 in NuGet, then self-resolved: the native assets package is a transitive dependency automatically included with the main SkiaSharp package.

**Analysis:** This is a usage question about NuGet package resolution for SkiaSharp.NativeAssets.iOS on iOS 18. The reporter confused the need to manually install SkiaSharp.NativeAssets.iOS with how NuGet transitive dependencies work. In SkiaSharp 3.x, the native assets packages are automatically pulled as transitive dependencies when the main SkiaSharp package is added. The reporter self-resolved within 3 days.

**Recommendations:** **close-as-not-a-bug** — Reporter self-resolved — SkiaSharp.NativeAssets.iOS is a transitive dependency and requires no manual installation. Issue already closed as not_planned by maintainer. NuGet TFM fallback handles iOS version compatibility.

---

## Classification

| Field | Value |
|-------|-------|
| Type | type/question |
| Area | area/libSkiaSharp.native |
| Platforms | os/iOS |
| Backends | — |
| Tenets | — |
| Partner | — |
| Current labels | type/bug, type/question, os/iOS, area/libSkiaSharp.native, triage/triaged |

## Evidence

### Reproduction

1. Target net8.0-ios18.0 in a .NET iOS/Android app
2. Try to install SkiaSharp 3.119 via NuGet Package Manager
3. Notice SkiaSharp.NativeAssets.iOS for 3.119 is not listed or rejected (labels only mention iOS17)
4. Reporter realized the package is a transitive dependency and self-resolved

**Environment:** Visual Studio (Windows), iOS 18, net8.0-ios18.0, SkiaSharp 3.116.0 attempted upgrade to 3.119

### Version Analysis

| Field | Value |
|-------|-------|
| Mentioned versions | 3.116.0, 3.119, 2.88.9 |
| Worked in | 2.88.9 |
| Broke in | — |
| Current relevance | unlikely |
| Relevance reason | Reporter self-resolved: SkiaSharp.NativeAssets.iOS is a transitive package automatically included with the main SkiaSharp package. Issue was closed as not_planned by maintainer. |

## Analysis

### Technical Summary

This is a usage question about NuGet package resolution for SkiaSharp.NativeAssets.iOS on iOS 18. The reporter confused the need to manually install SkiaSharp.NativeAssets.iOS with how NuGet transitive dependencies work. In SkiaSharp 3.x, the native assets packages are automatically pulled as transitive dependencies when the main SkiaSharp package is added. The reporter self-resolved within 3 days.

### Rationale

The issue title claims a bug, but the body and comment reveal it is a usage/comprehension question about NuGet package dependency resolution. The reporter assumed they needed to manually add the native assets package and was confused that iOS18 was not listed. In practice, the NativeAssets package is a transitive dependency — NuGet resolves it automatically. No broken behavior exists; the NuGet TFM fallback mechanism handles iOS versioning. Issue already closed as not_planned by maintainer.

### Key Signals

- "Cancel/close this issue! Nevermind - this is a NON-ISSUE. The 'SkiaSharp.NativeAssets.iOS' is a 'transitive package' which automatically gets included when I select 3.119 for 'SkiaSharp' main package." — **issue body (opening retraction) and comment #1** (Reporter self-resolved immediately upon discovering NuGet transitive dependency semantics. No actual defect.)
- "won't show us the 'SkiaSharp.NativeAssets.iOS' option for 3.119, which appears to be caused by our app targeting '.net8.0-iOS18' while this package support is only labeled for '.net8.0-iOS17'" — **issue body** (Misunderstanding of NuGet TFM version fallback — iOS TFMs use version-based fallback so a net8.0-ios17.x asset resolves for net8.0-ios18.0 apps.)

### Code Investigation

| File | Lines | Relevance | Finding |
|------|-------|-----------|---------|
| `binding/SkiaSharp.NativeAssets.iOS/SkiaSharp.NativeAssets.iOS.csproj` | 3 | direct | Package targets `$(TFMPrevious)-ios$(TPViOSPrevious);$(TFMCurrent)-ios$(TPViOSCurrent)` — version values come from build props. Current props show TPViOSPrevious=18.0 (net8.0-ios18.0) and TPViOSCurrent=26.2 (net10.0-ios26.2), confirming iOS 18 support exists. |
| `source/SkiaSharp.Build.props` | 75,83 | related | TPViOSPrevious is set to 18.0, TPViOSCurrent to 26.2. When this issue was filed in May 2025 against 3.116, the iOS TFM was likely still 17.x for the previous slot. NuGet's TFM fallback means iOS18 apps resolve to the iOS17 target automatically. |

### Workarounds

- No workaround needed — SkiaSharp.NativeAssets.iOS is a transitive dependency automatically included when you add the main SkiaSharp package. Do not install it manually.
- If using SkiaSharp 3.x, NuGet TFM fallback resolves iOS-versioned native asset packages automatically across minor iOS versions.

### Resolution Proposals

**Hypothesis:** User misunderstood NuGet transitive dependency resolution and TFM version fallback. The native assets package is never required to be installed manually — it flows transitively from the main package.

1. **No action required — self-resolved** — workaround, confidence 0.95 (95%), cost/xs, validated=untested
   - The reporter discovered the answer independently: SkiaSharp.NativeAssets.iOS is a transitive dependency. The issue is already closed as not_planned.

**Recommended proposal:** No action required — self-resolved

**Why:** Reporter self-resolved and maintainer closed the issue. No code change or response needed.

## Recommendations

### Actionability

| Field | Value |
|-------|-------|
| Suggested action | close-as-not-a-bug |
| Confidence | 0.95 (95%) |
| Reason | Reporter self-resolved — SkiaSharp.NativeAssets.iOS is a transitive dependency and requires no manual installation. Issue already closed as not_planned by maintainer. NuGet TFM fallback handles iOS version compatibility. |
| Suggested repro platform | linux |

### Automatable Actions

| Type | Risk | Confidence | Description | Details |
|------|------|------------|-------------|---------|
| update-labels | low | 0.90 (90%) | Correct labels: remove type/bug (not a bug), keep type/question, os/iOS, area/libSkiaSharp.native, triage/triaged | labels=type/question, os/iOS, area/libSkiaSharp.native, triage/triaged |

<details>
<summary>Raw JSON</summary>

```json
{
  "meta": {
    "schemaVersion": "1.0",
    "number": 3277,
    "repo": "mono/SkiaSharp",
    "analyzedAt": "2026-04-24T15:05:09Z",
    "currentLabels": [
      "type/bug",
      "type/question",
      "os/iOS",
      "area/libSkiaSharp.native",
      "triage/triaged"
    ]
  },
  "summary": "Reporter questioned whether SkiaSharp.NativeAssets.iOS supports iOS 18 when it appeared unavailable for net8.0-ios18.0 in NuGet, then self-resolved: the native assets package is a transitive dependency automatically included with the main SkiaSharp package.",
  "classification": {
    "type": {
      "value": "type/question",
      "confidence": 0.92
    },
    "area": {
      "value": "area/libSkiaSharp.native",
      "confidence": 0.9
    },
    "platforms": [
      "os/iOS"
    ]
  },
  "evidence": {
    "reproEvidence": {
      "stepsToReproduce": [
        "Target net8.0-ios18.0 in a .NET iOS/Android app",
        "Try to install SkiaSharp 3.119 via NuGet Package Manager",
        "Notice SkiaSharp.NativeAssets.iOS for 3.119 is not listed or rejected (labels only mention iOS17)",
        "Reporter realized the package is a transitive dependency and self-resolved"
      ],
      "environmentDetails": "Visual Studio (Windows), iOS 18, net8.0-ios18.0, SkiaSharp 3.116.0 attempted upgrade to 3.119",
      "repoLinks": []
    },
    "versionAnalysis": {
      "mentionedVersions": [
        "3.116.0",
        "3.119",
        "2.88.9"
      ],
      "workedIn": "2.88.9",
      "currentRelevance": "unlikely",
      "relevanceReason": "Reporter self-resolved: SkiaSharp.NativeAssets.iOS is a transitive package automatically included with the main SkiaSharp package. Issue was closed as not_planned by maintainer."
    }
  },
  "analysis": {
    "summary": "This is a usage question about NuGet package resolution for SkiaSharp.NativeAssets.iOS on iOS 18. The reporter confused the need to manually install SkiaSharp.NativeAssets.iOS with how NuGet transitive dependencies work. In SkiaSharp 3.x, the native assets packages are automatically pulled as transitive dependencies when the main SkiaSharp package is added. The reporter self-resolved within 3 days.",
    "rationale": "The issue title claims a bug, but the body and comment reveal it is a usage/comprehension question about NuGet package dependency resolution. The reporter assumed they needed to manually add the native assets package and was confused that iOS18 was not listed. In practice, the NativeAssets package is a transitive dependency — NuGet resolves it automatically. No broken behavior exists; the NuGet TFM fallback mechanism handles iOS versioning. Issue already closed as not_planned by maintainer.",
    "codeInvestigation": [
      {
        "file": "binding/SkiaSharp.NativeAssets.iOS/SkiaSharp.NativeAssets.iOS.csproj",
        "lines": "3",
        "finding": "Package targets `$(TFMPrevious)-ios$(TPViOSPrevious);$(TFMCurrent)-ios$(TPViOSCurrent)` — version values come from build props. Current props show TPViOSPrevious=18.0 (net8.0-ios18.0) and TPViOSCurrent=26.2 (net10.0-ios26.2), confirming iOS 18 support exists.",
        "relevance": "direct"
      },
      {
        "file": "source/SkiaSharp.Build.props",
        "lines": "75,83",
        "finding": "TPViOSPrevious is set to 18.0, TPViOSCurrent to 26.2. When this issue was filed in May 2025 against 3.116, the iOS TFM was likely still 17.x for the previous slot. NuGet's TFM fallback means iOS18 apps resolve to the iOS17 target automatically.",
        "relevance": "related"
      }
    ],
    "keySignals": [
      {
        "text": "Cancel/close this issue! Nevermind - this is a NON-ISSUE. The 'SkiaSharp.NativeAssets.iOS' is a 'transitive package' which automatically gets included when I select 3.119 for 'SkiaSharp' main package.",
        "source": "issue body (opening retraction) and comment #1",
        "interpretation": "Reporter self-resolved immediately upon discovering NuGet transitive dependency semantics. No actual defect."
      },
      {
        "text": "won't show us the 'SkiaSharp.NativeAssets.iOS' option for 3.119, which appears to be caused by our app targeting '.net8.0-iOS18' while this package support is only labeled for '.net8.0-iOS17'",
        "source": "issue body",
        "interpretation": "Misunderstanding of NuGet TFM version fallback — iOS TFMs use version-based fallback so a net8.0-ios17.x asset resolves for net8.0-ios18.0 apps."
      }
    ],
    "workarounds": [
      "No workaround needed — SkiaSharp.NativeAssets.iOS is a transitive dependency automatically included when you add the main SkiaSharp package. Do not install it manually.",
      "If using SkiaSharp 3.x, NuGet TFM fallback resolves iOS-versioned native asset packages automatically across minor iOS versions."
    ],
    "resolution": {
      "hypothesis": "User misunderstood NuGet transitive dependency resolution and TFM version fallback. The native assets package is never required to be installed manually — it flows transitively from the main package.",
      "proposals": [
        {
          "title": "No action required — self-resolved",
          "description": "The reporter discovered the answer independently: SkiaSharp.NativeAssets.iOS is a transitive dependency. The issue is already closed as not_planned.",
          "category": "workaround",
          "confidence": 0.95,
          "effort": "cost/xs",
          "validated": "untested"
        }
      ],
      "recommendedProposal": "No action required — self-resolved",
      "recommendedReason": "Reporter self-resolved and maintainer closed the issue. No code change or response needed."
    }
  },
  "output": {
    "actionability": {
      "suggestedAction": "close-as-not-a-bug",
      "confidence": 0.95,
      "reason": "Reporter self-resolved — SkiaSharp.NativeAssets.iOS is a transitive dependency and requires no manual installation. Issue already closed as not_planned by maintainer. NuGet TFM fallback handles iOS version compatibility.",
      "suggestedReproPlatform": "linux"
    },
    "actions": [
      {
        "type": "update-labels",
        "description": "Correct labels: remove type/bug (not a bug), keep type/question, os/iOS, area/libSkiaSharp.native, triage/triaged",
        "risk": "low",
        "confidence": 0.9,
        "labels": [
          "type/question",
          "os/iOS",
          "area/libSkiaSharp.native",
          "triage/triaged"
        ]
      }
    ]
  }
}
```

</details>
