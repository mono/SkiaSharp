# Issue Triage Report — #2544

| Field | Value |
|-------|-------|
| Repository | mono/SkiaSharp |
| Analyzed | 2026-05-02T06:22:15Z |
| Type | type/enhancement (0.85 (85%)) |
| Area | area/SkiaSharp (0.90 (90%)) |
| Suggested action | keep-open (0.80 (80%)) |

**Issue Summary:** Maintainer tracking issue to review whether the removal of all [Obsolete] members in PR #2539 was too aggressive for the SkiaSharp 3.x breaking-changes cycle.

**Analysis:** This is a maintainer-filed tracking issue requesting a review of the SkiaSharp 3.x API-breaking changes, specifically whether PR #2539 (which removed all [Obsolete] APIs) was too aggressive. The 3.0.0 changelog documents ABI breaking changes (SKImageFilter.CropRect→SKRect, SKMatrix44 struct rewrite, SK3dView removal, SKRuntimeEffect rewrite) and lists removed Obsolete members. The current codebase still carries new [Obsolete] attributes in SKPaint.cs and other files intended for future removal cycles, suggesting the pattern continues. No explicit sign that the review concern from the issue was formally resolved.

**Recommendations:** **keep-open** — Valid maintainer tracking issue about 3.x API compatibility. No evidence it was resolved; the 3.0 changelog exists but the specific review concern (was deletion too aggressive?) has no recorded outcome.

---

## Classification

| Field | Value |
|-------|-------|
| Type | type/enhancement |
| Area | area/SkiaSharp |
| Platforms | — |
| Backends | — |
| Tenets | tenet/compatibility |
| Partner | — |

## Evidence

### Reproduction

**Repository links:**
- https://github.com/mono/SkiaSharp/pull/2539 — PR that removed all [Obsolete] members as part of 3.x cleanup
- https://github.com/mono/SkiaSharp/issues/2541 — Referenced issue fixed by PR #2539

### Version Analysis

| Field | Value |
|-------|-------|
| Mentioned versions | 3.0 |
| Worked in | — |
| Broke in | — |
| Current relevance | likely |
| Relevance reason | The 3.x branch is shipping; the review of whether deletions were too aggressive is still open with no resolution comment. |

## Analysis

### Technical Summary

This is a maintainer-filed tracking issue requesting a review of the SkiaSharp 3.x API-breaking changes, specifically whether PR #2539 (which removed all [Obsolete] APIs) was too aggressive. The 3.0.0 changelog documents ABI breaking changes (SKImageFilter.CropRect→SKRect, SKMatrix44 struct rewrite, SK3dView removal, SKRuntimeEffect rewrite) and lists removed Obsolete members. The current codebase still carries new [Obsolete] attributes in SKPaint.cs and other files intended for future removal cycles, suggesting the pattern continues. No explicit sign that the review concern from the issue was formally resolved.

### Rationale

The issue is filed by the maintainer as a self-assigned review task, not a user-reported bug. It requests evaluation of backward-compatibility impact of the 3.x cleanup in PR #2539. Classifying as type/enhancement (improving library compatibility story) rather than type/bug because no broken behavior is described. The tenet/compatibility label applies because this is explicitly about API stability and migration cost.

### Key Signals

- "Even as part of #2539 there may have been a too-aggressive delete." — **issue body** (Maintainer concern that some deleted APIs were still useful or lacked adequate replacements at the time of removal.)

### Code Investigation

| File | Lines | Relevance | Finding |
|------|-------|-----------|---------|
| `changelogs/SkiaSharp/3.0.0/SkiaSharp.humanreadable.md` | — | direct | The 3.0.0 changelog documents ABI breaking changes including: SKImageFilter.CropRect replaced by SKRect, SKMatrix44 rewritten as struct, SK3dView removed, SKRuntimeEffect rewritten. A separate section lists removed [Obsolete] types and members. This confirms the scope of deletions in PR #2539 was documented. |
| `binding/SkiaSharp/SKPaint.cs` | — | related | SKPaint.cs still carries multiple [Obsolete] attributes (e.g. FilterQuality→SKSamplingOptions, text-related APIs→SKFont). These are newly marked in 3.x, not yet removed — showing the library continues to use the phase-out pattern. Reviewing the 3.x deletions may inform which newly-obsoleted APIs need clearer migration paths. |

