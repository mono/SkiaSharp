# Issue Triage Report — #956

| Field | Value |
|-------|-------|
| Repository | mono/SkiaSharp |
| Analyzed | 2026-04-26T09:31:25Z |
| Type | type/bug (0.78 (78%)) |
| Area | area/SkiaSharp (0.90 (90%)) |
| Suggested action | needs-info (0.80 (80%)) |

**Issue Summary:** SKTypeface.FromFamilyName("Wingdings", ...) returns Helvetica instead of the Wingdings typeface on macOS 10.14 with SkiaSharp 1.59.3, even though the font is installed in ~/Library/Fonts.

**Analysis:** SKTypeface.FromFamilyName delegates to SKFontManager.Default.MatchFamily which calls Skia's CoreText-based font manager. On macOS, CoreText's legacyMakeTypeface falls back to Helvetica when the requested font name is not resolved. The reporter's Wingdings font is in ~/Library/Fonts (user-level), which may not have been enumerated by Skia's CoreText manager in version 1.59.3. The maintainer was able to find Wingdings on their system, suggesting the issue is environment-specific.

**Recommendations:** **needs-info** — The maintainer asked for font enumeration output in November 2019 and never received a response. The issue is filed against a very old version (1.59.3). Need to confirm whether the font appears in SKFontManager.GetFontFamilies() and whether the issue still reproduces on newer SkiaSharp versions.

---

## Classification

| Field | Value |
|-------|-------|
| Type | type/bug |
| Area | area/SkiaSharp |
| Platforms | os/macOS |
| Backends | — |
| Tenets | tenet/reliability |
| Partner | — |

## Evidence

### Reproduction

1. Install Wingdings font to ~/Library/Fonts on macOS 10.14
2. Call: SKTypeface typeface = SKTypeface.FromFamilyName("Wingdings", SKFontStyleWeight.Normal, SKFontStyleWidth.Normal, SKFontStyleSlant.Upright)
3. Observe that typeface.FamilyName returns "Helvetica" instead of "Wingdings"

**Environment:** SkiaSharp 1.59.3, macOS 10.14, iOS 12.1, Visual Studio 2017

**Repository links:**
- https://github.com/mono/SkiaSharp/issues/1058 — Related: SKTypeface.FromFamilyName inconsistent null/fallback behavior across platforms
- https://github.com/mono/SkiaSharp/issues/3737 — v4 breaking change: FromFamilyName now returns Default instead of null on Android, macOS CoreText still falls back

### Bug Signals

| Field | Value |
|-------|-------|
| Severity | medium |
| Regression claimed | False |
| Error type | wrong-output |
| Error message | SKTypeface.FromFamilyName returns Helvetica instead of Wingdings |
| Repro quality | partial |
| Target frameworks | — |

### Version Analysis

| Field | Value |
|-------|-------|
| Mentioned versions | 1.59.3 |
| Worked in | — |
| Broke in | — |
| Current relevance | unknown |
| Relevance reason | Issue was filed against 1.59.3 (2019). Maintainer's comment from 2019 indicated they could find Wingdings on their machine, suggesting the issue may be environment-specific. No update from reporter. |

## Analysis

### Technical Summary

SKTypeface.FromFamilyName delegates to SKFontManager.Default.MatchFamily which calls Skia's CoreText-based font manager. On macOS, CoreText's legacyMakeTypeface falls back to Helvetica when the requested font name is not resolved. The reporter's Wingdings font is in ~/Library/Fonts (user-level), which may not have been enumerated by Skia's CoreText manager in version 1.59.3. The maintainer was able to find Wingdings on their system, suggesting the issue is environment-specific.

### Rationale

This is classified as a bug because the API silently returns a completely different typeface (Helvetica) without any signal that the requested font was not found. The reporter explicitly expects Wingdings. The root cause is in Skia's macOS CoreText font manager fallback behavior. However, the maintainer confirmed the font can be found by SKFontManager.GetFontFamilies() on their machine, so this may be an environment or installation issue. Classified as needs-info since the maintainer asked for font enumeration results in November 2019 but never received a response.

