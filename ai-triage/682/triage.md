# Issue Triage Report — #682

| Field | Value |
|-------|-------|
| Repository | mono/SkiaSharp |
| Analyzed | 2026-04-22T02:51:40Z |
| Type | type/enhancement (0.88 (88%)) |
| Area | area/SkiaSharp (0.95 (95%)) |
| Suggested action | keep-open (0.82 (82%)) |

**Issue Summary:** SKTypeface.FromFamilyName("Arial Black") returns Segoe UI on Windows because Skia uses DirectWrite, which groups 'Arial Black' as a weight variant under the 'Arial' typographic family rather than a separate family as GDI+ does.

**Analysis:** Skia's Windows font backend uses DirectWrite, which follows OpenType typographic family conventions. Under DirectWrite, 'Arial Black' is not an independent family — it is the Black weight variant of the 'Arial' family. GDI+ (System.Drawing) exposes it as a separate named family for legacy compatibility. FromFamilyName('Arial Black') fails because Skia asks DirectWrite for a family named 'Arial Black', which does not exist, so it falls back to the default (Segoe UI). The fix would be a compatibility shim in SkiaSharp that parses known GDI+ family name patterns (e.g., 'Arial Black' → family='Arial', weight=Black) before passing to the native API, or a new FromName helper that enumerates fonts by their GDI+ names.

**Recommendations:** **keep-open** — Valid enhancement request with community support. Maintainer acknowledged the GDI+ compat gap in 2018. A workaround exists but multiple users want a first-class API. No implementation decision has been made. Keeping open for design discussion.

---

## Classification

| Field | Value |
|-------|-------|
| Type | type/enhancement |
| Area | area/SkiaSharp |
| Platforms | os/Windows-Classic, os/Windows-Universal-UWP |
| Backends | — |
| Tenets | tenet/compatibility |
| Partner | — |
| Current labels | type/enhancement, type/feature-request, os/Windows-Classic, os/Windows-Universal-UWP, area/SkiaSharp |

## Evidence

### Reproduction

1. On Windows 10 (.NET Framework 4.7.2), call SKTypeface.FromFamilyName("Arial Black", SKFontStyleWeight.Bold, SKFontStyleWidth.Normal, SKFontStyleSlant.Upright)
2. Inspect the FamilyName of the returned SKTypeface
3. Observe it returns 'Segoe UI' instead of 'Arial Black'

**Environment:** SkiaSharp 1.60.3, Windows 10 / Windows Server 2016, .NET Framework 4.7.2

**Repository links:**
- https://github.com/mono/SkiaSharp/issues/682#issuecomment-438053141 — Reporter workaround: map 'Arial Black' to ('Arial', SKFontStyleWeight.Black)
- https://github.com/mono/SkiaSharp/issues/682#issuecomment-441464324 — Maintainer comment: technically Skia is correct but GDI+ compat issue needs investigation
- https://github.com/mono/SkiaSharp/issues/682#issuecomment-446129274 — User request for SKTypeface.FromName API
- https://github.com/mono/SkiaSharp/issues/682#issuecomment-1695762942 — Community-provided comprehensive lookup dictionary for Windows font names
- https://github.com/mono/SkiaSharp/issues/682#issuecomment-2342975483 — Advanced workaround using SKFontManager.FontFamilies + OpenType name/STAT table parsing

**Code snippets:**

```csharp
SKTypeface tf = SKTypeface.FromFamilyName("Arial Black", SKFontStyleWeight.Bold, SKFontStyleWidth.Normal, SKFontStyleSlant.Upright);
```

### Version Analysis

| Field | Value |
|-------|-------|
| Mentioned versions | 1.60.3 |
| Worked in | — |
| Broke in | — |
| Current relevance | likely |
| Relevance reason | FromFamilyName delegates directly to sk_typeface_create_from_name which calls into Skia's native font matching; the DirectWrite typographic family grouping behavior has not changed. |

## Analysis

### Technical Summary

Skia's Windows font backend uses DirectWrite, which follows OpenType typographic family conventions. Under DirectWrite, 'Arial Black' is not an independent family — it is the Black weight variant of the 'Arial' family. GDI+ (System.Drawing) exposes it as a separate named family for legacy compatibility. FromFamilyName('Arial Black') fails because Skia asks DirectWrite for a family named 'Arial Black', which does not exist, so it falls back to the default (Segoe UI). The fix would be a compatibility shim in SkiaSharp that parses known GDI+ family name patterns (e.g., 'Arial Black' → family='Arial', weight=Black) before passing to the native API, or a new FromName helper that enumerates fonts by their GDI+ names.

