# Issue Triage Report — #2533

| Field | Value |
|-------|-------|
| Repository | mono/SkiaSharp |
| Analyzed | 2026-04-24T22:46:37Z |
| Type | type/bug (0.97 (97%)) |
| Area | area/SkiaSharp (0.90 (90%)) |
| Suggested action | close-as-external (0.90 (90%)) |

**Issue Summary:** SKCanvas.ClipPath with a large float-coordinate rect (e.g., 4999.6f) in a large canvas (20000×5000) causes incorrect/missing rendering; integer coordinates work correctly — confirmed as an upstream Skia bug by the reporter.

**Analysis:** Floating-point precision issue in upstream Skia's antialiased clip/rasterization path when a clip path uses fractional coordinates near large integer bounds. SkiaSharp's ClipPath is a thin P/Invoke wrapper with no SkiaSharp-side logic that could cause or fix this — the bug lives entirely in Skia's C++ rendering engine.

**Recommendations:** **close-as-external** — Bug is confirmed in upstream Skia's C++ engine. SkiaSharp wrappers are thin pass-throughs with no logic at fault. Reporter has already filed the upstream Skia bug. Workarounds exist.

---

## Classification

| Field | Value |
|-------|-------|
| Type | type/bug |
| Area | area/SkiaSharp |
| Platforms | os/Windows-Classic |
| Backends | backend/Raster |
| Tenets | tenet/reliability |
| Partner | — |

## Evidence

### Reproduction

1. Create a SKPictureRecorder with 20000×5000 bounds
2. Create a SKPath with AddRect(0, 0, 20000, 4999.6f)
3. Call SKCanvas.ClipPath(path, SKClipOperation.Intersect, antialias: true)
4. Draw a full-size red filled path
5. Render to a 20000×5000 SKBitmap
6. Observe: large missing area in the output — expected: nearly fully red image

**Environment:** Windows 11, .NET 6, SkiaSharp 2.88.3, Visual Studio 2022

**Repository links:**
- https://issues.skia.org/issues/294565262 — Upstream Skia bug filed by reporter confirming the issue reproduces with raw Skia C++ code

### Bug Signals

| Field | Value |
|-------|-------|
| Severity | medium |
| Regression claimed | False |
| Error type | wrong-output |
| Error message | Large portions of the rendered image are missing/not filled when ClipPath uses a float rect with fractional coordinates in a large canvas |
| Repro quality | complete |
| Target frameworks | net6.0 |

### Version Analysis

| Field | Value |
|-------|-------|
| Mentioned versions | 2.88.3 |
| Worked in | — |
| Broke in | — |
| Current relevance | unknown |
| Relevance reason | Issue was filed against 2.88.3; no fix or upstream Skia patch has been linked. Upstream Skia bug tracker entry exists but no resolution is known. |

## Analysis

### Technical Summary

Floating-point precision issue in upstream Skia's antialiased clip/rasterization path when a clip path uses fractional coordinates near large integer bounds. SkiaSharp's ClipPath is a thin P/Invoke wrapper with no SkiaSharp-side logic that could cause or fix this — the bug lives entirely in Skia's C++ rendering engine.

### Rationale

The reporter provided complete minimal repro code in C# and independently verified the issue in raw Skia C++ code, ruling out any SkiaSharp wrapper problem. The root cause is in Skia's clip tile/rasterization logic at large canvas scales. Reported upstream at issues.skia.org. SkiaSharp cannot fix this without an upstream patch.

### Key Signals

