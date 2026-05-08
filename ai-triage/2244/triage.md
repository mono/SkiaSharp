# Issue Triage Report — #2244

| Field | Value |
|-------|-------|
| Repository | mono/SkiaSharp |
| Analyzed | 2026-04-28T14:30:46Z |
| Type | type/bug (0.88 (88%)) |
| Area | area/SkiaSharp (0.95 (95%)) |
| Suggested action | ready-to-fix (0.88 (88%)) |

**Issue Summary:** SKBitmap.ByteCount truncates the native size_t byte count to a 32-bit int, causing OverflowException for images larger than 2 GB; reporter requests a long-returning alternative.

**Analysis:** SKBitmap.ByteCount is declared as int and wraps sk_bitmap_get_byte_count which returns size_t (IntPtr). The (int) cast overflows for images larger than 2 GB. SKImageInfo already exposes BytesSize64 returning long as a pattern; the fix is to add a ByteCount64 (long) property on SKBitmap while keeping the existing int property for ABI safety.

**Recommendations:** **ready-to-fix** — Root cause is confirmed in code (int cast of size_t). Fix path is clear: add ByteCount64 returning long. Workaround exists. No repro project needed.

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

1. Create an SKBitmap with Width * Height * BytesPerPixel exceeding int.MaxValue (e.g. 32768 x 32768 x RGBA8888 = 4 GB)
2. Access bitmap.ByteCount
3. Observe OverflowException (or silent truncation if unchecked arithmetic)

**Environment:** Any platform; no specific OS/arch requirement — pure integer overflow on any 64-bit system

### Bug Signals

| Field | Value |
|-------|-------|
| Severity | medium |
| Regression claimed | False |
| Error type | exception |
| Error message | OverflowException when casting size_t to int in SKBitmap.ByteCount for images >2 GB |
| Repro quality | partial |
| Target frameworks | — |

### Version Analysis

| Field | Value |
|-------|-------|
| Mentioned versions | — |
| Worked in | — |
| Broke in | — |
| Current relevance | likely |
| Relevance reason | Code inspection confirms the cast to int is still present in the current codebase. |

## Analysis

### Technical Summary

SKBitmap.ByteCount is declared as int and wraps sk_bitmap_get_byte_count which returns size_t (IntPtr). The (int) cast overflows for images larger than 2 GB. SKImageInfo already exposes BytesSize64 returning long as a pattern; the fix is to add a ByteCount64 (long) property on SKBitmap while keeping the existing int property for ABI safety.

### Rationale

The overflow is confirmed by direct code inspection: sk_bitmap_get_byte_count returns IntPtr (size_t) and is immediately cast to int with no checked guard. The maintainer acknowledged the issue and pointed to bitmap.Info.BytesSize64 as a temporary workaround, which confirms this is a real limitation. Changing ByteCount return type from int to long would be ABI-breaking, so the fix requires an additive ByteCount64 property. This is classified as type/bug because the current API actively throws or silently corrupts data for valid large-image usage.

### Key Signals

- "public int ByteCount { get { return (int)SkiaApi.sk_bitmap_get_byte_count(Handle); } }" — **binding/SkiaSharp/SKBitmap.cs line 291-293** (Direct (int) cast of size_t result; overflows silently or throws for images >2 GB.)
- "Note: this truncates the result to 32-bits." — **XML doc comment on ByteCount (quoted in issue body)** (The truncation is documented but not guarded, making it a known-broken API for large images.)
- "Maybe not the same code, but also to avoid the crash for now you can use bitmap.Info.ByteSize64" — **maintainer comment #1241047305** (Maintainer confirmed the limitation and offered BytesSize64 (actual property name) as workaround.)

### Code Investigation

| File | Lines | Relevance | Finding |
|------|-------|-----------|---------|
| `binding/SkiaSharp/SKBitmap.cs` | 291-293 | direct | ByteCount getter casts the IntPtr (size_t) return of sk_bitmap_get_byte_count to int. For images where width*height*bpp > 2147483647, this overflows. |
| `binding/SkiaSharp/SKImageInfo.cs` | 112-114 | related | SKImageInfo provides both BytesSize (int) and BytesSize64 (long). This established pattern of pairing 32-bit and 64-bit variants is the correct fix model for SKBitmap.ByteCount. |
| `binding/SkiaSharp/SkiaApi.generated.cs` | 1607-1623 | direct | sk_bitmap_get_byte_count is mapped as returning IntPtr (size_t). The native type is large enough; the overflow is entirely in the C# cast. |
| `binding/SkiaSharp/SKBitmap.cs` | 300-301 | related | GetPixelSpan() also casts the IntPtr pixel length to int for Span<byte>. Span<T> is limited to int.MaxValue elements so this is a separate but related limitation. |

### Workarounds

- Use bitmap.Info.BytesSize64 (returns long) to get the unclamped byte count without overflow.
- Compute manually: (long)bitmap.Width * bitmap.Height * bitmap.BytesPerPixel