### Rationale

The behavior is technically correct at the Skia/DirectWrite level. The requested enhancement is a GDI+ compatibility layer in SkiaSharp's C# wrapper. Multiple users have confirmed the workaround (use 'Arial' + SKFontStyleWeight.Black) but want SkiaSharp to handle the mapping automatically. The maintainer acknowledged this in 2018 and left it open for community discussion. The issue is clearly scoped to SkiaSharp's C# API layer, not Skia itself.

### Key Signals

- "GDI+ handles font families differently — it uses different families for styles like narrow or black. DirectWrite or other frameworks just have one FontFamily 'Arial' and uses styles for all different combinations." — **comment by Gillibald (contributor)** (Confirms the root cause: DirectWrite typographic family model vs GDI+ RBIS family model.)
- "TECHNICALLY skia is right. But we are in the old world and it is unexpected. Google mentions that they have an extra layer over the font to read the prefix/suffix to read the weight and spacing." — **comment by mattleibow (maintainer)** (Maintainer confirms by-design behavior but acknowledges the UX problem. Enhancement is needed in SkiaSharp.)
- "My vote is to handle this in SkiaSharp so that the correct family and weight are chosen. Perhaps an additional API SKTypeface.FromName which can do this." — **comment by sansjunk** (Community request for a dedicated API.)

### Code Investigation

| File | Lines | Relevance | Finding |
|------|-------|-----------|---------|
| `binding/SkiaSharp/SKTypeface.cs` | 60-72 | direct | FromFamilyName(string, SKFontStyle) calls sk_typeface_create_from_name directly with the raw family name. No pre-processing or name-to-style mapping is performed. Passing 'Arial Black' goes straight to the native Skia/DirectWrite lookup. |
| `binding/SkiaSharp/SKFontManager.cs` | 43-72 | related | SKFontManager exposes FontFamilies enumeration and GetFontStyles(familyName). These APIs can be used to enumerate fonts and build a name-to-(family,style) mapping at the C# layer as a compatibility shim, which is the approach suggested by community members. |

### Workarounds

- Use SKTypeface.FromFamilyName("Arial", SKFontStyleWeight.Black, SKFontStyleWidth.Normal, SKFontStyleSlant.Upright) instead of "Arial Black"
- Build a lookup dictionary mapping legacy GDI+ font names to (family, weight, width) tuples using SKFontManager.FontFamilies + SKFontManager.GetFontStyles
- Use SKFontManager.FontFamilies + TryGetTableData('name'/'STAT') to build a full GDI+ name cache as described in comment #2342975483

### Next Questions

- Should a new SKTypeface.FromName() API be added that resolves GDI+ names to typographic family + style?
- Should a static helper/cache class be provided to build the mapping, or should FromFamilyName be patched to fall back to name-prefix parsing?
- Would this behavior also benefit non-Windows platforms (macOS CoreText, Linux fontconfig) that may have similar typographic family grouping?

### Resolution Proposals

**Hypothesis:** SkiaSharp needs a GDI+ compatibility layer that maps legacy family names containing weight/width keywords (e.g., 'Arial Black', 'Arial Narrow') to the correct typographic (family, weight, width) pair before calling the native font API.

1. **Use 'Arial' family name with Black weight** — workaround, confidence 0.95 (95%), cost/xs, validated=yes
   - Pass the typographic family name ('Arial') with the correct weight (SKFontStyleWeight.Black) to FromFamilyName. This is the standard DirectWrite approach.

```csharp
SKTypeface tf = SKTypeface.FromFamilyName("Arial", SKFontStyleWeight.Black, SKFontStyleWidth.Normal, SKFontStyleSlant.Upright);
```
2. **Add SKTypeface.FromName() with GDI+ name resolution** — fix, confidence 0.75 (75%), cost/l, validated=untested
   - Add a new public static method SKTypeface.FromName(string fullName) that enumerates all system fonts, reads their 'name' table entries, and matches by GDI+ family name. This would handle 'Arial Black', 'Arial Narrow', and similar legacy names transparently.

**Recommended proposal:** Use 'Arial' family name with Black weight

