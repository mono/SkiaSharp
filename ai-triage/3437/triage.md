# Issue Triage Report — #3437

| Field | Value |
|-------|-------|
| Repository | mono/SkiaSharp |
| Analyzed | 2026-04-26T18:15:00Z |
| Type | type/bug (0.98 (98%)) |
| Area | area/SkiaSharp (0.92 (92%)) |
| Suggested action | needs-investigation (0.92 (92%)) |

**Issue Summary:** When using SKSvgCanvas in parallel tasks on Windows, <text> elements in SVG output intermittently lose their text content and x/y position attributes; this is a regression from SkiaSharp 2.88.9 and does not occur on Linux.

**Analysis:** Race condition in the Skia SVG backend on Windows causes concurrent DrawText calls to corrupt SVG <text> element attributes (x/y positions and text content). The 3.x DrawText path routes through SKTextBlob, which requires font metric computation via DirectWrite on Windows; shared mutable state in this pipeline is likely not thread-safe, producing intermittent empty position arrays and stripped text.

**Recommendations:** **needs-investigation** — Confirmed regression with complete repro and Windows-specific race condition. Root cause likely in upstream Skia SVG device or DirectWrite font metrics — needs native-level investigation on Windows.

---

## Classification

| Field | Value |
|-------|-------|
| Type | type/bug |
| Area | area/SkiaSharp |
| Platforms | os/Windows-Classic |
| Backends | backend/SVG |
| Tenets | tenet/reliability |
| Partner | — |
| Current labels | type/bug, os/Windows-Classic, area/SkiaSharp, tenet/reliability, backend/SVG |

## Evidence

### Reproduction

1. Create 5+ parallel Task.Run tasks, each creating an SKSvgCanvas over a MemoryStream
2. In each task, call DrawText 10+ times with unique text
3. Decode the output SVG and check for <text> elements with empty x/y attributes or empty text body
4. Observe intermittent corrupted <text> elements on Windows (Linux is unaffected)

**Environment:** Windows 11, Visual Studio Code, SkiaSharp 3.116.0 / 3.119.1 / 3.119.2-preview.1

**Repository links:**
- https://github.com/jankurianski/skiasharp-svg-text-missing-bug — Full minimal reproduction project

### Bug Signals

| Field | Value |
|-------|-------|
| Severity | high |
| Regression claimed | True |
| Error type | wrong-output |
| Error message | <text> elements output with empty x="" y="" attributes and no text content when SKSvgCanvas.DrawText is called from parallel tasks on Windows |
| Repro quality | complete |
| Target frameworks | net9.0 |

### Version Analysis

| Field | Value |
|-------|-------|
| Mentioned versions | 3.116.0, 3.119.1, 3.119.2-preview.1, 2.88.9 |
| Worked in | 2.88.9 |
| Broke in | 3.116.0 |
| Current relevance | likely |
| Relevance reason | All tested 3.x versions exhibit the bug; the SVG text rendering path was overhauled between 2.x and 3.x |

### Regression

| Field | Value |
|-------|-------|
| Is regression | True |
| Confidence | 0.95 (95%) |
| Reason | Reporter confirmed working in 2.88.9, broken in all 3.x versions tested (3.116.0, 3.119.1, 3.119.2-preview.1). The DrawText code path changed significantly between major versions. |
| Worked in version | 2.88.9 |
| Broke in version | 3.116.0 |

## Analysis

### Technical Summary

Race condition in the Skia SVG backend on Windows causes concurrent DrawText calls to corrupt SVG <text> element attributes (x/y positions and text content). The 3.x DrawText path routes through SKTextBlob, which requires font metric computation via DirectWrite on Windows; shared mutable state in this pipeline is likely not thread-safe, producing intermittent empty position arrays and stripped text.

### Rationale

This is a confirmed regression with a complete repro. The symptom (empty x/y and missing text) matches a race condition in glyph position serialization. The 2.x→3.x change introduced SKTextBlob-based text drawing, replacing the direct string-to-SVG path. The Windows-only nature points to DirectWrite or the Skia platform layer for Windows font metrics, not SkiaSharp's C# wrapper.

### Key Signals

