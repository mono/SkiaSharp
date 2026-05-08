# Issue Triage Report — #3270

| Field | Value |
|-------|-------|
| Repository | mono/SkiaSharp |
| Analyzed | 2026-04-24T21:51:00Z |
| Type | type/bug (0.72 (72%)) |
| Area | area/SkiaSharp (0.92 (92%)) |
| Suggested action | close-as-not-a-bug (0.82 (82%)) |

**Issue Summary:** Reporter expects SKTypeface.OpenStream() to return a stream containing only the 'Bahnschrift Light SemiCondensed' variant bytes, but it returns the full variable font file — which is by-design because Bahnschrift is a single variable font file containing all styles.

**Analysis:** SKTypeface.OpenStream() returns the raw font file bytes via sk_typeface_open_stream. Bahnschrift on Windows is a variable font — a single .ttf file containing all style variations (weight, width, slant encoded as 'fvar' axes). There is no separate file for 'Bahnschrift Light SemiCondensed'. The style selection by FromFamilyName sets variation axis coordinates used at render time; it does not produce a separate subsetted font file. The behavior is correct by Skia design, but it conflicts with user expectations who want to embed a specific variant as a static font.

**Recommendations:** **close-as-not-a-bug** — SKTypeface.OpenStream() returning the full variable font file is correct Skia behavior. Bahnschrift is a single variable font file; there is no separate 'Light SemiCondensed' file. The variation coordinates are applied at render time, not as a separate file. This is a misunderstanding of variable font design.

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

1. On Windows 11, call SKTypeface.FromFamilyName("Bahnschrift", SKFontStyleWeight.Light, SKFontStyleWidth.SemiCondensed, SKFontStyleSlant.Upright)
2. Call typeface.OpenStream() on the result
3. Write the stream bytes to a .ttf file
4. Observe that the resulting file is the full Bahnschrift variable font, not a Light SemiCondensed-only file

**Environment:** Windows 11, Visual Studio 2022, .NET 8.0, SkiaSharp 3.116.1 (also reproduced in 3.119.0)

**Code snippets:**

```csharp
SKTypeface typeface = SKTypeface.FromFamilyName("Bahnschrift", SKFontStyleWeight.Light, SKFontStyleWidth.SemiCondensed, SKFontStyleSlant.Upright);
SKStream skFontStream = typeface.OpenStream();
```

### Bug Signals

| Field | Value |
|-------|-------|
| Severity | low |
| Regression claimed | False |
| Error type | wrong-output |
| Error message | OpenStream() returns the full Bahnschrift variable font instead of the specific Light SemiCondensed variant |
| Repro quality | complete |
| Target frameworks | net8.0 |

### Version Analysis

| Field | Value |
|-------|-------|
| Mentioned versions | 3.116.0, 3.116.1, 3.119.0 |
| Worked in | — |
| Broke in | — |
| Current relevance | likely |
| Relevance reason | OpenStream() delegates directly to sk_typeface_open_stream in native Skia, and the variable font handling in Skia has not changed. Comments confirm reproduction in 3.119.0. |

## Analysis

### Technical Summary

SKTypeface.OpenStream() returns the raw font file bytes via sk_typeface_open_stream. Bahnschrift on Windows is a variable font — a single .ttf file containing all style variations (weight, width, slant encoded as 'fvar' axes). There is no separate file for 'Bahnschrift Light SemiCondensed'. The style selection by FromFamilyName sets variation axis coordinates used at render time; it does not produce a separate subsetted font file. The behavior is correct by Skia design, but it conflicts with user expectations who want to embed a specific variant as a static font.

### Rationale

The behavior is technically by-design: Skia's openStream returns the underlying font file, which for a variable font is the single master file. However, the user expectation is understandable — many font families have separate files per style, and users assume a named style corresponds to a separate file. The issue is labeled type/bug and has 3 users reporting the same confusion, so it warrants a close-as-not-a-bug response with a thorough explanation and workaround options. Platform is Windows-only because Bahnschrift is a Windows system font.

### Key Signals

