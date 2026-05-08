# Issue Triage Report — #2794

| Field | Value |
|-------|-------|
| Repository | mono/SkiaSharp |
| Analyzed | 2026-04-24T14:29:38Z |
| Type | type/bug (0.92 (92%)) |
| Area | area/SkiaSharp (0.90 (90%)) |
| Suggested action | needs-investigation (0.80 (80%)) |

**Issue Summary:** Access Violation crash in SKObject finalizer/Dispose when migrating from SkiaSharp 2.88.6 to 3.x — root cause identified as missing disposal of SKImage returned from SKSurface.Snapshot() before calling ToShader(), causing native memory to be freed while still referenced.

**Analysis:** The crash is caused by calling surface.Snapshot().ToShader(...) as a method chain without retaining and disposing the intermediate SKImage. The snapshot SKImage becomes eligible for GC immediately and when finalized calls the native destructor, freeing underlying Skia GPU texture memory that may still be referenced by the resulting shader or queued GPU commands. Explicitly wrapping the snapshot in a 'using' block (or disposing it after ToShader) resolves the crash. The maintainer noted a potential lifecycle bug in SkiaSharp where the shader may not properly ref-count or retain the source image.

**Recommendations:** **needs-investigation** — The user-side workaround is confirmed but maintainer identified a possible SkiaSharp lifecycle bug in how Snapshot/ToShader ownership is handled. Needs investigation to confirm sk_image_make_shader properly ref-counts the image.

---

## Classification

| Field | Value |
|-------|-------|
| Type | type/bug |
| Area | area/SkiaSharp |
| Platforms | os/Windows-Classic, os/Android |
| Backends | backend/OpenGL |
| Tenets | tenet/reliability |
| Partner | — |
| Current labels | type/bug, os/Windows-Classic, os/Android, area/SkiaSharp, tenet/reliability, triage/triaged |

## Evidence

### Reproduction

1. Upgrade from SkiaSharp 2.88.6 to 3.0.0-preview2.1 (or 3.116.1)
2. Use a GL-based SKSurface (e.g., via OpenTK GLControl)
3. Repeatedly call surface.Snapshot().ToShader() without disposing the snapshot SKImage
4. After some time (minutes), the GC finalizes the undisposed SKImage snapshot
5. The finalizer calls the native destructor while the shader still holds a reference to the native GPU texture
6. Access Violation occurs

**Environment:** Windows 11 Home, Visual Studio 2022, .NET Framework 4.8 / .NET 8 Android, SkiaSharp 3.0.0-preview2.1 and 3.116.1, OpenTK 3.3.3

**Repository links:**
- https://github.com/mono/SkiaSharp/issues/2794#issuecomment-2715403972 — Reporter confirms fix by explicitly disposing SKImage snapshot before ToShader
- https://github.com/mono/SkiaSharp/issues/2794#issuecomment-2715349614 — Maintainer suspects surface snapshot not retained by shader causing crash
- https://github.com/mono/SkiaSharp/issues/2794#issuecomment-2711622520 — Maintainer analysis: SKBitmap ref not retained by image on GPU copy

### Bug Signals

| Field | Value |
|-------|-------|
| Severity | high |
| Regression claimed | True |
| Error type | crash |
| Error message | Access Violation Exception in SKNativeObject finalizer/Dispose |
| Repro quality | partial |
| Target frameworks | net4.8, net8.0-android |

### Version Analysis

| Field | Value |
|-------|-------|
| Mentioned versions | 2.88.2, 2.88.6, 2.88.9, 3.0.0-preview2.1, 3.116.1 |
| Worked in | 2.88.6 |
| Broke in | 3.0.0-preview2.1 |
| Current relevance | likely |
| Relevance reason | Reporter confirmed crash still occurs in 3.116.1 with the same pattern. Maintainer acknowledged possible lifecycle bug in snapshot/shader path. |

### Regression

