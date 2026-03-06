# Repro Anti-Patterns

❌ **NEVER do these during reproduction.** Key Rules in SKILL.md are the primary principles —
this file provides concrete examples of violations and additional patterns.

---

## Methodology

### 1. Hypothesis-driven experiment design

Don't build custom experiments to confirm a triage hypothesis. Triage provides educated guesses —
your job is to reproduce what the reporter described, not validate a theory. (Rule 8)

**Example:** Triage says "P/Invoke overhead," so you create a CPU-only microbenchmark instead of
running the reporter's GPU benchmark. That's not reproduction — it's speculation.

### 2. Skipping the reporter's provided baseline

If the reporter provides both sides of a comparison (working native app + broken SkiaSharp app,
v2 code + v3 code, valid input + corrupt input), run BOTH on your machine. Don't take claimed
numbers on faith and don't run only the failing side. (Rule 7)

### 3. Mismatched comparison conditions

Never compare across different rendering modes (GPU vs CPU), different backends (GL vs Metal), or
different input types. If you can't match conditions, change BOTH sides to match. (Rule 9)

**Example:** Measuring SkiaSharp at 33fps on CPU raster and native Skia at 120fps on GPU, then
claiming "3.6x P/Invoke overhead." The mode difference explains the entire gap.

### 4. Giving up too early

Try multiple approaches, versions, and platforms before concluding `not-reproduced`. If the first
attempt fails, change something meaningful — a different platform, version, or API approach.
Retrying the same thing with the same variables isn't persistence, it's stubbornness.

### 5. Building from source first

Always start with NuGet packages. Building SkiaSharp from source is only for Phase 3C (testing
main branch). NuGet packages match what real users have.

---

## Environment & Versions

### 6. Abandoning on environment issues

Missing workloads, `sudo` prompts, Docker timeouts, and missing Playwright are fixable — not
blockers. Install what's missing, use user-local installs instead of `sudo`, retry Docker with
longer timeouts. **Network operations (git clone, git-sync-deps, package restore) may be slow —
retry 2-3 times before declaring failure.** Native builds (Skia, GN/Ninja) can take 15+ minutes;
that's normal. Setup failures mean `needs-platform`, not `not-reproduced`. Only conclude
`needs-platform` when the platform itself is genuinely unavailable.

### 7. Pre-emptive version or TFM assumptions

.NET is forward-compatible — a `net8.0` library works on `net10.0` apps via NuGet TFM fallback.
Never claim "doesn't support .NET X" without actually testing. Inspect the nupkg metadata before
claiming incompatibility.

**Exception:** Platform-specific TFMs (e.g., `net8.0-ios`) where platform-specific native assets
are required.

### 8. Skipping Docker for Linux bugs

If the issue involves Linux and you're not on Linux, try Docker before concluding
`needs-platform`. Docker is readily available and handles most Linux reproduction scenarios.

---

## Verification

### 9. Fabricating output or evidence

Never use echo/print statements to simulate command output, claim "reproduced" from static code
analysis without executing the code, or report environment details you didn't actually check. If
you cannot run it, report `needs-platform`.

### 10. Accepting measurements without visual validation

Before accepting ANY FPS, timing, or rendering measurements, confirm the app is actually rendering
content. Take a screenshot, save a frame to PNG, or check pixel values. High FPS on a blank screen
means your drawing code isn't executing — not that SkiaSharp is fast. This commonly happens when
project files are created manually instead of using `dotnet new` templates (missing Info.plist,
bundle structure, etc.).

### 11. Mismarking step results

A step's `result` records the **technical outcome**, not whether it matched your expectation.
A build that fails is `result: "failure"` even if that failure confirms the bug.

| You Run | What Happens | Step Result | Why |
|---------|--------------|-------------|-----|
| `dotnet build` | Build succeeds, exits 0 | `success` | Command succeeded |
| `dotnet build` | Build fails with CS0117 | `failure` | Command failed |
| `dotnet build` (reporter said it fails) | Build fails with CS0117 | **`failure`** | Matching the report doesn't change the technical outcome |
| Render image | Exits 0 but pixels wrong | `wrong-output` | Process succeeded, output incorrect |

---

## Conclusions & Output

### 12. Wrong conclusion type

Use `reproduced`/`not-reproduced` for **bugs** and `confirmed`/`not-confirmed` for
**enhancements and feature requests**. "Reproduced" means "reported misbehavior was observed" —
it doesn't apply to "the feature is missing." And vice versa: `confirmed` means "reporter's
non-bug claim was verified" — it doesn't apply to behavioral bugs.
