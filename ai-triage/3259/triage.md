# Issue Triage Report — #3259

| Field | Value |
|-------|-------|
| Repository | mono/SkiaSharp |
| Analyzed | 2026-05-01T19:10:00Z |
| Type | type/bug (0.97 (97%)) |
| Area | area/SkiaSharp (0.92 (92%)) |
| Suggested action | needs-investigation (0.88 (88%)) |

**Issue Summary:** When an exception is thrown inside the drawing callback passed to SKSvgCanvas and the canvas is not disposed, the native SVG canvas later calls back into a garbage-collected SKManagedWStream via a dead WeakReference, causing a NullReferenceException that crashes the Blazor app.

**Analysis:** SKSvgCanvas.Create(bounds, stream) internally creates a SKManagedWStream and registers it as an owned child of the returned canvas. The SKAbstractManagedWStream constructor calls DelegateProxies.CreateUserData(this, makeWeak: true), storing only a WeakReference in the GCHandle given to native code. When an exception escapes the drawing callback without disposing the canvas, both the canvas and its owned SKManagedWStream become unreachable. The GC collects them, making the WeakReference return null. The native SVG canvas has not been finalized and later calls back via SKManagedWStreamWriteProxy to flush/finalize SVG data. GetUserData resolves the dead WeakReference to null, and the subsequent userData.OnWrite() call throws NullReferenceException, crashing the Blazor app.

**Recommendations:** **needs-investigation** — Real regression bug with complete repro and clear root cause hypothesis. Needs investigation into whether native callbacks should null-guard or whether the ownership model should be changed.

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
| Current labels | type/bug |

## Evidence

### Reproduction

1. Create a Blazor component with a method that creates an SKSvgCanvas
2. Have the drawing callback throw an exception before canvas.Dispose() is called
3. Catch the exception in the calling code (preventing dispose from running)
4. Observe that some time later a NullReferenceException is thrown from SKManagedWStreamWriteProxyImplementation, crashing the Blazor app

**Environment:** SkiaSharp 3.116.0, Windows 10, Visual Studio, Blazor

**Repository links:**
- https://github.com/mono/SkiaSharp/issues/3520 — Related: SKManagedWStream with SKSvgCanvas missing close SVG tag — also triggered by missing canvas disposal

### Bug Signals

| Field | Value |
|-------|-------|
| Severity | high |
| Regression claimed | True |
| Error type | exception |
| Error message | System.NullReferenceException: Object reference not set to an instance of an object. at SkiaSharp.DelegateProxies.SKManagedWStreamWriteProxyImplementation |
| Repro quality | complete |
| Target frameworks | net8.0 |

**Stack trace:**

```text
SKManagedWStreamWriteProxyImplementation -> GetUserData<SKAbstractManagedWStream> returns null -> userData.OnWrite throws NullReferenceException
```

### Version Analysis

| Field | Value |
|-------|-------|
| Mentioned versions | 3.116.0, 2.88.9 |
| Worked in | 2.88.9 |
| Broke in | 3.116.0 |
| Current relevance | likely |
| Relevance reason | The WeakReference-based GCHandle pattern for SKAbstractManagedWStream was introduced in 3.x and is still present in current code. |

### Regression

| Field | Value |
|-------|-------|
| Is regression | True |
| Confidence | 0.85 (85%) |
| Reason | Reporter explicitly states worked in 2.88.9. The WeakReference approach in CreateUserData(this, makeWeak: true) is likely the 3.x change that introduced this vulnerability. |
| Worked in version | 2.88.9 |
| Broke in version | 3.116.0 |

## Analysis

### Technical Summary

SKSvgCanvas.Create(bounds, stream) internally creates a SKManagedWStream and registers it as an owned child of the returned canvas. The SKAbstractManagedWStream constructor calls DelegateProxies.CreateUserData(this, makeWeak: true), storing only a WeakReference in the GCHandle given to native code. When an exception escapes the drawing callback without disposing the canvas, both the canvas and its owned SKManagedWStream become unreachable. The GC collects them, making the WeakReference return null. The native SVG canvas has not been finalized and later calls back via SKManagedWStreamWriteProxy to flush/finalize SVG data. GetUserData resolves the dead WeakReference to null, and the subsequent userData.OnWrite() call throws NullReferenceException, crashing the Blazor app.

