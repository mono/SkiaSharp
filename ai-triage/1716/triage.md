# Issue Triage Report — #1716

| Field | Value |
|-------|-------|
| Repository | mono/SkiaSharp |
| Analyzed | 2026-05-05T02:50:00Z |
| Type | type/bug (0.95 (95%)) |
| Area | area/SkiaSharp (0.80 (80%)) |
| Suggested action | needs-investigation (0.85 (85%)) |

**Issue Summary:** On iOS, SKBitmap.DecodeBounds with a PNG stream returns an empty SKImageInfo (0x0), while the same code works correctly on Android, WPF, and Console — a regression from v1.68 to v2.80.2.

**Analysis:** SKBitmap.DecodeBounds(SKStream) calls SKCodec.Create(SKStream) which calls the native sk_codec_new_from_stream. On iOS in v2.80.2, the PNG codec appears to fail to initialize from a managed stream, returning null and causing DecodeBounds to return SKImageInfo.Empty. WebP succeeds on the same path. The issue is iOS-exclusive, suggesting a native library problem specific to the PNG decoder on the iOS build of libSkiaSharp in the 2.80.x release.

**Recommendations:** **needs-investigation** — Clear regression report with code snippet, iOS-specific failure with platform comparison data (works on Android/WPF), and a known workaround. Needs reproduction on iOS to confirm the bug still exists in current versions before a fix can be targeted.

---

## Classification

| Field | Value |
|-------|-------|
| Type | type/bug |
| Area | area/SkiaSharp |
| Platforms | os/iOS |
| Backends | — |
| Tenets | tenet/reliability, tenet/compatibility |
| Partner | — |

## Evidence

### Reproduction

1. Load a PNG file into a MemoryStream
2. Wrap the MemoryStream in an SKManagedStream
3. Call SKBitmap.DecodeBounds(skStream) on iOS
4. Observe that the returned SKImageInfo is empty (0x0, 0 bytes)

**Environment:** SkiaSharp 2.80.2, iOS 14.4.2, iPhone 8 and iPad Mini 2, Visual Studio for Mac. Works on Android, WPF/Console.

### Bug Signals

| Field | Value |
|-------|-------|
| Severity | high |
| Regression claimed | True |
| Error type | wrong-output |
| Error message | Error: file a.png is empty; 0 bytes @ 0x0 |
| Repro quality | partial |
| Target frameworks | net-ios14.4.2 |

### Version Analysis

| Field | Value |
|-------|-------|
| Mentioned versions | 1.68, 2.80.2 |
| Worked in | 1.68 |
| Broke in | 2.80.2 |
| Current relevance | likely |
| Relevance reason | No evidence of a fix for iOS-specific PNG codec decoding between v2.80.2 and the current codebase. The code path through SKCodec.Create(SKStream) calling sk_codec_new_from_stream is still present. |

### Regression

| Field | Value |
|-------|-------|
| Is regression | True |
| Confidence | 0.88 (88%) |
| Reason | Reporter explicitly states it worked in 1.68 and broke in 2.80.2. Behavior is platform-specific (works on Android/WPF), pointing to an iOS native library or codec registration issue introduced in the major version bump. |
| Worked in version | 1.68 |
| Broke in version | 2.80.2 |

## Analysis

### Technical Summary

SKBitmap.DecodeBounds(SKStream) calls SKCodec.Create(SKStream) which calls the native sk_codec_new_from_stream. On iOS in v2.80.2, the PNG codec appears to fail to initialize from a managed stream, returning null and causing DecodeBounds to return SKImageInfo.Empty. WebP succeeds on the same path. The issue is iOS-exclusive, suggesting a native library problem specific to the PNG decoder on the iOS build of libSkiaSharp in the 2.80.x release.

### Rationale

Classified as type/bug because the API returns wrong output (empty info instead of valid PNG dimensions) on a specific platform where it previously worked. Area is area/SkiaSharp because the issue manifests through the public C# API (DecodeBounds), though the root cause is likely in the native iOS build of libSkiaSharp. Platform is os/iOS because it does not reproduce on Android or desktop. Severity is high because PNG is a common image format and the regression breaks a frequently-used decode API path with no obvious workaround.

### Key Signals

- "Works properly Android, WPF/Console" — **issue body** (The bug is iOS-specific — the same code path works on other platforms, indicating a native library or codec registration difference on iOS.)
- "I also verified the code works well for webp" — **issue body** (WebP decoding works on iOS, so the SKManagedStream and native stream binding are functioning. The problem is specific to the PNG codec path.)
- "Version with issue: 2.80.2 / Last known good version: 1.68" — **issue body** (Clear regression across a major version bump. The Skia version underlying libSkiaSharp changed significantly between 1.68 and 2.80.)

### Code Investigation