- "The bug will occur even with 1 text element, but more increases the chance of hitting it" — **issue body** (Classic race condition — probability scales with thread count and operations per thread.)
- "This affects all SkiaSharp 3.x versions on Windows. Linux is unaffected." — **issue body** (Platform-specific race, likely in the Windows (DirectWrite) font metrics or SVG device path.)
- "I tested the bug on Ubuntu 22.04.2 LTS (using WSL) and the bug does not occur there." — **comment by jankurianski** (Confirms the race is in Windows-specific native code, not in the SkiaSharp C# wrapper.)
- "This is a regression from SkiaSharp 2.88.9 which does not have the bug." — **issue body** (The 3.x rewrite of the text path introduced the race.)

### Code Investigation

| File | Lines | Relevance | Finding |
|------|-------|-----------|---------|
| `binding/SkiaSharp/SKSVG.cs` | 17-33 | direct | SKSvgCanvas.Create() creates a new native SVG canvas per call with no synchronization — each canvas is independent, so the race is inside the native SVG device or font metrics layer, not in this wrapper. |
| `binding/SkiaSharp/SKCanvas.cs` | 639-660 | direct | DrawText(string, float, float, SKTextAlign, SKFont, SKPaint) creates an SKTextBlob via SKTextBlob.Create(text, font), which internally calls font metric APIs (GetGlyphs, GetGlyphPositions). The race condition likely occurs here: on Windows, these font metric calls share mutable DirectWrite state not protected by locks. |

### Workarounds

- Serialize SVG canvas creation and DrawText calls with a shared lock object to avoid concurrent access to the Windows font metric pipeline.
- Render SVGs sequentially instead of in parallel (Task.WhenAll → sequential loop) as a temporary workaround at the cost of throughput.
- Downgrade to SkiaSharp 2.88.9 if parallel SVG rendering on Windows is a hard requirement and latency is acceptable.

### Next Questions

- Is the race in Skia's DirectWrite font metric cache, the SVG device's text attribute serializer, or both?
- Does the bug occur with fonts other than Arial (system fonts vs embedded fonts)?
- Does pre-building the SKTextBlob outside the parallel section (shared read-only blob) avoid the race?
- Is this a known upstream Skia issue with the SVG device on Windows?

### Resolution Proposals

**Hypothesis:** Shared mutable state in Skia's Windows DirectWrite font metric pipeline (or the SVG device's text position serializer) is accessed concurrently by multiple threads without synchronization, corrupting the glyph position arrays that become the x/y SVG attributes.

1. **Serialize SVG rendering with a lock** — workaround, confidence 0.85 (85%), cost/xs, validated=untested
   - Wrap each SVG canvas creation + drawing block in a lock to serialize concurrent access to the Windows font metrics pipeline.

```csharp
private static readonly object _svgRenderLock = new object();

Func<int, string> drawSvgFunc = (int taskI) => {
    lock (_svgRenderLock) {
        using var svgStream = new MemoryStream();
        using (var canvas = SKSvgCanvas.Create(new SKRect(0, 0, 500, 500), svgStream)) {
            for (int i = 0; i < numTextElementsPerTask; i++) {
                using var paint = new SKPaint();
                using var typeface = SKTypeface.FromFamilyName("Arial");
                using var font = new SKFont(typeface, 14);
                canvas.DrawText($"Hello, World {i + 1}!", 10, i * 15 + 20, SKTextAlign.Left, font, paint);
            }
        }
        return Encoding.UTF8.GetString(svgStream.ToArray());
    }
};

// Tasks can still run in parallel but SVG rendering is serialized
await Task.WhenAll(Enumerable.Range(0, 5).Select(i => Task.Run(() => drawSvgFunc(i))));
```
2. **Investigate upstream Skia SVG device thread safety on Windows** — investigation, confidence 0.90 (90%), cost/m, validated=untested
   - Trace the SVG text rendering path in Skia (SkSVGDevice::onDrawTextBlob / SkSVGDevice::AutoElement) to identify the shared mutable state and determine whether it is a Skia upstream bug or a SkiaSharp wrapper issue.

**Recommended proposal:** Serialize SVG rendering with a lock

**Why:** Zero-effort workaround the reporter can apply immediately while the root cause is investigated. The lock eliminates concurrent access to the racy Windows font pipeline.

## Recommendations

### Actionability

| Field | Value |
|-------|-------|
| Suggested action | needs-investigation |
| Confidence | 0.92 (92%) |
| Reason | Confirmed regression with complete repro and Windows-specific race condition. Root cause likely in upstream Skia SVG device or DirectWrite font metrics — needs native-level investigation on Windows. |
| Suggested repro platform | windows |