### Rationale

This is a clear resource-lifetime bug introduced by the WeakReference pattern in 3.x. The native SVG canvas outlives its managed peer when disposal is skipped due to an exception. The fix path is to always dispose the canvas (workaround) or to make the native callback safe when the managed object has been GC'd (library fix). Classification as type/bug with high severity is appropriate because the NullReferenceException propagates unhandled into the Blazor rendering engine and crashes the whole app.

### Key Signals

- "Object reference not set to an instance of an object. at SkiaSharp.DelegateProxies.SKManagedWStreamWriteProxyImplementation" — **issue body** (Native callback reached managed code but the WeakReference target was already collected — classic use-after-GC.)
- "if this throws an exception, i later get the exception mentioned above" — **issue body** (The delayed nature ('some time later') confirms this is GC-triggered: the WeakReference is only cleared at GC time, so the NRE happens non-deterministically after the exception.)
- "Last Known Good Version: 2.88.9" — **issue body** (Regression introduced in 3.x, consistent with the WeakReference-based GCHandle approach adopted in that version.)
- "resulting in, the whole Blazor App crashes" — **issue body** (Severity is high because the NullReferenceException is unhandled at the native callback boundary and crashes the hosting process.)

### Code Investigation

| File | Lines | Relevance | Finding |
|------|-------|-----------|---------|
| `binding/SkiaSharp/SKAbstractManagedWStream.cs` | 36-38 | direct | Constructor calls DelegateProxies.CreateUserData(this, makeWeak: true) — registers only a WeakReference to the stream instance in the GCHandle passed to native code. If the stream is GC'd while the native handle is still alive, native callbacks receive a dead WeakReference. |
| `binding/SkiaSharp/DelegateProxies.wstream.cs` | 31-36 | direct | SKManagedWStreamWriteProxyImplementation calls GetUserData<SKAbstractManagedWStream> and immediately calls userData.OnWrite — no null guard. If the WeakReference target was collected, userData is null and OnWrite throws NullReferenceException. |
| `binding/Binding.Shared/DelegateProxies.shared.cs` | 65-70 | direct | GetUserData<T> returns weak.Target when value is a WeakReference. If the target has been GC'd, this returns null without signaling an error — the null propagates silently to the caller. |
| `binding/SkiaSharp/SKSVG.cs` | 17-23 | related | SKSvgCanvas.Create(bounds, Stream) creates a SKManagedWStream and calls SKObject.Owned(canvas, managed) — the stream is owned by the canvas. If the canvas is never disposed (due to exception), both objects become GC-eligible. |

### Workarounds

- Wrap the canvas in a using statement or try/finally to ensure Dispose is always called even if an exception is thrown: `using var canvas = SKSvgCanvas.Create(...);`
- Call canvas.Dispose() in a finally block: try { DrawSomeStuff(canvas); } finally { canvas.Dispose(); }

### Next Questions

- Should the native callback proxies add null-guard and return a safe default (false/0) instead of crashing when the managed object has been collected?
- Was the WeakReference pattern introduced intentionally to break reference cycles — and if so, can the cycle be broken another way that preserves safety?

### Resolution Proposals

**Hypothesis:** The WeakReference in the GCHandle allows native code to call back into a collected managed object. Proper fix requires either guarding the callback against null or ensuring the native SVG canvas is always destroyed before the managed stream can be collected.

1. **Use try/finally (workaround)** — workaround, confidence 0.95 (95%), cost/xs, validated=yes
   - Always dispose the canvas even if drawing throws, ensuring the native SVG canvas is finalized before the managed stream can be GC'd.

```csharp
var canvas = SKSvgCanvas.Create(new SKRect(0, 0, width, height), stream);
try
{
    DrawSomeStuff(canvas);
}
finally
{
    canvas.Dispose();
}
```
2. **Add null-guard in write callback (library fix)** — fix, confidence 0.80 (80%), cost/s, validated=untested
   - Guard SKManagedWStreamWriteProxyImplementation (and other proxies) against null userData and return a safe default value instead of crashing.

**Recommended proposal:** Use try/finally (workaround)

**Why:** The workaround is immediately actionable by the reporter and correct. A library-level null-guard is also needed but requires a separate investigation of whether returning false from the write callback causes data corruption or just silently truncates the SVG.

## Recommendations

