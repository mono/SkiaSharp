# Issue Triage Report — #2790

| Field | Value |
|-------|-------|
| Repository | mono/SkiaSharp |
| Analyzed | 2026-04-27T21:59:46Z |
| Type | type/enhancement (0.92 (92%)) |
| Area | area/SkiaSharp (0.95 (95%)) |
| Suggested action | keep-open (0.90 (90%)) |

**Issue Summary:** Maintainer-created tracking issue for improving 2.x/3.x API compatibility by adding hidden backward-compat APIs in 3.x and forward-compat APIs in 2.x to ease the upgrade path.

**Analysis:** Maintainer tracking issue for a two-part compatibility effort: (1) adding hidden backward-compat overloads/APIs in 3.x so upgrading apps need fewer code changes, and (2) backporting new 3.x-style APIs to 2.x so apps can migrate ahead of the version bump. Code inspection confirms several compatibility shims have already landed (SKCanvas.SetMatrix with `in` modifier with an [Obsolete] bridge, SKPath.Transform with `in` modifier, SKImageFilter.CreateBlur/CreateMerge with SKRect overloads replacing CropRect).

**Recommendations:** **keep-open** — This is a maintainer-created tracking issue with clear implementation plan and linked PRs; it should remain open until all compatibility work is complete.

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
| Current labels | tenet/compatibility |

## Evidence

### Reproduction

**Repository links:**
- https://github.com/mono/SkiaSharp/pull/2789 — Add hidden 3.x APIs for 2.x compat
- https://github.com/mono/SkiaSharp/pull/2810 — Add forward-compat APIs to 2.x
- https://github.com/mono/SkiaSharp/pull/2756 — Related compatibility PR
- https://github.com/mono/SkiaSharp/pull/2775 — Related compatibility PR
- https://github.com/mono/SkiaSharp/blob/main/changelogs/SkiaSharp/3.0.0/SkiaSharp.humanreadable.md — Human-readable API diff for SkiaSharp 3.0.0

## Analysis

### Technical Summary

Maintainer tracking issue for a two-part compatibility effort: (1) adding hidden backward-compat overloads/APIs in 3.x so upgrading apps need fewer code changes, and (2) backporting new 3.x-style APIs to 2.x so apps can migrate ahead of the version bump. Code inspection confirms several compatibility shims have already landed (SKCanvas.SetMatrix with `in` modifier with an [Obsolete] bridge, SKPath.Transform with `in` modifier, SKImageFilter.CreateBlur/CreateMerge with SKRect overloads replacing CropRect).

### Rationale

The issue is authored by a core maintainer and describes deliberate planned work with concrete PRs. It is a type/enhancement because it improves an existing capability (upgrade experience) rather than fixing broken behavior. The `tenet/compatibility` label already reflects the quality tenet. The issue should remain open as a tracking item until all referenced PRs land and the compatibility story is complete.

### Key Signals

- "Add "hidden" APIs in 3.x that allow for upgrades where API usage is small or restricted to common things." — **issue body** (The maintainer is deliberately adding compatibility shims to reduce upgrade friction.)
- "Add new APIs in 2.x so that people can use the new APIs today, and then they can upgrade to 3.x without any changes." — **issue body** (Forward-compat strategy: teach 2.x apps to use 3.x patterns ahead of time.)
- "Fantastic effort on easing migration and adoption of v3!" — **comment by follesoe** (Community validation of the approach.)

### Code Investigation

