# Issue Triage Report — #1003

| Field | Value |
|-------|-------|
| Repository | mono/SkiaSharp |
| Analyzed | 2026-04-25T08:36:37Z |
| Type | type/feature-request (0.88 (88%)) |
| Area | area/Build (0.85 (85%)) |
| Suggested action | keep-open (0.85 (85%)) |

**Issue Summary:** Feature request proposing to auto-generate the C API shim layer from Skia C++ headers using CppAst, eliminating the need to hand-write C wrapper functions.

**Analysis:** The reporter asks whether CppAst can be used to auto-generate the C API shim layer, removing the need to hand-maintain C wrapper functions in the mono/skia fork. The C# P/Invoke generation is already automated via SkiaSharpGenerator which uses CppAst to parse C headers. The remaining gap is the C layer itself (the C shim wrapping Skia's C++ API). The maintainer acknowledged this is feasible but noted complexity: carefully designed struct types (SKSize, SKPoint) need intentional ABI decisions, and dropping the native fork is not planned.

**Recommendations:** **keep-open** — Valid long-term feature request acknowledged by maintainer. Already marked up-for-grabs. No new information to act on; the issue should remain open as a tracked enhancement.

---

## Classification

| Field | Value |
|-------|-------|
| Type | type/feature-request |
| Area | area/Build |
| Platforms | — |
| Backends | — |
| Tenets | tenet/compatibility |
| Partner | — |
| Current labels | status/help-wanted, status/long-term, status/low-priority, status/up-for-grabs |

## Evidence

### Reproduction

**Repository links:**
- https://github.com/xoofx/CppAst — CppAst library suggested by reporter for C++ header parsing
- https://github.com/SharpGenTools/SharpGenTools — Alternative code-gen tool referenced in comments (contributor suggestion)
- https://github.com/mono/SkiaSharp/issues/1002 — Related issue about auto-generated C# P/Invoke bindings (the C# side is already generated)

### Version Analysis

| Field | Value |
|-------|-------|
| Mentioned versions | — |
| Worked in | — |
| Broke in | — |
| Current relevance | likely |
| Relevance reason | The SkiaSharpGenerator already uses CppAst to generate C# P/Invoke bindings from C headers. The request to extend this to also generate the C shim layer remains open and unimplemented. |

## Analysis

### Technical Summary

The reporter asks whether CppAst can be used to auto-generate the C API shim layer, removing the need to hand-maintain C wrapper functions in the mono/skia fork. The C# P/Invoke generation is already automated via SkiaSharpGenerator which uses CppAst to parse C headers. The remaining gap is the C layer itself (the C shim wrapping Skia's C++ API). The maintainer acknowledged this is feasible but noted complexity: carefully designed struct types (SKSize, SKPoint) need intentional ABI decisions, and dropping the native fork is not planned.

### Rationale

The title says [QUESTION] but the body and discussion reveal a feature request to auto-generate the C shim layer. CppAst is already in use for C# generation, so the infrastructure foundation is present. The C shim generation is a separate, more complex task involving ABI decisions. The maintainer has acknowledged feasibility and marked the issue as up-for-grabs with long-term/low-priority status. This warrants keep-open rather than close, as it is a valid long-term enhancement with maintainer endorsement.

### Key Signals

