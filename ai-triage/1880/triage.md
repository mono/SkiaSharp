# Issue Triage Report — #1880

| Field | Value |
|-------|-------|
| Repository | mono/SkiaSharp |
| Analyzed | 2026-04-29T01:50:35Z |
| Type | type/bug (0.95 (95%)) |
| Area | area/SkiaSharp (0.95 (95%)) |
| Suggested action | needs-investigation (0.88 (88%)) |

**Issue Summary:** SKPaint (and related container objects) does not hold a C# reference to properties assigned to it (Shader, MaskFilter, ColorFilter, ImageFilter, PathEffect), allowing the GC to collect the C# wrapper and call unref() on the underlying C++ object while the paint is still alive, potentially causing fatal crashes.

**Analysis:** SKPaint property setters (Shader, MaskFilter, ColorFilter, ImageFilter, PathEffect, Blender) pass the native handle to Skia via P/Invoke but do not store a strong C# reference to the wrapper object. Because HandleDictionary uses WeakReference, the GC can collect the C# wrapper while the paint is still alive. If the finalizer calls unref() and the C++ refcount reaches zero (e.g., due to a borrowed-reference pattern in the C API shim or race condition), the shader is freed while the paint still holds a pointer to it, causing a fatal crash.

**Recommendations:** **needs-investigation** — Bug is real and reproducible with a clear root cause identified. Needs investigation to confirm current 3.x behavior, determine whether sk_paint_set_shader refs or borrows, and implement the Referenced() fix in setters.

---

## Classification

| Field | Value |
|-------|-------|
| Type | type/bug |
| Area | area/SkiaSharp |
| Platforms | — |
| Backends | — |
| Tenets | tenet/reliability |
| Partner | — |

## Evidence

### Reproduction

1. Create a new SKPaint instance
2. Set paint.Shader = SKShader.CreateColor(...) without keeping a C# reference to the shader
3. Trigger garbage collection (GC.Collect() or natural GC pressure)
4. Continue using the SKPaint or dispose it — fatal crash occurs

**Related issues:** #2794

**Attachments:**
- fatal-crash-2.80.3.png — https://user-images.githubusercontent.com/518177/145032550-149cd681-17b7-4c26-a5ca-8b3cfa895679.png
- fatal-crash-2.88.3-winui.png — https://user-images.githubusercontent.com/1216684/220228873-700f187d-06f8-462b-bf49-53c9b90eb158.png

**Code snippets:**

```csharp
public SKShader Shader {
  get => SKShader.GetObject(SkiaApi.sk_paint_get_shader(Handle));
  set => SkiaApi.sk_paint_set_shader(Handle, value == null ? IntPtr.Zero : value.Handle);
}
```

### Bug Signals

| Field | Value |
|-------|-------|
| Severity | high |
| Regression claimed | False |
| Error type | crash |
| Error message | Fatal crash / Access Violation when SKPaint uses a GC-collected shader |
| Repro quality | partial |
| Target frameworks | net6.0-windows10.0.19041.0 |

### Version Analysis

| Field | Value |
|-------|-------|
| Mentioned versions | 2.80.3, 2.88.3.0 |
| Worked in | — |
| Broke in | — |
| Current relevance | likely |
| Relevance reason | Code investigation confirms that SKPaint property setters (Shader, MaskFilter, ColorFilter, ImageFilter, PathEffect, Blender) in the current codebase do not call Referenced() to keep the C# wrapper alive. The Referenced() mechanism exists in SKObject but is not applied to SKPaint setters. |

## Analysis

### Technical Summary

SKPaint property setters (Shader, MaskFilter, ColorFilter, ImageFilter, PathEffect, Blender) pass the native handle to Skia via P/Invoke but do not store a strong C# reference to the wrapper object. Because HandleDictionary uses WeakReference, the GC can collect the C# wrapper while the paint is still alive. If the finalizer calls unref() and the C++ refcount reaches zero (e.g., due to a borrowed-reference pattern in the C API shim or race condition), the shader is freed while the paint still holds a pointer to it, causing a fatal crash.

### Rationale

Reporter provides a clear code analysis identifying the missing C# strong reference in SKPaint property setters. Source code investigation confirms the pattern: setters pass the handle via P/Invoke but do not call Referenced() or store the C# object anywhere with a strong reference. The Referenced() mechanism already exists and is used in SKDocument, SKColorSpace, and SKSVG — but not in SKPaint. The crash is classified as high severity because it causes a fatal/unrecoverable crash, though it requires a specific usage pattern (discarding the property value) and GC pressure.

