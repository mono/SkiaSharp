# Issue Triage Report — #3328

| Field | Value |
|-------|-------|
| Repository | mono/SkiaSharp |
| Analyzed | 2026-04-28T05:14:37Z |
| Type | type/bug (0.88 (88%)) |
| Area | area/SkiaSharp (0.90 (90%)) |
| Suggested action | needs-investigation (0.82 (82%)) |

**Issue Summary:** SKFont.GetTextPath returns an empty path (zero width/height bounds) for emoji characters on Windows 11 using Segoe UI Emoji font in SkiaSharp 3.119.0, reportedly working in 2.88.9.

**Analysis:** SKFont.GetTextPath calls Skia's SkTextUtils::GetPath via sk_text_utils_get_path, which constructs a path by accumulating individual glyph outlines. Modern color emoji fonts like Segoe UI Emoji on Windows 11 use COLR (v1) or CBDT/CBLC bitmap formats rather than traditional vector outlines. Skia's SkFont::getPath() returns an empty result for such glyphs because there is no conventional outline to extract, resulting in an empty overall path. The regression claim from 2.88.9 is plausible given major Skia version changes between 2.88.x and 3.x SkiaSharp releases.

**Recommendations:** **needs-investigation** — Regression claim from 2.88.9 to 3.119.0 with code reproduction steps. Root cause (color emoji font path handling in Skia) needs verification and the GetGlyphPath workaround needs confirmation.

---

## Classification

| Field | Value |
|-------|-------|
| Type | type/bug |
| Area | area/SkiaSharp |
| Platforms | os/Windows-Classic |
| Backends | — |
| Tenets | tenet/compatibility |
| Partner | — |
| Current labels | type/bug |

## Evidence

### Reproduction

1. Create an SKTypeface from 'Segoe UI Emoji' using SKTypeface.FromFamilyName
2. Create an SKFont with size 48
3. Call fontTest.GetTextPath with emoji string and new SKPoint(0, 0)
4. Observe path.Bounds returns empty rect (0 width and 0 height)

**Environment:** Windows 11, SkiaSharp 3.119.0, HarfBuzz 3.119.0, Visual Studio

**Related issues:** #3244

### Bug Signals

| Field | Value |
|-------|-------|
| Severity | medium |
| Regression claimed | True |
| Error type | wrong-output |
| Error message | path.Bounds shows 0 width and 0 height for emoji character |
| Repro quality | partial |
| Target frameworks | — |

### Version Analysis

| Field | Value |
|-------|-------|
| Mentioned versions | 3.119.0, 2.88.9 |
| Worked in | 2.88.9 |
| Broke in | 3.119.0 |
| Current relevance | likely |
| Relevance reason | No fix has been identified in the GetTextPath code path for color emoji font handling. |

### Regression

| Field | Value |
|-------|-------|
| Is regression | True |
| Confidence | 0.65 (65%) |
| Reason | Reporter explicitly states the feature worked in 2.88.9. The Skia library was significantly upgraded between SkiaSharp 2.88.x and 3.x (multiple Skia milestones), which may have changed COLR/bitmap emoji path extraction behavior in SkTextUtils::GetPath. |
| Worked in version | 2.88.9 |
| Broke in version | 3.119.0 |

## Analysis

### Technical Summary

SKFont.GetTextPath calls Skia's SkTextUtils::GetPath via sk_text_utils_get_path, which constructs a path by accumulating individual glyph outlines. Modern color emoji fonts like Segoe UI Emoji on Windows 11 use COLR (v1) or CBDT/CBLC bitmap formats rather than traditional vector outlines. Skia's SkFont::getPath() returns an empty result for such glyphs because there is no conventional outline to extract, resulting in an empty overall path. The regression claim from 2.88.9 is plausible given major Skia version changes between 2.88.x and 3.x SkiaSharp releases.

### Rationale

Classified as type/bug because the reporter explicitly claims a regression from 2.88.9 where the API previously returned a usable path and now returns empty. Area is area/SkiaSharp since the issue is in the SKFont.GetTextPath binding. Platform is os/Windows-Classic because Segoe UI Emoji is a Windows-specific font. tenet/compatibility applies due to the behavior change across major SkiaSharp versions. The root cause is likely in Skia's handling of color emoji glyphs, but investigation is needed to confirm the regression and identify a workaround.

### Key Signals

