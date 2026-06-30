# CI Status Report Schema

JSON schema for the CI status report. The AI generates this JSON by combining raw collector data
with analysis, then render scripts produce HTML and Markdown reports.

## Top-Level Structure

```json
{
  "meta": { ... },
  "verdict": { ... },
  "azdoHealth": { ... },
  "chainAnalysis": [ ... ],
  "rootCauses": [ ... ],
  "githubActions": { ... },
  "flakes": [ ... ],
  "releaseRisk": [ ... ],
  "recommendations": [ ... ]
}
```

## `meta` — Report Metadata

| Field | Type | Required | Description |
|-------|------|----------|-------------|
| `date` | string | Yes | ISO date of the report (YYYY-MM-DD) |
| `timestamp` | string | Yes | Full ISO timestamp |
| `schemaVersion` | string | Yes | Always `"1.0"` |
| `window` | object | Yes | Collection window parameters |
| `window.buildsPerPipeline` | integer | Yes | Number of builds per pipeline checked |
| `window.branchesCount` | integer | Yes | Number of branches checked |
| `branches` | array[string] | Yes | Branch names that were checked |

## `verdict` — Executive Summary

| Field | Type | Required | Description |
|-------|------|----------|-------------|
| `status` | string | Yes | One of: `"healthy"`, `"degraded"`, `"broken"` |
| `emoji` | string | Yes | `"🟢"`, `"🟡"`, or `"🔴"` |
| `summary` | string | Yes | 1-2 sentence summary of overall health |

Classification rules:
- 🟢 **healthy** — all branches green or only warnings
- 🟡 **degraded** — some failures but main is green
- 🔴 **broken** — main is red or a release branch is blocked

## `azdoHealth` — Azure DevOps Pipeline Health

```json
{
  "branches": [
    {
      "name": "main",
      "risk": "low",
      "pipelines": [
        {
          "name": "SkiaSharp (Public)",
          "definitionId": 4,
          "org": "xamarin/public",
          "latestResult": "succeeded",
          "latestIcon": "✅",
          "passRate": 100.0,
          "runs": [
            {
              "id": 157985,
              "buildNumber": "6.0.0-preview.3.24305.1",
              "result": "succeeded",
              "status": "completed",
              "icon": "✅",
              "startTime": "2026-05-28T10:00:00Z",
              "finishTime": "2026-05-28T11:30:00Z",
              "durationMinutes": 90,
              "sourceVersion": "abc1234",
              "url": "https://dev.azure.com/..."
            }
          ],
          "issues": [ ... ],
          "changes": [ ... ],
          "regression": null
        }
      ]
    }
  ],
  "regressions": [
    {
      "branch": "main",
      "pipeline": "SkiaSharp (Public)",
      "lastGreenId": 157980,
      "lastGreenBuildNumber": "6.0.0-preview.3.24300.1",
      "firstRedId": 157985,
      "firstRedBuildNumber": "6.0.0-preview.3.24305.1",
      "firstRedUrl": "https://...",
      "suspects": [ { "id": "abc1234", "author": "dev@example.com", "message": "..." } ]
    }
  ]
}
```

### `azdoHealth.branches[].risk`

| Value | Meaning |
|-------|---------|
| `"low"` | All pipelines green |
| `"medium"` | Some warnings/partial success, no failures |
| `"high"` | At least one pipeline failed |

## `chainAnalysis` — Pipeline Chain Verdicts

Array of chain analysis verdicts. One entry per branch with ≥1 red internal pipeline.

```json
{
  "branch": "release/3.119.x",
  "verdict": "cascade",
  "summary": "Tests red independently (Guardian TSA); Native/Managed warnings only — no cascade.",
  "rootPipeline": "SkiaSharp-Tests",
  "cascadedPipelines": []
}
```

| Field | Type | Required | Description |
|-------|------|----------|-------------|
| `branch` | string | Yes | Branch name |
| `verdict` | string | Yes | One of: `"cascade"`, `"independent"`, `"mixed"` |
| `summary` | string | Yes | One-sentence explanation |
| `rootPipeline` | string | Yes | Earliest-in-chain failing pipeline |
| `cascadedPipelines` | array[string] | Yes | Pipelines that failed due to cascade (not independent) |

## `rootCauses` — Clustered Error Analysis

```json
{
  "id": "A",
  "title": "SKRuntimeEffectTest hang on Windows .NET Framework",
  "category": "code_regression",
  "severity": "high",
  "footprint": {
    "branches": ["release/3.119.x"],
    "pipelines": ["SkiaSharp (Public)", "SkiaSharp-Tests"]
  },
  "firstSeen": "2026-05-25T10:00:00Z",
  "lastSeen": "2026-05-29T10:00:00Z",
  "sampleError": "Process exited with code -1 (hang timeout)...",
  "buildEvidence": [
    { "id": 157820, "url": "https://..." }
  ]
}
```

