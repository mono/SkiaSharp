# Issue Triage Report — #993

| Field | Value |
|-------|-------|
| Repository | mono/SkiaSharp |
| Analyzed | 2026-05-03T01:12:00Z |
| Type | type/enhancement (0.85 (85%)) |
| Area | area/SkiaSharp (0.95 (95%)) |
| Suggested action | keep-open (0.80 (80%)) |

**Issue Summary:** Reporter identifies exact float equality comparisons (== 1, == 0) in SKMatrix.cs and SKColor.cs that should use Utils.NearlyEqual for robustness against floating-point imprecision.

**Analysis:** SKMatrix.cs still uses exact float equality (== 1, == 0) in early-return guards in CreateScale, CreateTranslation, CreateSkew, and CreateScaleTranslation. Utils.NearlyEqual already exists in the codebase and is used in SKRoundRect.cs. The improvement would make these guards more robust to floating-point rounding errors. The SKColor.cs concern from the original report appears to have been addressed or is not present in the current codebase.

**Recommendations:** **keep-open** — Valid enhancement with clear implementation path. The fix is small, low-risk, and the infrastructure already exists. Keeping open for future contribution.

---

## Classification

| Field | Value |
|-------|-------|
| Type | type/enhancement |
| Area | area/SkiaSharp |
| Platforms | — |
| Backends | — |
| Tenets | tenet/reliability |
| Partner | — |

## Evidence

### Reproduction

**Code snippets:**

```csharp
if (x == 1 && y == 1) return Identity; // SKMatrix.CreateScale
```

```csharp
if (x == 0 && y == 0) return Identity; // SKMatrix.CreateTranslation
```

### Version Analysis

| Field | Value |
|-------|-------|
| Mentioned versions | — |
| Worked in | — |
| Broke in | — |
| Current relevance | likely |
| Relevance reason | Code investigation confirms the exact float equality patterns still exist in SKMatrix.cs at lines 124, 138, 150, 209, 223. |

## Analysis

### Technical Summary

SKMatrix.cs still uses exact float equality (== 1, == 0) in early-return guards in CreateScale, CreateTranslation, CreateSkew, and CreateScaleTranslation. Utils.NearlyEqual already exists in the codebase and is used in SKRoundRect.cs. The improvement would make these guards more robust to floating-point rounding errors. The SKColor.cs concern from the original report appears to have been addressed or is not present in the current codebase.

### Rationale

This is a code quality enhancement request, not a crash or incorrect output report. The reporter identifies real float-comparison patterns that could theoretically fail for near-identity values but does not report a concrete bug. The improvement would increase robustness. The Utils.NearlyEqual infrastructure exists and is already used elsewhere. The SKColor.cs concern is not confirmed in the current codebase.

### Key Signals

- "if (sx == 1 && sy == 1) — where sx and sy are floats" — **issue body** (Exact float equality can fail for values that are mathematically 1 but differ by a ULP after arithmetic operations.)
- "Utils.NearlyEqual should be used too" — **issue body** (Reporter is aware of the existing utility and requesting its broader application.)
- "The tolerance argument of Utils.NearlyEqual should be set to Utils.NearlyZero by default" — **issue body** (API ergonomics improvement request — a separate but related enhancement.)

### Code Investigation

| File | Lines | Relevance | Finding |
|------|-------|-----------|---------|
| `binding/SkiaSharp/SKMatrix.cs` | 124,138,150,209,223 | direct | Multiple static factory methods (CreateTranslation, CreateScale, CreateSkew, CreateScaleTranslation) use exact float equality (== 0, == 1) as early-return identity guards. These could miss values that are nearly 0 or 1 due to floating-point arithmetic. |
| `binding/SkiaSharp/Util.cs` | 15,42-43 | direct | Utils.NearlyZero (1/4096) and Utils.NearlyEqual(float a, float b, float tolerance) are already defined and available for use within the binding assembly. |
| `binding/SkiaSharp/SKRoundRect.cs` | 80,90-93 | related | CheckAllCornersCircular and related logic already use Utils.NearlyEqual and Utils.NearlyZero, demonstrating the intended pattern. |

### Resolution Proposals

**Hypothesis:** Replace exact float equality guards in SKMatrix.cs factory methods with Utils.NearlyEqual calls using Utils.NearlyZero tolerance.

