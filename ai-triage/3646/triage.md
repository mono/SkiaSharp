# Issue Triage Report — #3646

| Field | Value |
|-------|-------|
| Repository | mono/SkiaSharp |
| Analyzed | 2026-07-13T05:20:00Z |
| Type | type/question (0.85 (85%)) |
| Area | area/SkiaSharp (0.95 (95%)) |
| Suggested action | close-as-not-a-bug (0.90 (90%)) |

**Issue Summary:** Reporter claims the virtual modifier on SKNativeObject.Handle is meaningless and impacts performance, but code investigation shows that SKObject and SKPath both override this property, making the virtual modifier necessary.

**Analysis:** The reporter asserts that no derived class overrides SKNativeObject.Handle, making the virtual modifier superfluous and potentially hurting performance. However, code investigation shows SKObject explicitly overrides Handle (binding/SkiaSharp/SKObject.cs:47) to add handle registration/deregistration logic, and SKPath further overrides it (binding/SkiaSharp/SKPath.cs:35) to flush its internal builder before returning the handle. The virtual dispatch chain is intentional and required by the design. The performance concern is understandable but the virtual modifier cannot be removed without breaking the design.

**Recommendations:** **close-as-not-a-bug** — The virtual modifier on SKNativeObject.Handle is necessary: SKObject overrides it to manage handle registration, and SKPath overrides it to flush its builder. The reporter's assumption that no class overrides it (other than SKObject) is incorrect.

---

## Classification

| Field | Value |
|-------|-------|
| Type | type/question |
| Area | area/SkiaSharp |
| Platforms | — |
| Backends | — |
| Tenets | — |
| Perf | — |
| Partner | — |

## Evidence

### Reproduction

**Environment:** ~5 million get_Handle calls taking ~400ms on reporter's machine

## Analysis

### Technical Summary

The reporter asserts that no derived class overrides SKNativeObject.Handle, making the virtual modifier superfluous and potentially hurting performance. However, code investigation shows SKObject explicitly overrides Handle (binding/SkiaSharp/SKObject.cs:47) to add handle registration/deregistration logic, and SKPath further overrides it (binding/SkiaSharp/SKPath.cs:35) to flush its internal builder before returning the handle. The virtual dispatch chain is intentional and required by the design. The performance concern is understandable but the virtual modifier cannot be removed without breaking the design.

### Rationale

The reporter's core premise is incorrect: SKObject and SKPath both override SKNativeObject.Handle with meaningful behavior (handle registration and builder flushing respectively). The virtual modifier is intentional and necessary. This is a misunderstanding of the codebase, not a bug or actionable enhancement. Closing as not-a-bug is appropriate with an explanation.

### Key Signals

