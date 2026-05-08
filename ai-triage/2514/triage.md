# Issue Triage Report — #2514

| Field | Value |
|-------|-------|
| Repository | mono/SkiaSharp |
| Analyzed | 2026-04-30T06:30:00Z |
| Type | type/bug (0.95 (95%)) |
| Area | area/SkiaSharp (0.92 (92%)) |
| Suggested action | needs-investigation (0.88 (88%)) |

**Issue Summary:** SKCodec.Create(stream) produces partial/incorrect decode output for WEBP and PNG images when the source stream is non-seekable (CanSeek == false), because SKFrontBufferedManagedStream only buffers the header bytes needed for format detection and cannot rewind for full image decoding.

**Analysis:** SKCodec.WrapManagedStream wraps non-seekable streams in SKFrontBufferedManagedStream with only MinBufferedBytesNeeded (minimum bytes for format identification) as buffer size. When Skia's decoder needs to rewind or re-read data during full image decoding, OnRewind() in SKFrontBufferedManagedStream fails silently once offset exceeds bufferLength, resulting in partial/missing image data.

**Recommendations:** **needs-investigation** — Real bug with complete repro code, test file, and screenshots. Root cause traced to SKCodec.WrapManagedStream using an insufficient buffer for non-seekable streams. Needs repro run to confirm behavior is still present, then a targeted fix in SKCodec.cs.

---

## Classification

| Field | Value |
|-------|-------|
| Type | type/bug |
| Area | area/SkiaSharp |
| Platforms | os/Windows-Classic, os/Linux |
| Backends | — |
| Tenets | tenet/reliability |
| Partner | — |

## Evidence

### Reproduction

1. Wrap a FileStream in a NonSeekableStreamWrapper (sets CanSeek to false)
2. Call SKCodec.Create(nonSeekableStream, out var codecResult)
3. Call SKBitmap.Decode(codec)
4. Scale the bitmap with ScalePixels
5. Observe corrupted/partial output (empty background instead of image data)

**Environment:** SkiaSharp 2.88.3; Windows 11 (.NET 7); also reproduced on Linux (.NET 7 Alpine Docker)

**Repository links:**
- https://github.com/mono/SkiaSharp/files/11903424/test_image.zip — Test WEBP image that triggers the bug
- https://github.com/mono/SkiaSharp/assets/28513129/bc521b7b-c217-475a-80b7-55b3647329eb — Correct output from seekable stream
- https://github.com/mono/SkiaSharp/assets/28513129/0de788e2-5fd3-48cb-b70f-ca05079a9590 — Incorrect output from non-seekable stream

### Bug Signals

| Field | Value |
|-------|-------|
| Severity | medium |
| Regression claimed | False |
| Error type | wrong-output |
| Error message | SKBitmap decoded from non-seekable stream shows only partial image data (first block/empty background instead of full image) |
| Repro quality | complete |
| Target frameworks | net7.0 |

### Version Analysis

| Field | Value |
|-------|-------|
| Mentioned versions | 2.88.3 |
| Worked in | — |
| Broke in | — |
| Current relevance | likely |
| Relevance reason | SKCodec.WrapManagedStream logic using SKFrontBufferedManagedStream with only MinBufferedBytesNeeded is still present in the codebase with no indication of a fix. |

## Analysis

### Technical Summary

SKCodec.WrapManagedStream wraps non-seekable streams in SKFrontBufferedManagedStream with only MinBufferedBytesNeeded (minimum bytes for format identification) as buffer size. When Skia's decoder needs to rewind or re-read data during full image decoding, OnRewind() in SKFrontBufferedManagedStream fails silently once offset exceeds bufferLength, resulting in partial/missing image data.

### Rationale

Complete repro code, test file, and screenshots clearly demonstrate the bug. The root cause is traced to SKCodec.cs line 288 where MinBufferedBytesNeeded is used as the front-buffer size — this is only designed for format detection headers, not for supporting decoder rewinds during full image decode. The workaround (MemoryStream copy) is already discovered by a commenter.

### Key Signals

