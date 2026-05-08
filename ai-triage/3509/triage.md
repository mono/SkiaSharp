# Issue Triage Report — #3509

| Field | Value |
|-------|-------|
| Repository | mono/SkiaSharp |
| Analyzed | 2026-04-21T18:16:13Z |
| Type | type/bug (0.65 (65%)) |
| Area | area/SkiaSharp (0.55 (55%)) |
| Suggested action | needs-info (0.92 (92%)) |

**Issue Summary:** Reporter claims images are not appearing on screen after upgrading from SkiaSharp 2.88.9 to 3.116.0, but provides no reproduction code, no expected/actual behavior, and contradictory platform information (Windows IDE, macOS platform).

**Analysis:** Issue is essentially a blank template submission with a vague description. No code, no expected behavior, no actual behavior, no logs, and a contradictory platform combination (Windows IDE / macOS platform). Cannot classify or investigate without a minimal reproduction case.

**Recommendations:** **needs-info** — Issue contains only template placeholders. No code, no expected/actual behavior, no logs. Cannot investigate or reproduce without a minimal reproduction case.

---

## Classification

| Field | Value |
|-------|-------|
| Type | type/bug |
| Area | area/SkiaSharp |
| Platforms | os/macOS |
| Backends | — |
| Tenets | — |
| Partner | — |
| Current labels | type/bug |

## Evidence

### Reproduction

**Environment:** SkiaSharp 3.116.0, Visual Studio (Windows), Platform: macOS

### Bug Signals

| Field | Value |
|-------|-------|
| Severity | medium |
| Regression claimed | True |
| Error type | wrong-output |
| Error message | — |
| Repro quality | none |
| Target frameworks | — |

### Version Analysis

| Field | Value |
|-------|-------|
| Mentioned versions | 3.116.0, 2.88.9 |
| Worked in | 2.88.9 |
| Broke in | 3.116.0 |
| Current relevance | unknown |
| Relevance reason | No reproduction code provided — cannot determine if the issue is real or a usage error. |

### Regression

| Field | Value |
|-------|-------|
| Is regression | True |
| Confidence | 0.40 (40%) |
| Reason | Reporter states last known good version is 2.88.9 and current broken version is 3.116.0. However, no actual repro code is provided so this cannot be verified. |
| Worked in version | 2.88.9 |
| Broke in version | 3.116.0 |

## Analysis

### Technical Summary

Issue is essentially a blank template submission with a vague description. No code, no expected behavior, no actual behavior, no logs, and a contradictory platform combination (Windows IDE / macOS platform). Cannot classify or investigate without a minimal reproduction case.

### Rationale

The issue body contains only template placeholders — code section has `// some C# code here`, expected/actual behavior fields show 'No response', no logs or screenshots. The only actionable signals are a regression version claim (2.88.9 → 3.116.0) and a vague 'images not appearing' description. DrawImage/DrawBitmap APIs exist in SKCanvas and are not known to have breaking changes in 3.116.0. Needs-info is the correct action per the empty-template heuristic.

### Key Signals

- "I have some code that puts things on the screen, but nothing appears." — **issue body** (Vague description — no API names, no code, no error messages.)
- "3.116.0 (Current) / 2.88.9 (Previous)" — **issue body** (Regression claim between major versions, but unverified without repro code.)
- "IDE: Visual Studio (Windows) / Platform: macOS" — **issue body** (Contradictory platform information — may indicate confusion about the template or cross-platform development scenario.)

### Code Investigation

| File | Lines | Relevance | Finding |
|------|-------|-----------|---------|
| `binding/SkiaSharp/SKCanvas.cs` | — | context | DrawImage and DrawBitmap overloads are present and public. No obvious breaking changes visible in the public API surface for image drawing. The API accepts SKImage, SKBitmap, and SKPoint/SKRect parameters consistently across both 2.x and 3.x series. |

### Next Questions

- What is the actual code being used to draw images?
- What platform/framework is being targeted (MAUI, WPF, Blazor, etc.)?
- Is there a NullReferenceException, silent failure, or something else?
- Is the SKCanvas being used inside a PaintSurface callback or standalone surface?
- Does the issue reproduce on both Windows and macOS, or only one?

### Resolution Proposals

**Hypothesis:** Cannot determine root cause without reproduction code. Most common causes of 'nothing appears' after a version upgrade include: incorrect usage of the new SKSamplingOptions parameter introduced in 3.x, paint/canvas disposal ordering changes, or missing InvalidateSurface() call.

1. **Request minimal reproduction case** — investigation, confidence 0.95 (95%), cost/xs, validated=untested
   - Ask reporter to provide a minimal self-contained code sample showing the image drawing code, the expected result, and the actual result.

