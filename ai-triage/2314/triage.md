# Issue Triage Report — #2314

| Field | Value |
|-------|-------|
| Repository | mono/SkiaSharp |
| Analyzed | 2026-04-23T09:02:27Z |
| Type | type/enhancement (0.95 (95%)) |
| Area | area/SkiaSharp (0.95 (95%)) |
| Suggested action | close-as-fixed (0.95 (95%)) |

**Issue Summary:** Feature request to replace `ref` parameters with `in` parameters for SKMatrix and similar structs in methods like SKCanvas.Concat — the requested change has already been implemented.

**Analysis:** The reporter correctly identified that using `ref` for read-only struct parameters prevents callers from passing `readonly` fields (CS0192). The fix — switching to `in` keyword — has already been applied: SKCanvas.Concat(in SKMatrix m), SKCanvas.SetMatrix(in SKMatrix matrix), SKPath.Transform(in SKMatrix matrix), and others already use `in`. The old `ref`/value overloads are marked obsolete. The remaining `ref` parameters (SKMatrix.Concat target, SKPaint.MeasureText bounds, SKCodec.GetValidSubset) are intentional mutation/output parameters.

**Recommendations:** **close-as-fixed** — The requested change (using `in` instead of `ref` for SKMatrix parameters) is fully implemented in the current codebase. SKCanvas.Concat, SKCanvas.SetMatrix, SKPath.Transform, and other key methods all use `in SKMatrix`.

---

## Classification

| Field | Value |
|-------|-------|
| Type | type/enhancement |
| Area | area/SkiaSharp |
| Platforms | — |
| Backends | — |
| Tenets | tenet/performance |
| Partner | — |

## Evidence

### Reproduction

**Environment:** SkiaSharp (version unspecified, filed 2022-11-17)

**Code snippets:**

```csharp
canvas.Concat(ref obj.Matrix); // CS0192: A readonly field cannot be used as a ref or out value
```

### Fix Status

| Field | Value |
|-------|-------|
| Likely fixed | True |
| Confidence | 0.97 (97%) |
| Reason | SKCanvas.Concat and SetMatrix now use `in SKMatrix` in the current codebase. Old `ref`-based overloads are marked [Obsolete(error:true)]. SKPath.Transform also migrated to `in SKMatrix`. The primary use case from the issue report is resolved. |
| Related PRs | — |
| Related commits | — |
| Fixed in version | — |

## Analysis

### Technical Summary

The reporter correctly identified that using `ref` for read-only struct parameters prevents callers from passing `readonly` fields (CS0192). The fix — switching to `in` keyword — has already been applied: SKCanvas.Concat(in SKMatrix m), SKCanvas.SetMatrix(in SKMatrix matrix), SKPath.Transform(in SKMatrix matrix), and others already use `in`. The old `ref`/value overloads are marked obsolete. The remaining `ref` parameters (SKMatrix.Concat target, SKPaint.MeasureText bounds, SKCodec.GetValidSubset) are intentional mutation/output parameters.

### Rationale

The enhancement was valid and performance-relevant. Code investigation confirms the primary requested change (SKCanvas.Concat) is already implemented. Remaining `ref` parameters in public APIs serve different purposes (output/mutation) and are not covered by the feature request.

### Key Signals

- "public void Concat (ref SKMatrix m)" — **issue body** (This was the problematic signature at the time of filing. It has since been changed to `in SKMatrix m`.)
- "SKCanvas.Concat(in SKMatrix m) =>" — **binding/SkiaSharp/SKCanvas.cs line 235** (Current code already uses `in` — the fix was already applied.)
- "[Obsolete("Use SetMatrix(in SKMatrix) instead.", true)]" — **binding/SkiaSharp/SKCanvas.cs line 885** (The migration from ref/value to `in` for SKMatrix parameters was carried out with proper deprecation.)

### Code Investigation

| File | Lines | Relevance | Finding |
|------|-------|-----------|---------|
| `binding/SkiaSharp/SKCanvas.cs` | 235-243 | direct | SKCanvas.Concat(in SKMatrix m) already uses `in` keyword — the exact change requested in the issue is implemented. |
| `binding/SkiaSharp/SKCanvas.cs` | 882-893 | direct | SKCanvas.SetMatrix(in SKMatrix matrix) uses `in`. The old value-based overload SetMatrix(SKMatrix matrix) is marked [Obsolete(error:true)] with message 'Use SetMatrix(in SKMatrix) instead.' |
| `binding/SkiaSharp/SKPath.cs` | 331-352 | direct | SKPath.Transform(in SKMatrix matrix) uses `in`. Old overloads are marked [Obsolete(error:true)] with messages pointing to `in` variants. |
| `binding/SkiaSharp/SKMatrix.cs` | 290-295 | related | SKMatrix.Concat(ref SKMatrix target, ...) retains `ref` because `target` is a mutable output parameter — intentional, not covered by the feature request. |

