# Issue Triage Report — #1355

| Field | Value |
|-------|-------|
| Repository | mono/SkiaSharp |
| Analyzed | 2026-04-23T10:54:23Z |
| Type | type/question (0.90 (90%)) |
| Area | area/SkiaSharp (0.95 (95%)) |
| Suggested action | close-as-fixed (0.95 (95%)) |

**Issue Summary:** Reporter asks whether the uint rowBytes parameter in SKMask.Create is intentional, noting inconsistency with other RowBytes properties (int) in the API surface.

**Analysis:** The reporter noticed that SKMask.Create used uint rowBytes while other RowBytes APIs (SKBitmap.RowBytes, SKImageInfo.RowBytes, SKPixmap.RowBytes) return int. The maintainer acknowledged the inconsistency and encouraged a PR to add an int overload. However, SKMask was completely removed in SkiaSharp 3.0.0 (changelogs/SkiaSharp/3.0.0/SkiaSharp.breaking.md), which resolves the inconsistency entirely.

**Recommendations:** **close-as-fixed** — SKMask was completely removed in SkiaSharp 3.0.0, eliminating the uint/int rowBytes inconsistency. The question is moot in current versions.

---

## Classification

| Field | Value |
|-------|-------|
| Type | type/question |
| Area | area/SkiaSharp |
| Platforms | — |
| Backends | — |
| Tenets | — |
| Partner | — |

## Evidence

### Reproduction

**Environment:** SkiaSharp 1.x/2.x (issue filed 2020-06-25); SKMask struct existed at the time

### Version Analysis

| Field | Value |
|-------|-------|
| Mentioned versions | — |
| Worked in | — |
| Broke in | — |
| Current relevance | unlikely |
| Relevance reason | SKMask and all its members were removed in SkiaSharp 3.0.0. The type no longer exists in current versions. |

### Fix Status

| Field | Value |
|-------|-------|
| Likely fixed | True |
| Confidence | 0.98 (98%) |
| Reason | SKMask was completely removed in SkiaSharp 3.0.0, making the uint/int rowBytes inconsistency moot. |
| Related PRs | — |
| Related commits | — |
| Fixed in version | 3.0.0 |

## Analysis

### Technical Summary

The reporter noticed that SKMask.Create used uint rowBytes while other RowBytes APIs (SKBitmap.RowBytes, SKImageInfo.RowBytes, SKPixmap.RowBytes) return int. The maintainer acknowledged the inconsistency and encouraged a PR to add an int overload. However, SKMask was completely removed in SkiaSharp 3.0.0 (changelogs/SkiaSharp/3.0.0/SkiaSharp.breaking.md), which resolves the inconsistency entirely.

### Rationale

This is a design question that was answered by the maintainer (not by design, int overload would be welcome) and subsequently resolved when SKMask was removed in 3.0.0. The API surface no longer contains SKMask or its uint rowBytes inconsistency.

### Key Signals

- "Probably not... We can add an int overload." — **comment by mattleibow (maintainer)** (Maintainer confirmed the uint was not intentional and a fix was planned.)
- "SKMask - All types and members relating to SKMask have been removed." — **changelogs/SkiaSharp/3.0.0/SkiaSharp.humanreadable.md line 79** (The entire SKMask type was removed in 3.0.0, making the original question moot.)
- "I am a bit sad that I make SKMask a struct and not a class like it should have been... I think this needs to change at some point." — **comment by mattleibow (2020-10-13)** (Maintainer was reconsidering the entire SKMask design, foreshadowing the eventual removal.)

### Code Investigation

| File | Lines | Relevance | Finding |
|------|-------|-----------|---------|
| `changelogs/SkiaSharp/3.0.0/SkiaSharp.humanreadable.md` | 79 | direct | SKMask and all its members were completely removed in SkiaSharp 3.0.0. The uint rowBytes inconsistency no longer exists in the current API surface. |
| `changelogs/SkiaSharp/1.68.2/SkiaSharp.humanreadable.md` | 146-153 | related | In 1.68.2, a ReadOnlySpan<byte> overload of SKMask.Create was added, but still using uint rowBytes — no int overload was added before removal. |
| `binding/SkiaSharp/SKImageInfo.cs` | 116 | context | SKImageInfo.RowBytes returns int, confirming the int convention used across the rest of the API surface. |