### Key Signals

- "The SKPaint class should hold a reference to the C# Shader to prevent garbage collection." — **issue body** (Reporter correctly identifies the root cause: no strong C# reference to the shader wrapper.)
- "A fatal crash will occur since the SKPaint is making use of the freed memory" — **issue body** (Confirms this is a use-after-free scenario triggered by GC collecting the C# wrapper.)
- "v2.88.3.0, net6.0-windows10.0.19041.0\win10-x64\AppX\SkiaSharp.Views.Windows.dll" — **comment by DamianSuess** (Issue still reproducible on v2.88.3 on WinUI, confirming cross-platform and not fixed in 2.x branch.)

### Code Investigation

| File | Lines | Relevance | Finding |
|------|-------|-----------|---------|
| `binding/SkiaSharp/SKPaint.cs` | 204-207 | direct | SKPaint.Shader setter calls sk_paint_set_shader and does not store a C# reference to the shader object. No call to Referenced(this, value) or KeepAliveObjects. Same pattern for MaskFilter (210-212), ColorFilter (214-217), ImageFilter (219-222), PathEffect (276-279), and Blender (229-232). |
| `binding/SkiaSharp/SKObject.cs` | 200-209 | direct | Referenced<T>(T owner, SKObject child) stores child in owner.KeepAliveObjects[child.Handle], preventing GC of child as long as owner is alive. This mechanism exists but is not used in SKPaint property setters. |
| `binding/SkiaSharp/HandleDictionary.cs` | 21 | direct | HandleDictionary uses WeakReference for all registered objects. Once the only strong C# reference to an SKShader is gone, the GC can collect it and the finalizer will call unref() on the native handle. |
| `binding/SkiaSharp/SKObject.cs` | 94-98 | direct | DisposeNative() for ISKReferenceCounted objects calls SafeUnRef(), which decrements the C++ refcount. If the shader was created with owns=true (refcount 1) and Skia stores a borrowed reference in the paint, unref brings refcount to 0 and frees the shader. |
| `binding/SkiaSharp/SKColorSpace.cs` | 105 | related | SKColorSpace.Create uses Referenced(GetObject(...), profile) as a correct pattern to keep profile alive. This same pattern should be applied to SKPaint setters. |

### Workarounds

- Keep an explicit C# field/variable reference to the SKShader (and other properties) for as long as the SKPaint is in use: `private SKShader _shader; ... _shader = SKShader.CreateColor(...); paint.Shader = _shader;`
- Call GC.KeepAlive(shader) at the end of any method that uses the paint to prevent premature collection within a single method scope.

### Next Questions

- Does sk_paint_set_shader in the SkiaSharp C API shim call sk_ref_sp (incrementing refcount) or store a borrowed pointer? If it refs, then the crash requires a double-unref scenario.
- Does the issue reproduce in SkiaSharp 3.x with the current codebase?
- Should the fix use Referenced() in setters, or should SKPaint track a private field per property?
- Are SKFont, SKBitmap, SKImage or other container objects similarly missing keepalive references for their sub-objects?

### Resolution Proposals

**Hypothesis:** SKPaint property setters must store a strong C# reference to property objects to prevent premature GC. The existing Referenced() mechanism is the correct fix approach.

1. **Keep C# reference in SKPaint setters using Referenced()** — fix, confidence 0.85 (85%), cost/s, validated=untested
   - Modify each property setter in SKPaint (Shader, MaskFilter, ColorFilter, ImageFilter, PathEffect, Blender) to call Referenced(this, value) after setting the native value. This stores value in KeepAliveObjects, preventing GC until the paint is disposed.
2. **Workaround: keep explicit C# reference to shader/filter objects** — workaround, confidence 0.95 (95%), cost/xs, validated=untested
   - As a user-side workaround, store the SKShader (and other filter objects) in a field or local variable that lives at least as long as the SKPaint. This prevents GC of the C# wrapper.

```csharp
// Workaround: keep a strong reference to prevent premature GC
private SKShader _shader;

void SetupPaint()
{
    _shader = SKShader.CreateColor(SKColors.Red);
    paint.Shader = _shader; // _shader field prevents GC
}
```

**Recommended proposal:** Keep C# reference in SKPaint setters using Referenced()

**Why:** The fix is small, follows the existing pattern used in SKDocument/SKColorSpace, and is the correct long-term solution. Users should not need to manually track property lifetimes.

## Recommendations

### Actionability

| Field | Value |
|-------|-------|
| Suggested action | needs-investigation |
| Confidence | 0.88 (88%) |
| Reason | Bug is real and reproducible with a clear root cause identified. Needs investigation to confirm current 3.x behavior, determine whether sk_paint_set_shader refs or borrows, and implement the Referenced() fix in setters. |
| Suggested repro platform | linux |

### Automatable Actions

| Type | Risk | Confidence | Description | Details |
|------|------|------------|-------------|---------|
| update-labels | low | 0.95 (95%) | Apply bug, area/SkiaSharp, and reliability tenet labels | labels=type/bug, area/SkiaSharp, tenet/reliability |
| add-comment | medium | 0.88 (88%) | Post analysis with root cause and workaround | — |
| link-related | low | 0.75 (75%) | Link to related issue #2794 which describes a similar finalizer crash in SkiaSharp 3.x | linkedIssue=#2794 |

**Comment draft for `add-comment`:**

```markdown
Thank you for the detailed analysis! You've correctly identified the root cause: `SKPaint` property setters (e.g., `Shader`, `MaskFilter`, `ColorFilter`, `ImageFilter`, `PathEffect`, `Blender`) pass the native handle to Skia but do not hold a strong C# reference to the wrapper object. Since `HandleDictionary` stores only `WeakReference` entries, the GC can collect the C# wrapper and call `unref()` while the paint is still using the underlying C++ object.

The `SKObject.Referenced()` helper already exists in the codebase for this purpose (it's used in `SKDocument`, `SKColorSpace`, etc.) but hasn't been applied to `SKPaint`'s property setters.

**Workaround (until fixed):** Keep an explicit strong reference to your shader/filter objects for as long as the paint is in use:

```csharp
// Store in a field to prevent premature GC
private SKShader _shader;

void Setup()
{
    _shader = SKShader.CreateColor(SKColors.Red);
    paint.Shader = _shader;
}
```

Alternatively, call `GC.KeepAlive(shader)` at the end of any method scope where the paint is used.
```

<details>
<summary>Raw JSON</summary>

```json
{
  "meta": {
    "schemaVersion": "1.0",
    "number": 1880,
    "repo": "mono/SkiaSharp",
    "analyzedAt": "2026-04-29T01:50:35Z"
  },
  "summary": "SKPaint (and related container objects) does not hold a C# reference to properties assigned to it (Shader, MaskFilter, ColorFilter, ImageFilter, PathEffect), allowing the GC to collect the C# wrapper and call unref() on the underlying C++ object while the paint is still alive, potentially causing fatal crashes.",
  "classification": {
    "type": {
      "value": "type/bug",
      "confidence": 0.95
    },
    "area": {
      "value": "area/SkiaSharp",
      "confidence": 0.95
    },
    "tenets": [
      "tenet/reliability"
    ]
  },
  "evidence": {
    "bugSignals": {
      "severity": "high",
      "regressionClaimed": false,
      "errorType": "crash",
      "errorMessage": "Fatal crash / Access Violation when SKPaint uses a GC-collected shader",
      "reproQuality": "partial",
      "targetFrameworks": [
        "net6.0-windows10.0.19041.0"
      ]
    },
    "reproEvidence": {
      "stepsToReproduce": [
        "Create a new SKPaint instance",
        "Set paint.Shader = SKShader.CreateColor(...) without keeping a C# reference to the shader",
        "Trigger garbage collection (GC.Collect() or natural GC pressure)",
        "Continue using the SKPaint or dispose it — fatal crash occurs"
      ],
      "codeSnippets": [
        "public SKShader Shader {\n  get => SKShader.GetObject(SkiaApi.sk_paint_get_shader(Handle));\n  set => SkiaApi.sk_paint_set_shader(Handle, value == null ? IntPtr.Zero : value.Handle);\n}"
      ],
      "attachments": [
        {
          "url": "https://user-images.githubusercontent.com/518177/145032550-149cd681-17b7-4c26-a5ca-8b3cfa895679.png",
          "filename": "fatal-crash-2.80.3.png"
        },
        {
          "url": "https://user-images.githubusercontent.com/1216684/220228873-700f187d-06f8-462b-bf49-53c9b90eb158.png",
          "filename": "fatal-crash-2.88.3-winui.png"
        }
      ],
      "relatedIssues": [
        2794
      ],
      "repoLinks": []
    },
    "versionAnalysis": {
      "mentionedVersions": [
        "2.80.3",
        "2.88.3.0"
      ],
      "currentRelevance": "likely",
      "relevanceReason": "Code investigation confirms that SKPaint property setters (Shader, MaskFilter, ColorFilter, ImageFilter, PathEffect, Blender) in the current codebase do not call Referenced() to keep the C# wrapper alive. The Referenced() mechanism exists in SKObject but is not applied to SKPaint setters."
    }
  },
  "analysis": {
    "summary": "SKPaint property setters (Shader, MaskFilter, ColorFilter, ImageFilter, PathEffect, Blender) pass the native handle to Skia via P/Invoke but do not store a strong C# reference to the wrapper object. Because HandleDictionary uses WeakReference, the GC can collect the C# wrapper while the paint is still alive. If the finalizer calls unref() and the C++ refcount reaches zero (e.g., due to a borrowed-reference pattern in the C API shim or race condition), the shader is freed while the paint still holds a pointer to it, causing a fatal crash.",
    "codeInvestigation": [
      {
        "file": "binding/SkiaSharp/SKPaint.cs",
        "lines": "204-207",
        "finding": "SKPaint.Shader setter calls sk_paint_set_shader and does not store a C# reference to the shader object. No call to Referenced(this, value) or KeepAliveObjects. Same pattern for MaskFilter (210-212), ColorFilter (214-217), ImageFilter (219-222), PathEffect (276-279), and Blender (229-232).",
        "relevance": "direct"
      },
      {
        "file": "binding/SkiaSharp/SKObject.cs",
        "lines": "200-209",
        "finding": "Referenced<T>(T owner, SKObject child) stores child in owner.KeepAliveObjects[child.Handle], preventing GC of child as long as owner is alive. This mechanism exists but is not used in SKPaint property setters.",
        "relevance": "direct"
      },
      {
        "file": "binding/SkiaSharp/HandleDictionary.cs",
        "lines": "21",
        "finding": "HandleDictionary uses WeakReference for all registered objects. Once the only strong C# reference to an SKShader is gone, the GC can collect it and the finalizer will call unref() on the native handle.",
        "relevance": "direct"
      },
      {
        "file": "binding/SkiaSharp/SKObject.cs",
        "lines": "94-98",
        "finding": "DisposeNative() for ISKReferenceCounted objects calls SafeUnRef(), which decrements the C++ refcount. If the shader was created with owns=true (refcount 1) and Skia stores a borrowed reference in the paint, unref brings refcount to 0 and frees the shader.",
        "relevance": "direct"
      },
      {
        "file": "binding/SkiaSharp/SKColorSpace.cs",
        "lines": "105",
        "finding": "SKColorSpace.Create uses Referenced(GetObject(...), profile) as a correct pattern to keep profile alive. This same pattern should be applied to SKPaint setters.",
        "relevance": "related"
      }
    ],
    "keySignals": [
      {
        "text": "The SKPaint class should hold a reference to the C# Shader to prevent garbage collection.",
        "source": "issue body",
        "interpretation": "Reporter correctly identifies the root cause: no strong C# reference to the shader wrapper."
      },
      {
        "text": "A fatal crash will occur since the SKPaint is making use of the freed memory",
        "source": "issue body",
        "interpretation": "Confirms this is a use-after-free scenario triggered by GC collecting the C# wrapper."
      },
      {
        "text": "v2.88.3.0, net6.0-windows10.0.19041.0\\win10-x64\\AppX\\SkiaSharp.Views.Windows.dll",
        "source": "comment by DamianSuess",
        "interpretation": "Issue still reproducible on v2.88.3 on WinUI, confirming cross-platform and not fixed in 2.x branch."
      }
    ],
    "workarounds": [
      "Keep an explicit C# field/variable reference to the SKShader (and other properties) for as long as the SKPaint is in use: `private SKShader _shader; ... _shader = SKShader.CreateColor(...); paint.Shader = _shader;`",
      "Call GC.KeepAlive(shader) at the end of any method that uses the paint to prevent premature collection within a single method scope."
    ],
    "nextQuestions": [
      "Does sk_paint_set_shader in the SkiaSharp C API shim call sk_ref_sp (incrementing refcount) or store a borrowed pointer? If it refs, then the crash requires a double-unref scenario.",
      "Does the issue reproduce in SkiaSharp 3.x with the current codebase?",
      "Should the fix use Referenced() in setters, or should SKPaint track a private field per property?",
      "Are SKFont, SKBitmap, SKImage or other container objects similarly missing keepalive references for their sub-objects?"
    ],
    "rationale": "Reporter provides a clear code analysis identifying the missing C# strong reference in SKPaint property setters. Source code investigation confirms the pattern: setters pass the handle via P/Invoke but do not call Referenced() or store the C# object anywhere with a strong reference. The Referenced() mechanism already exists and is used in SKDocument, SKColorSpace, and SKSVG — but not in SKPaint. The crash is classified as high severity because it causes a fatal/unrecoverable crash, though it requires a specific usage pattern (discarding the property value) and GC pressure.",
    "resolution": {
      "hypothesis": "SKPaint property setters must store a strong C# reference to property objects to prevent premature GC. The existing Referenced() mechanism is the correct fix approach.",
      "proposals": [
        {
          "title": "Keep C# reference in SKPaint setters using Referenced()",
          "description": "Modify each property setter in SKPaint (Shader, MaskFilter, ColorFilter, ImageFilter, PathEffect, Blender) to call Referenced(this, value) after setting the native value. This stores value in KeepAliveObjects, preventing GC until the paint is disposed.",
          "category": "fix",
          "confidence": 0.85,
          "effort": "cost/s",
          "validated": "untested"
        },
        {
          "title": "Workaround: keep explicit C# reference to shader/filter objects",
          "description": "As a user-side workaround, store the SKShader (and other filter objects) in a field or local variable that lives at least as long as the SKPaint. This prevents GC of the C# wrapper.",
          "codeSnippet": "// Workaround: keep a strong reference to prevent premature GC\nprivate SKShader _shader;\n\nvoid SetupPaint()\n{\n    _shader = SKShader.CreateColor(SKColors.Red);\n    paint.Shader = _shader; // _shader field prevents GC\n}",
          "category": "workaround",
          "confidence": 0.95,
          "effort": "cost/xs",
          "validated": "untested"
        }
      ],
      "recommendedProposal": "Keep C# reference in SKPaint setters using Referenced()",
      "recommendedReason": "The fix is small, follows the existing pattern used in SKDocument/SKColorSpace, and is the correct long-term solution. Users should not need to manually track property lifetimes."
    }
  },
  "output": {
    "actionability": {
      "suggestedAction": "needs-investigation",
      "confidence": 0.88,
      "reason": "Bug is real and reproducible with a clear root cause identified. Needs investigation to confirm current 3.x behavior, determine whether sk_paint_set_shader refs or borrows, and implement the Referenced() fix in setters.",
      "suggestedReproPlatform": "linux"
    },
    "actions": [
      {
        "type": "update-labels",
        "description": "Apply bug, area/SkiaSharp, and reliability tenet labels",
        "risk": "low",
        "confidence": 0.95,
        "labels": [
          "type/bug",
          "area/SkiaSharp",
          "tenet/reliability"
        ]
      },
      {
        "type": "add-comment",
        "description": "Post analysis with root cause and workaround",
        "risk": "medium",
        "confidence": 0.88,
        "comment": "Thank you for the detailed analysis! You've correctly identified the root cause: `SKPaint` property setters (e.g., `Shader`, `MaskFilter`, `ColorFilter`, `ImageFilter`, `PathEffect`, `Blender`) pass the native handle to Skia but do not hold a strong C# reference to the wrapper object. Since `HandleDictionary` stores only `WeakReference` entries, the GC can collect the C# wrapper and call `unref()` while the paint is still using the underlying C++ object.\n\nThe `SKObject.Referenced()` helper already exists in the codebase for this purpose (it's used in `SKDocument`, `SKColorSpace`, etc.) but hasn't been applied to `SKPaint`'s property setters.\n\n**Workaround (until fixed):** Keep an explicit strong reference to your shader/filter objects for as long as the paint is in use:\n\n```csharp\n// Store in a field to prevent premature GC\nprivate SKShader _shader;\n\nvoid Setup()\n{\n    _shader = SKShader.CreateColor(SKColors.Red);\n    paint.Shader = _shader;\n}\n```\n\nAlternatively, call `GC.KeepAlive(shader)` at the end of any method scope where the paint is used."
      },
      {
        "type": "link-related",
        "description": "Link to related issue #2794 which describes a similar finalizer crash in SkiaSharp 3.x",
        "risk": "low",
        "confidence": 0.75,
        "linkedIssue": 2794
      }
    ]
  }
}
```

</details>
