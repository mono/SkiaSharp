# Issue Triage Report — #2342

| Field | Value |
|-------|-------|
| Repository | mono/SkiaSharp |
| Analyzed | 2026-05-04T07:20:00Z |
| Type | type/bug (0.90 (90%)) |
| Area | area/SkiaSharp (0.97 (97%)) |
| Suggested action | needs-investigation (0.85 (85%)) |

**Issue Summary:** SKBitmap.Decode(byte[]) throws ArgumentNullException when passed SKBitmap.Bytes because the raw pixel bytes are not an encoded image format and SKCodec.Create returns null.

**Analysis:** SKBitmap.Bytes returns raw decoded pixel data (not encoded image bytes). When passed to SKBitmap.Decode(byte[]), the raw pixel bytes are not a recognizable image format, so SKCodec.Create returns null. The subsequent Decode(SKCodec) overload throws ArgumentNullException(nameof(codec)) at line 436. The confusion stems from a naming mismatch: Bytes sounds like an encoded image export, but it is actually raw pixel memory.

**Recommendations:** **needs-investigation** — The root cause is clear from code inspection and a minimal repro is provided. Either the Decode(byte[]) path should handle a null codec gracefully (returning null), or documentation should clarify that Bytes is raw pixel data. A repro pass can confirm behavior and validate the proposed fix.

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

**Repository links:**
- https://github.com/mono/SkiaSharp/issues/1752 — Related ArgumentNullException issue in SKManagedStream (different root cause, same surface error)

**Code snippets:**

```csharp
using SkiaSharp.SKBitmap userImage = SkiaSharp.SKBitmap.Decode(@"C:\test.jpg");
using SkiaSharp.SKBitmap secondImage = SkiaSharp.SKBitmap.Decode(userImage.Bytes);
```

### Bug Signals

| Field | Value |
|-------|-------|
| Severity | medium |
| Regression claimed | False |
| Error type | exception |
| Error message | System.ArgumentNullException: Value cannot be null. Arg_ParamName_Name |
| Repro quality | complete |
| Target frameworks | — |

**Stack trace:**

```text
at SkiaSharp.SKBitmap.Decode(SKCodec codec)
   at SkiaSharp.SKBitmap.Decode(ReadOnlySpan`1 buffer)
   at SkiaSharp.SKBitmap.Decode(Byte[] buffer)
