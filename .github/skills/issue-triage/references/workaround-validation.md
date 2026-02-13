# Workaround Validation Workflow

Multi-agent validation for code workarounds in triage responses. Prevents publishing broken code.

## Gate Condition

**Trigger when BOTH true:**
1. `resolution.proposals[]` exists (non-empty)
2. Any proposal `description` or `add-comment` `draftBody` contains fenced code (`` ``` ``) or inline `SK*` API calls

**Skip when:** prose-only proposals, diagnostic commands only (`ldd`, `dotnet --info`), or `close-as-duplicate`.

## Architecture

```
Phase 3 (Analyze) → resolution + draftBody
        │
        ▼
  ┌─ Code gate ─┐
  │  Has code?   │
  │  No → skip   │
  └──────┬───────┘
         │ Yes
         ▼
  ┌──────────────────────────────────────┐
  │  3 parallel explore agents (Haiku)   │
  │                                      │
  │  Agent 1: API Correctness            │
  │  Agent 2: Behavioral Correctness     │
  │  Agent 3: Platform Safety            │
  └──────┬───────────────────────────────┘
         │
         ▼
  Synthesize → pass / fix / reject
         │
         ▼
  Phase 4 (Schema Validate) continues
```

Uses `explore` agents (read-only, Haiku, has grep/glob/view). All 3 are independent — launch in parallel.

## The 3 Validation Agents

### Agent 1: API Correctness

**Checks:** Every `SK*` type exists in `binding/SkiaSharp/`, method names and signatures match, factory-vs-constructor usage is correct, no `[Obsolete]` APIs recommended.

**Prompt template:**
```
Validate that every SkiaSharp API call in this code exists with correct signatures.
For each SK* call: grep binding/SkiaSharp/ for the type and method, verify param
types match, flag any [Obsolete] methods.

Code: {extracted_code}
Context: {issue_title} ({issue_type})

Return JSON: { "verdict": "pass|fail|warn", "issues": [{ "api", "problem",
"severity": "error|warning", "fix" }], "checked_count": N, "confidence": 0.0-1.0 }
```

### Agent 2: Behavioral Correctness

**Checks:** IDisposable objects disposed, same-instance trap (`Subset()`, `ToRasterImage()`) handled, factory nulls checked, no cross-thread SKCanvas/SKPaint/SKPath sharing, code actually solves reporter's problem.

**Prompt template:**
```
Check this SkiaSharp workaround for memory bugs, null-safety, and whether it
solves the stated problem. Rules: (1) IDisposable must use `using`, (2) factory
methods return null on failure, (3) Subset()/ToRasterImage() may return same
instance — check before disposing source, (4) Canvas/Paint/Path not thread-safe.

Code: {extracted_code}
Problem: {issue_summary} — Hypothesis: {hypothesis}

Return JSON: { "verdict": "pass|fail|warn", "issues": [{ "line_hint", "problem",
"severity": "error|warning", "fix" }], "solves_problem": bool,
"solves_problem_reason": "...", "confidence": 0.0-1.0 }
```

### Agent 3: Platform Safety

**Checks:** Platform-specific API compatibility, correct NativeAssets package implied, GPU/CPU concerns, WASM limitations, container constraints, cross-platform file paths.

**Prompt template:**
```
Check this SkiaSharp workaround works on the reporter's platform(s). Key rules:
Linux=fontconfig vs NoDependencies, Alpine=linux-musl RID, WASM=no P/Invoke,
Android CI=no GPU, Mac Catalyst≠macOS. Check path separators and font availability.
Consult: references/skia-patterns.md, documentation/packages.md.

Code: {extracted_code}
Platforms: {platforms_json}
Environment: {environment_details}

Return JSON: { "verdict": "pass|fail|warn", "issues": [{ "platform", "problem",
"severity": "error|warning", "fix" }], "platforms_checked": [...],
"confidence": 0.0-1.0 }
```

## Input Extraction

Before spawning agents, extract from the triage JSON: (1) all fenced code blocks from `resolution.proposals[].description` and `add-comment` `draftBody`, concatenated with `---` separators; (2) issue metadata — title, type, summary, hypothesis, platforms, environment clues. If no code blocks found, skip validation entirely.

## Agent Invocation

After Phase 3 generates the triage JSON:

1. Extract code blocks from proposals + draftBody
2. No code → skip to Phase 4
3. Code found → launch 3 parallel `task(agent_type="explore")` calls
4. Collect 3 JSON results → synthesize → apply fixes → continue to Phase 4

## Decision Matrix

| Agent 1 (API) | Agent 2 (Behavior) | Agent 3 (Platform) | Decision |
|---------------|-------------------|-------------------|----------|
| pass | pass | pass | **Accept** as-is |
| pass | pass | warn | **Accept + platform caveat** |
| pass | warn | pass | **Accept + disposal/null reminder** |
| fail | any | any | **Fix or reject** — API doesn't exist |
| any | fail | any | **Fix or reject** — memory bug or wrong fix |
| any | any | fail | **Fix or reject** — platform incompatible |
| warn | warn | warn | **Downgrade** proposal confidence by 0.15 |

## Synthesis Logic

```
errors = all severity=="error" from 3 agents
warnings = all severity=="warning" from 3 agents

if any verdict == "fail":
    if errors have "fix" suggestions → auto-apply, re-run failing agent(s) (max 1 retry)
    else → strip code from draftBody, confidence=0.40, requiresHumanReview=true

elif warnings:
    append as caveats to draftBody
    reduce confidence by 0.05/warning (floor 0.50)
```

## Edge Cases

- **Multiple code blocks:** Concatenate with `---` separators; all agents validate the full set
- **Inline-only code** (no fenced blocks): Trigger Agent 1 only if matches `SK[A-Z]\w+\.\w+\(` — skip Agents 2/3 (insufficient context)
- **Malformed agent JSON:** Lenient parse (first `{` to last `}`); if unparseable → `verdict: "warn", confidence: 0.5`
- **Agent timeout** (~30s): Treat as `verdict: "warn", confidence: 0.5` — never block triage
- **Non-SkiaSharp code** (MAUI, ASP.NET): Out of scope — warn if obviously wrong, don't fail
- **Retry budget:** Max 1 retry per failing agent, only if it provided a `fix`; second failure → strip code

## Integration Point in SKILL.md

Insert as **Phase 3.5** between Analyze and Validate:

```markdown
### Phase 3.5 — Workaround Validation (conditional)

If triage JSON contains code in proposals or draftBody:
1. Extract code blocks (fenced or inline SK* calls)
2. Launch 3 parallel validation agents per references/workaround-validation.md
3. Synthesize results — apply fixes, caveats, or downgrades
If no code → proceed to Phase 4.
```

## Example

**Issue:** "SKBitmap.Decode crashes with large images" — proposed fenced code block scales image before decode.

**Agent results:**
- Agent 1 (API): `pass` — all 5 APIs exist with correct signatures
- Agent 2 (Behavior): `warn` — SKBitmap not in `using`, SKCodec.Create not null-checked
- Agent 3 (Platform): `pass`

**Synthesis:** Accept with caveats. Append to draftBody: *"Wrap bitmap in `using` and null-check `SKCodec.Create`."* Confidence: 0.90 → 0.80 (−0.05 × 2 warnings).
