# Issue Triage Report — #4371

| Field | Value |
|-------|-------|
| Repository | mono/SkiaSharp |
| Analyzed | 2026-08-05T05:13:02Z |
| Type | type/bug (0.99 (99%)) |
| Area | area/SkiaSharp (0.99 (99%)) |
| Suggested action | ready-to-fix (0.95 (95%)) |

**Issue Summary:** Native use-after-free in SKNWayCanvas.AddCanvas and SKOverdrawCanvas constructor: GC.KeepAlive at call-site does not root borrowed SKCanvas* for the wrapper's lifetime, leading to AccessViolationException when the added/wrapped canvas is collected while the wrapper is still alive.

**Analysis:** SKNWayCanvas.AddCanvas and SKOverdrawCanvas constructor both use GC.KeepAlive(canvas) at call-site only, which does not root the borrowed SKCanvas* for the full lifetime of the wrapper object. Native SkNWayCanvas/SkOverdrawCanvas store raw non-owning SkCanvas* pointers; if the managed SKCanvas is GC'd after AddCanvas/constructor returns, the finalizer calls sk_canvas_destroy, leaving the native wrapper with a dangling pointer that causes AccessViolationException on the next draw. The fix is to root the canvas in a private instance field (List<SKCanvas> for NWayCanvas, single field for OverdrawCanvas), matching the existing SKRegion/SKPath iterator pattern already in the codebase.

**Recommendations:** **ready-to-fix** — Root cause is clear and fully proven. Fix pattern is established in codebase. Scope is small (two files, private field additions). A PR was reportedly created by the memory-leak-fixer workflow but is not yet linked/visible.

---

## Classification

| Field | Value |
|-------|-------|
| Type | type/bug |
| Area | area/SkiaSharp |
| Platforms | — |
| Backends | — |
| Tenets | tenet/reliability, tenet/performance |
| Perf | perf/memory-leak |
| Partner | — |

## Evidence

### Reproduction

1. Create an SKNWayCanvas and add a SKCanvas via AddCanvas
2. Allow the added SKCanvas to go out of scope with no other strong references
3. Force GC.Collect()
4. Draw through the SKNWayCanvas
5. Observe AccessViolationException / use-after-free crash

**Environment:** linux-x64, empirically validated by memory-leak-fixer workflow run 28909417142

**Repository links:**
- https://github.com/mono/SkiaSharp/actions/runs/28909417142/agentic_workflow — Memory Leak Fixer workflow run that produced this issue

### Bug Signals

| Field | Value |
|-------|-------|
| Severity | high |
| Regression claimed | False |
| Error type | crash |
| Error message | AccessViolationException when drawing through SKNWayCanvas or SKOverdrawCanvas after wrapped SKCanvas is GC'd |
| Repro quality | complete |
| Target frameworks | net10.0 |

### Version Analysis

| Field | Value |
|-------|-------|
| Mentioned versions | — |
| Worked in | — |
| Broke in | — |
| Current relevance | likely |
| Relevance reason | Source code inspection confirms GC.KeepAlive is only call-site scoped; no rooting fields exist in the current codebase. |

## Analysis

### Technical Summary

SKNWayCanvas.AddCanvas and SKOverdrawCanvas constructor both use GC.KeepAlive(canvas) at call-site only, which does not root the borrowed SKCanvas* for the full lifetime of the wrapper object. Native SkNWayCanvas/SkOverdrawCanvas store raw non-owning SkCanvas* pointers; if the managed SKCanvas is GC'd after AddCanvas/constructor returns, the finalizer calls sk_canvas_destroy, leaving the native wrapper with a dangling pointer that causes AccessViolationException on the next draw. The fix is to root the canvas in a private instance field (List<SKCanvas> for NWayCanvas, single field for OverdrawCanvas), matching the existing SKRegion/SKPath iterator pattern already in the codebase.

### Rationale

This is a high-confidence type/bug in area/SkiaSharp core. The use-after-free was empirically proven with red→green regression tests. The source code confirms no rooting fields exist. The fix pattern is established in the codebase (iterator classes). The issue was filed with complete repro evidence by the automated memory-leak-fixer workflow. Platform is not restricted — the bug affects all platforms. Severity is high because the crash (AccessViolationException) can corrupt the process with no workaround short of manually keeping references, which callers cannot reasonably be expected to do.

