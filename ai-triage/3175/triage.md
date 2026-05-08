# Issue Triage Report — #3175

| Field | Value |
|-------|-------|
| Repository | mono/SkiaSharp |
| Analyzed | 2026-05-02T04:38:10Z |
| Type | type/bug (0.90 (90%)) |
| Area | area/SkiaSharp (0.92 (92%)) |
| Suggested action | needs-investigation (0.82 (82%)) |

**Issue Summary:** SKBitmap.Decode produces a partially decoded image (only the first chunk) when given a non-seekable Azure Blob stream that delivers data in 4 MB chunks, pointing to a regression from 2.88.9 to 3.116.0.

**Analysis:** When a non-seekable stream is passed to SKBitmap.Decode (or SKCodec.Create), WrapManagedStream wraps it in SKFrontBufferedManagedStream with only MinBufferedBytesNeeded bytes buffered. PNG and BMP codecs perform multi-pass reads requiring seek/rewind; OnRewind() silently fails once the read position exceeds the buffer length, leaving the codec with only the initially buffered header data for subsequent passes and producing a correctly-dimensioned but partially-filled bitmap.

**Recommendations:** **needs-investigation** — Regression claimed from 2.88.9 to 3.116.0 with a plausible code path (SKFrontBufferedManagedStream rewind failure). A minimal repro is needed to confirm before a fix is authored.

---

## Classification

| Field | Value |
|-------|-------|
| Type | type/bug |
| Area | area/SkiaSharp |
| Platforms | os/Linux, os/Windows-Classic |
| Backends | — |
| Tenets | tenet/reliability |
| Partner | — |
| Current labels | type/bug |

## Evidence

### Reproduction

1. Obtain a PNG or BMP file larger than the Azure Blob SDK default chunk size (4 MB).
2. Open a direct stream from Azure Blob storage (non-seekable, streamed in 4 MB chunks).
3. Call SKBitmap.Decode(stream) passing the raw blob stream.
4. Observe that the decoded bitmap contains only the first ~4 MB of pixel data even though Width/Height are correct.
5. As a workaround: copy the stream to a MemoryStream first, then decode—this yields a correct image.

**Environment:** SkiaSharp 3.116.0, Linux and Windows, Visual Studio

### Bug Signals

| Field | Value |
|-------|-------|
| Severity | medium |
| Regression claimed | True |
| Error type | wrong-output |
| Error message | Image decoded from large PNG/BMP only reflects the first download chunk (e.g. 4 MB), while image dimensions are reported correctly. |
| Repro quality | partial |
| Target frameworks | net8.0 |

### Version Analysis

| Field | Value |
|-------|-------|
| Mentioned versions | 3.116.0, 2.88.9 |
| Worked in | 2.88.9 |
| Broke in | 3.116.0 |
| Current relevance | likely |
| Relevance reason | SKCodec.WrapManagedStream and SKFrontBufferedManagedStream have not changed significantly since 3.116.0; the same logic is present in the current codebase. |

### Regression

| Field | Value |
|-------|-------|
| Is regression | True |
| Confidence | 0.80 (80%) |
| Reason | Reporter explicitly states 2.88.9 worked and 3.116.0 does not. The stream-wrapping path via SKFrontBufferedManagedStream was introduced in the 3.x rewrite. |
| Worked in version | 2.88.9 |
| Broke in version | 3.116.0 |

## Analysis

### Technical Summary

When a non-seekable stream is passed to SKBitmap.Decode (or SKCodec.Create), WrapManagedStream wraps it in SKFrontBufferedManagedStream with only MinBufferedBytesNeeded bytes buffered. PNG and BMP codecs perform multi-pass reads requiring seek/rewind; OnRewind() silently fails once the read position exceeds the buffer length, leaving the codec with only the initially buffered header data for subsequent passes and producing a correctly-dimensioned but partially-filled bitmap.

### Rationale

Classified as type/bug because the API accepts a Stream parameter but silently produces wrong output for non-seekable streams with images larger than the front-buffer size. The regression from 2.88.9 and the code investigation confirming the OnRewind failure path justify needs-investigation so that a minimal repro can confirm the exact failure mode before a fix is authored.