### Automatable Actions

| Type | Risk | Confidence | Description | Details |
|------|------|------------|-------------|---------|
| update-labels | low | 0.98 (98%) | Labels already applied by prior triage bot; confirm correct labels are present | labels=type/bug, area/SkiaSharp, os/Windows-Classic, backend/SVG, tenet/reliability |
| add-comment | medium | 0.88 (88%) | Share workaround (serialize with lock) and request info about upstream Skia investigation | — |

**Comment draft for `add-comment`:**

```markdown
Thanks for the detailed report and the minimal reproduction repository!

This looks like a race condition in the Windows-specific (DirectWrite) font metrics pipeline inside Skia's SVG device, introduced in SkiaSharp 3.x when text rendering moved to the `SKTextBlob` path. Linux is unaffected because it uses a different font backend (FreeType).

**Immediate workaround** — serialize SVG rendering with a shared lock:

```csharp
private static readonly object _svgRenderLock = new object();

string RenderSvg(int taskI) {
    lock (_svgRenderLock) {
        using var svgStream = new MemoryStream();
        using (var canvas = SKSvgCanvas.Create(new SKRect(0, 0, 500, 500), svgStream)) {
            for (int i = 0; i < numTextElementsPerTask; i++) {
                using var paint = new SKPaint();
                using var typeface = SKTypeface.FromFamilyName("Arial");
                using var font = new SKFont(typeface, 14);
                canvas.DrawText($"Hello, World {i + 1}!", 10, i * 15 + 20, SKTextAlign.Left, font, paint);
            }
        }
        return Encoding.UTF8.GetString(svgStream.ToArray());
    }
}
```

This trades parallelism for correctness; SVG rendering will be serialized but all other work can still run in parallel.

We'll investigate the underlying native race condition.
```

<details>
<summary>Raw JSON</summary>

