# Issue Triage Report — #1866

| Field | Value |
|-------|-------|
| Repository | mono/SkiaSharp |
| Analyzed | 2026-06-23T05:33:51Z |
| Type | type/bug (0.65 (65%)) |
| Area | area/SkiaSharp.Views.Forms (0.60 (60%)) |
| Suggested action | needs-info (0.85 (85%)) |

**Issue Summary:** Reporter observes application slowness and throttling when Hot Reload is enabled in a Xamarin project using a 'SkiaPulse' element after upgrading to version 5.0.0.2012; disabling Hot Reload resolves the performance issue.

**Analysis:** Reporter describes throttling and slowness in a Xamarin app when Hot Reload is active and a 'SkiaPulse' element is rendered. The issue disappears when Hot Reload is disabled. No repro code, no stack traces, no platform, and no clarification of what SkiaPulse is were provided.

**Recommendations:** **needs-info** — Critical information is missing: no platform specified, no repro code, and the identity of 'SkiaPulse' is unknown. The version '5.0.0.2012' is ambiguous (Xamarin.Forms or SkiaSharp). Cannot investigate without these details.

---

## Classification

| Field | Value |
|-------|-------|
| Type | type/bug |
| Area | area/SkiaSharp.Views.Forms |
| Platforms | — |
| Backends | — |
| Tenets | tenet/performance |
| Partner | — |

## Evidence

### Reproduction

**Environment:** Xamarin project, version 5.0.0.2012 (Xamarin or SkiaSharp unclear), Hot Reload enabled in IDE

### Bug Signals

| Field | Value |
|-------|-------|
| Severity | medium |
| Regression claimed | True |
| Error type | performance |
| Error message | — |
| Repro quality | none |
| Target frameworks | — |

### Version Analysis

| Field | Value |
|-------|-------|
| Mentioned versions | 4.7, 5.0.0.2012 |
| Worked in | — |
| Broke in | — |
| Current relevance | unknown |
| Relevance reason | The reporter mentions upgrading from 4.7 to 5.0.0.2012 but it is unclear whether this refers to the Xamarin.Forms version or the SkiaSharp NuGet version. No platform specified. |

### Regression

| Field | Value |
|-------|-------|
| Is regression | True |
| Confidence | 0.55 (55%) |
| Reason | Reporter states it worked at version 4.7 and broke after upgrading to 5.0.0.2012. |
| Worked in version | 4.7 |
| Broke in version | 5.0.0.2012 |

## Analysis

### Technical Summary

Reporter describes throttling and slowness in a Xamarin app when Hot Reload is active and a 'SkiaPulse' element is rendered. The issue disappears when Hot Reload is disabled. No repro code, no stack traces, no platform, and no clarification of what SkiaPulse is were provided.

### Rationale

Classified as type/bug because a clear performance regression with a specific trigger (Hot Reload) is described. Area is SkiaSharp.Views.Forms given the Xamarin.Forms context, though note this package is now deprecated in favour of MAUI. Confidence is reduced because 'SkiaPulse' is not a SkiaSharp component and may be a third-party library, so the root cause may be external. The suggestedAction is needs-info because critical details — platform, SkiaPulse identity, version disambiguation, and a minimal repro — are absent.

### Key Signals

- "if i ve enabled hotreload when i render page with skiapulse element all app go slow and has trotthling issues" — **issue body** (Hot Reload triggers view lifecycle events (attach/detach/property re-application) that may cause rapid repeated invalidations in SkiaSharp views, leading to a rendering storm.)
- "I tried to update to 5.0.0.2012 Xamarin version" — **issue body** (Regression introduced at a version boundary — either a Xamarin.Forms 5.x change in the Hot Reload pipeline or a SkiaSharp 5.x rendering change.)
- "If i disable hot reload all go well" — **issue body** (The performance issue is exclusively tied to Hot Reload being active, confirming the root cause is the interaction between Hot Reload lifecycle events and SkiaSharp rendering.)

### Code Investigation

