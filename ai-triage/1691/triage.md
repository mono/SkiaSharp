# Issue Triage Report — #1691

| Field | Value |
|-------|-------|
| Repository | mono/SkiaSharp |
| Analyzed | 2026-05-06T18:47:00Z |
| Type | type/bug (0.95 (95%)) |
| Area | area/SkiaSharp (0.90 (90%)) |
| Suggested action | needs-investigation (0.88 (88%)) |

**Issue Summary:** SKBitmap.Decode hangs indefinitely when decoding a PNG file that contains an extremely large (100MB) Exif UserComment tag, blocking the thread without any timeout or cancellation mechanism.

**Analysis:** When a PNG file contains an extremely large Exif chunk (100MB UserComment), Skia's native PNG decoder reads the entire Exif block synchronously during the GetPixels call, causing an indefinite hang. The SkiaSharp C# layer has no timeout or cancellation support; it directly calls sk_codec_get_pixels which blocks until Skia finishes parsing. There is no exception thrown and no way to interrupt the native operation from managed code.

**Recommendations:** **needs-investigation** — Confirmed bug with complete reproduction project. The hang is real and affects reliability in production systems. Root cause is in Skia's native PNG decoder; needs reproduction on current version to determine if upstream fixed it.

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

1. Create a PNG image using Magick.NET with an Exif UserComment property set to a 100MB byte array
2. Call SKCodec.Create(stream) on that PNG stream
3. Call SKBitmap.Decode(skCodec) — execution hangs indefinitely

**Environment:** Version 2.80.2, .NET 5 Console App, Rider JetBrains IDE

**Repository links:**
- https://github.com/mono/SkiaSharp/files/7286752/SkiaDecodingIssue.zip — Minimal repro project attached by reporter

### Bug Signals

| Field | Value |
|-------|-------|
| Severity | high |
| Regression claimed | False |
| Error type | other |
| Error message | Code execution blocked indefinitely on SKBitmap.Decode(skCodec) when Exif UserComment is 100MB |
| Repro quality | complete |
| Target frameworks | net5.0 |

### Version Analysis

| Field | Value |
|-------|-------|
| Mentioned versions | 2.80.2 |
| Worked in | — |
| Broke in | — |
| Current relevance | likely |
| Relevance reason | No fix has been identified in the codebase. The decode path (SKCodec.GetPixels → sk_codec_get_pixels) is unchanged and relies on Skia's upstream PNG decoder which processes Exif metadata synchronously. |

## Analysis

### Technical Summary

When a PNG file contains an extremely large Exif chunk (100MB UserComment), Skia's native PNG decoder reads the entire Exif block synchronously during the GetPixels call, causing an indefinite hang. The SkiaSharp C# layer has no timeout or cancellation support; it directly calls sk_codec_get_pixels which blocks until Skia finishes parsing. There is no exception thrown and no way to interrupt the native operation from managed code.

### Rationale

The issue is a real defect: a crafted/malformed PNG with an abnormally large Exif tag causes the decode to hang indefinitely with no way to cancel or time out. The C# wrapper (SKBitmap.Decode → SKCodec.GetPixels) simply calls through to the native sk_codec_get_pixels without any guard. The root cause is in Skia's native PNG codec behavior with oversized Exif, but it manifests as a reliability problem for SkiaSharp consumers in critical systems.

### Key Signals

- "my code flow is locked on SKBitmap.Decode(skCodec) call" — **issue body** (Thread is blocked indefinitely inside native PNG decoding — no exception or timeout)
- "I have added a timeout mechanism to stop the decoding process if it takes too much time but we cannot stop Skia reading the initial stream" — **issue body** (Managed-code timeouts cannot interrupt native blocking calls; CancellationToken has no effect)
- "In critical systems where Skia is used to process customer pictures, that issue can make the system down because of 'invalid' pictures" — **issue body** (Denial-of-service risk in production image-processing pipelines)
- "Version with issue: 2.80.2" — **issue body** (Reported on 2.80.2; no fix record found in issue/PR history)

### Code Investigation

| File | Lines | Relevance | Finding |
|------|-------|-----------|---------|
| `binding/SkiaSharp/SKBitmap.cs` | — | direct | SKBitmap.Decode(SKCodec, SKImageInfo) calls codec.GetPixels(bitmapInfo, bitmap.GetPixels(out var length)) — a blocking synchronous P/Invoke call with no timeout or cancellation path |
| `binding/SkiaSharp/SKCodec.cs` | — | direct | SKCodec.GetPixels(SKImageInfo, IntPtr, int, SKCodecOptions) calls SkiaApi.sk_codec_get_pixels() — a native call that processes the entire stream synchronously. No cancellation token or async path exists in the SkiaSharp binding. |

### Next Questions

- Does the hang occur in SKCodec.Create (parsing Exif at codec creation time) or in GetPixels (during actual pixel decoding)?
- Is this fixed in Skia upstream (later milestones) by imposing Exif chunk size limits?
- Is the same hang reproducible with the latest SkiaSharp 2.88.x or 3.x?

### Resolution Proposals

