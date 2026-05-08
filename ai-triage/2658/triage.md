# Issue Triage Report — #2658

| Field | Value |
|-------|-------|
| Repository | mono/SkiaSharp |
| Analyzed | 2026-05-04T20:47:00Z |
| Type | type/bug (0.98 (98%)) |
| Area | area/HarfBuzzSharp (0.97 (97%)) |
| Suggested action | ready-to-fix (0.95 (95%)) |

**Issue Summary:** HarfBuzzSharp.NativeObject.Dispose(bool) returns early when called by the finalizer (disposing=false), skipping DisposeHandler() and leaking native HarfBuzz memory.

**Analysis:** HarfBuzzSharp.NativeObject.Dispose(bool) contains a guard `if (!disposing) return;` at line 42 that prevents DisposeHandler() from being called when the finalizer triggers disposal. This means native HarfBuzz memory (managed by DisposeHandler in subclasses) is never released unless the object is explicitly disposed. SkiaSharp.SKNativeObject (SKObject.cs line 269-271) correctly calls DisposeNative() unconditionally. The fix is to remove the early return so DisposeHandler() is always called, with managed cleanup (Handle=IntPtr.Zero) still guarded by disposing.

**Recommendations:** **ready-to-fix** — Root cause is confirmed by code inspection. The fix is a one-liner: remove the early return so DisposeHandler() is called regardless of the disposing flag, matching the pattern used by SKNativeObject.

---

## Classification

| Field | Value |
|-------|-------|
| Type | type/bug |
| Area | area/HarfBuzzSharp |
| Platforms | — |
| Backends | — |
| Tenets | tenet/reliability |
| Partner | — |
| Current labels | type/bug |

## Evidence

### Reproduction

1. Create a HarfBuzzSharp.NativeObject-derived instance (e.g., Font, Face, Buffer)
2. Do not explicitly call Dispose() on it
3. Allow GC to finalize the instance
4. Observe that native HarfBuzz resources are never freed

**Environment:** SkiaSharp 2.88.3, Visual Studio (Windows), all platforms

**Repository links:**
- https://github.com/mono/SkiaSharp/blob/main/binding/HarfBuzzSharp/NativeObject.cs#L34 — HarfBuzzSharp.NativeObject.Dispose(bool) implementation showing premature return
- https://learn.microsoft.com/en-us/dotnet/standard/garbage-collection/implementing-dispose#the-disposebool-method-overload — .NET Dispose pattern documentation stating unmanaged resources must be freed regardless of disposing flag

### Bug Signals

| Field | Value |
|-------|-------|
| Severity | medium |
| Regression claimed | False |
| Error type | memory-leak |
| Error message | — |
| Repro quality | complete |
| Target frameworks | net-standard |

### Version Analysis

| Field | Value |
|-------|-------|
| Mentioned versions | 2.88.3 |
| Worked in | — |
| Broke in | — |
| Current relevance | likely |
| Relevance reason | Code inspection of current main confirms the bug still exists: NativeObject.Dispose(bool) returns early at line 42-44 when disposing=false. |

## Analysis

### Technical Summary

HarfBuzzSharp.NativeObject.Dispose(bool) contains a guard `if (!disposing) return;` at line 42 that prevents DisposeHandler() from being called when the finalizer triggers disposal. This means native HarfBuzz memory (managed by DisposeHandler in subclasses) is never released unless the object is explicitly disposed. SkiaSharp.SKNativeObject (SKObject.cs line 269-271) correctly calls DisposeNative() unconditionally. The fix is to remove the early return so DisposeHandler() is always called, with managed cleanup (Handle=IntPtr.Zero) still guarded by disposing.

### Rationale

The bug is confirmed by code inspection. The NativeObject.Dispose(bool) implementation deviates from the .NET Dispose pattern by not freeing unmanaged (native) resources when called via the finalizer. The SkiaSharp SKObject counterpart handles this correctly. The fix is small and well-understood: remove the early return, always call DisposeHandler(), and keep Handle nulling guarded by `disposing`.

