# Triage Anti-Patterns

❌ **NEVER do these during triage.** Triage is READ-ONLY analysis.

---

### CRITICAL: Triage is READ-ONLY

**If you edit a source file during triage, you have FAILED.**

Do NOT create, edit, or run any of these: `.cs`, `.csproj`, `.cpp`, `.h`, `.json` (source),
`.sln`, `.targets`, `.props`. Do NOT run `dotnet build`, `dotnet test`, `dotnet cake`, or
execute any reporter code.

If reproduction is needed, set `suggestedAction: "needs-investigation"` and stop — that is
the `issue-repro` skill's job.

---

### 1. Pre-baked delegation

NEVER write your classification in a sub-agent prompt. Sub-agents investigate and return
evidence. YOU classify based on their evidence.

### 2. Age-based closure

NEVER close an issue because it's old. Old issues with no code fix are STILL OPEN
BUGS/REQUESTS.

### 3. Platform-deprecated ≠ stale

NEVER assume a Xamarin.Forms issue is stale. Check if the same code/gap exists in MAUI
before suggesting closure.

### 4. Assertion without citation

NEVER write "the code does X" without a `{file, lines}` entry in `codeInvestigation`.
No file:line = no claim.

### 5. Batch shortcuts

When triaging multiple issues, each gets FULL investigation. Parallel investigation is fine;
parallel conclusions are not.

### 6. .NET Forward Compatibility

NEVER conclude "doesn't support .NET X" when the library targets a lower TFM. .NET is
forward-compatible by design — a `net8.0` library works on `net10.0` apps via NuGet TFM
fallback. NEVER suggest "downgrade to .NET 8" as a workaround.

Exception: platform-specific TFMs (e.g., `net8.0-ios`) where platform-specific native assets
are required.

### 7. No repo artifacts

NEVER create markdown summary files, plans, or reports in the repository working tree.
All working files belong in the session workspace (`~/.copilot/session-state/`).

### 8. Fabrication

NEVER invent code investigation findings, claim "the code shows X" without reading the actual
file, or fabricate file paths / line numbers. If you cannot find the code, say so.

### 9. Skipping validation

NEVER skip the validation script (`validate-triage.ps1` / `validate-triage.py`). NEVER assume
the JSON is valid without running it. NEVER persist to the data cache without seeing ✅ from the
validator. Mentally reviewing the JSON is not a substitute for the script.
