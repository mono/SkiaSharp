# Issue Triage Report — #2637

| Field | Value |
|-------|-------|
| Repository | mono/SkiaSharp |
| Analyzed | 2026-05-04T19:45:39Z |
| Type | type/bug (0.85 (85%)) |
| Area | area/SkiaSharp (0.75 (75%)) |
| Suggested action | needs-info (0.80 (80%)) |

**Issue Summary:** SVG file loading crashes with a 'key not found: offset' exception in SkiaSharp.Extended.Svg when calling svg.Load() with a file path or stream.

**Analysis:** The crash occurs in SkiaSharp.Extended.Svg (a separate NuGet package from the main SkiaSharp library) when loading SVG files that contain gradient stop elements without an explicit 'offset' attribute. The SKSvg.ReadStops() method throws a KeyNotFoundException when the 'offset' key is missing from the parsed style dictionary. Issue #2670 reports the exact same exception stack trace. The regression between 2.88.2 and 2.88.3 suggests a change in SVG gradient parsing in that release.

**Recommendations:** **needs-info** — User reports a crash but does not provide a full stack trace. The error message 'Offset' strongly suggests a known issue in SkiaSharp.Extended.Svg (#2670), but needs confirmation of the package being used and the full exception.

---

## Classification

| Field | Value |
|-------|-------|
| Type | type/bug |
| Area | area/SkiaSharp |
| Platforms | — |
| Backends | backend/SVG |
| Tenets | tenet/reliability |
| Partner | — |

## Evidence

### Reproduction

**Attachments:**
- SvgToPng.zip — https://github.com/mono/SkiaSharp/files/12778167/SvgToPng.zip — Sample project demonstrating the SVG loading crash

### Bug Signals

| Field | Value |
|-------|-------|
| Severity | medium |
| Regression claimed | True |
| Error type | exception |
| Error message | Crashes asking 'Offet' (likely KeyNotFoundException: The given key 'offset' was not present in the dictionary) |
| Repro quality | partial |
| Target frameworks | — |

### Version Analysis

| Field | Value |
|-------|-------|
| Mentioned versions | 2.88.3, 2.88.2 |
| Worked in | 2.88.2 |
| Broke in | 2.88.3 |
| Current relevance | likely |
| Relevance reason | Regression from 2.88.2 to 2.88.3 in SVG loading |

### Regression

| Field | Value |
|-------|-------|
| Is regression | True |
| Confidence | 0.75 (75%) |
| Reason | User explicitly states worked in 2.88.2 and broken in 2.88.3 |
| Worked in version | 2.88.2 |
| Broke in version | 2.88.3 |

## Analysis

### Technical Summary

The crash occurs in SkiaSharp.Extended.Svg (a separate NuGet package from the main SkiaSharp library) when loading SVG files that contain gradient stop elements without an explicit 'offset' attribute. The SKSvg.ReadStops() method throws a KeyNotFoundException when the 'offset' key is missing from the parsed style dictionary. Issue #2670 reports the exact same exception stack trace. The regression between 2.88.2 and 2.88.3 suggests a change in SVG gradient parsing in that release.

### Rationale

Classified as type/bug because the user reports a clear regression (worked in 2.88.2, broken in 2.88.3) with a crash. The area is SkiaSharp (core) because the issue is reported against SkiaSharp version but the actual bug is in SkiaSharp.Extended.Svg. The suggested action is needs-info because we need the actual stack trace and confirmation of which package (SkiaSharp.Extended.Svg vs another) is being used.

### Key Signals

- "It cashes asking Offet which is bogus message." — **issue body** (Crash with 'Offset' key missing — matches KeyNotFoundException for 'offset' key in gradient stop parsing)
- "Version 2.88.3 (Current) / Last Known Good: 2.88.2" — **issue metadata** (Clear regression signal between patch versions)

### Code Investigation

| File | Lines | Relevance | Finding |
|------|-------|-----------|---------|
| `binding/SkiaSharp/SKSVG.cs` | — | context | This file only contains SKSvgCanvas for creating SVG output canvases. There is no SKSvg loading class in the main SkiaSharp repository. |
| `(SkiaSharp.Extended.Svg package - external)` | — | direct | The svg.Load() method and SKSvg class are part of SkiaSharp.Extended.Svg, a separate NuGet package maintained in the mono/SkiaSharp.Extended repository. The error stack in related issue #2670 points to SkiaSharp.Extended.Svg.SKSvg.ReadStops() which throws KeyNotFoundException when 'offset' key is absent in gradient stop elements. |

### Next Questions

- What is the full exception stack trace?
- Which SVG loading library/package is being used (SkiaSharp.Extended.Svg or Svg.Skia)?
- Does the dotnet_bot.svg file contain gradient elements with stop elements?

### Resolution Proposals

1. **Redirect to SkiaSharp.Extended and request stack trace** — investigation, cost/xs, validated=untested
   - Confirm the exact package being used (SkiaSharp.Extended.Svg) and redirect to the SkiaSharp.Extended repository if confirmed. The 'Offset' crash is the same as #2670 and is caused by SVG gradient stop elements missing explicit 'offset' attributes.
2. **Add explicit offset attributes to SVG gradient stops** — workaround, cost/xs, validated=untested
   - Ensure SVG files have explicit 'offset' attributes on all gradient stop elements (e.g., offset="0%" and offset="100%"). This avoids the KeyNotFoundException in ReadStops().

**Recommended proposal:** Redirect to SkiaSharp.Extended and request stack trace

**Why:** We need to confirm which package/version is in use and whether this is the same root cause as #2670 before suggesting code-level fixes.

## Recommendations

### Actionability

| Field | Value |
|-------|-------|
| Suggested action | needs-info |
| Confidence | 0.80 (80%) |
| Reason | User reports a crash but does not provide a full stack trace. The error message 'Offset' strongly suggests a known issue in SkiaSharp.Extended.Svg (#2670), but needs confirmation of the package being used and the full exception. |
| Suggested repro platform | windows |

### Missing Info

- Full exception stack trace
- Which SVG loading package is being used (SkiaSharp.Extended.Svg version?)
- Content of the SVG file being loaded (does it contain gradient stops?)

### Automatable Actions

| Type | Risk | Confidence | Description | Details |
|------|------|------------|-------------|---------|
| update-labels | low | 0.85 (85%) | Apply type/bug, area/SkiaSharp, backend/SVG, tenet/reliability labels | labels=type/bug, area/SkiaSharp, backend/SVG, tenet/reliability |
| add-comment | medium | 0.80 (80%) | Ask for full stack trace and confirm which SVG library is in use; mention related issue #2670 | — |
| link-related | low | 0.85 (85%) | Link to related issue #2670 which has identical error | linkedIssue=#2670 |

**Comment draft for `add-comment`:**

```markdown
Thank you for the report! To help investigate this issue, could you please provide:

1. The full exception stack trace (not just the message)
2. Which SVG loading package are you using? (e.g., `SkiaSharp.Extended.Svg` from NuGet, or another library)
3. Does the SVG file you're loading contain gradient elements with `<stop>` elements?

The 'Offset' crash you're seeing may be related to #2670 which reports a `KeyNotFoundException: The given key 'offset' was not present in the dictionary` in `SKSvg.ReadStops()` when parsing SVG gradient stops. If you're using `SkiaSharp.Extended.Svg`, please also check the [SkiaSharp.Extended repository](https://github.com/mono/SkiaSharp.Extended) for related issues.

As a potential workaround, ensure your SVG gradient `<stop>` elements have explicit `offset` attributes (e.g., `offset="0%"` and `offset="100%"`).
```

<details>
<summary>Raw JSON</summary>

```json
{
  "meta": {
    "schemaVersion": "1.0",
    "number": 2637,
    "repo": "mono/SkiaSharp",
    "analyzedAt": "2026-05-04T19:45:39Z"
  },
  "summary": "SVG file loading crashes with a 'key not found: offset' exception in SkiaSharp.Extended.Svg when calling svg.Load() with a file path or stream.",
  "classification": {
    "type": {
      "value": "type/bug",
      "confidence": 0.85
    },
    "area": {
      "value": "area/SkiaSharp",
      "confidence": 0.75
    },
    "backends": [
      "backend/SVG"
    ],
    "tenets": [
      "tenet/reliability"
    ]
  },
  "evidence": {
    "bugSignals": {
      "severity": "medium",
      "regressionClaimed": true,
      "errorType": "exception",
      "errorMessage": "Crashes asking 'Offet' (likely KeyNotFoundException: The given key 'offset' was not present in the dictionary)",
      "reproQuality": "partial",
      "targetFrameworks": []
    },
    "reproEvidence": {
      "attachments": [
        {
          "url": "https://github.com/mono/SkiaSharp/files/12778167/SvgToPng.zip",
          "filename": "SvgToPng.zip",
          "description": "Sample project demonstrating the SVG loading crash"
        }
      ]
    },
    "versionAnalysis": {
      "mentionedVersions": [
        "2.88.3",
        "2.88.2"
      ],
      "workedIn": "2.88.2",
      "brokeIn": "2.88.3",
      "currentRelevance": "likely",
      "relevanceReason": "Regression from 2.88.2 to 2.88.3 in SVG loading"
    },
    "regression": {
      "isRegression": true,
      "confidence": 0.75,
      "reason": "User explicitly states worked in 2.88.2 and broken in 2.88.3",
      "workedInVersion": "2.88.2",
      "brokeInVersion": "2.88.3"
    }
  },
  "analysis": {
    "summary": "The crash occurs in SkiaSharp.Extended.Svg (a separate NuGet package from the main SkiaSharp library) when loading SVG files that contain gradient stop elements without an explicit 'offset' attribute. The SKSvg.ReadStops() method throws a KeyNotFoundException when the 'offset' key is missing from the parsed style dictionary. Issue #2670 reports the exact same exception stack trace. The regression between 2.88.2 and 2.88.3 suggests a change in SVG gradient parsing in that release.",
    "codeInvestigation": [
      {
        "file": "binding/SkiaSharp/SKSVG.cs",
        "finding": "This file only contains SKSvgCanvas for creating SVG output canvases. There is no SKSvg loading class in the main SkiaSharp repository.",
        "relevance": "context"
      },
      {
        "file": "(SkiaSharp.Extended.Svg package - external)",
        "finding": "The svg.Load() method and SKSvg class are part of SkiaSharp.Extended.Svg, a separate NuGet package maintained in the mono/SkiaSharp.Extended repository. The error stack in related issue #2670 points to SkiaSharp.Extended.Svg.SKSvg.ReadStops() which throws KeyNotFoundException when 'offset' key is absent in gradient stop elements.",
        "relevance": "direct"
      }
    ],
    "keySignals": [
      {
        "text": "It cashes asking Offet which is bogus message.",
        "source": "issue body",
        "interpretation": "Crash with 'Offset' key missing — matches KeyNotFoundException for 'offset' key in gradient stop parsing"
      },
      {
        "text": "Version 2.88.3 (Current) / Last Known Good: 2.88.2",
        "source": "issue metadata",
        "interpretation": "Clear regression signal between patch versions"
      }
    ],
    "rationale": "Classified as type/bug because the user reports a clear regression (worked in 2.88.2, broken in 2.88.3) with a crash. The area is SkiaSharp (core) because the issue is reported against SkiaSharp version but the actual bug is in SkiaSharp.Extended.Svg. The suggested action is needs-info because we need the actual stack trace and confirmation of which package (SkiaSharp.Extended.Svg vs another) is being used.",
    "resolution": {
      "proposals": [
        {
          "title": "Redirect to SkiaSharp.Extended and request stack trace",
          "category": "investigation",
          "description": "Confirm the exact package being used (SkiaSharp.Extended.Svg) and redirect to the SkiaSharp.Extended repository if confirmed. The 'Offset' crash is the same as #2670 and is caused by SVG gradient stop elements missing explicit 'offset' attributes.",
          "effort": "cost/xs",
          "validated": "untested"
        },
        {
          "title": "Add explicit offset attributes to SVG gradient stops",
          "category": "workaround",
          "description": "Ensure SVG files have explicit 'offset' attributes on all gradient stop elements (e.g., offset=\"0%\" and offset=\"100%\"). This avoids the KeyNotFoundException in ReadStops().",
          "effort": "cost/xs",
          "validated": "untested"
        }
      ],
      "recommendedProposal": "Redirect to SkiaSharp.Extended and request stack trace",
      "recommendedReason": "We need to confirm which package/version is in use and whether this is the same root cause as #2670 before suggesting code-level fixes."
    },
    "nextQuestions": [
      "What is the full exception stack trace?",
      "Which SVG loading library/package is being used (SkiaSharp.Extended.Svg or Svg.Skia)?",
      "Does the dotnet_bot.svg file contain gradient elements with stop elements?"
    ]
  },
  "output": {
    "actionability": {
      "suggestedAction": "needs-info",
      "confidence": 0.8,
      "reason": "User reports a crash but does not provide a full stack trace. The error message 'Offset' strongly suggests a known issue in SkiaSharp.Extended.Svg (#2670), but needs confirmation of the package being used and the full exception.",
      "suggestedReproPlatform": "windows"
    },
    "missingInfo": [
      "Full exception stack trace",
      "Which SVG loading package is being used (SkiaSharp.Extended.Svg version?)",
      "Content of the SVG file being loaded (does it contain gradient stops?)"
    ],
    "actions": [
      {
        "type": "update-labels",
        "description": "Apply type/bug, area/SkiaSharp, backend/SVG, tenet/reliability labels",
        "risk": "low",
        "confidence": 0.85,
        "labels": [
          "type/bug",
          "area/SkiaSharp",
          "backend/SVG",
          "tenet/reliability"
        ]
      },
      {
        "type": "add-comment",
        "description": "Ask for full stack trace and confirm which SVG library is in use; mention related issue #2670",
        "risk": "medium",
        "confidence": 0.8,
        "comment": "Thank you for the report! To help investigate this issue, could you please provide:\n\n1. The full exception stack trace (not just the message)\n2. Which SVG loading package are you using? (e.g., `SkiaSharp.Extended.Svg` from NuGet, or another library)\n3. Does the SVG file you're loading contain gradient elements with `<stop>` elements?\n\nThe 'Offset' crash you're seeing may be related to #2670 which reports a `KeyNotFoundException: The given key 'offset' was not present in the dictionary` in `SKSvg.ReadStops()` when parsing SVG gradient stops. If you're using `SkiaSharp.Extended.Svg`, please also check the [SkiaSharp.Extended repository](https://github.com/mono/SkiaSharp.Extended) for related issues.\n\nAs a potential workaround, ensure your SVG gradient `<stop>` elements have explicit `offset` attributes (e.g., `offset=\"0%\"` and `offset=\"100%\"`)."
      },
      {
        "type": "link-related",
        "description": "Link to related issue #2670 which has identical error",
        "risk": "low",
        "confidence": 0.85,
        "linkedIssue": 2670
      }
    ]
  }
}
```

</details>