**Why:** The workaround is trivial to apply now and produces correct results. The long-term API addition is a design decision for maintainers.

## Recommendations

### Actionability

| Field | Value |
|-------|-------|
| Suggested action | keep-open |
| Confidence | 0.82 (82%) |
| Reason | Valid enhancement request with community support. Maintainer acknowledged the GDI+ compat gap in 2018. A workaround exists but multiple users want a first-class API. No implementation decision has been made. Keeping open for design discussion. |
| Suggested repro platform | windows |

### Automatable Actions

| Type | Risk | Confidence | Description | Details |
|------|------|------------|-------------|---------|
| update-labels | low | 0.90 (90%) | Correct labels: replace dual type labels with single type/enhancement, retain platform and area labels, add tenet/compatibility | labels=type/enhancement, area/SkiaSharp, os/Windows-Classic, os/Windows-Universal-UWP, tenet/compatibility |
| add-comment | medium | 0.88 (88%) | Post workaround and context explaining the DirectWrite vs GDI+ naming difference | — |

**Comment draft for `add-comment`:**

```markdown
Thanks for the detailed report!

This is a known difference between how **Skia/DirectWrite** and **GDI+** handle font families. Under DirectWrite (which Skia uses on Windows), *Arial Black* is not a separate font family — it is the **Black weight** of the *Arial* family. GDI+ exposes it as a distinct named family for legacy reasons.

**Workaround** — use the typographic family name with the Black weight:

```csharp
SKTypeface tf = SKTypeface.FromFamilyName(
    "Arial",
    SKFontStyleWeight.Black,
    SKFontStyleWidth.Normal,
    SKFontStyleSlant.Upright);
```

Similarly, `Arial Narrow` maps to `("Arial", weight=Normal, width=Condensed)`.

For a comprehensive lookup of all Windows font name mappings, see the dictionary posted in [this comment](https://github.com/mono/SkiaSharp/issues/682#issuecomment-1695762942).

We're tracking whether to add a `SKTypeface.FromName()` API that handles these GDI+ name patterns automatically. If you'd find that useful, please add a 👍 to keep the discussion moving.
```

<details>
<summary>Raw JSON</summary>

