# Issue Triage Report — #2983

| Field | Value |
|-------|-------|
| Repository | mono/SkiaSharp |
| Analyzed | 2026-04-26T10:30:00Z |
| Type | type/bug (0.90 (90%)) |
| Area | area/SkiaSharp (0.88 (88%)) |
| Suggested action | needs-investigation (0.82 (82%)) |

**Issue Summary:** Wingdings Symbol-charset glyphs (\u00FC, \u0071, \u00D8) render as empty boxes on Ubuntu Linux while working correctly on Windows, due to Wingdings not being installed by default on Linux and/or Skia not applying the Windows-specific Symbol charset 0xF000 offset remapping that GDI/DirectWrite provides.

**Analysis:** Wingdings is a Windows-proprietary 'Symbol' charset font not installed by default on Linux. On Windows, GDI/DirectWrite automatically remaps character codes 0x20–0xFF to Unicode Private Use Area codepoints 0xF020–0xF0FF when rendering Symbol fonts, so \u00FC maps to the Wingdings glyph at 0xF0FC. Skia on Linux does not apply this offset. When the font is also missing from the system, SKTypeface.FromFamilyName silently falls back to the default typeface. The second commenter's confirmation that even FromStream fails points to the Symbol charset remapping as the primary root cause.

**Recommendations:** **needs-investigation** — Complete repro provided with screenshots. Second commenter confirms the issue in a newer version even with explicit font loading. Root cause hypothesis (Symbol charset PUA offset) needs reproduction to confirm before closing or fixing.

---

## Classification

| Field | Value |
|-------|-------|
| Type | type/bug |
| Area | area/SkiaSharp |
| Platforms | os/Linux |
| Backends | — |
| Tenets | tenet/compatibility |
| Partner | — |
| Current labels | type/bug |

## Evidence

### Reproduction

1. Create a .NET 8.0 console app on Ubuntu Linux 22.04
2. Install SkiaSharp v2.88.8
3. Use SKTypeface.FromFamilyName("Wingdings") and draw characters \u00FC, \u0071, \u00D8
4. Save as JPEG
5. Observe empty boxes instead of Wingdings symbols

**Environment:** Ubuntu Linux 22.04 VM, .NET 8.0, SkiaSharp 2.88.8

**Repository links:**
- https://github.com/mono/SkiaSharp/issues/838 — Related: text measured differently on Linux vs Windows with Wingdings/Arial fonts

### Bug Signals

| Field | Value |
|-------|-------|
| Severity | medium |
| Regression claimed | False |
| Error type | wrong-output |
| Error message | Unicode characters rendered as empty rectangle boxes (tofu) on Ubuntu Linux |
| Repro quality | complete |
| Target frameworks | net8.0 |

### Version Analysis

| Field | Value |
|-------|-------|
| Mentioned versions | 2.88.8, 3.116.1 |
| Worked in | — |
| Broke in | — |
| Current relevance | likely |
| Relevance reason | Second commenter confirmed same issue in 3.116.1 even when loading font via FromStream, indicating this is not a version-specific regression but an ongoing cross-platform compatibility issue. |

## Analysis

### Technical Summary

Wingdings is a Windows-proprietary 'Symbol' charset font not installed by default on Linux. On Windows, GDI/DirectWrite automatically remaps character codes 0x20–0xFF to Unicode Private Use Area codepoints 0xF020–0xF0FF when rendering Symbol fonts, so \u00FC maps to the Wingdings glyph at 0xF0FC. Skia on Linux does not apply this offset. When the font is also missing from the system, SKTypeface.FromFamilyName silently falls back to the default typeface. The second commenter's confirmation that even FromStream fails points to the Symbol charset remapping as the primary root cause.

### Rationale

Classified as type/bug because the same SkiaSharp API produces different visual output on Windows vs Linux for a font that can be legitimately loaded on both platforms. The platform-specific behavior is in Symbol charset handling inherited from Windows GDI conventions, not replicated by Skia cross-platform. Area is area/SkiaSharp because the rendering path and font resolution go through the core SkiaSharp bindings (SKTypeface, SKFontManager). tenet/compatibility applies because Windows-created applications relying on Wingdings rendering will break when deployed to Linux.

### Key Signals

