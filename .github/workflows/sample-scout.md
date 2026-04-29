---
description: "Run the sample-scout skill to discover Skia GM samples worth porting to the SkiaSharp Gallery."
on:
  schedule: weekly
  workflow_dispatch:
  skip-bots: [github-actions, copilot, dependabot]
concurrency:
  group: sample-scout
  cancel-in-progress: true
timeout-minutes: 30
checkout:
  submodules: recursive
  fetch-depth: 1
steps:
  - name: Prepare working directory
    run: |
      mkdir -p /tmp/gh-aw/agent
      touch /tmp/gh-aw/agent/step-summary.md
      rm -f /tmp/gh-aw/agent-step-summary.md
      ln -s /tmp/gh-aw/agent/step-summary.md /tmp/gh-aw/agent-step-summary.md
permissions:
  contents: read
  issues: read
tools:
  bash: ["python3", "cat", "grep", "find", "jq", "head", "tail", "wc", "sort", "sed", "ls", "cp", "mkdir", "echo", "xargs", "basename"]
network:
  allowed:
    - defaults
    - python
safe-outputs:
  mentions: false
  allowed-github-references: []
  max-bot-mentions: 1
  create-issue:
    title-prefix: "Sample Scout:"
    labels: [report, area/Gallery]
    close-older-issues: true
    expires: 30
---

# Weekly Sample Scout Report

Scan Skia GM samples from the submodule and publish a report of Gallery opportunities
as a GitHub issue using the `create_issue` safe output tool.

## Step 1 — Run the sample-scout skill

Read `.agents/skills/sample-scout/SKILL.md` and follow its instructions for a full scan.

The GM files are at `externals/skia/gm/*.cpp` and binding code is in `binding/SkiaSharp/` —
read them directly from the checkout, no remote fetching needed.

## Step 2 — Condense and publish as GitHub issue

The rendered report is typically ~90KB which exceeds GitHub's issue body limit. Before
publishing, condense it to under 60000 characters:

```bash
python3 -c "
import json, datetime
with open('sample-scout-report.json') as f:
    report = json.load(f)
findings = report.get('findings', [])
high = [f for f in findings if f.get('interesting') == 'high' and f.get('apis_available') and f.get('sampleStatus') == 'none']
med = [f for f in findings if f.get('interesting') == 'medium' and f.get('apis_available') and f.get('sampleStatus') == 'none']
total = len(findings)
ready = len(high)
lines = [
    '### Summary',
    f'- **{total}** GM samples analyzed',
    f'- **{ready}** high-interest opportunities (APIs ready, no existing sample)',
    f'- **{len(med)}** medium-interest opportunities',
    f'- Date: {datetime.date.today()}',
    '',
    '### Top Opportunities',
    '',
    '| GM File | Description | Key APIs |',
    '|---------|-------------|----------|',
]
for f in high[:30]:
    apis = ', '.join((f.get('skiaSharpApis') or f.get('key_apis') or [])[:3])
    desc = (f.get('description') or '')[:80]
    lines.append(f'| {f[\"file\"]} | {desc} | {apis} |')
if med:
    lines += ['', '### Medium Interest', '', '| GM File | Description |', '|---------|-------------|']
    for f in med[:20]:
        desc = (f.get('description') or '')[:80]
        lines.append(f'| {f[\"file\"]} | {desc} |')
with open('/tmp/gh-aw/agent/issue-body.md', 'w') as out:
    out.write('\n'.join(lines))
print(f'Condensed: {len(lines)} lines')
"
```

**Now immediately use the `create_issue` safe output tool.** Read `/tmp/gh-aw/agent/issue-body.md`
and pass it as the issue body. Do this right away — do not re-read or review the content first.

The issue title **must** start with `Sample Scout:` followed by date and key metric
(e.g. `Sample Scout: 2025-01-15 (42 opportunities)`).

## Step 3 — Upload artifacts

```bash
cp sample-scout-report.json /tmp/gh-aw/agent/
cp sample-scout-report.md /tmp/gh-aw/agent/
cat sample-scout-report.md >> /tmp/gh-aw/agent/step-summary.md
```

Write to `/tmp/gh-aw/agent/step-summary.md` (symlinked to step summary).

## ⚠️ MANDATORY — Safe Output Required

**You MUST call the `create_issue` safe output tool before finishing.** If the full report
could not be generated, still call `create_issue` with a partial summary. Never exit without
calling at least one safe output tool.
