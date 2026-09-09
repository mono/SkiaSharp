# Overview and decision blueprint

Use an overview when the reader needs to understand available approaches, choose one, and navigate to the
right task. It should route readers, not duplicate every routed article.

## Plan

Define:

```text
Decision:
Audience:
Options in scope:
Comparison dimensions:
Recommended default:
Exceptions to the default:
Next task for each option:
```

Comparison dimensions must change the reader's decision: platform support, acceleration, ownership,
latency, complexity, compatibility, or lifecycle. Avoid tables filled with facts that do not help choose.

## Suggested shape

```markdown
---
title: "<Technology or decision> overview"
description: "<What the reader can choose or understand and the scope of the comparison>"
---

# <Technology or decision> overview

<State the decision, the recommended starting point, and the condition that changes it.>

## Choose an approach

| Approach | Use when | Avoid or reconsider when |
|---|---|---|
| <Option A> | <Decision criterion> | <Constraint> |
| <Option B> | <Decision criterion> | <Constraint> |

<Explain the recommendation and important caveats that do not fit the table.>

## How the approaches relate

<Only the mental model needed to understand the choice.>

## In this section

- [<Task-oriented article title>](<relative-link>) — <specific outcome>
- [<Task-oriented article title>](<relative-link>) — <specific outcome>

## Related links

- <External prerequisite or next section, only if useful>
```

## Quality checks

- The article names a recommended starting point or explains why no single default exists.
- Every option links to a concrete next task.
- The comparison is accurate for every named platform/version.
- "In this section" descriptions state outcomes rather than repeat titles.
- Details that do not affect the decision move to concept, task, or API-reference pages.