| File | Lines | Relevance | Finding |
|------|-------|-----------|---------|
| `source/SkiaSharp.Views/SkiaSharp.Views/Platform/Android/SKCanvasView.cs` | 119-147 | related | OnAttachedToWindow calls Invalidate() after UpdateCanvasSize(), and the IgnorePixelScaling setter also calls Invalidate(). Hot Reload in Xamarin/Visual Studio triggers view re-attachment and property re-application, which would repeatedly call Invalidate() without rate-limiting. This pattern could cause continuous redraws for any SkiaSharp-based view, consistent with the reported throttling. |

### Next Questions

- Which platform(s) are affected: Android, iOS, or both?
- What is SkiaPulse — is it a third-party NuGet package or a custom SkiaSharp view? Can you share a link or source?
- Does the same slowness occur with a plain SKCanvasView in place of SkiaPulse?
- Is the version 5.0.0.2012 referring to Xamarin.Forms or SkiaSharp (or both)?
- Is this issue reproducible in MAUI (the successor to Xamarin.Forms)?

### Resolution Proposals

**Hypothesis:** Hot Reload triggers rapid view lifecycle events (attach/detach/property re-application) which cause SkiaSharp views to invalidate and redraw continuously. If SkiaPulse has its own animation or redraw loop, the Hot Reload events may compound with it to create a cascading redraw cycle.

1. **Disable Hot Reload for the affected page as a development workaround** — workaround, confidence 0.80 (80%), cost/xs, validated=untested
   - Disable Xamarin Hot Reload entirely or temporarily navigate away before triggering a Hot Reload update. This is a development-time workaround until the root cause is identified.
2. **Reproduce with a plain SKCanvasView to isolate SkiaPulse vs SkiaSharp** — investigation, confidence 0.90 (90%), cost/s, validated=untested
   - Replace the SkiaPulse element with a plain SKCanvasView in a test page and verify if the throttling still occurs with Hot Reload. This isolates whether the root cause is in SkiaSharp.Views.Forms or in SkiaPulse's own rendering logic.

**Recommended proposal:** Reproduce with a plain SKCanvasView to isolate SkiaPulse vs SkiaSharp

**Why:** Isolating the issue to SkiaSharp vs SkiaPulse is the highest-value first step before any fix can be proposed.

## Recommendations

### Actionability

| Field | Value |
|-------|-------|
| Suggested action | needs-info |
| Confidence | 0.85 (85%) |
| Reason | Critical information is missing: no platform specified, no repro code, and the identity of 'SkiaPulse' is unknown. The version '5.0.0.2012' is ambiguous (Xamarin.Forms or SkiaSharp). Cannot investigate without these details. |
| Suggested repro platform | linux |

### Missing Info

- Which platform(s) are affected: Android, iOS, or both?
- What is SkiaPulse — a third-party NuGet package, a custom SkiaSharp view?
- Does the issue occur with a plain SKCanvasView instead of SkiaPulse?
- Is version 5.0.0.2012 referring to Xamarin.Forms or SkiaSharp?
- Can you provide a minimal reproduction project or steps?

### Automatable Actions

| Type | Risk | Confidence | Description | Details |
|------|------|------------|-------------|---------|
| update-labels | low | 0.75 (75%) | Apply bug, views-forms, and performance tenet labels | labels=type/bug, area/SkiaSharp.Views.Forms, tenet/performance |
| add-comment | medium | 0.85 (85%) | Request missing information from reporter | — |

**Comment draft for `add-comment`:**

```markdown
Thank you for filing this issue!

To help investigate this, we need a few more details:

1. **Platform**: Which platform(s) are you seeing this on — Android, iOS, or both?
2. **SkiaPulse**: What is 'SkiaPulse'? Is it a third-party NuGet package or a custom SkiaSharp-based view? Could you share a link or its source?
3. **Isolation test**: Does the same slowness occur if you replace SkiaPulse with a plain `SKCanvasView`? This would help isolate whether the issue is in SkiaSharp itself or in SkiaPulse's rendering logic.
4. **Version clarification**: Is `5.0.0.2012` the Xamarin.Forms version, the SkiaSharp version, or both? Please provide both NuGet package versions.
5. **Minimal repro**: If possible, a minimal reproduction project would greatly speed up investigation.

Also note that Xamarin.Forms is now superseded by .NET MAUI. If you are able to reproduce this in a MAUI project, that would be the currently supported path for investigation.
```

