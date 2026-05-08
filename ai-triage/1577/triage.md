# Issue Triage Report — #1577

| Field | Value |
|-------|-------|
| Repository | mono/SkiaSharp |
| Analyzed | 2026-04-29T12:21:04Z |
| Type | type/question (0.95 (95%)) |
| Area | area/SkiaSharp (0.90 (90%)) |
| Suggested action | close-as-not-a-bug (0.88 (88%)) |

**Issue Summary:** Reporter asks whether using fontName='Arial' with SKFontStyleWidth.Condensed is the correct way to obtain an 'Arial Narrow' SKTypeface, after finding that passing 'Arial Narrow' as the family name returns a Segoe UI fallback instead.

**Analysis:** The reporter is asking whether using fontName='Arial' + SKFontStyleWidth.Condensed is the correct way to load 'Arial Narrow'. This is a valid usage question: 'Arial Narrow' is not a separate font family on Windows; it is registered as a condensed-width variant of the 'Arial' family, so the workaround the reporter found is actually the correct approach. The behavior (falling back to Segoe UI when 'Arial Narrow' is not found as a family name) is by design.

**Recommendations:** **close-as-not-a-bug** — This is a usage question. The behavior is correct by design: 'Arial Narrow' is a width variant of 'Arial', not a separate family on Windows. The reporter already found the correct approach. Related to known issue #682.

---

## Classification

| Field | Value |
|-------|-------|
| Type | type/question |
| Area | area/SkiaSharp |
| Platforms | os/Windows-Classic |
| Backends | — |
| Tenets | tenet/compatibility |
| Partner | — |

## Evidence

### Reproduction

1. Call SKTypeface.FromFamilyName("Arial Narrow", SKFontStyleWeight.Normal, SKFontStyleWidth.Normal, SKFontStyleSlant.Upright)
2. Observe that returned typeface is 'Segoe UI' (the system default) instead of Arial Narrow
3. Workaround: use familyName='Arial' with SKFontStyleWidth.Condensed to get Arial Narrow correctly

**Related issues:** #682

**Repository links:**
- https://github.com/mono/SkiaSharp/issues/682 — Related issue: Can't load Arial Black as SKTypeface on Windows 10 — same underlying cause (variant fonts registered under parent family name)

**Code snippets:**

```csharp
SKTypeface skTypeface = SKTypeface.FromFamilyName("Arial Narrow", SKFontStyleWeight.Normal, SKFontStyleWidth.Normal, SKFontStyleSlant.Upright); // returns Segoe UI
```

```csharp
skTypeface = SKTypeface.FromFamilyName("Arial", SKFontStyleWeight.Normal, SKFontStyleWidth.Condensed, SKFontStyleSlant.Upright); // returns Arial Narrow correctly
```

### Version Analysis

| Field | Value |
|-------|-------|
| Mentioned versions | — |
| Worked in | — |
| Broke in | — |
| Current relevance | likely |
| Relevance reason | SKTypeface.FromFamilyName delegates directly to SKFontManager.Default.MatchFamily which calls sk_fontmgr_match_family_style — this delegation has not changed. |

## Analysis

### Technical Summary

The reporter is asking whether using fontName='Arial' + SKFontStyleWidth.Condensed is the correct way to load 'Arial Narrow'. This is a valid usage question: 'Arial Narrow' is not a separate font family on Windows; it is registered as a condensed-width variant of the 'Arial' family, so the workaround the reporter found is actually the correct approach. The behavior (falling back to Segoe UI when 'Arial Narrow' is not found as a family name) is by design.

### Rationale

The issue title contains '[QUESTION]' and the body explicitly asks 'Is it a correct way...?'. The reporter already found and described a working solution. No crash, no exception, and no wrong output — just uncertainty about the correct API usage. The behavior is by design: Skia's font manager searches for a font family by exact name; 'Arial Narrow' is not registered as its own family on Windows but as a condensed variant of 'Arial'. Issue #682 describes an identical phenomenon with 'Arial Black' and has been open for years as a known limitation/enhancement request.

### Key Signals

- "Is it a correct way to create a Narrow fonts SkTypeFace in SkiaSharp?" — **issue body** (This is a usage question, not a bug report. Reporter is seeking confirmation of the correct API usage pattern.)
- "Returns a TypeFace of default ('Segoe UI') font" — **issue body** (Expected behavior when the family name is not found — SKFontManager falls back to Default typeface.)
- "there currently isn't a fix for this, so you'll have to continue doing a workaround" — **comment by washamdev** (Community acknowledges this is a known limitation; the workaround (use parent family + width style) is the correct approach.)

