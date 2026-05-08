# Issue Triage Report — #3322

| Field | Value |
|-------|-------|
| Repository | mono/SkiaSharp |
| Analyzed | 2026-04-30T22:45:36Z |
| Type | type/bug (0.97 (97%)) |
| Area | area/SkiaSharp.Views.Blazor (0.97 (97%)) |
| Suggested action | needs-investigation (0.88 (88%)) |

**Issue Summary:** SKCanvasView in Blazor/WASM throws a JavaScript TypeError from ResizeObserver.unobserve when multiple SKCanvasView components are shown and hidden, because SizeWatcher.unobserve calls observer.unobserve(undefined) when the element is not found in the internal map.

**Analysis:** The JavaScript SizeWatcher.unobserve function retrieves the element from the internal map using elements.get(elementId), which returns undefined if the element was never observed or was already removed. It then passes this undefined value to ResizeObserver.unobserve(), which throws a TypeError because it requires a valid Element. This scenario arises when a Blazor component is disposed before or concurrently with the observe call completing, or when multiple dispose cycles run.

**Recommendations:** **needs-investigation** — Root cause is identified in SizeWatcher.ts — unobserve passes undefined to ResizeObserver. Needs a targeted fix in the JS and/or C# layer with proper testing.

---

## Classification

| Field | Value |
|-------|-------|
| Type | type/bug |
| Area | area/SkiaSharp.Views.Blazor |
| Platforms | os/WASM |
| Backends | — |
| Tenets | — |
| Partner | — |
| Current labels | type/bug |

## Evidence

### Reproduction

1. Create a Blazor WASM application with multiple SKCanvasView components
2. Show and hide those components dynamically (conditional rendering)
3. Observe the unhandled exception in the browser console during component disposal

**Environment:** SkiaSharp 3.116.0, .NET 8.0 Blazor WASM, Visual Studio on Windows

**Related issues:** #2441

**Repository links:**
- https://github.com/mono/SkiaSharp/issues/2441 — Prior related issue: NullReferenceException in SKCanvasView.Dispose() — same reporter, same scenario, partially fixed in 3.x but a new JS-level error remains

### Bug Signals

| Field | Value |
|-------|-------|
| Severity | medium |
| Regression claimed | False |
| Error type | exception |
| Error message | TypeError: Failed to execute 'unobserve' on 'ResizeObserver': parameter 1 is not of type 'Element'. |
| Repro quality | partial |
| Target frameworks | net8.0 |

**Stack trace:**

```text
at SkiaSharp.Views.Blazor.Internal.SizeWatcherInterop.Stop() at SkiaSharp.Views.Blazor.Internal.SizeWatcherInterop.OnDisposingModule() at SkiaSharp.Views.Blazor.Internal.JSModuleInterop.Dispose() at SkiaSharp.Views.Blazor.SKCanvasView.Dispose()
```

### Version Analysis

| Field | Value |
|-------|-------|
| Mentioned versions | 3.116.0 |
| Worked in | — |
| Broke in | — |
| Current relevance | likely |
| Relevance reason | The SizeWatcher.ts unobserve code has not changed to add a guard for undefined elements. |

## Analysis

### Technical Summary

The JavaScript SizeWatcher.unobserve function retrieves the element from the internal map using elements.get(elementId), which returns undefined if the element was never observed or was already removed. It then passes this undefined value to ResizeObserver.unobserve(), which throws a TypeError because it requires a valid Element. This scenario arises when a Blazor component is disposed before or concurrently with the observe call completing, or when multiple dispose cycles run.

### Rationale

The stack trace is clear: Stop() -> Unobserve JS call -> ResizeObserver.unobserve(undefined). The TypeScript source confirms no null/undefined guard before calling observer.unobserve(element). This is a real bug in the JS layer, not a usage error. The prior issue #2441 fixed a C# NullReferenceException in the same scenario but did not address the underlying JS-level problem.

### Key Signals