### Next Questions

- Should a ByteCount64 property be added to SKBitmap to mirror SKImageInfo.BytesSize64?
- Are there other SKBitmap members (RowBytes, GetPixelSpan) that need 64-bit variants for large-image support?

### Resolution Proposals

**Hypothesis:** The fix is additive: add a ByteCount64 property returning long that casts to (long) instead of (int), following the BytesSize / BytesSize64 pattern in SKImageInfo.

1. **Add SKBitmap.ByteCount64 property** — fix, confidence 0.90 (90%), cost/xs, validated=untested
   - Add a new long-returning property ByteCount64 that performs the (long) cast instead of (int), keeping the existing ByteCount int property to preserve ABI.
2. **Use workaround bitmap.Info.BytesSize64** — workaround, confidence 0.95 (95%), cost/xs, validated=yes
   - Access bitmap.Info.BytesSize64 which returns long via SKImageInfo and is already available.

**Recommended proposal:** Add SKBitmap.ByteCount64 property

**Why:** Follows the established SKImageInfo pattern; ABI-safe additive change; gives callers a first-class API without requiring knowledge of SKImageInfo.

## Recommendations

### Actionability

| Field | Value |
|-------|-------|
| Suggested action | ready-to-fix |
| Confidence | 0.88 (88%) |
| Reason | Root cause is confirmed in code (int cast of size_t). Fix path is clear: add ByteCount64 returning long. Workaround exists. No repro project needed. |
| Suggested repro platform | linux |

### Automatable Actions

| Type | Risk | Confidence | Description | Details |
|------|------|------------|-------------|---------|
| update-labels | low | 0.95 (95%) | Apply type/bug, area/SkiaSharp, tenet/reliability labels | labels=type/bug, area/SkiaSharp, tenet/reliability |
| add-comment | medium | 0.88 (88%) | Acknowledge bug, confirm workaround, and outline fix path | — |

**Comment draft for `add-comment`:**

```markdown
Thanks for the report! Confirmed: `SKBitmap.ByteCount` casts the native `size_t` result to `int`, which overflows for images larger than 2 GB.

**Workaround (available now):** use `bitmap.Info.BytesSize64` which returns `long` and won't overflow.

**Fix plan:** We'll add a `SKBitmap.ByteCount64` property returning `long`, following the same pattern as `SKImageInfo.BytesSize` / `BytesSize64`. The existing `ByteCount` property must remain `int`-returning to preserve ABI compatibility.
```

<details>
<summary>Raw JSON</summary>

