# Issue Triage Report — #755

| Field | Value |
|-------|-------|
| Repository | mono/SkiaSharp |
| Analyzed | 2026-04-27T08:00:00Z |
| Type | type/question (0.75 (75%)) |
| Area | area/SkiaSharp (0.85 (85%)) |
| Suggested action | needs-info (0.75 (75%)) |

**Issue Summary:** Reporter experiences intermittent SystemAccessViolation crashes when using GRContext-based GPU offscreen rendering to build and merge bitmaps on UWP and iOS, and asks for guidance on GPU synchronization, safe bitmap compositing patterns, and crash recovery.

**Analysis:** Reporter is compositing GPU-accelerated offscreen bitmaps using GRContext on UWP and iOS. Intermittent SystemAccessViolation crashes during bitmap merge are likely caused by non-synchronous GPU flush — GRContext.Flush() without synchronous=true submits GPU commands but does not wait for completion, so pixel read-back or subsequent canvas draws may race with pending GPU work. Additional platform complexity: iOS requires OpenGL operations on a designated GL thread; UWP/ANGLE requires the EGL context to be current on the calling thread before GPU API calls. No stack trace was provided, preventing definitive root cause confirmation.

**Recommendations:** **needs-info** — Guidance on GPU flush semantics can be provided, but root cause cannot be confirmed without a stack trace and clarity on threading. The maintainer's unanswered questions from 2019 remain open.

---

## Classification

| Field | Value |
|-------|-------|
| Type | type/question |
| Area | area/SkiaSharp |
| Platforms | os/Windows-Universal-UWP, os/iOS |
| Backends | backend/OpenGL |
| Tenets | tenet/reliability |
| Partner | — |
| Current labels | type/question, backend/OpenGL |

## Evidence

### Reproduction

1. Create a GRContext on UWP or iOS using EGL/OpenGL
2. In a single background thread, receive tiles and for each: create GPU surface, render features, call context.Flush(), take snapshot, convert to bitmap
3. Maintain two progressively-updated bitmaps (features and markups)
4. When merging: draw features bitmap then markup bitmap onto a new canvas
5. Under heavy load (rapid pan/zoom), observe intermittent SystemAccessViolation

**Environment:** UWP and iOS; GPU-accelerated GRContext via EGL/OpenGL; SkiaSharp circa early 2019; single background rendering thread

**Repository links:**
- https://github.com/mono/SkiaSharp/issues/688 — Predecessor question about offscreen GPU rendering, answered by maintainer
- https://github.com/mono/SkiaSharp/issues/976 — Follow-up issue by same reporter: multi-threaded GPU context memory violations

## Analysis

### Technical Summary

Reporter is compositing GPU-accelerated offscreen bitmaps using GRContext on UWP and iOS. Intermittent SystemAccessViolation crashes during bitmap merge are likely caused by non-synchronous GPU flush — GRContext.Flush() without synchronous=true submits GPU commands but does not wait for completion, so pixel read-back or subsequent canvas draws may race with pending GPU work. Additional platform complexity: iOS requires OpenGL operations on a designated GL thread; UWP/ANGLE requires the EGL context to be current on the calling thread before GPU API calls. No stack trace was provided, preventing definitive root cause confirmation.

### Rationale

Classified as type/question because the reporter is asking for usage guidance and crash recovery patterns. The crashes are real but the SkiaSharp API is working as designed — the problem is a combination of non-synchronous flush semantics, GPU thread affinity requirements on iOS, and EGL context binding requirements on UWP. The maintainer already asked clarifying questions about threading that were never answered.

### Key Signals

