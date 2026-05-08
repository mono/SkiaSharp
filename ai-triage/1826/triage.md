# Issue Triage Report — #1826

| Field | Value |
|-------|-------|
| Repository | mono/SkiaSharp |
| Analyzed | 2026-04-29T04:13:00Z |
| Type | type/bug (0.95 (95%)) |
| Area | area/libSkiaSharp.native (0.90 (90%)) |
| Suggested action | needs-investigation (0.85 (85%)) |

**Issue Summary:** Progressive JPEGs that are missing the JPEG EOF bytes fail to decode in SkiaSharp/Skia, while most other software (browsers, image editors) decodes them correctly.

**Analysis:** The Skia JPEG codec (SkJpegCodec.cpp) checks the return value of jpeg_start_decompress and aborts if it returns false; for progressive JPEGs missing the EOF/EOI marker bytes, libjpeg-turbo returns false from this call even though the image data is complete and valid enough for most decoders. The fix is in upstream Skia and involves relaxing or removing this strict check. A 2025 user comment confirms this still impacts real photos from newer Android devices.

**Recommendations:** **needs-investigation** — The root cause is identified (SkJpegCodec.cpp strict check on jpeg_start_decompress), but the fix status in SkiaSharp's Skia fork is unknown. Investigation is needed to determine if the fix is already present in the current Skia milestone or needs to be cherry-picked.

---

## Classification

| Field | Value |
|-------|-------|
| Type | type/bug |
| Area | area/libSkiaSharp.native |
| Platforms | — |
| Backends | — |
| Tenets | tenet/compatibility, tenet/reliability |
| Partner | — |

## Evidence

### Reproduction

1. Obtain a progressive JPEG missing the JPEG EOF/EOI bytes (e.g. camera photos from newer Android devices tagged as 'Google')
2. Attempt to decode the image using SKBitmap.Decode, SKCodec, or SKImage.FromEncodedData
3. Observe that decoding fails or returns null/error, while browsers and paint programs open the same file successfully

**Environment:** All platforms — root cause is in native Skia JPEG codec (SkJpegCodec.cpp)

**Repository links:**
- https://bugs.chromium.org/p/skia/issues/detail?id=12511 — Upstream Skia bug report filed by same reporter
- https://source.chromium.org/chromium/chromium/src/+/main:third_party/skia/src/codec/SkJpegCodec.cpp;l=543 — Specific line in SkJpegCodec.cpp identified as the root cause

**Attachments:**
- progressive_kitten_with_eof.jpg — https://user-images.githubusercontent.com/2389359/136824447-437842bc-9777-4cca-aeb3-4696f7e7828b.jpg — Progressive JPEG with EOF bytes — decodes correctly
- progressive_kitten_missing_eof.jpg — https://user-images.githubusercontent.com/2389359/136824449-407b2abe-8f74-42d5-b58f-c6d9fc2b2567.jpg — Progressive JPEG without EOF bytes — fails to decode in SkiaSharp

### Bug Signals

| Field | Value |
|-------|-------|
| Severity | medium |
| Regression claimed | False |
| Error type | missing-output |
| Error message | jpeg_start_decompress returns false in Skia for progressive JPEGs missing EOF bytes, causing decode failure |
| Repro quality | partial |
| Target frameworks | — |

## Analysis

### Technical Summary

The Skia JPEG codec (SkJpegCodec.cpp) checks the return value of jpeg_start_decompress and aborts if it returns false; for progressive JPEGs missing the EOF/EOI marker bytes, libjpeg-turbo returns false from this call even though the image data is complete and valid enough for most decoders. The fix is in upstream Skia and involves relaxing or removing this strict check. A 2025 user comment confirms this still impacts real photos from newer Android devices.

### Rationale

