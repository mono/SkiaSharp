# Issue Triage Report — #2249

| Field | Value |
|-------|-------|
| Repository | mono/SkiaSharp |
| Analyzed | 2026-04-22T17:33:25Z |
| Type | type/bug (0.97 (97%)) |
| Area | area/SkiaSharp (0.93 (93%)) |
| Suggested action | ready-to-fix (0.82 (82%)) |

**Issue Summary:** SKBitmap.Decode(Stream) produces corrupted output when the stream is a non-seekable network stream (e.g., HttpWebResponse.GetResponseStream()), due to SKManagedStream not looping to fill the read buffer and an incorrect isAsEnd condition that fires prematurely on partial reads.

**Analysis:** SKManagedStream.OnReadManagedStream uses a single Stream.Read() call with no fill-loop. Network streams (non-seekable) legally return fewer bytes than requested (partial reads) on every call. When this happens, Skia's codec receives partial PNG data and renders a corrupted image. Additionally, the isAsEnd sentinel is set when len <= size (always true for any non-empty read on a non-seekable stream), which can cause Skia to prematurely signal end-of-stream. The confirmed workaround—copying to MemoryStream first—bypasses both issues since MemoryStream fills the full requested buffer.

**Recommendations:** **ready-to-fix** — Root cause is identified (missing fill-loop and incorrect isAsEnd condition in SKManagedStream.OnReadManagedStream). Fix path is clear and low-risk. Confirmed workaround validates the diagnosis.

---

## Classification

| Field | Value |
|-------|-------|
| Type | type/bug |
| Area | area/SkiaSharp |
| Platforms | os/Windows-Classic |
| Backends | backend/Raster |
| Tenets | tenet/reliability |
| Partner | — |

## Evidence

### Reproduction

