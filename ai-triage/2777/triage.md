# Issue Triage Report — #2777

| Field | Value |
|-------|-------|
| Repository | mono/SkiaSharp |
| Analyzed | 2026-05-01T23:08:36Z |
| Type | type/feature-request (0.97 (97%)) |
| Area | area/SkiaSharp (0.92 (92%)) |
| Suggested action | needs-investigation (0.85 (85%)) |

**Issue Summary:** Feature request to add logging and tracing support to the managed-native interop layer so developers can diagnose crashes caused by invalid object lifecycle or memory management issues.

**Analysis:** The reporter (project maintainer) wants an opt-in tracing/logging mechanism so users can capture where SKObject instances are created and disposed during managed-native interop, aiding diagnosis of use-after-free and double-dispose crashes. The code already has a `THROW_OBJECT_EXCEPTIONS` compile-time flag and a `#if DEBUG` stack trace dictionary in HandleDictionary.cs, but these are build-time options unavailable to end users.

**Recommendations:** **needs-investigation** — Well-scoped feature request from the maintainer with a clear motivation and referenced prior art. Needs design discussion on which .NET tracing API to use before implementation.

---

## Classification

| Field | Value |
|-------|-------|
| Type | type/feature-request |
| Area | area/SkiaSharp |
| Platforms | — |
| Backends | — |
| Tenets | tenet/reliability |
| Partner | — |
| Current labels | type/feature-request, tenet/reliability |

## Evidence

### Reproduction

**Repository links:**
- https://github.com/mono/SkiaSharp/pull/2157 — Referenced PR exploring diagnostic tracing for object lifecycle

## Analysis

### Technical Summary

The reporter (project maintainer) wants an opt-in tracing/logging mechanism so users can capture where SKObject instances are created and disposed during managed-native interop, aiding diagnosis of use-after-free and double-dispose crashes. The code already has a `THROW_OBJECT_EXCEPTIONS` compile-time flag and a `#if DEBUG` stack trace dictionary in HandleDictionary.cs, but these are build-time options unavailable to end users.

### Rationale

This is a clear feature request from the maintainer themselves. The issue describes a gap in diagnostics — no runtime mechanism exists for end users to trace object lifecycle. The referenced PR #2157 is described as insufficient. No behavior is broken; this is entirely additive new functionality.

### Key Signals

- "Today there is almost no way to see why the app has crashed due to invalid interop or memory management." — **issue body** (Core diagnostic gap: no runtime-accessible tracing for object lifecycle.)
- "something like this: https://github.com/mono/SkiaSharp/pull/2157 — Neither are great, and .NET has features so we should work using that." — **issue body** (Reporter wants a .NET-idiomatic solution (likely System.Diagnostics.ActivitySource or ILogger) rather than compile-time flags.)

### Code Investigation

| File | Lines | Relevance | Finding |
|------|-------|-----------|---------|
| `binding/SkiaSharp/HandleDictionary.cs` | 7-25 | direct | `THROW_OBJECT_EXCEPTIONS` compile-time flag enables exception throwing for handle conflicts. `#if DEBUG` captures stack traces in a dictionary. Both are compile-time only — not available to end users at runtime. |
| `binding/SkiaSharp/HandleDictionary.cs` | 113-129 | direct | Under `THROW_OBJECT_EXCEPTIONS`, InvalidOperationException is thrown for type mismatches and disposal of dead objects. These diagnostics could be exposed via a runtime tracing API instead. |
| `binding/SkiaSharp/DelegateProxies.trace.cs` | 1-29 | related | SKTraceMemoryDump already exists as a way to hook into Skia's native memory tracing, but does not cover managed object lifecycle (create/dispose events). |

### Next Questions

- Should this use System.Diagnostics.ActivitySource (distributed tracing), ILogger, DiagnosticSource, or a custom event hook?
- Should the tracing be opt-in at runtime (e.g., via an event handler or listener) or always-on with filtering?
- Should it cover only creation/disposal, or also ref-count changes and handle transfers?

### Resolution Proposals

**Hypothesis:** Expose a runtime-opt-in tracing mechanism in HandleDictionary/SKObject using .NET's System.Diagnostics.ActivitySource or DiagnosticSource so users can subscribe to object lifecycle events without needing a debug build.

1. **Add DiagnosticSource events for object lifecycle** — fix, confidence 0.75 (75%), cost/m, validated=untested
   - Use System.Diagnostics.DiagnosticSource to emit events on SKObject creation, disposal, and handle registration/deregistration. Listeners can subscribe without modifying SkiaSharp.