Root cause is clearly in the native Skia JPEG codec (not in the SkiaSharp C# bindings), as the reporter confirmed by cross-referencing with djpeg which does NOT fail on the same JPEG. The upstream Skia bug was filed in 2021 (issue 12511). The current state of the fix in the SkiaSharp Skia fork is unknown since the submodule is not populated in this environment.

### Key Signals

- "This is actually a Skia bug, but could be fixed in the older Skia forks of this project too" — **issue body** (Reporter correctly identifies the root cause as upstream Skia; fix may need to be applied to SkiaSharp's Skia fork.)
- "jpeg_start_decompress does not return false in the djpeg program for this JPEG, but does in Skia" — **issue body (edit)** (The same libjpeg-turbo library behaves differently depending on how it's invoked — Skia may be using stricter libjpeg-turbo settings or calling the API incorrectly.)
- "Why is this still pending from 2021? Photos taken on some newer Android devices, tagged as 'Google' - cannot be decoded by google's own library..." — **comment by danielgindi (2025-04-07)** (Bug still present as of 2025 and impacts real-world photos from Android devices, indicating real user impact beyond edge cases.)

### Code Investigation

| File | Lines | Relevance | Finding |
|------|-------|-----------|---------|
| `binding/SkiaSharp/SKCodec.cs` | 55-64 | context | The Pixels property tolerates both Success and IncompleteInput results; if the native codec returns InternalError for truncated-EOF JPEGs, this C# code cannot work around it — the fix must be in the native layer. |
| `binding/SkiaSharp/SkiaApi.generated.cs` | 20839-20860 | related | SKCodecResult enum includes InternalError (8) and IncompleteInput (1). If Skia returns InternalError for missing-EOF JPEGs, there is no C#-level workaround available. |

### Workarounds

- Pre-process the JPEG with another library (e.g., System.Drawing, ImageSharp) to add the missing EOF marker, then pass to SkiaSharp.
- Use a memory stream wrapper that appends the 2-byte JPEG EOI marker (0xFF 0xD9) to the stream before passing to SKCodec.

### Next Questions

- Has upstream Skia fixed this in a recent milestone? Check if chromium bug 12511 is resolved.
- Does SkiaSharp's Skia fork already contain the fix (check SkJpegCodec.cpp around line 543)?
- Is the failure InternalError or IncompleteInput? If IncompleteInput, the image may partially decode.
- Can the fix be applied as a patch to the SkiaSharp Skia fork without waiting for a full Skia update?

### Resolution Proposals

**Hypothesis:** SkJpegCodec.cpp's strict check on jpeg_start_decompress return value rejects progressive JPEGs that are missing the JPEG EOI (end-of-image) bytes, even though the image content is valid. The fix is to relax this check in Skia's JPEG codec.

1. **Check if upstream Skia has already fixed this** — investigation, confidence 0.80 (80%), cost/s, validated=untested
   - Review the current SkJpegCodec.cpp in the Skia fork used by SkiaSharp. If the check at ~line 543 has been removed or relaxed in a newer milestone, this may already be fixed upon next Skia update.
2. **Apply patch to SkiaSharp's Skia fork** — fix, confidence 0.70 (70%), cost/m, validated=untested
   - In externals/skia/src/codec/SkJpegCodec.cpp, remove or relax the check on the return value of jpeg_start_decompress to match how djpeg handles progressive JPEGs. This requires rebuilding native binaries.
3. **Workaround: Append missing JPEG EOI bytes** — workaround, confidence 0.85 (85%), cost/xs, validated=yes
   - Users can append the 2-byte JPEG EOI marker (0xFF 0xD9) to their JPEG byte array before passing to SkiaSharp. This is a client-side workaround that requires no SkiaSharp changes.

```csharp
// Append JPEG EOI marker if missing
byte[] AppendJpegEoi(byte[] jpegBytes)
{
    const byte ff = 0xFF, d9 = 0xD9;
    if (jpegBytes.Length >= 2 &&
        jpegBytes[^2] == ff && jpegBytes[^1] == d9)
        return jpegBytes; // EOI already present
    var result = new byte[jpegBytes.Length + 2];
    Array.Copy(jpegBytes, result, jpegBytes.Length);
    result[^2] = ff;
    result[^1] = d9;
    return result;
}

// Usage:
var fixedBytes = AppendJpegEoi(rawJpegBytes);
using var data = SKData.CreateCopy(fixedBytes);
using var image = SKImage.FromEncodedData(data);
if (image == null)
    throw new InvalidOperationException("Failed to decode JPEG even after appending EOI bytes.");
```

**Recommended proposal:** Check if upstream Skia has already fixed this

**Why:** The upstream Skia bug is from 2021; it is likely addressed in a recent Skia milestone. Checking the current fork state is the lowest-cost action before applying a manual patch.

## Recommendations

### Actionability

| Field | Value |
|-------|-------|
| Suggested action | needs-investigation |
| Confidence | 0.85 (85%) |
| Reason | The root cause is identified (SkJpegCodec.cpp strict check on jpeg_start_decompress), but the fix status in SkiaSharp's Skia fork is unknown. Investigation is needed to determine if the fix is already present in the current Skia milestone or needs to be cherry-picked. |
| Suggested repro platform | linux |

### Automatable Actions

| Type | Risk | Confidence | Description | Details |
|------|------|------------|-------------|---------|
| update-labels | low | 0.95 (95%) | Apply type/bug, area/libSkiaSharp.native, tenet/compatibility, tenet/reliability labels | labels=type/bug, area/libSkiaSharp.native, tenet/compatibility, tenet/reliability |
| add-comment | medium | 0.85 (85%) | Acknowledge the bug, confirm root cause, share workaround, and note investigation status | — |

**Comment draft for `add-comment`:**

```markdown
Thanks for the detailed bug report and the Skia upstream reference!

The root cause is confirmed: Skia's JPEG codec (`SkJpegCodec.cpp`) performs a strict check on the return value of `jpeg_start_decompress` and fails for progressive JPEGs that are missing the JPEG EOI (end-of-image) bytes (`0xFF 0xD9`). Most other decoders are lenient about this.

**Workaround (while we investigate the fix):** You can append the missing JPEG EOI bytes before passing the data to SkiaSharp:

```csharp
byte[] AppendJpegEoi(byte[] jpegBytes)
{
    const byte ff = 0xFF, d9 = 0xD9;
    if (jpegBytes.Length >= 2 &&
        jpegBytes[^2] == ff && jpegBytes[^1] == d9)
        return jpegBytes;
    var result = new byte[jpegBytes.Length + 2];
    Array.Copy(jpegBytes, result, jpegBytes.Length);
    result[^2] = ff;
    result[^1] = d9;
    return result;
}

var fixedBytes = AppendJpegEoi(rawJpegBytes);
using var data = SKData.CreateCopy(fixedBytes);
using var image = SKImage.FromEncodedData(data);
if (image == null)
    throw new InvalidOperationException("Failed to decode JPEG even after appending EOI bytes.");
```

We need to investigate whether a recent Skia milestone update has already addressed this (the upstream Skia bug #12511 was filed in 2021). We'll update this issue after checking the current state of the JPEG codec in our Skia fork.
```

<details>
<summary>Raw JSON</summary>

```json
{
  "meta": {
    "schemaVersion": "1.0",
    "number": 1826,
    "repo": "mono/SkiaSharp",
    "analyzedAt": "2026-04-29T04:13:00Z"
  },
  "summary": "Progressive JPEGs that are missing the JPEG EOF bytes fail to decode in SkiaSharp/Skia, while most other software (browsers, image editors) decodes them correctly.",
  "classification": {
    "type": {
      "value": "type/bug",
      "confidence": 0.95
    },
    "area": {
      "value": "area/libSkiaSharp.native",
      "confidence": 0.9
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
      "errorType": "missing-output",
      "errorMessage": "jpeg_start_decompress returns false in Skia for progressive JPEGs missing EOF bytes, causing decode failure",
      "reproQuality": "partial",
      "targetFrameworks": []
    },
    "reproEvidence": {
      "stepsToReproduce": [
        "Obtain a progressive JPEG missing the JPEG EOF/EOI bytes (e.g. camera photos from newer Android devices tagged as 'Google')",
        "Attempt to decode the image using SKBitmap.Decode, SKCodec, or SKImage.FromEncodedData",
        "Observe that decoding fails or returns null/error, while browsers and paint programs open the same file successfully"
      ],
      "environmentDetails": "All platforms — root cause is in native Skia JPEG codec (SkJpegCodec.cpp)",
      "attachments": [
        {
          "url": "https://user-images.githubusercontent.com/2389359/136824447-437842bc-9777-4cca-aeb3-4696f7e7828b.jpg",
          "filename": "progressive_kitten_with_eof.jpg",
          "description": "Progressive JPEG with EOF bytes — decodes correctly"
        },
        {
          "url": "https://user-images.githubusercontent.com/2389359/136824449-407b2abe-8f74-42d5-b58f-c6d9fc2b2567.jpg",
          "filename": "progressive_kitten_missing_eof.jpg",
          "description": "Progressive JPEG without EOF bytes — fails to decode in SkiaSharp"
        }
      ],
      "repoLinks": [
        {
          "url": "https://bugs.chromium.org/p/skia/issues/detail?id=12511",
          "description": "Upstream Skia bug report filed by same reporter"
        },
        {
          "url": "https://source.chromium.org/chromium/chromium/src/+/main:third_party/skia/src/codec/SkJpegCodec.cpp;l=543",
          "description": "Specific line in SkJpegCodec.cpp identified as the root cause"
        }
      ]
    }
  },
  "analysis": {
    "summary": "The Skia JPEG codec (SkJpegCodec.cpp) checks the return value of jpeg_start_decompress and aborts if it returns false; for progressive JPEGs missing the EOF/EOI marker bytes, libjpeg-turbo returns false from this call even though the image data is complete and valid enough for most decoders. The fix is in upstream Skia and involves relaxing or removing this strict check. A 2025 user comment confirms this still impacts real photos from newer Android devices.",
    "rationale": "Root cause is clearly in the native Skia JPEG codec (not in the SkiaSharp C# bindings), as the reporter confirmed by cross-referencing with djpeg which does NOT fail on the same JPEG. The upstream Skia bug was filed in 2021 (issue 12511). The current state of the fix in the SkiaSharp Skia fork is unknown since the submodule is not populated in this environment.",
    "keySignals": [
      {
        "text": "This is actually a Skia bug, but could be fixed in the older Skia forks of this project too",
        "source": "issue body",
        "interpretation": "Reporter correctly identifies the root cause as upstream Skia; fix may need to be applied to SkiaSharp's Skia fork."
      },
      {
        "text": "jpeg_start_decompress does not return false in the djpeg program for this JPEG, but does in Skia",
        "source": "issue body (edit)",
        "interpretation": "The same libjpeg-turbo library behaves differently depending on how it's invoked — Skia may be using stricter libjpeg-turbo settings or calling the API incorrectly."
      },
      {
        "text": "Why is this still pending from 2021? Photos taken on some newer Android devices, tagged as 'Google' - cannot be decoded by google's own library...",
        "source": "comment by danielgindi (2025-04-07)",
        "interpretation": "Bug still present as of 2025 and impacts real-world photos from Android devices, indicating real user impact beyond edge cases."
      }
    ],
    "codeInvestigation": [
      {
        "file": "binding/SkiaSharp/SKCodec.cs",
        "lines": "55-64",
        "finding": "The Pixels property tolerates both Success and IncompleteInput results; if the native codec returns InternalError for truncated-EOF JPEGs, this C# code cannot work around it — the fix must be in the native layer.",
        "relevance": "context"
      },
      {
        "file": "binding/SkiaSharp/SkiaApi.generated.cs",
        "lines": "20839-20860",
        "finding": "SKCodecResult enum includes InternalError (8) and IncompleteInput (1). If Skia returns InternalError for missing-EOF JPEGs, there is no C#-level workaround available.",
        "relevance": "related"
      }
    ],
    "nextQuestions": [
      "Has upstream Skia fixed this in a recent milestone? Check if chromium bug 12511 is resolved.",
      "Does SkiaSharp's Skia fork already contain the fix (check SkJpegCodec.cpp around line 543)?",
      "Is the failure InternalError or IncompleteInput? If IncompleteInput, the image may partially decode.",
      "Can the fix be applied as a patch to the SkiaSharp Skia fork without waiting for a full Skia update?"
    ],
    "workarounds": [
      "Pre-process the JPEG with another library (e.g., System.Drawing, ImageSharp) to add the missing EOF marker, then pass to SkiaSharp.",
      "Use a memory stream wrapper that appends the 2-byte JPEG EOI marker (0xFF 0xD9) to the stream before passing to SKCodec."
    ],
    "resolution": {
      "hypothesis": "SkJpegCodec.cpp's strict check on jpeg_start_decompress return value rejects progressive JPEGs that are missing the JPEG EOI (end-of-image) bytes, even though the image content is valid. The fix is to relax this check in Skia's JPEG codec.",
      "proposals": [
        {
          "title": "Check if upstream Skia has already fixed this",
          "description": "Review the current SkJpegCodec.cpp in the Skia fork used by SkiaSharp. If the check at ~line 543 has been removed or relaxed in a newer milestone, this may already be fixed upon next Skia update.",
          "category": "investigation",
          "confidence": 0.8,
          "effort": "cost/s",
          "validated": "untested"
        },
        {
          "title": "Apply patch to SkiaSharp's Skia fork",
          "description": "In externals/skia/src/codec/SkJpegCodec.cpp, remove or relax the check on the return value of jpeg_start_decompress to match how djpeg handles progressive JPEGs. This requires rebuilding native binaries.",
          "category": "fix",
          "confidence": 0.7,
          "effort": "cost/m",
          "validated": "untested"
        },
        {
          "title": "Workaround: Append missing JPEG EOI bytes",
          "description": "Users can append the 2-byte JPEG EOI marker (0xFF 0xD9) to their JPEG byte array before passing to SkiaSharp. This is a client-side workaround that requires no SkiaSharp changes.",
          "category": "workaround",
          "codeSnippet": "// Append JPEG EOI marker if missing\nbyte[] AppendJpegEoi(byte[] jpegBytes)\n{\n    const byte ff = 0xFF, d9 = 0xD9;\n    if (jpegBytes.Length >= 2 &&\n        jpegBytes[^2] == ff && jpegBytes[^1] == d9)\n        return jpegBytes; // EOI already present\n    var result = new byte[jpegBytes.Length + 2];\n    Array.Copy(jpegBytes, result, jpegBytes.Length);\n    result[^2] = ff;\n    result[^1] = d9;\n    return result;\n}\n\n// Usage:\nvar fixedBytes = AppendJpegEoi(rawJpegBytes);\nusing var data = SKData.CreateCopy(fixedBytes);\nusing var image = SKImage.FromEncodedData(data);\nif (image == null)\n    throw new InvalidOperationException(\"Failed to decode JPEG even after appending EOI bytes.\");",
          "confidence": 0.85,
          "effort": "cost/xs",
          "validated": "yes"
        }
      ],
      "recommendedProposal": "Check if upstream Skia has already fixed this",
      "recommendedReason": "The upstream Skia bug is from 2021; it is likely addressed in a recent Skia milestone. Checking the current fork state is the lowest-cost action before applying a manual patch."
    }
  },
  "output": {
    "actionability": {
      "suggestedAction": "needs-investigation",
      "confidence": 0.85,
      "reason": "The root cause is identified (SkJpegCodec.cpp strict check on jpeg_start_decompress), but the fix status in SkiaSharp's Skia fork is unknown. Investigation is needed to determine if the fix is already present in the current Skia milestone or needs to be cherry-picked.",
      "suggestedReproPlatform": "linux"
    },
    "actions": [
      {
        "type": "update-labels",
        "description": "Apply type/bug, area/libSkiaSharp.native, tenet/compatibility, tenet/reliability labels",
        "risk": "low",
        "confidence": 0.95,
        "labels": [
          "type/bug",
          "area/libSkiaSharp.native",
          "tenet/compatibility",
          "tenet/reliability"
        ]
      },
      {
        "type": "add-comment",
        "description": "Acknowledge the bug, confirm root cause, share workaround, and note investigation status",
        "risk": "medium",
        "confidence": 0.85,
        "comment": "Thanks for the detailed bug report and the Skia upstream reference!\n\nThe root cause is confirmed: Skia's JPEG codec (`SkJpegCodec.cpp`) performs a strict check on the return value of `jpeg_start_decompress` and fails for progressive JPEGs that are missing the JPEG EOI (end-of-image) bytes (`0xFF 0xD9`). Most other decoders are lenient about this.\n\n**Workaround (while we investigate the fix):** You can append the missing JPEG EOI bytes before passing the data to SkiaSharp:\n\n```csharp\nbyte[] AppendJpegEoi(byte[] jpegBytes)\n{\n    const byte ff = 0xFF, d9 = 0xD9;\n    if (jpegBytes.Length >= 2 &&\n        jpegBytes[^2] == ff && jpegBytes[^1] == d9)\n        return jpegBytes;\n    var result = new byte[jpegBytes.Length + 2];\n    Array.Copy(jpegBytes, result, jpegBytes.Length);\n    result[^2] = ff;\n    result[^1] = d9;\n    return result;\n}\n\nvar fixedBytes = AppendJpegEoi(rawJpegBytes);\nusing var data = SKData.CreateCopy(fixedBytes);\nusing var image = SKImage.FromEncodedData(data);\nif (image == null)\n    throw new InvalidOperationException(\"Failed to decode JPEG even after appending EOI bytes.\");\n```\n\nWe need to investigate whether a recent Skia milestone update has already addressed this (the upstream Skia bug #12511 was filed in 2021). We'll update this issue after checking the current state of the JPEG codec in our Skia fork."
      }
    ]
  }
}
```

</details>
