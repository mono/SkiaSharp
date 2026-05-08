# Issue Triage Report — #2253

| Field | Value |
|-------|-------|
| Repository | mono/SkiaSharp |
| Analyzed | 2026-05-02T11:45:37Z |
| Type | type/bug (0.72 (72%)) |
| Area | area/SkiaSharp (0.90 (90%)) |
| Suggested action | needs-info (0.78 (78%)) |

**Issue Summary:** On Android, SKFontManager.Default.MatchCharacter returns a SKTypeface with a non-English or locale-encoded FamilyName, while the same call on Windows returns a typeface with a normal English family name.

**Analysis:** SKFontManager.MatchCharacter delegates to Skia's sk_fontmgr_match_family_style_character, and SKTypeface.FamilyName calls sk_typeface_get_family_name. On Android, the matched typeface for CJK characters is a system font (e.g., NotoSansCJK) whose Skia-reported FamilyName may be its localized/internal name rather than an English name. This is likely an Android-platform-specific behavior from Skia's Android font manager, not a SkiaSharp wrapping bug.

**Recommendations:** **needs-info** — Issue has code snippet and screenshots but no minimal reproducible project, no confirmation the font otherwise renders correctly, and no information about the exact string returned. More details are needed to determine if this is Skia by-design Android behavior or a real wrapping bug.

---

## Classification

| Field | Value |
|-------|-------|
| Type | type/bug |
| Area | area/SkiaSharp |
| Platforms | os/Android |
| Backends | — |
| Tenets | tenet/compatibility |
| Partner | — |

## Evidence

### Reproduction

1. Call SKFontManager.Default.MatchCharacter('中') on Android
2. Access the .FamilyName property on the returned SKTypeface
3. Observe the FamilyName is a non-English string unlike Windows

**Environment:** Android 9, SkiaSharp 2.88.2, Visual Studio

**Repository links:**
- https://user-images.githubusercontent.com/17793881/190309365-a264e3c2-5611-44ef-9fac-dbb535762615.png — Screenshot
- https://user-images.githubusercontent.com/17793881/190309379-00c05086-27b5-474f-8f35-ab91eaeb8c48.png — Screenshot

**Code snippets:**

```csharp
var defaultChineseFont = SKFontManager.Default.MatchCharacter('中');
var defaultEnglishFont = SKFontManager.Default.MatchCharacter('A');
```

### Bug Signals

| Field | Value |
|-------|-------|
| Severity | low |
| Regression claimed | False |
| Error type | wrong-output |
| Error message | FamilyName returns strange/non-English name on Android |
| Repro quality | partial |
| Target frameworks | net6.0-android |

### Version Analysis

| Field | Value |
|-------|-------|
| Mentioned versions | 2.88.2 |
| Worked in | — |
| Broke in | — |
| Current relevance | likely |
| Relevance reason | The FamilyName retrieval code path (sk_typeface_get_family_name) has not changed materially and Android font metadata handling in Skia remains platform-specific. |

## Analysis

### Technical Summary

SKFontManager.MatchCharacter delegates to Skia's sk_fontmgr_match_family_style_character, and SKTypeface.FamilyName calls sk_typeface_get_family_name. On Android, the matched typeface for CJK characters is a system font (e.g., NotoSansCJK) whose Skia-reported FamilyName may be its localized/internal name rather than an English name. This is likely an Android-platform-specific behavior from Skia's Android font manager, not a SkiaSharp wrapping bug.

### Rationale

The MatchCharacter API itself is correctly implemented; the issue is that the returned FamilyName on Android reflects the system font's native metadata (which may be in Chinese/localized form). This could be by-design in Skia's Android font backend, but constitutes unexpected wrong-output behavior from a SkiaSharp consumer's perspective. Classification as bug is appropriate given the inconsistent cross-platform behavior of the same API. No reproduction project was provided so needs-info is appropriate.

### Key Signals

- "i find it return a SkTypeface that have strange FamilyName at Android" — **issue body** (The FamilyName property returns a non-English/localized string on Android for the matched typeface.)
- "Android: [screenshot showing non-English font name], Windows: [screenshot showing normal English name]" — **issue body** (Cross-platform inconsistency — same MatchCharacter call produces differently named typefaces on Android vs Windows.)

