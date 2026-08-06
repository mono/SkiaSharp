# Structure and style for conceptual articles

This reference adapts the reusable editorial guidance in MicrosoftDocs/Contribute to SkiaSharp's local
DocFX site. It deliberately omits Microsoft Learn publishing metadata and authoring-pack extensions.

## Metadata, title, and introduction

Use the metadata supported by this docset:

```yaml
---
title: "Specific sentence-case title"
description: "Describe the reader outcome, approach, and meaningful scope."
---
```

- Do not add `ms.author`, `ms.date`, `ms.topic`, `ms.service`, or other Learn-only fields.
- Use one H1 after front matter. Keep it aligned with the metadata title.
- Make the title specific enough to distinguish the article in search and the TOC.
- For a section-landing `index.md`, use the section/topic noun phrase readers see in the TOC; use a
  verb-led task title only when the landing page itself is a procedure.
- Keep the description concise and natural. Roughly 115-160 characters often works, but do not pad it to
  reach a target.
- Open with the outcome, use case, and most consequential constraint. Do not repeat the title as an
  italic subtitle.

## Headings and scanning

- Use sentence case.
- Make H2s describe the decision, task, model, symptom, or result in that section. Avoid generic headings
  such as "Overview" or "Details."
- Preserve a logical hierarchy; do not skip levels to get smaller text.
- Keep paragraphs focused and put conditions before instructions.
- Use tables for genuine comparisons and lists for parallel choices.
- Keep long reference enumerations out of the task flow; link to API reference or a focused reference
  section instead.

The TOC and H2s are part of the reader interface. A reader should be able to predict the article's path
from them.

## Procedures

- Introduce the goal and prerequisites before the steps.
- Use a numbered list when order matters.
- Put one action in each step; place the reason or expected result immediately after it.
- Use imperative verbs and exact UI labels, commands, paths, and values.
- If a step branches, state the condition first and separate the paths clearly.
- End with a verification step rather than assuming success.

Do not hide required actions in notes, code comments, or paragraphs between numbered steps.

## Voice and global readiness

- Address the reader as "you" and use active voice.
- Prefer familiar words and short sentences.
- Use present tense for current behavior; avoid future tense when describing what a command does.
- Define a specialized term at first use, then use the same term consistently.
- Expand uncommon acronyms at first use.
- Avoid idioms, jokes, cultural references, and spatial instructions such as "see above" when a heading
  reference is clearer.
- Avoid "simple," "easy," "obvious," and "just" when setup or recovery is nontrivial.
- Avoid dismissive or exclusionary terms and assumptions about the reader's ability, environment, or
  preferred platform.

Short, explicit prose is easier to scan, translate, and maintain.

## Inclusive language

- Use gender-neutral terms unless gender is relevant.
- Describe the task or state rather than labeling a person by ability or experience.
- Avoid ableist idioms such as "sanity check," "blind to," or "crippled"; name the actual validation,
  omission, or limitation.
- Do not assume the reader uses a particular platform, input method, visual theme, or spoken language.
- Avoid humor and metaphors that depend on culture or can obscure technical meaning.

These checks are a practical local baseline. MicrosoftDocs/Contribute points to the separate Microsoft
Writing Style Guide for its complete inclusive-language policy.

## Text formatting

- Use **bold** for UI elements and labels the reader sees.
- Use *italics* for a newly introduced term or a placeholder the reader replaces.
- Use `code` for APIs, commands, filenames, paths, configuration keys, values, and literal input.
- Do not use formatting only for emphasis; rewrite the sentence so its point is clear.

## Links and xrefs

- Use `xref:` for SkiaSharp and .NET API reference.
- Use exact-case UIDs. Use the wildcard member form only when intentionally linking to an overload group.
- Use relative `.md` links for conceptual articles in this docset.
- Use descriptive link text that tells the reader what they will get; never use "click here."
- Use HTTPS and prefer current first-party sources.
- Recheck every heading fragment after renaming a heading.
- Link to third-party material only when it is necessary, maintained, and gives the reader a clear next
  step. Do not outsource a core procedure to an unstable blog post.

## Alerts

Readers often skip alerts. Keep required task information in the main flow, use no more than one or two
alerts per article when practical, and never stack alerts.

| Alert | Use for |
|---|---|
| `NOTE` | Context that can be skipped without preventing success |
| `IMPORTANT` | Information required for success |
| `CAUTION` | An action that can cause recoverable harm |
| `WARNING` | A risk of serious or difficult-to-reverse harm |

Do not promote ordinary prose to an alert merely to make it visible.

## Images and accessibility

- Use images only when they communicate spatial or visual information better than text.
- Do not use screenshots to present code; code blocks are searchable, copyable, and maintainable.
- Write alt text that conveys the image's purpose rather than repeating its filename or nearby caption.
- Explain complex diagrams, graphs, and multi-step screenshots in surrounding text or a long
  description.
- Do not rely on color, shape, or position alone to communicate meaning.
- Crop screenshots to the relevant UI, avoid sensitive data, and prefer images that will not become
  obsolete with minor theme or layout changes.

## Related links

End with a small set of likely next actions. Avoid dumping every related API or article. An overview/index
page may have an "In this section" list because routing is its primary purpose; a task article should
usually link only to prerequisites, alternatives, and the next task.
