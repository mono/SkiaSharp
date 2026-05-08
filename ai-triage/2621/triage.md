# Issue Triage Report — #2621

| Field | Value |
|-------|-------|
| Repository | mono/SkiaSharp |
| Analyzed | 2026-04-25T22:00:00Z |
| Type | type/feature-request (0.92 (92%)) |
| Area | area/SkiaSharp (0.90 (90%)) |
| Suggested action | keep-open (0.80 (80%)) |

**Issue Summary:** Reporter requests a way to reset or control the auto-incrementing clipPath ID counter used by the SVG canvas, because the counter persists across multiple SVG generations in the same process, producing non-deterministic SVG output that breaks snapshot-based unit tests.

**Analysis:** The auto-incrementing clipPath ID counter lives inside Skia's upstream C++ SVG device implementation. SkiaSharp's C API shim only exposes sk_svgcanvas_create_with_stream(bounds, stream) with no means to influence ID generation. The feature requires either a counter-reset API in Skia's SVGCanvas or a new SkiaSharp-level wrapper that post-processes the output. A reporter-provided post-processing workaround exists.

**Recommendations:** **keep-open** — Valid feature request with a clear use case (deterministic SVG output for snapshot tests). The fix requires investigation into upstream Skia's SVG device; a workaround exists. Keep open for tracking.

---

## Classification

| Field | Value |
|-------|-------|
| Type | type/feature-request |
| Area | area/SkiaSharp |
| Platforms | — |
| Backends | backend/SVG |
| Tenets | tenet/compatibility |
| Partner | — |
| Current labels | type/feature-request |

## Evidence

### Reproduction

1. Create multiple SVG canvases using SKSvgCanvas.Create() in sequence (e.g., as separate unit tests)
2. Each SVG uses a clip region, so Skia emits a <clipPath id='cl_N'> element
3. Run tests individually: the clipPath ID is always cl_3
4. Run the entire test class at once: the IDs increment (cl_3, cl_6, cl_9, ...)
5. Compare the SVG output bytes: they differ across runs, breaking snapshot tests

**Environment:** SkiaSharp 2.88.x; demonstrated to affect unit test environments where multiple SVG canvases are created in sequence

**Related issues:** #2602

**Repository links:**
- https://github.com/EvgenyMuryshkin/SVGBugs/blob/main/SVGBugs/SVGClipPathTests.cs — Minimal repro showing clipPath ID increment across tests

### Version Analysis

| Field | Value |
|-------|-------|
| Mentioned versions | 2.88 |
| Worked in | — |
| Broke in | — |
| Current relevance | likely |
| Relevance reason | The SVG canvas C API only exposes sk_svgcanvas_create_with_stream with no parameters to control ID generation; no reset mechanism has been added since the issue was filed. |

## Analysis

### Technical Summary

The auto-incrementing clipPath ID counter lives inside Skia's upstream C++ SVG device implementation. SkiaSharp's C API shim only exposes sk_svgcanvas_create_with_stream(bounds, stream) with no means to influence ID generation. The feature requires either a counter-reset API in Skia's SVGCanvas or a new SkiaSharp-level wrapper that post-processes the output. A reporter-provided post-processing workaround exists.

### Rationale

The issue is filed as a feature request and correctly classified. The request is well-scoped (deterministic clipPath IDs for snapshot testing), there is a clear workaround, and the fix path requires changes upstream in Skia's SVG device or a new C API parameter. The maintainer's comment ('field counter is not static') suggests the counter is instance-scoped in the C++ class, but the repro shows shared/global behavior — consistent with a static counter in Skia's SVGDevice or a global atomic.

### Key Signals

- "clipPath id property is being autoincrement of every run, resulting in different SVG content produced on every run" — **issue body** (Non-deterministic SVG output due to shared ID counter — breaks snapshot-based test comparisons.)
- "If you run them one by one, result clip path index is always cl_3, but if you run whole test class at once, they start to increment" — **comment by reporter** (Confirms the counter persists across SKSvgCanvas instances within the same process — consistent with a static/global counter in Skia C++.)
- "the field counter is not a static so this is interesting" — **comment by mattleibow** (Maintainer looked at the C++ code and thought it was instance-scoped; the repro evidence contradicts this, suggesting the static might be elsewhere in the call chain.)

### Code Investigation