### Key Signals

- "Default stream downloading chunk size is 4mb. When png is larger than 4mb it looks like image creates only for first 4mb, but image size is correct." — **issue body** (Partial decode: header is parsed correctly (size is right) but pixel data is truncated at the first stream chunk boundary.)
- "When I'm copying blob stream into memory stream it works correctly." — **issue body** (Copying to MemoryStream (seekable) routes through SKManagedStream instead of SKFrontBufferedManagedStream, bypassing the rewind limitation.)
- "Even if I change default size of stream chunk for example into 50mb png/bmp images below 50mb downloads correctly." — **issue body** (Confirms the boundary is determined by the stream's read-chunk size, consistent with SKFrontBufferedManagedStream's fixed buffer being sized by MinBufferedBytesNeeded and the underlying stream delivering data in chunks.)

### Code Investigation

| File | Lines | Relevance | Finding |
|------|-------|-----------|---------|
| `binding/SkiaSharp/SKCodec.cs` | 278-290 | direct | WrapManagedStream branches on stream.CanSeek. Seekable streams use SKManagedStream; non-seekable use SKFrontBufferedManagedStream with MinBufferedBytesNeeded bytes. |
| `binding/SkiaSharp/SKFrontBufferedManagedStream.cs` | 158-168 | direct | OnRewind() returns true only when offset <= bufferLength. After the codec reads beyond the small front buffer, rewind fails silently, causing repeat reads to get zero bytes and producing a partial decode. |
| `binding/SkiaSharp/SKBitmap.cs` | 461-484 | related | SKBitmap.Decode(Stream) delegates to SKCodec.Create(stream) which calls WrapManagedStream—no special handling for non-seekable streams at this layer. |

### Workarounds

- Copy the non-seekable stream to a MemoryStream before decoding: `await stream.CopyToAsync(ms); ms.Seek(0, SeekOrigin.Begin); SKBitmap.Decode(ms);`
- Load the full image bytes into a byte array and use SKBitmap.Decode(byte[]) or SKBitmap.Decode(ReadOnlySpan<byte>).

### Next Questions

- Can the reporter share a standalone console repro (not requiring Azure Blob) that reads from a NetworkStream or a custom non-seekable Stream in 4 MB chunks?
- Does SKImage.FromEncodedData exhibit the same partial-decode behavior with non-seekable streams?

### Resolution Proposals

**Hypothesis:** SKFrontBufferedManagedStream.OnRewind() fails silently after the read position exceeds the front-buffer length; PNG/BMP multi-pass codecs then receive zero bytes for subsequent passes and produce a partial bitmap.

1. **Copy stream to MemoryStream before decoding** — workaround, cost/xs, validated=yes
   - Users should copy non-seekable streams to a MemoryStream (or byte array) before passing to SKBitmap.Decode. This routes through SKManagedStream (full seek support) and avoids the front-buffer limitation.

```csharp
using var ms = new MemoryStream();
await nonSeekableStream.CopyToAsync(ms);
ms.Seek(0, SeekOrigin.Begin);
using var bitmap = SKBitmap.Decode(ms);
```
2. **Buffer the full stream to memory when non-seekable in WrapManagedStream** — fix, cost/s, validated=untested
   - In SKCodec.WrapManagedStream, when the stream is not seekable, copy the entire stream to an SKDynamicMemoryWStream (or a MemoryStream) before wrapping, so the codec always receives a seekable stream. This trades memory for correctness.

**Recommended proposal:** workaround-memorystream

**Why:** The workaround is already confirmed by the reporter and requires no library change. The fix proposal needs a repro to verify the root cause before implementation.

## Recommendations

### Actionability

| Field | Value |
|-------|-------|
| Suggested action | needs-investigation |
| Confidence | 0.82 (82%) |
| Reason | Regression claimed from 2.88.9 to 3.116.0 with a plausible code path (SKFrontBufferedManagedStream rewind failure). A minimal repro is needed to confirm before a fix is authored. |
| Suggested repro platform | linux |

