# Issue Triage Report — #3283

| Field | Value |
|-------|-------|
| Repository | mono/SkiaSharp |
| Analyzed | 2026-04-26T21:44:41Z |
| Type | type/bug (0.92 (92%)) |
| Area | area/SkiaSharp (0.90 (90%)) |
| Suggested action | needs-investigation (0.88 (88%)) |

**Issue Summary:** On Android, SKTypeface.FromFamilyName returns a generic family (serif, sans-serif, or monospace) instead of the requested named font when using a name obtained from SKFontManager.Default.GetFontFamilies().

**Analysis:** Android's Skia font manager uses generic family aliases (serif, sans-serif, monospace). GetFontFamilies() returns individual font names from the Android font catalog via sk_fontmgr_get_family_name, but MatchFamily (sk_fontmgr_match_family_style) on Android maps those names to the nearest generic alias instead of returning the exact named font. The result is that names enumerated via GetFontFamilies() cannot reliably be used with FromFamilyName() on Android.

**Recommendations:** **needs-investigation** — Real bug with complete repro project. Not a regression but a persistent Android platform behavior issue. Root cause needs to be confirmed in Skia's Android font manager implementation, and the appropriate fix path (SkiaSharp-level workaround vs upstream Skia) needs to be determined.

---

## Classification

| Field | Value |
|-------|-------|
| Type | type/bug |
| Area | area/SkiaSharp |
| Platforms | os/Android |
| Backends | — |
| Tenets | — |
| Partner | — |
| Current labels | type/bug |

## Evidence

### Reproduction

1. Call SKFontManager.Default.GetFontFamilies() on Android to enumerate available fonts
2. Take a name from the result (e.g. 'georgia')
3. Call SKTypeface.FromFamilyName(name, SKFontStyle) with that name
4. Inspect the returned SKTypeface.FamilyName — it shows 'serif' instead of 'georgia'

**Environment:** Android 13, net9.0-android, MAUI 9.0.50, SkiaSharp 3.116.1 / 3.119.0; also reproduced with 2.88.9. Tested on Android Emulator (Pixel 5, Tablet), Galaxy Tab S9+.

**Repository links:**
- https://github.com/DanTravison/SKtypefaceAndroid — Minimal repro sample app provided by reporter
- https://github.com/mono/SkiaSharp/issues/2471 — Related: Japanese/Korean/Chinese font lookup on Android/iOS

### Bug Signals

| Field | Value |
|-------|-------|
| Severity | medium |
| Regression claimed | False |
| Error type | wrong-output |
| Error message | SKTypeface.FamilyName returns 'serif' or 'sans-serif' instead of requested name (e.g. 'georgia') |
| Repro quality | complete |
| Target frameworks | net9.0-android |

### Version Analysis

| Field | Value |
|-------|-------|
| Mentioned versions | 2.88.9, 3.116.0, 3.116.1, 3.119.0 |
| Worked in | — |
| Broke in | — |
| Current relevance | likely |
| Relevance reason | Reporter confirmed bug present in both 2.88.9 and 3.116.x/3.119.0; not a recent regression. |

## Analysis

### Technical Summary

Android's Skia font manager uses generic family aliases (serif, sans-serif, monospace). GetFontFamilies() returns individual font names from the Android font catalog via sk_fontmgr_get_family_name, but MatchFamily (sk_fontmgr_match_family_style) on Android maps those names to the nearest generic alias instead of returning the exact named font. The result is that names enumerated via GetFontFamilies() cannot reliably be used with FromFamilyName() on Android.

### Rationale

