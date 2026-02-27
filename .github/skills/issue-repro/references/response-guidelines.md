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
