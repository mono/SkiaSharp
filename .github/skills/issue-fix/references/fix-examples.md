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

---

## Example 3: Performance Bug Fix (status: "fixed")

Based on #3525 — macOS GL rendering 15.6× slower than native C++.

```json
{
  "meta": {
    "schemaVersion": "1.0",
    "number": 3525,
    "repo": "mono/SkiaSharp",
    "analyzedAt": "2026-03-05T12:00:00Z",
    "inputs": {
      "triageFile": "ai-triage/3525.json",
      "reproFile": "ai-repro/3525.json"
    }
  },
  "status": {
    "value": "fixed"
  },
  "summary": "SKGLView reads stencil bits from glGetIntegerv (returns 0 on macOS) instead of NSOpenGLPixelFormat (returns 8). Without stencil, Skia disables TessellationPathRenderer and falls back to slow DefaultPathRenderer. Fix: read stencil from pixel format first, GL fallback second. Result: 5.3fps → 77-93fps (15.6× speedup).",
  "rootCause": {
    "category": "platform-quirk",
    "area": "platform",
    "description": "macOS glGetIntegerv(GL_STENCIL_BITS) returns 0 for default framebuffer even when pixel format allocates 8 stencil bits. SKGLView used this incorrect value in GRBackendRenderTarget, disabling Skia's fast TessellationPathRenderer.",
    "confidence": 0.98,
    "affectedFiles": [
      "source/SkiaSharp.Views/SkiaSharp.Views/Platform/macOS/SKGLView.cs"
    ]
  },
  "changes": {
    "files": [
      {
        "path": "source/SkiaSharp.Views/SkiaSharp.Views/Platform/macOS/SKGLView.cs",
        "changeType": "modified",
        "description": "Read stencil bits from NSOpenGLPixelFormat.GetValue() instead of glGetIntegerv, with GL fallback"
      }
    ],
    "breakingChange": false,
    "risk": "low"
  },
  "tests": {
    "regressionTestAdded": false,
    "testsAdded": [],
    "command": "dotnet build source/SkiaSharp.Views/SkiaSharp.Views/SkiaSharp.Views.csproj",
    "result": "passed"
  },
  "verification": {
    "reproScenario": "passed",
    "method": "device",
    "notes": "Benchmarked across 12 test apps at 4 complexity levels. GL went from 5.3fps to 77-93fps. Metal unchanged (still matches native). C# GL now 26-52% faster than native C++ GL."
  },
  "pr": {
    "number": 3546,
    "url": "https://github.com/mono/SkiaSharp/pull/3546",
    "branch": "dev/fix-macos-gl-stencil-perf",
    "title": "Fix macOS GL performance: read stencil bits from pixel format"
  }
}
```

**Key patterns for perf fixes:**
- `rootCause.category: "platform-quirk"` — the bug is in macOS GL driver behavior
- `verification.method: "device"` — performance was measured on real hardware
- `verification.notes` includes before/after numbers with comparison to native baseline
- `tests.regressionTestAdded: false` — performance test would need GPU, acceptable to skip

---

## Example 4: Needs-Info (status: "needs-info")

Investigation couldn't reproduce the issue — need more information from reporter.

```json
{
  "meta": {
    "schemaVersion": "1.0",
    "number": 9999,
    "repo": "mono/SkiaSharp",
    "analyzedAt": "2026-03-01T10:00:00Z"
  },
  "status": {
    "value": "needs-info"
  },
  "summary": "Cannot reproduce the reported crash in SKBitmap.Decode on macOS ARM64 with SkiaSharp 3.116.1 and .NET 10.0.102. Tested with 12 image formats including the reporter's HEIF sample. Need full exception stack trace and exact SDK version.",
  "rootCause": {
    "category": "other",
    "area": "unknown",
    "description": "Unable to determine root cause without reproduction. Reporter's stack trace was truncated at 3 frames, insufficient for diagnosis.",
    "confidence": 0.3,
    "affectedFiles": []
  },
  "changes": {
    "files": [],
    "breakingChange": false,
    "risk": "low"
  },
  "tests": {
    "regressionTestAdded": false,
    "testsAdded": [],
    "command": "dotnet test tests/SkiaSharp.Tests.Console/SkiaSharp.Tests.Console.csproj",
    "result": "passed"
  },
  "verification": {
    "reproScenario": "not-run",
    "method": "console",
    "notes": "All existing decode tests pass. Could not reproduce with any image format tested."
  },
  "blockers": [
    "Full exception stack trace (not truncated)",
    "Exact .NET SDK version from `dotnet --info`",
    "Minimal reproduction project (uploaded as .zip)"
  ]
}
```

**Key patterns for needs-info:**
- `blockers` is REQUIRED when status=needs-info — list exactly what's missing
- `rootCause.confidence` is low (0.3) — honest about uncertainty
- `verification.reproScenario: "not-run"` — couldn't reproduce, so couldn't verify
- Tests still run to establish baseline health
