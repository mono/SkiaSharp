# Issue Triage Report — #2282

| Field | Value |
|-------|-------|
| Repository | mono/SkiaSharp |
| Analyzed | 2026-05-02T14:13:57Z |
| Type | type/question (0.95 (95%)) |
| Area | area/SkiaSharp (0.90 (90%)) |
| Suggested action | close-as-not-a-bug (0.85 (85%)) |

**Issue Summary:** User asking how to access the palette (color table) and per-pixel palette index from an 8-bit indexed BMP image in SkiaSharp.

**Analysis:** The user wants to access two things from an 8-bit indexed BMP: (1) the 256-color palette table, and (2) the raw palette index byte for each pixel. Skia dropped support for the Index8 color type; when decoding indexed BMPs, Skia automatically expands pixels to a non-indexed format (e.g., Rgba8888), losing the index information. There is no SkiaSharp API to retrieve the palette or per-pixel palette index after decoding. The raw pixel data from GetPixelSpan() reflects expanded colors, not palette indices. To obtain palette indices, the caller must parse the raw BMP file bytes manually (the index array starts at offset 54 for a standard 8-bit BMP without extra headers).

**Recommendations:** **close-as-not-a-bug** — This is a usage question. Skia dropped Index8 support and does not expose palette/index data. The answer is to parse the BMP manually. No SkiaSharp change is needed.

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

1. Load a 4x2 8-bit BMP image with 256-color palette
2. Try to retrieve the 256-color palette (1024 bytes)
3. Try to retrieve the palette index (not decoded color) for each pixel position

**Environment:** 8-bit indexed BMP file, no SkiaSharp version specified

## Analysis

### Technical Summary

The user wants to access two things from an 8-bit indexed BMP: (1) the 256-color palette table, and (2) the raw palette index byte for each pixel. Skia dropped support for the Index8 color type; when decoding indexed BMPs, Skia automatically expands pixels to a non-indexed format (e.g., Rgba8888), losing the index information. There is no SkiaSharp API to retrieve the palette or per-pixel palette index after decoding. The raw pixel data from GetPixelSpan() reflects expanded colors, not palette indices. To obtain palette indices, the caller must parse the raw BMP file bytes manually (the index array starts at offset 54 for a standard 8-bit BMP without extra headers).

### Rationale

This is a usage question. The reporter is not describing broken behavior, they want to know how to accomplish a task. The underlying answer is that Skia (and therefore SkiaSharp) does not expose indexed-bitmap palette data — it converts on decode. No palette API exists in SKBitmap, SKPixmap, or SKCodec. The Index8 SKColorType that would have supported this was removed from Skia.

### Key Signals

- "How do i get this bmp's palette. (1024bytes, 256 colors)" — **issue body** (Reporter wants the raw 256-entry color table stored in the BMP file.)
- "How do i get the palette index of a position" — **issue body** (Reporter wants the raw index byte per pixel, not the decoded color.)
- "the position(0,0) palette index is 0x11" — **issue body** (Clear example showing they want raw index bytes from the pixel data buffer, not RGBA values.)

### Code Investigation

| File | Lines | Relevance | Finding |
|------|-------|-----------|---------|
| `binding/SkiaSharp/Definitions.cs` | 36-65 | direct | SKColorType enum does not include Index8 or any indexed color type — Skia dropped indexed pixel support. Decoding an 8-bit BMP will produce a non-indexed color type (e.g., Rgba8888). |
| `binding/SkiaSharp/SKBitmap.cs` | 16 | direct | Legacy comment references Index8: 'Setting the ColorTable is only supported for bitmaps with ColorTypes of Index8.' This is a dead code comment — Index8 is no longer in the enum, confirming palette/index access is not supported. |
| `binding/SkiaSharp/SKBitmap.cs` | 126-129 | direct | GetPixel(x, y) returns SKColor (fully decoded RGBA), not a palette index. There is no GetPaletteIndex or equivalent method. |

### Workarounds

- Parse the BMP file manually: read the 1024-byte color table at offset 54 (for standard 8-bit BMP), then read the pixel index array at the pixel data offset.
- Use System.IO to open the raw BMP bytes with BinaryReader, seek to offset 54 for the palette, then read W*H bytes for the index array.
- If only the decoded color is needed (not the index), use SKBitmap.Decode() and call GetPixel(x, y) to get the SKColor at each position — palette indices will not be available.

