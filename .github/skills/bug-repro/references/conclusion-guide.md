# Conclusion Guide

How to choose the correct `conclusion` value for a bug reproduction attempt.

---

## ⚠️ Critical Principle: Factual vs Editorial

**Reproduction is FACTUAL, not editorial.**

| Question | Type | Who Decides |
|----------|------|-------------|
| "Did the reported behavior occur?" | **Factual** | You (the reproducer) |
| "Is this behavior a defect?" | **Editorial** | Maintainers |
| "Should this be fixed?" | **Editorial** | Maintainers |

**Your job is ONLY to answer the factual question.** If the reporter says "I get error X" and you get error X, that's `reproduced` — even if error X is intentional, documented, or working-as-designed.

**Example:** Reporter says "SKMatrix.MakeIdentity() gives CS0117 in v3.0". You try it and get CS0117. Conclusion = `reproduced`. Your opinion that "this is an intentional API rename" goes in `notes`, NOT in the conclusion.

---

## Decision Flowchart

```
Did the REPORTED BEHAVIOR occur?
├─ Yes, observed what reporter described
│  ├─ Crash/exception/error? → reproduced
│  └─ Wrong visual output? → wrong-output
├─ No, behavior differs from report → not-reproduced
├─ Couldn't run at all
│  ├─ Missing platform/OS? → needs-platform
│  ├─ Missing hardware? → needs-hardware
│  └─ Missing info/assets? → partial or inconclusive
└─ Some aspects matched, some didn't → partial
```

---

## Conclusion Values

### `reproduced`

**The reported behavior was observed.** This is a factual statement, not a judgment.

- **Required evidence:** ≥1 reproduction step with `result: "failure"` or `result: "wrong-output"`
- **Required:** `primaryError` describing what was observed (error message, exception, incorrect value)
- **Use when:** the behavior the reporter described actually occurred — crash, exception, compiler error, wrong return value, etc.
- **Example:** `"SKMatrix.MapRect returns normalized rect instead of preserving orientation"`
- **Example (Breaking Change):** `"Build fails with CS0117 as reported (intentional breaking change in v3.0)"`

**⚠️ CRITICAL: "Working as Designed" is still `reproduced`.**
If the user reports a breaking change error (e.g., CS0117), and you verify they are correct (it does fail with CS0117), the conclusion is `reproduced`. Do NOT downgrade to `not-reproduced`. Use `notes` to explain it is intentional.

**Example of correct handling:**
- Reporter: "MakeIdentity() gives CS0117 error"
- You observe: CS0117 error
- Conclusion: `reproduced`
- Notes: "Reproduced as reported. Note: this API was intentionally renamed in v3.0 — see migration guide. This may be working-as-designed."

### `not-reproduced`

**The reported behavior was NOT observed.** All steps completed differently than the reporter described.

- **Required evidence:** ≥1 reproduction step with `result: "success"`
- **Required:** description of what WAS observed — prove you actually tried
- **Common reasons:** already fixed in current version, environment difference, missing repro details from reporter
- **Important:** this does NOT mean the bug doesn't exist. It means it could not be observed in the current environment. Always note what was tested so humans can evaluate whether the reproduction attempt was valid.

**⚠️ This is NOT for "it's not a bug":** If the reported behavior occurred (you saw the same error/crash/output), use `reproduced` even if you believe the behavior is intentional. `not-reproduced` means the behavior literally didn't happen.

### `wrong-output`

Special case of `reproduced` for visual/rendering bugs.

- **Use when:** the process exits successfully (no crash, no exception) but the output is visually incorrect
- **Required evidence:** description of expected vs actual output
- **Examples:**
  - `"Expected blue rectangle, got garbled pixels"`
  - `"Image decoded but colors are inverted"`
  - `"Text rendered with wrong font metrics — baseline offset by 4px"`
- **No automated pixel diff is available.** Describe the visual difference in plain language.
- **If the output is a file**, note its path so a human can inspect it.

### `needs-platform`

Cannot reproduce because the required platform or OS is not available.

- **Required evidence:** which platform is needed and why
- **Before concluding:** If the issue is Linux-specific, **try Docker first** (`--platform linux/amd64` or `linux/arm64`). Only conclude `needs-platform` if Docker can't help (e.g., the issue needs Windows GUI, iOS device, or Android).
- **Also use when:** a native rebuild is required (native rebuilds are forbidden during bug reproduction)
- **Examples:**
  - `"Bug reported on Windows with WPF/DirectX backend — Docker cannot run Windows GUI apps"`
  - `"Reproducing requires building native libs with a modified C API — not possible in repro mode"`
  - `"Issue requires iOS device for Metal rendering — Docker cannot simulate iOS"`

### `needs-hardware`

Cannot reproduce because specific hardware is required.

- **Required evidence:** what hardware is needed
- **Distinct from `needs-platform`:** the platform IS available but specific hardware is not
- **Examples:**
  - `"Requires Metal on Apple Silicon GPU — current env has no GPU access"`
  - `"Bug only manifests on iOS device (not simulator)"`
  - `"Requires Android device with specific Vulkan driver version"`

