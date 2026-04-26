---
name: issue-bulk-process
description: >-
  Bulk triage and reproduce multiple SkiaSharp GitHub issues in a single command.
  Orchestrates parallel triage agents then serial reproduction agents, and presents
  a consolidated summary. Triggers: "bulk process issues", "triage and repro these
  issues", "process issues 3400 3428 3429", "process the newest 10 issues",
  "bulk triage", "batch process issues", any request to triage or reproduce
  multiple issues at once.
---

# Issue Bulk Process

Orchestrate bulk triage and reproduction of multiple SkiaSharp issues by delegating to
`issue-triage` and `issue-repro` skills via sub-agents.

## Input Parsing

Parse the user's message to determine the issue list:

1. **Explicit issue references** — numbers preceded by `#`, or numbers in a space/comma-separated list that the user
   clearly intends as issue numbers (e.g., `#3400 #3428`, `issues 3400, 3428, 3429`, `triage 123 456 789`).
   Issue numbers may be 2–5 digits.
2. **"newest N issues"** — if the user says "newest", "latest", "recent", or "last N issues", extract the count N
   and fetch from GitHub (see Phase 1).
3. **Ambiguous numbers** — if a number could be an issue number OR a count/quantity (e.g., "process 10 issues starting
   from 3400"), ask the user to clarify before proceeding. Use the `ask_user` tool.

## Phase 1 — Resolve Repository and Issue List

### 1. Detect current repository

```bash
gh repo view --json owner,name -q '"\(.owner.login)/\(.name)"'
```

Store the `{owner}` and `{repo}` for use in all subsequent phases.

### 2. Resolve issue list

Use the `gh` CLI for all issue lookups. It handles pagination automatically via the `-L` (limit) flag —
no manual page iteration needed.

**If explicit numbers given:** Use them directly — they refer to the current repository.

**If "newest N issues" or similar:**

```bash
gh issue list --state open -L {N} --json number,title -q '.[] | "\(.number)\t\(.title)"'
```

**If search query** (e.g., "all blazor issues", "issues about WASM", "label:area/foo"):

```bash
gh issue list -S "{search terms}" --state open -L 500 --json number,title -q '.[] | "\(.number)\t\(.title)"'
```

For label-based queries, use `--label` instead of `-S`:

```bash
gh issue list --label "area/SkiaSharp.Views" --state open -L 500 --json number,title -q '.[] | "\(.number)\t\(.title)"'
```

> **Note:** `-L 500` is a generous upper bound. `gh` fetches exactly as many pages as needed and
> stops when results are exhausted, so over-specifying the limit is fine.

Extract issue numbers from the output. Present the list to the user and confirm before proceeding.

### 3. Skip already-processed issues

Before launching triage/repro agents, check for existing result files:

```bash
# Check for existing triage results
ls .data-cache/repos/{owner}-{repo}/ai-triage/{number}.json 2>/dev/null

# Check for existing repro results
ls .data-cache/repos/{owner}-{repo}/ai-repro/{number}.json 2>/dev/null
```

- **Default behavior:** Skip issues that already have a triage JSON file. Skip reproduction for
  issues that already have a repro JSON file. Report skipped issues in the summary with a ⏩ icon.
- **Force re-processing:** If the user explicitly says "force", "re-triage", "re-process",
  "redo", or "again", process all issues regardless of existing results.
- **Present skip info:** Before proceeding, show the user how many will be skipped vs processed:
  ```
  Found 33 issues. 15 already triaged, 12 already reproduced.
  Will triage: 18 | Will reproduce: 21
  ```

## Phase 2 — Triage (parallel)

> Triage is read-only — safe to parallelize.

Launch one `general-purpose` **background** agent per issue. Each agent's prompt is exactly:

```
triage issue {number}
```

This triggers the `issue-triage` skill in each agent.

**Steps:**
1. Launch all agents with `mode: "background"` via the `task` tool. Collect all returned `agent_id` values.
2. Call `read_agent` for **each** agent_id with `wait: true, timeout: 300`. You can call multiple `read_agent` in parallel.
3. If any agent is still running after 300s, call `read_agent` again with another 300s timeout (600s total max).
4. From each agent's response, extract the **Type**, **Severity**, and **suggested Action** from the triage summary.
   Look for patterns like `Type: type/bug (0.95)`, `Severity: high`, `Action: needs-investigation` in the response text.
5. If an agent's response does not mention `issue-triage` or `ai-triage`, it failed to invoke the skill — report it.

**Batch size limit:** Launch at most **12 agents** at once. If more issues, wait for the current batch to complete before launching the next.

## Phase 3 — Reproduce (serial)

> Reproduction may build projects, create files, open simulators — must run in series.

Launch one `general-purpose` **sync** agent per issue, sequentially. Each agent's prompt is exactly:

```
reproduce issue #{number}
```

This triggers the `issue-repro` skill in each agent.

**Steps:**
1. For each issue in order, launch a `task` agent with `mode: "sync"`. The call blocks until the agent completes.
2. From the agent's response, extract the **Conclusion** (one of: `reproduced`, `not-reproduced`, `needs-platform`,
   `needs-hardware`, `inconclusive`, `partial`). Look for `Conclusion:` in the response text.
3. Continue to the next issue regardless of the previous result.

## Phase 4 — Summary

Present a consolidated summary table using data extracted from the agent responses in Phases 2 and 3:

```
## Bulk Process Results

| # | Issue | Type | Severity | Triage Action | Repro Conclusion |
|---|-------|------|----------|---------------|------------------|
| 1 | #3400 | Bug | medium | needs-investigation | ⏭️ needs-platform |
| 2 | #3472 | Bug | medium | needs-investigation | ❌ reproduced |
...

### Stats
- Triaged: {N}
- Reproduced: {reproduced}
- Not reproduced: {not_reproduced}
- Blocked (needs-platform/hardware): {blocked}
- Inconclusive: {inconclusive}
```

**Conclusion icons:** ❌ reproduced · ✅ not-reproduced · ⏭️ needs-platform/hardware · ❓ inconclusive · ⏱️ timeout · ⏩ skipped (already processed)

## Error Handling

| Situation | Action |
|-----------|--------|
| Agent times out after 600s total | Record as `⏱️ timeout` in the summary and continue |
| Agent response doesn't mention the skill | Record as `failed` in the summary and continue |
| GitHub API fails for "newest N" | Ask user to provide explicit issue numbers |