1. **Use NearlyEqual in SKMatrix factory guards** — fix, confidence 0.80 (80%), cost/s, validated=untested
   - Replace == 0 and == 1 float guards in CreateTranslation, CreateScale, CreateSkew, CreateScaleTranslation with Utils.NearlyEqual(x, 0, Utils.NearlyZero) / Utils.NearlyEqual(x, 1, Utils.NearlyZero) patterns.
2. **Add default tolerance parameter to Utils.NearlyEqual** — fix, confidence 0.85 (85%), cost/xs, validated=untested
   - Add an overload Utils.NearlyEqual(float a, float b) that defaults tolerance to Utils.NearlyZero, reducing call-site verbosity.

**Recommended proposal:** Use NearlyEqual in SKMatrix factory guards

**Why:** Directly addresses the robustness concern raised by the reporter.

## Recommendations

### Actionability

| Field | Value |
|-------|-------|
| Suggested action | keep-open |
| Confidence | 0.80 (80%) |
| Reason | Valid enhancement with clear implementation path. The fix is small, low-risk, and the infrastructure already exists. Keeping open for future contribution. |
| Suggested repro platform | linux |

### Automatable Actions

| Type | Risk | Confidence | Description | Details |
|------|------|------------|-------------|---------|
| update-labels | low | 0.90 (90%) | Apply enhancement and core area labels | labels=type/enhancement, area/SkiaSharp, tenet/reliability |
| add-comment | medium | 0.82 (82%) | Acknowledge the valid enhancement and confirm the patterns still exist | — |

**Comment draft for `add-comment`:**

```markdown
Thanks for the suggestion! The exact float equality patterns (`== 1`, `== 0`) are still present in `SKMatrix.cs` factory methods (`CreateScale`, `CreateTranslation`, `CreateSkew`, `CreateScaleTranslation`). Using `Utils.NearlyEqual` with `Utils.NearlyZero` tolerance would make these guards more robust to floating-point rounding. `Utils.NearlyEqual` is already used in `SKRoundRect.cs`, so the pattern is established.

A PR implementing these two changes would be welcome:
1. Replace exact float guards in `SKMatrix.cs` with `Utils.NearlyEqual` calls.
2. Add a zero-tolerance default overload to `Utils.NearlyEqual` for ergonomics.
```

<details>
<summary>Raw JSON</summary>

