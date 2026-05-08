# Issue Triage Report — #987

| Field | Value |
|-------|-------|
| Repository | mono/SkiaSharp |
| Analyzed | 2026-04-29T09:00:00Z |
| Type | type/question (0.95 (95%)) |
| Area | area/SkiaSharp (0.92 (92%)) |
| Suggested action | close-as-not-a-bug (0.88 (88%)) |

**Issue Summary:** User asks whether SKBitmap/ImageSource can be converted to SKPicture, and whether a bitmap can be drawn with an SKMatrix transformation.

**Analysis:** Reporter asks two related usage questions: (1) how to convert an SKBitmap to an SKPicture and (2) how to draw a bitmap with an SKMatrix. Converting pixel data from SKBitmap into SKPicture drawing commands is not supported by the API design — SKPicture captures vector drawing commands, not raster pixel buffers. However, drawing a bitmap with arbitrary matrix transforms is fully supported via canvas.Concat(matrix) + canvas.DrawBitmap, or canvas.Save/Translate/DrawBitmap/Restore. A maintainer already answered the question in the issue comments in 2019 with a working code snippet.

**Recommendations:** **close-as-not-a-bug** — Usage question already answered by maintainer in 2019. API behavior is correct and documented. Drawing with a matrix is supported; pixel-to-picture conversion is not an API that exists by design.

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

## Analysis

### Technical Summary

Reporter asks two related usage questions: (1) how to convert an SKBitmap to an SKPicture and (2) how to draw a bitmap with an SKMatrix. Converting pixel data from SKBitmap into SKPicture drawing commands is not supported by the API design — SKPicture captures vector drawing commands, not raster pixel buffers. However, drawing a bitmap with arbitrary matrix transforms is fully supported via canvas.Concat(matrix) + canvas.DrawBitmap, or canvas.Save/Translate/DrawBitmap/Restore. A maintainer already answered the question in the issue comments in 2019 with a working code snippet.

### Rationale

The issue title is tagged [QUESTION] and the body only asks how to accomplish a task — there is no broken behavior described. The maintainer's existing comment answers both questions. This is a usage question with an existing answer, suitable for closing as completed.

### Key Signals

- "Is there any way to convert SKBitmap/ImageSource to SkPicture?" — **issue body** (Asking about a pixel-to-picture conversion API that does not exist — SKPicture records vector commands only.)
- "or draw Bitmap with SKMatrix?" — **issue body** (Second sub-question: this IS supported via canvas.Concat(matrix) + DrawBitmap.)
- "if you are just wanting to draw with a matrix, then you should be able to just save the canvas, do the transformation, draw the bitmap and then restore" — **comment by mattleibow** (Maintainer already answered with a code example — issue is resolved.)

### Code Investigation

| File | Lines | Relevance | Finding |
|------|-------|-----------|---------|
| `binding/SkiaSharp/SKPictureRecorder.cs` | 28-54 | direct | SKPicture is created only via SKPictureRecorder.BeginRecording + EndRecording, which records canvas drawing commands. There is no API to construct an SKPicture from pixel data (SKBitmap) — confirming conversion is not possible. |
| `binding/SkiaSharp/SKCanvas.cs` | 233-236 | direct | canvas.Concat(SKMatrix) concatenates a matrix onto the current canvas transform, allowing any subsequent DrawBitmap call to be transformed — this directly answers the SKMatrix question. |
| `binding/SkiaSharp/SKCanvas.cs` | 566-586 | related | DrawBitmap overloads accept position, dest rect, and source/dest rects. Combined with canvas.Save/Translate(x,y)/Restore or canvas.Concat(matrix), arbitrary bitmap placement with transforms is fully supported. |

### Workarounds

- Use canvas.Concat(matrix) + canvas.DrawBitmap(...) to draw a bitmap with an arbitrary SKMatrix transform.
- Use canvas.Save() / canvas.Translate(dx, dy) / canvas.DrawBitmap(...) / canvas.Restore() for translate-only transforms.
- To record a bitmap draw into an SKPicture, use SKPictureRecorder.BeginRecording() to get a recording canvas, then call canvas.DrawBitmap() on that canvas, and finally call EndRecording() — this records the draw command but not the pixel conversion.

### Resolution Proposals

**Hypothesis:** Reporter wants to apply a matrix transform when drawing a bitmap; the canvas transform APIs fully support this. There is no SKBitmap-to-SKPicture pixel conversion API, by design.

1. **Apply transform with canvas.Concat** — workaround, confidence 0.95 (95%), cost/xs, validated=yes
   - Use canvas.Save(), canvas.Concat(matrix), canvas.DrawBitmap(bitmap, ...), canvas.Restore() to draw a bitmap with any SKMatrix.
