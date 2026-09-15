# Source API documentation review criteria

## CRITICAL

- Malformed source XML documentation that produces invalid compiler XML.
- Fabricated APIs, invalid `cref` targets, or examples that do not compile.
- Incorrect ownership, disposal, threading, security, native-data-layout, or
  standards guidance.
- Use of an error-obsolete API in an example.

## IMPORTANT

- A new or changed consumer-visible public API has no accurate `///` summary
  or lacks required parameter/return/value information.
- The prose contradicts source validation, default values, nullability, or
  factory failure behavior.
- A property accessor verb, boolean convention, constructor form, or example
  disposal pattern is incorrect.
- A generated binding comment was manually changed instead of regenerated.

## MINOR

- A summary can be clearer without changing factual meaning.
- A useful nonessential example, `<remarks>`, or `<paramref>` is missing.
- Whitespace or punctuation does not follow the source-comment conventions.

Report each finding as:

```
SEVERITY | class | source file | member | message with source citation
```

Comments in compiler XML that correspond only to implementation assemblies are
not public-reference coverage gaps. The `ref` package XML defines that
consumer-facing scope.