**Recommended proposal:** Request minimal reproduction case

**Why:** Cannot proceed without actual code. The template is empty and there is nothing to investigate.

## Recommendations

### Actionability

| Field | Value |
|-------|-------|
| Suggested action | needs-info |
| Confidence | 0.92 (92%) |
| Reason | Issue contains only template placeholders. No code, no expected/actual behavior, no logs. Cannot investigate or reproduce without a minimal reproduction case. |
| Suggested repro platform | linux |

### Missing Info

- Actual C# code demonstrating the drawing that is not working
- Expected vs actual behavior (what should appear vs what actually appears)
- Target framework (net8.0-ios, net8.0-android, net8.0-maccatalyst, etc.)
- Whether the issue is a silent failure, exception, or something else
- Clarification on platform — Windows or macOS (IDE says Windows, Platform says macOS)

### Automatable Actions

| Type | Risk | Confidence | Description | Details |
|------|------|------------|-------------|---------|
| update-labels | low | 0.80 (80%) | Keep type/bug, add area/SkiaSharp and os/macOS labels | labels=type/bug, area/SkiaSharp, os/macOS |
| add-comment | medium | 0.92 (92%) | Request minimal repro code and clarify platform information | — |

**Comment draft for `add-comment`:**

```markdown
Thanks for the report.

To investigate the image rendering issue, we need a bit more information:

1. **Code**: A minimal C# snippet showing how you're loading and drawing the image (e.g., how you're using `DrawImage`, `DrawBitmap`, or `SKCanvas` in your `PaintSurface` handler).
2. **Expected vs actual**: What should appear, and what actually happens — is the canvas entirely blank, is only some content missing, or is there an exception?
3. **Target framework**: Which platform/framework are you building for (e.g., `net9.0-maccatalyst`, `net9.0-ios`, WPF, etc.)?
4. **Platform clarification**: The form shows Visual Studio (Windows) as IDE but macOS as the platform — are you running on a Mac or Windows machine?

One common cause of content disappearing after upgrading from 2.x to 3.x is that some APIs now take an explicit `SKSamplingOptions` parameter. If you were previously relying on the default filter mode, double-check which `DrawImage`/`DrawBitmap` overload you're calling.

A minimal project link or a short self-contained repro will help us reproduce this quickly.
```

<details>
<summary>Raw JSON</summary>

