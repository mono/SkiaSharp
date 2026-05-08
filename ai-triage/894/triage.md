# Issue Triage Report — #894

| Field | Value |
|-------|-------|
| Repository | mono/SkiaSharp |
| Analyzed | 2026-05-01T09:15:00Z |
| Type | type/enhancement (0.90 (90%)) |
| Area | area/HarfBuzzSharp (0.95 (95%)) |
| Suggested action | close-as-fixed (0.82 (82%)) |

**Issue Summary:** Feature request to add exception guards and input validation checks to HarfBuzzSharp managed bindings to help developers detect invalid usage (wrong ContentType, Direction, buffer state) at runtime.

**Analysis:** This enhancement requested adding runtime validation guards for invalid HarfBuzz buffer usage patterns. The current codebase already implements these checks extensively across Buffer.cs and Font.cs.

**Recommendations:** **close-as-fixed** — Code investigation confirms all major validation guards requested (ContentType checks in Add/AddUtf8/AddUtf16/AddUtf32, GuessSegmentProperties, NormalizeGlyphs; Direction check in Font.Shape) are present in the current codebase. The enhancement appears to have been implemented since the issue was filed in 2019.

---

## Classification

| Field | Value |
|-------|-------|
| Type | type/enhancement |
| Area | area/HarfBuzzSharp |
| Platforms | — |
| Backends | — |
| Tenets | — |
| Partner | — |

## Evidence

### Reproduction

**Repository links:**
- https://github.com/mono/SkiaSharp/pull/891 — PR #891 which surfaced the need for validation guards in managed HarfBuzz bindings
- https://github.com/mono/SkiaSharp/issues/894#issuecomment-506553210 — Comment explaining expected buffer state transitions: ContentType and Direction through Add/GuessSegmentProperties/Shape lifecycle

### Fix Status

| Field | Value |
|-------|-------|
| Likely fixed | True |
| Confidence | 0.85 (85%) |
| Reason | Current Buffer.cs and Font.cs already contain extensive InvalidOperationException guards: Add/AddUtf8/AddUtf16/AddUtf32 throw if ContentType is wrong, GuessSegmentProperties throws if ContentType != Unicode, NormalizeGlyphs throws if ContentType != Glyphs, Font.Shape throws if Direction == Invalid or ContentType != Unicode. |
| Related PRs | — |
| Related commits | — |
| Fixed in version | — |

## Analysis

### Technical Summary

This enhancement requested adding runtime validation guards for invalid HarfBuzz buffer usage patterns. The current codebase already implements these checks extensively across Buffer.cs and Font.cs.

### Rationale

The issue requests exceptions for invalid state (wrong ContentType, invalid Direction, empty buffer). Investigation of the current source shows these guards have already been added: Buffer.Add/AddUtf8/AddUtf16/AddUtf32 all throw InvalidOperationException for ContentType violations, GuessSegmentProperties checks ContentType, Font.Shape validates Direction and ContentType. This strongly suggests the enhancement was implemented after filing.

### Key Signals

- "Add some exceptions and other checks to help out developers." — **issue body** (Request to add defensive exception guards for invalid HarfBuzz buffer usage.)
- "GuessSegmentProperties does not guarantee everything is configured right but will work most of the time." — **comment #1** (Caller should set Direction/ContentType explicitly; validation helps surface misconfiguration.)

### Code Investigation

| File | Lines | Relevance | Finding |
|------|-------|-----------|---------|
| `binding/HarfBuzzSharp/Buffer.cs` | 94-99 | direct | Buffer.Add() throws InvalidOperationException if ContentType is not Unicode or is Glyphs — validation guard is in place. |
| `binding/HarfBuzzSharp/Buffer.cs` | 258-262 | direct | Buffer.GuessSegmentProperties() throws InvalidOperationException if ContentType != Unicode — requested check is implemented. |
| `binding/HarfBuzzSharp/Font.cs` | 362-369 | direct | Font.Shape() throws if buffer.Direction == Invalid and if ContentType != Unicode — key validation requested in issue is present. |
| `binding/HarfBuzzSharp/Buffer.cs` | 281-289 | related | Buffer.NormalizeGlyphs() throws if ContentType != Glyphs or GlyphPositions is empty. |

