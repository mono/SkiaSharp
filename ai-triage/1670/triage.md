# Issue Triage Report — #1670

| Field | Value |
|-------|-------|
| Repository | mono/SkiaSharp |
| Analyzed | 2026-04-30T20:32:22Z |
| Type | type/question (0.95 (95%)) |
| Area | area/SkiaSharp (0.90 (90%)) |
| Suggested action | close-as-not-a-bug (0.90 (90%)) |

**Issue Summary:** User asking how to crop/subset a rectangular region from an image in SkiaSharp, providing a System.Drawing (GDI+) equivalent as reference.

**Analysis:** The reporter wants to crop a rectangular region from an image, equivalent to System.Drawing's Graphics.DrawImage(source, destRect, srcRect, GraphicsUnit.Pixel). SkiaSharp provides this via SKBitmap.ExtractSubset(), SKImage.Subset(), and SKCanvas.DrawBitmap/DrawImage with source+dest rectangles.

**Recommendations:** **close-as-not-a-bug** — This is a usage question. The required API exists and works — SKBitmap.ExtractSubset and SKImage.Subset both provide image cropping. A direct answer with code example resolves this.

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

**Environment:** No version or platform specified

**Code snippets:**

```csharp
System.Drawing equivalent using Graphics.DrawImage with source and dest rectangles to extract a rectangular region from a Bitmap
```

## Analysis

### Technical Summary

The reporter wants to crop a rectangular region from an image, equivalent to System.Drawing's Graphics.DrawImage(source, destRect, srcRect, GraphicsUnit.Pixel). SkiaSharp provides this via SKBitmap.ExtractSubset(), SKImage.Subset(), and SKCanvas.DrawBitmap/DrawImage with source+dest rectangles.

### Rationale

The issue is a usage question — the user is porting from System.Drawing and doesn't know the SkiaSharp equivalent API. The functionality exists in SkiaSharp through multiple APIs (ExtractSubset, Subset, DrawBitmap with source/dest). No bug is described; no broken behavior exists. This is a 'how to do X' question with a clear, demonstrable answer.

### Key Signals

- "How should I cut the stream of the picture? Similar to the code under net core?" — **issue title and body** (Porting question — user wants the SkiaSharp equivalent of System.Drawing image cropping.)
- "grSmall.DrawImage(source, new System.Drawing.Rectangle(0, 0, bmSmall.Width, bmSmall.Height), rect, GraphicsUnit.Pixel)" — **issue body code snippet** (Classic GDI+ crop pattern — draw a source rect into a destination rect. SkiaSharp has direct equivalents.)

### Code Investigation

| File | Lines | Relevance | Finding |
|------|-------|-----------|---------|
| `binding/SkiaSharp/SKBitmap.cs` | 216-227 | direct | SKBitmap.ExtractSubset(SKBitmap destination, SKRectI subset) extracts a rectangular subset of the bitmap into a new destination bitmap — direct equivalent to the reported use case. |
| `binding/SkiaSharp/SKImage.cs` | 544-549 | direct | SKImage.Subset(SKRectI subset) returns a new SKImage containing only the specified rectangle — another direct approach for image cropping. |
| `binding/SkiaSharp/SKCanvas.cs` | 493-502 | related | SKCanvas.DrawBitmap(bitmap, SKRect source, SKRect dest) and DrawImage(image, SKRect source, SKRect dest) allow drawing a source rectangle into a destination rectangle, mirroring Graphics.DrawImage behavior. |

### Workarounds

- Use SKBitmap.ExtractSubset(destination, new SKRectI(x, y, x+width, y+height)) to extract a cropped bitmap.
- Use SKImage.Subset(new SKRectI(x, y, x+width, y+height)) when working with SKImage.
- Use SKCanvas.DrawBitmap(bitmap, sourceRect, destRect) to draw only a portion of the source image.

### Resolution Proposals

**Hypothesis:** The reporter needs to know the SkiaSharp API for image cropping. SKBitmap.ExtractSubset is the closest System.Drawing equivalent.

1. **Use SKBitmap.ExtractSubset** — fix, confidence 0.92 (92%), cost/xs, validated=yes
   - Extract a rectangular crop using SKBitmap.ExtractSubset into a new destination bitmap. This is the most direct SkiaSharp equivalent of the System.Drawing pattern shown.

```csharp
public static SKBitmap CropBitmap(SKBitmap source, SKRectI rect)
{
    var destination = new SKBitmap(rect.Width, rect.Height);
    source.ExtractSubset(destination, rect);
    return destination;
}
```
2. **Use SKImage.Subset** — alternative, confidence 0.90 (90%), cost/xs, validated=yes
   - If working with SKImage rather than SKBitmap, use SKImage.Subset(SKRectI) to obtain a cropped image.

```csharp
using var image = SKImage.FromBitmap(sourceBitmap);
using var cropped = image.Subset(new SKRectI(x, y, x + width, y + height));
```

**Recommended proposal:** Use SKBitmap.ExtractSubset

