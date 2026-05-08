# Issue Triage Report — #2263

| Field | Value |
|-------|-------|
| Repository | mono/SkiaSharp |
| Analyzed | 2026-04-22T01:49:22Z |
| Type | type/bug (0.82 (82%)) |
| Area | area/SkiaSharp (0.95 (95%)) |
| Suggested action | ready-to-fix (0.82 (82%)) |

**Issue Summary:** SKBitmap.Decode(Stream) unexpectedly disposes the caller-provided stream because the internal SKCodec.WrapManagedStream always wraps with disposeManagedStream:true, violating the .NET convention that callers control stream lifecycle.

**Analysis:** SKBitmap.Decode(Stream) routes through SKCodec.Create(Stream) → SKCodec.WrapManagedStream, which always wraps the caller's stream with SKManagedStream(stream, disposeManagedStream: true). When the SKCodec is disposed at the end of Decode, the wrapper disposes the underlying stream. This violates the standard .NET convention that a method should not dispose a resource it did not create. The maintainer confirmed the behavior is intentional (Skia reads to end; stream may not be seekable) but acknowledged the convention violation and proposed adding an internal non-disposing wrapper as a fix.

**Recommendations:** **ready-to-fix** — Root cause is clearly identified (WrapManagedStream passes disposeManagedStream:true), fix is a one-line change, maintainer has already confirmed the intent to fix it, and two working workarounds exist for users in the meantime.

---

## Classification

| Field | Value |
|-------|-------|
| Type | type/bug |
| Area | area/SkiaSharp |
| Platforms | — |
| Backends | — |
| Tenets | tenet/compatibility, tenet/reliability |
| Partner | — |

## Evidence

### Reproduction

1. Create any seekable Stream (e.g., a FileStream or MemoryStream containing an image)
2. Call SKBitmap.Decode(stream)
3. Attempt to use the stream after Decode returns
4. Observe that the stream is already disposed

**Environment:** SkiaSharp 2.88.2; any platform

**Repository links:**
- https://github.com/mono/SkiaSharp/issues/2263 — Original report — question about expected vs bug behavior

**Code snippets:**

```csharp
var bitmap = SKBitmap.Decode(stream);
// stream is now disposed — cannot continue using it
```

### Bug Signals

| Field | Value |
|-------|-------|
| Severity | medium |
| Regression claimed | False |
| Error type | wrong-output |
| Error message | Stream is disposed after calling SKBitmap.Decode(stream) |
| Repro quality | partial |
| Target frameworks | net6.0 |

### Version Analysis

| Field | Value |
|-------|-------|
| Mentioned versions | 2.88.2 |
| Worked in | — |
| Broke in | — |
| Current relevance | likely |
| Relevance reason | SKCodec.WrapManagedStream still passes disposeManagedStream:true in current code, so the behavior is unchanged. |

## Analysis

### Technical Summary

SKBitmap.Decode(Stream) routes through SKCodec.Create(Stream) → SKCodec.WrapManagedStream, which always wraps the caller's stream with SKManagedStream(stream, disposeManagedStream: true). When the SKCodec is disposed at the end of Decode, the wrapper disposes the underlying stream. This violates the standard .NET convention that a method should not dispose a resource it did not create. The maintainer confirmed the behavior is intentional (Skia reads to end; stream may not be seekable) but acknowledged the convention violation and proposed adding an internal non-disposing wrapper as a fix.

### Rationale

Classified as type/bug because the behavior violates the standard .NET convention that consumers must not dispose resources they didn't create (see StreamReader, JsonDocument, etc., which all accept a leaveOpen parameter). While the Skia-side behavior is intentional, the .NET wrapper should shield callers from this. The maintainer has already acknowledged the issue is worth fixing. Severity is medium because clear workarounds exist and no crash is involved — just unexpected resource invalidation.

### Key Signals