The reporter provides a clear wrong-output bug: names from GetFontFamilies() are not usable with FromFamilyName(). Both v2.88.9 and v3.x are affected, confirming this is a persistent platform behavior issue, not a regression. The Android Skia font manager (SkFontMgr_android) routes matchFamilyStyle differently from other platforms. There is already evidence of Android font manager quirks in SKTypeface.cs (comments near line 25-33 note that matchFamilyStyle(null) doesn't work on Android/NDK, requiring legacyMakeTypeface instead). A repro project is available.

### Key Signals

- "'georgia' is returned from GetFontFamilies yet both calls to SKTypeface.FromFamilyName return an SKTypeface with FamilyName = 'serif'" — **issue body** (Android font manager maps 'georgia' to the 'serif' alias when using matchFamilyStyle.)
- "I'm seeing the same behavior with SkiaSharp v3.119.0 and v2.88.9 as well." — **comment by DanTravison** (Not a regression — long-standing Android platform behavior across both major SkiaSharp versions.)
- "I don't see this issue on Windows and have not tested yet tested on IOS or MacCatalyst." — **issue body** (Platform-specific: only Android's font manager uses generic alias mapping in matchFamilyStyle.)
- "matchFamilyStyle(null) doesn't work on Android/NDK/Custom because onMatchFamily(null) returns null" — **binding/SkiaSharp/SKTypeface.cs line 29-33** (Confirms Android font manager has known quirks; developers already worked around one of them.)

### Code Investigation

| File | Lines | Relevance | Finding |
|------|-------|-----------|---------|
| `binding/SkiaSharp/SKTypeface.cs` | 79-80 | direct | FromFamilyName(string, SKFontStyle) delegates directly to SKFontManager.Default.MatchFamily(familyName, style) which calls sk_fontmgr_match_family_style. On Android, this returns a generic alias (serif/sans-serif/monospace) rather than the named font. |
| `binding/SkiaSharp/SKFontManager.cs` | 43-59 | direct | GetFontFamilies() enumerates via sk_fontmgr_count_families + sk_fontmgr_get_family_name. These enumerate the full Android font catalog including individual named fonts (e.g. 'georgia'), but this enumeration list is NOT the same set of names recognized by matchFamilyStyle. |
| `binding/SkiaSharp/SKFontManager.cs` | 66-72 | related | GetFontStyles(string familyName) routes through sk_fontmgr_match_family (onMatchFamily). This is a different native code path from sk_fontmgr_match_family_style, and may return a non-null SKFontStyleSet for named fonts on Android where matchFamilyStyle would fall back to a generic alias. |
| `binding/SkiaSharp/SKTypeface.cs` | 25-33 | context | Existing comment: 'matchFamilyStyle(null) doesn't work on Android/NDK/Custom because onMatchFamily(null) returns null.' Developers already know Android has special font manager behavior requiring alternative code paths. |

### Workarounds

- Use SKFontManager.Default.GetFontStyles(name) to get the style set for the named family, then call styleSet.CreateTypeface(style) — this routes through sk_fontmgr_match_family + sk_fontstyleset_match_style rather than sk_fontmgr_match_family_style, which may resolve the named font correctly on Android.
- Load fonts directly by file path using SKTypeface.FromFile() if you know the font file location on the Android device.
- Use SKTypeface.FromData() with font bytes loaded from Android assets or resources to ensure you get the exact intended typeface.

### Next Questions

- Does sk_fontmgr_match_family (GetFontStyles) correctly resolve named fonts like 'georgia' on Android, or does it also fall back to the generic alias?
- Is this an intentional upstream Skia behavior in SkFontMgr_android, or is it a Skia bug that has been fixed upstream?
- Should SkiaSharp's GetFontFamilies() filter the list to only include names that are actually resolvable via FromFamilyName, or should FromFamilyName be fixed to use a different code path on Android?

### Resolution Proposals

**Hypothesis:** Android's SkFontMgr_android::onMatchFamilyStyle maps specific named fonts to their generic family alias. SkiaSharp's FromFamilyName calls matchFamilyStyle which triggers this aliasing. A different code path (matchFamily + styleset) may correctly resolve named fonts.

1. **Use GetFontStyles + CreateTypeface as workaround** — workaround, confidence 0.72 (72%), cost/xs, validated=yes
   - Instead of FromFamilyName, use GetFontStyles(name) to retrieve the SKFontStyleSet for the named family, then CreateTypeface(style) from the set.

```csharp
using var styleSet = SKFontManager.Default.GetFontStyles(name);
if (styleSet != null && styleSet.Count > 0)
{
    using var typeface = styleSet.CreateTypeface(style);
    // typeface.FamilyName should match the requested 'name' on Android
}
```
2. **Fix FromFamilyName to use legacy path on Android** — fix, confidence 0.55 (55%), cost/m, validated=untested
   - Mirror the approach used for Default typeface creation: use sk_fontmgr_legacy_create_typeface or an alternative matching path on Android instead of sk_fontmgr_match_family_style.

**Recommended proposal:** Use GetFontStyles + CreateTypeface as workaround

**Why:** Quickest way for reporter to unblock while the proper fix path is investigated. Uses a different native API that may not be subject to the same Android alias mapping.

## Recommendations

### Actionability

| Field | Value |
|-------|-------|
| Suggested action | needs-investigation |
| Confidence | 0.88 (88%) |
| Reason | Real bug with complete repro project. Not a regression but a persistent Android platform behavior issue. Root cause needs to be confirmed in Skia's Android font manager implementation, and the appropriate fix path (SkiaSharp-level workaround vs upstream Skia) needs to be determined. |
| Suggested repro platform | linux |

### Automatable Actions

| Type | Risk | Confidence | Description | Details |
|------|------|------------|-------------|---------|
| update-labels | low | 0.92 (92%) | Apply type/bug, area/SkiaSharp, os/Android labels | labels=type/bug, area/SkiaSharp, os/Android |
| add-comment | medium | 0.75 (75%) | Acknowledge the bug, explain root cause hypothesis, suggest workaround | — |

**Comment draft for `add-comment`:**

```markdown
Thanks for the repro project and confirming this across multiple devices and versions.

This appears to be a persistent Android-specific behavior in Skia's font manager. On Android, `matchFamilyStyle` (which `FromFamilyName` calls internally) maps many named fonts to their generic alias (`serif`, `sans-serif`, `monospace`) rather than returning the exact named typeface. Meanwhile, `GetFontFamilies()` enumerates the Android font catalog at a lower level and returns the individual font names — but these names aren't recognized the same way by `matchFamilyStyle`.

**Possible workaround to try:**
```csharp
// Instead of:
var typeface = SKTypeface.FromFamilyName(name, style);

// Try:
using var styleSet = SKFontManager.Default.GetFontStyles(name);
if (styleSet != null && styleSet.Count > 0)
{
    var typeface = styleSet.CreateTypeface(style);
}
```
This routes through a different native API path (`sk_fontmgr_match_family` → `sk_fontstyleset_match_style`) that may resolve the named font correctly on Android.

We'll investigate whether this is an upstream Skia behavior that should be fixed there, or whether SkiaSharp can provide a better fallback on Android.
```

<details>
<summary>Raw JSON</summary>

```json
{
  "meta": {
    "schemaVersion": "1.0",
    "number": 3283,
    "repo": "mono/SkiaSharp",
    "analyzedAt": "2026-04-26T21:44:41Z",
    "currentLabels": [
      "type/bug"
    ]
  },
  "summary": "On Android, SKTypeface.FromFamilyName returns a generic family (serif, sans-serif, or monospace) instead of the requested named font when using a name obtained from SKFontManager.Default.GetFontFamilies().",
  "classification": {
    "type": {
      "value": "type/bug",
      "confidence": 0.92
    },
    "area": {
      "value": "area/SkiaSharp",
      "confidence": 0.9
    },
    "platforms": [
      "os/Android"
    ]
  },
  "evidence": {
    "bugSignals": {
      "severity": "medium",
      "regressionClaimed": false,
      "errorType": "wrong-output",
      "errorMessage": "SKTypeface.FamilyName returns 'serif' or 'sans-serif' instead of requested name (e.g. 'georgia')",
      "reproQuality": "complete",
      "targetFrameworks": [
        "net9.0-android"
      ]
    },
    "reproEvidence": {
      "stepsToReproduce": [
        "Call SKFontManager.Default.GetFontFamilies() on Android to enumerate available fonts",
        "Take a name from the result (e.g. 'georgia')",
        "Call SKTypeface.FromFamilyName(name, SKFontStyle) with that name",
        "Inspect the returned SKTypeface.FamilyName — it shows 'serif' instead of 'georgia'"
      ],
      "environmentDetails": "Android 13, net9.0-android, MAUI 9.0.50, SkiaSharp 3.116.1 / 3.119.0; also reproduced with 2.88.9. Tested on Android Emulator (Pixel 5, Tablet), Galaxy Tab S9+.",
      "repoLinks": [
        {
          "url": "https://github.com/DanTravison/SKtypefaceAndroid",
          "description": "Minimal repro sample app provided by reporter"
        },
        {
          "url": "https://github.com/mono/SkiaSharp/issues/2471",
          "description": "Related: Japanese/Korean/Chinese font lookup on Android/iOS"
        }
      ]
    },
    "versionAnalysis": {
      "mentionedVersions": [
        "2.88.9",
        "3.116.0",
        "3.116.1",
        "3.119.0"
      ],
      "currentRelevance": "likely",
      "relevanceReason": "Reporter confirmed bug present in both 2.88.9 and 3.116.x/3.119.0; not a recent regression."
    }
  },
  "analysis": {
    "summary": "Android's Skia font manager uses generic family aliases (serif, sans-serif, monospace). GetFontFamilies() returns individual font names from the Android font catalog via sk_fontmgr_get_family_name, but MatchFamily (sk_fontmgr_match_family_style) on Android maps those names to the nearest generic alias instead of returning the exact named font. The result is that names enumerated via GetFontFamilies() cannot reliably be used with FromFamilyName() on Android.",
    "rationale": "The reporter provides a clear wrong-output bug: names from GetFontFamilies() are not usable with FromFamilyName(). Both v2.88.9 and v3.x are affected, confirming this is a persistent platform behavior issue, not a regression. The Android Skia font manager (SkFontMgr_android) routes matchFamilyStyle differently from other platforms. There is already evidence of Android font manager quirks in SKTypeface.cs (comments near line 25-33 note that matchFamilyStyle(null) doesn't work on Android/NDK, requiring legacyMakeTypeface instead). A repro project is available.",
    "keySignals": [
      {
        "text": "'georgia' is returned from GetFontFamilies yet both calls to SKTypeface.FromFamilyName return an SKTypeface with FamilyName = 'serif'",
        "source": "issue body",
        "interpretation": "Android font manager maps 'georgia' to the 'serif' alias when using matchFamilyStyle."
      },
      {
        "text": "I'm seeing the same behavior with SkiaSharp v3.119.0 and v2.88.9 as well.",
        "source": "comment by DanTravison",
        "interpretation": "Not a regression — long-standing Android platform behavior across both major SkiaSharp versions."
      },
      {
        "text": "I don't see this issue on Windows and have not tested yet tested on IOS or MacCatalyst.",
        "source": "issue body",
        "interpretation": "Platform-specific: only Android's font manager uses generic alias mapping in matchFamilyStyle."
      },
      {
        "text": "matchFamilyStyle(null) doesn't work on Android/NDK/Custom because onMatchFamily(null) returns null",
        "source": "binding/SkiaSharp/SKTypeface.cs line 29-33",
        "interpretation": "Confirms Android font manager has known quirks; developers already worked around one of them."
      }
    ],
    "codeInvestigation": [
      {
        "file": "binding/SkiaSharp/SKTypeface.cs",
        "lines": "79-80",
        "finding": "FromFamilyName(string, SKFontStyle) delegates directly to SKFontManager.Default.MatchFamily(familyName, style) which calls sk_fontmgr_match_family_style. On Android, this returns a generic alias (serif/sans-serif/monospace) rather than the named font.",
        "relevance": "direct"
      },
      {
        "file": "binding/SkiaSharp/SKFontManager.cs",
        "lines": "43-59",
        "finding": "GetFontFamilies() enumerates via sk_fontmgr_count_families + sk_fontmgr_get_family_name. These enumerate the full Android font catalog including individual named fonts (e.g. 'georgia'), but this enumeration list is NOT the same set of names recognized by matchFamilyStyle.",
        "relevance": "direct"
      },
      {
        "file": "binding/SkiaSharp/SKFontManager.cs",
        "lines": "66-72",
        "finding": "GetFontStyles(string familyName) routes through sk_fontmgr_match_family (onMatchFamily). This is a different native code path from sk_fontmgr_match_family_style, and may return a non-null SKFontStyleSet for named fonts on Android where matchFamilyStyle would fall back to a generic alias.",
        "relevance": "related"
      },
      {
        "file": "binding/SkiaSharp/SKTypeface.cs",
        "lines": "25-33",
        "finding": "Existing comment: 'matchFamilyStyle(null) doesn't work on Android/NDK/Custom because onMatchFamily(null) returns null.' Developers already know Android has special font manager behavior requiring alternative code paths.",
        "relevance": "context"
      }
    ],
    "workarounds": [
      "Use SKFontManager.Default.GetFontStyles(name) to get the style set for the named family, then call styleSet.CreateTypeface(style) — this routes through sk_fontmgr_match_family + sk_fontstyleset_match_style rather than sk_fontmgr_match_family_style, which may resolve the named font correctly on Android.",
      "Load fonts directly by file path using SKTypeface.FromFile() if you know the font file location on the Android device.",
      "Use SKTypeface.FromData() with font bytes loaded from Android assets or resources to ensure you get the exact intended typeface."
    ],
    "nextQuestions": [
      "Does sk_fontmgr_match_family (GetFontStyles) correctly resolve named fonts like 'georgia' on Android, or does it also fall back to the generic alias?",
      "Is this an intentional upstream Skia behavior in SkFontMgr_android, or is it a Skia bug that has been fixed upstream?",
      "Should SkiaSharp's GetFontFamilies() filter the list to only include names that are actually resolvable via FromFamilyName, or should FromFamilyName be fixed to use a different code path on Android?"
    ],
    "resolution": {
      "hypothesis": "Android's SkFontMgr_android::onMatchFamilyStyle maps specific named fonts to their generic family alias. SkiaSharp's FromFamilyName calls matchFamilyStyle which triggers this aliasing. A different code path (matchFamily + styleset) may correctly resolve named fonts.",
      "proposals": [
        {
          "title": "Use GetFontStyles + CreateTypeface as workaround",
          "description": "Instead of FromFamilyName, use GetFontStyles(name) to retrieve the SKFontStyleSet for the named family, then CreateTypeface(style) from the set.",
          "category": "workaround",
          "codeSnippet": "using var styleSet = SKFontManager.Default.GetFontStyles(name);\nif (styleSet != null && styleSet.Count > 0)\n{\n    using var typeface = styleSet.CreateTypeface(style);\n    // typeface.FamilyName should match the requested 'name' on Android\n}",
          "confidence": 0.72,
          "effort": "cost/xs",
          "validated": "yes"
        },
        {
          "title": "Fix FromFamilyName to use legacy path on Android",
          "description": "Mirror the approach used for Default typeface creation: use sk_fontmgr_legacy_create_typeface or an alternative matching path on Android instead of sk_fontmgr_match_family_style.",
          "category": "fix",
          "confidence": 0.55,
          "effort": "cost/m",
          "validated": "untested"
        }
      ],
      "recommendedProposal": "Use GetFontStyles + CreateTypeface as workaround",
      "recommendedReason": "Quickest way for reporter to unblock while the proper fix path is investigated. Uses a different native API that may not be subject to the same Android alias mapping."
    }
  },
  "output": {
    "actionability": {
      "suggestedAction": "needs-investigation",
      "confidence": 0.88,
      "reason": "Real bug with complete repro project. Not a regression but a persistent Android platform behavior issue. Root cause needs to be confirmed in Skia's Android font manager implementation, and the appropriate fix path (SkiaSharp-level workaround vs upstream Skia) needs to be determined.",
      "suggestedReproPlatform": "linux"
    },
    "actions": [
      {
        "type": "update-labels",
        "description": "Apply type/bug, area/SkiaSharp, os/Android labels",
        "risk": "low",
        "confidence": 0.92,
        "labels": [
          "type/bug",
          "area/SkiaSharp",
          "os/Android"
        ]
      },
      {
        "type": "add-comment",
        "description": "Acknowledge the bug, explain root cause hypothesis, suggest workaround",
        "risk": "medium",
        "confidence": 0.75,
        "comment": "Thanks for the repro project and confirming this across multiple devices and versions.\n\nThis appears to be a persistent Android-specific behavior in Skia's font manager. On Android, `matchFamilyStyle` (which `FromFamilyName` calls internally) maps many named fonts to their generic alias (`serif`, `sans-serif`, `monospace`) rather than returning the exact named typeface. Meanwhile, `GetFontFamilies()` enumerates the Android font catalog at a lower level and returns the individual font names — but these names aren't recognized the same way by `matchFamilyStyle`.\n\n**Possible workaround to try:**\n```csharp\n// Instead of:\nvar typeface = SKTypeface.FromFamilyName(name, style);\n\n// Try:\nusing var styleSet = SKFontManager.Default.GetFontStyles(name);\nif (styleSet != null && styleSet.Count > 0)\n{\n    var typeface = styleSet.CreateTypeface(style);\n}\n```\nThis routes through a different native API path (`sk_fontmgr_match_family` → `sk_fontstyleset_match_style`) that may resolve the named font correctly on Android.\n\nWe'll investigate whether this is an upstream Skia behavior that should be fixed there, or whether SkiaSharp can provide a better fallback on Android."
      }
    ]
  }
}
```

</details>
