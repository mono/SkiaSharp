# Issue Triage Report — #2413

| Field | Value |
|-------|-------|
| Repository | mono/SkiaSharp |
| Analyzed | 2026-04-23T13:40:00Z |
| Type | type/bug (0.92 (92%)) |
| Area | area/SkiaSharp (0.75 (75%)) |
| Suggested action | needs-investigation (0.82 (82%)) |

**Issue Summary:** SVGs containing multiple <path> elements fail to render in SkiaSharp 2.88.x on Android/iOS after upgrading from 2.80.4; SVGs with a single <path> tag render correctly in both versions.

**Analysis:** SVG files with multiple <path> elements fail to render after upgrading from SkiaSharp 2.80.4 to 2.88.x. The SkiaSharp 2.88 release updated to a newer Skia milestone whose SVG DOM parser may handle multi-path SVG documents differently or introduced a regression in the SVG rendering pipeline.

**Recommendations:** **needs-investigation** — Clear regression with screenshots and reproduction repository. Root cause likely in upstream Skia SVG DOM changes between Skia milestones used in 2.80.4 vs 2.88.x. Needs deeper code investigation to confirm.

---

## Classification

| Field | Value |
|-------|-------|
| Type | type/bug |
| Area | area/SkiaSharp |
| Platforms | os/Android, os/iOS |
| Backends | backend/SVG |
| Tenets | tenet/compatibility |
| Partner | — |

## Evidence

### Reproduction

1. Load an SVG file containing multiple <path> elements using SKSvg (SkiaSharp.Extended.Svg)
2. Render the SVG on an Android or iOS device using SkiaSharp 2.88.x
3. Observe that SVG renders blank or incorrectly on 2.88.x
4. Revert to SkiaSharp 2.80.4 and observe correct rendering

**Environment:** SkiaSharp 2.88.3, SkiaSharp.Views.Forms, VS Community 2022 17.5.1, Android 13.0 API 33, Huawei Mate 9 Android 9

**Repository links:**
- https://github.com/fverdou/SkiaSharpIssue — Reporter minimal reproduction project (Xamarin Android/iOS)

### Bug Signals

| Field | Value |
|-------|-------|
| Severity | medium |
| Regression claimed | True |
| Error type | wrong-output |
| Error message | — |
| Repro quality | partial |
| Target frameworks | xamarin.android, xamarin.ios |

### Version Analysis

| Field | Value |
|-------|-------|
| Mentioned versions | 2.88.3, 2.80.4 |
| Worked in | 2.80.4 |
| Broke in | 2.88.0 |
| Current relevance | likely |
| Relevance reason | Issue remains open with no maintainer fix or response; no evidence of resolution in later releases. |

### Regression

| Field | Value |
|-------|-------|
| Is regression | True |
| Confidence | 0.92 (92%) |
| Reason | Reporter explicitly states SVGs rendered correctly in 2.80.4 and broke after upgrading to 2.88.x with no code changes. Screenshots confirm visual regression. Reverting to 2.80.4 restores correct rendering. |
| Worked in version | 2.80.4 |
| Broke in version | 2.88.0 |

## Analysis

### Technical Summary

SVG files with multiple <path> elements fail to render after upgrading from SkiaSharp 2.80.4 to 2.88.x. The SkiaSharp 2.88 release updated to a newer Skia milestone whose SVG DOM parser may handle multi-path SVG documents differently or introduced a regression in the SVG rendering pipeline.

### Rationale

This is a confirmed regression with before/after screenshots and a linked reproduction repository. The behavior change is directly tied to upgrading the SkiaSharp version from 2.80.4 to 2.88.x with no code changes. The issue is SVG rendering output (wrong-output type/bug), not a usage question. Investigation is needed to determine if the root cause is in the upstream Skia SVG DOM, the SkiaSharp.Extended.Svg package, or the SkiaSharp bindings themselves.

### Key Signals

- "svg's with multiple path tags appear just fine on 2.80.4 but don't appear on 2.88.x" — **issue body** (Regression is specifically triggered by multi-path SVGs, indicating a parsing or rendering traversal change in the SVG renderer between Skia milestones.)
- "Reverting back to 2.80.4 made both SVGs render again like in the first picture" — **issue body** (Confirms the SkiaSharp version upgrade is the direct cause of the regression.)
- "Now svg's with tags like <a:linearGradient> were added and they do not get rendered at all on 2.80.4 that we are still using." — **issue comment (fverdou, 2023-07-04)** (Reporter is also experiencing broader SVG spec compliance gaps, suggesting SVG support in SkiaSharp has historically had limitations beyond this specific regression.)

