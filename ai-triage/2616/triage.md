# Issue Triage Report — #2616

| Field | Value |
|-------|-------|
| Repository | mono/SkiaSharp |
| Analyzed | 2026-04-25T22:18:25Z |
| Type | type/enhancement (0.98 (98%)) |
| Area | area/SkiaSharp (0.98 (98%)) |
| Suggested action | keep-open (0.95 (95%)) |

**Issue Summary:** Enhancement request to add Span<T> and ReadOnlySpan<T> overloads alongside existing array-based APIs in SkiaSharp's core binding, tracking partial progress across ~24 files as part of epic #2615.

**Analysis:** Issue #2616 is a well-defined tracking issue (part of epic #2615) to add Span<T>/ReadOnlySpan<T> overloads to SkiaSharp's core binding. About 4 of the 24 listed files have been addressed via PR #2617 (SKData, SKImage, SKPathEffect, SKRoundRect). The remaining ~20 files still expose only array-based public APIs (e.g., SKPath.GetPoints returns SKPoint[], SKMatrix.Values returns float[], SKFont.GetGlyphs returns ushort[], SKShader.CreateLinearGradient takes SKColor[]/float[]). Community interest is high with contributor @xoofx proposing a comprehensive PR in Feb 2025 referencing issue #2669.

**Recommendations:** **keep-open** — Valid, well-scoped enhancement with partial implementation done (PR #2617), active community interest (PR #2669), and clear remaining work tracked in checklist. Keep open as a tracking issue.

---

## Classification

| Field | Value |
|-------|-------|
| Type | type/enhancement |
| Area | area/SkiaSharp |
| Platforms | — |
| Backends | — |
| Tenets | tenet/performance |
| Partner | — |
| Current labels | type/enhancement, status/up-for-grabs, area/SkiaSharp, tenet/performance |

## Evidence

### Reproduction

**Repository links:**
- https://github.com/mono/SkiaSharp/issues/2615 — Parent epic: Use newer/better interop primitives
- https://github.com/mono/SkiaSharp/pull/2617 — PR implementing Span for SKData, SKImage, SKPathEffect, SKRoundRect
- https://github.com/mono/SkiaSharp/issues/2669 — Community follow-up PR mentioned as possibly covering remaining work

## Analysis

### Technical Summary

Issue #2616 is a well-defined tracking issue (part of epic #2615) to add Span<T>/ReadOnlySpan<T> overloads to SkiaSharp's core binding. About 4 of the 24 listed files have been addressed via PR #2617 (SKData, SKImage, SKPathEffect, SKRoundRect). The remaining ~20 files still expose only array-based public APIs (e.g., SKPath.GetPoints returns SKPoint[], SKMatrix.Values returns float[], SKFont.GetGlyphs returns ushort[], SKShader.CreateLinearGradient takes SKColor[]/float[]). Community interest is high with contributor @xoofx proposing a comprehensive PR in Feb 2025 referencing issue #2669.

### Rationale

This is a `type/enhancement` (not feature-request) because the underlying functionality exists — arrays work — this is about adding Span overloads to improve ergonomics and reduce allocations. Classified as `area/SkiaSharp` since all affected files are in the core binding. `tenet/performance` applies since Span eliminates unnecessary heap allocations. No platform-specific label since this is a cross-platform API surface issue. Action is `keep-open` as work is in progress via PR #2669 and this is an active tracking issue.

### Key Signals

- "Part of #2615 — The SkiaSharp codebase is quite old and existed before the introduction of spans." — **issue body** (This is a planned, tracked enhancement part of a broader interop modernization effort.)
- "PR #2617 already addressed SKData, SKImage, SKPathEffect, SKRoundRect" — **issue body checklist** (Active progress: ~4/24 files done, 20+ remain. Issue is still open and relevant.)
- "Would it be ok if I was making a single PR to bring Span for all existing APIs?" — **comment by xoofx** (External contributor actively interested in implementing this in a bulk PR — high community momentum as of Feb 2025.)
- "Actually #2669 is maybe covering what is left so I will followup with this issue." — **comment by xoofx** (Issue #2669 may partially or fully address remaining work — should be cross-referenced.)

### Code Investigation

| File | Lines | Relevance | Finding |
|------|-------|-----------|---------|
| `binding/SkiaSharp/SKPath.cs` | 73,123,164,171,402,501,507,515 | direct | SKPath exposes SKPoint[] in Points property, GetLine(), GetPoints(), AddPoly(), and ConvertConicToQuads(). No Span overloads exist yet. |
| `binding/SkiaSharp/SKMatrix.cs` | 30,70,97,334,350,386,402 | direct | SKMatrix uses float[] for Values property and constructor, and SKPoint[] for MapPoints/MapVectors. No Span overloads present. |
| `binding/SkiaSharp/SKFont.cs` | 123,146,149,156,391,394,401,461,481,484,491,551,571,574 | direct | SKFont returns ushort[], SKPoint[], and float[] from GetGlyphs, GetGlyphPositions, GetGlyphOffsets, GetGlyphWidths. Many accept ReadOnlySpan input already but still return arrays. |
| `binding/SkiaSharp/SKBitmap.cs` | 300,303,320,328,420,423,565,568,571,580 | related | SKBitmap already has ReadOnlySpan<byte> overloads for Decode/DecodeBounds alongside byte[] versions, and GetPixelSpan() returns Span<byte>. Partial migration already done here. |
| `binding/SkiaSharp/SKShader.cs` | 122,125,139,153,156,170,186,189,202,215,218,231,246,249,252 | direct | SKShader.CreateLinearGradient, CreateRadialGradient, and CreateSweepGradient all take SKColor[] and float[] parameters with no ReadOnlySpan overloads. |
| `binding/SkiaSharp/SKVertices.cs` | 25,30,35 | direct | SKVertices.CreateCopy takes SKPoint[], SKColor[], and UInt16[] parameters. No Span overloads present. |

### Resolution Proposals

**Hypothesis:** Progressively add ReadOnlySpan<T>/Span<T> overloads to each listed file while preserving existing array-based overloads for backward compatibility (ABI stability).

1. **Add Span overloads file-by-file** — fix, confidence 0.95 (95%), cost/l, validated=untested
   - For each file in the checklist, add ReadOnlySpan<T> input overloads and where practical add Span<T> output variants (or return Memory<T>). Keep existing array overloads — do NOT remove them.
2. **Track via PR #2669** — alternative, confidence 0.80 (80%), cost/s, validated=untested
   - Follow PR #2669 (referenced by @xoofx) which may already be covering remaining files. Review and merge when ready.

**Recommended proposal:** Add Span overloads file-by-file

**Why:** Incremental approach matches existing checklist structure and avoids risk of one large PR. PR #2669 may already deliver this but needs verification.

## Recommendations

### Actionability

| Field | Value |
|-------|-------|
| Suggested action | keep-open |
| Confidence | 0.95 (95%) |
| Reason | Valid, well-scoped enhancement with partial implementation done (PR #2617), active community interest (PR #2669), and clear remaining work tracked in checklist. Keep open as a tracking issue. |
| Suggested repro platform | linux |

### Automatable Actions

| Type | Risk | Confidence | Description | Details |
|------|------|------------|-------------|---------|
| update-labels | low | 0.98 (98%) | Labels already match classification — type/enhancement, area/SkiaSharp, tenet/performance are all set | labels=type/enhancement, area/SkiaSharp, tenet/performance |
| link-related | low | 0.95 (95%) | Cross-reference parent epic #2615 | linkedIssue=#2615 |

<details>
<summary>Raw JSON</summary>

```json
{
  "meta": {
    "schemaVersion": "1.0",
    "number": 2616,
    "repo": "mono/SkiaSharp",
    "analyzedAt": "2026-04-25T22:18:25Z",
    "currentLabels": [
      "type/enhancement",
      "status/up-for-grabs",
      "area/SkiaSharp",
      "tenet/performance"
    ]
  },
  "summary": "Enhancement request to add Span<T> and ReadOnlySpan<T> overloads alongside existing array-based APIs in SkiaSharp's core binding, tracking partial progress across ~24 files as part of epic #2615.",
  "classification": {
    "type": {
      "value": "type/enhancement",
      "confidence": 0.98
    },
    "area": {
      "value": "area/SkiaSharp",
      "confidence": 0.98
    },
    "tenets": [
      "tenet/performance"
    ]
  },
  "evidence": {
    "reproEvidence": {
      "repoLinks": [
        {
          "url": "https://github.com/mono/SkiaSharp/issues/2615",
          "description": "Parent epic: Use newer/better interop primitives"
        },
        {
          "url": "https://github.com/mono/SkiaSharp/pull/2617",
          "description": "PR implementing Span for SKData, SKImage, SKPathEffect, SKRoundRect"
        },
        {
          "url": "https://github.com/mono/SkiaSharp/issues/2669",
          "description": "Community follow-up PR mentioned as possibly covering remaining work"
        }
      ]
    }
  },
  "analysis": {
    "summary": "Issue #2616 is a well-defined tracking issue (part of epic #2615) to add Span<T>/ReadOnlySpan<T> overloads to SkiaSharp's core binding. About 4 of the 24 listed files have been addressed via PR #2617 (SKData, SKImage, SKPathEffect, SKRoundRect). The remaining ~20 files still expose only array-based public APIs (e.g., SKPath.GetPoints returns SKPoint[], SKMatrix.Values returns float[], SKFont.GetGlyphs returns ushort[], SKShader.CreateLinearGradient takes SKColor[]/float[]). Community interest is high with contributor @xoofx proposing a comprehensive PR in Feb 2025 referencing issue #2669.",
    "codeInvestigation": [
      {
        "file": "binding/SkiaSharp/SKPath.cs",
        "lines": "73,123,164,171,402,501,507,515",
        "finding": "SKPath exposes SKPoint[] in Points property, GetLine(), GetPoints(), AddPoly(), and ConvertConicToQuads(). No Span overloads exist yet.",
        "relevance": "direct"
      },
      {
        "file": "binding/SkiaSharp/SKMatrix.cs",
        "lines": "30,70,97,334,350,386,402",
        "finding": "SKMatrix uses float[] for Values property and constructor, and SKPoint[] for MapPoints/MapVectors. No Span overloads present.",
        "relevance": "direct"
      },
      {
        "file": "binding/SkiaSharp/SKFont.cs",
        "lines": "123,146,149,156,391,394,401,461,481,484,491,551,571,574",
        "finding": "SKFont returns ushort[], SKPoint[], and float[] from GetGlyphs, GetGlyphPositions, GetGlyphOffsets, GetGlyphWidths. Many accept ReadOnlySpan input already but still return arrays.",
        "relevance": "direct"
      },
      {
        "file": "binding/SkiaSharp/SKBitmap.cs",
        "lines": "300,303,320,328,420,423,565,568,571,580",
        "finding": "SKBitmap already has ReadOnlySpan<byte> overloads for Decode/DecodeBounds alongside byte[] versions, and GetPixelSpan() returns Span<byte>. Partial migration already done here.",
        "relevance": "related"
      },
      {
        "file": "binding/SkiaSharp/SKShader.cs",
        "lines": "122,125,139,153,156,170,186,189,202,215,218,231,246,249,252",
        "finding": "SKShader.CreateLinearGradient, CreateRadialGradient, and CreateSweepGradient all take SKColor[] and float[] parameters with no ReadOnlySpan overloads.",
        "relevance": "direct"
      },
      {
        "file": "binding/SkiaSharp/SKVertices.cs",
        "lines": "25,30,35",
        "finding": "SKVertices.CreateCopy takes SKPoint[], SKColor[], and UInt16[] parameters. No Span overloads present.",
        "relevance": "direct"
      }
    ],
    "keySignals": [
      {
        "text": "Part of #2615 — The SkiaSharp codebase is quite old and existed before the introduction of spans.",
        "source": "issue body",
        "interpretation": "This is a planned, tracked enhancement part of a broader interop modernization effort."
      },
      {
        "text": "PR #2617 already addressed SKData, SKImage, SKPathEffect, SKRoundRect",
        "source": "issue body checklist",
        "interpretation": "Active progress: ~4/24 files done, 20+ remain. Issue is still open and relevant."
      },
      {
        "text": "Would it be ok if I was making a single PR to bring Span for all existing APIs?",
        "source": "comment by xoofx",
        "interpretation": "External contributor actively interested in implementing this in a bulk PR — high community momentum as of Feb 2025."
      },
      {
        "text": "Actually #2669 is maybe covering what is left so I will followup with this issue.",
        "source": "comment by xoofx",
        "interpretation": "Issue #2669 may partially or fully address remaining work — should be cross-referenced."
      }
    ],
    "rationale": "This is a `type/enhancement` (not feature-request) because the underlying functionality exists — arrays work — this is about adding Span overloads to improve ergonomics and reduce allocations. Classified as `area/SkiaSharp` since all affected files are in the core binding. `tenet/performance` applies since Span eliminates unnecessary heap allocations. No platform-specific label since this is a cross-platform API surface issue. Action is `keep-open` as work is in progress via PR #2669 and this is an active tracking issue.",
    "resolution": {
      "hypothesis": "Progressively add ReadOnlySpan<T>/Span<T> overloads to each listed file while preserving existing array-based overloads for backward compatibility (ABI stability).",
      "proposals": [
        {
          "title": "Add Span overloads file-by-file",
          "description": "For each file in the checklist, add ReadOnlySpan<T> input overloads and where practical add Span<T> output variants (or return Memory<T>). Keep existing array overloads — do NOT remove them.",
          "category": "fix",
          "confidence": 0.95,
          "effort": "cost/l",
          "validated": "untested"
        },
        {
          "title": "Track via PR #2669",
          "description": "Follow PR #2669 (referenced by @xoofx) which may already be covering remaining files. Review and merge when ready.",
          "category": "alternative",
          "confidence": 0.8,
          "effort": "cost/s",
          "validated": "untested"
        }
      ],
      "recommendedProposal": "Add Span overloads file-by-file",
      "recommendedReason": "Incremental approach matches existing checklist structure and avoids risk of one large PR. PR #2669 may already deliver this but needs verification."
    }
  },
  "output": {
    "actionability": {
      "suggestedAction": "keep-open",
      "confidence": 0.95,
      "reason": "Valid, well-scoped enhancement with partial implementation done (PR #2617), active community interest (PR #2669), and clear remaining work tracked in checklist. Keep open as a tracking issue.",
      "suggestedReproPlatform": "linux"
    },
    "actions": [
      {
        "type": "update-labels",
        "description": "Labels already match classification — type/enhancement, area/SkiaSharp, tenet/performance are all set",
        "risk": "low",
        "confidence": 0.98,
        "labels": [
          "type/enhancement",
          "area/SkiaSharp",
          "tenet/performance"
        ]
      },
      {
        "type": "link-related",
        "description": "Cross-reference parent epic #2615",
        "risk": "low",
        "confidence": 0.95,
        "linkedIssue": 2615
      }
    ]
  }
}
```

</details>
