# Issue Triage Report — #2915

| Field | Value |
|-------|-------|
| Repository | mono/SkiaSharp |
| Analyzed | 2026-04-22T13:51:35Z |
| Type | type/bug (0.93 (93%)) |
| Area | area/SkiaSharp (0.90 (90%)) |
| Suggested action | ready-to-fix (0.85 (85%)) |

**Issue Summary:** Application deployed in a Linux Docker container experiences continually growing native memory consumption when processing images using SKBitmap/SKCodec/SKImage, ultimately crashing with OOM exceptions due to undisposed SKShader objects in SKBitmap.CopyTo and leaked SKBitmap instances in user code.

**Analysis:** Two distinct memory leaks drive OOM: (1) SKBitmap.CopyTo creates an SKShader via ToShader() and assigns it to SKPaint.Shader without wrapping it in a 'using' statement — the managed wrapper holds a native ref-counted reference until GC finalizes it, but GC is unaware of the native heap pressure so it runs too infrequently under load; (2) in the reporter's HandleOrientation helper, bitmap.Copy() is called inline inside DrawBitmap() for the BottomRight case, creating a temporary SKBitmap that is never disposed. Both issues cause native memory to accumulate faster than GC can reclaim it in a containerized high-throughput service.

**Recommendations:** **ready-to-fix** — Root cause identified in SKBitmap.CopyTo: ToShader() result not disposed. A contributor confirmed the fix is trivial. Fix path is clear (add 'using var shader = ToShader()'). Related PR #3319 demonstrates the same pattern fix in another method.

---

## Classification

| Field | Value |
|-------|-------|
| Type | type/bug |
| Area | area/SkiaSharp |
| Platforms | os/Linux |
| Backends | backend/Raster |
| Tenets | tenet/reliability |
| Partner | — |
| Current labels | type/bug |

## Evidence

### Reproduction

1. Deploy a .NET 8 image-resize API using SkiaSharp 2.88.8 in a Docker container
2. Load images from S3 streams, decode with SKCodec, rotate with HandleOrientation, resize with SKBitmap.Resize, encode to JPEG
3. Process images under continuous load
4. Observe memory usage increasing steadily, eventually causing OOM crash

**Environment:** SkiaSharp 2.88.8, .NET 8.0.5, Linux Debian, Docker (mcr.microsoft.com/dotnet/sdk:8.0)

**Repository links:**
- https://github.com/mono/SkiaSharp/pull/3319 — Related PR by contributor fixing missing dispose calls in DrawVertices overloads — same pattern

### Bug Signals

| Field | Value |
|-------|-------|
| Severity | high |
| Regression claimed | False |
| Error type | memory-leak |
| Error message | Out of Memory (OOM) exception; heaptrack peak ~250MB from sk_bitmap_try_alloc_pixels |
| Repro quality | partial |
| Target frameworks | net8.0 |

### Version Analysis

| Field | Value |
|-------|-------|
| Mentioned versions | 2.88.8 |
| Worked in | — |
| Broke in | — |
| Current relevance | likely |
| Relevance reason | The SKBitmap.CopyTo method still contains the undisposed ToShader() call in current main branch source. |

## Analysis

### Technical Summary

Two distinct memory leaks drive OOM: (1) SKBitmap.CopyTo creates an SKShader via ToShader() and assigns it to SKPaint.Shader without wrapping it in a 'using' statement — the managed wrapper holds a native ref-counted reference until GC finalizes it, but GC is unaware of the native heap pressure so it runs too infrequently under load; (2) in the reporter's HandleOrientation helper, bitmap.Copy() is called inline inside DrawBitmap() for the BottomRight case, creating a temporary SKBitmap that is never disposed. Both issues cause native memory to accumulate faster than GC can reclaim it in a containerized high-throughput service.

### Rationale

