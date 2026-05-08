# Issue Triage Report — #1160

| Field | Value |
|-------|-------|
| Repository | mono/SkiaSharp |
| Analyzed | 2026-04-25T16:23:49Z |
| Type | type/enhancement (0.95 (95%)) |
| Area | area/SkiaSharp (0.97 (97%)) |
| Suggested action | keep-open (0.85 (85%)) |

**Issue Summary:** Reporter requests that SKRect.AspectFit and SKRect.AspectFill accept optional alignment arguments (0–1 range) so the resulting rectangle can be positioned at any point within the container, not just centered.

**Analysis:** The SKRect.AspectFit and SKRect.AspectFill methods unconditionally center the scaled rectangle inside the container by using MidX/MidY. The reporter proposes new overloads that accept xAlign and yAlign float parameters (0.0 = top/left, 0.5 = center, 1.0 = bottom/right) to control placement. The maintainer expressed interest and invited a PR, which was never submitted.

**Recommendations:** **keep-open** — Valid, well-specified enhancement with a clear and ABI-safe implementation path. Stale for 4+ years with a workaround available. Keep open for a future PR contribution.

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
| Current labels | type/enhancement |

## Evidence

### Reproduction

**Environment:** Filed 2020-03-02, no platform/version specified. Reporter uses their own C# math library as a workaround.

**Repository links:**
- https://github.com/mono/SkiaSharp/issues/176 — Prior issue #176 that added AspectFit/AspectFill convenience members to SKRect

### Version Analysis

| Field | Value |
|-------|-------|
| Mentioned versions | — |
| Worked in | — |
| Broke in | — |
| Current relevance | likely |
| Relevance reason | The AspectResize implementation in MathTypes.cs still hardcodes centering via MidX/MidY and has not changed to add alignment parameters. |

## Analysis

### Technical Summary

The SKRect.AspectFit and SKRect.AspectFill methods unconditionally center the scaled rectangle inside the container by using MidX/MidY. The reporter proposes new overloads that accept xAlign and yAlign float parameters (0.0 = top/left, 0.5 = center, 1.0 = bottom/right) to control placement. The maintainer expressed interest and invited a PR, which was never submitted.

### Rationale

This is clearly an enhancement: the existing methods work correctly but have a fixed alignment policy that is not always desired. The change would be ABI-safe because new overloads would be added rather than modifying existing signatures. The issue has been stale for 4+ years with a simple workaround available, making keep-open the appropriate action.

### Key Signals

- "AspectFill and AspectFit methods always center the resulting rectangle" — **issue body** (Reporter correctly identifies the hardcoded centering behavior as a limitation.)
- "add two extra arguments with default values 0.5f" — **issue body** (Proposed API design is concrete and uses default-parameter-like semantics; however, ABI rules require these be new overloads.)
- "I could make a PR if you think this is useful?" — **comment by ziriax** (Reporter offered to contribute; maintainer said yes but PR never came.)
- "Add any members that you think will be useful, and then we can talk about each one." — **comment by mattleibow** (Maintainer was receptive and open to design discussion.)

### Code Investigation

| File | Lines | Relevance | Finding |
|------|-------|-----------|---------|
| `binding/SkiaSharp/MathTypes.cs` | 404-426 | direct | SKRect.AspectResize computes the scaled width/height, then positions the result using `MidX - (aspectWidth / 2f)` and `MidY - (aspectHeight / 2f)`, which always produces a centered rectangle. Adding xAlign/yAlign overloads would replace these with `Left + (Width - aspectWidth) * xAlign` and `Top + (Height - aspectHeight) * yAlign`. |
| `binding/SkiaSharp/MathTypes.cs` | 569-573 | related | SKRectI.AspectFit and SKRectI.AspectFill delegate to the SKRect versions via a cast, so adding new overloads to SKRect.AspectResize would naturally extend to SKRectI as well. |

### Workarounds

- Use your own math library to compute the aligned rectangle: after computing aspectWidth/aspectHeight from the AspectResize formula, position with `Left + (Width - aspectWidth) * xAlign` and `Top + (Height - aspectHeight) * yAlign`.
- Use the existing SKRect.AspectFit/AspectFill (which centers), then use SKRect.Offset to shift the result.