2. **Expose a static SkiaSharp.Diagnostics API with callbacks** — alternative, confidence 0.70 (70%), cost/m, validated=untested
   - Add a static SKDiagnostics class where users can register delegates for object lifecycle events (created, disposed, double-dispose attempt). Gives maximum flexibility without external dependencies.

**Recommended proposal:** Add DiagnosticSource events for object lifecycle

**Why:** .NET DiagnosticSource is the standard mechanism for diagnostic hooks; zero overhead when no listener is attached, and compatible with existing .NET tooling (EventPipe, dotnet-trace).

## Recommendations

### Actionability

| Field | Value |
|-------|-------|
| Suggested action | needs-investigation |
| Confidence | 0.85 (85%) |
| Reason | Well-scoped feature request from the maintainer with a clear motivation and referenced prior art. Needs design discussion on which .NET tracing API to use before implementation. |
| Suggested repro platform | linux |

### Automatable Actions

| Type | Risk | Confidence | Description | Details |
|------|------|------------|-------------|---------|
| update-labels | low | 0.97 (97%) | Confirm existing labels are correct | labels=type/feature-request, area/SkiaSharp, tenet/reliability |
| add-comment | medium | 0.85 (85%) | Summarize the design space and ask for preferred .NET tracing API | — |

**Comment draft for `add-comment`:**

```markdown
The existing `THROW_OBJECT_EXCEPTIONS` flag and `#if DEBUG` stack-trace capture in `HandleDictionary.cs` are the closest analogs today, but they require a special build.

Two .NET-idiomatic approaches worth considering:
1. **`System.Diagnostics.DiagnosticSource`** — zero overhead when no listener, integrates with `dotnet-trace` / EventPipe. Emit events on `SKObject` creation, disposal, and handle conflicts.
2. **Static `SKDiagnostics` callback API** — a simple delegate-based hook (`SKDiagnostics.ObjectCreated`, `ObjectDisposed`) that apps register at startup.

