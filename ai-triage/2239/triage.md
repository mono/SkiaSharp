# Issue Triage Report — #2239

| Field | Value |
|-------|-------|
| Repository | mono/SkiaSharp |
| Analyzed | 2026-04-28T16:14:00Z |
| Type | type/bug (0.95 (95%)) |
| Area | area/SkiaSharp (0.80 (80%)) |
| Suggested action | needs-investigation (0.85 (85%)) |

**Issue Summary:** Alpha8 GPU surface (SKSurface.Create with GRContext) produces wrong pixel values on readback — clearing with alpha=50 yields pixel byte value 5 instead of 50, while the identical CPU surface returns the correct value.

**Analysis:** The GPU surface with Alpha8 format returns incorrect pixel values on readback (value 5 instead of 50). The most likely cause is how Skia internally maps the deprecated GL_ALPHA8 texture format — modern OpenGL removed GL_ALPHA8 from core profile, so Skia maps kAlpha_8 to GL_R8 (red channel). On readback, if the channel swizzle or color space conversion is applied incorrectly, the alpha value is corrupted. The reporter's native Skia test also shows unexpected behavior with Alpha8 GPU surfaces, strongly indicating the root cause is in the upstream Skia GPU pipeline rather than the SkiaSharp C# wrapper.

**Recommendations:** **needs-investigation** — Reproducible bug with clear expected/actual output and complete repro code. Root cause appears to be in upstream Skia GPU pipeline (Alpha8/GL_ALPHA8 mapping), but SkiaSharp-level investigation is needed to confirm before closing as external.

---

## Classification

| Field | Value |
|-------|-------|
| Type | type/bug |
| Area | area/SkiaSharp |
| Platforms | os/Windows-Classic |
| Backends | backend/OpenGL |
| Tenets | tenet/reliability |
| Partner | — |

## Evidence

### Reproduction

1. Create an SKImageInfo with SKColorType.Alpha8 (1x1 pixel)
2. Create a GPU SKSurface using SKSurface.Create(grContext, false, imageInfo)
3. Clear the canvas with SKColor(25, 0, 0, 50)
4. Flush surface and call Snapshot()
5. Convert to bitmap with SKBitmap.FromImage()
6. Observe bitmap bytes: GPU gives [0,0,0,5] while CPU gives [0,0,0,50]

**Environment:** SkiaSharp 2.88.1, Visual Studio 2022 17.3.2, Windows 10 WPF

**Repository links:**
- https://fiddle.skia.org/c/feaaefb4c89602123b712bd2688c723f — Native Skia fiddle showing Alpha8 GPU readback also returns wrong value in native code

**Attachments:**
- Skia.TestBed.zip — https://github.com/mono/SkiaSharp/files/9484517/Skia.TestBed.zip

**Code snippets:**

```csharp
SKSurface.Create(context, false, new SKImageInfo(1,1,SKColorType.Alpha8)); canvas.Clear(new SKColor(25,0,0,50));
```

### Bug Signals

| Field | Value |
|-------|-------|
| Severity | medium |
| Regression claimed | False |
| Error type | wrong-output |
| Error message | GPU Alpha8 surface returns pixel byte 5 instead of expected 50 when cleared with SKColor(25,0,0,50) |
| Repro quality | complete |
| Target frameworks | net6.0-windows |

### Version Analysis

| Field | Value |
|-------|-------|
| Mentioned versions | 2.88.1 |
| Worked in | — |
| Broke in | — |
| Current relevance | likely |
| Relevance reason | Alpha8 GPU surface handling in Skia (GL_ALPHA8 mapped to GL_R8 internally) is a known area of concern; the code path has not been fundamentally changed in SkiaSharp wrappers. |

## Analysis

### Technical Summary

The GPU surface with Alpha8 format returns incorrect pixel values on readback (value 5 instead of 50). The most likely cause is how Skia internally maps the deprecated GL_ALPHA8 texture format — modern OpenGL removed GL_ALPHA8 from core profile, so Skia maps kAlpha_8 to GL_R8 (red channel). On readback, if the channel swizzle or color space conversion is applied incorrectly, the alpha value is corrupted. The reporter's native Skia test also shows unexpected behavior with Alpha8 GPU surfaces, strongly indicating the root cause is in the upstream Skia GPU pipeline rather than the SkiaSharp C# wrapper.