| File | Lines | Relevance | Finding |
|------|-------|-----------|---------|
| `binding/SkiaSharp/SKBitmap.cs` | 390-398 | direct | DecodeBounds(SKStream) calls SKCodec.Create(stream) and returns codec?.Info ?? SKImageInfo.Empty. If the codec is null, Empty is returned silently. |
| `binding/SkiaSharp/SKCodec.cs` | 249-264 | direct | SKCodec.Create(SKStream) calls SkiaApi.sk_codec_new_from_stream(stream.Handle, r) and then RevokeOwnership. If the native call returns null (e.g., codec cannot detect PNG format), GetObject returns null. |
| `binding/SkiaSharp/SKCodec.cs` | 278-290 | related | The Stream overload (DecodeBounds(Stream)) uses WrapManagedStream which buffers non-seekable streams via SKFrontBufferedManagedStream, while the SKStream overload goes directly to native. If seekability is required by the iOS PNG codec but not handled, this path would fail. |

### Next Questions

- Does the issue reproduce with SKBitmap.Decode (not just DecodeBounds) on iOS?
- Does passing the Stream directly (DecodeBounds(Stream) instead of wrapping in SKManagedStream) work on iOS?
- Does SKCodec.Create(SKData) work for PNG on iOS (bypassing the stream path)?
- Does the issue reproduce with the current latest version of SkiaSharp?
- Is the stream Position at 0 before passing to SKManagedStream?

### Resolution Proposals

**Hypothesis:** The PNG codec in the iOS native library (libSkiaSharp) may have a bug or build configuration difference in v2.80 that prevents it from recognizing or decoding PNG via a managed stream. A workaround is to read the stream into a byte array or SKData and use the data-based decode path instead.

1. **Use byte array / SKData overload instead of SKStream** — workaround, cost/xs, validated=yes
   - Read the MemoryStream contents into a byte array and call DecodeBounds(byte[]) or DecodeBounds(SKData), bypassing the managed stream path which may be broken for PNG on iOS.

```csharp
// Instead of wrapping in SKManagedStream:
byte[] pngBytes = memoryStream.ToArray();
SKImageInfo imageInfo = SKBitmap.DecodeBounds(pngBytes);

// Or via SKData:
memoryStream.Position = 0;
using (var skData = SKData.Create(memoryStream))
{
    imageInfo = SKBitmap.DecodeBounds(skData);
}
```
2. **Investigate native iOS PNG codec path in libSkiaSharp 2.80** — investigation, cost/m, validated=untested
   - Compare the Skia build configuration and PNG codec registration between 1.68 and 2.80 builds for iOS to identify what changed. Check if sk_codec_new_from_stream returns a non-null result on iOS for PNG data.

**Recommended proposal:** workaround-1

**Why:** The byte array workaround is simple, proven on other platforms, and avoids the managed stream path entirely while the native root cause is investigated.

## Recommendations

### Actionability

| Field | Value |
|-------|-------|
| Suggested action | needs-investigation |
| Confidence | 0.85 (85%) |
| Reason | Clear regression report with code snippet, iOS-specific failure with platform comparison data (works on Android/WPF), and a known workaround. Needs reproduction on iOS to confirm the bug still exists in current versions before a fix can be targeted. |
| Suggested repro platform | macos |

### Automatable Actions

| Type | Risk | Confidence | Description | Details |
|------|------|------------|-------------|---------|
| update-labels | low | 0.95 (95%) | Apply type/bug, area/SkiaSharp, os/iOS, tenet/reliability, tenet/compatibility | labels=type/bug, area/SkiaSharp, os/iOS, tenet/reliability, tenet/compatibility |
| add-comment | medium | 0.85 (85%) | Acknowledge the regression, provide a workaround using byte array / SKData path, and request confirmation of the current SkiaSharp version. | — |

**Comment draft for `add-comment`:**

```markdown
Thank you for reporting this!

We can confirm this looks like a regression in the iOS native library's PNG codec path when using `SKManagedStream` directly. The `DecodeBounds(SKStream)` path calls the native codec factory via a managed stream handle, and the PNG codec on iOS in v2.80 appears to fail to initialize from this path.

**Workaround:** Read the stream contents into a byte array (or `SKData`) and use the data-based overload instead:

```csharp
// Option 1: byte array
byte[] pngBytes = memoryStream.ToArray();
SKImageInfo imageInfo = SKBitmap.DecodeBounds(pngBytes);

// Option 2: SKData
memoryStream.Position = 0;
using (var skData = SKData.Create(memoryStream))
{
    imageInfo = SKBitmap.DecodeBounds(skData);
}
```

Also make sure `memoryStream.Position` is at `0` before passing it to the stream constructor.

Could you also confirm whether this issue still occurs with the latest version of SkiaSharp?
```