- "All works as expected, except for emoji characters" — **issue body** (The issue is specific to emoji glyphs, which use color/bitmap font formats without extractable outline paths via the standard Skia path API.)
- "Last Known Good Version: 2.88.9 (Previous)" — **issue body** (Regression claim between SkiaSharp 2.88.9 and 3.119.0. This correlates with a major Skia upstream version bump that may have changed COLR emoji handling.)
- "fontFamily = Segoe UI Emoji" — **issue body** (Segoe UI Emoji on Windows 11 uses COLR v1 format (vector glyphs with gradient/color layers). These glyphs may not expose outline paths the same way they did in older Skia builds.)

### Code Investigation

| File | Lines | Relevance | Finding |
|------|-------|-----------|---------|
| `binding/SkiaSharp/SKFont.cs` | 746-754 | direct | GetTextPath (internal, void*) calls sk_text_utils_get_path which maps to Skia's SkTextUtils::GetPath. This function iterates over glyphs and calls SkFont::getPath() for each, returning an empty path for color/bitmap emoji glyphs (COLR, CBDT/CBLC) that have no traditional vector outline. |
| `binding/SkiaSharp/SKFont.cs` | 712-722 | related | GetGlyphPath calls sk_font_get_path (SkFont::getPath) and returns null when the function returns false. This is the same underlying limitation as GetTextPath for color emoji. However, for COLR v0 fonts, individual layer outlines might be accessible via this method. |

### Workarounds

- Use a non-color emoji font with traditional vector outlines rather than COLR or bitmap formats. GetTextPath will produce proper paths from outline-based fonts.
- Render the emoji text to a bitmap SKSurface using DrawText or DrawShapedText, then process the rasterized output instead of vector path operations.

### Next Questions

- Does SkiaSharp 2.88.9 actually return a non-empty path for Segoe UI Emoji, or is the regression claim incorrect?
- Which emoji glyph format does Segoe UI Emoji use on Windows 11 (COLR v0, COLR v1, CBDT/CBLC)?
- Does GetGlyphPath on individual glyph IDs from GetGlyphs return non-null paths for the emoji?
- Does the issue also reproduce with other emoji fonts such as Noto Color Emoji?

### Resolution Proposals

**Hypothesis:** Emoji glyphs in color fonts (COLR v1/CBDT) do not expose traditional outline paths through SkTextUtils::GetPath. A Skia version change between 2.88.x and 3.x may have altered how COLR emoji path extraction behaves.

1. **Try GetGlyphPath on individual emoji glyph IDs** — workaround, confidence 0.50 (50%), cost/xs, validated=untested
   - Use GetGlyphs to convert emoji text to glyph IDs, then GetGlyphPath on each glyph ID. For COLR v0 fonts, the component outline paths may still be accessible even if GetTextPath returns empty.

```csharp
var glyphs = fontTest.GetGlyphs(text);
var glyphPositions = fontTest.GetGlyphPositions(glyphs, new SKPoint(0, 0));
using var combinedPath = new SKPath();
for (int i = 0; i < glyphs.Length; i++) {
    using var glyphPath = fontTest.GetGlyphPath(glyphs[i]);
    if (glyphPath != null) {
        var offset = SKMatrix.CreateTranslation(glyphPositions[i].X, glyphPositions[i].Y);
        combinedPath.AddPath(glyphPath, in offset);
    }
}
// Note: returns null for CBDT/CBLC bitmap emoji. Results vary for COLR emoji.
```
2. **Render emoji to bitmap surface instead of path** — alternative, confidence 0.80 (80%), cost/s, validated=untested
   - For color emoji processing, render the text to an SKSurface and work with the resulting bitmap rather than attempting path extraction.
3. **Investigate COLR emoji path regression in Skia upgrade** — investigation, confidence 0.75 (75%), cost/m, validated=untested
   - Compare Skia's SkTextUtils::GetPath and SkFont::getPath COLR behavior between the Skia milestone used in SkiaSharp 2.88.9 vs 3.119.0 to confirm or refute the regression.

**Recommended proposal:** Try GetGlyphPath on individual emoji glyph IDs

**Why:** Simple to test first. If it works for the reporter's Segoe UI Emoji COLR font, it provides an immediate workaround. If also empty, it confirms the limitation is fundamental to Skia color emoji path extraction, guiding the fix direction.

## Recommendations

### Actionability

| Field | Value |
|-------|-------|
| Suggested action | needs-investigation |
| Confidence | 0.82 (82%) |
| Reason | Regression claim from 2.88.9 to 3.119.0 with code reproduction steps. Root cause (color emoji font path handling in Skia) needs verification and the GetGlyphPath workaround needs confirmation. |
| Suggested repro platform | windows |

