# Issue Triage Report — #1870

| Field | Value |
|-------|-------|
| Repository | mono/SkiaSharp |
| Analyzed | 2026-06-24T05:35:00Z |
| Type | type/bug (0.88 (88%)) |
| Area | area/SkiaSharp.Views (0.85 (85%)) |
| Suggested action | needs-investigation (0.80 (80%)) |

**Issue Summary:** SKCanvas rotation (RotateDegrees) causes a ~10x performance regression on UWP (500ms vs 50ms) when drawing surfaces in PaintSurface, while iOS shows no penalty and Android shows only minor overhead, suggesting the UWP raster surface backend incurs high software interpolation cost for non-axis-aligned transforms.

**Analysis:** On UWP, SKXamlCanvas uses a WriteableBitmap-backed raster surface (CPU rendering). Applying RotateDegrees forces Skia to use software anti-aliased interpolation for every draw call, creating a ~10x CPU overhead. On iOS (Metal GPU) and Android (GPU-accelerated), transforms are applied cheaply in hardware, explaining the platform gap.

**Recommendations:** **needs-investigation** — Real performance regression with partial repro code on UWP. Architectural root cause (raster CPU vs GPU rendering) is identified, but the reporter should confirm their SkiaSharp version and validate whether SKSwapChainPanel resolves the issue.

---

## Classification

| Field | Value |
|-------|-------|
| Type | type/bug |
| Area | area/SkiaSharp.Views |
| Platforms | os/Windows-Universal-UWP |
| Backends | — |
| Tenets | tenet/performance |
| Partner | — |

## Evidence

### Reproduction

1. Create a UWP app with SKXamlCanvas using PaintSurface
2. In PaintSurface, call canvas.RotateDegrees with any non-zero angle that is not a multiple of 360
3. Draw one or more SKSurface objects onto the canvas using DrawSurface
4. Measure elapsed time for the PaintSurface callback

**Environment:** Visual Studio 2019 16.11.3, UWP, Xamarin.iOS 14.20.0.25, Xamarin.Android 11.4.0.5

**Related issues:** #758

**Repository links:**
- https://github.com/mono/SkiaSharp/issues/758 — Prior related report: SKCanvas rendering rotated image very slow compared to System.Drawing (closed milestone 1.68.1)

### Bug Signals

| Field | Value |
|-------|-------|
| Severity | medium |
| Regression claimed | False |
| Error type | performance |
| Error message | — |
| Repro quality | partial |
| Target frameworks | uap10.0 |

### Version Analysis

| Field | Value |
|-------|-------|
| Mentioned versions | — |
| Worked in | — |
| Broke in | — |
| Current relevance | likely |
| Relevance reason | No SkiaSharp version specified, but UWP raster rendering path via WriteableBitmap has not changed substantially. The root cause (software rendering for rotation) is architectural. |

## Analysis

### Technical Summary

On UWP, SKXamlCanvas uses a WriteableBitmap-backed raster surface (CPU rendering). Applying RotateDegrees forces Skia to use software anti-aliased interpolation for every draw call, creating a ~10x CPU overhead. On iOS (Metal GPU) and Android (GPU-accelerated), transforms are applied cheaply in hardware, explaining the platform gap.

### Rationale

The 10x slowdown with any non-zero rotation specifically on UWP, with no penalty on iOS and minor penalty on Android, points directly to the software raster rendering path used by SKXamlCanvas. The UWP view creates its surface as a CPU-backed raster surface (SKSurface.Create with raw pixel pointer from WriteableBitmap), so all transforms including rotation must be applied in software. The related #758 was about general rotation slowness fixed by switching to Clang; this is a distinct architectural issue with the UWP canvas backend.

### Key Signals

