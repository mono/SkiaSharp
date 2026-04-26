# Release Notes Schema (v1)

JSON schema for the Release Notes Writer report. One unified `findings` array with two
pre-rendered markdown sections (slides + changelog). The formal JSON Schema is at
`release-notes-schema.json`.

## Top-Level Structure

```json
{
  "meta": { ... },
  "summary": { ... },
  "findings": [ ... ],
  "slides": "...",
  "changelog": "..."
}
```

## `meta` — Report Metadata

```json
{
  "date": "2026-04-25",
  "schemaVersion": "1.0",
  "repo": "mono/SkiaSharp",
  "refFrom": "v3.116.1",
  "refTo": "v3.119.0",
  "shaFrom": "abc123...",
  "shaTo": "def456...",
  "dateFrom": "2024-11-15",
  "dateTo": "2025-03-20",
  "commitCount": 47,
  "prCount": 32,
  "skiaMilestoneBumped": true,
  "skiaMilestoneFrom": 128,
  "skiaMilestoneTo": 133
}
```

| Field | Required | Description |
|-------|----------|-------------|
| `date` | Yes | Date report was generated (YYYY-MM-DD) |
| `schemaVersion` | Yes | Must be `"1.0"` |
| `repo` | Yes | Repository name |
| `refFrom` | Yes | Starting ref (tag, branch, or SHA) |
| `refTo` | Yes | Ending ref |
| `shaFrom` | Yes | Resolved SHA of refFrom |
| `shaTo` | Yes | Resolved SHA of refTo |
| `dateFrom` | No | Date of refFrom commit |
| `dateTo` | No | Date of refTo commit |
| `commitCount` | Yes | Number of commits in range |
| `prCount` | No | Number of PRs in range |
| `skiaMilestoneBumped` | No | Whether Skia submodule milestone changed |
| `skiaMilestoneFrom` | No | Previous Skia milestone |
| `skiaMilestoneTo` | No | New Skia milestone |

## `summary` — Overview Counts

```json
{
  "totalFindings": 42,
  "byChangeType": {
    "added": 12, "changed": 3, "fixed": 15, "deprecated": 2,
    "removed": 0, "dependency": 5, "platform": 5
  },
  "byImportance": {
    "breaking": 1, "major": 8, "minor": 18, "patch": 15
  },
  "byLabel": {
    "security": 3, "skia-upstream": 5, "breaking-change": 1
  }
}
```

## `findings` — Unified Array

Every finding (new API, bug fix, dependency bump, etc.) in one array with the same shape.

### Required fields (every finding)

| Field | Type | Description |
|-------|------|-------------|
| `name` | string | Human-readable title |
| `changeType` | string | See changeType enum below |
| `importance` | string | `breaking`, `major`, `minor`, `patch` |
| `description` | string | What changed and why it matters |

### Optional fields

| Field | Type | When to include |
|-------|------|-----------------|
| `labels` | string[] | Freeform tags: `security`, `skia-upstream`, `breaking-change`, `new-platform`, `performance`, `deprecation` |
| `pr` | integer | PR number |
| `prUrl` | string | Full PR URL |
| `commit` | string | Commit SHA (short or full) |
| `commitUrl` | string | Full commit URL |
| `issue` | integer | Linked issue number |
| `issueUrl` | string | Full issue URL |
| `author` | string | PR/commit author |
| `affectedTypes` | string[] | C# types affected (e.g., `["SKCanvas", "SKMesh"]`) |
| `affectedMethods` | string[] | Methods affected (e.g., `["SKImage.FromPicture"]`) |
| `cApiFunction` | string | New/changed C API function |
| `csharpMethod` | string | New/changed C# method |
| `csharpFile` | string | C# file path |
| `skiaFeature` | string | Related upstream Skia feature (if from submodule bump) |
| `skiaMilestone` | integer | Skia milestone that introduced this |
| `dependencyName` | string | Dependency name (for dependency changes) |
| `dependencyFrom` | string | Previous dependency version |
| `dependencyTo` | string | New dependency version |
| `platforms` | string[] | Affected platforms |
| `migrationGuide` | string | Migration guide for breaking changes (markdown) |
| `slideBullet` | string | Marketing slide bullet for this finding |
| `notes` | string | Additional context |

