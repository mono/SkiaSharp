# Issue Triage Report — #1996

| Field | Value |
|-------|-------|
| Repository | mono/SkiaSharp |
| Analyzed | 2026-05-02T23:25:00Z |
| Type | type/bug (0.72 (72%)) |
| Area | area/SkiaSharp (0.90 (90%)) |
| Suggested action | needs-info (0.80 (80%)) |

**Issue Summary:** Reporter claims that setting IsAntialias = false on SKPaint has no visible effect when drawing Chinese characters (simsun font) on Linux using the legacy DrawText overload.

**Analysis:** The reporter sets IsAntialias = false on SKPaint but observes that text rendering on Linux still appears anti-aliased. The SKPaint.IsAntialias setter calls sk_compatpaint_set_is_antialias (not sk_paint_set_antialias), which is the compat-layer API designed to synchronize anti-aliasing across both the paint and the font edging. On Linux, FreeType-based text rendering may also be influenced by SKFont.Edging settings. The obsolete DrawText(string, SKPoint, SKPaint) overload delegates to the new font-based API via paint.GetFont(), where SKFont.Edging defaults to Antialias. If sk_compatpaint_set_is_antialias does not correctly set the font edging to Alias, Linux text may still render anti-aliased.

**Recommendations:** **needs-info** — Issue lacks SkiaSharp version, OS version, and screenshots. A typo in the repro code (pain vs paint) suggests minimal testing. Need confirmation of the actual visual output before investigating the C API.

---

## Classification

| Field | Value |
|-------|-------|
| Type | type/bug |
| Area | area/SkiaSharp |
| Platforms | os/Linux |
| Backends | — |
| Tenets | tenet/reliability |
| Partner | — |

## Evidence

### Reproduction

1. Create SKPaint with IsAntialias = false, Typeface = SKTypeface.FromFamilyName("simsun", SKFontStyle.Normal), TextSize = 14
2. Create a 400x300 raster surface
3. Call DrawText with Chinese characters at SKPoint(10, 10)
4. Observe that output appears anti-aliased despite IsAntialias = false

**Environment:** Linux (no version or SkiaSharp version provided)

### Bug Signals

| Field | Value |
|-------|-------|
| Severity | low |
| Regression claimed | False |
| Error type | wrong-output |
| Error message | — |
| Repro quality | partial |
| Target frameworks | — |

## Analysis

### Technical Summary

The reporter sets IsAntialias = false on SKPaint but observes that text rendering on Linux still appears anti-aliased. The SKPaint.IsAntialias setter calls sk_compatpaint_set_is_antialias (not sk_paint_set_antialias), which is the compat-layer API designed to synchronize anti-aliasing across both the paint and the font edging. On Linux, FreeType-based text rendering may also be influenced by SKFont.Edging settings. The obsolete DrawText(string, SKPoint, SKPaint) overload delegates to the new font-based API via paint.GetFont(), where SKFont.Edging defaults to Antialias. If sk_compatpaint_set_is_antialias does not correctly set the font edging to Alias, Linux text may still render anti-aliased.

### Rationale

Classified as type/bug because the reporter claims a documented feature (IsAntialias=false) doesn't work as expected. The area is area/SkiaSharp because the issue is about the core drawing API (SKPaint + DrawText), not a view layer. Linux is the only stated platform. Severity is low because it's a cosmetic rendering quality issue with no crashes. The repro quality is partial: code is provided but there are no screenshots showing the actual vs expected output, no SkiaSharp version, and there's a typo in the code. suggestedAction is needs-info because key diagnostic information is missing.

### Key Signals

- "IsAntialias = false" — **issue body** (Reporter explicitly sets anti-aliasing off, expects aliased (non-smoothed) text output.)
- "DrawText("测试测试", new SKPoint(10, 10), pain)" — **issue body** (Code uses the obsolete SKPaint-based DrawText overload. Note: 'pain' appears to be a typo for 'paint', suggesting this is pseudocode rather than production code.)
- "Invalid under Linux" — **issue title** (Reporter claims this works on other platforms, implying Linux-specific behavior.)

### Code Investigation

