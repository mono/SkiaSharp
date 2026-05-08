# Issue Triage Report — #1172

| Field | Value |
|-------|-------|
| Repository | mono/SkiaSharp |
| Analyzed | 2026-04-26T01:06:23Z |
| Type | type/question (0.90 (90%)) |
| Area | area/SkiaSharp (0.90 (90%)) |
| Suggested action | close-as-not-a-bug (0.85 (85%)) |

**Issue Summary:** Reporter asks how to decode all frames of an animated WebP efficiently via SKCodec, noting that sequential calls to GetPixels(frameIndex) exhibit O(n²) slowness because each frame re-decodes from the beginning.

**Analysis:** The reporter is not using SKCodecOptions.PriorFrame, which tells the Skia codec that the target bitmap already contains a prior decoded frame. Without this hint, every call must re-decode all dependent frames recursively, causing O(n²) cost. The fix is to check SKCodecFrameInfo.RequiredFrame, copy that prior bitmap into the new buffer, and pass the prior frame index via SKCodecOptions(frameIndex, priorFrame).

**Recommendations:** **close-as-not-a-bug** — The PriorFrame optimization path is already present in SKCodecOptions. The slow decode is caused by not using this hint — not by a missing feature or library bug.

---

## Classification

| Field | Value |
|-------|-------|
| Type | type/question |
| Area | area/SkiaSharp |
| Platforms | os/Windows-Universal-UWP |
| Backends | — |
| Tenets | — |
| Partner | — |

## Evidence

### Reproduction

1. Create an animated WebP with 77 frames at 480x258
2. Load via SKManagedStream + SKCodec.Create
3. Call GetPixels with SKCodecOptions(frameIndex) for each frame in a loop
4. Observe progressively slower decode times — 35 seconds total on i5 9600K

**Environment:** UWP, i5 9600K, animated WebP 77 frames @ 480x258

**Code snippets:**

```csharp
for (int i = 0; i < codec.FrameCount; i++) { ... codec.GetPixels(codec.Info, skBmp.GetPixels(), new SKCodecOptions(i)) ... }
```

### Version Analysis

| Field | Value |
|-------|-------|
| Mentioned versions | — |
| Worked in | — |
| Broke in | — |
| Current relevance | likely |
| Relevance reason | The SKCodec API (GetPixels, FrameInfo, SKCodecOptions.PriorFrame) has not changed in a way that breaks this optimization pattern; the feature request for SkAnimatedImage wrapping remains open. |

## Analysis

### Technical Summary

The reporter is not using SKCodecOptions.PriorFrame, which tells the Skia codec that the target bitmap already contains a prior decoded frame. Without this hint, every call must re-decode all dependent frames recursively, causing O(n²) cost. The fix is to check SKCodecFrameInfo.RequiredFrame, copy that prior bitmap into the new buffer, and pass the prior frame index via SKCodecOptions(frameIndex, priorFrame).

### Rationale

Title is tagged [QUESTION] and body explicitly asks 'Is there a better way?' This is a usage question, not a broken API. The SKCodecOptions struct already has the PriorFrame optimization hint via the constructor overload accepting frameIndex and priorFrame, and SKCodecFrameInfo exposes RequiredFrame. IncrementalDecode is for streaming partial data, not animation frame stepping, so its unimplemented status on WebP is expected and unrelated.

### Key Signals

- "extracting each frame means going over all the previous frames doing the same calculations over and over" — **issue body** (Classic symptom of not passing PriorFrame hint — codec re-decodes entire frame chain from scratch on each call.)
- "SKCodec.IncrementalDecode but they all return 'unimplemented' status code" — **issue body** (IncrementalDecode is for streaming partial data, not per-frame animation; expected to be unimplemented for WebP.)
- "You may need to check out src/android/SkAnimatedImage" — **comment by yaindrop (2023)** (Points to a Skia C++ API not yet exposed in SkiaSharp; the PriorFrame workaround achieves the same O(n) result with the current API.)
- "Having the same issue. Did you ever find a workaround to this?" — **comment by Echostorm44 (2025)** (Still relevant in 2025 — the PriorFrame pattern is not well known.)

### Code Investigation

| File | Lines | Relevance | Finding |
|------|-------|-----------|---------|
| `binding/SkiaSharp/SKCodec.cs` | 116-137 | direct | GetPixels(SKImageInfo, IntPtr, SKCodecOptions) passes fPriorFrame from SKCodecOptions directly to sk_codec_get_pixels — the optimization path is fully wired at the P/Invoke level. |
| `binding/SkiaSharp/Definitions.cs` | 253-258 | direct | SKCodecOptions(int frameIndex, int priorFrame) constructor exists and sets PriorFrame field — the API for efficient animation decoding is already present in the public surface. |
| `binding/SkiaSharp/SkiaApi.generated.cs` | 18226-18285 | direct | SKCodecFrameInfo struct exposes RequiredFrame (fRequiredFrame int field) which identifies the minimum prior frame index the codec needs already decoded in the destination buffer. |
| `binding/SkiaSharp/SKBitmap.cs` | 175-186 | related | SKBitmap.CopyTo(SKBitmap destination) is available to copy prior frame pixels into a new bitmap before decoding the next dependent frame. |