### Next Questions

- Which specific [Obsolete] members removed in PR #2539 lacked direct replacements?
- Has any consumer feedback or downstream issue reported pain from the 3.x removals?
- Does the v4 release plan (#3681, #3682) supersede this review concern?

### Resolution Proposals

**Hypothesis:** Some APIs removed in PR #2539 may have had no clear migration path documented, making the 3.x upgrade harder. A focused diff of what was deleted vs what had documented replacements would close this issue.

1. **Audit PR #2539 deletions against the 3.0 changelog** — investigation, confidence 0.80 (80%), cost/s, validated=untested
   - Compare the APIs removed in PR #2539 against the 'Removed Obsolete' section of the 3.0 changelog to identify any deletions without documented replacement APIs. File follow-up issues for any gaps found.
2. **Add migration notes to 3.0.0 changelog for under-documented removals** — fix, confidence 0.75 (75%), cost/m, validated=untested
   - For any removed API that lacks a documented replacement, add a migration note to the 3.0.0 changelog and/or migration guide to reduce consumer friction.

**Recommended proposal:** Audit PR #2539 deletions against the 3.0 changelog

**Why:** Low-effort investigation to close the open question before any docs work begins.

## Recommendations

### Actionability

| Field | Value |
|-------|-------|
| Suggested action | keep-open |
| Confidence | 0.80 (80%) |
| Reason | Valid maintainer tracking issue about 3.x API compatibility. No evidence it was resolved; the 3.0 changelog exists but the specific review concern (was deletion too aggressive?) has no recorded outcome. |
| Suggested repro platform | linux |

### Automatable Actions

| Type | Risk | Confidence | Description | Details |
|------|------|------------|-------------|---------|
| update-labels | low | 0.90 (90%) | Apply enhancement, SkiaSharp area, and compatibility tenet labels | labels=type/enhancement, area/SkiaSharp, tenet/compatibility |
| add-comment | medium | 0.80 (80%) | Post analysis noting the 3.0 changelog documents the removals and proposing an audit | — |

**Comment draft for `add-comment`:**

```markdown
The 3.0.0 changelog documents the ABI breaking changes and removed [Obsolete] members from PR #2539. To close this review, it may be worth doing a quick audit of the deleted APIs against the changelog to identify any removals that lacked a documented migration path. If any gaps are found, adding migration notes to the changelog or migration guide would help consumers upgrading from 2.x.
```

<details>
<summary>Raw JSON</summary>

```json
{
  "meta": {
    "schemaVersion": "1.0",
    "number": 2544,
    "repo": "mono/SkiaSharp",
    "analyzedAt": "2026-05-02T06:22:15Z"
  },
  "summary": "Maintainer tracking issue to review whether the removal of all [Obsolete] members in PR #2539 was too aggressive for the SkiaSharp 3.x breaking-changes cycle.",
  "classification": {
    "type": {
      "value": "type/enhancement",
      "confidence": 0.85
    },
    "area": {
      "value": "area/SkiaSharp",
      "confidence": 0.9
    },
    "tenets": [
      "tenet/compatibility"
    ]
  },
  "evidence": {
    "reproEvidence": {
      "repoLinks": [
        {
          "url": "https://github.com/mono/SkiaSharp/pull/2539",
          "description": "PR that removed all [Obsolete] members as part of 3.x cleanup"
        },
        {
          "url": "https://github.com/mono/SkiaSharp/issues/2541",
          "description": "Referenced issue fixed by PR #2539"
        }
      ]
    },
    "versionAnalysis": {
      "mentionedVersions": [
        "3.0"
      ],
      "currentRelevance": "likely",
      "relevanceReason": "The 3.x branch is shipping; the review of whether deletions were too aggressive is still open with no resolution comment."
    }
  },
  "analysis": {
    "summary": "This is a maintainer-filed tracking issue requesting a review of the SkiaSharp 3.x API-breaking changes, specifically whether PR #2539 (which removed all [Obsolete] APIs) was too aggressive. The 3.0.0 changelog documents ABI breaking changes (SKImageFilter.CropRect→SKRect, SKMatrix44 struct rewrite, SK3dView removal, SKRuntimeEffect rewrite) and lists removed Obsolete members. The current codebase still carries new [Obsolete] attributes in SKPaint.cs and other files intended for future removal cycles, suggesting the pattern continues. No explicit sign that the review concern from the issue was formally resolved.",
    "rationale": "The issue is filed by the maintainer as a self-assigned review task, not a user-reported bug. It requests evaluation of backward-compatibility impact of the 3.x cleanup in PR #2539. Classifying as type/enhancement (improving library compatibility story) rather than type/bug because no broken behavior is described. The tenet/compatibility label applies because this is explicitly about API stability and migration cost.",
    "codeInvestigation": [
      {
        "file": "changelogs/SkiaSharp/3.0.0/SkiaSharp.humanreadable.md",
        "finding": "The 3.0.0 changelog documents ABI breaking changes including: SKImageFilter.CropRect replaced by SKRect, SKMatrix44 rewritten as struct, SK3dView removed, SKRuntimeEffect rewritten. A separate section lists removed [Obsolete] types and members. This confirms the scope of deletions in PR #2539 was documented.",
        "relevance": "direct"
      },
      {
        "file": "binding/SkiaSharp/SKPaint.cs",
        "finding": "SKPaint.cs still carries multiple [Obsolete] attributes (e.g. FilterQuality→SKSamplingOptions, text-related APIs→SKFont). These are newly marked in 3.x, not yet removed — showing the library continues to use the phase-out pattern. Reviewing the 3.x deletions may inform which newly-obsoleted APIs need clearer migration paths.",
        "relevance": "related"
      }
    ],
    "keySignals": [
      {
        "text": "Even as part of #2539 there may have been a too-aggressive delete.",
        "source": "issue body",
        "interpretation": "Maintainer concern that some deleted APIs were still useful or lacked adequate replacements at the time of removal."
      }
    ],
    "nextQuestions": [
      "Which specific [Obsolete] members removed in PR #2539 lacked direct replacements?",
      "Has any consumer feedback or downstream issue reported pain from the 3.x removals?",
      "Does the v4 release plan (#3681, #3682) supersede this review concern?"
    ],
    "resolution": {
      "hypothesis": "Some APIs removed in PR #2539 may have had no clear migration path documented, making the 3.x upgrade harder. A focused diff of what was deleted vs what had documented replacements would close this issue.",
      "proposals": [
        {
          "title": "Audit PR #2539 deletions against the 3.0 changelog",
          "description": "Compare the APIs removed in PR #2539 against the 'Removed Obsolete' section of the 3.0 changelog to identify any deletions without documented replacement APIs. File follow-up issues for any gaps found.",
          "category": "investigation",
          "confidence": 0.8,
          "effort": "cost/s",
          "validated": "untested"
        },
        {
          "title": "Add migration notes to 3.0.0 changelog for under-documented removals",
          "description": "For any removed API that lacks a documented replacement, add a migration note to the 3.0.0 changelog and/or migration guide to reduce consumer friction.",
          "category": "fix",
          "confidence": 0.75,
          "effort": "cost/m",
          "validated": "untested"
        }
      ],
      "recommendedProposal": "Audit PR #2539 deletions against the 3.0 changelog",
      "recommendedReason": "Low-effort investigation to close the open question before any docs work begins."
    }
  },
  "output": {
    "actionability": {
      "suggestedAction": "keep-open",
      "confidence": 0.8,
      "reason": "Valid maintainer tracking issue about 3.x API compatibility. No evidence it was resolved; the 3.0 changelog exists but the specific review concern (was deletion too aggressive?) has no recorded outcome.",
      "suggestedReproPlatform": "linux"
    },
    "actions": [
      {
        "type": "update-labels",
        "description": "Apply enhancement, SkiaSharp area, and compatibility tenet labels",
        "risk": "low",
        "confidence": 0.9,
        "labels": [
          "type/enhancement",
          "area/SkiaSharp",
          "tenet/compatibility"
        ]
      },
      {
        "type": "add-comment",
        "description": "Post analysis noting the 3.0 changelog documents the removals and proposing an audit",
        "risk": "medium",
        "confidence": 0.8,
        "comment": "The 3.0.0 changelog documents the ABI breaking changes and removed [Obsolete] members from PR #2539. To close this review, it may be worth doing a quick audit of the deleted APIs against the changelog to identify any removals that lacked a documented migration path. If any gaps are found, adding migration notes to the changelog or migration guide would help consumers upgrading from 2.x."
      }
    ]
  }
}
```

</details>