PR #2157 explored a custom approach. Would a `DiagnosticSource`-based design be the right direction, or is there a preference for the simpler static callback API?
```

<details>
<summary>Raw JSON</summary>

```json
{
  "meta": {
    "schemaVersion": "1.0",
    "number": 2777,
    "repo": "mono/SkiaSharp",
    "analyzedAt": "2026-05-01T23:08:36Z",
    "currentLabels": [
      "type/feature-request",
      "tenet/reliability"
    ]
  },
  "summary": "Feature request to add logging and tracing support to the managed-native interop layer so developers can diagnose crashes caused by invalid object lifecycle or memory management issues.",
  "classification": {
    "type": {
      "value": "type/feature-request",
      "confidence": 0.97
    },
    "area": {
      "value": "area/SkiaSharp",
      "confidence": 0.92
    },
    "tenets": [
      "tenet/reliability"
    ]
  },
  "evidence": {
    "reproEvidence": {
      "repoLinks": [
        {
          "url": "https://github.com/mono/SkiaSharp/pull/2157",
          "description": "Referenced PR exploring diagnostic tracing for object lifecycle"
        }
      ]
    }
  },
  "analysis": {
    "summary": "The reporter (project maintainer) wants an opt-in tracing/logging mechanism so users can capture where SKObject instances are created and disposed during managed-native interop, aiding diagnosis of use-after-free and double-dispose crashes. The code already has a `THROW_OBJECT_EXCEPTIONS` compile-time flag and a `#if DEBUG` stack trace dictionary in HandleDictionary.cs, but these are build-time options unavailable to end users.",
    "rationale": "This is a clear feature request from the maintainer themselves. The issue describes a gap in diagnostics — no runtime mechanism exists for end users to trace object lifecycle. The referenced PR #2157 is described as insufficient. No behavior is broken; this is entirely additive new functionality.",
    "codeInvestigation": [
      {
        "file": "binding/SkiaSharp/HandleDictionary.cs",
        "lines": "7-25",
        "finding": "`THROW_OBJECT_EXCEPTIONS` compile-time flag enables exception throwing for handle conflicts. `#if DEBUG` captures stack traces in a dictionary. Both are compile-time only — not available to end users at runtime.",
        "relevance": "direct"
      },
      {
        "file": "binding/SkiaSharp/HandleDictionary.cs",
        "lines": "113-129",
        "finding": "Under `THROW_OBJECT_EXCEPTIONS`, InvalidOperationException is thrown for type mismatches and disposal of dead objects. These diagnostics could be exposed via a runtime tracing API instead.",
        "relevance": "direct"
      },
      {
        "file": "binding/SkiaSharp/DelegateProxies.trace.cs",
        "lines": "1-29",
        "finding": "SKTraceMemoryDump already exists as a way to hook into Skia's native memory tracing, but does not cover managed object lifecycle (create/dispose events).",
        "relevance": "related"
      }
    ],
    "keySignals": [
      {
        "text": "Today there is almost no way to see why the app has crashed due to invalid interop or memory management.",
        "source": "issue body",
        "interpretation": "Core diagnostic gap: no runtime-accessible tracing for object lifecycle."
      },
      {
        "text": "something like this: https://github.com/mono/SkiaSharp/pull/2157 — Neither are great, and .NET has features so we should work using that.",
        "source": "issue body",
        "interpretation": "Reporter wants a .NET-idiomatic solution (likely System.Diagnostics.ActivitySource or ILogger) rather than compile-time flags."
      }
    ],
    "nextQuestions": [
      "Should this use System.Diagnostics.ActivitySource (distributed tracing), ILogger, DiagnosticSource, or a custom event hook?",
      "Should the tracing be opt-in at runtime (e.g., via an event handler or listener) or always-on with filtering?",
      "Should it cover only creation/disposal, or also ref-count changes and handle transfers?"
    ],
    "resolution": {
      "hypothesis": "Expose a runtime-opt-in tracing mechanism in HandleDictionary/SKObject using .NET's System.Diagnostics.ActivitySource or DiagnosticSource so users can subscribe to object lifecycle events without needing a debug build.",
      "proposals": [
        {
          "title": "Add DiagnosticSource events for object lifecycle",
          "description": "Use System.Diagnostics.DiagnosticSource to emit events on SKObject creation, disposal, and handle registration/deregistration. Listeners can subscribe without modifying SkiaSharp.",
          "category": "fix",
          "confidence": 0.75,
          "effort": "cost/m",
          "validated": "untested"
        },
        {
          "title": "Expose a static SkiaSharp.Diagnostics API with callbacks",
          "description": "Add a static SKDiagnostics class where users can register delegates for object lifecycle events (created, disposed, double-dispose attempt). Gives maximum flexibility without external dependencies.",
          "category": "alternative",
          "confidence": 0.7,
          "effort": "cost/m",
          "validated": "untested"
        }
      ],
      "recommendedProposal": "Add DiagnosticSource events for object lifecycle",
      "recommendedReason": ".NET DiagnosticSource is the standard mechanism for diagnostic hooks; zero overhead when no listener is attached, and compatible with existing .NET tooling (EventPipe, dotnet-trace)."
    }
  },
  "output": {
    "actionability": {
      "suggestedAction": "needs-investigation",
      "confidence": 0.85,
      "reason": "Well-scoped feature request from the maintainer with a clear motivation and referenced prior art. Needs design discussion on which .NET tracing API to use before implementation.",
      "suggestedReproPlatform": "linux"
    },
    "actions": [
      {
        "type": "update-labels",
        "description": "Confirm existing labels are correct",
        "risk": "low",
        "confidence": 0.97,
        "labels": [
          "type/feature-request",
          "area/SkiaSharp",
          "tenet/reliability"
        ]
      },
      {
        "type": "add-comment",
        "description": "Summarize the design space and ask for preferred .NET tracing API",
        "risk": "medium",
        "confidence": 0.85,
        "comment": "The existing `THROW_OBJECT_EXCEPTIONS` flag and `#if DEBUG` stack-trace capture in `HandleDictionary.cs` are the closest analogs today, but they require a special build.\n\nTwo .NET-idiomatic approaches worth considering:\n1. **`System.Diagnostics.DiagnosticSource`** — zero overhead when no listener, integrates with `dotnet-trace` / EventPipe. Emit events on `SKObject` creation, disposal, and handle conflicts.\n2. **Static `SKDiagnostics` callback API** — a simple delegate-based hook (`SKDiagnostics.ObjectCreated`, `ObjectDisposed`) that apps register at startup.\n\nPR #2157 explored a custom approach. Would a `DiagnosticSource`-based design be the right direction, or is there a preference for the simpler static callback API?"
      }
    ]
  }
}
```

</details>
