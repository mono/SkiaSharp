# Issue Triage Report — #2298

| Field | Value |
|-------|-------|
| Repository | mono/SkiaSharp |
| Analyzed | 2026-04-25T00:01:59Z |
| Type | type/question (0.92 (92%)) |
| Area | area/SkiaSharp (0.88 (88%)) |
| Suggested action | close-as-not-a-bug (0.88 (88%)) |

**Issue Summary:** User asks how to use SKFontManager.MatchCharacter with emoji characters and how to render complex emoji in SkiaSharp, hitting a crash because emoji in .NET strings are stored as surrogate pairs (two chars) rather than a single Unicode code point.

**Analysis:** The crash occurs because emoji characters (e.g., 😍 U+1F60D) are stored as UTF-16 surrogate pairs in .NET strings. Calling ToCharArray() yields two char values per emoji, and passing a surrogate-half char to MatchCharacter(char) implicitly casts it to an invalid Unicode code point, causing Skia's native code to crash. The correct approach is to iterate the string using StringInfo or by combining surrogate pairs with char.ConvertToUtf32, then pass the resulting int code point to the int overload. For rendering complex emoji (including ZWJ sequences), Skia itself does not perform text shaping; SkiaSharp.HarfBuzz / SKTextShaper must be used.

**Recommendations:** **close-as-not-a-bug** — Both questions (crash in MatchCharacter, complex emoji rendering) have been answered in comments. The crash is a usage error (passing surrogate halves to an int-based API); the rendering limitation is by design. Existing workarounds are well-documented in related issues.

---

## Classification

| Field | Value |
|-------|-------|
| Type | type/question |
| Area | area/SkiaSharp |
| Platforms | — |
| Backends | — |
| Tenets | — |
| Partner | — |

## Evidence

### Reproduction

1. Call value.ToCharArray() on a string containing 'P' followed by a heart-eyes emoji
2. Iterate over chars and call fontManager.MatchCharacter(charArray[index]) for each char
3. Observe crash when index reaches the high-surrogate char of the emoji

**Environment:** Xamarin app with SkiaSharp; Apple emoji font on iOS device

**Repository links:**
- https://github.com/mono/SkiaSharp/issues/982 — Related: emoji causes PDF to be too large — shows StringUtilities.GetUnicodeCharacterCode pattern
- https://github.com/mono/SkiaSharp/issues/232 — Related: SKCanvas.DrawText does not render Unicode characters — Skia does not perform text shaping

**Code snippets:**

```csharp
var charArray = value.ToCharArray();
while (++index < charArray.Length)
{
    var result = fontManager.MatchCharacter(paint.Typeface.FamilyName, charArray[index]);
    if (result == null)
    {
        var emojiTypeface = fontManager.MatchCharacter(charArray[index]);
    }
}
```

## Analysis

### Technical Summary

The crash occurs because emoji characters (e.g., 😍 U+1F60D) are stored as UTF-16 surrogate pairs in .NET strings. Calling ToCharArray() yields two char values per emoji, and passing a surrogate-half char to MatchCharacter(char) implicitly casts it to an invalid Unicode code point, causing Skia's native code to crash. The correct approach is to iterate the string using StringInfo or by combining surrogate pairs with char.ConvertToUtf32, then pass the resulting int code point to the int overload. For rendering complex emoji (including ZWJ sequences), Skia itself does not perform text shaping; SkiaSharp.HarfBuzz / SKTextShaper must be used.

### Rationale

This is a usage question, not a bug. The crash is caused by misusing the char overload of MatchCharacter with surrogate characters; the API itself is working as designed. The lack of complex emoji rendering via DrawText is by design — Skia delegates text shaping to HarfBuzz. Both issues have been explained in comments by a contributor. The correct patterns are documented in related issues.

### Key Signals

- "I see that the Array gets 3 chars but I think it's should be 2." — **issue body** (Confirms the reporter is iterating char values without handling surrogate pairs; a single emoji occupies 2 chars in .NET strings.)
- "Some characters come in surrogate pairs and therefore are stored via two char values" — **comment by Gillibald** (Root cause of crash explained — reporter needs to use int code points, not chars.)
- "Skia isn't doing any shaping. Use SkiaSharp.Harfbuzz" — **comment by Gillibald** (Complex emoji rendering requires the HarfBuzz text shaper; simple DrawText will not produce correct results.)

