# Issue Triage Report — #4111

| Field | Value |
|-------|-------|
| Repository | mono/SkiaSharp |
| Analyzed | 2026-06-05T05:45:00Z |
| Type | type/bug (0.98 (98%)) |
| Area | area/SkiaSharp (0.98 (98%)) |
| Suggested action | ready-to-fix (0.97 (97%)) |

**Issue Summary:** SKPixmap.GetPixelSpan<T>(int x, int y) uses info.Height instead of info.Width in the row-major offset formula, producing wrong pixel offsets for all non-square pixmaps in the typed-access branch.

**Analysis:** The typed-access branch of GetPixelSpan<T>(x, y) at SKPixmap.cs:162 computes the pixel offset as y * info.Height + x, but the correct row-major formula is y * info.Width + x. The bug is invisible for square pixmaps (Width==Height) and produces wrong pixel access for any non-square pixmap. The existing test suite only uses 3x3 square pixmaps and contains the same typo in expectedLength calculations, so CI does not catch this.

**Recommendations:** **ready-to-fix** — Root cause is confirmed in source (SKPixmap.cs:162), fix is a one-line change, and the reporter (project maintainer) has fully specified the bug and fix.

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
| Current labels | type/bug, area/SkiaSharp |

## Evidence

### Reproduction

1. Create a non-square SKBitmap (e.g., 10 wide x 5 tall, Rgba8888)
2. Set a pixel at (3, 2) using SetPixel
3. Call PeekPixels() to get SKPixmap
4. Call pixmap.GetPixelSpan<SKColor>(3, 2)
5. Observe span[0] is the wrong pixel — offset computed as 2*5+3=13 instead of 2*10+3=23

**Environment:** Pure C# binding bug — affects all platforms equally. No specific TFM or OS required.

**Repository links:**
- https://github.com/mono/SkiaSharp/pull/2609 — PR #2609 'Added GetPixelSpan() with offsets' — introduced the Height-vs-Width typo

### Bug Signals

| Field | Value |
|-------|-------|
| Severity | high |
| Regression claimed | False |
| Error type | wrong-output |
| Error message | GetPixelSpan<T>(x, y) returns span at wrong memory offset for non-square pixmaps — reads/writes wrong pixels |
| Repro quality | complete |
| Target frameworks | — |

### Version Analysis

| Field | Value |
|-------|-------|
| Mentioned versions | — |
| Worked in | — |
| Broke in | — |
| Current relevance | likely |
| Relevance reason | Bug is present in current source at binding/SkiaSharp/SKPixmap.cs:162. No fix has been applied. |

## Analysis

### Technical Summary

The typed-access branch of GetPixelSpan<T>(x, y) at SKPixmap.cs:162 computes the pixel offset as y * info.Height + x, but the correct row-major formula is y * info.Width + x. The bug is invisible for square pixmaps (Width==Height) and produces wrong pixel access for any non-square pixmap. The existing test suite only uses 3x3 square pixmaps and contains the same typo in expectedLength calculations, so CI does not catch this.

### Rationale

Bug is confirmed directly in source at SKPixmap.cs:162. The formula y * info.Height + x is mathematically wrong for row-major layout where Width (not Height) is the row stride in pixel units. Cross-check: the byte-typed path uses GetPixelBytesOffset which computes y * RowBytes = y * Width * BytesPerPixel, confirming the correct pixel-unit offset is y * Width + x. The reporter (project maintainer) has identified the exact location, root cause, and one-line fix.

### Key Signals

- "spanOffset = y * info.Height + x;" — **binding/SkiaSharp/SKPixmap.cs:162** (Uses Height instead of Width — wrong for row-major layout where each row spans Width pixels.)
- "The existing tests all use square pixmaps (3x3), so the bug was invisible." — **issue body** (Width==Height for 3x3 makes y*Height+x == y*Width+x, masking the bug from all existing tests.)
- "That doesn't sound like it accounts for stride?" — **comment by jeremy-visionaid** (Valid secondary concern: for ROI/padded pixmaps where pixmap.RowBytes > Width*bpp, the typed span approach is inherently limited regardless of this fix. Pre-existing design gap, not in scope for this PR.)

### Code Investigation