### Next Questions

- Are there any remaining asserts from the native HarfBuzz C code that have not yet been replicated in the managed layer?

## Recommendations

### Actionability

| Field | Value |
|-------|-------|
| Suggested action | close-as-fixed |
| Confidence | 0.82 (82%) |
| Reason | Code investigation confirms all major validation guards requested (ContentType checks in Add/AddUtf8/AddUtf16/AddUtf32, GuessSegmentProperties, NormalizeGlyphs; Direction check in Font.Shape) are present in the current codebase. The enhancement appears to have been implemented since the issue was filed in 2019. |
| Suggested repro platform | linux |

### Automatable Actions

| Type | Risk | Confidence | Description | Details |
|------|------|------------|-------------|---------|
| update-labels | low | 0.90 (90%) | Apply enhancement and area labels | labels=type/enhancement, area/HarfBuzzSharp |
| add-comment | high | 0.82 (82%) | Post analysis noting validation guards are already implemented | — |
| close-issue | medium | 0.82 (82%) | Close as implemented/fixed | stateReason=completed |

**Comment draft for `add-comment`:**

```markdown
After reviewing the current source, the validation guards requested in this issue appear to have been implemented:

- `Buffer.Add()`, `AddUtf8()`, `AddUtf16()`, `AddUtf32()`, `AddCodepoints()` all throw `InvalidOperationException` when `ContentType` is invalid.
- `Buffer.GuessSegmentProperties()` throws if `ContentType != Unicode`.
- `Buffer.NormalizeGlyphs()` throws if `ContentType != Glyphs`.
- `Font.Shape()` throws if `Direction == Invalid` or `ContentType != Unicode`.

If there are specific asserts from the native HarfBuzz C code that are still not covered, please let us know and we can reopen.
```

<details>
<summary>Raw JSON</summary>