### Key Signals

- "if (!disposing) { return; }" — **binding/HarfBuzzSharp/NativeObject.cs:42-44** (This guard prevents DisposeHandler() from being called during finalization, causing native memory leak.)
- "if (Handle != IntPtr.Zero && OwnsHandle) DisposeNative();" — **binding/SkiaSharp/SKObject.cs:269-271** (SKNativeObject correctly frees native resources regardless of the disposing flag.)

### Code Investigation

| File | Lines | Relevance | Finding |
|------|-------|-----------|---------|
| `binding/HarfBuzzSharp/NativeObject.cs` | 34-51 | direct | Dispose(bool disposing) returns early at line 42 (`if (!disposing) return;`), skipping DisposeHandler() call and the Handle zeroing. When the finalizer invokes Dispose(false), native resources are never freed. |
| `binding/SkiaSharp/SKObject.cs` | 260-276 | related | SKNativeObject.Dispose(bool) calls DisposeNative() at line 269-271 unconditionally (only guarded by Handle!=IntPtr.Zero and OwnsHandle). Managed-only cleanup (DisposeUnownedManaged, DisposeManaged) is guarded by disposing. This is the correct pattern. |

### Resolution Proposals

**Hypothesis:** Remove the `if (!disposing) return;` early exit from NativeObject.Dispose(bool), so DisposeHandler() is always invoked. The Handle zeroing should remain guarded by `disposing` to avoid accessing managed fields from a finalizer thread.

1. **Remove early return in NativeObject.Dispose(bool) and call DisposeHandler() unconditionally** — fix, cost/xs, validated=yes
   - Change the Dispose(bool) implementation to always call DisposeHandler(). Keep the Handle=IntPtr.Zero assignment guarded by `if (disposing)` since setting a field from a finalizer thread is a potential issue if subclasses read Handle.

```csharp
protected virtual void Dispose(bool disposing)
{
    if (isDisposed) {
        return;
    }

    isDisposed = true;

    DisposeHandler();

    if (disposing && zero) {
        Handle = IntPtr.Zero;
    }
}
```

**Recommended proposal:** fix-1

**Why:** Small, surgical change that aligns NativeObject with the correct .NET Dispose pattern already used by SKNativeObject. Low risk.

## Recommendations

### Actionability

| Field | Value |
|-------|-------|
| Suggested action | ready-to-fix |
| Confidence | 0.95 (95%) |
| Reason | Root cause is confirmed by code inspection. The fix is a one-liner: remove the early return so DisposeHandler() is called regardless of the disposing flag, matching the pattern used by SKNativeObject. |
| Suggested repro platform | linux |

### Automatable Actions

| Type | Risk | Confidence | Description | Details |
|------|------|------------|-------------|---------|
| update-labels | low | 0.97 (97%) | Add area/HarfBuzzSharp and tenet/reliability labels | labels=area/HarfBuzzSharp, tenet/reliability |
| add-comment | medium | 0.92 (92%) | Acknowledge the confirmed bug and outline the fix path | — |

**Comment draft for `add-comment`:**

```markdown
Thanks for the detailed report and the pointer to the reference docs.

Code inspection confirms this bug. In `HarfBuzzSharp.NativeObject.Dispose(bool)` (binding/HarfBuzzSharp/NativeObject.cs), the guard:

```csharp
if (!disposing) {
    return;
}
```

prevents `DisposeHandler()` from being called when the GC finalizer runs (`Dispose(false)`). Native HarfBuzz resources are therefore never freed unless the caller explicitly calls `Dispose()` — a memory leak.

The sibling class `SkiaSharp.SKNativeObject` handles this correctly by calling `DisposeNative()` unconditionally.

**Workaround (until fixed):** Always call `Dispose()` (or use `using`) on any `HarfBuzzSharp.NativeObject`-derived instance (`Font`, `Face`, `Buffer`, etc.) to ensure native resources are released deterministically.

The fix is straightforward — we'll remove the early return so `DisposeHandler()` is always invoked.
```

