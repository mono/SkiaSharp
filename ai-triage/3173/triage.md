# Issue Triage Report — #3173

| Field | Value |
|-------|-------|
| Repository | mono/SkiaSharp |
| Analyzed | 2026-04-27T23:59:17Z |
| Type | type/bug (0.98 (98%)) |
| Area | area/SkiaSharp (0.98 (98%)) |
| Suggested action | ready-to-fix (0.95 (95%)) |

**Issue Summary:** SKImageFilter.CreateTile(src, dst) always throws ArgumentNullException because the 2-param overload passes null to a 3-param overload that incorrectly rejects null, making the no-input-filter variant completely unusable.

**Analysis:** The 3-param CreateTile overload has an ArgumentNullException guard on its input parameter, but Skia's C++ API accepts null to mean 'use implicit source'. The 2-param convenience overload explicitly passes null, making it permanently broken. The fix is to remove the null guard and use input?.Handle ?? IntPtr.Zero, consistent with how other SKImageFilter methods (e.g. CreateMatrix) handle optional inputs.

**Recommendations:** **ready-to-fix** — Root cause is clear (incorrect null guard), fix is a one-line change matching existing patterns in the same file, and confirmed by Skia upstream API documentation.

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
| Current labels | type/bug |

## Evidence

### Reproduction

1. Call SKImageFilter.CreateTile(src, dst) with any valid SKRect values
2. Observe ArgumentNullException thrown from the null-check guard in the 3-param overload

**Repository links:**
- https://github.com/mono/SkiaSharp/blob/80d6b5915329d04ba8e557398e6ecc379dd48914/binding/SkiaSharp/SKImageFilter.cs#L321-L328 — Reported code location — 2-param overload delegates to 3-param with null, which throws
- https://api.skia.org/classSkImageFilters.html#aad7faeda56f1beb913936d67cce31d1e — Skia C++ API confirming null is valid for the input filter parameter

**Code snippets:**

```csharp
SKImageFilter.CreateTile(src, dst, (SKImageFilter) null); // throws
```

```csharp
SKImageFilter.CreateTile(src, dst); // also throws — calls above with null
```

### Bug Signals

| Field | Value |
|-------|-------|
| Severity | high |
| Regression claimed | True |
| Error type | exception |
| Error message | ArgumentNullException: Value cannot be null. (Parameter 'input') |
| Repro quality | complete |
| Target frameworks | net9.0 |

### Version Analysis

| Field | Value |
|-------|-------|
| Mentioned versions | 3.116.0, 2.88.9 |
| Worked in | 2.88.9 |
| Broke in | 3.116.0 |
| Current relevance | likely |
| Relevance reason | Code at binding/SkiaSharp/SKImageFilter.cs lines 321-328 still contains the incorrect null-check guard in 3.116.x. |

### Regression

| Field | Value |
|-------|-------|
| Is regression | True |
| Confidence | 0.90 (90%) |
| Reason | Reporter lists 2.88.9 as last known good version; the null check was introduced during the v3 refactor of SKImageFilter. |
| Worked in version | 2.88.9 |
| Broke in version | 3.116.0 |

## Analysis

### Technical Summary

The 3-param CreateTile overload has an ArgumentNullException guard on its input parameter, but Skia's C++ API accepts null to mean 'use implicit source'. The 2-param convenience overload explicitly passes null, making it permanently broken. The fix is to remove the null guard and use input?.Handle ?? IntPtr.Zero, consistent with how other SKImageFilter methods (e.g. CreateMatrix) handle optional inputs.

### Rationale

This is a clear bug: the 2-param convenience overload unconditionally throws because it passes null to a 3-param overload that incorrectly rejects null. Skia's upstream C++ API explicitly allows a null input filter (meaning 'use the implicit source'). The fix matches the pattern already used for CreateMatrix and other filters.

### Key Signals

- "CreateTile(src, dst) overload is useless. It makes a call to overload with input parameter and passes null to it. However this overload throws ArgumentException if input is null." — **issue body** (Clear regression: the shorthand overload always throws at runtime.)
- "Null is actually valid for the input filter in Skia" — **comment by jeremy-visionaid (contributor)** (Confirms the null check is incorrect; Skia C++ SkImageFilters::Tile accepts nullptr for input.)
- "Last Known Good Version: 2.88.9" — **issue body** (Regression introduced in v3.116.0 — the null guard was added incorrectly during the API refactor.)

### Code Investigation

