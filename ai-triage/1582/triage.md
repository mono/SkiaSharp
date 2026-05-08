# Issue Triage Report — #1582

| Field | Value |
|-------|-------|
| Repository | mono/SkiaSharp |
| Analyzed | 2026-05-03T08:13:57Z |
| Type | type/bug (0.92 (92%)) |
| Area | area/SkiaSharp (0.90 (90%)) |
| Suggested action | needs-investigation (0.82 (82%)) |

**Issue Summary:** SKTypeface.FontWeight returns 400 (Normal) instead of the correct weight (200 for Extra-Light) when loading a font from a file on macOS, while Windows returns the correct value.

**Analysis:** On macOS, SKTypeface.FontWeight returns 400 (Normal) for fonts with non-Normal weights loaded via SKTypeface.FromFile(). The underlying cause is likely in Skia's macOS CoreText font backend, which may not correctly read the OS/2 table weight when constructing the font style for a file-loaded typeface. The SKTypeface.FontWeight property directly calls sk_typeface_get_font_weight, which delegates to the native Skia SkTypeface::fontStyle().weight() — a difference in macOS CoreText vs Windows DirectWrite behavior when inferring font weight from the font file.

**Recommendations:** **needs-investigation** — Real platform-specific bug with clear repro code and platform comparison. Needs macOS reproduction to confirm and trace the root cause in Skia's CoreText backend.

---

## Classification

| Field | Value |
|-------|-------|
| Type | type/bug |
| Area | area/SkiaSharp |
| Platforms | os/macOS |
| Backends | — |
| Tenets | — |
| Partner | — |

## Evidence

### Reproduction

1. Load a font file with a non-Normal weight (e.g., Metropolis Extra-Light OTF)
2. Call SKTypeface.FromFile() on both Windows and macOS
3. Read the FontWeight property
4. On Windows: returns 200 (correct); on macOS: returns 400 (incorrect)

**Environment:** macOS (version not specified), Windows (working correctly). SkiaSharp version not specified.

**Repository links:**
- https://github.com/mono/SkiaSharp/files/5844440/ConsoleApp1.zip — Minimal repro project (ConsoleApp1.zip) attached by reporter

### Bug Signals

| Field | Value |
|-------|-------|
| Severity | medium |
| Regression claimed | False |
| Error type | wrong-output |
| Error message | SKTypeface.FontWeight returns 400 on macOS instead of 200 for Metropolis Extra-Light font |
| Repro quality | partial |
| Target frameworks | — |

## Analysis

### Technical Summary

On macOS, SKTypeface.FontWeight returns 400 (Normal) for fonts with non-Normal weights loaded via SKTypeface.FromFile(). The underlying cause is likely in Skia's macOS CoreText font backend, which may not correctly read the OS/2 table weight when constructing the font style for a file-loaded typeface. The SKTypeface.FontWeight property directly calls sk_typeface_get_font_weight, which delegates to the native Skia SkTypeface::fontStyle().weight() — a difference in macOS CoreText vs Windows DirectWrite behavior when inferring font weight from the font file.

### Rationale

The reporter provides clear before/after platform comparison (Windows=200 correct, macOS=400 wrong) with a reproducible code snippet. The SKTypeface.FontWeight property passes through to the native sk_typeface_get_font_weight C API, which reflects whatever Skia's platform font backend provides. The discrepancy between platforms points to a platform-specific issue in Skia's macOS CoreText backend when loading fonts from files versus the Windows DirectWrite backend. This is a real bug (wrong output) confined to macOS.

### Key Signals

- "equals to 200 on windows (correct), equals 400 of macOS (incorrect)" — **issue body** (Clear platform-specific wrong-output bug. The reporter has tested on both platforms with the same font file.)
- "var typeface = SKTypeface.FromFile(@"metropolis.extra-light.otf")" — **issue body** (File-loaded typeface via SKFontManager.CreateTypeface(path). The path goes through the platform-specific font manager.)

### Code Investigation

| File | Lines | Relevance | Finding |
|------|-------|-----------|---------|
| `binding/SkiaSharp/SKTypeface.cs` | 89-107 | direct | SKTypeface.FromFile delegates to SKFontManager.Default.CreateTypeface(path, index). SKTypeface.FontWeight calls SkiaApi.sk_typeface_get_font_weight(Handle) directly — no SkiaSharp-level transformation. The weight value comes entirely from the native Skia layer. |
| `binding/SkiaSharp/SKFontStyle.cs` | 48 | related | SKFontStyle.Weight property calls sk_fontstyle_get_weight(Handle), which is a separate C API entry point from sk_typeface_get_font_weight. Both ultimately rely on SkTypeface::fontStyle() from Skia's platform backend. A potential workaround: typeface.FontStyle.Weight may or may not differ on macOS. |

### Next Questions

- Does typeface.FontStyle.Weight also return 400 on macOS, or only typeface.FontWeight?
- Which SkiaSharp version is the reporter using?
- Does this affect all fonts with non-Normal weights, or only specific font formats (OTF vs TTF)?
- Is this reproducible with other variable-weight fonts on macOS?