### Code Investigation

| File | Lines | Relevance | Finding |
|------|-------|-----------|---------|
| `binding/SkiaSharp/SKTypeface.cs` | 79-80 | direct | SKTypeface.FromFamilyName(familyName, style) delegates to SKFontManager.Default.MatchFamily(familyName, style) ?? Default. When 'Arial Narrow' is not found as a distinct family, it returns the Default typeface (Segoe UI on Windows). This is expected behavior. |
| `binding/SkiaSharp/SKFontManager.cs` | 77-87 | direct | SKFontManager.MatchFamily calls native sk_fontmgr_match_family_style. If the family name is not registered as a top-level family (e.g., 'Arial Narrow' on Windows is a width variant of 'Arial', not its own family entry), Skia returns null and the C# code falls through to the Default typeface. |

### Workarounds

- Use SKTypeface.FromFamilyName("Arial", SKFontStyleWeight.Normal, SKFontStyleWidth.Condensed, SKFontStyleSlant.Upright) to load Arial Narrow.
- Use SKFontManager.Default.GetFontStyles("Arial") to enumerate available styles and find the condensed variant programmatically.
- Use SKTypeface.FromFile() with the path to the arial narrow font file (e.g., 'arialni.ttf') for explicit loading.

### Next Questions

- Is the reporter using a platform other than Windows? On macOS/Linux, Arial Narrow may be registered differently or absent.
- Would improved documentation for font variant matching (width/weight style) help users find the correct approach without filing an issue?

### Resolution Proposals

**Hypothesis:** Arial Narrow is a condensed-width variant of the Arial font family on Windows, not a separate family name. The correct approach is to request family 'Arial' with SKFontStyleWidth.Condensed, which the reporter already discovered.

1. **Answer with correct usage pattern** — workaround, confidence 0.92 (92%), cost/xs, validated=yes
   - Confirm that using fontName='Arial' + SKFontStyleWidth.Condensed is the correct approach and explain why 'Arial Narrow' is not a standalone family name.
2. **Enumerate font styles programmatically** — alternative, confidence 0.85 (85%), cost/xs, validated=yes
   - Use SKFontManager.Default.GetFontStyles("Arial") to list all available Arial variants and pick the correct style index.

**Recommended proposal:** Answer with correct usage pattern

**Why:** The reporter already found and implemented the correct workaround. A brief confirmation answer closes the question cleanly.

## Recommendations

### Actionability

| Field | Value |
|-------|-------|
| Suggested action | close-as-not-a-bug |
| Confidence | 0.88 (88%) |
| Reason | This is a usage question. The behavior is correct by design: 'Arial Narrow' is a width variant of 'Arial', not a separate family on Windows. The reporter already found the correct approach. Related to known issue #682. |
| Suggested repro platform | windows |

### Automatable Actions

| Type | Risk | Confidence | Description | Details |
|------|------|------------|-------------|---------|
| update-labels | low | 0.95 (95%) | Apply question and area labels | labels=type/question, area/SkiaSharp, os/Windows-Classic, tenet/compatibility |
| add-comment | medium | 0.88 (88%) | Confirm the workaround is correct and explain why | — |
| close-issue | medium | 0.85 (85%) | Close as answered — usage question, correct behavior | stateReason=completed |
| link-related | low | 0.95 (95%) | Cross-reference with related issue #682 | linkedIssue=#682 |

**Comment draft for `add-comment`:**

```markdown
Yes, using `fontName = "Arial"` with `SKFontStyleWidth.Condensed` is the correct approach for loading **Arial Narrow** on Windows.

On Windows, "Arial Narrow" is not registered as a standalone font family — it is a condensed-width variant of the "Arial" family. Skia's font manager looks up fonts by their registered family name, so passing `"Arial Narrow"` directly does not find a match and falls back to the system default (Segoe UI).

The correct pattern:
```csharp
var typeface = SKTypeface.FromFamilyName(
    "Arial",
    SKFontStyleWeight.Normal,
    SKFontStyleWidth.Condensed,
    SKFontStyleSlant.Upright);
```

Alternatively, you can enumerate the available styles for a family and pick by index:
```csharp
using var styles = SKFontManager.Default.GetFontStyles("Arial");
var typeface = styles.CreateTypeface(0); // inspect styles to find condensed
```

This is the same underlying behavior as #682 (Arial Black / Arial). I'll close this as answered — feel free to re-open if you have further questions.
```

<details>
<summary>Raw JSON</summary>