| File | Lines | Relevance | Finding |
|------|-------|-----------|---------|
| `binding/SkiaSharp/SKImageFilter.cs` | 321-328 | direct | CreateTile(SKRect src, SKRect dst) delegates to CreateTile(src, dst, null). The 3-param overload has '_ = input ?? throw new ArgumentNullException(nameof(input))' followed by 'input.Handle', making null impossible and the 2-param overload always throw. |
| `binding/SkiaSharp/SKImageFilter.cs` | 31-35 | related | CreateMatrix(in SKMatrix matrix, SKSamplingOptions sampling, SKImageFilter? input) correctly uses 'input?.Handle ?? IntPtr.Zero' — the same pattern should be applied to CreateTile. |

**Error fingerprint:** `SKImageFilter.CreateTile:ArgumentNullException:input`

### Workarounds

- Pass an explicit identity filter as input: SKImageFilter.CreateTile(src, dst, SKImageFilter.CreateMatrix(SKMatrix.Identity)) — this approximates the expected behavior but is not equivalent to a true null/implicit input.

### Next Questions

- Were other SKImageFilter overloads similarly broken during the v3 refactor (incorrect null guards on optional inputs)?
- Does the C API sk_imagefilter_new_tile correctly forward a null/zero handle to SkImageFilters::Tile?

### Resolution Proposals

**Hypothesis:** The null check on 'input' in CreateTile was incorrectly added during the v3 API refactor; upstream Skia allows null to mean 'implicit source'.

1. **Remove incorrect null guard — use input?.Handle ?? IntPtr.Zero** — fix, confidence 0.95 (95%), cost/xs, validated=yes
   - Replace '_ = input ?? throw new ArgumentNullException(nameof(input)); return GetObject(SkiaApi.sk_imagefilter_new_tile(&src, &dst, input.Handle));' with 'return GetObject(SkiaApi.sk_imagefilter_new_tile(&src, &dst, input?.Handle ?? IntPtr.Zero));' to match the pattern used by CreateMatrix and other optional-input overloads.

```csharp
public static SKImageFilter CreateTile (SKRect src, SKRect dst, SKImageFilter? input) =>
	GetObject (SkiaApi.sk_imagefilter_new_tile (&src, &dst, input?.Handle ?? IntPtr.Zero));
```

**Recommended proposal:** Remove incorrect null guard — use input?.Handle ?? IntPtr.Zero

**Why:** One-line fix matching the established pattern for optional SKImageFilter inputs throughout the same file. High confidence from upstream API docs and contributor confirmation.

## Recommendations

### Actionability

| Field | Value |
|-------|-------|
| Suggested action | ready-to-fix |
| Confidence | 0.95 (95%) |
| Reason | Root cause is clear (incorrect null guard), fix is a one-line change matching existing patterns in the same file, and confirmed by Skia upstream API documentation. |
| Suggested repro platform | linux |

### Automatable Actions

| Type | Risk | Confidence | Description | Details |
|------|------|------------|-------------|---------|
| update-labels | low | 0.98 (98%) | Apply bug, area/SkiaSharp, and reliability tenet labels | labels=type/bug, area/SkiaSharp, tenet/reliability |
| add-comment | medium | 0.95 (95%) | Confirm the bug and describe the fix | — |

**Comment draft for `add-comment`:**

```markdown
Thanks for the report! This is confirmed — the `CreateTile(SKRect src, SKRect dst)` overload is broken because it passes `null` to the 3-param overload which incorrectly throws `ArgumentNullException`. The Skia C++ API explicitly [allows `null` for the input filter](https://api.skia.org/classSkImageFilters.html#aad7faeda56f1beb913936d67cce31d1e) (it means "use the implicit source").

The fix is a one-line change in `binding/SkiaSharp/SKImageFilter.cs` — replace the null guard with `input?.Handle ?? IntPtr.Zero`, consistent with `CreateMatrix` and other optional-input overloads:

```csharp
public static SKImageFilter CreateTile (SKRect src, SKRect dst, SKImageFilter? input) =>
    GetObject (SkiaApi.sk_imagefilter_new_tile (&src, &dst, input?.Handle ?? IntPtr.Zero));
```

**Workaround (until fixed):** pass an explicit input — e.g. `SKImageFilter.CreateTile(src, dst, SKImageFilter.CreateMatrix(SKMatrix.Identity))` — though this is not fully equivalent.
```

<details>
<summary>Raw JSON</summary>

