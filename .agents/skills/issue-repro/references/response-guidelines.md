# Response Guidelines

How to write the `proposedResponse.body` Markdown GitHub comment after reproduction.

## Tone and Evidence

- **Tone:** Professional, helpful, empathetic. Thank the reporter.
- **Evidence:** Cite specific versions tested, platforms, and error messages observed.
- **Workarounds:** If discovered during reproduction, include them prominently.

## Status Thresholds

| Status | When |
|--------|------|
| `ready` | Confidence ≥ 0.85 — comment can be posted as-is |
| `needs-human-edit` | Confidence < 0.85, or sensitive/ambiguous situation |
| `do-not-post` | Internal-only findings, not suitable for public comment |

## Templates by Conclusion

### Reproduced (fixed on latest)

```markdown
Thanks for reporting this! We were able to reproduce the issue with SkiaSharp {reporter_version}.

The good news is that this appears to be fixed in {fixed_version}. Could you try upgrading?

**Tested versions:**
| Version | Result |
|---------|--------|
| {reporter_version} | ❌ Reproduced |
| {fixed_version} | ✅ Fixed |

{workaround_section_if_any}
```

### Not reproduced

```markdown
Thanks for the report. We attempted to reproduce this on {environment} with SkiaSharp {reporter_version} but were unable to observe the reported behavior.

Could you share the following to help us investigate further?
{missing_info_list}
```

### Reproduced universally

```markdown
We've confirmed this issue. The reported behavior reproduces on {platforms} with SkiaSharp {reporter_version}.

We're tracking this for a fix. {workaround_section_if_any}
```

### Confirmed (enhancement/feature request)

```markdown
Thanks for the suggestion! We investigated and can confirm that {feature_description} is not currently available in SkiaSharp.

**Investigation:**
{investigation_summary}

{related_infrastructure_if_any}

We've flagged this for consideration. {workaround_section_if_any}
```

### Confirmed (documentation gap)

```markdown
Thanks for pointing this out. We can confirm that the documentation for {topic} is {missing_or_incorrect}.

**Finding:**
{investigation_summary}

We'll work on updating the documentation.
```

### Not confirmed

```markdown
Thanks for the report. We investigated and found that {feature_or_topic} {actually_exists_or_is_documented}.

{evidence_summary}

Could you take a look and let us know if we're missing something?
{missing_info_list_if_any}
```
