# Issue Triage Report — #1442

| Field | Value |
|-------|-------|
| Repository | mono/SkiaSharp |
| Analyzed | 2026-04-28T21:02:36Z |
| Type | type/feature-request (0.88 (88%)) |
| Area | area/SkiaSharp (0.92 (92%)) |
| Suggested action | keep-open (0.80 (80%)) |

**Issue Summary:** Feature request to add a user-specified dispose callback on GPU-bound objects (e.g., SKSurface) so that finalization from the GC finalizer thread does not crash when the GPU context is not active on that thread.

**Analysis:** The SKNativeObject finalizer unconditionally calls Dispose(false) on the finalizer thread, but GPU-bound objects (SKSurface created with GRContext, GRContext itself) require the GPU context to be current on the disposing thread. There is no hook to redirect finalization to the correct thread. The proposed solution is a new disposeCallback API on GPU-bound factory methods.

**Recommendations:** **keep-open** — Valid feature request addressing a real crash scenario affecting production GPU apps. The disposeCallback API requires design discussion and ABI considerations. A workaround exists (explicit disposal + GC.SuppressFinalize). Keep open for future planning.

---

## Classification

| Field | Value |
|-------|-------|
| Type | type/feature-request |
| Area | area/SkiaSharp |
| Platforms | — |
| Backends | backend/OpenGL |
| Tenets | tenet/reliability |
| Partner | — |
| Current labels | type/bug, area/SkiaSharp, backend/OpenGL |

## Evidence

### Reproduction

1. Create a GPU-bound SkiaSharp object (e.g., SKSurface via GRContext)
2. Let the application exit via Main() exit or Environment.Exit(), or allow object to be GC'd without explicit Dispose()
3. The GC finalizer thread calls Dispose on the GPU-bound object without the GPU context being active on that thread
4. Observe crash (SIGSEGV / EXC_BAD_ACCESS or similar)

**Environment:** Any platform using GPU-backed SkiaSharp objects (OpenGL, Metal, etc.)

**Repository links:**
- https://github.com/AvaloniaUI/Avalonia/issues/4423 — Avalonia crash caused by this issue — GPU object finalized on wrong thread
- https://github.com/AvaloniaUI/Avalonia/pull/7887 — Avalonia workaround PR — disabling finalizer on SKFont to prevent crash
- https://github.com/mono/SkiaSharp/issues/2794 — Related: Access Violation on Finalizer/Dispose for SKPaint in SkiaSharp 3.x
- https://github.com/mono/SkiaSharp/issues/3178 — Related (closed/fixed): iOS crash when GC collects SKMetalView resources — GRContext finalized on wrong thread

## Analysis

### Technical Summary

The SKNativeObject finalizer unconditionally calls Dispose(false) on the finalizer thread, but GPU-bound objects (SKSurface created with GRContext, GRContext itself) require the GPU context to be current on the disposing thread. There is no hook to redirect finalization to the correct thread. The proposed solution is a new disposeCallback API on GPU-bound factory methods.

### Rationale

The issue title and body explicitly frame this as a [FEATURE] request — the reporter is asking for a new API (disposeCallback parameter on Create overloads) rather than reporting a regression in existing behavior. The underlying crash is real and confirmed by related issues and Avalonia workarounds, but the fix requires a new API surface. This is not a regression — the GPU-thread-safety requirement has always existed; the missing piece is a hook to route finalization to the right thread. Classification as type/feature-request is appropriate despite the crash potential, since no current API is broken; the missing mechanism is new functionality.

### Key Signals

- "[FEATURE] Controlled finalization for GPU-bound objects" — **issue title** (Reporter explicitly frames this as a feature request.)
- "Dispose might be called from the finalizer thread ... leads to crashes such as https://github.com/AvaloniaUI/Avalonia/issues/4423" — **issue body** (Real crash scenario affecting production apps using GPU-backed SkiaSharp (e.g., Avalonia).)
- "public static SKSurface Create(GRContext context, bool budgeted, SKImageInfo info, Action<Action> disposeCallback)" — **issue body** (Concrete API proposal for new disposeCallback parameter on GPU surface factory methods.)
- "This is the native stack trace ... EXC_BAD_ACCESS ... libSkiaSharp.dylib ... -[AvnWindow windowWillClose:]" — **comment by ltetak** (Confirms the crash affects macOS (Metal/OpenGL) when closing windows — GPU object finalized on wrong thread.)