2. **Record bitmap draw into SKPicture** — workaround, confidence 0.90 (90%), cost/xs, validated=yes
   - If an SKPicture containing a bitmap draw is needed, use SKPictureRecorder: begin recording, draw the bitmap on the recording canvas (with any transforms), then end recording to get an SKPicture.

**Recommended proposal:** Apply transform with canvas.Concat

**Why:** Simplest approach; directly addresses both the matrix and SKPicture recording scenarios. Maintainer already provided a Translate variant of this approach.

## Recommendations

### Actionability

| Field | Value |
|-------|-------|
| Suggested action | close-as-not-a-bug |
| Confidence | 0.88 (88%) |
| Reason | Usage question already answered by maintainer in 2019. API behavior is correct and documented. Drawing with a matrix is supported; pixel-to-picture conversion is not an API that exists by design. |
| Suggested repro platform | linux |

### Automatable Actions

| Type | Risk | Confidence | Description | Details |
|------|------|------------|-------------|---------|
| update-labels | low | 0.95 (95%) | Apply question and SkiaSharp area labels | labels=type/question, area/SkiaSharp |
| add-comment | medium | 0.88 (88%) | Summarize the answer to both sub-questions and provide code snippets | — |
| close-issue | medium | 0.85 (85%) | Close as answered — both sub-questions have clear answers | stateReason=completed |

**Comment draft for `add-comment`:**

```markdown
Thanks for the question!

**Drawing an `SKBitmap` with an `SKMatrix`:**
You can apply any transformation by saving the canvas state, applying the matrix, drawing, then restoring:

```csharp
var canvas = ...;
using (new SKAutoCanvasRestore(canvas)) {
    canvas.Concat(matrix); // apply your SKMatrix
    canvas.DrawBitmap(bitmap, 0, 0);
}
```

**Converting `SKBitmap` to `SKPicture`:**
There is no direct conversion — `SKPicture` captures *drawing commands*, not pixel data. However, if you need an `SKPicture` that contains a bitmap draw, you can record it:

```csharp
var bounds = new SKRect(0, 0, bitmap.Width, bitmap.Height);
using var recorder = new SKPictureRecorder();
var recordingCanvas = recorder.BeginRecording(bounds);
recordingCanvas.DrawBitmap(bitmap, 0, 0);
using var picture = recorder.EndRecording();
```

This records the draw-bitmap command into the picture, which can then be played back or used as a shader.
```

<details>
<summary>Raw JSON</summary>