- "Typeface = SKTypeface.FromFamilyName("Wingdings")" — **issue body** (Relies on system font lookup. Wingdings is a Windows-only font not present on Ubuntu by default. On Linux this silently falls back to the default typeface.)
- "I made sure to load the font via SKTypeface.FromStream(fontStream). This works great on Windows but only produces empty boxes on Ubuntu." — **comment by jbartlau** (Loading Wingdings directly via FromStream still fails on Ubuntu in SkiaSharp 3.116.1. This rules out font-not-found as the sole cause and points to Skia not applying the Symbol charset 0xF000 codepoint offset that Windows does automatically for Symbol fonts.)
- "If you did it in a linux vm, the most likely cause is that you have not installed any emoji capable fonts." — **comment by HinTak** (Partial diagnosis — correctly identifies the missing-font case, but does not address why loading via FromStream also fails (Symbol charset issue).)

### Code Investigation

| File | Lines | Relevance | Finding |
|------|-------|-----------|---------|
| `binding/SkiaSharp/SKTypeface.cs` | 79-80 | direct | SKTypeface.FromFamilyName(familyName, style) calls SKFontManager.Default.MatchFamily(familyName, style) ?? Default. If Wingdings is not installed on Linux, MatchFamily returns null and the call silently falls back to SKTypeface.Default (system sans-serif). No warning or null return to the caller. |
| `binding/SkiaSharp/SKFontManager.cs` | 77-87 | direct | MatchFamily(familyName, style) delegates to native sk_fontmgr_match_family_style. There is no Symbol charset remapping (0x0020-0x00FF -> 0xF020-0xF0FF) in the SkiaSharp managed layer. This remapping is a Windows GDI/DirectWrite convention not replicated in Skia on Linux. |

### Workarounds

- Install Wingdings on Linux: copy the font file from a Windows machine to ~/.local/share/fonts/ and run fc-cache -f
- Load the font explicitly via SKTypeface.FromFile("/path/to/Wingdings.ttf") instead of FromFamilyName, ensuring the font is distributed with the application
- Use the Unicode Private Use Area codepoints that the Wingdings font actually stores glyphs at: add 0xF000 to each character code (e.g., \u00FC becomes (char)(0xF000 + 0x00FC) = \uF0FC, \u0071 becomes \uF071, \u00D8 becomes \uF0D8). This is the actual cross-platform approach for Symbol charset fonts with Skia.

### Next Questions

- Does loading Wingdings via FromFile/FromStream with PUA codepoints (0xF000 offset) render correctly on Ubuntu?
- Does Skia upstream handle Symbol charset cmap remapping on any platform?
- Is there a way to detect whether a loaded typeface uses Symbol charset so SkiaSharp can document the required codepoint adjustment?

### Resolution Proposals

**Hypothesis:** Two issues compound: (1) Wingdings not installed by default on Linux causing silent fallback; (2) Skia cross-platform does not apply Windows GDI's automatic Symbol charset 0xF000 offset remapping, so the same character codes that work on Windows do not find glyphs in the font's cmap on Linux.

1. **Use PUA codepoints for Symbol charset fonts** — workaround, confidence 0.80 (80%), cost/xs, validated=untested
   - For Wingdings and other Symbol charset fonts, use Unicode Private Use Area codepoints by adding 0xF000 to each character code. This accesses the actual glyph slots in the font's cmap table and works cross-platform with Skia.

```csharp
// For Wingdings (Symbol charset), add 0xF000 to get PUA codepoints
string[] bullets = {
    ((char)(0xF000 + 0x00FC)).ToString(),  // was \u00FC
    ((char)(0xF000 + 0x0071)).ToString(),  // was \u0071
    ((char)(0xF000 + 0x00D8)).ToString()   // was \u00D8
};
// Load font explicitly (don't rely on system font lookup)
var typeface = SKTypeface.FromFile("/path/to/Wingdings.ttf");
var bulletPaint = new SKPaint { Typeface = typeface, TextSize = 30, IsAntialias = true, Color = SKColors.Black };
```
2. **Document Symbol charset font cross-platform requirements** — fix, confidence 0.85 (85%), cost/s, validated=untested
   - Add documentation explaining that Windows Symbol charset fonts (Wingdings, Webdings, Symbol) require loading via FromFile/FromStream and using PUA codepoints (0xF000 offset) for cross-platform compatibility with Skia, since Windows GDI's automatic Symbol charset remapping is not present on Linux/macOS.

**Recommended proposal:** Use PUA codepoints for Symbol charset fonts

**Why:** Addresses both causes: explicitly loading the font file avoids silent fallback, and using PUA codepoints (0xF000 offset) aligns with how Skia's cmap lookup works cross-platform for Symbol charset fonts.

## Recommendations

### Actionability