| File | Lines | Relevance | Finding |
|------|-------|-----------|---------|
| `binding/SkiaSharp/SKCanvas.cs` | 895-907 | direct | SKCanvas.SetMatrix(in SKMatrix) exists in 3.x; old by-value overload is marked [Obsolete(error:true)] bridging to the new API — confirms the hidden-API compatibility pattern described in the issue has been applied here. |
| `binding/SkiaSharp/SKPath.cs` | 331-352 | direct | SKPath.Transform(in SKMatrix) is the new 3.x signature; old by-value overloads are marked [Obsolete(error:true)] bridging to it — same pattern as SKCanvas.SetMatrix. |
| `binding/SkiaSharp/SKImageFilter.cs` | 15-35 | direct | SKImageFilter.CreateMatrix overloads with `in SKMatrix` added; old by-value overloads are [Obsolete(error:true)] redirecting to them. SKImageFilter.CreateBlur has SKRect-based overloads replacing the removed CropRect parameter. |
| `changelogs/SkiaSharp/3.0.0/SkiaSharp.breaking.md` | 1-50 | related | The changelog confirms many APIs were removed in 3.x that were already marked [Obsolete] in 2.x, providing the full scope of compatibility work covered by this tracking issue. |

### Resolution Proposals

**Hypothesis:** The work is partially complete (several compatibility shims have landed) but the issue remains open as a tracking item for all linked PRs and any remaining gaps.

