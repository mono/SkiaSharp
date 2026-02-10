# Response Guidelines

Tone and structure for `add-comment` action `payload.draftBody` content. Max 2000 chars, inline markdown. Always set `requiresHumanEdit: true`.

## Structure: Acknowledge → Analyze → Ask

1. **Acknowledge** — Recognize what the reporter did well. Be genuine, not effusive.
2. **Analyze** — Direct technical read of the situation. Be specific.
3. **Ask** (when needed) — Request info respectfully. One ask is better than three.

## Tone

- Empathetic, direct, technical. Not lecturing, not buddy-buddy.
- Write as a genuine attempt to understand, not a knowledge dump.
- No emoji, no "Great question!", no forced enthusiasm.
- Frame requests as questions: "Would you be able to..." not "Run..."

## Examples

**Bad:** "The issue here is that NoDependencies has zero external deps, so if fontconfig fails it means the wrong binary. You should run ldd on the .so to confirm."

**Good:** "Thanks for the detailed error output — that's really helpful. The fontconfig error with NoDependencies is interesting because that package shouldn't have any fontconfig dependency at all, which suggests the deployed binary might not actually be from the NoDependencies package. Would you be able to run `ldd` on the `libSkiaSharp.so` in your publish output?"

**Bad:** "This is a known issue. Use NativeAssets.Linux instead."

**Good:** "This looks like it could be related to how .NET Aspire handles native asset deployment — we've seen similar cases where the container ends up with a different binary than expected. Switching to `NativeAssets.Linux` with fontconfig installed has worked for others in this situation."
