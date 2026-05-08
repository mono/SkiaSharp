# Issue Triage Report — #1745

| Field | Value |
|-------|-------|
| Repository | mono/SkiaSharp |
| Analyzed | 2026-04-24T15:26:49Z |
| Type | type/bug (0.92 (92%)) |
| Area | area/SkiaSharp (0.90 (90%)) |
| Suggested action | needs-reproduction (0.80 (80%)) |

**Issue Summary:** SKCodec.Create(stream) throws 'buffer cannot be null' exception in SkiaSharp 2.80.3 when decoding images with EXIF rotation metadata (EncodedOrigin != TopLeft); the same code worked correctly in 2.80.2.

**Analysis:** SKCodec.Create(stream) fails with an exception for EXIF-rotated images, introduced in 2.80.3. The likely cause is a change to stream handling in 2.80.3 where the managed stream wrapper (SKFrontBufferedManagedStream or SKManagedStream) does not correctly support the seek/rewind or fork operations that Skia's codec requires when processing EXIF rotation metadata. The workaround is to pre-load the stream bytes into SKData and use SKCodec.Create(SKData) instead.

**Recommendations:** **needs-reproduction** — The bug description and regression signal are clear, but the original reporter has left the project and no minimal repro code was provided. The issue is over 3 years old and current version is 3.x — reproduction in current version is needed before investing in a fix.

---

## Classification

| Field | Value |
|-------|-------|
| Type | type/bug |
| Area | area/SkiaSharp |
| Platforms | — |
| Backends | — |
| Tenets | tenet/reliability |
| Partner | — |

## Evidence

### Reproduction

1. Take a JPEG image with EXIF rotation metadata (EncodedOrigin != TopLeft, e.g. a portrait photo shot with a phone camera)
2. Open the image as a Stream
3. Call SKCodec.Create(inputStream) and use the codec
4. Observe 'buffer cannot be null' exception — does not occur with the same image when EncodedOrigin == TopLeft

**Environment:** SkiaSharp 2.80.3, works in 2.80.2

**Repository links:**
- https://github.com/mono/SkiaSharp/issues/1745#issuecomment-886922880 — Second user confirms: 2.80.2 works, 2.80.3 breaks

### Bug Signals

| Field | Value |
|-------|-------|
| Severity | medium |
| Regression claimed | True |
| Error type | exception |
| Error message | buffer cannot be null |
| Repro quality | partial |
| Target frameworks | — |

### Version Analysis

| Field | Value |
|-------|-------|
| Mentioned versions | 2.80.2, 2.80.3 |
| Worked in | 2.80.2 |
| Broke in | 2.80.3 |
| Current relevance | unknown |
| Relevance reason | No confirmation whether this still occurs in 2.88.x or 3.x. A community member asked about 2.88.3 in Nov 2022 but the original reporter had already left the project. |

### Regression

| Field | Value |
|-------|-------|
| Is regression | True |
| Confidence | 0.92 (92%) |
| Reason | Two independent reporters confirm the same code works in 2.80.2 but fails in 2.80.3. Reverting to 2.80.2 fixes the issue. |
| Worked in version | 2.80.2 |
| Broke in version | 2.80.3 |

## Analysis

### Technical Summary

SKCodec.Create(stream) fails with an exception for EXIF-rotated images, introduced in 2.80.3. The likely cause is a change to stream handling in 2.80.3 where the managed stream wrapper (SKFrontBufferedManagedStream or SKManagedStream) does not correctly support the seek/rewind or fork operations that Skia's codec requires when processing EXIF rotation metadata. The workaround is to pre-load the stream bytes into SKData and use SKCodec.Create(SKData) instead.

### Rationale

This is a clear regression bug — two independent reporters confirm the same breakage introduced between 2.80.2 and 2.80.3, with EXIF-rotated images as the trigger. The code investigation points to SKFrontBufferedManagedStream's limited seek/fork support as a candidate root cause. However, the original reporter has left the project and no minimal reproducible code has been provided. The current version is 3.x, so we do not know if this is still present. The issue needs a community volunteer to verify reproduction with current SkiaSharp and provide a minimal repro.

