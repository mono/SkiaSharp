# Jon Pryor-style commit message notes

These notes are distilled from 100 recent Jon Pryor-authored commits in
`dotnet/android`.

## Observed traits

- Subjects are concise and imperative, usually with an area prefix and PR number:
  - `[Mono.Android] Bind API-37 Beta 3 (#11038)`
  - `[native] Ensure \`function_name()\` is part of log message (#10302)`
  - `Bump to dotnet/java-interop/main@d3d3a1bf (#9921)`
- Bodies are often narrative and evidence-driven.
- Many messages start with `Context:`, `Fixes:`, or `Changes:` lines.
- `Context:` can point to a URL *or* a bare commit SHA when prior history matters.
- Longer messages show the engineering story: symptom -> investigation -> root cause -> fix.
- Complex changes are split into topic-specific sections with headings or bullets.
- Some commits are intentionally subject-only when the change is mechanical and obvious.

## What makes the messages useful later

They preserve information that would otherwise disappear once the PR is merged:

- release announcements and API diff pages
- upstream compare links for bumps
- stack traces, failure messages, or repro commands
- reasoning behind the chosen approach
- constraints and tradeoffs

## Practical rules to emulate

1. Put the repo or subsystem in the subject when that helps scanning history.
2. Include the PR number in the subject when the repo convention supports it.
3. Add top-of-message links or prior commit SHAs so readers can reconstruct the source material quickly.
4. Show evidence for bug fixes instead of saying "it crashes."
5. Explain why the change was needed before describing the implementation.
6. Use bullets or section headings when a PR contains multiple related fixes; name sections after the actual topic.
7. For dependency bumps, include the compare link and call out the important pulled-in changes.
8. Preserve compatibility notes, workarounds, and limitations when they matter.
9. Respect the local formatting style for bullets and code/log blocks instead of imposing a new one.

## Example patterns

### Bug fix

```text
[area] Fix specific failure mode (#PR)

Context: <issue URL or prior commit SHA>

<what broke and how it surfaced>

<why it happened>

<what changed and why this fix is appropriate>
```

### Dependency bump

```text
Bump to org/repo/branch@sha (#PR)

Fixes: <issue URL>
Changes: <compare URL>

  * <upstream change 1>
  * <upstream change 2>

<short explanation of why this bump matters here>
```

### Platform or release update

```text
[Area] Bind or update <platform release> (#PR)

Context: <announcement URL>
Context: <API diff URL>

<release summary>
<compatibility, stability, or migration implications>
<what this repo is doing in response>
```
