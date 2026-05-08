# Issue Triage Report — #2670

| Field | Value |
|-------|-------|
| Repository | mono/SkiaSharp |
| Analyzed | 2026-04-30T04:15:00Z |
| Type | type/bug (0.90 (90%)) |
| Area | area/SkiaSharp (0.65 (65%)) |
| Suggested action | close-as-external (0.88 (88%)) |

**Issue Summary:** SkiaSharp.Extended.Svg.SKSvg throws KeyNotFoundException on 'offset' key when loading an SVG with a linear gradient whose stops omit the optional offset attribute.

**Analysis:** SkiaSharp.Extended.Svg.SKSvg.ReadStops() uses a dictionary lookup for the 'offset' attribute without a TryGetValue guard. The SVG spec allows gradient stops to omit the offset attribute (defaulting to 0 and 1 for first/last stop), but the implementation requires it unconditionally, causing a KeyNotFoundException for spec-compliant SVGs.

**Recommendations:** **close-as-external** — The bug is in SkiaSharp.Extended.Svg (mono/SkiaSharp.Extended), which is a separate repository and package. This repo (mono/SkiaSharp) does not contain SVG parsing code. The issue should be redirected to the correct repository.

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
| Current labels | type/bug |

## Evidence

### Reproduction

1. Create a new SKSvg() instance
2. Call svgDocument.Load(imageFile.FullName) with the provided SVG file
3. Observe KeyNotFoundException: The given key 'offset' was not present in the dictionary

**Environment:** macOS Ventura 13.1, Mac M1 Max, Visual Studio for Mac, SkiaSharp 2.88.3

**Repository links:**
- https://github.com/mono/SkiaSharp/assets/3392986/0d2385c6-be51-40af-86f4-f8ef30366438 — Screenshot of the SVG file (logo-Ponant+Marcel)

### Bug Signals

| Field | Value |
|-------|-------|
| Severity | medium |
| Regression claimed | False |
| Error type | exception |
| Error message | System.Collections.Generic.KeyNotFoundException: The given key 'offset' was not present in the dictionary. |
| Repro quality | partial |
| Target frameworks | — |

**Stack trace:**

```text
at SkiaSharp.Extended.Svg.SKSvg.ReadStops(XElement e) at SkiaSharp.Extended.Svg.SKSvg.ReadLinearGradient(XElement e) at SkiaSharp.Extended.Svg.SKSvg.ReadPaints(...) at SkiaSharp.Extended.Svg.SKSvg.ReadElement(...) at SkiaSharp.Extended.Svg.SKSvg.Load(Stream stream) at SkiaSharp.Extended.Svg.SKSvg.Load(String filename)
```

### Version Analysis

| Field | Value |
|-------|-------|
| Mentioned versions | 2.88.3 |
| Worked in | — |
| Broke in | — |
| Current relevance | likely |
| Relevance reason | The bug is in SkiaSharp.Extended.Svg, which has its own release cycle. No fix has been found in the main SkiaSharp repository. |

## Analysis

### Technical Summary

SkiaSharp.Extended.Svg.SKSvg.ReadStops() uses a dictionary lookup for the 'offset' attribute without a TryGetValue guard. The SVG spec allows gradient stops to omit the offset attribute (defaulting to 0 and 1 for first/last stop), but the implementation requires it unconditionally, causing a KeyNotFoundException for spec-compliant SVGs.

### Rationale

The stack trace clearly identifies SkiaSharp.Extended.Svg.SKSvg as the source of the bug. The crash occurs because ReadStops() performs a dictionary key access for 'offset' without checking for its presence. The 'offset' attribute is optional in the SVG specification (default: 0 for the first stop, 1 for the last stop), so any SVG using this valid shorthand will fail. The code is in SkiaSharp.Extended, not in this (mono/SkiaSharp) repository.

### Key Signals

- "The given key 'offset' was not present in the dictionary" — **issue body stack trace** (ReadStops() performs a hard dictionary lookup for 'offset' without null/missing check, violating the SVG spec which allows stops without explicit offset.)
- "at SkiaSharp.Extended.Svg.SKSvg.ReadStops(XElement e)" — **issue body stack trace** (Bug is in SkiaSharp.Extended.Svg package, not in this (mono/SkiaSharp) repository.)
- "Any solution for this?" — **comment by dejanbasic** (Issue still unresolved as of 2024-08-27; community is seeking a workaround.)

### Code Investigation

