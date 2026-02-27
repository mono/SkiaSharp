# Anti-Patterns (Full Reference)

The critical rules are in SKILL.md. This file has the complete list with examples.

## Step Result Decision Table

| You Run | What Happens | Step Result | Why |
|---------|--------------|-------------|-----|
| `dotnet build` | Build succeeds, exits 0 | `success` | Command succeeded |
| `dotnet build` | Build fails with CS0117 | `failure` | Command failed |
| `dotnet build` (reporter said it fails) | Build fails with CS0117 | **`failure`** | Command still failed — matching the report doesn't change the technical outcome |
| Render image | Exits 0 but pixels wrong | `wrong-output` | Process succeeded, output incorrect |

## Full Anti-Pattern List

1. **Inline binary content.** Use `artifacts` array with URLs, not inline data.
2. **Unlimited output.** Truncate: 2KB success, 4KB failure, 50 lines stack trace.
3. **Source code investigation.** Stop at "did it reproduce." Fixes are `issue-fix`'s job.
4. **Prompting the user.** Auto-proceed with logged warning. Skill must be non-interactive.
5. **Absolute paths in output.** Redact `/Users/{name}/` → `$HOME/`.
6. **Giving up too early.** Try multiple approaches, versions, platforms before `not-reproduced`.
7. **Building from source first.** Start with NuGet packages. Source is Phase 3C only.
8. **Editorial judgment in conclusion.** `reproduced` = reported behavior occurred, even if by-design.
9. **Mismarking step results.** `result` = technical outcome, not expectation match.
10. **Pre-emptive version assumptions.** Inspect the nupkg before claiming incompatibility.
11. **Abandoning on environment issues.** Missing workloads, sudo prompts are fixable — not blockers.
12. **Skipping Docker for Linux bugs.** Try Docker before concluding `needs-platform`.
13. **Assuming TFM incompatibility.** .NET is forward-compatible. `net8.0` works on `net10.0`.
14. **Stopping at build success for WASM.** Serve the app, check browser console with Playwright.
15. **Retrying without changing variables.** Change platform, version, or approach — not retry count.
16. **Setup failures ≠ "not reproduced."** Docker timeout, missing Playwright = blocker, not conclusion.
17. **Testing WASM for every bug.** Only when issue signals suggest browser/web.
18. **Silently skipping cross-platform.** Record why in `notes`, set `scope` to `"unknown"`.
19. **Reusing build artifacts across versions.** Fresh project dirs or `rm -rf bin/ obj/` between versions.

## Output Limits

See SKILL.md for the authoritative output limits table.

| Field | Max Size |
|-------|----------|
| `reproductionSteps[].output` (success) | 2KB |
| `reproductionSteps[].output` (failure) | 4KB |
| `errorMessages.stackTrace` | 5KB / 50 lines |
| File content | Inline for small source files; omit binaries |

**Redaction:** `/Users/{name}/` → `$HOME/`, tokens → `[REDACTED]`

## Additional Anti-Patterns

20. **Never use `sudo`.** If a command requires `sudo`, find an alternative. `sudo` prompts for a password interactively, which blocks the session indefinitely. Use user-local installs, Docker, or different approaches.

21. **No repo markdown artifacts.** NEVER create markdown summary files (e.g., `REPRO_SUMMARY.md`, `COMPLETION_REPORT.md`) in the repository working tree. All working files belong in the session workspace (`~/.copilot/session-state/`). Clean up any accidentally created files before persisting.

22. **Fabricating output or evidence.** NEVER use echo/print statements to simulate command output, claim "reproduced" from static code analysis without executing the code, or report environment details (OS version, SDK version) without actually running the diagnostic commands. If you cannot run it, report `needs-platform`.

23. **Modifying product source during repro.** Reproduction ONLY creates new test projects in `/tmp/skiasharp/repro/`. NEVER edit `binding/`, `externals/`, `samples/`, `source/`, `tests/`, `utils/`, or any other product source — that is the `issue-fix` skill's job. Revert immediately if done accidentally.

24. **Skipping validation.** NEVER skip the validation script (`validate-repro.ps1` / `validate-repro.py`). NEVER assume the JSON is valid without running it. NEVER persist to the data cache without seeing ✅ from the validator. Mentally reviewing the JSON is not a substitute for the script.
