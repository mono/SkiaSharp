# Issue Triage Report — #2602

| Field | Value |
|-------|-------|
| Repository | mono/SkiaSharp |
| Analyzed | 2026-05-02T09:45:00Z |
| Type | type/bug (0.90 (90%)) |
| Area | area/SkiaSharp (0.85 (85%)) |
| Suggested action | needs-investigation (0.80 (80%)) |

**Issue Summary:** Reporter identifies multiple SVG canvas output issues in SkiaSharp 2.88.3: DrawPicture fill not applied, DrawText producing broken output, and Canvas.Clear behaving inconsistently between SVG and PNG backends.

**Analysis:** Multiple SVG canvas output bugs: DrawPicture with fills, DrawText, and Canvas.Clear do not produce correct or consistent SVG output compared to raster rendering. Root cause likely in upstream Skia's SVGCanvas implementation which changed substantially between Skia versions backing SkiaSharp 1.x and 2.88.

**Recommendations:** **needs-investigation** — Multiple distinct SVG rendering bugs reported with screenshots and external test cases. Real bugs with partial repro — needs structured repro for each sub-issue.

---

## Classification

| Field | Value |
|-------|-------|
| Type | type/bug |
| Area | area/SkiaSharp |
| Platforms | — |
| Backends | backend/SVG, backend/Raster |
| Tenets | tenet/compatibility, tenet/reliability |
| Partner | — |
| Current labels | type/bug |

## Evidence

### Reproduction

1. Create an SKSvgCanvas and draw using DrawPicture, DrawText, and Canvas.Clear
2. Compare output SVG against equivalent PNG rendered canvas
3. Observe visual differences: fill not applied in DrawPicture, missing or broken text in SVG, Clear behavior differs

**Environment:** SkiaSharp 2.88.3, Visual Studio on Windows, Android 13, iOS 16

**Repository links:**
- https://github.com/EvgenyMuryshkin/SVGBugs/blob/main/SVGBugs/SVGBugsTests.cs — Reporter's test cases demonstrating each SVG bug

### Bug Signals

| Field | Value |
|-------|-------|
| Severity | medium |
| Regression claimed | True |
| Error type | wrong-output |
| Error message | SVG output does not match PNG output; DrawPicture fill not applied; DrawText output broken; Canvas.Clear inconsistency between SVG and PNG |
| Repro quality | partial |
| Target frameworks | net-android, net-ios |

### Version Analysis

| Field | Value |
|-------|-------|
| Mentioned versions | 2.88.3, 1.x |
| Worked in | 1.x |
| Broke in | 2.88.x |
| Current relevance | likely |
| Relevance reason | SKSvgCanvas in current codebase still delegates directly to Skia's SVG canvas; the SVG rendering pipeline has not been significantly reworked in recent versions. |

### Regression

| Field | Value |
|-------|-------|
| Is regression | True |
| Confidence | 0.65 (65%) |
| Reason | Reporter states SVG rendering worked in 1.x. The SVG canvas backend in upstream Skia changed between the versions SkiaSharp 1.x and 2.x, making some behaviours differ. |
| Worked in version | 1.x |
| Broke in version | 2.88.x |

## Analysis

### Technical Summary

Multiple SVG canvas output bugs: DrawPicture with fills, DrawText, and Canvas.Clear do not produce correct or consistent SVG output compared to raster rendering. Root cause likely in upstream Skia's SVGCanvas implementation which changed substantially between Skia versions backing SkiaSharp 1.x and 2.88.

### Rationale

Issue clearly describes broken visual output — not a usage question. Reporter provides screenshots showing SVG vs PNG discrepancies and an external test repo. SVG backend output quality depends heavily on upstream Skia's SVGDOM/SVGCanvas layer, and known limitations exist. The SKSvgCanvas C# wrapper is a thin pass-through, so the issue is likely in native Skia SVG serialization.

### Key Signals

- "DrawPicture issues with fill" — **issue body** (SVG canvas may not serialize picture fill attributes correctly.)
- "DrawText broken output" — **issue body** (Text rendering via SVGCanvas may produce malformed or missing SVG text elements.)
- "Canvas.Clear SVG and PNG inconsistency" — **issue body** (SKCanvas.Clear() writes a background rect in SVG mode but may behave differently than raster clear semantics.)
- "Last Known Good Version: 1.x" — **issue body** (Regression from old SkiaSharp 1.x SVG canvas to 2.88 — different Skia SVGCanvas implementation.)

