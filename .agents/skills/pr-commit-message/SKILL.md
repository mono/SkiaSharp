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

Write a historical record, not a diff summary. A future maintainer should be able to
recover what changed, why it mattered, what failed or constrained the design, why this
approach was chosen, and where the supporting evidence lives without reopening the PR.

Optimize for **durable understanding**, not assistant-response brevity. Remove repetition
and development chronology, but preserve every independently useful root cause, design
decision, compatibility rule, limitation, and proof point.

## Non-negotiable evidence contract

PR prose and early commit bodies often preserve useful history, but they can also preserve
stale constraints, mistaken link interpretations, and superseded absolutes. Prevent those
high-cost failures before drafting:

- Count the private ledger's material topics. With two or more topics, the final draft must
  have the same number of named topic sections; do not count the overview, and do not let a
  continuous narrative or overview stand in for those sections.
- For each candidate reference, privately record the target's actual entity type, title, and
  exact relationship after opening it. Omit the reference when that relationship is not
  supported, even when the PR description labels or explains it.
- For each candidate universal, bound, count, or inequality, privately record the complete
  search universe and check it for counterexamples. Proof over a subset cannot support a
  claim about the whole API, repository, platform set, or generated surface. One outlier
  means the quantified wording must be narrowed or removed.
- Inspect human discussion on the current PR and every related or superseded PR whose work or
  feedback is carried forward. Adopted feedback needs source-backed attribution or a required
  `Missing context:` line when the contributor's email cannot be verified.

## Workflow

### 1. Gather first-party evidence

Inspect at least:

1. the current PR title, body, URL, closing issues, commits, reviews, inline comments,
   complete changed-path list, and every substantive current-head diff hunk
2. linked issues, related or superseded PRs, required companion PRs, compare links,
   failure logs, repro steps, validation runs, and relevant release or API documentation;
   for carried-forward PRs, inspect their issue comments, reviews, and inline comments too
3. commit bodies and adopted review feedback that explain non-obvious final code
4. recent target-repository merge messages for subject, reference, section, bullet, and
   wrapping conventions

Useful commands include:

```bash
gh pr view <number> --json number,title,body,url,commits,files,comments,reviews,closingIssuesReferences
gh api repos/<owner>/<repo>/pulls/<number>/comments --paginate
gh api repos/<owner>/<repo>/issues/<number>/comments --paginate
```

Use the final diff and current head as the strongest evidence for shipped behavior. Use
the PR base or a specific referenced revision for the old side of a before/after claim.
Treat PR prose, commit bodies, review comments, and code comments as leads that still need
verification.

For dependency or submodule pointers, separate the parent PR's intent from the range the
pointer happens to cross. A required companion change or intentionally adopted dependency
behavior can be relevant. Unrelated intervening commits are not parent-PR topics merely
because the new pointer includes them; preserve the range with a compare link instead of
narrating it.

### 2. Build one private evidence ledger

Use these definitions before classifying the diff:

- **Material topic:** an independently searchable causal or design unit with both its own
  why boundary (motivation, failure, root cause, constraint, compatibility consequence, or
  decision) and at least one decisive anchor not derived from another topic.
- **Explicit decision:** a non-trivial correction, exception, safety rule, or design choice
  that shares a material topic's root cause and lacks independent search intent. It remains
  a named sub-point under that topic.
- **Mechanical derivative:** generated output, lock or compiled artifacts, repeated
  documentation, ordinary verification, or iterative fixups that only implement or repeat
  a parent topic.

Walk every changed path and substantive hunk once. Map each coherent change to exactly one
material topic, explicit decision under a named topic, or mechanical derivative under a
named topic. A hunk is substantive when it changes runtime, build, packaging, test, or
workflow behavior; an API or design decision; an emitted artifact; or rationale needed to
maintain one of those. Build, project, packaging, workflow, and configuration changes are
candidate topics just like source code.

For each material topic, record:

- need or user-visible symptom
- root cause or triggering condition
- chosen correction or design
- important tradeoff, compatibility rule, limitation, or rejected alternative
- verification evidence
- decisive anchors and their exact sources: errors, symbols, predicates, fallback order,
  counterexamples, measurements, revisions, or review-found semantic corrections