### Code Investigation

| File | Lines | Relevance | Finding |
|------|-------|-----------|---------|
| `binding/SkiaSharp/SKObject.cs` | 229-234 | direct | SKNativeObject finalizer unconditionally calls Dispose(false) — no hook to redirect finalization to a GPU-context thread. No disposeCallback mechanism exists. |
| `binding/SkiaSharp/SKObject.cs` | 259-277 | direct | Dispose(bool disposing) calls DisposeNative() which for GPU objects requires the GPU context to be active; called from finalizer thread without any thread-affinity check. |
| `binding/SkiaSharp/GRContext.cs` | 18-23 | direct | GRContext.DisposeNative() calls AbandonContext() which requires the GPU context to be current — crashes if called from the finalizer thread without the context being active. |
| `binding/SkiaSharp/SKSurface.cs` | 78-180 | direct | GPU surface factory overloads (Create with GRContext) have no disposeCallback parameter — the requested API does not exist in current codebase. |

### Workarounds

- Ensure all GPU-bound SkiaSharp objects (SKSurface, GRContext) are explicitly disposed before application exit using using statements or Dispose() calls in cleanup handlers.
- Register a handler for AppDomain.CurrentDomain.ProcessExit or Application shutdown events to explicitly dispose GPU objects on the correct thread before the runtime exits.
- Suppress finalization on long-lived GPU objects with GC.SuppressFinalize() once ownership is established, and ensure Dispose() is called manually on the GPU thread (Avalonia workaround pattern from PR #7887).

### Next Questions

- Should the disposeCallback be per-object (on Create) or a global/context-level hook on GRContext?
- Is this still reproducible in SkiaSharp 3.x, or were any mitigations added since 2020?
- Does the fix in issue #3178 (iOS SKMetalView GC crash) address the same root cause?

### Resolution Proposals

**Hypothesis:** Introducing a disposeCallback parameter on GPU-bound factory methods would allow callers to marshal finalization to the correct GPU thread or context, preventing crashes when the GC finalizer thread disposes GPU-bound objects.

1. **Add disposeCallback to GPU surface factory overloads** — fix, confidence 0.75 (75%), cost/m
   - Add Action<Action> disposeCallback parameter to GPU-backed SKSurface.Create overloads (and optionally GRContext). When the finalizer fires and a callback is set, invoke the callback with the Dispose action instead of calling Dispose directly.
2. **Add a global GRContext finalizer delegate** — alternative, confidence 0.60 (60%), cost/l
   - Allow a global or per-GRContext hook that intercepts finalization of any GPU-bound object associated with that context, routing disposal to the GPU thread.
3. **Workaround: suppress finalizer and dispose explicitly** — workaround, confidence 0.90 (90%), cost/xs
   - Document and encourage callers to use GC.SuppressFinalize() on GPU-bound objects and explicitly dispose on the GPU thread. This is the current best practice used by Avalonia.

**Recommended proposal:** Workaround: suppress finalizer and dispose explicitly

**Why:** The API design for a disposeCallback needs careful thought (scope, threading semantics, ABI compatibility). In the meantime the workaround of explicit disposal with GC.SuppressFinalize() is well-tested by Avalonia.

## Recommendations

### Actionability

| Field | Value |
|-------|-------|
| Suggested action | keep-open |
| Confidence | 0.80 (80%) |
| Reason | Valid feature request addressing a real crash scenario affecting production GPU apps. The disposeCallback API requires design discussion and ABI considerations. A workaround exists (explicit disposal + GC.SuppressFinalize). Keep open for future planning. |
| Suggested repro platform | linux |

### Automatable Actions

| Type | Risk | Confidence | Description | Details |
|------|------|------------|-------------|---------|
| update-labels | low | 0.88 (88%) | Correct type label from type/bug to type/feature-request; add tenet/reliability | labels=type/feature-request, area/SkiaSharp, backend/OpenGL, tenet/reliability |
| add-comment | medium | 0.80 (80%) | Acknowledge the feature request, confirm the root cause, suggest workaround | — |
| link-related | low | 0.80 (80%) | Link to related crash issue #2794 | linkedIssue=#2794 |
| link-related | low | 0.80 (80%) | Link to related (closed) crash issue #3178 | linkedIssue=#3178 |

**Comment draft for `add-comment`:**

```markdown
Thanks for the detailed write-up and proposal!

This is a real problem — `SKNativeObject`'s finalizer calls `Dispose(false)` directly on the GC finalizer thread, but GPU-bound objects like `SKSurface` (backed by a `GRContext`) and `GRContext` itself require the GPU context to be current on the disposing thread. When `Main` exits or the GC collects an undisposed GPU object, the finalizer can fire on the wrong thread, causing the crash you described.

**Workaround (current best practice):**

Until a disposal-callback API is available, you can suppress finalization and ensure explicit disposal on the GPU thread:

```csharp
// After creating a GPU-bound object, suppress the finalizer
GC.SuppressFinalize(surface); // or GRContext, etc.

// In your GPU thread cleanup / app shutdown:
gpuThread.Invoke(() => surface.Dispose());
```

Alternatively, register for process exit and dispose GPU objects explicitly before the runtime tears down:

```csharp
AppDomain.CurrentDomain.ProcessExit += (_, _) =>
{
    // Dispose on GPU thread
    gpuDispatcher.Invoke(() =>
    {
        grContext?.Dispose();
        surface?.Dispose();
    });
};
```

This is the pattern used by Avalonia (see https://github.com/AvaloniaUI/Avalonia/pull/7887).

The proposed `disposeCallback` API is a good idea for a future release — it would allow proper marshalling of finalization to the correct thread without requiring callers to manually suppress the finalizer. This will need design work to ensure ABI compatibility and define the threading contract.
```

<details>
<summary>Raw JSON</summary>

```json
{
  "meta": {
    "schemaVersion": "1.0",
    "number": 1442,
    "repo": "mono/SkiaSharp",
    "analyzedAt": "2026-04-28T21:02:36Z",
    "currentLabels": [
      "type/bug",
      "area/SkiaSharp",
      "backend/OpenGL"
    ]
  },
  "summary": "Feature request to add a user-specified dispose callback on GPU-bound objects (e.g., SKSurface) so that finalization from the GC finalizer thread does not crash when the GPU context is not active on that thread.",
  "classification": {
    "type": {
      "value": "type/feature-request",
      "confidence": 0.88
    },
    "area": {
      "value": "area/SkiaSharp",
      "confidence": 0.92
    },
    "backends": [
      "backend/OpenGL"
    ],
    "tenets": [
      "tenet/reliability"
    ]
  },
  "evidence": {
    "reproEvidence": {
      "stepsToReproduce": [
        "Create a GPU-bound SkiaSharp object (e.g., SKSurface via GRContext)",
        "Let the application exit via Main() exit or Environment.Exit(), or allow object to be GC'd without explicit Dispose()",
        "The GC finalizer thread calls Dispose on the GPU-bound object without the GPU context being active on that thread",
        "Observe crash (SIGSEGV / EXC_BAD_ACCESS or similar)"
      ],
      "environmentDetails": "Any platform using GPU-backed SkiaSharp objects (OpenGL, Metal, etc.)",
      "repoLinks": [
        {
          "url": "https://github.com/AvaloniaUI/Avalonia/issues/4423",
          "description": "Avalonia crash caused by this issue — GPU object finalized on wrong thread"
        },
        {
          "url": "https://github.com/AvaloniaUI/Avalonia/pull/7887",
          "description": "Avalonia workaround PR — disabling finalizer on SKFont to prevent crash"
        },
        {
          "url": "https://github.com/mono/SkiaSharp/issues/2794",
          "description": "Related: Access Violation on Finalizer/Dispose for SKPaint in SkiaSharp 3.x"
        },
        {
          "url": "https://github.com/mono/SkiaSharp/issues/3178",
          "description": "Related (closed/fixed): iOS crash when GC collects SKMetalView resources — GRContext finalized on wrong thread"
        }
      ]
    }
  },
  "analysis": {
    "summary": "The SKNativeObject finalizer unconditionally calls Dispose(false) on the finalizer thread, but GPU-bound objects (SKSurface created with GRContext, GRContext itself) require the GPU context to be current on the disposing thread. There is no hook to redirect finalization to the correct thread. The proposed solution is a new disposeCallback API on GPU-bound factory methods.",
    "rationale": "The issue title and body explicitly frame this as a [FEATURE] request — the reporter is asking for a new API (disposeCallback parameter on Create overloads) rather than reporting a regression in existing behavior. The underlying crash is real and confirmed by related issues and Avalonia workarounds, but the fix requires a new API surface. This is not a regression — the GPU-thread-safety requirement has always existed; the missing piece is a hook to route finalization to the right thread. Classification as type/feature-request is appropriate despite the crash potential, since no current API is broken; the missing mechanism is new functionality.",
    "keySignals": [
      {
        "text": "[FEATURE] Controlled finalization for GPU-bound objects",
        "source": "issue title",
        "interpretation": "Reporter explicitly frames this as a feature request."
      },
      {
        "text": "Dispose might be called from the finalizer thread ... leads to crashes such as https://github.com/AvaloniaUI/Avalonia/issues/4423",
        "source": "issue body",
        "interpretation": "Real crash scenario affecting production apps using GPU-backed SkiaSharp (e.g., Avalonia)."
      },
      {
        "text": "public static SKSurface Create(GRContext context, bool budgeted, SKImageInfo info, Action<Action> disposeCallback)",
        "source": "issue body",
        "interpretation": "Concrete API proposal for new disposeCallback parameter on GPU surface factory methods."
      },
      {
        "text": "This is the native stack trace ... EXC_BAD_ACCESS ... libSkiaSharp.dylib ... -[AvnWindow windowWillClose:]",
        "source": "comment by ltetak",
        "interpretation": "Confirms the crash affects macOS (Metal/OpenGL) when closing windows — GPU object finalized on wrong thread."
      }
    ],
    "codeInvestigation": [
      {
        "file": "binding/SkiaSharp/SKObject.cs",
        "lines": "229-234",
        "finding": "SKNativeObject finalizer unconditionally calls Dispose(false) — no hook to redirect finalization to a GPU-context thread. No disposeCallback mechanism exists.",
        "relevance": "direct"
      },
      {
        "file": "binding/SkiaSharp/SKObject.cs",
        "lines": "259-277",
        "finding": "Dispose(bool disposing) calls DisposeNative() which for GPU objects requires the GPU context to be active; called from finalizer thread without any thread-affinity check.",
        "relevance": "direct"
      },
      {
        "file": "binding/SkiaSharp/GRContext.cs",
        "lines": "18-23",
        "finding": "GRContext.DisposeNative() calls AbandonContext() which requires the GPU context to be current — crashes if called from the finalizer thread without the context being active.",
        "relevance": "direct"
      },
      {
        "file": "binding/SkiaSharp/SKSurface.cs",
        "lines": "78-180",
        "finding": "GPU surface factory overloads (Create with GRContext) have no disposeCallback parameter — the requested API does not exist in current codebase.",
        "relevance": "direct"
      }
    ],
    "workarounds": [
      "Ensure all GPU-bound SkiaSharp objects (SKSurface, GRContext) are explicitly disposed before application exit using using statements or Dispose() calls in cleanup handlers.",
      "Register a handler for AppDomain.CurrentDomain.ProcessExit or Application shutdown events to explicitly dispose GPU objects on the correct thread before the runtime exits.",
      "Suppress finalization on long-lived GPU objects with GC.SuppressFinalize() once ownership is established, and ensure Dispose() is called manually on the GPU thread (Avalonia workaround pattern from PR #7887)."
    ],
    "nextQuestions": [
      "Should the disposeCallback be per-object (on Create) or a global/context-level hook on GRContext?",
      "Is this still reproducible in SkiaSharp 3.x, or were any mitigations added since 2020?",
      "Does the fix in issue #3178 (iOS SKMetalView GC crash) address the same root cause?"
    ],
    "resolution": {
      "hypothesis": "Introducing a disposeCallback parameter on GPU-bound factory methods would allow callers to marshal finalization to the correct GPU thread or context, preventing crashes when the GC finalizer thread disposes GPU-bound objects.",
      "proposals": [
        {
          "title": "Add disposeCallback to GPU surface factory overloads",
          "description": "Add Action<Action> disposeCallback parameter to GPU-backed SKSurface.Create overloads (and optionally GRContext). When the finalizer fires and a callback is set, invoke the callback with the Dispose action instead of calling Dispose directly.",
          "category": "fix",
          "confidence": 0.75,
          "effort": "cost/m"
        },
        {
          "title": "Add a global GRContext finalizer delegate",
          "description": "Allow a global or per-GRContext hook that intercepts finalization of any GPU-bound object associated with that context, routing disposal to the GPU thread.",
          "category": "alternative",
          "confidence": 0.6,
          "effort": "cost/l"
        },
        {
          "title": "Workaround: suppress finalizer and dispose explicitly",
          "description": "Document and encourage callers to use GC.SuppressFinalize() on GPU-bound objects and explicitly dispose on the GPU thread. This is the current best practice used by Avalonia.",
          "category": "workaround",
          "confidence": 0.9,
          "effort": "cost/xs"
        }
      ],
      "recommendedProposal": "Workaround: suppress finalizer and dispose explicitly",
      "recommendedReason": "The API design for a disposeCallback needs careful thought (scope, threading semantics, ABI compatibility). In the meantime the workaround of explicit disposal with GC.SuppressFinalize() is well-tested by Avalonia."
    }
  },
  "output": {
    "actionability": {
      "suggestedAction": "keep-open",
      "confidence": 0.8,
      "reason": "Valid feature request addressing a real crash scenario affecting production GPU apps. The disposeCallback API requires design discussion and ABI considerations. A workaround exists (explicit disposal + GC.SuppressFinalize). Keep open for future planning.",
      "suggestedReproPlatform": "linux"
    },
    "actions": [
      {
        "type": "update-labels",
        "description": "Correct type label from type/bug to type/feature-request; add tenet/reliability",
        "risk": "low",
        "confidence": 0.88,
        "labels": [
          "type/feature-request",
          "area/SkiaSharp",
          "backend/OpenGL",
          "tenet/reliability"
        ]
      },
      {
        "type": "add-comment",
        "description": "Acknowledge the feature request, confirm the root cause, suggest workaround",
        "risk": "medium",
        "confidence": 0.8,
        "comment": "Thanks for the detailed write-up and proposal!\n\nThis is a real problem — `SKNativeObject`'s finalizer calls `Dispose(false)` directly on the GC finalizer thread, but GPU-bound objects like `SKSurface` (backed by a `GRContext`) and `GRContext` itself require the GPU context to be current on the disposing thread. When `Main` exits or the GC collects an undisposed GPU object, the finalizer can fire on the wrong thread, causing the crash you described.\n\n**Workaround (current best practice):**\n\nUntil a disposal-callback API is available, you can suppress finalization and ensure explicit disposal on the GPU thread:\n\n```csharp\n// After creating a GPU-bound object, suppress the finalizer\nGC.SuppressFinalize(surface); // or GRContext, etc.\n\n// In your GPU thread cleanup / app shutdown:\ngpuThread.Invoke(() => surface.Dispose());\n```\n\nAlternatively, register for process exit and dispose GPU objects explicitly before the runtime tears down:\n\n```csharp\nAppDomain.CurrentDomain.ProcessExit += (_, _) =>\n{\n    // Dispose on GPU thread\n    gpuDispatcher.Invoke(() =>\n    {\n        grContext?.Dispose();\n        surface?.Dispose();\n    });\n};\n```\n\nThis is the pattern used by Avalonia (see https://github.com/AvaloniaUI/Avalonia/pull/7887).\n\nThe proposed `disposeCallback` API is a good idea for a future release — it would allow proper marshalling of finalization to the correct thread without requiring callers to manually suppress the finalizer. This will need design work to ensure ABI compatibility and define the threading contract."
      },
      {
        "type": "link-related",
        "description": "Link to related crash issue #2794",
        "risk": "low",
        "confidence": 0.8,
        "linkedIssue": 2794
      },
      {
        "type": "link-related",
        "description": "Link to related (closed) crash issue #3178",
        "risk": "low",
        "confidence": 0.8,
        "linkedIssue": 3178
      }
    ]
  }
}
```

</details>