### Workarounds

- Use SKCodecOptions(frameIndex, priorFrame) overload and copy the required prior frame's pixels into the new bitmap before each GetPixels call, enabling O(n) forward decoding.

### Resolution Proposals

**Hypothesis:** Reporter is not using the PriorFrame hint in SKCodecOptions. Passing the required prior decoded frame index allows the codec to skip re-decoding that chain, eliminating O(n²) behavior.

1. **Use PriorFrame optimization in sequential decode loop** — workaround, confidence 0.88 (88%), cost/xs, validated=yes
   - Check SKCodecFrameInfo.RequiredFrame for each frame. If it requires a prior frame (value >= 0), copy that already-decoded bitmap into the new one first, then decode with SKCodecOptions(frameIndex, requiredFrame). This gives O(n) total decode time.

```csharp
var frameInfos = codec.FrameInfo;
var frameCount = codec.FrameCount;
var bitmaps = new SKBitmap[frameCount];

for (int i = 0; i < frameCount; i++)
{
    var bmp = new SKBitmap(codec.Info);
    var requiredFrame = frameInfos[i].RequiredFrame;

    // Populate buffer with the required prior frame before decoding
    if (requiredFrame >= 0 && bitmaps[requiredFrame] != null)
        bitmaps[requiredFrame].CopyTo(bmp);

    var options = new SKCodecOptions(i, requiredFrame);
    if (codec.GetPixels(codec.Info, bmp.GetPixels(), options) != SKCodecResult.Success)
        throw new Exception($"Unable to decode frame {i}.");

    bitmaps[i] = bmp;
}
```

**Recommended proposal:** Use PriorFrame optimization in sequential decode loop

**Why:** Directly addresses the O(n²) issue using the existing SkiaSharp API surface. No new features or workarounds needed.

## Recommendations

### Actionability

| Field | Value |
|-------|-------|
| Suggested action | close-as-not-a-bug |
| Confidence | 0.85 (85%) |
| Reason | The PriorFrame optimization path is already present in SKCodecOptions. The slow decode is caused by not using this hint — not by a missing feature or library bug. |
| Suggested repro platform | linux |

### Automatable Actions

| Type | Risk | Confidence | Description | Details |
|------|------|------------|-------------|---------|
| update-labels | low | 0.90 (90%) | Apply question, SkiaSharp core, and UWP platform labels | labels=type/question, area/SkiaSharp, os/Windows-Universal-UWP |
| add-comment | medium | 0.88 (88%) | Answer with PriorFrame usage pattern for efficient O(n) animated WebP decoding | — |
| close-issue | medium | 0.80 (80%) | Close as answered — the optimization API already exists in SKCodecOptions.PriorFrame | stateReason=completed |

**Comment draft for `add-comment`:**

```markdown
The performance issue comes from not passing the `PriorFrame` hint in `SKCodecOptions`. Without it, the codec re-decodes every prior frame from the beginning on each call — that's the O(n²) slowdown you're seeing.

Here's the fix using the existing API:

```csharp
var frameInfos = codec.FrameInfo;
var frameCount = codec.FrameCount;
var bitmaps = new SKBitmap[frameCount];

for (int i = 0; i < frameCount; i++)
{
    var bmp = new SKBitmap(codec.Info);
    var requiredFrame = frameInfos[i].RequiredFrame;

    // Populate buffer with the required prior frame before decoding
    if (requiredFrame >= 0 && bitmaps[requiredFrame] != null)
        bitmaps[requiredFrame].CopyTo(bmp);

    var options = new SKCodecOptions(i, requiredFrame);
    if (codec.GetPixels(codec.Info, bmp.GetPixels(), options) != SKCodecResult.Success)
        throw new Exception($"Unable to decode frame {i}.");

    bitmaps[i] = bmp;
}
```

`SKCodecFrameInfo.RequiredFrame` tells you the minimum frame index that must already be decoded into the destination buffer for the codec to skip re-calculating it. When you populate that buffer (via `CopyTo`) before calling `GetPixels`, each frame only processes its own delta — giving you O(n) total cost.

Regarding `IncrementalDecode`: that API is for streaming partial data (e.g. decoding a partially-downloaded image), not for stepping through animation frames. Its `Unimplemented` return on WebP is expected and unrelated to this issue.
```

<details>
<summary>Raw JSON</summary>

