# Issue Triage Report — #911

| Field | Value |
|-------|-------|
| Repository | mono/SkiaSharp |
| Analyzed | 2026-06-15T06:03:09Z |
| Type | type/documentation (0.85 (85%)) |
| Area | area/Docs (0.90 (90%)) |
| Suggested action | keep-open (0.75 (75%)) |

**Issue Summary:** Feature request from 2019 to add a 'Getting Started' section to README.md with links to tutorials, API docs, and introductory materials.

**Analysis:** The README.md has improved significantly since this 2019 request — it now includes API docs badges, SkiaSharp Guides links, and a 'Using SkiaSharp' section — but still lacks a dedicated structured 'Getting Started' section with organized tutorial links. The tutorials mentioned in the request (Xamarin Forms) are now superseded by MAUI.

**Recommendations:** **keep-open** — Valid documentation improvement request. README still lacks a dedicated Getting Started section. The Xamarin tutorial links mentioned in the original issue are outdated but the underlying need remains valid for new users.

---

## Classification

| Field | Value |
|-------|-------|
| Type | type/documentation |
| Area | area/Docs |
| Platforms | — |
| Backends | — |
| Tenets | — |
| Partner | — |

## Evidence

## Analysis

### Technical Summary

The README.md has improved significantly since this 2019 request — it now includes API docs badges, SkiaSharp Guides links, and a 'Using SkiaSharp' section — but still lacks a dedicated structured 'Getting Started' section with organized tutorial links. The tutorials mentioned in the request (Xamarin Forms) are now superseded by MAUI.

### Rationale

This is a documentation improvement request filed in 2019. Code investigation confirms the README now has API docs and guide badges but no dedicated Getting Started section. The Xamarin Forms tutorial links in the original request are outdated. Classifying as type/documentation, area/Docs; suggested action keep-open since the enhancement remains valid for new users discovering SkiaSharp.

### Key Signals

- "I am just starting with using SkiaSharp and the first thing I noticed was it wasn't obvious as to where to go to get started." — **issue body** (Reporter found the onboarding experience confusing — a common new-user pain point that remains relevant even after README improvements.)
- "Could we add a 'Getting Started' to the readme.md?" — **issue body** (Specific request for a structured Getting Started section, which still does not exist in the current README.)

### Code Investigation

| File | Lines | Relevance | Finding |
|------|-------|-----------|---------|
| `README.md` | 1-95 | direct | README contains a 'Using SkiaSharp' section with NuGet install instructions, API docs badge, SkiaSharp Guides badge, and Discord link — but no dedicated 'Getting Started' section with structured tutorial links. |

### Resolution Proposals

**Hypothesis:** The README would benefit from a structured 'Getting Started' section with links to modern .NET MAUI documentation, quick-start guides, and sample code.

1. **Add Getting Started section to README** — fix, confidence 0.80 (80%), cost/xs, validated=untested
   - Add a 'Getting Started' section to README.md with links to .NET MAUI SkiaSharp tutorials, API documentation at docs.microsoft.com, the samples directory in the repo, and the Discord community. Update any outdated Xamarin-era tutorial links.
2. **Expand existing Using SkiaSharp section** — alternative, confidence 0.65 (65%), cost/xs, validated=untested
   - Expand the existing 'Using SkiaSharp' section with sub-headings for tutorials, guides, API reference, and community links rather than creating a separate section.

**Recommended proposal:** Add Getting Started section to README

**Why:** Direct, simple improvement to README that addresses the core request with minimal effort. Modern MAUI and SkiaSharp docs would replace the outdated Xamarin tutorial links.

## Recommendations

### Actionability

| Field | Value |
|-------|-------|
| Suggested action | keep-open |
| Confidence | 0.75 (75%) |
| Reason | Valid documentation improvement request. README still lacks a dedicated Getting Started section. The Xamarin tutorial links mentioned in the original issue are outdated but the underlying need remains valid for new users. |
| Suggested repro platform | linux |

### Automatable Actions

| Type | Risk | Confidence | Description | Details |
|------|------|------------|-------------|---------|
| update-labels | low | 0.90 (90%) | Apply documentation type and docs area labels | labels=type/documentation, area/Docs |
| add-comment | medium | 0.75 (75%) | Acknowledge the request and note tutorial links need updating for modern MAUI era | — |

**Comment draft for `add-comment`:**

```markdown
Thanks for this suggestion! The README has been updated since this was filed and now includes links to API docs and SkiaSharp Guides, but a dedicated **Getting Started** section is still missing.

If you'd like to contribute this, a PR updating the README with a structured Getting Started section would be welcome. Note that the Xamarin Forms tutorials mentioned here have been superseded by .NET MAUI resources. Suggested links to include:
- [SkiaSharp API Documentation](https://docs.microsoft.com/dotnet/api/SkiaSharp/)
- [SkiaSharp Guides](https://docs.microsoft.com/xamarin/graphics-games/skiasharp/)
- The `samples/` directory in the repo for code examples
- [.NET Discord](https://aka.ms/dotnet-discord) for community support
```