### Code Investigation

| File | Lines | Relevance | Finding |
|------|-------|-----------|---------|
| `binding/SkiaSharp/SKFontManager.cs` | 176-191 | direct | MatchCharacter delegates to SkiaApi.sk_fontmgr_match_family_style_character — no FamilyName processing or normalization occurs at the SkiaSharp layer. The returned SKTypeface's name is whatever Skia's platform font manager reports. |
| `binding/SkiaSharp/SKTypeface.cs` | 103 | direct | FamilyName = (string)SKString.GetObject(SkiaApi.sk_typeface_get_family_name(Handle)) — passes through the Skia-reported name directly, no normalization or locale override. |

### Workarounds

- If you need a stable English identifier for font matching purposes, use the typeface's PostScript name instead of FamilyName.
- Enumerate SKFontManager.Default.FontFamilyCount / GetFamilyName(i) to build a cross-platform font-name mapping, or use a hardcoded fallback font name per platform.

### Next Questions

- Does the returned typeface render text correctly despite the non-English FamilyName?
- Is the FamilyName a localized name (e.g., 思源黑体), a file path, or something else entirely?
- Does this reproduce on current SkiaSharp 2.88.x or 3.x?
- What does SKTypeface.ToString() or PostScriptName return on Android for the same typeface?

### Resolution Proposals

**Hypothesis:** On Android, Skia's font manager returns system fonts whose reported FamilyName is the font's internal/localized name (often the Chinese/Japanese font family string). This is Skia-level behavior, not a SkiaSharp wrapping bug.

1. **Document platform-specific FamilyName behavior** — workaround, confidence 0.80 (80%), cost/xs, validated=untested
   - Add documentation noting that FamilyName may return localized/platform-native names on Android, and recommend using PostScriptName for cross-platform font identity.
2. **Investigate Skia Android font backend** — investigation, confidence 0.60 (60%), cost/m, validated=untested
   - Investigate if Skia's Android SkFontMgr_Android returns localized family names and whether a normalization pass is feasible/desirable at the C API shim level.

**Recommended proposal:** Document platform-specific FamilyName behavior

**Why:** The root cause is in Skia's Android font backend behavior. Documentation is the quickest actionable response; deeper investigation of Skia behavior can follow.

## Recommendations

### Actionability

| Field | Value |
|-------|-------|
| Suggested action | needs-info |
| Confidence | 0.78 (78%) |
| Reason | Issue has code snippet and screenshots but no minimal reproducible project, no confirmation the font otherwise renders correctly, and no information about the exact string returned. More details are needed to determine if this is Skia by-design Android behavior or a real wrapping bug. |
| Suggested repro platform | linux |

### Missing Info

- What is the exact FamilyName string returned on Android (the screenshot is hard to read)?
- Does the matched typeface render Chinese/English text correctly, or is rendering also broken?
- Can a minimal reproducible project be provided?
- Does the issue reproduce on newer SkiaSharp versions (2.88.8+, 3.x)?

### Automatable Actions

| Type | Risk | Confidence | Description | Details |
|------|------|------------|-------------|---------|
| update-labels | low | 0.90 (90%) | Apply bug, SkiaSharp core, and Android labels | labels=type/bug, area/SkiaSharp, os/Android, tenet/compatibility |
| add-comment | medium | 0.78 (78%) | Request more details and provide initial analysis and workaround | — |

**Comment draft for `add-comment`:**

```markdown
Thanks for the report! On Android, Skia's font manager may return system fonts whose `FamilyName` is their internal/localized name (for example, a CJK font matched for `'中'` might report its name in Chinese characters rather than an English transliteration). This appears to be platform-specific behavior from Skia's Android font backend rather than a SkiaSharp wrapping issue.

Could you help us investigate further?
1. What is the exact string value of `FamilyName` on Android? (The screenshot resolution makes it hard to read.)
2. Does the matched typeface **render** text correctly, or is rendering also affected?
3. Does this reproduce on a newer version of SkiaSharp (2.88.8 or 3.x)?
4. Can you share a minimal reproducible project?

**Workaround:** If you need a stable cross-platform identifier for font selection, try using the `SKTypeface.FamilyName` result from `SKFontManager.Default.GetFamilyName(i)` enumeration to build a platform-aware font map, or use a hardcoded fallback font name per platform.
```