### Next Questions

- Confirm which SkiaSharp release version first introduced the `in SKMatrix` overloads so a fix version can be cited in the closure comment.

## Recommendations

### Actionability

| Field | Value |
|-------|-------|
| Suggested action | close-as-fixed |
| Confidence | 0.95 (95%) |
| Reason | The requested change (using `in` instead of `ref` for SKMatrix parameters) is fully implemented in the current codebase. SKCanvas.Concat, SKCanvas.SetMatrix, SKPath.Transform, and other key methods all use `in SKMatrix`. |
| Suggested repro platform | linux |

### Automatable Actions

| Type | Risk | Confidence | Description | Details |
|------|------|------------|-------------|---------|
| update-labels | low | 0.95 (95%) | Apply enhancement and SkiaSharp area labels with performance tenet | labels=type/enhancement, area/SkiaSharp, tenet/performance |
| add-comment | medium | 0.95 (95%) | Inform reporter that the feature has already been implemented | — |
| close-issue | medium | 0.95 (95%) | Close as the feature has already been implemented | stateReason=completed |

**Comment draft for `add-comment`:**

```markdown
Thanks for this suggestion! The change you requested has already been implemented in the current codebase.

`SKCanvas.Concat`, `SKCanvas.SetMatrix`, `SKPath.Transform`, `SKPath.AddPath`, and other methods that previously used `ref SKMatrix` now use the `in` keyword:

```csharp
public void Concat(in SKMatrix m)       // ✅ already uses `in`
public void SetMatrix(in SKMatrix matrix) // ✅ already uses `in`
public void Transform(in SKMatrix matrix) // ✅ already uses `in` (on SKPath)
```

The old value-based overloads are marked `[Obsolete]` with errors pointing to the `in` versions. You should be able to write `canvas.Concat(in obj.Matrix)` now.

Note: A few `ref` parameters remain intentionally where the callee writes to the parameter (e.g., `SKMatrix.Concat(ref SKMatrix target, ...)` or `SKPaint.MeasureText(ref SKRect bounds)`) — these are output parameters and cannot be changed to `in`.
```

<details>
<summary>Raw JSON</summary>