### Automatable Actions

| Type | Risk | Confidence | Description | Details |
|------|------|------------|-------------|---------|
| update-labels | low | 0.90 (90%) | Apply bug, SkiaSharp area, Windows platform, and compatibility tenet labels | labels=type/bug, area/SkiaSharp, os/Windows-Classic, tenet/compatibility |
| add-comment | medium | 0.75 (75%) | Post analysis of color emoji path limitation with workaround suggestions | — |

**Comment draft for `add-comment`:**

```markdown
Thanks for the detailed report!

The empty path for emoji is likely related to how modern emoji fonts store glyph data. **Segoe UI Emoji** on Windows 11 uses **COLR** (color-layered vector) format rather than traditional outline paths. Skia's `GetTextPath` (via `SkTextUtils::GetPath`) extracts glyph outlines — for COLR or bitmap (CBDT/CBLC) emoji glyphs, there may be no conventional outline to extract, resulting in an empty path.

While we investigate the reported regression from 2.88.9, please try these workarounds:

**Option 1 — Use GetGlyphPath on individual glyph IDs:**
```csharp
var glyphs = fontTest.GetGlyphs(text);
var glyphPositions = fontTest.GetGlyphPositions(glyphs, new SKPoint(0, 0));
using var combinedPath = new SKPath();
for (int i = 0; i < glyphs.Length; i++) {
    using var glyphPath = fontTest.GetGlyphPath(glyphs[i]);
    if (glyphPath != null) {
        var offset = SKMatrix.CreateTranslation(glyphPositions[i].X, glyphPositions[i].Y);
        combinedPath.AddPath(glyphPath, in offset);
    }
}
```
*Note: Results depend on the COLR format version used by the font. This may also return empty for pure bitmap emoji glyphs.*

**Option 2 — Render to bitmap surface:**
For processing emoji appearance, render to a temporary `SKSurface` with `DrawText`/`DrawShapedText` and work with the rasterized bitmap output.

Could you confirm: does `GetGlyphPath` on the individual glyph IDs from `fontTest.GetGlyphs("\uD83D\uDE0A")` return a non-null path? That would help determine whether this is a `GetTextPath`-specific issue or a deeper Skia limitation with COLR emoji on Windows 11.
```

<details>
<summary>Raw JSON</summary>