### Code Investigation

| File | Lines | Relevance | Finding |
|------|-------|-----------|---------|
| `binding/SkiaSharp/SKFontManager.cs` | 131-133 | direct | The char overload of MatchCharacter passes the char value directly (implicitly cast to int) to the int overload. For emoji encoded as surrogate pairs, each half is an isolated surrogate (U+D800–U+DFFF), which is not a valid Unicode scalar value. This causes Skia's native sk_fontmgr_match_family_style_character to receive an invalid code point and crash. |
| `binding/SkiaSharp/SKFontManager.cs` | 176-191 | direct | The canonical int overload calls sk_fontmgr_match_family_style_character with the character as a raw int Unicode code point. This works correctly when given a full code point (e.g., 0x1F60D for 😍) obtained via char.ConvertToUtf32 or StringUtilities.GetUnicodeCharacterCode. |
| `binding/SkiaSharp/Util.cs` | 148-157 | related | StringUtilities.GetUnicodeCharacterCode(string, SKTextEncoding.Utf32) provides the correct way to obtain a full Unicode code point integer from a single-character string containing an emoji. This is the recommended API for this use-case. |

### Workarounds

- Use char.IsHighSurrogate / char.IsLowSurrogate and char.ConvertToUtf32 to reconstruct full Unicode code points when iterating a string
- Use StringUtilities.GetUnicodeCharacterCode(emojiString, SKTextEncoding.Utf32) to obtain the int code point for a single emoji
- Use SkiaSharp.HarfBuzz (SKTextShaper) for rendering complex emoji and mixed text/emoji strings

### Resolution Proposals

**Hypothesis:** Reporter needs to (1) use Unicode scalar values (int) rather than individual chars when calling MatchCharacter with emoji, and (2) use SkiaSharp.HarfBuzz for rendering strings that mix regular text and complex emoji.

1. **Iterate string by Unicode scalar values, not chars** — workaround, confidence 0.92 (92%), cost/s, validated=yes
   - Use StringInfo.GetTextElementEnumerator or manually handle surrogate pairs to obtain full Unicode code points before calling MatchCharacter(int).

```csharp
// Correct way to iterate a string and match fonts for each Unicode code point
for (var i = 0; i < value.Length; )
{
    int codePoint;
    if (char.IsHighSurrogate(value[i]) && i + 1 < value.Length && char.IsLowSurrogate(value[i + 1]))
    {
        codePoint = char.ConvertToUtf32(value[i], value[i + 1]);
        i += 2;
    }
    else
    {
        codePoint = value[i];
        i++;
    }
    var typeface = fontManager.MatchCharacter(paint.Typeface.FamilyName, codePoint);
    // use typeface to draw the character
}
```
2. **Use SkiaSharp.HarfBuzz for complex emoji rendering** — workaround, confidence 0.90 (90%), cost/s, validated=yes
   - For rendering strings with mixed text and complex emoji (including ZWJ sequences), use SKTextShaper from the SkiaSharp.HarfBuzz package. Skia's DrawText does not perform text shaping.

```csharp
// Add NuGet package: SkiaSharp.HarfBuzz
using SkiaSharp.HarfBuzz;

var paint = new SKPaint { Typeface = emojiTypeface };
using var shaper = new SKShaper(paint.Typeface);
canvas.DrawShapedText(shaper, "P 😍", 20, 40, paint);
```

**Recommended proposal:** Iterate string by Unicode scalar values, not chars

**Why:** Directly addresses the crash by correctly handling surrogate pairs when calling MatchCharacter. HarfBuzz can then be used for the final rendering step.

## Recommendations

### Actionability

| Field | Value |
|-------|-------|
| Suggested action | close-as-not-a-bug |
| Confidence | 0.88 (88%) |
| Reason | Both questions (crash in MatchCharacter, complex emoji rendering) have been answered in comments. The crash is a usage error (passing surrogate halves to an int-based API); the rendering limitation is by design. Existing workarounds are well-documented in related issues. |
| Suggested repro platform | linux |

### Automatable Actions

| Type | Risk | Confidence | Description | Details |
|------|------|------------|-------------|---------|
| update-labels | low | 0.92 (92%) | Apply question and core SkiaSharp area labels | labels=type/question, area/SkiaSharp |
| add-comment | medium | 0.88 (88%) | Post answer explaining surrogate pairs and HarfBuzz workaround | — |
| close-issue | medium | 0.85 (85%) | Close as answered — both questions resolved with documented workarounds | stateReason=completed |
| link-related | low | 0.88 (88%) | Link to related emoji question #982 | linkedIssue=#982 |