### Resolution Proposals

**Hypothesis:** Skia's macOS CoreText backend does not correctly infer font weight from the OS/2 table when loading a font from a file path, returning the default weight (400) instead of the declared weight (200). The Windows DirectWrite backend reads the OS/2 weight correctly.

1. **Use FontStyle.Weight as workaround** — workaround, confidence 0.55 (55%), cost/xs, validated=untested
   - Try reading typeface.FontStyle.Weight instead of typeface.FontWeight — these may differ on macOS if sk_typeface_get_fontstyle uses a different code path.
2. **Fix upstream in Skia macOS font backend** — fix, confidence 0.70 (70%), cost/l, validated=untested
   - The root fix would be in Skia's macOS CoreText typeface implementation to correctly read font weight from the OS/2 table or font metadata when loading from a file. This requires changes to the Skia submodule.

**Recommended proposal:** Use FontStyle.Weight as workaround

**Why:** Immediate workaround for reporter with no code changes required while upstream fix is investigated.

## Recommendations

### Actionability

| Field | Value |
|-------|-------|
| Suggested action | needs-investigation |
| Confidence | 0.82 (82%) |
| Reason | Real platform-specific bug with clear repro code and platform comparison. Needs macOS reproduction to confirm and trace the root cause in Skia's CoreText backend. |
| Suggested repro platform | macos |

### Missing Info

- SkiaSharp NuGet version used
- macOS version
- Whether other fonts or font formats (TTF) exhibit the same issue

### Automatable Actions

| Type | Risk | Confidence | Description | Details |
|------|------|------------|-------------|---------|
| update-labels | low | 0.92 (92%) | Apply bug, core SkiaSharp, and macOS platform labels | labels=type/bug, area/SkiaSharp, os/macOS |
| add-comment | medium | 0.82 (82%) | Acknowledge the bug, ask for version info, suggest FontStyle.Weight as a potential workaround | — |

**Comment draft for `add-comment`:**

```markdown
Thanks for the detailed report and repro project!

This looks like a platform-specific issue in Skia's macOS CoreText backend — when loading a font from a file, the weight metadata may not be read correctly on macOS, falling back to the default weight (400/Normal) instead of the declared weight (200/ExtraLight). Windows uses DirectWrite which appears to handle this correctly.

As a potential workaround, you could try reading the weight via `typeface.FontStyle.Weight` instead of `typeface.FontWeight` — these take slightly different code paths and may differ on macOS.

Could you also share:
- Which version of SkiaSharp you are using?
- Your macOS version?
- Does this affect TTF fonts as well, or only OTF?
```

<details>
<summary>Raw JSON</summary>