- "resulting font stream is Bahnschrift, which is a variable font containing multiple weights and widths, instead of the specific Bahnschrift Light SemiCondensed style" — **issue body** (Reporter expects a subsetted/instantiated static font file, but variable fonts are stored as a single master file.)
- "I am facing same problem in v3.119.0 too." — **comment #1 (MohanaselvamJ)** (Issue persists across versions, confirming it is not a regression but a consistent by-design behavior.)
- "Is there any suggestion to retrieve "Bahnschrift Light SemiCondensed" font stream from SKTypeFace?" — **comment #3 (RamarajMarimuthu)** (Multiple users asking the same question confirms this is a common usage confusion around variable fonts.)

### Code Investigation

| File | Lines | Relevance | Finding |
|------|-------|-----------|---------|
| `binding/SkiaSharp/SKTypeface.cs` | 290-300 | direct | OpenStream() calls sk_typeface_open_stream via P/Invoke, which returns the raw font data stream of the underlying font file. For variable fonts, this IS the full variable font file containing all style variations encoded in OpenType fvar/gvar/HVAR tables. No subsetting or variant-extraction logic exists. |
| `binding/SkiaSharp/SKTypeface.cs` | 50-77 | direct | FromFamilyName delegates to sk_typeface_create_from_name with an SKFontStyle. For variable fonts, Skia matches the best available font file and records the variation coordinates (e.g., wght=300, wdth=75 for Light SemiCondensed). These coordinates are used at render time, not to select a separate file. |
| `binding/SkiaSharp/SkiaApi.generated.cs` | 16493-16509 | related | sk_typeface_get_family_name and related C API wrappers confirm no sk_typeface_make_variant or font-instantiation API exists in the current SkiaSharp C binding layer. There is no API to extract a specific static instance from a variable font. |

### Workarounds

- Use the full variable font stream as-is: the typeface object DOES represent the correct Light SemiCondensed style at render time — the variation coordinates are embedded in the typeface metadata. If the goal is rendering, no action is needed.
- If embedding in a PDF/document requiring a static font, use the ttcIndex overload (OpenStream(out int ttcIndex)) and pass the index to a TTC-aware consumer. For a plain variable font (not TTC), ttcIndex will be 0.
- If a static (non-variable) font file is required, use a third-party font tool (e.g., fonttools/instfont, or the Windows Font Exporter) to instantiate a specific variation axis combination into a static OpenType font, then load that via SKTypeface.FromFile().
- Use SKTypeface.FromFile(@"C:\Windows\Fonts\Bahnschrift.ttf") to get the stream from the known path directly, then pair with SKFont variation axis overrides if the consumer supports variable font rendering.

### Next Questions

- What is the reporter's ultimate goal: embedding in PDF, passing to a third-party text engine, or something else? The answer determines the best workaround.
- Does Skia C++ expose SkTypeface::makeClone(const SkFontArguments&) in the C shim? If so, a future API to get a variation-coordinate-aware stream could be added.

### Resolution Proposals

**Hypothesis:** OpenStream() correctly returns the raw font file. For variable fonts, this is by design. The reporter's expectation of receiving a subsetted/instantiated variant is incorrect — no such variant file exists on disk.

1. **Explain variable font behavior and recommend direct use** — workaround, confidence 0.90 (90%), cost/xs, validated=untested
   - Explain that Bahnschrift is a variable font and OpenStream() returns the correct underlying file. If the goal is rendering with SkiaSharp, no special handling is needed — the typeface already has the correct variation coordinates. If embedding in another system, pass the full variable font + variation axes.
2. **Future enhancement: expose font variation axis instantiation** — fix, confidence 0.55 (55%), cost/l, validated=untested
   - Add an API to SkiaSharp that wraps SkTypeface::makeClone(SkFontArguments with variation coordinates) and allows callers to produce a 'locked-in' typeface whose OpenStream would carry variation coordinates in the stream (or expose variation axis values separately). This is a new feature, not a bug fix.

**Recommended proposal:** Explain variable font behavior and recommend direct use

**Why:** The behavior is by-design. A clear explanation with documentation of how variable fonts work in Skia will resolve the confusion for current users. A separate feature request for variation axis instantiation can be tracked independently.

## Recommendations

### Actionability

| Field | Value |
|-------|-------|
| Suggested action | close-as-not-a-bug |
| Confidence | 0.82 (82%) |
| Reason | SKTypeface.OpenStream() returning the full variable font file is correct Skia behavior. Bahnschrift is a single variable font file; there is no separate 'Light SemiCondensed' file. The variation coordinates are applied at render time, not as a separate file. This is a misunderstanding of variable font design. |
| Suggested repro platform | windows |