| File | Lines | Relevance | Finding |
|------|-------|-----------|---------|
| `binding/SkiaSharp/SKPaint.cs` | 105-108 | direct | IsAntialias getter uses sk_paint_is_antialias but the setter uses sk_compatpaint_set_is_antialias — an asymmetric compat-layer call intended to also update font edging. If this function does not correctly set SKFont.Edging to Alias, text anti-aliasing won't be disabled. |
| `binding/SkiaSharp/SKCanvas.cs` | 631-637 | direct | The DrawText(string, SKPoint, SKPaint) overload used by the reporter is marked [Obsolete] and delegates to DrawText(text, p, paint.TextAlign, paint.GetFont(), paint). The GetFont() call creates an SKFont from paint properties. If IsAntialias synchronization via sk_compatpaint_set_is_antialias doesn't propagate to SKFont.Edging, the font will default to anti-aliased rendering. |
| `binding/SkiaSharp/SKFont.cs` | 65-68 | related | SKFont.Edging controls whether text uses Alias, Antialias, or SubpixelAntialias rendering. This is the authoritative control for text AA on the new API. If IsAntialias=false does not update Edging to Alias, text will remain anti-aliased. |

### Workarounds

- Use the new SKFont API directly: create an SKFont with Edging = SKFontEdging.Alias and use DrawText(text, x, y, font, paint)
- Set font.Edging = SKFontEdging.Alias explicitly on the font obtained from paint.GetFont() before drawing

### Next Questions

- What SkiaSharp version is in use?
- Can you share screenshots of the expected (aliased) vs actual (anti-aliased) output?
- Does the issue reproduce with the new SKFont-based DrawText overload: DrawText(text, x, y, font, paint) where font.Edging = SKFontEdging.Alias?
- Is the font 'simsun' available on the Linux system, or is a fallback font being used?

### Resolution Proposals

**Hypothesis:** On Linux, SKPaint.IsAntialias = false may not propagate through sk_compatpaint_set_is_antialias to set SKFont.Edging = Alias, causing the font to render anti-aliased text regardless.

1. **Use new SKFont-based API with explicit Edging** — workaround, confidence 0.85 (85%), cost/xs, validated=yes
   - Replace the obsolete DrawText overload with the new font-based API and explicitly set SKFont.Edging = SKFontEdging.Alias.
2. **Investigate sk_compatpaint_set_is_antialias C API** — investigation, confidence 0.70 (70%), cost/m, validated=untested
   - Verify that sk_compatpaint_set_is_antialias correctly sets font edging to Alias on Linux. Compare with Windows/macOS behavior.

**Recommended proposal:** Use new SKFont-based API with explicit Edging

**Why:** Immediate workaround the reporter can apply while the underlying behavior is investigated.

## Recommendations

### Actionability

| Field | Value |
|-------|-------|
| Suggested action | needs-info |
| Confidence | 0.80 (80%) |
| Reason | Issue lacks SkiaSharp version, OS version, and screenshots. A typo in the repro code (pain vs paint) suggests minimal testing. Need confirmation of the actual visual output before investigating the C API. |
| Suggested repro platform | linux |

### Missing Info

- SkiaSharp version
- Linux distro and version
- Screenshot or image showing actual output vs expected aliased output
- Confirmation whether the new SKFont-based API (with Edging=Alias) produces the expected result

### Automatable Actions

| Type | Risk | Confidence | Description | Details |
|------|------|------------|-------------|---------|
| update-labels | low | 0.90 (90%) | Apply bug, SkiaSharp core, Linux labels | labels=type/bug, area/SkiaSharp, os/Linux |
| add-comment | medium | 0.80 (80%) | Ask for version info, screenshots, and suggest workaround via new SKFont API | — |

**Comment draft for `add-comment`:**

```markdown
Thanks for the report! To help investigate, could you provide:

1. **SkiaSharp version** you're using
2. **Linux distro/version**
3. **Screenshots** showing the actual (anti-aliased) output vs. what you expect (aliased/pixelated text)

In the meantime, you can try this workaround using the newer font API which gives you explicit control over anti-aliasing:

```csharp
var font = new SKFont
{
    Typeface = SKTypeface.FromFamilyName("simsun", SKFontStyle.Normal),
    Size = 14,
    Edging = SKFontEdging.Alias  // explicitly disables anti-aliasing
};

var paint = new SKPaint { Color = SKColors.Black };
canvas.DrawText("测试测试", new SKPoint(10, 10), font, paint);
```

Note: The `DrawText(string, SKPoint, SKPaint)` overload you're using is marked as obsolete in recent versions. The new font-based API separates font properties from paint properties and gives you direct control over text edging.
```

