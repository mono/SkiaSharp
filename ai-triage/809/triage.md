# Issue Triage Report — #809

| Field | Value |
|-------|-------|
| Repository | mono/SkiaSharp |
| Analyzed | 2026-05-01T12:00:55Z |
| Type | type/bug (0.95 (95%)) |
| Area | area/SkiaSharp (0.95 (95%)) |
| Suggested action | ready-to-fix (0.95 (95%)) |

**Issue Summary:** SKPoint.Reflect() computes the dot product of the vector with itself instead of with the normal, producing incorrect reflection results.

**Analysis:** The Reflect method on SKPoint (and SKPointI) computes `dot = point.x * point.x + point.y * point.y` (self-dot product) instead of `dot = point.x * normal.x + point.y * normal.y` (dot product with normal). The reflection formula r = v - 2(v·n)n requires the dot product of the vector with the normal, not with itself. This bug exists in both SKPoint.Reflect and SKPointI.Reflect.

**Recommendations:** **ready-to-fix** — Bug is confirmed by code inspection and a contributor. Root cause is a clear single-line math error in both SKPoint.Reflect and SKPointI.Reflect. Fix is trivial.

---

## Classification

| Field | Value |
|-------|-------|
| Type | type/bug |
| Area | area/SkiaSharp |
| Platforms | — |
| Backends | — |
| Tenets | tenet/reliability |
| Partner | — |
| Current labels | type/question |

## Evidence

### Reproduction

1. Call SKPoint.Reflect(new SKPoint(10,10), new SKPoint(0,1))
2. Observe result is (10, -390)
3. Expected result is (10, -10)

**Environment:** Any platform/version — pure math bug in MathTypes.cs

### Bug Signals

| Field | Value |
|-------|-------|
| Severity | medium |
| Regression claimed | False |
| Error type | wrong-output |
| Error message | SKPoint.Reflect(new SKPoint(10,10), new SKPoint(0,1)) returns (10, -390) instead of (10, -10) |
| Repro quality | complete |
| Target frameworks | — |

## Analysis

### Technical Summary

The Reflect method on SKPoint (and SKPointI) computes `dot = point.x * point.x + point.y * point.y` (self-dot product) instead of `dot = point.x * normal.x + point.y * normal.y` (dot product with normal). The reflection formula r = v - 2(v·n)n requires the dot product of the vector with the normal, not with itself. This bug exists in both SKPoint.Reflect and SKPointI.Reflect.

### Rationale

The reporter correctly identifies that the dot product should be between the input vector and the normal vector, not the vector with itself. A contributor (Gillibald) confirmed this with the correct formula r = point - 2(point·normal)normal. Code inspection confirms the bug is still present in the current codebase. The correct formula for specular/optical reflection of v across normal n is r = v - 2(v·n)n.

### Key Signals

- "var dot = point.x * point.x + point.y * point.y" — **binding/SkiaSharp/MathTypes.cs line 62** (Uses self-dot product instead of dot product with normal — incorrect reflection formula.)
- "I think you are right. r=point - 2(point*normal)normal" — **comment by Gillibald (contributor)** (Contributor confirms the bug and provides correct formula.)
- "by using this method to calculate the reflection we get a vector with coordinates 10, -390 which is kind of weird" — **issue body** (Concrete example demonstrating wrong output.)

### Code Investigation

| File | Lines | Relevance | Finding |
|------|-------|-----------|---------|
| `binding/SkiaSharp/MathTypes.cs` | 60-66 | direct | SKPoint.Reflect computes `dot = point.x * point.x + point.y * point.y` — self-dot product. Should be `dot = point.x * normal.x + point.y * normal.y`. Same bug exists for SKPointI.Reflect at lines 161-166. |
| `binding/SkiaSharp/MathTypes.cs` | 161-166 | direct | SKPointI.Reflect has the identical bug: `dot = point.x * point.x + point.y * point.y` instead of `dot = point.x * normal.x + point.y * normal.y`. |

### Resolution Proposals

**Hypothesis:** The dot product calculation in Reflect() uses the wrong operands — it multiplies the vector with itself rather than with the normal vector.