```json
{
  "meta": {
    "schemaVersion": "1.0",
    "number": 3509,
    "repo": "mono/SkiaSharp",
    "analyzedAt": "2026-04-21T18:16:13Z",
    "currentLabels": [
      "type/bug"
    ]
  },
  "summary": "Reporter claims images are not appearing on screen after upgrading from SkiaSharp 2.88.9 to 3.116.0, but provides no reproduction code, no expected/actual behavior, and contradictory platform information (Windows IDE, macOS platform).",
  "classification": {
    "type": {
      "value": "type/bug",
      "confidence": 0.65
    },
    "area": {
      "value": "area/SkiaSharp",
      "confidence": 0.55
    },
    "platforms": [
      "os/macOS"
    ]
  },
  "evidence": {
    "bugSignals": {
      "severity": "medium",
      "regressionClaimed": true,
      "errorType": "wrong-output",
      "reproQuality": "none",
      "targetFrameworks": []
    },
    "reproEvidence": {
      "environmentDetails": "SkiaSharp 3.116.0, Visual Studio (Windows), Platform: macOS"
    },
    "versionAnalysis": {
      "mentionedVersions": [
        "3.116.0",
        "2.88.9"
      ],
      "workedIn": "2.88.9",
      "brokeIn": "3.116.0",
      "currentRelevance": "unknown",
      "relevanceReason": "No reproduction code provided — cannot determine if the issue is real or a usage error."
    },
    "regression": {
      "isRegression": true,
      "confidence": 0.4,
      "reason": "Reporter states last known good version is 2.88.9 and current broken version is 3.116.0. However, no actual repro code is provided so this cannot be verified.",
      "workedInVersion": "2.88.9",
      "brokeInVersion": "3.116.0"
    }
  },
  "analysis": {
    "summary": "Issue is essentially a blank template submission with a vague description. No code, no expected behavior, no actual behavior, no logs, and a contradictory platform combination (Windows IDE / macOS platform). Cannot classify or investigate without a minimal reproduction case.",
    "rationale": "The issue body contains only template placeholders — code section has `// some C# code here`, expected/actual behavior fields show 'No response', no logs or screenshots. The only actionable signals are a regression version claim (2.88.9 → 3.116.0) and a vague 'images not appearing' description. DrawImage/DrawBitmap APIs exist in SKCanvas and are not known to have breaking changes in 3.116.0. Needs-info is the correct action per the empty-template heuristic.",
    "keySignals": [
      {
        "text": "I have some code that puts things on the screen, but nothing appears.",
        "source": "issue body",
        "interpretation": "Vague description — no API names, no code, no error messages."
      },
      {
        "text": "3.116.0 (Current) / 2.88.9 (Previous)",
        "source": "issue body",
        "interpretation": "Regression claim between major versions, but unverified without repro code."
      },
      {
        "text": "IDE: Visual Studio (Windows) / Platform: macOS",
        "source": "issue body",
        "interpretation": "Contradictory platform information — may indicate confusion about the template or cross-platform development scenario."
      }
    ],
    "codeInvestigation": [
      {
        "file": "binding/SkiaSharp/SKCanvas.cs",
        "finding": "DrawImage and DrawBitmap overloads are present and public. No obvious breaking changes visible in the public API surface for image drawing. The API accepts SKImage, SKBitmap, and SKPoint/SKRect parameters consistently across both 2.x and 3.x series.",
        "relevance": "context"
      }
    ],
    "nextQuestions": [
      "What is the actual code being used to draw images?",
      "What platform/framework is being targeted (MAUI, WPF, Blazor, etc.)?",
      "Is there a NullReferenceException, silent failure, or something else?",
      "Is the SKCanvas being used inside a PaintSurface callback or standalone surface?",
      "Does the issue reproduce on both Windows and macOS, or only one?"
    ],
    "resolution": {
      "hypothesis": "Cannot determine root cause without reproduction code. Most common causes of 'nothing appears' after a version upgrade include: incorrect usage of the new SKSamplingOptions parameter introduced in 3.x, paint/canvas disposal ordering changes, or missing InvalidateSurface() call.",
      "proposals": [
        {
          "title": "Request minimal reproduction case",
          "description": "Ask reporter to provide a minimal self-contained code sample showing the image drawing code, the expected result, and the actual result.",
          "category": "investigation",
          "confidence": 0.95,
          "effort": "cost/xs",
          "validated": "untested"
        }
      ],
      "recommendedProposal": "Request minimal reproduction case",
      "recommendedReason": "Cannot proceed without actual code. The template is empty and there is nothing to investigate."
    }
  },
  "output": {
    "actionability": {
      "suggestedAction": "needs-info",
      "confidence": 0.92,
      "reason": "Issue contains only template placeholders. No code, no expected/actual behavior, no logs. Cannot investigate or reproduce without a minimal reproduction case.",
      "suggestedReproPlatform": "linux"
    },
    "missingInfo": [
      "Actual C# code demonstrating the drawing that is not working",
      "Expected vs actual behavior (what should appear vs what actually appears)",
      "Target framework (net8.0-ios, net8.0-android, net8.0-maccatalyst, etc.)",
      "Whether the issue is a silent failure, exception, or something else",
      "Clarification on platform — Windows or macOS (IDE says Windows, Platform says macOS)"
    ],
    "actions": [
      {
        "type": "update-labels",
        "description": "Keep type/bug, add area/SkiaSharp and os/macOS labels",
        "risk": "low",
        "confidence": 0.8,
        "labels": [
          "type/bug",
          "area/SkiaSharp",
          "os/macOS"
        ]
      },
      {
        "type": "add-comment",
        "description": "Request minimal repro code and clarify platform information",
        "risk": "medium",
        "confidence": 0.92,
        "comment": "Thanks for the report.\n\nTo investigate the image rendering issue, we need a bit more information:\n\n1. **Code**: A minimal C# snippet showing how you're loading and drawing the image (e.g., how you're using `DrawImage`, `DrawBitmap`, or `SKCanvas` in your `PaintSurface` handler).\n2. **Expected vs actual**: What should appear, and what actually happens — is the canvas entirely blank, is only some content missing, or is there an exception?\n3. **Target framework**: Which platform/framework are you building for (e.g., `net9.0-maccatalyst`, `net9.0-ios`, WPF, etc.)?\n4. **Platform clarification**: The form shows Visual Studio (Windows) as IDE but macOS as the platform — are you running on a Mac or Windows machine?\n\nOne common cause of content disappearing after upgrading from 2.x to 3.x is that some APIs now take an explicit `SKSamplingOptions` parameter. If you were previously relying on the default filter mode, double-check which `DrawImage`/`DrawBitmap` overload you're calling.\n\nA minimal project link or a short self-contained repro will help us reproduce this quickly."
      }
    ]
  }
}
```

</details>