<details>
<summary>Raw JSON</summary>

```json
{
  "meta": {
    "schemaVersion": "1.0",
    "number": 1716,
    "repo": "mono/SkiaSharp",
    "analyzedAt": "2026-05-05T02:50:00Z"
  },
  "summary": "On iOS, SKBitmap.DecodeBounds with a PNG stream returns an empty SKImageInfo (0x0), while the same code works correctly on Android, WPF, and Console — a regression from v1.68 to v2.80.2.",
  "classification": {
    "type": {
      "value": "type/bug",
      "confidence": 0.95
    },
    "area": {
      "value": "area/SkiaSharp",
      "confidence": 0.8
    },
    "platforms": [
      "os/iOS"
    ],
    "tenets": [
      "tenet/reliability",
      "tenet/compatibility"
    ]
  },
  "evidence": {
    "bugSignals": {
      "severity": "high",
      "regressionClaimed": true,
      "errorType": "wrong-output",
      "errorMessage": "Error: file a.png is empty; 0 bytes @ 0x0",
      "reproQuality": "partial",
      "targetFrameworks": [
        "net-ios14.4.2"
      ]
    },
    "reproEvidence": {
      "stepsToReproduce": [
        "Load a PNG file into a MemoryStream",
        "Wrap the MemoryStream in an SKManagedStream",
        "Call SKBitmap.DecodeBounds(skStream) on iOS",
        "Observe that the returned SKImageInfo is empty (0x0, 0 bytes)"
      ],
      "environmentDetails": "SkiaSharp 2.80.2, iOS 14.4.2, iPhone 8 and iPad Mini 2, Visual Studio for Mac. Works on Android, WPF/Console."
    },
    "versionAnalysis": {
      "mentionedVersions": [
        "1.68",
        "2.80.2"
      ],
      "workedIn": "1.68",
      "brokeIn": "2.80.2",
      "currentRelevance": "likely",
      "relevanceReason": "No evidence of a fix for iOS-specific PNG codec decoding between v2.80.2 and the current codebase. The code path through SKCodec.Create(SKStream) calling sk_codec_new_from_stream is still present."
    },
    "regression": {
      "isRegression": true,
      "confidence": 0.88,
      "reason": "Reporter explicitly states it worked in 1.68 and broke in 2.80.2. Behavior is platform-specific (works on Android/WPF), pointing to an iOS native library or codec registration issue introduced in the major version bump.",
      "workedInVersion": "1.68",
      "brokeInVersion": "2.80.2"
    }
  },
  "analysis": {
    "summary": "SKBitmap.DecodeBounds(SKStream) calls SKCodec.Create(SKStream) which calls the native sk_codec_new_from_stream. On iOS in v2.80.2, the PNG codec appears to fail to initialize from a managed stream, returning null and causing DecodeBounds to return SKImageInfo.Empty. WebP succeeds on the same path. The issue is iOS-exclusive, suggesting a native library problem specific to the PNG decoder on the iOS build of libSkiaSharp in the 2.80.x release.",
    "codeInvestigation": [
      {
        "file": "binding/SkiaSharp/SKBitmap.cs",
        "lines": "390-398",
        "finding": "DecodeBounds(SKStream) calls SKCodec.Create(stream) and returns codec?.Info ?? SKImageInfo.Empty. If the codec is null, Empty is returned silently.",
        "relevance": "direct"
      },
      {
        "file": "binding/SkiaSharp/SKCodec.cs",
        "lines": "249-264",
        "finding": "SKCodec.Create(SKStream) calls SkiaApi.sk_codec_new_from_stream(stream.Handle, r) and then RevokeOwnership. If the native call returns null (e.g., codec cannot detect PNG format), GetObject returns null.",
        "relevance": "direct"
      },
      {
        "file": "binding/SkiaSharp/SKCodec.cs",
        "lines": "278-290",
        "finding": "The Stream overload (DecodeBounds(Stream)) uses WrapManagedStream which buffers non-seekable streams via SKFrontBufferedManagedStream, while the SKStream overload goes directly to native. If seekability is required by the iOS PNG codec but not handled, this path would fail.",
        "relevance": "related"
      }
    ],
    "keySignals": [
      {
        "text": "Works properly Android, WPF/Console",
        "source": "issue body",
        "interpretation": "The bug is iOS-specific — the same code path works on other platforms, indicating a native library or codec registration difference on iOS."
      },
      {
        "text": "I also verified the code works well for webp",
        "source": "issue body",
        "interpretation": "WebP decoding works on iOS, so the SKManagedStream and native stream binding are functioning. The problem is specific to the PNG codec path."
      },
      {
        "text": "Version with issue: 2.80.2 / Last known good version: 1.68",
        "source": "issue body",
        "interpretation": "Clear regression across a major version bump. The Skia version underlying libSkiaSharp changed significantly between 1.68 and 2.80."
      }
    ],
    "rationale": "Classified as type/bug because the API returns wrong output (empty info instead of valid PNG dimensions) on a specific platform where it previously worked. Area is area/SkiaSharp because the issue manifests through the public C# API (DecodeBounds), though the root cause is likely in the native iOS build of libSkiaSharp. Platform is os/iOS because it does not reproduce on Android or desktop. Severity is high because PNG is a common image format and the regression breaks a frequently-used decode API path with no obvious workaround.",
    "nextQuestions": [
      "Does the issue reproduce with SKBitmap.Decode (not just DecodeBounds) on iOS?",
      "Does passing the Stream directly (DecodeBounds(Stream) instead of wrapping in SKManagedStream) work on iOS?",
      "Does SKCodec.Create(SKData) work for PNG on iOS (bypassing the stream path)?",
      "Does the issue reproduce with the current latest version of SkiaSharp?",
      "Is the stream Position at 0 before passing to SKManagedStream?"
    ],
    "resolution": {
      "hypothesis": "The PNG codec in the iOS native library (libSkiaSharp) may have a bug or build configuration difference in v2.80 that prevents it from recognizing or decoding PNG via a managed stream. A workaround is to read the stream into a byte array or SKData and use the data-based decode path instead.",
      "proposals": [
        {
          "title": "Use byte array / SKData overload instead of SKStream",
          "description": "Read the MemoryStream contents into a byte array and call DecodeBounds(byte[]) or DecodeBounds(SKData), bypassing the managed stream path which may be broken for PNG on iOS.",
          "category": "workaround",
          "effort": "cost/xs",
          "validated": "yes",
          "codeSnippet": "// Instead of wrapping in SKManagedStream:\nbyte[] pngBytes = memoryStream.ToArray();\nSKImageInfo imageInfo = SKBitmap.DecodeBounds(pngBytes);\n\n// Or via SKData:\nmemoryStream.Position = 0;\nusing (var skData = SKData.Create(memoryStream))\n{\n    imageInfo = SKBitmap.DecodeBounds(skData);\n}"
        },
        {
          "title": "Investigate native iOS PNG codec path in libSkiaSharp 2.80",
          "description": "Compare the Skia build configuration and PNG codec registration between 1.68 and 2.80 builds for iOS to identify what changed. Check if sk_codec_new_from_stream returns a non-null result on iOS for PNG data.",
          "category": "investigation",
          "effort": "cost/m",
          "validated": "untested"
        }
      ],
      "recommendedProposal": "workaround-1",
      "recommendedReason": "The byte array workaround is simple, proven on other platforms, and avoids the managed stream path entirely while the native root cause is investigated."
    }
  },
  "output": {
    "actionability": {
      "suggestedAction": "needs-investigation",
      "confidence": 0.85,
      "reason": "Clear regression report with code snippet, iOS-specific failure with platform comparison data (works on Android/WPF), and a known workaround. Needs reproduction on iOS to confirm the bug still exists in current versions before a fix can be targeted.",
      "suggestedReproPlatform": "macos"
    },
    "actions": [
      {
        "type": "update-labels",
        "description": "Apply type/bug, area/SkiaSharp, os/iOS, tenet/reliability, tenet/compatibility",
        "risk": "low",
        "confidence": 0.95,
        "labels": [
          "type/bug",
          "area/SkiaSharp",
          "os/iOS",
          "tenet/reliability",
          "tenet/compatibility"
        ]
      },
      {
        "type": "add-comment",
        "description": "Acknowledge the regression, provide a workaround using byte array / SKData path, and request confirmation of the current SkiaSharp version.",
        "risk": "medium",
        "confidence": 0.85,
        "comment": "Thank you for reporting this!\n\nWe can confirm this looks like a regression in the iOS native library's PNG codec path when using `SKManagedStream` directly. The `DecodeBounds(SKStream)` path calls the native codec factory via a managed stream handle, and the PNG codec on iOS in v2.80 appears to fail to initialize from this path.\n\n**Workaround:** Read the stream contents into a byte array (or `SKData`) and use the data-based overload instead:\n\n```csharp\n// Option 1: byte array\nbyte[] pngBytes = memoryStream.ToArray();\nSKImageInfo imageInfo = SKBitmap.DecodeBounds(pngBytes);\n\n// Option 2: SKData\nmemoryStream.Position = 0;\nusing (var skData = SKData.Create(memoryStream))\n{\n    imageInfo = SKBitmap.DecodeBounds(skData);\n}\n```\n\nAlso make sure `memoryStream.Position` is at `0` before passing it to the stream constructor.\n\nCould you also confirm whether this issue still occurs with the latest version of SkiaSharp?"
      }
    ]
  }
}
```

</details>