- "500ms with Rotation != 0, and 50ms with no Rotation on UWP" — **issue body** (10x slowdown from rotation exclusively on UWP — strong signal of software rendering path being the bottleneck.)
- "With Android 100ms with and 70ms without, with iOS no difference, 70ms with or without rotation" — **issue body** (iOS shows zero penalty (GPU handles transform), Android shows small penalty (partial GPU), UWP shows massive penalty (full software CPU path).)
- "I'm following up https://github.com/mono/SkiaSharp/issues/758 — The issue is closed, but I dont know, if it was fixed or if there is a workaround" — **issue body** (Reporter is aware of #758 (fixed in 1.68.1 via Clang switch) but is seeing a different UWP-specific performance gap that persists.)

### Code Investigation

| File | Lines | Relevance | Finding |
|------|-------|-----------|---------|
| `source/SkiaSharp.Views/SkiaSharp.Views.WinUI/SKXamlCanvas.cs` | 195-206 | direct | DoInvalidate() creates surface as SKSurface.Create(info, pixels, info.RowBytes) — a CPU-backed raster surface using raw pixel pointer from WriteableBitmap. No GPU context is used. All draw operations including rotation happen in software. |
| `binding/SkiaSharp/SKCanvas.cs` | 224-230 | direct | RotateDegrees calls sk_canvas_rotate_degrees — sets a rotation transform on the canvas. For a raster (CPU) canvas, subsequent DrawSurface calls must apply this transform in software with interpolation, which is expensive for arbitrary angles. |
| `source/SkiaSharp.Views/SkiaSharp.Views.WinUI/SKSwapChainPanel.cs` | 1-50 | related | SKSwapChainPanel uses ANGLE (OpenGL ES) for GPU-accelerated rendering via GRGlInterface/GRContext. Rotation transforms would be applied on the GPU, avoiding the software interpolation cost. |

### Workarounds

- Use SKSwapChainPanel (GPU-accelerated ANGLE/OpenGL backend) instead of SKXamlCanvas (raster/CPU) on UWP. SKSwapChainPanel handles transforms on the GPU and will not have this penalty.
- If using SKXamlCanvas is required, reduce filter quality (SKFilterQuality.Low or None) — this was noted in #758 to restore performance at the cost of visual quality.
- Pre-rotate the layer surfaces before rendering so that the final DrawSurface call does not need an active rotation transform on the canvas.

### Next Questions

- What SkiaSharp version is the reporter using? The fix in #758 (Clang) may or may not apply.
- Does using SKSwapChainPanel (GPU) instead of SKXamlCanvas (raster) resolve the performance issue on UWP?
- Is the async/await timing approach accurate — could the 500ms include I/O or scheduling overhead from `await l.ViewHasChanged()`?
- What surface type do the layer surfaces (l.Surface) use? If they are raster surfaces, DrawSurface at a rotated angle requires software pixel interpolation.

### Resolution Proposals

**Hypothesis:** SKXamlCanvas uses CPU raster rendering on UWP. Any non-axis-aligned transform (rotation != 0 or 90/180/270) forces software pixel interpolation for every DrawSurface call, causing a 10x throughput drop. Switching to the GPU-backed SKSwapChainPanel should eliminate this penalty.

1. **Switch to SKSwapChainPanel for GPU rendering** — workaround, confidence 0.85 (85%), cost/s, validated=untested
   - Use SKSwapChainPanel instead of SKXamlCanvas in the UWP app. It uses ANGLE/OpenGL which handles rotation transforms on the GPU with no CPU interpolation overhead.
2. **Reduce filter quality for rotation-heavy use cases** — workaround, confidence 0.80 (80%), cost/xs, validated=untested
   - Set paint filter quality to Low or None when drawing surfaces that will be rotation-transformed. This trades visual quality for performance and was confirmed effective in related issue #758.
3. **Investigate GPU-backed SKXamlCanvas option** — investigation, confidence 0.55 (55%), cost/xl, validated=untested
   - Evaluate adding an optional GPU-backed rendering path to SKXamlCanvas on UWP (using DirectX/ANGLE) so users don't have to switch to SKSwapChainPanel for performance.

**Recommended proposal:** Switch to SKSwapChainPanel for GPU rendering

**Why:** Most direct fix with clear API path — SKSwapChainPanel already provides GPU-accelerated rendering on UWP. No SkiaSharp code changes needed.

## Recommendations

### Actionability

| Field | Value |
|-------|-------|
| Suggested action | needs-investigation |
| Confidence | 0.80 (80%) |
| Reason | Real performance regression with partial repro code on UWP. Architectural root cause (raster CPU vs GPU rendering) is identified, but the reporter should confirm their SkiaSharp version and validate whether SKSwapChainPanel resolves the issue. |
| Suggested repro platform | windows |

### Missing Info

- SkiaSharp NuGet package version being used
- Whether the timing also includes the async layer update (await l.ViewHasChanged) overhead or just the canvas drawing
- Whether the same performance regression occurs with a minimal repro without the async pattern

### Automatable Actions

| Type | Risk | Confidence | Description | Details |
|------|------|------------|-------------|---------|
| update-labels | low | 0.90 (90%) | Apply bug, views, UWP, and performance tenet labels | labels=type/bug, area/SkiaSharp.Views, os/Windows-Universal-UWP, tenet/performance |
| add-comment | medium | 0.80 (80%) | Suggest SKSwapChainPanel workaround and ask for version info | — |
| link-related | low | 0.95 (95%) | Link to prior rotation performance issue #758 | linkedIssue=#758 |

**Comment draft for `add-comment`:**

```markdown
Thanks for the detailed timing measurements and platform comparison!

The performance difference you're seeing is likely due to how the UWP canvas is implemented. `SKXamlCanvas` uses a CPU-backed raster surface (renders into a `WriteableBitmap` using software). When `RotateDegrees` is applied with a non-axis-aligned angle, Skia must interpolate every pixel in software for each `DrawSurface` call — this is much more expensive than a plain copy (no rotation).

On iOS, Metal (GPU) handles the rotation transform for free. On Android, the smaller overhead suggests partial GPU involvement. On UWP raster, the full cost is paid in CPU cycles.

**Workaround:** Try `SKSwapChainPanel` instead of `SKXamlCanvas` on UWP. It uses ANGLE/OpenGL for GPU-accelerated rendering and should handle rotation without this penalty.

Also:
- Could you share which SkiaSharp NuGet version you're using?
- Does the timing include the `await l.ViewHasChanged(...)` call, or only the canvas drawing portion? Separating the two would help isolate whether the 500ms is purely from the canvas operations.
- Related: #758 tracked general rotation slowness and was addressed in 1.68.1 (Clang compilation). If you're on an older version, upgrading may help somewhat, but the architectural CPU/GPU difference for UWP is a separate concern.
```

<details>
<summary>Raw JSON</summary>

```json
{
  "meta": {
    "schemaVersion": "1.0",
    "number": 1870,
    "repo": "mono/SkiaSharp",
    "analyzedAt": "2026-06-24T05:35:00Z"
  },
  "summary": "SKCanvas rotation (RotateDegrees) causes a ~10x performance regression on UWP (500ms vs 50ms) when drawing surfaces in PaintSurface, while iOS shows no penalty and Android shows only minor overhead, suggesting the UWP raster surface backend incurs high software interpolation cost for non-axis-aligned transforms.",
  "classification": {
    "type": {
      "value": "type/bug",
      "confidence": 0.88
    },
    "area": {
      "value": "area/SkiaSharp.Views",
      "confidence": 0.85
    },
    "platforms": [
      "os/Windows-Universal-UWP"
    ],
    "tenets": [
      "tenet/performance"
    ]
  },
  "evidence": {
    "bugSignals": {
      "severity": "medium",
      "regressionClaimed": false,
      "errorType": "performance",
      "reproQuality": "partial",
      "targetFrameworks": [
        "uap10.0"
      ]
    },
    "reproEvidence": {
      "stepsToReproduce": [
        "Create a UWP app with SKXamlCanvas using PaintSurface",
        "In PaintSurface, call canvas.RotateDegrees with any non-zero angle that is not a multiple of 360",
        "Draw one or more SKSurface objects onto the canvas using DrawSurface",
        "Measure elapsed time for the PaintSurface callback"
      ],
      "environmentDetails": "Visual Studio 2019 16.11.3, UWP, Xamarin.iOS 14.20.0.25, Xamarin.Android 11.4.0.5",
      "relatedIssues": [
        758
      ],
      "repoLinks": [
        {
          "url": "https://github.com/mono/SkiaSharp/issues/758",
          "description": "Prior related report: SKCanvas rendering rotated image very slow compared to System.Drawing (closed milestone 1.68.1)"
        }
      ]
    },
    "versionAnalysis": {
      "mentionedVersions": [],
      "currentRelevance": "likely",
      "relevanceReason": "No SkiaSharp version specified, but UWP raster rendering path via WriteableBitmap has not changed substantially. The root cause (software rendering for rotation) is architectural."
    }
  },
  "analysis": {
    "summary": "On UWP, SKXamlCanvas uses a WriteableBitmap-backed raster surface (CPU rendering). Applying RotateDegrees forces Skia to use software anti-aliased interpolation for every draw call, creating a ~10x CPU overhead. On iOS (Metal GPU) and Android (GPU-accelerated), transforms are applied cheaply in hardware, explaining the platform gap.",
    "rationale": "The 10x slowdown with any non-zero rotation specifically on UWP, with no penalty on iOS and minor penalty on Android, points directly to the software raster rendering path used by SKXamlCanvas. The UWP view creates its surface as a CPU-backed raster surface (SKSurface.Create with raw pixel pointer from WriteableBitmap), so all transforms including rotation must be applied in software. The related #758 was about general rotation slowness fixed by switching to Clang; this is a distinct architectural issue with the UWP canvas backend.",
    "codeInvestigation": [
      {
        "file": "source/SkiaSharp.Views/SkiaSharp.Views.WinUI/SKXamlCanvas.cs",
        "lines": "195-206",
        "finding": "DoInvalidate() creates surface as SKSurface.Create(info, pixels, info.RowBytes) — a CPU-backed raster surface using raw pixel pointer from WriteableBitmap. No GPU context is used. All draw operations including rotation happen in software.",
        "relevance": "direct"
      },
      {
        "file": "binding/SkiaSharp/SKCanvas.cs",
        "lines": "224-230",
        "finding": "RotateDegrees calls sk_canvas_rotate_degrees — sets a rotation transform on the canvas. For a raster (CPU) canvas, subsequent DrawSurface calls must apply this transform in software with interpolation, which is expensive for arbitrary angles.",
        "relevance": "direct"
      },
      {
        "file": "source/SkiaSharp.Views/SkiaSharp.Views.WinUI/SKSwapChainPanel.cs",
        "lines": "1-50",
        "finding": "SKSwapChainPanel uses ANGLE (OpenGL ES) for GPU-accelerated rendering via GRGlInterface/GRContext. Rotation transforms would be applied on the GPU, avoiding the software interpolation cost.",
        "relevance": "related"
      }
    ],
    "keySignals": [
      {
        "text": "500ms with Rotation != 0, and 50ms with no Rotation on UWP",
        "source": "issue body",
        "interpretation": "10x slowdown from rotation exclusively on UWP — strong signal of software rendering path being the bottleneck."
      },
      {
        "text": "With Android 100ms with and 70ms without, with iOS no difference, 70ms with or without rotation",
        "source": "issue body",
        "interpretation": "iOS shows zero penalty (GPU handles transform), Android shows small penalty (partial GPU), UWP shows massive penalty (full software CPU path)."
      },
      {
        "text": "I'm following up https://github.com/mono/SkiaSharp/issues/758 — The issue is closed, but I dont know, if it was fixed or if there is a workaround",
        "source": "issue body",
        "interpretation": "Reporter is aware of #758 (fixed in 1.68.1 via Clang switch) but is seeing a different UWP-specific performance gap that persists."
      }
    ],
    "nextQuestions": [
      "What SkiaSharp version is the reporter using? The fix in #758 (Clang) may or may not apply.",
      "Does using SKSwapChainPanel (GPU) instead of SKXamlCanvas (raster) resolve the performance issue on UWP?",
      "Is the async/await timing approach accurate — could the 500ms include I/O or scheduling overhead from `await l.ViewHasChanged()`?",
      "What surface type do the layer surfaces (l.Surface) use? If they are raster surfaces, DrawSurface at a rotated angle requires software pixel interpolation."
    ],
    "workarounds": [
      "Use SKSwapChainPanel (GPU-accelerated ANGLE/OpenGL backend) instead of SKXamlCanvas (raster/CPU) on UWP. SKSwapChainPanel handles transforms on the GPU and will not have this penalty.",
      "If using SKXamlCanvas is required, reduce filter quality (SKFilterQuality.Low or None) — this was noted in #758 to restore performance at the cost of visual quality.",
      "Pre-rotate the layer surfaces before rendering so that the final DrawSurface call does not need an active rotation transform on the canvas."
    ],
    "resolution": {
      "hypothesis": "SKXamlCanvas uses CPU raster rendering on UWP. Any non-axis-aligned transform (rotation != 0 or 90/180/270) forces software pixel interpolation for every DrawSurface call, causing a 10x throughput drop. Switching to the GPU-backed SKSwapChainPanel should eliminate this penalty.",
      "proposals": [
        {
          "title": "Switch to SKSwapChainPanel for GPU rendering",
          "description": "Use SKSwapChainPanel instead of SKXamlCanvas in the UWP app. It uses ANGLE/OpenGL which handles rotation transforms on the GPU with no CPU interpolation overhead.",
          "category": "workaround",
          "confidence": 0.85,
          "effort": "cost/s",
          "validated": "untested"
        },
        {
          "title": "Reduce filter quality for rotation-heavy use cases",
          "description": "Set paint filter quality to Low or None when drawing surfaces that will be rotation-transformed. This trades visual quality for performance and was confirmed effective in related issue #758.",
          "category": "workaround",
          "confidence": 0.8,
          "effort": "cost/xs",
          "validated": "untested"
        },
        {
          "title": "Investigate GPU-backed SKXamlCanvas option",
          "description": "Evaluate adding an optional GPU-backed rendering path to SKXamlCanvas on UWP (using DirectX/ANGLE) so users don't have to switch to SKSwapChainPanel for performance.",
          "category": "investigation",
          "confidence": 0.55,
          "effort": "cost/xl",
          "validated": "untested"
        }
      ],
      "recommendedProposal": "Switch to SKSwapChainPanel for GPU rendering",
      "recommendedReason": "Most direct fix with clear API path — SKSwapChainPanel already provides GPU-accelerated rendering on UWP. No SkiaSharp code changes needed."
    }
  },
  "output": {
    "actionability": {
      "suggestedAction": "needs-investigation",
      "confidence": 0.8,
      "reason": "Real performance regression with partial repro code on UWP. Architectural root cause (raster CPU vs GPU rendering) is identified, but the reporter should confirm their SkiaSharp version and validate whether SKSwapChainPanel resolves the issue.",
      "suggestedReproPlatform": "windows"
    },
    "missingInfo": [
      "SkiaSharp NuGet package version being used",
      "Whether the timing also includes the async layer update (await l.ViewHasChanged) overhead or just the canvas drawing",
      "Whether the same performance regression occurs with a minimal repro without the async pattern"
    ],
    "actions": [
      {
        "type": "update-labels",
        "description": "Apply bug, views, UWP, and performance tenet labels",
        "risk": "low",
        "confidence": 0.9,
        "labels": [
          "type/bug",
          "area/SkiaSharp.Views",
          "os/Windows-Universal-UWP",
          "tenet/performance"
        ]
      },
      {
        "type": "add-comment",
        "description": "Suggest SKSwapChainPanel workaround and ask for version info",
        "risk": "medium",
        "confidence": 0.8,
        "comment": "Thanks for the detailed timing measurements and platform comparison!\n\nThe performance difference you're seeing is likely due to how the UWP canvas is implemented. `SKXamlCanvas` uses a CPU-backed raster surface (renders into a `WriteableBitmap` using software). When `RotateDegrees` is applied with a non-axis-aligned angle, Skia must interpolate every pixel in software for each `DrawSurface` call — this is much more expensive than a plain copy (no rotation).\n\nOn iOS, Metal (GPU) handles the rotation transform for free. On Android, the smaller overhead suggests partial GPU involvement. On UWP raster, the full cost is paid in CPU cycles.\n\n**Workaround:** Try `SKSwapChainPanel` instead of `SKXamlCanvas` on UWP. It uses ANGLE/OpenGL for GPU-accelerated rendering and should handle rotation without this penalty.\n\nAlso:\n- Could you share which SkiaSharp NuGet version you're using?\n- Does the timing include the `await l.ViewHasChanged(...)` call, or only the canvas drawing portion? Separating the two would help isolate whether the 500ms is purely from the canvas operations.\n- Related: #758 tracked general rotation slowness and was addressed in 1.68.1 (Clang compilation). If you're on an older version, upgrading may help somewhat, but the architectural CPU/GPU difference for UWP is a separate concern."
      },
      {
        "type": "link-related",
        "description": "Link to prior rotation performance issue #758",
        "risk": "low",
        "confidence": 0.95,
        "linkedIssue": 758
      }
    ]
  }
}
```

</details>