- "TypeError: Failed to execute 'unobserve' on 'ResizeObserver': parameter 1 is not of type 'Element'" — **issue body** (JavaScript ResizeObserver.unobserve is called with undefined — means elements.get(elementId) returned undefined in SizeWatcher.ts)
- "When show and hide multiple SKCanvasView" — **issue body** (Race condition or re-entrant disposal scenario — component disposed before observe() registered the element)
- "Dispose() in SKCanvasView() does not follow proper .net Dispose pattern, it lacks a virtual Dispose(bool) method" — **issue body** (Secondary concern: non-standard dispose pattern prevents subclasses from suppressing the exception)

### Code Investigation

| File | Lines | Relevance | Finding |
|------|-------|-----------|---------|
| `source/SkiaSharp.Views/SkiaSharp.Views.Blazor/wwwroot/SizeWatcher.ts` | 37-46 | direct | unobserve() calls elements.get(elementId) which returns undefined if the element was never added or was already removed, then passes undefined directly to SizeWatcher.observer.unobserve(element) — no null/undefined guard before the call. |
| `source/SkiaSharp.Views/SkiaSharp.Views.Blazor/Internal/SizeWatcherInterop.cs` | 47-48 | direct | OnDisposingModule() unconditionally calls Stop(), which invokes JS unobserve — no check whether Start() was ever successfully called or whether the element was actually registered. |
| `source/SkiaSharp.Views/SkiaSharp.Views.Blazor/SKCanvasView.razor.cs` | 171-178 | related | Dispose() uses null-conditional operator on sizeWatcher but sizeWatcher is declared as null! — if the component is disposed before OnAfterRenderAsync completes, sizeWatcher may be partially initialized, yet Dispose() will still call Stop() on it. |
| `source/SkiaSharp.Views/SkiaSharp.Views.Blazor/Internal/JSModuleInterop.cs` | 37-41 | related | JSModuleInterop.Dispose() calls OnDisposingModule() then Module.Dispose(). Module property throws InvalidOperationException if ImportAsync was not awaited — potential second crash path if disposed very early. |

### Next Questions