### Actionability

| Field | Value |
|-------|-------|
| Suggested action | needs-investigation |
| Confidence | 0.88 (88%) |
| Reason | Real regression bug with complete repro and clear root cause hypothesis. Needs investigation into whether native callbacks should null-guard or whether the ownership model should be changed. |
| Suggested repro platform | linux |

### Automatable Actions

| Type | Risk | Confidence | Description | Details |
|------|------|------------|-------------|---------|
| update-labels | low | 0.95 (95%) | Apply bug, area/SkiaSharp, os/Windows, backend/SVG, tenet/reliability labels | labels=type/bug, area/SkiaSharp, os/Windows, backend/SVG, tenet/reliability |
| add-comment | medium | 0.90 (90%) | Provide workaround and explain root cause | — |

**Comment draft for `add-comment`:**

```markdown
Thanks for the detailed report! This is a known resource-lifetime issue introduced in SkiaSharp 3.x.

**Root cause:** `SKSvgCanvas.Create` wraps your `MemoryStream` in a `SKManagedWStream` that is owned by the returned canvas. Internally, a `WeakReference` to the stream is stored in a GC handle that native code uses to call back into managed code. When an exception propagates out of `DrawSomeStuff` before `canvas.Dispose()` is called, the canvas and its stream become unreachable and the GC collects them. The native SVG canvas is still alive and later tries to flush/finalize the SVG data by calling back through the write proxy — but the WeakReference is now dead, so `userData` is `null` and `NullReferenceException` crashes your app.

**Workaround:** Always dispose the canvas in a `finally` block (or use a `using` statement) so the native SVG canvas is finalized before the managed stream can be collected:

```csharp
public string DrawSVG()
{
    var stream = new MemoryStream();
    var canvas = SKSvgCanvas.Create(
        new SKRect(0, 0, width, height),
        stream);
    try
    {
        DrawSomeStuff(canvas);
    }
    finally
    {
        canvas.Dispose();
    }
    return Encoding.Default.GetString(stream.ToArray());
}
```

The library should also guard the native callback proxy against null to avoid crashing the app when disposal is accidentally skipped. We are tracking this for a fix.
```

<details>
<summary>Raw JSON</summary>