```json
{
  "meta": {
    "schemaVersion": "1.0",
    "number": 2314,
    "repo": "mono/SkiaSharp",
    "analyzedAt": "2026-04-23T09:02:27Z"
  },
  "summary": "Feature request to replace `ref` parameters with `in` parameters for SKMatrix and similar structs in methods like SKCanvas.Concat — the requested change has already been implemented.",
  "classification": {
    "type": {
      "value": "type/enhancement",
      "confidence": 0.95
    },
    "area": {
      "value": "area/SkiaSharp",
      "confidence": 0.95
    },
    "tenets": [
      "tenet/performance"
    ]
  },
  "evidence": {
    "reproEvidence": {
      "codeSnippets": [
        "canvas.Concat(ref obj.Matrix); // CS0192: A readonly field cannot be used as a ref or out value"
      ],
      "environmentDetails": "SkiaSharp (version unspecified, filed 2022-11-17)"
    },
    "fixStatus": {
      "likelyFixed": true,
      "confidence": 0.97,
      "reason": "SKCanvas.Concat and SetMatrix now use `in SKMatrix` in the current codebase. Old `ref`-based overloads are marked [Obsolete(error:true)]. SKPath.Transform also migrated to `in SKMatrix`. The primary use case from the issue report is resolved.",
      "relatedPRs": []
    }
  },
  "analysis": {
    "summary": "The reporter correctly identified that using `ref` for read-only struct parameters prevents callers from passing `readonly` fields (CS0192). The fix — switching to `in` keyword — has already been applied: SKCanvas.Concat(in SKMatrix m), SKCanvas.SetMatrix(in SKMatrix matrix), SKPath.Transform(in SKMatrix matrix), and others already use `in`. The old `ref`/value overloads are marked obsolete. The remaining `ref` parameters (SKMatrix.Concat target, SKPaint.MeasureText bounds, SKCodec.GetValidSubset) are intentional mutation/output parameters.",
    "rationale": "The enhancement was valid and performance-relevant. Code investigation confirms the primary requested change (SKCanvas.Concat) is already implemented. Remaining `ref` parameters in public APIs serve different purposes (output/mutation) and are not covered by the feature request.",
    "codeInvestigation": [
      {
        "file": "binding/SkiaSharp/SKCanvas.cs",
        "lines": "235-243",
        "finding": "SKCanvas.Concat(in SKMatrix m) already uses `in` keyword — the exact change requested in the issue is implemented.",
        "relevance": "direct"
      },
      {
        "file": "binding/SkiaSharp/SKCanvas.cs",
        "lines": "882-893",
        "finding": "SKCanvas.SetMatrix(in SKMatrix matrix) uses `in`. The old value-based overload SetMatrix(SKMatrix matrix) is marked [Obsolete(error:true)] with message 'Use SetMatrix(in SKMatrix) instead.'",
        "relevance": "direct"
      },
      {
        "file": "binding/SkiaSharp/SKPath.cs",
        "lines": "331-352",
        "finding": "SKPath.Transform(in SKMatrix matrix) uses `in`. Old overloads are marked [Obsolete(error:true)] with messages pointing to `in` variants.",
        "relevance": "direct"
      },
      {
        "file": "binding/SkiaSharp/SKMatrix.cs",
        "lines": "290-295",
        "finding": "SKMatrix.Concat(ref SKMatrix target, ...) retains `ref` because `target` is a mutable output parameter — intentional, not covered by the feature request.",
        "relevance": "related"
      }
    ],
    "keySignals": [
      {
        "text": "public void Concat (ref SKMatrix m)",
        "source": "issue body",
        "interpretation": "This was the problematic signature at the time of filing. It has since been changed to `in SKMatrix m`."
      },
      {
        "text": "SKCanvas.Concat(in SKMatrix m) =>",
        "source": "binding/SkiaSharp/SKCanvas.cs line 235",
        "interpretation": "Current code already uses `in` — the fix was already applied."
      },
      {
        "text": "[Obsolete(\"Use SetMatrix(in SKMatrix) instead.\", true)]",
        "source": "binding/SkiaSharp/SKCanvas.cs line 885",
        "interpretation": "The migration from ref/value to `in` for SKMatrix parameters was carried out with proper deprecation."
      }
    ],
    "nextQuestions": [
      "Confirm which SkiaSharp release version first introduced the `in SKMatrix` overloads so a fix version can be cited in the closure comment."
    ]
  },
  "output": {
    "actionability": {
      "suggestedAction": "close-as-fixed",
      "confidence": 0.95,
      "reason": "The requested change (using `in` instead of `ref` for SKMatrix parameters) is fully implemented in the current codebase. SKCanvas.Concat, SKCanvas.SetMatrix, SKPath.Transform, and other key methods all use `in SKMatrix`.",
      "suggestedReproPlatform": "linux"
    },
    "actions": [
      {
        "type": "update-labels",
        "description": "Apply enhancement and SkiaSharp area labels with performance tenet",
        "risk": "low",
        "confidence": 0.95,
        "labels": [
          "type/enhancement",
          "area/SkiaSharp",
          "tenet/performance"
        ]
      },
      {
        "type": "add-comment",
        "description": "Inform reporter that the feature has already been implemented",
        "risk": "medium",
        "confidence": 0.95,
        "comment": "Thanks for this suggestion! The change you requested has already been implemented in the current codebase.\n\n`SKCanvas.Concat`, `SKCanvas.SetMatrix`, `SKPath.Transform`, `SKPath.AddPath`, and other methods that previously used `ref SKMatrix` now use the `in` keyword:\n\n```csharp\npublic void Concat(in SKMatrix m)       // ✅ already uses `in`\npublic void SetMatrix(in SKMatrix matrix) // ✅ already uses `in`\npublic void Transform(in SKMatrix matrix) // ✅ already uses `in` (on SKPath)\n```\n\nThe old value-based overloads are marked `[Obsolete]` with errors pointing to the `in` versions. You should be able to write `canvas.Concat(in obj.Matrix)` now.\n\nNote: A few `ref` parameters remain intentionally where the callee writes to the parameter (e.g., `SKMatrix.Concat(ref SKMatrix target, ...)` or `SKPaint.MeasureText(ref SKRect bounds)`) — these are output parameters and cannot be changed to `in`."
      },
      {
        "type": "close-issue",
        "description": "Close as the feature has already been implemented",
        "risk": "medium",
        "confidence": 0.95,
        "stateReason": "completed"
      }
    ]
  }
}
```

</details>