```json
{
  "meta": {
    "schemaVersion": "1.0",
    "number": 3173,
    "repo": "mono/SkiaSharp",
    "analyzedAt": "2026-04-27T23:59:17Z",
    "currentLabels": [
      "type/bug"
    ]
  },
  "summary": "SKImageFilter.CreateTile(src, dst) always throws ArgumentNullException because the 2-param overload passes null to a 3-param overload that incorrectly rejects null, making the no-input-filter variant completely unusable.",
  "classification": {
    "type": {
      "value": "type/bug",
      "confidence": 0.98
    },
    "area": {
      "value": "area/SkiaSharp",
      "confidence": 0.98
    },
    "tenets": [
      "tenet/reliability"
    ]
  },
  "evidence": {
    "bugSignals": {
      "severity": "high",
      "regressionClaimed": true,
      "errorType": "exception",
      "errorMessage": "ArgumentNullException: Value cannot be null. (Parameter 'input')",
      "reproQuality": "complete",
      "targetFrameworks": [
        "net9.0"
      ]
    },
    "reproEvidence": {
      "stepsToReproduce": [
        "Call SKImageFilter.CreateTile(src, dst) with any valid SKRect values",
        "Observe ArgumentNullException thrown from the null-check guard in the 3-param overload"
      ],
      "codeSnippets": [
        "SKImageFilter.CreateTile(src, dst, (SKImageFilter) null); // throws",
        "SKImageFilter.CreateTile(src, dst); // also throws — calls above with null"
      ],
      "repoLinks": [
        {
          "url": "https://github.com/mono/SkiaSharp/blob/80d6b5915329d04ba8e557398e6ecc379dd48914/binding/SkiaSharp/SKImageFilter.cs#L321-L328",
          "description": "Reported code location — 2-param overload delegates to 3-param with null, which throws"
        },
        {
          "url": "https://api.skia.org/classSkImageFilters.html#aad7faeda56f1beb913936d67cce31d1e",
          "description": "Skia C++ API confirming null is valid for the input filter parameter"
        }
      ]
    },
    "versionAnalysis": {
      "mentionedVersions": [
        "3.116.0",
        "2.88.9"
      ],
      "workedIn": "2.88.9",
      "brokeIn": "3.116.0",
      "currentRelevance": "likely",
      "relevanceReason": "Code at binding/SkiaSharp/SKImageFilter.cs lines 321-328 still contains the incorrect null-check guard in 3.116.x."
    },
    "regression": {
      "isRegression": true,
      "confidence": 0.9,
      "reason": "Reporter lists 2.88.9 as last known good version; the null check was introduced during the v3 refactor of SKImageFilter.",
      "workedInVersion": "2.88.9",
      "brokeInVersion": "3.116.0"
    }
  },
  "analysis": {
    "summary": "The 3-param CreateTile overload has an ArgumentNullException guard on its input parameter, but Skia's C++ API accepts null to mean 'use implicit source'. The 2-param convenience overload explicitly passes null, making it permanently broken. The fix is to remove the null guard and use input?.Handle ?? IntPtr.Zero, consistent with how other SKImageFilter methods (e.g. CreateMatrix) handle optional inputs.",
    "codeInvestigation": [
      {
        "file": "binding/SkiaSharp/SKImageFilter.cs",
        "lines": "321-328",
        "finding": "CreateTile(SKRect src, SKRect dst) delegates to CreateTile(src, dst, null). The 3-param overload has '_ = input ?? throw new ArgumentNullException(nameof(input))' followed by 'input.Handle', making null impossible and the 2-param overload always throw.",
        "relevance": "direct"
      },
      {
        "file": "binding/SkiaSharp/SKImageFilter.cs",
        "lines": "31-35",
        "finding": "CreateMatrix(in SKMatrix matrix, SKSamplingOptions sampling, SKImageFilter? input) correctly uses 'input?.Handle ?? IntPtr.Zero' — the same pattern should be applied to CreateTile.",
        "relevance": "related"
      }
    ],
    "keySignals": [
      {
        "text": "CreateTile(src, dst) overload is useless. It makes a call to overload with input parameter and passes null to it. However this overload throws ArgumentException if input is null.",
        "source": "issue body",
        "interpretation": "Clear regression: the shorthand overload always throws at runtime."
      },
      {
        "text": "Null is actually valid for the input filter in Skia",
        "source": "comment by jeremy-visionaid (contributor)",
        "interpretation": "Confirms the null check is incorrect; Skia C++ SkImageFilters::Tile accepts nullptr for input."
      },
      {
        "text": "Last Known Good Version: 2.88.9",
        "source": "issue body",
        "interpretation": "Regression introduced in v3.116.0 — the null guard was added incorrectly during the API refactor."
      }
    ],
    "rationale": "This is a clear bug: the 2-param convenience overload unconditionally throws because it passes null to a 3-param overload that incorrectly rejects null. Skia's upstream C++ API explicitly allows a null input filter (meaning 'use the implicit source'). The fix matches the pattern already used for CreateMatrix and other filters.",
    "workarounds": [
      "Pass an explicit identity filter as input: SKImageFilter.CreateTile(src, dst, SKImageFilter.CreateMatrix(SKMatrix.Identity)) — this approximates the expected behavior but is not equivalent to a true null/implicit input."
    ],
    "nextQuestions": [
      "Were other SKImageFilter overloads similarly broken during the v3 refactor (incorrect null guards on optional inputs)?",
      "Does the C API sk_imagefilter_new_tile correctly forward a null/zero handle to SkImageFilters::Tile?"
    ],
    "errorFingerprint": "SKImageFilter.CreateTile:ArgumentNullException:input",
    "resolution": {
      "hypothesis": "The null check on 'input' in CreateTile was incorrectly added during the v3 API refactor; upstream Skia allows null to mean 'implicit source'.",
      "proposals": [
        {
          "title": "Remove incorrect null guard — use input?.Handle ?? IntPtr.Zero",
          "description": "Replace '_ = input ?? throw new ArgumentNullException(nameof(input)); return GetObject(SkiaApi.sk_imagefilter_new_tile(&src, &dst, input.Handle));' with 'return GetObject(SkiaApi.sk_imagefilter_new_tile(&src, &dst, input?.Handle ?? IntPtr.Zero));' to match the pattern used by CreateMatrix and other optional-input overloads.",
          "codeSnippet": "public static SKImageFilter CreateTile (SKRect src, SKRect dst, SKImageFilter? input) =>\n\tGetObject (SkiaApi.sk_imagefilter_new_tile (&src, &dst, input?.Handle ?? IntPtr.Zero));",
          "category": "fix",
          "confidence": 0.95,
          "effort": "cost/xs",
          "validated": "yes"
        }
      ],
      "recommendedProposal": "Remove incorrect null guard — use input?.Handle ?? IntPtr.Zero",
      "recommendedReason": "One-line fix matching the established pattern for optional SKImageFilter inputs throughout the same file. High confidence from upstream API docs and contributor confirmation."
    }
  },
  "output": {
    "actionability": {
      "suggestedAction": "ready-to-fix",
      "confidence": 0.95,
      "reason": "Root cause is clear (incorrect null guard), fix is a one-line change matching existing patterns in the same file, and confirmed by Skia upstream API documentation.",
      "suggestedReproPlatform": "linux"
    },
    "actions": [
      {
        "type": "update-labels",
        "description": "Apply bug, area/SkiaSharp, and reliability tenet labels",
        "risk": "low",
        "confidence": 0.98,
        "labels": [
          "type/bug",
          "area/SkiaSharp",
          "tenet/reliability"
        ]
      },
      {
        "type": "add-comment",
        "description": "Confirm the bug and describe the fix",
        "risk": "medium",
        "confidence": 0.95,
        "comment": "Thanks for the report! This is confirmed — the `CreateTile(SKRect src, SKRect dst)` overload is broken because it passes `null` to the 3-param overload which incorrectly throws `ArgumentNullException`. The Skia C++ API explicitly [allows `null` for the input filter](https://api.skia.org/classSkImageFilters.html#aad7faeda56f1beb913936d67cce31d1e) (it means \"use the implicit source\").\n\nThe fix is a one-line change in `binding/SkiaSharp/SKImageFilter.cs` — replace the null guard with `input?.Handle ?? IntPtr.Zero`, consistent with `CreateMatrix` and other optional-input overloads:\n\n```csharp\npublic static SKImageFilter CreateTile (SKRect src, SKRect dst, SKImageFilter? input) =>\n    GetObject (SkiaApi.sk_imagefilter_new_tile (&src, &dst, input?.Handle ?? IntPtr.Zero));\n```\n\n**Workaround (until fixed):** pass an explicit input — e.g. `SKImageFilter.CreateTile(src, dst, SKImageFilter.CreateMatrix(SKMatrix.Identity))` — though this is not fully equivalent."
      }
    ]
  }
}
```

</details>