- "I also thought that the flush method would finish guarantee that the GPU has all the instructions it needs to generate the updates to the canvas." — **issue body** (Reporter believes Flush() is synchronous. GRContext.Flush() without synchronous=true is fire-and-forget — GPU work is submitted but completion is not awaited.)
- "Adding in the 'surface.Dispose()' call at the end of the method significantly reduces the number of times this occurs." — **issue body** (Disposing the surface may incidentally flush and synchronize pending GPU work as part of cleanup, explaining why it reduces but does not eliminate the crash.)
- "It is like the markups bitmap hasn't closed yet, and I'm trying to render it to a canvas." — **issue body** (Accurate intuition — the GPU has not finished rendering the markup surface before its pixels are read back to create a bitmap and drawn to the next canvas.)
- "For some reason, I seem to recall that images created with one context can't be used by another" — **comment by mattleibow** (Maintainer hint: GPU-backed SKImage/SKBitmap objects are context-bound. Sharing them across rendering passes is unsafe unless pixels are fully materialized to CPU first.)

### Code Investigation

| File | Lines | Relevance | Finding |
|------|-------|-----------|---------|
| `binding/SkiaSharp/GRContext.cs` | 140-148 | direct | GRContext.Flush() and GRContext.Flush(bool submit, bool synchronous=false) both exist. The parameterless overload calls the two-param version with synchronous=false, meaning GPU commands are submitted but the call returns before GPU completion. Flush(submit: true, synchronous: true) is needed to wait for GPU work to finish before CPU read-back. |
| `binding/SkiaSharp/SKSurface.cs` | 274-275 | direct | SKSurface.Snapshot() returns an SKImage. For GPU-backed surfaces the snapshot may reference GPU-resident texture memory. If a synchronous flush has not been called, reading that memory from CPU via SKBitmap.FromImage races with in-flight GPU commands. |
| `binding/SkiaSharp/SKBitmap.cs` | 715-726 | related | SKBitmap.FromImage(SKImage) calls image.ReadPixels() internally and can return null if pixel read-back fails. If the underlying GPU texture data is inaccessible or incomplete the method disposes the bitmap and returns null silently — no exception is thrown. |

### Workarounds

- Call grContext.Flush(submit: true, synchronous: true) after drawing and before Snapshot()/SKBitmap.FromImage() — this waits for GPU completion. On iOS this must be on the designated GL thread; on UWP/ANGLE the EGL context must be current on the calling thread.
- Use a CPU raster surface (SKSurface.Create(SKImageInfo)) for the bitmap compositing/merge step. Render each tile to a GPU surface for speed, but merge already-rendered bitmaps on a CPU canvas — eliminates GPU sync complexity entirely for the compositing phase.
- Convert GPU surface pixels explicitly to CPU before compositing: call surface.ReadPixels() to pull data into a pre-allocated buffer, then construct an SKBitmap from that buffer to avoid GPU-resident bitmap sharing.

### Next Questions

- Stack trace for the SystemAccessViolation — which SkiaSharp or native method is at the top?
- SkiaSharp version in use.
- Is a single GRContext shared for both the features surface and markup surface, or are separate contexts used?
- Is the background thread where GPU work happens the same thread where the EGL context was created and made-current?
- Is any SkiaSharp object (GRContext, SKSurface, SKCanvas, SKBitmap) accessed from any other thread, even via async/await or dispatcher queues?

### Resolution Proposals

**Hypothesis:** Non-synchronous GPU flush causes pixel read-back or bitmap draw to race with pending GPU commands, compounded by GPU thread affinity requirements on iOS and EGL context binding requirements on UWP/ANGLE.

1. **Use synchronous flush before snapshot/bitmap operations** — workaround, confidence 0.40 (40%), cost/xs, validated=no
   - Replace grContext.Flush() with grContext.Flush(submit: true, synchronous: true) before Snapshot() and SKBitmap.FromImage(). Null-check the result. Platform caveats: on iOS must execute on the designated GL thread; on UWP/ANGLE the EGL context must be current on the calling thread. Note SKBitmap.FromImage() returns null on failure.

