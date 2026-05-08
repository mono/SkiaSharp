# Issue Triage Report — #2384

| Field | Value |
|-------|-------|
| Repository | mono/SkiaSharp |
| Analyzed | 2026-04-26T14:57:41Z |
| Type | type/question (0.92 (92%)) |
| Area | area/SkiaSharp (0.90 (90%)) |
| Suggested action | close-as-not-a-bug (0.88 (88%)) |

**Issue Summary:** User asks how to convert AV_PIX_FMT_BGR24 (24-bit BGR pixel data from FFmpeg) to SKBitmap; reporter's conversion code produces a blueish tint due to an incorrect channel-swap in the BGR24→BGRA32 loop.

**Analysis:** Reporter wants to load 24-bit BGR pixel data (AV_PIX_FMT_BGR24) from an FFmpeg video frame into an SKBitmap. SkiaSharp has no native 24-bit BGR color type, so data must be expanded to 32-bit (e.g. Bgra8888 or Rgb888x). The reporter's conversion has a channel-swap bug: reading R into the B slot and B into the R slot, causing the blueish tint. A community member already identified the fix (copy bytes without swapping). Related issue #2412 by the same user repeats the same question.

**Recommendations:** **close-as-not-a-bug** — This is a usage question already answered in the comments. SkiaSharp behaves correctly; the blueish image is caused by a channel-swap bug in the reporter's own code.

---

## Classification

| Field | Value |
|-------|-------|
| Type | type/question |
| Area | area/SkiaSharp |
| Platforms | — |
| Backends | — |
| Tenets | — |
| Partner | — |

## Evidence

### Reproduction

**Environment:** FFmpeg / SIPSorceryMedia.Abstractions.RawImage as pixel source; no specific platform mentioned

**Repository links:**
- https://github.com/mono/SkiaSharp/issues/2412 — Same user asking nearly identical BGR24 conversion question (potential duplicate)
- https://github.com/mono/SkiaSharp/issues/2384#issuecomment-1620537730 — Community answer: don't swap channels — BGR24 and Bgra8888 share the same B,G,R byte order

**Code snippets:**

```csharp
// Reporter's buggy BGR24→BGRA32 loop (channels swapped incorrectly)
byte[] bgra32 = new byte[width * height * 4];
for (int y = 0; y < height; y++) {
    int bgr24RowIndex = y * stride;
    int bgra32RowIndex = y * width * 4;
    for (int x = 0; x < width; x++) {
        int bgr24Index = bgr24RowIndex + x * 3;
        int bgra32Index = bgra32RowIndex + x * 4;
        bgra32[bgra32Index] = bgr24[bgr24Index + 2]; // BUG: reads R into B slot
        bgra32[bgra32Index + 1] = bgr24[bgr24Index + 1];
        bgra32[bgra32Index + 2] = bgr24[bgr24Index]; // BUG: reads B into R slot
        bgra32[bgra32Index + 3] = 255;
    }
}
SKImageInfo info = new SKImageInfo(width, height, SKColorType.Bgra8888);
using var data = SKData.CreateCopy(bgra32);
using var image = SKImage.FromPixels(info, data, width * 4);
using var bitmap = SKBitmap.FromImage(image);
```

## Analysis

### Technical Summary

Reporter wants to load 24-bit BGR pixel data (AV_PIX_FMT_BGR24) from an FFmpeg video frame into an SKBitmap. SkiaSharp has no native 24-bit BGR color type, so data must be expanded to 32-bit (e.g. Bgra8888 or Rgb888x). The reporter's conversion has a channel-swap bug: reading R into the B slot and B into the R slot, causing the blueish tint. A community member already identified the fix (copy bytes without swapping). Related issue #2412 by the same user repeats the same question.

### Rationale

This is a usage question, not a SkiaSharp defect. The reporter's blueish-image issue is caused by incorrect channel ordering in their own conversion loop, not by an SkiaSharp bug. A community member already provided the correct answer in the comments. Issue #2412 from the same user is a near-duplicate of this question.

### Key Signals