### Rationale

The issue is clearly type/bug because the same Alpha8 clear operation produces correct results on CPU (byte=50) but wrong results on GPU (byte=5). The reporter provides a complete, minimal reproduction with expected vs actual output. The reporter's follow-up native Skia C++ test (with uninitialized UB notwithstanding) also shows Alpha8 GPU readback returning unexpected values, pointing to an upstream Skia issue. Area is area/SkiaSharp as the issue manifests through core SkiaSharp GPU surface APIs; if investigation confirms root cause is in native Skia, it should be re-classified as area/libSkiaSharp.native or closed as external.

### Key Signals

- "GPU BYTES CONTENT: 0, 0, 0, 5 (expected 0, 0, 0, 50)" — **issue body — actual vs expected output** (The alpha channel is corrupted during GPU surface readback; CPU path is unaffected.)
- "i expect TWO FIVE FIVE... but got SOMETHING ELSE" — **comment #1 — native Skia C++ test** (The same wrong-alpha behavior appears in native Skia, suggesting the bug is in the Skia GPU pipeline (C++ level), not the SkiaSharp C# wrapper.)
- "BITMAP BYTES CONTENT: 50 / BITMAP SURFACE BYTES CONTENT: 50" — **issue body — CPU paths work correctly** (All CPU surfaces (SKBitmap and CPU SKSurface) return the correct value; only GPU surfaces are affected.)

### Code Investigation

| File | Lines | Relevance | Finding |
|------|-------|-----------|---------|
| `binding/SkiaSharp/GRDefinitions.cs` | 142 | direct | SKColorType.Alpha8 maps to GRGlSizedFormat.ALPHA8 — GL_ALPHA8 is deprecated/removed in OpenGL 3.1+ core profile; Skia internally remaps this to GL_R8 (red channel) for GPU rendering, which can cause incorrect channel mapping on readback |
| `binding/SkiaSharp/SKBitmap.cs` | 715-728 | related | SKBitmap.FromImage() uses SKImageInfo.PlatformColorType (RGBA8888/BGRA8888) instead of the image's native color type, causing Alpha8 GPU images to be read back as 4-byte RGBA. This is correct behavior but means any GPU-level corruption of the alpha value will appear in the result. |
| `binding/SkiaSharp/Definitions.cs` | 164-217 | context | GetAlphaType() for Alpha8 forcibly maps SKAlphaType.Unpremul to SKAlphaType.Premul — for an alpha-only channel this should not cause value changes (premul with zero RGB is still zero RGB), so this is not the root cause but may affect edge cases. |

### Workarounds

- Use a CPU SKSurface (SKSurface.Create(imageInfo)) instead of a GPU surface for Alpha8 rendering — CPU surfaces return correct pixel values.
- Perform GPU rendering in RGBA8888 format and manually extract the alpha channel after readback.

### Next Questions

- Does the issue reproduce with Direct3D 11 backend on Windows, or only OpenGL?
- Does a newer version of SkiaSharp (3.x) still exhibit this behavior?
- Can the Skia upstream bug tracker confirm this as a known issue with Alpha8 GPU surfaces?

### Resolution Proposals

**Hypothesis:** Skia's internal mapping of kAlpha_8 to GL_R8 on modern OpenGL causes incorrect alpha values during GPU surface readback. This is likely an upstream Skia defect that SkiaSharp inherits.

1. **Use CPU surface as workaround** — workaround, confidence 0.95 (95%), cost/xs, validated=yes
   - Replace SKSurface.Create(grContext, ...) with SKSurface.Create(imageInfo) for Alpha8 rendering. CPU surfaces return correct pixel values.
2. **Investigate upstream Skia GPU Alpha8 handling** — investigation, confidence 0.80 (80%), cost/m, validated=untested
   - File or search for an upstream Skia bug regarding kAlpha_8 GPU surface readback. If confirmed upstream, close SkiaSharp issue as external and track the Skia fix.

**Recommended proposal:** Use CPU surface as workaround