```json
{
  "meta": {
    "schemaVersion": "1.0",
    "number": 1577,
    "repo": "mono/SkiaSharp",
    "analyzedAt": "2026-04-29T12:21:04Z"
  },
  "summary": "Reporter asks whether using fontName='Arial' with SKFontStyleWidth.Condensed is the correct way to obtain an 'Arial Narrow' SKTypeface, after finding that passing 'Arial Narrow' as the family name returns a Segoe UI fallback instead.",
  "classification": {
    "type": {
      "value": "type/question",
      "confidence": 0.95
    },
    "area": {
      "value": "area/SkiaSharp",
      "confidence": 0.9
    },
    "platforms": [
      "os/Windows-Classic"
    ],
    "tenets": [
      "tenet/compatibility"
    ]
  },
  "evidence": {
    "reproEvidence": {
      "stepsToReproduce": [
        "Call SKTypeface.FromFamilyName(\"Arial Narrow\", SKFontStyleWeight.Normal, SKFontStyleWidth.Normal, SKFontStyleSlant.Upright)",
        "Observe that returned typeface is 'Segoe UI' (the system default) instead of Arial Narrow",
        "Workaround: use familyName='Arial' with SKFontStyleWidth.Condensed to get Arial Narrow correctly"
      ],
      "codeSnippets": [
        "SKTypeface skTypeface = SKTypeface.FromFamilyName(\"Arial Narrow\", SKFontStyleWeight.Normal, SKFontStyleWidth.Normal, SKFontStyleSlant.Upright); // returns Segoe UI",
        "skTypeface = SKTypeface.FromFamilyName(\"Arial\", SKFontStyleWeight.Normal, SKFontStyleWidth.Condensed, SKFontStyleSlant.Upright); // returns Arial Narrow correctly"
      ],
      "relatedIssues": [
        682
      ],
      "repoLinks": [
        {
          "url": "https://github.com/mono/SkiaSharp/issues/682",
          "description": "Related issue: Can't load Arial Black as SKTypeface on Windows 10 — same underlying cause (variant fonts registered under parent family name)"
        }
      ]
    },
    "versionAnalysis": {
      "mentionedVersions": [],
      "currentRelevance": "likely",
      "relevanceReason": "SKTypeface.FromFamilyName delegates directly to SKFontManager.Default.MatchFamily which calls sk_fontmgr_match_family_style — this delegation has not changed."
    }
  },
  "analysis": {
    "summary": "The reporter is asking whether using fontName='Arial' + SKFontStyleWidth.Condensed is the correct way to load 'Arial Narrow'. This is a valid usage question: 'Arial Narrow' is not a separate font family on Windows; it is registered as a condensed-width variant of the 'Arial' family, so the workaround the reporter found is actually the correct approach. The behavior (falling back to Segoe UI when 'Arial Narrow' is not found as a family name) is by design.",
    "rationale": "The issue title contains '[QUESTION]' and the body explicitly asks 'Is it a correct way...?'. The reporter already found and described a working solution. No crash, no exception, and no wrong output — just uncertainty about the correct API usage. The behavior is by design: Skia's font manager searches for a font family by exact name; 'Arial Narrow' is not registered as its own family on Windows but as a condensed variant of 'Arial'. Issue #682 describes an identical phenomenon with 'Arial Black' and has been open for years as a known limitation/enhancement request.",
    "keySignals": [
      {
        "text": "Is it a correct way to create a Narrow fonts SkTypeFace in SkiaSharp?",
        "source": "issue body",
        "interpretation": "This is a usage question, not a bug report. Reporter is seeking confirmation of the correct API usage pattern."
      },
      {
        "text": "Returns a TypeFace of default ('Segoe UI') font",
        "source": "issue body",
        "interpretation": "Expected behavior when the family name is not found — SKFontManager falls back to Default typeface."
      },
      {
        "text": "there currently isn't a fix for this, so you'll have to continue doing a workaround",
        "source": "comment by washamdev",
        "interpretation": "Community acknowledges this is a known limitation; the workaround (use parent family + width style) is the correct approach."
      }
    ],
    "codeInvestigation": [
      {
        "file": "binding/SkiaSharp/SKTypeface.cs",
        "lines": "79-80",
        "finding": "SKTypeface.FromFamilyName(familyName, style) delegates to SKFontManager.Default.MatchFamily(familyName, style) ?? Default. When 'Arial Narrow' is not found as a distinct family, it returns the Default typeface (Segoe UI on Windows). This is expected behavior.",
        "relevance": "direct"
      },
      {
        "file": "binding/SkiaSharp/SKFontManager.cs",
        "lines": "77-87",
        "finding": "SKFontManager.MatchFamily calls native sk_fontmgr_match_family_style. If the family name is not registered as a top-level family (e.g., 'Arial Narrow' on Windows is a width variant of 'Arial', not its own family entry), Skia returns null and the C# code falls through to the Default typeface.",
        "relevance": "direct"
      }
    ],
    "workarounds": [
      "Use SKTypeface.FromFamilyName(\"Arial\", SKFontStyleWeight.Normal, SKFontStyleWidth.Condensed, SKFontStyleSlant.Upright) to load Arial Narrow.",
      "Use SKFontManager.Default.GetFontStyles(\"Arial\") to enumerate available styles and find the condensed variant programmatically.",
      "Use SKTypeface.FromFile() with the path to the arial narrow font file (e.g., 'arialni.ttf') for explicit loading."
    ],
    "nextQuestions": [
      "Is the reporter using a platform other than Windows? On macOS/Linux, Arial Narrow may be registered differently or absent.",
      "Would improved documentation for font variant matching (width/weight style) help users find the correct approach without filing an issue?"
    ],
    "resolution": {
      "hypothesis": "Arial Narrow is a condensed-width variant of the Arial font family on Windows, not a separate family name. The correct approach is to request family 'Arial' with SKFontStyleWidth.Condensed, which the reporter already discovered.",
      "proposals": [
        {
          "title": "Answer with correct usage pattern",
          "description": "Confirm that using fontName='Arial' + SKFontStyleWidth.Condensed is the correct approach and explain why 'Arial Narrow' is not a standalone family name.",
          "category": "workaround",
          "confidence": 0.92,
          "effort": "cost/xs",
          "validated": "yes"
        },
        {
          "title": "Enumerate font styles programmatically",
          "description": "Use SKFontManager.Default.GetFontStyles(\"Arial\") to list all available Arial variants and pick the correct style index.",
          "category": "alternative",
          "confidence": 0.85,
          "effort": "cost/xs",
          "validated": "yes"
        }
      ],
      "recommendedProposal": "Answer with correct usage pattern",
      "recommendedReason": "The reporter already found and implemented the correct workaround. A brief confirmation answer closes the question cleanly."
    }
  },
  "output": {
    "actionability": {
      "suggestedAction": "close-as-not-a-bug",
      "confidence": 0.88,
      "reason": "This is a usage question. The behavior is correct by design: 'Arial Narrow' is a width variant of 'Arial', not a separate family on Windows. The reporter already found the correct approach. Related to known issue #682.",
      "suggestedReproPlatform": "windows"
    },
    "actions": [
      {
        "type": "update-labels",
        "description": "Apply question and area labels",
        "risk": "low",
        "confidence": 0.95,
        "labels": [
          "type/question",
          "area/SkiaSharp",
          "os/Windows-Classic",
          "tenet/compatibility"
        ]
      },
      {
        "type": "add-comment",
        "description": "Confirm the workaround is correct and explain why",
        "risk": "medium",
        "confidence": 0.88,
        "comment": "Yes, using `fontName = \"Arial\"` with `SKFontStyleWidth.Condensed` is the correct approach for loading **Arial Narrow** on Windows.\n\nOn Windows, \"Arial Narrow\" is not registered as a standalone font family — it is a condensed-width variant of the \"Arial\" family. Skia's font manager looks up fonts by their registered family name, so passing `\"Arial Narrow\"` directly does not find a match and falls back to the system default (Segoe UI).\n\nThe correct pattern:\n```csharp\nvar typeface = SKTypeface.FromFamilyName(\n    \"Arial\",\n    SKFontStyleWeight.Normal,\n    SKFontStyleWidth.Condensed,\n    SKFontStyleSlant.Upright);\n```\n\nAlternatively, you can enumerate the available styles for a family and pick by index:\n```csharp\nusing var styles = SKFontManager.Default.GetFontStyles(\"Arial\");\nvar typeface = styles.CreateTypeface(0); // inspect styles to find condensed\n```\n\nThis is the same underlying behavior as #682 (Arial Black / Arial). I'll close this as answered — feel free to re-open if you have further questions."
      },
      {
        "type": "close-issue",
        "description": "Close as answered — usage question, correct behavior",
        "risk": "medium",
        "confidence": 0.85,
        "stateReason": "completed"
      },
      {
        "type": "link-related",
        "description": "Cross-reference with related issue #682",
        "risk": "low",
        "confidence": 0.95,
        "linkedIssue": 682
      }
    ]
  }
}
```

</details>
