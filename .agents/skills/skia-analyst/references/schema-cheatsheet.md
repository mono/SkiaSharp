# Skia Analyst Report Schema (v1)

## Top-Level Structure

```json
{
  "meta": { ... },
  "summary": { ... },
  "findings": [ ... ]
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

## `summary`

| Field | Required | Description |
|-------|----------|-------------|
| `totalFindings` | Yes | Number of findings |
| `byChangeType` | No | Counts per changeType |
| `byImportance` | No | Counts per importance |
| `byBindingStatus` | No | Counts per bindingStatus |
| `byImpact` | No | Counts per impact |
| `byPriority` | No | Counts per priority |
| `bySource` | No | Counts per source |

## `findings` — Unified Array

Every finding has BOTH lenses — what it means for the changelog AND for the gap analysis.

### Required fields (every finding)

| Field | Type | Description |
|-------|------|-------------|
| `name` | string | Feature title |
| `category` | enum | What area (new_feature, codec, image, image_filter, shader, color, canvas, path, font, utility, performance, behavior_change, deprecation, security, platform, dependency) |
| `description` | string | What changed / what's missing |
| `source` | enum | release-notes, header-scan, binding-audit, git-diff |
| `changeType` | enum | added, changed, fixed, deprecated, removed, dependency, platform, upstream |
| `importance` | enum | breaking, major, minor, patch |
| `bindingStatus` | enum | full, partial, missing, action_needed, correctly_absent, not_applicable |
| `impact` | enum | transformative, significant, moderate, minor |
| `priority` | enum | critical, high, medium, low |

### Optional fields

| Field | Type | When to include |
|-------|------|-----------------|
| `effort` | enum | trivial/small/medium/large — cost to implement |
| `labels` | string[] | Freeform tags for filtering |
| `milestone` | integer | Primary Skia milestone |
| `milestones` | integer[] | Additional milestones |
| `milestoneDeprecated` | integer | When deprecated |
| `milestoneRemoved` | integer | When removed |
| `pr` / `prUrl` | int/string | PR reference |
| `commit` / `commitUrl` | string | Commit reference |
| `issue` / `issueUrl` | int/string | Issue reference |
| `author` | string | PR/commit author |
| `affectedTypes` / `affectedMethods` | string[] | C# types/methods affected |
| `cApiFunction` / `cApiFile` | string | C API details |
| `csharpMethod` / `csharpFile` | string | C# binding details |
| `cppClass` / `cppHeader` / `cppMethod` | string | C++ header scan details |
| `platforms` | string[] | Affected platforms |
| `skiaApi` | string | Upstream Skia API name |
| `skiaFeature` / `skiaMilestone` | string/int | Upstream feature reference |
| `userValue` | string | Why an app developer would want this |
| `notes` | string | Additional context |
| `slideBullet` | string | Marketing bullet (for downstream tooling) |
| `migrationGuide` | string | Migration guide for breaking changes |
| `replacement` / `obsoleteMessage` | string | Deprecation details |
| `dependencyName` / `dependencyFrom` / `dependencyTo` | string | Dependency version tracking |
| `skillToUse` | string | Which skill to invoke for follow-up |