### Automatable Actions

| Type | Risk | Confidence | Description | Details |
|------|------|------------|-------------|---------|
| update-labels | low | 0.95 (95%) | Apply classification labels | labels=type/bug, area/SkiaSharp, os/Linux, os/Windows-Classic, tenet/reliability |
| add-comment | medium | 0.82 (82%) | Acknowledge the regression, explain the root cause hypothesis, provide copy-paste workaround, and request a minimal repro. | — |

**Comment draft for `add-comment`:**

```markdown
Thanks for the report! This looks like a regression in how SkiaSharp handles **non-seekable streams** when decoding PNG/BMP images in 3.x.

**Root cause hypothesis:**
When a non-seekable stream is passed to `SKBitmap.Decode`, the library wraps it in `SKFrontBufferedManagedStream` which only buffers a small number of bytes for codec type detection. PNG and BMP decoders require multiple seek/rewind passes; once the read position exceeds the internal buffer, `OnRewind()` silently fails, and subsequent passes read zero bytes — yielding a correctly-dimensioned but partially-filled bitmap.

**Workaround (confirmed):**
Copy the stream to a `MemoryStream` (seekable) before decoding:

```csharp
using var ms = new MemoryStream();
await nonSeekableStream.CopyToAsync(ms);
ms.Seek(0, SeekOrigin.Begin);
using var bitmap = SKBitmap.Decode(ms);
```

Alternatively, decode from a `byte[]`:

```csharp
var bytes = await BinaryData.FromStreamAsync(nonSeekableStream);
using var bitmap = SKBitmap.Decode(bytes.ToArray());
```

**To help reproduce and fix this:**
Could you provide a minimal console repro that does **not** require Azure Blob — for example, using a custom `Stream` that reads from a local file in fixed-size chunks and reports `CanSeek = false`? That would let us confirm the exact boundary condition and author a fix.
```

<details>
<summary>Raw JSON</summary>

