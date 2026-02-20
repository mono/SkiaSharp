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
| `needs-platform`, `needs-hardware`, `partial`, `inconclusive` | `blockers` (string array, min 1 item) |

## Step-Result Constraints by Conclusion

| Conclusion | Step Constraint |
|------------|----------------|
| `reproduced` | Must contain ≥1 step with `result: "failure"` or `"wrong-output"` |
| `not-reproduced` | Must contain ≥1 `"success"` step AND zero `"failure"`/`"wrong-output"` steps |

⚠️ For `not-reproduced`: if a setup step failed then succeeded on retry, record **only the final successful attempt**. Any `failure`/`wrong-output` step will fail validation.

## Enum Values

| Enum | Values |
|------|--------|
| **conclusionValue** | `reproduced`, `not-reproduced`, `needs-platform`, `needs-hardware`, `partial`, `inconclusive` |
| **stepResult** | `success`, `failure`, `wrong-output`, `skip` |
| **stepLayer** | `csharp`, `c-api`, `native`, `deployment`, `setup` |
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

`source`: `"nuget"` or `"source"`. `result`: `"reproduced"`, `"not-reproduced"`, `"error"`, `"not-tested"`.

## Output (required when reproduced or not-reproduced)

```json
"output": {
  "actionability": { "suggestedAction": "<enum>", "confidence": 0.0-1.0, "reason": "..." },
  "actions": [{ "type": "<actionType>", "description": "...", "risk": "low|medium|high", "confidence": 0.0-1.0 }],
  "proposedResponse": { "body": "GitHub comment markdown", "status": "ready|needs-human-edit|do-not-post" },
  "missingInfo": ["What info is needed from reporter"]
}
```

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