**Comment draft for `add-comment`:**

```markdown
Thanks for the question! There are two separate issues here:

**1. Crash in `MatchCharacter`**

Emoji like 😍 are stored as UTF-16 *surrogate pairs* in .NET strings — `ToCharArray()` gives you two `char` values for each such emoji (one high surrogate + one low surrogate), which is why you see 3 chars for "P😍". Passing a surrogate `char` to `MatchCharacter(char)` sends an invalid Unicode code point to Skia's native code.

Use `char.IsHighSurrogate` / `char.ConvertToUtf32` to reconstruct the full Unicode scalar value, then use the `int` overload:

```csharp
for (var i = 0; i < text.Length; )
{
    int codePoint;
    if (char.IsHighSurrogate(text[i]) && i + 1 < text.Length && char.IsLowSurrogate(text[i + 1]))
    {
        codePoint = char.ConvertToUtf32(text[i], text[i + 1]);
        i += 2;
    }
    else
    {
        codePoint = text[i];
        i++;
    }
    var typeface = fontManager.MatchCharacter(paint.Typeface.FamilyName, codePoint);
    // use typeface to render the character
}
```

Alternatively, `StringUtilities.GetUnicodeCharacterCode("😍", SKTextEncoding.Utf32)` returns the correct int code point for a single emoji.

**2. Complex emoji not rendering correctly**

Skia does not perform text shaping on its own — complex emoji (including ZWJ sequences like 👨‍👩‍👧) require HarfBuzz. Install the **`SkiaSharp.HarfBuzz`** NuGet package and use `SKShaper` / `DrawShapedText`:

```csharp
using SkiaSharp.HarfBuzz;

using var shaper = new SKShaper(paint.Typeface);
canvas.DrawShapedText(shaper, "P 😍", x, y, paint);
```

See also #982 for a working emoji pattern with `MatchCharacter`.
```

<details>
<summary>Raw JSON</summary>

