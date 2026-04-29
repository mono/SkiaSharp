# Skia Analyst Report Schema (v1)

Unified analysis: what shipped (changelog) + what's missing (gap analysis).

## Top-Level Structure

```json
{
  "meta": { ... },
  "summary": { ... },
  "findings": [ ... ],
  "slides": "...",
  "changelog": "...",
  "nextSteps": [ ... ]
}
```

## `meta`

| Field | Required | Description |
|-------|----------|-------------|
| `date` | Yes | YYYY-MM-DD |
| `schemaVersion` | Yes | `"1.0"` |
| `repo` | Yes | `"mono/SkiaSharp"` |
| `currentMilestone` | Yes | SkiaSharp's current Skia milestone |
| `latestUpstreamMilestone` | Yes | Latest milestone in upstream Skia |
| `releaseNotesSource` | Yes | URL to Skia RELEASE_NOTES.md |
| `scanMode` | No | `"full"`, `"windowed"`, or `"diff"` |
| `refFrom` / `refTo` | No | Git refs when diffing |
| `milestoneFrom` / `milestoneTo` | No | Milestone range when windowed |
| `commitCount` / `prCount` | No | Commit stats when diffing |

## `findings` — Unified Array

Every finding has BOTH lenses — what it means for the changelog AND what it means for the gap analysis.

### Required fields (every finding)

| Field | Type | Changelog lens | Gap lens |
|-------|------|---------------|----------|
| `name` | string | Feature title | Feature title |
| `category` | enum | What area | What area |
| `description` | string | What changed | What's missing |
| `source` | enum | Where found | Where found |
| `changeType` | enum | added/changed/fixed/upstream/... | What kind of change |
| `importance` | enum | breaking/major/minor/patch | How big for users |
| `bindingStatus` | enum | full/partial/missing/... | Do we have it? |
| `impact` | enum | transformative/significant/moderate/minor | How much users care |
| `priority` | enum | critical/high/medium/low | How urgent |

### Optional fields

| Field | Type | When to include |
|-------|------|-----------------|
| `effort` | enum | trivial/small/medium/large — cost to implement |
| `labels` | string[] | Freeform tags for filtering |
| `milestone` | integer | Primary milestone |
| `milestones` | integer[] | Additional milestones |
| `pr` / `prUrl` | int/string | PR reference (changelog lens) |
| `commit` / `commitUrl` | string | Commit reference |
| `author` | string | PR author |
| `affectedTypes` / `affectedMethods` | string[] | C# types/methods affected |
| `cApiFunction` / `csharpMethod` / `csharpFile` | string | Binding details |
| `cppClass` / `cppHeader` / `cppMethod` | string | C++ header scan details |
| `slideBullet` | string | Marketing bullet for this finding |
| `migrationGuide` | string | Migration guide for breaking changes |
| `skiaFeature` / `skiaMilestone` | string/int | Upstream Skia feature reference |
| `dependencyName` / `dependencyFrom` / `dependencyTo` | string | Dependency version tracking |
| `replacement` / `obsoleteMessage` | string | Deprecation details |
| `skillToUse` | string | Which skill to invoke for follow-up |
| `userValue` | string | Why an app developer would want this |

## Enums

### changeType
`added`, `changed`, `fixed`, `deprecated`, `removed`, `dependency`, `platform`, `upstream`

### importance
`breaking`, `major`, `minor`, `patch`

### bindingStatus
| Value | Description |
|-------|-------------|
| `full` | C API + C# wrapper both exist |
| `partial` | C API exists but C# wrapper missing |
| `missing` | Neither C API nor C# wrapper |
| `action_needed` | Wraps deprecated/removed Skia API |
| `correctly_absent` | Skia removed; SkiaSharp never wrapped |
| `not_applicable` | Doesn't need a binding |

### impact
| Value | Meaning |
|-------|---------|
| `transformative` | Unlocks new app categories |
| `significant` | Major visible improvement |
| `moderate` | Useful gap fill |
| `minor` | Small utility, completeness |

### priority
`critical`, `high`, `medium`, `low`

### source
`release-notes`, `header-scan`, `binding-audit`, `git-diff`

### category
`new_feature`, `codec`, `image`, `image_filter`, `shader`, `color`, `canvas`, `path`, `font`, `utility`, `performance`, `behavior_change`, `deprecation`, `security`, `platform`, `dependency`

## `slides` — Marketing Slides Markdown

Pre-rendered emoji-prefixed bullets grouped by theme.

## `changelog` — Detailed Changelog Markdown

Grouped by: Breaking Changes (first), New APIs, Bug Fixes, Performance, Security, Engine Improvements, Deprecations, Dependencies, Platform Changes.

## `nextSteps` — Prioritized Action Items

```json
{ "severity": "high", "action": "Bind SkMesh", "reason": "Transformative feature", "skillToUse": "api-add-review", "effort": "large" }
```