| File | Lines | Relevance | Finding |
|------|-------|-----------|---------|
| `binding/SkiaSharp/SKPixmap.cs` | 162 | direct | spanOffset = y * info.Height + x — uses Height instead of Width. For a 10x5 pixmap, pixel (3,2) computes offset 2*5+3=13 instead of correct 2*10+3=23. This is the confirmed bug. |
| `binding/SkiaSharp/SKImageInfo.cs` | 128-131 | direct | GetPixelBytesOffset (byte-typed path) computes y * RowBytes + x * bpp = y * Width * bpp + x * bpp = (y * Width + x) * bpp, confirming y * Width + x is the correct pixel-unit offset formula. |
| `tests/Tests/SkiaSharp/SKPixmapTest.cs` | 308-333 | related | GetPixelSpanWithOffsetReadsValuesCorrectly uses a 3x3 square pixmap and computes expectedLength as (Width*Height) - (y * info.Height + x) — same typo. Code and test both use Height, so they agree and tests pass for 3x3 square pixmaps. |
| `tests/Tests/SkiaSharp/SKPixmapTest.cs` | 344-361 | related | GetPixelSpanWithOffsetReads565Correctly also uses 3x3 square pixmap with the same y * info.Height + x typo in expectedLength calculation. |
| `tests/Tests/SkiaSharp/SKPixmapTest.cs` | 370-389 | related | GetPixelSpanWithOffsetReadsGray8Correctly also uses 3x3 square pixmap with same typo — all three offset tests need expectedLength updated alongside the code fix. |

**Error fingerprint:** `SKPixmap.GetPixelSpan<T>(x,y):wrong-offset:height-instead-of-width`

### Workarounds

- Use GetPixelSpan<T>() (zero-arg overload) which starts at offset 0 correctly, then manually slice: var info = pixmap.Info; var full = pixmap.GetPixelSpan<SKColor>(); if (!full.IsEmpty) { var span = full.Slice(y * info.Width + x); }
- Use pixmap.GetPixelColor(x, y) for read-only pixel access — always correct regardless of pixmap geometry.

### Next Questions

- Should the stride concern for ROI/padded pixmaps (where RowBytes > Width*bpp) be addressed as a follow-up issue for GetPixelSpan<T>?
- The three test methods in SKPixmapTest.cs have the same y * info.Height + x typo in expectedLength — should these be fixed in the same PR?

### Resolution Proposals

**Hypothesis:** The row-major pixel offset formula uses Height where Width is required. Since pixel layout places Width pixels per row, the correct typed-unit offset is y * Width + x, not y * Height + x.

1. **Fix the offset formula in SKPixmap.cs** — fix, confidence 0.98 (98%), cost/xs, validated=yes
   - One-line change: replace info.Height with info.Width in the spanOffset calculation at line 162. Also update expectedLength in the three test methods that contain the same typo.

```csharp
// binding/SkiaSharp/SKPixmap.cs line 162 — change:
spanOffset = y * info.Height + x;
// to:
spanOffset = y * info.Width + x;
```
2. **Workaround: use zero-arg overload and manually slice** — workaround, confidence 0.90 (90%), cost/xs, validated=yes
   - Until the fix ships, bypass the buggy 2-arg overload by fetching the full span at offset 0 (which is correct) and manually computing the row-major offset. Ensure pixmap is not empty before slicing.

```csharp
var info = pixmap.Info;
var fullSpan = pixmap.GetPixelSpan<SKColor>(); // zero-arg: starts at (0,0) correctly
if (fullSpan.IsEmpty) return;
int offset = y * info.Width + x; // correct row-major formula
var pixelSpan = fullSpan.Slice(offset);
```

**Recommended proposal:** Fix the offset formula in SKPixmap.cs

**Why:** One-line fix with zero risk. Root cause is unambiguous, confirmed by cross-check with the byte-typed path. Tests in SKPixmapTest.cs will need the same typo corrected and a non-square test case added to prevent future regression.

## Recommendations

### Actionability

| Field | Value |
|-------|-------|
| Suggested action | ready-to-fix |
| Confidence | 0.97 (97%) |
| Reason | Root cause is confirmed in source (SKPixmap.cs:162), fix is a one-line change, and the reporter (project maintainer) has fully specified the bug and fix. |
| Suggested repro platform | linux |