**Why:** Closest to the System.Drawing pattern shown and works directly with SKBitmap without conversion overhead.

## Recommendations

### Actionability

| Field | Value |
|-------|-------|
| Suggested action | close-as-not-a-bug |
| Confidence | 0.90 (90%) |
| Reason | This is a usage question. The required API exists and works — SKBitmap.ExtractSubset and SKImage.Subset both provide image cropping. A direct answer with code example resolves this. |
| Suggested repro platform | linux |

### Automatable Actions

| Type | Risk | Confidence | Description | Details |
|------|------|------------|-------------|---------|
| update-labels | low | 0.95 (95%) | Apply question and SkiaSharp labels | labels=type/question, area/SkiaSharp |
| add-comment | high | 0.90 (90%) | Post answer with SkiaSharp crop API equivalents | — |
| close-issue | medium | 0.85 (85%) | Close as answered | stateReason=completed |

**Comment draft for `add-comment`:**

```markdown
Hi @bbhxwl! To crop a rectangular region from an image in SkiaSharp, you can use `SKBitmap.ExtractSubset` or `SKImage.Subset`.

**Option 1 — `SKBitmap.ExtractSubset` (closest to System.Drawing):**
```csharp
public static SKBitmap AcquireRectangleImage(SKBitmap source, SKRectI rect)
{
    if (source == null || rect.IsEmpty) return null;
    var destination = new SKBitmap(rect.Width, rect.Height);
    source.ExtractSubset(destination, rect);
    return destination;
}
```

**Option 2 — `SKImage.Subset`:**
```csharp
using var image = SKImage.FromBitmap(sourceBitmap);
using var cropped = image.Subset(new SKRectI(x, y, x + width, y + height));
```

**Option 3 — Draw source rect to dest rect (like `Graphics.DrawImage`):**
```csharp
var info = new SKImageInfo(rect.Width, rect.Height);
using var surface = SKSurface.Create(info);
var destRect = new SKRect(0, 0, rect.Width, rect.Height);
var sourceRect = new SKRect(rect.Left, rect.Top, rect.Right, rect.Bottom);
surface.Canvas.DrawBitmap(source, sourceRect, destRect);
using var result = surface.Snapshot();
```

Note: `SKRectI` coordinates are `(left, top, right, bottom)`, not `(x, y, width, height)`.
```

<details>
<summary>Raw JSON</summary>