<details>
<summary>Raw JSON</summary>

```json
{
  "meta": {
    "schemaVersion": "1.0",
    "number": 1866,
    "repo": "mono/SkiaSharp",
    "analyzedAt": "2026-06-23T05:33:51Z"
  },
  "summary": "Reporter observes application slowness and throttling when Hot Reload is enabled in a Xamarin project using a 'SkiaPulse' element after upgrading to version 5.0.0.2012; disabling Hot Reload resolves the performance issue.",
  "classification": {
    "type": {
      "value": "type/bug",
      "confidence": 0.65
    },
    "area": {
      "value": "area/SkiaSharp.Views.Forms",
      "confidence": 0.6
    },
    "tenets": [
      "tenet/performance"
    ]
  },
  "evidence": {
    "bugSignals": {
      "severity": "medium",
      "regressionClaimed": true,
      "errorType": "performance",
      "reproQuality": "none"
    },
    "reproEvidence": {
      "environmentDetails": "Xamarin project, version 5.0.0.2012 (Xamarin or SkiaSharp unclear), Hot Reload enabled in IDE"
    },
    "versionAnalysis": {
      "mentionedVersions": [
        "4.7",
        "5.0.0.2012"
      ],
      "currentRelevance": "unknown",
      "relevanceReason": "The reporter mentions upgrading from 4.7 to 5.0.0.2012 but it is unclear whether this refers to the Xamarin.Forms version or the SkiaSharp NuGet version. No platform specified."
    },
    "regression": {
      "isRegression": true,
      "confidence": 0.55,
      "reason": "Reporter states it worked at version 4.7 and broke after upgrading to 5.0.0.2012.",
      "workedInVersion": "4.7",
      "brokeInVersion": "5.0.0.2012"
    }
  },
  "analysis": {
    "summary": "Reporter describes throttling and slowness in a Xamarin app when Hot Reload is active and a 'SkiaPulse' element is rendered. The issue disappears when Hot Reload is disabled. No repro code, no stack traces, no platform, and no clarification of what SkiaPulse is were provided.",
    "rationale": "Classified as type/bug because a clear performance regression with a specific trigger (Hot Reload) is described. Area is SkiaSharp.Views.Forms given the Xamarin.Forms context, though note this package is now deprecated in favour of MAUI. Confidence is reduced because 'SkiaPulse' is not a SkiaSharp component and may be a third-party library, so the root cause may be external. The suggestedAction is needs-info because critical details — platform, SkiaPulse identity, version disambiguation, and a minimal repro — are absent.",
    "keySignals": [
      {
        "text": "if i ve enabled hotreload when i render page with skiapulse element all app go slow and has trotthling issues",
        "source": "issue body",
        "interpretation": "Hot Reload triggers view lifecycle events (attach/detach/property re-application) that may cause rapid repeated invalidations in SkiaSharp views, leading to a rendering storm."
      },
      {
        "text": "I tried to update to 5.0.0.2012 Xamarin version",
        "source": "issue body",
        "interpretation": "Regression introduced at a version boundary — either a Xamarin.Forms 5.x change in the Hot Reload pipeline or a SkiaSharp 5.x rendering change."
      },
      {
        "text": "If i disable hot reload all go well",
        "source": "issue body",
        "interpretation": "The performance issue is exclusively tied to Hot Reload being active, confirming the root cause is the interaction between Hot Reload lifecycle events and SkiaSharp rendering."
      }
    ],
    "codeInvestigation": [
      {
        "file": "source/SkiaSharp.Views/SkiaSharp.Views/Platform/Android/SKCanvasView.cs",
        "lines": "119-147",
        "finding": "OnAttachedToWindow calls Invalidate() after UpdateCanvasSize(), and the IgnorePixelScaling setter also calls Invalidate(). Hot Reload in Xamarin/Visual Studio triggers view re-attachment and property re-application, which would repeatedly call Invalidate() without rate-limiting. This pattern could cause continuous redraws for any SkiaSharp-based view, consistent with the reported throttling.",
        "relevance": "related"
      }
    ],
    "nextQuestions": [
      "Which platform(s) are affected: Android, iOS, or both?",
      "What is SkiaPulse — is it a third-party NuGet package or a custom SkiaSharp view? Can you share a link or source?",
      "Does the same slowness occur with a plain SKCanvasView in place of SkiaPulse?",
      "Is the version 5.0.0.2012 referring to Xamarin.Forms or SkiaSharp (or both)?",
      "Is this issue reproducible in MAUI (the successor to Xamarin.Forms)?"
    ],
    "resolution": {
      "hypothesis": "Hot Reload triggers rapid view lifecycle events (attach/detach/property re-application) which cause SkiaSharp views to invalidate and redraw continuously. If SkiaPulse has its own animation or redraw loop, the Hot Reload events may compound with it to create a cascading redraw cycle.",
      "proposals": [
        {
          "title": "Disable Hot Reload for the affected page as a development workaround",
          "description": "Disable Xamarin Hot Reload entirely or temporarily navigate away before triggering a Hot Reload update. This is a development-time workaround until the root cause is identified.",
          "category": "workaround",
          "confidence": 0.8,
          "effort": "cost/xs",
          "validated": "untested"
        },
        {
          "title": "Reproduce with a plain SKCanvasView to isolate SkiaPulse vs SkiaSharp",
          "description": "Replace the SkiaPulse element with a plain SKCanvasView in a test page and verify if the throttling still occurs with Hot Reload. This isolates whether the root cause is in SkiaSharp.Views.Forms or in SkiaPulse's own rendering logic.",
          "category": "investigation",
          "confidence": 0.9,
          "effort": "cost/s",
          "validated": "untested"
        }
      ],
      "recommendedProposal": "Reproduce with a plain SKCanvasView to isolate SkiaPulse vs SkiaSharp",
      "recommendedReason": "Isolating the issue to SkiaSharp vs SkiaPulse is the highest-value first step before any fix can be proposed."
    }
  },
  "output": {
    "actionability": {
      "suggestedAction": "needs-info",
      "confidence": 0.85,
      "reason": "Critical information is missing: no platform specified, no repro code, and the identity of 'SkiaPulse' is unknown. The version '5.0.0.2012' is ambiguous (Xamarin.Forms or SkiaSharp). Cannot investigate without these details.",
      "suggestedReproPlatform": "linux"
    },
    "missingInfo": [
      "Which platform(s) are affected: Android, iOS, or both?",
      "What is SkiaPulse — a third-party NuGet package, a custom SkiaSharp view?",
      "Does the issue occur with a plain SKCanvasView instead of SkiaPulse?",
      "Is version 5.0.0.2012 referring to Xamarin.Forms or SkiaSharp?",
      "Can you provide a minimal reproduction project or steps?"
    ],
    "actions": [
      {
        "type": "update-labels",
        "description": "Apply bug, views-forms, and performance tenet labels",
        "risk": "low",
        "confidence": 0.75,
        "labels": [
          "type/bug",
          "area/SkiaSharp.Views.Forms",
          "tenet/performance"
        ]
      },
      {
        "type": "add-comment",
        "description": "Request missing information from reporter",
        "risk": "medium",
        "confidence": 0.85,
        "comment": "Thank you for filing this issue!\n\nTo help investigate this, we need a few more details:\n\n1. **Platform**: Which platform(s) are you seeing this on — Android, iOS, or both?\n2. **SkiaPulse**: What is 'SkiaPulse'? Is it a third-party NuGet package or a custom SkiaSharp-based view? Could you share a link or its source?\n3. **Isolation test**: Does the same slowness occur if you replace SkiaPulse with a plain `SKCanvasView`? This would help isolate whether the issue is in SkiaSharp itself or in SkiaPulse's rendering logic.\n4. **Version clarification**: Is `5.0.0.2012` the Xamarin.Forms version, the SkiaSharp version, or both? Please provide both NuGet package versions.\n5. **Minimal repro**: If possible, a minimal reproduction project would greatly speed up investigation.\n\nAlso note that Xamarin.Forms is now superseded by .NET MAUI. If you are able to reproduce this in a MAUI project, that would be the currently supported path for investigation."
      }
    ]
  }
}
```

</details>