### Automatable Actions

| Type | Risk | Confidence | Description | Details |
|------|------|------------|-------------|---------|
| update-labels | low | 0.95 (95%) | Add tenet/reliability (type/bug and area/SkiaSharp are already applied) | labels=type/bug, area/SkiaSharp, tenet/reliability |
| add-comment | medium | 0.95 (95%) | Confirm root cause analysis, note test typos, and address the stride concern raised in comments | — |

**Comment draft for `add-comment`:**

```markdown
Code investigation confirms the bug.

**Root cause confirmed:** `binding/SkiaSharp/SKPixmap.cs:162` uses `y * info.Height + x` instead of `y * info.Width + x`. Row-major layout requires Width (columns per row) as the stride, not Height. Cross-check: the byte-typed path calls `GetPixelBytesOffset` which computes `y * RowBytes + x * bpp = (y * Width + x) * bpp` — confirming `y * Width + x` is the correct pixel-unit offset.

**Why tests didn't catch it:** All three `GetPixelSpanWithOffset*` tests use `new SKImageInfo(3, 3, ...)` (square), and the `expectedLength` calculations contain the same typo: `(info.Width * info.Height) - (y * info.Height + x)`. For square images `Width == Height` so both formulas agree and tests pass. These test methods will need the same `Height → Width` correction plus a non-square test case.

**Stride concern (@jeremy-visionaid):** Valid observation. For ROI or stride-padded pixmaps where `pixmap.RowBytes > info.Width * bpp`, the typed-span approach is inherently limited — `spanLength = info.Width * info.Height` under-counts elements and row boundaries misalign. This is a pre-existing design gap that applies regardless of this fix and is worth a separate issue.

**Workaround until fix ships:** Use the zero-arg overload (unaffected) and manually slice:
```csharp
var info = pixmap.Info;
var full = pixmap.GetPixelSpan<SKColor>();
if (!full.IsEmpty)
    return full.Slice(y * info.Width + x);
```
```

<details>
<summary>Raw JSON</summary>