### Workarounds

- Upgrade to SkiaSharp 3.x where SKMask no longer exists. Functionality previously covered by SKMask (installing mask pixels) was removed as part of the 3.0 cleanup.

### Resolution Proposals

**Hypothesis:** The uint rowBytes in SKMask.Create was an oversight that was never corrected before SKMask was removed in 3.0.0.

1. **Close as fixed — SKMask removed in 3.0.0** — fix, confidence 0.95 (95%), cost/xs, validated=untested
   - The SKMask type (including the uint rowBytes parameter) was entirely removed in SkiaSharp 3.0.0. No further action is needed.

**Recommended proposal:** Close as fixed — SKMask removed in 3.0.0

**Why:** The type causing the question no longer exists in SkiaSharp 3.0.0+. The issue is resolved.

## Recommendations

### Actionability

| Field | Value |
|-------|-------|
| Suggested action | close-as-fixed |
| Confidence | 0.95 (95%) |
| Reason | SKMask was completely removed in SkiaSharp 3.0.0, eliminating the uint/int rowBytes inconsistency. The question is moot in current versions. |
| Suggested repro platform | linux |

### Automatable Actions

| Type | Risk | Confidence | Description | Details |
|------|------|------------|-------------|---------|
| update-labels | low | 0.95 (95%) | Apply question and area/SkiaSharp labels | labels=type/question, area/SkiaSharp |
| add-comment | medium | 0.95 (95%) | Inform reporter that SKMask was removed in 3.0.0, closing the inconsistency | — |
| close-issue | medium | 0.95 (95%) | Close as fixed — SKMask removed in 3.0.0 | stateReason=completed |

**Comment draft for `add-comment`:**

```markdown
Thanks for reporting this! The `uint rowBytes` inconsistency in `SKMask.Create` was indeed unintentional — as the maintainer noted at the time.

This is now moot: `SKMask` and all its members were **completely removed in SkiaSharp 3.0.0** as part of the API cleanup. If you're on SkiaSharp 3.x, you won't encounter this type at all.

Closing this as fixed.
```

<details>
<summary>Raw JSON</summary>