### Automatable Actions

| Type | Risk | Confidence | Description | Details |
|------|------|------------|-------------|---------|
| update-labels | low | 0.90 (90%) | Apply type/bug, area/SkiaSharp, os/Windows labels | labels=type/bug, area/SkiaSharp, os/Windows-Classic, tenet/compatibility |
| add-comment | high | 0.82 (82%) | Post explanation of variable font behavior with workarounds | — |
| close-issue | medium | 0.82 (82%) | Close as not a bug — behavior is by-design for variable fonts | stateReason=not_planned |

**Comment draft for `add-comment`:**

```markdown
Thank you for the detailed report, and apologies for the delay.

**This is by-design behavior in Skia.** Bahnschrift on Windows is a [variable font](https://docs.microsoft.com/typography/opentype/spec/otvaroverview) — a single `.ttf` file that encodes all style variations (weight, width, slant) as OpenType variation axes (`wght`, `wdth`). There is no separate file for "Bahnschrift Light SemiCondensed" on disk.

`SKTypeface.FromFamilyName("Bahnschrift", Light, SemiCondensed, Upright)` correctly creates a typeface with variation coordinates `wght=300, wdth=75`. At render time, Skia applies these axes. `OpenStream()` returns the underlying font file bytes — which is the full variable font, because that's what Skia has.

**What to do instead:**

1. **If your goal is rendering text with SkiaSharp:** No change needed. The `SKTypeface` you have already has the correct style. Use it directly with `SKFont`/`SKPaint` and your text will render as Light SemiCondensed.

2. **If you need to embed the font in a PDF or pass it to an external text engine that supports variable fonts:** Use the stream as-is (it IS the correct font), and configure the variation axes (`wght=300`, `wdth=75`) in your target system.

3. **If the target system requires a static (non-variable) font file:** You'll need a font tool such as [fonttools](https://github.com/fonttools/fonttools) (`python -m fonttools instancer Bahnschrift.ttf -o BahnschriftLightSemiCondensed.ttf wght=300 wdth=75`) to instantiate a specific axis combination into a static font. This is outside the scope of SkiaSharp.

If you have a use case that requires SkiaSharp to expose variable font instantiation directly, please open a separate feature request describing your scenario.
```

<details>
<summary>Raw JSON</summary>

