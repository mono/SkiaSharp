# Concept blueprint

Use a concept article when the reader needs a mental model before making decisions or completing tasks.
The article should explain relationships, lifecycle, or behavior and then connect the model to practice.

## Plan

Define:

```text
Question the article answers:
What the reader already knows:
Terms that need definitions:
Components and relationships:
Lifecycle or data flow:
Constraints and invariants:
Task that applies this concept:
```

## Suggested shape

```markdown
---
title: "<Specific concept> in SkiaSharp"
description: "<What model or relationship the reader will understand and why it matters>"
---

# <Specific concept> in SkiaSharp

<State the practical question this model answers and where the concept affects code.>

## <Name the model or relationship>

<Define only the terms needed for this explanation.>

## <Describe the lifecycle or data flow>

<Walk through components in the order data, ownership, or work moves between them.>

## <Explain constraints and consequences>

| Constraint or state | Consequence for the reader |
|---|---|
| <Condition> | <Behavior or required action> |

## Apply the concept

<A focused example or link to a how-to that makes the model concrete.>

## Related links

- <Task that applies the concept>
```

## Quality checks

- The model answers a practical reader question rather than cataloging types.
- Terms are defined once and used consistently.
- Ownership arrows, lifecycle order, and thread boundaries match source.
- Any diagram has equivalent explanatory text and does not rely on color alone.
- The applied example shows why the model matters.