| Field | Value |
|-------|-------|
| Is regression | False |
| Confidence | 0.65 (65%) |
| Reason | Reporter later found the same crash also occurs in 2.88.6 and 2.88.9 when specifically tested — the crash was latent in 2.x but not encountered. The root cause (not disposing Snapshot SKImage) existed in both versions. |
| Worked in version | — |
| Broke in version | — |

## Analysis

### Technical Summary

The crash is caused by calling surface.Snapshot().ToShader(...) as a method chain without retaining and disposing the intermediate SKImage. The snapshot SKImage becomes eligible for GC immediately and when finalized calls the native destructor, freeing underlying Skia GPU texture memory that may still be referenced by the resulting shader or queued GPU commands. Explicitly wrapping the snapshot in a 'using' block (or disposing it after ToShader) resolves the crash. The maintainer noted a potential lifecycle bug in SkiaSharp where the shader may not properly ref-count or retain the source image.

### Rationale

Classified as type/bug and area/SkiaSharp because the crash occurs in the core SkiaSharp C# wrapper's SKObject finalizer. While the immediate root cause is user code not disposing the snapshot SKImage, the maintainer identified a possible underlying lifecycle issue where ToShader() may not retain the source image with appropriate ref-counting. Severity is 'high' because it crashes production apps (aviation navigation software) with no way to prevent it without knowing the exact pattern.

### Key Signals

- "Access Violation Exception in SKNativeObject finalizer/Dispose" — **issue body** (Native memory freed while still referenced — classic use-after-free.)
- "I wonder if I have a life cycle bug in where the secret font instance of paint is being disposed" — **comment by mattleibow** (Maintainer acknowledges potential lifecycle bug in SkiaSharp SKPaint/SKFont relationship.)
- "The crash in the finalizer is so weird because if you are disposing it, then the handle should be set to zero, which would then make the finalizer skip of the the logic. This means it has to be another image somewhere that is being collected - probably the snapshot." — **comment by mattleibow** (Confirms crash is due to undisposed snapshot SKImage being finalized, not a double-dispose.)
- "EVERYTHING LOOKING SUPER DUPER NOW! — 400K tiles loaded/drawn/Snapshotted, and counting" — **comment by najak3d** (Fix confirmed: wrapping snapshot in 'using' block resolves the crash.)
- "I just confirmed this bug, although BETTER, still happens within 5 minutes of operation with 3.116.1" — **comment by najak3d** (Bug persists in latest 3.116.1 release without proper disposal.)

### Code Investigation

| File | Lines | Relevance | Finding |
|------|-------|-----------|---------|
| `binding/SkiaSharp/SKSurface.cs` | 274-278 | direct | Snapshot() returns a new SKImage via GetObject wrapping sk_surface_new_image_snapshot. The returned SKImage is owned by the caller and must be disposed. No documentation or API design enforces this in a method chain. |
| `binding/SkiaSharp/SKImage.cs` | 427-428 | direct | ToShader() calls sk_image_make_shader which creates a shader from the image handle. If the C++ sk_sp<> properly retains the image ref, the C# SKImage disposal should be safe, but if not properly transferred, disposal after ToShader() call could cause use-after-free. |
| `binding/SkiaSharp/SKObject.cs` | 229-234 | related | Finalizer sets fromFinalizer=true and calls Dispose(false). The Handle is zeroed in Dispose(). If another image with the same handle is not properly tracked by the object registry, a second finalization attempt could AV. |
| `binding/SkiaSharp/SKObject.cs` | 259-277 | related | Dispose() uses Interlocked.CompareExchange to prevent double-dispose. Handle is zeroed after DisposeNative(). The guard should prevent re-entry but does not protect against concurrent finalization of a different object wrapping the same native handle. |

**Error fingerprint:** `AV-SKNativeObject-Finalizer-Snapshot-ToShader`

### Workarounds

- Wrap the Snapshot() return value in a 'using' block before calling ToShader(): `using (var snapshot = surface.Snapshot()) { shader = snapshot.ToShader(...); }`
- Store the snapshot in a variable, create shader, then explicitly call snapshot.Dispose()
- Use SKGraphics.PurgeAllCaches() or flush the GPU surface before disposal to reduce the window for use-after-free

