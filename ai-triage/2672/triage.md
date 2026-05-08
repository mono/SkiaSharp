# Issue Triage Report — #2672

| Field | Value |
|-------|-------|
| Repository | mono/SkiaSharp |
| Analyzed | 2026-04-30T05:31:11Z |
| Type | type/bug (0.98 (98%)) |
| Area | area/SkiaSharp (0.95 (95%)) |
| Suggested action | close-as-duplicate (0.95 (95%)) |

**Issue Summary:** Reporter encounters an AccessViolationException crash in SKCodec.GetScaledDimensions on Windows with SkiaSharp 2.88.3, and self-identifies the issue as a duplicate of #2645.

**Analysis:** AccessViolationException crash in SKCodec.GetScaledDimensions on certain images. The reporter self-identified this as a duplicate of #2645, which reported the same crash between 2.88.5 and 2.88.6. The C# code at SKCodec.cs:42-47 calls sk_codec_get_scaled_dimensions directly via P/Invoke with no null or validity guard.

**Recommendations:** **close-as-duplicate** — Reporter self-confirmed duplicate of #2645, which describes the same AccessViolationException in the same API.

---

## Classification

| Field | Value |
|-------|-------|
| Type | type/bug |
| Area | area/SkiaSharp |
| Platforms | os/Windows-Classic |
| Backends | — |
| Tenets | tenet/reliability |
| Partner | — |
| Current labels | type/bug |

## Evidence

### Reproduction

1. Load an image file into a MemoryStream
2. Create SKManagedStream from MemoryStream
3. Create SKData from SKManagedStream
4. Create SKCodec from SKData
5. Call codec.GetScaledDimensions(scale) on certain images

**Environment:** Windows, Visual Studio, SkiaSharp 2.88.3

**Related issues:** #2645

**Repository links:**
- https://github.com/mono/SkiaSharp/issues/2645 — Duplicate: same AccessViolationException in SKCodec.GetScaledDimensions, closed

### Bug Signals

| Field | Value |
|-------|-------|
| Severity | high |
| Regression claimed | True |
| Error type | crash |
| Error message | AccessViolationException in SKCodec.GetScaledDimensions |
| Repro quality | partial |
| Target frameworks | net6.0 |

### Version Analysis

| Field | Value |
|-------|-------|
| Mentioned versions | 2.88.3, 2.88.2 |
| Worked in | 2.88.2 |
| Broke in | 2.88.3 |
| Current relevance | unknown |
| Relevance reason | Issue #2645 (duplicate) was closed; unclear if fix was applied to 2.88.3 line. |

### Regression

| Field | Value |
|-------|-------|
| Is regression | True |
| Confidence | 0.85 (85%) |
| Reason | Reporter states last known good version is 2.88.2, crash occurs in 2.88.3. |
| Worked in version | 2.88.2 |
| Broke in version | 2.88.3 |

## Analysis

### Technical Summary

AccessViolationException crash in SKCodec.GetScaledDimensions on certain images. The reporter self-identified this as a duplicate of #2645, which reported the same crash between 2.88.5 and 2.88.6. The C# code at SKCodec.cs:42-47 calls sk_codec_get_scaled_dimensions directly via P/Invoke with no null or validity guard.

### Rationale

Reporter explicitly states this is a duplicate of #2645 (same AccessViolationException in GetScaledDimensions). Both issues report a regression between minor 2.88.x versions. The crash is in native code accessed via P/Invoke, suggesting a native-side bug introduced in Skia.

### Key Signals

- "I got crash in SKCodec > GetScaledDimensions with AccessViolation exception" — **issue body** (Native memory access violation during codec scaled dimensions query.)
- "Sorry I just see that its a duplicate of https://github.com/mono/SkiaSharp/issues/2645" — **comment #1** (Reporter self-confirmed duplicate.)
- "Last Known Good Version: 2.88.2" — **issue body** (Regression between 2.88.2 and 2.88.3.)

### Code Investigation

| File | Lines | Relevance | Finding |
|------|-------|-----------|---------|
| `binding/SkiaSharp/SKCodec.cs` | 42-47 | direct | GetScaledDimensions calls sk_codec_get_scaled_dimensions via P/Invoke with no null/validity guard on Handle. If the codec was created from corrupted or unsupported image data, the native call may dereference an invalid pointer. |

**Error fingerprint:** `AccessViolationException::SKCodec::GetScaledDimensions`

### Next Questions

- Was #2645 actually fixed in a subsequent release?
- Does the crash occur on specific image formats or all images?
- Is the codec handle valid when GetScaledDimensions is called (i.e., did SKCodec.Create return non-null)?

## Recommendations

### Actionability

| Field | Value |
|-------|-------|
| Suggested action | close-as-duplicate |
| Confidence | 0.95 (95%) |
| Reason | Reporter self-confirmed duplicate of #2645, which describes the same AccessViolationException in the same API. |
| Suggested repro platform | windows |

### Automatable Actions

| Type | Risk | Confidence | Description | Details |
|------|------|------------|-------------|---------|
| update-labels | low | 0.95 (95%) | Apply bug, core SkiaSharp, Windows platform labels | labels=type/bug, area/SkiaSharp, os/Windows-Classic, tenet/reliability |
| link-duplicate | medium | 0.95 (95%) | Mark as duplicate of #2645 | linkedIssue=#2645 |
| close-issue | medium | 0.90 (90%) | Close as duplicate of #2645 | stateReason=not_planned |