**Why:** The workaround is immediate and reliable. The root cause investigation can proceed in parallel.

## Recommendations

### Actionability

| Field | Value |
|-------|-------|
| Suggested action | needs-investigation |
| Confidence | 0.85 (85%) |
| Reason | Reproducible bug with clear expected/actual output and complete repro code. Root cause appears to be in upstream Skia GPU pipeline (Alpha8/GL_ALPHA8 mapping), but SkiaSharp-level investigation is needed to confirm before closing as external. |
| Suggested repro platform | linux |

### Automatable Actions

| Type | Risk | Confidence | Description | Details |
|------|------|------------|-------------|---------|
| update-labels | low | 0.90 (90%) | Apply type/bug, area/SkiaSharp, os/Windows-Classic, backend/OpenGL, tenet/reliability | labels=type/bug, area/SkiaSharp, os/Windows-Classic, backend/OpenGL, tenet/reliability |
| add-comment | medium | 0.85 (85%) | Acknowledge the bug, confirm CPU vs GPU discrepancy, provide workaround, and note upstream Skia investigation needed | — |

**Comment draft for `add-comment`:**

```markdown
Thank you for the detailed reproduction case with expected vs actual output.

This does look like a bug: **Alpha8 GPU surface pixel readback returns incorrect values** (5 instead of 50). The CPU surface path works correctly, confirming the issue is specific to the GPU rendering path.

The likely root cause is how Skia internally handles the `kAlpha_8` format on modern OpenGL — `GL_ALPHA8` was removed from the OpenGL core profile in 3.1+, so Skia maps it internally to `GL_R8` (red channel). Channel swizzle or color space conversion during GPU readback may corrupt the alpha value.

Your native Skia comment also shows unexpected behavior, which suggests this is an upstream Skia issue rather than a SkiaSharp wrapper bug.

**Immediate workaround:** Use a CPU surface instead of a GPU surface for Alpha8 rendering:
```csharp
// Instead of: SKSurface.Create(context, false, IMAGE_INFO)
using var SURFACE_CPU = SKSurface.Create(IMAGE_INFO); // CPU surface — returns correct Alpha8 values
```

We will investigate whether this can be fixed at the SkiaSharp level or needs to be addressed upstream in Skia.
```

<details>
<summary>Raw JSON</summary>