| Field | Type | Required | Description |
|-------|------|----------|-------------|
| `id` | string | Yes | Short ID (A, B, C...) for cross-referencing |
| `title` | string | Yes | Human-readable error title |
| `category` | string | Yes | One of: `"code_regression"`, `"flake"`, `"infra_network"`, `"quota_resource"`, `"chain_blockage"`, `"unknown"` |
| `severity` | string | Yes | One of: `"critical"`, `"high"`, `"medium"`, `"low"` |
| `footprint.branches` | array[string] | Yes | Affected branches |
| `footprint.pipelines` | array[string] | Yes | Affected pipelines/workflows |
| `firstSeen` | string | Yes | ISO timestamp of earliest occurrence in window |
| `lastSeen` | string | Yes | ISO timestamp of latest occurrence in window |
| `sampleError` | string | Yes | Verbatim error text (truncated at 500 chars) |
| `buildEvidence` | array[object] | Yes | Build/run IDs + URLs as evidence |

## `githubActions` — GitHub Actions Health

```json
{
  "workflows": [
    {
      "name": "Pages - Deploy",
      "repo": "mono/SkiaSharp",
      "trigger": "push",
      "scope": "branch",
      "severity": "high",
      "status": "healthy",
      "branches": [
        {
          "name": "main",
          "latestResult": "success",
          "latestIcon": "✅",
          "passRate": 100.0,
          "runs": [ ... ]
        }
      ],
      "failedJobs": []
    }
  ],
  "summary": {
    "total": 18,
    "healthy": 15,
    "failing": 2,
    "stale": 1,
    "highSeverityFailing": 0,
    "mediumSeverityFailing": 1,
    "lowSeverityFailing": 1
  }
}
```

### `githubActions.workflows[].status`

| Value | Meaning |
|-------|---------|
| `"healthy"` | Latest run(s) all passed |
| `"failing"` | Latest run failed |
| `"degraded"` | Mixed results (some branches pass, some fail) |
| `"stale"` | No runs in last 7 days |
| `"skipped"` | Workflow disabled or no trigger events |

### `githubActions.workflows[].severity`

| Value | Impact |
|-------|--------|
| `"high"` | User-facing impact or release process blocked |
| `"medium"` | Automation degraded, manual workaround exists |
| `"low"` | Cosmetic/housekeeping |

### `githubActions.workflows[].failedJobs`

```json
[
  {
    "name": "build-and-deploy",
    "failedSteps": ["Run build", "Deploy to Azure"],
    "runId": 26651087356,
    "runUrl": "https://github.com/mono/SkiaSharp/actions/runs/..."
  }
]
```

## `flakes` — Flake Detection

```json
{
  "branch": "main",
  "pipeline": "SkiaSharp (Public)",
  "pattern": "✅❌✅❌✅",
  "confidence": "high",
  "description": "Alternating pass/fail on NUnit runner timeout"
}
```

| Field | Type | Required | Description |
|-------|------|----------|-------------|
| `branch` | string | Yes | Branch where flake observed |
| `pipeline` | string | Yes | Pipeline or workflow name |
| `pattern` | string | Yes | Visual pattern of recent results |
| `confidence` | string | Yes | `"high"`, `"medium"`, `"low"` |
| `description` | string | Yes | What appears to be flaking |

## `releaseRisk` — Release Risk Assessment

```json
{
  "branch": "release/3.119.x",
  "shippable": false,
  "daysSinceGreen": 7,
  "blockers": ["A"],
  "recommendation": "investigate"
}
```

| Field | Type | Required | Description |
|-------|------|----------|-------------|
| `branch` | string | Yes | Release branch name |
| `shippable` | boolean | Yes | Can this be released right now? |
| `daysSinceGreen` | integer | No | Days since full green chain (null if never green in window) |
| `blockers` | array[string] | Yes | Root cause IDs blocking this release |
| `recommendation` | string | Yes | One of: `"ship"`, `"wait"`, `"cherry-pick"`, `"investigate"` |

## `recommendations` — Top Actions

```json
{
  "priority": 1,
  "severity": "high",
  "action": "Fix SKRuntimeEffectTest hang on Windows .NET Framework",
  "reason": "Blocks Public CI and Tests on release/3.119.x (5/5 failures)",
  "target": "release/3.119.x",
  "rootCauseId": "A",
  "buildUrl": "https://..."
}
```

| Field | Type | Required | Description |
|-------|------|----------|-------------|
| `priority` | integer | Yes | 1-5, lower is more urgent |
| `severity` | string | Yes | `"critical"`, `"high"`, `"medium"`, `"low"` |
| `action` | string | Yes | Imperative sentence: what to do |
| `reason` | string | Yes | Why this matters |
| `target` | string | Yes | Branch, workflow, or infrastructure affected |
| `rootCauseId` | string | No | Cross-reference to root cause cluster |
| `buildUrl` | string | Yes | Link to relevant build/run |

Maximum 5 recommendations, ordered by priority.