1. Create an HttpWebRequest to a PNG URL (e.g., https://tile.openstreetmap.org/1/0/0.png)
2. Obtain the response stream via GetResponseStream()
3. Pass the stream directly to SKBitmap.Decode(stream)
4. Encode result to PNG and save to file
5. Observe that saved image is partially rendered/corrupted

**Environment:** Windows Console, .NET Framework 4.8 and .NET 6, SkiaSharp 2.88.2–2.88.3

**Repository links:**
- https://github.com/mono/SkiaSharp/issues/1981 — Related PNG decode corruption issue (fixed for local streams in 2.88.1, zlib SSE bug)

**Code snippets:**

```csharp
using var webResponse = await request.GetResponseAsync();
await using var input = webResponse.GetResponseStream();
var bitmap = SKBitmap.Decode(input);
```

### Bug Signals

| Field | Value |
|-------|-------|
| Severity | medium |
| Regression claimed | True |
| Error type | wrong-output |
| Error message | Decoded PNG image is partially rendered / corrupted when loaded from network stream |
| Repro quality | partial |
| Target frameworks | net48, net6.0 |

### Version Analysis

| Field | Value |
|-------|-------|
| Mentioned versions | 2.80.2, 2.88.2, 2.88.3 |
| Worked in | 2.80.2 |
| Broke in | 2.88.2 |
| Current relevance | likely |
| Relevance reason | SKManagedStream.OnReadManagedStream still uses a single Stream.Read() call without a fill loop; the isAsEnd condition remains len <= size which is always true for non-empty reads on non-seekable streams. |

### Regression

| Field | Value |
|-------|-------|
| Is regression | True |
| Confidence | 0.85 (85%) |
| Reason | Reporter explicitly states 2.80.2 worked; 2.88.2 and 2.88.3 both exhibit corruption for the same URL and code. Maintainer confirmed a related PNG bug was fixed for local streams (zlib) but network stream path was not addressed. |
| Worked in version | 2.80.2 |
| Broke in version | 2.88.2 |

## Analysis

### Technical Summary

SKManagedStream.OnReadManagedStream uses a single Stream.Read() call with no fill-loop. Network streams (non-seekable) legally return fewer bytes than requested (partial reads) on every call. When this happens, Skia's codec receives partial PNG data and renders a corrupted image. Additionally, the isAsEnd sentinel is set when len <= size (always true for any non-empty read on a non-seekable stream), which can cause Skia to prematurely signal end-of-stream. The confirmed workaround—copying to MemoryStream first—bypasses both issues since MemoryStream fills the full requested buffer.

### Rationale

This is clearly a regression bug. The code path for non-seekable streams (SKFrontBufferedManagedStream → SKManagedStream) has a deficiency: Stream.Read() partial reads are not retried. The isAsEnd condition using <= instead of < looks like a typo-level bug that makes every non-empty read from a non-seekable stream appear to be the end of stream. The workaround—copy to MemoryStream before decoding—is confirmed effective by multiple reporters.

### Key Signals

- "This issue happens randomly, and a workaround is to copy the network stream to MemoryStream and then feed the SKBitmap." — **issue comment (reporter, 2022-11-01)** (Confirms partial-read root cause: when full data arrives in one Read() call it works; when chunked it corrupts. MemoryStream always fills the buffer.)
- "It works fine in 2.80.2, not working in the current one 2.88.2" — **issue body** (Regression between 2.80.2 and 2.88.2; related PNG bug fix for local streams (#1981) did not address network streams.)
- "I see the same issue in the latest version, 2.88.3. Do you mean the problem of #1981? It looks to me the case for the local stream is fixed, but not for the web stream." — **issue comment (reporter, 2022-10-16)** (Confirms issue #1981's local-stream fix did NOT address network streams, suggesting a different root cause.)
- "It works if I load the same image from a file." — **issue comment (reporter)** (File streams are seekable (CanSeek=true) → go through SKManagedStream without isAsEnd tracking; network streams are non-seekable → hit the partial-read and isAsEnd bug.)

### Code Investigation

| File | Lines | Relevance | Finding |
|------|-------|-----------|---------|
| `binding/SkiaSharp/SKManagedStream.cs` | 73-96 | direct | OnReadManagedStream calls stream.Read() once without a fill loop. For a non-seekable stream (e.g., NetworkStream), Stream.Read() may return fewer bytes than requested. The method returns the partial count to Skia, which then receives incomplete PNG chunk data. Additionally, the isAsEnd sentinel is set when `len <= (int)size` — since len is always <= size for any read, this condition is always true after the first non-empty read on a non-seekable stream, prematurely marking the stream as exhausted. |
| `binding/SkiaSharp/SKCodec.cs` | 278-288 | direct | WrapManagedStream branches on stream.CanSeek: seekable streams → SKManagedStream; non-seekable streams → SKFrontBufferedManagedStream(stream, MinBufferedBytesNeeded). Network streams are non-seekable, so they go through SKFrontBufferedManagedStream which itself delegates reads back to SKManagedStream for data beyond the front-buffer. Both layers are affected by the partial-read problem. |
| `binding/SkiaSharp/SKFrontBufferedManagedStream.cs` | 64-124 | related | OnRead phase 3 (reading past the front buffer) calls stream.Read(buffer, size) on SKManagedStream without looping. Any partial read here returns a short byte count, leaving `size > 0` unreported. This confirms partial reads propagate to Skia even past the initial buffer phase. |

### Workarounds

- Copy the network stream to a MemoryStream before decoding: `using var ms = new MemoryStream(); await input.CopyToAsync(ms); ms.Position = 0; var bitmap = SKBitmap.Decode(ms);`
- Read all bytes into a byte array first: `var bytes = await new HttpClient().GetByteArrayAsync(url); var bitmap = SKBitmap.Decode(bytes);`

### Next Questions

- Does the same corruption occur with SKImage.FromEncodedData(stream) / SKData.Create(stream) for non-seekable streams?
- Is the regression from 2.80.2→2.88.2 due to the SKManagedStream rewrite, or the SKFrontBufferedManagedStream introduction for non-seekable streams?
- Does the isAsEnd <= condition cause consistent failure or intermittent failure depending on timing of partial reads?

### Resolution Proposals

**Hypothesis:** SKManagedStream.OnReadManagedStream must loop until the buffer is filled or the underlying stream returns 0 bytes (true EOF). The isAsEnd condition should use strict less-than (`len < (int)size`) to only signal end-of-stream when a partial read is detected.

1. **Fix OnReadManagedStream to loop on partial reads** — fix, confidence 0.85 (85%), cost/s, validated=untested
   - Replace the single Stream.Read() call in OnReadManagedStream with a fill loop that retries until the requested bytes are read or the stream returns 0. Also correct the isAsEnd condition from `len <= (int)size` to `len < (int)size`.

```csharp
// In SKManagedStream.OnReadManagedStream:
using var managedBuffer = Utils.RentArray<byte>((int)size);
int totalRead = 0;
int remaining = (int)size;
while (remaining > 0) {
    var chunk = stream.Read(managedBuffer.Array, totalRead, remaining);
    if (chunk == 0) break; // true EOF
    totalRead += chunk;
    remaining -= chunk;
}
if (buffer != IntPtr.Zero) {
    var src = managedBuffer.Span.Slice(0, totalRead);
    var dst = buffer.AsSpan(managedBuffer.Length);
    src.CopyTo(dst);
}
if (!stream.CanSeek && (int)size > 0 && totalRead < (int)size)
    isAsEnd = true;
return (IntPtr)totalRead;
```
2. **Workaround: copy to MemoryStream before decoding** — workaround, confidence 0.99 (99%), cost/xs, validated=yes
   - Copy the network stream to a MemoryStream before passing to SKBitmap.Decode. MemoryStream is seekable and always satisfies full reads, bypassing the partial-read issue entirely.

```csharp
using var ms = new MemoryStream();
await networkStream.CopyToAsync(ms);
ms.Position = 0;
var bitmap = SKBitmap.Decode(ms);
```

**Recommended proposal:** Fix OnReadManagedStream to loop on partial reads

**Why:** The workaround requires callers to know about the issue and add boilerplate. The fix is low-risk, small-scope change in one method that correctly implements .NET Stream contract semantics.

## Recommendations

### Actionability

| Field | Value |
|-------|-------|
| Suggested action | ready-to-fix |
| Confidence | 0.82 (82%) |
| Reason | Root cause is identified (missing fill-loop and incorrect isAsEnd condition in SKManagedStream.OnReadManagedStream). Fix path is clear and low-risk. Confirmed workaround validates the diagnosis. |
| Suggested repro platform | linux |

### Automatable Actions

| Type | Risk | Confidence | Description | Details |
|------|------|------------|-------------|---------|
| update-labels | low | 0.97 (97%) | Apply bug, core, regression, and reliability labels | labels=type/bug, area/SkiaSharp, os/Windows-Classic, backend/Raster, tenet/reliability |
| add-comment | medium | 0.85 (85%) | Post analysis with root cause and confirmed workaround | — |

**Comment draft for `add-comment`:**

```markdown
Thanks for the detailed report and the workaround confirmation! After investigating the code, the root cause appears to be in `SKManagedStream.OnReadManagedStream` (binding/SkiaSharp/SKManagedStream.cs).

**Root Cause**

`Stream.Read()` is not guaranteed to fill the entire requested buffer — it can return fewer bytes than requested (a *partial read*). This is expected and documented .NET behavior, especially for network streams where data arrives in TCP packets. The current code calls `Stream.Read()` once and returns whatever partial result it gets to Skia's native codec. The codec then receives an incomplete PNG chunk and produces a corrupted image.

Additionally, the `isAsEnd` sentinel is set when `len <= (int)size`, but since `len` can never exceed `size`, this condition fires after *every* non-empty read from a non-seekable stream, which may cause the codec to signal end-of-stream prematurely.

The fix in #1981 addressed a zlib SSE decoding bug for local streams; this issue is a separate code path for non-seekable/network streams.

**Confirmed Workaround**

Copy the network stream to a `MemoryStream` before decoding — `MemoryStream` is seekable and always satisfies full buffer reads:

```csharp
using var ms = new MemoryStream();
await networkStream.CopyToAsync(ms);
ms.Position = 0;
var bitmap = SKBitmap.Decode(ms);
```

Alternatively, use `HttpClient.GetByteArrayAsync` and pass the byte array directly to `SKBitmap.Decode(byte[])`.
```

<details>
<summary>Raw JSON</summary>

```json
{
  "meta": {
    "schemaVersion": "1.0",
    "number": 2249,
    "repo": "mono/SkiaSharp",
    "analyzedAt": "2026-04-22T17:33:25Z"
  },
  "summary": "SKBitmap.Decode(Stream) produces corrupted output when the stream is a non-seekable network stream (e.g., HttpWebResponse.GetResponseStream()), due to SKManagedStream not looping to fill the read buffer and an incorrect isAsEnd condition that fires prematurely on partial reads.",
  "classification": {
    "type": {
      "value": "type/bug",
      "confidence": 0.97
    },
    "area": {
      "value": "area/SkiaSharp",
      "confidence": 0.93
    },
    "platforms": [
      "os/Windows-Classic"
    ],
    "backends": [
      "backend/Raster"
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
      "errorMessage": "Decoded PNG image is partially rendered / corrupted when loaded from network stream",
      "reproQuality": "partial",
      "targetFrameworks": [
        "net48",
        "net6.0"
      ]
    },
    "reproEvidence": {
      "stepsToReproduce": [
        "Create an HttpWebRequest to a PNG URL (e.g., https://tile.openstreetmap.org/1/0/0.png)",
        "Obtain the response stream via GetResponseStream()",
        "Pass the stream directly to SKBitmap.Decode(stream)",
        "Encode result to PNG and save to file",
        "Observe that saved image is partially rendered/corrupted"
      ],
      "environmentDetails": "Windows Console, .NET Framework 4.8 and .NET 6, SkiaSharp 2.88.2–2.88.3",
      "repoLinks": [
        {
          "url": "https://github.com/mono/SkiaSharp/issues/1981",
          "description": "Related PNG decode corruption issue (fixed for local streams in 2.88.1, zlib SSE bug)"
        }
      ],
      "codeSnippets": [
        "using var webResponse = await request.GetResponseAsync();\nawait using var input = webResponse.GetResponseStream();\nvar bitmap = SKBitmap.Decode(input);"
      ]
    },
    "versionAnalysis": {
      "mentionedVersions": [
        "2.80.2",
        "2.88.2",
        "2.88.3"
      ],
      "workedIn": "2.80.2",
      "brokeIn": "2.88.2",
      "currentRelevance": "likely",
      "relevanceReason": "SKManagedStream.OnReadManagedStream still uses a single Stream.Read() call without a fill loop; the isAsEnd condition remains len <= size which is always true for non-empty reads on non-seekable streams."
    },
    "regression": {
      "isRegression": true,
      "confidence": 0.85,
      "reason": "Reporter explicitly states 2.80.2 worked; 2.88.2 and 2.88.3 both exhibit corruption for the same URL and code. Maintainer confirmed a related PNG bug was fixed for local streams (zlib) but network stream path was not addressed.",
      "workedInVersion": "2.80.2",
      "brokeInVersion": "2.88.2"
    }
  },
  "analysis": {
    "summary": "SKManagedStream.OnReadManagedStream uses a single Stream.Read() call with no fill-loop. Network streams (non-seekable) legally return fewer bytes than requested (partial reads) on every call. When this happens, Skia's codec receives partial PNG data and renders a corrupted image. Additionally, the isAsEnd sentinel is set when len <= size (always true for any non-empty read on a non-seekable stream), which can cause Skia to prematurely signal end-of-stream. The confirmed workaround—copying to MemoryStream first—bypasses both issues since MemoryStream fills the full requested buffer.",
    "rationale": "This is clearly a regression bug. The code path for non-seekable streams (SKFrontBufferedManagedStream → SKManagedStream) has a deficiency: Stream.Read() partial reads are not retried. The isAsEnd condition using <= instead of < looks like a typo-level bug that makes every non-empty read from a non-seekable stream appear to be the end of stream. The workaround—copy to MemoryStream before decoding—is confirmed effective by multiple reporters.",
    "keySignals": [
      {
        "text": "This issue happens randomly, and a workaround is to copy the network stream to MemoryStream and then feed the SKBitmap.",
        "source": "issue comment (reporter, 2022-11-01)",
        "interpretation": "Confirms partial-read root cause: when full data arrives in one Read() call it works; when chunked it corrupts. MemoryStream always fills the buffer."
      },
      {
        "text": "It works fine in 2.80.2, not working in the current one 2.88.2",
        "source": "issue body",
        "interpretation": "Regression between 2.80.2 and 2.88.2; related PNG bug fix for local streams (#1981) did not address network streams."
      },
      {
        "text": "I see the same issue in the latest version, 2.88.3. Do you mean the problem of #1981? It looks to me the case for the local stream is fixed, but not for the web stream.",
        "source": "issue comment (reporter, 2022-10-16)",
        "interpretation": "Confirms issue #1981's local-stream fix did NOT address network streams, suggesting a different root cause."
      },
      {
        "text": "It works if I load the same image from a file.",
        "source": "issue comment (reporter)",
        "interpretation": "File streams are seekable (CanSeek=true) → go through SKManagedStream without isAsEnd tracking; network streams are non-seekable → hit the partial-read and isAsEnd bug."
      }
    ],
    "codeInvestigation": [
      {
        "file": "binding/SkiaSharp/SKManagedStream.cs",
        "lines": "73-96",
        "finding": "OnReadManagedStream calls stream.Read() once without a fill loop. For a non-seekable stream (e.g., NetworkStream), Stream.Read() may return fewer bytes than requested. The method returns the partial count to Skia, which then receives incomplete PNG chunk data. Additionally, the isAsEnd sentinel is set when `len <= (int)size` — since len is always <= size for any read, this condition is always true after the first non-empty read on a non-seekable stream, prematurely marking the stream as exhausted.",
        "relevance": "direct"
      },
      {
        "file": "binding/SkiaSharp/SKCodec.cs",
        "lines": "278-288",
        "finding": "WrapManagedStream branches on stream.CanSeek: seekable streams → SKManagedStream; non-seekable streams → SKFrontBufferedManagedStream(stream, MinBufferedBytesNeeded). Network streams are non-seekable, so they go through SKFrontBufferedManagedStream which itself delegates reads back to SKManagedStream for data beyond the front-buffer. Both layers are affected by the partial-read problem.",
        "relevance": "direct"
      },
      {
        "file": "binding/SkiaSharp/SKFrontBufferedManagedStream.cs",
        "lines": "64-124",
        "finding": "OnRead phase 3 (reading past the front buffer) calls stream.Read(buffer, size) on SKManagedStream without looping. Any partial read here returns a short byte count, leaving `size > 0` unreported. This confirms partial reads propagate to Skia even past the initial buffer phase.",
        "relevance": "related"
      }
    ],
    "workarounds": [
      "Copy the network stream to a MemoryStream before decoding: `using var ms = new MemoryStream(); await input.CopyToAsync(ms); ms.Position = 0; var bitmap = SKBitmap.Decode(ms);`",
      "Read all bytes into a byte array first: `var bytes = await new HttpClient().GetByteArrayAsync(url); var bitmap = SKBitmap.Decode(bytes);`"
    ],
    "nextQuestions": [
      "Does the same corruption occur with SKImage.FromEncodedData(stream) / SKData.Create(stream) for non-seekable streams?",
      "Is the regression from 2.80.2→2.88.2 due to the SKManagedStream rewrite, or the SKFrontBufferedManagedStream introduction for non-seekable streams?",
      "Does the isAsEnd <= condition cause consistent failure or intermittent failure depending on timing of partial reads?"
    ],
    "resolution": {
      "hypothesis": "SKManagedStream.OnReadManagedStream must loop until the buffer is filled or the underlying stream returns 0 bytes (true EOF). The isAsEnd condition should use strict less-than (`len < (int)size`) to only signal end-of-stream when a partial read is detected.",
      "proposals": [
        {
          "title": "Fix OnReadManagedStream to loop on partial reads",
          "description": "Replace the single Stream.Read() call in OnReadManagedStream with a fill loop that retries until the requested bytes are read or the stream returns 0. Also correct the isAsEnd condition from `len <= (int)size` to `len < (int)size`.",
          "codeSnippet": "// In SKManagedStream.OnReadManagedStream:\nusing var managedBuffer = Utils.RentArray<byte>((int)size);\nint totalRead = 0;\nint remaining = (int)size;\nwhile (remaining > 0) {\n    var chunk = stream.Read(managedBuffer.Array, totalRead, remaining);\n    if (chunk == 0) break; // true EOF\n    totalRead += chunk;\n    remaining -= chunk;\n}\nif (buffer != IntPtr.Zero) {\n    var src = managedBuffer.Span.Slice(0, totalRead);\n    var dst = buffer.AsSpan(managedBuffer.Length);\n    src.CopyTo(dst);\n}\nif (!stream.CanSeek && (int)size > 0 && totalRead < (int)size)\n    isAsEnd = true;\nreturn (IntPtr)totalRead;",
          "category": "fix",
          "confidence": 0.85,
          "effort": "cost/s",
          "validated": "untested"
        },
        {
          "title": "Workaround: copy to MemoryStream before decoding",
          "description": "Copy the network stream to a MemoryStream before passing to SKBitmap.Decode. MemoryStream is seekable and always satisfies full reads, bypassing the partial-read issue entirely.",
          "codeSnippet": "using var ms = new MemoryStream();\nawait networkStream.CopyToAsync(ms);\nms.Position = 0;\nvar bitmap = SKBitmap.Decode(ms);",
          "category": "workaround",
          "confidence": 0.99,
          "effort": "cost/xs",
          "validated": "yes"
        }
      ],
      "recommendedProposal": "Fix OnReadManagedStream to loop on partial reads",
      "recommendedReason": "The workaround requires callers to know about the issue and add boilerplate. The fix is low-risk, small-scope change in one method that correctly implements .NET Stream contract semantics."
    }
  },
  "output": {
    "actionability": {
      "suggestedAction": "ready-to-fix",
      "confidence": 0.82,
      "reason": "Root cause is identified (missing fill-loop and incorrect isAsEnd condition in SKManagedStream.OnReadManagedStream). Fix path is clear and low-risk. Confirmed workaround validates the diagnosis.",
      "suggestedReproPlatform": "linux"
    },
    "actions": [
      {
        "type": "update-labels",
        "description": "Apply bug, core, regression, and reliability labels",
        "risk": "low",
        "confidence": 0.97,
        "labels": [
          "type/bug",
          "area/SkiaSharp",
          "os/Windows-Classic",
          "backend/Raster",
          "tenet/reliability"
        ]
      },
      {
        "type": "add-comment",
        "description": "Post analysis with root cause and confirmed workaround",
        "risk": "medium",
        "confidence": 0.85,
        "comment": "Thanks for the detailed report and the workaround confirmation! After investigating the code, the root cause appears to be in `SKManagedStream.OnReadManagedStream` (binding/SkiaSharp/SKManagedStream.cs).\n\n**Root Cause**\n\n`Stream.Read()` is not guaranteed to fill the entire requested buffer — it can return fewer bytes than requested (a *partial read*). This is expected and documented .NET behavior, especially for network streams where data arrives in TCP packets. The current code calls `Stream.Read()` once and returns whatever partial result it gets to Skia's native codec. The codec then receives an incomplete PNG chunk and produces a corrupted image.\n\nAdditionally, the `isAsEnd` sentinel is set when `len <= (int)size`, but since `len` can never exceed `size`, this condition fires after *every* non-empty read from a non-seekable stream, which may cause the codec to signal end-of-stream prematurely.\n\nThe fix in #1981 addressed a zlib SSE decoding bug for local streams; this issue is a separate code path for non-seekable/network streams.\n\n**Confirmed Workaround**\n\nCopy the network stream to a `MemoryStream` before decoding — `MemoryStream` is seekable and always satisfies full buffer reads:\n\n```csharp\nusing var ms = new MemoryStream();\nawait networkStream.CopyToAsync(ms);\nms.Position = 0;\nvar bitmap = SKBitmap.Decode(ms);\n```\n\nAlternatively, use `HttpClient.GetByteArrayAsync` and pass the byte array directly to `SKBitmap.Decode(byte[])`."
      }
    ]
  }
}
```

</details>