### Resolution Proposals

**Hypothesis:** Skia does not expose palette/indexed bitmap data. The user needs to read raw BMP bytes directly to obtain palette entries and pixel indices.

1. **Parse BMP manually for palette and indices** — workaround, confidence 0.90 (90%), cost/s, validated=untested
   - Open the BMP as raw bytes using System.IO.File.ReadAllBytes or a BinaryReader. The 256-color palette is at byte offset 54 (4 bytes per entry: B, G, R, reserved). The pixel index array starts at the pixel data offset stored at BMP header offset 10 (4 bytes, little-endian). Each pixel byte IS the palette index.
2. **Use GetPixel for decoded color only** — alternative, confidence 0.95 (95%), cost/xs, validated=untested
   - If the goal is just to read pixel colors (not raw indices), decode the BMP with SKBitmap.Decode() and use GetPixel(x, y). This returns the resolved RGBA color but loses the palette index information.

**Recommended proposal:** Parse BMP manually for palette and indices

**Why:** Directly answers both questions the reporter asked (palette table + per-pixel index). SkiaSharp has no API for this.

## Recommendations

### Actionability

| Field | Value |
|-------|-------|
| Suggested action | close-as-not-a-bug |
| Confidence | 0.85 (85%) |
| Reason | This is a usage question. Skia dropped Index8 support and does not expose palette/index data. The answer is to parse the BMP manually. No SkiaSharp change is needed. |
| Suggested repro platform | linux |

### Automatable Actions

| Type | Risk | Confidence | Description | Details |
|------|------|------------|-------------|---------|
| update-labels | low | 0.95 (95%) | Apply question and SkiaSharp area labels | labels=type/question, area/SkiaSharp |
| add-comment | high | 0.85 (85%) | Answer the question: Skia does not expose palette/index data; parse BMP manually | — |
| close-issue | medium | 0.80 (80%) | Close as answered — not a SkiaSharp bug, palette access requires raw BMP parsing | stateReason=completed |

**Comment draft for `add-comment`:**

```markdown
Thanks for the question!

Unfortunately, **SkiaSharp (and the underlying Skia library) does not provide access to the raw palette or per-pixel palette indices** from indexed BMP images. Skia dropped support for the `Index8` color type, so when you decode an 8-bit BMP it is automatically expanded to a standard color format (e.g., RGBA8888). The palette index information is lost during this expansion.

**If you need the decoded color at a position**, use `SKBitmap.Decode()` and `GetPixel(x, y)`:
```csharp
using var bitmap = SKBitmap.Decode("yourfile.bmp");
SKColor color = bitmap.GetPixel(0, 0); // returns the RGBA color, not the palette index
```

**If you need the raw palette entries and per-pixel indices**, you must parse the BMP file yourself using `System.IO`:

```csharp
byte[] bmpBytes = File.ReadAllBytes("yourfile.bmp");

// Pixel data offset is stored at byte 10 (4 bytes, little-endian)
int pixelDataOffset = BitConverter.ToInt32(bmpBytes, 10);

// For a standard 8-bit BMP, the 256-color palette starts at offset 54
// Each entry is 4 bytes: Blue, Green, Red, Reserved
var palette = new (byte R, byte G, byte B)[256];
for (int i = 0; i < 256; i++)
{
    int entryOffset = 54 + i * 4;
    palette[i] = (bmpBytes[entryOffset + 2], bmpBytes[entryOffset + 1], bmpBytes[entryOffset]);
}

// Each pixel byte IS the palette index
// BMP rows are stored bottom-up, so flip y
int width = BitConverter.ToInt32(bmpBytes, 18);
int height = BitConverter.ToInt32(bmpBytes, 22);
byte GetPaletteIndex(int x, int y)
{
    int row = height - 1 - y;  // BMP is bottom-up
    int stride = (width + 3) & ~3;  // rows padded to 4-byte boundary
    return bmpBytes[pixelDataOffset + row * stride + x];
}

// Examples from your question:
byte idx00 = GetPaletteIndex(0, 0); // 0x11
byte idx12 = GetPaletteIndex(1, 2); // note: (1,2) is out of range for a 4x2 image
```

Note: In your example `position(1,2)` would be out of bounds for a 4×2 image — BMP coordinates are typically (column, row) with rows starting from the bottom.
```