- related issue, PR, run, compare view, or commit

Then perform one history-only pass over the parent PR's commits, reviews, linked context,
validation notes, and specifically adopted upstream changes. Capture information the final
tree cannot explain by itself: the original failure, a rejected obvious alternative, a
regression that changed the design, a compatibility exception, a review-found correction,
or a measurement that governs how evidence should be read. Do not walk an entire traversed
dependency range. Verify useful historical facts before adding them to the ledger.

Apply these boundary rules:

- Sharing a subsystem, umbrella feature, or final goal does not merge topics with different
  why boundaries or corrections.
- Do not split a topic merely because its implementation crosses files, commits, workflow
  stages, generated outputs, or tests.
- Multiple manifestations of one root cause remain one topic only when they share the same
  correction and compatibility rules.
- A feature's public programming model is a topic when its API, lifetime, interoperability,
  or compatibility design itself meets the material-topic definition; when it does, the
  overview is not a substitute for its section.
- Tests or benchmarks become a topic when they correct misleading prior evidence,
  introduce a distinct validation architecture, or independently establish a performance
  or compatibility decision. Ordinary verification stays with its parent.
- Correctness or equivalence evidence and performance evidence are separate topics when
  each independently justifies safe adoption.
- Review-found correctness or safety changes remain explicit decisions, or become topics
  when they have independent search intent.

Use a counterfactual independence test before collapsing any candidate: if its correction
were reverted while the proposed parent remained, would a distinct failure, constraint, or
maintenance question return? If so, keep it as a material topic. A path is not a derivative
merely because it supports the same feature; it must share the parent's why and correction.
A shared goal or symptom category is not a shared root cause. Do not demote a candidate that
passes the material-topic definition merely to shorten the message. If the returning problem
shares the parent's why and lacks independent search intent, keep it as an explicit decision
rather than promoting it.

After collapsing derivatives, choose the shape:

- **Mechanical change:** subject only or one short explanatory paragraph.
- **One material topic:** focused causal narrative; no forced section heading.
- **Two or more material topics:** overview followed by exactly one named section per
  topic. In SkiaSharp, use the established `~~ Topic ~~` syntax.

Every topic, explicit decision, and marked anchor gets one visible home. A section preserves
its causal chain; naming a file or category is not coverage. Collapsing a derivative removes
its separate heading, not useful evidence it contributes to the parent.

Use a counterfactual anchor test: retain the smallest observed fact that shows why the
chosen design is necessary, why an obvious alternative is unsafe, or what made the evidence
credible. Compress repetition, not the condition, counterexample, scope, or measurement
that carries the proof.

### 3. Draft in the repository's voice

Prefer the repository's established subject form:

- `[Area] Imperative summary (#PR)`
- `Area: imperative summary (#PR)`
- `Imperative summary (#PR)` when no area prefix is established

Use an imperative verb, identify the real subsystem when useful, preserve the established
area spelling, and avoid vague subjects such as `Fix review comments` or `Misc changes`.

Start the body with verified references. Use labels only for the relationship they express:

- `Fixes:` for an issue the PR closes
- `Context:` for related issues, PRs, runs, announcements, documentation, URLs, or exact
  commit SHAs
- `Requires:` for a companion PR or dependency that must land
- `Changes:` for dependency compare views or upstream ranges
- another label only when recent repository history establishes both its spelling and use

Fetch every target. Do not convert a PR URL to an issue URL, call a related PR an issue,
claim a PR is completed without evidence, or attach a correct URL to the wrong relationship.

Preserve the content expected for the kind of change:

- **Bug fix:** symptom, investigation evidence, root cause, and correction
- **Dependency or submodule bump:** repository and revision, compare link, and only the
  upstream changes relevant to this repository
- **Release or platform update:** capability gap or release trigger, compatibility impact,
  project response, and authoritative links
- **Public API:** programming model, ownership or lifetime rules, native dependency, and
  platform limitations

For multiple topics, use this shape:

```text
<imperative subject>

<verified reference lines>

<overview: what changes, why it matters, and the governing design>

~~ <material topic or failure mode 1> ~~

<symptom/need -> cause/constraint -> chosen change -> evidence>

  * <optional explicit decision with cause, consequence, and anchor>

~~ <material topic or failure mode 2> ~~

<symptom/need -> cause/constraint -> chosen change -> evidence>

<trailers>
```

Name sections after the actual problem or decision, not `Changes` or `Implementation`.
Use bullets for explicit decisions inside a topic, not as a substitute for topic sections.
Keep exception snippets, logs, reference lists, bullets, and wrapping in-family for the
target repository.

### 4. Run one claim-proof pass

Check the final draft by claim type:

- **References and relationships:** fetch and read each target, then record its actual entity
  type, title, and exact relationship to the PR. A resolving URL or its appearance in PR
  prose is not proof. Omit a reference whose relationship is unsupported. For a compare URL,
  verify its endpoints; do not walk every commit merely to validate the range.
- **Before/after behavior:** verify both sides in the artifacts that actually changed. If
  code already behaved that way and only guidance or validation changed, attribute the
  change to the guidance or validation.
- **Quantifiers and bounds:** scan for `all`, `every`, `only`, `no`, `always`, `never`,
  `exactly`, counts, minimums, maximums, and inequality symbols. Record the search universe
  and check the complete relevant set, including generated or aggregate surfaces, for an
  outlier. Narrow or remove the qualifier when any counterexample exists.
- **Precision:** verify exception names, predicates, revisions, versions, test counts,
  benchmark values, platform claims, and validation results against their exact sources.
  Prefer durable validation evidence; omit transient pending, queued, or action-required
  status unless it explains a shipped design decision.
- **Causal rationale:** verify reasons copied from PR descriptions, commits, or comments.
  Preserve meaningful non-goals and limitations, but label inference as inference or omit
  uncertain precision.
- **Mixed-truth sentences:** split a verified fact from an unsupported qualifier instead of
  accepting the whole sentence because half is true.

### 5. Build source-backed attribution

For the current PR and any related or superseded PR whose implementation or adopted human
feedback is carried forward, combine:

1. every `.commits[].authors[]` entry, including the primary PR author
2. every case-insensitive `Co-authored-by:` trailer in each commit `messageBody`
3. human reviewers or commenters whose approval or substantive feedback contributed to the
   final change

For each current, related, or superseded PR in scope, fetch issue comments, reviews, and
inline review comments rather than checking only the current PR. Deduplicate
case-insensitively by email. If the same display name used two distinct recorded emails,
retain both.

Accept a `(name, email)` pair only when that exact email appears in a relevant commit author
entry, a co-author trailer, or the non-null `email` field returned by
`gh api users/<login>`. Never infer an address from a name or organization, construct a
GitHub noreply address, or mine unrelated commits. If a contributing human reviewer has no
verified email, omit the trailer and report:

```text
Missing context: verified email for @login
```

A dependency author is not a parent-PR co-author merely because a pointer traverses their
commit. Credit copied or cherry-picked work only when authorship is preserved or an allowed
parent-PR source records it. Exclude routine automation identities, including dependency
updaters and automated review accounts; they are not missing human contributors. Include
coding agents when they authored code or appear in a co-author trailer.

Every eligible candidate in the resulting attribution map must appear exactly once in the
trailers, or in `Missing context:` when a contributing human lacks a verified email. Place
trailers at the end of the message after a blank line, one per line.

## Final quality gate

Before returning the message, confirm:

1. Every substantive changed path maps to a topic, explicit decision, or derivative, and
   `section count == material-topic count` whenever that count is two or more.
2. Every topic explains need or symptom, cause or constraint, chosen change, and evidence;
   every explicit decision and decisive anchor has one visible home.
3. Every precise, relational, causal, quantified, and validation claim has its completed
   proof entry; no proof over a subset is worded as a whole-surface claim.
4. Every eligible attribution candidate, including relevant PR and commit authors, appears
   exactly once as a source-backed trailer or required missing-context item.
5. The message answers what a future maintainer would otherwise have to reopen the PR to
   learn, without file-by-file narration or repeated prose.

Return the polished commit-message content and any required `Missing context:` facts.
Presentation and rendering belong to the host.