<details>
<summary>Raw JSON</summary>

```json
{
  "meta": {
    "schemaVersion": "1.0",
    "number": 2658,
    "repo": "mono/SkiaSharp",
    "analyzedAt": "2026-05-04T20:47:00Z",
    "currentLabels": [
      "type/bug"
    ]
  },
  "summary": "HarfBuzzSharp.NativeObject.Dispose(bool) returns early when called by the finalizer (disposing=false), skipping DisposeHandler() and leaking native HarfBuzz memory.",
  "classification": {
    "type": {
      "value": "type/bug",
      "confidence": 0.98
    },
    "area": {
      "value": "area/HarfBuzzSharp",
      "confidence": 0.97
    },
    "tenets": [
      "tenet/reliability"
    ]
  },
  "evidence": {
    "bugSignals": {
      "severity": "medium",
      "regressionClaimed": false,
      "errorType": "memory-leak",
      "reproQuality": "complete",
      "targetFrameworks": [
        "net-standard"
      ]
    },
    "reproEvidence": {
      "stepsToReproduce": [
        "Create a HarfBuzzSharp.NativeObject-derived instance (e.g., Font, Face, Buffer)",
        "Do not explicitly call Dispose() on it",
        "Allow GC to finalize the instance",
        "Observe that native HarfBuzz resources are never freed"
      ],
      "environmentDetails": "SkiaSharp 2.88.3, Visual Studio (Windows), all platforms",
      "repoLinks": [
        {
          "url": "https://github.com/mono/SkiaSharp/blob/main/binding/HarfBuzzSharp/NativeObject.cs#L34",
          "description": "HarfBuzzSharp.NativeObject.Dispose(bool) implementation showing premature return"
        },
        {
          "url": "https://learn.microsoft.com/en-us/dotnet/standard/garbage-collection/implementing-dispose#the-disposebool-method-overload",
          "description": ".NET Dispose pattern documentation stating unmanaged resources must be freed regardless of disposing flag"
        }
      ]
    },
    "versionAnalysis": {
      "mentionedVersions": [
        "2.88.3"
      ],
      "currentRelevance": "likely",
      "relevanceReason": "Code inspection of current main confirms the bug still exists: NativeObject.Dispose(bool) returns early at line 42-44 when disposing=false."
    }
  },
  "analysis": {
    "summary": "HarfBuzzSharp.NativeObject.Dispose(bool) contains a guard `if (!disposing) return;` at line 42 that prevents DisposeHandler() from being called when the finalizer triggers disposal. This means native HarfBuzz memory (managed by DisposeHandler in subclasses) is never released unless the object is explicitly disposed. SkiaSharp.SKNativeObject (SKObject.cs line 269-271) correctly calls DisposeNative() unconditionally. The fix is to remove the early return so DisposeHandler() is always called, with managed cleanup (Handle=IntPtr.Zero) still guarded by disposing.",
    "codeInvestigation": [
      {
        "file": "binding/HarfBuzzSharp/NativeObject.cs",
        "lines": "34-51",
        "finding": "Dispose(bool disposing) returns early at line 42 (`if (!disposing) return;`), skipping DisposeHandler() call and the Handle zeroing. When the finalizer invokes Dispose(false), native resources are never freed.",
        "relevance": "direct"
      },
      {
        "file": "binding/SkiaSharp/SKObject.cs",
        "lines": "260-276",
        "finding": "SKNativeObject.Dispose(bool) calls DisposeNative() at line 269-271 unconditionally (only guarded by Handle!=IntPtr.Zero and OwnsHandle). Managed-only cleanup (DisposeUnownedManaged, DisposeManaged) is guarded by disposing. This is the correct pattern.",
        "relevance": "related"
      }
    ],
    "keySignals": [
      {
        "text": "if (!disposing) { return; }",
        "source": "binding/HarfBuzzSharp/NativeObject.cs:42-44",
        "interpretation": "This guard prevents DisposeHandler() from being called during finalization, causing native memory leak."
      },
      {
        "text": "if (Handle != IntPtr.Zero && OwnsHandle) DisposeNative();",
        "source": "binding/SkiaSharp/SKObject.cs:269-271",
        "interpretation": "SKNativeObject correctly frees native resources regardless of the disposing flag."
      }
    ],
    "rationale": "The bug is confirmed by code inspection. The NativeObject.Dispose(bool) implementation deviates from the .NET Dispose pattern by not freeing unmanaged (native) resources when called via the finalizer. The SkiaSharp SKObject counterpart handles this correctly. The fix is small and well-understood: remove the early return, always call DisposeHandler(), and keep Handle nulling guarded by `disposing`.",
    "resolution": {
      "hypothesis": "Remove the `if (!disposing) return;` early exit from NativeObject.Dispose(bool), so DisposeHandler() is always invoked. The Handle zeroing should remain guarded by `disposing` to avoid accessing managed fields from a finalizer thread.",
      "proposals": [
        {
          "title": "Remove early return in NativeObject.Dispose(bool) and call DisposeHandler() unconditionally",
          "category": "fix",
          "effort": "cost/xs",
          "validated": "yes",
          "description": "Change the Dispose(bool) implementation to always call DisposeHandler(). Keep the Handle=IntPtr.Zero assignment guarded by `if (disposing)` since setting a field from a finalizer thread is a potential issue if subclasses read Handle.",
          "codeSnippet": "protected virtual void Dispose(bool disposing)\n{\n    if (isDisposed) {\n        return;\n    }\n\n    isDisposed = true;\n\n    DisposeHandler();\n\n    if (disposing && zero) {\n        Handle = IntPtr.Zero;\n    }\n}"
        }
      ],
      "recommendedProposal": "fix-1",
      "recommendedReason": "Small, surgical change that aligns NativeObject with the correct .NET Dispose pattern already used by SKNativeObject. Low risk."
    }
  },
  "output": {
    "actionability": {
      "suggestedAction": "ready-to-fix",
      "confidence": 0.95,
      "reason": "Root cause is confirmed by code inspection. The fix is a one-liner: remove the early return so DisposeHandler() is called regardless of the disposing flag, matching the pattern used by SKNativeObject.",
      "suggestedReproPlatform": "linux"
    },
    "actions": [
      {
        "type": "update-labels",
        "description": "Add area/HarfBuzzSharp and tenet/reliability labels",
        "risk": "low",
        "confidence": 0.97,
        "labels": [
          "area/HarfBuzzSharp",
          "tenet/reliability"
        ]
      },
      {
        "type": "add-comment",
        "description": "Acknowledge the confirmed bug and outline the fix path",
        "risk": "medium",
        "confidence": 0.92,
        "comment": "Thanks for the detailed report and the pointer to the reference docs.\n\nCode inspection confirms this bug. In `HarfBuzzSharp.NativeObject.Dispose(bool)` (binding/HarfBuzzSharp/NativeObject.cs), the guard:\n\n```csharp\nif (!disposing) {\n    return;\n}\n```\n\nprevents `DisposeHandler()` from being called when the GC finalizer runs (`Dispose(false)`). Native HarfBuzz resources are therefore never freed unless the caller explicitly calls `Dispose()` — a memory leak.\n\nThe sibling class `SkiaSharp.SKNativeObject` handles this correctly by calling `DisposeNative()` unconditionally.\n\n**Workaround (until fixed):** Always call `Dispose()` (or use `using`) on any `HarfBuzzSharp.NativeObject`-derived instance (`Font`, `Face`, `Buffer`, etc.) to ensure native resources are released deterministically.\n\nThe fix is straightforward — we'll remove the early return so `DisposeHandler()` is always invoked."
      }
    ]
  }
}
```

</details>