```csharp
// Ensure GPU work completes before CPU read-back (must be on correct GL thread):
grContext.Flush(submit: true, synchronous: true);
using var snapshot = surface.Snapshot();
var bitmap = SKBitmap.FromImage(snapshot);
if (bitmap == null) { /* GPU read-back failed — handle error */ return; }
// Use bitmap here; dispose it when done
```
2. **Use CPU raster surface for the compositing step** — alternative, confidence 0.82 (82%), cost/s, validated=untested
   - Render individual tiles to GPU surfaces for speed, then composite (merge features + markup bitmaps) using a CPU raster surface created with SKSurface.Create(SKImageInfo). The GPU surfaces produce CPU-resident bitmaps after a synchronous flush; compositing them on a CPU canvas avoids GPU synchronization complexity entirely.

**Recommended proposal:** Use CPU raster surface for the compositing step

**Why:** Avoids GPU synchronization entirely for the merge step. The GPU is still used for individual tile rendering where it provides speed benefit; the compositing phase does not require GPU acceleration and is where the crashes occur.

## Recommendations

### Actionability

| Field | Value |
|-------|-------|
| Suggested action | needs-info |
| Confidence | 0.75 (75%) |
| Reason | Guidance on GPU flush semantics can be provided, but root cause cannot be confirmed without a stack trace and clarity on threading. The maintainer's unanswered questions from 2019 remain open. |
| Suggested repro platform | linux |

### Missing Info

- Stack trace for the SystemAccessViolation exception
- SkiaSharp version in use
- Whether a single GRContext is shared between features and markup surface rendering
- Whether any thread boundary is crossed, even via async/await or dispatcher queues

### Automatable Actions

| Type | Risk | Confidence | Description | Details |
|------|------|------------|-------------|---------|
| update-labels | low | 0.90 (90%) | Add UWP and iOS platform labels and reliability tenet | labels=type/question, area/SkiaSharp, os/Windows-Universal-UWP, os/iOS, backend/OpenGL, tenet/reliability |
| add-comment | medium | 0.75 (75%) | Provide guidance on GPU flush semantics and request missing info | — |
| link-related | low | 0.95 (95%) | Link to predecessor question #688 | linkedIssue=#688 |
| link-related | low | 0.95 (95%) | Link to follow-up multi-thread issue #976 | linkedIssue=#976 |

**Comment draft for `add-comment`:**

```markdown
Thanks for the detailed description. The intermittent crash is most likely a **GPU synchronization issue** combined with **GPU thread-affinity requirements**.

**The core problem:** `grContext.Flush()` (without parameters) submits GPU commands but does **not** wait for them to complete. If you then call `Snapshot()` or draw the resulting bitmap to another canvas, you may be reading memory the GPU is still writing.

**Possible directions to investigate:**

1. **Synchronous flush** — Try flushing synchronously before snapshot/bitmap creation. Note that `SKBitmap.FromImage()` can return null if the GPU read-back fails:
```csharp
grContext.Flush(submit: true, synchronous: true);
using var snapshot = surface.Snapshot();
var bitmap = SKBitmap.FromImage(snapshot);
if (bitmap == null) { /* GPU read-back failed */ return; }
// Use bitmap here; dispose it when done
```
> ⚠️ **Platform caveats:** On iOS, all OpenGL ES calls must run on the designated GL render thread. On UWP/ANGLE, the EGL context must be bound (`MakeCurrent`) on the calling thread before any GPU call.

2. **CPU compositing** — For the merge step (drawing features + markup bitmaps together), consider switching to a CPU raster surface (`SKSurface.Create(SKImageInfo)`). You can still use the GPU for individual tile rendering, but compositing two already-rendered bitmaps on a CPU canvas avoids GPU sync complexity entirely for that phase.

To help diagnose further, could you provide:
- A stack trace for the `SystemAccessViolation`
- The SkiaSharp version you're using
- Whether a single `GRContext` is used for both the features and markup surfaces
- Whether the rendering thread is the **same thread** where the EGL context was created and made-current

Also see #976 where the same scenario is explored in a multi-threaded context.
```

<details>
<summary>Raw JSON</summary>

