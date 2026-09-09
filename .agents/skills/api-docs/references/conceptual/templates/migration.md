# Migration blueprint

Use a migration article when the reader has working code on one supported approach and needs to move to
another. Preserve their mental model by separating what changes from what remains valid.

## Plan

Define:

```text
Supported source state:
Supported target state:
Why migrate:
What remains unchanged:
Concept/API mappings:
Behavioral differences:
Compatibility or rollback path:
Completion check:
```

Do not present a replacement as universal when a source platform/backend has no target equivalent.

## Suggested shape

```markdown
---
title: "Migrate from <source> to <target>"
description: "<Migration outcome and the important scope or limitation>"
---

# Migrate from <source> to <target>

<State who should migrate, the supported starting/target states, and the largest behavioral change.>

## Before you migrate

- <Prerequisite/version/platform>
- <Unsupported or no-direct-equivalent case>
- <Fallback or reason to remain on the source approach>

## Map source concepts to target concepts

| Source | Target | What changes |
|---|---|---|
| <API/concept> | <API/concept> | <Behavioral difference> |

## What stays the same

<Name valid drawing, data, or application code the reader should keep.>

## Compare the workflows

### Source

```csharp
// Existing supported pattern.
```

### Target

```csharp
// Replacement pattern with complete failure and lifetime handling.
```

## Migrate step by step

1. <Replace or configure one concept.>
2. <Handle the changed lifecycle/failure path.>
3. <Remove obsolete source-specific setup only after the target works.>

## Verify the migration

<Behavioral parity, output, performance boundary, or platform check.>

## Related links

- <Target how-to>
- <Rollback/fallback or troubleshooting path>
```

## Quality checks

- "Before" code is still valid for the documented source version and is labeled as the source pattern,
  not as bad code.
- "Target" code uses current APIs and preserves required behavior.
- The article states what remains unchanged.
- Semantic differences, not just renamed methods, are explicit.
- Unsupported migrations have a supported alternative or clear boundary.
- Verification checks behavior, not only compilation.