| Field | Value |
|-------|-------|
| Suggested action | needs-investigation |
| Confidence | 0.82 (82%) |
| Reason | Complete repro provided with screenshots. Second commenter confirms the issue in a newer version even with explicit font loading. Root cause hypothesis (Symbol charset PUA offset) needs reproduction to confirm before closing or fixing. |
| Suggested repro platform | linux |

### Automatable Actions

| Type | Risk | Confidence | Description | Details |
|------|------|------------|-------------|---------|
| update-labels | low | 0.92 (92%) | Apply bug, area, platform, and tenet labels | labels=type/bug, area/SkiaSharp, os/Linux, tenet/compatibility |
| add-comment | medium | 0.80 (80%) | Explain the two root causes and provide PUA codepoint workaround | — |

**Comment draft for `add-comment`:**

```markdown
Thanks for the report and the screenshots.

There are two compounding issues here:

1. **Wingdings is not installed on Ubuntu by default.** When `SKTypeface.FromFamilyName("Wingdings")` can't find the font, it silently falls back to the system default typeface, which doesn't contain Wingdings glyphs.

2. **Symbol charset fonts require PUA codepoints on Linux.** Wingdings uses a Windows "Symbol" charset encoding where glyphs are stored in the font at Unicode Private Use Area codepoints (U+F000–U+F0FF). On Windows, GDI/DirectWrite automatically adds the 0xF000 offset when rendering Symbol fonts, so `\u00FC` is automatically looked up as `\uF0FC`. Skia on Linux does not apply this offset — confirmed by the second commenter who still sees empty boxes even after loading the font via `FromStream`.

**Workaround:** Load the font file explicitly and use PUA codepoints:

```csharp
// Load Wingdings from file (distribute the .ttf with your app)
var typeface = SKTypeface.FromFile("/path/to/Wingdings.ttf");

// Use PUA codepoints (add 0xF000 to the character code)
string[] bullets = {
    ((char)(0xF000 + 0x00FC)).ToString(),  // \u00FC → \uF0FC
    ((char)(0xF000 + 0x0071)).ToString(),  // \u0071 → \uF071
    ((char)(0xF000 + 0x00D8)).ToString()   // \u00D8 → \uF0D8
};
```

Could you try this workaround and confirm whether it resolves the issue on your Ubuntu VM? That will help confirm the root cause before we investigate whether Skia should handle Symbol charset remapping cross-platform.
```

<details>
<summary>Raw JSON</summary>