```json
{
  "meta": {
    "schemaVersion": "1.0",
    "number": 1582,
    "repo": "mono/SkiaSharp",
    "analyzedAt": "2026-05-03T08:13:57Z"
  },
  "summary": "SKTypeface.FontWeight returns 400 (Normal) instead of the correct weight (200 for Extra-Light) when loading a font from a file on macOS, while Windows returns the correct value.",
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
      "os/macOS"
    ]
  },
  "evidence": {
    "bugSignals": {
      "severity": "medium",
      "regressionClaimed": false,
      "errorType": "wrong-output",
      "errorMessage": "SKTypeface.FontWeight returns 400 on macOS instead of 200 for Metropolis Extra-Light font",
      "reproQuality": "partial",
      "targetFrameworks": []
    },
    "reproEvidence": {
      "stepsToReproduce": [
        "Load a font file with a non-Normal weight (e.g., Metropolis Extra-Light OTF)",
        "Call SKTypeface.FromFile() on both Windows and macOS",
        "Read the FontWeight property",
        "On Windows: returns 200 (correct); on macOS: returns 400 (incorrect)"
      ],
      "environmentDetails": "macOS (version not specified), Windows (working correctly). SkiaSharp version not specified.",
      "repoLinks": [
        {
          "url": "https://github.com/mono/SkiaSharp/files/5844440/ConsoleApp1.zip",
          "description": "Minimal repro project (ConsoleApp1.zip) attached by reporter"
        }
      ]
    }
  },
  "analysis": {
    "summary": "On macOS, SKTypeface.FontWeight returns 400 (Normal) for fonts with non-Normal weights loaded via SKTypeface.FromFile(). The underlying cause is likely in Skia's macOS CoreText font backend, which may not correctly read the OS/2 table weight when constructing the font style for a file-loaded typeface. The SKTypeface.FontWeight property directly calls sk_typeface_get_font_weight, which delegates to the native Skia SkTypeface::fontStyle().weight() — a difference in macOS CoreText vs Windows DirectWrite behavior when inferring font weight from the font file.",
    "rationale": "The reporter provides clear before/after platform comparison (Windows=200 correct, macOS=400 wrong) with a reproducible code snippet. The SKTypeface.FontWeight property passes through to the native sk_typeface_get_font_weight C API, which reflects whatever Skia's platform font backend provides. The discrepancy between platforms points to a platform-specific issue in Skia's macOS CoreText backend when loading fonts from files versus the Windows DirectWrite backend. This is a real bug (wrong output) confined to macOS.",
    "keySignals": [
      {
        "text": "equals to 200 on windows (correct), equals 400 of macOS (incorrect)",
        "source": "issue body",
        "interpretation": "Clear platform-specific wrong-output bug. The reporter has tested on both platforms with the same font file."
      },
      {
        "text": "var typeface = SKTypeface.FromFile(@\"metropolis.extra-light.otf\")",
        "source": "issue body",
        "interpretation": "File-loaded typeface via SKFontManager.CreateTypeface(path). The path goes through the platform-specific font manager."
      }
    ],
    "codeInvestigation": [
      {
        "file": "binding/SkiaSharp/SKTypeface.cs",
        "lines": "89-107",
        "finding": "SKTypeface.FromFile delegates to SKFontManager.Default.CreateTypeface(path, index). SKTypeface.FontWeight calls SkiaApi.sk_typeface_get_font_weight(Handle) directly — no SkiaSharp-level transformation. The weight value comes entirely from the native Skia layer.",
        "relevance": "direct"
      },
      {
        "file": "binding/SkiaSharp/SKFontStyle.cs",
        "lines": "48",
        "finding": "SKFontStyle.Weight property calls sk_fontstyle_get_weight(Handle), which is a separate C API entry point from sk_typeface_get_font_weight. Both ultimately rely on SkTypeface::fontStyle() from Skia's platform backend. A potential workaround: typeface.FontStyle.Weight may or may not differ on macOS.",
        "relevance": "related"
      }
    ],
    "nextQuestions": [
      "Does typeface.FontStyle.Weight also return 400 on macOS, or only typeface.FontWeight?",
      "Which SkiaSharp version is the reporter using?",
      "Does this affect all fonts with non-Normal weights, or only specific font formats (OTF vs TTF)?",
      "Is this reproducible with other variable-weight fonts on macOS?"
    ],
    "resolution": {
      "hypothesis": "Skia's macOS CoreText backend does not correctly infer font weight from the OS/2 table when loading a font from a file path, returning the default weight (400) instead of the declared weight (200). The Windows DirectWrite backend reads the OS/2 weight correctly.",
      "proposals": [
        {
          "title": "Use FontStyle.Weight as workaround",
          "description": "Try reading typeface.FontStyle.Weight instead of typeface.FontWeight — these may differ on macOS if sk_typeface_get_fontstyle uses a different code path.",
          "category": "workaround",
          "confidence": 0.55,
          "effort": "cost/xs",
          "validated": "untested"
        },
        {
          "title": "Fix upstream in Skia macOS font backend",
          "description": "The root fix would be in Skia's macOS CoreText typeface implementation to correctly read font weight from the OS/2 table or font metadata when loading from a file. This requires changes to the Skia submodule.",
          "category": "fix",
          "confidence": 0.7,
          "effort": "cost/l",
          "validated": "untested"
        }
      ],
      "recommendedProposal": "Use FontStyle.Weight as workaround",
      "recommendedReason": "Immediate workaround for reporter with no code changes required while upstream fix is investigated."
    }
  },
  "output": {
    "actionability": {
      "suggestedAction": "needs-investigation",
      "confidence": 0.82,
      "reason": "Real platform-specific bug with clear repro code and platform comparison. Needs macOS reproduction to confirm and trace the root cause in Skia's CoreText backend.",
      "suggestedReproPlatform": "macos"
    },
    "missingInfo": [
      "SkiaSharp NuGet version used",
      "macOS version",
      "Whether other fonts or font formats (TTF) exhibit the same issue"
    ],
    "actions": [
      {
        "type": "update-labels",
        "description": "Apply bug, core SkiaSharp, and macOS platform labels",
        "risk": "low",
        "confidence": 0.92,
        "labels": [
          "type/bug",
          "area/SkiaSharp",
          "os/macOS"
        ]
      },
      {
        "type": "add-comment",
        "description": "Acknowledge the bug, ask for version info, suggest FontStyle.Weight as a potential workaround",
        "risk": "medium",
        "confidence": 0.82,
        "comment": "Thanks for the detailed report and repro project!\n\nThis looks like a platform-specific issue in Skia's macOS CoreText backend — when loading a font from a file, the weight metadata may not be read correctly on macOS, falling back to the default weight (400/Normal) instead of the declared weight (200/ExtraLight). Windows uses DirectWrite which appears to handle this correctly.\n\nAs a potential workaround, you could try reading the weight via `typeface.FontStyle.Weight` instead of `typeface.FontWeight` — these take slightly different code paths and may differ on macOS.\n\nCould you also share:\n- Which version of SkiaSharp you are using?\n- Your macOS version?\n- Does this affect TTF fonts as well, or only OTF?"
      }
    ]
  }
}
```

</details>