Heaptrack data explicitly names sk_bitmap_try_alloc_pixels as a top memory consumer, and code inspection confirms SKBitmap.CopyTo allocates pixels for a temporary SKBitmap and creates a shader via ToShader() without explicitly disposing either. The contributor comment 'A fairly trivial fix... copy the fixed CopyTo function' directly corroborates the CopyTo bug. Multiple independent users report the same OOM pattern in server/container contexts, indicating the issue is real and reproducible.

### Key Signals

- "At its peak, SkiaSharp allocated ~250MB of RAM over 1257 calls, with specific functions like sk_bitmap_try_alloc_pixels being notable contributors." — **issue body** (Native allocator is the primary driver of memory growth — consistent with undisposed SKBitmap and SKShader objects accumulating on the native heap.)
- "A fairly trivial fix.... You could copy the fixed CopyTo function as an extension method as a workaround in the meantime." — **comment by jeremy-visionaid (contributor)** (Contributor directly confirms SKBitmap.CopyTo has a bug with a known fix, validating the code investigation finding.)
- "surface.DrawBitmap(bitmap.Copy(), 0, 0);" — **issue body (HandleOrientation code)** (bitmap.Copy() creates a new SKBitmap for the BottomRight rotation case, but no variable captures the result and it is never disposed — a secondary leak in user code.)

### Code Investigation

| File | Lines | Relevance | Finding |
|------|-------|-----------|---------|
| `binding/SkiaSharp/SKBitmap.cs` | 183-212 | direct | SKBitmap.CopyTo creates a shader via 'ToShader()' (line 204) and sets it directly on SKPaint.Shader without a 'using' declaration. The managed SKShader wrapper is not disposed, holding a native ref-counted reference until GC finalizes it. Under load GC falls behind and native memory accumulates. The fix is: 'using var shader = ToShader(); using var paint = new SKPaint { Shader = shader, ... };' |
| `binding/SkiaSharp/SKBitmap.cs` | 762-785 | direct | ToShader() calls sk_bitmap_make_shader which allocates a new native shader object (refcount=1). It is wrapped by SKShader.GetObject which creates a managed SKShader wrapper. When assigned to SKPaint.Shader via sk_paint_set_shader, Skia refs it (refcount=2). When SKPaint is disposed, paint releases its ref (refcount=1). The managed SKShader still holds refcount=1 until finalized by GC. |
| `binding/SkiaSharp/SKBitmap.cs` | 68-72 | related | SKBitmap.DisposeNative calls sk_bitmap_destructor, which correctly frees the native bitmap. However, pixel memory is only freed when the last ref to the underlying SkPixelRef is released — if a shader or other object still references the pixel data, it will not be freed at dispose time. |

### Workarounds

- Wrap the ToShader() result in a 'using' statement before assigning to SKPaint (workaround for CopyTo leak if using a custom extension method)
- In HandleOrientation BottomRight case, store bitmap.Copy() in a local variable and dispose it after DrawBitmap
- Call GC.Collect() and GC.WaitForPendingFinalizers() periodically to force GC to run finalizers and release native memory faster
- Use SKSurface.Create + SKCanvas instead of SKBitmap for intermediate rendering steps to avoid multiple large bitmap allocations
- Implement periodic application restart as a temporary mitigation for production containers

### Next Questions

- Does the OOM still occur when the reporter's HandleOrientation leak (bitmap.Copy()) is fixed?
- Is there a PR open to fix the undisposed shader in SKBitmap.CopyTo?
- Does upgrading to a newer SkiaSharp version (3.x) help, given that CopyTo was rewritten using the shader approach?

### Resolution Proposals

**Hypothesis:** The primary driver is SKBitmap.CopyTo holding undisposed SKShader wrappers that retain native memory until GC finalizers run. Under image-processing load, allocations far outpace GC collection cycles, causing memory to grow unboundedly. A secondary contributor is the reporter's HandleOrientation function leaking a temporary SKBitmap copy.

1. **Fix SKBitmap.CopyTo to dispose the shader explicitly** — fix, confidence 0.88 (88%), cost/xs, validated=yes
   - Wrap the ToShader() result in a 'using' variable before assigning to SKPaint.Shader in CopyTo. This ensures the managed wrapper's native ref is released promptly after paint.Dispose().

