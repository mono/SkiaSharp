# How-to blueprint

Use a how-to when the reader wants to complete one concrete task. The article should be executable in
order and end with an observable result.

## Plan

Record:

```text
Reader:
Starting state:
Outcome:
Prerequisites:
Supported platforms/versions:
Completion check:
Out of scope:
```

If several supported approaches require different setup, explain the choice first and give each path its
own procedure. Do not interleave platform branches step by step.

## Suggested shape

```markdown
---
title: "<Verb> <specific result> with SkiaSharp"
description: "<Outcome, approach, and meaningful scope>"
---

# <Verb> <specific result> with SkiaSharp

<What the reader will produce, when to use this approach, and the key constraint.>

## Prerequisites

- <Required package/workload/platform/context>
- <Existing reader state>

## <Optional decision the reader must make>

<Short comparison and explicit recommendation criteria.>

## <Complete the first meaningful phase>

1. <Action.>

   <Code or command.>

   <Expected result or reason.>

2. <Next action.>

## <Complete the next phase>

...

## Verify the result

<Concrete check, expected output, or observable behavior.>

## Related links

- <One prerequisite, alternative, or next task>
```

Rename headings to the actual actions. Omit optional sections rather than publishing empty boilerplate.

## Quality checks

- Each numbered step contains one action in the order performed.
- Required values are defined before use.
- Complete code handles failure and ownership.
- Platform-specific branches are clearly scoped.
- The final verification proves the promised outcome.
- Troubleshooting content stays focused on failures likely during this task; link to a dedicated
  troubleshooting article for broader diagnosis.
