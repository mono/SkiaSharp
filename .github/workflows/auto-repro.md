---
description: "Reproduce a triaged SkiaSharp issue on Linux and apply result labels."
on:
  schedule: every 10m
  workflow_dispatch:
    inputs:
      issue_number:
        description: "Issue number to reproduce (leave blank for auto-select)"
        required: false
        type: string
  skip-if-no-match: "is:issue is:open label:triage/triaged"
  skip-bots: [github-actions, copilot, dependabot]
  roles: all
  permissions:
    issues: read
  steps:
    - name: Select issue to reproduce
      id: find-issue
      env:
        INPUT_ISSUE: ${{ github.event.inputs.issue_number }}
        GH_TOKEN: ${{ github.token }}
        GH_AW_WRITE_PROJECT_TOKEN: ${{ secrets.GH_AW_WRITE_PROJECT_TOKEN }}
      run: |
        if [ -n "$INPUT_ISSUE" ]; then
          echo "issue_number=$INPUT_ISSUE" >> "$GITHUB_OUTPUT"
          exit 0
        fi

        # Find the project ID for mono/projects/1
        PROJECT_ID=$(gh api graphql -f query='
          query {
            organization(login: "mono") {
              projectV2(number: 1) { id }
            }
          }' --jq '.data.organization.projectV2.id' \
          --header "Authorization: token $GH_AW_WRITE_PROJECT_TOKEN")

        if [ -z "$PROJECT_ID" ] || [ "$PROJECT_ID" = "null" ]; then
          echo "::error::Could not find SkiaSharp Backlog project"
          exit 1
        fi

        # Find the "Suggested Repro Platform" field ID and "linux" option ID
        FIELD_INFO=$(gh api graphql -f query="
          query {
            node(id: \"$PROJECT_ID\") {
              ... on ProjectV2 {
                field(name: \"Suggested Repro Platform\") {
                  ... on ProjectV2SingleSelectField {
                    id
                    options { id name }
                  }
                }
              }
            }
          }" --jq '{
            field_id: .data.node.field.id,
            linux_id: (.data.node.field.options[] | select(.name == "linux") | .id)
          }' \
          --header "Authorization: token $GH_AW_WRITE_PROJECT_TOKEN")

        FIELD_ID=$(echo "$FIELD_INFO" | jq -r '.field_id')
        LINUX_ID=$(echo "$FIELD_INFO" | jq -r '.linux_id')

        if [ -z "$FIELD_ID" ] || [ "$FIELD_ID" = "null" ] || [ -z "$LINUX_ID" ] || [ "$LINUX_ID" = "null" ]; then
          echo "::notice::Could not find Suggested Repro Platform field or linux option"
          exit 1
        fi

        # Query project items where Suggested Repro Platform = linux
        # and the corresponding issue is open with triage/triaged label
        ITEMS=$(gh api graphql -f query="
          query {
            node(id: \"$PROJECT_ID\") {
              ... on ProjectV2 {
                items(first: 50) {
                  nodes {
                    id
                    fieldValueByName(name: \"Suggested Repro Platform\") {
                      ... on ProjectV2ItemFieldSingleSelectValue { optionId }
                    }
                    fieldValueByName(name: \"Suggested Action\") {
                      ... on ProjectV2ItemFieldSingleSelectValue { name }
                    }
                    content {
                      ... on Issue {
                        number
                        state
                        labels(first: 20) { nodes { name } }
                      }
                    }
                  }
                }
              }
            }
          }" --header "Authorization: token $GH_AW_WRITE_PROJECT_TOKEN")

        # Filter for:
        # 1. Suggested Repro Platform = linux (matching option ID)
        # 2. Issue is OPEN
        # 3. Has triage/triaged label
        # 4. Does NOT have Suggested Action = close-* (already resolved)
        ISSUE=$(echo "$ITEMS" | jq -r --arg lid "$LINUX_ID" '
          [.data.node.items.nodes[]
           | select(
               .fieldValueByName != null
               and (.fieldValueByName | objects | .optionId) == $lid
               and .content.state == "OPEN"
               and ([.content.labels.nodes[].name] | index("triage/triaged") != null)
             )
           | .content.number
          ] | first // empty')

        if [ -z "$ISSUE" ] || [ "$ISSUE" = "null" ]; then
          echo "::notice::No triaged Linux issues ready for reproduction"
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
  bash: ["gh", "python3", "pip3", "dotnet", "docker", "jq"]
permissions:
  contents: read
  issues: read
network:
  allowed:
    - defaults
    - python
    - "*.nuget.org"
    - "mcr.microsoft.com"
safe-outputs:
  add-labels:
    max: 5
    target: "*"
---

# Auto-Reproduce SkiaSharp Issue

Reproduce issue **#${{ needs.pre_activation.outputs.issue_number }}** using the issue-repro skill on Linux, then apply result labels.

## Step 1 — Run the issue-repro skill

Read and follow the instructions in `.agents/skills/issue-repro/SKILL.md` to reproduce issue #${{ needs.pre_activation.outputs.issue_number }}.

Complete all phases (Setup → Fetch → Assess → Reproduce → JSON + Output → Validate → Persist).

This is a **Linux environment** — use the Docker Linux or Console platform strategy. Do NOT attempt macOS or Windows-specific reproduction.

After Phase 6 completes, the repro JSON will be at `output/ai/repos/mono-SkiaSharp/ai-repro/${{ needs.pre_activation.outputs.issue_number }}.json`.

## Step 2 — Apply result label

Read the repro JSON and apply exactly ONE label based on the conclusion:

| Conclusion | Label |
|-----------|-------|
| `reproduced` | `triage/reproduced` |
| `confirmed` | `triage/reproduced` |
| `not-reproduced` | `triage/not-reproduced` |
| `not-confirmed` | `triage/not-reproduced` |
| `needs-platform` | `triage/error-during-repro` |
| `needs-hardware` | `triage/error-during-repro` |
| `partial` | `triage/error-during-repro` |
| `inconclusive` | `triage/error-during-repro` |

Before adding the new label, remove any existing `triage/reproduced`, `triage/not-reproduced`, or `triage/error-during-repro` labels from the issue (since repro always re-runs, we replace the old result).

## Step 3 — Upload repro reports and write step summary

Copy the three repro output files into `/tmp/gh-aw/agent/` so they are included in the workflow's `agent` artifact, then write the Markdown report to the step summary so it's visible on the Actions run page without downloading artifacts.

```bash
cp output/ai/repos/mono-SkiaSharp/ai-repro/${{ needs.pre_activation.outputs.issue_number }}.json /tmp/gh-aw/agent/
cp output/ai/repos/mono-SkiaSharp/ai-repro/${{ needs.pre_activation.outputs.issue_number }}.md /tmp/gh-aw/agent/
cp output/ai/repos/mono-SkiaSharp/ai-repro/${{ needs.pre_activation.outputs.issue_number }}.html /tmp/gh-aw/agent/
cat output/ai/repos/mono-SkiaSharp/ai-repro/${{ needs.pre_activation.outputs.issue_number }}.md >> /tmp/gh-aw/agent/step-summary.md
```

**IMPORTANT:** Write to the literal path `/tmp/gh-aw/agent/step-summary.md` — this file is symlinked to the step summary. Do NOT use `$GITHUB_STEP_SUMMARY` as it resolves to an inaccessible path.

All three files MUST be copied and the markdown MUST be appended to `/tmp/gh-aw/agent/step-summary.md`. Verify they exist before finishing.