```json
{
  "meta": {
    "schemaVersion": "1.0",
    "number": 4111,
    "repo": "mono/SkiaSharp",
    "analyzedAt": "2026-06-05T05:45:00Z",
    "currentLabels": [
      "type/bug",
      "area/SkiaSharp"
    ]
  },
  "summary": "SKPixmap.GetPixelSpan<T>(int x, int y) uses info.Height instead of info.Width in the row-major offset formula, producing wrong pixel offsets for all non-square pixmaps in the typed-access branch.",
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
      "regressionClaimed": false,
      "errorType": "wrong-output",
      "errorMessage": "GetPixelSpan<T>(x, y) returns span at wrong memory offset for non-square pixmaps — reads/writes wrong pixels",
      "reproQuality": "complete"
    },
    "reproEvidence": {
      "stepsToReproduce": [
        "Create a non-square SKBitmap (e.g., 10 wide x 5 tall, Rgba8888)",
        "Set a pixel at (3, 2) using SetPixel",
        "Call PeekPixels() to get SKPixmap",
        "Call pixmap.GetPixelSpan<SKColor>(3, 2)",
        "Observe span[0] is the wrong pixel — offset computed as 2*5+3=13 instead of 2*10+3=23"
      ],
      "environmentDetails": "Pure C# binding bug — affects all platforms equally. No specific TFM or OS required.",
      "repoLinks": [
        {
          "url": "https://github.com/mono/SkiaSharp/pull/2609",
          "description": "PR #2609 'Added GetPixelSpan() with offsets' — introduced the Height-vs-Width typo"
        }
      ]
    },
    "versionAnalysis": {
      "mentionedVersions": [],
      "currentRelevance": "likely",
      "relevanceReason": "Bug is present in current source at binding/SkiaSharp/SKPixmap.cs:162. No fix has been applied."
    }
  },
  "analysis": {
    "summary": "The typed-access branch of GetPixelSpan<T>(x, y) at SKPixmap.cs:162 computes the pixel offset as y * info.Height + x, but the correct row-major formula is y * info.Width + x. The bug is invisible for square pixmaps (Width==Height) and produces wrong pixel access for any non-square pixmap. The existing test suite only uses 3x3 square pixmaps and contains the same typo in expectedLength calculations, so CI does not catch this.",
    "rationale": "Bug is confirmed directly in source at SKPixmap.cs:162. The formula y * info.Height + x is mathematically wrong for row-major layout where Width (not Height) is the row stride in pixel units. Cross-check: the byte-typed path uses GetPixelBytesOffset which computes y * RowBytes = y * Width * BytesPerPixel, confirming the correct pixel-unit offset is y * Width + x. The reporter (project maintainer) has identified the exact location, root cause, and one-line fix.",
    "codeInvestigation": [
      {
        "file": "binding/SkiaSharp/SKPixmap.cs",
        "lines": "162",
        "finding": "spanOffset = y * info.Height + x — uses Height instead of Width. For a 10x5 pixmap, pixel (3,2) computes offset 2*5+3=13 instead of correct 2*10+3=23. This is the confirmed bug.",
        "relevance": "direct"
      },
      {
        "file": "binding/SkiaSharp/SKImageInfo.cs",
        "lines": "128-131",
        "finding": "GetPixelBytesOffset (byte-typed path) computes y * RowBytes + x * bpp = y * Width * bpp + x * bpp = (y * Width + x) * bpp, confirming y * Width + x is the correct pixel-unit offset formula.",
        "relevance": "direct"
      },
      {
        "file": "tests/Tests/SkiaSharp/SKPixmapTest.cs",
        "lines": "308-333",
        "finding": "GetPixelSpanWithOffsetReadsValuesCorrectly uses a 3x3 square pixmap and computes expectedLength as (Width*Height) - (y * info.Height + x) — same typo. Code and test both use Height, so they agree and tests pass for 3x3 square pixmaps.",
        "relevance": "related"
      },
      {
        "file": "tests/Tests/SkiaSharp/SKPixmapTest.cs",
        "lines": "344-361",
        "finding": "GetPixelSpanWithOffsetReads565Correctly also uses 3x3 square pixmap with the same y * info.Height + x typo in expectedLength calculation.",
        "relevance": "related"
      },
      {
        "file": "tests/Tests/SkiaSharp/SKPixmapTest.cs",
        "lines": "370-389",
        "finding": "GetPixelSpanWithOffsetReadsGray8Correctly also uses 3x3 square pixmap with same typo — all three offset tests need expectedLength updated alongside the code fix.",
        "relevance": "related"
      }
    ],
    "keySignals": [
      {
        "text": "spanOffset = y * info.Height + x;",
        "source": "binding/SkiaSharp/SKPixmap.cs:162",
        "interpretation": "Uses Height instead of Width — wrong for row-major layout where each row spans Width pixels."
      },
      {
        "text": "The existing tests all use square pixmaps (3x3), so the bug was invisible.",
        "source": "issue body",
        "interpretation": "Width==Height for 3x3 makes y*Height+x == y*Width+x, masking the bug from all existing tests."
      },
      {
        "text": "That doesn't sound like it accounts for stride?",
        "source": "comment by jeremy-visionaid",
        "interpretation": "Valid secondary concern: for ROI/padded pixmaps where pixmap.RowBytes > Width*bpp, the typed span approach is inherently limited regardless of this fix. Pre-existing design gap, not in scope for this PR."
      }
    ],
    "workarounds": [
      "Use GetPixelSpan<T>() (zero-arg overload) which starts at offset 0 correctly, then manually slice: var info = pixmap.Info; var full = pixmap.GetPixelSpan<SKColor>(); if (!full.IsEmpty) { var span = full.Slice(y * info.Width + x); }",
      "Use pixmap.GetPixelColor(x, y) for read-only pixel access — always correct regardless of pixmap geometry."
    ],
    "nextQuestions": [
      "Should the stride concern for ROI/padded pixmaps (where RowBytes > Width*bpp) be addressed as a follow-up issue for GetPixelSpan<T>?",
      "The three test methods in SKPixmapTest.cs have the same y * info.Height + x typo in expectedLength — should these be fixed in the same PR?"
    ],
    "errorFingerprint": "SKPixmap.GetPixelSpan<T>(x,y):wrong-offset:height-instead-of-width",
    "resolution": {
      "hypothesis": "The row-major pixel offset formula uses Height where Width is required. Since pixel layout places Width pixels per row, the correct typed-unit offset is y * Width + x, not y * Height + x.",
      "proposals": [
        {
          "title": "Fix the offset formula in SKPixmap.cs",
          "description": "One-line change: replace info.Height with info.Width in the spanOffset calculation at line 162. Also update expectedLength in the three test methods that contain the same typo.",
          "category": "fix",
          "codeSnippet": "// binding/SkiaSharp/SKPixmap.cs line 162 — change:\nspanOffset = y * info.Height + x;\n// to:\nspanOffset = y * info.Width + x;",
          "confidence": 0.98,
          "effort": "cost/xs",
          "validated": "yes"
        },
        {
          "title": "Workaround: use zero-arg overload and manually slice",
          "description": "Until the fix ships, bypass the buggy 2-arg overload by fetching the full span at offset 0 (which is correct) and manually computing the row-major offset. Ensure pixmap is not empty before slicing.",
          "category": "workaround",
          "codeSnippet": "var info = pixmap.Info;\nvar fullSpan = pixmap.GetPixelSpan<SKColor>(); // zero-arg: starts at (0,0) correctly\nif (fullSpan.IsEmpty) return;\nint offset = y * info.Width + x; // correct row-major formula\nvar pixelSpan = fullSpan.Slice(offset);",
          "confidence": 0.9,
          "effort": "cost/xs",
          "validated": "yes"
        }
      ],
      "recommendedProposal": "Fix the offset formula in SKPixmap.cs",
      "recommendedReason": "One-line fix with zero risk. Root cause is unambiguous, confirmed by cross-check with the byte-typed path. Tests in SKPixmapTest.cs will need the same typo corrected and a non-square test case added to prevent future regression."
    }
  },
  "output": {
    "actionability": {
      "suggestedAction": "ready-to-fix",
      "confidence": 0.97,
      "reason": "Root cause is confirmed in source (SKPixmap.cs:162), fix is a one-line change, and the reporter (project maintainer) has fully specified the bug and fix.",
      "suggestedReproPlatform": "linux"
    },
    "actions": [
      {
        "type": "update-labels",
        "description": "Add tenet/reliability (type/bug and area/SkiaSharp are already applied)",
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
        "description": "Confirm root cause analysis, note test typos, and address the stride concern raised in comments",
        "risk": "medium",
        "confidence": 0.95,
        "comment": "Code investigation confirms the bug.\n\n**Root cause confirmed:** `binding/SkiaSharp/SKPixmap.cs:162` uses `y * info.Height + x` instead of `y * info.Width + x`. Row-major layout requires Width (columns per row) as the stride, not Height. Cross-check: the byte-typed path calls `GetPixelBytesOffset` which computes `y * RowBytes + x * bpp = (y * Width + x) * bpp` — confirming `y * Width + x` is the correct pixel-unit offset.\n\n**Why tests didn't catch it:** All three `GetPixelSpanWithOffset*` tests use `new SKImageInfo(3, 3, ...)` (square), and the `expectedLength` calculations contain the same typo: `(info.Width * info.Height) - (y * info.Height + x)`. For square images `Width == Height` so both formulas agree and tests pass. These test methods will need the same `Height → Width` correction plus a non-square test case.\n\n**Stride concern (@jeremy-visionaid):** Valid observation. For ROI or stride-padded pixmaps where `pixmap.RowBytes > info.Width * bpp`, the typed-span approach is inherently limited — `spanLength = info.Width * info.Height` under-counts elements and row boundaries misalign. This is a pre-existing design gap that applies regardless of this fix and is worth a separate issue.\n\n**Workaround until fix ships:** Use the zero-arg overload (unaffected) and manually slice:\n```csharp\nvar info = pixmap.Info;\nvar full = pixmap.GetPixelSpan<SKColor>();\nif (!full.IsEmpty)\n    return full.Slice(y * info.Width + x);\n```"
      }
    ]
  }
}
```

</details>
