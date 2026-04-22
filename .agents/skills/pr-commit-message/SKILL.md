---
name: pr-commit-message
description: >
  Write or improve high-signal PR merge commit messages and squash commit messages.
  Use this whenever the user says "create a commit message for this PR", "write a
  merge commit message", "draft the squash commit body", "improve this commit
  message", or asks for a message that preserves the why behind a pull request.
  This skill inspects the PR, linked issues, and code changes so the final commit
  message is readable and useful from git history alone.
---

# PR Commit Message Skill

Write commit messages that preserve the *reason* for the change, not just the diff.

A strong merge commit message should let a future maintainer answer:

- What changed?
- Why was it needed now?
- What bug, regression, release, or design constraint motivated it?
- What approach or tradeoff was chosen?
- Where should I look for more context later?

Use this skill for merge commits, squash commits, and "please write a better commit
message for this PR" requests.

For calibration and examples drawn from Jon Pryor's commit history in `dotnet/android`,
see [`references/pryor-style-guide.md`](references/pryor-style-guide.md).

## Workflow

### 1. Gather the real context first

Before writing anything, inspect the available evidence:

- PR title and description
- linked issues, related PRs, and references in comments
- changed files and diff summary
- error messages, stack traces, logs, or repro steps
- dependency compare links or upstream commit summaries for bump PRs
- release announcements or API docs for platform/update PRs

When working inside a repo with PR context available, prefer first-party context over
guessing:

1. Read the current PR or branch summary.
2. Review the changed files and diffstat.
3. Pull in the linked issue(s) or related PR(s).
4. Identify the core problem, user impact, and notable implementation details.

If a crucial piece of context is missing, either:

- ask one focused question, or
- produce the best possible draft and call out the missing context briefly after it.

### 2. Match the length to the change

Do not force every commit into the same size.

- **Tiny or mechanical change:** the subject alone can be enough when it is truly
  self-explanatory; otherwise add one short explanatory paragraph.
- **Bug fix or regression:** include the symptom, why it happened, and how the fix works.
- **Dependency bump or submodule update:** include a compare link and the important upstream changes.
- **Multi-part change or platform update:** use a narrative body with section headers or bullets.

The message should be as detailed as the change needs, not as short as possible.
For bump commits in particular, a shorter body is fine if the compare link and a few
upstream bullets already carry the important context.

### 3. Write a durable subject line

Prefer the repo's existing conventions. Good defaults are:

- `[Area] Imperative summary (#PR)`
- `Area: imperative summary (#PR)`
- `Imperative summary (#PR)` if there is no area prefix convention

Subject-line rules:

- use an imperative verb: `Add`, `Fix`, `Update`, `Bind`, `Remove`, `Use`, `Bump`
- mention the subsystem or component when it is clear from the diff
- avoid vague summaries such as `address review comments`, `misc fixes`, or `merge PR`
- include the PR number when it is available and fits the repo style

### 4. Structure the body around understanding

Use the following pattern when it fits the change:

```text
Context: <issue/doc/announcement/compare URL or commit SHA>
Fixes: <issue URL or #NNNN>
Changes: <compare URL or related PR/commit>

<1-2 paragraphs describing the problem or motivation>
<show the symptom, failure mode, or external trigger when that helps>

<1-3 paragraphs explaining the solution and the reasoning behind it>

- <optional bullets for notable sub-changes>
- <optional bullets for tradeoffs, risks, or follow-up work>
```

For large or multi-part changes, add section headers named after the actual topic or
sub-problem, not generic template headings. For example:

```text
~~ Fix assertion failure in <CheckApiCompatibility/> ~~
~~ Changes: Minor SDK Versions ~~
~~ Stable API-36.1 ~~
```

If there are many references, use numbered links at the end. `Context:` may also point
at a prior commit SHA when the lineage of the fix matters.

### 5. What good messages include

Include concrete, durable context when it exists:

- linked issue numbers and URLs
- external docs or release announcements
- compare links for dependency bumps
- prior commits or bare SHAs when the new change builds on earlier work
- crash signatures, error snippets, stack traces, or test failures
- the decision-making behind the chosen fix
- compatibility notes, migration notes, or limitations

Translate review feedback into the underlying change. For example:

- **Bad:** `Address PR comments`
- **Good:** `Use explicit bounds checks in SKBitmap.Resize to avoid overflow`

The point is to preserve the engineering story, not the review choreography.

### 6. What to avoid

Avoid commit messages that:

- only restate filenames or the diff
- assume the reader has the PR open
- hide the real issue behind generic phrasing
- include filler with no durable value

Specifically avoid subjects or bodies like:

- `Fix review comments`
- `More changes`
- `PR feedback`
- `Update after discussion`

These help nobody once the PR is merged.

Also keep the formatting in-family for the target repo. If its history favors `  *`
bullets, numbered link references, or indented code/log blocks over fenced ones, keep
that local style instead of forcing a new template.

## Repository-aware heuristics

### Bug fixes

Explain:

1. what broke
2. how it showed up
3. why the bug existed
4. what changed to fix it

If there is a meaningful exception, assertion, or failing log line, include a short
snippet. Evidence makes the message more trustworthy and more searchable later.

### Dependency or submodule bumps

At minimum, include:

- the repository and commit/tag being pulled in
- a compare link
- 1-3 notable upstream changes or fixes relevant to this repo

Do not leave these as anonymous bump commits unless the repo already has a strict
mechanical bump format.

### Release / API / platform updates

State:

- the release or platform milestone
- why the project needs to respond now
- any compatibility or stability implications
- relevant docs or announcement links

### Multi-part PRs

When one PR contains multiple tightly related changes, organize the body so each part
has a name and purpose. Use bullets or short section headers instead of a wall of text.

## Output format

When the user asks for a commit message, return:

1. **One polished commit message** in a fenced code block, ready to paste.
2. **Optional `Missing context:` bullets** only if important context could not be found.

Do not dump brainstorming notes unless the user explicitly asks for alternatives.

## Quality bar

Before finalizing, make sure the message answers:

- Why was this PR worth merging?
- What would a future maintainer need to know without reopening the PR?
- What specific issue, failure mode, release, or behavior change does this correspond to?

If the answer is still "you had to be there," the message needs more context.

## Examples

**Weak**

```text
Fix NativeAOT issue
```

**Better**

```text
[NativeAOT] Wait for GC bridge processing via JNIEnv value manager (#12345)

Context: https://github.com/org/repo/issues/12340
Context: abcdef1234567890abcdef1234567890abcdef12

Apps using NativeAOT could abort during JNI wrapper creation while startup code
was still waiting on pending GC bridge work. The failures were intermittent, but
the stack traces consistently pointed at wrapper initialization before the active
value manager had finished processing queued state.

Route the wait through `JNIEnv.ValueManager?.WaitForGCBridgeProcessing()` instead
of the older runtime-specific path. This keeps the synchronization aligned with
the current value manager implementation and removes the startup crash without
changing the existing wrapper call pattern.
```

**Weak**

```text
Bump submodule
```

**Better**

```text
Bump to org/dependency@abc12345 (#4567)

Changes: https://github.com/org/dependency/compare/oldsha...abc12345

  * pull in the upstream fix for manifest parsing on Windows
  * include the follow-up change that avoids duplicate resolver entries during restore

These are the only behavior changes used by this PR; the remaining updates are
mechanical fallout from the bump.
```