<details>
<summary>Raw JSON</summary>

```json
{
  "meta": {
    "schemaVersion": "1.0",
    "number": 2282,
    "repo": "mono/SkiaSharp",
    "analyzedAt": "2026-05-02T14:13:57Z"
  },
  "summary": "User asking how to access the palette (color table) and per-pixel palette index from an 8-bit indexed BMP image in SkiaSharp.",
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
      "stepsToReproduce": [
        "Load a 4x2 8-bit BMP image with 256-color palette",
        "Try to retrieve the 256-color palette (1024 bytes)",
        "Try to retrieve the palette index (not decoded color) for each pixel position"
      ],
      "environmentDetails": "8-bit indexed BMP file, no SkiaSharp version specified"
    }
  },
  "analysis": {
    "summary": "The user wants to access two things from an 8-bit indexed BMP: (1) the 256-color palette table, and (2) the raw palette index byte for each pixel. Skia dropped support for the Index8 color type; when decoding indexed BMPs, Skia automatically expands pixels to a non-indexed format (e.g., Rgba8888), losing the index information. There is no SkiaSharp API to retrieve the palette or per-pixel palette index after decoding. The raw pixel data from GetPixelSpan() reflects expanded colors, not palette indices. To obtain palette indices, the caller must parse the raw BMP file bytes manually (the index array starts at offset 54 for a standard 8-bit BMP without extra headers).",
    "rationale": "This is a usage question. The reporter is not describing broken behavior, they want to know how to accomplish a task. The underlying answer is that Skia (and therefore SkiaSharp) does not expose indexed-bitmap palette data — it converts on decode. No palette API exists in SKBitmap, SKPixmap, or SKCodec. The Index8 SKColorType that would have supported this was removed from Skia.",
    "codeInvestigation": [
      {
        "file": "binding/SkiaSharp/Definitions.cs",
        "lines": "36-65",
        "finding": "SKColorType enum does not include Index8 or any indexed color type — Skia dropped indexed pixel support. Decoding an 8-bit BMP will produce a non-indexed color type (e.g., Rgba8888).",
        "relevance": "direct"
      },
      {
        "file": "binding/SkiaSharp/SKBitmap.cs",
        "lines": "16",
        "finding": "Legacy comment references Index8: 'Setting the ColorTable is only supported for bitmaps with ColorTypes of Index8.' This is a dead code comment — Index8 is no longer in the enum, confirming palette/index access is not supported.",
        "relevance": "direct"
      },
      {
        "file": "binding/SkiaSharp/SKBitmap.cs",
        "lines": "126-129",
        "finding": "GetPixel(x, y) returns SKColor (fully decoded RGBA), not a palette index. There is no GetPaletteIndex or equivalent method.",
        "relevance": "direct"
      }
    ],
    "keySignals": [
      {
        "text": "How do i get this bmp's palette. (1024bytes, 256 colors)",
        "source": "issue body",
        "interpretation": "Reporter wants the raw 256-entry color table stored in the BMP file."
      },
      {
        "text": "How do i get the palette index of a position",
        "source": "issue body",
        "interpretation": "Reporter wants the raw index byte per pixel, not the decoded color."
      },
      {
        "text": "the position(0,0) palette index is 0x11",
        "source": "issue body",
        "interpretation": "Clear example showing they want raw index bytes from the pixel data buffer, not RGBA values."
      }
    ],
    "workarounds": [
      "Parse the BMP file manually: read the 1024-byte color table at offset 54 (for standard 8-bit BMP), then read the pixel index array at the pixel data offset.",
      "Use System.IO to open the raw BMP bytes with BinaryReader, seek to offset 54 for the palette, then read W*H bytes for the index array.",
      "If only the decoded color is needed (not the index), use SKBitmap.Decode() and call GetPixel(x, y) to get the SKColor at each position — palette indices will not be available."
    ],
    "resolution": {
      "hypothesis": "Skia does not expose palette/indexed bitmap data. The user needs to read raw BMP bytes directly to obtain palette entries and pixel indices.",
      "proposals": [
        {
          "title": "Parse BMP manually for palette and indices",
          "description": "Open the BMP as raw bytes using System.IO.File.ReadAllBytes or a BinaryReader. The 256-color palette is at byte offset 54 (4 bytes per entry: B, G, R, reserved). The pixel index array starts at the pixel data offset stored at BMP header offset 10 (4 bytes, little-endian). Each pixel byte IS the palette index.",
          "category": "workaround",
          "confidence": 0.9,
          "effort": "cost/s",
          "validated": "untested"
        },
        {
          "title": "Use GetPixel for decoded color only",
          "description": "If the goal is just to read pixel colors (not raw indices), decode the BMP with SKBitmap.Decode() and use GetPixel(x, y). This returns the resolved RGBA color but loses the palette index information.",
          "category": "alternative",
          "confidence": 0.95,
          "effort": "cost/xs",
          "validated": "untested"
        }
      ],
      "recommendedProposal": "Parse BMP manually for palette and indices",
      "recommendedReason": "Directly answers both questions the reporter asked (palette table + per-pixel index). SkiaSharp has no API for this."
    }
  },
  "output": {
    "actionability": {
      "suggestedAction": "close-as-not-a-bug",
      "confidence": 0.85,
      "reason": "This is a usage question. Skia dropped Index8 support and does not expose palette/index data. The answer is to parse the BMP manually. No SkiaSharp change is needed.",
      "suggestedReproPlatform": "linux"
    },
    "actions": [
      {
        "type": "update-labels",
        "description": "Apply question and SkiaSharp area labels",
        "risk": "low",
        "confidence": 0.95,
        "labels": [
          "type/question",
          "area/SkiaSharp"
        ]
      },
      {
        "type": "add-comment",
        "description": "Answer the question: Skia does not expose palette/index data; parse BMP manually",
        "risk": "high",
        "confidence": 0.85,
        "comment": "Thanks for the question!\n\nUnfortunately, **SkiaSharp (and the underlying Skia library) does not provide access to the raw palette or per-pixel palette indices** from indexed BMP images. Skia dropped support for the `Index8` color type, so when you decode an 8-bit BMP it is automatically expanded to a standard color format (e.g., RGBA8888). The palette index information is lost during this expansion.\n\n**If you need the decoded color at a position**, use `SKBitmap.Decode()` and `GetPixel(x, y)`:\n```csharp\nusing var bitmap = SKBitmap.Decode(\"yourfile.bmp\");\nSKColor color = bitmap.GetPixel(0, 0); // returns the RGBA color, not the palette index\n```\n\n**If you need the raw palette entries and per-pixel indices**, you must parse the BMP file yourself using `System.IO`:\n\n```csharp\nbyte[] bmpBytes = File.ReadAllBytes(\"yourfile.bmp\");\n\n// Pixel data offset is stored at byte 10 (4 bytes, little-endian)\nint pixelDataOffset = BitConverter.ToInt32(bmpBytes, 10);\n\n// For a standard 8-bit BMP, the 256-color palette starts at offset 54\n// Each entry is 4 bytes: Blue, Green, Red, Reserved\nvar palette = new (byte R, byte G, byte B)[256];\nfor (int i = 0; i < 256; i++)\n{\n    int entryOffset = 54 + i * 4;\n    palette[i] = (bmpBytes[entryOffset + 2], bmpBytes[entryOffset + 1], bmpBytes[entryOffset]);\n}\n\n// Each pixel byte IS the palette index\n// BMP rows are stored bottom-up, so flip y\nint width = BitConverter.ToInt32(bmpBytes, 18);\nint height = BitConverter.ToInt32(bmpBytes, 22);\nbyte GetPaletteIndex(int x, int y)\n{\n    int row = height - 1 - y;  // BMP is bottom-up\n    int stride = (width + 3) & ~3;  // rows padded to 4-byte boundary\n    return bmpBytes[pixelDataOffset + row * stride + x];\n}\n\n// Examples from your question:\nbyte idx00 = GetPaletteIndex(0, 0); // 0x11\nbyte idx12 = GetPaletteIndex(1, 2); // note: (1,2) is out of range for a 4x2 image\n```\n\nNote: In your example `position(1,2)` would be out of bounds for a 4×2 image — BMP coordinates are typically (column, row) with rows starting from the bottom."
      },
      {
        "type": "close-issue",
        "description": "Close as answered — not a SkiaSharp bug, palette access requires raw BMP parsing",
        "risk": "medium",
        "confidence": 0.8,
        "stateReason": "completed"
      }
    ]
  }
}
```

</details>