```json
{
  "meta": {
    "schemaVersion": "1.0",
    "number": 3270,
    "repo": "mono/SkiaSharp",
    "analyzedAt": "2026-04-24T21:51:00Z",
    "currentLabels": [
      "type/bug"
    ]
  },
  "summary": "Reporter expects SKTypeface.OpenStream() to return a stream containing only the 'Bahnschrift Light SemiCondensed' variant bytes, but it returns the full variable font file — which is by-design because Bahnschrift is a single variable font file containing all styles.",
  "classification": {
    "type": {
      "value": "type/bug",
      "confidence": 0.72
    },
    "area": {
      "value": "area/SkiaSharp",
      "confidence": 0.92
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
      "severity": "low",
      "regressionClaimed": false,
      "errorType": "wrong-output",
      "errorMessage": "OpenStream() returns the full Bahnschrift variable font instead of the specific Light SemiCondensed variant",
      "reproQuality": "complete",
      "targetFrameworks": [
        "net8.0"
      ]
    },
    "reproEvidence": {
      "stepsToReproduce": [
        "On Windows 11, call SKTypeface.FromFamilyName(\"Bahnschrift\", SKFontStyleWeight.Light, SKFontStyleWidth.SemiCondensed, SKFontStyleSlant.Upright)",
        "Call typeface.OpenStream() on the result",
        "Write the stream bytes to a .ttf file",
        "Observe that the resulting file is the full Bahnschrift variable font, not a Light SemiCondensed-only file"
      ],
      "codeSnippets": [
        "SKTypeface typeface = SKTypeface.FromFamilyName(\"Bahnschrift\", SKFontStyleWeight.Light, SKFontStyleWidth.SemiCondensed, SKFontStyleSlant.Upright);\nSKStream skFontStream = typeface.OpenStream();"
      ],
      "environmentDetails": "Windows 11, Visual Studio 2022, .NET 8.0, SkiaSharp 3.116.1 (also reproduced in 3.119.0)"
    },
    "versionAnalysis": {
      "mentionedVersions": [
        "3.116.0",
        "3.116.1",
        "3.119.0"
      ],
      "currentRelevance": "likely",
      "relevanceReason": "OpenStream() delegates directly to sk_typeface_open_stream in native Skia, and the variable font handling in Skia has not changed. Comments confirm reproduction in 3.119.0."
    }
  },
  "analysis": {
    "summary": "SKTypeface.OpenStream() returns the raw font file bytes via sk_typeface_open_stream. Bahnschrift on Windows is a variable font — a single .ttf file containing all style variations (weight, width, slant encoded as 'fvar' axes). There is no separate file for 'Bahnschrift Light SemiCondensed'. The style selection by FromFamilyName sets variation axis coordinates used at render time; it does not produce a separate subsetted font file. The behavior is correct by Skia design, but it conflicts with user expectations who want to embed a specific variant as a static font.",
    "rationale": "The behavior is technically by-design: Skia's openStream returns the underlying font file, which for a variable font is the single master file. However, the user expectation is understandable — many font families have separate files per style, and users assume a named style corresponds to a separate file. The issue is labeled type/bug and has 3 users reporting the same confusion, so it warrants a close-as-not-a-bug response with a thorough explanation and workaround options. Platform is Windows-only because Bahnschrift is a Windows system font.",
    "codeInvestigation": [
      {
        "file": "binding/SkiaSharp/SKTypeface.cs",
        "lines": "290-300",
        "finding": "OpenStream() calls sk_typeface_open_stream via P/Invoke, which returns the raw font data stream of the underlying font file. For variable fonts, this IS the full variable font file containing all style variations encoded in OpenType fvar/gvar/HVAR tables. No subsetting or variant-extraction logic exists.",
        "relevance": "direct"
      },
      {
        "file": "binding/SkiaSharp/SKTypeface.cs",
        "lines": "50-77",
        "finding": "FromFamilyName delegates to sk_typeface_create_from_name with an SKFontStyle. For variable fonts, Skia matches the best available font file and records the variation coordinates (e.g., wght=300, wdth=75 for Light SemiCondensed). These coordinates are used at render time, not to select a separate file.",
        "relevance": "direct"
      },
      {
        "file": "binding/SkiaSharp/SkiaApi.generated.cs",
        "lines": "16493-16509",
        "finding": "sk_typeface_get_family_name and related C API wrappers confirm no sk_typeface_make_variant or font-instantiation API exists in the current SkiaSharp C binding layer. There is no API to extract a specific static instance from a variable font.",
        "relevance": "related"
      }
    ],
    "keySignals": [
      {
        "text": "resulting font stream is Bahnschrift, which is a variable font containing multiple weights and widths, instead of the specific Bahnschrift Light SemiCondensed style",
        "source": "issue body",
        "interpretation": "Reporter expects a subsetted/instantiated static font file, but variable fonts are stored as a single master file."
      },
      {
        "text": "I am facing same problem in v3.119.0 too.",
        "source": "comment #1 (MohanaselvamJ)",
        "interpretation": "Issue persists across versions, confirming it is not a regression but a consistent by-design behavior."
      },
      {
        "text": "Is there any suggestion to retrieve \"Bahnschrift Light SemiCondensed\" font stream from SKTypeFace?",
        "source": "comment #3 (RamarajMarimuthu)",
        "interpretation": "Multiple users asking the same question confirms this is a common usage confusion around variable fonts."
      }
    ],
    "workarounds": [
      "Use the full variable font stream as-is: the typeface object DOES represent the correct Light SemiCondensed style at render time — the variation coordinates are embedded in the typeface metadata. If the goal is rendering, no action is needed.",
      "If embedding in a PDF/document requiring a static font, use the ttcIndex overload (OpenStream(out int ttcIndex)) and pass the index to a TTC-aware consumer. For a plain variable font (not TTC), ttcIndex will be 0.",
      "If a static (non-variable) font file is required, use a third-party font tool (e.g., fonttools/instfont, or the Windows Font Exporter) to instantiate a specific variation axis combination into a static OpenType font, then load that via SKTypeface.FromFile().",
      "Use SKTypeface.FromFile(@\"C:\\Windows\\Fonts\\Bahnschrift.ttf\") to get the stream from the known path directly, then pair with SKFont variation axis overrides if the consumer supports variable font rendering."
    ],
    "nextQuestions": [
      "What is the reporter's ultimate goal: embedding in PDF, passing to a third-party text engine, or something else? The answer determines the best workaround.",
      "Does Skia C++ expose SkTypeface::makeClone(const SkFontArguments&) in the C shim? If so, a future API to get a variation-coordinate-aware stream could be added."
    ],
    "resolution": {
      "hypothesis": "OpenStream() correctly returns the raw font file. For variable fonts, this is by design. The reporter's expectation of receiving a subsetted/instantiated variant is incorrect — no such variant file exists on disk.",
      "proposals": [
        {
          "title": "Explain variable font behavior and recommend direct use",
          "description": "Explain that Bahnschrift is a variable font and OpenStream() returns the correct underlying file. If the goal is rendering with SkiaSharp, no special handling is needed — the typeface already has the correct variation coordinates. If embedding in another system, pass the full variable font + variation axes.",
          "category": "workaround",
          "confidence": 0.9,
          "effort": "cost/xs",
          "validated": "untested"
        },
        {
          "title": "Future enhancement: expose font variation axis instantiation",
          "description": "Add an API to SkiaSharp that wraps SkTypeface::makeClone(SkFontArguments with variation coordinates) and allows callers to produce a 'locked-in' typeface whose OpenStream would carry variation coordinates in the stream (or expose variation axis values separately). This is a new feature, not a bug fix.",
          "category": "fix",
          "confidence": 0.55,
          "effort": "cost/l",
          "validated": "untested"
        }
      ],
      "recommendedProposal": "Explain variable font behavior and recommend direct use",
      "recommendedReason": "The behavior is by-design. A clear explanation with documentation of how variable fonts work in Skia will resolve the confusion for current users. A separate feature request for variation axis instantiation can be tracked independently."
    }
  },
  "output": {
    "actionability": {
      "suggestedAction": "close-as-not-a-bug",
      "confidence": 0.82,
      "reason": "SKTypeface.OpenStream() returning the full variable font file is correct Skia behavior. Bahnschrift is a single variable font file; there is no separate 'Light SemiCondensed' file. The variation coordinates are applied at render time, not as a separate file. This is a misunderstanding of variable font design.",
      "suggestedReproPlatform": "windows"
    },
    "actions": [
      {
        "type": "update-labels",
        "description": "Apply type/bug, area/SkiaSharp, os/Windows labels",
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
        "description": "Post explanation of variable font behavior with workarounds",
        "risk": "high",
        "confidence": 0.82,
        "comment": "Thank you for the detailed report, and apologies for the delay.\n\n**This is by-design behavior in Skia.** Bahnschrift on Windows is a [variable font](https://docs.microsoft.com/typography/opentype/spec/otvaroverview) — a single `.ttf` file that encodes all style variations (weight, width, slant) as OpenType variation axes (`wght`, `wdth`). There is no separate file for \"Bahnschrift Light SemiCondensed\" on disk.\n\n`SKTypeface.FromFamilyName(\"Bahnschrift\", Light, SemiCondensed, Upright)` correctly creates a typeface with variation coordinates `wght=300, wdth=75`. At render time, Skia applies these axes. `OpenStream()` returns the underlying font file bytes — which is the full variable font, because that's what Skia has.\n\n**What to do instead:**\n\n1. **If your goal is rendering text with SkiaSharp:** No change needed. The `SKTypeface` you have already has the correct style. Use it directly with `SKFont`/`SKPaint` and your text will render as Light SemiCondensed.\n\n2. **If you need to embed the font in a PDF or pass it to an external text engine that supports variable fonts:** Use the stream as-is (it IS the correct font), and configure the variation axes (`wght=300`, `wdth=75`) in your target system.\n\n3. **If the target system requires a static (non-variable) font file:** You'll need a font tool such as [fonttools](https://github.com/fonttools/fonttools) (`python -m fonttools instancer Bahnschrift.ttf -o BahnschriftLightSemiCondensed.ttf wght=300 wdth=75`) to instantiate a specific axis combination into a static font. This is outside the scope of SkiaSharp.\n\nIf you have a use case that requires SkiaSharp to expose variable font instantiation directly, please open a separate feature request describing your scenario."
      },
      {
        "type": "close-issue",
        "description": "Close as not a bug — behavior is by-design for variable fonts",
        "risk": "medium",
        "confidence": 0.82,
        "stateReason": "not_planned"
      }
    ]
  }
}
```

</details>