```json
{
  "meta": {
    "schemaVersion": "1.0",
    "number": 3175,
    "repo": "mono/SkiaSharp",
    "analyzedAt": "2026-05-02T04:38:10Z",
    "currentLabels": [
      "type/bug"
    ]
  },
  "summary": "SKBitmap.Decode produces a partially decoded image (only the first chunk) when given a non-seekable Azure Blob stream that delivers data in 4 MB chunks, pointing to a regression from 2.88.9 to 3.116.0.",
  "classification": {
    "type": {
      "value": "type/bug",
      "confidence": 0.9
    },
    "area": {
      "value": "area/SkiaSharp",
      "confidence": 0.92
    },
    "platforms": [
      "os/Linux",
      "os/Windows-Classic"
    ],
    "tenets": [
      "tenet/reliability"
    ]
  },
  "evidence": {
    "bugSignals": {
      "severity": "medium",
      "regressionClaimed": true,
      "errorType": "wrong-output",
      "errorMessage": "Image decoded from large PNG/BMP only reflects the first download chunk (e.g. 4 MB), while image dimensions are reported correctly.",
      "reproQuality": "partial",
      "targetFrameworks": [
        "net8.0"
      ]
    },
    "reproEvidence": {
      "stepsToReproduce": [
        "Obtain a PNG or BMP file larger than the Azure Blob SDK default chunk size (4 MB).",
        "Open a direct stream from Azure Blob storage (non-seekable, streamed in 4 MB chunks).",
        "Call SKBitmap.Decode(stream) passing the raw blob stream.",
        "Observe that the decoded bitmap contains only the first ~4 MB of pixel data even though Width/Height are correct.",
        "As a workaround: copy the stream to a MemoryStream first, then decode—this yields a correct image."
      ],
      "environmentDetails": "SkiaSharp 3.116.0, Linux and Windows, Visual Studio"
    },
    "versionAnalysis": {
      "mentionedVersions": [
        "3.116.0",
        "2.88.9"
      ],
      "workedIn": "2.88.9",
      "brokeIn": "3.116.0",
      "currentRelevance": "likely",
      "relevanceReason": "SKCodec.WrapManagedStream and SKFrontBufferedManagedStream have not changed significantly since 3.116.0; the same logic is present in the current codebase."
    },
    "regression": {
      "isRegression": true,
      "confidence": 0.8,
      "reason": "Reporter explicitly states 2.88.9 worked and 3.116.0 does not. The stream-wrapping path via SKFrontBufferedManagedStream was introduced in the 3.x rewrite.",
      "workedInVersion": "2.88.9",
      "brokeInVersion": "3.116.0"
    }
  },
  "analysis": {
    "summary": "When a non-seekable stream is passed to SKBitmap.Decode (or SKCodec.Create), WrapManagedStream wraps it in SKFrontBufferedManagedStream with only MinBufferedBytesNeeded bytes buffered. PNG and BMP codecs perform multi-pass reads requiring seek/rewind; OnRewind() silently fails once the read position exceeds the buffer length, leaving the codec with only the initially buffered header data for subsequent passes and producing a correctly-dimensioned but partially-filled bitmap.",
    "codeInvestigation": [
      {
        "file": "binding/SkiaSharp/SKCodec.cs",
        "lines": "278-290",
        "finding": "WrapManagedStream branches on stream.CanSeek. Seekable streams use SKManagedStream; non-seekable use SKFrontBufferedManagedStream with MinBufferedBytesNeeded bytes.",
        "relevance": "direct"
      },
      {
        "file": "binding/SkiaSharp/SKFrontBufferedManagedStream.cs",
        "lines": "158-168",
        "finding": "OnRewind() returns true only when offset <= bufferLength. After the codec reads beyond the small front buffer, rewind fails silently, causing repeat reads to get zero bytes and producing a partial decode.",
        "relevance": "direct"
      },
      {
        "file": "binding/SkiaSharp/SKBitmap.cs",
        "lines": "461-484",
        "finding": "SKBitmap.Decode(Stream) delegates to SKCodec.Create(stream) which calls WrapManagedStream—no special handling for non-seekable streams at this layer.",
        "relevance": "related"
      }
    ],
    "keySignals": [
      {
        "text": "Default stream downloading chunk size is 4mb. When png is larger than 4mb it looks like image creates only for first 4mb, but image size is correct.",
        "source": "issue body",
        "interpretation": "Partial decode: header is parsed correctly (size is right) but pixel data is truncated at the first stream chunk boundary."
      },
      {
        "text": "When I'm copying blob stream into memory stream it works correctly.",
        "source": "issue body",
        "interpretation": "Copying to MemoryStream (seekable) routes through SKManagedStream instead of SKFrontBufferedManagedStream, bypassing the rewind limitation."
      },
      {
        "text": "Even if I change default size of stream chunk for example into 50mb png/bmp images below 50mb downloads correctly.",
        "source": "issue body",
        "interpretation": "Confirms the boundary is determined by the stream's read-chunk size, consistent with SKFrontBufferedManagedStream's fixed buffer being sized by MinBufferedBytesNeeded and the underlying stream delivering data in chunks."
      }
    ],
    "rationale": "Classified as type/bug because the API accepts a Stream parameter but silently produces wrong output for non-seekable streams with images larger than the front-buffer size. The regression from 2.88.9 and the code investigation confirming the OnRewind failure path justify needs-investigation so that a minimal repro can confirm the exact failure mode before a fix is authored.",
    "workarounds": [
      "Copy the non-seekable stream to a MemoryStream before decoding: `await stream.CopyToAsync(ms); ms.Seek(0, SeekOrigin.Begin); SKBitmap.Decode(ms);`",
      "Load the full image bytes into a byte array and use SKBitmap.Decode(byte[]) or SKBitmap.Decode(ReadOnlySpan<byte>)."
    ],
    "nextQuestions": [
      "Can the reporter share a standalone console repro (not requiring Azure Blob) that reads from a NetworkStream or a custom non-seekable Stream in 4 MB chunks?",
      "Does SKImage.FromEncodedData exhibit the same partial-decode behavior with non-seekable streams?"
    ],
    "resolution": {
      "hypothesis": "SKFrontBufferedManagedStream.OnRewind() fails silently after the read position exceeds the front-buffer length; PNG/BMP multi-pass codecs then receive zero bytes for subsequent passes and produce a partial bitmap.",
      "proposals": [
        {
          "title": "Copy stream to MemoryStream before decoding",
          "category": "workaround",
          "description": "Users should copy non-seekable streams to a MemoryStream (or byte array) before passing to SKBitmap.Decode. This routes through SKManagedStream (full seek support) and avoids the front-buffer limitation.",
          "effort": "cost/xs",
          "validated": "yes",
          "codeSnippet": "using var ms = new MemoryStream();\nawait nonSeekableStream.CopyToAsync(ms);\nms.Seek(0, SeekOrigin.Begin);\nusing var bitmap = SKBitmap.Decode(ms);"
        },
        {
          "title": "Buffer the full stream to memory when non-seekable in WrapManagedStream",
          "category": "fix",
          "description": "In SKCodec.WrapManagedStream, when the stream is not seekable, copy the entire stream to an SKDynamicMemoryWStream (or a MemoryStream) before wrapping, so the codec always receives a seekable stream. This trades memory for correctness.",
          "effort": "cost/s",
          "validated": "untested"
        }
      ],
      "recommendedProposal": "workaround-memorystream",
      "recommendedReason": "The workaround is already confirmed by the reporter and requires no library change. The fix proposal needs a repro to verify the root cause before implementation."
    }
  },
  "output": {
    "actionability": {
      "suggestedAction": "needs-investigation",
      "confidence": 0.82,
      "reason": "Regression claimed from 2.88.9 to 3.116.0 with a plausible code path (SKFrontBufferedManagedStream rewind failure). A minimal repro is needed to confirm before a fix is authored.",
      "suggestedReproPlatform": "linux"
    },
    "actions": [
      {
        "type": "update-labels",
        "description": "Apply classification labels",
        "risk": "low",
        "confidence": 0.95,
        "labels": [
          "type/bug",
          "area/SkiaSharp",
          "os/Linux",
          "os/Windows-Classic",
          "tenet/reliability"
        ]
      },
      {
        "type": "add-comment",
        "description": "Acknowledge the regression, explain the root cause hypothesis, provide copy-paste workaround, and request a minimal repro.",
        "risk": "medium",
        "confidence": 0.82,
        "comment": "Thanks for the report! This looks like a regression in how SkiaSharp handles **non-seekable streams** when decoding PNG/BMP images in 3.x.\n\n**Root cause hypothesis:**\nWhen a non-seekable stream is passed to `SKBitmap.Decode`, the library wraps it in `SKFrontBufferedManagedStream` which only buffers a small number of bytes for codec type detection. PNG and BMP decoders require multiple seek/rewind passes; once the read position exceeds the internal buffer, `OnRewind()` silently fails, and subsequent passes read zero bytes — yielding a correctly-dimensioned but partially-filled bitmap.\n\n**Workaround (confirmed):**\nCopy the stream to a `MemoryStream` (seekable) before decoding:\n\n```csharp\nusing var ms = new MemoryStream();\nawait nonSeekableStream.CopyToAsync(ms);\nms.Seek(0, SeekOrigin.Begin);\nusing var bitmap = SKBitmap.Decode(ms);\n```\n\nAlternatively, decode from a `byte[]`:\n\n```csharp\nvar bytes = await BinaryData.FromStreamAsync(nonSeekableStream);\nusing var bitmap = SKBitmap.Decode(bytes.ToArray());\n```\n\n**To help reproduce and fix this:**\nCould you provide a minimal console repro that does **not** require Azure Blob — for example, using a custom `Stream` that reads from a local file in fixed-size chunks and reports `CanSeek = false`? That would let us confirm the exact boundary condition and author a fix."
      }
    ]
  }
}
```

</details>