### `partial`

Some aspects of the bug were reproduced, others could not be verified.

- **Required evidence:** what WAS reproduced AND what remains unverified
- **Required:** populate `blockers[]` with specific reasons for incomplete reproduction
- **Use when:** you confirmed part of the problem but hit a wall on the rest
- **Example:** `"Crash reproduced on simple case but reporter's specific font file is unavailable for full verification"`

### `inconclusive`

Cannot determine whether the bug exists. Results are ambiguous or information is insufficient.

- **Required evidence:** what was attempted and why results are ambiguous
- **Common reasons:** issue description too vague, repro code incomplete, behavior is intermittent
- **Last resort.** Prefer any other conclusion when possible. If you can partially reproduce, use `partial`. If you tried and everything passed, use `not-reproduced`.

---

## Supporting Fields

### `assessment` (optional)

A structured, **editorial** classification of what you think is happening, without corrupting the factual `conclusion`.

Use this when the behavior is reproduced but appears intentional (API rename, breaking change, documentation gap, etc.).

Recommended values:
- `"unknown"`
- `"likely-bug"`
- `"working-as-designed"`
- `"breaking-change"`
- `"docs-gap"`
- `"user-error"`

**Example (#3279-style):** conclusion = `reproduced`, assessment = `breaking-change`, notes explain the rename/migration guidance.

### `blockers[]`

A list of specific reasons reproduction was incomplete. Each entry should be actionable — a human reading it should know exactly what's missing.

**Good blockers:**
- `"Requires Windows — current environment is macOS"`
- `"Reporter's input file (corrupted.png) not attached to issue"`
- `"Needs native rebuild to test C API change — forbidden in repro mode"`
- `"Intermittent: passed 10/10 runs but reporter says it fails ~20% of the time"`

**Bad blockers:**
- `"Couldn't reproduce"` — too vague, not actionable
- `"Need more info"` — say what info specifically

### Blocker evidence requirements

Version/compatibility blockers must cite concrete evidence:

| Blocker Type | Required Evidence |
|--------------|-------------------|
| "Version X won't work" | nupkg TFM listing showing no compatible target, OR actual build/runtime error |
| "No native binary" | nupkg `runtimes/` listing showing missing RID, AND confirmation Rosetta isn't viable |
| "Platform unavailable" | Attempted on available platform, noted specific limitation |

**Anti-pattern:** "Version 1.60.1 is too old for .NET 8" — this is speculation. Spend 30 seconds checking the nupkg before claiming blocked.

### `notes`

Free-text summary for humans. Write 1–3 sentences covering:
1. What you did (the approach)
2. What you observed (the result)
3. Any caveats or suggestions for further investigation

**Editorial observations go here.** If you believe the behavior is:
- Intentional / working-as-designed
- A documentation issue rather than a code bug
- A breaking change that was announced in release notes
- User error or misunderstanding

...put that assessment in `notes`, NOT in the `conclusion`. The conclusion is factual; notes are for interpretation.

**Example:** `"Reproduced: CS0117 errors occur when using SKMatrix.MakeIdentity() in v3.0. However, this appears to be an intentional API rename (now SKMatrix.Identity property) — see migration guide. Suggest closing as by-design with documentation pointer."`

---

## Confidence and Human Review

Note these situations in the `notes` field — they indicate the reproduction may need human follow-up:

- Conclusion is `partial` or `inconclusive` — always note why
- Conclusion is `not-reproduced` but the issue has multiple confirmations from other users
- The reproduction environment differs significantly from the reporter's environment
- Visual output was produced but correctness is subjective (e.g., font rendering differences)
- The bug is intermittent and your limited runs may not have triggered it

High-confidence conclusions (less likely to need review):

- `reproduced` or `wrong-output` with clear, unambiguous evidence
- `needs-platform` or `needs-hardware` with an obvious platform mismatch
- `not-reproduced` AND you closely matched the reporter's environment

**When in doubt, note the uncertainty.** A cautious conclusion is better than an overconfident one.

---

## Scope Derivation (from Phase 3D cross-platform verification)

After cross-platform verification, set the `scope` field in the output JSON:

| Cross-platform results | `scope` value |
|----------------------|--------------|
| Reproduced on ≥2 platforms | `"universal"` |
| Reproduced on primary only, not on alternative | `"platform-specific/{platform}"` (e.g., `"platform-specific/wasm"`) |
| Not reproduced on primary, reproduced on alternative | `"platform-specific/{alt-platform}"` |
| Phase 3D skipped (pure API bug, timeout, etc.) | `"unknown"` |
| Only tested one platform (no 3D) | `"unknown"` |

**Platform names for scope:** `macos`, `linux`, `windows`, `wasm`, `ios`, `android`

The `scope` field gives the downstream fix skill an immediate signal about where to look.
Always record the reason if scope is `"unknown"` — in the `notes` field.
