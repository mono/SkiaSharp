# Issue Triage Report — #2471

| Field | Value |
|-------|-------|
| Repository | mono/SkiaSharp |
| Analyzed | 2026-04-26T17:56:00Z |
| Type | type/question (0.95 (95%)) |
| Area | area/SkiaSharp (0.90 (90%)) |
| Suggested action | close-as-not-a-bug (0.88 (88%)) |

**Issue Summary:** User asks how to get SKTypeface.FromFamilyName to return a suitable CJK (Japanese, Korean, Chinese) font on Android and iOS, since the Mapsui library uses this API internally and CJK text renders as squares.

**Analysis:** User cannot render CJK text because the Mapsui library uses SKTypeface.FromFamilyName internally with a font family name that has no CJK glyphs. The correct SkiaSharp API for character-based font matching is SKFontManager.MatchCharacter(). The reporter self-answered with a working workaround using MatchCharacter to obtain the correct family name per language/locale.

**Recommendations:** **close-as-not-a-bug** — This is a how-to question with a known answer. The SkiaSharp API works as designed; the correct API for character-based font discovery is SKFontManager.MatchCharacter(). The reporter self-answered and the question is fully resolved.

---

## Classification

| Field | Value |
|-------|-------|
| Type | type/question |
| Area | area/SkiaSharp |
| Platforms | os/Android, os/iOS |
| Backends | — |
| Tenets | — |
| Partner | — |

## Evidence

### Reproduction