```json
{
  "meta": {
    "schemaVersion": "1.0",
    "number": 2983,
    "repo": "mono/SkiaSharp",
    "analyzedAt": "2026-04-26T10:30:00Z",
    "currentLabels": [
      "type/bug"
    ]
  },
  "summary": "Wingdings Symbol-charset glyphs (\\u00FC, \\u0071, \\u00D8) render as empty boxes on Ubuntu Linux while working correctly on Windows, due to Wingdings not being installed by default on Linux and/or Skia not applying the Windows-specific Symbol charset 0xF000 offset remapping that GDI/DirectWrite provides.",
  "classification": {
    "type": {
      "value": "type/bug",
      "confidence": 0.9
    },
    "area": {
      "value": "area/SkiaSharp",
      "confidence": 0.88
    },
    "platforms": [
      "os/Linux"
    ],
    "tenets": [
      "tenet/compatibility"
    ]
  },
  "evidence": {
    "bugSignals": {
      "severity": "medium",
      "regressionClaimed": false,
      "errorType": "wrong-output",
      "errorMessage": "Unicode characters rendered as empty rectangle boxes (tofu) on Ubuntu Linux",
      "reproQuality": "complete",
      "targetFrameworks": [
        "net8.0"
      ]
    },
    "reproEvidence": {
      "stepsToReproduce": [
        "Create a .NET 8.0 console app on Ubuntu Linux 22.04",
        "Install SkiaSharp v2.88.8",
        "Use SKTypeface.FromFamilyName(\"Wingdings\") and draw characters \\u00FC, \\u0071, \\u00D8",
        "Save as JPEG",
        "Observe empty boxes instead of Wingdings symbols"
      ],
      "environmentDetails": "Ubuntu Linux 22.04 VM, .NET 8.0, SkiaSharp 2.88.8",
      "repoLinks": [
        {
          "url": "https://github.com/mono/SkiaSharp/issues/838",
          "description": "Related: text measured differently on Linux vs Windows with Wingdings/Arial fonts"
        }
      ]
    },
    "versionAnalysis": {
      "mentionedVersions": [
        "2.88.8",
        "3.116.1"
      ],
      "currentRelevance": "likely",
      "relevanceReason": "Second commenter confirmed same issue in 3.116.1 even when loading font via FromStream, indicating this is not a version-specific regression but an ongoing cross-platform compatibility issue."
    }
  },
  "analysis": {
    "summary": "Wingdings is a Windows-proprietary 'Symbol' charset font not installed by default on Linux. On Windows, GDI/DirectWrite automatically remaps character codes 0x20–0xFF to Unicode Private Use Area codepoints 0xF020–0xF0FF when rendering Symbol fonts, so \\u00FC maps to the Wingdings glyph at 0xF0FC. Skia on Linux does not apply this offset. When the font is also missing from the system, SKTypeface.FromFamilyName silently falls back to the default typeface. The second commenter's confirmation that even FromStream fails points to the Symbol charset remapping as the primary root cause.",
    "rationale": "Classified as type/bug because the same SkiaSharp API produces different visual output on Windows vs Linux for a font that can be legitimately loaded on both platforms. The platform-specific behavior is in Symbol charset handling inherited from Windows GDI conventions, not replicated by Skia cross-platform. Area is area/SkiaSharp because the rendering path and font resolution go through the core SkiaSharp bindings (SKTypeface, SKFontManager). tenet/compatibility applies because Windows-created applications relying on Wingdings rendering will break when deployed to Linux.",
    "codeInvestigation": [
      {
        "file": "binding/SkiaSharp/SKTypeface.cs",
        "lines": "79-80",
        "finding": "SKTypeface.FromFamilyName(familyName, style) calls SKFontManager.Default.MatchFamily(familyName, style) ?? Default. If Wingdings is not installed on Linux, MatchFamily returns null and the call silently falls back to SKTypeface.Default (system sans-serif). No warning or null return to the caller.",
        "relevance": "direct"
      },
      {
        "file": "binding/SkiaSharp/SKFontManager.cs",
        "lines": "77-87",
        "finding": "MatchFamily(familyName, style) delegates to native sk_fontmgr_match_family_style. There is no Symbol charset remapping (0x0020-0x00FF -> 0xF020-0xF0FF) in the SkiaSharp managed layer. This remapping is a Windows GDI/DirectWrite convention not replicated in Skia on Linux.",
        "relevance": "direct"
      }
    ],
    "keySignals": [
      {
        "text": "Typeface = SKTypeface.FromFamilyName(\"Wingdings\")",
        "source": "issue body",
        "interpretation": "Relies on system font lookup. Wingdings is a Windows-only font not present on Ubuntu by default. On Linux this silently falls back to the default typeface."
      },
      {
        "text": "I made sure to load the font via SKTypeface.FromStream(fontStream). This works great on Windows but only produces empty boxes on Ubuntu.",
        "source": "comment by jbartlau",
        "interpretation": "Loading Wingdings directly via FromStream still fails on Ubuntu in SkiaSharp 3.116.1. This rules out font-not-found as the sole cause and points to Skia not applying the Symbol charset 0xF000 codepoint offset that Windows does automatically for Symbol fonts."
      },
      {
        "text": "If you did it in a linux vm, the most likely cause is that you have not installed any emoji capable fonts.",
        "source": "comment by HinTak",
        "interpretation": "Partial diagnosis — correctly identifies the missing-font case, but does not address why loading via FromStream also fails (Symbol charset issue)."
      }
    ],
    "workarounds": [
      "Install Wingdings on Linux: copy the font file from a Windows machine to ~/.local/share/fonts/ and run fc-cache -f",
      "Load the font explicitly via SKTypeface.FromFile(\"/path/to/Wingdings.ttf\") instead of FromFamilyName, ensuring the font is distributed with the application",
      "Use the Unicode Private Use Area codepoints that the Wingdings font actually stores glyphs at: add 0xF000 to each character code (e.g., \\u00FC becomes (char)(0xF000 + 0x00FC) = \\uF0FC, \\u0071 becomes \\uF071, \\u00D8 becomes \\uF0D8). This is the actual cross-platform approach for Symbol charset fonts with Skia."
    ],
    "nextQuestions": [
      "Does loading Wingdings via FromFile/FromStream with PUA codepoints (0xF000 offset) render correctly on Ubuntu?",
      "Does Skia upstream handle Symbol charset cmap remapping on any platform?",
      "Is there a way to detect whether a loaded typeface uses Symbol charset so SkiaSharp can document the required codepoint adjustment?"
    ],
    "resolution": {
      "hypothesis": "Two issues compound: (1) Wingdings not installed by default on Linux causing silent fallback; (2) Skia cross-platform does not apply Windows GDI's automatic Symbol charset 0xF000 offset remapping, so the same character codes that work on Windows do not find glyphs in the font's cmap on Linux.",
      "proposals": [
        {
          "title": "Use PUA codepoints for Symbol charset fonts",
          "description": "For Wingdings and other Symbol charset fonts, use Unicode Private Use Area codepoints by adding 0xF000 to each character code. This accesses the actual glyph slots in the font's cmap table and works cross-platform with Skia.",
          "category": "workaround",
          "codeSnippet": "// For Wingdings (Symbol charset), add 0xF000 to get PUA codepoints\nstring[] bullets = {\n    ((char)(0xF000 + 0x00FC)).ToString(),  // was \\u00FC\n    ((char)(0xF000 + 0x0071)).ToString(),  // was \\u0071\n    ((char)(0xF000 + 0x00D8)).ToString()   // was \\u00D8\n};\n// Load font explicitly (don't rely on system font lookup)\nvar typeface = SKTypeface.FromFile(\"/path/to/Wingdings.ttf\");\nvar bulletPaint = new SKPaint { Typeface = typeface, TextSize = 30, IsAntialias = true, Color = SKColors.Black };",
          "confidence": 0.8,
          "effort": "cost/xs",
          "validated": "untested"
        },
        {
          "title": "Document Symbol charset font cross-platform requirements",
          "description": "Add documentation explaining that Windows Symbol charset fonts (Wingdings, Webdings, Symbol) require loading via FromFile/FromStream and using PUA codepoints (0xF000 offset) for cross-platform compatibility with Skia, since Windows GDI's automatic Symbol charset remapping is not present on Linux/macOS.",
          "category": "fix",
          "confidence": 0.85,
          "effort": "cost/s",
          "validated": "untested"
        }
      ],
      "recommendedProposal": "Use PUA codepoints for Symbol charset fonts",
      "recommendedReason": "Addresses both causes: explicitly loading the font file avoids silent fallback, and using PUA codepoints (0xF000 offset) aligns with how Skia's cmap lookup works cross-platform for Symbol charset fonts."
    }
  },
  "output": {
    "actionability": {
      "suggestedAction": "needs-investigation",
      "confidence": 0.82,
      "reason": "Complete repro provided with screenshots. Second commenter confirms the issue in a newer version even with explicit font loading. Root cause hypothesis (Symbol charset PUA offset) needs reproduction to confirm before closing or fixing.",
      "suggestedReproPlatform": "linux"
    },
    "actions": [
      {
        "type": "update-labels",
        "description": "Apply bug, area, platform, and tenet labels",
        "risk": "low",
        "confidence": 0.92,
        "labels": [
          "type/bug",
          "area/SkiaSharp",
          "os/Linux",
          "tenet/compatibility"
        ]
      },
      {
        "type": "add-comment",
        "description": "Explain the two root causes and provide PUA codepoint workaround",
        "risk": "medium",
        "confidence": 0.8,
        "comment": "Thanks for the report and the screenshots.\n\nThere are two compounding issues here:\n\n1. **Wingdings is not installed on Ubuntu by default.** When `SKTypeface.FromFamilyName(\"Wingdings\")` can't find the font, it silently falls back to the system default typeface, which doesn't contain Wingdings glyphs.\n\n2. **Symbol charset fonts require PUA codepoints on Linux.** Wingdings uses a Windows \"Symbol\" charset encoding where glyphs are stored in the font at Unicode Private Use Area codepoints (U+F000–U+F0FF). On Windows, GDI/DirectWrite automatically adds the 0xF000 offset when rendering Symbol fonts, so `\\u00FC` is automatically looked up as `\\uF0FC`. Skia on Linux does not apply this offset — confirmed by the second commenter who still sees empty boxes even after loading the font via `FromStream`.\n\n**Workaround:** Load the font file explicitly and use PUA codepoints:\n\n```csharp\n// Load Wingdings from file (distribute the .ttf with your app)\nvar typeface = SKTypeface.FromFile(\"/path/to/Wingdings.ttf\");\n\n// Use PUA codepoints (add 0xF000 to the character code)\nstring[] bullets = {\n    ((char)(0xF000 + 0x00FC)).ToString(),  // \\u00FC → \\uF0FC\n    ((char)(0xF000 + 0x0071)).ToString(),  // \\u0071 → \\uF071\n    ((char)(0xF000 + 0x00D8)).ToString()   // \\u00D8 → \\uF0D8\n};\n```\n\nCould you try this workaround and confirm whether it resolves the issue on your Ubuntu VM? That will help confirm the root cause before we investigate whether Skia should handle Symbol charset remapping cross-platform."
      }
    ]
  }
}
```

</details>