1. **Fix dot product in SKPoint.Reflect and SKPointI.Reflect** — fix, confidence 0.98 (98%), cost/xs, validated=yes
   - Change `dot = point.x * point.x + point.y * point.y` to `dot = point.x * normal.x + point.y * normal.y` in both SKPoint.Reflect (line 62) and SKPointI.Reflect (line 163). Add unit tests to verify the fix.

```csharp
// SKPoint.Reflect
var dot = point.x * normal.x + point.y * normal.y;
// SKPointI.Reflect
var dot = point.x * normal.x + point.y * normal.y;
```

**Recommended proposal:** Fix dot product in SKPoint.Reflect and SKPointI.Reflect

**Why:** Trivial one-line fix that corrects the mathematical formula. High confidence because the correct formula r=v-2(v·n)n is mathematically unambiguous.

## Recommendations

### Actionability

| Field | Value |
|-------|-------|
| Suggested action | ready-to-fix |
| Confidence | 0.95 (95%) |
| Reason | Bug is confirmed by code inspection and a contributor. Root cause is a clear single-line math error in both SKPoint.Reflect and SKPointI.Reflect. Fix is trivial. |
| Suggested repro platform | linux |

### Automatable Actions

| Type | Risk | Confidence | Description | Details |
|------|------|------------|-------------|---------|
| update-labels | low | 0.95 (95%) | Correct label from type/question to type/bug; add area and tenet | labels=type/bug, area/SkiaSharp, tenet/reliability |
| add-comment | medium | 0.95 (95%) | Confirm the bug and explain the fix | — |

**Comment draft for `add-comment`:**

```markdown
You're correct — this is a bug. The reflection formula should be **r = v − 2(v·n)n**, where the dot product is between the incident vector and the normal, not the vector with itself.

The current code at `MathTypes.cs` line 62:
```csharp
var dot = point.x * point.x + point.y * point.y; // ❌ self-dot product
```
Should be:
```csharp
var dot = point.x * normal.x + point.y * normal.y; // ✅ dot with normal
```
The same bug exists in `SKPointI.Reflect`. A fix is tracked — the change is a one-liner in `binding/SkiaSharp/MathTypes.cs`.
```

<details>
<summary>Raw JSON</summary>