- "8 bits for B, 8 bits for G, 8 bits for R (=> 24 bits for one pixel), it's size (width and height) and it's stride" — **issue body** (Reporter understands the source format — needs help mapping it to SkiaSharp's 32-bit color types.)
- "getting blueish color in image and taking to long to convert too" — **comment #1428242317** (Two issues: (1) channel-swap bug producing wrong colors; (2) performance concern with per-pixel managed loop.)
- "The result image is blueish because you swap blue and red channels." — **comment #1620537730** (Community member identified and fixed the bug: Bgra8888 stores bytes in B,G,R,A order, matching BGR24's B,G,R order — no channel swap is needed.)

### Code Investigation

| File | Lines | Relevance | Finding |
|------|-------|-----------|---------|
| `binding/SkiaSharp/Definitions.cs` | 36-102 | direct | SKColorType enum contains Bgra8888 (value=6, 4 bytes/pixel, byte order B[0],G[1],R[2],A[3]) and Rgb888x (value=5, 4 bytes/pixel, byte order R[0],G[1],B[2],X[3]). No native 24-bit BGR type exists; conversion from BGR24 to a 32-bit format is required. |
| `binding/SkiaSharp/SKBitmap.cs` | 591-615 | related | SKBitmap.InstallPixels(SKImageInfo, IntPtr, int rowBytes) allows loading raw pixel data from a pinned pointer without allocating a copy, which can improve performance for large frames compared to SKData.CreateCopy. |
| `binding/SkiaSharp/SKImage.cs` | 111-117 | direct | SKImage.FromPixels(SKImageInfo, SKData, int rowBytes) exists with the expected signature and passes rowBytes to the native layer. Can return null if the native call fails. |
| `binding/SkiaSharp/SKBitmap.cs` | 715-728 | related | SKBitmap.FromImage(SKImage) calls image.ReadPixels internally and returns null on failure. Callers should null-check the result. |

### Workarounds

- Fix the channel copy: since SKColorType.Bgra8888 uses byte order B,G,R,A and AV_PIX_FMT_BGR24 uses B,G,R — copy each byte directly without swapping: bgra32[i*4+0]=bgr24[i*3+0] (B), bgra32[i*4+1]=bgr24[i*3+1] (G), bgra32[i*4+2]=bgr24[i*3+2] (R), bgra32[i*4+3]=255.
- For better performance pin the destination array with GCHandle and use SKBitmap.InstallPixels to avoid SKData.CreateCopy allocation.

### Resolution Proposals

**Hypothesis:** Reporter's code swapped B and R channels during BGR24→BGRA32 expansion. The fix is to copy bytes in the same order (no swap) when targeting SKColorType.Bgra8888.

1. **Fix channel ordering — copy BGR24 bytes directly into Bgra8888 slots** — fix, confidence 0.95 (95%), cost/xs, validated=yes
   - SKColorType.Bgra8888 stores bytes in B,G,R,A order, which matches AV_PIX_FMT_BGR24's B,G,R layout. Copy each channel byte-for-byte without swapping and append A=255 for full opacity. Add null-checks for factory return values.

```csharp
byte[] bgra32 = new byte[width * height * 4];
for (int y = 0; y < height; y++) {
    int srcRow = y * stride;      // respects non-tight source padding
    int dstRow = y * width * 4;
    for (int x = 0; x < width; x++) {
        bgra32[dstRow + x * 4 + 0] = bgr24[srcRow + x * 3 + 0]; // B
        bgra32[dstRow + x * 4 + 1] = bgr24[srcRow + x * 3 + 1]; // G
        bgra32[dstRow + x * 4 + 2] = bgr24[srcRow + x * 3 + 2]; // R
        bgra32[dstRow + x * 4 + 3] = 255;                        // A (opaque)
    }
}
var info = new SKImageInfo(width, height, SKColorType.Bgra8888, SKAlphaType.Opaque);
using var data = SKData.CreateCopy(bgra32);
using var image = SKImage.FromPixels(info, data, width * 4)
    ?? throw new InvalidOperationException("Failed to create SKImage from pixel data.");
using var bitmap = SKBitmap.FromImage(image)
    ?? throw new InvalidOperationException("Failed to create SKBitmap from SKImage.");
```

**Recommended proposal:** Fix channel ordering — copy BGR24 bytes directly into Bgra8888 slots

**Why:** Minimal change to fix the blueish tint; byte order confirmed by code investigation and validated by parallel agents. Community member confirmed the same approach.

## Recommendations

### Actionability

| Field | Value |
|-------|-------|
| Suggested action | close-as-not-a-bug |
| Confidence | 0.88 (88%) |
| Reason | This is a usage question already answered in the comments. SkiaSharp behaves correctly; the blueish image is caused by a channel-swap bug in the reporter's own code. |
| Suggested repro platform | linux |

### Automatable Actions

| Type | Risk | Confidence | Description | Details |
|------|------|------------|-------------|---------|
| update-labels | low | 0.92 (92%) | Apply question type and SkiaSharp area labels | labels=type/question, area/SkiaSharp |
| add-comment | high | 0.88 (88%) | Post answer confirming the channel ordering fix with corrected code snippet | — |
| close-issue | medium | 0.85 (85%) | Close as answered — usage question, SkiaSharp behaves correctly | stateReason=not_planned |
| link-related | low | 0.92 (92%) | Link related issue #2412 (same user, same BGR24 conversion question) | linkedIssue=#2412 |

**Comment draft for `add-comment`:**

```markdown
Thanks for the detailed question!

The root issue is a **channel ordering mismatch**. `SKColorType.Bgra8888` stores bytes in **B→G→R→A** order, which already matches `AV_PIX_FMT_BGR24`'s **B→G→R** layout — so there is **no need to swap channels**. Your original code was accidentally reading B into the R slot and R into the B slot, causing the blue tint.

Here is the corrected conversion:

```csharp
byte[] bgra32 = new byte[width * height * 4];
for (int y = 0; y < height; y++) {
    int srcRow = y * stride;      // respects non-tight source padding
    int dstRow = y * width * 4;
    for (int x = 0; x < width; x++) {
        bgra32[dstRow + x * 4 + 0] = bgr24[srcRow + x * 3 + 0]; // B
        bgra32[dstRow + x * 4 + 1] = bgr24[srcRow + x * 3 + 1]; // G
        bgra32[dstRow + x * 4 + 2] = bgr24[srcRow + x * 3 + 2]; // R
        bgra32[dstRow + x * 4 + 3] = 255;                        // A (opaque)
    }
}
var info = new SKImageInfo(width, height, SKColorType.Bgra8888, SKAlphaType.Opaque);
using var data = SKData.CreateCopy(bgra32);
using var image = SKImage.FromPixels(info, data, width * 4)
    ?? throw new InvalidOperationException("Failed to create SKImage.");
using var bitmap = SKBitmap.FromImage(image)
    ?? throw new InvalidOperationException("Failed to create SKBitmap.");
```

This is a usage question rather than a SkiaSharp bug, so I will close this issue. See also #2412 which covers the same topic.
```

<details>
<summary>Raw JSON</summary>

```json
{
  "meta": {
    "schemaVersion": "1.0",
    "number": 2384,
    "repo": "mono/SkiaSharp",
    "analyzedAt": "2026-04-26T14:57:41Z"
  },
  "summary": "User asks how to convert AV_PIX_FMT_BGR24 (24-bit BGR pixel data from FFmpeg) to SKBitmap; reporter's conversion code produces a blueish tint due to an incorrect channel-swap in the BGR24→BGRA32 loop.",
  "classification": {
    "type": {
      "value": "type/question",
      "confidence": 0.92
    },
    "area": {
      "value": "area/SkiaSharp",
      "confidence": 0.9
    }
  },
  "evidence": {
    "reproEvidence": {
      "codeSnippets": [
        "// Reporter's buggy BGR24→BGRA32 loop (channels swapped incorrectly)\nbyte[] bgra32 = new byte[width * height * 4];\nfor (int y = 0; y < height; y++) {\n    int bgr24RowIndex = y * stride;\n    int bgra32RowIndex = y * width * 4;\n    for (int x = 0; x < width; x++) {\n        int bgr24Index = bgr24RowIndex + x * 3;\n        int bgra32Index = bgra32RowIndex + x * 4;\n        bgra32[bgra32Index] = bgr24[bgr24Index + 2]; // BUG: reads R into B slot\n        bgra32[bgra32Index + 1] = bgr24[bgr24Index + 1];\n        bgra32[bgra32Index + 2] = bgr24[bgr24Index]; // BUG: reads B into R slot\n        bgra32[bgra32Index + 3] = 255;\n    }\n}\nSKImageInfo info = new SKImageInfo(width, height, SKColorType.Bgra8888);\nusing var data = SKData.CreateCopy(bgra32);\nusing var image = SKImage.FromPixels(info, data, width * 4);\nusing var bitmap = SKBitmap.FromImage(image);"
      ],
      "repoLinks": [
        {
          "url": "https://github.com/mono/SkiaSharp/issues/2412",
          "description": "Same user asking nearly identical BGR24 conversion question (potential duplicate)"
        },
        {
          "url": "https://github.com/mono/SkiaSharp/issues/2384#issuecomment-1620537730",
          "description": "Community answer: don't swap channels — BGR24 and Bgra8888 share the same B,G,R byte order"
        }
      ],
      "environmentDetails": "FFmpeg / SIPSorceryMedia.Abstractions.RawImage as pixel source; no specific platform mentioned"
    }
  },
  "analysis": {
    "summary": "Reporter wants to load 24-bit BGR pixel data (AV_PIX_FMT_BGR24) from an FFmpeg video frame into an SKBitmap. SkiaSharp has no native 24-bit BGR color type, so data must be expanded to 32-bit (e.g. Bgra8888 or Rgb888x). The reporter's conversion has a channel-swap bug: reading R into the B slot and B into the R slot, causing the blueish tint. A community member already identified the fix (copy bytes without swapping). Related issue #2412 by the same user repeats the same question.",
    "codeInvestigation": [
      {
        "file": "binding/SkiaSharp/Definitions.cs",
        "lines": "36-102",
        "finding": "SKColorType enum contains Bgra8888 (value=6, 4 bytes/pixel, byte order B[0],G[1],R[2],A[3]) and Rgb888x (value=5, 4 bytes/pixel, byte order R[0],G[1],B[2],X[3]). No native 24-bit BGR type exists; conversion from BGR24 to a 32-bit format is required.",
        "relevance": "direct"
      },
      {
        "file": "binding/SkiaSharp/SKBitmap.cs",
        "lines": "591-615",
        "finding": "SKBitmap.InstallPixels(SKImageInfo, IntPtr, int rowBytes) allows loading raw pixel data from a pinned pointer without allocating a copy, which can improve performance for large frames compared to SKData.CreateCopy.",
        "relevance": "related"
      },
      {
        "file": "binding/SkiaSharp/SKImage.cs",
        "lines": "111-117",
        "finding": "SKImage.FromPixels(SKImageInfo, SKData, int rowBytes) exists with the expected signature and passes rowBytes to the native layer. Can return null if the native call fails.",
        "relevance": "direct"
      },
      {
        "file": "binding/SkiaSharp/SKBitmap.cs",
        "lines": "715-728",
        "finding": "SKBitmap.FromImage(SKImage) calls image.ReadPixels internally and returns null on failure. Callers should null-check the result.",
        "relevance": "related"
      }
    ],
    "keySignals": [
      {
        "text": "8 bits for B, 8 bits for G, 8 bits for R (=> 24 bits for one pixel), it's size (width and height) and it's stride",
        "source": "issue body",
        "interpretation": "Reporter understands the source format — needs help mapping it to SkiaSharp's 32-bit color types."
      },
      {
        "text": "getting blueish color in image and taking to long to convert too",
        "source": "comment #1428242317",
        "interpretation": "Two issues: (1) channel-swap bug producing wrong colors; (2) performance concern with per-pixel managed loop."
      },
      {
        "text": "The result image is blueish because you swap blue and red channels.",
        "source": "comment #1620537730",
        "interpretation": "Community member identified and fixed the bug: Bgra8888 stores bytes in B,G,R,A order, matching BGR24's B,G,R order — no channel swap is needed."
      }
    ],
    "workarounds": [
      "Fix the channel copy: since SKColorType.Bgra8888 uses byte order B,G,R,A and AV_PIX_FMT_BGR24 uses B,G,R — copy each byte directly without swapping: bgra32[i*4+0]=bgr24[i*3+0] (B), bgra32[i*4+1]=bgr24[i*3+1] (G), bgra32[i*4+2]=bgr24[i*3+2] (R), bgra32[i*4+3]=255.",
      "For better performance pin the destination array with GCHandle and use SKBitmap.InstallPixels to avoid SKData.CreateCopy allocation."
    ],
    "rationale": "This is a usage question, not a SkiaSharp defect. The reporter's blueish-image issue is caused by incorrect channel ordering in their own conversion loop, not by an SkiaSharp bug. A community member already provided the correct answer in the comments. Issue #2412 from the same user is a near-duplicate of this question.",
    "resolution": {
      "hypothesis": "Reporter's code swapped B and R channels during BGR24→BGRA32 expansion. The fix is to copy bytes in the same order (no swap) when targeting SKColorType.Bgra8888.",
      "proposals": [
        {
          "title": "Fix channel ordering — copy BGR24 bytes directly into Bgra8888 slots",
          "description": "SKColorType.Bgra8888 stores bytes in B,G,R,A order, which matches AV_PIX_FMT_BGR24's B,G,R layout. Copy each channel byte-for-byte without swapping and append A=255 for full opacity. Add null-checks for factory return values.",
          "category": "fix",
          "codeSnippet": "byte[] bgra32 = new byte[width * height * 4];\nfor (int y = 0; y < height; y++) {\n    int srcRow = y * stride;      // respects non-tight source padding\n    int dstRow = y * width * 4;\n    for (int x = 0; x < width; x++) {\n        bgra32[dstRow + x * 4 + 0] = bgr24[srcRow + x * 3 + 0]; // B\n        bgra32[dstRow + x * 4 + 1] = bgr24[srcRow + x * 3 + 1]; // G\n        bgra32[dstRow + x * 4 + 2] = bgr24[srcRow + x * 3 + 2]; // R\n        bgra32[dstRow + x * 4 + 3] = 255;                        // A (opaque)\n    }\n}\nvar info = new SKImageInfo(width, height, SKColorType.Bgra8888, SKAlphaType.Opaque);\nusing var data = SKData.CreateCopy(bgra32);\nusing var image = SKImage.FromPixels(info, data, width * 4)\n    ?? throw new InvalidOperationException(\"Failed to create SKImage from pixel data.\");\nusing var bitmap = SKBitmap.FromImage(image)\n    ?? throw new InvalidOperationException(\"Failed to create SKBitmap from SKImage.\");",
          "confidence": 0.95,
          "effort": "cost/xs",
          "validated": "yes"
        }
      ],
      "recommendedProposal": "Fix channel ordering — copy BGR24 bytes directly into Bgra8888 slots",
      "recommendedReason": "Minimal change to fix the blueish tint; byte order confirmed by code investigation and validated by parallel agents. Community member confirmed the same approach."
    }
  },
  "output": {
    "actionability": {
      "suggestedAction": "close-as-not-a-bug",
      "confidence": 0.88,
      "reason": "This is a usage question already answered in the comments. SkiaSharp behaves correctly; the blueish image is caused by a channel-swap bug in the reporter's own code.",
      "suggestedReproPlatform": "linux"
    },
    "actions": [
      {
        "type": "update-labels",
        "description": "Apply question type and SkiaSharp area labels",
        "risk": "low",
        "confidence": 0.92,
        "labels": [
          "type/question",
          "area/SkiaSharp"
        ]
      },
      {
        "type": "add-comment",
        "description": "Post answer confirming the channel ordering fix with corrected code snippet",
        "risk": "high",
        "confidence": 0.88,
        "comment": "Thanks for the detailed question!\n\nThe root issue is a **channel ordering mismatch**. `SKColorType.Bgra8888` stores bytes in **B→G→R→A** order, which already matches `AV_PIX_FMT_BGR24`'s **B→G→R** layout — so there is **no need to swap channels**. Your original code was accidentally reading B into the R slot and R into the B slot, causing the blue tint.\n\nHere is the corrected conversion:\n\n```csharp\nbyte[] bgra32 = new byte[width * height * 4];\nfor (int y = 0; y < height; y++) {\n    int srcRow = y * stride;      // respects non-tight source padding\n    int dstRow = y * width * 4;\n    for (int x = 0; x < width; x++) {\n        bgra32[dstRow + x * 4 + 0] = bgr24[srcRow + x * 3 + 0]; // B\n        bgra32[dstRow + x * 4 + 1] = bgr24[srcRow + x * 3 + 1]; // G\n        bgra32[dstRow + x * 4 + 2] = bgr24[srcRow + x * 3 + 2]; // R\n        bgra32[dstRow + x * 4 + 3] = 255;                        // A (opaque)\n    }\n}\nvar info = new SKImageInfo(width, height, SKColorType.Bgra8888, SKAlphaType.Opaque);\nusing var data = SKData.CreateCopy(bgra32);\nusing var image = SKImage.FromPixels(info, data, width * 4)\n    ?? throw new InvalidOperationException(\"Failed to create SKImage.\");\nusing var bitmap = SKBitmap.FromImage(image)\n    ?? throw new InvalidOperationException(\"Failed to create SKBitmap.\");\n```\n\nThis is a usage question rather than a SkiaSharp bug, so I will close this issue. See also #2412 which covers the same topic."
      },
      {
        "type": "close-issue",
        "description": "Close as answered — usage question, SkiaSharp behaves correctly",
        "risk": "medium",
        "confidence": 0.85,
        "stateReason": "not_planned"
      },
      {
        "type": "link-related",
        "description": "Link related issue #2412 (same user, same BGR24 conversion question)",
        "risk": "low",
        "confidence": 0.92,
        "linkedIssue": 2412
      }
    ]
  }
}
```

</details>