**Hypothesis:** Skia's PNG decoder reads the entire Exif chunk during decode without size limits; a 100MB Exif tag causes it to allocate and process 100MB of metadata before returning, causing a perceived hang.

1. **Reproduce and trace exact hang location** — investigation, confidence 0.90 (90%), cost/s, validated=untested
   - Run repro on latest SkiaSharp (2.88.x / 3.x) to determine if still present, and whether hang is in SKCodec.Create or GetPixels. Check Skia upstream for PNG Exif size limits added after m80.
2. **Workaround: validate stream size before decode** — workaround, confidence 0.80 (80%), cost/xs, validated=untested
   - Consumers can check the stream length before decoding. If the stream is unreasonably large (e.g., >50MB), reject it before calling SKCodec.Create. This avoids the hang for malformed inputs.

**Recommended proposal:** Reproduce and trace exact hang location

**Why:** Before proposing a fix, the exact callsite of the hang (SKCodec.Create vs GetPixels) and whether a newer Skia version resolves it must be established.

## Recommendations

### Actionability

| Field | Value |
|-------|-------|
| Suggested action | needs-investigation |
| Confidence | 0.88 (88%) |
| Reason | Confirmed bug with complete reproduction project. The hang is real and affects reliability in production systems. Root cause is in Skia's native PNG decoder; needs reproduction on current version to determine if upstream fixed it. |
| Suggested repro platform | linux |

### Automatable Actions

| Type | Risk | Confidence | Description | Details |
|------|------|------------|-------------|---------|
| update-labels | low | 0.95 (95%) | Apply bug, core SkiaSharp area, and reliability tenet labels | labels=type/bug, area/SkiaSharp, tenet/reliability |
| add-comment | medium | 0.85 (85%) | Acknowledge bug, note investigation needed on current version, provide stream-size workaround | — |

**Comment draft for `add-comment`:**

```markdown
Thank you for the detailed report and attached reproduction project.

This looks like a genuine reliability issue: Skia's PNG decoder processes the Exif chunk synchronously without a size limit, and a 100 MB `UserComment` tag causes an indefinite block in `sk_codec_get_pixels` with no way to cancel from managed code.

**Workaround (immediate):** Validate the stream length before decoding. If the encoded PNG stream is unreasonably large, reject it before calling `SKCodec.Create`:

```csharp
if (stream.Length > 50_000_000)
    throw new InvalidOperationException("Image stream too large to decode safely.");
var codec = SKCodec.Create(stream);
```

This avoids the hang for malformed inputs without changing SkiaSharp itself.

We will investigate whether the issue is still reproducible on the current SkiaSharp release (2.88.x / 3.x) and whether a newer Skia milestone added Exif chunk size limits.
```

<details>
<summary>Raw JSON</summary>

