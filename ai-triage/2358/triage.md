# Issue Triage Report — #2358

| Field | Value |
|-------|-------|
| Repository | mono/SkiaSharp |
| Analyzed | 2026-04-29T21:09:00Z |
| Type | type/question (0.95 (95%)) |
| Area | area/SkiaSharp (0.90 (90%)) |
| Suggested action | close-as-not-a-bug (0.90 (90%)) |

**Issue Summary:** Reporter asks why writing pixel colors directly to a bitmap span produces moiré patterns compared to using DrawCircle, seeking help with correct span/bitmap usage.

**Analysis:** The moiré patterns stem from user code bugs: Method 3 omits midx/midy center offset when converting polar to Cartesian coordinates, placing the spoke at (0,0) instead of the bitmap center. Method 2 suffers from rotation aliasing (a 1-pixel-wide rotated bitmap cannot perfectly fill adjacent spoke angles). These are usage errors, not SkiaSharp defects.

**Recommendations:** **close-as-not-a-bug** — Usage question with identifiable code bugs in the reporter's implementation. No SkiaSharp defect. A helpful answer with the specific code fixes can resolve this.

---

## Classification

| Field | Value |
|-------|-------|
| Type | type/question |
| Area | area/SkiaSharp |
| Platforms | os/Android, os/iOS |
| Backends | — |
| Tenets | — |
| Partner | — |

## Evidence

### Reproduction

1. Method 1 (DrawCircle): Iterates over spoke data and calls DrawCircle with float x/y coordinates — works correctly
2. Method 2 (1x1024 bitmap + RotateDegrees): Writes pixels to 1024x1 bitmap span then rotates/translates canvas — produces moiré
3. Method 3 (full 1024x1024 span): Writes pixels by computing integer x/y from polar coordinates — produces moiré

**Environment:** SkiaSharp 2.88.3, Android and iOS

**Repository links:**
- https://user-images.githubusercontent.com/4975613/211543543-de282c1c-00af-4631-acd7-2903278a22a2.png — Screenshot: Method 1 DrawCircle (correct grey display)
- https://user-images.githubusercontent.com/4975613/211543606-222f3c3f-b5a4-4eea-9bd6-2b59373452b2.png — Screenshot: Method 2 rotate transform (moiré pattern)
- https://user-images.githubusercontent.com/4975613/211543656-53f92292-ac8f-4e80-bad3-27cc0b8197e4.png — Screenshot: Method 3 direct span (moiré pattern)

### Version Analysis

| Field | Value |
|-------|-------|
| Mentioned versions | 2.88.3 |
| Worked in | — |
| Broke in | — |
| Current relevance | unlikely |
| Relevance reason | The reported problem is user code errors, not a SkiaSharp defect. The API has not changed in ways that would affect this. |

## Analysis

### Technical Summary

The moiré patterns stem from user code bugs: Method 3 omits midx/midy center offset when converting polar to Cartesian coordinates, placing the spoke at (0,0) instead of the bitmap center. Method 2 suffers from rotation aliasing (a 1-pixel-wide rotated bitmap cannot perfectly fill adjacent spoke angles). These are usage errors, not SkiaSharp defects.

### Rationale

Reporter explicitly asks 'Any idea of what I do wrong?' — this is a usage question. Code investigation reveals concrete bugs in the user's code: missing center offset in Method 3, and expected rotation aliasing artifacts in Method 2. The DrawCircle method works because it uses floating-point coordinates with anti-aliased rendering. No SkiaSharp API defects were found.

### Key Signals

- "Any idea of what i do wrong?" — **issue body** (Reporter is asking for help, not reporting a SkiaSharp defect.)
- "int x = (int)(radcos * r); int y = (int)(radsin * r);" — **issue body — Method 3 code** (midx and midy center offsets are computed but never added, placing spoke at origin (0,0) instead of center (512,512).)
- "inCanvas.Translate(new SKPoint(512, 512)); inCanvas.RotateDegrees(drawdata.degree); inCanvas.DrawBitmap(lokBmp, 0, 0);" — **issue body — Method 2 code** (Rotating a 1px-wide bitmap causes aliasing gaps between adjacent spokes — expected rasterization behavior, not a bug.)

### Code Investigation