### Key Signals

- "Both only issued a call-site GC.KeepAlive(canvas), which keeps the canvas alive for the duration of that one method call but does not root it for the wrapper's lifetime." — **issue body** (Confirms the root cause: GC.KeepAlive does not extend lifetime past the method call boundary.)
- "NWayCanvasKeepsAddedCanvasesAlive — FAIL, OverdrawCanvasKeepsWrappedCanvasAlive — FAIL (without fix)" — **issue body** (Empirical red/green proof confirms the leak is real and deterministically reproducible.)
- "The SKRegion / SKPath iterators all root their parent object precisely because native holds a borrowed pointer. These two canvas wrappers were missed." — **issue body** (Established pattern exists in codebase; omission of rooting in these two classes is a missed case.)
- "Fixed by the linked pull request." — **issue body** (The memory-leak-fixer workflow reports a PR exists, but no PR is currently linked or visible in the repo.)

### Code Investigation

| File | Lines | Relevance | Finding |
|------|-------|-----------|---------|
| `binding/SkiaSharp/SKNWayCanvas.cs` | 20-28 | direct | AddCanvas calls sk_nway_canvas_add_canvas(Handle, canvas.Handle) then only GC.KeepAlive(canvas) at call-site. No private list roots the added canvas for the wrapper's lifetime. RemoveCanvas and RemoveAll also lack any managed tracking. |
| `binding/SkiaSharp/SKOverdrawCanvas.cs` | 14-22 | direct | Constructor calls sk_overdraw_canvas_new(canvas.Handle) then only GC.KeepAlive(canvas). No private field roots the wrapped canvas for the object's lifetime. |

### Resolution Proposals

**Hypothesis:** Root the borrowed SKCanvas* references in private managed instance fields so the GC cannot collect them while the wrapper is alive.

1. **Add private rooting fields to SKNWayCanvas and SKOverdrawCanvas** — fix, confidence 0.97 (97%), cost/xs, validated=yes
   - In SKNWayCanvas, add a private List<SKCanvas> field and keep it in sync in AddCanvas (add), RemoveCanvas (remove), and RemoveAll (clear). In SKOverdrawCanvas, add a private readonly SKCanvas field set in the constructor. Remove the now-redundant call-site GC.KeepAlive calls.

**Recommended proposal:** Add private rooting fields to SKNWayCanvas and SKOverdrawCanvas

**Why:** ABI-safe (private fields only), matches established iterator pattern already used in the codebase, empirically proven to resolve the crash.

## Recommendations

### Actionability

| Field | Value |
|-------|-------|
| Suggested action | ready-to-fix |
| Confidence | 0.95 (95%) |
| Reason | Root cause is clear and fully proven. Fix pattern is established in codebase. Scope is small (two files, private field additions). A PR was reportedly created by the memory-leak-fixer workflow but is not yet linked/visible. |
| Suggested repro platform | linux |

### Automatable Actions

| Type | Risk | Confidence | Description | Details |
|------|------|------------|-------------|---------|
| update-labels | low | 0.99 (99%) | Apply bug, core area, reliability and performance memory-leak labels | labels=type/bug, area/SkiaSharp, tenet/reliability, tenet/performance, perf/memory-leak |
| add-comment | medium | 0.92 (92%) | Acknowledge the AI-generated finding and confirm root cause with source evidence | — |

**Comment draft for `add-comment`:**

```markdown
✅ **Triage confirmed.** Source code inspection confirms the root cause: both `SKNWayCanvas.AddCanvas` and the `SKOverdrawCanvas` constructor use `GC.KeepAlive(canvas)` at call-site only, which does not root the borrowed native pointer for the wrapper's lifetime.

The fix — adding private rooting fields matching the `SKRegion`/`SKPath` iterator pattern — is well-understood, ABI-safe, and small in scope. Marking as **ready-to-fix**.

Next step: link or open a PR with the rooting-field changes and the two regression tests (`NWayCanvasKeepsAddedCanvasesAlive`, `OverdrawCanvasKeepsWrappedCanvasAlive`).
```