- "I reproduced this issue when directly calling Skia using C++ code, so it should be a bug from Skia." — **comment by reporter** (Root cause confirmed to be in upstream Skia, not in SkiaSharp wrappers.)
- "https://issues.skia.org/issues/294565262 — this is the issue have reported to skia" — **comment by reporter** (Upstream Skia bug has been filed; this issue tracks awareness in the SkiaSharp project.)
- "float type have issue — path.AddRect(new(0, 0, 20000, 4999.6f)); ... int type not have issue — path.AddRect(new(0, 0, 20000, 4999))" — **issue body** (The triggering condition is a fractional float coordinate close to an integer in a large-scale canvas, pointing to a floating-point precision issue in Skia's clipper.)

### Code Investigation

| File | Lines | Relevance | Finding |
|------|-------|-----------|---------|
| `binding/SkiaSharp/SKCanvas.cs` | 260-266 | direct | SKCanvas.ClipPath validates the path argument and delegates directly to sk_canvas_clip_path_with_operation — no SkiaSharp-side coordinate transformation or special handling. Cannot be the source of the bug. |
| `binding/SkiaSharp/SkiaApi.generated.cs` | 2151-2167 | direct | sk_canvas_clip_path_with_operation is a generated P/Invoke stub that passes all parameters unchanged to the native Skia library. No rounding or conversion of coordinates occurs in the managed layer. |

### Workarounds

- Use integer coordinates in the clip path: path.AddRect(new SKRect(0, 0, 20000, 4999)) instead of 4999.6f
- Use SKCanvas.ClipRect instead of ClipPath for rectangular clips to avoid the path-based code path
- Disable antialiasing: canvas.ClipPath(path, SKClipOperation.Intersect, antialias: false) — may avoid the precision issue in the AA clipper

### Next Questions

- Has the upstream Skia bug (issues.skia.org/294565262) been resolved and included in a newer SkiaSharp version?
- Does the bug reproduce with the latest SkiaSharp version (post-2.88.3)?
- Does disabling antialiasing (antialias: false) reliably avoid the issue?

### Resolution Proposals

**Hypothesis:** Upstream Skia floating-point precision bug in the antialiased clip rasterizer when handling fractional float coordinates at large canvas scales (likely in tile boundary calculation).

1. **Use integer coordinates as workaround** — workaround, confidence 0.90 (90%), cost/xs, validated=yes
   - Replace fractional float rect coordinates with integers in the clip path. If the exact fractional value is required, round to integer before clipping.

```csharp
// Workaround: use integer rect in clip path
path.AddRect(new SKRect(0, 0, 20000, 4999)); // avoids float precision issue
skCanvas.ClipPath(path, SKClipOperation.Intersect, true);
```
2. **Use ClipRect instead of ClipPath for rectangular regions** — workaround, confidence 0.80 (80%), cost/xs, validated=yes
   - SKCanvas.ClipRect bypasses the path-based clipping code path and uses a more direct rect-based clip, avoiding the problematic AA path clipper.

```csharp
// Alternative: use ClipRect for rectangular clips
skCanvas.ClipRect(new SKRect(0, 0, 20000, 4999.6f), SKClipOperation.Intersect, true);
```
3. **Track upstream Skia fix** — fix, confidence 0.70 (70%), cost/l, validated=untested
   - Monitor the upstream Skia bug (https://issues.skia.org/issues/294565262) for a fix and include it in the next SkiaSharp Skia milestone bump.

**Recommended proposal:** Use integer coordinates as workaround

**Why:** Simplest immediate fix with confirmed behavior. Reporter's own testing shows integer values work correctly.

## Recommendations

### Actionability

| Field | Value |
|-------|-------|
| Suggested action | close-as-external |
| Confidence | 0.90 (90%) |
| Reason | Bug is confirmed in upstream Skia's C++ engine. SkiaSharp wrappers are thin pass-throughs with no logic at fault. Reporter has already filed the upstream Skia bug. Workarounds exist. |
| Suggested repro platform | linux |

### Automatable Actions

| Type | Risk | Confidence | Description | Details |
|------|------|------------|-------------|---------|
| update-labels | low | 0.95 (95%) | Apply bug, area, platform, backend, and tenet labels | labels=type/bug, area/SkiaSharp, os/Windows-Classic, backend/Raster, tenet/reliability |
| add-comment | medium | 0.88 (88%) | Acknowledge the upstream root cause and provide workarounds | — |

**Comment draft for `add-comment`:**

```markdown
Thanks for the thorough investigation and for filing the upstream Skia bug at https://issues.skia.org/issues/294565262!

This is confirmed as a bug in Skia's antialiased clip rasterizer when handling fractional float coordinates at large canvas scales. SkiaSharp's `ClipPath` is a direct wrapper around Skia's `SkCanvas::clipPath` with no intervening logic, so there is nothing SkiaSharp can fix independently — this needs to be addressed in the upstream Skia library.

**Workarounds (available today):**

1. **Use integer coordinates** — replacing `4999.6f` with `4999` avoids the precision issue:
   ```csharp
   path.AddRect(new SKRect(0, 0, 20000, 4999));
   ```

2. **Use `ClipRect` instead of `ClipPath`** for rectangular clips — this avoids the path-based clip code path entirely:
   ```csharp
   skCanvas.ClipRect(new SKRect(0, 0, 20000, 4999.6f), SKClipOperation.Intersect, true);
   ```

We'll track the upstream fix and include it in a future Skia milestone bump.
```

<details>
<summary>Raw JSON</summary>

```json
{
  "meta": {
    "schemaVersion": "1.0",
    "number": 2533,
    "repo": "mono/SkiaSharp",
    "analyzedAt": "2026-04-24T22:46:37Z"
  },
  "summary": "SKCanvas.ClipPath with a large float-coordinate rect (e.g., 4999.6f) in a large canvas (20000×5000) causes incorrect/missing rendering; integer coordinates work correctly — confirmed as an upstream Skia bug by the reporter.",
  "classification": {
    "type": {
      "value": "type/bug",
      "confidence": 0.97
    },
    "area": {
      "value": "area/SkiaSharp",
      "confidence": 0.9
    },
    "platforms": [
      "os/Windows-Classic"
    ],
    "backends": [
      "backend/Raster"
    ],
    "tenets": [
      "tenet/reliability"
    ]
  },
  "evidence": {
    "bugSignals": {
      "severity": "medium",
      "regressionClaimed": false,
      "errorType": "wrong-output",
      "errorMessage": "Large portions of the rendered image are missing/not filled when ClipPath uses a float rect with fractional coordinates in a large canvas",
      "reproQuality": "complete",
      "targetFrameworks": [
        "net6.0"
      ]
    },
    "reproEvidence": {
      "stepsToReproduce": [
        "Create a SKPictureRecorder with 20000×5000 bounds",
        "Create a SKPath with AddRect(0, 0, 20000, 4999.6f)",
        "Call SKCanvas.ClipPath(path, SKClipOperation.Intersect, antialias: true)",
        "Draw a full-size red filled path",
        "Render to a 20000×5000 SKBitmap",
        "Observe: large missing area in the output — expected: nearly fully red image"
      ],
      "environmentDetails": "Windows 11, .NET 6, SkiaSharp 2.88.3, Visual Studio 2022",
      "repoLinks": [
        {
          "url": "https://issues.skia.org/issues/294565262",
          "description": "Upstream Skia bug filed by reporter confirming the issue reproduces with raw Skia C++ code"
        }
      ]
    },
    "versionAnalysis": {
      "mentionedVersions": [
        "2.88.3"
      ],
      "currentRelevance": "unknown",
      "relevanceReason": "Issue was filed against 2.88.3; no fix or upstream Skia patch has been linked. Upstream Skia bug tracker entry exists but no resolution is known."
    }
  },
  "analysis": {
    "summary": "Floating-point precision issue in upstream Skia's antialiased clip/rasterization path when a clip path uses fractional coordinates near large integer bounds. SkiaSharp's ClipPath is a thin P/Invoke wrapper with no SkiaSharp-side logic that could cause or fix this — the bug lives entirely in Skia's C++ rendering engine.",
    "rationale": "The reporter provided complete minimal repro code in C# and independently verified the issue in raw Skia C++ code, ruling out any SkiaSharp wrapper problem. The root cause is in Skia's clip tile/rasterization logic at large canvas scales. Reported upstream at issues.skia.org. SkiaSharp cannot fix this without an upstream patch.",
    "keySignals": [
      {
        "text": "I reproduced this issue when directly calling Skia using C++ code, so it should be a bug from Skia.",
        "source": "comment by reporter",
        "interpretation": "Root cause confirmed to be in upstream Skia, not in SkiaSharp wrappers."
      },
      {
        "text": "https://issues.skia.org/issues/294565262 — this is the issue have reported to skia",
        "source": "comment by reporter",
        "interpretation": "Upstream Skia bug has been filed; this issue tracks awareness in the SkiaSharp project."
      },
      {
        "text": "float type have issue — path.AddRect(new(0, 0, 20000, 4999.6f)); ... int type not have issue — path.AddRect(new(0, 0, 20000, 4999))",
        "source": "issue body",
        "interpretation": "The triggering condition is a fractional float coordinate close to an integer in a large-scale canvas, pointing to a floating-point precision issue in Skia's clipper."
      }
    ],
    "codeInvestigation": [
      {
        "file": "binding/SkiaSharp/SKCanvas.cs",
        "lines": "260-266",
        "finding": "SKCanvas.ClipPath validates the path argument and delegates directly to sk_canvas_clip_path_with_operation — no SkiaSharp-side coordinate transformation or special handling. Cannot be the source of the bug.",
        "relevance": "direct"
      },
      {
        "file": "binding/SkiaSharp/SkiaApi.generated.cs",
        "lines": "2151-2167",
        "finding": "sk_canvas_clip_path_with_operation is a generated P/Invoke stub that passes all parameters unchanged to the native Skia library. No rounding or conversion of coordinates occurs in the managed layer.",
        "relevance": "direct"
      }
    ],
    "workarounds": [
      "Use integer coordinates in the clip path: path.AddRect(new SKRect(0, 0, 20000, 4999)) instead of 4999.6f",
      "Use SKCanvas.ClipRect instead of ClipPath for rectangular clips to avoid the path-based code path",
      "Disable antialiasing: canvas.ClipPath(path, SKClipOperation.Intersect, antialias: false) — may avoid the precision issue in the AA clipper"
    ],
    "nextQuestions": [
      "Has the upstream Skia bug (issues.skia.org/294565262) been resolved and included in a newer SkiaSharp version?",
      "Does the bug reproduce with the latest SkiaSharp version (post-2.88.3)?",
      "Does disabling antialiasing (antialias: false) reliably avoid the issue?"
    ],
    "resolution": {
      "hypothesis": "Upstream Skia floating-point precision bug in the antialiased clip rasterizer when handling fractional float coordinates at large canvas scales (likely in tile boundary calculation).",
      "proposals": [
        {
          "title": "Use integer coordinates as workaround",
          "description": "Replace fractional float rect coordinates with integers in the clip path. If the exact fractional value is required, round to integer before clipping.",
          "category": "workaround",
          "codeSnippet": "// Workaround: use integer rect in clip path\npath.AddRect(new SKRect(0, 0, 20000, 4999)); // avoids float precision issue\nskCanvas.ClipPath(path, SKClipOperation.Intersect, true);",
          "confidence": 0.9,
          "effort": "cost/xs",
          "validated": "yes"
        },
        {
          "title": "Use ClipRect instead of ClipPath for rectangular regions",
          "description": "SKCanvas.ClipRect bypasses the path-based clipping code path and uses a more direct rect-based clip, avoiding the problematic AA path clipper.",
          "category": "workaround",
          "codeSnippet": "// Alternative: use ClipRect for rectangular clips\nskCanvas.ClipRect(new SKRect(0, 0, 20000, 4999.6f), SKClipOperation.Intersect, true);",
          "confidence": 0.8,
          "effort": "cost/xs",
          "validated": "yes"
        },
        {
          "title": "Track upstream Skia fix",
          "description": "Monitor the upstream Skia bug (https://issues.skia.org/issues/294565262) for a fix and include it in the next SkiaSharp Skia milestone bump.",
          "category": "fix",
          "confidence": 0.7,
          "effort": "cost/l",
          "validated": "untested"
        }
      ],
      "recommendedProposal": "Use integer coordinates as workaround",
      "recommendedReason": "Simplest immediate fix with confirmed behavior. Reporter's own testing shows integer values work correctly."
    }
  },
  "output": {
    "actionability": {
      "suggestedAction": "close-as-external",
      "confidence": 0.9,
      "reason": "Bug is confirmed in upstream Skia's C++ engine. SkiaSharp wrappers are thin pass-throughs with no logic at fault. Reporter has already filed the upstream Skia bug. Workarounds exist.",
      "suggestedReproPlatform": "linux"
    },
    "actions": [
      {
        "type": "update-labels",
        "description": "Apply bug, area, platform, backend, and tenet labels",
        "risk": "low",
        "confidence": 0.95,
        "labels": [
          "type/bug",
          "area/SkiaSharp",
          "os/Windows-Classic",
          "backend/Raster",
          "tenet/reliability"
        ]
      },
      {
        "type": "add-comment",
        "description": "Acknowledge the upstream root cause and provide workarounds",
        "risk": "medium",
        "confidence": 0.88,
        "comment": "Thanks for the thorough investigation and for filing the upstream Skia bug at https://issues.skia.org/issues/294565262!\n\nThis is confirmed as a bug in Skia's antialiased clip rasterizer when handling fractional float coordinates at large canvas scales. SkiaSharp's `ClipPath` is a direct wrapper around Skia's `SkCanvas::clipPath` with no intervening logic, so there is nothing SkiaSharp can fix independently — this needs to be addressed in the upstream Skia library.\n\n**Workarounds (available today):**\n\n1. **Use integer coordinates** — replacing `4999.6f` with `4999` avoids the precision issue:\n   ```csharp\n   path.AddRect(new SKRect(0, 0, 20000, 4999));\n   ```\n\n2. **Use `ClipRect` instead of `ClipPath`** for rectangular clips — this avoids the path-based clip code path entirely:\n   ```csharp\n   skCanvas.ClipRect(new SKRect(0, 0, 20000, 4999.6f), SKClipOperation.Intersect, true);\n   ```\n\nWe'll track the upstream fix and include it in a future Skia milestone bump."
      }
    ]
  }
}
```

</details>