| File | Lines | Relevance | Finding |
|------|-------|-----------|---------|
| `binding/SkiaSharp/SKSVG.cs` | 17-33 | direct | SKSvgCanvas.Create() wraps sk_svgcanvas_create_with_stream(bounds, stream) with no additional parameters — there is no API surface in SkiaSharp to influence clipPath ID generation or reset any counter. |
| `binding/SkiaSharp/SkiaApi.generated.cs` | — | direct | sk_svgcanvas_create_with_stream is the only SVG canvas API exposed — no flags, no ID seed parameter, no reset function exists in the current C API shim. |

### Workarounds

- Post-process generated SVG XML using XDocument to renumber clipPath IDs to deterministic values (provided by reporter in issue body)
- Run each test in an isolated process (e.g., xUnit process isolation) to ensure the counter always starts from zero

### Next Questions

- Does the counter reset between AppDomain reloads, or is it a native-level static that persists for the process lifetime?
- Is there a way in the Skia C++ SVGCanvas API to pass an ID prefix or starting index (e.g., SkSVGCanvas::Make flags)?
- Would adding a new sk_svgcanvas_create_with_stream_and_flags C API function (exposing an id-seed or id-prefix option) be accepted upstream?

### Resolution Proposals

**Hypothesis:** The clipPath ID counter is a native static/global in Skia's SVG device. Resetting it requires either an upstream Skia API change or a SkiaSharp-level post-processing wrapper.

1. **Post-process SVG to normalize clipPath IDs** — workaround, confidence 0.90 (90%), cost/xs, validated=untested
   - Parse the SVG output with XDocument and renumber all clipPath id attributes to deterministic values before comparison. This is the reporter's own workaround and requires no SkiaSharp changes.

```csharp
public static string NormalizeClipPathIds(string svg)
{
    var doc = XDocument.Parse(svg);
    var clipPaths = doc.Root.Elements().Where(e => e.Name.LocalName == "clipPath").ToList();
    int counter = 0;
    foreach (var clipPath in clipPaths)
    {
        var id = clipPath.Attribute("id");
        if (id != null)
            svg = svg.Replace(id.Value, $"cp_{counter++}");
    }
    return svg;
}
```
2. **Investigate Skia SVG device for counter reset mechanism** — investigation, confidence 0.70 (70%), cost/m, validated=untested
   - Examine the upstream Skia SVGDevice/SVGCanvas C++ source to locate the counter (likely SkSVGDevice or SkDocument_SVG). Determine if a flag or seed parameter can be exposed via the C API shim.

**Recommended proposal:** Post-process SVG to normalize clipPath IDs

**Why:** Immediately actionable by the reporter without any SkiaSharp changes; deterministic ID normalization is a common pattern for SVG snapshot testing.

## Recommendations

### Actionability

| Field | Value |
|-------|-------|
| Suggested action | keep-open |
| Confidence | 0.80 (80%) |
| Reason | Valid feature request with a clear use case (deterministic SVG output for snapshot tests). The fix requires investigation into upstream Skia's SVG device; a workaround exists. Keep open for tracking. |
| Suggested repro platform | linux |

### Automatable Actions

| Type | Risk | Confidence | Description | Details |
|------|------|------------|-------------|---------|
| update-labels | low | 0.92 (92%) | Apply feature-request, SkiaSharp area, and SVG backend labels | labels=type/feature-request, area/SkiaSharp, backend/SVG |
| add-comment | medium | 0.82 (82%) | Acknowledge the request, confirm workaround, note that the fix requires upstream Skia investigation | — |

**Comment draft for `add-comment`:**

```markdown
Thanks for the detailed repro! The clipPath ID counter lives inside Skia's native SVG device implementation — there's currently no SkiaSharp API to reset or seed it.

As a workaround for snapshot testing, you can post-process the SVG output to normalize the IDs before comparison:

```csharp
public static string NormalizeClipPathIds(string svg)
{
    var doc = XDocument.Parse(svg);
    var clipPaths = doc.Root.Elements().Where(e => e.Name.LocalName == "clipPath").ToList();
    int counter = 0;
    foreach (var clipPath in clipPaths)
    {
        var id = clipPath.Attribute("id");
        if (id != null)
            svg = svg.Replace(id.Value, $"cp_{counter++}");
    }
    return svg;
}
```

Alternatively, you can run each test in an isolated process so the counter always starts from zero.

We'll keep this open to track adding deterministic ID support — that would require a change in how the SVG canvas exposes ID generation options.
```

<details>
<summary>Raw JSON</summary>