- "i just feel it is not right, stream should be disposed by user" — **issue comment #1261527531** (Reporter confirms this is not a regression — the concern is a design/convention violation.)
- "This is a really bad design choice if not a bug." — **issue comment #1685777924** (Community agrees the behavior violates .NET expectations (16 upvotes).)
- "I am pretty sure this is expected. The decode usually reads all the way to the end, and the stream may not always be rewindable. So there was not much point in keeping it open." — **maintainer comment #1685893843** (Intentional behavior rooted in Skia's design — streams passed to Skia codecs are consumed.)
- "I think we could add some internal wrapper/proxy stream so you can disable this feature. @tigransard has a good workaround that we could implement internally." — **maintainer comment #3288657972** (Maintainer has acknowledged the API design problem and proposes an internal fix using SKManagedStream(stream, disposeManagedStream:false).)

### Code Investigation

| File | Lines | Relevance | Finding |
|------|-------|-----------|---------|
| `binding/SkiaSharp/SKCodec.cs` | 278-290 | direct | WrapManagedStream always instantiates SKManagedStream(stream, true) or SKFrontBufferedManagedStream(stream, ..., true), passing disposeManagedStream:true in both branches — this is the root cause of stream disposal. |
| `binding/SkiaSharp/SKBitmap.cs` | 461-485 | direct | Decode(Stream) calls SKCodec.Create(stream) in a using block. When the using block disposes the codec, the codec revokes ownership to itself (via RevokeOwnership), then the managed stream wrapper is also disposed, propagating disposal to the caller's stream. |
| `binding/SkiaSharp/SKManagedStream.cs` | 25-29 | related | SKManagedStream constructor accepts bool disposeManagedStream; when true, the wrapped Stream is disposed in DisposeManaged. Default constructor passes false — so the only problematic path is the internal WrapManagedStream helper. |
| `binding/SkiaSharp/SKCodec.cs` | 252-263 | related | SKCodec.Create(SKStream) calls stream.RevokeOwnership(codec), transferring native ownership of the stream to the codec. When codec is disposed, the native side also releases the stream handle, triggering the managed disposal chain. |

### Workarounds

- Use SKData.Create(stream) to buffer the data first, then call SKBitmap.Decode(skData). The stream is not disposed by this path.
- Wrap the stream manually: var skStream = new SKManagedStream(stream, disposeManagedStream: false); var bitmap = SKBitmap.Decode(skStream); — the original stream remains open after Decode returns.

### Next Questions

- Should the fix be a behavior change (WrapManagedStream passes false) or a new leaveOpen overload?
- Do other Decode-accepting methods (SKImage.FromEncodedData, SKCodec.Create, SKTypeface.FromStream) have the same issue?

### Resolution Proposals

**Hypothesis:** WrapManagedStream should not take ownership of the caller's stream. Passing disposeManagedStream:false in both branches of WrapManagedStream would fix the root cause without any API signature change.

1. **Use SKData to buffer stream before decoding** — workaround, confidence 0.95 (95%), cost/xs, validated=yes
   - Wrap the stream in an SKData object first. SKData reads all bytes into a native buffer; decoding proceeds from the buffer and the original stream is not disposed.

```csharp
using var data = SKData.Create(stream);
var bitmap = SKBitmap.Decode(data);
// stream is still open here
```
2. **Use SKManagedStream with disposeManagedStream:false** — workaround, confidence 0.95 (95%), cost/xs, validated=yes
   - Wrap the stream in SKManagedStream with disposeManagedStream:false before passing to Decode. The SKStream overload of Decode does not call WrapManagedStream, so the original stream is never disposed.

```csharp
using var skStream = new SKManagedStream(stream, disposeManagedStream: false);
var bitmap = SKBitmap.Decode(skStream);
// stream is still open here
```
3. **Fix WrapManagedStream to not dispose caller stream** — fix, confidence 0.80 (80%), cost/xs, validated=yes
   - Change SKCodec.WrapManagedStream to pass disposeManagedStream:false. This is the root cause; fixing it here would fix all callers (SKBitmap.Decode, SKImage.FromEncodedData, etc.) at once.

```csharp
// In SKCodec.cs WrapManagedStream:
if (stream.CanSeek) {
    return new SKManagedStream(stream, false); // was true
} else {
    return new SKFrontBufferedManagedStream(stream, MinBufferedBytesNeeded, false); // was true
}
```

**Recommended proposal:** Fix WrapManagedStream to not dispose caller stream

**Why:** One-line fix at the root cause, consistent with .NET conventions, aligns with maintainer's stated intent to add an internal wrapper. The behavior change is unlikely to break callers since it preserves more resources.