### Code Investigation

| File | Lines | Relevance | Finding |
|------|-------|-----------|---------|
| `binding/SkiaSharp/SKSVG.cs` | 9-33 | direct | SKSvgCanvas.Create() is a thin wrapper over sk_svgcanvas_create_with_stream — returns a regular SKCanvas that serializes draw calls as SVG. All rendering logic lives in upstream Skia's SVGCanvas C++ implementation. |
| `binding/SkiaSharp/SKCanvas.cs` | 100-108 | related | SKCanvas.Clear() overloads call down to native SkCanvas::clear(). In the SVG backend this writes a background rect element; the behavior may differ from raster clear depending on Skia version. |
| `binding/SkiaSharp/SKCanvas.cs` | 512-535 | direct | DrawPicture passes an SKPicture and optional SKMatrix/SKPaint to native sk_canvas_draw_picture. SVG canvas may not expand picture draw calls into inline SVG elements correctly when a fill paint is applied. |

### Next Questions

- Do these bugs reproduce with a minimal self-contained snippet (no external repo required)?
- Are these issues reproducible on current 2.88.x or 3.x releases?
- Is the DrawText issue related to deprecated DrawText(string, SKPaint) vs new font API?

### Resolution Proposals

**Hypothesis:** Upstream Skia's SVGCanvas does not fully implement all draw operations as SVG elements (pictures, clear, text with certain attributes). SkiaSharp's thin wrapper cannot fix these without changes in the Skia C++ layer or a newer Skia version.

1. **Reproduce and identify each sub-issue** — investigation, confidence 0.85 (85%), cost/m, validated=untested
   - Extract minimal reproducers for each of the three reported SVG issues from the reporter's test repo and confirm whether they reproduce on the latest SkiaSharp release.
2. **Check upstream Skia SVGCanvas bug tracker** — investigation, confidence 0.80 (80%), cost/s, validated=untested
   - Search the Skia bug tracker (bugs.chromium.org) for known SVGCanvas limitations around DrawPicture and text rendering to determine if fixes are already upstream.

**Recommended proposal:** Reproduce and identify each sub-issue

**Why:** Each sub-issue needs individual repro before determining which are Skia-level limitations vs SkiaSharp-fixable bugs.

## Recommendations

### Actionability

| Field | Value |
|-------|-------|
| Suggested action | needs-investigation |
| Confidence | 0.80 (80%) |
| Reason | Multiple distinct SVG rendering bugs reported with screenshots and external test cases. Real bugs with partial repro — needs structured repro for each sub-issue. |
| Suggested repro platform | linux |

### Automatable Actions

| Type | Risk | Confidence | Description | Details |
|------|------|------------|-------------|---------|
| update-labels | low | 0.90 (90%) | Apply bug, SkiaSharp, SVG backend, and tenet labels | labels=type/bug, area/SkiaSharp, backend/SVG, tenet/compatibility, tenet/reliability |
| add-comment | medium | 0.80 (80%) | Acknowledge issue and request minimal repro for each sub-issue | — |

**Comment draft for `add-comment`:**

```markdown
Thank you for reporting and providing test cases!

This touches multiple SVG canvas behaviours that depend on upstream Skia's SVGCanvas implementation. To help investigate each sub-issue separately, could you provide a minimal self-contained code snippet (no external project needed) for each of the following?

1. **DrawPicture fill issue** — a short snippet showing the picture recorder, the fill paint, and the expected vs actual SVG output
2. **DrawText broken output** — which DrawText overload are you using? The `DrawText(string, SKPaint)` API is deprecated in 2.88; try `DrawText(string, float, float, SKFont, SKPaint)` instead
3. **Canvas.Clear inconsistency** — what specific visual difference do you see between SVG and PNG for `Clear()`?

Also, could you confirm whether these issues reproduce on the latest SkiaSharp release (3.x)?
```

<details>
<summary>Raw JSON</summary>