<details>
<summary>Raw JSON</summary>

```json
{
  "meta": {
    "schemaVersion": "1.0",
    "number": 4371,
    "repo": "mono/SkiaSharp",
    "analyzedAt": "2026-08-05T05:13:02Z"
  },
  "summary": "Native use-after-free in SKNWayCanvas.AddCanvas and SKOverdrawCanvas constructor: GC.KeepAlive at call-site does not root borrowed SKCanvas* for the wrapper's lifetime, leading to AccessViolationException when the added/wrapped canvas is collected while the wrapper is still alive.",
  "classification": {
    "type": {
      "value": "type/bug",
      "confidence": 0.99
    },
    "area": {
      "value": "area/SkiaSharp",
      "confidence": 0.99
    },
    "tenets": [
      "tenet/reliability",
      "tenet/performance"
    ],
    "perf": [
      "perf/memory-leak"
    ]
  },
  "evidence": {
    "bugSignals": {
      "severity": "high",
      "regressionClaimed": false,
      "errorType": "crash",
      "errorMessage": "AccessViolationException when drawing through SKNWayCanvas or SKOverdrawCanvas after wrapped SKCanvas is GC'd",
      "reproQuality": "complete",
      "targetFrameworks": [
        "net10.0"
      ]
    },
    "reproEvidence": {
      "stepsToReproduce": [
        "Create an SKNWayCanvas and add a SKCanvas via AddCanvas",
        "Allow the added SKCanvas to go out of scope with no other strong references",
        "Force GC.Collect()",
        "Draw through the SKNWayCanvas",
        "Observe AccessViolationException / use-after-free crash"
      ],
      "environmentDetails": "linux-x64, empirically validated by memory-leak-fixer workflow run 28909417142",
      "repoLinks": [
        {
          "url": "https://github.com/mono/SkiaSharp/actions/runs/28909417142/agentic_workflow",
          "description": "Memory Leak Fixer workflow run that produced this issue"
        }
      ]
    },
    "versionAnalysis": {
      "mentionedVersions": [],
      "currentRelevance": "likely",
      "relevanceReason": "Source code inspection confirms GC.KeepAlive is only call-site scoped; no rooting fields exist in the current codebase."
    }
  },
  "analysis": {
    "summary": "SKNWayCanvas.AddCanvas and SKOverdrawCanvas constructor both use GC.KeepAlive(canvas) at call-site only, which does not root the borrowed SKCanvas* for the full lifetime of the wrapper object. Native SkNWayCanvas/SkOverdrawCanvas store raw non-owning SkCanvas* pointers; if the managed SKCanvas is GC'd after AddCanvas/constructor returns, the finalizer calls sk_canvas_destroy, leaving the native wrapper with a dangling pointer that causes AccessViolationException on the next draw. The fix is to root the canvas in a private instance field (List<SKCanvas> for NWayCanvas, single field for OverdrawCanvas), matching the existing SKRegion/SKPath iterator pattern already in the codebase.",
    "codeInvestigation": [
      {
        "file": "binding/SkiaSharp/SKNWayCanvas.cs",
        "lines": "20-28",
        "finding": "AddCanvas calls sk_nway_canvas_add_canvas(Handle, canvas.Handle) then only GC.KeepAlive(canvas) at call-site. No private list roots the added canvas for the wrapper's lifetime. RemoveCanvas and RemoveAll also lack any managed tracking.",
        "relevance": "direct"
      },
      {
        "file": "binding/SkiaSharp/SKOverdrawCanvas.cs",
        "lines": "14-22",
        "finding": "Constructor calls sk_overdraw_canvas_new(canvas.Handle) then only GC.KeepAlive(canvas). No private field roots the wrapped canvas for the object's lifetime.",
        "relevance": "direct"
      }
    ],
    "keySignals": [
      {
        "text": "Both only issued a call-site GC.KeepAlive(canvas), which keeps the canvas alive for the duration of that one method call but does not root it for the wrapper's lifetime.",
        "source": "issue body",
        "interpretation": "Confirms the root cause: GC.KeepAlive does not extend lifetime past the method call boundary."
      },
      {
        "text": "NWayCanvasKeepsAddedCanvasesAlive — FAIL, OverdrawCanvasKeepsWrappedCanvasAlive — FAIL (without fix)",
        "source": "issue body",
        "interpretation": "Empirical red/green proof confirms the leak is real and deterministically reproducible."
      },
      {
        "text": "The SKRegion / SKPath iterators all root their parent object precisely because native holds a borrowed pointer. These two canvas wrappers were missed.",
        "source": "issue body",
        "interpretation": "Established pattern exists in codebase; omission of rooting in these two classes is a missed case."
      },
      {
        "text": "Fixed by the linked pull request.",
        "source": "issue body",
        "interpretation": "The memory-leak-fixer workflow reports a PR exists, but no PR is currently linked or visible in the repo."
      }
    ],
    "rationale": "This is a high-confidence type/bug in area/SkiaSharp core. The use-after-free was empirically proven with red→green regression tests. The source code confirms no rooting fields exist. The fix pattern is established in the codebase (iterator classes). The issue was filed with complete repro evidence by the automated memory-leak-fixer workflow. Platform is not restricted — the bug affects all platforms. Severity is high because the crash (AccessViolationException) can corrupt the process with no workaround short of manually keeping references, which callers cannot reasonably be expected to do.",
    "resolution": {
      "hypothesis": "Root the borrowed SKCanvas* references in private managed instance fields so the GC cannot collect them while the wrapper is alive.",
      "proposals": [
        {
          "title": "Add private rooting fields to SKNWayCanvas and SKOverdrawCanvas",
          "description": "In SKNWayCanvas, add a private List<SKCanvas> field and keep it in sync in AddCanvas (add), RemoveCanvas (remove), and RemoveAll (clear). In SKOverdrawCanvas, add a private readonly SKCanvas field set in the constructor. Remove the now-redundant call-site GC.KeepAlive calls.",
          "category": "fix",
          "confidence": 0.97,
          "effort": "cost/xs",
          "validated": "yes"
        }
      ],
      "recommendedProposal": "Add private rooting fields to SKNWayCanvas and SKOverdrawCanvas",
      "recommendedReason": "ABI-safe (private fields only), matches established iterator pattern already used in the codebase, empirically proven to resolve the crash."
    }
  },
  "output": {
    "actionability": {
      "suggestedAction": "ready-to-fix",
      "confidence": 0.95,
      "reason": "Root cause is clear and fully proven. Fix pattern is established in codebase. Scope is small (two files, private field additions). A PR was reportedly created by the memory-leak-fixer workflow but is not yet linked/visible.",
      "suggestedReproPlatform": "linux"
    },
    "actions": [
      {
        "type": "update-labels",
        "description": "Apply bug, core area, reliability and performance memory-leak labels",
        "risk": "low",
        "confidence": 0.99,
        "labels": [
          "type/bug",
          "area/SkiaSharp",
          "tenet/reliability",
          "tenet/performance",
          "perf/memory-leak"
        ]
      },
      {
        "type": "add-comment",
        "description": "Acknowledge the AI-generated finding and confirm root cause with source evidence",
        "risk": "medium",
        "confidence": 0.92,
        "comment": "✅ **Triage confirmed.** Source code inspection confirms the root cause: both `SKNWayCanvas.AddCanvas` and the `SKOverdrawCanvas` constructor use `GC.KeepAlive(canvas)` at call-site only, which does not root the borrowed native pointer for the wrapper's lifetime.\n\nThe fix — adding private rooting fields matching the `SKRegion`/`SKPath` iterator pattern — is well-understood, ABI-safe, and small in scope. Marking as **ready-to-fix**.\n\nNext step: link or open a PR with the rooting-field changes and the two regression tests (`NWayCanvasKeepsAddedCanvasesAlive`, `OverdrawCanvasKeepsWrappedCanvasAlive`)."
      }
    ]
  }
}
```

</details>
