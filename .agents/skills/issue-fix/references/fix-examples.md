# Bug Fix — Example Outputs

Reference examples showing valid `issue-fix` JSON output for different status values.
Each example conforms to [`fix-schema.json`](fix-schema.json).

## Contents
1. [Example 1: Fixed — C# API Logic Error](#example-1-fixed--c-api-logic-error)
2. [Example 2: Cannot-Fix — Upstream Skia Issue](#example-2-cannot-fix--upstream-skia-issue)

---

## Example 1: Fixed — C# API Logic Error

**Scenario:** `SKMatrix.MapRect` normalizes the output rect, sorting coordinates so that
`Left < Right` and `Top < Bottom`. This discards negative scaling information. The fix
replaces the Skia `mapRect` call with direct matrix multiplication that preserves orientation.

Based on [#2997](https://github.com/mono/SkiaSharp/issues/2997).

```json
{
  "meta": {
    "schemaVersion": "1.0",
    "number": 2997,
    "repo": "mono/SkiaSharp",
    "analyzedAt": "2025-07-01T10:00:00Z"
  },
  "inputs": {
    "triageFile": "ai-triage/2997.json",
    "reproFile": "ai-repro/2997.json"
  },
  "status": {
    "value": "fixed",
    "reason": "Root cause identified in SKMatrix.MapRect and fix verified with regression test."
  },
  "summary": "SKMatrix.MapRect called Skia's SkMatrix::mapRect which sorts the output rect coordinates, discarding negative scale orientation. Fixed by computing the four corner points via matrix multiplication and constructing an SKRect from the raw min/max without normalization. Verified the fix preserves negative scaling and passes the full test suite.",
  "rootCause": {
    "category": "logic-error",
    "area": "managed",
    "description": "SKMatrix.MapRect delegated to SkMatrix::mapRect (via sk_matrix_map_rect), which internally calls SkRect::makeSorted() on the result. This sorts Left/Right and Top/Bottom, erasing any axis flip from negative scale factors. The issue is in the C# wrapper choosing the wrong Skia function for the use case.",
    "confidence": 0.95,
    "affectedFiles": [
      "binding/SkiaSharp/SKMatrix.cs"
    ]
  },
  "changes": {
    "files": [
      {
        "path": "binding/SkiaSharp/SKMatrix.cs",
        "changeType": "modified",
        "summary": "Replaced sk_matrix_map_rect call with manual 4-corner matrix multiplication that preserves rect orientation."
      },
      {
        "path": "tests/SkiaSharp.Tests.Console/SKMatrixTest.cs",
        "changeType": "modified",
        "summary": "Added regression test for negative scale MapRect behavior."
      }
    ],
    "breakingChange": false,
    "risk": "low"
  },
  "tests": {
    "regressionTestAdded": true,
    "testsAdded": [
      {
        "file": "tests/SkiaSharp.Tests.Console/SKMatrixTest.cs",
        "name": "MapRectPreservesNegativeScale",
        "description": "Verifies that MapRect with CreateScale(-2, 1) preserves Left=0, Right=-2 orientation."
      }
    ],
    "command": "dotnet test tests/SkiaSharp.Tests.Console/SkiaSharp.Tests.Console.csproj",
    "result": "passed"
  },
  "verification": {
    "reproScenario": "passed",
    "method": "automated-test",
    "notes": "Original repro scenario (CreateScale(-2,1) mapping unit rect) now returns Left=0, Right=-2 as expected. Full test suite passes with 0 failures."
  },
  "pr": {
    "number": 3501,
    "url": "https://github.com/mono/SkiaSharp/pull/3501",
    "status": "open"
  },
  "feedback": {
    "corrections": [
      {
        "source": "triage",
        "topic": "root-cause",
        "upstream": "Triage hypothesized the issue was in the C API shim (sk_matrix_map_rect).",
        "corrected": "The C API correctly wraps Skia. The issue is in the C# wrapper choosing mapRect (which normalizes) instead of computing corners manually."
      }
    ]
  }
}
```

### Why this is a good `fixed` example

- **`status.value` is `"fixed"`** with a clear `reason` sentence.
- **`pr` is present** — required by the allOf rule when status is `fixed`.
- **`rootCause.confidence` is 0.95** — high because the root cause was confirmed via debugging.
- **`changes.breakingChange` is false** and `risk` is `"low"` — isolated to one method.
- **`feedback.corrections`** records where the fix step's findings differed from triage.
- **Test added** with `description` explaining what it validates.

---

## Example 2: Cannot-Fix — Upstream Skia Issue

**Scenario:** SkiaSharp's SVG rendering does not support `<filter>` elements. This is a
limitation in Skia's SkSVGDOM implementation, not in SkiaSharp's bindings. The fix cannot
be made without upstream Skia changes.

Based on a hypothetical issue.

```json
{
  "meta": {
    "schemaVersion": "1.0",
    "number": 9999,
    "repo": "mono/SkiaSharp",
    "analyzedAt": "2025-07-05T15:00:00Z"
  },
  "inputs": {
    "triageFile": "ai-triage/9999.json",
    "reproFile": "ai-repro/9999.json"
  },
  "status": {
    "value": "cannot-fix",
    "reason": "SVG filter support requires upstream Skia changes to SkSVGDOM that are outside SkiaSharp's control."
  },
  "summary": "Investigated SVG <filter> rendering and confirmed that Skia's SkSVGDOM parser silently ignores <filter> elements. The C API and C# bindings correctly expose SkSVGDOM, but the underlying Skia implementation lacks filter support. This is an upstream Skia limitation, not a SkiaSharp bug. Filed upstream at skia.googlesource.com.",
  "rootCause": {
    "category": "upstream-skia",
    "area": "native",
    "description": "SkSVGDOM's parser in third_party/externals/skia/modules/svg/src does not implement <filter> element handling. When encountering <filter>, it silently skips the element. SkiaSharp's bindings correctly wrap SkSVGDOM — the limitation is entirely in Skia's SVG module.",
    "confidence": 0.90,
    "affectedFiles": [
      "externals/skia/modules/svg/src/SkSVGDOM.cpp"
    ]
  },
  "changes": {
    "files": [],
    "breakingChange": false,
    "risk": "low"
  },
  "tests": {
    "regressionTestAdded": false,
    "result": "not-run"
  },
  "verification": {
    "reproScenario": "not-applicable",
    "method": "code-review",
    "notes": "Reviewed Skia SVG module source. Confirmed <filter> is not in the supported element list. No SkiaSharp code change possible."
  },
  "blockers": [
    "Upstream Skia SkSVGDOM does not implement SVG <filter> element parsing",
    "Requires contribution to skia.googlesource.com SVG module before SkiaSharp can expose it"
  ],
  "relatedIssues": [8888]
}
```

### Why this is a good `cannot-fix` example

- **`status.value` is `"cannot-fix"`** with a clear `reason`.
- **`blockers` is present** — required by the allOf rule when status is `cannot-fix` or `needs-info`.
- **`changes.files` is empty** — no code changes were made since the fix is upstream.
- **`tests.result` is `"not-run"`** — appropriate since no changes were made.
- **`verification.method` is `"code-review"`** — verified by reading upstream Skia source.
- **`rootCause.area` is `"native"`** — correctly identifies the upstream layer.
- **`relatedIssues`** links to the hypothetical upstream tracking issue.