```json
{
  "meta": {
    "schemaVersion": "1.0",
    "number": 809,
    "repo": "mono/SkiaSharp",
    "analyzedAt": "2026-05-01T12:00:55Z",
    "currentLabels": [
      "type/question"
    ]
  },
  "summary": "SKPoint.Reflect() computes the dot product of the vector with itself instead of with the normal, producing incorrect reflection results.",
  "classification": {
    "type": {
      "value": "type/bug",
      "confidence": 0.95
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
    "bugSignals": {
      "severity": "medium",
      "regressionClaimed": false,
      "errorType": "wrong-output",
      "errorMessage": "SKPoint.Reflect(new SKPoint(10,10), new SKPoint(0,1)) returns (10, -390) instead of (10, -10)",
      "reproQuality": "complete"
    },
    "reproEvidence": {
      "stepsToReproduce": [
        "Call SKPoint.Reflect(new SKPoint(10,10), new SKPoint(0,1))",
        "Observe result is (10, -390)",
        "Expected result is (10, -10)"
      ],
      "environmentDetails": "Any platform/version — pure math bug in MathTypes.cs"
    }
  },
  "analysis": {
    "summary": "The Reflect method on SKPoint (and SKPointI) computes `dot = point.x * point.x + point.y * point.y` (self-dot product) instead of `dot = point.x * normal.x + point.y * normal.y` (dot product with normal). The reflection formula r = v - 2(v·n)n requires the dot product of the vector with the normal, not with itself. This bug exists in both SKPoint.Reflect and SKPointI.Reflect.",
    "rationale": "The reporter correctly identifies that the dot product should be between the input vector and the normal vector, not the vector with itself. A contributor (Gillibald) confirmed this with the correct formula r = point - 2(point·normal)normal. Code inspection confirms the bug is still present in the current codebase. The correct formula for specular/optical reflection of v across normal n is r = v - 2(v·n)n.",
    "keySignals": [
      {
        "text": "var dot = point.x * point.x + point.y * point.y",
        "source": "binding/SkiaSharp/MathTypes.cs line 62",
        "interpretation": "Uses self-dot product instead of dot product with normal — incorrect reflection formula."
      },
      {
        "text": "I think you are right. r=point - 2(point*normal)normal",
        "source": "comment by Gillibald (contributor)",
        "interpretation": "Contributor confirms the bug and provides correct formula."
      },
      {
        "text": "by using this method to calculate the reflection we get a vector with coordinates 10, -390 which is kind of weird",
        "source": "issue body",
        "interpretation": "Concrete example demonstrating wrong output."
      }
    ],
    "codeInvestigation": [
      {
        "file": "binding/SkiaSharp/MathTypes.cs",
        "lines": "60-66",
        "finding": "SKPoint.Reflect computes `dot = point.x * point.x + point.y * point.y` — self-dot product. Should be `dot = point.x * normal.x + point.y * normal.y`. Same bug exists for SKPointI.Reflect at lines 161-166.",
        "relevance": "direct"
      },
      {
        "file": "binding/SkiaSharp/MathTypes.cs",
        "lines": "161-166",
        "finding": "SKPointI.Reflect has the identical bug: `dot = point.x * point.x + point.y * point.y` instead of `dot = point.x * normal.x + point.y * normal.y`.",
        "relevance": "direct"
      }
    ],
    "resolution": {
      "hypothesis": "The dot product calculation in Reflect() uses the wrong operands — it multiplies the vector with itself rather than with the normal vector.",
      "proposals": [
        {
          "title": "Fix dot product in SKPoint.Reflect and SKPointI.Reflect",
          "description": "Change `dot = point.x * point.x + point.y * point.y` to `dot = point.x * normal.x + point.y * normal.y` in both SKPoint.Reflect (line 62) and SKPointI.Reflect (line 163). Add unit tests to verify the fix.",
          "category": "fix",
          "codeSnippet": "// SKPoint.Reflect\nvar dot = point.x * normal.x + point.y * normal.y;\n// SKPointI.Reflect\nvar dot = point.x * normal.x + point.y * normal.y;",
          "confidence": 0.98,
          "effort": "cost/xs",
          "validated": "yes"
        }
      ],
      "recommendedProposal": "Fix dot product in SKPoint.Reflect and SKPointI.Reflect",
      "recommendedReason": "Trivial one-line fix that corrects the mathematical formula. High confidence because the correct formula r=v-2(v·n)n is mathematically unambiguous."
    }
  },
  "output": {
    "actionability": {
      "suggestedAction": "ready-to-fix",
      "confidence": 0.95,
      "reason": "Bug is confirmed by code inspection and a contributor. Root cause is a clear single-line math error in both SKPoint.Reflect and SKPointI.Reflect. Fix is trivial.",
      "suggestedReproPlatform": "linux"
    },
    "actions": [
      {
        "type": "update-labels",
        "description": "Correct label from type/question to type/bug; add area and tenet",
        "risk": "low",
        "confidence": 0.95,
        "labels": [
          "type/bug",
          "area/SkiaSharp",
          "tenet/reliability"
        ]
      },
      {
        "type": "add-comment",
        "description": "Confirm the bug and explain the fix",
        "risk": "medium",
        "confidence": 0.95,
        "comment": "You're correct — this is a bug. The reflection formula should be **r = v − 2(v·n)n**, where the dot product is between the incident vector and the normal, not the vector with itself.\n\nThe current code at `MathTypes.cs` line 62:\n```csharp\nvar dot = point.x * point.x + point.y * point.y; // ❌ self-dot product\n```\nShould be:\n```csharp\nvar dot = point.x * normal.x + point.y * normal.y; // ✅ dot with normal\n```\nThe same bug exists in `SKPointI.Reflect`. A fix is tracked — the change is a one-liner in `binding/SkiaSharp/MathTypes.cs`."
      }
    ]
  }
}
```

</details>
