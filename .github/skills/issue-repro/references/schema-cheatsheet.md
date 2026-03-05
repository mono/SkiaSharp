# Repro Schema Quick Reference

Read this BEFORE generating JSON. Full schema: `references/repro-schema.json`.

## Required Top-Level Fields (always)

| Field | Type | Notes |
|-------|------|-------|
| `meta` | object | `schemaVersion` ("1.0"), `number` (int), `repo` ("mono/SkiaSharp"), `analyzedAt` (ISO 8601) |
| `conclusion` | string | One of the `conclusionValue` enum (see below) |
| `notes` | string | Summary of what happened |
| `reproductionSteps` | array | At least 1 step (see step fields below) |
| `environment` | object | `os`, `arch`, `dotnetVersion`, `skiaSharpVersion`, `dockerUsed` (all required) |

## Conditionally Required by Conclusion

| Conclusion | Additional Required Fields |
|------------|---------------------------|
| `reproduced` | `output`, `versionResults`, `reproProject` |
| `not-reproduced` | `output`, `versionResults` |
| `confirmed` | `output`, `versionResults`, `scope` |
| `not-confirmed` | `output`, `versionResults`, `scope` |
| `needs-platform`, `needs-hardware`, `partial`, `inconclusive` | `blockers` (string array, min 1 item) |

## Step-Result Constraints by Conclusion

| Conclusion | Step Constraint |
|------------|----------------|
| `reproduced` | Must contain ≥1 step with `result: "failure"` or `"wrong-output"` |
| `not-reproduced` | Must contain ≥1 `"success"` step AND zero `"failure"`/`"wrong-output"` steps |
| `confirmed` | No step-result constraint — investigation steps may have any result |
| `not-confirmed` | No step-result constraint — investigation steps may have any result |

⚠️ For `not-reproduced`: if a setup step failed then succeeded on retry, record **only the final successful attempt**. Any `failure`/`wrong-output` step will fail validation.