### Next Questions

- Should the new overloads also be added to SKRectI?
- Should there be an SKContentScale or SKAlignment enum type rather than raw floats?
- Was any draft PR ever opened but abandoned or closed?

### Resolution Proposals

**Hypothesis:** Add new overloads SKRect.AspectFit(SKSize, float xAlign, float yAlign) and SKRect.AspectFill(SKSize, float xAlign, float yAlign) that pass alignment through to a refactored AspectResize helper. This is ABI-safe since it adds new overloads rather than modifying existing ones.

1. **Implement custom alignment as a local workaround** — workaround, confidence 0.95 (95%), cost/xs, validated=untested
   - The reporter can replicate the AspectResize logic and substitute the centering calculation with alignment-controlled positioning using `Left + (Width - aspectWidth) * xAlign`.
2. **Add new AspectFit/AspectFill overloads with alignment parameters** — fix, confidence 0.88 (88%), cost/s, validated=untested
   - Add overloads `AspectFit(SKSize size, float xAlign, float yAlign)` and `AspectFill(SKSize size, float xAlign, float yAlign)` to SKRect and SKRectI. Refactor AspectResize to accept the alignment parameters. Existing zero-argument overloads remain unchanged (call the new overloads with 0.5f, 0.5f).
3. **Add a separate SKRect.Align helper method** — alternative, confidence 0.75 (75%), cost/s, validated=untested
   - As the reporter also suggests, add a standalone `SKRect.Align(SKRect container, float xAlign, float yAlign)` method that positions `this` rectangle inside `container` at the given alignment. This would be orthogonal to AspectFit/AspectFill.

**Recommended proposal:** Add new AspectFit/AspectFill overloads with alignment parameters

**Why:** Direct enhancement to the requested API surface. ABI-safe as new overloads. Minimal implementation scope. The reporter offered to contribute a PR.

## Recommendations

### Actionability

| Field | Value |
|-------|-------|
| Suggested action | keep-open |
| Confidence | 0.85 (85%) |
| Reason | Valid, well-specified enhancement with a clear and ABI-safe implementation path. Stale for 4+ years with a workaround available. Keep open for a future PR contribution. |
| Suggested repro platform | linux |

### Automatable Actions

| Type | Risk | Confidence | Description | Details |
|------|------|------------|-------------|---------|
| update-labels | low | 0.95 (95%) | Issue already has type/enhancement; add area/SkiaSharp and tenet/compatibility | labels=type/enhancement, area/SkiaSharp, tenet/compatibility |
| add-comment | medium | 0.85 (85%) | Acknowledge the request, confirm the current centering behavior, provide a workaround, and invite a PR | — |

**Comment draft for `add-comment`:**

```markdown
Thanks for the feature request! You're right that `SKRect.AspectFit` and `SKRect.AspectFill` always produce a centered result — the implementation uses `MidX`/`MidY` for positioning.

As a workaround until alignment overloads are added, you can replicate the logic and substitute custom positioning:

```csharp
// Fit with custom alignment (xAlign/yAlign: 0=left/top, 0.5=center, 1=right/bottom)
static SKRect AspectFitAligned(SKRect container, SKSize size, float xAlign = 0.5f, float yAlign = 0.5f)
{
    if (size.Width == 0 || size.Height == 0 || container.Width == 0 || container.Height == 0)
        return SKRect.Create(container.MidX, container.MidY, 0, 0);
    var imgAspect = size.Width / size.Height;
    var containerAspect = container.Width / container.Height;
    float w, h;
    if (containerAspect > imgAspect) { h = container.Height; w = h * imgAspect; }
    else { w = container.Width; h = w / imgAspect; }
    return SKRect.Create(
        container.Left + (container.Width - w) * xAlign,
        container.Top + (container.Height - h) * yAlign,
        w, h);
}
```

We're open to adding alignment overloads to `SKRect.AspectFit` and `SKRect.AspectFill`. If you'd like to submit a PR with the new overloads (and matching `SKRectI` equivalents), we'd be happy to review it!
```

<details>
<summary>Raw JSON</summary>