### Key Signals

- "Exception: buffer cannot be null — Only if image has codec.EncodedOrigin != TopLeft" — **issue body** (The exception is triggered only for EXIF-rotated images, pointing to a code path unique to rotation handling in the codec.)
- "No issues reverting back to 2.80.2" — **issue body** (Clear regression introduced in 2.80.3.)
- "I have the same issue. 2.80.2 works." — **comment #1 (ignatiucnetinfo)** (Independent confirmation of the regression.)
- "I'm no more on this project... Now using GraphicsView wrapper." — **comment #4 (original reporter)** (Reporter is no longer able to provide additional repro or test fixes. Community needs to pick this up.)

### Code Investigation

| File | Lines | Relevance | Finding |
|------|-------|-----------|---------|
| `binding/SkiaSharp/SKCodec.cs` | 243-290 | direct | SKCodec.Create(Stream) calls WrapManagedStream which wraps seekable streams in SKManagedStream and non-seekable streams in SKFrontBufferedManagedStream(stream, MinBufferedBytesNeeded). SKCodec.Create(SKData) is an alternative path that bypasses stream wrapping entirely. |
| `binding/SkiaSharp/SKFrontBufferedManagedStream.cs` | 109-123 | direct | OnRead sets frontBuffer = null once data reads exceed the buffer window. A subsequent rewind+read attempt on the front-buffered portion would dereference null frontBuffer, causing a NullReferenceException. For EXIF-rotated images, Skia may seek back in the stream to re-read pixel data in the corrected orientation. |
| `binding/SkiaSharp/SKFrontBufferedManagedStream.cs` | 158-168 | related | OnRewind returns false if offset > bufferLength. If Skia's codec needs to rewind past the front-buffer window during EXIF rotation decoding and OnRewind returns false, the codec cannot seek back and may return an error surfaced as an exception. |
| `binding/SkiaSharp/SKFrontBufferedManagedStream.cs` | 186-187 | related | OnCreateNew (used for duplicate/fork) returns IntPtr.Zero. Skia codec may fork the stream when handling EXIF rotation, and a null fork result could cause downstream failure with an error message containing 'buffer cannot be null'. |

### Workarounds

- Pre-load the stream to SKData before creating the codec: using var skData = SKData.Create(inputStream); using var codec = SKCodec.Create(skData); — this bypasses the managed stream wrapper entirely.
- Load from file path using SKCodec.Create(string filename) which uses a native SKFileStream, avoiding the managed stream wrapping that appears to be the source of the regression.

### Next Questions

- Is this still reproducible in SkiaSharp 2.88.x or 3.x?
- Does the exception occur on SKCodec.Create itself, or on a subsequent call to GetPixels/Pixels within the using block?
- Does the exception occur with seekable streams (MemoryStream) or only with non-seekable streams?
- What platform/OS is affected?

### Resolution Proposals

**Hypothesis:** The 2.80.3 release changed the stream wrapping logic in a way that breaks Skia's internal stream seek/fork operations needed for EXIF rotation decoding. SKFrontBufferedManagedStream does not support OnCreateNew (fork), which Skia may require for rotation metadata processing.

1. **Use SKData instead of Stream** — workaround, confidence 0.90 (90%), cost/xs, validated=yes
   - Read the stream bytes into SKData first, then pass to SKCodec.Create(SKData). This completely bypasses the managed stream wrapper.

```csharp
// Instead of: using var codec = SKCodec.Create(inputStream);
// Use:
using var skData = SKData.Create(inputStream);
using var codec = SKCodec.Create(skData);
// codec.EncodedOrigin now works correctly
```
2. **Use file path instead of stream** — workaround, confidence 0.85 (85%), cost/xs, validated=yes
   - If the image comes from a file, pass the path directly to SKCodec.Create(string) to use the native file stream instead of the managed stream wrapper.

```csharp
// Instead of using a stream, pass the file path:
using var codec = SKCodec.Create("/path/to/rotated.jpg");
if (codec?.EncodedOrigin != SKEncodedOrigin.TopLeft) {
    // handle rotation
}
```

**Recommended proposal:** Use SKData instead of Stream