| File | Lines | Relevance | Finding |
|------|-------|-----------|---------|
| `binding/SkiaSharp/SKColor.cs` | 19-21 | context | SKColor stores color as uint: (alpha<<24)|(red<<16)|(green<<8)|blue. On little-endian, memory bytes are [Blue, Green, Red, Alpha], matching Bgra8888. So writing SKColor directly to a Bgra8888 span is byte-layout correct. |
| `binding/SkiaSharp/SKPixmap.cs` | 129-171 | context | GetPixelSpan<T>() validates sizeof(T) == bytesPerPixel; for SKColor (4 bytes) on Bgra8888 (4 bpp) this passes. The span points directly to native pixel memory — correct for direct writes. |
| `binding/SkiaSharp/SKBitmap.cs` | 300-304 | context | SKBitmap.GetPixelSpan() returns Span<byte> over raw pixel memory. The API is correct for direct pixel manipulation when byte layout is understood. |

### Workarounds

- Method 3 fix: add midx/midy to the x/y calculation — int x = (int)(midx + radcos * r); int y = (int)(midy + radsin * r);
- Method 2 fix: use DrawBitmap with anti-aliased paint (paint.IsAntialias = true) to reduce aliasing when rotating thin bitmaps; or use an SKPaint with FilterQuality set.
- Alternative: Keep Method 1 but optimize by reducing DrawCircle calls with a batch approach, or draw into an off-screen bitmap using canvas operations then blit once.

### Resolution Proposals

**Hypothesis:** Method 3 has a missing center offset (midx/midy not added). Method 2 is affected by expected aliasing when rotating a 1px-wide bitmap. Neither is a SkiaSharp bug.

1. **Fix center offset in Method 3** — fix, confidence 0.95 (95%), cost/xs, validated=yes
   - Add the center coordinates when converting polar to Cartesian in the span-based method.

```csharp
int x = (int)(midx + radcos * r);
int y = (int)(midy + radsin * r);
int spanplac = y * 1024 + x;
```
2. **Use anti-aliased rendering in Method 2** — workaround, confidence 0.80 (80%), cost/xs, validated=yes
   - When drawing the rotated bitmap, pass a paint with IsAntialias=true to reduce aliasing from rotation.

```csharp
var bitmapPaint = new SKPaint { IsAntialias = true, FilterQuality = SKFilterQuality.High };
inCanvas.DrawBitmap(lokBmp, 0, 0, bitmapPaint);
```

**Recommended proposal:** Fix center offset in Method 3

**Why:** The missing midx/midy offset is a clear code bug and the primary cause of Method 3's incorrect output.

## Recommendations

### Actionability

| Field | Value |
|-------|-------|
| Suggested action | close-as-not-a-bug |
| Confidence | 0.90 (90%) |
| Reason | Usage question with identifiable code bugs in the reporter's implementation. No SkiaSharp defect. A helpful answer with the specific code fixes can resolve this. |
| Suggested repro platform | linux |

### Automatable Actions

| Type | Risk | Confidence | Description | Details |
|------|------|------------|-------------|---------|
| update-labels | low | 0.95 (95%) | Apply question and platform labels | labels=type/question, area/SkiaSharp, os/Android, os/iOS |
| add-comment | high | 0.90 (90%) | Post answer explaining the two code bugs and their fixes | — |
| close-issue | medium | 0.85 (85%) | Close as answered — usage question with clear code fix | stateReason=completed |

**Comment draft for `add-comment`:**

```markdown
Thanks for the detailed examples and screenshots!

After reviewing your three methods, the moiré patterns come from two separate issues in your code — not from SkiaSharp itself.

**Method 3 (direct span) — missing center offset**

You compute `midx` and `midy`, but never use them when calculating the pixel position:

```csharp
// Current (broken): spoke centered at (0, 0)
int x = (int)(radcos * r);
int y = (int)(radsin * r);

// Fixed: add center offset
int x = (int)(midx + radcos * r);
int y = (int)(midy + radsin * r);
int spanplac = y * 1024 + x;
```

**Method 2 (rotate transform) — rotation aliasing**

When you rotate a 1-pixel-wide bitmap, the rasterizer cannot perfectly fill adjacent spoke angles without leaving gaps. You can reduce this by using an anti-aliased paint:

```csharp
var bitmapPaint = new SKPaint { IsAntialias = true, FilterQuality = SKFilterQuality.High };
inCanvas.DrawBitmap(lokBmp, 0, 0, bitmapPaint);
```

**Note on Method 2 memory:** `lokBmp` is being created as a `new SKBitmap` inside the loop on every dequeue — it should be allocated once outside the `while` loop and reused (or disposed after the loop).

The color byte layout (`SKColor` as `Span<SKColor>` over a `Bgra8888` bitmap) is correct — `SKColor` stores bytes as BGRA on little-endian, matching `Bgra8888`.
```

<details>
<summary>Raw JSON</summary>