### Code Investigation

| File | Lines | Relevance | Finding |
|------|-------|-----------|---------|
| `binding/SkiaSharp/SKSVG.cs` | — | direct | Only SKSvgCanvas (for SVG output/writing) exists in the main binding. There is no SKSvg class for loading and rendering SVG files in the main SkiaSharp binding — SVG loading relies on SkiaSharp.Extended.Svg or direct Skia SVG DOM APIs. |
| `binding/SkiaSharp/SkiaApi.generated.cs` | — | direct | Only sk_svgcanvas_create_with_stream is present in the generated bindings. No sk_svgdom_* or SVG DOM loading APIs are bound, confirming that SVG file loading is not directly exposed through the main SkiaSharp binding. |

### Workarounds

- Revert to SkiaSharp 2.80.4 (confirmed working for multi-path SVGs)
- Use Svg.Skia (https://github.com/wieslawsoltes/Svg.Skia) as an alternative SVG renderer with better SVG specification compliance

### Next Questions

- Is the reporter using SkiaSharp.Extended.Svg (SKSvg class) or a different SVG loading method?
- Can the reporter share the minimal SVG file that reproduces the issue?
- Does the issue reproduce in SkiaSharp 3.x (latest)?
- Is the root cause in the upstream Skia SVG DOM parser or in SkiaSharp.Extended.Svg compatibility with 2.88.x?

### Resolution Proposals

**Hypothesis:** The Skia milestone bump in SkiaSharp 2.88.x changed the behavior of Skia's built-in SVG DOM parser for multi-path SVG documents, causing them to render blank or only partially on Android/iOS.

1. **Investigate Skia SVG DOM changes between milestones** — investigation, confidence 0.80 (80%), cost/m, validated=untested
   - Compare the Skia SVG DOM implementation between the Skia version used in SkiaSharp 2.80.4 and 2.88.x. Identify any upstream SVG parser changes that affect multi-path SVG document rendering.
2. **Use Svg.Skia as workaround** — workaround, confidence 0.70 (70%), cost/s, validated=untested
   - The third-party library Svg.Skia (https://github.com/wieslawsoltes/Svg.Skia) provides a more complete SVG renderer built on top of SkiaSharp APIs with better SVG specification compliance than the built-in SKSvg.

**Recommended proposal:** Investigate Skia SVG DOM changes between milestones

**Why:** Understanding the exact root cause in the Skia version bump is prerequisite to any targeted fix or mitigation.

## Recommendations

### Actionability

| Field | Value |
|-------|-------|
| Suggested action | needs-investigation |
| Confidence | 0.82 (82%) |
| Reason | Clear regression with screenshots and reproduction repository. Root cause likely in upstream Skia SVG DOM changes between Skia milestones used in 2.80.4 vs 2.88.x. Needs deeper code investigation to confirm. |
| Suggested repro platform | linux |

### Automatable Actions

| Type | Risk | Confidence | Description | Details |
|------|------|------------|-------------|---------|
| update-labels | low | 0.90 (90%) | Apply type/bug, area/SkiaSharp, platform, backend, and compatibility tenet labels | labels=type/bug, area/SkiaSharp, os/Android, os/iOS, backend/SVG, tenet/compatibility |
| add-comment | medium | 0.80 (80%) | Post analysis noting the regression and requesting minimal SVG reproduction and version confirmation | — |

**Comment draft for `add-comment`:**

```markdown
Thank you for the detailed report and reproduction repository.

This appears to be a regression in SVG rendering between SkiaSharp 2.80.4 and 2.88.x — specifically affecting SVG files with multiple `<path>` elements.

To help narrow down the root cause:
1. Are you using the `SKSvg` class from `SkiaSharp.Extended.Svg`, or a different approach to load the SVG?
2. Could you share a minimal SVG file that demonstrates the issue?
3. Does the problem reproduce with the latest SkiaSharp 3.x release?

As a short-term workaround, you could consider [Svg.Skia](https://github.com/wieslawsoltes/Svg.Skia), a third-party SVG renderer built on SkiaSharp with broader SVG specification support.
```

<details>
<summary>Raw JSON</summary>

```json
{
  "meta": {
    "schemaVersion": "1.0",
    "number": 2413,
    "repo": "mono/SkiaSharp",
    "analyzedAt": "2026-04-23T13:40:00Z"
  },
  "summary": "SVGs containing multiple <path> elements fail to render in SkiaSharp 2.88.x on Android/iOS after upgrading from 2.80.4; SVGs with a single <path> tag render correctly in both versions.",
  "classification": {
    "type": {
      "value": "type/bug",
      "confidence": 0.92
    },
    "area": {
      "value": "area/SkiaSharp",
      "confidence": 0.75
    },
    "platforms": [
      "os/Android",
      "os/iOS"
    ],
    "backends": [
      "backend/SVG"
    ],
    "tenets": [
      "tenet/compatibility"
    ]
  },
  "evidence": {
    "bugSignals": {
      "severity": "medium",
      "regressionClaimed": true,
      "errorType": "wrong-output",
      "reproQuality": "partial",
      "targetFrameworks": [
        "xamarin.android",
        "xamarin.ios"
      ]
    },
    "reproEvidence": {
      "stepsToReproduce": [
        "Load an SVG file containing multiple <path> elements using SKSvg (SkiaSharp.Extended.Svg)",
        "Render the SVG on an Android or iOS device using SkiaSharp 2.88.x",
        "Observe that SVG renders blank or incorrectly on 2.88.x",
        "Revert to SkiaSharp 2.80.4 and observe correct rendering"
      ],
      "environmentDetails": "SkiaSharp 2.88.3, SkiaSharp.Views.Forms, VS Community 2022 17.5.1, Android 13.0 API 33, Huawei Mate 9 Android 9",
      "repoLinks": [
        {
          "url": "https://github.com/fverdou/SkiaSharpIssue",
          "description": "Reporter minimal reproduction project (Xamarin Android/iOS)"
        }
      ]
    },
    "versionAnalysis": {
      "mentionedVersions": [
        "2.88.3",
        "2.80.4"
      ],
      "workedIn": "2.80.4",
      "brokeIn": "2.88.0",
      "currentRelevance": "likely",
      "relevanceReason": "Issue remains open with no maintainer fix or response; no evidence of resolution in later releases."
    },
    "regression": {
      "isRegression": true,
      "confidence": 0.92,
      "reason": "Reporter explicitly states SVGs rendered correctly in 2.80.4 and broke after upgrading to 2.88.x with no code changes. Screenshots confirm visual regression. Reverting to 2.80.4 restores correct rendering.",
      "workedInVersion": "2.80.4",
      "brokeInVersion": "2.88.0"
    }
  },
  "analysis": {
    "summary": "SVG files with multiple <path> elements fail to render after upgrading from SkiaSharp 2.80.4 to 2.88.x. The SkiaSharp 2.88 release updated to a newer Skia milestone whose SVG DOM parser may handle multi-path SVG documents differently or introduced a regression in the SVG rendering pipeline.",
    "rationale": "This is a confirmed regression with before/after screenshots and a linked reproduction repository. The behavior change is directly tied to upgrading the SkiaSharp version from 2.80.4 to 2.88.x with no code changes. The issue is SVG rendering output (wrong-output type/bug), not a usage question. Investigation is needed to determine if the root cause is in the upstream Skia SVG DOM, the SkiaSharp.Extended.Svg package, or the SkiaSharp bindings themselves.",
    "keySignals": [
      {
        "text": "svg's with multiple path tags appear just fine on 2.80.4 but don't appear on 2.88.x",
        "source": "issue body",
        "interpretation": "Regression is specifically triggered by multi-path SVGs, indicating a parsing or rendering traversal change in the SVG renderer between Skia milestones."
      },
      {
        "text": "Reverting back to 2.80.4 made both SVGs render again like in the first picture",
        "source": "issue body",
        "interpretation": "Confirms the SkiaSharp version upgrade is the direct cause of the regression."
      },
      {
        "text": "Now svg's with tags like <a:linearGradient> were added and they do not get rendered at all on 2.80.4 that we are still using.",
        "source": "issue comment (fverdou, 2023-07-04)",
        "interpretation": "Reporter is also experiencing broader SVG spec compliance gaps, suggesting SVG support in SkiaSharp has historically had limitations beyond this specific regression."
      }
    ],
    "codeInvestigation": [
      {
        "file": "binding/SkiaSharp/SKSVG.cs",
        "finding": "Only SKSvgCanvas (for SVG output/writing) exists in the main binding. There is no SKSvg class for loading and rendering SVG files in the main SkiaSharp binding — SVG loading relies on SkiaSharp.Extended.Svg or direct Skia SVG DOM APIs.",
        "relevance": "direct"
      },
      {
        "file": "binding/SkiaSharp/SkiaApi.generated.cs",
        "finding": "Only sk_svgcanvas_create_with_stream is present in the generated bindings. No sk_svgdom_* or SVG DOM loading APIs are bound, confirming that SVG file loading is not directly exposed through the main SkiaSharp binding.",
        "relevance": "direct"
      }
    ],
    "workarounds": [
      "Revert to SkiaSharp 2.80.4 (confirmed working for multi-path SVGs)",
      "Use Svg.Skia (https://github.com/wieslawsoltes/Svg.Skia) as an alternative SVG renderer with better SVG specification compliance"
    ],
    "nextQuestions": [
      "Is the reporter using SkiaSharp.Extended.Svg (SKSvg class) or a different SVG loading method?",
      "Can the reporter share the minimal SVG file that reproduces the issue?",
      "Does the issue reproduce in SkiaSharp 3.x (latest)?",
      "Is the root cause in the upstream Skia SVG DOM parser or in SkiaSharp.Extended.Svg compatibility with 2.88.x?"
    ],
    "resolution": {
      "hypothesis": "The Skia milestone bump in SkiaSharp 2.88.x changed the behavior of Skia's built-in SVG DOM parser for multi-path SVG documents, causing them to render blank or only partially on Android/iOS.",
      "proposals": [
        {
          "title": "Investigate Skia SVG DOM changes between milestones",
          "description": "Compare the Skia SVG DOM implementation between the Skia version used in SkiaSharp 2.80.4 and 2.88.x. Identify any upstream SVG parser changes that affect multi-path SVG document rendering.",
          "category": "investigation",
          "confidence": 0.8,
          "effort": "cost/m",
          "validated": "untested"
        },
        {
          "title": "Use Svg.Skia as workaround",
          "description": "The third-party library Svg.Skia (https://github.com/wieslawsoltes/Svg.Skia) provides a more complete SVG renderer built on top of SkiaSharp APIs with better SVG specification compliance than the built-in SKSvg.",
          "category": "workaround",
          "confidence": 0.7,
          "effort": "cost/s",
          "validated": "untested"
        }
      ],
      "recommendedProposal": "Investigate Skia SVG DOM changes between milestones",
      "recommendedReason": "Understanding the exact root cause in the Skia version bump is prerequisite to any targeted fix or mitigation."
    }
  },
  "output": {
    "actionability": {
      "suggestedAction": "needs-investigation",
      "confidence": 0.82,
      "reason": "Clear regression with screenshots and reproduction repository. Root cause likely in upstream Skia SVG DOM changes between Skia milestones used in 2.80.4 vs 2.88.x. Needs deeper code investigation to confirm.",
      "suggestedReproPlatform": "linux"
    },
    "actions": [
      {
        "type": "update-labels",
        "description": "Apply type/bug, area/SkiaSharp, platform, backend, and compatibility tenet labels",
        "risk": "low",
        "confidence": 0.9,
        "labels": [
          "type/bug",
          "area/SkiaSharp",
          "os/Android",
          "os/iOS",
          "backend/SVG",
          "tenet/compatibility"
        ]
      },
      {
        "type": "add-comment",
        "description": "Post analysis noting the regression and requesting minimal SVG reproduction and version confirmation",
        "risk": "medium",
        "confidence": 0.8,
        "comment": "Thank you for the detailed report and reproduction repository.\n\nThis appears to be a regression in SVG rendering between SkiaSharp 2.80.4 and 2.88.x — specifically affecting SVG files with multiple `<path>` elements.\n\nTo help narrow down the root cause:\n1. Are you using the `SKSvg` class from `SkiaSharp.Extended.Svg`, or a different approach to load the SVG?\n2. Could you share a minimal SVG file that demonstrates the issue?\n3. Does the problem reproduce with the latest SkiaSharp 3.x release?\n\nAs a short-term workaround, you could consider [Svg.Skia](https://github.com/wieslawsoltes/Svg.Skia), a third-party SVG renderer built on SkiaSharp with broader SVG specification support."
      }
    ]
  }
}
```

</details>