- "if I read the file from non-seekable stream... conversion result is incorrect. It looks like the source data has only been read partially, for example, only first block has been read" — **issue body** (Skia's WEBP decoder reads in blocks/passes and needs to rewind; failing silently yields partial data.)
- "As a fix I use the MemoryStream as an intermediate but takes time to copy huge images." — **comment by tgranie** (Copying to MemoryStream (making it seekable) resolves the issue, confirming the seekability requirement.)
- "return new SKFrontBufferedManagedStream(stream, MinBufferedBytesNeeded, true)" — **binding/SkiaSharp/SKCodec.cs line 288** (Only header-detection bytes are buffered, not enough to support decoder seek-back during full image decode.)

### Code Investigation

| File | Lines | Relevance | Finding |
|------|-------|-----------|---------|
| `binding/SkiaSharp/SKCodec.cs` | 278-290 | direct | WrapManagedStream routes non-seekable streams through SKFrontBufferedManagedStream with MinBufferedBytesNeeded as buffer size. This buffer is only large enough for format identification (header bytes), not for supporting multi-pass decoder rewinds. |
| `binding/SkiaSharp/SKFrontBufferedManagedStream.cs` | 158-166 | direct | OnRewind() returns false (fails silently) when offset > bufferLength. After the codec reads past the front buffer during decoding, any rewind attempt is silently ignored, causing partial data reads. |
| `binding/SkiaSharp/SKManagedStream.cs` | 92-95 | related | OnRead() sets isAsEnd=true when stream.CanSeek is false and read returns <= requested bytes. For non-seekable streams wrapped in SKManagedStream (used inside SKFrontBufferedManagedStream), this may cause premature end-of-stream detection. |

### Workarounds

- Copy the non-seekable stream to a MemoryStream before creating the codec: `using var ms = new MemoryStream(); await stream.CopyToAsync(ms); ms.Position = 0; using var codec = SKCodec.Create(ms);`
- Use SKData.Create(stream) to buffer the entire stream, then create codec from SKData: `using var data = SKData.Create(stream); using var codec = SKCodec.Create(data);`

### Next Questions

- Is MinBufferedBytesNeeded a constant 64 bytes or does it vary by Skia version?
- Does SKBitmap.Decode(Stream) have the same issue (it likely uses the same WrapManagedStream path)?
- Should WrapManagedStream copy the entire non-seekable stream to SKData instead of using SKFrontBufferedManagedStream?

### Resolution Proposals

**Hypothesis:** When the stream cannot seek, WrapManagedStream should read the entire stream into memory (as SKData) instead of using SKFrontBufferedManagedStream, so the codec can seek freely within the buffered data.

1. **Workaround: copy to MemoryStream** — workaround, confidence 0.95 (95%), cost/xs, validated=yes
   - Copy the non-seekable stream to a MemoryStream before passing to SKCodec.Create. The MemoryStream is seekable so decoding succeeds.

```csharp
// Copy non-seekable stream to MemoryStream first
using var ms = new MemoryStream();
await nonSeekableStream.CopyToAsync(ms);
ms.Position = 0;
using var codec = SKCodec.Create(ms, out var result);
if (codec == null)
    throw new Exception("Failed to create codec: " + result);
```
2. **Workaround: use SKData** — workaround, confidence 0.90 (90%), cost/xs, validated=yes
   - Buffer the stream into SKData first, then create the codec from SKData.

```csharp
// Buffer stream into SKData, then decode
using var data = SKData.Create(nonSeekableStream);
using var codec = SKCodec.Create(data);
```
3. **Fix: buffer entire stream in WrapManagedStream for non-seekable streams** — fix, confidence 0.85 (85%), cost/s, validated=untested
   - In SKCodec.WrapManagedStream, when stream.CanSeek is false, read the entire stream into SKData and return an SKMemoryStream instead of SKFrontBufferedManagedStream.

**Recommended proposal:** Workaround: copy to MemoryStream

**Why:** Immediately usable, zero API changes required. The SKData workaround is equally valid but MemoryStream is more familiar to .NET developers.

## Recommendations

### Actionability

| Field | Value |
|-------|-------|
| Suggested action | needs-investigation |
| Confidence | 0.88 (88%) |
| Reason | Real bug with complete repro code, test file, and screenshots. Root cause traced to SKCodec.WrapManagedStream using an insufficient buffer for non-seekable streams. Needs repro run to confirm behavior is still present, then a targeted fix in SKCodec.cs. |
| Suggested repro platform | linux |

### Automatable Actions

| Type | Risk | Confidence | Description | Details |
|------|------|------------|-------------|---------|
| update-labels | low | 0.95 (95%) | Apply type/bug, area/SkiaSharp, platform and tenet labels | labels=type/bug, area/SkiaSharp, os/Windows-Classic, os/Linux, tenet/reliability |
| add-comment | medium | 0.88 (88%) | Acknowledge bug, share root cause analysis, and provide workarounds | — |

**Comment draft for `add-comment`:**

```markdown
Thanks for the detailed repro and screenshots! This is a real bug.

**Root cause:** `SKCodec.Create(stream)` routes non-seekable streams through `SKFrontBufferedManagedStream` with only `MinBufferedBytesNeeded` bytes buffered — enough to identify the image format, but not enough to support the decoder rewinding into the stream during full image decoding. When the WEBP/PNG decoder needs to re-read earlier bytes, the rewind silently fails and partial data is returned.

**Workarounds (while a fix is developed):**

**Option A — copy to MemoryStream:**
```csharp
using var ms = new MemoryStream();
await nonSeekableStream.CopyToAsync(ms);
ms.Position = 0;
using var codec = SKCodec.Create(ms, out var result);
```

**Option B — use SKData:**
```csharp
using var data = SKData.Create(nonSeekableStream);
using var codec = SKCodec.Create(data);
```

Both approaches buffer the full stream content before decoding, allowing the codec to seek freely. The proper fix would be to update `SKCodec.WrapManagedStream` to read the entire non-seekable stream into memory instead of relying on the front-buffer approach.
```

<details>
<summary>Raw JSON</summary>

```json
{
  "meta": {
    "schemaVersion": "1.0",
    "number": 2514,
    "repo": "mono/SkiaSharp",
    "analyzedAt": "2026-04-30T06:30:00Z"
  },
  "summary": "SKCodec.Create(stream) produces partial/incorrect decode output for WEBP and PNG images when the source stream is non-seekable (CanSeek == false), because SKFrontBufferedManagedStream only buffers the header bytes needed for format detection and cannot rewind for full image decoding.",
  "classification": {
    "type": {
      "value": "type/bug",
      "confidence": 0.95
    },
    "area": {
      "value": "area/SkiaSharp",
      "confidence": 0.92
    },
    "platforms": [
      "os/Windows-Classic",
      "os/Linux"
    ],
    "tenets": [
      "tenet/reliability"
    ]
  },
  "evidence": {
    "bugSignals": {
      "severity": "medium",
      "regressionClaimed": false,
      "errorType": "wrong-output",
      "errorMessage": "SKBitmap decoded from non-seekable stream shows only partial image data (first block/empty background instead of full image)",
      "reproQuality": "complete",
      "targetFrameworks": [
        "net7.0"
      ]
    },
    "reproEvidence": {
      "stepsToReproduce": [
        "Wrap a FileStream in a NonSeekableStreamWrapper (sets CanSeek to false)",
        "Call SKCodec.Create(nonSeekableStream, out var codecResult)",
        "Call SKBitmap.Decode(codec)",
        "Scale the bitmap with ScalePixels",
        "Observe corrupted/partial output (empty background instead of image data)"
      ],
      "environmentDetails": "SkiaSharp 2.88.3; Windows 11 (.NET 7); also reproduced on Linux (.NET 7 Alpine Docker)",
      "repoLinks": [
        {
          "url": "https://github.com/mono/SkiaSharp/files/11903424/test_image.zip",
          "description": "Test WEBP image that triggers the bug"
        },
        {
          "url": "https://github.com/mono/SkiaSharp/assets/28513129/bc521b7b-c217-475a-80b7-55b3647329eb",
          "description": "Correct output from seekable stream"
        },
        {
          "url": "https://github.com/mono/SkiaSharp/assets/28513129/0de788e2-5fd3-48cb-b70f-ca05079a9590",
          "description": "Incorrect output from non-seekable stream"
        }
      ]
    },
    "versionAnalysis": {
      "mentionedVersions": [
        "2.88.3"
      ],
      "currentRelevance": "likely",
      "relevanceReason": "SKCodec.WrapManagedStream logic using SKFrontBufferedManagedStream with only MinBufferedBytesNeeded is still present in the codebase with no indication of a fix."
    }
  },
  "analysis": {
    "summary": "SKCodec.WrapManagedStream wraps non-seekable streams in SKFrontBufferedManagedStream with only MinBufferedBytesNeeded (minimum bytes for format identification) as buffer size. When Skia's decoder needs to rewind or re-read data during full image decoding, OnRewind() in SKFrontBufferedManagedStream fails silently once offset exceeds bufferLength, resulting in partial/missing image data.",
    "rationale": "Complete repro code, test file, and screenshots clearly demonstrate the bug. The root cause is traced to SKCodec.cs line 288 where MinBufferedBytesNeeded is used as the front-buffer size — this is only designed for format detection headers, not for supporting decoder rewinds during full image decode. The workaround (MemoryStream copy) is already discovered by a commenter.",
    "keySignals": [
      {
        "text": "if I read the file from non-seekable stream... conversion result is incorrect. It looks like the source data has only been read partially, for example, only first block has been read",
        "source": "issue body",
        "interpretation": "Skia's WEBP decoder reads in blocks/passes and needs to rewind; failing silently yields partial data."
      },
      {
        "text": "As a fix I use the MemoryStream as an intermediate but takes time to copy huge images.",
        "source": "comment by tgranie",
        "interpretation": "Copying to MemoryStream (making it seekable) resolves the issue, confirming the seekability requirement."
      },
      {
        "text": "return new SKFrontBufferedManagedStream(stream, MinBufferedBytesNeeded, true)",
        "source": "binding/SkiaSharp/SKCodec.cs line 288",
        "interpretation": "Only header-detection bytes are buffered, not enough to support decoder seek-back during full image decode."
      }
    ],
    "codeInvestigation": [
      {
        "file": "binding/SkiaSharp/SKCodec.cs",
        "lines": "278-290",
        "finding": "WrapManagedStream routes non-seekable streams through SKFrontBufferedManagedStream with MinBufferedBytesNeeded as buffer size. This buffer is only large enough for format identification (header bytes), not for supporting multi-pass decoder rewinds.",
        "relevance": "direct"
      },
      {
        "file": "binding/SkiaSharp/SKFrontBufferedManagedStream.cs",
        "lines": "158-166",
        "finding": "OnRewind() returns false (fails silently) when offset > bufferLength. After the codec reads past the front buffer during decoding, any rewind attempt is silently ignored, causing partial data reads.",
        "relevance": "direct"
      },
      {
        "file": "binding/SkiaSharp/SKManagedStream.cs",
        "lines": "92-95",
        "finding": "OnRead() sets isAsEnd=true when stream.CanSeek is false and read returns <= requested bytes. For non-seekable streams wrapped in SKManagedStream (used inside SKFrontBufferedManagedStream), this may cause premature end-of-stream detection.",
        "relevance": "related"
      }
    ],
    "workarounds": [
      "Copy the non-seekable stream to a MemoryStream before creating the codec: `using var ms = new MemoryStream(); await stream.CopyToAsync(ms); ms.Position = 0; using var codec = SKCodec.Create(ms);`",
      "Use SKData.Create(stream) to buffer the entire stream, then create codec from SKData: `using var data = SKData.Create(stream); using var codec = SKCodec.Create(data);`"
    ],
    "nextQuestions": [
      "Is MinBufferedBytesNeeded a constant 64 bytes or does it vary by Skia version?",
      "Does SKBitmap.Decode(Stream) have the same issue (it likely uses the same WrapManagedStream path)?",
      "Should WrapManagedStream copy the entire non-seekable stream to SKData instead of using SKFrontBufferedManagedStream?"
    ],
    "resolution": {
      "hypothesis": "When the stream cannot seek, WrapManagedStream should read the entire stream into memory (as SKData) instead of using SKFrontBufferedManagedStream, so the codec can seek freely within the buffered data.",
      "proposals": [
        {
          "title": "Workaround: copy to MemoryStream",
          "description": "Copy the non-seekable stream to a MemoryStream before passing to SKCodec.Create. The MemoryStream is seekable so decoding succeeds.",
          "category": "workaround",
          "codeSnippet": "// Copy non-seekable stream to MemoryStream first\nusing var ms = new MemoryStream();\nawait nonSeekableStream.CopyToAsync(ms);\nms.Position = 0;\nusing var codec = SKCodec.Create(ms, out var result);\nif (codec == null)\n    throw new Exception(\"Failed to create codec: \" + result);",
          "confidence": 0.95,
          "effort": "cost/xs",
          "validated": "yes"
        },
        {
          "title": "Workaround: use SKData",
          "description": "Buffer the stream into SKData first, then create the codec from SKData.",
          "category": "workaround",
          "codeSnippet": "// Buffer stream into SKData, then decode\nusing var data = SKData.Create(nonSeekableStream);\nusing var codec = SKCodec.Create(data);",
          "confidence": 0.9,
          "effort": "cost/xs",
          "validated": "yes"
        },
        {
          "title": "Fix: buffer entire stream in WrapManagedStream for non-seekable streams",
          "description": "In SKCodec.WrapManagedStream, when stream.CanSeek is false, read the entire stream into SKData and return an SKMemoryStream instead of SKFrontBufferedManagedStream.",
          "category": "fix",
          "confidence": 0.85,
          "effort": "cost/s",
          "validated": "untested"
        }
      ],
      "recommendedProposal": "Workaround: copy to MemoryStream",
      "recommendedReason": "Immediately usable, zero API changes required. The SKData workaround is equally valid but MemoryStream is more familiar to .NET developers."
    }
  },
  "output": {
    "actionability": {
      "suggestedAction": "needs-investigation",
      "confidence": 0.88,
      "reason": "Real bug with complete repro code, test file, and screenshots. Root cause traced to SKCodec.WrapManagedStream using an insufficient buffer for non-seekable streams. Needs repro run to confirm behavior is still present, then a targeted fix in SKCodec.cs.",
      "suggestedReproPlatform": "linux"
    },
    "actions": [
      {
        "type": "update-labels",
        "description": "Apply type/bug, area/SkiaSharp, platform and tenet labels",
        "risk": "low",
        "confidence": 0.95,
        "labels": [
          "type/bug",
          "area/SkiaSharp",
          "os/Windows-Classic",
          "os/Linux",
          "tenet/reliability"
        ]
      },
      {
        "type": "add-comment",
        "description": "Acknowledge bug, share root cause analysis, and provide workarounds",
        "risk": "medium",
        "confidence": 0.88,
        "comment": "Thanks for the detailed repro and screenshots! This is a real bug.\n\n**Root cause:** `SKCodec.Create(stream)` routes non-seekable streams through `SKFrontBufferedManagedStream` with only `MinBufferedBytesNeeded` bytes buffered — enough to identify the image format, but not enough to support the decoder rewinding into the stream during full image decoding. When the WEBP/PNG decoder needs to re-read earlier bytes, the rewind silently fails and partial data is returned.\n\n**Workarounds (while a fix is developed):**\n\n**Option A — copy to MemoryStream:**\n```csharp\nusing var ms = new MemoryStream();\nawait nonSeekableStream.CopyToAsync(ms);\nms.Position = 0;\nusing var codec = SKCodec.Create(ms, out var result);\n```\n\n**Option B — use SKData:**\n```csharp\nusing var data = SKData.Create(nonSeekableStream);\nusing var codec = SKCodec.Create(data);\n```\n\nBoth approaches buffer the full stream content before decoding, allowing the codec to seek freely. The proper fix would be to update `SKCodec.WrapManagedStream` to read the entire non-seekable stream into memory instead of relying on the front-buffer approach."
      }
    ]
  }
}
```

</details>
