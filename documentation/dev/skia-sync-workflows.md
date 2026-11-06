# Skia sync review

Comment exactly `/skia-sync-review` on a `mono/SkiaSharp` pull request to run
an advisory review. Its **Required skia PR** section must include one canonical
`https://github.com/mono/skia/pull/N` link. gh-aw restricts comment activation
to repository write/maintain/admin roles. Alternatively, manually dispatch
**Review - Skia Sync** with the parent `skiasharp_pr` number; the optional
`staged` input defaults to true (preview only), and non-main dispatches stay
staged. gh-aw cannot declare this input `required: true` alongside a slash
command, so the pre-agent step checks it at runtime.

The single workflow uses the existing
[review-skia-update skill](../../.agents/skills/review-skia-update/SKILL.md).
It runs the skill's mechanical checks once, inside a tokenless SDK container,
using public clones at the recorded PR heads. gh-aw installs the trusted skill
and its scripts; the agent job needs no repository checkout. Neither the
dependency sync nor generator runs on the credentialed host. The agent then
reviews the raw diffs, validates the schema-v1 report, and persists JSON and
HTML in the normal agent artifact's
`ai-review/` folder. The safe-output comment names both exact reviewed heads
and links the Actions run/artifact. Re-reviews hide older comments.

Reports are advisory, not approvals or machine-enforced landing evidence. This
workflow cannot merge, approve, repin, or create a pull request. A failed or
incomplete mechanical check stops before the agent reports findings; a genuine
binding mismatch is instead reported for human review. Successful main runs
can be copied to `aw-data` by the existing `persist-aw-data` workflow.