```json
{
  "meta": {
    "schemaVersion": "1.0",
    "number": 2358,
    "repo": "mono/SkiaSharp",
    "analyzedAt": "2026-04-29T21:09:00Z"
  },
  "summary": "Reporter asks why writing pixel colors directly to a bitmap span produces moiré patterns compared to using DrawCircle, seeking help with correct span/bitmap usage.",
  "classification": {
    "type": {
      "value": "type/question",
      "confidence": 0.95
    },
    "area": {
      "value": "area/SkiaSharp",
      "confidence": 0.9
    },
    "platforms": [
      "os/Android",
      "os/iOS"
    ]
  },
  "evidence": {
    "reproEvidence": {
      "stepsToReproduce": [
        "Method 1 (DrawCircle): Iterates over spoke data and calls DrawCircle with float x/y coordinates — works correctly",
        "Method 2 (1x1024 bitmap + RotateDegrees): Writes pixels to 1024x1 bitmap span then rotates/translates canvas — produces moiré",
        "Method 3 (full 1024x1024 span): Writes pixels by computing integer x/y from polar coordinates — produces moiré"
      ],
      "environmentDetails": "SkiaSharp 2.88.3, Android and iOS",
      "repoLinks": [
        {
          "url": "https://user-images.githubusercontent.com/4975613/211543543-de282c1c-00af-4631-acd7-2903278a22a2.png",
          "description": "Screenshot: Method 1 DrawCircle (correct grey display)"
        },
        {
          "url": "https://user-images.githubusercontent.com/4975613/211543606-222f3c3f-b5a4-4eea-9bd6-2b59373452b2.png",
          "description": "Screenshot: Method 2 rotate transform (moiré pattern)"
        },
        {
          "url": "https://user-images.githubusercontent.com/4975613/211543656-53f92292-ac8f-4e80-bad3-27cc0b8197e4.png",
          "description": "Screenshot: Method 3 direct span (moiré pattern)"
        }
      ]
    },
    "versionAnalysis": {
      "mentionedVersions": [
        "2.88.3"
      ],
      "currentRelevance": "unlikely",
      "relevanceReason": "The reported problem is user code errors, not a SkiaSharp defect. The API has not changed in ways that would affect this."
    }
  },
  "analysis": {
    "summary": "The moiré patterns stem from user code bugs: Method 3 omits midx/midy center offset when converting polar to Cartesian coordinates, placing the spoke at (0,0) instead of the bitmap center. Method 2 suffers from rotation aliasing (a 1-pixel-wide rotated bitmap cannot perfectly fill adjacent spoke angles). These are usage errors, not SkiaSharp defects.",
    "rationale": "Reporter explicitly asks 'Any idea of what I do wrong?' — this is a usage question. Code investigation reveals concrete bugs in the user's code: missing center offset in Method 3, and expected rotation aliasing artifacts in Method 2. The DrawCircle method works because it uses floating-point coordinates with anti-aliased rendering. No SkiaSharp API defects were found.",
    "keySignals": [
      {
        "text": "Any idea of what i do wrong?",
        "source": "issue body",
        "interpretation": "Reporter is asking for help, not reporting a SkiaSharp defect."
      },
      {
        "text": "int x = (int)(radcos * r); int y = (int)(radsin * r);",
        "source": "issue body — Method 3 code",
        "interpretation": "midx and midy center offsets are computed but never added, placing spoke at origin (0,0) instead of center (512,512)."
      },
      {
        "text": "inCanvas.Translate(new SKPoint(512, 512)); inCanvas.RotateDegrees(drawdata.degree); inCanvas.DrawBitmap(lokBmp, 0, 0);",
        "source": "issue body — Method 2 code",
        "interpretation": "Rotating a 1px-wide bitmap causes aliasing gaps between adjacent spokes — expected rasterization behavior, not a bug."
      }
    ],
    "codeInvestigation": [
      {
        "file": "binding/SkiaSharp/SKColor.cs",
        "lines": "19-21",
        "finding": "SKColor stores color as uint: (alpha<<24)|(red<<16)|(green<<8)|blue. On little-endian, memory bytes are [Blue, Green, Red, Alpha], matching Bgra8888. So writing SKColor directly to a Bgra8888 span is byte-layout correct.",
        "relevance": "context"
      },
      {
        "file": "binding/SkiaSharp/SKPixmap.cs",
        "lines": "129-171",
        "finding": "GetPixelSpan<T>() validates sizeof(T) == bytesPerPixel; for SKColor (4 bytes) on Bgra8888 (4 bpp) this passes. The span points directly to native pixel memory — correct for direct writes.",
        "relevance": "context"
      },
      {
        "file": "binding/SkiaSharp/SKBitmap.cs",
        "lines": "300-304",
        "finding": "SKBitmap.GetPixelSpan() returns Span<byte> over raw pixel memory. The API is correct for direct pixel manipulation when byte layout is understood.",
        "relevance": "context"
      }
    ],
    "workarounds": [
      "Method 3 fix: add midx/midy to the x/y calculation — int x = (int)(midx + radcos * r); int y = (int)(midy + radsin * r);",
      "Method 2 fix: use DrawBitmap with anti-aliased paint (paint.IsAntialias = true) to reduce aliasing when rotating thin bitmaps; or use an SKPaint with FilterQuality set.",
      "Alternative: Keep Method 1 but optimize by reducing DrawCircle calls with a batch approach, or draw into an off-screen bitmap using canvas operations then blit once."
    ],
    "resolution": {
      "hypothesis": "Method 3 has a missing center offset (midx/midy not added). Method 2 is affected by expected aliasing when rotating a 1px-wide bitmap. Neither is a SkiaSharp bug.",
      "proposals": [
        {
          "title": "Fix center offset in Method 3",
          "description": "Add the center coordinates when converting polar to Cartesian in the span-based method.",
          "category": "fix",
          "codeSnippet": "int x = (int)(midx + radcos * r);\nint y = (int)(midy + radsin * r);\nint spanplac = y * 1024 + x;",
          "confidence": 0.95,
          "effort": "cost/xs",
          "validated": "yes"
        },
        {
          "title": "Use anti-aliased rendering in Method 2",
          "description": "When drawing the rotated bitmap, pass a paint with IsAntialias=true to reduce aliasing from rotation.",
          "category": "workaround",
          "codeSnippet": "var bitmapPaint = new SKPaint { IsAntialias = true, FilterQuality = SKFilterQuality.High };\ninCanvas.DrawBitmap(lokBmp, 0, 0, bitmapPaint);",
          "confidence": 0.8,
          "effort": "cost/xs",
          "validated": "yes"
        }
      ],
      "recommendedProposal": "Fix center offset in Method 3",
      "recommendedReason": "The missing midx/midy offset is a clear code bug and the primary cause of Method 3's incorrect output."
    }
  },
  "output": {
    "actionability": {
      "suggestedAction": "close-as-not-a-bug",
      "confidence": 0.9,
      "reason": "Usage question with identifiable code bugs in the reporter's implementation. No SkiaSharp defect. A helpful answer with the specific code fixes can resolve this.",
      "suggestedReproPlatform": "linux"
    },
    "actions": [
      {
        "type": "update-labels",
        "description": "Apply question and platform labels",
        "risk": "low",
        "confidence": 0.95,
        "labels": [
          "type/question",
          "area/SkiaSharp",
          "os/Android",
          "os/iOS"
        ]
      },
      {
        "type": "add-comment",
        "description": "Post answer explaining the two code bugs and their fixes",
        "risk": "high",
        "confidence": 0.9,
        "comment": "Thanks for the detailed examples and screenshots!\n\nAfter reviewing your three methods, the moiré patterns come from two separate issues in your code — not from SkiaSharp itself.\n\n**Method 3 (direct span) — missing center offset**\n\nYou compute `midx` and `midy`, but never use them when calculating the pixel position:\n\n```csharp\n// Current (broken): spoke centered at (0, 0)\nint x = (int)(radcos * r);\nint y = (int)(radsin * r);\n\n// Fixed: add center offset\nint x = (int)(midx + radcos * r);\nint y = (int)(midy + radsin * r);\nint spanplac = y * 1024 + x;\n```\n\n**Method 2 (rotate transform) — rotation aliasing**\n\nWhen you rotate a 1-pixel-wide bitmap, the rasterizer cannot perfectly fill adjacent spoke angles without leaving gaps. You can reduce this by using an anti-aliased paint:\n\n```csharp\nvar bitmapPaint = new SKPaint { IsAntialias = true, FilterQuality = SKFilterQuality.High };\ninCanvas.DrawBitmap(lokBmp, 0, 0, bitmapPaint);\n```\n\n**Note on Method 2 memory:** `lokBmp` is being created as a `new SKBitmap` inside the loop on every dequeue — it should be allocated once outside the `while` loop and reused (or disposed after the loop).\n\nThe color byte layout (`SKColor` as `Span<SKColor>` over a `Bgra8888` bitmap) is correct — `SKColor` stores bytes as BGRA on little-endian, matching `Bgra8888`."
      },
      {
        "type": "close-issue",
        "description": "Close as answered — usage question with clear code fix",
        "risk": "medium",
        "confidence": 0.85,
        "stateReason": "completed"
      }
    ]
  }
}
```

</details>