### Next Questions

- Does sk_image_make_shader properly increment the ref count on the native image object?
- Is there a case where the C# SKObject registry returns a different wrapper for the same native handle, bypassing the disposed-flag guard?
- Does the SKPaint 'secret font' lifecycle issue identified by the maintainer represent a separate underlying bug?

### Resolution Proposals

**Hypothesis:** The snapshot SKImage returned from surface.Snapshot() is GC'd and finalized before GPU commands referencing its native texture complete execution. The fix is to explicitly dispose the snapshot only after ToShader() has been called, allowing the C++ ref-count to protect the underlying native resource.

1. **Use 'using' block for Snapshot() in ToShader() chains** — workaround, confidence 0.95 (95%), cost/xs, validated=yes
   - Replace `surface.Snapshot().ToShader(...)` with a 'using' block to ensure proper disposal.

```csharp
// Before (incorrect - snapshot may be GC'd before GPU commands complete):
_shader = _surface.Snapshot().ToShader(SKShaderTileMode.Clamp, SKShaderTileMode.Clamp);

// After (correct):
_shader?.Dispose();
using (var snapshot = _surface.Snapshot())
{
    _shader = snapshot.ToShader(SKShaderTileMode.Clamp, SKShaderTileMode.Clamp);
}
```
2. **Investigate sk_image_make_shader ref-counting in C API shim** — investigation, confidence 0.75 (75%), cost/m, validated=untested
   - Verify that sk_image_make_shader properly increments the native ref count on the image so the C# wrapper can be safely disposed after creating the shader.

**Recommended proposal:** Use 'using' block for Snapshot() in ToShader() chains

**Why:** Reporter confirmed fix resolves the crash after 400K tile operations. The 'using' pattern is idiomatic C# and forces deterministic disposal before GPU resource is reclaimed.

## Recommendations

### Actionability

| Field | Value |
|-------|-------|
| Suggested action | needs-investigation |
| Confidence | 0.80 (80%) |
| Reason | The user-side workaround is confirmed but maintainer identified a possible SkiaSharp lifecycle bug in how Snapshot/ToShader ownership is handled. Needs investigation to confirm sk_image_make_shader properly ref-counts the image. |
| Suggested repro platform | windows |

### Automatable Actions

| Type | Risk | Confidence | Description | Details |
|------|------|------------|-------------|---------|
| update-labels | low | 0.95 (95%) | Labels already applied — confirm type/bug, area/SkiaSharp, os/Windows-Classic, os/Android, backend/OpenGL, tenet/reliability | labels=type/bug, area/SkiaSharp, os/Windows-Classic, os/Android, backend/OpenGL, tenet/reliability |
| add-comment | medium | 0.88 (88%) | Post analysis with confirmed workaround and note about investigation needed for underlying lifecycle bug | — |

**Comment draft for `add-comment`:**

```markdown
## Analysis

The root cause of this crash is that `surface.Snapshot()` returns an `SKImage` that must be explicitly disposed by the caller. When called as a method chain (e.g., `surface.Snapshot().ToShader(...)`), the snapshot `SKImage` is immediately eligible for garbage collection. When the GC finalizer runs, it calls the native destructor which frees the underlying GPU texture, potentially while GPU commands referencing that texture are still queued.

**Workaround (confirmed working):**

```csharp
// Replace this pattern:
_shader = _surface.Snapshot().ToShader(SKShaderTileMode.Clamp, SKShaderTileMode.Clamp);

// With this:
_shader?.Dispose();
using (var snapshot = _surface.Snapshot())
{
    _shader = snapshot.ToShader(SKShaderTileMode.Clamp, SKShaderTileMode.Clamp);
}
```

The `using` block ensures the snapshot is disposed _deterministically_ after the shader is created, rather than leaving it to the GC finalizer.

A deeper investigation into whether `sk_image_make_shader` properly increments the native ref-count (which would make the C# disposal timing irrelevant) is still pending.
```