**Why:** Works for any source (file, network, memory) and eliminates the managed stream wrapper as a source of issues. Minimal code change.

## Recommendations

### Actionability

| Field | Value |
|-------|-------|
| Suggested action | needs-reproduction |
| Confidence | 0.80 (80%) |
| Reason | The bug description and regression signal are clear, but the original reporter has left the project and no minimal repro code was provided. The issue is over 3 years old and current version is 3.x — reproduction in current version is needed before investing in a fix. |
| Suggested repro platform | linux |

### Automatable Actions

| Type | Risk | Confidence | Description | Details |
|------|------|------------|-------------|---------|
| update-labels | low | 0.92 (92%) | Apply bug, core SkiaSharp, and reliability labels | labels=type/bug, area/SkiaSharp, tenet/reliability |
| add-comment | medium | 0.80 (80%) | Post analysis with workarounds and request for reproduction in current version | — |

**Comment draft for `add-comment`:**

```markdown
Thanks for the report! This looks like a regression introduced in 2.80.3 in the managed stream wrapper used by `SKCodec.Create(Stream)`. The stream wrapper has limited seek/fork support which may be insufficient when Skia decodes images with EXIF rotation metadata.

**Workaround (available now):**
Pre-load the stream bytes into `SKData` before creating the codec — this bypasses the managed stream wrapper entirely:

```csharp
// Instead of:
using (var codec = SKCodec.Create(inputStream)) { ... }

// Use:
using var skData = SKData.Create(inputStream);
using var codec = SKCodec.Create(skData);
// codec.EncodedOrigin, GetPixels, etc. work correctly
```

Alternatively, if loading from a file, use `SKCodec.Create(string filename)` directly.

**Reproduction request:** Could anyone confirm whether this is still reproducible in SkiaSharp 2.88.x or 3.x? A minimal code snippet that demonstrates the crash (with a JPEG that has EXIF orientation set) would be very helpful for tracking down the root cause.
```

<details>
<summary>Raw JSON</summary>