```json
{
  "meta": {
    "schemaVersion": "1.0",
    "number": 755,
    "repo": "mono/SkiaSharp",
    "analyzedAt": "2026-04-27T08:00:00Z",
    "currentLabels": [
      "type/question",
      "backend/OpenGL"
    ]
  },
  "summary": "Reporter experiences intermittent SystemAccessViolation crashes when using GRContext-based GPU offscreen rendering to build and merge bitmaps on UWP and iOS, and asks for guidance on GPU synchronization, safe bitmap compositing patterns, and crash recovery.",
  "classification": {
    "type": {
      "value": "type/question",
      "confidence": 0.75
    },
    "area": {
      "value": "area/SkiaSharp",
      "confidence": 0.85
    },
    "platforms": [
      "os/Windows-Universal-UWP",
      "os/iOS"
    ],
    "backends": [
      "backend/OpenGL"
    ],
    "tenets": [
      "tenet/reliability"
    ]
  },
  "evidence": {
    "reproEvidence": {
      "stepsToReproduce": [
        "Create a GRContext on UWP or iOS using EGL/OpenGL",
        "In a single background thread, receive tiles and for each: create GPU surface, render features, call context.Flush(), take snapshot, convert to bitmap",
        "Maintain two progressively-updated bitmaps (features and markups)",
        "When merging: draw features bitmap then markup bitmap onto a new canvas",
        "Under heavy load (rapid pan/zoom), observe intermittent SystemAccessViolation"
      ],
      "environmentDetails": "UWP and iOS; GPU-accelerated GRContext via EGL/OpenGL; SkiaSharp circa early 2019; single background rendering thread",
      "repoLinks": [
        {
          "url": "https://github.com/mono/SkiaSharp/issues/688",
          "description": "Predecessor question about offscreen GPU rendering, answered by maintainer"
        },
        {
          "url": "https://github.com/mono/SkiaSharp/issues/976",
          "description": "Follow-up issue by same reporter: multi-threaded GPU context memory violations"
        }
      ]
    }
  },
  "analysis": {
    "summary": "Reporter is compositing GPU-accelerated offscreen bitmaps using GRContext on UWP and iOS. Intermittent SystemAccessViolation crashes during bitmap merge are likely caused by non-synchronous GPU flush — GRContext.Flush() without synchronous=true submits GPU commands but does not wait for completion, so pixel read-back or subsequent canvas draws may race with pending GPU work. Additional platform complexity: iOS requires OpenGL operations on a designated GL thread; UWP/ANGLE requires the EGL context to be current on the calling thread before GPU API calls. No stack trace was provided, preventing definitive root cause confirmation.",
    "rationale": "Classified as type/question because the reporter is asking for usage guidance and crash recovery patterns. The crashes are real but the SkiaSharp API is working as designed — the problem is a combination of non-synchronous flush semantics, GPU thread affinity requirements on iOS, and EGL context binding requirements on UWP. The maintainer already asked clarifying questions about threading that were never answered.",
    "codeInvestigation": [
      {
        "file": "binding/SkiaSharp/GRContext.cs",
        "lines": "140-148",
        "finding": "GRContext.Flush() and GRContext.Flush(bool submit, bool synchronous=false) both exist. The parameterless overload calls the two-param version with synchronous=false, meaning GPU commands are submitted but the call returns before GPU completion. Flush(submit: true, synchronous: true) is needed to wait for GPU work to finish before CPU read-back.",
        "relevance": "direct"
      },
      {
        "file": "binding/SkiaSharp/SKSurface.cs",
        "lines": "274-275",
        "finding": "SKSurface.Snapshot() returns an SKImage. For GPU-backed surfaces the snapshot may reference GPU-resident texture memory. If a synchronous flush has not been called, reading that memory from CPU via SKBitmap.FromImage races with in-flight GPU commands.",
        "relevance": "direct"
      },
      {
        "file": "binding/SkiaSharp/SKBitmap.cs",
        "lines": "715-726",
        "finding": "SKBitmap.FromImage(SKImage) calls image.ReadPixels() internally and can return null if pixel read-back fails. If the underlying GPU texture data is inaccessible or incomplete the method disposes the bitmap and returns null silently — no exception is thrown.",
        "relevance": "related"
      }
    ],
    "keySignals": [
      {
        "text": "I also thought that the flush method would finish guarantee that the GPU has all the instructions it needs to generate the updates to the canvas.",
        "source": "issue body",
        "interpretation": "Reporter believes Flush() is synchronous. GRContext.Flush() without synchronous=true is fire-and-forget — GPU work is submitted but completion is not awaited."
      },
      {
        "text": "Adding in the 'surface.Dispose()' call at the end of the method significantly reduces the number of times this occurs.",
        "source": "issue body",
        "interpretation": "Disposing the surface may incidentally flush and synchronize pending GPU work as part of cleanup, explaining why it reduces but does not eliminate the crash."
      },
      {
        "text": "It is like the markups bitmap hasn't closed yet, and I'm trying to render it to a canvas.",
        "source": "issue body",
        "interpretation": "Accurate intuition — the GPU has not finished rendering the markup surface before its pixels are read back to create a bitmap and drawn to the next canvas."
      },
      {
        "text": "For some reason, I seem to recall that images created with one context can't be used by another",
        "source": "comment by mattleibow",
        "interpretation": "Maintainer hint: GPU-backed SKImage/SKBitmap objects are context-bound. Sharing them across rendering passes is unsafe unless pixels are fully materialized to CPU first."
      }
    ],
    "nextQuestions": [
      "Stack trace for the SystemAccessViolation — which SkiaSharp or native method is at the top?",
      "SkiaSharp version in use.",
      "Is a single GRContext shared for both the features surface and markup surface, or are separate contexts used?",
      "Is the background thread where GPU work happens the same thread where the EGL context was created and made-current?",
      "Is any SkiaSharp object (GRContext, SKSurface, SKCanvas, SKBitmap) accessed from any other thread, even via async/await or dispatcher queues?"
    ],
    "workarounds": [
      "Call grContext.Flush(submit: true, synchronous: true) after drawing and before Snapshot()/SKBitmap.FromImage() — this waits for GPU completion. On iOS this must be on the designated GL thread; on UWP/ANGLE the EGL context must be current on the calling thread.",
      "Use a CPU raster surface (SKSurface.Create(SKImageInfo)) for the bitmap compositing/merge step. Render each tile to a GPU surface for speed, but merge already-rendered bitmaps on a CPU canvas — eliminates GPU sync complexity entirely for the compositing phase.",
      "Convert GPU surface pixels explicitly to CPU before compositing: call surface.ReadPixels() to pull data into a pre-allocated buffer, then construct an SKBitmap from that buffer to avoid GPU-resident bitmap sharing."
    ],
    "resolution": {
      "hypothesis": "Non-synchronous GPU flush causes pixel read-back or bitmap draw to race with pending GPU commands, compounded by GPU thread affinity requirements on iOS and EGL context binding requirements on UWP/ANGLE.",
      "proposals": [
        {
          "title": "Use synchronous flush before snapshot/bitmap operations",
          "description": "Replace grContext.Flush() with grContext.Flush(submit: true, synchronous: true) before Snapshot() and SKBitmap.FromImage(). Null-check the result. Platform caveats: on iOS must execute on the designated GL thread; on UWP/ANGLE the EGL context must be current on the calling thread. Note SKBitmap.FromImage() returns null on failure.",
          "category": "workaround",
          "codeSnippet": "// Ensure GPU work completes before CPU read-back (must be on correct GL thread):\ngrContext.Flush(submit: true, synchronous: true);\nusing var snapshot = surface.Snapshot();\nvar bitmap = SKBitmap.FromImage(snapshot);\nif (bitmap == null) { /* GPU read-back failed — handle error */ return; }\n// Use bitmap here; dispose it when done",
          "confidence": 0.4,
          "effort": "cost/xs",
          "validated": "no"
        },
        {
          "title": "Use CPU raster surface for the compositing step",
          "description": "Render individual tiles to GPU surfaces for speed, then composite (merge features + markup bitmaps) using a CPU raster surface created with SKSurface.Create(SKImageInfo). The GPU surfaces produce CPU-resident bitmaps after a synchronous flush; compositing them on a CPU canvas avoids GPU synchronization complexity entirely.",
          "category": "alternative",
          "confidence": 0.82,
          "effort": "cost/s",
          "validated": "untested"
        }
      ],
      "recommendedProposal": "Use CPU raster surface for the compositing step",
      "recommendedReason": "Avoids GPU synchronization entirely for the merge step. The GPU is still used for individual tile rendering where it provides speed benefit; the compositing phase does not require GPU acceleration and is where the crashes occur."
    }
  },
  "output": {
    "actionability": {
      "suggestedAction": "needs-info",
      "confidence": 0.75,
      "reason": "Guidance on GPU flush semantics can be provided, but root cause cannot be confirmed without a stack trace and clarity on threading. The maintainer's unanswered questions from 2019 remain open.",
      "suggestedReproPlatform": "linux"
    },
    "missingInfo": [
      "Stack trace for the SystemAccessViolation exception",
      "SkiaSharp version in use",
      "Whether a single GRContext is shared between features and markup surface rendering",
      "Whether any thread boundary is crossed, even via async/await or dispatcher queues"
    ],
    "actions": [
      {
        "type": "update-labels",
        "description": "Add UWP and iOS platform labels and reliability tenet",
        "risk": "low",
        "confidence": 0.9,
        "labels": [
          "type/question",
          "area/SkiaSharp",
          "os/Windows-Universal-UWP",
          "os/iOS",
          "backend/OpenGL",
          "tenet/reliability"
        ]
      },
      {
        "type": "add-comment",
        "description": "Provide guidance on GPU flush semantics and request missing info",
        "risk": "medium",
        "confidence": 0.75,
        "comment": "Thanks for the detailed description. The intermittent crash is most likely a **GPU synchronization issue** combined with **GPU thread-affinity requirements**.\n\n**The core problem:** `grContext.Flush()` (without parameters) submits GPU commands but does **not** wait for them to complete. If you then call `Snapshot()` or draw the resulting bitmap to another canvas, you may be reading memory the GPU is still writing.\n\n**Possible directions to investigate:**\n\n1. **Synchronous flush** — Try flushing synchronously before snapshot/bitmap creation. Note that `SKBitmap.FromImage()` can return null if the GPU read-back fails:\n```csharp\ngrContext.Flush(submit: true, synchronous: true);\nusing var snapshot = surface.Snapshot();\nvar bitmap = SKBitmap.FromImage(snapshot);\nif (bitmap == null) { /* GPU read-back failed */ return; }\n// Use bitmap here; dispose it when done\n```\n> ⚠️ **Platform caveats:** On iOS, all OpenGL ES calls must run on the designated GL render thread. On UWP/ANGLE, the EGL context must be bound (`MakeCurrent`) on the calling thread before any GPU call.\n\n2. **CPU compositing** — For the merge step (drawing features + markup bitmaps together), consider switching to a CPU raster surface (`SKSurface.Create(SKImageInfo)`). You can still use the GPU for individual tile rendering, but compositing two already-rendered bitmaps on a CPU canvas avoids GPU sync complexity entirely for that phase.\n\nTo help diagnose further, could you provide:\n- A stack trace for the `SystemAccessViolation`\n- The SkiaSharp version you're using\n- Whether a single `GRContext` is used for both the features and markup surfaces\n- Whether the rendering thread is the **same thread** where the EGL context was created and made-current\n\nAlso see #976 where the same scenario is explored in a multi-threaded context."
      },
      {
        "type": "link-related",
        "description": "Link to predecessor question #688",
        "risk": "low",
        "confidence": 0.95,
        "linkedIssue": 688
      },
      {
        "type": "link-related",
        "description": "Link to follow-up multi-thread issue #976",
        "risk": "low",
        "confidence": 0.95,
        "linkedIssue": 976
      }
    ]
  }
}
```

</details>