```json
{
  "meta": {
    "schemaVersion": "1.0",
    "number": 1355,
    "repo": "mono/SkiaSharp",
    "analyzedAt": "2026-04-23T10:54:23Z"
  },
  "summary": "Reporter asks whether the uint rowBytes parameter in SKMask.Create is intentional, noting inconsistency with other RowBytes properties (int) in the API surface.",
  "classification": {
    "type": {
      "value": "type/question",
      "confidence": 0.9
    },
    "area": {
      "value": "area/SkiaSharp",
      "confidence": 0.95
    }
  },
  "evidence": {
    "reproEvidence": {
      "environmentDetails": "SkiaSharp 1.x/2.x (issue filed 2020-06-25); SKMask struct existed at the time",
      "repoLinks": []
    },
    "fixStatus": {
      "likelyFixed": true,
      "confidence": 0.98,
      "reason": "SKMask was completely removed in SkiaSharp 3.0.0, making the uint/int rowBytes inconsistency moot.",
      "fixedInVersion": "3.0.0"
    },
    "versionAnalysis": {
      "mentionedVersions": [],
      "currentRelevance": "unlikely",
      "relevanceReason": "SKMask and all its members were removed in SkiaSharp 3.0.0. The type no longer exists in current versions."
    }
  },
  "analysis": {
    "summary": "The reporter noticed that SKMask.Create used uint rowBytes while other RowBytes APIs (SKBitmap.RowBytes, SKImageInfo.RowBytes, SKPixmap.RowBytes) return int. The maintainer acknowledged the inconsistency and encouraged a PR to add an int overload. However, SKMask was completely removed in SkiaSharp 3.0.0 (changelogs/SkiaSharp/3.0.0/SkiaSharp.breaking.md), which resolves the inconsistency entirely.",
    "rationale": "This is a design question that was answered by the maintainer (not by design, int overload would be welcome) and subsequently resolved when SKMask was removed in 3.0.0. The API surface no longer contains SKMask or its uint rowBytes inconsistency.",
    "keySignals": [
      {
        "text": "Probably not... We can add an int overload.",
        "source": "comment by mattleibow (maintainer)",
        "interpretation": "Maintainer confirmed the uint was not intentional and a fix was planned."
      },
      {
        "text": "SKMask - All types and members relating to SKMask have been removed.",
        "source": "changelogs/SkiaSharp/3.0.0/SkiaSharp.humanreadable.md line 79",
        "interpretation": "The entire SKMask type was removed in 3.0.0, making the original question moot."
      },
      {
        "text": "I am a bit sad that I make SKMask a struct and not a class like it should have been... I think this needs to change at some point.",
        "source": "comment by mattleibow (2020-10-13)",
        "interpretation": "Maintainer was reconsidering the entire SKMask design, foreshadowing the eventual removal."
      }
    ],
    "codeInvestigation": [
      {
        "file": "changelogs/SkiaSharp/3.0.0/SkiaSharp.humanreadable.md",
        "lines": "79",
        "finding": "SKMask and all its members were completely removed in SkiaSharp 3.0.0. The uint rowBytes inconsistency no longer exists in the current API surface.",
        "relevance": "direct"
      },
      {
        "file": "changelogs/SkiaSharp/1.68.2/SkiaSharp.humanreadable.md",
        "lines": "146-153",
        "finding": "In 1.68.2, a ReadOnlySpan<byte> overload of SKMask.Create was added, but still using uint rowBytes — no int overload was added before removal.",
        "relevance": "related"
      },
      {
        "file": "binding/SkiaSharp/SKImageInfo.cs",
        "lines": "116",
        "finding": "SKImageInfo.RowBytes returns int, confirming the int convention used across the rest of the API surface.",
        "relevance": "context"
      }
    ],
    "workarounds": [
      "Upgrade to SkiaSharp 3.x where SKMask no longer exists. Functionality previously covered by SKMask (installing mask pixels) was removed as part of the 3.0 cleanup."
    ],
    "resolution": {
      "hypothesis": "The uint rowBytes in SKMask.Create was an oversight that was never corrected before SKMask was removed in 3.0.0.",
      "proposals": [
        {
          "title": "Close as fixed — SKMask removed in 3.0.0",
          "description": "The SKMask type (including the uint rowBytes parameter) was entirely removed in SkiaSharp 3.0.0. No further action is needed.",
          "category": "fix",
          "confidence": 0.95,
          "effort": "cost/xs",
          "validated": "untested"
        }
      ],
      "recommendedProposal": "Close as fixed — SKMask removed in 3.0.0",
      "recommendedReason": "The type causing the question no longer exists in SkiaSharp 3.0.0+. The issue is resolved."
    }
  },
  "output": {
    "actionability": {
      "suggestedAction": "close-as-fixed",
      "confidence": 0.95,
      "reason": "SKMask was completely removed in SkiaSharp 3.0.0, eliminating the uint/int rowBytes inconsistency. The question is moot in current versions.",
      "suggestedReproPlatform": "linux"
    },
    "actions": [
      {
        "type": "update-labels",
        "description": "Apply question and area/SkiaSharp labels",
        "risk": "low",
        "confidence": 0.95,
        "labels": [
          "type/question",
          "area/SkiaSharp"
        ]
      },
      {
        "type": "add-comment",
        "description": "Inform reporter that SKMask was removed in 3.0.0, closing the inconsistency",
        "risk": "medium",
        "confidence": 0.95,
        "comment": "Thanks for reporting this! The `uint rowBytes` inconsistency in `SKMask.Create` was indeed unintentional — as the maintainer noted at the time.\n\nThis is now moot: `SKMask` and all its members were **completely removed in SkiaSharp 3.0.0** as part of the API cleanup. If you're on SkiaSharp 3.x, you won't encounter this type at all.\n\nClosing this as fixed."
      },
      {
        "type": "close-issue",
        "description": "Close as fixed — SKMask removed in 3.0.0",
        "risk": "medium",
        "confidence": 0.95,
        "stateReason": "completed"
      }
    ]
  }
}
```

</details>