<details>
<summary>Raw JSON</summary>

```json
{
  "meta": {
    "schemaVersion": "1.0",
    "number": 911,
    "repo": "mono/SkiaSharp",
    "analyzedAt": "2026-06-15T06:03:09Z"
  },
  "summary": "Feature request from 2019 to add a 'Getting Started' section to README.md with links to tutorials, API docs, and introductory materials.",
  "classification": {
    "type": {
      "value": "type/documentation",
      "confidence": 0.85
    },
    "area": {
      "value": "area/Docs",
      "confidence": 0.9
    }
  },
  "evidence": {},
  "analysis": {
    "summary": "The README.md has improved significantly since this 2019 request — it now includes API docs badges, SkiaSharp Guides links, and a 'Using SkiaSharp' section — but still lacks a dedicated structured 'Getting Started' section with organized tutorial links. The tutorials mentioned in the request (Xamarin Forms) are now superseded by MAUI.",
    "rationale": "This is a documentation improvement request filed in 2019. Code investigation confirms the README now has API docs and guide badges but no dedicated Getting Started section. The Xamarin Forms tutorial links in the original request are outdated. Classifying as type/documentation, area/Docs; suggested action keep-open since the enhancement remains valid for new users discovering SkiaSharp.",
    "keySignals": [
      {
        "text": "I am just starting with using SkiaSharp and the first thing I noticed was it wasn't obvious as to where to go to get started.",
        "source": "issue body",
        "interpretation": "Reporter found the onboarding experience confusing — a common new-user pain point that remains relevant even after README improvements."
      },
      {
        "text": "Could we add a 'Getting Started' to the readme.md?",
        "source": "issue body",
        "interpretation": "Specific request for a structured Getting Started section, which still does not exist in the current README."
      }
    ],
    "codeInvestigation": [
      {
        "file": "README.md",
        "lines": "1-95",
        "finding": "README contains a 'Using SkiaSharp' section with NuGet install instructions, API docs badge, SkiaSharp Guides badge, and Discord link — but no dedicated 'Getting Started' section with structured tutorial links.",
        "relevance": "direct"
      }
    ],
    "resolution": {
      "hypothesis": "The README would benefit from a structured 'Getting Started' section with links to modern .NET MAUI documentation, quick-start guides, and sample code.",
      "proposals": [
        {
          "title": "Add Getting Started section to README",
          "description": "Add a 'Getting Started' section to README.md with links to .NET MAUI SkiaSharp tutorials, API documentation at docs.microsoft.com, the samples directory in the repo, and the Discord community. Update any outdated Xamarin-era tutorial links.",
          "category": "fix",
          "confidence": 0.8,
          "effort": "cost/xs",
          "validated": "untested"
        },
        {
          "title": "Expand existing Using SkiaSharp section",
          "description": "Expand the existing 'Using SkiaSharp' section with sub-headings for tutorials, guides, API reference, and community links rather than creating a separate section.",
          "category": "alternative",
          "confidence": 0.65,
          "effort": "cost/xs",
          "validated": "untested"
        }
      ],
      "recommendedProposal": "Add Getting Started section to README",
      "recommendedReason": "Direct, simple improvement to README that addresses the core request with minimal effort. Modern MAUI and SkiaSharp docs would replace the outdated Xamarin tutorial links."
    }
  },
  "output": {
    "actionability": {
      "suggestedAction": "keep-open",
      "confidence": 0.75,
      "reason": "Valid documentation improvement request. README still lacks a dedicated Getting Started section. The Xamarin tutorial links mentioned in the original issue are outdated but the underlying need remains valid for new users.",
      "suggestedReproPlatform": "linux"
    },
    "actions": [
      {
        "type": "update-labels",
        "description": "Apply documentation type and docs area labels",
        "risk": "low",
        "confidence": 0.9,
        "labels": [
          "type/documentation",
          "area/Docs"
        ]
      },
      {
        "type": "add-comment",
        "description": "Acknowledge the request and note tutorial links need updating for modern MAUI era",
        "risk": "medium",
        "confidence": 0.75,
        "comment": "Thanks for this suggestion! The README has been updated since this was filed and now includes links to API docs and SkiaSharp Guides, but a dedicated **Getting Started** section is still missing.\n\nIf you'd like to contribute this, a PR updating the README with a structured Getting Started section would be welcome. Note that the Xamarin Forms tutorials mentioned here have been superseded by .NET MAUI resources. Suggested links to include:\n- [SkiaSharp API Documentation](https://docs.microsoft.com/dotnet/api/SkiaSharp/)\n- [SkiaSharp Guides](https://docs.microsoft.com/xamarin/graphics-games/skiasharp/)\n- The `samples/` directory in the repo for code examples\n- [.NET Discord](https://aka.ms/dotnet-discord) for community support"
      }
    ]
  }
}
```

</details>
