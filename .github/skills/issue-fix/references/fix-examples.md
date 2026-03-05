# Bug Fix — Example Outputs

Reference examples showing valid `issue-fix` JSON output for different status values.
Each example conforms to [`fix-schema.json`](fix-schema.json).

## Contents
1. [Example 1: Fixed — C# API Logic Error](#example-1-fixed--c-api-logic-error)
2. [Example 2: Cannot-Fix — Upstream Skia Issue](#example-2-cannot-fix--upstream-skia-issue)
3. [Example 3: Fixed — Performance Bug](#example-3-fixed--performance-bug)
4. [Example 4: Needs-Info — Unable to Reproduce](#example-4-needs-info--unable-to-reproduce)

---

## Example 1: Fixed — C# API Logic Error

**Scenario:** `SKPaint.MeasureText` returns 0 width for certain multi-byte Unicode strings
(emoji sequences) because the byte-length calculation uses `string.Length` (UTF-16 code units)
instead of the actual UTF-8 encoded byte count passed to Skia.

```json
{
  "meta": {
    "schemaVersion": "1.0",
    "number": 1001,
    "repo": "mono/SkiaSharp",
    "analyzedAt": "2026-01-15T10:00:00Z"
  },
  "inputs": {
    "triageFile": "ai-triage/1001.json",
    "reproFile": "ai-repro/1001.json"
  },
  "status": {
    "value": "fixed",
    "reason": "Root cause identified in SKPaint.MeasureText UTF-8 encoding path and fix verified with regression test."
  },
  "summary": "SKPaint.MeasureText passed string.Length (UTF-16 code units) as the byte count to the native sk_paint_measure_text function, which expects UTF-8 byte length. For ASCII text this is identical, but multi-byte characters (emoji, CJK) produce a shorter count, causing Skia to measure a truncated string. Fixed by using Encoding.UTF8.GetByteCount(). Verified with emoji and CJK test cases.",
  "rootCause": {
    "category": "logic-error",
    "area": "managed",
    "description": "SKPaint.MeasureText used string.Length (UTF-16 code units) as the byte count parameter for the native P/Invoke call. The native function sk_paint_measure_text expects UTF-8 byte length. For ASCII these are identical, but for multi-byte characters the UTF-16 length is shorter than the UTF-8 byte count, causing truncated measurement.",
    "confidence": 0.95,
    "affectedFiles": [
      "binding/SkiaSharp/SKPaint.cs"
    ]
  },
  "changes": {
    "files": [
      {
        "path": "binding/SkiaSharp/SKPaint.cs",
        "changeType": "modified",
        "summary": "Replaced string.Length with Encoding.UTF8.GetByteCount(text) in MeasureText P/Invoke call."
      },
      {
        "path": "tests/SkiaSharp.Tests.Console/SKPaintTest.cs",
        "changeType": "modified",
        "summary": "Added regression tests for emoji and CJK text measurement."
      }
    ],
    "breakingChange": false,
    "risk": "low"
  },
  "tests": {
    "regressionTestAdded": true,
    "testsAdded": [
      {
        "file": "tests/SkiaSharp.Tests.Console/SKPaintTest.cs",
        "name": "MeasureTextReturnsNonZeroForEmoji",
        "description": "Verifies MeasureText returns positive width for emoji string U+1F600."
      }
    ],
    "command": "dotnet test tests/SkiaSharp.Tests.Console/SkiaSharp.Tests.Console.csproj",
    "result": "passed"
  },
  "verification": {
    "reproScenario": "passed",
    "method": "automated-test",
    "notes": "Original repro scenario (measuring emoji string) now returns correct non-zero width. Full test suite passes with 0 failures."
  },
  "pr": {
    "number": 1050,
    "url": "https://github.com/mono/SkiaSharp/pull/1050",
    "status": "open"
  },
  "feedback": {
    "corrections": [
      {
        "source": "triage",
        "topic": "root-cause",
        "upstream": "Triage hypothesized the issue was in the C API text encoding layer.",
        "corrected": "The C API correctly passes through byte buffers. The issue is in the C# wrapper computing the wrong byte count."
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

**Scenario:** Animated WebP frames lose their alpha channel when decoded via SKCodec.
This is a limitation in Skia's SkWebpCodec, not in SkiaSharp's bindings.

```json
{
  "meta": {
    "schemaVersion": "1.0",
    "number": 2002,
    "repo": "mono/SkiaSharp",
    "analyzedAt": "2026-02-10T15:00:00Z"
  },
  "inputs": {
    "triageFile": "ai-triage/2002.json",
    "reproFile": "ai-repro/2002.json"
  },
  "status": {
    "value": "cannot-fix",
    "reason": "Animated WebP alpha decoding requires upstream Skia changes to SkWebpCodec that are outside SkiaSharp's control."
  },
  "summary": "Investigated animated WebP alpha channel loss and confirmed that Skia's SkWebpCodec discards alpha when decoding subsequent frames of an animated WebP. The C API and C# bindings correctly expose SKCodec — the underlying Skia implementation doesn't preserve per-frame alpha for animated WebP. Filed upstream.",
  "rootCause": {
    "category": "upstream-skia",
    "area": "native",
    "description": "SkWebpCodec in externals/skia/src/codec/SkWebpCodec.cpp does not preserve per-frame alpha blending for animated WebP. Frame 0 decodes correctly, but subsequent frames are decoded with kOpaque alpha type regardless of source data.",
    "confidence": 0.90,
    "affectedFiles": [
      "externals/skia/src/codec/SkWebpCodec.cpp"
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
    "notes": "Reviewed Skia codec source. Confirmed animated WebP alpha handling is incomplete. No SkiaSharp code change possible."
  },
  "blockers": [
    "Upstream Skia SkWebpCodec does not preserve per-frame alpha for animated WebP",
    "Requires contribution to skia.googlesource.com codec module"
  ],
  "relatedIssues": [1998]
}
```

### Why this is a good `cannot-fix` example

- **`status.value` is `"cannot-fix"`** with a clear `reason`.
- **`blockers` is present** — required by the allOf rule when status is `cannot-fix` or `needs-info`.
- **`changes.files` is empty** — no code changes were made since the fix is upstream.
- **`tests.result` is `"not-run"`** — appropriate since no changes were made.
- **`verification.method` is `"code-review"`** — verified by reading upstream Skia source.
- **`rootCause.area` is `"native"`** — correctly identifies the upstream layer.
- **`relatedIssues`** links to the upstream tracking issue.

---

## Example 3: Fixed — Performance Bug

**Scenario:** Android GL rendering stutters because the view calls `GRContext.Flush()` twice
per frame — once in the draw handler and once in the view's `OnDraw` wrapper.

```json
{
  "meta": {
    "schemaVersion": "1.0",
    "number": 3003,
    "repo": "mono/SkiaSharp",
    "analyzedAt": "2026-03-01T12:00:00Z"
  },
  "inputs": {
    "triageFile": "ai-triage/3003.json",
    "reproFile": "ai-repro/3003.json"
  },
  "status": {
    "value": "fixed",
    "reason": "Removed redundant GRContext.Flush() call in Android SKGLSurfaceView. Verified 2x frame rate improvement on test device."
  },
  "summary": "Android SKGLSurfaceView called GRContext.Flush() at the end of OnDrawFrame, but the base GLSurfaceView already triggers eglSwapBuffers which implicitly flushes. The redundant flush caused a full GPU pipeline stall per frame. Removed the explicit flush. Result: 30fps to 60fps on Pixel 8 with complex path rendering.",
  "rootCause": {
    "category": "logic-error",
    "area": "managed",
    "description": "SKGLSurfaceView.OnDrawFrame called GRContext.Flush() explicitly before returning. Android's GLSurfaceView.Renderer contract already calls eglSwapBuffers after onDrawFrame, which performs an implicit flush. The redundant flush caused a full GPU pipeline stall each frame, halving the achievable frame rate.",
    "confidence": 0.92,
    "affectedFiles": [
      "source/SkiaSharp.Views/SkiaSharp.Views/Platform/Android/SKGLSurfaceView.cs"
    ]
  },
  "changes": {
    "files": [
      {
        "path": "source/SkiaSharp.Views/SkiaSharp.Views/Platform/Android/SKGLSurfaceView.cs",
        "changeType": "modified",
        "summary": "Removed redundant GRContext.Flush() call from OnDrawFrame."
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
    "method": "manual-repro",
    "notes": "Benchmarked complex path rendering on Pixel 8 (Android 15). Before: ~30fps with visible stuttering. After: stable 60fps. No visual regressions."
  },
  "pr": {
    "number": 3050,
    "url": "https://github.com/mono/SkiaSharp/pull/3050",
    "status": "open"
  }
}
```

### Key patterns for performance fixes

- `rootCause.area: "managed"` — the bug is in platform-specific managed rendering code
- `verification.method: "manual-repro"` — performance was measured on real hardware
- `verification.notes` includes before/after numbers
- `tests.regressionTestAdded: false` — performance test would need device, acceptable to skip

---

## Example 4: Needs-Info (status: "needs-info")

Investigation couldn't reproduce the issue — need more information from reporter.

```json
{
  "meta": {
    "schemaVersion": "1.0",
    "number": 4004,
    "repo": "mono/SkiaSharp",
    "analyzedAt": "2026-02-20T10:00:00Z"
  },
  "status": {
    "value": "needs-info",
    "reason": "Cannot reproduce the reported AccessViolationException. Need full crash dump and thread information."
  },
  "summary": "Cannot reproduce the reported AccessViolationException in SKImage.Encode on Linux x64 with SkiaSharp 3.118.0 and .NET 9.0. Tested with 15 image formats including the reporter's TIFF sample. The error suggests a threading issue but the reporter's code doesn't show threading. Need full crash dump to identify the actual call stack.",
  "rootCause": {
    "category": "other",
    "area": "native",
    "description": "Unable to determine root cause without reproduction. Reporter's stack trace shows AccessViolationException in native code during SKImage.Encode, suggesting either concurrent access or use-after-dispose, but no evidence of either in the provided code snippet.",
    "confidence": 0.25,
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
    "method": "automated-test",
    "notes": "All existing encode tests pass. Could not reproduce with any image format tested."
  },
  "blockers": [
    "Full crash dump (.dmp file) with native stack traces",
    "Thread dump showing all active threads at crash time",
    "Minimal reproduction project (uploaded as .zip)"
  ]
}
```

### Key patterns for needs-info

- `blockers` is required when status is `needs-info` — list exactly what's missing
- `rootCause.confidence` is low (0.25) — honest about uncertainty
- `verification.reproScenario: "not-run"` — couldn't reproduce, so couldn't verify
- Tests still run to establish baseline health
