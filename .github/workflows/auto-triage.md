---
description: "Triage a SkiaSharp issue: classify, label, and update the backlog project board."
on:
  schedule:
    - cron: "0 * * * *"
  workflow_dispatch:
    inputs:
      issue_number:
        description: "Issue number to triage (leave blank for auto-select)"
        required: false
        type: string
  skip-bots: [github-actions, copilot, dependabot]
  roles: all
tools:
  github:
    toolsets: [issues]
    allowed-repos: ["mono/skiasharp", "mono/skia", "google/skia"]
    min-integrity: none
  bash: ["gh", "python3", "pip3", "jq"]
permissions:
  contents: read
  issues: read
network:
  allowed:
    - defaults
    - python
safe-outputs:
  add-labels:
    allowed:
      - "type/*"
      - "area/*"
      - "os/*"
      - "backend/*"
      - "tenet/*"
      - "partner/*"
      - "triage/triaged"
    blocked: ["~*"]
    max: 10
    target: "*"
  update-project:
    project: "https://github.com/orgs/mono/projects/1"
    max: 1
    github-token: ${{ secrets.GH_AW_WRITE_PROJECT_TOKEN }}
---

# Auto-Triage SkiaSharp Issue

Triage a single SkiaSharp issue, apply labels, and update the backlog project board.

## Step 0 — Select which issue to triage

If `${{ github.event.inputs.issue_number }}` is provided and non-empty, use that issue number.

Otherwise (scheduled run), find the **newest** open issue that:
1. Does **not** have the `triage/triaged` label
2. Was created **more than 1 hour ago** (so reporters have time to edit)

Run this command to find it:

```bash
gh issue list --repo mono/SkiaSharp \
  --state open \
  --label "" \
  --search "NOT label:triage/triaged" \
  --sort created --order desc \
  --json number,createdAt \
  --limit 50 \
  | jq --arg cutoff "$(date -u -d '1 hour ago' +%Y-%m-%dT%H:%M:%SZ 2>/dev/null || date -u -v-1H +%Y-%m-%dT%H:%M:%SZ)" \
    '[.[] | select(.createdAt < $cutoff)] | first | .number'
```

If the result is `null` or empty, there are no untriaged issues older than 1 hour — exit successfully with a message "No untriaged issues found" and skip all remaining steps.

Store the selected issue number for the rest of the workflow.

## Step 1 — Run the issue-triage skill

Read and follow the instructions in `.agents/skills/issue-triage/SKILL.md` to triage the selected issue.

Complete all phases (Setup → Investigate → Analyze → Workarounds → Validate → Persist).

After Phase 6 completes, the triage JSON will be at `issue-triage-workspace/<issue_number>.json`.

## Step 2 — Apply labels from classification

Read the triage JSON file and extract the classification labels. Apply each label to the issue:

- `classification.type.value` — the issue type label (e.g. `type/bug`)
- `classification.area.value` — the component area label (e.g. `area/SkiaSharp`)
- Each entry in `classification.platforms[]` — platform labels (e.g. `os/Android`)
- Each entry in `classification.backends[]` — backend labels (e.g. `backend/Metal`)
- Each entry in `classification.tenets[]` — quality tenet labels (e.g. `tenet/compatibility`)
- `classification.partner` — partner label if present (e.g. `partner/maui`)

After applying classification labels, also add the `triage/triaged` label to mark the issue as processed.

Only apply labels that are non-null and non-empty. Skip any classification field that is absent or null.

## Step 3 — Update project board fields

Read the triage JSON and update the issue in the **SkiaSharp Backlog** project (https://github.com/orgs/mono/projects/1) with these custom fields:

| Project Field | JSON Path | Notes |
|--------------|-----------|-------|
| Severity | `evidence.bugSignals.severity` | SINGLE_SELECT: critical, high, medium, low. Skip if null. |
| Suggested Action | `output.actionability.suggestedAction` | SINGLE_SELECT: needs-investigation, close-as-fixed, etc. |
| Confidence | `output.actionability.confidence` | NUMBER: 0.0–1.0. |
| Repro Quality | `evidence.bugSignals.reproQuality` | SINGLE_SELECT: complete, partial, none. Skip if null. |
| Error Type | `evidence.bugSignals.errorType` | SINGLE_SELECT: crash, exception, wrong-output, etc. Skip if null. |
| Triage Summary | `analysis.summary` | TEXT: root cause hypothesis. Truncate to 1024 chars. |

The `update-project` safe output auto-creates missing SINGLE_SELECT options — no need to pre-create values.

Use `content_type: "issue"` and the selected issue number to identify the item.

Only include fields that have non-null values in the triage JSON. Omit any field where the source value is null or absent.