### Example: New API

```json
{
  "name": "Direct3D rendering support on Windows",
  "changeType": "added",
  "importance": "major",
  "description": "Compile Skia with Direct3D on Windows platform, enabling D3D backend alongside OpenGL and Vulkan.",
  "labels": ["new-platform"],
  "pr": 2823,
  "prUrl": "https://github.com/mono/SkiaSharp/pull/2823",
  "author": "nicklef",
  "affectedTypes": ["GRContext"],
  "platforms": ["Windows"],
  "slideBullet": "🎮 **Direct3D Support** — Windows apps can now render via Direct3D alongside OpenGL and Vulkan"
}
```

### Example: Bug Fix

```json
{
  "name": "Fix SKImage.FromPicture implementation",
  "changeType": "fixed",
  "importance": "minor",
  "description": "Fixed incorrect implementation of SKImage.FromPicture that could produce wrong results.",
  "pr": 3231,
  "prUrl": "https://github.com/mono/SkiaSharp/pull/3231",
  "affectedMethods": ["SKImage.FromPicture"],
  "csharpFile": "binding/SkiaSharp/SKImage.cs"
}
```

### Example: Dependency Update

```json
{
  "name": "Update Vulkan Memory Allocator to 3.2.1",
  "changeType": "dependency",
  "importance": "patch",
  "description": "Bumped vulkanmemoryallocator from 3.1.0 to 3.2.1.",
  "pr": 3196,
  "prUrl": "https://github.com/mono/SkiaSharp/pull/3196",
  "dependencyName": "vulkanmemoryallocator",
  "dependencyFrom": "3.1.0",
  "dependencyTo": "3.2.1"
}
```

### Example: Security / Build Hardening

```json
{
  "name": "Enable Control Flow Guard for Windows DLLs",
  "changeType": "platform",
  "importance": "minor",
  "description": "Enables CFG on all Windows native DLLs for improved security.",
  "labels": ["security"],
  "pr": 3397,
  "prUrl": "https://github.com/mono/SkiaSharp/pull/3397",
  "platforms": ["Windows"]
}
```

### Example: Breaking Change

```json
{
  "name": "Remove legacy SKPaint text methods",
  "changeType": "removed",
  "importance": "breaking",
  "description": "SKPaint.TextSize, TextAlign, and related properties removed in favor of SKFont.",
  "labels": ["breaking-change"],
  "pr": 3100,
  "prUrl": "https://github.com/mono/SkiaSharp/pull/3100",
  "affectedTypes": ["SKPaint"],
  "affectedMethods": ["SKPaint.TextSize", "SKPaint.TextAlign"],
  "migrationGuide": "### Before\\n```csharp\\npaint.TextSize = 24;\\n```\\n### After\\n```csharp\\nvar font = new SKFont(typeface, 24);\\n```"
}
```

## `slides` — Marketing Slides Markdown

Pre-rendered markdown string with themed, emoji-prefixed bullet points suitable for conference
slides and blog posts. Grouped by theme, not by PR number.

## `changelog` — Detailed Changelog Markdown

Pre-rendered markdown string with full technical changelog. Grouped by change type with
Breaking Changes always first. Includes PR references and affected APIs.

## changeType Enum

| Value | Description |
|-------|-------------|
| `added` | New API, class, method, or capability |
| `changed` | Modified behavior or signature |
| `fixed` | Bug fix |
| `deprecated` | New `[Obsolete]` marker |
| `removed` | Removed API or feature |
| `dependency` | Dependency version bump |
| `platform` | New platform support, build flags |

## importance Enum

| Value | Description |
|-------|-------------|
| `breaking` | Removes or changes existing public API |
| `major` | New capability or significant feature |
| `minor` | Enhancement, quality improvement |
| `patch` | Bug fix, internal improvement |