## Recommendations

### Actionability

| Field | Value |
|-------|-------|
| Suggested action | ready-to-fix |
| Confidence | 0.82 (82%) |
| Reason | Root cause is clearly identified (WrapManagedStream passes disposeManagedStream:true), fix is a one-line change, maintainer has already confirmed the intent to fix it, and two working workarounds exist for users in the meantime. |
| Suggested repro platform | linux |

### Automatable Actions

| Type | Risk | Confidence | Description | Details |
|------|------|------------|-------------|---------|
| update-labels | low | 0.90 (90%) | Apply bug, core SkiaSharp, and tenet labels | labels=type/bug, area/SkiaSharp, tenet/compatibility, tenet/reliability |
| add-comment | medium | 0.85 (85%) | Post explanation with confirmed workarounds and proposed fix path | — |

**Comment draft for `add-comment`:**

```markdown
Thanks for raising this. The behavior is caused by `SKCodec.WrapManagedStream` (in `SKCodec.cs`) wrapping the caller's stream with `disposeManagedStream: true`, which means Skia disposes the stream when the codec is released.

This violates the .NET convention that a method should not dispose resources it didn't create. The maintainer has acknowledged the issue and proposed an internal fix.

**Workarounds available now:**

1. Buffer via `SKData` — stream is never passed to the codec:
```csharp
using var data = SKData.Create(stream);
var bitmap = SKBitmap.Decode(data);
// stream is still open here
```

2. Use `SKManagedStream` with `disposeManagedStream: false`:
```csharp
using var skStream = new SKManagedStream(stream, disposeManagedStream: false);
var bitmap = SKBitmap.Decode(skStream);
// stream is still open here
```

**Proposed fix:** Change `WrapManagedStream` in `SKCodec.cs` to pass `false` for `disposeManagedStream`. This would fix all callers (`SKBitmap.Decode`, `SKImage.FromEncodedData`, etc.) in one place.
```

<details>
<summary>Raw JSON</summary>

