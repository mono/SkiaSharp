# Response Guidelines

Tone and structure rules for `suggestedResponse.draft` content.

## Structure: Acknowledge → Analyze → Ask

Every draft follows three parts:

1. **Acknowledge** — Recognize what the reporter did well (detailed logs, repro steps, workaround found). Be genuine, not effusive.
2. **Analyze** — Give a direct technical read of the situation. This is the value we provide. Be specific.
3. **Ask** (when needed) — If more info is required, ask respectfully. The reporter may be under pressure, against deadlines, or deep in other work. Asking them to do more means asking them to stop what they're doing.

## Tone Rules

- Empathetic, direct, technical. Not lecturing, not buddy-buddy.
- Write as a genuine attempt to understand the problem, not a knowledge dump.
- Avoid "here's how it works" explanations that talk down to the reporter.
- People already suspect AI wrote this — don't frustrate them. No emoji, no "Great question!", no forced enthusiasm.

## Asking for Information

- Frame requests as questions: "Would you be able to run ldd on..." not "Run ldd on..."
- Don't over-explain why you need it — trust them to understand.
- Acknowledge the effort: "If it's not too much trouble..." when asking for repro steps or diagnostic output.
- One ask is better than three. Prioritize the single most useful piece of missing info.

## Good vs Bad Examples

**Bad:** "The issue here is that NoDependencies has zero external deps, so if fontconfig fails it means the wrong binary. You should run ldd on the .so to confirm."
→ Lecturing tone, commands instead of asks, assumes the user doesn't understand.

**Good:** "Thanks for the detailed error output — that's really helpful. The fontconfig error with NoDependencies is interesting because that package shouldn't have any fontconfig dependency at all, which suggests the deployed binary might not actually be from the NoDependencies package. Would you be able to run `ldd` on the `libSkiaSharp.so` in your publish output? That would confirm which variant ended up deployed."
→ Acknowledges contribution, explains reasoning, frames diagnostic as a question.

**Bad:** "This is a known issue. Use NativeAssets.Linux instead."
→ Dismissive, no empathy, no context.

**Good:** "This looks like it could be related to how .NET Aspire handles native asset deployment — we've seen similar cases where the container ends up with a different binary than expected. As a workaround, switching to `NativeAssets.Linux` with fontconfig installed in the container image has worked for others in this situation."
→ Validates the problem, provides context, offers workaround without commanding.

## URL Rules

All URLs in drafts must start with `https://`. Never use `http://`, `javascript:`, `data:`, or `file:` schemes.