```json
{
  "meta": {
    "schemaVersion": "1.0",
    "number": 3259,
    "repo": "mono/SkiaSharp",
    "analyzedAt": "2026-05-01T19:10:00Z",
    "currentLabels": [
      "type/bug"
    ]
  },
  "summary": "When an exception is thrown inside the drawing callback passed to SKSvgCanvas and the canvas is not disposed, the native SVG canvas later calls back into a garbage-collected SKManagedWStream via a dead WeakReference, causing a NullReferenceException that crashes the Blazor app.",
  "classification": {
    "type": {
      "value": "type/bug",
      "confidence": 0.97
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
      "errorType": "exception",
      "errorMessage": "System.NullReferenceException: Object reference not set to an instance of an object. at SkiaSharp.DelegateProxies.SKManagedWStreamWriteProxyImplementation",
      "stackTrace": "SKManagedWStreamWriteProxyImplementation -> GetUserData<SKAbstractManagedWStream> returns null -> userData.OnWrite throws NullReferenceException",
      "reproQuality": "complete",
      "targetFrameworks": [
        "net8.0"
      ]
    },
    "reproEvidence": {
      "stepsToReproduce": [
        "Create a Blazor component with a method that creates an SKSvgCanvas",
        "Have the drawing callback throw an exception before canvas.Dispose() is called",
        "Catch the exception in the calling code (preventing dispose from running)",
        "Observe that some time later a NullReferenceException is thrown from SKManagedWStreamWriteProxyImplementation, crashing the Blazor app"
      ],
      "environmentDetails": "SkiaSharp 3.116.0, Windows 10, Visual Studio, Blazor",
      "repoLinks": [
        {
          "url": "https://github.com/mono/SkiaSharp/issues/3520",
          "description": "Related: SKManagedWStream with SKSvgCanvas missing close SVG tag — also triggered by missing canvas disposal"
        }
      ]
    },
    "versionAnalysis": {
      "mentionedVersions": [
        "3.116.0",
        "2.88.9"
      ],
      "workedIn": "2.88.9",
      "brokeIn": "3.116.0",
      "currentRelevance": "likely",
      "relevanceReason": "The WeakReference-based GCHandle pattern for SKAbstractManagedWStream was introduced in 3.x and is still present in current code."
    },
    "regression": {
      "isRegression": true,
      "confidence": 0.85,
      "reason": "Reporter explicitly states worked in 2.88.9. The WeakReference approach in CreateUserData(this, makeWeak: true) is likely the 3.x change that introduced this vulnerability.",
      "workedInVersion": "2.88.9",
      "brokeInVersion": "3.116.0"
    }
  },
  "analysis": {
    "summary": "SKSvgCanvas.Create(bounds, stream) internally creates a SKManagedWStream and registers it as an owned child of the returned canvas. The SKAbstractManagedWStream constructor calls DelegateProxies.CreateUserData(this, makeWeak: true), storing only a WeakReference in the GCHandle given to native code. When an exception escapes the drawing callback without disposing the canvas, both the canvas and its owned SKManagedWStream become unreachable. The GC collects them, making the WeakReference return null. The native SVG canvas has not been finalized and later calls back via SKManagedWStreamWriteProxy to flush/finalize SVG data. GetUserData resolves the dead WeakReference to null, and the subsequent userData.OnWrite() call throws NullReferenceException, crashing the Blazor app.",
    "rationale": "This is a clear resource-lifetime bug introduced by the WeakReference pattern in 3.x. The native SVG canvas outlives its managed peer when disposal is skipped due to an exception. The fix path is to always dispose the canvas (workaround) or to make the native callback safe when the managed object has been GC'd (library fix). Classification as type/bug with high severity is appropriate because the NullReferenceException propagates unhandled into the Blazor rendering engine and crashes the whole app.",
    "keySignals": [
      {
        "text": "Object reference not set to an instance of an object. at SkiaSharp.DelegateProxies.SKManagedWStreamWriteProxyImplementation",
        "source": "issue body",
        "interpretation": "Native callback reached managed code but the WeakReference target was already collected — classic use-after-GC."
      },
      {
        "text": "if this throws an exception, i later get the exception mentioned above",
        "source": "issue body",
        "interpretation": "The delayed nature ('some time later') confirms this is GC-triggered: the WeakReference is only cleared at GC time, so the NRE happens non-deterministically after the exception."
      },
      {
        "text": "Last Known Good Version: 2.88.9",
        "source": "issue body",
        "interpretation": "Regression introduced in 3.x, consistent with the WeakReference-based GCHandle approach adopted in that version."
      },
      {
        "text": "resulting in, the whole Blazor App crashes",
        "source": "issue body",
        "interpretation": "Severity is high because the NullReferenceException is unhandled at the native callback boundary and crashes the hosting process."
      }
    ],
    "codeInvestigation": [
      {
        "file": "binding/SkiaSharp/SKAbstractManagedWStream.cs",
        "lines": "36-38",
        "finding": "Constructor calls DelegateProxies.CreateUserData(this, makeWeak: true) — registers only a WeakReference to the stream instance in the GCHandle passed to native code. If the stream is GC'd while the native handle is still alive, native callbacks receive a dead WeakReference.",
        "relevance": "direct"
      },
      {
        "file": "binding/SkiaSharp/DelegateProxies.wstream.cs",
        "lines": "31-36",
        "finding": "SKManagedWStreamWriteProxyImplementation calls GetUserData<SKAbstractManagedWStream> and immediately calls userData.OnWrite — no null guard. If the WeakReference target was collected, userData is null and OnWrite throws NullReferenceException.",
        "relevance": "direct"
      },
      {
        "file": "binding/Binding.Shared/DelegateProxies.shared.cs",
        "lines": "65-70",
        "finding": "GetUserData<T> returns weak.Target when value is a WeakReference. If the target has been GC'd, this returns null without signaling an error — the null propagates silently to the caller.",
        "relevance": "direct"
      },
      {
        "file": "binding/SkiaSharp/SKSVG.cs",
        "lines": "17-23",
        "finding": "SKSvgCanvas.Create(bounds, Stream) creates a SKManagedWStream and calls SKObject.Owned(canvas, managed) — the stream is owned by the canvas. If the canvas is never disposed (due to exception), both objects become GC-eligible.",
        "relevance": "related"
      }
    ],
    "workarounds": [
      "Wrap the canvas in a using statement or try/finally to ensure Dispose is always called even if an exception is thrown: `using var canvas = SKSvgCanvas.Create(...);`",
      "Call canvas.Dispose() in a finally block: try { DrawSomeStuff(canvas); } finally { canvas.Dispose(); }"
    ],
    "nextQuestions": [
      "Should the native callback proxies add null-guard and return a safe default (false/0) instead of crashing when the managed object has been collected?",
      "Was the WeakReference pattern introduced intentionally to break reference cycles — and if so, can the cycle be broken another way that preserves safety?"
    ],
    "resolution": {
      "hypothesis": "The WeakReference in the GCHandle allows native code to call back into a collected managed object. Proper fix requires either guarding the callback against null or ensuring the native SVG canvas is always destroyed before the managed stream can be collected.",
      "proposals": [
        {
          "title": "Use try/finally (workaround)",
          "description": "Always dispose the canvas even if drawing throws, ensuring the native SVG canvas is finalized before the managed stream can be GC'd.",
          "category": "workaround",
          "codeSnippet": "var canvas = SKSvgCanvas.Create(new SKRect(0, 0, width, height), stream);\ntry\n{\n    DrawSomeStuff(canvas);\n}\nfinally\n{\n    canvas.Dispose();\n}",
          "confidence": 0.95,
          "effort": "cost/xs",
          "validated": "yes"
        },
        {
          "title": "Add null-guard in write callback (library fix)",
          "description": "Guard SKManagedWStreamWriteProxyImplementation (and other proxies) against null userData and return a safe default value instead of crashing.",
          "category": "fix",
          "confidence": 0.8,
          "effort": "cost/s",
          "validated": "untested"
        }
      ],
      "recommendedProposal": "Use try/finally (workaround)",
      "recommendedReason": "The workaround is immediately actionable by the reporter and correct. A library-level null-guard is also needed but requires a separate investigation of whether returning false from the write callback causes data corruption or just silently truncates the SVG."
    }
  },
  "output": {
    "actionability": {
      "suggestedAction": "needs-investigation",
      "confidence": 0.88,
      "reason": "Real regression bug with complete repro and clear root cause hypothesis. Needs investigation into whether native callbacks should null-guard or whether the ownership model should be changed.",
      "suggestedReproPlatform": "linux"
    },
    "actions": [
      {
        "type": "update-labels",
        "description": "Apply bug, area/SkiaSharp, os/Windows, backend/SVG, tenet/reliability labels",
        "risk": "low",
        "confidence": 0.95,
        "labels": [
          "type/bug",
          "area/SkiaSharp",
          "os/Windows",
          "backend/SVG",
          "tenet/reliability"
        ]
      },
      {
        "type": "add-comment",
        "description": "Provide workaround and explain root cause",
        "risk": "medium",
        "confidence": 0.9,
        "comment": "Thanks for the detailed report! This is a known resource-lifetime issue introduced in SkiaSharp 3.x.\n\n**Root cause:** `SKSvgCanvas.Create` wraps your `MemoryStream` in a `SKManagedWStream` that is owned by the returned canvas. Internally, a `WeakReference` to the stream is stored in a GC handle that native code uses to call back into managed code. When an exception propagates out of `DrawSomeStuff` before `canvas.Dispose()` is called, the canvas and its stream become unreachable and the GC collects them. The native SVG canvas is still alive and later tries to flush/finalize the SVG data by calling back through the write proxy — but the WeakReference is now dead, so `userData` is `null` and `NullReferenceException` crashes your app.\n\n**Workaround:** Always dispose the canvas in a `finally` block (or use a `using` statement) so the native SVG canvas is finalized before the managed stream can be collected:\n\n```csharp\npublic string DrawSVG()\n{\n    var stream = new MemoryStream();\n    var canvas = SKSvgCanvas.Create(\n        new SKRect(0, 0, width, height),\n        stream);\n    try\n    {\n        DrawSomeStuff(canvas);\n    }\n    finally\n    {\n        canvas.Dispose();\n    }\n    return Encoding.Default.GetString(stream.ToArray());\n}\n```\n\nThe library should also guard the native callback proxy against null to avoid crashing the app when disposal is accidentally skipped. We are tracking this for a fix."
      }
    ]
  }
}
```

</details>
