# MicrosoftDocs/Contribute source map

This route adapts the reusable editorial system from
[`MicrosoftDocs/Contribute`](https://github.com/MicrosoftDocs/Contribute) and combines it with
SkiaSharp-specific source verification. Use this file when maintaining the skill; normal authoring and
review runs should load the focused references instead.

## Adopted guidance

| Topic | Source | Local adaptation |
|---|---|---|
| Contribution/article triage | [`how-to-write-overview.md`](https://github.com/MicrosoftDocs/Contribute/blob/main/Contribute/content/how-to-write-overview.md) | Focused fix vs substantive review vs new/major rewrite |
| Voice, intent, concise/scannable prose, global readiness | [`style-quick-start.md`](https://github.com/MicrosoftDocs/Contribute/blob/main/Contribute/content/style-quick-start.md) | `structure-and-style.md` voice and localization pass |
| .NET voice and tone | [`dotnet-voice-tone.md`](https://github.com/MicrosoftDocs/Contribute/blob/main/Contribute/content/dotnet/dotnet-voice-tone.md) | Reader-focused introduction, second person, active voice, present tense |
| .NET article skeleton | [`dotnet-style-guide.md`](https://github.com/MicrosoftDocs/Contribute/blob/main/Contribute/content/dotnet/dotnet-style-guide.md) | Separate SkiaSharp blueprints for each reader intent |
| Code blocks and intentionally bad code | [`code-in-docs.md`](https://github.com/MicrosoftDocs/Contribute/blob/main/Contribute/content/code-in-docs.md) | Snippet/sample distinction, bad-code labeling, build/source verification |
| .NET sample quality and exception handling | [`dotnet-contribute.md`](https://github.com/MicrosoftDocs/Contribute/blob/main/Contribute/content/dotnet/dotnet-contribute.md) | Complete samples build, handle expected failures, avoid broad catches |
| Alerts, headings, images, and alt text | [`markdown-reference.md`](https://github.com/MicrosoftDocs/Contribute/blob/main/Contribute/content/markdown-reference.md) | Sparse alerts, accessible images, and DocFX-compatible Markdown; omit Learn's `TIP` alert to preserve this docset's established four-kind convention |
| Link and xref quality | [`how-to-write-links.md`](https://github.com/MicrosoftDocs/Contribute/blob/main/Contribute/content/how-to-write-links.md) | Exact UIDs, relative conceptual links, descriptive HTTPS links |
| Bold/italic/code usage | [`text-formatting-guidelines.md`](https://github.com/MicrosoftDocs/Contribute/blob/main/Contribute/content/text-formatting-guidelines.md) | UI/new-term/code formatting semantics |
| Discoverable titles and headings | [`seo-reference.md`](https://github.com/MicrosoftDocs/Contribute/blob/main/Contribute/content/seo-reference.md) | Specific titles/H2s without importing Learn SEO metadata quotas |
| .NET PR review triage | [`dotnet-pr-review.md`](https://github.com/MicrosoftDocs/Contribute/blob/main/Contribute/content/dotnet/dotnet-pr-review.md) | Focused/substantive/draft review depth and publish verdict |

## Intentionally not copied

SkiaSharp's DocFX site is not Microsoft Learn's Open Publishing System. Do not add:

- `ms.author`, `ms.date`, `ms.topic`, `ms.service`, `ms.custom`, or Learn ownership metadata from
  [`metadata.md`](https://github.com/MicrosoftDocs/Contribute/blob/main/Contribute/content/metadata.md).
- CLA, `#sign-off`, OPS validation, staging, auto-merge, or label mechanics from
  [`process-pull-request.md`](https://github.com/MicrosoftDocs/Contribute/blob/main/Contribute/content/process-pull-request.md).
- Learn Authoring Pack instructions or Learn-only Markdown extensions merely because they appear in the
  source repository.
- `:::code source=...:::` references until this repository adopts and validates an extracted-snippet
  system.
- Learn-specific title/description quotas as hard requirements. Local metadata should remain concise and
  useful without invented fields or padded prose.

The Contribute repository delegates deeper inclusive-language and procedure rules to the separate
Microsoft Writing Style Guide. This skill includes practical accessibility and inclusive-language checks
but does not claim that Contribute contains a complete policy.