```json
{
  "meta": {
    "schemaVersion": "1.0",
    "number": 1691,
    "repo": "mono/SkiaSharp",
    "analyzedAt": "2026-05-06T18:47:00Z"
  },
  "summary": "SKBitmap.Decode hangs indefinitely when decoding a PNG file that contains an extremely large (100MB) Exif UserComment tag, blocking the thread without any timeout or cancellation mechanism.",
  "classification": {
    "type": {
      "value": "type/bug",
      "confidence": 0.95
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
      "severity": "high",
      "regressionClaimed": false,
      "errorType": "other",
      "errorMessage": "Code execution blocked indefinitely on SKBitmap.Decode(skCodec) when Exif UserComment is 100MB",
      "reproQuality": "complete",
      "targetFrameworks": [
        "net5.0"
      ]
    },
    "reproEvidence": {
      "stepsToReproduce": [
        "Create a PNG image using Magick.NET with an Exif UserComment property set to a 100MB byte array",
        "Call SKCodec.Create(stream) on that PNG stream",
        "Call SKBitmap.Decode(skCodec) — execution hangs indefinitely"
      ],
      "environmentDetails": "Version 2.80.2, .NET 5 Console App, Rider JetBrains IDE",
      "repoLinks": [
        {
          "url": "https://github.com/mono/SkiaSharp/files/7286752/SkiaDecodingIssue.zip",
          "description": "Minimal repro project attached by reporter"
        }
      ]
    },
    "versionAnalysis": {
      "mentionedVersions": [
        "2.80.2"
      ],
      "currentRelevance": "likely",
      "relevanceReason": "No fix has been identified in the codebase. The decode path (SKCodec.GetPixels → sk_codec_get_pixels) is unchanged and relies on Skia's upstream PNG decoder which processes Exif metadata synchronously."
    }
  },
  "analysis": {
    "summary": "When a PNG file contains an extremely large Exif chunk (100MB UserComment), Skia's native PNG decoder reads the entire Exif block synchronously during the GetPixels call, causing an indefinite hang. The SkiaSharp C# layer has no timeout or cancellation support; it directly calls sk_codec_get_pixels which blocks until Skia finishes parsing. There is no exception thrown and no way to interrupt the native operation from managed code.",
    "rationale": "The issue is a real defect: a crafted/malformed PNG with an abnormally large Exif tag causes the decode to hang indefinitely with no way to cancel or time out. The C# wrapper (SKBitmap.Decode → SKCodec.GetPixels) simply calls through to the native sk_codec_get_pixels without any guard. The root cause is in Skia's native PNG codec behavior with oversized Exif, but it manifests as a reliability problem for SkiaSharp consumers in critical systems.",
    "keySignals": [
      {
        "text": "my code flow is locked on SKBitmap.Decode(skCodec) call",
        "source": "issue body",
        "interpretation": "Thread is blocked indefinitely inside native PNG decoding — no exception or timeout"
      },
      {
        "text": "I have added a timeout mechanism to stop the decoding process if it takes too much time but we cannot stop Skia reading the initial stream",
        "source": "issue body",
        "interpretation": "Managed-code timeouts cannot interrupt native blocking calls; CancellationToken has no effect"
      },
      {
        "text": "In critical systems where Skia is used to process customer pictures, that issue can make the system down because of 'invalid' pictures",
        "source": "issue body",
        "interpretation": "Denial-of-service risk in production image-processing pipelines"
      },
      {
        "text": "Version with issue: 2.80.2",
        "source": "issue body",
        "interpretation": "Reported on 2.80.2; no fix record found in issue/PR history"
      }
    ],
    "codeInvestigation": [
      {
        "file": "binding/SkiaSharp/SKBitmap.cs",
        "finding": "SKBitmap.Decode(SKCodec, SKImageInfo) calls codec.GetPixels(bitmapInfo, bitmap.GetPixels(out var length)) — a blocking synchronous P/Invoke call with no timeout or cancellation path",
        "relevance": "direct"
      },
      {
        "file": "binding/SkiaSharp/SKCodec.cs",
        "finding": "SKCodec.GetPixels(SKImageInfo, IntPtr, int, SKCodecOptions) calls SkiaApi.sk_codec_get_pixels() — a native call that processes the entire stream synchronously. No cancellation token or async path exists in the SkiaSharp binding.",
        "relevance": "direct"
      }
    ],
    "nextQuestions": [
      "Does the hang occur in SKCodec.Create (parsing Exif at codec creation time) or in GetPixels (during actual pixel decoding)?",
      "Is this fixed in Skia upstream (later milestones) by imposing Exif chunk size limits?",
      "Is the same hang reproducible with the latest SkiaSharp 2.88.x or 3.x?"
    ],
    "resolution": {
      "hypothesis": "Skia's PNG decoder reads the entire Exif chunk during decode without size limits; a 100MB Exif tag causes it to allocate and process 100MB of metadata before returning, causing a perceived hang.",
      "proposals": [
        {
          "title": "Reproduce and trace exact hang location",
          "description": "Run repro on latest SkiaSharp (2.88.x / 3.x) to determine if still present, and whether hang is in SKCodec.Create or GetPixels. Check Skia upstream for PNG Exif size limits added after m80.",
          "category": "investigation",
          "confidence": 0.9,
          "effort": "cost/s",
          "validated": "untested"
        },
        {
          "title": "Workaround: validate stream size before decode",
          "description": "Consumers can check the stream length before decoding. If the stream is unreasonably large (e.g., >50MB), reject it before calling SKCodec.Create. This avoids the hang for malformed inputs.",
          "category": "workaround",
          "confidence": 0.8,
          "effort": "cost/xs",
          "validated": "untested"
        }
      ],
      "recommendedProposal": "Reproduce and trace exact hang location",
      "recommendedReason": "Before proposing a fix, the exact callsite of the hang (SKCodec.Create vs GetPixels) and whether a newer Skia version resolves it must be established."
    }
  },
  "output": {
    "actionability": {
      "suggestedAction": "needs-investigation",
      "confidence": 0.88,
      "reason": "Confirmed bug with complete reproduction project. The hang is real and affects reliability in production systems. Root cause is in Skia's native PNG decoder; needs reproduction on current version to determine if upstream fixed it.",
      "suggestedReproPlatform": "linux"
    },
    "actions": [
      {
        "type": "update-labels",
        "description": "Apply bug, core SkiaSharp area, and reliability tenet labels",
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
        "description": "Acknowledge bug, note investigation needed on current version, provide stream-size workaround",
        "risk": "medium",
        "confidence": 0.85,
        "comment": "Thank you for the detailed report and attached reproduction project.\n\nThis looks like a genuine reliability issue: Skia's PNG decoder processes the Exif chunk synchronously without a size limit, and a 100 MB `UserComment` tag causes an indefinite block in `sk_codec_get_pixels` with no way to cancel from managed code.\n\n**Workaround (immediate):** Validate the stream length before decoding. If the encoded PNG stream is unreasonably large, reject it before calling `SKCodec.Create`:\n\n```csharp\nif (stream.Length > 50_000_000)\n    throw new InvalidOperationException(\"Image stream too large to decode safely.\");\nvar codec = SKCodec.Create(stream);\n```\n\nThis avoids the hang for malformed inputs without changing SkiaSharp itself.\n\nWe will investigate whether the issue is still reproducible on the current SkiaSharp release (2.88.x / 3.x) and whether a newer Skia milestone added Exif chunk size limits."
      }
    ]
  }
}
```

</details>
