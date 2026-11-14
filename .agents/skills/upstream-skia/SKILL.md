---
name: upstream-skia
description: >-
  Upstream a focused mono/skia fork fix through Google's Skia Gerrit workflow.
  Use for "upstream this fix", "send this Skia patch to Google", preparing a
  Skia CL from a path/commit/diff, or rebasing/updating our existing Gerrit CL.
  Investigate equivalent fixes first; keep preparation, upload, review and
  landing separate. Not for pulling upstream milestones into our fork.
---

# Upstream a focused Skia fix

Accept a path, fork commit or focused diff plus the intended rationale. Start
small; do not require a whole-fork audit. Default to investigation and local
candidate preparation. Read the linked official guidance before acting; verify
installed command options with `git cl help <command>`, not remembered flags.

## 1. Establish the delta and whether upstream needs it

- Record the exact fork SHA (normally SkiaSharp's `externals/skia` gitlink),
  matching upstream milestone SHA and current canonical `main` SHA. Establish
  the matching baseline from merge parents/history, not the milestone number
  alone. Stop if provenance or the baseline is uncertain.
- In a repository containing both histories, inspect
  `git diff <upstream-baseline-sha> <fork-sha> -- <paths>` and
  `git log -p <upstream-baseline-sha>..<fork-sha> -- <paths>`.
  This is a **two-tree diff**, not `A...B` or GitHub compare's merge-base view.
  Trace relevant headers, callers and originating commits; explain why the
  fork needed it and whether it is still missing on current upstream `main`.
- Check upstream's supported compiler/platform/API contract and our actual
  configuration. Distinguish an upstream defect from unsupported toolchains,
  incorrect GN flags or a downstream-only requirement. Preserve supported
  legacy/default paths and current functionality; smaller code alone is not a
  reason to remove compatibility.
- Search [Skia issues](https://issues.skia.org/) and
  [Gerrit](https://skia-review.googlesource.com/q/project:skia) by symbol,
  symptom, file and rationale. Search all statuses, including open/WIP,
  abandoned and merged; inspect actual patches and related bugs, not snippets.
  Public REST `/changes/?q=...` uses [Gerrit query syntax][search] and an XSSI
  prefix before JSON; paginate results. Inaccessible private changes/bugs are
  unknown, not inspected. Verify primary-source URLs before citing.
- If an equivalent fix exists, report its current status and validate it
  against the actual affected consumers instead of uploading a duplicate.
  For an unlanded fix, report what remains; do not silently adopt it into the
  fork. Study others' CLs as reference, never replace their Change-Id or upload
  onto their review. Propose discussion for new features or disputed scope.

## 2. Prepare only the upstream patch

Use an isolated, clean clone of `https://skia.googlesource.com/skia.git`, not
the GitHub mirror or our patched submodule. Keep it inside this session's
workspace; never reuse another session's checkout. Inspect existing tools first;
obtain approval before cloning/installing missing tooling or SDKs. Follow
[Skia setup][setup] for `depot_tools` on PATH and dependency provisioning.

```sh
# In the canonical clone, after checking clean status and origin's URL:
git fetch origin
git checkout -b upstream-focused-fix --track origin/main
```

Port only the necessary hunks onto that fresh `main`. A path is a starting
point, not a license to omit its required declarations/callers/tests. Do not
copy whole fork files, mono C API, DEPS pins or unrelated carry patches.
Use local `git diff`/`git apply --check` to inspect a candidate before applying.
When using someone else's patch as reference, retain provenance/attribution
but start our independent CL on a fresh branch with no borrowed CL association.

Confirm [CLA coverage and AUTHORS][contrib], the user's Skia Gerrit account and
registered commit email. Use repository-local Git identity settings; do not
change global identity. Follow the [current Google Git authentication guide][auth]
for the installed tools/account and Skia host; `git cl creds-check` diagnoses
credentials. Stop for user action on missing CLA/auth/permissions; never print
credentials or assume a previous session's access applies.

Add the smallest meaningful regression proof, preferably a failing-before,
passing-after Skia test; use a GM for rendering where appropriate. Follow
[Skia testing][tests] and the checkout's GN/Bazel source lists. Test affected
configuration branches, including old/default and new paths when applicable.
Record exact commands, tool versions, results and limits: syntax/compile,
link, runtime and CI are different evidence. Host checks do not prove a target
SDK/device; C++ harnesses do not prove .NET or CanvasKit. Missing tools/tests
remain explicit gaps, not skips or successful fallbacks.

```sh
git cl format
git diff --check
# Stage only the intended files and commit with an honest description.
git add <specific-files>
git commit
git cl presubmit
git diff origin/main HEAD --stat
git log origin/main..HEAD
```

Use a short imperative subject, blank line, then why, old/new behavior,
compatibility and exact test evidence/limitations; wrap prose around 72 columns.
Include a verified bug reference when applicable, not an invented issue.
For a **new CL**, remove inherited Change-Id/Reviewed-on/review-vote footers and
stale branch CL metadata. The [Gerrit commit-msg hook][identity] can generate a
fresh Change-Id; the normal `git cl` squash-upload path can also generate one.
Do not hand-invent an ID or reuse a reference CL's ID. Inspect the final upload
description/association and resulting Change-Id.

## 3. Upload or revise only with explicit authorization

Before any network write, confirm the exact intent and target: new upload,
patchset update to **our** CL, comment, review request or tryjob/CQ run.
Preparing a patch does not authorize any of these. Do not post bugs/comments,
upload, add reviewers, send mail or trigger jobs without the corresponding
explicit user authorization. Explain blockers and trade-offs before asking.

- **New upload:** recheck the complete upstream-relative diff, branch tracking,
  account, fresh identity and passing checks, then use `git cl upload`.
  Verify the returned URL, project `skia`, target `main`, Change-Id and WIP
  state. Do not assume a `--wip` flag exists. WIP is still a public upload.
- **Our existing CL:** first read live owner, status, latest patchset, project,
  branch and Change-Id. Verify ownership against the authenticated account and
  explicit user authorization. Stop on merged/abandoned changes or concurrent
  patchsets rather than overwriting them. Recover the latest patchset into an
  isolated branch using the CL's Gerrit download instructions if necessary;
  verify `git cl issue` identifies that exact CL. Preserve its Change-Id,
  attribution and Gerrit identity. Do not generate a new CL accidentally.
  Fetch `origin`, then `git rebase origin/main`; resolve conflicts deliberately
  or stop for uncertain behavior. Recheck the diff and rerun affected tests,
  formatting and presubmit. Amend only when explicitly authorized to amend;
  alternatively commit the update locally and let normal `git cl upload`
  squash it into the same review. With patchset-upload authorization, upload
  again and verify the same CL URL/Change-Id and the new patchset number.
  Practical notes: recover a patchset with
  `git fetch origin refs/changes/<last-2-digits>/<CL>/<PS>` and branch from
  `FETCH_HEAD`; run depot_tools from a clean clone with
  `DEPOT_TOOLS_UPDATE=0`; set repo-local `user.email` to the Gerrit account
  and run `git cl creds-check` (`git credential-luci login` if prompted).
  If upstream deleted or renamed a file the CL touches (`DU` conflict), read
  the removal commit, drop the stale hunk with `git rm`, and port its intent
  to the successor file and tests; never resurrect the file. Fix any commit
  message text that mentions the removed code, keeping the Change-Id.
  `git cl format` needs a Chromium `buildtools` checkout (or
  `CHROMIUM_BUILDTOOLS_PATH`); if absent, record formatting as a gap.
- **Review/testing:** use blame/OWNERS and Skia's suggested reviewers to propose
  an appropriate committer. Request review or send mail only when authorized.
  Select current jobs from `bin/try --list`; `git cl try` requires permission,
  so ask a Skia committer if unavailable. `git cl upload --dry-run` is **not**
  a local preview: it uploads and requests a CQ dry run. Avoid CQ, auto-submit,
  send-mail, dependency-upload and presubmit-bypass flags as shortcuts.
- **Landing:** report approvals, checks and live submit requirements for a human
  handoff. Never run `git cl land`, enable auto-submit, submit/merge, or push
  directly to upstream `main`. After human landing, separately validate
  consuming the fix and removing only the superseded fork delta.

Return a concise decision (candidate, existing fix, downstream-only or blocked),
exact refs/provenance, local patch location, validation evidence/gaps and the
next permission boundary. Include the CL URL only if one actually exists.

## Official references

[contrib]: https://skia.org/docs/dev/contrib/
[setup]: https://skia.org/docs/user/download/
[tests]: https://skia.org/docs/dev/testing/tests/
[auth]: https://chromium.googlesource.com/chromium/src/+/main/docs/gerrit_guide.md
[identity]: https://gerrit-review.googlesource.com/Documentation/user-changeid.html
[search]: https://gerrit-review.googlesource.com/Documentation/user-search.html

- [Skia submission, review, tryjobs and CQ](https://skia.org/docs/dev/contrib/submit/)
- [Skia build configuration and supported compilers](https://skia.org/docs/user/build/)
- [Skia coding style](https://skia.org/docs/dev/contrib/style/)
- [depot_tools git-cl commands](https://chromium.googlesource.com/chromium/tools/depot_tools/+/main/man/html/git-cl.html)
- [Current git-cl implementation/options](https://chromium.googlesource.com/chromium/tools/depot_tools/+/main/git_cl.py)