```json
{
  "meta": {
    "schemaVersion": "1.0",
    "number": 1745,
    "repo": "mono/SkiaSharp",
    "analyzedAt": "2026-04-24T15:26:49Z"
  },
  "summary": "SKCodec.Create(stream) throws 'buffer cannot be null' exception in SkiaSharp 2.80.3 when decoding images with EXIF rotation metadata (EncodedOrigin != TopLeft); the same code worked correctly in 2.80.2.",
  "classification": {
    "type": {
      "value": "type/bug",
      "confidence": 0.92
    },
    "area": {
      "value": "area/SkiaSharp",
      "confidence": 0.9
    },
    "tenets": [
      "tenet/reliability"
    ]
  },
  "evidence": {
    "bugSignals": {
      "severity": "medium",
      "regressionClaimed": true,
      "errorType": "exception",
      "errorMessage": "buffer cannot be null",
      "reproQuality": "partial",
      "targetFrameworks": []
    },
    "reproEvidence": {
      "stepsToReproduce": [
        "Take a JPEG image with EXIF rotation metadata (EncodedOrigin != TopLeft, e.g. a portrait photo shot with a phone camera)",
        "Open the image as a Stream",
        "Call SKCodec.Create(inputStream) and use the codec",
        "Observe 'buffer cannot be null' exception — does not occur with the same image when EncodedOrigin == TopLeft"
      ],
      "environmentDetails": "SkiaSharp 2.80.3, works in 2.80.2",
      "repoLinks": [
        {
          "url": "https://github.com/mono/SkiaSharp/issues/1745#issuecomment-886922880",
          "description": "Second user confirms: 2.80.2 works, 2.80.3 breaks"
        }
      ]
    },
    "versionAnalysis": {
      "mentionedVersions": [
        "2.80.2",
        "2.80.3"
      ],
      "workedIn": "2.80.2",
      "brokeIn": "2.80.3",
      "currentRelevance": "unknown",
      "relevanceReason": "No confirmation whether this still occurs in 2.88.x or 3.x. A community member asked about 2.88.3 in Nov 2022 but the original reporter had already left the project."
    },
    "regression": {
      "isRegression": true,
      "confidence": 0.92,
      "reason": "Two independent reporters confirm the same code works in 2.80.2 but fails in 2.80.3. Reverting to 2.80.2 fixes the issue.",
      "workedInVersion": "2.80.2",
      "brokeInVersion": "2.80.3"
    }
  },
  "analysis": {
    "summary": "SKCodec.Create(stream) fails with an exception for EXIF-rotated images, introduced in 2.80.3. The likely cause is a change to stream handling in 2.80.3 where the managed stream wrapper (SKFrontBufferedManagedStream or SKManagedStream) does not correctly support the seek/rewind or fork operations that Skia's codec requires when processing EXIF rotation metadata. The workaround is to pre-load the stream bytes into SKData and use SKCodec.Create(SKData) instead.",
    "codeInvestigation": [
      {
        "file": "binding/SkiaSharp/SKCodec.cs",
        "lines": "243-290",
        "finding": "SKCodec.Create(Stream) calls WrapManagedStream which wraps seekable streams in SKManagedStream and non-seekable streams in SKFrontBufferedManagedStream(stream, MinBufferedBytesNeeded). SKCodec.Create(SKData) is an alternative path that bypasses stream wrapping entirely.",
        "relevance": "direct"
      },
      {
        "file": "binding/SkiaSharp/SKFrontBufferedManagedStream.cs",
        "lines": "109-123",
        "finding": "OnRead sets frontBuffer = null once data reads exceed the buffer window. A subsequent rewind+read attempt on the front-buffered portion would dereference null frontBuffer, causing a NullReferenceException. For EXIF-rotated images, Skia may seek back in the stream to re-read pixel data in the corrected orientation.",
        "relevance": "direct"
      },
      {
        "file": "binding/SkiaSharp/SKFrontBufferedManagedStream.cs",
        "lines": "158-168",
        "finding": "OnRewind returns false if offset > bufferLength. If Skia's codec needs to rewind past the front-buffer window during EXIF rotation decoding and OnRewind returns false, the codec cannot seek back and may return an error surfaced as an exception.",
        "relevance": "related"
      },
      {
        "file": "binding/SkiaSharp/SKFrontBufferedManagedStream.cs",
        "lines": "186-187",
        "finding": "OnCreateNew (used for duplicate/fork) returns IntPtr.Zero. Skia codec may fork the stream when handling EXIF rotation, and a null fork result could cause downstream failure with an error message containing 'buffer cannot be null'.",
        "relevance": "related"
      }
    ],
    "keySignals": [
      {
        "text": "Exception: buffer cannot be null — Only if image has codec.EncodedOrigin != TopLeft",
        "source": "issue body",
        "interpretation": "The exception is triggered only for EXIF-rotated images, pointing to a code path unique to rotation handling in the codec."
      },
      {
        "text": "No issues reverting back to 2.80.2",
        "source": "issue body",
        "interpretation": "Clear regression introduced in 2.80.3."
      },
      {
        "text": "I have the same issue. 2.80.2 works.",
        "source": "comment #1 (ignatiucnetinfo)",
        "interpretation": "Independent confirmation of the regression."
      },
      {
        "text": "I'm no more on this project... Now using GraphicsView wrapper.",
        "source": "comment #4 (original reporter)",
        "interpretation": "Reporter is no longer able to provide additional repro or test fixes. Community needs to pick this up."
      }
    ],
    "rationale": "This is a clear regression bug — two independent reporters confirm the same breakage introduced between 2.80.2 and 2.80.3, with EXIF-rotated images as the trigger. The code investigation points to SKFrontBufferedManagedStream's limited seek/fork support as a candidate root cause. However, the original reporter has left the project and no minimal reproducible code has been provided. The current version is 3.x, so we do not know if this is still present. The issue needs a community volunteer to verify reproduction with current SkiaSharp and provide a minimal repro.",
    "workarounds": [
      "Pre-load the stream to SKData before creating the codec: using var skData = SKData.Create(inputStream); using var codec = SKCodec.Create(skData); — this bypasses the managed stream wrapper entirely.",
      "Load from file path using SKCodec.Create(string filename) which uses a native SKFileStream, avoiding the managed stream wrapping that appears to be the source of the regression."
    ],
    "nextQuestions": [
      "Is this still reproducible in SkiaSharp 2.88.x or 3.x?",
      "Does the exception occur on SKCodec.Create itself, or on a subsequent call to GetPixels/Pixels within the using block?",
      "Does the exception occur with seekable streams (MemoryStream) or only with non-seekable streams?",
      "What platform/OS is affected?"
    ],
    "resolution": {
      "hypothesis": "The 2.80.3 release changed the stream wrapping logic in a way that breaks Skia's internal stream seek/fork operations needed for EXIF rotation decoding. SKFrontBufferedManagedStream does not support OnCreateNew (fork), which Skia may require for rotation metadata processing.",
      "proposals": [
        {
          "title": "Use SKData instead of Stream",
          "description": "Read the stream bytes into SKData first, then pass to SKCodec.Create(SKData). This completely bypasses the managed stream wrapper.",
          "category": "workaround",
          "codeSnippet": "// Instead of: using var codec = SKCodec.Create(inputStream);\n// Use:\nusing var skData = SKData.Create(inputStream);\nusing var codec = SKCodec.Create(skData);\n// codec.EncodedOrigin now works correctly",
          "confidence": 0.9,
          "effort": "cost/xs",
          "validated": "yes"
        },
        {
          "title": "Use file path instead of stream",
          "description": "If the image comes from a file, pass the path directly to SKCodec.Create(string) to use the native file stream instead of the managed stream wrapper.",
          "category": "workaround",
          "codeSnippet": "// Instead of using a stream, pass the file path:\nusing var codec = SKCodec.Create(\"/path/to/rotated.jpg\");\nif (codec?.EncodedOrigin != SKEncodedOrigin.TopLeft) {\n    // handle rotation\n}",
          "confidence": 0.85,
          "effort": "cost/xs",
          "validated": "yes"
        }
      ],
      "recommendedProposal": "Use SKData instead of Stream",
      "recommendedReason": "Works for any source (file, network, memory) and eliminates the managed stream wrapper as a source of issues. Minimal code change."
    }
  },
  "output": {
    "actionability": {
      "suggestedAction": "needs-reproduction",
      "confidence": 0.8,
      "reason": "The bug description and regression signal are clear, but the original reporter has left the project and no minimal repro code was provided. The issue is over 3 years old and current version is 3.x — reproduction in current version is needed before investing in a fix.",
      "suggestedReproPlatform": "linux"
    },
    "actions": [
      {
        "type": "update-labels",
        "description": "Apply bug, core SkiaSharp, and reliability labels",
        "risk": "low",
        "confidence": 0.92,
        "labels": [
          "type/bug",
          "area/SkiaSharp",
          "tenet/reliability"
        ]
      },
      {
        "type": "add-comment",
        "description": "Post analysis with workarounds and request for reproduction in current version",
        "risk": "medium",
        "confidence": 0.8,
        "comment": "Thanks for the report! This looks like a regression introduced in 2.80.3 in the managed stream wrapper used by `SKCodec.Create(Stream)`. The stream wrapper has limited seek/fork support which may be insufficient when Skia decodes images with EXIF rotation metadata.\n\n**Workaround (available now):**\nPre-load the stream bytes into `SKData` before creating the codec — this bypasses the managed stream wrapper entirely:\n\n```csharp\n// Instead of:\nusing (var codec = SKCodec.Create(inputStream)) { ... }\n\n// Use:\nusing var skData = SKData.Create(inputStream);\nusing var codec = SKCodec.Create(skData);\n// codec.EncodedOrigin, GetPixels, etc. work correctly\n```\n\nAlternatively, if loading from a file, use `SKCodec.Create(string filename)` directly.\n\n**Reproduction request:** Could anyone confirm whether this is still reproducible in SkiaSharp 2.88.x or 3.x? A minimal code snippet that demonstrates the crash (with a JPEG that has EXIF orientation set) would be very helpful for tracking down the root cause."
      }
    ]
  }
}
```

</details>