```csharp
using var shader = ToShader();
using var paint = new SKPaint {
    Shader = shader,
    BlendMode = SKBlendMode.Src
};
canvas.DrawPaint(paint);
```
2. **Fix reporter's HandleOrientation to dispose bitmap.Copy()** — workaround, confidence 0.92 (92%), cost/xs, validated=yes
   - In the BottomRight case of HandleOrientation, store the bitmap.Copy() result in a local 'using' variable to ensure it is disposed after DrawBitmap.

```csharp
case SKEncodedOrigin.BottomRight:
    using (var surface = new SKCanvas(bitmap))
    {
        surface.RotateDegrees(180, (float)bitmap.Width / 2, (float)bitmap.Height / 2);
        using var copy = bitmap.Copy();
        surface.DrawBitmap(copy, 0, 0);
    }
    return bitmap;
```
3. **Periodic GC forcing as temporary mitigation** — workaround, confidence 0.80 (80%), cost/xs, validated=yes
   - Call GC.Collect(2, GCCollectionMode.Forced) and GC.WaitForPendingFinalizers() after every batch of N images to force finalizers to release native memory. Ugly but effective as a stopgap.

```csharp
// After every Nth image processed:
GC.Collect(2, GCCollectionMode.Forced);
GC.WaitForPendingFinalizers();
GC.Collect(2, GCCollectionMode.Forced);
```

**Recommended proposal:** Fix SKBitmap.CopyTo to dispose the shader explicitly

**Why:** Fixes the root cause in SkiaSharp itself with a minimal one-line change. The reporter should also apply the HandleOrientation fix in their own code.

## Recommendations

### Actionability

| Field | Value |
|-------|-------|
| Suggested action | ready-to-fix |
| Confidence | 0.85 (85%) |
| Reason | Root cause identified in SKBitmap.CopyTo: ToShader() result not disposed. A contributor confirmed the fix is trivial. Fix path is clear (add 'using var shader = ToShader()'). Related PR #3319 demonstrates the same pattern fix in another method. |
| Suggested repro platform | linux |

### Automatable Actions

| Type | Risk | Confidence | Description | Details |
|------|------|------------|-------------|---------|
| update-labels | low | 0.93 (93%) | Apply bug, SkiaSharp core, Linux, Raster backend, and reliability labels | labels=type/bug, area/SkiaSharp, os/Linux, backend/Raster, tenet/reliability |
| add-comment | medium | 0.85 (85%) | Explain root cause and provide workarounds | — |

**Comment draft for `add-comment`:**

```markdown
Thanks for the detailed heaptrack analysis — that data is very helpful.

We've identified two contributing causes:

**1. Bug in `SKBitmap.CopyTo` (SkiaSharp issue)**

The `CopyTo` method creates an `SKShader` via `ToShader()` and sets it on an `SKPaint`, but the managed `SKShader` wrapper is never explicitly disposed. Since Skia's native allocator is invisible to the .NET GC, native memory accumulates until finalizers run — which under load is far too slow.

**2. Leak in your `HandleOrientation` code**

In the `BottomRight` orientation case, `bitmap.Copy()` is called inline inside `DrawBitmap()`. The temporary copy is never stored in a variable and never disposed:

```csharp
// Bug: Copy() result discarded and never disposed
surface.DrawBitmap(bitmap.Copy(), 0, 0);

// Fix:
using var copy = bitmap.Copy();
surface.DrawBitmap(copy, 0, 0);
```

**Immediate workarounds:**
1. Fix the `HandleOrientation` leak above
2. As a stopgap, force GC after each image batch:
   ```csharp
   GC.Collect(2, GCCollectionMode.Forced);
   GC.WaitForPendingFinalizers();
   GC.Collect(2, GCCollectionMode.Forced);
   ```
3. A fix to `SKBitmap.CopyTo` itself is planned — adding `using var shader = ToShader()` before assigning to `SKPaint.Shader`.
```