<details>
<summary>Raw JSON</summary>

```json
{
  "meta": {
    "schemaVersion": "1.0",
    "number": 1996,
    "repo": "mono/SkiaSharp",
    "analyzedAt": "2026-05-02T23:25:00Z"
  },
  "summary": "Reporter claims that setting IsAntialias = false on SKPaint has no visible effect when drawing Chinese characters (simsun font) on Linux using the legacy DrawText overload.",
  "classification": {
    "type": {
      "value": "type/bug",
      "confidence": 0.72
    },
    "area": {
      "value": "area/SkiaSharp",
      "confidence": 0.9
    },
    "platforms": [
      "os/Linux"
    ],
    "tenets": [
      "tenet/reliability"
    ]
  },
  "evidence": {
    "bugSignals": {
      "severity": "low",
      "regressionClaimed": false,
      "errorType": "wrong-output",
      "reproQuality": "partial",
      "targetFrameworks": []
    },
    "reproEvidence": {
      "stepsToReproduce": [
        "Create SKPaint with IsAntialias = false, Typeface = SKTypeface.FromFamilyName(\"simsun\", SKFontStyle.Normal), TextSize = 14",
        "Create a 400x300 raster surface",
        "Call DrawText with Chinese characters at SKPoint(10, 10)",
        "Observe that output appears anti-aliased despite IsAntialias = false"
      ],
      "environmentDetails": "Linux (no version or SkiaSharp version provided)",
      "repoLinks": []
    }
  },
  "analysis": {
    "summary": "The reporter sets IsAntialias = false on SKPaint but observes that text rendering on Linux still appears anti-aliased. The SKPaint.IsAntialias setter calls sk_compatpaint_set_is_antialias (not sk_paint_set_antialias), which is the compat-layer API designed to synchronize anti-aliasing across both the paint and the font edging. On Linux, FreeType-based text rendering may also be influenced by SKFont.Edging settings. The obsolete DrawText(string, SKPoint, SKPaint) overload delegates to the new font-based API via paint.GetFont(), where SKFont.Edging defaults to Antialias. If sk_compatpaint_set_is_antialias does not correctly set the font edging to Alias, Linux text may still render anti-aliased.",
    "codeInvestigation": [
      {
        "file": "binding/SkiaSharp/SKPaint.cs",
        "lines": "105-108",
        "finding": "IsAntialias getter uses sk_paint_is_antialias but the setter uses sk_compatpaint_set_is_antialias — an asymmetric compat-layer call intended to also update font edging. If this function does not correctly set SKFont.Edging to Alias, text anti-aliasing won't be disabled.",
        "relevance": "direct"
      },
      {
        "file": "binding/SkiaSharp/SKCanvas.cs",
        "lines": "631-637",
        "finding": "The DrawText(string, SKPoint, SKPaint) overload used by the reporter is marked [Obsolete] and delegates to DrawText(text, p, paint.TextAlign, paint.GetFont(), paint). The GetFont() call creates an SKFont from paint properties. If IsAntialias synchronization via sk_compatpaint_set_is_antialias doesn't propagate to SKFont.Edging, the font will default to anti-aliased rendering.",
        "relevance": "direct"
      },
      {
        "file": "binding/SkiaSharp/SKFont.cs",
        "lines": "65-68",
        "finding": "SKFont.Edging controls whether text uses Alias, Antialias, or SubpixelAntialias rendering. This is the authoritative control for text AA on the new API. If IsAntialias=false does not update Edging to Alias, text will remain anti-aliased.",
        "relevance": "related"
      }
    ],
    "keySignals": [
      {
        "text": "IsAntialias = false",
        "source": "issue body",
        "interpretation": "Reporter explicitly sets anti-aliasing off, expects aliased (non-smoothed) text output."
      },
      {
        "text": "DrawText(\"测试测试\", new SKPoint(10, 10), pain)",
        "source": "issue body",
        "interpretation": "Code uses the obsolete SKPaint-based DrawText overload. Note: 'pain' appears to be a typo for 'paint', suggesting this is pseudocode rather than production code."
      },
      {
        "text": "Invalid under Linux",
        "source": "issue title",
        "interpretation": "Reporter claims this works on other platforms, implying Linux-specific behavior."
      }
    ],
    "rationale": "Classified as type/bug because the reporter claims a documented feature (IsAntialias=false) doesn't work as expected. The area is area/SkiaSharp because the issue is about the core drawing API (SKPaint + DrawText), not a view layer. Linux is the only stated platform. Severity is low because it's a cosmetic rendering quality issue with no crashes. The repro quality is partial: code is provided but there are no screenshots showing the actual vs expected output, no SkiaSharp version, and there's a typo in the code. suggestedAction is needs-info because key diagnostic information is missing.",
    "nextQuestions": [
      "What SkiaSharp version is in use?",
      "Can you share screenshots of the expected (aliased) vs actual (anti-aliased) output?",
      "Does the issue reproduce with the new SKFont-based DrawText overload: DrawText(text, x, y, font, paint) where font.Edging = SKFontEdging.Alias?",
      "Is the font 'simsun' available on the Linux system, or is a fallback font being used?"
    ],
    "workarounds": [
      "Use the new SKFont API directly: create an SKFont with Edging = SKFontEdging.Alias and use DrawText(text, x, y, font, paint)",
      "Set font.Edging = SKFontEdging.Alias explicitly on the font obtained from paint.GetFont() before drawing"
    ],
    "resolution": {
      "hypothesis": "On Linux, SKPaint.IsAntialias = false may not propagate through sk_compatpaint_set_is_antialias to set SKFont.Edging = Alias, causing the font to render anti-aliased text regardless.",
      "proposals": [
        {
          "title": "Use new SKFont-based API with explicit Edging",
          "description": "Replace the obsolete DrawText overload with the new font-based API and explicitly set SKFont.Edging = SKFontEdging.Alias.",
          "category": "workaround",
          "confidence": 0.85,
          "effort": "cost/xs",
          "validated": "yes"
        },
        {
          "title": "Investigate sk_compatpaint_set_is_antialias C API",
          "description": "Verify that sk_compatpaint_set_is_antialias correctly sets font edging to Alias on Linux. Compare with Windows/macOS behavior.",
          "category": "investigation",
          "confidence": 0.7,
          "effort": "cost/m",
          "validated": "untested"
        }
      ],
      "recommendedProposal": "Use new SKFont-based API with explicit Edging",
      "recommendedReason": "Immediate workaround the reporter can apply while the underlying behavior is investigated."
    }
  },
  "output": {
    "actionability": {
      "suggestedAction": "needs-info",
      "confidence": 0.8,
      "reason": "Issue lacks SkiaSharp version, OS version, and screenshots. A typo in the repro code (pain vs paint) suggests minimal testing. Need confirmation of the actual visual output before investigating the C API.",
      "suggestedReproPlatform": "linux"
    },
    "missingInfo": [
      "SkiaSharp version",
      "Linux distro and version",
      "Screenshot or image showing actual output vs expected aliased output",
      "Confirmation whether the new SKFont-based API (with Edging=Alias) produces the expected result"
    ],
    "actions": [
      {
        "type": "update-labels",
        "description": "Apply bug, SkiaSharp core, Linux labels",
        "risk": "low",
        "confidence": 0.9,
        "labels": [
          "type/bug",
          "area/SkiaSharp",
          "os/Linux"
        ]
      },
      {
        "type": "add-comment",
        "description": "Ask for version info, screenshots, and suggest workaround via new SKFont API",
        "risk": "medium",
        "confidence": 0.8,
        "comment": "Thanks for the report! To help investigate, could you provide:\n\n1. **SkiaSharp version** you're using\n2. **Linux distro/version**\n3. **Screenshots** showing the actual (anti-aliased) output vs. what you expect (aliased/pixelated text)\n\nIn the meantime, you can try this workaround using the newer font API which gives you explicit control over anti-aliasing:\n\n```csharp\nvar font = new SKFont\n{\n    Typeface = SKTypeface.FromFamilyName(\"simsun\", SKFontStyle.Normal),\n    Size = 14,\n    Edging = SKFontEdging.Alias  // explicitly disables anti-aliasing\n};\n\nvar paint = new SKPaint { Color = SKColors.Black };\ncanvas.DrawText(\"测试测试\", new SKPoint(10, 10), font, paint);\n```\n\nNote: The `DrawText(string, SKPoint, SKPaint)` overload you're using is marked as obsolete in recent versions. The new font-based API separates font properties from paint properties and gives you direct control over text edging."
      }
    ]
  }
}
```

</details>