```json
{
  "meta": {
    "schemaVersion": "1.0",
    "number": 1160,
    "repo": "mono/SkiaSharp",
    "analyzedAt": "2026-04-25T16:23:49Z",
    "currentLabels": [
      "type/enhancement"
    ]
  },
  "summary": "Reporter requests that SKRect.AspectFit and SKRect.AspectFill accept optional alignment arguments (0–1 range) so the resulting rectangle can be positioned at any point within the container, not just centered.",
  "classification": {
    "type": {
      "value": "type/enhancement",
      "confidence": 0.95
    },
    "area": {
      "value": "area/SkiaSharp",
      "confidence": 0.97
    },
    "tenets": [
      "tenet/compatibility"
    ]
  },
  "evidence": {
    "reproEvidence": {
      "environmentDetails": "Filed 2020-03-02, no platform/version specified. Reporter uses their own C# math library as a workaround.",
      "repoLinks": [
        {
          "url": "https://github.com/mono/SkiaSharp/issues/176",
          "description": "Prior issue #176 that added AspectFit/AspectFill convenience members to SKRect"
        }
      ]
    },
    "versionAnalysis": {
      "mentionedVersions": [],
      "currentRelevance": "likely",
      "relevanceReason": "The AspectResize implementation in MathTypes.cs still hardcodes centering via MidX/MidY and has not changed to add alignment parameters."
    }
  },
  "analysis": {
    "summary": "The SKRect.AspectFit and SKRect.AspectFill methods unconditionally center the scaled rectangle inside the container by using MidX/MidY. The reporter proposes new overloads that accept xAlign and yAlign float parameters (0.0 = top/left, 0.5 = center, 1.0 = bottom/right) to control placement. The maintainer expressed interest and invited a PR, which was never submitted.",
    "rationale": "This is clearly an enhancement: the existing methods work correctly but have a fixed alignment policy that is not always desired. The change would be ABI-safe because new overloads would be added rather than modifying existing signatures. The issue has been stale for 4+ years with a simple workaround available, making keep-open the appropriate action.",
    "keySignals": [
      {
        "text": "AspectFill and AspectFit methods always center the resulting rectangle",
        "source": "issue body",
        "interpretation": "Reporter correctly identifies the hardcoded centering behavior as a limitation."
      },
      {
        "text": "add two extra arguments with default values 0.5f",
        "source": "issue body",
        "interpretation": "Proposed API design is concrete and uses default-parameter-like semantics; however, ABI rules require these be new overloads."
      },
      {
        "text": "I could make a PR if you think this is useful?",
        "source": "comment by ziriax",
        "interpretation": "Reporter offered to contribute; maintainer said yes but PR never came."
      },
      {
        "text": "Add any members that you think will be useful, and then we can talk about each one.",
        "source": "comment by mattleibow",
        "interpretation": "Maintainer was receptive and open to design discussion."
      }
    ],
    "codeInvestigation": [
      {
        "file": "binding/SkiaSharp/MathTypes.cs",
        "lines": "404-426",
        "finding": "SKRect.AspectResize computes the scaled width/height, then positions the result using `MidX - (aspectWidth / 2f)` and `MidY - (aspectHeight / 2f)`, which always produces a centered rectangle. Adding xAlign/yAlign overloads would replace these with `Left + (Width - aspectWidth) * xAlign` and `Top + (Height - aspectHeight) * yAlign`.",
        "relevance": "direct"
      },
      {
        "file": "binding/SkiaSharp/MathTypes.cs",
        "lines": "569-573",
        "finding": "SKRectI.AspectFit and SKRectI.AspectFill delegate to the SKRect versions via a cast, so adding new overloads to SKRect.AspectResize would naturally extend to SKRectI as well.",
        "relevance": "related"
      }
    ],
    "workarounds": [
      "Use your own math library to compute the aligned rectangle: after computing aspectWidth/aspectHeight from the AspectResize formula, position with `Left + (Width - aspectWidth) * xAlign` and `Top + (Height - aspectHeight) * yAlign`.",
      "Use the existing SKRect.AspectFit/AspectFill (which centers), then use SKRect.Offset to shift the result."
    ],
    "nextQuestions": [
      "Should the new overloads also be added to SKRectI?",
      "Should there be an SKContentScale or SKAlignment enum type rather than raw floats?",
      "Was any draft PR ever opened but abandoned or closed?"
    ],
    "resolution": {
      "hypothesis": "Add new overloads SKRect.AspectFit(SKSize, float xAlign, float yAlign) and SKRect.AspectFill(SKSize, float xAlign, float yAlign) that pass alignment through to a refactored AspectResize helper. This is ABI-safe since it adds new overloads rather than modifying existing ones.",
      "proposals": [
        {
          "title": "Implement custom alignment as a local workaround",
          "description": "The reporter can replicate the AspectResize logic and substitute the centering calculation with alignment-controlled positioning using `Left + (Width - aspectWidth) * xAlign`.",
          "category": "workaround",
          "confidence": 0.95,
          "effort": "cost/xs",
          "validated": "untested"
        },
        {
          "title": "Add new AspectFit/AspectFill overloads with alignment parameters",
          "description": "Add overloads `AspectFit(SKSize size, float xAlign, float yAlign)` and `AspectFill(SKSize size, float xAlign, float yAlign)` to SKRect and SKRectI. Refactor AspectResize to accept the alignment parameters. Existing zero-argument overloads remain unchanged (call the new overloads with 0.5f, 0.5f).",
          "category": "fix",
          "confidence": 0.88,
          "effort": "cost/s",
          "validated": "untested"
        },
        {
          "title": "Add a separate SKRect.Align helper method",
          "description": "As the reporter also suggests, add a standalone `SKRect.Align(SKRect container, float xAlign, float yAlign)` method that positions `this` rectangle inside `container` at the given alignment. This would be orthogonal to AspectFit/AspectFill.",
          "category": "alternative",
          "confidence": 0.75,
          "effort": "cost/s",
          "validated": "untested"
        }
      ],
      "recommendedProposal": "Add new AspectFit/AspectFill overloads with alignment parameters",
      "recommendedReason": "Direct enhancement to the requested API surface. ABI-safe as new overloads. Minimal implementation scope. The reporter offered to contribute a PR."
    }
  },
  "output": {
    "actionability": {
      "suggestedAction": "keep-open",
      "confidence": 0.85,
      "reason": "Valid, well-specified enhancement with a clear and ABI-safe implementation path. Stale for 4+ years with a workaround available. Keep open for a future PR contribution.",
      "suggestedReproPlatform": "linux"
    },
    "actions": [
      {
        "type": "update-labels",
        "description": "Issue already has type/enhancement; add area/SkiaSharp and tenet/compatibility",
        "risk": "low",
        "confidence": 0.95,
        "labels": [
          "type/enhancement",
          "area/SkiaSharp",
          "tenet/compatibility"
        ]
      },
      {
        "type": "add-comment",
        "description": "Acknowledge the request, confirm the current centering behavior, provide a workaround, and invite a PR",
        "risk": "medium",
        "confidence": 0.85,
        "comment": "Thanks for the feature request! You're right that `SKRect.AspectFit` and `SKRect.AspectFill` always produce a centered result — the implementation uses `MidX`/`MidY` for positioning.\n\nAs a workaround until alignment overloads are added, you can replicate the logic and substitute custom positioning:\n\n```csharp\n// Fit with custom alignment (xAlign/yAlign: 0=left/top, 0.5=center, 1=right/bottom)\nstatic SKRect AspectFitAligned(SKRect container, SKSize size, float xAlign = 0.5f, float yAlign = 0.5f)\n{\n    if (size.Width == 0 || size.Height == 0 || container.Width == 0 || container.Height == 0)\n        return SKRect.Create(container.MidX, container.MidY, 0, 0);\n    var imgAspect = size.Width / size.Height;\n    var containerAspect = container.Width / container.Height;\n    float w, h;\n    if (containerAspect > imgAspect) { h = container.Height; w = h * imgAspect; }\n    else { w = container.Width; h = w / imgAspect; }\n    return SKRect.Create(\n        container.Left + (container.Width - w) * xAlign,\n        container.Top + (container.Height - h) * yAlign,\n        w, h);\n}\n```\n\nWe're open to adding alignment overloads to `SKRect.AspectFit` and `SKRect.AspectFill`. If you'd like to submit a PR with the new overloads (and matching `SKRectI` equivalents), we'd be happy to review it!"
      }
    ]
  }
}
```

</details>