```json
{
  "meta": {
    "schemaVersion": "1.0",
    "number": 2621,
    "repo": "mono/SkiaSharp",
    "analyzedAt": "2026-04-25T22:00:00Z",
    "currentLabels": [
      "type/feature-request"
    ]
  },
  "summary": "Reporter requests a way to reset or control the auto-incrementing clipPath ID counter used by the SVG canvas, because the counter persists across multiple SVG generations in the same process, producing non-deterministic SVG output that breaks snapshot-based unit tests.",
  "classification": {
    "type": {
      "value": "type/feature-request",
      "confidence": 0.92
    },
    "area": {
      "value": "area/SkiaSharp",
      "confidence": 0.9
    },
    "backends": [
      "backend/SVG"
    ],
    "tenets": [
      "tenet/compatibility"
    ]
  },
  "evidence": {
    "reproEvidence": {
      "stepsToReproduce": [
        "Create multiple SVG canvases using SKSvgCanvas.Create() in sequence (e.g., as separate unit tests)",
        "Each SVG uses a clip region, so Skia emits a <clipPath id='cl_N'> element",
        "Run tests individually: the clipPath ID is always cl_3",
        "Run the entire test class at once: the IDs increment (cl_3, cl_6, cl_9, ...)",
        "Compare the SVG output bytes: they differ across runs, breaking snapshot tests"
      ],
      "environmentDetails": "SkiaSharp 2.88.x; demonstrated to affect unit test environments where multiple SVG canvases are created in sequence",
      "relatedIssues": [
        2602
      ],
      "repoLinks": [
        {
          "url": "https://github.com/EvgenyMuryshkin/SVGBugs/blob/main/SVGBugs/SVGClipPathTests.cs",
          "description": "Minimal repro showing clipPath ID increment across tests"
        }
      ]
    },
    "versionAnalysis": {
      "mentionedVersions": [
        "2.88"
      ],
      "currentRelevance": "likely",
      "relevanceReason": "The SVG canvas C API only exposes sk_svgcanvas_create_with_stream with no parameters to control ID generation; no reset mechanism has been added since the issue was filed."
    }
  },
  "analysis": {
    "summary": "The auto-incrementing clipPath ID counter lives inside Skia's upstream C++ SVG device implementation. SkiaSharp's C API shim only exposes sk_svgcanvas_create_with_stream(bounds, stream) with no means to influence ID generation. The feature requires either a counter-reset API in Skia's SVGCanvas or a new SkiaSharp-level wrapper that post-processes the output. A reporter-provided post-processing workaround exists.",
    "rationale": "The issue is filed as a feature request and correctly classified. The request is well-scoped (deterministic clipPath IDs for snapshot testing), there is a clear workaround, and the fix path requires changes upstream in Skia's SVG device or a new C API parameter. The maintainer's comment ('field counter is not static') suggests the counter is instance-scoped in the C++ class, but the repro shows shared/global behavior — consistent with a static counter in Skia's SVGDevice or a global atomic.",
    "keySignals": [
      {
        "text": "clipPath id property is being autoincrement of every run, resulting in different SVG content produced on every run",
        "source": "issue body",
        "interpretation": "Non-deterministic SVG output due to shared ID counter — breaks snapshot-based test comparisons."
      },
      {
        "text": "If you run them one by one, result clip path index is always cl_3, but if you run whole test class at once, they start to increment",
        "source": "comment by reporter",
        "interpretation": "Confirms the counter persists across SKSvgCanvas instances within the same process — consistent with a static/global counter in Skia C++."
      },
      {
        "text": "the field counter is not a static so this is interesting",
        "source": "comment by mattleibow",
        "interpretation": "Maintainer looked at the C++ code and thought it was instance-scoped; the repro evidence contradicts this, suggesting the static might be elsewhere in the call chain."
      }
    ],
    "codeInvestigation": [
      {
        "file": "binding/SkiaSharp/SKSVG.cs",
        "lines": "17-33",
        "finding": "SKSvgCanvas.Create() wraps sk_svgcanvas_create_with_stream(bounds, stream) with no additional parameters — there is no API surface in SkiaSharp to influence clipPath ID generation or reset any counter.",
        "relevance": "direct"
      },
      {
        "file": "binding/SkiaSharp/SkiaApi.generated.cs",
        "finding": "sk_svgcanvas_create_with_stream is the only SVG canvas API exposed — no flags, no ID seed parameter, no reset function exists in the current C API shim.",
        "relevance": "direct"
      }
    ],
    "workarounds": [
      "Post-process generated SVG XML using XDocument to renumber clipPath IDs to deterministic values (provided by reporter in issue body)",
      "Run each test in an isolated process (e.g., xUnit process isolation) to ensure the counter always starts from zero"
    ],
    "nextQuestions": [
      "Does the counter reset between AppDomain reloads, or is it a native-level static that persists for the process lifetime?",
      "Is there a way in the Skia C++ SVGCanvas API to pass an ID prefix or starting index (e.g., SkSVGCanvas::Make flags)?",
      "Would adding a new sk_svgcanvas_create_with_stream_and_flags C API function (exposing an id-seed or id-prefix option) be accepted upstream?"
    ],
    "resolution": {
      "hypothesis": "The clipPath ID counter is a native static/global in Skia's SVG device. Resetting it requires either an upstream Skia API change or a SkiaSharp-level post-processing wrapper.",
      "proposals": [
        {
          "title": "Post-process SVG to normalize clipPath IDs",
          "description": "Parse the SVG output with XDocument and renumber all clipPath id attributes to deterministic values before comparison. This is the reporter's own workaround and requires no SkiaSharp changes.",
          "category": "workaround",
          "codeSnippet": "public static string NormalizeClipPathIds(string svg)\n{\n    var doc = XDocument.Parse(svg);\n    var clipPaths = doc.Root.Elements().Where(e => e.Name.LocalName == \"clipPath\").ToList();\n    int counter = 0;\n    foreach (var clipPath in clipPaths)\n    {\n        var id = clipPath.Attribute(\"id\");\n        if (id != null)\n            svg = svg.Replace(id.Value, $\"cp_{counter++}\");\n    }\n    return svg;\n}",
          "confidence": 0.9,
          "effort": "cost/xs",
          "validated": "untested"
        },
        {
          "title": "Investigate Skia SVG device for counter reset mechanism",
          "description": "Examine the upstream Skia SVGDevice/SVGCanvas C++ source to locate the counter (likely SkSVGDevice or SkDocument_SVG). Determine if a flag or seed parameter can be exposed via the C API shim.",
          "category": "investigation",
          "confidence": 0.7,
          "effort": "cost/m",
          "validated": "untested"
        }
      ],
      "recommendedProposal": "Post-process SVG to normalize clipPath IDs",
      "recommendedReason": "Immediately actionable by the reporter without any SkiaSharp changes; deterministic ID normalization is a common pattern for SVG snapshot testing."
    }
  },
  "output": {
    "actionability": {
      "suggestedAction": "keep-open",
      "confidence": 0.8,
      "reason": "Valid feature request with a clear use case (deterministic SVG output for snapshot tests). The fix requires investigation into upstream Skia's SVG device; a workaround exists. Keep open for tracking.",
      "suggestedReproPlatform": "linux"
    },
    "actions": [
      {
        "type": "update-labels",
        "description": "Apply feature-request, SkiaSharp area, and SVG backend labels",
        "risk": "low",
        "confidence": 0.92,
        "labels": [
          "type/feature-request",
          "area/SkiaSharp",
          "backend/SVG"
        ]
      },
      {
        "type": "add-comment",
        "description": "Acknowledge the request, confirm workaround, note that the fix requires upstream Skia investigation",
        "risk": "medium",
        "confidence": 0.82,
        "comment": "Thanks for the detailed repro! The clipPath ID counter lives inside Skia's native SVG device implementation — there's currently no SkiaSharp API to reset or seed it.\n\nAs a workaround for snapshot testing, you can post-process the SVG output to normalize the IDs before comparison:\n\n```csharp\npublic static string NormalizeClipPathIds(string svg)\n{\n    var doc = XDocument.Parse(svg);\n    var clipPaths = doc.Root.Elements().Where(e => e.Name.LocalName == \"clipPath\").ToList();\n    int counter = 0;\n    foreach (var clipPath in clipPaths)\n    {\n        var id = clipPath.Attribute(\"id\");\n        if (id != null)\n            svg = svg.Replace(id.Value, $\"cp_{counter++}\");\n    }\n    return svg;\n}\n```\n\nAlternatively, you can run each test in an isolated process so the counter always starts from zero.\n\nWe'll keep this open to track adding deterministic ID support — that would require a change in how the SVG canvas exposes ID generation options."
      }
    ]
  }
}
```

</details>