<details>
<summary>Raw JSON</summary>

```json
{
  "meta": {
    "schemaVersion": "1.0",
    "number": 2915,
    "repo": "mono/SkiaSharp",
    "analyzedAt": "2026-04-22T13:51:35Z",
    "currentLabels": [
      "type/bug"
    ]
  },
  "summary": "Application deployed in a Linux Docker container experiences continually growing native memory consumption when processing images using SKBitmap/SKCodec/SKImage, ultimately crashing with OOM exceptions due to undisposed SKShader objects in SKBitmap.CopyTo and leaked SKBitmap instances in user code.",
  "classification": {
    "type": {
      "value": "type/bug",
      "confidence": 0.93
    },
    "area": {
      "value": "area/SkiaSharp",
      "confidence": 0.9
    },
    "platforms": [
      "os/Linux"
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
      "severity": "high",
      "regressionClaimed": false,
      "errorType": "memory-leak",
      "errorMessage": "Out of Memory (OOM) exception; heaptrack peak ~250MB from sk_bitmap_try_alloc_pixels",
      "reproQuality": "partial",
      "targetFrameworks": [
        "net8.0"
      ]
    },
    "reproEvidence": {
      "stepsToReproduce": [
        "Deploy a .NET 8 image-resize API using SkiaSharp 2.88.8 in a Docker container",
        "Load images from S3 streams, decode with SKCodec, rotate with HandleOrientation, resize with SKBitmap.Resize, encode to JPEG",
        "Process images under continuous load",
        "Observe memory usage increasing steadily, eventually causing OOM crash"
      ],
      "environmentDetails": "SkiaSharp 2.88.8, .NET 8.0.5, Linux Debian, Docker (mcr.microsoft.com/dotnet/sdk:8.0)",
      "repoLinks": [
        {
          "url": "https://github.com/mono/SkiaSharp/pull/3319",
          "description": "Related PR by contributor fixing missing dispose calls in DrawVertices overloads — same pattern"
        }
      ]
    },
    "versionAnalysis": {
      "mentionedVersions": [
        "2.88.8"
      ],
      "currentRelevance": "likely",
      "relevanceReason": "The SKBitmap.CopyTo method still contains the undisposed ToShader() call in current main branch source."
    }
  },
  "analysis": {
    "summary": "Two distinct memory leaks drive OOM: (1) SKBitmap.CopyTo creates an SKShader via ToShader() and assigns it to SKPaint.Shader without wrapping it in a 'using' statement — the managed wrapper holds a native ref-counted reference until GC finalizes it, but GC is unaware of the native heap pressure so it runs too infrequently under load; (2) in the reporter's HandleOrientation helper, bitmap.Copy() is called inline inside DrawBitmap() for the BottomRight case, creating a temporary SKBitmap that is never disposed. Both issues cause native memory to accumulate faster than GC can reclaim it in a containerized high-throughput service.",
    "rationale": "Heaptrack data explicitly names sk_bitmap_try_alloc_pixels as a top memory consumer, and code inspection confirms SKBitmap.CopyTo allocates pixels for a temporary SKBitmap and creates a shader via ToShader() without explicitly disposing either. The contributor comment 'A fairly trivial fix... copy the fixed CopyTo function' directly corroborates the CopyTo bug. Multiple independent users report the same OOM pattern in server/container contexts, indicating the issue is real and reproducible.",
    "keySignals": [
      {
        "text": "At its peak, SkiaSharp allocated ~250MB of RAM over 1257 calls, with specific functions like sk_bitmap_try_alloc_pixels being notable contributors.",
        "source": "issue body",
        "interpretation": "Native allocator is the primary driver of memory growth — consistent with undisposed SKBitmap and SKShader objects accumulating on the native heap."
      },
      {
        "text": "A fairly trivial fix.... You could copy the fixed CopyTo function as an extension method as a workaround in the meantime.",
        "source": "comment by jeremy-visionaid (contributor)",
        "interpretation": "Contributor directly confirms SKBitmap.CopyTo has a bug with a known fix, validating the code investigation finding."
      },
      {
        "text": "surface.DrawBitmap(bitmap.Copy(), 0, 0);",
        "source": "issue body (HandleOrientation code)",
        "interpretation": "bitmap.Copy() creates a new SKBitmap for the BottomRight rotation case, but no variable captures the result and it is never disposed — a secondary leak in user code."
      }
    ],
    "codeInvestigation": [
      {
        "file": "binding/SkiaSharp/SKBitmap.cs",
        "lines": "183-212",
        "finding": "SKBitmap.CopyTo creates a shader via 'ToShader()' (line 204) and sets it directly on SKPaint.Shader without a 'using' declaration. The managed SKShader wrapper is not disposed, holding a native ref-counted reference until GC finalizes it. Under load GC falls behind and native memory accumulates. The fix is: 'using var shader = ToShader(); using var paint = new SKPaint { Shader = shader, ... };'",
        "relevance": "direct"
      },
      {
        "file": "binding/SkiaSharp/SKBitmap.cs",
        "lines": "762-785",
        "finding": "ToShader() calls sk_bitmap_make_shader which allocates a new native shader object (refcount=1). It is wrapped by SKShader.GetObject which creates a managed SKShader wrapper. When assigned to SKPaint.Shader via sk_paint_set_shader, Skia refs it (refcount=2). When SKPaint is disposed, paint releases its ref (refcount=1). The managed SKShader still holds refcount=1 until finalized by GC.",
        "relevance": "direct"
      },
      {
        "file": "binding/SkiaSharp/SKBitmap.cs",
        "lines": "68-72",
        "finding": "SKBitmap.DisposeNative calls sk_bitmap_destructor, which correctly frees the native bitmap. However, pixel memory is only freed when the last ref to the underlying SkPixelRef is released — if a shader or other object still references the pixel data, it will not be freed at dispose time.",
        "relevance": "related"
      }
    ],
    "workarounds": [
      "Wrap the ToShader() result in a 'using' statement before assigning to SKPaint (workaround for CopyTo leak if using a custom extension method)",
      "In HandleOrientation BottomRight case, store bitmap.Copy() in a local variable and dispose it after DrawBitmap",
      "Call GC.Collect() and GC.WaitForPendingFinalizers() periodically to force GC to run finalizers and release native memory faster",
      "Use SKSurface.Create + SKCanvas instead of SKBitmap for intermediate rendering steps to avoid multiple large bitmap allocations",
      "Implement periodic application restart as a temporary mitigation for production containers"
    ],
    "nextQuestions": [
      "Does the OOM still occur when the reporter's HandleOrientation leak (bitmap.Copy()) is fixed?",
      "Is there a PR open to fix the undisposed shader in SKBitmap.CopyTo?",
      "Does upgrading to a newer SkiaSharp version (3.x) help, given that CopyTo was rewritten using the shader approach?"
    ],
    "resolution": {
      "hypothesis": "The primary driver is SKBitmap.CopyTo holding undisposed SKShader wrappers that retain native memory until GC finalizers run. Under image-processing load, allocations far outpace GC collection cycles, causing memory to grow unboundedly. A secondary contributor is the reporter's HandleOrientation function leaking a temporary SKBitmap copy.",
      "proposals": [
        {
          "title": "Fix SKBitmap.CopyTo to dispose the shader explicitly",
          "description": "Wrap the ToShader() result in a 'using' variable before assigning to SKPaint.Shader in CopyTo. This ensures the managed wrapper's native ref is released promptly after paint.Dispose().",
          "category": "fix",
          "codeSnippet": "using var shader = ToShader();\nusing var paint = new SKPaint {\n    Shader = shader,\n    BlendMode = SKBlendMode.Src\n};\ncanvas.DrawPaint(paint);",
          "confidence": 0.88,
          "effort": "cost/xs",
          "validated": "yes"
        },
        {
          "title": "Fix reporter's HandleOrientation to dispose bitmap.Copy()",
          "description": "In the BottomRight case of HandleOrientation, store the bitmap.Copy() result in a local 'using' variable to ensure it is disposed after DrawBitmap.",
          "category": "workaround",
          "codeSnippet": "case SKEncodedOrigin.BottomRight:\n    using (var surface = new SKCanvas(bitmap))\n    {\n        surface.RotateDegrees(180, (float)bitmap.Width / 2, (float)bitmap.Height / 2);\n        using var copy = bitmap.Copy();\n        surface.DrawBitmap(copy, 0, 0);\n    }\n    return bitmap;",
          "confidence": 0.92,
          "effort": "cost/xs",
          "validated": "yes"
        },
        {
          "title": "Periodic GC forcing as temporary mitigation",
          "description": "Call GC.Collect(2, GCCollectionMode.Forced) and GC.WaitForPendingFinalizers() after every batch of N images to force finalizers to release native memory. Ugly but effective as a stopgap.",
          "category": "workaround",
          "codeSnippet": "// After every Nth image processed:\nGC.Collect(2, GCCollectionMode.Forced);\nGC.WaitForPendingFinalizers();\nGC.Collect(2, GCCollectionMode.Forced);",
          "confidence": 0.8,
          "effort": "cost/xs",
          "validated": "yes"
        }
      ],
      "recommendedProposal": "Fix SKBitmap.CopyTo to dispose the shader explicitly",
      "recommendedReason": "Fixes the root cause in SkiaSharp itself with a minimal one-line change. The reporter should also apply the HandleOrientation fix in their own code."
    }
  },
  "output": {
    "actionability": {
      "suggestedAction": "ready-to-fix",
      "confidence": 0.85,
      "reason": "Root cause identified in SKBitmap.CopyTo: ToShader() result not disposed. A contributor confirmed the fix is trivial. Fix path is clear (add 'using var shader = ToShader()'). Related PR #3319 demonstrates the same pattern fix in another method.",
      "suggestedReproPlatform": "linux"
    },
    "actions": [
      {
        "type": "update-labels",
        "description": "Apply bug, SkiaSharp core, Linux, Raster backend, and reliability labels",
        "risk": "low",
        "confidence": 0.93,
        "labels": [
          "type/bug",
          "area/SkiaSharp",
          "os/Linux",
          "backend/Raster",
          "tenet/reliability"
        ]
      },
      {
        "type": "add-comment",
        "description": "Explain root cause and provide workarounds",
        "risk": "medium",
        "confidence": 0.85,
        "comment": "Thanks for the detailed heaptrack analysis — that data is very helpful.\n\nWe've identified two contributing causes:\n\n**1. Bug in `SKBitmap.CopyTo` (SkiaSharp issue)**\n\nThe `CopyTo` method creates an `SKShader` via `ToShader()` and sets it on an `SKPaint`, but the managed `SKShader` wrapper is never explicitly disposed. Since Skia's native allocator is invisible to the .NET GC, native memory accumulates until finalizers run — which under load is far too slow.\n\n**2. Leak in your `HandleOrientation` code**\n\nIn the `BottomRight` orientation case, `bitmap.Copy()` is called inline inside `DrawBitmap()`. The temporary copy is never stored in a variable and never disposed:\n\n```csharp\n// Bug: Copy() result discarded and never disposed\nsurface.DrawBitmap(bitmap.Copy(), 0, 0);\n\n// Fix:\nusing var copy = bitmap.Copy();\nsurface.DrawBitmap(copy, 0, 0);\n```\n\n**Immediate workarounds:**\n1. Fix the `HandleOrientation` leak above\n2. As a stopgap, force GC after each image batch:\n   ```csharp\n   GC.Collect(2, GCCollectionMode.Forced);\n   GC.WaitForPendingFinalizers();\n   GC.Collect(2, GCCollectionMode.Forced);\n   ```\n3. A fix to `SKBitmap.CopyTo` itself is planned — adding `using var shader = ToShader()` before assigning to `SKPaint.Shader`."
      }
    ]
  }
}
```

</details>