<details>
<summary>Raw JSON</summary>

```json
{
  "meta": {
    "schemaVersion": "1.0",
    "number": 2794,
    "repo": "mono/SkiaSharp",
    "analyzedAt": "2026-04-24T14:29:38Z",
    "currentLabels": [
      "type/bug",
      "os/Windows-Classic",
      "os/Android",
      "area/SkiaSharp",
      "tenet/reliability",
      "triage/triaged"
    ]
  },
  "summary": "Access Violation crash in SKObject finalizer/Dispose when migrating from SkiaSharp 2.88.6 to 3.x — root cause identified as missing disposal of SKImage returned from SKSurface.Snapshot() before calling ToShader(), causing native memory to be freed while still referenced.",
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
      "os/Windows-Classic",
      "os/Android"
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
      "severity": "high",
      "regressionClaimed": true,
      "errorType": "crash",
      "errorMessage": "Access Violation Exception in SKNativeObject finalizer/Dispose",
      "reproQuality": "partial",
      "targetFrameworks": [
        "net4.8",
        "net8.0-android"
      ]
    },
    "reproEvidence": {
      "stepsToReproduce": [
        "Upgrade from SkiaSharp 2.88.6 to 3.0.0-preview2.1 (or 3.116.1)",
        "Use a GL-based SKSurface (e.g., via OpenTK GLControl)",
        "Repeatedly call surface.Snapshot().ToShader() without disposing the snapshot SKImage",
        "After some time (minutes), the GC finalizes the undisposed SKImage snapshot",
        "The finalizer calls the native destructor while the shader still holds a reference to the native GPU texture",
        "Access Violation occurs"
      ],
      "environmentDetails": "Windows 11 Home, Visual Studio 2022, .NET Framework 4.8 / .NET 8 Android, SkiaSharp 3.0.0-preview2.1 and 3.116.1, OpenTK 3.3.3",
      "repoLinks": [
        {
          "url": "https://github.com/mono/SkiaSharp/issues/2794#issuecomment-2715403972",
          "description": "Reporter confirms fix by explicitly disposing SKImage snapshot before ToShader"
        },
        {
          "url": "https://github.com/mono/SkiaSharp/issues/2794#issuecomment-2715349614",
          "description": "Maintainer suspects surface snapshot not retained by shader causing crash"
        },
        {
          "url": "https://github.com/mono/SkiaSharp/issues/2794#issuecomment-2711622520",
          "description": "Maintainer analysis: SKBitmap ref not retained by image on GPU copy"
        }
      ]
    },
    "versionAnalysis": {
      "mentionedVersions": [
        "2.88.2",
        "2.88.6",
        "2.88.9",
        "3.0.0-preview2.1",
        "3.116.1"
      ],
      "workedIn": "2.88.6",
      "brokeIn": "3.0.0-preview2.1",
      "currentRelevance": "likely",
      "relevanceReason": "Reporter confirmed crash still occurs in 3.116.1 with the same pattern. Maintainer acknowledged possible lifecycle bug in snapshot/shader path."
    },
    "regression": {
      "isRegression": false,
      "confidence": 0.65,
      "reason": "Reporter later found the same crash also occurs in 2.88.6 and 2.88.9 when specifically tested — the crash was latent in 2.x but not encountered. The root cause (not disposing Snapshot SKImage) existed in both versions."
    }
  },
  "analysis": {
    "summary": "The crash is caused by calling surface.Snapshot().ToShader(...) as a method chain without retaining and disposing the intermediate SKImage. The snapshot SKImage becomes eligible for GC immediately and when finalized calls the native destructor, freeing underlying Skia GPU texture memory that may still be referenced by the resulting shader or queued GPU commands. Explicitly wrapping the snapshot in a 'using' block (or disposing it after ToShader) resolves the crash. The maintainer noted a potential lifecycle bug in SkiaSharp where the shader may not properly ref-count or retain the source image.",
    "rationale": "Classified as type/bug and area/SkiaSharp because the crash occurs in the core SkiaSharp C# wrapper's SKObject finalizer. While the immediate root cause is user code not disposing the snapshot SKImage, the maintainer identified a possible underlying lifecycle issue where ToShader() may not retain the source image with appropriate ref-counting. Severity is 'high' because it crashes production apps (aviation navigation software) with no way to prevent it without knowing the exact pattern.",
    "keySignals": [
      {
        "text": "Access Violation Exception in SKNativeObject finalizer/Dispose",
        "source": "issue body",
        "interpretation": "Native memory freed while still referenced — classic use-after-free."
      },
      {
        "text": "I wonder if I have a life cycle bug in where the secret font instance of paint is being disposed",
        "source": "comment by mattleibow",
        "interpretation": "Maintainer acknowledges potential lifecycle bug in SkiaSharp SKPaint/SKFont relationship."
      },
      {
        "text": "The crash in the finalizer is so weird because if you are disposing it, then the handle should be set to zero, which would then make the finalizer skip of the the logic. This means it has to be another image somewhere that is being collected - probably the snapshot.",
        "source": "comment by mattleibow",
        "interpretation": "Confirms crash is due to undisposed snapshot SKImage being finalized, not a double-dispose."
      },
      {
        "text": "EVERYTHING LOOKING SUPER DUPER NOW! — 400K tiles loaded/drawn/Snapshotted, and counting",
        "source": "comment by najak3d",
        "interpretation": "Fix confirmed: wrapping snapshot in 'using' block resolves the crash."
      },
      {
        "text": "I just confirmed this bug, although BETTER, still happens within 5 minutes of operation with 3.116.1",
        "source": "comment by najak3d",
        "interpretation": "Bug persists in latest 3.116.1 release without proper disposal."
      }
    ],
    "codeInvestigation": [
      {
        "file": "binding/SkiaSharp/SKSurface.cs",
        "lines": "274-278",
        "finding": "Snapshot() returns a new SKImage via GetObject wrapping sk_surface_new_image_snapshot. The returned SKImage is owned by the caller and must be disposed. No documentation or API design enforces this in a method chain.",
        "relevance": "direct"
      },
      {
        "file": "binding/SkiaSharp/SKImage.cs",
        "lines": "427-428",
        "finding": "ToShader() calls sk_image_make_shader which creates a shader from the image handle. If the C++ sk_sp<> properly retains the image ref, the C# SKImage disposal should be safe, but if not properly transferred, disposal after ToShader() call could cause use-after-free.",
        "relevance": "direct"
      },
      {
        "file": "binding/SkiaSharp/SKObject.cs",
        "lines": "229-234",
        "finding": "Finalizer sets fromFinalizer=true and calls Dispose(false). The Handle is zeroed in Dispose(). If another image with the same handle is not properly tracked by the object registry, a second finalization attempt could AV.",
        "relevance": "related"
      },
      {
        "file": "binding/SkiaSharp/SKObject.cs",
        "lines": "259-277",
        "finding": "Dispose() uses Interlocked.CompareExchange to prevent double-dispose. Handle is zeroed after DisposeNative(). The guard should prevent re-entry but does not protect against concurrent finalization of a different object wrapping the same native handle.",
        "relevance": "related"
      }
    ],
    "workarounds": [
      "Wrap the Snapshot() return value in a 'using' block before calling ToShader(): `using (var snapshot = surface.Snapshot()) { shader = snapshot.ToShader(...); }`",
      "Store the snapshot in a variable, create shader, then explicitly call snapshot.Dispose()",
      "Use SKGraphics.PurgeAllCaches() or flush the GPU surface before disposal to reduce the window for use-after-free"
    ],
    "nextQuestions": [
      "Does sk_image_make_shader properly increment the ref count on the native image object?",
      "Is there a case where the C# SKObject registry returns a different wrapper for the same native handle, bypassing the disposed-flag guard?",
      "Does the SKPaint 'secret font' lifecycle issue identified by the maintainer represent a separate underlying bug?"
    ],
    "errorFingerprint": "AV-SKNativeObject-Finalizer-Snapshot-ToShader",
    "resolution": {
      "hypothesis": "The snapshot SKImage returned from surface.Snapshot() is GC'd and finalized before GPU commands referencing its native texture complete execution. The fix is to explicitly dispose the snapshot only after ToShader() has been called, allowing the C++ ref-count to protect the underlying native resource.",
      "proposals": [
        {
          "title": "Use 'using' block for Snapshot() in ToShader() chains",
          "description": "Replace `surface.Snapshot().ToShader(...)` with a 'using' block to ensure proper disposal.",
          "category": "workaround",
          "codeSnippet": "// Before (incorrect - snapshot may be GC'd before GPU commands complete):\n_shader = _surface.Snapshot().ToShader(SKShaderTileMode.Clamp, SKShaderTileMode.Clamp);\n\n// After (correct):\n_shader?.Dispose();\nusing (var snapshot = _surface.Snapshot())\n{\n    _shader = snapshot.ToShader(SKShaderTileMode.Clamp, SKShaderTileMode.Clamp);\n}",
          "confidence": 0.95,
          "effort": "cost/xs",
          "validated": "yes"
        },
        {
          "title": "Investigate sk_image_make_shader ref-counting in C API shim",
          "description": "Verify that sk_image_make_shader properly increments the native ref count on the image so the C# wrapper can be safely disposed after creating the shader.",
          "category": "investigation",
          "confidence": 0.75,
          "effort": "cost/m",
          "validated": "untested"
        }
      ],
      "recommendedProposal": "Use 'using' block for Snapshot() in ToShader() chains",
      "recommendedReason": "Reporter confirmed fix resolves the crash after 400K tile operations. The 'using' pattern is idiomatic C# and forces deterministic disposal before GPU resource is reclaimed."
    }
  },
  "output": {
    "actionability": {
      "suggestedAction": "needs-investigation",
      "confidence": 0.8,
      "reason": "The user-side workaround is confirmed but maintainer identified a possible SkiaSharp lifecycle bug in how Snapshot/ToShader ownership is handled. Needs investigation to confirm sk_image_make_shader properly ref-counts the image.",
      "suggestedReproPlatform": "windows"
    },
    "actions": [
      {
        "type": "update-labels",
        "description": "Labels already applied — confirm type/bug, area/SkiaSharp, os/Windows-Classic, os/Android, backend/OpenGL, tenet/reliability",
        "risk": "low",
        "confidence": 0.95,
        "labels": [
          "type/bug",
          "area/SkiaSharp",
          "os/Windows-Classic",
          "os/Android",
          "backend/OpenGL",
          "tenet/reliability"
        ]
      },
      {
        "type": "add-comment",
        "description": "Post analysis with confirmed workaround and note about investigation needed for underlying lifecycle bug",
        "risk": "medium",
        "confidence": 0.88,
        "comment": "## Analysis\n\nThe root cause of this crash is that `surface.Snapshot()` returns an `SKImage` that must be explicitly disposed by the caller. When called as a method chain (e.g., `surface.Snapshot().ToShader(...)`), the snapshot `SKImage` is immediately eligible for garbage collection. When the GC finalizer runs, it calls the native destructor which frees the underlying GPU texture, potentially while GPU commands referencing that texture are still queued.\n\n**Workaround (confirmed working):**\n\n```csharp\n// Replace this pattern:\n_shader = _surface.Snapshot().ToShader(SKShaderTileMode.Clamp, SKShaderTileMode.Clamp);\n\n// With this:\n_shader?.Dispose();\nusing (var snapshot = _surface.Snapshot())\n{\n    _shader = snapshot.ToShader(SKShaderTileMode.Clamp, SKShaderTileMode.Clamp);\n}\n```\n\nThe `using` block ensures the snapshot is disposed _deterministically_ after the shader is created, rather than leaving it to the GC finalizer.\n\nA deeper investigation into whether `sk_image_make_shader` properly increments the native ref-count (which would make the C# disposal timing irrelevant) is still pending."
      }
    ]
  }
}
```

</details>