```json
{
  "meta": {
    "schemaVersion": "1.0",
    "number": 682,
    "repo": "mono/SkiaSharp",
    "analyzedAt": "2026-04-22T02:51:40Z",
    "currentLabels": [
      "type/enhancement",
      "type/feature-request",
      "os/Windows-Classic",
      "os/Windows-Universal-UWP",
      "area/SkiaSharp"
    ]
  },
  "summary": "SKTypeface.FromFamilyName(\"Arial Black\") returns Segoe UI on Windows because Skia uses DirectWrite, which groups 'Arial Black' as a weight variant under the 'Arial' typographic family rather than a separate family as GDI+ does.",
  "classification": {
    "type": {
      "value": "type/enhancement",
      "confidence": 0.88
    },
    "area": {
      "value": "area/SkiaSharp",
      "confidence": 0.95
    },
    "platforms": [
      "os/Windows-Classic",
      "os/Windows-Universal-UWP"
    ],
    "tenets": [
      "tenet/compatibility"
    ]
  },
  "evidence": {
    "reproEvidence": {
      "codeSnippets": [
        "SKTypeface tf = SKTypeface.FromFamilyName(\"Arial Black\", SKFontStyleWeight.Bold, SKFontStyleWidth.Normal, SKFontStyleSlant.Upright);"
      ],
      "stepsToReproduce": [
        "On Windows 10 (.NET Framework 4.7.2), call SKTypeface.FromFamilyName(\"Arial Black\", SKFontStyleWeight.Bold, SKFontStyleWidth.Normal, SKFontStyleSlant.Upright)",
        "Inspect the FamilyName of the returned SKTypeface",
        "Observe it returns 'Segoe UI' instead of 'Arial Black'"
      ],
      "environmentDetails": "SkiaSharp 1.60.3, Windows 10 / Windows Server 2016, .NET Framework 4.7.2",
      "repoLinks": [
        {
          "url": "https://github.com/mono/SkiaSharp/issues/682#issuecomment-438053141",
          "description": "Reporter workaround: map 'Arial Black' to ('Arial', SKFontStyleWeight.Black)"
        },
        {
          "url": "https://github.com/mono/SkiaSharp/issues/682#issuecomment-441464324",
          "description": "Maintainer comment: technically Skia is correct but GDI+ compat issue needs investigation"
        },
        {
          "url": "https://github.com/mono/SkiaSharp/issues/682#issuecomment-446129274",
          "description": "User request for SKTypeface.FromName API"
        },
        {
          "url": "https://github.com/mono/SkiaSharp/issues/682#issuecomment-1695762942",
          "description": "Community-provided comprehensive lookup dictionary for Windows font names"
        },
        {
          "url": "https://github.com/mono/SkiaSharp/issues/682#issuecomment-2342975483",
          "description": "Advanced workaround using SKFontManager.FontFamilies + OpenType name/STAT table parsing"
        }
      ]
    },
    "versionAnalysis": {
      "mentionedVersions": [
        "1.60.3"
      ],
      "currentRelevance": "likely",
      "relevanceReason": "FromFamilyName delegates directly to sk_typeface_create_from_name which calls into Skia's native font matching; the DirectWrite typographic family grouping behavior has not changed."
    }
  },
  "analysis": {
    "summary": "Skia's Windows font backend uses DirectWrite, which follows OpenType typographic family conventions. Under DirectWrite, 'Arial Black' is not an independent family — it is the Black weight variant of the 'Arial' family. GDI+ (System.Drawing) exposes it as a separate named family for legacy compatibility. FromFamilyName('Arial Black') fails because Skia asks DirectWrite for a family named 'Arial Black', which does not exist, so it falls back to the default (Segoe UI). The fix would be a compatibility shim in SkiaSharp that parses known GDI+ family name patterns (e.g., 'Arial Black' → family='Arial', weight=Black) before passing to the native API, or a new FromName helper that enumerates fonts by their GDI+ names.",
    "rationale": "The behavior is technically correct at the Skia/DirectWrite level. The requested enhancement is a GDI+ compatibility layer in SkiaSharp's C# wrapper. Multiple users have confirmed the workaround (use 'Arial' + SKFontStyleWeight.Black) but want SkiaSharp to handle the mapping automatically. The maintainer acknowledged this in 2018 and left it open for community discussion. The issue is clearly scoped to SkiaSharp's C# API layer, not Skia itself.",
    "keySignals": [
      {
        "text": "GDI+ handles font families differently — it uses different families for styles like narrow or black. DirectWrite or other frameworks just have one FontFamily 'Arial' and uses styles for all different combinations.",
        "source": "comment by Gillibald (contributor)",
        "interpretation": "Confirms the root cause: DirectWrite typographic family model vs GDI+ RBIS family model."
      },
      {
        "text": "TECHNICALLY skia is right. But we are in the old world and it is unexpected. Google mentions that they have an extra layer over the font to read the prefix/suffix to read the weight and spacing.",
        "source": "comment by mattleibow (maintainer)",
        "interpretation": "Maintainer confirms by-design behavior but acknowledges the UX problem. Enhancement is needed in SkiaSharp."
      },
      {
        "text": "My vote is to handle this in SkiaSharp so that the correct family and weight are chosen. Perhaps an additional API SKTypeface.FromName which can do this.",
        "source": "comment by sansjunk",
        "interpretation": "Community request for a dedicated API."
      }
    ],
    "codeInvestigation": [
      {
        "file": "binding/SkiaSharp/SKTypeface.cs",
        "lines": "60-72",
        "finding": "FromFamilyName(string, SKFontStyle) calls sk_typeface_create_from_name directly with the raw family name. No pre-processing or name-to-style mapping is performed. Passing 'Arial Black' goes straight to the native Skia/DirectWrite lookup.",
        "relevance": "direct"
      },
      {
        "file": "binding/SkiaSharp/SKFontManager.cs",
        "lines": "43-72",
        "finding": "SKFontManager exposes FontFamilies enumeration and GetFontStyles(familyName). These APIs can be used to enumerate fonts and build a name-to-(family,style) mapping at the C# layer as a compatibility shim, which is the approach suggested by community members.",
        "relevance": "related"
      }
    ],
    "workarounds": [
      "Use SKTypeface.FromFamilyName(\"Arial\", SKFontStyleWeight.Black, SKFontStyleWidth.Normal, SKFontStyleSlant.Upright) instead of \"Arial Black\"",
      "Build a lookup dictionary mapping legacy GDI+ font names to (family, weight, width) tuples using SKFontManager.FontFamilies + SKFontManager.GetFontStyles",
      "Use SKFontManager.FontFamilies + TryGetTableData('name'/'STAT') to build a full GDI+ name cache as described in comment #2342975483"
    ],
    "nextQuestions": [
      "Should a new SKTypeface.FromName() API be added that resolves GDI+ names to typographic family + style?",
      "Should a static helper/cache class be provided to build the mapping, or should FromFamilyName be patched to fall back to name-prefix parsing?",
      "Would this behavior also benefit non-Windows platforms (macOS CoreText, Linux fontconfig) that may have similar typographic family grouping?"
    ],
    "resolution": {
      "hypothesis": "SkiaSharp needs a GDI+ compatibility layer that maps legacy family names containing weight/width keywords (e.g., 'Arial Black', 'Arial Narrow') to the correct typographic (family, weight, width) pair before calling the native font API.",
      "proposals": [
        {
          "title": "Use 'Arial' family name with Black weight",
          "description": "Pass the typographic family name ('Arial') with the correct weight (SKFontStyleWeight.Black) to FromFamilyName. This is the standard DirectWrite approach.",
          "category": "workaround",
          "codeSnippet": "SKTypeface tf = SKTypeface.FromFamilyName(\"Arial\", SKFontStyleWeight.Black, SKFontStyleWidth.Normal, SKFontStyleSlant.Upright);",
          "confidence": 0.95,
          "effort": "cost/xs",
          "validated": "yes"
        },
        {
          "title": "Add SKTypeface.FromName() with GDI+ name resolution",
          "description": "Add a new public static method SKTypeface.FromName(string fullName) that enumerates all system fonts, reads their 'name' table entries, and matches by GDI+ family name. This would handle 'Arial Black', 'Arial Narrow', and similar legacy names transparently.",
          "category": "fix",
          "confidence": 0.75,
          "effort": "cost/l",
          "validated": "untested"
        }
      ],
      "recommendedProposal": "Use 'Arial' family name with Black weight",
      "recommendedReason": "The workaround is trivial to apply now and produces correct results. The long-term API addition is a design decision for maintainers."
    }
  },
  "output": {
    "actionability": {
      "suggestedAction": "keep-open",
      "confidence": 0.82,
      "reason": "Valid enhancement request with community support. Maintainer acknowledged the GDI+ compat gap in 2018. A workaround exists but multiple users want a first-class API. No implementation decision has been made. Keeping open for design discussion.",
      "suggestedReproPlatform": "windows"
    },
    "actions": [
      {
        "type": "update-labels",
        "description": "Correct labels: replace dual type labels with single type/enhancement, retain platform and area labels, add tenet/compatibility",
        "risk": "low",
        "confidence": 0.9,
        "labels": [
          "type/enhancement",
          "area/SkiaSharp",
          "os/Windows-Classic",
          "os/Windows-Universal-UWP",
          "tenet/compatibility"
        ]
      },
      {
        "type": "add-comment",
        "description": "Post workaround and context explaining the DirectWrite vs GDI+ naming difference",
        "risk": "medium",
        "confidence": 0.88,
        "comment": "Thanks for the detailed report!\n\nThis is a known difference between how **Skia/DirectWrite** and **GDI+** handle font families. Under DirectWrite (which Skia uses on Windows), *Arial Black* is not a separate font family — it is the **Black weight** of the *Arial* family. GDI+ exposes it as a distinct named family for legacy reasons.\n\n**Workaround** — use the typographic family name with the Black weight:\n\n```csharp\nSKTypeface tf = SKTypeface.FromFamilyName(\n    \"Arial\",\n    SKFontStyleWeight.Black,\n    SKFontStyleWidth.Normal,\n    SKFontStyleSlant.Upright);\n```\n\nSimilarly, `Arial Narrow` maps to `(\"Arial\", weight=Normal, width=Condensed)`.\n\nFor a comprehensive lookup of all Windows font name mappings, see the dictionary posted in [this comment](https://github.com/mono/SkiaSharp/issues/682#issuecomment-1695762942).\n\nWe're tracking whether to add a `SKTypeface.FromName()` API that handles these GDI+ name patterns automatically. If you'd find that useful, please add a 👍 to keep the discussion moving."
      }
    ]
  }
}
```

</details>