<details>
<summary>Raw JSON</summary>

```json
{
  "meta": {
    "schemaVersion": "1.0",
    "number": 2253,
    "repo": "mono/SkiaSharp",
    "analyzedAt": "2026-05-02T11:45:37Z"
  },
  "summary": "On Android, SKFontManager.Default.MatchCharacter returns a SKTypeface with a non-English or locale-encoded FamilyName, while the same call on Windows returns a typeface with a normal English family name.",
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
      "os/Android"
    ],
    "tenets": [
      "tenet/compatibility"
    ]
  },
  "evidence": {
    "bugSignals": {
      "severity": "low",
      "regressionClaimed": false,
      "errorType": "wrong-output",
      "errorMessage": "FamilyName returns strange/non-English name on Android",
      "reproQuality": "partial",
      "targetFrameworks": [
        "net6.0-android"
      ]
    },
    "reproEvidence": {
      "stepsToReproduce": [
        "Call SKFontManager.Default.MatchCharacter('中') on Android",
        "Access the .FamilyName property on the returned SKTypeface",
        "Observe the FamilyName is a non-English string unlike Windows"
      ],
      "codeSnippets": [
        "var defaultChineseFont = SKFontManager.Default.MatchCharacter('中');\nvar defaultEnglishFont = SKFontManager.Default.MatchCharacter('A');"
      ],
      "environmentDetails": "Android 9, SkiaSharp 2.88.2, Visual Studio",
      "repoLinks": [
        {
          "url": "https://user-images.githubusercontent.com/17793881/190309365-a264e3c2-5611-44ef-9fac-dbb535762615.png",
          "description": "Screenshot"
        },
        {
          "url": "https://user-images.githubusercontent.com/17793881/190309379-00c05086-27b5-474f-8f35-ab91eaeb8c48.png",
          "description": "Screenshot"
        }
      ]
    },
    "versionAnalysis": {
      "mentionedVersions": [
        "2.88.2"
      ],
      "currentRelevance": "likely",
      "relevanceReason": "The FamilyName retrieval code path (sk_typeface_get_family_name) has not changed materially and Android font metadata handling in Skia remains platform-specific."
    }
  },
  "analysis": {
    "summary": "SKFontManager.MatchCharacter delegates to Skia's sk_fontmgr_match_family_style_character, and SKTypeface.FamilyName calls sk_typeface_get_family_name. On Android, the matched typeface for CJK characters is a system font (e.g., NotoSansCJK) whose Skia-reported FamilyName may be its localized/internal name rather than an English name. This is likely an Android-platform-specific behavior from Skia's Android font manager, not a SkiaSharp wrapping bug.",
    "rationale": "The MatchCharacter API itself is correctly implemented; the issue is that the returned FamilyName on Android reflects the system font's native metadata (which may be in Chinese/localized form). This could be by-design in Skia's Android font backend, but constitutes unexpected wrong-output behavior from a SkiaSharp consumer's perspective. Classification as bug is appropriate given the inconsistent cross-platform behavior of the same API. No reproduction project was provided so needs-info is appropriate.",
    "keySignals": [
      {
        "text": "i find it return a SkTypeface that have strange FamilyName at Android",
        "source": "issue body",
        "interpretation": "The FamilyName property returns a non-English/localized string on Android for the matched typeface."
      },
      {
        "text": "Android: [screenshot showing non-English font name], Windows: [screenshot showing normal English name]",
        "source": "issue body",
        "interpretation": "Cross-platform inconsistency — same MatchCharacter call produces differently named typefaces on Android vs Windows."
      }
    ],
    "codeInvestigation": [
      {
        "file": "binding/SkiaSharp/SKFontManager.cs",
        "lines": "176-191",
        "finding": "MatchCharacter delegates to SkiaApi.sk_fontmgr_match_family_style_character — no FamilyName processing or normalization occurs at the SkiaSharp layer. The returned SKTypeface's name is whatever Skia's platform font manager reports.",
        "relevance": "direct"
      },
      {
        "file": "binding/SkiaSharp/SKTypeface.cs",
        "lines": "103",
        "finding": "FamilyName = (string)SKString.GetObject(SkiaApi.sk_typeface_get_family_name(Handle)) — passes through the Skia-reported name directly, no normalization or locale override.",
        "relevance": "direct"
      }
    ],
    "workarounds": [
      "If you need a stable English identifier for font matching purposes, use the typeface's PostScript name instead of FamilyName.",
      "Enumerate SKFontManager.Default.FontFamilyCount / GetFamilyName(i) to build a cross-platform font-name mapping, or use a hardcoded fallback font name per platform."
    ],
    "nextQuestions": [
      "Does the returned typeface render text correctly despite the non-English FamilyName?",
      "Is the FamilyName a localized name (e.g., 思源黑体), a file path, or something else entirely?",
      "Does this reproduce on current SkiaSharp 2.88.x or 3.x?",
      "What does SKTypeface.ToString() or PostScriptName return on Android for the same typeface?"
    ],
    "resolution": {
      "hypothesis": "On Android, Skia's font manager returns system fonts whose reported FamilyName is the font's internal/localized name (often the Chinese/Japanese font family string). This is Skia-level behavior, not a SkiaSharp wrapping bug.",
      "proposals": [
        {
          "title": "Document platform-specific FamilyName behavior",
          "description": "Add documentation noting that FamilyName may return localized/platform-native names on Android, and recommend using PostScriptName for cross-platform font identity.",
          "category": "workaround",
          "confidence": 0.8,
          "effort": "cost/xs",
          "validated": "untested"
        },
        {
          "title": "Investigate Skia Android font backend",
          "description": "Investigate if Skia's Android SkFontMgr_Android returns localized family names and whether a normalization pass is feasible/desirable at the C API shim level.",
          "category": "investigation",
          "confidence": 0.6,
          "effort": "cost/m",
          "validated": "untested"
        }
      ],
      "recommendedProposal": "Document platform-specific FamilyName behavior",
      "recommendedReason": "The root cause is in Skia's Android font backend behavior. Documentation is the quickest actionable response; deeper investigation of Skia behavior can follow."
    }
  },
  "output": {
    "actionability": {
      "suggestedAction": "needs-info",
      "confidence": 0.78,
      "reason": "Issue has code snippet and screenshots but no minimal reproducible project, no confirmation the font otherwise renders correctly, and no information about the exact string returned. More details are needed to determine if this is Skia by-design Android behavior or a real wrapping bug.",
      "suggestedReproPlatform": "linux"
    },
    "missingInfo": [
      "What is the exact FamilyName string returned on Android (the screenshot is hard to read)?",
      "Does the matched typeface render Chinese/English text correctly, or is rendering also broken?",
      "Can a minimal reproducible project be provided?",
      "Does the issue reproduce on newer SkiaSharp versions (2.88.8+, 3.x)?"
    ],
    "actions": [
      {
        "type": "update-labels",
        "description": "Apply bug, SkiaSharp core, and Android labels",
        "risk": "low",
        "confidence": 0.9,
        "labels": [
          "type/bug",
          "area/SkiaSharp",
          "os/Android",
          "tenet/compatibility"
        ]
      },
      {
        "type": "add-comment",
        "description": "Request more details and provide initial analysis and workaround",
        "risk": "medium",
        "confidence": 0.78,
        "comment": "Thanks for the report! On Android, Skia's font manager may return system fonts whose `FamilyName` is their internal/localized name (for example, a CJK font matched for `'中'` might report its name in Chinese characters rather than an English transliteration). This appears to be platform-specific behavior from Skia's Android font backend rather than a SkiaSharp wrapping issue.\n\nCould you help us investigate further?\n1. What is the exact string value of `FamilyName` on Android? (The screenshot resolution makes it hard to read.)\n2. Does the matched typeface **render** text correctly, or is rendering also affected?\n3. Does this reproduce on a newer version of SkiaSharp (2.88.8 or 3.x)?\n4. Can you share a minimal reproducible project?\n\n**Workaround:** If you need a stable cross-platform identifier for font selection, try using the `SKTypeface.FamilyName` result from `SKFontManager.Default.GetFamilyName(i)` enumeration to build a platform-aware font map, or use a hardcoded fallback font name per platform."
      }
    ]
  }
}
```

</details>