```json
{
  "meta": {
    "schemaVersion": "1.0",
    "number": 3437,
    "repo": "mono/SkiaSharp",
    "analyzedAt": "2026-04-26T18:15:00Z",
    "currentLabels": [
      "type/bug",
      "os/Windows-Classic",
      "area/SkiaSharp",
      "tenet/reliability",
      "backend/SVG"
    ]
  },
  "summary": "When using SKSvgCanvas in parallel tasks on Windows, <text> elements in SVG output intermittently lose their text content and x/y position attributes; this is a regression from SkiaSharp 2.88.9 and does not occur on Linux.",
  "classification": {
    "type": {
      "value": "type/bug",
      "confidence": 0.98
    },
    "area": {
      "value": "area/SkiaSharp",
      "confidence": 0.92
    },
    "platforms": [
      "os/Windows-Classic"
    ],
    "backends": [
      "backend/SVG"
    ],
    "tenets": [
      "tenet/reliability"
    ]
  },
  "evidence": {
    "bugSignals": {
      "severity": "high",
      "regressionClaimed": true,
      "errorType": "wrong-output",
      "errorMessage": "<text> elements output with empty x=\"\" y=\"\" attributes and no text content when SKSvgCanvas.DrawText is called from parallel tasks on Windows",
      "reproQuality": "complete",
      "targetFrameworks": [
        "net9.0"
      ]
    },
    "reproEvidence": {
      "stepsToReproduce": [
        "Create 5+ parallel Task.Run tasks, each creating an SKSvgCanvas over a MemoryStream",
        "In each task, call DrawText 10+ times with unique text",
        "Decode the output SVG and check for <text> elements with empty x/y attributes or empty text body",
        "Observe intermittent corrupted <text> elements on Windows (Linux is unaffected)"
      ],
      "environmentDetails": "Windows 11, Visual Studio Code, SkiaSharp 3.116.0 / 3.119.1 / 3.119.2-preview.1",
      "repoLinks": [
        {
          "url": "https://github.com/jankurianski/skiasharp-svg-text-missing-bug",
          "description": "Full minimal reproduction project"
        }
      ]
    },
    "versionAnalysis": {
      "mentionedVersions": [
        "3.116.0",
        "3.119.1",
        "3.119.2-preview.1",
        "2.88.9"
      ],
      "workedIn": "2.88.9",
      "brokeIn": "3.116.0",
      "currentRelevance": "likely",
      "relevanceReason": "All tested 3.x versions exhibit the bug; the SVG text rendering path was overhauled between 2.x and 3.x"
    },
    "regression": {
      "isRegression": true,
      "confidence": 0.95,
      "reason": "Reporter confirmed working in 2.88.9, broken in all 3.x versions tested (3.116.0, 3.119.1, 3.119.2-preview.1). The DrawText code path changed significantly between major versions.",
      "workedInVersion": "2.88.9",
      "brokeInVersion": "3.116.0"
    }
  },
  "analysis": {
    "summary": "Race condition in the Skia SVG backend on Windows causes concurrent DrawText calls to corrupt SVG <text> element attributes (x/y positions and text content). The 3.x DrawText path routes through SKTextBlob, which requires font metric computation via DirectWrite on Windows; shared mutable state in this pipeline is likely not thread-safe, producing intermittent empty position arrays and stripped text.",
    "rationale": "This is a confirmed regression with a complete repro. The symptom (empty x/y and missing text) matches a race condition in glyph position serialization. The 2.x→3.x change introduced SKTextBlob-based text drawing, replacing the direct string-to-SVG path. The Windows-only nature points to DirectWrite or the Skia platform layer for Windows font metrics, not SkiaSharp's C# wrapper.",
    "keySignals": [
      {
        "text": "The bug will occur even with 1 text element, but more increases the chance of hitting it",
        "source": "issue body",
        "interpretation": "Classic race condition — probability scales with thread count and operations per thread."
      },
      {
        "text": "This affects all SkiaSharp 3.x versions on Windows. Linux is unaffected.",
        "source": "issue body",
        "interpretation": "Platform-specific race, likely in the Windows (DirectWrite) font metrics or SVG device path."
      },
      {
        "text": "I tested the bug on Ubuntu 22.04.2 LTS (using WSL) and the bug does not occur there.",
        "source": "comment by jankurianski",
        "interpretation": "Confirms the race is in Windows-specific native code, not in the SkiaSharp C# wrapper."
      },
      {
        "text": "This is a regression from SkiaSharp 2.88.9 which does not have the bug.",
        "source": "issue body",
        "interpretation": "The 3.x rewrite of the text path introduced the race."
      }
    ],
    "codeInvestigation": [
      {
        "file": "binding/SkiaSharp/SKSVG.cs",
        "lines": "17-33",
        "finding": "SKSvgCanvas.Create() creates a new native SVG canvas per call with no synchronization — each canvas is independent, so the race is inside the native SVG device or font metrics layer, not in this wrapper.",
        "relevance": "direct"
      },
      {
        "file": "binding/SkiaSharp/SKCanvas.cs",
        "lines": "639-660",
        "finding": "DrawText(string, float, float, SKTextAlign, SKFont, SKPaint) creates an SKTextBlob via SKTextBlob.Create(text, font), which internally calls font metric APIs (GetGlyphs, GetGlyphPositions). The race condition likely occurs here: on Windows, these font metric calls share mutable DirectWrite state not protected by locks.",
        "relevance": "direct"
      }
    ],
    "workarounds": [
      "Serialize SVG canvas creation and DrawText calls with a shared lock object to avoid concurrent access to the Windows font metric pipeline.",
      "Render SVGs sequentially instead of in parallel (Task.WhenAll → sequential loop) as a temporary workaround at the cost of throughput.",
      "Downgrade to SkiaSharp 2.88.9 if parallel SVG rendering on Windows is a hard requirement and latency is acceptable."
    ],
    "nextQuestions": [
      "Is the race in Skia's DirectWrite font metric cache, the SVG device's text attribute serializer, or both?",
      "Does the bug occur with fonts other than Arial (system fonts vs embedded fonts)?",
      "Does pre-building the SKTextBlob outside the parallel section (shared read-only blob) avoid the race?",
      "Is this a known upstream Skia issue with the SVG device on Windows?"
    ],
    "resolution": {
      "hypothesis": "Shared mutable state in Skia's Windows DirectWrite font metric pipeline (or the SVG device's text position serializer) is accessed concurrently by multiple threads without synchronization, corrupting the glyph position arrays that become the x/y SVG attributes.",
      "proposals": [
        {
          "title": "Serialize SVG rendering with a lock",
          "description": "Wrap each SVG canvas creation + drawing block in a lock to serialize concurrent access to the Windows font metrics pipeline.",
          "category": "workaround",
          "codeSnippet": "private static readonly object _svgRenderLock = new object();\n\nFunc<int, string> drawSvgFunc = (int taskI) => {\n    lock (_svgRenderLock) {\n        using var svgStream = new MemoryStream();\n        using (var canvas = SKSvgCanvas.Create(new SKRect(0, 0, 500, 500), svgStream)) {\n            for (int i = 0; i < numTextElementsPerTask; i++) {\n                using var paint = new SKPaint();\n                using var typeface = SKTypeface.FromFamilyName(\"Arial\");\n                using var font = new SKFont(typeface, 14);\n                canvas.DrawText($\"Hello, World {i + 1}!\", 10, i * 15 + 20, SKTextAlign.Left, font, paint);\n            }\n        }\n        return Encoding.UTF8.GetString(svgStream.ToArray());\n    }\n};\n\n// Tasks can still run in parallel but SVG rendering is serialized\nawait Task.WhenAll(Enumerable.Range(0, 5).Select(i => Task.Run(() => drawSvgFunc(i))));",
          "confidence": 0.85,
          "effort": "cost/xs",
          "validated": "untested"
        },
        {
          "title": "Investigate upstream Skia SVG device thread safety on Windows",
          "description": "Trace the SVG text rendering path in Skia (SkSVGDevice::onDrawTextBlob / SkSVGDevice::AutoElement) to identify the shared mutable state and determine whether it is a Skia upstream bug or a SkiaSharp wrapper issue.",
          "category": "investigation",
          "confidence": 0.9,
          "effort": "cost/m",
          "validated": "untested"
        }
      ],
      "recommendedProposal": "Serialize SVG rendering with a lock",
      "recommendedReason": "Zero-effort workaround the reporter can apply immediately while the root cause is investigated. The lock eliminates concurrent access to the racy Windows font pipeline."
    }
  },
  "output": {
    "actionability": {
      "suggestedAction": "needs-investigation",
      "confidence": 0.92,
      "reason": "Confirmed regression with complete repro and Windows-specific race condition. Root cause likely in upstream Skia SVG device or DirectWrite font metrics — needs native-level investigation on Windows.",
      "suggestedReproPlatform": "windows"
    },
    "actions": [
      {
        "type": "update-labels",
        "description": "Labels already applied by prior triage bot; confirm correct labels are present",
        "risk": "low",
        "confidence": 0.98,
        "labels": [
          "type/bug",
          "area/SkiaSharp",
          "os/Windows-Classic",
          "backend/SVG",
          "tenet/reliability"
        ]
      },
      {
        "type": "add-comment",
        "description": "Share workaround (serialize with lock) and request info about upstream Skia investigation",
        "risk": "medium",
        "confidence": 0.88,
        "comment": "Thanks for the detailed report and the minimal reproduction repository!\n\nThis looks like a race condition in the Windows-specific (DirectWrite) font metrics pipeline inside Skia's SVG device, introduced in SkiaSharp 3.x when text rendering moved to the `SKTextBlob` path. Linux is unaffected because it uses a different font backend (FreeType).\n\n**Immediate workaround** — serialize SVG rendering with a shared lock:\n\n```csharp\nprivate static readonly object _svgRenderLock = new object();\n\nstring RenderSvg(int taskI) {\n    lock (_svgRenderLock) {\n        using var svgStream = new MemoryStream();\n        using (var canvas = SKSvgCanvas.Create(new SKRect(0, 0, 500, 500), svgStream)) {\n            for (int i = 0; i < numTextElementsPerTask; i++) {\n                using var paint = new SKPaint();\n                using var typeface = SKTypeface.FromFamilyName(\"Arial\");\n                using var font = new SKFont(typeface, 14);\n                canvas.DrawText($\"Hello, World {i + 1}!\", 10, i * 15 + 20, SKTextAlign.Left, font, paint);\n            }\n        }\n        return Encoding.UTF8.GetString(svgStream.ToArray());\n    }\n}\n```\n\nThis trades parallelism for correctness; SVG rendering will be serialized but all other work can still run in parallel.\n\nWe'll investigate the underlying native race condition."
      }
    ]
  }
}
```

</details>