```json
{
  "meta": {
    "schemaVersion": "1.0",
    "number": 2263,
    "repo": "mono/SkiaSharp",
    "analyzedAt": "2026-04-22T01:49:22Z"
  },
  "summary": "SKBitmap.Decode(Stream) unexpectedly disposes the caller-provided stream because the internal SKCodec.WrapManagedStream always wraps with disposeManagedStream:true, violating the .NET convention that callers control stream lifecycle.",
  "classification": {
    "type": {
      "value": "type/bug",
      "confidence": 0.82
    },
    "area": {
      "value": "area/SkiaSharp",
      "confidence": 0.95
    },
    "tenets": [
      "tenet/compatibility",
      "tenet/reliability"
    ]
  },
  "evidence": {
    "bugSignals": {
      "severity": "medium",
      "regressionClaimed": false,
      "errorType": "wrong-output",
      "errorMessage": "Stream is disposed after calling SKBitmap.Decode(stream)",
      "reproQuality": "partial",
      "targetFrameworks": [
        "net6.0"
      ]
    },
    "reproEvidence": {
      "stepsToReproduce": [
        "Create any seekable Stream (e.g., a FileStream or MemoryStream containing an image)",
        "Call SKBitmap.Decode(stream)",
        "Attempt to use the stream after Decode returns",
        "Observe that the stream is already disposed"
      ],
      "environmentDetails": "SkiaSharp 2.88.2; any platform",
      "repoLinks": [
        {
          "url": "https://github.com/mono/SkiaSharp/issues/2263",
          "description": "Original report — question about expected vs bug behavior"
        }
      ],
      "codeSnippets": [
        "var bitmap = SKBitmap.Decode(stream);\n// stream is now disposed — cannot continue using it"
      ]
    },
    "versionAnalysis": {
      "mentionedVersions": [
        "2.88.2"
      ],
      "currentRelevance": "likely",
      "relevanceReason": "SKCodec.WrapManagedStream still passes disposeManagedStream:true in current code, so the behavior is unchanged."
    }
  },
  "analysis": {
    "summary": "SKBitmap.Decode(Stream) routes through SKCodec.Create(Stream) → SKCodec.WrapManagedStream, which always wraps the caller's stream with SKManagedStream(stream, disposeManagedStream: true). When the SKCodec is disposed at the end of Decode, the wrapper disposes the underlying stream. This violates the standard .NET convention that a method should not dispose a resource it did not create. The maintainer confirmed the behavior is intentional (Skia reads to end; stream may not be seekable) but acknowledged the convention violation and proposed adding an internal non-disposing wrapper as a fix.",
    "codeInvestigation": [
      {
        "file": "binding/SkiaSharp/SKCodec.cs",
        "lines": "278-290",
        "finding": "WrapManagedStream always instantiates SKManagedStream(stream, true) or SKFrontBufferedManagedStream(stream, ..., true), passing disposeManagedStream:true in both branches — this is the root cause of stream disposal.",
        "relevance": "direct"
      },
      {
        "file": "binding/SkiaSharp/SKBitmap.cs",
        "lines": "461-485",
        "finding": "Decode(Stream) calls SKCodec.Create(stream) in a using block. When the using block disposes the codec, the codec revokes ownership to itself (via RevokeOwnership), then the managed stream wrapper is also disposed, propagating disposal to the caller's stream.",
        "relevance": "direct"
      },
      {
        "file": "binding/SkiaSharp/SKManagedStream.cs",
        "lines": "25-29",
        "finding": "SKManagedStream constructor accepts bool disposeManagedStream; when true, the wrapped Stream is disposed in DisposeManaged. Default constructor passes false — so the only problematic path is the internal WrapManagedStream helper.",
        "relevance": "related"
      },
      {
        "file": "binding/SkiaSharp/SKCodec.cs",
        "lines": "252-263",
        "finding": "SKCodec.Create(SKStream) calls stream.RevokeOwnership(codec), transferring native ownership of the stream to the codec. When codec is disposed, the native side also releases the stream handle, triggering the managed disposal chain.",
        "relevance": "related"
      }
    ],
    "keySignals": [
      {
        "text": "i just feel it is not right, stream should be disposed by user",
        "source": "issue comment #1261527531",
        "interpretation": "Reporter confirms this is not a regression — the concern is a design/convention violation."
      },
      {
        "text": "This is a really bad design choice if not a bug.",
        "source": "issue comment #1685777924",
        "interpretation": "Community agrees the behavior violates .NET expectations (16 upvotes)."
      },
      {
        "text": "I am pretty sure this is expected. The decode usually reads all the way to the end, and the stream may not always be rewindable. So there was not much point in keeping it open.",
        "source": "maintainer comment #1685893843",
        "interpretation": "Intentional behavior rooted in Skia's design — streams passed to Skia codecs are consumed."
      },
      {
        "text": "I think we could add some internal wrapper/proxy stream so you can disable this feature. @tigransard has a good workaround that we could implement internally.",
        "source": "maintainer comment #3288657972",
        "interpretation": "Maintainer has acknowledged the API design problem and proposes an internal fix using SKManagedStream(stream, disposeManagedStream:false)."
      }
    ],
    "rationale": "Classified as type/bug because the behavior violates the standard .NET convention that consumers must not dispose resources they didn't create (see StreamReader, JsonDocument, etc., which all accept a leaveOpen parameter). While the Skia-side behavior is intentional, the .NET wrapper should shield callers from this. The maintainer has already acknowledged the issue is worth fixing. Severity is medium because clear workarounds exist and no crash is involved — just unexpected resource invalidation.",
    "workarounds": [
      "Use SKData.Create(stream) to buffer the data first, then call SKBitmap.Decode(skData). The stream is not disposed by this path.",
      "Wrap the stream manually: var skStream = new SKManagedStream(stream, disposeManagedStream: false); var bitmap = SKBitmap.Decode(skStream); — the original stream remains open after Decode returns."
    ],
    "nextQuestions": [
      "Should the fix be a behavior change (WrapManagedStream passes false) or a new leaveOpen overload?",
      "Do other Decode-accepting methods (SKImage.FromEncodedData, SKCodec.Create, SKTypeface.FromStream) have the same issue?"
    ],
    "resolution": {
      "hypothesis": "WrapManagedStream should not take ownership of the caller's stream. Passing disposeManagedStream:false in both branches of WrapManagedStream would fix the root cause without any API signature change.",
      "proposals": [
        {
          "title": "Use SKData to buffer stream before decoding",
          "description": "Wrap the stream in an SKData object first. SKData reads all bytes into a native buffer; decoding proceeds from the buffer and the original stream is not disposed.",
          "category": "workaround",
          "codeSnippet": "using var data = SKData.Create(stream);\nvar bitmap = SKBitmap.Decode(data);\n// stream is still open here",
          "confidence": 0.95,
          "effort": "cost/xs",
          "validated": "yes"
        },
        {
          "title": "Use SKManagedStream with disposeManagedStream:false",
          "description": "Wrap the stream in SKManagedStream with disposeManagedStream:false before passing to Decode. The SKStream overload of Decode does not call WrapManagedStream, so the original stream is never disposed.",
          "category": "workaround",
          "codeSnippet": "using var skStream = new SKManagedStream(stream, disposeManagedStream: false);\nvar bitmap = SKBitmap.Decode(skStream);\n// stream is still open here",
          "confidence": 0.95,
          "effort": "cost/xs",
          "validated": "yes"
        },
        {
          "title": "Fix WrapManagedStream to not dispose caller stream",
          "description": "Change SKCodec.WrapManagedStream to pass disposeManagedStream:false. This is the root cause; fixing it here would fix all callers (SKBitmap.Decode, SKImage.FromEncodedData, etc.) at once.",
          "category": "fix",
          "codeSnippet": "// In SKCodec.cs WrapManagedStream:\nif (stream.CanSeek) {\n    return new SKManagedStream(stream, false); // was true\n} else {\n    return new SKFrontBufferedManagedStream(stream, MinBufferedBytesNeeded, false); // was true\n}",
          "confidence": 0.8,
          "effort": "cost/xs",
          "validated": "yes"
        }
      ],
      "recommendedProposal": "Fix WrapManagedStream to not dispose caller stream",
      "recommendedReason": "One-line fix at the root cause, consistent with .NET conventions, aligns with maintainer's stated intent to add an internal wrapper. The behavior change is unlikely to break callers since it preserves more resources."
    }
  },
  "output": {
    "actionability": {
      "suggestedAction": "ready-to-fix",
      "confidence": 0.82,
      "reason": "Root cause is clearly identified (WrapManagedStream passes disposeManagedStream:true), fix is a one-line change, maintainer has already confirmed the intent to fix it, and two working workarounds exist for users in the meantime.",
      "suggestedReproPlatform": "linux"
    },
    "actions": [
      {
        "type": "update-labels",
        "description": "Apply bug, core SkiaSharp, and tenet labels",
        "risk": "low",
        "confidence": 0.9,
        "labels": [
          "type/bug",
          "area/SkiaSharp",
          "tenet/compatibility",
          "tenet/reliability"
        ]
      },
      {
        "type": "add-comment",
        "description": "Post explanation with confirmed workarounds and proposed fix path",
        "risk": "medium",
        "confidence": 0.85,
        "comment": "Thanks for raising this. The behavior is caused by `SKCodec.WrapManagedStream` (in `SKCodec.cs`) wrapping the caller's stream with `disposeManagedStream: true`, which means Skia disposes the stream when the codec is released.\n\nThis violates the .NET convention that a method should not dispose resources it didn't create. The maintainer has acknowledged the issue and proposed an internal fix.\n\n**Workarounds available now:**\n\n1. Buffer via `SKData` — stream is never passed to the codec:\n```csharp\nusing var data = SKData.Create(stream);\nvar bitmap = SKBitmap.Decode(data);\n// stream is still open here\n```\n\n2. Use `SKManagedStream` with `disposeManagedStream: false`:\n```csharp\nusing var skStream = new SKManagedStream(stream, disposeManagedStream: false);\nvar bitmap = SKBitmap.Decode(skStream);\n// stream is still open here\n```\n\n**Proposed fix:** Change `WrapManagedStream` in `SKCodec.cs` to pass `false` for `disposeManagedStream`. This would fix all callers (`SKBitmap.Decode`, `SKImage.FromEncodedData`, etc.) in one place."
      }
    ]
  }
}
```

</details>