```json
{
  "meta": {
    "schemaVersion": "1.0",
    "number": 2239,
    "repo": "mono/SkiaSharp",
    "analyzedAt": "2026-04-28T16:14:00Z"
  },
  "summary": "Alpha8 GPU surface (SKSurface.Create with GRContext) produces wrong pixel values on readback — clearing with alpha=50 yields pixel byte value 5 instead of 50, while the identical CPU surface returns the correct value.",
  "classification": {
    "type": {
      "value": "type/bug",
      "confidence": 0.95
    },
    "area": {
      "value": "area/SkiaSharp",
      "confidence": 0.8
    },
    "platforms": [
      "os/Windows-Classic"
    ],
    "backends": [
      "backend/OpenGL"
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
      "errorMessage": "GPU Alpha8 surface returns pixel byte 5 instead of expected 50 when cleared with SKColor(25,0,0,50)",
      "reproQuality": "complete",
      "targetFrameworks": [
        "net6.0-windows"
      ]
    },
    "reproEvidence": {
      "stepsToReproduce": [
        "Create an SKImageInfo with SKColorType.Alpha8 (1x1 pixel)",
        "Create a GPU SKSurface using SKSurface.Create(grContext, false, imageInfo)",
        "Clear the canvas with SKColor(25, 0, 0, 50)",
        "Flush surface and call Snapshot()",
        "Convert to bitmap with SKBitmap.FromImage()",
        "Observe bitmap bytes: GPU gives [0,0,0,5] while CPU gives [0,0,0,50]"
      ],
      "environmentDetails": "SkiaSharp 2.88.1, Visual Studio 2022 17.3.2, Windows 10 WPF",
      "attachments": [
        {
          "url": "https://github.com/mono/SkiaSharp/files/9484517/Skia.TestBed.zip",
          "filename": "Skia.TestBed.zip"
        }
      ],
      "repoLinks": [
        {
          "url": "https://fiddle.skia.org/c/feaaefb4c89602123b712bd2688c723f",
          "description": "Native Skia fiddle showing Alpha8 GPU readback also returns wrong value in native code"
        }
      ],
      "codeSnippets": [
        "SKSurface.Create(context, false, new SKImageInfo(1,1,SKColorType.Alpha8)); canvas.Clear(new SKColor(25,0,0,50));"
      ]
    },
    "versionAnalysis": {
      "mentionedVersions": [
        "2.88.1"
      ],
      "currentRelevance": "likely",
      "relevanceReason": "Alpha8 GPU surface handling in Skia (GL_ALPHA8 mapped to GL_R8 internally) is a known area of concern; the code path has not been fundamentally changed in SkiaSharp wrappers."
    }
  },
  "analysis": {
    "summary": "The GPU surface with Alpha8 format returns incorrect pixel values on readback (value 5 instead of 50). The most likely cause is how Skia internally maps the deprecated GL_ALPHA8 texture format — modern OpenGL removed GL_ALPHA8 from core profile, so Skia maps kAlpha_8 to GL_R8 (red channel). On readback, if the channel swizzle or color space conversion is applied incorrectly, the alpha value is corrupted. The reporter's native Skia test also shows unexpected behavior with Alpha8 GPU surfaces, strongly indicating the root cause is in the upstream Skia GPU pipeline rather than the SkiaSharp C# wrapper.",
    "rationale": "The issue is clearly type/bug because the same Alpha8 clear operation produces correct results on CPU (byte=50) but wrong results on GPU (byte=5). The reporter provides a complete, minimal reproduction with expected vs actual output. The reporter's follow-up native Skia C++ test (with uninitialized UB notwithstanding) also shows Alpha8 GPU readback returning unexpected values, pointing to an upstream Skia issue. Area is area/SkiaSharp as the issue manifests through core SkiaSharp GPU surface APIs; if investigation confirms root cause is in native Skia, it should be re-classified as area/libSkiaSharp.native or closed as external.",
    "codeInvestigation": [
      {
        "file": "binding/SkiaSharp/GRDefinitions.cs",
        "lines": "142",
        "finding": "SKColorType.Alpha8 maps to GRGlSizedFormat.ALPHA8 — GL_ALPHA8 is deprecated/removed in OpenGL 3.1+ core profile; Skia internally remaps this to GL_R8 (red channel) for GPU rendering, which can cause incorrect channel mapping on readback",
        "relevance": "direct"
      },
      {
        "file": "binding/SkiaSharp/SKBitmap.cs",
        "lines": "715-728",
        "finding": "SKBitmap.FromImage() uses SKImageInfo.PlatformColorType (RGBA8888/BGRA8888) instead of the image's native color type, causing Alpha8 GPU images to be read back as 4-byte RGBA. This is correct behavior but means any GPU-level corruption of the alpha value will appear in the result.",
        "relevance": "related"
      },
      {
        "file": "binding/SkiaSharp/Definitions.cs",
        "lines": "164-217",
        "finding": "GetAlphaType() for Alpha8 forcibly maps SKAlphaType.Unpremul to SKAlphaType.Premul — for an alpha-only channel this should not cause value changes (premul with zero RGB is still zero RGB), so this is not the root cause but may affect edge cases.",
        "relevance": "context"
      }
    ],
    "keySignals": [
      {
        "text": "GPU BYTES CONTENT: 0, 0, 0, 5 (expected 0, 0, 0, 50)",
        "source": "issue body — actual vs expected output",
        "interpretation": "The alpha channel is corrupted during GPU surface readback; CPU path is unaffected."
      },
      {
        "text": "i expect TWO FIVE FIVE... but got SOMETHING ELSE",
        "source": "comment #1 — native Skia C++ test",
        "interpretation": "The same wrong-alpha behavior appears in native Skia, suggesting the bug is in the Skia GPU pipeline (C++ level), not the SkiaSharp C# wrapper."
      },
      {
        "text": "BITMAP BYTES CONTENT: 50 / BITMAP SURFACE BYTES CONTENT: 50",
        "source": "issue body — CPU paths work correctly",
        "interpretation": "All CPU surfaces (SKBitmap and CPU SKSurface) return the correct value; only GPU surfaces are affected."
      }
    ],
    "workarounds": [
      "Use a CPU SKSurface (SKSurface.Create(imageInfo)) instead of a GPU surface for Alpha8 rendering — CPU surfaces return correct pixel values.",
      "Perform GPU rendering in RGBA8888 format and manually extract the alpha channel after readback."
    ],
    "nextQuestions": [
      "Does the issue reproduce with Direct3D 11 backend on Windows, or only OpenGL?",
      "Does a newer version of SkiaSharp (3.x) still exhibit this behavior?",
      "Can the Skia upstream bug tracker confirm this as a known issue with Alpha8 GPU surfaces?"
    ],
    "resolution": {
      "hypothesis": "Skia's internal mapping of kAlpha_8 to GL_R8 on modern OpenGL causes incorrect alpha values during GPU surface readback. This is likely an upstream Skia defect that SkiaSharp inherits.",
      "proposals": [
        {
          "title": "Use CPU surface as workaround",
          "description": "Replace SKSurface.Create(grContext, ...) with SKSurface.Create(imageInfo) for Alpha8 rendering. CPU surfaces return correct pixel values.",
          "category": "workaround",
          "confidence": 0.95,
          "effort": "cost/xs",
          "validated": "yes"
        },
        {
          "title": "Investigate upstream Skia GPU Alpha8 handling",
          "description": "File or search for an upstream Skia bug regarding kAlpha_8 GPU surface readback. If confirmed upstream, close SkiaSharp issue as external and track the Skia fix.",
          "category": "investigation",
          "confidence": 0.8,
          "effort": "cost/m",
          "validated": "untested"
        }
      ],
      "recommendedProposal": "Use CPU surface as workaround",
      "recommendedReason": "The workaround is immediate and reliable. The root cause investigation can proceed in parallel."
    }
  },
  "output": {
    "actionability": {
      "suggestedAction": "needs-investigation",
      "confidence": 0.85,
      "reason": "Reproducible bug with clear expected/actual output and complete repro code. Root cause appears to be in upstream Skia GPU pipeline (Alpha8/GL_ALPHA8 mapping), but SkiaSharp-level investigation is needed to confirm before closing as external.",
      "suggestedReproPlatform": "linux"
    },
    "actions": [
      {
        "type": "update-labels",
        "description": "Apply type/bug, area/SkiaSharp, os/Windows-Classic, backend/OpenGL, tenet/reliability",
        "risk": "low",
        "confidence": 0.9,
        "labels": [
          "type/bug",
          "area/SkiaSharp",
          "os/Windows-Classic",
          "backend/OpenGL",
          "tenet/reliability"
        ]
      },
      {
        "type": "add-comment",
        "description": "Acknowledge the bug, confirm CPU vs GPU discrepancy, provide workaround, and note upstream Skia investigation needed",
        "risk": "medium",
        "confidence": 0.85,
        "comment": "Thank you for the detailed reproduction case with expected vs actual output.\n\nThis does look like a bug: **Alpha8 GPU surface pixel readback returns incorrect values** (5 instead of 50). The CPU surface path works correctly, confirming the issue is specific to the GPU rendering path.\n\nThe likely root cause is how Skia internally handles the `kAlpha_8` format on modern OpenGL — `GL_ALPHA8` was removed from the OpenGL core profile in 3.1+, so Skia maps it internally to `GL_R8` (red channel). Channel swizzle or color space conversion during GPU readback may corrupt the alpha value.\n\nYour native Skia comment also shows unexpected behavior, which suggests this is an upstream Skia issue rather than a SkiaSharp wrapper bug.\n\n**Immediate workaround:** Use a CPU surface instead of a GPU surface for Alpha8 rendering:\n```csharp\n// Instead of: SKSurface.Create(context, false, IMAGE_INFO)\nusing var SURFACE_CPU = SKSurface.Create(IMAGE_INFO); // CPU surface — returns correct Alpha8 values\n```\n\nWe will investigate whether this can be fixed at the SkiaSharp level or needs to be addressed upstream in Skia."
      }
    ]
  }
}
```

</details>