```

### Version Analysis

| Field | Value |
|-------|-------|
| Mentioned versions | 2.88.3 |
| Worked in | — |
| Broke in | — |
| Current relevance | likely |
| Relevance reason | Code path in SKBitmap.Decode(ReadOnlySpan<byte>) still calls SKCodec.Create which can return null; the explicit null guard then throws ArgumentNullException(nameof(codec)) at line 436-438. No fix has been applied. |

## Analysis

### Technical Summary

SKBitmap.Bytes returns raw decoded pixel data (not encoded image bytes). When passed to SKBitmap.Decode(byte[]), the raw pixel bytes are not a recognizable image format, so SKCodec.Create returns null. The subsequent Decode(SKCodec) overload throws ArgumentNullException(nameof(codec)) at line 436. The confusion stems from a naming mismatch: Bytes sounds like an encoded image export, but it is actually raw pixel memory.

### Rationale

This is a type/bug because the API is misleading: SKBitmap.Bytes sounds like image bytes but returns raw pixels, and the resulting ArgumentNullException gives no actionable guidance. The error is real but arises from a documentation/API design gap, not a logic error per se. Severity is medium because a workaround exists (use SKImage.Encode) and it does not cause crashes or data loss beyond the exception.

### Key Signals

- "The byte array has been created by SkiaSharp." — **issue body** (Reporter assumes SKBitmap.Bytes produces re-decodable encoded bytes; in fact it returns raw pixel data.)
- "Value cannot be null. Arg_ParamName_Name … bei SkiaSharp.SKBitmap.Decode(SKCodec codec)" — **exception** (SKCodec.Create returned null because raw pixel bytes are not a valid encoded image format; the null-guard then throws on codec parameter.)

### Code Investigation

| File | Lines | Relevance | Finding |
|------|-------|-----------|---------|
| `binding/SkiaSharp/SKBitmap.cs` | 320-326 | direct | SKBitmap.Bytes getter returns GetPixelSpan().ToArray() — raw unencoded pixel data in whatever color format the bitmap uses. |
| `binding/SkiaSharp/SKBitmap.cs` | 565-577 | direct | Decode(byte[]) delegates to Decode(ReadOnlySpan<byte>) which creates SKData from the buffer and calls SKCodec.Create(skdata). If the buffer is not valid encoded image bytes (e.g. raw pixels), SKCodec.Create returns null. |
| `binding/SkiaSharp/SKBitmap.cs` | 434-438 | direct | Decode(SKCodec codec) explicitly throws ArgumentNullException(nameof(codec)) when codec is null, producing the confusing error that points to 'codec' rather than explaining the real issue. |

### Workarounds

- To get re-encodable bytes, encode the bitmap first: using var image = SKImage.FromBitmap(bitmap); using var data = image.Encode(SKEncodedImageFormat.Png, 100); byte[] bytes = data.ToArray(); — then pass bytes to SKBitmap.Decode(bytes).
- Alternatively, use SKImage.FromBitmap + SKImage.EncodedData to produce format-aware encoded bytes.

### Next Questions

- Should SKBitmap.Bytes be deprecated or have its documentation clarified to warn that it returns raw pixel data?
- Should Decode(byte[]) be updated to return null (instead of throwing) when SKCodec.Create returns null, to give callers a clearer signal?

### Resolution Proposals

**Hypothesis:** The bug is a combination of a misleading property name (Bytes = raw pixels) and a confusing internal null guard that throws ArgumentNullException(codec) rather than a user-visible message explaining that the buffer is not an encoded image.

1. **Encode bitmap before passing bytes to Decode** — workaround, cost/xs, validated=yes
   - Use SKImage.FromBitmap + Encode to get a valid encoded byte array.

```csharp
using var userImage = SKBitmap.Decode(@"C:\test.jpg");
using var skImage = SKImage.FromBitmap(userImage);
using var encoded = skImage.Encode(SKEncodedImageFormat.Jpeg, 90);
byte[] encodedBytes = encoded.ToArray();
using var secondImage = SKBitmap.Decode(encodedBytes);
```
2. **Improve error handling: return null instead of throwing when codec is null** — fix, cost/xs, validated=untested
   - In Decode(ReadOnlySpan<byte>), if SKCodec.Create returns null, return null rather than passing null into Decode(SKCodec) which throws an ArgumentNullException with a confusing message.

**Recommended proposal:** workaround-1

**Why:** The workaround is immediately usable and correct. The fix (fix-1) is low-effort and would improve error feedback, but requires a code change.

## Recommendations

### Actionability

| Field | Value |
|-------|-------|
| Suggested action | needs-investigation |
| Confidence | 0.85 (85%) |
| Reason | The root cause is clear from code inspection and a minimal repro is provided. Either the Decode(byte[]) path should handle a null codec gracefully (returning null), or documentation should clarify that Bytes is raw pixel data. A repro pass can confirm behavior and validate the proposed fix. |
| Suggested repro platform | linux |

### Automatable Actions

| Type | Risk | Confidence | Description | Details |
|------|------|------------|-------------|---------|
| update-labels | low | 0.95 (95%) | Apply type/bug, area/SkiaSharp, tenet/reliability | labels=type/bug, area/SkiaSharp, tenet/reliability |
| add-comment | medium | 0.90 (90%) | Explain root cause and provide workaround | — |

**Comment draft for `add-comment`:**

```markdown
Thanks for the report! The issue here is that `SKBitmap.Bytes` returns **raw unencoded pixel data** — not an encoded image format like JPEG or PNG. When you pass those raw bytes to `SKBitmap.Decode()`, the codec cannot recognize them as a valid image, `SKCodec.Create` returns `null`, and the internal null-guard throws `ArgumentNullException`.

To round-trip a bitmap through a byte array, you need to encode it first:

```csharp
using var userImage = SKBitmap.Decode(@"C:\test.jpg");

// Encode to JPEG bytes
using var skImage = SKImage.FromBitmap(userImage);
using var encoded = skImage.Encode(SKEncodedImageFormat.Jpeg, 90);
byte[] encodedBytes = encoded.ToArray();

// Now Decode works
using var secondImage = SKBitmap.Decode(encodedBytes);
```