```json
{
  "meta": {
    "schemaVersion": "1.0",
    "number": 987,
    "repo": "mono/SkiaSharp",
    "analyzedAt": "2026-04-29T09:00:00Z"
  },
  "summary": "User asks whether SKBitmap/ImageSource can be converted to SKPicture, and whether a bitmap can be drawn with an SKMatrix transformation.",
  "classification": {
    "type": {
      "value": "type/question",
      "confidence": 0.95
    },
    "area": {
      "value": "area/SkiaSharp",
      "confidence": 0.92
    }
  },
  "evidence": {
    "reproEvidence": {
      "repoLinks": []
    }
  },
  "analysis": {
    "summary": "Reporter asks two related usage questions: (1) how to convert an SKBitmap to an SKPicture and (2) how to draw a bitmap with an SKMatrix. Converting pixel data from SKBitmap into SKPicture drawing commands is not supported by the API design — SKPicture captures vector drawing commands, not raster pixel buffers. However, drawing a bitmap with arbitrary matrix transforms is fully supported via canvas.Concat(matrix) + canvas.DrawBitmap, or canvas.Save/Translate/DrawBitmap/Restore. A maintainer already answered the question in the issue comments in 2019 with a working code snippet.",
    "rationale": "The issue title is tagged [QUESTION] and the body only asks how to accomplish a task — there is no broken behavior described. The maintainer's existing comment answers both questions. This is a usage question with an existing answer, suitable for closing as completed.",
    "codeInvestigation": [
      {
        "file": "binding/SkiaSharp/SKPictureRecorder.cs",
        "lines": "28-54",
        "finding": "SKPicture is created only via SKPictureRecorder.BeginRecording + EndRecording, which records canvas drawing commands. There is no API to construct an SKPicture from pixel data (SKBitmap) — confirming conversion is not possible.",
        "relevance": "direct"
      },
      {
        "file": "binding/SkiaSharp/SKCanvas.cs",
        "lines": "233-236",
        "finding": "canvas.Concat(SKMatrix) concatenates a matrix onto the current canvas transform, allowing any subsequent DrawBitmap call to be transformed — this directly answers the SKMatrix question.",
        "relevance": "direct"
      },
      {
        "file": "binding/SkiaSharp/SKCanvas.cs",
        "lines": "566-586",
        "finding": "DrawBitmap overloads accept position, dest rect, and source/dest rects. Combined with canvas.Save/Translate(x,y)/Restore or canvas.Concat(matrix), arbitrary bitmap placement with transforms is fully supported.",
        "relevance": "related"
      }
    ],
    "keySignals": [
      {
        "text": "Is there any way to convert SKBitmap/ImageSource to SkPicture?",
        "source": "issue body",
        "interpretation": "Asking about a pixel-to-picture conversion API that does not exist — SKPicture records vector commands only."
      },
      {
        "text": "or draw Bitmap with SKMatrix?",
        "source": "issue body",
        "interpretation": "Second sub-question: this IS supported via canvas.Concat(matrix) + DrawBitmap."
      },
      {
        "text": "if you are just wanting to draw with a matrix, then you should be able to just save the canvas, do the transformation, draw the bitmap and then restore",
        "source": "comment by mattleibow",
        "interpretation": "Maintainer already answered with a code example — issue is resolved."
      }
    ],
    "workarounds": [
      "Use canvas.Concat(matrix) + canvas.DrawBitmap(...) to draw a bitmap with an arbitrary SKMatrix transform.",
      "Use canvas.Save() / canvas.Translate(dx, dy) / canvas.DrawBitmap(...) / canvas.Restore() for translate-only transforms.",
      "To record a bitmap draw into an SKPicture, use SKPictureRecorder.BeginRecording() to get a recording canvas, then call canvas.DrawBitmap() on that canvas, and finally call EndRecording() — this records the draw command but not the pixel conversion."
    ],
    "resolution": {
      "hypothesis": "Reporter wants to apply a matrix transform when drawing a bitmap; the canvas transform APIs fully support this. There is no SKBitmap-to-SKPicture pixel conversion API, by design.",
      "proposals": [
        {
          "title": "Apply transform with canvas.Concat",
          "description": "Use canvas.Save(), canvas.Concat(matrix), canvas.DrawBitmap(bitmap, ...), canvas.Restore() to draw a bitmap with any SKMatrix.",
          "category": "workaround",
          "confidence": 0.95,
          "effort": "cost/xs",
          "validated": "yes"
        },
        {
          "title": "Record bitmap draw into SKPicture",
          "description": "If an SKPicture containing a bitmap draw is needed, use SKPictureRecorder: begin recording, draw the bitmap on the recording canvas (with any transforms), then end recording to get an SKPicture.",
          "category": "workaround",
          "confidence": 0.9,
          "effort": "cost/xs",
          "validated": "yes"
        }
      ],
      "recommendedProposal": "Apply transform with canvas.Concat",
      "recommendedReason": "Simplest approach; directly addresses both the matrix and SKPicture recording scenarios. Maintainer already provided a Translate variant of this approach."
    }
  },
  "output": {
    "actionability": {
      "suggestedAction": "close-as-not-a-bug",
      "confidence": 0.88,
      "reason": "Usage question already answered by maintainer in 2019. API behavior is correct and documented. Drawing with a matrix is supported; pixel-to-picture conversion is not an API that exists by design.",
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
        "description": "Summarize the answer to both sub-questions and provide code snippets",
        "risk": "medium",
        "confidence": 0.88,
        "comment": "Thanks for the question!\n\n**Drawing an `SKBitmap` with an `SKMatrix`:**\nYou can apply any transformation by saving the canvas state, applying the matrix, drawing, then restoring:\n\n```csharp\nvar canvas = ...;\nusing (new SKAutoCanvasRestore(canvas)) {\n    canvas.Concat(matrix); // apply your SKMatrix\n    canvas.DrawBitmap(bitmap, 0, 0);\n}\n```\n\n**Converting `SKBitmap` to `SKPicture`:**\nThere is no direct conversion — `SKPicture` captures *drawing commands*, not pixel data. However, if you need an `SKPicture` that contains a bitmap draw, you can record it:\n\n```csharp\nvar bounds = new SKRect(0, 0, bitmap.Width, bitmap.Height);\nusing var recorder = new SKPictureRecorder();\nvar recordingCanvas = recorder.BeginRecording(bounds);\nrecordingCanvas.DrawBitmap(bitmap, 0, 0);\nusing var picture = recorder.EndRecording();\n```\n\nThis records the draw-bitmap command into the picture, which can then be played back or used as a shader."
      },
      {
        "type": "close-issue",
        "description": "Close as answered — both sub-questions have clear answers",
        "risk": "medium",
        "confidence": 0.85,
        "stateReason": "completed"
      }
    ]
  }
}
```

</details>