💡 For `confirmed`/`not-confirmed`: these are for non-bug issues (enhancement, feature-request, documentation). Create a project to demonstrate the gap, test across versions (a feature may exist in one version but not another), and derive `scope` from your investigation. Steps that find evidence of absence (e.g., project fails to compile because API doesn't exist, grep found no matching implementation) are valid.

## Enum Values

| Enum | Values |
|------|--------|
| **conclusionValue** | `reproduced`, `not-reproduced`, `confirmed`, `not-confirmed`, `needs-platform`, `needs-hardware`, `partial`, `inconclusive` |
| **stepResult** | `success`, `failure`, `wrong-output`, `skip` |
| **stepLayer** | `csharp`, `c-api`, `native`, `deployment`, `setup`, `investigation` |
| **reproProject.type** | `console`, `blazor-wasm`, `docker`, `mobile`, `wpf`, `winforms`, `winui`, `maui`, `test`, `existing`, `simulation` |
| **suggestedAction** | `needs-investigation`, `close-as-fixed`, `close-as-by-design`, `close-with-docs`, `close-as-duplicate`, `convert-to-discussion`, `request-info`, `keep-open` |

## Reproduction Step (each item in `reproductionSteps`)

Required: `stepNumber`, `description`, `layer`, `result`

```json
{
  "stepNumber": 1,
  "description": "Build console repro app",
  "command": "dotnet build",
  "exitCode": 0,
  "output": "Build succeeded (truncate to 2KB on success, 4KB on failure)",
  "layer": "csharp",
  "result": "success"
}
```

## Version Result (each item in `versionResults`)

Required: `version`, `source`, `result`

```json
{
  "version": "3.116.1",
  "source": "nuget",
  "result": "reproduced",
  "notes": "Brief explanation",
  "platform": "host-macos-arm64"
}
```

`source`: `"nuget"` or `"source"`. `result`: `"reproduced"`, `"not-reproduced"`, `"confirmed"`, `"not-confirmed"`, `"error"`, `"not-tested"`.

## Output (required when reproduced, not-reproduced, confirmed, or not-confirmed)

```json
"output": {
  "actionability": { "suggestedAction": "<enum>", "confidence": 0.0-1.0, "reason": "..." },
  "actions": [{ "type": "<actionType>", "description": "...", "risk": "low|medium|high", "confidence": 0.0-1.0, ...data fields }],
  "proposedResponse": { "body": "GitHub comment markdown", "status": "ready|needs-human-edit|do-not-post" },
  "missingInfo": ["What info is needed from reporter"]
}
```

### `add-comment` Risk Calculation

Risk for `add-comment` actions is computed dynamically from content and confidence:

| Condition | Risk | Rationale |
|-----------|------|-----------|
| Factual only (repro findings, version matrix, link references) AND confidence ≥ 0.85 | **low** | Reporting observed facts — minimal reputation exposure |
| Includes workaround or technical suggestion AND confidence ≥ 0.85 | **medium** | Advice could be wrong, but evidence is strong |
| Suggests closing, rejects the issue, or states "by-design" (any confidence) | **high** | Telling a reporter their issue isn't valid always needs human review |
| Any content AND confidence < 0.70 | **high** | Not confident enough to speak for the maintainer |
| Default / everything else | **medium** | |

Other action type risks remain static: `update-labels`=low, `close-issue`=medium, `link-related`=low, `link-duplicate`=medium, `convert-to-discussion`=high, `set-milestone`=low.

### Action Types & Required Fields

| Type | Risk | Required Fields |
|------|------|-----------------|
| `update-labels` | low | `labels` (array of label strings to apply) |
| `add-comment` | **dynamic** (see above) | `comment` (markdown string) |
| `close-issue` | medium | `stateReason` (`completed` or `not_planned`) |
| `link-related` | low | `linkedIssue` (integer issue number) |
| `link-duplicate` | medium | `linkedIssue` (integer issue number) |
| `convert-to-discussion` | high | `category` (discussion category name) |
| `set-milestone` | low | `milestone` (milestone title) |

### Action Data Requirements

Every action MUST include its required data fields to be automatable:
- `update-labels` without `labels` → **invalid**, omit the action
- `add-comment` without `comment` → **invalid**, omit the action
- `close-issue` without `stateReason` → **invalid**, omit the action
- `link-related`/`link-duplicate` without `linkedIssue` → **invalid**, omit the action
- `convert-to-discussion` without `category` → **invalid**, omit the action
- `set-milestone` without `milestone` → **invalid**, omit the action

If you cannot determine the required data for an action, do NOT include it. Hollow actions (correct type but missing data) are worse than no action — they pass schema validation but fail at execution time.

If `proposedResponse` contains comment text, also include a matching `add-comment` action with the same text in `comment`.

## Common Mistakes

1. **Missing `output` when not-reproduced** — `output` + `versionResults` are required for BOTH `reproduced` AND `not-reproduced`.
2. **Missing `blockers` for blocked conclusions** — `needs-platform`, `needs-hardware`, `partial`, `inconclusive` all require `blockers[]` (min 1 item).
3. **`failure`/`wrong-output` steps with `not-reproduced`** — When conclusion is `not-reproduced`, ALL steps must be `success` or `skip`. Record only final successful attempts.
4. **Missing `environment.dockerUsed`** — Always include, even if `false`. It's REQUIRED, not optional.
5. **Missing `proposedResponse.status`** — Must be `"ready"`, `"needs-human-edit"`, or `"do-not-post"`.
6. **Extra fields** — `additionalProperties: false` everywhere. No extra keys.
7. **Null values** — Omit optional fields entirely. Never set to `null`.
8. **Absolute paths** — Redact `/Users/name/` → `$HOME/`, `/tmp/...` → relative descriptions.
9. **Step `result` = expectation** — `result` is TECHNICAL outcome. Build fails = `"failure"` even if that confirms the bug.
10. **Missing `stepNumber`** — Every step needs a sequential number starting from 1.
11. **Using `reproduced`/`not-reproduced` for non-bug issues** — For enhancements, feature requests, and documentation issues, use `confirmed`/`not-confirmed` instead. `reproduced` is for bugs where reported misbehavior was observed.