We'll also look at improving the error message so it's clearer when `Decode` receives unrecognized bytes.
```

<details>
<summary>Raw JSON</summary>

```json
{
  "meta": {
    "schemaVersion": "1.0",
    "number": 2342,
    "repo": "mono/SkiaSharp",
    "analyzedAt": "2026-05-04T07:20:00Z"
  },
  "summary": "SKBitmap.Decode(byte[]) throws ArgumentNullException when passed SKBitmap.Bytes because the raw pixel bytes are not an encoded image format and SKCodec.Create returns null.",
  "classification": {
    "type": {
      "value": "type/bug",
      "confidence": 0.9
    },
    "area": {
      "value": "area/SkiaSharp",
      "confidence": 0.97
    },
    "tenets": [
      "tenet/reliability"
    ]
  },
  "evidence": {
    "bugSignals": {
      "severity": "medium",
      "regressionClaimed": false,
      "errorType": "exception",
      "errorMessage": "System.ArgumentNullException: Value cannot be null. Arg_ParamName_Name",
      "stackTrace": "at SkiaSharp.SKBitmap.Decode(SKCodec codec)\n   at SkiaSharp.SKBitmap.Decode(ReadOnlySpan`1 buffer)\n   at SkiaSharp.SKBitmap.Decode(Byte[] buffer)",
      "reproQuality": "complete",
      "targetFrameworks": []
    },
    "reproEvidence": {
      "codeSnippets": [
        "using SkiaSharp.SKBitmap userImage = SkiaSharp.SKBitmap.Decode(@\"C:\\test.jpg\");\nusing SkiaSharp.SKBitmap secondImage = SkiaSharp.SKBitmap.Decode(userImage.Bytes);"
      ],
      "repoLinks": [
        {
          "url": "https://github.com/mono/SkiaSharp/issues/1752",
          "description": "Related ArgumentNullException issue in SKManagedStream (different root cause, same surface error)"
        }
      ]
    },
    "versionAnalysis": {
      "mentionedVersions": [
        "2.88.3"
      ],
      "currentRelevance": "likely",
      "relevanceReason": "Code path in SKBitmap.Decode(ReadOnlySpan<byte>) still calls SKCodec.Create which can return null; the explicit null guard then throws ArgumentNullException(nameof(codec)) at line 436-438. No fix has been applied."
    }
  },
  "analysis": {
    "summary": "SKBitmap.Bytes returns raw decoded pixel data (not encoded image bytes). When passed to SKBitmap.Decode(byte[]), the raw pixel bytes are not a recognizable image format, so SKCodec.Create returns null. The subsequent Decode(SKCodec) overload throws ArgumentNullException(nameof(codec)) at line 436. The confusion stems from a naming mismatch: Bytes sounds like an encoded image export, but it is actually raw pixel memory.",
    "codeInvestigation": [
      {
        "file": "binding/SkiaSharp/SKBitmap.cs",
        "lines": "320-326",
        "finding": "SKBitmap.Bytes getter returns GetPixelSpan().ToArray() — raw unencoded pixel data in whatever color format the bitmap uses.",
        "relevance": "direct"
      },
      {
        "file": "binding/SkiaSharp/SKBitmap.cs",
        "lines": "565-577",
        "finding": "Decode(byte[]) delegates to Decode(ReadOnlySpan<byte>) which creates SKData from the buffer and calls SKCodec.Create(skdata). If the buffer is not valid encoded image bytes (e.g. raw pixels), SKCodec.Create returns null.",
        "relevance": "direct"
      },
      {
        "file": "binding/SkiaSharp/SKBitmap.cs",
        "lines": "434-438",
        "finding": "Decode(SKCodec codec) explicitly throws ArgumentNullException(nameof(codec)) when codec is null, producing the confusing error that points to 'codec' rather than explaining the real issue.",
        "relevance": "direct"
      }
    ],
    "keySignals": [
      {
        "text": "The byte array has been created by SkiaSharp.",
        "source": "issue body",
        "interpretation": "Reporter assumes SKBitmap.Bytes produces re-decodable encoded bytes; in fact it returns raw pixel data."
      },
      {
        "text": "Value cannot be null. Arg_ParamName_Name … bei SkiaSharp.SKBitmap.Decode(SKCodec codec)",
        "source": "exception",
        "interpretation": "SKCodec.Create returned null because raw pixel bytes are not a valid encoded image format; the null-guard then throws on codec parameter."
      }
    ],
    "rationale": "This is a type/bug because the API is misleading: SKBitmap.Bytes sounds like image bytes but returns raw pixels, and the resulting ArgumentNullException gives no actionable guidance. The error is real but arises from a documentation/API design gap, not a logic error per se. Severity is medium because a workaround exists (use SKImage.Encode) and it does not cause crashes or data loss beyond the exception.",
    "workarounds": [
      "To get re-encodable bytes, encode the bitmap first: using var image = SKImage.FromBitmap(bitmap); using var data = image.Encode(SKEncodedImageFormat.Png, 100); byte[] bytes = data.ToArray(); — then pass bytes to SKBitmap.Decode(bytes).",
      "Alternatively, use SKImage.FromBitmap + SKImage.EncodedData to produce format-aware encoded bytes."
    ],
    "nextQuestions": [
      "Should SKBitmap.Bytes be deprecated or have its documentation clarified to warn that it returns raw pixel data?",
      "Should Decode(byte[]) be updated to return null (instead of throwing) when SKCodec.Create returns null, to give callers a clearer signal?"
    ],
    "resolution": {
      "hypothesis": "The bug is a combination of a misleading property name (Bytes = raw pixels) and a confusing internal null guard that throws ArgumentNullException(codec) rather than a user-visible message explaining that the buffer is not an encoded image.",
      "proposals": [
        {
          "title": "Encode bitmap before passing bytes to Decode",
          "category": "workaround",
          "effort": "cost/xs",
          "description": "Use SKImage.FromBitmap + Encode to get a valid encoded byte array.",
          "codeSnippet": "using var userImage = SKBitmap.Decode(@\"C:\\test.jpg\");\nusing var skImage = SKImage.FromBitmap(userImage);\nusing var encoded = skImage.Encode(SKEncodedImageFormat.Jpeg, 90);\nbyte[] encodedBytes = encoded.ToArray();\nusing var secondImage = SKBitmap.Decode(encodedBytes);",
          "validated": "yes"
        },
        {
          "title": "Improve error handling: return null instead of throwing when codec is null",
          "category": "fix",
          "effort": "cost/xs",
          "description": "In Decode(ReadOnlySpan<byte>), if SKCodec.Create returns null, return null rather than passing null into Decode(SKCodec) which throws an ArgumentNullException with a confusing message.",
          "validated": "untested"
        }
      ],
      "recommendedProposal": "workaround-1",
      "recommendedReason": "The workaround is immediately usable and correct. The fix (fix-1) is low-effort and would improve error feedback, but requires a code change."
    }
  },
  "output": {
    "actionability": {
      "suggestedAction": "needs-investigation",
      "confidence": 0.85,
      "reason": "The root cause is clear from code inspection and a minimal repro is provided. Either the Decode(byte[]) path should handle a null codec gracefully (returning null), or documentation should clarify that Bytes is raw pixel data. A repro pass can confirm behavior and validate the proposed fix.",
      "suggestedReproPlatform": "linux"
    },
    "actions": [
      {
        "type": "update-labels",
        "description": "Apply type/bug, area/SkiaSharp, tenet/reliability",
        "risk": "low",
        "confidence": 0.95,
        "labels": [
          "type/bug",
          "area/SkiaSharp",
          "tenet/reliability"
        ]
      },
      {
        "type": "add-comment",
        "description": "Explain root cause and provide workaround",
        "risk": "medium",
        "confidence": 0.9,
        "comment": "Thanks for the report! The issue here is that `SKBitmap.Bytes` returns **raw unencoded pixel data** — not an encoded image format like JPEG or PNG. When you pass those raw bytes to `SKBitmap.Decode()`, the codec cannot recognize them as a valid image, `SKCodec.Create` returns `null`, and the internal null-guard throws `ArgumentNullException`.\n\nTo round-trip a bitmap through a byte array, you need to encode it first:\n\n```csharp\nusing var userImage = SKBitmap.Decode(@\"C:\\test.jpg\");\n\n// Encode to JPEG bytes\nusing var skImage = SKImage.FromBitmap(userImage);\nusing var encoded = skImage.Encode(SKEncodedImageFormat.Jpeg, 90);\nbyte[] encodedBytes = encoded.ToArray();\n\n// Now Decode works\nusing var secondImage = SKBitmap.Decode(encodedBytes);\n```\n\nWe'll also look at improving the error message so it's clearer when `Decode` receives unrecognized bytes."
      }
    ]
  }
}
```

</details>