```json
{
  "meta": {
    "schemaVersion": "1.0",
    "number": 1172,
    "repo": "mono/SkiaSharp",
    "analyzedAt": "2026-04-26T01:06:23Z"
  },
  "summary": "Reporter asks how to decode all frames of an animated WebP efficiently via SKCodec, noting that sequential calls to GetPixels(frameIndex) exhibit O(n²) slowness because each frame re-decodes from the beginning.",
  "classification": {
    "type": {
      "value": "type/question",
      "confidence": 0.9
    },
    "area": {
      "value": "area/SkiaSharp",
      "confidence": 0.9
    },
    "platforms": [
      "os/Windows-Universal-UWP"
    ]
  },
  "evidence": {
    "reproEvidence": {
      "stepsToReproduce": [
        "Create an animated WebP with 77 frames at 480x258",
        "Load via SKManagedStream + SKCodec.Create",
        "Call GetPixels with SKCodecOptions(frameIndex) for each frame in a loop",
        "Observe progressively slower decode times — 35 seconds total on i5 9600K"
      ],
      "environmentDetails": "UWP, i5 9600K, animated WebP 77 frames @ 480x258",
      "codeSnippets": [
        "for (int i = 0; i < codec.FrameCount; i++) { ... codec.GetPixels(codec.Info, skBmp.GetPixels(), new SKCodecOptions(i)) ... }"
      ]
    },
    "versionAnalysis": {
      "mentionedVersions": [],
      "currentRelevance": "likely",
      "relevanceReason": "The SKCodec API (GetPixels, FrameInfo, SKCodecOptions.PriorFrame) has not changed in a way that breaks this optimization pattern; the feature request for SkAnimatedImage wrapping remains open."
    }
  },
  "analysis": {
    "summary": "The reporter is not using SKCodecOptions.PriorFrame, which tells the Skia codec that the target bitmap already contains a prior decoded frame. Without this hint, every call must re-decode all dependent frames recursively, causing O(n²) cost. The fix is to check SKCodecFrameInfo.RequiredFrame, copy that prior bitmap into the new buffer, and pass the prior frame index via SKCodecOptions(frameIndex, priorFrame).",
    "rationale": "Title is tagged [QUESTION] and body explicitly asks 'Is there a better way?' This is a usage question, not a broken API. The SKCodecOptions struct already has the PriorFrame optimization hint via the constructor overload accepting frameIndex and priorFrame, and SKCodecFrameInfo exposes RequiredFrame. IncrementalDecode is for streaming partial data, not animation frame stepping, so its unimplemented status on WebP is expected and unrelated.",
    "keySignals": [
      {
        "text": "extracting each frame means going over all the previous frames doing the same calculations over and over",
        "source": "issue body",
        "interpretation": "Classic symptom of not passing PriorFrame hint — codec re-decodes entire frame chain from scratch on each call."
      },
      {
        "text": "SKCodec.IncrementalDecode but they all return 'unimplemented' status code",
        "source": "issue body",
        "interpretation": "IncrementalDecode is for streaming partial data, not per-frame animation; expected to be unimplemented for WebP."
      },
      {
        "text": "You may need to check out src/android/SkAnimatedImage",
        "source": "comment by yaindrop (2023)",
        "interpretation": "Points to a Skia C++ API not yet exposed in SkiaSharp; the PriorFrame workaround achieves the same O(n) result with the current API."
      },
      {
        "text": "Having the same issue. Did you ever find a workaround to this?",
        "source": "comment by Echostorm44 (2025)",
        "interpretation": "Still relevant in 2025 — the PriorFrame pattern is not well known."
      }
    ],
    "codeInvestigation": [
      {
        "file": "binding/SkiaSharp/SKCodec.cs",
        "lines": "116-137",
        "finding": "GetPixels(SKImageInfo, IntPtr, SKCodecOptions) passes fPriorFrame from SKCodecOptions directly to sk_codec_get_pixels — the optimization path is fully wired at the P/Invoke level.",
        "relevance": "direct"
      },
      {
        "file": "binding/SkiaSharp/Definitions.cs",
        "lines": "253-258",
        "finding": "SKCodecOptions(int frameIndex, int priorFrame) constructor exists and sets PriorFrame field — the API for efficient animation decoding is already present in the public surface.",
        "relevance": "direct"
      },
      {
        "file": "binding/SkiaSharp/SkiaApi.generated.cs",
        "lines": "18226-18285",
        "finding": "SKCodecFrameInfo struct exposes RequiredFrame (fRequiredFrame int field) which identifies the minimum prior frame index the codec needs already decoded in the destination buffer.",
        "relevance": "direct"
      },
      {
        "file": "binding/SkiaSharp/SKBitmap.cs",
        "lines": "175-186",
        "finding": "SKBitmap.CopyTo(SKBitmap destination) is available to copy prior frame pixels into a new bitmap before decoding the next dependent frame.",
        "relevance": "related"
      }
    ],
    "workarounds": [
      "Use SKCodecOptions(frameIndex, priorFrame) overload and copy the required prior frame's pixels into the new bitmap before each GetPixels call, enabling O(n) forward decoding."
    ],
    "resolution": {
      "hypothesis": "Reporter is not using the PriorFrame hint in SKCodecOptions. Passing the required prior decoded frame index allows the codec to skip re-decoding that chain, eliminating O(n²) behavior.",
      "proposals": [
        {
          "title": "Use PriorFrame optimization in sequential decode loop",
          "description": "Check SKCodecFrameInfo.RequiredFrame for each frame. If it requires a prior frame (value >= 0), copy that already-decoded bitmap into the new one first, then decode with SKCodecOptions(frameIndex, requiredFrame). This gives O(n) total decode time.",
          "category": "workaround",
          "codeSnippet": "var frameInfos = codec.FrameInfo;\nvar frameCount = codec.FrameCount;\nvar bitmaps = new SKBitmap[frameCount];\n\nfor (int i = 0; i < frameCount; i++)\n{\n    var bmp = new SKBitmap(codec.Info);\n    var requiredFrame = frameInfos[i].RequiredFrame;\n\n    // Populate buffer with the required prior frame before decoding\n    if (requiredFrame >= 0 && bitmaps[requiredFrame] != null)\n        bitmaps[requiredFrame].CopyTo(bmp);\n\n    var options = new SKCodecOptions(i, requiredFrame);\n    if (codec.GetPixels(codec.Info, bmp.GetPixels(), options) != SKCodecResult.Success)\n        throw new Exception($\"Unable to decode frame {i}.\");\n\n    bitmaps[i] = bmp;\n}",
          "confidence": 0.88,
          "effort": "cost/xs",
          "validated": "yes"
        }
      ],
      "recommendedProposal": "Use PriorFrame optimization in sequential decode loop",
      "recommendedReason": "Directly addresses the O(n²) issue using the existing SkiaSharp API surface. No new features or workarounds needed."
    }
  },
  "output": {
    "actionability": {
      "suggestedAction": "close-as-not-a-bug",
      "confidence": 0.85,
      "reason": "The PriorFrame optimization path is already present in SKCodecOptions. The slow decode is caused by not using this hint — not by a missing feature or library bug.",
      "suggestedReproPlatform": "linux"
    },
    "actions": [
      {
        "type": "update-labels",
        "description": "Apply question, SkiaSharp core, and UWP platform labels",
        "risk": "low",
        "confidence": 0.9,
        "labels": [
          "type/question",
          "area/SkiaSharp",
          "os/Windows-Universal-UWP"
        ]
      },
      {
        "type": "add-comment",
        "description": "Answer with PriorFrame usage pattern for efficient O(n) animated WebP decoding",
        "risk": "medium",
        "confidence": 0.88,
        "comment": "The performance issue comes from not passing the `PriorFrame` hint in `SKCodecOptions`. Without it, the codec re-decodes every prior frame from the beginning on each call — that's the O(n²) slowdown you're seeing.\n\nHere's the fix using the existing API:\n\n```csharp\nvar frameInfos = codec.FrameInfo;\nvar frameCount = codec.FrameCount;\nvar bitmaps = new SKBitmap[frameCount];\n\nfor (int i = 0; i < frameCount; i++)\n{\n    var bmp = new SKBitmap(codec.Info);\n    var requiredFrame = frameInfos[i].RequiredFrame;\n\n    // Populate buffer with the required prior frame before decoding\n    if (requiredFrame >= 0 && bitmaps[requiredFrame] != null)\n        bitmaps[requiredFrame].CopyTo(bmp);\n\n    var options = new SKCodecOptions(i, requiredFrame);\n    if (codec.GetPixels(codec.Info, bmp.GetPixels(), options) != SKCodecResult.Success)\n        throw new Exception($\"Unable to decode frame {i}.\");\n\n    bitmaps[i] = bmp;\n}\n```\n\n`SKCodecFrameInfo.RequiredFrame` tells you the minimum frame index that must already be decoded into the destination buffer for the codec to skip re-calculating it. When you populate that buffer (via `CopyTo`) before calling `GetPixels`, each frame only processes its own delta — giving you O(n) total cost.\n\nRegarding `IncrementalDecode`: that API is for streaming partial data (e.g. decoding a partially-downloaded image), not for stepping through animation frames. Its `Unimplemented` return on WebP is expected and unrelated to this issue."
      },
      {
        "type": "close-issue",
        "description": "Close as answered — the optimization API already exists in SKCodecOptions.PriorFrame",
        "risk": "medium",
        "confidence": 0.8,
        "stateReason": "completed"
      }
    ]
  }
}
```

</details>