```json
{
  "meta": {
    "schemaVersion": "1.0",
    "number": 2602,
    "repo": "mono/SkiaSharp",
    "analyzedAt": "2026-05-02T09:45:00Z",
    "currentLabels": [
      "type/bug"
    ]
  },
  "summary": "Reporter identifies multiple SVG canvas output issues in SkiaSharp 2.88.3: DrawPicture fill not applied, DrawText producing broken output, and Canvas.Clear behaving inconsistently between SVG and PNG backends.",
  "classification": {
    "type": {
      "value": "type/bug",
      "confidence": 0.9
    },
    "area": {
      "value": "area/SkiaSharp",
      "confidence": 0.85
    },
    "backends": [
      "backend/SVG",
      "backend/Raster"
    ],
    "tenets": [
      "tenet/compatibility",
      "tenet/reliability"
    ]
  },
  "evidence": {
    "bugSignals": {
      "severity": "medium",
      "regressionClaimed": true,
      "errorType": "wrong-output",
      "errorMessage": "SVG output does not match PNG output; DrawPicture fill not applied; DrawText output broken; Canvas.Clear inconsistency between SVG and PNG",
      "reproQuality": "partial",
      "targetFrameworks": [
        "net-android",
        "net-ios"
      ]
    },
    "reproEvidence": {
      "stepsToReproduce": [
        "Create an SKSvgCanvas and draw using DrawPicture, DrawText, and Canvas.Clear",
        "Compare output SVG against equivalent PNG rendered canvas",
        "Observe visual differences: fill not applied in DrawPicture, missing or broken text in SVG, Clear behavior differs"
      ],
      "environmentDetails": "SkiaSharp 2.88.3, Visual Studio on Windows, Android 13, iOS 16",
      "repoLinks": [
        {
          "url": "https://github.com/EvgenyMuryshkin/SVGBugs/blob/main/SVGBugs/SVGBugsTests.cs",
          "description": "Reporter's test cases demonstrating each SVG bug"
        }
      ]
    },
    "versionAnalysis": {
      "mentionedVersions": [
        "2.88.3",
        "1.x"
      ],
      "workedIn": "1.x",
      "brokeIn": "2.88.x",
      "currentRelevance": "likely",
      "relevanceReason": "SKSvgCanvas in current codebase still delegates directly to Skia's SVG canvas; the SVG rendering pipeline has not been significantly reworked in recent versions."
    },
    "regression": {
      "isRegression": true,
      "confidence": 0.65,
      "reason": "Reporter states SVG rendering worked in 1.x. The SVG canvas backend in upstream Skia changed between the versions SkiaSharp 1.x and 2.x, making some behaviours differ.",
      "workedInVersion": "1.x",
      "brokeInVersion": "2.88.x"
    }
  },
  "analysis": {
    "summary": "Multiple SVG canvas output bugs: DrawPicture with fills, DrawText, and Canvas.Clear do not produce correct or consistent SVG output compared to raster rendering. Root cause likely in upstream Skia's SVGCanvas implementation which changed substantially between Skia versions backing SkiaSharp 1.x and 2.88.",
    "rationale": "Issue clearly describes broken visual output — not a usage question. Reporter provides screenshots showing SVG vs PNG discrepancies and an external test repo. SVG backend output quality depends heavily on upstream Skia's SVGDOM/SVGCanvas layer, and known limitations exist. The SKSvgCanvas C# wrapper is a thin pass-through, so the issue is likely in native Skia SVG serialization.",
    "keySignals": [
      {
        "text": "DrawPicture issues with fill",
        "source": "issue body",
        "interpretation": "SVG canvas may not serialize picture fill attributes correctly."
      },
      {
        "text": "DrawText broken output",
        "source": "issue body",
        "interpretation": "Text rendering via SVGCanvas may produce malformed or missing SVG text elements."
      },
      {
        "text": "Canvas.Clear SVG and PNG inconsistency",
        "source": "issue body",
        "interpretation": "SKCanvas.Clear() writes a background rect in SVG mode but may behave differently than raster clear semantics."
      },
      {
        "text": "Last Known Good Version: 1.x",
        "source": "issue body",
        "interpretation": "Regression from old SkiaSharp 1.x SVG canvas to 2.88 — different Skia SVGCanvas implementation."
      }
    ],
    "codeInvestigation": [
      {
        "file": "binding/SkiaSharp/SKSVG.cs",
        "lines": "9-33",
        "finding": "SKSvgCanvas.Create() is a thin wrapper over sk_svgcanvas_create_with_stream — returns a regular SKCanvas that serializes draw calls as SVG. All rendering logic lives in upstream Skia's SVGCanvas C++ implementation.",
        "relevance": "direct"
      },
      {
        "file": "binding/SkiaSharp/SKCanvas.cs",
        "lines": "100-108",
        "finding": "SKCanvas.Clear() overloads call down to native SkCanvas::clear(). In the SVG backend this writes a background rect element; the behavior may differ from raster clear depending on Skia version.",
        "relevance": "related"
      },
      {
        "file": "binding/SkiaSharp/SKCanvas.cs",
        "lines": "512-535",
        "finding": "DrawPicture passes an SKPicture and optional SKMatrix/SKPaint to native sk_canvas_draw_picture. SVG canvas may not expand picture draw calls into inline SVG elements correctly when a fill paint is applied.",
        "relevance": "direct"
      }
    ],
    "nextQuestions": [
      "Do these bugs reproduce with a minimal self-contained snippet (no external repo required)?",
      "Are these issues reproducible on current 2.88.x or 3.x releases?",
      "Is the DrawText issue related to deprecated DrawText(string, SKPaint) vs new font API?"
    ],
    "resolution": {
      "hypothesis": "Upstream Skia's SVGCanvas does not fully implement all draw operations as SVG elements (pictures, clear, text with certain attributes). SkiaSharp's thin wrapper cannot fix these without changes in the Skia C++ layer or a newer Skia version.",
      "proposals": [
        {
          "title": "Reproduce and identify each sub-issue",
          "description": "Extract minimal reproducers for each of the three reported SVG issues from the reporter's test repo and confirm whether they reproduce on the latest SkiaSharp release.",
          "category": "investigation",
          "confidence": 0.85,
          "effort": "cost/m",
          "validated": "untested"
        },
        {
          "title": "Check upstream Skia SVGCanvas bug tracker",
          "description": "Search the Skia bug tracker (bugs.chromium.org) for known SVGCanvas limitations around DrawPicture and text rendering to determine if fixes are already upstream.",
          "category": "investigation",
          "confidence": 0.8,
          "effort": "cost/s",
          "validated": "untested"
        }
      ],
      "recommendedProposal": "Reproduce and identify each sub-issue",
      "recommendedReason": "Each sub-issue needs individual repro before determining which are Skia-level limitations vs SkiaSharp-fixable bugs."
    }
  },
  "output": {
    "actionability": {
      "suggestedAction": "needs-investigation",
      "confidence": 0.8,
      "reason": "Multiple distinct SVG rendering bugs reported with screenshots and external test cases. Real bugs with partial repro — needs structured repro for each sub-issue.",
      "suggestedReproPlatform": "linux"
    },
    "actions": [
      {
        "type": "update-labels",
        "description": "Apply bug, SkiaSharp, SVG backend, and tenet labels",
        "risk": "low",
        "confidence": 0.9,
        "labels": [
          "type/bug",
          "area/SkiaSharp",
          "backend/SVG",
          "tenet/compatibility",
          "tenet/reliability"
        ]
      },
      {
        "type": "add-comment",
        "description": "Acknowledge issue and request minimal repro for each sub-issue",
        "risk": "medium",
        "confidence": 0.8,
        "comment": "Thank you for reporting and providing test cases!\n\nThis touches multiple SVG canvas behaviours that depend on upstream Skia's SVGCanvas implementation. To help investigate each sub-issue separately, could you provide a minimal self-contained code snippet (no external project needed) for each of the following?\n\n1. **DrawPicture fill issue** — a short snippet showing the picture recorder, the fill paint, and the expected vs actual SVG output\n2. **DrawText broken output** — which DrawText overload are you using? The `DrawText(string, SKPaint)` API is deprecated in 2.88; try `DrawText(string, float, float, SKFont, SKPaint)` instead\n3. **Canvas.Clear inconsistency** — what specific visual difference do you see between SVG and PNG for `Clear()`?\n\nAlso, could you confirm whether these issues reproduce on the latest SkiaSharp release (3.x)?"
      }
    ]
  }
}
```

</details>