<details>
<summary>Raw JSON</summary>

```json
{
  "meta": {
    "schemaVersion": "1.0",
    "number": 2672,
    "repo": "mono/SkiaSharp",
    "analyzedAt": "2026-04-30T05:31:11Z",
    "currentLabels": [
      "type/bug"
    ]
  },
  "summary": "Reporter encounters an AccessViolationException crash in SKCodec.GetScaledDimensions on Windows with SkiaSharp 2.88.3, and self-identifies the issue as a duplicate of #2645.",
  "classification": {
    "type": {
      "value": "type/bug",
      "confidence": 0.98
    },
    "area": {
      "value": "area/SkiaSharp",
      "confidence": 0.95
    },
    "platforms": [
      "os/Windows-Classic"
    ],
    "tenets": [
      "tenet/reliability"
    ]
  },
  "evidence": {
    "bugSignals": {
      "severity": "high",
      "regressionClaimed": true,
      "errorType": "crash",
      "errorMessage": "AccessViolationException in SKCodec.GetScaledDimensions",
      "reproQuality": "partial",
      "targetFrameworks": [
        "net6.0"
      ]
    },
    "reproEvidence": {
      "stepsToReproduce": [
        "Load an image file into a MemoryStream",
        "Create SKManagedStream from MemoryStream",
        "Create SKData from SKManagedStream",
        "Create SKCodec from SKData",
        "Call codec.GetScaledDimensions(scale) on certain images"
      ],
      "environmentDetails": "Windows, Visual Studio, SkiaSharp 2.88.3",
      "relatedIssues": [
        2645
      ],
      "repoLinks": [
        {
          "url": "https://github.com/mono/SkiaSharp/issues/2645",
          "description": "Duplicate: same AccessViolationException in SKCodec.GetScaledDimensions, closed"
        }
      ]
    },
    "versionAnalysis": {
      "mentionedVersions": [
        "2.88.3",
        "2.88.2"
      ],
      "workedIn": "2.88.2",
      "brokeIn": "2.88.3",
      "currentRelevance": "unknown",
      "relevanceReason": "Issue #2645 (duplicate) was closed; unclear if fix was applied to 2.88.3 line."
    },
    "regression": {
      "isRegression": true,
      "confidence": 0.85,
      "reason": "Reporter states last known good version is 2.88.2, crash occurs in 2.88.3.",
      "workedInVersion": "2.88.2",
      "brokeInVersion": "2.88.3"
    }
  },
  "analysis": {
    "summary": "AccessViolationException crash in SKCodec.GetScaledDimensions on certain images. The reporter self-identified this as a duplicate of #2645, which reported the same crash between 2.88.5 and 2.88.6. The C# code at SKCodec.cs:42-47 calls sk_codec_get_scaled_dimensions directly via P/Invoke with no null or validity guard.",
    "rationale": "Reporter explicitly states this is a duplicate of #2645 (same AccessViolationException in GetScaledDimensions). Both issues report a regression between minor 2.88.x versions. The crash is in native code accessed via P/Invoke, suggesting a native-side bug introduced in Skia.",
    "keySignals": [
      {
        "text": "I got crash in SKCodec > GetScaledDimensions with AccessViolation exception",
        "source": "issue body",
        "interpretation": "Native memory access violation during codec scaled dimensions query."
      },
      {
        "text": "Sorry I just see that its a duplicate of https://github.com/mono/SkiaSharp/issues/2645",
        "source": "comment #1",
        "interpretation": "Reporter self-confirmed duplicate."
      },
      {
        "text": "Last Known Good Version: 2.88.2",
        "source": "issue body",
        "interpretation": "Regression between 2.88.2 and 2.88.3."
      }
    ],
    "codeInvestigation": [
      {
        "file": "binding/SkiaSharp/SKCodec.cs",
        "lines": "42-47",
        "finding": "GetScaledDimensions calls sk_codec_get_scaled_dimensions via P/Invoke with no null/validity guard on Handle. If the codec was created from corrupted or unsupported image data, the native call may dereference an invalid pointer.",
        "relevance": "direct"
      }
    ],
    "nextQuestions": [
      "Was #2645 actually fixed in a subsequent release?",
      "Does the crash occur on specific image formats or all images?",
      "Is the codec handle valid when GetScaledDimensions is called (i.e., did SKCodec.Create return non-null)?"
    ],
    "errorFingerprint": "AccessViolationException::SKCodec::GetScaledDimensions"
  },
  "output": {
    "actionability": {
      "suggestedAction": "close-as-duplicate",
      "confidence": 0.95,
      "reason": "Reporter self-confirmed duplicate of #2645, which describes the same AccessViolationException in the same API.",
      "suggestedReproPlatform": "windows"
    },
    "actions": [
      {
        "type": "update-labels",
        "description": "Apply bug, core SkiaSharp, Windows platform labels",
        "risk": "low",
        "confidence": 0.95,
        "labels": [
          "type/bug",
          "area/SkiaSharp",
          "os/Windows-Classic",
          "tenet/reliability"
        ]
      },
      {
        "type": "link-duplicate",
        "description": "Mark as duplicate of #2645",
        "risk": "medium",
        "confidence": 0.95,
        "linkedIssue": 2645
      },
      {
        "type": "close-issue",
        "description": "Close as duplicate of #2645",
        "risk": "medium",
        "confidence": 0.9,
        "stateReason": "not_planned"
      }
    ]
  }
}
```

</details>