- Does the exception also occur with the NET7+ JSImport path (SizeWatcherInterop.cs #if NET7_0_OR_GREATER block), or only the pre-NET7 path?
- Is the component disposed before or after OnAfterRenderAsync completes (timing)?
- Does adding a null/undefined guard in SizeWatcher.ts unobserve fully resolve the exception?

### Resolution Proposals

**Hypothesis:** Adding a null/undefined check in SizeWatcher.ts before calling observer.unobserve(element) will prevent the TypeError. Additionally, guarding Stop() in SizeWatcherInterop.cs to only call unobserve if Start() was actually called would be a belt-and-suspenders fix.

1. **Guard unobserve in JavaScript** — fix, confidence 0.90 (90%), cost/xs, validated=untested
   - In SizeWatcher.ts, check that element is not undefined before calling observer.unobserve(element).

```csharp
public static unobserve(elementId: string) {
  if (!elementId || !SizeWatcher.observer)
    return;
  const element = SizeWatcher.elements.get(elementId);
  SizeWatcher.elements.delete(elementId);
  if (element) {
    SizeWatcher.observer.unobserve(element);
  }
}
```
2. **Track Start/Stop state in SizeWatcherInterop** — fix, confidence 0.80 (80%), cost/s, validated=untested
   - Add a boolean flag in SizeWatcherInterop to track whether Start() was called. Only call Stop() (and therefore JS unobserve) if Start() succeeded.

**Recommended proposal:** Guard unobserve in JavaScript

**Why:** The fix is in the JavaScript layer where the error occurs. One-line guard prevents the TypeError regardless of how the C# dispose is called.

## Recommendations

### Actionability

| Field | Value |
|-------|-------|
| Suggested action | needs-investigation |
| Confidence | 0.88 (88%) |
| Reason | Root cause is identified in SizeWatcher.ts — unobserve passes undefined to ResizeObserver. Needs a targeted fix in the JS and/or C# layer with proper testing. |
| Suggested repro platform | linux |

### Automatable Actions

| Type | Risk | Confidence | Description | Details |
|------|------|------------|-------------|---------|
| update-labels | low | 0.95 (95%) | Apply Blazor area, WASM platform, and reliability tenet labels | labels=type/bug, area/SkiaSharp.Views.Blazor, os/WASM, tenet/reliability |
| add-comment | medium | 0.88 (88%) | Acknowledge bug, explain root cause, suggest fix path | — |
| link-related | low | 0.92 (92%) | Link to prior related Blazor dispose issue | linkedIssue=#2441 |

**Comment draft for `add-comment`:**

```markdown
Thanks for the detailed stack trace!

The root cause is in `SizeWatcher.ts` — the `unobserve` function calls `elements.get(elementId)` which returns `undefined` if the element was never registered (e.g., component disposed before or during `observe` completing), and then passes that `undefined` to `ResizeObserver.unobserve()`, which requires a valid `Element`.

The fix is to add a null/undefined guard in the JavaScript before calling `observer.unobserve(element)`:

```typescript
public static unobserve(elementId: string) {
  if (!elementId || !SizeWatcher.observer)
    return;
  const element = SizeWatcher.elements.get(elementId);
  SizeWatcher.elements.delete(elementId);
  if (element) {
    SizeWatcher.observer.unobserve(element);
  }
}
```

This is a regression from the fix applied in the #2441 era — the C# null-reference was addressed but the underlying JS-level unobserve issue was not.

Regarding the `Dispose(bool)` pattern: that's a valid secondary observation. The current `IDisposable` implementation doesn't allow subclasses to suppress the exception during finalization. Adding a `protected virtual void Dispose(bool disposing)` would follow the standard pattern.
```

<details>
<summary>Raw JSON</summary>

```json
{
  "meta": {
    "schemaVersion": "1.0",
    "number": 3322,
    "repo": "mono/SkiaSharp",
    "analyzedAt": "2026-04-30T22:45:36Z",
    "currentLabels": [
      "type/bug"
    ]
  },
  "summary": "SKCanvasView in Blazor/WASM throws a JavaScript TypeError from ResizeObserver.unobserve when multiple SKCanvasView components are shown and hidden, because SizeWatcher.unobserve calls observer.unobserve(undefined) when the element is not found in the internal map.",
  "classification": {
    "type": {
      "value": "type/bug",
      "confidence": 0.97
    },
    "area": {
      "value": "area/SkiaSharp.Views.Blazor",
      "confidence": 0.97
    },
    "platforms": [
      "os/WASM"
    ]
  },
  "evidence": {
    "bugSignals": {
      "severity": "medium",
      "regressionClaimed": false,
      "errorType": "exception",
      "errorMessage": "TypeError: Failed to execute 'unobserve' on 'ResizeObserver': parameter 1 is not of type 'Element'.",
      "stackTrace": "at SkiaSharp.Views.Blazor.Internal.SizeWatcherInterop.Stop() at SkiaSharp.Views.Blazor.Internal.SizeWatcherInterop.OnDisposingModule() at SkiaSharp.Views.Blazor.Internal.JSModuleInterop.Dispose() at SkiaSharp.Views.Blazor.SKCanvasView.Dispose()",
      "reproQuality": "partial",
      "targetFrameworks": [
        "net8.0"
      ]
    },
    "reproEvidence": {
      "stepsToReproduce": [
        "Create a Blazor WASM application with multiple SKCanvasView components",
        "Show and hide those components dynamically (conditional rendering)",
        "Observe the unhandled exception in the browser console during component disposal"
      ],
      "environmentDetails": "SkiaSharp 3.116.0, .NET 8.0 Blazor WASM, Visual Studio on Windows",
      "relatedIssues": [
        2441
      ],
      "repoLinks": [
        {
          "url": "https://github.com/mono/SkiaSharp/issues/2441",
          "description": "Prior related issue: NullReferenceException in SKCanvasView.Dispose() — same reporter, same scenario, partially fixed in 3.x but a new JS-level error remains"
        }
      ]
    },
    "versionAnalysis": {
      "mentionedVersions": [
        "3.116.0"
      ],
      "currentRelevance": "likely",
      "relevanceReason": "The SizeWatcher.ts unobserve code has not changed to add a guard for undefined elements."
    }
  },
  "analysis": {
    "summary": "The JavaScript SizeWatcher.unobserve function retrieves the element from the internal map using elements.get(elementId), which returns undefined if the element was never observed or was already removed. It then passes this undefined value to ResizeObserver.unobserve(), which throws a TypeError because it requires a valid Element. This scenario arises when a Blazor component is disposed before or concurrently with the observe call completing, or when multiple dispose cycles run.",
    "rationale": "The stack trace is clear: Stop() -> Unobserve JS call -> ResizeObserver.unobserve(undefined). The TypeScript source confirms no null/undefined guard before calling observer.unobserve(element). This is a real bug in the JS layer, not a usage error. The prior issue #2441 fixed a C# NullReferenceException in the same scenario but did not address the underlying JS-level problem.",
    "keySignals": [
      {
        "text": "TypeError: Failed to execute 'unobserve' on 'ResizeObserver': parameter 1 is not of type 'Element'",
        "source": "issue body",
        "interpretation": "JavaScript ResizeObserver.unobserve is called with undefined — means elements.get(elementId) returned undefined in SizeWatcher.ts"
      },
      {
        "text": "When show and hide multiple SKCanvasView",
        "source": "issue body",
        "interpretation": "Race condition or re-entrant disposal scenario — component disposed before observe() registered the element"
      },
      {
        "text": "Dispose() in SKCanvasView() does not follow proper .net Dispose pattern, it lacks a virtual Dispose(bool) method",
        "source": "issue body",
        "interpretation": "Secondary concern: non-standard dispose pattern prevents subclasses from suppressing the exception"
      }
    ],
    "codeInvestigation": [
      {
        "file": "source/SkiaSharp.Views/SkiaSharp.Views.Blazor/wwwroot/SizeWatcher.ts",
        "lines": "37-46",
        "finding": "unobserve() calls elements.get(elementId) which returns undefined if the element was never added or was already removed, then passes undefined directly to SizeWatcher.observer.unobserve(element) — no null/undefined guard before the call.",
        "relevance": "direct"
      },
      {
        "file": "source/SkiaSharp.Views/SkiaSharp.Views.Blazor/Internal/SizeWatcherInterop.cs",
        "lines": "47-48",
        "finding": "OnDisposingModule() unconditionally calls Stop(), which invokes JS unobserve — no check whether Start() was ever successfully called or whether the element was actually registered.",
        "relevance": "direct"
      },
      {
        "file": "source/SkiaSharp.Views/SkiaSharp.Views.Blazor/SKCanvasView.razor.cs",
        "lines": "171-178",
        "finding": "Dispose() uses null-conditional operator on sizeWatcher but sizeWatcher is declared as null! — if the component is disposed before OnAfterRenderAsync completes, sizeWatcher may be partially initialized, yet Dispose() will still call Stop() on it.",
        "relevance": "related"
      },
      {
        "file": "source/SkiaSharp.Views/SkiaSharp.Views.Blazor/Internal/JSModuleInterop.cs",
        "lines": "37-41",
        "finding": "JSModuleInterop.Dispose() calls OnDisposingModule() then Module.Dispose(). Module property throws InvalidOperationException if ImportAsync was not awaited — potential second crash path if disposed very early.",
        "relevance": "related"
      }
    ],
    "nextQuestions": [
      "Does the exception also occur with the NET7+ JSImport path (SizeWatcherInterop.cs #if NET7_0_OR_GREATER block), or only the pre-NET7 path?",
      "Is the component disposed before or after OnAfterRenderAsync completes (timing)?",
      "Does adding a null/undefined guard in SizeWatcher.ts unobserve fully resolve the exception?"
    ],
    "resolution": {
      "hypothesis": "Adding a null/undefined check in SizeWatcher.ts before calling observer.unobserve(element) will prevent the TypeError. Additionally, guarding Stop() in SizeWatcherInterop.cs to only call unobserve if Start() was actually called would be a belt-and-suspenders fix.",
      "proposals": [
        {
          "title": "Guard unobserve in JavaScript",
          "description": "In SizeWatcher.ts, check that element is not undefined before calling observer.unobserve(element).",
          "category": "fix",
          "codeSnippet": "public static unobserve(elementId: string) {\n  if (!elementId || !SizeWatcher.observer)\n    return;\n  const element = SizeWatcher.elements.get(elementId);\n  SizeWatcher.elements.delete(elementId);\n  if (element) {\n    SizeWatcher.observer.unobserve(element);\n  }\n}",
          "confidence": 0.9,
          "effort": "cost/xs",
          "validated": "untested"
        },
        {
          "title": "Track Start/Stop state in SizeWatcherInterop",
          "description": "Add a boolean flag in SizeWatcherInterop to track whether Start() was called. Only call Stop() (and therefore JS unobserve) if Start() succeeded.",
          "category": "fix",
          "confidence": 0.8,
          "effort": "cost/s",
          "validated": "untested"
        }
      ],
      "recommendedProposal": "Guard unobserve in JavaScript",
      "recommendedReason": "The fix is in the JavaScript layer where the error occurs. One-line guard prevents the TypeError regardless of how the C# dispose is called."
    }
  },
  "output": {
    "actionability": {
      "suggestedAction": "needs-investigation",
      "confidence": 0.88,
      "reason": "Root cause is identified in SizeWatcher.ts — unobserve passes undefined to ResizeObserver. Needs a targeted fix in the JS and/or C# layer with proper testing.",
      "suggestedReproPlatform": "linux"
    },
    "actions": [
      {
        "type": "update-labels",
        "description": "Apply Blazor area, WASM platform, and reliability tenet labels",
        "risk": "low",
        "confidence": 0.95,
        "labels": [
          "type/bug",
          "area/SkiaSharp.Views.Blazor",
          "os/WASM",
          "tenet/reliability"
        ]
      },
      {
        "type": "add-comment",
        "description": "Acknowledge bug, explain root cause, suggest fix path",
        "risk": "medium",
        "confidence": 0.88,
        "comment": "Thanks for the detailed stack trace!\n\nThe root cause is in `SizeWatcher.ts` — the `unobserve` function calls `elements.get(elementId)` which returns `undefined` if the element was never registered (e.g., component disposed before or during `observe` completing), and then passes that `undefined` to `ResizeObserver.unobserve()`, which requires a valid `Element`.\n\nThe fix is to add a null/undefined guard in the JavaScript before calling `observer.unobserve(element)`:\n\n```typescript\npublic static unobserve(elementId: string) {\n  if (!elementId || !SizeWatcher.observer)\n    return;\n  const element = SizeWatcher.elements.get(elementId);\n  SizeWatcher.elements.delete(elementId);\n  if (element) {\n    SizeWatcher.observer.unobserve(element);\n  }\n}\n```\n\nThis is a regression from the fix applied in the #2441 era — the C# null-reference was addressed but the underlying JS-level unobserve issue was not.\n\nRegarding the `Dispose(bool)` pattern: that's a valid secondary observation. The current `IDisposable` implementation doesn't allow subclasses to suppress the exception during finalization. Adding a `protected virtual void Dispose(bool disposing)` would follow the standard pattern."
      },
      {
        "type": "link-related",
        "description": "Link to prior related Blazor dispose issue",
        "risk": "low",
        "confidence": 0.92,
        "linkedIssue": 2441
      }
    ]
  }
}
```

</details>