```json
{
  "meta": {
    "schemaVersion": "1.0",
    "number": 993,
    "repo": "mono/SkiaSharp",
    "analyzedAt": "2026-05-03T01:12:00Z"
  },
  "summary": "Reporter identifies exact float equality comparisons (== 1, == 0) in SKMatrix.cs and SKColor.cs that should use Utils.NearlyEqual for robustness against floating-point imprecision.",
  "classification": {
    "type": {
      "value": "type/enhancement",
      "confidence": 0.85
    },
    "area": {
      "value": "area/SkiaSharp",
      "confidence": 0.95
    },
    "tenets": [
      "tenet/reliability"
    ]
  },
  "evidence": {
    "reproEvidence": {
      "codeSnippets": [
        "if (x == 1 && y == 1) return Identity; // SKMatrix.CreateScale",
        "if (x == 0 && y == 0) return Identity; // SKMatrix.CreateTranslation"
      ]
    },
    "versionAnalysis": {
      "mentionedVersions": [],
      "currentRelevance": "likely",
      "relevanceReason": "Code investigation confirms the exact float equality patterns still exist in SKMatrix.cs at lines 124, 138, 150, 209, 223."
    }
  },
  "analysis": {
    "summary": "SKMatrix.cs still uses exact float equality (== 1, == 0) in early-return guards in CreateScale, CreateTranslation, CreateSkew, and CreateScaleTranslation. Utils.NearlyEqual already exists in the codebase and is used in SKRoundRect.cs. The improvement would make these guards more robust to floating-point rounding errors. The SKColor.cs concern from the original report appears to have been addressed or is not present in the current codebase.",
    "codeInvestigation": [
      {
        "file": "binding/SkiaSharp/SKMatrix.cs",
        "lines": "124,138,150,209,223",
        "finding": "Multiple static factory methods (CreateTranslation, CreateScale, CreateSkew, CreateScaleTranslation) use exact float equality (== 0, == 1) as early-return identity guards. These could miss values that are nearly 0 or 1 due to floating-point arithmetic.",
        "relevance": "direct"
      },
      {
        "file": "binding/SkiaSharp/Util.cs",
        "lines": "15,42-43",
        "finding": "Utils.NearlyZero (1/4096) and Utils.NearlyEqual(float a, float b, float tolerance) are already defined and available for use within the binding assembly.",
        "relevance": "direct"
      },
      {
        "file": "binding/SkiaSharp/SKRoundRect.cs",
        "lines": "80,90-93",
        "finding": "CheckAllCornersCircular and related logic already use Utils.NearlyEqual and Utils.NearlyZero, demonstrating the intended pattern.",
        "relevance": "related"
      }
    ],
    "keySignals": [
      {
        "text": "if (sx == 1 && sy == 1) — where sx and sy are floats",
        "source": "issue body",
        "interpretation": "Exact float equality can fail for values that are mathematically 1 but differ by a ULP after arithmetic operations."
      },
      {
        "text": "Utils.NearlyEqual should be used too",
        "source": "issue body",
        "interpretation": "Reporter is aware of the existing utility and requesting its broader application."
      },
      {
        "text": "The tolerance argument of Utils.NearlyEqual should be set to Utils.NearlyZero by default",
        "source": "issue body",
        "interpretation": "API ergonomics improvement request — a separate but related enhancement."
      }
    ],
    "rationale": "This is a code quality enhancement request, not a crash or incorrect output report. The reporter identifies real float-comparison patterns that could theoretically fail for near-identity values but does not report a concrete bug. The improvement would increase robustness. The Utils.NearlyEqual infrastructure exists and is already used elsewhere. The SKColor.cs concern is not confirmed in the current codebase.",
    "resolution": {
      "hypothesis": "Replace exact float equality guards in SKMatrix.cs factory methods with Utils.NearlyEqual calls using Utils.NearlyZero tolerance.",
      "proposals": [
        {
          "title": "Use NearlyEqual in SKMatrix factory guards",
          "description": "Replace == 0 and == 1 float guards in CreateTranslation, CreateScale, CreateSkew, CreateScaleTranslation with Utils.NearlyEqual(x, 0, Utils.NearlyZero) / Utils.NearlyEqual(x, 1, Utils.NearlyZero) patterns.",
          "category": "fix",
          "confidence": 0.8,
          "effort": "cost/s",
          "validated": "untested"
        },
        {
          "title": "Add default tolerance parameter to Utils.NearlyEqual",
          "description": "Add an overload Utils.NearlyEqual(float a, float b) that defaults tolerance to Utils.NearlyZero, reducing call-site verbosity.",
          "category": "fix",
          "confidence": 0.85,
          "effort": "cost/xs",
          "validated": "untested"
        }
      ],
      "recommendedProposal": "Use NearlyEqual in SKMatrix factory guards",
      "recommendedReason": "Directly addresses the robustness concern raised by the reporter."
    }
  },
  "output": {
    "actionability": {
      "suggestedAction": "keep-open",
      "confidence": 0.8,
      "reason": "Valid enhancement with clear implementation path. The fix is small, low-risk, and the infrastructure already exists. Keeping open for future contribution.",
      "suggestedReproPlatform": "linux"
    },
    "actions": [
      {
        "type": "update-labels",
        "description": "Apply enhancement and core area labels",
        "risk": "low",
        "confidence": 0.9,
        "labels": [
          "type/enhancement",
          "area/SkiaSharp",
          "tenet/reliability"
        ]
      },
      {
        "type": "add-comment",
        "description": "Acknowledge the valid enhancement and confirm the patterns still exist",
        "risk": "medium",
        "confidence": 0.82,
        "comment": "Thanks for the suggestion! The exact float equality patterns (`== 1`, `== 0`) are still present in `SKMatrix.cs` factory methods (`CreateScale`, `CreateTranslation`, `CreateSkew`, `CreateScaleTranslation`). Using `Utils.NearlyEqual` with `Utils.NearlyZero` tolerance would make these guards more robust to floating-point rounding. `Utils.NearlyEqual` is already used in `SKRoundRect.cs`, so the pattern is established.\n\nA PR implementing these two changes would be welcome:\n1. Replace exact float guards in `SKMatrix.cs` with `Utils.NearlyEqual` calls.\n2. Add a zero-tolerance default overload to `Utils.NearlyEqual` for ergonomics."
      }
    ]
  }
}
```

</details>