1. **Keep open as tracking issue** — investigation, confidence 0.90 (90%), cost/xs, validated=untested
   - Leave the issue open until all referenced PRs (#2789, #2810, #2756, #2775) are merged and the full compatibility surface is confirmed.

**Recommended proposal:** Keep open as tracking issue

**Why:** Maintainer-authored tracking issues should stay open until the described work is fully complete and closed by the maintainer.

## Recommendations

### Actionability

| Field | Value |
|-------|-------|
| Suggested action | keep-open |
| Confidence | 0.90 (90%) |
| Reason | This is a maintainer-created tracking issue with clear implementation plan and linked PRs; it should remain open until all compatibility work is complete. |
| Suggested repro platform | linux |

### Automatable Actions

| Type | Risk | Confidence | Description | Details |
|------|------|------------|-------------|---------|
| update-labels | low | 0.92 (92%) | Apply enhancement and SkiaSharp area labels alongside existing tenet/compatibility | labels=type/enhancement, area/SkiaSharp, tenet/compatibility |

<details>
<summary>Raw JSON</summary>

```json
{
  "meta": {
    "schemaVersion": "1.0",
    "number": 2790,
    "repo": "mono/SkiaSharp",
    "analyzedAt": "2026-04-27T21:59:46Z",
    "currentLabels": [
      "tenet/compatibility"
    ]
  },
  "summary": "Maintainer-created tracking issue for improving 2.x/3.x API compatibility by adding hidden backward-compat APIs in 3.x and forward-compat APIs in 2.x to ease the upgrade path.",
  "classification": {
    "type": {
      "value": "type/enhancement",
      "confidence": 0.92
    },
    "area": {
      "value": "area/SkiaSharp",
      "confidence": 0.95
    },
    "tenets": [
      "tenet/compatibility"
    ]
  },
  "evidence": {
    "reproEvidence": {
      "repoLinks": [
        {
          "url": "https://github.com/mono/SkiaSharp/pull/2789",
          "description": "Add hidden 3.x APIs for 2.x compat"
        },
        {
          "url": "https://github.com/mono/SkiaSharp/pull/2810",
          "description": "Add forward-compat APIs to 2.x"
        },
        {
          "url": "https://github.com/mono/SkiaSharp/pull/2756",
          "description": "Related compatibility PR"
        },
        {
          "url": "https://github.com/mono/SkiaSharp/pull/2775",
          "description": "Related compatibility PR"
        },
        {
          "url": "https://github.com/mono/SkiaSharp/blob/main/changelogs/SkiaSharp/3.0.0/SkiaSharp.humanreadable.md",
          "description": "Human-readable API diff for SkiaSharp 3.0.0"
        }
      ]
    }
  },
  "analysis": {
    "summary": "Maintainer tracking issue for a two-part compatibility effort: (1) adding hidden backward-compat overloads/APIs in 3.x so upgrading apps need fewer code changes, and (2) backporting new 3.x-style APIs to 2.x so apps can migrate ahead of the version bump. Code inspection confirms several compatibility shims have already landed (SKCanvas.SetMatrix with `in` modifier with an [Obsolete] bridge, SKPath.Transform with `in` modifier, SKImageFilter.CreateBlur/CreateMerge with SKRect overloads replacing CropRect).",
    "rationale": "The issue is authored by a core maintainer and describes deliberate planned work with concrete PRs. It is a type/enhancement because it improves an existing capability (upgrade experience) rather than fixing broken behavior. The `tenet/compatibility` label already reflects the quality tenet. The issue should remain open as a tracking item until all referenced PRs land and the compatibility story is complete.",
    "codeInvestigation": [
      {
        "file": "binding/SkiaSharp/SKCanvas.cs",
        "lines": "895-907",
        "finding": "SKCanvas.SetMatrix(in SKMatrix) exists in 3.x; old by-value overload is marked [Obsolete(error:true)] bridging to the new API — confirms the hidden-API compatibility pattern described in the issue has been applied here.",
        "relevance": "direct"
      },
      {
        "file": "binding/SkiaSharp/SKPath.cs",
        "lines": "331-352",
        "finding": "SKPath.Transform(in SKMatrix) is the new 3.x signature; old by-value overloads are marked [Obsolete(error:true)] bridging to it — same pattern as SKCanvas.SetMatrix.",
        "relevance": "direct"
      },
      {
        "file": "binding/SkiaSharp/SKImageFilter.cs",
        "lines": "15-35",
        "finding": "SKImageFilter.CreateMatrix overloads with `in SKMatrix` added; old by-value overloads are [Obsolete(error:true)] redirecting to them. SKImageFilter.CreateBlur has SKRect-based overloads replacing the removed CropRect parameter.",
        "relevance": "direct"
      },
      {
        "file": "changelogs/SkiaSharp/3.0.0/SkiaSharp.breaking.md",
        "lines": "1-50",
        "finding": "The changelog confirms many APIs were removed in 3.x that were already marked [Obsolete] in 2.x, providing the full scope of compatibility work covered by this tracking issue.",
        "relevance": "related"
      }
    ],
    "keySignals": [
      {
        "text": "Add \"hidden\" APIs in 3.x that allow for upgrades where API usage is small or restricted to common things.",
        "source": "issue body",
        "interpretation": "The maintainer is deliberately adding compatibility shims to reduce upgrade friction."
      },
      {
        "text": "Add new APIs in 2.x so that people can use the new APIs today, and then they can upgrade to 3.x without any changes.",
        "source": "issue body",
        "interpretation": "Forward-compat strategy: teach 2.x apps to use 3.x patterns ahead of time."
      },
      {
        "text": "Fantastic effort on easing migration and adoption of v3!",
        "source": "comment by follesoe",
        "interpretation": "Community validation of the approach."
      }
    ],
    "resolution": {
      "hypothesis": "The work is partially complete (several compatibility shims have landed) but the issue remains open as a tracking item for all linked PRs and any remaining gaps.",
      "proposals": [
        {
          "title": "Keep open as tracking issue",
          "description": "Leave the issue open until all referenced PRs (#2789, #2810, #2756, #2775) are merged and the full compatibility surface is confirmed.",
          "category": "investigation",
          "confidence": 0.9,
          "effort": "cost/xs",
          "validated": "untested"
        }
      ],
      "recommendedProposal": "Keep open as tracking issue",
      "recommendedReason": "Maintainer-authored tracking issues should stay open until the described work is fully complete and closed by the maintainer."
    }
  },
  "output": {
    "actionability": {
      "suggestedAction": "keep-open",
      "confidence": 0.9,
      "reason": "This is a maintainer-created tracking issue with clear implementation plan and linked PRs; it should remain open until all compatibility work is complete.",
      "suggestedReproPlatform": "linux"
    },
    "actions": [
      {
        "type": "update-labels",
        "description": "Apply enhancement and SkiaSharp area labels alongside existing tenet/compatibility",
        "risk": "low",
        "confidence": 0.92,
        "labels": [
          "type/enhancement",
          "area/SkiaSharp",
          "tenet/compatibility"
        ]
      }
    ]
  }
}
```

</details>
