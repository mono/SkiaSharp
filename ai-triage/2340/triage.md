# Issue Triage Report — #2340

| Field | Value |
|-------|-------|
| Repository | mono/SkiaSharp |
| Analyzed | 2026-05-04T08:15:00Z |
| Type | type/question (0.88 (88%)) |
| Area | area/SkiaSharp.Views (0.85 (85%)) |
| Suggested action | close-as-not-a-bug (0.88 (88%)) |

**Issue Summary:** Reporter disposes a System.Drawing.Bitmap returned by ToBitmap() while it is still assigned to PictureBox.Image, causing 'Parameter is not valid.' when WinForms tries to paint the disposed bitmap — this is a WinForms Bitmap lifecycle misuse, not a SkiaSharp defect.

**Analysis:** The reporter calls ToBitmap() to get a System.Drawing.Bitmap, assigns it to PictureBox.Image, then immediately disposes it. When WinForms fires OnPaint, it calls Image.get_RawFormat() on the already-disposed bitmap, throwing 'Parameter is not valid.' The SkiaSharp ToBitmap() implementation is correct — it fully copies pixel data into an independent System.Drawing.Bitmap using LockBits. The bug is in the caller: a System.Drawing.Bitmap assigned to PictureBox.Image must remain alive and undisposed for as long as the PictureBox uses it.

**Recommendations:** **close-as-not-a-bug** — The ToBitmap() implementation is correct. The crash is caused by disposing a System.Drawing.Bitmap while it is still assigned to PictureBox.Image — a standard WinForms lifetime constraint, not a SkiaSharp defect. A clear workaround exists.

---

## Classification

| Field | Value |
|-------|-------|
| Type | type/question |
| Area | area/SkiaSharp.Views |
| Platforms | os/Windows-Classic |
| Backends | — |
| Tenets | — |
| Partner | — |

## Evidence

### Reproduction

1. Create a System.Drawing.Bitmap via SKBitmap/SKImage/SKPicture.ToBitmap()
2. Assign the bitmap to PictureBox.Image
3. Call Dispose() on the bitmap
4. WinForms PictureBox.OnPaint fires and tries to draw the disposed bitmap

**Environment:** SkiaSharp 2.88.3, VS 2022 Pro, .NET 6, Desktop WinForms

**Attachments:**
- screenshot.png — https://user-images.githubusercontent.com/42337026/207782505-03282254-1f77-4992-bf1f-6b17bf38ed28.png — Screenshot showing code patterns

**Code snippets:**

```csharp
Bitmap bmp1 = obj.picture.ToBitmap();
pb.image = bmp1;
bmp1.Dispose(); // crashes: 'Parameter is not valid.'
```

### Version Analysis

| Field | Value |
|-------|-------|
| Mentioned versions | 2.88.3 |
| Worked in | — |
| Broke in | — |
| Current relevance | likely |
| Relevance reason | The ToBitmap() implementation has not changed; this is a WinForms usage pattern issue that applies to all versions. |

## Analysis

### Technical Summary

The reporter calls ToBitmap() to get a System.Drawing.Bitmap, assigns it to PictureBox.Image, then immediately disposes it. When WinForms fires OnPaint, it calls Image.get_RawFormat() on the already-disposed bitmap, throwing 'Parameter is not valid.' The SkiaSharp ToBitmap() implementation is correct — it fully copies pixel data into an independent System.Drawing.Bitmap using LockBits. The bug is in the caller: a System.Drawing.Bitmap assigned to PictureBox.Image must remain alive and undisposed for as long as the PictureBox uses it.

### Rationale

The stack trace in the issue (System.Drawing.Image.get_RawFormat → Graphics.DrawImage → PictureBox.OnPaint) is the canonical WinForms crash from accessing a disposed bitmap. SkiaSharp ToBitmap() creates a fresh System.Drawing.Bitmap by copying pixels — there is no shared memory between SkiaSharp and the returned bitmap. The problem is that pb.image = bmp1 assigns the bitmap to the PictureBox, then bmp1.Dispose() invalidates it while the PictureBox still holds a reference. This is a WinForms lifetime management question, not a SkiaSharp defect.

### Key Signals