```json
{
  "meta": {
    "schemaVersion": "1.0",
    "number": 3328,
    "repo": "mono/SkiaSharp",
    "analyzedAt": "2026-04-28T05:14:37Z",
    "currentLabels": [
      "type/bug"
    ]
  },
  "summary": "SKFont.GetTextPath returns an empty path (zero width/height bounds) for emoji characters on Windows 11 using Segoe UI Emoji font in SkiaSharp 3.119.0, reportedly working in 2.88.9.",
  "classification": {
    "type": {
      "value": "type/bug",
      "confidence": 0.88
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
    "bugSignals": {
      "severity": "medium",
      "regressionClaimed": true,
      "errorType": "wrong-output",
      "errorMessage": "path.Bounds shows 0 width and 0 height for emoji character",
      "reproQuality": "partial"
    },
    "reproEvidence": {
      "stepsToReproduce": [
        "Create an SKTypeface from 'Segoe UI Emoji' using SKTypeface.FromFamilyName",
        "Create an SKFont with size 48",
        "Call fontTest.GetTextPath with emoji string and new SKPoint(0, 0)",
        "Observe path.Bounds returns empty rect (0 width and 0 height)"
      ],
      "environmentDetails": "Windows 11, SkiaSharp 3.119.0, HarfBuzz 3.119.0, Visual Studio",
      "relatedIssues": [
        3244
      ]
    },
    "versionAnalysis": {
      "mentionedVersions": [
        "3.119.0",
        "2.88.9"
      ],
      "workedIn": "2.88.9",
      "brokeIn": "3.119.0",
      "currentRelevance": "likely",
      "relevanceReason": "No fix has been identified in the GetTextPath code path for color emoji font handling."
    },
    "regression": {
      "isRegression": true,
      "confidence": 0.65,
      "reason": "Reporter explicitly states the feature worked in 2.88.9. The Skia library was significantly upgraded between SkiaSharp 2.88.x and 3.x (multiple Skia milestones), which may have changed COLR/bitmap emoji path extraction behavior in SkTextUtils::GetPath.",
      "workedInVersion": "2.88.9",
      "brokeInVersion": "3.119.0"
    }
  },
  "analysis": {
    "summary": "SKFont.GetTextPath calls Skia's SkTextUtils::GetPath via sk_text_utils_get_path, which constructs a path by accumulating individual glyph outlines. Modern color emoji fonts like Segoe UI Emoji on Windows 11 use COLR (v1) or CBDT/CBLC bitmap formats rather than traditional vector outlines. Skia's SkFont::getPath() returns an empty result for such glyphs because there is no conventional outline to extract, resulting in an empty overall path. The regression claim from 2.88.9 is plausible given major Skia version changes between 2.88.x and 3.x SkiaSharp releases.",
    "codeInvestigation": [
      {
        "file": "binding/SkiaSharp/SKFont.cs",
        "lines": "746-754",
        "finding": "GetTextPath (internal, void*) calls sk_text_utils_get_path which maps to Skia's SkTextUtils::GetPath. This function iterates over glyphs and calls SkFont::getPath() for each, returning an empty path for color/bitmap emoji glyphs (COLR, CBDT/CBLC) that have no traditional vector outline.",
        "relevance": "direct"
      },
      {
        "file": "binding/SkiaSharp/SKFont.cs",
        "lines": "712-722",
        "finding": "GetGlyphPath calls sk_font_get_path (SkFont::getPath) and returns null when the function returns false. This is the same underlying limitation as GetTextPath for color emoji. However, for COLR v0 fonts, individual layer outlines might be accessible via this method.",
        "relevance": "related"
      }
    ],
    "keySignals": [
      {
        "text": "All works as expected, except for emoji characters",
        "source": "issue body",
        "interpretation": "The issue is specific to emoji glyphs, which use color/bitmap font formats without extractable outline paths via the standard Skia path API."
      },
      {
        "text": "Last Known Good Version: 2.88.9 (Previous)",
        "source": "issue body",
        "interpretation": "Regression claim between SkiaSharp 2.88.9 and 3.119.0. This correlates with a major Skia upstream version bump that may have changed COLR emoji handling."
      },
      {
        "text": "fontFamily = Segoe UI Emoji",
        "source": "issue body",
        "interpretation": "Segoe UI Emoji on Windows 11 uses COLR v1 format (vector glyphs with gradient/color layers). These glyphs may not expose outline paths the same way they did in older Skia builds."
      }
    ],
    "rationale": "Classified as type/bug because the reporter explicitly claims a regression from 2.88.9 where the API previously returned a usable path and now returns empty. Area is area/SkiaSharp since the issue is in the SKFont.GetTextPath binding. Platform is os/Windows-Classic because Segoe UI Emoji is a Windows-specific font. tenet/compatibility applies due to the behavior change across major SkiaSharp versions. The root cause is likely in Skia's handling of color emoji glyphs, but investigation is needed to confirm the regression and identify a workaround.",
    "nextQuestions": [
      "Does SkiaSharp 2.88.9 actually return a non-empty path for Segoe UI Emoji, or is the regression claim incorrect?",
      "Which emoji glyph format does Segoe UI Emoji use on Windows 11 (COLR v0, COLR v1, CBDT/CBLC)?",
      "Does GetGlyphPath on individual glyph IDs from GetGlyphs return non-null paths for the emoji?",
      "Does the issue also reproduce with other emoji fonts such as Noto Color Emoji?"
    ],
    "workarounds": [
      "Use a non-color emoji font with traditional vector outlines rather than COLR or bitmap formats. GetTextPath will produce proper paths from outline-based fonts.",
      "Render the emoji text to a bitmap SKSurface using DrawText or DrawShapedText, then process the rasterized output instead of vector path operations."
    ],
    "resolution": {
      "hypothesis": "Emoji glyphs in color fonts (COLR v1/CBDT) do not expose traditional outline paths through SkTextUtils::GetPath. A Skia version change between 2.88.x and 3.x may have altered how COLR emoji path extraction behaves.",
      "proposals": [
        {
          "title": "Try GetGlyphPath on individual emoji glyph IDs",
          "description": "Use GetGlyphs to convert emoji text to glyph IDs, then GetGlyphPath on each glyph ID. For COLR v0 fonts, the component outline paths may still be accessible even if GetTextPath returns empty.",
          "category": "workaround",
          "codeSnippet": "var glyphs = fontTest.GetGlyphs(text);\nvar glyphPositions = fontTest.GetGlyphPositions(glyphs, new SKPoint(0, 0));\nusing var combinedPath = new SKPath();\nfor (int i = 0; i < glyphs.Length; i++) {\n    using var glyphPath = fontTest.GetGlyphPath(glyphs[i]);\n    if (glyphPath != null) {\n        var offset = SKMatrix.CreateTranslation(glyphPositions[i].X, glyphPositions[i].Y);\n        combinedPath.AddPath(glyphPath, in offset);\n    }\n}\n// Note: returns null for CBDT/CBLC bitmap emoji. Results vary for COLR emoji.",
          "confidence": 0.5,
          "effort": "cost/xs",
          "validated": "untested"
        },
        {
          "title": "Render emoji to bitmap surface instead of path",
          "description": "For color emoji processing, render the text to an SKSurface and work with the resulting bitmap rather than attempting path extraction.",
          "category": "alternative",
          "confidence": 0.8,
          "effort": "cost/s",
          "validated": "untested"
        },
        {
          "title": "Investigate COLR emoji path regression in Skia upgrade",
          "description": "Compare Skia's SkTextUtils::GetPath and SkFont::getPath COLR behavior between the Skia milestone used in SkiaSharp 2.88.9 vs 3.119.0 to confirm or refute the regression.",
          "category": "investigation",
          "confidence": 0.75,
          "effort": "cost/m",
          "validated": "untested"
        }
      ],
      "recommendedProposal": "Try GetGlyphPath on individual emoji glyph IDs",
      "recommendedReason": "Simple to test first. If it works for the reporter's Segoe UI Emoji COLR font, it provides an immediate workaround. If also empty, it confirms the limitation is fundamental to Skia color emoji path extraction, guiding the fix direction."
    }
  },
  "output": {
    "actionability": {
      "suggestedAction": "needs-investigation",
      "confidence": 0.82,
      "reason": "Regression claim from 2.88.9 to 3.119.0 with code reproduction steps. Root cause (color emoji font path handling in Skia) needs verification and the GetGlyphPath workaround needs confirmation.",
      "suggestedReproPlatform": "windows"
    },
    "actions": [
      {
        "type": "update-labels",
        "description": "Apply bug, SkiaSharp area, Windows platform, and compatibility tenet labels",
        "risk": "low",
        "confidence": 0.9,
        "labels": [
          "type/bug",
          "area/SkiaSharp",
          "os/Windows-Classic",
          "tenet/compatibility"
        ]
      },
      {
        "type": "add-comment",
        "description": "Post analysis of color emoji path limitation with workaround suggestions",
        "risk": "medium",
        "confidence": 0.75,
        "comment": "Thanks for the detailed report!\n\nThe empty path for emoji is likely related to how modern emoji fonts store glyph data. **Segoe UI Emoji** on Windows 11 uses **COLR** (color-layered vector) format rather than traditional outline paths. Skia's `GetTextPath` (via `SkTextUtils::GetPath`) extracts glyph outlines — for COLR or bitmap (CBDT/CBLC) emoji glyphs, there may be no conventional outline to extract, resulting in an empty path.\n\nWhile we investigate the reported regression from 2.88.9, please try these workarounds:\n\n**Option 1 — Use GetGlyphPath on individual glyph IDs:**\n```csharp\nvar glyphs = fontTest.GetGlyphs(text);\nvar glyphPositions = fontTest.GetGlyphPositions(glyphs, new SKPoint(0, 0));\nusing var combinedPath = new SKPath();\nfor (int i = 0; i < glyphs.Length; i++) {\n    using var glyphPath = fontTest.GetGlyphPath(glyphs[i]);\n    if (glyphPath != null) {\n        var offset = SKMatrix.CreateTranslation(glyphPositions[i].X, glyphPositions[i].Y);\n        combinedPath.AddPath(glyphPath, in offset);\n    }\n}\n```\n*Note: Results depend on the COLR format version used by the font. This may also return empty for pure bitmap emoji glyphs.*\n\n**Option 2 — Render to bitmap surface:**\nFor processing emoji appearance, render to a temporary `SKSurface` with `DrawText`/`DrawShapedText` and work with the rasterized bitmap output.\n\nCould you confirm: does `GetGlyphPath` on the individual glyph IDs from `fontTest.GetGlyphs(\"\\uD83D\\uDE0A\")` return a non-null path? That would help determine whether this is a `GetTextPath`-specific issue or a deeper Skia limitation with COLR emoji on Windows 11."
      }
    ]
  }
}
```

</details>