```json
{
  "meta": {
    "schemaVersion": "1.0",
    "number": 1670,
    "repo": "mono/SkiaSharp",
    "analyzedAt": "2026-04-30T20:32:22Z"
  },
  "summary": "User asking how to crop/subset a rectangular region from an image in SkiaSharp, providing a System.Drawing (GDI+) equivalent as reference.",
  "classification": {
    "type": {
      "value": "type/question",
      "confidence": 0.95
    },
    "area": {
      "value": "area/SkiaSharp",
      "confidence": 0.9
    }
  },
  "evidence": {
    "reproEvidence": {
      "codeSnippets": [
        "System.Drawing equivalent using Graphics.DrawImage with source and dest rectangles to extract a rectangular region from a Bitmap"
      ],
      "environmentDetails": "No version or platform specified"
    }
  },
  "analysis": {
    "summary": "The reporter wants to crop a rectangular region from an image, equivalent to System.Drawing's Graphics.DrawImage(source, destRect, srcRect, GraphicsUnit.Pixel). SkiaSharp provides this via SKBitmap.ExtractSubset(), SKImage.Subset(), and SKCanvas.DrawBitmap/DrawImage with source+dest rectangles.",
    "rationale": "The issue is a usage question — the user is porting from System.Drawing and doesn't know the SkiaSharp equivalent API. The functionality exists in SkiaSharp through multiple APIs (ExtractSubset, Subset, DrawBitmap with source/dest). No bug is described; no broken behavior exists. This is a 'how to do X' question with a clear, demonstrable answer.",
    "codeInvestigation": [
      {
        "file": "binding/SkiaSharp/SKBitmap.cs",
        "lines": "216-227",
        "finding": "SKBitmap.ExtractSubset(SKBitmap destination, SKRectI subset) extracts a rectangular subset of the bitmap into a new destination bitmap — direct equivalent to the reported use case.",
        "relevance": "direct"
      },
      {
        "file": "binding/SkiaSharp/SKImage.cs",
        "lines": "544-549",
        "finding": "SKImage.Subset(SKRectI subset) returns a new SKImage containing only the specified rectangle — another direct approach for image cropping.",
        "relevance": "direct"
      },
      {
        "file": "binding/SkiaSharp/SKCanvas.cs",
        "lines": "493-502",
        "finding": "SKCanvas.DrawBitmap(bitmap, SKRect source, SKRect dest) and DrawImage(image, SKRect source, SKRect dest) allow drawing a source rectangle into a destination rectangle, mirroring Graphics.DrawImage behavior.",
        "relevance": "related"
      }
    ],
    "keySignals": [
      {
        "text": "How should I cut the stream of the picture? Similar to the code under net core?",
        "source": "issue title and body",
        "interpretation": "Porting question — user wants the SkiaSharp equivalent of System.Drawing image cropping."
      },
      {
        "text": "grSmall.DrawImage(source, new System.Drawing.Rectangle(0, 0, bmSmall.Width, bmSmall.Height), rect, GraphicsUnit.Pixel)",
        "source": "issue body code snippet",
        "interpretation": "Classic GDI+ crop pattern — draw a source rect into a destination rect. SkiaSharp has direct equivalents."
      }
    ],
    "workarounds": [
      "Use SKBitmap.ExtractSubset(destination, new SKRectI(x, y, x+width, y+height)) to extract a cropped bitmap.",
      "Use SKImage.Subset(new SKRectI(x, y, x+width, y+height)) when working with SKImage.",
      "Use SKCanvas.DrawBitmap(bitmap, sourceRect, destRect) to draw only a portion of the source image."
    ],
    "resolution": {
      "hypothesis": "The reporter needs to know the SkiaSharp API for image cropping. SKBitmap.ExtractSubset is the closest System.Drawing equivalent.",
      "proposals": [
        {
          "title": "Use SKBitmap.ExtractSubset",
          "description": "Extract a rectangular crop using SKBitmap.ExtractSubset into a new destination bitmap. This is the most direct SkiaSharp equivalent of the System.Drawing pattern shown.",
          "category": "fix",
          "codeSnippet": "public static SKBitmap CropBitmap(SKBitmap source, SKRectI rect)\n{\n    var destination = new SKBitmap(rect.Width, rect.Height);\n    source.ExtractSubset(destination, rect);\n    return destination;\n}",
          "confidence": 0.92,
          "effort": "cost/xs",
          "validated": "yes"
        },
        {
          "title": "Use SKImage.Subset",
          "description": "If working with SKImage rather than SKBitmap, use SKImage.Subset(SKRectI) to obtain a cropped image.",
          "category": "alternative",
          "codeSnippet": "using var image = SKImage.FromBitmap(sourceBitmap);\nusing var cropped = image.Subset(new SKRectI(x, y, x + width, y + height));",
          "confidence": 0.9,
          "effort": "cost/xs",
          "validated": "yes"
        }
      ],
      "recommendedProposal": "Use SKBitmap.ExtractSubset",
      "recommendedReason": "Closest to the System.Drawing pattern shown and works directly with SKBitmap without conversion overhead."
    }
  },
  "output": {
    "actionability": {
      "suggestedAction": "close-as-not-a-bug",
      "confidence": 0.9,
      "reason": "This is a usage question. The required API exists and works — SKBitmap.ExtractSubset and SKImage.Subset both provide image cropping. A direct answer with code example resolves this.",
      "suggestedReproPlatform": "linux"
    },
    "actions": [
      {
        "type": "update-labels",
        "description": "Apply question and SkiaSharp labels",
        "risk": "low",
        "confidence": 0.95,
        "labels": [
          "type/question",
          "area/SkiaSharp"
        ]
      },
      {
        "type": "add-comment",
        "description": "Post answer with SkiaSharp crop API equivalents",
        "risk": "high",
        "confidence": 0.9,
        "comment": "Hi @bbhxwl! To crop a rectangular region from an image in SkiaSharp, you can use `SKBitmap.ExtractSubset` or `SKImage.Subset`.\n\n**Option 1 — `SKBitmap.ExtractSubset` (closest to System.Drawing):**\n```csharp\npublic static SKBitmap AcquireRectangleImage(SKBitmap source, SKRectI rect)\n{\n    if (source == null || rect.IsEmpty) return null;\n    var destination = new SKBitmap(rect.Width, rect.Height);\n    source.ExtractSubset(destination, rect);\n    return destination;\n}\n```\n\n**Option 2 — `SKImage.Subset`:**\n```csharp\nusing var image = SKImage.FromBitmap(sourceBitmap);\nusing var cropped = image.Subset(new SKRectI(x, y, x + width, y + height));\n```\n\n**Option 3 — Draw source rect to dest rect (like `Graphics.DrawImage`):**\n```csharp\nvar info = new SKImageInfo(rect.Width, rect.Height);\nusing var surface = SKSurface.Create(info);\nvar destRect = new SKRect(0, 0, rect.Width, rect.Height);\nvar sourceRect = new SKRect(rect.Left, rect.Top, rect.Right, rect.Bottom);\nsurface.Canvas.DrawBitmap(source, sourceRect, destRect);\nusing var result = surface.Snapshot();\n```\n\nNote: `SKRectI` coordinates are `(left, top, right, bottom)`, not `(x, y, width, height)`."
      },
      {
        "type": "close-issue",
        "description": "Close as answered",
        "risk": "medium",
        "confidence": 0.85,
        "stateReason": "completed"
      }
    ]
  }
}
```

</details>