| File | Lines | Relevance | Finding |
|------|-------|-----------|---------|
| `binding/SkiaSharp/SKSVG.cs` | 1-34 | context | This file implements SKSvgCanvas for writing SVG output, not for parsing SVG. The SkiaSharp.Extended.Svg.SKSvg parser (the class mentioned in the stack trace) is NOT present in this repository — it lives in mono/SkiaSharp.Extended. |
| `tests/Tests/SkiaSharp/SKCanvasTest.cs` | — | context | Tests reference SKSvgCanvas (SVG output) only, not SKSvg parsing. Confirms SkiaSharp core has no SVG parsing implementation. |

### Workarounds

- Pre-process the SVG file with an XML transform to add explicit 'offset' attributes to all gradient stops that lack them (e.g., set offset='0' on first stop, offset='1' on last stop).
- Use a different SVG rendering library such as Svg.Skia (https://github.com/wieslawsoltes/Svg.Skia) which has more complete SVG spec support.

### Next Questions

- Is there a fix available in a later version of SkiaSharp.Extended.Svg?
- Has this been reported and/or fixed in the mono/SkiaSharp.Extended repository?

### Resolution Proposals

**Hypothesis:** ReadStops() in SkiaSharp.Extended.Svg does not handle gradient stops that lack an explicit 'offset' attribute, which is permitted by the SVG specification.

1. **Pre-process SVG to add explicit offset attributes** — workaround, confidence 0.80 (80%), cost/s, validated=untested
   - Load the SVG as XML, add missing 'offset' attributes to gradient stops (first stop: '0', last stop: '1'), then reload into SKSvg. This is a client-side workaround until SkiaSharp.Extended.Svg is fixed.
2. **Switch to Svg.Skia library** — alternative, confidence 0.85 (85%), cost/m, validated=untested
   - Use the Svg.Skia NuGet package (https://www.nuget.org/packages/Svg.Skia) as a replacement. It is actively maintained and has more complete SVG spec coverage including optional gradient stop attributes.
3. **Report and fix in mono/SkiaSharp.Extended** — fix, confidence 0.90 (90%), cost/xs, validated=untested
   - File an issue (or fix PR) in the mono/SkiaSharp.Extended repository to add a safe dictionary lookup in ReadStops(), supplying default offset values when the attribute is absent.

**Recommended proposal:** Report and fix in mono/SkiaSharp.Extended

**Why:** Root fix is trivial (add ContainsKey/TryGetValue check with SVG-spec defaults). Client-side preprocessing is a reasonable immediate workaround.

## Recommendations

### Actionability

| Field | Value |
|-------|-------|
| Suggested action | close-as-external |
| Confidence | 0.88 (88%) |
| Reason | The bug is in SkiaSharp.Extended.Svg (mono/SkiaSharp.Extended), which is a separate repository and package. This repo (mono/SkiaSharp) does not contain SVG parsing code. The issue should be redirected to the correct repository. |
| Suggested repro platform | linux |

### Automatable Actions

| Type | Risk | Confidence | Description | Details |
|------|------|------------|-------------|---------|
| update-labels | low | 0.90 (90%) | Apply type/bug, area/SkiaSharp, os/macOS, tenet/reliability | labels=type/bug, area/SkiaSharp, os/macOS, tenet/reliability |
| add-comment | high | 0.88 (88%) | Redirect reporter to SkiaSharp.Extended repository and provide workarounds | — |
| close-issue | medium | 0.88 (88%) | Close as external — bug is in SkiaSharp.Extended, not SkiaSharp core | stateReason=not_planned |

**Comment draft for `add-comment`:**

```markdown
Thanks for the report! The exception originates in `SkiaSharp.Extended.Svg.SKSvg.ReadStops()`, which is part of the **SkiaSharp.Extended** package — that code lives in a [separate repository (mono/SkiaSharp.Extended)](https://github.com/mono/SkiaSharp.Extended), not here. Please file this issue there for the best chance of a fix.

The root cause is that `ReadStops()` performs a hard dictionary key access for the `offset` attribute, but the SVG specification allows gradient `<stop>` elements to omit `offset` (it defaults to `0` for the first stop and `1` for the last). Any spec-compliant SVG that relies on this default will trigger the exception.

**Workarounds in the meantime:**
1. Pre-process the SVG as XML and add explicit `offset` attributes to gradient stops that lack them before loading.
2. Switch to the [Svg.Skia](https://www.nuget.org/packages/Svg.Skia) NuGet package, which is actively maintained and has broader SVG spec support.

Closing this issue as it is out of scope for this repository.
```

<details>
<summary>Raw JSON</summary>

```json
{
  "meta": {
    "schemaVersion": "1.0",
    "number": 2670,
    "repo": "mono/SkiaSharp",
    "analyzedAt": "2026-04-30T04:15:00Z",
    "currentLabels": [
      "type/bug"
    ]
  },
  "summary": "SkiaSharp.Extended.Svg.SKSvg throws KeyNotFoundException on 'offset' key when loading an SVG with a linear gradient whose stops omit the optional offset attribute.",
  "classification": {
    "type": {
      "value": "type/bug",
      "confidence": 0.9
    },
    "area": {
      "value": "area/SkiaSharp",
      "confidence": 0.65
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
      "errorType": "exception",
      "errorMessage": "System.Collections.Generic.KeyNotFoundException: The given key 'offset' was not present in the dictionary.",
      "stackTrace": "at SkiaSharp.Extended.Svg.SKSvg.ReadStops(XElement e) at SkiaSharp.Extended.Svg.SKSvg.ReadLinearGradient(XElement e) at SkiaSharp.Extended.Svg.SKSvg.ReadPaints(...) at SkiaSharp.Extended.Svg.SKSvg.ReadElement(...) at SkiaSharp.Extended.Svg.SKSvg.Load(Stream stream) at SkiaSharp.Extended.Svg.SKSvg.Load(String filename)",
      "reproQuality": "partial",
      "targetFrameworks": []
    },
    "reproEvidence": {
      "stepsToReproduce": [
        "Create a new SKSvg() instance",
        "Call svgDocument.Load(imageFile.FullName) with the provided SVG file",
        "Observe KeyNotFoundException: The given key 'offset' was not present in the dictionary"
      ],
      "environmentDetails": "macOS Ventura 13.1, Mac M1 Max, Visual Studio for Mac, SkiaSharp 2.88.3",
      "repoLinks": [
        {
          "url": "https://github.com/mono/SkiaSharp/assets/3392986/0d2385c6-be51-40af-86f4-f8ef30366438",
          "description": "Screenshot of the SVG file (logo-Ponant+Marcel)"
        }
      ]
    },
    "versionAnalysis": {
      "mentionedVersions": [
        "2.88.3"
      ],
      "currentRelevance": "likely",
      "relevanceReason": "The bug is in SkiaSharp.Extended.Svg, which has its own release cycle. No fix has been found in the main SkiaSharp repository."
    }
  },
  "analysis": {
    "summary": "SkiaSharp.Extended.Svg.SKSvg.ReadStops() uses a dictionary lookup for the 'offset' attribute without a TryGetValue guard. The SVG spec allows gradient stops to omit the offset attribute (defaulting to 0 and 1 for first/last stop), but the implementation requires it unconditionally, causing a KeyNotFoundException for spec-compliant SVGs.",
    "rationale": "The stack trace clearly identifies SkiaSharp.Extended.Svg.SKSvg as the source of the bug. The crash occurs because ReadStops() performs a dictionary key access for 'offset' without checking for its presence. The 'offset' attribute is optional in the SVG specification (default: 0 for the first stop, 1 for the last stop), so any SVG using this valid shorthand will fail. The code is in SkiaSharp.Extended, not in this (mono/SkiaSharp) repository.",
    "keySignals": [
      {
        "text": "The given key 'offset' was not present in the dictionary",
        "source": "issue body stack trace",
        "interpretation": "ReadStops() performs a hard dictionary lookup for 'offset' without null/missing check, violating the SVG spec which allows stops without explicit offset."
      },
      {
        "text": "at SkiaSharp.Extended.Svg.SKSvg.ReadStops(XElement e)",
        "source": "issue body stack trace",
        "interpretation": "Bug is in SkiaSharp.Extended.Svg package, not in this (mono/SkiaSharp) repository."
      },
      {
        "text": "Any solution for this?",
        "source": "comment by dejanbasic",
        "interpretation": "Issue still unresolved as of 2024-08-27; community is seeking a workaround."
      }
    ],
    "codeInvestigation": [
      {
        "file": "binding/SkiaSharp/SKSVG.cs",
        "lines": "1-34",
        "finding": "This file implements SKSvgCanvas for writing SVG output, not for parsing SVG. The SkiaSharp.Extended.Svg.SKSvg parser (the class mentioned in the stack trace) is NOT present in this repository — it lives in mono/SkiaSharp.Extended.",
        "relevance": "context"
      },
      {
        "file": "tests/Tests/SkiaSharp/SKCanvasTest.cs",
        "finding": "Tests reference SKSvgCanvas (SVG output) only, not SKSvg parsing. Confirms SkiaSharp core has no SVG parsing implementation.",
        "relevance": "context"
      }
    ],
    "workarounds": [
      "Pre-process the SVG file with an XML transform to add explicit 'offset' attributes to all gradient stops that lack them (e.g., set offset='0' on first stop, offset='1' on last stop).",
      "Use a different SVG rendering library such as Svg.Skia (https://github.com/wieslawsoltes/Svg.Skia) which has more complete SVG spec support."
    ],
    "nextQuestions": [
      "Is there a fix available in a later version of SkiaSharp.Extended.Svg?",
      "Has this been reported and/or fixed in the mono/SkiaSharp.Extended repository?"
    ],
    "resolution": {
      "hypothesis": "ReadStops() in SkiaSharp.Extended.Svg does not handle gradient stops that lack an explicit 'offset' attribute, which is permitted by the SVG specification.",
      "proposals": [
        {
          "title": "Pre-process SVG to add explicit offset attributes",
          "description": "Load the SVG as XML, add missing 'offset' attributes to gradient stops (first stop: '0', last stop: '1'), then reload into SKSvg. This is a client-side workaround until SkiaSharp.Extended.Svg is fixed.",
          "category": "workaround",
          "confidence": 0.8,
          "effort": "cost/s",
          "validated": "untested"
        },
        {
          "title": "Switch to Svg.Skia library",
          "description": "Use the Svg.Skia NuGet package (https://www.nuget.org/packages/Svg.Skia) as a replacement. It is actively maintained and has more complete SVG spec coverage including optional gradient stop attributes.",
          "category": "alternative",
          "confidence": 0.85,
          "effort": "cost/m",
          "validated": "untested"
        },
        {
          "title": "Report and fix in mono/SkiaSharp.Extended",
          "description": "File an issue (or fix PR) in the mono/SkiaSharp.Extended repository to add a safe dictionary lookup in ReadStops(), supplying default offset values when the attribute is absent.",
          "category": "fix",
          "confidence": 0.9,
          "effort": "cost/xs",
          "validated": "untested"
        }
      ],
      "recommendedProposal": "Report and fix in mono/SkiaSharp.Extended",
      "recommendedReason": "Root fix is trivial (add ContainsKey/TryGetValue check with SVG-spec defaults). Client-side preprocessing is a reasonable immediate workaround."
    }
  },
  "output": {
    "actionability": {
      "suggestedAction": "close-as-external",
      "confidence": 0.88,
      "reason": "The bug is in SkiaSharp.Extended.Svg (mono/SkiaSharp.Extended), which is a separate repository and package. This repo (mono/SkiaSharp) does not contain SVG parsing code. The issue should be redirected to the correct repository.",
      "suggestedReproPlatform": "linux"
    },
    "actions": [
      {
        "type": "update-labels",
        "description": "Apply type/bug, area/SkiaSharp, os/macOS, tenet/reliability",
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
        "description": "Redirect reporter to SkiaSharp.Extended repository and provide workarounds",
        "risk": "high",
        "confidence": 0.88,
        "comment": "Thanks for the report! The exception originates in `SkiaSharp.Extended.Svg.SKSvg.ReadStops()`, which is part of the **SkiaSharp.Extended** package — that code lives in a [separate repository (mono/SkiaSharp.Extended)](https://github.com/mono/SkiaSharp.Extended), not here. Please file this issue there for the best chance of a fix.\n\nThe root cause is that `ReadStops()` performs a hard dictionary key access for the `offset` attribute, but the SVG specification allows gradient `<stop>` elements to omit `offset` (it defaults to `0` for the first stop and `1` for the last). Any spec-compliant SVG that relies on this default will trigger the exception.\n\n**Workarounds in the meantime:**\n1. Pre-process the SVG as XML and add explicit `offset` attributes to gradient stops that lack them before loading.\n2. Switch to the [Svg.Skia](https://www.nuget.org/packages/Svg.Skia) NuGet package, which is actively maintained and has broader SVG spec support.\n\nClosing this issue as it is out of scope for this repository."
      },
      {
        "type": "close-issue",
        "description": "Close as external — bug is in SkiaSharp.Extended, not SkiaSharp core",
        "risk": "medium",
        "confidence": 0.88,
        "stateReason": "not_planned"
      }
    ]
  }
}
```

</details>