1. Use Mapsui library which calls SKTypeface.FromFamilyName internally (LabelRenderer.cs#L277)
2. Display text containing Japanese, Korean, or Chinese characters
3. Observe squares (tofu) rendered instead of the actual characters

**Environment:** Android + iOS, Xamarin.Forms

**Repository links:**
- https://github.com/Mapsui/Mapsui/blob/5008d3ab8b0453c27cb487fe6ad3fac87435abbe/Mapsui.Rendering.Skia/LabelRenderer.cs#L277 — Third-party library call site using SKTypeface.FromFamilyName

## Analysis

### Technical Summary

User cannot render CJK text because the Mapsui library uses SKTypeface.FromFamilyName internally with a font family name that has no CJK glyphs. The correct SkiaSharp API for character-based font matching is SKFontManager.MatchCharacter(). The reporter self-answered with a working workaround using MatchCharacter to obtain the correct family name per language/locale.

### Rationale

This is a usage question with a clear answer. FromFamilyName requires knowing the exact family name; for cross-platform CJK support where family names differ by OS, MatchCharacter is the designed solution. The reporter already found and posted the workaround. The behavior of FromFamilyName is by-design.

### Key Signals

- "if the text to display is japanese, korean or chinese, it just prints squares" — **issue body** (The default typeface returned by FromFamilyName lacks CJK glyphs — not a bug in SkiaSharp, but a limitation of passing a non-CJK family name.)
- "SKFontManager.Default.MatchCharacter('あ').FamilyName" — **comment #1574110595 by reporter** (Reporter self-discovered the correct workaround using MatchCharacter to find a system font for a representative CJK character.)
- "can you explicitly name a system font family known to be available on the corresponding platform? E.g. 'Noto Sans CJK'?" — **comment #2224232143 by HinTak** (Alternative suggestion: use platform-specific CJK font family names directly if known.)

### Code Investigation

| File | Lines | Relevance | Finding |
|------|-------|-----------|---------|
| `binding/SkiaSharp/SKTypeface.cs` | 79-80 | direct | FromFamilyName(familyName, SKFontStyle) delegates to SKFontManager.Default.MatchFamily(familyName, style) ?? Default. If the named family has no CJK glyphs, MatchFamily either returns null (falls back to Default) or returns a Latin-only typeface, causing squares for CJK codepoints. |
| `binding/SkiaSharp/SKFontManager.cs` | 131-133 | direct | MatchCharacter(char character) calls sk_fontmgr_match_family_style_character, which searches installed system fonts for a typeface containing that Unicode codepoint — the correct API for finding a CJK-capable font without knowing the platform-specific family name. |

### Workarounds

- Use SKFontManager.Default.MatchCharacter(char) with a representative character from the target language to find the correct system typeface — e.g., MatchCharacter('\u3042') for Japanese, MatchCharacter('\ub9e4') for Korean, MatchCharacter('\u5b9e') for Simplified Chinese.
- Use SKTypeface.FromFile() or SKTypeface.FromData() to load a bundled CJK font (e.g., Noto Sans CJK) directly by file path, bypassing family-name lookup entirely.
- On platforms where the family name is known and stable, pass the platform-specific name directly to FromFamilyName (e.g., 'Noto Sans CJK JP' on Android, 'Hiragino Sans' on iOS).

### Resolution Proposals

**Hypothesis:** The reporter needs a way to pass CJK-capable typefaces into a library that calls FromFamilyName internally. The cleanest solution is to use MatchCharacter to obtain a family name that works on the current platform.

1. **Use MatchCharacter to resolve CJK family names** — workaround, confidence 0.90 (90%), cost/xs, validated=yes
   - Use SKFontManager.Default.MatchCharacter() with a representative codepoint for each language to discover the platform font family name. Null-check the result before calling FamilyName.

```csharp
string GetCjkFontFamily(char representativeChar)
{
    var typeface = SKFontManager.Default.MatchCharacter(representativeChar);
    return typeface?.FamilyName;
}

// Usage:
string jaFamily = GetCjkFontFamily('\u3042'); // hiragana 'a'
string koFamily = GetCjkFontFamily('\ub9e4'); // Korean character
string zhCnFamily = GetCjkFontFamily('\u5b9e'); // Simplified Chinese
string zhTwFamily = GetCjkFontFamily('\u5be6'); // Traditional Chinese
```
2. **Bundle and load a CJK font directly** — alternative, confidence 0.85 (85%), cost/s, validated=yes
   - Include a CJK-capable font (e.g., Noto Sans CJK) as an embedded resource and load it with SKTypeface.FromData(). Use FamilyName from the loaded typeface to pass to any API that requires a family name string.

```csharp
// Load bundled font and get its family name
using var stream = Assembly.GetExecutingAssembly()
    .GetManifestResourceStream("MyApp.Fonts.NotoSansCJK-Regular.ttf");
using var data = SKData.Create(stream);
using var typeface = SKTypeface.FromData(data);
string familyName = typeface.FamilyName; // Pass this to FromFamilyName
```

**Recommended proposal:** Use MatchCharacter to resolve CJK family names

**Why:** Requires no additional assets and works with system-installed CJK fonts on Android and iOS. Directly answers the reporter's constraint of not being able to modify the Mapsui library's font selection logic.

## Recommendations

### Actionability

| Field | Value |
|-------|-------|
| Suggested action | close-as-not-a-bug |
| Confidence | 0.88 (88%) |
| Reason | This is a how-to question with a known answer. The SkiaSharp API works as designed; the correct API for character-based font discovery is SKFontManager.MatchCharacter(). The reporter self-answered and the question is fully resolved. |
| Suggested repro platform | linux |

### Automatable Actions

| Type | Risk | Confidence | Description | Details |
|------|------|------------|-------------|---------|
| update-labels | low | 0.95 (95%) | Apply question, SkiaSharp core area, and platform labels | labels=type/question, area/SkiaSharp, os/Android, os/iOS |
| add-comment | medium | 0.88 (88%) | Post answer with MatchCharacter workaround and bundled-font alternative | — |
| close-issue | medium | 0.85 (85%) | Close as answered usage question | stateReason=completed |

**Comment draft for `add-comment`:**

```markdown
Thanks for the question and for sharing your workaround!

The right SkiaSharp API for CJK font discovery is `SKFontManager.MatchCharacter()`, which searches system fonts for one that can render a specific Unicode codepoint — rather than `FromFamilyName`, which requires you to know the exact font family name. Here's a safe version of the approach you found:

```csharp
string GetCjkFontFamily(char representativeChar)
{
    var typeface = SKFontManager.Default.MatchCharacter(representativeChar);
    return typeface?.FamilyName;
}

// Per-language representative characters:
string jaFamily  = GetCjkFontFamily('\u3042'); // hiragana 'a'
string koFamily  = GetCjkFontFamily('\ub9e4'); // Korean
string zhCnFamily = GetCjkFontFamily('\u5b9e'); // Simplified Chinese
string zhTwFamily = GetCjkFontFamily('\u5be6'); // Traditional Chinese
```

If you need a portable solution that doesn't rely on system fonts, you can bundle a CJK font (e.g., [Noto Sans CJK](https://fonts.google.com/noto/use#faq-which-font-files-should-i-use)) as an embedded resource and load it with `SKTypeface.FromData()`:

```csharp
using var stream = Assembly.GetExecutingAssembly()
    .GetManifestResourceStream("MyApp.Fonts.NotoSansCJK-Regular.ttf");
using var data = SKData.Create(stream);
using var typeface = SKTypeface.FromData(data);
string familyName = typeface.FamilyName;
```

Closing as answered — feel free to reopen if you hit other issues.
```

<details>
<summary>Raw JSON</summary>

```json
{
  "meta": {
    "schemaVersion": "1.0",
    "number": 2471,
    "repo": "mono/SkiaSharp",
    "analyzedAt": "2026-04-26T17:56:00Z"
  },
  "summary": "User asks how to get SKTypeface.FromFamilyName to return a suitable CJK (Japanese, Korean, Chinese) font on Android and iOS, since the Mapsui library uses this API internally and CJK text renders as squares.",
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
      "os/Android",
      "os/iOS"
    ]
  },
  "evidence": {
    "reproEvidence": {
      "stepsToReproduce": [
        "Use Mapsui library which calls SKTypeface.FromFamilyName internally (LabelRenderer.cs#L277)",
        "Display text containing Japanese, Korean, or Chinese characters",
        "Observe squares (tofu) rendered instead of the actual characters"
      ],
      "environmentDetails": "Android + iOS, Xamarin.Forms",
      "repoLinks": [
        {
          "url": "https://github.com/Mapsui/Mapsui/blob/5008d3ab8b0453c27cb487fe6ad3fac87435abbe/Mapsui.Rendering.Skia/LabelRenderer.cs#L277",
          "description": "Third-party library call site using SKTypeface.FromFamilyName"
        }
      ]
    }
  },
  "analysis": {
    "summary": "User cannot render CJK text because the Mapsui library uses SKTypeface.FromFamilyName internally with a font family name that has no CJK glyphs. The correct SkiaSharp API for character-based font matching is SKFontManager.MatchCharacter(). The reporter self-answered with a working workaround using MatchCharacter to obtain the correct family name per language/locale.",
    "codeInvestigation": [
      {
        "file": "binding/SkiaSharp/SKTypeface.cs",
        "lines": "79-80",
        "finding": "FromFamilyName(familyName, SKFontStyle) delegates to SKFontManager.Default.MatchFamily(familyName, style) ?? Default. If the named family has no CJK glyphs, MatchFamily either returns null (falls back to Default) or returns a Latin-only typeface, causing squares for CJK codepoints.",
        "relevance": "direct"
      },
      {
        "file": "binding/SkiaSharp/SKFontManager.cs",
        "lines": "131-133",
        "finding": "MatchCharacter(char character) calls sk_fontmgr_match_family_style_character, which searches installed system fonts for a typeface containing that Unicode codepoint — the correct API for finding a CJK-capable font without knowing the platform-specific family name.",
        "relevance": "direct"
      }
    ],
    "keySignals": [
      {
        "text": "if the text to display is japanese, korean or chinese, it just prints squares",
        "source": "issue body",
        "interpretation": "The default typeface returned by FromFamilyName lacks CJK glyphs — not a bug in SkiaSharp, but a limitation of passing a non-CJK family name."
      },
      {
        "text": "SKFontManager.Default.MatchCharacter('あ').FamilyName",
        "source": "comment #1574110595 by reporter",
        "interpretation": "Reporter self-discovered the correct workaround using MatchCharacter to find a system font for a representative CJK character."
      },
      {
        "text": "can you explicitly name a system font family known to be available on the corresponding platform? E.g. 'Noto Sans CJK'?",
        "source": "comment #2224232143 by HinTak",
        "interpretation": "Alternative suggestion: use platform-specific CJK font family names directly if known."
      }
    ],
    "rationale": "This is a usage question with a clear answer. FromFamilyName requires knowing the exact family name; for cross-platform CJK support where family names differ by OS, MatchCharacter is the designed solution. The reporter already found and posted the workaround. The behavior of FromFamilyName is by-design.",
    "workarounds": [
      "Use SKFontManager.Default.MatchCharacter(char) with a representative character from the target language to find the correct system typeface — e.g., MatchCharacter('\\u3042') for Japanese, MatchCharacter('\\ub9e4') for Korean, MatchCharacter('\\u5b9e') for Simplified Chinese.",
      "Use SKTypeface.FromFile() or SKTypeface.FromData() to load a bundled CJK font (e.g., Noto Sans CJK) directly by file path, bypassing family-name lookup entirely.",
      "On platforms where the family name is known and stable, pass the platform-specific name directly to FromFamilyName (e.g., 'Noto Sans CJK JP' on Android, 'Hiragino Sans' on iOS)."
    ],
    "resolution": {
      "hypothesis": "The reporter needs a way to pass CJK-capable typefaces into a library that calls FromFamilyName internally. The cleanest solution is to use MatchCharacter to obtain a family name that works on the current platform.",
      "proposals": [
        {
          "title": "Use MatchCharacter to resolve CJK family names",
          "description": "Use SKFontManager.Default.MatchCharacter() with a representative codepoint for each language to discover the platform font family name. Null-check the result before calling FamilyName.",
          "category": "workaround",
          "codeSnippet": "string GetCjkFontFamily(char representativeChar)\n{\n    var typeface = SKFontManager.Default.MatchCharacter(representativeChar);\n    return typeface?.FamilyName;\n}\n\n// Usage:\nstring jaFamily = GetCjkFontFamily('\\u3042'); // hiragana 'a'\nstring koFamily = GetCjkFontFamily('\\ub9e4'); // Korean character\nstring zhCnFamily = GetCjkFontFamily('\\u5b9e'); // Simplified Chinese\nstring zhTwFamily = GetCjkFontFamily('\\u5be6'); // Traditional Chinese",
          "confidence": 0.9,
          "effort": "cost/xs",
          "validated": "yes"
        },
        {
          "title": "Bundle and load a CJK font directly",
          "description": "Include a CJK-capable font (e.g., Noto Sans CJK) as an embedded resource and load it with SKTypeface.FromData(). Use FamilyName from the loaded typeface to pass to any API that requires a family name string.",
          "category": "alternative",
          "codeSnippet": "// Load bundled font and get its family name\nusing var stream = Assembly.GetExecutingAssembly()\n    .GetManifestResourceStream(\"MyApp.Fonts.NotoSansCJK-Regular.ttf\");\nusing var data = SKData.Create(stream);\nusing var typeface = SKTypeface.FromData(data);\nstring familyName = typeface.FamilyName; // Pass this to FromFamilyName",
          "confidence": 0.85,
          "effort": "cost/s",
          "validated": "yes"
        }
      ],
      "recommendedProposal": "Use MatchCharacter to resolve CJK family names",
      "recommendedReason": "Requires no additional assets and works with system-installed CJK fonts on Android and iOS. Directly answers the reporter's constraint of not being able to modify the Mapsui library's font selection logic."
    }
  },
  "output": {
    "actionability": {
      "suggestedAction": "close-as-not-a-bug",
      "confidence": 0.88,
      "reason": "This is a how-to question with a known answer. The SkiaSharp API works as designed; the correct API for character-based font discovery is SKFontManager.MatchCharacter(). The reporter self-answered and the question is fully resolved.",
      "suggestedReproPlatform": "linux"
    },
    "actions": [
      {
        "type": "update-labels",
        "description": "Apply question, SkiaSharp core area, and platform labels",
        "risk": "low",
        "confidence": 0.95,
        "labels": [
          "type/question",
          "area/SkiaSharp",
          "os/Android",
          "os/iOS"
        ]
      },
      {
        "type": "add-comment",
        "description": "Post answer with MatchCharacter workaround and bundled-font alternative",
        "risk": "medium",
        "confidence": 0.88,
        "comment": "Thanks for the question and for sharing your workaround!\n\nThe right SkiaSharp API for CJK font discovery is `SKFontManager.MatchCharacter()`, which searches system fonts for one that can render a specific Unicode codepoint — rather than `FromFamilyName`, which requires you to know the exact font family name. Here's a safe version of the approach you found:\n\n```csharp\nstring GetCjkFontFamily(char representativeChar)\n{\n    var typeface = SKFontManager.Default.MatchCharacter(representativeChar);\n    return typeface?.FamilyName;\n}\n\n// Per-language representative characters:\nstring jaFamily  = GetCjkFontFamily('\\u3042'); // hiragana 'a'\nstring koFamily  = GetCjkFontFamily('\\ub9e4'); // Korean\nstring zhCnFamily = GetCjkFontFamily('\\u5b9e'); // Simplified Chinese\nstring zhTwFamily = GetCjkFontFamily('\\u5be6'); // Traditional Chinese\n```\n\nIf you need a portable solution that doesn't rely on system fonts, you can bundle a CJK font (e.g., [Noto Sans CJK](https://fonts.google.com/noto/use#faq-which-font-files-should-i-use)) as an embedded resource and load it with `SKTypeface.FromData()`:\n\n```csharp\nusing var stream = Assembly.GetExecutingAssembly()\n    .GetManifestResourceStream(\"MyApp.Fonts.NotoSansCJK-Regular.ttf\");\nusing var data = SKData.Create(stream);\nusing var typeface = SKTypeface.FromData(data);\nstring familyName = typeface.FamilyName;\n```\n\nClosing as answered — feel free to reopen if you hit other issues."
      },
      {
        "type": "close-issue",
        "description": "Close as answered usage question",
        "risk": "medium",
        "confidence": 0.85,
        "stateReason": "completed"
      }
    ]
  }
}
```

</details>
