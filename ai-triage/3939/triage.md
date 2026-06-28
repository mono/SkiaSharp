# Issue Triage Report — #3939

| Field | Value |
|-------|-------|
| Repository | mono/SkiaSharp |
| Analyzed | 2026-06-28T05:45:00Z |
| Type | type/feature-request (0.98 (98%)) |
| Area | area/SkiaSharp (0.95 (95%)) |
| Suggested action | needs-investigation (0.92 (92%)) |

**Issue Summary:** Feature request to expose SkCodec::isAnimated() (added in Skia m135) as SKCodec.IsAnimated property, resolving the ambiguity where RepetitionCount == 0 could mean 'not animated' or 'unknown' for partial/streaming GIF data.

**Analysis:** SKCodec.IsAnimated is not yet exposed. SkCodec::isAnimated() has been available in upstream Skia since m135, returning a tri-state (Yes/No/Unknown) that disambiguates the ambiguous RepetitionCount == 0 case. The entire implementation (C API shim + binding regen + C# wrapper + tests) is estimated at ~45 lines / XS effort and is tracked under the v4 API additions milestone.

**Recommendations:** **needs-investigation** — Well-scoped feature request from the maintainer with a complete implementation plan. Tracked under the v4 API additions milestone. Proceed with api-add-review workflow.

---

## Classification

| Field | Value |
|-------|-------|
| Type | type/feature-request |
| Area | area/SkiaSharp |
| Platforms | — |
| Backends | — |
| Tenets | — |
| Partner | — |
| Current labels | type/feature-request, upgrading/4.x, cost/xs, priority/2 |

## Evidence

### Reproduction

**Environment:** SkiaSharp v4, milestone 4.150.0-preview.3, Skia m135+

**Related issues:** #3680, #3807

### Version Analysis

| Field | Value |
|-------|-------|
| Mentioned versions | 4.150.0-preview.3 |
| Worked in | — |
| Broke in | — |
| Current relevance | likely |
| Relevance reason | SkCodec::isAnimated() was added in Skia m135. Confirmed absent from SkiaSharp: no sk_codec_is_animated in SkiaApi.generated.cs, no SKCodecAnimationStatus enum, no SKCodec.IsAnimated property in SKCodec.cs. |

## Analysis

### Technical Summary

SKCodec.IsAnimated is not yet exposed. SkCodec::isAnimated() has been available in upstream Skia since m135, returning a tri-state (Yes/No/Unknown) that disambiguates the ambiguous RepetitionCount == 0 case. The entire implementation (C API shim + binding regen + C# wrapper + tests) is estimated at ~45 lines / XS effort and is tracked under the v4 API additions milestone.

### Rationale

Classified as type/feature-request because SKCodec.IsAnimated does not yet exist in SkiaSharp — confirmed by source inspection of SKCodec.cs and SkiaApi.generated.cs. Area is area/SkiaSharp (core codec bindings). No platform-specific concerns; FrameCount/codec APIs work cross-platform. Issue is from the project maintainer with a complete implementation plan. The Skia Analyst report (#3807) independently confirmed this API gap in m135.

### Key Signals

- "RepetitionCount == 0 is ambiguous — it could mean 'not animated' or 'unknown' (e.g. partial GIF data)" — **issue body** (Real usability problem. Partial GIF streams return 0 for RepetitionCount before all frames are known.)
- "~2 hours, ~45 lines total" — **issue body** (Very small, well-scoped implementation, consistent with cost/xs label.)
- "Part of #3680" — **issue body** (Sub-issue of the v4 API evaluation tracker, already in milestone 4.150.0-preview.3.)

### Code Investigation

| File | Lines | Relevance | Finding |
|------|-------|-----------|---------|
| `binding/SkiaSharp/SKCodec.cs` | 82-96 | direct | RepetitionCount and FrameCount properties exist, but no IsAnimated property or SKCodecAnimationStatus enum is present. The feature is confirmed absent. |
| `binding/SkiaSharp/SkiaApi.generated.cs` | — | direct | sk_codec_get_repetition_count is present but sk_codec_is_animated is absent, confirming the C API binding does not yet exist. |
| `samples/Gallery/Shared/Samples/AnimatedWebpEncoderSample.cs` | 54 | related | Existing gallery code uses `_codec.FrameCount > 1` as an IsAnimated proxy — confirms the workaround pattern is already established in the codebase. |

### Workarounds

- Use `codec.FrameCount > 1` as an approximate proxy for fully-loaded images. Caveat: fails for single-frame animated GIFs and partial/streaming data. Pattern is already used in samples/Gallery/Shared/Samples/AnimatedWebpEncoderSample.cs:54.

### Resolution Proposals

**Hypothesis:** Add SKCodecAnimationStatus enum (Yes, No, Unknown) and SKCodec.IsAnimated property wrapping SkCodec::isAnimated() via a new sk_codec_is_animated C API function.

1. **Workaround: use FrameCount as animation proxy** — workaround, confidence 0.65 (65%), cost/xs, validated=yes
   - Use `codec.FrameCount > 1` as a best-effort animation check. Confirmed pattern in AnimatedWebpEncoderSample.cs:54. Caveats validated by 3 agents: (1) single-frame animated GIFs classified as non-animated; (2) partial/streaming GIF data may still give false negative (same ambiguity as RepetitionCount).

```csharp
// Approximate workaround — not equivalent to IsAnimated
// Fails for single-frame animated GIFs and partial/streaming data
bool isAnimated = codec.FrameCount > 1;
```
2. **Implement SKCodec.IsAnimated via C API shim** — fix, confidence 0.95 (95%), cost/xs, validated=untested
   - Add sk_codec_is_animated to the C API shim (externals/skia/src/c/), regenerate P/Invoke bindings, add SKCodecAnimationStatus enum and SKCodec.IsAnimated property (~5 lines C#). Requires api-add-review workflow. Estimated ~45 lines total.

**Recommended proposal:** Implement SKCodec.IsAnimated via C API shim

**Why:** The fix is well-specified (~45 lines), cost/xs, already in milestone 4.150.0-preview.3, and resolves all edge cases (including partial streaming data) that the FrameCount workaround cannot handle.

## Recommendations

### Actionability

| Field | Value |
|-------|-------|
| Suggested action | needs-investigation |
| Confidence | 0.92 (92%) |
| Reason | Well-scoped feature request from the maintainer with a complete implementation plan. Tracked under the v4 API additions milestone. Proceed with api-add-review workflow. |
| Suggested repro platform | linux |

### Automatable Actions

| Type | Risk | Confidence | Description | Details |
|------|------|------------|-------------|---------|
| update-labels | low | 0.98 (98%) | Apply area/SkiaSharp label (missing from current labels which only have type/feature-request, upgrading/4.x, cost/xs, priority/2) | labels=type/feature-request, area/SkiaSharp |
| add-comment | medium | 0.92 (92%) | Post triage acknowledgment confirming the API gap and providing the FrameCount workaround | — |

**Comment draft for `add-comment`:**

```markdown
Confirmed: `sk_codec_is_animated` is absent from the C API shim and P/Invoke bindings. The implementation plan in the issue description is accurate.

In the meantime, `codec.FrameCount > 1` is an approximate workaround already used in the codebase (AnimatedWebpEncoderSample.cs):

```csharp
// Approximate workaround — not equivalent to IsAnimated
// Fails for single-frame animated GIFs and partial/streaming data
bool isAnimated = codec.FrameCount > 1;
```

Caveats: single-frame animated GIFs will be misclassified as non-animated, and partial GIF streams still have the same ambiguity as `RepetitionCount == 0`. The proposed `SKCodecAnimationStatus` enum + `SKCodec.IsAnimated` property will fully resolve both cases.
```

<details>
<summary>Raw JSON</summary>

```json
{
  "meta": {
    "schemaVersion": "1.0",
    "number": 3939,
    "repo": "mono/SkiaSharp",
    "analyzedAt": "2026-06-28T05:45:00Z",
    "currentLabels": [
      "type/feature-request",
      "upgrading/4.x",
      "cost/xs",
      "priority/2"
    ]
  },
  "summary": "Feature request to expose SkCodec::isAnimated() (added in Skia m135) as SKCodec.IsAnimated property, resolving the ambiguity where RepetitionCount == 0 could mean 'not animated' or 'unknown' for partial/streaming GIF data.",
  "classification": {
    "type": {
      "value": "type/feature-request",
      "confidence": 0.98
    },
    "area": {
      "value": "area/SkiaSharp",
      "confidence": 0.95
    }
  },
  "evidence": {
    "reproEvidence": {
      "environmentDetails": "SkiaSharp v4, milestone 4.150.0-preview.3, Skia m135+",
      "relatedIssues": [
        3680,
        3807
      ]
    },
    "versionAnalysis": {
      "mentionedVersions": [
        "4.150.0-preview.3"
      ],
      "currentRelevance": "likely",
      "relevanceReason": "SkCodec::isAnimated() was added in Skia m135. Confirmed absent from SkiaSharp: no sk_codec_is_animated in SkiaApi.generated.cs, no SKCodecAnimationStatus enum, no SKCodec.IsAnimated property in SKCodec.cs."
    }
  },
  "analysis": {
    "summary": "SKCodec.IsAnimated is not yet exposed. SkCodec::isAnimated() has been available in upstream Skia since m135, returning a tri-state (Yes/No/Unknown) that disambiguates the ambiguous RepetitionCount == 0 case. The entire implementation (C API shim + binding regen + C# wrapper + tests) is estimated at ~45 lines / XS effort and is tracked under the v4 API additions milestone.",
    "codeInvestigation": [
      {
        "file": "binding/SkiaSharp/SKCodec.cs",
        "lines": "82-96",
        "finding": "RepetitionCount and FrameCount properties exist, but no IsAnimated property or SKCodecAnimationStatus enum is present. The feature is confirmed absent.",
        "relevance": "direct"
      },
      {
        "file": "binding/SkiaSharp/SkiaApi.generated.cs",
        "finding": "sk_codec_get_repetition_count is present but sk_codec_is_animated is absent, confirming the C API binding does not yet exist.",
        "relevance": "direct"
      },
      {
        "file": "samples/Gallery/Shared/Samples/AnimatedWebpEncoderSample.cs",
        "lines": "54",
        "finding": "Existing gallery code uses `_codec.FrameCount > 1` as an IsAnimated proxy — confirms the workaround pattern is already established in the codebase.",
        "relevance": "related"
      }
    ],
    "keySignals": [
      {
        "text": "RepetitionCount == 0 is ambiguous — it could mean 'not animated' or 'unknown' (e.g. partial GIF data)",
        "source": "issue body",
        "interpretation": "Real usability problem. Partial GIF streams return 0 for RepetitionCount before all frames are known."
      },
      {
        "text": "~2 hours, ~45 lines total",
        "source": "issue body",
        "interpretation": "Very small, well-scoped implementation, consistent with cost/xs label."
      },
      {
        "text": "Part of #3680",
        "source": "issue body",
        "interpretation": "Sub-issue of the v4 API evaluation tracker, already in milestone 4.150.0-preview.3."
      }
    ],
    "rationale": "Classified as type/feature-request because SKCodec.IsAnimated does not yet exist in SkiaSharp — confirmed by source inspection of SKCodec.cs and SkiaApi.generated.cs. Area is area/SkiaSharp (core codec bindings). No platform-specific concerns; FrameCount/codec APIs work cross-platform. Issue is from the project maintainer with a complete implementation plan. The Skia Analyst report (#3807) independently confirmed this API gap in m135.",
    "workarounds": [
      "Use `codec.FrameCount > 1` as an approximate proxy for fully-loaded images. Caveat: fails for single-frame animated GIFs and partial/streaming data. Pattern is already used in samples/Gallery/Shared/Samples/AnimatedWebpEncoderSample.cs:54."
    ],
    "resolution": {
      "hypothesis": "Add SKCodecAnimationStatus enum (Yes, No, Unknown) and SKCodec.IsAnimated property wrapping SkCodec::isAnimated() via a new sk_codec_is_animated C API function.",
      "proposals": [
        {
          "title": "Workaround: use FrameCount as animation proxy",
          "description": "Use `codec.FrameCount > 1` as a best-effort animation check. Confirmed pattern in AnimatedWebpEncoderSample.cs:54. Caveats validated by 3 agents: (1) single-frame animated GIFs classified as non-animated; (2) partial/streaming GIF data may still give false negative (same ambiguity as RepetitionCount).",
          "category": "workaround",
          "codeSnippet": "// Approximate workaround — not equivalent to IsAnimated\n// Fails for single-frame animated GIFs and partial/streaming data\nbool isAnimated = codec.FrameCount > 1;",
          "confidence": 0.65,
          "effort": "cost/xs",
          "validated": "yes"
        },
        {
          "title": "Implement SKCodec.IsAnimated via C API shim",
          "description": "Add sk_codec_is_animated to the C API shim (externals/skia/src/c/), regenerate P/Invoke bindings, add SKCodecAnimationStatus enum and SKCodec.IsAnimated property (~5 lines C#). Requires api-add-review workflow. Estimated ~45 lines total.",
          "category": "fix",
          "confidence": 0.95,
          "effort": "cost/xs",
          "validated": "untested"
        }
      ],
      "recommendedProposal": "Implement SKCodec.IsAnimated via C API shim",
      "recommendedReason": "The fix is well-specified (~45 lines), cost/xs, already in milestone 4.150.0-preview.3, and resolves all edge cases (including partial streaming data) that the FrameCount workaround cannot handle."
    }
  },
  "output": {
    "actionability": {
      "suggestedAction": "needs-investigation",
      "confidence": 0.92,
      "reason": "Well-scoped feature request from the maintainer with a complete implementation plan. Tracked under the v4 API additions milestone. Proceed with api-add-review workflow.",
      "suggestedReproPlatform": "linux"
    },
    "actions": [
      {
        "type": "update-labels",
        "description": "Apply area/SkiaSharp label (missing from current labels which only have type/feature-request, upgrading/4.x, cost/xs, priority/2)",
        "risk": "low",
        "confidence": 0.98,
        "labels": [
          "type/feature-request",
          "area/SkiaSharp"
        ]
      },
      {
        "type": "add-comment",
        "description": "Post triage acknowledgment confirming the API gap and providing the FrameCount workaround",
        "risk": "medium",
        "confidence": 0.92,
        "comment": "Confirmed: `sk_codec_is_animated` is absent from the C API shim and P/Invoke bindings. The implementation plan in the issue description is accurate.\n\nIn the meantime, `codec.FrameCount > 1` is an approximate workaround already used in the codebase (AnimatedWebpEncoderSample.cs):\n\n```csharp\n// Approximate workaround — not equivalent to IsAnimated\n// Fails for single-frame animated GIFs and partial/streaming data\nbool isAnimated = codec.FrameCount > 1;\n```\n\nCaveats: single-frame animated GIFs will be misclassified as non-animated, and partial GIF streams still have the same ambiguity as `RepetitionCount == 0`. The proposed `SKCodecAnimationStatus` enum + `SKCodec.IsAnimated` property will fully resolve both cases."
      }
    ]
  }
}
```

</details>