```json
{
  "meta": {
    "schemaVersion": "1.0",
    "number": 2244,
    "repo": "mono/SkiaSharp",
    "analyzedAt": "2026-04-28T14:30:46Z",
    "currentLabels": [
      "type/bug",
      "area/SkiaSharp"
    ]
  },
  "summary": "SKBitmap.ByteCount truncates the native size_t byte count to a 32-bit int, causing OverflowException for images larger than 2 GB; reporter requests a long-returning alternative.",
  "classification": {
    "type": {
      "value": "type/bug",
      "confidence": 0.88
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
      "errorType": "exception",
      "errorMessage": "OverflowException when casting size_t to int in SKBitmap.ByteCount for images >2 GB",
      "reproQuality": "partial",
      "targetFrameworks": []
    },
    "reproEvidence": {
      "stepsToReproduce": [
        "Create an SKBitmap with Width * Height * BytesPerPixel exceeding int.MaxValue (e.g. 32768 x 32768 x RGBA8888 = 4 GB)",
        "Access bitmap.ByteCount",
        "Observe OverflowException (or silent truncation if unchecked arithmetic)"
      ],
      "environmentDetails": "Any platform; no specific OS/arch requirement — pure integer overflow on any 64-bit system"
    },
    "versionAnalysis": {
      "mentionedVersions": [],
      "currentRelevance": "likely",
      "relevanceReason": "Code inspection confirms the cast to int is still present in the current codebase."
    }
  },
  "analysis": {
    "summary": "SKBitmap.ByteCount is declared as int and wraps sk_bitmap_get_byte_count which returns size_t (IntPtr). The (int) cast overflows for images larger than 2 GB. SKImageInfo already exposes BytesSize64 returning long as a pattern; the fix is to add a ByteCount64 (long) property on SKBitmap while keeping the existing int property for ABI safety.",
    "rationale": "The overflow is confirmed by direct code inspection: sk_bitmap_get_byte_count returns IntPtr (size_t) and is immediately cast to int with no checked guard. The maintainer acknowledged the issue and pointed to bitmap.Info.BytesSize64 as a temporary workaround, which confirms this is a real limitation. Changing ByteCount return type from int to long would be ABI-breaking, so the fix requires an additive ByteCount64 property. This is classified as type/bug because the current API actively throws or silently corrupts data for valid large-image usage.",
    "keySignals": [
      {
        "text": "public int ByteCount { get { return (int)SkiaApi.sk_bitmap_get_byte_count(Handle); } }",
        "source": "binding/SkiaSharp/SKBitmap.cs line 291-293",
        "interpretation": "Direct (int) cast of size_t result; overflows silently or throws for images >2 GB."
      },
      {
        "text": "Note: this truncates the result to 32-bits.",
        "source": "XML doc comment on ByteCount (quoted in issue body)",
        "interpretation": "The truncation is documented but not guarded, making it a known-broken API for large images."
      },
      {
        "text": "Maybe not the same code, but also to avoid the crash for now you can use bitmap.Info.ByteSize64",
        "source": "maintainer comment #1241047305",
        "interpretation": "Maintainer confirmed the limitation and offered BytesSize64 (actual property name) as workaround."
      }
    ],
    "codeInvestigation": [
      {
        "file": "binding/SkiaSharp/SKBitmap.cs",
        "lines": "291-293",
        "finding": "ByteCount getter casts the IntPtr (size_t) return of sk_bitmap_get_byte_count to int. For images where width*height*bpp > 2147483647, this overflows.",
        "relevance": "direct"
      },
      {
        "file": "binding/SkiaSharp/SKImageInfo.cs",
        "lines": "112-114",
        "finding": "SKImageInfo provides both BytesSize (int) and BytesSize64 (long). This established pattern of pairing 32-bit and 64-bit variants is the correct fix model for SKBitmap.ByteCount.",
        "relevance": "related"
      },
      {
        "file": "binding/SkiaSharp/SkiaApi.generated.cs",
        "lines": "1607-1623",
        "finding": "sk_bitmap_get_byte_count is mapped as returning IntPtr (size_t). The native type is large enough; the overflow is entirely in the C# cast.",
        "relevance": "direct"
      },
      {
        "file": "binding/SkiaSharp/SKBitmap.cs",
        "lines": "300-301",
        "finding": "GetPixelSpan() also casts the IntPtr pixel length to int for Span<byte>. Span<T> is limited to int.MaxValue elements so this is a separate but related limitation.",
        "relevance": "related"
      }
    ],
    "workarounds": [
      "Use bitmap.Info.BytesSize64 (returns long) to get the unclamped byte count without overflow.",
      "Compute manually: (long)bitmap.Width * bitmap.Height * bitmap.BytesPerPixel"
    ],
    "nextQuestions": [
      "Should a ByteCount64 property be added to SKBitmap to mirror SKImageInfo.BytesSize64?",
      "Are there other SKBitmap members (RowBytes, GetPixelSpan) that need 64-bit variants for large-image support?"
    ],
    "resolution": {
      "hypothesis": "The fix is additive: add a ByteCount64 property returning long that casts to (long) instead of (int), following the BytesSize / BytesSize64 pattern in SKImageInfo.",
      "proposals": [
        {
          "title": "Add SKBitmap.ByteCount64 property",
          "description": "Add a new long-returning property ByteCount64 that performs the (long) cast instead of (int), keeping the existing ByteCount int property to preserve ABI.",
          "category": "fix",
          "confidence": 0.9,
          "effort": "cost/xs",
          "validated": "untested"
        },
        {
          "title": "Use workaround bitmap.Info.BytesSize64",
          "description": "Access bitmap.Info.BytesSize64 which returns long via SKImageInfo and is already available.",
          "category": "workaround",
          "confidence": 0.95,
          "effort": "cost/xs",
          "validated": "yes"
        }
      ],
      "recommendedProposal": "Add SKBitmap.ByteCount64 property",
      "recommendedReason": "Follows the established SKImageInfo pattern; ABI-safe additive change; gives callers a first-class API without requiring knowledge of SKImageInfo."
    }
  },
  "output": {
    "actionability": {
      "suggestedAction": "ready-to-fix",
      "confidence": 0.88,
      "reason": "Root cause is confirmed in code (int cast of size_t). Fix path is clear: add ByteCount64 returning long. Workaround exists. No repro project needed.",
      "suggestedReproPlatform": "linux"
    },
    "actions": [
      {
        "type": "update-labels",
        "description": "Apply type/bug, area/SkiaSharp, tenet/reliability labels",
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
        "description": "Acknowledge bug, confirm workaround, and outline fix path",
        "risk": "medium",
        "confidence": 0.88,
        "comment": "Thanks for the report! Confirmed: `SKBitmap.ByteCount` casts the native `size_t` result to `int`, which overflows for images larger than 2 GB.\n\n**Workaround (available now):** use `bitmap.Info.BytesSize64` which returns `long` and won't overflow.\n\n**Fix plan:** We'll add a `SKBitmap.ByteCount64` property returning `long`, following the same pattern as `SKImageInfo.BytesSize` / `BytesSize64`. The existing `ByteCount` property must remain `int`-returning to preserve ABI compatibility."
      }
    ]
  }
}
```

</details>
