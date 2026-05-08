# Issue Triage Report — #2430

| Field | Value |
|-------|-------|
| Repository | mono/SkiaSharp |
| Analyzed | 2026-05-04T17:55:00Z |
| Type | type/question (0.95 (95%)) |
| Area | area/SkiaSharp (0.85 (85%)) |
| Suggested action | close-as-not-a-bug (0.90 (90%)) |

**Issue Summary:** User migrating from Xamarin.Forms to MAUI asks how to load a Lottie animation from an in-memory JSON string (downloaded from a server) using SKLottieView/Skottie.

**Analysis:** The reporter wants to load a Skottie/Lottie animation from an in-memory JSON string rather than a file. The SkiaSharp.Skottie library already has `Animation.Parse(string json)` and `Animation.TryParse(string json, out Animation animation)` that do exactly this.

**Recommendations:** **close-as-not-a-bug** — The requested functionality exists via Animation.Parse(string json). This is a usage question that can be answered and closed.

---

## Classification

| Field | Value |
|-------|-------|
| Type | type/question |
| Area | area/SkiaSharp |
| Platforms | — |
| Backends | — |
| Tenets | — |
| Partner | partner/maui |

## Evidence

### Reproduction

**Environment:** Migrating from Xamarin.Forms to MAUI; animations stored as JSON strings in a database

## Analysis

### Technical Summary

The reporter wants to load a Skottie/Lottie animation from an in-memory JSON string rather than a file. The SkiaSharp.Skottie library already has `Animation.Parse(string json)` and `Animation.TryParse(string json, out Animation animation)` that do exactly this.

### Rationale

This is a usage question — the reporter does not know about the `Animation.Parse` API. No broken behavior is described. The functionality already exists in the `SkiaSharp.Skottie` package.

### Key Signals

- "is there a way to display an animation that is stored in memory inside as a JSON string variable?" — **issue body** (User is unaware of Animation.Parse(string json) which directly accepts a JSON string.)

### Code Investigation

| File | Lines | Relevance | Finding |
|------|-------|-----------|---------|
| `binding/SkiaSharp.Skottie/Animation.cs` | 31-42 | direct | Animation.Parse(string json) and Animation.TryParse(string json, out Animation animation) are public static methods that create an Animation directly from a JSON string, which is exactly what the reporter needs. |

### Workarounds

- Use `Animation.Parse(jsonString)` from the `SkiaSharp.Skottie` namespace to load directly from a JSON string.
- Alternatively, convert the string to a `Stream` or `SKData` and use `Animation.Create(stream)` / `Animation.Create(skData)`.

### Resolution Proposals

**Hypothesis:** The user wants to render a Lottie animation from a JSON string in memory. `Animation.Parse(string json)` already supports this.

1. **Use Animation.Parse(string json)** — fix, confidence 0.95 (95%), cost/xs, validated=yes
   - Pass the JSON string directly to `Animation.Parse()`. Then draw the animation on a `SKCanvas` using `animation.Render(canvas, rect)`.

```csharp
using SkiaSharp.Skottie;

// jsonString is the Lottie JSON downloaded from your server
var animation = Animation.Parse(jsonString);
if (animation != null)
{
    // In your SKCanvas paint handler:
    animation.Seek(progress); // 0.0 to 1.0
    animation.Render(canvas, destRect);
}
```

2. **Use Animation.Create(Stream) from string** — alternative, confidence 0.90 (90%), cost/xs, validated=yes
   - If you prefer a stream-based approach, convert the JSON string to a MemoryStream.

```csharp
using var stream = new System.IO.MemoryStream(System.Text.Encoding.UTF8.GetBytes(jsonString));
var animation = Animation.Create(stream);
```

**Recommended proposal:** Use Animation.Parse(string json)

**Why:** Simplest and most direct approach — no conversion needed.

## Recommendations

### Actionability

| Field | Value |
|-------|-------|
| Suggested action | close-as-not-a-bug |
| Confidence | 0.90 (90%) |
| Reason | The requested functionality exists via Animation.Parse(string json). This is a usage question that can be answered and closed. |
| Suggested repro platform | linux |

### Automatable Actions

| Type | Risk | Confidence | Description | Details |
|------|------|------------|-------------|---------|
| update-labels | low | 0.95 (95%) | Apply question and MAUI partner labels | labels=type/question, area/SkiaSharp, partner/maui |
| add-comment | medium | 0.95 (95%) | Answer with Animation.Parse code example | — |
| close-issue | medium | 0.85 (85%) | Close as answered | stateReason=completed |