- "https://github.com/xoofx/CppAst can parse C++ headers, so can we generate C API automatically too?" — **issue body** (Reporter is proposing extending the existing C# generation toolchain to also generate the C wrapper layer from Skia C++ headers.)
- "Been a long time since you asked, but this is something I am wanting to do." — **maintainer comment (mattleibow)** (Maintainer acknowledges the feature is desirable but not yet prioritized.)
- "the actual C++ API is a bit varied and will make tools a bit harder to write … I actually want the small, frequently used objects to be structs - SKSize and SKPoint." — **maintainer comment (mattleibow)** (Key design constraint: not all C++ types should map mechanically — some structs must be intentionally hand-designed for performance.)
- "On the point of the don't fork the native repo, I don't think this will be possible in the near future" — **maintainer comment (mattleibow)** (The secondary benefit (drop the fork) is out of scope for now; the primary feature (C API generation) could still be pursued independently.)

### Code Investigation

| File | Lines | Relevance | Finding |
|------|-------|-----------|---------|
| `utils/SkiaSharpGenerator/Generate/Generator.cs` | 1-8 | direct | The SkiaSharpGenerator already imports and uses CppAst to parse C headers and generate C# P/Invoke bindings. The CppAst dependency requested by the reporter is already in use for the C# side. |
| `utils/SkiaSharpGenerator/Program.cs` | 1-30 | direct | The tool exposes GenerateCommand (C# generation), CookieCommand, and VerifyCommand. No command exists for generating the C shim layer — confirming the feature is not yet implemented. |

### Workarounds

- Continue hand-writing C API functions in the mono/skia fork as currently done — the maintainer notes this is not particularly time-consuming
- Use SharpGenTools (https://github.com/SharpGenTools/SharpGenTools) as an alternative approach for generating C/C++ interop code

### Next Questions

- Would a partial generator (handling common type patterns automatically, with escape hatches for struct-typed objects) satisfy the use case?
- Has any community PR been attempted since the maintainer indicated PRs are welcome?

### Resolution Proposals

**Hypothesis:** Extend SkiaSharpGenerator with a new command that reads Skia C++ headers via CppAst and emits C shim function stubs, with a config-driven override system for types that need custom mapping (e.g., struct-valued types like SKSize/SKPoint).

1. **Add GenerateCApi command to SkiaSharpGenerator** — fix, confidence 0.70 (70%), cost/xl, validated=untested
   - Extend utils/SkiaSharpGenerator with a new command that uses CppAst to parse Skia C++ headers and generate C shim (.h/.cpp) stubs into the externals/skia fork. A JSON config (similar to the existing C# generation config) would control type overrides and exclusions.
2. **Use SharpGenTools as an alternative** — alternative, confidence 0.55 (55%), cost/l, validated=untested
   - Evaluate SharpGenTools (already suggested in comments) as a ready-made solution for generating C interop from C++ headers, potentially reducing implementation effort.

**Recommended proposal:** Add GenerateCApi command to SkiaSharpGenerator

**Why:** Builds on the existing CppAst investment already in the toolchain and matches the current architecture pattern. Config-driven overrides address the maintainer's concern about struct-typed objects.

## Recommendations

### Actionability

| Field | Value |
|-------|-------|
| Suggested action | keep-open |
| Confidence | 0.85 (85%) |
| Reason | Valid long-term feature request acknowledged by maintainer. Already marked up-for-grabs. No new information to act on; the issue should remain open as a tracked enhancement. |
| Suggested repro platform | linux |

### Automatable Actions

| Type | Risk | Confidence | Description | Details |
|------|------|------------|-------------|---------|
| update-labels | low | 0.88 (88%) | Apply type/feature-request and area/Build labels | labels=type/feature-request, area/Build |
| add-comment | medium | 0.80 (80%) | Provide status update noting CppAst is already in use for C# generation and outline the remaining gap | — |

**Comment draft for `add-comment`:**

```markdown
Thanks for the suggestion! As a status update: CppAst is already in use in `utils/SkiaSharpGenerator` to generate the C# P/Invoke bindings from C headers. The remaining gap — auto-generating the C shim layer itself — is the harder part, since some types (e.g., `SKSize`, `SKPoint`) need intentional ABI decisions rather than mechanical mapping.

This is still on the long-term roadmap and marked as **up-for-grabs**. If anyone wants to take a shot at extending `SkiaSharpGenerator` with a C-shim generation command (with a config-driven override system for custom type mappings), that would be very welcome!
```

<details>
<summary>Raw JSON</summary>

```json
{
  "meta": {
    "schemaVersion": "1.0",
    "number": 1003,
    "repo": "mono/SkiaSharp",
    "analyzedAt": "2026-04-25T08:36:37Z",
    "currentLabels": [
      "status/help-wanted",
      "status/long-term",
      "status/low-priority",
      "status/up-for-grabs"
    ]
  },
  "summary": "Feature request proposing to auto-generate the C API shim layer from Skia C++ headers using CppAst, eliminating the need to hand-write C wrapper functions.",
  "classification": {
    "type": {
      "value": "type/feature-request",
      "confidence": 0.88
    },
    "area": {
      "value": "area/Build",
      "confidence": 0.85
    },
    "tenets": [
      "tenet/compatibility"
    ]
  },
  "evidence": {
    "reproEvidence": {
      "repoLinks": [
        {
          "url": "https://github.com/xoofx/CppAst",
          "description": "CppAst library suggested by reporter for C++ header parsing"
        },
        {
          "url": "https://github.com/SharpGenTools/SharpGenTools",
          "description": "Alternative code-gen tool referenced in comments (contributor suggestion)"
        },
        {
          "url": "https://github.com/mono/SkiaSharp/issues/1002",
          "description": "Related issue about auto-generated C# P/Invoke bindings (the C# side is already generated)"
        }
      ]
    },
    "versionAnalysis": {
      "mentionedVersions": [],
      "currentRelevance": "likely",
      "relevanceReason": "The SkiaSharpGenerator already uses CppAst to generate C# P/Invoke bindings from C headers. The request to extend this to also generate the C shim layer remains open and unimplemented."
    }
  },
  "analysis": {
    "summary": "The reporter asks whether CppAst can be used to auto-generate the C API shim layer, removing the need to hand-maintain C wrapper functions in the mono/skia fork. The C# P/Invoke generation is already automated via SkiaSharpGenerator which uses CppAst to parse C headers. The remaining gap is the C layer itself (the C shim wrapping Skia's C++ API). The maintainer acknowledged this is feasible but noted complexity: carefully designed struct types (SKSize, SKPoint) need intentional ABI decisions, and dropping the native fork is not planned.",
    "codeInvestigation": [
      {
        "file": "utils/SkiaSharpGenerator/Generate/Generator.cs",
        "lines": "1-8",
        "finding": "The SkiaSharpGenerator already imports and uses CppAst to parse C headers and generate C# P/Invoke bindings. The CppAst dependency requested by the reporter is already in use for the C# side.",
        "relevance": "direct"
      },
      {
        "file": "utils/SkiaSharpGenerator/Program.cs",
        "lines": "1-30",
        "finding": "The tool exposes GenerateCommand (C# generation), CookieCommand, and VerifyCommand. No command exists for generating the C shim layer — confirming the feature is not yet implemented.",
        "relevance": "direct"
      }
    ],
    "keySignals": [
      {
        "text": "https://github.com/xoofx/CppAst can parse C++ headers, so can we generate C API automatically too?",
        "source": "issue body",
        "interpretation": "Reporter is proposing extending the existing C# generation toolchain to also generate the C wrapper layer from Skia C++ headers."
      },
      {
        "text": "Been a long time since you asked, but this is something I am wanting to do.",
        "source": "maintainer comment (mattleibow)",
        "interpretation": "Maintainer acknowledges the feature is desirable but not yet prioritized."
      },
      {
        "text": "the actual C++ API is a bit varied and will make tools a bit harder to write … I actually want the small, frequently used objects to be structs - SKSize and SKPoint.",
        "source": "maintainer comment (mattleibow)",
        "interpretation": "Key design constraint: not all C++ types should map mechanically — some structs must be intentionally hand-designed for performance."
      },
      {
        "text": "On the point of the don't fork the native repo, I don't think this will be possible in the near future",
        "source": "maintainer comment (mattleibow)",
        "interpretation": "The secondary benefit (drop the fork) is out of scope for now; the primary feature (C API generation) could still be pursued independently."
      }
    ],
    "rationale": "The title says [QUESTION] but the body and discussion reveal a feature request to auto-generate the C shim layer. CppAst is already in use for C# generation, so the infrastructure foundation is present. The C shim generation is a separate, more complex task involving ABI decisions. The maintainer has acknowledged feasibility and marked the issue as up-for-grabs with long-term/low-priority status. This warrants keep-open rather than close, as it is a valid long-term enhancement with maintainer endorsement.",
    "workarounds": [
      "Continue hand-writing C API functions in the mono/skia fork as currently done — the maintainer notes this is not particularly time-consuming",
      "Use SharpGenTools (https://github.com/SharpGenTools/SharpGenTools) as an alternative approach for generating C/C++ interop code"
    ],
    "nextQuestions": [
      "Would a partial generator (handling common type patterns automatically, with escape hatches for struct-typed objects) satisfy the use case?",
      "Has any community PR been attempted since the maintainer indicated PRs are welcome?"
    ],
    "resolution": {
      "hypothesis": "Extend SkiaSharpGenerator with a new command that reads Skia C++ headers via CppAst and emits C shim function stubs, with a config-driven override system for types that need custom mapping (e.g., struct-valued types like SKSize/SKPoint).",
      "proposals": [
        {
          "title": "Add GenerateCApi command to SkiaSharpGenerator",
          "description": "Extend utils/SkiaSharpGenerator with a new command that uses CppAst to parse Skia C++ headers and generate C shim (.h/.cpp) stubs into the externals/skia fork. A JSON config (similar to the existing C# generation config) would control type overrides and exclusions.",
          "category": "fix",
          "confidence": 0.7,
          "effort": "cost/xl",
          "validated": "untested"
        },
        {
          "title": "Use SharpGenTools as an alternative",
          "description": "Evaluate SharpGenTools (already suggested in comments) as a ready-made solution for generating C interop from C++ headers, potentially reducing implementation effort.",
          "category": "alternative",
          "confidence": 0.55,
          "effort": "cost/l",
          "validated": "untested"
        }
      ],
      "recommendedProposal": "Add GenerateCApi command to SkiaSharpGenerator",
      "recommendedReason": "Builds on the existing CppAst investment already in the toolchain and matches the current architecture pattern. Config-driven overrides address the maintainer's concern about struct-typed objects."
    }
  },
  "output": {
    "actionability": {
      "suggestedAction": "keep-open",
      "confidence": 0.85,
      "reason": "Valid long-term feature request acknowledged by maintainer. Already marked up-for-grabs. No new information to act on; the issue should remain open as a tracked enhancement.",
      "suggestedReproPlatform": "linux"
    },
    "actions": [
      {
        "type": "update-labels",
        "description": "Apply type/feature-request and area/Build labels",
        "risk": "low",
        "confidence": 0.88,
        "labels": [
          "type/feature-request",
          "area/Build"
        ]
      },
      {
        "type": "add-comment",
        "description": "Provide status update noting CppAst is already in use for C# generation and outline the remaining gap",
        "risk": "medium",
        "confidence": 0.8,
        "comment": "Thanks for the suggestion! As a status update: CppAst is already in use in `utils/SkiaSharpGenerator` to generate the C# P/Invoke bindings from C headers. The remaining gap — auto-generating the C shim layer itself — is the harder part, since some types (e.g., `SKSize`, `SKPoint`) need intentional ABI decisions rather than mechanical mapping.\n\nThis is still on the long-term roadmap and marked as **up-for-grabs**. If anyone wants to take a shot at extending `SkiaSharpGenerator` with a C-shim generation command (with a config-driven override system for custom type mappings), that would be very welcome!"
      }
    ]
  }
}
```

</details>