### Key Signals

- "returns as Helvetica" — **issue body** (Skia's CoreText implementation on macOS falls back to Helvetica (the platform default) when the requested family name cannot be resolved.)
- "Font is available in the location: ~Library/Fonts" — **issue body** (Font is installed in user-level directory. Skia's CoreText manager may not enumerate user-level fonts the same way as system fonts in older versions.)
- "I had a go at this, and it appears on my machine." — **comment by mattleibow (2019-11-05)** (Maintainer confirmed Wingdings is findable via SKFontManager on their machine, suggesting the issue is environment-specific or an installation problem on the reporter's side.)
- "You need to load the font from stream if the font isn't installed on the system." — **comment by Gillibald (2019-09-21)** (Contributor suggests the font may not be considered 'installed on the system' by Skia's font manager, implying that user-level font directories may not be fully supported.)

### Code Investigation

| File | Lines | Relevance | Finding |
|------|-------|-----------|---------|
| `binding/SkiaSharp/SKTypeface.cs` | 79-80 | direct | SKTypeface.FromFamilyName(string, SKFontStyle) delegates to SKFontManager.Default.MatchFamily(familyName, style) ?? Default. If the font is not found by the font manager, it silently returns the platform Default typeface (Helvetica on macOS) with no indication of failure. |
| `binding/SkiaSharp/SKFontManager.cs` | 77-87 | direct | SKFontManager.MatchFamily calls the C API sk_fontmgr_match_family_style, which on macOS invokes Skia's CoreText-based font manager. If the family name is not found, this returns null which triggers the ?? Default fallback in FromFamilyName. |

### Workarounds

- Use SKTypeface.FromFile("~/Library/Fonts/Wingdings.ttf") to load the font directly from its file path, bypassing the family name lookup.
- Use SKFontManager.Default.GetFontFamilies() to enumerate all fonts and find the exact name Skia uses for the Wingdings font, then pass that exact name to FromFamilyName.
- Use SKTypeface.FromStream() with a FileStream to load the font from ~/Library/Fonts.

### Next Questions

- Does `SKFontManager.Default.GetFontFamilies()` include 'Wingdings' on the reporter's machine?
- Is the Wingdings font in ~/Library/Fonts (user) vs /Library/Fonts (system) or /System/Library/Fonts?
- Does the issue reproduce on newer SkiaSharp versions (2.x, 3.x)?
- Does the maintainer's suggestion of checking font enumeration resolve the issue?

### Resolution Proposals

**Hypothesis:** Skia's CoreText font manager on macOS may not enumerate or resolve fonts in user-level directories (~/Library/Fonts) as reliably as system-level font directories (/Library/Fonts). When the font name lookup fails, CoreText silently returns Helvetica as a fallback.

1. **Load font directly from file path** — workaround, confidence 0.85 (85%), cost/xs, validated=untested
   - Use SKTypeface.FromFile() with the explicit path to the Wingdings font file instead of relying on family name resolution. This bypasses the CoreText font manager lookup entirely.

```csharp
var typeface = SKTypeface.FromFile("/Users/username/Library/Fonts/Wingdings.ttf");
```
2. **Enumerate fonts to find exact family name** — workaround, confidence 0.80 (80%), cost/xs, validated=untested
   - Use SKFontManager to enumerate all available fonts and find the exact family name Skia uses for Wingdings on this system, then use that name with FromFamilyName.

```csharp
var fonts = SKFontManager.Default.GetFontFamilies();
var wingdingsFonts = fonts.Where(f => f.Contains("Wingdings")).ToArray();
// use wingdingsFonts[0] as familyName if found
var typeface = SKTypeface.FromFamilyName(wingdingsFonts[0]);
```

**Recommended proposal:** Load font directly from file path

**Why:** Most reliable approach: bypasses all font manager name resolution and works regardless of how CoreText enumerates user-level fonts.

## Recommendations

### Actionability

| Field | Value |
|-------|-------|
| Suggested action | needs-info |
| Confidence | 0.80 (80%) |
| Reason | The maintainer asked for font enumeration output in November 2019 and never received a response. The issue is filed against a very old version (1.59.3). Need to confirm whether the font appears in SKFontManager.GetFontFamilies() and whether the issue still reproduces on newer SkiaSharp versions. |
| Suggested repro platform | macos |

### Missing Info

- Does SKFontManager.Default.GetFontFamilies() include 'Wingdings' on the reporter's machine?
- Is the font in ~/Library/Fonts (user-level) or /Library/Fonts (system-level)?
- Does the issue still reproduce with a newer version of SkiaSharp (e.g., 2.x or 3.x)?

### Automatable Actions

| Type | Risk | Confidence | Description | Details |
|------|------|------------|-------------|---------|
| update-labels | low | 0.90 (90%) | Apply bug, SkiaSharp, macOS, reliability labels | labels=type/bug, area/SkiaSharp, os/macOS, tenet/reliability |
| add-comment | medium | 0.80 (80%) | Ask reporter for font enumeration output and whether issue still exists in newer versions | — |

**Comment draft for `add-comment`:**

```markdown
Thanks for reporting! It looks like Skia's CoreText font manager on macOS falls back to Helvetica when the requested font name is not found.

Could you try the following to help diagnose the issue:

1. **Check if Wingdings appears in the font list:**
```csharp
var fonts = SKFontManager.Default.GetFontFamilies();
var wingdingsFonts = fonts.Where(f => f.Contains("Wingdings")).ToArray();
Console.WriteLine(string.Join(", ", wingdingsFonts));
```

2. **If found**, use the exact name returned from the enumeration.

3. **If not found**, load the font directly from file:
```csharp
var typeface = SKTypeface.FromFile("/Users/username/Library/Fonts/Wingdings.ttf");
```

Also, does this issue still occur with a newer version of SkiaSharp (2.x or 3.x)?
```

<details>
<summary>Raw JSON</summary>

```json
{
  "meta": {
    "schemaVersion": "1.0",
    "number": 956,
    "repo": "mono/SkiaSharp",
    "analyzedAt": "2026-04-26T09:31:25Z"
  },
  "summary": "SKTypeface.FromFamilyName(\"Wingdings\", ...) returns Helvetica instead of the Wingdings typeface on macOS 10.14 with SkiaSharp 1.59.3, even though the font is installed in ~/Library/Fonts.",
  "classification": {
    "type": {
      "value": "type/bug",
      "confidence": 0.78
    },
    "area": {
      "value": "area/SkiaSharp",
      "confidence": 0.9
    },
    "platforms": [
      "os/macOS"
    ],
    "tenets": [
      "tenet/reliability"
    ]
  },
  "evidence": {
    "bugSignals": {
      "severity": "medium",
      "regressionClaimed": false,
      "errorType": "wrong-output",
      "errorMessage": "SKTypeface.FromFamilyName returns Helvetica instead of Wingdings",
      "reproQuality": "partial",
      "targetFrameworks": []
    },
    "reproEvidence": {
      "stepsToReproduce": [
        "Install Wingdings font to ~/Library/Fonts on macOS 10.14",
        "Call: SKTypeface typeface = SKTypeface.FromFamilyName(\"Wingdings\", SKFontStyleWeight.Normal, SKFontStyleWidth.Normal, SKFontStyleSlant.Upright)",
        "Observe that typeface.FamilyName returns \"Helvetica\" instead of \"Wingdings\""
      ],
      "environmentDetails": "SkiaSharp 1.59.3, macOS 10.14, iOS 12.1, Visual Studio 2017",
      "repoLinks": [
        {
          "url": "https://github.com/mono/SkiaSharp/issues/1058",
          "description": "Related: SKTypeface.FromFamilyName inconsistent null/fallback behavior across platforms"
        },
        {
          "url": "https://github.com/mono/SkiaSharp/issues/3737",
          "description": "v4 breaking change: FromFamilyName now returns Default instead of null on Android, macOS CoreText still falls back"
        }
      ]
    },
    "versionAnalysis": {
      "mentionedVersions": [
        "1.59.3"
      ],
      "currentRelevance": "unknown",
      "relevanceReason": "Issue was filed against 1.59.3 (2019). Maintainer's comment from 2019 indicated they could find Wingdings on their machine, suggesting the issue may be environment-specific. No update from reporter."
    }
  },
  "analysis": {
    "summary": "SKTypeface.FromFamilyName delegates to SKFontManager.Default.MatchFamily which calls Skia's CoreText-based font manager. On macOS, CoreText's legacyMakeTypeface falls back to Helvetica when the requested font name is not resolved. The reporter's Wingdings font is in ~/Library/Fonts (user-level), which may not have been enumerated by Skia's CoreText manager in version 1.59.3. The maintainer was able to find Wingdings on their system, suggesting the issue is environment-specific.",
    "rationale": "This is classified as a bug because the API silently returns a completely different typeface (Helvetica) without any signal that the requested font was not found. The reporter explicitly expects Wingdings. The root cause is in Skia's macOS CoreText font manager fallback behavior. However, the maintainer confirmed the font can be found by SKFontManager.GetFontFamilies() on their machine, so this may be an environment or installation issue. Classified as needs-info since the maintainer asked for font enumeration results in November 2019 but never received a response.",
    "keySignals": [
      {
        "text": "returns as Helvetica",
        "source": "issue body",
        "interpretation": "Skia's CoreText implementation on macOS falls back to Helvetica (the platform default) when the requested family name cannot be resolved."
      },
      {
        "text": "Font is available in the location: ~Library/Fonts",
        "source": "issue body",
        "interpretation": "Font is installed in user-level directory. Skia's CoreText manager may not enumerate user-level fonts the same way as system fonts in older versions."
      },
      {
        "text": "I had a go at this, and it appears on my machine.",
        "source": "comment by mattleibow (2019-11-05)",
        "interpretation": "Maintainer confirmed Wingdings is findable via SKFontManager on their machine, suggesting the issue is environment-specific or an installation problem on the reporter's side."
      },
      {
        "text": "You need to load the font from stream if the font isn't installed on the system.",
        "source": "comment by Gillibald (2019-09-21)",
        "interpretation": "Contributor suggests the font may not be considered 'installed on the system' by Skia's font manager, implying that user-level font directories may not be fully supported."
      }
    ],
    "codeInvestigation": [
      {
        "file": "binding/SkiaSharp/SKTypeface.cs",
        "lines": "79-80",
        "finding": "SKTypeface.FromFamilyName(string, SKFontStyle) delegates to SKFontManager.Default.MatchFamily(familyName, style) ?? Default. If the font is not found by the font manager, it silently returns the platform Default typeface (Helvetica on macOS) with no indication of failure.",
        "relevance": "direct"
      },
      {
        "file": "binding/SkiaSharp/SKFontManager.cs",
        "lines": "77-87",
        "finding": "SKFontManager.MatchFamily calls the C API sk_fontmgr_match_family_style, which on macOS invokes Skia's CoreText-based font manager. If the family name is not found, this returns null which triggers the ?? Default fallback in FromFamilyName.",
        "relevance": "direct"
      }
    ],
    "workarounds": [
      "Use SKTypeface.FromFile(\"~/Library/Fonts/Wingdings.ttf\") to load the font directly from its file path, bypassing the family name lookup.",
      "Use SKFontManager.Default.GetFontFamilies() to enumerate all fonts and find the exact name Skia uses for the Wingdings font, then pass that exact name to FromFamilyName.",
      "Use SKTypeface.FromStream() with a FileStream to load the font from ~/Library/Fonts."
    ],
    "nextQuestions": [
      "Does `SKFontManager.Default.GetFontFamilies()` include 'Wingdings' on the reporter's machine?",
      "Is the Wingdings font in ~/Library/Fonts (user) vs /Library/Fonts (system) or /System/Library/Fonts?",
      "Does the issue reproduce on newer SkiaSharp versions (2.x, 3.x)?",
      "Does the maintainer's suggestion of checking font enumeration resolve the issue?"
    ],
    "resolution": {
      "hypothesis": "Skia's CoreText font manager on macOS may not enumerate or resolve fonts in user-level directories (~/Library/Fonts) as reliably as system-level font directories (/Library/Fonts). When the font name lookup fails, CoreText silently returns Helvetica as a fallback.",
      "proposals": [
        {
          "title": "Load font directly from file path",
          "description": "Use SKTypeface.FromFile() with the explicit path to the Wingdings font file instead of relying on family name resolution. This bypasses the CoreText font manager lookup entirely.",
          "category": "workaround",
          "codeSnippet": "var typeface = SKTypeface.FromFile(\"/Users/username/Library/Fonts/Wingdings.ttf\");",
          "confidence": 0.85,
          "effort": "cost/xs",
          "validated": "untested"
        },
        {
          "title": "Enumerate fonts to find exact family name",
          "description": "Use SKFontManager to enumerate all available fonts and find the exact family name Skia uses for Wingdings on this system, then use that name with FromFamilyName.",
          "category": "workaround",
          "codeSnippet": "var fonts = SKFontManager.Default.GetFontFamilies();\nvar wingdingsFonts = fonts.Where(f => f.Contains(\"Wingdings\")).ToArray();\n// use wingdingsFonts[0] as familyName if found\nvar typeface = SKTypeface.FromFamilyName(wingdingsFonts[0]);",
          "confidence": 0.8,
          "effort": "cost/xs",
          "validated": "untested"
        }
      ],
      "recommendedProposal": "Load font directly from file path",
      "recommendedReason": "Most reliable approach: bypasses all font manager name resolution and works regardless of how CoreText enumerates user-level fonts."
    }
  },
  "output": {
    "actionability": {
      "suggestedAction": "needs-info",
      "confidence": 0.8,
      "reason": "The maintainer asked for font enumeration output in November 2019 and never received a response. The issue is filed against a very old version (1.59.3). Need to confirm whether the font appears in SKFontManager.GetFontFamilies() and whether the issue still reproduces on newer SkiaSharp versions.",
      "suggestedReproPlatform": "macos"
    },
    "missingInfo": [
      "Does SKFontManager.Default.GetFontFamilies() include 'Wingdings' on the reporter's machine?",
      "Is the font in ~/Library/Fonts (user-level) or /Library/Fonts (system-level)?",
      "Does the issue still reproduce with a newer version of SkiaSharp (e.g., 2.x or 3.x)?"
    ],
    "actions": [
      {
        "type": "update-labels",
        "description": "Apply bug, SkiaSharp, macOS, reliability labels",
        "risk": "low",
        "confidence": 0.9,
        "labels": [
          "type/bug",
          "area/SkiaSharp",
          "os/macOS",
          "tenet/reliability"
        ]
      },
      {
        "type": "add-comment",
        "description": "Ask reporter for font enumeration output and whether issue still exists in newer versions",
        "risk": "medium",
        "confidence": 0.8,
        "comment": "Thanks for reporting! It looks like Skia's CoreText font manager on macOS falls back to Helvetica when the requested font name is not found.\n\nCould you try the following to help diagnose the issue:\n\n1. **Check if Wingdings appears in the font list:**\n```csharp\nvar fonts = SKFontManager.Default.GetFontFamilies();\nvar wingdingsFonts = fonts.Where(f => f.Contains(\"Wingdings\")).ToArray();\nConsole.WriteLine(string.Join(\", \", wingdingsFonts));\n```\n\n2. **If found**, use the exact name returned from the enumeration.\n\n3. **If not found**, load the font directly from file:\n```csharp\nvar typeface = SKTypeface.FromFile(\"/Users/username/Library/Fonts/Wingdings.ttf\");\n```\n\nAlso, does this issue still occur with a newer version of SkiaSharp (2.x or 3.x)?"
      }
    ]
  }
}
```

</details>