- "no derived class overrides SKNativeObject.Handle property (except SKObject, of course)" — **issue body** (Reporter's assertion is factually incorrect — SKPath also overrides Handle with different logic.)
- "~400ms for ~5kk get_Handle calls" — **issue body** (Performance micro-benchmark; virtual dispatch in .NET is typically devirtualized by JIT for concrete types in hot paths.)

### Code Investigation

| File | Lines | Relevance | Finding |
|------|-------|-----------|---------|
| `binding/SkiaSharp/SKObject.cs` | 219,253 | direct | SKNativeObject declares Handle as virtual at line 253. SKObject overrides it at line 47 to add handle registration (RegisterHandle) and deregistration (DeregisterHandle) on assignment. |
| `binding/SkiaSharp/SKPath.cs` | 35-40 | direct | SKPath overrides Handle to call FlushBuilder() before returning the base handle, ensuring pending path mutations are applied before the handle is passed to native code. |

### Resolution Proposals

1. **Close as not-a-bug with explanation** — investigation, cost/xs, validated=untested
   - Close the issue explaining that SKObject and SKPath both override Handle with meaningful behavior, making the virtual modifier necessary.

**Recommended proposal:** close-1

**Why:** The virtual modifier is required because SKObject and SKPath both override Handle. Removing it would break the design.

## Recommendations

### Actionability

| Field | Value |
|-------|-------|
| Suggested action | close-as-not-a-bug |
| Confidence | 0.90 (90%) |
| Reason | The virtual modifier on SKNativeObject.Handle is necessary: SKObject overrides it to manage handle registration, and SKPath overrides it to flush its builder. The reporter's assumption that no class overrides it (other than SKObject) is incorrect. |
| Suggested repro platform | linux |

### Automatable Actions

| Type | Risk | Confidence | Description | Details |
|------|------|------------|-------------|---------|
| update-labels | low | 0.90 (90%) | Apply classification labels | labels=type/question, area/SkiaSharp |
| add-comment | high | 0.90 (90%) | Explain that the virtual modifier is necessary because SKObject and SKPath both override Handle. | — |
| close-issue | medium | 0.90 (90%) | Close as not a bug after commenting | stateReason=not_planned |

**Comment draft for `add-comment`:**

```markdown
Thanks for raising this! After looking at the code, the `virtual` modifier on `SKNativeObject.Handle` is actually necessary — it's not meaningless.

Two classes override it with meaningful behavior:

1. **`SKObject.Handle`** (`SKObject.cs` line 47) — overrides the setter to call `RegisterHandle` and `DeregisterHandle`, which are critical for the handle-tracking dictionary that prevents double-disposal and enables handle reuse.
2. **`SKPath.Handle`** (`SKPath.cs` line 35) — overrides the getter to call `FlushBuilder()` before returning the handle, ensuring any pending path mutations are applied before the native handle is passed to Skia.

Removing the `virtual` modifier would break both of these overrides.

Regarding the performance concern: modern .NET JIT is very good at devirtualizing property getters, especially for concrete (non-abstract) call sites. The ~400ms for 5M calls benchmark may be affected by other factors (e.g., the measurement infrastructure itself). If you have a specific hot path in mind, feel free to share more details and we can look at targeted optimizations.

Closing this as not a bug. Please reopen if you have additional context!
```

<details>
<summary>Raw JSON</summary>

```json
{
  "meta": {
    "schemaVersion": "1.0",
    "number": 3646,
    "repo": "mono/SkiaSharp",
    "analyzedAt": "2026-07-13T05:20:00Z"
  },
  "summary": "Reporter claims the virtual modifier on SKNativeObject.Handle is meaningless and impacts performance, but code investigation shows that SKObject and SKPath both override this property, making the virtual modifier necessary.",
  "classification": {
    "type": {
      "value": "type/question",
      "confidence": 0.85
    },
    "area": {
      "value": "area/SkiaSharp",
      "confidence": 0.95
    }
  },
  "evidence": {
    "reproEvidence": {
      "environmentDetails": "~5 million get_Handle calls taking ~400ms on reporter's machine"
    }
  },
  "analysis": {
    "summary": "The reporter asserts that no derived class overrides SKNativeObject.Handle, making the virtual modifier superfluous and potentially hurting performance. However, code investigation shows SKObject explicitly overrides Handle (binding/SkiaSharp/SKObject.cs:47) to add handle registration/deregistration logic, and SKPath further overrides it (binding/SkiaSharp/SKPath.cs:35) to flush its internal builder before returning the handle. The virtual dispatch chain is intentional and required by the design. The performance concern is understandable but the virtual modifier cannot be removed without breaking the design.",
    "codeInvestigation": [
      {
        "file": "binding/SkiaSharp/SKObject.cs",
        "lines": "219,253",
        "finding": "SKNativeObject declares Handle as virtual at line 253. SKObject overrides it at line 47 to add handle registration (RegisterHandle) and deregistration (DeregisterHandle) on assignment.",
        "relevance": "direct"
      },
      {
        "file": "binding/SkiaSharp/SKPath.cs",
        "lines": "35-40",
        "finding": "SKPath overrides Handle to call FlushBuilder() before returning the base handle, ensuring pending path mutations are applied before the handle is passed to native code.",
        "relevance": "direct"
      }
    ],
    "keySignals": [
      {
        "text": "no derived class overrides SKNativeObject.Handle property (except SKObject, of course)",
        "source": "issue body",
        "interpretation": "Reporter's assertion is factually incorrect — SKPath also overrides Handle with different logic."
      },
      {
        "text": "~400ms for ~5kk get_Handle calls",
        "source": "issue body",
        "interpretation": "Performance micro-benchmark; virtual dispatch in .NET is typically devirtualized by JIT for concrete types in hot paths."
      }
    ],
    "rationale": "The reporter's core premise is incorrect: SKObject and SKPath both override SKNativeObject.Handle with meaningful behavior (handle registration and builder flushing respectively). The virtual modifier is intentional and necessary. This is a misunderstanding of the codebase, not a bug or actionable enhancement. Closing as not-a-bug is appropriate with an explanation.",
    "resolution": {
      "proposals": [
        {
          "title": "Close as not-a-bug with explanation",
          "description": "Close the issue explaining that SKObject and SKPath both override Handle with meaningful behavior, making the virtual modifier necessary.",
          "category": "investigation",
          "effort": "cost/xs",
          "validated": "untested"
        }
      ],
      "recommendedProposal": "close-1",
      "recommendedReason": "The virtual modifier is required because SKObject and SKPath both override Handle. Removing it would break the design."
    }
  },
  "output": {
    "actionability": {
      "suggestedAction": "close-as-not-a-bug",
      "confidence": 0.9,
      "reason": "The virtual modifier on SKNativeObject.Handle is necessary: SKObject overrides it to manage handle registration, and SKPath overrides it to flush its builder. The reporter's assumption that no class overrides it (other than SKObject) is incorrect.",
      "suggestedReproPlatform": "linux"
    },
    "actions": [
      {
        "type": "update-labels",
        "description": "Apply classification labels",
        "risk": "low",
        "confidence": 0.9,
        "labels": [
          "type/question",
          "area/SkiaSharp"
        ]
      },
      {
        "type": "add-comment",
        "description": "Explain that the virtual modifier is necessary because SKObject and SKPath both override Handle.",
        "risk": "high",
        "confidence": 0.9,
        "comment": "Thanks for raising this! After looking at the code, the `virtual` modifier on `SKNativeObject.Handle` is actually necessary — it's not meaningless.\n\nTwo classes override it with meaningful behavior:\n\n1. **`SKObject.Handle`** (`SKObject.cs` line 47) — overrides the setter to call `RegisterHandle` and `DeregisterHandle`, which are critical for the handle-tracking dictionary that prevents double-disposal and enables handle reuse.\n2. **`SKPath.Handle`** (`SKPath.cs` line 35) — overrides the getter to call `FlushBuilder()` before returning the handle, ensuring any pending path mutations are applied before the native handle is passed to Skia.\n\nRemoving the `virtual` modifier would break both of these overrides.\n\nRegarding the performance concern: modern .NET JIT is very good at devirtualizing property getters, especially for concrete (non-abstract) call sites. The ~400ms for 5M calls benchmark may be affected by other factors (e.g., the measurement infrastructure itself). If you have a specific hot path in mind, feel free to share more details and we can look at targeted optimizations.\n\nClosing this as not a bug. Please reopen if you have additional context!"
      },
      {
        "type": "close-issue",
        "description": "Close as not a bug after commenting",
        "risk": "medium",
        "confidence": 0.9,
        "stateReason": "not_planned"
      }
    ]
  }
}
```

</details>