```json
{
  "meta": {
    "schemaVersion": "1.0",
    "number": 894,
    "repo": "mono/SkiaSharp",
    "analyzedAt": "2026-05-01T09:15:00Z"
  },
  "summary": "Feature request to add exception guards and input validation checks to HarfBuzzSharp managed bindings to help developers detect invalid usage (wrong ContentType, Direction, buffer state) at runtime.",
  "classification": {
    "type": {
      "value": "type/enhancement",
      "confidence": 0.9
    },
    "area": {
      "value": "area/HarfBuzzSharp",
      "confidence": 0.95
    }
  },
  "evidence": {
    "reproEvidence": {
      "repoLinks": [
        {
          "url": "https://github.com/mono/SkiaSharp/pull/891",
          "description": "PR #891 which surfaced the need for validation guards in managed HarfBuzz bindings"
        },
        {
          "url": "https://github.com/mono/SkiaSharp/issues/894#issuecomment-506553210",
          "description": "Comment explaining expected buffer state transitions: ContentType and Direction through Add/GuessSegmentProperties/Shape lifecycle"
        }
      ]
    },
    "fixStatus": {
      "likelyFixed": true,
      "confidence": 0.85,
      "reason": "Current Buffer.cs and Font.cs already contain extensive InvalidOperationException guards: Add/AddUtf8/AddUtf16/AddUtf32 throw if ContentType is wrong, GuessSegmentProperties throws if ContentType != Unicode, NormalizeGlyphs throws if ContentType != Glyphs, Font.Shape throws if Direction == Invalid or ContentType != Unicode."
    }
  },
  "analysis": {
    "summary": "This enhancement requested adding runtime validation guards for invalid HarfBuzz buffer usage patterns. The current codebase already implements these checks extensively across Buffer.cs and Font.cs.",
    "rationale": "The issue requests exceptions for invalid state (wrong ContentType, invalid Direction, empty buffer). Investigation of the current source shows these guards have already been added: Buffer.Add/AddUtf8/AddUtf16/AddUtf32 all throw InvalidOperationException for ContentType violations, GuessSegmentProperties checks ContentType, Font.Shape validates Direction and ContentType. This strongly suggests the enhancement was implemented after filing.",
    "keySignals": [
      {
        "text": "Add some exceptions and other checks to help out developers.",
        "source": "issue body",
        "interpretation": "Request to add defensive exception guards for invalid HarfBuzz buffer usage."
      },
      {
        "text": "GuessSegmentProperties does not guarantee everything is configured right but will work most of the time.",
        "source": "comment #1",
        "interpretation": "Caller should set Direction/ContentType explicitly; validation helps surface misconfiguration."
      }
    ],
    "codeInvestigation": [
      {
        "file": "binding/HarfBuzzSharp/Buffer.cs",
        "lines": "94-99",
        "finding": "Buffer.Add() throws InvalidOperationException if ContentType is not Unicode or is Glyphs — validation guard is in place.",
        "relevance": "direct"
      },
      {
        "file": "binding/HarfBuzzSharp/Buffer.cs",
        "lines": "258-262",
        "finding": "Buffer.GuessSegmentProperties() throws InvalidOperationException if ContentType != Unicode — requested check is implemented.",
        "relevance": "direct"
      },
      {
        "file": "binding/HarfBuzzSharp/Font.cs",
        "lines": "362-369",
        "finding": "Font.Shape() throws if buffer.Direction == Invalid and if ContentType != Unicode — key validation requested in issue is present.",
        "relevance": "direct"
      },
      {
        "file": "binding/HarfBuzzSharp/Buffer.cs",
        "lines": "281-289",
        "finding": "Buffer.NormalizeGlyphs() throws if ContentType != Glyphs or GlyphPositions is empty.",
        "relevance": "related"
      }
    ],
    "nextQuestions": [
      "Are there any remaining asserts from the native HarfBuzz C code that have not yet been replicated in the managed layer?"
    ]
  },
  "output": {
    "actionability": {
      "suggestedAction": "close-as-fixed",
      "confidence": 0.82,
      "reason": "Code investigation confirms all major validation guards requested (ContentType checks in Add/AddUtf8/AddUtf16/AddUtf32, GuessSegmentProperties, NormalizeGlyphs; Direction check in Font.Shape) are present in the current codebase. The enhancement appears to have been implemented since the issue was filed in 2019.",
      "suggestedReproPlatform": "linux"
    },
    "actions": [
      {
        "type": "update-labels",
        "description": "Apply enhancement and area labels",
        "risk": "low",
        "confidence": 0.9,
        "labels": [
          "type/enhancement",
          "area/HarfBuzzSharp"
        ]
      },
      {
        "type": "add-comment",
        "description": "Post analysis noting validation guards are already implemented",
        "risk": "high",
        "confidence": 0.82,
        "comment": "After reviewing the current source, the validation guards requested in this issue appear to have been implemented:\n\n- `Buffer.Add()`, `AddUtf8()`, `AddUtf16()`, `AddUtf32()`, `AddCodepoints()` all throw `InvalidOperationException` when `ContentType` is invalid.\n- `Buffer.GuessSegmentProperties()` throws if `ContentType != Unicode`.\n- `Buffer.NormalizeGlyphs()` throws if `ContentType != Glyphs`.\n- `Font.Shape()` throws if `Direction == Invalid` or `ContentType != Unicode`.\n\nIf there are specific asserts from the native HarfBuzz C code that are still not covered, please let us know and we can reopen."
      },
      {
        "type": "close-issue",
        "description": "Close as implemented/fixed",
        "risk": "medium",
        "confidence": 0.82,
        "stateReason": "completed"
      }
    ]
  }
}
```

</details>