```json
{
  "meta": {
    "schemaVersion": "1.0",
    "number": 2298,
    "repo": "mono/SkiaSharp",
    "analyzedAt": "2026-04-25T00:01:59Z"
  },
  "summary": "User asks how to use SKFontManager.MatchCharacter with emoji characters and how to render complex emoji in SkiaSharp, hitting a crash because emoji in .NET strings are stored as surrogate pairs (two chars) rather than a single Unicode code point.",
  "classification": {
    "type": {
      "value": "type/question",
      "confidence": 0.92
    },
    "area": {
      "value": "area/SkiaSharp",
      "confidence": 0.88
    }
  },
  "evidence": {
    "reproEvidence": {
      "stepsToReproduce": [
        "Call value.ToCharArray() on a string containing 'P' followed by a heart-eyes emoji",
        "Iterate over chars and call fontManager.MatchCharacter(charArray[index]) for each char",
        "Observe crash when index reaches the high-surrogate char of the emoji"
      ],
      "codeSnippets": [
        "var charArray = value.ToCharArray();\nwhile (++index < charArray.Length)\n{\n    var result = fontManager.MatchCharacter(paint.Typeface.FamilyName, charArray[index]);\n    if (result == null)\n    {\n        var emojiTypeface = fontManager.MatchCharacter(charArray[index]);\n    }\n}"
      ],
      "environmentDetails": "Xamarin app with SkiaSharp; Apple emoji font on iOS device",
      "repoLinks": [
        {
          "url": "https://github.com/mono/SkiaSharp/issues/982",
          "description": "Related: emoji causes PDF to be too large — shows StringUtilities.GetUnicodeCharacterCode pattern"
        },
        {
          "url": "https://github.com/mono/SkiaSharp/issues/232",
          "description": "Related: SKCanvas.DrawText does not render Unicode characters — Skia does not perform text shaping"
        }
      ]
    }
  },
  "analysis": {
    "summary": "The crash occurs because emoji characters (e.g., 😍 U+1F60D) are stored as UTF-16 surrogate pairs in .NET strings. Calling ToCharArray() yields two char values per emoji, and passing a surrogate-half char to MatchCharacter(char) implicitly casts it to an invalid Unicode code point, causing Skia's native code to crash. The correct approach is to iterate the string using StringInfo or by combining surrogate pairs with char.ConvertToUtf32, then pass the resulting int code point to the int overload. For rendering complex emoji (including ZWJ sequences), Skia itself does not perform text shaping; SkiaSharp.HarfBuzz / SKTextShaper must be used.",
    "rationale": "This is a usage question, not a bug. The crash is caused by misusing the char overload of MatchCharacter with surrogate characters; the API itself is working as designed. The lack of complex emoji rendering via DrawText is by design — Skia delegates text shaping to HarfBuzz. Both issues have been explained in comments by a contributor. The correct patterns are documented in related issues.",
    "keySignals": [
      {
        "text": "I see that the Array gets 3 chars but I think it's should be 2.",
        "source": "issue body",
        "interpretation": "Confirms the reporter is iterating char values without handling surrogate pairs; a single emoji occupies 2 chars in .NET strings."
      },
      {
        "text": "Some characters come in surrogate pairs and therefore are stored via two char values",
        "source": "comment by Gillibald",
        "interpretation": "Root cause of crash explained — reporter needs to use int code points, not chars."
      },
      {
        "text": "Skia isn't doing any shaping. Use SkiaSharp.Harfbuzz",
        "source": "comment by Gillibald",
        "interpretation": "Complex emoji rendering requires the HarfBuzz text shaper; simple DrawText will not produce correct results."
      }
    ],
    "codeInvestigation": [
      {
        "file": "binding/SkiaSharp/SKFontManager.cs",
        "lines": "131-133",
        "finding": "The char overload of MatchCharacter passes the char value directly (implicitly cast to int) to the int overload. For emoji encoded as surrogate pairs, each half is an isolated surrogate (U+D800–U+DFFF), which is not a valid Unicode scalar value. This causes Skia's native sk_fontmgr_match_family_style_character to receive an invalid code point and crash.",
        "relevance": "direct"
      },
      {
        "file": "binding/SkiaSharp/SKFontManager.cs",
        "lines": "176-191",
        "finding": "The canonical int overload calls sk_fontmgr_match_family_style_character with the character as a raw int Unicode code point. This works correctly when given a full code point (e.g., 0x1F60D for 😍) obtained via char.ConvertToUtf32 or StringUtilities.GetUnicodeCharacterCode.",
        "relevance": "direct"
      },
      {
        "file": "binding/SkiaSharp/Util.cs",
        "lines": "148-157",
        "finding": "StringUtilities.GetUnicodeCharacterCode(string, SKTextEncoding.Utf32) provides the correct way to obtain a full Unicode code point integer from a single-character string containing an emoji. This is the recommended API for this use-case.",
        "relevance": "related"
      }
    ],
    "workarounds": [
      "Use char.IsHighSurrogate / char.IsLowSurrogate and char.ConvertToUtf32 to reconstruct full Unicode code points when iterating a string",
      "Use StringUtilities.GetUnicodeCharacterCode(emojiString, SKTextEncoding.Utf32) to obtain the int code point for a single emoji",
      "Use SkiaSharp.HarfBuzz (SKTextShaper) for rendering complex emoji and mixed text/emoji strings"
    ],
    "resolution": {
      "hypothesis": "Reporter needs to (1) use Unicode scalar values (int) rather than individual chars when calling MatchCharacter with emoji, and (2) use SkiaSharp.HarfBuzz for rendering strings that mix regular text and complex emoji.",
      "proposals": [
        {
          "title": "Iterate string by Unicode scalar values, not chars",
          "description": "Use StringInfo.GetTextElementEnumerator or manually handle surrogate pairs to obtain full Unicode code points before calling MatchCharacter(int).",
          "category": "workaround",
          "codeSnippet": "// Correct way to iterate a string and match fonts for each Unicode code point\nfor (var i = 0; i < value.Length; )\n{\n    int codePoint;\n    if (char.IsHighSurrogate(value[i]) && i + 1 < value.Length && char.IsLowSurrogate(value[i + 1]))\n    {\n        codePoint = char.ConvertToUtf32(value[i], value[i + 1]);\n        i += 2;\n    }\n    else\n    {\n        codePoint = value[i];\n        i++;\n    }\n    var typeface = fontManager.MatchCharacter(paint.Typeface.FamilyName, codePoint);\n    // use typeface to draw the character\n}",
          "confidence": 0.92,
          "effort": "cost/s",
          "validated": "yes"
        },
        {
          "title": "Use SkiaSharp.HarfBuzz for complex emoji rendering",
          "description": "For rendering strings with mixed text and complex emoji (including ZWJ sequences), use SKTextShaper from the SkiaSharp.HarfBuzz package. Skia's DrawText does not perform text shaping.",
          "category": "workaround",
          "codeSnippet": "// Add NuGet package: SkiaSharp.HarfBuzz\nusing SkiaSharp.HarfBuzz;\n\nvar paint = new SKPaint { Typeface = emojiTypeface };\nusing var shaper = new SKShaper(paint.Typeface);\ncanvas.DrawShapedText(shaper, \"P 😍\", 20, 40, paint);",
          "confidence": 0.9,
          "effort": "cost/s",
          "validated": "yes"
        }
      ],
      "recommendedProposal": "Iterate string by Unicode scalar values, not chars",
      "recommendedReason": "Directly addresses the crash by correctly handling surrogate pairs when calling MatchCharacter. HarfBuzz can then be used for the final rendering step."
    }
  },
  "output": {
    "actionability": {
      "suggestedAction": "close-as-not-a-bug",
      "confidence": 0.88,
      "reason": "Both questions (crash in MatchCharacter, complex emoji rendering) have been answered in comments. The crash is a usage error (passing surrogate halves to an int-based API); the rendering limitation is by design. Existing workarounds are well-documented in related issues.",
      "suggestedReproPlatform": "linux"
    },
    "actions": [
      {
        "type": "update-labels",
        "description": "Apply question and core SkiaSharp area labels",
        "risk": "low",
        "confidence": 0.92,
        "labels": [
          "type/question",
          "area/SkiaSharp"
        ]
      },
      {
        "type": "add-comment",
        "description": "Post answer explaining surrogate pairs and HarfBuzz workaround",
        "risk": "medium",
        "confidence": 0.88,
        "comment": "Thanks for the question! There are two separate issues here:\n\n**1. Crash in `MatchCharacter`**\n\nEmoji like 😍 are stored as UTF-16 *surrogate pairs* in .NET strings — `ToCharArray()` gives you two `char` values for each such emoji (one high surrogate + one low surrogate), which is why you see 3 chars for \"P😍\". Passing a surrogate `char` to `MatchCharacter(char)` sends an invalid Unicode code point to Skia's native code.\n\nUse `char.IsHighSurrogate` / `char.ConvertToUtf32` to reconstruct the full Unicode scalar value, then use the `int` overload:\n\n```csharp\nfor (var i = 0; i < text.Length; )\n{\n    int codePoint;\n    if (char.IsHighSurrogate(text[i]) && i + 1 < text.Length && char.IsLowSurrogate(text[i + 1]))\n    {\n        codePoint = char.ConvertToUtf32(text[i], text[i + 1]);\n        i += 2;\n    }\n    else\n    {\n        codePoint = text[i];\n        i++;\n    }\n    var typeface = fontManager.MatchCharacter(paint.Typeface.FamilyName, codePoint);\n    // use typeface to render the character\n}\n```\n\nAlternatively, `StringUtilities.GetUnicodeCharacterCode(\"😍\", SKTextEncoding.Utf32)` returns the correct int code point for a single emoji.\n\n**2. Complex emoji not rendering correctly**\n\nSkia does not perform text shaping on its own — complex emoji (including ZWJ sequences like 👨‍👩‍👧) require HarfBuzz. Install the **`SkiaSharp.HarfBuzz`** NuGet package and use `SKShaper` / `DrawShapedText`:\n\n```csharp\nusing SkiaSharp.HarfBuzz;\n\nusing var shaper = new SKShaper(paint.Typeface);\ncanvas.DrawShapedText(shaper, \"P 😍\", x, y, paint);\n```\n\nSee also #982 for a working emoji pattern with `MatchCharacter`."
      },
      {
        "type": "close-issue",
        "description": "Close as answered — both questions resolved with documented workarounds",
        "risk": "medium",
        "confidence": 0.85,
        "stateReason": "completed"
      },
      {
        "type": "link-related",
        "description": "Link to related emoji question #982",
        "risk": "low",
        "confidence": 0.88,
        "linkedIssue": 982
      }
    ]
  }
}
```

</details>
