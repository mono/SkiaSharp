# Response Guidelines

Tone and structure for `add-comment` action `payload.draftBody` content. Max 2000 chars, inline markdown. Always set `requiresHumanEdit: true`.

## Structure: Acknowledge → Workaround → Context

Prioritize giving the developer something actionable **now**.

1. **Acknowledge** (1 sentence) — Recognize the report. Be genuine, not effusive.
2. **Workaround** (if available) — Lead with a concrete fix they can use today. Frame as "Here's a workaround you can use while we investigate" — not "could you try X?". Use fenced code blocks (` ``` `) for any code. Explicitly name which `resolution.proposals[]` entry you're recommending and why.
3. **Context / Ask** — Brief technical analysis. If no workaround exists, this becomes the main body: explain what we know so far, then make one focused ask for missing info.

### When you HAVE a workaround

Lead with it immediately after the acknowledgement. Confidence guides framing:
- **High confidence (≥0.8):** State it directly — "Here's a workaround while we investigate."
- **Low confidence (<0.8):** Add a caveat — "This has worked in similar cases, but we'd appreciate hearing whether it resolves your issue too."

### When you DON'T have a workaround

Say so honestly. Shift the response toward diagnosis: explain the likely cause, state what we'll investigate, and ask for one piece of missing info if needed.

## Tone

- Empathetic, direct, technical. No emoji, no "Great question!", no forced enthusiasm.
- "Would you be able to..." not "Run..." for asks.

## Examples

**With workaround** (recommends proposal "Switch to NativeAssets.Linux", confidence 0.85):

> Thanks for the detailed error output.
>
> Here's a workaround you can use while we investigate: replace `SkiaSharp.NativeAssets.Linux.NoDependencies` with `SkiaSharp.NativeAssets.Linux` and install fontconfig in your container:
>
> ```dockerfile
> RUN apt-get update && apt-get install -y libfontconfig1
> ```
>
> The fontconfig error shouldn't happen with NoDependencies at all, which suggests the deployed binary may not be from that package. We'll dig into why Aspire might be substituting it. If switching packages resolves the crash, that confirms the diagnosis.

**Without workaround** (no viable proposal):

> Thanks for the stack trace — that's exactly what we needed.
>
> We don't have a workaround for this yet. The crash is in Android's view disposal path, and it looks like the native surface is being released before `OnDetachedFromWindow` completes — likely a threading issue specific to Android's lifecycle.
>
> We'll need to dig into the lifecycle timing to pin this down. Could you confirm which Android version(s) reproduce this? That'll help us narrow down whether it's OS-version-specific.
