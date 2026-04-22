---
description: "Triage a SkiaSharp issue: classify, label, and update the backlog project board."
on:
  schedule: every 10m
  workflow_dispatch:
    inputs:
      issue_number:
        description: "Issue number to triage (leave blank for auto-select)"
        required: false
        type: string
  skip-if-no-match: "is:issue is:open -label:triage/triaged"
  skip-bots: [github-actions, copilot, dependabot]
  roles: all
  permissions:
    issues: read
  steps:
    - name: Select issue to triage
      id: find-issue
      env:
        INPUT_ISSUE: ${{ github.event.inputs.issue_number }}
        GH_TOKEN: ${{ github.token }}
      run: |
        if [ -n "$INPUT_ISSUE" ]; then
          echo "issue_number=$INPUT_ISSUE" >> "$GITHUB_OUTPUT"
          exit 0
        fi
        CUTOFF=$(date -u -d '1 hour ago' +%Y-%m-%dT%H:%M:%SZ)
        ISSUE=$(gh issue list --repo "$GITHUB_REPOSITORY" \
          --state open \
          --search "-label:triage/triaged created:<${CUTOFF} sort:comments-desc" \
          --json number --limit 1 --jq '.[0].number')
        if [ -z "$ISSUE" ] || [ "$ISSUE" = "null" ]; then
          echo "::notice::No untriaged issues older than 1 hour"
          exit 1
        fi
        echo "issue_number=$ISSUE" >> "$GITHUB_OUTPUT"
jobs:
  pre-activation:
    outputs:
      issue_number: ${{ steps.find-issue.outputs.issue_number }}
if: needs.pre_activation.outputs.find-issue_result == 'success'
steps:
  - name: Redirect step summary into agent-writable directory
    run: |
      mkdir -p /tmp/gh-aw/agent
      touch /tmp/gh-aw/agent/step-summary.md
      rm -f /tmp/gh-aw/agent-step-summary.md
      ln -s /tmp/gh-aw/agent/step-summary.md /tmp/gh-aw/agent-step-summary.md
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
    max: 10
    target: "*"
  update-project:
    project: "https://github.com/orgs/mono/projects/1"
    max: 1
    github-token: ${{ secrets.GH_AW_WRITE_PROJECT_TOKEN }}
---

# Auto-Triage SkiaSharp Issue

Triage issue **#${{ needs.pre_activation.outputs.issue_number }}** using the issue-triage skill, then apply labels and update the SkiaSharp Backlog project board.

## Step 1 — Run the issue-triage skill

Read and follow the instructions in `.agents/skills/issue-triage/SKILL.md` to triage issue #${{ needs.pre_activation.outputs.issue_number }}.

Complete all phases (Setup → Investigate → Analyze → Workarounds → Validate → Persist).

After Phase 6 completes, the triage JSON will be at `issue-triage-workspace/${{ needs.pre_activation.outputs.issue_number }}.json`.

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

Use `content_type: "issue"` and issue number `${{ needs.pre_activation.outputs.issue_number }}` to identify the item.

Only include fields that have non-null values in the triage JSON. Omit any field where the source value is null or absent.

## Step 4 — Upload triage reports and write step summary

Copy the three triage output files into `/tmp/gh-aw/agent/` so they are included in the workflow's `agent` artifact, then write the Markdown report to the step summary so it's visible on the Actions run page without downloading artifacts.

```bash
cp output/ai/repos/mono-SkiaSharp/ai-triage/${{ needs.pre_activation.outputs.issue_number }}.json /tmp/gh-aw/agent/
cp output/ai/repos/mono-SkiaSharp/ai-triage/${{ needs.pre_activation.outputs.issue_number }}.md /tmp/gh-aw/agent/
cp output/ai/repos/mono-SkiaSharp/ai-triage/${{ needs.pre_activation.outputs.issue_number }}.html /tmp/gh-aw/agent/
cat output/ai/repos/mono-SkiaSharp/ai-triage/${{ needs.pre_activation.outputs.issue_number }}.md >> /tmp/gh-aw/agent/step-summary.md
```

**IMPORTANT:** Write to the literal path `/tmp/gh-aw/agent/step-summary.md` — this file is symlinked to the step summary. Do NOT use `$GITHUB_STEP_SUMMARY` as it resolves to an inaccessible path.

All three files MUST be copied and the markdown MUST be appended to `/tmp/gh-aw/agent/step-summary.md`. Verify they exist before finishing.