- "at System.Drawing.Image.get_RawFormat()
at System.Drawing.Graphics.DrawImage(...)
at System.Windows.Forms.PictureBox.OnPaint(PaintEventArgs pe)" — **issue body stack trace** (Classic WinForms crash from painting a disposed System.Drawing.Bitmap — has nothing to do with SkiaSharp internals.)
- "bmp1.Dispose(); //// FAILS HERE" — **issue body code** (The Dispose() itself doesn't fail — it invalidates the bitmap. The crash happens asynchronously when WinForms paints the PictureBox using the now-disposed image.)
- "If i force the garbage collector to run it cleans up all the objects and works" — **issue body** (GC.Collect() forces collection of other objects but does not help here; any apparent improvement is coincidental timing.)

### Code Investigation

| File | Lines | Relevance | Finding |
|------|-------|-----------|---------|
| `source/SkiaSharp.Views/SkiaSharp.Views.WindowsForms/Extensions.Desktop.cs` | 83-98 | direct | ToBitmap(SKImage) creates a new System.Drawing.Bitmap, uses LockBits to copy pixels from SkiaSharp into it, calls UnlockBits, and returns the bitmap. The returned bitmap is completely independent of SkiaSharp — no shared memory, no callbacks. Disposing SkiaSharp objects has no effect on it. |
| `source/SkiaSharp.Views/SkiaSharp.Views.WindowsForms/Extensions.Desktop.cs` | 100-109 | direct | ToBitmap(SKBitmap) creates a temporary SKImage.FromPixels on the pixmap, calls image.ToBitmap() (which copies into an independent System.Drawing.Bitmap), then disposes the temporary SKImage. GC.KeepAlive(skiaBitmap) ensures the source stays alive during the copy. The returned Bitmap is independent. |

### Workarounds

- Do not dispose the bitmap while PictureBox.Image still references it. Instead, swap the old image out first: var old = pb.Image; pb.Image = newBitmap; old?.Dispose();
- If using 'using var', move the PictureBox.Image assignment to a longer-lived scope so the bitmap outlives the using block.

### Resolution Proposals

**Hypothesis:** This is a WinForms Bitmap lifetime issue: the caller must not dispose a System.Drawing.Bitmap that is still assigned to a PictureBox. SkiaSharp ToBitmap() is correct.

1. **Swap old image before disposing** — workaround, confidence 0.95 (95%), cost/xs, validated=yes
   - Replace the old PictureBox.Image before disposing the previous bitmap. This is the idiomatic WinForms pattern for updating images without leaking.

```csharp
var newBitmap = result.ovpicture.ToBitmap(new SKSizeI(width, height));
var oldBitmap = pb.Image;
pb.Image = newBitmap;
oldBitmap?.Dispose();
```
2. **Hold bitmap reference for PictureBox lifetime** — alternative, confidence 0.92 (92%), cost/xs, validated=yes
   - Store the bitmap in a field and dispose it only when the form closes or the next image is assigned. Do not use 'using var' for bitmaps that are assigned to controls.

```csharp
// Field: private System.Drawing.Bitmap? _currentBitmap;

// Update method:
var newBitmap = result.ovpicture.ToBitmap(new SKSizeI(width, height));
_currentBitmap?.Dispose();
_currentBitmap = newBitmap;
pb.Image = _currentBitmap;
```

**Recommended proposal:** Swap old image before disposing

**Why:** Minimal change, idiomatic WinForms, directly fixes both the crash and the memory leak in two lines.

## Recommendations

### Actionability

| Field | Value |
|-------|-------|
| Suggested action | close-as-not-a-bug |
| Confidence | 0.88 (88%) |
| Reason | The ToBitmap() implementation is correct. The crash is caused by disposing a System.Drawing.Bitmap while it is still assigned to PictureBox.Image — a standard WinForms lifetime constraint, not a SkiaSharp defect. A clear workaround exists. |
| Suggested repro platform | windows |

### Automatable Actions

| Type | Risk | Confidence | Description | Details |
|------|------|------------|-------------|---------|
| update-labels | low | 0.90 (90%) | Apply question type, SkiaSharp.Views area, and Windows platform labels | labels=type/question, area/SkiaSharp.Views, os/Windows-Classic |
| add-comment | high | 0.88 (88%) | Explain WinForms Bitmap lifetime issue and provide correct code pattern | — |
| close-issue | medium | 0.85 (85%) | Close as not a bug — by-design WinForms behavior, workaround provided | stateReason=not_planned |

**Comment draft for `add-comment`:**

```markdown
Thanks for the detailed report and stack trace.

The `ToBitmap()` method returns a fully independent `System.Drawing.Bitmap` — it copies the pixel data via `LockBits` and has no ongoing connection to SkiaSharp. The crash is actually a WinForms lifetime issue: `System.Drawing.Bitmap.Dispose()` invalidates the bitmap immediately, but `PictureBox` still holds a reference to it. When `OnPaint` fires it calls `Image.get_RawFormat()` on the disposed bitmap and throws "Parameter is not valid."

Here's the correct pattern for updating a PictureBox without leaking memory:

```csharp
var newBitmap = result.ovpicture.ToBitmap(new SKSizeI(width, height));
var oldBitmap = pb.Image;
pb.Image = newBitmap;
oldBitmap?.Dispose();
```

Swap the image first, *then* dispose the old one. The `using var` pattern doesn't work here because the PictureBox needs the bitmap to stay alive after the `using` block ends. Instead, store the current bitmap in a field and dispose it only when you replace it or the form closes.
```

<details>
<summary>Raw JSON</summary>

```json
{
  "meta": {
    "schemaVersion": "1.0",
    "number": 2340,
    "repo": "mono/SkiaSharp",
    "analyzedAt": "2026-05-04T08:15:00Z"
  },
  "summary": "Reporter disposes a System.Drawing.Bitmap returned by ToBitmap() while it is still assigned to PictureBox.Image, causing 'Parameter is not valid.' when WinForms tries to paint the disposed bitmap — this is a WinForms Bitmap lifecycle misuse, not a SkiaSharp defect.",
  "classification": {
    "type": {
      "value": "type/question",
      "confidence": 0.88
    },
    "area": {
      "value": "area/SkiaSharp.Views",
      "confidence": 0.85
    },
    "platforms": [
      "os/Windows-Classic"
    ]
  },
  "evidence": {
    "reproEvidence": {
      "stepsToReproduce": [
        "Create a System.Drawing.Bitmap via SKBitmap/SKImage/SKPicture.ToBitmap()",
        "Assign the bitmap to PictureBox.Image",
        "Call Dispose() on the bitmap",
        "WinForms PictureBox.OnPaint fires and tries to draw the disposed bitmap"
      ],
      "codeSnippets": [
        "Bitmap bmp1 = obj.picture.ToBitmap();\npb.image = bmp1;\nbmp1.Dispose(); // crashes: 'Parameter is not valid.'"
      ],
      "environmentDetails": "SkiaSharp 2.88.3, VS 2022 Pro, .NET 6, Desktop WinForms",
      "attachments": [
        {
          "url": "https://user-images.githubusercontent.com/42337026/207782505-03282254-1f77-4992-bf1f-6b17bf38ed28.png",
          "description": "Screenshot showing code patterns",
          "filename": "screenshot.png"
        }
      ]
    },
    "versionAnalysis": {
      "mentionedVersions": [
        "2.88.3"
      ],
      "currentRelevance": "likely",
      "relevanceReason": "The ToBitmap() implementation has not changed; this is a WinForms usage pattern issue that applies to all versions."
    }
  },
  "analysis": {
    "summary": "The reporter calls ToBitmap() to get a System.Drawing.Bitmap, assigns it to PictureBox.Image, then immediately disposes it. When WinForms fires OnPaint, it calls Image.get_RawFormat() on the already-disposed bitmap, throwing 'Parameter is not valid.' The SkiaSharp ToBitmap() implementation is correct — it fully copies pixel data into an independent System.Drawing.Bitmap using LockBits. The bug is in the caller: a System.Drawing.Bitmap assigned to PictureBox.Image must remain alive and undisposed for as long as the PictureBox uses it.",
    "rationale": "The stack trace in the issue (System.Drawing.Image.get_RawFormat → Graphics.DrawImage → PictureBox.OnPaint) is the canonical WinForms crash from accessing a disposed bitmap. SkiaSharp ToBitmap() creates a fresh System.Drawing.Bitmap by copying pixels — there is no shared memory between SkiaSharp and the returned bitmap. The problem is that pb.image = bmp1 assigns the bitmap to the PictureBox, then bmp1.Dispose() invalidates it while the PictureBox still holds a reference. This is a WinForms lifetime management question, not a SkiaSharp defect.",
    "keySignals": [
      {
        "text": "at System.Drawing.Image.get_RawFormat()\nat System.Drawing.Graphics.DrawImage(...)\nat System.Windows.Forms.PictureBox.OnPaint(PaintEventArgs pe)",
        "source": "issue body stack trace",
        "interpretation": "Classic WinForms crash from painting a disposed System.Drawing.Bitmap — has nothing to do with SkiaSharp internals."
      },
      {
        "text": "bmp1.Dispose(); //// FAILS HERE",
        "source": "issue body code",
        "interpretation": "The Dispose() itself doesn't fail — it invalidates the bitmap. The crash happens asynchronously when WinForms paints the PictureBox using the now-disposed image."
      },
      {
        "text": "If i force the garbage collector to run it cleans up all the objects and works",
        "source": "issue body",
        "interpretation": "GC.Collect() forces collection of other objects but does not help here; any apparent improvement is coincidental timing."
      }
    ],
    "codeInvestigation": [
      {
        "file": "source/SkiaSharp.Views/SkiaSharp.Views.WindowsForms/Extensions.Desktop.cs",
        "lines": "83-98",
        "finding": "ToBitmap(SKImage) creates a new System.Drawing.Bitmap, uses LockBits to copy pixels from SkiaSharp into it, calls UnlockBits, and returns the bitmap. The returned bitmap is completely independent of SkiaSharp — no shared memory, no callbacks. Disposing SkiaSharp objects has no effect on it.",
        "relevance": "direct"
      },
      {
        "file": "source/SkiaSharp.Views/SkiaSharp.Views.WindowsForms/Extensions.Desktop.cs",
        "lines": "100-109",
        "finding": "ToBitmap(SKBitmap) creates a temporary SKImage.FromPixels on the pixmap, calls image.ToBitmap() (which copies into an independent System.Drawing.Bitmap), then disposes the temporary SKImage. GC.KeepAlive(skiaBitmap) ensures the source stays alive during the copy. The returned Bitmap is independent.",
        "relevance": "direct"
      }
    ],
    "workarounds": [
      "Do not dispose the bitmap while PictureBox.Image still references it. Instead, swap the old image out first: var old = pb.Image; pb.Image = newBitmap; old?.Dispose();",
      "If using 'using var', move the PictureBox.Image assignment to a longer-lived scope so the bitmap outlives the using block."
    ],
    "resolution": {
      "hypothesis": "This is a WinForms Bitmap lifetime issue: the caller must not dispose a System.Drawing.Bitmap that is still assigned to a PictureBox. SkiaSharp ToBitmap() is correct.",
      "proposals": [
        {
          "title": "Swap old image before disposing",
          "description": "Replace the old PictureBox.Image before disposing the previous bitmap. This is the idiomatic WinForms pattern for updating images without leaking.",
          "category": "workaround",
          "codeSnippet": "var newBitmap = result.ovpicture.ToBitmap(new SKSizeI(width, height));\nvar oldBitmap = pb.Image;\npb.Image = newBitmap;\noldBitmap?.Dispose();",
          "confidence": 0.95,
          "effort": "cost/xs",
          "validated": "yes"
        },
        {
          "title": "Hold bitmap reference for PictureBox lifetime",
          "description": "Store the bitmap in a field and dispose it only when the form closes or the next image is assigned. Do not use 'using var' for bitmaps that are assigned to controls.",
          "category": "alternative",
          "codeSnippet": "// Field: private System.Drawing.Bitmap? _currentBitmap;\n\n// Update method:\nvar newBitmap = result.ovpicture.ToBitmap(new SKSizeI(width, height));\n_currentBitmap?.Dispose();\n_currentBitmap = newBitmap;\npb.Image = _currentBitmap;",
          "confidence": 0.92,
          "effort": "cost/xs",
          "validated": "yes"
        }
      ],
      "recommendedProposal": "Swap old image before disposing",
      "recommendedReason": "Minimal change, idiomatic WinForms, directly fixes both the crash and the memory leak in two lines."
    }
  },
  "output": {
    "actionability": {
      "suggestedAction": "close-as-not-a-bug",
      "confidence": 0.88,
      "reason": "The ToBitmap() implementation is correct. The crash is caused by disposing a System.Drawing.Bitmap while it is still assigned to PictureBox.Image — a standard WinForms lifetime constraint, not a SkiaSharp defect. A clear workaround exists.",
      "suggestedReproPlatform": "windows"
    },
    "actions": [
      {
        "type": "update-labels",
        "description": "Apply question type, SkiaSharp.Views area, and Windows platform labels",
        "risk": "low",
        "confidence": 0.9,
        "labels": [
          "type/question",
          "area/SkiaSharp.Views",
          "os/Windows-Classic"
        ]
      },
      {
        "type": "add-comment",
        "description": "Explain WinForms Bitmap lifetime issue and provide correct code pattern",
        "risk": "high",
        "confidence": 0.88,
        "comment": "Thanks for the detailed report and stack trace.\n\nThe `ToBitmap()` method returns a fully independent `System.Drawing.Bitmap` — it copies the pixel data via `LockBits` and has no ongoing connection to SkiaSharp. The crash is actually a WinForms lifetime issue: `System.Drawing.Bitmap.Dispose()` invalidates the bitmap immediately, but `PictureBox` still holds a reference to it. When `OnPaint` fires it calls `Image.get_RawFormat()` on the disposed bitmap and throws \"Parameter is not valid.\"\n\nHere's the correct pattern for updating a PictureBox without leaking memory:\n\n```csharp\nvar newBitmap = result.ovpicture.ToBitmap(new SKSizeI(width, height));\nvar oldBitmap = pb.Image;\npb.Image = newBitmap;\noldBitmap?.Dispose();\n```\n\nSwap the image first, *then* dispose the old one. The `using var` pattern doesn't work here because the PictureBox needs the bitmap to stay alive after the `using` block ends. Instead, store the current bitmap in a field and dispose it only when you replace it or the form closes."
      },
      {
        "type": "close-issue",
        "description": "Close as not a bug — by-design WinForms behavior, workaround provided",
        "risk": "medium",
        "confidence": 0.85,
        "stateReason": "not_planned"
      }
    ]
  }
}
```

</details>
